using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace SpatialSurvivor.Tests {
public class SurvivorRoomThemeRenderingTests {
 [Test] public void ThemedWallLeavesCreaturesAndUiVisibleAndToggleRestoresAlpha(){
  var origin=new GameObject("Room theme rendering test");origin.transform.position=Vector3.one*100;
  var theme=new SurvivorRoomTheme(origin.transform);var cameraGo=new GameObject("Camera",typeof(Camera));cameraGo.transform.SetParent(origin.transform,false);var camera=cameraGo.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1;camera.nearClipPlane=.03f;
  var target=new RenderTexture(64,64,24,RenderTextureFormat.ARGB32);var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
  var actor=GameObject.CreatePrimitive(PrimitiveType.Cube);actor.transform.SetParent(origin.transform,false);actor.transform.localPosition=Vector3.forward*2;var paint=new Material(Shader.Find("Sprites/Default")){color=Color.green};actor.GetComponent<Renderer>().sharedMaterial=paint;
  var canvasGo=new GameObject("UI",typeof(RectTransform),typeof(Canvas));canvasGo.transform.SetParent(origin.transform,false);canvasGo.transform.localPosition=Vector3.forward*2;canvasGo.GetComponent<RectTransform>().sizeDelta=Vector2.one*2;var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
  var image=new GameObject("Graphic",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));image.transform.SetParent(canvasGo.transform,false);image.GetComponent<RectTransform>().sizeDelta=Vector2.one*2;image.GetComponent<Image>().color=Color.red;canvasGo.SetActive(false);
  try{
   theme.UpsertPolygon(Guid.NewGuid(),RoomThemeKind.Wall,Vector3.forward,Quaternion.identity,new[]{new Vector2(-2,-2),new Vector2(2,-2),new Vector2(2,2),new Vector2(-2,2)});camera.targetTexture=target;
   Assert.That(Read(camera,target,texture).g,Is.GreaterThan(.5f),"The room theme must not occlude an actor behind it");
   actor.SetActive(false);canvasGo.SetActive(true);Canvas.ForceUpdateCanvases();Assert.That(Read(camera,target,texture).r,Is.GreaterThan(.5f),"The room theme must not occlude world UI");
   canvasGo.SetActive(false);Assert.That(Read(camera,target,texture).a,Is.GreaterThan(.5f));theme.SetVisible(false);Assert.That(Read(camera,target,texture).a,Is.LessThan(.03f),"Disabling theme restores the passthrough background");
  }finally{RenderTexture.active=previous;camera.targetTexture=null;theme.Dispose();Object.DestroyImmediate(origin);Object.DestroyImmediate(target);Object.DestroyImmediate(texture);Object.DestroyImmediate(paint);}
 }
 static Color Read(Camera camera,RenderTexture target,Texture2D texture){camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,64,64),0,0);texture.Apply();return texture.GetPixel(32,32);}
}
}
