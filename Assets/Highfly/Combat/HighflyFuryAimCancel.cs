using UnityEngine;

namespace Highfly.Combat
{
    /// <summary>
    /// Cross-finger MOBA cancellation: press X while dragging an aim skill.
    /// The original aim press will NOT cast when its finger is released.
    /// This only cancels preview/input; it cannot rewind an already cast skill.
    /// </summary>
    public sealed class HighflyFuryAimCancel : MonoBehaviour
    {
        public void CancelAim()
        {
            HighflyGuinevereAimButton.CancelActiveAim();
        }
    }
}
