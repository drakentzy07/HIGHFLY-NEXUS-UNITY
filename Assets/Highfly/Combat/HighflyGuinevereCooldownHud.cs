using UnityEngine;
using UnityEngine.UI;

namespace Highfly.Combat
{
    /// <summary>
    /// Clear skill availability feedback in the temporary S23 FURY beta.
    /// Prevents cooldowns (4s / 17s / 52s) from appearing as dead UI buttons.
    /// Never controls casting or physics, only the displayed labels.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyGuinevereCooldownHud : MonoBehaviour
    {
        [SerializeField] private HighflyGuinevereCombat combat;
        [SerializeField] private Text prisma;
        [SerializeField] private Text ruptura;
        [SerializeField] private Text tempestad;
        private int _old1 = -99, _old2 = -99, _old3 = -99;
        private bool _oldRecast;

        public void Configure(HighflyGuinevereCombat source, Text skill1,
            Text skill2, Text skill3)
        {
            combat = source;
            prisma = skill1;
            ruptura = skill2;
            tempestad = skill3;
            _old1 = _old2 = _old3 = -99;
            Refresh();
        }

        private void Update() { Refresh(); }

        private void Refresh()
        {
            if (combat == null) return;
            int s1 = Mathf.CeilToInt(combat.PrismaCooldown);
            int s2 = Mathf.CeilToInt(combat.RupturaCooldown);
            int s3 = Mathf.CeilToInt(combat.TempestadCooldown);
            bool recast = combat.CanRupturaRecast;
            if (prisma != null && _old1 != s1)
                prisma.text = s1 > 0 ? "S1\nPRISMA\n" + s1 + "s" : "S1\nPRISMA";
            if (ruptura != null && (_old2 != s2 || _oldRecast != recast))
                ruptura.text = recast ? "S2\nRECAST" :
                    s2 > 0 ? "S2\nRUPTURA\n" + s2 + "s" : "S2\nRUPTURA";
            if (tempestad != null && _old3 != s3)
                tempestad.text = s3 > 0 ? "S3\nTEMPESTAD\n" + s3 + "s" :
                    "S3\nTEMPESTAD";
            _old1 = s1; _old2 = s2; _old3 = s3; _oldRecast = recast;
        }
    }
}
