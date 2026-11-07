using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
namespace SpatialSurvivor.Editor {
public static class SurvivorArtPreview {
 public static void Render(){SurvivorArtBuilder.Build();
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var cam=new GameObject("Art preview camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=.73f;cam.backgroundColor=new Color(.075f,.065f,.13f);cam.clearFlags=CameraClearFlags.SolidColor;cam.transform.position=new Vector3(0,.62f,5);cam.transform.rotation=Quaternion.Euler(0,180,0);
  var actors=new GameObject[5];for(int i=0;i<5;i++){actors[i]=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/"+SurvivorArtBuilder.Creatures[i]));actors[i].transform.position=new Vector3((2-i)*1.12f,0,0);actors[i].transform.rotation=Quaternion.Euler(0,-12,0);var skin=actors[i].GetComponent<SkinnedMeshRenderer>();skin.quality=SkinQuality.Bone4;var mesh=skin.sharedMesh;float edge=0;int bad=0;var vv=mesh.vertices;var ii=mesh.triangles;for(int k=0;k<ii.Length;k+=3){if(ii[k]>=vv.Length||ii[k+1]>=vv.Length||ii[k]<0||ii[k+1]<0){Debug.LogError("Invalid triangle "+k+" ids="+ii[k]+","+ii[k+1]+" vertices="+vv.Length);break;}float e=Vector3.Distance(vv[ii[k]],vv[ii[k+1]]);edge=Mathf.Max(edge,e);if(e>.5f)bad++;}Debug.Log(actors[i].name+" vertices="+vv.Length+" maxEdge="+edge+" bad="+bad);}
  Directory.CreateDirectory("Docs/Validation");Capture(cam,"Docs/Validation/creature-gallery.png",2000,520);
  for(int i=0;i<5;i++)Object.DestroyImmediate(actors[i]);
  for(int f=0;f<6;f++){var actor=Object.Instantiate(Resources.Load<GameObject>("Art/Creatures/EmberImp"));actor.transform.position=new Vector3((2.5f-f)*.93f,0,0);var a=actor.GetComponent<SurvivorCreatureAnimator>();a.attack.SampleAnimation(actor,f*.55f/6);}
  Capture(cam,"Docs/Validation/attack-animation.png",2000,520);Debug.Log("Rendered storybook gallery and attack animation contact sheet.");
 }
 static void Capture(Camera cam,string path,int w,int h){var rt=new RenderTexture(w,h,24);var tex=new Texture2D(w,h,TextureFormat.RGB24,false);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
}
}
