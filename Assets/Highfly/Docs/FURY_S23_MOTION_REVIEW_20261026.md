# FURY 360° / Guinevere — S23 video audit and isolated patch (2026-10-26)

## Provenance and safety
- User-supplied 73-second landscape WebGL recording on an Android phone (portrait-encoded capture).
- Baseline: `feature/nexus-guinevere-rgpoly-unity6-20261026`, commit `8df089128faf8e563af84bc470b8889b6a042c32`, build run #40377074812 GREEN.
- Beta Pages build/publish GREEN does **not** verify control response, frame-time, collision or animations.
- This patch is a separate review branch and **does not touch main, stable City01 or Pages**.

## Findings grounded in current code
1. The motor uses `moveSpeed=5.2`, `sprintSpeed=7.2`, `deceleration=40`. Analog joystick intensity nearly always selects running; no discrete walk/run mode. A zero-input `IsMoving` flag and speed-based animation can disagree transiently.
2. HUD's `FURY_JUMP` previously used `Button.onClick` (fires on release), which makes the jump seem delayed during standing taps even when motor logic is joystick-independent. `ConsumeJumpRequest()` also relies on cached ground or a narrow ground probe.
3. Guinevere's `_busy` blocks subsequent attacks during the entire ~2s Tempestad sequence; a single unbounded skill input buffer could then execute long after the original press.
4. Original legacy `HighflyCombatController` still exists for Heavy/Block while Guinevere replaces ATK + S1-S3 bindings. Competing Animator trigger sources are an architectural risk; keep their integration under review.
5. `HighflyGuinevereVfx` creates/destroys new GameObjects for bursts and rings, a possible source of garbage-collection spikes on Android WebGL; a frame-time profile is required before asserting this is the observed lag.
6. UAL1/UAL2 retargeting is legal CC0 and provides relevant walking, jogging, sprinting and combo clips. There is no evidence of fundamental motor/Quaternius incompatibility.

## Isolated changes in this review
- Add a **walk/run** toggle with explicit mobile button/readout. Retain analog joystick modulation within either selected speed.
- Adjust stop deceleration from 40 to 28 for less abrupt transitions. Test for sliding/foot skating on device.
- Jump on **touch-down** through `HighflyHoldActionButton.OnPressed`, not Unity UI's release-triggered click; preserve independent Space key behavior.
- Broaden near-ground grace probe to 0.22 m, use cached grounded contact in the first jump eligibility check.
- Limit buffered skill actions to 0.30 s; verify HP/Mana/cooldown before queuing.
- Free ability inputs after the first Tempestad tick while continuing its pulse/VFX coroutine.
- Clear pending skill state if the component is disabled.

## Acceptance checklist — must pass before another GREEN claim
1. Stationary **single tap jump** takes off immediately, without joystick movement.
2. Second tap starts the directional flip; releasing the joystick does not stall takeoff.
3. Walking mode clearly slower than running. Button label updates immediately and retains mode until toggled.
4. Joystick release brakes predictably with no sliding, snapping, or frozen pose.
5. Right camera touch never moves the character; left joystick never rotates the camera.
6. Attack 1-2-3-4 on the move: each attack animates and only deals damage on true contact.
7. Attacks while airborne and while dashing remain responsive within deliberately defined windows.
8. Tap skill during Tempestad: no casts **seconds** later; camera/movement remain unlocked.
9. Skill S1/S2 drag, release and cancel work together with right-side multitouch.
10. 60 seconds of repeated skills without visible hitches; measure FPS/frame time and GC rather than infer from recording.
11. Switching character appearance does not leave old animator or flip state active.
12. Beta path stays separate from stable City01; report link and SHA of actually tested build.

## Still open for later patches
- Move the original Heavy/Block functionality behind a single combat action arbiter; do not silently remove stable controls.
- Tune double-flip axis and jump clip/ground transitions with recorded input timestamps.
- Profile pooled particles/rings and reduce short-lived Instantiate/Destroy if verified.
- Calibrate per-clip impact markers, recoveries, animation blending and aerial action windows.
- Character orientation/foot calibration and phone-specific touch responsiveness need instrumented runtime testing.

**Patch status:** source change only. Unity compilation, WebGL deployment, and S23 validation are pending.
