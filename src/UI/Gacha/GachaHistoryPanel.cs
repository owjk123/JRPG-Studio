// GachaHistoryPanel.cs - 抽卡历史面板
// 展示抽卡历史记录

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;

namespace JRPG.UI.Gacha
{
    /// <summary>
    /// 抽卡历史条目
    /// </summary>
    [Serializable]
    public class GachaHistoryEntry
    {
        public int id;
        public string itemName;
        public int rarity;
        public string poolName;
        public int costType;
        public int costAmount;
        public long timestamp;
        public bool isRateUp;
    }

    /// <summary>
    /// 抽卡历史面板
    /// </summary>
    public class GachaHistoryPanel : BasePanel
    {
        #region UI References

        [Header("顶部栏")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("统计")]
        [SerializeField] private TextMeshProUGUI _totalGachaText;
        [SerializeField] private TextMeshProUGUI _totalCostText;

        [Header("筛选")]
        [SerializeField] private TabGroup _filterTabGroup;
        [SerializeField] private int _allTabIndex = 0;
        [SerializeField] private int _characterTabIndex = 1;
        [SerializeField] private int _weaponTabIndex = 2;

        [Header("历史列表")]
        [SerializeField] private ScrollRect _historyScrollRect;
        [SerializeField] private VerticalLayoutGroup _historyListContainer;
        [SerializeField] private GameObject _historyItemPrefab;

        [Header("空状态")]
        [SerializeField] private GameObject _emptyState;

        #endregion

        #region Fields

        private List<GachaHistoryEntry> _allHistory = new List<GachaHistoryEntry>();
        private List<GachaHistoryEntry> _filteredHistory = new List<GachaHistoryEntry>();
        private int _currentFilter = 0; // 0=全部, 1=角色, 2=武器

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
            LoadHistory();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();
            UpdateDisplay();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 加载历史记录
        /// </summary>
        public void LoadHistory()
        {
            _allHistory.Clear();

            // TODO: 从服务器加载历史记录
            // 模拟数据
            for (int i = 0; i < 50; i++)
            {
                _allHistory.Add(new GachaHistoryEntry
                {
                    id = 1000 + i,
                    itemName = $"角色{i % 10}",
                    rarity = i % 20 == 0 ? 5 : (i % 5 == 0 ? 4 : 3),
                    poolName = i % 3 == 0 ? "限定角色池" : "常驻池",
                    costType = 1,
                    costAmount = i % 3 == 0 ? 2800 : 280,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (i * 3600),
                    isRateUp = i % 20 == 0
                });
            }

            ApplyFilter();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            LoadHistory();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 应用筛选
        /// </summary>
        private void ApplyFilter()
        {
            _filteredHistory.Clear();

            foreach (var entry in _allHistory)
            {
                // 根据筛选条件过滤
                // _currentFilter: 0=全部, 1=角色, 2=武器
                if (_currentFilter == 0 ||
                    (_currentFilter == 1 && entry.rarity >= 4) ||
                    (_currentFilter == 2 && entry.rarity <= 3))
                {
                    _filteredHistory.Add(entry);
                }
            }

            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateStatistics();
            UpdateHistoryList();
            UpdateEmptyState();
        }

        /// <summary>
        /// 更新统计
        /// </summary>
        private void UpdateStatistics()
        {
            if (_totalGachaText != null)
            {
                _totalGachaText.text = $"总抽数: {_allHistory.Count}";
            }

            if (_totalCostText != null)
            {
                long totalCost = 0;
                foreach (var entry in _allHistory)
                {
                    totalCost += entry.costAmount;
                }
                _totalCostText.text = $"总消耗: {totalCost}";
            }
        }

        /// <summary>
        /// 更新历史列表
        /// </summary>
        private void UpdateHistoryList()
        {
            if (_historyListContainer == null) return;

            // 清空现有
            foreach (Transform child in _historyListContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // 创建条目
            foreach (var entry in _filteredHistory)
            {
                CreateHistoryItem(entry);
            }
        }

        /// <summary>
        /// 创建历史条目
        /// </summary>
        private void CreateHistoryItem(GachaHistoryEntry entry)
        {
            GameObject itemObj;

            if (_historyItemPrefab != null)
            {
                itemObj = Instantiate(_historyItemPrefab, _historyListContainer.transform);
            }
            else
            {
                itemObj = CreateDefaultHistoryItem(entry);
            }

            var item = itemObj.GetComponent<GachaHistoryItem>();
            if (item != null)
            {
                item.Initialize(entry);
            }
        }

        /// <summary>
        /// 创建默认历史条目
        /// </summary>
        private GameObject CreateDefaultHistoryItem(GachaHistoryEntry entry)
        {
            GameObject itemObj = new GameObject("HistoryItem");
            itemObj.transform.SetParent(_historyListContainer?.transform);

            var layout = itemObj.AddComponent<LayoutElement>();
            layout.minHeight = 80;

            var item = itemObj.AddComponent<GachaHistoryItem>();
            return itemObj;
        }

        /// <summary>
        /// 更新空状态
        /// </summary>
        private void UpdateEmptyState()
        {
            if (_emptyState != null)
            {
                _emptyState.SetActive(_filteredHistory.Count == 0);
            }

            if (_historyListContainer != null)
            {
                _historyListContainer.gameObject.SetActive(_filteredHistory.Count > 0);
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

            if (_filterTabGroup != null)
            {
                _filterTabGroup.OnTabChanged.AddListener(OnFilterChanged);
            }
        }

        #endregion

        #region Event Handlers

        private void OnBackClick()
        {
            Hide();
        }

        private void OnFilterChanged(int oldIndex, int newIndex)
        {
            _currentFilter = newIndex;
            ApplyFilter();
        }

        #endregion
    }

    /// <summary>
    /// 抽卡历史条目组件
    /// </summary>
    public class GachaHistoryItem : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _poolNameText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private Image _rarityBadge;
        [SerializeField] private GameObject _rateUpBadge;

        private GachaHistoryEntry _entry;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(GachaHistoryEntry entry)
        {
            _entry = entry;

            if (_itemNameText != null)
            {
                _itemNameText.text = entry.itemName;
            }

            if (_poolNameText != null)
            {
                _poolNameText.text = entry.poolName;
            }

            if (_timeText != null)
            {
                DateTime time = DateTimeOffset.FromUnixTimeSeconds(entry.timestamp).LocalDateTime;
                _timeText.text = time.ToString("MM-dd HH:mm");
            }

            if (_rarityBadge != null)
            {
                _rarityBadge.color = GetRarityColor(entry.rarity);
            }

            if (_rateUpBadge != null)
            {
                _rateUpBadge.SetActive(entry.isRateUp);
            }
        }

        private Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1f, 0.8f, 0.2f);
                case 4: return new Color(0.6f, 0.4f, 1f);
                case 3: return new Color(0.3f, 0.6f, 1f);
                default: return Color.white;
            }
        }
    }
}
