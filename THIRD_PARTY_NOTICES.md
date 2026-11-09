# Third-party dependencies and assets

This is a publicly readable project source repository. No blanket license is
granted over third-party SDKs, fonts, models or other assets. Existing rights and
licenses continue to apply.

- **PICO Unity SDK 6.1.1** is required but intentionally omitted from Git. Its
  embedded license prohibits redistribution without prior written permission.
  Obtain it from PICO under the applicable terms and run
  `Tools/Prepare-PublicClone.ps1`. That script also applies the project's two
  editor import-worker guards. The original development copy keeps its SDK.
- **Plattar glTF exporter 1.142.0** is embedded under
  `Packages/com.plattar.unitygltf`, including the local compatibility changes
  listed in `Docs/SDK_PATCHES.md`. Upstream is
  https://github.com/Plattar/gltf-exporter and uses Apache-2.0. The upstream
  license is included at `ThirdPartyNotices/Plattar-GLTF-Apache-2.0.txt`.
- **Unity packages** are restored by Unity Package Manager according to
  `Packages/manifest.json` and `packages-lock.json`, under their own licenses.
- **Chinese font:** the development machine's SimHei file is excluded. Public
  clones use the bundled Noto Sans SC variable font from notofonts/noto-cjk,
  commit `f8d157532fbfaeda587e826d4cd5b21a49186f7c`, under SIL Open Font License
  1.1. See `ThirdPartyNotices/NotoSansCJK-OFL.txt`. The preparation script copies
  it to the existing Unity resource path while preserving the asset GUID.
- **Imported GLB models** are retained from the user's original game project;
  no new license over these assets is asserted by this publication.
- The poster is AI-generated concept artwork, not a gameplay screenshot.
  Creature meshes, animations and effects authored by this project also have
  editable generator source in `Assets/Editor/SurvivorArtBuilder.cs`.

Publishing the repository publicly does not change the rights of any author.

## Android cloud runtime snapshot

`AndroidCloud` contains application-specific compiled Unity player/game binaries and data exported from the tested 2.6.1 application, plus Android Java entry source and resources. These are runtime build inputs, not a redistribution of the full Unity Editor or PICO SDK. The existing third-party rights still apply. SDK AAR/JAR and PICO native runtime dependencies are excluded from Git; root Gradle restores the exact required files from the publisher at a pinned revision (with a checksum-verified CDN fallback) and checks their SHA-256. See `AndroidCloud/snapshot.json`. The compiled snapshot retains the assets/font of the tested development application; the Noto Sans SC preparation instructions above apply to fresh Unity source builds.

The Gradle 8.11.1 wrapper scripts and JAR are from the Gradle project, Apache-2.0; see `ThirdPartyNotices/Gradle-Apache-2.0.txt`.

The cloud snapshot uses the official PICO 6.1.1 `PxrPlatform.aar`, which differs from the local development SDK runtime. This substitution is explicitly recorded in `AndroidCloud/snapshot.json`; device verification remains required.
