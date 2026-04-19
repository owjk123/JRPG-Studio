// BattleManager.cs - 战斗管理器
// 单例模式，管理战斗的初始化和生命周期

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using JRPG.Character;

namespace JRPG.Battle
{
    /// <summary>
    /// 战斗管理器 - 单例模式
    /// 管理战斗的初始化和生命周期
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        #region Singleton
        
        private static BattleManager _instance;
        public static BattleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BattleManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("BattleManager");
                        _instance = go.AddComponent<BattleManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 战斗开始事件
        /// </summary>
        public event Action OnBattleStart;
        
        /// <summary>
        /// 战斗结束事件
        /// </summary>
        public event Action<BattleResult> OnBattleEnd;
        
        /// <summary>
        /// 回合开始事件
        /// </summary>
        public event Action<int> OnTurnStart;
        
        /// <summary>
        /// 阶段变化事件
        /// </summary>
        public event Action<BattlePhase, BattlePhase> OnPhaseChanged;
        
        /// <summary>
        /// 单位行动事件
        /// </summary>
        public event Action<BattleUnit, BattleAction> OnUnitAction;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentTurn => BattleFlowController.Instance?.CurrentTurn ?? 0;
        
        /// <summary>
        /// 当前战斗阶段
        /// </summary>
        public BattlePhase CurrentPhase => BattleFlowController.Instance?.CurrentPhase ?? BattlePhase.None;
        
        /// <summary>
        /// 玩家单位列表
        /// </summary>
        public IReadOnlyList<BattleUnit> PlayerUnits => _playerUnits;
        
        /// <summary>
        /// 敌人单位列表
        /// </summary>
        public IReadOnlyList<BattleUnit> EnemyUnits => _enemyUnits;
        
        /// <summary>
        /// 是否正在战斗中
        /// </summary>
        public bool IsInBattle => _currentEncounter != null && _battleResult == BattleResult.None;
        
        /// <summary>
        /// 战斗是否可逃跑
        /// </summary>
        public bool CanFlee => _currentEncounter?.allowFlee ?? false && CurrentTurn > 0;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// 当前战斗遭遇
        /// </summary>
        private BattleEncounter _currentEncounter;
        
        /// <summary>
        /// 战斗结果
        /// </summary>
        private BattleResult _battleResult = BattleResult.None;
        
        /// <summary>
        /// 玩家单位
        /// </summary>
        private List<BattleUnit> _playerUnits = new List<BattleUnit>();
        
        /// <summary>
        /// 敌人单位
        /// </summary>
        private List<BattleUnit> _enemyUnits = new List<BattleUnit>();
        
