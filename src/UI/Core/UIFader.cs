// UIFader.cs - 场景切换淡入淡出
// 支持场景切换时的全屏淡入淡出效果

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using JRPG.UI.Common;

namespace JRPG.UI.Core
{
    /// <summary>
    /// 场景切换淡入淡出控制器
    /// </summary>
    public class UIFader : MonoBehaviour
    {
        #region Singleton

        private static UIFader _instance;
        public static UIFader Instance
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
            GameObject go = new GameObject("UIFader");
            _instance = go.AddComponent<UIFader>();
            _instance.Initialize();
            DontDestroyOnLoad(go);
        }

        #endregion

        #region Fields

        [Header("淡入淡出配置")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Color _fadeColor = Color.black;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("组件引用")]
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        // 状态
        private bool _isFading = false;
        private Action _onFadeInComplete;
        private Action _onFadeOutComplete;

        #endregion

        #region Properties

        /// <summary>
        /// 是否正在淡入淡出
        /// </summary>
        public bool IsFading => _isFading;

        /// <summary>
        /// 当前透明度
        /// </summary>
        public float Alpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // 创建Canvas
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("FadeCanvas");
                canvasObj.transform.SetParent(transform);
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 9999;
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 创建淡出图像
            if (_fadeImage == null)
            {
                GameObject imageObj = new GameObject("FadeImage");
                imageObj.transform.SetParent(_canvas.transform);
                _fadeImage = imageObj.AddComponent<Image>();
                _fadeImage.color = _fadeColor;
                _fadeImage.raycastTarget = false;

                // 设置全屏
                RectTransform rect = imageObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }

            // 创建CanvasGroup
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 初始状态：完全透明
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 淡入（从黑屏到透明）
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            if (_isFading)
            {
                Debug.LogWarning("[UIFader] 正在淡入淡出中，无法开始新的淡入");
                return;
            }

            _onFadeInComplete = onComplete;
            StartCoroutine(FadeCoroutine(1f, 0f));
        }

        /// <summary>
        /// 淡出（从透明到黑屏）
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            if (_isFading)
            {
                Debug.LogWarning("[UIFader] 正在淡入淡出中，无法开始新的淡出");
                return;
            }

            _onFadeOutComplete = onComplete;
            StartCoroutine(FadeCoroutine(0f, 1f));
        }

        /// <summary>
        /// 淡入淡出（先淡出再淡入）
        /// </summary>
        public void FadeInOut(Action onComplete = null)
        {
            if (_isFading)
            {
                Debug.LogWarning("[UIFader] 正在淡入淡出中，无法开始新的淡入淡出");
                return;
            }

            _onFadeInComplete = onComplete;
            StartCoroutine(FadeInOutCoroutine());
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Clamp01(alpha);
            }
        }

        /// <summary>
        /// 立即设置透明度（无动画）
        /// </summary>
        public void SetAlphaImmediate(float alpha)
        {
            StopAllCoroutines();
            _isFading = false;
            SetAlpha(alpha);

            // 如果完全黑屏，启用射线检测以阻塞点击
            _canvasGroup.blocksRaycasts = alpha >= 0.99f;
        }

        /// <summary>
        /// 显示黑屏
        /// </summary>
        public void ShowBlack()
        {
            SetAlphaImmediate(1f);
        }

        /// <summary>
        /// 隐藏黑屏
        /// </summary>
        public void HideBlack()
        {
            SetAlphaImmediate(0f);
        }

        #endregion

        #region Coroutines

        private IEnumerator FadeCoroutine(float from, float to)
        {
            _isFading = true;
            _canvasGroup.blocksRaycasts = to > 0.5f; // 完全黑屏时阻塞射线
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = _fadeCurve.Evaluate(Mathf.Clamp01(elapsed / _fadeDuration));
                _canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _canvasGroup.alpha = to;
            _isFading = false;
            _canvasGroup.blocksRaycasts = false;

            if (to > 0.5f)
            {
                // 淡出完成（黑屏状态）
                _onFadeOutComplete?.Invoke();
                _onFadeOutComplete = null;
            }
            else
            {
                // 淡入完成（透明状态）
                _onFadeInComplete?.Invoke();
                _onFadeInComplete = null;
            }
        }

        private IEnumerator FadeInOutCoroutine()
        {
            // 先淡出到黑
            _canvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = _fadeCurve.Evaluate(Mathf.Clamp01(elapsed / _fadeDuration));
                _canvasGroup.alpha = t;
                yield return null;
            }

            _canvasGroup.alpha = 1f;

            // 等待外部场景切换完成
            yield return new WaitForSeconds(0.1f);

            // 再淡入
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = _fadeCurve.Evaluate(Mathf.Clamp01(elapsed / _fadeDuration));
                _canvasGroup.alpha = 1f - t;
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _isFading = false;

            _onFadeInComplete?.Invoke();
            _onFadeInComplete = null;
        }

        #endregion
    }
}
