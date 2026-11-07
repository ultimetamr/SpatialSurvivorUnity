using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
namespace SpatialSurvivor.Tests {
public class SurvivorMrRenderingTests {
 [Test] public void RoomNeverOccludesModelsOrWorldUiAndRetainsNavigationColliders(){
  var origin=new GameObject("MR render test");origin.transform.position=new Vector3(100,100,100);
  var occlusion=new Material(Resources.Load<Shader>("Shaders/RoomOcclusion"));var visible=new Material(Shader.Find("Sprites/Default")){color=Color.green};
  var room=new SurvivorRoomGeometry(origin.transform,"MR test occluder",occlusion);
  var cameraObject=new GameObject("MR test camera",typeof(Camera));cameraObject.transform.SetParent(origin.transform,false);
  var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<RoomNavigation.Layer;camera.nearClipPlane=.05f;
  var target=new RenderTexture(64,64,24,RenderTextureFormat.ARGB32);var pixel=new Texture2D(64,64,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
  var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.layer=RoomNavigation.Layer;cube.transform.SetParent(origin.transform,false);cube.transform.localPosition=Vector3.forward*2;cube.GetComponent<Renderer>().sharedMaterial=visible;
  var canvasObject=new GameObject("World UI behind room",typeof(RectTransform),typeof(Canvas));canvasObject.layer=RoomNavigation.Layer;canvasObject.transform.SetParent(origin.transform,false);canvasObject.transform.localPosition=Vector3.forward*2;var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;canvasObject.GetComponent<RectTransform>().sizeDelta=new Vector2(2,2);
  var image=new GameObject("UI graphic",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));image.layer=RoomNavigation.Layer;image.transform.SetParent(canvasObject.transform,false);image.GetComponent<RectTransform>().sizeDelta=new Vector2(2,2);image.GetComponent<Image>().color=Color.red;canvasObject.SetActive(false);
  try {
   camera.targetTexture=target;var id=Guid.NewGuid();room.Upsert(id,Vector3.forward,Quaternion.identity,new[]{new Vector3(-2,-2,0),new Vector3(-2,2,0),new Vector3(2,2,0),new Vector3(2,-2,0)},new[]{0,1,2,0,2,3});
   var surface=origin.GetComponentInChildren<MeshCollider>();Assert.That(surface.enabled&&surface.sharedMesh,Is.True);Assert.That(surface.GetComponent<Renderer>().enabled,Is.False);
   Assert.That(Read(camera,target,pixel).g,Is.GreaterThan(.5f),"Room must not hide a creature");
   cube.SetActive(false);canvasObject.SetActive(true);Canvas.ForceUpdateCanvases();Assert.That(Read(camera,target,pixel).r,Is.GreaterThan(.5f),"Room must not hide world UI");canvasObject.SetActive(false);
   Assert.That(Read(camera,target,pixel).a,Is.LessThan(.03f),"Hidden room must preserve passthrough alpha");
   room.SetPreview(true);occlusion.SetFloat("_Preview",1);var preview=Read(camera,target,pixel);Assert.That(preview.b,Is.GreaterThan(.05f));Assert.That(preview.a,Is.GreaterThan(.05f));
   cube.SetActive(true);Assert.That(Read(camera,target,pixel).g,Is.GreaterThan(.5f),"Diagnostic preview must not write depth in front of a creature");cube.SetActive(false);canvasObject.SetActive(true);Canvas.ForceUpdateCanvases();Assert.That(Read(camera,target,pixel).r,Is.GreaterThan(.5f),"Diagnostic preview must not hide UI");
   room.SetPreview(false);occlusion.SetFloat("_Preview",0);Assert.That(Read(camera,target,pixel).r,Is.GreaterThan(.5f));Assert.That(surface.enabled,Is.True);
   var second=Guid.NewGuid();room.Upsert(second,Vector3.forward,Quaternion.identity,new[]{Vector3.zero,Vector3.up,Vector3.right},new[]{0,1,2});foreach(var renderer in origin.GetComponentsInChildren<MeshRenderer>())if(renderer.GetComponent<MeshCollider>()&&renderer.gameObject!=cube)Assert.That(renderer.enabled,Is.False,"New scans inherit hidden state");
  } finally {RenderTexture.active=previous;camera.targetTexture=null;room.Dispose();UnityEngine.Object.DestroyImmediate(origin);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixel);UnityEngine.Object.DestroyImmediate(occlusion);UnityEngine.Object.DestroyImmediate(visible);}
 }
 static Color Read(Camera camera,RenderTexture target,Texture2D pixel){camera.Render();RenderTexture.active=target;pixel.ReadPixels(new Rect(0,0,64,64),0,0);pixel.Apply();return pixel.GetPixel(32,32);}
}
}
