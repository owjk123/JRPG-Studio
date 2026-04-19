// TopBar.cs - 顶部状态栏
// 显示玩家等级、货币、体力等状态信息

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;

namespace JRPG.UI.Main
{
    /// <summary>
    /// 货币类型
    /// </summary>
    public enum CurrencyType
    {
        Gold,           // 金币
        Diamond,        // 钻石
        Stamina,        // 体力
        ArenaToken,     // 竞技场代币
        GuildToken      // 公会代币
    }

    /// <summary>
    /// 货币显示数据
    /// </summary>
    [Serializable]
    public class CurrencyDisplayData
    {
        public CurrencyType type;
        public long amount;
        public long maxAmount; // 体力等有上限的货币
        public bool showMax;  // 是否显示上限
    }

    /// <summary>
    /// 顶部状态栏组件
    /// </summary>
    public class TopBar : MonoBehaviour
    {
        #region UI References

        [Header("玩家信息")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        [SerializeField] private ProgressBar _expProgressBar;
        [SerializeField] private Button _avatarButton;

        [Header("货币显示")]
        [SerializeField] private Transform _currencyContainer;
        [SerializeField] private List<CurrencyItem> _currencyItems = new List<CurrencyItem>();

        [Header("功能按钮")]
        [SerializeField] private Button _mailButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private GameObject _mailRedDot;

        [Header("体力恢复")]
        [SerializeField] private TextMeshProUGUI _staminaRecoverTimeText;
        [SerializeField] private Button _staminaBuyButton;

        #endregion

        #region Fields

        // 玩家数据引用
        private int _playerId;
        private string _playerName;
        private int _playerLevel;
        private long _currentExp;
        private long _maxExp;

        // 货币数据
        private Dictionary<CurrencyType, CurrencyDisplayData> _currencyData = new Dictionary<CurrencyType, CurrencyDisplayData>();

        // 体力恢复计时
        private float _staminaRecoverInterval = 300f; // 5分钟恢复1点体力
        private int _staminaRecoverAmount = 1;
        private float _nextRecoverTime;

        // 回调
        private Action _onAvatarClick;
        private Action _onMailClick;
        private Action _onSettingsClick;

        #endregion

        #region Properties

        /// <summary>
        /// 玩家ID
        /// </summary>
        public int PlayerId => _playerId;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int PlayerLevel => _playerLevel;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeCurrencyItems();
        }

        protected virtual void Start()
        {
            SetupButtons();
            StartStaminaRecovery();
        }

