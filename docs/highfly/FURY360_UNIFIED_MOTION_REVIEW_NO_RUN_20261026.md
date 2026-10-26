# HIGHFLY NEXUS — FURY 360° unified movement audit (2026-10-26)
Status: **CODE-REVIEW CANDIDATE — NO UNITY BUILD / NO GITHUB ACTIONS / NO PAGES PROMOTION.**
Input: user S23 recording `1000460846.mp4` (62.57 seconds) plus actual repo source at `720cbac7be0e8a269d99609f5cbd2c535991aaf7`.
Base public beta: `40344784751`, published by `40348856412` (do not overwrite).

## 1. What the phone recording supports, and what it doesn't
Visible during ~12–60 seconds: Hunter travels through City01, several skill VFX, unusual poses/cycles, turns/camera and jumps. User's direct observation: jumping while running hardly carries forward; some movements are unnatural.
Video doesn't expose real game physics vectors, button event traces, contact normals or Avatar root curves. Exact causes of individual poses require Unity/phone instrumentation; don't claim animation quality GREEN from successful CI.

## 2. CONFIRMED code-level faults in original public beta
1. On joystick release, `deceleration=55`, `sprintSpeed=7.2`: forward velocity disappears in ~0.13 seconds even during airborne motion. Jump impulse updates Y only; no separate air inertia.
2. `DashRoutine` bypasses `Update` with early return; moves horizontal without gravity/regular collision processing for the dash frames and suddenly stops/restarts normal movement.
3. Legacy `ProbeNearGround` accepts up to 0.38m gap as "grounded" and can reset jump counts before an actual Below collision.
4. Double jump only allowed when `_verticalVelocity > 0`; no re-jump at apex/descending. Current joystick direction isn't authoritative for flips: stale `_lastDirectionalInput` can choose a previous direction.
5. Somersault rotates around the avatar's mesh root near feet, not around the center of mass, resulting in exaggerated limb/camera offsets while in air.
6. `HighflyAnimationBalance.AlignFootHeight()` samples foot bones once in `Start()` before guaranteeing a settled idle frame, introducing a fixed visual offset that can look like floating.
7. `HighflyGuinevereCombat.Face()` rotates the root immediately, fighting the motor's own rotation on the next Update when attacking while moving.
8. `RupturaLeap()` invokes another `CharacterController.Move` from its coroutine while FURY motor also moves once per frame; recast also did a single instantaneous 4.7m Move.
9. Quaternius `Sword_Dash` was a torso-only layer (full body clip on masked upper body), creating incomplete roll/dash posing.
10. Mobile joystick didn't own a pointer ID; simultaneous finger interactions had no explicit input ownership. PC Shift used the legacy combat dash instead of motor dash.
11. `FreeUpper` empty upper layer already changed to weight 0 in the prior build, and UAL2 sword combo A/B/C/heavy was already integrated, but timing/pose quality still needs active test.

**Useful official evidence:** Unity 6000.6 `CharacterController.Move` returns `CollisionFlags` and doesn't apply gravity. `CharacterController.isGrounded` describes the *last* Move, not current predicted contact.
Docs: https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/charactercontroller/move

## 3. Implemented in isolated REVIEW branch (no RUN)
Branch: `review/fury360-unified-motion-20261026`.
- A single authoritative motor Update drives planar displacement, gravity, jump and dash, one `CharacterController.Move` per frame.
- Ground acceleration/deceleration distinct from air acceleration/deceleration; preserves horizontal run inertia after releasing stick in the air; analog walk→jog→sprint velocity transition no .84 input cliff.
- Ground contact for *jump initiation* can use nearby floor; *landing* only resets air jump after `CollisionFlags.Below`; narrower floor-probe tolerance.
- Double jump allowed after short safety interval while rising OR falling; neutral double jump defaults to forward flip; cardinal input selects forward/back/left/right.
- Somersault pivots around chest/hips, not feet; physics capsule stays upright.
- Neutral/moving dash shares gravity/CollisionFlags, smooth ease in/out distance, and resumes with controllable running speed.
- Ruptura S2 requests movement *through* FURY; its coroutine only executes animation, VFX, skill timing and collision queries. Recast uses a short collision-safe dash instead of 4.7m instant physical displacement. S2 interrupts a previous ordinary dash for action chains.
- Facing for auto-target becomes a brief motor request, eliminating conflicting direct rotations in the combat Update.
- Base animator has full-body `Sword_Dash` state (rather than only masked upper torso); skills/ATK remain upper-body so legs can run.
- Foot calibration delayed until multiple genuinely grounded, stationary, upright animator frames. Visual position is changed once without shifting the physical capsule.
- Joystick captures/relinquishes exact pointer ID, keyboard Shift routes to `motor.Dash()`.
- Exposes `CurrentPhase` (Idle/Moving/Rising/Falling/Dashing/Swimming/Wading), `CurrentPlanarSpeed`, `CurrentVerticalSpeed` for future diagnostics.
- City01, Monsters, NPC and user-facing Pages remain untouched.

