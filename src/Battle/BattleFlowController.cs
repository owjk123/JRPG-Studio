// BattleFlowController.cs - 战斗流程状态机控制器
// 控制战斗的完整流程：开始、行动选择、执行、胜负检查等

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JRPG.Battle.ATB;
using JRPG.Battle.Skills;
using JRPG.Battle.Status;

namespace JRPG.Battle
{
    /// <summary>
    /// 战斗流程控制器
    /// 使用状态机模式控制战斗的完整流程
    /// </summary>
    public class BattleFlowController : MonoBehaviour
    {
        #region Singleton
        
        private static BattleFlowController _instance;
        public static BattleFlowController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BattleFlowController>();
                    if (_instance == null)
                    {
                        var go = new GameObject("BattleFlowController");
                        _instance = go.AddComponent<BattleFlowController>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 战斗阶段变化事件
        /// </summary>
        public event Action<BattlePhase, BattlePhase> OnPhaseChanged;
        
        /// <summary>
        /// 行动开始事件
        /// </summary>
        public event Action<BattleUnit, BattleAction> OnActionStart;
        
        /// <summary>
        /// 行动结束事件
        /// </summary>
        public event Action<BattleUnit, BattleAction, List<DamageResult>> OnActionEnd;
        
        /// <summary>
        /// 玩家需要输入事件
        /// </summary>
        public event Action<BattleUnit> OnPlayerInputRequired;
        
        /// <summary>
        /// 战斗开始事件
        /// </summary>
        public event Action OnBattleStarted;
        
        /// <summary>
        /// 战斗结束事件
        /// </summary>
        public event Action<BattleResult> OnBattleEnded;
        
        #endregion
        
        #region Settings
        
        [Header("战斗设置")]
        [SerializeField] private int _maxTurns = 99;
        [SerializeField] private float _turnTransitionDelay = 0.5f;
        [SerializeField] private float _actionTransitionDelay = 0.3f;
        [SerializeField] private bool _enableAutoBattle = false;
        
        [Header("逃跑设置")]
        [SerializeField] private float _fleeBaseChance = 0.5f;
        [SerializeField] private float _fleeTurnBonus = 0.05f;
        
        #endregion
        
        #region Private Fields
        
        // 状态机
        private BattleStateMachine _stateMachine;
        
        // 战斗数据
        private BattleEncounter _currentEncounter;
        private BattleResult _battleResult = BattleResult.None;
        
        // 当前状态
        private int _currentTurn = 0;
        private BattlePhase _currentPhase = BattlePhase.None;
        private BattleUnit _currentActor;
        private BattleAction _pendingAction;
        
        // UI引用
        private BattleHUD _battleHUD;
        private ATBController _atbController;
        private SkillExecutor _skillExecutor;
        private StatusEffectManager _statusManager;
        
        // 自动战斗
        private bool _isAutoBattle = false;
        
        // 协程
        private Coroutine _battleCoroutine;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 当前战斗阶段
        /// </summary>
        public BattlePhase CurrentPhase => _currentPhase;
        
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentTurn => _currentTurn;
        
        /// <summary>
        /// 当前行动单位
        /// </summary>
        public BattleUnit CurrentActor => _currentActor;
        
        /// <summary>
        /// 是否自动战斗
        /// </summary>
        public bool IsAutoBattle
        {
            get => _isAutoBattle;
            set => _isAutoBattle = value;
        }
        
        /// <summary>
        /// 是否可以逃跑
        /// </summary>
        public bool CanFlee => _currentTurn == 1; // 第一回合后才能逃跑
        
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
            
            InitializeReferences();
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
        /// 开始战斗
        /// </summary>
        public void StartBattle(BattleEncounter encounter)
        {
            if (_battleCoroutine != null)
            {
                StopCoroutine(_battleCoroutine);
            }
            
            _currentEncounter = encounter;
            _currentTurn = 0;
            _battleResult = BattleResult.None;
            
            _battleCoroutine = StartCoroutine(BattleCoroutine());
        }
        
        /// <summary>
        /// 停止战斗
        /// </summary>
        public void StopBattle()
        {
            if (_battleCoroutine != null)
            {
                StopCoroutine(_battleCoroutine);
                _battleCoroutine = null;
            }
            
            _atbController?.StopBattle();
            _battleResult = BattleResult.Draw;
        }
        
        /// <summary>
        /// 玩家选择行动
        /// </summary>
        public void PlayerSelectAction(BattleAction action)
        {
            if (_currentPhase != BattlePhase.PlayerTurn)
            {
                Debug.LogWarning("BattleFlowController: Not in player turn phase");
                return;
            }
            
            _pendingAction = action;
            StartCoroutine(ExecuteActionCoroutine(action));
        }
        
        /// <summary>
        /// 玩家取消行动
        /// </summary>
        public void PlayerCancelAction()
        {
            if (_currentPhase != BattlePhase.PlayerTurn)
                return;
            
            _pendingAction = default;
            
            // 重新显示行动菜单
            if (!_isAutoBattle && _currentActor != null)
            {
                _battleHUD?.ShowActionMenu(_currentActor);
            }
        }
        
        /// <summary>
        /// 玩家选择逃跑
        /// </summary>
        public void PlayerAttemptFlee()
        {
            if (!CanFlee)
            {
                _battleHUD?.AddLog("无法在第一回合逃跑！", BattleLog.LogType.System);
                return;
            }
            
            float fleeChance = CalculateFleeChance();
            
            if (UnityEngine.Random.value < fleeChance)
            {
                StartCoroutine(HandleFleeSuccess());
            }
            else
            {
                StartCoroutine(HandleFleeFail());
            }
        }
        
        /// <summary>
        /// 暂停战斗（用于UI等）
        /// </summary>
        public void PauseBattle()
        {
            Time.timeScale = 0f;
        }
        
        /// <summary>
        /// 恢复战斗
        /// </summary>
        public void ResumeBattle()
        {
            Time.timeScale = 1f;
        }
        
        /// <summary>
        /// 获取所有战斗单位
        /// </summary>
        public List<BattleUnit> GetAllUnits()
        {
            var units = new List<BattleUnit>();
            units.AddRange(BattleManager.Instance?.PlayerUnits ?? new List<BattleUnit>());
            units.AddRange(BattleManager.Instance?.EnemyUnits ?? new List<BattleUnit>());
            return units;
        }
        
        /// <summary>
        /// 获取敌方单位
        /// </summary>
        public List<BattleUnit> GetEnemyUnits(BattleUnit source)
        {
            return GetAllUnits()
                .Where(u => u.IsAlive && u.IsPlayerControlled != source.IsPlayerControlled)
                .ToList();
        }
        
        /// <summary>
        /// 获取友方单位
        /// </summary>
        public List<BattleUnit> GetAllyUnits(BattleUnit source)
        {
            return GetAllUnits()
                .Where(u => u.IsAlive && u.IsPlayerControlled == source.IsPlayerControlled)
                .ToList();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 初始化引用
        /// </summary>
        private void InitializeReferences()
        {
            _battleHUD = BattleHUD.Instance;
            _atbController = ATBController.Instance;
            _skillExecutor = SkillExecutor.Instance;
            _statusManager = StatusEffectManager.Instance;
            _stateMachine = new BattleStateMachine(this);
        }
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_atbController != null)
            {
                _atbController.OnATBFull += OnATBFull;
                _atbController.OnActionComplete += OnActionComplete;
            }
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_atbController != null)
            {
                _atbController.OnATBFull -= OnATBFull;
                _atbController.OnActionComplete -= OnActionComplete;
            }
        }
        
