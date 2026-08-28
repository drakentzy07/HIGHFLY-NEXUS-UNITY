using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;
using Highfly.Combat;

namespace Highfly.UI
{
    public sealed class HighflyCombatHUD : MonoBehaviour
    {
        [SerializeField] private HighflyPlayerResources resources;
        [SerializeField] private HighflyHunterProgression progression;
        [SerializeField] private HighflyTargetingSystem targeting;
        [SerializeField] private Text rankText;
        [SerializeField] private Text hpText;
        [SerializeField] private Text mpText;
        [SerializeField] private Text staminaText;
        [SerializeField] private Text targetText;
        [SerializeField] private Text staminaMiniText;
        [SerializeField] private Image hpFill;
        [SerializeField] private Image mpFill;
        [SerializeField] private Image staminaFill;

        private void Awake()
        {
            if (progression == null && resources != null)
                progression = resources.GetComponent<HighflyHunterProgression>();
        }

        private void Update()
        {
            if (resources != null)
            {
                HighflyHealth health = resources.Health;

                if (hpText != null && health != null)
                    hpText.text = string.Format("HP  {0:0} / {1:0}", health.CurrentHealth, health.MaxHealth);

                if (mpText != null)
                    mpText.text = string.Format("MP  {0:0} / {1:0}", resources.Mana, resources.MaxMana);

                if (hpFill != null && health != null)
                    hpFill.fillAmount = health.Normalized;

                if (mpFill != null)
                    mpFill.fillAmount = resources.MaxMana <= 0f ? 0f : resources.Mana / resources.MaxMana;

                if (staminaMiniText != null)
                    staminaMiniText.text = string.Format("STA  {0:0} / {1:0}", resources.Stamina, resources.MaxStamina);
            }

            if (progression != null)
            {
                if (rankText != null)
                {
                    rankText.text =
                        "CAZADOR  •  RANGO " + progression.Rank +
                        "  •  LV. " + progression.Level.ToString("00") +
                        "  •  " + progression.Gold + " G" +
                        "  •  LLAVES " + progression.GateKeys;
                }

                if (staminaText != null)
                {
                    staminaText.text = progression.Level >= 100
                        ? "XP  NIVEL MÁXIMO"
                        : "XP  " + progression.Experience + " / " + progression.ExperienceToNextLevel;
                }

                if (staminaFill != null)
                    staminaFill.fillAmount = progression.ExperienceNormalized;
            }

            if (targetText != null)
            {
                if (targeting != null && targeting.HasTarget)
                    targetText.text = "AUTO TARGET  •  " + targeting.CurrentTarget.name;
                else
                    targetText.text = "AUTO TARGET  •  BUSCANDO";
            }
        }
    }
}
