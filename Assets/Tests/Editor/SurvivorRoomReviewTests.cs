using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace SpatialSurvivor.Tests {
public class SurvivorRoomReviewTests {
 static readonly Vector2[] Square={new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)};
 [Test] public void ModalAnchorSurvivesContentRefreshAndTrackingOriginMoves(){
  var origin=new GameObject("origin");var head=new GameObject("head");var panel=new GameObject("panel");
  try{head.transform.position=new Vector3(0,1.65f,0);var anchor=new SurvivorModalAnchor();anchor.Apply(panel.transform,head.transform,origin.transform);
   var position=panel.transform.position;var rotation=panel.transform.rotation;
   head.transform.SetPositionAndRotation(new Vector3(.4f,1.8f,.3f),Quaternion.Euler(0,65,0));
   anchor.Apply(panel.transform,head.transform,origin.transform);Assert.That(panel.transform.position,Is.EqualTo(position));Assert.That(Quaternion.Angle(panel.transform.rotation,rotation),Is.LessThan(.001f));
   origin.transform.SetPositionAndRotation(Vector3.right*2,Quaternion.Euler(0,30,0));anchor.Apply(panel.transform,head.transform,origin.transform);
   Assert.That(Vector3.Distance(panel.transform.position,origin.transform.TransformPoint(position)),Is.LessThan(.001f));
   anchor.Close();anchor.Apply(panel.transform,head.transform,origin.transform);Assert.That(Quaternion.Angle(panel.transform.rotation,head.transform.rotation),Is.LessThan(.001f));
  }finally{Object.DestroyImmediate(panel);Object.DestroyImmediate(head);Object.DestroyImmediate(origin);}
 }
 [Test] public void RoomDressingUsesMeasuredPoseAndCannotChangeNavigation(){
  var origin=new GameObject("origin");var theme=new SurvivorRoomTheme(origin.transform);var id=Guid.NewGuid();
  try{origin.transform.SetPositionAndRotation(Vector3.right*2,Quaternion.Euler(0,35,0));
   var offset=new Vector3(.2f,.1f,-.1f);var boxRotation=Quaternion.Euler(0,25,0);var extent=new Vector3(1.2f,.8f,1.6f);
   Assert.That(theme.UpsertBox(id,RoomThemeKind.Table,Vector3.forward,Quaternion.Euler(0,20,0),offset,boxRotation,extent),Is.True);
   var go=theme.Root.GetChild(0);Assert.That(go.localPosition,Is.EqualTo(Vector3.forward));
   var mesh=go.GetComponent<MeshFilter>().sharedMesh;
   foreach(var point in mesh.vertices){var measured=Quaternion.Inverse(boxRotation)*(point-offset);Assert.That(Mathf.Abs(measured.x),Is.LessThanOrEqualTo(extent.x*.501f));Assert.That(Mathf.Abs(measured.y),Is.LessThanOrEqualTo(extent.y*.501f));Assert.That(Mathf.Abs(measured.z),Is.LessThanOrEqualTo(extent.z*.501f));}
   Assert.That(theme.Root.GetComponentsInChildren<Collider>(true),Is.Empty);Assert.That(mesh.vertexCount,Is.LessThan(2000));
   var material=go.GetComponent<Renderer>().sharedMaterial;Assert.That(material.renderQueue,Is.LessThan(2000));
   Assert.That(theme.UpsertBox(id,RoomThemeKind.Table,Vector3.zero,Quaternion.identity,offset,boxRotation,extent),Is.True);Assert.That(theme.Count,Is.EqualTo(1));
   theme.SetVisible(false);Assert.That(theme.Root.gameObject.activeSelf,Is.False);theme.SetVisible(true);
   theme.Remove(id);Assert.That(theme.Count,Is.Zero);Assert.That(theme.Root.childCount,Is.Zero);
  }finally{theme.Dispose();Object.DestroyImmediate(origin);}
 }
 [Test] public void ConcaveFloorAndDoorKeepTheirOpenRegions(){
  var origin=new GameObject("origin");var theme=new SurvivorRoomTheme(origin.transform);
  try{var polygon=new[]{new Vector2(0,0),new Vector2(3,0),new Vector2(3,1),new Vector2(1,1),new Vector2(1,3),new Vector2(0,3)};
   theme.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Floor,Vector3.zero,Quaternion.identity,polygon);var mesh=theme.Root.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
   var v=mesh.vertices;var ix=mesh.triangles;
   for(int i=0;i<ix.Length;i+=3){var center=(v[ix[i]]+v[ix[i+1]]+v[ix[i+2]])/3;Assert.That(center.x>1.03f&&center.y>1.03f,Is.False,"Dressing must not fill the unscanned recess");}
   theme.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Door,Vector3.zero,Quaternion.identity,Square);
   var door=theme.Root.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
   foreach(var p in door.vertices)Assert.That(Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.y)),Is.GreaterThan(.98f),"Keep the actual doorway open");
   for(int i=0;i<100;i++)theme.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Wall,Vector3.zero,Quaternion.identity,Square);
   Assert.That(theme.Count,Is.EqualTo(SurvivorRoomTheme.SurfaceLimit));theme.Clear();Assert.That(theme.Count,Is.Zero);
  }finally{theme.Dispose();Object.DestroyImmediate(origin);}
 }
 [Test] public void InvalidCaptureCannotAllocateVisualObjects(){var origin=new GameObject("origin");var theme=new SurvivorRoomTheme(origin.transform);try{
  Assert.That(theme.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Floor,Vector3.zero,Quaternion.identity,new[]{Vector2.zero,new Vector2(float.NaN,0),Vector2.one}),Is.False);
  Assert.That(theme.UpsertBox(Guid.NewGuid(),RoomThemeKind.Table,Vector3.zero,Quaternion.identity,Vector3.zero,Quaternion.identity,Vector3.zero),Is.False);
  Assert.That(theme.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Unknown,Vector3.zero,Quaternion.identity,Square),Is.False);Assert.That(theme.Count,Is.Zero);
 }finally{theme.Dispose();Object.DestroyImmediate(origin);}}
 [Test] public void LightningHasTaperedCoreHaloAndResetsAfterPooling(){
  var go=SurvivorVisualCatalog.Create(SkillVisual.ChainLightning,null);try{var fx=go.GetComponent<SurvivorMagicEffect>();fx.Bolt(Vector3.forward*2);var lines=go.GetComponentsInChildren<LineRenderer>();
   Assert.That(lines.Length,Is.EqualTo(2));Assert.That(lines[1].widthMultiplier,Is.GreaterThan(lines[0].widthMultiplier));Assert.That(lines[0].widthCurve.Evaluate(1),Is.Zero);
   fx.Fade(0);Assert.That(lines[0].startColor.a,Is.Zero);fx.Restart();fx.Bolt(Vector3.right);Assert.That(lines[0].startColor.a,Is.GreaterThan(.5f));Assert.That(lines[0].GetPosition(12),Is.EqualTo(Vector3.right));
  }finally{Object.DestroyImmediate(go);}
 }
 [UnityTest] public IEnumerator ActualButtonsKeepTheSamePanelPoseAcrossPages(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindFirstObjectByType<SurvivorGame>();var ui=game.GetComponent<SurvivorUI>();game.enabled=false;
  Assert.That(ui.enabled,Is.True,"The UI must update while the game clock is paused");
  var field=typeof(SurvivorUI).GetField("panel",BindingFlags.Instance|BindingFlags.NonPublic);
  var update=typeof(SurvivorUI).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
  var panel=(Canvas)field.GetValue(ui);var position=panel.transform.position;var rotation=panel.transform.rotation;
  game.platform.Head.transform.SetPositionAndRotation(new Vector3(.25f,1.7f,.1f),Quaternion.Euler(0,25,0));
  foreach(var page in new[]{ScreenState.Shop,ScreenState.Skills,ScreenState.Bestiary,ScreenState.Comfort,ScreenState.MR}){
   game.screen=page;update.Invoke(ui,null);yield return null;panel=(Canvas)field.GetValue(ui);
   Assert.That(panel.name,Is.EqualTo(page.ToString()),"The new page must actually rebuild");
   Assert.That(Vector3.Distance(position,panel.transform.position),Is.LessThan(.001f));Assert.That(Quaternion.Angle(rotation,panel.transform.rotation),Is.LessThan(.01f));
  }
  int navigationCount=game.platform.Navigation.Count;var themeButton=Array.Find(panel.GetComponentsInChildren<Button>(),b=>b.name.StartsWith("魔法场景："));Assert.That(themeButton,Is.Not.Null);themeButton.onClick.Invoke();update.Invoke(ui,null);yield return null;
  panel=(Canvas)field.GetValue(ui);Assert.That(Vector3.Distance(position,panel.transform.position),Is.LessThan(.001f));game.platform.RebuildRoom();Assert.That(game.platform.Navigation.Count,Is.EqualTo(navigationCount));
  game.platform.MR.ToggleTheme(); // Restore the saved preference after testing.
  var recenter=Array.Find(panel.GetComponentsInChildren<Button>(),b=>b.name=="移到面前");recenter.onClick.Invoke();Assert.That(Vector3.Distance(position,panel.transform.position),Is.GreaterThan(.2f));
  game.screen=ScreenState.Playing;update.Invoke(ui,null);yield return null;game.screen=ScreenState.Paused;update.Invoke(ui,null);yield return null;
  Assert.That(field.GetValue(ui),Is.Not.Null);yield return new ExitPlayMode();
 }
}
}