        /// <summary>
        /// 战斗流程控制器
        /// </summary>
        private BattleFlowController _flowController;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Initialize();
        }
        
        private void Start()
        {
            // 初始化流程控制器
            _flowController = BattleFlowController.Instance;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化战斗管理器
        /// </summary>
        public void Initialize()
        {
            _currentEncounter = null;
            _battleResult = BattleResult.None;
            _playerUnits.Clear();
            _enemyUnits.Clear();
        }
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle(BattleEncounter encounter)
        {
            if (encounter == null)
            {
                Debug.LogError("BattleManager: Encounter is null!");
                return;
            }
            
            _currentEncounter = encounter;
            _battleResult = BattleResult.None;
            
            // 清空之前的单位
            ClearUnits();
            
            // 创建玩家单位
            foreach (var charData in encounter.playerCharacters)
            {
                var unit = CreateUnit(charData, true);
                _playerUnits.Add(unit);
            }
            
            // 创建敌人单位
            foreach (var enemyFormation in encounter.enemies)
            {
                var unit = CreateUnit(enemyFormation.enemyData, false);
                
                // 设置等级
                unit.Initialize(enemyFormation.enemyData, false);
                
                _enemyUnits.Add(unit);
            }
            
            // 通知UI
            var battleHUD = UI.Battle.BattleHUD.Instance;
            if (battleHUD != null)
            {
                battleHUD.Initialize();
                battleHUD.SetPlayerUnits(_playerUnits);
                battleHUD.SetEnemyUnits(_enemyUnits);
            }
            
            // 开始战斗流程
            _flowController?.StartBattle(encounter);
            
            OnBattleStart?.Invoke();
        }
        
        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndBattle(BattleResult result)
        {
            _battleResult = result;
            
            // 清理战斗单位
            CleanupUnits();
            
            // 触发事件
            OnBattleEnd?.Invoke(result);
            
            // 处理战斗结果
            HandleBattleResult(result);
        }
        
        /// <summary>
        /// 玩家选择行动
        /// </summary>
        public void PlayerSelectAction(BattleAction action)
        {
            _flowController?.PlayerSelectAction(action);
        }
        
        /// <summary>
        /// 玩家取消行动选择
        /// </summary>
        public void PlayerCancelAction()
        {
            _flowController?.PlayerCancelAction();
        }
        
        /// <summary>
        /// 玩家尝试逃跑
        /// </summary>
        public void PlayerAttemptFlee()
        {
            if (!CanFlee)
            {
                Debug.Log("BattleManager: Cannot flee this battle!");
                return;
            }
            
            _flowController?.PlayerAttemptFlee();
        }
        
        /// <summary>
        /// 获取所有存活的单位
        /// </summary>
        public List<BattleUnit> GetAllAliveUnits()
        {
            return _playerUnits.Concat(_enemyUnits)
                .Where(u => u != null && u.IsAlive)
                .ToList();
        }
        
        /// <summary>
        /// 获取敌方存活单位
        /// </summary>
        public List<BattleUnit> GetAliveEnemies(BattleUnit source)
        {
            return (source.IsPlayerControlled ? _enemyUnits : _playerUnits)
                .Where(u => u != null && u.IsAlive)
                .ToList();
        }
        
        /// <summary>
        /// 获取友方存活单位
        /// </summary>
        public List<BattleUnit> GetAliveAllies(BattleUnit source)
        {
            return (source.IsPlayerControlled ? _playerUnits : _enemyUnits)
                .Where(u => u != null && u.IsAlive)
                .ToList();
        }
        
        /// <summary>
        /// 获取指定单位的预设位置
        /// </summary>
        public Vector3 GetUnitPosition(BattleUnit unit, bool isPlayer)
        {
            // 计算位置索引
            var units = isPlayer ? _playerUnits : _enemyUnits;
            int index = units.IndexOf(unit);
            if (index < 0) index = 0;
            
            // 根据位置计算世界坐标
            float spacing = 2f;
            float startX = isPlayer ? -3f : 3f;
            
            return new Vector3(startX, 0, index * spacing);
        }
        
        /// <summary>
        /// 设置自动战斗
        /// </summary>
        public void SetAutoBattle(bool enabled)
        {
            if (_flowController != null)
            {
                _flowController.IsAutoBattle = enabled;
            }
        }
        
        /// <summary>
        /// 暂停战斗
        /// </summary>
        public void PauseBattle()
        {
            _flowController?.PauseBattle();
        }
        
        /// <summary>
        /// 恢复战斗
        /// </summary>
        public void ResumeBattle()
        {
            _flowController?.ResumeBattle();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 创建战斗单位
        /// </summary>
        private BattleUnit CreateUnit(CharacterData data, bool isPlayer)
        {
            var go = new GameObject($"{(isPlayer ? "Player" : "Enemy")}_{data.characterName}");
            go.transform.SetParent(transform);
            
            var unit = go.AddComponent<BattleUnit>();
            unit.Initialize(data, isPlayer);
            
            return unit;
        }
        
        /// <summary>
        /// 清空单位
        /// </summary>
        private void ClearUnits()
        {
            foreach (var unit in _playerUnits.Concat(_enemyUnits))
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }
            
            _playerUnits.Clear();
            _enemyUnits.Clear();
        }
        
        /// <summary>
        /// 清理战斗单位（保留数据）
        /// </summary>
        private void CleanupUnits()
        {
            // 可以在这里实现对象池逻辑
            // 目前简单销毁
            ClearUnits();
        }
        
        /// <summary>
        /// 处理战斗结果
        /// </summary>
        private void HandleBattleResult(BattleResult result)
        {
            switch (result)
            {
                case BattleResult.Victory:
                    HandleVictory();
                    break;
                    
                case BattleResult.Defeat:
                    HandleDefeat();
                    break;
                    
                case BattleResult.Flee:
                    HandleFlee();
                    break;
                    
                case BattleResult.Draw:
                    HandleDraw();
                    break;
            }
        }
        
        /// <summary>
        /// 处理胜利
        /// </summary>
        private void HandleVictory()
        {
            Debug.Log("Battle Victory!");
            // TODO: 计算奖励、经验等
        }
        
        /// <summary>
        /// 处理失败
        /// </summary>
        private void HandleDefeat()
        {
            Debug.Log("Battle Defeat!");
            // TODO: 处理游戏结束或检查点
        }
        
        /// <summary>
        /// 处理逃跑
        /// </summary>
        private void HandleFlee()
        {
            Debug.Log("Fled from battle!");
            // TODO: 返回之前的场景
        }
        
        /// <summary>
        /// 处理平局
        /// </summary>
        private void HandleDraw()
        {
            Debug.Log("Battle Draw!");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 优先级队列简单实现
    /// </summary>
    public class PriorityQueue<TElement, TPriority>
    {
        private List<(TElement Element, TPriority Priority)> _items = new List<(TElement, TPriority)>();
        
        public int Count => _items.Count;
        
        public void Enqueue(TElement element, TPriority priority)
        {
            _items.Add((element, priority));
            _items.Sort((a, b) => Comparer<TPriority>.Default.Compare(a.Priority, b.Priority));
        }
        
        public TElement Dequeue()
        {
            if (_items.Count == 0) return default;
            var item = _items[0];
            _items.RemoveAt(0);
            return item.Element;
        }
        
        public void Clear()
        {
            _items.Clear();
        }
    }
}
