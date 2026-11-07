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
