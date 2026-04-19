using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using JRPGStudio.Core.Resources;

namespace JRPGStudio.Core.Startup
{
    /// <summary>
    /// 游戏启动入口
    /// 负责检查资源完整性、初始化各系统、显示加载进度
    /// </summary>
    public class GameStartup : MonoBehaviour
    {
        // 单例
        private static GameStartup instance;
        public static GameStartup Instance => instance;

        [Header("场景设置")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private float minLoadingTime = 2f;

        [Header("UI引用")]
        [SerializeField] private Image progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text versionText;

        [Header("初始化配置")]
        [SerializeField] private bool checkResourceIntegrity = true;
        [SerializeField] private bool preloadAssets = true;
        [SerializeField] private string[] preloadFolders;

        // 进度信息
        public static float LoadingProgress { get; private set; }
        public static string CurrentStatus { get; private set; }

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
            StartCoroutine(StartupSequence());
        }

        /// <summary>
        /// 启动序列
        /// </summary>
        private IEnumerator StartupSequence()
        {
            LoadingProgress = 0f;
            CurrentStatus = "游戏启动中...";
            UpdateUI();

            // 显示版本信息
            #if UNITY_EDITOR
            if (versionText != null)
                versionText.text = $"版本: 1.0.0 (开发版)";
            #else
            if (versionText != null)
                versionText.text = $"版本: {Application.version}";
            #endif

            // Step 1: 系统检查
            yield return PerformSystemCheck();
            yield return new WaitForSeconds(0.3f);

            // Step 2: 初始化日志系统
            yield return InitializeLogging();
            yield return new WaitForSeconds(0.2f);

            // Step 3: 初始化资源配置
            yield return InitializeResourceManager();
            yield return new WaitForSeconds(0.2f);

            // Step 4: 检查资源完整性
            if (checkResourceIntegrity)
            {
                yield return CheckResourceIntegrity();
                yield return new WaitForSeconds(0.2f);
            }

            // Step 5: 初始化游戏数据
            yield return InitializeGameData();
            yield return new WaitForSeconds(0.2f);

            // Step 6: 初始化音频系统
            yield return InitializeAudioSystem();
            yield return new WaitForSeconds(0.2f);

            // Step 7: 初始化存档系统
            yield return InitializeSaveSystem();
            yield return new WaitForSeconds(0.2f);

            // Step 8: 预加载资源
            if (preloadAssets)
            {
                yield return PreloadGameAssets();
                yield return new WaitForSeconds(0.2f);
            }

            // Step 9: 显示主菜单
            LoadingProgress = 0.95f;
            CurrentStatus = "准备进入游戏...";
            UpdateUI();

            yield return new WaitForSeconds(0.5f);

            // 确保最小加载时间
            float elapsed = Time.timeSinceLevelLoad;
            if (elapsed < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsed);
            }

            // 完成
            LoadingProgress = 1f;
            CurrentStatus = "完成";
            UpdateUI();

            Debug.Log("===== 游戏启动完成 =====");

            // 加载主菜单
            SceneManager.LoadScene(mainMenuScene);
        }

        #region 初始化步骤

