using System.Collections;
using UnityEngine;

namespace ICXK3
{
    // Tween leggero per alpha/pos/scale su UI
    public static class UIAnimator
    {
        public static IEnumerator Fade(CanvasGroup cg, float to, float dur)
        {
            cg.gameObject.SetActive(true);
            var from = cg.alpha; float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, dur);
                cg.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0,1,t));
                yield return null;
            }
            cg.alpha = to;
            if (to <= 0.001f) cg.gameObject.SetActive(false);
        }

        public static IEnumerator Move(RectTransform rt, Vector2 from, Vector2 to, float dur)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, dur);
                var k = Mathf.SmoothStep(0,1,t);
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
                yield return null;
            }
            rt.anchoredPosition = to;
        }

        public static IEnumerator Scale(RectTransform rt, Vector3 from, Vector3 to, float dur)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, dur);
                var k = Mathf.SmoothStep(0,1,t);
                rt.localScale = Vector3.LerpUnclamped(from, to, k);
                yield return null;
            }
            rt.localScale = to;
        }
    }
}
