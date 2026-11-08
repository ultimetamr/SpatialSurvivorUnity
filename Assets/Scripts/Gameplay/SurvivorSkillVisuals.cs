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
  if(!ink)ink=new Material(Shader.Find("Survivor/Additive"));int count=type==SkillVisual.ChainLightning||type==SkillVisual.HeavenlyThunder?2:type==SkillVisual.OrbitSword||type==SkillVisual.MyriadSwords?1:3;
  for(int i=0;i<count;i++){var go=new GameObject("Arcane ribbon "+i);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=ink;line.useWorldSpace=false;line.startColor=line.endColor=color;line.startWidth=line.endWidth=type==SkillVisual.HeavenlyThunder?.035f:.013f;line.numCornerVertices=2;line.numCapVertices=2;line.positionCount=49;lines.Add(line);}Tick(0);}
 public void Restart(){time=0;if(domain)domain.Restart();Tick(0);}
 public void Bolt(Vector3 end){for(int l=0;l<lines.Count;l++){var line=lines[l];line.positionCount=13;for(int i=0;i<13;i++){float t=i/12f;var offset=new Vector3(Mathf.Sin(i*2.6f+l*1.2f),Mathf.Cos(i*1.7f+l),0)*Mathf.Sin(t*Mathf.PI)*.09f;line.SetPosition(i,end*t+offset);}line.startColor=line.endColor=l==0?color:Color.white;}}
 public void Tick(float dt){if(domain){domain.Tick(dt);return;}time+=dt;if(kind==SkillVisual.ChainLightning||kind==SkillVisual.HeavenlyThunder)return;
  if(model&&(kind==SkillVisual.BlackHole||kind==SkillVisual.VoidHole||kind==SkillVisual.EnergyProjectile||kind==SkillVisual.LavaBomb||kind==SkillVisual.MeteorLava))model.localRotation=Quaternion.Euler(time*57,time*79,time*33);
  bool sword=kind==SkillVisual.OrbitSword||kind==SkillVisual.MyriadSwords||kind==SkillVisual.SwordRain;float radius=kind==SkillVisual.PoisonAura||kind==SkillVisual.NetherPoison||kind==SkillVisual.LightningField?1.35f:kind==SkillVisual.FreezePulse?.8f:kind==SkillVisual.Explosion?.45f:.28f;
  for(int l=0;l<lines.Count;l++){var line=lines[l];for(int i=0;i<line.positionCount;i++){float a=i/(float)(line.positionCount-1)*Mathf.PI*2+time*(l%2==0?1:-1)+l*2;Vector3 p;if(sword)p=new Vector3(Mathf.Sin(a)*.045f,Mathf.Cos(a)*.045f,-i/48f*.26f);else{float r=radius*(1+l*.13f);p=new Vector3(Mathf.Cos(a)*r,.045f+l*.055f+Mathf.Sin(a*3+time*2)*.025f,Mathf.Sin(a)*r);if(kind==SkillVisual.BlackHole||kind==SkillVisual.VoidHole)p=Quaternion.Euler(l*35,0,l*25)*p;}line.SetPosition(i,p);}}}
 public void Fade(float t){foreach(var line in lines){var c=color;c.a*=Mathf.Clamp01(t*4);line.startColor=line.endColor=c;}}
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
 public void Tick(float dt){foreach(var slot in pool)if(slot.remaining>0){slot.remaining=Mathf.Max(0,slot.remaining-dt);slot.effect.Tick(dt);slot.effect.Fade(slot.remaining/slot.duration);float t=1-slot.remaining/slot.duration;if(slot.kind==SkillVisual.SwordRain)slot.go.transform.position+=Vector3.down*dt*2.1f;if(slot.kind==SkillVisual.Explosion||slot.kind==SkillVisual.FreezePulse)slot.go.transform.localScale=Vector3.one*(.3f+t*2);if(slot.remaining<=0)slot.go.SetActive(false);}foreach(var go in new[]{poison,nether,field,magnet})if(go.activeSelf)go.GetComponent<SurvivorMagicEffect>().Tick(dt);foreach(var go in orbit)if(go.activeInHierarchy)go.GetComponent<SurvivorMagicEffect>().Tick(dt);foreach(var go in evolved)if(go.activeInHierarchy)go.GetComponent<SurvivorMagicEffect>().Tick(dt);}
 public void Refresh(PlayerState player,Vector3 position,bool visible) {
  bool poisonEvolved=player.Owns(UpgradeId.NETHER_POISON_DOMAIN);poison.SetActive(visible&&player.Owns(UpgradeId.POISON_AURA)&&!poisonEvolved);nether.SetActive(visible&&player.Owns(UpgradeId.POISON_AURA)&&poisonEvolved);
  float poisonRadius=player.range*1.35f*(poisonEvolved?2:1);
  foreach(var go in new[]{poison,nether}){go.transform.position=position+Vector3.up*.035f;go.GetComponent<SurvivorDomainField>().SetRadius(poisonRadius);}
  field.SetActive(visible&&player.Owns(UpgradeId.LIGHTNING_DOMAIN));field.transform.position=position+Vector3.up*.05f;field.GetComponent<SurvivorDomainField>().SetRadius(player.range*1.55f);
  magnet.SetActive(visible&&player.Owns(UpgradeId.MAGNETIC_FIELD));magnet.transform.position=position+Vector3.up*.065f;magnet.GetComponent<SurvivorDomainField>().SetRadius(player.pickup*2);
  for(int i=0;i<orbit.Length;i++){bool evo=player.Owns(UpgradeId.MYRIAD_SWORDS);orbit[i].SetActive(!evo);evolved[i].SetActive(evo);}
  if(victoryTime>0){victoryTime=Mathf.Max(0,victoryTime-Time.unscaledDeltaTime);victory.transform.position=position+Vector3.up*.2f;victory.transform.localScale=Vector3.one*(.36f/1.1f)*(1+5*(1-victoryTime));victory.SetActive(victoryTime>0);}
 }
 public void PulseDomain(bool lightning){var go=lightning?field:nether.activeSelf?nether:poison;go.GetComponent<SurvivorDomainField>().Pulse();}
 public void Victory(){victoryTime=1;victory.SetActive(true);}
}
}

