using System;
using UnityEngine;
using Highfly.Combat;

namespace Highfly.Core
{
    public sealed class HighflyPlayerResources : MonoBehaviour
    {
        [SerializeField] private HighflyHealth health;
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float manaRegenPerSecond = 6f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaRegenPerSecond = 22f;
        [SerializeField] private float staminaRegenDelay = 0.65f;

        private float _lastStaminaSpendTime = -99f;

        public HighflyHealth Health => health;
        public float MaxMana => maxMana;
        public float Mana { get; private set; }
        public float MaxStamina => maxStamina;
        public float Stamina { get; private set; }

        public event Action ResourcesChanged;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<HighflyHealth>();

            Mana = Mathf.Max(0f, maxMana);
            Stamina = Mathf.Max(0f, maxStamina);
        }

        private void Update()
        {
            bool changed = false;

            if (Mana < maxMana)
            {
                Mana = Mathf.Min(maxMana, Mana + manaRegenPerSecond * Time.deltaTime);
                changed = true;
            }

            if (Time.time - _lastStaminaSpendTime >= staminaRegenDelay && Stamina < maxStamina)
            {
                Stamina = Mathf.Min(maxStamina, Stamina + staminaRegenPerSecond * Time.deltaTime);
                changed = true;
            }

            if (changed)
                ResourcesChanged?.Invoke();
        }

        public bool TrySpendMana(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (Mana + 0.001f < amount)
                return false;

            Mana -= amount;
            ResourcesChanged?.Invoke();
            return true;
        }

        public bool TrySpendStamina(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (Stamina + 0.001f < amount)
                return false;

            Stamina -= amount;
            _lastStaminaSpendTime = Time.time;
            ResourcesChanged?.Invoke();
            return true;
        }

        public void RestoreAll()
        {
            Mana = maxMana;
            Stamina = maxStamina;
            if (health != null)
                health.ResetHealth();
            ResourcesChanged?.Invoke();
        }
    }
}
