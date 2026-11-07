using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
namespace SpatialSurvivor.Tests {
 public class SurvivorSceneTests {
  [UnityTest] public IEnumerator SceneRunsCombatUpgradeAndBossWithoutMissingAssets() {
   EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");
   yield return new EnterPlayMode();
   yield return null;
   yield return null;
   var game=Object.FindObjectOfType<SurvivorGame>();
   Assert.That(game,Is.Not.Null);
   for(int warmup=0;warmup<10&&!game.Ready;warmup++)yield return null;
   Assert.That(game.Ready,Is.True,"Simulated scanned room must be ready");
   Assert.That(Resources.Load<GameObject>("Art/Creatures/Mossling"),Is.Not.Null);
   Assert.That(Resources.Load<GameObject>("Art/Creatures/ElderBloom"),Is.Not.Null);
   foreach(var name in SurvivorVisualCatalog.Models)if(name!=null)Assert.That(Resources.Load<GameObject>("Art/Weapons/"+name),Is.Not.Null,name);
   Assert.That(game.skillVisuals.Capacity,Is.GreaterThanOrEqualTo(50));
   Assert.That(((System.Array)typeof(SurvivorGame).GetField("swords",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game)).Length,Is.EqualTo(14));
   Assert.That(((System.Array)typeof(SurvivorGame).GetField("crystals",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game)).Length,Is.EqualTo(48));
   Capture(game,"menu");
   game.screen=ScreenState.MR;yield return null;yield return null;
   Assert.That(Object.FindObjectsOfType<UnityEngine.UI.Button>().Any(button=>button.name=="刷新编辑器模拟房间"),Is.True,"MR menu must expose room scan action");
   Assert.That(Object.FindObjectsOfType<UnityEngine.UI.Button>().Any(button=>button.name=="重新授权 / 重试 MR 服务"),Is.True);
   game.platform.MR.ToggleMesh();Assert.That(game.platform.MR.ShowMesh,Is.True);game.platform.MR.ToggleMesh();
   Capture(game,"mr-settings");game.screen=ScreenState.Menu;yield return null;
   game.StartRun();
   game.player.maxHealth=100000;game.player.health=100000;
   game.player.damage=1000;game.player.range=30;game.player.pickup=30;
   for(int frame=0;frame<90*18;frame++) {
    if(game.screen==ScreenState.Upgrade)game.Select(0);
    game.Step(SurvivorRules.Step);
    if(frame%90==0)yield return null;
   }
   Assert.That(game.player.kills,Is.GreaterThan(0));
   Assert.That(game.player.level,Is.GreaterThan(1),"Combat drops must grant experience and offer cards");
   Assert.That(game.history.Count,Is.GreaterThan(0));
   if(game.screen==ScreenState.Upgrade)game.Select(0);
   game.Pause();Assert.That(game.screen,Is.EqualTo(ScreenState.Paused));
   float pausedTime=game.player.elapsed;game.Step(1);Assert.That(game.player.elapsed,Is.EqualTo(pausedTime));
   game.Pause();
   foreach(var card in SurvivorRules.Cards)game.player.Apply(card.id);
   for(int frame=0;frame<180;frame++)game.Step(SurvivorRules.Step);
   if(game.screen==ScreenState.Upgrade)game.Select(0);
   Assert.That(game.skillVisuals.Emit(SkillVisual.IceSpike,game.PlayerPosition,.4f,Quaternion.identity),Is.True);
   Assert.That(game.skillVisuals.Emit(SkillVisual.AbsoluteZero,game.PlayerPosition,.4f,Quaternion.identity),Is.True);
   int visible=game.skillVisuals.ActiveCount;game.Pause();for(int frame=0;frame<20;frame++)game.Step(SurvivorRules.Step);Assert.That(game.skillVisuals.ActiveCount,Is.EqualTo(visible),"Paused effects must stay aligned with combat");game.Pause();
   game.player.damage=20;game.player.range=2;
   var normalPool=(SurvivorGame.Enemy[])typeof(SurvivorGame).GetField("enemies",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game);
   var victim=normalPool.First(enemy=>enemy.type==0);typeof(SurvivorGame).GetMethod("Spawn",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(game,new object[]{victim,0});
   game.Damage(victim,100000,false);Assert.That(victim.active,Is.False);Assert.That(victim.go.activeSelf,Is.True,"Death must keep the pooled model visible during the shrink animation");
   for(int frame=0;frame<45;frame++){if(game.screen==ScreenState.Upgrade)game.Select(0);game.Step(SurvivorRules.Step);}Assert.That(victim.deathRemaining,Is.Zero);
   game.options=SurvivorRules.Draw(game.player,new System.Random(30));game.screen=ScreenState.Upgrade;yield return new WaitForSecondsRealtime(.2f);Capture(game,"upgrade");game.Select(0);
   Assert.That(game.Feedback,Is.Not.Null);
   var clips=(AudioClip[])typeof(SurvivorFeedback).GetField("clips",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game.Feedback);Assert.That(clips.Length,Is.EqualTo(13));Assert.That(clips.All(clip=>clip&&clip.samples>0),Is.True);
   game.player.elapsed=600;game.Step(SurvivorRules.Step);
   var pool=(SurvivorGame.Enemy[])typeof(SurvivorGame).GetField("enemies",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game);
   Assert.That(pool.Any(e=>e.active&&e.type==4),Is.True,"Boss must spawn within scanned boundaries");
   game.platform.Head.transform.rotation=Quaternion.LookRotation(pool.First(enemy=>enemy.active&&enemy.type==4).go.transform.position+Vector3.up*.5f-game.platform.Head.transform.position);
   game.screen=ScreenState.Playing;game.options.Clear();game.enabled=false;
   yield return new WaitForSecondsRealtime(.5f);Assert.That(game.screen,Is.EqualTo(ScreenState.Playing));Capture(game,"boss-and-skills");game.enabled=true;
   Assert.That(Object.FindObjectsOfType<UnityEngine.UI.Image>().Any(image=>image.name=="Spell Icon"),Is.True);
   System.IO.File.WriteAllText("Logs/scene-smoke-status.txt","Passed: scene boot, scanned room, models, combat kills, XP, modal card selection, pause, all weapon/evolution models, 14 swords, 48 crystals, paused FX, storybook HUD icons, boss spawn.");
   game.screen=ScreenState.Paused;yield return null;Capture(game,"paused");game.screen=ScreenState.Progression;yield return null;Capture(game,"progression");game.screen=ScreenState.Victory;yield return null;Capture(game,"victory");game.screen=ScreenState.Defeat;yield return null;Capture(game,"defeat");
   game.Menu();Assert.That(pool.Any(enemy=>enemy.go.activeSelf),Is.False,"Main menu must clear pooled combat visuals");Assert.That(game.skillVisuals.ActiveCount,Is.Zero);
   // Exercise the rewritten crystal lifecycle without depending on spawn RNG.
   game.StartRun();game.player.damage=0;game.player.range=0;game.player.pickup=1.1f;
   Call(game,"Drop",game.PlayerPosition+Vector3.right,1);
   var crystalPool=(System.Array)Private(game,"crystals");var crystal=crystalPool.GetValue(0);
   Call(game,"CrystalStep",.05f);
   Assert.That((bool)crystal.GetType().GetField("attracting").GetValue(crystal),Is.True);
   game.platform.Head.transform.position+=Vector3.left*2;
   float before=SurvivorGame.Distance((Vector3)crystal.GetType().GetField("position").GetValue(crystal),game.PlayerPosition);
   for(int frame=0;frame<10;frame++)Call(game,"CrystalStep",SurvivorRules.Step);
   float after=SurvivorGame.Distance((Vector3)crystal.GetType().GetField("position").GetValue(crystal),game.PlayerPosition);
   Assert.That(after,Is.LessThan(before),"Acquired crystal must keep chasing outside pickup radius");
   game.StartRun();game.player.experienceMultiplier=1.1f;
   for(int i=0;i<49;i++)Call(game,"Drop",game.PlayerPosition+Vector3.right*3,1);
   int stored=0;foreach(var entry in (System.Array)Private(game,"crystals"))stored+=(int)entry.GetType().GetField("exp").GetValue(entry);
   Assert.That(stored,Is.EqualTo(98),"Pool saturation must preserve per-drop EXP rounding");
   game.StartRun();game.player.Apply(UpgradeId.FREEZE_PULSE);
   game.options=new System.Collections.Generic.List<Card>{SurvivorRules.Definition(UpgradeId.ATTACK_DAMAGE)};game.screen=ScreenState.Upgrade;game.Select(0);
   var effect=((System.Array)Private(game,"fx")).Cast<object>().First(entry=>(float)entry.GetType().GetField("life").GetValue(entry)>0);
   float ringLife=(float)effect.GetType().GetField("life").GetValue(effect);
   game.Pause();yield return new WaitForSecondsRealtime(.15f);
   Assert.That((float)effect.GetType().GetField("life").GetValue(effect),Is.EqualTo(ringLife),"Render frames must not expire paused combat rings");game.Pause();
   var fresh=normalPool.First(enemy=>enemy.type==0);Assert.That((bool)Call(game,"Spawn",fresh,0),Is.True);
   Assert.That(fresh.freeze,Is.GreaterThan(1.9f),"Freeze pulse must include newly spawned enemies");
   game.player.maxHealth=game.player.health=100000;game.player.elapsed=600;game.Step(SurvivorRules.Step);
   string isolatedDirectory=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"SpatialSurvivorScene",System.Guid.NewGuid().ToString("N"));
   typeof(SurvivorGame).GetField("progressStore",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(game,new SurvivorProgressStore(System.IO.Path.Combine(isolatedDirectory,"progress.json")));
   game.meta=new MetaSave();game.Damage(game.Boss,100000,false);
   Assert.That(game.screen,Is.EqualTo(ScreenState.Victory));int settled=game.meta.crystals;int kills=game.player.kills;float endedAt=game.player.elapsed;
   fresh.active=true;fresh.hp=100;game.Damage(fresh,10000,false);game.Hurt(1000000);game.Step(1);
   Assert.That(fresh.hp,Is.EqualTo(100));Assert.That(game.player.kills,Is.EqualTo(kills));Assert.That(game.player.elapsed,Is.EqualTo(endedAt));Assert.That(game.meta.crystals,Is.EqualTo(settled),"Settlement must only be awarded once");
   game.Menu();System.IO.Directory.Delete(isolatedDirectory,true);
   System.IO.File.WriteAllText("Logs/scene-smoke-status.txt","Passed: scene boot, simulated room, all weapon/evolution models, combat/XP/card selection, 14 swords/48 crystals, boss, audio clips, crystal attraction latch/acceleration, merged EXP rounding, paused combat rings, freeze pulse on new spawns, one-shot settlement, terminal combat stop, menu cleanup.");
   yield return new ExitPlayMode();
  }
  static object Private(object target,string field)=>target.GetType().GetField(field,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(target);
  static object Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(target,args);
  static void Capture(SurvivorGame game,string name){var camera=game.platform.Head;if(game.screen!=ScreenState.Playing){var f=RoomNavigation.Horizontal(camera.transform.forward);camera.transform.rotation=Quaternion.LookRotation(f.sqrMagnitude>.1f?f:Vector3.forward);Call(game.GetComponent<SurvivorUI>(),"Build");}var previous=camera.targetTexture;var texture=new RenderTexture(1280,800,24);var image=new Texture2D(1280,800,TextureFormat.RGB24,false);var old=RenderTexture.active;try{camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();System.IO.Directory.CreateDirectory("Docs/Validation");System.IO.File.WriteAllBytes("Docs/Validation/"+name+".png",image.EncodeToPNG());}finally{camera.targetTexture=previous;RenderTexture.active=old;Object.Destroy(texture);Object.Destroy(image);}}
 }
}
