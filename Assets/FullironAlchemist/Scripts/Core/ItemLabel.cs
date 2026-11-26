using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class ItemLabel : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] AlchemicalItem item;
        [SerializeField] TMP_Text label;

        [Header("Comportamento")]
        [SerializeField] bool showTags = true;
        [SerializeField] bool faceCamera = true;
        [SerializeField] bool flipFacing;
        [SerializeField] bool hideWhenEmpty;

        void Awake()
        {
            if (item == null)
                item = GetComponentInParent<AlchemicalItem>();
            if (label == null)
                label = GetComponent<TMP_Text>();
            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);

            if (item != null)
                item.Changed += OnItemChanged;

            Refresh();
        }

        void OnDestroy()
        {
            if (item != null)
                item.Changed -= OnItemChanged;
        }

        void LateUpdate()
        {
            if (!faceCamera)
                return;

            var camera = Camera.main;
            if (camera == null)
                return;

            transform.rotation = camera.transform.rotation;
            if (flipFacing)
                transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
        }

        void OnItemChanged(AlchemicalItem changed)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (label == null)
                return;

            if (item == null || item.Component == null)
            {
                label.text = hideWhenEmpty ? string.Empty : "Sem componente";
                return;
            }

            string text = $"{item.Component.DisplayName}\n{item.State}";
            if (showTags)
            {
                string tags = item.Tags == EffectTag.None ? "Sem tags" : item.Tags.ToString();
                text += $"\n{tags}";
            }

            label.text = text;
        }
    }
}
