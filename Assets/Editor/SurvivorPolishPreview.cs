using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpatialSurvivor.Editor {
public static class SurvivorPolishPreview {
 const string Folder="Docs/Validation";
 public static void Render(){
  SurvivorArtBuilder.Build();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  Directory.CreateDirectory(Folder);
  var camera=new GameObject("Model polish review").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=.78f;
  camera.backgroundColor=new Color(.055f,.065f,.10f);camera.clearFlags=CameraClearFlags.SolidColor;
  camera.transform.position=new Vector3(1.65f,1.05f,5);camera.transform.LookAt(new Vector3(0,.55f,0));
  const int width=480,height=560;var sheet=new Texture2D(width*4,height*4,TextureFormat.RGB24,false);
  var background=new Color[width*4*height*4];for(int i=0;i<background.Length;i++)background[i]=camera.backgroundColor;sheet.SetPixels(background);
  var summary=new System.Text.StringBuilder("Model,Vertices,Triangles,Bones,Materials\n");
  for(int i=0;i<SurvivorArtBuilder.Creatures.Length;i++){
   string name=SurvivorArtBuilder.Creatures[i];camera.orthographicSize=i==12?.94f:.78f;var actor=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+name));
   var animator=actor.GetComponent<SurvivorCreatureAnimator>();animator.idle.SampleAnimation(actor,.2f);
   var skin=actor.GetComponent<SkinnedMeshRenderer>();summary.AppendLine(name+","+skin.sharedMesh.vertexCount+","+skin.sharedMesh.triangles.Length/3+","+skin.bones.Length+","+skin.sharedMaterials.Length);
   var label=new GameObject("Review label").AddComponent<TextMesh>();label.text=name;label.fontSize=36;label.characterSize=.035f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.82f,.87f,.94f);label.transform.position=new Vector3(0,-.11f,0);label.transform.rotation=camera.transform.rotation;
   var frame=Capture(camera,width,height);sheet.SetPixels((i%4)*width,(3-i/4)*height,width,height,frame.GetPixels());
   if(i==4||i>=11)File.WriteAllBytes(Folder+"/v260-model-"+name+".png",frame.EncodeToPNG());
   Object.DestroyImmediate(frame);Object.DestroyImmediate(label.gameObject);Object.DestroyImmediate(actor);
  }
  sheet.Apply();File.WriteAllBytes(Folder+"/v260-creature-gallery.png",sheet.EncodeToPNG());Object.DestroyImmediate(sheet);
  File.WriteAllText(Folder+"/v260-mesh-budgets.csv",summary.ToString());
  string[] actions={"EmberImp","CrystalGuardian","RuneShaman","SkyMoth","ThornSentinel","MoonRegent"};
  var poses=new Texture2D(6*320,actions.Length*360,TextureFormat.RGB24,false);
  camera.transform.position=new Vector3(2.2f,1.1f,5);camera.transform.LookAt(new Vector3(0,.55f,0));camera.orthographicSize=.8f;
  float[] times={0,.18f,.34f,.52f,.70f,.98f};
  for(int row=0;row<actions.Length;row++)for(int column=0;column<times.Length;column++){
   camera.orthographicSize=actions[row]=="MoonRegent"?.95f:.8f;var actor=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+actions[row]));var animator=actor.GetComponent<SurvivorCreatureAnimator>();animator.attack.SampleAnimation(actor,times[column]*animator.attack.length);
   var frame=Capture(camera,320,360);poses.SetPixels(column*320,(actions.Length-1-row)*360,320,360,frame.GetPixels());Object.DestroyImmediate(frame);Object.DestroyImmediate(actor);
  }
  poses.Apply();File.WriteAllBytes(Folder+"/v260-attack-poses.png",poses.EncodeToPNG());Object.DestroyImmediate(poses);
  RenderFrames(camera,"EmberImp",CreatureMotion.Walk,"v260-walk",24);
  RenderFrames(camera,"ThornSentinel",CreatureMotion.Attack,"v260-thrust",24);
  RenderFrames(camera,"StormDrake",CreatureMotion.Attack,"v260-dive",24);
  RenderFrames(camera,"MoonRegent",CreatureMotion.Attack,"v260-cast",24);
  RenderFrames(camera,"Mossling",CreatureMotion.Idle,"v260-idle",48);
  Debug.Log("V260 model and motion review rendered.");
 }
 static void RenderFrames(Camera camera,string name,CreatureMotion motion,string prefix,int count){
  camera.orthographicSize=name=="MoonRegent"?.95f:.8f;string folder="Logs/ArtReview/"+prefix;Directory.CreateDirectory(folder);
  var actor=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+name));var a=actor.GetComponent<SurvivorCreatureAnimator>();
  var clip=motion==CreatureMotion.Walk?a.walk:motion==CreatureMotion.Attack?a.attack:a.idle;
  for(int f=0;f<count;f++){clip.SampleAnimation(actor,clip.length*f/(count-1));var tex=Capture(camera,480,560);File.WriteAllBytes(folder+"/"+f.ToString("D3")+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}
  Object.DestroyImmediate(actor);
 }
 static Texture2D Capture(Camera camera,int width,int height){
  var old=RenderTexture.active;var target=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
  camera.targetTexture=target;camera.Render();RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(target);return tex;
 }
}
}
