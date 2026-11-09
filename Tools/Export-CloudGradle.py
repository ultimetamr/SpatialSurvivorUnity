#!/usr/bin/env python3
"""Refresh the portable Gradle snapshot from a successful local Unity Android build."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import zipfile
import urllib.request

ROOT = Path(__file__).resolve().parent.parent
SDK_COMMIT = "8ed808855c1dbc2a071abd709824b593977d6c24"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--export", type=Path, default=ROOT / "Library/Bee/Android/Prj/IL2CPP/Gradle")
    parser.add_argument("--apk", type=Path)
    args = parser.parse_args()
    source = args.export.resolve()
    apk = args.apk or source / "launcher/build/outputs/apk/debug/launcher-debug.apk"
    identity = json.loads((ROOT / "Marketing/work-info.json").read_text(encoding="utf-8-sig"))
    icon = (ROOT / "Assets/Resources/Icons/AppIcon.png").read_bytes()
    # Refuse stale exports before changing any snapshot files.
    launcher_gradle = (source / "launcher/build.gradle").read_text(encoding="utf-8-sig")
    if ('versionName "' + identity["version"] + '"') not in launcher_gradle:
        raise ValueError("Unity export version differs from work-info.json; rebuild Unity first")
    output = ROOT / "AndroidCloud"
    sdk = ROOT / "Packages/com.bytedance.pico.xr"
    dependencies = []
    request = urllib.request.Request(
        "https://api.github.com/repos/Pico-Developer/PICO-Unity-SDK/git/trees/" + SDK_COMMIT + "?recursive=1",
        headers={"User-Agent": "RealmWardens-build-export"})
    with urllib.request.urlopen(request, timeout=30) as response:
        tree = {item["path"]: item for item in json.load(response)["tree"]}

    def external_dependency(file, relative_destination):
        matches = [p for p in sdk.rglob(file.name) if p.is_file() and p.read_bytes() == file.read_bytes()]
        if len(matches) != 1:
            raise ValueError("Cannot identify official SDK binary: " + str(file))
        dependency = {
            "path": relative_destination,
            "url": "https://raw.githubusercontent.com/Pico-Developer/PICO-Unity-SDK/" + SDK_COMMIT + "/XR/" + matches[0].relative_to(sdk).as_posix(),
            "sha256": hashlib.sha256(file.read_bytes()).hexdigest(),
        }
        if file.name == "PxrPlatform.aar":
            dependency["sha256"] = "e70059608bd44b414f26a15f4746308ec6cdf42243c0e04b86643eca94aab7d9"
            dependency["note"] = "Official PICO 6.1.1 archive replaces differing local development copy; headset smoke test required."
        blob = tree["XR/" + matches[0].relative_to(sdk).as_posix()]
        dependency["gitBlobSha"] = blob["sha"]
        dependency["apiUrl"] = "https://api.github.com/repos/Pico-Developer/PICO-Unity-SDK/git/blobs/" + blob["sha"]
        dependencies.append(dependency)

    for file in (source / "unityLibrary/libs").iterdir():
        if file.is_file() and file.name != "unity-classes.jar":
            external_dependency(file, "unityLibrary/libs/" + file.name)
    for file in (source / "unityLibrary/src/main/jniLibs/arm64-v8a").glob("libPico*.so"):
        external_dependency(file, "unityLibrary/src/main/jniLibs/arm64-v8a/" + file.name)
    with zipfile.ZipFile(apk) as archive:
        if archive.read("res/drawable-nodpi-v4/spatial_survivor_icon.png") != icon:
            raise ValueError("APK does not use the current icon; rebuild Unity first")
        for module in ("launcher", "unityLibrary"):
            for directory in ("java", "res", "resources"):
                origin = source / module / "src/main" / directory
                if origin.exists():
                    shutil.copytree(origin, output / module / "src/main" / directory, dirs_exist_ok=True)
            destination = output / module / "src/main/AndroidManifest.xml"
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source / module / "src/main/AndroidManifest.xml", destination)
        # APK assets and ELF binaries are the exact tested player, without host paths or C++ build caches.
        for name in archive.namelist():
            relative = None
            if name.startswith("assets/") and not name.endswith("/"):
                relative = "unityLibrary/src/main/" + name
            elif name.startswith("lib/arm64-v8a/") and Path(name).name in (
                "libil2cpp.so", "libunity.so", "libmain.so", "lib_burst_generated.so",
                "libc++_shared.so", "libswappywrapper.so",
            ):
                relative = "unityLibrary/src/main/jniLibs/arm64-v8a/" + Path(name).name
            if relative:
                path = output / relative
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_bytes(archive.read(name))
    boot = output / "unityLibrary/src/main/assets/bin/Data/boot.config"
    content = boot.read_text(encoding="utf-8")
    boot.write_text("\n".join(line for line in content.splitlines() if not line.startswith("player-connection-ip=")) + "\n", encoding="utf-8")
    libraries = output / "unityLibrary/libs"
    libraries.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source / "unityLibrary/libs/unity-classes.jar", libraries / "unity-classes.jar")
    shutil.copyfile(source / "unityLibrary/proguard-unity.txt", output / "unityLibrary/proguard-unity.txt")
    snapshot = {
        "version": identity["version"], "versionCode": identity["versionCode"],
        "packageName": identity["packageName"],
        "sourceCommit": __import__("subprocess").check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip(),
        "unity": "6000.6.8f1", "configuration": "Development player, Android release packaging",
        "apkSha256": hashlib.sha256(apk.read_bytes()).hexdigest(),
        "runtimeLibraryChange": "Official PICO 6.1.1 PxrPlatform AAR replaces local development archive; headset verification pending.",
        "iconSha256": hashlib.sha256(icon).hexdigest(),
        "sdkCommit": SDK_COMMIT, "dependencies": dependencies,
    }
    (output / "snapshot.json").write_text(json.dumps(snapshot, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print("Portable Android snapshot refreshed:", output)
    print("SDK dependencies restored from publisher at Gradle startup:", len(dependencies))


if __name__ == "__main__":
    main()
