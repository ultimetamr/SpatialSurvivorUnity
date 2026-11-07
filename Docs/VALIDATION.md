2026-11-06 最终图标验收：用户卸载旧包后重新构建并全新安装 2.2.3（code 15），设备版本与回读 APK 哈希核对通过。用户佩戴确认“已显示游戏图标”。图标问题验收通过。构建同时隔离本工程 Gradle 临时目录，避免共享 daemon 使用其他工程已删除的临时路径。完整证据见 Validation/unity6-flat-icon-summary.json；下文保留排查历史。

2026-11-06 最新图标修正 2.2.3（code 15）：构建成功，应用/启动入口/所有密度的图标引用和圆形图标 ID 已核对为原游戏图标；两个平面 PNG 与原图逐像素一致。设备重新连接后已覆盖安装 2.2.3，回读哈希一致；用户仍看到 Unity 图标，已重启 PICO 桌面，应用列表验收未完成。详见 UNITY6_APP_ICON_FIX.md、Validation/unity6-flat-icon-summary.json。

2026-11-06 最新图标修正：2.2.2（code 14）构建、覆盖安装及设备版本核对通过，回读 APK 哈希一致；APK 12 个自适应图标图层已与原游戏图标比对，并目视检查提取的前景。用户反馈应用列表仍为 Unity 图标；本条为 2.2.2 历史验证，见下方后续修正。本次仅调整编辑器图标配置，未重跑完整游戏测试。详见 UNITY6_APP_ICON_FIX.md、Validation/unity6-icon-summary.json。

2026-11-06 最新检查：Unity 6.6（6000.6.8f1）构建 2.2.1（code 13）成功，95 项测试全部通过，包括前景几何遮挡下射线完整可见的渲染回归。真机覆盖安装成功，回读 APK 的 SHA256 与产物一致，应用已进入前台运行（PID 24774），崩溃缓冲为空。设备有眼动服务不可用警告，未观察到关键托管异常。用户已佩戴确认 2.2.1“射线完整，点击正常”；近战/手枪完整实玩、长期舒适度及性能仍待验收，设备截图功能不可用。详见 UNITY6_POINTER_FIX.md、Validation/unity6-device-summary.json。

# Unity 项目验证

2026-11-06：工程已升级至 Unity 6.6（6000.6.8f1），重新编译和 94 项完整回归全部通过。见 UNITY6_UPGRADE.md、Validation/unity6-tests.xml 和 Validation/unity6-summary.json。后续已完成 Unity 6 Android 构建与真机前台运行，见本页顶部最新记录。

2.2.0（code 12）：连续挥剑、跟踪连续性和两把枪械。94 项测试全部通过，Android 构建成功，已安装至真机；首次部署时处于休眠；后续 Unity 6 构建已唤醒并进入前台，用户确认可点击，射线部分可见触发 2.2.1 修正。完整结果见 Validation/physical-weapons-tests.xml；实际安装/启动状态见 Validation/physical-weapons-summary.json。操作和边界见 PHYSICAL_WEAPONS.md。

最新功能版：2.1.0（code 11），39 技能手册、7 种怪物图鉴、6 把手持武器、2 类飞行怪、金币永久商店及 v3 存档升级。88 项回归全部通过，Android 构建成功并已安装至 PICO 真机；2026-11-06 已重新安装并确认进入真机前台，右手柄有效，技能手册记录 3 次点击且无漏点；完整战斗及性能仍待佩戴验收。完整回归结果见 Validation/armory-tests.xml；实际渲染与部署状态见 ARMORY_AND_BESTIARY.md 和 Validation/armory-summary.json。

最新修正：2.0.4（code 10），射线平滑、按钮边缘有限容差、扳机按下/松开迟滞与输入诊断。77 项回归通过。修复前按钮边缘约 6 毫米偏移漏点已复现，修复后通过；结果见 Validation/pointer-stability-tests.xml，说明及部署状态见 POINTER_STABILITY_FIX.md 与 Validation/pointer-stability-summary.json。

最新修正：2.0.3（code 9），关闭真实房间深度遮挡、五种怪物整体缩小至一半、持续绕行避让。73 项回归通过，包含 UI/模型无遮挡实际渲染、混合波次持续移动与攻击、冻结和暂停。结果见 Validation/visibility-movement-tests.xml，说明见 VISIBILITY_MOVEMENT_FIX.md；下文为历史记录。

最新修正：2.0.2（code 8），怪物避让、间距分离与刷怪空位检查。72 项游戏测试通过，包含 9 项拥挤/避让回归；结果见 Validation/crowd-tests.xml，说明及部署状态见 ENEMY_CROWD_FIX.md 与 Validation/crowd-summary.json。

