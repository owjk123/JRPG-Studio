// BreakthroughPanel.cs - 突破界面组件
// 角色的突破系统

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
    /// 突破阶段数据
    /// </summary>
    [Serializable]
    public class BreakthroughStageData
    {
        public int stage;              // 突破阶段 (1-6)
        public string name;            // 阶段名称
        public int maxLevel;          // 该阶段最大等级
        public List<MaterialRequirement> materials; // 所需材料
        public List<StatBonus> statBonuses;         // 属性加成
        public bool isUnlocked;       // 是否解锁
        public bool isCompleted;       // 是否已完成
    }

    /// <summary>
    /// 材料需求
    /// </summary>
    [Serializable]
    public class MaterialRequirement
    {
        public int itemId;
        public string itemName;
        public int requiredCount;
        public int ownedCount;
        public Sprite icon;
    }

    /// <summary>
    /// 属性加成
    /// </summary>
    [Serializable]
    public class StatBonus
    {
        public string statName;
        public int bonusValue;
        public float percentBonus;
    }

    /// <summary>
    /// 突破面板组件
    /// </summary>
    public class BreakthroughPanel : BasePanel
    {
        #region UI References

        [Header("当前突破阶段")]
        [SerializeField] private Image _currentStageIcon;
        [SerializeField] private TextMeshProUGUI _currentStageText;
        [SerializeField] private TextMeshProUGUI _currentLevelText;
        [SerializeField] private ProgressBar _breakthroughProgressBar;

        [Header("突破阶段列表")]
        [SerializeField] private ScrollRect _stageScrollRect;
        [SerializeField] private Transform _stageContainer;
        [SerializeField] private BreakthroughStageItem _stageItemPrefab;
        [SerializeField] private List<BreakthroughStageItem> _stageItems = new List<BreakthroughStageItem>();

        [Header("突破详情")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TextMeshProUGUI _detailStageName;
        [SerializeField] private TextMeshProUGUI _detailLevelRange;
        [SerializeField] private Transform _materialContainer;
        [SerializeField] private Transform _bonusContainer;
        [SerializeField] private TextMeshProUGUI _detailDescText;

        [Header("按钮")]
        [SerializeField] private Button _breakthroughButton;
        [SerializeField] private TextMeshProUGUI _breakthroughCostText;
        [SerializeField] private Button _previewButton;

        [Header("突破动画")]
        [SerializeField] private GameObject _breakthroughEffect;
        [SerializeField] private AnimationCurve _effectAnimCurve;

        #endregion

        #region Fields

        private CharacterData _characterData;
        private List<BreakthroughStageData> _allStages = new List<BreakthroughStageData>();
        private BreakthroughStageData _selectedStage;
        private BreakthroughStageItem _selectedItem;
        private bool _isBreakthroughAnimating = false;

        // 配置
        private int _currentStage = 0;
        private int _currentLevel = 1;
        private int _maxLevel = 80;

        // 事件
        public event Action<BreakthroughStageData> OnBreakthroughRequested;

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
            LoadBreakthroughData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            CreateStageItems();
        }

        /// <summary>
        /// 设置角色
        /// </summary>
        public void SetCharacter(CharacterData data)
        {
            _characterData = data;
            UpdateFromCharacterData();
            UpdateDisplay();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            UpdateFromCharacterData();
            UpdateDisplay();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 从角色数据更新
        /// </summary>
        private void UpdateFromCharacterData()
        {
            if (_characterData == null) return;

            // TODO: 根据角色数据确定当前突破阶段和等级
            _currentStage = Mathf.Min(_characterData.level / 20, 5);
            _currentLevel = _characterData.level;

            // 更新阶段数据状态
            foreach (var stage in _allStages)
            {
                stage.isCompleted = stage.stage <= _currentStage;
                stage.isUnlocked = stage.stage <= _currentStage + 1;
            }
        }

        /// <summary>
        /// 加载突破数据
        /// </summary>
        private void LoadBreakthroughData()
        {
            _allStages.Clear();

            // 创建6个突破阶段
            for (int i = 1; i <= 6; i++)
            {
                var stageData = new BreakthroughStageData
                {
                    stage = i,
                    name = GetStageName(i),
                    maxLevel = i * 20,
                    materials = CreateStageMaterials(i),
                    statBonuses = CreateStageBonuses(i),
                    isUnlocked = i <= _currentStage + 1,
                    isCompleted = i <= _currentStage
                };

                _allStages.Add(stageData);
            }
        }

        /// <summary>
        /// 创建阶段材料需求
        /// </summary>
        private List<MaterialRequirement> CreateStageMaterials(int stage)
        {
            var materials = new List<MaterialRequirement>();

            // TODO: 从配置表加载实际材料需求
            for (int i = 0; i < 3; i++)
            {
                materials.Add(new MaterialRequirement
                {
                    itemId = 1000 + stage * 10 + i,
                    itemName = $"突破材料{stage}-{i + 1}",
                    requiredCount = 10 + stage * 5,
                    ownedCount = UnityEngine.Random.Range(0, 20)
                });
            }

            return materials;
        }

        /// <summary>
        /// 创建阶段属性加成
        /// </summary>
        private List<StatBonus> CreateStageBonuses(int stage)
        {
            var bonuses = new List<StatBonus>();

            bonuses.Add(new StatBonus { statName = "生命", bonusValue = 1000 * stage });
            bonuses.Add(new StatBonus { statName = "攻击", bonusValue = 100 * stage });
            bonuses.Add(new StatBonus { statName = "防御", bonusValue = 50 * stage });

            return bonuses;
        }

        /// <summary>
        /// 获取阶段名称
        /// </summary>
        private string GetStageName(int stage)
        {
            string[] names = { "", "+1", "+2", "+3", "+4", "+5", "+6" };
            return names[Mathf.Clamp(stage, 0, names.Length - 1)];
        }

        /// <summary>
        /// 创建阶段条目
        /// </summary>
        private void CreateStageItems()
        {
            // 清空现有条目
            ClearStageItems();

            foreach (var stageData in _allStages)
            {
                BreakthroughStageItem item;

                if (_stageItemPrefab != null)
                {
                    var itemObj = Instantiate(_stageItemPrefab.gameObject, _stageContainer);
                    item = itemObj.GetComponent<BreakthroughStageItem>();
                }
                else
                {
                    var itemObj = new GameObject($"Stage_{stageData.stage}");
                    itemObj.transform.SetParent(_stageContainer);
                    item = itemObj.AddComponent<BreakthroughStageItem>();
                }

                item.Initialize(stageData);
                item.OnItemClicked += OnStageClicked;
                _stageItems.Add(item);
            }
        }

        /// <summary>
        /// 清空阶段条目
        /// </summary>
        private void ClearStageItems()
        {
            foreach (var item in _stageItems)
            {
                if (item != null)
                {
                    item.OnItemClicked -= OnStageClicked;
                    Destroy(item.gameObject);
                }
            }
            _stageItems.Clear();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateCurrentStageDisplay();
            UpdateStageItemsDisplay();
            UpdateSelectedStageDisplay();
        }

        /// <summary>
        /// 更新当前突破阶段显示
        /// </summary>
        private void UpdateCurrentStageDisplay()
        {
            if (_currentStageText != null)
            {
                _currentStageText.text = $"突破 {GetStageName(_currentStage)}";
            }

            if (_currentLevelText != null)
            {
                int maxLevel = _allStages.Count > _currentStage - 1 ? _allStages[_currentStage - 1].maxLevel : _maxLevel;
                _currentLevelText.text = $"Lv.{_currentLevel}/{maxLevel}";
            }

            if (_breakthroughProgressBar != null)
            {
                int maxLevel = _allStages.Count > _currentStage - 1 ? _allStages[_currentStage - 1].maxLevel : _maxLevel;
                _breakthroughProgressBar.SetValue(_currentLevel);
                _breakthroughProgressBar.SetRange(0, maxLevel);
            }
        }

        /// <summary>
        /// 更新阶段条目显示
        /// </summary>
        private void UpdateStageItemsDisplay()
        {
            foreach (var item in _stageItems)
            {
                item.UpdateDisplay();
            }
        }

        /// <summary>
        /// 更新选中阶段显示
        /// </summary>
        private void UpdateSelectedStageDisplay()
        {
            if (_selectedStage == null)
            {
                // 默认选中当前阶段
                _selectedStage = _allStages.Count > _currentStage - 1 ? _allStages[_currentStage] : _allStages[0];
            }

            if (_detailStageName != null)
            {
                _detailStageName.text = $"突破 {_selectedStage.name}";
            }

            if (_detailLevelRange != null)
            {
                int maxLevel = _selectedStage.maxLevel;
                _detailLevelRange.text = $"角色等级上限: {maxLevel}";
            }

            // 更新材料显示
            UpdateMaterialsDisplay();

            // 更新加成显示
            UpdateBonusesDisplay();

            // 更新突破按钮
            UpdateBreakthroughButton();
        }

        /// <summary>
        /// 更新材料显示
        /// </summary>
        private void UpdateMaterialsDisplay()
        {
            if (_materialContainer == null) return;

            // 清空现有显示
            foreach (Transform child in _materialContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建材料显示
            foreach (var material in _selectedStage.materials)
            {
                CreateMaterialItem(material);
            }
        }

        /// <summary>
        /// 创建材料条目
        /// </summary>
        private void CreateMaterialItem(MaterialRequirement material)
        {
            var itemObj = new GameObject("MaterialItem");
            itemObj.transform.SetParent(_materialContainer);

            var layout = itemObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;

            // 图标
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemObj.transform);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.gray; // TODO: 加载实际图标

            // 数量文本
            var textObj = new GameObject("Count");
            textObj.transform.SetParent(itemObj.transform);
            var text = textObj.AddComponent<TextMeshProUGUI>();

            bool hasEnough = material.ownedCount >= material.requiredCount;
            text.text = $"{material.ownedCount}/{material.requiredCount}";
            text.color = hasEnough ? Color.green : Color.red;
        }

        /// <summary>
        /// 更新加成显示
        /// </summary>
        private void UpdateBonusesDisplay()
        {
            if (_bonusContainer == null) return;

            // 清空现有显示
            foreach (Transform child in _bonusContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建加成显示
            foreach (var bonus in _selectedStage.statBonuses)
            {
                CreateBonusItem(bonus);
            }
        }

        /// <summary>
        /// 创建加成条目
        /// </summary>
        private void CreateBonusItem(StatBonus bonus)
        {
            var itemObj = new GameObject("BonusItem");
            itemObj.transform.SetParent(_bonusContainer);

            var text = itemObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{bonus.statName} +{bonus.bonusValue}";
            text.fontSize = 24;
        }

        /// <summary>
        /// 更新突破按钮
        /// </summary>
        private void UpdateBreakthroughButton()
        {
            if (_breakthroughButton == null) return;

            // 检查是否可以突破
            bool canBreakthrough = CanBreakthrough();

            _breakthroughButton.interactable = canBreakthrough && !_isBreakthroughAnimating;

            if (_breakthroughCostText != null)
            {
                if (canBreakthrough)
                {
                    _breakthroughCostText.text = "突破!";
                }
                else if (_selectedStage.isCompleted)
                {
                    _breakthroughCostText.text = "已完成";
                }
                else
                {
                    _breakthroughCostText.text = "材料不足";
                }
            }
        }

        /// <summary>
        /// 检查是否可以突破
        /// </summary>
        private bool CanBreakthrough()
        {
            if (_selectedStage == null || _selectedStage.isCompleted)
                return false;

            if (_selectedStage.stage > _currentStage + 1)
                return false;

            // 检查材料是否足够
            foreach (var material in _selectedStage.materials)
            {
                if (material.ownedCount < material.requiredCount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_breakthroughButton != null)
            {
                _breakthroughButton.onClick.RemoveAllListeners();
                _breakthroughButton.onClick.AddListener(OnBreakthroughClick);
            }

            if (_previewButton != null)
            {
                _previewButton.onClick.RemoveAllListeners();
                _previewButton.onClick.AddListener(OnPreviewClick);
            }
        }

        #endregion

        #region Event Handlers

        private void OnStageClicked(BreakthroughStageItem item)
        {
            if (_selectedItem != null)
            {
                _selectedItem.SetSelected(false);
            }

            _selectedItem = item;
            _selectedItem.SetSelected(true);
            _selectedStage = item.Data;
            UpdateSelectedStageDisplay();
        }

        private void OnBreakthroughClick()
        {
            if (!CanBreakthrough() || _isBreakthroughAnimating) return;

            _isBreakthroughAnimating = true;

            // 发送突破请求
            OnBreakthroughRequested?.Invoke(_selectedStage);

            // 播放突破动画
            PlayBreakthroughAnimation();
        }

        private void OnPreviewClick()
        {
            // 预览下一阶段的加成
            Debug.Log("[BreakthroughPanel] 预览下一阶段");
        }

        /// <summary>
        /// 播放突破动画
        /// </summary>
        private void PlayBreakthroughAnimation()
        {
            if (_breakthroughEffect != null)
            {
                _breakthroughEffect.SetActive(true);

                // 动画结束后更新数据
                UITweener.TweenScale(
                    _breakthroughEffect.GetComponent<RectTransform>(),
                    Vector3.zero,
                    Vector3.one * 2f,
                    1f,
                    _effectAnimCurve,
                    () =>
                    {
                        // 完成突破
                        _selectedStage.isCompleted = true;
                        _currentStage = _selectedStage.stage;

                        // 隐藏特效
                        _breakthroughEffect.SetActive(false);

                        _isBreakthroughAnimating = false;
                        UpdateDisplay();
                    }
                );
            }
            else
            {
                _isBreakthroughAnimating = false;
            }
        }

        #endregion
    }

    /// <summary>
    /// 突破阶段条目组件
    /// </summary>
    public class BreakthroughStageItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Image _stageIcon;
        [SerializeField] private TextMeshProUGUI _stageNameText;
        [SerializeField] private Image _statusIcon;
        [SerializeField] private GameObject _completedIndicator;
        [SerializeField] private GameObject _currentIndicator;
        [SerializeField] private GameObject _lockedIndicator;
        [SerializeField] private Button _itemButton;

        [Header("状态图标")]
        [SerializeField] private Sprite _completedSprite;
        [SerializeField] private Sprite _currentSprite;
        [SerializeField] private Sprite _lockedSprite;

        #endregion

        #region Fields

        private BreakthroughStageData _data;
        private bool _isSelected = false;

        // 事件
        public event Action<BreakthroughStageItem> OnItemClicked;

        #endregion

        #region Properties

        public BreakthroughStageData Data => _data;

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
        public void Initialize(BreakthroughStageData data)
        {
            _data = data;
            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            if (_stageNameText != null)
            {
                _stageNameText.text = _data.name;
            }

            if (_completedIndicator != null)
            {
                _completedIndicator.SetActive(_data.isCompleted);
            }

            if (_currentIndicator != null)
            {
                _currentIndicator.SetActive(_data.isUnlocked && !_data.isCompleted);
            }

            if (_lockedIndicator != null)
            {
                _lockedIndicator.SetActive(!_data.isUnlocked);
            }

            if (_statusIcon != null)
            {
                if (_data.isCompleted)
                    _statusIcon.sprite = _completedSprite;
                else if (_data.isUnlocked)
                    _statusIcon.sprite = _currentSprite;
                else
                    _statusIcon.sprite = _lockedSprite;
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
            OnItemClicked?.Invoke(this);
        }

        #endregion
    }
}
