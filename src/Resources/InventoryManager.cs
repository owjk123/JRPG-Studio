// InventoryManager.cs - 背包管理器
// 管理玩家的物品背包

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JRPG.Resources
{
    /// <summary>
    /// 背包管理器单例类
    /// </summary>
    public class InventoryManager
    {
        #region 单例
        
        private static InventoryManager _instance;
        public static InventoryManager Instance => _instance ??= new InventoryManager();
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 物品变化事件
        /// </summary>
        public event Action<string, int, int> OnItemChanged;
        
        /// <summary>
        /// 物品使用事件
        /// </summary>
        public event Action<string, int> OnItemUsed;
        
        /// <summary>
        /// 背包满事件
        /// </summary>
        public event Action OnInventoryFull;
        
        /// <summary>
        /// 物品获得事件
        /// </summary>
        public event Action<string, int, ItemSource> OnItemAcquired;
        
        #endregion
        
        #region 私有变量
        
        /// <summary>
        /// 背包数据（key: itemId, value: ItemInstance）
        /// </summary>
        private Dictionary<string, ItemInstance> _inventory = new Dictionary<string, ItemInstance>();
        
        /// <summary>
        /// 背包容量
        /// </summary>
        private int _capacity = 500;
        
        /// <summary>
        /// 物品配置缓存
        /// </summary>
        private Dictionary<string, ItemDataScriptable> _itemConfigs = new Dictionary<string, ItemDataScriptable>();
        
        #endregion
        
        #region 物品来源
        
        /// <summary>
        /// 物品来源
        /// </summary>
        public enum ItemSource
        {
            Battle,
            Shop,
            Gacha,
            Quest,
            Event,
            Gift,
            Other
        }
        
        #endregion
        
        #region 构造函数
        
        private InventoryManager()
        {
            LoadItemConfigs();
        }
        
        /// <summary>
        /// 加载物品配置
        /// </summary>
        private void LoadItemConfigs()
        {
            var configs = Resources.LoadAll<ItemDataScriptable>("Data/Items/");
            foreach (var config in configs)
            {
                _itemConfigs[config.itemId] = config;
            }
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取物品数量
        /// </summary>
        public int GetItemCount(string itemId)
        {
            if (_inventory.TryGetValue(itemId, out var item))
                return item.count;
            return 0;
        }
        
        /// <summary>
        /// 获取物品实例
        /// </summary>
        public ItemInstance GetItem(string itemId)
        {
            return _inventory.TryGetValue(itemId, out var item) ? item : null;
        }
        
        /// <summary>
        /// 检查物品是否存在
        /// </summary>
        public bool HasItem(string itemId)
        {
            return _inventory.ContainsKey(itemId) && _inventory[itemId].count > 0;
        }
        
        /// <summary>
        /// 检查物品是否足够
        /// </summary>
        public bool HasEnoughItem(string itemId, int count)
        {
            return GetItemCount(itemId) >= count;
        }
        
        /// <summary>
        /// 添加物品
        /// </summary>
        public bool AddItem(string itemId, int count = 1, ItemSource source = ItemSource.Other)
        {
            // 检查背包容量
            if (GetUniqueItemCount() >= _capacity && !HasItem(itemId))
            {
                OnInventoryFull?.Invoke();
                Debug.LogWarning("背包已满，无法添加物品");
                return false;
            }
            
            // 获取物品配置
            var config = GetItemConfig(itemId);
            int maxStack = config?.maxStack ?? 999;
            
            if (_inventory.TryGetValue(itemId, out var item))
            {
                // 叠加
                int totalCount = item.count + count;
                if (totalCount > maxStack)
                {
                    // 超出部分无法添加
                    item.count = maxStack;
                    OnItemAcquired?.Invoke(itemId, maxStack - item.count, source);
                    return false;
                }
                
                item.count = totalCount;
                OnItemAcquired?.Invoke(itemId, count, source);
            }
            else
            {
                // 新建实例
                var newItem = new ItemInstance(itemId, Mathf.Min(count, maxStack));
                _inventory[itemId] = newItem;
                OnItemAcquired?.Invoke(itemId, count, source);
            }
            
            OnItemChanged?.Invoke(itemId, GetItemCount(itemId) - count, GetItemCount(itemId));
            return true;
        }
        
        /// <summary>
        /// 移除物品
        /// </summary>
        public bool RemoveItem(string itemId, int count = 1)
        {
            if (!HasEnoughItem(itemId, count))
            {
                Debug.LogWarning($"物品 {itemId} 数量不足");
                return false;
            }
            
            var item = _inventory[itemId];
            int oldCount = item.count;
            item.count -= count;
            
            // 数量为0时移除
            if (item.count <= 0)
            {
                _inventory.Remove(itemId);
            }
            
            OnItemChanged?.Invoke(itemId, oldCount, item.count);
            return true;
        }
        
        /// <summary>
        /// 使用物品
        /// </summary>
        public bool UseItem(string itemId, string targetCharacterId = null)
        {
            var config = GetItemConfig(itemId);
            if (config == null || !config.canUse)
            {
                Debug.LogWarning($"物品 {itemId} 不可使用");
                return false;
            }
            
            if (!HasItem(itemId))
            {
                Debug.LogWarning($"没有物品 {itemId}");
                return false;
            }
            
            // 执行物品效果
            foreach (var effect in config.effects)
            {
                ExecuteItemEffect(effect, targetCharacterId);
            }
            
            // 消耗物品
            RemoveItem(itemId, 1);
            OnItemUsed?.Invoke(itemId, 1);
            
            return true;
        }
        
        /// <summary>
        /// 批量使用物品
        /// </summary>
        public bool UseItemBatch(string itemId, int count, string targetCharacterId = null)
        {
            if (!HasEnoughItem(itemId, count))
            {
                count = GetItemCount(itemId);
            }
            
            for (int i = 0; i < count; i++)
            {
                if (!UseItem(itemId, targetCharacterId))
                    break;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取物品配置
        /// </summary>
        public ItemDataScriptable GetItemConfig(string itemId)
        {
            if (!_itemConfigs.TryGetValue(itemId, out var config))
            {
                config = Resources.Load<ItemDataScriptable>($"Data/Items/{itemId}");
                if (config != null)
                    _itemConfigs[itemId] = config;
            }
            return config;
        }
        
        /// <summary>
        /// 获取背包中所有物品
        /// </summary>
        public List<ItemInstance> GetAllItems()
        {
            return new List<ItemInstance>(_inventory.Values);
        }
        
        /// <summary>
        /// 获取背包中物品数量（去重）
        /// </summary>
        public int GetUniqueItemCount()
        {
            return _inventory.Count;
        }
        
        /// <summary>
        /// 获取背包容量
        /// </summary>
        public int GetCapacity()
        {
            return _capacity;
        }
        
        /// <summary>
        /// 扩展背包容量
        /// </summary>
        public void ExpandCapacity(int additional)
        {
            _capacity += additional;
            Debug.Log($"背包容量扩展至 {_capacity}");
        }
        
        /// <summary>
        /// 按类型获取物品
        /// </summary>
        public List<ItemInstance> GetItemsByType(ItemType type)
        {
            var result = new List<ItemInstance>();
            
            foreach (var item in _inventory.Values)
            {
                var config = GetItemConfig(item.itemId);
                if (config != null && config.itemType == type)
                {
                    result.Add(item);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 整理背包（合并相同物品）
        /// </summary>
        public void SortInventory()
        {
            // 简化处理，实际可能需要更复杂的排序逻辑
            Debug.Log("背包已整理");
        }
        
        /// <summary>
        /// 出售物品
        /// </summary>
        public bool SellItem(string itemId, int count = 1)
        {
            var config = GetItemConfig(itemId);
            if (config == null || !config.canSell)
            {
                Debug.LogWarning($"物品 {itemId} 不可出售");
                return false;
            }
            
            if (!HasEnoughItem(itemId, count))
            {
                Debug.LogWarning($"物品 {itemId} 数量不足");
                return false;
            }
            
            int totalPrice = config.sellPrice * count;
            CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, totalPrice);
            RemoveItem(itemId, count);
            
            Debug.Log($"出售 {count} 个 {itemId}，获得 {totalPrice} 金币");
            return true;
        }
        
        #endregion
        
        #region 物品效果执行
        
        /// <summary>
        /// 执行物品效果
        /// </summary>
        private void ExecuteItemEffect(ItemEffect effect, string targetCharacterId)
        {
            switch (effect.effectType)
            {
                case ItemEffectType.RecoverStamina:
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Stamina, effect.value);
                    break;
                    
                case ItemEffectType.RecoverStaminaPercent:
                    int maxStamina = (int)CurrencyManager.Instance.GetMaxValue(CurrencyType.Stamina);
                    int recover = Mathf.RoundToInt(maxStamina * (effect.value / 100f));
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Stamina, recover);
                    break;
                    
                case ItemEffectType.GainExp:
                    // 需要指定目标角色
                    break;
                    
                case ItemEffectType.GainGold:
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, effect.value);
                    break;
                    
                case ItemEffectType.GainAffection:
                    // 需要好感度系统支持
                    break;
            }
        }
        
        #endregion
        
        #region 数据保存/加载
        
        /// <summary>
        /// 保存背包数据
        /// </summary>
        public InventorySaveData SaveToData()
        {
            return new InventorySaveData
            {
                inventory = new List<ItemInstance>(_inventory.Values),
                capacity = _capacity
            };
        }
        
        /// <summary>
        /// 加载背包数据
        /// </summary>
        public void LoadFromData(InventorySaveData data)
        {
            if (data == null)
                return;
            
            _inventory.Clear();
            foreach (var item in data.inventory)
            {
                if (item.count > 0)
                    _inventory[item.itemId] = item;
            }
            _capacity = data.capacity;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 背包保存数据
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        public List<ItemInstance> inventory;
        public int capacity;
    }
}
