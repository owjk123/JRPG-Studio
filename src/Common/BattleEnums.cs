// BattleEnums.cs - 战斗系统通用枚举定义
// 定义战斗系统使用的所有枚举类型

namespace JRPG.Battle
{
    #region 战斗流程状态
    
    /// <summary>
    /// 战斗流程状态
    /// </summary>
    public enum BattlePhase
    {
        None,               // 无
        BattleStart,         // 战斗开始
        PlayerTurn,          // 玩家回合
        EnemyTurn,           // 敌人回合
        ActionExecution,     // 行动执行
        DamageCalculation,   // 伤害计算
        StatusEffects,       // 状态效果处理
        TurnEnd,             // 回合结束
        BattleVictory,       // 战斗胜利
        BattleDefeat,        // 战斗失败
        BattleEnd            // 战斗结束
    }
    
    /// <summary>
    /// 行动选择状态
    /// </summary>
    public enum ActionSelectionPhase
    {
        None,
        SelectingAction,     // 选择行动类型
        SelectingSkill,      // 选择技能
        SelectingTarget,     // 选择目标
        ConfirmingAction,    // 确认行动
        Cancelled            // 取消选择
    }
    
    /// <summary>
    /// 战斗结果
    /// </summary>
    public enum BattleResult
    {
        None,
        Victory,
        Defeat,
        Flee,
        Draw
    }
    
    #endregion
    
    #region 属性与伤害
    
    /// <summary>
    /// 元素类型
    /// </summary>
    public enum Element
    {
        None = 0,       // 无属性
        Fire,           // 火焰
        Ice,            // 寒冰
        Thunder,        // 雷电
        Light,          // 圣光
        Dark,           // 暗影
        Wind,           // 疾风
        Earth,          // 大地
        Water           // 水
    }
    
    /// <summary>
    /// 伤害类型
    /// </summary>
    public enum DamageType
    {
        Physical,       // 物理伤害
        Magic,          // 魔法伤害
        True,           // 真实伤害
        Heal            // 治疗
    }
    
    /// <summary>
    /// 伤害结算类型
    /// </summary>
    public enum DamageResultType
    {
        Normal,         // 正常
        Critical,       // 暴击
        Miss,           // 闪避
        Immune,         // 免疫
        Resist,         // 抗性减半
        Weakness,       // 弱点加倍
        Absorb          // 吸收
    }
    
    #endregion
    
    #region 技能相关
    
    /// <summary>
    /// 技能类型
    /// </summary>
    public enum SkillType
    {
        NormalAttack,       // 普通攻击
        ActiveSkill,        // 主动技能
        UltimateSkill,      // 终极技能
        PassiveSkill,       // 被动技能
        Item                // 道具
    }
    
    /// <summary>
    /// 目标类型
    /// </summary>
    public enum TargetType
    {
        None,
        Self,               // 自身
        SingleEnemy,        // 单体敌人
        SingleAlly,         // 单体队友
        SingleAny,          // 单体任意
        AllEnemies,         // 全体敌人
        AllAllies,          // 全体队友
        AllUnits,           // 全体单位
        RandomEnemy,        // 随机敌人
        RandomAlly          // 随机队友
    }
    
    /// <summary>
    /// 目标选择方式
    /// </summary>
    public enum TargetSelectionType
    {
        Manual,             // 手动选择
        Auto,               // 自动选择（血量最低等）
        Random              // 随机选择
    }
    
    #endregion
    
    #region 状态效果
    
    /// <summary>
    /// 状态效果类型
    /// </summary>
    public enum StatusEffectType
    {
        // 控制类（无法行动）
        Stun,               // 眩晕
        Freeze,              // 冻结
        Sleep,               // 睡眠
        Petrify,             // 石化
        Fear,                // 恐惧
        Charm,               // 魅惑
        Confuse,             // 混乱
        
        // 持续伤害类
        Burn,                // 灼烧
        Poison,              // 中毒
        Bleed,               // 出血
        Curse,               // 诅咒
        
        // 持续效果类
        Silence,             // 沉默
        Slow,                // 减速
        Paralyze,            // 麻痹
        Bind,                // 束缚
        
        // 增益类 Buff
        PowerUp,             // 攻击力提升
        DefenseUp,           // 防御力提升
        SpeedUp,             // 速度提升
        MagicUp,             // 魔力提升
        EvadeUp,             // 闪避提升
        Regen,               // 再生
        Shield,              // 护盾
        Invincible,          // 无敌
        Barrier,             // 屏障
        
        // 减益类 Debuff
        PowerDown,           // 攻击力下降
        DefenseDown,         // 防御力下降
        SpeedDown,           // 速度下降
        MagicDown,           // 魔力下降
        AccuracyDown,        // 命中下降
        
        // 特殊状态
        Invisible,           // 隐身
        Taunt,               // 嘲讽
        Reflect,             // 反弹
        AutoRevive           // 复活
    }
    
    /// <summary>
    /// 状态效果分类
    /// </summary>
    public enum StatusEffectCategory
    {
        Buff,               // 增益效果
        Debuff,             // 减益效果
        Control,            // 控制效果
        DoT,                // 持续伤害
        Special             // 特殊效果
    }
    
    /// <summary>
    /// 状态效果来源
    /// </summary>
    public enum StatusSource
    {
        Skill,              // 技能附加
        Equipment,          // 装备附加
        Environment,        // 环境效果
        Passive             // 被动技能
    }
    
    #endregion
    
    #region 种族
    
    /// <summary>
    /// 种族类型
    /// </summary>
    public enum Race
    {
        Human,              // 人族
        Beast,              // 兽人族
        Vampire,            // 吸血鬼
        Angel,              // 天使族
        Demon               // 魔人族
    }
    
    /// <summary>
    /// 稀有度
    /// </summary>
    public enum Rarity
    {
        Common = 1,          // 普通
        Uncommon = 2,        // 优秀
        Rare = 3,           // 稀有
        Epic = 4,           // 史诗
        Legendary = 5       // 传说
    }
    
    /// <summary>
    /// 战斗定位
    /// </summary>
    public enum BattleRole
    {
        Attacker,           // 攻击型
        Defender,           // 防御型
        Support,            // 支援型
        Healer,             // 治疗型
        Balanced            // 均衡型
    }
    
    /// <summary>
    /// 武器类型
    /// </summary>
    public enum WeaponType
    {
        Sword,
        Staff,
        Bow,
        Dagger,
        Hammer,
        Fist,
        Shield
    }
    
    #endregion
    
    #region 行动点系统
    
    /// <summary>
    /// 行动类型
    /// </summary>
    public enum ActionType
    {
        Attack,              // 攻击
        Skill,               // 技能
        Defend,              // 防御
        Item,                // 道具
        Ultimate,            // 终极技能
        Flee,                // 逃跑
        Guard,               // 守护队友
        Switch               // 换人
    }
    
    #endregion
    
    #region ATB相关
    
    /// <summary>
    /// ATB槽状态
    /// </summary>
    public enum ATBState
    {
        Charging,            // 充能中
        Ready,               // 待命（可行动）
        Acting,              // 行动中
        Waiting,             // 等待（已行动过）
        Disabled             // 禁用
    }
    
    #endregion
    
    #region 动画事件
    
    /// <summary>
    /// 战斗动画触发
    /// </summary>
    public enum BattleAnimation
    {
        Idle,
        Attack,
        Skill,
        Hit,
        Dodge,
        Die,
        Victory,
        Defend,
        Buff,
        Debuff,
        Heal,
        Ultimate
    }
    
    #endregion
}
