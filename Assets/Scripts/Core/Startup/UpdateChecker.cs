using UnityEngine;
using System;
using System.Collections;
using System.Text;
using JRPGStudio.Core.Resources;

namespace JRPGStudio.Core.Startup
{
    /// <summary>
    /// 更新检查器
    /// 负责检查游戏版本更新和资源更新
    /// </summary>
    public class UpdateChecker : MonoBehaviour
    {
        // 单例
        private static UpdateChecker instance;
        public static UpdateChecker Instance => instance;

        [Header("更新配置")]
        [SerializeField] private string updateCheckUrl = "https://your-server.com/version.json";
        [SerializeField] private float checkInterval = 3600f; // 检查间隔（秒）
        [SerializeField] private bool checkOnStart = true;
        [SerializeField] private bool autoDownloadUpdates = false;

        // 运行时数据
        private bool isChecking = false;
        private VersionInfo remoteVersion;
        private VersionInfo localVersion;
        private UpdateStatus currentStatus = UpdateStatus.Idle;

        // 事件
        public event Action<VersionInfo> OnUpdateAvailable;
        public event Action<UpdateStatus, string> OnStatusChanged;
        public event Action<float> OnDownloadProgress;

        #region 属性

        /// <summary>
        /// 获取当前更新状态
        /// </summary>
        public UpdateStatus CurrentUpdateStatus => currentStatus;

        /// <summary>
        /// 获取远程版本信息
        /// </summary>
        public VersionInfo RemoteVersion => remoteVersion;

        /// <summary>
        /// 是否有可用更新
        /// </summary>
        public bool HasUpdate => remoteVersion != null && localVersion != null && 
                                   remoteVersion.versionCode > localVersion.versionCode;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            localVersion = GetLocalVersion();
            
            if (checkOnStart)
            {
                CheckForUpdates();
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="showNoUpdateDialog">无更新时是否显示提示</param>
        public void CheckForUpdates(bool showNoUpdateDialog = false)
        {
            if (isChecking)
            {
                Debug.LogWarning("UpdateChecker: 正在检查更新中...");
                return;
            }

            StartCoroutine(CheckForUpdatesCoroutine(showNoUpdateDialog));
        }

        /// <summary>
        /// 下载并安装更新
        /// </summary>
        public void DownloadAndInstallUpdate()
        {
            if (!HasUpdate)
            {
                Debug.LogWarning("UpdateChecker: 没有可用更新");
                return;
            }

            if (remoteVersion == null)
            {
                Debug.LogError("UpdateChecker: 远程版本信息为空");
                return;
            }

            StartCoroutine(DownloadUpdateCoroutine());
        }

        /// <summary>
        /// 获取更新信息文本
        /// </summary>
        public string GetUpdateInfoText()
        {
            if (remoteVersion == null || localVersion == null)
            {
                return "无法获取更新信息";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"当前版本: {localVersion.versionName}");
            sb.AppendLine($"最新版本: {remoteVersion.versionName}");
            sb.AppendLine();

            if (HasUpdate)
            {
                sb.AppendLine("发现新版本！");
                sb.AppendLine();
                sb.AppendLine("更新内容:");
                
                if (remoteVersion.updateContent != null)
                {
                    foreach (var content in remoteVersion.updateContent)
                    {
                        sb.AppendLine($"• {content}");
                    }
                }

                if (remoteVersion.forceUpdate)
                {
                    sb.AppendLine();
                    sb.AppendLine("⚠️ 此更新为强制更新，必须安装后才能继续游戏");
                }
                else if (remoteVersion.recommendedUpdate)
                {
                    sb.AppendLine();
                    sb.AppendLine("💡 建议更新以获得最佳游戏体验");
                }
            }
            else
            {
                sb.AppendLine("当前已是最新版本");
            }

            return sb.ToString();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取本地版本信息
        /// </summary>
        private VersionInfo GetLocalVersion()
        {
            return new VersionInfo
            {
                versionName = Application.version,
                versionCode = PlayerPrefs.GetInt("VersionCode", 1),
                buildNumber = PlayerPrefs.GetInt("BuildNumber", 0),
                forceUpdate = false,
                recommendedUpdate = false,
                updateContent = null,
                downloadUrl = null,
                minimumVersionCode = 1
            };
        }

        /// <summary>
        /// 检查更新协程
        /// </summary>
        private IEnumerator CheckForUpdatesCoroutine(bool showNoUpdateDialog)
        {
            isChecking = true;
            UpdateStatusInfo(UpdateStatus.Checking, "正在检查更新...");

            Debug.Log($"UpdateChecker: 开始检查更新 - {updateCheckUrl}");

            // 模拟网络请求（实际使用时替换为真实请求）
            #if UNITY_EDITOR
            yield return SimulateNetworkRequest();
            #else
            using (UnityEngine.WWW www = new UnityEngine.WWW(updateCheckUrl))
            {
                while (!www.isDone)
                {
                    yield return null;
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError($"UpdateChecker: 网络请求失败 - {www.error}");
                    UpdateStatusInfo(UpdateStatus.Failed, "检查更新失败");
                    isChecking = false;
                    yield break;
                }

                ProcessVersionResponse(www.text);
            }
            #endif

            // 判断更新状态
            if (remoteVersion != null)
            {
                if (HasUpdate)
                {
                    Debug.Log($"UpdateChecker: 发现新版本 {remoteVersion.versionName}");
                    UpdateStatusInfo(UpdateStatus.UpdateAvailable, $"发现新版本: {remoteVersion.versionName}");
                    OnUpdateAvailable?.Invoke(remoteVersion);

                    // 如果是强制更新
                    if (remoteVersion.forceUpdate)
                    {
                        ShowForceUpdateDialog();
                    }
                }
                else
                {
                    Debug.Log("UpdateChecker: 当前已是最新版本");
                    UpdateStatusInfo(UpdateStatus.UpToDate, "已是最新版本");
                    
                    if (showNoUpdateDialog)
                    {
                        ShowNoUpdateDialog();
                    }
                }
            }
            else
            {
                UpdateStatusInfo(UpdateStatus.Failed, "无法获取版本信息");
            }

            isChecking = false;
        }

        /// <summary>
        /// 下载更新协程
        /// </summary>
        private IEnumerator DownloadUpdateCoroutine()
        {
            if (remoteVersion == null || string.IsNullOrEmpty(remoteVersion.downloadUrl))
            {
                Debug.LogError("UpdateChecker: 下载链接为空");
                yield break;
            }

            UpdateStatusInfo(UpdateStatus.Downloading, "正在下载更新...");

            using (UnityEngine.WWW www = new UnityEngine.WWW(remoteVersion.downloadUrl))
            {
                while (!www.isDone)
                {
                    OnDownloadProgress?.Invoke(www.progress);
                    yield return null;
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogError($"UpdateChecker: 下载失败 - {www.error}");
                    UpdateStatusInfo(UpdateStatus.Failed, "下载更新失败");
                    yield break;
                }

                // 保存下载的文件
                string savePath = Application.persistentDataPath + "/update.apk";
                System.IO.File.WriteAllBytes(savePath, www.bytes);

                UpdateStatusInfo(UpdateStatus.ReadyToInstall, "更新已下载");
                
                // 安装更新
                InstallUpdate(savePath);
            }
        }

        /// <summary>
        /// 安装更新
        /// </summary>
        private void InstallUpdate(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($"UpdateChecker: 更新文件不存在 - {filePath}");
                return;
            }

            Debug.Log($"UpdateChecker: 开始安装更新 - {filePath}");

            #if UNITY_ANDROID
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent",
                    intentClass.GetStatic<string>("ACTION_VIEW"));
                    
                AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
                AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse",
                    "file://" + filePath);
                    
