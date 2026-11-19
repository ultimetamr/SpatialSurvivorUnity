using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialSurvivor {
public enum SkillVisual { OrbitSword, PoisonAura, ChainLightning, IceSpike, LavaBomb, BlackHole, SwordRain, LightningField, MyriadSwords, HeavenlyThunder, NetherPoison, MeteorLava, AbsoluteZero, VoidHole, FreezePulse, Explosion, EnergyProjectile, MagneticField }

public static class SurvivorVisualCatalog {
 public static readonly string[] Models={"Runeblade",null,null,"FrostCrystal","EmberSeed","ArcaneCore","Runeblade",null,"Runeblade",null,null,"EmberSeed","FrostCrystal","ArcaneCore",null,null,"ArcaneCore",null};
 public static GameObject Create(SkillVisual kind,Transform parent){var root=new GameObject(kind.ToString());root.transform.SetParent(parent,false);var name=Models[(int)kind];if(name!=null){var model=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Art/Weapons/"+name),root.transform);model.transform.localScale=Vector3.one*(kind==SkillVisual.EnergyProjectile?.42f:kind==SkillVisual.OrbitSword?.52f:kind==SkillVisual.MyriadSwords?.72f:1);}root.AddComponent<SurvivorMagicEffect>().Initialize(kind);return root;}
}
public sealed class SurvivorMagicEffect:MonoBehaviour {
 SurvivorDomainField domain;SkillVisual kind;readonly List<LineRenderer> lines=new List<LineRenderer>();Transform model;float time;Color color;static Material ink;
 public void Initialize(SkillVisual type){kind=type;model=transform.childCount>0?transform.GetChild(0):null;color=type==SkillVisual.PoisonAura||type==SkillVisual.NetherPoison?new Color(.48f,.94f,.57f,.62f):type==SkillVisual.LavaBomb||type==SkillVisual.MeteorLava||type==SkillVisual.Explosion?new Color(1,.61f,.25f,.85f):type==SkillVisual.BlackHole||type==SkillVisual.VoidHole?new Color(.75f,.55f,1,.8f):new Color(.42f,.85f,1,.85f);
  if(type==SkillVisual.PoisonAura||type==SkillVisual.NetherPoison||type==SkillVisual.LightningField||type==SkillVisual.MagneticField){domain=gameObject.AddComponent<SurvivorDomainField>();domain.Initialize(type);return;}
  if(!ink)ink=new Material(Resources.Load<Shader>("Shaders/MagicRibbon"));int count=type==SkillVisual.ChainLightning||type==SkillVisual.HeavenlyThunder?2:type==SkillVisual.OrbitSword||type==SkillVisual.MyriadSwords?1:type==SkillVisual.Explosion||type==SkillVisual.FreezePulse?9:3;
  for(int i=0;i<count;i++){var go=new GameObject("Arcane ribbon "+i);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=ink;line.useWorldSpace=false;line.startColor=line.endColor=color;bool bolt=type==SkillVisual.ChainLightning||type==SkillVisual.HeavenlyThunder;
   line.widthMultiplier=bolt?(i==0?.014f:.045f):type==SkillVisual.Explosion?.024f:.016f;
   line.widthCurve=bolt||type==SkillVisual.OrbitSword||type==SkillVisual.MyriadSwords||type==SkillVisual.SwordRain?
    new AnimationCurve(new Keyframe(0,.25f),new Keyframe(.15f,1),new Keyframe(.65f,.7f),new Keyframe(1,0)):
    new AnimationCurve(new Keyframe(0,.7f),new Keyframe(.25f,1),new Keyframe(.5f,.7f),new Keyframe(.75f,1),new Keyframe(1,.7f));line.numCornerVertices=2;line.numCapVertices=2;line.positionCount=(type==SkillVisual.Explosion||type==SkillVisual.FreezePulse)&&i>=3?3:49;lines.Add(line);}Tick(0);}
 public void Restart(){time=0;if(domain)domain.Restart();Fade(1);Tick(0);}
 public void Bolt(Vector3 end){for(int l=0;l<lines.Count;l++){var line=lines[l];line.positionCount=13;for(int i=0;i<13;i++){float t=i/12f;var offset=new Vector3(Mathf.Sin(i*2.6f+l*1.2f),Mathf.Cos(i*1.7f+l),0)*Mathf.Sin(t*Mathf.PI)*.09f;line.SetPosition(i,i==0?Vector3.zero:i==12?end:end*t+offset);}SetLineColor(line,l,1);}}
 void SetLineColor(LineRenderer line,int index,float opacity){
  bool bolt=kind==SkillVisual.ChainLightning||kind==SkillVisual.HeavenlyThunder;
  var start=Color.Lerp(color,Color.white,bolt&&index==0?.86f:.3f);start.a=color.a*opacity*(bolt&&index==1?.28f:1);
  var end=color;end.a=start.a*(bolt?.25f:kind==SkillVisual.OrbitSword||kind==SkillVisual.MyriadSwords||kind==SkillVisual.SwordRain?0:.7f);
  line.startColor=start;line.endColor=end;
 }
 public void Tick(float dt){if(domain){domain.Tick(dt);return;}time+=dt;if(kind==SkillVisual.ChainLightning||kind==SkillVisual.HeavenlyThunder)return;
  if(model&&(kind==SkillVisual.BlackHole||kind==SkillVisual.VoidHole||kind==SkillVisual.EnergyProjectile||kind==SkillVisual.LavaBomb||kind==SkillVisual.MeteorLava))model.localRotation=Quaternion.Euler(time*57,time*79,time*33);
  bool sword=kind==SkillVisual.OrbitSword||kind==SkillVisual.MyriadSwords||kind==SkillVisual.SwordRain;float radius=kind==SkillVisual.PoisonAura||kind==SkillVisual.NetherPoison||kind==SkillVisual.LightningField?1.35f:kind==SkillVisual.FreezePulse?.8f:kind==SkillVisual.Explosion?.45f:.28f;
  for(int l=0;l<lines.Count;l++){var line=lines[l];float pulse=1+Mathf.Sin(time*3+l*2)*.12f;
   if((kind==SkillVisual.Explosion||kind==SkillVisual.FreezePulse)&&l>=3){
    float angle=(l-3)*Mathf.PI/3;var direction=new Vector3(Mathf.Cos(angle),.15f+(l%2)*.3f,Mathf.Sin(angle)).normalized;
    float travel=.16f+time*.6f;
    line.widthMultiplier=kind==SkillVisual.Explosion?.034f:.023f;
    for(int i=0;i<3;i++)line.SetPosition(i,direction*(travel+i*.11f)+Vector3.up*.07f);
    continue;
   }
   if(!sword)line.widthMultiplier=(kind==SkillVisual.Explosion?.024f:.016f)*pulse;
   for(int i=0;i<line.positionCount;i++){float a=i/(float)(line.positionCount-1)*Mathf.PI*2+time*(l%2==0?1:-1)+l*2;Vector3 p;if(sword)p=new Vector3(Mathf.Sin(a)*.034f,Mathf.Cos(a)*.034f,-i/48f*.4f);else{float r=radius*(1+l*.18f)*pulse;p=new Vector3(Mathf.Cos(a)*r,.045f+l*.055f+Mathf.Sin(a*3+time*2)*.025f,Mathf.Sin(a)*r);if(kind==SkillVisual.BlackHole||kind==SkillVisual.VoidHole)p=Quaternion.Euler(l*48,0,l*38)*p;
    else if(kind==SkillVisual.LavaBomb||kind==SkillVisual.MeteorLava||kind==SkillVisual.EnergyProjectile)p=Quaternion.Euler(l*60,time*21,l*35)*p;}line.SetPosition(i,p);}}}
 public void Fade(float t){for(int i=0;i<lines.Count;i++)SetLineColor(lines[i],i,Mathf.Clamp01(t*4));}
}

public sealed class SurvivorSkillVisuals:MonoBehaviour {
 sealed class Slot {public GameObject go;public SurvivorMagicEffect effect;public SkillVisual kind;public float remaining,duration;}
 readonly List<Slot> pool=new List<Slot>();GameObject poison,nether,field,magnet,victory;float victoryTime;
 readonly GameObject[] orbit=new GameObject[14],evolved=new GameObject[14];
 public bool ReducedEffects {get;set;}
 public int Capacity=>pool.Count;
 public int ActiveCount {get{int count=0;foreach(var slot in pool)if(slot.remaining>0)count++;return count;}}
 public void Initialize(Transform[] swords) {
  for(int i=0;i<swords.Length;i++){orbit[i]=SurvivorVisualCatalog.Create(SkillVisual.OrbitSword,swords[i]);evolved[i]=SurvivorVisualCatalog.Create(SkillVisual.MyriadSwords,swords[i]);evolved[i].SetActive(false);}
  poison=SurvivorVisualCatalog.Create(SkillVisual.PoisonAura,transform);nether=SurvivorVisualCatalog.Create(SkillVisual.NetherPoison,transform);field=SurvivorVisualCatalog.Create(SkillVisual.LightningField,transform);magnet=SurvivorVisualCatalog.Create(SkillVisual.MagneticField,transform);
  var kinds=new[]{SkillVisual.IceSpike,SkillVisual.AbsoluteZero,SkillVisual.LavaBomb,SkillVisual.MeteorLava,SkillVisual.BlackHole,SkillVisual.VoidHole,SkillVisual.SwordRain,SkillVisual.ChainLightning,SkillVisual.HeavenlyThunder,SkillVisual.FreezePulse,SkillVisual.Explosion};
  var counts=new[]{6,19,4,4,2,2,8,8,6,1,4};
  for(int k=0;k<kinds.Length;k++)for(int i=0;i<counts[k];i++){var go=SurvivorVisualCatalog.Create(kinds[k],transform);go.SetActive(false);pool.Add(new Slot{go=go,effect=go.GetComponent<SurvivorMagicEffect>(),kind=kinds[k]});}
  victory=SurvivorVisualCatalog.Create(SkillVisual.Explosion,transform);victory.name="Victory Gold Light";ResetVisuals();
 }
 public void ResetVisuals(){foreach(var slot in pool){slot.remaining=0;slot.go.SetActive(false);}if(poison){poison.SetActive(false);poison.GetComponent<SurvivorMagicEffect>().Restart();}if(nether){nether.SetActive(false);nether.GetComponent<SurvivorMagicEffect>().Restart();}if(field){field.SetActive(false);field.GetComponent<SurvivorMagicEffect>().Restart();}if(magnet){magnet.SetActive(false);magnet.GetComponent<SurvivorMagicEffect>().Restart();}if(victory)victory.SetActive(false);victoryTime=0;}
 public bool Emit(SkillVisual kind,Vector3 position,float duration,Quaternion rotation) {
  if(ReducedEffects&&ActiveCount>=10)return false;
  Slot selected=null;foreach(var slot in pool)if(slot.kind==kind&&slot.remaining<=0){selected=slot;break;}
  if(selected==null)return false;selected.remaining=selected.duration=duration;selected.go.transform.SetPositionAndRotation(position,kind==SkillVisual.SwordRain?SurvivorBladePose.RainRotation:rotation);selected.go.transform.localScale=Vector3.one;selected.go.SetActive(true);selected.effect.Restart();return true;
 }
 public void Bolt(Vector3 from,Vector3 to,bool evolution){var kind=evolution?SkillVisual.HeavenlyThunder:SkillVisual.ChainLightning;if(Emit(kind,from,.28f,Quaternion.identity)){foreach(var slot in pool)if(slot.kind==kind&&slot.remaining==.28f&&slot.go.transform.position==from){slot.effect.Bolt(to-from);break;}}}
 public void Tick(float dt){foreach(var slot in pool)if(slot.remaining>0){slot.remaining=Mathf.Max(0,slot.remaining-dt);slot.effect.Tick(dt);slot.effect.Fade(slot.remaining/slot.duration);float t=1-slot.remaining/slot.duration;if(slot.kind==SkillVisual.SwordRain)slot.go.transform.position+=Vector3.down*dt*2.1f;if(slot.kind==SkillVisual.Explosion||slot.kind==SkillVisual.FreezePulse)slot.go.transform.localScale=Vector3.one*(.3f+t*2);if(slot.remaining<=0)slot.go.SetActive(false);}TickDomain(poison,dt);TickDomain(nether,dt);TickDomain(field,dt);TickDomain(magnet,dt);foreach(var go in orbit)if(go.activeInHierarchy)go.GetComponent<SurvivorMagicEffect>().Tick(dt);foreach(var go in evolved)if(go.activeInHierarchy)go.GetComponent<SurvivorMagicEffect>().Tick(dt);}
 static void TickDomain(GameObject go,float dt){if(go.activeSelf)go.GetComponent<SurvivorMagicEffect>().Tick(dt);}
 public void Refresh(PlayerState player,Vector3 position,bool visible) {
  bool poisonEvolved=player.Owns(UpgradeId.NETHER_POISON_DOMAIN);poison.SetActive(visible&&player.Owns(UpgradeId.POISON_AURA)&&!poisonEvolved);nether.SetActive(visible&&player.Owns(UpgradeId.POISON_AURA)&&poisonEvolved);
  float poisonRadius=player.range*1.35f*(poisonEvolved?2:1);
  SetPoison(poison,position,poisonRadius);SetPoison(nether,position,poisonRadius);
  field.SetActive(visible&&player.Owns(UpgradeId.LIGHTNING_DOMAIN));field.transform.position=position+Vector3.up*.05f;field.GetComponent<SurvivorDomainField>().SetRadius(player.range*1.55f);
  magnet.SetActive(visible&&player.Owns(UpgradeId.MAGNETIC_FIELD));magnet.transform.position=position+Vector3.up*.065f;magnet.GetComponent<SurvivorDomainField>().SetRadius(player.pickup*2);
  for(int i=0;i<orbit.Length;i++){bool evo=player.Owns(UpgradeId.MYRIAD_SWORDS);orbit[i].SetActive(!evo);evolved[i].SetActive(evo);}
  if(victoryTime>0){victoryTime=Mathf.Max(0,victoryTime-Time.unscaledDeltaTime);victory.transform.position=position+Vector3.up*.2f;victory.transform.localScale=Vector3.one*(.36f/1.1f)*(1+5*(1-victoryTime));victory.SetActive(victoryTime>0);}
 }
 static void SetPoison(GameObject go,Vector3 position,float radius){go.transform.position=position+Vector3.up*.035f;go.GetComponent<SurvivorDomainField>().SetRadius(radius);}
 public void PulseDomain(bool lightning){var go=lightning?field:nether.activeSelf?nether:poison;go.GetComponent<SurvivorDomainField>().Pulse();}
 public void Victory(){victoryTime=1;victory.SetActive(true);}
}
}

