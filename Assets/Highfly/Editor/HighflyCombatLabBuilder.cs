#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Highfly.Combat;
using Highfly.Core;
using Highfly.Mobile;
using Highfly.UI;

namespace Highfly.Editor
{
    public static class HighflyCombatLabBuilder
    {
        public const string ScenePath = "Assets/Highfly/Scenes/CombatLab.unity";

        private const int EnemyLayer = 8;
        private const int EnemyMask = 1 << EnemyLayer;

        private const string GeneratedRoot = "Assets/Highfly/Generated";
        private const string GeneratedUiRoot = GeneratedRoot + "/UI";
        private const string GeneratedControllersRoot = GeneratedRoot + "/Controllers";

        private const string AdventurerRoot =
            "Assets/External/KayKit/Adventurers/addons/kaykit_character_pack_adventures";
        private const string SkeletonRoot =
            "Assets/External/KayKit/Skeletons/addons/kaykit_character_pack_skeletons";
        private const string DungeonRoot =
            "Assets/External/KayKit/Dungeon/addons/kaykit_dungeon_remastered";

        private const string PlayerModelPath = AdventurerRoot + "/Characters/fbx/RogueHooded.fbx";
        private const string PlayerWeaponPath = AdventurerRoot + "/Assets/fbx/sword_1handed.fbx";

        private const string SkeletonWarriorPath = SkeletonRoot + "/Characters/fbx/Skeleton_Warrior.fbx";
        private const string SkeletonRoguePath = SkeletonRoot + "/Characters/fbx/Skeleton_Rogue.fbx";
        private const string SkeletonMagePath = SkeletonRoot + "/Characters/fbx/Skeleton_Mage.fbx";
        private const string SkeletonMinionPath = SkeletonRoot + "/Characters/fbx/Skeleton_Minion.fbx";

        private const string DungeonAssetRoot = DungeonRoot + "/Assets/fbx";
        private const string FloorPath = DungeonAssetRoot + "/floor_tile_large.fbx";
        private const string WallPath = DungeonAssetRoot + "/wall.fbx";
        private const string WallCornerPath = DungeonAssetRoot + "/wall_corner.fbx";
        private const string DoorwayPath = DungeonAssetRoot + "/wall_doorway.fbx";
        private const string ColumnPath = DungeonAssetRoot + "/column.fbx";
        private const string StairsPath = DungeonAssetRoot + "/stairs_wide.fbx";
        private const string TorchPath = DungeonAssetRoot + "/torch_mounted.fbx";
        private const string BannerPath = DungeonAssetRoot + "/banner_patternC_blue.fbx";
        private const string ChestPath = DungeonAssetRoot + "/chest_gold.fbx";
        private const string CratesPath = DungeonAssetRoot + "/crates_stacked.fbx";
        private const string BarrelPath = DungeonAssetRoot + "/barrel_large_decorated.fbx";

        private static readonly Color Midnight = new Color(0.018f, 0.028f, 0.065f, 1f);
        private static readonly Color Panel = new Color(0.025f, 0.035f, 0.085f, 0.88f);
        private static readonly Color Cyan = new Color(0.12f, 0.82f, 1f, 1f);
        private static readonly Color Violet = new Color(0.55f, 0.28f, 1f, 1f);
        private static readonly Color Red = new Color(0.95f, 0.16f, 0.27f, 1f);

        [MenuItem("HIGHFLY/Build Combat Lab Scene")]
        public static void BuildOrRefreshCombatLab()
        {
            Directory.CreateDirectory("Assets/Highfly/Scenes");
            Directory.CreateDirectory(GeneratedRoot);
            Directory.CreateDirectory(GeneratedUiRoot);
            Directory.CreateDirectory(GeneratedControllersRoot);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Debug.Log("HIGHFLY visual assets: player=" + AssetExists(PlayerModelPath) +
                      ", skeleton=" + AssetExists(SkeletonWarriorPath) +
                      ", dungeon=" + AssetExists(FloorPath));

            Sprite circleSprite = CreateCircleSprite();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "HIGHFLY_COMBAT_LAB";

            ConfigureEnvironment();
            GameObject arenaRoot = new GameObject("ENVIRONMENT_KAYKIT_DUNGEON");
            ArenaMetrics arena = CreateDungeonArena(arenaRoot.transform);
            EnsureEventSystem();

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

            HighflyHealth playerHealth = player.GetComponent<HighflyHealth>();

            float frontZ = Mathf.Max(5.8f, arena.halfDepth * 0.15f);
            float bossZ = Mathf.Max(11.5f, arena.halfDepth * 0.62f);

            CreateEnemy(
                "Esqueleto_Guerrero",
                SkeletonWarriorPath,
                new Vector3(-2.4f, 0f, frontZ),
                80f, 2.7f, player.transform, playerHealth, false, 1f);

            CreateEnemy(
                "Esqueleto_Picaro",
                SkeletonRoguePath,
                new Vector3(0f, 0f, frontZ + 1.4f),
                68f, 3.2f, player.transform, playerHealth, false, 0.95f);

            CreateEnemy(
                "Esqueleto_Minion",
                SkeletonMinionPath,
                new Vector3(2.4f, 0f, frontZ),
                62f, 3.0f, player.transform, playerHealth, false, 0.93f);

            CreateEnemy(
                "GUARDIAN_DEL_PORTAL",
                SkeletonMagePath,
                new Vector3(0f, 0f, bossZ),
                280f, 2.25f, player.transform, playerHealth, true, 1.38f);

            CreateBossStageDressing(arenaRoot.transform, bossZ);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HIGHFLY Combat Lab V2 generated at " + ScenePath +
                      " | Player visual: " + (AssetExists(PlayerModelPath) ? "KayKit Rogue Hooded" : "fallback") +
                      " | Arena visual: " + (AssetExists(FloorPath) ? "KayKit Dungeon" : "fallback"));
        }

