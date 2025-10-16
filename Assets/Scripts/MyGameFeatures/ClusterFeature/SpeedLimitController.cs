using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class SpeedLimitController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image pillBg;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        [Header("Colors")]
        [SerializeField] private Color normalBg = new Color(0.16f, 0.18f, 0.22f, 1f);
        [SerializeField] private Color normalText = Color.white;
        [SerializeField] private Color alertBg = new Color(0.85f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color alertText = Color.white;

        [Header("Limit (km/h)")]
        [SerializeField] private int limitKmh = 90;

        [Header("Preset Keys")]
        [SerializeField] private KeyCode set30  = KeyCode.Alpha1;
        [SerializeField] private KeyCode set50  = KeyCode.Alpha2;
        [SerializeField] private KeyCode set70  = KeyCode.Alpha3;
        [SerializeField] private KeyCode set90  = KeyCode.Alpha4;
        [SerializeField] private KeyCode set110 = KeyCode.Alpha5;
        [SerializeField] private KeyCode set130 = KeyCode.Alpha6;

        const float FLASH_HZ = 1.0f;
        const float FLASH_MIN_ALPHA = 0.65f;
        const float FLASH_MAX_ALPHA = 1f;

        private IBroadcaster _bus;
        private bool _subscribed;
        private float _speedKmh;
        private Color _cachedAlertBg, _cachedNormalBg;

        void Awake()
        {
            AutoBindIfNeeded();
            Locator.TryResolve(out _bus);
            _cachedAlertBg = alertBg;
            _cachedNormalBg = pillBg ? pillBg.color : normalBg;
            UpdateValueText();
            ApplyStaticNormal();
        }

        void OnEnable()
        {
            TrySubscribe();
        }

        void OnDisable()
        {
            if (_bus != null && _subscribed)
            {
                _bus.Remove<OnSpeedChanged>(OnSpeed);
                _subscribed = false;
            }
        }

        void Update()
        {
            if (!_subscribed) TrySubscribe();

            if (Input.GetKeyDown(set30))  SetLimit(30);
            if (Input.GetKeyDown(set50))  SetLimit(50);
            if (Input.GetKeyDown(set70))  SetLimit(70);
            if (Input.GetKeyDown(set90))  SetLimit(90);
            if (Input.GetKeyDown(set110)) SetLimit(110);
            if (Input.GetKeyDown(set130)) SetLimit(130);

            UpdateVisual();
        }

        void TrySubscribe()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && !_subscribed)
            {
                _bus.Add<OnSpeedChanged>(OnSpeed);
                _subscribed = true;
            }
        }

        void OnSpeed(OnSpeedChanged e)
        {
            _speedKmh = e.kmh;
        }

        void UpdateVisual()
        {
            bool over = _speedKmh > limitKmh;

            if (over)
            {
                float a = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * FLASH_HZ) + 1f) * 0.5f;
                a = Mathf.Lerp(FLASH_MIN_ALPHA, FLASH_MAX_ALPHA, a);

                if (pillBg)
                {
                    var c = _cachedAlertBg; c.a = a;
                    pillBg.color = c;
                }
                if (valueText) valueText.color = alertText;
                if (labelText) labelText.color = alertText;
            }
            else
            {
                ApplyStaticNormal();
            }
        }

        void ApplyStaticNormal()
        {
            if (pillBg)    pillBg.color = _cachedNormalBg;
            if (valueText) valueText.color = normalText;
            if (labelText) labelText.color = normalText;
        }

        public void SetLimit(int kmh)
        {
            limitKmh = Mathf.Clamp(kmh, 0, 200);
            UpdateValueText();
        }

        void UpdateValueText()
        {
            if (valueText) valueText.text = limitKmh.ToString();
        }

        void AutoBindIfNeeded()
        {
            var r = transform;
            if (!pillBg)
            {
                pillBg = r.Find("PillBG")?.GetComponent<Image>();
                if (!pillBg) pillBg = r.GetComponent<Image>();
            }
            if (!labelText) labelText = r.Find("Label")?.GetComponent<TMP_Text>();
            if (!valueText) valueText = r.Find("Value")?.GetComponent<TMP_Text>();
        }
    }
}
