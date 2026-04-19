// BasePanel.cs - 面板基类
// 所有UI面板的基类，提供统一的打开/关闭动画和生命周期管理

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using JRPG.UI.Core;

namespace JRPG.UI
{
    /// <summary>
    /// 面板层级枚举
    /// </summary>
    public enum PanelLayer
    {
        Background = 0,    // 背景层
        Normal = 1,        // 普通层
        Popup = 2,         // 弹窗层
        Toast = 3,         // 提示层
        Top = 4            // 顶层
    }

    /// <summary>
    /// 面板基类 - 所有UI面板的基类
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        #region Fields

        [Header("面板配置")]
        [SerializeField] protected PanelLayer _layer = PanelLayer.Normal;
        [SerializeField] protected bool _autoSortByLayer = true;
        [SerializeField] protected bool _cacheOnClose = false;

        [Header("动画配置")]
        [SerializeField] protected bool _useOpenAnimation = true;
        [SerializeField] protected bool _useCloseAnimation = true;
        [SerializeField] protected float _openAnimDuration = 0.3f;
        [SerializeField] protected float _closeAnimDuration = 0.2f;
        [SerializeField] protected AnimationCurve _openAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected AnimationCurve _closeAnimCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        // 面板状态
        protected bool _isOpening = false;
        protected bool _isClosing = false;
        protected bool _isVisible = false;

        // Canvas Group用于透明度动画
        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;

        // 动画回调
        private Action _onOpenComplete;
        private Action _onCloseComplete;

        #endregion

        #region Properties

        /// <summary>
        /// 面板是否正在显示
        /// </summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// 面板层级
        /// </summary>
        public PanelLayer Layer => _layer;

        /// <summary>
        /// 面板名称
        /// </summary>
        public string PanelName => gameObject.name;

        #endregion

        #region Events

        /// <summary>
        /// 面板显示时触发
        /// </summary>
        public event Action<BasePanel> OnShow;

        /// <summary>
        /// 面板隐藏时触发
        /// </summary>
        public event Action<BasePanel> OnHide;

        /// <summary>
        /// 面板打开动画完成时触发
        /// </summary>
        public event Action<BasePanel> OnOpenComplete;

        /// <summary>
        /// 面板关闭动画完成时触发
        /// </summary>
        public event Action<BasePanel> OnCloseComplete;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            // 如果没有CanvasGroup，自动添加
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 初始状态为隐藏
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        protected virtual void Start()
        {
            // 子类初始化
            OnPanelInit();
        }

        protected virtual void OnDestroy()
        {
            // 清理事件
            OnShow = null;
            OnHide = null;
            OnOpenComplete = null;
            OnCloseComplete = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="callback">打开完成回调</param>
        public virtual void Show(Action callback = null)
        {
            if (_isVisible || _isOpening)
            {
                callback?.Invoke();
                return;
            }

            _onOpenComplete = callback;
            _isOpening = true;
            _isVisible = true;
            gameObject.SetActive(true);

            // 设置Raycast
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            // 触发显示事件
            OnShow?.Invoke(this);
            OnPanelShow();

            if (_useOpenAnimation)
            {
                PlayOpenAnimation();
            }
            else
            {
                _canvasGroup.alpha = 1f;
                CompleteOpen();
            }
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <param name="callback">关闭完成回调</param>
        public virtual void Hide(Action callback = null)
        {
            if (!_isVisible || _isClosing)
            {
                callback?.Invoke();
                return;
            }

            _onCloseComplete = callback;
            _isClosing = true;

            // 取消Raycast
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // 触发隐藏事件
            OnHide?.Invoke(this);
            OnPanelHide();

            if (_useCloseAnimation)
            {
                PlayCloseAnimation();
            }
            else
            {
                CompleteClose();
            }
        }

        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public virtual void Toggle()
        {
            if (_isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// 刷新面板数据
        /// </summary>
        public virtual void Refresh()
        {
            if (!_isVisible) return;
            OnPanelRefresh();
        }

        /// <summary>
        /// 设置面板数据
        /// </summary>
        /// <param name="data">数据对象</param>
        public virtual void SetData(object data)
        {
            OnPanelDataSet(data);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 面板初始化（Start时调用）
        /// </summary>
        protected virtual void OnPanelInit() { }

        /// <summary>
        /// 面板显示时调用（动画开始前）
        /// </summary>
        protected virtual void OnPanelShow() { }

        /// <summary>
        /// 面板隐藏时调用（动画开始前）
        /// </summary>
        protected virtual void OnPanelHide() { }

        /// <summary>
        /// 面板刷新（每次Refresh时调用）
        /// </summary>
        protected virtual void OnPanelRefresh() { }

        /// <summary>
        /// 设置面板数据（每次SetData时调用）
        /// </summary>
        protected virtual void OnPanelDataSet(object data) { }

        #endregion

        #region Animation Methods

        /// <summary>
        /// 播放打开动画
        /// </summary>
        protected virtual void PlayOpenAnimation()
        {
            // 默认使用透明度+缩放动画
            UITweener.TweenAlpha(_canvasGroup, 0f, 1f, _openAnimDuration, _openAnimCurve, () =>
            {
                CompleteOpen();
            });

            // 可扩展：同时播放缩放动画
            Vector3 targetScale = Vector3.one;
            Vector3 startScale = Vector3.one * 0.8f;
            _rectTransform.localScale = startScale;

            UITweener.TweenScale(_rectTransform, startScale, targetScale, _openAnimDuration, _openAnimCurve);
        }

        /// <summary>
        /// 播放关闭动画
        /// </summary>
        protected virtual void PlayCloseAnimation()
        {
            UITweener.TweenAlpha(_canvasGroup, 1f, 0f, _closeAnimDuration, _closeAnimCurve, () =>
            {
                CompleteClose();
            });

            Vector3 startScale = Vector3.one;
            Vector3 targetScale = Vector3.one * 0.8f;

            UITweener.TweenScale(_rectTransform, startScale, targetScale, _closeAnimDuration, _closeAnimCurve);
        }

        /// <summary>
        /// 完成打开动画
        /// </summary>
        protected virtual void CompleteOpen()
        {
            _isOpening = false;
            _canvasGroup.alpha = 1f;
            OnOpenComplete?.Invoke(this);
            _onOpenComplete?.Invoke();
            _onOpenComplete = null;
        }

        /// <summary>
        /// 完成关闭动画
        /// </summary>
        protected virtual void CompleteClose()
        {
            _isClosing = false;
            _isVisible = false;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);

            OnCloseComplete?.Invoke(this);
            _onCloseComplete?.Invoke();
            _onCloseComplete = null;
        }

        #endregion

        #region UI Helper Methods

        /// <summary>
        /// 查找子物体
        /// </summary>
        protected T FindChild<T>(string path) where T : Component
        {
            Transform child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }

        /// <summary>
        /// 添加按钮点击事件
        /// </summary>
        protected void AddButtonClick(Button button, Action action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => action?.Invoke());
            }
        }

        /// <summary>
        /// 设置UI元素可见性
        /// </summary>
        protected void SetActive(GameObject obj, bool active)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }

        /// <summary>
        /// 设置UI元素可见性
        /// </summary>
        protected void SetActive(Component comp, bool active)
        {
            if (comp != null)
            {
                comp.gameObject.SetActive(active);
            }
        }

        #endregion
    }
}
