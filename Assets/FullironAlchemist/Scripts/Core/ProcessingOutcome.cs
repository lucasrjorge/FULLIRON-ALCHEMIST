using System;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [Serializable]
    public class ProcessingOutcome
    {
        [Range(0f, 1f)] public float minProgress;
        [Range(0f, 1f)] public float maxProgress = 1f;
        public AlchemicalComponent output;
        public ComponentState outputState = ComponentState.Raw;
        public AlchemicalComponent residue;
        [TextArea(1, 3)] public string message;

        public bool Matches(float progress)
        {
            return progress >= minProgress && progress <= maxProgress;
        }
    }
}
