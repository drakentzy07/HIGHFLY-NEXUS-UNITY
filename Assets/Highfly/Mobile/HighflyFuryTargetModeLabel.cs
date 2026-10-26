using UnityEngine;
using UnityEngine.UI;
using Highfly.Combat;

namespace Highfly.Mobile
{
    public sealed class HighflyFuryTargetModeLabel : MonoBehaviour
    {
        [SerializeField] private HighflyTargetingSystem targeting;
        [SerializeField] private Text label;
        private void LateUpdate()
        {
            if (targeting == null || label == null) return;
            if (targeting.Mode == HighflyTargetingSystem.TargetPriority.Nearest)
                label.text = "OBJETIVO\nCERCANO";
            else if (targeting.Mode == HighflyTargetingSystem.TargetPriority.LowestHealth)
                label.text = "OBJETIVO\nMENOS HP";
            else
                label.text = "OBJETIVO\nAUTO";
        }
    }
}
