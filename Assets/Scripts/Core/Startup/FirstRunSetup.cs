using UnityEngine;
using System;
using System.Collections;

namespace JRPGStudio.Core.Startup
{
    /// <summary>
    /// 首次运行设置
    /// 负责检测和处理首次运行游戏的初始化任务
    /// </summary>
    public class FirstRunSetup : MonoBehaviour
    {
        // 单例
        private static FirstRunSetup instance;
        public static FirstRunSetup Instance => instance;

        // PlayerPrefs键名常量
        private const string KeyFirstRunCompleted = "FirstRunSetup_Completed";
        private const string KeyFirstRunVersion = "FirstRunSetup_Version";
        private const string KeyTutorialCompleted = "Tutorial_Completed";
        private const string KeyDefaultSettingsSet = "DefaultSettings_Set";

        [Header("首次运行设置")]
        [SerializeField] private bool simulateFirstRun = false; // 模拟首次运行（用于测试）
        [SerializeField] private string currentSetupVersion = "1.0.0";

        // 事件
        public event Action OnFirstRunSetupCompleted;

        #region 属性

        /// <summary>
        /// 是否是首次运行
        /// </summary>
        public bool IsFirstRun
        {
            get
            {
                if (simulateFirstRun) return true;
                
                #if UNITY_EDITOR
                // 编辑器中总是返回true（可配置）
                return !PlayerPrefs.HasKey(KeyFirstRunCompleted);
                #else
                return !PlayerPrefs.HasKey(KeyFirstRunCompleted);
                #endif
            }
        }

        /// <summary>
        /// 是否已完成教程
        /// </summary>
        public bool IsTutorialCompleted
        {
            get => PlayerPrefs.GetInt(KeyTutorialCompleted, 0) == 1;
            set => PlayerPrefs.SetInt(KeyTutorialCompleted, value ? 1 : 0);
        }

