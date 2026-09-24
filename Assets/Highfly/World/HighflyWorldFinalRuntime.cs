using System.Collections.Generic;
using UnityEngine;
using Highfly.Combat;
using Highfly.Core;
using Highfly.UI;
using UnityEngine.UI;

namespace Highfly.World
{
    /// <summary>
    /// HIGHFLY WORLD FINAL FREE v0.1.
    /// Builds the first deterministic isekai capital from real CC0 Quaternius
    /// modular medieval assets staged by CI, while preserving the approved
    /// HIGHFLY/Lucid-derived player motor and mobile camera.
    /// </summary>
    public sealed class HighflyWorldFinalRuntime : MonoBehaviour
    {
        private const string EnableMarker = "WorldFinal/world_final_enabled";
        private const string VillageRoot = "WorldFinal/Village/FBX";
        private const string PropsRoot = "WorldFinal/Props/FBX";

        private static readonly Vector3 PlayerSpawn = new Vector3(0f, 0.35f, -48f);

        private Transform _root;
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly Dictionary<string, Vector3> _sizes = new Dictionary<string, Vector3>();

        private float _wallWidth = 2.5f;
        private float _wallHeight = 3.0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (Resources.Load<TextAsset>(EnableMarker) == null)
                return;

            if (FindObjectOfType<HighflyWorldFinalRuntime>() != null)
                return;

            GameObject go = new GameObject("HIGHFLY_WORLD_FINAL_RUNTIME");
            go.AddComponent<HighflyWorldFinalRuntime>();
        }

        private void Start()
        {
            if (Resources.Load<TextAsset>(EnableMarker) == null)
                return;

            DisablePrototypeWorlds();
            CleanPrototypeHud();

            GameObject root = new GameObject("HIGHFLY_WORLD_FINAL_FREE_V01");
            _root = root.transform;

            ConfigureLighting();
            MeasureModules();
            BuildTerrainAndRoads();
            BuildCapital();
            BuildPerimeter();
            BuildEconomyProps();
            BuildIsekaiZoneAnchors();
            PlacePlayer();

            Debug.Log(
                "HIGHFLY WORLD FINAL FREE v0.1 | Quaternius CC0 capital | " +
                "city + property + farm/forest/mine/fishing anchors | approved motor/camera preserved");
        }

        private void DisablePrototypeWorlds()
        {
            string[] names =
            {
                "HIGHFLY_VILLAGE_CLEAN_V01",
                "ZONE_SAFE_CITY_KAYKIT",
                "ZONE_DUNGEON_KAYKIT",
                "BUILDING_INTERIORS",
                "CLAUDECRAFT_WORLD_V01"
            };

            for (int i = 0; i < names.Length; i++)
            {
                GameObject go = GameObject.Find(names[i]);
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

        private void CleanPrototypeHud()
        {
            HighflyDungeonObjective[] objectives = FindObjectsOfType<HighflyDungeonObjective>();
            for (int i = 0; i < objectives.Length; i++)
                objectives[i].enabled = false;

            Text[] texts = FindObjectsOfType<Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                string value = texts[i].text ?? string.Empty;
                if (value.Contains("CRIPTA") ||
                    value.Contains("ENEMIGOS") ||
                    value.Contains("AUTO TARGET") ||
                    value.Contains("AUTO-TARGET"))
                {
                    texts[i].gameObject.SetActive(false);
                }
            }

            GameObject targetPill = GameObject.Find("TargetPill");
            if (targetPill != null)
                targetPill.SetActive(false);
        }

        private void ConfigureLighting()
        {
            RenderSettings.ambientLight = new Color(0.54f, 0.59f, 0.67f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.80f, 1f);
            RenderSettings.fogDensity = 0.0028f;

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
                GameObject go = new GameObject("WorldFinal_Sun");
                go.transform.SetParent(_root, false);
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            sun.color = new Color(1f, 0.94f, 0.84f, 1f);
            sun.intensity = 1.12f;
            sun.shadows = LightShadows.Soft;

            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.farClipPlane = 320f;
            }
        }

