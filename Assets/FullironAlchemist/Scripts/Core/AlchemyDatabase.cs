using System.Collections.Generic;
using UnityEngine;

namespace Fulliron.Alchemy
{
    [CreateAssetMenu(menuName = "Fulliron/Alchemy Database", fileName = "AlchemyDatabase")]
    public class AlchemyDatabase : ScriptableObject
    {
        [Header("Componentes")]
        public List<AlchemicalComponent> components = new List<AlchemicalComponent>();

        [Header("Perfis de processo")]
        public List<ProcessingProfile> processingProfiles = new List<ProcessingProfile>();

        [Header("Receitas de reação")]
        public List<ReactionRecipe> reactionRecipes = new List<ReactionRecipe>();

        public AlchemicalComponent FindComponent(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || components == null)
                return null;

            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                if (component != null && component.Id == id)
                    return component;
            }

            return null;
        }

        public ProcessingProfile FindProfile(
            AlchemicalComponent input,
            ComponentState state,
            ProcessType process)
        {
            if (input == null || processingProfiles == null)
                return null;

            for (int i = 0; i < processingProfiles.Count; i++)
            {
                var profile = processingProfiles[i];
                if (profile == null || profile.input != input || profile.process != process)
                    continue;
                if (!profile.anyInputState && profile.inputState != state)
                    continue;
                return profile;
            }

            return null;
        }

        public bool TryGetOutcome(
            AlchemicalComponent input,
            ComponentState state,
            ProcessType process,
            float progress,
            out ProcessingOutcome outcome)
        {
            outcome = null;
            var profile = FindProfile(input, state, process);
            if (profile == null)
                return false;

            outcome = profile.FindOutcome(progress);
            return outcome != null;
        }

        [ContextMenu("Validar banco")]
        public void Validate()
        {
            var ids = new HashSet<string>();

            if (components != null)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (component == null)
                    {
                        Debug.LogWarning($"[Fulliron] AlchemyDatabase '{name}': componente {i} nulo.", this);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(component.Id))
                        Debug.LogWarning($"[Fulliron] AlchemyDatabase '{name}': componente '{component.name}' sem id.", this);
                    else if (!ids.Add(component.Id))
                        Debug.LogWarning($"[Fulliron] AlchemyDatabase '{name}': id duplicado '{component.Id}'.", this);
                }
            }

            if (processingProfiles != null)
            {
                for (int i = 0; i < processingProfiles.Count; i++)
                {
                    var profile = processingProfiles[i];
                    if (profile == null)
                    {
                        Debug.LogWarning($"[Fulliron] AlchemyDatabase '{name}': perfil {i} nulo.", this);
                        continue;
                    }
                    profile.Validate();
                }
            }

            Debug.Log($"[Fulliron] AlchemyDatabase '{name}': validação concluída.", this);
        }
    }
}
