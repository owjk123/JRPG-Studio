using UnityEngine;
using System.Collections.Generic;

namespace JRPGStudio.Data
{
    /// <summary>
    /// 战斗遭遇数据
    /// 定义了一场战斗的敌人配置
    /// </summary>
    [CreateAssetMenu(fileName = "BattleEncounter", menuName = "JRPG/Battle/Battle Encounter")]
    public class BattleEncounterData : ScriptableObject
    {
        [Header("遭遇信息")]
        public int encounterId;
        public string encounterName;
        public EncounterType encounterType;
        public string backgroundScene;

        [Header("敌人配置")]
        public List<EnemySpawn> enemies = new List<EnemySpawn>();

        [Header("掉落")]
        public List<DropItem> dropItems = new List<DropItem>();
        public int expReward = 100;
        public int goldReward = 100;

        [Header("条件")]
        public int recommendedLevel = 1;
        public int staminaCost = 6;
    }

    /// <summary>
    /// 敌人生成配置
    /// </summary>
    [System.Serializable]
    public class EnemySpawn
    {
        public int enemyId;
        public Vector2Int position;     // 敌人站位 (0-4, 左到右)
        public int level = 1;
        public bool isBoss = false;
    }

    /// <summary>
    /// 掉落物品
    /// </summary>
    [System.Serializable]
    public class DropItem
    {
        public int itemId;
        public int minCount = 1;
        public int maxCount = 1;
        [Range(0f, 100f)] public float dropRate = 100f;
    }

    /// <summary>
    /// 遭遇类型
    /// </summary>
    public enum EncounterType
    {
        Story = 0,      // 剧情战斗
        Normal = 1,     // 普通战斗
        Elite = 2,      // 精英战斗
        Boss = 3,       // BOSS战斗
        Event = 4,      // 活动战斗
        Arena = 5       // 竞技场
    }
}
