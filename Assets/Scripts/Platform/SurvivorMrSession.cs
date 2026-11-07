using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
#if ENABLE_PICO_XR_SDK
using ByteDance.PICO.XR;
#endif

namespace SpatialSurvivor {
[DefaultExecutionOrder(-150)]
public sealed class SurvivorMrSession:MonoBehaviour {
 public SurvivorMrState State {get;}=new SurvivorMrState();
 public bool ShowMesh {get;private set;}
 public int MeshCount=>liveRoom?.Count??0;
 public int SurfaceCount=>capturedRoom?.Count??0;
 public int TriangleCount=>(liveRoom?.Triangles??0)+(capturedRoom?.Triangles??0);
 public string LastError {get;private set;}="";
 public bool PassthroughActive {get;private set;}
 public bool HandPermissionGranted {get;private set;}
 public bool MeshRunning=>meshSubsystem!=null&&meshSubsystem.running;
 public bool SceneProviderReady {get;private set;}
 public string Diagnostics=>platform.Simulation?"当前为编辑器模拟；真实透视和系统扫描只在 PICO 头显中运行":
  "透视 "+(PassthroughActive?"已开启":"等待")+" · 定位 "+(platform.Tracking?"正常":"丢失")+"\n"+
  platform.TrackingOriginStatus+" · 实时网格 "+MeshCount+" · 场景表面 "+SurfaceCount+"\n"+
  "手势权限 "+(HandPermissionGranted?"已授权":"未授权（手柄备用）")+"\n"+
  "通行地面 "+(platform.Navigation.Count*RoomNavigation.Cell*RoomNavigation.Cell).ToString("F1")+" m² · "+TriangleCount+" 三角形\n"+State.Detail+
  (string.IsNullOrEmpty(LastError)?"":"\n"+LastError);
 SurvivorPlatform platform;SurvivorRoomGeometry liveRoom,capturedRoom;Material roomMaterial;
 XRMeshSubsystem meshSubsystem;CancellationTokenSource lifetime;Coroutine operation;
 string operationError;bool subscribed,reloadPending,alive=true;
 const string SpatialPermission="com.picovr.permission.SPATIAL_DATA";

 void Awake(){platform=GetComponent<SurvivorPlatform>();}
 void Start(){if(platform.Simulation){State.Set(SurvivorMrPhase.Simulation,"编辑器模拟房间");return;}Begin(Initialize(false));}
 void Begin(IEnumerator work){operation=StartCoroutine(Run(work));}
 IEnumerator Run(IEnumerator work){yield return work;operation=null;}
 public void Retry(){if(operation!=null)return;if(platform.Simulation){platform.RebuildRoom();return;}Begin(Initialize(false));}
 public void ScanRoom(){if(operation!=null)return;if(platform.Simulation){State.Set(SurvivorMrPhase.Simulation,"模拟房间已刷新；系统扫描需在头显中使用");platform.RebuildRoom();return;}
#if ENABLE_PICO_XR_SDK
  Begin(SceneProviderReady&&lifetime!=null&&!lifetime.IsCancellationRequested?Capture():Initialize(true));
#endif
 }
 public void ReloadRoom(){reloadPending=true;}
 public void ToggleMesh(){ShowMesh=!ShowMesh;if(roomMaterial)roomMaterial.SetFloat("_Preview",ShowMesh?1:0);liveRoom?.SetPreview(ShowMesh);capturedRoom?.SetPreview(ShowMesh);}
 public void RebuildNavigation(){
  // A partial live scan can contain walls before the floor arrives. Retain captured floor in that case.
  if(capturedRoom!=null)capturedRoom.SetActive(MeshCount==0);
  Physics.SyncTransforms();platform.Navigation.Rebuild(platform.Head.transform.position);
  if(!platform.Navigation.Ready&&MeshCount>0&&SurfaceCount>0){capturedRoom.SetActive(true);Physics.SyncTransforms();platform.Navigation.Rebuild(platform.Head.transform.position);}
 }

