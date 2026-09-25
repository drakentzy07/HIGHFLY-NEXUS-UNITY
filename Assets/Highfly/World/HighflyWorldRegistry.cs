using System;
using System.Collections.Generic;
using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// Single W0 authority for world definitions and stable anchors.
    /// It owns topology data only. It never owns locomotion, camera, combat or damage.
    /// </summary>
    public sealed class HighflyWorldRegistry : MonoBehaviour
    {
        private const string CatalogResourcePath = "World/W0W1Catalog";

        private readonly Dictionary<string, HighflyWorldSceneDefinition> _scenes =
            new Dictionary<string, HighflyWorldSceneDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HighflyZoneDefinition> _zones =
            new Dictionary<string, HighflyZoneDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HighflyInteriorDefinition> _interiors =
            new Dictionary<string, HighflyInteriorDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HighflyPortalDefinition> _portals =
            new Dictionary<string, HighflyPortalDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HighflyWorldAnchor> _anchors =
            new Dictionary<string, HighflyWorldAnchor>(StringComparer.OrdinalIgnoreCase);

        public event Action<HighflyWorldTransitionEvent> TransitionStarted;
        public event Action<HighflyWorldTransitionEvent> TransitionCompleted;

        public int SchemaVersion { get; private set; }
        public bool CatalogLoaded { get; private set; }

        private void Awake()
        {
            LoadCatalog();
        }

        public void LoadCatalog()
        {
            _scenes.Clear();
            _zones.Clear();
            _interiors.Clear();
            _portals.Clear();

            TextAsset asset = Resources.Load<TextAsset>(CatalogResourcePath);
            if (asset == null)
            {
                Debug.LogError("HIGHFLY W0: Resources/" + CatalogResourcePath + ".json not found.");
                CatalogLoaded = false;
                return;
            }

            HighflyWorldCatalogData catalog = JsonUtility.FromJson<HighflyWorldCatalogData>(asset.text);
            if (catalog == null)
            {
                Debug.LogError("HIGHFLY W0: invalid world catalog JSON.");
                CatalogLoaded = false;
                return;
            }

            SchemaVersion = catalog.schemaVersion;
            RegisterCatalog(catalog);
            CatalogLoaded = true;

            Debug.Log(
                "HIGHFLY W0 REGISTRY | schema=" + SchemaVersion +
                " scenes=" + _scenes.Count +
                " zones=" + _zones.Count +
                " interiors=" + _interiors.Count +
                " portals=" + _portals.Count);
        }

        private void RegisterCatalog(HighflyWorldCatalogData catalog)
        {
            if (catalog.scenes != null)
            {
                for (int i = 0; i < catalog.scenes.Length; i++)
                    Register(_scenes, catalog.scenes[i] != null ? catalog.scenes[i].id : null, catalog.scenes[i], "scene");
            }

            if (catalog.zones != null)
            {
                for (int i = 0; i < catalog.zones.Length; i++)
                    Register(_zones, catalog.zones[i] != null ? catalog.zones[i].id : null, catalog.zones[i], "zone");
            }

            if (catalog.interiors != null)
            {
                for (int i = 0; i < catalog.interiors.Length; i++)
                    Register(_interiors, catalog.interiors[i] != null ? catalog.interiors[i].id : null, catalog.interiors[i], "interior");
            }

            if (catalog.portals != null)
            {
                for (int i = 0; i < catalog.portals.Length; i++)
                    Register(_portals, catalog.portals[i] != null ? catalog.portals[i].id : null, catalog.portals[i], "portal");
            }
        }

        private static void Register<T>(Dictionary<string, T> map, string id, T value, string kind)
        {
            if (string.IsNullOrWhiteSpace(id) || value == null)
            {
                Debug.LogWarning("HIGHFLY W0: ignored " + kind + " with empty id.");
                return;
            }

            if (map.ContainsKey(id))
                Debug.LogWarning("HIGHFLY W0: duplicate " + kind + " id " + id + " replaced by latest catalog entry.");

            map[id] = value;
        }

        public bool TryGetScene(string id, out HighflyWorldSceneDefinition value)
        {
            return _scenes.TryGetValue(id ?? "", out value);
        }

        public bool TryGetZone(string id, out HighflyZoneDefinition value)
        {
            return _zones.TryGetValue(id ?? "", out value);
        }

        public bool TryGetInterior(string id, out HighflyInteriorDefinition value)
        {
            return _interiors.TryGetValue(id ?? "", out value);
        }

        public bool TryGetPortal(string id, out HighflyPortalDefinition value)
        {
            return _portals.TryGetValue(id ?? "", out value);
        }

        public void RegisterAnchor(HighflyWorldAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
                return;

            _anchors[anchor.AnchorId] = anchor;
        }

        public void UnregisterAnchor(HighflyWorldAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
                return;

            HighflyWorldAnchor current;
            if (_anchors.TryGetValue(anchor.AnchorId, out current) && current == anchor)
                _anchors.Remove(anchor.AnchorId);
        }

        public bool TryGetAnchor(string id, out HighflyWorldAnchor anchor)
        {
            return _anchors.TryGetValue(id ?? "", out anchor) && anchor != null;
        }

        internal void PublishStarted(HighflyWorldTransitionEvent evt)
        {
            Action<HighflyWorldTransitionEvent> callback = TransitionStarted;
            if (callback != null)
                callback(evt);
        }

        internal void PublishCompleted(HighflyWorldTransitionEvent evt)
        {
            Action<HighflyWorldTransitionEvent> callback = TransitionCompleted;
            if (callback != null)
                callback(evt);
        }
    }
}
