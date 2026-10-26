using System.Collections;
using UnityEngine;

namespace Highfly.Combat
{
    /// <summary>
    /// Native Unity particle/mesh VFX; no Three.js, remote textures or paid media.
    /// Scales particle counts for landscape Android/WebGL.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyGuinevereVfx : MonoBehaviour
    {
        public enum Quality { Fluido, Equilibrado, Ultra }
        [SerializeField] private Quality quality = Quality.Equilibrado;
        [SerializeField] private Color violet = new Color(.62f, .40f, 1f, 1f);
        [SerializeField] private Color rose = new Color(.96f, .57f, 1f, 1f);
        [SerializeField] private Color core = new Color(1f, .90f, .98f, 1f);

        private HighflyGuineverePremiumVfx _premium;
        private Material _particles;
        private Material _unlit;

        private void Awake()
        {
            // Visual-only R06 add-on. No change to hit/movement/cooldown authority.
            _premium = GetComponent<HighflyGuineverePremiumVfx>();
            if (_premium == null) _premium = gameObject.AddComponent<HighflyGuineverePremiumVfx>();
            _premium.SetQuality((int)quality);
            Shader p = Shader.Find("Particles/Standard Unlit");
            if (p == null) p = Shader.Find("Legacy Shaders/Particles/Additive");
            if (p == null) p = Shader.Find("Sprites/Default");
            if (p != null) _particles = new Material(p);
            Shader u = Shader.Find("Unlit/Color");
            if (u == null) u = Shader.Find("Sprites/Default");
            if (u != null) _unlit = new Material(u);
        }

        public void SetQuality(int value)
        {
            quality = (Quality)Mathf.Clamp(value, 0, 2);
            if (_premium != null) _premium.SetQuality((int)quality);
        }

        private int Count(int source)
        {
            float scale = quality == Quality.Fluido ? .42f : quality == Quality.Ultra ? 1f : .72f;
            return Mathf.Min(96, Mathf.Max(4, Mathf.RoundToInt(source * scale)));
        }

        public Transform CreatePrisma(Vector3 position)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Guinevere_Prism_Bolt";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * .31f;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false; // Destroy() completes end-of-frame; avoid a transient projectile self-hit.
                Destroy(collider);
            }
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && _unlit != null)
            {
                renderer.sharedMaterial = _unlit;
                // MaterialPropertyBlock does not instantiate a new material every shot.
                MaterialPropertyBlock props = new MaterialPropertyBlock();
                props.SetColor("_Color", core);
                renderer.SetPropertyBlock(props);
            }
            TrailRenderer trail = go.AddComponent<TrailRenderer>();
            trail.time = .20f;
            trail.startWidth = .24f;
            trail.endWidth = .025f;
            trail.minVertexDistance = .07f;
            trail.startColor = rose;
            trail.endColor = new Color(violet.r, violet.g, violet.b, 0f);
            if (_particles != null) trail.sharedMaterial = _particles;
            Burst(position, rose, 11, .45f, .13f, .26f);
            if (_premium != null) _premium.PrismaSpawn(go.transform);
            return go.transform;
        }

        public void Launch(Vector3 at)
        {
            Burst(at + Vector3.up, violet, 25, 3f, .16f, .48f);
            if (_premium != null) _premium.Launch(at);
        }

        public void Impact(Vector3 at, float radius)
        {
            Burst(at + Vector3.up * .8f, core, 38, 4f, .20f, .55f);
            Ring(at, radius, rose, .66f);
            if (_premium != null) _premium.Impact(at, radius);
        }

        public void Mark(Vector3 at, int stacks)
        {
            Burst(at + Vector3.up * 1.9f, stacks >= 3 ? core : rose, 9 + stacks * 3, 1.2f, .11f, .55f);
            if (_premium != null) _premium.Mark(at, stacks);
        }

        public void Lift(Vector3 at)
        {
            Burst(at + Vector3.up * .45f, violet, 18, 2.8f, .13f, .44f);
            Ring(at, 1f, violet, .38f);
            if (_premium != null) _premium.Lift(at);
        }

        public void Basic(Vector3 at, int combo)
        {
            Burst(at + Vector3.up * .9f, combo >= 4 ? core : violet, 11 + combo * 4, 2.8f, .12f, .34f);
            if (combo >= 4) Ring(at, 1.25f, rose, .35f);
            if (_premium != null) _premium.Basic(at, combo);
        }

        public void Blink(Vector3 origin, Vector3 destination)
        {
            Burst(origin + Vector3.up, violet, 22, 2f, .14f, .43f);
            Burst(destination + Vector3.up, rose, 25, 2.6f, .15f, .52f);
            GameObject line = new GameObject("Guinevere_Blink_Trail");
            LineRenderer r = line.AddComponent<LineRenderer>();
            r.positionCount = 2;
            r.SetPosition(0, origin + Vector3.up);
            r.SetPosition(1, destination + Vector3.up);
            r.startWidth = .20f;
            r.endWidth = .08f;
            r.startColor = rose;
            r.endColor = violet;
            if (_particles != null) r.sharedMaterial = _particles;
            Destroy(line, .22f);
            if (_premium != null) _premium.Blink(origin, destination);
        }

        public void Echo(Vector3 origin)
        {
            Ring(origin, 2.05f, rose, 1.5f);
            Burst(origin + Vector3.up, violet, 15, 1f, .15f, 1.5f);
            if (_premium != null) _premium.Echo(origin);
        }

        public void Tempestad(Vector3 origin, float radius, int index)
        {
            // Each of the 11 deterministic damage pulses gets its own clear visual cue.
            float theta = index * 2.39996323f;
            Vector3 offset = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * (1.4f + (index % 3) * .38f);
            Burst(origin + offset + Vector3.up * 1.3f, index % 2 == 0 ? rose : violet, 13, 2.9f, .14f, .30f);
            if (index == 1 || index == 6 || index == 11)
                Ring(origin, radius, index == 11 ? core : violet, .48f);
            if (_premium != null) _premium.Tempestad(origin, radius, index);
        }

        private void Burst(Vector3 at, Color color, int count, float speed, float size, float lifetime)
        {
            GameObject go = new GameObject("Guinevere_VFX_Burst");
            go.transform.position = at;
            ParticleSystem particles = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = Mathf.Max(.15f, lifetime);
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Count(count)) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .32f;
            ParticleSystemRenderer render = particles.GetComponent<ParticleSystemRenderer>();
            if (_particles != null) render.sharedMaterial = _particles;
            particles.Play();
            Destroy(go, lifetime + .4f);
        }

        private void Ring(Vector3 origin, float radius, Color color, float life)
        {
            GameObject go = new GameObject("Guinevere_VFX_Ring");
            go.transform.position = origin + Vector3.up * .10f;
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            int segments = quality == Quality.Fluido ? 28 : 48;
            line.positionCount = segments;
            line.startWidth = .10f;
            line.endWidth = .10f;
            line.startColor = color;
            line.endColor = color;
            if (_particles != null) line.sharedMaterial = _particles;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
            StartCoroutine(FadeRing(go, line, life));
        }

        private IEnumerator FadeRing(GameObject go, LineRenderer line, float life)
        {
            float t = 0f;
            while (t < life && go != null)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / Mathf.Max(.01f, life));
                line.startWidth = line.endWidth = Mathf.Lerp(.12f, .015f, u);
                go.transform.localScale = Vector3.one * Mathf.Lerp(.70f, 1.25f, u);
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        private void OnDestroy()
        {
            if (_particles != null) Destroy(_particles);
            if (_unlit != null) Destroy(_unlit);
        }
    }
}
