# HIGHFLY WORLD W1.1 — REAL INTERIORS + WATER FOUNDATION

## Baseline
Branch: `highfly/world-w1-1-real-interiors-water-v01`

Base: functional RG Poly W0/W1 WebGL build.

Immutable:
- HIGHFLY/LUCID locomotion
- HIGHFLY camera + mobile input
- HIGHFLY Combat Core / SkillGraph / damage authority
- RG Poly exterior city geometry unless a later visual pass explicitly approves a change

## W1.1 doctrine
Interior means a purpose-built playable interior, not an exterior shell copied into a black scene.

Every imported asset must have:
1. provenance
2. license/shipping decision
3. concrete building role
4. mobile budget
5. interaction role when relevant

## Donor priority

### P0 — Already in the RG Poly package
Audit first. Reuse matching structure and props before importing anything.

### P1 — Approved legal art candidates
- Quaternius Medieval Village MegaKit — modular medieval structure
- Quaternius Ultimate House Interior — household/interior furniture
- Quaternius Fantasy Props MegaKit — medieval utility, weapons, crates, furniture
- Kenney Furniture Kit — furniture filler where stylistically compatible
- KayKit Dungeon — utility props/doors/furniture only where coherent

External candidates fill verified gaps only. Do not mix styles casually.

### P2 — System / behavior donors
- AnyRPG patterns: Vendor, Blacksmith, Dialog, Quest, Recipe, Storage, Teleport, Save
- HIGHFLY owns all runtime implementations and adapters

### Reference only
- Aura Kingdom: hub/service/layout/data reference only; no original Aura assets ship in HIGHFLY

## Functional buildings

### FORGE — first vertical slice
Required zones:
- entrance / exit
- blacksmith NPC
- anvil
- furnace/smelter
- workbench
- weapon display/rack
- material storage

Required interactions:
- TALK
- BUY / SELL
- REPAIR
- FORGE / UPGRADE
- SMELT
- CRAFT / RECIPES
- INSPECT DISPLAY

### INN
Required zones:
- entrance / reception
- tavern floor
- counter
- tables / chairs
- kitchen/fireplace
- upstairs or room zone
- beds / chest

Required interactions:
- TALK
- REST
- RENT / ROOM
- RUMORS / CONTRACTS later
- STORAGE later

### GUILD
Required zones:
- entrance
- reception
- quest board
- portal board / forecast
- seating/social hall
- maps / trophies / shelves

Required interactions:
- TALK
- QUEST BOARD
- PORTAL STATUS
- RANK / REGISTRATION later

### MARKET
Default: exterior service zone.
No scene transition for open stalls.
Required:
- merchant NPC
- visible merchandise
- BUY
- SELL
- TALK

Closed specialty shops may gain interiors later.

## Water foundation
Current implementation is only a water surface plus swim trigger.

W1.1 must separate:
- WaterSurface
- WaterVolume
- Shore
- Bottom / real depth
- movement state

Movement policy:
- shallow: normal walk
- knee/waist depth: wade
- chest/shoulder depth: swim
- leave water: deterministic return to land movement

Depth must be based on actual water surface minus ground/bottom height, not merely trigger contact.

Visual minimum for mobile:
- shallow/deep color split
- shoreline foam or edge cue
- subtle animated surface
- transparency appropriate to depth
- compatible WebGL / S23 Ultra

## Gates before FORGE SUPREME
- donor inventory generated
- water geometry report generated
- no Combat/Core files modified
- baseline WebGL still compiles
- all imported donor gaps explicitly identified
