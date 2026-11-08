using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace SpatialSurvivor.Tests {
public class SurvivorBladeDirectionTests {
 [TestCase("Runeblade",.63f)][TestCase("HandRuneblade",.83f)]
 public void ExportedSwordTipUsesForwardAxisAndMatchesTheMesh(string name,float reach){
  var prefab=Resources.Load<GameObject>("Art/Weapons/"+name);var tip=prefab.transform.Find("BladeTip");var grip=prefab.transform.Find("GripCenter");
  Assert.That(tip&&grip,Is.True);var axis=(tip.localPosition-grip.localPosition).normalized;
  Assert.That(Vector3.Dot(axis,Vector3.forward),Is.GreaterThan(.9999f));
  float furthest=float.MinValue;foreach(var vertex in prefab.GetComponent<MeshFilter>().sharedMesh.vertices)furthest=Mathf.Max(furthest,vertex.z);
  Assert.That(furthest,Is.EqualTo(reach).Within(.0001f));Assert.That(tip.localPosition.z,Is.EqualTo(furthest).Within(.0001f));
  Assert.That(prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.z,Is.GreaterThan(prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.y*2));
 }
 [UnityTest] public IEnumerator HandTipFollowsTrackedPitchYawAndRollAndEndsAtDamageReach(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindFirstObjectByType<SurvivorGame>();game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;game.meta.equippedWeapon=1;game.StartRun();
  var models=(GameObject[])Get(game,"handModels");var sword=models[1].transform;
  foreach(var rotation in new[]{Quaternion.identity,Quaternion.Euler(45,80,0),Quaternion.Euler(-50,-110,90),Quaternion.Euler(15,145,-135)}){
   game.platform.Head.transform.rotation=rotation;Set(game.platform,"weaponFrame",-1);var pose=game.platform.TrackedWeaponPose;Call(game,"UpdateHandVisual");
   Assert.That(sword.gameObject.activeInHierarchy,Is.True);
   var grip=sword.Find("GripCenter").position;var tip=sword.Find("BladeTip").position;
   Assert.That(Vector3.Dot((tip-grip).normalized,pose.Ray.direction),Is.GreaterThan(.9999f),"Sword points along the tracked hit ray through wrist roll");
   Assert.That(Vector3.Distance(tip,pose.Ray.GetPoint(SurvivorArmory.Get(1).range)),Is.LessThan(.001f),"Visible tip and melee reach coincide");
   Assert.That(Mathf.Abs(Vector3.Dot(sword.up,pose.Rotation*Vector3.up)),Is.GreaterThan(.9999f),"Blade width follows controller roll");
  }
  game.Menu();yield return new ExitPlayMode();
 }
 [UnityTest] public IEnumerator BothOrbitVariantsPointAlongActualTravelAtEveryQuadrant(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindFirstObjectByType<SurvivorGame>();game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;game.StartRun();Array.Clear(game.player.stacks,0,game.player.stacks.Length);game.player.stacks[(int)UpgradeId.ORBITING_SWORD]=1;
  var swords=(GameObject[])Get(game,"swords");
  foreach(bool evolved in new[]{false,true}){
   game.player.stacks[(int)UpgradeId.MYRIAD_SWORDS]=evolved?1:0;game.skillVisuals.Refresh(game.player,game.PlayerPosition,true);
   for(int quadrant=0;quadrant<8;quadrant++){
    Set(game,"orbit",quadrant*45f);Call(game,"WeaponStep",0f);var before=swords[0].transform.position;
    Call(game,"WeaponStep",.001f);var root=swords[0].transform;var model=root.GetChild(evolved?1:0).GetChild(0);
    var tip=model.Find("BladeTip").position;var grip=model.Find("GripCenter").position;
    Assert.That(Vector3.Dot((tip-grip).normalized,(root.position-before).normalized),Is.GreaterThan(.999f),"Actual orbit travel and blade tip disagree at quadrant "+quadrant);
   }
  }
  game.Menu();yield return new ExitPlayMode();
 }
 [Test] public void ReusedSwordRainPointsDownAndTrailStaysBehindTheFallingTip(){
  var root=new GameObject("Sword rain review");try{
   var visuals=root.AddComponent<SurvivorSkillVisuals>();var pivots=new Transform[14];for(int i=0;i<pivots.Length;i++){pivots[i]=new GameObject("Orbit pivot "+i).transform;pivots[i].SetParent(root.transform,false);}visuals.Initialize(pivots);
   for(int reuse=0;reuse<3;reuse++){
    Assert.That(visuals.Emit(SkillVisual.SwordRain,Vector3.up*2,.55f,Quaternion.Euler(75,reuse*90,0)),Is.True);
    Transform rain=null;foreach(Transform child in root.transform)if(child.name=="SwordRain"&&child.gameObject.activeSelf){rain=child;break;}
    Assert.That(rain,Is.Not.Null);var model=rain.GetChild(0);var tip=model.Find("BladeTip");var grip=model.Find("GripCenter");var before=tip.position;
    Assert.That(Vector3.Dot((tip.position-grip.position).normalized,Vector3.down),Is.GreaterThan(.9999f));
    visuals.Tick(.1f);Assert.That(Vector3.Dot((tip.position-before).normalized,Vector3.down),Is.GreaterThan(.9999f));
    foreach(var line in rain.GetComponentsInChildren<LineRenderer>())Assert.That(rain.TransformPoint(line.GetPosition(line.positionCount-1)).y,Is.GreaterThan(grip.position.y),"Rain ribbon must trail upwards");
    visuals.ResetVisuals();
   }
  }finally{Object.DestroyImmediate(root);}
 }
 static object Get(object target,string field)=>target.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(target);
 static void Set(object target,string field,object value)=>target.GetType().GetField(field,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(target,value);
 static object Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
}
}
