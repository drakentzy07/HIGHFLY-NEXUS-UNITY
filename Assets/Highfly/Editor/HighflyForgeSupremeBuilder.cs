#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Highfly.World;

namespace Highfly.Editor
{
    /// <summary>
    /// W1.1 purpose-built forge interior.
    /// Uses approved CC0 art donors for the shell/props while HIGHFLY owns
    /// scene topology, interaction, services and all runtime authority.
    /// </summary>
    public static class HighflyForgeSupremeBuilder
    {
        private const string VillageRoot =
            "Assets/External/Quaternius/MedievalVillageMegaKit";

        private const string PropsRoot =
            "Assets/External/Quaternius/FantasyPropsMegaKit";

        private const string RgPolyRoot =
            "Assets/Stylized Medieval Kingdom URP";

        private const string KayKitBarbarian =
            "Assets/External/KayKit/Adventurers/addons/" +
            "kaykit_character_pack_adventures/Characters/fbx/Barbarian.fbx";

        private const string DiagnosticsDirectory = "build/diagnostics";

        private static readonly Vector3 RemoteOrigin =
            new Vector3(1000f, 0f, 1000f);

        private sealed class AssetLog
        {
            public readonly List<string> Lines = new List<string>();

            public void Add(string role, string path)
            {
                Lines.Add(role + "\t" + (path ?? "MISSING"));
            }
        }

        public static void Build(HighflyInteriorDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");

            Directory.CreateDirectory("Assets/Highfly/Scenes/Interiors");
            Directory.CreateDirectory(DiagnosticsDirectory);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive);

            GameObject root = new GameObject("HF_W1_FORGE_SUPREME");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = RemoteOrigin;

            AssetLog log = new AssetLog();

            BuildShell(root.transform, log);
            BuildForgeSet(root.transform, log);
            BuildBlacksmith(root.transform, log);
            BuildLighting(root.transform);
            BuildEntryExit(root.transform, definition);

            string scenePath =
                "Assets/Highfly/Scenes/Interiors/" +
                definition.sceneName + ".unity";

            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.CloseScene(scene, true);

            File.WriteAllLines(
                Path.Combine(
                    DiagnosticsDirectory,
                    "w1-1-forge-supreme-assets.tsv"),
                new[] { "role\tasset_path" }.Concat(log.Lines).ToArray());

            Debug.Log(
                "HIGHFLY W1.1 FORGE SUPREME built | scene=" + scenePath +
                " | donor assets=" + log.Lines.Count);
        }

