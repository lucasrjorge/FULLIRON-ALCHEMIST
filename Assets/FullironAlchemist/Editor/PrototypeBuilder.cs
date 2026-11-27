using System;
using System.Collections.Generic;
using System.IO;
using Fulliron.Alchemy;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Object = UnityEngine.Object;

namespace Fulliron.Alchemy.Editor
{
    public static class PrototypeBuilder
    {
        const string RootFolder = "Assets/FullironAlchemist";
        const string DataFolder = RootFolder + "/Data";
        const string ComponentsFolder = DataFolder + "/Components";
        const string MaterialsFolder = RootFolder + "/Materials";
        const string PrefabsFolder = RootFolder + "/Prefabs";
        const string ItemPrefabPath = PrefabsFolder + "/Item_Base.prefab";
        const string DatabasePath = DataFolder + "/AlchemyDatabase.asset";
        const string ScenePath = "Assets/Scenes/Laboratorium.unity";
        const string BasicScenePath = "Assets/Scenes/BasicScene.unity";
        const string GeneratedRootName = "FULLIRON_PROTOTYPE";

        static bool IsBatch => Application.isBatchMode;

        [MenuItem("Fulliron/Prototype/Construir Fase 0+1", priority = 0)]
        public static void BuildPhase01()
        {
            if (!CanBuild() || !EnsureSceneOpen())
                return;

            EnsureFolders();
            var materials = CreateMaterials();
            var data = CreateData();
            if (data == null || data.Sulfur == null || data.Mercury == null || data.Salt == null || data.Dew == null || data.Iron == null)
            {
                Debug.LogError("[Fulliron] Não foi possível criar os componentes demo.");
                if (!IsBatch)
                    EditorUtility.DisplayDialog("Fulliron", "Não foi possível criar os componentes demo. Veja o Console.", "OK");
                return;
            }

            var itemPrefab = CreateItemPrefab(materials);
            if (itemPrefab == null)
            {
                Debug.LogError("[Fulliron] Não foi possível criar o prefab Item_Base.");
                if (!IsBatch)
                    EditorUtility.DisplayDialog("Fulliron", "Não foi possível criar o prefab Item_Base. Veja o Console.", "OK");
                return;
            }

            BuildPrototypeScene(data, materials, itemPrefab);
            SaveSceneAndBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Fulliron] Fase 0+1 construída em " + ScenePath);
            if (!IsBatch)
                EditorUtility.DisplayDialog(
                    "Fulliron",
                    "Protótipo da Fase 0+1 construído.\n\nAbra a cena Laboratorium e entre em Play Mode.",
                    "OK");
        }

