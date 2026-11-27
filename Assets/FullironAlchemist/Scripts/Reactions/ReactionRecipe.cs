using System.Collections.Generic;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [CreateAssetMenu(menuName = "Fulliron/Reaction Recipe", fileName = "Reaction_")]
    public class ReactionRecipe : ScriptableObject
    {
        public string id;
        public string displayName;
        public string symbolId;
        [TextArea(2, 4)] public string resultDescription;
        public List<ReactionRequirement> requirements = new List<ReactionRequirement>();
        public AlchemicalComponent result;
        public ComponentState resultState = ComponentState.Raw;
        public bool consumeComponents = true;
        public FailureProfile failureProfile = new FailureProfile();
        public int priority;
    }
}
