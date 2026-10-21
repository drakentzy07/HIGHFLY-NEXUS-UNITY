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
                    if (terrain != null && terrain.enabled) terrainCount++;
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
            if (terrainCount != 0)
                throw new InvalidOperationException(
                    "G1: Unity Terrain detected; glTFast omits terrain mesh. Export separately before GREEN.");

            manifest.renderMeshCount = meshCount;
            manifest.terrainCount = terrainCount;
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
                      " roots=" + roots.Length +
                      " colliders=" + colliders.Count +
                      " glbBytes=" + manifest.glbBytes);
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
