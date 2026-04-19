// EquipmentData.cs - 装备数据定义
// 定义装备的ScriptableObject数据

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Character;

namespace JRPG.Equipment
{
    /// <summary>
    /// 装备数据 - ScriptableObject定义
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "JRPG/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        #region 基础信息
        
        [Header("基础信息")]
        [Tooltip("装备唯一ID")]
        public string equipmentId;
        
        [Tooltip("装备名称")]
        public string equipmentName;
        
        [Tooltip("装备描述")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("装备图标")]
        public Sprite icon;
        
        #endregion
        
        #region 分类
        
        [Header("分类")]
        [Tooltip("装备类型")]
        public EquipmentType equipmentType;
        
        [Tooltip("装备槽位")]
        public EquipmentSlot slot;
        
        [Tooltip("稀有度")]
        public EquipmentRarity rarity;
        
        [Tooltip("装备等级")]
        public int level = 1;
        
        [Tooltip("套装ID（为空表示无套装）")]
        public string setId;
        
        #endregion
        
        #region 属性
        
        [Header("主属性")]
        [Tooltip("主属性类型")]
        public StatType mainStatType;
        
        [Tooltip("主属性基础值")]
        public int mainStatValue;
        
        [Tooltip("主属性成长值（每级）")]
        public float mainStatGrowth = 0.05f;
        
        [Header("副属性")]
        [Tooltip("副属性列表")]
        public List<SubStatEntry> subStats = new List<SubStatEntry>();
        
        [Tooltip("副属性数量上限")]
        public int maxSubStats = 2;
        
        #endregion
        
        #region 强化
        
        [Header("强化")]
        [Tooltip("当前强化等级")]
        public int enhanceLevel = 0;
        
        [Tooltip("最大强化等级")]
        public int maxEnhanceLevel = 20;
        
        [Tooltip("强化成功率为100%的等级上限")]
        public int safeEnhanceLevel = 10;
        
        [Tooltip("强化消耗倍率（每级）")]
        public float enhanceCostMultiplier = 1.15f;
        
        #endregion
        
        #region 特殊效果
        
        [Header("特殊效果")]
        [Tooltip("被动技能（可选）")]
        public string passiveSkillId;
        
        [Tooltip("装备特效描述")]
        [TextArea(2, 3)]
        public string specialEffectDescription;
        
        #endregion
        
        #region 属性计算
        
        /// <summary>
        /// 获取当前强化等级的主属性值
        /// </summary>
        public int GetCurrentMainStat()
        {
            float multiplier = 1f + (enhanceLevel * mainStatGrowth);
            return Mathf.RoundToInt(mainStatValue * multiplier);
        }
        
        /// <summary>
        /// 获取该装备提供的属性（用于CharacterStats）
        /// </summary>
        public CharacterStats GetStats()
        {
            var stats = new CharacterStats();
            
            // 添加主属性
            switch (mainStatType)
            {
                case StatType.HP:
                    stats.MaxHp = GetCurrentMainStat();
                    break;
                case StatType.ATK:
                    stats.Atk = GetCurrentMainStat();
                    break;
                case StatType.DEF:
                    stats.Def = GetCurrentMainStat();
                    break;
                case StatType.SPD:
                    stats.Speed = GetCurrentMainStat();
                    break;
                case StatType.CritRate:
                    stats.CritRate = GetCurrentMainStat() / 100f;
                    break;
                case StatType.CritDamage:
                    stats.CritDamage = GetCurrentMainStat() / 100f;
                    break;
            }
            
            // 添加副属性
            foreach (var subStat in subStats)
            {
                switch (subStat.statType)
                {
                    case StatType.HP:
                        stats.MaxHp += subStat.value;
                        break;
                    case StatType.ATK:
                        stats.Atk += subStat.value;
                        break;
                    case StatType.DEF:
                        stats.Def += subStat.value;
                        break;
                    case StatType.SPD:
                        stats.Speed += subStat.value;
                        break;
                    case StatType.CritRate:
                        stats.CritRate += subStat.value / 100f;
                        break;
                    case StatType.CritDamage:
                        stats.CritDamage += subStat.value / 100f;
                        break;
                }
            }
            
            return stats;
        }
        
        /// <summary>
        /// 获取强化消耗金币
        /// </summary>
        public int GetEnhanceGoldCost()
        {
            // 基础消耗 = 装备等级 × 100
            int baseCost = level * 100;
            return Mathf.RoundToInt(baseCost * Mathf.Pow(enhanceCostMultiplier, enhanceLevel));
        }
        
