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

            // Serin is the first real W2 quest giver. Legacy NPC dialogue
            // remains intact for all other town residents and services.
            if (string.Equals(displayName, "Serin",
                System.StringComparison.OrdinalIgnoreCase))
            {
                HighflyW2Progress progress =
                    controller.GetComponent<HighflyW2Progress>();
                if (progress != null)
                {
                    controller.ShowDialogue(displayName,
                        dialogue + "\\n\\n" + progress.TalkToSerin());
                    return;
                }
            }
            controller.ShowDialogue(displayName, dialogue);
        }
    }
}
