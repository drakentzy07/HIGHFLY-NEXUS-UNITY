#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;
using Highfly.Mobile;
using Highfly.Combat;

namespace Highfly.Editor
{
    /// <summary>FURY touch bindings; jump button does not replace camera/joystick.</summary>
    public static class HighflyFuryMobileHud
    {
        public static void Install(GameObject canvas, HighflyThirdPersonMotor motor,
            HighflyGuinevereCombat combat)
        {
            if (canvas == null || motor == null || combat == null)
                return;
            Transform existing = canvas.transform.Find("FURY_JUMP");
            if (existing != null) return;
            GameObject go = new GameObject("FURY_JUMP",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = new Vector2(-805f, 520f);
            rect.sizeDelta = new Vector2(168f, 168f);
            Image img = go.GetComponent<Image>();
            img.color = new Color(.13f, .25f, .45f, .90f);
            Button button = go.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, motor.Jump);

            GameObject label = new GameObject("Label",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(go.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            Text txt = label.GetComponent<Text>();
            txt.text = "SALTAR\n×2 MORTAL";
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 24;
            txt.fontStyle = FontStyle.Bold;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.raycastTarget = false;

            GameObject mode = new GameObject("FURY_Mode",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            mode.transform.SetParent(canvas.transform, false);
            RectTransform modeRect = mode.GetComponent<RectTransform>();
            modeRect.anchorMin = modeRect.anchorMax = new Vector2(1f, 1f);
            modeRect.pivot = new Vector2(.5f, .5f);
            modeRect.anchoredPosition = new Vector2(-416f, -124f);
            modeRect.sizeDelta = new Vector2(900f, 54f);
            Text status = mode.GetComponent<Text>();
            status.text = "FURY 360° 2.0  ·  CÁMARA LIBRE  ·  COMBOS AÉREOS";
            status.color = new Color(.75f, .90f, 1f);
            status.alignment = TextAnchor.MiddleCenter;
            status.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            status.fontSize = 20;
            status.raycastTarget = false;

            // Tap to switch between smart / nearest / lowest HP soft aim.
            HighflyTargetingSystem targeting = combat.GetComponent<HighflyTargetingSystem>();
            if (targeting != null)
            {
                GameObject target = new GameObject("FURY_TargetMode",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
                    typeof(Button), typeof(HighflyFuryTargetModeLabel));
                target.transform.SetParent(canvas.transform, false);
                RectTransform tRect = target.GetComponent<RectTransform>();
                tRect.anchorMin = tRect.anchorMax = new Vector2(1f, 1f);
                tRect.pivot = new Vector2(.5f, .5f);
                tRect.anchoredPosition = new Vector2(-220f, -318f);
                tRect.sizeDelta = new Vector2(270f, 88f);
                target.GetComponent<Image>().color = new Color(.14f, .17f, .31f, .90f);
                UnityEventTools.AddPersistentListener(
                    target.GetComponent<Button>().onClick, targeting.CyclePriority);
                GameObject textGo = new GameObject("Label",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textGo.transform.SetParent(target.transform, false);
                RectTransform tLabel = textGo.GetComponent<RectTransform>();
                tLabel.anchorMin = Vector2.zero; tLabel.anchorMax = Vector2.one;
                tLabel.offsetMin = tLabel.offsetMax = Vector2.zero;
                Text t = textGo.GetComponent<Text>();
                t.text = "OBJETIVO\nAUTO";
                t.alignment = TextAnchor.MiddleCenter;
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                t.fontSize = 18;
                t.fontStyle = FontStyle.Bold;
                t.color = Color.white;
                t.raycastTarget = false;
                SerializedObject ts = new SerializedObject(target.GetComponent<HighflyFuryTargetModeLabel>());
                ts.FindProperty("targeting").objectReferenceValue = targeting;
                ts.FindProperty("label").objectReferenceValue = t;
                ts.ApplyModifiedPropertiesWithoutUndo();
            }

            GameObject cancelText = new GameObject("FURY_CancelHint",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            cancelText.transform.SetParent(canvas.transform, false);
            RectTransform cancelRect = cancelText.GetComponent<RectTransform>();
            cancelRect.anchorMin = cancelRect.anchorMax = new Vector2(1f, 1f);
            cancelRect.anchoredPosition = new Vector2(-175f, -61f);
            cancelRect.sizeDelta = new Vector2(305f, 53f);
            Text cancel = cancelText.GetComponent<Text>();
            cancel.text = "✕ CANCELAR SKILL";
            cancel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            cancel.fontSize = 20;
            cancel.fontStyle = FontStyle.Bold;
            cancel.alignment = TextAnchor.MiddleCenter;
            cancel.color = new Color(1f, .57f, .66f, 1f);
            cancel.raycastTarget = false;

            // PC/editor can test the same input without a phone.
            if (canvas.GetComponent<HighflyFuryKeyboardInput>() == null)
            {
                HighflyFuryKeyboardInput keyboard = canvas.AddComponent<HighflyFuryKeyboardInput>();
                SerializedObject so = new SerializedObject(keyboard);
                so.FindProperty("motor").objectReferenceValue = motor;
                so.FindProperty("combat").objectReferenceValue = combat;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
