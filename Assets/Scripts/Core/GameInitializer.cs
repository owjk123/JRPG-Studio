using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using JRPGStudio.Audio;
using JRPGStudio.Data;

namespace JRPGStudio.Core
{
    /// <summary>
    /// 游戏初始化器
    /// 负责游戏启动时的初始化流程
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("初始化设置")]
        [SerializeField] private float minLoadingTime = 2f;
        [SerializeField] private string mainMenuScene = "MainMenu";

        [Header("资源引用")]
        [SerializeField] private CharacterDataList characterData;
        [SerializeField] private SkillDatabase skillDatabase;

        // 初始化进度
        public static float Progress { get; private set; }
        public static string CurrentStep { get; private set; }

        private void Start()
        {
            StartCoroutine(InitializeGame());
        }

        private IEnumerator InitializeGame()
        {
            Debug.Log("===== 游戏初始化开始 =====");

            // Step 1: 初始化核心系统
            CurrentStep = "初始化核心系统...";
            Progress = 0.1f;
            yield return InitializeCoreSystems();
            yield return null;

            // Step 2: 加载游戏数据
            CurrentStep = "加载游戏数据...";
            Progress = 0.3f;
            yield return LoadGameData();
            yield return null;

            // Step 3: 初始化音频系统
            CurrentStep = "初始化音频系统...";
            Progress = 0.5f;
            yield return InitializeAudioSystem();
            yield return null;

            // Step 4: 加载玩家存档
            CurrentStep = "加载存档数据...";
            Progress = 0.7f;
            yield return LoadPlayerData();
            yield return null;

            // Step 5: 预加载资源
            CurrentStep = "预加载资源...";
            Progress = 0.85f;
            yield return PreloadAssets();
            yield return null;

            // 确保最小加载时间
            CurrentStep = "准备进入游戏...";
            Progress = 0.95f;
            float elapsed = Time.timeSinceLevelLoad;
            if (elapsed < minLoadingTime)
            {
                yield return new WaitForSeconds(minLoadingTime - elapsed);
            }

            // 完成
            Progress = 1f;
            CurrentStep = "完成";
            Debug.Log("===== 游戏初始化完成 =====");

            // 加载主菜单场景
            SceneManager.LoadScene(mainMenuScene);
        }

        /// <summary>
        /// 初始化核心系统
        /// </summary>
        private IEnumerator InitializeCoreSystems()
        {
            // 初始化GameManager
            if (GameManager.Instance == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }

            yield return new WaitForSeconds(0.1f);

            Debug.Log("核心系统初始化完成");
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        private IEnumerator LoadGameData()
        {
            // 加载角色数据
            if (characterData == null)
            {
                characterData = Resources.Load<CharacterDataList>("Data/Characters/InitialCharacters");
            }

            // 加载技能数据
            if (skillDatabase == null)
            {
                skillDatabase = Resources.Load<SkillDatabase>("Data/Skills/SkillDatabase");
            }

            yield return new WaitForSeconds(0.1f);

            Debug.Log($"已加载 {characterData?.characters?.Count ?? 0} 个角色数据");
            Debug.Log($"已加载 {skillDatabase?.skills?.Count ?? 0} 个技能数据");
        }

        /// <summary>
        /// 初始化音频系统
        /// </summary>
        private IEnumerator InitializeAudioSystem()
        {
            if (AudioManager.Instance == null)
            {
                var go = new GameObject("AudioManager");
                go.AddComponent<AudioManager>();
            }

            AudioManager.Instance.Initialize();

            yield return new WaitForSeconds(0.1f);

            Debug.Log("音频系统初始化完成");
        }

        /// <summary>
        /// 加载玩家存档
        /// </summary>
        private IEnumerator LoadPlayerData()
        {
            // 检查是否有存档
            if (PlayerPrefs.HasKey("PlayerID"))
            {
                // 加载存档逻辑
                int playerId = PlayerPrefs.GetInt("PlayerID");
                Debug.Log($"加载玩家存档: {playerId}");
            }
            else
            {
                // 创建新玩家
                Debug.Log("创建新玩家数据");
                PlayerPrefs.SetInt("PlayerID", System.DateTime.Now.GetHashCode());
                PlayerPrefs.Save();
            }

            yield return new WaitForSeconds(0.1f);
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        private IEnumerator PreloadAssets()
        {
            // 预加载常用UI预制体
            // 预加载常用特效
            // 预加载常用音效

            yield return new WaitForSeconds(0.1f);

            Debug.Log("资源预加载完成");
        }
    }
}
