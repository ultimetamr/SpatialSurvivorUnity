using UnityEngine;
using UnityEditor;

namespace SpatialSurvivor.Editor {
public static partial class SurvivorArtBuilder {
 static void MotionCurve(AnimationClip clip,string path,string property,AnimationCurve curve){
  var keys=curve.keys;bool constant=true;for(int i=1;i<keys.Length;i++)constant&=Mathf.Abs(keys[i].value-keys[0].value)<.000001f;
  if(constant)curve=new AnimationCurve(new Keyframe(keys[0].time,keys[0].value),new Keyframe(keys[keys.Length-1].time,keys[0].value));
  for(int i=0;i<curve.length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);}
  clip.SetCurve(path,typeof(Transform),property,curve);
 }
 // Accessories use existing skin batches. Flexible pieces have their own joints.
 static void PolishCreature(Writer m,Transform[] bones,int type,Color skin,Color pale,Color dark,Color gold){
  var body=bones[1];var head=bones[2];var crest=bones[15];var mantle=bones[17];
  var teal=C("93E6D3");var ivory=C("F6E9BB");
  for(int side=-1;side<=1;side+=2){
   var arm=bones[side<0?3:4];var fore=bones[side<0?5:6];var knee=bones[side<0?12:13];
   // A narrow lid sits above the eye when open and covers it during a blink.
   m.Blob(bones[side<0?18:19],Vector3.zero,new Vector3(.069f,.078f,.018f),skin);
   if(type==2||type==9||type==11){
    m.Blob(fore,new Vector3(side*.01f,-.027f,.059f),new Vector3(.085f,.078f,.029f),dark,.7f);
    m.Tube(fore,new[]{new Vector3(-.06f,.022f,.07f),new Vector3(0,.037f,.088f),new Vector3(.06f,.022f,.07f)},.009f,.009f,gold);
    m.Gem(knee,new Vector3(0,.005f,.069f),new Vector3(.05f,.068f,.028f),pale);
    m.Gem(arm,new Vector3(side*.05f,.07f,.104f),new Vector3(.028f,.041f,.019f),gold);
   }else{
    m.Tube(fore,new[]{new Vector3(-.055f,-.017f,.05f),new Vector3(0,-.025f,.077f),new Vector3(.055f,-.017f,.05f)},.009f,.009f,dark);
    if(type==1||type==6)for(int claw=0;claw<3;claw++)m.Gem(knee,new Vector3((claw-1)*.042f,-.075f,.136f),new Vector3(.018f,.017f,.038f),ivory);
   }
  }
  if(type==0||type==4){
   // Layered leaf mantle with painted veins and acorn clasps.
   for(int i=0;i<7;i++){
    float a=(i-3)*.39f;var from=new Vector3(Mathf.Sin(a)*.12f,-.02f,-Mathf.Cos(a)*.025f);
    var to=from+new Vector3(Mathf.Sin(a)*.2f,-.36f,-.06f);
    m.Leaf(mantle,from,to,.09f,i%2==0?C("597E59"):C("86A35D"));
    m.Tube(mantle,new[]{from,(from+to)*.5f+Vector3.back*.012f,to},.004f,.002f,C("B7C67E"));
   }
   for(int side=-1;side<=1;side+=2){
    m.Tube(body,new[]{new Vector3(side*.13f,-.19f,.105f),new Vector3(side*.19f,-.02f,.13f),new Vector3(side*.12f,.15f,.13f)},.011f,.006f,dark);
    m.Blob(body,new Vector3(side*.16f,-.14f,.132f),new Vector3(.033f,.047f,.025f),gold);
   }
   if(type==4){
    for(int i=0;i<5;i++){float x=(i-2)*.065f;m.Tube(body,new[]{new Vector3(x,.17f,.16f),new Vector3(x*.8f,.06f,.20f),new Vector3(x,-.19f,.17f)},.006f,.003f,C("A09565"));}
    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;m.Leaf(crest,new Vector3(0,.15f,.01f),new Vector3(Mathf.Cos(a)*.15f,.15f+Mathf.Sin(a)*.13f,.04f),.043f,i%2==0?C("DCA79B"):ivory);}
    m.Gem(crest,new Vector3(0,.15f,.065f),new Vector3(.04f,.052f,.02f),teal);
   }
  }
  if(type==1){
   m.Tube(body,new[]{new Vector3(-.16f,-.11f,.045f),new Vector3(0,-.14f,.15f),new Vector3(.16f,-.11f,.045f)},.025f,.025f,dark);
   m.Gem(body,new Vector3(.035f,-.13f,.176f),new Vector3(.035f,.04f,.015f),gold);
   for(int i=0;i<4;i++)m.Leaf(crest,new Vector3((i-1.5f)*.05f,.02f,-.025f),new Vector3((i-1.5f)*.08f,.21f+i%2*.04f,-.10f),.036f,i%2==0?C("E97E48"):C("FFD489"));
   m.Leaf(bones[16],Vector3.zero,new Vector3(.12f,.1f,-.03f),.058f,C("F9B751"));
   for(int side=-1;side<=1;side+=2)m.Gem(bones[14],new Vector3(side*.036f,.005f,.091f),new Vector3(.012f,.023f,.009f),ivory);
  }
  if(type==2||type==11){
   for(int i=0;i<3;i++){
    float y=.10f-i*.09f,w=.19f-i*.014f;
    m.Tube(body,new[]{new Vector3(-w,y,.13f),new Vector3(0,y-.03f,.199f),new Vector3(w,y,.13f)},.015f,.015f,dark);
    for(int side=-1;side<=1;side+=2)m.Gem(body,new Vector3(side*(w-.02f),y,.161f),new Vector3(.012f,.018f,.012f),gold);
   }
   if(type==11){
    m.Blob(bones[5],new Vector3(-.12f,-.035f,.173f),new Vector3(.175f,.255f,.018f),C("6C8864"),.65f);
    for(int side=-1;side<=1;side+=2)m.Tube(bones[5],new[]{new Vector3(-.12f,-.22f,.199f),new Vector3(-.12f+side*.11f,-.04f,.207f),new Vector3(-.12f,.16f,.199f)},.008f,.008f,gold);
   }
  }
  if(type==3||type==10||type==12){
   for(int i=0;i<7;i++){
    float a=(i-3)*.48f;var from=new Vector3(Mathf.Sin(a)*.115f,.01f,0);
    var to=new Vector3(Mathf.Sin(a)*.27f,-.44f,-.045f-Mathf.Cos(a)*.09f);
    m.Leaf(mantle,from,to,.095f,i%2==0?dark:C("A4A7D3"));
    m.Tube(mantle,new[]{from,to},.005f,.003f,ivory);
   }
   m.Tube(body,new[]{new Vector3(-.17f,.12f,.045f),new Vector3(0,.03f,.172f),new Vector3(.17f,.12f,.045f)},.012f,.012f,gold);
   for(int side=-1;side<=1;side+=2)m.Gem(head,new Vector3(side*.19f,-.1f,.04f),new Vector3(.019f,.055f,.018f),teal);
   if(type==12){
    for(int i=0;i<7;i++){float a=(35+i*40)*Mathf.Deg2Rad;m.Gem(head,new Vector3(Mathf.Cos(a)*.38f,.25f+Mathf.Sin(a)*.38f,-.06f),new Vector3(.025f,.045f,.025f),i%2==0?teal:ivory);}
    m.Tube(head,new[]{new Vector3(-.13f,.14f,.13f),new Vector3(0,.22f,.15f),new Vector3(.13f,.14f,.13f)},.012f,.01f,gold);
   }
  }
  if(type==5){
   for(int side=-1;side<=1;side+=2){var wing=bones[side<0?10:11];
    for(int i=0;i<4;i++){float y=.27f-i*.12f;var end=new Vector3(side*(i<2?.47f:.34f),y,.068f);m.Tube(wing,new[]{Vector3.zero,end},.006f,.002f,ivory);}
    m.Blob(wing,new Vector3(side*.31f,.16f,.063f),new Vector3(.07f,.085f,.012f),C("3E6D8C"));
    m.Blob(wing,new Vector3(side*.31f,.16f,.077f),new Vector3(.034f,.044f,.008f),gold);
   }
   for(int i=0;i<3;i++)m.Blob(body,new Vector3(0,.08f-i*.105f,.135f),new Vector3(.095f,.038f,.021f),i%2==0?ivory:teal);
   m.Leaf(crest,new Vector3(-.03f,0,0),new Vector3(-.12f,.21f,0),.028f,ivory);m.Leaf(crest,new Vector3(.03f,0,0),new Vector3(.12f,.21f,0),.028f,ivory);
  }
  if(type==6){
   for(int i=0;i<5;i++)m.Gem(body,new Vector3(0,.16f-i*.082f,-.19f),new Vector3(.045f,.055f,.053f),i%2==0?gold:C("B9ABD9"));
   m.Blob(head,new Vector3(0,-.045f,.205f),new Vector3(.13f,.075f,.064f),pale);
   for(int side=-1;side<=1;side+=2)m.Blob(head,new Vector3(side*.065f,-.024f,.26f),new Vector3(.017f,.012f,.006f),dark);
   m.Leaf(bones[16],Vector3.zero,new Vector3(.12f,.09f,-.17f),.085f,gold);
  }
  if(type==7){
   for(int i=0;i<13;i++){float a=i*Mathf.PI*2/13;var from=new Vector3(Mathf.Cos(a)*.15f,.13f,Mathf.Sin(a)*.11f);var to=new Vector3(Mathf.Cos(a)*.37f,.14f,Mathf.Sin(a)*.28f);m.Tube(head,new[]{from,to},.007f,.003f,C("BA8C92"));}
   for(int i=0;i<5;i++)m.Tube(body,new[]{new Vector3((i-2)*.038f,.11f,.13f),new Vector3((i-2)*.042f,-.17f,.128f)},.004f,.002f,C("A97D5E"));
   for(int side=-1;side<=1;side+=2)m.Leaf(mantle,new Vector3(side*.04f,0,0),new Vector3(side*.24f,-.24f,-.09f),.1f,C("A1A674"));
  }
  if(type==8){
   for(int side=-1;side<=1;side+=2){
    m.Tube(body,new[]{new Vector3(side*.16f,.13f,.09f),new Vector3(side*.09f,-.13f,.163f),new Vector3(side*.18f,-.21f,.12f)},.012f,.009f,gold);
    for(int i=0;i<3;i++)m.Gem(head,new Vector3(side*(.09f+i*.034f),-.03f,.184f-i*.018f),new Vector3(.012f,.018f,.009f),ivory);
   }
   for(int i=0;i<5;i++)m.Leaf(bones[14],new Vector3((i-2)*.027f,-.018f,.033f),new Vector3((i-2)*.018f,-.14f,.05f),.024f,pale);
   m.Tube(mantle,new[]{new Vector3(-.14f,0,-.015f),new Vector3(0,-.32f,-.1f),new Vector3(.14f,0,-.015f)},.015f,.015f,dark);
  }
  if(type==9){
   for(int i=0;i<4;i++)m.Tube(body,new[]{new Vector3(-.21f,.12f-i*.075f,.12f),new Vector3(0,.1f-i*.075f,.188f),new Vector3(.21f,.12f-i*.075f,.12f)},.017f,.017f,dark);
   m.Tube(body,new[]{new Vector3(0,.29f,-.28f),new Vector3(0,.04f,-.371f),new Vector3(0,-.23f,-.28f)},.014f,.014f,gold);
   for(int side=-1;side<=1;side+=2)m.Gem(head,new Vector3(side*.13f,-.12f,.17f),new Vector3(.045f,.09f,.04f),gold);
  }
  if(type==10){
   var staff=bones[5];var center=new Vector3(-.04f,.37f,.065f);
   for(int i=0;i<6;i++){float a=i*Mathf.PI/3;var end=center+new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*.095f;m.Tube(staff,new[]{center,end},.007f,.003f,ivory);m.Gem(staff,end,new Vector3(.015f,.02f,.01f),teal);}
  }
 }
}
}
