using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#if ENABLE_PICO_XR_SDK
using ByteDance.PICO.XR;
#endif
namespace SpatialSurvivor {
[DefaultExecutionOrder(-200)]
public sealed class SurvivorPlatform:MonoBehaviour {
 [SerializeField] Transform xrOrigin;
 [SerializeField] Camera headCamera;
 public Camera Head=>headCamera;
 public Transform Origin=>xrOrigin;
 public RoomNavigation Navigation {get;}=new RoomNavigation();
 public SurvivorMrSession MR {get;private set;}
 public bool Tracking {get;private set;}
 public bool Simulation=>Application.isEditor;
 public bool Ready=>MR&&MR.State.CanPlay(Tracking,Navigation.Ready);
 public bool MenuPressed {get;private set;}
 public SurvivorPointerSource PointerSource {get;private set;}
 public string TriggerStatus {get{bool right=PointerSource==SurvivorPointerSource.RightController;if(!right&&PointerSource!=SurvivorPointerSource.LeftController)return "";return PointerSource+" axis="+((right?rightHasAxis:leftHasAxis)?(right?rightAxis:leftAxis).ToString("F2"):"unavailable")+" digital="+(right?rightDigital:leftDigital);}}
 readonly SurvivorTriggerLatch rightTrigger=new SurvivorTriggerLatch(),leftTrigger=new SurvivorTriggerLatch();
 float rightAxis,leftAxis;bool rightHasAxis,leftHasAxis,rightDigital,leftDigital;
 public string PointerStatus=>PointerSource==SurvivorPointerSource.RightController?"右手柄":PointerSource==SurvivorPointerSource.LeftController?"左手柄":PointerSource==SurvivorPointerSource.RightHand?"右手手势":PointerSource==SurvivorPointerSource.LeftHand?"左手手势":PointerSource==SurvivorPointerSource.Mouse?"鼠标":"未检测到手柄 / 手势";
 public string TrackingOriginStatus {get;private set;}="等待地面坐标";
 public string Status {
  get {
   if(!MR)return "MR 正在初始化";
   if(!MR.State.AllowsCombat)return MR.State.Detail;
   if(!Tracking)return "定位丢失，战斗暂停";
   if(!Navigation.Ready)return "请扫描地面、墙壁和家具；有效地面不足";
   return Simulation?"编辑器模拟房间（不是头显 MR）":"MR 房间就绪 · "+Navigation.Count+" 个通行格";
  }
 }
 readonly List<XRInputSubsystem> inputs=new List<XRInputSubsystem>();
 float lastPose=-100,stable,nextRebuild;bool dirty=true,focused=true,menuHeld,poseSampleGood;Vector2 look;
 // Also called by editor setup so the XR rig is visible before Play.
 public void EnsureRig() {
  if(!xrOrigin){xrOrigin=new GameObject("XR Origin - Room Scale").transform;xrOrigin.SetParent(transform,false);}
  if(!headCamera){var go=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));go.tag="MainCamera";go.transform.SetParent(xrOrigin,false);headCamera=go.GetComponent<Camera>();}
  Head.nearClipPlane=.05f;Head.farClipPlane=50;Head.clearFlags=CameraClearFlags.SolidColor;Head.backgroundColor=Color.clear;Head.stereoTargetEye=StereoTargetEyeMask.Both;
 }
 void Awake(){EnsureRig();MR=GetComponent<SurvivorMrSession>();if(!MR)MR=gameObject.AddComponent<SurvivorMrSession>();if(Simulation){Head.transform.localPosition=new Vector3(0,1.65f,0);CreateRoom();}}
 IEnumerator Start(){if(Simulation){yield return null;RebuildRoom();Tracking=true;yield break;}Application.onBeforeRender+=SampleHead;}
 public void SetFloorOrigin() {
  SubsystemManager.GetSubsystems(inputs);
  bool floor=false;foreach(var input in inputs){floor|=input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);input.trackingOriginUpdated-=OriginUpdated;input.trackingOriginUpdated+=OriginUpdated;}
  TrackingOriginStatus=floor?"地面坐标 · 米":"设备坐标 · 以扫描地面为准";
 }
 void OriginUpdated(XRInputSubsystem input){RoomChanged();if(MR)MR.ReloadRoom();}
 public void RoomChanged(){dirty=true;}
 public void RebuildRoom(){if(MR)MR.RebuildNavigation();else{Physics.SyncTransforms();Navigation.Rebuild(Head.transform.position);}dirty=false;nextRebuild=Time.realtimeSinceStartup+1.5f;}
 void OnApplicationFocus(bool value){focused=Application.isBatchMode||value;if(!focused){Tracking=false;stable=0;}}
 void OnApplicationPause(bool value){focused=Application.isBatchMode||!value;if(!focused){Tracking=false;stable=0;}}
 void SampleHead(){poseSampleGood=false;var d=InputDevices.GetDeviceAtXRNode(XRNode.Head);if(d.isValid&&d.TryGetFeatureValue(CommonUsages.isTracked,out bool tracked)&&tracked&&d.TryGetFeatureValue(CommonUsages.devicePosition,out var p)&&d.TryGetFeatureValue(CommonUsages.deviceRotation,out var q)){Head.transform.SetLocalPositionAndRotation(p,q);lastPose=Time.realtimeSinceStartup;poseSampleGood=true;}}
 void Update() {
  MenuPressed=false;
  if(Simulation){if(Input.GetMouseButton(1)){look.x+=Input.GetAxis("Mouse X")*2;look.y=Mathf.Clamp(look.y-Input.GetAxis("Mouse Y")*2,-65,65);Head.transform.rotation=Quaternion.Euler(look.y,look.x,0);}var move=Quaternion.Euler(0,look.x,0)*new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));Head.transform.position+=move*Time.unscaledDeltaTime*1.4f;Tracking=focused;}
  else {
   bool menu=false;foreach(var node in new[]{XRNode.LeftHand,XRNode.RightHand}){var d=InputDevices.GetDeviceAtXRNode(node);if(d.TryGetFeatureValue(CommonUsages.menuButton,out bool down))menu|=down;}MenuPressed=menu&&!menuHeld;menuHeld=menu;
   SampleHead();if(!focused||Time.realtimeSinceStartup-lastPose>.35f){Tracking=false;stable=0;}else if(poseSampleGood){stable+=Time.unscaledDeltaTime;if(stable>=.15f)Tracking=true;}else stable=0;
  }
  if(dirty&&Tracking&&Time.realtimeSinceStartup>=nextRebuild)RebuildRoom();
 }
 public bool Pointer(out Ray ray,out bool pressed) {
  SurvivorPointerSample sample=default;
  if(Simulation)sample=new SurvivorPointerSample(SurvivorPointerSource.Mouse,Head.ScreenPointToRay(Input.mousePosition),Input.GetMouseButton(0));
  else {
   var right=ControllerPointer(XRNode.RightHand,SurvivorPointerSource.RightController);
   var left=ControllerPointer(XRNode.LeftHand,SurvivorPointerSource.LeftController);
   sample=SurvivorPointerInput.Choose(right,left,PointerSource);
#if ENABLE_PICO_XR_SDK
   // Valid hand data may still be present while holding controllers. It must
   // not swallow the controller trigger. Respect the runtime's active mode.
   if(!sample.pressed&&(PXR_HandTracking.GetActiveInputDevice()==ActiveInputDevice.HandTrackingActive||!sample.valid)){
    var hands=SurvivorPointerInput.Choose(HandPointer(HandType.HandRight,SurvivorPointerSource.RightHand),HandPointer(HandType.HandLeft,SurvivorPointerSource.LeftHand),PointerSource);
    if(hands.valid)sample=hands;
   }
#endif
  }
  PointerSource=sample.valid?sample.source:SurvivorPointerSource.None;ray=sample.ray;pressed=sample.pressed;return sample.valid;
 }
 readonly SurvivorWeaponTracking weaponTracking=new SurvivorWeaponTracking();
 int weaponFrame=-1;
 public SurvivorWeaponTracking TrackedWeaponPose {get{SampleWeapon();return weaponTracking;}}
 public bool WeaponPose(out Ray ray,out bool pressed){SampleWeapon();ray=weaponTracking.Ray;pressed=weaponTracking.Fresh&&weaponTracking.Pressed;return weaponTracking.Fresh;}
 void SampleWeapon(){
  if(weaponFrame==Time.frameCount)return;weaponFrame=Time.frameCount;
  if(!focused||!Tracking){weaponTracking.Reset();return;}
  if(Simulation){var ray=new Ray(Head.transform.position+Head.transform.right*.22f-Head.transform.up*.18f,Head.transform.forward);weaponTracking.Sample(new SurvivorPointerSample(SurvivorPointerSource.Mouse,ray,Input.GetMouseButton(0)),Head.transform.rotation,Time.realtimeSinceStartup);return;}
  var right=ControllerPointer(XRNode.RightHand,SurvivorPointerSource.RightController,out var rightRotation);
  var left=ControllerPointer(XRNode.LeftHand,SurvivorPointerSource.LeftController,out var leftRotation);
  bool preferLeft=weaponTracking.Source==SurvivorPointerSource.LeftController;
  var sample=preferLeft&&left.valid?left:right.valid?right:left;
  var rotation=sample.source==SurvivorPointerSource.LeftController?leftRotation:rightRotation;
#if ENABLE_PICO_XR_SDK
  bool handsActive=PXR_HandTracking.GetActiveInputDevice()==ActiveInputDevice.HandTrackingActive;
  if(handsActive||!sample.valid){var hand=HandPointer(HandType.HandRight,SurvivorPointerSource.RightHand,out var handRotation);if(!hand.valid)hand=HandPointer(HandType.HandLeft,SurvivorPointerSource.LeftHand,out handRotation);if(hand.valid&&(handsActive||weaponTracking.Source==SurvivorPointerSource.None||weaponTracking.Source==hand.source)){sample=hand;rotation=handRotation;}}
#endif
  weaponTracking.Sample(sample,rotation,Time.realtimeSinceStartup);
 }
 SurvivorPointerSample ControllerPointer(XRNode node,SurvivorPointerSource source)=>ControllerPointer(node,source,out _);
 SurvivorPointerSample ControllerPointer(XRNode node,SurvivorPointerSource source,out Quaternion rotation){rotation=Quaternion.identity;var latch=node==XRNode.RightHand?rightTrigger:leftTrigger;var d=InputDevices.GetDeviceAtXRNode(node);if(!d.isValid||!d.TryGetFeatureValue(CommonUsages.isTracked,out bool tracked)||!tracked||!d.TryGetFeatureValue(CommonUsages.devicePosition,out var p)||!d.TryGetFeatureValue(CommonUsages.deviceRotation,out var q)){latch.Reset();return default;}d.TryGetFeatureValue(CommonUsages.triggerButton,out bool down);bool hasAxis=d.TryGetFeatureValue(CommonUsages.trigger,out float trigger);bool pressed=latch.Step(hasAxis,trigger,down);if(node==XRNode.RightHand){rightAxis=trigger;rightHasAxis=hasAxis;rightDigital=down;}else{leftAxis=trigger;leftHasAxis=hasAxis;leftDigital=down;}rotation=Origin.rotation*q;return new SurvivorPointerSample(source,new Ray(Origin.TransformPoint(p),rotation*Vector3.forward),pressed);}
