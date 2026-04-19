// ItemData.cs - 物品基类
// 定义游戏中所有物品的基础数据

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JRPG.Resources
{
    /// <summary>
    /// 物品类型枚举
    /// </summary>
    public enum ItemType
    {
        /// <summary>角色碎片</summary>
        CharacterFragment,
        
        /// <summary>经验道具</summary>
        ExpItem,
        
        /// <summary>技能书</summary>
        SkillBook,
        
        /// <summary>突破材料</summary>
        BreakthroughMaterial,
        
        /// <summary>强化石</summary>
        EnhanceStone,
        
        /// <summary>消耗品</summary>
        Consumable,
        
        /// <summary>任务道具</summary>
        QuestItem,
        
        /// <summary>其他</summary>
        Other
    }
    
    /// <summary>
    /// 物品稀有度
    /// </summary>
    public enum ItemRarity
    {
        /// <summary>普通</summary>
        N,
        
        /// <summary>稀有</summary>
        R,
        
        /// <summary>史诗</summary>
        SR,
        
        /// <summary>传说</summary>
        SSR
    }
    
    /// <summary>
    /// 物品数据基类
    /// </summary>
    [Serializable]
    public class ItemData
    {
        [Header("基础信息")]
        [Tooltip("物品ID")]
        public string itemId;
        
        [Tooltip("物品名称")]
        public string itemName;
        
        [Tooltip("物品描述")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("物品图标")]
        public Sprite icon;
        
        [Header("分类")]
        [Tooltip("物品类型")]
        public ItemType itemType;
        
        [Tooltip("稀有度")]
        public ItemRarity rarity;
        
        [Header("使用配置")]
        [Tooltip("是否可使用")]
        public bool canUse = false;
        
        [Tooltip("是否可出售")]
        public bool canSell = true;
        
        [Tooltip("出售价格")]
        public int sellPrice = 10;
        
        [Tooltip("是否可批量使用")]
        public bool batchUse = true;
        
        [Header("堆叠配置")]
        [Tooltip("最大堆叠数量")]
        public int maxStack = 999;
        
        [Header("效果配置")]
        [Tooltip("使用效果ID")]
        public string effectId;
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.N => Color.white,
                ItemRarity.R => new Color(0.3f, 0.7f, 0.3f),
                ItemRarity.SR => new Color(0.1f, 0.6f, 0.9f),
                ItemRarity.SSR => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public string GetRarityName()
        {
            return rarity switch
            {
                ItemRarity.N => "普通",
                ItemRarity.R => "稀有",
                ItemRarity.SR => "史诗",
                ItemRarity.SSR => "传说",
                _ => "普通"
            };
        }
    }
    
    /// <summary>
    /// 物品实例
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        [Tooltip("实例ID")]
        public string instanceId;
        
        [Tooltip("物品ID")]
        public string itemId;
        
        [Tooltip("数量")]
        public int count = 1;
        
        [Tooltip("获得时间戳")]
        public long acquiredTime;
        
        [Tooltip("过期时间戳（0表示不过期）")]
        public long expireTime;
        
        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired => expireTime > 0 && DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expireTime;
        
        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        public long RemainingTime => expireTime > 0 ? Mathf.Max(0, (int)(expireTime - DateTimeOffset.UtcNow.ToUnixTimeSeconds())) : 0;
        
        public ItemInstance(string itemId, int count = 1)
        {
            this.instanceId = Guid.NewGuid().ToString();
            this.itemId = itemId;
            this.count = count;
            this.acquiredTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
    
    /// <summary>
    /// 物品使用效果配置
    /// </summary>
    [Serializable]
    public class ItemEffect
    {
        [Tooltip("效果类型")]
        public ItemEffectType effectType;
        
        [Tooltip("效果数值")]
        public int value;
        
        [Tooltip("目标角色ID（空表示自己）")]
        public string targetCharacterId;
        
        [Tooltip("效果描述")]
        public string description;
    }
    
    /// <summary>
    /// 物品效果类型
    /// </summary>
    public enum ItemEffectType
    {
        /// <summary>无效果</summary>
        None,
        
        /// <summary>恢复体力</summary>
        RecoverStamina,
        
        /// <summary>恢复体力百分比</summary>
        RecoverStaminaPercent,
        
        /// <summary>获得经验</summary>
        GainExp,
        
        /// <summary>获得金币</summary>
        GainGold,
        
        /// <summary>角色好感度</summary>
        GainAffection
    }
    
    /// <summary>
    /// 物品ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "JRPG/Item Data")]
    public class ItemDataScriptable : ScriptableObject
    {
        public string itemId;
        public string itemName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public ItemType itemType;
        public ItemRarity rarity;
        public bool canUse = false;
        public bool canSell = true;
        public int sellPrice = 10;
        public int maxStack = 999;
        public List<ItemEffect> effects = new List<ItemEffect>();
        
        /// <summary>
        /// 转换为基础ItemData
        /// </summary>
        public ItemData ToItemData()
        {
            return new ItemData
            {
                itemId = itemId,
                itemName = itemName,
                description = description,
                icon = icon,
                itemType = itemType,
                rarity = rarity,
                canUse = canUse,
                canSell = canSell,
                sellPrice = sellPrice,
                maxStack = maxStack
            };
        }
    }
}
