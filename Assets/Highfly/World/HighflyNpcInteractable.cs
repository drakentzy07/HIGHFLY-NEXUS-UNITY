using UnityEngine;
using Highfly.Core;

namespace Highfly.World
{
    public sealed class HighflyNpcInteractable : HighflyInteractable
    {
        private const string ArchiveRewardKey = "HIGHFLY_ARCHIVE_INTRO_REWARD";

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

            HighflyHunterProgression progression = controller.GetComponent<HighflyHunterProgression>();
            string body = dialogue;

            HighflyContractJournal journal = controller.GetComponent<HighflyContractJournal>();

            if (displayName == "Serin" && journal != null)
            {
                body += "\n\n" + journal.InteractWithSerin();
            }

            if (progression != null)
            {
                if (displayName == "Herrero Kael")
                {
                    progression.TryUpgradeWeapon(out string result);
                    body += "\n\n" + result +
                            "\nForja actual: +" + progression.ForgeLevel +
                            "  •  Poder x" + progression.EquipmentPowerMultiplier.ToString("0.00");
                }
                else if (displayName == "Mercader Nia")
                {
                    const int keyCost = 125;
                    progression.TryBuyGateKey(keyCost, out string result);
                    body += "\n\n" + result;
                }
                else if (displayName == "Erudita Lyra")
                {
                    if (PlayerPrefs.GetInt(ArchiveRewardKey, 0) == 0)
                    {
                        progression.AddExperience(40);
                        PlayerPrefs.SetInt(ArchiveRewardKey, 1);
                        PlayerPrefs.Save();
                        body += "\n\nREGISTRO NUEVO: Esqueletos de la Cripta F. +40 XP.";
                    }
                    else
                    {
                        body += "\n\nBestiario de la Cripta F ya registrado.";
                    }
                }
            }

            controller.ShowDialogue(displayName, body);
        }
    }
}
