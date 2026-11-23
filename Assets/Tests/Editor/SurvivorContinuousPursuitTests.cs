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
 [UnityTest]public IEnumerator FootRingIsHiddenWithoutReducingAttackRangeOrRoomTheme(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();game.player.range=3;var theme=game.platform.MR.Theme;int surfaces=theme.Count;Call(game,"Visuals");var ring=(LineRenderer)Get(game,"halo");
  Assert.That(ring.enabled,Is.False);game.player.stacks[(int)UpgradeId.MAGNETIC_FIELD]=1;Call(game,"Visuals");Assert.That(((GameObject)Get(game.skillVisuals,"magnet")).activeSelf,Is.False,"Passive gold pickup circle must stay hidden");
  Assert.That(game.player.range,Is.EqualTo(3));Assert.That(game.platform.MR.Theme,Is.SameAs(theme));Assert.That(theme.Count,Is.EqualTo(surfaces).And.GreaterThan(0));
  game.Menu();yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator AllSpeciesAndBossPatternsRequireActualContact(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();game.player.dodge=0;game.player.regeneration=0;
  foreach(int type in new[]{0,1,2,3,5,6,7,8,9,10}){
   var enemy=Spawn(game,0,type,game.PlayerPosition+Vector3.forward*5);enemy.speed=0;enemy.abilityCooldown=0;
   float health=game.player.health;Call(game,"EnemyAbilityStep",enemy,.01f);Call(game,"HostileSpellStep",10f);
   Assert.That(game.HostileBoltCount+game.HostileZoneCount,Is.Zero);Assert.That(game.player.health,Is.EqualTo(health),"No remote damage from species "+type);
   enemy.go.transform.position=SurvivorBestiary.Flying(type)?game.platform.Head.transform.position-Vector3.up*.42f:game.PlayerPosition+Vector3.forward*(enemy.radius+.1f);enemy.attack=0;
   Assert.That((bool)Call(game,"TryEnemyContact",enemy,1f),Is.True,"Contact must still damage: "+type);Assert.That(game.player.health,Is.LessThan(health));
   enemy.active=false;enemy.go.SetActive(false);
  }
  var boss=Spawn(game,18,4,game.PlayerPosition+Vector3.forward*4);boss.speed=0;
  for(int encounter=1;encounter<=3;encounter++)for(int sequence=0;sequence<3;sequence++){
   Set(game,"bossEncounter",encounter);Set(game,"bossAttackIndex",sequence);Set(game,"bossDash",0f);boss.telegraph=boss.aoe=0;
   boss.go.transform.position=game.PlayerPosition+Vector3.forward*4;float health=game.player.health;
   Call(game,"BossCombatStep",boss,.01f);Call(game,"BossCombatStep",boss,2f);Call(game,"HostileSpellStep",10f);
   Assert.That(game.player.health,Is.EqualTo(health),"Boss windup completion must not hit remotely");Assert.That(game.HostileBoltCount,Is.Zero);
  }
  game.Menu();yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator GroundPursuitCrossesFurnitureAndMissingFloorWithoutStopping(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Begin();game.platform.enabled=false;game.platform.Head.transform.position=new Vector3(100,1.65f,100);
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=RoomNavigation.Layer;wall.transform.position=new Vector3(102,1,100);wall.transform.localScale=new Vector3(.8f,2,10);Physics.SyncTransforms();game.platform.RebuildRoom();
  Assert.That(game.platform.Navigation.Ready,Is.False);Assert.That(game.Ready,Is.True,"An active tracked run cannot pause for missing floor cells");
  Set(game.platform,"<Tracking>k__BackingField",false);Assert.That(game.Ready,Is.False,"Actual tracking loss must still pause combat");Set(game.platform,"<Tracking>k__BackingField",true);
  var enemy=Spawn(game,0,0,new Vector3(105,0,100));Assert.That(game.platform.Navigation.Contains(enemy.go.transform.position,enemy.radius),Is.False);
  for(int i=0;i<300;i++){var before=enemy.go.transform.position;game.Step(1/90f);Assert.That(enemy.go.transform.position.x,Is.LessThan(before.x),"Must advance directly even with no scanned path");}
  Assert.That(enemy.go.transform.position.x,Is.LessThan(102),"Creature must cross the furniture collider");
  var crowd=(SurvivorGame.Enemy[])Get(game,"enemies");enemy.go.transform.position=new Vector3(110,0,100);
  // An existing overlap can block every avoidance candidate. The fallback must
  // still advance toward the nearest target instead of waiting for a free cell.
  var blocker=Spawn(game,1,4,enemy.go.transform.position);blocker.freeze=100;blocker.falling=false;
  var start=enemy.go.transform.position;var next=SurvivorEnemyCrowd.Move(0,crowd,game.PlayerPosition,.02f,game.platform.Navigation);
  Assert.That(next.x,Is.LessThan(start.x));Assert.That(next.z,Is.EqualTo(start.z).Within(.0001f));
  game.Menu();Object.DestroyImmediate(wall);yield return new ExitPlayMode();
 }

}
}
