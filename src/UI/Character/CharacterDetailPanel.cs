// CharacterDetailPanel.cs - 角色详情面板
// 显示角色的详细信息、属性、技能等

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;
using JRPG.UI.Core;
using JRPG.Character;

namespace JRPG.UI.Character
{
    /// <summary>
    /// 角色详情面板
    /// </summary>
    public class CharacterDetailPanel : BasePanel
    {
        #region UI References

        [Header("顶部栏")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _shareButton;
        [SerializeField] private Button _favoriteButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("角色基础信息")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image _elementIcon;
        [SerializeField] private Image _rarityFrame;
        [SerializeField] private ProgressBar _expProgressBar;

        [Header("标签页")]
        [SerializeField] private TabGroup _tabGroup;
        [SerializeField] private int _statsTabIndex = 0;
        [SerializeField] private int _skillTabIndex = 1;
        [SerializeField] private int _equipmentTabIndex = 2;
        [SerializeField] private int _breakthroughTabIndex = 3;

        [Header("详情内容")]
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private GameObject _skillPanel;
        [SerializeField] private GameObject _equipmentPanel;
        [SerializeField] private GameObject _breakthroughPanel;

        [Header("功能按钮")]
        [SerializeField] private Button _levelUpButton;
        [SerializeField] private Button _breakthroughButton;
        [SerializeField] private Button _teamButton;
        [SerializeField] private TextMeshProUGUI _levelUpCostText;
        [SerializeField] private TextMeshProUGUI _breakthroughCostText;

        [Header("子组件")]
        [SerializeField] private CharacterStatsDisplay _statsDisplay;
        [SerializeField] private SkillTreePanel _skillTreePanel;
        [SerializeField] private EquipmentPanel _equipmentPanelComponent;
        [SerializeField] private BreakthroughPanel _breakthroughPanelComponent;

        #endregion

        #region Fields

        private CharacterData _characterData;
        private int _currentTabIndex = 0;

        // 事件
        public event Action<CharacterData> OnLevelUp;
        public event Action<CharacterData> OnBreakthrough;
        public event Action<CharacterData> OnAddToTeam;

        #endregion

        #region Properties

        /// <summary>
        /// 当前角色数据
        /// </summary>
        public CharacterData CurrentCharacter => _characterData;

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
            SetupTabs();
            SetupButtons();
        }