        private static void BuildShell(Transform parent, AssetLog log)
        {
            Material floorMat = GetOrCreateHighflyMaterial(
                "HF_ForgeFloor",
                new Color(0.16f, 0.11f, 0.075f, 1f));

            Material wallMat = GetOrCreateHighflyMaterial(
                "HF_ForgeWall",
                new Color(0.34f, 0.28f, 0.20f, 1f));

            Material beamMat = GetOrCreateHighflyMaterial(
                "HF_ForgeBeam",
                new Color(0.11f, 0.07f, 0.045f, 1f));

            Material roofMat = GetOrCreateHighflyMaterial(
                "HF_ForgeRoof",
                new Color(0.095f, 0.085f, 0.075f, 1f));

            CreateBox(
                parent, "FORGE_FLOOR",
                new Vector3(0f, -0.14f, 0f),
                new Vector3(15f, 0.28f, 11.5f),
                floorMat, true);

            CreateBox(
                parent, "FORGE_WALL_SOUTH_L",
                new Vector3(-5f, 2.25f, -5.6f),
                new Vector3(5f, 4.5f, 0.35f),
                wallMat, true);

            CreateBox(
                parent, "FORGE_WALL_SOUTH_R",
                new Vector3(5f, 2.25f, -5.6f),
                new Vector3(5f, 4.5f, 0.35f),
                wallMat, true);

            CreateBox(
                parent, "FORGE_WALL_NORTH",
                new Vector3(0f, 2.25f, 5.6f),
                new Vector3(15f, 4.5f, 0.35f),
                wallMat, true);

            CreateBox(
                parent, "FORGE_WALL_WEST",
                new Vector3(-7.35f, 2.25f, 0f),
                new Vector3(0.35f, 4.5f, 11.5f),
                wallMat, true);

            CreateBox(
                parent, "FORGE_WALL_EAST",
                new Vector3(7.35f, 2.25f, 0f),
                new Vector3(0.35f, 4.5f, 11.5f),
                wallMat, true);

            // Heavy timber frame keeps the room visually medieval and avoids
            // importing another full structural pack.
            for (int i = -2; i <= 2; i++)
            {
                CreateBox(
                    parent,
                    "FORGE_BEAM_" + i,
                    new Vector3(i * 3.4f, 3.7f, 0f),
                    new Vector3(0.25f, 0.25f, 11.2f),
                    beamMat,
                    false);
            }

            CreateBox(
                parent, "FORGE_CEILING",
                new Vector3(0f, 4.55f, 0f),
                new Vector3(15f, 0.22f, 11.5f),
                roofMat, false);

            string door = FindAnyModel(
                RgPolyRoot,
                new[] { "door" },
                false);
            log.Add("shell.rgpoly_door", door);

            string window = FindAnyModel(
                RgPolyRoot,
                new[] { "window" },
                false);
            log.Add("shell.rgpoly_window", window);

            if (!string.IsNullOrEmpty(door))
            {
                PlaceDecor(
                    door,
                    parent,
                    "FORGE_ENTRY_DOOR_VISUAL",
                    new Vector3(0f, 0f, -5.45f),
                    new Vector3(0f, 180f, 0f),
                    2.8f,
                    false);
            }

            if (!string.IsNullOrEmpty(window))
            {
                PlaceDecor(
                    window,
                    parent,
                    "FORGE_WINDOW_NORTH_A",
                    new Vector3(-4.1f, 1.35f, 5.4f),
                    Vector3.zero,
                    1.9f,
                    false);

                PlaceDecor(
                    window,
                    parent,
                    "FORGE_WINDOW_NORTH_B",
                    new Vector3(4.1f, 1.35f, 5.4f),
                    Vector3.zero,
                    1.9f,
                    false);
            }

            log.Add("shell.structure", "HIGHFLY generated + RG Poly donor detail");
        }

