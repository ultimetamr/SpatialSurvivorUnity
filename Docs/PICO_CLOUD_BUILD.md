# PICO 云打包审核交接 · 幻域守卫 2.6.1

## 当前云平台入口（2026-11-09 修正）

平台日志确认：Jenkins 在仓库根目录执行 Gradle `assembleRelease`，没有先运行 Unity。现在根目录已提供标准 `settings.gradle`、`build.gradle` 和 Gradle 8.11.1 wrapper，模块源码位于 `AndroidCloud/launcher` 与 `AndroidCloud/unityLibrary`。**重新审核时必须选用本次修复后的提交，不能继续使用 262d780。**

- 项目目录：仓库根目录 `.`；Android 构建任务：`assembleRelease`。
- Gradle 8.11.1、AGP 8.10.1、JDK 17、Android SDK Platform 35 / Build Tools 35.0.0。
- 默认 versionName 2.6.1、versionCode 20，平台 init script 可以覆盖版本号。平台日志曾注入 1；若要覆盖设备上已有 20 版本，应在平台把 versionCode 设为 21 或更高。
- 产物：`AndroidCloud/launcher/build/outputs/apk/release/launcher-release.apk`。
- 包名：`com.example.spatialsurvivor.unity`；应用名：幻域守卫；图标：翡翠符文剑与月冠（1024²）。
- 云端需要访问 Google Maven / Maven Central / Gradle 分发源及 `api.github.com` / `raw.githubusercontent.com`（或校验相同哈希的 `cdn.jsdelivr.net` 备用地址）。没有本机绝对 SDK / NDK / JDK 路径，不需要 Unity 或 NDK 来重编译此 Android 快照。

```sh
./gradlew --no-daemon --console=plain assembleRelease --stacktrace
```

云平台继续使用系统 `gradle` 8.11.1 执行原有命令也可。11 个 SDK 运行依赖在 Gradle 配置阶段从 PICO 官方固定提交恢复（优先 GitHub Blob API，备用原始文件地址与固定版本 CDN 副本），并逐个验证 SHA-256；不将受限 SDK 源码、AAR 或 PICO 原始 native 库复制进公共 Git。固定地址及哈希见 `AndroidCloud/snapshot.json`。隔离网络中的构建机也可设置环境变量 `PICO_SDK_PATH` 指向已授权的 SDK 6.1.1 目录，仍会检查各文件哈希。

### 源码与运行快照的关系

完整 Unity/C# 源码仍在 `Assets`、`Packages` 与 `ProjectSettings`。`AndroidCloud` 保存 Unity 已编译的游戏运行快照、Android Java 源码、清单和资源，Gradle 会执行真实 Android 编译、资源合并、DEX、打包与签名；**云端不会重新编译 C# / IL2CPP，也不是从 APK 直接复制出最终安装包**。

当前快照来自已验证的 2.6.1 APK（SHA-256 `7acdd3b9f05a7a7d0f3af722d217309a236cd4dae22a80aa166fb78bffd99576`）。游戏 player 是 Development 配置，Android `release` 任务使用调试签名供审核测试；平台正式签名政策需要在其控制台另行满足。运行快照中原有资产的权利不因提交而改变。

改 C#、场景、模型或游戏资源后，必须先用 Unity 重新构建，再运行：

```sh
python Tools/Export-CloudGradle.py
```

该脚本读取本机成功 Unity 构建的 Gradle 工程和 APK，核对版本及图标，更新可携带的运行产物与依赖清单，不复制本机 SDK、缓存或路径。提交更新后的 `AndroidCloud` 后，云端才能获得新玩法。此导出器用于当前固定结构；若删除或重命名资源，应先清理旧快照中相应文件，避免遗留。

## 仅适用于能先运行 Unity 的构建环境

下面保留完整 Unity 源码构建流程，**不是当前 PICO Jenkins 的默认步骤**。


此文记录源码交接参数，不代表 PICO 平台已打包或审核通过。平台页面的字段名称与可用 Unity 版本以实际控制台为准。本项目没有配置 GitHub Actions；GitHub 用于提供源码。

## 源码与环境

- GitHub：`https://github.com/ultimetamr/SpatialSurvivorUnity`
- 分支：`main`；审核时固定使用本次提交的完整 Commit ID。
- Unity 项目路径：仓库根目录（`.`），不是 `Assets`。
- Unity：`6000.6.8f1`，需 Android Build Support、相应 SDK / NDK / OpenJDK 与有效编辑器授权。
- 作品：幻域守卫 / Realm Wardens；版本 `2.6.1`，versionCode `20`。
- 包名：`com.example.spatialsurvivor.unity`。
- 平台：Android、ARM64、IL2CPP、OpenGLES3；minSdk 29。本地验证 APK targetSdk 35，现有构建配置使用安装环境的最高 API。
- 场景：`Assets/Scenes/SpatialSurvivor.unity`。
- 构建方法：`SpatialSurvivor.Editor.SurvivorProject.BuildAndroid`。
- 产物：`Builds/SpatialSurvivorUnity.apk`。
- 作品资料：`Marketing/work-info.json`；图标：`Marketing/RealmWardens-icon.png`。

