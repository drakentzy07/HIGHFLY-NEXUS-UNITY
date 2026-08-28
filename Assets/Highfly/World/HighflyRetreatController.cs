using UnityEngine;
using Highfly.Combat;
using Highfly.Core;

namespace Highfly.World
{
    public sealed class HighflyRetreatController : MonoBehaviour
    {
        [SerializeField] private Vector3 cityDestination;
        [SerializeField] private Vector3 cityFacing = Vector3.forward;
        [SerializeField] private float cooldown = 1.5f;

        private float _nextUse;

        public void Configure(Vector3 destination, Vector3 facing)
        {
            cityDestination = destination;
            cityFacing = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector3.forward;
        }

        public void Retreat()
        {
            if (Time.time < _nextUse)
                return;

            _nextUse = Time.time + cooldown;

            CharacterController controller = GetComponent<CharacterController>();
            HighflyThirdPersonMotor motor = GetComponent<HighflyThirdPersonMotor>();
            HighflyPlayerResources resources = GetComponent<HighflyPlayerResources>();
            HighflyWorldSafety safety = GetComponent<HighflyWorldSafety>();

            if (motor != null)
                motor.ResetMotion();

            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
                controller.enabled = false;

            transform.position = cityDestination + Vector3.up * 0.12f;
            transform.rotation = Quaternion.LookRotation(cityFacing, Vector3.up);
            Physics.SyncTransforms();

            if (wasEnabled)
                controller.enabled = true;

            if (resources != null)
                resources.RestoreAll();

            if (safety != null)
            {
                safety.SetRespawnPosition(cityDestination);
                safety.SetDeathRespawnPosition(cityDestination);
            }

            Debug.Log("HIGHFLY: retirada ejecutada. Regreso seguro a ciudad.");
        }
    }
}
