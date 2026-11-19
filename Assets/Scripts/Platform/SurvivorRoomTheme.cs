using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpatialSurvivor {
public enum RoomThemeKind { Unknown,Floor,Wall,Door,Window,Table,Sofa,Chair,Cabinet,Bed,Plant }

// Semantic capture owns the measurements; this layer owns visual meshes only.
// No decorative colliders enter the navigation grid or the pointer ray.
public sealed class SurvivorRoomTheme:IDisposable {
 public const int SurfaceLimit=64;
 readonly Transform root;readonly Material material;
 readonly Dictionary<Guid,GameObject> surfaces=new Dictionary<Guid,GameObject>();
 public int Count=>surfaces.Count;
 public Transform Root=>root;
 public bool Visible {get;private set;}=true;
 static readonly Color Stone=new Color(.19f,.29f,.32f),Gold=new Color(.73f,.58f,.32f),Jade=new Color(.33f,.78f,.65f),Dark=new Color(.09f,.16f,.20f);
 public SurvivorRoomTheme(Transform origin){
  root=new GameObject("Enchanted Room - Scanned Measurements").transform;root.SetParent(origin,false);
  material=new Material(Resources.Load<Shader>("Shaders/RoomTheme")){name="Room jade and sandstone"};
 }
 public void SetVisible(bool visible){Visible=visible;root.gameObject.SetActive(visible);}
 public bool UpsertPolygon(Guid id,RoomThemeKind kind,Vector3 position,Quaternion rotation,Vector2[] polygon){
  if(kind!=RoomThemeKind.Floor&&kind!=RoomThemeKind.Wall&&kind!=RoomThemeKind.Door&&kind!=RoomThemeKind.Window)return false;
  if(!ValidPose(position,rotation)||polygon==null||polygon.Length>128)return false;
  foreach(var p in polygon)if(!Finite(p.x)||!Finite(p.y))return false;
  int[] indices=SurvivorRoomGeometry.Triangulate(polygon);if(indices.Length==0)return false;
  var b=new Builder();var baseColor=kind==RoomThemeKind.Floor?Stone:kind==RoomThemeKind.Wall?new Color(.24f,.29f,.37f):Dark;
  // Keep the captured concave outline exactly; do not fill its bounding box.
  bool opening=kind==RoomThemeKind.Door||kind==RoomThemeKind.Window;
  if(!opening)for(int i=0;i<indices.Length;i+=3)b.Triangle(V(polygon[indices[i]]),V(polygon[indices[i+1]]),V(polygon[indices[i+2]]),baseColor);
  for(int i=0;i<polygon.Length;i++)b.Strip(V(polygon[i])+Vector3.forward*.006f,V(polygon[(i+1)%polygon.Length])+Vector3.forward*.006f,.018f,Gold);
  var bounds=new Bounds(V(polygon[0]),Vector3.zero);foreach(var p in polygon)bounds.Encapsulate(V(p));
  // Every glyph vertex is tested, including its stroke width, against the scan.
  float spacing=kind==RoomThemeKind.Floor?.8f:.65f;int glyphs=0;
  for(float x=Mathf.Ceil(bounds.min.x/spacing)*spacing;!opening&&x<=bounds.max.x&&glyphs<36;x+=spacing)
   for(float y=Mathf.Ceil(bounds.min.y/spacing)*spacing;y<=bounds.max.y&&glyphs<36;y+=spacing){
    var p=new Vector2(x,y);bool fits=true;
    foreach(var d in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})fits&=Inside(p+d*.14f,polygon);
    if(!fits)continue;glyphs++;
    var c=V(p)+Vector3.forward*.01f;
    b.Diamond(c,.10f,Jade);b.Diamond(c,.065f,Gold);
   }
  return Store(id,kind,position,rotation,b);
 }
 public bool UpsertBox(Guid id,RoomThemeKind kind,Vector3 position,Quaternion rotation,Vector3 offset,Quaternion boxRotation,Vector3 extent){
  if(kind<RoomThemeKind.Table||!ValidPose(position,rotation)||!ValidPose(offset,boxRotation)||!Finite(extent.x)||!Finite(extent.y)||!Finite(extent.z)||extent.x<.08f||extent.y<.08f||extent.z<.08f||extent.x>8||extent.y>4||extent.z>8)return false;
  var b=new Builder();bool altar=kind==RoomThemeKind.Table,plant=kind==RoomThemeKind.Plant;
  // All pieces remain within the reported physical volume, including the gem.
  if(plant){
   b.Box(new Vector3(0,-.28f,0),new Vector3(.7f,.42f,.7f),Stone);
   for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;var p=new Vector3(Mathf.Cos(a)*.19f,.08f+i%2*.12f,Mathf.Sin(a)*.19f);b.Crystal(p,new Vector3(.15f,.28f,.15f),i%2==0?Jade:Gold);}
  }else if(altar){
   b.Box(new Vector3(0,-.43f,0),new Vector3(.98f,.14f,.98f),Dark);
   for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){
    b.Box(new Vector3(x*.33f,-.05f,z*.33f),new Vector3(.19f,.65f,.19f),Stone);
    b.Box(new Vector3(x*.33f,.18f,z*.33f),new Vector3(.23f,.06f,.23f),Gold);
   }
   b.Crystal(new Vector3(0,-.03f,0),new Vector3(.17f,.22f,.17f),Jade);
   b.Box(new Vector3(0,.32f,0),new Vector3(1,.13f,1),Gold);
   b.Box(new Vector3(0,.41f,0),new Vector3(.92f,.13f,.92f),Dark);
   b.Box(new Vector3(0,.484f,0),new Vector3(.55f,.025f,.55f),Jade);
   b.Box(new Vector3(0,.498f,0),new Vector3(.32f,.004f,.32f),Gold);
  }else{
   b.Box(new Vector3(0,-.43f,0),new Vector3(.98f,.14f,.98f),Dark);
   b.Box(new Vector3(0,altar?-.12f:-.02f,0),new Vector3(.82f,altar?.54f:.76f,.82f),Stone);
   b.Box(new Vector3(0,.32f,0),new Vector3(1,.13f,1),Gold);
   b.Box(new Vector3(0,.41f,0),new Vector3(.92f,.13f,.92f),Dark);
   for(int side=-1;side<=1;side+=2){
    b.Box(new Vector3(side*.43f,-.04f,0),new Vector3(.055f,.6f,.85f),Gold);
    b.Box(new Vector3(0,-.04f,side*.43f),new Vector3(.85f,.6f,.055f),Gold);
    b.Box(new Vector3(0,.0f,side*.463f),new Vector3(.12f,.25f,.018f),Jade);
   }
   // A recessed seal reads as a magical object without extending into the room.
   b.Box(new Vector3(0,.484f,0),new Vector3(.50f,.025f,.50f),Jade);
   b.Box(new Vector3(0,.498f,0),new Vector3(.32f,.004f,.32f),Gold);
   if(kind==RoomThemeKind.Sofa||kind==RoomThemeKind.Bed){b.Box(new Vector3(0,.25f,-.36f),new Vector3(.75f,.4f,.15f),Stone);}
  }
  b.Transform(Matrix4x4.TRS(offset,boxRotation,extent));return Store(id,kind,position,rotation,b);
 }
 bool Store(Guid id,RoomThemeKind kind,Vector3 position,Quaternion rotation,Builder builder){
  if(!surfaces.TryGetValue(id,out var go)){
   if(Count>=SurfaceLimit)return false;
   go=new GameObject("Room "+kind+" "+id,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);surfaces.Add(id,go);
   var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.sortingOrder=kind==RoomThemeKind.Floor?0:kind<RoomThemeKind.Table?1:2;
  }
  var filter=go.GetComponent<MeshFilter>();if(filter.sharedMesh)Release(filter.sharedMesh);
  filter.sharedMesh=builder.Finish(kind.ToString());go.transform.SetLocalPositionAndRotation(position,rotation);return true;
 }
 public void Remove(Guid id){if(!surfaces.TryGetValue(id,out var go))return;go.SetActive(false);Release(go.GetComponent<MeshFilter>().sharedMesh);Release(go);surfaces.Remove(id);}
 public void Clear(){foreach(var id in new List<Guid>(surfaces.Keys))Remove(id);}
 public void Dispose(){Clear();Release(material);Release(root.gameObject);}
 static Vector3 V(Vector2 p)=>new Vector3(p.x,p.y,0);
 static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
 static bool ValidPose(Vector3 p,Quaternion q)=>Finite(p.x)&&Finite(p.y)&&Finite(p.z)&&Finite(q.x)&&Finite(q.y)&&Finite(q.z)&&Finite(q.w)&&Quaternion.Dot(q,q)>.5f;
 static bool Inside(Vector2 p,Vector2[] polygon){bool inside=false;for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++){var a=polygon[i];var b=polygon[j];if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;}return inside;}
 static void Release(UnityEngine.Object o){if(!o)return;if(Application.isPlaying)UnityEngine.Object.Destroy(o);else UnityEngine.Object.DestroyImmediate(o);}
 sealed class Builder {
  readonly List<Vector3> vertices=new List<Vector3>();readonly List<Color> colors=new List<Color>();readonly List<int> triangles=new List<int>();
  public void Triangle(Vector3 a,Vector3 b,Vector3 c,Color color){int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);}
  void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){Triangle(a,b,c,color);Triangle(a,c,d,color);}
  public void Strip(Vector3 a,Vector3 b,float width,Color color){var side=Vector3.Cross((b-a).normalized,Vector3.forward)*width*.5f;Quad(a-side,a+side,b+side,b-side,color);Quad(b-side,b+side,a+side,a-side,color);}
  public void Diamond(Vector3 c,float r,Color color){var p=new[]{c+Vector3.up*r,c+Vector3.right*r,c-Vector3.up*r,c-Vector3.right*r,c+Vector3.up*r};for(int i=0;i<4;i++)Strip(p[i],p[i+1],.009f,color);}
  public void Box(Vector3 p,Vector3 size,Color c){
   var v=new Vector3[8];for(int i=0;i<8;i++)v[i]=p+Vector3.Scale(new Vector3((i&1)==0?-.5f:.5f,(i&2)==0?-.5f:.5f,(i&4)==0?-.5f:.5f),size);
   Quad(v[0],v[2],v[3],v[1],c*.88f);Quad(v[4],v[5],v[7],v[6],c);Quad(v[0],v[4],v[6],v[2],c*.86f);Quad(v[1],v[3],v[7],v[5],c*.94f);Quad(v[0],v[1],v[5],v[4],c*.8f);Quad(v[2],v[6],v[7],v[3],c*1.12f);
  }
  public void Crystal(Vector3 p,Vector3 s,Color c){var top=p+Vector3.up*s.y;var bottom=p-Vector3.up*s.y;for(int i=0;i<6;i++){float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;var u=p+new Vector3(Mathf.Cos(a)*s.x,0,Mathf.Sin(a)*s.z);var v=p+new Vector3(Mathf.Cos(b)*s.x,0,Mathf.Sin(b)*s.z);Triangle(top,u,v,c);Triangle(bottom,v,u,c*.75f);}}
  public void Transform(Matrix4x4 matrix){for(int i=0;i<vertices.Count;i++)vertices[i]=matrix.MultiplyPoint3x4(vertices[i]);}
  public Mesh Finish(string name){var mesh=new Mesh{name="Scanned "+name+" dressing"};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
 }
}
}
