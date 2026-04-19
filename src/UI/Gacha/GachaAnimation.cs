// GachaAnimation.cs - 抽卡动画控制器
// 控制抽卡过程中的各种动画效果

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using JRPG.UI.Core;
using JRPG.Character;

namespace JRPG.UI.Gacha
{
    /// <summary>
    /// 抽卡动画阶段
    /// </summary>
    public enum GachaAnimationPhase
    {
        Idle,           // 待机
        Prepare,        // 准备
        Rolling,        // 滚动
        Reveal,         // 揭晓
        Celebration,    // 庆祝
        Complete        // 完成
    }

    /// <summary>
    /// 抽卡结果数据
    /// </summary>
    [Serializable]
    public class GachaResultData
    {
        public int id;
        public string name;
        public int rarity;          // 3=普通, 4=稀有, 5=传说
        public bool isNew;          // 是否新获得
        public bool isRateUp;       // 是否为UP角色
        public Sprite icon;
        public CharacterData characterData;
    }

    /// <summary>
    /// 抽卡动画控制器
    /// </summary>
    public class GachaAnimation : MonoBehaviour
    {
        #region UI References

        [Header("背景")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private ParticleSystem _backgroundParticles;

        [Header("中央展示")]
        [SerializeField] private Image _centerDisplayImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private ParticleSystem _revealEffect;

        [Header("卡牌滚动区")]
        [SerializeField] private RectTransform _cardScrollArea;
        [SerializeField] private Image _cardTemplate;
        [SerializeField] private List<Image> _rollingCards = new List<Image>();

        [Header("结果列表")]
        [SerializeField] private Transform _resultContainer;
        [SerializeField] private GameObject _resultCardPrefab;

        [Header("按钮")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;

        [Header("特效")]
        [SerializeField] private GameObject _fiveStarEffect;
        [SerializeField] private GameObject _fourStarEffect;
        [SerializeField] private GameObject _rateUpEffect;
        [SerializeField] private AudioSource _gachaAudioSource;

        [Header("动画配置")]
        [SerializeField] private float _cardRollDuration = 2f;
        [SerializeField] private float _revealDelay = 0.5f;
        [SerializeField] private float _celebrationDuration = 2f;

        #endregion

        #region Fields

        private GachaAnimationPhase _currentPhase = GachaAnimationPhase.Idle;
        private List<GachaResultData> _results = new List<GachaResultData>();
        private int _currentRevealIndex = 0;
        private bool _isAnimating = false;
        private Action _onAnimationComplete;
        private Action _onSkipRequested;

        // 动画数据
        private float _animationTimer = 0f;
        private Vector3 _originalCardPosition;

        #endregion

        #region Properties

        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        public bool IsAnimating => _isAnimating;

        /// <summary>
        /// 当前阶段
        /// </summary>
        public GachaAnimationPhase CurrentPhase => _currentPhase;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            SetupButtons();
            Initialize();
        }

        protected virtual void Update()
        {
            if (!_isAnimating) return;

            _animationTimer += Time.deltaTime;
            UpdateAnimation();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            _currentPhase = GachaAnimationPhase.Idle;
            _isAnimating = false;

            // 初始化卡牌
            InitializeCards();
        }

        /// <summary>
        /// 播放抽卡动画
        /// </summary>
        public void PlayAnimation(List<GachaResultData> results, Action onComplete, Action onSkip = null)
        {
            if (_isAnimating) return;

            _results = results;
            _onAnimationComplete = onComplete;
            _onSkipRequested = onSkip;
            _currentRevealIndex = 0;

            // 显示界面
            gameObject.SetActive(true);

            // 开始动画序列
            StartCoroutine(AnimationSequence());
        }

        /// <summary>
        /// 跳过动画
        /// </summary>
        public void SkipAnimation()
        {
            if (!_isAnimating) return;

            _onSkipRequested?.Invoke();

            // 直接显示所有结果
            ShowAllResults();
        }

        /// <summary>
        /// 结束动画
        /// </summary>
        public void EndAnimation()
        {
            _isAnimating = false;
            _currentPhase = GachaAnimationPhase.Complete;

            // 隐藏特效
            HideAllEffects();

            // 回调
            _onAnimationComplete?.Invoke();
            _onAnimationComplete = null;

            // 隐藏界面
            gameObject.SetActive(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 初始化卡牌
        /// </summary>
        private void InitializeCards()
        {
            // 清空现有卡牌
            foreach (var card in _rollingCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _rollingCards.Clear();
        }

        /// <summary>
        /// 设置按钮
        /// </summary>
        private void SetupButtons()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
                _continueButton.onClick.AddListener(OnContinueClick);
                _continueButton.gameObject.SetActive(false);
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.RemoveAllListeners();
                _skipButton.onClick.AddListener(OnSkipClick);
            }
        }

        /// <summary>
        /// 动画序列
        /// </summary>
        private IEnumerator AnimationSequence()
        {
            _isAnimating = true;
            _currentPhase = GachaAnimationPhase.Prepare;

            // 准备阶段
            yield return new WaitForSeconds(0.5f);

            // 开始滚动
            _currentPhase = GachaAnimationPhase.Rolling;
            PlayRollingAnimation();

            // 滚动持续时间
            yield return new WaitForSeconds(_cardRollDuration);

            // 揭晓阶段
            _currentPhase = GachaAnimationPhase.Reveal;
            yield return StartCoroutine(RevealSequence());

            // 庆祝阶段
            _currentPhase = GachaAnimationPhase.Celebration;
            PlayCelebrationEffects();
            yield return new WaitForSeconds(_celebrationDuration);

            // 完成
            _currentPhase = GachaAnimationPhase.Complete;
            _isAnimating = false;

            // 显示继续按钮
            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 播放滚动动画
        /// </summary>
        private void PlayRollingAnimation()
        {
            // 创建滚动卡牌
            for (int i = 0; i < 20; i++)
            {
                StartCoroutine(CreateRollingCard(i));
            }
        }

        private IEnumerator CreateRollingCard(int index)
        {
            yield return new WaitForSeconds(index * 0.05f);

            if (_cardTemplate == null || _cardScrollArea == null) yield break;

            var card = Instantiate(_cardTemplate, _cardScrollArea);
            card.gameObject.SetActive(true);

            // 设置随机模糊卡面
            card.color = GetRandomRarityColor(UnityEngine.Random.Range(3, 6));

            // 动画：从上到下滚动
            RectTransform rect = card.GetComponent<RectTransform>();
            Vector3 startPos = new Vector3(UnityEngine.Random.Range(-200, 200), 500, 0);
            Vector3 endPos = new Vector3(UnityEngine.Random.Range(-200, 200), -500, 0);

            rect.anchoredPosition = startPos;
            rect.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-15, 15));

            UITweener.TweenPosition(rect, startPos, endPos, _cardRollDuration / 2f, null, () =>
            {
                Destroy(card.gameObject);
            });

            _rollingCards.Add(card);
        }

        /// <summary>
        /// 揭晓序列
        /// </summary>
        private IEnumerator RevealSequence()
        {
            // 按稀有度从低到高揭晓
            _results.Sort((a, b) => a.rarity.CompareTo(b.rarity));

            foreach (var result in _results)
            {
                RevealCard(result);
                yield return new WaitForSeconds(_revealDelay);
            }
        }

        /// <summary>
        /// 揭晓卡牌
        /// </summary>
        private void RevealCard(GachaResultData result)
        {
            // 播放揭晓特效
            if (_revealEffect != null)
            {
                _revealEffect.Play();
            }

            // 更新中央展示
            if (_centerDisplayImage != null && result.icon != null)
            {
                _centerDisplayImage.sprite = result.icon;
            }

            if (_characterNameText != null)
            {
                _characterNameText.text = result.name;
            }

            // 根据稀有度播放不同效果
            if (result.rarity >= 5)
            {
                ShowFiveStarEffect(result);
            }
            else if (result.rarity >= 4)
            {
                ShowFourStarEffect(result);
            }

            // 添加到结果列表
            CreateResultCard(result);
        }

        /// <summary>
        /// 显示五星特效
        /// </summary>
        private void ShowFiveStarEffect(GachaResultData result)
        {
            if (_fiveStarEffect != null)
            {
                _fiveStarEffect.SetActive(true);
                // 播放特效动画
                UITweener.BounceIn(_fiveStarEffect.GetComponent<RectTransform>(), 0.5f);
            }

            if (result.isRateUp && _rateUpEffect != null)
            {
                _rateUpEffect.SetActive(true);
            }
        }

        /// <summary>
        /// 显示四星特效
        /// </summary>
        private void ShowFourStarEffect(GachaResultData result)
        {
            if (_fourStarEffect != null)
            {
                _fourStarEffect.SetActive(true);
            }
        }

        /// <summary>
        /// 创建结果卡牌
        /// </summary>
        private void CreateResultCard(GachaResultData result)
        {
            if (_resultCardPrefab == null || _resultContainer == null) return;

            var cardObj = Instantiate(_resultCardPrefab, _resultContainer);
            var card = cardObj.GetComponent<GachaResultCard>();

            if (card != null)
            {
                card.Initialize(result);
            }
        }

        /// <summary>
        /// 显示所有结果（跳过动画时调用）
        /// </summary>
        private void ShowAllResults()
        {
            // 停止当前动画
            StopAllCoroutines();
            _isAnimating = false;

            // 隐藏滚动区域
            if (_cardScrollArea != null)
            {
                _cardScrollArea.gameObject.SetActive(false);
            }

            // 显示所有结果
            foreach (var result in _results)
            {
                RevealCard(result);
            }

            // 检查是否有五星
            bool hasFiveStar = false;
            foreach (var result in _results)
            {
                if (result.rarity >= 5)
                {
                    hasFiveStar = true;
                    break;
                }
            }

            if (hasFiveStar)
            {
                ShowFiveStarEffect(null);
            }

            _currentPhase = GachaAnimationPhase.Complete;

            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 播放庆祝特效
        /// </summary>
        private void PlayCelebrationEffects()
        {
            // 检查是否有五星
            bool hasFiveStar = false;
            foreach (var result in _results)
            {
                if (result.rarity >= 5)
                {
                    hasFiveStar = true;
                    break;
                }
            }

            if (hasFiveStar)
            {
                // 播放背景粒子
                if (_backgroundParticles != null)
                {
                    _backgroundParticles.Play();
                }
            }
        }

        /// <summary>
        /// 隐藏所有特效
        /// </summary>
        private void HideAllEffects()
        {
            if (_fiveStarEffect != null) _fiveStarEffect.SetActive(false);
            if (_fourStarEffect != null) _fourStarEffect.SetActive(false);
            if (_rateUpEffect != null) _rateUpEffect.SetActive(false);

            if (_backgroundParticles != null)
            {
                _backgroundParticles.Stop();
            }
        }

        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        private Color GetRandomRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 5: return new Color(1f, 0.8f, 0.2f);
                case 4: return new Color(0.6f, 0.4f, 1f);
                case 3: return new Color(0.3f, 0.6f, 1f);
                default: return Color.gray;
            }
        }

        /// <summary>
        /// 更新动画
        /// </summary>
        private void UpdateAnimation()
        {
            // 根据当前阶段更新动画
            switch (_currentPhase)
            {
                case GachaAnimationPhase.Rolling:
                    // 更新滚动卡牌
                    break;
                case GachaAnimationPhase.Reveal:
                    // 更新揭晓动画
                    break;
            }
        }

        #endregion

        #region Event Handlers

        private void OnContinueClick()
        {
            EndAnimation();
        }

        private void OnSkipClick()
        {
            SkipAnimation();
        }

        #endregion
    }

    /// <summary>
    /// 抽卡结果卡牌组件
    /// </summary>
    public class GachaResultCard : MonoBehaviour
    {
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _rarityBadge;
        [SerializeField] private GameObject _newBadge;

        private GachaResultData _data;

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(GachaResultData data)
        {
            _data = data;

            if (_nameText != null)
            {
                _nameText.text = data.name;
            }

            if (_newBadge != null)
            {
                _newBadge.SetActive(data.isNew);
            }

            if (_cardImage != null && data.icon != null)
            {
                _cardImage.sprite = data.icon;
            }

            if (_frameImage != null)
            {
                _frameImage.color = GetRarityColor(data.rarity);
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
    }
}
