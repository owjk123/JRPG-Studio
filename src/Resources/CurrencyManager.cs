// CurrencyManager.cs - 货币管理系统
// 管理游戏中的各种货币（金币、钻石、体力等）

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JRPG.Resources
{
    /// <summary>
    /// 货币类型枚举
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>金币</summary>
        Gold,
        
        /// <summary>钻石</summary>
        Diamond,
        
        /// <summary>星辉（免费抽卡货币）</summary>
        StarShine,
        
        /// <summary>星晶石（付费货币）</summary>
        StarCrystal,
        
        /// <summary>命运之印（重复角色转化）</summary>
        FateStamp,
        
        /// <summary>体力</summary>
        Stamina,
        
        /// <summary>友情点</summary>
        FriendshipPoint
    }
    
    /// <summary>
    /// 货币配置
    /// </summary>
    [Serializable]
    public class CurrencyConfig
    {
        public CurrencyType type;
        public string name;
        public string description;
        public Sprite icon;
        public int maxValue;
        public int dailyRecoveryAmount;
        public int dailyRecoveryTimeMinutes;
    }
    
    /// <summary>
    /// 货币数据
    /// </summary>
    [Serializable]
    public class CurrencyData
    {
        public CurrencyType type;
        public long amount;
        public long lastUpdateTime;
        
        /// <summary>
        /// 获取当前值（考虑自然恢复）
        /// </summary>
        public long GetCurrentAmount()
        {
            // 简化处理，实际应计算自然恢复
            return amount;
        }
    }
    
    /// <summary>
    /// 货币管理器单例类
    /// </summary>
    public class CurrencyManager
    {
        #region 单例
        
        private static CurrencyManager _instance;
        public static CurrencyManager Instance => _instance ??= new CurrencyManager();
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 货币变化事件
        /// </summary>
        public event Action<CurrencyType, long, long> OnCurrencyChanged;
        
        /// <summary>
        /// 货币不足事件
        /// </summary>
        public event Action<CurrencyType, long> OnCurrencyInsufficient;
        
        #endregion
        
        #region 私有变量
        
        /// <summary>
        /// 货币数据表
        /// </summary>
        private Dictionary<CurrencyType, CurrencyData> _currencies = new Dictionary<CurrencyType, CurrencyData>();
        
        /// <summary>
        /// 货币配置表
        /// </summary>
        private Dictionary<CurrencyType, CurrencyConfig> _configs = new Dictionary<CurrencyType, CurrencyConfig>();
        
        #endregion
        
        #region 构造函数
        
        private CurrencyManager()
        {
            InitializeCurrencies();
            InitializeConfigs();
        }
        
        /// <summary>
        /// 初始化货币
        /// </summary>
        private void InitializeCurrencies()
        {
            // 默认货币值
            _currencies[CurrencyType.Gold] = new CurrencyData { type = CurrencyType.Gold, amount = 10000 };
            _currencies[CurrencyType.Diamond] = new CurrencyData { type = CurrencyType.Diamond, amount = 0 };
            _currencies[CurrencyType.StarShine] = new CurrencyData { type = CurrencyType.StarShine, amount = 0 };
            _currencies[CurrencyType.StarCrystal] = new CurrencyData { type = CurrencyType.StarCrystal, amount = 0 };
            _currencies[CurrencyType.FateStamp] = new CurrencyData { type = CurrencyType.FateStamp, amount = 0 };
            _currencies[CurrencyType.Stamina] = new CurrencyData { type = CurrencyType.Stamina, amount = 100, maxValue = 100 };
            _currencies[CurrencyType.FriendshipPoint] = new CurrencyData { type = CurrencyType.FriendshipPoint, amount = 0 };
        }
        
        /// <summary>
        /// 初始化配置
        /// </summary>
        private void InitializeConfigs()
        {
            // 金币
            _configs[CurrencyType.Gold] = new CurrencyConfig
            {
                type = CurrencyType.Gold,
                name = "金币",
                description = "最基础的货币，用于装备强化、技能升级等",
                maxValue = 999999999
            };
            
            // 钻石
            _configs[CurrencyType.Diamond] = new CurrencyConfig
            {
                type = CurrencyType.Diamond,
                name = "钻石",
                description = "稀有货币，可用于购买礼包、刷新商店等",
                maxValue = 999999999
            };
            
            // 星辉
            _configs[CurrencyType.StarShine] = new CurrencyConfig
            {
                type = CurrencyType.StarShine,
                name = "星辉",
                description = "免费获得的抽卡货币",
                maxValue = 99999
            };
            
            // 星晶石
            _configs[CurrencyType.StarCrystal] = new CurrencyConfig
            {
                type = CurrencyType.StarCrystal,
                name = "星晶石",
                description = "付费货币，用于购买限定内容",
                maxValue = 999999999
            };
            
            // 命运之印
            _configs[CurrencyType.FateStamp] = new CurrencyConfig
            {
                type = CurrencyType.FateStamp,
                name = "命运之印",
                description = "重复角色转化而来，用于兑换指定角色",
                maxValue = 999
            };
            
            // 体力
            _configs[CurrencyType.Stamina] = new CurrencyConfig
            {
                type = CurrencyType.Stamina,
                name = "体力",
                description = "用于挑战关卡，每6分钟恢复1点",
                maxValue = 100,
                dailyRecoveryAmount = 0,
                dailyRecoveryTimeMinutes = 6
            };
            
            // 友情点
            _configs[CurrencyType.FriendshipPoint] = new CurrencyConfig
            {
                type = CurrencyType.FriendshipPoint,
                name = "友情点",
                description = "与好友互动获得，可用于友情抽卡",
                maxValue = 999999
            };
        }
        
        #endregion
        
        #region 公开方法
        
        /// <summary>
        /// 获取货币数量
        /// </summary>
        public long GetCurrency(CurrencyType type)
        {
            if (_currencies.TryGetValue(type, out var data))
                return data.amount;
            return 0;
        }
        
        /// <summary>
        /// 获取货币配置
        /// </summary>
        public CurrencyConfig GetConfig(CurrencyType type)
        {
            return _configs.TryGetValue(type, out var config) ? config : null;
        }
        
        /// <summary>
        /// 检查货币是否足够
        /// </summary>
        public bool HasEnoughCurrency(CurrencyType type, long amount)
        {
            return GetCurrency(type) >= amount;
        }
        
        /// <summary>
        /// 增加货币
        /// </summary>
        public void AddCurrency(CurrencyType type, long amount)
        {
            if (!_currencies.ContainsKey(type))
                _currencies[type] = new CurrencyData { type = type };
            
            long oldAmount = _currencies[type].amount;
            _currencies[type].amount = Mathf.Min(
                (long)GetMaxValue(type), 
                oldAmount + amount
            );
            
            OnCurrencyChanged?.Invoke(type, oldAmount, _currencies[type].amount);
        }
        
        /// <summary>
        /// 消耗货币
        /// </summary>
        public bool ConsumeCurrency(CurrencyType type, long amount)
        {
            if (!HasEnoughCurrency(type, amount))
            {
                OnCurrencyInsufficient?.Invoke(type, amount - GetCurrency(type));
                return false;
            }
            
            long oldAmount = _currencies[type].amount;
            _currencies[type].amount -= amount;
            
            OnCurrencyChanged?.Invoke(type, oldAmount, _currencies[type].amount);
            return true;
        }
        
        /// <summary>
        /// 设置货币数量
        /// </summary>
        public void SetCurrency(CurrencyType type, long amount)
        {
            if (!_currencies.ContainsKey(type))
                _currencies[type] = new CurrencyData { type = type };
            
            long oldAmount = _currencies[type].amount;
            _currencies[type].amount = Mathf.Clamp(amount, 0, GetMaxValue(type));
            
            OnCurrencyChanged?.Invoke(type, oldAmount, _currencies[type].amount);
        }
        
        /// <summary>
        /// 获取最大货币值
        /// </summary>
        public long GetMaxValue(CurrencyType type)
        {
            if (_configs.TryGetValue(type, out var config))
                return config.maxValue;
            return long.MaxValue;
        }
        
        /// <summary>
        /// 恢复体力（定时恢复）
        /// </summary>
        public void RecoverStamina()
        {
            if (!_configs.TryGetValue(CurrencyType.Stamina, out var config))
                return;
            
            if (!_currencies.ContainsKey(CurrencyType.Stamina))
                _currencies[CurrencyType.Stamina] = new CurrencyData { type = CurrencyType.Stamina };
            
            int maxStamina = config.maxValue;
            long current = _currencies[CurrencyType.Stamina].amount;
            
            if (current < maxStamina)
            {
                long oldAmount = current;
                _currencies[CurrencyType.Stamina].amount = Mathf.Min(maxStamina, current + 1);
                OnCurrencyChanged?.Invoke(CurrencyType.Stamina, oldAmount, _currencies[CurrencyType.Stamina].amount);
            }
        }
        
        /// <summary>
        /// 消耗体力
        /// </summary>
        public bool ConsumeStamina(int amount)
        {
            return ConsumeCurrency(CurrencyType.Stamina, amount);
        }
        
        /// <summary>
        /// 获取体力恢复倒计时（秒）
        /// </summary>
        public int GetStaminaRecoveryTime()
        {
            if (!_currencies.TryGetValue(CurrencyType.Stamina, out var data))
                return 0;
            
            if (data.amount >= GetMaxValue(CurrencyType.Stamina))
                return 0;
            
            // 每6分钟恢复1点
            return 6 * 60;
        }
        
        /// <summary>
        /// 获取货币显示文本
        /// </summary>
        public string GetCurrencyDisplay(CurrencyType type)
        {
            long amount = GetCurrency(type);
            
            if (amount >= 100000000)
                return $"{amount / 100000000}.{((amount % 100000000) / 10000000)}亿";
            if (amount >= 10000)
                return $"{amount / 10000}.{((amount % 10000) / 1000)}万";
            
            return amount.ToString();
        }
        
        #endregion
        
        #region 数据保存/加载
        
        /// <summary>
        /// 保存货币数据
        /// </summary>
        public CurrencySaveData SaveToData()
        {
            return new CurrencySaveData
            {
                currencies = new SerializableDictionary<CurrencyType, CurrencyData>(_currencies),
                lastStaminaRecoveryTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
        
        /// <summary>
        /// 加载货币数据
        /// </summary>
        public void LoadFromData(CurrencySaveData data)
        {
            if (data == null)
                return;
            
            _currencies = new Dictionary<CurrencyType, CurrencyData>(data.currencies);
            
            // 计算离线期间的体力恢复
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsed = currentTime - data.lastStaminaRecoveryTime;
            int staminaRecoveryMinutes = (int)(elapsed / (6 * 60)); // 每6分钟恢复1点
            
            if (staminaRecoveryMinutes > 0)
            {
                RecoverStaminaTimes(staminaRecoveryMinutes);
            }
        }
        
        /// <summary>
        /// 恢复多次体力
        /// </summary>
        private void RecoverStaminaTimes(int times)
        {
            for (int i = 0; i < times; i++)
            {
                RecoverStamina();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 货币保存数据
    /// </summary>
    [Serializable]
    public class CurrencySaveData
    {
        public SerializableDictionary<CurrencyType, CurrencyData> currencies;
        public long lastStaminaRecoveryTime;
    }
    
    /// <summary>
    /// 可序列化的字典
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        
        [SerializeField]
        private List<TValue> values = new List<TValue>();
        
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }
        
        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}