#if ENABLE_PICO_XR_SDK
 SurvivorPointerSample HandPointer(HandType type,SurvivorPointerSource source)=>HandPointer(type,source,out _);
 SurvivorPointerSample HandPointer(HandType type,SurvivorPointerSource source,out Quaternion rotation){rotation=Quaternion.identity;var hand=new HandAimState();if(!PXR_HandTracking.GetAimState(type,ref hand)||(hand.aimStatus&HandAimStatus.AimRayValid)==0)return default;var p=hand.aimRayPose.Position.ToVector3();var q=hand.aimRayPose.Orientation.ToQuat();bool down=(hand.aimStatus&(HandAimStatus.AimRayTouched|HandAimStatus.AimIndexPinching))!=0;rotation=Origin.rotation*q;return new SurvivorPointerSample(source,new Ray(Origin.TransformPoint(p),rotation*Vector3.forward),down);}
#endif
 void OnDestroy(){Application.onBeforeRender-=SampleHead;foreach(var input in inputs)input.trackingOriginUpdated-=OriginUpdated;}
 void CreateRoom(){Head.backgroundColor=new Color(.018f,.028f,.05f);Box("Floor",new Vector3(0,-.1f,0),new Vector3(10,.2f,10));Box("Ceiling",new Vector3(0,2.7f,0),new Vector3(10,.15f,10));Box("Wall N",new Vector3(0,1.3f,5),new Vector3(10,2.6f,.15f));Box("Wall S",new Vector3(0,1.3f,-5),new Vector3(10,2.6f,.15f));Box("Wall E",new Vector3(5,1.3f,0),new Vector3(.15f,2.6f,10));Box("Wall W",new Vector3(-5,1.3f,0),new Vector3(.15f,2.6f,10));Box("Furniture",new Vector3(2,.5f,1),new Vector3(1.2f,1,1.5f));Box("Furniture",new Vector3(-2,.4f,-1.5f),new Vector3(1.5f,.8f,1));var sun=new GameObject("Room Light",typeof(Light));sun.transform.SetParent(transform,false);sun.GetComponent<Light>().type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(45,-30,0);RenderSettings.ambientLight=new Color(.4f,.5f,.65f);}
 void Box(string name,Vector3 position,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.layer=RoomNavigation.Layer;go.transform.SetParent(transform,false);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=new Material(Shader.Find("Standard")){color=new Color(.07f,.11f,.17f)};}
}
}
