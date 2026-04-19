// AudioSettings.cs - 音频设置组件
// 管理游戏音频设置

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using JRPG.UI.Common;

namespace JRPG.UI.Settings
{
    /// <summary>
    /// 音频设置数据
    /// </summary>
    [Serializable]
    public class AudioSettingsData
    {
        public float masterVolume = 1f;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 1f;
        public float voiceVolume = 1f;
        public bool bgmMute = false;
        public bool sfxMute = false;
        public bool voiceMute = false;
    }

    /// <summary>
    /// 音频设置组件
    /// </summary>
    public class AudioSettings : MonoBehaviour
    {
        #region UI References

        [Header("总音量")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterVolumeText;
        [SerializeField] private Button _masterMuteButton;
        [SerializeField] private Image _masterMuteIcon;

        [Header("背景音乐")]
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private TextMeshProUGUI _bgmVolumeText;
        [SerializeField] private Button _bgmMuteButton;
        [SerializeField] private Image _bgmMuteIcon;

        [Header("音效")]
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI _sfxVolumeText;
        [SerializeField] private Button _sfxMuteButton;
        [SerializeField] private Image _sfxMuteIcon;

        [Header("语音")]
        [SerializeField] private Slider _voiceVolumeSlider;
        [SerializeField] private TextMeshProUGUI _voiceVolumeText;
        [SerializeField] private Button _voiceMuteButton;
        [SerializeField] private Image _voiceMuteIcon;

        [Header("预设")]
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _testBgmButton;
        [SerializeField] private Button _testSfxButton;

        [Header("静音图标")]
        [SerializeField] private Sprite _muteOnIcon;
        [SerializeField] private Sprite _muteOffIcon;

        #endregion

        #region Fields

        private AudioSettingsData _settings = new AudioSettingsData();
        private bool _isInitialized = false;

        // 事件
        public event Action<AudioSettingsData> OnSettingsChanged;

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
            SetupSliders();
            SetupButtons();
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
            // TODO: 保存到PlayerPrefs或服务器
            PlayerPrefs.SetFloat("Audio_MasterVolume", _settings.masterVolume);
            PlayerPrefs.SetFloat("Audio_BgmVolume", _settings.bgmVolume);
            PlayerPrefs.SetFloat("Audio_SfxVolume", _settings.sfxVolume);
            PlayerPrefs.SetFloat("Audio_VoiceVolume", _settings.voiceVolume);
            PlayerPrefs.Save();

            Debug.Log("[AudioSettings] 音频设置已保存");
        }

        /// <summary>
        /// 重置为默认
        /// </summary>
        public void ResetToDefault()
        {
            _settings = new AudioSettingsData();
            UpdateAllDisplay();
            ApplySettings();
        }

        /// <summary>
        /// 获取设置数据
        /// </summary>
        public AudioSettingsData GetSettings()
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
            // 从PlayerPrefs加载
            _settings.masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
            _settings.bgmVolume = PlayerPrefs.GetFloat("Audio_BgmVolume", 0.8f);
            _settings.sfxVolume = PlayerPrefs.GetFloat("Audio_SfxVolume", 1f);
            _settings.voiceVolume = PlayerPrefs.GetFloat("Audio_VoiceVolume", 1f);
            _settings.bgmMute = PlayerPrefs.GetInt("Audio_BgmMute", 0) == 1;
            _settings.sfxMute = PlayerPrefs.GetInt("Audio_SfxMute", 0) == 1;
            _settings.voiceMute = PlayerPrefs.GetInt("Audio_VoiceMute", 0) == 1;
        }

