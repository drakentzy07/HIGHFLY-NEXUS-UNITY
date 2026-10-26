#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Highfly.Combat;
using Highfly.UI;

namespace Highfly.Editor
{
    /// <summary>
    /// Installs the standalone Guinevere skill kit into the exact RG POLY
    /// WorldLab scene created by the Unity6 Golden Host build. Never modifies
    /// the original RG POLY demo scene, external packages or live Pages.
    /// </summary>
    public static class HighflyGuinevereWorldInstaller
    {
        public static void Install(GameObject player, GameObject canvas, Camera gameplayCamera)
        {
            if (player == null || canvas == null)
                throw new ArgumentNullException("Guinevere needs a player and HUD");

            HighflyGuinevereVfx effects = player.GetComponent<HighflyGuinevereVfx>();
            if (effects == null) effects = player.AddComponent<HighflyGuinevereVfx>();
            HighflyGuinevereCombat guinevere = player.GetComponent<HighflyGuinevereCombat>();
            if (guinevere == null) guinevere = player.AddComponent<HighflyGuinevereCombat>();

            WireButton(canvas, "ATAQUE_Button", "ATK", guinevere.BasicAttack, null);
            WireButton(canvas, "S1_CORTE_Button", "S1\nPRISMA", guinevere.Prisma, guinevere, gameplayCamera, 1, 15f);
            WireButton(canvas, "S2_ABANICO_Button", "S2\nRUPTURA", guinevere.Ruptura, guinevere, gameplayCamera, 2, 8f);
            WireButton(canvas, "S3_ÁREA_Button", "S3\nTEMPESTAD", guinevere.Tempestad, null);
            CreateChargeLabel(canvas, guinevere);
            Debug.Log("HIGHFLY Guinevere installed on RG POLY CITY01 generated world");
        }

        private static void WireButton(GameObject canvas, string name, string display,
            UnityAction cast, HighflyGuinevereCombat owner,
            Camera camera = null, int slot = 0, float range = 0f)
        {
            Transform candidate = canvas.transform.Find(name);
            if (candidate == null)
                throw new InvalidOperationException("Missing RG POLY action button: " + name);
            Button button = candidate.GetComponent<Button>();
            if (button == null)
                throw new InvalidOperationException("Not a UI Button: " + name);

            // Remove only existing persistent click bindings in this generated
            // scene. Original HUD still exists, but no legacy skill is invoked.
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; --i)
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            UnityEventTools.AddPersistentListener(button.onClick, cast);

            Text label = candidate.GetComponentInChildren<Text>(true);
            if (label != null) label.text = display;
            if (owner != null)
            {
                HighflyGuinevereAimButton aim = candidate.GetComponent<HighflyGuinevereAimButton>();
                if (aim == null) aim = candidate.gameObject.AddComponent<HighflyGuinevereAimButton>();
                aim.Configure(owner, camera, slot, range);
            }
        }

        private static void CreateChargeLabel(GameObject canvas, HighflyGuinevereCombat guinevere)
        {
            GameObject go = new GameObject("Guinevere_SuperMagic", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(canvas.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(-420f, -70f);
            rect.sizeDelta = new Vector2(830f, 64f);
            Text label = go.GetComponent<Text>();
            label.text = "GUINEVERE · SUPER MAGIC 0%";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 24;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(.79f, .60f, 1f, 1f);
            label.raycastTarget = false;
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            HighflyGuinevereSuperMagicHUD hud = go.AddComponent<HighflyGuinevereSuperMagicHUD>();
            SetReference(hud, "guinevere", guinevere);
            SetReference(hud, "readout", label);
        }

        private static void SetReference(UnityEngine.Object component, string field, UnityEngine.Object value)
        {
            SerializedObject so = new SerializedObject(component);
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null) throw new InvalidOperationException("Serialized field missing: " + field);
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
