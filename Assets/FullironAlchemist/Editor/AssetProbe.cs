using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fulliron.Alchemy.Editor
{
    public static class AssetProbe
    {
        [MenuItem("Fulliron/Debug/Log Placeholder Asset Bounds")]
        public static void LogBounds()
        {
            string[] paths =
            {
                "Assets/Placeholder-Assets/AlchemyTableCabinet.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Table.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Desk.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Round_Table.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Bench.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Fireplace.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Cauldron.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Alembic.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Mortar.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Scales.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Test_Tube_Rack.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Shelf.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Wall_Shelf.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Apothecary_Cabinet.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Bookcase_Full.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Spellbook.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Open_Book.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Potion_Vial.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Potion_Flask.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Potion_Round.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Ingredient_Jar.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Lantern.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Candle.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Hourglass.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Drying_Rack.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Firewood_Stack.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Books_Stack.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Scroll.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Bucket.fbx",
                "Assets/Placeholder-Assets/FBX/SM_Broom.fbx"
            };

            for (int p = 0; p < paths.Length; p++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[p]);
                if (prefab == null)
                {
                    Debug.LogWarning("[Probe] missing " + paths[p]);
                    continue;
                }

                var instance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                instance.name = "PROBE_" + prefab.name;
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    Debug.Log("[Probe] " + paths[p] + " | no renderers | scale=" + instance.transform.localScale.ToString("F3"));
                    UnityEngine.Object.DestroyImmediate(instance);
                    continue;
                }

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                var materials = new List<string>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    var material = renderers[i].sharedMaterial;
                    if (material != null)
                        materials.Add(material.name + "(" + material.shader.name + ")");
                }

                Debug.Log("[Probe] " + paths[p] +
                          " | size=" + bounds.size.ToString("F3") +
                          " center=" + bounds.center.ToString("F3") +
                          " renderers=" + renderers.Length +
                          " scale=" + instance.transform.localScale.ToString("F3") +
                          " mats=" + string.Join(", ", materials));

                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
    }
}
