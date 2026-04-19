// SkillSelectPanel.cs - 技能选择面板
// 显示可用技能列表并允许玩家选择

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using JRPG.Character;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// 技能选择面板组件
    /// 显示技能列表，支持键盘/鼠标选择
    /// </summary>
    public class SkillSelectPanel : MonoBehaviour
    {
        #region UI References
        
        [Header("技能列表容器")]
        [SerializeField] private Transform _skillListContainer;
        [SerializeField] private GameObject _skillItemPrefab;
        
        [Header("技能信息显示")]
        [SerializeField] private TextMeshProUGUI _skillNameText;
        [SerializeField] private TextMeshProUGUI _skillDescriptionText;
        [SerializeField] private TextMeshProUGUI _skillCostText;
        [SerializeField] private TextMeshProUGUI _skillEffectText;
        
        [Header("分类标签")]
        [SerializeField] private ToggleGroup _categoryToggleGroup;
        [SerializeField] private Toggle _allToggle;
        [SerializeField] private Toggle _attackToggle;
        [SerializeField] private Toggle _defenseToggle;
        [SerializeField] private Toggle _supportToggle;
        
        [Header("分页")]
        [SerializeField] private Button _prevPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private TextMeshProUGUI _pageText;
        
        [Header("操作提示")]
        [SerializeField] private TextMeshProUGUI _hintText;
        
        #endregion
        
        #region Settings
        
        [Header("设置")]
        [SerializeField] private int _itemsPerPage = 5;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _unavailableColor = Color.gray;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 技能选中事件
        /// </summary>
        public event Action<BattleUnit, SkillData> OnSkillSelected;
        
        /// <summary>
        /// 面板取消事件
        /// </summary>
        public event Action OnPanelCancelled;
        
        #endregion
        
        #region Private Fields
        
        private BattleUnit _currentUnit;
        private List<SkillData> _allSkills = new List<SkillData>();
        private List<SkillData> _filteredSkills = new List<SkillData>();
        private List<SkillItem> _skillItems = new List<SkillItem>();
        
        private int _currentPage = 0;
        private int _selectedIndex = 0;
        private SkillCategory _currentCategory = SkillCategory.All;
        
        private enum SkillCategory
        {
            All,
            Attack,
            Defense,
            Support
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_prevPageButton != null)
                _prevPageButton.onClick.AddListener(PreviousPage);
            
            if (_nextPageButton != null)
                _nextPageButton.onClick.AddListener(NextPage);
        }
        
        private void Update()
        {
            if (!gameObject.activeSelf) return;
            
            HandleKeyboardInput();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化技能面板
        /// </summary>
        public void Initialize(BattleUnit unit)
        {
            _currentUnit = unit;
            _currentPage = 0;
            _selectedIndex = 0;
            
            // 获取所有技能
            _allSkills.Clear();
            _allSkills.AddRange(GetAvailableSkills(unit));
            
            // 设置分类切换事件
            SetupCategoryToggles();
            
            // 初始过滤
            FilterSkills();
            
            // 显示第一页
            ShowPage(0);
            
            UpdateHint();
        }
        
        /// <summary>
        /// 刷新技能列表
        /// </summary>
        public void Refresh()
        {
            if (_currentUnit != null)
            {
                Initialize(_currentUnit);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 获取可用技能列表
        /// </summary>
        private List<SkillData> GetAvailableSkills(BattleUnit unit)
        {
            var skills = new List<SkillData>();
            
            if (unit?.CharacterData?.activeSkills != null)
            {
                skills.AddRange(unit.CharacterData.activeSkills);
            }
            
            // 检查冷却和MP
            return skills.Where(s => CanUseSkill(unit, s)).ToList();
        }
        
        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        private bool CanUseSkill(BattleUnit unit, SkillData skill)
        {
            if (unit == null || skill == null) return false;
            
            // MP检查
            if (unit.CurrentMp < skill.mpCost) return false;
            
            // 冷却检查
            if (skill.cooldown > 0 && unit.GetSkillCooldown(skill) > 0) return false;
            
            return true;
        }
        
        /// <summary>
        /// 设置分类切换
        /// </summary>
        private void SetupCategoryToggles()
        {
            // 分类切换事件已通过Unity事件绑定
        }
        
        /// <summary>
        /// 过滤技能
        /// </summary>
        private void FilterSkills()
        {
            _filteredSkills.Clear();
            
            foreach (var skill in _allSkills)
            {
                bool include = _currentCategory switch
                {
                    SkillCategory.All => true,
                    SkillCategory.Attack => skill.damageType != DamageType.Heal && skill.effects.All(e => !(e is HealEffect)),
                    SkillCategory.Defense => skill.effects.Any(e => e is ShieldEffect || e is BuffEffect),
                    SkillCategory.Support => skill.damageType == DamageType.Heal || skill.effects.Any(e => e is HealEffect),
                    _ => true
                };
                
                if (include)
                    _filteredSkills.Add(skill);
            }
        }
        
        /// <summary>
        /// 显示指定页
        /// </summary>
        private void ShowPage(int pageIndex)
        {
            int totalPages = Mathf.CeilToInt((float)_filteredSkills.Count / _itemsPerPage);
            _currentPage = Mathf.Clamp(pageIndex, 0, Mathf.Max(0, totalPages - 1));
            
            // 清空现有技能项
            ClearSkillItems();
            
            // 计算页内技能
            int startIndex = _currentPage * _itemsPerPage;
            int endIndex = Mathf.Min(startIndex + _itemsPerPage, _filteredSkills.Count);
            
            for (int i = startIndex; i < endIndex; i++)
            {
                CreateSkillItem(_filteredSkills[i], i - startIndex);
            }
            
            // 更新页码显示
            if (_pageText != null)
            {
                _pageText.text = $"{_currentPage + 1}/{Mathf.Max(1, totalPages)}";
            }
            
            // 更新翻页按钮
            if (_prevPageButton != null)
                _prevPageButton.interactable = _currentPage > 0;
            if (_nextPageButton != null)
                _nextPageButton.interactable = _currentPage < totalPages - 1;
            
            // 默认选中第一项
            SelectItem(0);
        }
        
        /// <summary>
        /// 创建技能项
        /// </summary>
        private void CreateSkillItem(SkillData skill, int index)
        {
            if (_skillItemPrefab == null || _skillListContainer == null) return;
            
            var itemObj = Instantiate(_skillItemPrefab, _skillListContainer);
            var item = itemObj.GetComponent<SkillItem>();
            
            if (item != null)
            {
                bool canUse = _currentUnit != null && CanUseSkill(_currentUnit, skill);
                item.Initialize(skill, index, canUse, OnSkillItemClicked);
                _skillItems.Add(item);
            }
        }
        
        /// <summary>
        /// 清空技能项
        /// </summary>
        private void ClearSkillItems()
        {
            foreach (var item in _skillItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _skillItems.Clear();
        }
        
        /// <summary>
        /// 选中技能项
        /// </summary>
        private void SelectItem(int index)
        {
            if (index < 0 || index >= _skillItems.Count) return;
            
            _selectedIndex = index;
            
            // 更新选中状态
            for (int i = 0; i < _skillItems.Count; i++)
            {
                _skillItems[i].SetSelected(i == index);
            }
            
            // 更新技能信息显示
            var skill = _skillItems[index].Skill;
            UpdateSkillInfo(skill);
        }
        
        /// <summary>
        /// 更新技能信息显示
        /// </summary>
        private void UpdateSkillInfo(SkillData skill)
        {
            if (skill == null) return;
            
            if (_skillNameText != null)
                _skillNameText.text = skill.skillName;
            
            if (_skillDescriptionText != null)
                _skillDescriptionText.text = skill.description;
            
            if (_skillCostText != null)
                _skillCostText.text = $"MP: {skill.mpCost}";
            
            if (_skillEffectText != null)
            {
                string effectDesc = GetSkillEffectDescription(skill);
                _skillEffectText.text = effectDesc;
            }
        }
        
        /// <summary>
        /// 获取技能效果描述
        /// </summary>
        private string GetSkillEffectDescription(SkillData skill)
        {
            var desc = new System.Text.StringBuilder();
            
            if (skill.element != Element.None)
            {
                desc.Append($"[{skill.element}] ");
            }
            
            if (skill.damageType == DamageType.Heal)
            {
                desc.Append($"治疗 {skill.baseDamage}");
            }
            else
            {
                desc.Append($"伤害 x{skill.damageMultiplier:F1}");
            }
            
            if (skill.targetType == TargetType.AllEnemies || skill.targetType == TargetType.AllAllies)
            {
                desc.Append(" [全体]");
            }
            else if (skill.targetType == TargetType.RandomEnemy)
            {
                desc.Append($" [随机{skill.targetCount}]");
            }
            
            return desc.ToString();
        }
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            // 上选择
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                NavigateVertical(-1);
            }
            // 下选择
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                NavigateVertical(1);
            }
            // 左选择（可能用于翻页）
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
            {
                PreviousPage();
            }
            // 右选择
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.E))
            {
                NextPage();
            }
            // 确认选择
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                ConfirmSelection();
            }
            // 取消
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                CancelSelection();
            }
        }
        
        /// <summary>
        /// 垂直导航
        /// </summary>
        private void NavigateVertical(int direction)
        {
            int newIndex = _selectedIndex + direction;
            
            if (newIndex < 0)
                newIndex = _skillItems.Count - 1;
            else if (newIndex >= _skillItems.Count)
                newIndex = 0;
            
            SelectItem(newIndex);
        }
        
        /// <summary>
        /// 上一页
        /// </summary>
        public void PreviousPage()
        {
            int totalPages = Mathf.CeilToInt((float)_filteredSkills.Count / _itemsPerPage);
            if (_currentPage > 0)
            {
                ShowPage(_currentPage - 1);
            }
        }
        
        /// <summary>
        /// 下一页
        /// </summary>
        public void NextPage()
        {
            int totalPages = Mathf.CeilToInt((float)_filteredSkills.Count / _itemsPerPage);
            if (_currentPage < totalPages - 1)
            {
                ShowPage(_currentPage + 1);
            }
        }
        
        /// <summary>
        /// 确认选择
        /// </summary>
        private void ConfirmSelection()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _skillItems.Count) return;
            
            var skill = _skillItems[_selectedIndex].Skill;
            if (skill != null && _currentUnit != null)
            {
                OnSkillSelected?.Invoke(_currentUnit, skill);
            }
        }
        
        /// <summary>
        /// 取消选择
        /// </summary>
        private void CancelSelection()
        {
            OnPanelCancelled?.Invoke();
        }
        
        /// <summary>
        /// 更新提示文本
        /// </summary>
        private void UpdateHint()
        {
            if (_hintText != null)
            {
                _hintText.text = "↑↓选择技能 Z/Enter确认 X/Esc取消";
            }
        }
        
        /// <summary>
        /// 技能项点击回调
        /// </summary>
        private void OnSkillItemClicked(SkillItem item)
        {
            for (int i = 0; i < _skillItems.Count; i++)
            {
                if (_skillItems[i] == item)
                {
                    _selectedIndex = i;
                    SelectItem(i);
                    ConfirmSelection();
                    break;
                }
            }
        }
        
        #endregion
        
        #region Category Toggle Handlers
        
        public void OnAllToggleChanged(bool isOn)
        {
            if (isOn)
            {
                _currentCategory = SkillCategory.All;
                FilterSkills();
                ShowPage(0);
            }
        }
        
        public void OnAttackToggleChanged(bool isOn)
        {
            if (isOn)
            {
                _currentCategory = SkillCategory.Attack;
                FilterSkills();
                ShowPage(0);
            }
        }
        
        public void OnDefenseToggleChanged(bool isOn)
        {
            if (isOn)
            {
                _currentCategory = SkillCategory.Defense;
                FilterSkills();
                ShowPage(0);
            }
        }
        
        public void OnSupportToggleChanged(bool isOn)
        {
            if (isOn)
            {
                _currentCategory = SkillCategory.Support;
                FilterSkills();
                ShowPage(0);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 技能项组件
    /// </summary>
    public class SkillItem : MonoBehaviour
    {
        #region UI References
        
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Image _elementIcon;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _selectedIndicator;
        
        #endregion
        
        #region Properties
        
        public SkillData Skill { get; private set; }
        
        #endregion
        
        #region Private Fields
        
        private int _index;
        private bool _isAvailable;
        private System.Action<SkillItem> _onClick;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化技能项
        /// </summary>
        public void Initialize(SkillData skill, int index, bool available, System.Action<SkillItem> onClick)
        {
            Skill = skill;
            _index = index;
            _isAvailable = available;
            _onClick = onClick;
            
            UpdateDisplay();
            SetSelected(false);
        }
        
        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(selected);
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = selected ? 
                    new Color(0.3f, 0.5f, 1f, 0.3f) : 
                    new Color(0, 0, 0, 0.2f);
            }
        }
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateDisplay()
        {
            if (_nameText != null)
                _nameText.text = Skill?.skillName ?? "";
            
            if (_costText != null)
                _costText.text = Skill != null ? $"{Skill.mpCost}MP" : "";
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _isAvailable ? 
                    Color.white : 
                    new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
        
        /// <summary>
        /// 点击回调
        /// </summary>
        public void OnItemClicked()
        {
            if (_isAvailable && _onClick != null)
            {
                _onClick(this);
            }
        }
        
        #endregion
    }
}
