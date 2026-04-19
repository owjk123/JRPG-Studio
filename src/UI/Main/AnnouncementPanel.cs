// AnnouncementPanel.cs - 公告面板组件
// 显示游戏公告和活动信息

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text;
using JRPG.UI.Common;
using JRPG.UI.Core;

namespace JRPG.UI.Main
{
    /// <summary>
    /// 公告数据
    /// </summary>
    [Serializable]
    public class AnnouncementData
    {
        public int id;
        public string title;
        public string content;
        public AnnouncementType type;
        public string imageUrl;
        public long startTime;
        public long endTime;
        public bool isPinned;        // 是否置顶
        public bool isNew;          // 是否为新公告
        public string linkUrl;       // 跳转链接
        public int priority;         // 优先级
    }

    /// <summary>
    /// 公告类型
    /// </summary>
    public enum AnnouncementType
    {
        System = 0,     // 系统公告
        Activity = 1,   // 活动公告
        Maintenance = 2, // 维护公告
        Update = 3,     // 更新公告
        Event = 4       // 事件公告
    }

    /// <summary>
    /// 公告条目显示数据
    /// </summary>
    public class AnnouncementItemData
    {
        public AnnouncementData data;
        public bool isRead;
        public bool isClaimed;      // 是否已领取
    }

    /// <summary>
    /// 公告面板组件
    /// </summary>
    public class AnnouncementPanel : BasePanel
    {
        #region UI References

