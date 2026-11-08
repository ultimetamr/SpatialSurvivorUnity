using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpatialSurvivor.Editor {
public static partial class SurvivorArtBuilder {
 public const string Root="Assets/Resources/Art";
 public static readonly string[] Creatures={"Mossling","EmberImp","CrystalGuardian","MoonWisp","ElderBloom","SkyMoth","StormDrake","Sporecap","RuneShaman","IronBeetle","FrostWitch","ThornSentinel","MoonRegent"};
 static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
 [MenuItem("SpatialSurvivor/Art/Rebuild Storybook Assets")]
 public static void Build(){
  Directory.CreateDirectory(Root+"/Creatures");Directory.CreateDirectory(Root+"/Weapons");Directory.CreateDirectory(Root+"/Animations");
  var material=new Material(Shader.Find("Survivor/Storybook")){name="Storybook Vertex Paint"};Save(material,Root+"/Storybook.mat");material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Storybook.mat");
  for(int i=0;i<Creatures.Length;i++)Creature(i,material);
  Weapons(material);HandWeapons(material);ImportArt();AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Storybook art rebuilt: 13 articulated creatures, 78 clips and complete magic arsenal.");
 }
 static void Save(UnityEngine.Object asset,string path){var old=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);if(old){if(asset is Mesh source&&old is Mesh target){target.Clear();target.indexFormat=source.indexFormat;target.vertices=source.vertices;target.colors=source.colors;target.normals=source.normals;target.triangles=source.triangles;target.bindposes=source.bindposes;target.boneWeights=source.boneWeights;target.bounds=source.bounds;}else EditorUtility.CopySerialized(asset,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(asset);}else AssetDatabase.CreateAsset(asset,path);}
 static Transform Bone(string name,Transform parent,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
 static void Creature(int type,Material material){
  int variant=type;string modelName=Creatures[variant];type=variant==11?2:variant==12?3:type;
  var go=new GameObject(modelName);var rig=Bone("Rig",go.transform,Vector3.zero);
  var body=Bone("Body",rig,new Vector3(0,.4f,0));var head=Bone("Head",body,new Vector3(0,.29f,.015f));
  float shoulder=type==2||type==4?.29f:.2f;
  var la=Bone("ArmL",body,new Vector3(-shoulder,.05f,0));var ra=Bone("ArmR",body,new Vector3(shoulder,.05f,0));
  var lf=Bone("ForearmL",la,new Vector3(-.025f,-.14f,0));var rf=Bone("ForearmR",ra,new Vector3(.025f,-.14f,0));
  var ll=Bone("LegL",rig,new Vector3(-.115f,.22f,0));var rl=Bone("LegR",rig,new Vector3(.115f,.22f,0));
  var tail=Bone("Tail",body,new Vector3(0,-.05f,-.13f));var lw=Bone("WingL",body,new Vector3(-.16f,.15f,-.06f));var rw=Bone("WingR",body,new Vector3(.16f,.15f,-.06f));
  var kl=Bone("KneeL",ll,new Vector3(0,-.105f,0));var kr=Bone("KneeR",rl,new Vector3(0,-.105f,0));
  var jaw=Bone("Jaw",head,new Vector3(0,-.075f,.105f));var crest=Bone("Crest",head,new Vector3(0,.14f,-.02f));
  var tip=Bone("TailTip",tail,new Vector3(0,-.08f,-.18f));var mantle=Bone("Mantle",body,new Vector3(0,.13f,-.125f));
  var lidL=Bone("LidL",head,new Vector3(-.087f,.098f,.207f));var lidR=Bone("LidR",head,new Vector3(.087f,.098f,.207f));
  var bones=new[]{rig,body,head,la,ra,lf,rf,ll,rl,tail,lw,rw,kl,kr,jaw,crest,tip,mantle,lidL,lidR};var m=new Writer(bones);
  Color skin=type==0?C("88A964"):type==1?C("D87955"):type==2?C("627E99"):type==3?C("A49ACD"):type==5?C("62BFC5"):type==6?C("8474C6"):C("70644B");
  Color pale=type==0?C("ECE0AE"):type==1?C("F4B88A"):type==2?C("99B6C4"):type==3?C("E5DCF3"):C("B4A078");
  Color dark=type==0?C("496951"):type==1?C("843F4A"):type==2?C("42506C"):type==3?C("655A99"):C("385F54");
  Color gold=C("D6AD61"),eye=type==2||type==4?C("63E8ED"):C("CB8733");
  bool bulky=type==2||type==4||type==9;
  if(type==7){skin=C("CAA477");pale=C("FFF0C5");dark=C("805174");}if(type==8){skin=C("739C8A");pale=C("DAD5A2");dark=C("4C526F");}if(type==9){skin=C("506A80");pale=C("91A0B2");dark=C("293F59");}if(type==10){skin=C("A7CADD");pale=C("E5F4F0");dark=C("626DA5");}
  var torsoSize=new Vector3(bulky?.29f:.185f,.25f,bulky?.19f:.14f);
  if(type==5)torsoSize=new Vector3(.14f,.26f,.12f);if(type==6)torsoSize=new Vector3(.18f,.25f,.19f);
  if(type==7)torsoSize=new Vector3(.16f,.22f,.14f);if(type==9)torsoSize=new Vector3(.3f,.22f,.22f);
  if(variant==11){skin=C("587E70");pale=C("A9BA8E");dark=C("304F51");eye=C("D9E9A6");}
  m.Blob(body,Vector3.zero,torsoSize,skin,bulky?.72f:1);
  m.Bib(body,torsoSize,new Vector2(bulky?.18f:type==5?.095f:.13f,.17f),-.04f,pale,bulky?.72f:1);
  m.Blob(head,Vector3.zero,new Vector3(type==5?.2f:type==9?.21f:.235f,type==9?.17f:.205f,type==6?.22f:.175f),skin,bulky?.72f:1);
  m.Blob(head,new Vector3(0,-.06f,.135f),new Vector3(.18f,.125f,.063f),pale);
  foreach(int side in new[]{-1,1}){
   float x=side*.087f;
   m.Blob(head,new Vector3(x,.018f,.153f),new Vector3(.066f,.077f,.035f),C("FFF5DD"));
   m.Blob(head,new Vector3(x,.012f,.184f),new Vector3(.043f,.052f,.016f),eye);
   m.Blob(head,new Vector3(x,.01f,.197f),new Vector3(.023f,.035f,.009f),C("302C3A"));
   m.Blob(head,new Vector3(x-.012f,.029f,.205f),new Vector3(.012f,.014f,.004f),Color.white);
   m.Tube(head,new[]{new Vector3(x-side*.048f,.104f,.151f),new Vector3(x,.12f,.168f),new Vector3(x+side*.044f,.105f,.148f)},.013f,.009f,dark);
   m.Blob(head,new Vector3(side*.155f,-.06f,.16f),new Vector3(.035f,.018f,.01f),type==2?C("90C8CE"):C("DB9C85"));
   Transform arm=side<0?la:ra,fore=side<0?lf:rf,leg=side<0?ll:rl,knee=side<0?kl:kr;
   m.Blob(arm,new Vector3(side*.012f,-.06f,0),new Vector3(bulky?.095f:.058f,.11f,.065f),skin,bulky?.7f:1);
   m.Blob(fore,new Vector3(side*.009f,-.05f,.01f),new Vector3(bulky?.105f:.067f,.1f,.075f),skin,bulky?.65f:1,true);
   m.Blob(fore,new Vector3(side*.005f,-.11f,.03f),new Vector3(bulky?.09f:.06f,.045f,.07f),pale);
   for(int finger=0;finger<3;finger++)m.Blob(fore,new Vector3(side*(-.036f+finger*.027f),-.125f,.063f),new Vector3(.019f,.027f,.03f),pale);
   m.Blob(leg,new Vector3(0,-.045f,0),new Vector3(bulky?.092f:.07f,.09f,.075f),skin);
   m.Blob(knee,new Vector3(0,-.025f,0),new Vector3(bulky?.085f:.063f,.08f,.066f),skin,1,true);
   m.Blob(knee,new Vector3(0,-.065f,.04f),new Vector3(bulky?.105f:.084f,.055f,.118f),dark,bulky?.65f:1);
  }
  m.Blob(head,new Vector3(0,-.043f,.205f),new Vector3(.028f,.019f,.017f),dark);
  m.Blob(jaw,new Vector3(0,-.016f,.062f),new Vector3(.07f,.035f,.038f),pale);
  m.Tube(jaw,new[]{new Vector3(-.045f,-.012f,.081f),new Vector3(0,-.028f,.097f),new Vector3(.045f,-.012f,.081f)},.005f,.005f,C("664F4D"));
  if(type==0||type==4){
   for(int i=0;i<9;i++){float a=(i-4)*.42f;var start=new Vector3(Mathf.Sin(a)*.12f,.14f,Mathf.Cos(a)*.08f-.04f);m.Leaf(crest,start-crest.localPosition,start-crest.localPosition+new Vector3(Mathf.Sin(a)*.19f,.19f-i%2*.06f,-.045f),.07f,i%2==0?C("8EA756"):C("4F7954"));}
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
  if(type==3||type==5||type==6){
   for(int side=-1;side<=1;side+=2){var wing=side<0?lw:rw;if(type!=5)m.Wing(wing,side,C("9180BC"),C("D4C9EC"));m.Leaf(head,new Vector3(side*.17f,.04f,0),new Vector3(side*.31f,.13f,0),.065f,skin);}
   m.Gem(head,new Vector3(0,.13f,.14f),new Vector3(.045f,.06f,.018f),gold);m.Tube(head,new[]{new Vector3(-.19f,.09f,.04f),new Vector3(0,.14f,.15f),new Vector3(.19f,.09f,.04f)},.014f,.014f,gold);
  }
  if(type==5||type==6){for(int side=-1;side<=1;side+=2){var wing=side<0?lw:rw;if(type==5){m.Leaf(wing,Vector3.zero,new Vector3(side*.55f,.32f,.04f),.2f,C("92E0DC"));m.Leaf(wing,Vector3.zero,new Vector3(side*.42f,-.22f,.02f),.15f,C("4C93AB"));m.Gem(wing,new Vector3(side*.29f,.15f,.065f),new Vector3(.05f,.06f,.015f),gold);m.Tube(head,new[]{new Vector3(side*.08f,.15f,0),new Vector3(side*.15f,.35f,.025f)},.012f,.006f,gold);m.Blob(head,new Vector3(side*.15f,.35f,.025f),Vector3.one*.025f,pale);}else{m.Gem(head,new Vector3(side*.13f,.17f,0),new Vector3(.045f,.19f,.04f),C("EACD8B"));}}if(type==6)m.Tube(tail,new[]{Vector3.zero,new Vector3(0,-.1f,-.3f),new Vector3(.14f,-.04f,-.45f)},.035f,.004f,skin);}
  if(type==7){
   m.Blob(head,new Vector3(0,.22f,-.015f),new Vector3(.4f,.18f,.31f),C("A65079"));
   m.Blob(head,new Vector3(0,.14f,0),new Vector3(.38f,.035f,.29f),C("F4D3AC"));
   for(int i=0;i<9;i++){float a=i*2.4f;m.Blob(head,new Vector3(Mathf.Cos(a)*.26f,.30f+(i%2)*.035f,Mathf.Sin(a)*.18f),new Vector3(.045f,.025f,.038f),pale);}
   for(int side=-1;side<=1;side+=2)m.Blob(side<0?lf:rf,new Vector3(side*.015f,-.06f,.1f),Vector3.one*.075f,C("BEB870"));
  }
  if(type==8){
   for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;m.Leaf(body,new Vector3(Mathf.Sin(a)*.12f,.15f,Mathf.Cos(a)*.12f),new Vector3(Mathf.Sin(a)*.31f,-.24f,Mathf.Cos(a)*.23f),.12f,i%2==0?dark:skin);}
   m.Tube(rf,new[]{new Vector3(.08f,-.30f,.06f),new Vector3(.08f,.46f,.06f)},.025f,.018f,C("806A58"));
   m.Gem(rf,new Vector3(.08f,.5f,.06f),new Vector3(.09f,.13f,.05f),C("B9E99D"));
   for(int side=-1;side<=1;side+=2)m.Leaf(head,new Vector3(side*.12f,.11f,0),new Vector3(side*.25f,.35f,-.06f),.08f,C("D8C984"));
   m.Gem(head,new Vector3(0,.03f,.197f),new Vector3(.03f,.055f,.017f),gold);
  }
  if(type==9){
   m.Blob(body,new Vector3(0,.04f,-.15f),new Vector3(.35f,.33f,.22f),dark,.6f);
   for(int side=-1;side<=1;side+=2){m.Blob(body,new Vector3(side*.16f,.04f,-.23f),new Vector3(.16f,.3f,.1f),skin,.55f);for(int i=0;i<3;i++)m.Tube(body,new[]{new Vector3(side*.22f,-.03f-i*.06f,-.08f),new Vector3(side*.4f,-.15f-i*.045f,-.12f)},.033f,.012f,dark);}
   m.Tube(head,new[]{new Vector3(0,.11f,.1f),new Vector3(0,.3f,.2f),new Vector3(0,.43f,.24f)},.075f,.005f,gold);
  }
  if(type==10){
   for(int i=0;i<5;i++)m.Gem(head,new Vector3((i-2)*.073f,.17f,-.015f),new Vector3(.04f,.18f-Mathf.Abs(i-2)*.025f,.04f),C("BCECF4"));
   for(int i=0;i<8;i++){float a=i*Mathf.PI*2/8;m.Leaf(body,new Vector3(Mathf.Sin(a)*.12f,.08f,Mathf.Cos(a)*.12f),new Vector3(Mathf.Sin(a)*.28f,-.3f,Mathf.Cos(a)*.22f),.09f,i%2==0?dark:C("8FBCCA"));}
   m.Tube(lf,new[]{new Vector3(-.04f,-.27f,.02f),new Vector3(-.04f,.32f,.02f)},.018f,.014f,gold);
   m.Gem(lf,new Vector3(-.04f,.37f,.02f),new Vector3(.055f,.12f,.04f),C("DDF8F6"));
  }
  if(variant==11){
   // A shield-bearing bramble knight, with a broad asymmetric silhouette.
   m.Blob(lf,new Vector3(-.12f,-.035f,.1f),new Vector3(.23f,.32f,.08f),C("456B59"),.5f);
   for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;m.Gem(lf,new Vector3(-.12f+Mathf.Cos(a)*.2f,-.035f+Mathf.Sin(a)*.28f,.14f),new Vector3(.04f,.08f,.035f),C("B7C486"));}
   m.Gem(lf,new Vector3(-.12f,-.03f,.2f),new Vector3(.09f,.13f,.03f),C("E1CF83"));
   m.Tube(rf,new[]{new Vector3(.035f,-.3f,.025f),new Vector3(.035f,.45f,.025f)},.035f,.018f,C("75664C"));
   m.Leaf(rf,new Vector3(.035f,.35f,.025f),new Vector3(.035f,.78f,.025f),.095f,C("C3D6A2"));
   for(int side=-1;side<=1;side+=2)m.Tube(head,new[]{new Vector3(side*.16f,.12f,-.04f),new Vector3(side*.3f,.3f,-.08f),new Vector3(side*.35f,.42f,-.06f)},.045f,.005f,C("6E8356"));
  }
  if(variant==12){
   // A wide moon crown and layered ceremonial skirt distinguish the second boss.
   var moon=new Vector3[25];for(int i=0;i<moon.Length;i++){float a=(-70+i*280f/24)*Mathf.Deg2Rad;moon[i]=new Vector3(Mathf.Cos(a)*.38f,.25f+Mathf.Sin(a)*.38f,-.10f);}
   m.Tube(head,moon,.048f,.014f,C("E2D3A0"));
   for(int i=0;i<10;i++){float a=i*Mathf.PI*2/10;m.Leaf(body,new Vector3(Mathf.Cos(a)*.13f,.08f,Mathf.Sin(a)*.13f),new Vector3(Mathf.Cos(a)*.39f,-.37f,Mathf.Sin(a)*.29f),.14f,i%2==0?C("756497"):C("C1B4DB"));}
   m.Tube(rf,new[]{new Vector3(.05f,-.3f,.02f),new Vector3(.05f,.55f,.02f)},.021f,.015f,C("D7C997"));m.Gem(rf,new Vector3(.05f,.6f,.02f),new Vector3(.1f,.17f,.06f),C("D3E7F6"));
  }
  PolishCreature(m,bones,variant,skin,pale,dark,gold);
  var mesh=m.Finish(modelName+" Painted Skin");Save(mesh,Root+"/Creatures/"+modelName+".asset");mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Creatures/"+modelName+".asset");
  var skinRenderer=go.AddComponent<SkinnedMeshRenderer>();skinRenderer.sharedMesh=mesh;skinRenderer.bones=bones;skinRenderer.rootBone=rig;skinRenderer.sharedMaterial=material;skinRenderer.localBounds=new Bounds(new Vector3(0,.55f,0),new Vector3(2.8f,2.8f,2.8f));skinRenderer.updateWhenOffscreen=false;
  var animator=go.AddComponent<SurvivorCreatureAnimator>();AnimationClip[] clips=new AnimationClip[6];for(int i=0;i<clips.Length;i++)clips[i]=Clip(go,bones,variant,(CreatureMotion)i);
  animator.idle=clips[0];animator.walk=clips[1];animator.attack=clips[2];animator.hit=clips[3];animator.spawn=clips[4];animator.death=clips[5];
  PrefabUtility.SaveAsPrefabAsset(go,Root+"/Creatures/"+modelName+".prefab");UnityEngine.Object.DestroyImmediate(go);
 }
 static AnimationClip Clip(GameObject root,Transform[] bones,int type,CreatureMotion state){
  float length=state==CreatureMotion.Walk?.72f:state==CreatureMotion.Attack?.55f:state==CreatureMotion.Hit?.22f:state==CreatureMotion.Spawn?.6f:state==CreatureMotion.Death?.45f:2.4f;
  var clip=new AnimationClip{name=Creatures[type]+"_"+state,frameRate=30,legacy=true,wrapMode=state==CreatureMotion.Idle||state==CreatureMotion.Walk?WrapMode.Loop:WrapMode.ClampForever};
  for(int b=0;b<bones.Length;b++){
   var tx=new AnimationCurve();var ty=new AnimationCurve();var tz=new AnimationCurve();var rx=new AnimationCurve();var ry=new AnimationCurve();var rz=new AnimationCurve();var sx=new AnimationCurve();var sy=new AnimationCurve();var sz=new AnimationCurve();
   for(int f=0;f<=30;f++){float t=f/30f;var pos=bones[b].localPosition;var rot=Vector3.zero;var scale=Vector3.one;
    EvaluateCreaturePose(type,state,b,t,ref pos,ref rot,ref scale);
    float time=t*length;tx.AddKey(time,pos.x);ty.AddKey(time,pos.y);tz.AddKey(time,pos.z);rx.AddKey(time,rot.x);ry.AddKey(time,rot.y);rz.AddKey(time,rot.z);sx.AddKey(time,scale.x);sy.AddKey(time,scale.y);sz.AddKey(time,scale.z);
   }
   string path=AnimationUtility.CalculateTransformPath(bones[b],root.transform);MotionCurve(clip,path,"localPosition.x",tx);MotionCurve(clip,path,"localPosition.y",ty);MotionCurve(clip,path,"localPosition.z",tz);MotionCurve(clip,path,"localEulerAnglesRaw.x",rx);MotionCurve(clip,path,"localEulerAnglesRaw.y",ry);MotionCurve(clip,path,"localEulerAnglesRaw.z",rz);MotionCurve(clip,path,"localScale.x",sx);MotionCurve(clip,path,"localScale.y",sy);MotionCurve(clip,path,"localScale.z",sz);
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
   var mesh=m.Finish(name);if(name=="Runeblade")CanonicalBlade(go,mesh,false);Save(mesh,Root+"/Weapons/"+name+".asset");go.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Weapons/"+name+".asset");go.AddComponent<MeshRenderer>().sharedMaterial=material;PrefabUtility.SaveAsPrefabAsset(go,Root+"/Weapons/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);
  }
 }
 static void HandWeapons(Material material){foreach(var definition in SurvivorArmory.Weapons){var go=new GameObject(definition.model);var m=new Writer();var gold=C("DDBB76");var color=definition.color;
  bool gun=definition.kind==HandWeaponKind.ArcPistol||definition.kind==HandWeaponKind.Scattergun;
  if(!gun){bool sword=definition.kind==HandWeaponKind.Runeblade;m.Tube(null,new[]{new Vector3(0,sword?-.11f:-.17f,0),new Vector3(0,sword?.1f:.25f,0)},.024f,.018f,C("4F486E"));for(int i=0;i<5;i++)m.Tube(null,new[]{new Vector3(-.025f,-.13f+i*.037f,0),new Vector3(.025f,-.115f+i*.037f,0)},.008f,.008f,gold);}
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
  if(definition.kind==HandWeaponKind.Runeblade){m.Leaf(null,new Vector3(0,.14f,0),new Vector3(0,.83f,0),.065f,color);m.Leaf(null,new Vector3(0,.15f,.009f),new Vector3(0,.81f,.009f),.016f,C("F7F3DD"));m.Tube(null,new[]{new Vector3(-.14f,.14f,0),new Vector3(0,.12f,0),new Vector3(.14f,.14f,0)},.02f,.02f,gold);}else{m.Gem(null,new Vector3(0,.32f,0),new Vector3(.08f,.15f,.08f),color);for(int side=-1;side<=1;side+=2)m.Leaf(null,new Vector3(side*.025f,.15f,0),new Vector3(side*.12f,.43f,0),.035f,gold);if(definition.kind==HandWeaponKind.EmberStaff)m.Blob(null,new Vector3(0,.32f,0),Vector3.one*.09f,color);if(definition.kind==HandWeaponKind.VoidScepter||definition.kind==HandWeaponKind.StormWand){var ring=new Vector3[33];for(int i=0;i<33;i++){float a=i*Mathf.PI*2/32;ring[i]=new Vector3(Mathf.Cos(a)*.14f,.32f+Mathf.Sin(a)*.14f,0);}m.Tube(null,ring,.012f,.012f,gold);}}
  var mesh=m.Finish(definition.model);if(definition.kind==HandWeaponKind.Runeblade)CanonicalBlade(go,mesh,true);Save(mesh,Root+"/Weapons/"+definition.model+".asset");go.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Weapons/"+definition.model+".asset");go.AddComponent<MeshRenderer>().sharedMaterial=material;PrefabUtility.SaveAsPrefabAsset(go,Root+"/Weapons/"+definition.model+".prefab");UnityEngine.Object.DestroyImmediate(go);
 }}
 static void ImportArt(){foreach(var name in new[]{"MenuIllustration","SkillAtlas"}){string path=Root+"/"+name+".png";if(!File.Exists(path))continue;AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.maxTextureSize=2048;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.wrapMode=TextureWrapMode.Clamp;imp.SaveAndReimport();}}

 // Paint and skin every generated vertex. Parts share one skinned renderer and
 // one material; there are no Unity primitive objects in the exported actors.
 sealed class Writer {
  readonly List<Vector3> v=new List<Vector3>();readonly List<Color> c=new List<Color>();readonly List<int> ix=new List<int>();readonly List<BoneWeight> bw=new List<BoneWeight>();readonly Transform[] bones;
  public Writer(Transform[] bones=null){this.bones=bones;}
  int Vertex(Transform bone,Vector3 p,Color color){int b=bone&&bones!=null?Array.IndexOf(bones,bone):0;if(b<0)b=0;v.Add(bone?bone.localToWorldMatrix.MultiplyPoint3x4(p):p);c.Add(color);bw.Add(new BoneWeight{boneIndex0=b,weight0=1});return v.Count-1;}
  void Tri(int a,int b,int d){if(a>=v.Count||b>=v.Count||d>=v.Count)throw new Exception("Triangle outside vertices: "+a+","+b+","+d+" / "+v.Count);ix.Add(a);ix.Add(b);ix.Add(d);} void Back(int a,int b,int d){int start=v.Count;foreach(int i in new[]{a,d,b}){v.Add(v[i]);c.Add(c[i]);bw.Add(bw[i]);}Tri(start,start+1,start+2);}
  public void Blob(Transform bone,Vector3 p,Vector3 size,Color color,float power=1,bool blendParent=false){int start=v.Count;int rings=size.magnitude<.105f?6:12,segs=size.magnitude<.105f?10:20;for(int y=0;y<=rings;y++){float a=y*Mathf.PI/rings;for(int x=0;x<=segs;x++){float b=x*Mathf.PI*2/segs;Vector3 n=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));for(int k=0;k<3;k++)n[k]=Mathf.Sign(n[k])*Mathf.Pow(Mathf.Abs(n[k]),power);float shade=.93f+.07f*(n.y*.5f+.5f);var local=p+Vector3.Scale(n,size);int index=Vertex(bone,local,color*new Color(shade,shade,shade,1));
    if(blendParent&&bones!=null&&bone&&bone.parent){int parent=Array.IndexOf(bones,bone.parent);if(parent>=0){float influence=Mathf.SmoothStep(0,.42f,Mathf.InverseLerp(-.055f,.055f,local.y));var weight=bw[index];weight.boneIndex1=parent;weight.weight1=influence;weight.weight0=1-influence;bw[index]=weight;}}}}for(int y=0;y<rings;y++)for(int x=0;x<segs;x++){int a=start+y*(segs+1)+x,b=a+segs+1;Tri(a,a+1,b);Tri(a+1,b+1,b);}}
  public void Bib(Transform bone,Vector3 torso,Vector2 size,float y,Color color,float power){
   const int rings=8,segments=24;int start=v.Count;float exponent=2/power;
   for(int ring=0;ring<=rings;ring++)for(int step=0;step<=segments;step++){
    float a=step*Mathf.PI*2/segments,r=ring/(float)rings;var p=new Vector3(Mathf.Cos(a)*size.x*r,y+Mathf.Sin(a)*size.y*r,0);
    float surface=Mathf.Max(.001f,1-Mathf.Pow(Mathf.Abs(p.x/torso.x),exponent)-Mathf.Pow(Mathf.Abs(p.y/torso.y),exponent));p.z=torso.z*Mathf.Pow(surface,1/exponent)+.009f;
    Vertex(bone,p,Color.Lerp(color*.86f,color,Mathf.Clamp01((p.y+.22f)/.4f)));
   }
   for(int ring=0;ring<rings;ring++)for(int step=0;step<segments;step++){int a=start+ring*(segments+1)+step,b=a+segments+1;Tri(a,b,b+1);Tri(a,b+1,a+1);}
  }
  public void Tube(Transform bone,Vector3[] points,float r0,float r1,Color color){int start=v.Count;const int sides=10;for(int i=0;i<points.Length;i++){var dir=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)]).normalized;var right=Vector3.Cross(dir,Mathf.Abs(dir.y)>.95f?Vector3.forward:Vector3.up).normalized;var up=Vector3.Cross(dir,right);float r=Mathf.Lerp(r0,r1,i/(float)(points.Length-1));for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides;Vertex(bone,points[i]+(right*Mathf.Cos(a)+up*Mathf.Sin(a))*r,color);}}for(int i=0;i<points.Length-1;i++)for(int j=0;j<sides;j++){int a=start+i*(sides+1)+j,b=a+sides+1;Tri(a,a+1,b);Tri(a+1,b+1,b);}int capA=Vertex(bone,points[0],color),capB=Vertex(bone,points[points.Length-1],color);for(int j=0;j<sides;j++){Tri(capA,start+j,start+j+1);int a=start+(points.Length-1)*(sides+1)+j;Tri(capB,a+1,a);}}
  public void Leaf(Transform bone,Vector3 from,Vector3 to,float width,Color color){var direction=(to-from).normalized;var side=Vector3.Cross(direction,Vector3.forward).normalized;if(side.sqrMagnitude<.1f)side=Vector3.right;int start=v.Count;for(int i=0;i<=8;i++){float t=i/8f;var p=Vector3.Lerp(from,to,t);float w=Mathf.Sin(t*Mathf.PI)*width;float z=Mathf.Sin(t*Mathf.PI)*width*.32f;Vertex(bone,p-side*w,color*.86f);Vertex(bone,p+Vector3.forward*z,color);Vertex(bone,p+side*w,color*.93f);}for(int i=0;i<8;i++){int a=start+i*3;Tri(a,a+3,a+1);Tri(a+1,a+3,a+4);Tri(a+1,a+4,a+2);Tri(a+2,a+4,a+5);Back(a,a+3,a+1);Back(a+1,a+3,a+4);Back(a+1,a+4,a+2);Back(a+2,a+4,a+5);}}
  public void Gem(Transform bone,Vector3 p,Vector3 size,Color color){var pts=new[]{Vector3.up,Vector3.down,Vector3.right*.7f,Vector3.forward*.7f,Vector3.left*.7f,Vector3.back*.7f};for(int i=0;i<4;i++){int j=2+i,k=2+(i+1)%4;int a=Vertex(bone,p+Vector3.Scale(pts[0],size),color),b=Vertex(bone,p+Vector3.Scale(pts[k],size),color*.86f),d=Vertex(bone,p+Vector3.Scale(pts[j],size),color);Tri(a,b,d);a=Vertex(bone,p+Vector3.Scale(pts[1],size),color*.78f);b=Vertex(bone,p+Vector3.Scale(pts[j],size),color);d=Vertex(bone,p+Vector3.Scale(pts[k],size),color*.86f);Tri(a,b,d);}}
  public void Wing(Transform bone,int side,Color dark,Color pale){var points=new[]{Vector3.zero,new Vector3(side*.14f,.23f,-.025f),new Vector3(side*.36f,.19f,-.045f),new Vector3(side*.29f,-.02f,-.04f),new Vector3(side*.17f,-.095f,-.02f),new Vector3(side*.06f,-.13f,0)};var center=new Vector3(side*.12f,.025f,0);for(int i=0;i<points.Length;i++){int a=Vertex(bone,center,pale),b=Vertex(bone,points[i],dark),d=Vertex(bone,points[(i+1)%points.Length],dark);Tri(a,b,d);Back(a,b,d);}for(int i=1;i<points.Length;i++)Tube(bone,new[]{points[0],points[i]},.006f,.003f,C("D6AD61"));}
  public Mesh Finish(string name){var mesh=new Mesh{name=name,indexFormat=v.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(ix,0);if(bones!=null){mesh.boneWeights=bw.ToArray();var bind=new Matrix4x4[bones.Length];for(int i=0;i<bones.Length;i++)bind[i]=bones[i].worldToLocalMatrix;mesh.bindposes=bind;}mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
 }
}
}

