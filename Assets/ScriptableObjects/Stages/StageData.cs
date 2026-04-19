// StageData.cs - 关卡数据定义
// 定义游戏关卡的结构、敌人配置、奖励等

using UnityEngine;
using System;
using System.Collections.Generic;

namespace JRPG.Stage
{
    /// <summary>
    /// 关卡类型
    /// </summary>
    public enum StageType
    {
        Normal,              // 普通关卡
        Elite,               // 精英关卡
        Boss,                // BOSS关卡
        Story,               // 剧情关卡
        Daily,               // 每日关卡
        Event,               // 活动关卡
        Tower,               // 爬塔
        Arena                // 竞技场
    }

    /// <summary>
    /// 关卡难度
    /// </summary>
    public enum StageDifficulty
    {
        Easy,                // 简单
        Normal,              // 普通
        Hard,                // 困难
        Nightmare,           // 噩梦
        Hell                 // 地狱
    }

    /// <summary>
    /// 关卡通关条件类型
    /// </summary>
    public enum ClearConditionType
    {
        DefeatAll,           // 击败所有敌人
        DefeatBoss,          // 击败BOSS
        Survive,             // 存活一定回合
        DefeatWithinTurns,   // 在指定回合内击败
        NoUnitLost,          // 不损失单位
        UseSkill             // 使用指定技能
    }

    /// <summary>
    /// 通关条件
    /// </summary>
    [Serializable]
    public class ClearCondition
    {
        /// <summary>
        /// 条件类型
        /// </summary>
        public ClearConditionType type;

        /// <summary>
        /// 参数值（如回合数、敌人ID等）
        /// </summary>
        public int param1;

        /// <summary>
        /// 额外参数
        /// </summary>
        public int param2;

        /// <summary>
        /// 条件描述
        /// </summary>
        public string description;
    }

    /// <summary>
    /// 关卡星级的达成条件
    /// </summary>
    [Serializable]
    public class StarCondition
    {
        /// <summary>
        /// 星星数量（1-3）
        /// </summary>
        public int stars = 1;

        /// <summary>
        /// 达成条件
        /// </summary>
        public ClearCondition condition;

        /// <summary>
        /// 是否达成
        /// </summary>
        public bool achieved;
    }

    /// <summary>
    /// 章节数据
    /// 包含多个关卡
    /// </summary>
    [Serializable]
    public class ChapterData
    {
        /// <summary>
        /// 章节ID
        /// </summary>
        public int chapterId;

        /// <summary>
        /// 章节名称
        /// </summary>
        public string chapterName;

        /// <summary>
        /// 章节副标题
        /// </summary>
        public string subtitle;

        /// <summary>
        /// 章节描述
        /// </summary>
        [TextArea(2, 4)]
        public string description;

        /// <summary>
        /// 章节图标
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// 章节背景
        /// </summary>
        public string backgroundPath;

        /// <summary>
        /// 章节背景音乐
        /// </summary>
        public string bgmPath;

        /// <summary>
        /// 章节等级要求
        /// </summary>
        public int requiredPlayerLevel = 1;

        /// <summary>
        /// 章节总关卡数
        /// </summary>
        public int totalStages;

        /// <summary>
        /// 通关奖励
        /// </summary>
        public ChapterReward reward;

        /// <summary>
        /// 是否解锁
        /// </summary>
        public bool unlocked;

        /// <summary>
        /// 是否已通关
        /// </summary>
        public bool cleared;
    }

    /// <summary>
    /// 章节奖励
    /// </summary>
    [Serializable]
    public class ChapterReward
    {
        /// <summary>
        /// 金币奖励
        /// </summary>
        public int goldReward;

        /// <summary>
        /// 经验奖励
        /// </summary>
        public int expReward;

        /// <summary>
        /// 物品奖励ID列表
        /// </summary>
        public int[] itemRewardIds;

        /// <summary>
        /// 物品奖励数量
        /// </summary>
        public int[] itemRewardCounts;

        /// <summary>
        /// 是否已领取
        /// </summary>
        public bool claimed;
    }

