// DisplaySettings.cs - 显示设置组件
// 管理游戏显示设置

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using JRPG.UI.Common;

namespace JRPG.UI.Settings
{
    /// <summary>
    /// 显示设置数据
    /// </summary>
    [Serializable]
    public class DisplaySettingsData
    {
        public int graphicsQuality = 2;           // 0=低, 1=中, 2=高
        public bool enableBloom = true;
        public bool enableShadow = true;
        public bool enableVsync = false;
        public int targetFrameRate = 60;
        public bool enableSafeArea = true;
        public bool enableNotchAdaptation = true;
        public float uiScale = 1f;
        public bool autoBattleSpeed = false;
        public int battleSpeed = 1;               // 1=1倍, 2=2倍
        public bool showDamageNumber = true;
        public bool showHpBar = true;
        public bool showTutorial = true;
    }

    /// <summary>
    /// 显示设置组件
    /// </summary>
    public class DisplaySettings : MonoBehaviour
    {
        #region UI References

        [Header("画质设置")]
        [SerializeField] private Dropdown _qualityDropdown;
        [SerializeField] private Toggle _bloomToggle;
        [SerializeField] private Toggle _shadowToggle;
        [SerializeField] private Toggle _vsyncToggle;

        [Header("帧率设置")]
        [SerializeField] private Dropdown _frameRateDropdown;
        [SerializeField] private TextMeshProUGUI _currentFpsText;

        [Header("UI设置")]
        [SerializeField] private Slider _uiScaleSlider;
        [SerializeField] private TextMeshProUGUI _uiScaleText;
        [SerializeField] private Toggle _safeAreaToggle;
        [SerializeField] private Toggle _notchAdaptToggle;

        [Header("战斗设置")]
        [SerializeField] private Toggle _autoBattleToggle;
        [SerializeField] private Dropdown _battleSpeedDropdown;
        [SerializeField] private Toggle _damageNumberToggle;
        [SerializeField] private Toggle _hpBarToggle;

        [Header("其他")]
        [SerializeField] private Toggle _showTutorialToggle;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _applyButton;

        #endregion

        #region Fields

        private DisplaySettingsData _settings = new DisplaySettingsData();
        private bool _isInitialized = false;

        // 预设选项
        private string[] _qualityOptions = { "低", "中", "高", "极致" };
        private string[] _frameRateOptions = { "30 FPS", "60 FPS", "120 FPS" };
        private string[] _battleSpeedOptions = { "1倍速", "2倍速" };

        // 事件
        public event Action<DisplaySettingsData> OnSettingsChanged;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            LoadSettings();
            SetupControls();
            UpdateAllDisplay();
            _isInitialized = true;
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            if (!_isInitialized) return;
            LoadSettings();
            UpdateAllDisplay();
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings()
        {
            // 保存到PlayerPrefs
            PlayerPrefs.SetInt("Display_Quality", _settings.graphicsQuality);
            PlayerPrefs.SetInt("Display_Bloom", _settings.enableBloom ? 1 : 0);
            PlayerPrefs.SetInt("Display_Shadow", _settings.enableShadow ? 1 : 0);
            PlayerPrefs.SetInt("Display_Vsync", _settings.enableVsync ? 1 : 0);
            PlayerPrefs.SetInt("Display_FrameRate", _settings.targetFrameRate);
            PlayerPrefs.SetInt("Display_SafeArea", _settings.enableSafeArea ? 1 : 0);
            PlayerPrefs.SetInt("Display_Notch", _settings.enableNotchAdaptation ? 1 : 0);
            PlayerPrefs.SetFloat("Display_UiScale", _settings.uiScale);
            PlayerPrefs.SetInt("Display_AutoBattle", _settings.autoBattleSpeed ? 1 : 0);
            PlayerPrefs.SetInt("Display_BattleSpeed", _settings.battleSpeed);
            PlayerPrefs.Save();

            ApplySettings();
            Debug.Log("[DisplaySettings] 显示设置已保存");
        }

        /// <summary>
        /// 重置为默认
        /// </summary>
        public void ResetToDefault()
        {
            _settings = new DisplaySettingsData();
            UpdateAllDisplay();
        }

