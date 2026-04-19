// CharacterListPanel.cs - 角色列表面板
// 展示和管理玩家拥有的所有角色

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using JRPG.UI.Common;
using JRPG.UI.Core;
using JRPG.Character;

namespace JRPG.UI.Character
{
    /// <summary>
    /// 角色排序方式
    /// </summary>
    public enum CharacterSortType
    {
        Power,       // 战力排序
        Level,       // 等级排序
        Rarity,      // 稀有度排序
        Element,     // 元素排序
        New          // 最新获得
    }

    /// <summary>
    /// 角色筛选类型
    /// </summary>
    [Flags]
    public enum CharacterFilter
    {
        None = 0,
        Rarity5 = 1 << 0,    // 5星
        Rarity4 = 1 << 1,    // 4星
        Rarity3 = 1 << 2,    // 3星
        Element_Fire = 1 << 3,    // 火元素
        Element_Water = 1 << 4,   // 水元素
        Element_Earth = 1 << 5,   // 土元素
        Element_Wind = 1 << 6,    // 风元素
        Element_Light = 1 << 7,   // 光元素
        Element_Dark = 1 << 8     // 暗元素
    }

    /// <summary>
    /// 角色列表面板
    /// </summary>
    public class CharacterListPanel : BasePanel
    {
        #region UI References

        [Header("顶部栏")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _characterCountText;

        [Header("筛选排序")]
        [SerializeField] private Button _filterButton;
        [SerializeField] private Button _sortButton;
        [SerializeField] private TextMeshProUGUI _sortText;
        [SerializeField] private TextMeshProUGUI _filterText;

        [Header("角色列表")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private Transform _contentContainer;

        [Header("角色卡片预制体")]
        [SerializeField] private CharacterCard _characterCardPrefab;

        [Header("空状态")]
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private TextMeshProUGUI _emptyText;
        [SerializeField] private Button _goGetButton;

        [Header("筛选面板")]
        [SerializeField] private GameObject _filterPanel;
        [SerializeField] private Toggle[] _rarityToggles;
        [SerializeField] private Toggle[] _elementToggles;

        [Header("排序面板")]
        [SerializeField] private GameObject _sortPanel;
        [SerializeField] private Button[] _sortButtons;

        #endregion

        #region Fields

        // 数据
        private List<CharacterData> _allCharacters = new List<CharacterData>();
        private List<CharacterData> _filteredCharacters = new List<CharacterData>();
        private List<CharacterCard> _characterCards = new List<CharacterCard>();

        // 状态
        private CharacterSortType _currentSortType = CharacterSortType.Power;
        private CharacterFilter _currentFilter = CharacterFilter.None;

        // 配置
        private int _gridColumnCount = 3;
        private float _cardSpacing = 10f;

        // 事件
        public event Action<CharacterData> OnCharacterSelected;

        #endregion

        #region Properties

        /// <summary>
        /// 当前显示的角色数量
        /// </summary>
        public int DisplayCount => _filteredCharacters.Count;

        /// <summary>
        /// 当前排序方式
        /// </summary>
        public CharacterSortType CurrentSortType => _currentSortType;

        /// <summary>
        /// 当前筛选条件
        /// </summary>
        public CharacterFilter CurrentFilter => _currentFilter;

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
            SetupUI();
            SetupEvents();
        }

        protected override void OnPanelInit()
        {
            LoadCharacters();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();
            Refresh();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 刷新角色列表
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            ApplyFilter();
            ApplySort();
            UpdateCharacterList();
            UpdateCountText();
        }

        /// <summary>
        /// 设置排序方式
        /// </summary>
        public void SetSortType(CharacterSortType sortType)
        {
            _currentSortType = sortType;
            UpdateSortText();
            ApplySort();
            UpdateCharacterList();
        }

        /// <summary>
        /// 设置筛选条件
        /// </summary>
        public void SetFilter(CharacterFilter filter)
        {
            _currentFilter = filter;
            UpdateFilterText();
            ApplyFilter();
            ApplySort();
            UpdateCharacterList();
        }

        /// <summary>
        /// 添加筛选条件
        /// </summary>
        public void AddFilter(CharacterFilter filter)
        {
            _currentFilter |= filter;
            UpdateFilterText();
            Refresh();
        }

        /// <summary>
        /// 移除筛选条件
        /// </summary>
        public void RemoveFilter(CharacterFilter filter)
        {
            _currentFilter &= ~filter;
            UpdateFilterText();
            Refresh();
        }

        /// <summary>
        /// 清除所有筛选
        /// </summary>
        public void ClearFilter()
        {
            _currentFilter = CharacterFilter.None;
            UpdateFilterText();
            Refresh();
        }

        /// <summary>
        /// 高亮指定角色
        /// </summary>
        public void HighlightCharacter(int characterId)
        {
            var card = _characterCards.Find(c => c.CharacterId == characterId);
            if (card != null)
            {
                // 滚动到可视范围
                ScrollToCard(card);
                // 播放高亮动画
                UITweener.BounceIn(card.GetComponent<RectTransform>(), 0.3f);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            // 设置网格布局
            if (_gridLayout != null)
            {
                _gridLayout.cellSize = new Vector2(200, 280);
                _gridLayout.spacing = new Vector2(_cardSpacing, _cardSpacing);
                _gridLayout.constraintCount = _gridColumnCount;
            }

            // 初始化筛选面板
            SetupFilterPanel();

            // 初始化排序面板
            SetupSortPanel();
        }

        /// <summary>
        /// 设置事件
        /// </summary>
        private void SetupEvents()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(OnBackClick);
            }

            if (_filterButton != null)
            {
                _filterButton.onClick.RemoveAllListeners();
                _filterButton.onClick.AddListener(OnFilterClick);
            }

            if (_sortButton != null)
            {
                _sortButton.onClick.RemoveAllListeners();
                _sortButton.onClick.AddListener(OnSortClick);
            }

            if (_goGetButton != null)
            {
                _goGetButton.onClick.RemoveAllListeners();
                _goGetButton.onClick.AddListener(OnGoGetClick);
            }
        }

