using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// Stable semantic anchor registered by ID. The transform remains the source
    /// of position/facing; no world system writes player locomotion continuously.
    /// </summary>
    public sealed class HighflyWorldAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId;
        [SerializeField] private string displayName;
        [SerializeField] private string anchorKind = "generic";

        public string AnchorId { get { return anchorId; } }
        public string DisplayName { get { return displayName; } }
        public string AnchorKind { get { return anchorKind; } }

        public void Configure(string id, string label, string kind)
        {
            anchorId = id;
            displayName = label;
            anchorKind = kind;
            Register();
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            HighflyWorldRuntimeHost host = HighflyWorldRuntimeHost.Current;
            if (host != null && host.Registry != null)
                host.Registry.UnregisterAnchor(this);
        }

        private void Register()
        {
            HighflyWorldRuntimeHost host = HighflyWorldRuntimeHost.Current;
            if (host != null && host.Registry != null)
                host.Registry.RegisterAnchor(this);
        }
    }
}
