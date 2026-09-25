#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Highfly.World;

namespace Highfly.Editor
{
    /// <summary>
    /// Generates the four W1 interiors as independent Unity scenes.
    /// The scenes contain environment/service endpoints only: never player,
    /// camera, joystick, targeting or combat authority.
    /// </summary>
    public static class HighflyWorldW0W1Builder
    {
        private const string CatalogAssetPath = "Assets/Highfly/Resources/World/W0W1Catalog.json";
        private const string InteriorRoot = "Assets/Highfly/Scenes/Interiors";
        private static readonly Vector3 RemoteOrigin = new Vector3(1000f, 0f, 1000f);

        public static void BuildOrRefresh()
        {
            Directory.CreateDirectory(InteriorRoot);

            TextAsset catalogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogAssetPath);
            if (catalogAsset == null)
                throw new FileNotFoundException("HIGHFLY W0/W1 catalog missing", CatalogAssetPath);

            HighflyWorldCatalogData catalog = JsonUtility.FromJson<HighflyWorldCatalogData>(catalogAsset.text);
            if (catalog == null || catalog.interiors == null || catalog.interiors.Length == 0)
                throw new System.InvalidOperationException("HIGHFLY W0/W1 catalog has no interiors.");

            for (int i = 0; i < catalog.interiors.Length; i++)
                BuildInterior(catalog.interiors[i]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HIGHFLY W0/W1: generated " + catalog.interiors.Length + " independent interior scenes.");
        }

        public static string[] GetBuildScenes(string hostScenePath)
        {
            List<string> scenes = new List<string>();
            scenes.Add(hostScenePath);

            TextAsset catalogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogAssetPath);
            if (catalogAsset == null)
                return scenes.ToArray();

            HighflyWorldCatalogData catalog = JsonUtility.FromJson<HighflyWorldCatalogData>(catalogAsset.text);
            if (catalog == null || catalog.interiors == null)
                return scenes.ToArray();

            for (int i = 0; i < catalog.interiors.Length; i++)
            {
                string path = InteriorScenePath(catalog.interiors[i]);
                if (File.Exists(path))
                    scenes.Add(path);
            }

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenes.Count];
            for (int i = 0; i < scenes.Count; i++)
                buildScenes[i] = new EditorBuildSettingsScene(scenes[i], true);
            EditorBuildSettings.scenes = buildScenes;