 IEnumerator Initialize(bool scan) {
  lifetime?.Cancel();lifetime?.Dispose();lifetime=new CancellationTokenSource();LastError="";SceneProviderReady=false;
  State.Set(SurvivorMrPhase.Booting,"正在启动 PICO XR（最长 20 秒）");
  float until=Time.realtimeSinceStartup+20;
  while((XRGeneralSettings.Instance==null||XRGeneralSettings.Instance.Manager==null||XRGeneralSettings.Instance.Manager.activeLoader==null)&&Time.realtimeSinceStartup<until)yield return null;
  if(XRGeneralSettings.Instance==null||XRGeneralSettings.Instance.Manager==null||XRGeneralSettings.Instance.Manager.activeLoader==null){Fail("PICO XR 未启动，请检查 PICO Loader 和设备支持");yield break;}
#if ENABLE_PICO_XR_SDK
  if(!(XRGeneralSettings.Instance.Manager.activeLoader is PXR_Loader)){Fail("当前 Loader 不是 PICO XR，无法启动 MR");yield break;}
  if(!platform.Origin.GetComponent<PXR_Manager>()){var manager=platform.Origin.gameObject.AddComponent<PXR_Manager>();manager.openMRC=false;manager.screenFade=false;}
  EnablePassthrough();platform.SetFloorOrigin();
  State.Set(SurvivorMrPhase.Permission,"请在系统弹窗中允许访问空间数据");
#if UNITY_ANDROID && !UNITY_EDITOR
  bool answered=UnityEngine.Android.Permission.HasUserAuthorizedPermission(SpatialPermission),granted=answered,permanent=false;
  if(!answered){var callbacks=new UnityEngine.Android.PermissionCallbacks();callbacks.PermissionGranted+=p=>{if(p==SpatialPermission){answered=true;granted=true;}};callbacks.PermissionDenied+=p=>{if(p==SpatialPermission)answered=true;};callbacks.PermissionDeniedAndDontAskAgain+=p=>{if(p==SpatialPermission){answered=true;permanent=true;}};UnityEngine.Android.Permission.RequestUserPermission(SpatialPermission,callbacks);}
  until=Time.realtimeSinceStartup+60;while(!answered&&Time.realtimeSinceStartup<until)yield return null;
  // A grant may arrive through activity resume rather than the callback on some OS versions.
  granted|=UnityEngine.Android.Permission.HasUserAuthorizedPermission(SpatialPermission);
  if(!granted){State.Set(SurvivorMrPhase.Denied,permanent?"空间权限已被关闭，请到系统应用权限开启后重试":"未获得空间权限，请点击重新授权 / 重试");yield break;}
  const string handPermission="com.picovr.permission.HAND_TRACKING";
  HandPermissionGranted=UnityEngine.Android.Permission.HasUserAuthorizedPermission(handPermission);
  if(!HandPermissionGranted){
   State.Set(SurvivorMrPhase.Permission,"请允许手部追踪；拒绝时仍可使用手柄");bool handAnswered=false;
   var handCallbacks=new UnityEngine.Android.PermissionCallbacks();handCallbacks.PermissionGranted+=p=>{handAnswered=true;};handCallbacks.PermissionDenied+=p=>{handAnswered=true;};handCallbacks.PermissionDeniedAndDontAskAgain+=p=>{handAnswered=true;};
   UnityEngine.Android.Permission.RequestUserPermission(handPermission,handCallbacks);until=Time.realtimeSinceStartup+30;while(!handAnswered&&Time.realtimeSinceStartup<until)yield return null;
   HandPermissionGranted=UnityEngine.Android.Permission.HasUserAuthorizedPermission(handPermission);
  }
#endif
  EnsureGeometry();
  if(!subscribed){PXR_Manager.SpatialMeshDataUpdated+=OnMeshData;PXR_Manager.SceneAnchorDataUpdated+=OnSceneChanged;subscribed=true;}
  State.Set(SurvivorMrPhase.Starting,"正在启动空间网格和场景捕获服务");
  var meshes=new List<XRMeshSubsystem>();SubsystemManager.GetSubsystems(meshes);meshSubsystem=meshes.Count>0?meshes[0]:null;
  if(meshSubsystem!=null&&!meshSubsystem.running)meshSubsystem.Start();
  // The mesh provider is owned by XRMeshSubsystem. Scene Capture has its own explicit provider.
  operationError=null;
  if(PXR_MixedReality.GetSenseDataProviderState(PxrSenseDataProviderType.SceneCapture,out var providerState)==PxrResult.SUCCESS&&providerState==PxrSenseDataProviderState.Running)SceneProviderReady=true;
  else {
   var providerTask=PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SceneCapture,lifetime.Token);
   yield return Wait(providerTask,25);
   if(operationError==null&&providerTask.Result==PxrResult.SUCCESS)SceneProviderReady=true;
   else LastError="场景捕获服务未就绪："+(operationError??providerTask.Result.ToString());
  }
  if(!SceneProviderReady&&!MeshRunning){Fail(LastError+"；空间网格服务也不可用");yield break;}
  if(operationError!=null){Fail(LastError);yield break;}
  if(!MeshRunning)LastError="实时网格不可用；将使用扫描的房间表面";
  if(SceneProviderReady){if(scan)yield return Capture();else yield return LoadCapturedRoom();}
  if(State.Phase==SurvivorMrPhase.Failed)yield break;
  State.Set(SurvivorMrPhase.Running,"环顾房间以更新网格；也可点击扫描 / 重扫房间");platform.RoomChanged();
#else
  Fail("构建未启用 PICO XR SDK");yield break;
#endif
 }
 void EnsureGeometry(){
  if(roomMaterial)return;var shader=Resources.Load<Shader>("Shaders/RoomOcclusion");roomMaterial=new Material(shader);
  roomMaterial.SetFloat("_Preview",ShowMesh?1:0);liveRoom=new SurvivorRoomGeometry(platform.Origin,"Live Spatial Mesh - Navigation Only",roomMaterial);capturedRoom=new SurvivorRoomGeometry(platform.Origin,"Captured Room - Floor Walls Furniture",roomMaterial);
  liveRoom.SetPreview(ShowMesh);capturedRoom.SetPreview(ShowMesh);
  RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.7f,.8f);
  var fill=new GameObject("MR Model Light",typeof(Light));fill.transform.SetParent(transform,false);fill.transform.rotation=Quaternion.Euler(45,-30,0);fill.GetComponent<Light>().type=LightType.Directional;fill.GetComponent<Light>().intensity=.7f;
  if(!GetComponent<SurvivorTrackedHands>())gameObject.AddComponent<SurvivorTrackedHands>();
 }
 IEnumerator Wait(Task task,float timeout){
  operationError=null;float until=Time.realtimeSinceStartup+timeout;
  while(alive&&!task.IsCompleted&&Time.realtimeSinceStartup<until)yield return null;
  if(!alive)yield break;
  if(!task.IsCompleted){operationError="操作超时，请重试";lifetime.Cancel();}
  else if(task.IsCanceled)operationError="操作已取消，请重试";
  else if(task.IsFaulted)operationError=task.Exception?.GetBaseException().Message??"空间服务异常";
  if(operationError!=null)Debug.LogWarning("[Survivor MR] "+operationError);
 }
 void Fail(string detail){LastError=detail;State.Set(SurvivorMrPhase.Failed,detail+"；点击重新授权 / 重试");}
