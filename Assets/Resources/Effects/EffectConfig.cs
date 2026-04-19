// EffectConfig.cs - 技能特效配置
// 定义所有技能特效的路径，支持特效类型分类管理和默认占位符

using UnityEngine;
using System.Collections.Generic;
using JRPG.Battle;

namespace JRPG.Assets.Effects
{
    /// <summary>
    /// 技能特效配置 - ScriptableObject管理所有战斗特效和动画路径
    /// 支持按技能类型、元素类型分组管理
    /// </summary>
    [CreateAssetMenu(fileName = "EffectConfig", menuName = "JRPG/Effects/Effect Config")]
    public class EffectConfig : ScriptableObject
    {
        #region Singleton
        
        private static EffectConfig _instance;
        public static EffectConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EffectConfig>("Effects/EffectConfig");
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region 默认占位符
        
        [Header("===== 默认占位符 =====")]
        
        [Tooltip("默认打击特效路径")]
        public string defaultHitEffect = "Effects/Default/HitEffect";
        
        [Tooltip("默认光芒特效路径")]
        public string defaultFlashEffect = "Effects/Default/FlashEffect";
        
        [Tooltip("默认粒子特效路径")]
        public string defaultParticleEffect = "Effects/Default/ParticleEffect";
        
        [Tooltip("默认动画路径")]
        public string defaultAnimation = "Effects/Default/DefaultAnimation";
        
        #endregion
        
        #region 通用战斗特效
        
        [Header("===== 通用战斗特效 =====")]
        
        [Tooltip("攻击命中特效")]
        public string attackHitEffect = "Effects/Battle/AttackHit";
        
        [Tooltip("暴击命中特效")]
        public string criticalHitEffect = "Effects/Battle/CriticalHit";
        
        [Tooltip("闪避特效")]
        public string dodgeEffect = "Effects/Battle/Dodge";
        
        [Tooltip("防御特效")]
        public string defendEffect = "Effects/Battle/Defend";
        
        [Tooltip("格挡特效")]
        public string blockEffect = "Effects/Battle/Block";
        
        [Tooltip("反击特效")]
        public string counterEffect = "Effects/Battle/Counter";
        
        [Tooltip("治疗特效")]
        public string healEffect = "Effects/Battle/Heal";
        
        [Tooltip("复活特效")]
        public string reviveEffect = "Effects/Battle/Revive";
        
        [Tooltip("护盾生效特效")]
        public string shieldEffect = "Effects/Battle/Shield";
        
        [Tooltip("无敌特效")]
        public string invincibleEffect = "Effects/Battle/Invincible";
        
        [Tooltip("增益特效")]
        public string buffEffect = "Effects/Battle/Buff";
        
        [Tooltip("减益特效")]
        public string debuffEffect = "Effects/Battle/Debuff";
        
        [Tooltip("死亡消散特效")]
        public string deathEffect = "Effects/Battle/Death";
        
        [Tooltip("逃跑成功特效")]
        public string fleeEffect = "Effects/Battle/Flee";
        
        #endregion
        
        #region 元素特效
        
        [Header("===== 元素特效 =====")]
        
        // 火属性特效
        [Tooltip("火属性攻击特效")]
        public string fireAttackEffect = "Effects/Element/Fire/Attack";
        
        [Tooltip("火属性持续燃烧特效")]
        public string fireBurnEffect = "Effects/Element/Fire/Burn";
        
        [Tooltip("火属性爆发特效")]
        public string fireExplosionEffect = "Effects/Element/Fire/Explosion";
        
        // 冰属性特效
        [Tooltip("冰属性攻击特效")]
        public string iceAttackEffect = "Effects/Element/Ice/Attack";
        
        [Tooltip("冰属性冻结特效")]
        public string iceFreezeEffect = "Effects/Element/Ice/Freeze";
        
        [Tooltip("冰属性冰刺特效")]
        public string iceSpikeEffect = "Effects/Element/Ice/Spike";
        
        // 雷属性特效
        [Tooltip("雷属性攻击特效")]
        public string thunderAttackEffect = "Effects/Element/Thunder/Attack";
        
        [Tooltip("雷属性闪电链特效")]
        public string thunderChainEffect = "Effects/Element/Thunder/Chain";
        
        [Tooltip("雷属性落雷特效")]
        public string thunderStrikeEffect = "Effects/Element/Thunder/Strike";
        
        // 圣光属性特效
        [Tooltip("圣光攻击特效")]
        public string lightAttackEffect = "Effects/Element/Light/Attack";
        
        [Tooltip("圣光净化特效")]
        public string lightPurifyEffect = "Effects/Element/Light/Purify";
        
        [Tooltip("圣光神圣之光特效")]
        public string lightDivineEffect = "Effects/Element/Light/Divine";
        
        // 暗影属性特效
        [Tooltip("暗影攻击特效")]
        public string darkAttackEffect = "Effects/Element/Dark/Attack";
        