        private static GameObject CreatePlayer(
            out HighflyThirdPersonMotor motor,
            out HighflyTargetingSystem targeting,
            out HighflyCombatController combat,
            out HighflyPlayerResources resources,
            out Transform attackOrigin,
            out Animator animator)
        {
            GameObject player = new GameObject("HIGHFLY_Hunter");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.42f;
            cc.center = new Vector3(0f, 1f, 0f);
            cc.skinWidth = 0.06f;
            cc.stepOffset = 0.35f;

            HighflyHealth health = player.AddComponent<HighflyHealth>();
            SetFloat(health, "maxHealth", 160f);

            resources = player.AddComponent<HighflyPlayerResources>();
            motor = player.AddComponent<HighflyThirdPersonMotor>();
            targeting = player.AddComponent<HighflyTargetingSystem>();
            combat = player.AddComponent<HighflyCombatController>();
            HighflyCombatVfx vfx = player.AddComponent<HighflyCombatVfx>();

            GameObject visual = InstantiateModel(PlayerModelPath, player.transform, "Hunter_RogueHooded");
            if (visual == null)
                visual = CreateFallbackVisual(player.transform, "Hunter_Fallback", new Color(0.12f, 0.18f, 0.32f, 1f), 1f);

            NormalizeCharacterVisual(visual, 1.82f);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            animator = visual.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = visual.AddComponent<Animator>();

            AnimatorController playerController = CreatePlayerAnimatorController(PlayerModelPath);
            if (playerController != null)
                animator.runtimeAnimatorController = playerController;
            animator.applyRootMotion = false;

            TryAttachWeapon(visual.transform, PlayerWeaponPath);

            GameObject attack = new GameObject("AttackOrigin");
            attack.transform.SetParent(player.transform, false);
            attack.transform.localPosition = new Vector3(0f, 1.05f, 0.75f);
            attackOrigin = attack.transform;
            SetObjectReference(vfx, "origin", attackOrigin);
            SetObjectReference(combat, "vfx", vfx);

            HighflyHealthFeedback feedback = player.AddComponent<HighflyHealthFeedback>();
            SetObjectReference(feedback, "animator", animator);
            SetBool(feedback, "disableMovementOnDeath", false);

            return player;
        }

