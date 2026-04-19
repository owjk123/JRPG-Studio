// NewPlayerTemplate.cs - 新玩家初始存档模板
// 定义新玩家创建时的默认数据配置

using UnityEngine;
using System;
using System.Collections.Generic;

namespace JRPG.Player
{
    /// <summary>
    /// 玩家数据模板
    /// 新建角色时使用此模板初始化玩家数据
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlayerTemplate", menuName = "JRPG/Player/New Player Template")]
    public class NewPlayerTemplate : ScriptableObject
    {
        [Header("玩家基础信息")]
        /// <summary>
        /// 默认玩家名称
        /// </summary>
        public string defaultPlayerName = "冒险者";

        /// <summary>
        /// 默认头像ID
        /// </summary>
        public int defaultAvatarId = 1;

        [Header("初始货币")]
        /// <summary>
        /// 初始金币数量
        /// </summary>
        public int initialGold = 10000;

        /// <summary>
        /// 初始钻石数量
        /// </summary>
        public int initialDiamond = 500;

        /// <summary>
        /// 初始竞技场币
        /// </summary>
        public int initialArenaCoin = 0;

        /// <summary>
        /// 初始公会币
        /// </summary>
        public int initialGuildCoin = 0;

        [Header("初始体力")]
        /// <summary>
        /// 初始体力值
        /// </summary>
        public int initialStamina = 100;

        /// <summary>
        /// 体力上限
        /// </summary>
        public int maxStamina = 100;

        /// <summary>
        /// 体力恢复间隔（秒）
        /// </summary>
        public int staminaRecoveryInterval = 300; // 5分钟

        /// <summary>
        /// 每次恢复体力值
        /// </summary>
        public int staminaRecoveryAmount = 1;

        [Header("初始角色")]
        /// <summary>
        /// 默认赠送的角色ID列表
        /// </summary>
        public int[] initialCharacterIds = new int[] { 1001 }; // 默认人族骑士

        /// <summary>
        /// 初始角色等级
        /// </summary>
        public int initialCharacterLevel = 1;

        /// <summary>
        /// 初始角色星级
        /// </summary>
        public int initialCharacterStars = 1;

        [Header("初始背包")]
        /// <summary>
        /// 初始物品ID列表
        /// </summary>
        public int[] initialItemIds = new int[] { 401, 401, 401, 401, 401 }; // 5个小经验药水

        /// <summary>
        /// 初始物品数量列表
        /// </summary>
        public int[] initialItemCounts = new int[] { 10, 10, 10, 10, 10 };

        [Header("初始装备")]
        /// <summary>
        /// 是否赠送初始装备
        /// </summary>
        public bool giveInitialEquipment = true;

        /// <summary>
        /// 初始装备ID列表
        /// </summary>
        public int[] initialEquipmentIds = new int[] { 1001, 2001, 3001 }; // 武器、防具、饰品

        [Header("教程进度")]
        /// <summary>
        /// 初始教程步骤ID
        /// </summary>
        public int initialTutorialStep = 1;

        /// <summary>
        /// 是否跳过新手教程
        /// </summary>
        public bool skipTutorial = false;

        [Header("系统进度")]
        /// <summary>
        /// 初始章节ID
        /// </summary>
        public int initialChapterId = 1;

        /// <summary>
        /// 初始关卡ID
        /// </summary>
        public int initialStageId = 1;

        /// <summary>
        /// 已解锁的关卡列表
        /// </summary>
        public int[] unlockedStageIds = new int[] { 10101 }; // 第一章第一关

        [Header("游戏设置默认值")]
        /// <summary>
        /// 自动战斗默认开关
        /// </summary>
        public bool defaultAutoBattle = false;

        /// <summary>
        /// 战斗速度默认
        /// </summary>
        [Range(1f, 2f)]
        public float defaultBattleSpeed = 1f;

        /// <summary>
        /// 音效音量默认
        /// </summary>
        [Range(0f, 1f)]
        public float defaultSfxVolume = 0.8f;

        /// <summary>
        /// 音乐音量默认
        /// </summary>
        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.6f;

        /// <summary>
        /// 通知开关默认
        /// </summary>
        public bool defaultNotifications = true;

        [Header("签到配置")]
        /// <summary>
        /// 累计签到天数（用于七日签到）
        /// </summary>
        public int totalSignInDays = 0;

        /// <summary>
        /// 最后签到日期
        /// </summary>
        public string lastSignInDate = "";

        /// <summary>
        /// 连续签到天数
        /// </summary>
        public int consecutiveSignInDays = 0;

        [Header("首充配置")]
        /// <summary>
        /// 是否已领取首充奖励
        /// </summary>
        public bool firstPurchaseClaimed = false;

        /// <summary>
        /// 累计充值金额（分）
        /// </summary>
        public int totalRechargeAmount = 0;

