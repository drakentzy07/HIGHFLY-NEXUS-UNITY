# FURY S23 — WebGL neutral-joystick execution freeze: root-cause repair

## Evidence
- S23 62-second gameplay video from 2026-10-26: apparent frozen movement/airborne poses with left stick centered; moving the stick seems to resume controls.
- Published source `b2af10b3abdcddfe1b3dc7b1cdb7d359ced0c603` used `Input.GetAxisRaw("Horizontal" / "Vertical")` inside `HighflyThirdPersonMotor.Update` **ONLY WHEN** joystick input was neutral.
- The Unity6 scene builder specifically sets up `InputSystemUIInputModule` for a New Input System-only host.
- Legacy `UnityEngine.Input` access under New Input System-only can throw `InvalidOperationException`, aborting the rest of Motor.Update before capsule movement, gravity or Animator parameters. Pressing the joystick bypassed that branch, allowing physics to run again. This is a high-confidence explanation of the observed symptoms; needs WebGL console/runtime confirmation.
- `HighflyFuryKeyboardInput.Update` ALSO called legacy `Input.GetKeyDown` every frame, generating repeated exceptions independently of the joystick.

## New isolated branch
`fix/fury-webgl-new-input-stop-freeze-20261026` is based on the exact compiled GREEN S23 commit `b2af10b3abdcddfe1b3dc7b1cdb7d359ced0c603`.

## Corrective changes
- New-Input-System keyboard polling through `UnityEngine.InputSystem.Keyboard.current`, gated on `ENABLE_INPUT_SYSTEM`.
- Legacy fallback only under `ENABLE_LEGACY_INPUT_MANAGER`; no reads of obsolete legacy APIs in New-Input-System-only builds.
- The motor and keyboard test component both use this isolation.
- Increase `airDeceleration` from 1.6 to 10 to avoid very long midair drifting after joystick release.
- Reset virtual joystick when mobile browser loses focus or pauses (additional resilience, not a substitute for checking lost-pointer release handling).
- Display real S1/S2/S3 cooldown seconds and S2 recast availability to distinguish unavailable skills from missed input.

## Required acceptance test (Unity build + S23, not yet run)
1. Boot and leave joystick neutral for >=10 s. Hunter continues idle animation, gravity/ground contact continue.
2. Jump from rest with no joystick touch; complete takeoff, apex, falling and landing without moving joystick.
3. Double tap while stationary; flip and landing continue without touching stick.
4. Dash and S2 Ruptura from idle; motion runs to completion without joystick.
5. S1 projectile and S3 11 pulse VFX from idle, with cooldown visibly counting down.
6. Press ATK during stationary/running/air states: animation plays at the intended event time.
7. Run diagonally then release joystick in air and on ground: no stuck stick or multi-second drift.
8. Check WebGL console for NO `InvalidOperationException` about old `UnityEngine.Input`; capture error stack if any persist.
9. Preserve independently rendered CITY01, old Guinevere beta and S23 beta. No automatic Pages publication of this review branch.

## Scope limits
- No Wafflus/Genshin source copied, no new premium skills.
- No replacement of Quaternius or FURY physical authority.
- Unity compilation and Android/device behaviour remain unverified until a new RUN and test.
