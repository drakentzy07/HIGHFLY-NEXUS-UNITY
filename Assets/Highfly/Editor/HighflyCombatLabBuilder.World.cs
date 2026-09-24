#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Highfly.World;
using Highfly.UI;
using Highfly.Combat;
using Highfly.Mobile;
using Highfly.Core;

namespace Highfly.Editor
{
    public static partial class HighflyCombatLabBuilder
    {
        public const string WorldScenePath = "Assets/Highfly/Scenes/WorldLab.unity";

        [MenuItem("HIGHFLY/Build World Lab Scene")]
        public static void BuildOrRefreshWorldLabShell()
        {
            if (CanBuildRgPolyWorld())
            {
                BuildOrRefreshRgPolyWorld();
                return;
            }

            Directory.CreateDirectory("Assets/Highfly/Scenes");
            Directory.CreateDirectory(GeneratedRoot);
            Directory.CreateDirectory(GeneratedUiRoot);
            Directory.CreateDirectory(GeneratedControllersRoot);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HIGHFLY_WORLD_LAB";

            ConfigureEnvironment();
            EnsureEventSystem();

            GameObject player = CreatePlayer(
                out HighflyThirdPersonMotor motor,
                out HighflyTargetingSystem targeting,
                out HighflyCombatController combat,
                out HighflyPlayerResources resources,
                out Transform attackOrigin,
                out Animator playerAnimator);

            Camera camera = CreateCamera(player.transform, targeting, out HighflyThirdPersonCamera cameraRig);
            Sprite circleSprite = CreateCircleSprite();

            GameObject canvas = CreateMobileHUD(
                motor, targeting, combat, resources, cameraRig, circleSprite,
                out HighflyVirtualJoystick joystick, out HighflyCameraLookArea lookArea);

            HighflyInteractionController interaction = player.GetComponent<HighflyInteractionController>();
            CreateInteractionUI(interaction, canvas.GetComponent<RectTransform>(), circleSprite);

            SetObjectReference(motor, "cameraTransform", camera.transform);
            SetObjectReference(motor, "movementJoystick", joystick);
            SetObjectReference(motor, "animator", playerAnimator);
            SetObjectReference(cameraRig, "lookArea", lookArea);
            SetObjectReference(cameraRig, "targeting", targeting);

            // Keep only the movement shell. The runtime hides combat presentation,
            // but the motor and Dash remain available for world traversal.
            GameObject hunterSword = GameObject.Find("Hunter_Sword");
            if (hunterSword != null)
                hunterSword.SetActive(false);

            HighflyWorldSafety safety = player.GetComponent<HighflyWorldSafety>();
            if (safety == null)
                safety = player.AddComponent<HighflyWorldSafety>();
            safety.Configure(Vector3.zero, -100f, true, true);

            GameObject runtime = new GameObject("HIGHFLY_VILLAGE_WORLD_RUNTIME");
            runtime.AddComponent<HighflyVillageWorldRuntime>();

            EditorSceneManager.SaveScene(scene, WorldScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(WorldScenePath, true) };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HIGHFLY WORLD LAB shell generated at " + WorldScenePath +
                      " | clean village runtime | no legacy crypt/dungeon/enemy shell baked into the scene.");
        }

