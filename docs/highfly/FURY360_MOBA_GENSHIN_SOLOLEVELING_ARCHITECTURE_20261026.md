# FURY 360° — Benchmarks de movilidad y combate (2026-10-26)
**Estado: review-only, no RUN, sin despliegue.** Fuente de verdad: código HIGHFLY + documentación pública sobre referencias de diseño. No se usó código propietario de ninguno de estos juegos.

## Benchmarks verificables
- **Mobile Legends: Bang Bang:** joystick izquierdo + botones derechos, autolock/target selection (product documentation): https://play.google.com/store/apps/details?id=com.mobile.legends and advanced targeting from https://apps.apple.com/id/iphone/story/id1677291177
- **Genshin Impact:** frame-level attack phases startup/hitmark/recovery, animation cancel into dash/jump/skill in many cases, short hitlag for contact feedback: https://library.keqingmains.com/combat-mechanics/frames
- **Solo Leveling: ARISE:** Extreme Evasion -> Shadow Step into the enemy's back; contextual QTE and counterattack: https://ch.netmarble.com/Eng/Games/Detail?bbs_code=1018&post_seq=4686&post_tag=SOLO+LEVELING%3AARISE
- **Unity CharacterController:** one Move returns CollisionFlags; Move itself doesn't apply gravity: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController.Move.html
- **Unity animations:** root Y/XZ/rotation settings and `heightFromFeet` require attention for in-place retargeting: https://docs.unity3d.com/cn/6000.0/ScriptReference/ModelImporterClipAnimation.html

## Gameplay comparison by system
| Axis | MLBB | Genshin | SL ARISE | FURY decision |
|---|---|---|---|---|
| Input | Touch-centric 2-thumb, smart targets | 3D camera-centered | Action contextual QTE | Keep 2-thumb + free right-camera + separate jump/dash; no forced target |
| Ground movement | Responsive heading changes | Analog walk/run, jump/dash exploration | Combat arena gap closers | Single FURY physics owner, analog speed and explicit air inertia |
| Basic attacks | Attack/cast on demand | Startup/active/recovery, cancel windows | Chainers/counters | UAL2 A/B/C + heavy, distinct attack timeline and input buffer |
| Evade | Positioning / mobility skill | Dash/jump cancel recovery, i-frames in some cases | Reward exact dodge with Shadow Step | Dash stays collision-safe; perfect evade only after telegraphed enemies exist |
| Air | Usually 2D MOBA, no conventional platform jumping | Jumps / air traversal | Arena mobility & air combos | FURY jump + directional second jump, momentum preserved |
| Aim | Smart targeting + drag release | Auto target / camera relative attacks | Lock/auto + conditional skills | Camera-relative drag, auto-nearest with manual override |
| Verticality | Primarily flat plane | Fully 3D traversal | Stylized combat mobility | Keep 3D terrain and safe controller; do not reuse 2D movement model |
| Feedback | Readable clear quick skill VFX | Timing and hitlag/hitmark | Evade counters/QTE | Sync actual hit frame, contact VFX + optional light hitstop later |

## What HIGHFLY already had
Unity6 RG POLY City01, Quaternius humanoid CC0, 360° joystick, free camera, two jumps, 4-way procedural flips, basic dash, upper/lower Animator split, four CC0 UAL2 basic strikes, aimed Prisma/Ruptura and AoE Tempestad.

## Code changes in isolated `review/fury360-unified-motion-20261026`
- **Movement:** a single `CharacterController.Move` per frame for all actions (except intentionally explicit fallback in Guinevere, to remove only after compile validation); tuned air inertia, smoother analog run, robust grounded logic, neutral/moving/jumping dash, central pivot flips.
- **Input:** exact pointer ID per joystick, keyboard Shift uses same dash, right-camera remains independent.
- **Actions:** motor arbitrates directional facing, full-body dash state, Ruptura uses motor instead of another per-frame controller.Move; S2 can override normal dash.
- **Animation:** foot calibration after settled idle; Quaternius Standard in-place `_RM` excluded; author `ModelImporterClipAnimation` root XZ/Y/rotation bake and walking loop flags; new clip audit logs.
- **ATK timings:** new serialized `comboCadence` per hit [.38, .38, .42, .62] seconds, `comboImpactDelay` [.13, .14, .16, .24], impact collision and damage at active frame, cancellation of pre-hit attacks on dash/skill, one buffered thumb tap during recovery/dash, clip speed initially calibrated to cadence (must be verified in Unity).
- **No copies of gameplay scripts, animation assets or trademarks from other games; concepts are inspirations.**

## Cancel/priority policy to test
| From | Input | Policy |
|---|---|---|
| Idle / running | Jump, dash, ATK, skill | Execute immediately unless invalid geometry/resource |
| Attack startup (before hit) | Dash or skill | May cancel; pending hit forfeited |
| Attack recovery (after impact) | Dash, jump, next ATK | Cancel or buffer; no ghost damage |
| Dash | ATK | Buffer until exit; no frozen legs |
| Dash | Ruptura | S2 mobility overrides ordinary dash |
| Jump rising / falling | ATK, dash | Allow with same root motion; double jump once only |
| Neutral jump | No direction held | Pure vertical jump is correct |
| Sprint jump | Jump after release stick | Keep horizontal inertia, not instant stop |
| Tempestad channel | Movement and camera | Remain live; no hidden global lock |
| Airborne | Landing | Confirm `CollisionFlags.Below` before resetting jump |

## Pending, not claimed complete
1. **Unity Editor compile** and real S23 QA NOT run by explicit user request.
2. Impact delays are candidate tuning, **not recorded hitmark frames from UAL2**. Animator clip `length` and clip speed must be inspected; repeated heavy hits may need independent active windows.
3. Perfect dodge / Shadow Step requires enemy telegraph and damage hit validation; not implemented as a cosmetic fake.
4. 2D directional strafe tree needs verified side/back walk clips; UAL1/UAL2 samples verified so far do NOT establish those clips. Do not mirror forward locomotion and call it 2D strafe.
5. Foot IK, locomotion stride matching, rig Avatar T-pose and per-clip root/Y settings still need Unity+Android validation; the root import settings could change output unexpectedly and are fully isolated.
6. No extra assets; no Unity RUN; no Pages change until user approval.

## Acceptance gates
- Static: code has one authoritative motor Move; four timed attack steps; no legacy dash binding; root import flags on CC0 animation donors only; all six edited files syntactically balanced.
- Unity next (only with explicit approval): compile + inspect logs for `FURY ROOT BAKE SETTINGS`, `FURY TIMED COMBO` clip lengths, full-body dash, valid Humanoid Avatars.
- S23: original exact sequence from `docs/highfly/FURY360_UNIFIED_MOTION_REVIEW_NO_RUN_20261026.md`, especially sprint-release-jump travel distance, 4-way second jump and strike-hit synchrony.
