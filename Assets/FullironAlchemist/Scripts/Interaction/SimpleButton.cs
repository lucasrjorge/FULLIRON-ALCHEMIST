using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Fulliron.Alchemy
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class SimpleButton : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] Transform cap;
        [SerializeField] Vector3 pressDirection = Vector3.down;
        [SerializeField, Min(0f)] float pressDepth = 0.02f;
        [SerializeField, Min(0.01f)] float pressDuration = 0.08f;
        [SerializeField, Min(0.01f)] float returnDuration = 0.15f;

        [Header("Evento")]
        [SerializeField] UnityEvent onPressed = new UnityEvent();

        XRSimpleInteractable interactable;
        Vector3 capRestPosition;
        Coroutine pressRoutine;
        bool isPressing;

        public UnityEvent OnPressed => onPressed;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            if (cap != null)
                capRestPosition = cap.localPosition;
            var collider = GetComponent<Collider>();
            if (collider == null)
                Debug.LogWarning($"[Fulliron] SimpleButton '{name}': adicione um Collider no mesmo GameObject do XRSimpleInteractable.", this);
            else if (collider.isTrigger)
                Debug.LogWarning($"[Fulliron] SimpleButton '{name}': o Collider do botão não deve ser trigger.", this);
            if (interactable == null)
                Debug.LogError($"[Fulliron] SimpleButton '{name}': XRSimpleInteractable ausente.", this);
        }

        void OnEnable()
        {
            if (interactable != null)
                interactable.selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            if (interactable != null)
                interactable.selectEntered.RemoveListener(OnSelectEntered);

            if (pressRoutine != null)
            {
                StopCoroutine(pressRoutine);
                pressRoutine = null;
            }

            if (cap != null)
                cap.localPosition = capRestPosition;

            isPressing = false;
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (isPressing)
                return;
            if (pressRoutine != null)
                StopCoroutine(pressRoutine);
            pressRoutine = StartCoroutine(PressRoutine());
        }

        IEnumerator PressRoutine()
        {
            isPressing = true;
            onPressed.Invoke();
            PrototypeLog.Log($"Botão '{name}' pressionado.");

            if (cap != null)
            {
                Vector3 pressed = capRestPosition + pressDirection.normalized * pressDepth;
                yield return MoveCap(capRestPosition, pressed, pressDuration);
                yield return MoveCap(pressed, capRestPosition, returnDuration);
            }
            else
            {
                yield return new WaitForSeconds(pressDuration);
            }

            isPressing = false;
            pressRoutine = null;
        }

        IEnumerator MoveCap(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cap.localPosition = Vector3.Lerp(from, to, t);
                yield return null;
            }

            cap.localPosition = to;
        }
    }
}
