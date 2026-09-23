using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Highfly.Combat;
using Highfly.Core;

namespace Highfly.World
{
    public sealed class HighflyClaudeWorldRuntime : MonoBehaviour
    {
        private const string ResourcePath = "ClaudeCraft/world";

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

        private WorldData _data;
        private Transform _worldRoot;
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
            BuildDecorations();
            BuildWorldMarkers();
            ConfigurePlayerAndCamera();

            Debug.Log(
                "HIGHFLY ClaudeCraft WORLD v0.2 loaded | zones=" + (_data.zones != null ? _data.zones.Length : 0) +
                " terrain=" + _data.terrain.Length +
                " roads=" + (_data.roads != null ? _data.roads.Length : 0) +
                " camps=" + (_data.camps != null ? _data.camps.Length : 0) +
                " gather=" + (_data.gatherNodes != null ? _data.gatherNodes.Length : 0) +
                " props=" + (_data.props != null ? _data.props.Length : 0) +
                " decorations=" + (_data.decorations != null ? _data.decorations.Length : 0));
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
                else if (category == "fences" || category == "walls")
                {
                    BuildFenceProp(parent, prop, category == "walls");
                    continue;
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

            float startX = _data.playerStart != null ? _data.playerStart.x : 0f;
            float startZ = _data.playerStart != null ? _data.playerStart.z : 0f;
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
            else if (kind.Contains("water") || kind.Contains("lily") || kind.Contains("reed"))
            {
                resource = (index & 1) == 0
                    ? "ClaudeWorldAssets/nature/waterplant_A"
                    : "ClaudeWorldAssets/nature/waterlily_A";
                size = 1.2f * Mathf.Max(0.65f, decor.scale);
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
            if (value.Contains("watchtower")) return "ClaudeWorldAssets/buildings/blue/building_tower_A_blue";
            if (value.Contains("fence")) return "ClaudeWorldAssets/buildings/neutral/fence_wood_straight";
            if (value.Contains("shrub") || value.Contains("flower") || value.Contains("reed"))
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
                Material portal = GetMaterial("portal", new Color(0.44f, 0.18f, 0.85f, 1f));
                for (int i = 0; i < _data.portals.Length; i++)
                {
                    Portal p = _data.portals[i];
                    CreateMarker(
                        parent,
                        "Portal_" + p.id,
                        PrimitiveType.Cylinder,
                        p.x,
                        p.z,
                        0.75f,
                        portal);
                }
            }
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

            float sourceX = _data.playerStart != null ? _data.playerStart.x : 0f;
            float sourceZ = _data.playerStart != null ? _data.playerStart.z : 0f;
            Vector3 spawn = new Vector3(ToUnityX(sourceX), SampleHeight(sourceX, sourceZ) + 2.2f, sourceZ);

            CharacterController controller = motor.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled) controller.enabled = false;
            motor.transform.position = spawn;
            motor.transform.rotation = Quaternion.identity;
            if (wasEnabled) controller.enabled = true;

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
}
