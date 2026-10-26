# HIGHFLY NEXUS UNITY — FURY Motion/Combat Contract V1 (2026-10-26)

## Scope and protected reference
- Unity 6, Quaternius Humanoid CC0 rig/UAL, RG POLY Medieval environment.
- Starting GREEN reference: `8df089128faf8e563af84bc470b8889b6a042c32`, RUN #40377074812.
- Corrections are on review branch `review/nexus-fury-s23-movement-input-20261026`, separate from published feature/main.
- NO new premium skills, imported VFX, public build or Pages publishing in this review.
- The preview installer now requires Quaternius Humanoid; an absent rig fails preparation instead of silently falling back to KayKit. Any use of KayKit in the older builder remains build-time legacy scaffolding, never the accepted beta character.

## Authority boundaries

| Subsystem | May do | Must NOT do |
| --- | --- | --- |
| `HighflyThirdPersonMotor` | Move the single Hunter CharacterController each frame; gravity, collision, jump, dash, rotation, facing, movement mode | Apply combat damage or spawn skill VFX |
| `HighflyGuinevereCombat` / Combat Core | Spend Mana/Energy, cooldowns, queued action windows, targeting, hit detection, damage, buffs; request FURY movement | Translate Hunter Transform or CharacterController directly |
| `Animator` + `HighflyAnimationBalance` | Humanoid blend tree, upper-body action mask, grounded/air transitions and visual foot offset | Own root XZ/Y motion or lock the player movement by default |
| `HighflyGuinevereVfx` | Create **visual-only** particles, line previews, trails, rings, impact illustrations | Apply damage, add collision impulses, alter Hunter capsule position, select targets or manage cooldowns |
| `HighflyGuinevereAimButton` / `HighflyFuryAimCancel` | Keep drag direction, draw visual preview, release cast request, cancel uncommitted aim | Fire an aimed skill after cancellation or bypass Combat Core |

## Available FURY APIs (current review branch)

### Motion
- `motor.Jump()`: requests first or second jump; can be buffered across short dash. **Void / no success response yet.**
- `motor.Dash()`, `motor.Dash(Vector3 direction)`, `motor.Dash(Vector3 direction, float distance, float duration[, bool playDashAnimation, bool interruptExisting])`: backwards-compatible calls.
- `motor.CanDash(bool interruptExisting = false)`: returns whether current state allows a dash.
- `motor.TryDash(Vector3 direction, float distance, float duration, bool playDashAnimation = true, bool interruptExisting = false)`: bool; one authoritative capsule movement request. Prefer for S2 and future mobility.
- `motor.FaceForAction(Vector3 worldDirection, float seconds = 0.17f)`: temporary facing request; motor handles rotation.
- `motor.ToggleMoveMode()` + `motor.IsRunningMode`: independent WALK/RUN speed selection.

### Read-only phase/feedback
- `motor.IsGrounded`, `motor.IsAirborne`, `motor.CurrentPhase` (Idle/Moving/Rising/Falling/Dashing/Swimming/Wading).
- `motor.IsDashing`, `motor.IsDoingSomersault`, `motor.JumpCount`, `motor.IsInAerialComboWindow`.
- `motor.CurrentPlanarSpeed`, `motor.CurrentVerticalSpeed`, `motor.LastMoveDirection`, `motor.IsMoving`.

### Aim/cancel
- `HighflyGuinevereAimButton.CancelActiveAim()`: cancel currently dragged S1/S2 before release. The upper-right CANCEL button calls through `HighflyFuryAimCancel.CancelAim()`.
- Existing drag/release aim is S1/S2 only; no generic fourth-skill aim API yet.
- Cancellation cannot retroactively undo an already cast skill, damage, or VFX.
- There is **no public `CancelDash` or `TryJump` success-result API yet**; add only after gameplay acceptance tests establish the intended transitions.

### Guinevere combat already implemented
- `BasicAttack()`: four-stage basic combo, delayed impact and aerial damage opportunity.
- `CastPrisma(Vector3 direction)`, `CastRuptura(Vector3 direction)`, `Tempestad()`: existing 3 active abilities (not an implemented fourth skill).
- `CastRuptura` now uses `CanDash/TryDash`, not direct `CharacterController.Move`.
- Current buffer expires after 0.30s; visual/physical responsiveness still requires Android verification.
- S2 actual lift/recall, 11-hit S3, and attack timing still require visual and gameplay QA. Do not claim premium parity.

## Rules for future Guinevere premium and Hayabusa
1. Single physics authority: **no** `transform.position +=`, `Rigidbody.MovePosition`, `CharacterController.Move` or teleport from skill/VFX code on the Hunter. Request `motor.TryDash` (or a later approved `IFuryMotionPort` adapter).
2. S2 must check `CanDash` before resource consumption and `TryDash` result when executing. On denied requests, do not apply a fictitious positional impact.
3. Skill targeting must accept an explicit direction and not silently replace a deliberately aimed miss with nearest-target lock-on. Auto-target is only a tap default.
4. Animator root motion is disabled for playable Hunter. The upper-body layer can play attacks while legs use motion from planar velocity. Animation events/time windows govern hit presentation; gameplay damage stays in Combat Core.
5. No VFX creates physics authority or combat outcome; VFX subscribes to events / positions emitted by Combat Core.
6. Pool repeated particles/trails as needed AFTER profiling WebGL Android; keep a device FPS/frame-time log before attributing lag to allocations.
7. All introduced assets must have verified compatible licenses (CC0 or legally permitted source); don't copy proprietary Mobile Legends clips/models/sounds/effects.
8. Do not import future skill/VFX branch without an explicit diff and integration contract.
9. Do not touch CITY01 or published FURY preview until a reviewed Unity build + S23 verification has passed.

## Pre-GREEN QA requirements
1. **Compilation:** Unity 6 full build without C# errors; confirm Quaternius avatar + UAL animation/controller generated.
2. **Android WebGL:** S23 horizontal, independent two-thumb movement/camera, stationary and running jump, double flip 4 directions, sensible dash end, walk/run toggle.
3. **Combat:** ATK 1-2-3-4 running, in air; S1/S2 drag/release/cancel, S2 recast, S3 11 hits, no queued casts seconds later.
4. **Perf:** frame-time/GC capture during repeated VFX, no apparent input stalls.
5. **Regression:** original stable CITY01/Pages intact, feature branch unchanged.

**Status:** GitHub source patches prepared only; no new Unity or Android validation yet. The GREEN run is the historical baseline, not this review.
