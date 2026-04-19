// TargetSelectArrow.cs - 目标选择指示器
// 管理目标选择时的箭头和选中高亮

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// 目标选择指示器组件
    /// 用于在战斗中显示目标选择箭头和高亮
    /// </summary>
    public class TargetSelectArrow : MonoBehaviour
    {
        #region UI References
        
        [Header("箭头指示器")]
        [SerializeField] private Image _arrowImage;
        [SerializeField] private Sprite _singleTargetSprite;    // 单体目标箭头
        [SerializeField] private Sprite _multiTargetSprite;     // 群体目标箭头
        [SerializeField] private float _arrowBobSpeed = 3f;     // 箭头浮动速度
        [SerializeField] private float _arrowBobHeight = 10f;   // 箭头浮动高度
        
        [Header("高亮效果")]
        [SerializeField] private Image _targetHighlightPrefab;
        [SerializeField] private Color _selectableColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.5f, 0f, 0.5f);
        [SerializeField] private Color _enemyColor = new Color(1f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _allyColor = new Color(0.3f, 0.6f, 1f, 0.3f);
        
        [Header("信息显示")]
        [SerializeField] private TextMeshProUGUI _targetNameText;
        [SerializeField] private TextMeshProUGUI _targetHPText;
        [SerializeField] private TextMeshProUGUI _targetStatusText;
        
        [Header("操作提示")]
        [SerializeField] private TextMeshProUGUI _hintText;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 目标选中事件
        /// </summary>
        public event Action<BattleUnit> OnTargetSelected;
        
        /// <summary>
        /// 取消选择事件
        /// </summary>
        public event Action OnSelectionCancelled;
        
        #endregion
        
        #region Private Fields
        
        private List<BattleUnit> _targets = new List<BattleUnit>();
        private List<Image> _highlightImages = new List<Image>();
        
        private BattleUnit _currentTarget;
        private int _selectedIndex = 0;
        private bool _isSelecting = false;
        
        private Vector3 _originalArrowPosition;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 当前选中的目标
        /// </summary>
        public BattleUnit CurrentTarget => _currentTarget;
        
        /// <summary>
        /// 当前选中索引
        /// </summary>
        public int SelectedIndex => _selectedIndex;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_arrowImage != null)
            {
                _originalArrowPosition = _arrowImage.transform.localPosition;
            }
        }
        
        private void Update()
        {
            if (!_isSelecting) return;
            
            HandleInput();
            UpdateArrowPosition();
            UpdateTargetInfo();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化目标选择
        /// </summary>
        public void Initialize(List<BattleUnit> targets, Action<BattleUnit> onSelected)
        {
            _targets = targets.Where(t => t != null && t.IsAlive).ToList();
            _selectedIndex = 0;
            _isSelecting = true;
            
            OnTargetSelected = onSelected;
            
            // 清除旧高亮
            ClearHighlights();
            
            // 创建新高亮
            CreateHighlights();
            
            // 更新箭头样式
            UpdateArrowStyle();
            
            // 选择第一个目标
            SelectTarget(_selectedIndex);
            
            // 更新提示
            UpdateHint();
            
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 隐藏目标选择
        /// </summary>
        public void Hide()
        {
            _isSelecting = false;
            ClearHighlights();
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 设置目标列表（动态更新）
        /// </summary>
        public void SetTargets(List<BattleUnit> targets)
        {
            // 清除旧数据
            ClearHighlights();
            
            _targets = targets.Where(t => t != null && t.IsAlive).ToList();
            _selectedIndex = 0;
            
            // 创建新高亮
            CreateHighlights();
            
            if (_targets.Count > 0)
            {
                SelectTarget(_selectedIndex);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 创建目标高亮
        /// </summary>
        private void CreateHighlights()
        {
            foreach (var target in _targets)
            {
                if (target == null) continue;
                
                // 创建高亮图片（可以附加到目标对象或UI层）
                var highlight = CreateHighlightForTarget(target);
                if (highlight != null)
                {
                    _highlightImages.Add(highlight);
                }
            }
        }
        
        /// <summary>
        /// 为目标创建高亮
        /// </summary>
        private Image CreateHighlightForTarget(BattleUnit target)
        {
            if (_targetHighlightPrefab == null) return null;
            
            // 在目标位置创建高亮
            var highlightObj = Instantiate(_targetHighlightPrefab, target.transform);
            var highlight = highlightObj.GetComponent<Image>();
            
            if (highlight != null)
            {
                // 设置颜色
                highlight.color = target.IsPlayerControlled ? _allyColor : _enemyColor;
                
                // 设置位置和大小
                var rect = highlight.rectTransform;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(80, 80); // 默认大小
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                
                // 添加选中效果组件
                var pulseEffect = highlightObj.AddComponent<TargetHighlightPulse>();
                pulseEffect.Initialize(_selectedColor);
            }
            
            return highlight;
        }
        
        /// <summary>
        /// 清除所有高亮
        /// </summary>
        private void ClearHighlights()
        {
            foreach (var highlight in _highlightImages)
            {
                if (highlight != null && highlight.gameObject != null)
                {
                    Destroy(highlight.gameObject);
                }
            }
            _highlightImages.Clear();
        }
        
        /// <summary>
        /// 更新箭头样式
        /// </summary>
        private void UpdateArrowStyle()
        {
            if (_arrowImage == null) return;
            
            // 根据目标数量选择箭头样式
            bool isMultiTarget = _targets.Count > 1;
            _arrowImage.sprite = isMultiTarget ? _multiTargetSprite : _singleTargetSprite;
        }
        
        /// <summary>
        /// 选择目标
        /// </summary>
        private void SelectTarget(int index)
        {
            if (index < 0 || index >= _targets.Count) return;
            
            _selectedIndex = index;
            _currentTarget = _targets[index];
            
            // 更新高亮颜色
            UpdateHighlightColors();
            
            // 更新箭头位置
            if (_arrowImage != null && _currentTarget != null)
            {
                _arrowImage.transform.position = _currentTarget.transform.position + Vector3.up * 60;
            }
        }
        
        /// <summary>
        /// 更新高亮颜色
        /// </summary>
        private void UpdateHighlightColors()
        {
            for (int i = 0; i < _highlightImages.Count && i < _targets.Count; i++)
            {
                var highlight = _highlightImages[i];
                if (highlight == null) continue;
                
                bool isSelected = (i == _selectedIndex);
                highlight.color = isSelected ? _selectedColor : 
                    (_targets[i].IsPlayerControlled ? _allyColor : _enemyColor);
            }
        }
        
        /// <summary>
        /// 更新箭头位置（浮动效果）
        /// </summary>
        private void UpdateArrowPosition()
        {
            if (_arrowImage == null || _currentTarget == null) return;
            
            // 上下浮动
            float offset = Mathf.Sin(Time.time * _arrowBobSpeed) * _arrowBobHeight;
            Vector3 basePos = _currentTarget.transform.position + Vector3.up * 60;
            _arrowImage.transform.position = basePos + Vector3.up * offset;
        }
        
        /// <summary>
        /// 更新目标信息显示
        /// </summary>
        private void UpdateTargetInfo()
        {
            if (_currentTarget == null) return;
            
            if (_targetNameText != null)
            {
                _targetNameText.text = _currentTarget.CharacterData.characterName;
            }
            
            if (_targetHPText != null)
            {
                var stats = _currentTarget.Stats;
                float hpPercent = (float)_currentTarget.CurrentHp / stats.MaxHp;
                _targetHPText.text = $"{_currentTarget.CurrentHp}/{stats.MaxHp} ({hpPercent:P0})";
            }
            
            if (_targetStatusText != null)
            {
                var statuses = _currentTarget.GetAllStatuses().Take(3).ToList();
                if (statuses.Count > 0)
                {
                    _targetStatusText.text = string.Join(", ", 
                        statuses.Select(s => s.StatusType.ToString()));
                }
                else
                {
                    _targetStatusText.text = "";
                }
            }
        }
        
        /// <summary>
        /// 更新操作提示
        /// </summary>
        private void UpdateHint()
        {
            if (_hintText != null)
            {
                string hint = "←→选择目标 ";
                if (_targets.Count > 1)
                {
                    hint += $"({_selectedIndex + 1}/{_targets.Count}) ";
                }
                hint += "Z/Enter确认 X/Esc取消";
                _hintText.text = hint;
            }
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            // 切换目标
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
            {
                Navigate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.E))
            {
                Navigate(1);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                Navigate(-GetColumnCount());
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                Navigate(GetColumnCount());
            }
            // Tab切换
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                Navigate(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
            }
            // 确认
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
        /// 导航到目标
        /// </summary>
        private void Navigate(int delta)
        {
            if (_targets.Count == 0) return;
            
            int newIndex = _selectedIndex + delta;
            
            // 循环选择
            if (newIndex < 0)
                newIndex = _targets.Count - 1;
            else if (newIndex >= _targets.Count)
                newIndex = 0;
            
            SelectTarget(newIndex);
            
            // 播放音效
            // AudioManager.PlaySE("cursor");
        }
        
        /// <summary>
        /// 获取列数（用于上下导航）
        /// </summary>
        private int GetColumnCount()
        {
            // 默认1列，可以根据UI布局调整
            return 1;
        }
        
        /// <summary>
        /// 确认选择
        /// </summary>
        private void ConfirmSelection()
        {
            if (_currentTarget != null)
            {
                OnTargetSelected?.Invoke(_currentTarget);
                Hide();
            }
        }
        
        /// <summary>
        /// 取消选择
        /// </summary>
        private void CancelSelection()
        {
            OnSelectionCancelled?.Invoke();
            Hide();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 目标高亮脉冲效果
    /// </summary>
    public class TargetHighlightPulse : MonoBehaviour
    {
        [SerializeField] private Color _pulseColor;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _minAlpha = 0.3f;
        [SerializeField] private float _maxAlpha = 0.8f;
        
        private Image _image;
        private Color _originalColor;
        private bool _isPulsing = false;
        
        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null)
            {
                _originalColor = _image.color;
            }
        }
        
        public void Initialize(Color pulseColor)
        {
            _pulseColor = pulseColor;
            _isPulsing = true;
        }
        
        private void Update()
        {
            if (!_isPulsing || _image == null) return;
            
            float t = (Mathf.Sin(Time.time * _pulseSpeed * Mathf.PI * 2) + 1f) / 2f;
            float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);
            
            _image.color = new Color(_pulseColor.r, _pulseColor.g, _pulseColor.b, alpha);
        }
        
        public void StopPulse()
        {
            _isPulsing = false;
            if (_image != null)
            {
                _image.color = _originalColor;
            }
        }
    }
}
