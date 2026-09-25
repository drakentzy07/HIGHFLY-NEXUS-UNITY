#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Highfly.World;

namespace Highfly.Editor
{
    /// <summary>
    /// W1 overlay for the audited RG Poly CITY01.
    /// It does not move, replace or remodel any RG Poly object. It only adds
    /// HIGHFLY semantic anchors and interaction triggers at verified locations.
    /// </summary>
    public static class HighflyRgPolyW1Layout
    {
        public const string BindingRootName = "HIGHFLY_W1_RGPOLY_BINDINGS";

        public static void BindExterior(Scene scene)
        {
            GameObject existing = FindRoot(scene, BindingRootName);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing);

            GameObject root = new GameObject(BindingRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            BindDoor(
                scene, root.transform,
                "House 7", "Door_1 Variant (3)",
                "interior.capital.guild", "anchor.capital.guild.door",
                "GREMIO", "ENTRAR");

            BindDoor(
                scene, root.transform,
                "Inn", "Door_1 Variant",
                "interior.capital.inn", "anchor.capital.inn.door",
                "POSADA / TABERNA", "ENTRAR");

            BindDoor(
                scene, root.transform,
                "Smithy", "Building_Addon_6 Variant (1)",
                "interior.capital.forge", "anchor.capital.forge.door",
                "FORJA", "ENTRAR");

            BindExteriorService(
                scene, root.transform,
                "Market Stall 3", "Market_Table_2 Variant",
                "service.market",
                "MERCADER DEL MERCADO", "COMERCIAR");

            Debug.Log(
                "HIGHFLY W1.1 RG POLY exterior bindings ready | " +
                "Guild=House 7 | Inn=Inn | Forge=Smithy | " +
                "Market=exterior service at Market Stall 3");
        }


        private static void BindExteriorService(
            Scene scene,
            Transform parent,
            string rootName,
            string childName,
            string serviceId,
            string displayName,
            string prompt)
        {
            GameObject sourceRoot = FindRoot(scene, rootName);
            if (sourceRoot == null)
                throw new InvalidOperationException(
                    "HIGHFLY W1.1 missing audited service root: " + rootName);

            Transform target = FindDescendant(sourceRoot.transform, childName);
            if (target == null)
                throw new InvalidOperationException(
                    "HIGHFLY W1.1 missing audited service target: " +
                    rootName + " / " + childName);

            GameObject marker = new GameObject(
                "W1_SERVICE_" + displayName.Replace(" ", "_"));

            marker.transform.SetParent(parent, false);
            marker.transform.position = target.position;
            marker.transform.rotation = target.rotation;

            BoxCollider trigger = marker.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.0f, 0f);
            trigger.size = new Vector3(3.5f, 2.3f, 3.5f);

            HighflyW1ServiceStation service =
                marker.AddComponent<HighflyW1ServiceStation>();

            service.Configure(serviceId, displayName, prompt);
        }

        private static void BindDoor(
            Scene scene,
            Transform parent,
            string rootName,
            string childName,
            string interiorId,
            string anchorId,
            string displayName,
            string prompt)
        {
            GameObject sourceRoot = FindRoot(scene, rootName);
            if (sourceRoot == null)
                throw new InvalidOperationException(
                    "HIGHFLY W1 RG POLY missing audited root: " + rootName);

            Transform target = FindDescendant(sourceRoot.transform, childName);
            if (target == null)
                throw new InvalidOperationException(
                    "HIGHFLY W1 RG POLY missing audited target: " +
                    rootName + " / " + childName);

            GameObject marker = new GameObject(
                "W1_DOOR_" + displayName.Replace(" ", "_").Replace("/", "_"));

            marker.transform.SetParent(parent, false);
            marker.transform.position = target.position;
            marker.transform.rotation = target.rotation;

            BoxCollider trigger = marker.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.05f, 0f);
            trigger.size = new Vector3(3.2f, 2.4f, 3.2f);

            HighflyWorldAnchor anchor = marker.AddComponent<HighflyWorldAnchor>();
            anchor.Configure(anchorId, displayName + " exterior", "exterior_door");

            HighflyWorldSceneDoor door = marker.AddComponent<HighflyWorldSceneDoor>();
            door.Configure(interiorId, anchorId, displayName, prompt);
        }

        public static GameObject FindRoot(Scene scene, string exactName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(
                    roots[i].name,
                    exactName,
                    StringComparison.Ordinal))
                    return roots[i];
            }

            return null;
        }

        public static Transform FindDescendant(Transform root, string exactName)
        {
            if (root == null)
                return null;

            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (string.Equals(
                    all[i].name,
                    exactName,
                    StringComparison.Ordinal))
                    return all[i];
            }

            return null;
        }
    }
}
#endif
