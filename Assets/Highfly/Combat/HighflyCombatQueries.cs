using System.Collections.Generic;
using UnityEngine;

namespace Highfly.Combat
{
    public static class HighflyCombatQueries
    {
        private static readonly Collider[] Buffer = new Collider[96];
        private static readonly HashSet<HighflyHealth> UniqueHealth = new HashSet<HighflyHealth>();

        public static int DamageArc(Vector3 origin, Vector3 forward, float radius, float halfAngle, LayerMask mask, float damage)
        {
            UniqueHealth.Clear();
            int count = Physics.OverlapSphereNonAlloc(origin, radius, Buffer, mask, QueryTriggerInteraction.Collide);
            int hits = 0;

            for (int i = 0; i < count; i++)
            {
                Collider col = Buffer[i];
                if (col == null)
                    continue;

                HighflyHealth health = col.GetComponentInParent<HighflyHealth>();
                if (health == null || !health.IsAlive || !UniqueHealth.Add(health))
                    continue;

                Vector3 to = health.transform.position - origin;
                to.y = 0f;
                if (to.sqrMagnitude > 0.001f && Vector3.Angle(forward, to) > halfAngle)
                    continue;

                if (health.ApplyDamage(damage))
                    hits++;
            }

            return hits;
        }

        public static int DamageLine(Vector3 start, Vector3 direction, float length, float radius, LayerMask mask, float damage)
        {
            UniqueHealth.Clear();
            direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
            Vector3 end = start + direction * Mathf.Max(0.1f, length);
            int count = Physics.OverlapCapsuleNonAlloc(start, end, Mathf.Max(0.05f, radius), Buffer, mask, QueryTriggerInteraction.Collide);
            int hits = 0;

            for (int i = 0; i < count; i++)
            {
                Collider col = Buffer[i];
                if (col == null)
                    continue;

                HighflyHealth health = col.GetComponentInParent<HighflyHealth>();
                if (health == null || !health.IsAlive || !UniqueHealth.Add(health))
                    continue;

                if (health.ApplyDamage(damage))
                    hits++;
            }

            return hits;
        }

        public static int DamageArea(Vector3 center, float radius, LayerMask mask, float damage)
        {
            UniqueHealth.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, Buffer, mask, QueryTriggerInteraction.Collide);
            int hits = 0;

            for (int i = 0; i < count; i++)
            {
                Collider col = Buffer[i];
                if (col == null)
                    continue;

                HighflyHealth health = col.GetComponentInParent<HighflyHealth>();
                if (health == null || !health.IsAlive || !UniqueHealth.Add(health))
                    continue;

                if (health.ApplyDamage(damage))
                    hits++;
            }

            return hits;
        }
    }
}
