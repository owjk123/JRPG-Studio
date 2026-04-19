using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace JRPGStudio.Core
{
    /// <summary>
    /// 场景加载管理器
    /// 提供场景切换、加载动画等功能
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("场景名称")]
        public const string LoadingScene = "Loading";
        public const string MainMenuScene = "MainMenu";
        public const string BattleScene = "Battle";

        [Header("加载设置")]
        [SerializeField] private float minLoadingDisplayTime = 0.5f;

        // 当前加载状态
        private bool isLoading = false;
        private string targetScene;

        // 加载完成回调
        private System.Action onSceneLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(string sceneName, System.Action callback = null)
        {
            if (isLoading)
            {
                Debug.LogWarning("已有场景正在加载中");
                return;
            }

            targetScene = sceneName;
            onSceneLoaded = callback;
            StartCoroutine(LoadSceneAsync());
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        private IEnumerator LoadSceneAsync()
        {
            isLoading = true;

            // 触发加载开始事件
            GameManager.Instance?.ChangeState(GameState.Loading);

            // 开始异步加载
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            asyncLoad.allowSceneActivation = false;

            float startTime = Time.time;

            // 等待加载完成
            while (!asyncLoad.isDone)
            {
                // 更新进度 (0-0.9是加载阶段，0.9-1是激活阶段)
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // 更新UI显示进度
                UpdateLoadingProgress(progress);

                // 加载完成
                if (asyncLoad.progress >= 0.9f)
                {
                    // 确保最小显示时间
                    float elapsed = Time.time - startTime;
                    if (elapsed < minLoadingDisplayTime)
                    {
                        yield return new WaitForSeconds(minLoadingDisplayTime - elapsed);
                    }

                    // 激活场景
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // 加载完成
            isLoading = false;
            onSceneLoaded?.Invoke();
            onSceneLoaded = null;

            Debug.Log($"场景加载完成: {targetScene}");
        }

        /// <summary>
        /// 更新加载进度显示
        /// </summary>
        private void UpdateLoadingProgress(float progress)
        {
            GameInitializer.Progress = progress;
            GameInitializer.CurrentStep = $"加载中... {Mathf.RoundToInt(progress * 100)}%";
        }

        /// <summary>
        /// 重载当前场景
        /// </summary>
        public void ReloadCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            LoadScene(MainMenuScene);
        }

        /// <summary>
        /// 进入战斗场景
        /// </summary>
        public void EnterBattle()
        {
            LoadScene(BattleScene);
        }
    }
}
