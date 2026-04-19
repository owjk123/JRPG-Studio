// ATBBar.cs - ATB行动条UI组件
// 显示和管理单个角色的ATB进度条

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JRPG.Battle.ATB;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// ATB行动条UI组件
    /// 显示角色头像、ATB进度条、状态图标等
    /// </summary>
    public class ATBBar : MonoBehaviour
    {
        #region UI References
        
        [Header("UI组件")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private Image _atbFillImage;
        [SerializeField] private Image _atbBackgroundImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _mpText;
        
        [Header("血条组件")]
        [SerializeField] private Image _hpFillImage;
        [SerializeField] private Image _hpBackgroundImage;
        
        [Header("MP条组件")]
        [SerializeField] private Image _mpFillImage;
        [SerializeField] private Image _mpBackgroundImage;
        
        [Header("状态图标")]
        [SerializeField] private Transform _statusIconContainer;
        [SerializeField] private GameObject _statusIconPrefab;
        
        [Header("特殊状态指示")]
        [SerializeField] private Image _readyIndicator;      // ATB满时的闪烁指示
        [SerializeField] private Image _actionIndicator;      // 正在行动指示
        [SerializeField] private GameObject _deadOverlay;     // 死亡遮罩
        
        #endregion
        
        #region Settings
        
        [Header("颜色设置")]
        [SerializeField] private Color _playerColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color _enemyColor = new Color(1f, 0.4f, 0.3f);
        [SerializeField] private Color _readyColor = new Color(1f, 0.9f, 0.3f);
        [SerializeField] private Color _deadColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        
        [Header("动画设置")]
        [SerializeField] private float _readyPulseSpeed = 2f;
        [SerializeField] private float _damageShakeIntensity = 5f;
        [SerializeField] private float _damageShakeDuration = 0.3f;
        
        #endregion
        
        #region Private Fields
        
        private BattleUnit _boundUnit;
        private ATBController _atbController;
        private RectTransform _rectTransform;
        private Vector3 _originalPosition;
        
        private float _readyPulseTimer = 0f;
        private bool _isReady = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 绑定的战斗单位
        /// </summary>
        public BattleUnit BoundUnit => _boundUnit;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.localPosition;
            _atbController = ATBController.Instance;
        }
        
        private void Update()
        {
            if (_boundUnit == null) return;
            
            UpdateATBBar();
            UpdateReadyIndicator();
            UpdateStatusDisplay();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 绑定战斗单位
        /// </summary>
        public void BindUnit(BattleUnit unit)
        {
            if (unit == null) return;
            
            _boundUnit = unit;
            
            // 设置基础信息
            UpdateUnitInfo();
            
            // 设置颜色
            Color color = unit.IsPlayerControlled ? _playerColor : _enemyColor;
            _atbFillImage.color = color;
            
            // 注册到ATB控制器
            _atbController.RegisterUnit(unit);
            
            // 订阅事件
            unit.OnDamageTaken += OnUnitDamaged;
            unit.OnHPChanged += OnHPChanged;
            unit.OnMPChanged += OnMPChanged;
            unit.OnStatusAdded += OnStatusAdded;
            unit.OnStatusRemoved += OnStatusRemoved;
            unit.OnDeath += OnUnitDeath;
            unit.OnRevive += OnUnitRevive;
            
            // ATB满事件
            _atbController.OnATBFull += OnATBFull;
            _atbController.OnActionComplete += OnActionComplete;
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// 解绑单位
        /// </summary>
        public void UnbindUnit()
        {
            if (_boundUnit == null) return;
            
            // 取消订阅
            _boundUnit.OnDamageTaken -= OnUnitDamaged;
            _boundUnit.OnHPChanged -= OnHPChanged;
            _boundUnit.OnMPChanged -= OnMPChanged;
            _boundUnit.OnStatusAdded -= OnStatusAdded;
            _boundUnit.OnStatusRemoved -= OnStatusRemoved;
            _boundUnit.OnDeath -= OnUnitDeath;
            _boundUnit.OnRevive -= OnUnitRevive;
            
            // 从ATB控制器注销
            _atbController.UnregisterUnit(_boundUnit);
            
            // ATB事件
            _atbController.OnATBFull -= OnATBFull;
            _atbController.OnActionComplete -= OnActionComplete;
            
            _boundUnit = null;
        }
        
        /// <summary>
        /// 高亮显示（用于目标选择）
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            // 可以添加边框高亮等效果
            var outline = GetComponent<UnityEngine.UIOutline>();
            if (outline != null)
            {
                outline.enabled = highlight;
            }
        }
        
        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            _rectTransform.anchoredPosition = position;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 更新ATB进度条
        /// </summary>
        private void UpdateATBBar()
        {
            if (_boundUnit == null) return;
            
            float percent = _atbController.GetATBPercent(_boundUnit);
            _atbFillImage.fillAmount = Mathf.Lerp(_atbFillImage.fillAmount, percent, Time.deltaTime * 10f);
            
            // 检查是否满
            bool isReadyNow = percent >= 0.99f;
            if (isReadyNow != _isReady)
            {
                _isReady = isReadyNow;
                _atbFillImage.color = _isReady ? _readyColor : 
                    (_boundUnit.IsPlayerControlled ? _playerColor : _enemyColor);
            }
        }
        
        /// <summary>
        /// 更新待命指示器
        /// </summary>
        private void UpdateReadyIndicator()
        {
            if (_readyIndicator == null) return;
            
            if (_isReady)
            {
                _readyPulseTimer += Time.deltaTime * _readyPulseSpeed;
                float alpha = (Mathf.Sin(_readyPulseTimer * Mathf.PI * 2) + 1f) / 2f;
                _readyIndicator.color = new Color(1f, 0.9f, 0.3f, alpha * 0.8f);
                _readyIndicator.gameObject.SetActive(true);
            }
            else
            {
                _readyIndicator.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 更新状态显示
        /// </summary>
        private void UpdateStatusDisplay()
        {
            // 状态图标已通过事件更新
        }
        
        /// <summary>
        /// 更新单位基础信息
        /// </summary>
        private void UpdateUnitInfo()
        {
            if (_boundUnit == null) return;
            
            var data = _boundUnit.CharacterData;
            
            if (_nameText != null)
                _nameText.text = data.characterName;
            
            if (_portraitImage != null && data.portrait != null)
                _portraitImage.sprite = data.portrait;
        }
        
        /// <summary>
        /// 更新完整显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (_boundUnit == null) return;
            
            // HP
            if (_hpText != null)
                _hpText.text = $"{_boundUnit.CurrentHp}/{_boundUnit.Stats.MaxHp}";
            
            if (_hpFillImage != null)
                _hpFillImage.fillAmount = (float)_boundUnit.CurrentHp / _boundUnit.Stats.MaxHp;
            
            // MP
            if (_mpText != null)
                _mpText.text = $"{_boundUnit.CurrentMp}/{_boundUnit.Stats.MaxMp}";
            
            if (_mpFillImage != null)
                _mpFillImage.fillAmount = (float)_boundUnit.CurrentMp / _boundUnit.Stats.MaxMp;
            
            // 死亡状态
            if (_deadOverlay != null)
                _deadOverlay.SetActive(!_boundUnit.IsAlive);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnUnitDamaged(BattleUnit unit, int damage)
        {
            // 受伤抖动效果
            StartCoroutine(DamageShake());
        }
        
        private void OnHPChanged(BattleUnit unit, int oldHP, int newHP)
        {
            UpdateDisplay();
            
            // 血量减少时变红闪烁
            if (newHP < oldHP)
            {
                StartCoroutine(HPDamageFlash());
            }
        }
        
        private void OnMPChanged(BattleUnit unit, int oldMP, int newMP)
        {
            UpdateDisplay();
        }
        
        private void OnStatusAdded(BattleUnit unit, StatusEffectInstance status)
        {
            CreateStatusIcon(status);
        }
        
        private void OnStatusRemoved(BattleUnit unit, StatusEffectInstance status)
        {
            RemoveStatusIcon(status);
        }
        
        private void OnUnitDeath(BattleUnit unit)
        {
            UpdateDisplay();
            
            // 死亡时变灰
            var canvasGroup = GetComponent<UnityEngine.CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.5f;
            }
        }
        
        private void OnUnitRevive(BattleUnit unit)
        {
            UpdateDisplay();
            
            var canvasGroup = GetComponent<UnityEngine.CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        private void OnATBFull(BattleUnit unit)
        {
            if (unit != _boundUnit) return;
            // 可以播放音效或特效
        }
        
        private void OnActionComplete(BattleUnit unit)
        {
            if (unit != _boundUnit) return;
            // 重置显示
        }
        
        #endregion
        
        #region Coroutines
        
        /// <summary>
        /// 受伤抖动效果
        /// </summary>
        private System.Collections.IEnumerator DamageShake()
        {
            float elapsed = 0f;
            Vector3 pos = _originalPosition;
            
            while (elapsed < _damageShakeDuration)
            {
                float x = Random.Range(-1f, 1f) * _damageShakeIntensity;
                float y = Random.Range(-1f, 1f) * _damageShakeIntensity;
                
                _rectTransform.localPosition = pos + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            _rectTransform.localPosition = pos;
        }
        
        /// <summary>
        /// 血量减少闪烁效果
        /// </summary>
        private System.Collections.IEnumerator HPDamageFlash()
        {
            if (_hpFillImage == null) yield break;
            
            Color original = _hpFillImage.color;
            _hpFillImage.color = Color.red;
            
            yield return new WaitForSeconds(0.1f);
            
            _hpFillImage.color = original;
        }
        
        #endregion
        
        #region Status Icons
        
        /// <summary>
        /// 创建状态图标
        /// </summary>
        private void CreateStatusIcon(StatusEffectInstance status)
        {
            if (_statusIconPrefab == null || _statusIconContainer == null) return;
            
            var icon = Instantiate(_statusIconPrefab, _statusIconContainer);
            var iconComponent = icon.GetComponent<StatusIcon>();
            if (iconComponent != null)
            {
                iconComponent.Initialize(status);
            }
        }
        
        /// <summary>
        /// 移除状态图标
        /// </summary>
        private void RemoveStatusIcon(StatusEffectInstance status)
        {
            if (_statusIconContainer == null) return;
            
            foreach (Transform child in _statusIconContainer)
            {
                var iconComponent = child.GetComponent<StatusIcon>();
                if (iconComponent != null && iconComponent.Status == status)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            UnbindUnit();
        }
        
        #endregion
    }
    
    /// <summary>
    /// 状态图标组件（需要单独实现）
    /// </summary>
    public class StatusIcon : MonoBehaviour
    {
        public StatusEffectInstance Status { get; private set; }
        
        public void Initialize(StatusEffectInstance status)
        {
            Status = status;
            // 设置图标和tooltip
        }
    }
}