        private static void BuildForgeSet(Transform parent, AssetLog log)
        {
            string anvil = FindAnyModel(
                PropsRoot,
                new[] { "anvil" },
                true);
            log.Add("forge.anvil", anvil);

            string furnace = FindAnyModel(
                PropsRoot,
                new[] { "furnace", "smelter", "forge", "kiln" },
                false);

            if (string.IsNullOrEmpty(furnace))
            {
                furnace = FindAnyModel(
                    RgPolyRoot,
                    new[] { "smithy", "cimey", "chimney" },
                    true);
            }
            log.Add("forge.furnace", furnace);

            string hammer = FindAnyModel(
                PropsRoot,
                new[] { "hammer", "mallet" },
                false);
            log.Add("forge.hammer", hammer);

            string workbench = FindAnyModel(
                PropsRoot,
                new[] { "workbench", "work_bench", "work bench", "craftingtable", "crafting_table" },
                false);

            if (string.IsNullOrEmpty(workbench))
            {
                workbench = FindAnyModel(
                    PropsRoot,
                    new[] { "table" },
                    true);
            }
            log.Add("forge.workbench", workbench);

            string rack = FindAnyModel(
                PropsRoot,
                new[] { "weaponrack", "weapon_rack", "rack", "display" },
                false);
            log.Add("forge.weapon_display", rack);

            string chest = FindAnyModel(
                RgPolyRoot,
                new[] { "chest" },
                false);
            log.Add("forge.storage_chest", chest);

            string barrel = FindAnyModel(
                RgPolyRoot,
                new[] { "barrel" },
                false);
            log.Add("forge.barrel", barrel);

            string crate = FindAnyModel(
                RgPolyRoot,
                new[] { "crate" },
                false);
            log.Add("forge.crate", crate);

            string sword = FindAnyModel(
                PropsRoot,
                new[] { "sword" },
                false);
            log.Add("forge.weapon.sword", sword);

            string axe = FindAnyModel(
                PropsRoot,
                new[] { "axe" },
                false);
            log.Add("forge.weapon.axe", axe);

            GameObject anvilGo = PlaceDecor(
                anvil,
                parent,
                "FORGE_ANVIL",
                new Vector3(-1.8f, 0f, 0.1f),
                new Vector3(0f, -12f, 0f),
                1.05f,
                true);
            ApplyMaterial(
                anvilGo,
                GetOrCreateHighflyMaterial(
                    "HF_ForgeMetal",
                    new Color(0.12f, 0.13f, 0.15f, 1f)));

            AddInteraction(
                parent,
                "STATION_ANVIL",
                anvilGo.transform.position,
                new Vector3(2.5f, 2.0f, 2.5f),
                HighflyForgeActionKind.Anvil,
                "YUNQUE",
                "FORJAR");

            GameObject furnaceGo = PlaceDecor(
                furnace,
                parent,
                "FORGE_FURNACE",
                new Vector3(-5.4f, 0f, 3.4f),
                new Vector3(0f, 135f, 0f),
                2.5f,
                true);

            AddInteraction(
                parent,
                "STATION_FURNACE",
                furnaceGo.transform.position + Vector3.forward * 0.4f,
                new Vector3(3.2f, 2.5f, 3.2f),
                HighflyForgeActionKind.Furnace,
                "HORNO DE FUNDICIÓN",
                "FUNDIR");

            GameObject benchGo = PlaceDecor(
                workbench,
                parent,
                "FORGE_WORKBENCH",
                new Vector3(4.3f, 0f, 3.1f),
                new Vector3(0f, -145f, 0f),
                1.5f,
                true);
            ApplyMaterial(
                benchGo,
                GetOrCreateHighflyMaterial(
                    "HF_ForgeWood",
                    new Color(0.20f, 0.12f, 0.07f, 1f)));

            AddInteraction(
                parent,
                "STATION_WORKBENCH",
                benchGo.transform.position,
                new Vector3(3.1f, 2.0f, 2.5f),
                HighflyForgeActionKind.Workbench,
                "BANCO DE TRABAJO",
                "CRAFT");

            if (!string.IsNullOrEmpty(hammer))
            {
                PlaceDecor(
                    hammer,
                    parent,
                    "FORGE_HAMMER",
                    new Vector3(-0.7f, 1.0f, 0.1f),
                    new Vector3(8f, 20f, 72f),
                    0.6f,
                    false);
            }

            if (!string.IsNullOrEmpty(rack))
            {
                GameObject rackGo = PlaceDecor(
                    rack,
                    parent,
                    "FORGE_WEAPON_RACK",
                    new Vector3(5.6f, 0f, -2.8f),
                    new Vector3(0f, -90f, 0f),
                    2.0f,
                    true);

                AddInteraction(
                    parent,
                    "STATION_WEAPON_DISPLAY",
                    rackGo.transform.position,
                    new Vector3(3.2f, 2.4f, 2.4f),
                    HighflyForgeActionKind.WeaponDisplay,
                    "EXHIBIDOR DE ARMAS",
                    "INSPECCIONAR");
            }
            else
            {
                GameObject display = new GameObject("FORGE_WEAPON_DISPLAY");
                display.transform.SetParent(parent, false);
                display.transform.localPosition = new Vector3(5.6f, 0.9f, -2.8f);

                if (!string.IsNullOrEmpty(sword))
                {
                    PlaceDecor(
                        sword,
                        display.transform,
                        "DISPLAY_SWORD",
                        Vector3.zero,
                        new Vector3(0f, 0f, 88f),
                        1.2f,
                        false);
                }

                if (!string.IsNullOrEmpty(axe))
                {
                    PlaceDecor(
                        axe,
                        display.transform,
                        "DISPLAY_AXE",
                        new Vector3(0f, 0f, 1.0f),
                        new Vector3(0f, 0f, 80f),
                        1.2f,
                        false);
                }

                AddInteraction(
                    parent,
                    "STATION_WEAPON_DISPLAY",
                    parent.TransformPoint(new Vector3(5.6f, 1f, -2.8f)),
                    new Vector3(3.2f, 2.4f, 2.4f),
                    HighflyForgeActionKind.WeaponDisplay,
                    "EXHIBIDOR DE ARMAS",
                    "INSPECCIONAR");
            }

            Vector3 storagePosition = new Vector3(-5.3f, 0f, -2.6f);

            if (!string.IsNullOrEmpty(chest))
            {
                PlaceDecor(
                    chest,
                    parent,
                    "FORGE_STORAGE_CHEST",
                    storagePosition,
                    new Vector3(0f, 40f, 0f),
                    1.2f,
                    true);
            }

            if (!string.IsNullOrEmpty(barrel))
            {
                PlaceDecor(
                    barrel,
                    parent,
                    "FORGE_BARREL_A",
                    storagePosition + new Vector3(1.1f, 0f, 0.2f),
                    Vector3.zero,
                    1.2f,
                    true);
            }

            if (!string.IsNullOrEmpty(crate))
            {
                PlaceDecor(
                    crate,
                    parent,
                    "FORGE_CRATE_A",
                    storagePosition + new Vector3(0.4f, 0f, 1.2f),
                    new Vector3(0f, 22f, 0f),
                    1.0f,
                    true);
            }

            AddInteraction(
                parent,
                "STATION_MATERIAL_STORAGE",
                parent.TransformPoint(storagePosition + new Vector3(0.5f, 1f, 0.7f)),
                new Vector3(3.5f, 2.4f, 3.5f),
                HighflyForgeActionKind.MaterialStorage,
                "ALMACÉN DE MATERIALES",
                "REVISAR");
        }