        private static void CreateInteractionUI(
            HighflyInteractionController interaction,
            RectTransform canvasRect,
            Sprite circleSprite)
        {
            if (interaction == null || canvasRect == null)
                return;

            GameObject promptPanel = CreateImage(
                "InteractionPrompt",
                canvasRect,
                new Color(0.025f, 0.045f, 0.10f, 0.94f),
                circleSprite);
            RectTransform promptRect = promptPanel.GetComponent<RectTransform>();
            SetAnchored(
                promptRect,
                new Vector2(0.5f, 0f),
                new Vector2(720f, 112f),
                new Vector2(0f, 145f),
                new Vector2(0.5f, 0.5f));

            Button interactButton = promptPanel.AddComponent<Button>();
            ColorBlock promptColors = interactButton.colors;
            promptColors.normalColor = Color.white;
            promptColors.highlightedColor = new Color(0.92f, 1f, 1f, 1f);
            promptColors.pressedColor = new Color(0.55f, 0.85f, 1f, 1f);
            interactButton.colors = promptColors;

            Text promptText = CreateText(
                "INTERACTUAR",
                promptRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(650f, 82f),
                Vector2.zero,
                27,
                TextAnchor.MiddleCenter);
            promptText.fontStyle = FontStyle.Bold;
            promptText.color = Cyan;

            GameObject dialoguePanel = CreateImage(
                "DialoguePanel",
                canvasRect,
                new Color(0.015f, 0.024f, 0.065f, 0.97f),
                null);
            RectTransform dialogueRect = dialoguePanel.GetComponent<RectTransform>();
            SetAnchored(
                dialogueRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(1420f, 520f),
                new Vector2(0f, -30f),
                new Vector2(0.5f, 0.5f));

            Text dialogueTitle = CreateText(
                "NPC",
                dialogueRect,
                new Vector2(0.5f, 1f),
                new Vector2(1260f, 76f),
                new Vector2(0f, -62f),
                34,
                TextAnchor.MiddleLeft);
            dialogueTitle.fontStyle = FontStyle.Bold;
            dialogueTitle.color = Cyan;

            Text dialogueBody = CreateText(
                "",
                dialogueRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(1260f, 260f),
                new Vector2(0f, 0f),
                27,
                TextAnchor.UpperLeft);
            dialogueBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            dialogueBody.verticalOverflow = VerticalWrapMode.Overflow;
            dialogueBody.color = new Color(0.90f, 0.93f, 1f, 1f);

            GameObject closeGo = CreateImage(
                "CloseDialogue",
                dialogueRect,
                new Color(0.09f, 0.17f, 0.30f, 0.98f),
                circleSprite);
            RectTransform closeRect = closeGo.GetComponent<RectTransform>();
            SetAnchored(
                closeRect,
                new Vector2(0.5f, 0f),
                new Vector2(370f, 86f),
                new Vector2(0f, 64f),
                new Vector2(0.5f, 0.5f));

            Button closeButton = closeGo.AddComponent<Button>();
            Text closeText = CreateText(
                "CERRAR",
                closeRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(320f, 60f),
                Vector2.zero,
                25,
                TextAnchor.MiddleCenter);
            closeText.fontStyle = FontStyle.Bold;

            dialoguePanel.SetActive(false);

            SetObjectReference(interaction, "promptText", promptText);
            SetObjectReference(interaction, "promptPanel", promptPanel);
            SetObjectReference(interaction, "interactButton", interactButton);
            SetObjectReference(interaction, "dialoguePanel", dialoguePanel);
            SetObjectReference(interaction, "dialogueTitle", dialogueTitle);
            SetObjectReference(interaction, "dialogueBody", dialogueBody);
            SetObjectReference(interaction, "closeDialogueButton", closeButton);
            SetFloat(interaction, "interactionRange", 3.4f);
        }

        private static float GetCityBuildingTargetHeight(string label)
        {
            string value = (label ?? string.Empty).ToUpperInvariant();

            if (value.Contains("TORRE"))
                return 15.5f;
            if (value.Contains("NEXUS"))
                return 13.5f;
            if (value.Contains("SANTUARIO"))
                return 10.5f;
            if (value.Contains("GREMIO"))
                return 9.2f;
            if (value.Contains("ACADEMIA"))
                return 8.8f;
            if (value.Contains("TABERNA"))
                return 8.2f;
            if (value.Contains("ARCHIVO"))
                return 8.2f;
            if (value.Contains("FORJA"))
                return 7.8f;
            if (value.Contains("MERCADO"))
                return 7.2f;
            if (value.Contains("MINA"))
                return 7.4f;

            return 7.6f;
        }

