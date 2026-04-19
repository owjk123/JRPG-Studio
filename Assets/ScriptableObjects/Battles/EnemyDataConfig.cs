// EnemyDataConfig.cs - 敌人数据配置文件
// 包含所有Demo战斗使用的敌人定义

using UnityEngine;

namespace JRPG.Data
{
    /// <summary>
    /// 敌人类型枚举
    /// </summary>
    public enum EnemyType
    {
        Normal = 0,      // 普通怪
        Elite = 1,       // 精英怪
        Boss = 2,        // BOSS
        MiniBoss = 3,    // 小BOSS
        EventBoss = 4    // 活动BOSS
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

    /// <summary>
    /// 元素类型
    /// </summary>
    public enum Element
    {
        None = 0,       // 无属性
        Fire,           // 火焰
        Ice,            // 寒冰
        Thunder,        // 雷电
        Light,          // 圣光
        Dark,           // 暗影
        Wind,           // 疾风
        Earth,          // 大地
        Water           // 水
    }

    /// <summary>
    /// 敌人静态数据表
    /// 用于在编辑器外快速查找敌人数据
    /// </summary>
    public static class EnemyDataConfig
    {
        /// <summary>
        /// 获取敌人静态数据
        /// 格式: enemyId -> EnemyStaticData
        /// </summary>
        public static EnemyStaticData GetEnemyData(int enemyId)
        {
            return enemyDatabase.GetValueOrDefault(enemyId, GetDefaultEnemy(enemyId));
        }

