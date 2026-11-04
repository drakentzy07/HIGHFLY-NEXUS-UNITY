#if UNITY_EDITOR
using System;
using System.IO;
using Highfly.Combat;
using Highfly.Core;
using Highfly.SkillsLab;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Highfly.Editor
{
    /// <summary>
    /// Copy-on-write scene installer. No modifications to CITY01, stable
    /// scenes, main branch, player motor, or the existing R06 scripts.
    /// </summary>
    public static class HighflyMobaLabInstaller
    {
        private const string LabDir = "Assets/Highfly/SkillsLab/Scenes";
        private const string LabScene = LabDir + "/HIGHFLY_MOBA_P01.unity";

        [MenuItem("HIGHFLY/Skills Lab P01/Crear escena de pruebas aislada")]
        public static void CreateIsolatedScene()
        {
            Scene original = SceneManager.GetActiveScene();
            if (!original.IsValid() || string.IsNullOrEmpty(original.path))
            {
                Debug.LogError("MOBA P01: open a SAVED FURY + Guinevere scene first.");
                return;
            }
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                Debug.LogError("MOBA P01: first save/discard edits to your SOURCE scene. Nothing was modified.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LabScene) != null)
            {
                Debug.LogWarning("MOBA P01 already exists. Refusing to overwrite it. Open " + LabScene);
                return;
            }

            Directory.CreateDirectory(LabDir);
            AssetDatabase.Refresh();
            if (!AssetDatabase.CopyAsset(original.path, LabScene))
            {
                Debug.LogError("MOBA P01: could not create isolated scene copy.");
                return;
            }

            Scene clone = EditorSceneManager.OpenScene(LabScene, OpenSceneMode.Single);
            HighflyThirdPersonMotor motor =
                UnityEngine.Object.FindFirstObjectByType<HighflyThirdPersonMotor>(
                    FindObjectsInactive.Include);
            if (motor == null)
            {
                Debug.LogError("MOBA P01: FURY motor missing from source scene; test scene copy retained for inspection.");
                return;
            }
            if (EventSystem.current == null)
            {
                Debug.LogError("MOBA P01: source scene has no EventSystem; keep its existing mobile input setup.");
                return;
            }

            GameObject player = motor.gameObject;
            HighflyMobaSkillFx fx = player.GetComponent<HighflyMobaSkillFx>();
            if (fx == null) fx = player.AddComponent<HighflyMobaSkillFx>();
            HighflyMobaSkillsLab lab = player.GetComponent<HighflyMobaSkillsLab>();
            if (lab == null) lab = player.AddComponent<HighflyMobaSkillsLab>();

            // Disable only legacy Guinevere controls IN THE COPIED SCENE.
            // FURY movement, camera, jump, joystick, dash and ATK stay intact.
            HighflyGuinevereCombat previous = player.GetComponent<HighflyGuinevereCombat>();
            if (previous != null) previous.enabled = false;
            Button[] existing = UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button b in existing)
            {
                if (b == null) continue;
                string n = b.gameObject.name;
                if (n == "S1_CORTE_Button" || n == "S2_ABANICO_Button" ||
                    n == "S3_ÁREA_Button")
                    b.interactable = false;
            }

            InstallHud(lab, fx, Camera.main);
            EditorSceneManager.MarkSceneDirty(clone);
            EditorSceneManager.SaveScene(clone);
            AssetDatabase.SaveAssets();
            Debug.Log("MOBA P01 created ONLY at " + LabScene +
                " | player " + player.name + " | FURY unchanged.");
        }

        private static void InstallHud(HighflyMobaSkillsLab lab,
            HighflyMobaSkillFx fx, Camera gameplayCamera)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject uiRoot = new GameObject("HIGHFLY_MOBA_LAB_P01_HUD",
                typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = uiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 200;

            CanvasScaler scale = uiRoot.GetComponent<CanvasScaler>();
            scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution = new Vector2(2400f, 1080f);
            scale.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scale.matchWidthOrHeight = .5f;

            // All UI is Spanish and intentionally transparent outside buttons.
            Text status = CreateText(uiRoot.transform, "LAB_STATUS",
                new Vector2(.5f, 1f), new Vector2(0f, -58f),
                new Vector2(1220f, 118f), "HIGHFLY SKILLS LAB", font, 32);
            Text instruction = CreateText(uiRoot.transform, "LAB_HELP",
                new Vector2(.5f, 0f), new Vector2(0f, 52f),
                new Vector2(1030f, 55f),
                "JOYSTICK + CAMARA | S1 / S2 / S3 | DESLIZAR PARA APUNTAR",
                font, 19);
            instruction.color = new Color(.72f, .86f, .97f);

            CreateButton(uiRoot.transform, "MODO_MAGICO", "AEREA",
                new Vector2(1f, 1f), new Vector2(-580f, -76f),
                new Vector2(170f, 86f), font, new Color(.37f,.17f,.64f,.95f),
                lab.SelectAerial);
            CreateButton(uiRoot.transform, "MODO_FISICO", "ABISAL",
                new Vector2(1f, 1f), new Vector2(-390f, -76f),
                new Vector2(170f, 86f), font, new Color(.65f,.16f,.27f,.95f),
                lab.SelectRift);
            CreateButton(uiRoot.transform, "MODO_CONTROL", "GUARDIAN",
                new Vector2(1f, 1f), new Vector2(-200f, -76f),
                new Vector2(170f, 86f), font, new Color(.50f,.40f,.17f,.95f),
                lab.SelectGravity);

            var cancel = uiRoot.AddComponent<HighflyMobaCancelAim>();
            CreateButton(uiRoot.transform, "CANCELAR_APUNTADO", "CANCELAR",
                new Vector2(1f, 1f), new Vector2(-190f, -177f),
                new Vector2(190f, 68f), font, new Color(.38f,.22f,.24f,.92f),
                cancel.CancelAim);
            CreateButton(uiRoot.transform, "CALIDAD_VFX", "VFX / CALIDAD",
                new Vector2(1f, 1f), new Vector2(-395f, -177f),
                new Vector2(192f, 68f), font, new Color(.13f,.25f,.31f,.92f),
                fx.CycleQuality);

            Button s1 = CreateButton(uiRoot.transform, "MOBA_S1",
                "S1", new Vector2(1f, 0f), new Vector2(-610f, 138f),
                new Vector2(185f, 153f), font, new Color(.36f,.23f,.68f,.96f),
                lab.SkillOne);
            Button s2 = CreateButton(uiRoot.transform, "MOBA_S2",
                "S2", new Vector2(1f, 0f), new Vector2(-400f, 138f),
                new Vector2(185f, 153f), font, new Color(.32f,.26f,.59f,.96f),
                lab.SkillTwo);
            Button s3 = CreateButton(uiRoot.transform, "MOBA_S3",
                "S3", new Vector2(1f, 0f), new Vector2(-190f, 138f),
                new Vector2(185f, 153f), font, new Color(.52f,.32f,.21f,.96f),
                lab.SkillThree);

            s1.gameObject.AddComponent<HighflyMobaAimButton>()
                .Configure(lab, gameplayCamera, 1, 15f);
            s2.gameObject.AddComponent<HighflyMobaAimButton>()
                .Configure(lab, gameplayCamera, 2, 8f);
            s3.gameObject.AddComponent<HighflyMobaAimButton>()
                .Configure(lab, gameplayCamera, 3, 7f);
            var hud = uiRoot.AddComponent<HighflyMobaSkillHud>();
            hud.Configure(lab, status, s1.GetComponentInChildren<Text>(),
                s2.GetComponentInChildren<Text>(), s3.GetComponentInChildren<Text>());
        }

        private static Button CreateButton(Transform parent, string name,
            string initial, Vector2 anchor, Vector2 position, Vector2 size,
            Font font, Color color, UnityAction action)
        {
            GameObject obj = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = color;
            Button button = obj.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            Text label = CreateText(obj.transform, "LABEL", new Vector2(.5f,.5f),
                Vector2.zero, size, initial, font, 19);
            label.raycastTarget = false;
            return button;
        }

        private static Text CreateText(Transform parent, string name,
            Vector2 anchor, Vector2 position, Vector2 size, string value,
            Font font, int fontSize)
        {
            GameObject obj = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text label = obj.GetComponent<Text>();
            label.font = font;
            label.text = value;
            label.color = Color.white;
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }
    }
}
#endif
