using System.Collections.Generic;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class ComponentTray : MonoBehaviour
    {
        [SerializeField] ItemSocket[] slots;

        public int Capacity => slots != null ? slots.Length : 0;

        public int Count
        {
            get
            {
                if (slots == null)
                    return 0;

                int count = 0;
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i] != null && slots[i].HasItem)
                        count++;
                return count;
            }
        }

        public IReadOnlyList<AlchemicalItem> GetItems()
        {
            var items = new List<AlchemicalItem>();
            if (slots == null)
                return items;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                    continue;
                var item = slots[i].CurrentItem;
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        [ContextMenu("Debug/Listar itens")]
        void DebugListItems()
        {
            var items = GetItems();
            if (items.Count == 0)
            {
                PrototypeLog.Log($"Bandeja '{name}': vazia.");
                return;
            }

            for (int i = 0; i < items.Count; i++)
                PrototypeLog.Log($"Bandeja '{name}' [{i}]: {items[i].DisplayName}.");
        }
    }
}
