// SkillEffect.cs - 技能效果基类
// 定义所有技能效果的基类和通用接口

using UnityEngine;
using System;

namespace JRPG.Battle.Skills
{
    /// <summary>
    /// 技能效果基类
    /// 所有技能效果都继承自此类
    /// </summary>
    [Serializable]
    public abstract class SkillEffect
    {
        #region Fields
        
        [Header("基础设置")]
        [Tooltip("效果ID")]
        public int effectId;
        
        [Tooltip("效果名称")]
        public string effectName;
        
        [Tooltip("效果描述")]
        [TextArea(2, 4)]
        public string description;
        
        [Header("触发设置")]
        [Tooltip("触发时机")]
        public EffectTiming timing = EffectTiming.OnHit;
        
        [Tooltip("触发概率 (0-1)")]
        [Range(0f, 1f)]
        public float triggerChance = 1f;
        
        [Tooltip("效果持续回合数")]
        public int duration = 1;
        
        [Header("目标设置")]
        [Tooltip("效果应用目标")]
        public EffectTarget target = EffectTarget.Target;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 应用效果
        /// </summary>
        public virtual void Apply(BattleUnit target, BattleUnit source)
        {
            // 触发概率检查
            if (UnityEngine.Random.value > triggerChance)
                return;
            
            // 执行具体效果
            Execute(target, source);
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        public virtual void Remove(BattleUnit target, BattleUnit source)
        {
            // 虚方法，子类可重写
        }
        
        /// <summary>
        /// 每回合开始时更新
        /// </summary>
        public virtual void OnTurnStart(BattleUnit target, BattleUnit source)
        {
            // 虚方法，子类可重写
        }
        
        /// <summary>
        /// 每回合结束时更新
        /// </summary>
        public virtual void OnTurnEnd(BattleUnit target, BattleUnit source)
        {
            // 虚方法，子类可重写
        }
        
        /// <summary>
        /// 获取效果显示信息
        /// </summary>
        public virtual string GetDisplayText()
        {
            return effectName;
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// 执行具体效果（子类实现）
        /// </summary>
        protected abstract void Execute(BattleUnit target, BattleUnit source);
        
        /// <summary>
        /// 创建状态效果实例
        /// </summary>
        protected StatusEffectInstance CreateStatusInstance(StatusEffectType type, int duration)
        {
            return new StatusEffectInstance
            {
                StatusType = type,
                Duration = duration,
                Source = StatusSource.Skill,
                Intensity = 1f
            };
        }
        
        #endregion
        
        #region Enums
        
        /// <summary>
        /// 效果触发时机
        /// </summary>
        public enum EffectTiming
        {
            OnHit,          // 命中时
            OnCrit,         // 暴击时
            OnKill,         // 击杀时
            OnDamageTaken,  // 受到伤害时
            OnTurnStart,    // 回合开始时
            OnTurnEnd,      // 回合结束时
            OnHeal,         // 治疗时
            Passive         // 被动（立即应用）
        }
        
        /// <summary>
        /// 效果目标
        /// </summary>
        public enum EffectTarget
        {
            Target,         // 技能目标
            Self,           // 自身
            AllEnemies,     // 所有敌人
            AllAllies,      // 所有队友
            RandomEnemy,    // 随机敌人
            Attacker        // 攻击者（用于反击等）
        }
        
        #endregion
    }
    
    /// <summary>
    /// 伤害效果
    /// </summary>
    [Serializable]
    public class DamageEffect : SkillEffect
    {
        [Header("伤害设置")]
        public DamageType damageType = DamageType.Physical;
        public Element element = Element.None;
        public int baseDamage;
        public float damageMultiplier = 1f;
        
        [Header("暴击设置")]
        public float critBonus = 0f;
        
        [Header("无视防御")]
        public bool ignoreDefense = false;
        public bool ignoreResistance = false;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            int damage = CalculateDamage(source, target);
            target.TakeDamage(damage, element);
        }
        
        private int CalculateDamage(BattleUnit attacker, BattleUnit defender)
        {
            var stats = attacker.Stats;
            var defStats = defender.Stats;
            
            float damage;
            
            if (damageType == DamageType.Physical)
            {
                float atk = stats.Atk;
                float def = ignoreDefense ? 0 : defStats.Def / 2f;
                damage = (atk - def) * damageMultiplier;
            }
            else if (damageType == DamageType.Magic)
            {
                float mag = stats.Mag;
                float res = ignoreResistance ? 0 : defStats.Res / 2f;
                damage = (mag - res) * damageMultiplier;
            }
            else
            {
                damage = baseDamage * damageMultiplier;
            }
            
            // 元素克制
            damage *= ElementChart.GetElementModifier(element, defender.CharacterData.element);
            
            // 暴击
            if (critBonus > 0 && UnityEngine.Random.value < stats.CritRate + critBonus)
            {
                damage *= stats.CritDamage;
            }
            
            // 伤害浮动
            damage *= UnityEngine.Random.Range(0.95f, 1.05f);
            
            return Mathf.Max(1, (int)damage);
        }
    }
    
    /// <summary>
    /// 治疗效果
    /// </summary>
    [Serializable]
    public class HealEffect : SkillEffect
    {
        [Header("治疗设置")]
        public int baseHeal;
        public float healMultiplier = 1f;
        
