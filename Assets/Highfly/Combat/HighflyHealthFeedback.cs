using System.Collections;
using UnityEngine;

namespace Highfly.Combat
{
    [RequireComponent(typeof(HighflyHealth))]
    public sealed class HighflyHealthFeedback : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform textAnchor;
        [SerializeField] private Color normalTextColor = new Color(0.85f, 0.95f, 1f, 1f);
        [SerializeField] private Color criticalTextColor = new Color(0.72f, 0.35f, 1f, 1f);
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.18f, 0.22f, 1f);
        [SerializeField] private float hitFlashDuration = 0.09f;
        [SerializeField] private bool disableMovementOnDeath = true;

        private HighflyHealth _health;
        private Renderer[] _renderers;
        private Material[] _materials;
        private Color[] _baseColors;
        private float _previousHealth;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _health = GetComponent<HighflyHealth>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            _renderers = GetComponentsInChildren<Renderer>(true);
            _materials = new Material[_renderers.Length];
            _baseColors = new Color[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
            {
                Material material = _renderers[i].material;
                _materials[i] = material;
                _baseColors[i] = ReadColor(material);
            }
        }

        private void OnEnable()
        {
            if (_health == null)
                return;

            _previousHealth = _health.CurrentHealth > 0f ? _health.CurrentHealth : _health.MaxHealth;
            _health.HealthChanged += OnHealthChanged;
            _health.Died += OnDied;
        }

        private void Start()
        {
            if (_health != null)
                _previousHealth = _health.CurrentHealth;
        }

        private void OnDisable()
        {
            if (_health == null)
                return;

            _health.HealthChanged -= OnHealthChanged;
            _health.Died -= OnDied;
        }

        private void OnHealthChanged(float current, float max)
        {
            float damage = Mathf.Max(0f, _previousHealth - current);
            _previousHealth = current;

            if (damage <= 0f)
                return;

            if (animator != null && HasParameter(animator, "Hit"))
                animator.SetTrigger("Hit");

            SpawnDamageText(damage, max > 0f && damage >= max * 0.2f);

            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(HitFlash());
        }

        private void OnDied(HighflyHealth health)
        {
            if (animator != null && HasParameter(animator, "Death"))
                animator.SetTrigger("Death");

            if (disableMovementOnDeath)
            {
                HighflySimpleEnemyAI ai = GetComponent<HighflySimpleEnemyAI>();
                if (ai != null)
                    ai.enabled = false;

                CharacterController controller = GetComponent<CharacterController>();
                if (controller != null)
                    controller.enabled = false;
            }
        }

        private IEnumerator HitFlash()
        {
            SetMaterialColor(hitFlashColor);
            yield return new WaitForSeconds(hitFlashDuration);

            for (int i = 0; i < _materials.Length; i++)
                WriteColor(_materials[i], _baseColors[i]);

            _flashRoutine = null;
        }

        private void SpawnDamageText(float damage, bool critical)
        {
            GameObject go = new GameObject("Damage_" + Mathf.RoundToInt(damage));
            go.transform.position = textAnchor != null ? textAnchor.position : transform.position + Vector3.up * 2.25f;

            TextMesh text = go.AddComponent<TextMesh>();
            text.text = Mathf.RoundToInt(damage).ToString();
            text.fontSize = critical ? 64 : 48;
            text.characterSize = critical ? 0.045f : 0.038f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = critical ? criticalTextColor : normalTextColor;

            go.AddComponent<HighflyFloatingDamage>();
        }

        private void SetMaterialColor(Color color)
        {
            for (int i = 0; i < _materials.Length; i++)
                WriteColor(_materials[i], color);
        }

        private static Color ReadColor(Material material)
        {
            if (material == null)
                return Color.white;
            if (material.HasProperty("_BaseColor"))
                return material.GetColor("_BaseColor");
            if (material.HasProperty("_Color"))
                return material.GetColor("_Color");
            return Color.white;
        }

        private static void WriteColor(Material material, Color color)
        {
            if (material == null)
                return;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }

        private static bool HasParameter(Animator target, string name)
        {
            if (target == null)
                return false;

            AnimatorControllerParameter[] parameters = target.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == name)
                    return true;
            }

            return false;
        }
    }

    internal sealed class HighflyFloatingDamage : MonoBehaviour
    {
        private float _life = 0.85f;
        private Vector3 _velocity = new Vector3(0f, 1.2f, 0f);

        private void Update()
        {
            transform.position += _velocity * Time.deltaTime;
            _velocity *= 1f - Mathf.Clamp01(Time.deltaTime * 1.8f);

            Camera camera = Camera.main;
            if (camera != null)
                transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position, Vector3.up);

            _life -= Time.deltaTime;
            if (_life <= 0f)
                Destroy(gameObject);
        }
    }
}
