using System.Collections;
using UnityEngine;
using Highfly.Core;

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
        [SerializeField] private float attackWindup = 0.28f;
        [SerializeField] private float turnSpeed = 10f;

        [Header("World behaviour")]
        [SerializeField] private float leashRange = 22f;
        [SerializeField] private float patrolRadius = 4.5f;
        [SerializeField] private float patrolSpeedMultiplier = 0.48f;
        [SerializeField] private float patrolPauseMin = 1.2f;
        [SerializeField] private float patrolPauseMax = 3.8f;

        [Header("Elite / boss phase")]
        [SerializeField] private bool enrageEnabled;
        [SerializeField] private float enrageThreshold = 0.45f;
        [SerializeField] private float enrageMoveMultiplier = 1.25f;
        [SerializeField] private float enrageDamageMultiplier = 1.35f;
        [SerializeField] private float enrageIntervalMultiplier = 0.72f;

        private CharacterController _controller;
        private float _nextAttackTime;
        private bool _attackPending;
        private HighflyCombatController _targetCombat;
        private HighflyThirdPersonMotor _targetMotor;
        private Vector3 _home;
        private Vector3 _patrolPoint;
        private bool _hasPatrolPoint;
        private float _patrolWaitUntil;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (selfHealth == null)
                selfHealth = GetComponent<HighflyHealth>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            _home = transform.position;
        }

        private void Start()
        {
            ResolvePlayerTarget();
        }

        private void Update()
        {
            if (selfHealth != null && !selfHealth.IsAlive)
            {
                SetMoveAnimation(0f);
                return;
            }

            if (target == null || targetHealth == null || !targetHealth.IsAlive)
                ResolvePlayerTarget();

            if (target == null)
            {
                Patrol();
                return;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float homeDistance = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(_home.x, 0f, _home.z));

            if (homeDistance > leashRange)
            {
                ReturnHome();
                return;
            }

            if (distance > detectionRange || distance <= 0.001f)
            {
                Patrol();
                return;
            }

            Vector3 direction = toTarget / distance;
            Face(direction);

            bool enraged = IsEnraged();
            float currentMoveSpeed = moveSpeed * (enraged ? enrageMoveMultiplier : 1f);

            if (distance > attackRange)
            {
                SetMoveAnimation(enraged ? 1f : 0.82f);
                _controller.SimpleMove(direction * currentMoveSpeed);
                return;
            }

            SetMoveAnimation(0f);

            if (!_attackPending && Time.time >= _nextAttackTime)
            {
                float currentInterval = attackInterval * (enraged ? enrageIntervalMultiplier : 1f);
                _nextAttackTime = Time.time + Mathf.Max(0.35f, currentInterval);

                if (animator != null && HasParameter(animator, "Attack"))
                    animator.SetTrigger("Attack");

                StartCoroutine(AttackAfterWindup(enraged));
            }
        }

        private void Patrol()
        {
            if (Time.time < _patrolWaitUntil)
            {
                SetMoveAnimation(0f);
                return;
            }

            if (!_hasPatrolPoint)
            {
                Vector2 offset = Random.insideUnitCircle * Mathf.Max(0.5f, patrolRadius);
                _patrolPoint = _home + new Vector3(offset.x, 0f, offset.y);
                _hasPatrolPoint = true;
            }

            Vector3 delta = _patrolPoint - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.35f)
            {
                _hasPatrolPoint = false;
                _patrolWaitUntil = Time.time + Random.Range(patrolPauseMin, patrolPauseMax);
                SetMoveAnimation(0f);
                return;
            }

            Vector3 direction = delta.normalized;
            Face(direction);
            SetMoveAnimation(0.34f);
            _controller.SimpleMove(direction * moveSpeed * patrolSpeedMultiplier);
        }

        private void ReturnHome()
        {
            Vector3 delta = _home - transform.position;
            delta.y = 0f;

            if (delta.sqrMagnitude < 0.5f)
            {
                _hasPatrolPoint = false;
                SetMoveAnimation(0f);
                return;
            }

            Vector3 direction = delta.normalized;
            Face(direction);
            SetMoveAnimation(0.62f);
            _controller.SimpleMove(direction * moveSpeed * 0.78f);
        }

        private void Face(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
                return;

            Quaternion desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }

        private IEnumerator AttackAfterWindup(bool enraged)
        {
            _attackPending = true;
            yield return new WaitForSeconds(Mathf.Max(0.05f, attackWindup));

            if (targetHealth != null &&
                targetHealth.IsAlive &&
                selfHealth != null &&
                selfHealth.IsAlive &&
                target != null)
            {
                Vector3 delta = target.position - transform.position;
                delta.y = 0f;

                if (delta.magnitude <= attackRange + 0.45f)
                {
                    if (_targetMotor == null || !_targetMotor.IsDashing)
                    {
                        float finalDamage = attackDamage * (enraged ? enrageDamageMultiplier : 1f);
                        if (_targetCombat != null && _targetCombat.IsBlocking)
                            finalDamage *= 0.30f;
                        targetHealth.ApplyDamage(finalDamage);
                    }
                }
            }

            _attackPending = false;
        }

        private bool IsEnraged()
        {
            return enrageEnabled &&
                   selfHealth != null &&
                   selfHealth.IsAlive &&
                   selfHealth.Normalized <= Mathf.Clamp01(enrageThreshold);
        }

        private void ResolvePlayerTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;

            target = player.transform;
            targetHealth = player.GetComponent<HighflyHealth>();
            _targetCombat = player.GetComponent<HighflyCombatController>();
            _targetMotor = player.GetComponent<HighflyThirdPersonMotor>();
        }

        public void SetTarget(Transform targetTransform, HighflyHealth health)
        {
            target = targetTransform;
            targetHealth = health;

            if (targetTransform != null)
            {
                _targetCombat = targetTransform.GetComponent<HighflyCombatController>();
                _targetMotor = targetTransform.GetComponent<HighflyThirdPersonMotor>();
            }
        }

        public void SetAnimator(Animator value)
        {
            animator = value;
        }

        private void SetMoveAnimation(float amount)
        {
            if (animator == null)
                return;

            if (HasParameter(animator, "MoveSpeed"))
                animator.SetFloat("MoveSpeed", amount, 0.12f, Time.deltaTime);
            if (HasParameter(animator, "IsMoving"))
                animator.SetBool("IsMoving", amount > 0.05f);
        }

        private static bool HasParameter(Animator targetAnimator, string parameterName)
        {
            if (targetAnimator == null)
                return false;

            AnimatorControllerParameter[] parameters = targetAnimator.parameters;
            for (int i = 0; i < parameters.Length; i++)
                if (parameters[i].name == parameterName)
                    return true;

            return false;
        }
    }
}