        [Header("治疗类型")]
        public bool healPercentOfMaxHP = false;
        public float healPercent = 0f;
        
        [Header("额外效果")]
        public bool removeDebuffs = false;
        public int removeDebuffCount = 0;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            int healAmount = CalculateHeal(source, target);
            target.Heal(healAmount);
            
            // 驱散debuff
            if (removeDebuffs)
            {
                target.RemoveDebuffs(removeDebuffCount);
            }
        }
        
        private int CalculateHeal(BattleUnit healer, BattleUnit target)
        {
            float heal;
            
            if (healPercentOfMaxHP)
            {
                heal = target.Stats.MaxHp * healPercent;
            }
            else
            {
                heal = baseHeal + healer.Stats.Mag * healMultiplier * 0.5f;
            }
            
            // 浮动
            heal *= UnityEngine.Random.Range(0.95f, 1.05f);
            
            return Mathf.Max(1, (int)heal);
        }
    }
    
    /// <summary>
    /// 增益效果（Buff）
    /// </summary>
    [Serializable]
    public class BuffEffect : SkillEffect
    {
        [Header("Buff类型")]
        public StatusEffectType buffType;
        
        [Header("属性加成")]
        public int atkBonus = 0;
        public int defBonus = 0;
        public int magBonus = 0;
        public int spdBonus = 0;
        public float atkPercentBonus = 0f;
        public float defPercentBonus = 0f;
        
        [Header("特殊加成")]
        public float critRateBonus = 0f;
        public float evadeBonus = 0f;
        public float damageBonus = 0f;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            var status = CreateStatusInstance(buffType, duration);
            target.ApplyStatus(status);
        }
    }
    
    /// <summary>
    /// 减益效果（Debuff）
    /// </summary>
    [Serializable]
    public class DebuffEffect : SkillEffect
    {
        [Header("Debuff类型")]
        public StatusEffectType debuffType;
        
        [Header("属性减少")]
        public int atkPenalty = 0;
        public int defPenalty = 0;
        public int magPenalty = 0;
        public int spdPenalty = 0;
        public float atkPercentPenalty = 0f;
        public float defPercentPenalty = 0f;
        
        [Header("特殊效果")]
        public float slowPercent = 0f;  // 减速百分比
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            var status = CreateStatusInstance(debuffType, duration);
            target.ApplyStatus(status);
        }
    }
    
    /// <summary>
    /// 护盾效果
    /// </summary>
    [Serializable]
    public class ShieldEffect : SkillEffect
    {
        [Header("护盾设置")]
        public int shieldAmount;
        public float shieldPercentOfMaxHP = 0f;
        
        [Header("护盾类型")]
        public bool isPhysicalShield = false;
        public bool isMagicShield = false;
        public Element shieldElement = Element.None;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            int shieldValue = shieldAmount;
            
            if (shieldPercentOfMaxHP > 0)
            {
                shieldValue = Mathf.Max(shieldValue, (int)(target.Stats.MaxHp * shieldPercentOfMaxHP));
            }
            
            target.AddShield(shieldValue, isPhysicalShield, isMagicShield, shieldElement);
        }
    }
    
    /// <summary>
    /// 复活效果
    /// </summary>
    [Serializable]
    public class ReviveEffect : SkillEffect
    {
        [Header("复活设置")]
        public float reviveHPPercent = 0.25f;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            if (!target.IsAlive)
            {
                target.Revive(target.Stats.MaxHp * reviveHPPercent);
            }
        }
    }
    
    /// <summary>
    /// 驱散效果
    /// </summary>
    [Serializable]
    public class DispelEffect : SkillEffect
    {
        [Header("驱散设置")]
        public bool dispelBuffs = true;
        public bool dispelDebuffs = true;
        public int dispelCount = 999;  // 999表示全部
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            if (dispelBuffs)
            {
                target.RemoveBuffs(dispelCount);
            }
            
            if (dispelDebuffs)
            {
                target.RemoveDebuffs(dispelCount);
            }
        }
    }
    
    /// <summary>
    /// 属性附加效果
    /// </summary>
    [Serializable]
    public class Status附加Effect : SkillEffect
    {
        [Header("附加状态")]
        public StatusEffectType statusToApply;
        
        [Header("附加概率")]
        [Range(0f, 1f)]
        public float applyChance = 0.5f;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            if (UnityEngine.Random.value < applyChance)
            {
                var status = CreateStatusInstance(statusToApply, duration);
                target.ApplyStatus(status);
            }
        }
    }
    
    /// <summary>
    /// 属性吸收效果
    /// </summary>
    [Serializable]
    public class AbsorbEffect : SkillEffect
    {
        [Header("吸收设置")]
        public Element absorbElement;
        public float absorbPercent = 0.5f;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            target.SetElementAbsorb(absorbElement, absorbPercent);
        }
    }
    
    /// <summary>
    /// 反击效果
    /// </summary>
    [Serializable]
    public class CounterEffect : SkillEffect
    {
        [Header("反击设置")]
        public float counterPercent = 1f;  // 反击伤害百分比
        public int counterCount = 1;
        
        protected override void Execute(BattleUnit target, BattleUnit source)
        {
            // 标记为目标正在反击
            target.SetCounterData(counterPercent, counterCount);
        }
    }
}
