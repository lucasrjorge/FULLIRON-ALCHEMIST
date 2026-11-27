using System;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [Serializable]
    public class ReactionRequirement
    {
        public AlchemicalElement anchorElement;
        public AlchemicalComponent exactComponent;
        public bool ignoreComponent;
        public bool requireState;
        public ComponentState requiredState = ComponentState.Raw;
        public EffectTag requiredTags;
        public bool matchAllTags = true;

        public bool Matches(AlchemicalItem item)
        {
            if (item == null || item.Component == null)
                return false;

            if (!ignoreComponent && exactComponent != null && item.Component != exactComponent)
                return false;

            if (requireState && item.State != requiredState)
                return false;

            if (requiredTags != EffectTag.None)
            {
                if (matchAllTags)
                {
                    if ((item.Tags & requiredTags) != requiredTags)
                        return false;
                }
                else if ((item.Tags & requiredTags) == EffectTag.None)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
