using UnityEngine;

namespace ICXK3
{
    [CreateAssetMenu(menuName="Cluster/Theme", fileName="Theme")]
    public class ThemeSO : ScriptableObject
    {
        public Color bg = Color.black;
        public Color primary = Color.white;
        public Color secondary = Color.gray;
        public Color accent = new(0.2f,0.6f,1f);
        [Range(0,1)] public float brightness = 1f;
        [HideInInspector] public string themeName = "Unnamed";
    }

    public readonly struct ThemeChanged
    {
        public readonly ThemeSO theme;
        public ThemeChanged(ThemeSO t) => theme = t;
    }
}
