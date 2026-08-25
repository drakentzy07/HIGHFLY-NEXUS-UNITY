using System.Collections;
using UnityEngine;
using Highfly.Core;

namespace Highfly.Combat
{
    public sealed class HighflyCombatController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HighflyThirdPersonMotor motor;
        [SerializeField] private HighflyTargetingSystem targeting;
        [SerializeField] private HighflyPlayerResources resources;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private LayerMask enemyMask = ~0;

        [Header("Basic combo")]
        [SerializeField] private float basicDamage = 12f;
        [SerializeField] private float basicRadius = 2.1f;
        [SerializeField] private float basicHalfAngle = 62f;
        [SerializeField] private float comboResetTime = 0.72f;
        [SerializeField] private float basicHitDelay = 0.10f;

        [Header("Heavy")]
        [SerializeField] private float heavyDamage = 25f;
        [SerializeField] private float heavyRadius = 2.5f;
        [SerializeField] private float heavyHitDelay = 0.18f;
        [SerializeField] private float heavyStaminaCost = 12f;

        [Header("Mobility / defense")]
        [SerializeField] private float dashStaminaCost = 20f;
        [SerializeField] private float blockStaminaDrainPerSecond = 12f;

        [Header("Skills")]
        [SerializeField] private float lineDamage = 34f;
        [SerializeField] private float lineLength = 6.5f;
        [SerializeField] private float lineRadius = 1.05f;
        [SerializeField] private float lineManaCost = 16f;
        [SerializeField] private float coneDamage = 30f;
        [SerializeField] private float coneRadius = 4.2f;
        [SerializeField] private float coneHalfAngle = 48f;
        [SerializeField] private float coneManaCost = 20f;
        [SerializeField] private float areaDamage = 26f;
        [SerializeField] private float areaRadius = 3.2f;
        [SerializeField] private float areaManaCost = 24f;

        private int _comboIndex;
        private float _lastBasicTime = -99f;
        private bool _isBlocking;

        public bool IsBlocking => _isBlocking;

        private void Awake()
        {
            if (motor == null)
                motor = GetComponent<HighflyThirdPersonMotor>();
            if (targeting == null)
                targeting = GetComponent<HighflyTargetingSystem>();
            if (resources == null)
                resources = GetComponent<HighflyPlayerResources>();
            if (attackOrigin == null)
                attackOrigin = transform;
        }

        private void Update()
        {
            if (_isBlocking && resources != null)
            {
                if (!resources.TrySpendStamina(blockStaminaDrainPerSecond * Time.deltaTime))
                    EndBlock();
            }
        }

        public void BasicAttack()
        {
            // Critical design rule: an attack always fires even with NO selected target.
            // Targeting is soft assistance, never a requirement.
            if (Time.time - _lastBasicTime > comboResetTime)
                _comboIndex = 0;

            _comboIndex = (_comboIndex % 3) + 1;
            _lastBasicTime = Time.time;

            FaceSoftTarget();

            if (animator != null)
            {
                animator.SetInteger("ComboIndex", _comboIndex);
                animator.SetTrigger("BasicAttack");
            }

            StartCoroutine(DelayedBasicHit(basicHitDelay));
        }

        public void HeavyAttack()
        {
            if (resources != null && !resources.TrySpendStamina(heavyStaminaCost))
                return;

            FaceSoftTarget();
            if (animator != null)
                animator.SetTrigger("HeavyAttack");
            StartCoroutine(DelayedHeavyHit(heavyHitDelay));
        }

        public void DashOrDodge()
        {
            if (motor == null)
                return;
            if (resources != null && !resources.TrySpendStamina(dashStaminaCost))
                return;

            Vector3 direction = motor.LastMoveDirection.sqrMagnitude > 0.001f ? motor.LastMoveDirection : transform.forward;
            motor.Dash(direction);
        }

        public void BeginBlock()
        {
            SetBlock(true);
        }

        public void EndBlock()
        {
            SetBlock(false);
        }

        public void SetBlock(bool active)
        {
            _isBlocking = active;
            if (animator != null)
                animator.SetBool("Blocking", active);
        }

        public void SkillLineCleave()
        {
            if (resources != null && !resources.TrySpendMana(lineManaCost))
                return;

            FaceSoftTarget();
            if (animator != null)
                animator.SetTrigger("Skill1");

            Vector3 origin = attackOrigin.position + Vector3.up * 0.7f;
            Vector3 direction = GetAttackDirection();
            HighflyCombatQueries.DamageLine(origin, direction, lineLength, lineRadius, enemyMask, lineDamage);
        }

        public void SkillCone()
        {
            if (resources != null && !resources.TrySpendMana(coneManaCost))
                return;

            FaceSoftTarget();
            if (animator != null)
                animator.SetTrigger("Skill2");

            HighflyCombatQueries.DamageArc(attackOrigin.position, GetAttackDirection(), coneRadius, coneHalfAngle, enemyMask, coneDamage);
        }

        public void SkillArea()
        {
            if (resources != null && !resources.TrySpendMana(areaManaCost))
                return;

            if (animator != null)
                animator.SetTrigger("Skill3");

            Vector3 center = transform.position + transform.forward * 1.1f;
            HighflyCombatQueries.DamageArea(center, areaRadius, enemyMask, areaDamage);
        }

        // Final animation clips can call these events. Prototype timing currently
        // uses the delayed fallbacks so Combat Lab works before final animation import.
        public void AnimationEventBasicHit()
        {
            ApplyBasicHit();
        }

        public void AnimationEventHeavyHit()
        {
            ApplyHeavyHit();
        }

        private IEnumerator DelayedBasicHit(float delay)
        {
            yield return new WaitForSeconds(delay);
            ApplyBasicHit();
        }

        private IEnumerator DelayedHeavyHit(float delay)
        {
            yield return new WaitForSeconds(delay);
            ApplyHeavyHit();
        }

        private void ApplyBasicHit()
        {
            // Sweep/cleave by design: two enemies standing together can both be hit.
            HighflyCombatQueries.DamageArc(attackOrigin.position, GetAttackDirection(), basicRadius, basicHalfAngle, enemyMask, basicDamage);
        }

        private void ApplyHeavyHit()
        {
            HighflyCombatQueries.DamageArc(attackOrigin.position, GetAttackDirection(), heavyRadius, 78f, enemyMask, heavyDamage);
        }

        private Vector3 GetAttackDirection()
        {
            if (targeting != null && targeting.HasTarget)
            {
                Vector3 direction = targeting.CurrentTarget.AimPoint.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                    return direction.normalized;
            }

            return transform.forward;
        }

        private void FaceSoftTarget()
        {
            if (targeting == null)
                return;

            if (!targeting.HasTarget)
                targeting.AcquireBestTarget();

            if (!targeting.HasTarget)
                return;

            Vector3 direction = targeting.CurrentTarget.AimPoint.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
