using System;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [Serializable]
    public class FailureProfile
    {
        public bool consumeComponents;
        public bool recordInGrimoire = true;
        [TextArea(1, 3)] public string failureMessage = "A reação não se completou.";
    }
}
