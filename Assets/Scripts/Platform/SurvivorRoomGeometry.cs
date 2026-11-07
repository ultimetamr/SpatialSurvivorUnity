using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpatialSurvivor {
// Owns runtime meshes and colliders. Poses are in tracking space, under XR Origin.
public sealed class SurvivorRoomGeometry : IDisposable {
 readonly Transform root;
 readonly Material material;
 readonly Dictionary<Guid,GameObject> chunks=new Dictionary<Guid,GameObject>();
 bool preview;
 public int Count=>chunks.Count;
 public bool Contains(Guid id)=>chunks.ContainsKey(id);
 public int Triangles {get;private set;}
 public SurvivorRoomGeometry(Transform parent,string name,Material sharedMaterial) {
  root=new GameObject(name).transform;root.SetParent(parent,false);material=sharedMaterial;
 }
 public void SetActive(bool active)=>root.gameObject.SetActive(active);
 public void SetPreview(bool visible){preview=visible;foreach(var go in chunks.Values)go.GetComponent<MeshRenderer>().enabled=visible;}
 public bool Upsert(Guid id,Vector3 position,Quaternion rotation,Vector3[] vertices,int[] indices) {
  if(vertices==null||indices==null||vertices.Length<3||indices.Length<3||indices.Length%3!=0)return false;
  foreach(var p in vertices)if(!Finite(p.x)||!Finite(p.y)||!Finite(p.z))return false;
  foreach(int index in indices)if(index<0||index>=vertices.Length)return false;
  if(!chunks.TryGetValue(id,out var go)) {
   go=new GameObject("Room surface "+id,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
   go.layer=RoomNavigation.Layer;go.transform.SetParent(root,false);
   go.GetComponent<MeshRenderer>().sharedMaterial=material;go.GetComponent<MeshRenderer>().enabled=preview;chunks.Add(id,go);
  }
  var filter=go.GetComponent<MeshFilter>();var mesh=filter.sharedMesh;
  if(!mesh){mesh=new Mesh {indexFormat=IndexFormat.UInt32,name="Scanned room surface"};filter.sharedMesh=mesh;}
  else {Triangles-=mesh.triangles.Length/3;go.GetComponent<MeshCollider>().sharedMesh=null;mesh.Clear();}
  mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
  go.transform.SetLocalPositionAndRotation(position,rotation);go.GetComponent<MeshCollider>().sharedMesh=mesh;
  Triangles+=indices.Length/3;return true;
 }
 public void Remove(Guid id) {
  if(!chunks.TryGetValue(id,out var go))return;
  var mesh=go.GetComponent<MeshFilter>().sharedMesh;Triangles-=mesh.triangles.Length/3;
  go.SetActive(false);Release(mesh);Release(go);chunks.Remove(id);
 }
 public void Clear(){foreach(var id in new List<Guid>(chunks.Keys))Remove(id);}
 public void Dispose(){Clear();if(root)Release(root.gameObject);}
 static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
 static void Release(UnityEngine.Object item){if(Application.isPlaying)UnityEngine.Object.Destroy(item);else UnityEngine.Object.DestroyImmediate(item);}

 // Ear clipping preserves concave scanned floor boundaries; a triangle fan would fill furniture recesses.
 public static int[] Triangulate(Vector2[] polygon) {
  if(polygon==null||polygon.Length<3)return Array.Empty<int>();
  var remaining=new List<int>();float area=0;
  for(int i=0;i<polygon.Length;i++)if(remaining.Count==0||(polygon[i]-polygon[remaining[remaining.Count-1]]).sqrMagnitude>.00000001f)remaining.Add(i);
  if(remaining.Count>1&&(polygon[remaining[0]]-polygon[remaining[remaining.Count-1]]).sqrMagnitude<.00000001f)remaining.RemoveAt(remaining.Count-1);
  bool simplified=true;
  while(simplified&&remaining.Count>3){simplified=false;for(int j=0;j<remaining.Count;j++){var a=polygon[remaining[(j+remaining.Count-1)%remaining.Count]];var b=polygon[remaining[j]];var c=polygon[remaining[(j+1)%remaining.Count]];if(Mathf.Abs(Cross(b-a,c-b))<.000001f){remaining.RemoveAt(j);simplified=true;break;}}}
  if(remaining.Count<3)return Array.Empty<int>();
  for(int i=0;i<remaining.Count;i++){var a=polygon[remaining[i]];var b=polygon[remaining[(i+1)%remaining.Count]];area+=a.x*b.y-b.x*a.y;}
  if(Mathf.Abs(area)<.00001f)return Array.Empty<int>();
  float sign=Mathf.Sign(area);var result=new List<int>();int guard=polygon.Length*polygon.Length;
  while(remaining.Count>2&&guard-->0) {
   bool clipped=false;
   for(int j=0;j<remaining.Count;j++) {
    int a=remaining[(j+remaining.Count-1)%remaining.Count],b=remaining[j],c=remaining[(j+1)%remaining.Count];
    if(Cross(polygon[b]-polygon[a],polygon[c]-polygon[b])*sign<=.000001f)continue;
    bool occupied=false;
    foreach(int k in remaining)if(k!=a&&k!=b&&k!=c&&Inside(polygon[k],polygon[a],polygon[b],polygon[c],sign)){occupied=true;break;}
    if(occupied)continue;
    // Both sides are collidable, including floor and ceiling planes regardless of SDK winding.
    result.AddRange(new[]{a,b,c,c,b,a});remaining.RemoveAt(j);clipped=true;break;
   }
   if(!clipped)return Array.Empty<int>();
  }
  return result.ToArray();
 }
 static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
 static bool Inside(Vector2 p,Vector2 a,Vector2 b,Vector2 c,float sign)=>Cross(b-a,p-a)*sign>=0&&Cross(c-b,p-b)*sign>=0&&Cross(a-c,p-c)*sign>=0;
}
}
