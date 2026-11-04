using System;
using System.Collections;
using System.Collections.Generic;
using Highfly.Combat;
using Highfly.Core;
using UnityEngine;

namespace Highfly.SkillsLab
{
    // ORIGINAL HIGHFLY combat archetypes. Mechanics are genre references,
    // never Mobile Legends assets, extracted code, textures or animations.
    // This component is ONLY installed in the copied MOBA Lab scene.
    [DisallowMultipleComponent]
    public sealed class HighflyMobaSkillsLab : MonoBehaviour
    {
        public enum Style { AerialMage, RiftFighter, GravityGuardian }

        [SerializeField] private Style style;
        [SerializeField] private HighflyThirdPersonMotor fury;
        [SerializeField] private HighflyPlayerResources resources;
        [SerializeField] private HighflyTargetingSystem targetAssist;
        [SerializeField] private HighflyMobaSkillFx fx;
        [SerializeField] private LayerMask enemies = 1 << 8;
        [SerializeField] private float inputBufferSeconds = .18f;
        private readonly Collider[] _colliders = new Collider[96];
        private readonly HashSet<HighflyHealth> _unique = new HashSet<HighflyHealth>();
        private readonly Dictionary<HighflyHealth, float> _fracture = new Dictionary<HighflyHealth, float>();
        private readonly float[,] _readyAt = new float[3, 3];
        private float _recastUntil;
        private Style _recastOwner;
        private float _occupiedUntil;
        private Coroutine _ultimate;
        public Style CurrentStyle => style;
        public bool RecastAvailable => style == _recastOwner && Time.time < _recastUntil;
        public string StyleName => style == Style.AerialMage ? "HECHICERA AEREA"
            : style == Style.RiftFighter ? "ASALTANTE ABISAL" : "GUARDIAN GRAVITACIONAL";
        public string SkillName(int slot)
        {
            if (style == Style.AerialMage) return slot == 1 ? "LANZA VIOLETA" : slot == 2 ? "ASCENSO / RECAST" : "TEMPESTAD";
            if (style == Style.RiftFighter) return slot == 1 ? "CORTES DE FURIA" : slot == 2 ? "PASO / RECAST" : "CORTE FINAL";
            return slot == 1 ? "ONDA DE HIERRO" : slot == 2 ? "EMBESTIDA / RECAST" : "COLAPSO";
        }
        public float Cooldown(int slot)
        {
            if (slot < 1 || slot > 3) return 0f;
            if (slot == 2 && RecastAvailable) return 0f;
            return Mathf.Max(0f, _readyAt[(int)style, slot - 1] - Time.time);
        }

        private void Awake()
        {
            if (fury == null) fury = GetComponent<HighflyThirdPersonMotor>();
            if (resources == null) resources = GetComponent<HighflyPlayerResources>();
            if (targetAssist == null) targetAssist = GetComponent<HighflyTargetingSystem>();
            if (fx == null) fx = GetComponent<HighflyMobaSkillFx>();
        }

        public void SelectAerial() { SelectStyle(Style.AerialMage); }
        public void SelectRift() { SelectStyle(Style.RiftFighter); }
        public void SelectGravity() { SelectStyle(Style.GravityGuardian); }
        public void SelectStyle(Style chosen)
        {
            if (!Enum.IsDefined(typeof(Style), chosen)) return;
            // No skill/cooldown is silently reset on switching styles.
            style = chosen;
            _recastUntil = 0f;
        }
        public void SkillOne() { Cast(1, Vector3.zero); }
        public void SkillTwo() { Cast(2, Vector3.zero); }
        public void SkillThree() { Cast(3, Vector3.zero); }

        private Vector3 Aim(Vector3 request)
        {
            request.y = 0f;
            if (request.sqrMagnitude > .001f) return request.normalized;
            if (targetAssist != null)
            {
                if (!targetAssist.HasTarget) targetAssist.AcquireBestTarget();
                if (targetAssist.HasTarget && targetAssist.CurrentTarget != null)
                {
                    Vector3 toTarget = targetAssist.CurrentTarget.AimPoint.position - transform.position;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude > .001f) return toTarget.normalized;
                }
            }
            Vector3 fallback = fury != null && fury.LastMoveDirection.sqrMagnitude > .005f
                ? fury.LastMoveDirection : transform.forward;
            fallback.y = 0f;
            return fallback.sqrMagnitude > .001f ? fallback.normalized : Vector3.forward;
        }

