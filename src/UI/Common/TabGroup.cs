// TabGroup.cs - 标签页组件
// 提供标签页切换功能

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using TMPro;

namespace JRPG.UI.Common
{
    /// <summary>
    /// 标签页按钮数据
    /// </summary>
    [Serializable]
    public class TabButtonData
    {
        public string id;
        public string title;
        public Sprite icon;
        public bool isInteractable = true;
        [HideInInspector] public int index;
    }

    /// <summary>
    /// 标签页状态改变事件
    /// </summary>
    [Serializable]
    public class TabChangedEvent : UnityEvent<int, int> { }

    /// <summary>
    /// 标签页按钮组件
    /// </summary>
    public class TabButton : MonoBehaviour
    {
        #region Fields

        [Header("UI引用")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;

        [Header("状态样式")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _disabledColor = Color.gray;
        [SerializeField] private Sprite _normalBg;
        [SerializeField] private Sprite _selectedBg;

        [Header("特效")]
        [SerializeField] private GameObject _selectedIndicator;
        [SerializeField] private GameObject _badge;

        // 状态
        private bool _isSelected = false;
        private bool _isInteractable = true;
        private TabGroup _parent;
        private int _tabIndex = -1;

        #endregion

        #region Properties

        public bool IsSelected => _isSelected;
        public bool IsInteractable => _isInteractable;
        public int TabIndex => _tabIndex;

        #endregion

        #region Events

        public event Action<TabButton> OnSelected;
        public event Action<TabButton> OnClicked;

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
        /// 初始化
        /// </summary>
        public void Initialize(TabGroup parent, int index)
        {
            _parent = parent;
            _tabIndex = index;
        }

        /// <summary>
        /// 设置是否可选
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;

            if (_button != null)
            {
                _button.interactable = interactable;
            }

            // 更新颜色
            UpdateVisual();
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected, bool immediate = false)
        {
            if (_isSelected == selected) return;

            _isSelected = selected;
            UpdateVisual();

            if (selected)
            {
                OnSelected?.Invoke(this);
            }
        }

        /// <summary>
        /// 设置标题
        /// </summary>
        public void SetTitle(string title)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
        }

