using UnityEngine;

namespace SpatialSurvivor.Editor {
public static partial class SurvivorArtBuilder {
 static void PolishHandWeapon(Writer m,HandWeaponDefinition weapon){
  var gold=C("DDBB76");var steel=C("AAB9CF");var dark=C("26354B");var glow=weapon.color;
  bool gun=weapon.kind==HandWeaponKind.ArcPistol||weapon.kind==HandWeaponKind.Scattergun;
  if(gun){
   bool scatter=weapon.kind==HandWeaponKind.Scattergun;float muzzle=scatter?.36f:.28f;
   m.BevelBox(null,new Vector3(0,.072f,.083f),new Vector3(scatter?.063f:.05f,.015f,.13f),steel);
   for(int side=-1;side<=1;side+=2){
    m.BevelBox(null,new Vector3(side*(scatter?.057f:.046f),.034f,.06f),new Vector3(.008f,.028f,.08f),gold);
    for(int i=0;i<3;i++)m.BevelBox(null,new Vector3(side*(scatter?.067f:.056f),.04f,.025f+i*.037f),new Vector3(.004f,.016f,.006f),dark);
    m.BevelBox(null,new Vector3(side*.033f,-.077f,-.018f),new Vector3(.005f,.067f,.026f),dark);
    m.Gem(null,new Vector3(side*.041f,-.07f,-.018f),new Vector3(.006f,.024f,.019f),glow);
   }
   m.BevelBox(null,new Vector3(0,-.166f,-.018f),new Vector3(.037f,.012f,.046f),gold);
   m.BevelBox(null,new Vector3(0,.105f,muzzle-.033f),new Vector3(.009f,.014f,.011f),dark);
   m.Gem(null,new Vector3(0,.12f,muzzle-.033f),new Vector3(.008f,.008f,.008f),glow);
   if(scatter){
    // A second visible barrel and ribbed pump distinguish the scatter weapon.
    m.Tube(null,new[]{new Vector3(0,-.015f,.19f),new Vector3(0,-.015f,muzzle)},.027f,.027f,steel);
    m.Tube(null,new[]{new Vector3(0,-.015f,muzzle-.008f),new Vector3(0,-.015f,muzzle+.003f)},.02f,.02f,dark);
    for(int i=0;i<4;i++)m.BevelBox(null,new Vector3(0,-.038f,.16f+i*.026f),new Vector3(.051f,.013f,.009f),gold);
   }else{
    m.BevelBox(null,new Vector3(0,.05f,.19f),new Vector3(.029f,.026f,.023f),steel);
    m.Tube(null,new[]{new Vector3(0,.035f,muzzle-.025f),new Vector3(0,.035f,muzzle-.016f)},.029f,.029f,glow);
   }
   return;
  }
  if(weapon.kind==HandWeaponKind.Runeblade){
   for(int side=-1;side<=1;side+=2){
    m.Leaf(null,new Vector3(side*.035f,.115f,0),new Vector3(side*.19f,.21f,0),.034f,gold);
    m.Gem(null,new Vector3(side*.115f,.165f,.017f),new Vector3(.022f,.032f,.015f),glow);
   }
   m.Gem(null,new Vector3(0,.122f,.032f),new Vector3(.042f,.053f,.022f),glow);
   for(int i=0;i<5;i++)for(int side=-1;side<=1;side+=2){float y=.27f+i*.075f;m.Tube(null,new[]{new Vector3(0,y,.027f),new Vector3(side*.018f,y+.023f,.026f)},.003f,.002f,gold);}
   m.BevelBox(null,new Vector3(0,-.115f,0),new Vector3(.032f,.015f,.029f),gold);
   m.Gem(null,new Vector3(0,-.145f,0),new Vector3(.035f,.043f,.029f),glow);
  }else{
   if(weapon.kind==HandWeaponKind.FrostStaff){
    for(int side=-1;side<=1;side+=2){m.Gem(null,new Vector3(side*.10f,.31f,.015f),new Vector3(.04f,.15f,.038f),C("D2F5F3"));m.Gem(null,new Vector3(side*.14f,.22f,.015f),new Vector3(.032f,.09f,.032f),glow);}
   }
   if(weapon.kind==HandWeaponKind.StarWand){
    for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;var p=new Vector3(Mathf.Sin(a)*.12f,.32f+Mathf.Cos(a)*.12f,.035f);m.Gem(null,p,new Vector3(.02f,.035f,.015f),C("F6EDBB"));}
   }
   if(weapon.kind==HandWeaponKind.EmberStaff){
    for(int i=0;i<3;i++){float x=(i-1)*.055f;m.Leaf(null,new Vector3(x,.27f,.07f),new Vector3(x*1.5f,.51f-Mathf.Abs(i-1)*.055f,.02f),.04f,i==1?C("FFF1BB"):C("F7C45F"));}
   }
   if(weapon.kind==HandWeaponKind.StormWand){
    for(int side=-1;side<=1;side+=2){m.Tube(null,new[]{new Vector3(side*.035f,.18f,0),new Vector3(side*.17f,.35f,0),new Vector3(side*.12f,.49f,0)},.015f,.006f,gold);m.Leaf(null,new Vector3(side*.12f,.32f,0),new Vector3(side*.21f,.39f,0),.025f,C("D5E9EC"));}
   }
   if(weapon.kind==HandWeaponKind.VoidScepter){var ring=new Vector3[25];for(int i=0;i<ring.Length;i++){float a=i*Mathf.PI*2/24;ring[i]=new Vector3(Mathf.Cos(a)*.11f,.32f+Mathf.Sin(a)*.045f,Mathf.Sin(a)*.11f);}m.Tube(null,ring,.008f,.008f,C("BA9CDE"));}
   for(int i=0;i<3;i++)m.Tube(null,new[]{new Vector3(-.02f,.02f+i*.055f,.016f),new Vector3(0,.035f+i*.055f,.026f),new Vector3(.02f,.02f+i*.055f,.016f)},.006f,.004f,steel);
   for(int side=-1;side<=1;side+=2)m.Gem(null,new Vector3(side*.09f,.23f,.025f),new Vector3(.026f,.043f,.024f),glow);
   m.BevelBox(null,new Vector3(0,-.17f,0),new Vector3(.026f,.018f,.026f),gold);
  }
 }
 sealed partial class Writer {
  // Octagonal sections with bevelled end faces produce real metal edges.
  public void BevelBox(Transform bone,Vector3 p,Vector3 half,Color color){
   int start=v.Count;var corners=new[]{new Vector2(-.7f,-1),new Vector2(.7f,-1),new Vector2(1,-.7f),new Vector2(1,.7f),new Vector2(.7f,1),new Vector2(-.7f,1),new Vector2(-1,.7f),new Vector2(-1,-.7f)};
   for(int ring=0;ring<4;ring++){float z=ring==0?-1:ring==1?-.85f:ring==2?.85f:1;float scale=ring==0||ring==3?.82f:1;
    for(int i=0;i<8;i++)Vertex(bone,p+new Vector3(corners[i].x*half.x*scale,corners[i].y*half.y*scale,z*half.z),color*(ring==0?.75f:ring==3?1.12f:1));
   }
   for(int ring=0;ring<3;ring++)for(int i=0;i<8;i++){int a=start+ring*8+i,b=start+ring*8+(i+1)%8;Tri(a,b,b+8);Tri(a,b+8,a+8);}
   int back=Vertex(bone,p-Vector3.forward*half.z,color*.75f),front=Vertex(bone,p+Vector3.forward*half.z,color*1.12f);
   for(int i=0;i<8;i++){Tri(back,start+(i+1)%8,start+i);Tri(front,start+24+i,start+24+(i+1)%8);}
  }
  public void ForgedBlade(float root,float tip,float width,Color color){
   int start=v.Count;float[] stations={0,.12f,.60f,.86f,1};
   for(int row=0;row<stations.Length;row++){float t=stations[row],w=width*(row==0?.66f:row==1?1:row==2?.8f:row==3?.45f:.004f),y=Mathf.Lerp(root,tip,t),thickness=.022f*(1-t)+.001f;
    Vertex(null,new Vector3(-w,y,0),color*.68f);Vertex(null,new Vector3(0,y,thickness),C("E5F3EC"));Vertex(null,new Vector3(w,y,0),color);Vertex(null,new Vector3(0,y,-thickness),color*.82f);
   }
   for(int row=0;row<stations.Length-1;row++)for(int i=0;i<4;i++){int a=start+row*4+i,b=start+row*4+(i+1)%4;Tri(a,b,b+4);Tri(a,b+4,a+4);}
  }
 }
}
}
