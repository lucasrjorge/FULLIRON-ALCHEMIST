using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class GaugeBar : MonoBehaviour
    {
        [SerializeField] Transform fill;
        [SerializeField] Renderer fillRenderer;
        [SerializeField] TMP_Text label;
        [SerializeField] Color lowColor = new Color(0.9f, 0.55f, 0.1f);
        [SerializeField] Color highColor = new Color(1f, 0.15f, 0.05f);

        Vector3 baseScale;
        Vector3 basePosition;
        float bottomY;

        void Awake()
        {
            if (fill != null)
            {
                baseScale = fill.localScale;
                basePosition = fill.localPosition;
                bottomY = basePosition.y - baseScale.y * 0.5f;
            }

            if (fillRenderer == null && fill != null)
                fillRenderer = fill.GetComponent<Renderer>();
        }

        public void SetValue(float normalized, string text = null)
        {
            normalized = Mathf.Clamp01(normalized);

            if (fill != null)
            {
                float height = baseScale.y * normalized;
                fill.localScale = new Vector3(baseScale.x, height, baseScale.z);
                fill.localPosition = new Vector3(basePosition.x, bottomY + height * 0.5f, basePosition.z);
            }

            if (fillRenderer != null)
            {
                var color = Color.Lerp(lowColor, highColor, normalized);
                fillRenderer.material.SetColor("_BaseColor", color);
                fillRenderer.material.SetColor("_Color", color);
            }

            if (label != null && text != null)
                label.text = text;
        }
    }
}
