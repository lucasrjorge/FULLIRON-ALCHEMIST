using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Fulliron.Alchemy
{
    public class ItemLabel : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] AlchemicalItem item;
        [SerializeField] TMP_Text label;
        [SerializeField] XRGrabInteractable grabInteractable;

        [Header("Comportamento")]
        [SerializeField] bool showOnHover = true;
        [SerializeField] bool showOnSelect = true;
        [SerializeField] bool hideWhenSocketed = true;
        [SerializeField] bool showTags;
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
            if (grabInteractable == null)
                grabInteractable = GetComponentInParent<XRGrabInteractable>();

            if (item != null)
                item.Changed += OnItemChanged;

            if (grabInteractable != null)
            {
                grabInteractable.hoverEntered.AddListener(OnHoverEntered);
                grabInteractable.hoverExited.AddListener(OnHoverExited);
                grabInteractable.selectEntered.AddListener(OnSelectEntered);
                grabInteractable.selectExited.AddListener(OnSelectExited);
            }

            Refresh();
        }

        void OnDestroy()
        {
            if (item != null)
                item.Changed -= OnItemChanged;

            if (grabInteractable != null)
            {
                grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
                grabInteractable.hoverExited.RemoveListener(OnHoverExited);
                grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
                grabInteractable.selectExited.RemoveListener(OnSelectExited);
            }
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

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            DiscoverComponent();
            Refresh();
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            Refresh();
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            DiscoverComponent();
            Refresh();
        }

        void DiscoverComponent()
        {
            if (item != null && item.Component != null && GrimoireManager.Instance != null)
                GrimoireManager.Instance.DiscoverComponent(item.Component);
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (label == null)
                return;

            string text;
            if (item == null || item.Component == null)
            {
                text = hideWhenEmpty ? string.Empty : "Sem componente";
            }
            else
            {
                text = $"{item.Component.DisplayName}\n{item.State}";
                if (showTags)
                {
                    string tags = item.Tags == EffectTag.None ? "Sem tags" : item.Tags.ToString();
                    text += $"\n{tags}";
                }
            }

            label.text = text;

            bool socketed = false;
            if (grabInteractable != null && hideWhenSocketed)
            {
                var selecting = grabInteractable.interactorsSelecting;
                for (int i = 0; i < selecting.Count; i++)
                {
                    if (selecting[i] is XRSocketInteractor)
                    {
                        socketed = true;
                        break;
                    }
                }
            }

            bool hovered = grabInteractable != null && grabInteractable.isHovered;
            bool selected = grabInteractable != null && grabInteractable.isSelected;
            bool visible = !socketed &&
                           ((showOnHover && hovered) || (showOnSelect && selected));

            label.enabled = visible;
        }
    }
}
