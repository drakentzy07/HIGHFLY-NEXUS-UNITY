# HIGHFLY NEXUS UNITY — Guinevere R06 + FURY input-green integration

## Scope and provenance

- Independent integration branch: `feature/guinevere-r06-fury-input-green-integration-20261026`.
- Exact user-tested FURY base: `bd259a1da856339df398e04da5118c920345484a`, Unity GREEN #40398525735, Pages GREEN #40403869805.
- Donor R06 branch: `feature/nexus-guinevere-premium-vfx-r06-isolated-20261026`, `c516cf76f1135b2f45a7fc42c0652c207ba1eaa5`.
- Base `HighflyGuinevereCombat.cs` remains the authority for damage/cooldown/mana; `HighflyThirdPersonMotor.cs` unchanged and remains the sole physical CharacterController.Move authority.
- Only donor `HighflyGuineverePremiumVfx.cs` and `HighflyGuinevereVfx.cs` were adapted and imported, not their old Combat/Motor.
- Quaternius Humanoid, RG POLY Medieval, CITY01, touch controls and skill animation controller remain the stable FURY versions.

## Integrated visual-only hooks

| Hook | Source of authority | R06 output |
| --- | --- | --- |
| PrismaSpawn | Combat projectile initial position and lifetime | brighter layered orb halo, starbursts |
| Launch | S2 calls from RupturaLeap | launch rings and rising particles |
| Impact | real projectile, dash or echo collision timing | white-magenta spark and expanding rings |
| Mark | valid hit on living target | visible 1..3 stack energy |
| Lift | real non-boss lifted state | lift column, suspension rings |
| Basic | the existing ATK 1-2-3-4 hit/miss visual callback | charged finisher flare |
| Blink | completed S2 recast motor displacement | visual streak only |
| Echo | recast phantom position, delayed echo | mesh-baked Quaternius pose snapshot (eligible compatible meshes), no AI/collider |
| Tempestad | exactly 11 Combat coroutine pulse callbacks | varied orbit sparks, rings, stronger 11th |

Additional integration: `SuperMagicNormalized` drives a low-cost visual aura at charge >=35%; true hit-charged heal triggers `HealPulse` after the existing actual healing has been applied. Never generates charge on a miss.

### Enemy knock-up completeness
The previous `HighflySimpleEnemyAI.ApplyAirborne` only suppressed enemy movement (it did not visibly raise the target). The integration now lifts the **Animator's child visual only** during the authoritative airborne/combat state, using a short rise / held suspension / fall envelope. The enemy physics root and its hitbox stay fixed, so the eleven S3 pulse OverlapSphere queries still detect the real target. Bosses remain immune to normal lifting and instead take poise pressure. The displacement is reverted on state expiry/disable, and ordinary non-airborne animations are not overwritten.

**R06 replaces rather than stacks old effects** when active. Disabled R06 uses original effects for A/B testing. Projectile shape/trail retains its existing real transform so collision logic is unchanged. No direct force/animation/state ownership in visual classes. No proprietary Genshin/MLBB assets.

## Controls for S23 WebGL comparison

A pair of small buttons under the left HUD: `FX FLUIDO / FX EQUIL. / FX ULTRA`, `R06 ACTIVO / FX BASE`, plus FPS counter measured as rendered Unity frames / realtime window. The counter is an indicative runtime display, not a device GPU profiler. Do not assume 60fps without actual S23 measurement.

## Known implementation facts and limitations

- S1 4s/14mana, S2 17s/23mana with 5s recast, S3 52s/40mana. These values remain HIGHFLY placeholders.
- S1 uses existing projectile sphere-casts and ~0.14s windup. S2 uses FURY TryDash and delayed hit zone. S3 has 11 evenly-timed pulse callbacks over ~2s.
- Quaternius animation names are those already pinned in `HighflyQuaterniusHeroInstaller`; no new proprietary or dedicated skill clips were introduced. Quality of exact S2/S3 body choreography still needs device review.
- Donor R06 is authored as **procedural Unity particles/lines**, not imported authored custom sprite meshes or official Mobile Legends animations; R06 is more articulated than old circles but final aesthetics require an S23 video and further art direction.
- Postimage is a static baked SkinnedMeshRenderer pose, not a moving illusion. It is rendered only if an eligible Quaternius mesh is present and graphics supports the transparent shader.
- Shader resolution, memory/GC, performance and actual skill hit physics must be confirmed by the new Unity WebGL build and S23 tests. This file is not a certification.
- No Wafflus/Genshin movement source was imported.

## Mandatory Android gameplay acceptance

1. Verify neutral-stick inputs (idle, standing jumps, DASH, ATK, S1/S2/S3) remain responsive; prior FURY input regression must not return.
2. Validate joystick and camera multitouch simultaneously while walking, jumping, reversing and parkouring roofs.
3. S1 projectile launch while idle, running and midair; manual misses do NOT apply damage/marks and projectile collides with scenery. Camera-relative drag aim & cancel.
4. S2 continuous dash/elevation, no teleport or wall clipping; recast goes via FURY TryDash (no second Move).
5. S3 pulses exactly eleven times visually and in Combat, available during recovery; no lingering animation lock.
6. Combo: S2 -> elevate -> S3 -> recast S2. Also S1 -> S2 -> S3 and jump+somersault -> S1.
7. Charge true-hit passive, observe aura buildup, empowered basic and heal; misses grant no magic.
8. Switch FX R06/Base mid-session and Fluido/Equilibrado/Ultra; compare FPS counter and S23 frame pacing. Inspect runtime shader errors and unsupported particle shaders.
9. Check performance (allocations and mesh snapshot) and any missing/humanoid model clips.

## Deployment policy

Keep CITY01 stable and the old three FURY previews unchanged. Do not deploy this integration from the compiling workflow. Publish another isolated URL only after a GREEN Unity build.
