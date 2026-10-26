# HIGHFLY WORLD W2-A — First classless RPG loop

**Status:** source integration on isolated branch, not Unity GREEN until an actual Unity6/WebGL workflow succeeds.

**Branch:** `feature/highfly-world-w2-rpg-loop-20261026`.
**Frozen playable FURY donor base:** `bd259a1da856339df398e04da5118c920345484a`.
**Guinevere R06:** remains separately available as a visual-prototype branch; W2 intentionally does not merge R06's unapproved visual changes.

## Real implementation in W2-A

1. Persistent local-browser PlayerPrefs/JSON save, schema version 1, independent of the World of ClaudeCraft server. Saves items, gold, equip IDs, quest stage, kill count, level, XP and Quaternius look index.
2. Classless inventory item catalog and stacking, starting CC0 sword, `Armor Rookie` reward giving +3 VIT; no Warrior/Mage/Rogue restrictions or specialization UI.
3. Equipped weapon shows a **legal KayKit CC0 sword** parented to each legal Quaternius Humanoid `RightHand`. Hunter appearance is cosmetic; clothing/armor never auto-changes based on armor stats. Uses existing Quaternius variants and common FURY animator.
4. Equipped sword +3 STR modifies damage 6%; rookie armor +3 VIT modifies max HP by +30. This is an initial tuning choice and can be rebalanced.
5. The new `MÁS ...` mobile UI exposes Character, Backpack, Equipment, Missions, and a clearly labelled Training Core placeholder. It includes Save, Equip Sword, Remove Sword, Equip Armor and Change Skin.
6. Two low-level skeletons spawn near the existing training area outside CITY01 and credit the first quest on death (after acceptance). Each drops a tangible collectible bone + gold reward, via an interactable Unity object; **loot is not awarded without interacting**.
7. Serin is guaranteed by editor construction near the original RG POLY Hunter spawn. TALK: accept kill-2-skeleton quest; return: +75 gold, +80 XP, rookies' armor. Quest, equipment and inventory are auto-saved.

## Code provenance and restrictions

- World of ClaudeCraft original source: `https://github.com/levy-street/world-of-claudecraft`, MIT `LICENSE`; its item-ledger, inventory-grant, quest snapshot, loot system are architecture references. W2 is an original C# ported pattern, **not a drop-in TypeScript module or wholesale repository copy**.
- ClaudeCraft's reserved/proprietary character art, classes and UI icons have NOT been imported.
- KayKit weapons and Quaternius outfits are CC0 donors already staged by the existing Unity build process. RG POLY Medieval remains the licensed external world pack, already present on the Golden Host.
- FURY locomotion, camera, joystick, combat hit authority and HUD action positions are unchanged. Equipment only adjusts combat amount and health capacity.
- No classes or specializations. Quaternius Knight/Rogue/Warrior descriptors are **appearance file names**, not class assignments.

## Known limits — do not oversell

- Local PlayerPrefs/JSON is a browser/device save only. Clearing site storage clears save; no cloud sync, character accounts, server transactions, save migration beyond schema 1 or anti-cheat.
- Source script exists for stamina/mana but not yet Training Core real exercise validation. MENU labels it correctly as forthcoming.
- No dynamic spawning/respawn schedule yet. First two skeletons respawn on a fresh scene load, not on a timer; kill-credit persists after accepting contract.
- Generated sword orientation/scale and clothed starter preset require S23 WebGL visual validation. Missing weapon FBX is intentionally a build-time failure to avoid falsely claiming the sword is visible.
- Early enemy spawn positions are based on stable FURY source coordinates. Actual accessible terrain must be validated on S23.
- No multiplayer, crafting, shops, dungeon rewards or expanded quest graph are claimed as complete.
- Not a replacement for the isolated Guinevere R06 skill-polish branch.

## S23 acceptance checklist

- Fullscreen landscape and previous FURY neutral joystick, dash, double jump and attack must remain GREEN.
- Open and close MÁS without camera/joystick accidental input; tabs and EQUIP actions work.
- Change Quaternius skin and verify sword remains attached to the new right hand; armour item does not alter cosmetic skin.
- Unequip and re-equip sword, check visual and STR damage bonus.
- Interact with Serin (first NPC near world entrance), accept first hunt quest.
- Reach forest outside city, defeat both W2 skeletons, and pick up their orbs with USAR.
- View bones and gold in backpack, return to Serin to claim armour, equip and verify max HP rises.
- Reload same Pages URL without clearing browser data; check inventory, gold, quest, level and skin.
- Compare FPS and mobile HUD readability; do not declare benchmark results without measurement.