        /// <summary>
        /// 获取设置数据
        /// </summary>
        public DisplaySettingsData GetSettings()
        {
            return _settings;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            _settings.graphicsQuality = PlayerPrefs.GetInt("Display_Quality", 2);
            _settings.enableBloom = PlayerPrefs.GetInt("Display_Bloom", 1) == 1;
            _settings.enableShadow = PlayerPrefs.GetInt("Display_Shadow", 1) == 1;
            _settings.enableVsync = PlayerPrefs.GetInt("Display_Vsync", 0) == 1;
            _settings.targetFrameRate = PlayerPrefs.GetInt("Display_FrameRate", 60);
            _settings.enableSafeArea = PlayerPrefs.GetInt("Display_SafeArea", 1) == 1;
            _settings.enableNotchAdaptation = PlayerPrefs.GetInt("Display_Notch", 1) == 1;
            _settings.uiScale = PlayerPrefs.GetFloat("Display_UiScale", 1f);
            _settings.autoBattleSpeed = PlayerPrefs.GetInt("Display_AutoBattle", 0) == 1;
            _settings.battleSpeed = PlayerPrefs.GetInt("Display_BattleSpeed", 1);
            _settings.showDamageNumber = PlayerPrefs.GetInt("Display_DamageNumber", 1) == 1;
            _settings.showHpBar = PlayerPrefs.GetInt("Display_HpBar", 1) == 1;
            _settings.showTutorial = PlayerPrefs.GetInt("Display_Tutorial", 1) == 1;
        }

        /// <summary>
        /// 设置控件
        /// </summary>
        private void SetupControls()
        {
            // 画质设置
            if (_qualityDropdown != null)
            {
                _qualityDropdown.ClearOptions();
                _qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(_qualityOptions));
                _qualityDropdown.onValueChanged.RemoveAllListeners();
                _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            // 帧率设置
            if (_frameRateDropdown != null)
            {
                _frameRateDropdown.ClearOptions();
                _frameRateDropdown.AddOptions(new System.Collections.Generic.List<string>(_frameRateOptions));
                _frameRateDropdown.onValueChanged.RemoveAllListeners();
                _frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
            }

            // 战斗速度设置
            if (_battleSpeedDropdown != null)
            {
                _battleSpeedDropdown.ClearOptions();
                _battleSpeedDropdown.AddOptions(new System.Collections.Generic.List<string>(_battleSpeedOptions));
                _battleSpeedDropdown.onValueChanged.RemoveAllListeners();
                _battleSpeedDropdown.onValueChanged.AddListener(OnBattleSpeedChanged);
            }

            // Toggle设置
            SetupToggle(_bloomToggle, _settings.enableBloom, OnBloomChanged);
            SetupToggle(_shadowToggle, _settings.enableShadow, OnShadowChanged);
            SetupToggle(_vsyncToggle, _settings.enableVsync, OnVsyncChanged);
            SetupToggle(_safeAreaToggle, _settings.enableSafeArea, OnSafeAreaChanged);
            SetupToggle(_notchAdaptToggle, _settings.enableNotchAdaptation, OnNotchAdaptChanged);
            SetupToggle(_autoBattleToggle, _settings.autoBattleSpeed, OnAutoBattleChanged);
            SetupToggle(_damageNumberToggle, _settings.showDamageNumber, OnDamageNumberChanged);
            SetupToggle(_hpBarToggle, _settings.showHpBar, OnHpBarChanged);
            SetupToggle(_showTutorialToggle, _settings.showTutorial, OnShowTutorialChanged);

            // 滑块设置
            if (_uiScaleSlider != null)
            {
                _uiScaleSlider.minValue = 0.8f;
                _uiScaleSlider.maxValue = 1.2f;
                _uiScaleSlider.onValueChanged.RemoveAllListeners();
                _uiScaleSlider.onValueChanged.AddListener(OnUiScaleChanged);
            }

            // 按钮设置
            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
                _resetButton.onClick.AddListener(OnResetClick);
            }

            if (_applyButton != null)
            {
                _applyButton.onClick.RemoveAllListeners();
                _applyButton.onClick.AddListener(OnApplyClick);
            }
        }

        /// <summary>
        /// 设置Toggle
        /// </summary>
        private void SetupToggle(Toggle toggle, bool value, Action<bool> callback)
        {
            if (toggle != null)
            {
                toggle.isOn = value;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(callback);
            }
        }