                intentObject.Call<AndroidJavaObject>("setDataAndType", uriObject, "application/vnd.android.package-archive");
                intentObject.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK
                currentActivity.Call("startActivity", intentObject);
            }
            #else
            Debug.LogWarning("UpdateChecker: 当前平台不支持自动安装");
            #endif
        }

        /// <summary>
        /// 处理版本响应
        /// </summary>
        private void ProcessVersionResponse(string json)
        {
            try
            {
                // 简单的JSON解析（实际使用时建议使用JsonUtility或Newtonsoft）
                remoteVersion = JsonUtility.FromJson<VersionInfo>(json);
                
                if (remoteVersion != null)
                {
                    Debug.Log($"UpdateChecker: 解析版本信息成功 - {remoteVersion.versionName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"UpdateChecker: 解析版本信息失败 - {e.Message}");
                remoteVersion = null;
            }
        }

        /// <summary>
        /// 模拟网络请求（用于编辑器测试）
        /// </summary>
        private IEnumerator SimulateNetworkRequest()
        {
            yield return new WaitForSeconds(1f);
            
            // 模拟版本数据
            remoteVersion = new VersionInfo
            {
                versionName = "1.0.1",
                versionCode = 2,
                buildNumber = 1,
                forceUpdate = false,
                recommendedUpdate = true,
                updateContent = new string[]
                {
                    "修复了已知问题",
                    "优化了游戏性能",
                    "新增了角色立绘"
                },
                downloadUrl = "https://example.com/game-update.apk",
                minimumVersionCode = 1
            };
        }

        /// <summary>
        /// 显示强制更新对话框
        /// </summary>
        private void ShowForceUpdateDialog()
        {
            // 在实际项目中，这里应该显示一个UI对话框
            Debug.LogWarning("UpdateChecker: 强制更新对话框");
            
            // 示例：使用Unity的对话框
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog(
                "发现新版本",
                $"发现新版本 {remoteVersion.versionName}，请更新后继续游戏。",
                "立即更新"
            );
            #endif
        }

        /// <summary>
        /// 显示无更新对话框
        /// </summary>
        private void ShowNoUpdateDialog()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog(
                "版本检查",
                "当前已是最新版本！",
                "确定"
            );
            #endif
        }

        /// <summary>
        /// 更新状态信息
        /// </summary>
        private void UpdateStatusInfo(UpdateStatus status, string message)
        {
            currentStatus = status;
            OnStatusChanged?.Invoke(status, message);
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 更新状态
        /// </summary>
        public enum UpdateStatus
        {
            Idle,
            Checking,
            UpdateAvailable,
            Downloading,
            ReadyToInstall,
            UpToDate,
            Failed
        }

        /// <summary>
        /// 版本信息
        /// </summary>
        [Serializable]
        public class VersionInfo
        {
            public string versionName;      // 版本名称（如 "1.0.0"）
            public int versionCode;          // 版本代码
            public int buildNumber;         // 构建号
            public bool forceUpdate;        // 是否强制更新
            public bool recommendedUpdate;  // 是否建议更新
            public string[] updateContent;   // 更新内容
            public string downloadUrl;       // 下载链接
            public int minimumVersionCode;   // 最低兼容版本代码
        }

        #endregion
    }
}