        private static void ConfigureNpcInteraction(HighflyNpcInteractable interactable, string rawName)
        {
            if (interactable == null)
                return;

            string key = (rawName ?? string.Empty).ToLowerInvariant();
            string name = rawName != null ? rawName.Replace("_", " ") : "NPC";
            string prompt = "HABLAR";
            string message = "Bienvenido a HIGHFLY.";
            bool restore = false;

            if (key.Contains("serin"))
            {
                name = "Serin";
                message =
                    "Cazador, el Portal F conduce a la Cripta del Guardián. " +
                    "Tu contrato actual es limpiar las cámaras y derrotar al Guardián. " +
                    "Volvé al Gremio cuando termines.";
            }
            else if (key.Contains("herrero"))
            {
                name = "Herrero Kael";
                message =
                    "La Forja está abierta. Todavía estamos preparando mejora y reparación, " +
                    "pero ya podés entrar al taller y revisar la zona.";
            }
            else if (key.Contains("erudita"))
            {
                name = "Erudita Lyra";
                message =
                    "El Archivo registra cada criatura encontrada. La Cripta F está infestada " +
                    "de esqueletos; observá sus patrones antes de lanzarte contra el Guardián.";
            }
            else if (key.Contains("tabernera"))
            {
                name = "Tabernera Mira";
                prompt = "DESCANSAR";
                message =
                    "Comé, recuperate y volvé al combate cuando estés listo. " +
                    "HP, MP y stamina restaurados.";
                restore = true;
            }
            else if (key.Contains("maestro"))
            {
                name = "Maestro de Academia";
                message =
                    "Usá ataque, pesado, bloqueo, dash y las tres habilidades. " +
                    "El objetivo es moverte libremente y golpear grupos sin depender de un target manual.";
            }
            else if (key.Contains("mercader"))
            {
                name = "Mercader Nia";
                message =
                    "El mercado todavía no consume oro, pero esta zona ya queda reservada " +
                    "para equipo, llaves, consumibles y contratos.";
            }
            else if (key.Contains("sacerd") || key.Contains("santuario"))
            {
                name = "Custodio del Santuario";
                prompt = "RECUPERAR";
                message = "El Santuario restaura tus recursos antes de una expedición.";
                restore = true;
            }

            interactable.Configure(name, prompt, message, restore);
        }

