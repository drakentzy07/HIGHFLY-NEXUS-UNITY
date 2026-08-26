using UnityEngine;

namespace Highfly.World
{
    public abstract class HighflyInteractable : MonoBehaviour
    {
        [SerializeField] protected string prompt = "INTERACTUAR";
        [SerializeField] protected string displayName = "OBJETO";

        public virtual string Prompt => prompt;
        public virtual string DisplayName => displayName;
        public virtual Vector3 InteractionPoint => transform.position + Vector3.up;

        public abstract void Interact(HighflyInteractionController controller);
    }
}
