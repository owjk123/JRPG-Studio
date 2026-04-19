// StatusEffectManager.cs - 状态效果管理器
// 管理所有状态效果的添加、移除、更新

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Battle.Status
{
    /// <summary>
    /// 状态效果管理器 - 单例模式
    /// 管理所有状态效果的完整生命周期
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        #region Singleton
        
        private static StatusEffectManager _instance;
        public static StatusEffectManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<StatusEffectManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("StatusEffectManager");
                        _instance = go.AddComponent<StatusEffectManager>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 状态效果添加事件
        /// </summary>
        public event Action<BattleUnit, StatusEffectInstance> OnStatusAdded;
        
        /// <summary>
        /// 状态效果移除事件
        /// </summary>
        public event Action<BattleUnit, StatusEffectInstance> OnStatusRemoved;
        
        /// <summary>
        /// 状态效果触发事件（如灼烧伤害）
        /// </summary>
        public event Action<BattleUnit, StatusEffectInstance, int> OnStatusTriggered;
        
        /// <summary>
        /// 状态效果刷新事件（叠加时）
        /// </summary>
        public event Action<BattleUnit, StatusEffectInstance, StatusEffectInstance> OnStatusRefreshed;
        
        #endregion
        
        #region Settings
        
        [Header("DoT设置")]
        [Tooltip("灼烧每回合伤害百分比")]
        [SerializeField] private float _burnDamagePercent = 0.08f;
        
        [Tooltip("中毒每回合伤害百分比")]
        [SerializeField] private float _poisonDamagePercent = 0.10f;
        
        [Tooltip("出血每回合伤害百分比")]
        [SerializeField] private float _bleedDamagePercent = 0.05f;
        
        [Header("控制状态设置")]
        [Tooltip("麻痹无法行动概率")]
        [SerializeField] private float _paralyzeFailChance = 0.3f;
        
        [Tooltip("混乱攻击队友概率")]
        [SerializeField] private float _confuseAllyChance = 0.3f;
        
        [Tooltip("魅惑攻击敌人概率")]
        [SerializeField] private float _charmAttackEnemyChance = 0.2f;
        
        [Header("Buff/Debuff数值")]
        [Tooltip("属性提升百分比")]
        [SerializeField] private float _statUpPercent = 0.3f;
        
        [Tooltip("属性下降百分比")]
        [SerializeField] private float _statDownPercent = 0.3f;
        
        [Tooltip("速度提升百分比")]
        [SerializeField] private float _speedUpPercent = 0.5f;
        
        [Tooltip("速度下降百分比")]
        [SerializeField] private float _speedDownPercent = 0.4f;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// 全局状态效果定义
        /// </summary>
        private Dictionary<StatusEffectType, StatusEffectDefinition> _definitions;
        
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
            
            InitializeDefinitions();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化状态效果定义
        /// </summary>
        public void Initialize()
        {
            InitializeDefinitions();
        }
        
        /// <summary>
        /// 获取状态效果定义
        /// </summary>
        public StatusEffectDefinition GetDefinition(StatusEffectType type)
        {
            return _definitions.TryGetValue(type, out var def) ? def : null;
        }
        
        /// <summary>
        /// 添加状态效果到单位
        /// </summary>
        public bool ApplyStatus(BattleUnit unit, StatusEffectInstance status)
        {
            if (unit == null || !unit.IsAlive || status == null)
                return false;
            
            var definition = GetDefinition(status.StatusType);
            if (definition == null)
            {
                Debug.LogWarning($"StatusEffectManager: No definition found for {status.StatusType}");
                return false;
            }
            
            // 检查免疫
            if (unit.IsImmuneTo(status.StatusType))
            {
                return false;
            }
            
            // 检查是否存在相同效果
            var existing = unit.GetStatus(status.StatusType);
            if (existing != null)
            {
                // 可叠加效果
                if (definition.Stackable)
                {
                    existing.StackCount++;
                    existing.RemainingDuration = Mathf.Max(existing.RemainingDuration, status.Duration);
                    OnStatusRefreshed?.Invoke(unit, existing, status);
                }
                // 刷新持续时间
                else if (definition.RefreshDuration)
                {
                    existing.RemainingDuration = status.Duration;
                    OnStatusRefreshed?.Invoke(unit, existing, status);
                }
                // 不叠加
                else
                {
                    return false;
                }
                
                return true;
            }
            
            // 应用新效果
            unit.AddStatus(status);
            
            // 触发立即效果
            ApplyImmediateEffect(unit, status);
            
            OnStatusAdded?.Invoke(unit, status);
            return true;
        }
        
        /// <summary>
        /// 移除状态效果
        /// </summary>
        public void RemoveStatus(BattleUnit unit, StatusEffectInstance status)
        {
            if (unit == null || status == null)
                return;
            
            // 触发移除效果
            ApplyRemoveEffect(unit, status);
            
            unit.RemoveStatus(status);
            OnStatusRemoved?.Invoke(unit, status);
        }
        
        /// <summary>
        /// 移除特定类型的所有状态
        /// </summary>
        public void RemoveStatusByType(BattleUnit unit, StatusEffectType type)
        {
            var status = unit.GetStatus(type);
            if (status != null)
            {
                RemoveStatus(unit, status);
            }
        }
        
        /// <summary>
        /// 回合开始处理
        /// </summary>
        public void OnTurnStart(BattleUnit unit)
        {
            if (unit == null || !unit.IsAlive)
                return;
            
            foreach (var status in unit.GetAllStatuses())
            {
                var def = GetDefinition(status.StatusType);
                if (def == null) continue;
                
                // 每回合开始的触发效果
                if (def.TriggersOnTurnStart)
                {
                    ProcessStatusEffect(unit, status);
                }
            }
        }
        
        /// <summary>
        /// 回合结束处理
        /// </summary>
        public void OnTurnEnd(BattleUnit unit)
        {
            if (unit == null || !unit.IsAlive)
                return;
            
            // 处理DoT效果
            ProcessDotEffects(unit);
            
            // 减少持续时间
            var statusesToRemove = new List<StatusEffectInstance>();
            
            foreach (var status in unit.GetAllStatuses())
            {
                status.RemainingDuration--;
                
                if (status.RemainingDuration <= 0)
                {
                    statusesToRemove.Add(status);
                }
            }
            
            // 移除过期效果
            foreach (var status in statusesToRemove)
            {
                RemoveStatus(unit, status);
            }
        }
        
        /// <summary>
        /// 受到伤害时处理
        /// </summary>
        public void OnDamageTaken(BattleUnit unit, int damage, BattleUnit attacker)
        {
            if (unit == null || !unit.IsAlive)
                return;
            
            // 检查沉睡被攻击唤醒
            var sleepStatus = unit.GetStatus(StatusEffectType.Sleep);
            if (sleepStatus != null)
            {
                RemoveStatus(unit, sleepStatus);
            }
            
            // 检查冰冻被攻击解除
            var freezeStatus = unit.GetStatus(StatusEffectType.Freeze);
            if (freezeStatus != null)
            {
                RemoveStatus(unit, freezeStatus);
            }
            
            // 检查反弹
            var reflectStatus = unit.GetStatus(StatusEffectType.Reflect);
            if (reflectStatus != null && attacker != null)
            {
                int reflectDamage = (int)(damage * 0.5f);
                attacker.TakeDamage(reflectDamage);
            }
        }
        
        /// <summary>
        /// 造成伤害时处理
        /// </summary>
        public void OnDamageDealt(BattleUnit unit, BattleUnit target, int damage)
        {
            if (unit == null || !unit.IsAlive)
                return;
            
            // 检查出血
            var bleedStatus = unit.GetStatus(StatusEffectType.Bleed);
            if (bleedStatus != null)
            {
                int bleedDamage = (int)(unit.Stats.MaxHp * _bleedDamagePercent);
                unit.TakeDamage(bleedDamage);
                OnStatusTriggered?.Invoke(unit, bleedStatus, bleedDamage);
            }
        }
        
        /// <summary>
        /// 检查控制状态影响
        /// </summary>
        public bool CheckControlStatus(BattleUnit unit)
        {
            if (unit == null || !unit.IsAlive)
                return false;
            
            // 眩晕
            if (unit.HasStatus(StatusEffectType.Stun))
                return true;
            
            // 冻结
            if (unit.HasStatus(StatusEffectType.Freeze))
                return true;
            
            // 石化
            if (unit.HasStatus(StatusEffectType.Petrify))
                return true;
            
            // 恐惧
            if (unit.HasStatus(StatusEffectType.Fear))
                return true;
            
            // 睡眠（概率跳过行动）
            if (unit.HasStatus(StatusEffectType.Sleep))
            {
                // 睡眠下回合有概率继续睡
                return UnityEngine.Random.value < 0.7f;
            }
            
            // 麻痹（概率无法行动）
            if (unit.HasStatus(StatusEffectType.Paralyze))
            {
                return UnityEngine.Random.value < _paralyzeFailChance;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取混乱后的实际目标
        /// </summary>
        public BattleUnit GetConfusedTarget(BattleUnit unit, List<BattleUnit> enemies, List<BattleUnit> allies)
        {
            // 混乱时可能攻击自己或队友
            if (UnityEngine.Random.value < _confuseAllyChance)
            {
                // 攻击自己
                if (UnityEngine.Random.value < 0.3f)
                    return unit;
                
                // 攻击队友
                if (allies.Count > 1)
                {
                    var validAllies = allies.Where(u => u != unit && u.IsAlive).ToList();
                    if (validAllies.Count > 0)
                        return validAllies[UnityEngine.Random.Range(0, validAllies.Count)];
                }
            }
            
            return null; // 正常选择目标
        }
        
        /// <summary>
        /// 获取魅惑后的实际目标
        /// </summary>
        public BattleUnit GetCharmedTarget(BattleUnit unit, List<BattleUnit> enemies, List<BattleUnit> allies)
        {
            if (UnityEngine.Random.value < _charmAttackEnemyChance)
            {
                // 攻击敌人
                if (enemies.Count > 0)
                    return enemies[UnityEngine.Random.Range(0, enemies.Count)];
            }
            
            return null; // 正常选择目标
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 初始化状态效果定义
        /// </summary>
        private void InitializeDefinitions()
        {
            _definitions = new Dictionary<StatusEffectType, StatusEffectDefinition>();
            
            // === 增益效果 (Buffs) ===
            AddDefinition(StatusEffectType.PowerUp, "力量提升", StatusEffectCategory.Buff, 
                color: Color.red, statModifiers: new StatModifier(atkPercent: _statUpPercent));
            
            AddDefinition(StatusEffectType.DefenseUp, "防御提升", StatusEffectCategory.Buff,
                color: Color.blue, statModifiers: new StatModifier(defPercent: _statUpPercent));
            
            AddDefinition(StatusEffectType.SpeedUp, "速度提升", StatusEffectCategory.Buff,
                color: Color.green, statModifiers: new StatModifier(spdPercent: _speedUpPercent));
            
            AddDefinition(StatusEffectType.MagicUp, "魔力提升", StatusEffectCategory.Buff,
                color: new Color(0.6f, 0.2f, 1f), statModifiers: new StatModifier(magPercent: _statUpPercent));
            
            AddDefinition(StatusEffectType.EvadeUp, "闪避提升", StatusEffectCategory.Buff,
                color: Color.white, statModifiers: new StatModifier(evadeFlat: 0.25f));
            
            AddDefinition(StatusEffectType.Regen, "再生", StatusEffectCategory.Buff,
                color: new Color(0.2f, 0.8f, 0.2f), triggersOnTurnEnd: true);
            
            AddDefinition(StatusEffectType.Shield, "护盾", StatusEffectCategory.Buff,
                color: new Color(0.3f, 0.5f, 0.9f), isShield: true);
            
            AddDefinition(StatusEffectType.Invincible, "无敌", StatusEffectCategory.Buff,
                color: Color.yellow);
            
            AddDefinition(StatusEffectType.Barrier, "魔法屏障", StatusEffectCategory.Buff,
                color: new Color(0.5f, 0.3f, 1f), isMagicShield: true);
            
            // === 减益效果 (Debuffs) ===
            AddDefinition(StatusEffectType.PowerDown, "攻击力下降", StatusEffectCategory.Debuff,
                color: new Color(0.8f, 0.3f, 0.3f), statModifiers: new StatModifier(atkPercent: -_statDownPercent));
            
            AddDefinition(StatusEffectType.DefenseDown, "防御力下降", StatusEffectCategory.Debuff,
                color: new Color(0.3f, 0.3f, 0.8f), statModifiers: new StatModifier(defPercent: -_statDownPercent));
            
            AddDefinition(StatusEffectType.SpeedDown, "速度下降", StatusEffectCategory.Debuff,
                color: new Color(0.3f, 0.8f, 0.3f), statModifiers: new StatModifier(spdPercent: -_speedDownPercent));
            
            AddDefinition(StatusEffectType.Burn, "灼烧", StatusEffectCategory.DoT,
                color: new Color(1f, 0.5f, 0f), triggersOnTurnEnd: true);
            
            AddDefinition(StatusEffectType.Poison, "中毒", StatusEffectCategory.DoT,
                color: new Color(0.3f, 0.8f, 0.3f), triggersOnTurnEnd: true);
            
            AddDefinition(StatusEffectType.Bleed, "出血", StatusEffectCategory.DoT,
                color: new Color(0.8f, 0.2f, 0.2f), triggersOnTurnEnd: true);
            
            AddDefinition(StatusEffectType.Curse, "诅咒", StatusEffectCategory.Debuff,
                color: new Color(0.5f, 0f, 0.5f), statModifiers: new StatModifier(healPercent: -0.5f));
            
            // === 控制效果 ===
            AddDefinition(StatusEffectType.Stun, "眩晕", StatusEffectCategory.Control,
                color: Color.yellow, isControlEffect: true, stackable: false);
            
            AddDefinition(StatusEffectType.Freeze, "冻结", StatusEffectCategory.Control,
                color: new Color(0.5f, 0.8f, 1f), isControlEffect: true, stackable: false,
                removedOnHit: true);
            
            AddDefinition(StatusEffectType.Sleep, "睡眠", StatusEffectCategory.Control,
                color: new Color(0.5f, 0.5f, 0.8f), isControlEffect: true, stackable: false,
                removedOnHit: true);
            
            AddDefinition(StatusEffectType.Paralyze, "麻痹", StatusEffectCategory.Control,
                color: Color.yellow, isControlEffect: true);
            
            AddDefinition(StatusEffectType.Confuse, "混乱", StatusEffectCategory.Control,
                color: new Color(0.6f, 0.3f, 0.8f), isControlEffect: true);
            
            AddDefinition(StatusEffectType.Charm, "魅惑", StatusEffectCategory.Control,
                color: new Color(1f, 0.5f, 0.7f), isControlEffect: true);
            
            AddDefinition(StatusEffectType.Fear, "恐惧", StatusEffectCategory.Control,
                color: new Color(0.3f, 0.3f, 0.3f), isControlEffect: true);
            
            AddDefinition(StatusEffectType.Petrify, "石化", StatusEffectCategory.Control,
                color: new Color(0.6f, 0.6f, 0.6f), isControlEffect: true, stackable: false);
            
            AddDefinition(StatusEffectType.Silence, "沉默", StatusEffectCategory.Debuff,
                color: new Color(0.5f, 0.5f, 0.5f), preventsAction: ActionType.Skill);
            
            AddDefinition(StatusEffectType.Bind, "束缚", StatusEffectCategory.Control,
                color: new Color(0.3f, 0.5f, 0.3f), isControlEffect: true);
        }
        
        /// <summary>
        /// 添加状态效果定义
        /// </summary>
        private void AddDefinition(
            StatusEffectType type, 
            string name, 
            StatusEffectCategory category,
            Color? color = null,
            StatModifier statModifiers = default,
            bool triggersOnTurnEnd = false,
            bool triggersOnTurnStart = false,
            bool isControlEffect = false,
            bool stackable = true,
            bool refreshDuration = true,
            bool removedOnHit = false,
            bool isShield = false,
            bool isMagicShield = false,
            bool isPhysicalShield = false,
            ActionType? preventsAction = null)
        {
            _definitions[type] = new StatusEffectDefinition
            {
                Type = type,
                Name = name,
                Category = category,
                Color = color ?? Color.white,
                StatModifiers = statModifiers,
                TriggersOnTurnEnd = triggersOnTurnEnd,
                TriggersOnTurnStart = triggersOnTurnStart,
                IsControlEffect = isControlEffect,
                Stackable = stackable,
                RefreshDuration = refreshDuration,
                RemovedOnHit = removedOnHit,
                IsShield = isShield,
                IsMagicShield = isMagicShield,
                IsPhysicalShield = isPhysicalShield,
                PreventsAction = preventsAction
            };
        }
        
        /// <summary>
        /// 应用立即效果
        /// </summary>
        private void ApplyImmediateEffect(BattleUnit unit, StatusEffectInstance status)
        {
            var def = GetDefinition(status.StatusType);
            if (def == null) return;
            
            // 应用属性修改
            if (def.StatModifiers.HasModifiers)
            {
                unit.ApplyStatModifiers(def.StatModifiers);
            }
        }
        
        /// <summary>
        /// 应用移除效果
        /// </summary>
        private void ApplyRemoveEffect(BattleUnit unit, StatusEffectInstance status)
        {
            var def = GetDefinition(status.StatusType);
            if (def == null) return;
            
            // 移除属性修改
            if (def.StatModifiers.HasModifiers)
            {
                unit.RemoveStatModifiers(def.StatModifiers);
            }
        }
        
        /// <summary>
        /// 处理状态效果触发
        /// </summary>
        private void ProcessStatusEffect(BattleUnit unit, StatusEffectInstance status)
        {
            var def = GetDefinition(status.StatusType);
            if (def == null) return;
            
            if (def.TriggersOnTurnEnd)
            {
                int damage = CalculateDotDamage(unit, status.StatusType);
                unit.TakeDamage(damage);
                OnStatusTriggered?.Invoke(unit, status, damage);
            }
            
            if (def.TriggersOnTurnStart && def.Category == StatusEffectCategory.Buff)
            {
                // 再生效果
                if (status.StatusType == StatusEffectType.Regen)
                {
                    int heal = (int)(unit.Stats.MaxHp * 0.05f);
                    unit.Heal(heal);
                }
            }
        }
        
        /// <summary>
        /// 处理持续伤害效果
        /// </summary>
        private void ProcessDotEffects(BattleUnit unit)
        {
            int totalDamage = 0;
            
            // 灼烧
            if (unit.HasStatus(StatusEffectType.Burn))
            {
                totalDamage += CalculateDotDamage(unit, StatusEffectType.Burn);
            }
            
            // 中毒
            if (unit.HasStatus(StatusEffectType.Poison))
            {
                totalDamage += CalculateDotDamage(unit, StatusEffectType.Poison);
            }
            
            // 出血（在攻击时处理，不在回合结束）
            
            if (totalDamage > 0)
            {
                unit.TakeDamage(totalDamage);
            }
        }
        
        /// <summary>
        /// 计算持续伤害
        /// </summary>
        private int CalculateDotDamage(BattleUnit unit, StatusEffectType dotType)
        {
            float maxHp = unit.Stats.MaxHp;
            
            switch (dotType)
            {
                case StatusEffectType.Burn:
                    return (int)(maxHp * _burnDamagePercent);
                case StatusEffectType.Poison:
                    return (int)(maxHp * _poisonDamagePercent);
                case StatusEffectType.Bleed:
                    return (int)(maxHp * _bleedDamagePercent);
                default:
                    return 0;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 状态效果定义
    /// </summary>
    [Serializable]
    public class StatusEffectDefinition
    {
        public StatusEffectType Type;
        public string Name;
        public StatusEffectCategory Category;
        public Color Color;
        public StatModifier StatModifiers;
        public bool TriggersOnTurnEnd;
        public bool TriggersOnTurnStart;
        public bool IsControlEffect;
        public bool Stackable;
        public bool RefreshDuration;
        public bool RemovedOnHit;
        public bool IsShield;
        public bool IsMagicShield;
        public bool IsPhysicalShield;
        public ActionType? PreventsAction;
    }
    
    /// <summary>
    /// 属性修改器
    /// </summary>
    [Serializable]
    public struct StatModifier
    {
        public int atkFlat;
        public int defFlat;
        public int magFlat;
        public int spdFlat;
        public float atkPercent;
        public float defPercent;
        public float magPercent;
        public float spdPercent;
        public float critRate;
        public float evadeFlat;
        public float healPercent;
        
        public bool HasModifiers =>
            atkFlat != 0 || defFlat != 0 || magFlat != 0 || spdFlat != 0 ||
            atkPercent != 0 || defPercent != 0 || magPercent != 0 || spdPercent != 0 ||
            critRate != 0 || evadeFlat != 0 || healPercent != 0;
    }
    
    /// <summary>
    /// 状态效果实例
    /// </summary>
    [Serializable]
    public class StatusEffectInstance
    {
        public StatusEffectType StatusType;
        public int Duration;
        public int RemainingDuration => Duration;
        public StatusSource Source;
        public float Intensity;
        public int StackCount = 1;
        public string SourceSkillId;
        
        /// <summary>
        /// 剩余持续时间
        /// </summary>
        public int RemainingTime;
        
        /// <summary>
        /// 创建状态效果实例
        /// </summary>
        public static StatusEffectInstance Create(StatusEffectType type, int duration, StatusSource source = StatusSource.Skill)
        {
            return new StatusEffectInstance
            {
                StatusType = type,
                Duration = duration,
                RemainingTime = duration,
                Source = source,
                Intensity = 1f,
                StackCount = 1
            };
        }
    }
}
