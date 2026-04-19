// DamageNumber.cs - 伤害数字飘字组件
// 显示战斗中的伤害数字、治疗数字、状态图标等

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// 伤害数字组件
    /// 在战斗界面显示伤害数字、治疗数字等浮动文字
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        #region UI References
        
        [Header("文字组件")]
        [SerializeField] private TextMeshProUGUI _damageText;
        
        [Header("设置")]
        [SerializeField] private float _floatSpeed = 50f;         // 上升速度
        [SerializeField] private float _fadeDuration = 1.5f;      // 淡出时长
        [SerializeField] private float _shakeIntensity = 5f;      // 抖动强度
        [SerializeField] private float _shakeDuration = 0.2f;     // 抖动时长
        
        #endregion
        
        #region Colors
        
        private static readonly Color NormalDamageColor = new Color(1f, 0.9f, 0.8f);
        private static readonly Color CriticalDamageColor = new Color(1f, 0.4f, 0.4f);
        private static readonly Color HealColor = new Color(0.4f, 1f, 0.4f);
        private static readonly Color MissColor = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color BlockColor = new Color(0.5f, 0.5f, 1f);
        private static readonly Color AbsorbColor = new Color(0.5f, 1f, 0.5f);
        private static readonly Color PoisonColor = new Color(0.5f, 1f, 0.3f);
        private static readonly Color BurnColor = new Color(1f, 0.5f, 0.2f);
        
        #endregion
        
        #region Private Fields
        
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _startPosition;
        private float _elapsedTime;
        private bool _isAnimating = true;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        private void Update()
        {
            if (!_isAnimating) return;
            
            _elapsedTime += Time.deltaTime;
            
            // 上升动画
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition += Vector2.up * _floatSpeed * Time.deltaTime;
            }
            
            // 淡出
            if (_elapsedTime > _fadeDuration * 0.5f)
            {
                float fadeProgress = (_elapsedTime - _fadeDuration * 0.5f) / (_fadeDuration * 0.5f);
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }
            
            // 结束
            if (_elapsedTime >= _fadeDuration)
            {
                _isAnimating = false;
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 初始化伤害数字
        /// </summary>
        public void Initialize(Vector3 worldPosition, DamageResult result)
        {
            // 设置位置
            if (_rectTransform != null)
            {
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                    Camera.main, worldPosition);
                _rectTransform.anchoredPosition = screenPos;
            }
            
            // 设置文字
            UpdateDisplay(result);
            
            // 播放动画
            if (result.IsCritical)
            {
                StartCoroutine(CriticalAnimation());
            }
            else if (result.ResultType == DamageResultType.Miss)
            {
                StartCoroutine(MissAnimation());
            }
            
            // 初始随机偏移
            RandomizePosition();
        }
        
        /// <summary>
        /// 初始化简单伤害数字
        /// </summary>
        public void Initialize(Vector3 worldPosition, int damage, bool isCritical = false, bool isHeal = false)
        {
            var result = new DamageResult
            {
                FinalDamage = damage,
                IsCritical = isCritical,
                ResultType = isHeal ? DamageResultType.Normal : DamageResultType.Normal
            };
            
            Initialize(worldPosition, result);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// 更新显示内容
        /// </summary>
        private void UpdateDisplay(DamageResult result)
        {
            if (_damageText == null) return;
            
            var sb = new StringBuilder();
            
            // 设置颜色和前缀
            Color color;
            string prefix = "";
            
            switch (result.ResultType)
            {
                case DamageResultType.Normal:
                    color = result.HealAmount > 0 ? HealColor : NormalDamageColor;
                    prefix = result.HealAmount > 0 ? "+" : "";
                    break;
                    
                case DamageResultType.Critical:
                    color = CriticalDamageColor;
                    prefix = result.HealAmount > 0 ? "+" : "";
                    sb.AppendLine("暴击!");
                    break;
                    
                case DamageResultType.Miss:
                    color = MissColor;
                    sb.Append("Miss");
                    _damageText.text = sb.ToString();
                    _damageText.color = color;
                    return;
                    
                case DamageResultType.Immune:
                    color = BlockColor;
                    sb.Append("无效");
                    _damageText.text = sb.ToString();
                    _damageText.color = color;
                    return;
                    
                case DamageResultType.Resist:
                    color = BlockColor;
                    prefix = "";
                    sb.AppendLine("抗性");
                    break;
                    
                case DamageResultType.Weakness:
                    color = CriticalDamageColor;
                    sb.AppendLine("弱点!");
                    break;
                    
                case DamageResultType.Absorb:
                    color = AbsorbColor;
                    prefix = "+";
                    break;
                    
                default:
                    color = NormalDamageColor;
                    break;
            }
            
            // 显示数值
            int displayValue = result.HealAmount > 0 ? result.HealAmount : result.FinalDamage;
            sb.Append($"{prefix}{displayValue}");
            
            // 附加状态提示
            if (result.AppliedStatuses != null && result.AppliedStatuses.Count > 0)
            {
                foreach (var status in result.AppliedStatuses)
                {
                    sb.Append($"\n<size=50%>{GetStatusName(status)}</size>");
                }
            }
            
            _damageText.text = sb.ToString();
            _damageText.color = color;
            
            // 设置字体大小
            if (result.IsCritical)
            {
                _damageText.fontSize = _damageText.fontSize * 1.5f;
            }
            
            // 设置文字样式
            _damageText.fontStyle = FontStyles.Bold;
        }
        
        /// <summary>
        /// 获取状态名称
        /// </summary>
        private string GetStatusName(StatusEffectType status)
        {
            return status switch
            {
                StatusEffectType.Burn => "灼烧",
                StatusEffectType.Poison => "中毒",
                StatusEffectType.Bleed => "出血",
                StatusEffectType.Paralyze => "麻痹",
                StatusEffectType.Freeze => "冻结",
                StatusEffectType.Stun => "眩晕",
                StatusEffectType.Sleep => "睡眠",
                _ => status.ToString()
            };
        }
        
        /// <summary>
        /// 随机化初始位置
        /// </summary>
        private void RandomizePosition()
        {
            if (_rectTransform == null) return;
            
            float offsetX = Random.Range(-20f, 20f);
            float offsetY = Random.Range(-10f, 10f);
            
            _rectTransform.anchoredPosition += new Vector2(offsetX, offsetY);
        }
        
        /// <summary>
        /// 暴击动画
        /// </summary>
        private IEnumerator CriticalAnimation()
        {
            float elapsed = 0f;
            Vector3 originalScale = Vector3.one * 1.5f;
            Vector3 targetScale = Vector3.one;
            
            // 放大后缩小
            while (elapsed < _shakeDuration * 2)
            {
                float t = elapsed / (_shakeDuration * 2);
                float scale = Mathf.Lerp(1.5f, 1f, t);
                transform.localScale = Vector3.one * scale;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Miss动画
        /// </summary>
        private IEnumerator MissAnimation()
        {
            // Miss只需要简单显示
            yield break;
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// 创建伤害数字（静态方法）
        /// </summary>
        public static DamageNumber Create(Vector3 position, int damage, DamageResultType resultType, bool isCritical = false)
        {
            var prefab = Resources.Load<DamageNumber>("Prefabs/UI/DamageNumber");
            if (prefab == null)
            {
                // 如果没有预制体，动态创建
                var go = new GameObject("DamageNumber");
                var component = go.AddComponent<DamageNumber>();
                
                var canvas = GameObject.Find("BattleCanvas");
                if (canvas != null)
                {
                    component.transform.SetParent(canvas.transform);
                }
                
                var text = go.AddComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 36;
                
                component._damageText = text;
                component.Initialize(position, damage, isCritical);
                
                return component;
            }
            
            var instance = Instantiate(prefab);
            instance.Initialize(position, damage, isCritical);
            return instance;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 元素图标显示组件
    /// </summary>
    public class ElementIcon : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite[] _elementSprites;
        
        public void SetElement(Element element)
        {
            if (_iconImage == null || _elementSprites == null) return;
            
            int index = (int)element;
            if (index >= 0 && index < _elementSprites.Length)
            {
                _iconImage.sprite = _elementSprites[index];
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.enabled = false;
            }
        }
    }
}
