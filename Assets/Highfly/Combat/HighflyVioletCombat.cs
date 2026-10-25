using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Highfly.Core;

namespace Highfly.Combat
{
    /// <summary>
    /// Independent HIGHFLY port of Proyecto Violeta R05 (not a Mobile Legends asset).
    /// No global game state, no direct dependencies on ClaudeCraft or Three.js.
    /// The combat rules remain separate from the renderer and mobile input.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyVioletCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HighflyPlayerResources resources;
        [SerializeField] private HighflyTargetingSystem targeting;
        [SerializeField] private HighflyThirdPersonMotor motor;
        [SerializeField] private HighflyVioletVfx effects;
        [SerializeField] private Animator animator;
        [SerializeField] private LayerMask enemyMask = 1 << 8;

        [Header("Prisma U1")]
        [SerializeField] private float prismaDamage = 78f;
        [SerializeField] private float prismaSpeed = 25f;
        [SerializeField] private float prismaRange = 15f;
        [SerializeField] private float prismaRadius = .52f;

        [Header("Ruptura U2")]
        [SerializeField] private float rupturaRange = 8f;
        [SerializeField] private float rupturaDuration = .42f;
        [SerializeField] private float rupturaRadius = 2.1f;

        [Header("Tempestad U3")]
        [SerializeField] private float tempestRadius = 4.2f;
        [SerializeField] private int tempestHits = 11;
        [SerializeField] private float tempestDuration = 2f;
        [SerializeField] private float tempestDamage = 352f;

        public float SuperMagic { get; private set; }
        public float SuperMagicNormalized => SuperMagic / 100f;
        public float PrismaCooldown => Mathf.Max(0f, _nextU1 - Time.time);
        public float RupturaCooldown => Mathf.Max(0f, _nextU2 - Time.time);
        public float TempestadCooldown => Mathf.Max(0f, _nextU3 - Time.time);
        public bool CanRupturaRecast => Time.time < _recastUntil;
        public bool IsCasting => _busy;

        private struct State
        {
            public int Marks;
            public float MarksUntil;
            public float AirborneUntil;
            public int Juggles;
        }

        private readonly Dictionary<HighflyHealth, State> _states = new Dictionary<HighflyHealth, State>();
        private readonly Collider[] _hits = new Collider[80];
        private readonly RaycastHit[] _rayHits = new RaycastHit[24];
        private CharacterController _controller;
        private float _nextU1, _nextU2, _nextU3, _nextAttack, _recastUntil;
        private int _combo;
        private float _lastAttack = -99f;
        private bool _busy;
        private int _bufferedSkill;
        private Vector3 _bufferedDirection;

        private void Awake()
        {
            if (resources == null) resources = GetComponent<HighflyPlayerResources>();
            if (targeting == null) targeting = GetComponent<HighflyTargetingSystem>();
            if (motor == null) motor = GetComponent<HighflyThirdPersonMotor>();
            if (effects == null) effects = GetComponent<HighflyVioletVfx>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            _controller = GetComponent<CharacterController>();
        }

        private bool TryCast(int slot, float mana, float readyAt)
        {
            if (_busy)
            {
                // One buffered action; prevents a fast tap during landing from disappearing.
                _bufferedSkill = slot;
                _bufferedDirection = AimDirection(Vector3.zero);
                return false;
            }
            if (resources == null || resources.Health == null || !resources.Health.IsAlive) return false;
            if (Time.time < readyAt || resources.Mana < mana) return false;
            return resources.TrySpendMana(mana);
        }

