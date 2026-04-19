using UnityEngine;
using System.Collections.Generic;

namespace JRPGStudio.Data
{
    /// <summary>
    /// 示例角色数据资源
    /// 包含游戏初始化时需要的角色定义
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDataList", menuName = "JRPG/Data/Character Data List")]
    public class CharacterDataList : ScriptableObject
    {
        [Header("角色列表")]
        public List<CharacterDefinition> characters = new List<CharacterDefinition>();

        /// <summary>
        /// 根据ID获取角色数据
        /// </summary>
        public CharacterDefinition GetCharacterById(int id)
        {
            return characters.Find(c => c.characterId == id);
        }

        /// <summary>
        /// 根据稀有度筛选角色
        /// </summary>
        public List<CharacterDefinition> GetCharactersByRarity(Rarity rarity)
        {
            return characters.FindAll(c => c.rarity == rarity);
        }
    }

    /// <summary>
    /// 角色定义数据
    /// </summary>
    [System.Serializable]
    public class CharacterDefinition
    {
        [Header("基础信息")]
        public int characterId;
        public string characterName;
        public string description;
        public Rarity rarity;
        public Race race;
        public CharacterClass characterClass;
        public Element element;

        [Header("基础属性")]
        public int baseHP = 100;
        public int baseMP = 50;
        public int baseAttack = 20;
        public int baseDefense = 15;
        public int baseMagicAtk = 20;
        public int baseMagicDef = 15;
        public int baseSpeed = 10;
        public int baseLuck = 5;

        [Header("成长系数")]
        public float hpGrowth = 10f;
        public float mpGrowth = 5f;
        public float atkGrowth = 2f;
        public float defGrowth = 1.5f;
        public float matkGrowth = 2f;
        public float mdefGrowth = 1.5f;
        public float spdGrowth = 0.5f;
        public float lukGrowth = 0.3f;

        [Header("技能")]
        public List<int> skillIds = new List<int>();
        public int ultimateSkillId;
        public int passiveSkillId;
        public int raceSkillId;

        [Header("突破材料")]
        public int breakthroughItemId;
        public int[] breakthroughCounts = new int[] { 5, 10, 20, 40, 80 };
    }

    /// <summary>
    /// 稀有度枚举
    /// </summary>
    public enum Rarity
    {
        N = 0,      // 普通
        R = 1,      // 稀有
        SR = 2,     // 超稀有
        SSR = 3     // 传说
    }

    /// <summary>
    /// 种族枚举
    /// </summary>
    public enum Race
    {
        Human = 0,      // 人族
        Beastfolk = 1,  // 兽人族
        Vampire = 2,    // 吸血鬼
        Angel = 3,      // 天使族
        Demonkin = 4    // 魔人族
    }

    /// <summary>
    /// 职业枚举
    /// </summary>
    public enum CharacterClass
    {
        Swordsman = 0,      // 剑士
        Mage = 1,           // 法师
        Priest = 2,         // 牧师
        Ranger = 3,         // 游侠
        Berserker = 4,      // 狂战士
        Knight = 5,         // 骑士
        Assassin = 6,       // 刺客
        Elementalist = 7,   // 元素使
        Summoner = 8,       // 召唤师
        Dancer = 9,         // 舞者
        Sage = 10,          // 贤者
        Ninja = 11          // 忍者
    }

    /// <summary>
    /// 元素枚举
    /// </summary>
    public enum Element
    {
        None = 0,
        Fire = 1,   // 火
        Water = 2,  // 水
        Wind = 3,   // 风
        Earth = 4,  // 地
        Thunder = 5,// 雷
        Ice = 6,    // 冰
        Light = 7,  // 光
        Dark = 8    // 暗
    }
}
