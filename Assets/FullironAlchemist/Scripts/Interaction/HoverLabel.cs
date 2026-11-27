using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Fulliron.Alchemy
{
    public class HoverLabel : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] bool faceCamera = true;

        XRBaseInteractable interactable;
        XRBaseInteractor interactor;
        Renderer[] renderers;
        bool hovered;
        bool selected;

        void Awake()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);

            interactable = GetComponentInParent<XRBaseInteractable>();
            if (interactable == null)
                interactor = GetComponentInParent<XRBaseInteractor>();

            renderers = GetComponentsInChildren<Renderer>(true);
            SetVisible(false);
        }

        void OnEnable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.AddListener(OnHoverEntered);
                interactable.hoverExited.AddListener(OnHoverExited);
                interactable.selectEntered.AddListener(OnSelectEntered);
                interactable.selectExited.AddListener(OnSelectExited);
            }

            if (interactor != null)
            {
                interactor.hoverEntered.AddListener(OnHoverEntered);
                interactor.hoverExited.AddListener(OnHoverExited);
            }
        }

        void OnDisable()
        {
            if (interactable != null)
            {
                interactable.hoverEntered.RemoveListener(OnHoverEntered);
                interactable.hoverExited.RemoveListener(OnHoverExited);
                interactable.selectEntered.RemoveListener(OnSelectEntered);
                interactable.selectExited.RemoveListener(OnSelectExited);
            }

            if (interactor != null)
            {
                interactor.hoverEntered.RemoveListener(OnHoverEntered);
                interactor.hoverExited.RemoveListener(OnHoverExited);
            }
        }

        void LateUpdate()
        {
            if (!faceCamera || (!hovered && !selected))
                return;

            var camera = Camera.main;
            if (camera == null)
                return;

            transform.rotation = camera.transform.rotation;
        }

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            hovered = true;
            SetVisible(true);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            hovered = false;
            SetVisible(selected);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            selected = true;
            SetVisible(true);
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            selected = false;
            SetVisible(hovered);
        }

        void SetVisible(bool visible)
        {
            if (label != null)
                label.enabled = visible;

            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = visible;
            }
        }
    }
}
