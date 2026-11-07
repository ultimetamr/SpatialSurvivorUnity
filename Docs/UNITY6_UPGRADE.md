# Unity 6 升级

升级日期：2026-11-06。主工程：E:\DEMO\SpatialSurvivorUnity。

编辑器改为本机安装的 Unity 6.6，6000.6.8f1（237b6593792b），路径 D:\Would\Unity\6000.6.8f1\Editor\Unity.exe。

## 打开

通过 Unity Hub 添加主工程并选择 6000.6.8f1，或运行 Tools/Open-Unity6.ps1。主场景是 Assets/Scenes/SpatialSurvivor.unity。编辑器模拟操作保持 WASD、鼠标右键转向、Enter 开局、Escape 暂停。

## 已处理的兼容问题

- 原 ProjectVersion 使用 2022.3.62f3c1，本机 Unity 6 包管理器拒绝其地区版本后缀；版本标识更新为当前实际安装编辑器。
- 旧嵌入 uGUI/TextMeshPro、输入、数学、XR 通用包和测试框架替换为该 Unity 6 安装自带版本。数学模块不再重复编译旧 Unity.Mathematics 类型；输入系统和 XR 工具包不再引用 Unity 6 已移除的编辑器 API。
- 依赖版本：Input System 1.20.1、Mathematics 1.4.0（模块兼容包）、uGUI 2.6.0、TextMeshPro 5.0.0（兼容包）、Test Framework 1.8.0、NUnit 2.1.0、XR Management 4.7.0、XR Core Utils 2.6.0、XR Legacy Helpers 3.0.1、XR Hands 1.9.0、OpenXR 1.18.0、XR Interaction Toolkit 3.6.1。
- PICO SDK 仍为 6.1.1，本工程中的 SDK/GLTF 本地补丁保留。
- 清理升级后失效的两个可选 OpenXR 特性引用，再由包重新枚举有效特性；Android 保持 PICO Loader。
- Active Input Handling 设为 Both，保留键鼠模拟并启用新输入系统，初始化工具也保留该配置。
- 测试脚本默认使用 Unity 6。新增 Tools/Open-Unity6.ps1，便于直接打开指定工程。

## 回退备份

升级前 Assets、Packages、ProjectSettings、Tools、README 完整备份：E:\DEMO\Backups\SpatialSurvivorUnity-before-Unity6-20261106-140947.zip。

替换的旧嵌入包另存在 E:\DEMO\Backups\SpatialSurvivorUnity-Unity6-retired-packages。Unity 6 后续保存的场景/资源不应直接用旧编辑器覆盖；需要旧工程时从升级前压缩包恢复至单独目录。

## 验证范围

Unity 6 下 94 项测试全部通过，交互编辑器已打开主场景并进入 Play，主菜单正常显示，控制台错误数为 0。编译、玩法回归和编辑器打开结果记录于 Validation/unity6-summary.json，测试 XML 为 Validation/unity6-tests.xml。升级编辑器时的 2.2.0 APK 原为 Unity 2022 构建；后续 Unity 6 Android 构建与真机部署结果见下节及 Validation/unity6-device-summary.json。

## Unity 6 Android 构建（2026-11-06）

使用 `Tools/Build-Android.ps1` 进行 ARM64 / IL2CPP / OpenGLES3 开发包构建。默认 TEMP 的短路径使本机 JDK 17 NIO 管道连接失败；脚本仅对子进程使用工程内 Logs/java-sockets 作为 TEMP/TMP，并在结束后恢复调用进程环境。完整 Unity BuildPipeline 已成功。

2.2.1 射线修正版的输出为 Builds/SpatialSurvivorUnity.apk，75,069,772 字节，SHA256 `ac9fa013cb8f0ba1cb3ba4a1859bcd1cc56009a3b7f157375f4944d41c2698cb`。应用版本 2.2.1（13），覆盖安装并确认进入真机前台；回读设备 APK 哈希一致。射线显示修正后 95 项回归通过，用户已在头显确认射线完整、点击正常。真机验证另见 Validation/unity6-device-summary.json。

## 后续图标修正

2.2.2（14）补齐 Android 自适应图标配置，但用户反馈应用列表仍为 Unity 图标。已成功构建和覆盖安装，安装包回读哈希一致；图标资源验证与应用列表验收状态见 UNITY6_APP_ICON_FIX.md、Validation/unity6-icon-summary.json。上节 2.2.1 的哈希记录保留为射线修正版历史。

2.2.3（15）进一步统一应用、启动 Activity 和备用平面图标；构建及 APK 资源验证完成，安装时设备离线，等待重新连接。设备随后已重新连接，2.2.3 覆盖安装和哈希回读通过；桌面仍显示旧图标，已重启桌面等待确认。最新状态见 Validation/unity6-flat-icon-summary.json。

最终图标验收：用户卸载后重新构建并全新安装 2.2.3（15），回读 APK 哈希一致，用户确认应用列表已显示游戏图标。当前 Gradle 导出同时指定本工程 Java 临时目录并禁用 daemon 复用，解决跨工程临时路径导致的压缩任务失败。
