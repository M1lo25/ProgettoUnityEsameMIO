using UnityEngine;
using TMPro;

namespace ICXK3
{
    public class ModeBadgeUpdater : MonoBehaviour
    {
        [SerializeField] private TMP_Text speedModeText;
        [SerializeField] private TMP_Text rpmModeText;

        private IBroadcaster _bus;

        private void Awake()
        {
            Locator.TryResolve(out _bus);
            AutoBind();
        }

        private void OnEnable()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            _bus?.Add<TerrainModeChanged>(OnMode);
        }

        private void OnDisable()
        {
            _bus?.Remove<TerrainModeChanged>(OnMode);
        }

        private void OnMode(TerrainModeChanged e)
        {
            if (!isActiveAndEnabled) return;
            string t = (e.mode ? e.mode.modeName : "MODE")?.ToUpperInvariant();
            if (speedModeText) speedModeText.text = t;
            if (rpmModeText)   rpmModeText.text   = t;
        }

        private void AutoBind()
        {
            var root = transform.root;

            if (!speedModeText)
            {
                speedModeText =
                    root.Find("Canvas/Panels/SpeedPanel/Header/ModeBadge/ModeBadgeTextSpe")?.GetComponent<TMP_Text>()
                    ?? root.Find("Canvas/Panels/SpeedPanel/Header/ModeBadge/ModeBadgeText")?.GetComponent<TMP_Text>();
            }

            if (!rpmModeText)
            {
                rpmModeText =
                    root.Find("Canvas/Panels/RpmPanel/Header/ModeBadge/ModeBadgeTextRpm")?.GetComponent<TMP_Text>()
                    ?? root.Find("Canvas/Panels/RpmPanel/Header/ModeBadge/ModeBadgeText")?.GetComponent<TMP_Text>();
            }
        }
    }
}
