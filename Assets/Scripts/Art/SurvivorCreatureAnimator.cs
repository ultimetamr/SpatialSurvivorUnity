using UnityEngine;

namespace SpatialSurvivor {
public enum CreatureMotion { Idle, Walk, Attack, Hit, Spawn, Death }

// Clips animate the actual joint hierarchy. The combat clock owns their time,
// including pause, hit stop, tracking loss and pooled respawns.
public sealed class SurvivorCreatureAnimator:MonoBehaviour {
 public AnimationClip idle,walk,attack,hit,spawn,death;
 public CreatureMotion Motion {get;private set;}
 public float MotionTime {get;private set;}
 public int Samples {get;private set;}
 bool dead;float actionRemaining,movementGrace;Vector3 previousPosition;bool positionKnown;
 public void Respawn(){dead=false;positionKnown=false;movementGrace=0;Play(CreatureMotion.Spawn);}
 public void Attack(){if(!dead&&Motion!=CreatureMotion.Spawn)Play(CreatureMotion.Attack);}
 public void Hit(){if(!dead&&Motion!=CreatureMotion.Spawn)Play(CreatureMotion.Hit);}
 public void Die(){if(dead)return;dead=true;Play(CreatureMotion.Death);}
 AnimationClip Clip(CreatureMotion motion)=>motion==CreatureMotion.Walk?walk:motion==CreatureMotion.Attack?attack:motion==CreatureMotion.Hit?hit:motion==CreatureMotion.Spawn?spawn:motion==CreatureMotion.Death?death:idle;
 void Play(CreatureMotion motion){Motion=motion;MotionTime=0;var clip=Clip(motion);actionRemaining=clip?clip.length:0;Sample();}
 void Sample(){var clip=Clip(Motion);if(clip){clip.SampleAnimation(gameObject,Mathf.Min(MotionTime,clip.length));Samples++;}}
 public void Tick(float dt,bool frozen=false){
  if(dt<=0)return;var p=transform.parent?transform.parent.position:transform.position;
  bool displaced=positionKnown&&(p-previousPosition).sqrMagnitude>.00000005f;previousPosition=p;positionKnown=true;
  movementGrace=displaced?.14f:Mathf.Max(0,movementGrace-dt);bool moving=movementGrace>0;
  if(frozen&&!dead)return;
  bool action=Motion==CreatureMotion.Spawn||Motion==CreatureMotion.Attack||Motion==CreatureMotion.Hit||Motion==CreatureMotion.Death;
  if(action&&!dead){actionRemaining-=dt;if(actionRemaining<=0){Play(moving?CreatureMotion.Walk:CreatureMotion.Idle);action=false;}}
  if(!action){var desired=moving?CreatureMotion.Walk:CreatureMotion.Idle;if(Motion!=desired)Play(desired);}
  MotionTime+=dt;var clip=Clip(Motion);if(!action&&clip&&clip.length>0)MotionTime%=clip.length;Sample();
 }
}
}
