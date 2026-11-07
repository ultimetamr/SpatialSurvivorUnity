using UnityEngine;
#if ENABLE_PICO_XR_SDK
using ByteDance.PICO.XR;
#endif
namespace SpatialSurvivor {
// Lightweight visible joints, driven by the same tracking-space origin as the camera and room.
public sealed class SurvivorTrackedHands:MonoBehaviour {
 SurvivorPlatform platform;readonly LineRenderer[,] fingers=new LineRenderer[2,5];Material material;
#if ENABLE_PICO_XR_SDK
 HandJointLocations left,right;
#endif
 void Start(){platform=GetComponent<SurvivorPlatform>();material=new Material(Shader.Find("Sprites/Default"));for(int h=0;h<2;h++)for(int f=0;f<5;f++){var go=new GameObject((h==0?"Left":"Right")+" Hand Finger "+f);go.transform.SetParent(platform.Origin,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=material;line.startWidth=.007f;line.endWidth=.004f;line.startColor=line.endColor=new Color(.35f,.9f,1,.8f);line.positionCount=f==0?5:6;line.enabled=false;fingers[h,f]=line;}Application.onBeforeRender+=Refresh;}
 void Refresh(){
#if ENABLE_PICO_XR_SDK
  if(!platform||platform.Simulation)return;
  Draw(0,PXR_HandTracking.GetJointLocations(HandType.HandLeft,ref left),left);Draw(1,PXR_HandTracking.GetJointLocations(HandType.HandRight,ref right),right);
#endif
 }
#if ENABLE_PICO_XR_SDK
 void Draw(int h,bool found,HandJointLocations joints){for(int f=0;f<5;f++){var line=fingers[h,f];bool valid=found&&joints.isActive!=0&&joints.jointLocations!=null&&joints.jointCount>=26;int start=f==0?2:6+(f-1)*5;for(int i=0;valid&&i<line.positionCount;i++){int index=i==0?1:start+i-1;valid=(joints.jointLocations[index].locationStatus&HandLocationStatus.PositionValid)!=0;}line.enabled=valid;if(!valid)continue;for(int i=0;i<line.positionCount;i++){int index=i==0?1:start+i-1;line.SetPosition(i,joints.jointLocations[index].pose.Position.ToVector3());}}}
#endif
 void OnDestroy(){Application.onBeforeRender-=Refresh;if(material)Destroy(material);for(int h=0;h<2;h++)for(int f=0;f<5;f++)if(fingers[h,f])Destroy(fingers[h,f].gameObject);}
}
}
