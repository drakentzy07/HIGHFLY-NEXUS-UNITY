using UnityEngine;

namespace Highfly.World
{
    public sealed class HighflyNpcInteractable : HighflyInteractable
    {
        [TextArea(2, 6)]
        [SerializeField] private string dialogue = "Bienvenido a HIGHFLY.";
        [SerializeField] private bool restoreAllOnInteract;

        public void Configure(string npcName, string npcPrompt, string message, bool restoreAll)
        {
            displayName = npcName;
            prompt = npcPrompt;
            dialogue = message;
            restoreAllOnInteract = restoreAll;
        }

        public override void Interact(HighflyInteractionController controller)
        {
            if (controller == null)
                return;

            if (restoreAllOnInteract)
                controller.RestoreAll();

            controller.ShowDialogue(displayName, dialogue);
        }
    }
}