    /// <summary>
    /// 关卡数据
    /// 定义单个关卡的所有配置
    /// </summary>
    [CreateAssetMenu(fileName = "StageData", menuName = "JRPG/Stage/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("基础信息")]
        /// <summary>
        /// 关卡ID（格式：章节*100+关序号，如10101表示第1章第1关）
        /// </summary>
        public int stageId;

        /// <summary>
        /// 关卡名称
        /// </summary>
        public string stageName;

        /// <summary>
        /// 所属章节
        /// </summary>
        public int chapterId;

        /// <summary>
        /// 关卡序号（在章节内的顺序）
        /// </summary>
        public int stageIndex;

        /// <summary>
        /// 关卡类型
        /// </summary>
        public StageType stageType;

        /// <summary>
        /// 难度等级
        /// </summary>
        public StageDifficulty difficulty = StageDifficulty.Normal;

        [Header("关卡配置")]
        /// <summary>
        /// 推荐等级
        /// </summary>
        public int recommendedLevel = 1;

        /// <summary>
        /// 推荐战力
        /// </summary>
        public int recommendedPower;

        /// <summary>
        /// 消耗体力
        /// </summary>
        public int staminaCost = 6;

        /// <summary>
        /// 每日挑战次数限制（-1表示不限制）
        /// </summary>
        public int dailyChallengeLimit = -1;

        [Header("战斗配置")]
        /// <summary>
        /// 敌人配置列表
        /// </summary>
        public List<StageEnemyConfig> enemies = new List<StageEnemyConfig>();

        /// <summary>
        /// 战斗遭遇数据引用
        /// </summary>
        public string encounterDataPath;

        [Header("通关条件")]
        /// <summary>
        /// 通关条件
        /// </summary>
        public ClearCondition[] clearConditions;

        /// <summary>
        /// 星级条件
        /// </summary>
        public StarCondition[] starConditions = new StarCondition[3];

        [Header("奖励配置")]
        /// <summary>
        /// 通关基础奖励
        /// </summary>
        public StageReward baseReward;

        /// <summary>
        /// 首通奖励
        /// </summary>
        public StageReward firstClearReward;

        /// <summary>
        /// 完美通关奖励（3星）
        /// </summary>
        public StageReward perfectClearReward;

        /// <summary>
        /// 额外奖励（随机掉落）
        /// </summary>
        public List<DropConfig> bonusDrops = new List<DropConfig>();

        [Header("剧情配置")]
        /// <summary>
        /// 是否为剧情关卡
        /// </summary>
        public bool isStoryStage;

        /// <summary>
        /// 战斗前剧情ID
        /// </summary>
        public int preBattleDialogueId;

        /// <summary>
        /// 战斗后剧情ID
        /// </summary>
        public int postBattleDialogueId;

        /// <summary>
        /// 通关后剧情ID
        /// </summary>
        public int victoryDialogueId;

        [Header("环境配置")]
        /// <summary>
        /// 战斗场景路径
        /// </summary>
        public string battleScenePath;

        /// <summary>
        /// 战斗背景
        /// </summary>
        public string backgroundPath;

        /// <summary>
        /// 背景音乐
        /// </summary>
        public string bgmPath;

        [Header("其他配置")]
        /// <summary>
        /// 是否自动战斗
        /// </summary>
        public bool autoBattleEnabled = true;

        /// <summary>
        /// 是否可以使用道具
        /// </summary>
        public bool itemUsageEnabled = true;

        /// <summary>
        /// 是否可以使用伙伴助战
        /// </summary>
        public bool supportEnabled = true;

        /// <summary>
        /// 解锁下一关的ID（留空则自动根据序号计算）
        /// </summary>
        public int nextStageId;

        /// <summary>
        /// 解锁条件描述
        /// </summary>
        [TextArea(1, 3)]
        public string unlockHint;

        /// <summary>
        /// 获取通关条件描述
        /// </summary>
        public string GetClearConditionText()
        {
            if (clearConditions == null || clearConditions.Length == 0)
                return "击败所有敌人";

            string text = "";
            foreach (var condition in clearConditions)
            {
                if (!string.IsNullOrEmpty(condition.description))
                    text += condition.description + "\n";
            }
            return text.TrimEnd('\n');
        }

        /// <summary>
        /// 获取星级描述
        /// </summary>
        public string GetStarDescription(int stars)
        {
            if (starConditions == null || stars < 1 || stars > 3)
                return "";

            var condition = starConditions[stars - 1];
            return condition?.condition?.description ?? "";
        }
    }

    /// <summary>
    /// 关卡敌人配置
    /// </summary>
    [Serializable]
    public class StageEnemyConfig
    {
        /// <summary>
        /// 敌人数据ID
        /// </summary>
        public int enemyId;

        /// <summary>
        /// 敌人名称（冗余显示用）
        /// </summary>
        public string enemyName;

        /// <summary>
        /// 等级
        /// </summary>
        public int level = 1;

        /// <summary>
        /// 位置（x: 列, y: 行）
        /// </summary>
        public Vector2Int position;

        /// <summary>
        /// 是否为BOSS
        /// </summary>
        public bool isBoss;

        /// <summary>
        /// 是否在特定回合出现
        /// </summary>
        public int appearAtTurn = -1;

        /// <summary>
        /// 是否可被指定为目标
        /// </summary>
        public bool targetable = true;
    }

    /// <summary>
    /// 关卡奖励配置
    /// </summary>
    [Serializable]
    public class StageReward
    {
        /// <summary>
        /// 经验值
        /// </summary>
        public int exp = 100;

        /// <summary>
        /// 金币
        /// </summary>
        public int gold = 100;

        /// <summary>
        /// 物品奖励ID
        /// </summary>
        public int[] itemIds;

        /// <summary>
        /// 物品数量
        /// </summary>
        public int[] itemCounts;

        /// <summary>
        /// 物品掉落概率（百分比）
        /// </summary>
        public float[] itemDropRates;
    }

    /// <summary>
    /// 掉落配置
    /// </summary>
    [Serializable]
    public class DropConfig
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public int itemId;

        /// <summary>
        /// 物品名称
        /// </summary>
        public string itemName;

        /// <summary>
        /// 最小数量
        /// </summary>
        public int minCount = 1;

        /// <summary>
        /// 最大数量
        /// </summary>
        public int maxCount = 1;

        /// <summary>
        /// 掉落概率（0-100）
        /// </summary>
        [Range(0f, 100f)]
        public float dropRate = 50f;

        /// <summary>
        /// 是否必定掉落
        /// </summary>
        public bool guaranteed;
    }

    /// <summary>
    /// 关卡存档数据
    /// </summary>
    [Serializable]
    public class StageProgress
    {
        /// <summary>
        /// 关卡ID
        /// </summary>
        public int stageId;

        /// <summary>
        /// 是否已通关
        /// </summary>
        public bool cleared;

        /// <summary>
        /// 最高星级
        /// </summary>
        public int bestStars;

        /// <summary>
        /// 最快通关时间（秒）
        /// </summary>
        public float bestTime;

        /// <summary>
        /// 通关次数
        /// </summary>
        public int clearCount;

        /// <summary>
        /// 今日挑战次数
        /// </summary>
        public int todayChallengeCount;

        /// <summary>
        /// 是否已领取首通奖励
        /// </summary>
        public bool firstClearClaimed;

        /// <summary>
        /// 最高连击数
        /// </summary>
        public int bestCombo;
    }
}