        /// <summary>
        /// 主战斗协程
        /// </summary>
        private IEnumerator BattleCoroutine()
        {
            // 战斗初始化
            yield return StartCoroutine(BattleStartSequence());
            
            // 战斗主循环
            while (_battleResult == BattleResult.None)
            {
                // 检查回合上限
                if (_currentTurn >= _maxTurns)
                {
                    yield return StartCoroutine(HandleMaxTurnsReached());
                    break;
                }
                
                // 开始新回合
                yield return StartCoroutine(StartNewTurn());
                
                // ATB等待循环
                yield return StartCoroutine(ATBWaitLoop());
                
                yield return new WaitForSeconds(_turnTransitionDelay);
            }
            
            // 战斗结束
            yield return StartCoroutine(BattleEndSequence());
        }
        
        /// <summary>
        /// 战斗开始序列
        /// </summary>
        private IEnumerator BattleStartSequence()
        {
            ChangePhase(BattlePhase.BattleStart);
            
            // 初始化ATB系统
            _atbController.Initialize();
            
            // 注册所有单位
            foreach (var unit in GetAllUnits())
            {
                _atbController.RegisterUnit(unit);
            }
            
            // 初始化UI
            _battleHUD?.Initialize();
            _battleHUD?.SetPlayerUnits(BattleManager.Instance.PlayerUnits);
            _battleHUD?.SetEnemyUnits(BattleManager.Instance.EnemyUnits);
            
            // 显示战斗开始
            string playerTeam = string.Join(", ", BattleManager.Instance.PlayerUnits.Select(u => u.CharacterData.characterName));
            string enemyTeam = string.Join(", ", BattleManager.Instance.EnemyUnits.Select(u => u.CharacterData.characterName));
            _battleHUD?.AddBattleStartLog(playerTeam, enemyTeam);
            
            // 等待动画播放
            yield return new WaitForSeconds(1f);
            
            OnBattleStarted?.Invoke();
        }
        
