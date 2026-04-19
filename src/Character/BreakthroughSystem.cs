// BreakthroughSystem.cs - 角色突破系统
// 管理角色的突破进阶

using System;
using System.Collections.Generic;
using UnityEngine;
using JRPG.Resources;

namespace JRPG.Character
{
    /// <summary>
    /// 角色突破系统单例类
    /// </summary>
    public class BreakthroughSystem
    {
        #region 单例
        
        private static BreakthroughSystem _instance;
        public static BreakthroughSystem Instance => _instance ??= new BreakthroughSystem();
        
        #endregion
        
        #region 突破配置
        
        /// <summary>
        /// 突破阶段配置
        /// </summary>
        [Serializable]
        public class BreakthroughConfig
        {
            public int stage;                      // 突破阶段 (0-5)
            public int levelCap;                   // 等级上限
            public int fragmentCost;              // 碎片消耗
            public string elementCrystalId;       // 元素结晶ID（可选）
            public int elementCrystalCost;         // 元素结晶消耗
            public string rareMaterialId;          // 稀有材料ID（可选）
            public int rareMaterialCost;          // 稀有材料消耗
            public string legendaryMaterialId;     // 传说材料ID（可选）
            public int legendaryMaterialCost;     // 传说材料消耗
            public string divineFragmentId;        // 神器碎片ID（可选）
            public int divineFragmentCost;        // 神器碎片消耗
            public string specialReward;           // 特殊奖励描述
        }
        
        /// <summary>
        /// 突破配置表
        /// </summary>
        private static readonly BreakthroughConfig[] BreakthroughConfigs = new BreakthroughConfig[]
        {
            new BreakthroughConfig { stage = 0, levelCap = 100, fragmentCost = 0 },                                           // 0阶 - 无需突破
            new BreakthroughConfig { stage = 1, levelCap = 120, fragmentCost = 30, specialReward = "解锁被动技能槽" },       // 1阶
            new BreakthroughConfig { stage = 2, levelCap = 140, fragmentCost = 50, elementCrystalId = "element_crystal", 
                elementCrystalCost = 10, specialReward = "属性飞跃+10%" },                                                    // 2阶
            new BreakthroughConfig { stage = 3, levelCap = 160, fragmentCost = 80, rareMaterialId = "rare_material", 
                rareMaterialCost = 5, specialReward = "解锁终极技能强化" },                                                    // 3阶
            new BreakthroughConfig { stage = 4, levelCap = 180, fragmentCost = 120, rareMaterialId = "rare_material", 
                rareMaterialCost = 10, legendaryMaterialId = "legendary_material", legendaryMaterialCost = 3, 
                specialReward = "外观变化" },                                                                                  // 4阶
            new BreakthroughConfig { stage = 5, levelCap = 200, fragmentCost = 200, legendaryMaterialId = "legendary_material", 
                legendaryMaterialCost = 5, divineFragmentId = "divine_fragment", divineFragmentCost = 1, 
                specialReward = "全属性+20%" }                                                                                 // 5阶 - 满突破
        };
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 突破成功事件
        /// </summary>
        public event Action<CharacterInstance, int> OnBreakthroughSuccess;
        
        /// <summary>
        /// 突破材料不足事件
        /// </summary>
        public event Action<CharacterInstance, int, List<string>> OnBreakthroughFailed;
        
        #endregion
        
        #region 构造函数
        
