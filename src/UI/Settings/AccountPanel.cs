// AccountPanel.cs - 账号相关面板
// 管理账号设置、绑定和登录

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using JRPG.UI.Common;

namespace JRPG.UI.Settings
{
    /// <summary>
    /// 账号信息
    /// </summary>
    [Serializable]
    public class AccountInfo
    {
        public int userId;
        public string username;
        public string displayName;
        public string email;
        public string phone;
        public bool isGuest;
        public bool isBound;           // 是否已绑定其他账号
        public long createTime;
        public long lastLoginTime;
    }

    /// <summary>
    /// 账号面板组件
    /// </summary>
    public class AccountPanel : MonoBehaviour
    {
        #region UI References

        [Header("账号信息")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _userIdText;
        [SerializeField] private TextMeshProUGUI _usernameText;
        [SerializeField] private TextMeshProUGUI _memberSinceText;
        [SerializeField] private TextMeshProUGUI _lastLoginText;

        [Header("绑定状态")]
        [SerializeField] private GameObject _guestWarningPanel;
        [SerializeField] private TextMeshProUGUI _bindStatusText;
        [SerializeField] private Button _bindEmailButton;
        [SerializeField] private Button _bindPhoneButton;
        [SerializeField] private Button _bindThirdPartyButton;

        [Header("登录选项")]
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _logoutButton;
        [SerializeField] private Button _switchAccountButton;

        [Header("数据管理")]
        [SerializeField] private Button _changeNameButton;
        [SerializeField] private Button _changePasswordButton;
        [SerializeField] private Button _resetDataButton;
        [SerializeField] private Button _downloadDataButton;

        [Header("其他")]
        [SerializeField] private GameObject _linkedAccountPanel;
        [SerializeField] private Transform _linkedAccountContainer;

        #endregion

        #region Fields

        private AccountInfo _accountInfo;
        private bool _isInitialized = false;

        // 事件
        public event Action OnLogoutRequested;
        public event Action OnBindRequested;
        public event Action OnDataResetRequested;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            LoadAccountInfo();
            SetupButtons();
            UpdateDisplay();
            _isInitialized = true;
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            if (!_isInitialized) return;
            LoadAccountInfo();
            UpdateDisplay();
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings()
        {
            // 账号信息通常不需要保存
        }

        /// <summary>
        /// 设置账号信息
        /// </summary>
        public void SetAccountInfo(AccountInfo info)
        {
            _accountInfo = info;
            UpdateDisplay();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 加载账号信息
        /// </summary>
        private void LoadAccountInfo()
        {
            // TODO: 从服务器或本地加载账号信息
            // 模拟数据
            _accountInfo = new AccountInfo
            {
                userId = 10001,
                username = "Player10001",
                displayName = "冒险者",
                email = "player@example.com",
                phone = "138****8888",
                isGuest = false,
                isBound = true,
                createTime = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
                lastLoginTime = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds()
            };
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (_accountInfo == null) return;

            UpdateAccountInfoDisplay();
            UpdateBindStatusDisplay();
            UpdateLoginOptionsDisplay();
        }

        /// <summary>
        /// 更新账号信息显示
        /// </summary>
        private void UpdateAccountInfoDisplay()
        {
            if (_userIdText != null)
            {
                _userIdText.text = $"ID: {_accountInfo.userId}";
            }

            if (_usernameText != null)
            {
                _usernameText.text = _accountInfo.displayName;
            }

            if (_memberSinceText != null)
            {
                DateTime createTime = DateTimeOffset.FromUnixTimeSeconds(_accountInfo.createTime).LocalDateTime;
                _memberSinceText.text = $"注册时间: {createTime:yyyy-MM-dd}";
            }

            if (_lastLoginText != null)
            {
                DateTime lastLogin = DateTimeOffset.FromUnixTimeSeconds(_accountInfo.lastLoginTime).LocalDateTime;
                _lastLoginText.text = $"最后登录: {lastLogin:yyyy-MM-dd HH:mm}";
            }
        }

        /// <summary>
        /// 更新绑定状态显示
        /// </summary>
        private void UpdateBindStatusDisplay()
        {
            // 游客账号警告
            if (_guestWarningPanel != null)
            {
                _guestWarningPanel.SetActive(_accountInfo.isGuest);
            }

            // 绑定状态文本
            if (_bindStatusText != null)
            {
                if (_accountInfo.isBound)
                {
                    _bindStatusText.text = "已绑定";
                }
                else
                {
                    _bindStatusText.text = "未绑定（建议绑定以保护账号）";
                }
            }

            // 绑定按钮状态
            UpdateBindButtons();
        }

        /// <summary>
        /// 更新绑定按钮
        /// </summary>
        private void UpdateBindButtons()
        {
            if (_bindEmailButton != null)
            {
                bool hasEmail = !string.IsNullOrEmpty(_accountInfo.email);
                _bindEmailButton.interactable = !hasEmail;
                var text = _bindEmailButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = hasEmail ? "已绑定邮箱" : "绑定邮箱";
                }
            }

            if (_bindPhoneButton != null)
            {
                bool hasPhone = !string.IsNullOrEmpty(_accountInfo.phone);
                _bindPhoneButton.interactable = !hasPhone;
                var text = _bindPhoneButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = hasPhone ? "已绑定手机" : "绑定手机";
                }
            }
        }