        /// <summary>
        /// 开始新回合
        /// </summary>
        private IEnumerator StartNewTurn()
        {
            _currentTurn++;
            _battleHUD?.UpdateTurnCounter(_currentTurn);
            
            // 处理回合开始状态效果
            foreach (var unit in GetAllUnits().Where(u => u.IsAlive))
            {
                _statusManager.OnTurnStart(unit);
            }
            
            yield return new WaitForSeconds(_actionTransitionDelay);
        }
        
        /// <summary>
        /// ATB等待循环
        /// </summary>
        private IEnumerator ATBWaitLoop()
        {
            while (true)
            {
                // 检查胜负
                if (CheckBattleEnd())
                    yield break;
                
                // 获取下一个可行动单位
                var nextActor = _atbController.GetNextActor();
                
                if (nextActor == null)
                {
                    // 没有可行动单位，等待ATB充能
                    yield return null;
                    continue;
                }
                
                // 设置为当前行动者
                _currentActor = nextActor;
                _atbController.SetUnitActing(nextActor);
                
                // 检查控制状态
                if (_statusManager.CheckControlStatus(nextActor))
                {
                    // 被控制，跳过
                    _battleHUD?.AddLog($"{nextActor.CharacterData.characterName}无法行动！", BattleLog.LogType.System);
                    _atbController.CompleteAction(nextActor);
                    _currentActor = null;
                    continue;
                }
                
                // 根据单位类型处理回合
                if (nextActor.IsPlayerControlled)
                {
                    yield return StartCoroutine(HandlePlayerTurn(nextActor));
                }
                else
                {
                    yield return StartCoroutine(HandleEnemyTurn(nextActor));
                }
                
                // 完成行动
                _atbController.CompleteAction(nextActor);
                _currentActor = null;
                
                yield return new WaitForSeconds(_actionTransitionDelay);
            }
        }
        
        /// <summary>
        /// 处理玩家回合
        /// </summary>
        private IEnumerator HandlePlayerTurn(BattleUnit unit)
        {
            ChangePhase(BattlePhase.PlayerTurn);
            
            // 自动战斗
            if (_isAutoBattle)
            {
                yield return StartCoroutine(AutoBattleAction(unit));
                yield break;
            }
            
            // 等待玩家输入
            OnPlayerInputRequired?.Invoke(unit);
            _battleHUD?.ShowActionMenu(unit);
            
            // 等待玩家选择（通过事件回调）
            while (_pendingAction.Source == null)
            {
                yield return null;
            }
        }
        
        /// <summary>
        /// 处理敌人回合
        /// </summary>
        private IEnumerator HandleEnemyTurn(BattleUnit unit)
        {
            ChangePhase(BattlePhase.EnemyTurn);
            
            // 敌人AI决策
            var action = DecideEnemyAction(unit);
            
            yield return StartCoroutine(ExecuteActionCoroutine(action));
        }
        
