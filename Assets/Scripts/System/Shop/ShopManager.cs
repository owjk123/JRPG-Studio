using UnityEngine;
using System.Collections.Generic;

namespace JRPG.Shop
{
    /// <summary>
    /// 商品数据
    /// </summary>
    [CreateAssetMenu(fileName = "ShopItem", menuName = "JRPG/Shop/ShopItem")]
    public class ShopItemData : ScriptableObject
    {
        [Header("商品信息")]
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        
        [Header("商品类型")]
        public ShopItemType itemType;
        public string rewardId;         // 奖励ID（角色ID、道具ID等）
        public int rewardAmount = 1;    // 奖励数量
        
        [Header("价格")]
        public CurrencyType currencyType;
        public int price;
        public int originalPrice;       // 原价（用于显示折扣）
        
        [Header("限购")]
        public bool hasLimit = false;
        public int purchaseLimit = 1;
        public LimitResetType limitResetType = LimitResetType.Never;
        
        [Header("时间限制")]
        public bool hasTimeLimit = false;
        public System.DateTime startTime;
        public System.DateTime endTime;
        
        [Header("其他")]
        public bool isHot = false;          // 热卖标记
        public bool isNew = false;          // 新品标记
        public int sortPriority = 0;        // 排序优先级
    }
    
    /// <summary>
    /// 商店类型
    /// </summary>
    public enum ShopType
    {
        Daily,          // 每日商店
        Weekly,         // 每周商店
        Monthly,        // 每月商店
        Premium,        // 高级商店
        Event,          // 活动商店
        Arena,          // 竞技场商店
        Guild,          // 公会商店
        Gacha           // 抽卡商店
    }
    
    /// <summary>
    /// 商品类型
    /// </summary>
    public enum ShopItemType
    {
        Character,      // 角色
        CharacterFragment, // 角色碎片
        Equipment,      // 装备
        Item,           // 道具
        Resource,       // 资源（金币、钻石等）
        Package,        // 礼包
        GachaTicket     // 抽卡券
    }
    
    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyType
    {
        Gold,           // 金币
        Gems,           // 钻石
        Stardust,       // 星辉
        ArenaCoin,      // 竞技场币
        GuildCoin,      // 公会币
        EventCoin       // 活动币
    }
    
    /// <summary>
    /// 限购重置类型
    /// </summary>
    public enum LimitResetType
    {
        Never,          // 永不重置
        Daily,          // 每日重置
        Weekly,         // 每周重置
        Monthly         // 每月重置
    }
    
    /// <summary>
    /// 商店管理器
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("商店配置")]
        [SerializeField] private ShopConfig[] shopConfigs;
        
        private Dictionary<ShopType, List<ShopItemData>> shopItems = new Dictionary<ShopType, List<ShopItemData>>();
        private Dictionary<string, int> purchaseRecords = new Dictionary<string, int>();
        
        /// <summary>
        /// 获取商店商品列表
        /// </summary>
        public List<ShopItemData> GetShopItems(ShopType shopType)
        {
            if (shopItems.TryGetValue(shopType, out var items))
            {
                return items;
            }
            return new List<ShopItemData>();
        }
        
        /// <summary>
        /// 购买商品
        /// </summary>
        public bool PurchaseItem(ShopType shopType, string itemId, int amount = 1)
        {
            var items = GetShopItems(shopType);
            var item = items.Find(i => i.itemId == itemId);
            
            if (item == null)
            {
                Debug.LogWarning($"[ShopManager] 商品不存在: {itemId}");
                return false;
            }
            
            // 检查限购
            if (item.hasLimit)
            {
                int purchased = GetPurchasedCount(itemId);
                if (purchased + amount > item.purchaseLimit)
                {
                    Debug.LogWarning($"[ShopManager] 超过购买限制: {itemId}");
                    return false;
                }
            }
            
            // 检查时间限制
            if (item.hasTimeLimit)
            {
                var now = System.DateTime.Now;
                if (now < item.startTime || now > item.endTime)
                {
                    Debug.LogWarning($"[ShopManager] 商品不在销售时间内: {itemId}");
                    return false;
                }
            }
            
            // 检查货币
            int totalCost = item.price * amount;
            if (!CheckCurrency(item.currencyType, totalCost))
            {
                Debug.LogWarning($"[ShopManager] 货币不足: {item.currencyType}");
                return false;
            }
            
            // 扣除货币
            DeductCurrency(item.currencyType, totalCost);
            
            // 发放奖励
            GrantReward(item);
            
            // 记录购买
            RecordPurchase(itemId, amount);
            
            Debug.Log($"[ShopManager] 购买成功: {item.itemName} x{amount}");
            return true;
        }
        
        /// <summary>
        /// 获取已购买次数
        /// </summary>
        public int GetPurchasedCount(string itemId)
        {
            if (purchaseRecords.TryGetValue(itemId, out int count))
            {
                return count;
            }
            return 0;
        }
        
        /// <summary>
        /// 检查货币是否足够
        /// </summary>
        private bool CheckCurrency(CurrencyType type, int amount)
        {
            // 实际实现需要调用CurrencyManager
            return true;
        }
        
        /// <summary>
        /// 扣除货币
        /// </summary>
        private void DeductCurrency(CurrencyType type, int amount)
        {
            // 实际实现需要调用CurrencyManager
        }
        
        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GrantReward(ShopItemData item)
        {
            // 实际实现需要根据itemType发放对应奖励
        }
        
        /// <summary>
        /// 记录购买
        /// </summary>
        private void RecordPurchase(string itemId, int amount)
        {
            if (!purchaseRecords.ContainsKey(itemId))
            {
                purchaseRecords[itemId] = 0;
            }
            purchaseRecords[itemId] += amount;
        }
        
        /// <summary>
        /// 重置限购记录
        /// </summary>
        public void ResetPurchaseRecords(LimitResetType resetType)
        {
            // 根据重置类型清理购买记录
            var keysToRemove = new List<string>();
            
            foreach (var kvp in purchaseRecords)
            {
                // 这里需要根据商品的limitResetType判断是否需要重置
                // 简化实现：清空所有记录
            }
            
            Debug.Log($"[ShopManager] 已重置限购记录: {resetType}");
        }
        
        #region 单例
        
        private static ShopManager _instance;
        public static ShopManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ShopManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[ShopManager]");
                        _instance = go.AddComponent<ShopManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 商店配置
    /// </summary>
    [System.Serializable]
    public class ShopConfig
    {
        public ShopType shopType;
        public ShopItemData[] items;
        public bool autoRefresh = true;
        public int refreshHour = 0;  // 刷新时间（小时）
    }
}
