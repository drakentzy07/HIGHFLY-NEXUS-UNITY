using System.Collections;
using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// REUSE FIRST binder: converts the already approved World Final FREE capital
    /// into W1 functional buildings without rebuilding or replacing its art/layout.
    /// </summary>
    public sealed class HighflyW1CityBinder : MonoBehaviour
    {
        private bool _bound;

        private IEnumerator Start()
        {
            // HighflyWorldFinalRuntime constructs the capital in Start().
            // Wait until it exists rather than introducing execution-order coupling.
            GameObject capital = null;
            for (int i = 0; i < 300 && capital == null; i++)
            {
                capital = GameObject.Find("CAPITAL_HIGHFLY");
                if (capital == null)
                    yield return null;
            }

            if (capital == null)
            {
                Debug.LogError("HIGHFLY W1: CAPITAL_HIGHFLY was not created; city binder aborted.");
                yield break;
            }

            yield return null;
            BindCapital(capital.transform);
        }

        private void BindCapital(Transform capital)
        {
            if (_bound || capital == null)
                return;

            HighflyWorldRuntimeHost host = HighflyWorldRuntimeHost.Current;
            if (host == null || host.Registry == null || !host.Registry.CatalogLoaded)
            {
                Debug.LogError("HIGHFLY W1: registry/catalog not ready.");
                return;
            }

            int bound = 0;
            bound += BindBuilding(capital, "GUILD", "interior.capital.guild", "anchor.capital.guild.door", "GREMIO", "ENTRAR");
            bound += BindBuilding(capital, "TAVERN", "interior.capital.inn", "anchor.capital.inn.door", "POSADA / TABERNA", "ENTRAR");
            bound += BindBuilding(capital, "BLACKSMITH", "interior.capital.forge", "anchor.capital.forge.door", "FORJA", "ENTRAR");
            bound += BindBuilding(capital, "MARKET_HALL", "interior.capital.market", "anchor.capital.market.door", "MERCADO", "ENTRAR");

            _bound = bound == 4;

            Debug.Log(
                "HIGHFLY W1 CITY BINDER | functional buildings=" + bound + "/4" +
                " | legacy same-scene interiors bypassed | combat core untouched");
        }

        private static int BindBuilding(
            Transform capital,
            string objectName,
            string interiorId,
            string anchorId,
            string label,
            string actionPrompt)
        {
            Transform building = FindDeep(capital, objectName);
            if (building == null)
            {
                Debug.LogError("HIGHFLY W1: building not found: " + objectName);
                return 0;
            }

            HighflyWorldSceneDoor existing = building.GetComponentInChildren<HighflyWorldSceneDoor>(true);
            if (existing != null)
                return 1;

            // Disable only legacy same-scene doors under this W1 building, if any.
            HighflyBuildingDoor[] legacy = building.GetComponentsInChildren<HighflyBuildingDoor>(true);
            for (int i = 0; i < legacy.Length; i++)
                legacy[i].enabled = false;

            Bounds localBounds;
            if (!TryGetLocalRendererBounds(building, out localBounds))
                localBounds = new Bounds(Vector3.zero, new Vector3(8f, 5f, 8f));

            float frontZ = localBounds.min.z - 0.75f;
            float y = Mathf.Clamp(localBounds.min.y + 1.25f, 1.0f, 2.0f);

            GameObject trigger = new GameObject("W1_SCENE_DOOR_" + objectName);
            trigger.transform.SetParent(building, false);
            trigger.transform.localPosition = new Vector3(0f, y, frontZ);
            trigger.transform.localRotation = Quaternion.identity;

            BoxCollider collider = trigger.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(2.6f, 2.6f, 1.8f);

            HighflyWorldAnchor anchor = trigger.AddComponent<HighflyWorldAnchor>();
            anchor.Configure(anchorId, label + " exterior", "exterior_door");

            HighflyWorldSceneDoor door = trigger.AddComponent<HighflyWorldSceneDoor>();
            door.Configure(interiorId, anchorId, label, actionPrompt);

            return 1;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static bool TryGetLocalRendererBounds(Transform root, out Bounds localBounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                localBounds = new Bounds();
                return false;
            }

            bool initialized = false;
            localBounds = new Bounds();

            for (int i = 0; i < renderers.Length; i++)
            {
                Bounds world = renderers[i].bounds;
                Vector3 min = world.min;
                Vector3 max = world.max;

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            Vector3 corner = new Vector3(
                                x == 0 ? min.x : max.x,
                                y == 0 ? min.y : max.y,
                                z == 0 ? min.z : max.z);
                            Vector3 local = root.InverseTransformPoint(corner);

                            if (!initialized)
                            {
                                localBounds = new Bounds(local, Vector3.zero);
                                initialized = true;
                            }
                            else
                            {
                                localBounds.Encapsulate(local);
                            }
                        }
                    }
                }
            }

            return initialized;
        }
    }
}
