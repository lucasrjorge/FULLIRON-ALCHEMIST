using System.Collections.Generic;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [CreateAssetMenu(menuName = "Fulliron/Processing Profile", fileName = "Profile_")]
    public class ProcessingProfile : ScriptableObject
    {
        [Header("Entrada")]
        public AlchemicalComponent input;
        public bool anyInputState = true;
        public ComponentState inputState = ComponentState.Raw;

        [Header("Processo")]
        public ProcessType process = ProcessType.None;

        [Header("Resultados")]
        public List<ProcessingOutcome> outcomes = new List<ProcessingOutcome>();

        [Header("Interrupção")]
        public ProcessingOutcome interruptOutcome;

        public ProcessingOutcome FindOutcome(float progress)
        {
            if (outcomes == null)
                return null;

            for (int i = 0; i < outcomes.Count; i++)
            {
                var outcome = outcomes[i];
                if (outcome != null && outcome.Matches(progress))
                    return outcome;
            }

            return null;
        }

        public ProcessingOutcome GetInterruptOutcome()
        {
            return interruptOutcome != null && interruptOutcome.output != null ? interruptOutcome : null;
        }

        public void Validate()
        {
            if (input == null)
                Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': input vazio.", this);
            if (process == ProcessType.None)
                Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': processo None.", this);
            if (outcomes == null || outcomes.Count == 0)
            {
                Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': nenhum outcome.", this);
                return;
            }

            for (int i = 0; i < outcomes.Count; i++)
            {
                var a = outcomes[i];
                if (a == null)
                {
                    Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': outcome {i} nulo.", this);
                    continue;
                }

                if (a.output == null)
                    Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': outcome {i} sem output.", this);
                if (a.minProgress > a.maxProgress)
                    Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': outcome {i} com faixa invertida.", this);

                for (int j = i + 1; j < outcomes.Count; j++)
                {
                    var b = outcomes[j];
                    if (b == null)
                        continue;
                    if (a.minProgress <= b.maxProgress && b.minProgress <= a.maxProgress)
                        Debug.LogWarning($"[Fulliron] ProcessingProfile '{name}': outcomes {i} e {j} se sobrepõem.", this);
                }
            }
        }
    }
}
