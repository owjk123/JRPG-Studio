// EquipmentManager.cs - 装备管理器
// 管理所有装备的创建、装备/卸下、强化等操作

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Character;

namespace JRPG.Equipment
{
    /// <summary>
    /// 装备管理器单例类
    /// </summary>
    public class EquipmentManager
    {
        #region 单例
        
        private static EquipmentManager _instance;
        public static EquipmentManager Instance => _instance ??= new EquipmentManager();
        
        #endregion
        
        #region 数据存储
        
        /// <summary>
        /// 所有装备实例（key: instanceId）
        /// </summary>
        private Dictionary<string, EquipmentInstance> _allEquipment = new Dictionary<string, EquipmentInstance>();
        
        /// <summary>
        /// 按角色ID分组的装备
        /// </summary>
        private Dictionary<string, List<string>> _characterEquipment = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// 未装备的背包物品
        /// </summary>
        private List<string> _inventoryEquipment = new List<string>();
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 装备变更事件
        /// </summary>
        public event Action<string, EquipmentSlot, string> OnEquipmentChanged;
        
        /// <summary>
        /// 强化成功事件
        /// </summary>
        public event Action<string, int, int> OnEnhanceSuccess;
        
        /// <summary>
        /// 强化失败事件
        /// </summary>
        public event Action<string, int> OnEnhanceFailed;
        
        #endregion
        
        #region 构造函数
        