        /// <summary>
        /// 更新登录选项显示
        /// </summary>
        private void UpdateLoginOptionsDisplay()
        {
            if (_accountInfo.isGuest)
            {
                // 游客账号
                if (_loginButton != null)
                {
                    _loginButton.gameObject.SetActive(true);
                    var text = _loginButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null) text.text = "登录/注册";
                }

                if (_logoutButton != null)
                {
                    _logoutButton.gameObject.SetActive(false);
                }
            }
            else
            {
                // 正式账号
                if (_loginButton != null)
                {
                    _loginButton.gameObject.SetActive(false);
                }

                if (_logoutButton != null)
                {
                    _logoutButton.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_bindEmailButton != null)
            {
                _bindEmailButton.onClick.RemoveAllListeners();
                _bindEmailButton.onClick.AddListener(OnBindEmailClick);
            }

            if (_bindPhoneButton != null)
            {
                _bindPhoneButton.onClick.RemoveAllListeners();
                _bindPhoneButton.onClick.AddListener(OnBindPhoneClick);
            }

            if (_bindThirdPartyButton != null)
            {
                _bindThirdPartyButton.onClick.RemoveAllListeners();
                _bindThirdPartyButton.onClick.AddListener(OnBindThirdPartyClick);
            }

            if (_loginButton != null)
            {
                _loginButton.onClick.RemoveAllListeners();
                _loginButton.onClick.AddListener(OnLoginClick);
            }

            if (_logoutButton != null)
            {
                _logoutButton.onClick.RemoveAllListeners();
                _logoutButton.onClick.AddListener(OnLogoutClick);
            }

            if (_switchAccountButton != null)
            {
                _switchAccountButton.onClick.RemoveAllListeners();
                _switchAccountButton.onClick.AddListener(OnSwitchAccountClick);
            }

            if (_changeNameButton != null)
            {
                _changeNameButton.onClick.RemoveAllListeners();
                _changeNameButton.onClick.AddListener(OnChangeNameClick);
            }

            if (_changePasswordButton != null)
            {
                _changePasswordButton.onClick.RemoveAllListeners();
                _changePasswordButton.onClick.AddListener(OnChangePasswordClick);
            }

            if (_resetDataButton != null)
            {
                _resetDataButton.onClick.RemoveAllListeners();
                _resetDataButton.onClick.AddListener(OnResetDataClick);
            }

            if (_downloadDataButton != null)
            {
                _downloadDataButton.onClick.RemoveAllListeners();
                _downloadDataButton.onClick.AddListener(OnDownloadDataClick);
            }
        }

        #endregion

        #region Event Handlers

        private void OnBindEmailClick()
        {
            Debug.Log("[AccountPanel] 绑定邮箱");
            OnBindRequested?.Invoke();
        }

        private void OnBindPhoneClick()
        {
            Debug.Log("[AccountPanel] 绑定手机");
            OnBindRequested?.Invoke();
        }

        private void OnBindThirdPartyClick()
        {
            Debug.Log("[AccountPanel] 第三方账号绑定");
            OnBindRequested?.Invoke();
        }

        private void OnLoginClick()
        {
            // 打开登录界面
            Debug.Log("[AccountPanel] 打开登录界面");
        }

        private void OnLogoutClick()
        {
            Debug.Log("[AccountPanel] 退出登录");
            OnLogoutRequested?.Invoke();
        }

        private void OnSwitchAccountClick()
        {
            Debug.Log("[AccountPanel] 切换账号");
            OnLogoutRequested?.Invoke();
        }

        private void OnChangeNameClick()
        {
            // 打开改名界面
            Debug.Log("[AccountPanel] 打开改名界面");
        }

        private void OnChangePasswordClick()
        {
            // 打开修改密码界面
            Debug.Log("[AccountPanel] 打开修改密码界面");
        }

        private void OnResetDataClick()
        {
            // 显示重置确认对话框
            Debug.Log("[AccountPanel] 重置游戏数据");
            OnDataResetRequested?.Invoke();
        }

        private void OnDownloadDataClick()
        {
            // 下载账号数据
            Debug.Log("[AccountPanel] 下载账号数据");
        }

        #endregion
    }
}
