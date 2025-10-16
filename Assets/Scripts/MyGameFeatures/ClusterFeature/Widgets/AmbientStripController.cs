using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public class AmbientStripController : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private float vmax = 200f;

        private float uiFill, targetFill, rpm;
        private bool pulseEnabled = true;

        private void OnEnable()
        {
            var bus = Locator.Resolve<IBroadcaster>();
            bus.Add<OnSpeedChanged>(OnSpeed);
            bus.Add<OnRpmChanged>(OnRpm);
        }

        private void OnDisable()
        {
            var bus = Locator.Resolve<IBroadcaster>();
            bus.Remove<OnSpeedChanged>(OnSpeed);
            bus.Remove<OnRpmChanged>(OnRpm);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) pulseEnabled = !pulseEnabled;

            uiFill = Mathf.Lerp(uiFill, targetFill, Time.deltaTime * 4f);
            float pulse = pulseEnabled ? 0.05f * Mathf.Sin(Time.time * (rpm/60f) * 2f * Mathf.PI) : 0f;
            if (bar) bar.fillAmount = Mathf.Clamp01(uiFill + pulse);
        }

        private void OnSpeed(OnSpeedChanged e)
        {
            targetFill = Mathf.InverseLerp(0, vmax, e.kmh);
        }

        private void OnRpm(OnRpmChanged e)
        {
            rpm = e.rpm;
        }
    }
}
