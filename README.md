# HIGHFLY NEXUS UNITY

Private development repository for the Unity-based HIGHFLY rebuild.

## Foundation

- Target: Android first (landscape, Samsung S23 Ultra reference device)
- Engine baseline: Unity 2021.3.45f1
- Technical reference/foundation: SubspaceHunter-SAO (MIT code/original reusable systems, with third-party/IP assets audited separately)
- Direction: action MMORPG combining HIGHFLY fitness progression with original systems inspired by the feel of SAO and Solo Leveling.

## Rules

1. HIGHFLY code and content live under `Assets/Highfly/` where possible.
2. SAO/IP-specific non-commercial assets are reference-only and must not ship in HIGHFLY builds.
3. Third-party packages require license verification before shipping.
4. Android gameplay and touch controls are the primary target.
5. GitHub Actions will become the canonical APK build pipeline.

## First milestone — Combat Lab #1

- Third-person player
- Left virtual joystick
- Right-side camera look only
- Basic 1-2-3 combo
- Heavy attack
- Directional dash/dodge
- Block/parry foundation
- Smart auto-target
- Linear cleave skill
- Cone/AoE skill
- 3 enemies + miniboss
- HP/MP/Stamina HUD
- Damage/hit reactions/loot

## Upstream attribution

SubspaceHunter-SAO is © Hexin Wang and distributed under the MIT License for covered original code/assets. IP-specific and third-party assets remain subject to their own restrictions and are audited separately before use.
