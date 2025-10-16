using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public struct OnRollChanged { public float deg; public OnRollChanged(float d){ deg=d; } }

    public class InclinometerController : MonoBehaviour
    {
        [SerializeField] private RectTransform suvIcon;
        [SerializeField] private Image colorBand;
        [SerializeField] private Color green = new(0.5f,1f,0.5f);
        [SerializeField] private Color yellow = new(1f,0.9f,0.3f);
        [SerializeField] private Color red = new(1f,0.4f,0.4f);

        private void OnEnable() => Locator.Resolve<IBroadcaster>().Add<OnRollChanged>(OnRoll);
        private void OnDisable() => Locator.Resolve<IBroadcaster>().Remove<OnRollChanged>(OnRoll);

        private void OnRoll(OnRollChanged e)
        {
            float cl = Mathf.Clamp(e.deg, -30f, 30f);
            if (suvIcon) suvIcon.localEulerAngles = new(0,0, -cl);
            var a = Mathf.Abs(cl);
            if (colorBand) colorBand.color = a < 10 ? green : a < 20 ? yellow : red;
        }
    }
}
