// Toast.cs - 轻提示组件
// 用于显示短暂的操作反馈信息

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Core;

namespace JRPG.UI.Common
{
    /// <summary>
    /// Toast类型
    /// </summary>
    public enum ToastType
    {
        Normal,     // 普通
        Success,    // 成功
        Warning,    // 警告
        Error,      // 错误
        Info        // 信息
    }

    /// <summary>
    /// Toast消息配置
    /// </summary>
    [Serializable]
    public class ToastData
    {
        public string message;
        public ToastType type;
        public float duration;
        public bool autoClose;
        public Sprite icon;

        public ToastData(string message, ToastType type = ToastType.Normal, float duration = 2f)
        {
            this.message = message;
            this.type = type;
            this.duration = duration;
            this.autoClose = true;
        }
    }

    /// <summary>
    /// Toast位置
    /// </summary>
    public enum ToastPosition
    {
        Top,
        Center,
        Bottom
    }

    /// <summary>
    /// 轻提示组件
    /// </summary>
    public class Toast : MonoBehaviour
    {
        #region Singleton

        private static Toast _instance;
        public static Toast Instance
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
            GameObject go = new GameObject("ToastManager");
            _instance = go.AddComponent<Toast>();
            DontDestroyOnLoad(go);
        }

        #endregion

        #region Fields

        [Header("预制体")]
        [SerializeField] private GameObject _toastItemPrefab;

        [Header("容器")]
        [SerializeField] private Transform _topContainer;
        [SerializeField] private Transform _centerContainer;
        [SerializeField] private Transform _bottomContainer;

        [Header("配置")]
        [SerializeField] private float _defaultDuration = 2f;
        [SerializeField] private float _itemSpacing = 10f;
        [SerializeField] private int _maxVisibleToasts = 5;
        [SerializeField] private float _animDuration = 0.3f;

        [Header("样式")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _successColor = Color.green;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _errorColor = Color.red;
        [SerializeField] private Color _infoColor = Color.blue;

        [Header("图标")]
        [SerializeField] private Sprite _successIcon;
        [SerializeField] private Sprite _warningIcon;
        [SerializeField] private Sprite _errorIcon;
        [SerializeField] private Sprite _infoIcon;

        // Toast队列
        private Queue<ToastItem> _toastQueue = new Queue<ToastItem>();
        private List<ToastItem> _visibleToasts = new List<ToastItem>();
        private bool _isProcessingQueue = false;

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
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 确保容器存在
            EnsureContainersExist();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示Toast
        /// </summary>
        public void Show(string message, ToastType type = ToastType.Normal, float duration = -1f, ToastPosition position = ToastPosition.Bottom)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[Toast] 消息不能为空");
                return;
            }

            ToastData data = new ToastData
            {
                message = message,
                type = type,
                duration = duration < 0 ? _defaultDuration : duration
            };

