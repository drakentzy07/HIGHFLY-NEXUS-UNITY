using System.Collections.Generic;
using UnityEngine;
using Highfly.Combat;
using Highfly.Core;

namespace Highfly.World
{
    /// <summary>
    /// Compact, deterministic HIGHFLY world shell.
    /// It deliberately does not depend on ClaudeCraft world JSON or runtime data.
    /// Visuals are loaded from the CC0 KayKit Medieval assets staged by CI.
    /// The existing HIGHFLY/Lucid-derived motor, mobile camera and HUD are left untouched.
    /// </summary>
    public sealed class HighflyVillageWorldRuntime : MonoBehaviour
    {
        private const string ResourceRoot = "ClaudeWorldAssets";
        private static readonly Vector3 PlayerSpawn = new Vector3(0f, 0.25f, -20f);

        private Transform _worldRoot;
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (FindObjectOfType<HighflyVillageWorldRuntime>() != null)
                return;

            GameObject go = new GameObject("HIGHFLY_VILLAGE_WORLD_RUNTIME");
            DontDestroyOnLoad(go);
            go.AddComponent<HighflyVillageWorldRuntime>();
        }

        private void Start()
        {
            DisableLegacyWorld();

            GameObject root = new GameObject("HIGHFLY_VILLAGE_CLEAN_V01");
            _worldRoot = root.transform;

            ConfigureLighting();
            BuildGround();
            BuildRoadsAndPlaza();
            BuildVillage();
            BuildNatureAndProps();
            PlacePlayer();

            Debug.Log("HIGHFLY WORLD CLEAN v0.1 loaded | KayKit Medieval village | stable motor/camera preserved");
        }

        private void DisableLegacyWorld()
        {
            string[] roots =
            {
                "ZONE_SAFE_CITY_KAYKIT",
                "ZONE_DUNGEON_KAYKIT",
                "BUILDING_INTERIORS",
                "CLAUDECRAFT_WORLD_V01"
            };

            for (int i = 0; i < roots.Length; i++)
            {
                GameObject go = GameObject.Find(roots[i]);
                if (go != null)
                    go.SetActive(false);
            }

            HighflySimpleEnemyAI[] enemies = FindObjectsOfType<HighflySimpleEnemyAI>();
            for (int i = 0; i < enemies.Length; i++)
                enemies[i].gameObject.SetActive(false);

            HighflyZonePortal[] portals = FindObjectsOfType<HighflyZonePortal>();
            for (int i = 0; i < portals.Length; i++)
                portals[i].gameObject.SetActive(false);
        }

