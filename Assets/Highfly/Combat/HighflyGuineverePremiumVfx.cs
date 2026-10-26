using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Highfly.Combat
{
    /// <summary>
    /// R06: optional, visual-only Guinevere fidelity layer for Unity 6 / Quaternius.
    /// Combat, movement, airborne state, hit timing and cooldown remain authoritative
    /// in HighflyGuinevereCombat and FURY. No MLBB textures, models or code.
    /// No external assets. Safe to disable for A/B mobile performance testing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyGuineverePremiumVfx : MonoBehaviour
    {
        [SerializeField, Range(0, 2)] private int quality = 1;
        [SerializeField] private bool allowQuaterniusGhost = true;
        [SerializeField] private Color violet = new Color(.49f, .19f, 1f, .92f);
        [SerializeField] private Color rose = new Color(1f, .36f, .89f, .95f);
        [SerializeField] private Color white = new Color(1f, .90f, 1f, .98f);

        private readonly Queue<ParticleSystem> _free = new Queue<ParticleSystem>();
        private readonly Queue<LineRenderer> _freeRings = new Queue<LineRenderer>();
        private readonly List<GameObject> _activeGhosts = new List<GameObject>();
        private Material _particleMaterial;
        private Material _lineMaterial;
        private Material _ghostMaterial;
        private int _particleObjects;
        private int _ringObjects;
        private bool _ultimateActive;

        public void SetQuality(int level) { quality = Mathf.Clamp(level, 0, 2); }
        private float Density => quality == 0 ? .38f : quality == 1 ? .70f : 1f;
        private int MaxParticles => quality == 0 ? 16 : quality == 1 ? 26 : 40;
        private int MaxRings => quality == 0 ? 10 : quality == 1 ? 18 : 28;

        private void Awake()
        {
            // Built-in shaders, not external paid textures or imported materials.
            Shader particle = Shader.Find("Particles/Standard Unlit");
            if (particle == null) particle = Shader.Find("Legacy Shaders/Particles/Additive");
            if (particle == null) particle = Shader.Find("Sprites/Default");
            Shader line = Shader.Find("Sprites/Default");
            if (line == null) line = Shader.Find("Unlit/Color");
            Shader ghost = Shader.Find("Unlit/Transparent");
            if (ghost == null) ghost = Shader.Find("Sprites/Default");
            if (particle != null) _particleMaterial = new Material(particle);
            if (line != null) _lineMaterial = new Material(line);
            if (ghost != null)
            {
                _ghostMaterial = new Material(ghost);
                _ghostMaterial.color = rose;
            }
        }

        // Existing HIGHFLY calls are mirrored here, never substituted with damage logic.
        public void PrismaSpawn(Transform projectile)
        {
            if (projectile == null) return;
            // A second halo is parented to the moving orb: no projectile-motion ownership.
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shell.name = "R06_Prism_SecondaryHalo";
            Collider c = shell.GetComponent<Collider>();
            if (c != null) Destroy(c);
            shell.transform.SetParent(projectile, false);
            shell.transform.localPosition = Vector3.zero;
            shell.transform.localScale = Vector3.one * 1.44f;
            Renderer r = shell.GetComponent<Renderer>();
            if (r != null)
            {
                if (_ghostMaterial != null) r.sharedMaterial = _ghostMaterial;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", new Color(rose.r, rose.g, rose.b, .24f));
                r.SetPropertyBlock(block);
            }
            Spark(projectile.position, rose, 11, .65f, .10f, .24f);
        }

        public void Launch(Vector3 position)
        {
            Spark(position + Vector3.up * .35f, violet, 32, 3.7f, .16f, .42f);
            Ring(position, 1.35f, rose, .34f, .16f, 0f);
            Ring(position + Vector3.up * .65f, .80f, white, .28f, .10f, 65f);
        }

        public void Impact(Vector3 position, float radius)
        {
            Spark(position + Vector3.up * .5f, white, 35, 4.7f, .18f, .42f);
            Spark(position + Vector3.up * .7f, rose, 25, 2.4f, .15f, .52f);
            Ring(position, Mathf.Max(.6f, radius), white, .40f, .16f, 0f);
            Ring(position + Vector3.up * .20f, Mathf.Max(.35f, radius * .72f),
                violet, .53f, .11f, 0f);
        }

        public void Mark(Vector3 position, int stacks)
        {
            int count = Mathf.Clamp(stacks, 1, 3);
            Vector3 focus = position + Vector3.up * 1.85f;
            Spark(focus, count >= 3 ? white : violet, 6 + count * 5, .8f, .09f, .65f);
            Ring(focus, .30f + count * .13f, count >= 3 ? rose : violet,
                .56f, .07f, 90f);
        }

        public void Lift(Vector3 position)
        {
            Vector3 bottom = position + Vector3.up * .20f;
            Spark(bottom, violet, 25, 2.6f, .12f, .70f);
            Ring(bottom, 1.0f, rose, .53f, .12f, 0f);
            Ring(bottom + Vector3.up * 1.25f, .72f, white, .68f, .07f, 0f);
        }

        public void Basic(Vector3 at, int stage)
        {
            if (stage < 4) return; // Clarity/readability: only charged finisher.
            Spark(at + Vector3.up, rose, 24, 2.6f, .12f, .42f);
            Ring(at, 1.1f, white, .40f, .10f, 0f);
        }

        public void Blink(Vector3 from, Vector3 to)
        {
            // The motor already moves the player; this is only a readable transit cue.
            Vector3 axis = to - from;
            float length = axis.magnitude;
            if (length > .01f)
            {
                int hops = quality == 0 ? 3 : quality == 1 ? 5 : 7;
                for (int i = 1; i <= hops; i++)
                {
                    float t = (float)i / (hops + 1);
                    Vector3 point = Vector3.Lerp(from, to, t) + Vector3.up * 1.0f;
                    Spark(point, Color.Lerp(violet, rose, t), 5, .35f, .12f, .32f);
                }
            }
            Ring(from, .85f, violet, .37f, .10f, 0f);
            Ring(to, 1.10f, rose, .40f, .11f, 0f);
        }

        public void Echo(Vector3 at)
        {
            Spark(at + Vector3.up, violet, 20, .8f, .10f, 1.25f);
            Ring(at, 1.30f, violet, 1.42f, .08f, 0f);
            if (allowQuaterniusGhost && quality > 0 && _ghostMaterial != null)
                CreateQuaterniusSnapshot(at);
        }

        public void Tempestad(Vector3 at, float radius, int hit)
        {
            // Called by real U3 pulses (1..11); never schedules or applies damage.
            float angle = hit * 2.39996323f;
            float r = Mathf.Min(radius, 1.5f + (hit % 4) * .44f);
            Vector3 impact = at + new Vector3(Mathf.Cos(angle), 0f,
                Mathf.Sin(angle)) * r + Vector3.up * (1.15f + (hit % 3) * .35f);
            Spark(impact, hit % 2 == 0 ? violet : rose, 24, 3.1f, .13f, .39f);
            if (hit == 1 && !_ultimateActive) StartCoroutine(UltimateHalo());
            if (hit == 4 || hit == 8)
                Ring(at, radius, rose, .52f, .11f, 0f);
            if (hit == 11)
            {
                Spark(at + Vector3.up * 1.2f, white, 48, 5f, .19f, .59f);
                Ring(at, radius, white, .75f, .18f, 0f);
            }
        }

        private IEnumerator UltimateHalo()
        {
            _ultimateActive = true;
            float t = 0f;
            // Each ring follows the Hunter independently of whether FURY allows movement.
            while (t < 2.08f)
            {
                t += Time.deltaTime;
                if (Time.frameCount % (quality == 0 ? 9 : 5) == 0)
                {
                    float angle = t * 12f;
                    Vector3 center = transform.position;
                    Vector3 p = center + new Vector3(Mathf.Cos(angle), .7f + t * .18f,
                        Mathf.Sin(angle)) * 1.6f;
                    Spark(p, t < 1.65f ? violet : rose, 4, .7f, .10f, .25f);
                }
                yield return null;
            }
            _ultimateActive = false;
        }

        private void Spark(Vector3 at, Color color, int count, float speed,
            float size, float lifetime)
        {
            if (_particleMaterial == null || !isActiveAndEnabled) return;
            ParticleSystem p = AcquireParticles();
            if (p == null) return;
            GameObject go = p.gameObject;
            go.transform.position = at;
            go.SetActive(true);
            ParticleSystem.MainModule main = p.main;
            main.startSpeed = speed;
            main.startSize = size;
            main.startLifetime = lifetime;
            main.startColor = color;
            main.maxParticles = 100;
            p.Clear(true);
            p.Play(true);
            p.Emit(Mathf.Clamp(Mathf.RoundToInt(count * Density), 2, 80));
            StartCoroutine(ReleaseParticles(p, lifetime + .12f));
        }

        private ParticleSystem AcquireParticles()
        {
            if (_free.Count > 0) return _free.Dequeue();
            if (_particleObjects >= MaxParticles) return null;
            _particleObjects++;
            GameObject go = new GameObject("R06_Pooled_Particles");
            go.transform.SetParent(transform, false);
            ParticleSystem p = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = p.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.duration = 1.5f;
            ParticleSystem.EmissionModule emission = p.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = p.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = .22f;
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _particleMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return p;
        }

        private IEnumerator ReleaseParticles(ParticleSystem p, float after)
        {
            yield return new WaitForSeconds(after);
            if (p == null) yield break;
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            p.gameObject.SetActive(false);
            _free.Enqueue(p);
        }

        private void Ring(Vector3 center, float radius, Color color,
            float duration, float thickness, float tiltDegrees)
        {
            if (_lineMaterial == null || !isActiveAndEnabled) return;
            LineRenderer line = AcquireRing();
            if (line == null) return;
            line.gameObject.SetActive(true);
            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = thickness;
            StartCoroutine(RingRoutine(line, center, radius, color,
                Mathf.Max(.07f, duration), thickness, tiltDegrees));
        }

        private LineRenderer AcquireRing()
        {
            if (_freeRings.Count > 0) return _freeRings.Dequeue();
            if (_ringObjects >= MaxRings) return null;
            _ringObjects++;
            GameObject go = new GameObject("R06_Pooled_Ring");
            go.transform.SetParent(transform, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _lineMaterial;
            line.loop = true;
            line.useWorldSpace = true;
            line.textureMode = LineTextureMode.Stretch;
            line.numCornerVertices = 0;
            line.numCapVertices = 0;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = quality == 0 ? 20 : quality == 1 ? 36 : 54;
            return line;
        }

        private IEnumerator RingRoutine(LineRenderer line, Vector3 center,
            float radius, Color color, float duration, float thickness, float tilt)
        {
            float elapsed = 0f;
            int segments = line.positionCount;
            while (elapsed < duration && line != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float size = Mathf.Lerp(.62f, 1.26f, t) * radius;
                float fade = (1f - t) * color.a;
                Color now = new Color(color.r, color.g, color.b, fade);
                line.startColor = line.endColor = now;
                line.startWidth = line.endWidth = Mathf.Lerp(thickness, .005f, t);
                for (int i = 0; i < segments; i++)
                {
                    float a = Mathf.PI * 2f * i / segments + elapsed * 1.7f;
                    float sx = Mathf.Cos(a) * size;
                    float sz = Mathf.Sin(a) * size;
                    float y = Mathf.Sin(a) * size * Mathf.Sin(tilt * Mathf.Deg2Rad);
                    line.SetPosition(i, center + new Vector3(sx, y, sz));
                }
                yield return null;
            }
            if (line != null)
            {
                line.gameObject.SetActive(false);
                _freeRings.Enqueue(line);
            }
        }

        private void CreateQuaterniusSnapshot(Vector3 origin)
        {
            // One pose-snapshot rather than another simulated player / duplicated AI.
            // Guards keep one Echo inexpensive enough for WebGL mobile.
            SkinnedMeshRenderer[] sources = GetComponentsInChildren<SkinnedMeshRenderer>(false);
            int copied = 0;
            for (int i = 0; i < sources.Length && copied < 2; i++)
            {
                SkinnedMeshRenderer src = sources[i];
                if (!src.enabled || src.sharedMesh == null ||
                    src.sharedMesh.vertexCount > 22000) continue;
                Mesh snapshot = new Mesh();
                try { src.BakeMesh(snapshot); }
                catch (System.Exception)
                {
                    Destroy(snapshot);
                    continue;
                }
                GameObject go = new GameObject("R06_Quaternius_Ghost");
                go.transform.SetPositionAndRotation(src.transform.position +
                    origin - transform.position, src.transform.rotation);
                go.transform.localScale = src.transform.lossyScale;
                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = snapshot;
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _ghostMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                _activeGhosts.Add(go);
                StartCoroutine(FadeGhost(go, renderer, snapshot));
                copied++;
            }
        }

        private IEnumerator FadeGhost(GameObject go, MeshRenderer renderer, Mesh mesh)
        {
            const float duration = 1.42f;
            float elapsed = 0f;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            while (elapsed < duration && go != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                block.SetColor("_Color", new Color(rose.r, rose.g, rose.b,
                    .37f * (1f - t)));
                if (renderer != null) renderer.SetPropertyBlock(block);
                go.transform.position += Vector3.up * (.11f * Time.deltaTime);
                yield return null;
            }
            _activeGhosts.Remove(go);
            if (go != null) Destroy(go);
            if (mesh != null) Destroy(mesh);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _ultimateActive = false;
            // In-flight resources were not returned by canceled coroutines.
            // Destroy them once; an enabled component starts with a clean pool.
            foreach (Transform child in transform)
            {
                if (child != null && child.name.StartsWith("R06_Pooled_"))
                    Destroy(child.gameObject);
            }
            for (int i = 0; i < _activeGhosts.Count; i++)
            {
                GameObject ghost = _activeGhosts[i];
                if (ghost == null) continue;
                MeshFilter filter = ghost.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null) Destroy(filter.sharedMesh);
                Destroy(ghost);
            }
            _activeGhosts.Clear();
            _free.Clear();
            _freeRings.Clear();
            _particleObjects = 0;
            _ringObjects = 0;
        }

        private void OnDestroy()
        {
            if (_particleMaterial != null) Destroy(_particleMaterial);
            if (_lineMaterial != null) Destroy(_lineMaterial);
            if (_ghostMaterial != null) Destroy(_ghostMaterial);
        }
    }
}
