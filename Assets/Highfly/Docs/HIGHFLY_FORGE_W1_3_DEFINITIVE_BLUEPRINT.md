# HIGHFLY — FORGE W1.3 DEFINITIVE BLUEPRINT

## Locked purpose

This pass touches only the Forge interior. HIGHFLY/LUCID movement, camera, input,
lock-on, Combat Core, Training Stats, RG Poly exterior city, Inn, Guild, Market
and water systems remain unchanged.

## Spatial contract

### Ground floor — retail shop (26 x 18 m)

Customer flow:

1. Exterior door -> ENTRY_ANCHOR.
2. Clear central aisle.
3. Weapon and equipment showcases on the left.
4. Customer seating / secondary shelves on the right.
5. Main counter at the rear-center.
6. Blacksmith/vendor behind the counter.
7. Stairwell on the rear-right, physically open through the floor.
8. EXIT_TO_CAPITAL remains beside the exterior-facing entrance.

Retail functions:

- Blacksmith/vendor: talk, buy/sell intent, repair intent.
- Weapon showcase: inspect.
- Equipment showcase: inspect.
- No forge machinery on the customer floor.

### Basement — production workshop (26 x 18 m)

Workshop zones:

- Forge/furnace zone: back-left.
- Main anvil zone: center-left.
- Craft/workbench zone: back-right.
- Material storage: front-left.
- Repair/utility table: right side.
- Clear circulation from stair landing to every station.

The basement is physically below the retail floor. The stair is built from
collidable steps and the retail floor is split around a real stairwell opening.

## Visual doctrine

- RG Poly remains the primary visual family.
- Quaternius CC0 is used only for the verified Anvil and Workbench donors.
- KayKit contributes the blacksmith visual and a sword display asset.
- HIGHFLY-generated structure is allowed where no coherent licensed donor exists.
- Donor runtime scripts are never imported into world authority.
- No donor PlayerController, camera, combat, damage or game manager is permitted.

## Approval gate

W1.3 Forge is structurally valid only if the generated scene contains:

- FORGE_SHOP_LEVEL
- SHOP_COUNTER_BODY
- SHOP_VITRINE_WEAPONS_BASE
- HERRERO_KAYKIT
- FORGE_STAIR_STEP_0
- FORGE_BASEMENT_LEVEL
- BASEMENT_FURNACE
- BASEMENT_ANVIL
- BASEMENT_WORKBENCH
- EXIT_TO_CAPITAL

The WebGL CI explicitly validates these objects before building.