        [Tooltip("暗影腐蚀特效")]
        public string darkCorruptionEffect = "Effects/Element/Dark/Corruption";
        
        [Tooltip("暗影诅咒特效")]
        public string darkCurseEffect = "Effects/Element/Dark/Curse";
        
        // 风属性特效
        [Tooltip("风属性攻击特效")]
        public string windAttackEffect = "Effects/Element/Wind/Attack";
        
        [Tooltip("风属性龙卷特效")]
        public string windTornadoEffect = "Effects/Element/Wind/Tornado";
        
        [Tooltip("风属性切割特效")]
        public string windSlashEffect = "Effects/Element/Wind/Slash";
        
        // 土属性特效
        [Tooltip("土属性攻击特效")]
        public string earthAttackEffect = "Effects/Element/Earth/Attack";
        
        [Tooltip("土属性陨石特效")]
        public string earthMeteorEffect = "Effects/Element/Earth/Meteor";
        
        [Tooltip("土属性岩石特效")]
        public string earthRockEffect = "Effects/Element/Earth/Rock";
        
        // 水属性特效
        [Tooltip("水属性攻击特效")]
        public string waterAttackEffect = "Effects/Element/Water/Attack";
        
        [Tooltip("水属性海浪特效")]
        public string waterWaveEffect = "Effects/Element/Water/Wave";
        
        [Tooltip("水属性漩涡特效")]
        public string waterVortexEffect = "Effects/Element/Water/Vortex";
        
        #endregion
        
        #region 技能类型特效
        
        [Header("===== 技能类型特效 =====")]
        
        // 物理技能特效
        [Tooltip("物理攻击特效基础路径")]
        public string physicalAttackBase = "Effects/Skills/Physical/";
        
        [Tooltip("物理斩击特效")]
        public string slashEffect = "Effects/Skills/Physical/Slash";
        
        [Tooltip("物理突刺特效")]
        public string pierceEffect = "Effects/Skills/Physical/Pierce";
        
        [Tooltip("物理打击特效")]
        public string strikeEffect = "Effects/Skills/Physical/Strike";
        
        // 魔法技能特效
        [Tooltip("魔法攻击特效基础路径")]
        public string magicAttackBase = "Effects/Skills/Magic/";
        
        [Tooltip("魔法爆发特效")]
        public string magicBurstEffect = "Effects/Skills/Magic/Burst";
        
        [Tooltip("魔法光环特效")]
        public string magicAuraEffect = "Effects/Skills/Magic/Aura";
        
        [Tooltip("魔法符文特效")]
        public string magicRuneEffect = "Effects/Skills/Magic/Rune";
        
        // 治疗技能特效
        [Tooltip("治疗光环特效")]
        public string healingAuraEffect = "Effects/Skills/Healing/Aura";
        
        [Tooltip("治疗雨特效")]
        public string healingRainEffect = "Effects/Skills/Healing/Rain";
        
        [Tooltip("治愈之光特效")]
        public string healingLightEffect = "Effects/Skills/Healing/Light";
        
        // 终极技能特效
        [Tooltip("终极技能特效基础路径")]
        public string ultimateSkillBase = "Effects/Skills/Ultimate/";
        
        [Tooltip("终极技能全屏特效")]
        public string ultimateFullScreenEffect = "Effects/Skills/Ultimate/FullScreen";
        
        [Tooltip("终极技能爆发特效")]
        public string ultimateBurstEffect = "Effects/Skills/Ultimate/Burst";
        
        #endregion
        
        #region 状态效果特效
        
        [Header("===== 状态效果特效 =====")]
        
        // 眩晕
        [Tooltip("眩晕状态特效")]
        public string stunEffect = "Effects/Status/Stun";
        
        // 冻结
        [Tooltip("冻结状态特效")]
        public string freezeEffect = "Effects/Status/Freeze";
        
        // 睡眠
        [Tooltip("睡眠状态特效")]
        public string sleepEffect = "Effects/Status/Sleep";
        
        // 石化
        [Tooltip("石化状态特效")]
        public string petrifyEffect = "Effects/Status/Petrify";
        
        // 灼烧
        [Tooltip("灼烧状态特效")]
        public string burnEffect = "Effects/Status/Burn";
        
        // 中毒
        [Tooltip("中毒状态特效")]
        public string poisonEffect = "Effects/Status/Poison";
        
        // 出血
        [Tooltip("出血状态特效")]
        public string bleedEffect = "Effects/Status/Bleed";
        
        // 沉默
        [Tooltip("沉默状态特效")]
        public string silenceEffect = "Effects/Status/Silence";
        
        // 减速
        [Tooltip("减速状态特效")]
        public string slowEffect = "Effects/Status/Slow";
        
        // 麻痹
        [Tooltip("麻痹状态特效")]
        public string paralyzeEffect = "Effects/Status/Paralyze";
        
        #endregion
        
        #region UI特效
        
