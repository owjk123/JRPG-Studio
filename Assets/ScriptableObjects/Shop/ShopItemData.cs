// ShopItemData.cs - 商店商品数据定义
// 定义商店中出售的商品类型、价格、库存等

using UnityEngine;
using System;
using System.Collections.Generic;

namespace JRPG.Shop
{
    /// <summary>
    /// 商店类型
    /// </summary>
    public enum ShopType
    {
        Daily,              // 每日商店
        Premium,            // 高级商店
        Arena,              // 竞技场商店
        Guild,              // 公会商店
        Event,              // 活动商店
        BlackMarket         // 黑市
    }

    /// <summary>
    /// 商品类型
    /// </summary>
    public enum ItemCategory
    {
        Character,           // 角色
        Equipment,           // 装备
        Material,            // 材料
        Consumable,          // 消耗品
        Currency,            // 货币
        SkillBook,           // 技能书
        Fragment             // 碎片
    }

    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyType
    {
        Gold,                // 金币
        Diamond,             // 钻石
        ArenaCoin,           // 竞技场币
        GuildCoin,           // 公会币
        EventCoin            // 活动币
    }

    /// <summary>
    /// 刷新类型
    /// </summary>
    public enum RefreshType
    {
        Never,               // 不刷新
        Daily,               // 每日刷新
        Weekly,              // 每周刷新
        Manual               // 手动刷新
    }

    /// <summary>
    /// 购买限制类型
    /// </summary>
    public enum PurchaseLimitType
    {
        None,                // 无限制
        Daily,               // 每日限购
        Weekly,              // 每周限购
        Total                // 终身限购
    }

    /// <summary>
    /// 商店商品数据
    /// 定义单个商品的属性
    /// </summary>
    [Serializable]
    public class ShopItemData
    {
        [Header("商品标识")]
        /// <summary>
        /// 商品唯一ID
        /// </summary>
        public int itemId;

        /// <summary>
        /// 商品名称
        /// </summary>
        public string itemName;

        /// <summary>
        /// 商品分类
        /// </summary>
        public ItemCategory category;

        [Header("价格配置")]
        /// <summary>
        /// 原价（显示划线价格）
        /// </summary>
        public int originalPrice;

        /// <summary>
        /// 实际售价
        /// </summary>
        public int price;

        /// <summary>
        /// 货币类型
        /// </summary>
        public CurrencyType currencyType = CurrencyType.Gold;

        [Header("库存与限制")]
        /// <summary>
        /// 初始库存数量（-1表示无限）
        /// </summary>
        public int stock = -1;

        /// <summary>
        /// 限购类型
        /// </summary>
        public PurchaseLimitType limitType = PurchaseLimitType.None;

        /// <summary>
        /// 限购数量
        /// </summary>
        public int purchaseLimit = 1;

        /// <summary>
        /// 已购买数量（运行时）
        /// </summary>
        [HideInInspector]
        public int purchasedCount = 0;

        [Header("商品属性")]
        /// <summary>
        /// 关联ID（如角色ID、装备ID等）
        /// </summary>
        public int relatedId;

        /// <summary>
        /// 数量（购买后获得的物品数量）
        /// </summary>
        public int quantity = 1;

        /// <summary>
        /// 是否为折扣商品
        /// </summary>
        public bool isDiscounted;

        /// <summary>
        /// 折扣标签（如"限时"、"hot"等）
        /// </summary>
        public string discountTag;

        [Header("显示配置")]
        /// <summary>
        /// 商品图标路径
        /// </summary>
        public string iconPath;

        /// <summary>
        /// 商品描述
        /// </summary>
        [TextArea(2, 4)]
        public string description;

        /// <summary>
        /// 排序优先级（数字越小越靠前）
        /// </summary>
        public int sortOrder;

        /// <summary>
        /// 是否在商店中显示
        /// </summary>
        public bool visible = true;

        /// <summary>
        /// 是否推荐商品
        /// </summary>
        public bool isRecommended;

        /// <summary>
        /// 检查是否可以购买
        /// </summary>
        public bool CanPurchase()
        {
            // 检查库存
            if (stock >= 0 && purchasedCount >= stock)
                return false;

            // 检查限购
            if (limitType != PurchaseLimitType.None && purchasedCount >= purchaseLimit)
                return false;

            return true;
        }

        /// <summary>
        /// 获取剩余购买次数
        /// </summary>
        public int GetRemainingPurchases()
        {
            if (stock >= 0)
                return Mathf.Max(0, stock - purchasedCount);

            if (limitType != PurchaseLimitType.None)
                return Mathf.Max(0, purchaseLimit - purchasedCount);

            return -1; // 无限制
        }
    }

    /// <summary>
    /// 商店数据资产
    /// 定义一个完整商店的配置
    /// </summary>
    [CreateAssetMenu(fileName = "ShopData", menuName = "JRPG/Shop/Shop Data")]
    public class ShopData : ScriptableObject
    {
        [Header("商店配置")]
        /// <summary>
        /// 商店ID
        /// </summary>
        public int shopId;

        /// <summary>
        /// 商店名称
        /// </summary>
        public string shopName;

        /// <summary>
        /// 商店类型
        /// </summary>
        public ShopType shopType;

        /// <summary>
        /// 商店描述
        /// </summary>
        [TextArea(2, 3)]
        public string description;

        [Header("刷新配置")]
        /// <summary>
        /// 刷新类型
        /// </summary>
        public RefreshType refreshType = RefreshType.Daily;

        /// <summary>
        /// 刷新消耗（手动刷新时）
        /// </summary>
        public int refreshCost;

        /// <summary>
        /// 刷新消耗货币类型
        /// </summary>
        public CurrencyType refreshCurrencyType = CurrencyType.Diamond;

        /// <summary>
        /// 刷新时间（每日刷新的具体时间）
        /// </summary>
        public string refreshTime = "00:00";

        [Header("商品配置")]
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<ShopItemData> items = new List<ShopItemData>();

        /// <summary>
        /// 每日商品数量
        /// </summary>
        public int dailyItemCount = 6;

        /// <summary>
        /// 刷新后是否重置限购
        /// </summary>
        public bool resetPurchaseLimitOnRefresh = true;

        [Header("显示配置")]
        /// <summary>
        /// 商店图标
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// 解锁等级
        /// </summary>
        public int unlockLevel = 1;

        /// <summary>
        /// 获取可用商品（考虑库存和限购）
        /// </summary>
        public List<ShopItemData> GetAvailableItems()
        {
            List<ShopItemData> available = new List<ShopItemData>();
            foreach (var item in items)
            {
                if (item.visible && item.CanPurchase())
                    available.Add(item);
            }
            return available;
        }

        /// <summary>
        /// 获取推荐商品
        /// </summary>
        public List<ShopItemData> GetRecommendedItems()
        {
            List<ShopItemData> recommended = new List<ShopItemData>();
            foreach (var item in items)
            {
                if (item.visible && item.isRecommended && item.CanPurchase())
                    recommended.Add(item);
            }
            return recommended;
        }
    }
}
