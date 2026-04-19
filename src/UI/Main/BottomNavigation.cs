// BottomNavigation.cs - 底部导航组件
// 提供底部标签导航功能

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;

namespace JRPG.UI.Main
{
    /// <summary>
    /// 导航项数据
    /// </summary>
    [Serializable]
    public class NavItemData
    {
        public string id;
        public string title;
        public Sprite iconNormal;
        public Sprite iconSelected;
        public bool isInteractable = true;
        public bool showRedDot = false;
        public int badgeCount = 0;
    }

    /// <summary>
    /// 底部导航组件
    /// </summary>
    public class BottomNavigation : MonoBehaviour
    {
        #region UI References

        [Header("导航项")]
        [SerializeField] private List<NavItem> _navItems = new List<NavItem>();

        [Header("选中指示器")]
        [SerializeField] private Image _selectedIndicator;
        [SerializeField] private float _indicatorMoveDuration = 0.2f;

        [Header("背景")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _bgSprite;

        #endregion

        #region Fields

        // 配置
        [SerializeField] private List<NavItemData> _navDataList = new List<NavItemData>();

        // 状态
        private int _selectedIndex = 0;
        private bool _isAnimating = false;
        private float _originalIndicatorX;

        // 事件
        public event Action<int> OnNavSelected;

        #endregion

        #region Properties

        /// <summary>
        /// 当前选中索引
        /// </summary>
        public int SelectedIndex => _selectedIndex;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeNavItems();
        }

