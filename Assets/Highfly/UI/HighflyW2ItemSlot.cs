using UnityEngine;
using UnityEngine.UI;
using Highfly.World;

namespace Highfly.UI
{
    /// <summary>Native RPG slot, colored rarity frame + owned icon + count.</summary>
    public sealed class HighflyW2ItemSlot : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private HighflyW2Menu menu;
        [SerializeField] private Image frame;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Text quantity;
        [SerializeField] private Sprite swordIcon;
        [SerializeField] private Sprite armorIcon;
        [SerializeField] private Sprite boneIcon;
        [SerializeField] private Sprite hideIcon;
        private readonly Color _empty = new Color(.075f, .105f, .16f, .97f);
        private readonly Color _weapon = new Color(.16f, .27f, .43f, .99f);
        private readonly Color _armor = new Color(.27f, .18f, .36f, .99f);
        private readonly Color _material = new Color(.15f, .23f, .25f, .99f);

        public void Configure(int index, HighflyW2Menu owner, Image background,
            Image icon, Text amount, Sprite sword, Sprite armor,
            Sprite bone, Sprite hide)
        {
            slotIndex = index;
            menu = owner;
            frame = background;
            itemIcon = icon;
            quantity = amount;
            swordIcon = sword;
            armorIcon = armor;
            boneIcon = bone;
            hideIcon = hide;
        }

        public void Click()
        {
            if (menu != null) menu.SelectItem(slotIndex);
        }

        public void Present(string itemId, int count)
        {
            HighflyW2Progress.Item item = HighflyW2Progress.FindItem(itemId);
            bool filled = !string.IsNullOrEmpty(item.Id) && count > 0;
            if (itemIcon != null)
            {
                itemIcon.enabled = filled;
                itemIcon.sprite = itemId == "sword_rookie" ? swordIcon :
                    itemId == "armor_rookie" ? armorIcon :
                    itemId == "bone_fragment" ? boneIcon : hideIcon;
                itemIcon.color = Color.white;
                itemIcon.preserveAspect = true;
            }
            if (quantity != null)
                quantity.text = filled && count > 1 ? "×" + count : "";
            if (frame != null)
            {
                Color baseColor = !filled ? _empty :
                    item.Category == "arma" ? _weapon :
                    item.Category == "armadura" ? _armor : _material;
                frame.color = baseColor;
            }
        }
    }
}
