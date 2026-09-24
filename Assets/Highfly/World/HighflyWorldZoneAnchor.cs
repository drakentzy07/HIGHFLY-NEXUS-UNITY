using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// Stable semantic anchor for future isekai systems.
    /// Keeps world composition decoupled from gathering/farming/fishing/property logic.
    /// </summary>
    public sealed class HighflyWorldZoneAnchor : MonoBehaviour
    {
        [SerializeField] private string zoneId;
        [SerializeField] private string displayName;
        [SerializeField] private float radius;

        public string ZoneId { get { return zoneId; } }
        public string DisplayName { get { return displayName; } }
        public float Radius { get { return radius; } }

        public void Configure(string id, string label, float zoneRadius)
        {
            zoneId = id;
            displayName = label;
            radius = zoneRadius;
        }
    }
}
