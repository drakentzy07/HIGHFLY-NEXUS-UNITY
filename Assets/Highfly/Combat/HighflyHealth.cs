using System;
using UnityEngine;

namespace Highfly.Combat
{
    public sealed class HighflyHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath;
        [SerializeField] private float destroyDelay = 2f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        public float Normalized => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;

        public event Action<float, float> HealthChanged;
        public event Action<HighflyHealth> Died;

        private void Awake()
        {
            CurrentHealth = Mathf.Max(1f, maxHealth);
        }

        public bool ApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return false;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
            {
                Died?.Invoke(this);
                if (destroyOnDeath)
                    Destroy(gameObject, destroyDelay);
            }

            return true;
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = Mathf.Max(1f, maxHealth);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}
