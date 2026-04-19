// CharacterInstance.cs - 角色运行时实例
// 管理角色在游戏中的实际状态数据

using System;
using UnityEngine;

// EquipmentSlot 枚举定义在 Equipment 命名空间

namespace JRPG.Character
{
    /// <summary>
    /// 角色实例数据 - 存储角色运行时的可变数据
    /// </summary>
    [Serializable]
    public class CharacterInstance
    {
        #region 基础信息
        
        [Tooltip("实例唯一ID")]
        public string instanceId;
        
        [Tooltip("关联的角色数据ID")]
        public int characterId;
        
        [Tooltip("角色名称（可自定义）")]
        public string customName;
        
        #endregion
        
        #region 等级与经验
        
        [Tooltip("当前等级")]
        public int level = 1;
        
        [Tooltip("当前经验值")]
        public long currentExp = 0;
        
        [Tooltip("突破次数 (0-5)")]
        public int breakthroughCount = 0;
        
        [Tooltip("命座数/共鸣等级 (0-6)")]
        public int constellationLevel = 0;
        
        #endregion
        
        #region 装备
        
        [Tooltip("已装备的物品ID列表")]
        public string[] equippedItems = new string[6]; // 6个装备槽位
        
        #endregion
        
        #region 技能
        
        [Tooltip("主动技能等级列表")]
        public int[] activeSkillLevels;
        
        [Tooltip("被动技能等级列表")]
        public int[] passiveSkillLevels;
        
        [Tooltip("终极技能等级")]
        public int ultimateSkillLevel = 1;
        
        #endregion
        
        #region 羁绊
        
        [Tooltip("好感度/羁绊等级")]
        public int affectionLevel = 1;
        
        [Tooltip("好感度经验值")]
        public int affectionExp = 0;
        
        #endregion
        
        #region 属性加成来源
        
        [Tooltip("装备提供的总属性加成")]
        public CharacterStats equipmentBonus = new CharacterStats();
        
        [Tooltip("羁绊提供的属性加成百分比")]
        public float bondBonusPercent = 0f;
        
        [Tooltip("命座提供的属性加成")]
        public CharacterStats constellationBonus = new CharacterStats();
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 角色是否已达到最大突破等级
        /// </summary>
        public bool IsMaxBreakthrough => breakthroughCount >= 5;
        
        /// <summary>
        /// 角色当前等级上限（基于突破等级）
        /// </summary>
        public int CurrentMaxLevel => 100 + (breakthroughCount * 20);
        
        /// <summary>
        /// 角色是否已满级
        /// </summary>
        public bool IsMaxLevel => level >= CurrentMaxLevel;
        
        /// <summary>
        /// 命座是否已满
        /// </summary>
        public bool IsMaxConstellation => constellationLevel >= 6;
        
        /// <summary>
        /// 获取角色显示名称
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(customName) ? 
            GetCharacterData()?.characterName ?? "未知角色" : customName;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 创建角色实例
        /// </summary>
        public CharacterInstance(int characterId)
        {
            this.instanceId = Guid.NewGuid().ToString();
            this.characterId = characterId;
            this.level = 1;
            this.currentExp = 0;
            this.breakthroughCount = 0;
            this.constellationLevel = 0;
            this.affectionLevel = 1;
            this.affectionExp = 0;
            
            // 初始化装备槽位
            this.equippedItems = new string[6];
        }
        
