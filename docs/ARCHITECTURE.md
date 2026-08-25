# SubspaceHunter-SAO Architecture and Feature Logic

This document describes the Unity project structure, core modules, and runtime logic visible in the public repository. It is intended to help contributors understand how the code is organized. The actual runnable project may still require external UnityPackages, licensed asset packages, and locally imported third-party dependencies.

## Project Scope

The public repository mainly contains:

- `Assets/PublicDemo/PublicScenes`: public demo scene files.
- `Assets/SubspaceHunter/Script`: main runtime code.
- `Assets/SubspaceHunter/Scenes/Script`: bridge scripts used by public scenes.
- `Packages`: Unity package dependency declarations.
- `ProjectSettings`: Unity project settings.
- `docs`: open-source asset notes, third-party asset notes, architecture, and release documentation.

The repository does not directly include the full commercial asset library. Large models, textures, audio, videos, baked data, third-party Asset Store packages, and copyright/IP-related assets should be imported through external UnityPackages or licensed sources.

## Runtime Entry Points

Public demo scenes are registered in `ProjectSettings/EditorBuildSettings.asset`:

- `进入游戏.unity`
- `AR_tourNew.unity`
- `AR.unity`
- `All_Enemy.unity`
- `74层boss房.unity`
- `地狱围栏.unity`
- `第一层boss房.unity`
- `雪地场景.unity`
- `SAO_75.unity`
- `花园改.unity`
- `地狱平台.unity`
- `VR_BattleRoom.unity`

Start inspection from `进入游戏.unity` or from a specific demo scene. Scene objects bind script fields through the Unity Inspector, and many flows rely on `Tag`, `SendMessage`, prefab references, and runtime lookup.

## Top-Level Modules

### Scene Bridges

Directory:

- `Assets/SubspaceHunter/Scenes/Script`

Responsibilities:

- Activate or hide demo scene objects.
- Connect button clicks, video switching, voice display, and AR/VR module toggles.
- Provide lightweight entry points between public demo scenes and core systems.

Representative scripts:

- `Activate_all_child.cs`
- `Battle_module_AR.cs`
- `Click_2Pop_UI.cs`
- `Vedio_switch.cs`
- `Voice_Display.cs`
- `input2_activateVoice.cs`

### Battle Flow

Directory:

- `Assets/SubspaceHunter/Script/WangSiFu/Battle_manager`

Responsibilities:

- Select and spawn enemies.
- Manage single bosses, fixed-size enemy groups, and continuous spawn modes.
- Control battle timing, win/loss UI, player reset, and combat music switching.

Core scripts:

- `Battle_manager.cs`: main battle controller.
- `Battle_call.cs`: battle start or external call entry.

Main logic flow:

1. A scene event or UI action starts a battle.
2. `Battle_manager` selects enemy prefabs and spawn points from configured fields.
3. Enemy state scripts call back into the battle manager when enemies die.
4. The battle manager counts deaths, decides win/loss state, and switches UI and music.

### Enemy System and AI

Directory:

- `Assets/SubspaceHunter/Script/WangSiFu/Enemy`

Responsibilities:

- Enemy spawning, birth presentation, and portal presentation.
- HP, hit reactions, death, slicing, outline, audio, and particle feedback.
- Behavior-tree nodes, skill cooldowns, distance checks, hit interrupts, and action counters.
- Enemy weapon visibility, ranged aiming, dual-pistol control, and IK.

Key subdirectories:

- `敌人行为树/行为树脚本`: Behavior Designer task nodes.
- `敌人行为树/非行为树脚本`: hit state and global hit state.
- `出场控制`: enemy entrance timing, behavior-tree enable timing, and weapon visibility.
- `敌人CD判断`: skill cooldown checks.
- `敌人射击定位`: ranged attack aim positions.
- `角色IK控制`: enemy IK animation control.

Main logic flow:

1. `Battle_manager` or a scene generator instantiates enemies.
2. Entrance control scripts play spawn logic and enable the behavior tree.
3. Behavior-tree nodes choose actions based on player distance, cooldowns, action counts, and hit state.
4. Weapons, skills, or bullets hit enemies and call enemy state scripts to apply damage and feedback.
5. When HP reaches zero, death presentation runs and the battle manager is notified.

### Player System

Directory:

- `Assets/SubspaceHunter/Script/WangSiFu/Player_system`

Responsibilities:

- Manage player HP, magic, and runtime state.
- Provide VR player control entry points.
- Reset player state after battle end or restart.

Representative scripts:

- `Player_Manager.cs`
- `Player_managerV2.cs`
- `VRplayer_controller.cs`

### Weapons and Physical Interaction

Directories:

- `Assets/SubspaceHunter/Script/WangSiFu/Weapon`
- `Assets/SubspaceHunter/Script/WangSiFu/Weapon_physic`

Responsibilities:

- Left/right hand detection, weapon switching, and durability management.
- Item holsters, weapon following, and slot detection.
- Physical weapon grabbing, releasing, Rigidbody setup, collision checks, shield blocking, and trace detection.
- Sword-slash or blade-wave effect placement from weapon pose.

Main logic flow:

1. Hand or controller interaction triggers grabbing.
2. Physical weapon scripts set Rigidbody, parent-child relationship, and follow state.
3. Weapon collision or trace detection identifies hit targets.
4. Hit logic calls enemy hit reactions, weapon durability, audio, and VFX.

### Firearm System

Directory:

- `Assets/SubspaceHunter/Script/WangSiFu/Gun`

