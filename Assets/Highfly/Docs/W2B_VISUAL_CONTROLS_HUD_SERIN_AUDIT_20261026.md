# HIGHFLY WORLD W2-B — S23 corrective pass

## Provenance / isolation

- Based on user-tested W2-A Unity6 GREEN commit `f5bc8741061fb70c6cfc790931239b986f6aaa67`, Pages preview `/preview/highfly-w2/`.
- W2-B is separate branch; no commits to FURY tested GREEN or to W2-A. W2-A beta must stay unchanged.
- User feedback October 26: Quaternius character appears to move sideways; residual weapon; flat web-like, non-full-screen menu lacking real item slots; Serin hard to find; Guinevere hit effects but chained skills feel unsynchronized; wants Mobile Legends-like combat HUD.

## What was actually implemented in code

1. Quaternius visual-only initial 90-degree yaw correction in `HighflyHunterAppearance`, with an optional +90-degree correction button, persistent `visualYaw`. FURY movement root/CharacterController unchanged. 90 degrees is a provisional rig alignment that must be verified against imported FBX model coordinates on S23.
2. Inventory/UI: replace text-card with **full-screen native Unity Canvas**; left navigation; 24 clickable backpack cells (6x4); item labels, stack counts, classless equipment and stats; premium dark/gold decoration; close control; orientation calibration and save. No HTML front-end. Avoid copying proprietary ClaudeCraft UI media.
3. Disallow residual embedded gear visuals from Quaternius cosmetic prefabs; only `W2_Equipped_Sword` follows equipped state. Preserve bones, damage authority and all appearance variants.
4. Serin: ground-snap encounter near city entrance, floating yellow world-space quest exclamation and HUD distance/direction compass. Source city's exact terrain accessibility still must be verified on Android.
5. Disable mobile W2 camera `targetAssist` automatic yaw while retaining combat `HighflyTargetingSystem`. Existing free-touch right camera retained.
6. Preserve Guinevere gameplay skills; increase S2->S3 buffered input duration from .30s to .82s so an input during S2's ~.42-second dash no longer expires before `FlushBuffer`. This does NOT add the still-pending parabola / bespoke Skill2 animation.
7. Restyle existing MOBA action ring by decorating ATK/S1/S2/S3/DASH buttons without rebinding them or moving joystick/camera zones.
8. Preserve existing PlayerPrefs schema v1 save, migrate new visual yaw field without losing prior items, XP, quest and gold.

## Known limitations and no false claims

- This is native Unity UI styling with existing text/font/color components; it does not yet have commissioned icons or native atlased artwork. Premium *layout and interaction* is implemented; final art pass needs legal or authored graphics.
- The user-provided mp4 could not be decoded by the current media runtime (read attempts failed). Camera and sideways analysis were based on inspected W2 code + user's observations, not unverified per-frame vision claims.
- The S2 physical movement is **still a horizontal dash**, not the requested airborne parabola. It will be tackled on a separate Guinevere animation/trajectory lane after WORLD W2-B is tested.
- No new animation clips added. The 0.82s input buffer improves command chaining, not the visual quality of the cast animations.
- The rig compensation and hand sword transforms must be visually verified on S23; users can adjust facing via the menu, without changing joystick mechanics.
- The legacy item ledger and quests of World of ClaudeCraft are MIT inspiration; W2-B has no proprietary ClaudeCraft art, trademarks or class/spec selection.

## S23 acceptance

- Character runs forward facing the correct way; rotate visual using menu if correction sign varies by outfit. Joystick movement and camera independent (no target camera hijack).
- One equipped sword visible on right hand, none when unequipped, no decorative doubles after cycling skins.
- Full-screen game-native inventory, 24 cells; tapping Sword/Armor equip controls actual stats, bones, gold and quest count refresh.
- Closing menu returns to HUD without attacking or jumping.
- Serin marker and compass visible from spawn, Serin reachable and interactable for starting contract.
- S1/S2/S3 buffer visible through cooldowns; try S2 then queue S3 before landing; verify hits and finish independently.
- Guardado persistence through page reload including skin forward-axis offset.
- No performance/FPS claims until Samsung test.
