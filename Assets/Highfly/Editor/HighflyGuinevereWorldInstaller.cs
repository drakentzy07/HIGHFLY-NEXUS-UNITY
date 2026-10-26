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
            // Make cooldowns visible so testers can distinguish an unavailable
            // skill from a broken touch event. This only updates HUD text.
            HighflyGuinevereCooldownHud cooldowns =
                canvas.GetComponent<HighflyGuinevereCooldownHud>();
            if (cooldowns == null)
                cooldowns = canvas.AddComponent<HighflyGuinevereCooldownHud>();
            cooldowns.Configure(guinevere,
                canvas.transform.Find("S1_CORTE_Button").GetComponentInChildren<Text>(true),
                canvas.transform.Find("S2_ABANICO_Button").GetComponentInChildren<Text>(true),
                canvas.transform.Find("S3_ÁREA_Button").GetComponentInChildren<Text>(true));
            CreateChargeLabel(canvas, guinevere);
            Highfly.Core.HighflyThirdPersonMotor motor =
                player.GetComponent<Highfly.Core.HighflyThirdPersonMotor>();
            HighflyFuryMobileHud.Install(canvas, motor, guinevere);
            InstallGuinevereFxTester(canvas, effects);
            // The original "DASH" button targeted the legacy combat layer.
            // Route both Jump and Dash to the exact same FURY physics motor.
            Transform dashRoot = canvas.transform.Find("DASH_Button");
            if (dashRoot == null) throw new InvalidOperationException("Missing FURY DASH button");
            Button dashButton = dashRoot.GetComponent<Button>();
            if (dashButton == null) throw new InvalidOperationException("No Button on DASH");
            for (int i = dashButton.onClick.GetPersistentEventCount() - 1; i >= 0; --i)
                UnityEventTools.RemovePersistentListener(dashButton.onClick, i);
            UnityEventTools.AddPersistentListener(dashButton.onClick, motor.Dash);
            Debug.Log("FURY INPUT VERIFIED: separate JUMP=Motor.Jump and DASH=Motor.Dash");
            HighflyQuaterniusHeroInstaller.Install(player, guinevere);
            // This beta's character contract is Quaternius CC0, not KayKit.
            // Fail the PREPARATION if legal humanoid rigs are unavailable,
            // instead of silently publishing the historical fallback avatar.
            if (player.GetComponent<Highfly.World.HighflyHunterAppearance>() == null)
                throw new InvalidOperationException(
                    "FURY preview requires Quaternius Humanoid CC0; KayKit fallback is not permitted.");
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

        // Small optional S23 test panel; no modifications to the stable CITY01 canvas.
        private static void InstallGuinevereFxTester(GameObject canvas, HighflyGuinevereVfx effects)
        {
            if (canvas.GetComponent<HighflyGuinevereFxControls>() != null) return;
            HighflyGuinevereFxControls panel =
                canvas.AddComponent<HighflyGuinevereFxControls>();
            Text quality = CreateFxTesterButton(canvas.transform, "FURY_FX_QUALITY",
                new Vector2(505f, -346f), "FX EQUIL.",
                panel.CycleQuality);
            Text premium = CreateFxTesterButton(canvas.transform, "FURY_FX_PREMIUM",
                new Vector2(505f, -438f), "R06 ACTIVO",
                panel.TogglePremium);
            GameObject fpsGo = new GameObject("FURY_FX_FPS",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            fpsGo.transform.SetParent(canvas.transform, false);
            RectTransform fpsRect = fpsGo.GetComponent<RectTransform>();
            fpsRect.anchorMin = fpsRect.anchorMax = new Vector2(0f, 1f);
            fpsRect.pivot = new Vector2(.5f, .5f);
            fpsRect.anchoredPosition = new Vector2(505f, -514f);
            fpsRect.sizeDelta = new Vector2(255f, 68f);
            Text fps = fpsGo.GetComponent<Text>();
            fps.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            fps.alignment = TextAnchor.MiddleCenter;
            fps.fontSize = 20;
            fps.color = Color.white;
            fps.raycastTarget = false;
            panel.Configure(effects, quality, premium, fps);
        }

        private static Text CreateFxTesterButton(Transform parent, string name,
            Vector2 at, string initial, UnityAction click)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = new Vector2(255f, 80f);
            go.GetComponent<Image>().color = new Color(.18f, .13f, .27f, .89f);
            UnityEventTools.AddPersistentListener(
                go.GetComponent<Button>().onClick, click);
            GameObject label = new GameObject("Label",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(go.transform, false);
            RectTransform tRect = label.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = tRect.offsetMax = Vector2.zero;
            Text text = label.GetComponent<Text>();
            text.text = initial;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
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
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
