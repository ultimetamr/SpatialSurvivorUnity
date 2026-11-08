using UnityEngine;
namespace SpatialSurvivor {
// Small fixed crowd: navigation checks the room; these discs check each other.
// Collision footprints include the new creatures' arms and wings.
public static class SurvivorEnemyCrowd {
 static readonly float[] radii={.135f,.145f,.23f,.2f,.525f,.18f,.24f,.19f,.19f,.25f,.2f};
 public static float Radius(SurvivorGame.Enemy e)=>radii[e.type];
 public static bool Occupied(SurvivorGame.Enemy e)=>!SurvivorBestiary.Flying(e.type)&&(e.active||e.deathRemaining>0);
 public static bool SpawnClear(Vector3 p,int type,SurvivorGame.Enemy self,SurvivorGame.Enemy[] crowd){float radius=radii[type];foreach(var other in crowd){if(other==null||other==self||!Occupied(other))continue;if(RoomNavigation.Horizontal(p-other.go.transform.position).sqrMagnitude<Mathf.Pow(radius+Radius(other)+.12f,2))return false;}return true;}
 static bool Clear(Vector3 p,SurvivorGame.Enemy self,SurvivorGame.Enemy[] crowd){foreach(var other in crowd){if(other==self||!Occupied(other)||other.falling)continue;float distance=Radius(self)+Radius(other)+.035f;if(RoomNavigation.Horizontal(p-other.go.transform.position).sqrMagnitude<distance*distance)return false;}return true;}
 static Vector3 Axis(int a,int b){float angle=((a+1)*137.508f+(b+1)*71.3f)*Mathf.Deg2Rad;return new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));}
 public static Vector3 PursuitGoal(int index,SurvivorGame.Enemy[] crowd,Vector3 player,RoomNavigation nav){var e=crowd[index];var away=RoomNavigation.Horizontal(e.go.transform.position-player);float d=away.magnitude;if(d>1.8f)return player;var radial=d>.001f?away/d:Axis(index,index);float ring=e.radius+.25f;
  // Keep circling at attack range; an occupied approach is a detour, not a wait.
  var tangent=Vector3.Cross(Vector3.up,radial);var goal=player+radial*ring+tangent*.32f;goal.y=e.y;if(nav.Contains(goal,e.radius))return goal;goal=player+radial*ring-tangent*.32f;goal.y=e.y;return nav.Contains(goal,e.radius)?goal:player;
 }
 public static Vector3 Move(int index,SurvivorGame.Enemy[] crowd,Vector3 goal,float step,RoomNavigation nav){var self=crowd[index];var from=self.go.transform.position;var delta=RoomNavigation.Horizontal(goal-from);if(delta.sqrMagnitude<.000001f||step<=0)return from;step=Mathf.Min(step,delta.magnitude);var direction=delta.normalized;Vector3 steer=Vector3.zero;
  for(int j=0;j<crowd.Length;j++){var other=crowd[j];if(j==index||!Occupied(other)||other.falling)continue;var away=RoomNavigation.Horizontal(from-other.go.transform.position);float d=away.magnitude,range=Radius(self)+Radius(other)+.25f;if(d<range)steer+=(d>.001f?away/d:Axis(index,j))*(1-d/range);}
  var heading=(direction+steer*1.5f).normalized;var best=from;float score=-100;
  // Short substeps prevent late-wave fast creatures from tunnelling through peers.
  int substeps=Mathf.Max(1,Mathf.CeilToInt(step/.06f));float distance=step/substeps;
  for(int s=0;s<substeps;s++){bool found=false;float bestScore=-100;for(int n=0;n<13;n++){float angle=n==0?0:((n+1)/2)*30*(n%2==1?1:-1)*(index%2==0?1:-1);var offset=Quaternion.AngleAxis(angle,Vector3.up)*heading*distance;var p=from+offset;p.y=self.y;if(!nav.Contains(p,self.radius)||!Clear(p,self,crowd))continue;score=Vector3.Dot(offset/distance,direction)-Mathf.Abs(angle)*.002f;if(score>bestScore){bestScore=score;best=p;found=true;}}if(!found)break;from=best;}
  return from;
 }
 static bool Movable(SurvivorGame.Enemy e)=>e.active&&!e.falling&&e.freeze<=0&&e.telegraph<=0;
 static bool Push(SurvivorGame.Enemy e,Vector3 offset,RoomNavigation nav){var from=e.go.transform.position;var next=from+offset;next.y=e.y;if(nav.Contains(next,e.radius)){e.go.transform.position=next;return true;}next=from+new Vector3(offset.x,0,0);if(Mathf.Abs(offset.x)>.00001f&&nav.Contains(next,e.radius)){e.go.transform.position=next;return true;}next=from+new Vector3(0,0,offset.z);if(Mathf.Abs(offset.z)>.00001f&&nav.Contains(next,e.radius)){e.go.transform.position=next;return true;}return false;}
 public static void Resolve(SurvivorGame.Enemy[] crowd,RoomNavigation nav,float dt){if(dt<=0)return;for(int pass=0;pass<4;pass++)for(int i=0;i<crowd.Length;i++){var a=crowd[i];if(!Occupied(a)||a.falling)continue;for(int j=i+1;j<crowd.Length;j++){var b=crowd[j];if(!Occupied(b)||b.falling)continue;var delta=RoomNavigation.Horizontal(a.go.transform.position-b.go.transform.position);float d=delta.magnitude,wanted=Radius(a)+Radius(b)+.035f;if(d>=wanted)continue;bool ma=Movable(a),mb=Movable(b);if(!ma&&!mb)continue;var normal=d>.001f?delta/d:Axis(i,j);float amount=Mathf.Min(wanted-d+.001f,dt*1.8f);bool pa=ma&&Push(a,normal*amount*(mb?.5f:1),nav);bool pb=mb&&Push(b,-normal*amount*(ma?.5f:1),nav);if(ma&&!pb&&mb)Push(a,normal*amount*.5f,nav);if(mb&&!pa&&ma)Push(b,-normal*amount*.5f,nav);}}}
}
}
