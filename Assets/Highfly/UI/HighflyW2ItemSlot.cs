using UnityEngine;
using UnityEngine.UI;
using Highfly.World;

namespace Highfly.UI
{
    // Individually clickable RPG backpack slots (not an HTML/text list).
    public sealed class HighflyW2ItemSlot : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private HighflyW2Menu menu;
        [SerializeField] private Image frame;
        [SerializeField] private Text glyph;
        [SerializeField] private Text quantity;
        private static readonly Color Empty = new Color(.10f,.16f,.25f,.92f);
        private static readonly Color Weapon = new Color(.20f,.37f,.58f,.96f);
        private static readonly Color Armor = new Color(.35f,.27f,.56f,.96f);
        private static readonly Color Material = new Color(.24f,.34f,.29f,.96f);

        public void Configure(int index, HighflyW2Menu owner, Image background,
            Text nameGlyph, Text amountLabel)
        {
            slotIndex = index;
            menu = owner;
            frame = background;
            glyph = nameGlyph;
            quantity = amountLabel;
        }

        public void Click()
        {
            if (menu != null) menu.SelectItem(slotIndex);
        }

        public void Present(string itemId, int count)
        {
            HighflyW2Progress.Item item = HighflyW2Progress.FindItem(itemId);
            bool occupied = !string.IsNullOrEmpty(item.Id) && count > 0;
            if (glyph != null)
            {
                glyph.text = !occupied ? "+" :
                    itemId == "sword_rookie" ? "ESPADA" :
                    itemId == "armor_rookie" ? "ARMAD." :
                    itemId == "bone_fragment" ? "HUESO" : "PIEL";
                glyph.color = occupied ? Color.white :
                    new Color(.50f,.58f,.70f,.38f);
            }
            if (quantity != null)
                quantity.text = occupied && count > 1 ? "x" + count : "";
            if (frame != null)
                frame.color = !occupied ? Empty :
                    item.Category == "arma" ? Weapon :
                    item.Category == "armadura" ? Armor : Material;
        }
    }
}