        private BreakthroughSystem()
        {
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取突破配置
        /// </summary>
        public BreakthroughConfig GetConfig(int stage)
        {
            stage = Mathf.Clamp(stage, 0, 5);
            return BreakthroughConfigs[stage];
        }
        
        /// <summary>
        /// 检查角色是否可以突破
        /// </summary>
        public bool CanBreakthrough(CharacterInstance instance)
        {
            // 已达最大突破
            if (instance.IsMaxBreakthrough)
                return false;
            
            // 检查是否达到当前等级上限
            if (instance.level < instance.CurrentMaxLevel)
                return false;
            
            // 检查材料是否足够
            int nextStage = instance.breakthroughCount + 1;
            var config = GetConfig(nextStage);
            return CheckMaterials(config);
        }
        
        /// <summary>
        /// 检查突破材料是否足够
        /// </summary>
        public bool CheckMaterials(BreakthroughConfig config)
        {
            // 检查角色碎片
            if (config.fragmentCost > 0)
            {
                int fragmentCount = ResourcesManager.Instance.GetItemCount($"character_fragment_{config.stage + 1}");
                if (fragmentCount < config.fragmentCost)
                    return false;
            }
            
            // 检查元素结晶
            if (!string.IsNullOrEmpty(config.elementCrystalId) && config.elementCrystalCost > 0)
            {
                int count = ResourcesManager.Instance.GetItemCount(config.elementCrystalId);
                if (count < config.elementCrystalCost)
                    return false;
            }
            
            // 检查稀有材料
            if (!string.IsNullOrEmpty(config.rareMaterialId) && config.rareMaterialCost > 0)
            {
                int count = ResourcesManager.Instance.GetItemCount(config.rareMaterialId);
                if (count < config.rareMaterialCost)
                    return false;
            }
            
            // 检查传说材料
            if (!string.IsNullOrEmpty(config.legendaryMaterialId) && config.legendaryMaterialCost > 0)
            {
                int count = ResourcesManager.Instance.GetItemCount(config.legendaryMaterialId);
                if (count < config.legendaryMaterialCost)
                    return false;
            }
            
            // 检查神器碎片
            if (!string.IsNullOrEmpty(config.divineFragmentId) && config.divineFragmentCost > 0)
            {
                int count = ResourcesManager.Instance.GetItemCount(config.divineFragmentId);
                if (count < config.divineFragmentCost)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取突破所需材料列表
        /// </summary>
        public List<MaterialRequirement> GetRequiredMaterials(int stage)
        {
            var config = GetConfig(stage);
            var materials = new List<MaterialRequirement>();
            
            if (config.fragmentCost > 0)
            {
                materials.Add(new MaterialRequirement
                {
                    itemId = $"character_fragment_{stage}",
                    itemName = $"角色碎片×{config.fragmentCost}",
                    requiredCount = config.fragmentCost,
                    currentCount = ResourcesManager.Instance.GetItemCount($"character_fragment_{stage}")
                });
            }
            
            if (!string.IsNullOrEmpty(config.elementCrystalId))
            {
                materials.Add(new MaterialRequirement
                {
                    itemId = config.elementCrystalId,
                    itemName = "元素结晶",
                    requiredCount = config.elementCrystalCost,
                    currentCount = ResourcesManager.Instance.GetItemCount(config.elementCrystalId)
                });
            }
            
            if (!string.IsNullOrEmpty(config.rareMaterialId))
            {
                materials.Add(new MaterialRequirement
                {
                    itemId = config.rareMaterialId,
                    itemName = "稀有材料",
                    requiredCount = config.rareMaterialCost,
                    currentCount = ResourcesManager.Instance.GetItemCount(config.rareMaterialId)
                });
            }
            
            if (!string.IsNullOrEmpty(config.legendaryMaterialId))
            {
                materials.Add(new MaterialRequirement
                {
                    itemId = config.legendaryMaterialId,
                    itemName = "传说材料",
                    requiredCount = config.legendaryMaterialCost,
                    currentCount = ResourcesManager.Instance.GetItemCount(config.legendaryMaterialId)
                });
            }
            
            if (!string.IsNullOrEmpty(config.divineFragmentId))
            {
                materials.Add(new MaterialRequirement
                {
                    itemId = config.divineFragmentId,
                    itemName = "神器碎片",
                    requiredCount = config.divineFragmentCost,
                    currentCount = ResourcesManager.Instance.GetItemCount(config.divineFragmentId)
                });
            }
            
            return materials;
        }
        
        /// <summary>
        /// 执行突破
        /// </summary>
        public bool ExecuteBreakthrough(CharacterInstance instance)
        {
            if (!CanBreakthrough(instance))
                return false;
            
            int nextStage = instance.breakthroughCount + 1;
            var config = GetConfig(nextStage);
            
            // 消耗材料
            ConsumeMaterials(config);
            
            // 执行突破
            instance.breakthroughCount = nextStage;
            
            // 重置经验（等级上限提升后）
            ExperienceManager.Instance.RecalculateExpAfterBreakthrough(instance);
            
            // 触发事件
            OnBreakthroughSuccess?.Invoke(instance, nextStage);
            
            Debug.Log($"角色 {instance.DisplayName} 突破到 {nextStage} 阶！");
            
            return true;
        }
        
        /// <summary>
        /// 消耗突破材料
        /// </summary>
        private void ConsumeMaterials(BreakthroughConfig config)
        {
            if (config.fragmentCost > 0)
            {
                ResourcesManager.Instance.RemoveItem($"character_fragment_{config.stage}", config.fragmentCost);
            }
            
            if (!string.IsNullOrEmpty(config.elementCrystalId) && config.elementCrystalCost > 0)
            {
                ResourcesManager.Instance.RemoveItem(config.elementCrystalId, config.elementCrystalCost);
            }
            
            if (!string.IsNullOrEmpty(config.rareMaterialId) && config.rareMaterialCost > 0)
            {
                ResourcesManager.Instance.RemoveItem(config.rareMaterialId, config.rareMaterialCost);
            }
            
            if (!string.IsNullOrEmpty(config.legendaryMaterialId) && config.legendaryMaterialCost > 0)
            {
                ResourcesManager.Instance.RemoveItem(config.legendaryMaterialId, config.legendaryMaterialCost);
            }
            
            if (!string.IsNullOrEmpty(config.divineFragmentId) && config.divineFragmentCost > 0)
            {
                ResourcesManager.Instance.RemoveItem(config.divineFragmentId, config.divineFragmentCost);
            }
        }
        
        /// <summary>
        /// 获取突破进度描述
        /// </summary>
        public string GetBreakthroughProgress(CharacterInstance instance)
        {
            if (instance.IsMaxBreakthrough)
                return "已满突破";
            
            var config = GetConfig(instance.breakthroughCount + 1);
            return $"下次突破：{config.specialReward}";
        }
        
        /// <summary>
        /// 获取突破属性加成
        /// </summary>
        public float GetBreakthroughStatBonus(int stage)
        {
            stage = Mathf.Clamp(stage, 0, 5);
            
            // 各阶段属性加成倍率
            float[] bonuses = { 1.0f, 1.1f, 1.2f, 1.3f, 1.5f, 1.8f };
            return bonuses[stage];
        }
        
        /// <summary>
        /// 计算突破后属性提升
        /// </summary>
        public CharacterStats CalculateBreakthroughIncrease(CharacterInstance instance)
        {
            var before = CharacterStatsCalculator.Instance.CalculateBaseStats(
                instance.GetCharacterData(), instance.level, instance.breakthroughCount);
            
            var after = CharacterStatsCalculator.Instance.CalculateBaseStats(
                instance.GetCharacterData(), instance.level, instance.breakthroughCount + 1);
            
            return new CharacterStats
            {
                MaxHp = after.MaxHp - before.MaxHp,
                Atk = after.Atk - before.Atk,
                Def = after.Def - before.Def,
                MaxMp = after.MaxMp - before.MaxMp
            };
        }
        
        #endregion
        
        #region 材料需求结构
        
        /// <summary>
        /// 材料需求
        /// </summary>
        public struct MaterialRequirement
        {
            public string itemId;
            public string itemName;
            public int requiredCount;
            public int currentCount;
            
            public bool IsSatisfied => currentCount >= requiredCount;
            public int shortage => Mathf.Max(0, requiredCount - currentCount);
        }
        
        #endregion
    }
}
