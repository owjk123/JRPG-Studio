// MainMenuController.cs - 主界面控制器
// 管理主界面各个模块的显示和交互

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using JRPG.Core;
using JRPG.UI.Common;

namespace JRPG.UI.Main
{
    /// <summary>
    /// 主界面导航索引
    /// </summary>
    public enum MainNavIndex
    {
        Home = 0,       // 主页
        Character = 1,   // 角色
        Gacha = 2,      // 抽卡
        Story = 3,      // 剧情
        Settings = 4    // 设置
    }

    /// <summary>
    /// 主界面控制器
    /// </summary>
    public class MainMenuController : BasePanel
    {
        #region UI References

        [Header("顶部状态栏")]
        [SerializeField] private TopBar _topBar;

        [Header("底部导航")]
        [SerializeField] private BottomNavigation _bottomNavigation;

        [Header("公告面板")]
        [SerializeField] private AnnouncementPanel _announcementPanel;

        [Header("功能按钮")]
        [SerializeField] private Button _dailyButton;
        [SerializeField] private Button _mailButton;
        [SerializeField] private Button _questButton;
        [SerializeField] private Button _shopButton;

        [Header("子界面容器")]
        [SerializeField] private Transform _subPanelContainer;

        #endregion

        #region Fields

        // 子面板引用
        private Dictionary<MainNavIndex, BasePanel> _subPanels = new Dictionary<MainNavIndex, BasePanel>();
        private MainNavIndex _currentNavIndex = MainNavIndex.Home;
        private BasePanel _currentSubPanel;

        // 状态
        private bool _isInitialized = false;
        private bool _isSubPanelTransitioning = false;

        // 事件
        public event Action<MainNavIndex> OnNavChanged;

        #endregion

        #region Properties

        /// <summary>
        /// 当前导航索引
        /// </summary>
        public MainNavIndex CurrentNavIndex => _currentNavIndex;

        /// <summary>
        /// 当前子面板
        /// </summary>
        public BasePanel CurrentSubPanel => _currentSubPanel;

        #endregion

        #region BasePanel Override

        protected override void Awake()
        {
            base.Awake();
            _layer = PanelLayer.Normal;
            _useOpenAnimation = false;
        }

        protected override void Start()
        {
            base.Start();
            Initialize();
        }

        protected override void OnPanelInit()
        {
            // 注册导航回调
            if (_bottomNavigation != null)
            {
                _bottomNavigation.OnNavSelected += OnNavSelected;
            }

            // 注册功能按钮
            RegisterFeatureButtons();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();

            // 刷新顶部数据
            _topBar?.Refresh();

            // 检查公告
            CheckAnnouncement();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化主界面
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // 初始化顶部状态栏
            _topBar?.Initialize();

            // 初始化底部导航
            _bottomNavigation?.Initialize();

            // 加载子面板
            LoadSubPanels();

            // 显示默认子界面
            ShowSubPanel(MainNavIndex.Home);

            _isInitialized = true;
        }

        private void LoadSubPanels()
        {
            // 预加载子面板
            // 子面板应该在 Resources 或 Addressables 中定义
        }

        #endregion

        #region Navigation

        /// <summary>
        /// 导航选中回调
        /// </summary>
        private void OnNavSelected(int index)
        {
            if (index < 0 || index >= Enum.GetValues(typeof(MainNavIndex)).Length) return;

            MainNavIndex navIndex = (MainNavIndex)index;
            ShowSubPanel(navIndex);
        }

        /// <summary>
        /// 显示指定子面板
        /// </summary>
        public void ShowSubPanel(MainNavIndex navIndex)
        {
            if (_isSubPanelTransitioning) return;

            if (_currentSubPanel != null)
            {
                // 隐藏当前面板
                _currentSubPanel.Hide(() =>
                {
                    TransitionToSubPanel(navIndex);
                });
            }
            else
            {
                TransitionToSubPanel(navIndex);
            }
        }

