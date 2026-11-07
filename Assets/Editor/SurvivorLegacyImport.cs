using System.IO;
using UnityEditor;
using UnityEngine;
namespace SpatialSurvivor.Editor {
public static class SurvivorLegacyImport {
 [MenuItem("SpatialSurvivor/Import Android Progress XML")]
 public static void Import(){if(!EditorApplication.isPlaying){Debug.LogWarning("Press Play and stay in the main menu before importing Android progress.");return;}var path=EditorUtility.OpenFilePanel("选择原版 SharedPreferences XML","","xml");if(string.IsNullOrEmpty(path))return;var game=Object.FindObjectOfType<SurvivorGame>();if(game&&game.ImportLegacyXml(File.ReadAllText(path),out var message))Debug.Log(message);else Debug.LogWarning(game?game.notice:"Game not running");}
}
}
