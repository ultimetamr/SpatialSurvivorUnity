using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace SpatialSurvivor.Tests {
public class SurvivorRoomPursuitTests {
 const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Set(object target,string field,object value)=>target.GetType().GetField(field,Private).SetValue(target,value);
 [Test]public void PartialLiveMeshKeepsCompleteCapturedFloorInvisibleAndTraversable(){
  var root=new GameObject("Pursuit room fixture");var platform=root.AddComponent<SurvivorPlatform>();platform.EnsureRig();platform.Head.transform.position=new Vector3(100,1.65f,100);
  var session=root.GetComponent<SurvivorMrSession>();if(!session)session=root.AddComponent<SurvivorMrSession>();Set(session,"platform",platform);
  var material=new Material(Resources.Load<Shader>("Shaders/RoomOcclusion"));var captured=new SurvivorRoomGeometry(platform.Origin,"capture",material);var live=new SurvivorRoomGeometry(platform.Origin,"live",material);
  Set(session,"capturedRoom",captured);Set(session,"liveRoom",live);
  try{
   Vector3[] Floor(float extent)=>new[]{new Vector3(-extent,0,-extent),new Vector3(-extent,0,extent),new Vector3(extent,0,extent),new Vector3(extent,0,-extent)};
   captured.Upsert(Guid.NewGuid(),new Vector3(100,0,100),Quaternion.identity,Floor(4),new[]{0,1,2,0,2,3});
   live.Upsert(Guid.NewGuid(),new Vector3(100,0,100),Quaternion.identity,Floor(.75f),new[]{0,1,2,0,2,3});
   session.RebuildNavigation();Assert.That(platform.Navigation.Ready);
   Assert.That(platform.Navigation.Contains(new Vector3(103,0,100),.1f),Is.True,"A valid small live patch must not disable the full captured room");
   Assert.That(platform.Navigation.Waypoint(new Vector3(103,0,100),new Vector3(100,0,100),.1f).x,Is.LessThan(103));
   foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())Assert.That(renderer.enabled,Is.False);
  }finally{Set(session,"capturedRoom",null);Set(session,"liveRoom",null);captured.Dispose();live.Dispose();Object.DestroyImmediate(root);Object.DestroyImmediate(material);}
 }
 [UnityTest]public IEnumerator WalkingRefreshesNavigationAndEnemiesFollowBeyondTheInitialWindow(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindFirstObjectByType<SurvivorGame>();game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;var platform=game.platform;platform.enabled=false;
  var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.layer=RoomNavigation.Layer;floor.transform.position=new Vector3(100,-.1f,100);floor.transform.localScale=new Vector3(44,.2f,8);
  platform.Head.transform.position=new Vector3(100,1.65f,100);Physics.SyncTransforms();platform.RebuildRoom();game.StartRun();game.player.health=game.player.maxHealth=1000000;game.player.damage=0;Array.Clear(game.player.stacks,0,game.player.stacks.Length);Set(game,"spawn",1000f);
  var pool=(SurvivorGame.Enemy[])typeof(SurvivorGame).GetField("enemies",Private).GetValue(game);var enemy=pool[0];Assert.That((bool)typeof(SurvivorGame).GetMethod("Spawn",Private).Invoke(game,new object[]{enemy,0}));
  enemy.go.transform.position=new Vector3(99,0,100);enemy.y=0;enemy.speed=1.1f;enemy.abilityCooldown=1000;
  var update=typeof(SurvivorPlatform).GetMethod("Update",Private);
  for(int i=0;i<1200;i++){
   platform.Head.transform.position=new Vector3(100+16f*(i+1)/1200,1.65f,100);
   if(i%60==0){Set(platform,"nextRebuild",-1f);update.Invoke(platform,null);}
   game.Step(1/60f);
  }
  Assert.That(enemy.active,Is.True);Assert.That(enemy.go.transform.position.x,Is.GreaterThan(114),"Enemy must follow a moving player beyond the initial cached room window");
  Assert.That(SurvivorGame.Distance(enemy.go.transform.position,game.PlayerPosition),Is.LessThan(2));
  Assert.That(platform.Navigation.Contains(game.PlayerPosition,.1f));game.Menu();Object.DestroyImmediate(floor);yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator FlyingCreatureKeepsPursuingAroundAFullHeightWall(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindFirstObjectByType<SurvivorGame>();game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;game.platform.Head.transform.position=new Vector3(0,1.65f,-2.5f);
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=RoomNavigation.Layer;wall.transform.position=new Vector3(0,1.2f,0);wall.transform.localScale=new Vector3(3,2.4f,.2f);Physics.SyncTransforms();game.platform.RebuildRoom();game.StartRun();
  game.player.health=game.player.maxHealth=1000000;game.player.damage=0;Array.Clear(game.player.stacks,0,game.player.stacks.Length);Set(game,"spawn",1000f);
  var pool=(SurvivorGame.Enemy[])typeof(SurvivorGame).GetField("enemies",Private).GetValue(game);var enemy=pool[19];Assert.That((bool)typeof(SurvivorGame).GetMethod("Spawn",Private).Invoke(game,new object[]{enemy,5}));
  enemy.go.transform.position=new Vector3(0,1.25f,2.5f);enemy.y=1.25f;enemy.speed=1.1f;enemy.abilityCooldown=1000;
  for(int i=0;i<1100;i++){game.Step(1/90f);Assert.That(game.platform.Navigation.ContainsAir(enemy.go.transform.position,enemy.radius));}
  Assert.That(enemy.go.transform.position.z,Is.LessThan(-1),"Flying enemy must get around the wall and keep following the player");
  game.Menu();Object.DestroyImmediate(wall);yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator RuntimeRoomThemeIsPreservedWhileRawNavigationMeshesRemainHidden(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var platform=Object.FindFirstObjectByType<SurvivorPlatform>();Assert.That(platform.Navigation.Ready);
  var session=platform.MR;Assert.That(session.Theme,Is.Not.Null);Assert.That(session.Theme.Count,Is.GreaterThan(0));
  bool wasVisible=session.ThemeVisible;
  if(!wasVisible)session.ToggleTheme();
  Assert.That(GameObject.Find("Enchanted Room - Scanned Measurements"),Is.Not.Null);
  Assert.That(session.Theme.Root.GetComponentsInChildren<MeshRenderer>().Length,Is.GreaterThan(0));
  session.ToggleTheme();Assert.That(session.Theme.Root.gameObject.activeSelf,Is.False);
  foreach(var renderer in platform.GetComponentsInChildren<MeshRenderer>())if(renderer.gameObject.layer==RoomNavigation.Layer)Assert.That(renderer.enabled,Is.False,"Turning theme off must not show raw simulation meshes");
  if(wasVisible)session.ToggleTheme();
  foreach(var renderer in platform.GetComponentsInChildren<MeshRenderer>())if(renderer.gameObject.layer==RoomNavigation.Layer)Assert.That(renderer.enabled,Is.False,"Navigation geometry must remain invisible");
  yield return new ExitPlayMode();
 }
}
}
