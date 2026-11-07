using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SpatialSurvivor.Tests {
public class SurvivorProgressStoreTests {
    string directory, path;
    SurvivorProgressStore store;

    [SetUp] public void SetUp() {
        directory = Path.Combine(Path.GetTempPath(), "SpatialSurvivorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "progress.json");
        store = new SurvivorProgressStore(path);
    }

    [TearDown] public void TearDown() {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }

    [Test] public void FreshInstallStartsWithoutAndroidFiles() {
        var save = store.Load(out var message);
        Assert.That(save.crystals, Is.Zero);
        Assert.That(save.levels.Length, Is.EqualTo(8));
        Assert.That(message, Is.Empty);
    }

    [Test] public void PurchasesAndSettingsSurviveReload() {
        var save = new MetaSave { crystals = 500, soundEnabled = false, hapticsEnabled = false };
        Assert.That(save.Buy(1), Is.True);
        store.Save(save);
        var reloaded = store.Load(out var message);
        Assert.That(reloaded.crystals, Is.EqualTo(455));
        Assert.That(reloaded.levels[1], Is.EqualTo(1));
        Assert.That(reloaded.NewPlayer().damage, Is.EqualTo(22));
        Assert.That(reloaded.soundEnabled || reloaded.hapticsEnabled, Is.False);
        Assert.That(message, Is.Empty);
    }

    [Test] public void DamagedPrimaryRecoversLastValidBackup() {
        store.Save(new MetaSave { crystals = 100 });
        store.Save(new MetaSave { crystals = 200 });
        File.WriteAllText(path, "{broken");
        Assert.That(store.Load(out var message).crystals, Is.EqualTo(100));
        Assert.That(message, Does.Contain("恢复"));
        store.Save(new MetaSave { crystals = 150 });
        Assert.That(store.Load(out _).crystals, Is.EqualTo(150));
        Assert.That(JsonUtility.FromJson<MetaSave>(File.ReadAllText(path + ".bak")).crystals, Is.EqualTo(100));
    }

    [Test] public void InterruptedFirstWriteRecoversCompleteTemporaryFile() {
        File.WriteAllText(path + ".tmp", JsonUtility.ToJson(new MetaSave { crystals = 70 }));
        Assert.That(store.Load(out _).crystals, Is.EqualTo(70));
    }

    [Test] public void ValidPrimaryWinsOverStaleTemporaryFile() {
        store.Save(new MetaSave { crystals = 40 });
        File.WriteAllText(path + ".tmp", JsonUtility.ToJson(new MetaSave { crystals = 900 }));
        Assert.That(store.Load(out _).crystals, Is.EqualTo(40));
    }

    [Test] public void MissingFieldsAndOversizedFilesFallBackSafely() {
        store.Save(new MetaSave { crystals = 60 });
        store.Save(new MetaSave { crystals = 80 });
        File.WriteAllText(path, "{}");
        Assert.That(store.Load(out _).crystals, Is.EqualTo(60));
        File.WriteAllText(path, new string('x', 70000));
        Assert.That(store.Load(out _).crystals, Is.EqualTo(60));
    }

    [Test] public void InvalidLevelsAreClampedAndOldAudioDefaultsAreUpgraded() {
        var save = new MetaSave { version = 1, crystals = -20, soundEnabled = false, hapticsEnabled = false };
        save.levels[0] = 200;
        save.levels[1] = -5;
        File.WriteAllText(path, JsonUtility.ToJson(save));
        var loaded = store.Load(out _);
        Assert.That(loaded.version, Is.EqualTo(3));
        Assert.That(loaded.crystals, Is.Zero);
        Assert.That(loaded.levels[0], Is.EqualTo(MetaSave.Caps[0]));
        Assert.That(loaded.levels[1], Is.Zero);
        Assert.That(loaded.soundEnabled && loaded.hapticsEnabled, Is.True);
    }
}
}
