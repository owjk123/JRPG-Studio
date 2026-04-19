// GachaResultPanel.cs - 抽卡结果展示面板
// 展示抽卡结果

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using JRPG.UI.Common;
using JRPG.UI.Core;

namespace JRPG.UI.Gacha
{
    /// <summary>
    /// 抽卡结果展示面板
    /// </summary>
    public class GachaResultPanel : BasePanel
    {
        #region UI References

        [Header("结果列表")]
        [SerializeField] private ScrollRect _resultScrollRect;
        [SerializeField] private GridLayoutGroup _resultGrid;
        [SerializeField] private GameObject _resultCardPrefab;

        [Header("统计信息")]
        [SerializeField] private TextMeshProUGUI _totalCountText;
        [SerializeField] private TextMeshProUGUI _fiveStarCountText;
        [SerializeField] private TextMeshProUGUI _fourStarCountText;
        [SerializeField] private TextMeshProUGUI _threeStarCountText;

        [Header("按钮")]
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _shareButton;

        [Header("保底信息")]
        [SerializeField] private TextMeshProUGUI _pityInfoText;

        #endregion

        #region Fields

        private List<GachaResultData> _results = new List<GachaResultData>();
        private int _fiveStarCount = 0;
        private int _fourStarCount = 0;
        private int _threeStarCount = 0;

        // 事件
        public event Action OnAgainClicked;
        public event Action OnCloseClicked;
        public event Action OnShareClicked;

        #endregion

        #region BasePanel Override

        protected override void Awake()
        {
            base.Awake();
            _layer = PanelLayer.Popup;
            _useOpenAnimation = true;
        }

        protected override void Start()
        {
            base.Start();
            SetupButtons();
        }

        protected override void OnPanelShow()
        {
            base.OnPanelShow();
            UpdateDisplay();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 设置抽卡结果
        /// </summary>
        public void SetResults(List<GachaResultData> results)
        {
            _results = results;

            // 统计各稀有度数量
            _fiveStarCount = 0;
            _fourStarCount = 0;
            _threeStarCount = 0;

            foreach (var result in results)
            {
                switch (result.rarity)
                {
                    case 5:
                        _fiveStarCount++;
                        break;
                    case 4:
                        _fourStarCount++;
                        break;
                    case 3:
                        _threeStarCount++;
                        break;
                }
            }
        }

        /// <summary>
        /// 添加结果
        /// </summary>
        public void AddResult(GachaResultData result)
        {
            _results.Add(result);

            // 更新统计
            switch (result.rarity)
            {
                case 5: _fiveStarCount++; break;
                case 4: _fourStarCount++; break;
                case 3: _threeStarCount++; break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            CreateResultCards();
            UpdateStatistics();
        }

        /// <summary>
        /// 创建结果卡牌
        /// </summary>
        private void CreateResultCards()
        {
            if (_resultGrid == null) return;

            // 清空现有
            foreach (Transform child in _resultGrid.transform)
            {
                Destroy(child.gameObject);
            }

            // 按稀有度排序（高稀有度在前）
            _results.Sort((a, b) => b.rarity.CompareTo(a.rarity));

            // 创建卡牌
            foreach (var result in _results)
            {
                CreateResultCard(result);
            }
        }

        /// <summary>
        /// 创建单个结果卡牌
        /// </summary>
        private void CreateResultCard(GachaResultData result)
        {
            GameObject cardObj;

            if (_resultCardPrefab != null)
            {
                cardObj = Instantiate(_resultCardPrefab, _resultGrid.transform);
            }
            else
            {
                cardObj = CreateDefaultCard(result);
            }

            var card = cardObj.GetComponent<GachaResultCard>();
            if (card != null)
            {
                card.Initialize(result);
            }

            // 入场动画
            UITweener.PopIn(cardObj.GetComponent<RectTransform>(), 0.3f);
        }

        /// <summary>
        /// 创建默认卡牌
        /// </summary>
        private GameObject CreateDefaultCard(GachaResultData result)
        {
            GameObject cardObj = new GameObject("ResultCard");
            cardObj.transform.SetParent(_resultGrid?.transform);

            RectTransform rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 200);

            var card = cardObj.AddComponent<GachaResultCard>();
            return cardObj;
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics()
        {
            if (_totalCountText != null)
            {
                _totalCountText.text = $"共 {_results.Count} 抽";
            }

            if (_fiveStarCountText != null)
            {
                _fiveStarCountText.text = _fiveStarCount.ToString();
            }

            if (_fourStarCountText != null)
            {
                _fourStarCountText.text = _fourStarCount.ToString();
            }

            if (_threeStarCountText != null)
            {
                _threeStarCountText.text = _threeStarCount.ToString();
            }
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_againButton != null)
            {
                _againButton.onClick.RemoveAllListeners();
                _againButton.onClick.AddListener(OnAgainClick);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(OnCloseClick);
            }

            if (_shareButton != null)
            {
                _shareButton.onClick.RemoveAllListeners();
                _shareButton.onClick.AddListener(OnShareClick);
            }
        }

        #endregion

        #region Event Handlers

        private void OnAgainClick()
        {
            OnAgainClicked?.Invoke();
        }

        private void OnCloseClick()
        {
            OnCloseClicked?.Invoke();
            Hide();
        }

        private void OnShareClick()
        {
            OnShareClicked?.Invoke();
        }

        #endregion
    }
}