最新修正：2.0.1（code 7），仅调整战斗 HUD 的距离、朝向及跟随方式。63 项回归通过，包含 5 项 HUD 空间姿态测试；结果见 Validation/ui-placement-tests.xml，设计和实际部署状态见 UI_PLACEMENT_FIX.md 与 Validation/ui-placement-summary.json。

当前版本：2026-11-05 魔法生物美术重制 **2.0.0（code 6）**。58 项测试通过，APK 构建成功，已覆盖安装到 PICO 真机。模型、动画、UI 和编辑器画面检查见 ART_REMAKE.md；最新 APK 校验值及设备状态见 Validation/art-remake-summary.json。头显唤醒后已确认 2.0.0 进入前台（PID 21274），右手柄有效，点击“开始生存”后进入 Playing。模型观感、动画流畅度及 MR 融合仍需佩戴验收。下文为历史版本记录。

后续更新：2026-11-05 的 UI 点击修复版为 1.3.1（code 5），51 项回归通过，已安装到真机。当前启动/人工点击状态见 UI_INPUT_FIX.md 及 Validation/ui-input-fix-summary.json。以下内容记录此前 1.3.0 构建的验证。

验证日期：2026-11-05。Unity 2022.3.62f3c1，嵌入 PICO Unity SDK 6.1.1，应用 1.3.0（code 4）。

## 本次修复

此前版本的 MR 只有基础透视和网格回调，未提供系统扫描入口、授权结果处理、启动失败重试、房间状态和可检查的场景 XR Rig；场景捕获功能也未开启。本次实现详见 MR_SETUP.md。

现在主菜单和暂停菜单均提供 MR 设置，支持系统扫描/重扫、网格预览和授权/服务重试；加入真实房间表面读取、深度遮挡、碰撞、定位恢复及手部追踪骨架反馈。XR 相机和 MR 组件已保存至 Unity 场景。

## 已完成验证

- 游戏测试程序集共 45 项通过，0 失败、0 跳过。完整 NUnit 结果：Validation/game-tests.xml；摘要：Validation/game-tests-summary.json。
- MR 状态门控：初始化、空间授权、读取房间、扫描、拒绝和失败阶段，即使缓存地面仍有效，也不能推进战斗；恢复需要 MR、定位和导航同时就绪。
- 凹房间边界三角剖分：正反点序、重复闭合点、连续重复点、共线点及退化多边形；不填平凹边界。
- 空间网格：增加、覆盖更新、共享跟踪原点变换、非法三角形拒绝、删除及导航失效；MeshCollider 使用更新后的地面高度。
- 渲染测试：真实表面深度可遮挡后方虚拟物体；关闭预览时保持透明背景；开启预览时可显示扫描表面。
- 菜单测试：MR 设置显示房间刷新/扫描、授权重试和网格入口，能返回主菜单。MR 页面截图已检查：Validation/mr-settings.png。
- 原有玩法和存档回归继续通过：9 类武器、39 张卡牌、6 种进化、怪物出生与家具避让、经验吸附、选卡暂停、Boss、结算、永久强化和独立存档恢复。
- 菜单、选卡、Boss/HUD 截图在编辑器模拟房间生成。测试加速时间并设置 elapsed=600 触发 Boss，不代表十分钟佩戴实玩。
- Android APK 编译结果、版本、文件大小和 SHA-256：Validation/mr-build-summary.json 与 Validation/apk-sha256.txt。
- 合并 Manifest 已核对空间数据/手部追踪权限，以及 enable_vst=1、enable_mesh_anchor=1、enable_scene_anchor=1。

## 复现

关闭本工程编辑器后执行 Tools/Run-GameTests.ps1 -UnityEditor "Unity.exe 的完整路径"。该脚本只运行 SpatialSurvivor.Tests；后台缺少 ALLUSERSPROFILE 时仅为当前进程补齐，不修改系统变量。

Unity 菜单 SpatialSurvivor → Build Android APK 生成 ARM64 / IL2CPP / OpenGLES3 开发构建，使用调试签名。

## 真机验证尚未完成

pico-cli device list --format json 返回空设备列表，未向任何设备安装或启动本版 APK。环境检查未改动全局 Spatial/Kotlin AI 工具配置。具体状态保存在 Validation/mr-device-status.json。

系统真实透视、原生房间扫描、扫描数据与真实家具对齐、双眼遮挡、手势/手柄切换、设备定位恢复、触觉及连续运行性能仍需连接并佩戴 PICO 头显验收；编辑器回归和 Manifest 开关不能替代上述验收。具体真机步骤见 MR_SETUP.md。

原模型、字体和 SDK 权利说明见 README 与 SDK_PATCHES.md。