#if ENABLE_PICO_XR_SDK
 void EnablePassthrough(){PXR_Manager.EnableVideoSeeThrough=true;platform.Head.clearFlags=CameraClearFlags.SolidColor;platform.Head.backgroundColor=Color.clear;PassthroughActive=true;}
 IEnumerator Capture(){
  LastError="";State.Set(SurvivorMrPhase.Scanning,"系统房间扫描中，完成或取消后返回游戏");
  var capture=PXR_MixedReality.StartSceneCaptureAsync(lifetime.Token);yield return Wait(capture,240);
  if(operationError!=null){Fail(operationError);yield break;}
  if(capture.Result!=PxrResult.SUCCESS){LastError="扫描未完成："+capture.Result+"，可再次扫描";State.Set(SurvivorMrPhase.Running,"保留原房间数据");yield break;}
  liveRoom.Clear();capturedRoom.Clear();platform.RoomChanged();
  yield return LoadCapturedRoom();
  if(State.Phase!=SurvivorMrPhase.Failed)State.Set(SurvivorMrPhase.Running,"房间扫描完成，等待地面网格就绪");
 }
 IEnumerator LoadCapturedRoom(){
  State.Set(SurvivorMrPhase.Loading,"正在读取房间地面、墙壁和家具");
  var query=PXR_MixedReality.QuerySceneAnchorAsync(lifetime.Token);yield return Wait(query,20);
  if(operationError!=null){Fail(operationError);yield break;}
  if(query.Result.result!=PxrResult.SUCCESS){LastError="读取场景失败："+query.Result.result;State.Set(SurvivorMrPhase.Running,"请扫描房间或重试");yield break;}
  // Reconcile the complete snapshot, including deletion and changed room layout.
  capturedRoom.Clear();
  foreach(var item in query.Result.anchorDictionary)AddSceneSurface(item.Key,item.Value);
  capturedRoom.SetActive(MeshCount==0);platform.RoomChanged();
  State.Set(SurvivorMrPhase.Running,SurfaceCount==0?"没有已扫描房间，请点击扫描 / 重扫房间":"房间表面已加载；实时网格持续更新");
 }
 void AddSceneSurface(ulong handle,Guid id){
  if(PXR_MixedReality.LocateAnchor(handle,out var position,out var rotation)!=PxrResult.SUCCESS)return;
  if(PXR_MixedReality.GetScenePolygonData(handle,out var polygon)==PxrResult.SUCCESS&&polygon.Length>=3){
   var indices=SurvivorRoomGeometry.Triangulate(polygon);var vertices=Array.ConvertAll(polygon,p=>new Vector3(p.x,p.y,0));capturedRoom.Upsert(id,position,rotation,vertices,indices);return;
  }
  if(PXR_MixedReality.GetSceneBox3DData(handle,out var offset,out var boxRotation,out var extent)==PxrResult.SUCCESS){
   var v=new Vector3[8];for(int i=0;i<8;i++)v[i]=offset+boxRotation*Vector3.Scale(new Vector3((i&1)==0?-.5f:.5f,(i&2)==0?-.5f:.5f,(i&4)==0?-.5f:.5f),extent);
   capturedRoom.Upsert(id,position,rotation,v,new[]{0,2,3,0,3,1,4,5,7,4,7,6,0,4,6,0,6,2,1,3,7,1,7,5,0,1,5,0,5,4,2,6,7,2,7,3});return;
  }
  if(PXR_MixedReality.GetSceneBox2DData(handle,out var center,out var size)==PxrResult.SUCCESS){
   var p=new[]{center+new Vector2(-size.x,-size.y)*.5f,center+new Vector2(size.x,-size.y)*.5f,center+size*.5f,center+new Vector2(-size.x,size.y)*.5f};
   capturedRoom.Upsert(id,position,rotation,Array.ConvertAll(p,v=>new Vector3(v.x,v.y,0)),SurvivorRoomGeometry.Triangulate(p));
  }
 }
 void OnMeshData(List<PxrSpatialMeshInfo> infos){
  if(!alive||liveRoom==null)return;
  bool changed=false;
  foreach(var info in infos){if(info.state==MeshChangeState.Removed){changed|=liveRoom.Contains(info.uuid);liveRoom.Remove(info.uuid);continue;}
   if(info.state==MeshChangeState.Unchanged&&liveRoom.Contains(info.uuid))continue;
   // An existing SDK snapshot may report Unchanged on first subscription; it still needs a local mesh.
   if(info.vertices==null||info.indices==null)continue;
   changed|=liveRoom.Upsert(info.uuid,info.position,info.rotation,info.vertices,Array.ConvertAll(info.indices,index=>(int)index));
  }
  if(changed){capturedRoom.SetActive(MeshCount==0);platform.RoomChanged();}
 }
 void OnSceneChanged(){reloadPending=true;}
 void OnApplicationPause(bool paused){if(!paused&&PassthroughActive){EnablePassthrough();reloadPending=true;
#if UNITY_ANDROID && !UNITY_EDITOR
  HandPermissionGranted=UnityEngine.Android.Permission.HasUserAuthorizedPermission("com.picovr.permission.HAND_TRACKING");
  if(State.Phase==SurvivorMrPhase.Running&&!UnityEngine.Android.Permission.HasUserAuthorizedPermission(SpatialPermission))State.Set(SurvivorMrPhase.Denied,"空间权限已被关闭，请重新授权后重试");
#endif
 }}
#endif
 void Update(){
  if(platform==null||platform.Simulation)return;
  if(reloadPending&&operation==null&&SceneProviderReady&&State.Phase==SurvivorMrPhase.Running){reloadPending=false;
#if ENABLE_PICO_XR_SDK
   Begin(LoadCapturedRoom());
#endif
  }
 }
 void OnDestroy(){alive=false;lifetime?.Cancel();lifetime?.Dispose();
#if ENABLE_PICO_XR_SDK
  if(subscribed){PXR_Manager.SpatialMeshDataUpdated-=OnMeshData;PXR_Manager.SceneAnchorDataUpdated-=OnSceneChanged;}
  if(!platform.Simulation&&SceneProviderReady)PXR_MixedReality.StopSenseDataProvider(PxrSenseDataProviderType.SceneCapture);
#endif
  if(meshSubsystem!=null&&meshSubsystem.running)meshSubsystem.Stop();liveRoom?.Dispose();capturedRoom?.Dispose();if(roomMaterial)Destroy(roomMaterial);
 }
}
}
