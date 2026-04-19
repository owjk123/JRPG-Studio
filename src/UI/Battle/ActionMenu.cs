// ActionMenu.cs - 行动菜单组件
// 显示攻击、技能、防御、道具等行动选项

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// 行动菜单组件
    /// 提供攻击、技能、防御、道具等行动选项
    /// </summary>
    public class ActionMenu : MonoBehaviour
    {
        #region UI References
        
        [Header("菜单项")]
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _defendButton;
        [SerializeField] private Button _itemButton;
        [SerializeField] private Button _ultimateButton;
        [SerializeField] private Button _fleeButton;
        
        [Header("菜单项文本")]
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _skillText;
        [SerializeField] private TextMeshProUGUI _defendText;
        [SerializeField] private TextMeshProUGUI _itemText;
        [SerializeField] private TextMeshProUGUI _ultimateText;
        [SerializeField] private TextMeshProUGUI _fleeText;
        
        [Header("消耗显示")]
        [SerializeField] private TextMeshProUGUI _skillCostText;
        [SerializeField] private TextMeshProUGUI _ultimateCostText;
        
        [Header("选中效果")]
        [SerializeField] private Image _selectionHighlight;
        [SerializeField] private float _selectionAnimationSpeed = 5f;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 攻击按钮点击事件
        /// </summary>
        public event Action<BattleUnit> OnAttackSelected;
        
        /// <summary>
        /// 技能按钮点击事件
        /// </summary>
        public event Action<BattleUnit> OnSkillSelected;
        
        /// <summary>
        /// 防御按钮点击事件
        /// </summary>
        public event Action<BattleUnit> OnDefendSelected;
        
        /// <summary>
        /// 道具按钮点击事件
        /// </summary>
        public event Action<BattleUnit> OnItemSelected;
        
        /// <summary>
        /// 终极技能按钮点击事件
        /// </summary>
        public event Action<BattleUnit> OnUltimateSelected;
        
        /// <summary>
        /// 逃跑按钮点击事件
        /// </summary>
        public event Action<BattleUnit> OnFleeSelected;
        
        /// <summary>
        /// 菜单取消事件
        /// </summary>
        public event Action OnMenuCancelled;
        
        #endregion
        
        #region Private Fields
        
        private BattleUnit _currentUnit;
        private Button[] _allButtons;
        private int _selectedIndex = 0;
        private bool _isKeyboardNavigation = true;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 当前选中的行动索引
        /// </summary>
        public int SelectedIndex => _selectedIndex;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _allButtons = new Button[] { _attackButton, _skillButton, _defendButton, _itemButton, _ultimateButton, _fleeButton };
        }
        
        private void Update()
        {
            if (!gameObject.activeSelf) return;
            
            HandleKeyboardInput();
            UpdateSelectionHighlight();
            UpdateButtonStates();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化行动菜单
        /// </summary>
        public void Initialize(BattleUnit unit)
        {
            _currentUnit = unit;
            _selectedIndex = 0;
            
            UpdateButtonTexts();
            UpdateCostDisplay();
            UpdateButtonStates();
            
            // 默认选中攻击
            SelectButton(_attackButton);
        }
        
        /// <summary>
        /// 启用/禁用菜单
        /// </summary>
        public void SetMenuEnabled(bool enabled)
        {
            foreach (var button in _allButtons)
            {
                if (button != null)
                    button.interactable = enabled;
            }
        }
        
        /// <summary>
        /// 刷新菜单状态
        /// </summary>
        public void Refresh()
        {
            if (_currentUnit != null)
            {
                Initialize(_currentUnit);
            }
        }
        
        /// <summary>
        /// 获取当前选中的行动类型
        /// </summary>
        public ActionType GetSelectedAction()
        {
            switch (_selectedIndex)
            {
                case 0: return ActionType.Attack;
                case 1: return ActionType.Skill;
                case 2: return ActionType.Defend;
                case 3: return ActionType.Item;
                case 4: return ActionType.Ultimate;
                case 5: return ActionType.Flee;
                default: return ActionType.Attack;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            // 上下选择
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                NavigateVertical(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                NavigateVertical(1);
            }
            
            // 左右选择
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                NavigateHorizontal(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                NavigateHorizontal(1);
            }
            
            // 确认选择
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {
                ConfirmSelection();
            }
            
            // 取消
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                CancelSelection();
            }
        }
        
        /// <summary>
        /// 垂直导航
        /// </summary>
        private void NavigateVertical(int direction)
        {
            int oldIndex = _selectedIndex;
            
            // 上下移动会影响行
            if (_selectedIndex < 3)
            {
                // 第一行
                if (direction > 0) _selectedIndex += 3; // 跳到第二行
            }
            else
            {
                // 第二行
                if (direction < 0) _selectedIndex -= 3; // 跳到第一行
            }
            
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _allButtons.Length - 1);
            
            if (oldIndex != _selectedIndex && _allButtons[_selectedIndex] != null)
            {
                SelectButton(_allButtons[_selectedIndex]);
            }
        }
        
        /// <summary>
        /// 水平导航
        /// </summary>
        private void NavigateHorizontal(int direction)
        {
            int oldIndex = _selectedIndex;
            
            if (_selectedIndex < 3)
            {
                // 第一行
                _selectedIndex += direction;
                if (_selectedIndex < 0) _selectedIndex = 0;
                if (_selectedIndex > 2) _selectedIndex = 2;
            }
            else
            {
                // 第二行
                _selectedIndex += direction;
                if (_selectedIndex < 3) _selectedIndex = 3;
                if (_selectedIndex > 5) _selectedIndex = 5;
            }
            
            if (oldIndex != _selectedIndex && _allButtons[_selectedIndex] != null)
            {
                SelectButton(_allButtons[_selectedIndex]);
            }
        }
        
        /// <summary>
        /// 确认选择
        /// </summary>
        private void ConfirmSelection()
        {
            Button selectedButton = _allButtons[_selectedIndex];
            if (selectedButton != null && selectedButton.interactable)
            {
                selectedButton.onClick.Invoke();
            }
        }
        
        /// <summary>
        /// 取消选择
        /// </summary>
        private void CancelSelection()
        {
            OnMenuCancelled?.Invoke();
        }
        
        /// <summary>
        /// 选中按钮
        /// </summary>
        private void SelectButton(Button button)
        {
            if (button == null) return;
            
            // 更新选择索引
            for (int i = 0; i < _allButtons.Length; i++)
            {
                if (_allButtons[i] == button)
                {
                    _selectedIndex = i;
                    break;
                }
            }
            
            // 播放音效
            // AudioManager.PlaySE("cursor_move");
        }
        
        /// <summary>
        /// 更新选择高亮
        /// </summary>
        private void UpdateSelectionHighlight()
        {
            if (_selectionHighlight == null) return;
            
            Button selectedButton = _allButtons[_selectedIndex];
            if (selectedButton == null) return;
            
            // 平滑移动高亮到选中按钮位置
            Vector3 targetPos = selectedButton.transform.position;
            _selectionHighlight.transform.position = Vector3.Lerp(
                _selectionHighlight.transform.position,
                targetPos,
                Time.deltaTime * _selectionAnimationSpeed
            );
        }
        
        /// <summary>
        /// 更新按钮文本
        /// </summary>
        private void UpdateButtonTexts()
        {
            if (_attackText != null) _attackText.text = "攻击";
            if (_skillText != null) _skillText.text = "技能";
            if (_defendText != null) _defendText.text = "防御";
            if (_itemText != null) _itemText.text = "道具";
            if (_ultimateText != null) _ultimateText.text = "终极";
            if (_fleeText != null) _fleeText.text = "逃跑";
        }
        
        /// <summary>
        /// 更新消耗显示
        /// </summary>
        private void UpdateCostDisplay()
        {
            if (_currentUnit == null) return;
            
            // 技能消耗
            if (_skillCostText != null)
            {
                var skills = _currentUnit.CharacterData.activeSkills;
                if (skills != null && skills.Count > 0)
                {
                    _skillCostText.text = $"MP: {skills[0].mpCost}";
                }
            }
            
            // 终极技能消耗
            if (_ultimateText != null && _currentUnit.CharacterData.ultimateSkill != null)
            {
                _ultimateCostText.text = $"能量: {_currentUnit.UltimateEnergy}/100";
            }
        }
        
        /// <summary>
        /// 更新按钮状态（可用/不可用）
        /// </summary>
        private void UpdateButtonStates()
        {
            if (_currentUnit == null) return;
            
            // 技能按钮
            if (_skillButton != null)
            {
                bool hasSkills = _currentUnit.CharacterData.activeSkills != null && 
                                  _currentUnit.CharacterData.activeSkills.Count > 0;
                bool hasEnoughMP = _currentUnit.CurrentMp >= GetMinSkillCost();
                bool notSilenced = !_currentUnit.HasStatus(StatusEffectType.Silence);
                
                _skillButton.interactable = hasSkills && hasEnoughMP && notSilenced;
            }
            
            // 终极技能按钮
            if (_ultimateButton != null)
            {
                bool hasUltimate = _currentUnit.CharacterData.ultimateSkill != null;
                bool hasEnoughEnergy = _currentUnit.UltimateEnergy >= 100;
                
                _ultimateButton.interactable = hasUltimate && hasEnoughEnergy;
            }
            
            // 道具按钮
            if (_itemButton != null)
            {
                // 检查是否有道具
                _itemButton.interactable = true; // TODO: 检查背包
            }
            
            // 逃跑按钮
            if (_fleeButton != null)
            {
                // 某些战斗不能逃跑
                _fleeButton.interactable = BattleManager.Instance?.CanFlee ?? true;
            }
        }
        
        /// <summary>
        /// 获取最低技能MP消耗
        /// </summary>
        private int GetMinSkillCost()
        {
            if (_currentUnit?.CharacterData?.activeSkills == null || 
                _currentUnit.CharacterData.activeSkills.Count == 0)
                return int.MaxValue;
            
            int minCost = int.MaxValue;
            foreach (var skill in _currentUnit.CharacterData.activeSkills)
            {
                if (skill.mpCost < minCost)
                    minCost = skill.mpCost;
            }
            return minCost;
        }
        
        #endregion
        
        #region Button Event Handlers
        
        /// <summary>
        /// 攻击按钮点击
        /// </summary>
        public void OnAttackClick()
        {
            OnAttackSelected?.Invoke(_currentUnit);
        }
        
        /// <summary>
        /// 技能按钮点击
        /// </summary>
        public void OnSkillClick()
        {
            OnSkillSelected?.Invoke(_currentUnit);
        }
        
        /// <summary>
        /// 防御按钮点击
        /// </summary>
        public void OnDefendClick()
        {
            OnDefendSelected?.Invoke(_currentUnit);
        }
        
        /// <summary>
        /// 道具按钮点击
        /// </summary>
        public void OnItemClick()
        {
            OnItemSelected?.Invoke(_currentUnit);
        }
        
        /// <summary>
        /// 终极技能按钮点击
        /// </summary>
        public void OnUltimateClick()
        {
            OnUltimateSelected?.Invoke(_currentUnit);
        }
        
        /// <summary>
        /// 逃跑按钮点击
        /// </summary>
        public void OnFleeClick()
        {
            OnFleeSelected?.Invoke(_currentUnit);
        }
        
        #endregion
    }
}
