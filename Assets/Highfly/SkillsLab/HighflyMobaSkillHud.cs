using UnityEngine;
using UnityEngine.UI;

namespace Highfly.SkillsLab
{
    /// <summary>Visual labels only. Does not cast, spend resources or change cooldowns.</summary>
    public sealed class HighflyMobaSkillHud : MonoBehaviour
    {
        [SerializeField] private HighflyMobaSkillsLab lab;
        [SerializeField] private Text status;
        [SerializeField] private Text[] skillLabels;
        private float _nextRefresh;
        private void Update()
        {
            if (lab == null || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + .12f;
            if (status != null)
                status.text = "HIGHFLY SKILLS LAB P01 · " + lab.StyleName +
                    "\nFURY 360 intacto | S1 S2 S3 | arrastrar, soltar o cancelar";
            if (skillLabels == null) return;
            for (int i = 0; i < Mathf.Min(3, skillLabels.Length); i++)
            {
                Text label = skillLabels[i];
                if (label == null) continue;
                float cd = lab.Cooldown(i + 1);
                string name = lab.SkillName(i + 1);
                if (i == 1 && lab.RecastAvailable) name = "RECAST LISTO";
                label.text = "S" + (i + 1) + "\n" + name +
                    (cd > 0.01f ? "\n" + Mathf.CeilToInt(cd) + "s" : "");
                label.color = cd > 0.01f ? new Color(.75f,.78f,.82f) : Color.white;
            }
        }

        public void Configure(HighflyMobaSkillsLab combat, Text title,
            Text one, Text two, Text three)
        {
            lab = combat; status = title;
            skillLabels = new[] { one, two, three };
        }
    }
}
