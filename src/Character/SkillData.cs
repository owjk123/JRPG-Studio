// SkillData.cs - 技能数据定义
// 定义技能的基础属性、效果、消耗等

using UnityEngine;
using System.Collections.Generic;
using JRPG.Battle;

namespace JRPG.Character
{
    /// <summary>
    /// 技能数据 - 定义技能的所有属性
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "JRPG/Skill Data")]
    public class SkillData : ScriptableObject
    {
        #region Basic Info
        
        [Header("基础信息")]
        public int skillId;
        public string skillName;
        [TextArea(2, 4)] public string description;
        public SkillType skillType;
        
        #endregion
        
        #region Cost & Cooldown
        
        [Header("消耗与冷却")]
        public int mpCost;
        public int cooldown;
        public int ultimateEnergyGain;
        
        #endregion
        
        #region Targeting
        
        [Header("目标设置")]
        public TargetType targetType;
        public int targetCount = 1;
        public TargetSelectionType selectionType;
        
        #endregion
        
        #region Damage
        
        [Header("伤害设置")]
        public DamageType damageType;
        public Element element;
        public int baseDamage;
        public float damageMultiplier = 1.0f;
        public float critBonus = 0f;
        
        #endregion
        
        #region Effects
        
        [Header("效果")]
        public List<SkillEffect> effects = new List<SkillEffect>();
        
        #endregion
        
        #region Animation
        
        [Header("动画")]
        public string animationTrigger;
        public GameObject vfxPrefab;
        public AudioClip soundEffect;
        public float animationDuration = 1.0f;
        
        #endregion
    }
    
    /// <summary>
    /// 被动技能数据
    /// </summary>
    [CreateAssetMenu(fileName = "PassiveSkillData", menuName = "JRPG/Passive Skill")]
    public class PassiveSkillData : ScriptableObject
    {
        [Header("基础信息")]
        public int skillId;
        public string skillName;
        [TextArea] public string description;
        
        [Header("触发条件")]
        public PassiveTrigger trigger;
        public float triggerChance = 1.0f;
        
        [Header("效果")]
        public List<PassiveEffect> effects = new List<PassiveEffect>();
    }
    
    #region Enums
    
    public enum SkillType
    {
        NormalAttack,    // 普通攻击
        ActiveSkill,     // 主动技能
        UltimateSkill,   // 终极技能
        PassiveSkill     // 被动技能
    }
    
    public enum TargetType
    {
        SingleEnemy,     // 单个敌人
        AllEnemies,      // 所有敌人
        RandomEnemy,     // 随机敌人
        Self,            // 自己
        SingleAlly,      // 单个队友
        AllAllies,       // 所有队友
        AllUnits         // 所有单位
    }
    
    public enum TargetSelectionType
    {
        Manual,          // 手动选择
        Auto,            // 自动选择
        AutoLowestHp,    // 自动选择血量最低
        AutoHighestAtk,  // 自动选择攻击最高
        Random           // 随机选择
    }
    
    public enum DamageType
    {
        Physical,        // 物理
        Magical,         // 魔法
        True,            // 真实伤害
        Percent,         // 百分比伤害
        Fixed            // 固定伤害
    }
    
    public enum PassiveTrigger
    {
        BattleStart,     // 战斗开始
        TurnStart,       // 回合开始
        TurnEnd,         // 回合结束
        OnAttack,        // 攻击时
        OnHit,           // 受击时
        OnCritical,      // 暴击时
        OnKill,          // 击杀时
        OnAllyDeath,     // 队友死亡时
        OnLowHp,         // 低血量时
        Always           // 永久
    }
    
    #endregion
    
    #region Effect Classes
    
    [System.Serializable]
    public class SkillEffect
    {
        public EffectType type;
        public int value;
        public float multiplier;
        public int duration;
        public StatusEffectData statusEffect;
    }
    
    [System.Serializable]
    public class PassiveEffect
    {
        public PassiveEffectType type;
        public float value;
        public bool isPercentage;
    }
    
    public enum EffectType
    {
        Damage,
        Heal,
        StatusApply,
        StatusRemove,
        Buff,
        Debuff,
        Revive,
        Summon,
        Special
    }
    
    public enum PassiveEffectType
    {
        StatBoost,
        CritRateBoost,
        CritDamageBoost,
        DamageBoost,
        DamageReduction,
        HealBoost,
        MpRecovery,
        SpeedBoost,
        Special
    }
    
    #endregion
}
