// CharacterCard.cs - 角色卡片组件
// 在角色列表中显示单个角色

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using JRPG.Character;
using JRPG.UI.Core;

namespace JRPG.UI.Character
{
    /// <summary>
    /// 角色卡片组件
    /// </summary>
    public class CharacterCard : MonoBehaviour
    {
        #region UI References

        [Header("背景")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _frameImage;

        [Header("角色信息")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _powerText;

        [Header("元素图标")]
        [SerializeField] private Image _elementIcon;

        [Header("稀有度")]
        [SerializeField] private Image _rarityBar;
        [SerializeField] private Sprite[] _raritySprites;

        [Header("状态标识")]
        [SerializeField] private GameObject _newBadge;
        [SerializeField] private GameObject _maxLevelBadge;
        [SerializeField] private GameObject _favoriteIcon;

        [Header("交互")]
        [SerializeField] private Button _cardButton;

        #endregion

        #region Fields

        private CharacterData _data;
        private bool _isSelected = false;

        // 事件
        public event Action<CharacterCard> OnCardClicked;

        #endregion

        #region Properties

        /// <summary>
        /// 角色ID
        /// </summary>
        public int CharacterId => _data?.id ?? 0;

        /// <summary>
        /// 角色数据
        /// </summary>
        public CharacterData Data => _data;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (_cardButton != null)
            {
                _cardButton.onClick.RemoveAllListeners();
                _cardButton.onClick.AddListener(OnClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化卡片
        /// </summary>
        public void Initialize(CharacterData data)
        {
            _data = data;

            UpdateAllDisplay();
        }

        /// <summary>
        /// 更新所有显示
        /// </summary>
        public void UpdateAllDisplay()
        {
            if (_data == null) return;

            UpdateBasicInfo();
            UpdateRarity();
            UpdateElement();
            UpdateStatus();
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateSelectionVisual();
        }

        /// <summary>
        /// 设置收藏状态
        /// </summary>
        public void SetFavorite(bool favorite)
        {
            if (_favoriteIcon != null)
            {
                _favoriteIcon.SetActive(favorite);
            }
        }

        #endregion

        #region Private Methods

        private void UpdateBasicInfo()
        {
            if (_nameText != null)
            {
                _nameText.text = _data.name;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lv.{_data.level}";
            }

            if (_powerText != null)
            {
                _powerText.text = _data.power.ToString("N0");
            }

            // 加载头像
            if (_avatarImage != null)
            {
                LoadAvatar();
            }
        }

        private void UpdateRarity()
        {
            if (_rarityBar != null && _raritySprites != null && _raritySprites.Length > 0)
            {
                int rarityIndex = Mathf.Clamp(_data.rarity - 1, 0, _raritySprites.Length - 1);
                _rarityBar.sprite = _raritySprites[rarityIndex];
            }

            // 更新边框颜色
            if (_frameImage != null)
            {
                _frameImage.color = GetRarityColor(_data.rarity);
            }
        }

        private void UpdateElement()
        {
            if (_elementIcon != null)
            {
                // 加载元素图标
                string elementPath = $"UI/Icons/Element/{_data.element}";
                // Sprite icon = Resources.Load<Sprite>(elementPath);
                // _elementIcon.sprite = icon;
            }
        }

        private void UpdateStatus()
        {
            // 新角色标识
            if (_newBadge != null)
            {
                _newBadge.SetActive(_data.isNew);
            }

            // 满级标识
            if (_maxLevelBadge != null)
            {
                bool isMaxLevel = _data.level >= 100; // TODO: 获取角色最大等级
                _maxLevelBadge.SetActive(isMaxLevel);
            }
        }

        private void UpdateSelectionVisual()
        {
            // 更新选中状态视觉效果
            if (_backgroundImage != null)
            {
                Color color = _backgroundImage.color;
                color.a = _isSelected ? 1f : 0.8f;
                _backgroundImage.color = color;
            }

            // 播放动画
            if (_isSelected)
            {
                UITweener.BounceIn(GetComponent<RectTransform>());
            }
        }

        private Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1f, 0.8f, 0.2f); // 金色
                case 4: return new Color(0.6f, 0.4f, 1f);   // 紫色
                case 3: return new Color(0.3f, 0.6f, 1f);  // 蓝色
                default: return Color.white;
            }
        }

        private void LoadAvatar()
        {
            // 异步加载角色头像
            // TODO: 使用资源加载系统
            // string avatarPath = $"Characters/{_data.characterId}/Avatar";
            // Resources.LoadAsync<Sprite>(avatarPath, (sprite) =>
            // {
            //     if (_avatarImage != null)
            //     {
            //         _avatarImage.sprite = sprite;
            //     }
            // });
        }

        private void OnClick()
        {
            // 播放点击音效
            // AudioManager.Instance.PlaySFX("character_select");

            // 播放动画
            UITweener.Pulse(GetComponent<RectTransform>(), 1.1f);

            // 触发回调
            OnCardClicked?.Invoke(this);
        }

        #endregion
    }
}
