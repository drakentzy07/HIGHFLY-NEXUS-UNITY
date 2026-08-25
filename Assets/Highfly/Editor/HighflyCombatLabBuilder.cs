#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

        [MenuItem("HIGHFLY/Build Combat Lab Scene")]
        public static void BuildOrRefreshCombatLab()
        {
            Directory.CreateDirectory("Assets/Highfly/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            CreateGround();
            EnsureEventSystem();

            GameObject player = CreatePlayer(out HighflyThirdPersonMotor motor, out HighflyTargetingSystem targeting,
                out HighflyCombatController combat, out HighflyPlayerResources resources, out Transform attackOrigin);

            Camera camera = CreateCamera(player.transform, targeting, out HighflyThirdPersonCamera cameraRig);
            GameObject canvas = CreateMobileHUD(motor, targeting, combat, resources, cameraRig, out HighflyVirtualJoystick joystick,
                out HighflyCameraLookArea lookArea);

            SetObjectReference(motor, "cameraTransform", camera.transform);
            SetObjectReference(motor, "movementJoystick", joystick);
            SetObjectReference(targeting, "origin", player.transform);
            SetObjectReference(targeting, "gameplayCamera", camera);
            SetInt(targeting, "targetMask", EnemyMask);
            SetObjectReference(combat, "motor", motor);
            SetObjectReference(combat, "targeting", targeting);
            SetObjectReference(combat, "resources", resources);
            SetObjectReference(combat, "attackOrigin", attackOrigin);
            SetInt(combat, "enemyMask", EnemyMask);
            SetObjectReference(cameraRig, "lookArea", lookArea);
            SetObjectReference(cameraRig, "targeting", targeting);

            HighflyHealth playerHealth = player.GetComponent<HighflyHealth>();
            CreateEnemy("Goblin_A", new Vector3(-1.7f, 1f, 6f), 65f, 2.6f, player.transform, playerHealth, false);
            CreateEnemy("Goblin_B", new Vector3(0f, 1f, 6.8f), 65f, 2.6f, player.transform, playerHealth, false);
            CreateEnemy("Goblin_C", new Vector3(1.7f, 1f, 6f), 65f, 2.6f, player.transform, playerHealth, false);
            CreateEnemy("Miniboss", new Vector3(0f, 1.4f, 11f), 240f, 2.0f, player.transform, playerHealth, true);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HIGHFLY Combat Lab generated at " + ScenePath);
        }

        private static GameObject CreatePlayer(out HighflyThirdPersonMotor motor, out HighflyTargetingSystem targeting,
            out HighflyCombatController combat, out HighflyPlayerResources resources, out Transform attackOrigin)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "HIGHFLY_Hunter";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);

            CapsuleCollider primitiveCollider = player.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null)
                Object.DestroyImmediate(primitiveCollider);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.45f;
            cc.center = Vector3.zero;

            HighflyHealth health = player.AddComponent<HighflyHealth>();
            SetFloat(health, "maxHealth", 160f);
            resources = player.AddComponent<HighflyPlayerResources>();
            motor = player.AddComponent<HighflyThirdPersonMotor>();
            targeting = player.AddComponent<HighflyTargetingSystem>();
            combat = player.AddComponent<HighflyCombatController>();

            GameObject attack = new GameObject("AttackOrigin");
            attack.transform.SetParent(player.transform, false);
            attack.transform.localPosition = new Vector3(0f, 0.8f, 0.65f);
            attackOrigin = attack.transform;

            return player;
        }

        private static Camera CreateCamera(Transform player, HighflyTargetingSystem targeting, out HighflyThirdPersonCamera rig)
        {
            GameObject go = new GameObject("HIGHFLY_Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 3f, -5f);

            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            go.AddComponent<AudioListener>();

            rig = go.AddComponent<HighflyThirdPersonCamera>();
            SetObjectReference(rig, "followTarget", player);
            SetObjectReference(rig, "targeting", targeting);
            return camera;
        }

        private static GameObject CreateMobileHUD(HighflyThirdPersonMotor motor, HighflyTargetingSystem targeting,
            HighflyCombatController combat, HighflyPlayerResources resources, HighflyThirdPersonCamera cameraRig,
            out HighflyVirtualJoystick joystick, out HighflyCameraLookArea lookArea)
        {
            GameObject canvasGo = new GameObject("HIGHFLY_MobileHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(3088f, 1440f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();

            // Right camera touch zone. It is created first so action buttons render/raycast above it.
            GameObject lookGo = CreateImage("CameraLookArea", canvasRect, new Color(0f, 0f, 0f, 0.001f));
            RectTransform lookRect = lookGo.GetComponent<RectTransform>();
            lookRect.anchorMin = new Vector2(0.42f, 0f);
            lookRect.anchorMax = new Vector2(1f, 1f);
            lookRect.offsetMin = Vector2.zero;
            lookRect.offsetMax = Vector2.zero;
            lookArea = lookGo.AddComponent<HighflyCameraLookArea>();
            SetObjectReference(cameraRig, "lookArea", lookArea);

            // Joystick bottom-left.
            GameObject joystickBg = CreateImage("MoveJoystick", canvasRect, new Color(0.06f, 0.08f, 0.12f, 0.70f));
            RectTransform bgRect = joystickBg.GetComponent<RectTransform>();
            SetAnchored(bgRect, new Vector2(0f, 0f), new Vector2(360f, 360f), new Vector2(250f, 235f), new Vector2(0.5f, 0.5f));

            GameObject handleGo = CreateImage("Handle", bgRect, new Color(0.45f, 0.75f, 1f, 0.85f));
            RectTransform handleRect = handleGo.GetComponent<RectTransform>();
            SetAnchored(handleRect, new Vector2(0.5f, 0.5f), new Vector2(145f, 145f), Vector2.zero, new Vector2(0.5f, 0.5f));

            joystick = joystickBg.AddComponent<HighflyVirtualJoystick>();
            SetObjectReference(joystick, "background", bgRect);
            SetObjectReference(joystick, "handle", handleRect);

            // Core action layout, intentionally higher than the very bottom edge for landscape grip.
            CreateButton("ATTACK", canvasRect, new Vector2(1f, 0f), new Vector2(250f, 250f), new Vector2(-190f, 245f), combat.BasicAttack);
            CreateButton("HEAVY", canvasRect, new Vector2(1f, 0f), new Vector2(175f, 175f), new Vector2(-430f, 175f), combat.HeavyAttack);
            CreateButton("DASH", canvasRect, new Vector2(1f, 0f), new Vector2(165f, 165f), new Vector2(-615f, 180f), combat.DashOrDodge);
            CreateButton("S1", canvasRect, new Vector2(1f, 0f), new Vector2(180f, 180f), new Vector2(-565f, 390f), combat.SkillLineCleave);
            CreateButton("S2", canvasRect, new Vector2(1f, 0f), new Vector2(180f, 180f), new Vector2(-385f, 485f), combat.SkillCone);
            CreateButton("S3", canvasRect, new Vector2(1f, 0f), new Vector2(180f, 180f), new Vector2(-180f, 515f), combat.SkillArea);
            CreateHoldButton("BLOCK", canvasRect, new Vector2(1f, 0f), new Vector2(160f, 160f), new Vector2(-760f, 205f), combat.BeginBlock, combat.EndBlock);

            Text hp = CreateText("HP", canvasRect, new Vector2(0f, 1f), new Vector2(460f, 70f), new Vector2(260f, -65f), 36, TextAnchor.MiddleLeft);
            Text mp = CreateText("MP", canvasRect, new Vector2(0f, 1f), new Vector2(460f, 70f), new Vector2(260f, -125f), 32, TextAnchor.MiddleLeft);
            Text stamina = CreateText("STA", canvasRect, new Vector2(0f, 1f), new Vector2(460f, 70f), new Vector2(260f, -180f), 28, TextAnchor.MiddleLeft);
            Text target = CreateText("TARGET: AUTO", canvasRect, new Vector2(0.5f, 1f), new Vector2(650f, 65f), new Vector2(0f, -70f), 30, TextAnchor.MiddleCenter);

            HighflyCombatHUD hud = canvasGo.AddComponent<HighflyCombatHUD>();
            SetObjectReference(hud, "resources", resources);
            SetObjectReference(hud, "targeting", targeting);
            SetObjectReference(hud, "hpText", hp);
            SetObjectReference(hud, "mpText", mp);
            SetObjectReference(hud, "staminaText", stamina);
            SetObjectReference(hud, "targetText", target);

            return canvasGo;
        }

        private static void CreateEnemy(string name, Vector3 position, float hp, float speed, Transform player,
            HighflyHealth playerHealth, bool boss)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            enemy.layer = EnemyLayer;
            enemy.transform.position = position;
            if (boss)
                enemy.transform.localScale = new Vector3(1.45f, 1.45f, 1.45f);

            CapsuleCollider primitiveCollider = enemy.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null)
                Object.DestroyImmediate(primitiveCollider);

            CharacterController cc = enemy.AddComponent<CharacterController>();
            cc.height = boss ? 2.8f : 2f;
            cc.radius = boss ? 0.62f : 0.45f;

            HighflyHealth health = enemy.AddComponent<HighflyHealth>();
            SetFloat(health, "maxHealth", hp);
            HighflyTargetable targetable = enemy.AddComponent<HighflyTargetable>();
            SetObjectReference(targetable, "health", health);
            SetInt(targetable, "faction", 1);

            HighflySimpleEnemyAI ai = enemy.AddComponent<HighflySimpleEnemyAI>();
            SetObjectReference(ai, "selfHealth", health);
            SetFloat(ai, "moveSpeed", speed);
            SetFloat(ai, "attackDamage", boss ? 16f : 7f);
            SetFloat(ai, "attackRange", boss ? 2.3f : 1.7f);
            ai.SetTarget(player, playerHealth);
        }

        private static void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "CombatLab_Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
        }

        private static void CreateLighting()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return go;
        }

        private static Button CreateButton(string label, RectTransform parent, Vector2 anchor, Vector2 size, Vector2 position, UnityAction action)
        {
            GameObject go = CreateImage(label + "_Button", parent, new Color(0.08f, 0.11f, 0.18f, 0.88f));
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchored(rect, anchor, size, position, new Vector2(0.5f, 0.5f));

            Button button = go.AddComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            CreateText(label, rect, new Vector2(0.5f, 0.5f), size, Vector2.zero, 34, TextAnchor.MiddleCenter);
            return button;
        }

        private static HighflyHoldActionButton CreateHoldButton(string label, RectTransform parent, Vector2 anchor, Vector2 size,
            Vector2 position, UnityAction pressed, UnityAction released)
        {
            GameObject go = CreateImage(label + "_Button", parent, new Color(0.10f, 0.13f, 0.20f, 0.88f));
            RectTransform rect = go.GetComponent<RectTransform>();
            SetAnchored(rect, anchor, size, position, new Vector2(0.5f, 0.5f));

            HighflyHoldActionButton hold = go.AddComponent<HighflyHoldActionButton>();
            UnityEventTools.AddPersistentListener(hold.OnPressed, pressed);
            UnityEventTools.AddPersistentListener(hold.OnReleased, released);
            CreateText(label, rect, new Vector2(0.5f, 0.5f), size, Vector2.zero, 28, TextAnchor.MiddleCenter);
            return hold;
        }

        private static Text CreateText(string initial, RectTransform parent, Vector2 anchor, Vector2 size, Vector2 position,
            int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(initial + "_Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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
            return text;
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position, Vector2 pivot)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
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

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return;
            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
                return;
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
