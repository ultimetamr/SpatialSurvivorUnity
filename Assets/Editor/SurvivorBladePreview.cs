using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace SpatialSurvivor.Editor {
public static class SurvivorBladePreview {
 public static void Render(){
  SurvivorProject.BatchInitialize();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var camera=new GameObject("Blade direction review").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=.9f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.08f,.10f);
  camera.transform.position=new Vector3(1.7f,1.25f,2.5f);camera.transform.LookAt(new Vector3(0,.15f,.2f));
  var hand=Object.Instantiate(Resources.Load<GameObject>("Art/Weapons/HandRuneblade"));
  var arrow=Line(new[]{new Vector3(0,-.02f,-.15f),new Vector3(0,-.02f,.95f)},new Color(.35f,.85f,.95f));
  Capture(camera,"v261-hand-sword");Object.DestroyImmediate(hand);Object.DestroyImmediate(arrow);
  var rain=SurvivorVisualCatalog.Create(SkillVisual.SwordRain,new GameObject("Rain root").transform);rain.transform.SetPositionAndRotation(new Vector3(0,.7f,0),SurvivorBladePose.RainRotation);
  arrow=Line(new[]{new Vector3(.2f,.8f,0),new Vector3(.2f,-.2f,0)},new Color(.35f,.85f,.95f));Capture(camera,"v261-sword-rain");Object.DestroyImmediate(rain.transform.parent.gameObject);Object.DestroyImmediate(arrow);
  var orbitRoot=new GameObject("Orbit tangent review");camera.transform.position=new Vector3(2,3,3);camera.transform.LookAt(Vector3.zero);camera.orthographicSize=1.6f;
  for(int i=0;i<6;i++){float angle=i*60;float a=angle*Mathf.Deg2Rad;var pivot=new GameObject("Flying sword").transform;pivot.SetParent(orbitRoot.transform);pivot.SetPositionAndRotation(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),SurvivorBladePose.OrbitRotation(angle));SurvivorVisualCatalog.Create(i%2==0?SkillVisual.OrbitSword:SkillVisual.MyriadSwords,pivot);}
  var circle=new Vector3[65];for(int i=0;i<circle.Length;i++){float a=i*Mathf.PI*2/64;circle[i]=new Vector3(Mathf.Cos(a),-.08f,Mathf.Sin(a));}arrow=Line(circle,new Color(.35f,.85f,.95f));Capture(camera,"v261-orbit-swords");Object.DestroyImmediate(orbitRoot);Object.DestroyImmediate(arrow);
  Debug.Log("V261 blade direction previews rendered.");
 }
 static GameObject Line(Vector3[] points,Color color){var go=new GameObject("Motion direction guide");var line=go.AddComponent<LineRenderer>();line.sharedMaterial=new Material(Shader.Find("Sprites/Default"));line.startWidth=line.endWidth=.006f;line.positionCount=points.Length;line.SetPositions(points);line.startColor=line.endColor=color;return go;}
 static void Capture(Camera camera,string name){
  var target=new RenderTexture(960,800,24);var tex=new Texture2D(960,800,TextureFormat.RGB24,false);var old=RenderTexture.active;
  camera.targetTexture=target;camera.Render();RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,960,800),0,0);tex.Apply();Directory.CreateDirectory("Docs/Validation");File.WriteAllBytes("Docs/Validation/"+name+".png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(target);Object.DestroyImmediate(tex);
 }
}
}
