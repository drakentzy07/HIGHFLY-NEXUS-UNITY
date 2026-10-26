using UnityEngine;
using Highfly.Combat;

namespace Highfly.World
{
    /// <summary>
    /// In-world loot, deliberately interactable rather than instantly granted.
    /// Inventory saves when the Hunter actually picks the reward up.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public sealed class HighflyW2LootPickup : HighflyInteractable
    {
        [SerializeField] private string itemId = "bone_fragment";
        [SerializeField] private int quantity = 1;
        [SerializeField] private int gold;
        private Transform _visual;

        public static void Spawn(Vector3 at, string id, int count, int money)
        {
            GameObject root = new GameObject("W2_LOOT_" + id);
            root.transform.position = at;
            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.1f;
            HighflyW2LootPickup pickup = root.AddComponent<HighflyW2LootPickup>();
            pickup.Configure(id, count, money);

            // Simple ORIGINAL Unity temporary pickup marker (not a paid asset).
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "W2_Loot_Marker";
            orb.transform.SetParent(root.transform, false);
            orb.transform.localScale = Vector3.one * .35f;
            Collider c = orb.GetComponent<Collider>();
            if (c != null) { c.enabled = false; Destroy(c); }
            Renderer renderer = orb.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (renderer != null && shader != null)
            {
                Material mat = new Material(shader);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", new Color(1f, .78f, .25f, 1f));
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", new Color(1f, .78f, .25f, 1f));
                renderer.material = mat;
            }
            pickup._visual = orb.transform;
            Destroy(root, 120f); // Only uncollected world drops expire.
        }

        public void Configure(string id, int count, int money)
        {
            itemId = id;
            quantity = Mathf.Max(0, count);
            gold = Mathf.Max(0, money);
            HighflyW2Progress.Item item = HighflyW2Progress.FindItem(id);
            displayName = string.IsNullOrEmpty(item.Name) ? "Botín" : item.Name;
            prompt = "RECOGER";
        }

        private void Update()
        {
            if (_visual != null)
                _visual.localPosition = Vector3.up *
                    (.2f + .09f * Mathf.Sin(Time.time * 2f));
        }

        public override void Interact(HighflyInteractionController controller)
        {
            if (controller == null) return;
            HighflyW2Progress progress = controller.GetComponent<HighflyW2Progress>();
            if (progress == null) return;
            if (quantity > 0) progress.AddItem(itemId, quantity);
            if (gold > 0) progress.AddGold(gold);
            controller.ShowDialogue("BOTÍN RECOGIDO",
                displayName + " x" + quantity + "  ·  +" + gold + " oro");
            Destroy(gameObject);
        }
    }
}
