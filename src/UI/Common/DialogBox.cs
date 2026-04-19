// DialogBox.cs - 对话框组件
// 提供确认对话框、提示对话框等功能

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Core;

namespace JRPG.UI.Common
{
    /// <summary>
    /// 对话框按钮类型
    /// </summary>
    public enum DialogButtonType
    {
        None = 0,          // 无按钮
        Confirm = 1,       // 确认
        Cancel = 2,        // 取消
        ConfirmCancel = 3, // 确认+取消
        YesNo = 4,         // 是/否
        Custom = 5         // 自定义
    }

    /// <summary>
    /// 对话框样式
    /// </summary>
    public enum DialogStyle
    {
        Normal,        // 普通
        Warning,       // 警告
        Error,         // 错误
        Success,       // 成功
        Info           // 信息
    }

    /// <summary>
    /// 对话框配置
    /// </summary>
    [Serializable]
    public class DialogConfig
    {
        public string title = "提示";
        public string message = "";
        public DialogButtonType buttonType = DialogButtonType.Confirm;
        public DialogStyle style = DialogStyle.Normal;
        public string confirmText = "确定";
        public string cancelText = "取消";
        public bool closeOnConfirm = true;
        public bool closeOnCancel = true;
        public bool showCloseButton = true;
        public float autoCloseTime = 0f; // 0表示不自动关闭
    }

    /// <summary>
    /// 对话框回调
    /// </summary>
    public delegate void DialogCallback(bool confirmed, object data);

    /// <summary>
    /// 对话框组件
    /// </summary>
    public class DialogBox : BasePanel
    {
        #region UI References

