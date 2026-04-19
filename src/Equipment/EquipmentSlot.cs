// EquipmentSlot.cs - 装备槽位枚举
// 定义所有可装备的位置

namespace JRPG.Equipment
{
    /// <summary>
    /// 装备槽位枚举
    /// </summary>
    public enum EquipmentSlot
    {
        /// <summary>武器</summary>
        Weapon = 0,
        
        /// <summary>头盔</summary>
        Helmet = 1,
        
        /// <summary>铠甲/衣服</summary>
        Armor = 2,
        
        /// <summary>护手</summary>
        Gloves = 3,
        
        /// <summary>饰品1</summary>
        Accessory1 = 4,
        
        /// <summary>饰品2</summary>
        Accessory2 = 5
    }
    
    /// <summary>
    /// 装备槽位配置
    /// </summary>
    public static class EquipmentSlotConfig
    {
        /// <summary>
        /// 槽位数量
        /// </summary>
        public const int SlotCount = 6;
        
        /// <summary>
        /// 获取槽位显示名称
        /// </summary>
        public static string GetSlotName(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => "武器",
                EquipmentSlot.Helmet => "头盔",
                EquipmentSlot.Armor => "铠甲",
                EquipmentSlot.Gloves => "护手",
                EquipmentSlot.Accessory1 => "饰品1",
                EquipmentSlot.Accessory2 => "饰品2",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取槽位对应的装备类型限制
        /// </summary>
        public static EquipmentType GetAllowedType(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => EquipmentType.Weapon,
                EquipmentSlot.Helmet => EquipmentType.Helmet,
                EquipmentSlot.Armor => EquipmentType.Armor,
                EquipmentSlot.Gloves => EquipmentType.Gloves,
                EquipmentSlot.Accessory1 => EquipmentType.Accessory,
                EquipmentSlot.Accessory2 => EquipmentType.Accessory,
                _ => EquipmentType.Accessory
            };
        }
        
        /// <summary>
        /// 饰品槽是否可装备指定类型
        /// </summary>
        public static bool CanEquipInSlot(EquipmentType equipType, EquipmentSlot slot)
        {
            if (slot == EquipmentSlot.Accessory1 || slot == EquipmentSlot.Accessory2)
            {
                return equipType == EquipmentType.Accessory;
            }
            return GetAllowedType(slot) == equipType;
        }
    }
    
    /// <summary>
    /// 装备类型枚举
    /// </summary>
    public enum EquipmentType
    {
        /// <summary>武器</summary>
        Weapon,
        
        /// <summary>头盔</summary>
        Helmet,
        
        /// <summary>铠甲</summary>
        Armor,
        
        /// <summary>护手</summary>
        Gloves,
        
        /// <summary>饰品</summary>
        Accessory
    }
    
    /// <summary>
    /// 装备稀有度
    /// </summary>
    public enum EquipmentRarity
    {
        /// <summary>普通</summary>
        N = 0,
        
        /// <summary>稀有</summary>
        R = 1,
        
        /// <summary>史诗</summary>
        SR = 2,
        
        /// <summary>传说</summary>
        SSR = 3,
        
        /// <summary>神器</summary>
        UR = 4
    }
    
    /// <summary>
    /// 装备稀有度配置
    /// </summary>
    public static class EquipmentRarityConfig
    {
        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public static string GetRarityName(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.N => "普通",
                EquipmentRarity.R => "稀有",
                EquipmentRarity.SR => "史诗",
                EquipmentRarity.SSR => "传说",
                EquipmentRarity.UR => "神器",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取稀有度颜色（十六进制）
        /// </summary>
        public static string GetRarityColor(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.N => "#FFFFFF",      // 白色
                EquipmentRarity.R => "#4CAF50",       // 绿色
                EquipmentRarity.SR => "#2196F3",      // 蓝色
                EquipmentRarity.SSR => "#9C27B0",    // 紫色
                EquipmentRarity.UR => "#FF9800",      // 橙色
                _ => "#FFFFFF"
            };
        }
        
        /// <summary>
        /// 获取稀有度属性倍率
        /// </summary>
        public static float GetRarityMultiplier(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.N => 1.0f,
                EquipmentRarity.R => 1.2f,
                EquipmentRarity.SR => 1.5f,
                EquipmentRarity.SSR => 1.8f,
                EquipmentRarity.UR => 2.2f,
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// 获取稀有度副属性数量范围
        /// </summary>
        public static (int min, int max) GetSubStatRange(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.N => (0, 1),
                EquipmentRarity.R => (1, 2),
                EquipmentRarity.SR => (2, 3),
                EquipmentRarity.SSR => (3, 4),
                EquipmentRarity.UR => (4, 4),
                _ => (0, 1)
            };
        }
    }
}
