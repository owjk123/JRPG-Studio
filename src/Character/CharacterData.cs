// CharacterData.cs - 角色数据ScriptableObject
// 定义角色的基础属性、技能、稀有度等

using UnityEngine;
using System.Collections.Generic;

namespace JRPG.Character
{
    /// <summary>
    /// 角色数据 - 定义角色的所有基础信息
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "JRPG/Character Data")]
    public class CharacterData : ScriptableObject
    {
        #region Basic Info
        
        [Header("基础信息")]
        [Tooltip("角色唯一ID")]
        public int characterId;
        
        [Tooltip("角色名称")]
        public string characterName;
        
        [Tooltip("角色称号")]
        public string title;
        
        [TextArea(3, 5)]
        [Tooltip("角色简介")]
        public string description;
        
        #endregion
        
        #region Classification
        
        [Header("分类")]
        [Tooltip("稀有度")]
        public Rarity rarity;
        
        [Tooltip("种族")]
        public Race race;
        
        [Tooltip("元素属性")]
        public Element element;
        
        [Tooltip("战斗定位")]
        public BattleRole role;
        
        [Tooltip("武器类型")]
        public WeaponType weaponType;
        
        #endregion
        
        #region Stats
        
        [Header("基础属性")]
        [Tooltip("初始等级上限")]
        public int maxLevel = 100;
        
        [Tooltip("基础HP")]
        public int baseHp = 1000;
        
        [Tooltip("HP成长系数")]
        public float hpGrowth = 1.2f;
        
        [Tooltip("基础攻击力")]
        public int baseAtk = 100;
        
        [Tooltip("攻击力成长系数")]
        public float atkGrowth = 1.1f;
        
        [Tooltip("基础防御力")]
        public int baseDef = 50;
        
        [Tooltip("防御力成长系数")]
        public float defGrowth = 1.1f;
        
        [Tooltip("基础速度")]
        public int baseSpeed = 100;
        
        [Tooltip("基础暴击率")]
        public float baseCritRate = 0.05f;
        
        [Tooltip("基础暴击伤害")]
        public float baseCritDamage = 1.5f;
        
        #endregion
        
        #region Skills
        
        [Header("技能")]
        [Tooltip("普通攻击")]
        public SkillData normalAttack;
        
        [Tooltip("主动技能列表")]
        public List<SkillData> activeSkills = new List<SkillData>();
        
        [Tooltip("被动技能列表")]
        public List<PassiveSkillData> passiveSkills = new List<PassiveSkillData>();
        
        [Tooltip("终极技能")]
        public SkillData ultimateSkill;
        
        #endregion
        
        #region Resources
        
        [Header("资源")]
        [Tooltip("基础MP上限")]
        public int baseMaxMp = 100;
        
        [Tooltip("终极技能能量上限")]
        public int maxUltimateEnergy = 100;
        
        #endregion
        
        #region Visual
        
        [Header("视觉资源")]
        [Tooltip("立绘")]
        public Sprite portrait;
        
        [Tooltip("战斗模型")]
        public GameObject battleModel;
        
        [Tooltip("Q版小人")]
        public GameObject chibiModel;
        
        #endregion
        
        #region Audio
        
        [Header("音频")]
        [Tooltip("角色CV")]
        public AudioClip[] voiceLines;
        
        [Tooltip("战斗语音")]
        public BattleVoiceSet battleVoices;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// 计算指定等级的属性
        /// </summary>
        public CharacterStats GetStatsAtLevel(int level)
        {
            return new CharacterStats
            {
                MaxHp = Mathf.RoundToInt(baseHp * Mathf.Pow(hpGrowth, level - 1)),
                Atk = Mathf.RoundToInt(baseAtk * Mathf.Pow(atkGrowth, level - 1)),
                Def = Mathf.RoundToInt(baseDef * Mathf.Pow(defGrowth, level - 1)),
                Speed = baseSpeed,
                CritRate = baseCritRate,
                CritDamage = baseCritDamage,
                MaxMp = baseMaxMp
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// 稀有度枚举
    /// </summary>
    public enum Rarity
    {
        N,      // 普通
        R,      // 稀有
        SR,     // 超稀有
        SSR     // 传说
    }
    
    /// <summary>
    /// 元素属性枚举
    /// </summary>
    public enum Element
    {
        Fire,   // 火
        Water,  // 水
        Wind,   // 风
        Earth,  // 地
        Light,  // 光
        Dark    // 暗
    }
    
    /// <summary>
    /// 战斗定位枚举
    /// </summary>
    public enum BattleRole
    {
        Attacker,   // 攻击手
        Defender,   // 防御者
        Support,    // 辅助
        Healer      // 治疗
    }
    
    /// <summary>
    /// 武器类型枚举
    /// </summary>
    public enum WeaponType
    {
        Sword,      // 剑
        Greatsword, // 大剑
        Staff,      // 法杖
        Bow,        // 弓
        Dagger,     // 匕首
        Spear,      // 枪
        Fist        // 拳套
    }
    
    /// <summary>
    /// 角色属性结构体
    /// </summary>
    [System.Serializable]
    public struct CharacterStats
    {
        public int MaxHp;
        public int Atk;
        public int Def;
        public int Speed;
        public float CritRate;
        public float CritDamage;
        public int MaxMp;
    }
    
    /// <summary>
    /// 战斗语音集
    /// </summary>
    [System.Serializable]
    public class BattleVoiceSet
    {
        public AudioClip battleStart;
        public AudioClip skillUse;
        public AudioClip ultimateUse;
        public AudioClip damage;
        public AudioClip victory;
        public AudioClip defeat;
    }
}
