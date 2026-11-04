using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Highfly.SkillsLab
{
    /// <summary>
    /// HIGHFLY-owned procedural, visual-only VFX for the isolated MOBA lab.
    /// Damage / movement are NEVER controlled here. No third-party media.
    /// Pool limits are deliberately conservative for mobile WebGL.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyMobaSkillFx : MonoBehaviour
    {
        [SerializeField, Range(0, 2)] private int quality = 1;
        private readonly Queue<ParticleSystem> _particles = new Queue<ParticleSystem>();
        private readonly Queue<LineRenderer> _lines = new Queue<LineRenderer>();
        private int _particleCount, _lineCount;
        private Material _particleMaterial, _lineMaterial;
        private float _lastProjectile;
        private static readonly Color Aether = new Color(.66f, .27f, 1f);
        private static readonly Color AetherHot = new Color(1f, .35f, .9f);
        private static readonly Color Rift = new Color(.96f, .14f, .27f);
        private static readonly Color RiftHot = new Color(1f, .52f, .26f);
        private static readonly Color Iron = new Color(.96f, .75f, .34f);
        private static readonly Color IronHot = new Color(1f, .96f, .75f);

        private Color Main(int style) => style == 0 ? Aether : style == 1 ? Rift : Iron;
        private Color Accent(int style) => style == 0 ? AetherHot : style == 1 ? RiftHot : IronHot;
        private int MaxEmitters => quality == 0 ? 14 : quality == 1 ? 25 : 35;
        private int MaxLines => quality == 0 ? 10 : quality == 1 ? 19 : 28;
        public void SetQuality(int value) { quality = Mathf.Clamp(value, 0, 2); }
        public void CycleQuality() { quality = (quality + 1) % 3; }

        private void Awake()
        {
            Shader particleShader = Shader.Find("Particles/Standard Unlit");
            if (particleShader == null) particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader == null) lineShader = Shader.Find("Unlit/Color");
            if (particleShader != null) _particleMaterial = new Material(particleShader);
            if (lineShader != null) _lineMaterial = new Material(lineShader);
        }

        private void OnDestroy()
        {
            if (_particleMaterial != null) Destroy(_particleMaterial);
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }

        public void Projectile(Vector3 world, int style)
        {
            if (Time.time - _lastProjectile < (quality == 0 ? .095f : .045f)) return;
            _lastProjectile = Time.time;
            Spark(world, Main(style), quality == 0 ? 3 : 7, 1.2f, .085f, .28f);
        }

        public void Dash(Vector3 start, Vector3 direction, int style)
        {
            Color c = Main(style);
            Pulse(start, 1.25f, .34f, c, .08f);
            Pulse(start + direction * 2.4f, 1.2f, .30f, Accent(style), .08f);
            Spark(start + Vector3.up, c, quality == 0 ? 8 : 19, 2.4f, .14f, .43f);
            StartCoroutine(DashTrail(start, direction, style));
        }

        private IEnumerator DashTrail(Vector3 start, Vector3 direction, int style)
        {
            for (int i = 1; i <= 5; i++)
            {
                yield return new WaitForSeconds(.055f);
                Vector3 p = start + direction * (i * .84f);
                Pulse(p, .48f + i * .15f, .25f, Accent(style), .08f);
            }
        }

        public void Recast(Vector3 at, Vector3 direction, int style)
        {
            Pulse(at, 1.6f, .32f, Main(style), .16f);
            Pulse(at + direction * 2f, 1.3f, .36f, Accent(style), .11f);
            Spark(at + Vector3.up, Accent(style), quality == 0 ? 13 : 28, 3.4f, .12f, .45f);
        }

        public void Cone(Vector3 origin, Vector3 direction, int style)
        {
            Color c = Main(style);
            // Slash arcs convey heading, not collision. HitCone owns the actual gameplay.
            Arc(origin + Vector3.up * .85f, direction, 3.3f, 112f, .34f, c, .20f);
            Arc(origin + Vector3.up * .55f + direction * 1.2f,
                direction, 4.8f, 88f, .47f, Accent(style), .12f);
            Spark(origin + direction * 2.6f + Vector3.up, Accent(style),
                quality == 0 ? 10 : 22, 3.2f, .17f, .42f);
        }

        public void Impact(Vector3 at, int style, float radius)
        {
            Spark(at + Vector3.up * .55f, Accent(style),
                quality == 0 ? 18 : 39, 4.0f, .16f, .54f);
            Pulse(at, radius, .57f, Main(style), .22f);
            Pulse(at, radius * .64f, .29f, Accent(style), .11f);
        }

        public void UltimateTick(Vector3 at, int hit, int style)
        {
            float angle = hit * 2.39996323f;
            Vector3 point = at + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) *
                (1.3f + (hit % 4) * .55f);
            point.y += .95f + (hit % 3) * .42f;
            Spark(point, hit % 2 == 0 ? Main(style) : Accent(style),
                quality == 0 ? 8 : 20, 3.3f, .10f, .34f);
            if (hit == 0 || hit == 4 || hit == 7) Pulse(at, 4f, .48f, Main(style), .10f);
            if (hit == 10) Impact(at, style, 4.2f);
        }

        public void Finisher(Vector3 origin, Vector3 direction, int style)
        {
            StartCoroutine(FinisherWindup(origin, direction, style));
        }

        private IEnumerator FinisherWindup(Vector3 origin, Vector3 direction, int style)
        {
            for (int i = 0; i < 3; i++)
            {
                Spark(origin + Vector3.up * 1.1f + direction * i * .65f,
                    Accent(style), 14, 1.6f, .14f, .28f);
                yield return new WaitForSeconds(.075f);
            }
            Arc(origin + Vector3.up * 1.3f + direction * 5f, direction,
                4.8f, 100f, .55f, Main(style), .25f);
            Arc(origin + Vector3.up * 1.1f + direction * 6f, direction,
                4.2f, 120f, .42f, Accent(style), .14f);
        }

        public void Vortex(Vector3 center, float radius)
        {
            StartCoroutine(Spiral(center, radius));
        }

        private IEnumerator Spiral(Vector3 center, float radius)
        {
            Color c = Iron;
            for (int i = 0; i < 9; i++)
            {
                float t = (float)i / 8f;
                Pulse(center, radius * (1f - t * .80f), .40f, c, .15f);
                float a = i * 2.38f;
                Vector3 p = center + new Vector3(Mathf.Cos(a), 0f,
                    Mathf.Sin(a)) * (radius * (1f - t));
                Spark(p + Vector3.up * (.2f + t), IronHot, quality == 0 ? 5 : 12,
                    1.3f, .13f, .42f);
                yield return new WaitForSeconds(.09f);
            }
        }

        private void Spark(Vector3 center, Color color, int count,
            float speed, float size, float life)
        {
            ParticleSystem ps = AcquireParticles();
            if (ps == null) return;
            ps.transform.position = center;
            ps.gameObject.SetActive(true);
            var main = ps.main;
            main.startColor = color;
            main.startSpeed = speed;
            main.startSize = size;
            main.startLifetime = life;
            ps.Clear(true);
            ps.Emit(Mathf.Min(count, quality == 0 ? 24 : quality == 1 ? 52 : 76));
            StartCoroutine(ReturnParticles(ps, life + .12f));
        }

        private ParticleSystem AcquireParticles()
        {
            if (_particles.Count > 0) return _particles.Dequeue();
            if (_particleCount >= MaxEmitters || _particleMaterial == null) return null;
            _particleCount++;
            var obj = new GameObject("HIGHFLY_MOBA_POOLED_PARTICLES");
            obj.transform.SetParent(transform);
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false; main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.duration = 1f; main.maxParticles = 100;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = .12f;
            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _particleMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return ps;
        }

        private IEnumerator ReturnParticles(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (ps == null) yield break;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            _particles.Enqueue(ps);
        }

        private void Pulse(Vector3 center, float radius, float seconds,
            Color color, float width)
        {
            LineRenderer line = AcquireLine();
            if (line == null) return;
            line.gameObject.SetActive(true);
            StartCoroutine(PulseLine(line, center, radius, seconds, color, width));
        }

        private IEnumerator PulseLine(LineRenderer line, Vector3 center, float radius,
            float seconds, Color color, float width)
        {
            line.loop = true;
            line.positionCount = quality == 0 ? 18 : 40;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                float t = Mathf.Clamp01(elapsed / seconds);
                float r = radius * (.15f + .85f * t);
                Color fade = color; fade.a = 1f - t;
                line.startColor = line.endColor = fade;
                line.startWidth = line.endWidth = width * (1f - t * .45f);
                for (int i = 0; i < line.positionCount; i++)
                {
                    float angle = 2f * Mathf.PI * i / line.positionCount;
                    line.SetPosition(i, center + new Vector3(
                        Mathf.Cos(angle) * r, .09f + .3f * t, Mathf.Sin(angle) * r));
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            ReturnLine(line);
        }

        private void Arc(Vector3 center, Vector3 facing, float radius, float spread,
            float seconds, Color color, float width)
        {
            LineRenderer line = AcquireLine();
            if (line == null) return;
            line.gameObject.SetActive(true);
            StartCoroutine(ArcLine(line, center, facing, radius, spread, seconds, color, width));
        }

        private IEnumerator ArcLine(LineRenderer line, Vector3 center,
            Vector3 facing, float radius, float spread, float seconds, Color color, float width)
        {
            line.loop = false; line.positionCount = 21;
            float angle0 = Mathf.Atan2(facing.z, facing.x);
            float time = 0f;
            while (time < seconds)
            {
                float t = Mathf.Clamp01(time / seconds);
                Color fade = color; fade.a = 1f - t;
                line.startColor = line.endColor = fade;
                line.startWidth = line.endWidth = width * (1f - .6f * t);
                for (int i = 0; i < line.positionCount; i++)
                {
                    float a = angle0 + (i / 20f - .5f) * spread * Mathf.Deg2Rad;
                    line.SetPosition(i, center + new Vector3(
                        Mathf.Cos(a) * radius, Mathf.Sin(i / 20f * Mathf.PI) * .55f,
                        Mathf.Sin(a) * radius));
                }
                time += Time.deltaTime;
                yield return null;
            }
            ReturnLine(line);
        }

        private LineRenderer AcquireLine()
        {
            if (_lines.Count > 0) return _lines.Dequeue();
            if (_lineCount >= MaxLines || _lineMaterial == null) return null;
            _lineCount++;
            GameObject obj = new GameObject("HIGHFLY_MOBA_POOLED_LINE");
            obj.transform.SetParent(transform);
            LineRenderer l = obj.AddComponent<LineRenderer>();
            l.useWorldSpace = true; l.sharedMaterial = _lineMaterial;
            l.shadowCastingMode = ShadowCastingMode.Off; l.receiveShadows = false;
            l.numCapVertices = 0; l.numCornerVertices = 0;
            return l;
        }

        private void ReturnLine(LineRenderer line)
        {
            if (line == null) return;
            line.gameObject.SetActive(false);
            _lines.Enqueue(line);
        }
    }
}
