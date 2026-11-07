using UnityEngine;
namespace SpatialSurvivor {
// Brief controller dropouts retain the rendered weapon without authorizing attacks.
public sealed class SurvivorWeaponTracking {
 public const float Grace=.22f;
 public SurvivorPointerSource Source {get;private set;}
 public Ray Ray {get;private set;}
 public Quaternion Rotation {get;private set;}=Quaternion.identity;
 public bool Visible {get;private set;}
 public bool Fresh {get;private set;}
 public bool Pressed {get;private set;}
 public int Revision {get;private set;}
 float lastGood=-100;
 public void Reset(){Source=SurvivorPointerSource.None;Visible=Fresh=Pressed=false;lastGood=-100;Revision++;}
 public void Sample(SurvivorPointerSample input,Quaternion rotation,float now){
  bool valid=input.valid&&Finite(input.ray.origin)&&Finite(input.ray.direction)&&input.ray.direction.sqrMagnitude>.5f&&Finite(rotation);
  Fresh=Pressed=false;
  // A single missing right-hand frame must not move the weapon into the left hand.
  if(valid&&(input.source==Source||Source==SurvivorPointerSource.None||now-lastGood>Grace)){
   if(Source!=input.source||now-lastGood>.06f)Revision++;
   Source=input.source;Ray=new Ray(input.ray.origin,input.ray.direction.normalized);Rotation=rotation;
   Visible=Fresh=true;Pressed=input.pressed;lastGood=now;return;
  }
  Visible=Source!=SurvivorPointerSource.None&&now-lastGood<=Grace;
 }
 static bool Finite(Vector3 v)=>Finite(v.x)&&Finite(v.y)&&Finite(v.z);
 static bool Finite(Quaternion q)=>Finite(q.x)&&Finite(q.y)&&Finite(q.z)&&Finite(q.w)&&q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w>.5f;
 static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
}
}
