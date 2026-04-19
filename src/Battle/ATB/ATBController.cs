// ATBController.cs - ATB行动条管理器
// 控制角色行动条的填充、状态管理和行动排序

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Battle.ATB
{
    /// <summary>
    /// ATB行动条管理器 - 单例模式
    /// 控制所有战斗单位的ATB槽填充和行动排序
    /// </summary>
    public class ATBController : MonoBehaviour
    {
        #region Singleton
        
        private static ATBController _instance;
        public static ATBController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ATBController>();
                    if (_instance == null)
                    {
                        var go = new GameObject("ATBController");
                        _instance = go.AddComponent<ATBController>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// ATB更新事件（每帧）
        /// </summary>
        public event Action<BattleUnit, float> OnATBUpdate;
        
        /// <summary>
        /// 单位ATB满事件（可以行动）
        /// </summary>
        public event Action<BattleUnit> OnATBFull;
        
        /// <summary>
        /// 单位行动完成事件
        /// </summary>
        public event Action<BattleUnit> OnActionComplete;
        
        /// <summary>
        /// 下一个行动单位变化事件
        /// </summary>
        public event Action<BattleUnit> OnNextActorChanged;
        
        #endregion
        
        #region Settings
        
        [Header("ATB设置")]
        [Tooltip("ATB最大充能值")]
        [SerializeField] private float _maxATBValue = 100f;
        
        [Tooltip("基础充能速度（单位/秒）")]
        [SerializeField] private float _baseChargeSpeed = 20f;
        
        [Tooltip("速度影响系数")]
        [SerializeField] private float _speedInfluence = 0.5f;
        
        [Tooltip("行动后ATB重置值")]
        [SerializeField] private float _atbResetValue = 0f;
        
        [Tooltip("行动后ATB重置延迟")]
        [SerializeField] private float _resetDelay = 0.5f;
        
        [Tooltip("是否启用加速战斗模式")]
        [SerializeField] private bool _fastBattleMode = false;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// 所有战斗单位的ATB数据
        /// </summary>
        private Dictionary<BattleUnit, ATBData> _atbDataMap = new Dictionary<BattleUnit, ATBData>();
        
        /// <summary>
        /// 当前可以行动的单位列表
        /// </summary>
        private List<BattleUnit> _readyUnits = new List<BattleUnit>();
        
        /// <summary>
        /// 当前正在行动的单位
        /// </summary>
        private BattleUnit _currentActor;
        
        /// <summary>
        /// 行动顺序队列
        /// </summary>
        private List<BattleUnit> _actionOrder = new List<BattleUnit>();
        
        /// <summary>
        /// 战斗是否在进行
        /// </summary>
        private bool _isBattleActive = false;
        
        /// <summary>
        /// 时间缩放（用于加速）
        /// </summary>
        private float _timeScale = 1f;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 最大ATB值
        /// </summary>
        public float MaxATBValue => _maxATBValue;
        
        /// <summary>
        /// 当前时间缩放
        /// </summary>
        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Max(0f, Mathf.Min(3f, value));
        }
        
        /// <summary>
        /// 是否正在战斗
        /// </summary>
        public bool IsBattleActive => _isBattleActive;
        
        /// <summary>
        /// 当前行动单位
        /// </summary>
        public BattleUnit CurrentActor => _currentActor;
        
        /// <summary>
        /// 待命单位列表
        /// </summary>
        public IReadOnlyList<BattleUnit> ReadyUnits => _readyUnits;
        
        /// <summary>
        /// 行动顺序
        /// </summary>
        public IReadOnlyList<BattleUnit> ActionOrder => _actionOrder;
        
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
        }
        
        private void Update()
        {
            if (!_isBattleActive) return;
            
            float deltaTime = Time.deltaTime * _timeScale;
            if (_fastBattleMode) deltaTime *= 2f;
            
            UpdateATB(deltaTime);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化ATB系统
        /// </summary>
        public void Initialize()
        {
            _atbDataMap.Clear();
            _readyUnits.Clear();
            _actionOrder.Clear();
            _currentActor = null;
            _isBattleActive = true;
            _timeScale = 1f;
        }
        
        /// <summary>
        /// 注册战斗单位
        /// </summary>
        public void RegisterUnit(BattleUnit unit)
        {
            if (unit == null || _atbDataMap.ContainsKey(unit)) return;
            
            var atbData = new ATBData
            {
                Unit = unit,
                CurrentValue = UnityEngine.Random.Range(0f, _maxATBValue * 0.3f), // 随机初始位置
                State = ATBState.Charging,
                SpeedMultiplier = 1f,
                IsPaused = false
            };
            
            _atbDataMap[unit] = atbData;
            
            // 订阅单位死亡事件
            unit.OnDeath += OnUnitDeath;
            unit.OnStatusAdded += OnUnitStatusAdded;
        }
        
        /// <summary>
        /// 注销战斗单位
        /// </summary>
        public void UnregisterUnit(BattleUnit unit)
        {
            if (unit == null) return;
            
            _atbDataMap.Remove(unit);
            _readyUnits.Remove(unit);
            _actionOrder.Remove(unit);
            
            unit.OnDeath -= OnUnitDeath;
            unit.OnStatusAdded -= OnUnitStatusAdded;
        }
        
        /// <summary>
        /// 获取单位的ATB数据
        /// </summary>
        public ATBData GetATBData(BattleUnit unit)
        {
            return _atbDataMap.TryGetValue(unit, out var data) ? data : null;
        }
        
        /// <summary>
        /// 获取单位的ATB当前值
        /// </summary>
        public float GetATBValue(BattleUnit unit)
        {
            return _atbDataMap.TryGetValue(unit, out var data) ? data.CurrentValue : 0f;
        }
        
        /// <summary>
        /// 获取单位的ATB百分比 (0-1)
        /// </summary>
        public float GetATBPercent(BattleUnit unit)
        {
            if (_atbDataMap.TryGetValue(unit, out var data))
            {
                return Mathf.Clamp01(data.CurrentValue / _maxATBValue);
            }
            return 0f;
        }
        
        /// <summary>
        /// 获取下一个行动单位
        /// </summary>
        public BattleUnit GetNextActor()
        {
            if (_readyUnits.Count == 0) return null;
            
            // 按ATB值排序，返回最高的
            _readyUnits.Sort((a, b) => 
                GetATBValue(b).CompareTo(GetATBValue(a)));
            
            return _readyUnits[0];
        }
        
        /// <summary>
        /// 设置单位为正在行动状态
        /// </summary>
        public void SetUnitActing(BattleUnit unit)
        {
            if (!_atbDataMap.TryGetValue(unit, out var data)) return;
            
            data.State = ATBState.Acting;
            _currentActor = unit;
            _readyUnits.Remove(unit);
            
            OnNextActorChanged?.Invoke(unit);
        }
        
        /// <summary>
        /// 完成单位行动，重置ATB
        /// </summary>
        public void CompleteAction(BattleUnit unit)
        {
            if (!_atbDataMap.TryGetValue(unit, out var data)) return;
            
            data.CurrentValue = _atbResetValue;
            data.State = ATBState.Charging;
            data.ActionCount++;
            
            _currentActor = null;
            
            OnActionComplete?.Invoke(unit);
        }
        
        /// <summary>
        /// 强制设置单位ATB值
        /// </summary>
        public void SetATBValue(BattleUnit unit, float value)
        {
            if (!_atbDataMap.TryGetValue(unit, out var data)) return;
            
            data.CurrentValue = Mathf.Clamp(value, 0f, _maxATBValue);
        }
        
        /// <summary>
        /// 增加单位ATB值
        /// </summary>
        public void AddATBValue(BattleUnit unit, float value)
        {
            if (!_atbDataMap.TryGetValue(unit, out var data)) return;
            
            data.CurrentValue = Mathf.Clamp(data.CurrentValue + value, 0f, _maxATBValue);
        }
        
        /// <summary>
        /// 暂停单位ATB充能
        /// </summary>
        public void PauseUnit(BattleUnit unit)
        {
            if (_atbDataMap.TryGetValue(unit, out var data))
            {
                data.IsPaused = true;
            }
        }
        
        /// <summary>
        /// 恢复单位ATB充能
        /// </summary>
        public void ResumeUnit(BattleUnit unit)
        {
            if (_atbDataMap.TryGetValue(unit, out var data))
            {
                data.IsPaused = false;
            }
        }
        
        /// <summary>
        /// 设置速度倍率
        /// </summary>
        public void SetSpeedMultiplier(BattleUnit unit, float multiplier)
        {
            if (_atbDataMap.TryGetValue(unit, out var data))
            {
                data.SpeedMultiplier = multiplier;
            }
        }
        
        /// <summary>
        /// 停止战斗
        /// </summary>
        public void StopBattle()
        {
            _isBattleActive = false;
            _currentActor = null;
        }
        
        /// <summary>
        /// 重置ATB系统
        /// </summary>
        public void Reset()
        {
            _atbDataMap.Clear();
            _readyUnits.Clear();
            _actionOrder.Clear();
            _currentActor = null;
            _isBattleActive = false;
        }
        
        /// <summary>
        /// 强制填充指定单位的ATB（用于某些技能效果）
        /// </summary>
        public void ForceFillATB(BattleUnit unit, float percent = 1f)
        {
            if (!_atbDataMap.TryGetValue(unit, out var data)) return;
            
            data.CurrentValue = _maxATBValue * percent;
        }
        
        /// <summary>
        /// 减少指定单位的ATB（用于某些debuff效果）
        /// </summary>
        public void DrainATB(BattleUnit unit, float percent)
        {
            if (!_atbDataMap.TryGetValue(unit, out var data)) return;
            
            data.CurrentValue -= _maxATBValue * percent;
            data.CurrentValue = Mathf.Max(0f, data.CurrentValue);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 更新所有ATB槽
        /// </summary>
        private void UpdateATB(float deltaTime)
        {
            foreach (var kvp in _atbDataMap)
            {
                var unit = kvp.Key;
                var data = kvp.Value;
                
                // 跳过无效状态
                if (data.State == ATBState.Acting || 
                    data.State == ATBState.Disabled ||
                    data.IsPaused ||
                    !unit.IsAlive)
                {
                    continue;
                }
                
                // 检查控制状态
                if (unit.HasControlImpairingStatus())
                {
                    continue;
                }
                
                // 计算充能速度
                float chargeSpeed = CalculateChargeSpeed(unit, data);
                
                // 更新ATB值
                data.CurrentValue += chargeSpeed * deltaTime;
                
                // 触发更新事件
                OnATBUpdate?.Invoke(unit, data.CurrentValue);
                
                // 检查是否满
                if (data.CurrentValue >= _maxATBValue)
                {
                    if (data.State != ATBState.Ready)
                    {
                        data.CurrentValue = _maxATBValue;
                        data.State = ATBState.Ready;
                        
                        if (!_readyUnits.Contains(unit))
                        {
                            _readyUnits.Add(unit);
                        }
                        
                        OnATBFull?.Invoke(unit);
                    }
                }
            }
        }
        
        /// <summary>
        /// 计算ATB充能速度
        /// </summary>
        private float CalculateChargeSpeed(BattleUnit unit, ATBData data)
        {
            // 基础速度
            float speed = unit.Stats.Spd;
            
            // 速度影响
            float speedBonus = speed * _speedInfluence / 100f;
            
            // 速度状态加成
            float statusBonus = 0f;
            if (unit.HasStatus(StatusEffectType.SpeedUp))
                statusBonus = 0.5f;
            if (unit.HasStatus(StatusEffectType.SpeedDown))
                statusBonus = -0.4f;
            
            // 计算最终速度
            float finalSpeed = (_baseChargeSpeed + speedBonus) * (1f + statusBonus) * data.SpeedMultiplier;
            
            return finalSpeed;
        }
        
        /// <summary>
        /// 单位死亡回调
        /// </summary>
        private void OnUnitDeath(BattleUnit unit)
        {
            if (_atbDataMap.TryGetValue(unit, out var data))
            {
                data.State = ATBState.Disabled;
            }
            
            _readyUnits.Remove(unit);
            _actionOrder.Remove(unit);
            
            if (_currentActor == unit)
            {
                _currentActor = null;
            }
        }
        
        /// <summary>
        /// 状态效果添加回调
        /// </summary>
        private void OnUnitStatusAdded(BattleUnit unit, StatusEffectInstance status)
        {
            // 根据状态类型调整ATB
            switch (status.StatusType)
            {
                case StatusEffectType.Slow:
                case StatusEffectType.SpeedDown:
                    SetSpeedMultiplier(unit, 0.6f);
                    break;
                    
                case StatusEffectType.SpeedUp:
                    SetSpeedMultiplier(unit, 1.5f);
                    break;
            }
        }
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// 获取ATB调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine("=== ATB Debug Info ===");
            lines.AppendLine($"Max Value: {_maxATBValue}");
            lines.AppendLine($"Active: {_isBattleActive}");
            lines.AppendLine($"Current Actor: {_currentActor?.name ?? "None"}");
            lines.AppendLine($"Ready Units: {_readyUnits.Count}");
            
            foreach (var kvp in _atbDataMap)
            {
                var data = kvp.Value;
                lines.AppendLine($"  {kvp.Key.name}: {data.CurrentValue:F1}/{_maxATBValue} [{data.State}]");
            }
            
            return lines.ToString();
        }
        
        #endregion
    }
    
    /// <summary>
    /// ATB数据
    /// </summary>
    [Serializable]
    public class ATBData
    {
        /// <summary>
        /// 关联的单位
        /// </summary>
        public BattleUnit Unit;
        
        /// <summary>
        /// 当前ATB值
        /// </summary>
        public float CurrentValue;
        
        /// <summary>
        /// ATB状态
        /// </summary>
        public ATBState State;
        
        /// <summary>
        /// 速度倍率
        /// </summary>
        public float SpeedMultiplier = 1f;
        
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused;
        
        /// <summary>
        /// 行动次数统计
        /// </summary>
        public int ActionCount;
    }
}