        /// <summary>
        /// 执行行动协程
        /// </summary>
        private IEnumerator ExecuteActionCoroutine(BattleAction action)
        {
            ChangePhase(BattlePhase.ActionExecution);
            
            OnActionStart?.Invoke(action.Source, action);
            
            // 确定目标
            if (action.Targets.Count == 0)
            {
                action.Targets = DetermineTargets(action);
            }
            
            // 执行技能
            if (action.Skill != null)
            {
                yield return _skillExecutor.ExecuteAction(action);
            }
            else if (action.ActionType == ActionType.Defend)
            {
                // 防御
                action.Source.ApplyStatus(StatusEffectInstance.Create(StatusEffectType.DefenseUp, 1, StatusSource.Skill));
                _battleHUD?.AddLog($"{action.Source.CharacterData.characterName}进入防御姿态", BattleLog.LogType.Buff);
            }
            
            // 处理行动结束效果
            ProcessActionEndEffects(action);
            
            // 检查胜负
            CheckBattleEnd();
            
            // 回合结束状态效果
            foreach (var target in action.Targets)
            {
                _statusManager.OnTurnEnd(target);
            }
            
            // 清除待处理行动
            _pendingAction = default;
            
            OnActionEnd?.Invoke(action.Source, action, null);
            
            // 等待动画
            yield return new WaitForSeconds(_actionTransitionDelay);
        }
        
        /// <summary>
        /// 自动战斗行动
        /// </summary>
        private IEnumerator AutoBattleAction(BattleUnit unit)
        {
            var enemies = GetEnemyUnits(unit);
            if (enemies.Count == 0) yield break;
            
            // 简单AI：使用技能或普攻
            var skills = unit.CharacterData?.activeSkills
                .Where(s => _skillExecutor.ValidateSkill(unit, s))
                .ToList();
            
            if (skills != null && skills.Count > 0)
            {
                // 使用第一个可用技能
                var skill = skills[UnityEngine.Random.Range(0, skills.Count)];
                var targets = _skillExecutor.SelectTargets(unit, skill, GetAllUnits());
                
                var action = BattleAction.CreateSkillAction(unit, skill, targets);
                yield return ExecuteActionCoroutine(action);
            }
            else
            {
                // 普通攻击
                var target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
                var action = BattleAction.CreateNormalAttack(unit, target);
                yield return ExecuteActionCoroutine(action);
            }
        }
        
        /// <summary>
        /// 敌人AI决策
        /// </summary>
        private BattleAction DecideEnemyAction(BattleUnit enemy)
        {
            var enemies = GetEnemyUnits(enemy);
            var allies = GetAllyUnits(enemy);
            
            // 简单AI策略
            // 1. 检查是否需要治疗
            if (enemy.CurrentHp < enemy.Stats.MaxHp * 0.3f)
            {
                var healSkill = enemy.CharacterData?.activeSkills
                    .FirstOrDefault(s => s.damageType == DamageType.Heal && _skillExecutor.ValidateSkill(enemy, s));
                
                if (healSkill != null)
                {
                    return BattleAction.CreateSkillAction(enemy, healSkill, new List<BattleUnit> { enemy });
                }
            }
            
            // 2. 使用攻击技能
            var attackSkills = enemy.CharacterData?.activeSkills
                .Where(s => s.damageType != DamageType.Heal && _skillExecutor.ValidateSkill(enemy, s))
                .ToList();
            
            if (attackSkills != null && attackSkills.Count > 0)
            {
                var skill = attackSkills[UnityEngine.Random.Range(0, attackSkills.Count)];
                var targets = _skillExecutor.SelectTargets(enemy, skill, GetAllUnits());
                return BattleAction.CreateSkillAction(enemy, skill, targets);
            }
            
            // 3. 普通攻击
            if (enemies.Count > 0)
            {
                var target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
                return BattleAction.CreateNormalAttack(enemy, target);
            }
            
            // 4. 防御
            return BattleAction.CreateDefendAction(enemy);
        }
        