        private void MeasureModules()
        {
            Vector3 wall = Measure(VillageRoot + "/Wall_Plaster_Straight");
            if (wall.sqrMagnitude > 0.001f)
            {
                _wallWidth = Mathf.Max(wall.x, wall.z);
                _wallHeight = wall.y;
            }

            if (_wallWidth < 0.8f || _wallWidth > 8f)
                _wallWidth = 2.5f;
            if (_wallHeight < 1.2f || _wallHeight > 8f)
                _wallHeight = 3.0f;

            Debug.Log("HIGHFLY World Final module size: wallWidth=" + _wallWidth + " wallHeight=" + _wallHeight);
        }

        private void BuildTerrainAndRoads()
        {
            CreatePrimitive(
                "WORLD_TERRAIN",
                new Vector3(0f, -0.55f, 24f),
                new Vector3(190f, 1f, 180f),
                new Color(0.25f, 0.37f, 0.20f, 1f));

            Color road = new Color(0.34f, 0.31f, 0.28f, 1f);
            Color plaza = new Color(0.43f, 0.42f, 0.39f, 1f);

            CreatePrimitive("ROAD_SOUTH_GATE", new Vector3(0f, 0.02f, -21f), new Vector3(9f, 0.15f, 58f), road);
            CreatePrimitive("ROAD_CAPITAL_MAIN", new Vector3(0f, 0.03f, 27f), new Vector3(10f, 0.16f, 48f), road);
            CreatePrimitive("ROAD_EAST_WEST", new Vector3(0f, 0.04f, 10f), new Vector3(76f, 0.17f, 8f), road);
            CreatePrimitive("PLAZA_CENTRAL", new Vector3(0f, 0.06f, 9f), new Vector3(28f, 0.20f, 24f), plaza);

            CreatePrimitive("ROAD_FOREST", new Vector3(-62f, 0.01f, 15f), new Vector3(55f, 0.13f, 5f), road);
            CreatePrimitive("ROAD_FARM", new Vector3(58f, 0.01f, 8f), new Vector3(50f, 0.13f, 5f), road);
            CreatePrimitive("ROAD_MINE", new Vector3(-49f, 0.01f, 63f), new Vector3(5f, 0.13f, 45f), road);
            CreatePrimitive("ROAD_FISHING", new Vector3(53f, 0.01f, 62f), new Vector3(5f, 0.13f, 46f), road);
        }

        private void BuildCapital()
        {
            Transform city = NewGroup("CAPITAL_HIGHFLY");

            CreateBuilding(city, "GUILD", new Vector3(-18f, 0f, 20f), 0f, 4, 3, true);
            CreateBuilding(city, "TAVERN", new Vector3(18f, 0f, 20f), 0f, 4, 3, false);
            CreateBuilding(city, "BLACKSMITH", new Vector3(-25f, 0f, -3f), 90f, 3, 2, true);
            CreateBuilding(city, "MARKET_HALL", new Vector3(25f, 0f, -3f), -90f, 3, 2, false);
            CreateBuilding(city, "SANCTUARY", new Vector3(-26f, 0f, 39f), 90f, 3, 3, true);
            CreateBuilding(city, "ACADEMY", new Vector3(26f, 0f, 39f), -90f, 4, 3, false);

            CreateBuilding(city, "HOUSE_A", new Vector3(-39f, 0f, 18f), 90f, 3, 2, false);
            CreateBuilding(city, "HOUSE_B", new Vector3(39f, 0f, 18f), -90f, 3, 2, false);
            CreateBuilding(city, "HOUSE_C", new Vector3(-39f, 0f, 34f), 90f, 3, 2, false);
            CreateBuilding(city, "HOUSE_D", new Vector3(39f, 0f, 34f), -90f, 3, 2, false);

            CreateCastle(city, new Vector3(0f, 0f, 63f));
        }