        private Vector3 AimDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > .001f) return direction.normalized;
            if (targeting != null && !targeting.HasTarget) targeting.AcquireBestTarget();
            if (targeting != null && targeting.HasTarget)
            {
                direction = targeting.CurrentTarget.AimPoint.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > .001f) return direction.normalized;
            }
            direction = motor != null ? motor.LastMoveDirection : transform.forward;
            direction.y = 0f;
            return direction.sqrMagnitude > .001f ? direction.normalized : transform.forward;
        }

        private void Face(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > .001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        public void BasicAttack()
        {
            if (_busy || Time.time < _nextAttack || resources == null ||
                resources.Health == null || !resources.Health.IsAlive) return;
            if (Time.time - _lastAttack > .92f) _combo = 0;
            _combo = (_combo % 4) + 1;
            _lastAttack = Time.time;
            _nextAttack = Time.time + .42f;

            bool charged = SuperMagic >= 100f;
            float range = charged ? 6.3f : 2.6f;
            Vector3 dir = AimDirection(Vector3.zero);
            HighflyHealth target = FindNearestTarget(transform.position, range, dir, 85f);
            Face(dir);
            if (animator != null)
            {
                animator.SetInteger("ComboIndex", Mathf.Min(_combo, 3));
                animator.SetTrigger("BasicAttack");
            }
            if (target == null)
            {
                if (effects != null) effects.Basic(transform.position, _combo);
                return; // Missed basics do not recharge Super Magic.
            }
            Vector3 strikeDirection = target.transform.position - transform.position;
            strikeDirection.y = 0f;
            Face(strikeDirection);
            float amount = charged ? 96f : 44f * (_combo == 4 ? 1.35f : 1f);
            if (Damage(target, amount, false, charged ? "charged-atk" : "basic-atk"))
            {
                if (charged)
                {
                    SuperMagic = 0f;
                    if (resources.Health != null)
                    {
                        // R05 was tuned for 1000 HP. Scale the flat heal to this Unity hero's max HP.
                        float missing = resources.Health.MaxHealth - resources.Health.CurrentHealth;
                        resources.Health.Heal(resources.Health.MaxHealth * .08f + missing * .08f);
                    }
                    Mark(target);
                }
                else GainMagic(34f);
            }
            if (effects != null) effects.Basic(target.transform.position, charged ? 5 : _combo);
        }

        public void Prisma() { CastPrisma(Vector3.zero); }
        public void CastPrisma(Vector3 requestedDirection)
        {
            if (!TryCast(1, 14f, _nextU1)) return;
            _nextU1 = Time.time + 4f;
            Vector3 direction = AimDirection(requestedDirection);
            Face(direction);
            if (animator != null) animator.SetTrigger("Skill1");
            StartCoroutine(PrismaProjectile(direction));
        }

        private IEnumerator PrismaProjectile(Vector3 direction)
        {
            yield return new WaitForSeconds(.14f);
            Vector3 current = transform.position + Vector3.up * 1.1f + direction * .85f;
            Transform orb = effects != null ? effects.CreatePrisma(current) : null;
            float distance = 0f;
            while (distance < prismaRange)
            {
                float step = Mathf.Min(prismaSpeed * Time.deltaTime, prismaRange - distance);
                int count = Physics.SphereCastNonAlloc(current, prismaRadius, direction, _rayHits, step, ~0, QueryTriggerInteraction.Collide);
                float nearest = float.PositiveInfinity;
                HighflyHealth hitHealth = null;
                bool blocked = false;
                for (int i = 0; i < count; i++)
                {
                    Collider hitCollider = _rayHits[i].collider;
                    if (hitCollider == null || hitCollider.transform.IsChildOf(transform)) continue;
                    int mask = 1 << hitCollider.gameObject.layer;
                    if ((mask & enemyMask.value) != 0)
                    {
                        HighflyHealth candidate = hitCollider.GetComponentInParent<HighflyHealth>();
                        if (candidate == null || !candidate.IsAlive || candidate == resources.Health) continue;
                        if (_rayHits[i].distance < nearest)
                        {
                            nearest = _rayHits[i].distance; hitHealth = candidate; blocked = true;
                        }
                    }
                    else if (!hitCollider.isTrigger && _rayHits[i].distance < nearest)
                    {
                        nearest = _rayHits[i].distance; hitHealth = null; blocked = true;
                    }
                }
                if (blocked)
                {
                    current += direction * Mathf.Max(0f, nearest);
                    if (hitHealth != null && Damage(hitHealth, prismaDamage, true, "prisma"))
                    {
                        Mark(hitHealth);
                        var ai = hitHealth.GetComponent<HighflySimpleEnemyAI>();
                        if (ai != null) ai.ApplySlow(.75f, 1.2f);
                        _nextU1 = Mathf.Max(Time.time, _nextU1 - 1f);
                        _nextU2 = Mathf.Max(Time.time, _nextU2 - 1f);
                        _nextU3 = Mathf.Max(Time.time, _nextU3 - 1f);
                    }
                    if (effects != null) effects.Impact(current, .8f);
                    break;
                }
                current += direction * step;
                distance += step;
                if (orb != null) orb.position = current;
                yield return null;
            }
            if (orb != null) Destroy(orb.gameObject);
        }

        public void Ruptura() { CastRuptura(Vector3.zero); }
        public void CastRuptura(Vector3 requestedDirection)
        {
            Vector3 dir = AimDirection(requestedDirection);
            if (CanRupturaRecast && !_busy)
            {
                _recastUntil = 0f;
                Vector3 oldPos = transform.position;
                if (_controller != null) _controller.Move(dir * 4.7f);
                else transform.position += dir * 4.7f;
                Face(dir);
                if (effects != null) effects.Blink(oldPos, transform.position);
                StartCoroutine(EchoExplosion(oldPos));
                return;
            }
            if (!TryCast(2, 23f, _nextU2)) return;
            _nextU2 = Time.time + 17f;
            Face(dir);
            if (animator != null) animator.SetTrigger("Skill2");
            StartCoroutine(RupturaLeap(dir));
        }

        private IEnumerator RupturaLeap(Vector3 direction)
        {
            _busy = true;
            if (motor != null) motor.ExternalMovementLock = true;
            float distance = rupturaRange * .75f;
            if (targeting != null && targeting.HasTarget)
                distance = Mathf.Min(rupturaRange, Vector3.Distance(transform.position, targeting.CurrentTarget.AimPoint.position));
            Vector3 start = transform.position;
            float elapsed = 0f;
            while (elapsed < rupturaDuration)
            {
                float dt = Mathf.Min(Time.deltaTime, rupturaDuration - elapsed);
                elapsed += dt;
                Vector3 delta = direction * (distance / rupturaDuration) * dt;
                if (_controller != null) _controller.Move(delta);
                else transform.position += delta;
                if (effects != null && elapsed < dt + .02f) effects.Launch(start);
                yield return null;
            }
            if (effects != null) effects.Impact(transform.position, rupturaRadius);
            int count = Physics.OverlapSphereNonAlloc(transform.position, rupturaRadius, _hits, enemyMask, QueryTriggerInteraction.Collide);
            HashSet<HighflyHealth> unique = new HashSet<HighflyHealth>();
            for (int i = 0; i < count; i++)
            {
                HighflyHealth target = _hits[i].GetComponentInParent<HighflyHealth>();
                if (target == null || target == resources.Health || !unique.Add(target)) continue;
                if (Damage(target, 96f, true, "ruptura") && target.IsAlive)
                {
                    Damage(target, 48f, true, "ruptura-bonus");
                    Mark(target);
                    Lift(target, 1f);
                }
            }
            _recastUntil = Time.time + 5f;
            if (motor != null) motor.ExternalMovementLock = false;
            _busy = false;
            FlushBuffer();
        }

        private IEnumerator EchoExplosion(Vector3 position)
        {
            if (effects != null) effects.Echo(position);
            yield return new WaitForSeconds(1.5f);
            if (effects != null) effects.Impact(position, 2.05f);
            int count = Physics.OverlapSphereNonAlloc(position, 2.05f, _hits, enemyMask, QueryTriggerInteraction.Collide);
            HashSet<HighflyHealth> unique = new HashSet<HighflyHealth>();
            for (int i = 0; i < count; i++)
            {
                HighflyHealth target = _hits[i].GetComponentInParent<HighflyHealth>();
                if (target != null && target != resources.Health && unique.Add(target))
                    Damage(target, 45f, true, "echo");
            }
        }

        public void Tempestad()
        {
            if (!TryCast(3, 40f, _nextU3)) return;
            _nextU3 = Time.time + 52f;
            if (animator != null) animator.SetTrigger("Skill3");
            StartCoroutine(TempestSequence());
        }

        private IEnumerator TempestSequence()
        {
            _busy = true;
            int ticks = Mathf.Max(1, tempestHits);
            float tickSeconds = tempestDuration / ticks;
            for (int tick = 0; tick < ticks; tick++)
            {
                yield return new WaitForSeconds(tickSeconds);
                if (effects != null) effects.Tempestad(transform.position, tempestRadius, tick + 1);
                int count = Physics.OverlapSphereNonAlloc(transform.position, tempestRadius, _hits, enemyMask, QueryTriggerInteraction.Collide);
                HashSet<HighflyHealth> unique = new HashSet<HighflyHealth>();
                for (int i = 0; i < count; i++)
                {
                    HighflyHealth target = _hits[i].GetComponentInParent<HighflyHealth>();
                    if (target == null || !unique.Add(target) || !target.IsAlive) continue;
                    State st = Read(target);
                    bool tripleMark = st.Marks >= 3 && Time.time < st.MarksUntil;
                    bool wasAirborne = Time.time < st.AirborneUntil;
                    if (!Damage(target, tempestDamage / ticks, true, "tempestad")) continue;
                    if (tripleMark)
                    {
                        st.Marks = 0;
                        st.MarksUntil = 0f;
                        _states[target] = st;
                        Lift(target, Mathf.Max(.48f, (ticks - tick) * tickSeconds + .16f));
                    }
                    else if (wasAirborne && st.Juggles < 8)
                    {
                        st.Juggles++;
                        _states[target] = st;
                        Lift(target, .23f);
                    }
                }
            }
            _busy = false;
            FlushBuffer();
        }

        private void OnDisable()
        {
            if (motor != null) motor.ExternalMovementLock = false;
        }

        private void FlushBuffer()
        {
            int command = _bufferedSkill;
            Vector3 direction = _bufferedDirection;
            _bufferedSkill = 0;
            if (command == 1) CastPrisma(direction);
            if (command == 2) CastRuptura(direction);
            if (command == 3) Tempestad();
        }

        private State Read(HighflyHealth health)
        {
            State st;
            if (!_states.TryGetValue(health, out st)) st = new State();
            if (Time.time > st.MarksUntil) st.Marks = 0;
            return st;
        }

        private void Mark(HighflyHealth target)
        {
            if (target == null || !target.IsAlive) return;
            State st = Read(target);
            st.Marks = Mathf.Min(3, st.Marks + 1);
            st.MarksUntil = Time.time + 5f;
            _states[target] = st;
            if (effects != null) effects.Mark(target.transform.position, st.Marks);
        }

        private void Lift(HighflyHealth target, float seconds)
        {
            if (target == null || !target.IsAlive) return;
            HighflySimpleEnemyAI ai = target.GetComponent<HighflySimpleEnemyAI>();
            // Bosses are never airborne: damage their poise, not their airborne state.
            if (ai != null && ai.IsBoss)
            {
                ai.ApplyAirborne(seconds);
                if (effects != null) effects.Impact(target.transform.position, .9f);
                return;
            }
            State st = Read(target);
            st.AirborneUntil = Mathf.Max(st.AirborneUntil, Time.time + seconds);
            _states[target] = st;
            if (ai != null) ai.ApplyAirborne(seconds);
            if (effects != null) effects.Lift(target.transform.position);
        }

        private bool Damage(HighflyHealth target, float amount, bool gain, string source)
        {
            if (target == null || !target.IsAlive) return false;
            State st = Read(target);
            if (Time.time < st.AirborneUntil) amount *= 1.25f;
            if (!target.ApplyDamage(amount)) return false;
            if (gain) GainMagic(source == "tempestad" ? 10f : 34f);
            return true;
        }

        private void GainMagic(float points) { SuperMagic = Mathf.Clamp(SuperMagic + points, 0f, 100f); }

        private HighflyHealth FindNearestTarget(Vector3 origin, float maxDistance, Vector3 forward, float halfAngle)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, maxDistance, _hits, enemyMask, QueryTriggerInteraction.Collide);
            HighflyHealth best = null;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i];
                if (col == null) continue;
                HighflyHealth health = col.GetComponentInParent<HighflyHealth>();
                if (health == null || !health.IsAlive || health == resources.Health) continue;
                Vector3 toward = health.transform.position - origin;
                toward.y = 0f;
                if (toward.sqrMagnitude < .001f || Vector3.Angle(forward, toward) > halfAngle) continue;
                if (toward.sqrMagnitude < bestDistance)
                {
                    best = health;
                    bestDistance = toward.sqrMagnitude;
                }
            }
            return best;
        }
    }
}