        protected virtual void Start()
        {
            SetupNavItems();

            // 默认选中第一项
            if (_navItems.Count > 0)
            {
                SetSelectedIndex(0, false);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            InitializeNavItems();
            SetupNavItems();
        }

        /// <summary>
        /// 设置选中索引
        /// </summary>
        public void SetSelectedIndex(int index, bool notify = true)
        {
            if (index < 0 || index >= _navItems.Count) return;
            if (_isAnimating && notify) return;

            int oldIndex = _selectedIndex;
            _selectedIndex = index;

            // 更新UI
            UpdateNavSelection();

            // 移动指示器
            MoveIndicator(index);

            // 触发事件
            if (notify)
            {
                OnNavSelected?.Invoke(index);
            }
        }

        /// <summary>
        /// 显示/隐藏红点
        /// </summary>
        public void ShowRedDot(int index, bool show)
        {
            if (index < 0 || index >= _navItems.Count) return;
            _navItems[index].ShowRedDot(show);
        }

        /// <summary>
        /// 设置徽章数量
        /// </summary>
        public void SetBadge(int index, int count)
        {
            if (index < 0 || index >= _navItems.Count) return;
            _navItems[index].SetBadge(count);
        }

        /// <summary>
        /// 设置导航项是否可交互
        /// </summary>
        public void SetInteractable(int index, bool interactable)
        {
            if (index < 0 || index >= _navItems.Count) return;
            _navItems[index].SetInteractable(interactable);
        }

        /// <summary>
        /// 更新导航数据
        /// </summary>
        public void UpdateNavData(int index, NavItemData data)
        {
            if (index < 0 || index >= _navDataList.Count) return;

            _navDataList[index] = data;

            if (index < _navItems.Count)
            {
                _navItems[index].SetData(data);
            }
        }

        /// <summary>
        /// 获取导航项
        /// </summary>
        public NavItem GetNavItem(int index)
        {
            if (index < 0 || index >= _navItems.Count) return null;
            return _navItems[index];
        }

        /// <summary>
        /// 设置所有红点状态
        /// </summary>
        public void ClearAllRedDots()
        {
            foreach (var item in _navItems)
            {
                item.ShowRedDot(false);
            }
        }

        /// <summary>
        /// 刷新导航数据
        /// </summary>
        public void Refresh()
        {
            // 重新从数据源加载红点状态等
            // TODO: 从通知管理器等数据源刷新
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化导航项
        /// </summary>
        private void InitializeNavItems()
        {
            // 如果没有预配置的导航项，根据默认数据创建
            if (_navItems.Count == 0)
            {
                CreateDefaultNavItems();
            }

            // 设置导航项索引
            for (int i = 0; i < _navItems.Count; i++)
            {
                _navItems[i].SetIndex(i);
                _navItems[i].OnItemSelected += OnNavItemSelected;
            }
        }

        /// <summary>
        /// 创建默认导航项
        /// </summary>
        private void CreateDefaultNavItems()
        {
            _navItems.Clear();

            // 默认导航数据
            string[] defaultTitles = { "首页", "角色", "抽卡", "剧情", "设置" };
            MainNavIndex[] defaultNavs = { MainNavIndex.Home, MainNavIndex.Character, MainNavIndex.Gacha, MainNavIndex.Story, MainNavIndex.Settings };

            for (int i = 0; i < defaultTitles.Length; i++)
            {
                // 创建导航项GameObject
                GameObject itemObj = new GameObject($"NavItem_{i}");
                itemObj.transform.SetParent(transform);

                NavItem item = itemObj.AddComponent<NavItem>();
                item.SetData(new NavItemData
                {
                    id = defaultNavs[i].ToString(),
                    title = defaultTitles[i],
                    isInteractable = true
                });

                _navItems.Add(item);
            }
        }

        /// <summary>
        /// 设置导航项
        /// </summary>
        private void SetupNavItems()
        {
            // 加载导航数据
            for (int i = 0; i < _navItems.Count; i++)
            {
                if (i < _navDataList.Count)
                {
                    _navItems[i].SetData(_navDataList[i]);
                }
                else
                {
                    // 使用默认数据
                    _navItems[i].SetData(new NavItemData
                    {
                        id = $"nav_{i}",
                        title = $"导航{i}",
                        isInteractable = true
                    });
                }
            }

            // 记录指示器原始位置
            if (_selectedIndicator != null)
            {
                _originalIndicatorX = _selectedIndicator.rectTransform.anchoredPosition.x;
            }
        }

        /// <summary>
        /// 导航项选中回调
        /// </summary>
        private void OnNavItemSelected(int index)
        {
            SetSelectedIndex(index);
        }

        /// <summary>
        /// 更新导航选中状态
        /// </summary>
        private void UpdateNavSelection()
        {
            for (int i = 0; i < _navItems.Count; i++)
            {
                _navItems[i].SetSelected(i == _selectedIndex);
            }
        }

        /// <summary>
        /// 移动指示器
        /// </summary>
        private void MoveIndicator(int index)
        {
            if (_selectedIndicator == null || index < 0 || index >= _navItems.Count) return;

            RectTransform indicatorRect = _selectedIndicator.rectTransform;
            RectTransform itemRect = _navItems[index].RectTransform;

            // 计算目标位置
            Vector2 targetPos = indicatorRect.anchoredPosition;
            targetPos.x = itemRect.anchoredPosition.x;

            // 动画移动
            Core.UITweener.TweenPosition(
                indicatorRect,
                indicatorRect.anchoredPosition,
                targetPos,
                _indicatorMoveDuration,
                AnimationCurve.EaseInOut(0, 0, 1, 1)
            );
        }

        #endregion
    }

    /// <summary>
    /// 导航项组件
    /// </summary>
    public class NavItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private GameObject _redDot;
        [SerializeField] private GameObject _badge;
        [SerializeField] private TextMeshProUGUI _badgeText;

        [Header("样式")]
        [SerializeField] private Sprite _normalIcon;
        [SerializeField] private Sprite _selectedIcon;
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _selectedColor = Color.white;

        #endregion

        #region Fields

        private int _index = -1;
        private bool _isSelected = false;
        private bool _isInteractable = true;
        private NavItemData _data;

        // 事件
        public event Action<int> OnItemSelected;

        #endregion

        #region Properties

        public RectTransform RectTransform => transform as RectTransform;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 设置索引
        /// </summary>
        public void SetIndex(int index)
        {
            _index = index;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData(NavItemData data)
        {
            _data = data;

            if (_titleText != null)
            {
                _titleText.text = data.title;
            }

            if (_iconImage != null && data.iconNormal != null)
            {
                _iconImage.sprite = data.iconNormal;
            }

            ShowRedDot(data.showRedDot);
            SetBadge(data.badgeCount);
            SetInteractable(data.isInteractable);
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisual();
        }

        /// <summary>
        /// 显示/隐藏红点
        /// </summary>
        public void ShowRedDot(bool show)
        {
            if (_redDot != null)
            {
                _redDot.SetActive(show);
            }
        }

        /// <summary>
        /// 设置徽章数量
        /// </summary>
        public void SetBadge(int count)
        {
            if (_badge != null)
            {
                _badge.SetActive(count > 0);

                if (_badgeText != null)
                {
                    _badgeText.text = count > 99 ? "99+" : count.ToString();
                }
            }
        }

        /// <summary>
        /// 设置是否可交互
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;

            if (_button != null)
            {
                _button.interactable = interactable;
            }

            UpdateVisual();
        }

        #endregion

        #region Private Methods

        private void OnClick()
        {
            if (!_isInteractable || _isSelected) return;
            OnItemSelected?.Invoke(_index);
        }

        private void UpdateVisual()
        {
            // 更新图标
            if (_iconImage != null)
            {
                _iconImage.sprite = _isSelected ? _selectedIcon : _normalIcon;
                _iconImage.color = _isSelected ? _selectedColor : (_isInteractable ? _normalColor : _normalColor * 0.5f);
            }

            // 更新文字
            if (_titleText != null)
            {
                _titleText.color = _isSelected ? _selectedColor : (_isInteractable ? _normalColor : _normalColor * 0.5f);
            }
        }

        #endregion
    }
}
