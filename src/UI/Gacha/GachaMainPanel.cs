// GachaMainPanel.cs - 抽卡主界面
// 展示和管理抽卡功能

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;
using JRPG.UI.Core;

namespace JRPG.UI.Gacha
{
    /// <summary>
    /// 抽卡保底类型
    /// </summary>
    public enum GachaPityType
    {
        None,
        Character,     // 角色保底
        Weapon         // 武器保底
    }

    /// <summary>
    /// 卡池信息
    /// </summary>
    [Serializable]
    public class GachaPoolData
    {
        public int poolId;
        public string poolName;
        public string description;
        public Sprite bannerImage;
        public int costType;          // 消耗货币类型
        public int singleCost;         // 单抽消耗
        public int tenCost;           // 十连消耗
        public float[] rarityRates;   // 各稀有度概率 [3星, 4星, 5星]
        public GachaPityType pityType;
        public int pityCount;         // 保底计数
        public int guaranteedRateUp;   // 必出高稀有度保底数
        public long startTime;
        public long endTime;
        public bool isLimited;        // 是否限定池
    }

    /// <summary>
    /// 抽卡主界面
    /// </summary>
    public class GachaMainPanel : BasePanel
    {
        #region UI References

        [Header("顶部栏")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _historyButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("货币显示")]
        [SerializeField] private TextMeshProUGUI _currencyAmountText;
        [SerializeField] private Image _currencyIcon;

        [Header("轮播图")]
        [SerializeField] private ScrollRect _bannerScrollRect;
        [SerializeField] private PageIndicator _pageIndicator;
        [SerializeField] private List<Image> _bannerImages = new List<Image>();

        [Header("卡池选择")]
        [SerializeField] private TabGroup _poolTabGroup;
        [SerializeField] private GachaPoolSelect _poolSelector;

        [Header("保底显示")]
        [SerializeField] private ProgressBar _pityProgressBar;
        [SerializeField] private TextMeshProUGUI _pityCountText;
        [SerializeField] private TextMeshProUGUI _pityInfoText;

        [Header("抽卡按钮")]
        [SerializeField] private Button _singleGachaButton;
        [SerializeField] private TextMeshProUGUI _singleCostText;
        [SerializeField] private Button _tenGachaButton;
        [SerializeField] private TextMeshProUGUI _tenCostText;

        [Header("概率信息")]
        [SerializeField] private Button _rateInfoButton;
        [SerializeField] private GameObject _rateInfoPanel;

        [Header("卡池信息")]
        [SerializeField] private TextMeshProUGUI _poolNameText;
        [SerializeField] private TextMeshProUGUI _poolTimeText;
        [SerializeField] private TextMeshProUGUI _poolDescText;

        #endregion

        #region Fields

        private List<GachaPoolData> _availablePools = new List<GachaPoolData>();
        private GachaPoolData _currentPool;
        private int _currentPity = 0;
        private int _guaranteedPity = 90; // 90抽保底
        private bool _isAnimating = false;

        // 事件
        public event Action<GachaPoolData, int> OnGachaRequested; // pool, count (1 or 10)

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
            LoadGachaPools();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();
            RefreshCurrency();
            UpdatePityDisplay();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 刷新货币显示
        /// </summary>
        public void RefreshCurrency()
        {
            if (_currencyAmountText != null)
            {
                // TODO: 从货币管理器获取
                _currencyAmountText.text = "1000";
            }
        }

