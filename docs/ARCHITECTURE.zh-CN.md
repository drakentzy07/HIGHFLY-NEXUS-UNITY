# SubspaceHunter-SAO 项目架构与功能逻辑

本文档说明公开仓库中可见的 Unity 项目结构、核心功能模块和运行逻辑。它用于帮助贡献者理解代码组织方式；实际可运行内容还依赖外部 UnityPackage、授权资源包和本地导入的第三方依赖。

## 项目边界

公开仓库主要包含：

- `Assets/PublicDemo/PublicScenes`：公开 Demo 场景文件。
- `Assets/SubspaceHunter/Script`：主要运行时代码。
- `Assets/SubspaceHunter/Scenes/Script`：公开场景中使用的桥接脚本。
- `Packages`：Unity 包依赖声明。
- `ProjectSettings`：Unity 项目设置。
- `docs`：开源资源、第三方资源、架构和发布说明。

仓库不直接包含完整商业资源库。大体量模型、贴图、音频、视频、烘焙数据、第三方 Asset Store 包和版权/IP 相关素材，应通过外部 UnityPackage 或授权来源导入。

## 运行入口

公开 Demo 场景在 `ProjectSettings/EditorBuildSettings.asset` 中注册，当前包含：

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

推荐从 `进入游戏.unity` 或具体 Demo 场景开始检查。场景内对象通过 Unity Inspector 绑定脚本字段，很多逻辑依赖 `Tag`、`SendMessage`、预制体引用和运行时查找。

## 顶层模块

### 场景桥接

目录：

- `Assets/SubspaceHunter/Scenes/Script`

职责：

- 激活或隐藏 Demo 场景对象。
- 连接按钮点击、视频切换、语音显示和 AR/VR 模块开关。
- 作为公开 Demo 场景和核心系统之间的轻量入口。

代表脚本：

- `Activate_all_child.cs`
- `Battle_module_AR.cs`
- `Click_2Pop_UI.cs`
- `Vedio_switch.cs`
- `Voice_Display.cs`
- `input2_activateVoice.cs`

### 战斗流程

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Battle_manager`

职责：

- 选择并生成敌人。
- 管理单体 Boss、固定数量敌人和持续刷怪模式。
- 控制战斗计时、胜负 UI、玩家状态重置和战斗音乐切换。

核心脚本：

- `Battle_manager.cs`：战斗主控。
- `Battle_call.cs`：战斗启动或外部调用入口。

主要逻辑流：

1. 场景或 UI 触发战斗开始。
2. `Battle_manager` 根据配置选择敌人预制体和生成点。
3. 敌人死亡时通过敌人状态脚本回调战斗管理器。
4. 战斗管理器统计死亡数量、判断胜负、切换 UI 和音乐。

### 敌人系统与 AI

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Enemy`

职责：

- 敌人生成、出生表现和传送门表现。
- 生命值、受击、死亡、切割、描边、音效和粒子反馈。
- 行为树节点、技能冷却、距离判断、受击打断和动作计数。
- 敌人武器显示、远程射击定位、双枪控制和 IK。

关键子目录：

- `敌人行为树/行为树脚本`：Behavior Designer 行为树任务节点。
- `敌人行为树/非行为树脚本`：受击状态和全局命中状态。
- `出场控制`：敌人出场阶段、行为树启用和武器显隐。
- `敌人CD判断`：技能冷却判断。
- `敌人射击定位`：远程攻击瞄准位置。
- `角色IK控制`：敌人 IK 动画控制。

主要逻辑流：

1. `Battle_manager` 或场景生成器实例化敌人。
2. 敌人出生控制脚本播放出场逻辑并启用行为树。
3. 行为树节点根据玩家距离、技能冷却、动作次数和受击状态选择行为。
4. 武器、技能或子弹命中敌人时，敌人状态脚本扣血并触发反馈。
5. 生命值归零后执行死亡表现，并通知战斗管理器更新战斗进度。

### 玩家系统

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Player_system`

职责：

- 玩家生命、魔力或运行状态管理。
- VR 玩家控制入口。
- 战斗结束或重开时重置玩家状态。

代表脚本：

- `Player_Manager.cs`
- `Player_managerV2.cs`
- `VRplayer_controller.cs`

### 武器与物理交互

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Weapon`
- `Assets/SubspaceHunter/Script/WangSiFu/Weapon_physic`

职责：

- 左右手持有检测、武器切换和耐久管理。
- 道具收纳、武器跟随和插槽检测。
- 物理武器抓取、释放、刚体添加、碰撞检测、盾牌阻挡和轨迹检测。
- 通过武器姿态生成剑气或斩击特效。

主要逻辑流：

1. 手部或控制器触发抓取。
2. 物理武器脚本设置刚体、父子关系和跟随状态。
3. 武器碰撞或轨迹检测识别命中对象。
4. 命中后调用敌人受击、武器耐久、音效和特效逻辑。

