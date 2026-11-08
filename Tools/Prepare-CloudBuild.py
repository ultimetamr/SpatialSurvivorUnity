#!/usr/bin/env python3
"""Restore public-clone dependencies before starting Unity (Python 3.8+)."""
import argparse
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile

SDK_REPOSITORY = "https://github.com/Pico-Developer/PICO-Unity-SDK.git"
SDK_COMMIT = "8ed808855c1dbc2a071abd709824b593977d6c24"
SDK_NAME = "com.bytedance.pico.xr"
SDK_VERSION = "6.1.1"
GUARD = "if (AssetDatabase.IsAssetImportWorkerProcess()) return;"
PATCHES = (
    ("Editor/PXR_Utils.cs", "public static void SetDefaultDefineSymbols()"),
    ("Editor/PXR_PortalEditor.cs", "private static void InitializeOnLoad()"),
)


def validate_sdk(directory):
    package = json.loads((directory / "package.json").read_text(encoding="utf-8-sig"))
    if package.get("name") != SDK_NAME or package.get("version") != SDK_VERSION:
        raise ValueError("PICO SDK must be com.bytedance.pico.xr version 6.1.1")


def run_git(directory, *arguments):
    subprocess.run(["git", "-C", str(directory), *arguments], check=True)


def fetch_sdk(destination):
    # Download from its publisher; never redistribute SDK bytes in this repo.
    with tempfile.TemporaryDirectory(prefix="realm-wardens-sdk-") as scratch:
        checkout = Path(scratch)
        run_git(checkout, "init", "--quiet")
        run_git(checkout, "remote", "add", "origin", SDK_REPOSITORY)
        run_git(checkout, "config", "core.sparseCheckout", "true")
        (checkout / ".git/info/sparse-checkout").write_text("/XR/\n", encoding="utf-8")
        run_git(checkout, "fetch", "--quiet", "--depth=1", "--filter=blob:none", "origin", SDK_COMMIT)
        run_git(checkout, "checkout", "--quiet", "--detach", "FETCH_HEAD")
        source = checkout / "XR"
        validate_sdk(source)
        shutil.copytree(source, destination)


def prepare(root, sdk_source, download, check_only):
    for required in ("ProjectSettings/ProjectVersion.txt", "Packages/manifest.json",
                     "Assets/Scenes/SpatialSurvivor.unity", "Assets/Resources/Icons/AppIcon.png"):
        if not (root / required).is_file():
            raise FileNotFoundError("Project file missing: " + required)
    editor = (root / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8-sig")
    if "m_EditorVersion: 6000.6.8f1" not in editor.splitlines():
        raise ValueError("Expected Unity 6000.6.8f1")
    destination = root / "Packages" / SDK_NAME
    if not (destination / "package.json").is_file():
        if check_only:
            raise FileNotFoundError("PICO SDK is missing; run with --fetch-pico-sdk or --pico-sdk PATH before Unity")
        if destination.exists():
            raise ValueError("Incomplete SDK directory already exists; repair it before preparing the project")
        if sdk_source:
            source = Path(sdk_source).resolve()
            validate_sdk(source)
            shutil.copytree(source, destination)
        elif download:
            fetch_sdk(destination)
        else:
            raise ValueError("Supply --pico-sdk PATH or --fetch-pico-sdk (PICO SDK license applies)")
    validate_sdk(destination)
    for relative, method in PATCHES:
        path = destination / relative
        content = path.read_text(encoding="utf-8-sig")
        match = re.search(re.escape(method) + r"\s*\{\s*", content)
        if not match:
            raise ValueError("SDK compatibility patch location missing: " + relative)
        if not content[match.end():].startswith(GUARD):
            if check_only:
                raise ValueError("SDK compatibility guard missing: " + relative)
            content = content[:match.end()] + GUARD + "\n            " + content[match.end():]
            path.write_bytes(content.replace("\r\n", "\n").encode("utf-8"))
    font = root / "Assets/Resources/Fonts/Chinese.ttf"
    if not font.is_file():
        if check_only:
            raise FileNotFoundError("Chinese font is missing; run preparation before Unity")
        meta = Path(str(font) + ".meta")
        # Preserve the committed GUID used by the scene and UI resources.
        content = meta.read_text(encoding="utf-8-sig")
        shutil.copyfile(root / "PublicDependencies/Fonts/NotoSansSC-VF.ttf", font)
        meta.write_bytes(content.replace("- SimHei", "- Noto Sans SC").encode("utf-8"))
    print("Dependencies ready: Unity 6000.6.8f1, PICO SDK 6.1.1, Chinese font.")
    print("Build method: SpatialSurvivor.Editor.SurvivorProject.BuildAndroid")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", type=Path, default=Path(__file__).resolve().parent.parent)
    source = parser.add_mutually_exclusive_group()
    source.add_argument("--pico-sdk", help="Authorized local com.bytedance.pico.xr directory")
    source.add_argument("--fetch-pico-sdk", action="store_true", help="Download pinned official SDK; publisher license applies")
    parser.add_argument("--check", action="store_true", help="Validate only; do not download or write files")
    args = parser.parse_args()
    try:
        prepare(args.project.resolve(), args.pico_sdk, args.fetch_pico_sdk, args.check)
    except (OSError, ValueError, subprocess.CalledProcessError) as error:
        print("Cloud preparation failed: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