        [Header("限时活动")]
        /// <summary>
        /// 已领取的限时奖励ID列表
        /// </summary>
        public int[] claimedLimitedRewards = new int[0];

        /// <summary>
        /// 限时活动完成进度
        /// </summary>
        public Dictionary<string, int> eventProgress = new Dictionary<string, int>();

        /// <summary>
        /// 创建新玩家数据
        /// </summary>
        public PlayerData CreateNewPlayerData()
        {
            PlayerData data = new PlayerData
            {
                playerName = defaultPlayerName,
                avatarId = defaultAvatarId,
                
                // 货币
                gold = initialGold,
                diamond = initialDiamond,
                arenaCoin = initialArenaCoin,
                guildCoin = initialGuildCoin,
                
                // 体力
                stamina = initialStamina,
                maxStamina = maxStamina,
                staminaRecoveryTime = DateTime.Now,
                
                // 教程
                tutorialStep = initialTutorialStep,
                tutorialCompleted = false,
                
                // 章节进度
                currentChapterId = initialChapterId,
                currentStageId = initialStageId,
                unlockedStages = new List<int>(unlockedStageIds),
                
                // 设置
                autoBattleEnabled = defaultAutoBattle,
                battleSpeed = defaultBattleSpeed,
                sfxVolume = defaultSfxVolume,
                musicVolume = defaultMusicVolume,
                notificationsEnabled = defaultNotifications,
                
                // 签到
                totalSignInDays = totalSignInDays,
                lastSignInDate = lastSignInDate,
                consecutiveSignInDays = consecutiveSignInDays,
                
                // 首充
                firstPurchaseClaimed = firstPurchaseClaimed,
                totalRechargeAmount = totalRechargeAmount
            };
            
            return data;
        }
    }

    /// <summary>
    /// 玩家数据（存储用）
    /// 包含玩家所有的游戏进度数据
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [Header("基础信息")]
        public string playerName;
        public int avatarId;
        public DateTime createTime;
        public DateTime lastLoginTime;

        [Header("货币")]
        public int gold;
        public int diamond;
        public int arenaCoin;
        public int guildCoin;

        [Header("体力系统")]
        public int stamina;
        public int maxStamina;
        public DateTime staminaRecoveryTime;

        [Header("角色数据")]
        public List<CharacterSaveData> characters = new List<CharacterSaveData>();
        public List<int> ownedCharacterIds = new List<int>();

        [Header("背包")]
        public List<ItemSaveData> inventory = new List<ItemSaveData>();

        [Header("装备")]
        public List<EquipmentSaveData> equipment = new List<EquipmentSaveData>();

        [Header("教程")]
        public int tutorialStep;
        public bool tutorialCompleted;

        [Header("章节进度")]
        public int currentChapterId;
        public int currentStageId;
        public List<int> unlockedStages;
        public List<int> completedStages;
        public Dictionary<int, int> stageStars; // 关卡星级

        [Header("战斗记录")]
        public int totalBattles;
        public int victories;
        public int defeats;

        [Header("设置")]
        public bool autoBattleEnabled;
        public float battleSpeed;
        public float sfxVolume;
        public float musicVolume;
        public bool notificationsEnabled;

        [Header("社交")]
        public string guildId;
        public string guildName;
        public List<string> friends;

        [Header("签到")]
        public int totalSignInDays;
        public string lastSignInDate;
        public int consecutiveSignInDays;
        public List<string> signInHistory;

        [Header("充值")]
        public bool firstPurchaseClaimed;
        public int totalRechargeAmount;
        public List<PurchaseRecord> purchaseHistory;

        [Header("成就")]
        public List<string> achievedAchievements;

        [Header("其他")]
        public int level;
        public int exp;
        public int vipLevel;
    }

    /// <summary>
    /// 角色存档数据
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        public int characterId;
        public int level;
        public int exp;
        public int stars;
        public int breakthroughLevel;
        public List<int> ownedSkillIds;
        public List<int> equippedSkillIds;
        public List<int> unlockedPassiveSkillIds;
        public int currentExp;
        public int currentHp;
    }

    /// <summary>
    /// 物品存档数据
    /// </summary>
    [Serializable]
    public class ItemSaveData
    {
        public int itemId;
        public int count;
        public bool isNew;
    }

    /// <summary>
    /// 装备存档数据
    /// </summary>
    [Serializable]
    public class EquipmentSaveData
    {
        public int equipmentId;
        public int level;
        public int refinementLevel;
        public int equippedCharacterId;
        public bool isLocked;
    }

    /// <summary>
    /// 充值记录
    /// </summary>
    [Serializable]
    public class PurchaseRecord
    {
        public string orderId;
        public int productId;
        public int amount;
        public DateTime purchaseTime;
    }
}
