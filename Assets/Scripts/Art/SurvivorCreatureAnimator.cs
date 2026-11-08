using UnityEngine;

namespace SpatialSurvivor {
public enum CreatureMotion { Idle, Walk, Attack, Hit, Spawn, Death }

// The combat clock owns animation time, including pause, hit stop and respawns.
// A cached pose buffer blends joints without per-frame allocations.
public sealed class SurvivorCreatureAnimator:MonoBehaviour {
 public AnimationClip idle,walk,attack,hit,spawn,death;
 public CreatureMotion Motion {get;private set;}
 public float MotionTime {get;private set;}
 public int Samples {get;private set;}
 public float AttackWindup {get;private set;}
 public const float ImpactNormalized=.52f;
 const float BlendDuration=.085f;static int spawnSequence;
 bool dead,positionKnown;float actionRemaining,movementGrace,blendTime,flinch,flinchCooldown,gaitTime,idleTime;
 Vector3 previousPosition;Transform[] joints;Vector3[] fromPosition,fromScale;Quaternion[] fromRotation;
 void Awake(){Prepare();}
 void Prepare(){
  if(joints!=null)return;
  var skin=GetComponent<SkinnedMeshRenderer>();joints=skin?skin.bones:new Transform[0];
  fromPosition=new Vector3[joints.Length];fromScale=new Vector3[joints.Length];fromRotation=new Quaternion[joints.Length];
 }
 public void Respawn(){
  Prepare();dead=false;positionKnown=false;movementGrace=0;flinch=flinchCooldown=0;AttackWindup=0;
  int phase=unchecked(++spawnSequence*97);idleTime=(phase&255)/256f*(idle?idle.length:1);gaitTime=(phase&63)/64f*(walk?walk.length:1);
  Play(CreatureMotion.Spawn,false);
 }
 public void Attack(float windup=0){
  if(dead||Motion==CreatureMotion.Spawn||(Motion==CreatureMotion.Attack&&(windup<=0||AttackWindup>0)))return;
  AttackWindup=Mathf.Max(0,windup);Play(CreatureMotion.Attack);
  if(AttackWindup>0&&attack)actionRemaining=AttackWindup+attack.length*(1-ImpactNormalized);
 }
 public void Hit(){
  if(dead||Motion==CreatureMotion.Spawn||flinchCooldown>0)return;
  flinchCooldown=.14f;
  if(Motion==CreatureMotion.Attack){flinch=.18f;return;}
  if(Motion!=CreatureMotion.Hit)Play(CreatureMotion.Hit);
 }
 public void Die(){if(dead)return;dead=true;flinch=0;Play(CreatureMotion.Death);}
 AnimationClip Clip(CreatureMotion motion)=>motion==CreatureMotion.Walk?walk:motion==CreatureMotion.Attack?attack:motion==CreatureMotion.Hit?hit:motion==CreatureMotion.Spawn?spawn:motion==CreatureMotion.Death?death:idle;
 void Play(CreatureMotion motion,bool blend=true){
  Prepare();for(int i=0;i<joints.Length;i++){fromPosition[i]=joints[i].localPosition;fromRotation[i]=joints[i].localRotation;fromScale[i]=joints[i].localScale;}
  Motion=motion;MotionTime=0;blendTime=blend?0:BlendDuration;var clip=Clip(motion);actionRemaining=clip?clip.length:0;Sample();
 }
 float SampleTime(AnimationClip clip){
  if(Motion==CreatureMotion.Attack&&AttackWindup>0){float impact=clip.length*ImpactNormalized;return MotionTime<=AttackWindup?impact*MotionTime/AttackWindup:impact+MotionTime-AttackWindup;}
  if(Motion==CreatureMotion.Walk)return gaitTime%clip.length;
  if(Motion==CreatureMotion.Idle)return idleTime%clip.length;
  return MotionTime;
 }
 void Sample(){
  var clip=Clip(Motion);if(!clip)return;
  clip.SampleAnimation(gameObject,Mathf.Clamp(SampleTime(clip),0,clip.length));Samples++;
  float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01(blendTime/BlendDuration));
  if(blend<1)for(int i=0;i<joints.Length;i++){
   var joint=joints[i];joint.localPosition=Vector3.LerpUnclamped(fromPosition[i],joint.localPosition,blend);
   joint.localRotation=Quaternion.SlerpUnclamped(fromRotation[i],joint.localRotation,blend);
   joint.localScale=Vector3.LerpUnclamped(fromScale[i],joint.localScale,blend);
  }
  // Recoil never erases the timing of a telegraphed attack.
  if(flinch>0&&joints.Length>2){float recoil=Mathf.Sin(flinch/.18f*Mathf.PI);joints[1].localRotation*=Quaternion.Euler(-recoil*8,0,recoil*4);joints[2].localRotation*=Quaternion.Euler(recoil*6,0,0);}
 }
 public void Tick(float dt,bool frozen=false){
  if(dt<=0)return;var p=transform.parent?transform.parent.position:transform.position;
  float distance=positionKnown?Vector3.Distance(p,previousPosition):0;previousPosition=p;positionKnown=true;
  movementGrace=distance*distance>.00000005f?.14f:Mathf.Max(0,movementGrace-dt);bool moving=movementGrace>0;
  if(frozen&&!dead)return;
  flinch=Mathf.Max(0,flinch-dt);flinchCooldown=Mathf.Max(0,flinchCooldown-dt);blendTime+=dt;
  bool action=Motion==CreatureMotion.Spawn||Motion==CreatureMotion.Attack||Motion==CreatureMotion.Hit||Motion==CreatureMotion.Death;
  if(action&&!dead){actionRemaining-=dt;if(actionRemaining<=0){Play(moving?CreatureMotion.Walk:CreatureMotion.Idle);action=false;}}
  if(!action){var desired=moving?CreatureMotion.Walk:CreatureMotion.Idle;if(Motion!=desired)Play(desired);}
  MotionTime+=dt;idleTime+=dt;
  gaitTime+=dt*Mathf.Clamp(distance/dt/.55f,.8f,1.35f);
  if(blendTime<dt)blendTime=dt;
  var clip=Clip(Motion);if(!action&&clip&&clip.length>0)MotionTime%=clip.length;Sample();
 }
}
}