        [Header("标题")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _titleIcon;

        [Header("内容")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private ScrollRect _messageScrollRect;

        [Header("按钮区域")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _confirmText;
        [TextMeshProUGUI(_confirmText)]
        [SerializeField] private TextMeshProUGUI _cancelText;
        [SerializeField] private Transform _buttonContainer;

        [Header("样式图标")]
        [SerializeField] private Sprite _warningIcon;
        [SerializeField] private Sprite _errorIcon;
        [SerializeField] private Sprite _successIcon;
        [SerializeField] private Sprite _infoIcon;

        [Header("关闭按钮")]
        [SerializeField] private Button _closeButton;

        #endregion

        #region Fields

        private DialogCallback _onConfirm;
        private DialogCallback _onCancel;
        private DialogCallback _onClose;
        private object _callbackData;
        private float _autoCloseTimer;
        private bool _isAutoClosing;

        // 按钮文本样式
        private const string ConfirmButtonText = "confirmBtn_Text";
        private const string CancelButtonText = "cancelBtn_Text";

        #endregion

        #region Properties

        public bool IsAutoClosing => _isAutoClosing;

        #endregion

        #region BasePanel Override

        protected override void Awake()
        {
            base.Awake();
            _layer = PanelLayer.Popup;
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        protected override void OnPanelInit()
        {
            // 设置默认配置
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示对话框
        /// </summary>
        public void ShowDialog(DialogConfig config, DialogCallback onConfirm = null, DialogCallback onCancel = null, DialogCallback onClose = null)
        {
            // 设置内容
            SetTitle(config.title);
            SetMessage(config.message);
            SetStyle(config.style);
            SetButtonType(config.buttonType);
            SetButtonText(config.confirmText, config.cancelText);

            // 设置回调
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _onClose = onClose;

            // 设置配置
            _config = config;

            // 显示面板
            Show(() =>
            {
                _onConfirm?.Invoke(true, _callbackData);
            });
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        public void ShowConfirm(string title, string message, DialogCallback onConfirm = null, DialogCallback onCancel = null)
        {
            DialogConfig config = new DialogConfig
            {
                title = title,
                message = message,
                buttonType = DialogButtonType.ConfirmCancel
            };
            ShowDialog(config, onConfirm, onCancel);
        }

        /// <summary>
        /// 显示提示对话框
        /// </summary>
        public void ShowAlert(string title, string message, DialogCallback onClose = null)
        {
            DialogConfig config = new DialogConfig
            {
                title = title,
                message = message,
                buttonType = DialogButtonType.Confirm,
                closeOnConfirm = true
            };
            ShowDialog(config, onClose);
        }

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        public void ShowError(string title, string message, DialogCallback onClose = null)
        {
            DialogConfig config = new DialogConfig
            {
                title = title,
                message = message,
                buttonType = DialogButtonType.Confirm,
                style = DialogStyle.Error,
                closeOnConfirm = true
            };
            ShowDialog(config, onClose);
        }

        /// <summary>
        /// 显示成功对话框
        /// </summary>
        public void ShowSuccess(string title, string message, DialogCallback onClose = null)
        {
            DialogConfig config = new DialogConfig
            {
                title = title,
                message = message,
                buttonType = DialogButtonType.Confirm,
                style = DialogStyle.Success,
                closeOnConfirm = true
            };
            ShowDialog(config, onClose);
        }

        /// <summary>
        /// 设置回调数据
        /// </summary>
        public void SetCallbackData(object data)
        {
            _callbackData = data;
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        public void CloseDialog()
        {
            Hide(() =>
            {
                _onClose?.Invoke(false, _callbackData);
            });
        }

        #endregion

        #region Private Methods

        private DialogConfig _config;

        private void SetupButtons()
        {
            // 确认按钮
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(OnConfirmClick);
            }

            // 取消按钮
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(OnCancelClick);
            }

            // 关闭按钮
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(OnCloseClick);
            }
        }

        private void SetTitle(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
            }
        }

        private void SetStyle(DialogStyle style)
        {
            if (_titleIcon != null)
            {
                switch (style)
                {
                    case DialogStyle.Warning:
                        _titleIcon.sprite = _warningIcon;
                        _titleIcon.gameObject.SetActive(true);
                        break;
                    case DialogStyle.Error:
                        _titleIcon.sprite = _errorIcon;
                        _titleIcon.gameObject.SetActive(true);
                        break;
                    case DialogStyle.Success:
                        _titleIcon.sprite = _successIcon;
                        _titleIcon.gameObject.SetActive(true);
                        break;
                    case DialogStyle.Info:
                        _titleIcon.sprite = _infoIcon;
                        _titleIcon.gameObject.SetActive(true);
                        break;
                    default:
                        _titleIcon.gameObject.SetActive(false);
                        break;
                }
            }
        }

        private void SetButtonType(DialogButtonType buttonType)
        {
            bool showConfirm = buttonType == DialogButtonType.Confirm ||
                              buttonType == DialogButtonType.ConfirmCancel ||
                              buttonType == DialogButtonType.YesNo;

            bool showCancel = buttonType == DialogButtonType.Cancel ||
                              buttonType == DialogButtonType.ConfirmCancel ||
                              buttonType == DialogButtonType.YesNo;

            SetActive(_confirmButton?.gameObject, showConfirm);
            SetActive(_cancelButton?.gameObject, showCancel);
        }

        private void SetButtonText(string confirmText, string cancelText)
        {
            if (_confirmText != null)
            {
                _confirmText.text = confirmText;
            }

            if (_cancelText != null)
            {
                _cancelText.text = cancelText;
            }
        }

        #endregion

        #region Button Events

        private void OnConfirmClick()
        {
            if (_config != null && !_config.closeOnConfirm)
            {
                _onConfirm?.Invoke(true, _callbackData);
            }
            else
            {
                Hide(() =>
                {
                    _onConfirm?.Invoke(true, _callbackData);
                });
            }
        }

        private void OnCancelClick()
        {
            if (_config != null && !_config.closeOnCancel)
            {
                _onCancel?.Invoke(false, _callbackData);
            }
            else
            {
                Hide(() =>
                {
                    _onCancel?.Invoke(false, _callbackData);
                });
            }
        }

        private void OnCloseClick()
        {
            CloseDialog();
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_config != null && _config.autoCloseTime > 0)
            {
                _autoCloseTimer += Time.deltaTime;
                if (_autoCloseTimer >= _config.autoCloseTime)
                {
                    _isAutoClosing = true;
                    CloseDialog();
                }
            }
        }

        #endregion
    }
}
