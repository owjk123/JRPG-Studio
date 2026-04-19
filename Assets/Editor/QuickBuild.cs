using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace JRPGStudio.Editor
{
    /// <summary>
    /// 快速打包工具 - 一键生成APK
    /// </summary>
    public class QuickBuildWindow : EditorWindow
    {
        [MenuItem("JRPG Studio/Quick Build APK", false, 1)]
        public static void QuickBuild()
        {
            string buildPath = "Builds/Android";
            string apkName = "DivineChaos_v0.1.apk";

            // 确保目录存在
            Directory.CreateDirectory(buildPath);

            // 获取所有场景
            string[] scenes = new string[]
            {
                "Assets/Scenes/Loading.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Battle.unity"
            };

            // 构建选项
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(buildPath, apkName),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("🚀 开始构建APK...");
            Debug.Log($"📦 输出路径: {options.locationPathName}");

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                double sizeMB = summary.totalSize / (1024.0 * 1024.0);
                Debug.Log($"✅ 构建成功!");
                Debug.Log($"📊 APK大小: {sizeMB:F2} MB");
                Debug.Log($"⏱️ 构建时间: {summary.totalTime.TotalSeconds:F1} 秒");

                EditorUtility.RevealInFinder(options.locationPathName);
            }
            else
            {
                Debug.LogError($"❌ 构建失败! 错误数: {summary.totalErrors}");
            }
        }

        [MenuItem("JRPG Studio/Open Project Folder", false, 100)]
        public static void OpenProjectFolder()
        {
            EditorUtility.RevealInFinder(Application.dataPath);
        }

        [MenuItem("JRPG Studio/Open Build Folder", false, 101)]
        public static void OpenBuildFolder()
        {
            string buildPath = Path.Combine(Application.dataPath, "..", "Builds", "Android");
            if (Directory.Exists(buildPath))
            {
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogWarning("构建目录不存在，请先构建APK");
            }
        }
    }
}