        /// <summary>
        /// 设置滑块
        /// </summary>
        private void SetupSliders()
        {
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.onValueChanged.RemoveAllListeners();
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.onValueChanged.RemoveAllListeners();
                _bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.onValueChanged.RemoveAllListeners();
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_voiceVolumeSlider != null)
            {
                _voiceVolumeSlider.onValueChanged.RemoveAllListeners();
                _voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            }
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_masterMuteButton != null)
            {
                _masterMuteButton.onClick.RemoveAllListeners();
                _masterMuteButton.onClick.AddListener(OnMasterMuteClick);
            }

            if (_bgmMuteButton != null)
            {
                _bgmMuteButton.onClick.RemoveAllListeners();
                _bgmMuteButton.onClick.AddListener(OnBgmMuteClick);
            }

            if (_sfxMuteButton != null)
            {
                _sfxMuteButton.onClick.RemoveAllListeners();
                _sfxMuteButton.onClick.AddListener(OnSfxMuteClick);
            }

            if (_voiceMuteButton != null)
            {
                _voiceMuteButton.onClick.RemoveAllListeners();
                _voiceMuteButton.onClick.AddListener(OnVoiceMuteClick);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.RemoveAllListeners();
                _resetButton.onClick.AddListener(OnResetClick);
            }

            if (_testBgmButton != null)
            {
                _testBgmButton.onClick.RemoveAllListeners();
                _testBgmButton.onClick.AddListener(OnTestBgmClick);
            }

            if (_testSfxButton != null)
            {
                _testSfxButton.onClick.RemoveAllListeners();
                _testSfxButton.onClick.AddListener(OnTestSfxClick);
            }
        }

        /// <summary>
        /// 更新所有显示
        /// </summary>
        private void UpdateAllDisplay()
        {
            UpdateMasterVolumeDisplay();
            UpdateBgmVolumeDisplay();
            UpdateSfxVolumeDisplay();
            UpdateVoiceVolumeDisplay();
        }

        /// <summary>
        /// 更新主音量显示
        /// </summary>
        private void UpdateMasterVolumeDisplay()
        {
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.value = _settings.masterVolume;
            }

            if (_masterVolumeText != null)
            {
                _masterVolumeText.text = Mathf.RoundToInt(_settings.masterVolume * 100) + "%";
            }

            UpdateMuteIcon(_masterMuteIcon, _settings.masterVolume <= 0);
        }

        /// <summary>
        /// 更新背景音乐显示
        /// </summary>
        private void UpdateBgmVolumeDisplay()
        {
            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.value = _settings.bgmVolume;
            }

            if (_bgmVolumeText != null)
            {
                _bgmVolumeText.text = Mathf.RoundToInt(_settings.bgmVolume * 100) + "%";
            }

            UpdateMuteIcon(_bgmMuteIcon, _settings.bgmMute);
        }

        /// <summary>
        /// 更新音效显示
        /// </summary>
        private void UpdateSfxVolumeDisplay()
        {
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.value = _settings.sfxVolume;
            }

            if (_sfxVolumeText != null)
            {
                _sfxVolumeText.text = Mathf.RoundToInt(_settings.sfxVolume * 100) + "%";
            }

            UpdateMuteIcon(_sfxMuteIcon, _settings.sfxMute);
        }

        /// <summary>
        /// 更新语音显示
        /// </summary>
        private void UpdateVoiceVolumeDisplay()
        {
            if (_voiceVolumeSlider != null)
            {
                _voiceVolumeSlider.value = _settings.voiceVolume;
            }

            if (_voiceVolumeText != null)
            {
                _voiceVolumeText.text = Mathf.RoundToInt(_settings.voiceVolume * 100) + "%";
            }

            UpdateMuteIcon(_voiceMuteIcon, _settings.voiceMute);
        }

        /// <summary>
        /// 更新静音图标
        /// </summary>
        private void UpdateMuteIcon(Image icon, bool isMuted)
        {
            if (icon != null)
            {
                icon.sprite = isMuted ? _muteOnIcon : _muteOffIcon;
            }
        }

        /// <summary>
        /// 应用设置到音频系统
        /// </summary>
        private void ApplySettings()
        {
            // TODO: 应用到AudioManager
            OnSettingsChanged?.Invoke(_settings);
        }

        #endregion

        #region Event Handlers

        private void OnMasterVolumeChanged(float value)
        {
            _settings.masterVolume = value;
            UpdateMasterVolumeDisplay();
            ApplySettings();
        }

        private void OnBgmVolumeChanged(float value)
        {
            _settings.bgmVolume = value;
            UpdateBgmVolumeDisplay();
            ApplySettings();
        }

        private void OnSfxVolumeChanged(float value)
        {
            _settings.sfxVolume = value;
            UpdateSfxVolumeDisplay();
            ApplySettings();
        }

        private void OnVoiceVolumeChanged(float value)
        {
            _settings.voiceVolume = value;
            UpdateVoiceVolumeDisplay();
            ApplySettings();
        }

        private void OnMasterMuteClick()
        {
            _settings.masterVolume = _settings.masterVolume > 0 ? 0 : 1f;
            UpdateMasterVolumeDisplay();
            ApplySettings();
        }

        private void OnBgmMuteClick()
        {
            _settings.bgmMute = !_settings.bgmMute;
            UpdateBgmVolumeDisplay();
            ApplySettings();
        }

        private void OnSfxMuteClick()
        {
            _settings.sfxMute = !_settings.sfxMute;
            UpdateSfxVolumeDisplay();
            ApplySettings();
        }

        private void OnVoiceMuteClick()
        {
            _settings.voiceMute = !_settings.voiceMute;
            UpdateVoiceVolumeDisplay();
            ApplySettings();
        }

        private void OnResetClick()
        {
            ResetToDefault();
        }

        private void OnTestBgmClick()
        {
            // TODO: 播放测试背景音乐
            Debug.Log("[AudioSettings] 播放测试BGM");
        }

        private void OnTestSfxClick()
        {
            // TODO: 播放测试音效
            Debug.Log("[AudioSettings] 播放测试SFX");
        }

        #endregion
    }
}
