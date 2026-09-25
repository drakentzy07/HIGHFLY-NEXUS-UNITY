#if UNITY_EDITOR
using System;
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
    /// Builds the four W1 interiors by instancing audited RG Poly structures.
    /// No player/camera/combat authority is created in these scenes.
    /// </summary>
    public static class HighflyWorldW0W1Builder
    {
        private const string CatalogAssetPath =
            "Assets/Highfly/Resources/World/W0W1Catalog.json";

        private const string InteriorRoot =
            "Assets/Highfly/Scenes/Interiors";

        private static readonly Vector3 RemoteEntry =
            new Vector3(1000f, 1.1f, 1000f);

        public static void BuildOrRefreshRgPoly(string hostScenePath)
        {
            Directory.CreateDirectory(InteriorRoot);

            Scene host = SceneManager.GetSceneByPath(hostScenePath);
            if (!host.IsValid() || !host.isLoaded)
                host = EditorSceneManager.OpenScene(hostScenePath, OpenSceneMode.Single);

            TextAsset catalogAsset =
                AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogAssetPath);

            if (catalogAsset == null)
                throw new FileNotFoundException(
                    "HIGHFLY W0/W1 catalog missing",
                    CatalogAssetPath);

            HighflyWorldCatalogData catalog =
                JsonUtility.FromJson<HighflyWorldCatalogData>(catalogAsset.text);

            if (catalog == null || catalog.interiors == null)
                throw new InvalidOperationException(
                    "HIGHFLY W0/W1 catalog has no interiors.");

            BuildOne(
                host, FindInterior(catalog, "interior.capital.guild"),
                new[] { "House 7" },
                "Door_1 Variant (3)",
                null);

            BuildOne(
                host, FindInterior(catalog, "interior.capital.inn"),
                new[] { "Inn" },
                "Door_1 Variant",
                "Table_1 Variant");

            BuildOne(
                host, FindInterior(catalog, "interior.capital.forge"),
                new[] { "Smithy" },
                "Building_Addon_6 Variant (1)",
                "Grinder Variant");

            BuildOne(
                host, FindInterior(catalog, "interior.capital.market"),
                new[] { "Market Stall 1", "Market Stall 2", "Market Stall 3" },
                "Market_Table_2 Variant",
                "Market_Table_2 Variant");

            SceneManager.SetActiveScene(host);
            EditorBuildSettings.scenes = GetBuildSceneSettings(hostScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "HIGHFLY W0/W1 RG POLY | 4 additive interiors generated " +
                "from audited CITY01 structures.");
        }

        public static string[] GetBuildScenes(string hostScenePath)
        {
            EditorBuildSettingsScene[] settings =
                GetBuildSceneSettings(hostScenePath);

            EditorBuildSettings.scenes = settings;

            string[] paths = new string[settings.Length];
            for (int i = 0; i < settings.Length; i++)
                paths[i] = settings[i].path;

            return paths;
        }

        private static EditorBuildSettingsScene[] GetBuildSceneSettings(
            string hostScenePath)
        {
            List<EditorBuildSettingsScene> result =
                new List<EditorBuildSettingsScene>();

            result.Add(new EditorBuildSettingsScene(hostScenePath, true));

            TextAsset catalogAsset =
                AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogAssetPath);

            if (catalogAsset == null)
                return result.ToArray();

            HighflyWorldCatalogData catalog =
                JsonUtility.FromJson<HighflyWorldCatalogData>(catalogAsset.text);

            if (catalog == null || catalog.interiors == null)
                return result.ToArray();

            for (int i = 0; i < catalog.interiors.Length; i++)
            {
                HighflyInteriorDefinition definition = catalog.interiors[i];
                if (definition == null || string.IsNullOrEmpty(definition.sceneName))
                    continue;

                string path = InteriorScenePath(definition);
                if (File.Exists(path))
                    result.Add(new EditorBuildSettingsScene(path, true));
            }

            return result.ToArray();
        }

        private static HighflyInteriorDefinition FindInterior(
            HighflyWorldCatalogData catalog,
            string id)
        {
            if (catalog == null || catalog.interiors == null)
                throw new InvalidOperationException("HIGHFLY W1 catalog missing.");

            for (int i = 0; i < catalog.interiors.Length; i++)
            {
                HighflyInteriorDefinition definition = catalog.interiors[i];
                if (definition != null &&
                    string.Equals(definition.id, id, StringComparison.OrdinalIgnoreCase))
                    return definition;
            }

            throw new InvalidOperationException(
                "HIGHFLY W1 interior not found: " + id);
        }

        private static void BuildOne(
            Scene host,
            HighflyInteriorDefinition definition,
            string[] sourceRootNames,
            string entryDescendantName,
            string serviceDescendantName)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            Scene interior =
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Additive);

            GameObject interiorRoot = new GameObject(
                "HF_W1_RGPOLY_" + definition.layoutProfile.ToUpperInvariant());

            SceneManager.MoveGameObjectToScene(interiorRoot, interior);

            List<GameObject> clones = new List<GameObject>();
            Transform entryReference = null;
            Transform serviceReference = null;

            for (int i = 0; i < sourceRootNames.Length; i++)
            {
                GameObject source =
                    HighflyRgPolyW1Layout.FindRoot(host, sourceRootNames[i]);

                if (source == null)
                    throw new InvalidOperationException(
                        "HIGHFLY W1 source root missing: " + sourceRootNames[i]);

                GameObject clone =
                    UnityEngine.Object.Instantiate(source);

                clone.name = source.name;
                SceneManager.MoveGameObjectToScene(clone, interior);
                clone.transform.SetParent(interiorRoot.transform, true);

                RemoveForeignRuntimeScripts(clone);
                clones.Add(clone);

                if (entryReference == null)
                    entryReference =
                        HighflyRgPolyW1Layout.FindDescendant(
                            clone.transform,
                            entryDescendantName);

                if (serviceReference == null &&
                    !string.IsNullOrEmpty(serviceDescendantName))
                {
                    serviceReference =
                        HighflyRgPolyW1Layout.FindDescendant(
                            clone.transform,
                            serviceDescendantName);
                }
            }

            if (entryReference == null)
                throw new InvalidOperationException(
                    "HIGHFLY W1 entry reference missing for " + definition.id +
                    ": " + entryDescendantName);

            Vector3 delta = RemoteEntry - entryReference.position;
            interiorRoot.transform.position += delta;

            // The entry reference moves with the root.
            DisableBlockingDoorCollider(entryReference);

            AddSafetyFloor(interiorRoot.transform, clones);
            AddInteriorLighting(interiorRoot.transform);

            Vector3 entryPosition =
                entryReference.position +
                entryReference.forward * 1.35f +
                Vector3.up * 0.15f;

            GameObject entry = new GameObject("ENTRY_ANCHOR");
            SceneManager.MoveGameObjectToScene(entry, interior);
            entry.transform.SetParent(interiorRoot.transform, true);
            entry.transform.position = entryPosition;
            entry.transform.rotation = entryReference.rotation;

            HighflyWorldAnchor entryAnchor =
                entry.AddComponent<HighflyWorldAnchor>();

            entryAnchor.Configure(
                definition.entryAnchorId,
                definition.displayName + " entry",
                "interior_entry");

            GameObject exit = new GameObject("EXIT_TO_CAPITAL");
            SceneManager.MoveGameObjectToScene(exit, interior);
            exit.transform.SetParent(interiorRoot.transform, true);
            exit.transform.position =
                entryReference.position + Vector3.up * 0.1f;
            exit.transform.rotation = entryReference.rotation;

            BoxCollider exitCollider = exit.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            exitCollider.center = new Vector3(0f, 1.05f, 0f);
            exitCollider.size = new Vector3(3.0f, 2.4f, 2.5f);

            HighflyWorldAnchor exitAnchor =
                exit.AddComponent<HighflyWorldAnchor>();

            exitAnchor.Configure(
                definition.exitAnchorId,
                definition.displayName + " exit",
                "interior_exit");

            HighflyWorldExitDoor exitDoor =
                exit.AddComponent<HighflyWorldExitDoor>();

            exitDoor.Configure("CAPITAL HIGHFLY");

            CreateService(
                interior,
                interiorRoot.transform,
                definition,
                entryReference,
                serviceReference);

            string path = InteriorScenePath(definition);
            EditorSceneManager.SaveScene(interior, path);
            EditorSceneManager.CloseScene(interior, true);

            Debug.Log(
                "HIGHFLY W1 interior built | " + definition.id +
                " | source=" + string.Join(",", sourceRootNames) +
                " | scene=" + path);
        }

        private static void CreateService(
            Scene scene,
            Transform parent,
            HighflyInteriorDefinition definition,
            Transform entryReference,
            Transform serviceReference)
        {
            Vector3 position;

            if (serviceReference != null)
            {
                position = serviceReference.position + Vector3.up * 0.35f;
            }
            else
            {
                position =
                    entryReference.position +
                    entryReference.forward * 3.4f +
                    Vector3.up * 0.35f;
            }

            GameObject station = new GameObject(
                "SERVICE_" + definition.serviceRef);

            SceneManager.MoveGameObjectToScene(station, scene);
            station.transform.SetParent(parent, true);
            station.transform.position = position;
            station.transform.rotation = entryReference.rotation;

            BoxCollider collider = station.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 0.8f, 0f);
            collider.size = new Vector3(3.0f, 2.0f, 3.0f);

            HighflyW1ServiceStation service =
                station.AddComponent<HighflyW1ServiceStation>();

            string profile =
                (definition.layoutProfile ?? string.Empty).ToLowerInvariant();

            if (profile == "guild")
                service.Configure(
                    definition.serviceRef,
                    "TABLÓN DEL GREMIO",
                    "REVISAR");
            else if (profile == "inn")
                service.Configure(
                    definition.serviceRef,
                    "MOSTRADOR DE POSADA",
                    "DESCANSAR");
            else if (profile == "forge")
                service.Configure(
                    definition.serviceRef,
                    "ESTACIÓN DE FORJA",
                    "USAR");
            else if (profile == "market")
                service.Configure(
                    definition.serviceRef,
                    "PUESTO DE MERCADO",
                    "REVISAR");
            else
                service.Configure(
                    definition.serviceRef,
                    definition.displayName,
                    "USAR");
        }

        private static void DisableBlockingDoorCollider(Transform reference)
        {
            if (reference == null)
                return;

            Collider[] colliders =
                reference.GetComponentsInChildren<Collider>(true);

            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }

        private static void AddSafetyFloor(
            Transform parent,
            List<GameObject> clones)
        {
            bool hasBounds = false;
            Bounds bounds = default(Bounds);

            for (int i = 0; i < clones.Count; i++)
            {
                Renderer[] renderers =
                    clones[i].GetComponentsInChildren<Renderer>(true);

                for (int r = 0; r < renderers.Length; r++)
                {
                    Renderer renderer = renderers[r];
                    if (renderer == null)
                        continue;

                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            GameObject floor =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            floor.name = "HF_INTERIOR_SAFETY_FLOOR";
            floor.transform.SetParent(parent, true);

            Vector3 center = hasBounds
                ? bounds.center
                : RemoteEntry;

            float sizeX = hasBounds
                ? Mathf.Max(22f, bounds.size.x + 10f)
                : 28f;

            float sizeZ = hasBounds
                ? Mathf.Max(22f, bounds.size.z + 10f)
                : 28f;

            float y = hasBounds
                ? bounds.min.y - 0.18f
                : RemoteEntry.y - 0.2f;

            floor.transform.position =
                new Vector3(center.x, y, center.z);

            floor.transform.localScale =
                new Vector3(sizeX, 0.35f, sizeZ);

            Renderer rendererComponent =
                floor.GetComponent<Renderer>();

            if (rendererComponent != null)
                rendererComponent.enabled = false;
        }

        private static void AddInteriorLighting(Transform parent)
        {
            GameObject key = new GameObject("HF_INTERIOR_KEY_LIGHT");
            key.transform.SetParent(parent, false);
            key.transform.localPosition = new Vector3(0f, 8f, 0f);

            Light light = key.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 45f;
            light.intensity = 1.4f;
            light.shadows = LightShadows.None;
        }

        private static void RemoveForeignRuntimeScripts(GameObject root)
        {
            MonoBehaviour[] behaviours =
                root.GetComponentsInChildren<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                Type type = behaviour.GetType();
                string ns = type.Namespace ?? string.Empty;

                if (ns.StartsWith("Highfly", StringComparison.Ordinal))
                    continue;

                UnityEngine.Object.DestroyImmediate(behaviour);
            }
        }

        private static string InteriorScenePath(
            HighflyInteriorDefinition definition)
        {
            return InteriorRoot + "/" + definition.sceneName + ".unity";
        }
    }
}
#endif
