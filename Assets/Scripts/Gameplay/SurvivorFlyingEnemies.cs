using UnityEngine;
namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
 bool AirClear(Vector3 p,Enemy self,float radius){if(!platform.Navigation.ContainsAir(p,radius))return false;foreach(var other in enemies)if(other!=self&&(other.active||other.deathRemaining>0)&&Vector3.Distance(p,other.go.transform.position)<radius+other.radius+.07f)return false;return true;}
 bool SpawnFlyer(Enemy e,int type,float radius,float speed,float damage,int hp,int exp){float height=Mathf.Clamp(platform.Head.transform.position.y-platform.Navigation.Floor-.35f,.8f,1.7f);if(!platform.Navigation.Spawn(PlayerPosition,2,radius,rng,out var floor,out _,false,p=>AirClear(p+Vector3.up*height,e,radius)))return false;int wave=CombatWave;e.type=type;e.radius=radius;e.speed=speed*Mathf.Pow(1.1f,wave-1)*enemySpeedBonus;e.damage=damage*enemyDamageBonus;e.hp=e.maxHp=Mathf.CeilToInt(hp*Mathf.Pow(1.2f,wave-1));e.exp=exp;e.generation++;e.active=true;e.falling=false;e.freeze=freezePulseRemaining;e.deathRemaining=e.vulnerable=e.burnTick=e.attack=e.repath=e.sword=e.poison=e.slow=e.burn=e.telegraph=0;e.y=floor.y+height;e.go.transform.position=floor+Vector3.up*height;e.go.SetActive(true);SetVisual(e);PrepareEnemy(e);e.animator.Respawn();return true;}
 // If another creature enters an existing footprint, allow only separation.
 bool AirStepClear(Vector3 from,Vector3 next,Enemy self){
  if(!platform.Navigation.ContainsAir(next,self.radius))return false;
  foreach(var other in enemies)if(other!=self&&(other.active||other.deathRemaining>0)){
   float gap=self.radius+other.radius+.07f;
   if(Vector3.Distance(next,other.go.transform.position)<gap&&Vector3.Distance(next,other.go.transform.position)<=Vector3.Distance(from,other.go.transform.position)+.00001f)return false;
  }
  return true;
 }
 void FlyerStep(Enemy e,float dt){var from=e.go.transform.position;var target=HostileTarget;target.y=Mathf.Clamp(target.y+Mathf.Sin(player.elapsed*2+System.Array.IndexOf(enemies,e))*.12f,platform.Navigation.Floor+.75f,platform.Navigation.Floor+1.8f);var offset=RoomNavigation.Horizontal(from-target);var radial=offset.sqrMagnitude>.001f?offset.normalized:Vector3.forward;var goal=target;if(offset.magnitude<1.4f)goal+=radial*.4f+Vector3.Cross(Vector3.up,radial)*.3f;var direction=(goal-from).normalized;var steering=Vector3.zero;foreach(var other in enemies)if(other!=e&&other.active){var away=from-other.go.transform.position;float distance=away.magnitude;float range=e.radius+other.radius+.3f;if(distance<range)steering+=(distance>.001f?away/distance:Vector3.right)*(1-distance/range);}var heading=(direction+steering*1.5f).normalized;float step=e.speed*(e.slow>0?.7f:1)*dt;int count=Mathf.Max(1,Mathf.CeilToInt(step/.05f));for(int s=0;s<count;s++){var best=from;float score=-100;for(int n=0;n<13;n++){float angle=n==0?0:((n+1)/2)*30*(n%2==1?1:-1);var move=Quaternion.AngleAxis(angle,Vector3.up)*heading*(step/count);var next=from+move;if(!AirStepClear(from,next,e))continue;float value=Vector3.Dot(move.normalized,direction)-Mathf.Abs(angle)*.002f;if(value>score){best=next;score=value;}}// A yaw-only search cannot climb away from a desk or a blocked descent.
 if((best-from).sqrMagnitude<.00000001f){
  for(int n=0;n<10;n++){
   Vector3 escape=n==0?Vector3.up:n==1?Vector3.down:Quaternion.Euler(0,(n-2)*45,0)*Vector3.forward;
   var next=from+escape*(step/count);if(!AirStepClear(from,next,e))continue;
   float value=Vector3.Dot(escape,direction);if(value>score){best=next;score=value;}
  }
 }
 from=best;}e.go.transform.position=from;e.y=from.y;var face=target-from;if(face.sqrMagnitude>.001f)e.go.transform.rotation=Quaternion.LookRotation(face);if(Vector3.Distance(EnemyCenter(e),target)<e.radius+.5f&&e.attack<=0){e.attack=.9f;e.animator.Attack();HitObjective(e.damage);}}
}
}
