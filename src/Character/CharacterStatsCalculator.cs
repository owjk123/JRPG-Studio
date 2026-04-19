// CharacterStatsCalculator.cs - 角色属性计算器
// 计算角色的最终属性，包括基础属性、成长加成、装备加成等

using System;
using System.Collections.Generic;
using UnityEngine;

// EquipmentSlot, EquipmentManager, SetBonus 定义在 Equipment 命名空间

namespace JRPG.Character
{
    /// <summary>
    /// 角色属性计算器 - 负责计算角色最终属性
    /// </summary>
    public class CharacterStatsCalculator
    {
        #region 单例
        
        private static CharacterStatsCalculator _instance;
        public static CharacterStatsCalculator Instance => _instance ??= new CharacterStatsCalculator();
        
        #endregion
        
        #region 突破属性倍率
        
        /// <summary>
        /// 突破阶段的属性加成倍率
        /// </summary>
        private static readonly float[] BreakthroughMultipliers = 
        {
            1.0f,   // 0阶突破
            1.1f,   // 1阶突破 (+10%)
            1.2f,   // 2阶突破 (+20%)
            1.3f,   // 3阶突破 (+30%)
            1.5f,   // 4阶突破 (+50%)
            1.8f    // 5阶突破 (+80%)
        };
        
        #endregion
        
        #region 羁绊属性加成
        
        /// <summary>
        /// 羁绊等级提供的属性加成百分比
        /// </summary>
        private static readonly float[] BondBonusPercents = 
        {
            0.00f,  // Lv.1
            0.02f,  // Lv.2 (+2%)
            0.05f,  // Lv.3 (+5%)
            0.08f,  // Lv.4 (+8%)
            0.12f,  // Lv.5 (+12%)
            0.15f,  // Lv.6 (+15%)
            0.18f,  // Lv.7 (+18%)
            0.20f,  // Lv.8 (+20%)
            0.22f,  // Lv.9 (+22%)
            0.25f   // Lv.10 (+25%)
        };
        
        #endregion
        
        #region 命座属性加成
        
        /// <summary>
        /// 每个命座的属性加成
        /// </summary>
        private const float ConstellationBonusPerLevel = 0.05f; // 每个命座+5%
        
        #endregion
        
        #region 核心计算方法
        
        /// <summary>
        /// 计算角色的总属性
        /// </summary>
        /// <param name="instance">角色实例</param>
        /// <returns>最终属性</returns>
        public CharacterStats CalculateTotalStats(CharacterInstance instance)
        {
            var characterData = instance.GetCharacterData();
            if (characterData == null)
            {
                Debug.LogError($"CharacterData not found for characterId: {instance.characterId}");
                return new CharacterStats();
            }
            
            // 1. 计算基础属性（根据等级和突破）
            var baseStats = CalculateBaseStats(characterData, instance.level, instance.breakthroughCount);
            
            // 2. 获取种族加成
            var raceBonus = GetRaceBonus(characterData);
            
            // 3. 获取羁绊加成百分比
            float bondPercent = GetBondBonusPercent(instance.affectionLevel);
            
            // 4. 获取命座加成
            var constellationBonus = CalculateConstellationBonus(baseStats, instance.constellationLevel);
            
            // 5. 获取装备加成
            var equipmentBonus = CalculateEquipmentBonus(instance);
            
            // 6. 累加所有属性
            var totalStats = new CharacterStats();
            
            // 基础属性 + 种族加成 + 命座加成 + 装备加成
            totalStats.MaxHp = baseStats.MaxHp + raceBonus.MaxHp + constellationBonus.MaxHp + equipmentBonus.MaxHp;
            totalStats.Atk = baseStats.Atk + raceBonus.Atk + constellationBonus.Atk + equipmentBonus.Atk;
            totalStats.Def = baseStats.Def + raceBonus.Def + constellationBonus.Def + equipmentBonus.Def;
            totalStats.Speed = baseStats.Speed + raceBonus.Speed + constellationBonus.Speed + equipmentBonus.Speed;
            totalStats.MaxMp = baseStats.MaxMp + raceBonus.MaxMp + constellationBonus.MaxMp + equipmentBonus.MaxMp;
            totalStats.CritRate = baseStats.CritRate + raceBonus.CritRate + constellationBonus.CritRate + equipmentBonus.CritRate;
            totalStats.CritDamage = baseStats.CritDamage + raceBonus.CritDamage + constellationBonus.CritDamage + equipmentBonus.CritDamage;
            
            // 羁绊加成（百分比）
            float multiplier = 1f + bondPercent;
            totalStats.MaxHp = Mathf.RoundToInt(totalStats.MaxHp * multiplier);
            totalStats.Atk = Mathf.RoundToInt(totalStats.Atk * multiplier);
            totalStats.Def = Mathf.RoundToInt(totalStats.Def * multiplier);
            
            // 套装效果加成
            var setBonus = CalculateSetBonus(instance);
            totalStats.MaxHp += setBonus.MaxHp;
            totalStats.Atk += setBonus.Atk;
            totalStats.Def += setBonus.Def;
            totalStats.Speed += setBonus.Speed;
            totalStats.CritRate += setBonus.CritRate;
            totalStats.CritDamage += setBonus.CritDamage;
            
            // 限制属性在合理范围内
            totalStats.MaxHp = Math.Max(1, totalStats.MaxHp);
            totalStats.Atk = Math.Max(1, totalStats.Atk);
            totalStats.Def = Math.Max(0, totalStats.Def);
            totalStats.Speed = Math.Max(1, totalStats.Speed);
            totalStats.CritRate = Mathf.Clamp(totalStats.CritRate, 0f, 1f);
            totalStats.CritDamage = Math.Max(1f, totalStats.CritDamage);
            
            return totalStats;
        }
        
