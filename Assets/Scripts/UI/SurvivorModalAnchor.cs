using UnityEngine;

namespace SpatialSurvivor {
// A book stays in tracking space for its entire open session, across pages.
public sealed class SurvivorModalAnchor {
 Vector3 position;Quaternion rotation;bool placed;
 public void Close(){placed=false;}
 public void Apply(Transform panel,Transform head,Transform origin,bool recenter=false){
  if(!placed||recenter){
   var forward=RoomNavigation.Horizontal(head.forward).normalized;
   if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
   position=origin.InverseTransformPoint(head.position+forward*1.5f-Vector3.up*.08f);
   rotation=Quaternion.Inverse(origin.rotation)*Quaternion.LookRotation(forward);
   placed=true;
  }
  panel.SetPositionAndRotation(origin.TransformPoint(position),origin.rotation*rotation);
 }
}
}