        public bool Cast(int slot, Vector3 requestedDirection)
        {
            if (!isActiveAndEnabled || slot < 1 || slot > 3 || resources == null ||
                resources.Health == null || !resources.Health.IsAlive) return false;
            Vector3 dir = Aim(requestedDirection);
            if (slot == 2 && RecastAvailable)
            {
                if (fury == null || !fury.CanDash()) return false;
                if (!fury.TryDash(dir, style == Style.GravityGuardian ? 3.8f : 4.4f, .18f)) return false;
                _recastUntil = 0f;
                if (fx != null) fx.Recast(transform.position, dir, (int)style);
                if (style == Style.RiftFighter)
                    StartCoroutine(DelayedHitCone(.18f, 2.8f, 75f, 65f, 0f, true, dir));
                else if (style == Style.GravityGuardian)
                    StartCoroutine(DelayedHitSphere(.18f, 2.8f, 55f, 1.05f));
                return true;
            }

            int idx = (int)style;
            if (Time.time < _readyAt[idx, slot - 1] || Time.time < _occupiedUntil - inputBufferSeconds)
                return false;
            if (slot == 2 && (fury == null || !fury.CanDash())) return false;
            float cost = style == Style.AerialMage
                ? (slot == 1 ? 14f : slot == 2 ? 23f : 40f)
                : (slot == 1 ? 18f : slot == 2 ? 25f : 42f);
            // Magic -> Mana; Physical -> Stamina = HIGHFLY Energy.
            bool spent = style == Style.AerialMage
                ? resources.TrySpendMana(cost) : resources.TrySpendStamina(cost);
            if (!spent) return false;
            if (fury != null) fury.FaceForAction(dir);
            float cooldown = style == Style.AerialMage
                ? (slot == 1 ? 4.4f : slot == 2 ? 15f : 40f)
                : style == Style.RiftFighter
                    ? (slot == 1 ? 5f : slot == 2 ? 12f : 32f)
                    : (slot == 1 ? 6f : slot == 2 ? 14f : 44f);
            _readyAt[idx, slot - 1] = Time.time + cooldown;
            if (slot == 2)
            {
                _recastOwner = style;
                _recastUntil = Time.time + 3.5f;
            }
            if (style == Style.AerialMage) CastAerial(slot, dir);
            else if (style == Style.RiftFighter) CastRift(slot, dir);
            else CastGravity(slot, dir);
            return true;
        }

        private void CastAerial(int slot, Vector3 direction)
        {
            if (slot == 1) StartCoroutine(ArcaneBolt(direction));
            if (slot == 2)
            {
                if (fury != null && fury.IsGrounded) fury.Jump();
                fury.TryDash(direction, 7.0f, .42f, false, true);
                _occupiedUntil = Time.time + .43f;
                if (fx != null) fx.Dash(transform.position, direction, 0);
                StartCoroutine(DelayedHitSphere(.43f, 2.35f, 84f, 1.05f));
            }
            if (slot == 3)
            {
                if (_ultimate != null) StopCoroutine(_ultimate);
                _ultimate = StartCoroutine(AerialTempest());
            }
        }

