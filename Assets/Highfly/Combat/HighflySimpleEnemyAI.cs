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

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (selfHealth == null)
                selfHealth = GetComponent<HighflyHealth>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
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
                    _targetCombat = player.GetComponent<HighflyCombatController>();
                    _targetMotor = player.GetComponent<HighflyThirdPersonMotor>();
                }
            }
        }

        private void Update()
        {
            if (target == null || (selfHealth != null && !selfHealth.IsAlive))
            {
                SetMoveAnimation(0f);
                return;
            }

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

            bool enraged = IsEnraged();
            float currentMoveSpeed = moveSpeed * (enraged ? enrageMoveMultiplier : 1f);

            if (distance > attackRange)
            {
                SetMoveAnimation(enraged ? 1.2f : 1f);
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
                    // A dash during the telegraph is a successful evade.
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