        protected override void OnPanelInit()
        {
            // 初始化子组件
            InitializeSubComponents();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 设置角色数据
        /// </summary>
        public void SetCharacter(CharacterData data)
        {
            _characterData = data;
            UpdateAllDisplay();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            if (_characterData != null)
            {
                UpdateAllDisplay();
            }
        }

        /// <summary>
        /// 刷新角色数据
        /// </summary>
        public void RefreshCharacterData()
        {
            // TODO: 从服务器刷新角色数据
            Refresh();
        }

        /// <summary>
        /// 显示升级动画
        /// </summary>
        public void ShowLevelUpAnimation(int newLevel)
        {
            if (_levelText != null)
            {
                // 数字滚动动画
                UITweener.CountUp(_levelText, _characterData.level, newLevel, 1f, "Lv.");
            }

            // 播放特效
            // TODO: 播放升级粒子特效
        }

        /// <summary>
        /// 显示突破动画
        /// </summary>
        public void ShowBreakthroughAnimation()
        {
            // 播放突破特效
            // TODO: 播放突破粒子特效
        }

        #endregion

        #region Protected Methods

        protected override void OnPanelDataSet(object data)
        {
            if (data is CharacterData characterData)
            {
                SetCharacter(characterData);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 更新所有显示
        /// </summary>
        private void UpdateAllDisplay()
        {
            if (_characterData == null) return;

            UpdateBasicInfo();
            UpdateButtonStates();
            UpdateSubComponents();
        }

        /// <summary>
        /// 更新基础信息
        /// </summary>
        private void UpdateBasicInfo()
        {
            if (_nameText != null)
            {
                _nameText.text = _characterData.name;
            }

            if (_titleText != null)
            {
                _titleText.text = _characterData.name;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{_characterData.level}";
            }

            if (_expProgressBar != null)
            {
                // TODO: 设置经验值
                // _expProgressBar.SetValue(_characterData.exp);
            }

            if (_rarityFrame != null)
            {
                // 设置稀有度边框
                _rarityFrame.color = GetRarityColor(_characterData.rarity);
            }

            if (_elementIcon != null)
            {
                // 设置元素图标
                // LoadElementIcon();
            }

            if (_avatarImage != null)
            {
                // 加载头像
                // LoadAvatar();
            }
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            if (_characterData == null) return;

            // 检查是否可以升级
            bool canLevelUp = CanLevelUp();
            if (_levelUpButton != null)
            {
                _levelUpButton.interactable = canLevelUp;
            }

            // 检查是否可以突破
            bool canBreakthrough = CanBreakthrough();
            if (_breakthroughButton != null)
            {
                _breakthroughButton.interactable = canBreakthrough;
            }
        }

        /// <summary>
        /// 更新子组件
        /// </summary>
        private void UpdateSubComponents()
        {
            if (_statsDisplay != null)
            {
                _statsDisplay.SetCharacter(_characterData);
            }

            if (_skillTreePanel != null)
            {
                _skillTreePanel.SetCharacter(_characterData);
            }

            if (_equipmentPanelComponent != null)
            {
                _equipmentPanelComponent.SetCharacter(_characterData);
            }

            if (_breakthroughPanelComponent != null)
            {
                _breakthroughPanelComponent.SetCharacter(_characterData);
            }
        }

        /// <summary>
        /// 初始化子组件
        /// </summary>
        private void InitializeSubComponents()
        {
            if (_statsDisplay != null)
            {
                _statsDisplay.Initialize();
            }

            if (_skillTreePanel != null)
            {
                _skillTreePanel.Initialize();
            }

            if (_equipmentPanelComponent != null)
            {
                _equipmentPanelComponent.Initialize();
            }

            if (_breakthroughPanelComponent != null)
            {
                _breakthroughPanelComponent.Initialize();
            }
        }

        /// <summary>
        /// 设置标签页
        /// </summary>
        private void SetupTabs()
        {
            if (_tabGroup != null)
            {
                _tabGroup.OnTabChanged.AddListener(OnTabChanged);
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

            if (_shareButton != null)
            {
                _shareButton.onClick.RemoveAllListeners();
                _shareButton.onClick.AddListener(OnShareClick);
            }

            if (_favoriteButton != null)
            {
                _favoriteButton.onClick.RemoveAllListeners();
                _favoriteButton.onClick.AddListener(OnFavoriteClick);
            }

            if (_levelUpButton != null)
            {
                _levelUpButton.onClick.RemoveAllListeners();
                _levelUpButton.onClick.AddListener(OnLevelUpClick);
            }

            if (_breakthroughButton != null)
            {
                _breakthroughButton.onClick.RemoveAllListeners();
                _breakthroughButton.onClick.AddListener(OnBreakthroughClick);
            }

            if (_teamButton != null)
            {
                _teamButton.onClick.RemoveAllListeners();
                _teamButton.onClick.AddListener(OnTeamClick);
            }
        }

        /// <summary>
        /// 检查是否可以升级
        /// </summary>
        private bool CanLevelUp()
        {
            // TODO: 检查经验和材料是否足够
            return _characterData.exp >= GetLevelUpExpRequirement();
        }

        /// <summary>
        /// 检查是否可以突破
        /// </summary>
        private bool CanBreakthrough()
        {
            // TODO: 检查突破材料是否足够
            return false;
        }

        /// <summary>
        /// 获取升级所需经验
        /// </summary>
        private long GetLevelUpExpRequirement()
        {
            // TODO: 根据等级计算所需经验
            return 1000 * _characterData.level;
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

        #endregion

        #region Event Handlers

        private void OnTabChanged(int oldIndex, int newIndex)
        {
            _currentTabIndex = newIndex;

            // 切换内容面板
            if (_statsPanel != null) _statsPanel.SetActive(newIndex == _statsTabIndex);
            if (_skillPanel != null) _skillPanel.SetActive(newIndex == _skillTabIndex);
            if (_equipmentPanel != null) _equipmentPanel.SetActive(newIndex == _equipmentTabIndex);
            if (_breakthroughPanel != null) _breakthroughPanel.SetActive(newIndex == _breakthroughTabIndex);
        }

        private void OnBackClick()
        {
            Hide();
        }

        private void OnShareClick()
        {
            Debug.Log("[CharacterDetail] 分享角色");
            // TODO: 实现分享功能
        }

        private void OnFavoriteClick()
        {
            if (_favoriteButton != null)
            {
                // TODO: 切换收藏状态
                Debug.Log("[CharacterDetail] 切换收藏");
            }
        }

        private void OnLevelUpClick()
        {
            if (CanLevelUp())
            {
                OnLevelUp?.Invoke(_characterData);
            }
        }

        private void OnBreakthroughClick()
        {
            if (CanBreakthrough())
            {
                OnBreakthrough?.Invoke(_characterData);
            }
        }

        private void OnTeamClick()
        {
            OnAddToTeam?.Invoke(_characterData);
        }

        #endregion
    }
}
