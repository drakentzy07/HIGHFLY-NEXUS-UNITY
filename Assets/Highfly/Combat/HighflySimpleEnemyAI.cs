using UnityEngine;

namespace Highfly.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HighflySimpleEnemyAI : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private HighflyHealth targetHealth;
        [SerializeField] private HighflyHealth selfHealth;
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float attackRange = 1.7f;
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackInterval = 1.25f;
        [SerializeField] private float turnSpeed = 10f;

        private CharacterController _controller;
        private float _nextAttackTime;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (selfHealth == null)
                selfHealth = GetComponent<HighflyHealth>();
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
                return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (distance > detectionRange || distance <= 0.001f)
                return;

            Vector3 direction = toTarget / distance;
            Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));

            if (distance > attackRange)
            {
                _controller.SimpleMove(direction * moveSpeed);
                return;
            }

            if (Time.time >= _nextAttackTime)
            {
                _nextAttackTime = Time.time + attackInterval;
                if (targetHealth != null && targetHealth.IsAlive)
                    targetHealth.ApplyDamage(attackDamage);
            }
        }

        public void SetTarget(Transform targetTransform, HighflyHealth health)
        {
            target = targetTransform;
            targetHealth = health;
        }
    }
}
