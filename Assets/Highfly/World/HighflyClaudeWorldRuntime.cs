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

            BuildTerrain();
            BuildWater();
            BuildRoads();
            BuildWorldMarkers();
            ConfigurePlayerAndCamera();

            Debug.Log(
                "HIGHFLY ClaudeCraft WORLD v0.1 loaded | zones=" + (_data.zones != null ? _data.zones.Length : 0) +
                " terrain=" + _data.terrain.Length +
                " roads=" + (_data.roads != null ? _data.roads.Length : 0) +
                " camps=" + (_data.camps != null ? _data.camps.Length : 0) +
                " gather=" + (_data.gatherNodes != null ? _data.gatherNodes.Length : 0));
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

                        // X is mirrored from ClaudeCraft into Unity. Reverse winding.
                        triangles[t++] = a;
                        triangles[t++] = c;
                        triangles[t++] = b;
                        triangles[t++] = b;
                        triangles[t++] = c;
                        triangles[t++] = d;
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
            if (_data.zones == null)
                return;

            Transform parent = NewGroup("Water");
            Material water = GetMaterial("water", new Color(0.05f, 0.32f, 0.58f, 0.72f));

            for (int zi = 0; zi < _data.zones.Length; zi++)
            {
                Zone zone = _data.zones[zi];
                if (zone == null || zone.lakes == null)
                    continue;

                for (int li = 0; li < zone.lakes.Length; li++)
                {
                    Lake lake = zone.lakes[li];
                    GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    disc.name = "Lake_" + zone.id + "_" + li;
                    disc.transform.SetParent(parent, false);
                    disc.transform.position = new Vector3(ToUnityX(lake.x), _data.waterLevel + 0.03f, lake.z);
                    float radius = Mathf.Max(0.5f, lake.radius * 1.6f);
                    disc.transform.localScale = new Vector3(radius * 2f, 0.025f, radius * 2f);
                    Renderer renderer = disc.GetComponent<Renderer>();
                    if (renderer != null) renderer.sharedMaterial = water;
                    Collider collider = disc.GetComponent<Collider>();
                    if (collider != null) Destroy(collider);
                }
            }
        }

        private void BuildRoads()
        {
            if (_data.roads == null)
                return;

            Transform parent = NewGroup("Roads");
            Material roadMaterial = GetMaterial("road", new Color(0.36f, 0.31f, 0.24f, 1f));

            for (int ri = 0; ri < _data.roads.Length; ri++)
            {
                Road road = _data.roads[ri];
                if (road == null || road.points == null || road.points.Length < 2)
                    continue;

                GameObject go = new GameObject(string.IsNullOrEmpty(road.id) ? "Road_" + ri : road.id);
                go.transform.SetParent(parent, false);
                LineRenderer line = go.AddComponent<LineRenderer>();
                line.sharedMaterial = roadMaterial;
                line.widthMultiplier = 0.55f;
                line.numCornerVertices = 2;
                line.numCapVertices = 2;
                line.useWorldSpace = true;
                line.positionCount = road.points.Length;

                for (int i = 0; i < road.points.Length; i++)
                {
                    Point point = road.points[i];
                    float y = SampleHeight(point.x, point.z) + 0.12f;
                    line.SetPosition(i, new Vector3(ToUnityX(point.x), y, point.z));
                }
            }
        }

        private void BuildWorldMarkers()
        {
            Transform parent = NewGroup("WorldMarkers");

            if (_data.camps != null)
            {
                Material campMaterial = GetMaterial("camp", new Color(0.78f, 0.17f, 0.12f, 1f));
                for (int i = 0; i < _data.camps.Length; i++)
                {
                    Camp camp = _data.camps[i];
                    CreateMarker(
                        parent,
                        "Camp_" + (string.IsNullOrEmpty(camp.mobId) ? i.ToString() : camp.mobId),
                        PrimitiveType.Sphere,
                        camp.x,
                        camp.z,
                        Mathf.Clamp(0.45f + camp.count * 0.06f, 0.45f, 1.2f),
                        campMaterial);
                }
            }

            if (_data.gatherNodes != null)
            {
                Material ore = GetMaterial("ore", new Color(0.32f, 0.48f, 0.64f, 1f));
                Material wood = GetMaterial("wood", new Color(0.31f, 0.20f, 0.11f, 1f));
                Material herb = GetMaterial("herb", new Color(0.16f, 0.56f, 0.24f, 1f));

                for (int i = 0; i < _data.gatherNodes.Length; i++)
                {
                    GatherNode node = _data.gatherNodes[i];
                    Material material = node.type == "ore" ? ore : node.type == "wood" ? wood : herb;
                    CreateMarker(
                        parent,
                        "Gather_" + node.id,
                        PrimitiveType.Cube,
                        node.x,
                        node.z,
                        0.30f + Mathf.Clamp(node.tier, 1, 6) * 0.06f,
                        material);
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
            Vector3 spawn = new Vector3(ToUnityX(sourceX), SampleHeight(sourceX, sourceZ) + 1.1f, sourceZ);

            CharacterController controller = motor.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled) controller.enabled = false;
            motor.transform.position = spawn;
            motor.transform.rotation = Quaternion.identity;
            if (wasEnabled) controller.enabled = true;

            HighflyWorldSafety safety = motor.GetComponent<HighflyWorldSafety>();
            if (safety != null)
            {
                safety.Configure(spawn, -80f, true, true);
                safety.SetRespawnPosition(spawn);
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.farClipPlane = 900f;
                camera.nearClipPlane = 0.08f;
            }
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
                return new Color(0.24f, 0.42f, 0.25f, 1f);

            string value = id.ToLowerInvariant();
            if (value.Contains("frost")) return new Color(0.58f, 0.68f, 0.72f, 1f);
            if (value.Contains("marsh") || value.Contains("willow")) return new Color(0.19f, 0.34f, 0.24f, 1f);
            if (value.Contains("drake")) return new Color(0.36f, 0.25f, 0.20f, 1f);
            if (value.Contains("night") || value.Contains("wraith")) return new Color(0.18f, 0.22f, 0.28f, 1f);
            if (value.Contains("palm") || value.Contains("farshore")) return new Color(0.32f, 0.50f, 0.28f, 1f);
            if (value.Contains("gale")) return new Color(0.34f, 0.46f, 0.34f, 1f);
            if (value.Contains("amber")) return new Color(0.48f, 0.38f, 0.22f, 1f);
            return new Color(0.26f, 0.46f, 0.27f, 1f);
        }
    }
}