        private static Camera CreateCamera(
            Transform player,
            HighflyTargetingSystem targeting,
            out HighflyThirdPersonCamera rig)
        {
            GameObject go = new GameObject("HIGHFLY_Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 3.4f, -6.3f);

            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 54f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 220f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Midnight;
            camera.allowHDR = true;

            go.AddComponent<AudioListener>();

            rig = go.AddComponent<HighflyThirdPersonCamera>();
            SetObjectReference(rig, "followTarget", player);
            SetObjectReference(rig, "targeting", targeting);
            SetFloat(rig, "distance", 6.2f);
            SetFloat(rig, "height", 1.72f);
            SetFloat(rig, "pitch", 12f);

            return camera;
        }

        private static GameObject CreateMobileHUD(
            HighflyThirdPersonMotor motor,
            HighflyTargetingSystem targeting,
            HighflyCombatController combat,
            HighflyPlayerResources resources,
            HighflyThirdPersonCamera cameraRig,
            Sprite circleSprite,
            out HighflyVirtualJoystick joystick,
            out HighflyCameraLookArea lookArea)
        {
            GameObject canvasGo = new GameObject(
                "HIGHFLY_MobileHUD",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(3088f, 1440f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            // Camera look region is created first so all controls sit above it.
            GameObject lookGo = CreateImage(
                "CameraLookArea",
                canvasRect,
                new Color(0f, 0f, 0f, 0.001f),
                null);

            RectTransform lookRect = lookGo.GetComponent<RectTransform>();
            lookRect.anchorMin = new Vector2(0.40f, 0f);
            lookRect.anchorMax = new Vector2(1f, 1f);
            lookRect.offsetMin = Vector2.zero;
            lookRect.offsetMax = Vector2.zero;

            lookArea = lookGo.AddComponent<HighflyCameraLookArea>();
            SetObjectReference(cameraRig, "lookArea", lookArea);

            // Branding / player status panel.
            GameObject statusPanel = CreateImage("StatusPanel", canvasRect, Panel, null);
            RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
            SetAnchored(statusRect, new Vector2(0f, 1f), new Vector2(720f, 278f), new Vector2(380f, -165f), new Vector2(0.5f, 0.5f));

            Text title = CreateText(
                "HIGHFLY  //  COMBAT LAB",
                statusRect,
                new Vector2(0f, 1f),
                new Vector2(620f, 54f),
                new Vector2(350f, -38f),
                32,
                TextAnchor.MiddleLeft);
            title.color = Cyan;
            title.fontStyle = FontStyle.Bold;

            Text rank = CreateText(
                "CAZADOR  •  RANGO F  •  LV. 01",
                statusRect,
                new Vector2(0f, 1f),
                new Vector2(620f, 42f),
                new Vector2(350f, -78f),
                23,
                TextAnchor.MiddleLeft);
            rank.color = new Color(0.75f, 0.78f, 0.92f, 1f);

            Image hpFill = CreateResourceBar(statusRect, "HP_BAR", new Vector2(350f, -126f), new Color(0.96f, 0.12f, 0.28f, 1f));
            Image mpFill = CreateResourceBar(statusRect, "MP_BAR", new Vector2(350f, -178f), new Color(0.14f, 0.55f, 1f, 1f));
            Image staminaFill = CreateResourceBar(statusRect, "STA_BAR", new Vector2(350f, -230f), new Color(0.12f, 0.92f, 0.72f, 1f));

            Text hp = CreateText("HP", statusRect, new Vector2(0f, 1f), new Vector2(570f, 38f), new Vector2(350f, -126f), 22, TextAnchor.MiddleLeft);
            Text mp = CreateText("MP", statusRect, new Vector2(0f, 1f), new Vector2(570f, 38f), new Vector2(350f, -178f), 22, TextAnchor.MiddleLeft);
            Text stamina = CreateText("STA", statusRect, new Vector2(0f, 1f), new Vector2(570f, 38f), new Vector2(350f, -230f), 21, TextAnchor.MiddleLeft);

            // Target pill.
            GameObject targetPill = CreateImage("TargetPill", canvasRect, new Color(0.025f, 0.035f, 0.09f, 0.86f), circleSprite);
            RectTransform targetPillRect = targetPill.GetComponent<RectTransform>();
            SetAnchored(targetPillRect, new Vector2(0.5f, 1f), new Vector2(760f, 92f), new Vector2(0f, -82f), new Vector2(0.5f, 0.5f));

            Text target = CreateText(
                "AUTO TARGET  •  BUSCANDO",
                targetPillRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(700f, 70f),
                Vector2.zero,
                27,
                TextAnchor.MiddleCenter);
            target.color = new Color(0.9f, 0.94f, 1f, 1f);

            // Joystick.
            GameObject joystickOuter = CreateImage(
                "MoveJoystick",
                canvasRect,
                new Color(0.03f, 0.05f, 0.11f, 0.72f),
                circleSprite);
            RectTransform bgRect = joystickOuter.GetComponent<RectTransform>();
            SetAnchored(bgRect, new Vector2(0f, 0f), new Vector2(390f, 390f), new Vector2(270f, 265f), new Vector2(0.5f, 0.5f));

            GameObject joystickRing = CreateImage(
                "JoystickRing",
                bgRect,
                new Color(Cyan.r, Cyan.g, Cyan.b, 0.16f),
                circleSprite);
            RectTransform ringRect = joystickRing.GetComponent<RectTransform>();
            SetAnchored(ringRect, new Vector2(0.5f, 0.5f), new Vector2(310f, 310f), Vector2.zero, new Vector2(0.5f, 0.5f));
            joystickRing.GetComponent<Image>().raycastTarget = false;

            GameObject handleGo = CreateImage(
                "Handle",
                bgRect,
                new Color(Cyan.r, Cyan.g, Cyan.b, 0.92f),
                circleSprite);
            RectTransform handleRect = handleGo.GetComponent<RectTransform>();
            SetAnchored(handleRect, new Vector2(0.5f, 0.5f), new Vector2(135f, 135f), Vector2.zero, new Vector2(0.5f, 0.5f));

            joystick = joystickOuter.AddComponent<HighflyVirtualJoystick>();
            SetObjectReference(joystick, "background", bgRect);
            SetObjectReference(joystick, "handle", handleRect);

            // Action cluster. Large attack, arc of three skills, dedicated utility buttons.
            CreateRoundButton(
                "ATAQUE", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(260f, 260f), new Vector2(-205f, 235f),
                new Color(0.12f, 0.24f, 0.42f, 0.94f), Cyan, 30, combat.BasicAttack);

            CreateRoundButton(
                "PESADO", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(170f, 170f), new Vector2(-470f, 175f),
                new Color(0.16f, 0.10f, 0.30f, 0.94f), Violet, 23, combat.HeavyAttack);

            CreateRoundButton(
                "DASH", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(166f, 166f), new Vector2(-660f, 185f),
                new Color(0.06f, 0.21f, 0.30f, 0.94f), Cyan, 24, combat.DashOrDodge);

            CreateRoundButton(
                "S1\nCORTE", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(182f, 182f), new Vector2(-600f, 400f),
                new Color(0.05f, 0.22f, 0.31f, 0.94f), Cyan, 22, combat.SkillLineCleave);

            CreateRoundButton(
                "S2\nABANICO", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(182f, 182f), new Vector2(-407f, 500f),
                new Color(0.16f, 0.10f, 0.33f, 0.94f), Violet, 21, combat.SkillCone);

            CreateRoundButton(
                "S3\nÁREA", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(182f, 182f), new Vector2(-190f, 530f),
                new Color(0.23f, 0.08f, 0.34f, 0.96f), Violet, 22, combat.SkillArea);

            CreateRoundHoldButton(
                "BLOQ", canvasRect, circleSprite,
                new Vector2(1f, 0f), new Vector2(150f, 150f), new Vector2(-835f, 220f),
                new Color(0.08f, 0.12f, 0.22f, 0.94f), new Color(0.72f, 0.8f, 1f, 1f), 21,
                combat.BeginBlock, combat.EndBlock);

            Text hint = CreateText(
                "ARRASTRÁ DERECHA PARA CÁMARA  •  AUTO-TARGET ACTIVO",
                canvasRect,
                new Vector2(0.5f, 0f),
                new Vector2(1260f, 48f),
                new Vector2(0f, 48f),
                19,
                TextAnchor.MiddleCenter);
            hint.color = new Color(0.62f, 0.68f, 0.82f, 0.75f);

            HighflyCombatHUD hud = canvasGo.AddComponent<HighflyCombatHUD>();
            SetObjectReference(hud, "resources", resources);
            SetObjectReference(hud, "targeting", targeting);
            SetObjectReference(hud, "hpText", hp);
            SetObjectReference(hud, "mpText", mp);
            SetObjectReference(hud, "staminaText", stamina);
            SetObjectReference(hud, "targetText", target);
            SetObjectReference(hud, "hpFill", hpFill);
            SetObjectReference(hud, "mpFill", mpFill);
            SetObjectReference(hud, "staminaFill", staminaFill);

            return canvasGo;
        }

        private static void CreateEnemy(
            string name,
            string modelPath,
            Vector3 position,
            float hp,
            float speed,
            Transform player,
            HighflyHealth playerHealth,
            bool boss,
            float visualScale)
        {
            GameObject enemy = new GameObject(name);
            enemy.layer = EnemyLayer;
            enemy.transform.position = position;

            CharacterController cc = enemy.AddComponent<CharacterController>();
            cc.height = boss ? 2.65f : 2f;
            cc.radius = boss ? 0.58f : 0.42f;
            cc.center = new Vector3(0f, boss ? 1.32f : 1f, 0f);
            cc.skinWidth = 0.05f;

            HighflyHealth health = enemy.AddComponent<HighflyHealth>();
            SetFloat(health, "maxHealth", hp);
            SetBool(health, "destroyOnDeath", true);
            SetFloat(health, "destroyDelay", boss ? 3.2f : 2.4f);

            HighflyTargetable targetable = enemy.AddComponent<HighflyTargetable>();
            SetObjectReference(targetable, "health", health);
            SetInt(targetable, "faction", 1);
            if (boss)
                SetFloat(targetable, "priorityBias", 0.15f);

            GameObject visual = InstantiateModel(modelPath, enemy.transform, name + "_Visual");
            if (visual == null)
                visual = CreateFallbackVisual(enemy.transform, name + "_Fallback", boss ? Violet : Red, boss ? 1.35f : 0.95f);

            NormalizeCharacterVisual(visual, (boss ? 2.3f : 1.85f) * visualScale);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            Animator animator = visual.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = visual.AddComponent<Animator>();

            AnimatorController controller = CreateEnemyAnimatorController(modelPath, name);
            if (controller != null)
                animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            HighflySimpleEnemyAI ai = enemy.AddComponent<HighflySimpleEnemyAI>();
            SetObjectReference(ai, "selfHealth", health);
            SetObjectReference(ai, "animator", animator);
            SetFloat(ai, "moveSpeed", speed);
            SetFloat(ai, "attackDamage", boss ? 18f : 8f);
            SetFloat(ai, "attackRange", boss ? 2.15f : 1.65f);
            SetFloat(ai, "detectionRange", boss ? 18f : 14f);
            ai.SetTarget(player, playerHealth);

            HighflyHealthFeedback feedback = enemy.AddComponent<HighflyHealthFeedback>();
            SetObjectReference(feedback, "animator", animator);

            CreateWorldHealthBar(enemy.transform, health, boss);
        }

        private static ArenaMetrics CreateDungeonArena(Transform root)
        {
            if (!AssetExists(FloorPath))
                return CreateFallbackArena(root);

            const int tileCountX = 7;
            const int tileCountZ = 8;

            GameObject sample = InstantiateModel(FloorPath, root, "FloorSample");
            if (sample == null)
                return CreateFallbackArena(root);

            Bounds sampleBounds = GetRendererBounds(sample);
            float tileX = Mathf.Clamp(sampleBounds.size.x, 2.2f, 8f);
            float tileZ = Mathf.Clamp(sampleBounds.size.z, 2.2f, 8f);
            UnityEngine.Object.DestroyImmediate(sample);

            float width = tileX * tileCountX;
            float depth = tileZ * tileCountZ;
            float halfWidth = width * 0.5f;
            float halfDepth = depth * 0.5f;

            GameObject floorRoot = new GameObject("Dungeon_Floor");
            floorRoot.transform.SetParent(root, false);

            for (int z = 0; z < tileCountZ; z++)
            {
                for (int x = 0; x < tileCountX; x++)
                {
                    float px = (x - (tileCountX - 1) * 0.5f) * tileX;
                    float pz = (z - (tileCountZ - 1) * 0.5f) * tileZ;

                    GameObject tile = InstantiateModel(FloorPath, floorRoot.transform, "Floor_" + x + "_" + z);
                    if (tile == null)
                        continue;

                    tile.transform.position = new Vector3(px, 0f, pz);
                    MoveBottomToY(tile, 0f);
                }
            }

            // Reliable physics floor, independent of FBX collider setup.
            GameObject floorCollider = new GameObject("ArenaFloorCollider");
            floorCollider.transform.SetParent(root, false);
            floorCollider.transform.position = new Vector3(0f, -0.12f, 0f);
            BoxCollider floorBox = floorCollider.AddComponent<BoxCollider>();
            floorBox.size = new Vector3(width, 0.25f, depth);

            CreatePerimeterWalls(root, tileX, tileZ, tileCountX, tileCountZ);

            // Invisible hard boundaries keep AI and player inside the presentation arena.
            CreateBoundary(root, "NorthBoundary", new Vector3(0f, 1.5f, halfDepth + 0.25f), new Vector3(width + 1f, 3f, 0.5f));
            CreateBoundary(root, "SouthBoundary", new Vector3(0f, 1.5f, -halfDepth - 0.25f), new Vector3(width + 1f, 3f, 0.5f));
            CreateBoundary(root, "EastBoundary", new Vector3(halfWidth + 0.25f, 1.5f, 0f), new Vector3(0.5f, 3f, depth + 1f));
            CreateBoundary(root, "WestBoundary", new Vector3(-halfWidth - 0.25f, 1.5f, 0f), new Vector3(0.5f, 3f, depth + 1f));

            DressDungeon(root, halfWidth, halfDepth, tileX, tileZ);

            return new ArenaMetrics
            {
                width = width,
                depth = depth,
                halfWidth = halfWidth,
                halfDepth = halfDepth
            };
        }

        private static void CreatePerimeterWalls(
            Transform root,
            float tileX,
            float tileZ,
            int tileCountX,
            int tileCountZ)
        {
            GameObject wallsRoot = new GameObject("Dungeon_Walls");
            wallsRoot.transform.SetParent(root, false);

            float halfWidth = tileX * tileCountX * 0.5f;
            float halfDepth = tileZ * tileCountZ * 0.5f;

            for (int x = 0; x < tileCountX; x++)
            {
                float px = (x - (tileCountX - 1) * 0.5f) * tileX;

                string northPath = x == tileCountX / 2 ? DoorwayPath : WallPath;
                PlaceEnvironmentModel(northPath, wallsRoot.transform, new Vector3(px, 0f, halfDepth), Quaternion.identity, Vector3.one, "NorthWall_" + x);

                PlaceEnvironmentModel(WallPath, wallsRoot.transform, new Vector3(px, 0f, -halfDepth), Quaternion.Euler(0f, 180f, 0f), Vector3.one, "SouthWall_" + x);
            }

            for (int z = 1; z < tileCountZ - 1; z++)
            {
                float pz = (z - (tileCountZ - 1) * 0.5f) * tileZ;

                PlaceEnvironmentModel(WallPath, wallsRoot.transform, new Vector3(halfWidth, 0f, pz), Quaternion.Euler(0f, 90f, 0f), Vector3.one, "EastWall_" + z);
                PlaceEnvironmentModel(WallPath, wallsRoot.transform, new Vector3(-halfWidth, 0f, pz), Quaternion.Euler(0f, -90f, 0f), Vector3.one, "WestWall_" + z);
            }

            PlaceEnvironmentModel(WallCornerPath, wallsRoot.transform, new Vector3(halfWidth, 0f, halfDepth), Quaternion.Euler(0f, 90f, 0f), Vector3.one, "Corner_NE");
            PlaceEnvironmentModel(WallCornerPath, wallsRoot.transform, new Vector3(-halfWidth, 0f, halfDepth), Quaternion.identity, Vector3.one, "Corner_NW");
            PlaceEnvironmentModel(WallCornerPath, wallsRoot.transform, new Vector3(halfWidth, 0f, -halfDepth), Quaternion.Euler(0f, 180f, 0f), Vector3.one, "Corner_SE");
            PlaceEnvironmentModel(WallCornerPath, wallsRoot.transform, new Vector3(-halfWidth, 0f, -halfDepth), Quaternion.Euler(0f, -90f, 0f), Vector3.one, "Corner_SW");
        }

        private static void DressDungeon(
            Transform root,
            float halfWidth,
            float halfDepth,
            float tileX,
            float tileZ)
        {
            GameObject props = new GameObject("Dungeon_Props");
            props.transform.SetParent(root, false);

            PlaceEnvironmentModel(ColumnPath, props.transform, new Vector3(-halfWidth + 1.1f, 0f, halfDepth - 2f), Quaternion.identity, Vector3.one, "Column_NW");
            PlaceEnvironmentModel(ColumnPath, props.transform, new Vector3(halfWidth - 1.1f, 0f, halfDepth - 2f), Quaternion.identity, Vector3.one, "Column_NE");
            PlaceEnvironmentModel(ColumnPath, props.transform, new Vector3(-halfWidth + 1.1f, 0f, 1f), Quaternion.identity, Vector3.one, "Column_W");
            PlaceEnvironmentModel(ColumnPath, props.transform, new Vector3(halfWidth - 1.1f, 0f, 1f), Quaternion.identity, Vector3.one, "Column_E");

            PlaceEnvironmentModel(BannerPath, props.transform, new Vector3(-halfWidth + 0.15f, 2.1f, halfDepth * 0.40f), Quaternion.Euler(0f, 90f, 0f), Vector3.one, "Banner_W");
            PlaceEnvironmentModel(BannerPath, props.transform, new Vector3(halfWidth - 0.15f, 2.1f, halfDepth * 0.40f), Quaternion.Euler(0f, -90f, 0f), Vector3.one, "Banner_E");

            PlaceEnvironmentModel(CratesPath, props.transform, new Vector3(-halfWidth + 2f, 0f, -halfDepth + 3f), Quaternion.Euler(0f, 18f, 0f), Vector3.one, "Crates_W");
            PlaceEnvironmentModel(BarrelPath, props.transform, new Vector3(halfWidth - 2.1f, 0f, -halfDepth + 2.7f), Quaternion.Euler(0f, -16f, 0f), Vector3.one, "Barrel_E");

            PlaceEnvironmentModel(StairsPath, props.transform, new Vector3(0f, 0f, halfDepth - tileZ * 0.65f), Quaternion.Euler(0f, 180f, 0f), Vector3.one, "Boss_Stairs");

            CreateTorch(props.transform, new Vector3(-halfWidth + 0.2f, 2.1f, -halfDepth * 0.35f), Quaternion.Euler(0f, 90f, 0f));
            CreateTorch(props.transform, new Vector3(halfWidth - 0.2f, 2.1f, -halfDepth * 0.35f), Quaternion.Euler(0f, -90f, 0f));
            CreateTorch(props.transform, new Vector3(-halfWidth + 0.2f, 2.1f, halfDepth * 0.38f), Quaternion.Euler(0f, 90f, 0f));
            CreateTorch(props.transform, new Vector3(halfWidth - 0.2f, 2.1f, halfDepth * 0.38f), Quaternion.Euler(0f, -90f, 0f));
        }

        private static void CreateBossStageDressing(Transform root, float bossZ)
        {
            GameObject stage = new GameObject("Boss_Stage");
            stage.transform.SetParent(root, false);

            PlaceEnvironmentModel(ChestPath, stage.transform, new Vector3(2.4f, 0f, bossZ + 2.5f), Quaternion.Euler(0f, -25f, 0f), Vector3.one, "BossRewardChest");

            CreateAccentLight("Boss_Violet_Light", new Vector3(0f, 3.2f, bossZ + 1.2f), Violet, 3.8f, 11f);
            CreateAccentLight("Boss_Cyan_Light", new Vector3(0f, 1.2f, bossZ - 2.2f), Cyan, 1.7f, 8f);
        }

        private static void ConfigureEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.11f, 0.14f, 0.23f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.025f, 0.038f, 0.085f, 1f);
            RenderSettings.fogDensity = 0.010f;

            GameObject lightGo = new GameObject("Moon_Key_Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.82f;
            light.color = new Color(0.67f, 0.78f, 1f, 1f);
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            CreateAccentLight("Cyan_Rim", new Vector3(-5f, 4.2f, 1f), Cyan, 2.2f, 13f);
            CreateAccentLight("Violet_Rim", new Vector3(5f, 3.8f, 7f), Violet, 2.0f, 13f);
        }

        private static void CreateAccentLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static void CreateTorch(Transform parent, Vector3 position, Quaternion rotation)
        {
            GameObject torch = PlaceEnvironmentModel(TorchPath, parent, position, rotation, Vector3.one, "Torch");
            if (torch == null)
                return;

            GameObject lightGo = new GameObject("TorchLight");
            lightGo.transform.SetParent(torch.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.35f, 0.12f);
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.48f, 0.18f, 1f);
            light.intensity = 1.65f;
            light.range = 6.5f;
            light.shadows = LightShadows.None;
        }

        private static ArenaMetrics CreateFallbackArena(Transform root)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "FallbackArenaFloor";
            ground.transform.SetParent(root, false);
            ground.transform.position = new Vector3(0f, -0.15f, 5f);
            ground.transform.localScale = new Vector3(24f, 0.3f, 32f);

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateStandardMaterial("FallbackGround", new Color(0.06f, 0.075f, 0.12f, 1f));

            CreateBoundary(root, "NorthBoundary", new Vector3(0f, 1.5f, 21f), new Vector3(24f, 3f, 0.5f));
            CreateBoundary(root, "SouthBoundary", new Vector3(0f, 1.5f, -11f), new Vector3(24f, 3f, 0.5f));
            CreateBoundary(root, "EastBoundary", new Vector3(12f, 1.5f, 5f), new Vector3(0.5f, 3f, 32f));
            CreateBoundary(root, "WestBoundary", new Vector3(-12f, 1.5f, 5f), new Vector3(0.5f, 3f, 32f));

            return new ArenaMetrics
            {
                width = 24f,
                depth = 32f,
                halfWidth = 12f,
                halfDepth = 16f
            };
        }

