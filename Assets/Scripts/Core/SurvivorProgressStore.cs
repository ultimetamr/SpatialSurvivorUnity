using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SpatialSurvivor {
/// <summary>Unity-owned progression with recovery after an interrupted write.</summary>
public sealed class SurvivorProgressStore {
    const long MaximumSaveBytes = 65536;
    readonly string path;

    public SurvivorProgressStore(string path) {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public MetaSave Load(out string message) {
        message = "";
        if (TryRead(path, out var primary)) return primary;
        // A complete temporary file is newer than the backup if the app exited
        // between writing it and replacing the primary file.
        foreach (var candidate in new[] { path + ".tmp", path + ".bak" }) {
            if (!TryRead(candidate, out var recovered)) continue;
            message = "已恢复本地进度备份";
            return recovered;
        }
        if (File.Exists(path) || File.Exists(path + ".tmp") || File.Exists(path + ".bak"))
            message = "本地进度无法读取，已使用默认进度";
        return new MetaSave();
    }

    public void Save(MetaSave progress) {
        if (progress == null) throw new ArgumentNullException(nameof(progress));
        string parent = Path.GetDirectoryName(Path.GetFullPath(path));
        Directory.CreateDirectory(parent);
        string temporary = path + ".tmp";
        byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(progress, true));
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)) {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
        }
        if (!TryRead(temporary, out _)) throw new InvalidDataException("保存的进度无效");
        if (!File.Exists(path)) {
            File.Move(temporary, path);
            return;
        }
        // Preserve a valid backup when the primary file itself was damaged.
        bool primaryValid = TryRead(path, out _);
        try {
            File.Replace(temporary, path, primaryValid ? path + ".bak" : null);
        } catch (PlatformNotSupportedException) {
            ReplaceWithRecovery(temporary, primaryValid);
        } catch (NotSupportedException) {
            ReplaceWithRecovery(temporary, primaryValid);
        }
    }

    void ReplaceWithRecovery(string temporary, bool primaryValid) {
        if (primaryValid) File.Copy(path, path + ".bak", true);
        File.Delete(path);
        File.Move(temporary, path);
    }

    static bool TryRead(string file, out MetaSave progress) {
        progress = null;
        try {
            var info = new FileInfo(file);
            if (!info.Exists || info.Length <= 0 || info.Length > MaximumSaveBytes) return false;
            string json = File.ReadAllText(file);
            if (!json.TrimStart().StartsWith("{") || !json.TrimEnd().EndsWith("}") ||
                !json.Contains("\"levels\"") || !json.Contains("\"crystals\"")) return false;
            var loaded = JsonUtility.FromJson<MetaSave>(json);
            if (loaded == null || loaded.levels == null || loaded.levels.Length != MetaSave.Caps.Length)
                return false;
            loaded.crystals = Math.Max(0, loaded.crystals);
            for (int i = 0; i < loaded.levels.Length; i++)
                loaded.levels[i] = Mathf.Clamp(loaded.levels[i], 0, MetaSave.Caps[i]);
            if (loaded.version < 2) {
                loaded.soundEnabled = loaded.hapticsEnabled = true;
                loaded.version = 2;
            }
            loaded.coins=Math.Max(0,loaded.coins);
            loaded.ownedWeapons=(loaded.ownedWeapons&((1<<SurvivorArmory.Weapons.Length)-1))|1;
            if(!loaded.OwnsWeapon(loaded.equippedWeapon))loaded.equippedWeapon=0;
            loaded.version=3;
            progress = loaded;
            return true;
        } catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException ||
                                          exception is ArgumentException) {
            return false;
        }
    }
}
}
