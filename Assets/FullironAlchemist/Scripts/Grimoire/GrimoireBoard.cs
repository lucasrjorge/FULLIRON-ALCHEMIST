using System.Text;
using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class GrimoireBoard : MonoBehaviour
    {
        [SerializeField] GrimoireManager manager;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text reactionsText;
        [SerializeField] TMP_Text componentsText;

        bool subscribed;

        void Awake()
        {
            if (manager == null)
                manager = GrimoireManager.Instance;
            Subscribe();
            Refresh();
        }

        void Start()
        {
            if (manager == null)
                manager = GrimoireManager.Instance;
            Subscribe();
            Refresh();
        }

        void OnDestroy()
        {
            if (manager != null && subscribed)
                manager.Changed -= Refresh;
        }

        void Subscribe()
        {
            if (manager == null || subscribed)
                return;
            manager.Changed += Refresh;
            subscribed = true;
        }

        public void Refresh()
        {
            if (manager == null)
                manager = GrimoireManager.Instance;
            if (manager == null)
                return;

            var database = manager.Database;
            if (titleText != null)
                titleText.text = "Grimório";

            if (reactionsText != null)
                reactionsText.text = BuildReactions(database);
            if (componentsText != null)
                componentsText.text = BuildComponents(database);
        }

        string BuildReactions(AlchemyDatabase database)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Reações:");

            if (database == null || database.reactionRecipes == null || database.reactionRecipes.Count == 0)
            {
                builder.AppendLine("  (sem páginas)");
                return builder.ToString();
            }

            for (int i = 0; i < database.reactionRecipes.Count; i++)
            {
                var recipe = database.reactionRecipes[i];
                if (recipe == null)
                    continue;

                bool known = manager.IsRecipeDiscovered(recipe);
                string resultName = recipe.result != null ? recipe.result.DisplayName : "???";
                builder.AppendLine(known
                    ? "• " + recipe.displayName + " -> " + resultName
                    : "• ??? -> " + resultName);

                if (!known)
                {
                    builder.AppendLine("  Componentes: ???");
                    continue;
                }

                builder.AppendLine("  Símbolo: " + recipe.symbolId);
                for (int j = 0; j < recipe.requirements.Count; j++)
                {
                    var requirement = recipe.requirements[j];
                    if (requirement == null)
                        continue;
                    builder.AppendLine("  " + requirement.anchorElement + ": " + RequirementLabel(requirement));
                }
            }

            return builder.ToString();
        }

        string BuildComponents(AlchemyDatabase database)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Componentes descobertos:");

            int count = 0;
            if (database != null && database.components != null)
            {
                for (int i = 0; i < database.components.Count; i++)
                {
                    var component = database.components[i];
                    if (component == null || !manager.IsComponentDiscovered(component))
                        continue;
                    builder.AppendLine("- " + component.DisplayName);
                    count++;
                }
            }

            if (count == 0)
                builder.AppendLine("  (nenhum)");

            if (!string.IsNullOrEmpty(manager.LastFailure))
                builder.AppendLine("\nÚltima falha: " + manager.LastFailure);

            return builder.ToString();
        }

        static string RequirementLabel(ReactionRequirement requirement)
        {
            var builder = new StringBuilder();

            if (!requirement.ignoreComponent && requirement.exactComponent != null)
                builder.Append(requirement.exactComponent.DisplayName);
            else if (requirement.requiredTags != EffectTag.None)
                builder.Append(requirement.requiredTags.ToString());
            else
                builder.Append("???");

            if (requirement.requireState)
                builder.Append(" (").Append(requirement.requiredState).Append(")");

            return builder.ToString();
        }
    }
}
