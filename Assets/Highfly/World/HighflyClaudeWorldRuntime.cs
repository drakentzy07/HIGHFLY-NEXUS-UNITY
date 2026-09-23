using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Highfly.Combat;
using Highfly.Core;
using Highfly.UI;

namespace Highfly.World
{
    public sealed class HighflyClaudeWorldRuntime : MonoBehaviour
    {
        private const string ResourcePath = "ClaudeCraft/world";
        private const string EntityResourcePath = "ClaudeCraft/entities";
        private const string StructureResourcePath = "ClaudeCraft/structure";

        [Serializable] private sealed class WorldData
        {
            public string schema;
            public float waterLevel;
            public Point playerStart;
            public Zone[] zones;
            public TerrainZone[] terrain;
            public Road[] roads;
            public Camp[] camps;
            public GatherNode[] gatherNodes;
            public Portal[] portals;
            public Decoration[] decorations;
            public PropData[] props;
        }

        [Serializable] private sealed class Point { public float x; public float z; }
        [Serializable] private sealed class Lake { public float x; public float z; public float radius; }

        [Serializable] private sealed class Zone
        {
            public string id;
            public string name;
            public string biome;
            public float xMin;
            public float xMax;
            public float zMin;
            public float zMax;
            public Point hub;
            public Lake[] lakes;
        }

        [Serializable] private sealed class TerrainZone
        {
            public string zoneId;
            public float step;
            public float minX;
            public float maxX;
            public float minZ;
            public float maxZ;
            public int width;
            public int depth;
            public float minHeight;
            public float maxHeight;
            public float[] heights;
            public int[] waterMask;
            public Lake[] lakes;
        }

        [Serializable] private sealed class Road
        {
            public string id;
            public Point[] points;
        }

        [Serializable] private sealed class Camp
        {
            public string id;
            public string mobId;
            public float x;
            public float z;
            public float radius;
            public int count;
        }

        [Serializable] private sealed class GatherNode
        {
            public string id;
            public string type;
            public string zoneId;
            public int tier;
            public float x;
            public float z;
        }

        [Serializable] private sealed class Portal
        {
            public string id;
            public float x;
            public float z;
            public float targetX;
            public float targetZ;
        }

        [Serializable] private sealed class Decoration
        {
            public string id;
            public string kind;
            public float x;
            public float z;
            public float scale;
            public float rotation;
        }

        [Serializable] private sealed class PropData
        {
            public string id;
            public string category;
            public string kind;
            public string assetId;
            public string key;
            public float x;
            public float z;
            public float x2;
            public float z2;
            public float rot;
            public float scale;
            public float radius;
            public float width;
            public float depth;
            public float height;
            public int count;
        }

        [Serializable] private sealed class StructureData
        {
            public string schema;
            public Point citySpawn;
            public Zone[] zones;
            public Road[] roads;
            public PropData[] props;
        }

        [Serializable] private sealed class EntityData
        {
            public string schema;
            public NpcData[] npcs;
            public DungeonData[] dungeons;
            public MobData[] mobs;
            public StationData[] stations;
            public WorldObjectData[] objects;
        }

        [Serializable] private sealed class NpcData
        {
            public string id;
            public string name;
            public string title;
            public string role;
            public string greeting;
            public float x;
            public float z;
            public float facing;
            public int color;
            public int questCount;
        }

        [Serializable] private sealed class DungeonData
        {
            public string id;
            public string name;
            public string interior;
            public string enterText;
            public string leaveText;
            public float x;
            public float z;
            public int suggestedPlayers;
            public bool staticDoor;
        }

        [Serializable] private sealed class MobData
        {
            public string id;
            public string name;
            public string family;
            public float scale;
            public int color;
            public bool elite;
            public bool rare;
            public bool boss;
        }

        [Serializable] private sealed class StationData
        {
            public string id;
            public string type;
            public float x;
            public float z;
        }

        [Serializable] private sealed class WorldObjectData
        {
            public string id;
            public string name;
            public string templateId;
            public float x;
            public float z;
        }

        private WorldData _data;
        private StructureData _structure;
        private EntityData _entities;
        private Transform _worldRoot;
        private HighflyThirdPersonMotor _playerMotor;
        private Vector3 _lastDryPosition;
        private bool _hasLastDryPosition;
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (FindObjectOfType<HighflyClaudeWorldRuntime>() != null)
                return;

            GameObject go = new GameObject("HIGHFLY_CLAUDECRAFT_WORLD_RUNTIME");
            DontDestroyOnLoad(go);
            go.AddComponent<HighflyClaudeWorldRuntime>();
        }

        private void Start()
        {
            TextAsset json = Resources.Load<TextAsset>(ResourcePath);
            if (json == null)
            {
                Debug.LogError("HIGHFLY ClaudeCraft: Resources/" + ResourcePath + ".json not found.");
                return;
            }

            _data = JsonUtility.FromJson<WorldData>(json.text);

            TextAsset structureJson = Resources.Load<TextAsset>(StructureResourcePath);
            if (structureJson != null)
            {
                _structure = JsonUtility.FromJson<StructureData>(structureJson.text);
                if (_structure != null)
                {
                    if (_structure.zones != null && _structure.zones.Length > 0) _data.zones = _structure.zones;
                    if (_structure.roads != null && _structure.roads.Length > 0) _data.roads = _structure.roads;
                    if (_structure.props != null && _structure.props.Length > 0) _data.props = _structure.props;
                }
            }
            else
            {
                Debug.LogWarning("HIGHFLY ClaudeCraft: Resources/" + StructureResourcePath + ".json not found; using legacy world structure export.");
            }

            TextAsset entityJson = Resources.Load<TextAsset>(EntityResourcePath);
            if (entityJson != null)
                _entities = JsonUtility.FromJson<EntityData>(entityJson.text);
            else
                Debug.LogWarning("HIGHFLY ClaudeCraft: Resources/" + EntityResourcePath + ".json not found; authored NPC/dungeon pass disabled.");

            if (_data == null || _data.terrain == null || _data.terrain.Length == 0)
            {
                Debug.LogError("HIGHFLY ClaudeCraft: invalid or empty world export.");
                return;
            }

            DisableLegacyLabWorld();

            GameObject root = new GameObject("CLAUDECRAFT_WORLD_V01");
            _worldRoot = root.transform;

            ImproveWorldLighting();
            BuildTerrain();
            BuildWater();
            BuildRoads();
            BuildProps();
            EnsureEastbrookVisible();
            BuildDecorations();
            EnsureEastbrookVegetation();
            BuildWorldMarkers();
            ConfigurePlayerAndCamera();
            ConfigureWorldLabUi();
            BuildDungeonEntrances();

            Debug.Log(
                "HIGHFLY ClaudeCraft WORLD v0.3 loaded | zones=" + (_data.zones != null ? _data.zones.Length : 0) +
                " terrain=" + _data.terrain.Length +
                " roads=" + (_data.roads != null ? _data.roads.Length : 0) +
                " camps=" + (_data.camps != null ? _data.camps.Length : 0) +
                " gather=" + (_data.gatherNodes != null ? _data.gatherNodes.Length : 0) +
                " props=" + (_data.props != null ? _data.props.Length : 0) +
                " decorations=" + (_data.decorations != null ? _data.decorations.Length : 0) +
                " worldMode=NO_NPCS_NO_MOBS" +
                " dungeons=" + (_entities != null && _entities.dungeons != null ? _entities.dungeons.Length : 0));
        }

        private void LateUpdate()
        {
            MaintainWaterTraversal();
        }

