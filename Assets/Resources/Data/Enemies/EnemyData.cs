using UnityEngine;

namespace JRPGStudio.Data
{
    /// <summary>
    /// 敌人数据
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "JRPG/Enemy/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基础信息")]
        public int enemyId;
        public string enemyName;
        public string description;
        public EnemyType enemyType;
        public Element element;

        [Header("属性")]
        public int baseHP = 500;
        public int baseMP = 100;
        public int baseAttack = 30;
        public int baseDefense = 20;
        public int baseMagicAtk = 30;
        public int baseMagicDef = 20;
        public int baseSpeed = 10;

        [Header("技能")]
        public int[] skillIds = new int[0];
        public int ultimateSkillId;
        public int[] passiveSkillIds = new int[0];

        [Header("AI设置")]
        public EnemyAIType aiType = EnemyAIType.Random;
        public float aggressionLevel = 0.5f;  // 攻击倾向

        [Header("视觉")]
        public string prefabPath;
        public string portraitPath;
    }

    /// <summary>
    /// 敌人类型
    /// </summary>
    public enum EnemyType
    {
        Normal = 0,     // 普通怪
        Elite = 1,      // 精英怪
        Boss = 2,       // BOSS
        MiniBoss = 3,   // 小BOSS
        EventBoss = 4   // 活动BOSS
    }

    /// <summary>
    /// 敌人AI类型
    /// </summary>
    public enum EnemyAIType
    {
        Random = 0,         // 随机使用技能
        Aggressive = 1,     // 优先攻击
        Defensive = 2,      // 优先防御/治疗
        Smart = 3,          // 智能决策
        Scripted = 4        // 脚本控制
    }
}
