// BattleUnit.cs - 战斗单位
// 回合制战斗中的参战单位，管理属性、状态效果、行动等

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Battle
{
    /// <summary>
    /// 战斗单位 - 回合制战斗中的参战单位
    /// </summary>
    public class BattleUnit : MonoBehaviour
    {
        #region Events
        
        /// <summary>
        /// 受伤事件
        /// </summary>
        public event Action<BattleUnit, int> OnDamageTaken;
        
        /// <summary>
        /// 死亡事件
        /// </summary>
        public event Action<BattleUnit> OnDeath;
        
        /// <summary>
        /// 复活事件
        /// </summary>
        public event Action<BattleUnit> OnRevive;
        
        /// <summary>
        /// HP变化事件
        /// </summary>
        public event Action<BattleUnit, int, int> OnHPChanged;
        
        /// <summary>
        /// MP变化事件
        /// </summary>
        public event Action<BattleUnit, int, int> OnMPChanged;
        
        /// <summary>
        /// 状态效果添加事件
        /// </summary>
        public event Action<BattleUnit, StatusEffectInstance> OnStatusAdded;
        
        /// <summary>
        /// 状态效果移除事件
        /// </summary>
        public event Action<BattleUnit, StatusEffectInstance> OnStatusRemoved;
        
        /// <summary>
        /// 回合开始事件
        /// </summary>
        public event Action<BattleUnit> OnTurnStart;
        
        /// <summary>
        /// 回合结束事件
        /// </summary>
        public event Action<BattleUnit> OnTurnEnd;
        
        #endregion
        
        #region Fields
        
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        // 基础数据
        private CharacterData _characterData;
        private bool _isPlayerControlled;
        
        // 当前属性
        private int _currentLevel;
        private int _currentHp;
        private int _currentMp;
        private int _ultimateEnergy;
        
        // 状态效果
        private List<StatusEffectInstance> _statusEffects = new List<StatusEffectInstance>();
        
        // 行动状态
        private bool _hasActedThisTurn = false;
        
        // 护盾
        private int _physicalShield;
        private int _magicShield;
        private int _shieldValue;
        private Element _elementShield = Element.None;
        
        // 冷却
        private Dictionary<int, int> _skillCooldowns = new Dictionary<int, int>();
        
        // 属性修正
        private StatModifier _activeStatModifiers = new StatModifier();
        private Element? _absorbElement = null;
        private float _absorbPercent = 0f;
        
        // 反击数据
        private float _counterPercent = 0f;
        private int _counterCount = 0;
        
        #endregion
        
        #region Properties
        
        public CharacterData CharacterData => _characterData;
        public bool IsPlayerControlled => _isPlayerControlled;
        public int CurrentLevel => _currentLevel;
        public int CurrentHp => _currentHp;
        public int CurrentMp => _currentMp;
        public int UltimateEnergy => _ultimateEnergy;
        
        public bool IsAlive => _currentHp > 0;
        public bool CanAct => IsAlive && !HasStatus(StatusEffectType.Stun) && !HasStatus(StatusEffectType.Sleep);
        public bool CanMove => IsAlive;
        
        /// <summary>
        /// 计算后属性（包含状态效果修正）
        /// </summary>
        public CharacterStats Stats => CalculateFinalStats();
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化战斗单位
        /// </summary>
        public void Initialize(CharacterData data, bool isPlayerControlled)
        {
            _characterData = data;
            _isPlayerControlled = isPlayerControlled;
            _currentLevel = 1;
            
            var baseStats = data.GetStatsAtLevel(_currentLevel);
            _currentHp = baseStats.MaxHp;
            _currentMp = baseStats.MaxMp;
            _ultimateEnergy = 0;
            
            _statusEffects.Clear();
            _skillCooldowns.Clear();
            _hasActedThisTurn = false;
            
            // 清除护盾
            ClearAllShields();
            
            // 清除属性修正
            _activeStatModifiers = new StatModifier();
            _absorbElement = null;
            _absorbPercent = 0f;
        }
        
        #region 伤害与治疗
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int amount, Element? element = null)
        {
            if (!IsAlive) return;
            
            int oldHp = _currentHp;
            
            // 计算最终伤害
            int finalDamage = amount;
            
            // 护盾吸收
            finalDamage = ReduceByShield(finalDamage, element);
            
            // 无敌检查
            if (HasStatus(StatusEffectType.Invincible))
            {
                finalDamage = 0;
            }
            
            _currentHp = Mathf.Max(0, _currentHp - finalDamage);
            
            // 触发事件
            OnDamageTaken?.Invoke(this, finalDamage);
            OnHPChanged?.Invoke(this, oldHp, _currentHp);
            
            // 播放受击动画
            PlayAnimation(BattleAnimation.Hit);
            
            // 检查死亡
            if (!IsAlive)
            {
                OnDeath?.Invoke(this);
                PlayAnimation(BattleAnimation.Die);
            }
        }
        
        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(int amount)
        {
            if (!IsAlive) return;
            
            int oldHp = _currentHp;
            _currentHp = Mathf.Min(Stats.MaxHp, _currentHp + amount);
            
            OnHPChanged?.Invoke(this, oldHp, _currentHp);
            
            // 播放治疗动画
            PlayAnimation(BattleAnimation.Heal);
        }
        
        /// <summary>
        /// 复活
        /// </summary>
        public void Revive(float hpPercent)
        {
            if (IsAlive) return;
            
            _currentHp = (int)(Stats.MaxHp * hpPercent);
            
            OnRevive?.Invoke(this);
            PlayAnimation(BattleAnimation.Revive);
        }
        
        /// <summary>
        /// 消耗MP
        /// </summary>
        public bool ConsumeMP(int amount)
        {
            if (_currentMp >= amount)
            {
                _currentMp -= amount;
                OnMPChanged?.Invoke(this, _currentMp + amount, _currentMp);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 恢复MP
        /// </summary>
        public void RestoreMP(int amount)
        {
            int oldMp = _currentMp;
            _currentMp = Mathf.Min(Stats.MaxMp, _currentMp + amount);
            OnMPChanged?.Invoke(this, oldMp, _currentMp);
        }
        
        /// <summary>
        /// 消耗终极能量
        /// </summary>
        public bool ConsumeUltimateEnergy(int amount)
        {
            if (_ultimateEnergy >= amount)
            {
                _ultimateEnergy -= amount;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 增加终极能量
        /// </summary>
        public void AddUltimateEnergy(int amount)
        {
            _ultimateEnergy = Mathf.Min(100, _ultimateEnergy + amount);
        }
        
        #endregion
        
        #region 状态效果
        
        /// <summary>
        /// 应用状态效果
        /// </summary>
        public void ApplyStatus(StatusEffectInstance status)
        {
            // 检查免疫
            if (IsImmuneTo(status.StatusType))
                return;
            
            // 检查是否已存在
            var existing = GetStatus(status.StatusType);
            if (existing != null)
            {
                existing.RemainingTime = status.Duration;
                existing.StackCount++;
                existing.Intensity = Mathf.Max(existing.Intensity, status.Intensity);
            }
            else
            {
                var newStatus = new StatusEffectInstance
                {
                    StatusType = status.StatusType,
                    Duration = status.Duration,
                    RemainingTime = status.Duration,
                    Source = status.Source,
                    Intensity = status.Intensity,
                    StackCount = status.StackCount,
                    SourceSkillId = status.SourceSkillId
                };
                _statusEffects.Add(newStatus);
            }
            
            OnStatusAdded?.Invoke(this, status);
            PlayAnimation(BattleAnimation.Buff);
        }
        
        /// <summary>
        /// 移除状态效果
        /// </summary>
        public void RemoveStatus(StatusEffectInstance status)
        {
            if (_statusEffects.Remove(status))
            {
                OnStatusRemoved?.Invoke(this, status);
            }
        }
        
        /// <summary>
        /// 获取状态效果
        /// </summary>
        public StatusEffectInstance GetStatus(StatusEffectType type)
        {
            return _statusEffects.FirstOrDefault(s => s.StatusType == type);
        }
        
        /// <summary>
        /// 检查是否有指定状态
        /// </summary>
        public bool HasStatus(StatusEffectType type)
        {
            return _statusEffects.Any(s => s.StatusType == type);
        }
        
        /// <summary>
        /// 获取所有状态效果
        /// </summary>
        public List<StatusEffectInstance> GetAllStatuses()
        {
            return _statusEffects.ToList();
        }
        
        /// <summary>
        /// 检查是否有控制类状态
        /// </summary>
        public bool HasControlImpairingStatus()
        {
            return _statusEffects.Any(s => 
                s.StatusType == StatusEffectType.Stun ||
                s.StatusType == StatusEffectType.Freeze ||
                s.StatusType == StatusEffectType.Petrify ||
                s.StatusType == StatusEffectType.Fear);
        }
        
        /// <summary>
        /// 检查是否免疫指定状态
        /// </summary>
        public bool IsImmuneTo(StatusEffectType type)
        {
            // 天使族免疫即死
            if (_characterData.race == Race.Angel && type == StatusEffectType.InstantDeath)
                return true;
            
            // 检查装备/被动免疫
            // TODO: 从装备和被动技能获取
            
            return false;
        }
        
        /// <summary>
        /// 移除所有debuff
        /// </summary>
        public void RemoveDebuffs(int count = int.MaxValue)
        {
            var debuffs = _statusEffects
                .Where(s => IsDebuff(s.StatusType))
                .Take(count)
                .ToList();
            
            foreach (var debuff in debuffs)
            {
                RemoveStatus(debuff);
            }
        }
        
        /// <summary>
        /// 移除所有buff
        /// </summary>
        public void RemoveBuffs(int count = int.MaxValue)
        {
            var buffs = _statusEffects
                .Where(s => IsBuff(s.StatusType))
                .Take(count)
                .ToList();
            
            foreach (var buff in buffs)
            {
                RemoveStatus(buff);
            }
        }
        
        /// <summary>
        /// 移除所有状态
        /// </summary>
        public void ClearAllStatuses()
        {
            var all = _statusEffects.ToList();
            _statusEffects.Clear();
            
            foreach (var status in all)
            {
                OnStatusRemoved?.Invoke(this, status);
            }
        }
        
        private bool IsDebuff(StatusEffectType type)
        {
            return type >= StatusEffectType.PowerDown && type <= StatusEffectType.Slow ||
                   type == StatusEffectType.Paralyze || type == StatusEffectType.Confuse ||
                   type == StatusEffectType.Charm || type == StatusEffectType.Fear ||
                   type == StatusEffectType.Burn || type == StatusEffectType.Poison ||
                   type == StatusEffectType.Bleed || type == StatusEffectType.Curse ||
                   type == StatusEffectType.Silence;
        }
        
        private bool IsBuff(StatusEffectType type)
        {
            return type >= StatusEffectType.PowerUp && type <= StatusEffectType.Invincible ||
                   type == StatusEffectType.Shield || type == StatusEffectType.Barrier ||
                   type == StatusEffectType.Regen || type == StatusEffectType.EvadeUp;
        }
        
        #endregion
        
        #region 护盾
        
        /// <summary>
        /// 添加护盾
        /// </summary>
        public void AddShield(int value, bool isPhysical = false, bool isMagic = false, Element element = Element.None)
        {
            _shieldValue += value;
            
            if (isPhysical)
                _physicalShield += value;
            if (isMagic)
                _magicShield += value;
            if (element != Element.None)
                _elementShield = element;
        }
        
        /// <summary>
        /// 获取护盾值
        /// </summary>
        public int GetShieldValue()
        {
            return _shieldValue;
        }
        
        /// <summary>
        /// 减少护盾
        /// </summary>
        public void ReduceShield(int amount)
        {
            _shieldValue = Mathf.Max(0, _shieldValue - amount);
            _physicalShield = Mathf.Max(0, _physicalShield - amount);
            _magicShield = Mathf.Max(0, _magicShield - amount);
            
            if (_shieldValue <= 0)
                _elementShield = Element.None;
        }
        
        /// <summary>
        /// 清除所有护盾
        /// </summary>
        public void ClearAllShields()
        {
            _shieldValue = 0;
            _physicalShield = 0;
            _magicShield = 0;
            _elementShield = Element.None;
        }
        
        /// <summary>
        /// 通过护盾减少伤害
        /// </summary>
        private int ReduceByShield(int damage, Element? element)
        {
            int reduced = damage;
            
            // 元素护盾
            if (element.HasValue && _elementShield == element.Value && _shieldValue > 0)
            {
                int absorbed = Mathf.Min(_shieldValue, reduced);
                reduced -= absorbed;
                _shieldValue -= absorbed;
            }
            
            // 普通护盾
            if (reduced > 0 && _shieldValue > 0)
            {
                int absorbed = Mathf.Min(_shieldValue, reduced);
                reduced -= absorbed;
                _shieldValue -= absorbed;
            }
            
            return reduced;
        }
        
        #endregion
        
        #region 属性修改
        
        /// <summary>
        /// 应用属性修改器
        /// </summary>
        public void ApplyStatModifiers(StatModifier modifiers)
        {
            _activeStatModifiers.atkFlat += modifiers.atkFlat;
            _activeStatModifiers.defFlat += modifiers.defFlat;
            _activeStatModifiers.magFlat += modifiers.magFlat;
            _activeStatModifiers.spdFlat += modifiers.spdFlat;
            _activeStatModifiers.atkPercent += modifiers.atkPercent;
            _activeStatModifiers.defPercent += modifiers.defPercent;
            _activeStatModifiers.magPercent += modifiers.magPercent;
            _activeStatModifiers.spdPercent += modifiers.spdPercent;
            _activeStatModifiers.critRate += modifiers.critRate;
            _activeStatModifiers.evadeFlat += modifiers.evadeFlat;
            _activeStatModifiers.healPercent += modifiers.healPercent;
        }
        
        /// <summary>
        /// 移除属性修改器
        /// </summary>
        public void RemoveStatModifiers(StatModifier modifiers)
        {
            _activeStatModifiers.atkFlat -= modifiers.atkFlat;
            _activeStatModifiers.defFlat -= modifiers.defFlat;
            _activeStatModifiers.magFlat -= modifiers.magFlat;
            _activeStatModifiers.spdFlat -= modifiers.spdFlat;
            _activeStatModifiers.atkPercent -= modifiers.atkPercent;
            _activeStatModifiers.defPercent -= modifiers.defPercent;
            _activeStatModifiers.magPercent -= modifiers.magPercent;
            _activeStatModifiers.spdPercent -= modifiers.spdPercent;
            _activeStatModifiers.critRate -= modifiers.critRate;
            _activeStatModifiers.evadeFlat -= modifiers.evadeFlat;
            _activeStatModifiers.healPercent -= modifiers.healPercent;
        }
        
        /// <summary>
        /// 获取元素抗性
        /// </summary>
        public float GetElementalResistance(Element element)
        {
            float resist = 0f;
            
            // TODO: 从装备和状态获取
            
            return resist;
        }
        
        /// <summary>
        /// 设置元素吸收
        /// </summary>
        public void SetElementAbsorb(Element element, float percent)
        {
            _absorbElement = element;
            _absorbPercent = percent;
        }
        
        /// <summary>
        /// 获取吸收元素
        /// </summary>
        public Element? GetAbsorbElement()
        {
            return _absorbElement;
        }
        
        /// <summary>
        /// 获取吸收百分比
        /// </summary>
        public float GetAbsorbPercent()
        {
            return _absorbPercent;
        }
        
        /// <summary>
        /// 设置反击数据
        /// </summary>
        public void SetCounterData(float percent, int count)
        {
            _counterPercent = percent;
            _counterCount = count;
        }
        
        #endregion
        
        #region 冷却
        
        /// <summary>
        /// 设置技能冷却
        /// </summary>
        public void SetSkillCooldown(SkillData skill, int turns)
        {
            _skillCooldowns[skill.skillId] = turns;
        }
        
        /// <summary>
        /// 获取技能冷却
        /// </summary>
        public int GetSkillCooldown(SkillData skill)
        {
            return _skillCooldowns.TryGetValue(skill.skillId, out var cd) ? cd : 0;
        }
        
        /// <summary>
        /// 减少所有技能冷却
        /// </summary>
        public void ReduceAllCooldowns()
        {
            var keys = _skillCooldowns.Keys.ToList();
            foreach (var key in keys)
            {
                _skillCooldowns[key] = Mathf.Max(0, _skillCooldowns[key] - 1);
            }
        }
        
        #endregion
        
        #region 动画
        
        /// <summary>
        /// 播放动画
        /// </summary>
        public void PlayAnimation(BattleAnimation animation)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(animation.ToString());
            }
        }
        
        /// <summary>
        /// 播放指定动画（字符串）
        /// </summary>
        public void PlayAnimation(string trigger)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(trigger);
            }
        }
        
        #endregion
        
        #region 回合处理
        
        /// <summary>
        /// 回合开始
        /// </summary>
        public void OnTurnStart()
        {
            _hasActedThisTurn = false;
            OnTurnStart?.Invoke(this);
        }
        
        /// <summary>
        /// 回合结束
        /// </summary>
        public void OnTurnEnd()
        {
            // 减少状态持续时间
            foreach (var status in _statusEffects)
            {
                status.RemainingTime--;
            }
            
            // 移除过期状态
            _statusEffects.RemoveAll(s => s.RemainingTime <= 0);
            
            // 减少冷却
            ReduceAllCooldowns();
            
            // 恢复少量HP/MP
            if (HasStatus(StatusEffectType.Regen))
            {
                int heal = (int)(Stats.MaxHp * 0.05f);
                Heal(heal);
            }
            
            // 再生效果结束检查
            if (HasStatus(StatusEffectType.Regen))
            {
                RemoveStatus(GetStatus(StatusEffectType.Regen));
            }
            
            OnTurnEnd?.Invoke(this);
        }
        
        /// <summary>
        /// 标记为已行动
        /// </summary>
        public void MarkAsActed()
        {
            _hasActedThisTurn = true;
        }
        
        #endregion
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 计算最终属性
        /// </summary>
        private CharacterStats CalculateFinalStats()
        {
            var baseStats = _characterData.GetStatsAtLevel(_currentLevel);
            var finalStats = new CharacterStats
            {
                MaxHp = baseStats.MaxHp,
                MaxMp = baseStats.MaxMp,
                Atk = baseStats.Atk,
                Def = baseStats.Def,
                Mag = baseStats.Mag,
                Res = baseStats.Res,
                Spd = baseStats.Spd,
                CritRate = baseStats.CritRate,
                CritDamage = baseStats.CritDamage,
                EvadeRate = baseStats.EvadeRate,
                Accuracy = baseStats.Accuracy,
                BlockRate = baseStats.BlockRate
            };
            
            // 应用属性修正
            finalStats.Atk += _activeStatModifiers.atkFlat;
            finalStats.Def += _activeStatModifiers.defFlat;
            finalStats.Mag += _activeStatModifiers.magFlat;
            finalStats.Spd += _activeStatModifiers.spdFlat;
            
            finalStats.Atk = (int)(finalStats.Atk * (1 + _activeStatModifiers.atkPercent));
            finalStats.Def = (int)(finalStats.Def * (1 + _activeStatModifiers.defPercent));
            finalStats.Mag = (int)(finalStats.Mag * (1 + _activeStatModifiers.magPercent));
            finalStats.Spd = (int)(finalStats.Spd * (1 + _activeStatModifiers.spdPercent));
            
            finalStats.CritRate += _activeStatModifiers.critRate;
            finalStats.EvadeRate += _activeStatModifiers.evadeFlat;
            
            // 应用状态效果
            foreach (var status in _statusEffects)
            {
                ApplyStatusEffectToStats(status, ref finalStats);
            }
            
            // 确保值不为负
            finalStats.Atk = Mathf.Max(1, finalStats.Atk);
            finalStats.Def = Mathf.Max(0, finalStats.Def);
            finalStats.Mag = Mathf.Max(1, finalStats.Mag);
            finalStats.Res = Mathf.Max(0, finalStats.Res);
            finalStats.Spd = Mathf.Max(1, finalStats.Spd);
            finalStats.CritRate = Mathf.Clamp01(finalStats.CritRate);
            finalStats.EvadeRate = Mathf.Clamp01(finalStats.EvadeRate);
            
            return finalStats;
        }
        
        /// <summary>
        /// 应用状态效果到属性
        /// </summary>
        private void ApplyStatusEffectToStats(StatusEffectInstance status, ref CharacterStats stats)
        {
            switch (status.StatusType)
            {
                case StatusEffectType.PowerUp:
                    stats.Atk = (int)(stats.Atk * 1.3f);
                    break;
                case StatusEffectType.PowerDown:
                    stats.Atk = (int)(stats.Atk * 0.7f);
                    break;
                    
                case StatusEffectType.DefenseUp:
                    stats.Def = (int)(stats.Def * 1.4f);
                    break;
                case StatusEffectType.DefenseDown:
                    stats.Def = (int)(stats.Def * 0.6f);
                    break;
                    
                case StatusEffectType.SpeedUp:
                    stats.Spd = (int)(stats.Spd * 1.5f);
                    break;
                case StatusEffectType.SpeedDown:
                    stats.Spd = (int)(stats.Spd * 0.6f);
                    break;
                    
                case StatusEffectType.MagicUp:
                    stats.Mag = (int)(stats.Mag * 1.35f);
                    break;
                case StatusEffectType.MagicDown:
                    stats.Mag = (int)(stats.Mag * 0.65f);
                    break;
                    
                case StatusEffectType.EvadeUp:
                    stats.EvadeRate += 0.25f;
                    break;
            }
        }
        
        /// <summary>
        /// 计算防御减伤
        /// </summary>
        private int CalculateDamageAfterDefense(int damage)
        {
            var stats = Stats;
            
            if (HasStatus(StatusEffectType.DefenseUp))
                damage = (int)(damage * 0.6f);
            if (HasStatus(StatusEffectType.DefenseDown))
                damage = (int)(damage * 1.4f);
            
            return damage;
        }
        
        /// <summary>
        /// 计算元素克制
        /// </summary>
        private int CalculateElementalModifier(int damage, Element element)
        {
            float modifier = ElementChart.GetElementModifier(element, _characterData.element);
            return (int)(damage * modifier);
        }
        
        #endregion
    }
}
