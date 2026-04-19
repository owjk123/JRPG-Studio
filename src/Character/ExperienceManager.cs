// ExperienceManager.cs - 经验与升级管理系统
// 管理角色的经验获取、等级提升

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Resources;

namespace JRPG.Character
{
    /// <summary>
    /// 经验值管理单例类
    /// </summary>
    public class ExperienceManager
    {
        #region 单例
        
        private static ExperienceManager _instance;
        public static ExperienceManager Instance => _instance ??= new ExperienceManager();
        
        #endregion
        
        #region 配置常量
        
        /// <summary>
        /// 基础经验需求（每级）
        /// </summary>
        private const int BaseExpPerLevel = 100;
        
        /// <summary>
        /// 经验曲线指数
        /// </summary>
        private const float ExpCurveExponent = 1.5f;
        
        /// <summary>
        /// 各等级的经验需求表（用于快速查询）
        /// </summary>
        private long[] _expTable;
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 角色升级事件
        /// </summary>
        public event Action<CharacterInstance, int> OnLevelUp;
        
        /// <summary>
        /// 角色突破事件
        /// </summary>
        public event Action<CharacterInstance, int> OnBreakthrough;
        
        #endregion
        
        #region 构造函数
        
        private ExperienceManager()
        {
            InitializeExpTable();
        }
        
        /// <summary>
        /// 初始化经验表
        /// </summary>
        private void InitializeExpTable()
        {
            // 最高200级 + 1（索引0不使用）
            _expTable = new long[202];
            
            for (int level = 1; level <= 201; level++)
            {
                _expTable[level] = CalculateCumulativeExp(level);
            }
        }
        
        /// <summary>
        /// 计算指定等级的总累积经验（从1级开始）
        /// </summary>
        private long CalculateCumulativeExp(int level)
        {
            if (level <= 1)
                return 0;
            
            long total = 0;
            for (int i = 1; i < level; i++)
            {
                total += CalculateExpForSingleLevel(i);
            }
            return total;
        }
        
        /// <summary>
        /// 计算单级所需经验
        /// </summary>
        private long CalculateExpForSingleLevel(int level)
        {
            // 公式：基础经验 × 等级^1.5
            return Mathf.RoundToInt(BaseExpPerLevel * Mathf.Pow(level, ExpCurveExponent));
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取指定等级所需的总累积经验
        /// </summary>
        public long GetCumulativeExpForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, 201);
            return _expTable[level];
        }
        
        /// <summary>
        /// 获取当前等级升到下一级所需的经验
        /// </summary>
        public long GetExpForLevel(int level, int breakthroughCount = 0)
        {
            // 考虑突破等级
            int adjustedLevel = level;
            
            // 突破后等级从100开始重新计算
            if (breakthroughCount > 0 && level > 100)
            {
                adjustedLevel = level - 100;
            }
            
            return CalculateExpForSingleLevel(adjustedLevel);
        }
        
        /// <summary>
        /// 尝试为角色添加经验并处理升级
        /// </summary>
        /// <param name="instance">角色实例</param>
        /// <param name="exp">添加的经验值</param>
        /// <returns>是否发生升级</returns>
        public bool TryLevelUp(CharacterInstance instance, long exp = 0)
        {
            if (exp > 0)
            {
                instance.currentExp += exp;
            }
            
            bool leveledUp = false;
            int oldLevel = instance.level;
            
            // 检查是否可以升级
            while (instance.level < instance.CurrentMaxLevel)
            {
                long expNeeded = GetExpForLevel(instance.level + 1, instance.breakthroughCount);
                if (instance.currentExp >= expNeeded)
                {
                    instance.currentExp -= expNeeded;
                    instance.level++;
                    oldLevel = instance.level;
                    leveledUp = true;
                    
                    // 触发升级事件
                    OnLevelUp?.Invoke(instance, instance.level);
                    
                    Debug.Log($"角色 {instance.DisplayName} 升级到 {instance.level} 级");
                }
                else
                {
                    break;
                }
            }
            
            return leveledUp;
        }
        
        /// <summary>
        /// 获取当前升级进度百分比
        /// </summary>
        public float GetLevelProgress(CharacterInstance instance)
        {
            if (instance.level >= instance.CurrentMaxLevel)
                return 1f;
            
            long currentLevelExp = instance.currentExp;
            long nextLevelExp = GetExpForLevel(instance.level + 1, instance.breakthroughCount);
            
            return Mathf.Clamp01((float)currentLevelExp / nextLevelExp);
        }
        