        /// <summary>
        /// 计算基础属性（等级成长 + 突破加成）
        /// </summary>
        public CharacterStats CalculateBaseStats(CharacterData data, int level, int breakthroughCount)
        {
            var stats = data.GetStatsAtLevel(level);
            
            // 突破倍率加成
            float breakthroughMultiplier = BreakthroughMultipliers[Mathf.Clamp(breakthroughCount, 0, 5)];
            
            stats.MaxHp = Mathf.RoundToInt(stats.MaxHp * breakthroughMultiplier);
            stats.Atk = Mathf.RoundToInt(stats.Atk * breakthroughMultiplier);
            stats.Def = Mathf.RoundToInt(stats.Def * breakthroughMultiplier);
            stats.MaxMp = Mathf.RoundToInt(stats.MaxMp * breakthroughMultiplier);
            
            return stats;
        }
        
        /// <summary>
        /// 获取种族属性加成
        /// </summary>
        public CharacterStats GetRaceBonus(CharacterData data)
        {
            var bonus = new CharacterStats();
            
            // 根据种族添加对应的属性加成
            // 这里简化处理，实际应该从CharacterData中读取种族类型
            // 人族：HP +5%, MP +10%, 力量 +5%, 魔力 +5%, 速度 +5%, 防御 +5%, 魔防 +5%
            
            return bonus;
        }
        
        /// <summary>
        /// 获取羁绊加成百分比
        /// </summary>
        public float GetBondBonusPercent(int bondLevel)
        {
            bondLevel = Mathf.Clamp(bondLevel, 1, 10);
            return BondBonusPercents[bondLevel - 1];
        }
        
        /// <summary>
        /// 计算命座加成
        /// </summary>
        public CharacterStats CalculateConstellationBonus(CharacterStats baseStats, int constellationLevel)
        {
            var bonus = new CharacterStats();
            
            float bonusPercent = constellationLevel * ConstellationBonusPerLevel;
            
            bonus.MaxHp = Mathf.RoundToInt(baseStats.MaxHp * bonusPercent);
            bonus.Atk = Mathf.RoundToInt(baseStats.Atk * bonusPercent);
            bonus.Def = Mathf.RoundToInt(baseStats.Def * bonusPercent);
            
            return bonus;
        }
        
        /// <summary>
        /// 计算装备加成
        /// </summary>
        public CharacterStats CalculateEquipmentBonus(CharacterInstance instance)
        {
            var totalBonus = new CharacterStats();
            
            if (instance == null || instance.equippedItems == null)
                return totalBonus;
            
            for (int i = 0; i < instance.equippedItems.Length; i++)
            {
                if (string.IsNullOrEmpty(instance.equippedItems[i]))
                    continue;
                
                var equipmentData = EquipmentManager.Instance.GetEquipmentData(instance.equippedItems[i]);
                if (equipmentData != null)
                {
                    totalBonus += equipmentData.stats;
                }
            }
            
            return totalBonus;
        }
        
