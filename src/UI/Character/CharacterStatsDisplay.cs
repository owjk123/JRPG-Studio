// CharacterStatsDisplay.cs - 角色属性展示组件
// 展示角色的详细属性数值

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.Character;

namespace JRPG.UI.Character
{
    /// <summary>
    /// 属性条目数据
    /// </summary>
    [Serializable]
    public class StatEntry
    {
        public string statName;
        public int baseValue;
        public int bonusValue;
        public float percentBonus;
    }

    /// <summary>
    /// 角色属性展示组件
    /// </summary>
    public class CharacterStatsDisplay : MonoBehaviour
    {
        #region UI References

        [Header("基础属性")]
        [SerializeField] private Transform _basicStatsContainer;
        [SerializeField] private StatDisplayItem _hpItem;
        [SerializeField] private StatDisplayItem _atkItem;
        [SerializeField] private StatDisplayItem _defItem;
        [SerializeField] private StatDisplayItem _spdItem;

        [Header("进阶属性")]
        [SerializeField] private Transform _advancedStatsContainer;
        [SerializeField] private StatDisplayItem _critRateItem;
        [SerializeField] private StatDisplayItem _critDmgItem;
        [SerializeField] private StatDisplayItem _atkPercentItem;
        [SerializeField] private StatDisplayItem _defPercentItem;
        [SerializeField] private StatDisplayItem _hpPercentItem;
        [SerializeField] private StatDisplayItem _effectHitItem;
        [SerializeField] private StatDisplayItem _effectResistItem;

        [Header("战力显示")]
        [SerializeField] private TextMeshProUGUI _powerText;
        [SerializeField] private TextMeshProUGUI _powerLabel;

        [Header("对比高亮")]
        [SerializeField] private bool _showDiff = true;
        [SerializeField] private Color _increaseColor = Color.green;
        [SerializeField] private Color _decreaseColor = Color.red;

        [Header("预制体")]
        [SerializeField] private StatDisplayItem _statItemPrefab;

        #endregion

        #region Fields

        private CharacterData _characterData;
        private CharacterData _compareData; // 用于对比的数据
        private Dictionary<StatType, StatDisplayItem> _statItems = new Dictionary<StatType, StatDisplayItem>();

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeStatItems();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            CreateStatItems();
        }

        /// <summary>
        /// 设置角色数据
        /// </summary>
        public void SetCharacter(CharacterData data, CharacterData compareData = null)
        {
            _characterData = data;
            _compareData = compareData;
            UpdateAllStats();
        }

        /// <summary>
        /// 更新所有属性显示
        /// </summary>
        public void UpdateAllStats()
        {
            if (_characterData == null) return;

            UpdateBasicStats();
            UpdateAdvancedStats();
            UpdatePower();
        }

        /// <summary>
        /// 设置对比数据
        /// </summary>
        public void SetCompareData(CharacterData data)
        {
            _compareData = data;
            UpdateAllStats();
        }

        /// <summary>
        /// 清除对比
        /// </summary>
        public void ClearCompare()
        {
            _compareData = null;
            UpdateAllStats();
        }