        /// <summary>
        /// 更新保底显示
        /// </summary>
        public void UpdatePityDisplay()
        {
            if (_currentPool == null) return;

            int currentPity = _currentPool.pityCount;
            int guaranteedPity = _currentPool.guaranteedRateUp;

            if (_pityProgressBar != null)
            {
                _pityProgressBar.SetValue(currentPity);
                _pityProgressBar.SetRange(0, guaranteedPity);
            }

            if (_pityCountText != null)
            {
                _pityCountText.text = $"{currentPity}/{guaranteedPity}";
            }

            if (_pityInfoText != null)
            {
                int remaining = guaranteedPity - currentPity;
                _pityInfoText.text = $"再抽{remaining}抽必出5星";
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 加载卡池数据
        /// </summary>
        private void LoadGachaPools()
        {
            _availablePools.Clear();

            // 模拟卡池数据
            _availablePools.Add(new GachaPoolData
            {
                poolId = 1,
                poolName = "限定角色池",
                description = "限定角色「星之守护者」概率UP！",
                costType = 1,
                singleCost = 280,
                tenCost = 2800,
                rarityRates = new float[] { 0.5f, 0.43f, 0.07f },
                pityType = GachaPityType.Character,
                pityCount = 45,
                guaranteedRateUp = 90,
                isLimited = true
            });

            _availablePools.Add(new GachaPoolData
            {
                poolId = 2,
                poolName = "常驻角色池",
                description = "所有角色均有机会获得",
                costType = 1,
                singleCost = 280,
                tenCost = 2800,
                rarityRates = new float[] { 0.6f, 0.35f, 0.05f },
                pityType = GachaPityType.Character,
                pityCount = 0,
                guaranteedRateUp = 90,
                isLimited = false
            });

            _availablePools.Add(new GachaPoolData
            {
                poolId = 3,
                poolName = "武器池",
                description = "强力武器等你来拿",
                costType = 1,
                singleCost = 100,
                tenCost = 1000,
                rarityRates = new float[] { 0.7f, 0.25f, 0.05f },
                pityType = GachaPityType.Weapon,
                pityCount = 12,
                guaranteedRateUp = 80,
                isLimited = false
            });

            // 默认选中第一个
            if (_availablePools.Count > 0)
            {
                SelectPool(_availablePools[0]);
            }

            UpdatePoolSelector();
        }

        /// <summary>
        /// 更新卡池选择器
        /// </summary>
        private void UpdatePoolSelector()
        {
            if (_poolSelector != null)
            {
                _poolSelector.Initialize(_availablePools);
                _poolSelector.OnPoolSelected += OnPoolSelected;
            }
        }

        /// <summary>
        /// 选中卡池
        /// </summary>
        private void SelectPool(GachaPoolData pool)
        {
            _currentPool = pool;
            UpdatePoolDisplay();
            UpdatePityDisplay();
        }

        /// <summary>
        /// 更新卡池显示
        /// </summary>
        private void UpdatePoolDisplay()
        {
            if (_currentPool == null) return;

            if (_poolNameText != null)
            {
                _poolNameText.text = _currentPool.poolName;
            }

            if (_poolTimeText != null)
            {
                // 显示剩余时间
                DateTime endTime = DateTimeOffset.FromUnixTimeSeconds(_currentPool.endTime).LocalDateTime;
                TimeSpan remaining = endTime - DateTime.Now;
                if (remaining.TotalDays > 0)
                {
                    _poolTimeText.text = $"剩余 {remaining.Days}天";
                }
                else
                {
                    _poolTimeText.text = $"剩余 {remaining.Hours}小时";
                }
            }

            if (_poolDescText != null)
            {
                _poolDescText.text = _currentPool.description;
            }

            // 更新按钮消耗显示
            if (_singleCostText != null)
            {
                _singleCostText.text = _currentPool.singleCost.ToString();
            }

            if (_tenCostText != null)
            {
                _tenCostText.text = _currentPool.tenCost.ToString();
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

            if (_historyButton != null)
            {
                _historyButton.onClick.RemoveAllListeners();
                _historyButton.onClick.AddListener(OnHistoryClick);
            }

            if (_singleGachaButton != null)
            {
                _singleGachaButton.onClick.RemoveAllListeners();
                _singleGachaButton.onClick.AddListener(OnSingleGachaClick);
            }

            if (_tenGachaButton != null)
            {
                _tenGachaButton.onClick.RemoveAllListeners();
                _tenGachaButton.onClick.AddListener(OnTenGachaClick);
            }

            if (_rateInfoButton != null)
            {
                _rateInfoButton.onClick.RemoveAllListeners();
                _rateInfoButton.onClick.AddListener(OnRateInfoClick);
            }
        }

        /// <summary>
        /// 检查是否有足够的货币
        /// </summary>
        private bool HasEnoughCurrency(int cost)
        {
            // TODO: 检查货币是否足够
            return true;
        }

        #endregion

        #region Event Handlers

        private void OnPoolSelected(GachaPoolData pool)
        {
            SelectPool(pool);
        }

        private void OnBackClick()
        {
            Hide();
        }

        private void OnHistoryClick()
        {
            // 打开抽卡历史界面
            Debug.Log("[GachaMainPanel] 打开抽卡历史");
        }

        private void OnSingleGachaClick()
        {
            if (_isAnimating) return;
            if (!HasEnoughCurrency(_currentPool.singleCost))
            {
                // 显示货币不足提示
                Toast.Instance.ShowError("钻石不足");
                return;
            }

            _isAnimating = true;
            OnGachaRequested?.Invoke(_currentPool, 1);
        }

        private void OnTenGachaClick()
        {
            if (_isAnimating) return;
            if (!HasEnoughCurrency(_currentPool.tenCost))
            {
                Toast.Instance.ShowError("钻石不足");
                return;
            }

            _isAnimating = true;
            OnGachaRequested?.Invoke(_currentPool, 10);
        }

        private void OnRateInfoClick()
        {
            if (_rateInfoPanel != null)
            {
                bool isActive = _rateInfoPanel.activeSelf;
                _rateInfoPanel.SetActive(!isActive);
            }
        }

        /// <summary>
        /// 抽卡完成回调
        /// </summary>
        public void OnGachaComplete()
        {
            _isAnimating = false;
            RefreshCurrency();
            UpdatePityDisplay();
        }

        #endregion
    }

    /// <summary>
    /// 卡池选择组件
    /// </summary>
    public class GachaPoolSelect : MonoBehaviour
    {
        #region UI References

        [SerializeField] private HorizontalLayoutGroup _poolListContainer;
        [SerializeField] private GachaPoolItem _poolItemPrefab;
        [SerializeField] private List<GachaPoolItem> _poolItems = new List<GachaPoolItem>();

        #endregion

        #region Fields

        private List<GachaPoolData> _pools = new List<GachaPoolData>();
        private GachaPoolData _selectedPool;

        // 事件
        public event Action<GachaPoolData> OnPoolSelected;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            // 初始化
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(List<GachaPoolData> pools)
        {
            _pools = pools;
            CreatePoolItems();
        }

        /// <summary>
        /// 选中卡池
        /// </summary>
        public void SelectPool(GachaPoolData pool)
        {
            _selectedPool = pool;

            foreach (var item in _poolItems)
            {
                item.SetSelected(item.Data.poolId == pool.poolId);
            }
        }

        #endregion

        #region Private Methods

        private void CreatePoolItems()
        {
            ClearPoolItems();

            foreach (var pool in _pools)
            {
                GachaPoolItem item;

                if (_poolItemPrefab != null)
                {
                    var itemObj = Instantiate(_poolItemPrefab.gameObject, _poolListContainer?.transform);
                    item = itemObj.GetComponent<GachaPoolItem>();
                }
                else
                {
                    var itemObj = new GameObject($"Pool_{pool.poolId}");
                    itemObj.transform.SetParent(_poolListContainer?.transform);
                    item = itemObj.AddComponent<GachaPoolItem>();
                }

                item.Initialize(pool);
                item.OnPoolClicked += OnPoolItemClicked;
                _poolItems.Add(item);
            }
        }

        private void ClearPoolItems()
        {
            foreach (var item in _poolItems)
            {
                if (item != null)
                {
                    item.OnPoolClicked -= OnPoolItemClicked;
                    Destroy(item.gameObject);
                }
            }
            _poolItems.Clear();
        }

        private void OnPoolItemClicked(GachaPoolItem item)
        {
            _selectedPool = item.Data;
            SelectPool(item.Data);
            OnPoolSelected?.Invoke(item.Data);
        }

        #endregion
    }

    /// <summary>
    /// 卡池选择条目组件
    /// </summary>
    public class GachaPoolItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Image _poolImage;
        [SerializeField] private TextMeshProUGUI _poolNameText;
        [SerializeField] private Image _limitedBadge;
        [SerializeField] private Button _itemButton;

        #endregion

        #region Fields

        private GachaPoolData _data;
        private bool _isSelected = false;

        // 事件
        public event Action<GachaPoolItem> OnPoolClicked;

        #endregion

        #region Properties

        public GachaPoolData Data => _data;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_itemButton != null)
            {
                _itemButton.onClick.RemoveAllListeners();
                _itemButton.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(GachaPoolData data)
        {
            _data = data;

            if (_poolNameText != null)
            {
                _poolNameText.text = data.poolName;
            }

            if (_limitedBadge != null)
            {
                _limitedBadge.gameObject.SetActive(data.isLimited);
            }
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            // TODO: 更新选中视觉效果
        }

        #endregion

        #region Private Methods

        private void OnClick()
        {
            OnPoolClicked?.Invoke(this);
        }

        #endregion
    }

    /// <summary>
    /// 页面指示器
    /// </summary>
    public class PageIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject _dotPrefab;
        [SerializeField] private Transform _dotContainer;
        private List<GameObject> _dots = new List<GameObject>();
        private int _currentPage = 0;

        public void SetTotalPages(int total)
        {
            // 清空现有
            foreach (var dot in _dots)
            {
                Destroy(dot);
            }
            _dots.Clear();

            // 创建指示点
            for (int i = 0; i < total; i++)
            {
                var dot = Instantiate(_dotPrefab, _dotContainer);
                _dots.Add(dot);
            }

            UpdateIndicator();
        }

        public void SetCurrentPage(int page)
        {
            _currentPage = page;
            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            for (int i = 0; i < _dots.Count; i++)
            {
                _dots[i]?.SetActive(i == _currentPage);
            }
        }
    }
}