        private void CreateBuilding(
            Transform parent,
            string name,
            Vector3 center,
            float yaw,
            int widthSegments,
            int depthSegments,
            bool brick)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = center;
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            string straight = brick ? "Wall_UnevenBrick_Straight" : "Wall_Plaster_Straight";
            string window = brick ? "Wall_UnevenBrick_Window_Wide_Round" : "Wall_Plaster_Window_Wide_Round";
            string door = brick ? "Wall_UnevenBrick_Door_Round" : "Wall_Plaster_Door_Round";

            float halfW = widthSegments * _wallWidth * 0.5f;
            float halfD = depthSegments * _wallWidth * 0.5f;

            for (int i = 0; i < widthSegments; i++)
            {
                float x = -halfW + _wallWidth * 0.5f + i * _wallWidth;
                bool centerDoor = i == widthSegments / 2;
                string front = centerDoor ? door : (i % 2 == 0 ? window : straight);
                string back = i % 2 == 0 ? window : straight;

                PlaceVillage(root.transform, front, new Vector3(x, 0f, -halfD), 0f, true);
                PlaceVillage(root.transform, back, new Vector3(-x, 0f, halfD), 180f, true);
            }

            for (int i = 0; i < depthSegments; i++)
            {
                float z = -halfD + _wallWidth * 0.5f + i * _wallWidth;
                string side = i % 2 == 0 ? window : straight;
                PlaceVillage(root.transform, side, new Vector3(-halfW, 0f, -z), 90f, true);
                PlaceVillage(root.transform, side, new Vector3(halfW, 0f, z), -90f, true);
            }

            for (int x = 0; x < Mathf.Max(1, widthSegments / 2); x++)
            {
                for (int z = 0; z < Mathf.Max(1, depthSegments / 2); z++)
                {
                    Vector3 p = new Vector3(
                        -halfW + _wallWidth + x * _wallWidth * 2f,
                        0.03f,
                        -halfD + _wallWidth + z * _wallWidth * 2f);
                    PlaceVillage(root.transform, "Floor_UnevenBrick", p, 0f, false);
                }
            }

            string roof = PickRoof(widthSegments, depthSegments);
            FitVillageModelXZ(
                root.transform,
                roof,
                new Vector3(0f, _wallHeight * 0.96f, 0f),
                0f,
                widthSegments * _wallWidth * 1.08f,
                depthSegments * _wallWidth * 1.08f);

