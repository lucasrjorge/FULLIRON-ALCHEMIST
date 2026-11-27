using TMPro;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class SymbolTable : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] AlchemyDatabase database;
        [SerializeField] ItemSocket[] sockets;
        [SerializeField] SimpleButton castButton;
        [SerializeField] GrimoireManager grimoire;
        [SerializeField] AlchemicalItem resultItemPrefab;
        [SerializeField] Transform resultSpawn;
        [SerializeField] TMP_Text feedbackText;

        [Header("Símbolo demo")]
        [SerializeField] string symbolId = "ouro";

        void Start()
        {
            if (grimoire == null)
                grimoire = GrimoireManager.Instance;
            if (castButton != null)
                castButton.OnPressed.AddListener(Cast);
        }

        void OnDestroy()
        {
            if (castButton != null)
                castButton.OnPressed.RemoveListener(Cast);
        }

        public void Cast()
        {
            if (database == null)
            {
                Feedback("Banco de alquimia ausente.");
                return;
            }

            var request = new ReactionRequest { symbolId = symbolId };
            if (sockets != null)
            {
                for (int i = 0; i < sockets.Length; i++)
                {
                    var socket = sockets[i];
                    if (socket == null)
                        continue;
                    var item = socket.CurrentItem;
                    if (item != null)
                        request.placements.Add(new Placement(socket.Element, item));
                }
            }

            var result = ReactionResolver.Resolve(database, request);
            if (result.success)
            {
                var recipe = result.recipe;
                if (recipe.consumeComponents)
                {
                    for (int i = 0; i < request.placements.Count; i++)
                    {
                        var item = request.placements[i].item;
                        if (item != null)
                            item.Consume();
                    }
                }

                if (resultItemPrefab != null && resultSpawn != null && recipe.result != null)
                {
                    var resultItem = Instantiate(resultItemPrefab, resultSpawn.position, resultSpawn.rotation);
                    resultItem.SetData(recipe.result, recipe.resultState);
                }

                grimoire?.DiscoverRecipe(recipe);
                grimoire?.DiscoverComponent(recipe.result);
                Feedback("Reação concluída: " + recipe.displayName + ".");
                PrototypeLog.Log("Símbolo: reação concluída: " + recipe.displayName + ".");
            }
            else
            {
                grimoire?.RecordFailure(result.recipe, result.message, result.failure);
                Feedback(result.message);
            }
        }

        void Feedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
            PrototypeLog.Log("Símbolo: " + message);
        }
    }
}
