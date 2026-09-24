# HIGHFLY — WORLD / ISEKAI SYSTEM DONOR AUDIT

Fecha de auditoría: 2026-09-24

## Regla principal

HIGHFLY CORE sigue siendo el host. Ningún donor puede reemplazar:
- PlayerController / HighflyThirdPersonMotor
- cámara móvil
- joystick / multitouch
- targeting / lock
- HUD fundamental
- stats / progresión
- inventario HIGHFLY
- combate HIGHFLY

Los donors se usan quirúrgicamente a través de adapters.

## Arsenal primero

Antes de buscar una dependencia externa:
1. Revisar /PACKS.
2. Revisar Library.
3. Revisar ramas/repos/artifacts previos.
4. Recién entonces buscar web.

## Donors aprobados para LAB / auditoría técnica

### Tiempo, calendario y rutinas NPC
**FelixBole/schedule-master**
- URL: https://github.com/FelixBole/schedule-master
- Licencia: MIT
- Uso deseado: reloj de juego, ticks, fecha/estaciones, eventos programados, horarios NPC.
- NO tomar: UI/demo como autoridad visual de HIGHFLY.
- Adapter objetivo: HighflyWorldClock + HighflyNpcScheduleBridge.

### Diálogo NPC
**YarnSpinnerTool/YarnSpinner-Unity**
- URL: https://github.com/YarnSpinnerTool/YarnSpinner-Unity
- Licencia: MIT
- Para Unity 2021.3 usar línea Yarn Spinner 2.x.
- Uso deseado: diálogo ramificado, comandos/eventos narrativos.
- HIGHFLY conserva InteractionController y UI móvil propia.

### Misiones
**lluispalerm/QuestSystem**
- URL: https://github.com/lluispalerm/QuestSystem
- Licencia: MIT
- Uso deseado: quest graph, giver/updater, estados, objetivos y save.
- Integrar recompensas mediante HIGHFLY RewardEngine / inventory bridge.

### Persistencia del mundo
**DerKekser/unity-save-system**
- URL: https://github.com/DerKekser/unity-save-system
- Licencia: MIT
- Uso deseado: Savable components, transforms, GUID/data, objetos/prefabs.
- Casos HIGHFLY: árboles talados, vetas, cofres, cultivos, puertas, parcelas, construcciones.

### Construcción / placement
**SunnyValleyStudio/Grid-Placement-System-Unity-2022**
- URL: https://github.com/SunnyValleyStudio/Grid-Placement-System-Unity-2022
- Licencia: MIT
- Uso deseado: grid, preview ghost, placement/removal states, occupancy.
- Adaptar input a controles HIGHFLY mobile.

**Arsenic-23/FortniteStyle-Building-System-Unity**
- URL: https://github.com/Arsenic-23/FortniteStyle-Building-System-Unity
- Licencia: MIT
- Uso deseado: claim/ownership de parcelas, permisos, move/rotate/edit.
- PROHIBIDO importar Photon PUN2 o su PlayerController.
- Extraer sólo lógica local reutilizable de plot/ownership/edit.

### Farming
**viskakov/Farm**
- URL: https://github.com/viskakov/Farm
- Licencia: MIT
- Uso deseado: estados de crecimiento, cultivo, cosecha/corte.
- Recompensa final pasa por HighflyGatheringCore -> Inventory.

### Fishing
**Kevin-Kwan/Unity3D-FishingRodMotion**
- URL: https://github.com/Kevin-Kwan/Unity3D-FishingRodMotion
- Licencia: MIT
- Proyecto donor: Unity 2022.3.
- Uso deseado: caña, línea, cast/reel, física Verlet.
- No importar Animator/PlayerController del demo.
- Catch tables, rareza y loot pertenecen a HIGHFLY.

### Weather
**Slord6/WeatherSystem**
- URL: https://github.com/Slord6/WeatherSystem
- Licencia: MIT
- Proyecto original: Unity 2017.3.
- Uso deseado: modelo de estados, intensidad, temperatura/humedad y callbacks.
- NO importar rendering/legacy image effects.
- HIGHFLY implementará presentación URP ligera (luz/fog/particles/audio).

## Sistema propio preferido: HighflyGatheringCore

No hace falta un framework externo para cada profesión.

Un mismo núcleo debe manejar:
- TreeNode + axe -> wood
- OreNode + pickaxe -> ore
- CropNode + seed/time -> harvest
- FishingSpot + rod -> fish
- forage/resource nodes

Pipeline:
INTERACT -> TOOL CHECK -> ACTION -> RESOURCE STATE -> LOOT TABLE -> INVENTORY -> RESPAWN/PERSISTENCE

## Orden de integración

1. World Final visual + zone anchors.
2. World Clock + persistence.
3. Gathering Core.
4. Inventory/Loot bridge.
5. Property + Placement.
6. Farming.
7. Fishing.
8. NPC schedules + dialogue.
9. Quests/economy.
10. Weather presentation.

## No-go

- No framework survival completo.
- No segundo PlayerController.
- No segundo CameraRig.
- No Photon/Netcode por un donor de building.
- No packs/reuploads sin licencia verificable.
- No sistema externo de inventario como autoridad.
- No dependencia que rompa Android/WebGL sin una prueba aislada previa.