        /// <summary>
        /// 敌人静态数据库
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<int, EnemyStaticData> enemyDatabase = 
            new System.Collections.Generic.Dictionary<int, EnemyStaticData>
        {
            // ==================== 第一章敌人 ====================
            // 史莱姆系 (ID: 1)
            {
                1, new EnemyStaticData
                {
                    enemyId = 1,
                    enemyName = "史莱姆",
                    enemyType = EnemyType.Normal,
                    element = Element.Water,
                    baseHP = 200,
                    baseAttack = 25,
                    baseDefense = 10,
                    baseSpeed = 8,
                    baseExp = 10,
                    baseGold = 15,
                    skillIds = new int[] { 1001 }, // 普通攻击
                    aiType = EnemyAIType.Random,
                    description = "最弱小的怪物，水系属性，动作迟缓"
                }
            },
            // 强化史莱姆 (ID: 2)
            {
                2, new EnemyStaticData
                {
                    enemyId = 2,
                    enemyName = "精英史莱姆",
                    enemyType = EnemyType.Elite,
                    element = Element.Water,
                    baseHP = 500,
                    baseAttack = 50,
                    baseDefense = 20,
                    baseSpeed = 10,
                    baseExp = 30,
                    baseGold = 40,
                    skillIds = new int[] { 1001, 1002 },
                    aiType = EnemyAIType.Random,
                    description = "进化的史莱姆，具有一定攻击力"
                }
            },
            // 鱼人系 (ID: 3)
            {
                3, new EnemyStaticData
                {
                    enemyId = 3,
                    enemyName = "鱼人",
                    enemyType = EnemyType.Normal,
                    element = Element.Water,
                    baseHP = 400,
                    baseAttack = 45,
                    baseDefense = 15,
                    baseSpeed = 12,
                    baseExp = 20,
                    baseGold = 25,
                    skillIds = new int[] { 1001, 1003 },
                    aiType = EnemyAIType.Aggressive,
                    description = "栖息在水边的类人怪物，攻击性强"
                }
            },
            // 鱼人王 (ID: 4)
            {
                4, new EnemyStaticData
                {
                    enemyId = 4,
                    enemyName = "鱼人王",
                    enemyType = EnemyType.Elite,
                    element = Element.Water,
                    baseHP = 1200,
                    baseAttack = 80,
                    baseDefense = 35,
                    baseSpeed = 15,
                    baseExp = 80,
                    baseGold = 120,
                    skillIds = new int[] { 1001, 1003, 1004 },
                    aiType = EnemyAIType.Smart,
                    description = "鱼人族的领袖，拥有强大的水系魔法"
                }
            },
            // 哥布林系 (ID: 5, 6, 7)
            {
                5, new EnemyStaticData
                {
                    enemyId = 5,
                    enemyName = "哥布林弓箭手",
                    enemyType = EnemyType.Normal,
                    element = Element.None,
                    baseHP = 300,
                    baseAttack = 55,
                    baseDefense = 12,
                    baseSpeed = 14,
                    baseExp = 25,
                    baseGold = 35,
                    skillIds = new int[] { 1001, 1005 },
                    aiType = EnemyAIType.Aggressive,
                    description = "擅长远程攻击的哥布林"
                }
            },
            {
                6, new EnemyStaticData
                {
                    enemyId = 6,
                    enemyName = "哥布林战士",
                    enemyType = EnemyType.Normal,
                    element = Element.None,
                    baseHP = 600,
                    baseAttack = 65,
                    baseDefense = 30,
                    baseSpeed = 10,
                    baseExp = 35,
                    baseGold = 45,
                    skillIds = new int[] { 1001, 1006 },
                    aiType = EnemyAIType.Aggressive,
                    description = "近战型的哥布林战士，皮糙肉厚"
                }
            },
            {
                7, new EnemyStaticData
                {
                    enemyId = 7,
                    enemyName = "哥布林首领",
                    enemyType = EnemyType.Boss,
                    element = Element.None,
                    baseHP = 3000,
                    baseAttack = 120,
                    baseDefense = 60,
                    baseSpeed = 12,
                    baseExp = 200,
                    baseGold = 500,
                    skillIds = new int[] { 1001, 1006, 1007, 1008 },
                    aiType = EnemyAIType.Smart,
                    description = "哥布林族群的领袖，统领整个哥布林部落"
                }
            },

            // ==================== 第二章敌人 ====================
            // 蛛魔系 (ID: 8, 9)
            {
                8, new EnemyStaticData
                {
                    enemyId = 8,
                    enemyName = "蛛魔幼体",
                    enemyType = EnemyType.Normal,
                    element = Element.Dark,
                    baseHP = 500,
                    baseAttack = 70,
                    baseDefense = 25,
                    baseSpeed = 16,
                    baseExp = 40,
                    baseGold = 60,
                    skillIds = new int[] { 1001, 1009 },
                    aiType = EnemyAIType.Aggressive,
                    description = "黑暗城堡外围的蜘蛛怪物"
                }
            },
            {
                9, new EnemyStaticData
                {
                    enemyId = 9,
                    enemyName = "蛛魔王",
                    enemyType = EnemyType.Elite,
                    element = Element.Dark,
                    baseHP = 1500,
                    baseAttack = 100,
                    baseDefense = 45,
                    baseSpeed = 18,
                    baseExp = 120,
                    baseGold = 180,
                    skillIds = new int[] { 1001, 1009, 1010 },
                    aiType = EnemyAIType.Smart,
                    description = "蛛魔族群的统治者，剧毒无比"
                }
            },
            // 蛛魔骑士系 (ID: 10, 11)
            {
                10, new EnemyStaticData
                {
                    enemyId = 10,
                    enemyName = "蛛魔骑士",
                    enemyType = EnemyType.Normal,
                    element = Element.Dark,
                    baseHP = 800,
                    baseAttack = 90,
                    baseDefense = 50,
                    baseSpeed = 13,
                    baseExp = 55,
                    baseGold = 80,
                    skillIds = new int[] { 1001, 1011 },
                    aiType = EnemyAIType.Aggressive,
                    description = "骑着蛛魔的精锐战士"
                }
            },
            {
                11, new EnemyStaticData
                {
                    enemyId = 11,
                    enemyName = "蛛魔骑队长",
                    enemyType = EnemyType.Elite,
                    element = Element.Dark,
                    baseHP = 2000,
                    baseAttack = 130,
                    baseDefense = 70,
                    baseSpeed = 15,
                    baseExp = 150,
                    baseGold = 250,
                    skillIds = new int[] { 1001, 1011, 1012 },
                    aiType = EnemyAIType.Smart,
                    description = "蛛魔骑士团的首领"
                }
            },
            // 死灵法师系 (ID: 12, 13)
            {
                12, new EnemyStaticData
                {
                    enemyId = 12,
                    enemyName = "死灵蛛魔法师",
                    enemyType = EnemyType.Normal,
                    element = Element.Dark,
                    baseHP = 600,
                    baseAttack = 110,
                    baseDefense = 30,
                    baseSpeed = 11,
                    baseExp = 70,
                    baseGold = 100,
                    skillIds = new int[] { 1001, 1013, 1014 },
                    aiType = EnemyAIType.Smart,
                    description = "使用暗系魔法的死灵法师"
                }
            },
            {
                13, new EnemyStaticData
                {
                    enemyId = 13,
                    enemyName = "蛛魔大法师",
                    enemyType = EnemyType.Elite,
                    element = Element.Dark,
                    baseHP = 2500,
                    baseAttack = 150,
                    baseDefense = 50,
                    baseSpeed = 14,
                    baseExp = 200,
                    baseGold = 350,
                    skillIds = new int[] { 1001, 1013, 1014, 1015 },
                    aiType = EnemyAIType.Smart,
                    description = "蛛魔族的首席大法师"
                }
            },
            // 黑暗骑士系 (ID: 14, 15)
            {
                14, new EnemyStaticData
                {
                    enemyId = 14,
                    enemyName = "黑暗骑士",
                    enemyType = EnemyType.Normal,
                    element = Element.Dark,
                    baseHP = 1200,
                    baseAttack = 140,
                    baseDefense = 80,
                    baseSpeed = 12,
                    baseExp = 100,
                    baseGold = 150,
                    skillIds = new int[] { 1001, 1016, 1017 },
                    aiType = EnemyAIType.Aggressive,
                    description = "效忠于黑暗领主的骑士"
                }
            },
            {
                15, new EnemyStaticData
                {
                    enemyId = 15,
                    enemyName = "死亡骑士长",
                    enemyType = EnemyType.Boss,
                    element = Element.Dark,
                    baseHP = 5000,
                    baseAttack = 180,
                    baseDefense = 100,
                    baseSpeed = 14,
                    baseExp = 500,
                    baseGold = 1000,
                    skillIds = new int[] { 1001, 1016, 1017, 1018, 1019 },
                    aiType = EnemyAIType.Smart,
                    description = "黑暗骑士团的最高指挥官"
                }
            }
        };