### 枪械系统

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Gun`

职责：

- 玩家或敌人的枪械射击。
- 子弹状态、飞行、命中和生命周期。
- 动画事件驱动的射击。
- 弹痕或命中特效对象池。

主要逻辑流：

1. 控制器输入或动画事件触发射击。
2. 枪械控制器生成或激活子弹。
3. 子弹命中敌人、表面或场景对象后触发伤害、弹痕和音效。

### 技能与魔法系统

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Skill`

职责：

- 玩家魔法选择、准备、读条和释放。
- 火、电、冰、陨石、盾牌、治疗等技能入口。
- 范围攻击、飞行弹体、命中爆炸和技能提示。
- 控制器输入到魔法系统的映射。

关键脚本：

- `Player_magicController.cs`：玩家魔法状态和释放主控。
- `Magic_SkillBar_controller.cs`：魔法准备读条。
- `Magic_controller.cs` / `Magic_choice.cs`：控制器魔法选择。
- `Shoot_rigbody.cs` / `ele_round_realease.cs`：魔法弹体与释放。

主要逻辑流：

1. 玩家通过控制器或 UI 选择魔法类型。
2. 魔法系统检查当前魔法和资源是否可用。
3. 读条完成后生成弹体、范围效果、护盾或治疗效果。
4. 命中敌人或场景时触发伤害、爆炸或状态反馈。

### UI 与教程系统

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/UI`

职责：

- 主菜单、提示框、物品菜单、信息流、教程 UI 和玩家 HUD。
- 血条动画、受击提示、手势追踪 UI、UI 跟随玩家。
- UI 面板打开/关闭、渐隐和定位。

主要逻辑流：

1. 控制器、手势追踪或按钮触发 UI 事件。
2. UI 脚本打开面板、生成提示或更新 HUD。
3. 部分 UI 事件继续调用场景切换、道具生成、战斗启动或系统设置。

### 音频系统

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Music_setPlay`
- `Assets/SubspaceHunter/Script/WangSiFu/Sound_controller`
- `Assets/SubspaceHunter/Script/WangSiFu/System/声音`

职责：

- 背景音乐播放与切换。
- 脚步声、挥砍声、动画事件音效和系统音量设置。
- 将 AudioSource 与战斗、动画、UI 状态联动。

### 场景切换与流程

目录：

- `Assets/SubspaceHunter/Script/WangSiFu/Scence_change`
- `Assets/SubspaceHunter/Script/WangSiFu/Scene`
- `Assets/SubspaceHunter/Script/WangSiFu/Keyboard`

职责：

- 进入游戏、加载界面、按钮跳转和 VR 键盘输入。
- 从大厅、教程、AR、VR、Boss 房等场景之间切换。

### 配置表与本地数据

目录：

- `Assets/SubspaceHunter/Script/excel`
- `Assets/SubspaceHunter/Script/WangSiFu/Sqlite`

职责：

- `excel` 目录包含 Luban/Excel 生成配置读取入口和部分生成数据结构。
- `Sqlite` 目录封装本地 SQLite 增删改查、建表和测试入口。

注意：

- `Generate` 和 `LubanLib` 更接近生成代码或基础库，通常不建议手工修改。
- 数据库逻辑涉及本地文件路径和运行环境，开源前应继续检查是否有敏感数据、私有路径或测试数据。

## 依赖与外部系统

主要 Unity 包依赖见 `Packages/manifest.json`，包括：

- XR：`com.unity.xr.management`、`com.unity.xr.oculus`、`com.unity.xr.openxr`
- UI：`com.unity.ugui`、`com.unity.textmeshpro`
- 动画：`com.unity.animation.rigging`、`com.unity.timeline`
- 摄像机：`com.unity.cinemachine`
- 渲染与性能：`com.unity.burst`、`com.unity.collections`、`com.unity.postprocessing`

代码中还出现了第三方或外部依赖特征，例如 Behavior Designer、EzySlice、SQLite、Oculus/Meta SDK 等。商业化或再分发前应按 `docs/THIRD_PARTY_ASSET_PACKAGES.zh-CN.md` 逐项确认授权。

## 代码注释策略

为了便于中英文贡献者理解项目，现存功能脚本顶部统一增加了双语文件说明：

- `模块 / Module`：脚本所属功能模块。
- `功能 / Purpose`：中文职责说明。
- `English`：英文职责说明。

未批量修改的内容：

- 自动生成配置代码：`Assets/SubspaceHunter/Script/excel/Generate`
- Luban 基础库：`Assets/SubspaceHunter/Script/excel/LubanLib`
- 当前本地已删除、未参与公开提交的旧脚本

后续维护建议：

1. 新增功能脚本时保留同样格式的双语文件头。
2. 对复杂算法、跨对象通信、Inspector 绑定约束和异步流程，再补充局部注释。
3. 不要在生成代码中手写注释；需要说明时写到外层文档或生成模板。

## 推荐阅读顺序

1. `README.md`
2. `docs/ARCHITECTURE.zh-CN.md`
3. `docs/PUBLIC_DEMO_DEPENDENCIES.zh-CN.md`
4. `docs/THIRD_PARTY_ASSET_PACKAGES.zh-CN.md`
5. 进入 Unity，从 `Assets/PublicDemo/PublicScenes` 中的场景开始查看 Inspector 绑定。
