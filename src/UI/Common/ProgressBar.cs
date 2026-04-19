// ProgressBar.cs - 进度条组件
// 支持多种样式的进度条显示

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using JRPG.UI.Core;

namespace JRPG.UI.Common
{
    /// <summary>
    /// 进度条类型
    /// </summary>
    public enum ProgressBarType
    {
        Horizontal,      // 水平进度条
        Circular,        // 圆形进度条
        Diamond         // 菱形进度条
    }

    /// <summary>
    /// 进度条填充模式
    /// </summary>
    public enum FillMode
    {
        Normal,          // 普通填充
        Smooth,          // 平滑填充
        Instant          // 立即填充
    }

    /// <summary>
    /// 进度条配置
    /// </summary>
    [Serializable]
    public class ProgressBarConfig
    {
        public float minValue = 0f;
        public float maxValue = 100f;
        public float currentValue = 0f;
        public bool showPercentage = true;
        public bool showValue = false;
        public bool showLabel = false;
        public string labelText = "";
        public string suffix = "%";
        public FillMode fillMode = FillMode.Smooth;
        public bool useColorGradient = true;
        public Gradient colorGradient;
    }

    /// <summary>
    /// 进度条组件
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        #region UI References

        [Header("类型配置")]
        [SerializeField] private ProgressBarType _barType = ProgressBarType.Horizontal;

        [Header("填充")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _backgroundImage;

        [Header("文字")]
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _labelText;

        [Header("特效")]
        [SerializeField] private Image _glowEffect;
        [SerializeField] private ParticleSystem _fillParticles;

        [Header("动画配置")]
        [SerializeField] private bool _useAnimUpdate = true;
        [SerializeField] private float _animSpeed = 5f;
        [SerializeField] private AnimationCurve _animCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        #endregion

        #region Fields

        [Header("配置")]
        [SerializeField] private ProgressBarConfig _config = new ProgressBarConfig();

        private float _displayValue = 0f;
        private float _targetValue = 0f;
        private float _animProgress = 0f;
        private bool _isAnimating = false;
        private Action _onComplete;

        #endregion

        #region Properties

        /// <summary>
        /// 当前值
        /// </summary>
        public float Value
        {
            get => _config.currentValue;
            set => SetValue(value);
        }

        /// <summary>
        /// 进度 (0-1)
        /// </summary>
        public float Progress => (_config.maxValue - _config.minValue) > 0 
            ? (_config.currentValue - _config.minValue) / (_config.maxValue - _config.minValue) 
            : 0f;

        /// <summary>
        /// 显示值
        /// </summary>
        public float DisplayValue => _displayValue;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void Start()
        {
            // 初始化显示
            UpdateDisplay(_config.currentValue);
        }

        protected virtual void Update()
        {
            if (_isAnimating && _useAnimUpdate)
            {
                UpdateAnimation();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            _displayValue = _config.currentValue;
            _targetValue = _config.currentValue;
            UpdateFill(Progress);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void SetValue(float value, bool instant = false)
        {
            float oldValue = _config.currentValue;
            _config.currentValue = Mathf.Clamp(value, _config.minValue, _config.maxValue);
            _targetValue = _config.currentValue;

            if (instant || _config.fillMode == FillMode.Instant)
            {
                _displayValue = _config.currentValue;
                UpdateDisplay(_displayValue);
                UpdateFill(Progress);
            }
            else
            {
                // 开始动画
                _animProgress = 0f;
                _isAnimating = true;
            }
        }

        /// <summary>
        /// 增加值
        /// </summary>
        public void AddValue(float delta, Action onComplete = null)
        {
            SetValue(_config.currentValue + delta);
            _onComplete = onComplete;
        }

        /// <summary>
        /// 设置范围
        /// </summary>
        public void SetRange(float min, float max)
        {
            _config.minValue = min;
            _config.maxValue = max;
            UpdateFill(Progress);
            UpdateDisplay(_displayValue);
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        public void SetConfig(ProgressBarConfig config)
        {
            _config = config;
            UpdateDisplay(_config.currentValue);
            UpdateFill(Progress);
        }

        /// <summary>
        /// 设置标签文字
        /// </summary>
        public void SetLabel(string label)
        {
            _config.labelText = label;
            if (_labelText != null)
            {
                _labelText.text = label;
            }
        }

        /// <summary>
        /// 设置百分比格式
        /// </summary>
        public void SetPercentageFormat(string prefix = "", string suffix = "%")
        {
            _config.showPercentage = true;
            _config.suffix = suffix;
        }

        /// <summary>
        /// 完成动画
        /// </summary>
        public void CompleteAnimation()
        {
            _displayValue = _targetValue;
            _isAnimating = false;
            UpdateDisplay(_displayValue);
            UpdateFill(Progress);
            _onComplete?.Invoke();
            _onComplete = null;
        }

        #endregion

        #region Protected Methods

        protected virtual void UpdateAnimation()
        {
            _animProgress += Time.deltaTime * _animSpeed;
            float t = _animCurve.Evaluate(Mathf.Clamp01(_animProgress));
            
            float oldDisplay = _displayValue;
            _displayValue = Mathf.Lerp(oldDisplay, _targetValue, t);

            UpdateDisplay(_displayValue);

            // 平滑模式下实时更新填充
            if (_config.fillMode == FillMode.Smooth)
            {
                UpdateFill(Progress);
            }

            // 动画完成
            if (_animProgress >= 1f)
            {
                CompleteAnimation();
            }
        }

        protected virtual void UpdateDisplay(float value)
        {
            // 更新数值文字
            if (_valueText != null)
            {
                if (_config.showPercentage)
                {
                    float percentage = Progress * 100;
                    _valueText.text = $"{percentage:F0}{_config.suffix}";
                }
                else if (_config.showValue)
                {
                    _valueText.text = $"{value:F0}{_config.suffix}";
                }
            }

            // 更新标签
            if (_labelText != null && !string.IsNullOrEmpty(_config.labelText))
            {
                _labelText.text = _config.labelText;
            }

            // 更新颜色
            UpdateColor();
        }

        protected virtual void UpdateFill(float progress)
        {
            if (_fillImage == null) return;

            progress = Mathf.Clamp01(progress);

            switch (_barType)
            {
                case ProgressBarType.Horizontal:
                    _fillImage.fillAmount = progress;
                    break;
                case ProgressBarType.Circular:
                    UpdateCircularFill(progress);
                    break;
                case ProgressBarType.Diamond:
                    UpdateDiamondFill(progress);
                    break;
            }
        }

        protected virtual void UpdateColor()
        {
            if (_fillImage == null) return;

            if (_config.useColorGradient && _config.colorGradient != null)
            {
                _fillImage.color = _config.colorGradient.Evaluate(Progress);
            }
        }

        #endregion

        #region Special Fill Mode

        private void UpdateCircularFill(float progress)
        {
            // 圆形进度条使用Image的fillAmount配合rotation
            if (_fillImage != null)
            {
                _fillImage.fillAmount = progress;
                _fillImage.transform.localRotation = Quaternion.Euler(0, 0, -90);
            }
        }

        private void UpdateDiamondFill(float progress)
        {
            // 菱形进度条通过scale或mask实现
            if (_fillImage != null)
            {
                Vector3 scale = _fillImage.transform.localScale;
                scale.y = progress;
                _fillImage.transform.localScale = scale;
            }
        }

        #endregion
    }
}
