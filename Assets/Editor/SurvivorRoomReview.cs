using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace SpatialSurvivor.Editor {
public static class SurvivorRoomReview {
 const string Folder="Docs/Validation";
 public static void Render(){
  SurvivorProject.BatchInitialize();SurvivorArtBuilder.Build();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Directory.CreateDirectory(Folder);
  var camera=new GameObject("Review camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.065f,.095f);camera.nearClipPlane=.03f;camera.farClipPlane=40;camera.orthographic=true;
  var sheet=new Texture2D(1600,1680,TextureFormat.RGB24,false);Fill(sheet,camera.backgroundColor);var report=new System.Text.StringBuilder("Model,Vertices,Triangles,Materials\n");
  for(int i=0;i<SurvivorArtBuilder.Creatures.Length;i++){
   var go=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+SurvivorArtBuilder.Creatures[i]));var a=go.GetComponent<SurvivorCreatureAnimator>();a.idle.SampleAnimation(go,.2f);
   var skin=go.GetComponent<SkinnedMeshRenderer>();report.AppendLine(go.name+","+skin.sharedMesh.vertexCount+","+skin.sharedMesh.triangles.Length/3+",1");
   camera.orthographicSize=i==12?.94f:.78f;camera.transform.position=new Vector3(1.65f,1.05f,5);camera.transform.LookAt(new Vector3(0,.55f,0));
   var label=Label(SurvivorArtBuilder.Creatures[i],new Vector3(0,-.1f,0),camera,.025f);var frame=Capture(camera,400,420);sheet.SetPixels(i%4*400,(3-i/4)*420,400,420,frame.GetPixels());Object.DestroyImmediate(frame);Object.DestroyImmediate(label);Object.DestroyImmediate(go);
  }
  sheet.Apply();File.WriteAllBytes(Folder+"/v270-creatures.png",sheet.EncodeToPNG());Object.DestroyImmediate(sheet);File.WriteAllText(Folder+"/v270-mesh-budgets.csv",report.ToString());
  sheet=new Texture2D(1800,1000,TextureFormat.RGB24,false);Fill(sheet,camera.backgroundColor);
  for(int i=0;i<SurvivorArmory.Weapons.Length;i++){
   var d=SurvivorArmory.Weapons[i];var go=Object.Instantiate(Resources.Load<GameObject>("Art/Weapons/"+d.model));if(d.kind==HandWeaponKind.Runeblade)go.transform.rotation=Quaternion.Euler(0,90,0)*Quaternion.Euler(-90,0,0);
   var bounds=go.GetComponent<Renderer>().bounds;camera.orthographicSize=Mathf.Max(bounds.size.y,.42f)*.7f;camera.transform.position=bounds.center+new Vector3(.9f,.4f,2.2f);camera.transform.LookAt(bounds.center);
   var label=Label(d.model,bounds.center+Vector3.down*camera.orthographicSize*.82f,camera,.016f);var frame=Capture(camera,450,500);sheet.SetPixels(i%4*450,(1-i/4)*500,450,500,frame.GetPixels());Object.DestroyImmediate(frame);Object.DestroyImmediate(label);Object.DestroyImmediate(go);
  }
  sheet.Apply();File.WriteAllBytes(Folder+"/v270-arsenal.png",sheet.EncodeToPNG());Object.DestroyImmediate(sheet);
  var origin=new GameObject("Simulated semantic measurements");var room=new SurvivorRoomTheme(origin.transform);
  var floor=new[]{new Vector2(-3,-3),new Vector2(3,-3),new Vector2(3,3),new Vector2(-3,3)};var wall=new[]{new Vector2(-3,0),new Vector2(3,0),new Vector2(3,2.6f),new Vector2(-3,2.6f)};
  room.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Floor,Vector3.zero,Quaternion.Euler(90,0,0),floor);
  for(int i=-1;i<=1;i++){var q=Quaternion.Euler(0,i*90,0);room.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Wall,q*Vector3.forward*3,q,wall);}
  room.UpsertBox(Guid.NewGuid(),RoomThemeKind.Table,new Vector3(1.4f,.5f,1),Quaternion.Euler(0,-15,0),Vector3.zero,Quaternion.identity,new Vector3(1.2f,1,1.3f));
  room.UpsertBox(Guid.NewGuid(),RoomThemeKind.Cabinet,new Vector3(-1.6f,.4f,.8f),Quaternion.Euler(0,20,0),Vector3.zero,Quaternion.identity,new Vector3(1,.8f,1));
  for(int i=0;i<3;i++){var actor=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+SurvivorArtBuilder.Creatures[i]));actor.transform.SetParent(origin.transform);actor.transform.position=new Vector3(-.7f+i*.75f,0,-.4f-i*.25f);actor.transform.localScale=Vector3.one*.48f;actor.transform.rotation=Quaternion.Euler(0,155,0);}
  camera.orthographic=false;camera.fieldOfView=65;camera.transform.position=new Vector3(2.7f,2.1f,-3.7f);camera.transform.LookAt(new Vector3(0,.7f,.5f));
  var caption=Label("SIMULATED CAPTURE / JADE COURTYARD",new Vector3(0,2.4f,1),camera,.036f);var image=Capture(camera,1600,1000);File.WriteAllBytes(Folder+"/v270-room-simulation.png",image.EncodeToPNG());Object.DestroyImmediate(image);Object.DestroyImmediate(caption);room.Dispose();Object.DestroyImmediate(origin);
  camera.orthographic=true;camera.orthographicSize=.75f;camera.transform.position=new Vector3(0,1.8f,-2);camera.transform.LookAt(Vector3.zero);
  sheet=new Texture2D(1440,900,TextureFormat.RGB24,false);Fill(sheet,camera.backgroundColor);
  var kinds=new[]{SkillVisual.OrbitSword,SkillVisual.ChainLightning,SkillVisual.LavaBomb,SkillVisual.BlackHole,SkillVisual.Explosion,SkillVisual.FreezePulse};
  for(int i=0;i<kinds.Length;i++){camera.orthographicSize=kinds[i]==SkillVisual.FreezePulse?1.15f:.75f;var go=SurvivorVisualCatalog.Create(kinds[i],null);var fx=go.GetComponent<SurvivorMagicEffect>();fx.Tick(.25f);fx.Fade(1);if(kinds[i]==SkillVisual.ChainLightning){go.transform.position=Vector3.left*.55f;fx.Bolt(Vector3.right*1.1f);}var frame=Capture(camera,480,450);sheet.SetPixels(i%3*480,(1-i/3)*450,480,450,frame.GetPixels());Object.DestroyImmediate(frame);Object.DestroyImmediate(go);}
  sheet.Apply();File.WriteAllBytes(Folder+"/v270-effects.png",sheet.EncodeToPNG());Object.DestroyImmediate(sheet);Debug.Log("V270 room, arsenal, creature and effect review rendered.");
 }
 static GameObject Label(string text,Vector3 position,Camera camera,float scale){var go=new GameObject("Review caption");go.transform.SetPositionAndRotation(position,camera.transform.rotation);var t=go.AddComponent<TextMesh>();t.text=text;t.fontSize=32;t.characterSize=scale;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.8f,.87f,.89f);return go;}
 static void Fill(Texture2D texture,Color c){var pixels=new Color[texture.width*texture.height];for(int i=0;i<pixels.Length;i++)pixels[i]=c;texture.SetPixels(pixels);}
 static Texture2D Capture(Camera camera,int width,int height){var previous=RenderTexture.active;var target=new RenderTexture(width,height,24);var texture=new Texture2D(width,height,TextureFormat.RGB24,false);camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(target);return texture;}
}
}
