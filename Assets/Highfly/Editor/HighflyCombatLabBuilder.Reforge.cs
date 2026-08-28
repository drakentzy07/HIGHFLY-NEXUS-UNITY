#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Highfly.Combat;
using Highfly.Core;
using Highfly.Mobile;
using Highfly.World;

namespace Highfly.Editor
{
    public static partial class HighflyCombatLabBuilder
    {
        public const string ReforgeScenePath = "Assets/Highfly/Scenes/HIGHFLY_World_Reforge.unity";

        private static readonly Vector3 ReforgeDungeonCenter = new Vector3(0f, 0f, 255f);

        private const string NeutralMedievalRoot = MedievalAssetRoot + "/buildings/neutral";
        private const string NatureMedievalRoot = MedievalAssetRoot + "/decoration/nature";
        private const string PropsMedievalRoot = MedievalAssetRoot + "/decoration/props";

        private const string BridgeAPath = NeutralMedievalRoot + "/building_bridge_A.fbx";
        private const string DestroyedBuildingPath = NeutralMedievalRoot + "/building_destroyed.fbx";
        private const string TreeAPath = NatureMedievalRoot + "/tree_single_A.fbx";
        private const string TreeBPath = NatureMedievalRoot + "/tree_single_B.fbx";
        private const string RockAPath = NatureMedievalRoot + "/rock_single_A.fbx";
        private const string RockBPath = NatureMedievalRoot + "/rock_single_B.fbx";
        private const string HillAPath = NatureMedievalRoot + "/hills_A_trees.fbx";
        private const string HillBPath = NatureMedievalRoot + "/hills_B_trees.fbx";
        private const string MountainAPath = NatureMedievalRoot + "/mountain_A_grass_trees.fbx";
        private const string MountainBPath = NatureMedievalRoot + "/mountain_B_grass_trees.fbx";
        private const string WatermillPath = MedievalAssetRoot + "/buildings/blue/building_watermill_blue.fbx";
        private const string WindmillPath = MedievalAssetRoot + "/buildings/blue/building_windmill_blue.fbx";
        private const string LumbermillPath = MedievalAssetRoot + "/buildings/blue/building_lumbermill_blue.fbx";
        private const string BridgePath = NeutralMedievalRoot + "/building_bridge_A.fbx";
        private const string TargetPropPath = PropsMedievalRoot + "/target.fbx";
        private const string WeaponRackPropPath = PropsMedievalRoot + "/weaponrack.fbx";
        private const string LumberPropPath = PropsMedievalRoot + "/resource_lumber.fbx";
        private const string StonePropPath = PropsMedievalRoot + "/resource_stone.fbx";
        private const string TentPropPath = PropsMedievalRoot + "/tent.fbx";

