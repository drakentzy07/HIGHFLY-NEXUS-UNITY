# HIGHFLY — WORLD / ISEKAI W0/W1 EXECUTION

Baseline: `highfly/world-final-free-v01` @ `9e5703e76a7ea4d128cdaf85e0fec9450260b8ca`

Master contracts:
- WORLD: `HIGHFLY_WORLD_ISEKAI_MASTER_BUILD_BIBLE_v1.0_SUPREME.pdf`
- COMBAT: `HIGHFLY_COMBAT_REBOOT_MASTER_BUILD_BIBLE_v2.1_SUPREME_AURA_ANYRPG_FUSION.pdf`

## Immutable boundary

This branch does **not** replace or edit:
- HIGHFLY/LUCID player locomotion
- mobile camera / joystick / multitouch
- targeting / lock-on
- combat executor / damage authority
- Training Stats

WORLD owns topology, scene transitions, stable anchors and world state only.

## Audit result

### Keep unchanged
- `HighflyThirdPersonMotor`
- `HighflyThirdPersonCamera`
- `HighflyVirtualJoystick`
- HIGHFLY Combat folder/runtime
- World Final FREE capital art/layout
- Quaternius CC0 staged village/props
- existing interaction controller/UI

### Reuse through W0 adapters
- World Final capital -> W1 functional host
- ClaudeCraft runtime/data model -> future converter/reference, not concurrent host
- AnyRPG SceneNode/Teleport/Quest/etc. -> data patterns, not runtime authority
- existing WorldZoneAnchor -> semantic reference for later exterior systems

### Legacy/proxy behavior intentionally bypassed
- `HighflyBuildingDoor`: same-scene coordinate teleport
- `HighflyZonePortal`: same-scene coordinate teleport
- old `BUILDING_INTERIORS`: rooms parked in the host scene
- Village/Claude runtimes as competing world hosts

They remain in source for compatibility/provenance but W0/W1 does not use them as scene authority.

## W0 executable architecture

`W0W1Catalog.json`
→ `HighflyWorldRegistry`
→ `HighflyWorldRuntimeHost`
→ `HighflyWorldSceneService`
→ typed `HighflyWorldAnchor` IDs
→ additive Unity interior scenes
→ exact exterior pose/anchor restore

Interior flow:

`Exterior SceneDoor`
→ validate registry definition
→ snapshot exterior scene + anchor + exact Hunter pose
→ async additive load
→ resolve stable entry anchor
→ teleport through existing HIGHFLY InteractionController
→ service interaction
→ ExitDoor
→ unload additive scene
→ restore exact exterior pose/rotation

The player, camera and combat objects never move to a second authority and are never duplicated.

## W1 city vertical slice

The existing capital is reused. Four buildings become functional first:

1. GUILD → `HF_INT_CAPITAL_GUILD`
2. TAVERN → `HF_INT_CAPITAL_INN`
3. BLACKSMITH → `HF_INT_CAPITAL_FORGE`
4. MARKET_HALL → `HF_INT_CAPITAL_MARKET`

Each interior is an independent scene containing only environment + a W1 service endpoint + exit.

W2 will attach NPC/dialog/vendor/quest systems to these endpoints without changing the W0 transition contract.

## Gate

W0/W1 passes only if:
- all 4 exterior doors enter their correct independent scene;
- Hunter/camera/joystick remain the approved baseline;
- exiting restores the exact exterior location and facing;
- 20 repeated enter/exit cycles create no duplicate player/camera/world host;
- old same-scene interior proxy is not used;
- no enemies are required for the W1 city test;
- WebGL build is green before S23 validation;
- no file under Combat, motor or mobile camera is changed by this branch.