        /// <summary>
        /// 计算套装效果加成
        /// </summary>
        public CharacterStats CalculateSetBonus(CharacterInstance instance)
        {
            var totalBonus = new CharacterStats();
            
            if (instance == null || instance.equippedItems == null)
                return totalBonus;
            
            // 统计各套装装备数量
            var setCount = new Dictionary<string, int>();
            
            foreach (var itemId in instance.equippedItems)
            {
                if (string.IsNullOrEmpty(itemId))
                    continue;
                
                var equipmentData = EquipmentManager.Instance.GetEquipmentData(itemId);
                if (equipmentData != null && !string.IsNullOrEmpty(equipmentData.setId))
                {
                    if (!setCount.ContainsKey(equipmentData.setId))
                        setCount[equipmentData.setId] = 0;
                    setCount[equipmentData.setId]++;
                }
            }
            
            // 计算套装效果
            foreach (var kvp in setCount)
            {
                var setBonus = SetBonus.GetSetBonus(kvp.Key, kvp.Value);
                if (setBonus != null)
                {
                    totalBonus += setBonus.stats;
                }
            }
            
            return totalBonus;
        }
        
        #endregion
        
        #region 战斗属性计算
        
        /// <summary>
        /// 计算最终伤害
        /// </summary>
        public int CalculateDamage(int baseDamage, CharacterStats attacker, CharacterStats defender, 
            Element element, bool isCritical)
        {
            // 基础伤害公式：攻击力 × (1 - 防御减免)
            float defenseReduction = defender.Def / (defender.Def + 200f);
            float damage = baseDamage * (1f - defenseReduction);
            
            // 暴击加成
            if (isCritical)
            {
                damage *= attacker.CritDamage;
            }
            
            // 属性克制加成（简化处理）
            damage = ApplyElementBonus(damage, element, defender.Element);
            
            return Mathf.RoundToInt(damage);
        }
        
        /// <summary>
        /// 应用属性克制
        /// </summary>
        private float ApplyElementBonus(float damage, Element attackElement, Element defenseElement)
        {
            // 属性克制表：火>风>地>水>火，光暗互克
            float bonus = 1f;
            
            // 火克风
            if (attackElement == Element.Fire && defenseElement == Element.Wind)
                bonus = 1.5f;
            else if (attackElement == Element.Wind && defenseElement == Element.Fire)
                bonus = 0.5f;
            
            // 地克水
            if (attackElement == Element.Earth && defenseElement == Element.Water)
                bonus = 1.5f;
            else if (attackElement == Element.Water && defenseElement == Element.Earth)
                bonus = 0.5f;
            
            // 光暗互克
            if ((attackElement == Element.Light && defenseElement == Element.Dark) ||
                (attackElement == Element.Dark && defenseElement == Element.Light))
                bonus = 1.5f;
            
            return damage * bonus;
        }
        
        /// <summary>
        /// 计算治疗量
        /// </summary>
        public int CalculateHealing(int baseHeal, CharacterStats healer)
        {
            // 治疗量 = 基础治疗 × (1 + 魔力加成系数)
            float healBonus = 1f + (healer.MaxMp * 0.01f);
            return Mathf.RoundToInt(baseHeal * healBonus);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 角色属性结构
    /// </summary>
    [Serializable]
    public struct CharacterStats
    {
        public int MaxHp;
        public int Atk;
        public int Def;
        public int Speed;
        public int MaxMp;
        public float CritRate;
        public float CritDamage;
        public Element Element;
        
        // 运算符重载
        public static CharacterStats operator +(CharacterStats a, CharacterStats b)
        {
            return new CharacterStats
            {
                MaxHp = a.MaxHp + b.MaxHp,
                Atk = a.Atk + b.Atk,
                Def = a.Def + b.Def,
                Speed = a.Speed + b.Speed,
                MaxMp = a.MaxMp + b.MaxMp,
                CritRate = a.CritRate + b.CritRate,
                CritDamage = a.CritDamage + b.CritDamage,
                Element = a.Element
            };
        }
        
        public static CharacterStats operator *(CharacterStats a, float multiplier)
        {
            return new CharacterStats
            {
                MaxHp = Mathf.RoundToInt(a.MaxHp * multiplier),
                Atk = Mathf.RoundToInt(a.Atk * multiplier),
                Def = Mathf.RoundToInt(a.Def * multiplier),
                Speed = Mathf.RoundToInt(a.Speed * multiplier),
                MaxMp = Mathf.RoundToInt(a.MaxMp * multiplier),
                CritRate = a.CritRate,
                CritDamage = a.CritDamage,
                Element = a.Element
            };
        }
        
        public override string ToString()
        {
            return $"HP:{MaxHp} ATK:{Atk} DEF:{Def} SPD:{Speed} MP:{MaxMp} CRT:{CritRate:P0} CRD:{CritDamage:P0}";
        }
    }
}
