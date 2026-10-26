using UnityEngine;
using UnityEngine.UI;
using Highfly.Combat;

namespace Highfly.UI
{
    /// <summary>Passive charge readout for the isolated Guinevere kit.</summary>
    public sealed class HighflyGuinevereSuperMagicHUD : MonoBehaviour
    {
        [SerializeField] private HighflyGuinevereCombat guinevere;
        [SerializeField] private Text readout;
        private void Update()
        {
            if (guinevere == null || readout == null) return;
            readout.text = guinevere.SuperMagic >= 100f
                ? "GUINEVERE · SUPER MAGIC 100% · ATK POTENCIADO"
                : "GUINEVERE · SUPER MAGIC  " + Mathf.RoundToInt(guinevere.SuperMagic) + "%";
            readout.color = guinevere.SuperMagic >= 100f
                ? new Color(1f, .84f, .96f, 1f)
                : new Color(.79f, .60f, 1f, 1f);
        }
    }
}
