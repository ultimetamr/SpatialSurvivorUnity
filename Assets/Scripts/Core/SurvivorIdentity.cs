namespace SpatialSurvivor {
public static class SurvivorIdentity {
 public const string Name="幻域守卫";
 public const string EnglishName="REALM WARDENS";
 public const string MenuTitle="幻域\n守卫";
 public const string Version="2.6.1";
 public const int VersionCode=20;
 public static string SaveDirectory {
  get {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
   // Windows derives its folder from the display name; keep existing progress.
   return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(UnityEngine.Application.persistentDataPath),"空间幸存者 Unity");
#else
   return UnityEngine.Application.persistentDataPath;
#endif
  }
 }
}
}
