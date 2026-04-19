#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace JRPG.Editor
{
    /// <summary>
    /// JRPG Studio 编辑器菜单项
    /// </summary>
    public static class JRPGMenuItems
    {
        private const string MENU_PATH = "JRPG Studio/";
        
        #region 创建数据
        
        [MenuItem(MENU_PATH + "创建/角色数据", false, 100)]
        public static void CreateCharacterData()
        {
            CreateScriptableObject<CharacterData>("Characters", "NewCharacterData");
        }
        
        [MenuItem(MENU_PATH + "创建/敌人数据", false, 101)]
        public static void CreateEnemyData()
        {
            CreateScriptableObject<EnemyData>("Enemies", "NewEnemyData");
        }
        
        [MenuItem(MENU_PATH + "创建/技能数据", false, 102)]
        public static void CreateSkillData()
        {
            CreateScriptableObject<SkillData>("Skills", "NewSkillData");
        }
        
        [MenuItem(MENU_PATH + "创建/装备数据", false, 103)]
        public static void CreateEquipmentData()
        {
            CreateScriptableObject<EquipmentData>("Equipment", "NewEquipmentData");
        }
        
        [MenuItem(MENU_PATH + "创建/卡池数据", false, 104)]
        public static void CreateGachaPool()
        {
            CreateScriptableObject<GachaPool>("Gacha/Pools", "NewGachaPool");
        }
        
        [MenuItem(MENU_PATH + "创建/关卡数据", false, 105)]
        public static void CreateStageData()
        {
            CreateScriptableObject<StageData>("Stages", "NewStageData");
        }
        
        #endregion
        
        #region 数据生成
        
        [MenuItem(MENU_PATH + "数据生成/生成所有测试数据", false, 200)]
        public static void GenerateAllTestData()
        {
            DataGenerator.GenerateAllTestData();
        }
        
        [MenuItem(MENU_PATH + "数据生成/生成初始角色数据", false, 201)]
        public static void GenerateInitialCharacters()
        {
            DataGenerator.GenerateInitialCharacters();
        }
        
        [MenuItem(MENU_PATH + "数据生成/生成敌人数据", false, 202)]
        public static void GenerateEnemyData()
        {
            DataGenerator.GenerateEnemies();
        }
        
        [MenuItem(MENU_PATH + "数据生成/生成技能数据库", false, 203)]
        public static void GenerateSkillDatabase()
        {
            DataGenerator.GenerateSkills();
        }
        
        [MenuItem(MENU_PATH + "数据生成/生成装备数据", false, 204)]
        public static void GenerateEquipmentData()
        {
            DataGenerator.GenerateEquipment();
        }
        
        #endregion
        
        #region 构建工具
        
        [MenuItem(MENU_PATH + "构建/快速构建APK (Debug)", false, 300)]
        public static void QuickBuildDebug()
        {
            QuickBuild.BuildDebugAPK();
        }
        
        [MenuItem(MENU_PATH + "构建/快速构建APK (Release)", false, 301)]
        public static void QuickBuildRelease()
        {
            QuickBuild.BuildReleaseAPK();
        }
        
        [MenuItem(MENU_PATH + "构建/打开构建输出目录", false, 302)]
        public static void OpenBuildOutputDirectory()
        {
            string buildPath = Path.Combine(Application.dataPath, "../Builds");
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            EditorUtility.RevealInFinder(buildPath);
        }
        
        [MenuItem(MENU_PATH + "构建/清理构建缓存", false, 303)]
        public static void CleanBuildCache()
        {
            string buildPath = Path.Combine(Application.dataPath, "../Builds");
            if (Directory.Exists(buildPath))
            {
                Directory.Delete(buildPath, true);
                Directory.CreateDirectory(buildPath);
                Debug.Log("[JRPG] 已清理构建缓存");
            }
            
            // 清理Library
            if (EditorUtility.DisplayDialog("清理缓存", 
                "是否同时清理Unity Library缓存？这会重新导入所有资源，需要较长时间。", 
                "清理Library", "仅清理Builds"))
            {
                string libraryPath = Path.Combine(Application.dataPath, "../Library");
                if (Directory.Exists(libraryPath))
                {
                    Directory.Delete(libraryPath, true);
                    Debug.Log("[JRPG] 已清理Library缓存，请重启Unity");
                }
            }
        }
        
        #endregion
        
        #region 工具
        
        [MenuItem(MENU_PATH + "工具/重置玩家存档", false, 400)]
        public static void ResetPlayerSave()
        {
            if (EditorUtility.DisplayDialog("重置存档", 
                "确定要重置所有玩家存档数据吗？此操作不可撤销。", 
                "确定", "取消"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[JRPG] 已重置所有玩家存档");
            }
        }
        
        [MenuItem(MENU_PATH + "工具/显示FPS计数器", false, 401)]
        public static void ToggleFPSCounter()
        {
            var fpsCounter = Object.FindObjectOfType<FPSCounter>();
            if (fpsCounter != null)
            {
                fpsCounter.Toggle();
            }
        }
        
        [MenuItem(MENU_PATH + "工具/显示调试控制台", false, 402)]
        public static void ToggleDebugConsole()
        {
            var debugConsole = Object.FindObjectOfType<DebugConsole>();
            if (debugConsole != null)
            {
                debugConsole.ToggleConsole();
            }
        }
        
        [MenuItem(MENU_PATH + "工具/执行GC", false, 403)]
        public static void ForceGC()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            Debug.Log("[JRPG] 已执行垃圾回收");
        }
        
        #endregion
        
        #region 关于
        
        [MenuItem(MENU_PATH + "关于/项目信息", false, 500)]
        public static void ShowProjectInfo()
        {
            EditorUtility.DisplayDialog("神界：混沌边境",
                "版本: 1.0.0\n" +
                "构建号: 1\n" +
                "构建日期: 2026-04-19\n\n" +
                "Unity版本: 2022.3 LTS\n" +
                "最低Android版本: Android 7.0 (API 24)\n" +
                "目标Android版本: Android 13 (API 33)",
                "确定");
        }
        
        [MenuItem(MENU_PATH + "关于/打开文档", false, 501)]
        public static void OpenDocumentation()
        {
            string docPath = Path.Combine(Application.dataPath, "../docs");
            if (Directory.Exists(docPath))
            {
                EditorUtility.RevealInFinder(docPath);
            }
            else
            {
                Debug.LogWarning("[JRPG] 文档目录不存在");
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 创建ScriptableObject资源
        /// </summary>
        private static void CreateScriptableObject<T>(string subPath, string defaultName) where T : ScriptableObject
        {
            // 确保目录存在
            string path = $"Assets/ScriptableObjects/{subPath}";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            // 创建资源
            T asset = ScriptableObject.CreateInstance<T>();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}.asset");
            
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            
            // 选中并高亮
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            
            Debug.Log($"[JRPG] 创建 {typeof(T).Name}: {assetPath}");
        }
        
        #endregion
    }
}
#endif
