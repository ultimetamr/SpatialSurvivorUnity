using System.Collections.Generic;
using UnityEngine;
namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
 sealed class HandShot {public GameObject go;public Vector3 velocity;public float remaining;public int weapon,damage,pierce;public readonly Dictionary<Enemy,int> hit=new Dictionary<Enemy,int>();}
 readonly HandShot[] handShots=new HandShot[24];readonly GameObject[] handModels=new GameObject[SurvivorArmory.Weapons.Length];
 Transform handRoot;float handCooldown;bool handArmed,poseKnown,runSettled;Ray previousBlade;int previousPoseRevision=-1;float meleeClock,recoil;readonly float[] meleeNextHit=new float[EnemyCapacity];readonly int[] meleeGeneration=new int[EnemyCapacity];readonly Vector3[] bladeStarts=new Vector3[97],bladeEnds=new Vector3[97];readonly bool[] bladeClear=new bool[97];int runWeapon;
 public int RunWeapon=>runWeapon;
 public HandWeaponDefinition EquippedHandWeapon=>SurvivorArmory.Get(screen==ScreenState.Playing||screen==ScreenState.Paused||screen==ScreenState.Upgrade?runWeapon:meta.equippedWeapon);
 void BuildHandArsenal(){handRoot=new GameObject("Tracked Hand Weapon").transform;handRoot.SetParent(transform,false);for(int i=0;i<handModels.Length;i++){handModels[i]=Instantiate(Resources.Load<GameObject>("Art/Weapons/"+SurvivorArmory.Get(i).model),handRoot);bool blade=SurvivorArmory.Get(i).kind==HandWeaponKind.Runeblade;bool gun=IsGun(i);handModels[i].transform.localPosition=gun?Vector3.down*.035f:Vector3.zero;handModels[i].transform.localRotation=blade||gun?Quaternion.identity:Quaternion.Euler(65,0,0);handModels[i].transform.localScale=Vector3.one*(blade||gun?1:.7f);handModels[i].SetActive(false);}var material=new Material(Shader.Find("Survivor/Additive"));for(int i=0;i<handShots.Length;i++){var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="Hand Spell "+i;Destroy(go.GetComponent<Collider>());go.transform.SetParent(transform,false);go.transform.localScale=Vector3.one*.065f;go.GetComponent<Renderer>().sharedMaterial=material;go.SetActive(false);handShots[i]=new HandShot{go=go};}handRoot.gameObject.SetActive(false);}
 static bool IsGun(int index)=>SurvivorArmory.Get(index).kind==HandWeaponKind.ArcPistol||SurvivorArmory.Get(index).kind==HandWeaponKind.Scattergun;
 void ResetHandCombat(bool clearShots=true){handArmed=poseKnown=false;previousPoseRevision=-1;if(!clearShots)return;handCooldown=meleeClock=recoil=0;System.Array.Clear(meleeNextHit,0,meleeNextHit.Length);System.Array.Clear(meleeGeneration,0,meleeGeneration.Length);foreach(var shot in handShots)if(shot!=null){shot.remaining=0;shot.go.SetActive(false);}}
 void UpdateHandVisual(){
  if(!handRoot)return;var pose=platform.TrackedWeaponPose;bool visible=screen==ScreenState.Playing&&Ready&&focused&&pose.Visible;
  handRoot.gameObject.SetActive(visible);if(!visible){handArmed=poseKnown=false;return;}
  recoil=Mathf.MoveTowards(recoil,0,Time.unscaledDeltaTime*.35f);
  handRoot.SetPositionAndRotation(pose.Ray.origin+pose.Ray.direction*(.07f-recoil),pose.Rotation);
  for(int i=0;i<handModels.Length;i++)handModels[i].SetActive(i==runWeapon);
 }
 void HandWeaponStep(float dt){handCooldown=Mathf.Max(0,handCooldown-dt);meleeClock+=dt;HandShotStep(dt);if(screen!=ScreenState.Playing)return;ProcessHandPose(platform.TrackedWeaponPose,dt);}
 void ProcessHandPose(SurvivorWeaponTracking pose,float dt){
  if(screen!=ScreenState.Playing||!Ready||!focused||!pose.Fresh){handArmed=poseKnown=false;return;}
  if(previousPoseRevision!=pose.Revision){poseKnown=handArmed=false;previousPoseRevision=pose.Revision;}
  var ray=pose.Ray;if(!pose.Pressed)handArmed=true;
  bool continuous=poseKnown&&Vector3.Distance(previousBlade.origin,ray.origin)<1.25f&&Vector3.Angle(previousBlade.direction,ray.direction)<160;
  float tipSpeed=continuous?Mathf.Max(Vector3.Distance(previousBlade.GetPoint(.9f),ray.GetPoint(.9f)),Vector3.Distance(previousBlade.origin,ray.origin))/Mathf.Max(.001f,dt):0;
  if(handArmed){
   if(runWeapon==(int)HandWeaponKind.Runeblade&&continuous&&tipSpeed>.8f){swingPower=SurvivorQualityRules.SwingMultiplier(tipSpeed);MeleeSweep(previousBlade,ray,true);swingPower=1;}
   if(pose.Pressed&&handCooldown<=0)FireHandWeapon(ray);
  }
  previousBlade=ray;poseKnown=true;
 }
 // Sample the swept blade, including rotation about a stationary wrist.
 bool MeleeSweep(Ray from,Ray to,bool swept){
  bool hit=false;float travel=Vector3.Distance(from.origin,to.origin)+Vector3.Angle(from.direction,to.direction)*Mathf.Deg2Rad*.9f;
  int samples=swept?Mathf.Clamp(Mathf.CeilToInt(travel/.035f),1,96):1;
  // Room casts are shared by all enemies, bounded to 97 blade samples.
  for(int n=0;n<=samples;n++){
   float t=n/(float)samples;var origin=Vector3.Lerp(from.origin,to.origin,t);var direction=Vector3.Slerp(from.direction,to.direction,t).normalized;
   float reach=SurvivorArmory.Get(runWeapon).range;
   bladeClear[n]=!Physics.CheckSphere(origin,.025f,1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore);
   if(bladeClear[n]&&Physics.SphereCast(origin,.045f,direction,out var wall,reach,1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore))reach=Mathf.Max(0,wall.distance);
   bladeClear[n]&=reach>=.14f;bladeStarts[n]=origin+direction*.14f;bladeEnds[n]=origin+direction*reach;
  }
  for(int i=0;i<enemies.Length;i++){
   var enemy=enemies[i];if(!enemy.active||(meleeGeneration[i]==enemy.generation&&meleeClock<meleeNextHit[i]))continue;
   var center=EnemyCenter(enemy);bool contact=false;
   for(int n=0;n<=samples;n++){
    if(!bladeClear[n])continue;
    var nearest=NearestOnSegment(bladeStarts[n],bladeEnds[n],center);
    if(Vector3.Distance(nearest,center)<=enemy.radius+.055f&&!Physics.Linecast(bladeStarts[n],center,1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore)){contact=true;break;}
   }
   if(!contact)continue;
   meleeGeneration[i]=enemy.generation;meleeNextHit[i]=meleeClock+Mathf.Max(.12f,.32f*player.interval*HandInterval);
   Damage(enemy,HandDamage(SurvivorArmory.Get(runWeapon).damage*swingPower));HandImpact(enemy,to.origin,swingPower>1);hit=true;
   if(screen!=ScreenState.Playing)break;
  }
  if(hit){Beam(from.GetPoint(.85f),to.GetPoint(.85f),SurvivorArmory.Get(runWeapon).color,.09f);Cue(swingPower>1?FeedbackCue.HeavySwing:FeedbackCue.BladeSwing,to.origin);}
  return hit;
 }
 // Shared path for trigger/pinch, controller swing and regression verification.
 public bool FireHandWeapon(Ray ray){
  if(screen!=ScreenState.Playing||!Ready||!focused||handCooldown>0||ray.direction.sqrMagnitude<.5f)return false;
  ray=new Ray(ray.origin,ray.direction.normalized);var weapon=SurvivorArmory.Get(runWeapon);
  if(weapon.kind==HandWeaponKind.Runeblade){handCooldown=weapon.interval*player.interval*HandInterval;MeleeSweep(ray,ray,false);Beam(ray.origin,ray.GetPoint(weapon.range),weapon.color,.12f);Cue(FeedbackCue.BladeSwing,ray.origin);return true;}
  int pellets=weapon.kind==HandWeaponKind.Scattergun?5:1,available=0;foreach(var shot in handShots)if(shot.remaining<=0)available++;if(available<pellets)return false;
  // The bullet starts at the visible muzzle. Test from the grip too, so a barrel
  // pushed into furniture cannot create bullets on its far side.
  float muzzle=IsGun(runWeapon)?(weapon.kind==HandWeaponKind.Scattergun?.43f:.35f):.1f;
  var origin=ray.GetPoint(muzzle);if(Physics.CheckSphere(ray.origin,.025f,1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore)||Physics.SphereCast(ray.origin,.025f,ray.direction,out _,muzzle,1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore)){handCooldown=weapon.interval*player.interval*HandInterval;return true;}
  float speed=IsGun(runWeapon)?(pellets>1?20:28):7;
  var right=Vector3.Cross(ray.direction,Vector3.up);if(right.sqrMagnitude<.01f)right=Vector3.Cross(ray.direction,Vector3.right);right.Normalize();var up=Vector3.Cross(right,ray.direction).normalized;
  int fired=0;
  foreach(var shot in handShots)if(shot.remaining<=0){
   Vector2 offset=Vector2.zero;if(pellets>1&&fired>0){float a=(fired-1)*Mathf.PI*.5f;offset=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.065f;}
   var direction=(ray.direction+right*offset.x+up*offset.y).normalized;
   shot.weapon=runWeapon;shot.damage=HandDamage(weapon.damage);shot.pierce=runWeapon==2?2:runWeapon==5?4:1;shot.hit.Clear();shot.remaining=Mathf.Max(.01f,(weapon.range-muzzle)/speed);shot.velocity=direction*speed;shot.go.transform.SetPositionAndRotation(origin,Quaternion.LookRotation(direction));shot.go.transform.localScale=IsGun(runWeapon)?new Vector3(.025f,.025f,.11f):Vector3.one*.065f;shot.go.SetActive(true);
   var block=new MaterialPropertyBlock();block.SetColor("_Color",weapon.color);shot.go.GetComponent<Renderer>().SetPropertyBlock(block);
   if(++fired==pellets)break;
  }
  handCooldown=weapon.interval*player.interval*HandInterval;if(IsGun(runWeapon)){recoil=.035f;Beam(origin,origin+ray.direction*.13f,weapon.color,.045f);}Cue(weapon.kind==HandWeaponKind.ArcPistol?FeedbackCue.Pistol:weapon.kind==HandWeaponKind.Scattergun?FeedbackCue.Shotgun:FeedbackCue.Cast,ray.origin);return true;
 }
 static Vector3 NearestOnSegment(Vector3 a,Vector3 b,Vector3 point){var d=b-a;return a+d*(d.sqrMagnitude>.000001f?Mathf.Clamp01(Vector3.Dot(point-a,d)/d.sqrMagnitude):0);}
 static Vector3 EnemyCenter(Enemy enemy)=>enemy.go.transform.position+Vector3.up*(SurvivorBestiary.Flying(enemy.type)?.12f:enemy.type==4?.42f:.2f);
 void HandShotStep(float dt){foreach(var shot in handShots){if(shot.remaining<=0)continue;var from=shot.go.transform.position;float travelTime=Mathf.Min(dt,shot.remaining);var to=from+shot.velocity*travelTime;bool wall=Physics.SphereCast(from,.035f,shot.velocity.normalized,out var wallHit,Vector3.Distance(from,to),1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore);if(wall)to=wallHit.point;shot.remaining-=travelTime;shot.go.transform.position=to;var targets=new List<Enemy>();foreach(var enemy in enemies)if(enemy.active&&!(shot.hit.TryGetValue(enemy,out var generation)&&generation==enemy.generation)&&Vector3.Distance(NearestOnSegment(from,to,EnemyCenter(enemy)),EnemyCenter(enemy))<enemy.radius+.075f)targets.Add(enemy);targets.Sort((a,b)=>Vector3.Distance(from,EnemyCenter(a)).CompareTo(Vector3.Distance(from,EnemyCenter(b))));foreach(var enemy in targets){shot.hit[enemy]=enemy.generation;var center=EnemyCenter(enemy);Damage(enemy,shot.damage);HandImpact(enemy,from,shot.weapon==7);if(screen!=ScreenState.Playing)return;if(shot.weapon==2&&enemy.active)enemy.slow=2;if(shot.weapon==5&&enemy.active)enemy.slow=1;if(shot.weapon==3){foreach(var other in enemies)if(other.active&&Vector3.Distance(center,EnemyCenter(other))<.75f){if(other!=enemy)Damage(other,shot.damage);if(other.active){other.burn=3;other.burnTick=.5f;}if(screen!=ScreenState.Playing)return;}skillVisuals.Emit(SkillVisual.Explosion,center,.4f,Quaternion.identity);}if(shot.weapon==4){int chains=0;foreach(var other in Targets(center,enemies.Length))if(other!=enemy&&other.active&&Vector3.Distance(EnemyCenter(other),center)<1.2f){skillVisuals.Bolt(center,EnemyCenter(other),false);Damage(other,Mathf.CeilToInt(shot.damage*.7f),false);if(screen!=ScreenState.Playing)return;if(++chains==2)break;}}if(--shot.pierce<=0){shot.remaining=0;break;}}if(wall)shot.remaining=0;if(shot.remaining<=0)shot.go.SetActive(false);}}
 public bool PurchaseHandWeapon(int i){if(screen!=ScreenState.Shop)return false;var backup=JsonUtility.FromJson<MetaSave>(JsonUtility.ToJson(meta));bool owned=meta.OwnsWeapon(i);if(!(owned?meta.EquipWeapon(i):meta.BuyWeapon(i))){notice="金币不足，或商品不可用";return false;}Save();if(!string.IsNullOrEmpty(saveError)){meta=backup;notice=saveError;return false;}notice=owned?"已装备 "+SurvivorArmory.Get(i).name:"已永久解锁并装备 "+SurvivorArmory.Get(i).name;feedback=4;Cue(FeedbackCue.Purchase,PlayerPosition);return true;}
 void SettleRun(bool victory){if(runSettled)return;runSettled=true;RecordRun(victory);lastReward=SurvivorRules.Reward(player.elapsed,player.kills,victory,meta.levels[6],meta.levels[7]);lastCoinReward=SurvivorArmory.CoinReward(player.elapsed,player.kills,victory)+EliteBounty+TacticalBounty+Mathf.FloorToInt(SurvivorArmory.CoinReward(player.elapsed,player.kills,victory)*trialRewardBonus);meta.crystals=(int)System.Math.Min(int.MaxValue,(long)meta.crystals+lastReward);meta.coins=(int)System.Math.Min(int.MaxValue,(long)meta.coins+lastCoinReward);Save();notice="旅程结算：金币 +"+lastCoinReward+"，晶核 +"+lastReward;feedback=6;}
}
}