        private IEnumerator ArcaneBolt(Vector3 direction)
        {
            yield return new WaitForSeconds(.12f);
            Vector3 previous = transform.position + Vector3.up * 1.05f + direction * .8f;
            float traveled = 0f;
            while (traveled < 16f)
            {
                float step = Mathf.Min(28f * Time.deltaTime, 16f - traveled);
                RaycastHit hit;
                if (Physics.SphereCast(previous, .4f, direction, out hit, step, ~0, QueryTriggerInteraction.Ignore)
                    && hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                {
                    HighflyHealth target = hit.collider.GetComponentInParent<HighflyHealth>();
                    if (target != null && IsEnemy(target))
                    {
                        Damage(target, 65f);
                        HighflySimpleEnemyAI ai = target.GetComponent<HighflySimpleEnemyAI>();
                        if (ai != null) ai.ApplySlow(.72f, 1.3f);
                        if (fx != null) fx.Impact(hit.point, 0, 1.3f);
                    }
                    break;
                }
                previous += direction * step;
                traveled += step;
                if (fx != null) fx.Projectile(previous, 0);
                yield return null;
            }
        }

        private IEnumerator AerialTempest()
        {
            _occupiedUntil = Time.time + 2f;
            const int hits = 11;
            float interval = 2f / hits;
            for (int tick = 0; tick < hits; tick++)
            {
                // One hit per target per tick, not per collider.
                HitSphere(transform.position, 4.0f, tick == 10 ? 55f : 22f, 0.38f, false);
                if (fx != null) fx.UltimateTick(transform.position, tick, 0);
                yield return new WaitForSeconds(interval);
            }
            _ultimate = null;
        }

        private void CastRift(int slot, Vector3 direction)
        {
            if (slot == 1)
            {
                HitCone(transform.position, direction, 6.7f, 47f, 54f, false);
                if (fx != null) fx.Cone(transform.position, direction, 1);
            }
            if (slot == 2)
            {
                fury.TryDash(direction, 6.3f, .27f, true, true);
                _occupiedUntil = Time.time + .28f;
                if (fx != null) fx.Dash(transform.position, direction, 1);
                StartCoroutine(DelayedHitCone(.28f, 2.9f, 95f, 60f, 0f, true, direction));
            }
            if (slot == 3)
            {
                _occupiedUntil = Time.time + .31f;
                if (fx != null) fx.Finisher(transform.position, direction, 1);
                StartCoroutine(DelayedFinisher(direction));
            }
        }

        private IEnumerator DelayedFinisher(Vector3 direction)
        {
            yield return new WaitForSeconds(.31f);
            CollectSphere(transform.position + direction * 5f, 6.5f);
            foreach (HighflyHealth enemy in _unique)
            {
                Vector3 delta = enemy.transform.position - transform.position;
                delta.y = 0f;
                float forward = Vector3.Dot(delta, direction);
                float side = Vector3.Cross(delta, direction).magnitude;
                if (forward < 0f || forward > 10f || side > 1.6f) continue;
                float missing = 1f - enemy.Normalized;
                Damage(enemy, 105f + 85f * missing);
            }
            if (fx != null) fx.Impact(transform.position + direction * 6f, 1, 2.8f);
        }

        private void CastGravity(int slot, Vector3 direction)
        {
            if (slot == 1)
            {
                HitCone(transform.position, direction, 7f, 57f, 50f, false);
                if (fx != null) fx.Cone(transform.position, direction, 2);
            }
            if (slot == 2)
            {
                fury.TryDash(direction, 5.9f, .36f, true, true);
                _occupiedUntil = Time.time + .37f;
                if (fx != null) fx.Dash(transform.position, direction, 2);
                StartCoroutine(DelayedPush(direction));
            }
            if (slot == 3)
            {
                if (_ultimate != null) StopCoroutine(_ultimate);
                _ultimate = StartCoroutine(GravityCollapse(direction));
            }
        }

        private IEnumerator DelayedPush(Vector3 direction)
        {
            yield return new WaitForSeconds(.36f);
            CollectSphere(transform.position, 3.0f);
            foreach (HighflyHealth enemy in _unique)
            {
                Damage(enemy, 45f);
                StartCoroutine(MoveEnemy(enemy, direction, .8f, .20f, .29f));
            }
            if (fx != null) fx.Impact(transform.position, 2, 3.0f);
        }

        private IEnumerator GravityCollapse(Vector3 direction)
        {
            Vector3 center = transform.position + direction * 1.0f;
            _occupiedUntil = Time.time + 1.02f;
            if (fx != null) fx.Vortex(center, 4.9f);
            // Pull is collision-aware and only affects the enemy capsule.
            // Never directly move the player/FURY capsule.
            for (int tick = 0; tick < 8; tick++)
            {
                CollectSphere(center, 4.9f);
                foreach (HighflyHealth enemy in _unique)
                {
                    if (!enemy.IsAlive) continue;
                    HighflySimpleEnemyAI ai = enemy.GetComponent<HighflySimpleEnemyAI>();
                    if (ai != null)
                    {
                        if (ai.IsBoss) continue;
                        ai.ApplyAirborne(.25f);
                    }
                    CharacterController cc = enemy.GetComponent<CharacterController>();
                    if (cc == null || !cc.enabled) continue;
                    Vector3 dir = center - enemy.transform.position;
                    dir.y = 0f;
                    if (dir.magnitude > 1.1f) cc.Move(dir.normalized * .30f);
                }
                yield return new WaitForSeconds(.09f);
            }
            HitSphere(center, 4.9f, 105f, 1.2f, false);
            if (fx != null) fx.Impact(center, 2, 4.9f);
            _ultimate = null;
        }

        private IEnumerator DelayedHitSphere(float delay, float radius, float damage, float airborne)
        {
            yield return new WaitForSeconds(delay);
            HitSphere(transform.position, radius, damage, airborne, false);
            if (fx != null) fx.Impact(transform.position, (int)style, radius);
        }

        private IEnumerator DelayedHitCone(float delay, float radius, float degrees,
            float damage, float airborne, bool fracture, Vector3 direction)
        {
            yield return new WaitForSeconds(delay);
            HitCone(transform.position, direction, radius, degrees, damage, fracture);
            if (fx != null) fx.Impact(transform.position, 1, radius);
        }

        private void HitSphere(Vector3 center, float radius, float damage,
            float airborneSeconds, bool fracture)
        {
            CollectSphere(center, radius);
            foreach (HighflyHealth enemy in _unique)
            {
                if (!Damage(enemy, damage)) continue;
                if (fracture) _fracture[enemy] = Time.time + 2.6f;
                if (airborneSeconds > 0f)
                {
                    HighflySimpleEnemyAI ai = enemy.GetComponent<HighflySimpleEnemyAI>();
                    if (ai != null) ai.ApplyAirborne(airborneSeconds);
                }
            }
        }

        private void HitCone(Vector3 center, Vector3 direction, float radius,
            float halfAngle, float damage, bool fracture)
        {
            CollectSphere(center, radius);
            foreach (HighflyHealth enemy in _unique)
            {
                Vector3 delta = enemy.transform.position - center;
                delta.y = 0f;
                if (delta.sqrMagnitude > .01f && Vector3.Angle(direction, delta) > halfAngle) continue;
                if (!Damage(enemy, damage)) continue;
                if (fracture) _fracture[enemy] = Time.time + 2.6f;
                else if (style == Style.GravityGuardian)
                {
                    HighflySimpleEnemyAI ai = enemy.GetComponent<HighflySimpleEnemyAI>();
                    if (ai != null) ai.ApplySlow(.65f, 1.2f);
                }
            }
        }

        private IEnumerator MoveEnemy(HighflyHealth enemy, Vector3 direction,
            float distance, float duration, float stagger)
        {
            if (enemy == null || !enemy.IsAlive) yield break;
            HighflySimpleEnemyAI ai = enemy.GetComponent<HighflySimpleEnemyAI>();
            if (ai != null)
            {
                if (ai.IsBoss) yield break;
                ai.ApplyAirborne(stagger);
            }
            CharacterController cc = enemy.GetComponent<CharacterController>();
            if (cc == null || !cc.enabled) yield break;
            float passed = 0f;
            while (passed < duration && enemy != null && enemy.IsAlive)
            {
                float step = Mathf.Min(Time.deltaTime, duration - passed);
                cc.Move(direction * (distance / duration) * step);
                passed += step;
                yield return null;
            }
        }

        private void CollectSphere(Vector3 center, float radius)
        {
            _unique.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, _colliders,
                enemies, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider c = _colliders[i];
                if (c == null) continue;
                HighflyHealth h = c.GetComponentInParent<HighflyHealth>();
                if (IsEnemy(h)) _unique.Add(h);
            }
        }

        private bool IsEnemy(HighflyHealth health)
        {
            if (health == null || !health.IsAlive || health == resources.Health) return false;
            HighflyTargetable target = health.GetComponentInChildren<HighflyTargetable>();
            return target == null || target.Faction == 1;
        }

        private bool Damage(HighflyHealth health, float raw)
        {
            if (!IsEnemy(health)) return false;
            float until;
            bool exposed = _fracture.TryGetValue(health, out until) && Time.time < until;
            return health.ApplyDamage(raw * (exposed ? 1.2f : 1f));
        }
    }
}
