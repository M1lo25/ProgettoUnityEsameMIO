using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ICXK3
{
    public class GearController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform pill;
        [SerializeField] private TMP_Text pillText;
        [SerializeField] private RectTransform labelP;
        [SerializeField] private RectTransform labelR;
        [SerializeField] private RectTransform labelN;
        [SerializeField] private RectTransform labelD;

        [Header("Style")]
        [SerializeField] private Color labelNormal = new Color32(123,138,150,255);
        [SerializeField] private Color labelActive = new Color32(211,219,226,255);
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

        private TMP_Text _txtP, _txtR, _txtN, _txtD;
        private int _curIndex = 2;
        private Coroutine _moveCo;

        private void Awake()
        {
            _txtP = labelP ? labelP.GetComponent<TMP_Text>() : null;
            _txtR = labelR ? labelR.GetComponent<TMP_Text>() : null;
            _txtN = labelN ? labelN.GetComponent<TMP_Text>() : null;
            _txtD = labelD ? labelD.GetComponent<TMP_Text>() : null;
            Highlight();
            if (pill) pill.gameObject.SetActive(false);
        }

        public void SetSelector(char selPRND)
        {
            int idx = selPRND switch { 'P'=>0, 'R'=>1, 'N'=>2, 'D'=>3, _=>_curIndex };
            if (idx == _curIndex) return;
            _curIndex = idx;
            Highlight();
            MovePillToCurrent();

            // se entro in D, mostra la pill anche a veicolo fermo (gear minimo 1)
            if (_curIndex == 3) {
                if (pill && !pill.gameObject.activeSelf) pill.gameObject.SetActive(true);
                if (pillText && string.IsNullOrEmpty(pillText.text)) pillText.text = "1";
            } else {
                if (pill) pill.gameObject.SetActive(false);
                if (pillText) pillText.text = "";
            }
        }

        public void SetDriveGearNumber(int n) 
        {
            if (!pillText) return;
            if (_curIndex != 3 || n <= 0)
            {
                pillText.text = "";
                if (pill) pill.gameObject.SetActive(false);
                return;
            }
            pillText.text = Mathf.Clamp(n, 1, 7).ToString();
            if (pill && !pill.gameObject.activeSelf) pill.gameObject.SetActive(true);
        }

        private void Highlight()
        {
            if (_txtP) _txtP.color = (_curIndex==0)? labelActive : labelNormal;
            if (_txtR) _txtR.color = (_curIndex==1)? labelActive : labelNormal;
            if (_txtN) _txtN.color = (_curIndex==2)? labelActive : labelNormal;
            if (_txtD) _txtD.color = (_curIndex==3)? labelActive : labelNormal;
        }

        private void MovePillToCurrent()
        {
            if (!pill) return;
            RectTransform target = _curIndex switch { 0=>labelP, 1=>labelR, 2=>labelN, 3=>labelD, _=>null };
            if (!target) return;

            Vector3 world = target.TransformPoint(Vector3.zero);
            Vector3 local = pill.parent.InverseTransformPoint(world);
            float startX = pill.anchoredPosition.x;
            float endX = local.x;

            if (_moveCo != null) StopCoroutine(_moveCo);
            _moveCo = StartCoroutine(CoMove(startX, endX));
        }

        private IEnumerator CoMove(float fromX, float toX)
        {
            float t = 0f;
            var start = new Vector2(fromX, pill.anchoredPosition.y);
            var end   = new Vector2(toX,   pill.anchoredPosition.y);
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.001f, moveDuration);
                float k = ease.Evaluate(Mathf.Clamp01(t));
                pill.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
                yield return null;
            }
            pill.anchoredPosition = end;
        }
    }
}
