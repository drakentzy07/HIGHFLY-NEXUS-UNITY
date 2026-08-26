using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;
using Highfly.Combat;

namespace Highfly.UI
{
    public sealed class HighflyCombatHUD : MonoBehaviour
    {
        [SerializeField] private HighflyPlayerResources resources;
        [SerializeField] private HighflyTargetingSystem targeting;
        [SerializeField] private Text hpText;
        [SerializeField] private Text mpText;
        [SerializeField] private Text staminaText;
        [SerializeField] private Text targetText;
        [SerializeField] private Image hpFill;
        [SerializeField] private Image mpFill;
        [SerializeField] private Image staminaFill;

        private void Update()
        {
            if (resources != null)
            {
                HighflyHealth health = resources.Health;
                if (hpText != null && health != null)
                    hpText.text = string.Format("HP  {0:0} / {1:0}", health.CurrentHealth, health.MaxHealth);
                if (mpText != null)
                    mpText.text = string.Format("MP  {0:0} / {1:0}", resources.Mana, resources.MaxMana);
                if (staminaText != null)
                    staminaText.text = string.Format("STA {0:0} / {1:0}", resources.Stamina, resources.MaxStamina);

                if (hpFill != null && health != null)
                    hpFill.fillAmount = health.Normalized;
                if (mpFill != null)
                    mpFill.fillAmount = resources.MaxMana <= 0f ? 0f : resources.Mana / resources.MaxMana;
                if (staminaFill != null)
                    staminaFill.fillAmount = resources.MaxStamina <= 0f ? 0f : resources.Stamina / resources.MaxStamina;
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
