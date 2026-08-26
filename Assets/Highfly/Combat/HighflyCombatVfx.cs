using UnityEngine;

namespace Highfly.Combat
{
    public sealed class HighflyCombatVfx : MonoBehaviour
    {
        [SerializeField] private Transform origin;
        [SerializeField] private Color basicColor = new Color(0.25f, 0.85f, 1f, 1f);
        [SerializeField] private Color heavyColor = new Color(0.65f, 0.35f, 1f, 1f);
        [SerializeField] private Color skillOneColor = new Color(0.20f, 0.95f, 1f, 1f);
        [SerializeField] private Color skillTwoColor = new Color(0.55f, 0.30f, 1f, 1f);
        [SerializeField] private Color skillThreeColor = new Color(0.85f, 0.30f, 1f, 1f);

        private Material _particleMaterial;

        private void Awake()
        {
            if (origin == null)
                origin = transform;
            _particleMaterial = CreateParticleMaterial();
        }

        public void PlayBasic()
        {
            SpawnBurst("BasicSlash", basicColor, 16, 2.8f, 0.12f, 0.30f, 35f);
        }

        public void PlayHeavy()
        {
            SpawnBurst("HeavySlash", heavyColor, 24, 3.2f, 0.17f, 0.38f, 50f);
        }

        public void PlaySkillOne()
        {
            SpawnBurst("SkillLine", skillOneColor, 34, 5.0f, 0.13f, 0.42f, 16f);
        }

        public void PlaySkillTwo()
        {
            SpawnBurst("SkillCone", skillTwoColor, 42, 4.0f, 0.16f, 0.46f, 58f);
        }

        public void PlaySkillThree()
        {
            SpawnRing("SkillArea", skillThreeColor, 58, 4.6f, 0.18f, 0.55f);
        }

        public void PlayDash()
        {
            SpawnBurst("DashTrail", basicColor, 22, 2.2f, 0.10f, 0.25f, 90f);
        }

        private void SpawnBurst(string effectName, Color color, int count, float speed, float size, float lifetime, float angle)
        {
            GameObject go = new GameObject("VFX_" + effectName);
            go.transform.position = origin.position + Vector3.up * 0.9f;
            go.transform.rotation = transform.rotation;

            ParticleSystem system = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = Mathf.Max(64, count + 8);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = 0.08f;
            shape.length = 0.15f;

            ParticleSystem.ColorOverLifetimeModule colorLife = system.colorOverLifetime;
            colorLife.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.white, 0.45f),
                    new GradientColorKey(color, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.85f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorLife.color = gradient;

            ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.1f;
            renderer.velocityScale = 0.18f;
            if (_particleMaterial != null)
                renderer.sharedMaterial = _particleMaterial;

            system.Play();
            Destroy(go, lifetime + 0.35f);
        }

        private void SpawnRing(string effectName, Color color, int count, float speed, float size, float lifetime)
        {
            GameObject go = new GameObject("VFX_" + effectName);
            go.transform.position = transform.position + Vector3.up * 0.08f;

            ParticleSystem system = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.loop = false;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = Mathf.Max(80, count + 8);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.0f;
            shape.radiusThickness = 0.25f;

            ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (_particleMaterial != null)
                renderer.sharedMaterial = _particleMaterial;

            system.Play();
            Destroy(go, lifetime + 0.4f);
        }

        private static Material CreateParticleMaterial()
        {
            Shader shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null)
                shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            return shader != null ? new Material(shader) : null;
        }
    }
}