        private void DisableLegacyLabWorld()
        {
            string[] roots =
            {
                "ZONE_SAFE_CITY_KAYKIT",
                "ZONE_DUNGEON_KAYKIT",
                "BUILDING_INTERIORS"
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

            HighflyZonePortal[] oldPortals = FindObjectsOfType<HighflyZonePortal>();
            for (int i = 0; i < oldPortals.Length; i++)
                oldPortals[i].gameObject.SetActive(false);
        }

        private void BuildTerrain()
        {
            Transform parent = NewGroup("Terrain");

            for (int zi = 0; zi < _data.terrain.Length; zi++)
            {
                TerrainZone zone = _data.terrain[zi];
                if (zone == null || zone.heights == null || zone.width < 2 || zone.depth < 2)
                    continue;

                int expected = zone.width * zone.depth;
                if (zone.heights.Length < expected)
                {
                    Debug.LogWarning("HIGHFLY ClaudeCraft: terrain array too short for " + zone.zoneId);
                    continue;
                }

                Vector3[] vertices = new Vector3[expected];
                Vector2[] uv = new Vector2[expected];
                int p = 0;

                for (int z = 0; z < zone.depth; z++)
                {
                    float sourceZ = Mathf.Min(zone.maxZ, zone.minZ + z * zone.step);
                    for (int x = 0; x < zone.width; x++)
                    {
                        float sourceX = Mathf.Min(zone.maxX, zone.minX + x * zone.step);
                        vertices[p] = new Vector3(ToUnityX(sourceX), zone.heights[p], sourceZ);
                        uv[p] = new Vector2(
                            zone.width > 1 ? x / (float)(zone.width - 1) : 0f,
                            zone.depth > 1 ? z / (float)(zone.depth - 1) : 0f);
                        p++;
                    }
                }

                int[] triangles = new int[(zone.width - 1) * (zone.depth - 1) * 6];
                int t = 0;
                for (int z = 0; z < zone.depth - 1; z++)
                {
                    for (int x = 0; x < zone.width - 1; x++)
                    {
                        int a = z * zone.width + x;
                        int b = a + 1;
                        int c = a + zone.width;
                        int d = c + 1;

                        // Mirroring X reverses handedness. Keeping the source grid
                        // order restores upward-facing triangles in Unity.
                        triangles[t++] = a;
                        triangles[t++] = b;
                        triangles[t++] = c;
                        triangles[t++] = b;
                        triangles[t++] = d;
                        triangles[t++] = c;
                    }
                }

                Mesh mesh = new Mesh();
                mesh.name = "ClaudeTerrain_" + zone.zoneId;
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uv;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                GameObject go = new GameObject("Terrain_" + zone.zoneId);
                go.transform.SetParent(parent, false);
                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = GetMaterial("terrain_" + zone.zoneId, TerrainColor(zone.zoneId));
                MeshCollider collider = go.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
            }
        }

        private void BuildWater()
        {
            if (_data.terrain == null)
                return;

            Transform parent = NewGroup("Water");
            Material water = GetMaterial("water", new Color(0.05f, 0.32f, 0.58f, 0.82f));

            // ClaudeCraft already exports a per-vertex water mask. Rendering only
            // circular lake metadata hid coastlines/harbours (including the spawn).
            // Build one lightweight water mesh per terrain zone instead.
            for (int zi = 0; zi < _data.terrain.Length; zi++)
            {
                TerrainZone zone = _data.terrain[zi];
                if (zone == null || zone.waterMask == null ||
                    zone.width < 2 || zone.depth < 2 ||
                    zone.waterMask.Length < zone.width * zone.depth)
                    continue;

                int count = zone.width * zone.depth;
                Vector3[] vertices = new Vector3[count];
                Vector2[] uv = new Vector2[count];

                for (int z = 0; z < zone.depth; z++)
                {
                    float sourceZ = Mathf.Min(zone.maxZ, zone.minZ + z * zone.step);
                    for (int x = 0; x < zone.width; x++)
                    {
                        float sourceX = Mathf.Min(zone.maxX, zone.minX + x * zone.step);
                        int index = z * zone.width + x;
                        vertices[index] = new Vector3(ToUnityX(sourceX), _data.waterLevel + 0.04f, sourceZ);
                        uv[index] = new Vector2(
                            zone.width > 1 ? x / (float)(zone.width - 1) : 0f,
                            zone.depth > 1 ? z / (float)(zone.depth - 1) : 0f);
                    }
                }

                List<int> triangles = new List<int>();
                for (int z = 0; z < zone.depth - 1; z++)
                {
                    for (int x = 0; x < zone.width - 1; x++)
                    {
                        int a = z * zone.width + x;
                        int b = a + 1;
                        int c0 = a + zone.width;
                        int d = c0 + 1;

                        int wet =
                            (zone.waterMask[a] != 0 ? 1 : 0) +
                            (zone.waterMask[b] != 0 ? 1 : 0) +
                            (zone.waterMask[c0] != 0 ? 1 : 0) +
                            (zone.waterMask[d] != 0 ? 1 : 0);

                        // Two wet corners gives a smooth enough shoreline at the
                        // exported 4 m grid while avoiding large water sheets on land.
                        if (wet < 2)
                            continue;

                        // Same winding as the mirrored terrain so the water faces up.
                        triangles.Add(a); triangles.Add(b); triangles.Add(c0);
                        triangles.Add(b); triangles.Add(d); triangles.Add(c0);
                    }
                }

                if (triangles.Count == 0)
                    continue;

                Mesh mesh = new Mesh();
                mesh.name = "ClaudeWater_" + zone.zoneId;
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.vertices = vertices;
                mesh.SetTriangles(triangles, 0);
                mesh.uv = uv;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                GameObject go = new GameObject("Water_" + zone.zoneId);
                go.transform.SetParent(parent, false);
                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = water;
            }
        }

        private void BuildRoads()
        {
            if (_data.roads == null)
                return;

            Transform parent = NewGroup("Roads");
            Material roadMaterial = GetMaterial("road_surface", new Color(0.38f, 0.29f, 0.18f, 1f));

            for (int ri = 0; ri < _data.roads.Length; ri++)
            {
                Road road = _data.roads[ri];
                if (road == null || road.points == null || road.points.Length < 2)
                    continue;

                GameObject go = new GameObject(string.IsNullOrEmpty(road.id) ? "Road_" + ri : road.id);
                go.transform.SetParent(parent, false);

                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector2> uvs = new List<Vector2>();

                const float halfWidth = 1.35f;
                for (int i = 0; i < road.points.Length - 1; i++)
                {
                    Point p0 = road.points[i];
                    Point p1 = road.points[i + 1];
                    Vector3 a = new Vector3(ToUnityX(p0.x), SampleHeight(p0.x, p0.z) + 0.08f, p0.z);
                    Vector3 b = new Vector3(ToUnityX(p1.x), SampleHeight(p1.x, p1.z) + 0.08f, p1.z);
                    Vector3 dir = b - a;
                    dir.y = 0f;
                    if (dir.sqrMagnitude < 0.001f) continue;
                    dir.Normalize();
                    Vector3 side = new Vector3(-dir.z, 0f, dir.x) * halfWidth;

                    int v = vertices.Count;
                    vertices.Add(a - side);
                    vertices.Add(a + side);
                    vertices.Add(b - side);
                    vertices.Add(b + side);
                    uvs.Add(new Vector2(0f, 0f));
                    uvs.Add(new Vector2(1f, 0f));
                    uvs.Add(new Vector2(0f, 1f));
                    uvs.Add(new Vector2(1f, 1f));

                    triangles.Add(v + 0); triangles.Add(v + 2); triangles.Add(v + 1);
                    triangles.Add(v + 1); triangles.Add(v + 2); triangles.Add(v + 3);
                }

                if (vertices.Count == 0)
                {
                    Destroy(go);
                    continue;
                }

                Mesh mesh = new Mesh();
                mesh.name = "RoadMesh_" + ri;
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = roadMaterial;
            }
        }

        private void ImproveWorldLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.46f, 0.50f, 0.55f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.34f, 0.45f, 0.56f, 1f);
            RenderSettings.fogDensity = 0.0012f;