        private static void BuildBlacksmith(Transform parent, AssetLog log)
        {
            GameObject source =
                AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBarbarian);

            if (source == null)
                throw new FileNotFoundException(
                    "HIGHFLY W1.1 blacksmith visual missing",
                    KayKitBarbarian);

            GameObject npc =
                UnityEngine.Object.Instantiate(source);

            npc.name = "HERRERO_KAYKIT";
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = new Vector3(0.8f, 0f, 3.7f);
            npc.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            NormalizeHeight(npc, 1.82f);
            MoveBottomToLocalY(npc, parent, 0f);

            CapsuleCollider trigger = npc.AddComponent<CapsuleCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.95f, 0f);
            trigger.height = 2.1f;
            trigger.radius = 0.75f;

            HighflyForgeInteractable interactable =
                npc.AddComponent<HighflyForgeInteractable>();

            interactable.Configure(
                HighflyForgeActionKind.Blacksmith,
                "HERRERO",
                "HABLAR");

            log.Add("forge.blacksmith_npc", KayKitBarbarian);
        }

        private static void BuildEntryExit(
            Transform parent,
            HighflyInteriorDefinition definition)
        {
            GameObject entry = new GameObject("ENTRY_ANCHOR");
            entry.transform.SetParent(parent, false);
            entry.transform.localPosition = new Vector3(0f, 0.1f, -3.8f);
            entry.transform.localRotation = Quaternion.identity;

            HighflyWorldAnchor entryAnchor =
                entry.AddComponent<HighflyWorldAnchor>();

            entryAnchor.Configure(
                definition.entryAnchorId,
                definition.displayName + " entry",
                "interior_entry");

            GameObject exit = new GameObject("EXIT_TO_CAPITAL");
            exit.transform.SetParent(parent, false);
            exit.transform.localPosition = new Vector3(0f, 0f, -5.15f);
            exit.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            BoxCollider exitCollider = exit.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            exitCollider.center = new Vector3(0f, 1.05f, 0f);
            exitCollider.size = new Vector3(3.5f, 2.5f, 2.0f);

            HighflyWorldAnchor exitAnchor =
                exit.AddComponent<HighflyWorldAnchor>();

            exitAnchor.Configure(
                definition.exitAnchorId,
                definition.displayName + " exit",
                "interior_exit");

            HighflyWorldExitDoor exitDoor =
                exit.AddComponent<HighflyWorldExitDoor>();

            exitDoor.Configure("CAPITAL HIGHFLY");
        }

        private static void BuildLighting(Transform parent)
        {
            CreateLight(
                parent,
                "FORGE_LIGHT_MAIN",
                new Vector3(0f, 3.8f, 0f),
                new Color(1f, 0.77f, 0.54f, 1f),
                2.0f,
                12f);

            CreateLight(
                parent,
                "FORGE_LIGHT_FURNACE",
                new Vector3(-5.0f, 1.6f, 3.0f),
                new Color(1f, 0.30f, 0.08f, 1f),
                3.0f,
                7f);

            CreateLight(
                parent,
                "FORGE_LIGHT_ENTRY",
                new Vector3(0f, 2.8f, -3.8f),
                new Color(1f, 0.62f, 0.32f, 1f),
                1.35f,
                6f);
        }

        private static void CreateLight(
            Transform parent,
            string name,
            Vector3 localPosition,
            Color color,
            float intensity,
            float range)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static void AddInteraction(
            Transform parent,
            string name,
            Vector3 worldPosition,
            Vector3 size,
            HighflyForgeActionKind kind,
            string label,
            string prompt)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = worldPosition;

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, size.y * 0.45f, 0f);
            collider.size = size;

            HighflyForgeInteractable interactable =
                go.AddComponent<HighflyForgeInteractable>();

            interactable.Configure(kind, label, prompt);
        }

        private static GameObject CreateBox(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool colliderEnabled)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = colliderEnabled;

            return go;
        }

        private static Material GetOrCreateHighflyMaterial(
            string name,
            Color color)
        {
            string path =
                "Assets/Highfly/Generated/" + name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null)
                return material;

            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            material = new Material(shader);
            material.name = name;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.2f);

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ApplyMaterial(
            GameObject root,
            Material material)
        {
            if (root == null || material == null)
                return;

            Renderer[] renderers =
                root.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                    materials[m] = material;

                renderer.sharedMaterials = materials;
            }
        }

        private static GameObject PlaceFitted(
            string assetPath,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localEuler,
            Vector3 targetSize,
            bool addCollider)
        {
            GameObject wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);

            GameObject visual = InstantiateAsset(assetPath, wrapper.transform, name + "_VISUAL");

            Bounds initial = GetRendererBounds(visual);
            Vector3 safe = new Vector3(
                Mathf.Max(0.001f, initial.size.x),
                Mathf.Max(0.001f, initial.size.y),
                Mathf.Max(0.001f, initial.size.z));

            visual.transform.localScale = new Vector3(
                targetSize.x / safe.x,
                targetSize.y / safe.y,
                targetSize.z / safe.z);

            Bounds resized = GetRendererBounds(visual);
            visual.transform.position += wrapper.transform.position - resized.center;

            wrapper.transform.localPosition = localPosition;
            wrapper.transform.localRotation = Quaternion.Euler(localEuler);

            if (addCollider)
            {
                BoxCollider collider = wrapper.AddComponent<BoxCollider>();
                collider.center = Vector3.zero;
                collider.size = targetSize;
            }

            return wrapper;
        }

        private static GameObject PlaceDecor(
            string assetPath,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localEuler,
            float targetHeight,
            bool addCollider)
        {
            if (string.IsNullOrEmpty(assetPath))
                return null;

            GameObject wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);

            GameObject visual = InstantiateAsset(assetPath, wrapper.transform, name + "_VISUAL");
            NormalizeHeight(visual, targetHeight);

            Bounds resized = GetRendererBounds(visual);
            visual.transform.position += wrapper.transform.position - resized.center;
            visual.transform.localPosition += Vector3.up * (resized.extents.y);

            wrapper.transform.localPosition = localPosition;
            wrapper.transform.localRotation = Quaternion.Euler(localEuler);

            if (addCollider)
            {
                Bounds worldBounds = GetRendererBounds(wrapper);
                BoxCollider collider = wrapper.AddComponent<BoxCollider>();

                Vector3 localCenter =
                    wrapper.transform.InverseTransformPoint(worldBounds.center);

                Vector3 lossy = wrapper.transform.lossyScale;
                collider.center = localCenter;
                collider.size = new Vector3(
                    worldBounds.size.x / Mathf.Max(0.001f, Mathf.Abs(lossy.x)),
                    worldBounds.size.y / Mathf.Max(0.001f, Mathf.Abs(lossy.y)),
                    worldBounds.size.z / Mathf.Max(0.001f, Mathf.Abs(lossy.z)));
            }

            return wrapper;
        }

        private static GameObject InstantiateAsset(
            string assetPath,
            Transform parent,
            string name)
        {
            GameObject asset =
                AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (asset == null)
                throw new FileNotFoundException(
                    "HIGHFLY W1.1 donor model failed to load",
                    assetPath);

            GameObject go =
                UnityEngine.Object.Instantiate(asset);

            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            RemoveForeignRuntimeScripts(go);
            return go;
        }

        private static string FindAnyModel(
            string root,
            string[] anyTokens,
            bool required)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:Model",
                new[] { root });

            List<string> paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsFbx)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            int bestScore = -1;
            string best = null;

            for (int i = 0; i < paths.Count; i++)
            {
                string normalized = Normalize(Path.GetFileNameWithoutExtension(paths[i]));
                int score = 0;

                for (int t = 0; t < anyTokens.Length; t++)
                {
                    string token = Normalize(anyTokens[t]);
                    if (normalized.Contains(token))
                        score = Mathf.Max(score, token.Length * 10);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = paths[i];
                }
            }

            if (bestScore <= 0)
                best = null;

            if (required && string.IsNullOrEmpty(best))
            {
                throw new InvalidOperationException(
                    "HIGHFLY W1.1 required donor asset not found in " +
                    root + " | any=" + string.Join(",", anyTokens));
            }

            return best;
        }

        private static string FindModel(
            string root,
            string[] includeTokens,
            string[] excludeTokens,
            bool required)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:Model",
                new[] { root });

            List<string> paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsFbx)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            string best = null;
            int bestScore = -1;

            for (int i = 0; i < paths.Count; i++)
            {
                string normalized =
                    Normalize(Path.GetFileNameWithoutExtension(paths[i]));

                bool include = true;
                int score = 0;

                for (int t = 0; t < includeTokens.Length; t++)
                {
                    string token = Normalize(includeTokens[t]);
                    if (!normalized.Contains(token))
                    {
                        include = false;
                        break;
                    }

                    score += token.Length * 10;
                }

                if (!include)
                    continue;

                bool excluded = false;
                for (int t = 0; t < excludeTokens.Length; t++)
                {
                    if (normalized.Contains(Normalize(excludeTokens[t])))
                    {
                        excluded = true;
                        break;
                    }
                }

                if (excluded)
                    continue;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = paths[i];
                }
            }

            if (required && string.IsNullOrEmpty(best))
            {
                throw new InvalidOperationException(
                    "HIGHFLY W1.1 required structural donor missing in " +
                    root + " | include=" + string.Join(",", includeTokens));
            }

            return best;
        }

        private static bool IsFbx(string path)
        {
            return !string.IsNullOrEmpty(path) &&
                path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty)
                .ToLowerInvariant()
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty);
        }

        private static Bounds GetRendererBounds(GameObject root)
        {
            Renderer[] renderers =
                root.GetComponentsInChildren<Renderer>(true);

            bool hasBounds = false;
            Bounds bounds = new Bounds(root.transform.position, Vector3.one * 0.1f);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
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

            if (!hasBounds)
                throw new InvalidOperationException(
                    "HIGHFLY W1.1 donor object has no Renderer: " + root.name);

            return bounds;
        }

        private static void NormalizeHeight(GameObject root, float targetHeight)
        {
            Bounds bounds = GetRendererBounds(root);
            if (bounds.size.y < 0.001f)
                return;

            float scale = targetHeight / bounds.size.y;
            root.transform.localScale *= scale;
        }

        private static void MoveBottomToLocalY(
            GameObject go,
            Transform parent,
            float localY)
        {
            Bounds bounds = GetRendererBounds(go);
            float delta = parent.TransformPoint(new Vector3(0f, localY, 0f)).y -
                bounds.min.y;
            go.transform.position += Vector3.up * delta;
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
    }
}
#endif
