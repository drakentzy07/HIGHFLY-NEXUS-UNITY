using UnityEngine;

namespace Highfly.World
{
    public sealed class HighflyBuildingDoor : HighflyInteractable
    {
        [SerializeField] private Vector3 destination;
        [SerializeField] private Vector3 facing = Vector3.forward;
        [SerializeField] private bool restoreResources;

        public void Configure(
            string buildingName,
            string actionPrompt,
            Vector3 targetPosition,
            Vector3 targetFacing,
            bool restore)
        {
            displayName = buildingName;
            prompt = actionPrompt;
            destination = targetPosition;
            facing = targetFacing;
            restoreResources = restore;
        }

        public override void Interact(HighflyInteractionController controller)
        {
            if (controller == null)
                return;

            controller.Teleport(destination, facing, restoreResources);
        }
    }
}
