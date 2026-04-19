using UnityEngine;
using System.Collections.Generic;

namespace JRPGStudio.Data
{
    /// <summary>
    /// 技能数据库
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "JRPG/Skills/Skill Database")]
    public class SkillDatabase : ScriptableObject
    {
        public List<SkillInfo> skills = new List<SkillInfo>();

        public SkillInfo GetSkillById(int id)
        {
            return skills.Find(s => s.skillId == id);
        }
    }

    /// <summary>
    /// 技能信息
    /// </summary>
    [System.Serializable]
    public class SkillInfo
    {
        [Header("基础信息")]
        public int skillId;
        public string skillName;
        public string description;
        public SkillType skillType;
        public TargetType targetType;
        public DamageType damageType;
        public Element element;

        [Header("消耗与冷却")]
        public int mpCost = 10;
        public int apCost = 10;
        public int cooldown = 0;

        [Header("效果")]
        public int power = 100;          // 威力百分比
        public int hitCount = 1;         // 连击数
        public float accuracy = 100f;    // 命中率

        [Header("附加效果")]
        public List<SkillEffect> effects = new List<SkillEffect>();

        [Header("动画与音效")]
        public string animationTrigger;
        public string vfxPath;
        public string sfxPath;
    }

    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        Attack = 0,     // 攻击
        Defense = 1,    // 防御
        Support = 2,    // 辅助
        Heal = 3,       // 治疗
        Passive = 4,    // 被动
        Ultimate = 5,   // 终极技
        RaceSkill = 6   // 种族技
    }

    /// <summary>
    /// 目标类型
    /// </summary>
    public enum TargetType
    {
        SingleEnemy = 0,    // 单体敌人
        AllEnemies = 1,     // 全体敌人
        RandomEnemy = 2,    // 随机敌人
        SingleAlly = 3,     // 单体友方
        AllAllies = 4,      // 全体友方
        Self = 5,           // 自身
        SelfAndAllies = 6   // 自身与全体友方
    }

    /// <summary>
    /// 伤害类型
    /// </summary>
    public enum DamageType
    {
        Physical = 0,   // 物理
        Magical = 1,    // 魔法
        True = 2,       // 真实伤害
        None = 3        // 无伤害
    }

    /// <summary>
    /// 技能效果
    /// </summary>
    [System.Serializable]
    public class SkillEffect
    {
        public EffectType effectType;
        public int value;
        public int duration;        // 持续回合数
        public float chance = 100f; // 触发概率
    }

    /// <summary>
    /// 效果类型
    /// </summary>
    public enum EffectType
    {
        // Buff
        AttackUp = 100,
        DefenseUp = 101,
        SpeedUp = 102,
        MagicAtkUp = 103,
        MagicDefUp = 104,
        CriticalUp = 105,
        
        // Debuff
        AttackDown = 200,
        DefenseDown = 201,
        SpeedDown = 202,
        MagicAtkDown = 203,
        MagicDefDown = 204,
        
        // 状态异常
        Poison = 300,
        Burn = 301,
        Freeze = 302,
        Paralyze = 303,
        Sleep = 304,
        Stun = 305,
        Confuse = 306,
        Silence = 307,
        
        // 特殊效果
        Heal = 400,
        HealOverTime = 401,
        Shield = 402,
        Revive = 403,
        Cleanse = 404,
        Dispel = 405
    }
}