## 4. Complete movement/input acceptance matrix

| Feature | Start/input | Required observable behavior | Verification |
|---|---|---|---|
| Idle posture | No stick 3s | Upright, feet grounded, no open-leg/bind pose | S23 & Avatar |
| Walk analog | 20–35% stick | Walking clip and measured low speed | S23 |
| Jog analog | 55–75% stick | Jog clip, smoothly blended | S23 |
| Sprint analog | 90–100% stick | Sprint clip, no abrupt threshold teleport | S23 |
| Diagonal 360 | 8 stick vectors | Correct camera-relative heading, no inversion | S23 |
| Camera orbit | Drag right; move left simultaneously | Camera independent, stable multitouch | S23 |
| Braking | Release stick on ground | Predictable short run-out and idle | S23 |
| Standing jump | Release stick, tap SALTAR | Vertical-only jump is EXPECTED, not a bug | S23 |
| Running jump | Sprint, briefly release stick, tap SALTAR | Keeps forward air inertia; covers distance | S23 |
| Air steer | Jump, steer left/right | Limited air adjustment, not instant stop | S23 |
| Second jump | Rising, apex, falling | One allowed extra impulse; no triple exploit | S23 |
| Flip directions | 2nd jump with ↑ ↓ ← → | Distinct front/back/cartwheel axes, center stays stable | S23 |
| Landing | Jump/fall to RG POLY floor | Contact before reset; no hovering/instant grounded | S23 |
| Dash neutral | No joystick; DASH | Dashes forward on current facing, gravity maintained | S23 |
| Dash moving | Run; DASH | Correct velocity/direction and easing | S23 |
| Dash in air | Jump then DASH | Does not freeze Y; no snap through floor | S23 |
| ATK 1–4 | Tap ATK repeatedly | UAL2 sword A/B/C/heavy correctly staged | S23 |
| Running ATK | Sprint + ATK | Legs keep locomotion, upper body strikes | S23 |
| Jumping ATK | Jump then ATK | Hit logic and animation still available | S23 |
| Ruptura S2 | Drag aim, cast in motion | One owner of displacement; lift+recast work | S23 |
| Dash→Ruptura | DASH then S2 quickly | Higher-priority S2 replaces basic dash displacement | S23 |
| Prisma S1 | Tap/drag while moving | Camera-relative aim, no unwanted stop | S23 |
| Tempestad S3 | Move during AOE | Tick damage and torso animation, no physics lock | S23 |
| Male/female swap | Change appearance | Ground offset and Animator bindings survive | S23 |
| Water/wading | Enter water zones | Distinct movement states, regain ground correctly | S23 |
| Collision | Walls, steps, slopes | No pass-through, no root/visual separation | S23 |
| Performance | 3–5 min gameplay | No excessive allocations and acceptable stability | S23 |

## 5. Not yet solved (must not claim "fixed")
- Need Unity **compile** proof. Current branch deliberately does not trigger workflows.
- Need instrumented S23 comparison for takeoff XZ displacement, foot/slope contact, root-pose Y, animation clip duration, and true camera-relative flip directions.
- No calibrated 2D strafe BlendTree, foot IK, additive hit reactions or root-transform bake audit yet. Avoid speculative IK/foot adjustments until actual visual behavior is measured.
- UAL2 `Sword_Heavy_Combo` includes multiple motions: its clip duration relative to the 0.36s basic attack cadence needs Unity and phone testing.
- UAL1 and UAL2 Humanoid Avatar retargeting is plausible and already built GREEN in the public version, but correct toe contact during transitions isn't certified.
- Source has old fallback systems; only the currently installed HIGHFLY motor/Guinevere actions were refactored. Future Unity native/Android build must confirm no legacy handlers run twice.
- Gameplay F5 remains prior GREEN. No current new build, no app deployment, no false "fully fixed" claim.

## 6. Proposed next controlled validation (ONLY when authorized)
1. Run static code/style tests, then manually authorize **one** Unity build from an approved branch; no further scope expansion.
2. Inspect actual compile diagnostics for FURY motor, Animator transitions, dash clip lengths, rig foot calibration and one-Move rule.
3. Publish isolated Pages preview only after GREEN and explicit approval.
4. Record same reproducible phone test sequence on Samsung S23 Ultra (idle, sprint→release→jump, jump→air dash, 4-axis flip, 4-hit UAL2, sprint skill).
5. If grounded posture still looks wrong, inspect Unity humanoid Avatar T-pose and clip `Root Transform Position (Y)` baking; distinguish animation root from physical capsule before further offsets.