        /// <summary>
        /// 设置筛选面板
        /// </summary>
        private void SetupFilterPanel()
        {
            if (_rarityToggles != null)
            {
                foreach (var toggle in _rarityToggles)
                {
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(OnRarityToggleChanged);
                }
            }

            if (_elementToggles != null)
            {
                foreach (var toggle in _elementToggles)
                {
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(OnElementToggleChanged);
                }
            }
        }

        /// <summary>
        /// 设置排序面板
        /// </summary>
        private void SetupSortPanel()
        {
            if (_sortButtons != null)
            {
                for (int i = 0; i < _sortButtons.Length; i++)
                {
                    int index = i;
                    _sortButtons[i].onClick.RemoveAllListeners();
                    _sortButtons[i].onClick.AddListener(() => OnSortButtonSelected(index));
                }
            }
        }

        /// <summary>
        /// 加载角色数据
        /// </summary>
        private void LoadCharacters()
        {
            // TODO: 从角色管理器加载角色数据
            // _allCharacters = CharacterManager.Instance.GetAllCharacters();

            // 使用模拟数据
            LoadMockCharacters();
        }

        /// <summary>
        /// 加载模拟角色数据
        /// </summary>
        private void LoadMockCharacters()
        {
            _allCharacters.Clear();

            // 创建模拟角色
            string[] names = { "艾琳", "雷欧", "卡莲", "尤利乌斯", "菲莉娅", "洛克", "艾琳娜", "亚瑟", "莉莉安", "修" };
            int[] elements = { 0, 1, 2, 3, 4, 0, 1, 2, 3, 4 };
            int[] rarities = { 5, 4, 5, 4, 3, 4, 5, 3, 4, 5 };
            int[] levels = { 80, 75, 90, 60, 50, 70, 85, 45, 65, 88 };
            int[] powers = { 52000, 48000, 60000, 35000, 28000, 42000, 55000, 25000, 38000, 58000 };

            for (int i = 0; i < names.Length; i++)
            {
                var character = new CharacterData
                {
                    id = 1000 + i,
                    characterId = 1000 + i,
                    name = names[i],
                    level = levels[i],
                    exp = 0,
                    rarity = rarities[i],
                    element = (ElementType)elements[i],
                    power = powers[i],
                    isOwned = true,
                    isNew = i < 2 // 前两个是新获得的
                };

                _allCharacters.Add(character);
            }
        }

        /// <summary>
        /// 应用筛选
        /// </summary>
        private void ApplyFilter()
        {
            _filteredCharacters.Clear();

            foreach (var character in _allCharacters)
            {
                if (ShouldShowCharacter(character))
                {
                    _filteredCharacters.Add(character);
                }
            }
        }

        /// <summary>
        /// 判断角色是否应该显示
        /// </summary>
        private bool ShouldShowCharacter(CharacterData character)
        {
            // 稀有度筛选
            if ((_currentFilter & CharacterFilter.Rarity5) != 0 && character.rarity != 5) return false;
            if ((_currentFilter & CharacterFilter.Rarity4) != 0 && character.rarity != 4) return false;
            if ((_currentFilter & CharacterFilter.Rarity3) != 0 && character.rarity != 3) return false;

            // 元素筛选
            ElementType element = character.element;
            bool elementMatch = true;

            if ((_currentFilter & CharacterFilter.Element_Fire) != 0 && element != ElementType.Fire) elementMatch = false;
            else if ((_currentFilter & CharacterFilter.Element_Water) != 0 && element != ElementType.Water) elementMatch = false;
            else if ((_currentFilter & CharacterFilter.Element_Earth) != 0 && element != ElementType.Earth) elementMatch = false;
            else if ((_currentFilter & CharacterFilter.Element_Wind) != 0 && element != ElementType.Wind) elementMatch = false;
            else if ((_currentFilter & CharacterFilter.Element_Light) != 0 && element != ElementType.Light) elementMatch = false;
            else if ((_currentFilter & CharacterFilter.Element_Dark) != 0 && element != ElementType.Dark) elementMatch = false;

            return elementMatch;
        }

