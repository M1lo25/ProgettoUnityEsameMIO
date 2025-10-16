using UnityEngine;

namespace ICXK3
{
    public enum GaugeVariant { Dial, Bar }

    [CreateAssetMenu(menuName="Cluster/Mode", fileName="Mode")]
    public class TerrainModeSO : ScriptableObject
    {
        public string modeName = "Road";
        public Sprite icon;
        public Color accent = Color.cyan;

        public GaugeVariant speedVariant = GaugeVariant.Dial;
        public GaugeVariant rpmVariant = GaugeVariant.Dial;

        public int prioSpeed = 100;
        public int prioRpm = 90;
        public int prioInclino = 80;
        public int prioGMeter = 70;
        public int prioFCW = 60;
    }

    public readonly struct TerrainModeChanged
    {
        public readonly TerrainModeSO mode;
        public TerrainModeChanged(TerrainModeSO m) => mode = m;
    }
}