            GameObject sun = new GameObject("CLAUDECRAFT_SUN");
            sun.transform.SetParent(_worldRoot, false);
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.18f;
            light.color = new Color(1f, 0.94f, 0.82f, 1f);
            light.shadows = LightShadows.Soft;
        }

        private void BuildProps()
        {
            if (_data.props == null)
                return;

            Transform parent = NewGroup("AuthoredProps");
            for (int i = 0; i < _data.props.Length; i++)
            {
                PropData prop = _data.props[i];
                if (prop == null)
                    continue;

                string category = (prop.category ?? string.Empty).ToLowerInvariant();
                string kind = (prop.kind ?? string.Empty).ToLowerInvariant();
                string resource = null;
                float target = 2.5f;
                float authoredScale = Mathf.Clamp(prop.scale <= 0f ? 1f : prop.scale, 0.75f, 1.35f);

                if (category == "buildings")
                {
                    resource = BuildingResource(kind);
                    // The export width/depth are already world-space footprints.
                    // v0.2 multiplied them by 2 again, which made authored buildings
                    // drift away from the intended player scale.
                    target = Mathf.Max(5.5f, Mathf.Max(prop.width, prop.depth) * 1.15f);
                    authoredScale = 1f;
                }
                else if (category == "wells")
                {
                    resource = "ClaudeWorldAssets/buildings/blue/building_well_blue";
                    target = 2.5f;
                }
                else if (category == "mines")
                {
                    resource = "ClaudeWorldAssets/buildings/blue/building_mine_blue";
                    target = 6f;
                }
                else if (category == "tents")
                {
                    resource = "ClaudeWorldAssets/props/tent";
                    target = 3f;
                }
                else if (category == "crates")
                {
                    resource = prop.count > 1
                        ? "ClaudeWorldAssets/props/crate_A_big"
                        : "ClaudeWorldAssets/props/crate_A_small";
                    target = prop.count > 1 ? 1.5f : 0.9f;
                }
                else if (category == "campfires")
                {
                    // Reuse-first placeholder until a dedicated CC0 fire prefab is staged.
                    resource = "ClaudeWorldAssets/props/barrel";
                    target = 0.75f;
                }
                else if (category == "fences")
                {
                    BuildFenceProp(parent, prop, false);
                    continue;
                }
                else if (category == "walls")
                {
                    float wallDx = prop.x2 - prop.x;
                    float wallDz = prop.z2 - prop.z;
                    if (wallDx * wallDx + wallDz * wallDz > 0.25f)
                    {
                        BuildFenceProp(parent, prop, true);
                    }
                    else
                    {
                        string wallResource = (!string.IsNullOrEmpty(prop.assetId) && prop.assetId.ToLowerInvariant().Contains("gate"))
                            ? "ClaudeWorldAssets/buildings/neutral/wall_straight_gate"
                            : "ClaudeWorldAssets/buildings/neutral/wall_straight";
                        float wallFootprint = Mathf.Max(3.2f, Mathf.Max(prop.width, prop.depth));
                        CreateWorldPrefab(parent, wallResource, "Wall_" + prop.id, prop.x, prop.z, prop.rot, wallFootprint, 1f);
                    }
                    continue;
                }
                else if (category == "benches")
                {
                    resource = "ClaudeWorldAssets/props/crate_A_big";
                    target = Mathf.Max(1.4f, Mathf.Max(prop.width, prop.depth));
                    authoredScale = 1f;
                }
                else if (category == "greattrees")
                {
                    resource = "ClaudeWorldAssets/nature/trees_A_large";
                    target = Mathf.Max(5.5f, prop.radius * 3.2f);
                    authoredScale = 1f;
                }
                else if (category == "decorprops")
                {
                    resource = DecorResource(prop.key);
                    target = DecorTargetFootprint(prop.key, prop.radius);
                    // ClaudeCraft's decor scale is an authoring scalar (often 5-7),
                    // not a Unity metres multiplier. Applying it twice caused giants.
                    authoredScale = 1f;
                }
                else if (category == "docks")
                {
                    resource = "ClaudeWorldAssets/buildings/neutral/building_bridge_A";
                    target = 7f;
                }
                else if (category == "mudhuts")
                {
                    resource = "ClaudeWorldAssets/buildings/blue/building_home_B_blue";
                    target = 5.5f;
                }
                else if (category == "graveyards")
                {
                    resource = "ClaudeWorldAssets/buildings/blue/building_church_blue";
                    target = 4.5f;
                }
                else if (category == "stalls")
                {
                    resource = "ClaudeWorldAssets/props/tent";
                    target = Mathf.Max(2.8f, Mathf.Max(prop.width, prop.depth));
                    authoredScale = 1f;
                }
                else if (category == "ruinrings")
                {
                    resource = "ClaudeWorldAssets/buildings/neutral/building_destroyed";
                    target = Mathf.Max(4.5f, prop.radius * 2f);
                    authoredScale = 1f;
                }
                else if (category == "marshreeds")
                {
                    resource = "ClaudeWorldAssets/nature/waterplant_B";
                    target = 1.2f;
                    authoredScale = 1f;
                }
                else if (category == "racejump")
                {
                    resource = "ClaudeWorldAssets/buildings/neutral/building_bridge_B";
                    target = Mathf.Max(3.5f, prop.width);
                    authoredScale = 1f;
                }
                else if (category == "delvemarkers")
                {
                    resource = "ClaudeWorldAssets/nature/rock_single_C";
                    target = 1.6f;
                    authoredScale = 1f;
                }
                else if (category == "racearch")
                {
                    resource = "ClaudeWorldAssets/buildings/neutral/wall_straight_gate";
                    target = 5f;
                    authoredScale = 1f;
                }

                if (resource != null)
                    CreateWorldPrefab(
                        parent,
                        resource,
                        "Prop_" + prop.id,
                        prop.x,
                        prop.z,
                        prop.rot,
                        target,
                        authoredScale);
            }
        }

        private void BuildDecorations()
        {
            if (_data.decorations == null || _data.decorations.Length == 0)
                return;

            Transform parent = NewGroup("Decorations");
            HashSet<string> spawned = new HashSet<string>();
            int spawnedCount = 0;

            float startX;
            float startZ;
            ResolveWorldLabSpawn(out startX, out startZ);
            const float priorityRadius = 180f;
            float priorityRadiusSq = priorityRadius * priorityRadius;

            // First guarantee the area the player actually sees after loading.
            // v0.2 used a global "first 950" cap; Eastbrook spawn decorations are
            // later in the export, so the starting landscape looked empty.
            for (int i = 0; i < _data.decorations.Length; i++)
            {
                Decoration decor = _data.decorations[i];
                if (decor == null)
                    continue;

                float dx = decor.x - startX;
                float dz = decor.z - startZ;
                if (dx * dx + dz * dz > priorityRadiusSq)
                    continue;

                if (TrySpawnDecoration(parent, decor, i))
                {
                    spawned.Add(DecorationKey(decor, i));
                    spawnedCount++;
                }
            }

            // Then distribute a mobile/WebGL-safe budget across every zone instead
            // of exhausting the entire budget in the first zones of the JSON.
            const int maxPerZone = 150;
            const int maxTotal = 2500;

            if (_data.zones != null)
            {
                for (int zi = 0; zi < _data.zones.Length && spawnedCount < maxTotal; zi++)
                {
                    Zone zone = _data.zones[zi];
                    if (zone == null)
                        continue;

                    int zoneCount = 0;
                    for (int i = 0; i < _data.decorations.Length && spawnedCount < maxTotal; i++)
                    {
                        Decoration decor = _data.decorations[i];
                        if (decor == null || zoneCount >= maxPerZone)
                            continue;

                        string key = DecorationKey(decor, i);
                        if (spawned.Contains(key) || !ContainsPoint(zone, decor.x, decor.z))
                            continue;

                        if (!TrySpawnDecoration(parent, decor, i))
                            continue;

                        spawned.Add(key);
                        spawnedCount++;
                        zoneCount++;
                    }
                }
            }

            Debug.Log("HIGHFLY ClaudeCraft decorations spawned=" + spawnedCount +
                      " / exported=" + _data.decorations.Length);
        }

        private bool TrySpawnDecoration(Transform parent, Decoration decor, int index)
        {
            string kind = (decor.kind ?? string.Empty).ToLowerInvariant();
            string resource = null;
            float size = 2.2f;

            if (kind.Contains("tree") || kind.Contains("pine") || kind.Contains("oak"))
            {
                resource = (index & 1) == 0
                    ? "ClaudeWorldAssets/nature/tree_single_A"
                    : "ClaudeWorldAssets/nature/tree_single_B";
                size = 3.4f * Mathf.Max(0.72f, decor.scale);
            }
            else if (kind.Contains("rock") || kind.Contains("stone"))
            {
                resource = (index % 3) == 0
                    ? "ClaudeWorldAssets/nature/rock_single_C"
                    : ((index & 1) == 0
                        ? "ClaudeWorldAssets/nature/rock_single_A"
                        : "ClaudeWorldAssets/nature/rock_single_B");
                size = 1.5f * Mathf.Max(0.65f, decor.scale);
            }
            else if (kind.Contains("water") || kind.Contains("lily") || kind.Contains("reed") ||
                     kind.Contains("grass") || kind.Contains("shrub") || kind.Contains("flower") ||
                     kind.Contains("bush") || kind.Contains("fern") || kind.Contains("foliage"))
            {
                resource = (index & 1) == 0
                    ? "ClaudeWorldAssets/nature/waterplant_A"
                    : "ClaudeWorldAssets/nature/waterlily_A";
                size = 1.2f * Mathf.Max(0.65f, decor.scale);
            }
            else if (kind.Contains("palm") || kind.Contains("forest") || kind.Contains("grove"))
            {
                resource = (index % 3) == 0
                    ? "ClaudeWorldAssets/nature/trees_A_medium"
                    : "ClaudeWorldAssets/nature/tree_single_A";
                size = 3.8f * Mathf.Max(0.72f, decor.scale);
            }
            else
            {
                return false;
            }

            return CreateWorldPrefab(
                parent,
                resource,
                "Decor_" + DecorationKey(decor, index),
                decor.x,
                decor.z,
                decor.rotation,
                size,
                1f) != null;
        }

        private static string DecorationKey(Decoration decor, int index)
        {
            return decor != null && !string.IsNullOrEmpty(decor.id)
                ? decor.id
                : "index_" + index;
        }

        private static bool ContainsPoint(Zone zone, float x, float z)
        {
            return zone != null &&
                   x >= zone.xMin && x <= zone.xMax &&
                   z >= zone.zMin && z <= zone.zMax;
        }

        private static string BuildingResource(string kind)
        {
            if (kind.Contains("inn") || kind.Contains("tavern")) return "ClaudeWorldAssets/buildings/blue/building_tavern_blue";
            if (kind.Contains("smith") || kind.Contains("forge")) return "ClaudeWorldAssets/buildings/blue/building_blacksmith_blue";
            if (kind.Contains("market") || kind.Contains("shop")) return "ClaudeWorldAssets/buildings/blue/building_market_blue";
            if (kind.Contains("chapel") || kind.Contains("church") || kind.Contains("temple")) return "ClaudeWorldAssets/buildings/blue/building_church_blue";
            if (kind.Contains("castle") || kind.Contains("keep")) return "ClaudeWorldAssets/buildings/blue/building_castle_blue";
            if (kind.Contains("tower")) return "ClaudeWorldAssets/buildings/blue/building_tower_A_blue";
            if (kind.Contains("barrack") || kind.Contains("guard")) return "ClaudeWorldAssets/buildings/blue/building_barracks_blue";
            if (kind.Contains("archer")) return "ClaudeWorldAssets/buildings/blue/building_archeryrange_blue";
            if (kind.Contains("lumber")) return "ClaudeWorldAssets/buildings/blue/building_lumbermill_blue";
            if (kind.Contains("windmill")) return "ClaudeWorldAssets/buildings/blue/building_windmill_blue";
            if (kind.Contains("watermill")) return "ClaudeWorldAssets/buildings/blue/building_watermill_blue";
            if (kind.Contains("mine")) return "ClaudeWorldAssets/buildings/blue/building_mine_blue";
            return kind.GetHashCode() % 2 == 0
                ? "ClaudeWorldAssets/buildings/blue/building_home_A_blue"
                : "ClaudeWorldAssets/buildings/blue/building_home_B_blue";
        }

        private static string DecorResource(string key)
        {
            string value = (key ?? string.Empty).ToLowerInvariant();

            if (value.Contains("tree")) return "ClaudeWorldAssets/nature/tree_single_A";
            if (value.Contains("rock") || value.Contains("crystal")) return "ClaudeWorldAssets/nature/rock_single_C";
            if (value.Contains("tent")) return "ClaudeWorldAssets/props/tent";
            if (value.Contains("crate")) return "ClaudeWorldAssets/props/crate_A_big";
            if (value.Contains("sack")) return "ClaudeWorldAssets/props/sack";
            if (value.Contains("barrel")) return "ClaudeWorldAssets/props/barrel";
            if (value.Contains("watchtower") || value.Contains("tower")) return "ClaudeWorldAssets/buildings/blue/building_tower_A_blue";
            if (value.Contains("blacksmith") || value.Contains("forge")) return "ClaudeWorldAssets/buildings/blue/building_blacksmith_blue";
            if (value.Contains("barracks") || value.Contains("barrack")) return "ClaudeWorldAssets/buildings/blue/building_barracks_blue";
            if (value.Contains("tavern") || value.Contains("inn")) return "ClaudeWorldAssets/buildings/blue/building_tavern_blue";
            if (value.Contains("market")) return "ClaudeWorldAssets/buildings/blue/building_market_blue";
            if (value.Contains("church") || value.Contains("chapel")) return "ClaudeWorldAssets/buildings/blue/building_church_blue";
            if (value.Contains("castle") || value.Contains("keep")) return "ClaudeWorldAssets/buildings/blue/building_castle_blue";
            if (value.Contains("mine")) return "ClaudeWorldAssets/buildings/blue/building_mine_blue";
            if (value.Contains("windmill")) return "ClaudeWorldAssets/buildings/blue/building_windmill_blue";
            if (value.Contains("lumber")) return "ClaudeWorldAssets/buildings/blue/building_lumbermill_blue";
            if (value.Contains("bridge") || value.Contains("dock")) return "ClaudeWorldAssets/buildings/neutral/building_bridge_A";
            if (value.Contains("fence") || value.Contains("iron")) return "ClaudeWorldAssets/buildings/neutral/fence_wood_straight";
            if (value.Contains("shrub") || value.Contains("flower") || value.Contains("reed") || value.Contains("grass") || value.Contains("bush"))
                return "ClaudeWorldAssets/nature/waterplant_A";

            // Boats/ships/anchors/buoys/torches intentionally remain unmapped here.
            // We do not fake them with unrelated geometry; they get a proper CC0 donor
            // in the next art pass.
            return null;
        }

        private static float DecorTargetFootprint(string key, float radius)
        {
            string value = (key ?? string.Empty).ToLowerInvariant();

            if (value.Contains("sack")) return 0.65f;
            if (value.Contains("barrel")) return 0.8f;
            if (value.Contains("crate")) return Mathf.Max(0.9f, radius * 1.4f);
            if (value.Contains("watchtower")) return Mathf.Max(5f, radius * 2f);
            if (value.Contains("fence")) return 2.5f;
            if (value.Contains("shrub") || value.Contains("flower") || value.Contains("reed")) return 0.9f;
            if (value.Contains("rock") || value.Contains("crystal")) return Mathf.Max(1.1f, radius * 2f);
            if (value.Contains("tent")) return 3f;

            return Mathf.Max(1f, radius * 2f);
        }

        private GameObject CreateWorldPrefab(
            Transform parent,
            string resourcePath,
            string name,
            float sourceX,
            float sourceZ,
            float sourceRot,
            float targetFootprint,
            float authoredScale)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning("HIGHFLY ClaudeCraft: missing Resources prefab " + resourcePath);
                return null;
            }

            GameObject go = Instantiate(prefab, parent);
            go.name = name;
            go.transform.position = new Vector3(ToUnityX(sourceX), SampleHeight(sourceX, sourceZ), sourceZ);
            go.transform.rotation = Quaternion.Euler(0f, SourceYawToUnity(sourceRot), 0f);

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                float footprint = Mathf.Max(0.01f, Mathf.Max(bounds.size.x, bounds.size.z));
                float factor = Mathf.Clamp(targetFootprint / footprint, 0.18f, 8f) * Mathf.Max(0.1f, authoredScale);
                go.transform.localScale = Vector3.one * factor;

                // Re-base to terrain after scaling so the asset's lowest visible point
                // sits on the sampled ClaudeCraft ground.
                renderers = go.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                    float ground = SampleHeight(sourceX, sourceZ);
                    go.transform.position += Vector3.up * (ground - bounds.min.y + 0.02f);
                }
            }

            AddSimpleCollider(go);
            return go;
        }

        private void BuildFenceProp(Transform parent, PropData prop, bool wall)
        {
            float dx = prop.x2 - prop.x;
            float dz = prop.z2 - prop.z;
            float length = Mathf.Sqrt(dx * dx + dz * dz);
            if (length < 0.2f) return;

            float mx = (prop.x + prop.x2) * 0.5f;
            float mz = (prop.z + prop.z2) * 0.5f;
            string resource = wall
                ? "ClaudeWorldAssets/buildings/neutral/wall_straight"
                : "ClaudeWorldAssets/buildings/neutral/fence_wood_straight";

            GameObject go = CreateWorldPrefab(parent, resource, "Fence_" + prop.id, mx, mz, 0f, Mathf.Max(1.5f, length), 1f);
            if (go != null)
            {
                float yaw = Mathf.Atan2(-dx, dz) * Mathf.Rad2Deg;
                go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }

        private static void AddSimpleCollider(GameObject go)
        {
            if (go.GetComponentInChildren<Collider>() != null)
                return;

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.center = go.transform.InverseTransformPoint(b.center);
            Vector3 localSize = go.transform.InverseTransformVector(b.size);
            collider.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        }

        private static float SourceYawToUnity(float sourceRot)
        {
            float degrees = Mathf.Abs(sourceRot) <= Mathf.PI * 2.2f
                ? sourceRot * Mathf.Rad2Deg
                : sourceRot;
            return -degrees;
        }

        private void BuildWorldMarkers()
        {
            Transform parent = NewGroup("WorldMarkers");

            if (_data.camps != null)
            {
                for (int i = 0; i < _data.camps.Length; i++)
                {
                    Camp camp = _data.camps[i];
                    CreateWorldPrefab(
                        parent,
                        "ClaudeWorldAssets/props/tent",
                        "Camp_" + (string.IsNullOrEmpty(camp.mobId) ? i.ToString() : camp.mobId),
                        camp.x,
                        camp.z,
                        0f,
                        Mathf.Clamp(2.2f + camp.count * 0.12f, 2.2f, 3.8f),
                        1f);
                }
            }

            if (_data.gatherNodes != null)
            {
                for (int i = 0; i < _data.gatherNodes.Length; i++)
                {
                    GatherNode node = _data.gatherNodes[i];
                    string resource = node.type == "ore"
                        ? "ClaudeWorldAssets/props/resource_stone"
                        : node.type == "wood"
                            ? "ClaudeWorldAssets/props/resource_lumber"
                            : "ClaudeWorldAssets/nature/waterplant_B";
                    CreateWorldPrefab(
                        parent,
                        resource,
                        "Gather_" + node.id,
                        node.x,
                        node.z,
                        0f,
                        0.9f + Mathf.Clamp(node.tier, 1, 6) * 0.12f,
                        1f);
                }
            }

            if (_data.portals != null)
            {
                for (int i = 0; i < _data.portals.Length; i++)
                    CreatePortalLandmark(parent, _data.portals[i]);
            }
        }


        private void CreatePortalLandmark(Transform parent, Portal portal)
        {
            if (portal == null)
                return;

            GameObject root = new GameObject("Portal_" + portal.id);
            root.transform.SetParent(parent, false);
            float ground = SampleHeight(portal.x, portal.z);
            root.transform.position = new Vector3(ToUnityX(portal.x), ground, portal.z);

            CreateWorldPrefab(
                root.transform,
                "ClaudeWorldAssets/buildings/neutral/wall_straight_gate",
                "PortalFrame_" + portal.id,
                portal.x,
                portal.z,
                0f,
                5.2f,
                1f);

            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            surface.name = "PortalSurface_" + portal.id;
            surface.transform.SetParent(root.transform, false);
            surface.transform.localPosition = new Vector3(0f, 2.15f, 0.08f);
            surface.transform.localScale = new Vector3(2.25f, 3.15f, 1f);
            MeshRenderer renderer = surface.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = GetMaterial("portal_surface", new Color(0.16f, 0.62f, 0.95f, 0.86f));
            Collider surfaceCollider = surface.GetComponent<Collider>();
            if (surfaceCollider != null)
                Destroy(surfaceCollider);

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 2.0f, 0f);
            trigger.size = new Vector3(3.2f, 4.2f, 2.0f);

            HighflyWorldPortalTrigger portalTrigger = root.AddComponent<HighflyWorldPortalTrigger>();
            portalTrigger.Configure(
                new Vector3(
                    ToUnityX(portal.targetX),
                    SampleHeight(portal.targetX, portal.targetZ) + 1.1f,
                    portal.targetZ),
                portal.id);
        }

        private void BuildNpcPopulation()
        {
            if (_entities == null || _entities.npcs == null || _entities.npcs.Length == 0)
                return;

            Transform parent = NewGroup("AuthoredNPCs");
            RuntimeAnimatorController sharedController = null;
            HighflyThirdPersonMotor motor = _playerMotor != null ? _playerMotor : FindObjectOfType<HighflyThirdPersonMotor>();
            if (motor != null)
            {
                Animator playerAnimator = motor.GetComponentInChildren<Animator>();
                if (playerAnimator != null)
                    sharedController = playerAnimator.runtimeAnimatorController;
            }

            int spawned = 0;
            for (int i = 0; i < _entities.npcs.Length; i++)
            {
                NpcData npc = _entities.npcs[i];
                if (npc == null || string.IsNullOrEmpty(npc.id))
                    continue;

                string resource = NpcResource(npc, i);
                GameObject go = CreateWorldPrefab(
                    parent,
                    resource,
                    "NPC_" + npc.id,
                    npc.x,
                    npc.z,
                    npc.facing,
                    0.82f,
                    1f);
                if (go == null)
                    continue;

                Animator animator = go.GetComponentInChildren<Animator>();
                if (animator != null && sharedController != null)
                {
                    animator.runtimeAnimatorController = sharedController;
                    animator.applyRootMotion = false;
                    animator.SetFloat("Speed", 0f);
                }

                CreateNpcNameplate(go, npc);

                HighflyWorldNpcInteraction interaction = go.AddComponent<HighflyWorldNpcInteraction>();
                interaction.Configure(
                    npc.name,
                    npc.title,
                    npc.greeting,
                    string.IsNullOrEmpty(npc.role) ? "habitante" : npc.role);

                spawned++;
            }

            Debug.Log("HIGHFLY ClaudeCraft authored NPCs spawned=" + spawned);
        }

        private static string NpcResource(NpcData npc, int index)
        {
            string role = (npc.role ?? string.Empty).ToLowerInvariant();
            string id = (npc.id ?? string.Empty).ToLowerInvariant();

            if (role.Contains("guard") || role.Contains("marshal") || id.Contains("marshal") || id.Contains("warden"))
                return "ClaudeWorldAssets/characters/Knight";
            if (role.Contains("mage") || role.Contains("healer") || id.Contains("apothecary") || id.Contains("priest"))
                return "ClaudeWorldAssets/characters/Mage";
            if (role.Contains("farmer") || id.Contains("foreman"))
                return "ClaudeWorldAssets/characters/Barbarian";
            if (role.Contains("vendor") || role.Contains("banker") || role.Contains("market"))
                return (index & 1) == 0
                    ? "ClaudeWorldAssets/characters/Rogue"
                    : "ClaudeWorldAssets/characters/Mage";

            switch (index % 4)
            {
                case 0: return "ClaudeWorldAssets/characters/Knight";
                case 1: return "ClaudeWorldAssets/characters/Rogue";
                case 2: return "ClaudeWorldAssets/characters/Mage";
                default: return "ClaudeWorldAssets/characters/Barbarian";
            }
        }

        private static void CreateNpcNameplate(GameObject npcObject, NpcData npc)
        {
            Renderer[] renderers = npcObject.GetComponentsInChildren<Renderer>();
            float y = npcObject.transform.position.y + 2.0f;
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);
                y = bounds.max.y + 0.32f;
            }

            GameObject label = new GameObject("Nameplate");
            label.transform.SetParent(npcObject.transform, true);
            label.transform.position = new Vector3(npcObject.transform.position.x, y, npcObject.transform.position.z);

            TextMesh text = label.AddComponent<TextMesh>();
            text.text = string.IsNullOrEmpty(npc.title)
                ? npc.name
                : npc.name + "\n<size=70%>" + npc.title + "</size>";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 44;
            text.characterSize = 0.035f;
            text.color = Color.white;

            label.AddComponent<HighflyWorldBillboard>();
        }

        private void BuildDungeonEntrances()
        {
            if (_entities == null || _entities.dungeons == null)
                return;

            Transform parent = NewGroup("DungeonEntrances");
            int spawned = 0;

            for (int i = 0; i < _entities.dungeons.Length; i++)
            {
                DungeonData dungeon = _entities.dungeons[i];
                if (dungeon == null || string.IsNullOrEmpty(dungeon.id))
                    continue;

                GameObject gate = CreateWorldPrefab(
                    parent,
                    "ClaudeWorldAssets/buildings/neutral/wall_straight_gate",
                    "Dungeon_" + dungeon.id,
                    dungeon.x,
                    dungeon.z,
                    0f,
                    dungeon.staticDoor ? 6.0f : 4.8f,
                    1f);

                if (gate == null)
                    continue;

                Renderer[] renderers = gate.GetComponentsInChildren<Renderer>();
                float labelY = gate.transform.position.y + 3.5f;
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int r = 1; r < renderers.Length; r++)
                        b.Encapsulate(renderers[r].bounds);
                    labelY = b.max.y + 0.45f;
                }

                GameObject label = new GameObject("DungeonName");
                label.transform.SetParent(gate.transform, true);
                label.transform.position = new Vector3(gate.transform.position.x, labelY, gate.transform.position.z);
                TextMesh text = label.AddComponent<TextMesh>();
                text.text = dungeon.name + "\n" + Mathf.Max(1, dungeon.suggestedPlayers) + " JUG.";
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.fontSize = 48;
                text.characterSize = 0.04f;
                text.color = new Color(0.78f, 0.88f, 1f, 1f);
                label.AddComponent<HighflyWorldBillboard>();

                HighflyWorldLandmarkInteraction interaction = gate.AddComponent<HighflyWorldLandmarkInteraction>();
                interaction.Configure(
                    dungeon.name,
                    string.IsNullOrEmpty(dungeon.enterText) ? "Entrada de mazmorra de ClaudeCraft." : dungeon.enterText,
                    "EXAMINAR");

                spawned++;
            }

            Debug.Log("HIGHFLY ClaudeCraft dungeon entrances spawned=" + spawned);
        }

        private void CreateMarker(
            Transform parent,
            string name,
            PrimitiveType primitive,
            float sourceX,
            float sourceZ,
            float scale,
            Material material)
        {
            GameObject go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parent, false);
            float y = SampleHeight(sourceX, sourceZ);
            go.transform.position = new Vector3(ToUnityX(sourceX), y + scale, sourceZ);
            go.transform.localScale = Vector3.one * Mathf.Max(0.12f, scale);
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private void ConfigurePlayerAndCamera()
        {
            HighflyThirdPersonMotor motor = FindObjectOfType<HighflyThirdPersonMotor>();
            if (motor == null)
            {
                Debug.LogWarning("HIGHFLY ClaudeCraft: player motor not found.");
                return;
            }

            float sourceX;
            float sourceZ;
            if (!TryFindTownSpawn(out sourceX, out sourceZ))
            {
                sourceX = _data.playerStart != null ? _data.playerStart.x : 0f;
                sourceZ = _data.playerStart != null ? _data.playerStart.z : 0f;
            }

            Vector3 spawn = new Vector3(ToUnityX(sourceX), SampleHeight(sourceX, sourceZ) + 2.2f, sourceZ);
            _playerMotor = motor;

            CharacterController controller = motor.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled) controller.enabled = false;
            motor.transform.position = spawn;
            motor.transform.rotation = Quaternion.identity;
            if (wasEnabled) controller.enabled = true;

            _lastDryPosition = spawn;
            _hasLastDryPosition = true;

            HighflyWorldSafety safety = motor.GetComponent<HighflyWorldSafety>();
            if (safety != null)
            {
                safety.Configure(spawn, WorldMinHeight() - 25f, true, true);
                safety.SetRespawnPosition(spawn);
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.farClipPlane = 1200f;
                camera.nearClipPlane = 0.08f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.33f, 0.48f, 0.63f, 1f);
            }
        }


        private bool TryFindTownSpawn(out float sourceX, out float sourceZ)
        {
            // WORLD LAB is a world-experience test, so it must open inside a real
            // settlement. Eastbrook Vale is ClaudeCraft v0.43.3's authored starter
            // town hub at (-14,-100). Prefer the parsed source record, then use the
            // canonical source coordinate as a last-resort pin.
            if (_data != null && _data.zones != null)
            {
                for (int i = 0; i < _data.zones.Length; i++)
                {
                    Zone zone = _data.zones[i];
                    if (zone == null || zone.hub == null)
                        continue;
                    if (!string.Equals(zone.id, "eastbrook_vale", StringComparison.OrdinalIgnoreCase))
                        continue;

                    sourceX = zone.hub.x;
                    sourceZ = zone.hub.z;
                    Debug.Log("HIGHFLY WORLD spawn: Eastbrook zone hub=(" + sourceX + "," + sourceZ + ")");
                    return true;
                }
            }

            if (_structure != null && _structure.citySpawn != null)
            {
                sourceX = _structure.citySpawn.x;
                sourceZ = _structure.citySpawn.z;
                Debug.Log("HIGHFLY WORLD spawn: authored structure hub=(" + sourceX + "," + sourceZ + ")");
                return true;
            }

            sourceX = -14f;
            sourceZ = -100f;
            Debug.LogWarning("HIGHFLY WORLD spawn: source hub record missing; using ClaudeCraft v0.43.3 Eastbrook canonical coordinate.");
            return true;
        }

        private void ResolveWorldLabSpawn(out float sourceX, out float sourceZ)
        {
            if (TryFindTownSpawn(out sourceX, out sourceZ))
                return;

            sourceX = -14f;
            sourceZ = -100f;
        }

        private void EnsureEastbrookVisible()
        {
            const float hubX = -14f;
            const float hubZ = -100f;
            const float radius = 92f;

            Transform props = _worldRoot != null ? _worldRoot.Find("AuthoredProps") : null;
            int visibleNearHub = 0;
            if (props != null)
            {
                for (int i = 0; i < props.childCount; i++)
                {
                    Transform child = props.GetChild(i);
                    Vector3 p = child.position;
                    float dx = p.x - ToUnityX(hubX);
                    float dz = p.z - hubZ;
                    if (dx * dx + dz * dz <= radius * radius &&
                        child.GetComponentsInChildren<Renderer>(true).Length > 0)
                        visibleNearHub++;
                }
            }

            Debug.Log("HIGHFLY WORLD Eastbrook authored visuals near hub=" + visibleNearHub);
            if (visibleNearHub >= 7)
                return;

            Debug.LogWarning("HIGHFLY WORLD: Eastbrook visual importer produced too little near the hub; building the canonical v0.43.3 town slice.");

            Transform city = NewGroup("EastbrookGuaranteed");
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_A_blue", "Eastbrook_Bank", 12f, -94f, -2.3561945f, 8.6f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_blacksmith_blue", "Eastbrook_Smithy", -2f, -122f, -2.0344439f, 8.4f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_tavern_blue", "Eastbrook_Inn", -38f, -88f, -2.5535901f, 8.6f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_church_blue", "Eastbrook_Chapel", 2f, -78f, 0.7853982f, 6.9f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_A_blue", "Eastbrook_Weaving", -28f, -122f, 2.5535901f, 6.9f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_B_blue", "Eastbrook_Toolworks", -16f, -128f, 0.5880026f, 6.9f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_A_blue", "Eastbrook_MarketHome", -33f, -111f, 0.2f, 6.9f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_B_blue", "Eastbrook_EastHome", 22f, -106f, -1.46f, 6.9f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_B_blue", "Eastbrook_QuaysideHome", -82f, -102f, -2.2f, 6.9f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_market_blue", "Eastbrook_HarbourMarket", -68f, -108f, -2.2f, 8.6f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_home_A_blue", "Eastbrook_DockHome", -50f, -112f, -2.2f, 6.9f, 1f);

            CreateWorldPrefab(city, "ClaudeWorldAssets/buildings/blue/building_well_blue", "Eastbrook_Well", -14f, -100f, 0f, 2.8f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/props/tent", "Eastbrook_MarketStall_A", -20.5f, -94f, 2.4805495f, 2.8f, 1f);
            CreateWorldPrefab(city, "ClaudeWorldAssets/props/tent", "Eastbrook_MarketStall_B", -19f, -108f, 0.6610432f, 2.8f, 1f);

            Transform roads = NewGroup("EastbrookGuaranteedRoads");
            Material road = GetMaterial("eastbrook_road_surface", new Color(0.34f, 0.25f, 0.15f, 1f));

            CreateGuaranteedRoad(roads, road, 2f, new[]
            {
                new Point { x = -20f, z = -102f }, new Point { x = -26f, z = -101f },
                new Point { x = -44f, z = -98f }, new Point { x = -56f, z = -88f },
                new Point { x = -62f, z = -76f }, new Point { x = -70f, z = -68f },
                new Point { x = -80f, z = -66f }, new Point { x = -88f, z = -60f },
                new Point { x = -92f, z = -56f }
            }, "Eastbrook_MainStreet");

            CreateGuaranteedRoad(roads, road, 1.5f, new[]
            {
                new Point { x = -11f, z = -105.5f }, new Point { x = -10f, z = -112f },
                new Point { x = -9f, z = -119f }, new Point { x = -12f, z = -123f },
                new Point { x = -22f, z = -120.5f }
            }, "Eastbrook_CraftsLane");

            CreateGuaranteedRoad(roads, road, 1.5f, new[]
            {
                new Point { x = 14.6f, z = -88.6f }, new Point { x = 12f, z = -85f },
                new Point { x = 10f, z = -80f }, new Point { x = 6f, z = -72f },
                new Point { x = 0f, z = -58f }
            }, "Eastbrook_NorthRoad");
        }

        private void CreateGuaranteedRoad(Transform parent, Material material, float halfWidth, Point[] points, string namePrefix)
        {
            if (points == null || points.Length < 2)
                return;

            for (int i = 0; i < points.Length - 1; i++)
            {
                Point p0 = points[i];
                Point p1 = points[i + 1];
                Vector3 a = new Vector3(ToUnityX(p0.x), SampleHeight(p0.x, p0.z) + 0.10f, p0.z);
                Vector3 b = new Vector3(ToUnityX(p1.x), SampleHeight(p1.x, p1.z) + 0.10f, p1.z);
                Vector3 dir = b - a;
                dir.y = 0f;
                float length = dir.magnitude;
                if (length < 0.1f)
                    continue;

                GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.name = namePrefix + "_" + i;
                strip.transform.SetParent(parent, false);
                strip.transform.position = (a + b) * 0.5f;
                strip.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                strip.transform.localScale = new Vector3(halfWidth * 2f, 0.08f, length);

                Renderer renderer = strip.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = material;

                Collider collider = strip.GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);
            }
        }

        private void EnsureEastbrookVegetation()
        {
            if (_data == null || _data.decorations == null || _data.decorations.Length == 0)
                return;

            const float hubX = -14f;
            const float hubZ = -100f;
            const float radius = 220f;
            float radiusSq = radius * radius;

            Transform parent = _worldRoot != null ? _worldRoot.Find("Decorations") : null;
            if (parent == null)
                parent = NewGroup("Decorations");

            int nearHub = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                Vector3 p = parent.GetChild(i).position;
                float sourceX = -p.x;
                float dx = sourceX - hubX;
                float dz = p.z - hubZ;
                if (dx * dx + dz * dz <= radiusSq)
                    nearHub++;
            }

            if (nearHub >= 80)
            {
                Debug.Log("HIGHFLY WORLD Eastbrook vegetation near hub=" + nearHub);
                return;
            }

            int added = 0;
            for (int i = 0; i < _data.decorations.Length && nearHub + added < 180; i++)
            {
                Decoration decor = _data.decorations[i];
                if (decor == null)
                    continue;

                float dx = decor.x - hubX;
                float dz = decor.z - hubZ;
                if (dx * dx + dz * dz > radiusSq)
                    continue;

                string key = "Decor_" + DecorationKey(decor, i);
                if (GameObject.Find(key) != null)
                    continue;

                if (TrySpawnDecoration(parent, decor, i))
                    added++;
            }

            Debug.Log("HIGHFLY WORLD Eastbrook source vegetation added=" + added + " previous=" + nearHub);
        }

        private bool IsWaterAt(float sourceX, float sourceZ)
        {
            if (_data == null || _data.terrain == null)
                return false;

            for (int i = 0; i < _data.terrain.Length; i++)
            {
                TerrainZone zone = _data.terrain[i];
                if (zone == null || zone.waterMask == null || zone.waterMask.Length == 0)
                    continue;
                if (sourceX < zone.minX || sourceX > zone.maxX || sourceZ < zone.minZ || sourceZ > zone.maxZ)
                    continue;

                int x = Mathf.Clamp(
                    Mathf.RoundToInt((sourceX - zone.minX) / Mathf.Max(0.001f, zone.step)),
                    0,
                    zone.width - 1);
                int z = Mathf.Clamp(
                    Mathf.RoundToInt((sourceZ - zone.minZ) / Mathf.Max(0.001f, zone.step)),
                    0,
                    zone.depth - 1);
                int index = z * zone.width + x;
                return index >= 0 && index < zone.waterMask.Length && zone.waterMask[index] != 0;
            }

            return false;
        }

        private void MaintainWaterTraversal()
        {
            if (_playerMotor == null || _data == null)
                return;

            Vector3 position = _playerMotor.transform.position;
            float sourceX = -position.x;
            float sourceZ = position.z;
            float ground = SampleHeight(sourceX, sourceZ);
            bool water = IsWaterAt(sourceX, sourceZ);

            if (!water)
            {
                if (position.y >= ground - 0.8f)
                {
                    _lastDryPosition = position;
                    _hasLastDryPosition = true;
                }
                return;
            }

            float depth = _data.waterLevel - ground;
            if (depth <= 0.75f)
                return;

            // World Lab traversal only: keep the hunter at the surface instead
            // of letting the CharacterController sink to the seabed. A dedicated
            // swimming locomotion state can replace this without touching world data.
            float surfaceRootY = _data.waterLevel - 0.38f;
            if (position.y >= surfaceRootY)
                return;

            CharacterController controller = _playerMotor.GetComponent<CharacterController>();
            bool enabled = controller != null && controller.enabled;
            if (enabled)
                controller.enabled = false;

            position.y = surfaceRootY;
            _playerMotor.transform.position = position;

            if (enabled)
                controller.enabled = true;
        }

        private void ConfigureWorldLabUi()
        {
            // WORLD LAB is for exploration only. Keep locomotion/camera/interaction,
            // but hide Combat Lab presentation and action controls in this build.
            string[] hideObjects =
            {
                "StatusPanel",
                "TargetPill",
                "ATAQUE_Button",
                "PESADO_Button",
                "S1_CORTE_Button",
                "S2_ABANICO_Button",
                "S3_ÁREA_Button",
                "BLOQ_Button"
            };

            for (int i = 0; i < hideObjects.Length; i++)
            {
                GameObject go = GameObject.Find(hideObjects[i]);
                if (go != null)
                    go.SetActive(false);
            }

            HighflyCombatHUD combatHud = FindObjectOfType<HighflyCombatHUD>();
            if (combatHud != null)
                combatHud.enabled = false;

            HighflyDungeonObjective objective = FindObjectOfType<HighflyDungeonObjective>();
            if (objective != null)
                objective.enabled = false;

            GameObject attachedSword = GameObject.Find("Hunter_Sword");
            if (attachedSword != null)
                attachedSword.SetActive(false);

            UnityEngine.UI.Text[] texts = FindObjectsOfType<UnityEngine.UI.Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                string value = texts[i] != null ? texts[i].text : string.Empty;
                if (value.Contains("CRIPTA F") || value.Contains("AUTO-TARGET ACTIVO"))
                    texts[i].gameObject.SetActive(false);
            }
        }

        private float WorldMinHeight()
        {
            float min = 0f;
            bool hasValue = false;
            if (_data != null && _data.terrain != null)
            {
                for (int i = 0; i < _data.terrain.Length; i++)
                {
                    TerrainZone zone = _data.terrain[i];
                    if (zone == null) continue;
                    if (!hasValue || zone.minHeight < min)
                    {
                        min = zone.minHeight;
                        hasValue = true;
                    }
                }
            }
            return hasValue ? min : -50f;
        }

        private float SampleHeight(float sourceX, float sourceZ)
        {
            if (_data == null || _data.terrain == null)
                return 0f;

            TerrainZone best = null;
            for (int i = 0; i < _data.terrain.Length; i++)
            {
                TerrainZone zone = _data.terrain[i];
                if (zone == null) continue;
                if (sourceX >= zone.minX && sourceX <= zone.maxX &&
                    sourceZ >= zone.minZ && sourceZ <= zone.maxZ)
                {
                    best = zone;
                    break;
                }
            }

            if (best == null || best.heights == null || best.heights.Length == 0)
                return 0f;

            float fx = Mathf.Clamp((sourceX - best.minX) / Mathf.Max(0.001f, best.step), 0f, best.width - 1);
            float fz = Mathf.Clamp((sourceZ - best.minZ) / Mathf.Max(0.001f, best.step), 0f, best.depth - 1);

            int x0 = Mathf.Clamp(Mathf.FloorToInt(fx), 0, best.width - 1);
            int z0 = Mathf.Clamp(Mathf.FloorToInt(fz), 0, best.depth - 1);
            int x1 = Mathf.Min(x0 + 1, best.width - 1);
            int z1 = Mathf.Min(z0 + 1, best.depth - 1);

            float tx = fx - x0;
            float tz = fz - z0;

            float h00 = HeightAt(best, x0, z0);
            float h10 = HeightAt(best, x1, z0);
            float h01 = HeightAt(best, x0, z1);
            float h11 = HeightAt(best, x1, z1);

            float a = Mathf.Lerp(h00, h10, tx);
            float b = Mathf.Lerp(h01, h11, tx);
            return Mathf.Lerp(a, b, tz);
        }

        private static float HeightAt(TerrainZone zone, int x, int z)
        {
            int index = z * zone.width + x;
            if (index < 0 || index >= zone.heights.Length)
                return 0f;
            return zone.heights[index];
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
            if (_materials.TryGetValue(key, out material))
                return material;

            Shader shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Diffuse");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            material = new Material(shader);
            material.name = "HIGHFLY_" + key;
            material.color = color;
            _materials[key] = material;
            return material;
        }

        private static float ToUnityX(float sourceX)
        {
            return -sourceX;
        }

        private static Color TerrainColor(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new Color(0.32f, 0.52f, 0.31f, 1f);

            string value = id.ToLowerInvariant();
            if (value.Contains("frost")) return new Color(0.58f, 0.68f, 0.72f, 1f);
            if (value.Contains("marsh") || value.Contains("willow")) return new Color(0.19f, 0.34f, 0.24f, 1f);
            if (value.Contains("drake")) return new Color(0.36f, 0.25f, 0.20f, 1f);
            if (value.Contains("night") || value.Contains("wraith")) return new Color(0.18f, 0.22f, 0.28f, 1f);
            if (value.Contains("palm") || value.Contains("farshore")) return new Color(0.32f, 0.50f, 0.28f, 1f);
            if (value.Contains("gale")) return new Color(0.34f, 0.46f, 0.34f, 1f);
            if (value.Contains("amber")) return new Color(0.48f, 0.38f, 0.22f, 1f);
            return new Color(0.34f, 0.55f, 0.33f, 1f);
        }
    }

    public sealed class HighflyWorldPortalTrigger : MonoBehaviour
    {
        private static float _nextUseAt;
        private Vector3 _destination;
        private string _portalId;

        public void Configure(Vector3 destination, string portalId)
        {
            _destination = destination;
            _portalId = portalId ?? "portal";
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.unscaledTime < _nextUseAt)
                return;

            HighflyThirdPersonMotor motor = other.GetComponentInParent<HighflyThirdPersonMotor>();
            if (motor == null)
                return;

            CharacterController controller = motor.GetComponent<CharacterController>();
            bool enabled = controller != null && controller.enabled;
            if (enabled)
                controller.enabled = false;

            motor.transform.position = _destination;

            if (enabled)
                controller.enabled = true;

            _nextUseAt = Time.unscaledTime + 1.25f;
            Debug.Log("HIGHFLY WORLD portal used: " + _portalId);
        }
    }

    public sealed class HighflyWorldNpcInteraction : MonoBehaviour
    {
        private string _displayName;
        private string _title;
        private string _greeting;
        private string _role;
        private HighflyThirdPersonMotor _player;
        private bool _open;

        public void Configure(string displayName, string title, string greeting, string role)
        {
            _displayName = string.IsNullOrEmpty(displayName) ? "Habitante" : displayName;
            _title = title ?? string.Empty;
            _greeting = string.IsNullOrEmpty(greeting) ? "..." : greeting;
            _role = role ?? "habitante";
        }

        private void Update()
        {
            if (_player == null)
                _player = FindObjectOfType<HighflyThirdPersonMotor>();
            if (_player != null && Vector3.Distance(transform.position, _player.transform.position) > 4.5f)
                _open = false;
        }

        private void OnGUI()
        {
            if (_player == null)
                return;

            float distance = Vector3.Distance(transform.position, _player.transform.position);
            if (distance > 3.0f)
                return;

            float width = Mathf.Min(360f, Screen.width * 0.64f);
            Rect button = new Rect((Screen.width - width) * 0.5f, Screen.height - 118f, width, 54f);
            if (GUI.Button(button, "HABLAR · " + _displayName))
                _open = !_open;

            if (!_open)
                return;

            string heading = string.IsNullOrEmpty(_title)
                ? _displayName
                : _displayName + " — " + _title;
            string body = heading + "\n[" + _role.ToUpperInvariant() + "]\n" + _greeting;
            GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height - 285f, width, 155f), body);
        }
    }

    public sealed class HighflyWorldLandmarkInteraction : MonoBehaviour
    {
        private string _title;
        private string _body;
        private string _verb;
        private HighflyThirdPersonMotor _player;
        private bool _open;

        public void Configure(string title, string body, string verb)
        {
            _title = title ?? "Lugar";
            _body = body ?? string.Empty;
            _verb = string.IsNullOrEmpty(verb) ? "INTERACTUAR" : verb;
        }

        private void Update()
        {
            if (_player == null)
                _player = FindObjectOfType<HighflyThirdPersonMotor>();
            if (_player != null && Vector3.Distance(transform.position, _player.transform.position) > 6f)
                _open = false;
        }

        private void OnGUI()
        {
            if (_player == null || Vector3.Distance(transform.position, _player.transform.position) > 4.5f)
                return;

            float width = Mathf.Min(380f, Screen.width * 0.68f);
            if (GUI.Button(new Rect((Screen.width - width) * 0.5f, Screen.height - 118f, width, 54f), _verb + " · " + _title))
                _open = !_open;

            if (_open)
                GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height - 265f, width, 135f), _title + "\n" + _body);
        }
    }
}