        private static void CreateBoundary(Transform parent, string name, Vector3 position, Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.size = size;
        }

        private static GameObject PlaceEnvironmentModel(
            string assetPath,
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string name)
        {
            GameObject go = InstantiateModel(assetPath, parent, name);
            if (go == null)
                return null;

            go.transform.position = position;
            go.transform.rotation = rotation;
            go.transform.localScale = Vector3.Scale(go.transform.localScale, scale);
            MoveBottomToY(go, position.y);
            return go;
        }

        private static GameObject InstantiateModel(string assetPath, Transform parent, string name)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null)
                return null;

            GameObject instance = UnityEngine.Object.Instantiate(source);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static GameObject CreateFallbackVisual(
            Transform parent,
            string name,
            Color color,
            float scale)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = Vector3.one * scale;

            Collider primitiveCollider = visual.GetComponent<Collider>();
            if (primitiveCollider != null)
                UnityEngine.Object.DestroyImmediate(primitiveCollider);

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateStandardMaterial(name + "_Material", color);

            return visual;
        }

        private static void NormalizeCharacterVisual(GameObject visual, float targetHeight)
        {
            if (visual == null)
                return;

            Bounds bounds = GetRendererBounds(visual);
            if (bounds.size.y <= 0.01f)
                return;

            float factor = targetHeight / bounds.size.y;
            visual.transform.localScale *= factor;

            bounds = GetRendererBounds(visual);
            float offsetY = -bounds.min.y;
            visual.transform.position += Vector3.up * offsetY;
        }

        private static void MoveBottomToY(GameObject go, float targetY)
        {
            if (go == null)
                return;

            Bounds bounds = GetRendererBounds(go);
            if (bounds.size.sqrMagnitude <= 0.001f)
                return;

            float delta = targetY - bounds.min.y;
            go.transform.position += Vector3.up * delta;
        }

        private static Bounds GetRendererBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(go.transform.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void TryAttachWeapon(Transform characterRoot, string weaponPath)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);
            if (source == null || characterRoot == null)
                return;

            Transform hand = FindRightHand(characterRoot);
            GameObject weapon = UnityEngine.Object.Instantiate(source);
            weapon.name = "Hunter_Sword";

            if (hand != null)
            {
                weapon.transform.SetParent(hand, false);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
            }
            else
            {
                weapon.transform.SetParent(characterRoot, false);
                weapon.transform.localPosition = new Vector3(0.38f, 0.95f, 0.12f);
                weapon.transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
            }

            TrailRenderer trail = weapon.AddComponent<TrailRenderer>();
            trail.time = 0.13f;
            trail.startWidth = 0.075f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.04f;
            trail.startColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.85f);
            trail.endColor = new Color(Violet.r, Violet.g, Violet.b, 0f);

            Shader trailShader = Shader.Find("Sprites/Default");
            if (trailShader != null)
                trail.sharedMaterial = new Material(trailShader);
        }

        private static Transform FindRightHand(Transform root)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);

            string[] exactHints =
            {
                "righthand", "right_hand", "hand_r", "hand.r", "r_hand",
                "weapon_r", "weapon.r", "handright"
            };

            for (int h = 0; h < exactHints.Length; h++)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    string normalized = all[i].name.ToLowerInvariant().Replace(" ", string.Empty);
                    if (normalized.Contains(exactHints[h]))
                        return all[i];
                }
            }

            for (int i = 0; i < all.Length; i++)
            {
                string n = all[i].name.ToLowerInvariant();
                if (n.Contains("hand") && (n.Contains("right") || n.EndsWith("_r") || n.EndsWith(".r")))
                    return all[i];
            }

            return null;
        }

        private static AnimatorController CreatePlayerAnimatorController(string modelPath)
        {
            AnimationClip[] clips = LoadUsableClips(modelPath);
            if (clips.Length == 0)
            {
                Debug.LogWarning("HIGHFLY: no animation clips found for player model " + modelPath);
                return null;
            }

            string path = GeneratedControllersRoot + "/HIGHFLY_Player.controller";
            DeleteAssetIfExists(path);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("ComboIndex", AnimatorControllerParameterType.Int);
            controller.AddParameter("BasicAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HeavyAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dash", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Skill1", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Skill2", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Skill3", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Blocking", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            AnimationClip idleClip = PickClip(clips, "idle");
            AnimationClip runClip = PickClip(clips, "run", "running", "sprint", "walk");
            AnimationClip attackClip = PickClip(clips, "attack", "slash", "melee");
            AnimationClip heavyClip = PickClip(clips, "heavy", "smash", "attack");
            AnimationClip dashClip = PickClip(clips, "roll", "dodge", "dash", "run");
            AnimationClip blockClip = PickClip(clips, "block", "defend", "idle");
            AnimationClip skill1Clip = PickClip(clips, "slash", "attack");
            AnimationClip skill2Clip = PickClip(clips, "spin", "attack");
            AnimationClip skill3Clip = PickClip(clips, "spell", "cast", "attack");

            AnimatorState idle = AddState(sm, "Idle", idleClip ?? clips[0]);
            sm.defaultState = idle;

            AnimatorState run = AddState(sm, "Run", runClip ?? idle.motion as AnimationClip);
            AnimatorStateTransition toRun = idle.AddTransition(run);
            toRun.hasExitTime = false;
            toRun.duration = 0.12f;
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.12f, "MoveSpeed");

            AnimatorStateTransition toIdle = run.AddTransition(idle);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.12f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.12f, "MoveSpeed");

            AddTriggeredState(sm, idle, "Basic_Attack", attackClip ?? idleClip ?? clips[0], "BasicAttack", 0.78f);
            AddTriggeredState(sm, idle, "Heavy_Attack", heavyClip ?? attackClip ?? clips[0], "HeavyAttack", 0.82f);
            AddTriggeredState(sm, idle, "Dash", dashClip ?? runClip ?? clips[0], "Dash", 0.76f);
            AddTriggeredState(sm, idle, "Skill_1", skill1Clip ?? attackClip ?? clips[0], "Skill1", 0.82f);
            AddTriggeredState(sm, idle, "Skill_2", skill2Clip ?? attackClip ?? clips[0], "Skill2", 0.82f);
            AddTriggeredState(sm, idle, "Skill_3", skill3Clip ?? attackClip ?? clips[0], "Skill3", 0.82f);

            AnimatorState block = AddState(sm, "Block", blockClip ?? idleClip ?? clips[0]);
            AnimatorStateTransition enterBlock = sm.AddAnyStateTransition(block);
            enterBlock.hasExitTime = false;
            enterBlock.duration = 0.08f;
            enterBlock.canTransitionToSelf = false;
            enterBlock.AddCondition(AnimatorConditionMode.If, 0f, "Blocking");

            AnimatorStateTransition leaveBlock = block.AddTransition(idle);
            leaveBlock.hasExitTime = false;
            leaveBlock.duration = 0.10f;
            leaveBlock.AddCondition(AnimatorConditionMode.IfNot, 0f, "Blocking");

            Debug.Log("HIGHFLY player clips: " + string.Join(", ", clips.Select(c => c.name).ToArray()));
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimatorController CreateEnemyAnimatorController(string modelPath, string enemyName)
        {
            AnimationClip[] clips = LoadUsableClips(modelPath);
            if (clips.Length == 0)
            {
                Debug.LogWarning("HIGHFLY: no animation clips found for enemy model " + modelPath);
                return null;
            }

            string safeName = new string(enemyName.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
            string path = GeneratedControllersRoot + "/" + safeName + ".controller";
            DeleteAssetIfExists(path);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            AnimationClip idleClip = PickClip(clips, "idle") ?? clips[0];
            AnimationClip runClip = PickClip(clips, "run", "running", "walk") ?? idleClip;
            AnimationClip attackClip = PickClip(clips, "attack", "slash", "melee") ?? idleClip;
            AnimationClip hitClip = PickClip(clips, "hit", "damage", "impact") ?? idleClip;
            AnimationClip deathClip = PickClip(clips, "death", "die", "defeat") ?? hitClip;

            AnimatorState idle = AddState(sm, "Idle", idleClip);
            sm.defaultState = idle;
            AnimatorState run = AddState(sm, "Run", runClip);

            AnimatorStateTransition toRun = idle.AddTransition(run);
            toRun.hasExitTime = false;
            toRun.duration = 0.12f;
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.12f, "MoveSpeed");

            AnimatorStateTransition toIdle = run.AddTransition(idle);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.12f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.12f, "MoveSpeed");

            AddTriggeredState(sm, idle, "Attack", attackClip, "Attack", 0.80f);
            AddTriggeredState(sm, idle, "Hit", hitClip, "Hit", 0.65f);

            AnimatorState death = AddState(sm, "Death", deathClip);
            AnimatorStateTransition deathTransition = sm.AddAnyStateTransition(death);
            deathTransition.hasExitTime = false;
            deathTransition.duration = 0.06f;
            deathTransition.canTransitionToSelf = false;
            deathTransition.AddCondition(AnimatorConditionMode.If, 0f, "Death");

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine sm, string name, Motion motion)
        {
            AnimatorState state = sm.AddState(name);
            state.motion = motion;
            state.speed = 1f;
            return state;
        }

        private static void AddTriggeredState(
            AnimatorStateMachine sm,
            AnimatorState returnState,
            string stateName,
            Motion motion,
            string trigger,
            float exitTime)
        {
            AnimatorState state = AddState(sm, stateName, motion);

            AnimatorStateTransition enter = sm.AddAnyStateTransition(state);
            enter.hasExitTime = false;
            enter.duration = 0.05f;
            enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);

            AnimatorStateTransition leave = state.AddTransition(returnState);
            leave.hasExitTime = true;
            leave.exitTime = exitTime;
            leave.duration = 0.08f;
        }

        private static AnimationClip[] LoadUsableClips(string modelPath)
        {
            if (!AssetExists(modelPath))
                return new AnimationClip[0];

            return AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<AnimationClip>()
                .Where(c => c != null && !c.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        private static AnimationClip PickClip(AnimationClip[] clips, params string[] hints)
        {
            if (clips == null || clips.Length == 0)
                return null;

            for (int h = 0; h < hints.Length; h++)
            {
                string hint = hints[h];
                AnimationClip exact = clips.FirstOrDefault(
                    c => c.name.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0);
                if (exact != null)
                    return exact;
            }

            return null;
        }

        private static Sprite CreateCircleSprite()
        {
            string pngPath = GeneratedUiRoot + "/circle.png";
            const int size = 128;

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "HIGHFLY_Circle";
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.49f;
            float feather = 2.5f;

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01((radius - distance) / feather);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(pngPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        }

        private static Image CreateResourceBar(
            RectTransform parent,
            string name,
            Vector2 position,
            Color color)
        {
            GameObject background = CreateImage(name + "_BG", parent, new Color(0.06f, 0.07f, 0.12f, 0.95f), null);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            SetAnchored(bgRect, new Vector2(0f, 1f), new Vector2(570f, 34f), position, new Vector2(0.5f, 0.5f));

            GameObject fillGo = CreateImage(name + "_FILL", bgRect, color, null);
            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);

            Image fill = fillGo.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;

            return fill;
        }

        private static void CreateWorldHealthBar(Transform enemy, HighflyHealth health, bool boss)
        {
            GameObject canvasGo = new GameObject("WorldHealthBar", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            canvasGo.transform.SetParent(enemy, false);
            canvasGo.transform.localPosition = new Vector3(0f, boss ? 3.15f : 2.35f, 0f);
            canvasGo.transform.localScale = Vector3.one * (boss ? 0.012f : 0.009f);

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 30;

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.sizeDelta = boss ? new Vector2(260f, 34f) : new Vector2(190f, 26f);

            GameObject bg = CreateImage("HP_BG", canvasRect, new Color(0.02f, 0.02f, 0.04f, 0.94f), null);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            GameObject fillGo = CreateImage("HP_FILL", bgRect, boss ? Violet : Red, null);
            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);

            Image fill = fillGo.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;

            HighflyWorldHealthBar bar = canvasGo.AddComponent<HighflyWorldHealthBar>();
            SetObjectReference(bar, "health", health);
            SetObjectReference(bar, "fill", fill);
            SetObjectReference(bar, "canvasGroup", canvasGo.GetComponent<CanvasGroup>());
        }

        private static Button CreateRoundButton(
            string label,
            RectTransform parent,
            Sprite sprite,
            Vector2 anchor,
            Vector2 size,
            Vector2 position,
            Color background,
            Color accent,
            int fontSize,
            UnityAction action)
        {
            GameObject go = CreateImage(label.Replace("\n", "_") + "_Button", parent, background, sprite);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchored(rect, anchor, size, position, new Vector2(0.5f, 0.5f));

            GameObject accentGo = CreateImage("Accent", rect, new Color(accent.r, accent.g, accent.b, 0.22f), sprite);
            RectTransform accentRect = accentGo.GetComponent<RectTransform>();
            accentRect.anchorMin = Vector2.zero;
            accentRect.anchorMax = Vector2.one;
            accentRect.offsetMin = new Vector2(10f, 10f);
            accentRect.offsetMax = new Vector2(-10f, -10f);
            accentGo.GetComponent<Image>().raycastTarget = false;

            Button button = go.AddComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, action);

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.72f, 0.82f, 1f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.35f, 0.35f, 0.4f, 0.6f);
            button.colors = colors;

            Text text = CreateText(label, rect, new Vector2(0.5f, 0.5f), size * 0.84f, Vector2.zero, fontSize, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;

            return button;
        }

        private static HighflyHoldActionButton CreateRoundHoldButton(
            string label,
            RectTransform parent,
            Sprite sprite,
            Vector2 anchor,
            Vector2 size,
            Vector2 position,
            Color background,
            Color accent,
            int fontSize,
            UnityAction pressed,
            UnityAction released)
        {
            GameObject go = CreateImage(label + "_Button", parent, background, sprite);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchored(rect, anchor, size, position, new Vector2(0.5f, 0.5f));

            GameObject accentGo = CreateImage("Accent", rect, new Color(accent.r, accent.g, accent.b, 0.20f), sprite);
            RectTransform accentRect = accentGo.GetComponent<RectTransform>();
            accentRect.anchorMin = Vector2.zero;
            accentRect.anchorMax = Vector2.one;
            accentRect.offsetMin = new Vector2(9f, 9f);
            accentRect.offsetMax = new Vector2(-9f, -9f);
            accentGo.GetComponent<Image>().raycastTarget = false;

            HighflyHoldActionButton hold = go.AddComponent<HighflyHoldActionButton>();
            UnityEventTools.AddPersistentListener(hold.OnPressed, pressed);
            UnityEventTools.AddPersistentListener(hold.OnReleased, released);

            Text text = CreateText(label, rect, new Vector2(0.5f, 0.5f), size * 0.82f, Vector2.zero, fontSize, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            return hold;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreateImage(string name, Transform parent, Color color, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = true;

            return go;
        }

        private static Text CreateText(
            string initial,
            RectTransform parent,
            Vector2 anchor,
            Vector2 size,
            Vector2 position,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject go = new GameObject(initial.Replace("\n", "_") + "_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchored(rect, anchor, size, position, new Vector2(0.5f, 0.5f));

            Text text = go.GetComponent<Text>();
            text.text = initial;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.resizeTextForBestFit = false;

            return text;
        }

        private static void SetAnchored(
            RectTransform rect,
            Vector2 anchor,
            Vector2 size,
            Vector2 position,
            Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static Material CreateStandardMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Legacy Shaders/Diffuse");

            Material material = shader != null ? new Material(shader) : null;
            if (material != null)
            {
                material.name = name;
                material.color = color;
            }

            return material;
        }

        private static bool AssetExists(string path)
        {
            return AssetDatabase.LoadMainAssetAtPath(path) != null;
        }

        private static void DeleteAssetIfExists(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                AssetDatabase.DeleteAsset(path);
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            if (target == null)
                return;

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning("HIGHFLY builder could not find property " + propertyName + " on " + target.name);
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            if (target == null)
                return;

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return;

            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            if (target == null)
                return;

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return;

            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
        {
            if (target == null)
                return;

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return;

            property.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private struct ArenaMetrics
        {
            public float width;
            public float depth;
            public float halfWidth;
            public float halfDepth;
        }
    }
}
#endif
