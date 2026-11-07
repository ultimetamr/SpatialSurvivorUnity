using NUnit.Framework;
namespace SpatialSurvivor.Tests {
public class LegacyProgressTests {
 const string Xml="<map><int name=\"total_crystals\" value=\"1200\"/><int name=\"starting_max_health_level\" value=\"3\"/><int name=\"starting_attack_damage_level\" value=\"4\"/><int name=\"wave_reward_level\" value=\"2\"/></map>";
 [Test] public void OriginalSharedPreferencesImportsCurrencyAndBonuses(){Assert.That(LegacyProgressMigration.TryRead(Xml,out var result,out _),Is.True);Assert.That(result.crystals,Is.EqualTo(1200));Assert.That(result.NewPlayer().maxHealth,Is.EqualTo(130));Assert.That(result.NewPlayer().damage,Is.EqualTo(28));Assert.That(result.levels[7],Is.EqualTo(2));}
 [Test] public void ImportDoesNotDuplicateCurrencyOrDiscardUnityProgress(){var result=new MetaSave{crystals=1500};result.levels[1]=6;Assert.That(LegacyProgressMigration.MergeOnce(result,Xml,out _),Is.True);Assert.That(result.crystals,Is.EqualTo(1500));Assert.That(result.levels[0],Is.EqualTo(3));Assert.That(result.levels[1],Is.EqualTo(6));result.crystals=7;Assert.That(LegacyProgressMigration.MergeOnce(result,Xml,out _),Is.False);Assert.That(result.crystals,Is.EqualTo(7));}
 [TestCase("<map><int name=\"total_crystals\" value=\"-3\"/></map>")]
 [TestCase("<map><int name=\"total_crystals\" value=\"broken\"/></map>")]
 [TestCase("<map/>")]
 [TestCase("<!DOCTYPE map [<!ENTITY ext SYSTEM 'file:///should-never-read'>]><map>&ext;</map>")]
 public void InvalidOrUnsafeLegacyFilesDoNotChangeProgress(string xml){var result=new MetaSave{crystals=87};Assert.That(LegacyProgressMigration.MergeOnce(result,xml,out _),Is.False);Assert.That(result.crystals,Is.EqualTo(87));Assert.That(result.legacyImportHash,Is.Empty);}
 [Test] public void LegacyLevelsAreClampedToCurrentCaps(){Assert.That(LegacyProgressMigration.TryRead("<map><int name=\"total_crystals\" value=\"1\"/><int name=\"starting_max_health_level\" value=\"999\"/></map>",out var result,out _),Is.True);Assert.That(result.levels[0],Is.EqualTo(MetaSave.Caps[0]));}
}
}