        /// <summary>
        /// 更新所有显示
        /// </summary>
        private void UpdateAllDisplay()
        {
            UpdateQualityDisplay();
            UpdateFrameRateDisplay();
            UpdateBattleSpeedDisplay();
            UpdateUiScaleDisplay();
            UpdateFpsDisplay();
        }

        /// <summary>
        /// 更新画质显示
        /// </summary>
        private void UpdateQualityDisplay()
        {
            if (_qualityDropdown != null)
            {
                _qualityDropdown.value = _settings.graphicsQuality;
            }
        }

        /// <summary>
        /// 更新帧率显示
        /// </summary>
        private void UpdateFrameRateDisplay()
        {
            if (_frameRateDropdown != null)
            {
                int index = _settings.targetFrameRate switch
                {
                    30 => 0,
                    60 => 1,
                    120 => 2,
                    _ => 1
                };
                _frameRateDropdown.value = index;
            }

            UpdateFpsDisplay();
        }

        /// <summary>
        /// 更新战斗速度显示
        /// </summary>
        private void UpdateBattleSpeedDisplay()
        {
            if (_battleSpeedDropdown != null)
            {
                _battleSpeedDropdown.value = _settings.battleSpeed - 1;
            }
        }

        /// <summary>
        /// 更新UI缩放显示
        /// </summary>
        private void UpdateUiScaleDisplay()
        {
            if (_uiScaleSlider != null)
            {
                _uiScaleSlider.value = _settings.uiScale;
            }

            if (_uiScaleText != null)
            {
                _uiScaleText.text = $"{Mathf.RoundToInt(_settings.uiScale * 100)}%";
            }
        }

        /// <summary>
        /// 更新FPS显示
        /// </summary>
        private void UpdateFpsDisplay()
        {
            if (_currentFpsText != null)
            {
                int currentFps = (int)(1f / Time.deltaTime);
                _currentFpsText.text = $"当前: {currentFps} FPS";
            }
        }

        /// <summary>
        /// 应用设置
        /// </summary>
        private void ApplySettings()
        {
            // 应用画质设置
            QualitySettings.SetQualityLevel(_settings.graphicsQuality);

            // 应用帧率设置
            Application.targetFrameRate = _settings.targetFrameRate;

            // 应用垂直同步
            QualitySettings.vSyncCount = _settings.enableVsync ? 1 : 0;

            // 触发事件
            OnSettingsChanged?.Invoke(_settings);
        }

        #endregion

        #region Event Handlers

        private void OnQualityChanged(int value)
        {
            _settings.graphicsQuality = value;
            ApplySettings();
        }

        private void OnBloomChanged(bool value)
        {
            _settings.enableBloom = value;
        }

        private void OnShadowChanged(bool value)
        {
            _settings.enableShadow = value;
        }

        private void OnVsyncChanged(bool value)
        {
            _settings.enableVsync = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
        }

        private void OnFrameRateChanged(int value)
        {
            int[] frameRates = { 30, 60, 120 };
            _settings.targetFrameRate = frameRates[value];
            Application.targetFrameRate = _settings.targetFrameRate;
            UpdateFpsDisplay();
        }

        private void OnSafeAreaChanged(bool value)
        {
            _settings.enableSafeArea = value;
        }

        private void OnNotchAdaptChanged(bool value)
        {
            _settings.enableNotchAdaptation = value;
        }

        private void OnUiScaleChanged(float value)
        {
            _settings.uiScale = value;
            UpdateUiScaleDisplay();
        }

        private void OnAutoBattleChanged(bool value)
        {
            _settings.autoBattleSpeed = value;
        }

        private void OnBattleSpeedChanged(int value)
        {
            _settings.battleSpeed = value + 1;
        }

        private void OnDamageNumberChanged(bool value)
        {
            _settings.showDamageNumber = value;
        }

        private void OnHpBarChanged(bool value)
        {
            _settings.showHpBar = value;
        }

        private void OnShowTutorialChanged(bool value)
        {
            _settings.showTutorial = value;
        }

        private void OnResetClick()
        {
            ResetToDefault();
            ApplySettings();
        }

        private void OnApplyClick()
        {
            SaveSettings();
        }

        #endregion
    }
}
