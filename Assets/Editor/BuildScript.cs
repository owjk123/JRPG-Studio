using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace JRPGStudio.Editor
{
    /// <summary>
    /// APK构建脚本
    /// 用于命令行和编辑器中构建Android APK
    /// </summary>
    public class BuildScript
    {
        // 构建输出路径
        private static readonly string BuildPath = "Builds/Android";
        private static readonly string ApkName = "DivineChaos_EdgeOfWorlds.apk";

        // 构建场景
        private static readonly string[] BuildScenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Battle.unity"
        };

        /// <summary>
        /// 命令行构建入口
        /// </summary>
        public static void BuildAndroid()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = BuildScenes,
                locationPathName = Path.Combine(BuildPath, ApkName),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // 确保输出目录存在
            Directory.CreateDirectory(BuildPath);

            Debug.Log("===== 开始构建 Android APK =====");
            Debug.Log($"输出路径: {buildPlayerOptions.locationPathName}");
            Debug.Log($"构建场景: {string.Join(", ", BuildScenes)}");

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"===== 构建成功 =====");
                Debug.Log($"APK大小: {summary.totalSize / (1024 * 1024):F2} MB");
                Debug.Log($"构建时间: {summary.totalTime.TotalMinutes:F2} 分钟");
                Debug.Log($"输出位置: {buildPlayerOptions.locationPathName}");
                
                // 自动打开输出目录
                EditorUtility.RevealInFinder(buildPlayerOptions.locationPathName);
            }
            else
            {
                Debug.LogError($"===== 构建失败 =====");
                Debug.LogError($"错误数: {summary.totalErrors}");
                
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                        {
                            Debug.LogError($"[{step.name}] {message.content}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 开发版本构建（带调试信息）
        /// </summary>
        public static void BuildAndroidDevelopment()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = BuildScenes,
                locationPathName = Path.Combine(BuildPath, "Dev_" + ApkName),
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging
            };

            Directory.CreateDirectory(BuildPath);

            Debug.Log("===== 开始构建开发版 APK =====");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("开发版构建成功！");
                EditorUtility.RevealInFinder(buildPlayerOptions.locationPathName);
            }
        }

        /// <summary>
        /// 菜单项：构建正式版APK
        /// </summary>
        [MenuItem("JRPG Studio/Build/Build Release APK", false, 100)]
        public static void BuildReleaseMenu()
        {
            BuildAndroid();
        }

        /// <summary>
        /// 菜单项：构建开发版APK
        /// </summary>
        [MenuItem("JRPG Studio/Build/Build Development APK", false, 101)]
        public static void BuildDevelopmentMenu()
        {
            BuildAndroidDevelopment();
        }

        /// <summary>
        /// 菜单项：打开构建目录
        /// </summary>
        [MenuItem("JRPG Studio/Build/Open Build Folder", false, 200)]
        public static void OpenBuildFolder()
        {
            if (Directory.Exists(BuildPath))
            {
                EditorUtility.RevealInFinder(BuildPath);
            }
            else
            {
                Debug.LogWarning($"构建目录不存在: {BuildPath}");
            }
        }

        /// <summary>
        /// 菜单项：清理构建缓存
        /// </summary>
        [MenuItem("JRPG Studio/Build/Clean Build Cache", false, 300)]
        public static void CleanBuildCache()
        {
            if (Directory.Exists("Library/Bee"))
            {
                Directory.Delete("Library/Bee", true);
                Debug.Log("已清理构建缓存");
            }
            
            if (Directory.Exists(BuildPath))
            {
                Directory.Delete(BuildPath, true);
                Debug.Log("已清理构建输出");
            }
        }
    }
}
