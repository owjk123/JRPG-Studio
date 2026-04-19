// SettingsPanel.cs - 设置主界面
// 游戏设置主面板

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;

namespace JRPG.UI.Settings
{
    /// <summary>
    /// 设置面板
    /// </summary>
    public class SettingsPanel : BasePanel
    {
        #region UI References

        [Header("顶部栏")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("设置分类")]
        [SerializeField] private TabGroup _categoryTabGroup;
        [SerializeField] private int _audioTabIndex = 0;
        [SerializeField] private int _displayTabIndex = 1;
        [SerializeField] private int _accountTabIndex = 2;

        [Header("设置内容")]
        [SerializeField] private GameObject _audioPanel;
        [SerializeField] private GameObject _displayPanel;
        [SerializeField] private GameObject _accountPanel;

        [Header("子组件")]
        [SerializeField] private AudioSettings _audioSettings;
        [SerializeField] private DisplaySettings _displaySettings;
        [SerializeField] private AccountPanel _accountPanelComponent;

        [Header("其他设置")]
        [SerializeField] private Button _aboutButton;
        [SerializeField] private Button _helpButton;
        [SerializeField] private Button _logoutButton;
        [SerializeField] private Button _quitButton;

        #endregion

        #region Fields

        private int _currentTabIndex = 0;
        private bool _hasUnsavedChanges = false;

        // 事件
        public event Action OnSettingsChanged;
        public event Action OnLogoutRequested;
        public event Action OnQuitRequested;

        #endregion

        #region BasePanel Override

        protected override void Awake()
        {
            base.Awake();
            _layer = PanelLayer.Normal;
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
            InitializeSubPanels();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();
            Refresh();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 刷新设置
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // 刷新子面板
            _audioSettings?.Refresh();
            _displaySettings?.Refresh();
            _accountPanelComponent?.Refresh();
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings()
        {
            _audioSettings?.SaveSettings();
            _displaySettings?.SaveSettings();
            _accountPanelComponent?.SaveSettings();

            _hasUnsavedChanges = false;
            OnSettingsChanged?.Invoke();

            Debug.Log("[SettingsPanel] 设置已保存");
        }

        /// <summary>
        /// 重置设置
        /// </summary>
        public void ResetSettings()
        {
            _audioSettings?.ResetToDefault();
            _displaySettings?.ResetToDefault();
        }

        /// <summary>
        /// 检查是否有未保存的更改
        /// </summary>
        public bool HasUnsavedChanges()
        {
            return _hasUnsavedChanges;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化子面板
        /// </summary>
        private void InitializeSubPanels()
        {
            _audioSettings?.Initialize();
            _displaySettings?.Initialize();
            _accountPanelComponent?.Initialize();

            // 默认显示第一个面板
            ShowPanel(0);
        }

        /// <summary>
        /// 显示指定面板
        /// </summary>
        private void ShowPanel(int index)
        {
            if (_audioPanel != null)
            {
                _audioPanel.SetActive(index == _audioTabIndex);
            }

            if (_displayPanel != null)
            {
                _displayPanel.SetActive(index == _displayTabIndex);
            }

            if (_accountPanel != null)
            {
                _accountPanel.SetActive(index == _accountTabIndex);
            }
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(OnBackClick);
            }

            if (_categoryTabGroup != null)
            {
                _categoryTabGroup.OnTabChanged.AddListener(OnCategoryChanged);
            }

            if (_aboutButton != null)
            {
                _aboutButton.onClick.RemoveAllListeners();
                _aboutButton.onClick.AddListener(OnAboutClick);
            }

            if (_helpButton != null)
            {
                _helpButton.onClick.RemoveAllListeners();
                _helpButton.onClick.AddListener(OnHelpClick);
            }

            if (_logoutButton != null)
            {
                _logoutButton.onClick.RemoveAllListeners();
                _logoutButton.onClick.AddListener(OnLogoutClick);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveAllListeners();
                _quitButton.onClick.AddListener(OnQuitClick);
            }
        }

        /// <summary>
        /// 标记有未保存的更改
        /// </summary>
        private void MarkAsChanged()
        {
            _hasUnsavedChanges = true;
        }

        #endregion

        #region Event Handlers

        private void OnBackClick()
        {
            // 检查未保存的更改
            if (_hasUnsavedChanges)
            {
                // 显示确认对话框
                ShowUnsavedChangesDialog();
            }
            else
            {
                Hide();
            }
        }

        private void ShowUnsavedChangesDialog()
        {
            // TODO: 显示保存确认对话框
            // DialogBox.ShowConfirm("设置已更改，是否保存？", OnSaveDialogConfirm);
            SaveSettings();
            Hide();
        }

        private void OnCategoryChanged(int oldIndex, int newIndex)
        {
            _currentTabIndex = newIndex;
            ShowPanel(newIndex);
        }

        private void OnAboutClick()
        {
            // 显示关于界面
            Debug.Log("[SettingsPanel] 显示关于");
        }

        private void OnHelpClick()
        {
            // 显示帮助界面
            Debug.Log("[SettingsPanel] 显示帮助");
        }

        private void OnLogoutClick()
        {
            // 显示退出登录确认
            Debug.Log("[SettingsPanel] 退出登录");
            OnLogoutRequested?.Invoke();
        }

        private void OnQuitClick()
        {
            // 显示退出游戏确认
            Debug.Log("[SettingsPanel] 退出游戏");
            OnQuitRequested?.Invoke();
        }

        #endregion
    }
}
