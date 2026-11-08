# PICO 云打包审核交接 · 幻域守卫 2.6.1

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
