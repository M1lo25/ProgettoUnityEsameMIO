using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public class TerrainModeController : MonoBehaviour
    {
        [SerializeField] private Image badgeIcon;
        [SerializeField] private Image accentBar;
        [SerializeField] private TerrainModeSO road, trail, snow;

        [Header("Refs (varianti)")]
        [SerializeField] private VariantGauge speedGauge;
        [SerializeField] private VariantGauge rpmGauge;

        [Header("Panels con priorità")]
        [SerializeField] private RectTransform speedPanel, rpmPanel, inclinoPanel, gMeterPanel, fcwPanel;

        private IBroadcaster _bus;

        private void Awake()
        {
            _bus = Locator.Resolve<IBroadcaster>();
            _bus.Add<TerrainModeChanged>(OnMode);
        }

        private void OnDestroy()
        {
            _bus.Remove<TerrainModeChanged>(OnMode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) _bus.Broadcast(new TerrainModeChanged(road));
            if (Input.GetKeyDown(KeyCode.F2)) _bus.Broadcast(new TerrainModeChanged(trail));
            if (Input.GetKeyDown(KeyCode.F3)) _bus.Broadcast(new TerrainModeChanged(snow));
        }

        private void OnMode(TerrainModeChanged e)
        {
            if (badgeIcon) badgeIcon.sprite = e.mode.icon;
            if (accentBar) accentBar.color = e.mode.accent;

            speedGauge?.SwapVariant(e.mode.speedVariant);
            rpmGauge?.SwapVariant(e.mode.rpmVariant);

            SetPriority(speedPanel, e.mode.prioSpeed);
            SetPriority(rpmPanel, e.mode.prioRpm);
            SetPriority(inclinoPanel, e.mode.prioInclino);
            SetPriority(gMeterPanel, e.mode.prioGMeter);
            SetPriority(fcwPanel, e.mode.prioFCW);

            StopAllCoroutines();
            StartCoroutine(Reflow());
        }

        private static void SetPriority(RectTransform panel, int prio)
        {
            if (!panel) return;
            panel.SetSiblingIndex(Mathf.Clamp(prio, 0, panel.parent.childCount-1));
        }

        private IEnumerator Reflow()
        {
            // Esempio: piccola anim di scale+alpha sul container principale
            var cg = GetComponent<CanvasGroup>();
            if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
            yield return UIAnimator.Fade(cg, 0.0f, 0.15f);
            yield return UIAnimator.Fade(cg, 1.0f, 0.35f);
        }
    }
}