        /// <summary>
        /// 系统检查
        /// </summary>
        private IEnumerator PerformSystemCheck()
        {
            LoadingProgress = 0.05f;
            CurrentStatus = "执行系统检查...";
            UpdateUI();

            yield return null;

            // 检查设备性能
            bool isLowEndDevice = SystemInfo.systemMemorySize < 2048 || SystemInfo.graphicsMemorySize < 512;
            if (isLowEndDevice)
            {
                Debug.Log("GameStartup: 检测到低性能设备，应用性能优化");
                QualitySettings.SetQualityLevel(1);
            }

            // 检查存储空间
            long availableSpace = GetAvailableDiskSpace();
            if (availableSpace < 500 * 1024 * 1024) // 500MB
            {
                Debug.LogWarning("GameStartup: 存储空间不足");
                // 可以在此处提示用户清理空间
            }

            Debug.Log($"GameStartup: 系统检查完成 - 内存: {SystemInfo.systemMemorySize}MB, 显存: {SystemInfo.graphicsMemorySize}MB");
        }

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        private IEnumerator InitializeLogging()
        {
            LoadingProgress = 0.15f;
            CurrentStatus = "初始化日志系统...";
            UpdateUI();

            yield return null;

            Debug.Log("===== 游戏日志系统初始化 =====");
            Debug.Log($"设备: {SystemInfo.deviceModel}");
            Debug.Log($"系统内存: {SystemInfo.systemMemorySize}MB");
            Debug.Log($"Graphics: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"Unity版本: {Application.unityVersion}");
        }

        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        private IEnumerator InitializeResourceManager()
        {
            LoadingProgress = 0.25f;
            CurrentStatus = "初始化资源管理器...";
            UpdateUI();

            yield return null;

            // 初始化ResourceLoader
            ResourceLoader loader = ResourceLoader.Instance;
            if (loader != null)
            {
                Debug.Log("GameStartup: ResourceLoader初始化成功");
            }

            // 初始化AssetBundleManager
            AssetBundleManager abManager = AssetBundleManager.Instance;
            if (abManager != null)
            {
                abManager.Initialize((success) =>
                {
                    Debug.Log($"GameStartup: AssetBundleManager初始化{(success ? "成功" : "失败")}");
                });
            }
        }

        /// <summary>
        /// 检查资源完整性
        /// </summary>
        private IEnumerator CheckResourceIntegrity()
        {
            LoadingProgress = 0.4f;
            CurrentStatus = "检查资源完整性...";
            UpdateUI();

            yield return null;

            // 检查关键资源是否存在
            string[] criticalResources = {
                "Data/Characters/CharacterDataList",
                "Data/Skills/SkillDatabase",
                "Data/Enemies/EnemyData"
            };

            int missingCount = 0;
            foreach (var resourcePath in criticalResources)
            {
                var resource = Resources.Load(resourcePath);
                if (resource == null)
                {
                    Debug.LogWarning($"GameStartup: 关键资源缺失 - {resourcePath}");
                    missingCount++;
                }
            }

            if (missingCount > 0)
            {
                Debug.LogWarning($"GameStartup: 发现 {missingCount} 个缺失的关键资源");
            }
            else
            {
                Debug.Log("GameStartup: 所有关键资源检查通过");
            }
        }

        /// <summary>
        /// 初始化游戏数据
        /// </summary>
        private IEnumerator InitializeGameData()
        {
            LoadingProgress = 0.5f;
            CurrentStatus = "加载游戏数据...";
            UpdateUI();

            yield return null;

            // 加载角色数据
            var characterData = Resources.Load("Data/Characters/CharacterDataList");
            if (characterData != null)
            {
                Debug.Log("GameStartup: 角色数据加载成功");
            }
            else
            {
                Debug.LogWarning("GameStartup: 角色数据加载失败");
            }

            // 加载技能数据库
            var skillDatabase = Resources.Load("Data/Skills/SkillDatabase");
            if (skillDatabase != null)
            {
                Debug.Log("GameStartup: 技能数据库加载成功");
            }
            else
            {
                Debug.LogWarning("GameStartup: 技能数据库加载失败");
            }
        }

        /// <summary>
        /// 初始化音频系统
        /// </summary>
        private IEnumerator InitializeAudioSystem()
        {
            LoadingProgress = 0.65f;
            CurrentStatus = "初始化音频系统...";
            UpdateUI();

            yield return null;

            // 设置音频音量
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.pause = false;

            Debug.Log("GameStartup: 音频系统初始化完成");
        }

        /// <summary>
        /// 初始化存档系统
        /// </summary>
        private IEnumerator InitializeSaveSystem()
        {
            LoadingProgress = 0.75f;
            CurrentStatus = "检查存档数据...";
            UpdateUI();

            yield return null;

            // 检查是否存在存档
            bool hasSaveData = PlayerPrefs.HasKey("SaveData_Exists");
            if (hasSaveData)
            {
                Debug.Log("GameStartup: 检测到存档数据");
            }
            else
            {
                Debug.Log("GameStartup: 未检测到存档数据，将创建新存档");
            }
        }

        /// <summary>
        /// 预加载游戏资源
        /// </summary>
        private IEnumerator PreloadGameAssets()
        {
            LoadingProgress = 0.85f;
            CurrentStatus = "预加载游戏资源...";
            UpdateUI();

            yield return null;

            // 预加载UI纹理
            ResourceLoader loader = ResourceLoader.Instance;
            if (loader != null)
            {
                loader.PreloadFolder<Texture2D>("UI", (progress) =>
                {
                    LoadingProgress = 0.85f + progress * 0.1f;
                    UpdateUI();
                }, () =>
                {
                    Debug.Log("GameStartup: UI资源预加载完成");
                });
            }
        }

        #endregion

        #region UI更新

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUI()
        {
            if (progressBar != null)
            {
                progressBar.fillAmount = LoadingProgress;
            }

            if (progressText != null)
            {
                progressText.text = $"{LoadingProgress * 100:F0}%";
            }

            if (statusText != null)
            {
                statusText.text = CurrentStatus;
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取可用磁盘空间
        /// </summary>
        private long GetAvailableDiskSpace()
        {
            try
            {
                string path = Application.persistentDataPath;
                System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(path));
                return driveInfo.AvailableFreeSpace;
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取启动进度
        /// </summary>
        public static float GetProgress()
        {
            return LoadingProgress;
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public static string GetStatus()
        {
            return CurrentStatus;
        }

        #endregion
    }
}