            return scenes.ToArray();
        }

        private static void BuildInterior(HighflyInteriorDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.sceneName))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = definition.sceneName;

            GameObject root = new GameObject("HF_W1_INTERIOR_" + definition.layoutProfile.ToUpperInvariant());
            root.transform.position = RemoteOrigin;

            BuildShell(root.transform);
            BuildLighting(root.transform);

            GameObject entry = new GameObject("ENTRY_ANCHOR");
            entry.transform.SetParent(root.transform, false);
            entry.transform.localPosition = new Vector3(0f, 0.35f, -4.2f);
            entry.transform.localRotation = Quaternion.identity;
            HighflyWorldAnchor entryAnchor = entry.AddComponent<HighflyWorldAnchor>();
            entryAnchor.Configure(definition.entryAnchorId, definition.displayName + " entry", "interior_entry");

            GameObject exit = new GameObject("EXIT_TO_CAPITAL");
            exit.transform.SetParent(root.transform, false);
            exit.transform.localPosition = new Vector3(0f, 1.15f, -5.15f);
            BoxCollider exitCollider = exit.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector3(2.8f, 2.4f, 1.5f);
            HighflyWorldAnchor exitAnchor = exit.AddComponent<HighflyWorldAnchor>();
            exitAnchor.Configure(definition.exitAnchorId, definition.displayName + " exit", "interior_exit");
            HighflyWorldExitDoor exitDoor = exit.AddComponent<HighflyWorldExitDoor>();
            exitDoor.Configure("CAPITAL HIGHFLY");

            BuildService(root.transform, definition);
            BuildProfileProps(root.transform, definition.layoutProfile);

            string path = InteriorScenePath(definition);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void BuildShell(Transform root)
        {
            CreateCube(root, "FLOOR", new Vector3(0f, -0.15f, 0f), new Vector3(16f, 0.3f, 12f));

            CreateCube(root, "WALL_NORTH", new Vector3(0f, 2.5f, 5.75f), new Vector3(16f, 5f, 0.5f));
            CreateCube(root, "WALL_WEST", new Vector3(-7.75f, 2.5f, 0f), new Vector3(0.5f, 5f, 12f));
            CreateCube(root, "WALL_EAST", new Vector3(7.75f, 2.5f, 0f), new Vector3(0.5f, 5f, 12f));

            // South wall leaves a readable central doorway.
            CreateCube(root, "WALL_SOUTH_L", new Vector3(-5f, 2.5f, -5.75f), new Vector3(6f, 5f, 0.5f));
            CreateCube(root, "WALL_SOUTH_R", new Vector3(5f, 2.5f, -5.75f), new Vector3(6f, 5f, 0.5f));

            TryPlaceVillage(root, "Wall_Plaster_Door_Round", new Vector3(0f, 0f, -5.55f), 0f);
        }

        private static void BuildLighting(Transform root)
        {
            GameObject lightGo = new GameObject("Interior_KeyLight");
            lightGo.transform.SetParent(root, false);
            lightGo.transform.localPosition = new Vector3(0f, 4.2f, 0f);
            lightGo.transform.localRotation = Quaternion.Euler(70f, 20f, 0f);

            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 22f;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
        }

        private static void BuildService(Transform root, HighflyInteriorDefinition definition)
        {
            GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cube);
            station.name = "SERVICE_" + definition.serviceRef;
            station.transform.SetParent(root, false);
            station.transform.localPosition = new Vector3(0f, 0.65f, 2.6f);
            station.transform.localScale = new Vector3(3.2f, 1.3f, 1.1f);

            BoxCollider collider = station.GetComponent<BoxCollider>();
            collider.isTrigger = true;

            HighflyW1ServiceStation service = station.AddComponent<HighflyW1ServiceStation>();

            string profile = (definition.layoutProfile ?? "").ToLowerInvariant();
            if (profile == "guild")
                service.Configure(definition.serviceRef, "TABLÓN DEL GREMIO", "REVISAR");
            else if (profile == "inn")
                service.Configure(definition.serviceRef, "MOSTRADOR DE POSADA", "DESCANSAR");
            else if (profile == "forge")
                service.Configure(definition.serviceRef, "ESTACIÓN DE FORJA", "USAR");
            else if (profile == "market")
                service.Configure(definition.serviceRef, "PUESTO DE MERCADO", "REVISAR");
            else
                service.Configure(definition.serviceRef, definition.displayName, "USAR");
        }

        private static void BuildProfileProps(Transform root, string rawProfile)
        {
            string profile = (rawProfile ?? "").ToLowerInvariant();

            if (profile == "forge")
            {
                TryPlaceProp(root, "Anvil", new Vector3(-3.2f, 0f, 1.5f), 20f);
                TryPlaceProp(root, "Workbench", new Vector3(3.0f, 0f, 1.7f), -12f);
                TryPlaceProp(root, "WeaponStand", new Vector3(5.5f, 0f, 3.6f), 0f);
            }
            else if (profile == "market")
            {
                TryPlaceProp(root, "Stall_Empty", new Vector3(-4.2f, 0f, 2.4f), 180f);
                TryPlaceProp(root, "Stall_Cart_Empty", new Vector3(4.1f, 0f, 2.4f), 180f);
                TryPlaceProp(root, "FarmCrate_Apple", new Vector3(-4.3f, 0f, 0.2f), 0f);
                TryPlaceProp(root, "FarmCrate_Carrot", new Vector3(4.3f, 0f, 0.2f), 0f);
            }
            else if (profile == "guild")
            {
                TryPlaceProp(root, "Chest_Wood", new Vector3(-4.8f, 0f, 3.2f), 90f);
                TryPlaceProp(root, "WeaponStand", new Vector3(4.8f, 0f, 3.2f), -90f);
                TryPlaceProp(root, "Coin_Pile", new Vector3(0.8f, 0f, 2.0f), 0f);
            }
            else if (profile == "inn")
            {
                TryPlaceProp(root, "Barrel", new Vector3(-5.0f, 0f, 3.0f), 0f);
                TryPlaceProp(root, "Barrel", new Vector3(-4.0f, 0f, 3.2f), 0f);
                TryPlaceProp(root, "Chest_Wood", new Vector3(4.8f, 0f, 3.2f), -90f);
                CreateCube(root, "TABLE_A", new Vector3(-2.8f, 0.6f, 0f), new Vector3(2.6f, 1.2f, 1.6f));
                CreateCube(root, "TABLE_B", new Vector3(2.8f, 0.6f, 0f), new Vector3(2.6f, 1.2f, 1.6f));
            }
        }

        private static void TryPlaceVillage(Transform parent, string assetName, Vector3 localPosition, float yaw)
        {
            TryPlaceResource(parent, "WorldFinal/Village/FBX/" + assetName, assetName, localPosition, yaw);
        }

        private static void TryPlaceProp(Transform parent, string assetName, Vector3 localPosition, float yaw)
        {
            TryPlaceResource(parent, "WorldFinal/Props/FBX/" + assetName, assetName, localPosition, yaw);
        }

        private static void TryPlaceResource(
            Transform parent,
            string resourcePath,
            string objectName,
            Vector3 localPosition,
            float yaw)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
                return;

            GameObject go = Object.Instantiate(prefab);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private static GameObject CreateCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            return go;
        }

        private static string InteriorScenePath(HighflyInteriorDefinition definition)
        {
            return InteriorRoot + "/" + definition.sceneName + ".unity";
        }
    }
}
#endif
