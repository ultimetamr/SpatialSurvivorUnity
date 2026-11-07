# Unity 6 应用图标修正（2.2.2 / 14）

用户确认问题发生在头显应用列表：空间幸存者显示为 Unity 默认图标，并非应用停在启动 Logo。

原游戏图标仍在 Assets/Resources/Icons/AppIcon.png。此前初始化只设置通用图标，Android 自适应图标槽为空；2.2.1 APK 的 ic_launcher_foreground.png 实际为 Unity 标志。现在初始化使用 Unity 6 支持的 Android PlatformIcon API，将原图标填入自适应图标的全部六种尺寸及必需图层，后续构建不会再次回落到默认前景。当前 Unity 6 已弃用旧的 Round/Legacy 类型，不调用这些已禁用的 API。

2.2.2 已成功构建并覆盖安装，设备版本为 2.2.2 / 14，回读安装包与构建产物 SHA256 一致。APK 自适应 XML 的前景/背景引用已核对，12 个图层资源均与原图缩放结果比对通过，最大平均像素差约 2.16/255；从实际 APK 提取的前景已目视检查为蓝色魔法飞刃图案。证据见 Validation/unity6-icon-summary.json、Validation/unity6-icon-resource-check.json 和 Validation/unity6-ic_launcher_foreground.png。

本次只改编辑器图标配置与版本号，未重新运行完整游戏测试。此前 2.2.1 的 95 项测试通过，用户确认射线完整、点击正常。用户反馈 2.2.2 的应用列表仍显示 Unity 图标，说明仅验证自适应图层不足以完成此项验收。


## 2.2.3（code 15）：统一应用列表的平面图标入口

回查 2.2.2 设备 APK，发现 res/mipmap-mdpi-v4/app_icon.png 仍是 Unity 标志。新增 Assets/Editor/SurvivorAndroidIcon.cs，在 Gradle 项目生成后复制原始游戏 PNG 为独立 drawable，将 application 的 icon、roundIcon 和 UnityPlayerActivity 的 icon 显式指向它，同时替换所有平面 app_icon.png 备用资源。自适应图层继续保留原游戏图案。

2.2.3 已构建成功；aapt 核对应用、启动 Activity 和所有密度的图标都指向 spatial_survivor_icon.png，roundIcon 的资源 ID 相同。APK 内独立图标与备用平面图标均为 1024×1024，逐像素比较与原图完全一致。

安装 2.2.3 时头显已离线，CLI 返回 Online devices: none，因此尚未覆盖安装，当前头显最后确认安装的版本为 2.2.2。需重新连接头显后继续安装并验收应用列表。详见 Validation/unity6-flat-icon-summary.json。

设备重新连接后，2.2.3 已成功覆盖安装，设备版本 2.2.3 / 15，回读 APK 哈希与产物一致。用户仍反馈应用列表显示 Unity 图标。包管理器 query-activities 返回应用/启动入口图标均为 0x7f020000，与 APK 中游戏 drawable 一致。已通过停止并启动 com.picoxr.launcher/.MainActivity 重启桌面进程（PID 11372），未清除桌面数据；等待刷新后的显示反馈。

## 最终真机验收

用户卸载旧包后，确认包列表不含本应用，重新构建并全新安装 2.2.3（15）。设备版本、回读 APK SHA256 和最终 PNG 资源均核对通过，用户已确认“已显示游戏图标”。此前覆盖安装与桌面重启后的显示结果均保留为排查历史。

重新构建首次遇到共享 Gradle daemon 引用另一工程失效 TEMP 的 compressDebugAssets 错误。新增 SurvivorAndroidBuildEnvironment.cs，在生成的 gradle.properties 内指定本工程 Logs/java-sockets 为 java.io.tmpdir，并禁用 daemon 复用；重试构建成功。未操作其他工程的进程或文件。
