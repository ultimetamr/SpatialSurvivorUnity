using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SpatialSurvivor.Tests {
public class SurvivorPointerTests {
 static SurvivorPointerSample Sample(SurvivorPointerSource source,bool down)=>new SurvivorPointerSample(source,new Ray(Vector3.zero,Vector3.forward),down);
 [Test] public void LeftTriggerTakesOverFromIdleRightController(){var right=Sample(SurvivorPointerSource.RightController,false);var left=Sample(SurvivorPointerSource.LeftController,true);Assert.That(SurvivorPointerInput.Choose(right,left,right.source).source,Is.EqualTo(left.source));left.pressed=false;Assert.That(SurvivorPointerInput.Choose(right,left,left.source).source,Is.EqualTo(left.source));}
 [Test] public void PressedRightControllerTakesOverFromIdleLeft(){var right=Sample(SurvivorPointerSource.RightController,true);var left=Sample(SurvivorPointerSource.LeftController,false);Assert.That(SurvivorPointerInput.Choose(right,left,left.source).source,Is.EqualTo(right.source));}
 [Test] public void LeftPinchIsNotSwallowedByValidRightHand(){var right=Sample(SurvivorPointerSource.RightHand,false);var left=Sample(SurvivorPointerSource.LeftHand,true);Assert.That(SurvivorPointerInput.Choose(right,left,right.source).source,Is.EqualTo(left.source));}
 [Test] public void LostSourceFallsBackAndBothLostAreInvalid(){var left=Sample(SurvivorPointerSource.LeftController,false);Assert.That(SurvivorPointerInput.Choose(default,left,SurvivorPointerSource.RightController).source,Is.EqualTo(left.source));Assert.That(SurvivorPointerInput.Choose(default,default,left.source).valid,Is.False);}
 [Test] public void HeldTriggerCannotClickNewModalOrTrackingRecovery(){var click=new SurvivorPointerClick();Assert.That(click.Step(true,SurvivorPointerSource.RightController,true),Is.False);Assert.That(click.Step(true,SurvivorPointerSource.RightController,false),Is.False);Assert.That(click.Step(true,SurvivorPointerSource.LeftController,true),Is.True);Assert.That(click.Step(true,SurvivorPointerSource.LeftController,true),Is.False);click.RequireRelease();Assert.That(click.Step(true,SurvivorPointerSource.RightController,true),Is.False);click.Step(true,SurvivorPointerSource.RightController,false);Assert.That(click.Step(true,SurvivorPointerSource.RightController,true),Is.True);click.Step(false,SurvivorPointerSource.None,false);Assert.That(click.Step(true,SurvivorPointerSource.RightController,true),Is.False);}
 [Test] public void AnalogTriggerHasSeparatePressAndReleaseThresholds(){var trigger=new SurvivorTriggerLatch();var click=new SurvivorPointerClick();int count=0;foreach(float axis in new[]{0f,.45f,.57f,.54f,.66f,.51f,.3f,.24f,.4f,.56f,.2f})if(click.Step(true,SurvivorPointerSource.RightController,trigger.Step(true,axis,false)))count++;Assert.That(count,Is.EqualTo(2),"Partial pull jitter must not lose release or produce extra clicks");Assert.That(trigger.Step(false,0,true),Is.True,"Digital-only controllers remain supported");Assert.That(trigger.Step(false,0,false),Is.False);}
 [Test] public void RayFilterDampsTremorAndFollowsIntentionalTurns(){var filter=new SurvivorPointerRayFilter();float before=0,after=0;for(int i=0;i<90;i++){float angle=i%2==0?.5f:-.5f;var raw=new Ray(Vector3.right*(i%2==0?.004f:-.004f),Quaternion.Euler(0,angle,0)*Vector3.forward);var filtered=filter.Step(raw,SurvivorPointerSource.RightController,1/90f);if(i>20){before+=Mathf.Abs(angle);after+=Vector3.Angle(filtered.direction,Vector3.forward);Assert.That(Mathf.Abs(filtered.origin.x),Is.LessThan(.002f));}}Assert.That(after/before,Is.LessThan(.45f),"High frequency aim jitter should be reduced");var turned=new Ray(Vector3.one,Vector3.right);Ray result=default;for(int i=0;i<18;i++)result=filter.Step(turned,SurvivorPointerSource.RightController,1/90f);Assert.That(Vector3.Angle(result.direction,Vector3.right),Is.LessThan(1));Assert.That(Vector3.Distance(result.origin,Vector3.one),Is.LessThan(.005f));}
 [Test] public void RayFilterResetsWhenSourceOrTrackingChanges(){var filter=new SurvivorPointerRayFilter();filter.Step(new Ray(Vector3.zero,Vector3.forward),SurvivorPointerSource.RightController,.01f);var left=new Ray(Vector3.left,Vector3.left);Assert.That(filter.Step(left,SurvivorPointerSource.LeftController,.01f).direction,Is.EqualTo(left.direction));filter.Reset();var recovered=new Ray(Vector3.right,Vector3.back);Assert.That(filter.Step(recovered,SurvivorPointerSource.LeftController,.01f).direction,Is.EqualTo(recovered.direction));}
 [UnityTest] public IEnumerator SmallTriggerPullWobbleKeepsHoveredTargetButCannotClickFarAwayOrDisabledButtons(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindObjectOfType<SurvivorGame>();var ui=game.GetComponent<SurvivorUI>();ui.enabled=false;var buttons=(List<Button>)Field(ui,"buttons");var mr=buttons.Find(button=>button.name.StartsWith("MR 设置"));var rect=mr.GetComponent<RectTransform>();var origin=game.platform.Head.transform.position;var edge=rect.TransformPoint(new Vector3(rect.rect.xMax-2,0,0));Apply(ui,new Ray(origin,(edge-origin).normalized),false);
  // Trigger pull nudges the beam just 6 mm beyond the now highlighted button.
  var wobble=rect.TransformPoint(new Vector3(rect.rect.xMax+6,0,0));Apply(ui,new Ray(origin,(wobble-origin).normalized),true);Assert.That(game.screen,Is.EqualTo(ScreenState.MR),"A single press must survive a small edge wobble");
  Call(ui,"Build");buttons=(List<Button>)Field(ui,"buttons");var back=buttons.Find(button=>button.name=="返回");rect=back.GetComponent<RectTransform>();Apply(ui,new Ray(origin,(back.transform.position-origin).normalized),false);var far=rect.TransformPoint(new Vector3(rect.rect.xMax+70,0,0));Apply(ui,new Ray(origin,(far-origin).normalized),true);Assert.That(game.screen,Is.EqualTo(ScreenState.MR),"Hover tolerance must not click a distant previous target");
  Apply(ui,new Ray(origin,(back.transform.position-origin).normalized),false);var outside=rect.TransformPoint(new Vector3(rect.rect.xMax+6,0,0));Call(ui,"ApplyPointer",true,SurvivorPointerSource.LeftController,new Ray(origin,(outside-origin).normalized),true);Assert.That(game.screen,Is.EqualTo(ScreenState.MR),"Other controller cannot borrow the previous controller's target");
  Apply(ui,new Ray(origin,(back.transform.position-origin).normalized),false);back.interactable=false;Apply(ui,new Ray(origin,(back.transform.position-origin).normalized),true);Assert.That(game.screen,Is.EqualTo(ScreenState.MR),"Disabled hovered buttons must not accept a click");yield return new ExitPlayMode();
 }
 [UnityTest] public IEnumerator ControllerRayClicksMenuThroughNoisyRoomMeshAndRequiresRelease(){
  EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
  var game=Object.FindObjectOfType<SurvivorGame>();var ui=game.GetComponent<SurvivorUI>();ui.enabled=false;
  var panel=(Canvas)Field(ui,"panel");var buttons=(List<Button>)Field(ui,"buttons");var mr=buttons.Find(button=>button.name.StartsWith("MR 设置"));Assert.That(mr,Is.Not.Null);
  var origin=game.platform.Head.transform.position;var destination=mr.transform.position;var ray=new Ray(origin,(destination-origin).normalized);
  var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.layer=RoomNavigation.Layer;obstacle.transform.position=(origin+destination)*.5f;obstacle.transform.localScale=Vector3.one*.06f;Physics.SyncTransforms();
  Assert.That(Physics.Raycast(ray,Vector3.Distance(origin,destination),1<<RoomNavigation.Layer),Is.True);
  Apply(ui,ray,false);Apply(ui,ray,true);Assert.That(game.screen,Is.EqualTo(ScreenState.MR),"Controller ray must invoke the actual MR menu listener despite a scan triangle in front");
  Call(ui,"Build");var mrButtons=(List<Button>)Field(ui,"buttons");var back=mrButtons.Find(button=>button.name=="返回");ray=new Ray(origin,(back.transform.position-origin).normalized);
  Apply(ui,ray,true);Assert.That(game.screen,Is.EqualTo(ScreenState.MR),"Holding trigger across menu rebuild cannot click through");
  Apply(ui,ray,false);Apply(ui,ray,true);Assert.That(game.screen,Is.EqualTo(ScreenState.Menu));
  Call(ui,"Build");buttons=(List<Button>)Field(ui,"buttons");var start=buttons.Find(button=>button.name=="开始生存");start.interactable=false;ray=new Ray(origin,(start.transform.position-origin).normalized);Apply(ui,ray,false);Apply(ui,ray,true);Assert.That(game.screen,Is.EqualTo(ScreenState.Menu),"Disabled start button must remain disabled");
  Object.Destroy(obstacle);yield return new ExitPlayMode();
 }
 static void Apply(SurvivorUI ui,Ray ray,bool down)=>Call(ui,"ApplyPointer",true,SurvivorPointerSource.RightController,ray,down);
 static object Field(object target,string name)=>target.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(target);
 static object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
}
}
