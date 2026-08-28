using UnityEngine;
using Highfly.Core;

namespace Highfly.Combat
{
    public sealed class HighflyWorldSafety : MonoBehaviour
    {
        [SerializeField] private float killY = -8f;
        [SerializeField] private Vector3 respawnPosition;
        [SerializeField] private bool restoreResourcesOnRespawn;
        [SerializeField] private bool respawnWhenDead;
        [SerializeField] private float deathRespawnDelay = 1.25f;

        private CharacterController _controller;
        private HighflyHealth _health;
        private HighflyPlayerResources _resources;
        private float _deadSince = -1f;
        private bool _configured;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<HighflyHealth>();
            _resources = GetComponent<HighflyPlayerResources>();
        }

        private void Start()
        {
            if (!_configured)
                respawnPosition = transform.position;
        }

        private void Update()
        {
            if (transform.position.y < killY)
            {
                Respawn();
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
                Respawn();
        }

        public void Configure(Vector3 spawn, float worldKillY, bool restoreResources, bool respawnOnDeath)
        {
            respawnPosition = spawn;
            killY = worldKillY;
            restoreResourcesOnRespawn = restoreResources;
            respawnWhenDead = respawnOnDeath;
            _configured = true;
        }

        public void SetRespawnPosition(Vector3 spawn)
        {
            respawnPosition = spawn;
            _configured = true;
        }

        private void Respawn()
        {
            bool controllerWasEnabled = _controller != null && _controller.enabled;
            if (controllerWasEnabled)
                _controller.enabled = false;

            HighflyThirdPersonMotor motor = GetComponent<HighflyThirdPersonMotor>();
            if (motor != null)
                motor.ResetMotion();

            transform.position = respawnPosition + Vector3.up * 0.12f;
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

            _deadSince = -1f;
        }
    }
}
