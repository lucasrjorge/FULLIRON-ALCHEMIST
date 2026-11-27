using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Fulliron.Alchemy
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public class ItemSocket : MonoBehaviour
    {
        [SerializeField] AlchemicalElement element = AlchemicalElement.None;
        [SerializeField] bool logChanges = true;

        XRSocketInteractor socket;

        public AlchemicalElement Element => element;
        public XRSocketInteractor Socket => socket;

        public AlchemicalItem CurrentItem
        {
            get
            {
                if (socket == null)
                    socket = GetComponent<XRSocketInteractor>();
                if (socket == null || socket.interactablesSelected.Count == 0)
                    return null;
                return socket.interactablesSelected[0].transform.GetComponent<AlchemicalItem>();
            }
        }

        public bool HasItem => CurrentItem != null;

        void Awake()
        {
            socket = GetComponent<XRSocketInteractor>();
            if (socket == null)
                Debug.LogError($"[Fulliron] ItemSocket '{name}': XRSocketInteractor ausente.", this);

            var collider = GetComponent<Collider>();
            if (collider == null)
                Debug.LogWarning($"[Fulliron] ItemSocket '{name}': adicione um Collider trigger.", this);
            else if (!collider.isTrigger)
                Debug.LogWarning($"[Fulliron] ItemSocket '{name}': o Collider deve ser trigger.", this);
        }

        void OnEnable()
        {
            if (socket == null)
                socket = GetComponent<XRSocketInteractor>();

            if (socket != null)
            {
                socket.selectEntered.AddListener(OnSelectEntered);
                socket.selectExited.AddListener(OnSelectExited);
            }
        }

        void OnDisable()
        {
            if (socket != null)
            {
                socket.selectEntered.RemoveListener(OnSelectEntered);
                socket.selectExited.RemoveListener(OnSelectExited);
            }
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            var item = args.interactableObject.transform.GetComponent<AlchemicalItem>();
            if (item == null)
            {
                if (logChanges)
                    PrototypeLog.Log($"Socket '{name}': item sem AlchemicalItem.");
                return;
            }

            if (logChanges)
                PrototypeLog.Log($"Socket '{name}': recebeu {item.DisplayName}.");
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            var item = args.interactableObject.transform.GetComponent<AlchemicalItem>();
            if (item != null && logChanges)
                PrototypeLog.Log($"Socket '{name}': removeu {item.DisplayName}.");
        }

        [ContextMenu("Debug/Log item atual")]
        void DebugLogCurrentItem()
        {
            var item = CurrentItem;
            PrototypeLog.Log($"Socket '{name}' ({(element == AlchemicalElement.None ? "qualquer" : element.ToString())}): " +
                             (item != null ? item.DisplayName : "vazio"));
        }
    }
}
