using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Highfly.World
{
    /// <summary>
    /// Living-world Gate director. Uses authored anchors, so a portal never appears inside a house
    /// or in the middle of a critical road. Unresolved Gates can breach and enable invasion enemies.
    /// </summary>
    public sealed class HighflyDynamicGateDirector : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private HighflyGateAnchor[] anchors;
        [SerializeField] private Vector3 dungeonDestination;
        [SerializeField] private Vector3 dungeonFacing = Vector3.forward;
        [SerializeField] private Text alertText;

        [Header("Timing")]
        [SerializeField] private float firstGateDelay = 18f;
        [SerializeField] private float respawnDelayMin = 30f;
        [SerializeField] private float respawnDelayMax = 58f;
        [SerializeField] private float gateLifetime = 55f;

        [Header("Placement")]
        [SerializeField] private float minPlayerDistance = 18f;
        [SerializeField] private float maxPlayerDistance = 135f;

        private HighflyDynamicGate _activeGate;
        private float _nextGateAt;
        private float _alertUntil;
        private int _lastAnchor = -1;

        public void Configure(
            Transform playerTransform,
            HighflyGateAnchor[] worldAnchors,
            Vector3 targetDungeon,
            Vector3 targetFacing,
            Text worldAlert)
        {
            player = playerTransform;
            anchors = worldAnchors;
            dungeonDestination = targetDungeon;
            dungeonFacing = targetFacing.sqrMagnitude > 0.001f ? targetFacing.normalized : Vector3.forward;
            alertText = worldAlert;
        }

        private void Start()
        {
            _nextGateAt = Time.time + Mathf.Max(3f, firstGateDelay);
            SetAlert("SISTEMA • Mundo estable", 3f);
        }

        private void Update()
        {
            if (alertText != null && Time.time >= _alertUntil && _activeGate == null)
                alertText.text = string.Empty;

            if (_activeGate != null || Time.time < _nextGateAt)
                return;

            SpawnGate();
        }

        private void SpawnGate()
        {
            HighflyGateAnchor anchor = PickAnchor();
            if (anchor == null)
            {
                ScheduleNext(10f, 18f);
                return;
            }

            string rank = anchor.PickRank();
            GameObject root = new GameObject("DYNAMIC_GATE_" + rank + "_" + anchor.Biome);
            root.transform.position = anchor.transform.position;
            root.transform.rotation = Quaternion.identity;

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1.8f;
            collider.center = Vector3.up * 1.35f;

            Light light = CreateGateVisual(root.transform, rank);

            HighflyDynamicGate gate = root.AddComponent<HighflyDynamicGate>();
            gate.Configure(
                this,
                dungeonDestination,
                dungeonFacing,
                rank,
                gateLifetime,
                anchor.BreachRoot,
                light);

            _activeGate = gate;
            SetAlert(
                "⚠ ANOMALÍA DE MANÁ • GATE RANGO " + rank +
                " • " + anchor.Biome +
                " • cerralo antes del BREAK",
                9f);
        }

        private HighflyGateAnchor PickAnchor()
        {
            if (anchors == null || anchors.Length == 0)
                return null;

            List<int> valid = new List<int>();
            for (int i = 0; i < anchors.Length; i++)
            {
                HighflyGateAnchor anchor = anchors[i];
                if (anchor == null || i == _lastAnchor)
                    continue;

                if (player != null)
                {
                    float distance = Vector3.Distance(player.position, anchor.transform.position);
                    if (distance < minPlayerDistance || distance > maxPlayerDistance)
                        continue;
                }

                // Anchors are authored on clear pads. This check only rejects newly blocked pads.
                Collider[] blockers = Physics.OverlapSphere(
                    anchor.transform.position + Vector3.up * 1.0f,
                    1.4f,
                    ~0,
                    QueryTriggerInteraction.Ignore);

                bool blocked = false;
                for (int b = 0; b < blockers.Length; b++)
                {
                    Collider hit = blockers[b];
                    if (hit == null || hit.transform.IsChildOf(anchor.transform))
                        continue;

                    if (hit.CompareTag("Player"))
                        continue;

                    Bounds hb = hit.bounds;
                    if (hb.size.y > 1.6f && hb.center.y > anchor.transform.position.y + 0.2f)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                    valid.Add(i);
            }

            if (valid.Count == 0)
            {
                for (int i = 0; i < anchors.Length; i++)
                    if (anchors[i] != null && i != _lastAnchor)
                        valid.Add(i);
            }

            if (valid.Count == 0)
                return null;

            int selected = valid[Random.Range(0, valid.Count)];
            _lastAnchor = selected;
            return anchors[selected];
        }

        private static Light CreateGateVisual(Transform root, string rank)
        {
            Color core = RankColor(rank);
            Material material = CreateGateMaterial(core);

            const int pieces = 14;
            for (int i = 0; i < pieces; i++)
            {
                float angle = i * Mathf.PI * 2f / pieces;
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "ManaShard_" + i;
                shard.transform.SetParent(root, false);
                shard.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 1.25f,
                    1.55f + Mathf.Sin(angle * 2f) * 0.08f,
                    Mathf.Sin(angle) * 0.36f);
                shard.transform.localRotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
                shard.transform.localScale = new Vector3(0.13f, 0.46f, 0.10f);

                Renderer renderer = shard.GetComponent<Renderer>();
                if (renderer != null && material != null)
                    renderer.sharedMaterial = material;

                Collider col = shard.GetComponent<Collider>();
                if (col != null)
                    UnityEngine.Object.Destroy(col);
            }

            GameObject coreGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coreGo.name = "GateCore";
            coreGo.transform.SetParent(root, false);
            coreGo.transform.localPosition = new Vector3(0f, 1.55f, 0.08f);
            coreGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            coreGo.transform.localScale = new Vector3(1.05f, 0.03f, 1.45f);
            Renderer coreRenderer = coreGo.GetComponent<Renderer>();
            if (coreRenderer != null && material != null)
                coreRenderer.sharedMaterial = material;
            Collider coreCollider = coreGo.GetComponent<Collider>();
            if (coreCollider != null)
                UnityEngine.Object.Destroy(coreCollider);

            GameObject lightGo = new GameObject("GateLight");
            lightGo.transform.SetParent(root, false);
            lightGo.transform.localPosition = new Vector3(0f, 1.55f, -0.35f);
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = core;
            light.range = 9f;
            light.intensity = 3.2f;
            light.shadows = LightShadows.None;

            GameObject particlesGo = new GameObject("ManaParticles");
            particlesGo.transform.SetParent(root, false);
            particlesGo.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            ParticleSystem particles = particlesGo.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 0.7f;
            main.startSize = 0.08f;
            main.startColor = core;
            main.maxParticles = 70;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 24f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.25f;

            return light;
        }

        private static Material CreateGateMaterial(Color color)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null)
                return null;

            Material material = new Material(shader);
            material.name = "DynamicGateMaterial";
            material.color = color;

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.1f);
            }

            return material;
        }

        private static Color RankColor(string rank)
        {
            switch ((rank ?? "F").ToUpperInvariant())
            {
                case "D": return new Color(0.25f, 0.55f, 1f, 1f);
                case "C": return new Color(0.55f, 0.28f, 1f, 1f);
                case "B": return new Color(0.95f, 0.28f, 0.88f, 1f);
                case "A": return new Color(1f, 0.24f, 0.38f, 1f);
                case "S": return new Color(1f, 0.10f, 0.16f, 1f);
                default: return new Color(0.15f, 0.88f, 1f, 1f);
            }
        }

        public void OnGateEntered(HighflyDynamicGate gate)
        {
            if (_activeGate == gate)
                _activeGate = null;

            SetAlert("GATE INGRESADO • cerrá la Dungeon para estabilizar la zona", 7f);
            ScheduleNext(respawnDelayMin, respawnDelayMax);
        }

        public void OnGateBreached(HighflyDynamicGate gate)
        {
            if (_activeGate == gate)
                _activeGate = null;

            SetAlert("⚠ BREAK • monstruos invadiendo el mundo • misión urgente", 12f);
            ScheduleNext(respawnDelayMin + 12f, respawnDelayMax + 18f);
        }

        private void ScheduleNext(float minDelay, float maxDelay)
        {
            _nextGateAt = Time.time + Random.Range(
                Mathf.Max(5f, minDelay),
                Mathf.Max(minDelay + 1f, maxDelay));
        }

        private void SetAlert(string message, float duration)
        {
            if (alertText == null)
                return;

            alertText.text = message;
            _alertUntil = Time.time + Mathf.Max(1f, duration);
        }
    }
}