        /// <summary>
        /// 播放数值变化动画
        /// </summary>
        public void PlayStatChangeAnimation(StatType statType)
        {
            if (_statItems.TryGetValue(statType, out var item))
            {
                // TODO: 播放变化动画
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化属性条目
        /// </summary>
        private void InitializeStatItems()
        {
            // 建立属性和UI项的映射
            _statItems[StatType.HP] = _hpItem;
            _statItems[StatType.ATK] = _atkItem;
            _statItems[StatType.DEF] = _defItem;
            _statItems[StatType.SPD] = _spdItem;
            _statItems[StatType.CritRate] = _critRateItem;
            _statItems[StatType.CritDmg] = _critDmgItem;
            _statItems[StatType.AtkPercent] = _atkPercentItem;
            _statItems[StatType.DefPercent] = _defPercentItem;
            _statItems[StatType.HpPercent] = _hpPercentItem;
            _statItems[StatType.EffectHit] = _effectHitItem;
            _statItems[StatType.EffectResist] = _effectResistItem;
        }

        /// <summary>
        /// 创建属性条目
        /// </summary>
        private void CreateStatItems()
        {
            // 如果预制体存在但UI未配置，自动创建
            if (_statItemPrefab != null && _basicStatsContainer != null)
            {
                CreateStatItemIfMissing(StatType.HP, _basicStatsContainer);
                CreateStatItemIfMissing(StatType.ATK, _basicStatsContainer);
                CreateStatItemIfMissing(StatType.DEF, _basicStatsContainer);
                CreateStatItemIfMissing(StatType.SPD, _basicStatsContainer);
            }
        }

        private void CreateStatItemIfMissing(StatType type, Transform container)
        {
            if (!_statItems.ContainsKey(type) || _statItems[type] == null)
            {
                var itemObj = Instantiate(_statItemPrefab.gameObject, container);
                var item = itemObj.GetComponent<StatDisplayItem>();
                if (item != null)
                {
                    item.Initialize(type, GetStatName(type));
                    _statItems[type] = item;
                }
            }
        }

        /// <summary>
        /// 更新基础属性
        /// </summary>
        private void UpdateBasicStats()
        {
            if (_characterData == null) return;

            // 获取角色属性
            var stats = CalculateStats();

            // 更新显示
            UpdateStatDisplay(StatType.HP, stats.hp);
            UpdateStatDisplay(StatType.ATK, stats.atk);
            UpdateStatDisplay(StatType.DEF, stats.def);
            UpdateStatDisplay(StatType.SPD, stats.spd);
        }

        /// <summary>
        /// 更新进阶属性
        /// </summary>
        private void UpdateAdvancedStats()
        {
            if (_characterData == null) return;

            var advancedStats = CalculateAdvancedStats();

            UpdateStatDisplay(StatType.CritRate, advancedStats.critRate, true);
            UpdateStatDisplay(StatType.CritDmg, advancedStats.critDmg, true);
            UpdateStatDisplay(StatType.AtkPercent, advancedStats.atkPercent, true);
            UpdateStatDisplay(StatType.DefPercent, advancedStats.defPercent, true);
            UpdateStatDisplay(StatType.HpPercent, advancedStats.hpPercent, true);
            UpdateStatDisplay(StatType.EffectHit, advancedStats.effectHit, true);
            UpdateStatDisplay(StatType.EffectResist, advancedStats.effectResist, true);
        }

        /// <summary>
        /// 更新战力显示
        /// </summary>
        private void UpdatePower()
        {
            if (_powerText != null)
            {
                _powerText.text = _characterData.power.ToString("N0");
            }
        }

        /// <summary>
        /// 更新单个属性显示
        /// </summary>
        private void UpdateStatDisplay(StatType type, int value, bool isPercent = false)
        {
            if (!_statItems.TryGetValue(type, out var item) || item == null) return;

            item.SetValue(value, isPercent);

            // 如果有对比数据，显示差异
            if (_showDiff && _compareData != null)
            {
                var compareStats = CalculateStats(_compareData);
                int compareValue = GetStatValue(type, compareStats);
                item.SetDifference(value - compareValue, isPercent);
            }
        }

        /// <summary>
        /// 计算角色属性
        /// </summary>
        private BasicStats CalculateStats(CharacterData data = null)
        {
            data = data ?? _characterData;

            BasicStats stats = new BasicStats();

            // TODO: 根据角色等级、稀有度等计算基础属性
            // 基础公式：baseValue * (1 + levelBonus) * (1 + rarityBonus)

            stats.hp = 1000 + data.level * 100;
            stats.atk = 50 + data.level * 10;
            stats.def = 30 + data.level * 5;
            stats.spd = 80 + data.level * 2;

            return stats;
        }

        /// <summary>
        /// 计算进阶属性
        /// </summary>
        private AdvancedStats CalculateAdvancedStats(CharacterData data = null)
        {
            data = data ?? _characterData;

            AdvancedStats stats = new AdvancedStats();

            // TODO: 从装备、技能等获取加成
            stats.critRate = 5;
            stats.critDmg = 150;
            stats.atkPercent = 0;
            stats.defPercent = 0;
            stats.hpPercent = 0;
            stats.effectHit = 0;
            stats.effectResist = 0;

            return stats;
        }

        private int GetStatValue(StatType type, BasicStats stats)
        {
            switch (type)
            {
                case StatType.HP: return stats.hp;
                case StatType.ATK: return stats.atk;
                case StatType.DEF: return stats.def;
                case StatType.SPD: return stats.spd;
                default: return 0;
            }
        }

        private string GetStatName(StatType type)
        {
            switch (type)
            {
                case StatType.HP: return "生命";
                case StatType.ATK: return "攻击";
                case StatType.DEF: return "防御";
                case StatType.SPD: return "速度";
                case StatType.CritRate: return "暴击率";
                case StatType.CritDmg: return "暴击伤害";
                case StatType.AtkPercent: return "攻击加成";
                case StatType.DefPercent: return "防御加成";
                case StatType.HpPercent: return "生命加成";
                case StatType.EffectHit: return "效果命中";
                case StatType.EffectResist: return "效果抵抗";
                default: return type.ToString();
            }
        }

        #endregion

        #region Inner Types

        private enum StatType
        {
            HP, ATK, DEF, SPD,
            CritRate, CritDmg,
            AtkPercent, DefPercent, HpPercent,
            EffectHit, EffectResist
        }

        private struct BasicStats
        {
            public int hp, atk, def, spd;
        }

        private struct AdvancedStats
        {
            public int critRate, critDmg;
            public int atkPercent, defPercent, hpPercent;
            public int effectHit, effectResist;
        }

        #endregion
    }

    /// <summary>
    /// 属性显示条目组件
    /// </summary>
    public class StatDisplayItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _bonusText;
        [SerializeField] private TextMeshProUGUI _diffText;
        [SerializeField] private Image _iconImage;

        #endregion

        #region Fields

        private string _statName;
        private int _baseValue;
        private int _bonusValue;
        private bool _isPercent;

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(string statName)
        {
            _statName = statName;
            if (_nameText != null)
            {
                _nameText.text = statName;
            }
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void SetValue(int value, bool isPercent = false)
        {
            _baseValue = value;
            _isPercent = isPercent;

            if (_valueText != null)
            {
                if (isPercent)
                {
                    _valueText.text = $"{value}%";
                }
                else
                {
                    _valueText.text = value.ToString("N0");
                }
            }
        }

        /// <summary>
        /// 设置加成值
        /// </summary>
        public void SetBonus(int bonus)
        {
            _bonusValue = bonus;

            if (_bonusText != null)
            {
                if (bonus > 0)
                {
                    _bonusText.text = $"+{bonus}";
                    _bonusText.gameObject.SetActive(true);
                }
                else
                {
                    _bonusText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 设置差异值
        /// </summary>
        public void SetDifference(int diff, bool isPercent = false)
        {
            if (_diffText != null)
            {
                if (diff > 0)
                {
                    _diffText.text = isPercent ? $"+{diff}%" : $"+{diff}";
                    _diffText.color = Color.green;
                    _diffText.gameObject.SetActive(true);
                }
                else if (diff < 0)
                {
                    _diffText.text = isPercent ? $"{diff}%" : $"{diff}";
                    _diffText.color = Color.red;
                    _diffText.gameObject.SetActive(true);
                }
                else
                {
                    _diffText.gameObject.SetActive(false);
                }
            }
        }

        #endregion
    }
}
