using UnityEngine;

namespace Highfly.Combat
{
    public sealed class HighflyTargetable : MonoBehaviour
    {
        [SerializeField] private Transform aimPoint;
        [SerializeField] private HighflyHealth health;
        [SerializeField] private int faction = 1;
        [SerializeField] private float priorityBias;

        public Transform AimPoint => aimPoint != null ? aimPoint : transform;
        public HighflyHealth Health => health;
        public int Faction => faction;
        public float PriorityBias => priorityBias;
        public bool IsAlive => health == null || health.IsAlive;

        private void Awake()
        {
            if (health == null)
                health = GetComponentInParent<HighflyHealth>();
        }
    }
}
