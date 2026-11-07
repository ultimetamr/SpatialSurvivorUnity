using UnityEngine;
namespace SpatialSurvivor {
// Heading follows only deliberate turns; looking down at the HUD must not
// push it farther down. Canvas forward points away from the viewing eye.
public sealed class SurvivorUIPlacement {
 public const float HudDistance=1.6f,HudDrop=.48f;
 public Vector3 Position {get;private set;}
 public Quaternion Rotation {get;private set;}
 public float Heading {get;private set;}
 bool initialized,turning;float outsideTime;
 public void Reset(){initialized=turning=false;outsideTime=0;}
 public static float Yaw(Transform head,float fallback=0){var flat=Vector3.ProjectOnPlane(head.forward,Vector3.up);return flat.sqrMagnitude>.04f?Mathf.Atan2(flat.x,flat.z)*Mathf.Rad2Deg:fallback;}
 public void Tick(Transform head,float dt){
  float desired=Yaw(head,Heading);bool snap=!initialized;
  if(snap){Heading=desired;initialized=true;}
  float error=Mathf.Abs(Mathf.DeltaAngle(Heading,desired));
  if(error>24)outsideTime+=dt;else outsideTime=0;
  if(outsideTime>.3f)turning=true;
  if(turning){Heading=Mathf.MoveTowardsAngle(Heading,desired,55*dt);if(Mathf.Abs(Mathf.DeltaAngle(Heading,desired))<2){Heading=desired;turning=false;outsideTime=0;}}
  Vector3 forward=Quaternion.Euler(0,Heading,0)*Vector3.forward;
  var target=head.position+forward*HudDistance-Vector3.up*HudDrop;
  if(snap||Vector3.Distance(Position,target)>2)Position=target;else Position=Vector3.Lerp(Position,target,1-Mathf.Exp(-dt*7));
  Rotation=Quaternion.LookRotation(Position-head.position,Vector3.up);
 }
}
}
