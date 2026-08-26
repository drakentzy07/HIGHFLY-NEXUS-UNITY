using UnityEngine;
using Highfly.Core;
using Highfly.Combat;

namespace Highfly.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class HighflyZonePortal : MonoBehaviour
    {
        [SerializeField] private Vector3 destination;
        [SerializeField] private Vector3 facing = Vector3.forward;
        [SerializeField] private bool restoreResources;
        [SerializeField] private float cooldown = 1.0f;

        private static float _globalNextUse;

        public void Configure(Vector3 targetPosition, Vector3 targetFacing, bool restore)
        {
            destination = targetPosition;
            facing = targetFacing.sqrMagnitude > 0.001f ? targetFacing.normalized : Vector3.forward;
            restoreResources = restore;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Time.time < _globalNextUse)
                return;

            Transform actor = other.transform;
            CharacterController controller = other.GetComponent<CharacterController>();
            if (controller == null)
                controller = other.GetComponentInParent<CharacterController>();

            if (controller == null || !controller.CompareTag("Player"))
                return;

            _globalNextUse = Time.time + cooldown;

            controller.enabled = false;
            controller.transform.position = destination;
            controller.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            controller.enabled = true;

            HighflyWorldSafety safety = controller.GetComponent<HighflyWorldSafety>();
            if (safety != null)
                safety.SetRespawnPosition(destination);

            if (restoreResources)
            {
                HighflyPlayerResources resources = controller.GetComponent<HighflyPlayerResources>();
                if (resources != null)
                    resources.RestoreAll();
            }
        }
    }
}
