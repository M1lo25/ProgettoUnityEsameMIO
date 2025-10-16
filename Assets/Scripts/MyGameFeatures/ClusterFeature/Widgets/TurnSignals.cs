using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ICXK3
{
    public enum TurnSignalState { Off, Left, Right, Hazard }
    public struct OnTurnSignalChanged
    {
        public TurnSignalState state;
        public OnTurnSignalChanged(TurnSignalState s) { state = s; }
    }

    /// <summary>
    /// Gestione frecce sinistra/destra e 4 frecce con lampeggio ON/OFF deciso.
    /// Gerarchia auto-bind (facoltativa):
    ///   Header/LeftArrow         (Button/Image o qualunque Graphic)
    ///   Header/RightArrow        (Button/Image o qualunque Graphic)
    ///   Header/HazardArrows/HazardIcon (Graphic) + Button nel parent
    /// </summary>
    public class TurnSignals : MonoBehaviour
    {
        [Header("Buttons (facoltativi)")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button hazardButton;

        [Header("Icons to tint/blink")]
        [SerializeField] private Graphic leftIcon;
        [SerializeField] private Graphic rightIcon;
        [SerializeField] private Graphic hazardIcon;

        [Header("Colors")]
        [SerializeField] private Color inactiveColor = new Color32(168, 179, 188, 255); // grigio icone spente
        [SerializeField] private Color activeColor   = new Color32(35, 227, 210, 255);  // verde acqua acceso

        [Header("Blink timing (realtime)")]
        [Tooltip("Durata fase accesa")]
        [SerializeField, Range(0.05f, 1f)] private float onTime  = 0.25f;
        [Tooltip("Durata fase spenta")]
        [SerializeField, Range(0.05f, 1f)] private float offTime = 0.25f;

        [Header("Hotkeys (opzionali)")]
        [SerializeField] private bool enableHotkeys = false;
        [SerializeField] private KeyCode keyLeft   = KeyCode.Z; // personalizza se vuoi
        [SerializeField] private KeyCode keyRight  = KeyCode.X;
        [SerializeField] private KeyCode keyHazard = KeyCode.C;

        private IBroadcaster _bus;
        private Coroutine _blinkCo;
        private TurnSignalState _state = TurnSignalState.Off;

        void Awake()
        {
            Locator.TryResolve(out _bus);

            // Auto-bind dai nomi (facoltativo, puoi assegnare a mano da Inspector)
            if (!leftIcon)   leftIcon   = transform.Find("Header/LeftArrow")?.GetComponent<Graphic>();
            if (!rightIcon)  rightIcon  = transform.Find("Header/RightArrow")?.GetComponent<Graphic>();
            if (!hazardIcon) hazardIcon = transform.Find("Header/HazardArrows/HazardIcon")?.GetComponent<Graphic>();

            if (!leftButton)   leftButton   = leftIcon   ? leftIcon.GetComponentInParent<Button>()   : null;
            if (!rightButton)  rightButton  = rightIcon  ? rightIcon.GetComponentInParent<Button>()  : null;
            if (!hazardButton) hazardButton = hazardIcon ? hazardIcon.GetComponentInParent<Button>() : null;

            if (leftButton)   leftButton.onClick.AddListener(() => Toggle(TurnSignalState.Left));
            if (rightButton)  rightButton.onClick.AddListener(() => Toggle(TurnSignalState.Right));
            if (hazardButton) hazardButton.onClick.AddListener(() => Toggle(TurnSignalState.Hazard));

            ApplyVisuals();
        }

        void Update()
        {
            if (!enableHotkeys) return;

            if (Input.GetKeyDown(keyLeft))   Toggle(TurnSignalState.Left);
            if (Input.GetKeyDown(keyRight))  Toggle(TurnSignalState.Right);
            if (Input.GetKeyDown(keyHazard)) Toggle(TurnSignalState.Hazard);
        }

        void OnEnable()  => ApplyVisuals();
        void OnDisable() => StopBlink();

        // --- API pubbliche ----------------------------------------------------

        public void Toggle(TurnSignalState requested)
        {
            _state = (_state == requested) ? TurnSignalState.Off : requested;
            Broadcast();
            ApplyVisuals();
        }

        public void SetState(TurnSignalState s)
        {
            _state = s;
            Broadcast();
            ApplyVisuals();
        }

        public void Clear()
        {
            _state = TurnSignalState.Off;
            Broadcast();
            ApplyVisuals();
        }

        // --- Interni ----------------------------------------------------------

        private void Broadcast()
        {
            if (_bus == null) Locator.TryResolve(out _bus);
            _bus?.Broadcast(new OnTurnSignalChanged(_state));
        }

        private void ApplyVisuals()
        {
            // reset icone al colore di sfondo spento
            if (leftIcon)   leftIcon.color   = inactiveColor;
            if (rightIcon)  rightIcon.color  = inactiveColor;
            if (hazardIcon) hazardIcon.color = inactiveColor;

            StopBlink();

            switch (_state)
            {
                case TurnSignalState.Left:
                    _blinkCo = StartCoroutine(BlinkHard(leftIcon));
                    break;
                case TurnSignalState.Right:
                    _blinkCo = StartCoroutine(BlinkHard(rightIcon));
                    break;
                case TurnSignalState.Hazard:
                    _blinkCo = StartCoroutine(BlinkPairHard(leftIcon, rightIcon));
                    break;
                case TurnSignalState.Off:
                default:
                    // già riportate a inactiveColor
                    break;
            }
        }

        private IEnumerator BlinkHard(Graphic g)
        {
            if (!g) yield break;

            while (true)
            {
                g.color = activeColor;              // ON pieno
                yield return new WaitForSecondsRealtime(onTime);

                g.color = inactiveColor;            // OFF immediato (torna al colore di sfondo)
                yield return new WaitForSecondsRealtime(offTime);
            }
        }

        private IEnumerator BlinkPairHard(Graphic a, Graphic b)
        {
            while (true)
            {
                if (a) a.color = activeColor;
                if (b) b.color = activeColor;
                yield return new WaitForSecondsRealtime(onTime);

                if (a) a.color = inactiveColor;
                if (b) b.color = inactiveColor;
                yield return new WaitForSecondsRealtime(offTime);
            }
        }

        private void StopBlink()
        {
            if (_blinkCo != null) StopCoroutine(_blinkCo);
            _blinkCo = null;
        }
    }
}
