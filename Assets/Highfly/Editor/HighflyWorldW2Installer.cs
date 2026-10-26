#if UNITY_EDITOR
using System;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Highfly.World;
using Highfly.UI;

namespace Highfly.Editor
{
    public static class HighflyWorldW2Installer
    {
        public static void Install(GameObject player, GameObject canvas)
        {
            if (player == null || canvas == null)
                throw new ArgumentNullException("W2 requires Hunter and HUD");
            HighflyHunterAppearance appearance =
                player.GetComponent<HighflyHunterAppearance>();
            if (appearance == null)
                throw new InvalidOperationException("W2 needs existing legal Quaternius skins");
            HighflyW2WeaponVisual weapon = player.GetComponent<HighflyW2WeaponVisual>();
            if (weapon == null) weapon = player.AddComponent<HighflyW2WeaponVisual>();
            HighflyW2Progress state = player.GetComponent<HighflyW2Progress>();
            if (state == null) state = player.AddComponent<HighflyW2Progress>();

            HighflyW2Menu menu = canvas.GetComponent<HighflyW2Menu>();
            if (menu != null) return;
            menu = canvas.AddComponent<HighflyW2Menu>();
            Button more = MakeButton(canvas.transform, "W2_MORE", "MÁS ...",
                new Vector2(.5f, 1f), new Vector2(0f, -72f),
                new Vector2(250f, 75f));
            UnityEventTools.AddPersistentListener(more.onClick, menu.Toggle);

            GameObject panel = MakeBox(canvas.transform, "W2_MAIN_PANEL",
                new Vector2(.5f, .5f), Vector2.zero,
                new Vector2(1540f, 810f),
                new Color(.045f, .064f, .12f, .97f));
            Text title = MakeText(panel.transform, "W2_Title",
                new Vector2(0f, 337f), new Vector2(1100f, 65f), 36);
            title.fontStyle = FontStyle.Bold;
            Text detail = MakeText(panel.transform, "W2_Detail",
                new Vector2(0f, -15f), new Vector2(1300f, 380f), 30);
            detail.alignment = TextAnchor.UpperLeft;
            Text footer = MakeText(panel.transform, "W2_Status",
                new Vector2(0f, -347f), new Vector2(1280f, 56f), 22);
            footer.text = "HIGHFLY WORLD W2 · progreso local automático";

            AddButton(panel.transform, "W2_Close", "X", new Vector2(676f, 344f),
                new Vector2(80f, 70f), menu.Close);
            string[] tabs = { "PERSONAJE", "MOCHILA", "EQUIPO", "MISIONES", "TRAINING" };
            UnityAction[] actions = {
                menu.Personaje, menu.Inventario, menu.Equipo,
                menu.Misiones, menu.Entrenamiento
            };
            for (int i = 0; i < tabs.Length; i++)
                AddButton(panel.transform, "W2_Tab_" + i, tabs[i],
                    new Vector2(-550f + i * 275f, 263f),
                    new Vector2(245f, 75f), actions[i]);

            AddButton(panel.transform, "W2_EquipSword", "EQUIPAR ESPADA",
                new Vector2(-570f, -266f), new Vector2(260f, 75f),
                menu.EquipSword);
            AddButton(panel.transform, "W2_UnequipSword", "QUITAR ESPADA",
                new Vector2(-280f, -266f), new Vector2(260f, 75f),
                menu.UnequipSword);
            AddButton(panel.transform, "W2_EquipArmor", "EQUIPAR ARMADURA",
                new Vector2(20f, -266f), new Vector2(280f, 75f),
                menu.EquipArmor);
            AddButton(panel.transform, "W2_NextSkin", "SKIN QUATERNIUS",
                new Vector2(320f, -266f), new Vector2(280f, 75f),
                menu.NextLook);
            AddButton(panel.transform, "W2_Save", "GUARDAR",
                new Vector2(615f, -266f), new Vector2(245f, 75f),
                menu.SaveNow);

            menu.Configure(state, appearance, panel, title, detail, footer);
            // Skin management moves into MAS without deleting existing skin data.
            Transform oldSkinButton = canvas.transform.Find("FURY_Appearance");
            if (oldSkinButton != null) oldSkinButton.gameObject.SetActive(false);
            Debug.Log("HIGHFLY W2: classless menu/inventory/save/skins bound to FURY stable");
        }

        private static GameObject MakeBox(Transform parent, string name,
            Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject go = MakeBox(parent, name, anchor, position, size,
                new Color(.13f, .20f, .35f, .98f));
            Button button = go.AddComponent<Button>();
            Text text = MakeText(go.transform, name + "_Text", Vector2.zero,
                size, 24);
            text.fontStyle = FontStyle.Bold;
            text.text = label;
            text.raycastTarget = false;
            return button;
        }

        private static void AddButton(Transform parent, string name, string title,
            Vector2 at, Vector2 size, UnityAction action)
        {
            Button button = MakeButton(parent, name, title,
                new Vector2(.5f, .5f), at, size);
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static Text MakeText(Transform parent, string name,
            Vector2 position, Vector2 size, int fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}
#endif
