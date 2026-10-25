using UnityEngine;
using UnityEngine.UI;
using Highfly.Combat;

namespace Highfly.UI
{
    /// <summary>Passive charge readout for the isolated Violet kit.</summary>
    public sealed class HighflyVioletSuperMagicHUD : MonoBehaviour
    {
        [SerializeField] private HighflyVioletCombat violet;
        [SerializeField] private Text readout;
        private void Update()
        {
            if (violet == null || readout == null) return;
            readout.text = violet.SuperMagic >= 100f
                ? "SUPER MAGIC 100%  ·  ATK POTENCIADO"
                : "SUPER MAGIC  " + Mathf.RoundToInt(violet.SuperMagic) + "%";
            readout.color = violet.SuperMagic >= 100f
                ? new Color(1f, .84f, .96f, 1f)
                : new Color(.79f, .60f, 1f, 1f);
        }
    }
}
