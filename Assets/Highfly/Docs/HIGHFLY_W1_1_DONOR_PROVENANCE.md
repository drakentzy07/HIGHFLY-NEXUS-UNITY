# HIGHFLY W1.1 — Donor provenance

## Quaternius — Fantasy Props MegaKit
- Source: https://quaternius.itch.io/fantasy-props-megakit
- License: CC0 1.0 Universal
- HIGHFLY role: forge props and medieval interactable visuals.
- Shipping policy: approved.
- Integration policy: art only; no gameplay authority.

## Quaternius — Medieval Village MegaKit
- Source: https://quaternius.itch.io/medieval-village-megakit
- License: CC0 1.0 Universal
- HIGHFLY role: modular interior shell pieces (walls/floors/doors/windows/roof).
- Shipping policy: approved.
- Integration policy: art only; no scene/player/camera/combat authority.

## RG Poly Stylized Medieval Village
- Existing approved CITY01 package.
- HIGHFLY role: exterior capital host and first-source props.
- W1.1 rule: audit/reuse before external import.

## KayKit Character Pack Adventures
- Existing HIGHFLY donor.
- W1.1 role: Barbarian visual as blacksmith NPC until the Hunter Anime/NPC art pass.
- Gameplay authority: none. HIGHFLY interaction component owns the service behavior.

## AnyRPG
- MIT code/pattern donor.
- W1.1 role: Blacksmith/Vendor/Recipe/Dialog/Storage patterns only.
- No AnyRPG PlayerController, CameraController, combat authority or UI host is imported.

## Aura Kingdom
- Reference-only for hub/service/data organization.
- No original Aura Kingdom assets ship in HIGHFLY.

## Gate
Every visual must be mapped to a concrete function or environmental role.
No pack may install its own GameManager, PlayerController, camera, damage authority or combat loop.
