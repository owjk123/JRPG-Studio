using UnityEngine;
using System.Collections.Generic;

namespace JRPG.Quest
{
    /// <summary>
    /// 任务类型
    /// </summary>
    public enum QuestType
    {
        Main,           // 主线任务
        Side,           // 支线任务
        Daily,          // 每日任务
        Weekly,         // 每周任务
        Achievement,    // 成就
        Event           // 活动任务
    }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum QuestState
    {
        Locked,         // 未解锁
        Available,      // 可接取
        InProgress,     // 进行中
        Completed,      // 已完成
        Claimed         // 已领取奖励
    }
    
    /// <summary>
    /// 任务目标类型
    /// </summary>
    public enum ObjectiveType
    {
        KillEnemy,          // 击杀敌人
        KillEnemyType,      // 击杀特定类型敌人
        ClearStage,         // 通关关卡
        ClearStageCount,    // 通关关卡次数
        CollectItem,        // 收集道具
        UseItem,            // 使用道具
        EnhanceCharacter,   // 强化角色
        EnhanceEquipment,   // 强化装备
        GachaPull,          // 抽卡
        GachaSRCount,       // 抽到SR数量
        GachaSSRCount,      // 抽到SSR数量
        WinBattle,          // 战斗胜利
        WinBattleCount,     // 战斗胜利次数
        Login,              // 登录
        LoginDays,          // 累计登录天数
        SpendGold,          // 消耗金币
        SpendGems,          // 消耗钻石
        CompleteQuest       // 完成任务
    }
    
    /// <summary>
    /// 任务目标
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string targetId;         // 目标ID
        public int requiredAmount;      // 需要数量
        public int currentAmount;       // 当前进度
        