        protected virtual void Update()
        {
            UpdateStaminaRecoverTime();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 从玩家数据管理器加载数据
            LoadPlayerData();
            LoadCurrencyData();
            UpdateAllDisplay();
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        public void Refresh()
        {
            LoadPlayerData();
            LoadCurrencyData();
            UpdateAllDisplay();
        }

        /// <summary>
        /// 更新玩家信息
        /// </summary>
        public void UpdatePlayerInfo(string name, int level, long currentExp, long maxExp)
        {
            _playerName = name;
            _playerLevel = level;
            _currentExp = currentExp;
            _maxExp = maxExp;

            UpdatePlayerDisplay();
        }

        /// <summary>
        /// 更新货币
        /// </summary>
        public void UpdateCurrency(CurrencyType type, long amount, long maxAmount = 0)
        {
            if (!_currencyData.ContainsKey(type))
            {
                _currencyData[type] = new CurrencyDisplayData { type = type };
            }

            _currencyData[type].amount = amount;
            _currencyData[type].maxAmount = maxAmount;
            _currencyData[type].showMax = maxAmount > 0;

            UpdateCurrencyDisplay(type);
        }

        /// <summary>
        /// 更新体力
        /// </summary>
        public void UpdateStamina(long current, long max, float nextRecoverTime)
        {
            UpdateCurrency(CurrencyType.Stamina, current, max);
            _nextRecoverTime = nextRecoverTime;
        }

        /// <summary>
        /// 添加货币动画
        /// </summary>
        public void ShowCurrencyAddAnimation(CurrencyType type, long addAmount)
        {
            CurrencyItem item = GetCurrencyItem(type);
            if (item != null)
            {
                item.PlayAddAnimation(addAmount);
            }
        }

        /// <summary>
        /// 消耗货币动画
        /// </summary>
        public void ShowCurrencyConsumeAnimation(CurrencyType type, long consumeAmount)
        {
            CurrencyItem item = GetCurrencyItem(type);
            if (item != null)
            {
                item.PlayConsumeAnimation(consumeAmount);
            }
        }

        /// <summary>
        /// 设置经验值增加动画
        /// </summary>
        public void ShowExpAddAnimation(long addExp)
        {
            if (_expProgressBar != null)
            {
                float oldProgress = (float)_currentExp / _maxExp;
                float newExp = _currentExp + addExp;

                // 更新进度
                _expProgressBar.SetRange(0, _maxExp);
                _expProgressBar.SetValue(newExp, false);
            }
        }

        /// <summary>
        /// 设置等级提升动画
        /// </summary>
        public void ShowLevelUpAnimation(int newLevel)
        {
            _playerLevel = newLevel;
            UpdatePlayerDisplay();

            // 播放升级特效
            // TODO: 实现升级动画
        }

        /// <summary>
        /// 显示/隐藏邮件红点
        /// </summary>
        public void ShowMailRedDot(bool show)
        {
            if (_mailRedDot != null)
            {
                _mailRedDot.SetActive(show);
            }
        }

        /// <summary>
        /// 注册头像点击回调
        /// </summary>
        public void RegisterAvatarClick(Action callback)
        {
            _onAvatarClick = callback;
        }

        /// <summary>
        /// 注册邮件点击回调
        /// </summary>
        public void RegisterMailClick(Action callback)
        {
            _onMailClick = callback;
        }

        /// <summary>
        /// 注册设置点击回调
        /// </summary>
        public void RegisterSettingsClick(Action callback)
        {
            _onSettingsClick = callback;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化货币显示项
        /// </summary>
        private void InitializeCurrencyItems()
        {
            // 如果没有预配置的货币项，根据CurrencyType自动创建
            if (_currencyItems.Count == 0 && _currencyContainer != null)
            {
                foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
                {
                    CreateCurrencyItem(type);
                }
            }
        }

        /// <summary>
        /// 创建货币显示项
        /// </summary>
        private void CreateCurrencyItem(CurrencyType type)
        {
            GameObject itemObj = new GameObject($"Currency_{type}");
            itemObj.transform.SetParent(_currencyContainer);

            CurrencyItem item = itemObj.AddComponent<CurrencyItem>();
            item.Initialize(type);
            _currencyItems.Add(item);
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_avatarButton != null)
            {
                _avatarButton.onClick.RemoveAllListeners();
                _avatarButton.onClick.AddListener(() => _onAvatarClick?.Invoke());
            }

            if (_mailButton != null)
            {
                _mailButton.onClick.RemoveAllListeners();
                _mailButton.onClick.AddListener(() => _onMailClick?.Invoke());
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(() => _onSettingsClick?.Invoke());
            }
        }

        /// <summary>
        /// 加载玩家数据
        /// </summary>
        private void LoadPlayerData()
        {
            // TODO: 从玩家数据管理器加载
            // 示例：
            // var playerData = PlayerDataManager.Instance.GetPlayerData();
            // _playerId = playerData.id;
            // _playerName = playerData.name;
            // _playerLevel = playerData.level;
            // _currentExp = playerData.currentExp;
            // _maxExp = playerData.maxExp;

            // 临时数据
            _playerId = 10001;
            _playerName = "冒险者";
            _playerLevel = 25;
            _currentExp = 1500;
            _maxExp = 2000;
        }

        /// <summary>
        /// 加载货币数据
        /// </summary>
        private void LoadCurrencyData()
        {
            // TODO: 从货币管理器加载
            // 示例：
            // _currencyData[CurrencyType.Gold] = new CurrencyDisplayData { amount = CurrencyManager.Instance.GetGold() };
            // _currencyData[CurrencyType.Diamond] = new CurrencyDisplayData { amount = CurrencyManager.Instance.GetDiamond() };
            // _currencyData[CurrencyType.Stamina] = CurrencyManager.Instance.GetStaminaData();

            // 临时数据
            _currencyData[CurrencyType.Gold] = new CurrencyDisplayData { amount = 1234567 };
            _currencyData[CurrencyType.Diamond] = new CurrencyDisplayData { amount = 680 };
            _currencyData[CurrencyType.Stamina] = new CurrencyDisplayData { amount = 80, maxAmount = 100, showMax = true };
            _currencyData[CurrencyType.ArenaToken] = new CurrencyDisplayData { amount = 520 };
            _currencyData[CurrencyType.GuildToken] = new CurrencyDisplayData { amount = 1200 };
        }

        /// <summary>
        /// 更新所有显示
        /// </summary>
        private void UpdateAllDisplay()
        {
            UpdatePlayerDisplay();
            UpdateCurrencyDisplay();
        }

        /// <summary>
        /// 更新玩家显示
        /// </summary>
        private void UpdatePlayerDisplay()
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = _playerName;
            }

            if (_playerLevelText != null)
            {
                _playerLevelText.text = $"Lv.{_playerLevel}";
            }

