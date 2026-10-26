# HIGHFLY NEXUS — FURY 360° Integrated Audit (Android S23 Ultra, 2026-10-26)

## Why this pass exists
User video 1000460799.mp4 (48.5 s) demonstrates residual visual hover/feet-apart posture, movement cycles not fully matched to ground speed, jump apparently requiring joystick movement, dash not independently convincing, and an insufficiently expressive ATK 1–2–3–4. Keep the original RG POLY City01 and FURY physics root untouched.

Previous evidence:
- Previous green UNITY6 run 40334053153 selected exact UAL1 `Idle_Loop/Walk_Loop/Jog_Fwd_Loop/Sprint_Loop/Jump_Start/Jump_Loop/Jump_Land`. Thus "clip exists and compiled" did not mean it looked good on phone.
- Original automatic selection chose `Crouch_Idle_Loop` as idle and `Sword_Idle` as ATK4; fixed in ed6ab0e.
- Isolated UAL2 Standard *real free CC0* pack (sha256 4008ea208a604773a2b2177d965f0f5d3195498b5bf838c3f5785d68e95f2a68), run 40327951004 GREEN: exact `Sword_Regular_A/B/C`, `Sword_Heavy_Combo` confirmed as Unity animation clips, but older branch used crouch idle.

## Integrated code candidate
Commit `539840444ba23241b7a375d89c0e935520916624`; build `40344784751`. **NOT a passed mobile QA until Unity GREEN, Pages GREEN, and S23 playtest.**

- Physically supported idle-jump: `HighflyThirdPersonMotor.ProbeNearGround()` uses short-range non-alloc raycast and coyote time, ignoring own character collider, not stick input.
- Separate HUD `SALTAR` -> `motor.Jump()`; `DASH` button -> `motor.Dash()` (not legacy CombatController action); neutral DASH chooses actor facing.
- Upper animation layer: former `FreeUpper` null motion at weight 1 can overwrite the underlying torso pose; now neutral layer starts weight 0 and `HighflyAnimationBalance` enables actions only.
- Initial model foot correction: `HighflyAnimationBalance` reads left/right Humanoid foot bones and capsule floor in runtime, without shifting physical collider.
- In-place UAL1: idle, walk, jog, sprint and takeoff/air/landing remain exact; no `_RM` clips.
- In-place UAL2: actual free CC0 sword regular A/B/C + heavy combo remate, loaded at build; no paid packs; never use `Sword_Idle` to attack.
- World City01 still from original approved RG POLY scene.

## Acceptance test matrix — do not mark GREEN on compilation alone

| Feature | Real phone test | Pass requirement |
|---|---|---|
| Standing idle | No joystick for 3 seconds | Feet touching ground, torso relaxed, no T-pose and no drifting |
| Walk/Jog/Sprint | Stick 20%/55%/100% | Distinct gait, fluid response, minimal foot skating |
| Turns/camera | 8 stick angles and right-side orbit | Direction matches stick and camera relative framing |
| Stationary first jump | Hands OFF joystick, tap SALTAR | Actual vertical motion and takeoff clip immediately |
| Moving first jump | Run and tap SALTAR | Runs into jump without disabling stick |
| Double jump | Tap SALTAR twice with stick neutral | Second impulse visible and consistent airborne rotation |
| Directional flip | Jump twice with up/down/left/right stick | Forward/backward/left/right visually distinguishable |
| Neutral dash | No stick, tap DASH | Travels forward, distinct motion, no extra input |
| Moving dash | Run + DASH | Applies dash in current move direction |
| ATK 1–2–3–4 | Four spaced thumb taps at rest | Four sword stages A/B/C/heavy appear, stage 4 stronger |
| Running/air ATK | Run then attack; jump and attack | No forced full-body standing stance |
| Skills S1/S2/S3 | Neutral and move, directed drag | Cast independently and animate torso without freezing movement |
| Appearance switch | Male/female outfit control | Both rigs contact ground and keep controls |
| World regression | Visit capital, gates, forge and forest | RG POLY remains intact, skeleton outside wall, no newfound clipping |
| Android perf | Play continuously for several minutes | FPS, memory and temperature acceptable on Samsung S23 Ultra |

Open issues outside this pass: additional forja/shield prop, overall VFX aesthetic, persistent RPG save, deeper mobs/loot, realtime network. Do not conceal them inside movement GREEN.

## Proof required before releasing
1. New Unity6 build green and log confirms `Animations2: 1 FBX`, upright takes, `FURY COMBO STAGE 1–4`, 2 Quaternius humanoid avatars, and the independent input setup.
2. Automatic Pages publishing green, artifact sourced from exactly current Unity run, isolated path `/preview/guinevere-fury/`.
3. Android landscape S23 hands-on recording. Build + Pages GREEN is **not** movement/animation approval.
