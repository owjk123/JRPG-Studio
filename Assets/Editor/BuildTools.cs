#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace JRPG.Editor
{
    /// <summary>
    /// 构建工具增强 - 版本管理和构建前检查
    /// </summary>
    public static class BuildTools
    {
        private const string VERSION_FILE = "Assets/StreamingAssets/version.txt";
        private const string BUILD_OUTPUT = "Builds";
        
        /// <summary>
        /// 构建前检查
        /// </summary>
        public static bool PreBuildCheck()
        {
            bool allPassed = true;
            
            // 1. 检查场景配置
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogError("[BuildTools] 错误: 没有配置任何构建场景！");
                allPassed = false;
            }
            
            // 2. 检查Bundle标识符
            if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
            {
                Debug.LogError("[BuildTools] 错误: 未设置Bundle Identifier！");
                allPassed = false;
            }
            
            // 3. 检查Keystore（Release构建）
            if (EditorUserBuildSettings.buildAppBundle == false)
            {
                // 检查是否设置了Keystore
                if (string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) && 
                    !EditorUserBuildSettings.development)
                {
                    Debug.LogWarning("[BuildTools] 警告: Release构建未设置Keystore！");
                }
            }
            
            // 4. 检查版本号
            Debug.Log($"[BuildTools] 当前版本: {PlayerSettings.bundleVersion} (Build {PlayerSettings.Android.bundleVersionCode})");
            
            return allPassed;
        }
        
        /// <summary>
        /// 自动递增版本号
        /// </summary>
        public static void IncrementBuildNumber()
        {
            PlayerSettings.Android.bundleVersionCode++;
            Debug.Log($"[BuildTools] 构建号递增至: {PlayerSettings.Android.bundleVersionCode}");
        }
        
        /// <summary>
        /// 更新版本文件
        /// </summary>
        public static void UpdateVersionFile()
        {
            string content = $"神界：混沌边境 v{PlayerSettings.bundleVersion}\n" +
                           $"构建号: {PlayerSettings.Android.bundleVersionCode}\n" +
                           $"构建日期: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                           $"Unity版本: {Application.unityVersion}\n" +
                           $"最低Android版本: Android 7.0 (API 24)\n" +
                           $"目标Android版本: Android 13 (API 33)\n";
            
            // 确保目录存在
            string directory = Path.GetDirectoryName(VERSION_FILE);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(VERSION_FILE, content);
            Debug.Log("[BuildTools] 版本文件已更新");
        }
        
        /// <summary>
        /// 执行APK构建
        /// </summary>
        public static BuildResult BuildAPK(bool development)
        {
            // 前置检查
            if (!PreBuildCheck())
            {
                Debug.LogError("[BuildTools] 构建前检查未通过，请修复上述错误");
                return null;
            }
            
            // 更新版本
            UpdateVersionFile();
            IncrementBuildNumber();
            
            // 设置构建选项
            string buildPath = GetBuildPath(development);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = development ? BuildOptions.Development : BuildOptions.None
            };
            
            Debug.Log($"[BuildTools] 开始构建: {buildPath}");
            Debug.Log($"[BuildTools] 构建模式: {(development ? "Debug" : "Release")}");
            
            // 执行构建
            BuildReport report = BuildPipeline.BuildPlayer(options);
            
            // 输出结果
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildTools] 构建成功！\n" +
                         $"输出路径: {buildPath}\n" +
                         $"构建大小: {report.summary.totalSize / (1024 * 1024):F2} MB\n" +
                         $"构建时间: {report.summary.totalTime}");
                
                // 打开输出目录
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"[BuildTools] 构建失败: {report.summary.result}");
                
                // 输出错误信息
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                        {
                            Debug.LogError($"  - {message.content}");
                        }
                    }
                }
            }
            
            return report.summary;
        }
        
        /// <summary>
        /// 获取构建路径
        /// </summary>
        private static string GetBuildPath(bool development)
        {
            string fileName = development ? 
                $"DivineChaos_v{PlayerSettings.bundleVersion}_debug.apk" :
                $"DivineChaos_v{PlayerSettings.bundleVersion}_release.apk";
            
            return Path.Combine(BUILD_OUTPUT, fileName);
        }
        
        /// <summary>
        /// 获取启用的场景路径
        /// </summary>
        private static string[] GetEnabledScenePaths()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }
    }
}
#endif
