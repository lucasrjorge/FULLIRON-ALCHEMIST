using UnityEngine;

namespace Fulliron.Alchemy
{
    [CreateAssetMenu(menuName = "Fulliron/Alchemical Component", fileName = "Component_")]
    public class AlchemicalComponent : ScriptableObject
    {
        [Header("Identidade")]
        [SerializeField] string id;
        [SerializeField] string displayName;
        [TextArea(2, 5)]
        [SerializeField] string description;

        [Header("Alquimia")]
        [SerializeField] bool primary;
        [SerializeField] ComponentState defaultState = ComponentState.Raw;
        [SerializeField] EffectTag tags = EffectTag.None;

        [Header("Resíduos (Terra)")]
        [SerializeField] AlchemicalComponent dissolutionResidue;
        [SerializeField] AlchemicalComponent calcinationResidueWeak;
        [SerializeField] AlchemicalComponent calcinationResidueStrong;

        [Header("Aparência")]
        [SerializeField] Color color = Color.white;
        [ColorUsage(false, true)]
        [SerializeField] Color emissionColor = Color.black;
        [SerializeField, Min(0f)] float emissionIntensity;

        public string Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public bool IsPrimary => primary;
        public ComponentState DefaultState => defaultState;
        public EffectTag Tags => tags;
        public AlchemicalComponent DissolutionResidue => dissolutionResidue;
        public AlchemicalComponent CalcinationResidueWeak => calcinationResidueWeak;
        public AlchemicalComponent CalcinationResidueStrong => calcinationResidueStrong;
        public Color Tint => color;
        public Color EmissionColor => emissionColor;
        public float EmissionIntensity => emissionIntensity;

        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = name;
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = name;
        }
    }
}