        /// <summary>
        /// 切换到子面板
        /// </summary>
        private void TransitionToSubPanel(MainNavIndex navIndex)
        {
            _isSubPanelTransitioning = true;
            _currentNavIndex = navIndex;

            // 获取或创建子面板
            if (!_subPanels.TryGetValue(navIndex, out _currentSubPanel))
            {
                _currentSubPanel = CreateSubPanel(navIndex);
                if (_currentSubPanel != null)
                {
                    _subPanels[navIndex] = _currentSubPanel;
                }
            }

            if (_currentSubPanel != null)
            {
                _currentSubPanel.Show(() =>
                {
                    _isSubPanelTransitioning = false;
                });
            }
            else
            {
                _isSubPanelTransitioning = false;
            }

            // 更新导航状态
            _bottomNavigation?.SetSelectedIndex((int)navIndex);

            // 触发事件
            OnNavChanged?.Invoke(navIndex);
        }

        /// <summary>
        /// 创建子面板
        /// </summary>
        private BasePanel CreateSubPanel(MainNavIndex navIndex)
        {
            // 根据导航索引创建对应的子面板
            string panelName = GetSubPanelName(navIndex);

            // 这里应该通过UIManager或资源加载来创建面板
            // 示例：return UIManager.Instance.OpenSubPanel(panelName, _subPanelContainer);

            return null;
        }

        /// <summary>
        /// 获取子面板名称
        /// </summary>
        private string GetSubPanelName(MainNavIndex navIndex)
        {
            switch (navIndex)
            {
                case MainNavIndex.Home:
                    return "HomePanel";
                case MainNavIndex.Character:
                    return "CharacterListPanel";
                case MainNavIndex.Gacha:
                    return "GachaMainPanel";
                case MainNavIndex.Story:
                    return "StoryMainPanel";
                case MainNavIndex.Settings:
                    return "SettingsPanel";
                default:
                    return "UnknownPanel";
            }
        }

        #endregion

        #region Feature Buttons

        /// <summary>
        /// 注册功能按钮
        /// </summary>
        private void RegisterFeatureButtons()
        {
            if (_dailyButton != null)
            {
                _dailyButton.onClick.RemoveAllListeners();
                _dailyButton.onClick.AddListener(OnDailyButtonClick);
            }

            if (_mailButton != null)
            {
                _mailButton.onClick.RemoveAllListeners();
                _mailButton.onClick.AddListener(OnMailButtonClick);
            }

            if (_questButton != null)
            {
                _questButton.onClick.RemoveAllListeners();
                _questButton.onClick.AddListener(OnQuestButtonClick);
            }

            if (_shopButton != null)
            {
                _shopButton.onClick.RemoveAllListeners();
                _shopButton.onClick.AddListener(OnShopButtonClick);
            }
        }

        private void OnDailyButtonClick()
        {
            Debug.Log("[MainMenu] 打开日常界面");
            // 打开日常界面
        }

        private void OnMailButtonClick()
        {
            Debug.Log("[MainMenu] 打开邮件界面");
            // 打开邮件界面
        }

        private void OnQuestButtonClick()
        {
            Debug.Log("[MainMenu] 打开任务界面");
            // 打开任务界面
        }

        private void OnShopButtonClick()
        {
            Debug.Log("[MainMenu] 打开商店界面");
            // 打开商店界面
        }

        #endregion

        #region Announcement

        /// <summary>
        /// 检查公告
        /// </summary>
        private void CheckAnnouncement()
        {
            // 检查是否需要显示公告
            bool hasNewAnnouncement = true; // TODO: 从数据管理器获取

            if (hasNewAnnouncement && _announcementPanel != null)
            {
                _announcementPanel.ShowAnnouncement();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 返回主页
        /// </summary>
        public void GoToHome()
        {
            ShowSubPanel(MainNavIndex.Home);
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            _topBar?.Refresh();

            if (_currentSubPanel != null)
            {
                _currentSubPanel.Refresh();
            }
        }

        /// <summary>
        /// 显示红点提示
        /// </summary>
        public void ShowRedDot(MainNavIndex navIndex, bool show)
        {
            _bottomNavigation?.ShowRedDot((int)navIndex, show);
        }

        #endregion
    }
}
