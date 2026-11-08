using NUnit.Framework;
using UnityEngine;

namespace SpatialSurvivor.Tests {
public class SurvivorMotionPolishTests {
 static GameObject Create(string name)=>Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+name));
 [TestCase("Mossling")][TestCase("EmberImp")][TestCase("CrystalGuardian")][TestCase("MoonWisp")][TestCase("ElderBloom")][TestCase("SkyMoth")][TestCase("StormDrake")][TestCase("Sporecap")][TestCase("RuneShaman")][TestCase("IronBeetle")][TestCase("FrostWitch")][TestCase("ThornSentinel")][TestCase("MoonRegent")]
 public void SkinWeightsAndLoopSeamsRemainValid(string name){
  var go=Create(name);try{
   var skin=go.GetComponent<SkinnedMeshRenderer>();var mesh=skin.sharedMesh;
   Assert.That(mesh.triangles.Length/3,Is.LessThan(24000),"Single creature triangle budget");
   int blended=0;foreach(var weight in mesh.boneWeights){Assert.That(weight.weight0+weight.weight1+weight.weight2+weight.weight3,Is.EqualTo(1).Within(.0001f));Assert.That(weight.boneIndex0,Is.InRange(0,skin.bones.Length-1));if(weight.weight1>0){blended++;Assert.That(weight.boneIndex1,Is.InRange(0,skin.bones.Length-1));}}
   Assert.That(blended,Is.GreaterThan(10),"Elbow and knee joins need blended skinning");
   var a=go.GetComponent<SurvivorCreatureAnimator>();
   foreach(var clip in new[]{a.idle,a.walk}){
    clip.SampleAnimation(go,0);var positions=new Vector3[skin.bones.Length];var rotations=new Quaternion[skin.bones.Length];var scales=new Vector3[skin.bones.Length];
    for(int i=0;i<skin.bones.Length;i++){positions[i]=skin.bones[i].localPosition;rotations[i]=skin.bones[i].localRotation;scales[i]=skin.bones[i].localScale;}
    clip.SampleAnimation(go,clip.length);
    for(int i=0;i<skin.bones.Length;i++){Assert.That(Vector3.Distance(positions[i],skin.bones[i].localPosition),Is.LessThan(.0001f),clip.name+" position seam "+i);Assert.That(Quaternion.Angle(rotations[i],skin.bones[i].localRotation),Is.LessThan(.08f),clip.name+" rotation seam "+i);Assert.That(Vector3.Distance(scales[i],skin.bones[i].localScale),Is.LessThan(.0001f),clip.name+" scale seam "+i);}
   }
   // Check authored attack poses stay inside culling bounds, including staff tips.
   var baked=new Mesh();try{for(int i=0;i<=8;i++){
    a.attack.SampleAnimation(go,a.attack.length*i/8f);skin.BakeMesh(baked);foreach(var v in baked.vertices)Assert.That(skin.localBounds.Contains(v),Is.True,name+" animated skin escapes bounds");
   }}finally{Object.DestroyImmediate(baked);}
  }finally{Object.DestroyImmediate(go);}
 }
 [Test] public void TimedAttackKeepsItsWindupThroughHitsAndRepeatedCalls(){
  var go=Create("ThornSentinel");var reference=Create("ThornSentinel");try{
   var a=go.GetComponent<SurvivorCreatureAnimator>();a.Respawn();a.Tick(.65f);a.Attack(1.5f);a.Tick(.6f);a.Hit();a.Attack();a.Attack(1.5f);
   Assert.That(a.Motion,Is.EqualTo(CreatureMotion.Attack));Assert.That(a.MotionTime,Is.EqualTo(.6f).Within(.001f));
   a.Tick(.9f);Assert.That(a.Motion,Is.EqualTo(CreatureMotion.Attack));
   var refAnimator=reference.GetComponent<SurvivorCreatureAnimator>();refAnimator.attack.SampleAnimation(reference,refAnimator.attack.length*SurvivorCreatureAnimator.ImpactNormalized);
   Assert.That(Quaternion.Angle(go.transform.Find("Rig/Body/ArmR").localRotation,reference.transform.Find("Rig/Body/ArmR").localRotation),Is.LessThan(.1f),"Pose reaches impact when the gameplay telegraph expires");
   a.Tick(.3f);Assert.That(a.Motion,Is.EqualTo(CreatureMotion.Idle));
  }finally{Object.DestroyImmediate(go);Object.DestroyImmediate(reference);}
 }
 [Test] public void TimedAttackReplacesAnOrdinaryContactAttack(){var go=Create("ElderBloom");try{var a=go.GetComponent<SurvivorCreatureAnimator>();a.Respawn();a.Tick(.65f);a.Attack();a.Tick(.1f);a.Attack(1.35f);Assert.That(a.AttackWindup,Is.EqualTo(1.35f));Assert.That(a.MotionTime,Is.Zero);a.Tick(.7f);Assert.That(a.Motion,Is.EqualTo(CreatureMotion.Attack));}finally{Object.DestroyImmediate(go);}}
 [Test] public void FreezeStopsPoseAndBlendAndDeathStillFinishes(){var go=Create("StormDrake");try{
  var a=go.GetComponent<SurvivorCreatureAnimator>();a.Respawn();a.Tick(.65f);a.Attack();a.Tick(.03f);var wing=go.transform.Find("Rig/Body/WingL");var rotation=wing.localRotation;int samples=a.Samples;
  a.Tick(.4f,true);Assert.That(a.Samples,Is.EqualTo(samples));Assert.That(Quaternion.Angle(rotation,wing.localRotation),Is.LessThan(.01f));
  a.Die();a.Tick(.45f,true);Assert.That(go.transform.Find("Rig").localScale.x,Is.LessThan(.1f));a.Respawn();a.Tick(.65f);Assert.That(go.transform.Find("Rig").localScale.x,Is.EqualTo(1).Within(.001f));
 }finally{Object.DestroyImmediate(go);}}
 [Test] public void StateChangeStartsFromCurrentPoseInsteadOfSnapping(){var go=Create("RuneShaman");try{
  var a=go.GetComponent<SurvivorCreatureAnimator>();a.Respawn();a.Tick(.65f);var arm=go.transform.Find("Rig/Body/ArmR");var pose=arm.localRotation;a.Attack();Assert.That(Quaternion.Angle(pose,arm.localRotation),Is.LessThan(.01f));a.Tick(.1f);Assert.That(Quaternion.Angle(pose,arm.localRotation),Is.GreaterThan(5));
 }finally{Object.DestroyImmediate(go);}}
 [Test] public void IdleHasBlinkAndWalkBendsKnees(){var go=Create("Mossling");try{
  var a=go.GetComponent<SurvivorCreatureAnimator>();var lid=go.transform.Find("Rig/Body/Head/LidL");a.idle.SampleAnimation(go,0);float open=lid.localScale.y;a.idle.SampleAnimation(go,a.idle.length*.71f);Assert.That(lid.localScale.y,Is.GreaterThan(open*20));
  var knee=go.transform.Find("Rig/LegR/KneeR");a.walk.SampleAnimation(go,0);var rest=knee.localRotation;a.walk.SampleAnimation(go,a.walk.length*.25f);Assert.That(Quaternion.Angle(rest,knee.localRotation),Is.GreaterThan(35));
 }finally{Object.DestroyImmediate(go);}}
}
}
