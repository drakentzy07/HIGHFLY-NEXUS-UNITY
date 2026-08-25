# 第三方资产包、插件和库授权清单

这份文档记录当前项目审计中发现的第三方资产包、插件、SDK 和库，用于提醒商业化使用前逐项确认授权。

这不是法律意见，也不是任何第三方资产的授权证明。是否可以用于商业项目、是否允许再分发、是否允许随开源仓库或外部 `.unitypackage` 一起发布，必须以资源作者、供应商、官方 SDK 或对应许可证的最新条款为准。

## 商业化前检查原则

1. 第三方资源包、插件、SDK、字体、音频、模型、特效和代码库都按独立授权对象处理。
2. 已购买或能在本地使用，不等于可以把原始文件重新分发到 GitHub、外部资源包或安装包中。
3. 带 `LICENSE`、`NOTICE`、`README`、PDF 文档或供应商说明的目录，应保留原始授权文件，并按条款处理署名、notice、二进制分发和修改要求。
4. 商业化项目应由使用者自行从官方来源购买、下载或导入所需第三方依赖，避免复用来源不清的旧副本。
5. 授权范围未确认前，不要把第三方资产归入本项目的“原创、可商用资产”。

## 已发现授权或说明文件痕迹的目录

这些目录在本地项目中发现了 `LICENSE`、`NOTICE`、`README`、PDF 或供应商说明文件。它们需要作为独立依赖处理。

| 目录 | 当前发现的依据 | 商业化前处理 |
| --- | --- | --- |
| `Assets/Oculus` | Oculus/Meta SDK 中发现 `LICENSE`、`NOTICE` 和第三方 notice 文件 | 以 Meta 官方 SDK 条款为准，确认 SDK、示例资源和第三方 notice 的使用范围 |
| `Assets/Plugins/Demigiant/DOTween` | 发现 `readme.txt` | 确认 DOTween 版本和许可证，再决定是否随项目分发 |
| `Assets/Plugins/RootMotion/FinalIK` | 发现 `FinalIK ReadMe.rtf` | 按 RootMotion/供应商授权处理；若为商业插件，使用者需自行取得授权 |
| `Assets/Plugins/RootMotion/PuppetMaster` | 发现 `PuppetMaster ReadMe.rtf` | 按 RootMotion/供应商授权处理；若为商业插件，使用者需自行取得授权 |
| `Assets/MagicaCloth2` | 发现 `Readme.txt`、`Readme_jpn.txt` | 确认购买状态、商业使用范围和再分发限制 |
| `Assets/CurvedUI` | 发现 `CurvedUI_README.pdf` | 确认供应商授权和发布限制 |
| `Assets/Bakery`、`Assets/Editor/x64/Bakery` | 发现 Bakery 相关插件目录和 `xatlas-license.txt` | 确认 Bakery 插件授权；插件二进制和附带第三方组件分别审查 |
| `Assets/DynamicBone` | 发现 `ReadMe.txt` | 确认商业使用和再分发授权 |
| `Assets/Resources/XWeaponTrail` | 发现 `readme.txt` | 按第三方拖尾资源/插件处理，先确认授权 |
| `Assets/JMO Assets` | Cartoon FX 目录中发现多份 readme 和 Kino Bloom license | 按 JMO/Cartoon FX 及其附带第三方组件授权分别处理 |
| `Assets/LukePeek` | Snow VFX 中发现 `README.txt` | 确认特效资源的商业使用和再分发范围 |
| `Assets/ARPG Effects` | 发现 `Readme.txt` | 按第三方特效包处理，先确认授权 |
| `Assets/Loot Beams` | 发现 `Readme.txt` | 按第三方特效资源处理，先确认授权 |
| `Assets/OrdosFX` | Magic Slashes FX 中发现 readme PDF | 按第三方特效包处理，先确认授权 |
| `Assets/OrdossFX` | LootDropFX 中发现 readme PDF | 按第三方特效包处理，先确认授权 |
| `Assets/SpearAndHalberdAnimset` | 发现 `readme.txt` | 按第三方动画资源处理，先确认授权 |
| `Assets/YummyGames` | Zombie Animations Mocap 中发现 readme | 按第三方动画资源处理，先确认授权 |
| `Assets/Piloto Studio` | 发现 Readme 目录和说明脚本 | 按供应商资源说明确认授权 |

