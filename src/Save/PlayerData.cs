// PlayerData.cs - 玩家存档数据结构
// 定义玩家数据的完整结构

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Resources;
using JRPG.Gacha;

namespace JRPG.Save
{
    /// <summary>
    /// 玩家存档数据根类
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [Header("基础信息")]
        [Tooltip("玩家ID")]
        public string playerId;
        
        [Tooltip("玩家名称")]
        public string playerName;
        
        [Tooltip("创建时间")]
        public long createTime;
        
        [Tooltip("最后登录时间")]
        public long lastLoginTime;
        
        [Tooltip("游戏版本")]
        public string gameVersion;
        
        [Header("进度数据")]
        [Tooltip("玩家等级")]
        public int playerLevel = 1;
        
        [Tooltip("玩家经验")]
        public long playerExp = 0;
        
        [Tooltip("章节进度")]
        public int chapterProgress = 1;
        
        [Tooltip("关卡进度")]
        public int stageProgress = 1;
        
        [Header("角色数据")]
        [Tooltip("角色实例列表")]
        public List<CharacterInstanceData> characters = new List<CharacterInstanceData>();
        
        [Tooltip("当前队伍")]
        public string[] currentParty = new string[4]; // 最多4人队伍
        
        [Tooltip("已拥有的角色ID列表")]
        public List<int> ownedCharacterIds = new List<int>();
        
        [Header("资源数据")]
        [Tooltip("货币数据")]
        public CurrencySaveData currencyData;
        
        [Tooltip("背包数据")]
        public InventorySaveData inventoryData;
        
        [Header("抽卡数据")]
        [Tooltip("抽卡历史")]
        public GachaHistory gachaHistory;
        
        [Tooltip("抽卡保底数据")]
        public PitySaveData pityData;
        
        [Tooltip("已抽取的SSR列表")]
        public List<int> pulledSSRIds = new List<int>();
        
        [Header("装备数据")]
        [Tooltip("装备实例列表")]
        public List<EquipmentInstanceData> equipmentList = new List<EquipmentInstanceData>();
        
        [Header("活动数据")]
        [Tooltip("已完成的活动ID列表")]
        public List<string> completedEventIds = new List<string>();
        
        [Tooltip("签到天数")]
        public int signInDays = 0;
        
        [Tooltip("上次签到日期")]
        public string lastSignInDate;
        
        [Header("设置数据")]
        [Tooltip("音乐音量")]
        public float musicVolume = 1f;
        
        [Tooltip("音效音量")]
        public float sfxVolume = 1f;
        
        [Tooltip("语言设置")]
        public string language = "zh-CN";
        
        [Header("其他数据")]
        [Tooltip("成就进度")]
        public Dictionary<string, int> achievementProgress = new Dictionary<string, int>();
        
        [Tooltip("已领取的成就奖励")]
        public List<string> claimedAchievementIds = new List<string>();
        
        /// <summary>
        /// 创建一个新的玩家数据
        /// </summary>
        public static PlayerData CreateNew(string playerName)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            return new PlayerData
            {
                playerId = Guid.NewGuid().ToString(),
                playerName = playerName,
                createTime = now,
                lastLoginTime = now,
                gameVersion = Application.version,
                playerLevel = 1,
                playerExp = 0,
                chapterProgress = 1,
                stageProgress = 1,
                characters = new List<CharacterInstanceData>(),
                currentParty = new string[4],
                ownedCharacterIds = new List<int>(),
                completedEventIds = new List<string>(),
                signInDays = 0,
                lastSignInDate = "",
                currencyData = new CurrencySaveData
                {
                    currencies = new SerializableDictionary<CurrencyType, CurrencyData>(),
                    lastStaminaRecoveryTime = now
                },
                inventoryData = new InventorySaveData
                {
                    inventory = new List<ItemInstance>(),
                    capacity = 500
                },
                gachaHistory = new GachaHistory(),
                pityData = new PitySaveData
                {
                    pityStates = new Dictionary<string, PitySystem.PityState>(),
                    globalPityState = new PitySystem.PityState()
                },
                pulledSSRIds = new List<int>(),
                equipmentList = new List<EquipmentInstanceData>()
            };
        }
        
        /// <summary>
        /// 更新登录时间
        /// </summary>
        public void UpdateLoginTime()
        {
            lastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        /// <summary>
        /// 检查是否是新的一天
        /// </summary>
        public bool IsNewDay()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            return lastSignInDate != today;
        }
        
        /// <summary>
        /// 签到
        /// </summary>
        public bool SignIn()
        {
            if (!IsNewDay())
                return false;
            
            signInDays++;
            lastSignInDate = DateTime.Now.ToString("yyyy-MM-dd");
            return true;
        }
        
        /// <summary>
        /// 获取存档大小估算
        /// </summary>
        public int GetSaveSize()
        {
            string json = JsonUtility.ToJson(this);
            return json.Length;
        }
    }
    
    /// <summary>
    /// 存档列表项
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        [Tooltip("槽位索引")]
        public int slotIndex;
        
        [Tooltip("是否有存档")]
        public bool hasSave = false;
        
        [Tooltip("玩家名称")]
        public string playerName;
        
        [Tooltip("玩家等级")]
        public int playerLevel;
        
        [Tooltip("存档时间")]
        public long saveTime;
        
        [Tooltip("存档版本")]
        public string gameVersion;
        
        [Tooltip("缩略图路径")]
        public string thumbnailPath;
        
        /// <summary>
        /// 获取存档时间格式化字符串
        /// </summary>
        public string GetSaveTimeString()
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(saveTime);
            return dto.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    /// <summary>
    /// 存档列表
    /// </summary>
    [Serializable]
    public class SaveSlotList
    {
        [Tooltip("存档槽位列表")]
        public SaveSlotInfo[] slots = new SaveSlotInfo[3]; // 3个存档槽位
        
        [Tooltip("上次使用的槽位")]
        public int lastUsedSlot = 0;
    }
}