        /// <summary>
        /// 计算溢出经验的升级预估
        /// </summary>
        public int EstimateLevelUps(CharacterInstance instance, long extraExp)
        {
            long totalExp = instance.currentExp + extraExp;
            int levelsGained = 0;
            int tempLevel = instance.level;
            
            while (tempLevel < instance.CurrentMaxLevel)
            {
                long expNeeded = GetExpForLevel(tempLevel + 1, instance.breakthroughCount);
                if (totalExp >= expNeeded)
                {
                    totalExp -= expNeeded;
                    tempLevel++;
                    levelsGained++;
                }
                else
                {
                    break;
                }
            }
            
            return levelsGained;
        }
        
        /// <summary>
        /// 添加经验值（带经验加成）
        /// </summary>
        public long ApplyExpBonus(long baseExp, float bonusPercent)
        {
            return Mathf.RoundToInt(baseExp * (1f + bonusPercent));
        }
        
        /// <summary>
        /// 获取突破后等级重置的经验重算
        /// </summary>
        public void RecalculateExpAfterBreakthrough(CharacterInstance instance)
        {
            // 突破后等级超过原上限时，保留超出部分的等级
            // 简化处理：清零重新开始
            instance.currentExp = 0;
            Debug.Log($"角色 {instance.DisplayName} 突破到 {instance.breakthroughCount + 1} 阶，等级上限提升");
        }
        
        #endregion
        
        #region 经验获取配置
        
        /// <summary>
        /// 经验获取倍率（用于活动等）
        /// </summary>
        public float ExpMultiplier { get; set; } = 1f;
        
        /// <summary>
        /// 经验获取类型
        /// </summary>
        public enum ExpSourceType
        {
            MainStage,      // 主线关卡
            DailyDungeon,   // 日常副本
            DailyQuest,     // 日常任务
            Item            // 经验道具
        }
        
        /// <summary>
        /// 获取指定来源的经验倍率
        /// </summary>
        public float GetExpMultiplier(ExpSourceType source)
        {
            float multiplier = 1f;
            
            switch (source)
            {
                case ExpSourceType.MainStage:
                    multiplier = 1f;
                    break;
                case ExpSourceType.DailyDungeon:
                    multiplier = 2f;
                    break;
                case ExpSourceType.DailyQuest:
                    multiplier = 1.5f;
                    break;
                case ExpSourceType.Item:
                    multiplier = 1f;
                    break;
            }
            
            return multiplier * ExpMultiplier;
        }
        
        /// <summary>
        /// 计算实际获得的经验
        /// </summary>
        public long CalculateGainedExp(long baseExp, ExpSourceType source, bool hasRacialBonus = false)
        {
            float exp = baseExp;
            
            // 应用来源倍率
            exp *= GetExpMultiplier(source);
            
            // 应用种族加成（人类经验+15%）
            if (hasRacialBonus)
            {
                exp *= 1.15f;
            }
            
            return Mathf.RoundToInt(exp);
        }
        
        #endregion
        
        #region 经验道具
        
        /// <summary>
        /// 经验道具数据
        /// </summary>
        [Serializable]
        public class ExpItemData
        {
            public string itemId;
            public string itemName;
            public long expValue;
            public string description;
        }
        
        /// <summary>
        /// 使用经验道具
        /// </summary>
        public bool UseExpItem(CharacterInstance instance, string itemId, long itemCount)
        {
            // 从背包获取道具
            var item = ResourcesManager.Instance.GetItem(itemId);
            if (item == null || item.count < itemCount)
                return false;
            
            // 获取经验值
            long totalExp = GetExpFromItem(itemId) * itemCount;
            
            // 消耗道具
            ResourcesManager.Instance.RemoveItem(itemId, itemCount);
            
            // 添加经验
            instance.AddExp(totalExp);
            
            return true;
        }
        
        /// <summary>
        /// 从道具ID获取经验值
        /// </summary>
        private long GetExpFromItem(string itemId)
        {
            // 简化处理，实际应从配置读取
            switch (itemId)
            {
                case "exp_small": return 100;
                case "exp_medium": return 500;
                case "exp_large": return 2000;
                case "exp_huge": return 10000;
                default: return 100;
            }
        }
        
        #endregion
    }
}
