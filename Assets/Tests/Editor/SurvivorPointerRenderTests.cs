using NUnit.Framework;
using UnityEngine;
namespace SpatialSurvivor.Tests {
public class SurvivorPointerRenderTests {
 [Test] public void WholePointerRemainsVisibleAcrossForegroundGeometry(){
  var root=new GameObject("Pointer render regression");
  var target=new RenderTexture(128,64,24);var image=new Texture2D(128,64,TextureFormat.RGB24,false);
  var material=new Material(Resources.Load<Shader>("Shaders/PointerOverlay"));
  var occluderMaterial=new Material(Shader.Find("Unlit/Color")){color=Color.black};
  var previous=RenderTexture.active;
  try{
   var camera=root.AddComponent<Camera>();camera.transform.position=new Vector3(0,0,-2);camera.orthographic=true;camera.orthographicSize=1;camera.aspect=2;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.cullingMask=1<<30;camera.targetTexture=target;
   var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.transform.SetParent(root.transform,false);obstacle.layer=30;obstacle.transform.position=new Vector3(.8f,0,1);obstacle.transform.localScale=new Vector3(1.7f,1.5f,.2f);obstacle.GetComponent<Renderer>().sharedMaterial=occluderMaterial;
   var beam=new GameObject("Ray",typeof(LineRenderer));beam.transform.SetParent(root.transform,false);beam.layer=30;var line=beam.GetComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=2;line.startWidth=line.endWidth=.12f;line.startColor=line.endColor=Color.cyan;line.SetPosition(0,new Vector3(-1.5f,0,2));line.SetPosition(1,new Vector3(1.5f,0,2));
   camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,128,64),0,0);image.Apply();
   foreach(int x in new[]{32,96}){var pixel=image.GetPixel(x,32);Assert.That(pixel.g,Is.GreaterThan(.7f),"Pointer must remain visible on both sides of the foreground obstacle");Assert.That(pixel.b,Is.GreaterThan(.7f));}
  }finally{RenderTexture.active=previous;Object.DestroyImmediate(root);Object.DestroyImmediate(target);Object.DestroyImmediate(image);Object.DestroyImmediate(material);Object.DestroyImmediate(occluderMaterial);}
 }
}
}
