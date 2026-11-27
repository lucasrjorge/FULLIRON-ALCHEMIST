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
                Item = GetOrCreateMaterial(MaterialsFolder + "/M_Item_Base.mat", Color.white, 0.45f),
                Table = GetOrCreateMaterial(MaterialsFolder + "/M_Table.mat", Hex("#6B4A2B"), 0.2f),
                Shelf = GetOrCreateMaterial(MaterialsFolder + "/M_Shelf.mat", Hex("#5A3E2B"), 0.2f),
                Tray = GetOrCreateMaterial(MaterialsFolder + "/M_Tray.mat", Hex("#3A3F45"), 0.55f),
                Button = GetOrCreateMaterial(MaterialsFolder + "/M_Button.mat", Hex("#B08D57"), 0.75f),
                Wall = GetOrCreateMaterial(MaterialsFolder + "/M_Wall.mat", Hex("#8C8C84"), 0.1f),
                Symbol = GetOrCreateSymbolMaterial(MaterialsFolder + "/M_Symbol.mat", Hex("#F0C15A")),
                Pages = GetOrCreateMaterial(MaterialsFolder + "/M_Pages.mat", Hex("#E8D8B8"), 0.05f),
                Leather = GetOrCreateMaterial(MaterialsFolder + "/M_Leather.mat", Hex("#5A2E1E"), 0.15f),
                WoodDark = GetOrCreateMaterial(MaterialsFolder + "/M_WoodDark.mat", Hex("#3E2A1A"), 0.15f),
                WoodLight = GetOrCreateMaterial(MaterialsFolder + "/M_WoodLight.mat", Hex("#8A6338"), 0.25f),
                Stone = GetOrCreateMaterial(MaterialsFolder + "/M_Stone.mat", Hex("#76766E"), 0.12f),
                StoneDark = GetOrCreateMaterial(MaterialsFolder + "/M_StoneDark.mat", Hex("#44443E"), 0.15f),
                Metal = GetOrCreateMaterial(MaterialsFolder + "/M_Metal.mat", Hex("#2E3238"), 0.6f),
                Brass = GetOrCreateMaterial(MaterialsFolder + "/M_Brass.mat", Hex("#C9A35A"), 0.8f),
                Glass = GetOrCreateMaterial(MaterialsFolder + "/M_Glass.mat", Hex("#BCD2DC"), 0.9f),
                Cork = GetOrCreateMaterial(MaterialsFolder + "/M_Cork.mat", Hex("#A97B4C"), 0.1f),
                Fabric = GetOrCreateMaterial(MaterialsFolder + "/M_Fabric.mat", Hex("#6B1E1E"), 0.05f),
                Ember = GetOrCreateEmissiveMaterial(MaterialsFolder + "/M_Ember.mat", Hex("#FF7A1A"), Hex("#FF3D00"), 2.5f),
                Hover = GetOrCreateSymbolMaterial(MaterialsFolder + "/M_Hover.mat", Hex("#121212"))
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

        static Material GetOrCreateEmissiveMaterial(string path, Color color, Color emissionColor, float intensity)
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
            material.SetColor("_EmissionColor", emissionColor * intensity);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
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
            collider.radius = 0.06f;

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

            var visualRoot = new GameObject("Visual").transform;
            visualRoot.SetParent(root.transform, false);
            visualRoot.localPosition = Vector3.zero;

            CreateCylinder(visualRoot, "Base", new Vector3(0f, -0.044f, 0f), 0.052f, 0.012f, materials.Brass, false);
            var liquid = CreateSphere(visualRoot, "Liquid", new Vector3(0f, 0.0025f, 0f), 0.095f, materials.Item, false);
            CreateCylinder(visualRoot, "Neck", new Vector3(0f, 0.065f, 0f), 0.018f, 0.04f, materials.Glass, false);
            CreateCylinder(visualRoot, "Cork", new Vector3(0f, 0.0925f, 0f), 0.022f, 0.025f, materials.Cork, false);

            var liquidRenderer = liquid.GetComponent<MeshRenderer>();

            var label = CreateWorldText(
                root.transform,
                "Label",
                string.Empty,
                new Vector3(0f, 0.2f, 0f),
                Quaternion.identity,
                0.02f,
                36f,
                new Vector2(20f, 10f),
                new Color(1f, 0.95f, 0.8f));

            var itemLabel = label.gameObject.AddComponent<ItemLabel>();

            var visualSerialized = new SerializedObject(visuals);
            visualSerialized.FindProperty("item").objectReferenceValue = item;
            visualSerialized.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            var renderers = visualSerialized.FindProperty("renderers");
            renderers.arraySize = 1;
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = liquidRenderer;
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
            labelSerialized.FindProperty("hideWhenEmpty").boolValue = true;
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
            RenderSettings.ambientLight = new Color(0.42f, 0.37f, 0.32f);
            RenderSettings.ambientIntensity = 1f;

            var sun = GameObject.Find("Directional Light");
            if (sun != null && sun.TryGetComponent<Light>(out var sunLight))
            {
                sunLight.intensity = 0.9f;
                sunLight.color = new Color(1f, 0.94f, 0.86f);
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
            plane.transform.rotation = Quaternion.identity;
            plane.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

            var planeRenderer = plane.GetComponent<MeshRenderer>();
            if (planeRenderer != null)
                planeRenderer.sharedMaterial = materials.WoodDark;

            if (plane.GetComponent<TeleportationArea>() == null)
                plane.AddComponent<TeleportationArea>();

            var room = new GameObject("Room").transform;
            room.SetParent(root.transform, false);

            BuildRoom(room, materials);
            BuildLighting(room);
            BuildWallSymbol(room, materials);

            var grimoireObject = new GameObject("GrimoireManager");
            grimoireObject.transform.SetParent(root.transform, false);
            var grimoire = grimoireObject.AddComponent<GrimoireManager>();
            var grimoireSerialized = new SerializedObject(grimoire);
            grimoireSerialized.FindProperty("database").objectReferenceValue = data.Database;
            grimoireSerialized.ApplyModifiedPropertiesWithoutUndo();

            var stations = new GameObject("Stations").transform;
            stations.SetParent(root.transform, false);

            BuildSymbolTable(stations, data, materials, itemPrefab, grimoire);

            BuildCalcinationStation(stations, data, materials, itemPrefab, grimoire);

            BuildGrimoireStation(stations, grimoire, materials);
            BuildShelves(root.transform, data, materials, itemPrefab);

            var log = Object.FindAnyObjectByType<PrototypeLog>();
            if (log == null)
            {
                var logObject = new GameObject("PrototypeLog");
                logObject.transform.SetParent(root.transform, false);
                log = logObject.AddComponent<PrototypeLog>();
            }

            var logSerialized = new SerializedObject(log);
            logSerialized.FindProperty("display").objectReferenceValue = null;
            logSerialized.FindProperty("maxLines").intValue = 8;
            logSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        static void BuildRoom(Transform room, PrototypeMaterials materials)
        {
            const float half = 4f;
            const float height = 3f;
            const float thickness = 0.2f;

            CreateBox(room, "Wall_North", new Vector3(0f, height * 0.5f, half), new Vector3(8f, height, thickness), materials.Wall, true);
            CreateBox(room, "Wall_South", new Vector3(0f, height * 0.5f, -half), new Vector3(8f, height, thickness), materials.Wall, true);
            CreateBox(room, "Wall_East", new Vector3(half, height * 0.5f, 0f), new Vector3(thickness, height, 8f), materials.Wall, true);
            CreateBox(room, "Wall_West", new Vector3(-half, height * 0.5f, 0f), new Vector3(thickness, height, 8f), materials.Wall, true);

            CreateBox(room, "Ceiling", new Vector3(0f, height, 0f), new Vector3(8f, 0.12f, 8f), materials.StoneDark, false);
            CreateBox(room, "Beam_01", new Vector3(0f, height - 0.08f, -2.4f), new Vector3(8f, 0.14f, 0.18f), materials.WoodDark, false);
            CreateBox(room, "Beam_02", new Vector3(0f, height - 0.08f, 0f), new Vector3(8f, 0.14f, 0.18f), materials.WoodDark, false);
            CreateBox(room, "Beam_03", new Vector3(0f, height - 0.08f, 2.4f), new Vector3(8f, 0.14f, 0.18f), materials.WoodDark, false);

            CreateBox(room, "Wainscot_North", new Vector3(0f, 0.55f, 3.885f), new Vector3(7.8f, 1.1f, 0.03f), materials.WoodDark, false);
            CreateBox(room, "Wainscot_South", new Vector3(0f, 0.55f, -3.885f), new Vector3(7.8f, 1.1f, 0.03f), materials.WoodDark, false);
            CreateBox(room, "Wainscot_East", new Vector3(3.885f, 0.55f, 0f), new Vector3(0.03f, 1.1f, 7.8f), materials.WoodDark, false);
            CreateBox(room, "Wainscot_West", new Vector3(-3.885f, 0.55f, 0f), new Vector3(0.03f, 1.1f, 7.8f), materials.WoodDark, false);

            CreateBox(room, "Baseboard_North", new Vector3(0f, 0.08f, 3.87f), new Vector3(7.8f, 0.16f, 0.06f), materials.StoneDark, false);
            CreateBox(room, "Baseboard_South", new Vector3(0f, 0.08f, -3.87f), new Vector3(7.8f, 0.16f, 0.06f), materials.StoneDark, false);
            CreateBox(room, "Baseboard_East", new Vector3(3.87f, 0.08f, 0f), new Vector3(0.06f, 0.16f, 7.8f), materials.StoneDark, false);
            CreateBox(room, "Baseboard_West", new Vector3(-3.87f, 0.08f, 0f), new Vector3(0.06f, 0.16f, 7.8f), materials.StoneDark, false);

            CreateBox(room, "Crown_North", new Vector3(0f, 2.92f, 3.87f), new Vector3(7.8f, 0.12f, 0.08f), materials.StoneDark, false);
            CreateBox(room, "Crown_South", new Vector3(0f, 2.92f, -3.87f), new Vector3(7.8f, 0.12f, 0.08f), materials.StoneDark, false);
            CreateBox(room, "Crown_East", new Vector3(3.87f, 2.92f, 0f), new Vector3(0.08f, 0.12f, 7.8f), materials.StoneDark, false);
            CreateBox(room, "Crown_West", new Vector3(-3.87f, 2.92f, 0f), new Vector3(0.08f, 0.12f, 7.8f), materials.StoneDark, false);

            CreateBox(room, "Rug", new Vector3(0f, 0.012f, 1.7f), new Vector3(3.6f, 0.02f, 2.4f), materials.Fabric, false);
            CreateBox(room, "RugTrim", new Vector3(0f, 0.024f, 1.7f), new Vector3(3.2f, 0.006f, 2.0f), materials.Leather, false);

            BuildChandelier(room, materials);
        }

        static void BuildChandelier(Transform room, PrototypeMaterials materials)
        {
            var root = new GameObject("Chandelier").transform;
            root.SetParent(room, false);
            root.localPosition = new Vector3(0f, 2.55f, 0.4f);

            CreateCylinder(root, "Chain", new Vector3(0f, 0.3f, 0f), 0.012f, 0.5f, materials.Metal, false);
            CreateBox(root, "ArmX", Vector3.zero, new Vector3(1.2f, 0.06f, 0.08f), materials.WoodDark, false);
            CreateBox(root, "ArmZ", Vector3.zero, new Vector3(0.08f, 0.06f, 1.2f), materials.WoodDark, false);
            CreateCylinder(root, "Hub", new Vector3(0f, 0.02f, 0f), 0.09f, 0.1f, materials.Brass, false);

            var offsets = new[]
            {
                new Vector3(-0.5f, 0.05f, 0f),
                new Vector3(0.5f, 0.05f, 0f),
                new Vector3(0f, 0.05f, -0.5f),
                new Vector3(0f, 0.05f, 0.5f)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                CreateCylinder(root, "Candle_" + i, offsets[i], 0.022f, 0.12f, materials.Pages, false);
                CreateSphere(root, "Flame_" + i, offsets[i] + new Vector3(0f, 0.08f, 0f), 0.035f, materials.Ember, false);
            }
        }

        static void BuildLighting(Transform room)
        {
            CreatePointLight(room, "Light_Center", new Vector3(0f, 2.55f, 0.4f), 8f, 2.4f);
            CreatePointLight(room, "Light_Symbol", new Vector3(0f, 2.1f, 2.2f), 5f, 1.8f);
            CreatePointLight(room, "Light_ShelfWest", new Vector3(-3.0f, 2.0f, 1.2f), 5f, 1.4f);
            CreatePointLight(room, "Light_ShelfEast", new Vector3(3.0f, 2.0f, 2.2f), 4.5f, 1.3f);
            CreatePointLight(room, "Light_Grimoire", new Vector3(-2.3f, 2.0f, 2.3f), 4f, 1.5f);
        }

        static void BuildWallSymbol(Transform room, PrototypeMaterials materials)
        {
            CreateBox(room, "IgnisPlaque", new Vector3(3.88f, 2.3f, 0.4f), new Vector3(0.03f, 0.9f, 0.9f), materials.StoneDark, false);
            CreateWallElementSymbol(room, new Vector3(3.85f, 2.3f, 0.4f), Quaternion.LookRotation(Vector3.left), AlchemicalElement.Ignis, materials);
        }

        static void BuildSymbolTable(Transform stations, PrototypeData data, PrototypeMaterials materials, AlchemicalItem itemPrefab, GrimoireManager grimoire)
        {
            var symbolTable = new GameObject("SymbolTable");
            symbolTable.transform.SetParent(stations, false);
            symbolTable.transform.position = new Vector3(0f, 0f, 2.35f);
            symbolTable.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var table = symbolTable.transform;

            CreateBox(table, "Cabinet", new Vector3(0f, 0.39f, 0f), new Vector3(1.8f, 0.78f, 1.1f), materials.WoodDark, true);
            CreateBox(table, "Top", new Vector3(0f, 0.84f, 0f), new Vector3(2.0f, 0.12f, 1.3f), materials.Table, true);
            CreateBox(table, "FrontPanel", new Vector3(0f, 0.52f, 0.56f), new Vector3(1.5f, 0.34f, 0.03f), materials.WoodLight, false);
            CreateBox(table, "DrawerHandle", new Vector3(0f, 0.52f, 0.585f), new Vector3(0.34f, 0.035f, 0.035f), materials.Brass, false);

            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    CreateBox(table, "Leg", new Vector3(sx * 0.9f, 0.42f, sz * 0.58f), new Vector3(0.12f, 0.84f, 0.12f), materials.WoodDark, true);
                }
            }

            CreateBox(table, "LipFront", new Vector3(0f, 0.915f, 0.635f), new Vector3(2.02f, 0.04f, 0.04f), materials.WoodDark, false);
            CreateBox(table, "LipBack", new Vector3(0f, 0.915f, -0.635f), new Vector3(2.02f, 0.04f, 0.04f), materials.WoodDark, false);
            CreateBox(table, "LipLeft", new Vector3(-0.985f, 0.915f, 0f), new Vector3(0.04f, 0.04f, 1.24f), materials.WoodDark, false);
            CreateBox(table, "LipRight", new Vector3(0.985f, 0.915f, 0f), new Vector3(0.04f, 0.04f, 1.24f), materials.WoodDark, false);

            CreateCylinder(table, "SymbolDisc", new Vector3(0f, 0.912f, -0.1f), 0.4f, 0.02f, materials.StoneDark, false);
            CreateFlatCircle(table, "OuterRing", new Vector3(0f, 0.925f, -0.1f), 0.4f, 0.012f, materials.Symbol);
            CreateFlatCircle(table, "InnerRing", new Vector3(0f, 0.926f, -0.1f), 0.16f, 0.008f, materials.Symbol);
            CreateCylinder(table, "SunDot", new Vector3(0f, 0.928f, -0.1f), 0.024f, 0.008f, materials.Symbol, false);

            var elementSockets = new ItemSocket[4];
            elementSockets[0] = CreateElementSocket(table, "ElementSocket_Aqua", AlchemicalElement.Aqua, new Vector3(-0.46f, 0.9f, -0.1f));
            elementSockets[1] = CreateElementSocket(table, "ElementSocket_Ignis", AlchemicalElement.Ignis, new Vector3(0.46f, 0.9f, -0.1f));
            elementSockets[2] = CreateElementSocket(table, "ElementSocket_Terra", AlchemicalElement.Terra, new Vector3(0f, 0.9f, 0.3f));
            elementSockets[3] = CreateElementSocket(table, "ElementSocket_Aer", AlchemicalElement.Aer, new Vector3(0f, 0.9f, -0.5f));

            CreateCylinder(table, "Ring_Aqua", new Vector3(-0.46f, 0.915f, -0.1f), 0.085f, 0.012f, materials.Brass, false);
            CreateCylinder(table, "Ring_Ignis", new Vector3(0.46f, 0.915f, -0.1f), 0.085f, 0.012f, materials.Brass, false);
            CreateCylinder(table, "Ring_Terra", new Vector3(0f, 0.915f, 0.3f), 0.085f, 0.012f, materials.Brass, false);
            CreateCylinder(table, "Ring_Aer", new Vector3(0f, 0.915f, -0.5f), 0.085f, 0.012f, materials.Brass, false);

            CreateElementLabelProbe(table, AlchemicalElement.Aqua, "AQUA", new Vector3(-0.7f, 0.912f, -0.1f), materials);
            CreateElementLabelProbe(table, AlchemicalElement.Ignis, "IGNIS", new Vector3(0.7f, 0.912f, -0.1f), materials);
            CreateElementLabelProbe(table, AlchemicalElement.Terra, "TERRA", new Vector3(-0.25f, 0.912f, 0.3f), materials);
            CreateElementLabelProbe(table, AlchemicalElement.Aer, "AER", new Vector3(0.25f, 0.912f, -0.5f), materials);

            CreateTray(symbolTable, materials);
            var castButton = CreateButton(table, "CastButton", new Vector3(0.72f, 0.9f, 0.4f), materials, "CONJURAR");

            var resultSpawn = new GameObject("ResultSpawn").transform;
            resultSpawn.SetParent(table, false);
            resultSpawn.localPosition = new Vector3(0f, 1.0f, -0.1f);

            var symbolTableComponent = symbolTable.AddComponent<SymbolTable>();
            var symbolSerialized = new SerializedObject(symbolTableComponent);
            symbolSerialized.FindProperty("database").objectReferenceValue = data.Database;
            symbolSerialized.FindProperty("castButton").objectReferenceValue = castButton;
            symbolSerialized.FindProperty("grimoire").objectReferenceValue = grimoire;
            symbolSerialized.FindProperty("resultItemPrefab").objectReferenceValue = itemPrefab;
            symbolSerialized.FindProperty("resultSpawn").objectReferenceValue = resultSpawn;
            symbolSerialized.FindProperty("feedbackText").objectReferenceValue = null;
            symbolSerialized.FindProperty("symbolId").stringValue = "ouro";
            var socketArray = symbolSerialized.FindProperty("sockets");
            socketArray.arraySize = elementSockets.Length;
            for (int i = 0; i < elementSockets.Length; i++)
                socketArray.GetArrayElementAtIndex(i).objectReferenceValue = elementSockets[i];
            symbolSerialized.ApplyModifiedPropertiesWithoutUndo();

            SpawnTestItem(table, itemPrefab, data.Sulfur, new Vector3(-0.9f, 0.95f, -0.4f), "TestItem_Enxofre");
            SpawnTestItem(table, itemPrefab, data.Salt, new Vector3(-0.9f, 0.95f, -0.1f), "TestItem_Sal");
            SpawnTestItem(table, itemPrefab, data.Dew, new Vector3(-0.9f, 0.95f, 0.2f), "TestItem_Orvalho");
        }

        static void BuildCalcinationStation(Transform stations, PrototypeData data, PrototypeMaterials materials, AlchemicalItem itemPrefab, GrimoireManager grimoire)
        {
            var calcination = new GameObject("CalcinationStation");
            calcination.transform.SetParent(stations, false);
            calcination.transform.position = new Vector3(2.65f, 0f, 0.4f);
            calcination.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            var bench = calcination.transform;

            CreateBox(bench, "FurnaceBody", new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 1.0f), materials.Stone, true);
            CreateBox(bench, "FurnaceTop", new Vector3(0f, 0.92f, 0f), new Vector3(1.5f, 0.08f, 1.08f), materials.StoneDark, true);

            CreateBox(bench, "FireboxInset", new Vector3(0f, 0.48f, 0.505f), new Vector3(0.7f, 0.56f, 0.02f), materials.Hover, false);
            CreateBox(bench, "FireboxPillarL", new Vector3(-0.4f, 0.48f, 0.52f), new Vector3(0.1f, 0.66f, 0.06f), materials.StoneDark, false);
            CreateBox(bench, "FireboxPillarR", new Vector3(0.4f, 0.48f, 0.52f), new Vector3(0.1f, 0.66f, 0.06f), materials.StoneDark, false);
            CreateBox(bench, "FireboxLintel", new Vector3(0f, 0.83f, 0.52f), new Vector3(0.9f, 0.12f, 0.06f), materials.StoneDark, false);
            CreateBox(bench, "FireboxSill", new Vector3(0f, 0.14f, 0.52f), new Vector3(0.9f, 0.08f, 0.06f), materials.StoneDark, false);

            for (int i = -1; i <= 1; i++)
                CreateBox(bench, "GrateBar_" + i, new Vector3(i * 0.2f, 0.48f, 0.53f), new Vector3(0.025f, 0.56f, 0.025f), materials.Metal, false);

            for (int i = 0; i < 3; i++)
            {
                var log = CreateCylinder(bench, "Log_" + i, new Vector3(-0.15f + i * 0.15f, 0.3f, 0.44f), 0.055f, 0.42f, materials.WoodDark, false);
                log.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            CreateSphere(bench, "Ember_A", new Vector3(-0.1f, 0.22f, 0.45f), 0.07f, materials.Ember, false);
            CreateSphere(bench, "Ember_B", new Vector3(0.12f, 0.2f, 0.43f), 0.06f, materials.Ember, false);

            var fireLightObject = new GameObject("FireLight");
            fireLightObject.transform.SetParent(bench, false);
            fireLightObject.transform.localPosition = new Vector3(0f, 0.45f, 0.4f);
            var fireLight = fireLightObject.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.range = 4f;
            fireLight.intensity = 0.5f;
            fireLight.shadows = LightShadows.None;
            fireLight.color = new Color(1f, 0.45f, 0.12f);

            CreateCylinder(bench, "Crucible", new Vector3(0f, 1.05f, 0f), 0.24f, 0.18f, materials.Metal, false);
            CreateCylinder(bench, "CrucibleRim", new Vector3(0f, 1.15f, 0f), 0.27f, 0.03f, materials.Brass, false);
            CreateCylinder(bench, "CrucibleBowl", new Vector3(0f, 1.14f, 0f), 0.21f, 0.01f, materials.Hover, false);

            var calcinationSocket = CreateElementSocket(bench, "CalcinationSocket", AlchemicalElement.None, new Vector3(0f, 1.15f, 0f));

            CreateBox(bench, "Chimney", new Vector3(0f, 1.55f, -0.36f), new Vector3(0.6f, 1.1f, 0.3f), materials.StoneDark, false);
            CreateBox(bench, "ChimneyCap", new Vector3(0f, 2.12f, -0.36f), new Vector3(0.75f, 0.1f, 0.42f), materials.Metal, false);

            CreateBox(bench, "BellowsBase", new Vector3(-0.88f, 0.18f, 0.18f), new Vector3(0.35f, 0.16f, 0.55f), materials.WoodDark, false);
            CreateBox(bench, "BellowsLower", new Vector3(-0.88f, 0.32f, 0.18f), new Vector3(0.36f, 0.05f, 0.45f), materials.Leather, false);
            var upper = CreateBox(bench, "BellowsUpper", new Vector3(-0.88f, 0.46f, 0.28f), new Vector3(0.36f, 0.05f, 0.45f), materials.Leather, false);
            upper.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            CreateCylinder(bench, "BellowsHandle", new Vector3(-0.88f, 0.55f, 0.5f), 0.025f, 0.3f, materials.WoodLight, false);

            var pumpButton = CreateButton(bench, "PumpButton", new Vector3(-0.48f, 0.96f, 0.36f), materials, "BOMBEAR");
            var sealButton = CreateButton(bench, "SealButton", new Vector3(0.48f, 0.96f, 0.36f), materials, "SELAR");
            var heatGauge = CreateGauge(bench, new Vector3(0.55f, 0.96f, 0.1f), materials);
            var residueSpawn = CreateTerraExtension(bench, new Vector3(1.15f, 0f, 0.05f), materials);

            var calcinationStation = calcination.AddComponent<CalcinationStation>();
            var calcinationSerialized = new SerializedObject(calcinationStation);
            calcinationSerialized.FindProperty("database").objectReferenceValue = data.Database;
            calcinationSerialized.FindProperty("itemSocket").objectReferenceValue = calcinationSocket;
            calcinationSerialized.FindProperty("pumpButton").objectReferenceValue = pumpButton;
            calcinationSerialized.FindProperty("sealButton").objectReferenceValue = sealButton;
            calcinationSerialized.FindProperty("heatGauge").objectReferenceValue = heatGauge;
            calcinationSerialized.FindProperty("statusText").objectReferenceValue = null;
            calcinationSerialized.FindProperty("fireLight").objectReferenceValue = fireLight;
            calcinationSerialized.FindProperty("residueSpawn").objectReferenceValue = residueSpawn;
            calcinationSerialized.FindProperty("residueItemPrefab").objectReferenceValue = itemPrefab;
            calcinationSerialized.FindProperty("grimoire").objectReferenceValue = grimoire;
            calcinationSerialized.ApplyModifiedPropertiesWithoutUndo();

            SpawnTestItem(bench, itemPrefab, data.Sulfur, new Vector3(0.55f, 1.01f, -0.35f), "TestItem_Enxofre_Calcinatio");
        }

        static void BuildGrimoireStation(Transform stations, GrimoireManager grimoire, PrototypeMaterials materials)
        {
            var grimoireStation = new GameObject("GrimoireLectern");
            grimoireStation.transform.SetParent(stations, false);
            grimoireStation.transform.position = new Vector3(-2.35f, 0f, 2.6f);
            var lectern = grimoireStation.transform;

            CreateBox(lectern, "Base", new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.55f), materials.WoodDark, true);
            CreateBox(lectern, "BaseTop", new Vector3(0f, 0.94f, 0f), new Vector3(1.0f, 0.08f, 0.62f), materials.Table, true);
            CreateBox(lectern, "PostL", new Vector3(-0.58f, 1.43f, 0.02f), new Vector3(0.08f, 0.9f, 0.08f), materials.WoodDark, false);
            CreateBox(lectern, "PostR", new Vector3(0.58f, 1.43f, 0.02f), new Vector3(0.08f, 0.9f, 0.08f), materials.WoodDark, false);
            CreateBox(lectern, "BookBack", new Vector3(0f, 1.5f, 0.05f), new Vector3(1.36f, 0.96f, 0.06f), materials.Leather, false);
            CreateBox(lectern, "PageLeft", new Vector3(-0.33f, 1.5f, -0.02f), new Vector3(0.62f, 0.84f, 0.02f), materials.Pages, false);
            CreateBox(lectern, "PageRight", new Vector3(0.33f, 1.5f, -0.02f), new Vector3(0.62f, 0.84f, 0.02f), materials.Pages, false);
            CreateBox(lectern, "Spine", new Vector3(0f, 1.5f, 0.03f), new Vector3(0.04f, 0.92f, 0.05f), materials.Leather, false);
            CreateBox(lectern, "TrimTop", new Vector3(0f, 2f, 0.02f), new Vector3(1.44f, 0.04f, 0.08f), materials.Brass, false);
            CreateBox(lectern, "TrimBottom", new Vector3(0f, 1f, 0.02f), new Vector3(1.44f, 0.04f, 0.08f), materials.Brass, false);
            CreateBox(lectern, "TrimLeft", new Vector3(-0.7f, 1.5f, 0.02f), new Vector3(0.04f, 1f, 0.08f), materials.Brass, false);
            CreateBox(lectern, "TrimRight", new Vector3(0.7f, 1.5f, 0.02f), new Vector3(0.04f, 1f, 0.08f), materials.Brass, false);

            var title = CreateWorldText(lectern, "Title", "GRIMÓRIO", new Vector3(0f, 1.93f, -0.045f), Quaternion.identity, 0.04f, 30f, new Vector2(15f, 2f), new Color(1f, 0.88f, 0.5f));
            title.alignment = TextAlignmentOptions.Center;

            var reactions = CreateWorldText(lectern, "Reactions", "Reações:", new Vector3(-0.33f, 1.5f, -0.045f), Quaternion.identity, 0.03f, 24f, new Vector2(20f, 26f), new Color(0.12f, 0.08f, 0.04f));
            reactions.alignment = TextAlignmentOptions.TopLeft;

            var components = CreateWorldText(lectern, "Components", "Componentes:", new Vector3(0.33f, 1.5f, -0.045f), Quaternion.identity, 0.03f, 24f, new Vector2(20f, 26f), new Color(0.12f, 0.08f, 0.04f));
            components.alignment = TextAlignmentOptions.TopLeft;

            var board = grimoireStation.AddComponent<GrimoireBoard>();
            var boardSerialized = new SerializedObject(board);
            boardSerialized.FindProperty("manager").objectReferenceValue = grimoire;
            boardSerialized.FindProperty("titleText").objectReferenceValue = title;
            boardSerialized.FindProperty("reactionsText").objectReferenceValue = reactions;
            boardSerialized.FindProperty("componentsText").objectReferenceValue = components;
            boardSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildShelves(Transform parent, PrototypeData data, PrototypeMaterials materials, AlchemicalItem itemPrefab)
        {
            var shelves = new GameObject("Shelves").transform;
            shelves.SetParent(parent, false);

            CreateShelfUnit(
                shelves,
                "ShelfWest",
                new Vector3(-3.55f, 0f, 1.2f),
                Quaternion.Euler(0f, 90f, 0f),
                2.2f,
                0.4f,
                new[] { data.Sulfur, data.Mercury, data.Salt },
                new[] { 0.9f, 1.2f, 1.5f },
                itemPrefab,
                materials);

            CreateShelfUnit(
                shelves,
                "ShelfEast",
                new Vector3(3.55f, 0f, 2.2f),
                Quaternion.Euler(0f, -90f, 0f),
                1.6f,
                0.4f,
                new[] { data.Dew, data.Iron },
                new[] { 0.95f, 1.25f },
                itemPrefab,
                materials);
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

        static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material, bool keepCollider)
        {
            return CreateBox(parent, name, localPosition, size, material, Quaternion.identity, keepCollider);
        }

        static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material, Quaternion localRotation, bool keepCollider)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = localRotation;
            cube.transform.localScale = size;

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;

            if (!keepCollider)
                DestroyCollider(cube);

            return cube;
        }

        static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, float radius, float height, Material material, bool keepCollider)
        {
            return CreateCylinder(parent, name, localPosition, radius, height, material, Quaternion.identity, keepCollider);
        }

        static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, float radius, float height, Material material, Quaternion localRotation, bool keepCollider)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = localRotation;
            cylinder.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);

            var renderer = cylinder.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;

            if (!keepCollider)
                DestroyCollider(cylinder);

            return cylinder;
        }

        static GameObject CreateSphere(Transform parent, string name, Vector3 localPosition, float diameter, Material material, bool keepCollider)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = Vector3.one * diameter;

            var renderer = sphere.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;

            if (!keepCollider)
                DestroyCollider(sphere);

            return sphere;
        }

        static void DestroyCollider(GameObject go)
        {
            if (go == null)
                return;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
        }

        static void CreateFlatCircle(Transform parent, string name, Vector3 localPosition, float radius, float width, Material material)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;

            var points = new Vector3[48];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = (i / (float)points.Length) * Mathf.PI * 2f;
                points[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            CreateLine(root.transform, "Circle", points, true, width, material);
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
            var root = extensionRoot.transform;

            CreateBox(root, "Body", new Vector3(0f, 0.42f, 0f), new Vector3(0.7f, 0.84f, 0.55f), materials.WoodDark, true);
            CreateBox(root, "Top", new Vector3(0f, 0.88f, 0f), new Vector3(0.75f, 0.08f, 0.6f), materials.Table, true);
            CreateBox(root, "Connector", new Vector3(-0.4f, 0.88f, 0f), new Vector3(0.12f, 0.06f, 0.5f), materials.WoodDark, false);

            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    CreateBox(root, "Leg", new Vector3(sx * 0.28f, 0.21f, sz * 0.22f), new Vector3(0.08f, 0.42f, 0.08f), materials.WoodDark, true);
                }
            }

            CreateFlatEarthSymbol(root, new Vector3(0f, 0.925f, 0f), materials.Symbol);

            var spawn = new GameObject("ResidueSpawn").transform;
            spawn.SetParent(root, false);
            spawn.localPosition = new Vector3(0f, 0.97f, 0f);
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

        static ComponentTray CreateTray(GameObject table, PrototypeMaterials materials, float topY = 0.915f, float trayZ = 0.52f)
        {
            var tray = new GameObject("Tray");
            tray.transform.SetParent(table.transform, false);
            tray.transform.localPosition = new Vector3(0f, topY, trayZ);
            var trayRoot = tray.transform;

            CreateBox(trayRoot, "Base", Vector3.zero, new Vector3(0.72f, 0.03f, 0.26f), materials.Metal, true);
            CreateBox(trayRoot, "RimFront", new Vector3(0f, 0.02f, 0.14f), new Vector3(0.72f, 0.03f, 0.02f), materials.Brass, false);
            CreateBox(trayRoot, "RimBack", new Vector3(0f, 0.02f, -0.14f), new Vector3(0.72f, 0.03f, 0.02f), materials.Brass, false);
            CreateBox(trayRoot, "RimLeft", new Vector3(-0.36f, 0.02f, 0f), new Vector3(0.02f, 0.03f, 0.26f), materials.Brass, false);
            CreateBox(trayRoot, "RimRight", new Vector3(0.36f, 0.02f, 0f), new Vector3(0.02f, 0.03f, 0.26f), materials.Brass, false);

            var sockets = new ItemSocket[6];

            for (int i = 0; i < sockets.Length; i++)
            {
                int row = i / 3;
                int column = i % 3;
                float x = (column - 1) * 0.2f;
                float z = (row - 0.5f) * 0.1f;

                var socketObject = new GameObject("TraySocket_" + (i + 1));
                socketObject.transform.SetParent(trayRoot, false);
                socketObject.transform.localPosition = new Vector3(x, 0.015f, z);

                var box = socketObject.AddComponent<BoxCollider>();
                box.size = new Vector3(0.1f, 0.14f, 0.1f);
                box.center = new Vector3(0f, 0.07f, 0f);
                box.isTrigger = true;

                var socket = socketObject.AddComponent<XRSocketInteractor>();
                socket.socketActive = true;
                socket.showInteractableHoverMeshes = false;

                var attach = new GameObject("Attach").transform;
                attach.SetParent(socketObject.transform, false);
                attach.localPosition = new Vector3(0f, 0.045f, 0f);
                socket.attachTransform = attach;

                sockets[i] = socketObject.AddComponent<ItemSocket>();
                CreateCylinder(trayRoot, "SocketRing_" + (i + 1), new Vector3(x, 0.032f, z), 0.055f, 0.008f, materials.Brass, false);
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

        static void CreateShelfUnit(
            Transform parent,
            string name,
            Vector3 worldPosition,
            Quaternion worldRotation,
            float width,
            float depth,
            AlchemicalComponent[] components,
            float[] heights,
            AlchemicalItem itemPrefab,
            PrototypeMaterials materials)
        {
            var unit = new GameObject(name);
            unit.transform.SetParent(parent, false);
            unit.transform.position = worldPosition;
            unit.transform.rotation = worldRotation;
            var root = unit.transform;

            CreateBox(root, "Back", new Vector3(0f, 1f, -depth * 0.5f + 0.02f), new Vector3(width, 2f, 0.04f), materials.WoodDark, true);
            CreateBox(root, "SideLeft", new Vector3(-width * 0.5f + 0.025f, 1f, 0f), new Vector3(0.05f, 2f, depth), materials.WoodDark, true);
            CreateBox(root, "SideRight", new Vector3(width * 0.5f - 0.025f, 1f, 0f), new Vector3(0.05f, 2f, depth), materials.WoodDark, true);
            CreateBox(root, "Top", new Vector3(0f, 2f, 0f), new Vector3(width + 0.1f, 0.08f, depth + 0.08f), materials.Table, true);
            CreateBox(root, "Bottom", new Vector3(0f, 0.08f, 0f), new Vector3(width, 0.16f, depth), materials.WoodDark, true);

            for (int i = 0; i < components.Length; i++)
            {
                float shelfY = heights[i];
                CreateBox(root, "Shelf_" + i, new Vector3(0f, shelfY - 0.025f, 0f), new Vector3(width - 0.1f, 0.05f, depth), materials.Table, true);
                CreateBox(root, "BracketLeft_" + i, new Vector3(-width * 0.44f, shelfY - 0.09f, 0f), new Vector3(0.04f, 0.12f, depth * 0.8f), materials.WoodDark, false);
                CreateBox(root, "BracketRight_" + i, new Vector3(width * 0.44f, shelfY - 0.09f, 0f), new Vector3(0.04f, 0.12f, depth * 0.8f), materials.WoodDark, false);

                var row = new GameObject("ShelfRow_" + components[i].DisplayName);
                row.transform.SetParent(root, false);
                row.transform.localPosition = new Vector3(0f, shelfY, 0f);

                var slots = new Transform[3];
                for (int s = 0; s < slots.Length; s++)
                {
                    var slot = new GameObject("Slot_" + (s + 1)).transform;
                    slot.SetParent(row.transform, false);
                    slot.localPosition = new Vector3((s - 1) * width * 0.28f, 0.05f, -0.02f);
                    slots[s] = slot;
                }

                var button = CreateButton(row.transform, "RestockButton", new Vector3(width * 0.42f, 0.02f, depth * 0.5f - 0.02f), materials, "REPOR " + components[i].DisplayName);

                var rowComponent = row.AddComponent<ShelfRow>();
                var rowSerialized = new SerializedObject(rowComponent);
                rowSerialized.FindProperty("component").objectReferenceValue = components[i];
                rowSerialized.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
                var slotsProperty = rowSerialized.FindProperty("slots");
                slotsProperty.arraySize = slots.Length;
                for (int s = 0; s < slots.Length; s++)
                    slotsProperty.GetArrayElementAtIndex(s).objectReferenceValue = slots[s];
                rowSerialized.FindProperty("restockButton").objectReferenceValue = button;
                rowSerialized.FindProperty("restockOnStart").boolValue = true;
                rowSerialized.FindProperty("occupiedRadius").floatValue = 0.15f;
                rowSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
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
            box.size = new Vector3(0.16f, 0.08f, 0.16f);
            box.center = new Vector3(0f, 0.03f, 0f);

            buttonObject.AddComponent<XRSimpleInteractable>();
            var button = buttonObject.AddComponent<SimpleButton>();

            CreateCylinder(buttonObject.transform, "Base", new Vector3(0f, 0.005f, 0f), 0.075f, 0.02f, materials.Brass, false);
            CreateCylinder(buttonObject.transform, "GlowRing", new Vector3(0f, 0.016f, 0f), 0.072f, 0.008f, materials.Ember, false);
            CreateCylinder(buttonObject.transform, "Ring", new Vector3(0f, 0.022f, 0f), 0.06f, 0.02f, materials.Metal, false);

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Cap";
            cap.transform.SetParent(buttonObject.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            cap.transform.localScale = new Vector3(0.11f, 0.012f, 0.11f);
            DestroyCollider(cap);
            var capRenderer = cap.GetComponent<MeshRenderer>();
            if (capRenderer != null && materials.Button != null)
                capRenderer.sharedMaterial = materials.Button;

            var serialized = new SerializedObject(button);
            serialized.FindProperty("cap").objectReferenceValue = cap.transform;
            serialized.FindProperty("pressDirection").vector3Value = Vector3.down;
            serialized.FindProperty("pressDepth").floatValue = 0.015f;
            serialized.FindProperty("pressDuration").floatValue = 0.08f;
            serialized.FindProperty("returnDuration").floatValue = 0.15f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (!string.IsNullOrEmpty(label))
                CreateHoverLabel(buttonObject.transform, "HoverLabel", label, new Vector3(0f, 0.17f, 0f), 0.025f, 28f, new Vector2(18f, 4f), materials.Hover);

            return button;
        }

        static HoverLabel CreateHoverLabel(
            Transform parent,
            string name,
            string text,
            Vector3 localPosition,
            float scale,
            float fontSize,
            Vector2 size,
            Material panelMaterial)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPosition;

            var label = CreateWorldText(
                root.transform,
                "Text",
                text,
                Vector3.zero,
                Quaternion.identity,
                scale,
                fontSize,
                size,
                new Color(1f, 0.94f, 0.78f));

            label.ForceMeshUpdate();
            float width = Mathf.Clamp(label.textBounds.size.x * scale + 0.03f, 0.14f, 0.7f);
            float height = Mathf.Clamp(label.textBounds.size.y * scale + 0.02f, 0.06f, 0.18f);
            var panel = CreateBox(root.transform, "Panel", new Vector3(0f, 0f, 0.008f), new Vector3(width, height, 0.004f), panelMaterial, false);

            label.enabled = false;
            var labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null)
                labelRenderer.enabled = false;
            var panelRenderer = panel.GetComponent<MeshRenderer>();
            if (panelRenderer != null)
                panelRenderer.enabled = false;

            var hover = root.AddComponent<HoverLabel>();
            var serialized = new SerializedObject(hover);
            serialized.FindProperty("label").objectReferenceValue = label;
            serialized.FindProperty("faceCamera").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return hover;
        }

        static void CreateElementLabelProbe(
            Transform parent,
            AlchemicalElement element,
            string text,
            Vector3 localPosition,
            PrototypeMaterials materials)
        {
            var probe = CreateBox(parent, "ElementProbe_" + element, localPosition, new Vector3(0.22f, 0.012f, 0.22f), materials.StoneDark, false);
            var box = probe.AddComponent<BoxCollider>();
            box.size = new Vector3(0.22f, 0.03f, 0.22f);
            probe.AddComponent<XRSimpleInteractable>();
            CreateFlatElementSymbol(probe.transform, new Vector3(0f, 0.009f, 0f), element, materials.Symbol);
            CreateHoverLabel(probe.transform, "HoverLabel", text, new Vector3(0f, 0.1f, 0f), 0.025f, 28f, new Vector2(10f, 3f), materials.Hover);
        }

        static ItemSocket CreateElementSocket(Transform parent, string name, AlchemicalElement element, Vector3 localPosition)
        {
            var socketObject = new GameObject(name);
            socketObject.transform.SetParent(parent, false);
            socketObject.transform.localPosition = localPosition;

            var box = socketObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.16f, 0.18f, 0.16f);
            box.center = new Vector3(0f, 0.07f, 0f);
            box.isTrigger = true;

            var socket = socketObject.AddComponent<XRSocketInteractor>();
            socket.socketActive = true;
            socket.showInteractableHoverMeshes = false;

            var attach = new GameObject("Attach").transform;
            attach.SetParent(socketObject.transform, false);
            attach.localPosition = new Vector3(0f, 0.045f, 0f);
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

            CreateCylinder(root.transform, "PostBase", new Vector3(0f, 0.01f, 0f), 0.06f, 0.02f, materials.Brass, false);
            CreateBox(root.transform, "Post", new Vector3(0f, 0.08f, 0f), new Vector3(0.04f, 0.16f, 0.04f), materials.Metal, false);
            CreateBox(root.transform, "Frame", new Vector3(0f, 0.3f, 0f), new Vector3(0.18f, 0.34f, 0.05f), materials.Metal, false);
            CreateBox(root.transform, "Inset", new Vector3(0f, 0.3f, 0.027f), new Vector3(0.12f, 0.28f, 0.01f), materials.Hover, false);
            var fill = CreateBox(root.transform, "Fill", new Vector3(0f, 0.3f, 0.034f), new Vector3(0.08f, 0.24f, 0.008f), materials.Button, false);

            var box = root.AddComponent<BoxCollider>();
            box.size = new Vector3(0.22f, 0.52f, 0.1f);
            box.center = new Vector3(0f, 0.3f, 0f);
            root.AddComponent<XRSimpleInteractable>();

            var hover = CreateHoverLabel(root.transform, "HoverLabel", "Calor 0", new Vector3(0f, 0.53f, 0f), 0.02f, 24f, new Vector2(12f, 4f), materials.Hover);
            var hoverText = hover.GetComponentInChildren<TMP_Text>(true);

            var gauge = root.AddComponent<GaugeBar>();
            var serialized = new SerializedObject(gauge);
            serialized.FindProperty("fill").objectReferenceValue = fill.transform;
            serialized.FindProperty("fillRenderer").objectReferenceValue = fill.GetComponent<MeshRenderer>();
            serialized.FindProperty("label").objectReferenceValue = hoverText;
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
            public Material WoodDark;
            public Material WoodLight;
            public Material Stone;
            public Material StoneDark;
            public Material Metal;
            public Material Brass;
            public Material Glass;
            public Material Cork;
            public Material Fabric;
            public Material Ember;
            public Material Hover;
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
