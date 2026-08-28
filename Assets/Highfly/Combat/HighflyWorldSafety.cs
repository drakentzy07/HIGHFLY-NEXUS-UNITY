using UnityEngine;
using Highfly.Core;

namespace Highfly.Combat
{
    public sealed class HighflyWorldSafety : MonoBehaviour
    {
        [SerializeField] private float killY = -8f;
        [SerializeField] private Vector3 respawnPosition;
        [SerializeField] private Vector3 deathRespawnPosition;
        [SerializeField] private bool restoreResourcesOnRespawn;
        [SerializeField] private bool respawnWhenDead;
        [SerializeField] private float deathRespawnDelay = 1.25f;
        [SerializeField] private float deathGoldPenaltyPercent = 0.05f;

        private CharacterController _controller;
        private HighflyHealth _health;
        private HighflyPlayerResources _resources;
        private HighflyHunterProgression _progression;
        private float _deadSince = -1f;
        private bool _configured;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<HighflyHealth>();
            _resources = GetComponent<HighflyPlayerResources>();
            _progression = GetComponent<HighflyHunterProgression>();
        }

        private void Start()
        {
            if (!_configured)
            {
                respawnPosition = transform.position;
                deathRespawnPosition = transform.position;
            }
        }

        private void Update()
        {
            if (transform.position.y < killY)
            {
                Respawn(respawnPosition, false);
                return;
            }

            if (!respawnWhenDead || _health == null)
                return;

            if (_health.IsAlive)
            {
                _deadSince = -1f;
                return;
            }

            if (_deadSince < 0f)
                _deadSince = Time.time;

            if (Time.time - _deadSince >= deathRespawnDelay)
                Respawn(deathRespawnPosition, true);
        }

        public void Configure(Vector3 spawn, float worldKillY, bool restoreResources, bool respawnOnDeath)
        {
            respawnPosition = spawn;
            deathRespawnPosition = spawn;
            killY = worldKillY;
            restoreResourcesOnRespawn = restoreResources;
            respawnWhenDead = respawnOnDeath;
            _configured = true;
        }

        public void SetRespawnPosition(Vector3 spawn)
        {
            // Zone portals update fall recovery only. Combat death keeps the
            // original safe-city home established by Configure().
            respawnPosition = spawn;
            _configured = true;
        }

        public void SetDeathRespawnPosition(Vector3 spawn)
        {
            deathRespawnPosition = spawn;
            _configured = true;
        }

        private void Respawn(Vector3 targetPosition, bool fromCombatDeath)
        {
            bool controllerWasEnabled = _controller != null && _controller.enabled;
            if (controllerWasEnabled)
                _controller.enabled = false;

            HighflyThirdPersonMotor motor = GetComponent<HighflyThirdPersonMotor>();
            if (motor != null)
                motor.ResetMotion();

            transform.position = targetPosition + Vector3.up * 0.12f;
            Physics.SyncTransforms();

            if (controllerWasEnabled)
                _controller.enabled = true;

            if (restoreResourcesOnRespawn && _resources != null)
                _resources.RestoreAll();
            else if (_health != null)
                _health.ResetHealth();

            HighflySimpleEnemyAI ai = GetComponent<HighflySimpleEnemyAI>();
            if (ai != null)
                ai.enabled = true;

            if (fromCombatDeath && _progression != null && _progression.Gold > 0)
            {
                int penalty = Mathf.Max(1, Mathf.CeilToInt(_progression.Gold * Mathf.Clamp01(deathGoldPenaltyPercent)));
                int lost = _progression.LoseGold(penalty);
                if (lost > 0)
                    Debug.Log("HIGHFLY derrota: -" + lost + " oro. Nivel, rango y stats conservados.");
            }

            _deadSince = -1f;
        }
    }
}
