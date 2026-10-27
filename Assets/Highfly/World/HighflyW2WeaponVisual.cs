using UnityEngine;

namespace Highfly.World
{
    /// <summary>
    /// One visible weapon only. The source Quaternius outfit can contain
    /// decorative embedded swords/bows; disable their renderers, not bones.
    /// The classless inventory exclusively controls W2_Equipped_Sword.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HighflyW2WeaponVisual : MonoBehaviour
    {
        private bool _equipped = true;
        private static readonly string[] EmbeddedGearHints = {
            "sword", "dagger", "axe", "mace", "hammer", "spear",
            "staff", "wand", "bow", "crossbow", "quiver", "shield"
        };

        private void Start() { Refresh(); }

        public void SetEquipped(bool equipped)
        {
            _equipped = equipped;
            Refresh();
        }

        public void Refresh()
        {
            HighflyHunterAppearance looks = GetComponent<HighflyHunterAppearance>();
            if (looks == null) return;
            Transform[] transforms = looks.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; ++i)
            {
                Transform node = transforms[i];
                if (node == null) continue;
                if (node.name == "W2_Equipped_Sword")
                {
                    node.gameObject.SetActive(_equipped);
                    continue;
                }
                // Never remove our equipped sword's renderers/child meshes.
                bool ownedWeapon = false;
                Transform cursor = node.parent;
                while (cursor != null && cursor != transform)
                {
                    if (cursor.name == "W2_Equipped_Sword")
                    {
                        ownedWeapon = true;
                        break;
                    }
                    cursor = cursor.parent;
                }
                if (ownedWeapon) continue;
                string lower = node.name.ToLowerInvariant();
                bool legacy = false;
                foreach (string hint in EmbeddedGearHints)
                    if (lower.Contains(hint)) { legacy = true; break; }
                if (!legacy) continue;
                Renderer renderer = node.GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }
            // Original KayKit sword was attached under the hidden Rogue mesh;
            // enforce its off state even if an appearance rebind toggles parents.
            Transform legacySword = transform.Find("Hunter_RogueHooded");
            if (legacySword != null) legacySword.gameObject.SetActive(false);
        }
    }
}
