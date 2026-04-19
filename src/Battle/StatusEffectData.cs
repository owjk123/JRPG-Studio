// StatusEffectData.cs - 状态效果数据定义
// 定义增益、减益、控制等状态效果

using UnityEngine;

namespace JRPG.Battle
{
    /// <summary>
    /// 状态效果数据
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "JRPG/Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("基础信息")]
        public int effectId;
        public string effectName;
        [TextArea] public string description;
        public StatusEffectType type;
        
        [Header("持续时间")]
        public int baseDuration = 3;
        public bool isPermanent = false;
        public bool dispellable = true;
        
        [Header("效果")]
        public float statModifier = 0f;     // 属性修正
        public bool isPercentage = true;    // 是否为百分比
        public AffectedStat affectedStat;   // 影响的属性
        
        [Header("特殊效果")]
        public SpecialEffect specialEffect;
        public int specialValue;
        
        [Header("视觉")]
        public GameObject vfxPrefab;
        public Color effectColor = Color.white;
    }
    
    /// <summary>
    /// 状态效果类型
    /// </summary>
    public enum StatusEffectType
    {
        // 增益
        AttackUp,       // 攻击提升
        DefenseUp,      // 防御提升
        SpeedUp,        // 速度提升
        CritUp,         // 暴击提升
        Regeneration,   // 再生
        Shield,         // 护盾
        Immunity,       // 免疫
        Reflect,        // 反射
        
        // 减益
        AttackDown,     // 攻击降低
        DefenseDown,    // 防御降低
        SpeedDown,      // 速度降低
        Poison,         // 毒
        Burn,           // 灼烧
        Bleed,          // 流血
        Curse,          // 诅咒
        Weakness,       // 虚弱
        
        // 控制
        Stun,           // 眩晕
        Sleep,          // 睡眠
        Charm,          // 魅惑
        Fear,           // 恐惧
        Freeze,         // 冻结
        Petrify,        // 石化
        
        // 种族特殊
        HolyLight,      // 圣光（天使族）
        BloodThirst,    // 血渴（吸血鬼）
        Berserk,        // 狂暴（兽人族）
        ShadowVeil,     // 暗影（魔人族）
        HumanSpirit     // 人类精神（人族）
    }
    
    public enum AffectedStat
    {
        None,
        MaxHp,
        CurrentHp,
        Atk,
        Def,
        Speed,
        CritRate,
        CritDamage,
        Mp
    }
    
    public enum SpecialEffect
    {
        None,
        DamageOverTime,
        HealOverTime,
        BlockAction,
        RedirectDamage,
        ExtraTurn,
        DamageReduction,
        DamageReflection
    }
    
    /// <summary>
    /// 运行时状态效果实例
    /// </summary>
    public class StatusEffectInstance
    {
        public StatusEffectData Data { get; private set; }
        public int RemainingDuration { get; private set; }
        public int Stacks { get; private set; }
        public BattleUnit Source { get; private set; }
        
        public StatusEffectInstance(StatusEffectData data, BattleUnit source, int duration = -1)
        {
            Data = data;
            Source = source;
            RemainingDuration = duration >= 0 ? duration : data.baseDuration;
            Stacks = 1;
        }
        
        public void OnTurnStart()
        {
            // 处理持续伤害/治疗
            if (Data.specialEffect == SpecialEffect.DamageOverTime)
            {
                // 计算持续伤害
            }
            else if (Data.specialEffect == SpecialEffect.HealOverTime)
            {
                // 计算持续治疗
            }
        }
        
        public void OnTurnEnd()
        {
            if (!Data.isPermanent)
            {
                RemainingDuration--;
            }
        }
        
        public bool IsExpired => !Data.isPermanent && RemainingDuration <= 0;
        
        public void AddStack(int amount = 1)
        {
            Stacks += amount;
        }
        
        public void RemoveStack(int amount = 1)
        {
            Stacks = Mathf.Max(0, Stacks - amount);
        }
    }
}