        private EquipmentManager()
        {
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 注册装备实例
        /// </summary>
        public void RegisterEquipment(EquipmentInstance instance)
        {
            if (instance == null || string.IsNullOrEmpty(instance.instanceId))
                return;
            
            _allEquipment[instance.instanceId] = instance;
            _inventoryEquipment.Add(instance.instanceId);
            
            Debug.Log($"装备已注册: {instance.instanceId}");
        }
        
        /// <summary>
        /// 获取装备实例
        /// </summary>
        public EquipmentInstance GetEquipment(string instanceId)
        {
            return _allEquipment.TryGetValue(instanceId, out var equipment) ? equipment : null;
        }
        
        /// <summary>
        /// 获取装备数据
        /// </summary>
        public EquipmentData GetEquipmentData(string instanceId)
        {
            var instance = GetEquipment(instanceId);
            return instance?.GetData();
        }
        
        /// <summary>
        /// 为角色装备物品
        /// </summary>
        public bool EquipItem(string characterId, string equipmentInstanceId, EquipmentSlot slot)
        {
            var equipment = GetEquipment(equipmentInstanceId);
            if (equipment == null)
            {
                Debug.LogWarning($"装备实例不存在: {equipmentInstanceId}");
                return false;
            }
            
            var data = equipment.GetData();
            if (data == null)
            {
                Debug.LogWarning($"装备数据不存在: {equipment.equipmentId}");
                return false;
            }
            
            // 检查槽位是否匹配
            if (!EquipmentSlotConfig.CanEquipInSlot(data.equipmentType, slot))
            {
                Debug.LogWarning($"装备类型{data.equipmentType}不能装备到槽位{slot}");
                return false;
            }
            
            // 卸下原有装备
            string oldEquipmentId = null;
            var character = PartyManager.Instance?.GetCharacter(characterId);
            if (character != null)
            {
                oldEquipmentId = character.GetEquippedItemId(slot);
                if (!string.IsNullOrEmpty(oldEquipmentId))
                {
                    UnequipItem(characterId, slot);
                }
            }
            
            // 装备新物品
            equipment.ownerCharacterId = characterId;
            _inventoryEquipment.Remove(equipmentInstanceId);
            
            if (!_characterEquipment.ContainsKey(characterId))
                _characterEquipment[characterId] = new List<string>();
            
            _characterEquipment[characterId].Add(equipmentInstanceId);
            
            // 更新角色装备槽位
            if (character != null)
            {
                character.EquipItem(equipmentInstanceId, slot);
            }
            
            // 触发事件
            OnEquipmentChanged?.Invoke(characterId, slot, equipmentInstanceId);
            
            Debug.Log($"角色 {characterId} 装备了 {data.equipmentName}");
            return true;
        }
        
        /// <summary>
        /// 卸下角色装备
        /// </summary>
        public bool UnequipItem(string characterId, EquipmentSlot slot)
        {
            var character = PartyManager.Instance?.GetCharacter(characterId);
            if (character == null)
                return false;
            
            string equipmentId = character.GetEquippedItemId(slot);
            if (string.IsNullOrEmpty(equipmentId))
                return false;
            
            var equipment = GetEquipment(equipmentId);
            if (equipment == null)
                return false;
            
            // 卸下装备
            equipment.ownerCharacterId = null;
            character.UnequipItem(slot);
            
            _characterEquipment[characterId].Remove(equipmentId);
            _inventoryEquipment.Add(equipmentId);
            
            // 触发事件
            OnEquipmentChanged?.Invoke(characterId, slot, null);
            
            var data = equipment.GetData();
            Debug.Log($"角色 {characterId} 卸下了 {data?.equipmentName}");
            return true;
        }
        
        /// <summary>
        /// 强化装备
        /// </summary>
        public bool EnhanceEquipment(string equipmentInstanceId)
        {
            var equipment = GetEquipment(equipmentInstanceId);
            if (equipment == null)
                return false;
            
            var data = equipment.GetData();
            if (data == null)
                return false;
            
            // 检查是否满级
            if (equipment.enhanceLevel >= data.maxEnhanceLevel)
            {
                Debug.Log("装备已达最大强化等级");
                return false;
            }
            
            // 检查金币
            int goldCost = data.GetEnhanceGoldCost();
            if (!CurrencyManager.Instance.HasEnoughCurrency(CurrencyType.Gold, goldCost))
            {
                Debug.Log("金币不足");
                return false;
            }
            
            // 检查材料
            string materialId = data.GetEnhanceMaterialId();
            int materialCount = data.GetEnhanceMaterialCount();
            if (!ResourcesManager.Instance.HasEnoughItem(materialId, materialCount))
            {
                Debug.Log("强化材料不足");
                return false;
            }
            
            // 消耗资源
            CurrencyManager.Instance.ConsumeCurrency(CurrencyType.Gold, goldCost);
            ResourcesManager.Instance.RemoveItem(materialId, materialCount);
            
            // 计算成功率
            float successRate = data.GetEnhanceSuccessRate();
            float roll = UnityEngine.Random.value;
            
            if (roll <= successRate)
            {
                // 强化成功
                equipment.enhanceLevel++;
                OnEnhanceSuccess?.Invoke(equipmentInstanceId, equipment.enhanceLevel - 1, equipment.enhanceLevel);
                Debug.Log($"强化成功: {data.equipmentName} +{equipment.enhanceLevel}");
                return true;
            }
            else
            {
                // 强化失败（降级）
                int newLevel = Mathf.Max(0, equipment.enhanceLevel - 1);
                equipment.enhanceLevel = newLevel;
                OnEnhanceFailed?.Invoke(equipmentInstanceId, newLevel);
                Debug.Log($"强化失败: {data.equipmentName} 降级到 +{newLevel}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取角色的已装备物品列表
        /// </summary>
        public List<EquipmentInstance> GetCharacterEquipment(string characterId)
        {
            var result = new List<EquipmentInstance>();
            
            if (_characterEquipment.TryGetValue(characterId, out var equipmentIds))
            {
                foreach (var id in equipmentIds)
                {
                    var equipment = GetEquipment(id);
                    if (equipment != null)
                        result.Add(equipment);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取背包中的装备列表
        /// </summary>
        public List<EquipmentInstance> GetInventoryEquipment()
        {
            var result = new List<EquipmentInstance>();
            
            foreach (var id in _inventoryEquipment)
            {
                var equipment = GetEquipment(id);
                if (equipment != null)
                    result.Add(equipment);
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取角色特定槽位的装备
        /// </summary>
        public EquipmentInstance GetEquippedItem(string characterId, EquipmentSlot slot)
        {
            var character = PartyManager.Instance?.GetCharacter(characterId);
            if (character == null)
                return null;
            
            string equipmentId = character.GetEquippedItemId(slot);
            return string.IsNullOrEmpty(equipmentId) ? null : GetEquipment(equipmentId);
        }
        
        /// <summary>
        /// 自动穿戴最优装备
        /// </summary>
        public void AutoEquipBest(string characterId)
        {
            var character = PartyManager.Instance?.GetCharacter(characterId);
            if (character == null)
                return;
            
            var stats = CharacterStatsCalculator.Instance.CalculateTotalStats(character);
            
            // 遍历所有槽位
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                var bestEquipment = FindBestEquipmentForSlot(characterId, slot, stats);
                if (bestEquipment != null)
                {
                    EquipItem(characterId, bestEquipment.instanceId, slot);
                }
            }
        }
        
        /// <summary>
        /// 查找特定槽位的最佳装备
        /// </summary>
        private EquipmentInstance FindBestEquipmentForSlot(string characterId, EquipmentSlot slot, CharacterStats characterStats)
        {
            EquipmentInstance best = null;
            int bestScore = int.MinValue;
            
            foreach (var equipmentId in _inventoryEquipment)
            {
                var equipment = GetEquipment(equipmentId);
                if (equipment == null)
                    continue;
                
                var data = equipment.GetData();
                if (data == null || data.slot != slot)
                    continue;
                
                // 计算装备评分
                int score = CalculateEquipmentScore(equipment, characterStats);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    best = equipment;
                }
            }
            
            return best;
        }
        
        /// <summary>
        /// 计算装备评分
        /// </summary>
        private int CalculateEquipmentScore(EquipmentInstance equipment, CharacterStats characterStats)
        {
            var data = equipment.GetData();
            if (data == null)
                return 0;
            
            var stats = equipment.GetStats();
            
            // 根据角色职业和定位计算权重
            int score = 0;
            score += stats.Atk * 2;        // 攻击权重
            score += stats.MaxHp;         // HP权重
            score += stats.Def;            // 防御权重
            score += stats.Speed * 3;     // 速度权重
            score += (int)(stats.CritRate * 100) * 5;  // 暴击权重
            score += (int)(stats.CritDamage * 100);    // 暴伤权重
            
            return score;
        }
        
        #endregion
        
        #region 数据加载/保存
        
        /// <summary>
        /// 从存档加载数据
        /// </summary>
        public void LoadFromSave(List<EquipmentInstanceData> equipmentList)
        {
            _allEquipment.Clear();
            _characterEquipment.Clear();
            _inventoryEquipment.Clear();
            
            foreach (var data in equipmentList)
            {
                var instance = EquipmentInstance.FromSaveData(data);
                _allEquipment[instance.instanceId] = instance;
                
                if (string.IsNullOrEmpty(instance.ownerCharacterId))
                {
                    _inventoryEquipment.Add(instance.instanceId);
                }
                else
                {
                    if (!_characterEquipment.ContainsKey(instance.ownerCharacterId))
                        _characterEquipment[instance.ownerCharacterId] = new List<string>();
                    _characterEquipment[instance.ownerCharacterId].Add(instance.instanceId);
                }
            }
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public List<EquipmentInstanceData> SaveToData()
        {
            var result = new List<EquipmentInstanceData>();
            
            foreach (var equipment in _allEquipment.Values)
            {
                result.Add(equipment.ToSaveData());
            }
            
            return result;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 队伍管理器（引用角色实例）
    /// </summary>
    public static class PartyManager
    {
        /// <summary>
        /// 获取角色实例
        /// </summary>
        public static CharacterInstance GetCharacter(string characterId)
        {
            // 实际实现应从角色管理器获取
            return null;
        }
    }
}
