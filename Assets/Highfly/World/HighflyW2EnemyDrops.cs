using UnityEngine;
using Highfly.Combat;

namespace Highfly.World
{
    /// <summary>
    /// Monster-death bridge: one event per death, credits an accepted quest
    /// and leaves a collectible item. No modification to FURY or hit damage.
    /// </summary>
    [RequireComponent(typeof(HighflyHealth))]
    public sealed class HighflyW2EnemyDrops : MonoBehaviour
    {
        [SerializeField] private string dropItemId = "bone_fragment";
        [SerializeField] private int dropCount = 1;
        [SerializeField] private int gold = 12;
        private HighflyHealth _health;
        private bool _awarded;

        private void Awake()
        {
            _health = GetComponent<HighflyHealth>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_health != null) _health.Died -= OnDied;
        }

        private void OnDied(HighflyHealth health)
        {
            if (_awarded) return;
            _awarded = true;
            HighflyW2Progress progress = FindObjectOfType<HighflyW2Progress>();
            if (progress != null) progress.OnSkeletonDefeated();
            HighflyW2LootPickup.Spawn(transform.position + Vector3.up * .65f,
                dropItemId, dropCount, gold);
        }
    }
}