## 必须先恢复依赖，再启动 Unity

PICO SDK 受原有许可约束，不复制进公开仓库。开源中文字体已在仓库内。新增脚本支持 Windows / Linux / macOS，只需 Python 3.8+；在线获取 SDK 还需 Git 与 GitHub 访问。

1. 云端拉取指定 Commit 的完整源码。
2. 在云平台的 **Unity 启动前** 准备步骤执行（Python 命令名按运行环境调整）：

   ```sh
   python3 Tools/Prepare-CloudBuild.py --fetch-pico-sdk
   python3 Tools/Prepare-CloudBuild.py --check
   ```

   脚本从官方 `Pico-Developer/PICO-Unity-SDK` 获取固定提交 `8ed808855c1dbc2a071abd709824b593977d6c24` 的 `XR` 目录，核对 SDK 6.1.1，恢复到 `Packages/com.bytedance.pico.xr`，应用两个导入兼容守卫，并恢复 Noto Sans SC 到原有字体 GUID 路径。原 SDK 许可继续适用，详见 `THIRD_PARTY_NOTICES.md`。

   云端已有获授权 SDK 时，可用以下命令替代在线获取：

   ```sh
   python3 Tools/Prepare-CloudBuild.py --pico-sdk /authorized-sdk/com.bytedance.pico.xr
   ```

3. Unity 启动时指定 Android 平台，并执行构建方法。将占位路径替换成云端实际路径：

   ```sh
   /path/to/Unity -batchmode -quit -nographics -projectPath /path/to/checkout -buildTarget Android -executeMethod SpatialSurvivor.Editor.SurvivorProject.BuildAndroid -logFile /path/to/build.log
   ```

4. 检查 Unity 退出状态、构建日志与 APK 是否存在，然后由平台继续审核。保存构建日志和源码 Commit ID。

## 遇到平台限制时

- **没有启动前脚本入口**：请平台构建方在启动 Unity 前提供 SDK 6.1.1、应用 `Docs/SDK_PATCHES.md` 中两个守卫，并把 `PublicDependencies/Fonts/NotoSansSC-VF.ttf` 复制到 `Assets/Resources/Fonts/Chinese.ttf`，保留原 `.meta` GUID。仅导入 SDK 而不恢复字体不够。不能假定平台会自动安装 SDK。
- **没有 Unity 6000.6.8f1**：把编辑器版本、完整构建日志交给构建方确认支持；不要直接以 Unity 2022 打开此项目。
- **网络不可达**：将已获授权 SDK 放到平台的私有依赖区，改用 `--pico-sdk`。不要公开上传受限 SDK。
- **提示找不到 `ByteDance.PICO.XR` 或无法解析包**：检查依赖步骤是否在 Unity 首次启动前成功完成，SDK 是否放到准确目录。
- **需要正式发行签名或非开发构建**：现有构建方法使用 `BuildOptions.Development`、调试签名。它是本项目已验证的测试构建入口；若平台要求发行包，需按平台规则改用发行构建与安全注入的签名材料。本仓库不包含发行密钥，不能据此声称已满足商店发行要求。

## 已有验证与边界

本地 Unity 回归 142/142 通过，2.6.1 APK 已构建并核验包名、双层应用名称、图标和签名。证据在 `Docs/BLADE_BRANDING_FIX.md` 与 `Docs/Validation/v261-*`。该 APK 尚未做本轮头显验收。新增依赖脚本已在不带 SDK 与中文字体的干净源码副本上通过官方 SDK 在线恢复、兼容守卫、字体一致性与 GUID 保留、重复运行幂等性检查，结果见 `Docs/Validation/v261-cloud-source-preparation.json`；这不等同于 PICO 云端构建成功。PICO 平台审核状态需在实际控制台确认。

### 官方 SDK 运行依赖差异

清洁构建检查发现，本机开发副本 `PxrPlatform.aar` 与官方固定版本存在 native 库差异。云端统一采用官方 6.1.1 的 AAR（SHA-256 `e70059608bd44b414f26a15f4746308ec6cdf42243c0e04b86643eca94aab7d9`）；其余 10 个依赖与本机完全一致。官方运行库替换后需补头显启动、MR 与输入冒烟验证，不能将旧包的真机记录直接视为本构建已验收。

## 本次验证结果

本地 Windows 以 Gradle 8.11.1 / JDK 17 从干净 Gradle 源码副本恢复全部 11 个官方依赖，`assembleRelease` 成功（86 项任务执行）；模拟平台 init-script 注入 versionCode=1，APK 回读确认为 1。默认构建回读为 2.6.1 / 20，名称为幻域守卫，1024² 图标字节与源文件一致，v2 签名通过。证据见 `Docs/Validation/v261-cloud-gradle-summary.json`。尚未执行平台的 Linux Jenkins 或头显验收，因此不能宣称云审核通过。
