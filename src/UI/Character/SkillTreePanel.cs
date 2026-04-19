// SkillTreePanel.cs - 技能树面板
// 展示和升级角色的技能

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
    /// 技能节点状态
    /// </summary>
    public enum SkillNodeState
    {
        Locked,      // 锁定
        Available,   // 可学习
        Learned,     // 已学习
        MaxLevel     // 已满级
    }

    /// <summary>
    /// 技能节点数据
    /// </summary>
    [Serializable]
    public class SkillNodeData
    {
        public int skillId;
        public string skillName;
        public string description;
        public int currentLevel;
        public int maxLevel;
        public SkillNodeState state;
        public Vector2 gridPosition;
        public List<int> prerequisiteSkillIds;
        public List<int> unlockSkillIds;
    }

    /// <summary>
    /// 技能树面板
    /// </summary>
    public class SkillTreePanel : BasePanel
    {
        #region UI References

        [Header("技能信息")]
        [SerializeField] private TextMeshProUGUI _skillNameText;
        [SerializeField] private TextMeshProUGUI _skillLevelText;
        [SerializeField] private TextMeshProUGUI _skillDescriptionText;
        [SerializeField] private Image _skillIcon;
        [SerializeField] private TextMeshProUGUI _skillCostText;

        [Header("技能树显示")]
        [SerializeField] private ScrollRect _skillTreeScrollRect;
        [SerializeField] private RectTransform _skillTreeContent;
        [SerializeField] private GridLayoutGroup _skillGrid;

        [Header("技能节点")]
        [SerializeField] private SkillNode _skillNodePrefab;
        [SerializeField] private List<SkillNode> _skillNodes = new List<SkillNode>();

        [Header("连接线")]
        [SerializeField] private LineRenderer _connectionLinePrefab;
        [SerializeField] private Transform _lineContainer;

        [Header("按钮")]
        [SerializeField] private Button _learnButton;
        [SerializeField] private Button _autoLearnButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private TextMeshProUGUI _learnCostText;

        [Header("消耗品")]
        [SerializeField] private Image _costIcon;
        [SerializeField] private TextMeshProUGUI _currentCostAmountText;
        [SerializeField] private TextMeshProUGUI _requiredCostAmountText;

        #endregion

        #region Fields

        private CharacterData _characterData;
        private List<SkillNodeData> _allSkillNodes = new List<SkillNodeData>();
        private SkillNodeData _selectedSkill;
        private Dictionary<int, SkillNode> _skillNodeMap = new Dictionary<int, SkillNode>();

        // 配置
        private bool _autoConnectLines = true;
        private int _skillPointsToUse = 1;

        // 事件
        public event Action<int, int> OnSkillLearnRequested; // skillId, currentLevel

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
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 初始化技能树结构
        }

        /// <summary>
        /// 设置角色
        /// </summary>
        public void SetCharacter(CharacterData data)
        {
            _characterData = data;
            LoadSkillTree();
            UpdateDisplay();
        }

        /// <summary>
        /// 刷新技能树
        /// </summary>
        public void Refresh()
        {
            UpdateSkillStates();
            UpdateSelectedSkillInfo();
        }

        /// <summary>
        /// 显示技能详情
        /// </summary>
        public void ShowSkillDetail(int skillId)
        {
            if (_skillNodeMap.TryGetValue(skillId, out var node))
            {
                SelectSkill(node.Data);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 加载技能树
        /// </summary>
        private void LoadSkillTree()
        {
            // 清空现有节点
            ClearSkillNodes();

            // 加载角色技能数据
            LoadSkillData();

            // 创建技能节点
            CreateSkillNodes();

            // 绘制连接线
            if (_autoConnectLines)
            {
                DrawConnectionLines();
            }

            // 更新状态
            UpdateSkillStates();
        }

        /// <summary>
        /// 加载技能数据
        /// </summary>
        private void LoadSkillData()
        {
            _allSkillNodes.Clear();

            // TODO: 从角色数据或技能配置加载
            // 模拟数据
            _allSkillNodes.Add(new SkillNodeData
            {
                skillId = 1,
                skillName = "普通攻击",
                description = "对单个敌人造成100%攻击力的伤害",
                currentLevel = 1,
                maxLevel = 10,
                state = SkillNodeState.Learned,
                gridPosition = new Vector2(0, 0),
                prerequisiteSkillIds = new List<int>(),
                unlockSkillIds = new List<int> { 2, 3 }
            });

            _allSkillNodes.Add(new SkillNodeData
            {
                skillId = 2,
                skillName = "强力打击",
                description = "对单个敌人造成150%攻击力的伤害，有20%几率造成眩晕",
                currentLevel = 3,
                maxLevel = 10,
                state = SkillNodeState.Learned,
                gridPosition = new Vector2(-1, 1),
                prerequisiteSkillIds = new List<int> { 1 },
                unlockSkillIds = new List<int> { 4 }
            });

            _allSkillNodes.Add(new SkillNodeData
            {
                skillId = 3,
                skillName = "连击",
                description = "连续攻击敌人2次，每次造成80%攻击力的伤害",
                currentLevel = 5,
                maxLevel = 10,
                state = SkillNodeState.Learned,
                gridPosition = new Vector2(1, 1),
                prerequisiteSkillIds = new List<int> { 1 },
                unlockSkillIds = new List<int> { 5 }
            });

            _allSkillNodes.Add(new SkillNodeData
            {
                skillId = 4,
                skillName = "毁灭打击",
                description = "对单个敌人造成300%攻击力的伤害，冷却时间增加",
                currentLevel = 0,
                maxLevel = 5,
                state = SkillNodeState.Available,
                gridPosition = new Vector2(-1, 2),
                prerequisiteSkillIds = new List<int> { 2 },
                unlockSkillIds = new List<int>()
            });

            _allSkillNodes.Add(new SkillNodeData
            {
                skillId = 5,
                skillName = "疾风连击",
                description = "连续攻击敌人3次，每次造成60%攻击力的伤害",
                currentLevel = 0,
                maxLevel = 5,
                state = SkillNodeState.Available,
                gridPosition = new Vector2(1, 2),
                prerequisiteSkillIds = new List<int> { 3 },
                unlockSkillIds = new List<int>()
            });

            _allSkillNodes.Add(new SkillNodeData
            {
                skillId = 6,
                skillName = "被动技能",
                description = "永久提升10%攻击力",
                currentLevel = 0,
                maxLevel = 5,
                state = SkillNodeState.Locked,
                gridPosition = new Vector2(0, 1),
                prerequisiteSkillIds = new List<int>(),
                unlockSkillIds = new List<int>()
            });
        }

        /// <summary>
        /// 创建技能节点
        /// </summary>
        private void CreateSkillNodes()
        {
            foreach (var skillData in _allSkillNodes)
            {
                CreateSkillNode(skillData);
            }
        }

        /// <summary>
        /// 创建单个技能节点
        /// </summary>
        private void CreateSkillNode(SkillNodeData data)
        {
            SkillNode node;

            if (_skillNodePrefab != null)
            {
                var nodeObj = Instantiate(_skillNodePrefab.gameObject, _skillGrid?.transform);
                node = nodeObj.GetComponent<SkillNode>();
            }
            else
            {
                var nodeObj = new GameObject($"SkillNode_{data.skillId}");
                nodeObj.transform.SetParent(_skillGrid?.transform);
                node = nodeObj.AddComponent<SkillNode>();
            }

            node.Initialize(data);
            node.OnNodeClicked += OnSkillNodeClicked;
            node.OnNodeHovered += OnSkillNodeHovered;
            node.OnNodeUnhovered += OnSkillNodeUnhovered;

            _skillNodes.Add(node);
            _skillNodeMap[data.skillId] = node;
        }

        /// <summary>
        /// 绘制连接线
        /// </summary>
        private void DrawConnectionLines()
        {
            if (_lineContainer == null) return;

            foreach (var skillData in _allSkillNodes)
            {
                // 绘制到前置技能的线
                if (skillData.prerequisiteSkillIds != null)
                {
                    foreach (var prereqId in skillData.prerequisiteSkillIds)
                    {
                        if (_skillNodeMap.TryGetValue(prereqId, out var prereqNode) &&
                            _skillNodeMap.TryGetValue(skillData.skillId, out var currentNode))
                        {
                            DrawLine(prereqNode.transform, currentNode.transform);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制两点之间的线
        /// </summary>
        private void DrawLine(Transform from, Transform to)
        {
            var lineObj = Instantiate(_connectionLinePrefab.gameObject, _lineContainer);
            var line = lineObj.GetComponent<LineRenderer>();

            if (line != null)
            {
                line.positionCount = 2;
                line.SetPosition(0, from.position);
                line.SetPosition(1, to.position);
            }
        }

        /// <summary>
        /// 清空技能节点
        /// </summary>
        private void ClearSkillNodes()
        {
            foreach (var node in _skillNodes)
            {
                if (node != null)
                {
                    node.OnNodeClicked -= OnSkillNodeClicked;
                    node.OnNodeHovered -= OnSkillNodeHovered;
                    node.OnNodeUnhovered -= OnSkillNodeUnhovered;
                    Destroy(node.gameObject);
                }
            }
            _skillNodes.Clear();
            _skillNodeMap.Clear();

            // 清空连接线
            if (_lineContainer != null)
            {
                foreach (Transform child in _lineContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateSkillStates();
            UpdateSelectedSkillInfo();
        }

        /// <summary>
        /// 更新技能状态
        /// </summary>
        private void UpdateSkillStates()
        {
            foreach (var node in _skillNodes)
            {
                UpdateNodeState(node);
            }
        }

        /// <summary>
        /// 更新节点状态
        /// </summary>
        private void UpdateNodeState(SkillNode node)
        {
            var data = node.Data;

            // 判断是否可以解锁
            SkillNodeState newState = GetNodeState(data);

            // 更新状态
            if (data.state != newState)
            {
                data.state = newState;
                node.SetState(newState);
            }

            // 更新显示
            node.UpdateDisplay();
        }

        /// <summary>
        /// 获取节点状态
        /// </summary>
        private SkillNodeState GetNodeState(SkillNodeData data)
        {
            // 已满级
            if (data.currentLevel >= data.maxLevel)
            {
                return SkillNodeState.MaxLevel;
            }

            // 已学习
            if (data.currentLevel > 0)
            {
                return SkillNodeState.Learned;
            }

            // 检查前置技能是否满足
            bool prerequisitesMet = true;
            foreach (var prereqId in data.prerequisiteSkillIds)
            {
                var prereqData = _allSkillNodes.Find(s => s.skillId == prereqId);
                if (prereqData == null || prereqData.currentLevel == 0)
                {
                    prerequisitesMet = false;
                    break;
                }
            }

            return prerequisitesMet ? SkillNodeState.Available : SkillNodeState.Locked;
        }

        /// <summary>
        /// 更新选中技能信息
        /// </summary>
        private void UpdateSelectedSkillInfo()
        {
            if (_selectedSkill == null)
            {
                ClearSkillInfo();
                return;
            }

            if (_skillNameText != null)
            {
                _skillNameText.text = _selectedSkill.skillName;
            }

            if (_skillLevelText != null)
            {
                _skillLevelText.text = $"Lv.{_selectedSkill.currentLevel}/{_selectedSkill.maxLevel}";
            }

            if (_skillDescriptionText != null)
            {
                _skillDescriptionText.text = _selectedSkill.description;
            }

            // 更新学习按钮状态
            UpdateLearnButton();
        }

        /// <summary>
        /// 清空技能信息
        /// </summary>
        private void ClearSkillInfo()
        {
            if (_skillNameText != null) _skillNameText.text = "";
            if (_skillLevelText != null) _skillLevelText.text = "";
            if (_skillDescriptionText != null) _skillDescriptionText.text = "";
            if (_learnButton != null) _learnButton.interactable = false;
        }

        /// <summary>
        /// 更新学习按钮
        /// </summary>
        private void UpdateLearnButton()
        {
            if (_learnButton == null) return;

            bool canLearn = _selectedSkill != null &&
                           (_selectedSkill.state == SkillNodeState.Available ||
                            _selectedSkill.state == SkillNodeState.Learned);

            _learnButton.interactable = canLearn;

            if (_learnCostText != null)
            {
                // 计算学习消耗
                int cost = CalculateLearnCost(_selectedSkill);
                _learnCostText.text = cost.ToString();
            }
        }

        /// <summary>
        /// 计算学习消耗
        /// </summary>
        private int CalculateLearnCost(SkillNodeData data)
        {
            if (data == null) return 0;
            return 100 * (data.currentLevel + 1); // 模拟消耗
        }

        #endregion

        #region Event Handlers

        private void OnSkillNodeClicked(SkillNode node)
        {
            SelectSkill(node.Data);
        }

        private void OnSkillNodeHovered(SkillNode node)
        {
            // 显示提示
        }

        private void OnSkillNodeUnhovered(SkillNode node)
        {
            // 隐藏提示
        }

        private void SelectSkill(SkillNodeData data)
        {
            // 取消之前的选中
            if (_selectedSkill != null && _skillNodeMap.TryGetValue(_selectedSkill.skillId, out var oldNode))
            {
                oldNode.SetSelected(false);
            }

            _selectedSkill = data;

            // 设置新的选中
            if (_selectedSkill != null && _skillNodeMap.TryGetValue(_selectedSkill.skillId, out var newNode))
            {
                newNode.SetSelected(true);
            }

            UpdateSelectedSkillInfo();
        }

        private void SetupButtons()
        {
            if (_learnButton != null)
            {
                _learnButton.onClick.RemoveAllListeners();
                _learnButton.onClick.AddListener(OnLearnClick);
            }

            if (_autoLearnButton != null)
            {
                _autoLearnButton.onClick.RemoveAllListeners();
                _autoLearnButton.onClick.AddListener(OnAutoLearnClick);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
                _resetButton.onClick.AddListener(OnResetClick);
            }
        }

        private void OnLearnClick()
        {
            if (_selectedSkill == null) return;

            // 检查资源是否足够
            // TODO: 检查技能点或消耗品

            OnSkillLearnRequested?.Invoke(_selectedSkill.skillId, _selectedSkill.currentLevel);

            // 升级技能
            _selectedSkill.currentLevel++;
            UpdateNodeState(_skillNodeMap[_selectedSkill.skillId]);
            UpdateSelectedSkillInfo();
        }

        private void OnAutoLearnClick()
        {
            // 自动学习所有可用技能
            foreach (var node in _skillNodes)
            {
                if (node.Data.state == SkillNodeState.Available ||
                    node.Data.state == SkillNodeState.Learned)
                {
                    // TODO: 检查资源
                    SelectSkill(node.Data);
                    OnLearnClick();
                }
            }
        }

        private void OnResetClick()
        {
            // 重置所有技能（需要消耗道具）
            Debug.Log("[SkillTree] 重置技能树");
        }

        #endregion
    }

    /// <summary>
    /// 技能节点组件
    /// </summary>
    public class SkillNode : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _levelBadge;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private GameObject _selectedIndicator;

        [Header("状态颜色")]
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _availableColor = Color.yellow;
        [SerializeField] private Color _learnedColor = Color.green;
        [SerializeField] private Color _maxLevelColor = Color.cyan;

        [Header("交互")]
        [SerializeField] private Button _nodeButton;

        #endregion

        #region Fields

        private SkillNodeData _data;
        private bool _isSelected = false;

        // 事件
        public event Action<SkillNode> OnNodeClicked;
        public event Action<SkillNode> OnNodeHovered;
        public event Action<SkillNode> OnNodeUnhovered;

        #endregion

        #region Properties

        public SkillNodeData Data => _data;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_nodeButton != null)
            {
                _nodeButton.onClick.RemoveAllListeners();
                _nodeButton.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(SkillNodeData data)
        {
            _data = data;
            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            if (_levelText != null)
            {
                _levelText.text = _data.currentLevel > 0 ? $"+{_data.currentLevel}" : "";
            }

            SetState(_data.state);
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        public void SetState(SkillNodeState state)
        {
            _data.state = state;

            if (_backgroundImage != null)
            {
                switch (state)
                {
                    case SkillNodeState.Locked:
                        _backgroundImage.color = _lockedColor;
                        break;
                    case SkillNodeState.Available:
                        _backgroundImage.color = _availableColor;
                        break;
                    case SkillNodeState.Learned:
                        _backgroundImage.color = _learnedColor;
                        break;
                    case SkillNodeState.MaxLevel:
                        _backgroundImage.color = _maxLevelColor;
                        break;
                }
            }

            if (_lockedOverlay != null)
            {
                _lockedOverlay.SetActive(state == SkillNodeState.Locked);
            }
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (_selectedIndicator != null)
            {
                _selectedIndicator.SetActive(selected);
            }

            if (selected)
            {
                UITweener.BounceIn(GetComponent<RectTransform>());
            }
        }

        #endregion

        #region Private Methods

        private void OnClick()
        {
            OnNodeClicked?.Invoke(this);
        }

        #endregion
    }
}
