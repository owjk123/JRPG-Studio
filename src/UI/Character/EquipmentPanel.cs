// EquipmentPanel.cs - 装备界面组件
// 展示和管理角色的装备

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;
using JRPG.Character;

namespace JRPG.UI.Character
{
    /// <summary>
    /// 装备槽位类型
    /// </summary>
    public enum EquipmentSlotType
    {
        Weapon = 0,      // 武器
        Armor = 1,       // 护甲
        Accessory = 2,   // 饰品
        Special = 3      // 特殊装备
    }

    /// <summary>
    /// 装备数据
    /// </summary>
    [Serializable]
    public class EquipmentData
    {
        public int id;
        public string name;
        public string iconPath;
        public EquipmentSlotType slotType;
        public int level;
        public int rarity;
        public int hpBonus;
        public int atkBonus;
        public int defBonus;
    }

    /// <summary>
    /// 装备面板组件
    /// </summary>
    public class EquipmentPanel : BasePanel
    {
        #region UI References

        [Header("装备槽位")]
        [SerializeField] private Transform _equipmentSlotsContainer;
        [SerializeField] private EquipmentSlot[] _equipmentSlots;

        [Header("选中装备信息")]
        [SerializeField] private Image _selectedIcon;
        [SerializeField] private TextMeshProUGUI _selectedNameText;
        [SerializeField] private TextMeshProUGUI _selectedLevelText;
        [SerializeField] private TextMeshProUGUI _selectedStatsText;
        [SerializeField] private TextMeshProUGUI _selectedDescText;

        [Header("按钮")]
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _lockButton;

        [Header("对比面板")]
        [SerializeField] private GameObject _comparePanel;
        [SerializeField] private TextMeshProUGUI _compareStatsText;

        [Header("背包列表")]
        [SerializeField] private ScrollRect _inventoryScrollRect;
        [SerializeField] private GridLayoutGroup _inventoryGrid;
        [SerializeField] private GameObject _inventoryItemPrefab;

        #endregion

        #region Fields

        private CharacterData _characterData;
        private Dictionary<EquipmentSlotType, EquipmentData> _equippedItems = new Dictionary<EquipmentSlotType, EquipmentData>();
        private List<EquipmentData> _inventoryItems = new List<EquipmentData>();
        private EquipmentData _selectedEquipment;
        private EquipmentSlot _selectedSlot;

        // 事件
        public event Action<EquipmentData, EquipmentSlotType> OnEquip;
        public event Action<EquipmentData> OnUnequip;
        public event Action<EquipmentData> OnUpgrade;

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
            // 初始化装备槽位
            InitializeSlots();
            // 加载背包数据
            LoadInventory();
        }