        [MenuItem("Fulliron/Prototype/Criar Dados e Materiais", priority = 20)]
        public static void CreateAssetsOnly()
        {
            if (!CanBuild())
                return;

            EnsureFolders();
            CreateMaterials();
            CreateData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Fulliron] Dados e materiais criados/atualizados.");
        }

        [MenuItem("Fulliron/Prototype/Criar Prefab do Item", priority = 21)]
        public static void CreateItemOnly()
        {
            if (!CanBuild())
                return;

            EnsureFolders();
            var materials = CreateMaterials();
            CreateItemPrefab(materials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Fulliron] Prefab do item criado/atualizado.");
        }

        [MenuItem("Fulliron/Prototype/Reconstruir Cena", priority = 22)]
        public static void RebuildScene()
        {
            BuildPhase01();
        }

        static bool CanBuild()
        {
            if (IsBatch)
                return true;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Fulliron", "Saia do Play Mode antes de construir o protótipo.", "OK");
                return false;
            }

            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Fulliron", "Aguarde a compilação terminar antes de construir.", "OK");
                return false;
            }

            return true;
        }

        static bool EnsureSceneOpen()
        {
            var active = EditorSceneManager.GetActiveScene();
            if (IsLabScene(active.path))
                return true;

            if (!IsBatch && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            var existing = FindExistingLabScene();
            if (!string.IsNullOrEmpty(existing))
            {
                EditorSceneManager.OpenScene(existing);
                return true;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BasicScenePath) == null)
            {
                Debug.LogError("[Fulliron] BasicScene não encontrada em " + BasicScenePath + ".");
                if (!IsBatch)
                    EditorUtility.DisplayDialog("Fulliron", "BasicScene não encontrada em " + BasicScenePath + ".", "OK");
                return false;
            }

            var basic = EditorSceneManager.OpenScene(BasicScenePath);
            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(basic, ScenePath);
            return true;
        }

        static bool IsLabScene(string path)
        {
            return !string.IsNullOrEmpty(path) &&
                   path.EndsWith("Laboratorium.unity", StringComparison.OrdinalIgnoreCase);
        }

        static string FindExistingLabScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                return ScenePath;

            const string alt = "Assets/FullironAlchemist/Laboratorium.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(alt) != null)
                return alt;

            return null;
        }

        static void SaveSceneAndBuildSettings()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogError("[Fulliron] A cena ativa não tem caminho salvo.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(scene.path, true)
            };

            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (existing == null || string.IsNullOrEmpty(existing.path))
                    continue;
                if (existing.path == scene.path || IsLabScene(existing.path))
                    continue;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(existing.path) == null)
                    continue;

                scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureFolders()
        {
            EnsureFolder(RootFolder);
            EnsureFolder(DataFolder);
            EnsureFolder(ComponentsFolder);
            EnsureFolder(DataFolder + "/Processing");
            EnsureFolder(DataFolder + "/Reactions");
            EnsureFolder(MaterialsFolder);
            EnsureFolder(PrefabsFolder);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
                return;

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static PrototypeMaterials CreateMaterials()
        {
            EnsureFolders();
            return new PrototypeMaterials
            {
                Item = GetOrCreateMaterial(MaterialsFolder + "/M_Item_Base.mat", Color.white, 0.35f),
                Table = GetOrCreateMaterial(MaterialsFolder + "/M_Table.mat", Hex("#5A3E2B"), 0.25f),
                Shelf = GetOrCreateMaterial(MaterialsFolder + "/M_Shelf.mat", Hex("#6B4A2B"), 0.25f),
                Tray = GetOrCreateMaterial(MaterialsFolder + "/M_Tray.mat", Hex("#3A3F45"), 0.4f),
                Button = GetOrCreateMaterial(MaterialsFolder + "/M_Button.mat", Hex("#B08D57"), 0.6f),
                Wall = GetOrCreateMaterial(MaterialsFolder + "/M_Wall.mat", Hex("#8C8C8C"), 0.2f),
                Symbol = GetOrCreateSymbolMaterial(MaterialsFolder + "/M_Symbol.mat", Hex("#F0C15A")),
                Pages = GetOrCreateMaterial(MaterialsFolder + "/M_Pages.mat", Hex("#E8D8B8"), 0.1f),
                Leather = GetOrCreateMaterial(MaterialsFolder + "/M_Leather.mat", Hex("#5A2E1E"), 0.25f)
            };
        }

        static Material GetOrCreateMaterial(string path, Color color, float smoothness)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");
                if (shader == null)
                {
                    Debug.LogError("[Fulliron] Shader URP/Lit não encontrado para " + path);
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material GetOrCreateSymbolMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                    shader = Shader.Find("Unlit/Color");
                if (shader == null)
                {
                    Debug.LogError("[Fulliron] Shader unlit não encontrado para " + path);
                    return null;
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Color Hex(string value)
        {
            return ColorUtility.TryParseHtmlString(value, out var color) ? color : Color.white;
        }

        static PrototypeData CreateData()
        {
            EnsureFolders();
            DeleteOldAssets();

            var terraSulfur = CreateResidue("TerraDeEnxofre", "terra_sulfur", "Terra de Enxofre", "Enxofre", Hex("#8A6A2B"));
            var terraSulfurWeak = CreateResidue("TerraDeEnxofreFraca", "terra_sulfur_weak", "Terra de Enxofre Fraca", "Enxofre", Hex("#B89B4A"));
            var terraSulfurStrong = CreateResidue("TerraDeEnxofreForte", "terra_sulfur_strong", "Terra de Enxofre Forte", "Enxofre", Hex("#5A3E12"));

            var terraMercury = CreateResidue("TerraDeMercurio", "terra_mercury", "Terra de Mercúrio", "Mercúrio", Hex("#6B6B7A"));
            var terraMercuryWeak = CreateResidue("TerraDeMercurioFraca", "terra_mercury_weak", "Terra de Mercúrio Fraca", "Mercúrio", Hex("#9A9AB0"));
            var terraMercuryStrong = CreateResidue("TerraDeMercurioForte", "terra_mercury_strong", "Terra de Mercúrio Forte", "Mercúrio", Hex("#3E3E4A"));

            var terraSalt = CreateResidue("TerraDeSal", "terra_salt", "Terra de Sal", "Sal", Hex("#D9D9D9"));
            var terraSaltWeak = CreateResidue("TerraDeSalFraca", "terra_salt_weak", "Terra de Sal Fraca", "Sal", Hex("#EDEDED"));
            var terraSaltStrong = CreateResidue("TerraDeSalForte", "terra_salt_strong", "Terra de Sal Forte", "Sal", Hex("#AFAFAF"));

            var terraDew = CreateResidue("TerraDeOrvalho", "terra_dew", "Terra de Orvalho", "Orvalho", Hex("#4A6B5A"));
            var terraDewWeak = CreateResidue("TerraDeOrvalhoFraca", "terra_dew_weak", "Terra de Orvalho Fraca", "Orvalho", Hex("#6F9A86"));
            var terraDewStrong = CreateResidue("TerraDeOrvalhoForte", "terra_dew_strong", "Terra de Orvalho Forte", "Orvalho", Hex("#2E4A3D"));

            var terraIron = CreateResidue("TerraDeFerro", "terra_iron", "Terra de Ferro", "Ferro", Hex("#6B4A2B"));
            var terraIronWeak = CreateResidue("TerraDeFerroFraca", "terra_iron_weak", "Terra de Ferro Fraca", "Ferro", Hex("#8C6A45"));
            var terraIronStrong = CreateResidue("TerraDeFerroForte", "terra_iron_strong", "Terra de Ferro Forte", "Ferro", Hex("#3E2A18"));

            var sulfur = CreatePrimary(
                "Enxofre", "sulfur", "Enxofre",
                "Representa a alma e a inflamabilidade.",
                EffectTag.Heat, Hex("#D8B43A"),
                terraSulfur, terraSulfurWeak, terraSulfurStrong);

            var mercury = CreatePrimary(
                "Mercurio", "mercury", "Mercúrio",
                "Representa o espírito e a volatilidade.",
                EffectTag.Volatile, Hex("#A9B0C0"),
                terraMercury, terraMercuryWeak, terraMercuryStrong);

            var salt = CreatePrimary(
                "Sal", "salt", "Sal",
                "Representa o corpo físico e a estabilidade.",
                EffectTag.Dry | EffectTag.Pure, Hex("#E8E8E8"),
                terraSalt, terraSaltWeak, terraSaltStrong);

            var dew = CreatePrimary(
                "Orvalho", "dew", "Orvalho",
                "Meio líquido puro para banhar e umedecer as misturas.",
                EffectTag.Moist | EffectTag.Pure, Hex("#7FB7D9"),
                terraDew, terraDewWeak, terraDewStrong);

            var iron = CreatePrimary(
                "Ferro", "iron", "Ferro",
                "Liga-se à força, à ação e à coragem.",
                EffectTag.Dry, Hex("#8A8F98"),
                terraIron, terraIronWeak, terraIronStrong);

            var sulfurIncomplete = CreateProcessed(
                "EnxofreIncompleto", "sulfur_incomplete", "Enxofre Incompleto",
                "Enxofre que passou por uma Dissolutio incompleta.",
                ComponentState.Incomplete, EffectTag.Moist | EffectTag.Dry, Hex("#C9B06A"));

            var sulfurDissolved = CreateProcessed(
                "EnxofreDissolvido", "sulfur_dissolved", "Enxofre Dissolvido",
                "Enxofre filtrado por Dissolutio.",
                ComponentState.Dissolved, EffectTag.Moist, Hex("#B89B4A"));

            var sulfurSuperdissolved = CreateProcessed(
                "EnxofreSuperdissolvido", "sulfur_superdissolved", "Enxofre Superdissolvido",
                "Enxofre que passou do ponto na Dissolutio.",
                ComponentState.Superdissolved, EffectTag.Moist, Hex("#8A6A2B"));

            var sulfurCalcined = CreateProcessed(
                "EnxofreCalcinado", "sulfur_calcined", "Enxofre Calcinado",
                "Enxofre que passou por Calcinatio.",
                ComponentState.Calcined, EffectTag.Heat | EffectTag.Dry, Hex("#D19A3A"));

            var sulfurSupercalcined = CreateProcessed(
                "EnxofreSupercalcinado", "sulfur_supercalcined", "Enxofre Supercalcinado",
                "Enxofre que passou do ponto na Calcinatio.",
                ComponentState.Supercalcined, EffectTag.Heat | EffectTag.Dry, Hex("#A86B1F"));

            var sulfurAsh = CreateProcessed(
                "EnxofreCinza", "sulfur_ash", "Enxofre Cinza",
                "Enxofre reduzido a cinza.",
                ComponentState.Ash, EffectTag.Dry, Hex("#5A5A5A"));

            var sulfurGasWeak = CreateProcessed(
                "EnxofreGasFraco", "sulfur_gas_weak", "Enxofre Gás Fraco",
                "Gás de Enxofre no estado fraco.",
                ComponentState.GasWeak, EffectTag.Volatile, Hex("#E8E0A0"));

            var sulfurGasStable = CreateProcessed(
                "EnxofreGasEstavel", "sulfur_gas_stable", "Enxofre Gás Estável",
                "Gás de Enxofre no estado estável.",
                ComponentState.GasStable, EffectTag.Volatile, Hex("#D8B43A"));

            var sulfurGasUnstable = CreateProcessed(
                "EnxofreGasInstavel", "sulfur_gas_unstable", "Enxofre Gás Instável",
                "Gás de Enxofre no estado instável.",
                ComponentState.GasUnstable, EffectTag.Volatile, Hex("#B07C2A"));

            var sulfurCrystalWeak = CreateProcessed(
                "CristalDeEnxofreFraco", "sulfur_crystal_weak", "Cristal de Enxofre Fraco",
                "Cristal formado a partir do gás fraco de Enxofre.",
                ComponentState.Crystallized, EffectTag.Pure, Hex("#E8D98A"));

            var sulfurCrystalStable = CreateProcessed(
                "CristalDeEnxofreEstavel", "sulfur_crystal_stable", "Cristal de Enxofre Estável",
                "Cristal formado a partir do gás estável de Enxofre.",
                ComponentState.Crystallized, EffectTag.Pure, Hex("#F0C15A"));

            var sulfurCrystalUnstable = CreateProcessed(
                "CristalDeEnxofreInstavel", "sulfur_crystal_unstable", "Cristal de Enxofre Instável",
                "Cristal formado a partir do gás instável de Enxofre.",
                ComponentState.Crystallized, EffectTag.Pure, Hex("#C49A2A"));

            var gold = GetOrCreateComponent(
                "Ouro", "gold", "Ouro",
                "Simboliza a perfeição e a pureza.",
                true, ComponentState.Crystallized, EffectTag.Pure, Hex("#F0C15A"),
                null, null, null);

            var all = new List<AlchemicalComponent>
            {
                terraSulfur, terraSulfurWeak, terraSulfurStrong,
                terraMercury, terraMercuryWeak, terraMercuryStrong,
                terraSalt, terraSaltWeak, terraSaltStrong,
                terraDew, terraDewWeak, terraDewStrong,
                terraIron, terraIronWeak, terraIronStrong,
                sulfur, mercury, salt, dew, iron, gold,
                sulfurIncomplete, sulfurDissolved, sulfurSuperdissolved,
                sulfurCalcined, sulfurSupercalcined, sulfurAsh,
                sulfurGasWeak, sulfurGasStable, sulfurGasUnstable,
                sulfurCrystalWeak, sulfurCrystalStable, sulfurCrystalUnstable
            };

            var database = AssetDatabase.LoadAssetAtPath<AlchemyDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<AlchemyDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            if (database.components == null)
                database.components = new List<AlchemicalComponent>();
            if (database.processingProfiles == null)
                database.processingProfiles = new List<ProcessingProfile>();
            if (database.reactionRecipes == null)
                database.reactionRecipes = new List<ReactionRecipe>();

            var calcinatioProfile = CreateCalcinatioProfile(sulfur, sulfurCalcined, sulfurSupercalcined, sulfurAsh);
            var ouroReaction = CreateOuroReaction(sulfurCalcined, salt, dew, gold);

            database.components.Clear();
            database.components.AddRange(all);

            database.processingProfiles.Clear();
            database.processingProfiles.Add(calcinatioProfile);

            database.reactionRecipes.Clear();
            database.reactionRecipes.Add(ouroReaction);

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            return new PrototypeData
            {
                Sulfur = sulfur,
                Mercury = mercury,
                Salt = salt,
                Dew = dew,
                Iron = iron,
                Database = database
            };
        }

        static AlchemicalComponent CreateResidue(
            string fileName,
            string id,
            string displayName,
            string sourceName,
            Color tint)
        {
            return GetOrCreateComponent(
                fileName, id, displayName,
                "Resíduo de Terra deixado por " + sourceName + ".",
                false, ComponentState.Residue, EffectTag.Dry, tint,
                null, null, null);
        }

        static AlchemicalComponent CreatePrimary(
            string fileName,
            string id,
            string displayName,
            string description,
            EffectTag tags,
            Color tint,
            AlchemicalComponent dissolutionResidue,
            AlchemicalComponent calcinationResidueWeak,
            AlchemicalComponent calcinationResidueStrong)
        {
            return GetOrCreateComponent(
                fileName, id, displayName, description,
                true, ComponentState.Raw, tags, tint,
                dissolutionResidue, calcinationResidueWeak, calcinationResidueStrong);
        }

        static AlchemicalComponent CreateProcessed(
            string fileName,
            string id,
            string displayName,
            string description,
            ComponentState state,
            EffectTag tags,
            Color tint)
        {
            return GetOrCreateComponent(
                fileName, id, displayName, description,
                false, state, tags, tint,
                null, null, null);
        }

        static void DeleteOldAssets()
        {
            string[] names =
            {
                "Ferrum", "Sulfur", "Aqua",
                "TerraFerrosa", "TerraSulfurosa", "TerraAquosa", "TerraSalina",
                "FerrumCalcinadoLeve", "SalDissolvido", "FerrumSublimado", "CristalDeFerro"
            };

            for (int i = 0; i < names.Length; i++)
            {
                string path = ComponentsFolder + "/" + names[i] + ".asset";
                if (AssetDatabase.LoadAssetAtPath<AlchemicalComponent>(path) != null)
                    AssetDatabase.DeleteAsset(path);
            }
        }

        static ProcessingProfile CreateCalcinatioProfile(
            AlchemicalComponent input,
            AlchemicalComponent calcined,
            AlchemicalComponent supercalcined,
            AlchemicalComponent ash)
        {
            string path = DataFolder + "/Processing/Profile_Calcinatio_" + input.Id + ".asset";
            var profile = AssetDatabase.LoadAssetAtPath<ProcessingProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<ProcessingProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.input = input;
            profile.anyInputState = true;
            profile.inputState = ComponentState.Raw;
            profile.process = ProcessType.Calcinatio;
            profile.outcomes = new List<ProcessingOutcome>
            {
                new ProcessingOutcome
                {
                    minProgress = 0f,
                    maxProgress = 0.15f,
                    output = input,
                    outputState = ComponentState.Raw,
                    residueAmount = 0,
                    message = "Ainda cru."
                },
                new ProcessingOutcome
                {
                    minProgress = 0.15f,
                    maxProgress = 0.45f,
                    output = calcined,
                    outputState = ComponentState.Calcined,
                    residueAmount = 1,
                    message = "Calcinado."
                },
                new ProcessingOutcome
                {
                    minProgress = 0.45f,
                    maxProgress = 0.75f,
                    output = supercalcined,
                    outputState = ComponentState.Supercalcined,
                    residueAmount = 1,
                    message = "Supercalcinado."
                },
                new ProcessingOutcome
                {
                    minProgress = 0.75f,
                    maxProgress = 1f,
                    output = ash,
                    outputState = ComponentState.Ash,
                    residueAmount = 1,
                    message = "Cinza."
                }
            };
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static ReactionRecipe CreateOuroReaction(
            AlchemicalComponent calcinado,
            AlchemicalComponent sal,
            AlchemicalComponent orvalho,
            AlchemicalComponent ouro)
        {
            const string path = DataFolder + "/Reactions/Reaction_Ouro.asset";
            var recipe = AssetDatabase.LoadAssetAtPath<ReactionRecipe>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<ReactionRecipe>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            recipe.id = "ouro";
            recipe.displayName = "Transmutação de Ouro";
            recipe.symbolId = "ouro";
            recipe.resultDescription = "O Enxofre Calcinado, o Sal e o Orvalho se unem para formar Ouro.";
            recipe.requirements = new List<ReactionRequirement>
            {
                new ReactionRequirement
                {
                    anchorElement = AlchemicalElement.Ignis,
                    exactComponent = calcinado,
                    requireState = true,
                    requiredState = ComponentState.Calcined
                },
                new ReactionRequirement
                {
                    anchorElement = AlchemicalElement.Terra,
                    exactComponent = sal,
                    requireState = true,
                    requiredState = ComponentState.Raw
                },
                new ReactionRequirement
                {
                    anchorElement = AlchemicalElement.Aqua,
                    exactComponent = orvalho,
                    requireState = true,
                    requiredState = ComponentState.Raw
                }
            };
            recipe.result = ouro;
            recipe.resultState = ComponentState.Crystallized;
            recipe.consumeComponents = true;
            recipe.failureProfile = new FailureProfile
            {
                failureMessage = "Os componentes não se combinaram."
            };
            recipe.priority = 0;
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        static AlchemicalComponent GetOrCreateComponent(
            string fileName,
            string id,
            string displayName,
            string description,
            bool primary,
            ComponentState defaultState,
            EffectTag tags,
            Color tint,
            AlchemicalComponent dissolutionResidue,
            AlchemicalComponent calcinationResidueWeak,
            AlchemicalComponent calcinationResidueStrong)
        {
            string path = ComponentsFolder + "/" + fileName + ".asset";
            var component = AssetDatabase.LoadAssetAtPath<AlchemicalComponent>(path);
            if (component == null)
            {
                component = ScriptableObject.CreateInstance<AlchemicalComponent>();
                AssetDatabase.CreateAsset(component, path);
            }

            var serialized = new SerializedObject(component);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("primary").boolValue = primary;
            serialized.FindProperty("defaultState").enumValueIndex = (int)defaultState;
            serialized.FindProperty("tags").intValue = (int)tags;
            serialized.FindProperty("color").colorValue = tint;
            serialized.FindProperty("emissionColor").colorValue = Color.black;
            serialized.FindProperty("emissionIntensity").floatValue = 0f;
            serialized.FindProperty("dissolutionResidue").objectReferenceValue = dissolutionResidue;
            serialized.FindProperty("calcinationResidueWeak").objectReferenceValue = calcinationResidueWeak;
            serialized.FindProperty("calcinationResidueStrong").objectReferenceValue = calcinationResidueStrong;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(component);
            return component;
        }

        static AlchemicalItem CreateItemPrefab(PrototypeMaterials materials)
        {
            EnsureFolders();

            var root = new GameObject("Item_Base");
            var item = root.AddComponent<AlchemicalItem>();
            var visuals = root.AddComponent<ComponentVisuals>();

            var body = root.AddComponent<Rigidbody>();
            body.mass = 0.2f;
            body.linearDamping = 0.5f;
            body.angularDamping = 5f;
            body.useGravity = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var collider = root.AddComponent<SphereCollider>();
            collider.radius = 0.05f;

            var grab = root.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.throwOnDetach = true;
            grab.throwSmoothingDuration = 0.25f;
            grab.useDynamicAttach = true;
            grab.matchAttachPosition = true;
            grab.matchAttachRotation = true;
            grab.snapToColliderVolume = true;

            var itemSerialized = new SerializedObject(item);
            itemSerialized.FindProperty("initialComponent").objectReferenceValue = null;
            itemSerialized.FindProperty("initialState").enumValueIndex = (int)ComponentState.Raw;
            itemSerialized.FindProperty("applyInitialOnAwake").boolValue = false;
            itemSerialized.ApplyModifiedPropertiesWithoutUndo();

            var visual = CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Potion_Vial.fbx",
                root.transform,
                "Visual",
                root.transform.position + new Vector3(0f, -0.05f, 0f),
                Quaternion.identity,
                new Vector3(0.1f, 0.1f, 0.1f),
                materials.Item);

            if (visual == null)
            {
                visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                visual.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            }

            var visualRenderers = visual.GetComponentsInChildren<MeshRenderer>(true);

            var label = CreateWorldText(
                root.transform,
                "Label",
                string.Empty,
                new Vector3(0f, 0.12f, 0f),
                Quaternion.identity,
                0.02f,
                36f,
                new Vector2(20f, 10f),
                new Color(1f, 0.95f, 0.8f));

            var itemLabel = label.gameObject.AddComponent<ItemLabel>();

            var visualSerialized = new SerializedObject(visuals);
            visualSerialized.FindProperty("item").objectReferenceValue = item;
            visualSerialized.FindProperty("visualRoot").objectReferenceValue = visual.transform;
            var renderers = visualSerialized.FindProperty("renderers");
            renderers.arraySize = visualRenderers.Length;
            for (int i = 0; i < visualRenderers.Length; i++)
                renderers.GetArrayElementAtIndex(i).objectReferenceValue = visualRenderers[i];
            visualSerialized.FindProperty("gasVisual").objectReferenceValue = null;
            visualSerialized.FindProperty("tintRenderers").boolValue = true;
            visualSerialized.FindProperty("toggleGasVisual").boolValue = true;
            visualSerialized.FindProperty("gasScale").floatValue = 1.25f;
            visualSerialized.FindProperty("solidScale").floatValue = 1f;
            visualSerialized.ApplyModifiedPropertiesWithoutUndo();

            var labelSerialized = new SerializedObject(itemLabel);
            labelSerialized.FindProperty("item").objectReferenceValue = item;
            labelSerialized.FindProperty("label").objectReferenceValue = label;
            labelSerialized.FindProperty("showTags").boolValue = false;
            labelSerialized.FindProperty("faceCamera").boolValue = true;
            labelSerialized.FindProperty("flipFacing").boolValue = false;
            labelSerialized.FindProperty("hideWhenEmpty").boolValue = false;
            labelSerialized.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ItemPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();

            if (prefab == null)
            {
                Debug.LogError("[Fulliron] Falha ao salvar " + ItemPrefabPath);
                return null;
            }

            return prefab.GetComponent<AlchemicalItem>();
        }

        static void BuildPrototypeScene(PrototypeData data, PrototypeMaterials materials, AlchemicalItem itemPrefab)
        {
            var oldRoot = GameObject.Find(GeneratedRootName);
            if (oldRoot != null)
                Object.DestroyImmediate(oldRoot);

            var root = new GameObject(GeneratedRootName);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.40f, 0.38f, 0.35f);
            RenderSettings.ambientIntensity = 1f;

            var sun = GameObject.Find("Directional Light");
            if (sun != null && sun.TryGetComponent<Light>(out var sunLight))
            {
                sunLight.intensity = 1.3f;
                sunLight.color = new Color(1f, 0.96f, 0.9f);
            }

            if (Object.FindAnyObjectByType<XRInteractionManager>() == null)
            {
                var manager = new GameObject("XR Interaction Manager");
                manager.transform.SetParent(root.transform, false);
                manager.AddComponent<XRInteractionManager>();
            }

            var plane = GameObject.Find("Plane");
            if (plane == null)
            {
                plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = "Plane";
            }
            plane.transform.position = Vector3.zero;
            plane.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

            if (plane.GetComponent<TeleportationArea>() == null)
                plane.AddComponent<TeleportationArea>();

            var room = new GameObject("Room").transform;
            room.SetParent(root.transform, false);

            CreateCube("Wall_North", room, new Vector3(0f, 1.5f, 4f), new Vector3(8f, 3f, 0.2f), materials.Wall, true);
            CreateCube("Wall_South", room, new Vector3(0f, 1.5f, -4f), new Vector3(8f, 3f, 0.2f), materials.Wall, true);
            CreateCube("Wall_East", room, new Vector3(4f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f), materials.Wall, true);
            CreateCube("Wall_West", room, new Vector3(-4f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f), materials.Wall, true);
            var ceiling = CreateCube("Ceiling", room, new Vector3(0f, 3f, 0f), new Vector3(8f, 0.1f, 8f), materials.Wall, false);
            var ceilingRenderer = ceiling.GetComponent<MeshRenderer>();
            if (ceilingRenderer != null)
                ceilingRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            CreatePointLight(room, "Light_Center", new Vector3(0f, 2.7f, 0f), 9f, 3.5f);
            CreatePointLight(room, "Light_Symbol", new Vector3(0f, 2.4f, 2.5f), 5f, 2.5f);
            CreatePointLight(room, "Light_West", new Vector3(-2.8f, 2.6f, 1.0f), 7f, 2.5f);
            CreatePointLight(room, "Light_East", new Vector3(2.8f, 2.6f, 1.0f), 7f, 2.5f);
            CreatePointLight(room, "Light_North", new Vector3(0f, 2.6f, -2.5f), 6f, 2.2f);

            CreateWallElementSymbol(room, new Vector3(3.88f, 2.2f, 0f), Quaternion.LookRotation(Vector3.left), AlchemicalElement.Ignis, materials);

            var grimoireObject = new GameObject("GrimoireManager");
            grimoireObject.transform.SetParent(root.transform, false);
            var grimoire = grimoireObject.AddComponent<GrimoireManager>();
            var grimoireSerialized = new SerializedObject(grimoire);
            grimoireSerialized.FindProperty("database").objectReferenceValue = data.Database;
            grimoireSerialized.ApplyModifiedPropertiesWithoutUndo();

            var stations = new GameObject("Stations").transform;
            stations.SetParent(root.transform, false);

            var symbolTable = new GameObject("SymbolTable");
            symbolTable.transform.SetParent(stations, false);
            symbolTable.transform.position = new Vector3(0f, 0f, 2.5f);

            CreateModelProp(
                "Assets/Placeholder-Assets/AlchemyTableCabinet.fbx",
                symbolTable.transform,
                "AlchemyTableCabinet",
                symbolTable.transform.position,
                Quaternion.identity,
                new Vector3(1.6f, 0.9f, 0.9f),
                materials.Table);
            CreateInvisibleBox("TableSurface", symbolTable.transform, new Vector3(0f, 0.88f, 0f), new Vector3(1.6f, 0.04f, 0.9f));

            var tray = CreateTray(symbolTable, materials, 0.92f, 0.3f);
            var elementSockets = new ItemSocket[4];
            elementSockets[0] = CreateElementSocket(symbolTable.transform, "ElementSocket_Aqua", AlchemicalElement.Aqua, new Vector3(-0.45f, 0.93f, -0.1f));
            elementSockets[1] = CreateElementSocket(symbolTable.transform, "ElementSocket_Ignis", AlchemicalElement.Ignis, new Vector3(0.45f, 0.93f, -0.1f));
            elementSockets[2] = CreateElementSocket(symbolTable.transform, "ElementSocket_Terra", AlchemicalElement.Terra, new Vector3(-0.45f, 0.93f, 0.15f));
            elementSockets[3] = CreateElementSocket(symbolTable.transform, "ElementSocket_Aer", AlchemicalElement.Aer, new Vector3(0.45f, 0.93f, 0.15f));

            CreateFlatElementSymbol(symbolTable.transform, new Vector3(-0.45f, 0.91f, -0.22f), AlchemicalElement.Aqua, materials.Symbol);
            CreateFlatElementSymbol(symbolTable.transform, new Vector3(0.45f, 0.91f, -0.22f), AlchemicalElement.Ignis, materials.Symbol);
            CreateFlatElementSymbol(symbolTable.transform, new Vector3(-0.45f, 0.91f, 0.27f), AlchemicalElement.Terra, materials.Symbol);
            CreateFlatElementSymbol(symbolTable.transform, new Vector3(0.45f, 0.91f, 0.27f), AlchemicalElement.Aer, materials.Symbol);

            var castButton = CreateButton(symbolTable.transform, "CastButton", new Vector3(0f, 0.93f, -0.35f), materials, "CONJURAR");

            var resultSpawn = new GameObject("ResultSpawn").transform;
            resultSpawn.SetParent(symbolTable.transform, false);
            resultSpawn.localPosition = new Vector3(0f, 0.93f, -0.15f);

            var feedback = CreateWorldText(
                symbolTable.transform,
                "Feedback",
                "Ouro: aguardando componentes.\nUse os 3 símbolos + CONJURAR.",
                new Vector3(0f, 1.3f, 0f),
                Quaternion.identity,
                0.03f,
                30f,
                new Vector2(30f, 8f),
                new Color(1f, 0.95f, 0.8f));
            feedback.gameObject.AddComponent<WorldBillboard>();

            var symbolTitle = CreateWorldText(
                symbolTable.transform,
                "SymbolTitle",
                "MESA DO SÍMBOLO",
                new Vector3(0f, 1.62f, 0f),
                Quaternion.identity,
                0.025f,
                26f,
                new Vector2(30f, 4f),
                new Color(1f, 0.9f, 0.55f));
            symbolTitle.gameObject.AddComponent<WorldBillboard>();

            CreateSocketLabel(symbolTable.transform, "AQUA", new Vector3(-0.45f, 1.04f, -0.1f));
            CreateSocketLabel(symbolTable.transform, "IGNIS", new Vector3(0.45f, 1.04f, -0.1f));
            CreateSocketLabel(symbolTable.transform, "TERRA", new Vector3(-0.45f, 1.04f, 0.15f));
            CreateSocketLabel(symbolTable.transform, "AER", new Vector3(0.45f, 1.04f, 0.15f));

            var symbolTableComponent = symbolTable.AddComponent<SymbolTable>();
            var symbolSerialized = new SerializedObject(symbolTableComponent);
            symbolSerialized.FindProperty("database").objectReferenceValue = data.Database;
            symbolSerialized.FindProperty("castButton").objectReferenceValue = castButton;
            symbolSerialized.FindProperty("grimoire").objectReferenceValue = grimoire;
            symbolSerialized.FindProperty("resultItemPrefab").objectReferenceValue = itemPrefab;
            symbolSerialized.FindProperty("resultSpawn").objectReferenceValue = resultSpawn;
            symbolSerialized.FindProperty("feedbackText").objectReferenceValue = feedback;
            symbolSerialized.FindProperty("symbolId").stringValue = "ouro";
            var socketArray = symbolSerialized.FindProperty("sockets");
            socketArray.arraySize = elementSockets.Length;
            for (int i = 0; i < elementSockets.Length; i++)
                socketArray.GetArrayElementAtIndex(i).objectReferenceValue = elementSockets[i];
            symbolSerialized.ApplyModifiedPropertiesWithoutUndo();

            SpawnTestItem(symbolTable.transform, itemPrefab, data.Sulfur, new Vector3(-0.55f, 0.96f, 0.35f), "TestItem_Enxofre");
            SpawnTestItem(symbolTable.transform, itemPrefab, data.Salt, new Vector3(-0.55f, 0.96f, 0.05f), "TestItem_Sal");
            SpawnTestItem(symbolTable.transform, itemPrefab, data.Dew, new Vector3(-0.55f, 0.96f, -0.25f), "TestItem_Orvalho");

            var calcination = new GameObject("CalcinationStation");
            calcination.transform.SetParent(stations, false);
            calcination.transform.position = new Vector3(2.8f, 0f, 0f);

            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Table.fbx",
                calcination.transform,
                "CalcinationBench",
                calcination.transform.position,
                Quaternion.identity,
                new Vector3(1.2f, 0.9f, 0.8f),
                materials.Table);
            CreateInvisibleBox("CalcinationSurface", calcination.transform, new Vector3(0f, 0.88f, 0f), new Vector3(1.2f, 0.04f, 0.8f));

            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Fireplace.fbx",
                calcination.transform,
                "Fireplace",
                calcination.transform.position + new Vector3(0.85f, 0f, 0f),
                Quaternion.Euler(0f, -90f, 0f),
                new Vector3(0.8f, 1.6f, 0.5f),
                materials.Wall);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Firewood_Stack.fbx",
                calcination.transform,
                "Firewood",
                calcination.transform.position + new Vector3(0.55f, 0f, -0.55f),
                Quaternion.identity,
                new Vector3(0.5f, 0.4f, 0.3f),
                materials.Table);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Cauldron.fbx",
                calcination.transform,
                "Cauldron",
                calcination.transform.position + new Vector3(0f, 0.9f, 0f),
                Quaternion.identity,
                new Vector3(0.35f, 0.25f, 0.35f),
                materials.Tray);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Mortar.fbx",
                calcination.transform,
                "Mortar",
                calcination.transform.position + new Vector3(-0.45f, 0.9f, 0.25f),
                Quaternion.identity,
                new Vector3(0.2f, 0.12f, 0.2f),
                materials.Tray);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Scales.fbx",
                calcination.transform,
                "Scales",
                calcination.transform.position + new Vector3(-0.5f, 0.9f, -0.25f),
                Quaternion.identity,
                new Vector3(0.25f, 0.25f, 0.2f),
                materials.Button);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Bucket.fbx",
                calcination.transform,
                "BellowsProxy",
                calcination.transform.position + new Vector3(-0.4f, 0.9f, 0.1f),
                Quaternion.identity,
                new Vector3(0.25f, 0.2f, 0.25f),
                materials.Leather);

            var calcinationSocket = CreateElementSocket(calcination.transform, "CalcinationSocket", AlchemicalElement.None, new Vector3(0f, 1.16f, 0f));
            var pumpButton = CreateButton(calcination.transform, "PumpButton", new Vector3(-0.4f, 1.08f, 0.1f), materials, "BOMBEAR");
            var sealButton = CreateButton(calcination.transform, "SealButton", new Vector3(0.35f, 0.95f, 0.15f), materials, "SELAR");
            var heatGauge = CreateGauge(calcination.transform, new Vector3(0.45f, 0.9f, 0.25f), materials);
            var statusText = CreateWorldText(
                calcination.transform,
                "Status",
                "Coloque um componente no cadinho.",
                new Vector3(0f, 1.35f, 0.1f),
                Quaternion.identity,
                0.03f,
                30f,
                new Vector2(30f, 8f),
                new Color(1f, 0.95f, 0.8f));
            statusText.gameObject.AddComponent<WorldBillboard>();

            var calcinationTitle = CreateWorldText(
                calcination.transform,
                "CalcinationTitle",
                "CALCINATIO",
                new Vector3(0f, 1.7f, 0.1f),
                Quaternion.identity,
                0.025f,
                26f,
                new Vector2(30f, 4f),
                new Color(1f, 0.9f, 0.55f));
            calcinationTitle.gameObject.AddComponent<WorldBillboard>();

            var cauldronLabel = CreateWorldText(
                calcination.transform,
                "CauldronLabel",
                "CADINHO",
                new Vector3(0f, 1.26f, 0f),
                Quaternion.identity,
                0.012f,
                16f,
                new Vector2(12f, 2f),
                new Color(1f, 0.95f, 0.75f));
            cauldronLabel.gameObject.AddComponent<WorldBillboard>();

            var fireLightObject = new GameObject("FireLight");
            fireLightObject.transform.SetParent(calcination.transform, false);
            fireLightObject.transform.localPosition = new Vector3(0.45f, 0.8f, 0f);
            var fireLight = fireLightObject.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.range = 3f;
            fireLight.intensity = 0.5f;
            fireLight.shadows = LightShadows.None;
            fireLight.color = new Color(1f, 0.45f, 0.12f);

            var residueSpawn = CreateTerraExtension(calcination.transform, new Vector3(-0.8f, 0f, 0.5f), materials);

            var calcinationStation = calcination.AddComponent<CalcinationStation>();
            var calcinationSerialized = new SerializedObject(calcinationStation);
            calcinationSerialized.FindProperty("database").objectReferenceValue = data.Database;
            calcinationSerialized.FindProperty("itemSocket").objectReferenceValue = calcinationSocket;
            calcinationSerialized.FindProperty("pumpButton").objectReferenceValue = pumpButton;
            calcinationSerialized.FindProperty("sealButton").objectReferenceValue = sealButton;
            calcinationSerialized.FindProperty("heatGauge").objectReferenceValue = heatGauge;
            calcinationSerialized.FindProperty("statusText").objectReferenceValue = statusText;
            calcinationSerialized.FindProperty("fireLight").objectReferenceValue = fireLight;
            calcinationSerialized.FindProperty("residueSpawn").objectReferenceValue = residueSpawn;
            calcinationSerialized.FindProperty("residueItemPrefab").objectReferenceValue = itemPrefab;
            calcinationSerialized.FindProperty("grimoire").objectReferenceValue = grimoire;
            calcinationSerialized.ApplyModifiedPropertiesWithoutUndo();

            SpawnTestItem(calcination.transform, itemPrefab, data.Sulfur, new Vector3(0.55f, 0.96f, -0.25f), "TestItem_Enxofre_Calcinatio");

            var grimoireStation = new GameObject("GrimoireLectern");
            grimoireStation.transform.SetParent(stations, false);
            grimoireStation.transform.position = new Vector3(1.4f, 0f, 2.5f);

            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Desk.fbx",
                grimoireStation.transform,
                "GrimoireDesk",
                grimoireStation.transform.position,
                Quaternion.identity,
                new Vector3(0.7f, 0.9f, 0.5f),
                materials.Table);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Open_Book.fbx",
                grimoireStation.transform,
                "OpenBook",
                grimoireStation.transform.position + new Vector3(0f, 0.92f, 0f),
                Quaternion.identity,
                new Vector3(0.35f, 0.08f, 0.25f),
                materials.Pages);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Bookcase_Full.fbx",
                grimoireStation.transform,
                "Bookcase",
                grimoireStation.transform.position + new Vector3(0f, 0f, 1.1f),
                Quaternion.identity,
                new Vector3(0.8f, 1.8f, 0.4f),
                materials.Shelf);

            var boardTitle = CreateWorldText(
                grimoireStation.transform,
                "BoardTitle",
                "Grimório",
                new Vector3(0f, 1.9f, 0.2f),
                Quaternion.identity,
                0.04f,
                48f,
                new Vector2(40f, 8f),
                new Color(1f, 0.9f, 0.7f));
            boardTitle.gameObject.AddComponent<WorldBillboard>();
            var boardReactions = CreateWorldText(
                grimoireStation.transform,
                "BoardReactions",
                "Reações:",
                new Vector3(0.8f, 1.55f, 0.2f),
                Quaternion.identity,
                0.025f,
                24f,
                new Vector2(50f, 20f),
                new Color(1f, 0.95f, 0.8f));
            boardReactions.gameObject.AddComponent<WorldBillboard>();
            var boardComponents = CreateWorldText(
                grimoireStation.transform,
                "BoardComponents",
                "Componentes:",
                new Vector3(0.8f, 1.1f, 0.2f),
                Quaternion.identity,
                0.025f,
                24f,
                new Vector2(50f, 20f),
                new Color(1f, 0.95f, 0.8f));
            boardComponents.gameObject.AddComponent<WorldBillboard>();

            CreateCube(
                "GrimoirePanel",
                grimoireStation.transform,
                new Vector3(0.8f, 1.4f, 0.18f),
                new Vector3(0.62f, 0.86f, 0.02f),
                materials.Wall,
                false);

            var board = grimoireStation.AddComponent<GrimoireBoard>();
            var boardSerialized = new SerializedObject(board);
            boardSerialized.FindProperty("manager").objectReferenceValue = grimoire;
            boardSerialized.FindProperty("titleText").objectReferenceValue = boardTitle;
            boardSerialized.FindProperty("reactionsText").objectReferenceValue = boardReactions;
            boardSerialized.FindProperty("componentsText").objectReferenceValue = boardComponents;
            boardSerialized.ApplyModifiedPropertiesWithoutUndo();

            var shelves = new GameObject("Shelves").transform;
            shelves.SetParent(root.transform, false);

            var shelfWest = CreateCube("Shelf_West", shelves, new Vector3(-3.8f, 1.2f, 1.5f), new Vector3(0.3f, 0.05f, 2f), materials.Shelf, true);
            var shelfEast = CreateCube("Shelf_East", shelves, new Vector3(3.8f, 1.2f, 1.5f), new Vector3(0.3f, 0.05f, 2f), materials.Shelf, true);
            var lipWest = CreateCube("Lip_West", shelves, new Vector3(-3.65f, 1.25f, 1.5f), new Vector3(0.03f, 0.04f, 2f), materials.Shelf, true);
            var lipEast = CreateCube("Lip_East", shelves, new Vector3(3.65f, 1.25f, 1.5f), new Vector3(0.03f, 0.04f, 2f), materials.Shelf, true);
            HideRenderers(shelfWest);
            HideRenderers(shelfEast);
            HideRenderers(lipWest);
            HideRenderers(lipEast);

            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Wall_Shelf.fbx",
                shelves,
                "ShelfWestVisual",
                new Vector3(-3.8f, 0.875f, 1.5f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(2f, 0.35f, 0.3f),
                materials.Shelf);
            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Wall_Shelf.fbx",
                shelves,
                "ShelfEastVisual",
                new Vector3(3.8f, 0.875f, 1.5f),
                Quaternion.Euler(0f, -90f, 0f),
                new Vector3(2f, 0.35f, 0.3f),
                materials.Shelf);

            CreateShelfRow(shelves, "ShelfRow_Enxofre", new Vector3(-3.8f, 1.08f, 0.8f), 1f, data.Sulfur, itemPrefab, materials);
            CreateShelfRow(shelves, "ShelfRow_Mercurio", new Vector3(-3.8f, 1.08f, 1.45f), 1f, data.Mercury, itemPrefab, materials);
            CreateShelfRow(shelves, "ShelfRow_Sal", new Vector3(-3.8f, 1.08f, 2.1f), 1f, data.Salt, itemPrefab, materials);
            CreateShelfRow(shelves, "ShelfRow_Orvalho", new Vector3(3.8f, 1.08f, 1.05f), -1f, data.Dew, itemPrefab, materials);
            CreateShelfRow(shelves, "ShelfRow_Ferro", new Vector3(3.8f, 1.08f, 1.9f), -1f, data.Iron, itemPrefab, materials);

            var log = Object.FindAnyObjectByType<PrototypeLog>();
            if (log == null)
            {
                var logObject = new GameObject("PrototypeLog");
                logObject.transform.SetParent(root.transform, false);
                log = logObject.AddComponent<PrototypeLog>();
            }

            var logDisplay = CreateWorldText(
                root.transform,
                "LogDisplay",
                "FULLIRON — LOG",
                new Vector3(0f, 1.8f, 3.85f),
                Quaternion.identity,
                0.02f,
                24f,
                new Vector2(60f, 30f),
                new Color(1f, 0.95f, 0.8f));
            logDisplay.gameObject.AddComponent<WorldBillboard>();

            var logSerialized = new SerializedObject(log);
            logSerialized.FindProperty("display").objectReferenceValue = logDisplay;
            logSerialized.FindProperty("maxLines").intValue = 8;
            logSerialized.ApplyModifiedPropertiesWithoutUndo();

            var title = CreateWorldText(
                root.transform,
                "Title",
                "FULLIRON ALCHEMIST",
                new Vector3(0f, 2.5f, 3.85f),
                Quaternion.identity,
                0.04f,
                48f,
                new Vector2(60f, 8f),
                new Color(1f, 0.9f, 0.7f));
            title.gameObject.AddComponent<WorldBillboard>();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        static void SetItemInitialData(AlchemicalItem item, AlchemicalComponent component, ComponentState state)
        {
            if (item == null)
                return;

            var serialized = new SerializedObject(item);
            serialized.FindProperty("initialComponent").objectReferenceValue = component;
            serialized.FindProperty("initialState").enumValueIndex = (int)state;
            serialized.FindProperty("applyInitialOnAwake").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SpawnTestItem(
            Transform parent,
            AlchemicalItem prefab,
            AlchemicalComponent component,
            Vector3 localPosition,
            string name)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.identity;
            SetItemInitialData(instance.GetComponent<AlchemicalItem>(), component, component.DefaultState);
        }

        static GameObject CreateCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;

            if (!keepCollider)
            {
                var collider = cube.GetComponent<Collider>();
                if (collider != null)
                    Object.DestroyImmediate(collider);
            }

            return cube;
        }

        static GameObject CreateTable(Transform parent, string name, Vector3 worldPosition, Vector3 topSize, Material material)
        {
            var table = new GameObject(name);
            table.transform.SetParent(parent, false);
            table.transform.position = worldPosition;

            CreateCube("Top", table.transform, new Vector3(0f, 0.86f, 0f), topSize, material, true);

            const float legHeight = 0.86f;
            const float legThickness = 0.08f;
            float x = topSize.x * 0.5f - 0.12f;
            float z = topSize.z * 0.5f - 0.12f;

            CreateCube("Leg", table.transform, new Vector3(-x, legHeight * 0.5f, -z), new Vector3(legThickness, legHeight, legThickness), material, true);
            CreateCube("Leg", table.transform, new Vector3(x, legHeight * 0.5f, -z), new Vector3(legThickness, legHeight, legThickness), material, true);
            CreateCube("Leg", table.transform, new Vector3(-x, legHeight * 0.5f, z), new Vector3(legThickness, legHeight, legThickness), material, true);
            CreateCube("Leg", table.transform, new Vector3(x, legHeight * 0.5f, z), new Vector3(legThickness, legHeight, legThickness), material, true);

            return table;
        }

        static Transform CreateTerraExtension(Transform table, Vector3 localOffset, PrototypeMaterials materials)
        {
            var extensionRoot = new GameObject("TerraExtension");
            extensionRoot.transform.SetParent(table, false);
            extensionRoot.transform.localPosition = localOffset;

            CreateModelProp(
                "Assets/Placeholder-Assets/FBX/SM_Table.fbx",
                table,
                "TerraBench",
                table.TransformPoint(new Vector3(localOffset.x, 0f, localOffset.z)),
                Quaternion.identity,
                new Vector3(0.6f, 0.9f, 0.5f),
                materials.Table);

            CreateInvisibleBox("TerraSurface", extensionRoot.transform, new Vector3(0f, 0.88f, 0f), new Vector3(0.6f, 0.04f, 0.5f));

            var spawn = new GameObject("ResidueSpawn").transform;
            spawn.SetParent(extensionRoot.transform, false);
            spawn.localPosition = new Vector3(0f, 0.95f, 0f);

            CreateFlatEarthSymbol(extensionRoot.transform, new Vector3(0f, 0.91f, 0f), materials.Symbol);
            return spawn;
        }

        static void CreateWallElementSymbol(Transform parent, Vector3 localPosition, Quaternion localRotation, AlchemicalElement element, PrototypeMaterials materials)
        {
            var symbolRoot = new GameObject("ElementSymbol_" + element);
            symbolRoot.transform.SetParent(parent, false);
            symbolRoot.transform.localPosition = localPosition;
            symbolRoot.transform.localRotation = localRotation;

            Vector3[] triangle;
            Vector3[] line = null;

            if (element == AlchemicalElement.Aqua)
            {
                triangle = new[]
                {
                    new Vector3(0f, -0.35f, 0f),
                    new Vector3(-0.3f, 0.2f, 0f),
                    new Vector3(0.3f, 0.2f, 0f)
                };
            }
            else if (element == AlchemicalElement.Ignis)
            {
                triangle = new[]
                {
                    new Vector3(0f, 0.35f, 0f),
                    new Vector3(-0.3f, -0.2f, 0f),
                    new Vector3(0.3f, -0.2f, 0f)
                };
            }
            else if (element == AlchemicalElement.Aer)
            {
                triangle = new[]
                {
                    new Vector3(0f, 0.35f, 0f),
                    new Vector3(-0.3f, -0.2f, 0f),
                    new Vector3(0.3f, -0.2f, 0f)
                };
                line = new[]
                {
                    new Vector3(-0.18f, 0.12f, 0f),
                    new Vector3(0.18f, 0.12f, 0f)
                };
            }
            else
            {
                triangle = new[]
                {
                    new Vector3(0f, -0.35f, 0f),
                    new Vector3(-0.3f, 0.2f, 0f),
                    new Vector3(0.3f, 0.2f, 0f)
                };
                line = new[]
                {
                    new Vector3(-0.18f, -0.12f, 0f),
                    new Vector3(0.18f, -0.12f, 0f)
                };
            }

            CreateLine(symbolRoot.transform, "Shape", triangle, true, 0.025f, materials.Symbol);
            if (line != null)
                CreateLine(symbolRoot.transform, "Line", line, false, 0.025f, materials.Symbol);
        }

        static void CreateFlatEarthSymbol(Transform parent, Vector3 localPosition, Material material)
        {
            var symbolRoot = new GameObject("EarthSymbol");
            symbolRoot.transform.SetParent(parent, false);
            symbolRoot.transform.localPosition = localPosition;

            var triangle = new[]
            {
                new Vector3(0f, 0f, 0.2f),
                new Vector3(-0.15f, 0f, -0.12f),
                new Vector3(0.15f, 0f, -0.12f)
            };
            var line = new[]
            {
                new Vector3(-0.1f, 0f, -0.04f),
                new Vector3(0.1f, 0f, -0.04f)
            };

            CreateLine(symbolRoot.transform, "Shape", triangle, true, 0.012f, material);
            CreateLine(symbolRoot.transform, "Line", line, false, 0.012f, material);
        }

        static void CreateLine(Transform parent, string name, Vector3[] points, bool loop, float width, Material material)
        {
            var lineObject = new GameObject(name);
            lineObject.transform.SetParent(parent, false);

            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = loop;
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.widthMultiplier = width;
            if (material != null)
                line.sharedMaterial = material;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
        }

        static ComponentTray CreateTray(GameObject table, PrototypeMaterials materials, float topY = 0.92f, float trayZ = 0f)
        {
            var tray = CreateCube("Tray", table.transform, new Vector3(0f, topY, trayZ), new Vector3(0.6f, 0.04f, 0.4f), materials.Tray, true);
            var sockets = new ItemSocket[6];

            for (int i = 0; i < sockets.Length; i++)
            {
                int row = i / 3;
                int column = i % 3;
                float x = (column - 1) * 0.18f;
                float z = (row - 0.5f) * 0.16f;

                var socketObject = new GameObject("TraySocket_" + (i + 1));
                socketObject.transform.SetParent(tray.transform, false);
                socketObject.transform.localPosition = new Vector3(x, 0.03f, z);

                var box = socketObject.AddComponent<BoxCollider>();
                box.size = new Vector3(0.08f, 0.05f, 0.08f);
                box.isTrigger = true;

                var socket = socketObject.AddComponent<XRSocketInteractor>();
                socket.socketActive = true;
                socket.showInteractableHoverMeshes = false;

                var attach = new GameObject("Attach").transform;
                attach.SetParent(socketObject.transform, false);
                attach.localPosition = new Vector3(0f, 0.03f, 0f);
                socket.attachTransform = attach;

                sockets[i] = socketObject.AddComponent<ItemSocket>();
            }

            var trayComponent = tray.AddComponent<ComponentTray>();
            var serialized = new SerializedObject(trayComponent);
            var slots = serialized.FindProperty("slots");
            slots.arraySize = sockets.Length;
            for (int i = 0; i < sockets.Length; i++)
                slots.GetArrayElementAtIndex(i).objectReferenceValue = sockets[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return trayComponent;
        }

        static void CreateShelfRow(
            Transform parent,
            string name,
            Vector3 worldPosition,
            float frontSign,
            AlchemicalComponent component,
            AlchemicalItem itemPrefab,
            PrototypeMaterials materials)
        {
            var row = new GameObject(name);
            row.transform.SetParent(parent, false);
            row.transform.position = worldPosition;

            var slots = new Transform[3];
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = new GameObject("Slot_" + (i + 1)).transform;
                slot.SetParent(row.transform, false);
                slot.localPosition = new Vector3(0f, 0.03f, (i - 1) * 0.3f);
                slots[i] = slot;
            }

            var buttonObject = new GameObject("RestockButton");
            buttonObject.transform.SetParent(row.transform, false);
            buttonObject.transform.localPosition = new Vector3(0.15f * frontSign, 0.06f, 0f);

            var box = buttonObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.14f, 0.04f, 0.14f);

            buttonObject.AddComponent<XRSimpleInteractable>();
            var button = buttonObject.AddComponent<SimpleButton>();

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Cap";
            cap.transform.SetParent(buttonObject.transform, false);
            cap.transform.localPosition = Vector3.zero;
            cap.transform.localScale = new Vector3(0.12f, 0.01f, 0.12f);
            Object.DestroyImmediate(cap.GetComponent<Collider>());
            var capRenderer = cap.GetComponent<MeshRenderer>();
            if (capRenderer != null && materials.Button != null)
                capRenderer.sharedMaterial = materials.Button;

            var buttonSerialized = new SerializedObject(button);
            buttonSerialized.FindProperty("cap").objectReferenceValue = cap.transform;
            buttonSerialized.FindProperty("pressDirection").vector3Value = Vector3.down;
            buttonSerialized.FindProperty("pressDepth").floatValue = 0.02f;
            buttonSerialized.FindProperty("pressDuration").floatValue = 0.08f;
            buttonSerialized.FindProperty("returnDuration").floatValue = 0.15f;
            buttonSerialized.ApplyModifiedPropertiesWithoutUndo();

            var restockLabel = CreateWorldText(
                row.transform,
                "RestockLabel",
                "REPOR " + component.DisplayName,
                new Vector3(0.15f * frontSign, 0.18f, 0f),
                Quaternion.identity,
                0.018f,
                20f,
                new Vector2(22f, 4f),
                new Color(1f, 0.95f, 0.75f));
            restockLabel.gameObject.AddComponent<WorldBillboard>();

            var rowComponent = row.AddComponent<ShelfRow>();
            var rowSerialized = new SerializedObject(rowComponent);
            rowSerialized.FindProperty("component").objectReferenceValue = component;
            rowSerialized.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
            var slotsProperty = rowSerialized.FindProperty("slots");
            slotsProperty.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++)
                slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            rowSerialized.FindProperty("restockButton").objectReferenceValue = button;
            rowSerialized.FindProperty("restockOnStart").boolValue = true;
            rowSerialized.FindProperty("occupiedRadius").floatValue = 0.15f;
            rowSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void HideRenderers(GameObject go)
        {
            if (go == null)
                return;

            var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = false;
        }

        static GameObject CreateInvisibleBox(string name, Transform parent, Vector3 localPosition, Vector3 size)
        {
            var go = CreateCube(name, parent, localPosition, size, null, true);
            HideRenderers(go);
            return go;
        }

        static GameObject CreateModelProp(
            string assetPath,
            Transform parent,
            string name,
            Vector3 worldPosition,
            Quaternion worldRotation,
            Vector3 targetSize,
            Material material)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Fulliron] Modelo não encontrado: " + assetPath);
                return null;
            }

            var wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.position = worldPosition;
            wrapper.transform.rotation = worldRotation;

            var model = Object.Instantiate<GameObject>(prefab, wrapper.transform);
            model.name = prefab.name;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning("[Fulliron] Modelo sem renderers: " + assetPath);
                return wrapper;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            float sx = Mathf.Max(targetSize.x, 0.001f) / Mathf.Max(bounds.size.x, 0.001f);
            float sy = Mathf.Max(targetSize.y, 0.001f) / Mathf.Max(bounds.size.y, 0.001f);
            float sz = Mathf.Max(targetSize.z, 0.001f) / Mathf.Max(bounds.size.z, 0.001f);
            float scale = Mathf.Min(sx, Mathf.Min(sy, sz));

            model.transform.localScale = Vector3.one * scale;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Vector3 offset = new Vector3(
                bounds.center.x - wrapper.transform.position.x,
                bounds.min.y - wrapper.transform.position.y,
                bounds.center.z - wrapper.transform.position.z);
            model.transform.position -= offset;

            if (material != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].sharedMaterial = material;
            }

            return wrapper;
        }

        static SimpleButton CreateButton(Transform parent, string name, Vector3 localPosition, PrototypeMaterials materials, string label = null)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            buttonObject.transform.localPosition = localPosition;

            var box = buttonObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.14f, 0.04f, 0.14f);

            buttonObject.AddComponent<XRSimpleInteractable>();
            var button = buttonObject.AddComponent<SimpleButton>();

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Cap";
            cap.transform.SetParent(buttonObject.transform, false);
            cap.transform.localPosition = Vector3.zero;
            cap.transform.localScale = new Vector3(0.12f, 0.01f, 0.12f);
            Object.DestroyImmediate(cap.GetComponent<Collider>());
            var capRenderer = cap.GetComponent<MeshRenderer>();
            if (capRenderer != null && materials.Button != null)
                capRenderer.sharedMaterial = materials.Button;

            var serialized = new SerializedObject(button);
            serialized.FindProperty("cap").objectReferenceValue = cap.transform;
            serialized.FindProperty("pressDirection").vector3Value = Vector3.down;
            serialized.FindProperty("pressDepth").floatValue = 0.02f;
            serialized.FindProperty("pressDuration").floatValue = 0.08f;
            serialized.FindProperty("returnDuration").floatValue = 0.15f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (!string.IsNullOrEmpty(label))
            {
                var labelText = CreateWorldText(
                    parent,
                    name + "_Label",
                    label,
                    localPosition + new Vector3(0f, 0.12f, 0f),
                    Quaternion.identity,
                    0.018f,
                    22f,
                    new Vector2(20f, 4f),
                    new Color(1f, 0.95f, 0.75f));
                labelText.gameObject.AddComponent<WorldBillboard>();
            }

            return button;
        }

        static void CreateSocketLabel(Transform parent, string text, Vector3 localPosition)
        {
            var label = CreateWorldText(
                parent,
                "Label_" + text,
                text,
                localPosition,
                Quaternion.identity,
                0.012f,
                16f,
                new Vector2(10f, 2f),
                new Color(1f, 0.95f, 0.75f));
            label.gameObject.AddComponent<WorldBillboard>();
        }

        static ItemSocket CreateElementSocket(Transform parent, string name, AlchemicalElement element, Vector3 localPosition)
        {
            var socketObject = new GameObject(name);
            socketObject.transform.SetParent(parent, false);
            socketObject.transform.localPosition = localPosition;

            var box = socketObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.08f, 0.05f, 0.08f);
            box.isTrigger = true;

            var socket = socketObject.AddComponent<XRSocketInteractor>();
            socket.socketActive = true;
            socket.showInteractableHoverMeshes = false;

            var attach = new GameObject("Attach").transform;
            attach.SetParent(socketObject.transform, false);
            attach.localPosition = new Vector3(0f, 0.03f, 0f);
            socket.attachTransform = attach;

            var itemSocket = socketObject.AddComponent<ItemSocket>();
            var serialized = new SerializedObject(itemSocket);
            serialized.FindProperty("element").enumValueIndex = (int)element;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return itemSocket;
        }

        static GaugeBar CreateGauge(Transform parent, Vector3 localPosition, PrototypeMaterials materials)
        {
            var root = new GameObject("HeatGauge");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;

            CreateCube("Background", root.transform, new Vector3(0f, 0.15f, 0f), new Vector3(0.16f, 0.3f, 0.06f), materials.Tray, false);
            var fill = CreateCube("Fill", root.transform, new Vector3(0f, 0.15f, 0.035f), new Vector3(0.1f, 0.26f, 0.02f), materials.Button, false);

            var gauge = root.AddComponent<GaugeBar>();
            var label = CreateWorldText(
                root.transform,
                "Label",
                "Calor 0",
                new Vector3(0f, 0.35f, 0f),
                Quaternion.identity,
                0.02f,
                24f,
                new Vector2(20f, 5f),
                new Color(1f, 0.95f, 0.8f));
            label.gameObject.AddComponent<WorldBillboard>();

            var serialized = new SerializedObject(gauge);
            serialized.FindProperty("fill").objectReferenceValue = fill.transform;
            serialized.FindProperty("fillRenderer").objectReferenceValue = fill.GetComponent<MeshRenderer>();
            serialized.FindProperty("label").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return gauge;
        }

        static void CreateFlatElementSymbol(Transform parent, Vector3 localPosition, AlchemicalElement element, Material material)
        {
            var root = new GameObject("Symbol_" + element);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;

            Vector3[] triangle;
            Vector3[] line = null;

            if (element == AlchemicalElement.Aqua)
            {
                triangle = new[]
                {
                    new Vector3(0f, 0f, 0.2f),
                    new Vector3(-0.15f, 0f, -0.12f),
                    new Vector3(0.15f, 0f, -0.12f)
                };
            }
            else if (element == AlchemicalElement.Ignis)
            {
                triangle = new[]
                {
                    new Vector3(0f, 0f, -0.2f),
                    new Vector3(-0.15f, 0f, 0.12f),
                    new Vector3(0.15f, 0f, 0.12f)
                };
            }
            else if (element == AlchemicalElement.Aer)
            {
                triangle = new[]
                {
                    new Vector3(0f, 0f, -0.2f),
                    new Vector3(-0.15f, 0f, 0.12f),
                    new Vector3(0.15f, 0f, 0.12f)
                };
                line = new[]
                {
                    new Vector3(-0.1f, 0f, 0.04f),
                    new Vector3(0.1f, 0f, 0.04f)
                };
            }
            else
            {
                triangle = new[]
                {
                    new Vector3(0f, 0f, 0.2f),
                    new Vector3(-0.15f, 0f, -0.12f),
                    new Vector3(0.15f, 0f, -0.12f)
                };
                line = new[]
                {
                    new Vector3(-0.1f, 0f, -0.04f),
                    new Vector3(0.1f, 0f, -0.04f)
                };
            }

            CreateLine(root.transform, "Shape", triangle, true, 0.012f, material);
            if (line != null)
                CreateLine(root.transform, "Line", line, false, 0.012f, material);
        }

        static TextMeshPro CreateWorldText(
            Transform parent,
            string name,
            string text,
            Vector3 localPosition,
            Quaternion localRotation,
            float scale,
            float fontSize,
            Vector2 size,
            Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = Vector3.one * scale;

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = GetFont();
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = text;
            tmp.color = color;
            tmp.rectTransform.sizeDelta = size;

            if (go.GetComponent<MeshFilter>() == null)
                go.AddComponent<MeshFilter>();

            tmp.ForceMeshUpdate();
            return tmp;
        }

        static TMP_FontAsset GetFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/VRTemplateAssets/Fonts/Inter/Inter-Regular SDF.asset");
            if (font == null)
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null)
                font = TMP_Settings.defaultFontAsset;
            return font;
        }

        static void CreatePointLight(Transform parent, string name, Vector3 localPosition, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.color = new Color(1f, 0.93f, 0.82f);
        }

        class PrototypeMaterials
        {
            public Material Item;
            public Material Table;
            public Material Shelf;
            public Material Tray;
            public Material Button;
            public Material Wall;
            public Material Symbol;
            public Material Pages;
            public Material Leather;
        }

        class PrototypeData
        {
            public AlchemicalComponent Sulfur;
            public AlchemicalComponent Mercury;
            public AlchemicalComponent Salt;
            public AlchemicalComponent Dew;
            public AlchemicalComponent Iron;
            public AlchemyDatabase Database;
        }
    }
}