            Show(data, position);
        }

        /// <summary>
        /// 显示Toast
        /// </summary>
        public void Show(ToastData data, ToastPosition position = ToastPosition.Bottom)
        {
            if (_visibleToasts.Count >= _maxVisibleToasts)
            {
                // 加入队列等待
                _toastQueue.Enqueue(new ToastItem { data = data, position = position });
                return;
            }

            CreateToast(data, position);
        }

        /// <summary>
        /// 显示成功提示
        /// </summary>
        public void ShowSuccess(string message, float duration = -1f, ToastPosition position = ToastPosition.Bottom)
        {
            Show(message, ToastType.Success, duration, position);
        }

        /// <summary>
        /// 显示警告提示
        /// </summary>
        public void ShowWarning(string message, float duration = -1f, ToastPosition position = ToastPosition.Bottom)
        {
            Show(message, ToastType.Warning, duration, position);
        }

        /// <summary>
        /// 显示错误提示
        /// </summary>
        public void ShowError(string message, float duration = -1f, ToastPosition position = ToastPosition.Bottom)
        {
            Show(message, ToastType.Error, duration, position);
        }

        /// <summary>
        /// 显示信息提示
        /// </summary>
        public void ShowInfo(string message, float duration = -1f, ToastPosition position = ToastPosition.Bottom)
        {
            Show(message, ToastType.Info, duration, position);
        }

        /// <summary>
        /// 隐藏所有Toast
        /// </summary>
        public void HideAll()
        {
            foreach (var toast in _visibleToasts)
            {
                if (toast != null && toast.gameObject != null)
                {
                    Destroy(toast.gameObject);
                }
            }
            _visibleToasts.Clear();
        }

        #endregion

        #region Private Methods

        private void EnsureContainersExist()
        {
            if (_centerContainer == null)
            {
                CreateContainer("ToastCenter", ToastPosition.Center);
            }
            if (_bottomContainer == null)
            {
                CreateContainer("ToastBottom", ToastPosition.Bottom);
            }
            if (_topContainer == null)
            {
                CreateContainer("ToastTop", ToastPosition.Top);
            }
        }

        private void CreateContainer(string name, ToastPosition position)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(transform);

            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            switch (position)
            {
                case ToastPosition.Top:
                    rect.anchoredPosition = new Vector2(0, 100);
                    break;
                case ToastPosition.Bottom:
                    rect.anchoredPosition = new Vector2(0, -100);
                    break;
            }

            switch (position)
            {
                case ToastPosition.Top:
                    _topContainer = container.transform;
                    break;
                case ToastPosition.Center:
                    _centerContainer = container.transform;
                    break;
                case ToastPosition.Bottom:
                    _bottomContainer = container.transform;
                    break;
            }
        }

        private void CreateToast(ToastData data, ToastPosition position)
        {
            // 如果没有预制体，动态创建
            GameObject toastObj;
            if (_toastItemPrefab != null)
            {
                Transform container = GetContainer(position);
                toastObj = Instantiate(_toastItemPrefab, container);
            }
            else
            {
                toastObj = CreateDefaultToast(data, position);
            }

            ToastItem item = toastObj.GetComponent<ToastItem>();
            if (item == null)
            {
                item = toastObj.AddComponent<ToastItem>();
            }

            item.Initialize(data, _animDuration, () =>
            {
                OnToastClosed(item);
            });

            // 设置样式
            SetToastStyle(item, data.type);

            _visibleToasts.Add(item);

            // 播放进入动画
            PlayEnterAnimation(item);
        }

        private GameObject CreateDefaultToast(ToastData data, ToastPosition position)
        {
            GameObject toastObj = new GameObject("ToastItem");
            toastObj.transform.SetParent(GetContainer(position));

            RectTransform rect = toastObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 60);
            rect.anchoredPosition = Vector2.zero;

            // 添加背景
            Image bg = toastObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            // 添加文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(toastObj.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = data.message;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return toastObj;
        }

        private Transform GetContainer(ToastPosition position)
        {
            switch (position)
            {
                case ToastPosition.Top:
                    return _topContainer ?? transform;
                case ToastPosition.Center:
                    return _centerContainer ?? transform;
                case ToastPosition.Bottom:
                default:
                    return _bottomContainer ?? transform;
            }
        }

        private void SetToastStyle(ToastItem item, ToastType type)
        {
            Color bgColor = Color.black;
            Color textColor = Color.white;
            Sprite icon = null;

            switch (type)
            {
                case ToastType.Success:
                    bgColor = _successColor;
                    icon = _successIcon;
                    break;
                case ToastType.Warning:
                    bgColor = _warningColor;
                    icon = _warningIcon;
                    break;
                case ToastType.Error:
                    bgColor = _errorColor;
                    icon = _errorIcon;
                    break;
                case ToastType.Info:
                    bgColor = _infoColor;
                    icon = _infoIcon;
                    break;
                default:
                    bgColor = _normalColor;
                    break;
            }

            // 应用样式
            Image bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0.9f);
            }
        }

        private void PlayEnterAnimation(ToastItem item)
        {
            if (item == null) return;

            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null)
            {
                UITweener.TweenScale(rect, Vector3.zero, Vector3.one, _animDuration, AnimationCurve.EaseInOut(0, 0, 1, 1));
            }
        }

        private void OnToastClosed(ToastItem item)
        {
            _visibleToasts.Remove(item);

            if (_toastQueue.Count > 0)
            {
                var nextToast = _toastQueue.Dequeue();
                CreateToast(nextToast.data, nextToast.position);
            }
        }

        #endregion

        #region Inner Class

        private class ToastItem : MonoBehaviour
        {
            private ToastData _data;
            private float _animDuration;
            private Action _onClose;
            private bool _isClosing = false;

            public GameObject gameObject => base.gameObject;

            public void Initialize(ToastData data, float animDuration, Action onClose)
            {
                _data = data;
                _animDuration = animDuration;
                _onClose = onClose;

                // 设置自动关闭
                if (data.autoClose && data.duration > 0)
                {
                    Invoke(nameof(Close), data.duration);
                }
            }

            public void Close()
            {
                if (_isClosing) return;
                _isClosing = true;

                RectTransform rect = GetComponent<RectTransform>();
                if (rect != null)
                {
                    UITweener.TweenScale(rect, Vector3.one, Vector3.zero, _animDuration, null, () =>
                    {
                        _onClose?.Invoke();
                        Destroy(gameObject);
                    });
                }
                else
                {
                    _onClose?.Invoke();
                    Destroy(gameObject);
                }
            }
        }

        #endregion
    }
}