        /// <summary>
        /// 获取默认敌人数据
        /// </summary>
        private static EnemyStaticData GetDefaultEnemy(int enemyId)
        {
            return new EnemyStaticData
            {
                enemyId = enemyId,
                enemyName = $"未知敌人_{enemyId}",
                enemyType = EnemyType.Normal,
                element = Element.None,
                baseHP = 100 * enemyId,
                baseAttack = 10 * enemyId,
                baseDefense = 5 * enemyId,
                baseSpeed = 10,
                baseExp = 10 * enemyId,
                baseGold = 15 * enemyId,
                skillIds = new int[] { 1001 },
                aiType = EnemyAIType.Random
            };
        }
    }

    /// <summary>
    /// 敌人静态数据
    /// </summary>
    [System.Serializable]
    public class EnemyStaticData
    {
        /// <summary>
        /// 敌人ID
        /// </summary>
        public int enemyId;

        /// <summary>
        /// 敌人名称
        /// </summary>
        public string enemyName;

        /// <summary>
        /// 敌人类型
        /// </summary>
        public EnemyType enemyType;

        /// <summary>
        /// 元素属性
        /// </summary>
        public Element element;

        /// <summary>
        /// 基础生命值
        /// </summary>
        public int baseHP;

        /// <summary>
        /// 基础攻击力
        /// </summary>
        public int baseAttack;

        /// <summary>
        /// 基础防御力
        /// </summary>
        public int baseDefense;

        /// <summary>
        /// 基础速度
        /// </summary>
        public int baseSpeed;

        /// <summary>
        /// 基础经验值
        /// </summary>
        public int baseExp;

        /// <summary>
        /// 基础金币
        /// </summary>
        public int baseGold;

        /// <summary>
        /// 技能ID列表
        /// </summary>
        public int[] skillIds;

        /// <summary>
        /// AI类型
        /// </summary>
        public EnemyAIType aiType;

        /// <summary>
        /// 描述
        /// </summary>
        public string description;

        /// <summary>
        /// 获取等级缩放后的数据
        /// </summary>
        public EnemyLevelData GetScaledData(int level)
        {
            float scale = 1f + (level - 1) * 0.1f;
            return new EnemyLevelData
            {
                hp = Mathf.RoundToInt(baseHP * scale),
                attack = Mathf.RoundToInt(baseAttack * scale),
                defense = Mathf.RoundToInt(baseDefense * scale),
                speed = Mathf.RoundToInt(baseSpeed * scale),
                exp = Mathf.RoundToInt(baseExp * scale),
                gold = Mathf.RoundToInt(baseGold * scale)
            };
        }
    }

    /// <summary>
    /// 敌人等级缩放数据
    /// </summary>
    [System.Serializable]
    public class EnemyLevelData
    {
        public int hp;
        public int attack;
        public int defense;
        public int speed;
        public int exp;
        public int gold;
    }
}