        /// <summary>
        /// 设置角色
        /// </summary>
        public void SetCharacter(CharacterData data)
        {
            _characterData = data;
            LoadEquippedItems();
            UpdateDisplay();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            LoadEquippedItems();
            LoadInventory();
            UpdateDisplay();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化槽位
        /// </summary>
        private void InitializeSlots()
        {
            if (_equipmentSlots == null || _equipmentSlots.Length == 0)
            {
                // 自动创建槽位
                Array slotTypes = Enum.GetValues(typeof(EquipmentSlotType));
                _equipmentSlots = new EquipmentSlot[slotTypes.Length];

                for (int i = 0; i < slotTypes.Length; i++)
                {
                    CreateEquipmentSlot((EquipmentSlotType)slotTypes.GetValue(i), i);
                }
            }
            else
            {
                for (int i = 0; i < _equipmentSlots.Length; i++)
                {
                    if (_equipmentSlots[i] != null)
                    {
                        _equipmentSlots[i].Initialize((EquipmentSlotType)i);
                        _equipmentSlots[i].OnSlotClicked += OnSlotClicked;
                    }
                }
            }
        }

        /// <summary>
        /// 创建装备槽位
        /// </summary>
        private void CreateEquipmentSlot(EquipmentSlotType slotType, int index)
        {
            // TODO: 从预制体创建
            Debug.Log($"[EquipmentPanel] 创建槽位: {slotType}");
        }

        /// <summary>
        /// 加载已装备物品
        /// </summary>
        private void LoadEquippedItems()
        {
            _equippedItems.Clear();

            // TODO: 从角色数据加载装备
            // 模拟数据
            _equippedItems[EquipmentSlotType.Weapon] = new EquipmentData
            {
                id = 1,
                name = "炎龙剑",
                slotType = EquipmentSlotType.Weapon,
                level = 20,
                rarity = 5,
                atkBonus = 500
            };

            _equippedItems[EquipmentSlotType.Armor] = new EquipmentData
            {
                id = 2,
                name = "圣光护甲",
                slotType = EquipmentSlotType.Armor,
                level = 15,
                rarity = 4,
                defBonus = 300,
                hpBonus = 2000
            };
        }

        /// <summary>
        /// 加载背包
        /// </summary>
        private void LoadInventory()
        {
            _inventoryItems.Clear();

            // TODO: 从背包数据加载
            // 模拟数据
            for (int i = 0; i < 10; i++)
            {
                _inventoryItems.Add(new EquipmentData
                {
                    id = 100 + i,
                    name = $"装备{i + 1}",
                    slotType = (EquipmentSlotType)(i % 4),
                    level = 10 + i,
                    rarity = 3 + (i % 3),
                    atkBonus = 100 + i * 20
                });
            }
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            UpdateSlotsDisplay();
            UpdateInventoryDisplay();

            if (_selectedEquipment != null)
            {
                UpdateSelectedDisplay();
            }
            else
            {
                ClearSelectedDisplay();
            }
        }

        /// <summary>
        /// 更新槽位显示
        /// </summary>
        private void UpdateSlotsDisplay()
        {
            foreach (var slot in _equipmentSlots)
            {
                if (slot != null)
                {
                    EquipmentData equipped = null;
                    _equippedItems.TryGetValue(slot.SlotType, out equipped);
                    slot.SetEquipment(equipped);
                }
            }
        }

        /// <summary>
        /// 更新背包显示
        /// </summary>
        private void UpdateInventoryDisplay()
        {
            // 清空现有物品
            ClearInventoryDisplay();

            // 创建物品
            foreach (var item in _inventoryItems)
            {
                CreateInventoryItem(item);
            }
        }

        /// <summary>
        /// 清空背包显示
        /// </summary>
        private void ClearInventoryDisplay()
        {
            if (_inventoryGrid == null) return;

            foreach (Transform child in _inventoryGrid.transform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 创建背包物品
        /// </summary>
        private void CreateInventoryItem(EquipmentData data)
        {
            var itemObj = Instantiate(_inventoryItemPrefab, _inventoryGrid?.transform);
            var item = itemObj.GetComponent<EquipmentInventoryItem>();

            if (item != null)
            {
                item.Initialize(data);
                item.OnItemClicked += OnInventoryItemClicked;
            }
        }

        /// <summary>
        /// 更新选中物品显示
        /// </summary>
        private void UpdateSelectedDisplay()
        {
            if (_selectedEquipment == null) return;

            if (_selectedNameText != null)
            {
                _selectedNameText.text = _selectedEquipment.name;
            }

            if (_selectedLevelText != null)
            {
                _selectedLevelText.text = $"+{_selectedEquipment.level}";
            }

            if (_selectedStatsText != null)
            {
                _selectedStatsText.text = GenerateStatsText(_selectedEquipment);
            }

            // 更新按钮状态
            UpdateButtonStates();
        }

        /// <summary>
        /// 清空选中显示
        /// </summary>
        private void ClearSelectedDisplay()
        {
            if (_selectedNameText != null) _selectedNameText.text = "";
            if (_selectedLevelText != null) _selectedLevelText.text = "";
            if (_selectedStatsText != null) _selectedStatsText.text = "";
            if (_selectedDescText != null) _selectedDescText.text = "";
        }

        /// <summary>
        /// 生成属性文本
        /// </summary>
        private string GenerateStatsText(EquipmentData data)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (data.atkBonus > 0)
                sb.AppendLine($"攻击 +{data.atkBonus}");
            if (data.defBonus > 0)
                sb.AppendLine($"防御 +{data.defBonus}");
            if (data.hpBonus > 0)
                sb.AppendLine($"生命 +{data.hpBonus}");

            return sb.ToString();
        }

        /// <summary>
        /// 更新按钮状态
        /// </summary>
        private void UpdateButtonStates()
        {
            if (_selectedEquipment == null)
            {
                if (_equipButton != null) _equipButton.interactable = false;
                if (_unequipButton != null) _unequipButton.interactable = false;
                if (_upgradeButton != null) _upgradeButton.interactable = false;
                return;
            }

            // 检查是否已装备
            bool isEquipped = false;
            EquipmentSlotType? slotOfEquipped = null;

            foreach (var kvp in _equippedItems)
            {
                if (kvp.Value.id == _selectedEquipment.id)
                {
                    isEquipped = true;
                    slotOfEquipped = kvp.Key;
                    break;
                }
            }

            if (_equipButton != null)
            {
                _equipButton.interactable = !isEquipped;
            }

            if (_unequipButton != null)
            {
                _unequipButton.interactable = isEquipped;
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = true; // TODO: 检查是否可升级
            }
        }

        #endregion

        #region Event Handlers

        private void OnSlotClicked(EquipmentSlot slot)
        {
            _selectedSlot = slot;
            _selectedEquipment = slot.EquippedEquipment;
            UpdateSelectedDisplay();
        }

        private void OnInventoryItemClicked(EquipmentInventoryItem item)
        {
            _selectedEquipment = item.Data;
            _selectedSlot = null;
            UpdateSelectedDisplay();
        }

        private void SetupButtons()
        {
            if (_equipButton != null)
            {
                _equipButton.onClick.RemoveAllListeners();
                _equipButton.onClick.AddListener(OnEquipClick);
            }

            if (_unequipButton != null)
            {
                _unequipButton.onClick.RemoveAllListeners();
                _unequipButton.onClick.AddListener(OnUnequipClick);
            }

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveAllListeners();
                _upgradeButton.onClick.AddListener(OnUpgradeClick);
            }
        }

        private void OnEquipClick()
        {
            if (_selectedEquipment == null || _selectedSlot == null) return;

            OnEquip?.Invoke(_selectedEquipment, _selectedSlot.SlotType);

            // 更新装备
            _equippedItems[_selectedSlot.SlotType] = _selectedEquipment;
            _inventoryItems.Remove(_selectedEquipment);
            UpdateDisplay();
        }

        private void OnUnequipClick()
        {
            if (_selectedEquipment == null) return;

            OnUnequip?.Invoke(_selectedEquipment);

            // 卸下装备
            EquipmentSlotType? slotToRemove = null;
            foreach (var kvp in _equippedItems)
            {
                if (kvp.Value.id == _selectedEquipment.id)
                {
                    slotToRemove = kvp.Key;
                    break;
                }
            }

            if (slotToRemove.HasValue)
            {
                _equippedItems.Remove(slotToRemove.Value);
                _inventoryItems.Add(_selectedEquipment);
                _selectedEquipment = null;
                UpdateDisplay();
            }
        }

        private void OnUpgradeClick()
        {
            if (_selectedEquipment == null) return;

            OnUpgrade?.Invoke(_selectedEquipment);

            // 升级装备
            _selectedEquipment.level++;
            UpdateSelectedDisplay();
        }

        #endregion
    }