        private static void DisableEmbeddedWeaponRenderers(GameObject character)
        {
            if (character == null)
                return;

            string[] unwanted =
            {
                "crossbow", "quiver", "bow", "dagger", "staff", "wand",
                "shield", "axe", "mace", "hammer", "spear", "sword"
            };

            Renderer[] renderers = character.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                string n = renderer.gameObject.name.ToLowerInvariant();

                for (int j = 0; j < unwanted.Length; j++)
                {
                    if (!n.Contains(unwanted[j]))
                        continue;

                    renderer.enabled = false;
                    break;
                }
            }
        }

        private static void AddStairStepColliders(GameObject stairs)
        {
            if (stairs == null)
                return;

            MeshCollider[] imported = stairs.GetComponentsInChildren<MeshCollider>(true);
            for (int i = 0; i < imported.Length; i++)
                UnityEngine.Object.DestroyImmediate(imported[i]);

            Bounds bounds = GetRendererBounds(stairs);
            if (bounds.size.sqrMagnitude <= 0.001f)
                return;

            const int steps = 9;
            bool alongZ = bounds.size.z >= bounds.size.x;
            float run = alongZ ? bounds.size.z : bounds.size.x;
            float cross = alongZ ? bounds.size.x : bounds.size.z;
            float stepRun = run / steps;
            float stepHeight = Mathf.Max(0.08f, bounds.size.y / steps);

            Transform parent = stairs.transform.parent;
            for (int i = 0; i < steps; i++)
            {
                float t = (i + 0.5f) / steps;
                GameObject step = new GameObject(stairs.name + "_PhysicsStep_" + i);
                step.transform.SetParent(parent, true);

                Vector3 position = bounds.center;
                if (alongZ)
                    position.z = Mathf.Lerp(bounds.min.z, bounds.max.z, t);
                else
                    position.x = Mathf.Lerp(bounds.min.x, bounds.max.x, t);

                position.y = bounds.min.y + stepHeight * (i + 0.5f);
                step.transform.position = position;
                step.transform.rotation = Quaternion.identity;

                BoxCollider box = step.AddComponent<BoxCollider>();
                box.size = alongZ
                    ? new Vector3(cross * 0.92f, stepHeight + 0.06f, stepRun + 0.05f)
                    : new Vector3(stepRun + 0.05f, stepHeight + 0.06f, cross * 0.92f);
            }
        }

        private static void CreateBuildingInteriorsAndDoors(RuntimeAnimatorController npcController)
        {
            string[] buildings =
            {
                "GREMIO",
                "TABERNA",
                "FORJA",
                "SANTUARIO",
                "MERCADO",
                "ARCHIVO",
                "ACADEMIA",
                "TORRE",
                "MINA",
                "NEXUS CENTRAL"
            };

            GameObject interiorsRoot = new GameObject("BUILDING_INTERIORS");
            for (int i = 0; i < buildings.Length; i++)
            {
                string label = buildings[i];
                if (!CityBuildings.TryGetValue(label, out GameObject building) || building == null)
                    continue;

                Vector3 interiorCenter = new Vector3(220f + i * 24f, 0f, -20f);
                CreateInteriorRoom(
                    interiorsRoot.transform,
                    label,
                    interiorCenter,
                    npcController,
                    out Vector3 interiorSpawn,
                    out Vector3 interiorExitPoint);

                Bounds buildingBounds = GetRendererBounds(building);
                Vector3 plazaDirection = CityCenter - building.transform.position;
                plazaDirection.y = 0f;
                if (plazaDirection.sqrMagnitude < 0.01f)
                    plazaDirection = -building.transform.forward;
                plazaDirection.Normalize();

                float doorOffset = Mathf.Clamp(
                    Mathf.Max(buildingBounds.extents.x, buildingBounds.extents.z) * 0.72f,
                    2.8f,
                    5.5f);

                Vector3 cityDoorPoint = building.transform.position + plazaDirection * doorOffset;
                cityDoorPoint.y = 0.65f;
                Vector3 cityReturnPoint = cityDoorPoint + plazaDirection * 2.2f;
                cityReturnPoint.y = 0.12f;

                CreateBuildingDoorTrigger(
                    "ENTRADA_" + label.Replace(" ", "_"),
                    cityDoorPoint,
                    label,
                    "ENTRAR",
                    interiorSpawn,
                    Vector3.forward,
                    false);

                CreateBuildingDoorTrigger(
                    "SALIDA_" + label.Replace(" ", "_"),
                    interiorExitPoint,
                    label,
                    "SALIR",
                    cityReturnPoint,
                    plazaDirection,
                    false);
            }
        }

        private static void CreateInteriorRoom(
            Transform parent,
            string label,
            Vector3 center,
            RuntimeAnimatorController npcController,
            out Vector3 spawn,
            out Vector3 exitPoint)
        {
            GameObject room = new GameObject("INTERIOR_" + label.Replace(" ", "_"));
            room.transform.SetParent(parent, false);
            room.transform.position = center;

            const int countX = 4;
            const int countZ = 4;

            GameObject sample = InstantiateModel(FloorPath, room.transform, "FloorSample");
            Bounds sampleBounds = sample != null
                ? GetRendererBounds(sample)
                : new Bounds(center, new Vector3(3.2f, 0.2f, 3.2f));

            float tileX = Mathf.Clamp(sampleBounds.size.x, 2.5f, 6f);
            float tileZ = Mathf.Clamp(sampleBounds.size.z, 2.5f, 6f);
            if (sample != null)
                UnityEngine.Object.DestroyImmediate(sample);

            float width = tileX * countX;
            float depth = tileZ * countZ;
            float halfW = width * 0.5f;
            float halfD = depth * 0.5f;

            GameObject floorRoot = new GameObject("InteriorFloor");
            floorRoot.transform.SetParent(room.transform, false);

            for (int z = 0; z < countZ; z++)
            {
                for (int x = 0; x < countX; x++)
                {
                    float px = center.x + (x - (countX - 1) * 0.5f) * tileX;
                    float pz = center.z + (z - (countZ - 1) * 0.5f) * tileZ;

                    GameObject tile = InstantiateModel(
                        FloorPath,
                        floorRoot.transform,
                        "Floor_" + x + "_" + z);
                    if (tile == null)
                        continue;

                    tile.transform.position = new Vector3(px, 0f, pz);
                    MoveBottomToY(tile, 0f);
                }
            }

            GameObject physicsFloor = new GameObject("InteriorPhysicsFloor");
            physicsFloor.transform.SetParent(room.transform, true);
            physicsFloor.transform.position = new Vector3(center.x, -0.15f, center.z);
            BoxCollider floorCollider = physicsFloor.AddComponent<BoxCollider>();
            floorCollider.size = new Vector3(width + 0.8f, 0.3f, depth + 0.8f);

            for (int x = 0; x < countX; x++)
            {
                float px = center.x + (x - (countX - 1) * 0.5f) * tileX;

                PlaceEnvironmentModel(
                    WallPath,
                    room.transform,
                    new Vector3(px, 0f, center.z + halfD),
                    Quaternion.identity,
                    Vector3.one,
                    "NorthWall_" + x);

                string southPath = x == countX / 2 ? DoorwayPath : WallPath;
                PlaceEnvironmentModel(
                    southPath,
                    room.transform,
                    new Vector3(px, 0f, center.z - halfD),
                    Quaternion.Euler(0f, 180f, 0f),
                    Vector3.one,
                    "SouthWall_" + x);
            }

            for (int z = 1; z < countZ - 1; z++)
            {
                float pz = center.z + (z - (countZ - 1) * 0.5f) * tileZ;

                PlaceEnvironmentModel(
                    WallPath,
                    room.transform,
                    new Vector3(center.x + halfW, 0f, pz),
                    Quaternion.Euler(0f, 90f, 0f),
                    Vector3.one,
                    "EastWall_" + z);

                PlaceEnvironmentModel(
                    WallPath,
                    room.transform,
                    new Vector3(center.x - halfW, 0f, pz),
                    Quaternion.Euler(0f, -90f, 0f),
                    Vector3.one,
                    "WestWall_" + z);
            }

            spawn = new Vector3(center.x, 0.12f, center.z - halfD + 2.3f);
            exitPoint = new Vector3(center.x, 0.75f, center.z - halfD + 0.7f);

            DressInterior(room.transform, label, center, halfW, halfD, npcController);

            CreateAccentLight(
                "InteriorLight_" + label.Replace(" ", "_"),
                center + Vector3.up * 3.4f,
                label == "SANTUARIO" ? new Color(0.75f, 0.72f, 1f, 1f) : Cyan,
                2.1f,
                13f);
        }

        private static void DressInterior(
            Transform parent,
            string label,
            Vector3 center,
            float halfW,
            float halfD,
            RuntimeAnimatorController npcController)
        {
            string key = label.ToUpperInvariant();

            if (key == "GREMIO")
            {
                PlaceInteriorProp(TableLongPath, parent, center + new Vector3(0f, 0f, 1.8f), Quaternion.identity, "GuildTable");
                PlaceInteriorProp(ShelfLargePath, parent, center + new Vector3(-halfW + 1.0f, 0f, 2.5f), Quaternion.Euler(0f, 90f, 0f), "GuildShelf");
                PlaceInteriorProp(ChestPath, parent, center + new Vector3(halfW - 1.3f, 0f, 2.8f), Quaternion.identity, "GuildChest");
                CreateCityNpc("Serin_Interior", AdventurerRoot + "/Characters/fbx/Knight.fbx", center + new Vector3(0f, 0f, 3.4f), parent, npcController);
            }
            else if (key == "TABERNA")
            {
                PlaceInteriorProp(TableMediumPath, parent, center + new Vector3(-2.2f, 0f, 1.5f), Quaternion.identity, "TavernTableA");
                PlaceInteriorProp(TableMediumPath, parent, center + new Vector3(2.2f, 0f, 1.5f), Quaternion.identity, "TavernTableB");
                PlaceInteriorProp(StoolPath, parent, center + new Vector3(-2.2f, 0f, -0.1f), Quaternion.identity, "TavernStoolA");
                PlaceInteriorProp(FoodPath, parent, center + new Vector3(2.2f, 0f, 1.5f), Quaternion.identity, "TavernFood");
                PlaceInteriorProp(BarrelPath, parent, center + new Vector3(halfW - 1.3f, 0f, 2.7f), Quaternion.identity, "TavernBarrel");
                CreateCityNpc("Tabernera_Interior", AdventurerRoot + "/Characters/fbx/Rogue.fbx", center + new Vector3(0f, 0f, 3.1f), parent, npcController);
            }
            else if (key == "FORJA")
            {
                PlaceInteriorProp(TableMediumPath, parent, center + new Vector3(0f, 0f, 1.7f), Quaternion.identity, "ForgeBench");
                PlaceInteriorProp(SwordShieldPath, parent, center + new Vector3(-halfW + 0.5f, 1.4f, 2.0f), Quaternion.Euler(0f, 90f, 0f), "ForgeWeapons");
                PlaceInteriorProp(CratesPath, parent, center + new Vector3(halfW - 1.4f, 0f, 2.4f), Quaternion.identity, "ForgeCrates");
                CreateCityNpc("Herrero_Interior", AdventurerRoot + "/Characters/fbx/Barbarian.fbx", center + new Vector3(0f, 0f, 3.1f), parent, npcController);
            }
            else if (key == "SANTUARIO")
            {
                for (int i = -1; i <= 1; i++)
                {
                    PlaceInteriorProp(ChairPath, parent, center + new Vector3(i * 1.5f, 0f, 0.8f), Quaternion.identity, "SanctuarySeat_" + i);
                }
                PlaceInteriorProp(TableMediumPath, parent, center + new Vector3(0f, 0f, 3.0f), Quaternion.identity, "SanctuaryAltar");
                CreateCityNpc("Santuario_Custodio", AdventurerRoot + "/Characters/fbx/Mage.fbx", center + new Vector3(0f, 0f, 3.9f), parent, npcController);
            }
            else if (key == "ARCHIVO")
            {
                PlaceInteriorProp(ShelfLargePath, parent, center + new Vector3(-halfW + 1.0f, 0f, 2.5f), Quaternion.Euler(0f, 90f, 0f), "ArchiveShelfA");
                PlaceInteriorProp(ShelfLargePath, parent, center + new Vector3(halfW - 1.0f, 0f, 2.5f), Quaternion.Euler(0f, -90f, 0f), "ArchiveShelfB");
                PlaceInteriorProp(TableLongPath, parent, center + new Vector3(0f, 0f, 1.1f), Quaternion.identity, "ArchiveDesk");
                CreateCityNpc("Erudita_Interior", AdventurerRoot + "/Characters/fbx/Mage.fbx", center + new Vector3(0f, 0f, 3.5f), parent, npcController);
            }
            else if (key == "ACADEMIA")
            {
                PlaceInteriorProp(TableMediumPath, parent, center + new Vector3(0f, 0f, 2.7f), Quaternion.identity, "AcademyDesk");
                PlaceInteriorProp(SwordShieldPath, parent, center + new Vector3(halfW - 0.6f, 1.4f, 1.7f), Quaternion.Euler(0f, -90f, 0f), "AcademyWeapons");
                CreateCityNpc("Maestro_Academia_Interior", AdventurerRoot + "/Characters/fbx/Knight.fbx", center + new Vector3(0f, 0f, 3.4f), parent, npcController);
            }
            else if (key == "MERCADO")
            {
                PlaceInteriorProp(TableLongPath, parent, center + new Vector3(0f, 0f, 1.5f), Quaternion.identity, "MarketCounter");
                PlaceInteriorProp(CratesPath, parent, center + new Vector3(-halfW + 1.2f, 0f, 2.4f), Quaternion.identity, "MarketCrates");
                PlaceInteriorProp(BarrelPath, parent, center + new Vector3(halfW - 1.2f, 0f, 2.4f), Quaternion.identity, "MarketBarrels");
                CreateCityNpc("Mercader_Interior", AdventurerRoot + "/Characters/fbx/RogueHooded.fbx", center + new Vector3(0f, 0f, 3.4f), parent, npcController);
            }
            else
            {
                PlaceInteriorProp(TableLongPath, parent, center + new Vector3(0f, 0f, 1.7f), Quaternion.identity, "NexusTable");
                PlaceInteriorProp(ShelfLargePath, parent, center + new Vector3(-halfW + 1f, 0f, 2.4f), Quaternion.Euler(0f, 90f, 0f), "NexusShelf");
                PlaceInteriorProp(ChestPath, parent, center + new Vector3(halfW - 1.2f, 0f, 2.7f), Quaternion.identity, "NexusChest");
            }
        }

        private static void PlaceInteriorProp(
            string path,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            string name)
        {
            PlaceEnvironmentModel(path, parent, position, rotation, Vector3.one, name);
        }

        private static void CreateBuildingDoorTrigger(
            string objectName,
            Vector3 position,
            string buildingName,
            string actionPrompt,
            Vector3 destination,
            Vector3 facing,
            bool restore)
        {
            GameObject door = new GameObject(objectName);
            door.transform.position = position;

            SphereCollider trigger = door.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.25f;
            trigger.center = new Vector3(0f, 0.7f, 0f);

            HighflyBuildingDoor interactable = door.AddComponent<HighflyBuildingDoor>();
            interactable.Configure(
                buildingName,
                actionPrompt,
                destination,
                facing,
                restore);
        }
    }
}
#endif
