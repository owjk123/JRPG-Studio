// BattleLog.cs - 战斗日志组件
// 显示战斗过程中的事件记录

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// 战斗日志组件
    /// 显示战斗过程中的事件记录，支持滚动和分类显示
    /// </summary>
    public class BattleLog : MonoBehaviour
    {
        #region UI References
        
        [Header("日志列表")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private GameObject _logEntryPrefab;
        
        [Header("设置")]
        [SerializeField] private int _maxEntries = 50;           // 最大日志条数
        [SerializeField] private float _autoScrollDelay = 0.1f; // 自动滚动延迟
        [SerializeField] private bool _autoScroll = true;        // 是否自动滚动到底部
        
        [Header("分类筛选")]
        [SerializeField] private Toggle _showDamageToggle;
        [SerializeField] private Toggle _showHealToggle;
        [SerializeField] private Toggle _showStatusToggle;
        [SerializeField] private Toggle _showSystemToggle;
        
        #endregion
        
        #region Types
        
        /// <summary>
        /// 日志类型
        /// </summary>
        public enum LogType
        {
            Normal,     // 普通消息
            Damage,     // 伤害
            Heal,       // 治疗
            Critical,   // 暴击
            Miss,       // 闪避
            Status,     // 状态变化
            Buff,       // 增益
            Debuff,     // 减益
            System,     // 系统消息
            Victory,    // 胜利
            Defeat      // 失败
        }
        
        /// <summary>
        /// 日志条目
        /// </summary>
        [Serializable]
        public class LogEntry
        {
            public string Message;
            public LogType Type;
            public float Timestamp;
            public string SourceName;
            public string TargetName;
            public int? Value;
            
            public LogEntry(string message, LogType type)
            {
                Message = message;
                Type = type;
                Timestamp = Time.time;
            }
        }
        
        #endregion
        
        #region Private Fields
        
        private List<LogEntry> _entries = new List<LogEntry>();
        private List<LogEntry> _filteredEntries = new List<LogEntry>();
        private Dictionary<LogType, bool> _typeFilters = new Dictionary<LogType, bool>();
        private bool _isInitialized = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 所有日志条目
        /// </summary>
        public IReadOnlyList<LogEntry> Entries => _entries;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeFilters();
            _isInitialized = true;
        }
        
        private void Start()
        {
            SetupToggleListeners();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 添加日志条目
        /// </summary>
        public void AddEntry(string message, LogType type = LogType.Normal)
        {
            var entry = new LogEntry(message, type);
            _entries.Add(entry);
            
            // 限制条数
            while (_entries.Count > _maxEntries)
            {
                _entries.RemoveAt(0);
            }
            
            // 应用过滤
            if (ShouldShowEntry(entry))
            {
                _filteredEntries.Add(entry);
                CreateLogUI(entry);
            }
            
            // 自动滚动
            if (_autoScroll)
            {
                ScheduleAutoScroll();
            }
        }
        
        /// <summary>
        /// 添加伤害日志
        /// </summary>
        public void AddDamageLog(string attacker, string defender, int damage, bool isCritical = false)
        {
            var type = isCritical ? LogType.Critical : LogType.Damage;
            string message = isCritical 
                ? $"{attacker}对{defender}造成暴击伤害 {damage}！"
                : $"{attacker}对{defender}造成伤害 {damage}";
            
            AddEntry(message, type);
        }
        
        /// <summary>
        /// 添加治疗日志
        /// </summary>
        public void AddHealLog(string healer, string target, int healAmount)
        {
            string message = $"{healer}为{target}恢复 {healAmount} HP";
            AddEntry(message, LogType.Heal);
        }
        
        /// <summary>
        /// 添加状态日志
        /// </summary>
        public void AddStatusLog(string target, string statusName, bool isApplied)
        {
            string action = isApplied ? "获得" : "解除";
            string message = $"{target}{action}了 {statusName}";
            var type = isApplied ? LogType.Debuff : LogType.Status;
            AddEntry(message, type);
        }
        
        /// <summary>
        /// 添加系统日志
        /// </summary>
        public void AddSystemLog(string message)
        {
            AddEntry(message, LogType.System);
        }
        
        /// <summary>
        /// 添加战斗开始日志
        /// </summary>
        public void AddBattleStartLog(string playerTeam, string enemyTeam)
        {
            AddEntry($"=== 战斗开始 ===", LogType.System);
            AddEntry($"我方: {playerTeam}", LogType.System);
            AddEntry($"敌方: {enemyTeam}", LogType.System);
        }
        
        /// <summary>
        /// 添加胜利日志
        /// </summary>
        public void AddVictoryLog()
        {
            AddEntry($"=== 战斗胜利！===", LogType.Victory);
        }
        
        /// <summary>
        /// 添加失败日志
        /// </summary>
        public void AddDefeatLog()
        {
            AddEntry($"=== 战斗失败...===", LogType.Defeat);
        }
        
        /// <summary>
        /// 清空日志
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            _filteredEntries.Clear();
            
            // 清空UI
            if (_contentTransform != null)
            {
                foreach (Transform child in _contentTransform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            Clear();
            
            foreach (var entry in _entries)
            {
                if (ShouldShowEntry(entry))
                {
                    _filteredEntries.Add(entry);
                    CreateLogUI(entry);
                }
            }
        }
        
        /// <summary>
        /// 获取指定类型的最新日志
        /// </summary>
        public LogEntry GetLatestEntry(LogType type)
        {
            return _entries.LastOrDefault(e => e.Type == type);
        }
        
        /// <summary>
        /// 获取指定时间范围内的日志
        /// </summary>
        public List<LogEntry> GetEntriesInRange(float startTime, float endTime)
        {
            return _entries
                .Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime)
                .ToList();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 初始化过滤器设置
        /// </summary>
        private void InitializeFilters()
        {
            _typeFilters[LogType.Normal] = true;
            _typeFilters[LogType.Damage] = true;
            _typeFilters[LogType.Heal] = true;
            _typeFilters[LogType.Critical] = true;
            _typeFilters[LogType.Miss] = true;
            _typeFilters[LogType.Status] = true;
            _typeFilters[LogType.Buff] = true;
            _typeFilters[LogType.Debuff] = true;
            _typeFilters[LogType.System] = true;
            _typeFilters[LogType.Victory] = true;
            _typeFilters[LogType.Defeat] = true;
        }
        
        /// <summary>
        /// 设置切换监听
        /// </summary>
        private void SetupToggleListeners()
        {
            if (_showDamageToggle != null)
                _showDamageToggle.onValueChanged.AddListener(OnFilterChanged);
            
            if (_showHealToggle != null)
                _showHealToggle.onValueChanged.AddListener(OnFilterChanged);
            
            if (_showStatusToggle != null)
                _showStatusToggle.onValueChanged.AddListener(OnFilterChanged);
            
            if (_showSystemToggle != null)
                _showSystemToggle.onValueChanged.AddListener(OnFilterChanged);
        }
        
        /// <summary>
        /// 过滤改变回调
        /// </summary>
        private void OnFilterChanged(bool isOn)
        {
            Refresh();
        }
        
        /// <summary>
        /// 检查是否应该显示条目
        /// </summary>
        private bool ShouldShowEntry(LogEntry entry)
        {
            // 检查分类过滤器
            if (_typeFilters.TryGetValue(entry.Type, out var enabled))
            {
                return enabled;
            }
            
            // 根据Toggle状态判断
            switch (entry.Type)
            {
                case LogType.Damage:
                case LogType.Critical:
                case LogType.Miss:
                    return _showDamageToggle == null || _showDamageToggle.isOn;
                    
                case LogType.Heal:
                    return _showHealToggle == null || _showHealToggle.isOn;
                    
                case LogType.Status:
                case LogType.Buff:
                case LogType.Debuff:
                    return _showStatusToggle == null || _showStatusToggle.isOn;
                    
                case LogType.System:
                case LogType.Victory:
                case LogType.Defeat:
                    return _showSystemToggle == null || _showSystemToggle.isOn;
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// 创建日志UI
        /// </summary>
        private void CreateLogUI(LogEntry entry)
        {
            if (_logEntryPrefab == null || _contentTransform == null) return;
            
            var entryObj = Instantiate(_logEntryPrefab, _contentTransform);
            var textComponent = entryObj.GetComponent<TextMeshProUGUI>();
            
            if (textComponent != null)
            {
                textComponent.text = entry.Message;
                textComponent.color = GetColorForType(entry.Type);
            }
        }
        
        /// <summary>
        /// 获取类型的颜色
        /// </summary>
        private Color GetColorForType(LogType type)
        {
            return type switch
            {
                LogType.Normal => Color.white,
                LogType.Damage => new Color(1f, 0.8f, 0.6f),
                LogType.Heal => new Color(0.4f, 1f, 0.4f),
                LogType.Critical => new Color(1f, 0.3f, 0.3f),
                LogType.Miss => new Color(0.6f, 0.6f, 0.6f),
                LogType.Status => new Color(0.7f, 0.7f, 1f),
                LogType.Buff => new Color(0.4f, 1f, 0.4f),
                LogType.Debuff => new Color(1f, 0.4f, 0.4f),
                LogType.System => new Color(0.8f, 0.8f, 0.8f),
                LogType.Victory => new Color(1f, 0.9f, 0.3f),
                LogType.Defeat => new Color(0.8f, 0.4f, 0.4f),
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 计划自动滚动
        /// </summary>
        private void ScheduleAutoScroll()
        {
            StartCoroutine(AutoScrollCoroutine());
        }
        
        /// <summary>
        /// 自动滚动协程
        /// </summary>
        private System.Collections.IEnumerator AutoScrollCoroutine()
        {
            yield return new WaitForSeconds(_autoScrollDelay);
            
            if (_scrollRect != null)
            {
                // 滚动到底部
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        #endregion
    }
}
