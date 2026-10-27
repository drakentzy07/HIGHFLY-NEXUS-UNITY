# HIGHFLY W2-C — Presentation Pass, first testable implementation

## User gameplay regression report
Observed in S23 landscape screen recording from `preview/highfly-w2b/`: the default bare-looking Quaternius superhero costume; an offset sideways-looking run; white weapon mesh; a long magenta line beside the Hunter; a giant plain-text/fullscreen menu with rectangular tabs. Skills are better and should not be redesigned. Root cause is NOT a failed Unity build; visuals and bindings were built that way.

## Scope, protected baseline
- New isolated branch: `feature/highfly-w2c-presentation-premium-20261027`.
- Baseline: exact W2-B Unity WebGL GREEN commit `55c5f1a4d74b290c2556357018580e1a94ff798a`, RUN #40480007730.
- The `HighflyThirdPersonMotor`, `HighflyThirdPersonCamera`, `HighflyGuinevereCombat`, skill HUD actions/listeners, W2 quest, reward, loot/save schema all remain unchanged.
- No new motion authority; player's CharacterController stays under existing FURY.
- Visual aim-only tracer cleanup / cyan recolor prevents residual pink line if pointer-up is lost on Android.

## Character / sword
- W2-B caused lateral-looking model: `HighflyW2Progress.SaveData.visualYaw=90` and `HighflyHunterAppearance.visualYawOffset=90` for ALL outfits, even if the native Quaternius Humanoid forward was already correct. W2-C defaults to 0 and migrates previous W2-B visual-only calibration to zero, with inventory, XP, gold, quest and equipment untouched. If native FBX offsets differ this still requires directional S23 validation.
- The loose base meshes `Regular`, `Superhero` excluded from offered cosmetic variants. The default selects a Knight (or Warrior) dressed Quaternius Humanoid while preserving the universal Hunter. Older looks are restored only when they resolve to a dressed variant in W2-C.
- Sword weapon: legal CC0 KayKit source, each active Quaternius Humanoid's right-hand bone; W2-C generates persistent native URP Lit steel metallic material to replace any blank/white missing material. Quaternius embedded decorative gear renderers and legacy trail/line components on the weapon suppressed. Original raw KayKit sword attached under hidden Rogue model remains hidden. Armor pieces still affect attributes only.

## Native UI rearchitecture
- An entirely new native Unity uGUI installer `HighflyW2CPremiumInstaller` replaces W2-B debug `HighflyWorldW2Installer` for the RG POLY scene. No HTML, no additional runtime camera controller for world movement.
- Proportional fullscreen anchors instead of W2-B menu's raw fixed 3088×1440 positioning; independent left navigation rail, top heading with breadcrumb, bottom status bar, content panels.
- Pages: Hunter with actual offscreen Quaternius 3D portrait (480×640 rendered ONLY when character page visible) and stats, Backpack 24 slots and detail/equip inspector, Equipment uses slots, Missions with Serin kill progress, Training (honest placeholder), System/Save.
- Menu covers game and blocks unintended attack/camera clicks when open. Native buttons and 24 Unity slot images, original procedurally generated icons for sword, armor, bone and hide — never proprietary source art.
- Single tap INSPECTS instead of auto-equipping; a second explicit button equips the selected item. Weapon display is tied to real inventory and armor remains stats only.
- HUD/skills preserve W2-B look, hit logic, timing and controller.
- Live 3D portrait is rendered isolated on layer 29 at off-world coordinates, so it cannot alter FURY camera or world gameplay. Clone rebuilds only when the outfit or equipped weapon changes, not for each XP/gold save.

## Technical / legal
- ClaudeCraft original code MIT patterns are references; no reserved branding, menu textures, button art or premium assets copied.
- Quaternius and KayKit CC0 art as already legally staged in the Unity6 build. RG POLY source pack and verified sha256 unchanged.

## Acceptance gates before promotion
1. Unity6 host compilation GREEN, WebGL artifact present; isolated Pages deployment only if exact commit GREEN.
2. No startup stripped/undressed Superhero model. Check 8 directions and ensure model forward faces forward when Hunter moves forward. No mandatory +90° UI button.
3. No second sword or pink long-lived tracer. Correct visible steel material, animation grip, physical hit damage unchanged; unequip and restore.
4. Full-screen mobile menu must display 3D character preview, 6 contextual tabs, 24 original illustrated slots with proper interaction, equip selected, save and close.
5. Original Guinevere cast cooldowns, S1/S2/S3, ATK while running and FURY parkour remain functional.
6. Quest Serin beacon, kill two skeletons and collect loot; reload page and confirm SaveKey persisted.
7. Observe Android WebGL frame pacing especially when opening portrait (extra 480×640 render) and turn off portrait if too costly before production.
8. Device-based acceptance: Unity compilation alone does NOT establish corrected model orientation, sword grip or pixel-perfect UI.

## Publication policy
Keep CITY01, FURY stable, R06, W2-A, W2-B untouched. New distinct W2-C route ONLY on successful test compile. No unverified GREEN claims.