        /// <summary>
        /// 是否已设置默认设置
        /// </summary>
        public bool HasDefaultSettings => PlayerPrefs.GetInt(KeyDefaultSettingsSet, 0) == 1;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (IsFirstRun)
            {
                Debug.Log("FirstRunSetup: 检测到首次运行，开始初始化设置...");
                StartCoroutine(PerformFirstRunSetup());
            }
            else
            {
                Debug.Log("FirstRunSetup: 非首次运行，跳过初始化");
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 执行首次运行设置
        /// </summary>
        /// <param name="callback">设置完成回调</param>
        public void PerformSetup(Action callback = null)
        {
            if (!IsFirstRun && !simulateFirstRun)
            {
                callback?.Invoke();
                return;
            }

            StartCoroutine(PerformFirstRunSetup(callback));
        }

        /// <summary>
        /// 重置首次运行设置（用于调试）
        /// </summary>
        [ContextMenu("重置首次运行设置")]
        public void ResetFirstRunSetup()
        {
            PlayerPrefs.DeleteKey(KeyFirstRunCompleted);
            PlayerPrefs.DeleteKey(KeyFirstRunVersion);
            PlayerPrefs.DeleteKey(KeyTutorialCompleted);
            PlayerPrefs.DeleteKey(KeyDefaultSettingsSet);
            
            Debug.Log("FirstRunSetup: 已重置首次运行设置");
        }

        /// <summary>
        /// 跳过教程
        /// </summary>
        public void SkipTutorial()
        {
            IsTutorialCompleted = true;
            Debug.Log("FirstRunSetup: 教程已跳过");
        }

        /// <summary>
        /// 重新开始教程
        /// </summary>
        public void RestartTutorial()
        {
            IsTutorialCompleted = false;
            Debug.Log("FirstRunSetup: 教程已重新开始");
        }

        /// <summary>
        /// 获取设置进度
        /// </summary>
        public float GetSetupProgress()
        {
            float progress = 0f;
            
            if (HasDefaultSettings) progress += 0.25f;
            if (IsTutorialCompleted) progress += 0.25f;
            if (PlayerPrefs.HasKey(KeyFirstRunCompleted)) progress += 0.5f;
            
            return progress;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 执行首次运行设置协程
        /// </summary>
        private IEnumerator PerformFirstRunSetup(Action callback = null)
        {
            Debug.Log("===== 开始首次运行设置 =====");

            // Step 1: 初始化PlayerPrefs默认值
            yield return InitializeDefaultSettings();
            yield return new WaitForSeconds(0.2f);

            // Step 2: 设置默认游戏设置
            yield return SetupDefaultGameSettings();
            yield return new WaitForSeconds(0.2f);

            // Step 3: 创建初始存档数据
            yield return CreateInitialSaveData();
            yield return new WaitForSeconds(0.2f);

            // Step 4: 初始化教程状态
            yield return InitializeTutorialState();
            yield return new WaitForSeconds(0.2f);

            // Step 5: 预加载首屏资源
            yield return PreloadInitialAssets();
            yield return new WaitForSeconds(0.2f);

            // 标记首次运行完成
            CompleteFirstRunSetup();

            Debug.Log("===== 首次运行设置完成 =====");

            OnFirstRunSetupCompleted?.Invoke();
            callback?.Invoke();
        }

        /// <summary>
        /// 初始化默认设置
        /// </summary>
        private IEnumerator InitializeDefaultSettings()
        {
            Debug.Log("FirstRunSetup: 初始化默认设置...");

            // 音量设置
            PlayerPrefs.SetFloat("MasterVolume", 1f);
            PlayerPrefs.SetFloat("BGMVolume", 0.8f);
            PlayerPrefs.SetFloat("SFXVolume", 1f);
            PlayerPrefs.SetFloat("VoiceVolume", 1f);

            // 画面设置
            PlayerPrefs.SetInt("GraphicsQuality", 2); // 高画质
            PlayerPrefs.SetInt("VSync", 1); // 开启垂直同步
            PlayerPrefs.SetInt("FrameRate", 60);

            // 游戏设置
            PlayerPrefs.SetInt("AutoSave", 1);
            PlayerPrefs.SetInt("ShowDamageNumber", 1);
            PlayerPrefs.SetInt("ShowSkillEffect", 1);
            PlayerPrefs.SetInt("BattleSpeed", 1);

            // 语言设置（根据系统语言自动设置）
            string systemLang = Application.systemLanguage.ToString();
            string gameLang = "Chinese"; // 默认中文
            
            if (systemLang == "English")
                gameLang = "English";
            
            PlayerPrefs.SetString("GameLanguage", gameLang);

            yield return null;
        }

        /// <summary>
        /// 设置默认游戏设置
        /// </summary>
        private IEnumerator SetupDefaultGameSettings()
        {
            Debug.Log("FirstRunSetup: 设置默认游戏配置...");

            // 设置画质
            QualitySettings.SetQualityLevel(2);

            // 设置帧率
            Application.targetFrameRate = 60;

            yield return null;

            // 标记默认设置已设置
            PlayerPrefs.SetInt(KeyDefaultSettingsSet, 1);
        }

        /// <summary>
        /// 创建初始存档数据
        /// </summary>
        private IEnumerator CreateInitialSaveData()
        {
            Debug.Log("FirstRunSetup: 创建初始存档数据...");

            yield return null;

            // 创建新玩家数据
            PlayerData newPlayer = new PlayerData
            {
                playerId = System.Guid.NewGuid().ToString(),
                playerName = "冒险者",
                level = 1,
                exp = 0,
                gold = 1000,
                gems = 100,
                createTime = System.DateTime.Now.Ticks,
                lastLoginTime = System.DateTime.Now.Ticks
            };

            // 保存玩家数据
            SavePlayerData(newPlayer);

            Debug.Log($"FirstRunSetup: 创建玩家 {newPlayer.playerName}，等级 {newPlayer.level}");
        }

        /// <summary>
        /// 初始化教程状态
        /// </summary>
        private IEnumerator InitializeTutorialState()
        {
            Debug.Log("FirstRunSetup: 初始化教程状态...");

            yield return null;

            // 重置教程进度
            string[] tutorialKeys = {
                "Tutorial_Move",
                "Tutorial_Battle",
                "Tutorial_Skill",
                "Tutorial_Item",
                "Tutorial_Equip"
            };

            foreach (var key in tutorialKeys)
            {
                PlayerPrefs.SetInt(key, 0);
            }
        }

        /// <summary>
        /// 预加载首屏资源
        /// </summary>
        private IEnumerator PreloadInitialAssets()
        {
            Debug.Log("FirstRunSetup: 预加载首屏资源...");

            // 预加载主菜单需要的资源
            // 实际项目中根据需求添加

            yield return null;
        }

        /// <summary>
        /// 完成首次运行设置
        /// </summary>
        private void CompleteFirstRunSetup()
        {
            PlayerPrefs.SetInt(KeyFirstRunCompleted, 1);
            PlayerPrefs.SetString(KeyFirstRunVersion, currentSetupVersion);
            PlayerPrefs.Save();

            Debug.Log("FirstRunSetup: 首次运行设置已标记完成");
        }

        /// <summary>
        /// 保存玩家数据
        /// </summary>
        private void SavePlayerData(PlayerData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("SaveData_Player", json);
            PlayerPrefs.SetInt("SaveData_Exists", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载玩家数据
        /// </summary>
        private PlayerData LoadPlayerData()
        {
            string json = PlayerPrefs.GetString("SaveData_Player", "");
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonUtility.FromJson<PlayerData>(json);
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 玩家数据
        /// </summary>
        [System.Serializable]
        public class PlayerData
        {
            public string playerId;
            public string playerName;
            public int level;
            public long exp;
            public long gold;
            public int gems;
            public long createTime;
            public long lastLoginTime;
        }

        #endregion
    }
}
