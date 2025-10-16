using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class ThemeController : MonoBehaviour
    {
        [SerializeField] private Image screenDimmer;
        [SerializeField] private bool auto = false;
        [SerializeField, Range(0,1)] private float lightSensor = 1f;
        [SerializeField] private ThemeSO day;
        [SerializeField] private ThemeSO night;

        private readonly List<Graphic> _graphics = new();
        private IBroadcaster _bus;

        private void Awake()
        {
            _bus = Locator.Resolve<IBroadcaster>();
            GetComponentsInChildren(true, _graphics);
            _bus.Add<ThemeChanged>(ApplyTheme);
        }

        private void OnDestroy()
        {
            _bus.Remove<ThemeChanged>(ApplyTheme);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N)) _bus.Broadcast(new ThemeChanged(day));
            if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.N)) auto = !auto;

            if (auto)
            {
                lightSensor = Mathf.PingPong(Time.time * 0.02f, 1f);
                var t = lightSensor < 0.4f ? night : day;
                _bus.Broadcast(new ThemeChanged(t));
            }
        }

        private void CollectGraphics(bool includeInactive, List<Graphic> outList)
        {
            outList.Clear();
            outList.AddRange(GetComponentsInChildren<Graphic>(includeInactive));
        }

        private void ApplyTheme(ThemeChanged e)
        {
            foreach (var g in _graphics)
            {
                if (g is Image img) img.color = Color.Lerp(img.color, e.theme.bg, 0.4f);
                if (g is TMP_Text txt) txt.color = Color.Lerp(txt.color, e.theme.primary, 0.4f);
            }
            if (screenDimmer) screenDimmer.color = new Color(0,0,0, 1f - e.theme.brightness);
        }
    }
}
