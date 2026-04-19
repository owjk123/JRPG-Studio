// LoadingScreen.cs - 加载界面组件
// 用于场景切换或长时间操作时的加载提示

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using JRPG.UI.Core;

namespace JRPG.UI.Common
{
    /// <summary>
    /// 加载类型
    /// </summary>
    public enum LoadingType
    {
        Normal,         // 普通加载
        SceneLoad,      // 场景加载
        DataLoad,       // 数据加载
        Download        // 下载进度
    }

    /// <summary>
    /// 加载界面组件
    /// </summary>
    public class LoadingScreen : BasePanel
    {
        #region Singleton

        private static LoadingScreen _instance;
        public static LoadingScreen Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }
                return _instance;
            }
        }

        private static void CreateInstance()
        {
            GameObject go = new GameObject("LoadingScreen");
            _instance = go.AddComponent<LoadingScreen>();
            DontDestroyOnLoad(go);
        }

        #endregion

        #region UI References

        [Header("背景")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RawImage _backgroundRawImage;

        [Header("加载条")]
        [SerializeField] private Image _progressBarFill;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _percentageText;

        [Header("加载动画")]
        [SerializeField] private Image _loadingIcon;
        [SerializeField] private TextMeshProUGUI _loadingTipText;
        [SerializeField] private Transform _loadingDotContainer;

        [Header("文字提示")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        #endregion

        #region Fields

        [Header("配置")]
        [SerializeField] private LoadingType _loadingType = LoadingType.Normal;
        [SerializeField] private bool _showPercentage = true;
        [SerializeField] private bool _showProgressBar = true;
        [SerializeField] private bool _showLoadingAnimation = true;
        [SerializeField] private float _minDisplayTime = 0.5f; // 最小显示时间，防止闪烁

        private float _currentProgress = 0f;
        private float _targetProgress = 0f;
        private bool _isLoading = false;
        private float _loadingStartTime;
        private Action _onCompleteCallback;
        private Action<float> _onProgressCallback;

        // 加载提示轮换
        private string[] _loadingTips = new string[]
        {
            "正在努力加载中...",
            "正在准备游戏内容...",
            "正在加载资源文件...",
            "请稍候片刻...",
            "正在进入游戏世界..."
        };
        private int _currentTipIndex = 0;
        private float _tipChangeInterval = 2f;
        private float _lastTipChangeTime = 0f;

        // 进度条动画
        private const float ProgressAnimSpeed = 5f;

        #endregion

        #region Properties

        /// <summary>
        /// 当前进度 (0-1)
        /// </summary>
        public float Progress => _currentProgress;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading => _isLoading;

        #endregion

        #region BasePanel Override

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            _layer = PanelLayer.Top;
            _useOpenAnimation = false;
            _useCloseAnimation = false;
        }

        protected override void Start()
        {
            base.Start();
            // 初始状态为隐藏
            gameObject.SetActive(false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示加载界面
        /// </summary>
        public void ShowLoading(Action onComplete = null)
        {
            ShowLoading(LoadingType.Normal, null, onComplete);
        }

        /// <summary>
        /// 显示加载界面
        /// </summary>
        public void ShowLoading(LoadingType type, Action<float> onProgress = null, Action onComplete = null)
        {
            _loadingType = type;
            _onProgressCallback = onProgress;
            _onCompleteCallback = onComplete;
            _targetProgress = 0f;
            _currentProgress = 0f;
            _isLoading = true;
            _loadingStartTime = Time.time;

            // 更新UI
            UpdateUI();
            UpdateLoadingTip();

            // 显示面板
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _isVisible = true;

            // 开始加载动画
            StartLoadingAnimation();
        }

        /// <summary>
        /// 设置加载进度
        /// </summary>
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
            _onProgressCallback?.Invoke(_targetProgress);
        }

        /// <summary>
        /// 增加加载进度
        /// </summary>
        public void AddProgress(float delta)
        {
            SetProgress(_targetProgress + delta);
        }

        /// <summary>
        /// 设置加载提示文字
        /// </summary>
        public void SetTip(string tip)
        {
            if (_loadingTipText != null)
            {
                _loadingTipText.text = tip;
            }
        }

        /// <summary>
        /// 设置标题
        /// </summary>
        public void SetTitle(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
        }

        /// <summary>
        /// 设置副标题
        /// </summary>
        public void SetSubtitle(string subtitle)
        {
            if (_subtitleText != null)
            {
                _subtitleText.text = subtitle;
            }
        }

        /// <summary>
        /// 隐藏加载界面
        /// </summary>
        public void HideLoading()
        {
            if (!_isLoading) return;

            // 检查最小显示时间
            float elapsed = Time.time - _loadingStartTime;
            if (elapsed < _minDisplayTime)
            {
                StartCoroutine(WaitAndHide(_minDisplayTime - elapsed));
                return;
            }

            CompleteLoading();
        }

        /// <summary>
        /// 强制立即隐藏
        /// </summary>
        public void ForceHide()
        {
            StopAllCoroutines();
            CompleteLoading();
        }

        #endregion

        #region Private Methods

        private void Update()
        {
            if (!_isLoading) return;

            // 平滑更新进度
            _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * ProgressAnimSpeed);

            // 更新进度显示
            UpdateProgressDisplay();

            // 轮换加载提示
            UpdateLoadingTip();

            // 更新加载动画
            UpdateLoadingAnimation();
        }

        private void UpdateUI()
        {
            // 设置进度条可见性
            SetActive(_progressBarFill?.gameObject, _showProgressBar);

            // 设置百分比可见性
            SetActive(_percentageText?.gameObject, _showPercentage);

            // 设置加载动画可见性
            SetActive(_loadingIcon?.gameObject, _showLoadingAnimation);
            SetActive(_loadingDotContainer?.gameObject, _showLoadingAnimation);
        }

        private void UpdateProgressDisplay()
        {
            // 更新进度条
            if (_progressBarFill != null)
            {
                _progressBarFill.fillAmount = _currentProgress;
            }

            // 更新百分比文字
            if (_percentageText != null)
            {
                _percentageText.text = Mathf.RoundToInt(_currentProgress * 100) + "%";
            }

            // 更新进度文字
            if (_progressText != null)
            {
                _progressText.text = $"加载中... {Mathf.RoundToInt(_currentProgress * 100)}%";
            }
        }

        private void UpdateLoadingTip()
        {
            if (_loadingTipText == null) return;

            if (Time.time - _lastTipChangeTime > _tipChangeInterval)
            {
                _currentTipIndex = (_currentTipIndex + 1) % _loadingTips.Length;
                _loadingTipText.text = _loadingTips[_currentTipIndex];
                _lastTipChangeTime = Time.time;
            }
        }

        private void UpdateLoadingAnimation()
        {
            // 旋转加载图标
            if (_loadingIcon != null)
            {
                _loadingIcon.transform.Rotate(0f, 0f, -360f * Time.deltaTime);
            }

            // 动画加载点
            if (_loadingDotContainer != null)
            {
                UpdateLoadingDots();
            }
        }

        private void UpdateLoadingDots()
        {
            int childCount = _loadingDotContainer.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = _loadingDotContainer.GetChild(i);
                float targetAlpha = (Mathf.Sin(Time.time * 5f + i * 0.5f) + 1f) * 0.5f;
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = child.gameObject.AddComponent<CanvasGroup>();
                }
                cg.alpha = targetAlpha;
            }
        }

        private void StartLoadingAnimation()
        {
            // 重置状态
            _currentProgress = 0f;
            _targetProgress = 0f;
        }

        private void CompleteLoading()
        {
            _isLoading = false;

            // 确保进度为100%
            _currentProgress = 1f;
            _targetProgress = 1f;
            UpdateProgressDisplay();

            // 延迟关闭，给用户一个完成的视觉反馈
            StartCoroutine(DelayClose());
        }

        private System.Collections.IEnumerator WaitAndHide(float delay)
        {
            yield return new WaitForSeconds(delay);
            CompleteLoading();
        }

        private System.Collections.IEnumerator DelayClose()
        {
            yield return new WaitForSeconds(0.3f);

            // 隐藏面板
            _canvasGroup.alpha = 0f;
            _isVisible = false;
            gameObject.SetActive(false);

            // 回调
            _onCompleteCallback?.Invoke();
            _onCompleteCallback = null;
            _onProgressCallback = null;
        }

        #endregion
    }
}
