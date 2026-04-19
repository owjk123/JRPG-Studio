// GameConfig.cs - 游戏全局配置
// 定义游戏的基础数值、难度设置、活动倍率等全局参数

using UnityEngine;
using System.Collections.Generic;

namespace JRPG.Config
{
    /// <summary>
    /// 游戏全局配置 - ScriptableObject管理游戏基础参数
    /// 包含难度设置、经验倍率、掉落倍率、数值平衡等
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "JRPG/Game Config")]
    public class GameConfig : ScriptableObject
    {
        #region Singleton
        
        private static GameConfig _instance;
        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameConfig>("Config/GameConfig");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 版本信息
        
        [Header("===== 版本信息 =====")]
        
        [Tooltip("游戏版本号")]
        public string gameVersion = "1.0.0";
        
        [Tooltip("数据版本号")]
        public int dataVersion = 1;
        
        [Tooltip("最后更新时间")]
        public string lastUpdateTime = "2024-01-01";
        
        #endregion
        
        #region 难度设置
        
        [Header("===== 难度设置 =====")]
        
        [Tooltip("当前难度等级")]
        public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
        
        [Tooltip("难度定义列表")]
        public List<DifficultySettings> difficultySettings = new List<DifficultySettings>();
        
        [System.Serializable]
        public class DifficultySettings
        {
            [Tooltip("难度名称")]
            public DifficultyLevel level;
            
            [Tooltip("敌人攻击力倍率")]
            [Range(0.5f, 3f)]
            public float enemyAttackMultiplier = 1f;
            
            [Tooltip("敌人防御力倍率")]
            [Range(0.5f, 3f)]
            public float enemyDefenseMultiplier = 1f;
            
            [Tooltip("敌人HP倍率")]
            [Range(0.5f, 5f)]
            public float enemyHpMultiplier = 1f;
            
            [Tooltip("敌人速度倍率")]
            [Range(0.8f, 1.5f)]
            public float enemySpeedMultiplier = 1f;
            
            [Tooltip("敌人暴击率加成")]
            [Range(0f, 0.5f)]
            public float enemyCritRateBonus = 0f;
            
            [Tooltip("敌人技能伤害倍率")]
            [Range(0.5f, 3f)]
            public float enemySkillDamageMultiplier = 1f;
            
            [Tooltip("获得经验倍率")]
            [Range(0.5f, 2f)]
            public float expMultiplier = 1f;
            
            [Tooltip("获得金币倍率")]
            [Range(0.5f, 2f)]
            public float goldMultiplier = 1f;
            
            [Tooltip("掉落倍率")]
            [Range(0.5f, 2f)]
            public float dropMultiplier = 1f;
            
            [Tooltip("是否允许自动战斗")]
            public bool allowAutoBattle = true;
            
            [Tooltip("是否允许跳过战斗")]
            public bool allowBattleSkip = false;
        }
        
        public enum DifficultyLevel
        {
            Easy,       // 简单
            Normal,     // 普通
            Hard,      // 困难
            Nightmare, // 噩梦
            Hell       // 地狱
        }
        
        #endregion
        
        #region 经验与等级
        
        [Header("===== 经验与等级 =====")]
        
        [Tooltip("初始角色等级上限")]
        public int initialLevelCap = 100;
        
        [Tooltip("突破后等级上限加成")]
        public int breakthroughLevelBonus = 20;
        
        [Tooltip("基础经验需求公式系数")]
        public float baseExpFormula = 100f;
        
        [Tooltip("经验需求增长指数")]
        public float expGrowthRate = 1.5f;
        
        [Tooltip("角色升级经验倍率")]
        [Range(0.1f, 3f)]
        public float characterExpMultiplier = 1f;
        
        [Tooltip("好友助阵经验加成")]
        [Range(0f, 1f)]
        public float friendSupportExpBonus = 0.2f;
        
        [Tooltip("是否有等级压制惩罚")]
        public bool enableLevelPenalty = true;
        
        [Tooltip("等级压制惩罚阈值")]
        public int levelPenaltyThreshold = 10;
        
        [Tooltip("等级压制惩罚系数")]
        [Range(0f, 1f)]
        public float levelPenaltyFactor = 0.5f;
        
        #endregion
        
        #region 金币与资源
        
        [Header("===== 金币与资源 =====")]
        
        [Tooltip("基础金币获取倍率")]
        [Range(0.1f, 5f)]
        public float goldMultiplier = 1f;
        
        [Tooltip("每日金币上限（0表示无限制）")]
        public int dailyGoldLimit = 0;
        
        [Tooltip("关卡基础金币奖励")]
        public int baseGoldPerStage = 100;
        
        [Tooltip("金币增长率")]
        public float goldGrowthRate = 1.2f;
        
        [Tooltip("Boss关卡金币加成")]
        [Range(1f, 5f)]
        public float bossGoldMultiplier = 2f;
        
        #endregion
        
        #region 掉落配置
        
        [Header("===== 掉落配置 =====")]
        
        [Tooltip("基础掉落倍率")]
        [Range(0.1f, 5f)]
        public float dropMultiplier = 1f;
        
        [Tooltip("稀有物品掉落倍率")]
        [Range(0.1f, 5f)]
        public float rareDropMultiplier = 1f;
        
        [Tooltip("装备掉落倍率")]
        [Range(0.1f, 5f)]
        public float equipmentDropMultiplier = 1f;
        
        [Tooltip("是否启用掉落保护机制")]
        public bool enableDropProtection = true;
        
        [Tooltip("稀有掉落保底次数")]
        public int rareDropProtectionCount = 10;
        
        [Tooltip("Boss必掉装备")]
        public bool bossGuaranteeEquipment = true;
        
        [Tooltip("精英怪额外掉落概率")]
        [Range(0f, 1f)]
        public float eliteExtraDropChance = 0.3f;
        
        #endregion
        
        #region 战斗数值
        
        [Header("===== 战斗数值 =====")]
        
        [Tooltip("暴击伤害基础倍率")]
        [Range(1.0f, 3f)]
        public float baseCritDamageMultiplier = 1.5f;
        
        [Tooltip("暴击率上限")]
        [Range(0f, 1f)]
        public float maxCritRate = 0.8f;
        
        [Tooltip("闪避率上限")]
        [Range(0f, 1f)]
        public float maxEvadeRate = 0.7f;
        
        [Tooltip("伤害波动范围")]
        [Range(0f, 0.3f)]
        public float damageVariance = 0.1f;
        
        [Tooltip("最小伤害下限")]
        public int minimumDamage = 1;
        
        [Tooltip("防御力减伤系数")]
        [Range(0f, 1f)]
        public float defenseReductionFactor = 0.5f;
        
        [Tooltip("防御力减伤上限（百分比）")]
        [Range(0f, 0.9f)]
        public float maxDefenseReduction = 0.7f;
        
        [Tooltip("属性克制伤害加成")]
        [Range(1f, 2f)]
        public float elementalAdvantageBonus = 1.5f;
        
        [Tooltip("属性克制被克制惩罚")]
        [Range(0.5f, 1f)]
        public float elementalDisadvantagePenalty = 0.75f;
        
        [Tooltip("ATB充能速度")]
        [Range(1f, 5f)]
        public float atbChargeSpeed = 2f;
        
        [Tooltip("ATB满槽时间（秒）")]
        public float atbFullChargeTime = 3f;
        
        #endregion
        
        #region 技能配置
        
        [Header("===== 技能配置 =====")]
        
        [Tooltip("技能等级上限")]
        public int skillLevelCap = 20;
        
        [Tooltip("技能升级经验需求倍率")]
        [Range(1f, 3f)]
        public float skillUpgradeExpMultiplier = 1f;
        
        [Tooltip("终极技能解锁等级")]
        public int ultimateSkillUnlockLevel = 50;
        
        [Tooltip("终极技能能量消耗")]
        public int ultimateSkillEnergyCost = 100;
        
        [Tooltip("被动技能槽位上限")]
        public int maxPassiveSkillSlots = 3;
        
        [Tooltip("技能冷却显示精度")]
        public SkillCooldownPrecision cooldownPrecision = SkillCooldownPrecision.Second;
        
        public enum SkillCooldownPrecision
        {
            Second,     // 秒
            OneDecimal, // 一位小数
            TwoDecimal  // 两位小数
        }
        
        #endregion
        
        #region 装备配置
        
        [Header("===== 装备配置 =====")]
        
        [Tooltip("装备强化等级上限")]
        public int maxEnhancementLevel = 15;
        
        [Tooltip("强化成功率基础值")]
        [Range(0f, 1f)]
        public float baseEnhancementSuccessRate = 0.5f;
        
        [Tooltip("强化成功率衰减率")]
        public float enhancementSuccessDecay = 0.1f;
        
        [Tooltip("装备突破等级上限")]
        public int maxBreakthroughLevel = 5;
        
        [Tooltip("套装件数阈值")]
        public List<int> setPieceThresholds = new List<int> { 2, 4, 6 };
        
        [Tooltip("套装效果件数加成")]
        [Range(0f, 1f)]
        public float setBonusEffectIncrement = 0.1f;
        
        #endregion
        
        #region 体力与恢复
        
        [Header("===== 体力与恢复 =====")]
        
        [Tooltip("最大体力值")]
        public int maxStamina = 120;
        
        [Tooltip("体力恢复间隔（分钟）")]
        public float staminaRecoveryInterval = 5f;
        
        [Tooltip("每次恢复体力")]
        public int staminaRecoveryAmount = 1;
        
        [Tooltip("体力购买最大次数")]
        public int maxStaminaPurchaseCount = 10;
        
        [Tooltip("体力购买费用递增值")]
        public int staminaPurchaseCostIncrement = 10;
        
        [Tooltip("体力购买基础费用")]
        public int staminaPurchaseBaseCost = 50;
        
        [Tooltip("体力溢出上限（百分比）")]
        [Range(1f, 3f)]
        public float maxStaminaOverflow = 1.5f;
        
        #endregion
        
        #region 抽卡配置
        
        [Header("===== 抽卡配置 =====")]
        
        [Tooltip("单抽消耗")]
        public int singleGachaCost = 280;
        
        [Tooltip("十连消耗")]
        public int multiGachaCost = 2800;
        
        [Tooltip("十连是否保底4星")]
        public bool multiGachaGuarantee4Star = true;
        
        [Tooltip("保底所需抽数")]
        public int pityPullCount = 90;
        
        [Tooltip("保底触发概率（%）")]
        [Range(0f, 100f)]
        public float pityTriggerRate = 100f;
        
        [Tooltip("保底4星概率")]
        [Range(0f, 1f)]
        public float pity4StarRate = 0.8f;
        
        [Tooltip("保底5星概率")]
        [Range(0f, 1f)]
        public float pity5StarRate = 0.2f;
        
        [Tooltip("每日免费抽卡次数")]
        public int dailyFreeGachaCount = 0;
        
        [Tooltip("每日免费抽卡重置时间")]
        public string dailyFreeResetTime = "05:00";
        
        #endregion
        
        #region 活动配置
        
        [Header("===== 活动配置 =====")]
        
        [Tooltip("当前活动倍率")]
        public float currentEventMultiplier = 1f;
        
        [Tooltip("经验活动倍率")]
        [Range(1f, 5f)]
        public float eventExpMultiplier = 1f;
        
        [Tooltip("金币活动倍率")]
        [Range(1f, 5f)]
        public float eventGoldMultiplier = 1f;
        
        [Tooltip("掉落活动倍率")]
        [Range(1f, 5f)]
        public float eventDropMultiplier = 1f;
        
        [Tooltip("技能经验活动倍率")]
        [Range(1f, 5f)]
        public float eventSkillExpMultiplier = 1f;
        
        [Tooltip("活动开始时间")]
        public string eventStartTime = "";
        
        [Tooltip("活动结束时间")]
        public string eventEndTime = "";
        
        [Tooltip("是否有进行中的活动")]
        public bool hasActiveEvent = false;
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取当前难度设置
        /// </summary>
        public DifficultySettings GetCurrentDifficultySettings()
        {
            var settings = difficultySettings.Find(x => x.level == currentDifficulty);
            if (settings == null)
            {
                settings = new DifficultySettings { level = currentDifficulty };
            }
            return settings;
        }
        
        /// <summary>
        /// 计算等级压制后的经验获取
        /// </summary>
        public float CalculateExpAfterPenalty(int playerLevel, int enemyLevel)
        {
            if (!enableLevelPenalty) return 1f;
            
            int levelDiff = enemyLevel - playerLevel;
            if (levelDiff > levelPenaltyThreshold)
            {
                float penalty = levelPenaltyFactor * (levelDiff - levelPenaltyThreshold);
                return Mathf.Max(0.1f, 1f - penalty);
            }
            return 1f;
        }
        
        /// <summary>
        /// 计算强化成功率
        /// </summary>
        public float CalculateEnhancementSuccessRate(int currentLevel)
        {
            return Mathf.Max(0.05f, baseEnhancementSuccessRate - (currentLevel * enhancementSuccessDecay));
        }
        
        /// <summary>
        /// 计算有效经验倍率（包含活动和难度加成）
        /// </summary>
        public float GetEffectiveExpMultiplier()
        {
            float multiplier = characterExpMultiplier * eventExpMultiplier;
            var difficulty = GetCurrentDifficultySettings();
            return multiplier * difficulty.expMultiplier;
        }
        
        /// <summary>
        /// 计算有效掉落倍率
        /// </summary>
        public float GetEffectiveDropMultiplier()
        {
            float multiplier = dropMultiplier * eventDropMultiplier;
            var difficulty = GetCurrentDifficultySettings();
            return multiplier * difficulty.dropMultiplier;
        }
        
        /// <summary>
        /// 获取体力恢复间隔（秒）
        /// </summary>
        public float GetStaminaRecoveryIntervalSeconds()
        {
            return staminaRecoveryInterval * 60f;
        }
        
        /// <summary>
        /// 计算体力购买费用
        /// </summary>
        public int CalculateStaminaPurchaseCost(int purchaseCount)
        {
            return staminaPurchaseBaseCost + (purchaseCount * staminaPurchaseCostIncrement);
        }
        
        #endregion
    }
}