        /// <summary>
        /// 确定目标
        /// </summary>
        private List<BattleUnit> DetermineTargets(BattleAction action)
        {
            var allUnits = GetAllUnits();
            
            if (action.Skill != null)
            {
                return _skillExecutor.SelectTargets(action.Source, action.Skill, allUnits);
            }
            
            // 默认选择敌方单体
            var enemies = GetEnemyUnits(action.Source);
            if (enemies.Count > 0)
            {
                return new List<BattleUnit> { enemies[0] };
            }
            
            return new List<BattleUnit>();
        }
        
        /// <summary>
        /// 处理行动结束效果
        /// </summary>
        private void ProcessActionEndEffects(BattleAction action)
        {
            // 检查反击
            // 检查连携
            // 触发被动技能效果
        }
        
        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        private bool CheckBattleEnd()
        {
            var alivePlayers = BattleManager.Instance?.PlayerUnits?.Where(u => u.IsAlive).ToList() ?? new List<BattleUnit>();
            var aliveEnemies = BattleManager.Instance?.EnemyUnits?.Where(u => u.IsAlive).ToList() ?? new List<BattleUnit>();
            
            if (alivePlayers.Count == 0)
            {
                _battleResult = BattleResult.Defeat;
                return true;
            }
            
            if (aliveEnemies.Count == 0)
            {
                _battleResult = BattleResult.Victory;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 处理最大回合数
        /// </summary>
        private IEnumerator HandleMaxTurnsReached()
        {
            _battleHUD?.AddLog($"达到最大回合数({_maxTurns})，战斗强制结束", BattleLog.LogType.System);
            
            // 比较剩余HP
            int playerHP = BattleManager.Instance.PlayerUnits.Sum(u => u.CurrentHp);
            int enemyHP = BattleManager.Instance.EnemyUnits.Sum(u => u.CurrentHp);
            
            if (playerHP > enemyHP)
            {
                _battleResult = BattleResult.Victory;
            }
            else if (enemyHP > playerHP)
            {
                _battleResult = BattleResult.Defeat;
            }
            else
            {
                _battleResult = BattleResult.Draw;
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// 处理逃跑成功
        /// </summary>
        private IEnumerator HandleFleeSuccess()
        {
            _battleHUD?.AddLog("逃跑成功！", BattleLog.LogType.System);
            yield return new WaitForSeconds(1f);
            
            _battleResult = BattleResult.Flee;
        }
        
        /// <summary>
        /// 处理逃跑失败
        /// </summary>
        private IEnumerator HandleFleeFail()
        {
            _battleHUD?.AddLog("逃跑失败！", BattleLog.LogType.System);
            yield return new WaitForSeconds(0.5f);
            
            // 敌人获得一次行动机会
            var enemies = GetEnemyUnits(_currentActor);
            if (enemies.Count > 0)
            {
                _currentActor = enemies[0];
                yield return StartCoroutine(HandleEnemyTurn(_currentActor));
            }
        }
        
        /// <summary>
        /// 计算逃跑概率
        /// </summary>
        private float CalculateFleeChance()
        {
            float chance = _fleeBaseChance + (_currentTurn - 1) * _fleeTurnBonus;
            
            // 速度加成
            int playerAvgSpeed = (int)BattleManager.Instance.PlayerUnits.Average(u => u.Stats.Spd);
            int enemyAvgSpeed = (int)BattleManager.Instance.EnemyUnits.Average(u => u.Stats.Spd);
            
            if (playerAvgSpeed > enemyAvgSpeed)
            {
                chance += 0.1f;
            }
            else if (playerAvgSpeed < enemyAvgSpeed)
            {
                chance -= 0.1f;
            }
            
            return Mathf.Clamp01(chance);
        }
        
        /// <summary>
        /// 战斗结束序列
        /// </summary>
        private IEnumerator BattleEndSequence()
        {
            ChangePhase(BattlePhase.BattleEnd);
            
            _atbController?.StopBattle();
            
            switch (_battleResult)
            {
                case BattleResult.Victory:
                    _battleHUD?.AddVictoryLog();
                    yield return StartCoroutine(PlayVictorySequence());
                    break;
                    
                case BattleResult.Defeat:
                    _battleHUD?.AddDefeatLog();
                    yield return StartCoroutine(PlayDefeatSequence());
                    break;
                    
                case BattleResult.Flee:
                    _battleHUD?.AddLog("成功脱离战斗！", BattleLog.LogType.System);
                    break;
                    
                case BattleResult.Draw:
                    _battleHUD?.AddLog("战斗平局", BattleLog.LogType.System);
                    break;
            }
            
            OnBattleEnded?.Invoke(_battleResult);
            
            // 清理
            _battleHUD?.HideAllPanels();
            
            yield return new WaitForSeconds(2f);
            
            // 通知BattleManager战斗结束
            BattleManager.Instance?.EndBattle(_battleResult);
        }
        
        /// <summary>
        /// 播放胜利序列
        /// </summary>
        private IEnumerator PlayVictorySequence()
        {
            // 播放胜利动画
            foreach (var unit in BattleManager.Instance.PlayerUnits.Where(u => u.IsAlive))
            {
                unit.PlayAnimation(BattleAnimation.Victory);
            }
            
            yield return new WaitForSeconds(2f);
        }
        
        /// <summary>
        /// 播放失败序列
        /// </summary>
        private IEnumerator PlayDefeatSequence()
        {
            // 播放失败动画
            foreach (var unit in BattleManager.Instance.PlayerUnits.Where(u => u.IsAlive))
            {
                unit.PlayAnimation(BattleAnimation.Die);
            }
            
            yield return new WaitForSeconds(2f);
        }
        
        /// <summary>
        /// 改变战斗阶段
        /// </summary>
        private void ChangePhase(BattlePhase newPhase)
        {
            var oldPhase = _currentPhase;
            _currentPhase = newPhase;
            
            _battleHUD?.UpdatePhase(newPhase);
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnATBFull(BattleUnit unit)
        {
            // ATB满了，可以行动
        }
        
        private void OnActionComplete(BattleUnit unit)
        {
            // 行动完成
        }
        
        #endregion
    }
    
    /// <summary>
    /// 战斗状态机
    /// </summary>
    public class BattleStateMachine
    {
        private BattleFlowController _controller;
        private Dictionary<BattlePhase, BattlePhaseState> _states;
        private BattlePhaseState _currentState;
        
        public BattleStateMachine(BattleFlowController controller)
        {
            _controller = controller;
            InitializeStates();
        }
        
        private void InitializeStates()
        {
            _states = new Dictionary<BattlePhase, BattlePhaseState>
            {
                { BattlePhase.None, new BattlePhaseState(BattlePhase.None) },
                { BattlePhase.BattleStart, new BattlePhaseState(BattlePhase.BattleStart) },
                { BattlePhase.PlayerTurn, new BattlePhaseState(BattlePhase.PlayerTurn) },
                { BattlePhase.EnemyTurn, new BattlePhaseState(BattlePhase.EnemyTurn) },
                { BattlePhase.ActionExecution, new BattlePhaseState(BattlePhase.ActionExecution) },
                { BattlePhase.DamageCalculation, new BattlePhaseState(BattlePhase.DamageCalculation) },
                { BattlePhase.StatusEffects, new BattlePhaseState(BattlePhase.StatusEffects) },
                { BattlePhase.TurnEnd, new BattlePhaseState(BattlePhase.TurnEnd) },
                { BattlePhase.BattleVictory, new BattlePhaseState(BattlePhase.BattleVictory) },
                { BattlePhase.BattleDefeat, new BattlePhaseState(BattlePhase.BattleDefeat) },
                { BattlePhase.BattleEnd, new BattlePhaseState(BattlePhase.BattleEnd) }
            };
        }
        
        public void Transition(BattlePhase targetPhase)
        {
            if (_states.TryGetValue(targetPhase, out var state))
            {
                _currentState?.OnExit(_controller);
                _currentState = state;
                _currentState.OnEnter(_controller);
            }
        }
    }
    
    /// <summary>
    /// 战斗阶段状态
    /// </summary>
    public class BattlePhaseState
    {
        public BattlePhase Phase { get; }
        
        public BattlePhaseState(BattlePhase phase)
        {
            Phase = phase;
        }
        
        public virtual void OnEnter(BattleFlowController controller) { }
        public virtual void OnExit(BattleFlowController controller) { }
        public virtual void OnUpdate(BattleFlowController controller) { }
    }
}