## 之前依赖审计中标记为第三方或需单独导入的目录

这些目录在 PublicDemo 依赖审计或公开资源清单中已被标记为官方 SDK、插件、商业资源包、第三方素材包或授权来源待确认内容。即使当前表中没有列出明确许可证文件，也应在商业化前逐项核验来源。

| 目录 | 类型判断 | 建议 |
| --- | --- | --- |
| `Assets/TextMesh Pro` | 官方/第三方字体与 UI 相关资源 | 确认 Unity 官方包、字体和示例资源使用范围 |
| `Assets/_MK` | MK Toon 相关资源 | 确认 shader 包授权和发布方式 |
| `Assets/PureNature` | 第三方环境资源包 | 从授权来源导入，不把未确认资源归入原创资产 |
| `Assets/Weapons Pro Pack` | 第三方武器资源包 | 商业化前确认购买和分发限制 |
| `Assets/Perfect RPG MMO 3D Effect VFX Pack` | 第三方 VFX 资源包 | 商业化前确认购买和分发限制 |
| `Assets/NatureManufacture Assets` | 第三方环境资源包 | 商业化前确认购买和分发限制 |
| `Assets/SimpleLowPolyNature` | 第三方自然环境资源 | 商业化前确认来源和授权 |
| `Assets/Suntail Village` | 第三方场景资源 | 商业化前确认来源和授权 |
| `Assets/Humanoid Static Poses` | 第三方姿态/动画资源 | 按资源作者条款处理 |
| `Assets/Vefects` | 第三方特效资源 | 按资源作者条款处理 |
| `Assets/SubspaceHunter/3rd` | 混合第三方目录 | 按子包拆分后逐项核验授权 |
| `Assets/SubspaceHunter/Animation` | 含动画包说明文档的混合目录 | 区分原创动画和第三方动画后再发布 |
| `Assets/SubspaceHunter/Font` | 字体和 SDF 资产 | 逐项确认字体授权，公开版优先使用可分发字体 |

## 旧审计记录中出现但当前工作区未复核到的目录

早期公开资源清单曾记录下列第三方库目录。当前工作区没有复核到对应目录；如果后续重新导入，仍需按原许可证处理。

| 目录 | 旧审计记录 | 建议 |
| --- | --- | --- |
| `Assets/UniGLTF` | 旧清单记录到 `LICENSE.md`、`README.md` | 重新导入后核对版本、许可证和上游分发方式 |
| `Assets/VRM` | 旧清单记录到 `LICENSE.md`、`README.md`、MToon license | 重新导入后核对 VRM、MToon 及附带资源授权 |

## 与开源发布的关系

- 本项目开源不自动覆盖第三方依赖的授权。
- 第三方原始资产、插件二进制、商业 Asset Store 包和来源不明资源，不应因为项目开源而默认再分发。
- 若某个公开 Demo 场景依赖第三方资产，推荐提供依赖说明或官方导入步骤，再让使用者自行取得授权。
- 若某个资源最终要放入公开仓库或外部资源包，应先确认其许可证允许该发布方式。

## 后续维护

新增或替换第三方资源时，至少记录：

1. 资源或插件名称。
2. 本地目录。
3. 来源页面或供应商。
4. 版本号。
5. 许可证或购买凭据位置。
6. 是否允许商业使用。
7. 是否允许公开再分发。
8. 是否需要在 README、notice 或发布包中保留署名和许可证文本。
