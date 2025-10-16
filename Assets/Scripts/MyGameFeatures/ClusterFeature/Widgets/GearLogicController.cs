using UnityEngine;

namespace ICXK3
{
    /// <summary>
    /// Logica PRND + rapporti 1..7.
    /// - Legge la velocità via evento OnSpeedChanged (non dipende rigidamente dal service).
    /// - In D scala tra 1..7 secondo le soglie ICX-K3 (con isteresi per evitare pompaggi).
    /// - Tasti P/R/N/D opzionali.
    /// </summary>
    public class GearLogicController : MonoBehaviour
    {
        // Eventi broadcast (se ti servono a valle)
        public struct OnGearSelectorChanged { public char selector; public OnGearSelectorChanged(char s){ selector=s; } }
        public struct OnDriveGearChanged   { public int  gear;     public OnDriveGearChanged(int g){ gear=g; } }

        [Header("UI")]
        [SerializeField] private GearController ui;

        [Header("Use Preset (override arrays below)")]
        [Tooltip("Se attivo, forza le soglie ICX-K3 qui sotto (consigliato).")]
        [SerializeField] private bool usePresetICXK3 = true;

        // Preset ICX-K3: 1:0–30 | 2:25–55 | 3:50–85 | 4:80–115 | 5:110–145 | 6:140–170 | 7:>170–185
        private static readonly float[] PRESET_UP =  { 30f, 55f, 85f, 115f, 145f, 170f, 185f }; // confini superiori (g -> g+1)
        private static readonly float[] PRESET_DN =  { 25f, 50f, 80f, 110f, 140f, 168f, 999f }; // confini inferiori (g -> g-1) con isteresi

        [Header("Upshift (km/h) per cambio 1→2 … 6→7")]
        [SerializeField] private float[] upshift   = new float[7] { 30f, 55f, 85f, 115f, 145f, 170f, 185f };

        [Header("Downshift (km/h) con isteresi")]
        [SerializeField] private float[] downshift = new float[7] { 25f, 50f, 80f, 110f, 140f, 168f, 999f };

        [Header("Input (tastiera)")]
        [SerializeField] private bool enableHotkeys = true;

        private IBroadcaster _bus;
        private IVehicleDataService _vehicle;     // opzionale (fallback lettura diretta)
        private float _speedKmh;                  // aggiornato via OnSpeedChanged o fallback da service
        private char  _selector = 'N';            // P,R,N,D
        private int   _driveGear = 1;             // 1..7

        // handler per l'evento velocità
        private System.Action<OnSpeedChanged> _onSpeedHandler;

        // ---------- Lifecycle ----------
        private void Awake()
        {
            Locator.TryResolve(out _bus);

            // Sottoscrizione all'evento velocità (preferito)
            _onSpeedHandler = (e) => { _speedKmh = e.kmh; };
            _bus?.Add(_onSpeedHandler);

            // Fallback: prova anche a risolvere il servizio (se serve)
            Locator.TryResolve(out _vehicle);

            if (!ui) ui = GetComponentInChildren<GearController>(true);

            // Applica preset se richiesto (utile contro valori “vecchi” salvati nell’Inspector)
            if (usePresetICXK3)
            {
                EnsureArrays();
                for (int i = 0; i < 7; i++) { upshift[i] = PRESET_UP[i]; downshift[i] = PRESET_DN[i]; }
            }
        }

        private void Start()
        {
            ApplyUI();

            if (_vehicle == null)
                Debug.Log("[GearLogicController] Nessun VehicleDataService nel Locator: uso solo gli eventi OnSpeedChanged (ok).");
        }

        private void OnDestroy()
        {
            if (_bus != null && _onSpeedHandler != null) _bus.Remove(_onSpeedHandler);
        }

        private void Update()
        {
            // Hotkeys PRND
            if (enableHotkeys)
            {
                if (Input.GetKeyDown(KeyCode.P)) SelectP();
                if (Input.GetKeyDown(KeyCode.R)) SelectR();
                if (Input.GetKeyDown(KeyCode.N)) SelectN();
                if (Input.GetKeyDown(KeyCode.D)) SelectD();
            }

            // Fallback: se mancano eventi, prova a leggere dal service
            if (_vehicle == null && Time.frameCount % 60 == 0) Locator.TryResolve(out _vehicle);
            if (_vehicle != null && Time.frameCount % 5 == 0)  _speedKmh = _vehicle.Speed;

            // Scala solo in D
            if (_selector == 'D')
            {
                int g = ComputeDriveGear(_speedKmh);
                if (g != _driveGear)
                {
                    _driveGear = g;
                    _bus?.Broadcast(new OnDriveGearChanged(_driveGear));
                    ui?.SetDriveGearNumber(_driveGear);
                }

                // safety UI: in D la pill deve vedersi con almeno "1"
                ui?.SetDriveGearNumber(Mathf.Max(_driveGear, 1));
            }
        }

        // ---------- API per UI Buttons / Input ----------
        public void SelectP() => SetSelector('P');
        public void SelectR() => SetSelector('R');
        public void SelectN() => SetSelector('N');
        public void SelectD() => SetSelector('D');

        public void SetSelector(char s)
        {
            _selector = s;
            if (_selector == 'D' && _driveGear < 1) _driveGear = 1;
            _bus?.Broadcast(new OnGearSelectorChanged(_selector));
            ApplyUI();
        }

        // ---------- Interni ----------
        private void ApplyUI()
        {
            ui?.SetSelector(_selector);
            ui?.SetDriveGearNumber(_selector == 'D' ? Mathf.Max(_driveGear, 1) : 0);
        }

        /// <summary>
        /// Calcolo rapporto con isteresi: sale ai confini 'upshift', scende ai confini 'downshift'.
        /// </summary>
        private int ComputeDriveGear(float v)
        {
            EnsureArrays();

            int g = Mathf.Clamp(_driveGear, 1, 7);

            // Upshift: usa upshift[g-1] per g -> g+1
            while (g < 7 && v >= upshift[g - 1]) g++;

            // Downshift: usa downshift[g-2] per g -> g-1
            while (g > 1 && v <= downshift[g - 2]) g--;

            return Mathf.Clamp(g, 1, 7);
        }

        private void EnsureArrays()
        {
            if (upshift == null || upshift.Length != 7)   upshift   = new float[7];
            if (downshift == null || downshift.Length != 7) downshift = new float[7];
        }

#if UNITY_EDITOR
        // Assicura che l’Inspector non tenga valori “stagnanti” se vuoi il preset sempre attivo.
        private void OnValidate()
        {
            if (!usePresetICXK3) return;
            EnsureArrays();
            for (int i = 0; i < 7; i++) { upshift[i] = PRESET_UP[i]; downshift[i] = PRESET_DN[i]; }
        }
#endif
    }
}
