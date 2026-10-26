using System;
using UnityEngine;

namespace Highfly.Combat
{
    public sealed class HighflyHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath;
        [SerializeField] private float destroyDelay = 2f;

        private float _equipmentHealthBonus;
        public float MaxHealth => maxHealth + _equipmentHealthBonus;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        public float Normalized => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;

        public event Action<float, float> HealthChanged;
        public event Action<HighflyHealth> Died;

        private void Awake()
        {
            CurrentHealth = Mathf.Max(1f, MaxHealth);
        }

        public bool ApplyDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return false;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0f)
            {
                Died?.Invoke(this);
                if (destroyOnDeath)
                    Destroy(gameObject, destroyDelay);
            }

            return true;
        }

        // Equipment vitality changes actual max HP, never the visible skin.
        public void SetEquipmentHealthBonus(float amount)
        {
            float previousMax = MaxHealth;
            _equipmentHealthBonus = Mathf.Max(0f, amount);
            float delta = MaxHealth - previousMax;
            CurrentHealth = Mathf.Clamp(CurrentHealth + delta, 0f, MaxHealth);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return;

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = Mathf.Max(1f, MaxHealth);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