    /// <summary>
    /// 装备槽位组件
    /// </summary>
    public class EquipmentSlot : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Image _slotIcon;
        [SerializeField] private Image _equippedIcon;
        [SerializeField] private Image _slotTypeIcon;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _emptyIndicator;
        [SerializeField] private GameObject _lockIndicator;
        [SerializeField] private Button _slotButton;

        #endregion

        #region Fields

        private EquipmentSlotType _slotType;
        private EquipmentData _equippedEquipment;

        // 事件
        public event Action<EquipmentSlot> OnSlotClicked;

        #endregion

        #region Properties

        public EquipmentSlotType SlotType => _slotType;
        public EquipmentData EquippedEquipment => _equippedEquipment;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveAllListeners();
                _slotButton.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(EquipmentSlotType slotType)
        {
            _slotType = slotType;
            UpdateDisplay();
        }

        /// <summary>
        /// 设置装备
        /// </summary>
        public void SetEquipment(EquipmentData equipment)
        {
            _equippedEquipment = equipment;
            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            bool hasEquipment = _equippedEquipment != null;

            if (_equippedIcon != null)
            {
                _equippedIcon.gameObject.SetActive(hasEquipment);
            }

            if (_emptyIndicator != null)
            {
                _emptyIndicator.SetActive(!hasEquipment);
            }

            if (_levelText != null && hasEquipment)
            {
                _levelText.text = $"+{_equippedEquipment.level}";
            }
        }

        #endregion

        #region Private Methods

        private void OnClick()
        {
            OnSlotClicked?.Invoke(this);
        }

        #endregion
    }

    /// <summary>
    /// 背包物品组件
    /// </summary>
    public class EquipmentInventoryItem : MonoBehaviour
    {
        #region UI References

        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityFrame;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _itemButton;

        #endregion

        #region Fields

        private EquipmentData _data;

        // 事件
        public event Action<EquipmentInventoryItem> OnItemClicked;

        #endregion

        #region Properties

        public EquipmentData Data => _data;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_itemButton != null)
            {
                _itemButton.onClick.RemoveAllListeners();
                _itemButton.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(EquipmentData data)
        {
            _data = data;

            if (_levelText != null)
            {
                _levelText.text = $"+{data.level}";
            }

            // 设置稀有度颜色
            if (_rarityFrame != null)
            {
                _rarityFrame.color = GetRarityColor(data.rarity);
            }
        }

        private Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1f, 0.8f, 0.2f);
                case 4: return new Color(0.6f, 0.4f, 1f);
                case 3: return new Color(0.3f, 0.6f, 1f);
                default: return Color.white;
            }
        }

        private void OnClick()
        {
            OnItemClicked?.Invoke(this);
        }

        #endregion
    }
}
