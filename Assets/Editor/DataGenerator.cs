#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace JRPG.Editor
{
    /// <summary>
    /// 数据生成器 - 批量生成游戏数据
    /// </summary>
    public static class DataGenerator
    {
        private const string DATA_PATH = "Assets/ScriptableObjects";
        
        /// <summary>
        /// 生成所有测试数据
        /// </summary>
        public static void GenerateAllTestData()
        {
            Debug.Log("[DataGenerator] 开始生成所有测试数据...");
            
            GenerateInitialCharacters();
            GenerateEnemies();
            GenerateSkills();
            GenerateEquipment();
            GenerateGachaPools();
            GenerateStages();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[DataGenerator] 所有测试数据生成完成！");
        }
        
        /// <summary>
        /// 生成初始角色数据
        /// </summary>
        public static void GenerateInitialCharacters()
        {
            Debug.Log("[DataGenerator] 生成初始角色数据...");
            
            string path = $"{DATA_PATH}/Characters";
            EnsureDirectoryExists(path);
            
            // 5个初始角色
            var characterConfigs = new List<(string id, string name, Race race, CharacterClass charClass)>
            {
                ("char_human_knight_001", "光耀骑士·亚瑟", Race.Human, CharacterClass.Knight),
                ("char_vampire_mage_001", "血夜魔女·莉莉丝", Race.Vampire, CharacterClass.Mage),
                ("char_angel_archer_001", "圣光射手·米迦勒", Race.Angel, CharacterClass.Archer),
                ("char_beast_warrior_001", "狂野战士·雷恩", Race.Beastman, CharacterClass.Warrior),
                ("char_demon_assassin_001", "暗影刺客·夜刃", Race.Demon, CharacterClass.Assassin)
            };
            
            foreach (var config in characterConfigs)
            {
                CreateCharacterData(path, config.id, config.name, config.race, config.charClass);
            }
            
            Debug.Log($"[DataGenerator] 生成了 {characterConfigs.Count} 个角色数据");
        }
        
        /// <summary>
        /// 生成敌人数据
        /// </summary>
        public static void GenerateEnemies()
        {
            Debug.Log("[DataGenerator] 生成敌人数据...");
            
            string path = $"{DATA_PATH}/Enemies";
            EnsureDirectoryExists(path);
            
            // 基础敌人
            var enemyConfigs = new List<(string id, string name, int level, int hp, int atk)>
            {
                ("enemy_slime_001", "史莱姆", 1, 100, 10),
                ("enemy_goblin_001", "哥布林", 3, 200, 25),
                ("enemy_skeleton_001", "骷髅兵", 5, 350, 40),
                ("enemy_orc_001", "兽人", 8, 600, 60),
                ("enemy_dark_knight_001", "黑暗骑士", 15, 1200, 100),
                ("enemy_demon_lord_001", "魔王", 30, 5000, 200)
            };
            
            foreach (var config in enemyConfigs)
            {
                CreateEnemyData(path, config.id, config.name, config.level, config.hp, config.atk);
            }
            
            Debug.Log($"[DataGenerator] 生成了 {enemyConfigs.Count} 个敌人数据");
        }
        
        /// <summary>
        /// 生成技能数据
        /// </summary>
        public static void GenerateSkills()
        {
            Debug.Log("[DataGenerator] 生成技能数据...");
            
            string path = $"{DATA_PATH}/Skills";
            EnsureDirectoryExists(path);
            
            // 基础技能
            var skillConfigs = new List<(string id, string name, SkillType type, Element element)>
            {
                ("skill_slash_001", "斩击", SkillType.Physical, Element.None),
                ("skill_fireball_001", "火球术", SkillType.Magical, Element.Fire),
                ("skill_ice_shard_001", "冰刺", SkillType.Magical, Element.Ice),
                ("skill_thunder_001", "雷电", SkillType.Magical, Element.Thunder),
                ("skill_heal_001", "治愈", SkillType.Heal, Element.None),
                ("skill_shield_001", "护盾", SkillType.Buff, Element.None),
                ("skill_poison_001", "剧毒", SkillType.Debuff, Element.None)
            };
            
            foreach (var config in skillConfigs)
            {
                CreateSkillData(path, config.id, config.name, config.type, config.element);
            }
            
            Debug.Log($"[DataGenerator] 生成了 {skillConfigs.Count} 个技能数据");
        }
        
        /// <summary>
        /// 生成装备数据
        /// </summary>
        public static void GenerateEquipment()
        {
            Debug.Log("[DataGenerator] 生成装备数据...");
            
            string path = $"{DATA_PATH}/Equipment";
            EnsureDirectoryExists(path);
            
            // 基础装备
            var equipmentConfigs = new List<(string id, string name, EquipmentSlot slot, int rarity)>
            {
                ("equip_sword_001", "新手长剑", EquipmentSlot.Weapon, 1),
                ("equip_staff_001", "学徒法杖", EquipmentSlot.Weapon, 1),
                ("equip_bow_001", "猎人之弓", EquipmentSlot.Weapon, 1),
                ("equip_helmet_001", "铁头盔", EquipmentSlot.Helmet, 1),
                ("equip_armor_001", "铁铠甲", EquipmentSlot.Armor, 1),
                ("equip_gloves_001", "皮手套", EquipmentSlot.Gloves, 1),
                ("equip_ring_001", "力量戒指", EquipmentSlot.Accessory1, 2),
                ("equip_necklace_001", "智慧项链", EquipmentSlot.Accessory2, 2)
            };
            
            foreach (var config in equipmentConfigs)
            {
                CreateEquipmentData(path, config.id, config.name, config.slot, config.rarity);
            }
            
            Debug.Log($"[DataGenerator] 生成了 {equipmentConfigs.Count} 个装备数据");
        }
        
        /// <summary>
        /// 生成卡池数据
        /// </summary>
        public static void GenerateGachaPools()
        {
            Debug.Log("[DataGenerator] 生成卡池数据...");
            
            string path = $"{DATA_PATH}/Gacha/Pools";
            EnsureDirectoryExists(path);
            
            // 常驻池
            CreateGachaPool(path, "pool_standard_001", "常驻召唤池", GachaPoolType.Standard);
            
            // 限定池
            CreateGachaPool(path, "pool_limited_001", "限定角色池", GachaPoolType.Limited);
            
            Debug.Log("[DataGenerator] 生成了 2 个卡池数据");
        }
        
        /// <summary>
        /// 生成关卡数据
        /// </summary>
        public static void GenerateStages()
        {
            Debug.Log("[DataGenerator] 生成关卡数据...");
            
            string path = $"{DATA_PATH}/Stages";
            EnsureDirectoryExists(path);
            
            // 第1章关卡
            for (int i = 1; i <= 5; i++)
            {
                CreateStageData(path, $"stage_1_{i:D2}", $"第1章-{i}", 1, i);
            }
            
            // 第2章关卡
            for (int i = 1; i <= 5; i++)
            {
                CreateStageData(path, $"stage_2_{i:D2}", $"第2章-{i}", 2, i);
            }
            
            Debug.Log("[DataGenerator] 生成了 10 个关卡数据");
        }
        
        #region 创建方法
        
        private static void CreateCharacterData(string path, string id, string name, Race race, CharacterClass charClass)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            data.characterId = id;
            data.characterName = name;
            data.race = race;
            data.characterClass = charClass;
            data.rarity = 4; // SSR
            
            AssetDatabase.CreateAsset(data, $"{path}/{id}.asset");
        }
        
        private static void CreateEnemyData(string path, string id, string name, int level, int hp, int atk)
        {
            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyId = id;
            data.enemyName = name;
            data.level = level;
            data.baseHP = hp;
            data.baseATK = atk;
            
            AssetDatabase.CreateAsset(data, $"{path}/{id}.asset");
        }
        
        private static void CreateSkillData(string path, string id, string name, SkillType type, Element element)
        {
            var data = ScriptableObject.CreateInstance<SkillData>();
            data.skillId = id;
            data.skillName = name;
            data.skillType = type;
            data.element = element;
            
            AssetDatabase.CreateAsset(data, $"{path}/{id}.asset");
        }
        
        private static void CreateEquipmentData(string path, string id, string name, EquipmentSlot slot, int rarity)
        {
            var data = ScriptableObject.CreateInstance<EquipmentData>();
            data.equipmentId = id;
            data.equipmentName = name;
            data.slot = slot;
            data.rarity = rarity;
            
            AssetDatabase.CreateAsset(data, $"{path}/{id}.asset");
        }
        
        private static void CreateGachaPool(string path, string id, string name, GachaPoolType type)
        {
            var data = ScriptableObject.CreateInstance<GachaPool>();
            data.poolId = id;
            data.poolName = name;
            data.poolType = type;
            
            AssetDatabase.CreateAsset(data, $"{path}/{id}.asset");
        }
        
        private static void CreateStageData(string path, string id, string name, int chapter, int stageNum)
        {
            var data = ScriptableObject.CreateInstance<StageData>();
            data.stageId = id;
            data.stageName = name;
            data.chapter = chapter;
            data.stageNumber = stageNum;
            
            AssetDatabase.CreateAsset(data, $"{path}/{id}.asset");
        }
        
        #endregion
        
        #region 辅助方法
        
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        #endregion
    }
    
    // 辅助枚举（如果未定义）
    public enum GachaPoolType { Standard, Limited, Race, Weapon }
    public enum SkillType { Physical, Magical, Heal, Buff, Debuff }
}
#endif