        [Header("标题栏")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _settingsButton;

        [Header("标签页")]
        [SerializeField] private TabGroup _tabGroup;
        [SerializeField] private List<AnnouncementType> _tabTypes = new List<AnnouncementType>();

        [Header("列表")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _announcementItemPrefab;

        [Header("空状态")]
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private TextMeshProUGUI _emptyText;

        [Header("详情")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TextMeshProUGUI _detailTitleText;
        [SerializeField] private TextMeshProUGUI _detailTimeText;
        [SerializeField] private TextMeshProUGUI _detailContentText;
        [SerializeField] private Image _detailImage;
        [SerializeField] private Button _detailLinkButton;

        #endregion

        #region Fields

        // 数据
        private List<AnnouncementData> _allAnnouncements = new List<AnnouncementData>();
        private Dictionary<AnnouncementType, List<AnnouncementData>> _announcementsByType = new Dictionary<AnnouncementType, List<AnnouncementData>>();
        private AnnouncementType _currentType = AnnouncementType.System;
        private AnnouncementData _currentDetailData;

        // 状态
        private bool _isLoading = false;
        private AnnouncementItem _currentSelectedItem;

        // 设置
        private bool _showEmptyOnNoData = true;
        private bool _autoMarkAsRead = true;

        // 回调
        private Action<AnnouncementData> _onAnnouncementClick;
        private Action<int> _onLinkClick;

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
            InitializeTabs();
            SetupEvents();
        }

        protected override void OnPanelInit()
        {
            // 加载公告数据
            LoadAnnouncements();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();
            RefreshAnnouncementList();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示公告
        /// </summary>
        public void ShowAnnouncement()
        {
            Show();
        }

        /// <summary>
        /// 显示指定类型的公告
        /// </summary>
        public void ShowAnnouncement(AnnouncementType type)
        {
            _currentType = type;
            Show();
        }

        /// <summary>
        /// 显示公告详情
        /// </summary>
        public void ShowDetail(AnnouncementData data)
        {
            _currentDetailData = data;
            UpdateDetailPanel(data);

            if (_detailPanel != null)
            {
                _detailPanel.SetActive(true);
                UITweener.PopIn(_detailPanel.GetComponent<RectTransform>());
            }

            // 标记为已读
            if (_autoMarkAsRead)
            {
                MarkAsRead(data.id);
            }
        }

        /// <summary>
        /// 加载公告数据
        /// </summary>
        public void LoadAnnouncements()
        {
            if (_isLoading) return;
            _isLoading = true;

            // TODO: 从网络加载公告数据
            // 这里使用模拟数据
            LoadMockAnnouncements();

            // 按类型分组
            GroupAnnouncementsByType();

            _isLoading = false;
        }

        /// <summary>
        /// 刷新公告列表
        /// </summary>
        public void RefreshAnnouncementList()
        {
            if (!_isVisible) return;

            // 根据当前标签类型显示对应公告
            UpdateAnnouncementList(_currentType);
        }

        /// <summary>
        /// 标记公告为已读
        /// </summary>
        public void MarkAsRead(int announcementId)
        {
            var data = _allAnnouncements.Find(a => a.id == announcementId);
            if (data != null)
            {
                data.isNew = false;
            }
        }

        /// <summary>
        /// 注册公告点击回调
        /// </summary>
        public void RegisterAnnouncementClick(Action<AnnouncementData> callback)
        {
            _onAnnouncementClick = callback;
        }

        /// <summary>
        /// 注册链接点击回调
        /// </summary>
        public void RegisterLinkClick(Action<int> callback)
        {
            _onLinkClick = callback;
        }

        /// <summary>
        /// 获取未读公告数量
        /// </summary>
        public int GetUnreadCount()
        {
            return _allAnnouncements.FindAll(a => a.isNew).Count;
        }

        /// <summary>
        /// 获取指定类型的未读数量
        /// </summary>
        public int GetUnreadCount(AnnouncementType type)
        {
            if (_announcementsByType.TryGetValue(type, out var list))
            {
                return list.FindAll(a => a.isNew).Count;
            }
            return 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化标签页
        /// </summary>
        private void InitializeTabs()
        {
            if (_tabGroup != null)
            {
                _tabGroup.OnTabChanged.AddListener(OnTabChanged);

                // 添加标签
                foreach (var type in _tabTypes)
                {
                    _tabGroup.AddTab(GetTabTitle(type));
                }

                // 默认选中第一个
                if (_tabTypes.Count > 0)
                {
                    _currentType = _tabTypes[0];
                }
            }
        }

        /// <summary>
        /// 设置事件
        /// </summary>
        private void SetupEvents()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(Hide);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettingsClick);
            }

            if (_detailLinkButton != null)
            {
                _detailLinkButton.onClick.RemoveAllListeners();
                _detailLinkButton.onClick.AddListener(OnDetailLinkClick);
            }
        }

        /// <summary>
        /// 标签切换回调
        /// </summary>
        private void OnTabChanged(int oldIndex, int newIndex)
        {
            if (newIndex >= 0 && newIndex < _tabTypes.Count)
            {
                _currentType = _tabTypes[newIndex];
                UpdateAnnouncementList(_currentType);
            }
        }

        /// <summary>
        /// 加载模拟公告数据
        /// </summary>
        private void LoadMockAnnouncements()
        {
            _allAnnouncements.Clear();

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 系统公告
            _allAnnouncements.Add(new AnnouncementData
            {
                id = 1001,
                title = "游戏开服公告",
                content = "亲爱的冒险者，欢迎来到JRPG的世界！祝您游戏愉快。\n\n新增内容：\n1. 全新角色「星之守护者」登场\n2. 新增「远古遗迹」副本\n3. 优化了战斗体验\n4. 修复了若干已知问题",
                type = AnnouncementType.System,
                startTime = currentTime - 86400,
                endTime = currentTime + 86400 * 30,
                isPinned = true,
                isNew = true
            });

            // 活动公告
            _allAnnouncements.Add(new AnnouncementData
            {
                id = 2001,
                title = "周末双倍奖励活动",
                content = "活动期间，完成副本可获得双倍经验和金币奖励！\n\n活动时间：周六00:00 - 周日24:00\n参与方式：通关任意副本即可自动参与",
                type = AnnouncementType.Activity,
                startTime = currentTime,
                endTime = currentTime + 86400 * 2,
                isPinned = true,
                isNew = true
            });

            // 维护公告
            _allAnnouncements.Add(new AnnouncementData
            {
                id = 3001,
                title = "服务器维护通知",
                content = "为了提供更好的游戏体验，我们将在以下时间进行服务器维护。\n\n维护时间：2024年1月15日 06:00-10:00\n维护内容：版本更新、数据库优化",
                type = AnnouncementType.Maintenance,
                startTime = currentTime - 3600,
                endTime = currentTime + 3600 * 4,
                isPinned = true,
                isNew = false
            });

            // 更新公告
            _allAnnouncements.Add(new AnnouncementData
            {
                id = 4001,
                title = "V1.2.0版本更新",
                content = "新版本现已发布！\n\n更新内容：\n- 新增角色培养系统\n- 开放公会战玩法\n- 新增时装系统\n- UI界面优化",
                type = AnnouncementType.Update,
                startTime = currentTime - 86400,
                endTime = currentTime + 86400 * 365,
                isNew = true
            });
        }

        /// <summary>
        /// 按类型分组公告
        /// </summary>
        private void GroupAnnouncementsByType()
        {
            _announcementsByType.Clear();

            foreach (var data in _allAnnouncements)
            {
                if (!_announcementsByType.ContainsKey(data.type))
                {
                    _announcementsByType[data.type] = new List<AnnouncementData>();
                }
                _announcementsByType[data.type].Add(data);
            }

            // 排序：置顶优先，然后按时间倒序
            foreach (var list in _announcementsByType.Values)
            {
                list.Sort((a, b) =>
                {
                    if (a.isPinned != b.isPinned) return b.isPinned.CompareTo(a.isPinned);
                    return b.startTime.CompareTo(a.startTime);
                });
            }
        }

        /// <summary>
        /// 更新公告列表
        /// </summary>
        private void UpdateAnnouncementList(AnnouncementType type)
        {
            if (_listContainer == null) return;

            // 清空现有列表
            foreach (Transform child in _listContainer)
            {
                Destroy(child.gameObject);
            }

            // 获取对应类型的公告
            List<AnnouncementData> announcements;
            if (_announcementsByType.TryGetValue(type, out announcements))
            {
                // 显示公告列表
                foreach (var data in announcements)
                {
                    CreateAnnouncementItem(data);
                }
            }

            // 更新空状态
            if (_emptyState != null)
            {
                _emptyState.SetActive(announcements == null || announcements.Count == 0);
            }
        }

        /// <summary>
        /// 创建公告条目
        /// </summary>
        private void CreateAnnouncementItem(AnnouncementData data)
        {
            GameObject itemObj;

            if (_announcementItemPrefab != null)
            {
                itemObj = Instantiate(_announcementItemPrefab, _listContainer);
            }
            else
            {
                itemObj = CreateDefaultAnnouncementItem();
            }

            AnnouncementItem item = itemObj.GetComponent<AnnouncementItem>();
            if (item != null)
            {
                item.Initialize(data);
                item.OnItemClicked += OnAnnouncementItemClicked;
            }
        }

        /// <summary>
        /// 创建默认公告条目
        /// </summary>
        private GameObject CreateDefaultAnnouncementItem()
        {
            GameObject itemObj = new GameObject("AnnouncementItem");
            itemObj.transform.SetParent(_listContainer);

            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 120);
            rect.anchoredPosition = Vector2.zero;

            itemObj.AddComponent<AnnouncementItem>();
            return itemObj;
        }