        public bool IsCompleted => currentAmount >= requiredAmount;
        public float Progress => (float)currentAmount / requiredAmount;
    }
    
    /// <summary>
    /// 任务数据
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "JRPG/Quest/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("基本信息")]
        public string questId;
        public string questName;
        [TextArea(3, 5)] public string description;
        public QuestType questType;
        public Sprite icon;
        
        [Header("前置条件")]
        public string prerequisiteQuestId;  // 前置任务ID
        public int requiredPlayerLevel;     // 需要玩家等级
        public int requiredChapter;         // 需要通关章节
        
        [Header("任务目标")]
        public QuestObjective[] objectives;
        
        [Header("奖励")]
        public QuestReward[] rewards;
        
        [Header("时间限制")]
        public bool hasTimeLimit;
        public int durationDays;            // 任务持续天数
        
        [Header("其他")]
        public bool autoAccept;             // 自动接取
        public bool autoComplete;           // 自动完成
        public int sortPriority;
    }
    
    /// <summary>
    /// 任务奖励
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        public RewardType type;
        public string rewardId;
        public int amount;
    }
    
    /// <summary>
    /// 奖励类型
    /// </summary>
    public enum RewardType
    {
        Gold,
        Gems,
        Stamina,
        Item,
        Character,
        CharacterFragment,
        Equipment
    }
    
    /// <summary>
    /// 运行时任务实例
    /// </summary>
    public class QuestInstance
    {
        public string questId;
        public QuestState state;
        public QuestObjective[] objectives;
        public System.DateTime acceptTime;
        public System.DateTime? completeTime;
        
        public bool IsCompleted
        {
            get
            {
                foreach (var obj in objectives)
                {
                    if (!obj.IsCompleted) return false;
                }
                return true;
            }
        }
    }
    
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [Header("任务配置")]
        [SerializeField] private QuestData[] allQuests;
        
        private Dictionary<string, QuestInstance> activeQuests = new Dictionary<string, QuestInstance>();
        private HashSet<string> completedQuests = new HashSet<string>();
        private HashSet<string> claimedQuests = new HashSet<string>();
        
        /// <summary>
        /// 任务状态变化事件
        /// </summary>
        public event System.Action<string, QuestState> OnQuestStateChanged;
        
        /// <summary>
        /// 任务进度更新事件
        /// </summary>
        public event System.Action<string, QuestObjective> OnObjectiveProgress;
        
        /// <summary>
        /// 获取可接取的任务
        /// </summary>
        public List<QuestData> GetAvailableQuests()
        {
            var result = new List<QuestData>();
            foreach (var quest in allQuests)
            {
                if (CanAcceptQuest(quest.questId))
                {
                    result.Add(quest);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 获取进行中的任务
        /// </summary>
        public List<QuestInstance> GetActiveQuests()
        {
            var result = new List<QuestInstance>();
            foreach (var kvp in activeQuests)
            {
                result.Add(kvp.Value);
            }
            return result;
        }
        
        /// <summary>
        /// 检查是否可以接取任务
        /// </summary>
        public bool CanAcceptQuest(string questId)
        {
            // 已完成或已接取的任务不能重复接取
            if (completedQuests.Contains(questId) || activeQuests.ContainsKey(questId))
            {
                return false;
            }
            
            // 检查前置任务
            var quest = GetQuestData(questId);
            if (quest == null) return false;
            
            if (!string.IsNullOrEmpty(quest.prerequisiteQuestId))
            {
                if (!completedQuests.Contains(quest.prerequisiteQuestId))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 接取任务
        /// </summary>
        public bool AcceptQuest(string questId)
        {
            if (!CanAcceptQuest(questId))
            {
                Debug.LogWarning($"[QuestManager] 无法接取任务: {questId}");
                return false;
            }
            
            var quest = GetQuestData(questId);
            if (quest == null) return false;
            
            // 创建任务实例
            var instance = new QuestInstance
            {
                questId = questId,
                state = QuestState.InProgress,
                acceptTime = System.DateTime.Now,
                objectives = new QuestObjective[quest.objectives.Length]
            };
            
            // 复制目标
            for (int i = 0; i < quest.objectives.Length; i++)
            {
                instance.objectives[i] = new QuestObjective
                {
                    type = quest.objectives[i].type,
                    targetId = quest.objectives[i].targetId,
                    requiredAmount = quest.objectives[i].requiredAmount,
                    currentAmount = 0
                };
            }
            
            activeQuests[questId] = instance;
            OnQuestStateChanged?.Invoke(questId, QuestState.InProgress);
            
            Debug.Log($"[QuestManager] 接取任务: {quest.questName}");
            return true;
        }
        
        /// <summary>
        /// 更新任务进度
        /// </summary>
        public void UpdateProgress(ObjectiveType type, string targetId, int amount = 1)
        {
            foreach (var kvp in activeQuests)
            {
                var instance = kvp.Value;
                if (instance.state != QuestState.InProgress) continue;
                
                for (int i = 0; i < instance.objectives.Length; i++)
                {
                    var obj = instance.objectives[i];
                    if (obj.type == type && (string.IsNullOrEmpty(obj.targetId) || obj.targetId == targetId))
                    {
                        obj.currentAmount = Mathf.Min(obj.currentAmount + amount, obj.requiredAmount);
                        OnObjectiveProgress?.Invoke(kvp.Key, obj);
                        
                        Debug.Log($"[QuestManager] 任务进度更新: {kvp.Key} - {obj.type} ({obj.currentAmount}/{obj.requiredAmount})");
                    }
                }
                
                // 检查是否完成
                if (instance.IsCompleted)
                {
                    CompleteQuest(kvp.Key);
                }
            }
        }
        
        /// <summary>
        /// 完成任务
        /// </summary>
        private void CompleteQuest(string questId)
        {
            if (!activeQuests.TryGetValue(questId, out var instance)) return;
            
            instance.state = QuestState.Completed;
            instance.completeTime = System.DateTime.Now;
            
            completedQuests.Add(questId);
            OnQuestStateChanged?.Invoke(questId, QuestState.Completed);
            
            Debug.Log($"[QuestManager] 任务完成: {questId}");
        }
        
        /// <summary>
        /// 领取奖励
        /// </summary>
        public bool ClaimReward(string questId)
        {
            if (!activeQuests.TryGetValue(questId, out var instance)) return false;
            if (instance.state != QuestState.Completed) return false;
            
            var quest = GetQuestData(questId);
            if (quest == null) return false;
            
            // 发放奖励
            foreach (var reward in quest.rewards)
            {
                GrantReward(reward);
            }
            
            // 更新状态
            instance.state = QuestState.Claimed;
            claimedQuests.Add(questId);
            activeQuests.Remove(questId);
            
            OnQuestStateChanged?.Invoke(questId, QuestState.Claimed);
            
            Debug.Log($"[QuestManager] 领取奖励: {quest.questName}");
            return true;
        }
        
        /// <summary>
        /// 获取任务数据
        /// </summary>
        private QuestData GetQuestData(string questId)
        {
            foreach (var quest in allQuests)
            {
                if (quest.questId == questId) return quest;
            }
            return null;
        }
        
        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GrantReward(QuestReward reward)
        {
            // 实际实现需要调用对应的管理器
            Debug.Log($"[QuestManager] 发放奖励: {reward.type} x{reward.amount}");
        }
        
        #region 单例
        
        private static QuestManager _instance;
        public static QuestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<QuestManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[QuestManager]");
                        _instance = go.AddComponent<QuestManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        #endregion
    }
}
