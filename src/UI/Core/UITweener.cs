// UITweener.cs - UI动画工具类
// 提供常用的UI动画效果

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace JRPG.UI.Core
{
    /// <summary>
    /// UI动画工具类
    /// 提供透明度、缩放、位置等常用动画
    /// </summary>
    public static class UITweener
    {
        #region Alpha Tween

        /// <summary>
        /// 透明度渐变
        /// </summary>
        public static void TweenAlpha(CanvasGroup canvasGroup, float from, float to, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            if (canvasGroup == null) return;

            curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            CoroutineManager.StartCoroutine(AlphaTweenCoroutine(canvasGroup, from, to, duration, curve, onComplete));
        }

        private static System.Collections.IEnumerator AlphaTweenCoroutine(CanvasGroup canvasGroup, float from, float to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 透明度渐变（Image组件）
        /// </summary>
        public static void TweenImageAlpha(Image image, float from, float to, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            if (image == null) return;

            curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            Color fromColor = image.color;
            fromColor.a = from;
            Color toColor = image.color;
            toColor.a = to;
            image.color = fromColor;

            CoroutineManager.StartCoroutine(ImageAlphaTweenCoroutine(image, fromColor, toColor, duration, curve, onComplete));
        }

        private static System.Collections.IEnumerator ImageAlphaTweenCoroutine(Image image, Color from, Color to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                image.color = Color.Lerp(from, to, t);
                yield return null;
            }

            image.color = to;
            onComplete?.Invoke();
        }

        #endregion

        #region Scale Tween

        /// <summary>
        /// 缩放动画
        /// </summary>
        public static void TweenScale(RectTransform rectTransform, Vector3 from, Vector3 to, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            if (rectTransform == null) return;

            curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            rectTransform.localScale = from;
            CoroutineManager.StartCoroutine(ScaleTweenCoroutine(rectTransform, from, to, duration, curve, onComplete));
        }

        private static System.Collections.IEnumerator ScaleTweenCoroutine(RectTransform rectTransform, Vector3 from, Vector3 to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                rectTransform.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }

            rectTransform.localScale = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 弹出动画（从中心放大）
        /// </summary>
        public static void PopIn(RectTransform rectTransform, float duration = 0.3f, Action onComplete = null)
        {
            TweenScale(rectTransform, Vector3.zero, Vector3.one, duration, AnimationCurve.EaseInOut(0, 0, 1, 1), onComplete);
        }

        /// <summary>
        /// 收缩动画（缩小到中心）
        /// </summary>
        public static void PopOut(RectTransform rectTransform, float duration = 0.2f, Action onComplete = null)
        {
            TweenScale(rectTransform, Vector3.one, Vector3.zero, duration, AnimationCurve.EaseInOut(0, 1, 1, 0), onComplete);
        }

        /// <summary>
        /// 弹跳动画
        /// </summary>
        public static void BounceIn(RectTransform rectTransform, float duration = 0.4f, Action onComplete = null)
        {
            AnimationCurve bounceCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1.2f),
                new Keyframe(0.7f, 0.9f),
                new Keyframe(0.9f, 1.05f),
                new Keyframe(1f, 1f)
            );

            TweenScale(rectTransform, Vector3.zero, Vector3.one, duration, bounceCurve, onComplete);
        }

        /// <summary>
        /// 脉冲动画
        /// </summary>
        public static void Pulse(RectTransform rectTransform, float scale = 1.1f, float duration = 0.15f)
        {
            Vector3 normalScale = Vector3.one;
            Vector3 pulseScale = normalScale * scale;

            TweenScale(rectTransform, normalScale, pulseScale, duration, null, () =>
            {
                TweenScale(rectTransform, pulseScale, normalScale, duration, null, null);
            });
        }

        #endregion

        #region Position Tween

        /// <summary>
        /// 位置移动动画
        /// </summary>
        public static void TweenPosition(RectTransform rectTransform, Vector3 from, Vector3 to, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            if (rectTransform == null) return;

            curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            rectTransform.anchoredPosition = from;
            CoroutineManager.StartCoroutine(PositionTweenCoroutine(rectTransform, from, to, duration, curve, onComplete));
        }

        private static System.Collections.IEnumerator PositionTweenCoroutine(RectTransform rectTransform, Vector3 from, Vector3 to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                rectTransform.anchoredPosition = Vector3.Lerp(from, to, t);
                yield return null;
            }

            rectTransform.anchoredPosition = to;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 从左滑入
        /// </summary>
        public static void SlideInFromLeft(RectTransform rectTransform, float duration = 0.3f, Action onComplete = null)
        {
            float width = Screen.width;
            Vector3 from = new Vector3(-width, rectTransform.anchoredPosition.y, 0);
            Vector3 to = rectTransform.anchoredPosition;
            TweenPosition(rectTransform, from, to, duration, null, onComplete);
        }

        /// <summary>
        /// 从右滑入
        /// </summary>
        public static void SlideInFromRight(RectTransform rectTransform, float duration = 0.3f, Action onComplete = null)
        {
            float width = Screen.width;
            Vector3 from = new Vector3(width, rectTransform.anchoredPosition.y, 0);
            Vector3 to = rectTransform.anchoredPosition;
            TweenPosition(rectTransform, from, to, duration, null, onComplete);
        }

        /// <summary>
        /// 从上滑入
        /// </summary>
        public static void SlideInFromTop(RectTransform rectTransform, float duration = 0.3f, Action onComplete = null)
        {
            float height = Screen.height;
            Vector3 from = new Vector3(rectTransform.anchoredPosition.x, height, 0);
            Vector3 to = rectTransform.anchoredPosition;
            TweenPosition(rectTransform, from, to, duration, null, onComplete);
        }

        /// <summary>
        /// 从下滑入
        /// </summary>
        public static void SlideInFromBottom(RectTransform rectTransform, float duration = 0.3f, Action onComplete = null)
        {
            float height = Screen.height;
            Vector3 from = new Vector3(rectTransform.anchoredPosition.x, -height, 0);
            Vector3 to = rectTransform.anchoredPosition;
            TweenPosition(rectTransform, from, to, duration, null, onComplete);
        }

        #endregion

        #region Rotation Tween

        /// <summary>
        /// 旋转动画
        /// </summary>
        public static void TweenRotation(RectTransform rectTransform, float from, float to, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            if (rectTransform == null) return;

            curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            float currentRotation = rectTransform.localEulerAngles.z;
            rectTransform.localEulerAngles = new Vector3(0, 0, from);
            CoroutineManager.StartCoroutine(RotationTweenCoroutine(rectTransform, from, to, duration, curve, onComplete));
        }

        private static System.Collections.IEnumerator RotationTweenCoroutine(RectTransform rectTransform, float from, float to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                float angle = Mathf.Lerp(from, to, t);
                rectTransform.localEulerAngles = new Vector3(0, 0, angle);
                yield return null;
            }

            rectTransform.localEulerAngles = new Vector3(0, 0, to);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 旋转一圈
        /// </summary>
        public static void SpinOnce(RectTransform rectTransform, float duration = 1f, Action onComplete = null)
        {
            TweenRotation(rectTransform, 0f, 360f, duration, null, onComplete);
        }

        /// <summary>
        /// 抖动动画
        /// </summary>
        public static void Shake(RectTransform rectTransform, float intensity = 10f, float duration = 0.3f)
        {
            CoroutineManager.StartCoroutine(ShakeCoroutine(rectTransform, intensity, duration));
        }

        private static System.Collections.IEnumerator ShakeCoroutine(RectTransform rectTransform, float intensity, float duration)
        {
            Vector3 originalPos = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = originalPos.x + UnityEngine.Random.Range(-intensity, intensity);
                float y = originalPos.y + UnityEngine.Random.Range(-intensity, intensity);
                rectTransform.anchoredPosition = new Vector3(x, y, 0);
                yield return null;
            }

            rectTransform.anchoredPosition = originalPos;
        }

        #endregion

        #region Value Tween

        /// <summary>
        /// 数值渐变（用于进度条等）
        /// </summary>
        public static void TweenValue(float from, float to, float duration, Action<float> onUpdate, AnimationCurve curve = null, Action onComplete = null)
        {
            curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
            CoroutineManager.StartCoroutine(ValueTweenCoroutine(from, to, duration, curve, onUpdate, onComplete));
        }

        private static System.Collections.IEnumerator ValueTweenCoroutine(float from, float to, float duration, AnimationCurve curve, Action<float> onUpdate, Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                float value = Mathf.Lerp(from, to, t);
                onUpdate?.Invoke(value);
                yield return null;
            }

            onUpdate?.Invoke(to);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 数字滚动动画
        /// </summary>
        public static void CountUp(Text.TextMeshProUGUI text, int from, int to, float duration, string suffix = "")
        {
            TweenValue(from, to, duration, (value) =>
            {
                text.text = Mathf.RoundToInt(value).ToString() + suffix;
            });
        }

        #endregion
    }

    /// <summary>
    /// 协程管理器 - 用于启动协程
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        private static CoroutineManager _instance;

        public static CoroutineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineManager");
                    _instance = go.AddComponent<CoroutineManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public new static Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            return Instance.StartCoroutine(routine);
        }

        public new static void StopCoroutine(Coroutine routine)
        {
            if (_instance != null)
            {
                _instance.StopCoroutine(routine);
            }
        }
    }
}
