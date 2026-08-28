using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// One temporary world Gate. It can be entered before expiry or breach into the overworld.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class HighflyDynamicGate : HighflyInteractable
    {
        [SerializeField] private Vector3 destination;
        [SerializeField] private Vector3 facing = Vector3.forward;
        [SerializeField] private float lifetime = 55f;
        [SerializeField] private string rank = "F";
        [SerializeField] private GameObject breachRoot;
        [SerializeField] private Light gateLight;

        private HighflyDynamicGateDirector _director;
        private float _expiresAt;
        private bool _resolved;

        public string Rank => rank;

        public void Configure(
            HighflyDynamicGateDirector director,
            Vector3 targetDestination,
            Vector3 targetFacing,
            string gateRank,
            float duration,
            GameObject breachEnemies,
            Light visualLight)
        {
            _director = director;
            destination = targetDestination;
            facing = targetFacing.sqrMagnitude > 0.001f ? targetFacing.normalized : Vector3.forward;
            rank = string.IsNullOrEmpty(gateRank) ? "F" : gateRank;
            lifetime = Mathf.Max(12f, duration);
            breachRoot = breachEnemies;
            gateLight = visualLight;

            displayName = "GATE RANGO " + rank;
            prompt = "ENTRAR";
            _expiresAt = Time.time + lifetime;

            if (breachRoot != null)
                breachRoot.SetActive(false);
        }

        private void Start()
        {
            if (_expiresAt <= 0f)
                _expiresAt = Time.time + Mathf.Max(12f, lifetime);
        }

        private void Update()
        {
            if (_resolved || Time.time < _expiresAt)
                return;

            Breach();
        }

        public override void Interact(HighflyInteractionController controller)
        {
            if (_resolved || controller == null)
                return;

            _resolved = true;
            if (_director != null)
                _director.OnGateEntered(this);

            controller.Teleport(destination, facing, false);
            Destroy(gameObject);
        }

        private void Breach()
        {
            if (_resolved)
                return;

            _resolved = true;
            prompt = "BRECHA";
            displayName = "GATE RANGO " + rank + " — BREAK";

            if (gateLight != null)
            {
                gateLight.color = new Color(1f, 0.08f, 0.16f, 1f);
                gateLight.intensity = 4.5f;
            }

            if (breachRoot != null)
                breachRoot.SetActive(true);

            Collider gateCollider = GetComponent<Collider>();
            if (gateCollider != null)
                gateCollider.enabled = false;

            if (_director != null)
                _director.OnGateBreached(this);

            Destroy(gameObject, 7f);
        }
    }
}