        /// <summary>
        /// 设置图标
        /// </summary>
        public void SetIcon(Sprite icon)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.gameObject.SetActive(icon != null);
            }
        }

        /// <summary>
        /// 设置徽章
        /// </summary>
        public void SetBadge(int count, bool show = true)
        {
            if (_badge != null)
            {
                _badge.SetActive(show);
                var badgeText = _badge.GetComponentInChildren<TextMeshProUGUI>();
                if (badgeText != null)
                {
                    badgeText.text = count > 99 ? "99+" : count.ToString();
                }
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData(TabButtonData data)
        {
            if (data != null)
            {
                SetTitle(data.title);
                SetIcon(data.icon);
                SetInteractable(data.isInteractable);
            }
        }

        #endregion

        #region Private Methods

        private void OnClick()
        {
            if (!_isInteractable || _isSelected) return;

            _parent?.SelectTab(_tabIndex);
            OnClicked?.Invoke(this);
        }

        private void UpdateVisual()
        {
            // 更新颜色
            Color targetColor = GetTargetColor();

            if (_titleText != null)
            {
                _titleText.color = targetColor;
            }

            if (_iconImage != null)
            {
                _iconImage.color = targetColor;
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.sprite = _isSelected ? _selectedBg : _normalBg;
            }

            // 更新选中指示器
            if (_selectedIndicator != null)
            {
                _selectedIndicator.SetActive(_isSelected);
            }
        }

        private Color GetTargetColor()
        {
            if (!_isInteractable)
            {
                return _disabledColor;
            }

            return _isSelected ? _selectedColor : _normalColor;
        }

        #endregion
    }

    /// <summary>
    /// 标签页组件
    /// </summary>
    public class TabGroup : MonoBehaviour
    {
        #region Fields

        [Header("标签配置")]
        [SerializeField] private List<TabButton> _tabButtons = new List<TabButton>();
        [SerializeField] private TabButton _buttonPrefab;
        [SerializeField] private Transform _buttonContainer;

        [Header("内容面板")]
        [SerializeField] private List<GameObject> _contentPanels = new List<GameObject>();
        [SerializeField] private bool _hideInactiveContent = true;

        [Header("设置")]
        [SerializeField] private int _defaultIndex = 0;
        [SerializeField] private bool _allowSwitchSameTab = false;
        [SerializeField] private bool _autoSetup = false;

        [Header("动画")]
        [SerializeField] private bool _useAnimation = true;
        [SerializeField] private float _animDuration = 0.2f;

        // 状态
        private int _currentIndex = -1;
        private TabButton _selectedButton;

        #endregion

        #region Properties

        /// <summary>
        /// 当前选中的标签索引
        /// </summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// 当前选中的按钮
        /// </summary>
        public TabButton CurrentButton => _selectedButton;

        /// <summary>
        /// 标签数量
        /// </summary>
        public int TabCount => _tabButtons.Count;

        #endregion

        #region Events

        /// <summary>
        /// 标签切换事件 (旧索引, 新索引)
        /// </summary>
        public TabChangedEvent OnTabChanged = new TabChangedEvent();

        /// <summary>
        /// 标签点击事件
        /// </summary>
        public event Action<int> OnTabClicked;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_autoSetup)
            {
                AutoSetup();
            }
        }

        protected virtual void Start()
        {
            // 默认选中
            if (_currentIndex < 0 && _tabButtons.Count > 0)
            {
                SelectTab(_defaultIndex);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 自动设置（根据子对象自动生成）
        /// </summary>
        public void AutoSetup()
        {
            _tabButtons.Clear();
            TabButton[] buttons = GetComponentsInChildren<TabButton>(true);
            _tabButtons.AddRange(buttons);

            for (int i = 0; i < _tabButtons.Count; i++)
            {
                _tabButtons[i].Initialize(this, i);
            }
        }

        /// <summary>
        /// 添加标签
        /// </summary>
        public TabButton AddTab(string title, Sprite icon = null)
        {
            TabButton button;

            if (_buttonPrefab != null && _buttonContainer != null)
            {
                GameObject obj = Instantiate(_buttonPrefab.gameObject, _buttonContainer);
                button = obj.GetComponent<TabButton>();
            }
            else
            {
                GameObject obj = new GameObject($"Tab_{_tabButtons.Count}");
                obj.transform.SetParent(_buttonContainer);
                button = obj.AddComponent<TabButton>();
            }

            button.Initialize(this, _tabButtons.Count);
            button.SetTitle(title);
            button.SetIcon(icon);
            button.OnClicked += (btn) => OnTabClicked?.Invoke(btn.TabIndex);

            _tabButtons.Add(button);
            return button;
        }

        /// <summary>
        /// 移除标签
        /// </summary>
        public void RemoveTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Count) return;

            var button = _tabButtons[index];
            _tabButtons.RemoveAt(index);
            Destroy(button.gameObject);

            // 重新编号
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                _tabButtons[i].Initialize(this, i);
            }
        }

        /// <summary>
        /// 选择标签
        /// </summary>
        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Count) return;
            if (!_allowSwitchSameTab && index == _currentIndex) return;

            var button = _tabButtons[index];
            if (!button.IsInteractable) return;

            int oldIndex = _currentIndex;
            _currentIndex = index;
            _selectedButton = button;

            // 更新按钮状态
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                _tabButtons[i].SetSelected(i == index);
            }

            // 更新内容面板
            UpdateContent(index);

            // 触发事件
            OnTabChanged?.Invoke(oldIndex, index);
        }

        /// <summary>
        /// 获取标签按钮
        /// </summary>
        public TabButton GetTabButton(int index)
        {
            if (index < 0 || index >= _tabButtons.Count) return null;
            return _tabButtons[index];
        }

        /// <summary>
        /// 设置标签数据
        /// </summary>
        public void SetTabData(int index, TabButtonData data)
        {
            if (index < 0 || index >= _tabButtons.Count) return;
            _tabButtons[index].SetData(data);
        }

        /// <summary>
        /// 设置标签徽章
        /// </summary>
        public void SetTabBadge(int index, int count)
        {
            if (index < 0 || index >= _tabButtons.Count) return;
            _tabButtons[index].SetBadge(count, count > 0);
        }

        /// <summary>
        /// 启用/禁用标签
        /// </summary>
        public void SetTabInteractable(int index, bool interactable)
        {
            if (index < 0 || index >= _tabButtons.Count) return;
            _tabButtons[index].SetInteractable(interactable);
        }

        /// <summary>
        /// 添加内容面板
        /// </summary>
        public void AddContentPanel(GameObject panel)
        {
            _contentPanels.Add(panel);
        }

        /// <summary>
        /// 清空标签
        /// </summary>
        public void ClearTabs()
        {
            foreach (var button in _tabButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _tabButtons.Clear();
            _currentIndex = -1;
        }

        #endregion

        #region Private Methods

        private void UpdateContent(int index)
        {
            for (int i = 0; i < _contentPanels.Count; i++)
            {
                if (_contentPanels[i] == null) continue;

                if (_hideInactiveContent)
                {
                    bool show = i == index;
                    if (_useAnimation)
                    {
                        // 动画显示/隐藏
                        SetActiveWithAnimation(_contentPanels[i], show);
                    }
                    else
                    {
                        _contentPanels[i].SetActive(show);
                    }
                }
                else
                {
                    // 仅隐藏未选中的内容
                    _contentPanels[i].SetActive(i == index);
                }
            }
        }

        private void SetActiveWithAnimation(GameObject obj, bool active)
        {
            if (obj.activeSelf == active) return;

            if (active)
            {
                obj.SetActive(true);
                // 播放进入动画
                Core.UITweener.PopIn(obj.GetComponent<RectTransform>(), _animDuration);
            }
            else
            {
                // 播放退出动画
                Core.UITweener.PopOut(obj.GetComponent<RectTransform>(), _animDuration, () =>
                {
                    obj.SetActive(false);
                });
            }
        }

        #endregion
    }
}
