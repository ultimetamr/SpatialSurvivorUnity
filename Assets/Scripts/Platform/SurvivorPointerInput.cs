using UnityEngine;

namespace SpatialSurvivor {
public enum SurvivorPointerSource { None, Mouse, RightController, LeftController, RightHand, LeftHand }

public struct SurvivorPointerSample {
 public SurvivorPointerSource source;
 public Ray ray;
 public bool valid,pressed;
 public SurvivorPointerSample(SurvivorPointerSource source,Ray ray,bool pressed){this.source=source;this.ray=ray;this.pressed=pressed;valid=true;}
}

public static class SurvivorPointerInput {
 // Prefer the hand/controller that is actually confirming. Keep the last source
 // while idle so the ray does not jump back to the right after a left-hand click.
 public static SurvivorPointerSample Choose(SurvivorPointerSample right,SurvivorPointerSample left,SurvivorPointerSource previous){
  if(right.valid&&right.pressed&&right.source==previous)return right;
  if(left.valid&&left.pressed)return left;
  if(right.valid&&right.pressed)return right;
  if(left.valid&&left.source==previous)return left;
  return right.valid?right:left;
 }
}

// Different press/release thresholds stop a partially held trigger chattering.
public sealed class SurvivorTriggerLatch {
 bool held;
 public void Reset(){held=false;}
 public bool Step(bool hasAxis,float axis,bool digital){
  held=hasAxis&&!float.IsNaN(axis)&&!float.IsInfinity(axis)?axis>=(held?.25f:.55f):digital;
  return held;
 }
}

// The displayed ray and UI hit test both use this one filtered pose. Fast
// intentional turns follow quickly; small aiming tremor is damped.
public sealed class SurvivorPointerRayFilter {
 bool initialized;SurvivorPointerSource source;Ray filtered;Vector3 previousDirection;
 public void Reset(){initialized=false;}
 public Ray Step(Ray raw,SurvivorPointerSource nextSource,float dt){
  if(!initialized||source!=nextSource||dt<=0||dt>.15f){initialized=true;source=nextSource;filtered=raw;previousDirection=raw.direction;return raw;}
  float speed=Vector3.Angle(previousDirection,raw.direction)/dt;
  float rate=Mathf.Lerp(24,65,Mathf.InverseLerp(15,120,speed));
  filtered=new Ray(Vector3.Lerp(filtered.origin,raw.origin,1-Mathf.Exp(-35*dt)),Vector3.Slerp(filtered.direction,raw.direction,1-Mathf.Exp(-rate*dt)).normalized);
  previousDirection=raw.direction;return filtered;
 }
}

public sealed class SurvivorPointerClick {
 bool wasPressed,releaseRequired=true;
 public void RequireRelease(){releaseRequired=true;}
 public bool Step(bool valid,SurvivorPointerSource nextSource,bool pressed){
  if(!valid){wasPressed=false;releaseRequired=true;return false;}
  // A source switch is allowed to begin a click, but never to retrigger a held
  // click. A modal opened by that click still requires a fresh release.
  bool click=pressed&&!wasPressed&&!releaseRequired;
  if(!pressed)releaseRequired=false;
  wasPressed=pressed;return click;
 }
}
}
