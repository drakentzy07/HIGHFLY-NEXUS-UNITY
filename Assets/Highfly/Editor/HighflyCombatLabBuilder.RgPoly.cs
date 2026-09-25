#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif
using Highfly.Combat;
using Highfly.Core;
using Highfly.Mobile;
using Highfly.UI;
using Highfly.World;

namespace Highfly.Editor
{
    public static partial class HighflyCombatLabBuilder
    {
        private const string RgPolySourceScene =
            "Assets/Stylized Medieval Kingdom URP/Scenes/Demo Stylized Medieval.unity";

        private static readonly Vector3 RgPolySpawn =
            new Vector3(0f, 13.35f, -42f);

        private static bool CanBuildRgPolyWorld()
        {
            return File.Exists(RgPolySourceScene) &&
                   File.Exists("Assets/Highfly/Resources/WorldFinal/world_final_enabled.txt");
        }

        private static void BuildOrRefreshRgPolyWorld()
        {
            Directory.CreateDirectory("Assets/Highfly/Scenes");
            Directory.CreateDirectory(GeneratedRoot);
            Directory.CreateDirectory(GeneratedUiRoot);
            Directory.CreateDirectory(GeneratedControllersRoot);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (!File.Exists(RgPolySourceScene))
                throw new Exception("HIGHFLY RG Poly source scene missing: " + RgPolySourceScene);

            OptimizeRgPolyTextures();

            Scene scene = EditorSceneManager.OpenScene(RgPolySourceScene, OpenSceneMode.Single);
            ConfigureRgPolyRenderPipeline();
            ReplaceRgPolyWaterForWebGL(scene);
            RemoveRgPolyDemoCameras(scene);
            RemoveRgPolyDemoRuntime(scene);
            EnsureEventSystem();

            GameObject marker = new GameObject("HIGHFLY_RG_POLY_CITY01");
            SceneManager.MoveGameObjectToScene(marker, scene);

            GameObject player = CreatePlayer(
                out HighflyThirdPersonMotor motor,
                out HighflyTargetingSystem targeting,
                out HighflyCombatController combat,
                out HighflyPlayerResources resources,
                out Transform attackOrigin,
                out Animator playerAnimator);

            player.transform.position = RgPolySpawn;
            player.transform.rotation = Quaternion.Euler(0f, 155f, 0f);

            Camera camera = CreateCamera(
                player.transform,
                targeting,
                out HighflyThirdPersonCamera cameraRig);

            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 650f;

            // WORLD v0.2 framing: closer action-RPG composition while preserving
            // the proven orbit/touch implementation.
            SetFloat(cameraRig, "distance", 4.65f);
            SetFloat(cameraRig, "height", 1.48f);
            SetFloat(cameraRig, "pitch", 12f);

            Sprite circleSprite = CreateCircleSprite();
            GameObject canvas = CreateMobileHUD(
                motor,
                targeting,
                combat,
                resources,
                cameraRig,
                circleSprite,
                out HighflyVirtualJoystick joystick,
                out HighflyCameraLookArea lookArea);

            HighflyInteractionController interaction =
                player.GetComponent<HighflyInteractionController>();
            CreateInteractionUI(
                interaction,
                canvas.GetComponent<RectTransform>(),
                circleSprite);

            SetObjectReference(motor, "cameraTransform", camera.transform);
            SetObjectReference(motor, "movementJoystick", joystick);
            SetObjectReference(motor, "animator", playerAnimator);

            SetObjectReference(targeting, "origin", player.transform);
            SetObjectReference(targeting, "gameplayCamera", camera);

            SetObjectReference(combat, "motor", motor);
            SetObjectReference(combat, "targeting", targeting);
            SetObjectReference(combat, "resources", resources);
            SetObjectReference(combat, "attackOrigin", attackOrigin);
            SetObjectReference(combat, "animator", playerAnimator);

            SetObjectReference(cameraRig, "lookArea", lookArea);
            SetObjectReference(cameraRig, "targeting", targeting);

            CleanRgPolyWorldHud(canvas);

            // World traversal test: keep the character visible but do not attach
            // the extra HIGHFLY sword. RogueHooded already carries its own visual gear.
            GameObject hunterSword = GameObject.Find("Hunter_Sword");
            if (hunterSword != null)
                hunterSword.SetActive(false);

            HighflyWorldSafety safety = player.GetComponent<HighflyWorldSafety>();
            if (safety == null)
                safety = player.AddComponent<HighflyWorldSafety>();
            safety.Configure(RgPolySpawn, -30f, true, true);

            AddRgPolySemanticAnchors(scene);
            HighflyRgPolyW1Layout.BindExterior(scene);
            HighflyRgPolyCityAudit.Write(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, WorldScenePath, true))
                throw new Exception("HIGHFLY failed to save RG Poly WorldLab.");

