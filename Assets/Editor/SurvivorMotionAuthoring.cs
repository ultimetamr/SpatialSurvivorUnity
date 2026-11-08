using UnityEngine;

namespace SpatialSurvivor.Editor {
public static partial class SurvivorArtBuilder {
 static float Ease(float t,float start,float end)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,t));
 // Shared timing: anticipation 0-.36, impact .52, follow-through .68, settle 1.
 static void EvaluateCreaturePose(int type,CreatureMotion state,int b,float t,ref Vector3 pos,ref Vector3 rot,ref Vector3 scale){
  float cycle=t*Mathf.PI*2,s=Mathf.Sin(cycle),c=Mathf.Cos(cycle);
  bool heavy=type==2||type==4||type==9||type==11,fly=type==5||type==6;
  bool caster=type==3||type==8||type==10||type==12;
  int side=b==3||b==5||b==7||b==10||b==12?-1:1;
  if(b==18||b==19){
   float blink=(Ease(t,.68f,.705f)-Ease(t,.72f,.755f));
   if(state!=CreatureMotion.Idle)blink=state==CreatureMotion.Hit?Mathf.Sin(t*Mathf.PI)*.7f:state==CreatureMotion.Death?Ease(t,.05f,.5f):0;
   scale.y=Mathf.Lerp(.025f,1.03f,blink);pos.y-=.079f*blink;return;
  }
  if(state==CreatureMotion.Idle||state==CreatureMotion.Walk){
   bool walking=state==CreatureMotion.Walk;
   if(b==1){pos.y+=s*.008f;scale=new Vector3(1-s*.012f,1+s*.018f,1-s*.012f);rot.x=walking?(heavy?8:11):0;rot.z=walking?s*(heavy?3:6):0;rot.y=walking?s*(heavy?3:5):s*2;}
   if(b==2){rot.z=-s*(walking?3:2);rot.y=Mathf.Sin(cycle)*3;rot.x=walking?-4:Mathf.Sin(cycle)*2;}
   if(b==3||b==4){rot.z=side*(6+s*2);rot.x=walking?s*side*(heavy?19:32):Mathf.Sin(cycle+.4f)*3;}
   if(b==5||b==6)rot.x=-12-(walking?Mathf.Max(0,s*side)*18:0);
   if(b==7||b==8)rot.x=walking?-s*side*(heavy?22:34):0;
   if(b==12||b==13)rot.x=walking?Mathf.Max(0,s*side)*44:3;
   if(b==0&&walking)pos.y+=(1-Mathf.Cos(cycle*2))*(heavy?.009f:.015f);
   if(b==9)rot.y=Mathf.Sin(cycle-.5f)*(walking?22:10);
   if(b==16){rot.y=Mathf.Sin(cycle-1.1f)*(walking?27:14);rot.x=Mathf.Sin(cycle-.5f)*7;}
   if(b==15){rot.x=Mathf.Sin(cycle-.65f)*(walking?9:4);rot.z=-s*3;}
   if(b==17){rot.x=8+Mathf.Sin(cycle-.8f)*(walking?12:4);rot.z=-s*(walking?6:2);}
   if(b>=10&&b<=11){rot.y=side*(12+s*8);}
   if(b==14)rot.x=Mathf.Max(0,s)*3;
   if(fly){
    if(b==0)pos.y+=s*.03f;
    if(b==1)rot.x=walking?(type==6?18:10):3;
    if(b==10||b==11){rot.z=-side*Mathf.Sin(cycle*(type==5?4:2))*(type==5?36:26);rot.y=side*(12+Mathf.Cos(cycle*(type==5?4:2)) *7);}
    if(b==7||b==8)rot.x=-20;if(b==12||b==13)rot.x=42;
   }
   if(caster){
    if(b==0)pos.y+=.025f+s*.018f;
    if(b==3||b==4){rot.z=side*(14+s*3);rot.x=(walking?s*side*9:0)-12;}
    if(b==7||b==8)rot.x=walking?-s*side*14:-5;
   }
   if(type==7){if(b==0)pos.y+=walking?(1-c)*.025f:0;if(b==2){scale.y=1+s*.025f;rot.x=-s*5;}}
   if(type==11){if(b==3)rot.x=-25;if(b==4)rot.x=-18;}
  }
  if(state==CreatureMotion.Attack){
   float wind=Ease(t,0,.34f)*(1-Ease(t,.36f,.52f));
   float strike=Ease(t,.35f,.52f)*(1-Ease(t,.62f,1));
   float settle=Mathf.Sin(Mathf.InverseLerp(.56f,1,t)*Mathf.PI*2)*(1-Ease(t,.56f,1));
   if(b==0){pos.y-=wind*(heavy?.065f:.035f);pos.z+=strike*(heavy?.07f:.105f);}
   if(b==1){rot.x=-wind*14+strike*(heavy?32:21);rot.y=-wind*15+strike*22;scale.y=1-wind*.065f+strike*.035f;scale.x=1+wind*.035f-strike*.018f;}
   if(b==2){rot.x=wind*9-strike*12;rot.y=wind*8-strike*9;}
   if(b==3||b==4){rot.x=-wind*(heavy?120:50)-strike*(side<0?86:112);rot.z=side*(wind*30+strike*12);}
   if(b==5||b==6)rot.x=-wind*55-strike*16;
   if(b==7||b==8)rot.x=wind*18-strike*side*20;
   if(b==12||b==13)rot.x=wind*33+strike*8;
   if(b==14)rot.x=wind*4+strike*(type==6?25:12);
   if(b==15)rot.x=-wind*18+strike*24+settle*6;
   if(b==17){rot.x=wind*8+strike*32+settle*9;rot.z=-strike*10;}
   if(b==9||b==16)rot.y=wind*25-strike*(b==16?45:30);
   if(b==10||b==11){rot.z=-side*(wind*24-strike*37);rot.y=side*12;}
   if(caster){
    if(b==0){pos.y+=wind*.06f+strike*.015f;pos.z-=strike*.04f;}
    if(b==1){rot.x=-wind*9+strike*9;rot.y=-wind*9+strike*14;}
    if(b==3||b==4){rot.x=-wind*(b==4?132:68)-strike*(b==4?80:100);rot.z=side*(wind*28+strike*42);}
    if(b==5||b==6)rot.x=-wind*30-strike*8;
    if(b==7||b==8)rot.x=-wind*9;
    bool staffArm=type==10?b==3:b==4,staffFore=type==10?b==5:b==6;
    if(type!=3&&staffArm){rot.x=-wind*78-strike*24;rot.z=side*(wind*7+strike*23);}
    if(type!=3&&staffFore){rot.x=wind*78+strike*24;pos.y+=wind*.06f+strike*.03f;}
   }
   if(type==11){
    if(b==3){rot.x=-25-wind*12-strike*16;rot.z=-wind*15;}
    if(b==5)rot.x=25+wind*12+strike*16;
    if(b==4){rot.x=-wind*55-strike*108;rot.y=-wind*28+strike*12;rot.z=wind*22;}
    if(b==6)rot.x=-wind*65+strike*10;
   }
   if(type==5||type==6){
    if(b==0)pos.y+=wind*.075f-strike*.035f;
    if(b==1)rot.x=-wind*18+strike*(type==6?38:26);
    if(b==10||b==11){rot.z=-side*(wind*52-strike*16);rot.y=side*(12+strike*27);}
    if(b==7||b==8)rot.x=-20-strike*16;if(b==12||b==13)rot.x=44;
   }
   if(type==7){if(b==0)pos.y+=strike*.08f;if(b==2){rot.x=-wind*16+strike*26;scale.y=1-wind*.1f+strike*.07f;}}
   if(type==0||type==1){if(b==0)pos.y+=strike*.065f;if(b==1)rot.x=-wind*18+strike*34;}
   if(type==9){if(b==1){rot.x=-wind*12+strike*38;rot.y=0;}if(b==3||b==4)rot.x=-wind*30-strike*60;if(b==5||b==6)rot.x=-wind*15-strike*18;}
  }
  if(state==CreatureMotion.Hit){
   float recoil=Ease(t,0,.16f)*(1-Ease(t,.26f,1));
   if(b==1){rot.x=-recoil*21;rot.z=recoil*8;pos.z-=recoil*.036f;}
   if(b==2)rot.x=recoil*15;if(b==3||b==4)rot.z=-side*recoil*22;
   if(b==14)rot.x=recoil*13;if(b==15||b==17)rot.x=recoil*22;
  }
  if(state==CreatureMotion.Spawn){
   float grow=Ease(t,0,.64f),overshoot=Mathf.Sin(Mathf.InverseLerp(.48f,1,t)*Mathf.PI)*.07f;
   if(b==0){scale=Vector3.one*Mathf.Lerp(.03f,1,grow);scale.y+=overshoot;scale.x-=overshoot*.4f;scale.z-=overshoot*.4f;pos.y+=Mathf.Sin(t*Mathf.PI)*.09f;}
   if(b==3||b==4)rot.z=-side*Mathf.Sin(t*Mathf.PI)*35;
   if(b==12||b==13)rot.x=Mathf.Sin(t*Mathf.PI)*28;
   if(b==15||b==17)rot.x=-Mathf.Sin(t*Mathf.PI)*20;
   if(b==10||b==11)rot.z=-side*Mathf.Sin(t*Mathf.PI)*38;
  }
  if(state==CreatureMotion.Death){
   float collapse=Ease(t,.12f,.8f),shrink=Ease(t,.55f,1),recoil=Mathf.Sin(Mathf.Min(1,t*2.5f)*Mathf.PI);
   if(b==0){rot.z=collapse*(fly?-100:heavy?55:78);rot.x=collapse*(fly?35:8);pos.y-=collapse*.16f;scale=Vector3.one*Mathf.Lerp(1,.05f,shrink);}
   if(b==1)rot.x=-recoil*18;if(b==2){rot.x=-collapse*25;rot.z=collapse*14;}
   if(b==3||b==4){rot.z=side*recoil*45;rot.x=-collapse*25;}
   if(b==12||b==13)rot.x=collapse*65;
   if(b==14)rot.x=recoil*18;
   if(b==15||b==17)rot.x=collapse*35;
   if(b==10||b==11)rot.z=side*collapse*55;
  }
 }
}
}
