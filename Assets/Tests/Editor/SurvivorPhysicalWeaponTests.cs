using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace SpatialSurvivor.Tests {
public class SurvivorPhysicalWeaponTests {
 static SurvivorPointerSample Sample(Vector3 p,Quaternion q,bool pressed=false,SurvivorPointerSource source=SurvivorPointerSource.RightController)=>new SurvivorPointerSample(source,new Ray(p,q*Vector3.forward),pressed);
 [Test]public void BriefDropoutKeepsWeaponVisibleButNeverAuthorizesDamage(){var t=new SurvivorWeaponTracking();var q=Quaternion.Euler(30,50,85);t.Sample(Sample(new Vector3(.4f,1,2),q,true),q,0);Assert.That(t.Fresh&&t.Pressed);Assert.That(Quaternion.Angle(t.Rotation,q),Is.LessThan(.01f));var last=t.Ray;t.Sample(default,Quaternion.identity,.1f);Assert.That(t.Visible);Assert.That(t.Fresh||t.Pressed,Is.False);Assert.That(t.Ray.origin,Is.EqualTo(last.origin));t.Sample(default,Quaternion.identity,.23f);Assert.That(t.Visible,Is.False);}
 [Test]public void MissingRightFrameCannotTeleportWeaponIntoLeftHand(){var t=new SurvivorWeaponTracking();var q=Quaternion.identity;t.Sample(Sample(Vector3.right,q),q,0);int rev=t.Revision;t.Sample(Sample(Vector3.left,q,true,SurvivorPointerSource.LeftController),q,.03f);Assert.That(t.Source,Is.EqualTo(SurvivorPointerSource.RightController));Assert.That(t.Visible&&!t.Fresh&&!t.Pressed);t.Sample(Sample(Vector3.right,q),q,.04f);Assert.That(t.Fresh);Assert.That(t.Revision,Is.EqualTo(rev));t.Sample(Sample(Vector3.left,q,false,SurvivorPointerSource.LeftController),q,.3f);Assert.That(t.Source,Is.EqualTo(SurvivorPointerSource.LeftController));Assert.That(t.Revision,Is.GreaterThan(rev));}
 [Test]public void RecoveredPoseStartsNewSwingAndInvalidCoordinatesAreRejected(){var t=new SurvivorWeaponTracking();var q=Quaternion.identity;t.Sample(Sample(Vector3.zero,q),q,0);int rev=t.Revision;t.Sample(default,q,.1f);t.Sample(Sample(Vector3.right,q,true),q,.11f);Assert.That(t.Revision,Is.GreaterThan(rev));t.Sample(Sample(new Vector3(float.NaN,0,0),q,true),q,.12f);Assert.That(t.Fresh||t.Pressed,Is.False);t.Reset();Assert.That(t.Visible,Is.False);}
 [Test]public void FreePistolAndPaidShotgunPreserveExistingWeaponIndices(){Assert.That((int)HandWeaponKind.VoidScepter,Is.EqualTo(5));Assert.That(SurvivorArmory.Weapons.Length,Is.EqualTo(8));var save=new MetaSave();Assert.That(save.BuyWeapon(6));Assert.That(save.coins,Is.Zero);Assert.That(save.OwnsWeapon(6));Assert.That(save.BuyWeapon(7),Is.False);save.coins=240;Assert.That(save.BuyWeapon(7));Assert.That(save.coins,Is.Zero);Assert.That(save.OwnsWeapon(6)&&save.OwnsWeapon(7));}
 [UnityTest]public IEnumerator RotatingBladeSweepsBetweenFramesWithoutTriggerAndCannotHitThroughWalls(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindObjectOfType<SurvivorGame>();game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;game.meta.equippedWeapon=1;game.StartRun();game.player.damage=20;game.player.critChance=0;
  var pool=(SurvivorGame.Enemy[])Get(game,"enemies");var target=pool[19];Assert.That((bool)Call(game,"Spawn",target,5));target.go.transform.position=new Vector3(0,1.25f,.68f);target.hp=1000;
  var pose=new SurvivorWeaponTracking();var origin=new Vector3(0,1.37f,0);var a=Quaternion.Euler(0,-45,0);var b=Quaternion.Euler(0,45,0);
  pose.Sample(Sample(origin,a),a,0);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(1000),"Holding a still blade does no swing damage");
  pose.Sample(Sample(origin,b),b,.02f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(945),"Rotation alone sweeps through a target missed by both endpoint rays");
  pose.Sample(Sample(origin,a),a,.04f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(945),"An overlapping reverse sweep cannot repeatedly hit the same enemy");
  Set(game,"meleeClock",.4f);pose.Sample(Sample(origin,b),b,.06f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(890));
  // Lost samples may keep the mesh visible, but cannot strike from the old pose.
  pose.Sample(default,Quaternion.identity,.08f);Call(game,"ProcessHandPose",pose,.02f);Set(game,"meleeClock",.8f);pose.Sample(Sample(origin,a,true),a,.1f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(890),"Recovery while trigger is held must rearm first");
  pose.Sample(Sample(origin,a),a,.12f);Call(game,"ProcessHandPose",pose,.02f);
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=RoomNavigation.Layer;wall.transform.position=new Vector3(0,1.37f,.38f);wall.transform.localScale=new Vector3(1.5f,.6f,.1f);Physics.SyncTransforms();
  pose.Sample(Sample(origin,b),b,.14f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(890),"Scanned furniture blocks the swept blade");Object.DestroyImmediate(wall);
  target.hp=34;pose.Sample(Sample(origin,a),a,.16f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.active,Is.False);Assert.That(game.player.kills,Is.EqualTo(1));
  // A restored/teleported pose must not form a long damage sweep through the room.
  Assert.That((bool)Call(game,"Spawn",target,5));target.go.transform.position=new Vector3(0,1.25f,.68f);target.hp=1000;Call(game,"ResetHandCombat",true);pose.Reset();
  pose.Sample(Sample(origin+Vector3.left,a),a,1);Call(game,"ProcessHandPose",pose,.02f);pose.Sample(Sample(origin+Vector3.right,b),b,1.02f);Call(game,"ProcessHandPose",pose,.02f);Assert.That(target.hp,Is.EqualTo(1000));
  game.Menu();yield return new ExitPlayMode();
 }
 [UnityTest]public IEnumerator GunsKillGroundAndAirTargetsKeepRewardsAndShowRealModels(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindObjectOfType<SurvivorGame>();game.enabled=false;var ui=game.GetComponent<SurvivorUI>();ui.enabled=false;
  string dir=Path.Combine(Path.GetTempPath(),"SurvivorGuns",Guid.NewGuid().ToString("N"));Set(game,"progressStore",new SurvivorProgressStore(Path.Combine(dir,"save.json")));
  var pool=(SurvivorGame.Enemy[])Get(game,"enemies");
  foreach(int gun in new[]{6,7})foreach(int type in new[]{0,5}){
   game.meta.equippedWeapon=gun;game.StartRun();game.player.damage=20;game.player.critChance=0;Array.Clear(game.player.stacks,0,game.player.stacks.Length);
   var target=pool[type==0?0:19];Assert.That((bool)Call(game,"Spawn",target,type));float y=type==0?.2f:1.37f;target.go.transform.position=new Vector3(0,y-(type==0?.2f:.12f),1.2f);target.hp=gun==6?32:14;
   var ray=new Ray(new Vector3(0,y,0),Vector3.forward);Assert.That(game.FireHandWeapon(ray));Assert.That(game.FireHandWeapon(ray),Is.False);
   for(int f=0;f<20;f++)Call(game,"HandShotStep",1/90f);Assert.That(target.active,Is.False);Assert.That(game.player.kills,Is.EqualTo(1));
   Call(game,"Finish",false);Assert.That(game.lastCoinReward,Is.EqualTo(3));
  }
  foreach(int gun in new[]{6,7}){game.screen=ScreenState.Shop;Set(ui,"libraryIndex",gun);Call(ui,"Build");Call(game,"Visuals");foreach(var enemy in pool)if(enemy.visual)Assert.That(enemy.visual.activeSelf,Is.False,"Combat models must not cover the weapon shop");Assert.That((GameObject)Get(ui,"previewModel"),Is.Not.Null);Capture(game,gun==6?"physical-pistol-shop":"physical-shotgun-shop");}
  game.meta.equippedWeapon=6;game.StartRun();Call(game,"UpdateHandVisual");var root=(Transform)Get(game,"handRoot");Assert.That(root.gameObject.activeSelf);Assert.That(root.GetChild(6).gameObject.activeSelf);var panel=(Canvas)Get(ui,"panel");panel.gameObject.SetActive(false);Capture(game,"physical-pistol-hand");
  // A shot spawned past a nearby wall must never damage the enemy behind it.
  var e=pool[19];Assert.That((bool)Call(game,"Spawn",e,5));e.go.transform.position=new Vector3(0,1.25f,1);e.hp=1000;
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=RoomNavigation.Layer;wall.transform.position=new Vector3(0,1.37f,.18f);wall.transform.localScale=new Vector3(1,.6f,.06f);Physics.SyncTransforms();
  Assert.That(game.FireHandWeapon(new Ray(new Vector3(0,1.37f,0),Vector3.forward)));for(int f=0;f<30;f++)Call(game,"HandShotStep",1/90f);Assert.That(e.hp,Is.EqualTo(1000));Object.DestroyImmediate(wall);
  game.Pause();Assert.That(game.FireHandWeapon(new Ray(Vector3.zero,Vector3.forward)),Is.False);game.Menu();Object.DestroyImmediate(game.gameObject);Directory.Delete(dir,true);yield return new ExitPlayMode();
 }
 static object Get(object t,string f)=>t.GetType().GetField(f,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(t);
 static void Set(object t,string f,object v)=>t.GetType().GetField(f,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(t,v);
 static object Call(object t,string m,params object[] a)=>t.GetType().GetMethod(m,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(t,a);
 static void Capture(SurvivorGame game,string name){var c=game.platform.Head;var old=c.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1400,1000,24);var tex=new Texture2D(1400,1000,TextureFormat.RGB24,false);try{Canvas.ForceUpdateCanvases();c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1400,1000),0,0);tex.Apply();Directory.CreateDirectory("Docs/Validation");File.WriteAllBytes("Docs/Validation/"+name+".png",tex.EncodeToPNG());}finally{c.targetTexture=old;RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(tex);}}
}
}