            EditorBuildSettings.scenes =
                new[] { new EditorBuildSettingsScene(WorldScenePath, true) };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "HIGHFLY WORLD FINAL RG POLY prepared from full Demo Stylized Medieval scene" +
                " | current HIGHFLY motor/camera/HUD preserved" +
                " | spawn=" + RgPolySpawn);
        }

        private static void ReplaceRgPolyWaterForWebGL(Scene scene)
        {
            const string materialPath =
                "Assets/Highfly/Generated/HIGHFLY_Water_WebGL.mat";

            Material water = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (water == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (shader == null)
                    throw new Exception(
                        "HIGHFLY RG Poly: no URP fallback shader available for WebGL water.");

                Directory.CreateDirectory("Assets/Highfly/Generated");

                water = new Material(shader);
                water.name = "HIGHFLY_Water_WebGL";

                Color waterColor = new Color(0.025f, 0.28f, 0.52f, 0.68f);
                if (water.HasProperty("_BaseColor"))
                    water.SetColor("_BaseColor", waterColor);
                if (water.HasProperty("_Color"))
                    water.SetColor("_Color", waterColor);
                if (water.HasProperty("_Smoothness"))
                    water.SetFloat("_Smoothness", 0.90f);
                if (water.HasProperty("_Metallic"))
                    water.SetFloat("_Metallic", 0.03f);
                if (water.HasProperty("_Surface"))
                    water.SetFloat("_Surface", 1f);
                if (water.HasProperty("_Blend"))
                    water.SetFloat("_Blend", 0f);
                if (water.HasProperty("_SrcBlend"))
                    water.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                if (water.HasProperty("_DstBlend"))
                    water.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                if (water.HasProperty("_ZWrite"))
                    water.SetFloat("_ZWrite", 0f);

                water.SetOverrideTag("RenderType", "Transparent");
                water.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                water.DisableKeyword("_ALPHATEST_ON");
                water.renderQueue = (int)RenderQueue.Transparent;

                AssetDatabase.CreateAsset(water, materialPath);
                AssetDatabase.SaveAssets();
            }

            int replacements = 0;
            int unsupported = 0;

            Renderer[] renderers =
                UnityEngine.Object.FindObjectsOfType<Renderer>(true);

            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer renderer = renderers[r];
                if (renderer == null || renderer.gameObject.scene != scene)
                    continue;

                Material[] materials = renderer.sharedMaterials;
                bool changed = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material == null)
                        continue;

                    string materialName = material.name ?? string.Empty;
                    string shaderName =
                        material.shader != null ? material.shader.name : string.Empty;

                    bool isKnownRgPolyWater =
                        materialName.IndexOf(
                            "Water IS",
                            StringComparison.OrdinalIgnoreCase) >= 0;

                    bool looksLikeWater =
                        materialName.IndexOf(
                            "Water",
                            StringComparison.OrdinalIgnoreCase) >= 0 ||
                        materialName.IndexOf(
                            "River",
                            StringComparison.OrdinalIgnoreCase) >= 0;

                    bool unsupportedShader =
                        material.shader == null || !material.shader.isSupported;

                    if (unsupportedShader)
                        unsupported++;

                    if (!isKnownRgPolyWater &&
                        !(looksLikeWater && unsupportedShader))
                        continue;

                    materials[i] = water;
                    changed = true;
                    replacements++;

                    Debug.Log(
                        "HIGHFLY RG Poly WebGL water replacement: " +
                        renderer.name + " | material=" + materialName +
                        " | shader=" + shaderName);
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                    ConfigureRgPolySwimmableWater(renderer, scene);
                }
            }

            Debug.Log(
                "HIGHFLY RG Poly WebGL water fallback applied: replacements=" +
                replacements + " | unsupportedMaterialSlotsSeen=" + unsupported);
        }

        private static void ConfigureRgPolySwimmableWater(
            Renderer renderer,
            Scene scene)
        {
            float surfaceY = renderer.bounds.max.y;
            Collider[] colliders = renderer.GetComponents<Collider>();

            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    Collider collider = colliders[i];
                    if (collider == null)
                        continue;

                    collider.enabled = true;
                    collider.isTrigger = true;

                    HighflyWaterZone zone =
                        collider.GetComponent<HighflyWaterZone>();
                    if (zone == null)
                        zone = collider.gameObject.AddComponent<HighflyWaterZone>();

                    zone.Configure(surfaceY);
                }

                Debug.Log(
                    "HIGHFLY swimmable water: converted " +
                    colliders.Length + " collider(s) to trigger on " + renderer.name);
                return;
            }

            Bounds bounds = renderer.bounds;
            float depth = Mathf.Max(2.6f, bounds.size.y + 2.6f);

            GameObject triggerObject =
                new GameObject("HIGHFLY_SWIM_TRIGGER_" + renderer.name);
            SceneManager.MoveGameObjectToScene(triggerObject, scene);

            triggerObject.transform.position = new Vector3(
                bounds.center.x,
                surfaceY - depth * 0.5f + 0.25f,
                bounds.center.z);

            BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(
                Mathf.Max(0.5f, bounds.size.x),
                depth,
                Mathf.Max(0.5f, bounds.size.z));

            HighflyWaterZone fallbackZone =
                triggerObject.AddComponent<HighflyWaterZone>();
            fallbackZone.Configure(surfaceY);

            Debug.Log(
                "HIGHFLY swimmable water: fallback trigger created for " +
                renderer.name + " | surfaceY=" + surfaceY);
        }

        private static void OptimizeRgPolyTextures()
        {
            const string packRoot = "Assets/Stylized Medieval Kingdom URP";
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { packRoot });
            int changed = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                bool dirty = false;

                if (importer.maxTextureSize > 1024)
                {
                    importer.maxTextureSize = 1024;
                    dirty = true;
                }

                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    dirty = true;
                }

                if (!dirty)
                    continue;

                importer.SaveAndReimport();
                changed++;
            }

            Debug.Log("HIGHFLY RG Poly mobile texture pass: " + changed + "/" + guids.Length);
        }

        private static void ConfigureRgPolyRenderPipeline()
        {
            const string packRoot = "Assets/Stylized Medieval Kingdom URP";
            string[] guids = AssetDatabase.FindAssets(
                "t:RenderPipelineAsset",
                new[] { packRoot });

            if (guids.Length == 0)
            {
                Debug.LogWarning("HIGHFLY RG Poly: no RenderPipelineAsset found in pack.");
                return;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                RenderPipelineAsset pipeline =
                    AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);

                if (pipeline == null)
                    continue;

                GraphicsSettings.renderPipelineAsset = pipeline;
                QualitySettings.renderPipeline = pipeline;

#if UNITY_6000_0_OR_NEWER
                UniversalRenderPipelineAsset urp = pipeline as UniversalRenderPipelineAsset;
                if (urp != null)
                {
                    Type globalSettingsType = typeof(UniversalRenderPipelineAsset).Assembly.GetType(
                        "UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings");

                    if (globalSettingsType == null)
                        throw new Exception("HIGHFLY RG Poly: URP Global Settings type was not found.");

                    MethodInfo ensureMethod = globalSettingsType.GetMethod(
                        "Ensure",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    if (ensureMethod == null)
                        throw new Exception("HIGHFLY RG Poly: URP Global Settings Ensure method was not found.");

                    object ensured = ensureMethod.Invoke(null, new object[] { true });
                    if (ensured == null)
                        throw new Exception("HIGHFLY RG Poly: URP Global Settings could not be created.");

                    UnityEngine.Object ensuredObject = ensured as UnityEngine.Object;
                    if (ensuredObject != null)
                        EditorUtility.SetDirty(ensuredObject);

                    AssetDatabase.SaveAssets();
                    Debug.Log(
                        "HIGHFLY RG Poly URP Global Settings ready: " +
                        (ensuredObject != null ? ensuredObject.name : globalSettingsType.Name));
                }
#endif

                Debug.Log("HIGHFLY RG Poly render pipeline: " + path);
                return;
            }
        }

        private static void RemoveRgPolyDemoCameras(Scene scene)
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam == null || cam.gameObject.scene != scene)
                    continue;

                UnityEngine.Object.DestroyImmediate(cam.gameObject);
            }

            AudioListener[] listeners =
                UnityEngine.Object.FindObjectsOfType<AudioListener>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                AudioListener listener = listeners[i];
                if (listener == null || listener.gameObject.scene != scene)
                    continue;

                UnityEngine.Object.DestroyImmediate(listener);
            }
        }

        private static void RemoveRgPolyDemoRuntime(Scene scene)
        {
            MonoBehaviour[] behaviours =
                UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null || behaviour.gameObject.scene != scene)
                    continue;

                Type type = behaviour.GetType();
                string typeName = type.FullName ?? type.Name;

                bool demoCamera =
                    typeName.IndexOf("Cinemachine", StringComparison.OrdinalIgnoreCase) >= 0;
                bool rotator =
                    string.Equals(type.Name, "Rotator", StringComparison.OrdinalIgnoreCase);

                if (demoCamera || rotator)
                    UnityEngine.Object.DestroyImmediate(behaviour);
            }
        }

        private static void CleanRgPolyWorldHud(GameObject canvas)
        {
            if (canvas == null)
                return;

            HighflyDungeonObjective[] objectives =
                canvas.GetComponentsInChildren<HighflyDungeonObjective>(true);
            for (int i = 0; i < objectives.Length; i++)
                objectives[i].enabled = false;

            Text[] texts = canvas.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                string value = texts[i].text ?? string.Empty;
                if (value.IndexOf("CRIPTA", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("ENEMIGOS", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("AUTO TARGET", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("AUTO-TARGET", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    texts[i].gameObject.SetActive(false);
                }
            }

            Transform targetPill = canvas.transform.Find("TargetPill");
            if (targetPill != null)
                targetPill.gameObject.SetActive(false);
        }

        private static void AddRgPolySemanticAnchors(Scene scene)
        {
            GameObject root = new GameObject("HIGHFLY_ISEKAI_ANCHORS");
            SceneManager.MoveGameObjectToScene(root, scene);

            AddAnchor(root.transform, "CITY_CORE", "Capital HIGHFLY",
                new Vector3(0f, 13.0f, 0f), 32f);
            AddAnchor(root.transform, "PROPERTY", "Parcela futura",
                new Vector3(48f, 13.0f, -18f), 12f);
            AddAnchor(root.transform, "FOREST", "Bosque / Tala",
                new Vector3(-60f, 13.0f, 22f), 18f);
            AddAnchor(root.transform, "FARM", "Granja / Cultivos",
                new Vector3(62f, 13.0f, 24f), 18f);
            AddAnchor(root.transform, "MINE", "Mina",
                new Vector3(-54f, 13.0f, 72f), 14f);
            AddAnchor(root.transform, "FISHING", "Pesca",
                new Vector3(55f, 13.0f, 72f), 14f);
            AddAnchor(root.transform, "PORTAL_FIELD", "Campo de Portales",
                new Vector3(0f, 13.0f, 92f), 18f);
        }

        private static void AddAnchor(
            Transform parent,
            string id,
            string display,
            Vector3 position,
            float radius)
        {
            GameObject go = new GameObject("ZONE_" + id);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            SphereCollider trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;

            HighflyWorldZoneAnchor anchor = go.AddComponent<HighflyWorldZoneAnchor>();
            anchor.Configure(id, display, radius);
        }
    }
}
#endif
