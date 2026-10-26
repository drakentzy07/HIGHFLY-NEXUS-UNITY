using UnityEngine;

namespace Highfly.World
{
    // The legal KayKit CC0 sword is attached by the Quaternius editor installer
    // to every valid Humanoid right hand. No extra gameplay/weapon class rig.
    [DisallowMultipleComponent]
    public sealed class HighflyW2WeaponVisual : MonoBehaviour
    {
        private bool _equipped = true;

        public void SetEquipped(bool equipped)
        {
            _equipped = equipped;
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child != null && child.name == "W2_Equipped_Sword")
                    child.gameObject.SetActive(equipped);
            }
        }

        // A restored look can activate a different Quaternius Humanoid variant;
        // embedded equipment in inactive sibling variants follows parent state.
        public void Refresh()
        {
            SetEquipped(_equipped);
        }
    }
}
