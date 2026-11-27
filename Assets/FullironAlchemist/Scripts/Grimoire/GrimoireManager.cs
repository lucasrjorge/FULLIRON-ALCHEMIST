using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fulliron.Alchemy
{
    public class GrimoireManager : MonoBehaviour
    {
        public static GrimoireManager Instance { get; private set; }

        const string SaveKey = "fulliron.grimoire.v1";

        [SerializeField] AlchemyDatabase database;

        [Serializable]
        class SaveData
        {
            public List<string> components = new List<string>();
            public List<string> recipes = new List<string>();
            public string lastFailure;
        }

        SaveData state = new SaveData();

        public AlchemyDatabase Database => database;
        public string LastFailure => state.lastFailure;
        public event Action Changed;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Load();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool IsComponentDiscovered(AlchemicalComponent component)
        {
            if (component == null)
                return false;

            return state.components.Contains(ComponentKey(component));
        }

        public bool IsRecipeDiscovered(ReactionRecipe recipe)
        {
            if (recipe == null)
                return false;

            return state.recipes.Contains(RecipeKey(recipe));
        }

        public void DiscoverComponent(AlchemicalComponent component)
        {
            if (component == null)
                return;

            string key = ComponentKey(component);
            if (string.IsNullOrEmpty(key) || state.components.Contains(key))
                return;

            state.components.Add(key);
            Save();
            Changed?.Invoke();
            PrototypeLog.Log("Grimório: componente descoberto: " + component.DisplayName + ".");
        }

        public void DiscoverRecipe(ReactionRecipe recipe)
        {
            if (recipe == null)
                return;

            string key = RecipeKey(recipe);
            if (string.IsNullOrEmpty(key) || state.recipes.Contains(key))
                return;

            state.recipes.Add(key);
            DiscoverComponent(recipe.result);
            Save();
            Changed?.Invoke();
            PrototypeLog.Log("Grimório: reação descoberta: " + recipe.displayName + ".");
        }

        public void RecordFailure(ReactionRecipe recipe, string message, FailureProfile profile)
        {
            if (profile != null && !profile.recordInGrimoire)
                return;

            state.lastFailure = message;
            Save();
            Changed?.Invoke();
            PrototypeLog.Log("Grimório: falha registrada: " + message);
        }

        [ContextMenu("Reset Save")]
        public void ResetSave()
        {
            state = new SaveData();
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            Changed?.Invoke();
            PrototypeLog.Log("Grimório: save resetado.");
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return;

            string json = PlayerPrefs.GetString(SaveKey);
            var loaded = JsonUtility.FromJson<SaveData>(json);
            if (loaded == null)
                return;

            state = loaded;
            if (state.components == null)
                state.components = new List<string>();
            if (state.recipes == null)
                state.recipes = new List<string>();
        }

        void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        static string ComponentKey(AlchemicalComponent component)
        {
            if (component == null)
                return null;
            return string.IsNullOrWhiteSpace(component.Id) ? component.name : component.Id;
        }

        static string RecipeKey(ReactionRecipe recipe)
        {
            if (recipe == null)
                return null;
            return string.IsNullOrWhiteSpace(recipe.id) ? recipe.name : recipe.id;
        }
    }
}
