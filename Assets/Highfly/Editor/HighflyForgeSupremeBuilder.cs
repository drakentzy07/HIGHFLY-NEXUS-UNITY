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
    /// W1.3 definitive two-level forge interior.
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

        private const string KayKitSword =
            "Assets/External/KayKit/Adventurers/addons/" +
            "kaykit_character_pack_adventures/Assets/fbx/sword_1handed.fbx";

        private const string DiagnosticsDirectory = "build/diagnostics";

        private static readonly Vector3 RemoteOrigin =
            new Vector3(1000f, 30f, 1000f);

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
                    "w1-3-forge-definitive-assets.tsv"),
                new[] { "role\tasset_path" }.Concat(log.Lines).ToArray());

            Debug.Log(
                "HIGHFLY W1.3 FORGE DEFINITIVE built | scene=" + scenePath +
                " | donor assets=" + log.Lines.Count);
        }

        private static void BuildShell(Transform parent, AssetLog log)
        {
            Material floorMat = GetOrCreateHighflyMaterial(
                "HF_ForgeFloor",
                new Color(0.15f, 0.105f, 0.075f, 1f));

            Material wallMat = GetOrCreateHighflyMaterial(
                "HF_ForgeWall",
                new Color(0.34f, 0.275f, 0.19f, 1f));

            Material beamMat = GetOrCreateHighflyMaterial(
                "HF_ForgeBeam",
                new Color(0.105f, 0.065f, 0.04f, 1f));

            Material roofMat = GetOrCreateHighflyMaterial(
                "HF_ForgeRoof",
                new Color(0.07f, 0.075f, 0.085f, 1f));

            Material stoneMat = GetOrCreateHighflyMaterial(
                "HF_ForgeBasementStone",
                new Color(0.16f, 0.16f, 0.17f, 1f));

            Material stairMat = GetOrCreateHighflyMaterial(
                "HF_ForgeStair",
                new Color(0.20f, 0.13f, 0.08f, 1f));

            GameObject shopMarker = new GameObject("FORGE_SHOP_LEVEL");
            shopMarker.transform.SetParent(parent, false);

            GameObject basementMarker = new GameObject("FORGE_BASEMENT_LEVEL");
            basementMarker.transform.SetParent(parent, false);
            basementMarker.transform.localPosition = new Vector3(0f, -5.25f, 0f);

            // Ground floor is built from slabs so the stairwell is a REAL hole,
            // not a decorative staircase clipped through a solid floor.
            CreateBox(parent, "SHOP_FLOOR_LEFT",
                new Vector3(-3f, -0.16f, 0f),
                new Vector3(20f, 0.32f, 18f), floorMat, true);
            CreateBox(parent, "SHOP_FLOOR_RIGHT",
                new Vector3(11.75f, -0.16f, 0f),
                new Vector3(2.5f, 0.32f, 18f), floorMat, true);
            CreateBox(parent, "SHOP_FLOOR_STAIR_FRONT",
                new Vector3(8.75f, -0.16f, -3.9f),
                new Vector3(3.5f, 0.32f, 10.2f), floorMat, true);
            CreateBox(parent, "SHOP_FLOOR_STAIR_REAR",
                new Vector3(8.75f, -0.16f, 8.2f),
                new Vector3(3.5f, 0.32f, 1.6f), floorMat, true);

            // Basement floor and enclosing stone shell.
            CreateBox(parent, "BASEMENT_FLOOR",
                new Vector3(0f, -5.42f, 0f),
                new Vector3(26f, 0.34f, 18f), stoneMat, true);

            CreateBox(parent, "BASEMENT_WALL_SOUTH",
                new Vector3(0f, -2.72f, -8.82f),
                new Vector3(26f, 5.4f, 0.36f), stoneMat, true);
            CreateBox(parent, "BASEMENT_WALL_NORTH",
                new Vector3(0f, -2.72f, 8.82f),
                new Vector3(26f, 5.4f, 0.36f), stoneMat, true);
            CreateBox(parent, "BASEMENT_WALL_WEST",
                new Vector3(-12.82f, -2.72f, 0f),
                new Vector3(0.36f, 5.4f, 18f), stoneMat, true);
            CreateBox(parent, "BASEMENT_WALL_EAST",
                new Vector3(12.82f, -2.72f, 0f),
                new Vector3(0.36f, 5.4f, 18f), stoneMat, true);

            // Main shop shell: broader and taller than W1.2 so it reads as a
            // real fantasy blacksmith shop rather than a box with props.
            CreateBox(parent, "SHOP_WALL_SOUTH_L",
                new Vector3(-7.8f, 3.1f, -8.82f),
                new Vector3(10.4f, 6.2f, 0.36f), wallMat, true);
            CreateBox(parent, "SHOP_WALL_SOUTH_R",
                new Vector3(7.8f, 3.1f, -8.82f),
                new Vector3(10.4f, 6.2f, 0.36f), wallMat, true);
            CreateBox(parent, "SHOP_WALL_NORTH",
                new Vector3(0f, 3.1f, 8.82f),
                new Vector3(26f, 6.2f, 0.36f), wallMat, true);
            CreateBox(parent, "SHOP_WALL_WEST",
                new Vector3(-12.82f, 3.1f, 0f),
                new Vector3(0.36f, 6.2f, 18f), wallMat, true);
            CreateBox(parent, "SHOP_WALL_EAST",
                new Vector3(12.82f, 3.1f, 0f),
                new Vector3(0.36f, 6.2f, 18f), wallMat, true);

            CreateBox(parent, "SHOP_CEILING",
                new Vector3(0f, 6.28f, 0f),
                new Vector3(26f, 0.30f, 18f), roofMat, true);

            // Timber framing.
            for (int i = -4; i <= 4; i++)
            {
                CreateBox(parent, "SHOP_CEILING_BEAM_" + i,
                    new Vector3(i * 2.85f, 5.15f, 0f),
                    new Vector3(0.24f, 0.28f, 17.4f), beamMat, false);
            }

            for (int z = -1; z <= 1; z++)
            {
                CreateBox(parent, "SHOP_CROSS_BEAM_" + z,
                    new Vector3(0f, 4.85f, z * 5.4f),
                    new Vector3(25.4f, 0.30f, 0.26f), beamMat, false);
            }

            // Real descending stair from shop to basement.
            const int stepCount = 12;
            const float basementTop = -5.24f;
            for (int i = 0; i < stepCount; i++)
            {
                float topY = -0.20f - i * 0.42f;
                float height = Mathf.Max(0.22f, topY - basementTop);
                float centerY = basementTop + height * 0.5f;
                float z = 1.45f + i * 0.49f;

                CreateBox(parent, "FORGE_STAIR_STEP_" + i,
                    new Vector3(8.75f, centerY, z),
                    new Vector3(3.15f, height, 0.48f), stairMat, true);
            }

            // Stairwell rails and landing frame.
            CreateBox(parent, "FORGE_STAIR_RAIL_LEFT",
                new Vector3(6.95f, 0.62f, 4.25f),
                new Vector3(0.16f, 1.20f, 5.9f), beamMat, true);
            CreateBox(parent, "FORGE_STAIR_RAIL_RIGHT",
                new Vector3(10.55f, 0.62f, 4.25f),
                new Vector3(0.16f, 1.20f, 5.9f), beamMat, true);
            CreateBox(parent, "FORGE_STAIR_RAIL_REAR",
                new Vector3(8.75f, 0.62f, 7.35f),
                new Vector3(3.65f, 1.20f, 0.16f), beamMat, true);

            string door = FindAnyModel(RgPolyRoot, new[] { "door" }, false);
            string window = FindAnyModel(RgPolyRoot, new[] { "window" }, false);
            log.Add("shop.rgpoly_door", door);
            log.Add("shop.rgpoly_window", window);

            if (!string.IsNullOrEmpty(door))
            {
                PlaceDecor(door, parent, "SHOP_ENTRY_DOOR_VISUAL",
                    new Vector3(0f, 0f, -8.66f),
                    new Vector3(0f, 180f, 0f), 3.3f, false);
            }

            if (!string.IsNullOrEmpty(window))
            {
                PlaceDecor(window, parent, "SHOP_WINDOW_WEST",
                    new Vector3(-12.62f, 1.65f, -2.8f),
                    new Vector3(0f, 90f, 0f), 2.2f, false);
                PlaceDecor(window, parent, "SHOP_WINDOW_EAST",
                    new Vector3(12.62f, 1.65f, -2.8f),
                    new Vector3(0f, -90f, 0f), 2.2f, false);
                PlaceDecor(window, parent, "SHOP_WINDOW_NORTH",
                    new Vector3(-7.0f, 1.65f, 8.62f),
                    Vector3.zero, 2.2f, false);
            }

            log.Add("shell.structure",
                "HIGHFLY two-level shop + real stairwell + RG Poly detail");
        }
        private static void BuildForgeSet(Transform parent, AssetLog log)
        {
            Material woodMat = GetOrCreateHighflyMaterial(
                "HF_ForgeWood",
                new Color(0.20f, 0.115f, 0.065f, 1f));

            Material darkWoodMat = GetOrCreateHighflyMaterial(
                "HF_ForgeDarkWood",
                new Color(0.105f, 0.06f, 0.035f, 1f));

            Material metalMat = GetOrCreateHighflyMaterial(
                "HF_ForgeMetal",
                new Color(0.12f, 0.13f, 0.15f, 1f));

            string anvil = FindAnyModel(PropsRoot, new[] { "anvil" }, true);
            string workbench = FindAnyModel(
                PropsRoot,
                new[] { "workbench", "work_bench", "work bench", "craftingtable", "crafting_table" },
                true);

            string furnace = FindAnyModel(
                RgPolyRoot,
                new[] { "smithy", "cimey", "chimney" },
                true);

            string shelf = FindAnyModel(RgPolyRoot, new[] { "shelf" }, false);
            string table = FindAnyModel(RgPolyRoot, new[] { "table" }, false);
            string bench = FindAnyModel(RgPolyRoot, new[] { "bench" }, false);
            string stool = FindAnyModel(RgPolyRoot, new[] { "stool" }, false);
            string barrel = FindAnyModel(RgPolyRoot, new[] { "barrel" }, false);
            string crate = FindAnyModel(RgPolyRoot, new[] { "crate" }, false);
            string shield = FindAnyModel(RgPolyRoot, new[] { "shield" }, false);
            string axe = FindAnyModel(RgPolyRoot, new[] { "axe" }, false);
            string grinder = FindAnyModel(RgPolyRoot, new[] { "grinder" }, false);
            string helmet = FindAnyModel(RgPolyRoot, new[] { "helmet" }, false);
            string sword =
                AssetDatabase.LoadAssetAtPath<GameObject>(KayKitSword) != null
                    ? KayKitSword
                    : FindAnyModel(RgPolyRoot, new[] { "sword" }, false);

            log.Add("shop.shelf", shelf);
            log.Add("shop.table", table);
            log.Add("shop.bench", bench);
            log.Add("shop.stool", stool);
            log.Add("shop.weapon.sword", sword);
            log.Add("shop.weapon.axe", axe);
            log.Add("shop.armor.shield", shield);
            log.Add("shop.armor.helmet", helmet);
            log.Add("basement.anvil", anvil);
            log.Add("basement.furnace", furnace);
            log.Add("basement.workbench", workbench);
            log.Add("basement.grinder", grinder);
            log.Add("basement.storage.barrel", barrel);
            log.Add("basement.storage.crate", crate);

            // -----------------------------------------------------------------
            // GROUND FLOOR — CUSTOMER SHOP
            // -----------------------------------------------------------------
            CreateBox(parent, "SHOP_COUNTER_BODY",
                new Vector3(0f, 0.72f, 3.15f),
                new Vector3(8.8f, 1.35f, 1.35f), darkWoodMat, true);
            CreateBox(parent, "SHOP_COUNTER_TOP",
                new Vector3(0f, 1.46f, 3.15f),
                new Vector3(9.4f, 0.18f, 1.62f), woodMat, true);
            CreateBox(parent, "SHOP_COUNTER_SIDE_L",
                new Vector3(-4.55f, 0.82f, 3.15f),
                new Vector3(0.22f, 1.65f, 1.75f), metalMat, false);
            CreateBox(parent, "SHOP_COUNTER_SIDE_R",
                new Vector3(4.55f, 0.82f, 3.15f),
                new Vector3(0.22f, 1.65f, 1.75f), metalMat, false);

            // Two ordered display plinths form the shop windows/showroom.
            CreateBox(parent, "SHOP_VITRINE_WEAPONS_BASE",
                new Vector3(-8.4f, 0.48f, -2.2f),
                new Vector3(5.4f, 0.95f, 2.3f), woodMat, true);
            CreateBox(parent, "SHOP_VITRINE_ARMOR_BASE",
                new Vector3(-8.4f, 0.48f, 1.0f),
                new Vector3(5.4f, 0.95f, 2.3f), woodMat, true);

            if (!string.IsNullOrEmpty(sword))
            {
                PlaceDecor(sword, parent, "SHOP_DISPLAY_SWORD",
                    new Vector3(-9.3f, 1.0f, -2.2f),
                    new Vector3(0f, 0f, 78f), 1.45f, false);
            }

            if (!string.IsNullOrEmpty(axe))
            {
                PlaceDecor(axe, parent, "SHOP_DISPLAY_AXE",
                    new Vector3(-7.5f, 1.0f, -2.2f),
                    new Vector3(0f, 0f, 82f), 1.45f, false);
            }

            if (!string.IsNullOrEmpty(shield))
            {
                PlaceDecor(shield, parent, "SHOP_DISPLAY_SHIELD",
                    new Vector3(-8.6f, 1.0f, 1.0f),
                    new Vector3(0f, 180f, 0f), 1.65f, false);
            }

            if (!string.IsNullOrEmpty(helmet))
            {
                PlaceDecor(helmet, parent, "SHOP_DISPLAY_HELMET",
                    new Vector3(-6.9f, 1.0f, 1.0f),
                    Vector3.zero, 1.25f, false);
            }

            AddInteraction(parent, "SHOP_WEAPON_SHOWCASE",
                parent.TransformPoint(new Vector3(-8.4f, 1.0f, -2.2f)),
                new Vector3(5.8f, 2.4f, 2.8f),
                HighflyForgeActionKind.WeaponDisplay,
                "VITRINA DE ARMAS", "INSPECCIONAR");

            AddInteraction(parent, "SHOP_ARMOR_SHOWCASE",
                parent.TransformPoint(new Vector3(-8.4f, 1.0f, 1.0f)),
                new Vector3(5.8f, 2.4f, 2.8f),
                HighflyForgeActionKind.WeaponDisplay,
                "EXHIBICIÓN DE EQUIPO", "INSPECCIONAR");

            if (!string.IsNullOrEmpty(shelf))
            {
                PlaceDecor(shelf, parent, "SHOP_SHELF_NORTH_A",
                    new Vector3(-9.8f, 0f, 7.7f),
                    Vector3.zero, 2.7f, true);
                PlaceDecor(shelf, parent, "SHOP_SHELF_NORTH_B",
                    new Vector3(-6.7f, 0f, 7.7f),
                    Vector3.zero, 2.7f, true);
                PlaceDecor(shelf, parent, "SHOP_SHELF_EAST",
                    new Vector3(11.7f, 0f, -4.0f),
                    new Vector3(0f, -90f, 0f), 2.7f, true);
            }

            if (!string.IsNullOrEmpty(bench))
            {
                PlaceDecor(bench, parent, "SHOP_CUSTOMER_BENCH",
                    new Vector3(7.8f, 0f, -4.8f),
                    new Vector3(0f, 180f, 0f), 1.0f, true);
            }

            if (!string.IsNullOrEmpty(stool))
            {
                PlaceDecor(stool, parent, "SHOP_HERRERO_STOOL",
                    new Vector3(3.5f, 0f, 5.2f),
                    new Vector3(0f, 15f, 0f), 0.75f, true);
            }

            // Sign/partition that visually leads the player toward the stair.
            CreateBox(parent, "SHOP_STAIR_SIGN_POST",
                new Vector3(6.2f, 1.15f, 0.2f),
                new Vector3(0.22f, 2.3f, 0.22f), darkWoodMat, false);
            CreateBox(parent, "SHOP_STAIR_SIGN_BOARD",
                new Vector3(6.2f, 2.0f, 0.2f),
                new Vector3(2.2f, 0.7f, 0.18f), woodMat, false);

            // -----------------------------------------------------------------
            // BASEMENT — WORKSHOP / PRODUCTION
            // -----------------------------------------------------------------
            GameObject furnaceGo = PlaceDecor(
                furnace, parent, "BASEMENT_FURNACE",
                new Vector3(-8.8f, -5.22f, 4.9f),
                new Vector3(0f, 135f, 0f), 2.9f, true);

            GameObject anvilGo = PlaceDecor(
                anvil, parent, "BASEMENT_ANVIL",
                new Vector3(-3.0f, -5.22f, 1.2f),
                new Vector3(0f, -12f, 0f), 1.15f, true);
            ApplyMaterial(anvilGo, metalMat);

            GameObject benchGo = PlaceDecor(
                workbench, parent, "BASEMENT_WORKBENCH",
                new Vector3(4.4f, -5.22f, 4.6f),
                new Vector3(0f, -145f, 0f), 1.6f, true);
            ApplyMaterial(benchGo, woodMat);

            AddInteraction(parent, "STATION_FURNACE",
                furnaceGo.transform.position + Vector3.forward * 0.45f,
                new Vector3(3.5f, 2.6f, 3.5f),
                HighflyForgeActionKind.Furnace,
                "HORNO DE FUNDICIÓN", "FUNDIR");

            AddInteraction(parent, "STATION_ANVIL",
                anvilGo.transform.position,
                new Vector3(2.8f, 2.1f, 2.8f),
                HighflyForgeActionKind.Anvil,
                "YUNQUE PRINCIPAL", "FORJAR");

            AddInteraction(parent, "STATION_WORKBENCH",
                benchGo.transform.position,
                new Vector3(3.4f, 2.1f, 2.8f),
                HighflyForgeActionKind.Workbench,
                "BANCO DE TRABAJO", "CRAFT");

            // Materials occupy one dedicated corner instead of floating through
            // the room: crates + barrels + optional grinder.
            Vector3 storage = new Vector3(-8.9f, -5.22f, -4.8f);
            if (!string.IsNullOrEmpty(barrel))
            {
                PlaceDecor(barrel, parent, "BASEMENT_BARREL_A",
                    storage, Vector3.zero, 1.35f, true);
                PlaceDecor(barrel, parent, "BASEMENT_BARREL_B",
                    storage + new Vector3(1.45f, 0f, 0.25f),
                    new Vector3(0f, 18f, 0f), 1.20f, true);
            }

            if (!string.IsNullOrEmpty(crate))
            {
                PlaceDecor(crate, parent, "BASEMENT_CRATE_A",
                    storage + new Vector3(0.4f, 0f, 1.55f),
                    new Vector3(0f, 25f, 0f), 1.05f, true);
                PlaceDecor(crate, parent, "BASEMENT_CRATE_B",
                    storage + new Vector3(1.7f, 0f, 1.7f),
                    new Vector3(0f, -12f, 0f), 0.9f, true);
            }

            if (!string.IsNullOrEmpty(grinder))
            {
                PlaceDecor(grinder, parent, "BASEMENT_GRINDER",
                    new Vector3(6.8f, -5.22f, -4.2f),
                    new Vector3(0f, -90f, 0f), 1.45f, true);
            }

            CreateBox(parent, "BASEMENT_MATERIAL_PLATFORM",
                new Vector3(-8.6f, -5.05f, -4.5f),
                new Vector3(6.5f, 0.22f, 5.2f), darkWoodMat, true);

            AddInteraction(parent, "STATION_MATERIAL_STORAGE",
                parent.TransformPoint(new Vector3(-8.5f, -4.5f, -4.4f)),
                new Vector3(6.5f, 2.4f, 5.4f),
                HighflyForgeActionKind.MaterialStorage,
                "ALMACÉN DE MATERIALES", "REVISAR");

            // Low dividers make workshop zones legible while retaining an open
            // path from stair landing to every station.
            CreateBox(parent, "BASEMENT_DIVIDER_FORGE",
                new Vector3(-5.6f, -4.62f, -0.7f),
                new Vector3(0.22f, 1.25f, 7.6f), darkWoodMat, false);
            CreateBox(parent, "BASEMENT_DIVIDER_CRAFT",
                new Vector3(1.0f, -4.62f, 5.9f),
                new Vector3(7.6f, 1.25f, 0.22f), darkWoodMat, false);

            if (!string.IsNullOrEmpty(table))
            {
                PlaceDecor(table, parent, "BASEMENT_REPAIR_TABLE",
                    new Vector3(7.5f, -5.22f, 0.8f),
                    new Vector3(0f, -90f, 0f), 1.15f, true);
            }

            log.Add("layout.shop", "counter + showcases + shelves + customer circulation");
            log.Add("layout.basement", "furnace + anvil + crafting + materials + repair");
        }
        private static void BuildBlacksmith(Transform parent, AssetLog log)
        {
            GameObject source =
                AssetDatabase.LoadAssetAtPath<GameObject>(KayKitBarbarian);

            if (source == null)
                throw new FileNotFoundException(
                    "HIGHFLY W1.3 blacksmith visual missing",
                    KayKitBarbarian);

            GameObject npc = UnityEngine.Object.Instantiate(source);
            npc.name = "HERRERO_KAYKIT";
            npc.transform.SetParent(parent, false);

            // Behind the retail counter, facing the customer entrance.
            npc.transform.localPosition = new Vector3(0f, 0f, 4.9f);
            npc.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            NormalizeHeight(npc, 1.82f);
            MoveBottomToLocalY(npc, parent, 0f);

            CapsuleCollider trigger = npc.AddComponent<CapsuleCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.95f, 0f);
            trigger.height = 2.1f;
            trigger.radius = 0.85f;

            HighflyForgeInteractable interactable =
                npc.AddComponent<HighflyForgeInteractable>();

            interactable.Configure(
                HighflyForgeActionKind.Blacksmith,
                "HERRERO / VENDEDOR",
                "COMERCIAR");

            log.Add("shop.blacksmith_npc", KayKitBarbarian);
        }
        private static void BuildEntryExit(
            Transform parent,
            HighflyInteriorDefinition definition)
        {
            GameObject entry = new GameObject("ENTRY_ANCHOR");
            entry.transform.SetParent(parent, false);
            entry.transform.localPosition = new Vector3(0f, 0.15f, -7.25f);
            entry.transform.localRotation = Quaternion.identity;

            HighflyWorldAnchor entryAnchor =
                entry.AddComponent<HighflyWorldAnchor>();

            entryAnchor.Configure(
                definition.entryAnchorId,
                definition.displayName + " entry",
                "interior_entry");

            GameObject exit = new GameObject("EXIT_TO_CAPITAL");
            exit.transform.SetParent(parent, false);
            exit.transform.localPosition = new Vector3(0f, 0f, -8.35f);
            exit.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            BoxCollider exitCollider = exit.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            exitCollider.center = new Vector3(0f, 1.10f, 0f);
            exitCollider.size = new Vector3(3.8f, 2.6f, 1.45f);

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
            // Retail floor: warm but clean enough to read weapons/armor.
            CreateLight(parent, "SHOP_LIGHT_MAIN",
                new Vector3(0f, 4.75f, -1.0f),
                new Color(1f, 0.79f, 0.58f, 1f),
                2.15f, 18f);

            CreateLight(parent, "SHOP_LIGHT_SHOWCASE",
                new Vector3(-8.4f, 3.0f, -0.7f),
                new Color(1f, 0.72f, 0.45f, 1f),
                1.65f, 8f);

            CreateLight(parent, "SHOP_LIGHT_COUNTER",
                new Vector3(0f, 3.2f, 4.0f),
                new Color(1f, 0.68f, 0.40f, 1f),
                1.55f, 8f);

            // Basement: forge heat dominates, with a softer utility fill on the
            // crafting/material side so the workshop never turns into a void.
            CreateLight(parent, "BASEMENT_LIGHT_FORGE",
                new Vector3(-7.8f, -2.8f, 4.2f),
                new Color(1f, 0.25f, 0.055f, 1f),
                3.4f, 9f);

            CreateLight(parent, "BASEMENT_LIGHT_WORK",
                new Vector3(3.8f, -2.7f, 2.6f),
                new Color(1f, 0.58f, 0.28f, 1f),
                2.0f, 11f);

            CreateLight(parent, "BASEMENT_LIGHT_STORAGE",
                new Vector3(-7.5f, -3.0f, -4.4f),
                new Color(0.86f, 0.62f, 0.38f, 1f),
                1.3f, 7f);
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
