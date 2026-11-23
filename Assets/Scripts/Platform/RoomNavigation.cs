using System;
using System.Collections.Generic;
using UnityEngine;
namespace SpatialSurvivor {
/// <summary>Cached room raster. Only scanned floor is traversable on device; footprint clearance prevents furniture crossings.</summary>
public sealed class RoomNavigation {
 public const int Size=112; public const float Cell=.25f; public const int Layer=8;
 public bool Ready{get;private set;} public float Floor{get;private set;} public float Ceiling{get;private set;}=2.5f; public int Revision{get;private set;} public int Count{get;private set;}
 readonly bool[] open=new bool[Size*Size]; readonly float[] heights=new float[Size*Size]; readonly int[] parent=new int[Size*Size],queue=new int[Size*Size]; readonly bool[] visited=new bool[Size*Size],reachable=new bool[Size*Size]; readonly List<int> boundary=new List<int>(); Vector3 origin;
 public void Rebuild(Vector3 player){origin=new Vector3(Mathf.Floor(player.x/Cell)*Cell-Size*Cell/2,0,Mathf.Floor(player.z/Cell)*Cell-Size*Cell/2);Count=0;boundary.Clear();Floor=0;var floors=new List<float>();
 for(int z=0;z<Size;z++)for(int x=0;x<Size;x++){int i=z*Size+x;Vector3 p=Point(i);var hits=Physics.RaycastAll(new Vector3(p.x,player.y+.15f,p.z),Vector3.down,3.5f,1<<Layer,QueryTriggerInteraction.Ignore);float best=float.NegativeInfinity;foreach(var h in hits)if(h.normal.y>.65f&&h.point.y<player.y-.6f&&h.point.y>best)best=h.point.y;open[i]=!float.IsNegativeInfinity(best);heights[i]=best;if(open[i]){p.y=best;open[i]=!Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.05f,.05f,1<<Layer,QueryTriggerInteraction.Ignore);if(open[i])floors.Add(best);}}
 if(floors.Count>0){floors.Sort();Floor=floors[floors.Count/2];}for(int i=0;i<open.Length;i++)if(open[i]&&Mathf.Abs(heights[i]-Floor)>.22f)open[i]=false;
 // Retain all measured floor for existing pursuers; only spawning uses the player component.
 Array.Clear(reachable,0,reachable.Length);int start=Nearest(player);if(start>=0){Array.Clear(visited,0,visited.Length);int head=0,tail=0;queue[tail++]=start;visited[start]=true;while(head<tail){int a=queue[head++];foreach(int b in Neighbors(a))if(open[b]&&!visited[b]&&Mathf.Abs(heights[b]-heights[a])<.2f){visited[b]=true;queue[tail++]=b;}}Array.Copy(visited,reachable,visited.Length);}
 for(int i=0;i<open.Length;i++)if(open[i]){if(reachable[i])Count++;int x=i%Size,z=i/Size;if(x==0||z==0||x==Size-1||z==Size-1||!open[i-1]||!open[i+1]||!open[i-Size]||!open[i+Size])boundary.Add(i);}Ready=Count>=12;Revision++;}
 IEnumerable<int> Neighbors(int i){int x=i%Size,z=i/Size;if(x>0)yield return i-1;if(x<Size-1)yield return i+1;if(z>0)yield return i-Size;if(z<Size-1)yield return i+Size;}
 int Index(Vector3 p){int x=Mathf.FloorToInt((p.x-origin.x)/Cell),z=Mathf.FloorToInt((p.z-origin.z)/Cell);return x<0||z<0||x>=Size||z>=Size?-1:z*Size+x;}
 Vector3 Point(int i)=>new Vector3(origin.x+(i%Size+.5f)*Cell,0,origin.z+(i/Size+.5f)*Cell);
 int Nearest(Vector3 p){int best=-1;float d=float.MaxValue;for(int i=0;i<open.Length;i++)if(open[i]){float v=Horizontal(Point(i)-p).sqrMagnitude;if(v<d){d=v;best=i;}}return best;}
 public bool Contains(Vector3 p,float radius=.2f){int i=Index(p);if(i<0||!open[i])return false;p.y=heights[i];return !Physics.CheckCapsule(p+Vector3.up*(radius+.05f),p+Vector3.up*(radius+.55f),radius,1<<Layer,QueryTriggerInteraction.Ignore);}
 public bool ContainsAir(Vector3 p,float radius){if(!Ready||Index(p)<0||p.y<Floor+.4f||p.y>Floor+2.3f||Physics.CheckSphere(p,radius,1<<Layer,QueryTriggerInteraction.Ignore))return false;return Physics.Raycast(p,Vector3.down,out var floor,4,1<<Layer,QueryTriggerInteraction.Ignore)&&floor.normal.y>.5f;}
 public bool Spawn(Vector3 player,float minimum,float radius,System.Random rng,out Vector3 point,out float ceiling,bool drop=false,Func<Vector3,bool> accept=null){point=default;ceiling=0;if(!Ready)return false;int offset=boundary.Count>0?rng.Next(boundary.Count):0;for(int k=0;k<boundary.Count;k++){int i=boundary[(offset+k)%boundary.Count];if(!reachable[i])continue;var edge=Point(i);var p=edge;p.y=heights[i];if(!Contains(p,radius)){bool found=false;var inward=Horizontal(player-edge).normalized;for(int step=1;step<=Mathf.CeilToInt(radius/Cell)+2;step++){var candidate=edge+inward*(step*Cell);int cell=Index(candidate);if(cell>=0&&open[cell]&&Contains(candidate,radius)){p=candidate;p.y=heights[cell];found=true;break;}}if(!found)continue;}if(Horizontal(p-player).sqrMagnitude<minimum*minimum)continue;if(drop){if(!Physics.Raycast(p+Vector3.up*.4f,Vector3.up,out var hit,4,1<<Layer)||hit.normal.y>-.5f)continue;ceiling=hit.point.y;}if(accept!=null&&!accept(p))continue;point=p;return true;}return false;}
 // A target at a wall or scan edge can be invalid for a particular creature's
 // footprint. Search its reachable floor, rather than rejecting the entire chase.
 public Vector3 Waypoint(Vector3 from,Vector3 to,float radius){
  int a=Index(from),target=Index(to);if(a<0||!open[a])return from;
  bool validTarget=Contains(to,radius);
  if(validTarget&&ClearRoute(from,to,radius))return new Vector3(to.x,heights[Index(to)],to.z);
  Array.Clear(visited,0,visited.Length);int head=0,tail=0,best=a;
  float nearest=float.PositiveInfinity;
  queue[tail++]=a;visited[a]=true;parent[a]=-1;
  while(head<tail){
   int i=queue[head++];var point=Point(i);point.y=heights[i];
   if(Contains(point,radius)){
    float distance=Horizontal(point-to).sqrMagnitude;
    if(distance<nearest){nearest=distance;best=i;}
    if(validTarget&&i==target){best=i;break;}
   }
   foreach(int n in Neighbors(i))if(open[n]&&!visited[n]&&Mathf.Abs(heights[n]-heights[i])<.2f){
    var next=Point(n);next.y=heights[n];
    if(!Contains(next,radius)||!ClearRoute(point,next,radius))continue;
    visited[n]=true;parent[n]=i;queue[tail++]=n;
   }
  }
  if(best==a){var point=Point(a);point.y=heights[a];return Contains(point,radius)&&ClearRoute(from,point,radius)?point:from;}
  int step=best;while(parent[step]!=a&&parent[step]>=0)step=parent[step];
  var waypoint=Point(step);waypoint.y=heights[step];return waypoint;
 }
 bool ClearRoute(Vector3 from,Vector3 to,float radius){
  var direction=Horizontal(to-from);float distance=direction.magnitude;
  if(distance<.0001f)return true;
  if(Physics.CapsuleCast(from+Vector3.up*(radius+.05f),from+Vector3.up*(radius+.55f),radius,direction/distance,out _,distance,1<<Layer,QueryTriggerInteraction.Ignore))return false;
  int steps=Mathf.CeilToInt(distance/(Cell*.5f));
  for(int i=1;i<=steps;i++)if(!Contains(Vector3.Lerp(from,to,(float)i/steps),radius))return false;
  return true;
 }
 public static Vector3 Horizontal(Vector3 p)=>new Vector3(p.x,0,p.z);
}
}