        /// <summary>
        /// 从存档数据创建实例
        /// </summary>
        public CharacterInstance(string instanceId, int characterId, int level, long exp,
            int breakthrough, int constellation, int affection, int affectionExp)
        {
            this.instanceId = instanceId;
            this.characterId = characterId;
            this.level = level;
            this.currentExp = exp;
            this.breakthroughCount = breakthrough;
            this.constellationLevel = constellation;
            this.affectionLevel = affection;
            this.affectionExp = affectionExp;
            this.equippedItems = new string[6];
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取关联的角色数据
        /// </summary>
        public CharacterData GetCharacterData()
        {
            return Resources.Load<CharacterData>($"Data/Characters/Character_{characterId}");
        }
        
        /// <summary>
        /// 获取角色总属性（基础+加成）
        /// </summary>
        public CharacterStats GetTotalStats(CharacterStatsCalculator calculator)
        {
            return calculator.CalculateTotalStats(this);
        }
        
        /// <summary>
        /// 升级到指定等级
        /// </summary>
        public void SetLevel(int newLevel)
        {
            newLevel = Mathf.Clamp(newLevel, 1, CurrentMaxLevel);
            level = newLevel;
        }
        
        /// <summary>
        /// 添加经验值
        /// </summary>
        /// <returns>是否发生升级</returns>
        public bool AddExp(long exp)
        {
            currentExp += exp;
            return ExperienceManager.Instance.TryLevelUp(this);
        }
        
        /// <summary>
        /// 获取下一个等级需要的经验值
        /// </summary>
        public long GetExpForNextLevel()
        {
            return ExperienceManager.Instance.GetExpForLevel(level + 1, breakthroughCount);
        }
        
        /// <summary>
        /// 装备物品
        /// </summary>
        public bool EquipItem(string itemInstanceId, EquipmentSlot slot)
        {
            int slotIndex = (int)slot;
            if (slotIndex < 0 || slotIndex >= equippedItems.Length)
                return false;
            
            equippedItems[slotIndex] = itemInstanceId;
            return true;
        }
        
        /// <summary>
        /// 卸下装备
        /// </summary>
        public bool UnequipItem(EquipmentSlot slot)
        {
            int slotIndex = (int)slot;
            if (slotIndex < 0 || slotIndex >= equippedItems.Length)
                return false;
            
            equippedItems[slotIndex] = null;
            return true;
        }
        
        /// <summary>
        /// 获取指定槽位的装备ID
        /// </summary>
        public string GetEquippedItemId(EquipmentSlot slot)
        {
            int slotIndex = (int)slot;
            if (slotIndex < 0 || slotIndex >= equippedItems.Length)
                return null;
            
            return equippedItems[slotIndex];
        }
        
        #endregion
        
        #region 数据转换
        
        /// <summary>
        /// 转换为可保存的JSON数据
        /// </summary>
        public CharacterInstanceData ToSaveData()
        {
            return new CharacterInstanceData
            {
                instanceId = instanceId,
                characterId = characterId,
                customName = customName,
                level = level,
                currentExp = currentExp,
                breakthroughCount = breakthroughCount,
                constellationLevel = constellationLevel,
                affectionLevel = affectionLevel,
                affectionExp = affectionExp,
                activeSkillLevels = activeSkillLevels,
                passiveSkillLevels = passiveSkillLevels,
                ultimateSkillLevel = ultimateSkillLevel,
                equippedItems = equippedItems
            };
        }
        
        /// <summary>
        /// 从保存数据恢复
        /// </summary>
        public static CharacterInstance FromSaveData(CharacterInstanceData data)
        {
            var instance = new CharacterInstance(data.instanceId, data.characterId, 
                data.level, data.currentExp, data.breakthroughCount, 
                data.constellationLevel, data.affectionLevel, data.affectionExp);
            
            instance.customName = data.customName;
            instance.activeSkillLevels = data.activeSkillLevels ?? new int[3];
            instance.passiveSkillLevels = data.passiveSkillLevels ?? new int[3];
            instance.ultimateSkillLevel = data.ultimateSkillLevel;
            instance.equippedItems = data.equippedItems ?? new string[6];
            
            return instance;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 角色实例保存数据
    /// </summary>
    [Serializable]
    public class CharacterInstanceData
    {
        public string instanceId;
        public int characterId;
        public string customName;
        public int level;
        public long currentExp;
        public int breakthroughCount;
        public int constellationLevel;
        public int affectionLevel;
        public int affectionExp;
        public int[] activeSkillLevels;
        public int[] passiveSkillLevels;
        public int ultimateSkillLevel;
        public string[] equippedItems;
    }
}
