#if UNITY_EDITOR
// HIGHFLY ORIGIN WORLD GENESIS G1 — real RG Poly city extraction.
// Operates ONLY on the licensed stock scene; imports no Unity game runtime into Three.js.
using System;
using System.Collections.Generic;
using System.IO;
using GLTFast;
using GLTFast.Export;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Highfly.Editor
{
    public static class HighflyG01RgPolyGlbExporter
    {
        private const string SourceScene =
            "Assets/Stylized Medieval Kingdom URP/Scenes/Demo Stylized Medieval.unity";
        private const string ExportDir = "build/genesis";
        private const string ExportGlb = ExportDir + "/rgpoly-city01.glb";

        [Serializable]
        private class AnchorRecord
        {
            public string id;
            public string sourceObject;
            public Vector3 unityPosition;
            public Quaternion unityRotation;
        }
        [Serializable]
        private class ColliderRecord
        {
            public string hierarchy;
            public string type;
            public Vector3 unityWorldBoundsCenter;
            public Vector3 unityWorldBoundsSize;
        }
        [Serializable]
        private class Manifest
        {
            public string id = "highfly.capital.rgpoly.g01";
            public string provenance = "RG Poly [Free] Stylized Medieval Village - Unity URP (CC0)";
            public string scene = SourceScene;
            public string geometryFile = "rgpoly-city01.glb";
            public string coordinateSystem = "Unity world LH; convert semantic positions to glTF/Three RH alongside scene";
            public string collisionRepresentation = "Unity collider WORLD AABBs for auditing only; triangle collision not yet enabled";
            public Vector3 unityHunterSpawn = new Vector3(0f, 13.35f, -42f);
            public int rootCount;
            public int renderMeshCount;
            public int terrainCount;
            public int convertedTerrainCount;
            public int terrainMeshResolution;
            public int colliderCount;
            public long glbBytes;
            public AnchorRecord[] anchors;
            public ColliderRecord[] colliders;
        }

        // Invoked by game-ci/unity-builder on the dedicated NEXUS source/export branch.
        public static void ExportG01RgPolyCity()
        {
            if (!File.Exists(SourceScene))
                throw new FileNotFoundException("G1: official RG Poly source scene is not extracted", SourceScene);

            Directory.CreateDirectory(ExportDir);
            var scene = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            if (roots.Length < 10)
                throw new InvalidOperationException("G1: unexpectedly few RG Poly scene roots");

            var manifest = new Manifest { rootCount = roots.Length };
            var colliders = new List<ColliderRecord>();
            int meshCount = 0;
            int terrainCount = 0;
            int terrainConverted = 0;
            const int terrainResolution = 257; // 256 x 256 triangles grid, acceptable first Android LOD.
            foreach (var root in roots)
            {
                if (!root.activeInHierarchy) continue;
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (renderer != null && renderer.enabled &&
                        renderer.GetComponent<MeshFilter>() != null &&
                        renderer.GetComponent<MeshFilter>().sharedMesh != null)
                        meshCount++;
                }
                foreach (var terrain in root.GetComponentsInChildren<Terrain>(true))
                {
                    if (terrain == null || !terrain.enabled) continue;
                    terrainCount++;
                    // glTFast exports MeshRenderers but NOT Unity's Terrain component.
                    // Convert the source heightmap to actual vertices BEFORE GLB creation.
                    if (AddG01TerrainMesh(terrain, terrainResolution)) terrainConverted++;
                }
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    if (collider == null || !collider.enabled || collider.isTrigger) continue;
                    colliders.Add(new ColliderRecord
                    {
                        hierarchy = HierarchyPath(collider.transform),
                        type = collider.GetType().Name,
                        unityWorldBoundsCenter = collider.bounds.center,
                        unityWorldBoundsSize = collider.bounds.size
                    });
                }
            }
            if (meshCount < 100)
                throw new InvalidOperationException("G1: imported scene has fewer than 100 render meshes");
            if (terrainCount == 0 || terrainConverted != terrainCount)
                throw new InvalidOperationException(
                    "G1: original Unity Terrain was not successfully converted to GLB meshes.");

            manifest.renderMeshCount = meshCount;
            manifest.terrainCount = terrainCount;
            manifest.convertedTerrainCount = terrainConverted;
            manifest.terrainMeshResolution = terrainResolution;
            manifest.colliderCount = colliders.Count;
            manifest.colliders = colliders.ToArray();
            manifest.anchors = new[]
            {
                ExtractAnchor(scene, "anchor.capital.guild.door", "House 7", "Door_1 Variant (3)"),
                ExtractAnchor(scene, "anchor.capital.inn.door", "Inn", "Door_1 Variant"),
                ExtractAnchor(scene, "anchor.capital.forge.door", "Smithy", "Building_Addon_6 Variant (1)"),
                ExtractAnchor(scene, "service.market", "Market Stall 3", "Market_Table_2 Variant")
            };

            // Export only RG Poly visual hierarchy, NOT NEXUS motor, HUD or Unity scene scripts.
            var settings = new ExportSettings
            {
                Format = GltfFormat.Binary,
                FileConflictResolution = FileConflictResolution.Overwrite,
                ComponentMask = ~(ComponentType.Camera | ComponentType.Animation)
            };
            var gameObjectSettings = new GameObjectExportSettings
            {
                OnlyActiveInHierarchy = true,
                DisabledComponents = false
            };
            var exporter = new GameObjectExport(settings, gameObjectSettings);
            if (!exporter.AddScene(roots, "HIGHFLY_G01_RG_POLY_CITY"))
                throw new InvalidOperationException("G1: exporter failed to collect real scene meshes");

            // Critical: synchronous writes in Unity Editor batch mode avoid deadlock.
            bool success = exporter.SaveToFileAndDispose(ExportGlb, forceSync: true)
                .GetAwaiter().GetResult();
            if (!success || !File.Exists(ExportGlb))
                throw new InvalidOperationException("G1: glTFast GLB export failed");

            manifest.glbBytes = new FileInfo(ExportGlb).Length;
            if (manifest.glbBytes < 10240)
                throw new InvalidOperationException("G1: GLB is implausibly small");

            File.WriteAllText(ExportDir + "/rgpoly-city01.manifest.json",
                JsonUtility.ToJson(manifest, true));
            Debug.Log("HIGHFLY_GENESIS_G1_REAL_GLB_READY=1" +
                      " meshes=" + meshCount +
                      " terrain=" + terrainConverted + "/" + terrainCount +
                      " roots=" + roots.Length +
                      " colliders=" + colliders.Count +
                      " glbBytes=" + manifest.glbBytes);
        }


        /// <summary>
        /// Construct a real geometry surface from the authoritative Unity TerrainData
        /// height field. Leave the original Terrain object untouched. glTFast will
        /// see the generated child MeshRenderer when traversing scene roots.
        ///
        /// First-pass export is geometry+flat material; precise TerrainLayers,
        /// splatmap blending, grass/detail vegetation and physics remain separate
        /// requirements before Android world acceptance.
        /// </summary>
        private static bool AddG01TerrainMesh(Terrain terrain, int resolution)
        {
            TerrainData data = terrain.terrainData;
            if (data == null || data.heightmapResolution < 2)
                return false;
            if (resolution < 3 || resolution > 513)
                throw new ArgumentOutOfRangeException(nameof(resolution));
            Vector3 size = data.size;
            if (size.x <= 0 || size.z <= 0)
                throw new InvalidOperationException("G1: terrain extents invalid");

            var vertices = new Vector3[resolution * resolution];
            var uv = new Vector2[vertices.Length];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
            for (int z = 0; z < resolution; z++)
            {
                float t = (float)z / (resolution - 1);
                for (int x = 0; x < resolution; x++)
                {
                    float u = (float)x / (resolution - 1);
                    int idx = z * resolution + x;
                    float height = data.GetInterpolatedHeight(u, t);
                    vertices[idx] = new Vector3(u * size.x, height, t * size.z);
                    uv[idx] = new Vector2(u, t);
                }
            }
            int ptr = 0;
            for (int z = 0; z < resolution - 1; z++)
                for (int x = 0; x < resolution - 1; x++)
                {
                    int a = z * resolution + x;
                    int b = a + 1;
                    int c = a + resolution;
                    int d = c + 1;
                    triangles[ptr++] = a;
                    triangles[ptr++] = c;
                    triangles[ptr++] = b;
                    triangles[ptr++] = b;
                    triangles[ptr++] = c;
                    triangles[ptr++] = d;
                }
            var mesh = new Mesh();
            mesh.name = "HIGHFLY_RG_POLY_REAL_TERRAIN_HEIGHTMAP";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var visual = new GameObject("HIGHFLY_G1_TERRAIN_GLTF_VISUAL");
            visual.transform.SetParent(terrain.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            visual.AddComponent<MeshFilter>().sharedMesh = mesh;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("G1: no Unity terrain compatible shader");
            var material = new Material(shader);
            material.name = "HIGHFLY_G1_TERRAIN_TEMPORARY_TINT";
            var color = new Color(0.33f, 0.46f, 0.27f, 1f);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            visual.AddComponent<MeshRenderer>().sharedMaterial = material;

            Debug.Log("HIGHFLY_G1_TERRAIN_HEIGHTMAP_EXPORTED=1" +
                      " vertices=" + vertices.Length +
                      " triangles=" + triangles.Length / 3 +
                      " size=" + size + " origin=" + terrain.transform.position);
            return true;
        }

        private static string HierarchyPath(Transform transform)
        {
            var parts = new List<string>();
            for (var t = transform; t != null; t = t.parent)
                parts.Add(t.name);
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static AnchorRecord ExtractAnchor(Scene scene, string id, string rootName, string targetName)
        {
            Transform root = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == rootName) { root = go.transform; break; }
            }
            if (root == null)
                throw new InvalidOperationException("G1 missing audited root: " + rootName);
            Transform target = null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == targetName) { target = t; break; }
            if (target == null)
                throw new InvalidOperationException("G1 missing audited target: " + rootName + "/" + targetName);
            return new AnchorRecord
            {
                id = id,
                sourceObject = HierarchyPath(target),
                unityPosition = target.position,
                unityRotation = target.rotation
            };
        }
    }
}
#endif