        /// <summary>
        /// 公告条目点击
        /// </summary>
        private void OnAnnouncementItemClicked(AnnouncementItem item)
        {
            ShowDetail(item.Data);
            _currentSelectedItem = item;
            _onAnnouncementClick?.Invoke(item.Data);
        }

        /// <summary>
        /// 更新详情面板
        /// </summary>
        private void UpdateDetailPanel(AnnouncementData data)
        {
            if (_detailTitleText != null)
            {
                _detailTitleText.text = data.title;
            }

            if (_detailTimeText != null)
            {
                DateTime startTime = DateTimeOffset.FromUnixTimeSeconds(data.startTime).LocalDateTime;
                DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(data.endTime).LocalDateTime;
                _detailTimeText.text = $"{startTime:yyyy-MM-dd HH:mm} - {endTime:yyyy-MM-dd HH:mm}";
            }

            if (_detailContentText != null)
            {
                _detailContentText.text = data.content;
            }

            // 显示链接按钮
            if (_detailLinkButton != null)
            {
                _detailLinkButton.gameObject.SetActive(!string.IsNullOrEmpty(data.linkUrl));
            }
        }

        /// <summary>
        /// 设置按钮点击
        /// </summary>
        private void OnSettingsClick()
        {
            Debug.Log("[AnnouncementPanel] 打开公告设置");
        }

        /// <summary>
        /// 详情链接点击
        /// </summary>
        private void OnDetailLinkClick()
        {
            if (_currentDetailData != null && !string.IsNullOrEmpty(_currentDetailData.linkUrl))
            {
                _onLinkClick?.Invoke(_currentDetailData.id);
                Application.OpenURL(_currentDetailData.linkUrl);
            }
        }

        /// <summary>
        /// 获取标签标题
        /// </summary>
        private string GetTabTitle(AnnouncementType type)
        {
            switch (type)
            {
                case AnnouncementType.System: return "系统";
                case AnnouncementType.Activity: return "活动";
                case AnnouncementType.Maintenance: return "维护";
                case AnnouncementType.Update: return "更新";
                case AnnouncementType.Event: return "事件";
                default: return "全部";
            }
        }

        #endregion
    }

    /// <summary>
    /// 公告条目组件
    /// </summary>
    public class AnnouncementItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private GameObject _newBadge;
        [SerializeField] private GameObject _pinnedBadge;
        [SerializeField] private Image _typeIcon;

        #endregion

        #region Fields

        private AnnouncementData _data;
        public event Action<AnnouncementItem> OnItemClicked;

        #endregion

        #region Properties

        public AnnouncementData Data => _data;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(AnnouncementData data)
        {
            _data = data;

            // 设置标题
            if (_titleText != null)
            {
                _titleText.text = data.title;
            }

            // 设置时间
            if (_timeText != null)
            {
                DateTime time = DateTimeOffset.FromUnixTimeSeconds(data.startTime).LocalDateTime;
                _timeText.text = time.ToString("yyyy-MM-dd");
            }

            // 设置新标签
            if (_newBadge != null)
            {
                _newBadge.SetActive(data.isNew);
            }

            // 设置置顶标签
            if (_pinnedBadge != null)
            {
                _pinnedBadge.SetActive(data.isPinned);
            }

            // 设置类型图标
            UpdateTypeIcon();
        }

        #endregion

        #region Private Methods

        private void OnClick()
        {
            OnItemClicked?.Invoke(this);
        }

        private void UpdateTypeIcon()
        {
            if (_typeIcon == null) return;

            // 根据类型设置图标
            // TODO: 加载对应类型的图标
        }

        #endregion
    }
}
