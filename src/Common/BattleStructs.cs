// BattleStructs.cs - 战斗系统数据结构
// 定义战斗系统使用的主要数据结构

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JRPG.Battle
{
    /// <summary>
    /// 战斗行动 - 表示一个完整的战斗行动
    /// </summary>
    [Serializable]
    public struct BattleAction
    {
        /// <summary>
        /// 行动来源单位
        /// </summary>
        public BattleUnit Source;
        
        /// <summary>
        /// 行动类型
        /// </summary>
        public ActionType ActionType;
        
        /// <summary>
        /// 使用的技能（如果没有则为null）
        /// </summary>
        public SkillData Skill;
        
        /// <summary>
        /// 目标单位列表
        /// </summary>
        public List<BattleUnit> Targets;
        
        /// <summary>
        /// 行动执行时间戳
        /// </summary>
        public float ExecuteTime;
        
        /// <summary>
        /// 行动优先级（影响ATB排序）
        /// </summary>
        public int Priority;
        
        /// <summary>
        /// 是否为自动战斗
        /// </summary>
        public bool IsAutoAction;
        
        /// <summary>
        /// 创建普通攻击行动
        /// </summary>
        public static BattleAction CreateNormalAttack(BattleUnit source, BattleUnit target)
        {
            return new BattleAction
            {
                Source = source,
                ActionType = ActionType.Attack,
                Skill = source.CharacterData.normalAttack,
                Targets = new List<BattleUnit> { target },
                ExecuteTime = Time.time,
                Priority = 100,
                IsAutoAction = false
            };
        }
        
        /// <summary>
        /// 创建技能行动
        /// </summary>
        public static BattleAction CreateSkillAction(BattleUnit source, SkillData skill, List<BattleUnit> targets)
        {
            return new BattleAction
            {
                Source = source,
                ActionType = ActionType.Skill,
                Skill = skill,
                Targets = targets,
                ExecuteTime = Time.time,
                Priority = skill.skillType == SkillType.UltimateSkill ? 200 : 150,
                IsAutoAction = false
            };
        }
        
        /// <summary>
        /// 创建防御行动
        /// </summary>
        public static BattleAction CreateDefendAction(BattleUnit source)
        {
            return new BattleAction
            {
                Source = source,
                ActionType = ActionType.Defend,
                Skill = null,
                Targets = new List<BattleUnit> { source },
                ExecuteTime = Time.time,
                Priority = 50,
                IsAutoAction = false
            };
        }
    }
    
    /// <summary>
    /// 伤害结算结果
    /// </summary>
    [Serializable]
    public struct DamageResult
    {
        /// <summary>
        /// 目标单位
        /// </summary>
        public BattleUnit Target;
        
        /// <summary>
        /// 原始伤害值
        /// </summary>
        public int RawDamage;
        
        /// <summary>
        /// 最终伤害值
        /// </summary>
        public int FinalDamage;
        
        /// <summary>
        /// 伤害类型
        /// </summary>
        public DamageType DamageType;
        
        /// <summary>
        /// 元素类型
        /// </summary>
        public Element Element;
        
        /// <summary>
        /// 结算结果类型
        /// </summary>
        public DamageResultType ResultType;
        
        /// <summary>
        /// 是否为暴击
        /// </summary>
        public bool IsCritical => ResultType == DamageResultType.Critical;
        
        /// <summary>
        /// 是否闪避
        /// </summary>
        public bool IsMiss => ResultType == DamageResultType.Miss;
        
        /// <summary>
        /// 治疗量（如果是治疗技能）
        /// </summary>
        public int HealAmount;
        
        /// <summary>
        /// 吸收护盾量
        /// </summary>
        public int AbsorbedShield;
        
        /// <summary>
        /// 附加的状态效果
        /// </summary>
        public List<StatusEffectType> AppliedStatuses;
        
        /// <summary>
        /// 创建伤害结果
        /// </summary>
        public static DamageResult CreateDamage(BattleUnit target, int rawDamage, int finalDamage, 
            DamageType type, Element element, DamageResultType resultType)
        {
            return new DamageResult
            {
                Target = target,
                RawDamage = rawDamage,
                FinalDamage = finalDamage,
                DamageType = type,
                Element = element,
                ResultType = resultType,
                HealAmount = 0,
                AbsorbedShield = 0,
                AppliedStatuses = new List<StatusEffectType>()
            };
        }
        
        /// <summary>
        /// 创建治疗结果
        /// </summary>
        public static DamageResult CreateHeal(BattleUnit target, int healAmount)
        {
            return new DamageResult
            {
                Target = target,
                RawDamage = 0,
                FinalDamage = 0,
                DamageType = DamageType.Heal,
                Element = Element.None,
                ResultType = DamageResultType.Normal,
                HealAmount = healAmount,
                AbsorbedShield = 0,
                AppliedStatuses = new List<StatusEffectType>()
            };
        }
    }
    
    /// <summary>
    /// 战斗事件数据
    /// </summary>
    [Serializable]
    public class BattleEventData
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        public string EventType;
        
        /// <summary>
        /// 事件时间戳
        /// </summary>
        public float Timestamp;
        
        /// <summary>
        /// 关联的行动
        /// </summary>
        public BattleAction Action;
        
        /// <summary>
        /// 伤害结果列表
        /// </summary>
        public List<DamageResult> DamageResults;
        
        /// <summary>
        /// 额外数据
        /// </summary>
        public Dictionary<string, object> ExtraData;
        
        /// <summary>
        /// 创建战斗事件
        /// </summary>
        public static BattleEventData Create(string eventType, BattleAction action = default)
        {
            return new BattleEventData
            {
                EventType = eventType,
                Timestamp = Time.time,
                Action = action,
                DamageResults = new List<DamageResult>(),
                ExtraData = new Dictionary<string, object>()
            };
        }
    }
    
    /// <summary>
    /// 角色属性结构
    /// </summary>
    [Serializable]
    public struct CharacterStats
    {
        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHp;
        
        /// <summary>
        /// 最大魔法值
        /// </summary>
        public int MaxMp;
        
        /// <summary>
        /// 攻击力
        /// </summary>
        public int Atk;
        
        /// <summary>
        /// 防御力
        /// </summary>
        public int Def;
        
        /// <summary>
        /// 魔力
        /// </summary>
        public int Mag;
        
        /// <summary>
        /// 抗性
        /// </summary>
        public int Res;
        
        /// <summary>
        /// 速度
        /// </summary>
        public int Spd;
        
        /// <summary>
        /// 暴击率 (0-1)
        /// </summary>
        public float CritRate;
        
        /// <summary>
        /// 暴击伤害倍率
        /// </summary>
        public float CritDamage;
        
        /// <summary>
        /// 闪避率 (0-1)
        /// </summary>
        public float EvadeRate;
        
        /// <summary>
        /// 命中率 (0-1)
        /// </summary>
        public float Accuracy;
        
        /// <summary>
        /// 格挡率 (0-1)
        /// </summary>
        public float BlockRate;
        
        /// <summary>
        /// 创建默认属性
        /// </summary>
        public static CharacterStats Default => new CharacterStats
        {
            MaxHp = 1000,
            MaxMp = 100,
            Atk = 100,
            Def = 50,
            Mag = 100,
            Res = 50,
            Spd = 100,
            CritRate = 0.05f,
            CritDamage = 1.5f,
            EvadeRate = 0.05f,
            Accuracy = 0.95f,
            BlockRate = 0.1f
        };
    }
    
    /// <summary>
    /// 元素克制关系表
    /// </summary>
    public static class ElementChart
    {
        private static readonly Dictionary<Element, Dictionary<Element, float>> _chart = new Dictionary<Element, Dictionary<Element, float>>
        {
            { Element.Fire, new Dictionary<Element, float> { { Element.Ice, 2f }, { Element.Water, 0.5f }, { Element.Fire, 0.5f } } },
            { Element.Ice, new Dictionary<Element, float> { { Element.Fire, 0.5f }, { Element.Thunder, 0.5f }, { Element.Ice, 0.5f } } },
            { Element.Thunder, new Dictionary<Element, float> { { Element.Water, 2f }, { Element.Earth, 0.5f }, { Element.Thunder, 0.5f } } },
            { Element.Light, new Dictionary<Element, float> { { Element.Dark, 2f }, { Element.Light, 0.5f } } },
            { Element.Dark, new Dictionary<Element, float> { { Element.Light, 0.5f }, { Element.Dark, 0.5f } } },
            { Element.Water, new Dictionary<Element, float> { { Element.Fire, 2f }, { Element.Earth, 0.5f }, { Element.Water, 0.5f } } },
            { Element.Earth, new Dictionary<Element, float> { { Element.Thunder, 2f }, { Element.Wind, 0.5f }, { Element.Earth, 0.5f } } },
            { Element.Wind, new Dictionary<Element, float> { { Element.Earth, 2f }, { Element.Fire, 0.5f }, { Element.Wind, 0.5f } } }
        };
        
        /// <summary>
        /// 获取元素克制倍率
        /// </summary>
        public static float GetElementModifier(Element attackElement, Element defenseElement)
        {
            if (attackElement == Element.None || defenseElement == Element.None)
                return 1f;
                
            if (_chart.TryGetValue(attackElement, out var modifiers))
            {
                if (modifiers.TryGetValue(defenseElement, out var modifier))
                    return modifier;
            }
            
            return 1f;
        }
    }
    
    /// <summary>
    /// 种族克制关系表
    /// </summary>
    public static class RaceChart
    {
        private static readonly Dictionary<Race, Dictionary<Race, float>> _chart = new Dictionary<Race, Dictionary<Race, float>>
        {
            { Race.Angel, new Dictionary<Race, float> { { Race.Demon, 1.5f }, { Race.Vampire, 1.3f }, { Race.Beast, 1.2f } } },
            { Race.Demon, new Dictionary<Race, float> { { Race.Angel, 1.5f }, { Race.Human, 1.2f }, { Race.Beast, 1.3f } } },
            { Race.Vampire, new Dictionary<Race, float> { { Race.Angel, 1.2f }, { Race.Human, 1.1f } } },
            { Race.Beast, new Dictionary<Race, float> { { Race.Demon, 1.3f } } },
            { Race.Human, new Dictionary<Race, float>() }
        };
        
        /// <summary>
        /// 获取种族克制倍率
        /// </summary>
        public static float GetRaceModifier(Race attackerRace, Race defenderRace)
        {
            if (_chart.TryGetValue(attackerRace, out var modifiers))
            {
                if (modifiers.TryGetValue(defenderRace, out var modifier))
                    return modifier;
            }
            return 1f;
        }
    }
}
