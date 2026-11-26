using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Fulliron.Alchemy
{
    public class ShelfRow : MonoBehaviour
    {
        [Header("Conteúdo")]
        [SerializeField] AlchemicalComponent component;
        [SerializeField] AlchemicalItem itemPrefab;

        [Header("Slots")]
        [SerializeField] Transform[] slots;
        [SerializeField, Min(0.01f)] float occupiedRadius = 0.15f;

        [Header("Reposição")]
        [SerializeField] SimpleButton restockButton;
        [SerializeField] ParticleSystem restockVfx;
        [SerializeField] bool restockOnStart = true;

        readonly List<AlchemicalItem> spawned = new List<AlchemicalItem>();

        void Start()
        {
            if (restockButton != null)
                restockButton.OnPressed.AddListener(Restock);
            if (restockOnStart)
                Restock();
        }

        void OnDestroy()
        {
            if (restockButton != null)
                restockButton.OnPressed.RemoveListener(Restock);
        }

        public void Restock()
        {
            if (component == null)
            {
                PrototypeLog.Log($"Estante '{name}': componente não configurado.");
                return;
            }
            if (itemPrefab == null)
            {
                PrototypeLog.Log($"Estante '{name}': prefab de item não configurado.");
                return;
            }
            if (slots == null || slots.Length == 0)
            {
                PrototypeLog.Log($"Estante '{name}': nenhum slot configurado.");
                return;
            }

            PruneSpawned();
            int created = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null || IsSlotOccupied(slot))
                    continue;

                var item = Instantiate(itemPrefab, slot.position, slot.rotation, slot);
                item.SetData(component, component.DefaultState);
                spawned.Add(item);
                created++;
            }

            if (created > 0 && restockVfx != null)
                restockVfx.Play();

            PrototypeLog.Log($"Estante '{name}': repostos {created} item(ns) de {component.DisplayName}.");
        }

        bool IsSlotOccupied(Transform slot)
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                var item = spawned[i];
                if (item == null || IsHeld(item))
                    continue;
                if (Vector3.Distance(item.transform.position, slot.position) <= occupiedRadius)
                    return true;
            }

            return false;
        }

        static bool IsHeld(AlchemicalItem item)
        {
            return item.TryGetComponent<XRGrabInteractable>(out var grab) && grab.isSelected;
        }

        void PruneSpawned()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
                if (spawned[i] == null)
                    spawned.RemoveAt(i);
        }
    }
}