            if (_expProgressBar != null)
            {
                _expProgressBar.SetRange(0, _maxExp);
                _expProgressBar.SetValue(_currentExp, true);
            }
        }

        /// <summary>
        /// 更新货币显示
        /// </summary>
        private void UpdateCurrencyDisplay(CurrencyType? type = null)
        {
            if (type.HasValue)
            {
                CurrencyItem item = GetCurrencyItem(type.Value);
                if (item != null && _currencyData.TryGetValue(type.Value, out var data))
                {
                    item.SetAmount(data.amount, data.maxAmount, data.showMax);
                }
            }
            else
            {
                foreach (var kvp in _currencyData)
                {
                    CurrencyItem item = GetCurrencyItem(kvp.Key);
                    if (item != null)
                    {
                        item.SetAmount(kvp.Value.amount, kvp.Value.maxAmount, kvp.Value.showMax);
                    }
                }
            }
        }

        /// <summary>
        /// 获取货币显示项
        /// </summary>
        private CurrencyItem GetCurrencyItem(CurrencyType type)
        {
            return _currencyItems.Find(item => item.CurrencyType == type);
        }

        /// <summary>
        /// 开始体力恢复计时
        /// </summary>
        private void StartStaminaRecovery()
        {
            // TODO: 从服务器同步恢复时间
            _nextRecoverTime = Time.time + _staminaRecoverInterval;
        }

        /// <summary>
        /// 更新体力恢复时间显示
        /// </summary>
        private void UpdateStaminaRecoverTime()
        {
            if (_staminaRecoverTimeText == null) return;

            if (_currencyData.TryGetValue(CurrencyType.Stamina, out var staminaData))
            {
                // 体力已满时不显示
                if (staminaData.amount >= staminaData.maxAmount)
                {
                    _staminaRecoverTimeText.gameObject.SetActive(false);
                    return;
                }

                _staminaRecoverTimeText.gameObject.SetActive(true);

                float remainingTime = _nextRecoverTime - Time.time;
                if (remainingTime <= 0)
                {
                    // 恢复体力
                    UpdateCurrency(CurrencyType.Stamina, staminaData.amount + _staminaRecoverAmount, staminaData.maxAmount);
                    _nextRecoverTime = Time.time + _staminaRecoverInterval;
                    remainingTime = _staminaRecoverInterval;
                }

                // 格式化为 MM:SS
                int minutes = (int)(remainingTime / 60);
                int seconds = (int)(remainingTime % 60);
                _staminaRecoverTimeText.text = $"{minutes:D2}:{seconds:D2}";
            }
        }

        #endregion
    }

    /// <summary>
    /// 货币显示项组件
    /// </summary>
    public class CurrencyItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _maxText;

        #endregion

        #region Fields

        [SerializeField] private CurrencyType _currencyType;
        private long _currentAmount;
        private long _maxAmount;

        #endregion

        #region Properties

        public CurrencyType CurrencyType => _currencyType;

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(CurrencyType type)
        {
            _currencyType = type;

            // 加载对应图标
            string iconPath = $"UI/Icons/Currency/{type}";
            // Sprite icon = Resources.Load<Sprite>(iconPath);
            // if (_iconImage != null && icon != null)
            // {
            //     _iconImage.sprite = icon;
            // }
        }

        /// <summary>
        /// 设置货币数量
        /// </summary>
        public void SetAmount(long amount, long max = 0, bool showMax = false)
        {
            _currentAmount = amount;
            _maxAmount = max;

            if (_amountText != null)
            {
                _amountText.text = FormatAmount(amount);
            }

            if (_maxText != null)
            {
                _maxText.gameObject.SetActive(showMax);
                if (showMax)
                {
                    _maxText.text = $"/{FormatAmount(max)}";
                }
            }
        }

        /// <summary>
        /// 播放增加动画
        /// </summary>
        public void PlayAddAnimation(long addAmount)
        {
            // TODO: 实现增加动画
            Core.UITweener.Pulse(transform as RectTransform);
        }

        /// <summary>
        /// 播放消耗动画
        /// </summary>
        public void PlayConsumeAnimation(long consumeAmount)
        {
            // TODO: 实现消耗动画
            Core.UITweener.Shake(transform as RectTransform);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 格式化货币数量显示
        /// </summary>
        private string FormatAmount(long amount)
        {
            if (amount >= 100000000)
            {
                return $"{amount / 100000000.0:F1}亿";
            }
            else if (amount >= 10000)
            {
                return $"{amount / 10000.0:F1}万";
            }
            else
            {
                return amount.ToString("N0");
            }
        }

        #endregion
    }
}