        [Header("===== UI特效 =====")]
        
        [Tooltip("升级特效")]
        public string levelUpEffect = "Effects/UI/LevelUp";
        
        [Tooltip("突破特效")]
        public string breakthroughEffect = "Effects/UI/Breakthrough";
        
        [Tooltip("抽卡金光特效")]
        public string gachaGoldEffect = "Effects/UI/GachaGold";
        
        [Tooltip("抽卡彩光特效")]
        public string gachaRainbowEffect = "Effects/UI/GachaRainbow";
        
        [Tooltip("获得角色特效")]
        public string obtainCharacterEffect = "Effects/UI/ObtainCharacter";
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取元素攻击特效路径
        /// </summary>
        public string GetElementAttackEffect(Element element)
        {
            switch (element)
            {
                case Element.Fire: return fireAttackEffect;
                case Element.Ice: return iceAttackEffect;
                case Element.Thunder: return thunderAttackEffect;
                case Element.Light: return lightAttackEffect;
                case Element.Dark: return darkAttackEffect;
                case Element.Wind: return windAttackEffect;
                case Element.Earth: return earthAttackEffect;
                case Element.Water: return waterAttackEffect;
                default: return defaultFlashEffect;
            }
        }
        
        /// <summary>
        /// 获取元素持续伤害特效路径
        /// </summary>
        public string GetElementDotEffect(Element element)
        {
            switch (element)
            {
                case Element.Fire: return fireBurnEffect;
                case Element.Ice: return iceFreezeEffect;
                case Element.Thunder: return thunderChainEffect;
                case Element.Dark: return darkCorruptionEffect;
                default: return defaultParticleEffect;
            }
        }
        
        /// <summary>
        /// 获取状态效果特效路径
        /// </summary>
        public string GetStatusEffectPath(StatusEffectType statusType)
        {
            switch (statusType)
            {
                case StatusEffectType.Stun: return stunEffect;
                case StatusEffectType.Freeze: return freezeEffect;
                case StatusEffectType.Sleep: return sleepEffect;
                case StatusEffectType.Petrify: return petrifyEffect;
                case StatusEffectType.Burn: return burnEffect;
                case StatusEffectType.Poison: return poisonEffect;
                case StatusEffectType.Bleed: return bleedEffect;
                case StatusEffectType.Silence: return silenceEffect;
                case StatusEffectType.Slow: return slowEffect;
                case StatusEffectType.Paralyze: return paralyzeEffect;
                case StatusEffectType.Buff:
                case StatusEffectType.PowerUp:
                case StatusEffectType.DefenseUp:
                case StatusEffectType.SpeedUp:
                case StatusEffectType.MagicUp:
                case StatusEffectType.EvadeUp:
                case StatusEffectType.Regen:
                case StatusEffectType.Shield:
                case StatusEffectType.Invincible:
                case StatusEffectType.Barrier:
                    return buffEffect;
                case StatusEffectType.Debuff:
                case StatusEffectType.PowerDown:
                case StatusEffectType.DefenseDown:
                case StatusEffectType.SpeedDown:
                case StatusEffectType.MagicDown:
                case StatusEffectType.AccuracyDown:
                    return debuffEffect;
                default: return defaultFlashEffect;
            }
        }
        
        /// <summary>
        /// 获取伤害类型特效路径
        /// </summary>
        public string GetDamageTypeEffect(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Physical: return slashEffect;
                case DamageType.Magic: return magicBurstEffect;
                case DamageType.Heal: return healEffect;
                case DamageType.True: return strikeEffect;
                default: return attackHitEffect;
            }
        }
        
        /// <summary>
        /// 异步加载特效资源
        /// </summary>
        public void LoadEffectAsync(string path, System.Action<Object> onLoaded)
        {
            Resources.LoadAsync(path).completed += (operation) =>
            {
                var request = operation as ResourceRequest;
                if (request != null && request.asset != null)
                {
                    onLoaded?.Invoke(request.asset);
                }
                else
                {
                    // 加载失败，返回默认特效
                    Resources.LoadAsync(defaultFlashEffect).completed += (defaultOp) =>
                    {
                        var defaultRequest = defaultOp as ResourceRequest;
                        if (defaultRequest != null && defaultRequest.asset != null)
                        {
                            onLoaded?.Invoke(defaultRequest.asset);
                        }
                        else
                        {
                            onLoaded?.Invoke(null);
                        }
                    };
                }
            };
        }
        
        /// <summary>
        /// 实例化特效
        /// </summary>
        public GameObject InstantiateEffect(string path, Vector3 position, Quaternion rotation = default)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                return Object.Instantiate(prefab, position, rotation);
            }
            
            // 返回默认特效
            var defaultPrefab = Resources.Load<GameObject>(defaultFlashEffect);
            if (defaultPrefab != null)
            {
                return Object.Instantiate(defaultPrefab, position, rotation);
            }
            
            return null;
        }
        
        #endregion
    }
}
