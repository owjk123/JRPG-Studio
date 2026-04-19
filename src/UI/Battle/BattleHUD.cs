// BattleHUD.cs - 战斗界面总控制器
// 管理整个战斗界面的UI组件

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using JRPG.Battle.ATB;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// 战斗界面总控制器
    /// 管理所有战斗相关的UI组件
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        #region Singleton
        
        private static BattleHUD _instance;
        public static BattleHUD Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BattleHUD>();
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region UI References
        
        [Header("ATB条容器")]
        [SerializeField] private Transform _playerATBContainer;
        [SerializeField] private Transform _enemyATBContainer;
        [SerializeField] private ATBBar _atbBarPrefab;
        
        [Header("行动菜单")]
        [SerializeField] private ActionMenu _actionMenu;
        
        [Header("技能选择面板")]
        [SerializeField] private SkillSelectPanel _skillSelectPanel;
        
        [Header("目标选择")]
        [SerializeField] private TargetSelectArrow _targetArrow;
        
        [Header("伤害数字")]
        [SerializeField] private DamageNumber _damageNumberPrefab;
        [SerializeField] private Transform _damageNumberContainer;
        
        [Header("战斗日志")]
        [SerializeField] private BattleLog _battleLog;
        
        [Header("信息面板")]
        [SerializeField] private TextMeshProUGUI _turnCounter;
        [SerializeField] private TextMeshProUGUI _phaseIndicator;
        
        [Header("快捷操作")]
        [SerializeField] private Button _autoBattleButton;
        [SerializeField] private Button _speedButton;
        [SerializeField] private Button _skipButton;
        
        #endregion
        
        #region Private Fields
        
        private List<ATBBar> _playerATBBars = new List<ATBBar>();
        private List<ATBBar> _enemyATBBars = new List<ATBBar>();
        private List<DamageNumber> _activeDamageNumbers = new List<DamageNumber>();
        
        private bool _isAutoBattle = false;
        private int _speedLevel = 1; // 1x, 2x, 3x
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 行动菜单
        /// </summary>
        public ActionMenu ActionMenu => _actionMenu;
        
        /// <summary>
        /// 技能选择面板
        /// </summary>
        public SkillSelectPanel SkillSelectPanel => _skillSelectPanel;
        
        /// <summary>
        /// 目标选择箭头
        /// </summary>
        public TargetSelectArrow TargetArrow => _targetArrow;
        
        /// <summary>
        /// 战斗日志
        /// </summary>
        public BattleLog BattleLog => _battleLog;
        
        /// <summary>
        /// 是否自动战斗
        /// </summary>
        public bool IsAutoBattle => _isAutoBattle;
        
        /// <summary>
        /// 当前速度等级
        /// </summary>
        public int SpeedLevel => _speedLevel;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeUI();
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化战斗HUD
        /// </summary>
        public void Initialize()
        {
            // 清空现有ATB条
            ClearATBBars();
            
            // 隐藏UI
            HideAllPanels();
        }
        
        /// <summary>
        /// 设置玩家单位
        /// </summary>
        public void SetPlayerUnits(List<BattleUnit> units)
        {
            ClearATBBars(_playerATBBars);
            _playerATBBars.Clear();
            
            for (int i = 0; i < units.Count; i++)
            {
                var atbBar = CreateATBBar(units[i], _playerATBContainer);
                atbBar.SetPosition(new Vector2(-300, 150 - i * 80));
                _playerATBBars.Add(atbBar);
            }
        }
        
        /// <summary>
        /// 设置敌人单位
        /// </summary>
        public void SetEnemyUnits(List<BattleUnit> units)
        {
            ClearATBBars(_enemyATBBars);
            _enemyATBBars.Clear();
            
            for (int i = 0; i < units.Count; i++)
            {
                var atbBar = CreateATBBar(units[i], _enemyATBContainer);
                atbBar.SetPosition(new Vector2(300, 150 - i * 80));
                _enemyATBBars.Add(atbBar);
            }
        }
        
        /// <summary>
        /// 显示行动菜单
        /// </summary>
        public void ShowActionMenu(BattleUnit unit)
        {
            if (_actionMenu != null)
            {
                _actionMenu.gameObject.SetActive(true);
                _actionMenu.Initialize(unit);
            }
        }
        
        /// <summary>
        /// 隐藏行动菜单
        /// </summary>
        public void HideActionMenu()
        {
            if (_actionMenu != null)
            {
                _actionMenu.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 显示技能选择面板
        /// </summary>
        public void ShowSkillPanel(BattleUnit unit)
        {
            if (_skillSelectPanel != null)
            {
                _skillSelectPanel.gameObject.SetActive(true);
                _skillSelectPanel.Initialize(unit);
            }
        }
        
        /// <summary>
        /// 隐藏技能选择面板
        /// </summary>
        public void HideSkillPanel()
        {
            if (_skillSelectPanel != null)
            {
                _skillSelectPanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 显示目标选择
        /// </summary>
        public void ShowTargetSelection(List<BattleUnit> targets, System.Action<BattleUnit> onSelected)
        {
            if (_targetArrow != null)
            {
                _targetArrow.gameObject.SetActive(true);
                _targetArrow.Initialize(targets, onSelected);
            }
        }
        
        /// <summary>
        /// 隐藏目标选择
        /// </summary>
        public void HideTargetSelection()
        {
            if (_targetArrow != null)
            {
                _targetArrow.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamageNumber(BattleUnit target, DamageResult result)
        {
            if (_damageNumberPrefab == null || _damageNumberContainer == null) return;
            
            var damageNum = Instantiate(_damageNumberPrefab, _damageNumberContainer);
            damageNum.Initialize(target.transform.position, result);
            
            _activeDamageNumbers.Add(damageNum);
            
            // 延迟销毁
            StartCoroutine(DestroyAfterDelay(damageNum, 2f));
        }
        
        /// <summary>
        /// 添加战斗日志
        /// </summary>
        public void AddLog(string message, BattleLog.LogType logType = BattleLog.LogType.Normal)
        {
            if (_battleLog != null)
            {
                _battleLog.AddEntry(message, logType);
            }
        }
        
        /// <summary>
        /// 更新回合数显示
        /// </summary>
        public void UpdateTurnCounter(int turn)
        {
            if (_turnCounter != null)
            {
                _turnCounter.text = $"回合 {turn}";
            }
        }
        
        /// <summary>
        /// 更新阶段显示
        /// </summary>
        public void UpdatePhase(BattlePhase phase)
        {
            if (_phaseIndicator != null)
            {
                _phaseIndicator.text = GetPhaseName(phase);
            }
        }
        
        /// <summary>
        /// 切换自动战斗
        /// </summary>
        public void ToggleAutoBattle()
        {
            _isAutoBattle = !_isAutoBattle;
            
            if (_autoBattleButton != null)
            {
                _autoBattleButton.GetComponentInChildren<TextMeshProUGUI>().text = 
                    _isAutoBattle ? "自动:开" : "自动:关";
            }
        }
        
        /// <summary>
        /// 切换战斗速度
        /// </summary>
        public void CycleSpeed()
        {
            _speedLevel = (_speedLevel % 3) + 1; // 1 -> 2 -> 3 -> 1
            
            if (_speedButton != null)
            {
                _speedButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{_speedLevel}x";
            }
            
            // 更新ATB控制器速度
            ATBController.Instance.TimeScale = _speedLevel;
        }
        
        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanels()
        {
            HideActionMenu();
            HideSkillPanel();
            HideTargetSelection();
        }
        
        /// <summary>
        /// 显示等待玩家输入指示
        /// </summary>
        public void ShowWaitingIndicator()
        {
            // 可以添加闪烁动画等
        }
        
        /// <summary>
        /// 高亮可行动单位
        /// </summary>
        public void HighlightReadyUnits()
        {
            foreach (var bar in _playerATBBars)
            {
                if (bar.BoundUnit != null)
                {
                    float atbPercent = ATBController.Instance.GetATBPercent(bar.BoundUnit);
                    bar.SetHighlight(atbPercent >= 1f);
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 初始化UI组件
        /// </summary>
        private void InitializeUI()
        {
            // 初始化按钮事件
            if (_autoBattleButton != null)
            {
                _autoBattleButton.onClick.AddListener(ToggleAutoBattle);
            }
            
            if (_speedButton != null)
            {
                _speedButton.onClick.AddListener(CycleSpeed);
            }
            
            // 默认隐藏面板
            HideAllPanels();
        }
        
        /// <summary>
        /// 创建ATB条
        /// </summary>
        private ATBBar CreateATBBar(BattleUnit unit, Transform container)
        {
            var atbBar = Instantiate(_atbBarPrefab, container);
            atbBar.BindUnit(unit);
            return atbBar;
        }
        
        /// <summary>
        /// 清空ATB条
        /// </summary>
        private void ClearATBBars()
        {
            ClearATBBars(_playerATBBars);
            ClearATBBars(_enemyATBBars);
        }
        
        private void ClearATBBars(List<ATBBar> bars)
        {
            foreach (var bar in bars)
            {
                if (bar != null)
                {
                    bar.UnbindUnit();
                    Destroy(bar.gameObject);
                }
            }
            bars.Clear();
        }
        
        /// <summary>
        /// 获取阶段名称
        /// </summary>
        private string GetPhaseName(BattlePhase phase)
        {
            switch (phase)
            {
                case BattlePhase.BattleStart: return "战斗开始";
                case BattlePhase.PlayerTurn: return "玩家回合";
                case BattlePhase.EnemyTurn: return "敌人回合";
                case BattlePhase.ActionExecution: return "执行行动";
                case BattlePhase.DamageCalculation: return "伤害计算";
                case BattlePhase.StatusEffects: return "状态效果";
                case BattlePhase.TurnEnd: return "回合结束";
                case BattlePhase.BattleVictory: return "胜利";
                case BattlePhase.BattleDefeat: return "失败";
                case BattlePhase.BattleEnd: return "战斗结束";
                default: return "";
            }
        }
        
        /// <summary>
        /// 延迟销毁协程
        /// </summary>
        private System.Collections.IEnumerator DestroyAfterDelay(DamageNumber obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (obj != null)
            {
                _activeDamageNumbers.Remove(obj);
                Destroy(obj.gameObject);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void SubscribeToEvents()
        {
            // 订阅战斗管理器事件
            var battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                battleManager.OnTurnStart += OnTurnStart;
                battleManager.OnPhaseChanged += OnPhaseChanged;
                battleManager.OnBattleEnd += OnBattleEnd;
            }
            
            // 订阅技能执行器事件
            var skillExecutor = JRPG.Battle.Skills.SkillExecutor.Instance;
            if (skillExecutor != null)
            {
                skillExecutor.OnDamageNumberRequest += OnDamageNumberRequest;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            var battleManager = BattleManager.Instance;
            if (battleManager != null)
            {
                battleManager.OnTurnStart -= OnTurnStart;
                battleManager.OnPhaseChanged -= OnPhaseChanged;
                battleManager.OnBattleEnd -= OnBattleEnd;
            }
            
            var skillExecutor = JRPG.Battle.Skills.SkillExecutor.Instance;
            if (skillExecutor != null)
            {
                skillExecutor.OnDamageNumberRequest -= OnDamageNumberRequest;
            }
        }
        
        private void OnTurnStart(int turn)
        {
            UpdateTurnCounter(turn);
            HighlightReadyUnits();
        }
        
        private void OnPhaseChanged(BattlePhase oldPhase, BattlePhase newPhase)
        {
            UpdatePhase(newPhase);
            
            if (newPhase == BattlePhase.PlayerTurn)
            {
                // 玩家回合开始，显示行动菜单
            }
        }
        
        private void OnBattleEnd(BattleResult result)
        {
            HideAllPanels();
            // 显示结果面板
        }
        
        private void OnDamageNumberRequest(BattleUnit target, DamageResult result)
        {
            ShowDamageNumber(target, result);
        }
        
        #endregion
    }
}
