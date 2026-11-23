using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace SpatialSurvivor.Tests {
public class SurvivorContinuousPursuitTests {
 const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Get(object target,string name)=>target.GetType().GetField(name,Private).GetValue(target);
 static void Set(object target,string name,object value)=>target.GetType().GetField(name,Private).SetValue(target,value);
 static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Private).Invoke(target,args);
 static SurvivorGame Begin(){
  var game=Object.FindFirstObjectByType<SurvivorGame>();game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;game.StartRun();
  game.player.health=game.player.maxHealth=1000000;game.player.damage=0;Array.Clear(game.player.stacks,0,game.player.stacks.Length);Set(game,"spawn",1000f);return game;
 }
 static SurvivorGame.Enemy Spawn(SurvivorGame game,int index,int type,Vector3 position){
  var enemy=((SurvivorGame.Enemy[])Get(game,"enemies"))[index];Assert.That((bool)Call(game,"Spawn",enemy,type));
  enemy.go.transform.position=position;enemy.y=game.platform.Navigation.Floor;enemy.falling=false;enemy.speed=1;enemy.abilityCooldown=1000;enemy.repath=0;return enemy;
 }
 [UnityTest]public IEnumerator DistantEnemyAdvancesOnEveryCombatStep(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.layer=RoomNavigation.Layer;floor.transform.position=new Vector3(100,-.1f,100);floor.transform.localScale=new Vector3(26,.2f,8);
  game.platform.Head.transform.position=new Vector3(105,1.65f,100);Physics.SyncTransforms();game.platform.RebuildRoom();
  var enemy=Spawn(game,0,0,new Vector3(94,0,100));
  for(int i=0;i<24;i++){var before=enemy.go.transform.position;game.Step(1/90f);Assert.That(SurvivorGame.Distance(before,enemy.go.transform.position),Is.GreaterThan(.005f),"Far enemies must not skip three of four movement steps");}
  game.Menu();Object.DestroyImmediate(floor);yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator WindupAndHitReactionKeepMovingWhileFreezeStillStopsMovement(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();var enemy=Spawn(game,6,1,game.PlayerPosition+Vector3.forward*2.8f);enemy.abilityCooldown=0;
  for(int i=0;i<24;i++){var before=enemy.go.transform.position;game.Step(1/90f);Assert.That(SurvivorGame.Distance(before,enemy.go.transform.position),Is.GreaterThan(.003f),"Charge anticipation must continue pursuit");}
  Assert.That(enemy.abilityWindup,Is.GreaterThan(0));enemy.abilityWindup=0;enemy.abilityCooldown=1000;enemy.stagger=.2f;enemy.knockRemaining=0;
  var hitPosition=enemy.go.transform.position;game.Step(.02f);Assert.That(SurvivorGame.Distance(hitPosition,enemy.go.transform.position),Is.GreaterThan(.003f),"Hit feedback must not suspend pursuit");
  enemy.freeze=.5f;var frozen=enemy.go.transform.position;game.Step(.1f);Assert.That(enemy.go.transform.position,Is.EqualTo(frozen),"Freeze skill remains a real immobilization");
  enemy.freeze=0;game.Pause();var paused=enemy.go.transform.position;game.Step(1);Assert.That(enemy.go.transform.position,Is.EqualTo(paused));game.Pause();game.Menu();yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator BossTelegraphKeepsMovingAndLockedWarningDoesNotFollow(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();var boss=Spawn(game,18,4,game.PlayerPosition-Vector3.forward*3);boss.speed=.5f;Set(game,"bossEncounter",1);Set(game,"bossAttackIndex",0);boss.aoe=0;
  var beforeStart=boss.go.transform.position;game.Step(.02f);Assert.That(SurvivorGame.Distance(beforeStart,boss.go.transform.position),Is.GreaterThan(.001f));Assert.That(boss.telegraph,Is.GreaterThan(0));
  var warning=((Vector3[])Get(game,"bossTargets"))[0];
  for(int i=0;i<24;i++){var before=boss.go.transform.position;game.Step(1/90f);Assert.That(SurvivorGame.Distance(before,boss.go.transform.position),Is.GreaterThan(.001f),"Boss casting must not stop chase");Assert.That(((Vector3[])Get(game,"bossTargets"))[0],Is.EqualTo(warning),"A locked telegraph must remain avoidable");}
  game.Menu();yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator FootRingIsHalfSizeWithoutReducingAttackRangeOrRoomTheme(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();game.player.range=3;var theme=game.platform.MR.Theme;int surfaces=theme.Count;Call(game,"Visuals");var ring=(LineRenderer)Get(game,"halo");
  for(int i=0;i<ring.positionCount;i++)Assert.That(SurvivorGame.Distance(ring.GetPosition(i),game.PlayerPosition),Is.EqualTo(1.5f).Within(.0001f));
  Assert.That(game.player.range,Is.EqualTo(3));Assert.That(game.platform.MR.Theme,Is.SameAs(theme));Assert.That(theme.Count,Is.EqualTo(surfaces).And.GreaterThan(0));
  game.Menu();yield return new ExitPlayMode();
 }
}
}
