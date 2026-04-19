// SetBonus.cs - 套装效果系统
// 定义和管理装备套装效果

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Character;

namespace JRPG.Equipment
{
    /// <summary>
    /// 套装效果数据
    /// </summary>
    [Serializable]
    public class SetBonusData
    {
        [Tooltip("套装ID")]
        public string setId;
        
        [Tooltip("套装名称")]
        public string setName;
        
        [Tooltip("套装描述")]
        public string description;
        
        [Tooltip("2件套效果")]
        public SetEffect effect2Piece;
        
        [Tooltip("4件套效果")]
        public SetEffect effect4Piece;
        
        [Tooltip("套装图标")]
        public Sprite icon;
    }
    
    /// <summary>
    /// 套装效果
    /// </summary>
    [Serializable]
    public class SetEffect
    {
        [Tooltip("效果描述")]
        public string description;
        
        [Tooltip("效果类型")]
        public SetEffectType effectType;
        
        [Tooltip("属性加成")]
        public CharacterStats stats;
        
        [Tooltip("数值加成（如伤害加成百分比）")]
        public float value;
    }
    
    /// <summary>
    /// 套装效果类型
    /// </summary>
    public enum SetEffectType
    {
        /// <summary>无效果</summary>
        None,
        
        /// <summary>属性加成</summary>
        StatBonus,
        
        /// <summary>伤害加成</summary>
        DamageBonus,
        
        /// <summary>伤害减免</summary>
        DamageReduction,
        
        /// <summary>元素伤害加成</summary>
        ElementDamageBonus,
        
        /// <summary>暴击加成</summary>
        CritBonus,
        
        /// <summary>特殊效果</summary>
        Special,
        
        /// <summary>行动后再动</summary>
        ExtraTurn,
        
        /// <summary>复活</summary>
        Revival
    }
    
    /// <summary>
    /// 套装效果管理器
    /// </summary>
    public static class SetBonus
    {
        #region 套装配置
        
        /// <summary>
        /// 套装效果表
        /// </summary>
        private static readonly Dictionary<string, SetBonusData> _setBonusTable = new Dictionary<string, SetBonusData>
        {
            // 烈焰之心套装
            {
                "fury_heart",
                new SetBonusData
                {
                    setId = "fury_heart",
                    setName = "烈焰之心",
                    description = "炽热的火焰在装备中燃烧，灼烧一切敌人",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.DamageBonus,
                        description = "攻击+15%",
                        value = 0.15f,
                        stats = new CharacterStats { Atk = 0 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.ElementDamageBonus,
                        description = "火属性伤害+30%",
                        value = 0.30f,
                        stats = new CharacterStats()
                    }
                }
            },
            
            // 冰霜守护套装
            {
                "frost_guard",
                new SetBonusData
                {
                    setId = "frost_guard",
                    setName = "冰霜守护",
                    description = "寒冰之力环绕周身，冻结敌人的进攻",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.DamageReduction,
                        description = "防御+20%",
                        value = 0.20f,
                        stats = new CharacterStats { Def = 0 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.Special,
                        description = "受击时30%概率冻结敌人",
                        value = 0.30f,
                        stats = new CharacterStats()
                    }
                }
            },
            
            // 疾风之翼套装
            {
                "wind_wing",
                new SetBonusData
                {
                    setId = "wind_wing",
                    setName = "疾风之翼",
                    description = "风之羽翼赋予穿戴者闪电般的速度",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.StatBonus,
                        description = "速度+15",
                        value = 15f,
                        stats = new CharacterStats { Speed = 15 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.ExtraTurn,
                        description = "行动后30%概率再动",
                        value = 0.30f,
                        stats = new CharacterStats()
                    }
                }
            },
            
            // 不朽之魂套装
            {
                "immortal_soul",
                new SetBonusData
                {
                    setId = "immortal_soul",
                    setName = "不朽之魂",
                    description = "灵魂永不消逝，即使倒下也能再次站起",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.StatBonus,
                        description = "HP+25%",
                        value = 0.25f,
                        stats = new CharacterStats { MaxHp = 0 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.Revival,
                        description = "死亡时复活一次(50%HP)",
                        value = 0.50f,
                        stats = new CharacterStats()
                    }
                }
            },
            
            // 虚空之力套装
            {
                "void_power",
                new SetBonusData
                {
                    setId = "void_power",
                    setName = "虚空之力",
                    description = "来自虚空深处的力量，增强暴击伤害",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.CritBonus,
                        description = "暴击+10%",
                        value = 0.10f,
                        stats = new CharacterStats { CritRate = 0 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.DamageBonus,
                        description = "暴击伤害+50%",
                        value = 0.50f,
                        stats = new CharacterStats { CritDamage = 0 }
                    }
                }
            },
            
            // 神圣之光套装
            {
                "holy_light",
                new SetBonusData
                {
                    setId = "holy_light",
                    setName = "神圣之光",
                    description = "圣光庇护，对邪恶生物造成额外伤害",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.StatBonus,
                        description = "HP+20%",
                        value = 0.20f,
                        stats = new CharacterStats { MaxHp = 0 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.DamageBonus,
                        description = "对邪恶标签敌人伤害+35%",
                        value = 0.35f,
                        stats = new CharacterStats()
                    }
                }
            },
            
            // 暗影收割套装
            {
                "shadow_reaper",
                new SetBonusData
                {
                    setId = "shadow_reaper",
                    setName = "暗影收割",
                    description = "暗影缠绕，每一次攻击都如同死神降临",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.StatBonus,
                        description = "攻击+15%",
                        value = 0.15f,
                        stats = new CharacterStats { Atk = 0 }
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.DamageBonus,
                        description = "攻击附加20%当前生命值伤害",
                        value = 0.20f,
                        stats = new CharacterStats()
                    }
                }
            },
            
