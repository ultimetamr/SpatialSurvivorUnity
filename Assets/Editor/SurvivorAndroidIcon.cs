using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace SpatialSurvivor.Editor {
public sealed class SurvivorAndroidIcon : IPostGenerateGradleAndroidProject {
 const string AndroidNamespace="http://schemas.android.com/apk/res/android";
 const string IconReference="@drawable/spatial_survivor_icon";
 public int callbackOrder=>int.MaxValue;

 public void OnPostGenerateGradleAndroidProject(string path){
  // A passthrough game must declare MR safety; VST alone keeps VR policy.
  var roomManifest=new XmlDocument();roomManifest.Load(Path.Combine(path,"src/main/AndroidManifest.xml"));
  var roomNamespaces=Namespaces(roomManifest);
  foreach(var flag in new[]{"enable_mr_safeguard","enable_vst"}){
   var metadata=roomManifest.SelectNodes("/manifest/application/meta-data[@android:name='"+flag+"']",roomNamespaces);
   if(metadata.Count!=1||((XmlElement)metadata[0]).GetAttribute("value",AndroidNamespace)!="1")
    throw new BuildFailedException("Room-scale MR requires "+flag+"=1. Enable MR Safeguard and Video SeeThrough in PICO project settings.");
  }
  var source=Path.Combine(Application.dataPath,"Resources/Icons/AppIcon.png");
  var launcher=Path.Combine(Directory.GetParent(Path.GetFullPath(path)).FullName,"launcher/src/main");
  var manifest=Path.Combine(launcher,"AndroidManifest.xml");
  if(!File.Exists(source)||!File.Exists(manifest))
   throw new BuildFailedException("Game icon or generated launcher manifest is missing.");

  var resources=Path.Combine(launcher,"res");
  var drawable=Path.Combine(resources,"drawable-nodpi");
  Directory.CreateDirectory(drawable);
  File.Copy(source,Path.Combine(drawable,"spatial_survivor_icon.png"),true);
  // Some launchers read the flat fallback instead of the adaptive layers.
  foreach(var fallback in Directory.GetFiles(resources,"app_icon.png",SearchOption.AllDirectories))
   File.Copy(source,fallback,true);

  var doc=new XmlDocument();doc.Load(manifest);
  var application=doc.SelectSingleNode("/manifest/application") as XmlElement;
  if(application==null)throw new BuildFailedException("Launcher manifest has no application element.");
  SetIcon(application);
  var activity=application.SelectSingleNode("activity[@android:name='com.unity3d.player.UnityPlayerActivity']",Namespaces(doc)) as XmlElement;
  if(activity==null){
   activity=doc.CreateElement("activity");
   activity.SetAttribute("name",AndroidNamespace,"com.unity3d.player.UnityPlayerActivity");
   application.AppendChild(activity);
  }
  SetIcon(activity);
  doc.Save(manifest);
  Debug.Log("Android launcher application, activity and flat fallback use the game app icon.");
 }

 static void SetIcon(XmlElement element){
  element.SetAttribute("icon",AndroidNamespace,IconReference);
  element.SetAttribute("label",AndroidNamespace,SurvivorIdentity.Name);
  if(element.Name=="application")element.SetAttribute("roundIcon",AndroidNamespace,IconReference);
 }
 static XmlNamespaceManager Namespaces(XmlDocument doc){
  var manager=new XmlNamespaceManager(doc.NameTable);manager.AddNamespace("android",AndroidNamespace);return manager;
 }
}
}