        [MenuItem("HIGHFLY/SUPREME/Build World Reforge")]
        public static void BuildOrRefreshWorldReforge()
        {
            Directory.CreateDirectory("Assets/Highfly/Scenes");
            Directory.CreateDirectory(GeneratedRoot);
            Directory.CreateDirectory(GeneratedUiRoot);
            Directory.CreateDirectory(GeneratedControllersRoot);
            CityBuildings.Clear();

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HIGHFLY_WORLD_REFORGE";

            ConfigureEnvironment();
            EnsureEventSystem();

            GameObject worldRoot = new GameObject("WORLD_OVERWORLD_REFORGE");
            CreateReforgeWorldFoundation(worldRoot.transform);

            GameObject cityRoot = new GameObject("ZONE_CAPITAL_VALTHERA");
            CityMetrics city = CreateReforgeCapital(cityRoot.transform, null);
            cityRoot.transform.position = CityCenter;

            Sprite circleSprite = CreateCircleSprite();

            GameObject player = CreatePlayer(
                out HighflyThirdPersonMotor motor,
                out HighflyTargetingSystem targeting,
                out HighflyCombatController combat,
                out HighflyPlayerResources resources,
                out Transform attackOrigin,
                out Animator playerAnimator);

            Camera camera = CreateCamera(player.transform, targeting, out HighflyThirdPersonCamera cameraRig);
            GameObject canvas = CreateMobileHUD(
                motor, targeting, combat, resources, cameraRig, circleSprite,
                out HighflyVirtualJoystick joystick, out HighflyCameraLookArea lookArea);

            HighflyInteractionController interaction = player.GetComponent<HighflyInteractionController>();
            CreateInteractionUI(interaction, canvas.GetComponent<RectTransform>(), circleSprite);

            SetObjectReference(motor, "cameraTransform", camera.transform);
            SetObjectReference(motor, "movementJoystick", joystick);
            SetObjectReference(motor, "animator", playerAnimator);

            SetObjectReference(targeting, "origin", player.transform);
            SetObjectReference(targeting, "gameplayCamera", camera);
            SetInt(targeting, "targetMask", EnemyMask);

            SetObjectReference(combat, "motor", motor);
            SetObjectReference(combat, "targeting", targeting);
            SetObjectReference(combat, "resources", resources);
            SetObjectReference(combat, "attackOrigin", attackOrigin);
            SetObjectReference(combat, "animator", playerAnimator);
            SetInt(combat, "enemyMask", EnemyMask);

            SetObjectReference(cameraRig, "lookArea", lookArea);
            SetObjectReference(cameraRig, "targeting", targeting);

            // Rebuild capital residents with the player's valid humanoid controller.
            RuntimeAnimatorController npcController =
                playerAnimator != null ? playerAnimator.runtimeAnimatorController : null;
            CreateReforgeCapitalResidents(cityRoot.transform, npcController);
            CreateReforgeInteriorsAndDoors(npcController);

            Vector3 citySpawn = CityCenter + new Vector3(0f, 0.12f, -8f);
            Vector3 fixedGatePosition = CityCenter + new Vector3(0f, 0f, 20f);

            CharacterController playerController = player.GetComponent<CharacterController>();
            if (playerController != null)
                playerController.enabled = false;
            player.transform.position = citySpawn;
            player.transform.rotation = Quaternion.identity;
            if (playerController != null)
                playerController.enabled = true;

            HighflyWorldSafety playerSafety = player.AddComponent<HighflyWorldSafety>();
            playerSafety.Configure(citySpawn, -18f, true, true);

            HighflyRetreatController retreat = player.GetComponent<HighflyRetreatController>();
            if (retreat != null)
                retreat.Configure(citySpawn, Vector3.forward);

            // A separate, dense dungeon instance lives outside the overworld footprint.
            GameObject dungeonRoot = new GameObject("INSTANCE_CRIPTA_F_REFORGE");
            ArenaMetrics arena = CreateDungeonArena(dungeonRoot.transform);
            CreateBossStageDressing(dungeonRoot.transform, Mathf.Max(12f, arena.halfDepth * 0.62f));
            dungeonRoot.transform.position = ReforgeDungeonCenter;

            Vector3 dungeonSpawn = ReforgeDungeonCenter + new Vector3(0f, 0.12f, -arena.halfDepth + 4.2f);
            Vector3 dungeonReturn = ReforgeDungeonCenter + new Vector3(0f, 0f, -arena.halfDepth + 1.8f);

            CreateZonePortal(
                "GATE_F_PERMANENTE",
                fixedGatePosition,
                dungeonSpawn,
                Vector3.forward,
                Violet,
                "GATE F • CRIPTA DEL GUARDIÁN",
                true);

            CreateZonePortal(
                "RETORNO_CAPITAL",
                dungeonReturn,
                citySpawn,
                Vector3.back,
                Cyan,
                "RETORNO • VALTHERA",
                true);

            HighflyHealth playerHealth = player.GetComponent<HighflyHealth>();
            CreateReforgeDungeonEnemies(dungeonRoot.transform, arena, player.transform, playerHealth);

            HighflyDungeonObjective objective = canvas.GetComponent<HighflyDungeonObjective>();
            if (objective != null)
                objective.ConfigureTrackingRoot(dungeonRoot.transform);

            Text gateAlert = CreateGateAlert(canvas.GetComponent<RectTransform>());
            HighflyGateAnchor[] anchors = CreateGateAnchors(worldRoot.transform, player.transform, playerHealth);

            GameObject directorGo = new GameObject("WORLD_GATE_DIRECTOR");
            HighflyDynamicGateDirector director = directorGo.AddComponent<HighflyDynamicGateDirector>();
            director.Configure(player.transform, anchors, dungeonSpawn, Vector3.forward, gateAlert);

            CreateDayNightSystem();
            CreateWorldTitle(worldRoot.transform);

            EditorSceneManager.SaveScene(scene, ReforgeScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ReforgeScenePath, true) };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "HIGHFLY WORLD REFORGE generated: capital + unique interiors + overworld biomes + " +
                "living Gates + patrol AI + scoped dungeon objective.");
        }

        private static void CreateReforgeWorldFoundation(Transform root)
        {
            Material grass = CreateStandardMaterial("ReforgeGrass", new Color(0.18f, 0.34f, 0.20f, 1f));
            Material stone = CreateStandardMaterial("ReforgeRoad", new Color(0.25f, 0.27f, 0.31f, 1f));
            Material water = CreateStandardMaterial("ReforgeWater", new Color(0.05f, 0.34f, 0.56f, 1f));
            Material earth = CreateStandardMaterial("ReforgeEarth", new Color(0.28f, 0.22f, 0.14f, 1f));

            CreatePrimitiveBlock(
                "OverworldGround", root, new Vector3(0f, -0.35f, 22f),
                new Vector3(214f, 0.6f, 260f), grass, true);

            // Main road: capital -> plains -> foothills. Deliberately authored, never random.
            CreatePrimitiveBlock("KingsRoad_A", root, new Vector3(0f, 0.01f, -14f), new Vector3(8f, 0.10f, 90f), stone, false);
            CreatePrimitiveBlock("KingsRoad_B", root, new Vector3(0f, 0.01f, 70f), new Vector3(8f, 0.10f, 78f), stone, false);
            CreatePrimitiveBlock("WestVillageRoad", root, new Vector3(-34f, 0.015f, 35f), new Vector3(68f, 0.10f, 6f), stone, false);
            CreatePrimitiveBlock("ForestRoad", root, new Vector3(32f, 0.015f, 56f), new Vector3(58f, 0.10f, 5f), earth, false);

            // River is intentionally offset from all building doors and Gate pads.
            GameObject river = CreatePrimitiveBlock(
                "River_Aster", root, new Vector3(73f, -0.13f, 35f),
                new Vector3(15f, 0.16f, 176f), water, false);
            Collider riverCollider = river != null ? river.GetComponent<Collider>() : null;
            if (riverCollider != null)
                UnityEngine.Object.DestroyImmediate(riverCollider);

            if (AssetExists(BridgePath))
                PlaceEnvironmentModel(BridgePath, root, new Vector3(73f, 0f, 8f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.1f, "Bridge_Aster");

            // WEST: useful village/resource zone.
            PlaceEnvironmentModel(CityHomeAPath, root, new Vector3(-66f, 0f, 35f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 2.1f, "Village_Home_A");
            PlaceEnvironmentModel(CityHomeBPath, root, new Vector3(-75f, 0f, 48f), Quaternion.Euler(0f, -25f, 0f), Vector3.one * 2.0f, "Village_Home_B");
            PlaceEnvironmentModel(WindmillPath, root, new Vector3(-83f, 0f, 69f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 2.0f, "Village_Windmill");
            PlaceEnvironmentModel(LumbermillPath, root, new Vector3(-58f, 0f, 64f), Quaternion.Euler(0f, -12f, 0f), Vector3.one * 2.0f, "Lumber_Camp");
            PlaceEnvironmentModel(LumberPropPath, root, new Vector3(-54f, 0f, 57f), Quaternion.identity, Vector3.one * 1.8f, "Lumber_Resource");
            PlaceEnvironmentModel(TentPropPath, root, new Vector3(-48f, 0f, 66f), Quaternion.identity, Vector3.one * 1.7f, "Lumber_Tent");

            // EAST: river activity and forest. Keep a wide clean pad around (42,55) for a living Gate.
            PlaceEnvironmentModel(WatermillPath, root, new Vector3(63f, 0f, 1f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.9f, "Aster_Watermill");
            Vector3[] forest =
            {
                new Vector3(31f,0f,28f), new Vector3(43f,0f,29f), new Vector3(54f,0f,34f),
                new Vector3(29f,0f,45f), new Vector3(57f,0f,48f), new Vector3(29f,0f,66f),
                new Vector3(55f,0f,68f), new Vector3(36f,0f,78f), new Vector3(51f,0f,82f),
                new Vector3(62f,0f,74f), new Vector3(24f,0f,84f)
            };
            for (int i = 0; i < forest.Length; i++)
            {
                string treePath = i % 2 == 0 ? TreeAPath : TreeBPath;
                PlaceEnvironmentModel(treePath, root, forest[i], Quaternion.Euler(0f, (i * 37) % 360, 0f), Vector3.one * (1.7f + (i % 3) * 0.18f), "ForestTree_" + i);
            }

            PlaceEnvironmentModel(RockAPath, root, new Vector3(20f, 0f, 92f), Quaternion.identity, Vector3.one * 2f, "ForestRock_A");
            PlaceEnvironmentModel(RockBPath, root, new Vector3(61f, 0f, 91f), Quaternion.Euler(0f, 60f, 0f), Vector3.one * 2f, "ForestRock_B");

            // NORTH: ruins and mountains create a readable high-level destination.
            PlaceEnvironmentModel(DestroyedBuildingPath, root, new Vector3(-39f, 0f, 82f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 2.4f, "Ruins_Of_Aster");
            PlaceEnvironmentModel(StonePropPath, root, new Vector3(-31f, 0f, 87f), Quaternion.identity, Vector3.one * 2.2f, "Ruins_Stone");
            PlaceEnvironmentModel(HillAPath, root, new Vector3(-72f, 0f, 111f), Quaternion.identity, Vector3.one * 2.0f, "Western_Hills");
            PlaceEnvironmentModel(HillBPath, root, new Vector3(68f, 0f, 113f), Quaternion.identity, Vector3.one * 1.9f, "Eastern_Hills");
            PlaceEnvironmentModel(MountainAPath, root, new Vector3(-34f, 0f, 137f), Quaternion.identity, Vector3.one * 2.0f, "North_Mountain_A");
            PlaceEnvironmentModel(MountainBPath, root, new Vector3(38f, 0f, 141f), Quaternion.Euler(0f, 25f, 0f), Vector3.one * 2.0f, "North_Mountain_B");
        }

        private static CityMetrics CreateReforgeCapital(Transform root, RuntimeAnimatorController unused)
        {
            Material plaza = CreateStandardMaterial("CapitalPlaza", new Color(0.30f, 0.31f, 0.34f, 1f));
            Material wall = CreateStandardMaterial("CapitalWall", new Color(0.31f, 0.32f, 0.36f, 1f));

            const float halfWidth = 44f;
            const float halfDepth = 40f;

            CreatePrimitiveBlock("CapitalFloor", root, Vector3.down * 0.12f, new Vector3(90f, 0.22f, 82f), plaza, true);
            CreatePrimitiveBlock("CapitalMainRoad", root, new Vector3(0f, 0.02f, 0f), new Vector3(7f, 0.08f, 78f), wall, false);
            CreatePrimitiveBlock("CapitalCrossRoad", root, new Vector3(0f, 0.025f, -2f), new Vector3(76f, 0.08f, 7f), wall, false);
            CreatePrimitiveBlock("CapitalPlaza", root, new Vector3(0f, 0.03f, 1f), new Vector3(22f, 0.08f, 22f), wall, false);

            // Complete walls with one intentional north gate; no floating or missing procedural segments.
            CreatePrimitiveBlock("WallSouth", root, new Vector3(0f, 2.1f, -halfDepth), new Vector3(90f, 4.2f, 1.2f), wall, true);
            CreatePrimitiveBlock("WallWest", root, new Vector3(-halfWidth, 2.1f, 0f), new Vector3(1.2f, 4.2f, 81f), wall, true);
            CreatePrimitiveBlock("WallEast", root, new Vector3(halfWidth, 2.1f, 0f), new Vector3(1.2f, 4.2f, 81f), wall, true);
            CreatePrimitiveBlock("WallNorthL", root, new Vector3(-25f, 2.1f, halfDepth), new Vector3(38f, 4.2f, 1.2f), wall, true);
            CreatePrimitiveBlock("WallNorthR", root, new Vector3(25f, 2.1f, halfDepth), new Vector3(38f, 4.2f, 1.2f), wall, true);

            PlaceEnvironmentModel(CityTowerPath, root, new Vector3(-42f, 0f, 38f), Quaternion.identity, Vector3.one * 2.8f, "WallTower_NW");
            PlaceEnvironmentModel(CityTowerPath, root, new Vector3(42f, 0f, 38f), Quaternion.identity, Vector3.one * 2.8f, "WallTower_NE");

            GameObject buildings = new GameObject("Capital_Functional_Buildings");
            buildings.transform.SetParent(root, false);

            PlaceCityBuilding(buildings.transform, CityGuildPath, new Vector3(-22f, 0f, 12f), Quaternion.Euler(0f, 25f, 0f), "GREMIO");
            PlaceCityBuilding(buildings.transform, CityTavernPath, new Vector3(22f, 0f, 12f), Quaternion.Euler(0f, -25f, 0f), "TABERNA");
            PlaceCityBuilding(buildings.transform, CityBlacksmithPath, new Vector3(-28f, 0f, -4f), Quaternion.Euler(0f, 20f, 0f), "FORJA");
            PlaceCityBuilding(buildings.transform, CityMarketPath, new Vector3(28f, 0f, -4f), Quaternion.Euler(0f, -20f, 0f), "MERCADO");
            PlaceCityBuilding(buildings.transform, CityChurchPath, new Vector3(-23f, 0f, -23f), Quaternion.Euler(0f, 15f, 0f), "SANTUARIO");
            PlaceCityBuilding(buildings.transform, CityHomeBPath, new Vector3(23f, 0f, -23f), Quaternion.Euler(0f, -15f, 0f), "ARCHIVO");
            PlaceCityBuilding(buildings.transform, CityAcademyPath, new Vector3(-11f, 0f, 29f), Quaternion.Euler(0f, 160f, 0f), "ACADEMIA");
            PlaceCityBuilding(buildings.transform, CityTowerPath, new Vector3(11f, 0f, 29f), Quaternion.Euler(0f, -160f, 0f), "TORRE");
            PlaceCityBuilding(buildings.transform, CityMinePath, new Vector3(34f, 0f, -28f), Quaternion.Euler(0f, -135f, 0f), "MINA");
            PlaceCityBuilding(buildings.transform, CityCastlePath, new Vector3(0f, 0f, -31f), Quaternion.identity, "NEXUS CENTRAL");

            PlaceEnvironmentModel(CityWellPath, root, new Vector3(0f, 0f, 1f), Quaternion.identity, Vector3.one * 2.3f, "Capital_Well");

            return new CityMetrics
            {
                width = halfWidth * 2f,
                depth = halfDepth * 2f,
                halfWidth = halfWidth,
                halfDepth = halfDepth
            };
        }

        private static void CreateReforgeCapitalResidents(Transform cityRoot, RuntimeAnimatorController controller)
        {
            CreateResident("Serin_Gremio", AdventurerRoot + "/Characters/fbx/Knight.fbx", CityCenter + new Vector3(-10f,0f,4f), cityRoot, controller, false);
            CreateResident("Herrero", AdventurerRoot + "/Characters/fbx/Barbarian.fbx", CityCenter + new Vector3(-22f,0f,-4f), cityRoot, controller, false);
            CreateResident("Erudita", AdventurerRoot + "/Characters/fbx/Mage.fbx", CityCenter + new Vector3(18f,0f,-19f), cityRoot, controller, false);
            CreateResident("Tabernera", AdventurerRoot + "/Characters/fbx/Rogue.fbx", CityCenter + new Vector3(18f,0f,8f), cityRoot, controller, false);
            CreateResident("Maestro_Academia", AdventurerRoot + "/Characters/fbx/Knight.fbx", CityCenter + new Vector3(-9f,0f,23f), cityRoot, controller, false);
            CreateResident("Mercader", AdventurerRoot + "/Characters/fbx/RogueHooded.fbx", CityCenter + new Vector3(22f,0f,-2f), cityRoot, controller, false);

            string[] models =
            {
                AdventurerRoot + "/Characters/fbx/Knight.fbx",
                AdventurerRoot + "/Characters/fbx/Rogue.fbx",
                AdventurerRoot + "/Characters/fbx/Mage.fbx",
                AdventurerRoot + "/Characters/fbx/RogueHooded.fbx"
            };

            Vector3[] positions =
            {
                new Vector3(-5f,0f,12f), new Vector3(7f,0f,10f), new Vector3(-16f,0f,-8f),
                new Vector3(14f,0f,-10f), new Vector3(-6f,0f,-22f), new Vector3(8f,0f,-18f),
                new Vector3(-28f,0f,20f), new Vector3(27f,0f,19f)
            };

            for (int i = 0; i < positions.Length; i++)
                CreateResident("Ciudadano_" + (i + 1), models[i % models.Length], CityCenter + positions[i], cityRoot, controller, true);
        }

        private static void CreateResident(
            string name,
            string modelPath,
            Vector3 worldPosition,
            Transform parent,
            RuntimeAnimatorController controller,
            bool wander)
        {
            CreateCityNpc(name, modelPath, worldPosition, parent, controller);
            GameObject npc = GameObject.Find(name);
            if (wander && npc != null)
                npc.AddComponent<HighflyNpcWander>();
        }

        private static void CreateReforgeInteriorsAndDoors(RuntimeAnimatorController npcController)
        {
            string[] labels =
            {
                "GREMIO", "TABERNA", "FORJA", "SANTUARIO", "MERCADO",
                "ARCHIVO", "ACADEMIA", "TORRE", "MINA", "NEXUS CENTRAL"
            };

            Vector2[] sizes =
            {
                new Vector2(18f,14f), new Vector2(20f,13f), new Vector2(14f,11f),
                new Vector2(12f,20f), new Vector2(20f,12f), new Vector2(14f,18f),
                new Vector2(20f,16f), new Vector2(11f,11f), new Vector2(15f,12f),
                new Vector2(18f,18f)
            };

            GameObject root = new GameObject("FUNCTIONAL_UNIQUE_INTERIORS");

            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                if (!CityBuildings.TryGetValue(label, out GameObject building) || building == null)
                    continue;

                Vector3 center = new Vector3(310f + (i % 2) * 34f, 0f, -80f + (i / 2) * 34f);
                CreateUniqueInterior(root.transform, label, center, sizes[i].x, sizes[i].y, npcController, out Vector3 spawn, out Vector3 exit);

                Bounds bounds = GetRendererBounds(building);
                Vector3 towardPlaza = CityCenter - building.transform.position;
                towardPlaza.y = 0f;
                if (towardPlaza.sqrMagnitude < 0.01f)
                    towardPlaza = -building.transform.forward;
                towardPlaza.Normalize();

                float offset = Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z) * 0.72f, 2.8f, 6.0f);
                Vector3 entrance = building.transform.position + towardPlaza * offset;
                entrance.y = 0.65f;
                Vector3 returnPoint = entrance + towardPlaza * 2.2f;
                returnPoint.y = 0.12f;

                CreateBuildingDoorTrigger(
                    "ENTRADA_" + label.Replace(" ", "_"),
                    entrance, label, "ENTRAR", spawn, Vector3.forward, false);

                CreateBuildingDoorTrigger(
                    "SALIDA_" + label.Replace(" ", "_"),
                    exit, label, "SALIR", returnPoint, towardPlaza, false);
            }
        }

        private static void CreateUniqueInterior(
            Transform parent,
            string label,
            Vector3 center,
            float width,
            float depth,
            RuntimeAnimatorController npcController,
            out Vector3 spawn,
            out Vector3 exit)
        {
            GameObject room = new GameObject("INTERIOR_" + label.Replace(" ", "_"));
            room.transform.SetParent(parent, false);

            Color floorColor = label == "TABERNA" || label == "GREMIO"
                ? new Color(0.26f,0.18f,0.12f,1f)
                : new Color(0.20f,0.22f,0.27f,1f);
            Material floorMat = CreateStandardMaterial(label + "_FloorMat", floorColor);
            Material wallMat = CreateStandardMaterial(label + "_WallMat", new Color(0.31f,0.32f,0.36f,1f));

            CreatePrimitiveBlock(label + "_Floor", room.transform, center + Vector3.down * 0.12f, new Vector3(width,0.22f,depth), floorMat, true);
            CreateRoomWalls(room.transform, label, center, width, depth, wallMat);

            float halfW = width * 0.5f;
            float halfD = depth * 0.5f;
            spawn = center + new Vector3(0f,0.12f,-halfD + 2.2f);
            exit = center + new Vector3(0f,0.75f,-halfD + 0.7f);

            if (label == "GREMIO")
            {
                PlaceInteriorProp(TableLongPath, room.transform, center + new Vector3(0f,0f,2f), Quaternion.identity, "Guild_Main_Table");
                PlaceInteriorProp(ShelfLargePath, room.transform, center + new Vector3(-halfW+1f,0f,3f), Quaternion.Euler(0f,90f,0f), "Guild_Records");
                PlaceInteriorProp(ChestPath, room.transform, center + new Vector3(halfW-1.2f,0f,3f), Quaternion.identity, "Guild_Reward_Chest");
                CreateInteriorPartition(room.transform, center + new Vector3(-4.2f,1.5f,1.5f), new Vector3(0.25f,3f,7f), wallMat, "Guild_Meeting_Wall");
                CreateCityNpc("Serin_Interior", AdventurerRoot + "/Characters/fbx/Knight.fbx", center + new Vector3(2.5f,0f,4.3f), room.transform, npcController);
            }
            else if (label == "TABERNA")
            {
                // Long bar + dining floor + sleeping annex: intentionally not the Guild shell.
                CreateInteriorPartition(room.transform, center + new Vector3(0f,1.25f,4.0f), new Vector3(width-2f,2.5f,0.45f), wallMat, "Tavern_Bar");
                for (int i = -2; i <= 2; i += 2)
                    PlaceInteriorProp(TableMediumPath, room.transform, center + new Vector3(i*2.0f,0f,0.2f), Quaternion.identity, "Tavern_Table_" + i);
                PlaceInteriorProp(BarrelPath, room.transform, center + new Vector3(halfW-1.4f,0f,4.8f), Quaternion.identity, "Tavern_Barrels");
                PlaceInteriorProp(BedPath, room.transform, center + new Vector3(-halfW+2.2f,0f,4.8f), Quaternion.Euler(0f,90f,0f), "Tavern_Guest_Bed");
                CreateCityNpc("Tabernera_Interior", AdventurerRoot + "/Characters/fbx/Rogue.fbx", center + new Vector3(1f,0f,5f), room.transform, npcController);
            }
            else if (label == "FORJA")
            {
                PlaceInteriorProp(TableMediumPath, room.transform, center + new Vector3(0f,0f,1.6f), Quaternion.identity, "Forge_Workbench");
                PlaceInteriorProp(SwordShieldPath, room.transform, center + new Vector3(-halfW+0.7f,1.3f,2f), Quaternion.Euler(0f,90f,0f), "Forge_Display");
                PlaceEnvironmentModel(WeaponRackPropPath, room.transform, center + new Vector3(halfW-1.3f,0f,2.5f), Quaternion.Euler(0f,-90f,0f), Vector3.one*1.4f, "Forge_Rack");
                PlaceInteriorProp(CratesPath, room.transform, center + new Vector3(halfW-1.5f,0f,-1.5f), Quaternion.identity, "Forge_Stock");
                CreateTorch(room.transform, center + new Vector3(-halfW+0.3f,1.8f,3f), Quaternion.Euler(0f,90f,0f));
                CreateCityNpc("Herrero_Interior", AdventurerRoot + "/Characters/fbx/Barbarian.fbx", center + new Vector3(0f,0f,3.1f), room.transform, npcController);
            }
            else if (label == "SANTUARIO")
            {
                for (int z = -4; z <= 3; z += 3)
                    for (int x = -1; x <= 1; x += 2)
                        PlaceInteriorProp(ChairPath, room.transform, center + new Vector3(x*2f,0f,z), Quaternion.identity, "Sanctuary_Seat_" + x + "_" + z);
                CreatePrimitiveBlock("Sanctuary_Dais", room.transform, center + new Vector3(0f,0.22f,halfD-2f), new Vector3(6f,0.44f,3f), floorMat, true);
                PlaceInteriorProp(TableMediumPath, room.transform, center + new Vector3(0f,0.45f,halfD-2f), Quaternion.identity, "Sanctuary_Altar");
                CreateCityNpc("Santuario_Custodio", AdventurerRoot + "/Characters/fbx/Mage.fbx", center + new Vector3(0f,0f,halfD-3.4f), room.transform, npcController);
            }
            else if (label == "MERCADO")
            {
                for (int x = -2; x <= 2; x += 2)
                {
                    PlaceInteriorProp(TableMediumPath, room.transform, center + new Vector3(x*2.2f,0f,2f), Quaternion.identity, "Market_Stall_" + x);
                    PlaceInteriorProp(CratesPath, room.transform, center + new Vector3(x*2.2f,0f,-1.2f), Quaternion.identity, "Market_Crate_" + x);
                }
                CreateCityNpc("Mercader_Interior", AdventurerRoot + "/Characters/fbx/RogueHooded.fbx", center + new Vector3(0f,0f,4f), room.transform, npcController);
            }
            else if (label == "ARCHIVO")
            {
                PlaceInteriorProp(ShelfLargePath, room.transform, center + new Vector3(-halfW+1f,0f,2f), Quaternion.Euler(0f,90f,0f), "Archive_Shelf_L");
                PlaceInteriorProp(ShelfLargePath, room.transform, center + new Vector3(halfW-1f,0f,2f), Quaternion.Euler(0f,-90f,0f), "Archive_Shelf_R");
                PlaceInteriorProp(TableLongPath, room.transform, center + new Vector3(0f,0f,-1f), Quaternion.identity, "Archive_Reading_Table");
                CreatePrimitiveBlock("Archive_Gallery", room.transform, center + new Vector3(0f,1.2f,halfD-3f), new Vector3(width-3f,0.3f,5f), floorMat, true);
                CreateSimpleStairs(room.transform, center + new Vector3(-4f,0f,halfD-5.6f), 6, 1.2f, floorMat, "Archive_Stairs");
                CreateCityNpc("Erudita_Interior", AdventurerRoot + "/Characters/fbx/Mage.fbx", center + new Vector3(0f,0f,3.2f), room.transform, npcController);
            }
            else if (label == "ACADEMIA")
            {
                CreatePrimitiveBlock("Academy_Ring", room.transform, center + new Vector3(0f,0.05f,1f), new Vector3(10f,0.10f,9f), floorMat, false);
                PlaceEnvironmentModel(TargetPropPath, room.transform, center + new Vector3(-4f,0f,4f), Quaternion.identity, Vector3.one*1.6f, "Academy_Target_A");
                PlaceEnvironmentModel(TargetPropPath, room.transform, center + new Vector3(4f,0f,4f), Quaternion.identity, Vector3.one*1.6f, "Academy_Target_B");
                PlaceEnvironmentModel(WeaponRackPropPath, room.transform, center + new Vector3(halfW-1.5f,0f,-2f), Quaternion.Euler(0f,-90f,0f), Vector3.one*1.6f, "Academy_Rack");
                CreateCityNpc("Maestro_Academia_Interior", AdventurerRoot + "/Characters/fbx/Knight.fbx", center + new Vector3(0f,0f,5.2f), room.transform, npcController);
            }
            else if (label == "TORRE")
            {
                CreatePrimitiveBlock("Tower_Central_Dais", room.transform, center + new Vector3(0f,0.28f,1.2f), new Vector3(5.5f,0.55f,5.5f), floorMat, true);
                PlaceInteriorProp(ChestPath, room.transform, center + new Vector3(0f,0.55f,1.2f), Quaternion.identity, "Tower_Trial_Chest");
                CreateSimpleStairs(room.transform, center + new Vector3(3.4f,0f,2.7f), 7, 1.8f, floorMat, "Tower_Stairs");
            }
            else if (label == "MINA")
            {
                CreateInteriorPartition(room.transform, center + new Vector3(-3f,1.3f,2f), new Vector3(0.5f,2.6f,6f), wallMat, "Mine_Tunnel_Divider");
                PlaceInteriorProp(CratesPath, room.transform, center + new Vector3(3.5f,0f,2f), Quaternion.identity, "Mine_Crates");
                PlaceEnvironmentModel(StonePropPath, room.transform, center + new Vector3(-4.5f,0f,3f), Quaternion.identity, Vector3.one*2f, "Mine_Ore");
                CreateTorch(room.transform, center + new Vector3(halfW-0.4f,1.8f,2f), Quaternion.Euler(0f,-90f,0f));
            }
            else
            {
                PlaceInteriorProp(TableLongPath, room.transform, center + new Vector3(0f,0f,1f), Quaternion.identity, "Nexus_Command_Table");
                PlaceInteriorProp(ShelfLargePath, room.transform, center + new Vector3(-halfW+1.2f,0f,3f), Quaternion.Euler(0f,90f,0f), "Nexus_Archive");
                PlaceInteriorProp(ChestPath, room.transform, center + new Vector3(halfW-1.4f,0f,3f), Quaternion.identity, "Nexus_Vault");
                CreatePrimitiveBlock("Nexus_Command_Dais", room.transform, center + new Vector3(0f,0.2f,5f), new Vector3(8f,0.4f,4f), floorMat, true);
            }

            CreateAccentLight(
                "InteriorLight_" + label.Replace(" ", "_"),
                center + Vector3.up * 3.2f,
                label == "SANTUARIO" ? new Color(0.76f,0.72f,1f,1f) : new Color(1f,0.74f,0.46f,1f),
                1.55f,
                Mathf.Max(width, depth) * 0.75f);
        }

        private static void CreateRoomWalls(Transform parent, string label, Vector3 center, float width, float depth, Material material)
        {
            const float h = 3.2f;
            const float thickness = 0.35f;
            float halfW = width * 0.5f;
            float halfD = depth * 0.5f;

            CreatePrimitiveBlock(label+"_NorthWall", parent, center + new Vector3(0f,h*0.5f,halfD), new Vector3(width,h,thickness), material, true);
            CreatePrimitiveBlock(label+"_EastWall", parent, center + new Vector3(halfW,h*0.5f,0f), new Vector3(thickness,h,depth), material, true);
            CreatePrimitiveBlock(label+"_WestWall", parent, center + new Vector3(-halfW,h*0.5f,0f), new Vector3(thickness,h,depth), material, true);

            float side = Mathf.Max(1f, (width - 3.2f) * 0.5f);
            CreatePrimitiveBlock(label+"_SouthWall_L", parent, center + new Vector3(-(side+3.2f)*0.25f,h*0.5f,-halfD), new Vector3(side,h,thickness), material, true);
            CreatePrimitiveBlock(label+"_SouthWall_R", parent, center + new Vector3((side+3.2f)*0.25f,h*0.5f,-halfD), new Vector3(side,h,thickness), material, true);
        }

        private static void CreateInteriorPartition(Transform parent, Vector3 position, Vector3 size, Material material, string name)
        {
            CreatePrimitiveBlock(name, parent, position, size, material, true);
        }

        private static void CreateSimpleStairs(Transform parent, Vector3 start, int steps, float height, Material material, string name)
        {
            float stepHeight = height / Mathf.Max(1, steps);
            for (int i = 0; i < steps; i++)
            {
                float y = stepHeight * (i + 0.5f);
                float z = i * 0.48f;
                CreatePrimitiveBlock(
                    name + "_" + i,
                    parent,
                    start + new Vector3(0f,y,z),
                    new Vector3(2.2f,stepHeight+0.03f,0.55f),
                    material,
                    true);
            }
        }

        private static void CreateReforgeDungeonEnemies(
            Transform dungeonRoot,
            ArenaMetrics arena,
            Transform player,
            HighflyHealth playerHealth)
        {
            float frontZ = Mathf.Max(5.8f, arena.halfDepth * 0.15f);
            float bossZ = Mathf.Max(11.5f, arena.halfDepth * 0.62f);

            string[] names =
            {
                "Cripta_Guerrero_01","Cripta_Picaro_01","Cripta_Minion_01",
                "Cripta_Guerrero_02","Cripta_Picaro_02","Cripta_Mago_01",
                "Cripta_Elite_01","Cripta_Elite_02","GUARDIAN_DEL_GATE"
            };
            string[] models =
            {
                SkeletonWarriorPath,SkeletonRoguePath,SkeletonMinionPath,
                SkeletonWarriorPath,SkeletonRoguePath,SkeletonMagePath,
                SkeletonWarriorPath,SkeletonRoguePath,SkeletonMagePath
            };
            Vector3[] local =
            {
                new Vector3(-2.7f,0.18f,frontZ),new Vector3(0f,0.18f,frontZ+1.4f),new Vector3(2.7f,0.18f,frontZ),
                new Vector3(-3.2f,0.18f,frontZ+7f),new Vector3(0.2f,0.18f,frontZ+7.8f),new Vector3(3.2f,0.18f,frontZ+7.1f),
                new Vector3(-2.8f,0.18f,bossZ-5f),new Vector3(2.8f,0.18f,bossZ-5f),new Vector3(0f,0.18f,bossZ)
            };

            for (int i = 0; i < names.Length; i++)
            {
                bool boss = i == names.Length - 1;
                float hp = boss ? 340f : (i >= 6 ? 145f : 78f + i*7f);
                float speed = boss ? 2.35f : 2.75f + (i%3)*0.22f;
                CreateEnemy(names[i], models[i], ReforgeDungeonCenter + local[i], hp, speed, player, playerHealth, boss, boss ? 1.45f : 1f);

                GameObject enemy = GameObject.Find(names[i]);
                if (enemy != null)
                    enemy.transform.SetParent(dungeonRoot, true);
            }
        }

        private static HighflyGateAnchor[] CreateGateAnchors(Transform worldRoot, Transform player, HighflyHealth playerHealth)
        {
            Vector3[] points =
            {
                new Vector3(0f,0f,30f),
                new Vector3(42f,0f,55f),
                new Vector3(-39f,0f,72f),
                new Vector3(18f,0f,108f)
            };
            string[] biomes = { "LLANURAS", "BOSQUE ASTER", "RUINAS", "ESTRIBACIONES" };
            string[] ranks = { "F,E,D", "E,D,C", "D,C,B", "C,B,A" };

            List<HighflyGateAnchor> result = new List<HighflyGateAnchor>();

            for (int i = 0; i < points.Length; i++)
            {
                GameObject anchorGo = new GameObject("GateAnchor_" + biomes[i].Replace(" ","_"));
                anchorGo.transform.SetParent(worldRoot, false);
                anchorGo.transform.position = points[i];

                GameObject breachRoot = new GameObject("BreachEnemies_" + i);
                breachRoot.transform.SetParent(worldRoot, false);

                string enemyA = "Breach_" + i + "_Warrior";
                string enemyB = "Breach_" + i + "_Rogue";

                CreateEnemy(enemyA, SkeletonWarriorPath, points[i] + new Vector3(-2.8f,0.18f,2.6f), 88f+i*18f, 2.8f, player, playerHealth, false, 1f);
                CreateEnemy(enemyB, i%2==0 ? SkeletonRoguePath : SkeletonMagePath, points[i] + new Vector3(2.8f,0.18f,2.8f), 78f+i*18f, 3.0f, player, playerHealth, false, 1f);

                GameObject a = GameObject.Find(enemyA);
                GameObject b = GameObject.Find(enemyB);
                if (a != null) a.transform.SetParent(breachRoot.transform, true);
                if (b != null) b.transform.SetParent(breachRoot.transform, true);
                breachRoot.SetActive(false);

                HighflyGateAnchor anchor = anchorGo.AddComponent<HighflyGateAnchor>();
                anchor.Configure(biomes[i], ranks[i], breachRoot);
                result.Add(anchor);
            }

            return result.ToArray();
        }

        private static Text CreateGateAlert(RectTransform canvas)
        {
            Text text = CreateText(
                "",
                canvas,
                new Vector2(0.5f,1f),
                new Vector2(1500f,70f),
                new Vector2(0f,-95f),
                25,
                TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.64f,0.88f,1f,1f);
            return text;
        }

        private static void CreateDayNightSystem()
        {
            Light sun = null;
            Light[] lights = UnityEngine.Object.FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    sun = lights[i];
                    break;
                }
            }

            if (sun == null)
                return;

            GameObject go = new GameObject("WORLD_DAY_NIGHT");
            HighflyDayNightCycle cycle = go.AddComponent<HighflyDayNightCycle>();
            cycle.Configure(sun, 10.5f);
        }

        private static void CreateWorldTitle(Transform root)
        {
            CreateWorldLabel("VALTHERA • CAPITAL HIGHFLY", CityCenter + new Vector3(0f,8f,0f), new Color(0.55f,0.85f,1f,1f));
            CreateWorldLabel("LLANURAS DE ASTER", new Vector3(0f,6f,42f), new Color(0.75f,0.92f,0.78f,1f));
            CreateWorldLabel("BOSQUE DE ASTER", new Vector3(43f,7f,76f), new Color(0.45f,0.95f,0.66f,1f));
            CreateWorldLabel("RUINAS DEL NORTE", new Vector3(-39f,7f,88f), new Color(0.72f,0.74f,0.88f,1f));
        }

        private static GameObject CreatePrimitiveBlock(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 size,
            Material material,
            bool keepCollider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = size;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;

            Collider collider = go.GetComponent<Collider>();
            if (!keepCollider && collider != null)
                UnityEngine.Object.DestroyImmediate(collider);

            return go;
        }
    }
}
#endif