Responsibilities:

- Player or enemy firearm shooting.
- Bullet state, flight, hit, and lifecycle.
- Animation-event-driven shooting.
- Bullet decal or hit-effect pooling.

Main logic flow:

1. Controller input or an animation event triggers a shot.
2. The gun controller spawns or activates a bullet.
3. The bullet hits an enemy, surface, or scene object and triggers damage, decals, and audio.

### Skills and Magic

Directory:

- `Assets/SubspaceHunter/Script/WangSiFu/Skill`

Responsibilities:

- Player magic selection, preparation, cast bar, and release.
- Fire, lightning, ice, meteor, shield, healing, and other skill entry points.
- Area attacks, flying projectiles, hit explosions, and skill hints.
- Controller input mapping into the magic system.

Key scripts:

- `Player_magicController.cs`: main player magic state and release controller.
- `Magic_SkillBar_controller.cs`: magic cast-bar control.
- `Magic_controller.cs` / `Magic_choice.cs`: controller-driven magic selection.
- `Shoot_rigbody.cs` / `ele_round_realease.cs`: magic projectile and release behavior.

Main logic flow:

1. The player selects a magic type through controller input or UI.
2. The magic system checks current magic state and resource availability.
3. After cast preparation completes, it spawns a projectile, area effect, shield, or healing effect.
4. Hits against enemies or scene objects trigger damage, explosions, or state feedback.

### UI and Tutorial Systems

Directory:

- `Assets/SubspaceHunter/Script/WangSiFu/UI`

Responsibilities:

- Main menu, hint boxes, item menus, info stream, tutorial UI, and player HUD.
- HP animation, damage feedback, hand-tracking UI, and UI-follow behavior.
- UI panel open/close, fade, and positioning.

Main logic flow:

1. Controller, hand-tracking, or button input triggers a UI event.
2. UI scripts open panels, generate hints, or update HUD elements.
3. Some UI events continue into scene switching, item generation, battle start, or system settings.

### Audio System

Directories:

- `Assets/SubspaceHunter/Script/WangSiFu/Music_setPlay`
- `Assets/SubspaceHunter/Script/WangSiFu/Sound_controller`
- `Assets/SubspaceHunter/Script/WangSiFu/System/声音`

Responsibilities:

- Background music playback and switching.
- Footsteps, swing sounds, animation-event audio, and system volume settings.
- Connect AudioSource state with battle, animation, and UI state.

### Scene Transition and Flow

Directories:

- `Assets/SubspaceHunter/Script/WangSiFu/Scence_change`
- `Assets/SubspaceHunter/Script/WangSiFu/Scene`
- `Assets/SubspaceHunter/Script/WangSiFu/Keyboard`

Responsibilities:

- Game entry, loading screens, button navigation, and VR keyboard input.
- Scene transitions between lobby, tutorial, AR, VR, boss rooms, and demo scenes.

### Config Tables and Local Data

Directories:

- `Assets/SubspaceHunter/Script/excel`
- `Assets/SubspaceHunter/Script/WangSiFu/Sqlite`

Responsibilities:

- The `excel` directory includes Luban/Excel generated configuration access and related generated data structures.
- The `Sqlite` directory wraps local SQLite create, insert, select, update, delete, alter, and test entry points.

Notes:

- `Generate` and `LubanLib` are closer to generated code or base libraries and generally should not be edited manually.
- Database logic may involve local file paths and runtime environment assumptions. Before open-source releases, continue checking for sensitive data, private paths, or test data.

## Dependencies and External Systems

Main Unity package dependencies are declared in `Packages/manifest.json`, including:

- XR: `com.unity.xr.management`, `com.unity.xr.oculus`, `com.unity.xr.openxr`
- UI: `com.unity.ugui`, `com.unity.textmeshpro`
- Animation: `com.unity.animation.rigging`, `com.unity.timeline`
- Camera: `com.unity.cinemachine`
- Rendering and performance: `com.unity.burst`, `com.unity.collections`, `com.unity.postprocessing`

The code also shows third-party or external dependency characteristics such as Behavior Designer, EzySlice, SQLite, and Oculus/Meta SDK usage. Confirm authorization item by item before commercial use or redistribution, using `docs/THIRD_PARTY_ASSET_PACKAGES.zh-CN.md` as the current audit reference.

## Code Comment Strategy

To make the project easier for Chinese and English contributors to understand, existing feature scripts now include a bilingual file-level note:

- `模块 / Module`: the script's feature module.
- `功能 / Purpose`: Chinese responsibility summary.
- `English`: English responsibility summary.

Content intentionally not bulk-edited:

- Generated configuration code: `Assets/SubspaceHunter/Script/excel/Generate`
- Luban base library: `Assets/SubspaceHunter/Script/excel/LubanLib`
- Old scripts that are currently deleted locally and not part of this public commit

Maintenance recommendations:

1. Keep the same bilingual file header for new feature scripts.
2. Add local comments for complex algorithms, cross-object communication, Inspector binding requirements, and asynchronous flows.
3. Do not hand-edit generated code for comments; document behavior in wrapper code or docs instead.

## Recommended Reading Order

1. `README.en.md`
2. `docs/ARCHITECTURE.md`
3. `docs/PUBLIC_DEMO_DEPENDENCIES.zh-CN.md`
4. `docs/THIRD_PARTY_ASSET_PACKAGES.zh-CN.md`
5. Open Unity and inspect scene bindings under `Assets/PublicDemo/PublicScenes`.
