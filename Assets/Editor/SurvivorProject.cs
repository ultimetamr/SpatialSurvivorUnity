using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using ByteDance.PICO.XR;
using UnityGLTF;
namespace SpatialSurvivor.Editor {
[InitializeOnLoad] public static class SurvivorProject {
 const string Scene="Assets/Scenes/SpatialSurvivor.unity";
 static SurvivorProject(){if(AssetDatabase.IsAssetImportWorkerProcess())return;EditorApplication.delayCall+=()=>{if(!Application.isBatchMode&&!File.Exists("ProjectSettings/SurvivorInitialized.txt"))Initialize();};}
 [MenuItem("SpatialSurvivor/Open Game Scene")]
 public static void OpenSceneForReview(){EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode){EditorSceneManager.OpenScene(Scene);Debug.Log("Unity 6 review scene opened: "+Scene);}};}
 public static void PrepareUnity6(){
  Configure();
  // Old optional OpenXR feature subassets can deserialize as missing objects
  // after updating the package. Keep every valid feature and its enabled state.
  const string settingsPath="Assets/XR/Settings/OpenXR Package Settings.asset";
  int removed=0;
  foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(settingsPath)){
   if(!(asset is UnityEngine.XR.OpenXR.OpenXRSettings))continue;
   var serialized=new SerializedObject(asset);var features=serialized.FindProperty("features");
   if(features==null)continue;
   for(int i=features.arraySize-1;i>=0;i--)if(features.GetArrayElementAtIndex(i).objectReferenceValue==null){features.DeleteArrayElementAtIndex(i);removed++;}
   serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);
  }
  AssetDatabase.SaveAssets();
  UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
  UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
  EnsureMrScene();AssetDatabase.SaveAssets();
  Debug.Log("Unity 6 migration prepared; removed missing optional OpenXR references: "+removed);
 }
 public static void PreparePhysicalWeapons(){Initialize();Debug.Log("Physical weapon assets and 2.2.0 configuration ready.");}
 [MenuItem("SpatialSurvivor/Initialize Project")]
 public static void Initialize(){Configure();SurvivorArtBuilder.Build();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};EditorSceneManager.OpenScene(Scene);EnsureMrScene();AssetDatabase.SaveAssets();File.WriteAllText("ProjectSettings/SurvivorInitialized.txt","Unity migration configuration generated at "+DateTime.UtcNow.ToString("O"));Debug.Log("SpatialSurvivor project initialized. Open the scene and press Play.");}
 static void EnsureMrScene(){var bootstrap=UnityEngine.Object.FindObjectOfType<SurvivorBootstrap>();var root=bootstrap?bootstrap.gameObject:new GameObject("SpatialSurvivor");var platform=root.GetComponent<SurvivorPlatform>();if(!platform)platform=root.AddComponent<SurvivorPlatform>();platform.EnsureRig();if(!root.GetComponent<SurvivorMrSession>())root.AddComponent<SurvivorMrSession>();if(!root.GetComponent<SurvivorGame>())root.AddComponent<SurvivorGame>();EditorUtility.SetDirty(platform);EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);}
 static void Configure(){var inputSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var inputHandler=inputSettings.FindProperty("activeInputHandler")??inputSettings.FindProperty("m_ActiveInputHandler");if(inputHandler!=null){inputHandler.intValue=2;inputSettings.ApplyModifiedPropertiesWithoutUndo();}PlayerSettings.companyName="SpatialSurvivor";PlayerSettings.productName="空间幸存者 Unity";PlayerSettings.bundleVersion="2.2.3";PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.example.spatialsurvivor.unity");PlayerSettings.Android.bundleVersionCode=15;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android,ApiCompatibilityLevel.NET_Standard);PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,"ENABLE_PICO_XR_SDK;PICO_XR_SDK");
 Directory.CreateDirectory("Assets/Resources");var config=PXR_ProjectSetting.GetProjectConfig();config.handTracking=true;config.handTrackingSupportType=HandTrackingSupport.ControllersAndHands;config.spatialMesh=true;config.sceneCapture=true;config.videoSeeThrough=true;config.stageMode=true;config.isSpatialAdapter=false;EditorUtility.SetDirty(config);
 var settings=PXR_Settings.GetSettings();if(!settings){settings=ScriptableObject.CreateInstance<PXR_Settings>();AssetDatabase.CreateAsset(settings,"Assets/Resources/PXR_Settings.asset");EditorBuildSettings.AddConfigObject("ByteDance.PICO.XR.Settings",settings,true);}settings.appMode=PXR_Settings.AppMode.XR;settings.systemDisplayFrequency=PXR_Settings.SystemDisplayFrequency.RefreshRate90;EditorUtility.SetDirty(settings);
 if(!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey,out var targets)){targets=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(targets,"Assets/XRGeneralSettings.asset");EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,targets,true);}if(!targets.HasSettingsForBuildTarget(BuildTargetGroup.Android))targets.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);if(!targets.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))targets.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);var xr=targets.SettingsForBuildTarget(BuildTargetGroup.Android);xr.InitManagerOnStart=true;var manager=targets.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);XRPackageMetadataStore.AssignLoader(manager,"ByteDance.PICO.XR.PXR_Loader",BuildTargetGroup.Android);EditorUtility.SetDirty(targets);EditorUtility.SetDirty(xr);EditorUtility.SetDirty(manager);
 var graphics=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");if(graphics.Length>0){var serialized=new SerializedObject(graphics[0]);var shaders=serialized.FindProperty("m_AlwaysIncludedShaders");foreach(var shaderName in new[]{"Standard","Sprites/Default","UI/Default","Survivor/Storybook","Survivor/Additive"}){var sh=Shader.Find(shaderName);if(!sh)continue;bool has=false;for(int i=0;i<shaders.arraySize;i++)has|=shaders.GetArrayElementAtIndex(i).objectReferenceValue==sh;if(!has){int i=shaders.arraySize;shaders.InsertArrayElementAtIndex(i);shaders.GetArrayElementAtIndex(i).objectReferenceValue=sh;}}serialized.ApplyModifiedPropertiesWithoutUndo();}
 ConfigureAppIcons();
 AssetDatabase.SaveAssets();}
 static void ConfigureAppIcons(){
  var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Icons/AppIcon.png");
  if(!icon)throw new InvalidOperationException("Game app icon is missing.");
  PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);
  // Android's adaptive/round slots override the generic fallback, including
  // when their serialized texture lists are empty after an editor upgrade.
  var target=NamedBuildTarget.Android;
  foreach(var kind in PlayerSettings.GetSupportedIconKinds(target)){
   var slots=PlayerSettings.GetPlatformIcons(target,kind);
   foreach(var slot in slots){
    var layers=new Texture2D[slot.minLayerCount];
    for(int i=0;i<layers.Length;i++)layers[i]=icon;
    slot.SetTextures(layers);
   }
   PlayerSettings.SetPlatformIcons(target,kind,slots);
   Debug.Log("Configured Android app icon "+kind+": "+slots.Length+" slots.");
  }
 }
 [MenuItem("SpatialSurvivor/Import Original GLB Models")]
 public static void ImportModels(){Directory.CreateDirectory("Assets/Resources/Imported");foreach(var file in Directory.GetFiles("Assets/Resources/Models","*.glb")){string name=Path.GetFileNameWithoutExtension(file);string final="Assets/Resources/Imported/"+name+".prefab";if(File.Exists(final))continue;string folder=Path.GetFullPath("Assets/ImportedSource/"+name);Directory.CreateDirectory(folder);bool done=false;var importer=new GLTFEditorImporter((step,current,total)=>{},()=>done=true);importer.setupForPath(Path.GetFullPath(file),folder,name,false);importer.Load();int guard=0;while(!done&&guard++<100000)importer.Update();if(!done)throw new Exception("Model import did not complete: "+file);AssetDatabase.Refresh();string[] prefabs=Directory.GetFiles(folder,"*.prefab",SearchOption.AllDirectories);if(prefabs.Length==0)throw new Exception("No prefab generated: "+file);string assetPath="Assets"+prefabs[0].Substring(Application.dataPath.Length).Replace(Path.DirectorySeparatorChar,'/');var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);if(!prefab)throw new Exception("Prefab load failed: "+assetPath);PrefabUtility.SaveAsPrefabAsset(prefab,final);Debug.Log("Imported SpatialSurvivor model "+name);}AssetDatabase.Refresh();}
 [MenuItem("SpatialSurvivor/Build Android APK")]
 public static void BuildAndroid(){Initialize();if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android){EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);Debug.Log("Switched to Android. Run Build Android APK again after scripts reload.");return;}Directory.CreateDirectory("Builds");var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName="Builds/SpatialSurvivorUnity.apk",target=BuildTarget.Android,options=BuildOptions.Development});if(r.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed: "+r.summary.result);Debug.Log("Built Builds/SpatialSurvivorUnity.apk");}
 public static void BatchInitialize(){Initialize();}
}
}

