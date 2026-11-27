using System;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class AlchemicalItem : MonoBehaviour
    {
        [Header("Estado inicial (opcional)")]
        [SerializeField] AlchemicalComponent initialComponent;
        [SerializeField] ComponentState initialState = ComponentState.Raw;
        [SerializeField] bool applyInitialOnAwake;

        [Header("Histórico")]
        [SerializeField] ProcessHistory history = new ProcessHistory();

        AlchemicalComponent component;
        ComponentState state = ComponentState.Raw;
        EffectTag additionalTags = EffectTag.None;

        public event Action<AlchemicalItem> Changed;
        public event Action<AlchemicalItem> Consumed;

        public AlchemicalComponent Component => component;
        public ComponentState State => state;
        public EffectTag Tags => (component != null ? component.Tags : EffectTag.None) | additionalTags;
        public ProcessHistory History => history;
        public bool IsEmpty => component == null;

        public string DisplayName
        {
            get
            {
                if (component == null)
                    return "Sem componente";
                return $"{component.DisplayName} ({state})";
            }
        }

        void Awake()
        {
            if (history == null)
                history = new ProcessHistory();
            if (applyInitialOnAwake && initialComponent != null)
                SetData(initialComponent, initialState);
        }

        public void SetData(AlchemicalComponent newComponent, ComponentState newState)
        {
            component = newComponent;
            state = newState;
            additionalTags = EffectTag.None;
            Changed?.Invoke(this);
        }

        public void SetData(AlchemicalComponent newComponent)
        {
            SetData(newComponent, newComponent != null ? newComponent.DefaultState : ComponentState.Raw);
        }

        public void ApplyProcess(ProcessType process, float intensity, ProcessingOutcome outcome, float duration = 0f)
        {
            if (outcome == null || outcome.output == null)
            {
                PrototypeLog.Log($"{name}: processo {process} sem resultado válido.");
                return;
            }
            if (history == null)
                history = new ProcessHistory();

            history.Add(process, intensity, duration, outcome.output.Id);
            SetData(outcome.output, outcome.outputState);

            if (!string.IsNullOrWhiteSpace(outcome.message))
                PrototypeLog.Log($"{name}: {outcome.message}");

            PrototypeLog.Log($"{name}: {process} ({intensity:0.00}) -> {outcome.output.DisplayName} ({outcome.outputState}).");
        }

        public void AddTag(EffectTag tag)
        {
            if (tag == EffectTag.None)
                return;
            additionalTags |= tag;
            Changed?.Invoke(this);
        }

        public void RemoveTag(EffectTag tag)
        {
            if (tag == EffectTag.None)
                return;
            additionalTags &= ~tag;
            Changed?.Invoke(this);
        }

        public bool HasTag(EffectTag tag)
        {
            if (tag == EffectTag.None)
                return true;
            return (Tags & tag) == tag;
        }

        public void ResetToDefault()
        {
            if (history == null)
                history = new ProcessHistory();
            history.Clear();
            SetData(initialComponent, initialState);
        }

        public void Consume()
        {
            Consumed?.Invoke(this);
            Destroy(gameObject);
        }

        [ContextMenu("Debug/Ciclar estado")]
        void DebugCycleState()
        {
            var values = (ComponentState[])Enum.GetValues(typeof(ComponentState));
            if (values.Length == 0)
                return;

            int index = Array.IndexOf(values, state);
            index = (index + 1) % values.Length;
            SetData(component, values[index]);
            PrototypeLog.Log($"{name}: estado de teste -> {values[index]}.");
        }
    }
}
