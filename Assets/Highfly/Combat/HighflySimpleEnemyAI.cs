using UnityEngine;

namespace Highfly.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HighflySimpleEnemyAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private HighflyHealth targetHealth;
        [SerializeField] private HighflyHealth selfHealth;
        [SerializeField] private Animator animator;
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackInterval = 1.25f;
        [SerializeField] private float turnSpeed = 10f;
        [Header("Violet crowd control")]
        [SerializeField] private bool boss;
        [SerializeField] private float maxPoise = 100f;
        [Header("Guinevere suspension visual only (enemy capsule stays put)")]
        [SerializeField] private float airborneVisualHeight = 1.35f;
        [SerializeField] private float airborneRiseSeconds = .22f;
        [SerializeField] private float airborneFallSeconds = .25f;

        private float _slowUntil;
        private float _slowFactor = 1f;
        private float _airborneUntil;
        private float _airborneStart = -100f;
        private Transform _airborneVisual;
        private Vector3 _airborneRestLocal;
        private bool _airborneOffsetActive;
        private float _poise;
        public bool IsBoss => boss;

        private CharacterController _controller;
        private float _nextAttackTime;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (selfHealth == null)
                selfHealth = GetComponent<HighflyHealth>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            _poise = Mathf.Max(1f, maxPoise);
            BindAirborneVisual();
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    targetHealth = player.GetComponent<HighflyHealth>();
                }
            }
        }

        private void Update()
        {
            if (target == null || (selfHealth != null && !selfHealth.IsAlive))
            {
                RestoreAirborneVisual();
                SetMoveAnimation(0f);
                return;
            }

            if (Time.time < _airborneUntil)
            {
                UpdateAirborneVisual();
                SetMoveAnimation(0f);
                return;
            }
            RestoreAirborneVisual();

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > detectionRange || distance <= 0.001f)
            {
                SetMoveAnimation(0f);
                return;
            }

            Vector3 direction = toTarget / distance;
            Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));

            if (distance > attackRange)
            {
                SetMoveAnimation(1f);
                _controller.SimpleMove(direction * moveSpeed * (Time.time < _slowUntil ? _slowFactor : 1f));
                return;
            }

            SetMoveAnimation(0f);

            if (Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + attackInterval;

                if (animator != null && HasParameter(animator, "Attack"))
                    animator.SetTrigger("Attack");

                if (targetHealth != null && targetHealth.IsAlive)
                    targetHealth.ApplyDamage(attackDamage);
            }
        }

        public void ApplySlow(float multiplier, float seconds)
        {
            if (boss) return; // Bosses resist ordinary slows.
            _slowFactor = Mathf.Clamp(multiplier, .15f, 1f);
            _slowUntil = Mathf.Max(_slowUntil, Time.time + Mathf.Max(0f, seconds));
        }

        public void ApplyAirborne(float seconds)
        {
            if (boss)
            {
                _poise -= 42f;
                if (_poise <= 0f)
                {
                    _poise = Mathf.Max(1f, maxPoise);
                    _airborneUntil = Mathf.Max(_airborneUntil, Time.time + .35f);
                }
                return;
            }
            // Combat state is authoritative; only the humanoid child is raised
            // for the S2 -> S3 suspension visual. Collision/hitbox stay at root.
            if (Time.time >= _airborneUntil) _airborneStart = Time.time;
            _airborneUntil = Mathf.Max(_airborneUntil, Time.time + Mathf.Max(0f, seconds));
            BindAirborneVisual();
        }

        public void SetTarget(Transform targetTransform, HighflyHealth health)
        {
            target = targetTransform;
            targetHealth = health;
        }

        public void SetAnimator(Animator value)
        {
            RestoreAirborneVisual();
            animator = value;
            _airborneVisual = null;
            BindAirborneVisual();
        }

        private void BindAirborneVisual()
        {
            if (_airborneVisual != null || animator == null ||
                animator.transform == transform) return;
            _airborneVisual = animator.transform;
            _airborneRestLocal = _airborneVisual.localPosition;
        }

        private void UpdateAirborneVisual()
        {
            BindAirborneVisual();
            if (_airborneVisual == null || boss) return;
            float rise = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp01((Time.time - _airborneStart) /
                    Mathf.Max(.05f, airborneRiseSeconds)));
            float fall = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp01((_airborneUntil - Time.time) /
                    Mathf.Max(.05f, airborneFallSeconds)));
            float height = airborneVisualHeight * Mathf.Min(rise, fall);
            _airborneVisual.localPosition =
                _airborneRestLocal + Vector3.up * height;
            _airborneOffsetActive = true;
        }

        private void RestoreAirborneVisual()
        {
            // Never overwrite ordinary walking/idle animation root positions;
            // only unwind the offset if our own suspension code applied one.
            if (!_airborneOffsetActive) return;
            if (_airborneVisual != null)
                _airborneVisual.localPosition = _airborneRestLocal;
            _airborneOffsetActive = false;
        }

        private void OnDisable()
        {
            RestoreAirborneVisual();
            _airborneUntil = 0f;
        }

        private void SetMoveAnimation(float amount)
        {
            if (animator == null)
                return;

            if (HasParameter(animator, "MoveSpeed"))
                animator.SetFloat("MoveSpeed", amount, 0.1f, Time.deltaTime);
            if (HasParameter(animator, "IsMoving"))
                animator.SetBool("IsMoving", amount > 0.05f);
        }

        private static bool HasParameter(Animator targetAnimator, string parameterName)
        {
            AnimatorControllerParameter[] parameters = targetAnimator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName)
                    return true;
            }

            return false;
        }
    }
}
