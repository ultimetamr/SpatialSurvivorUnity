using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpatialSurvivor.Editor {
public static class SurvivorArtBuilder {
 public const string Root="Assets/Resources/Art";
 public static readonly string[] Creatures={"Mossling","EmberImp","CrystalGuardian","MoonWisp","ElderBloom","SkyMoth","StormDrake"};
 static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
 [MenuItem("SpatialSurvivor/Art/Rebuild Storybook Assets")]
 public static void Build(){
  Directory.CreateDirectory(Root+"/Creatures");Directory.CreateDirectory(Root+"/Weapons");Directory.CreateDirectory(Root+"/Animations");
  var material=new Material(Shader.Find("Survivor/Storybook")){name="Storybook Vertex Paint"};Save(material,Root+"/Storybook.mat");material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Storybook.mat");
  for(int i=0;i<Creatures.Length;i++)Creature(i,material);
  Weapons(material);HandWeapons(material);ImportArt();AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Storybook art rebuilt: 7 articulated creatures, 42 clips and complete magic arsenal.");
 }
 static void Save(UnityEngine.Object asset,string path){var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);if(old){if(asset is Mesh source&&old is Mesh target){target.Clear();target.indexFormat=source.indexFormat;target.vertices=source.vertices;target.colors=source.colors;target.normals=source.normals;target.triangles=source.triangles;target.bindposes=source.bindposes;target.boneWeights=source.boneWeights;target.bounds=source.bounds;}else EditorUtility.CopySerialized(asset,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(asset);}else AssetDatabase.CreateAsset(asset,path);}
 static Transform Bone(string name,Transform parent,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
 static void Creature(int type,Material material){
  var go=new GameObject(Creatures[type]);var rig=Bone("Rig",go.transform,Vector3.zero);
  var body=Bone("Body",rig,new Vector3(0,.4f,0));var head=Bone("Head",body,new Vector3(0,.29f,.015f));
  float shoulder=type==2||type==4?.29f:.2f;
  var la=Bone("ArmL",body,new Vector3(-shoulder,.05f,0));var ra=Bone("ArmR",body,new Vector3(shoulder,.05f,0));
  var lf=Bone("ForearmL",la,new Vector3(-.025f,-.14f,0));var rf=Bone("ForearmR",ra,new Vector3(.025f,-.14f,0));
  var ll=Bone("LegL",rig,new Vector3(-.115f,.22f,0));var rl=Bone("LegR",rig,new Vector3(.115f,.22f,0));
  var tail=Bone("Tail",body,new Vector3(0,-.05f,-.13f));var lw=Bone("WingL",body,new Vector3(-.16f,.15f,-.06f));var rw=Bone("WingR",body,new Vector3(.16f,.15f,-.06f));
  var bones=new[]{rig,body,head,la,ra,lf,rf,ll,rl,tail,lw,rw};var m=new Writer(bones);
  Color skin=type==0?C("88A964"):type==1?C("D87955"):type==2?C("627E99"):type==3?C("A49ACD"):type==5?C("62BFC5"):type==6?C("8474C6"):C("70644B");
  Color pale=type==0?C("ECE0AE"):type==1?C("F4B88A"):type==2?C("99B6C4"):type==3?C("E5DCF3"):C("B4A078");
  Color dark=type==0?C("496951"):type==1?C("843F4A"):type==2?C("42506C"):type==3?C("655A99"):C("385F54");
  Color gold=C("D6AD61"),eye=type==2||type==4?C("63E8ED"):C("CB8733");
  bool bulky=type==2||type==4;
  m.Blob(body,Vector3.zero,new Vector3(bulky?.29f:.185f,.25f,bulky?.19f:.14f),skin,bulky?.65f:1);
  m.Blob(body,new Vector3(0,-.04f,.125f),new Vector3(bulky?.18f:.13f,.17f,.055f),pale);
  m.Blob(head,Vector3.zero,new Vector3(bulky?.235f:.235f,.205f,.175f),skin,bulky?.65f:1);
  m.Blob(head,new Vector3(0,-.06f,.135f),new Vector3(.18f,.125f,.063f),pale);
  foreach(int side in new[]{-1,1}){
   float x=side*.087f;
   m.Blob(head,new Vector3(x,.018f,.153f),new Vector3(.066f,.077f,.035f),C("FFF5DD"));
   m.Blob(head,new Vector3(x,.012f,.184f),new Vector3(.043f,.052f,.016f),eye);
   m.Blob(head,new Vector3(x,.01f,.197f),new Vector3(.023f,.035f,.009f),C("302C3A"));
   m.Blob(head,new Vector3(x-.012f,.029f,.205f),new Vector3(.012f,.014f,.004f),Color.white);
   m.Tube(head,new[]{new Vector3(x-side*.048f,.104f,.151f),new Vector3(x,.12f,.168f),new Vector3(x+side*.044f,.105f,.148f)},.013f,.009f,dark);
   m.Blob(head,new Vector3(side*.155f,-.06f,.16f),new Vector3(.035f,.018f,.01f),type==2?C("90C8CE"):C("DB9C85"));
   Transform arm=side<0?la:ra,fore=side<0?lf:rf,leg=side<0?ll:rl;
   m.Blob(arm,new Vector3(side*.012f,-.06f,0),new Vector3(bulky?.095f:.058f,.11f,.065f),skin,bulky?.7f:1);
   m.Blob(fore,new Vector3(side*.009f,-.05f,.01f),new Vector3(bulky?.105f:.067f,.1f,.075f),skin,bulky?.65f:1);
   m.Blob(fore,new Vector3(side*.005f,-.11f,.03f),new Vector3(bulky?.09f:.06f,.045f,.07f),pale);
   for(int finger=0;finger<3;finger++)m.Blob(fore,new Vector3(side*(-.036f+finger*.027f),-.125f,.063f),new Vector3(.019f,.027f,.03f),pale);
   m.Blob(leg,new Vector3(0,-.07f,0),new Vector3(bulky?.092f:.07f,.14f,.075f),skin);
   m.Blob(leg,new Vector3(0,-.17f,.04f),new Vector3(bulky?.105f:.084f,.055f,.118f),dark,bulky?.65f:1);
  }
  m.Blob(head,new Vector3(0,-.043f,.205f),new Vector3(.028f,.019f,.017f),dark);
  m.Tube(head,new[]{new Vector3(-.045f,-.09f,.186f),new Vector3(0,-.107f,.201f),new Vector3(.045f,-.09f,.186f)},.006f,.006f,C("664F4D"));
  if(type==0||type==4){
   for(int i=0;i<9;i++){float a=(i-4)*.42f;var start=new Vector3(Mathf.Sin(a)*.12f,.14f,Mathf.Cos(a)*.08f-.04f);m.Leaf(head,start,start+new Vector3(Mathf.Sin(a)*.19f,.19f-i%2*.06f,-.045f),.07f,i%2==0?C("8EA756"):C("4F7954"));}
   for(int side=-1;side<=1;side+=2){m.Leaf(body,new Vector3(side*.02f,.16f,.13f),new Vector3(side*.24f,-.09f,.14f),.09f,dark);m.Leaf(head,new Vector3(side*.17f,.015f,0),new Vector3(side*.34f,.1f,.025f),.085f,C("9BB765"));}
   m.Gem(body,new Vector3(0,.12f,.175f),new Vector3(.042f,.052f,.025f),gold);
   if(type==4){for(int side=-1;side<=1;side+=2){var p=new[]{new Vector3(side*.13f,.14f,-.03f),new Vector3(side*.23f,.32f,-.02f),new Vector3(side*.34f,.44f,-.04f),new Vector3(side*.4f,.5f,-.06f)};m.Tube(head,p,.043f,.005f,dark);m.Tube(head,new[]{p[1],new Vector3(side*.11f,.43f,-.04f),new Vector3(side*.17f,.53f,-.05f)},.026f,.003f,dark);m.Leaf(head,p[2],p[2]+new Vector3(side*.11f,.08f,0),.045f,C("A3B36A"));for(int i=0;i<3;i++)m.Gem(side<0?la:ra,new Vector3(side*(.02f+i*.03f),.02f+i*.045f,-.015f),new Vector3(.035f,.085f,.035f),C("70D0C3"));}m.Gem(body,new Vector3(0,0,.193f),new Vector3(.085f,.12f,.05f),C("70E2CD"));}
  }
  if(type==1){
   for(int side=-1;side<=1;side+=2){m.Leaf(head,new Vector3(side*.17f,.04f,0),new Vector3(side*.36f,.17f,-.03f),.07f,skin);m.Leaf(head,new Vector3(side*.2f,.065f,.018f),new Vector3(side*.33f,.155f,0),.04f,pale);m.Tube(head,new[]{new Vector3(side*.13f,.14f,-.03f),new Vector3(side*.17f,.28f,-.055f),new Vector3(side*.12f,.34f,-.03f)},.048f,.005f,dark);}
   m.Tube(tail,new[]{Vector3.zero,new Vector3(.04f,-.11f,-.14f),new Vector3(.17f,-.16f,-.22f),new Vector3(.24f,-.04f,-.21f)},.025f,.012f,skin);m.Leaf(tail,new Vector3(.21f,-.08f,-.21f),new Vector3(.26f,.06f,-.21f),.05f,C("F0B957"));
   for(int i=0;i<5;i++)m.Leaf(head,new Vector3((i-2)*.05f,.16f,.04f),new Vector3((i-2)*.07f,.32f+i%2*.06f,-.055f),.04f,C("DFA154"));
   m.Tube(body,new[]{new Vector3(-.16f,.15f,0),new Vector3(0,.11f,.16f),new Vector3(.16f,.15f,0)},.035f,.035f,C("8F5265"));
  }
  if(type==2){
   for(int side=-1;side<=1;side+=2){var arm=side<0?la:ra;m.Blob(arm,new Vector3(side*.025f,.035f,0),new Vector3(.125f,.105f,.12f),dark,.55f);for(int i=0;i<3;i++)m.Gem(arm,new Vector3(side*(.03f+i*.026f),.08f+i*.018f,-.015f),new Vector3(.035f,.115f+i*.015f,.04f),C("87D8E5"));}
   for(int i=0;i<5;i++)m.Gem(head,new Vector3((i-2)*.068f,.16f,-.02f),new Vector3(.045f,.12f+(.05f*(1-Mathf.Abs(i-2)/2f)),.055f),i%2==0?C("9ADBEC"):C("7B9FDC"));
   m.Gem(body,new Vector3(0,.02f,.19f),new Vector3(.09f,.12f,.035f),C("6BE0EE"));
  }
  if(type==3||type>=5){
   for(int side=-1;side<=1;side+=2){var wing=side<0?lw:rw;m.Wing(wing,side,C("9180BC"),C("D4C9EC"));m.Leaf(head,new Vector3(side*.17f,.04f,0),new Vector3(side*.31f,.13f,0),.065f,skin);}
   m.Gem(head,new Vector3(0,.13f,.14f),new Vector3(.045f,.06f,.018f),gold);m.Tube(head,new[]{new Vector3(-.19f,.09f,.04f),new Vector3(0,.14f,.15f),new Vector3(.19f,.09f,.04f)},.014f,.014f,gold);
  }
  if(type>=5){for(int side=-1;side<=1;side+=2){var wing=side<0?lw:rw;if(type==5){m.Leaf(wing,Vector3.zero,new Vector3(side*.55f,.32f,.04f),.2f,C("92E0DC"));m.Leaf(wing,Vector3.zero,new Vector3(side*.42f,-.22f,.02f),.15f,C("4C93AB"));m.Gem(wing,new Vector3(side*.29f,.15f,.065f),new Vector3(.05f,.06f,.015f),gold);m.Tube(head,new[]{new Vector3(side*.08f,.15f,0),new Vector3(side*.15f,.35f,.025f)},.012f,.006f,gold);m.Blob(head,new Vector3(side*.15f,.35f,.025f),Vector3.one*.025f,pale);}else{m.Gem(head,new Vector3(side*.13f,.17f,0),new Vector3(.045f,.19f,.04f),C("EACD8B"));m.Wing(wing,side,C("5C518A"),C("A79BE6"));}}if(type==6)m.Tube(tail,new[]{Vector3.zero,new Vector3(0,-.1f,-.3f),new Vector3(.14f,-.04f,-.45f)},.035f,.004f,skin);}
  var mesh=m.Finish(Creatures[type]+" Painted Skin");Save(mesh,Root+"/Creatures/"+Creatures[type]+".asset");mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Creatures/"+Creatures[type]+".asset");
  var skinRenderer=go.AddComponent<SkinnedMeshRenderer>();skinRenderer.sharedMesh=mesh;skinRenderer.bones=bones;skinRenderer.rootBone=rig;skinRenderer.sharedMaterial=material;skinRenderer.localBounds=new Bounds(new Vector3(0,.5f,0),new Vector3(2,2,2));skinRenderer.updateWhenOffscreen=false;
  var animator=go.AddComponent<SurvivorCreatureAnimator>();AnimationClip[] clips=new AnimationClip[6];for(int i=0;i<clips.Length;i++)clips[i]=Clip(go,bones,type,(CreatureMotion)i);
  animator.idle=clips[0];animator.walk=clips[1];animator.attack=clips[2];animator.hit=clips[3];animator.spawn=clips[4];animator.death=clips[5];
  PrefabUtility.SaveAsPrefabAsset(go,Root+"/Creatures/"+Creatures[type]+".prefab");UnityEngine.Object.DestroyImmediate(go);
 }
 static AnimationClip Clip(GameObject root,Transform[] bones,int type,CreatureMotion state){
  float length=state==CreatureMotion.Walk?.72f:state==CreatureMotion.Attack?.55f:state==CreatureMotion.Hit?.22f:state==CreatureMotion.Spawn?.6f:state==CreatureMotion.Death?.45f:2.4f;
  var clip=new AnimationClip{name=Creatures[type]+"_"+state,frameRate=30,legacy=true,wrapMode=state==CreatureMotion.Idle||state==CreatureMotion.Walk?WrapMode.Loop:WrapMode.ClampForever};
  for(int b=0;b<bones.Length;b++){
   var tx=new AnimationCurve();var ty=new AnimationCurve();var tz=new AnimationCurve();var rx=new AnimationCurve();var ry=new AnimationCurve();var rz=new AnimationCurve();var sx=new AnimationCurve();var sy=new AnimationCurve();var sz=new AnimationCurve();
   for(int f=0;f<=30;f++){float t=f/30f,s=Mathf.Sin(t*Mathf.PI*2),p=Mathf.Sin(t*Mathf.PI),bounce=Mathf.Abs(s);var pos=bones[b].localPosition;var rot=Vector3.zero;var scale=Vector3.one;
    if(state==CreatureMotion.Idle){if(b==1){pos.y+=s*.008f;scale=new Vector3(1-s*.012f,1+s*.02f,1-s*.012f);}if(b==2)rot.z=s*2;if(b==3||b==4)rot.z=(b==3?-1:1)*(5+s*2);if(b>=10)rot.y=(b==10?-1:1)*(12+s*8);}
    if(state==CreatureMotion.Walk){if(b==0)pos.y+=bounce*.025f;if(b==1){rot.z=s*(type==2?4:7);rot.x=5;}if(b==7||b==8)rot.x=s*(b==7?1:-1)*(type==2?22:35);if(b==3||b==4)rot.x=s*(b==3?-1:1)*32;if(b==5||b==6)rot.x=-12;if(b==2)rot.z=-s*3;if(b==9)rot.y=s*23;if(b>=10)rot.y=(b==10?-1:1)*(20+Mathf.Sin(t*Mathf.PI*4)*25);}
    if(state==CreatureMotion.Attack){float wind=t<.35f?-t/.35f:Mathf.Sin((t-.35f)/.65f*Mathf.PI);if(b==1){rot.x=wind*24;pos.z+=Mathf.Max(0,wind)*.065f;}if(b==3||b==4)rot.x=-wind*100;if(b==5||b==6)rot.x=-Mathf.Max(0,wind)*28;if(b==2)rot.x=-wind*12;if(b>=10)rot.y=(b==10?-1:1)*wind*40;}
    if(state==CreatureMotion.Hit){if(b==1){rot.x=-p*22;rot.z=Mathf.Sin(t*Mathf.PI*3)*9;pos.z-=p*.045f;}if(b==3||b==4)rot.z=(b==3?1:-1)*p*20;}
    if(state==CreatureMotion.Spawn){if(b==0){float q=Mathf.Clamp01(t*1.6f);scale=Vector3.one*Mathf.Lerp(.03f,1,Mathf.SmoothStep(0,1,q));pos.y+=Mathf.Sin(t*Mathf.PI)*.1f;}if(b==3||b==4)rot.z=(b==3?1:-1)*p*28;}
    if(state==CreatureMotion.Death){if(b==0){rot.z=t*78;pos.y-=t*.18f;scale=Vector3.one*Mathf.Lerp(1,.05f,Mathf.Pow(t,2));}if(b==3||b==4)rot.z=(b==3?-1:1)*p*40;if(b==2)rot.x=-t*25;}
    if(type>=5&&(state==CreatureMotion.Idle||state==CreatureMotion.Walk)){if(b==0)pos.y+=s*.035f;if(b>=10){rot.z=(b==10?1:-1)*Mathf.Sin(t*Mathf.PI*4)*42;rot.y=(b==10?-1:1)*12;}if(b==7||b==8)rot.x=-25;}
    float time=t*length;tx.AddKey(time,pos.x);ty.AddKey(time,pos.y);tz.AddKey(time,pos.z);rx.AddKey(time,rot.x);ry.AddKey(time,rot.y);rz.AddKey(time,rot.z);sx.AddKey(time,scale.x);sy.AddKey(time,scale.y);sz.AddKey(time,scale.z);
   }
   string path=AnimationUtility.CalculateTransformPath(bones[b],root.transform);clip.SetCurve(path,typeof(Transform),"localPosition.x",tx);clip.SetCurve(path,typeof(Transform),"localPosition.y",ty);clip.SetCurve(path,typeof(Transform),"localPosition.z",tz);clip.SetCurve(path,typeof(Transform),"localEulerAnglesRaw.x",rx);clip.SetCurve(path,typeof(Transform),"localEulerAnglesRaw.y",ry);clip.SetCurve(path,typeof(Transform),"localEulerAnglesRaw.z",rz);clip.SetCurve(path,typeof(Transform),"localScale.x",sx);clip.SetCurve(path,typeof(Transform),"localScale.y",sy);clip.SetCurve(path,typeof(Transform),"localScale.z",sz);
  }
  string final=Root+"/Animations/"+clip.name+".anim";Save(clip,final);return AssetDatabase.LoadAssetAtPath<AnimationClip>(final);
 }
 static void Weapons(Material material){
  string[] names={"Runeblade","FrostCrystal","EmberSeed","ArcaneCore","ExperienceGem"};
  foreach(var name in names){var go=new GameObject(name);var m=new Writer();Color gold=C("DDBB76"),blue=C("97DDE8");
   if(name=="Runeblade"){
    m.Leaf(null,new Vector3(0,0,0),new Vector3(0,.63f,0),.08f,blue);m.Leaf(null,new Vector3(0,.015f,.008f),new Vector3(0,.61f,.008f),.018f,C("F3F2D4"));m.Tube(null,new[]{new Vector3(-.14f,.02f,0),new Vector3(0,-.01f,0),new Vector3(.14f,.02f,0)},.022f,.022f,gold);m.Tube(null,new[]{new Vector3(0,-.03f,0),new Vector3(0,-.19f,0)},.028f,.025f,C("536389"));m.Gem(null,new Vector3(0,-.21f,0),new Vector3(.04f,.05f,.025f),gold);for(int i=0;i<3;i++)m.Gem(null,new Vector3(0,.12f+i*.1f,.018f),new Vector3(.012f,.019f,.004f),gold);
   }else if(name=="FrostCrystal"){m.Gem(null,new Vector3(0,.18f,0),new Vector3(.11f,.42f,.085f),blue);m.Gem(null,new Vector3(-.08f,-.02f,0),new Vector3(.055f,.18f,.05f),C("D8F2EA"));m.Gem(null,new Vector3(.075f,-.04f,0),new Vector3(.05f,.16f,.05f),C("769FCD"));}
   else if(name=="EmberSeed"){m.Blob(null,Vector3.zero,Vector3.one*.18f,C("654B69"),.7f);for(int i=0;i<11;i++){float a=i*2.4f;var pos=new Vector3(Mathf.Cos(a),Mathf.Sin(a*.7f),Mathf.Sin(a))*.14f;m.Gem(null,pos,new Vector3(.045f,.09f,.04f),i%2==0?C("F5BB64"):C("D97A64"));}}
   else if(name=="ExperienceGem"){m.Gem(null,Vector3.zero,new Vector3(.055f,.095f,.055f),C("78D8AB"));m.Gem(null,new Vector3(0,.01f,.002f),new Vector3(.025f,.04f,.06f),C("C2F0C7"));}
   else{m.Blob(null,Vector3.zero,Vector3.one*.13f,C("695B9C"));for(int ring=0;ring<2;ring++){var points=new Vector3[33];for(int i=0;i<points.Length;i++){float a=i/32f*Mathf.PI*2;points[i]=ring==0?new Vector3(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.14f,Mathf.Sin(a)*.09f):new Vector3(Mathf.Sin(a)*.09f,Mathf.Cos(a)*.22f,Mathf.Sin(a)*.18f);}m.Tube(null,points,.009f,.009f,gold);}for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;m.Gem(null,new Vector3(Mathf.Cos(a)*.21f,.03f,Mathf.Sin(a)*.2f),Vector3.one*.035f,blue);}}
   var mesh=m.Finish(name);Save(mesh,Root+"/Weapons/"+name+".asset");go.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Weapons/"+name+".asset");go.AddComponent<MeshRenderer>().sharedMaterial=material;PrefabUtility.SaveAsPrefabAsset(go,Root+"/Weapons/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);
  }
 }
 static void HandWeapons(Material material){foreach(var definition in SurvivorArmory.Weapons){var go=new GameObject(definition.model);var m=new Writer();var gold=C("DDBB76");var color=definition.color;
  bool gun=definition.kind==HandWeaponKind.ArcPistol||definition.kind==HandWeaponKind.Scattergun;
  if(!gun){m.Tube(null,new[]{new Vector3(0,-.17f,0),new Vector3(0,.25f,0)},.024f,.018f,C("4F486E"));for(int i=0;i<5;i++)m.Tube(null,new[]{new Vector3(-.025f,-.13f+i*.037f,0),new Vector3(.025f,-.115f+i*.037f,0)},.008f,.008f,gold);}
  if(gun){bool scatter=definition.kind==HandWeaponKind.Scattergun;float barrel=scatter?.36f:.28f;
   m.Blob(null,new Vector3(0,.035f,.08f),new Vector3(scatter?.055f:.043f,.045f,.13f),C("42495F"),.25f);
   m.Tube(null,new[]{new Vector3(0,.035f,.13f),new Vector3(0,.035f,barrel)},scatter?.036f:.025f,scatter?.036f:.025f,gold);
   m.Tube(null,new[]{new Vector3(0,.035f,barrel-.012f),new Vector3(0,.035f,barrel+.002f)},scatter?.026f:.017f,scatter?.026f:.017f,C("182338"));
   m.Blob(null,new Vector3(0,-.073f,-.018f),new Vector3(.032f,.105f,.042f),C("655B84"),.3f);
   m.Tube(null,new[]{new Vector3(-.034f,-.12f,-.03f),new Vector3(-.034f,-.04f,0)},.007f,.007f,gold);
   m.Tube(null,new[]{new Vector3(0,-.01f,.04f),new Vector3(0,-.075f,.04f),new Vector3(0,-.075f,.11f),new Vector3(0,-.01f,.12f)},.009f,.009f,gold);
   m.Gem(null,new Vector3(0,.09f,.05f),new Vector3(.034f,.04f,.05f),color);
   m.Gem(null,new Vector3(0,.082f,barrel-.025f),Vector3.one*.012f,color);
   if(scatter){m.Blob(null,new Vector3(0,-.015f,.2f),new Vector3(.047f,.03f,.065f),C("655B84"),.25f);m.Tube(null,new[]{new Vector3(-.047f,.04f,.015f),new Vector3(-.047f,.04f,.15f)},.012f,.012f,color);}
  }else
  if(definition.kind==HandWeaponKind.Runeblade){m.Leaf(null,new Vector3(0,.07f,0),new Vector3(0,.83f,0),.065f,color);m.Leaf(null,new Vector3(0,.08f,.009f),new Vector3(0,.81f,.009f),.016f,C("F7F3DD"));m.Tube(null,new[]{new Vector3(-.14f,.05f,0),new Vector3(0,.025f,0),new Vector3(.14f,.05f,0)},.02f,.02f,gold);}else{m.Gem(null,new Vector3(0,.32f,0),new Vector3(.08f,.15f,.08f),color);for(int side=-1;side<=1;side+=2)m.Leaf(null,new Vector3(side*.025f,.15f,0),new Vector3(side*.12f,.43f,0),.035f,gold);if(definition.kind==HandWeaponKind.EmberStaff)m.Blob(null,new Vector3(0,.32f,0),Vector3.one*.09f,color);if(definition.kind==HandWeaponKind.VoidScepter||definition.kind==HandWeaponKind.StormWand){var ring=new Vector3[33];for(int i=0;i<33;i++){float a=i*Mathf.PI*2/32;ring[i]=new Vector3(Mathf.Cos(a)*.14f,.32f+Mathf.Sin(a)*.14f,0);}m.Tube(null,ring,.012f,.012f,gold);}}
  var mesh=m.Finish(definition.model);Save(mesh,Root+"/Weapons/"+definition.model+".asset");go.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Weapons/"+definition.model+".asset");go.AddComponent<MeshRenderer>().sharedMaterial=material;PrefabUtility.SaveAsPrefabAsset(go,Root+"/Weapons/"+definition.model+".prefab");UnityEngine.Object.DestroyImmediate(go);
 }}
 static void ImportArt(){foreach(var name in new[]{"MenuIllustration","SkillAtlas"}){string path=Root+"/"+name+".png";if(!File.Exists(path))continue;AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.maxTextureSize=2048;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.wrapMode=TextureWrapMode.Clamp;imp.SaveAndReimport();}}

 // Paint and skin every generated vertex. Parts share one skinned renderer and
 // one material; there are no Unity primitive objects in the exported actors.
 sealed class Writer {
  readonly List<Vector3> v=new List<Vector3>();readonly List<Color> c=new List<Color>();readonly List<int> ix=new List<int>();readonly List<BoneWeight> bw=new List<BoneWeight>();readonly Transform[] bones;
  public Writer(Transform[] bones=null){this.bones=bones;}
  int Vertex(Transform bone,Vector3 p,Color color){int b=bone&&bones!=null?Array.IndexOf(bones,bone):0;if(b<0)b=0;v.Add(bone?bone.localToWorldMatrix.MultiplyPoint3x4(p):p);c.Add(color);bw.Add(new BoneWeight{boneIndex0=b,weight0=1});return v.Count-1;}
  void Tri(int a,int b,int d){if(a>=v.Count||b>=v.Count||d>=v.Count)throw new Exception("Triangle outside vertices: "+a+","+b+","+d+" / "+v.Count);ix.Add(a);ix.Add(b);ix.Add(d);} void Back(int a,int b,int d){int start=v.Count;foreach(int i in new[]{a,d,b}){v.Add(v[i]);c.Add(c[i]);bw.Add(bw[i]);}Tri(start,start+1,start+2);}
  public void Blob(Transform bone,Vector3 p,Vector3 size,Color color,float power=1){int start=v.Count;const int rings=12,segs=20;for(int y=0;y<=rings;y++){float a=y*Mathf.PI/rings;for(int x=0;x<=segs;x++){float b=x*Mathf.PI*2/segs;Vector3 n=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));for(int k=0;k<3;k++)n[k]=Mathf.Sign(n[k])*Mathf.Pow(Mathf.Abs(n[k]),power);float shade=.93f+.07f*(n.y*.5f+.5f);Vertex(bone,p+Vector3.Scale(n,size),color*new Color(shade,shade,shade,1));}}for(int y=0;y<rings;y++)for(int x=0;x<segs;x++){int a=start+y*(segs+1)+x,b=a+segs+1;Tri(a,a+1,b);Tri(a+1,b+1,b);}}
  public void Tube(Transform bone,Vector3[] points,float r0,float r1,Color color){int start=v.Count;const int sides=10;for(int i=0;i<points.Length;i++){var dir=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)]).normalized;var right=Vector3.Cross(dir,Mathf.Abs(dir.y)>.95f?Vector3.forward:Vector3.up).normalized;var up=Vector3.Cross(dir,right);float r=Mathf.Lerp(r0,r1,i/(float)(points.Length-1));for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides;Vertex(bone,points[i]+(right*Mathf.Cos(a)+up*Mathf.Sin(a))*r,color);}}for(int i=0;i<points.Length-1;i++)for(int j=0;j<sides;j++){int a=start+i*(sides+1)+j,b=a+sides+1;Tri(a,a+1,b);Tri(a+1,b+1,b);}int capA=Vertex(bone,points[0],color),capB=Vertex(bone,points[points.Length-1],color);for(int j=0;j<sides;j++){Tri(capA,start+j,start+j+1);int a=start+(points.Length-1)*(sides+1)+j;Tri(capB,a+1,a);}}
  public void Leaf(Transform bone,Vector3 from,Vector3 to,float width,Color color){var direction=(to-from).normalized;var side=Vector3.Cross(direction,Vector3.forward).normalized;if(side.sqrMagnitude<.1f)side=Vector3.right;int start=v.Count;for(int i=0;i<=8;i++){float t=i/8f;var p=Vector3.Lerp(from,to,t);float w=Mathf.Sin(t*Mathf.PI)*width;float z=Mathf.Sin(t*Mathf.PI)*width*.32f;Vertex(bone,p-side*w,color*.86f);Vertex(bone,p+Vector3.forward*z,color);Vertex(bone,p+side*w,color*.93f);}for(int i=0;i<8;i++){int a=start+i*3;Tri(a,a+3,a+1);Tri(a+1,a+3,a+4);Tri(a+1,a+4,a+2);Tri(a+2,a+4,a+5);Back(a,a+3,a+1);Back(a+1,a+3,a+4);Back(a+1,a+4,a+2);Back(a+2,a+4,a+5);}}
  public void Gem(Transform bone,Vector3 p,Vector3 size,Color color){var pts=new[]{Vector3.up,Vector3.down,Vector3.right*.7f,Vector3.forward*.7f,Vector3.left*.7f,Vector3.back*.7f};for(int i=0;i<4;i++){int j=2+i,k=2+(i+1)%4;int a=Vertex(bone,p+Vector3.Scale(pts[0],size),color),b=Vertex(bone,p+Vector3.Scale(pts[k],size),color*.86f),d=Vertex(bone,p+Vector3.Scale(pts[j],size),color);Tri(a,b,d);a=Vertex(bone,p+Vector3.Scale(pts[1],size),color*.78f);b=Vertex(bone,p+Vector3.Scale(pts[j],size),color);d=Vertex(bone,p+Vector3.Scale(pts[k],size),color*.86f);Tri(a,b,d);}}
  public void Wing(Transform bone,int side,Color dark,Color pale){var points=new[]{Vector3.zero,new Vector3(side*.14f,.23f,-.025f),new Vector3(side*.36f,.19f,-.045f),new Vector3(side*.29f,-.02f,-.04f),new Vector3(side*.17f,-.095f,-.02f),new Vector3(side*.06f,-.13f,0)};var center=new Vector3(side*.12f,.025f,0);for(int i=0;i<points.Length;i++){int a=Vertex(bone,center,pale),b=Vertex(bone,points[i],dark),d=Vertex(bone,points[(i+1)%points.Length],dark);Tri(a,b,d);Back(a,b,d);}for(int i=1;i<points.Length;i++)Tube(bone,new[]{points[0],points[i]},.006f,.003f,C("D6AD61"));}
  public Mesh Finish(string name){var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(ix,0);if(bones!=null){mesh.boneWeights=bw.ToArray();var bind=new Matrix4x4[bones.Length];for(int i=0;i<bones.Length;i++)bind[i]=bones[i].worldToLocalMatrix;mesh.bindposes=bind;}mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
 }
}
}