        /// <summary>
        /// 应用排序
        /// </summary>
        private void ApplySort()
        {
            switch (_currentSortType)
            {
                case CharacterSortType.Power:
                    _filteredCharacters = _filteredCharacters.OrderByDescending(c => c.power).ToList();
                    break;
                case CharacterSortType.Level:
                    _filteredCharacters = _filteredCharacters.OrderByDescending(c => c.level).ThenByDescending(c => c.rarity).ToList();
                    break;
                case CharacterSortType.Rarity:
                    _filteredCharacters = _filteredCharacters.OrderByDescending(c => c.rarity).ThenByDescending(c => c.level).ToList();
                    break;
                case CharacterSortType.Element:
                    _filteredCharacters = _filteredCharacters.OrderBy(c => c.element).ThenByDescending(c => c.rarity).ToList();
                    break;
                case CharacterSortType.New:
                    _filteredCharacters = _filteredCharacters.OrderByDescending(c => c.isNew).ToList();
                    break;
            }
        }

        /// <summary>
        /// 更新角色列表显示
        /// </summary>
        private void UpdateCharacterList()
        {
            // 清空现有卡片
            ClearCharacterCards();

            // 更新空状态
            if (_emptyState != null)
            {
                _emptyState.SetActive(_filteredCharacters.Count == 0);
            }

            // 创建新卡片
            foreach (var character in _filteredCharacters)
            {
                CreateCharacterCard(character);
            }
        }

        /// <summary>
        /// 创建角色卡片
        /// </summary>
        private void CreateCharacterCard(CharacterData data)
        {
            CharacterCard card;

            if (_characterCardPrefab != null)
            {
                var cardObj = Instantiate(_characterCardPrefab.gameObject, _contentContainer);
                card = cardObj.GetComponent<CharacterCard>();
            }
            else
            {
                // 创建默认卡片
                var cardObj = new GameObject("CharacterCard");
                cardObj.transform.SetParent(_contentContainer);
                card = cardObj.AddComponent<CharacterCard>();
            }

            card.Initialize(data);
            card.OnCardClicked += OnCharacterCardClicked;
            _characterCards.Add(card);
        }

        /// <summary>
        /// 清空角色卡片
        /// </summary>
        private void ClearCharacterCards()
        {
            foreach (var card in _characterCards)
            {
                if (card != null)
                {
                    card.OnCardClicked -= OnCharacterCardClicked;
                    Destroy(card.gameObject);
                }
            }
            _characterCards.Clear();
        }

        /// <summary>
        /// 更新计数文本
        /// </summary>
        private void UpdateCountText()
        {
            if (_characterCountText != null)
            {
                _characterCountText.text = $"{_filteredCharacters.Count}/{_allCharacters.Count}";
            }
        }

        /// <summary>
        /// 更新排序文本
        /// </summary>
        private void UpdateSortText()
        {
            if (_sortText != null)
            {
                string[] sortNames = { "战力", "等级", "稀有度", "元素", "最新" };
                _sortText.text = sortNames[(int)_currentSortType];
            }
        }

        /// <summary>
        /// 更新筛选文本
        /// </summary>
        private void UpdateFilterText()
        {
            if (_filterText != null)
            {
                if (_currentFilter == CharacterFilter.None)
                {
                    _filterText.text = "全部";
                }
                else
                {
                    int count = 0;
                    // 统计筛选条件数量
                    for (int i = 0; i < 9; i++)
                    {
                        if (((int)_currentFilter & (1 << i)) != 0) count++;
                    }
                    _filterText.text = $"筛选({count})";
                }
            }
        }

        /// <summary>
        /// 滚动到指定卡片
        /// </summary>
        private void ScrollToCard(CharacterCard card)
        {
            // 实现滚动到指定位置
            // TODO: 使用ScrollRect的scrollRect.content.localPosition来调整
        }

        #endregion

        #region Event Handlers

        private void OnBackClick()
        {
            Hide();
        }

        private void OnFilterClick()
        {
            if (_filterPanel != null)
            {
                bool isActive = _filterPanel.activeSelf;
                _filterPanel.SetActive(!isActive);
                _sortPanel?.SetActive(false);
            }
        }

        private void OnSortClick()
        {
            if (_sortPanel != null)
            {
                bool isActive = _sortPanel.activeSelf;
                _sortPanel.SetActive(!isActive);
                _filterPanel?.SetActive(false);
            }
        }

        private void OnSortButtonSelected(int index)
        {
            SetSortType((CharacterSortType)index);
            if (_sortPanel != null)
            {
                _sortPanel.SetActive(false);
            }
        }

        private void OnRarityToggleChanged(bool isOn)
        {
            Refresh();
        }

        private void OnElementToggleChanged(bool isOn)
        {
            Refresh();
        }

        private void OnCharacterCardClicked(CharacterCard card)
        {
            OnCharacterSelected?.Invoke(card.Data);
        }

        private void OnGoGetClick()
        {
            // 打开抽卡界面
            Debug.Log("[CharacterList] 打开抽卡界面");
        }

        #endregion
    }
}
