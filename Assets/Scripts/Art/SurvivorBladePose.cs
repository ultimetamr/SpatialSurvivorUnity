using UnityEngine;
namespace SpatialSurvivor {
// Every sword mesh is authored grip-to-tip along +Z. Pose roots supply the
// tracked aim, orbital tangent, or falling direction without model corrections.
public static class SurvivorBladePose {
 public static readonly Quaternion RainRotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);
 public static Quaternion OrbitRotation(float angle){
  float radians=angle*Mathf.Deg2Rad;
  var tangent=new Vector3(Mathf.Sin(radians),0,-Mathf.Cos(radians));
  return Quaternion.LookRotation(tangent,Vector3.up)*Quaternion.AngleAxis(18,Vector3.forward);
 }
}
}
