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
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                HighflyAnimationBalance balance = visual.GetComponent<HighflyAnimationBalance>();
                if (balance == null) balance = visual.AddComponent<HighflyAnimationBalance>();
                balance.Configure(animator, player.GetComponent<CharacterController>());

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
                Debug.Log("FURY RIG DIAG visual=" + visual.name +
                    " avatar=" + animator.avatar.name +
                    " scale=" + visual.transform.localScale +
                    " sourceHeight=" + (found ? bounds.size.y : 0f) +
                    " controllerHeight=" + player.GetComponent<CharacterController>().height);
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
            // Prefer in-place takes. Root motion stays owned by FURY CharacterController.
            var assets = AssetDatabase.FindAssets("t:AnimationClip",
                new[] { Animations, Animations2 })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !Path.GetFileNameWithoutExtension(p).EndsWith("_RM",
                    StringComparison.OrdinalIgnoreCase))
                .Distinct().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
            var clips = assets.SelectMany(p => AssetDatabase.LoadAllAssetsAtPath(p)
                .OfType<AnimationClip>()).Where(c => c != null &&
                c.isHumanMotion && !c.name.StartsWith("__preview__"))
                .GroupBy(c => c.name).Select(g => g.First()).ToArray();
            // These names were verified against the REAL CC0 UAL1 Standard FBX.
            // Loose "Idle" had selected Crouch_Idle_Loop: the Hunter crouched!
            AnimationClip idle = PickExact(clips, "Idle_Loop");
            AnimationClip walk = PickExact(clips, "Walk_Loop");
            AnimationClip jog = PickExact(clips, "Jog_Fwd_Loop");
            AnimationClip sprint = PickExact(clips, "Sprint_Loop");
            AnimationClip jumpStart = PickExact(clips, "Jump_Start");
            AnimationClip jumpLoop = PickExact(clips, "Jump_Loop");
            AnimationClip jumpLand = PickExact(clips, "Jump_Land");
            AnimationClip attack = PickExact(clips, "Sword_Attack");
            AnimationClip magic = PickExact(clips, "Spell_Simple_Shoot");
            AnimationClip dodge = PickExact(clips, "Sword_Dash") ??
                PickExact(clips, "Roll");
            if (idle == null || walk == null || jog == null || sprint == null ||
                jumpStart == null || jumpLoop == null || jumpLand == null ||
                attack == null)
                throw new InvalidOperationException(
                    "FURY UAL1 required upright gait/jump clips missing; " +
                    "refusing Crouch_Idle or Sword_Idle fallback.");

            AssetDatabase.DeleteAsset(ControllerPath);
            AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            animator.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
            animator.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            animator.AddParameter("ComboIndex", AnimatorControllerParameterType.Int);
            animator.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
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
            moveBlend.AddChild(walk, .43f);
            moveBlend.AddChild(jog, .73f);
            moveBlend.AddChild(sprint, 1f);
            AnimatorState locomotionState = locomotion.AddState("FURY_Move_360");
            locomotionState.motion = moveBlend;
            locomotion.defaultState = locomotionState;

            // Original controller exposed jump triggers but NO corresponding
            // animation states. Drive true takeoff -> air loop -> landing.
            AnimatorState leap = locomotion.AddState("FURY_Jump_Start");
            leap.motion = jumpStart;
            AnimatorState aerial = locomotion.AddState("FURY_Jump_Air");
            aerial.motion = jumpLoop;
            AnimatorState landing = locomotion.AddState("FURY_Jump_Land");
            landing.motion = jumpLand;
            AnimatorState flipLeap = locomotion.AddState("FURY_DoubleJump_Start");
            flipLeap.motion = jumpStart;
            AnimatorStateTransition begin = locomotion.AddAnyStateTransition(leap);
            begin.hasExitTime = false; begin.duration = .06f;
            begin.canTransitionToSelf = false;
            begin.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            AnimatorStateTransition beginDouble = locomotion.AddAnyStateTransition(flipLeap);
            beginDouble.hasExitTime = false; beginDouble.duration = .05f;
            beginDouble.canTransitionToSelf = false;
            beginDouble.AddCondition(AnimatorConditionMode.If, 0f, "DoubleJump");
            AnimatorStateTransition leapToAir = leap.AddTransition(aerial);
            leapToAir.hasExitTime = true; leapToAir.exitTime = .75f;
            leapToAir.duration = .07f;
            AnimatorStateTransition flipToAir = flipLeap.AddTransition(aerial);
            flipToAir.hasExitTime = true; flipToAir.exitTime = .75f;
            flipToAir.duration = .07f;
            AnimatorStateTransition airToLand = aerial.AddTransition(landing);
            airToLand.hasExitTime = false; airToLand.duration = .08f;
            airToLand.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");
            AnimatorStateTransition landToMove = landing.AddTransition(locomotionState);
            landToMove.hasExitTime = true; landToMove.exitTime = .58f;
            landToMove.duration = .07f;
            // DASH owns a short full-body state, not a torso-only mask over
            // running legs. In-place Sword_Dash preserves FURY physics.
            AnimatorState dashBody = locomotion.AddState("FURY_Dash_Body");
            dashBody.motion = dodge ?? jog;
            dashBody.speed = dodge != null ?
                Mathf.Clamp(dodge.length / .22f, .85f, 5f) : 1.5f;
            Debug.Log("FURY DASH CLIP " + (dodge != null ? dodge.name : jog.name) +
                " clipSeconds=" + (dodge != null ? dodge.length : jog.length) +
                " playbackSpeed=" + dashBody.speed);
            AnimatorStateTransition dashEnter = locomotion.AddAnyStateTransition(dashBody);
            dashEnter.hasExitTime = false;
            dashEnter.duration = .04f;
            dashEnter.canTransitionToSelf = false;
            dashEnter.AddCondition(AnimatorConditionMode.If, 0f, "Dash");
            AnimatorStateTransition dashLeave = dashBody.AddTransition(locomotionState);
            dashLeave.hasExitTime = true;
            dashLeave.exitTime = .82f;
            dashLeave.duration = .09f;
            Debug.Log("FURY POSTURE VERIFIED upright=" + idle.name +
                " walk=" + walk.name + " jog=" + jog.name +
                " sprint=" + sprint.name + " jump=" + jumpStart.name +
                "/" + jumpLoop.name + "/" + jumpLand.name);

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
                // Empty FreeUpper at weight 1 overrides the running upper body
                // with an unanimated bind pose (the "open legs / floating" look).
                // The runtime driver enables the layer ONLY for action states.
                avatarMask = mask, defaultWeight = 0f,
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
            // Never select Sword_Idle as an ATK finisher: it is a pose!
            AnimationClip[] legacyStrikes =
                { PickExact(clips, "Punch_Jab"), PickExact(clips, "Punch_Cross"),
                  PickExact(clips, "Sword_Attack"), PickExact(clips, "Sword_Attack") };
            AnimationClip[] newStrikes =
                { PickExact(clips, "Sword_Regular_A"),
                  PickExact(clips, "Sword_Regular_B"),
                  PickExact(clips, "Sword_Regular_C"),
                  PickExact(clips, "Sword_Heavy_Combo") };
            bool ual2Ready = newStrikes.All(c => c != null);
            // Four distinct visual phases when the free CC0 pack has loaded.
            float[] comboSpeeds = { 1.15f, 1.2f, 1.1f, 1.18f };
            for (int i = 0; i < 4; i++)
            {
                AnimationClip clip = ual2Ready ? newStrikes[i] : legacyStrikes[i];
                if (clip == null) throw new InvalidOperationException("FURY ATK clip missing stage " + i);
                AddCast(sm, neutral, "ATK_" + (i + 1), clip,
                    "BasicAttack" + (i + 1), comboSpeeds[i], false);
                Debug.Log("FURY COMBO STAGE " + (i + 1) + " = " + clip.name +
                    (ual2Ready ? " UAL2 Standard CC0" : " UAL1 fallback"));
            }
            AddCast(sm, neutral, "ATK_Legacy", attack, "BasicAttack");
            AddCast(sm, neutral, "Heavy",
                PickExact(clips, "Sword_Heavy_Combo") ?? attack, "HeavyAttack");
            AddCast(sm, neutral, "Prisma", magic ?? attack, "Skill1");
            AddCast(sm, neutral, "Ruptura", attack, "Skill2");
            AddCast(sm, neutral, "Tempestad", magic ?? attack, "Skill3");
            AssetDatabase.SaveAssets();
            return animator;
        }

        private static string ClipName(AnimationClip clip)
        {
            string name = clip.name;
            int separator = name.LastIndexOf('|');
            return separator < 0 ? name : name.Substring(separator + 1);
        }

        private static AnimationClip PickExact(AnimationClip[] clips, string take)
        {
            return clips.FirstOrDefault(c =>
                string.Equals(ClipName(c), take, StringComparison.OrdinalIgnoreCase));
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
