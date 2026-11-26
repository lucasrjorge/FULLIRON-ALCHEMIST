using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [RequireComponent(typeof(AlchemicalItem))]
    public class ComponentVisuals : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] AlchemicalItem item;
        [SerializeField] Transform visualRoot;
        [SerializeField] MeshRenderer[] renderers;
        [SerializeField] GameObject gasVisual;

        [Header("Comportamento")]
        [SerializeField] bool tintRenderers = true;
        [SerializeField] bool toggleGasVisual = true;
        [SerializeField] float gasScale = 1.25f;
        [SerializeField] float solidScale = 1f;

        MaterialPropertyBlock block;
        Vector3 initialVisualScale = Vector3.one;
        bool initialized;

        void Awake()
        {
            if (item == null)
                item = GetComponent<AlchemicalItem>();
            if (visualRoot != null)
                initialVisualScale = visualRoot.localScale;

            CacheRenderers();
            block = new MaterialPropertyBlock();
            initialized = true;

            if (item != null)
                item.Changed += OnItemChanged;

            Refresh();
        }

        void OnDestroy()
        {
            if (item != null)
                item.Changed -= OnItemChanged;
        }

        void OnItemChanged(AlchemicalItem changed)
        {
            Refresh();
        }

        public void Refresh()
        {
            if (!initialized || item == null)
                return;

            var component = item.Component;
            bool hasComponent = component != null;

            if (tintRenderers && renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    if (renderer == null)
                        continue;

                    renderer.GetPropertyBlock(block);
                    if (hasComponent)
                    {
                        block.SetColor("_BaseColor", component.Tint);
                        block.SetColor("_Color", component.Tint);
                        block.SetColor("_EmissionColor", component.EmissionColor * component.EmissionIntensity);
                    }
                    else
                    {
                        block.SetColor("_BaseColor", Color.white);
                        block.SetColor("_Color", Color.white);
                        block.SetColor("_EmissionColor", Color.black);
                    }
                    renderer.SetPropertyBlock(block);
                }
            }

            bool isGas = hasComponent && IsGasState(item.State);

            if (visualRoot != null)
                visualRoot.localScale = initialVisualScale * (isGas ? gasScale : solidScale);

            if (toggleGasVisual && gasVisual != null && gasVisual.activeSelf != isGas)
                gasVisual.SetActive(isGas);
        }

        static bool IsGasState(ComponentState state)
        {
            return state == ComponentState.GasWeak ||
                   state == ComponentState.GasStable ||
                   state == ComponentState.GasUnstable;
        }

        void CacheRenderers()
        {
            if (renderers != null && renderers.Length > 0)
                return;

            var source = visualRoot != null ? visualRoot : transform;
            var found = source.GetComponentsInChildren<MeshRenderer>(true);
            var list = new List<MeshRenderer>();

            for (int i = 0; i < found.Length; i++)
            {
                if (found[i].GetComponent<TMP_Text>() != null)
                    continue;
                list.Add(found[i]);
            }

            renderers = list.ToArray();
        }
    }
}