        private void ConfigureLighting()
        {
            RenderSettings.ambientLight = new Color(0.58f, 0.62f, 0.70f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.60f, 0.71f, 0.80f, 1f);
            RenderSettings.fogDensity = 0.0042f;

            Light sun = null;
            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    sun = lights[i];
                    break;
                }
            }

            if (sun == null)
            {
                GameObject go = new GameObject("Village_Sun");
                go.transform.SetParent(_worldRoot, false);
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            sun.color = new Color(1f, 0.93f, 0.82f, 1f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.farClipPlane = 240f;
            }
        }

        private void BuildGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Village_Ground";
            ground.transform.SetParent(_worldRoot, false);
            ground.transform.position = new Vector3(0f, -0.45f, 8f);
            ground.transform.localScale = new Vector3(92f, 0.9f, 86f);
            ground.GetComponent<Renderer>().sharedMaterial =
                GetMaterial("ground", new Color(0.30f, 0.42f, 0.24f, 1f));

            CreateStrip("NorthGround", new Vector3(0f, -0.25f, 49f), new Vector3(100f, 0.5f, 5f),
                new Color(0.20f, 0.29f, 0.17f, 1f));
        }

        private void BuildRoadsAndPlaza()
        {
            Color road = new Color(0.39f, 0.35f, 0.30f, 1f);
            Color plaza = new Color(0.48f, 0.48f, 0.45f, 1f);

            CreateStrip("MainRoad", new Vector3(0f, 0.05f, 5f), new Vector3(7f, 0.18f, 74f), road);
            CreateStrip("CrossRoad", new Vector3(0f, 0.06f, 10f), new Vector3(56f, 0.19f, 6f), road);
            CreateStrip("CentralPlaza", new Vector3(0f, 0.07f, 11f), new Vector3(22f, 0.20f, 19f), plaza);
            CreateStrip("CastleApproach", new Vector3(0f, 0.06f, 30f), new Vector3(11f, 0.19f, 23f), road);
        }

        private void BuildVillage()
        {
            Transform buildings = NewGroup("Buildings");

            PlaceBuilding(buildings, "building_castle_blue", new Vector3(0f, 0f, 42f), 180f, 12.5f);
            PlaceBuilding(buildings, "building_tavern_blue", new Vector3(-17f, 0f, 17f), 90f, 7.6f);
            PlaceBuilding(buildings, "building_blacksmith_blue", new Vector3(17f, 0f, 17f), -90f, 7.2f);
            PlaceBuilding(buildings, "building_market_blue", new Vector3(-17f, 0f, 3f), 90f, 6.8f);
            PlaceBuilding(buildings, "building_church_blue", new Vector3(18f, 0f, 2f), -90f, 8.4f);

            PlaceBuilding(buildings, "building_home_A_blue", new Vector3(-30f, 0f, 25f), 80f, 6.3f);
            PlaceBuilding(buildings, "building_home_B_blue", new Vector3(30f, 0f, 25f), -80f, 6.3f);
            PlaceBuilding(buildings, "building_home_A_blue", new Vector3(-29f, 0f, 8f), 90f, 6.0f);
            PlaceBuilding(buildings, "building_home_B_blue", new Vector3(29f, 0f, 8f), -90f, 6.0f);
            PlaceBuilding(buildings, "building_home_B_blue", new Vector3(-27f, 0f, -10f), 70f, 6.0f);
            PlaceBuilding(buildings, "building_home_A_blue", new Vector3(27f, 0f, -10f), -70f, 6.0f);

            PlaceBuilding(buildings, "building_windmill_blue", new Vector3(-36f, 0f, 39f), 35f, 10.0f);
            PlaceBuilding(buildings, "building_lumbermill_blue", new Vector3(35f, 0f, 37f), -25f, 7.2f);
            PlaceBuilding(buildings, "building_barracks_blue", new Vector3(-17f, 0f, 34f), 30f, 7.4f);
            PlaceBuilding(buildings, "building_archeryrange_blue", new Vector3(18f, 0f, 33f), -30f, 6.5f);
            PlaceBuilding(buildings, "building_well_blue", new Vector3(0f, 0f, 10f), 0f, 3.0f);

            BuildPerimeter();
        }

        private void BuildPerimeter()
        {
            Transform walls = NewGroup("Perimeter");

            for (int x = -36; x <= 36; x += 9)
            {
                PlaceNeutral(walls, "wall_straight", new Vector3(x, 0f, 51f), 0f, 3.2f);
                if (x < -6 || x > 6)
                    PlaceNeutral(walls, "wall_straight", new Vector3(x, 0f, -28f), 0f, 3.2f);
            }

            PlaceNeutral(walls, "wall_straight_gate", new Vector3(0f, 0f, -28f), 0f, 4.3f);

            for (int z = -20; z <= 44; z += 8)
            {
                PlaceNeutral(walls, "wall_straight", new Vector3(-43f, 0f, z), 90f, 3.2f);
                PlaceNeutral(walls, "wall_straight", new Vector3(43f, 0f, z), 90f, 3.2f);
            }
        }

        private void BuildNatureAndProps()
        {
            Transform nature = NewGroup("Nature");
            string[] trees = { "tree_single_A", "tree_single_B", "trees_A_small" };
            Vector3[] treePositions =
            {
                new Vector3(-38f, 0f, -18f), new Vector3(-35f, 0f, 1f), new Vector3(-39f, 0f, 18f),
                new Vector3(38f, 0f, -18f), new Vector3(36f, 0f, 0f), new Vector3(39f, 0f, 18f),
                new Vector3(-24f, 0f, 43f), new Vector3(25f, 0f, 44f)
            };

            for (int i = 0; i < treePositions.Length; i++)
            {
                string id = trees[i % trees.Length];
                PlaceResource(nature, "nature/" + id, "Tree_" + i, treePositions[i], (i * 41f) % 360f, 5.5f, false);
            }

            Transform props = NewGroup("Props");
            PlaceResource(props, "props/crate_A_big", "Crates_Blacksmith", new Vector3(13.5f, 0f, 13.5f), 15f, 1.2f, true);
            PlaceResource(props, "props/barrel", "Barrel_Tavern", new Vector3(-13.8f, 0f, 13.5f), -20f, 1.1f, true);
            PlaceResource(props, "props/weaponrack", "WeaponRack", new Vector3(12.8f, 0f, 20.5f), 0f, 2.0f, true);
            PlaceResource(props, "props/wheelbarrow", "Wheelbarrow", new Vector3(31f, 0f, 34f), 110f, 1.4f, true);
            PlaceResource(props, "props/resource_lumber", "Lumber", new Vector3(35f, 0f, 31f), 0f, 1.4f, true);
            PlaceResource(props, "props/resource_stone", "Stone", new Vector3(-31f, 0f, -4f), 0f, 1.4f, true);
        }

        private GameObject PlaceBuilding(Transform parent, string asset, Vector3 position, float yaw, float targetHeight)
        {
            return PlaceResource(parent, "buildings/blue/" + asset, asset, position, yaw, targetHeight, true);
        }

        private GameObject PlaceNeutral(Transform parent, string asset, Vector3 position, float yaw, float targetHeight)
        {
            return PlaceResource(parent, "buildings/neutral/" + asset, asset, position, yaw, targetHeight, true);
        }

        private GameObject PlaceResource(
            Transform parent,
            string relativePath,
            string objectName,
            Vector3 position,
            float yaw,
            float targetHeight,
            bool addCollider)
        {
            GameObject prefab = Resources.Load<GameObject>(ResourceRoot + "/" + relativePath);
            if (prefab == null)
            {
                Debug.LogWarning("HIGHFLY Village: missing resource " + ResourceRoot + "/" + relativePath);
                return CreateFallbackBuilding(parent, objectName + "_Fallback", position, yaw, targetHeight);
            }

            GameObject go = Instantiate(prefab, position, Quaternion.Euler(0f, yaw, 0f), parent);
            go.name = objectName;

            Bounds before = GetRendererBounds(go);
            if (before.size.y > 0.01f)
            {
                float scale = Mathf.Clamp(targetHeight / before.size.y, 0.12f, 12f);
                go.transform.localScale *= scale;
            }

            Bounds after = GetRendererBounds(go);
            if (after.size.y > 0.01f)
                go.transform.position += Vector3.up * (-after.min.y);

            if (addCollider)
                AddBoundsCollider(go);

            return go;
        }

        private GameObject CreateFallbackBuilding(
            Transform parent,
            string name,
            Vector3 position,
            float yaw,
            float targetHeight)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position + Vector3.up * (targetHeight * 0.5f);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = new Vector3(targetHeight * 0.85f, targetHeight, targetHeight * 0.72f);
            go.GetComponent<Renderer>().sharedMaterial =
                GetMaterial("fallback", new Color(0.40f, 0.30f, 0.24f, 1f));
            return go;
        }

        private void AddBoundsCollider(GameObject go)
        {
            if (go == null || go.GetComponent<Collider>() != null)
                return;

            Bounds bounds = GetRendererBounds(go);
            if (bounds.size.sqrMagnitude < 0.001f)
                return;

            BoxCollider box = go.AddComponent<BoxCollider>();
            box.center = go.transform.InverseTransformPoint(bounds.center);

            Vector3 scale = go.transform.lossyScale;
            box.size = new Vector3(
                bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
                bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)),
                bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(scale.z)));
        }

        private void PlacePlayer()
        {
            HighflyThirdPersonMotor motor = FindObjectOfType<HighflyThirdPersonMotor>();
            if (motor == null)
            {
                Debug.LogError("HIGHFLY Village: player motor not found.");
                return;
            }

            CharacterController controller = motor.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
                controller.enabled = false;

            motor.transform.position = PlayerSpawn;
            motor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            if (wasEnabled)
                controller.enabled = true;

            HighflyWorldSafety safety = motor.GetComponent<HighflyWorldSafety>();
            if (safety == null)
                safety = motor.gameObject.AddComponent<HighflyWorldSafety>();
            safety.Configure(PlayerSpawn, -12f, true, true);
        }

        private void CreateStrip(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_worldRoot, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(name, color);
        }

        private Transform NewGroup(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_worldRoot, false);
            return go.transform;
        }

        private Material GetMaterial(string key, Color color)
        {
            Material material;
            if (_materials.TryGetValue(key, out material) && material != null)
                return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            material = new Material(shader);
            material.name = "HF_" + key;
            material.color = color;
            _materials[key] = material;
            return material;
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
    }
}