        /// <summary>
        /// 获取强化消耗材料
        /// </summary>
        public string GetEnhanceMaterialId()
        {
            return rarity switch
            {
                EquipmentRarity.N => "enhance_stone_n",
                EquipmentRarity.R => "enhance_stone_r",
                EquipmentRarity.SR => "enhance_stone_sr",
                EquipmentRarity.SSR => "enhance_stone_ssr",
                EquipmentRarity.UR => "enhance_stone_ur",
                _ => "enhance_stone_n"
            };
        }
        
        /// <summary>
        /// 获取强化材料数量
        /// </summary>
        public int GetEnhanceMaterialCount()
        {
            return rarity switch
            {
                EquipmentRarity.N => 1,
                EquipmentRarity.R => 2,
                EquipmentRarity.SR => 5,
                EquipmentRarity.SSR => 10,
                EquipmentRarity.UR => 20,
                _ => 1
            } * (enhanceLevel + 1);
        }
        
        /// <summary>
        /// 获取强化成功率
        /// </summary>
        public float GetEnhanceSuccessRate()
        {
            if (enhanceLevel < safeEnhanceLevel)
                return 1f;
            
            // 超过安全等级后，成功率递减
            int overLevel = enhanceLevel - safeEnhanceLevel;
            return Mathf.Max(0.3f, 1f - (overLevel * 0.1f));
        }
        
        #endregion
        
        #region 装备实例
        
        /// <summary>
        /// 创建装备实例
        /// </summary>
        public EquipmentInstance CreateInstance()
        {
            return new EquipmentInstance(this);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 副属性条目
    /// </summary>
    [Serializable]
    public class SubStatEntry
    {
        [Tooltip("属性类型")]
        public StatType statType;
        
        [Tooltip("属性值")]
        public int value;
        
        [Tooltip("是否随机生成")]
        public bool isRandom = true;
    }
    
    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum StatType
    {
        /// <summary>生命值</summary>
        HP,
        
        /// <summary>攻击力</summary>
        ATK,
        
        /// <summary>防御力</summary>
        DEF,
        
        /// <summary>速度</summary>
        SPD,
        
        /// <summary>暴击率</summary>
        CritRate,
        
        /// <summary>暴击伤害</summary>
        CritDamage,
        
        /// <summary>生命值百分比</summary>
        HPPercent,
        
        /// <summary>攻击力百分比</summary>
        ATKPercent,
        
        /// <summary>防御力百分比</summary>
        DEFPercent,
        
        /// <summary>效果命中</summary>
        EffectHit,
        
        /// <summary>效果抵抗</summary>
        EffectResist
    }
    
    /// <summary>
    /// 装备实例 - 运行时数据
    /// </summary>
    [Serializable]
    public class EquipmentInstance
    {
        #region 数据
        
        [Tooltip("实例唯一ID")]
        public string instanceId;
        
        [Tooltip("装备数据ID")]
        public string equipmentId;
        
        [Tooltip("当前强化等级")]
        public int enhanceLevel;
        
        [Tooltip("装备拥有者角色实例ID")]
        public string ownerCharacterId;
        
        #endregion
        
        #region 构造函数
        
        public EquipmentInstance(EquipmentData data)
        {
            this.instanceId = Guid.NewGuid().ToString();
            this.equipmentId = data.equipmentId;
            this.enhanceLevel = 0;
        }
        
        public EquipmentInstance(string instanceId, string equipmentId, int enhanceLevel)
        {
            this.instanceId = instanceId;
            this.equipmentId = equipmentId;
            this.enhanceLevel = enhanceLevel;
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取装备数据
        /// </summary>
        public EquipmentData GetData()
        {
            return Resources.Load<EquipmentData>($"Data/Equipment/{equipmentId}");
        }
        
        /// <summary>
        /// 获取当前属性
        /// </summary>
        public CharacterStats GetStats()
        {
            var data = GetData();
            if (data == null)
                return new CharacterStats();
            
            return data.GetStats();
        }
        
        #endregion
        
        #region 数据转换
        
        /// <summary>
        /// 转换为保存数据
        /// </summary>
        public EquipmentInstanceData ToSaveData()
        {
            return new EquipmentInstanceData
            {
                instanceId = instanceId,
                equipmentId = equipmentId,
                enhanceLevel = enhanceLevel,
                ownerCharacterId = ownerCharacterId
            };
        }
        
        /// <summary>
        /// 从保存数据恢复
        /// </summary>
        public static EquipmentInstance FromSaveData(EquipmentInstanceData data)
        {
            return new EquipmentInstance(data.instanceId, data.equipmentId, data.enhanceLevel)
            {
                ownerCharacterId = data.ownerCharacterId
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// 装备实例保存数据
    /// </summary>
    [Serializable]
    public class EquipmentInstanceData
    {
        public string instanceId;
        public string equipmentId;
        public int enhanceLevel;
        public string ownerCharacterId;
    }
}