            // 元素和谐套装
            {
                "elemental_harmony",
                new SetBonusData
                {
                    setId = "elemental_harmony",
                    setName = "元素和谐",
                    description = "四大元素完美平衡，发挥最大威力",
                    effect2Piece = new SetEffect
                    {
                        effectType = SetEffectType.StatBonus,
                        description = "全属性+8%",
                        value = 0.08f,
                        stats = new CharacterStats()
                    },
                    effect4Piece = new SetEffect
                    {
                        effectType = SetEffectType.DamageBonus,
                        description = "所有元素伤害+25%",
                        value = 0.25f,
                        stats = new CharacterStats()
                    }
                }
            }
        };
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取套装数据
        /// </summary>
        public static SetBonusData GetSetData(string setId)
        {
            return _setBonusTable.TryGetValue(setId, out var data) ? data : null;
        }
        
        /// <summary>
        /// 根据装备数量获取套装效果
        /// </summary>
        public static SetEffect GetSetBonus(string setId, int pieceCount)
        {
            var setData = GetSetData(setId);
            if (setData == null)
                return null;
            
            if (pieceCount >= 4)
                return setData.effect4Piece;
            else if (pieceCount >= 2)
                return setData.effect2Piece;
            
            return null;
        }
        
        /// <summary>
        /// 获取套装效果描述
        /// </summary>
        public static string GetSetBonusDescription(string setId, int pieceCount)
        {
            var effect = GetSetBonus(setId, pieceCount);
            return effect?.description ?? "";
        }
        
        /// <summary>
        /// 计算套装属性加成
        /// </summary>
        public static CharacterStats CalculateSetStats(string setId, int pieceCount)
        {
            var effect = GetSetBonus(setId, pieceCount);
            if (effect == null || effect.effectType != SetEffectType.StatBonus)
                return new CharacterStats();
            
            return effect.stats;
        }
        
        /// <summary>
        /// 获取所有套装列表
        /// </summary>
        public static List<SetBonusData> GetAllSets()
        {
            return new List<SetBonusData>(_setBonusTable.Values);
        }
        
        /// <summary>
        /// 获取套装名称
        /// </summary>
        public static string GetSetName(string setId)
        {
            var data = GetSetData(setId);
            return data?.setName ?? "未知套装";
        }
        
        #endregion
        
        #region 套装效果应用
        
        /// <summary>
        /// 应用套装效果到角色
        /// </summary>
        public static void ApplySetEffects(CharacterInstance character, Dictionary<string, int> setCounts)
        {
            foreach (var kvp in setCounts)
            {
                var effect = GetSetBonus(kvp.Key, kvp.Value);
                if (effect == null)
                    continue;
                
                ApplySetEffect(character, effect);
            }
        }
        
        /// <summary>
        /// 应用单个套装效果
        /// </summary>
        private static void ApplySetEffect(CharacterInstance character, SetEffect effect)
        {
            switch (effect.effectType)
            {
                case SetEffectType.StatBonus:
                    // 属性加成已在CharacterStatsCalculator中计算
                    break;
                    
                case SetEffectType.DamageBonus:
                case SetEffectType.ElementDamageBonus:
                case SetEffectType.CritBonus:
                case SetEffectType.DamageReduction:
                    // 这些效果在战斗时计算
                    break;
                    
                case SetEffectType.ExtraTurn:
                    // 行动后再动效果
                    character.equipmentBonus.Atk += (int)(effect.value * 100);
                    break;
            }
        }
        
        #endregion
        
        #region 套装信息查询
        
        /// <summary>
        /// 获取角色的套装信息
        /// </summary>
        public static Dictionary<string, int> GetCharacterSetInfo(CharacterInstance character)
        {
            var setCounts = new Dictionary<string, int>();
            
            foreach (var equipmentId in character.equippedItems)
            {
                if (string.IsNullOrEmpty(equipmentId))
                    continue;
                
                var equipment = EquipmentManager.Instance.GetEquipment(equipmentId);
                if (equipment == null)
                    continue;
                
                var data = equipment.GetData();
                if (data == null || string.IsNullOrEmpty(data.setId))
                    continue;
                
                if (!setCounts.ContainsKey(data.setId))
                    setCounts[data.setId] = 0;
                setCounts[data.setId]++;
            }
            
            return setCounts;
        }
        
        /// <summary>
        /// 获取角色的套装效果描述
        /// </summary>
        public static List<string> GetCharacterSetDescriptions(CharacterInstance character)
        {
            var descriptions = new List<string>();
            var setInfo = GetCharacterSetInfo(character);
            
            foreach (var kvp in setInfo)
            {
                var effect = GetSetBonus(kvp.Key, kvp.Value);
                if (effect != null)
                {
                    string setName = GetSetName(kvp.Key);
                    descriptions.Add($"[{setName}] {kvp.Value}件: {effect.description}");
                }
            }
            
            return descriptions;
        }
        
        #endregion
    }
}
