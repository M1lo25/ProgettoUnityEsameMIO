using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class FcwController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image barFill;
        [SerializeField] private TMP_Text ttcText;
        [SerializeField] private Image brakeIcon;

        [Header("Optional")]
        [SerializeField] private RectTransform barArea;
        [SerializeField] private CanvasGroup brakeGroup;

        [Header("Mapping")]
        [SerializeField] private float ttcMin = 0.5f;
        [SerializeField] private float ttcMax = 4f;
        [SerializeField] private float criticalTtc = 1f;

        [Header("Keys")]
        [SerializeField] private KeyCode egoInc = KeyCode.UpArrow;
        [SerializeField] private KeyCode egoDec = KeyCode.DownArrow;
        [SerializeField] private KeyCode leadInc = KeyCode.O;
        [SerializeField] private KeyCode leadDec = KeyCode.U;
        [SerializeField] private KeyCode closer  = KeyCode.L;
        [SerializeField] private KeyCode farther = KeyCode.K;

        private float egoSpeedKmh = 30f;
        private float leadSpeedKmh = 10f;
        private float distanceM = 25f;

        private float _ttc;
        private Coroutine _flashCo;
        private RectTransform _barFillRt;

        void Awake()
        {
            AutoBindIfNeeded();
            EnsureBarSetup();
            if (!brakeGroup && brakeIcon) brakeGroup = brakeIcon.GetComponent<CanvasGroup>();
            if (!brakeGroup && brakeIcon) brakeGroup = brakeIcon.gameObject.AddComponent<CanvasGroup>();
            SetBrakeAlpha(1f);
        }

        void OnDisable()
        {
            if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
            SetBrakeAlpha(1f);
        }

        void OnDestroy()
        {
            if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
        }

        void Update()
        {
            HandleInput();
            ComputeTTC();
            UpdateUI();
            HandleBrakeFlash();
        }

        void HandleInput()
        {
            if (Input.GetKey(egoInc)) egoSpeedKmh += 40f * Time.deltaTime;
            if (Input.GetKey(egoDec)) egoSpeedKmh -= 40f * Time.deltaTime;

            if (Input.GetKey(leadInc)) leadSpeedKmh += 40f * Time.deltaTime;
            if (Input.GetKey(leadDec)) leadSpeedKmh -= 40f * Time.deltaTime;

            if (Input.GetKey(closer))  distanceM -= 10f * Time.deltaTime;
            if (Input.GetKey(farther)) distanceM += 10f * Time.deltaTime;

            egoSpeedKmh  = Mathf.Max(0f, egoSpeedKmh);
            leadSpeedKmh = Mathf.Max(0f, leadSpeedKmh);
            distanceM    = Mathf.Clamp(distanceM, 0.5f, 200f);
        }

        void ComputeTTC()
        {
            float egoMs  = egoSpeedKmh  / 3.6f;
            float leadMs = leadSpeedKmh / 3.6f;
            float rel = Mathf.Max(0f, egoMs - leadMs);
            _ttc = rel > 0f ? distanceM / rel : 999f;
        }

        void UpdateUI()
        {
            float t = Mathf.Clamp(_ttc, ttcMin, ttcMax);
            float danger = Mathf.InverseLerp(ttcMax, ttcMin, t);

            if (barFill)
            {
                if (barFill.type != Image.Type.Filled || barFill.fillMethod != Image.FillMethod.Horizontal)
                {
                    barFill.type = Image.Type.Filled;
                    barFill.fillMethod = Image.FillMethod.Horizontal;
                    barFill.fillOrigin = 0;
                }

                barFill.fillAmount = danger;
                barFill.color = (t > 2f) ? Color.green : (t > 1f ? Color.yellow : Color.red);
            }
            else if (_barFillRt && barArea)
            {
                float w = barArea.rect.width;
                _barFillRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp01(danger) * w);
            }

            if (ttcText)
            {
                float disp = Mathf.Min(_ttc, 99.9f);
                ttcText.text = disp.ToString("0.0").Replace('.', ',') + "s";
            }
        }

        void HandleBrakeFlash()
        {
            bool critical = _ttc <= criticalTtc;
            if (critical)
            {
                if (_flashCo == null) _flashCo = StartCoroutine(FlashBrake());
            }
            else
            {
                if (_flashCo != null) { StopCoroutine(_flashCo); _flashCo = null; }
                SetBrakeAlpha(1f);
            }
        }

        IEnumerator FlashBrake()
        {
            const float freq = 6f;
            const float aMin = 0.1f;
            const float aMax = 1f;

            while (this && isActiveAndEnabled)
            {
                float a = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * freq) + 1f) * 0.5f;
                a = Mathf.Lerp(aMin, aMax, a);
                SetBrakeAlpha(a);
                yield return null;
            }
        }

        void SetBrakeAlpha(float a)
        {
            if (brakeGroup) { brakeGroup.alpha = a; return; }
            if (!brakeIcon) return;
            var c = brakeIcon.color; c.a = a; brakeIcon.color = c;
        }

        void EnsureBarSetup()
        {
            if (barFill) _barFillRt = barFill.rectTransform;
        }

        void AutoBindIfNeeded()
        {
            var r = transform;

            if (!barArea) barArea = r.Find("TTCFrame/BarArea")?.GetComponent<RectTransform>();
            if (!barFill) barFill = r.Find("TTCFrame/BarArea/BarFill")?.GetComponent<Image>();

            if (!ttcText)
            {
                var t1 = r.Find("TTCFrame/Header/TTCLabel (1)")?.GetComponent<TMP_Text>();
                if (!t1) t1 = r.Find("TTCFrame/Header/TTCLabelValue")?.GetComponent<TMP_Text>();
                ttcText = t1;
            }

            if (!brakeIcon)
            {
                var bi = r.Find("BrakeButton/BrakeIcon")?.GetComponent<Image>();
                if (!bi) bi = r.Find("BrakeButton/Brakelcon")?.GetComponent<Image>();
                brakeIcon = bi;
            }
        }
    }
}
