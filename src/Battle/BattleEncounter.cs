// BattleEncounter.cs - 战斗遭遇配置
// 定义战斗的初始化数据

using UnityEngine;
using System;
using System.Collections.Generic;
using JRPG.Character;

namespace JRPG.Battle
{
    /// <summary>
    /// 战斗遭遇数据
    /// 定义一场战斗的初始配置
    /// </summary>
    [Serializable]
    public class BattleEncounter
    {
        [Header("玩家队伍")]
        [Tooltip("玩家角色数据列表")]
        public List<CharacterData> playerCharacters = new List<CharacterData>();
        
        [Header("敌人配置")]
        [Tooltip("敌人数据列表")]
        public List<EnemyFormation> enemies = new List<EnemyFormation>();
        
        [Header("战斗设置")]
        [Tooltip("战斗背景")]
        public Sprite battleBackground;
        
        [Tooltip("战斗BGM")]
        public AudioClip battleBGM;
        
        [Tooltip("是否允许逃跑")]
        public bool allowFlee = true;
        
        [Tooltip("战斗类型")]
        public BattleType battleType = BattleType.Normal;
        
        [Header("特殊设置")]
        [Tooltip("是否启用ATB")]
        public bool enableATB = true;
        
        [Tooltip("ATB充能速度倍率")]
        public float atbSpeedMultiplier = 1f;
        
        [Tooltip("最大回合数")]
        public int maxTurns = 99;
    }
    
    /// <summary>
    /// 敌人编队
    /// </summary>
    [Serializable]
    public class EnemyFormation
    {
        [Tooltip("敌人数据")]
        public CharacterData enemyData;
        
        [Tooltip("位置索引")]
        public int positionIndex;
        
        [Tooltip("等级")]
        public int level = 1;
        
        [Tooltip("是否Boss")]
        public bool isBoss = false;
    }
    
    /// <summary>
    /// 战斗类型
    /// </summary>
    public enum BattleType
    {
        Normal,         // 普通战斗
        Elite,          // 精英战斗
        Boss,           // Boss战
        Story,          // 剧情战斗
        Tutorial,       // 教程战斗
        Raid            // raid战斗
    }
    
    /// <summary>
    /// 快速创建敌人编队
    /// </summary>
    public static class EnemyFormationHelper
    {
        /// <summary>
        /// 创建单个敌人编队
        /// </summary>
        public static EnemyFormation CreateSingleEnemy(CharacterData data, int position = 0, int level = 1)
        {
            return new EnemyFormation
            {
                enemyData = data,
                positionIndex = position,
                level = level,
                isBoss = false
            };
        }
        
        /// <summary>
        /// 创建Boss编队
        /// </summary>
        public static EnemyFormation CreateBoss(CharacterData data, int position = 0, int level = 1)
        {
            return new EnemyFormation
            {
                enemyData = data,
                positionIndex = position,
                level = level,
                isBoss = true
            };
        }
        
        /// <summary>
        /// 从敌人数据列表创建编队
        /// </summary>
        public static List<EnemyFormation> CreateFormation(List<CharacterData> enemies, int startPosition = 0)
        {
            var formation = new List<EnemyFormation>();
            for (int i = 0; i < enemies.Count; i++)
            {
                formation.Add(CreateSingleEnemy(enemies[i], startPosition + i, 1));
            }
            return formation;
        }
    }
    
    /// <summary>
    /// 预制战斗配置（ScriptableObject）
    /// </summary>
    [CreateAssetMenu(fileName = "BattleEncounter", menuName = "JRPG/Battle Encounter")]
    public class BattleEncounterAsset : ScriptableObject
    {
        public BattleEncounter encounter;
    }
}
