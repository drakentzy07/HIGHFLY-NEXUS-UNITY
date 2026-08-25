using System.Collections.Generic;
using UnityEngine;

namespace Highfly.Combat
{
    /// <summary>
    /// Soft auto-target designed for mobile action combat. The player never
    /// needs to manually select a monster just to use basic attacks.
    /// </summary>
    public sealed class HighflyTargetingSystem : MonoBehaviour
    {
        [SerializeField] private Transform origin;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private float acquireRadius = 12f;
        [SerializeField, Range(10f, 180f)] private float forwardCone = 110f;
        [SerializeField] private float distanceWeight = 1f;
        [SerializeField] private float screenCenterWeight = 4f;
        [SerializeField] private float forwardWeight = 2f;
        [SerializeField] private float refreshInterval = 0.12f;
        [SerializeField] private int hostileFaction = 1;

        private readonly Collider[] _overlap = new Collider[64];
        private readonly HashSet<HighflyTargetable> _unique = new HashSet<HighflyTargetable>();
        private float _nextRefresh;

        public HighflyTargetable CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null && CurrentTarget.IsAlive;

        private void Awake()
        {
            if (origin == null)
                origin = transform;
            if (gameplayCamera == null)
                gameplayCamera = Camera.main;
        }

        private void Update()
        {
            if (Time.time < _nextRefresh)
                return;

            _nextRefresh = Time.time + refreshInterval;

            if (!IsCurrentTargetValid())
                AcquireBestTarget();
            else
            {
                float distance = Vector3.Distance(origin.position, CurrentTarget.AimPoint.position);
                if (distance > acquireRadius * 1.25f)
                    AcquireBestTarget();
            }
        }

        public HighflyTargetable AcquireBestTarget()
        {
            _unique.Clear();
            int count = Physics.OverlapSphereNonAlloc(origin.position, acquireRadius, _overlap, targetMask, QueryTriggerInteraction.Collide);

            HighflyTargetable best = null;
            float bestScore = float.PositiveInfinity;

            for (int i = 0; i < count; i++)
            {
                Collider col = _overlap[i];
                if (col == null)
                    continue;

                HighflyTargetable candidate = col.GetComponentInParent<HighflyTargetable>();
                if (candidate == null || !_unique.Add(candidate) || !candidate.IsAlive || candidate.Faction != hostileFaction)
                    continue;

                Vector3 toTarget = candidate.AimPoint.position - origin.position;
                float distance = toTarget.magnitude;
                if (distance <= 0.001f)
                    continue;

                float angle = Vector3.Angle(origin.forward, toTarget);
                bool inForwardCone = angle <= forwardCone * 0.5f;

                float screenPenalty = 0f;
                if (gameplayCamera != null)
                {
                    Vector3 viewport = gameplayCamera.WorldToViewportPoint(candidate.AimPoint.position);
                    if (viewport.z <= 0f)
                        continue;

                    Vector2 fromCenter = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
                    screenPenalty = fromCenter.sqrMagnitude * screenCenterWeight;
                }

                float conePenalty = inForwardCone ? 0f : forwardWeight;
                float score = (distance / acquireRadius) * distanceWeight + screenPenalty + conePenalty - candidate.PriorityBias;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            CurrentTarget = best;
            return CurrentTarget;
        }

        public HighflyTargetable CycleTarget()
        {
            HighflyTargetable previous = CurrentTarget;
            CurrentTarget = null;
            HighflyTargetable next = AcquireBestTarget();

            if (next == previous)
            {
                // A future pass will add deterministic cycling. For now,
                // reacquisition remains automatic and never blocks attacks.
            }

            return CurrentTarget;
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
        }

        private bool IsCurrentTargetValid()
        {
            return CurrentTarget != null && CurrentTarget.IsAlive;
        }
    }
}
