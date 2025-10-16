using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public class ModeButtonsController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button btnRoad;
        [SerializeField] private Button btnTrail;
        [SerializeField] private Button btnSnow;

        [Header("Button BG images")]
        [SerializeField] private Image bgRoad;
        [SerializeField] private Image bgTrail;
        [SerializeField] private Image bgSnow;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.1f, 0.1f, 0.13f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.2f, 0.3f, 0.6f, 1f);

        [Header("Header LED")]
        [SerializeField] private Image led;
        [SerializeField] private Image ledGlow;
        [SerializeField] private float blinkDuration = 0.6f;
        [SerializeField] private int blinkCount = 2;

        [Header("Mode Assets")]
        [SerializeField] private TerrainModeSO road;
        [SerializeField] private TerrainModeSO trail;
        [SerializeField] private TerrainModeSO snow;

        [Header("Hotkeys")]
        [SerializeField] private KeyCode keyRoad = KeyCode.F1;
        [SerializeField] private KeyCode keyTrail = KeyCode.F2;
        [SerializeField] private KeyCode keySnow = KeyCode.F3;

        private IBroadcaster _bus;
        private Coroutine _blinkCo;

        private void Awake()
        {
            Locator.TryResolve(out _bus);
            AutoBindIfNeeded();

            if (btnRoad)  btnRoad.onClick.AddListener(() => SelectMode(road));
            if (btnTrail) btnTrail.onClick.AddListener(() => SelectMode(trail));
            if (btnSnow)  btnSnow.onClick.AddListener(() => SelectMode(snow));
        }

        private void OnEnable()
        {
            if (trail) SetVisual(trail);
            else if (road) SetVisual(road);
            else if (snow) SetVisual(snow);
        }

        private void OnDisable()
        {
            if (_blinkCo != null) { StopCoroutine(_blinkCo); _blinkCo = null; }
        }

        private void Update()
        {
            if (Input.GetKeyDown(keyRoad))  SelectMode(road);
            if (Input.GetKeyDown(keyTrail)) SelectMode(trail);
            if (Input.GetKeyDown(keySnow))  SelectMode(snow);
        }

        private void SelectMode(TerrainModeSO mode)
        {
            if (!isActiveAndEnabled || mode == null) return;

            SetVisual(mode);

            if (_bus == null) Locator.TryResolve(out _bus);
            _bus?.Broadcast(new TerrainModeChanged(mode));

            if (_blinkCo != null) StopCoroutine(_blinkCo);
            _blinkCo = StartCoroutine(BlinkLed());
        }

        private void SetVisual(TerrainModeSO mode)
        {
            if (bgRoad)  bgRoad.color  = (mode == road)  ? selectedColor : normalColor;
            if (bgTrail) bgTrail.color = (mode == trail) ? selectedColor : normalColor;
            if (bgSnow)  bgSnow.color  = (mode == snow)  ? selectedColor : normalColor;
        }

        private IEnumerator BlinkLed()
        {
            if (!led && !ledGlow) yield break;

            float per = blinkDuration / (blinkCount * 2f);
            for (int i = 0; i < blinkCount; i++)
            {
                SetLedAlpha(1f);
                yield return new WaitForSecondsRealtime(per);
                SetLedAlpha(0.2f);
                yield return new WaitForSecondsRealtime(per);
            }
            SetLedAlpha(1f);
        }

        private void SetLedAlpha(float a)
        {
            if (led)
            {
                var c = led.color; c.a = a; led.color = c;
            }
            if (ledGlow)
            {
                var c = ledGlow.color; c.a = a * 0.6f; ledGlow.color = c;
            }
        }

        private void AutoBindIfNeeded()
        {
            var root = transform;

            if (!btnRoad)  btnRoad  = root.Find("Content/BtnModePrefabRoad")   ?.GetComponent<Button>();
            if (!btnTrail) btnTrail = root.Find("Content/BtnModePrefabTrail")  ?.GetComponent<Button>();
            if (!btnSnow)  btnSnow  = root.Find("Content/BtnModePrefabSnow")  ?.GetComponent<Button>();

            if (!bgRoad)   bgRoad   = root.Find("Content/BtnModePrefabRoad/BGRoad")   ?.GetComponent<Image>();
            if (!bgTrail)  bgTrail  = root.Find("Content/BtnModePrefabTrail/BGTrail") ?.GetComponent<Image>();
            if (!bgSnow)   bgSnow   = root.Find("Content/BtnModePrefabSnow/BGSnow")   ?.GetComponent<Image>();

            if (!led)      led      = root.Find("Header/LED")     ?.GetComponent<Image>();
            if (!ledGlow)  ledGlow  = root.Find("Header/LEDGlow") ?.GetComponent<Image>();
        }
    }
}
