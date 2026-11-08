using UnityEngine;
namespace SpatialSurvivor.Editor {
public static partial class SurvivorArtBuilder {
 static void CanonicalBlade(GameObject prefab,Mesh mesh,bool hand){
  // The shape builder works in an upright drawing plane. Export sword assets
  // with a +Z tip and a vertical broad face so controllers need no 90° offset.
  var rotation=Quaternion.AngleAxis(90,Vector3.forward)*Quaternion.FromToRotation(Vector3.up,Vector3.forward);
  var vertices=mesh.vertices;var normals=mesh.normals;
  for(int i=0;i<vertices.Length;i++){vertices[i]=rotation*vertices[i];normals[i]=rotation*normals[i];}
  mesh.vertices=vertices;mesh.normals=normals;mesh.RecalculateBounds();
  Bone("GripCenter",prefab.transform,Vector3.zero);
  Bone("BladeGuard",prefab.transform,new Vector3(0,0,hand?.12f:0));
  Bone("BladeTip",prefab.transform,new Vector3(0,0,hand?.83f:.63f));
 }
}
}
