// SkillExecutor.cs - 技能执行器
// 处理技能的完整执行流程

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.Battle.Skills
{
    /// <summary>
    /// 技能执行器 - 单例模式
    /// 负责技能的完整执行流程：验证、伤害计算、效果应用
    /// </summary>
    public class SkillExecutor : MonoBehaviour
    {
        #region Singleton
        
        private static SkillExecutor _instance;
        public static SkillExecutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SkillExecutor>();
                    if (_instance == null)
                    {
                        var go = new GameObject("SkillExecutor");
                        _instance = go.AddComponent<SkillExecutor>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 技能执行开始事件
        /// </summary>
        public event Action<BattleAction> OnSkillExecutionStart;
        
        /// <summary>
        /// 技能执行完成事件
        /// </summary>
        public event Action<BattleAction, List<DamageResult>> OnSkillExecutionComplete;
        
        /// <summary>
        /// 单个目标效果应用事件
        /// </summary>
        public event Action<BattleUnit, DamageResult> OnTargetEffectApplied;
        
        /// <summary>
        /// 伤害数字显示请求
        /// </summary>
        public event Action<BattleUnit, DamageResult> OnDamageNumberRequest;
        
        #endregion
        
        #region Settings
        
        [Header("执行设置")]
        [Tooltip("伤害数字显示延迟")]
        [SerializeField] private float _damageNumberDelay = 0.3f;
        
        [Tooltip("效果执行间隔（群体技能时）")]
        [SerializeField] private float _effectInterval = 0.2f;
        
        [Tooltip("技能执行动画时长")]
        [SerializeField] private float _skillAnimationDuration = 1.0f;
        
        [Tooltip("是否启用暴击慢动作")]
        [SerializeField] private bool _enableCritSlowMotion = true;
        
        [Tooltip("暴击慢动作持续时间")]
        [SerializeField] private float _critSlowMotionDuration = 0.15f;
        
        [Tooltip("暴击慢动作倍率")]
        [SerializeField] private float _critSlowMotionScale = 0.3f;
        
        #endregion
        
        #region Private Fields
        
        private bool _isExecuting = false;
        private BattleAction _currentAction;
        private List<DamageResult> _results = new List<DamageResult>();
        private Coroutine _executionCoroutine;
        
        // 伤害公式设置
        private float _baseVariance = 0.1f;  // 基础伤害浮动 ±10%
        private float _critMultiplier = 1.5f; // 暴击倍率
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 是否正在执行技能
        /// </summary>
        public bool IsExecuting => _isExecuting;
        
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
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 执行战斗行动
        /// </summary>
        public Coroutine ExecuteAction(BattleAction action)
        {
            if (_isExecuting)
            {
                Debug.LogWarning("SkillExecutor: Already executing an action!");
                return null;
            }
            
            _currentAction = action;
            _results.Clear();
            
            return _executionCoroutine = StartCoroutine(ExecuteActionCoroutine(action));
        }
        
        /// <summary>
        /// 取消当前执行
        /// </summary>
        public void CancelExecution()
        {
            if (_executionCoroutine != null)
            {
                StopCoroutine(_executionCoroutine);
                _executionCoroutine = null;
            }
            _isExecuting = false;
        }
        
        /// <summary>
        /// 验证技能是否可用
        /// </summary>
        public bool ValidateSkill(BattleUnit user, SkillData skill)
        {
            // 检查MP
            if (user.CurrentMp < skill.mpCost)
            {
                Debug.Log($"SkillExecutor: Not enough MP. Need {skill.mpCost}, have {user.CurrentMp}");
                return false;
            }
            
            // 检查冷却
            if (skill.cooldown > 0 && user.GetSkillCooldown(skill) > 0)
            {
                Debug.Log($"SkillExecutor: Skill is on cooldown. {user.GetSkillCooldown(skill)} turns remaining.");
                return false;
            }
            
            // 检查沉默
            if (user.HasStatus(StatusEffectType.Silence))
            {
                Debug.Log("SkillExecutor: User is silenced.");
                return false;
            }
            
            // 检查技能类型
            if (skill.skillType == SkillType.UltimateSkill)
            {
                // 检查终极能量
                if (user.UltimateEnergy < 100)
                {
                    Debug.Log("SkillExecutor: Not enough ultimate energy.");
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 计算伤害
        /// </summary>
        public DamageResult CalculateDamage(BattleUnit attacker, BattleUnit defender, SkillData skill)
        {
            // 获取基础属性
            var attackerStats = attacker.Stats;
            var defenderStats = defender.Stats;
            
            // 计算基础伤害
            int baseDamage = skill.baseDamage;
            
            // 根据伤害类型计算
            float damage;
            if (skill.damageType == DamageType.Physical)
            {
                damage = (attackerStats.Atk - defenderStats.Def / 2f) * skill.damageMultiplier;
            }
            else if (skill.damageType == DamageType.Magic)
            {
                damage = (attackerStats.Mag - defenderStats.Res / 2f) * skill.damageMultiplier;
            }
            else // True damage
            {
                damage = baseDamage * skill.damageMultiplier;
            }
            
            // 确保持续伤害为正
            damage = Mathf.Max(1f, damage);
            
            // 元素克制
            float elementMod = ElementChart.GetElementModifier(skill.element, defender.CharacterData.element);
            damage *= elementMod;
            
            // 种族克制
            float raceMod = RaceChart.GetRaceModifier(
                attacker.CharacterData.race, 
                defender.CharacterData.race
            );
            damage *= raceMod;
            
            // 暴击判定
            DamageResultType resultType = DamageResultType.Normal;
            float critChance = attackerStats.CritRate;
            
            // 检查暴击加成
            if (skill.critBonus > 0)
                critChance += skill.critBonus;
            
            bool isCrit = UnityEngine.Random.value < critChance;
            if (isCrit)
            {
                damage *= _critMultiplier;
                resultType = DamageResultType.Critical;
            }
            
            // 状态效果加成
            if (attacker.HasStatus(StatusEffectType.PowerUp))
                damage *= 1.3f;
            if (attacker.HasStatus(StatusEffectType.AttackUp))
                damage *= 1.2f;
            if (attacker.HasStatus(StatusEffectType.PowerDown))
                damage *= 0.7f;
            
            // 防御方状态效果减免
            if (defender.HasStatus(StatusEffectType.DefenseUp))
                damage *= 0.6f;
            if (defender.HasStatus(StatusEffectType.DefenseDown))
                damage *= 1.4f;
            
            // 元素抗性
            float elementalResist = defender.GetElementalResistance(skill.element);
            damage *= (1f - elementalResist);
            
            // 伤害浮动
            float variance = UnityEngine.Random.Range(1f - _baseVariance, 1f + _baseVariance);
            damage *= variance;
            
            // 护盾吸收
            int shieldAbsorb = defender.GetShieldValue();
            int actualDamage = Mathf.Max(0, (int)damage - shieldAbsorb);
            int absorbedShield = Mathf.Min(shieldAbsorb, (int)damage);
            
            // 无敌检查
            if (defender.HasStatus(StatusEffectType.Invincible))
            {
                resultType = DamageResultType.Immune;
                actualDamage = 0;
            }
            
            // 闪避检查
            float evadeChance = defenderStats.EvadeRate;
            if (defender.HasStatus(StatusEffectType.EvadeUp))
                evadeChance += 0.25f;
            if (UnityEngine.Random.value < evadeChance)
            {
                resultType = DamageResultType.Miss;
                actualDamage = 0;
            }
            
            // 吸收
            if (defender.HasStatus(StatusEffectType.AbsorbElement) && 
                defender.GetAbsorbElement() == skill.element)
            {
                resultType = DamageResultType.Absorb;
                actualDamage = -(int)damage; // 转为治疗
            }
            
            return DamageResult.CreateDamage(
                defender,
                (int)damage,
                actualDamage,
                skill.damageType,
                skill.element,
                resultType
            );
        }
        
        /// <summary>
        /// 计算治疗量
        /// </summary>
        public int CalculateHeal(BattleUnit healer, BattleUnit target, SkillData skill)
        {
            var stats = healer.Stats;
            
            // 基础治疗量
            float healAmount = skill.baseDamage;
            
            // 魔力加成
            healAmount += stats.Mag * 0.5f;
            
            // 状态加成
            if (healer.HasStatus(StatusEffectType.MagicUp))
                healAmount *= 1.35f;
            
            // 目标Debuff减免
            if (target.HasStatus(StatusEffectType.Curse))
                healAmount *= 0.5f;
            
            // 浮动
            float variance = UnityEngine.Random.Range(0.95f, 1.05f);
            healAmount *= variance;
            
            return Mathf.Max(1, (int)healAmount);
        }
        
        /// <summary>
        /// 选择技能目标
        /// </summary>
        public List<BattleUnit> SelectTargets(BattleUnit user, SkillData skill, List<BattleUnit> allUnits)
        {
            var targets = new List<BattleUnit>();
            
            switch (skill.targetType)
            {
                case TargetType.Self:
                    targets.Add(user);
                    break;
                    
                case TargetType.SingleEnemy:
                    // 手动选择目标时返回空，由UI处理
                    break;
                    
                case TargetType.SingleAlly:
                    // 手动选择目标时返回空
                    break;
                    
                case TargetType.SingleAny:
                    // 手动选择目标时返回空
                    break;
                    
                case TargetType.AllEnemies:
                    targets = allUnits.Where(u => u.IsAlive && u.IsPlayerControlled != user.IsPlayerControlled).ToList();
                    break;
                    
                case TargetType.AllAllies:
                    targets = allUnits.Where(u => u.IsAlive && u.IsPlayerControlled == user.IsPlayerControlled).ToList();
                    break;
                    
                case TargetType.AllUnits:
                    targets = allUnits.Where(u => u.IsAlive).ToList();
                    break;
                    
                case TargetType.RandomEnemy:
                    var enemies = allUnits.Where(u => u.IsAlive && u.IsPlayerControlled != user.IsPlayerControlled).ToList();
                    if (enemies.Count > 0)
                    {
                        int count = Mathf.Min(skill.targetCount, enemies.Count);
                        var randomTargets = enemies.OrderBy(x => UnityEngine.Random.value).Take(count);
                        targets.AddRange(randomTargets);
                    }
                    break;
                    
                case TargetType.RandomAlly:
                    var allies = allUnits.Where(u => u.IsAlive && u.IsPlayerControlled == user.IsPlayerControlled && u != user).ToList();
                    if (allies.Count > 0)
                    {
                        int count = Mathf.Min(skill.targetCount, allies.Count);
                        var randomTargets = allies.OrderBy(x => UnityEngine.Random.value).Take(count);
                        targets.AddRange(randomTargets);
                    }
                    break;
            }
            
            return targets;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 执行行动协程
        /// </summary>
        private IEnumerator ExecuteActionCoroutine(BattleAction action)
        {
            _isExecuting = true;
            
            // 触发开始事件
            OnSkillExecutionStart?.Invoke(action);
            
            // 消耗资源
            ConsumeResources(action);
            
            // 获取所有单位
            var allUnits = GetAllBattleUnits();
            
            // 如果没有指定目标，需要确定目标
            if (action.Targets.Count == 0)
            {
                action.Targets = SelectTargets(action.Source, action.Skill, allUnits);
            }
            
            // 执行动画
            if (action.Skill != null)
            {
                yield return StartCoroutine(PlaySkillAnimation(action));
            }
            
            // 应用效果到每个目标
            float delay = 0f;
            foreach (var target in action.Targets)
            {
                if (target == null || !target.IsAlive) continue;
                
                var result = ApplyEffect(action.Source, target, action.Skill);
                _results.Add(result);
                
                // 显示伤害数字
                if (result.FinalDamage != 0 || result.HealAmount > 0 || result.ResultType != DamageResultType.Normal)
                {
                    OnDamageNumberRequest?.Invoke(target, result);
                }
                
                // 触发单个效果事件
                OnTargetEffect?.Invoke(target, result);
                
                // 群体技能间隔
                if (action.Targets.Count > 1)
                {
                    yield return new WaitForSeconds(_effectInterval);
                }
            }
            
            // 应用技能附加效果
            ApplySkillEffects(action);
            
            // 等待动画结束
            yield return new WaitForSeconds(_skillAnimationDuration * 0.5f);
            
            // 触发完成事件
            OnSkillExecutionComplete?.Invoke(action, _results);
            
            _isExecuting = false;
            _currentAction = default;
        }
        
        /// <summary>
        /// 消耗行动资源
        /// </summary>
        private void ConsumeResources(BattleAction action)
        {
            if (action.Skill == null) return;
            
            // 消耗MP
            action.Source.ConsumeMP(action.Skill.mpCost);
            
            // 设置冷却
            if (action.Skill.cooldown > 0)
            {
                action.Source.SetSkillCooldown(action.Skill, action.Skill.cooldown);
            }
            
            // 消耗终极能量
            if (action.Skill.skillType == SkillType.UltimateSkill)
            {
                action.Source.ConsumeUltimateEnergy(100);
            }
        }
        
        /// <summary>
        /// 应用效果到单个目标
        /// </summary>
        private DamageResult ApplyEffect(BattleUnit attacker, BattleUnit defender, SkillData skill)
        {
            DamageResult result;
            
            if (skill.damageType == DamageType.Heal)
            {
                // 治疗
                int healAmount = CalculateHeal(attacker, defender, skill);
                defender.Heal(healAmount);
                result = DamageResult.CreateHeal(defender, healAmount);
            }
            else
            {
                // 伤害
                result = CalculateDamage(attacker, defender, skill);
                
                // 应用伤害
                if (!result.IsMiss && result.ResultType != DamageResultType.Immune)
                {
                    defender.TakeDamage(result.FinalDamage, result.Element);
                    
                    // 吸收护盾
                    if (result.AbsorbedShield > 0)
                    {
                        defender.ReduceShield(result.AbsorbedShield);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 应用技能附加效果（状态效果等）
        /// </summary>
        private void ApplySkillEffects(BattleAction action)
        {
            if (action.Skill == null) return;
            
            // 应用预设效果
            foreach (var effect in action.Skill.effects)
            {
                if (effect == null) continue;
                
                foreach (var target in action.Targets)
                {
                    if (target == null || !target.IsAlive) continue;
                    
                    effect.Apply(target, action.Source);
                }
            }
        }
        
        /// <summary>
        /// 播放技能动画
        /// </summary>
        private IEnumerator PlaySkillAnimation(BattleAction action)
        {
            // 播放施法者动画
            action.Source.PlayAnimation(BattleAnimation.Skill);
            
            // 播放特效
            if (action.Skill.vfxPrefab != null)
            {
                foreach (var target in action.Targets)
                {
                    if (target == null) continue;
                    
                    var vfx = Instantiate(action.Skill.vfxPrefab, target.transform.position, Quaternion.identity);
                    Destroy(vfx, 3f);
                }
            }
            
            // 播放音效
            if (action.Skill.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(action.Skill.soundEffect, Camera.main.transform.position);
            }
            
            // 等待动画时长
            yield return new WaitForSeconds(action.Skill.animationDuration);
        }
        
        /// <summary>
        /// 获取所有战斗单位
        /// </summary>
        private List<BattleUnit> GetAllBattleUnits()
        {
            var units = new List<BattleUnit>();
            units.AddRange(BattleManager.Instance.PlayerUnits);
            units.AddRange(BattleManager.Instance.EnemyUnits);
            return units;
        }
        
        #endregion
        
        #region Events (Internal)
        
        // 内部事件引用，用于替代Action避免冲突
        private event Action<BattleUnit, DamageResult> OnTargetEffect;
        
        #endregion
    }
}
