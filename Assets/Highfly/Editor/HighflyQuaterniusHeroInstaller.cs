#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using Highfly.Core;
using Highfly.Combat;
using Highfly.World;

namespace Highfly.Editor
{
    /// <summary>
    /// Retargets CC0 Quaternius Humanoid FBX rigs to the FURY controller.
    /// Complete outfit presets are supported without changing the physics root.
    /// Missing/invalid rigs never delete KayKit (safe fallback).
    /// </summary>
    public static class HighflyQuaterniusHeroInstaller
    {
        private const string Root = "Assets/External/Quaternius/Hunters";
        private const string Base = Root + "/Base";
        private const string Outfits = Root + "/Outfits";
        private const string Animations = Root + "/Animations";
        private const string Animations2 = Root + "/Animations2";
        private const string Generated = "Assets/Highfly/Generated/Controllers";
        private const string ControllerPath = Generated + "/Guinevere_Quaternius_FURY.controller";
        private const string MaskPath = Generated + "/Guinevere_UpperBody.mask";

        public static void Install(GameObject player, HighflyGuinevereCombat combat)
        {
            if (!AssetDatabase.IsValidFolder(Base) || !AssetDatabase.IsValidFolder(Animations))
            {
                Debug.LogWarning("Quaternius hunter not staged: retain KayKit until legal CC0 donor arrives.");
                return;
            }
            PrepareHumanoidImports();
            string[] paths = AssetDatabase.FindAssets("t:Model", new[] { Base, Outfits })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                .Distinct().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
            if (paths.Length == 0) return;

            AnimatorController controller = CreateController();
            if (controller == null)
            {
                Debug.LogWarning("Quaternius animation set is missing, keeping animated KayKit.");
                return;
            }

            List<GameObject> variants = new List<GameObject>();
            List<Animator> animators = new List<Animator>();
            foreach (string path in paths)
            {
                string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (!name.Contains("regular") && !name.Contains("knight") &&
                    !name.Contains("wizard") && !name.Contains("rogue") &&
                    !name.Contains("warrior") && !name.Contains("superhero")) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                GameObject visual = PrefabUtility.InstantiatePrefab(prefab, player.transform) as GameObject;
                if (visual == null) continue;
                visual.name = "QUATERNIUS_" + Path.GetFileNameWithoutExtension(path);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.SetActive(true);
                Animator animator = visual.GetComponentInChildren<Animator>();
                if (animator == null) animator = visual.AddComponent<Animator>();
                if (animator.avatar == null || !animator.avatar.isHuman || !animator.avatar.isValid)
                {
                    Debug.LogWarning("Not a valid Humanoid avatar: " + path);
                    UnityEngine.Object.DestroyImmediate(visual);
                    continue;
                }
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;

                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
                Bounds bounds = default(Bounds);
                bool found = false;
                foreach (Renderer renderer in renderers)
                {
                    if (!found) { bounds = renderer.bounds; found = true; }
                    else bounds.Encapsulate(renderer.bounds);
                }
                if (found && bounds.size.y > .1f)
                    visual.transform.localScale *= Mathf.Clamp(1.82f / bounds.size.y, .30f, 4f);
                variants.Add(visual);
                animators.Add(animator);
            }
            if (variants.Count == 0)
            {
                Debug.LogWarning("Quaternius models imported but none has a usable humanoid avatar.");
                return;
            }

            HighflyHunterAppearance appearance = player.AddComponent<HighflyHunterAppearance>();
            SerializedObject so = new SerializedObject(appearance);
            SerializedProperty v = so.FindProperty("variants");
            SerializedProperty a = so.FindProperty("variantAnimators");
            v.arraySize = variants.Count;
            a.arraySize = animators.Count;
            for (int i = 0; i < variants.Count; i++)
            {
                v.GetArrayElementAtIndex(i).objectReferenceValue = variants[i];
                a.GetArrayElementAtIndex(i).objectReferenceValue = animators[i];
                variants[i].SetActive(i == 0);
            }
            so.FindProperty("motor").objectReferenceValue =
                player.GetComponent<HighflyThirdPersonMotor>();
            so.FindProperty("combat").objectReferenceValue = combat;
            so.FindProperty("legacyCombat").objectReferenceValue =
                player.GetComponent<HighflyCombatController>();
            so.ApplyModifiedPropertiesWithoutUndo();

            Transform kayKit = player.transform.Find("Hunter_RogueHooded");
            if (kayKit != null) kayKit.gameObject.SetActive(false);
            AddAppearanceButton(player, appearance);
            Debug.Log("HIGHFLY Quaternius ready: " + variants.Count +
                " switchable hero/outfit presets, FURY rig linked");
        }