            PlaceVillage(root.transform, "Prop_Chimney", new Vector3(halfW * 0.48f, _wallHeight * 1.02f, 0f), 0f, false);
        }

        private string PickRoof(int widthSegments, int depthSegments)
        {
            int longSide = Mathf.Max(widthSegments, depthSegments);
            if (longSide >= 5)
                return "Roof_RoundTiles_8x14";
            if (longSide >= 4)
                return "Roof_RoundTiles_8x12";
            return "Roof_RoundTiles_6x8";
        }

        private void CreateCastle(Transform parent, Vector3 center)
        {
            CreateBuilding(parent, "CASTLE_KEEP", center, 0f, 5, 4, true);

            Vector3[] towerOffsets =
            {
                new Vector3(-13f, 0f, -8f),
                new Vector3(13f, 0f, -8f),
                new Vector3(-13f, 0f, 8f),
                new Vector3(13f, 0f, 8f)
            };

            for (int i = 0; i < towerOffsets.Length; i++)
            {
                GameObject tower = new GameObject("CASTLE_TOWER_" + (i + 1));
                tower.transform.SetParent(parent, false);
                tower.transform.position = center + towerOffsets[i];

                PlaceVillage(tower.transform, "Wall_UnevenBrick_Straight", new Vector3(0f, 0f, -1.2f), 0f, true);
                PlaceVillage(tower.transform, "Wall_UnevenBrick_Straight", new Vector3(0f, 0f, 1.2f), 180f, true);
                PlaceVillage(tower.transform, "Wall_UnevenBrick_Straight", new Vector3(-1.2f, 0f, 0f), 90f, true);
                PlaceVillage(tower.transform, "Wall_UnevenBrick_Straight", new Vector3(1.2f, 0f, 0f), -90f, true);
                PlaceVillage(tower.transform, "Roof_Tower_RoundTiles", new Vector3(0f, _wallHeight * 0.95f, 0f), 0f, false);
            }
        }

        private void BuildPerimeter()
        {
            Transform perimeter = NewGroup("CAPITAL_WALLS");
            string wall = "Wall_UnevenBrick_Straight";

            float xExtent = 49f;
            float south = -39f;
            float north = 82f;

            for (float x = -xExtent; x <= xExtent; x += _wallWidth)
            {
                if (Mathf.Abs(x) > 5f)
                    PlaceVillage(perimeter, wall, new Vector3(x, 0f, south), 0f, true);
                PlaceVillage(perimeter, wall, new Vector3(x, 0f, north), 180f, true);
            }

            PlaceVillage(perimeter, "Wall_Arch", new Vector3(0f, 0f, south), 0f, true);

            for (float z = south + _wallWidth; z < north; z += _wallWidth)
            {
                PlaceVillage(perimeter, wall, new Vector3(-xExtent, 0f, z), 90f, true);
                PlaceVillage(perimeter, wall, new Vector3(xExtent, 0f, z), -90f, true);
            }
        }

        private void BuildEconomyProps()
        {
            Transform props = NewGroup("WORLD_ECONOMY_PROPS");

            PlaceProp(props, "Anvil", new Vector3(-28f, 0f, -8f), 20f, true);
            PlaceProp(props, "Workbench", new Vector3(-24f, 0f, -9f), -10f, true);
            PlaceProp(props, "WeaponStand", new Vector3(-21f, 0f, -7f), 0f, true);

            PlaceProp(props, "Stall_Empty", new Vector3(15f, 0f, 1f), 180f, true);
            PlaceProp(props, "Stall_Cart_Empty", new Vector3(21f, 0f, 1f), 180f, true);
            PlaceProp(props, "FarmCrate_Apple", new Vector3(17f, 0f, -1f), 15f, true);
            PlaceProp(props, "FarmCrate_Carrot", new Vector3(19f, 0f, -1f), -15f, true);

            PlaceProp(props, "Chest_Wood", new Vector3(-15f, 0f, 15f), 90f, true);
            PlaceProp(props, "Coin_Pile", new Vector3(-13.5f, 0f, 15.2f), 0f, false);
            PlaceProp(props, "Barrel", new Vector3(21f, 0f, 15f), 0f, true);

            CreatePropertyPlot(new Vector3(69f, 0f, 34f));
        }

        private void CreatePropertyPlot(Vector3 center)
        {
            Transform plot = NewGroup("PROPERTY_PLOT_01");
            plot.position = center;

            float halfX = 10f;
            float halfZ = 8f;

            for (float x = -halfX; x <= halfX; x += 2.5f)
            {
                if (Mathf.Abs(x) > 2.5f)
                    PlaceVillage(plot, "Prop_WoodenFence_Single", new Vector3(x, 0f, -halfZ), 0f, true);
                PlaceVillage(plot, "Prop_WoodenFence_Single", new Vector3(x, 0f, halfZ), 180f, true);
            }

            for (float z = -halfZ + 2.5f; z < halfZ; z += 2.5f)
            {
                PlaceVillage(plot, "Prop_WoodenFence_Single", new Vector3(-halfX, 0f, z), 90f, true);
                PlaceVillage(plot, "Prop_WoodenFence_Single", new Vector3(halfX, 0f, z), -90f, true);
            }

            CreateZoneAnchor("PROPERTY", "Parcela comprable", plot.position, 12f);
        }

        private void BuildIsekaiZoneAnchors()
        {
            CreateZoneAnchor("FOREST", "Bosque / Tala", new Vector3(-78f, 0f, 15f), 15f);
            CreateZoneAnchor("FARM", "Granja / Cultivos", new Vector3(78f, 0f, 8f), 15f);
            CreateZoneAnchor("MINE", "Mina", new Vector3(-49f, 0f, 87f), 13f);
            CreateZoneAnchor("FISHING", "Rio / Pesca", new Vector3(53f, 0f, 87f), 13f);
            CreateZoneAnchor("PORTAL_FIELD", "Campo de Portales", new Vector3(0f, 0f, 105f), 16f);
            CreateZoneAnchor("SECONDARY_VILLAGE", "Pueblo secundario", new Vector3(-82f, 0f, -28f), 18f);
        }

        private void CreateZoneAnchor(string id, string displayName, Vector3 position, float radius)
        {
            GameObject go = new GameObject("ZONE_" + id);
            go.transform.SetParent(_root, false);
            go.transform.position = position;

            SphereCollider trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;

            HighflyWorldZoneAnchor anchor = go.AddComponent<HighflyWorldZoneAnchor>();
            anchor.Configure(id, displayName, radius);
        }

        private GameObject PlaceVillage(Transform parent, string asset, Vector3 localPosition, float yaw, bool collider)
        {
            return PlaceModel(parent, VillageRoot + "/" + asset, asset, localPosition, yaw, collider);
        }

        private GameObject PlaceProp(Transform parent, string asset, Vector3 position, float yaw, bool collider)
        {
            GameObject holder = PlaceModel(parent, PropsRoot + "/" + asset, asset, position, yaw, collider);
            return holder;
        }

        private GameObject PlaceModel(
            Transform parent,
            string resourcePath,
            string objectName,
            Vector3 localPosition,
            float yaw,
            bool addCollider)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning("HIGHFLY World Final missing resource: " + resourcePath);
                return null;
            }

            GameObject holder = new GameObject(objectName);
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = localPosition;
            holder.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            GameObject model = Instantiate(prefab, holder.transform);
            model.name = objectName + "_Model";
            model.transform.localPosition = prefab.transform.localPosition;
            model.transform.localRotation = prefab.transform.localRotation;
            model.transform.localScale = prefab.transform.localScale;

            Bounds bounds = GetRendererBounds(holder);
            if (bounds.size.y > 0.001f)
            {
                float targetMinY = parent.TransformPoint(localPosition).y;
                holder.transform.position += Vector3.up * (targetMinY - bounds.min.y);
            }

            if (addCollider)
                AddBoundsCollider(holder);

            return holder;
        }

        private void FitVillageModelXZ(
            Transform parent,
            string asset,
            Vector3 localPosition,
            float yaw,
            float targetX,
            float targetZ)
        {
            GameObject holder = PlaceVillage(parent, asset, localPosition, yaw, false);
            if (holder == null)
                return;

            Bounds bounds = GetRendererBounds(holder);
            if (bounds.size.x < 0.001f || bounds.size.z < 0.001f)
                return;

            float sx = targetX / bounds.size.x;
            float sz = targetZ / bounds.size.z;
            float scale = Mathf.Clamp(Mathf.Min(sx, sz), 0.35f, 3.5f);
            holder.transform.localScale *= scale;

            Bounds after = GetRendererBounds(holder);
            float desiredY = parent.TransformPoint(localPosition).y;
            holder.transform.position += Vector3.up * (desiredY - after.min.y);
        }

        private Vector3 Measure(string resourcePath)
        {
            Vector3 cached;
            if (_sizes.TryGetValue(resourcePath, out cached))
                return cached;

            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
                return Vector3.zero;

            GameObject sample = Instantiate(prefab, new Vector3(5000f, -5000f, 5000f), Quaternion.identity);
            sample.SetActive(true);
            Bounds bounds = GetRendererBounds(sample);
            Vector3 size = bounds.size;
            Destroy(sample);

            _sizes[resourcePath] = size;
            return size;
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
                Debug.LogError("HIGHFLY World Final: player motor not found.");
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
            safety.Configure(PlayerSpawn, -15f, true, true);
        }

        private void CreatePrimitive(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(name, color);
        }

        private Transform NewGroup(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_root, false);
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
            material.name = "HF_WORLD_" + key;
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
