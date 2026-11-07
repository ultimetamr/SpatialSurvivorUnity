# SpatialSurvivor Unity 独立重写版

参考 E:\DEMO\SpatialSurvivor 的玩法和数值逻辑，用 Unity/C# 独立实现的 PICO MR 肉鸽游戏。运行、保存和打包均不依赖安卓工程；2.0.0 已按“精致卡通 · 魔法生物”重做角色、动画、技能和全流程 UI；详见 Docs/ART_REMAKE.md。2.0.1 调整战斗 HUD 的距离、倾角与跟随方式，见 Docs/UI_PLACEMENT_FIX.md。2.0.3 关闭真实房间深度遮挡，怪物缩小至一半，拥挤时持续绕行；见 Docs/VISIBILITY_MOVEMENT_FIX.md。2.0.4 修正射线抖动、按钮边缘漏点与扳机阈值跳变，见 Docs/POINTER_STABILITY_FIX.md。

2.1.0 新增 RoomSiege 式技能/怪物介绍、六把可永久购买并装备的手持武器、两种飞行怪和独立局外金币。详见 Docs/ARMORY_AND_BESTIARY.md。

**2.2.0** 加入剑刃连续挥击、手柄短时失跟显示保留，以及免费星火手枪和符文霰弹枪。商店共八把武器，两把枪在第二页；详见 Docs/PHYSICAL_WEAPONS.md。

**2.2.1（code 13）** 已使用 Unity 6 构建并安装到 PICO 真机，调整射线遮挡与菜单表面的视觉终点；95 项测试通过。用户已在头显确认射线完整、点击正常，见 Docs/UNITY6_POINTER_FIX.md。工程升级与备份说明见 Docs/UNITY6_UPGRADE.md。

当前构建 **2.2.3（code 15）** 统一应用、启动入口和备用平面图标，卸载旧版后已重新构建并全新安装，设备版本、APK 哈希和图标资源核对通过；用户已确认应用列表显示游戏图标。见 Docs/UNITY6_APP_ICON_FIX.md。

## 打开与操作

1. Unity Hub 添加本目录，使用 **Unity 6.6 · 6000.6.8f1**。也可运行 Tools/Open-Unity6.ps1 直接打开。Android 打包需安装 Android Build Support（SDK、NDK、OpenJDK）。
2. 打开 Assets/Scenes/SpatialSurvivor.unity，点击 Play。
3. 编辑器：WASD 移动、按住鼠标右键转向、鼠标确认；Enter 开局、1–4 选卡、Escape 暂停。
4. PICO：真实走位、手势射线捏合确认，手柄扳机备用。HUD 提供手势可点的暂停按钮；手柄菜单键也可暂停。
5. 主菜单打开「MR 设置」，授权空间数据后使用「扫描 / 重扫房间」扫描地面、墙壁、天花板和家具；支持网格预览、状态诊断和失败重试。完整说明见 Docs/MR_SETUP.md。没有有效房间网格时不会刷怪。定位丢失自动停战，恢复稳定定位后继续。

6. 主菜单可查看技能手册、生物图鉴和武器商店。金币永久解锁手持武器，下局携带；按住手柄扳机或捏合施法，短剑可挥击。

## 已实现的游戏内容

- 9 类武器、39 张强化卡、6 种进化、稀有度抽卡与保底、三/四选一、刷新、永久强化及结算。
- 两分钟波次、四类地面普通怪、两类飞行怪、十分钟 Boss 和范围攻击预警。
- 七个顶点绘色骨骼角色，每个包含待机、走路、攻击、受击、出生与死亡六段动画；新武器、冰晶、熔岩种子、奥术核心与动态魔法环、闪电和脉冲效果。
- 原版 14 把飞剑、48 个经验晶石、18 个地面怪、6 个飞行怪和 1 个 Boss（普通怪同时存活上限 18） 的固定对象池；关节受击与死亡动作、升级魔法环、胜利光效。
- 房间碰撞、网格导航、天花板掉落、无房间深度遮挡；模型资源在开局前预热。
- 血条、经验条、Boss 血条、倒计时、新魔法技能图集、受伤红闪、完整分页强化历史和永久强化效果预览。
- 启动图标已沿用原版。主菜单及暂停菜单可独立开关音效与手柄触觉，并保存设置。
- 新增 13 类合成事件音效、10 个声音播放槽和有节流的手柄触觉。原安卓工程未包含音频文件或触觉代码，这部分为 Unity 版补充。
- Unity 独立本地进度：临时文件完成后替换主存档，主文件损坏时恢复备份；没有旧安卓存档也可直接开局。
- 晶石进入拾取范围后持续加速吸附，对象池合并保留每次掉落的经验取整；暂停、选卡和跟踪丢失时战斗及短时特效同步停止。
- 可选工具：如需保留旧安卓永久进度，可手动导入 XML，详见 Docs/PROGRESS_IMPORT.md；启动时不会自动导入。

## Android 构建

Unity 菜单 SpatialSurvivor → Build Android APK，输出 **Builds/SpatialSurvivorUnity.apk**。版本 2.2.3（code 15），包名 com.example.spatialsurvivor.unity，ARM64 / IL2CPP / OpenGLES3。开发构建使用调试签名。

PICO Unity SDK **6.1.1** 和依赖已嵌入 Packages，可从本机工程继续离线开发。复制工程时保留 Assets、Packages、ProjectSettings。

## 验证范围

回归结果、截图及 APK 校验值见 Docs/VALIDATION.md 和 Docs/Validation。编辑器测试使用模拟房间；真实 PICO 上的扫描、透视、手势、触觉、遮挡和性能仍需佩戴设备验收。

Unity 使用世界空间 Canvas 和缓存网格导航重写对应平台能力，不直接复用 Android Spatial ECS / SpatialUI 二进制。存档通过显式导入迁移，不跨应用自动读取私有目录。

各包保留原许可证；原模型保留原项目权利状态。中文字体来自本机 SimHei，分发前仍需确认原模型与字体授权。SDK 本地兼容修改见 Docs/SDK_PATCHES.md。

## 代码入口

- Assets/Scripts/Core/SurvivorRules.cs：卡牌、数值、永久强化。
- Assets/Scripts/Core/SurvivorProgressStore.cs：Unity 本地存档与备份恢复；LegacyProgressMigration.cs 为可选旧存档工具。
- Assets/Scripts/Gameplay/SurvivorGame.cs：战斗、波次、对象池；SurvivorExperienceGameplay.cs 实现晶石吸附与升级；SurvivorCombatPresentation.cs 管理战斗特效时钟。
- Assets/Scripts/Gameplay/SurvivorSkillVisuals.cs：新技能模型、法术动画与特效池。
- Assets/Scripts/Gameplay/SurvivorFeedback.cs：音效与触觉。
- Assets/Scripts/Platform：PICO 输入、透视、房间网格与导航。
- Assets/Scripts/UI/SurvivorUI.cs 与 SurvivorUILayout.cs：射线输入、手册风格菜单和紧凑 HUD。

SpatialSurvivor → Run Game Tests 可复现游戏回归测试，场景测试会在 Docs/Validation 生成菜单、选卡、Boss 与技能截图。

关闭本工程编辑器后，也可运行 Tools/Run-GameTests.ps1 -UnityEditor "Unity.exe 的完整路径"，在后台只执行游戏自身的 72 项回归用例。结果输出到 Logs/game-tests.xml。


