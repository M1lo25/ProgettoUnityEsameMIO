using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class GaugeSpeed : MonoBehaviour
    {
        [Header("Speed Panel")]
        [SerializeField] private TMP_Text speedValue;
        [SerializeField] private TMP_Text speedUnit;
        [SerializeField] private Image speedBarFill;
        [SerializeField] private float vmaxKmh = 220f;

        [Header("RPM Panel")]
        [SerializeField] private TMP_Text rpmValue;
        [SerializeField] private TMP_Text rpmUnit;
        [SerializeField] private Image rpmBarFill;
        [SerializeField] private float rpmMax = 7000f;

        [Header("Visual")]
        [SerializeField, Range(0f, 1f)] private float numberSmoothing = 0.25f;
        [SerializeField] private bool showZeroPadding = false;

        private IBroadcaster _bus;
        private float _uiSpeed, _targetSpeed;
        private float _uiRpm, _targetRpm;
        private bool _subscribed;

        private void Awake()
        {
            Locator.TryResolve(out _bus);
            TryAutoBind();
        }

        private void OnEnable()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            if (_bus != null && !_subscribed)
            {
                _bus.Add<OnSpeedChanged>(OnSpeed);
                _bus.Add<OnRpmChanged>(OnRpm);
                _subscribed = true;
            }
        }

        private void OnDisable()
        {
            if (_bus != null && _subscribed)
            {
                _bus.Remove<OnSpeedChanged>(OnSpeed);
                _bus.Remove<OnRpmChanged>(OnRpm);
                _subscribed = false;
            }
        }

        private void Update()
        {
            if (_bus == null)
            {
                if (Locator.TryResolve(out _bus) && !_subscribed)
                {
                    _bus.Add<OnSpeedChanged>(OnSpeed);
                    _bus.Add<OnRpmChanged>(OnRpm);
                    _subscribed = true;
                }
            }

            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(numberSmoothing), Time.unscaledDeltaTime * 60f);
            _uiSpeed = Mathf.Lerp(_uiSpeed, _targetSpeed, k);
            _uiRpm = Mathf.Lerp(_uiRpm, _targetRpm, k);

            if (speedValue) speedValue.text = FormatSpeed(_uiSpeed);
            if (rpmValue) rpmValue.text = Mathf.RoundToInt(_uiRpm).ToString();

            if (speedUnit && string.IsNullOrEmpty(speedUnit.text)) speedUnit.text = "km/h";
            if (rpmUnit && string.IsNullOrEmpty(rpmUnit.text)) rpmUnit.text = "RPM";

            if (speedBarFill) speedBarFill.fillAmount = Mathf.Clamp01(_uiSpeed / Mathf.Max(1f, vmaxKmh));
            if (rpmBarFill) rpmBarFill.fillAmount = Mathf.Clamp01(_uiRpm / Mathf.Max(1f, rpmMax));
        }

        private void OnSpeed(OnSpeedChanged e)
        {
            _targetSpeed = Mathf.Max(0f, e.kmh);
        }

        private void OnRpm(OnRpmChanged e)
        {
            _targetRpm = Mathf.Max(0f, e.rpm);
        }

        private string FormatSpeed(float kmh)
        {
            int v = Mathf.RoundToInt(kmh);
            return showZeroPadding ? v.ToString("D3") : v.ToString();
        }

        private void TryAutoBind()
        {
            if (!speedValue) speedValue = transform.Find("Panels/SpeedPanel/Body/SpeedValue")?.GetComponent<TMP_Text>();
            if (!speedUnit) speedUnit = transform.Find("Panels/SpeedPanel/Body/Unit")?.GetComponent<TMP_Text>();
            if (!speedBarFill) speedBarFill = transform.Find("Panels/SpeedPanel/SpeedBar")?.GetComponent<Image>();
            if (!rpmValue) rpmValue = transform.Find("Panels/RpmPanel/Body/RpmValue")?.GetComponent<TMP_Text>();
            if (!rpmUnit) rpmUnit = transform.Find("Panels/RpmPanel/Body/Unit")?.GetComponent<TMP_Text>();
            if (!rpmBarFill) rpmBarFill = transform.Find("Panels/RpmPanel/RpmBar")?.GetComponent<Image>();
        }
    }
}