        private static void PrepareHumanoidImports()
        {
            foreach (string folder in new[] { Base, Outfits, Animations, Animations2 })
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                string[] files = AssetDatabase.FindAssets("t:Model", new[] { folder })
                    .Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();
                foreach (string path in files)
                {
                    if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                    ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (mi == null) continue;
                    if (mi.animationType == ModelImporterAnimationType.Human) continue;
                    mi.animationType = ModelImporterAnimationType.Human;
                    mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    mi.importAnimation = true;
                    try { mi.SaveAndReimport(); }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Failed Humanoid import: " + path + " / " + ex.Message);
                    }
                }
            }
        }

        private static AnimatorController CreateController()
        {
            if (!Directory.Exists(Generated)) Directory.CreateDirectory(Generated);
            var assets = AssetDatabase.FindAssets("t:AnimationClip",
                new[] { Animations, Animations2 })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();
            var clips = assets.SelectMany(p => AssetDatabase.LoadAllAssetsAtPath(p)
                .OfType<AnimationClip>()).Where(c => c != null &&
                c.isHumanMotion && !c.name.StartsWith("__preview__"))
                .GroupBy(c => c.name).Select(g => g.First()).ToArray();
            AnimationClip idle = Pick(clips, "Idle");
            AnimationClip walk = Pick(clips, "Walking", "Walk");
            AnimationClip run = Pick(clips, "Running", "Run", "Jog", "Walking");
            AnimationClip attack = Pick(clips, "Sword", "Melee", "Punch", "Attack");
            AnimationClip magic = Pick(clips, "Magic", "Cast", "Spell", "Attack");
            AnimationClip dodge = PickExact(clips, "Sword_Dash") ??
                Pick(clips, "Dodge", "Roll", "Run");
            if (idle == null || run == null || attack == null) return null;

            AssetDatabase.DeleteAsset(ControllerPath);
            AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            animator.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            animator.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            animator.AddParameter("ComboIndex", AnimatorControllerParameterType.Int);
            foreach (string trigger in new[] {
                "BasicAttack","BasicAttack1","BasicAttack2","BasicAttack3","BasicAttack4",
                "HeavyAttack","Dash","Skill1","Skill2","Skill3","Jump","DoubleJump"})
                animator.AddParameter(trigger, AnimatorControllerParameterType.Trigger);

            // Blend the whole lower-body locomotion continuously from actual
            // CharacterController planar speed; do not snap Idle <-> full Run.
            AnimatorStateMachine locomotion = animator.layers[0].stateMachine;
            BlendTree moveBlend = new BlendTree {
                name = "FURY_Locomotion_IdleWalkRun",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "MoveSpeed",
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(moveBlend, animator);
            moveBlend.AddChild(idle, 0f);
            moveBlend.AddChild(walk ?? run, .44f);
            moveBlend.AddChild(run, .98f);
            AnimatorState locomotionState = locomotion.AddState("FURY_Move_360");
            locomotionState.motion = moveBlend;
            locomotion.defaultState = locomotionState;
            Debug.Log("FURY locomotion clips: idle=" + idle.name +
                ", walk=" + (walk != null ? walk.name : run.name) +
                ", run=" + run.name);

            AssetDatabase.DeleteAsset(MaskPath);
            AvatarMask mask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
            foreach (AvatarMaskBodyPart part in new[] {
                AvatarMaskBodyPart.Body, AvatarMaskBodyPart.Head,
                AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.RightArm,
                AvatarMaskBodyPart.LeftFingers, AvatarMaskBodyPart.RightFingers})
                mask.SetHumanoidBodyPartActive(part, true);
            AssetDatabase.CreateAsset(mask, MaskPath);

            AnimatorControllerLayer upper = new AnimatorControllerLayer {
                name = "FURY_UpperBody_Cast",
                blendingMode = AnimatorLayerBlendingMode.Override,
                avatarMask = mask, defaultWeight = 1f,
                stateMachine = new AnimatorStateMachine { name = "FURY_UpperStateMachine" }
            };
            AssetDatabase.AddObjectToAsset(upper.stateMachine, animator);
            animator.AddLayer(upper);
            AnimatorStateMachine sm = animator.layers[1].stateMachine;
            AnimatorState neutral = sm.AddState("FreeUpper");
            sm.defaultState = neutral;
            // Real 1-2-3-4 Animator chain. Pick up to four separate CC0 attack
            // clips when the Universal Animation Library supplies them, rather
            // than using one repeated animation for all stages.
            AnimationClip[] attacks = clips.Where(c =>
                new[] { "attack", "sword", "slash", "punch", "melee", "strike", "thrust" }
                .Any(key => c.name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase).ToArray();
            if (attacks.Length == 0) attacks = new[] { attack };
            // UAL2 Standard ACTUAL audited clips. Three separate strikes and
            // recoveries exist in the free archive; fourth uses its heavy combo
            // as a provisional finisher, pending phone timing/VFX review.
            AnimationClip[] provenUAL2 =
            {
                PickExact(clips, "Sword_Regular_A"),
                PickExact(clips, "Sword_Regular_B"),
                PickExact(clips, "Sword_Regular_C"),
                PickExact(clips, "Sword_Heavy_Combo")
            };
            bool ual2Available = provenUAL2[0] != null &&
                provenUAL2[1] != null && provenUAL2[2] != null;
            float[] comboSpeeds = { 1.18f, 1.23f, 1.08f, 1.12f };
            for (int i = 0; i < 4; i++)
            {
                AnimationClip clip = ual2Available ?
                    (provenUAL2[i] ?? provenUAL2[2]) :
                    attacks[i % attacks.Length];
                AddCast(sm, neutral, "ATK_" + (i + 1), clip,
                    "BasicAttack" + (i + 1), comboSpeeds[i], (i & 1) == 1);
                Debug.Log("FURY 4-hit ATK stage " + (i + 1) + ": " +
                    clip.name + (ual2Available ? " [UAL2 Standard CC0]" : " [fallback UAL1]"));
            }
            AddCast(sm, neutral, "ATK_Legacy", attack, "BasicAttack");
            AddCast(sm, neutral, "Heavy", attacks[attacks.Length - 1], "HeavyAttack");
            AddCast(sm, neutral, "Dash", dodge ?? attack, "Dash");
            AddCast(sm, neutral, "Prisma", magic ?? attack, "Skill1");
            AddCast(sm, neutral, "Ruptura", attack, "Skill2");
            AddCast(sm, neutral, "Tempestad", magic ?? attack, "Skill3");
            AssetDatabase.SaveAssets();
            return animator;
        }

        private static AnimationClip PickExact(AnimationClip[] clips, string expected)
        {
            // Unity FBX take names sometimes carry an Armature| prefix.
            return clips.FirstOrDefault(c =>
            {
                string name = c.name;
                int i = name.LastIndexOf('|');
                if (i >= 0) name = name.Substring(i + 1);
                return string.Equals(name, expected, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static AnimationClip Pick(AnimationClip[] clips, params string[] hints)
        {
            foreach (string hint in hints)
            {
                AnimationClip c = clips.FirstOrDefault(x => string.Equals(x.name,
                    hint, StringComparison.OrdinalIgnoreCase));
                if (c != null) return c;
            }
            foreach (string hint in hints)
            {
                AnimationClip c = clips.FirstOrDefault(x => x.name.IndexOf(
                    hint, StringComparison.OrdinalIgnoreCase) >= 0);
                if (c != null) return c;
            }
            return null;
        }

        private static void AddCast(AnimatorStateMachine sm, AnimatorState neutral,
            string name, AnimationClip clip, string trigger,
            float playbackSpeed = 1f, bool mirrored = false)
        {
            AnimatorState state = sm.AddState(name);
            state.motion = clip;
            state.speed = playbackSpeed;
            state.mirror = mirrored;
            AnimatorStateTransition enter = sm.AddAnyStateTransition(state);
            enter.hasExitTime = false;
            enter.duration = .045f;
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            AnimatorStateTransition leave = state.AddTransition(neutral);
            leave.hasExitTime = true;
            leave.exitTime = .78f;
            leave.duration = .08f;
        }

        private static void AddAppearanceButton(GameObject player, HighflyHunterAppearance appearance)
        {
            Canvas canvas = UnityEngine.Object.FindObjectsOfType<Canvas>()
                .FirstOrDefault(c => c.name == "HIGHFLY_MobileHUD");
            if (canvas == null) return;
            GameObject go = new GameObject("FURY_Appearance",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(.5f, .5f);
            rt.anchoredPosition = new Vector2(-230f, -206f);
            rt.sizeDelta = new Vector2(270f, 86f);
            go.GetComponent<Image>().color = new Color(.20f, .15f, .32f, .85f);
            UnityEventTools.AddPersistentListener(go.GetComponent<Button>().onClick, appearance.NextLook);

            GameObject textGo = new GameObject("Text",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            RectTransform tr = textGo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            Text text = textGo.GetComponent<Text>();
            text.text = "CAMBIAR HÉROE / EQUIPO";
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
        }
    }
}
#endif
