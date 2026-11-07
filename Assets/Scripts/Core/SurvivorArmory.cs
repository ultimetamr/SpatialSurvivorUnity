using System;
using UnityEngine;
namespace SpatialSurvivor {
public enum HandWeaponKind { StarWand, Runeblade, FrostStaff, EmberStaff, StormWand, VoidScepter, ArcPistol, Scattergun }
public sealed class HandWeaponDefinition {
 public readonly HandWeaponKind kind;public readonly string name,description,model;public readonly int cost,damage,icon;public readonly float interval,range;public readonly Color color;
 public HandWeaponDefinition(HandWeaponKind kind,string name,string description,string model,int cost,int damage,float interval,float range,int icon,Color color){this.kind=kind;this.name=name;this.description=description;this.model=model;this.cost=cost;this.damage=damage;this.interval=interval;this.range=range;this.icon=icon;this.color=color;}
}
public static class SurvivorArmory {
 public static readonly HandWeaponDefinition[] Weapons={
  new HandWeaponDefinition(HandWeaponKind.StarWand,"星光魔杖","入门魔杖。沿手部朝向射出星光弹，可按住扳机连续施法。","StarWand",0,18,.5f,7,1,new Color(.5f,.85f,1)),
  new HandWeaponDefinition(HandWeaponKind.Runeblade,"符文短剑","直接挥剑击打生物，连续检测整段剑刃扫过的轨迹；也可按扳机斩击。同一目标有独立命中间隔。","HandRuneblade",80,34,.48f,.9f,0,new Color(.6f,.95f,.85f)),
  new HandWeaponDefinition(HandWeaponKind.FrostStaff,"霜晶法杖","发射穿透两只生物的冰弹，减速 30% 持续两秒。","FrostStaff",140,24,.65f,8,4,new Color(.55f,.85f,1)),
  new HandWeaponDefinition(HandWeaponKind.EmberStaff,"焰种法杖","火弹命中后在 0.75 米范围爆炸，造成伤害并附加灼烧。","EmberStaff",200,30,.85f,7,5,new Color(1,.55f,.25f)),
  new HandWeaponDefinition(HandWeaponKind.StormWand,"雷羽魔杖","快速发射雷弹，命中后向附近最多两只生物连锁。","StormWand",280,19,.3f,8,3,new Color(.8f,.65f,1)),
  new HandWeaponDefinition(HandWeaponKind.VoidScepter,"星环权杖","高伤害穿透弹可贯穿四只生物；命中附加短暂减速。","VoidScepter",400,40,.75f,9,6,new Color(.8f,.5f,1)),
  new HandWeaponDefinition(HandWeaponKind.ArcPistol,"星火手枪","免费领取。抬起手柄瞄准并按住扳机，发射高速单发子弹；命中地面或飞行生物均可造成伤害和击杀。","ArcPistol",0,32,.28f,12,1,new Color(.35f,.9f,1)),
  new HandWeaponDefinition(HandWeaponKind.Scattergun,"符文霰弹枪","每次发射五枚散射弹，每枚独立伤害；近距离集中命中威力更高，可攻击地面与空中目标。","Scattergun",240,14,.8f,8,5,new Color(1,.7f,.32f))
 };
 public static HandWeaponDefinition Get(int index)=>Weapons[Mathf.Clamp(index,0,Weapons.Length-1)];
 public static int CoinReward(float seconds,int kills,bool victory)=>Mathf.Max(0,kills)*3+Mathf.FloorToInt(Mathf.Max(0,seconds)/10)*2+Mathf.FloorToInt(Mathf.Max(0,seconds)/120)*20+(victory?120:0);
 public static bool ValidIndex(int i)=>i>=0&&i<Weapons.Length;
}
public sealed class CreatureDefinition {
 public readonly string name,model,description,tactic;public readonly int health,damage,wave;public readonly float speed,scale;
 public bool Flying=>wave>0&&(model=="SkyMoth"||model=="StormDrake");
 public CreatureDefinition(string name,string model,string description,string tactic,int hp,int damage,float speed,int wave,float scale){this.name=name;this.model=model;this.description=description;this.tactic=tactic;health=hp;this.damage=damage;this.speed=speed;this.wave=wave;this.scale=scale;}
}
public static class SurvivorBestiary {
 public static readonly CreatureDefinition[] Creatures={
  new CreatureDefinition("苔芽精","Mossling","成群沿地面追逐的小生物，会绕开同伴和家具。","保持走位，让飞剑或毒雾处理近身群体。",40,8,.65f,1,.32f),
  new CreatureDefinition("余烬小鬼","EmberImp","动作轻快，快速逼近玩家并持续近战攻击。","优先处理靠近的一只；冰系减速可以拉开距离。",30,7,1.2f,2,.33f),
  new CreatureDefinition("晶岩守卫","CrystalGuardian","移动缓慢但生命较高，身体占据更大的空间。","利用穿透和持续伤害，同时注意它身后的快怪。",140,18,.38f,3,.515f),
  new CreatureDefinition("月翼灵","MoonWisp","从天花板落入房间，落地后加入地面追击。","留意降落点，避免被突然落下的生物包围。",55,11,.78f,4,.36f),
  new CreatureDefinition("古树守望者","ElderBloom","十分钟时登场。周期性蓄力后发动 2.5 米范围冲击。","看见红色预警环时离开范围，蓄力结束后继续输出。",6000,30,.48f,6,1.075f),
  new CreatureDefinition("星粉飞蛾","SkyMoth","第二波开始出现，拍翼悬浮，沿空中弧线追击。","举起手部武器瞄准空中目标；自动技能也能命中。",35,7,.85f,2,.32f),
  new CreatureDefinition("雷翼幼龙","StormDrake","第三波开始出现，速度更快、生命更高，会从空中逼近。","用冰弹或连锁雷弹处理空中群体，避免贴脸。",85,13,1.05f,3,.4f)
 };
 public static bool Flying(int type)=>type==5||type==6;
 public static string Evolution(UpgradeId id){switch(id){case UpgradeId.MYRIAD_SWORDS:return "进化条件：环绕飞剑 Lv.5 ＋ 全伤害 Lv.5";case UpgradeId.NINE_HEAVENS_THUNDER:return "进化条件：连锁闪电 Lv.5 ＋ 攻击速度 Lv.5";case UpgradeId.NETHER_POISON_DOMAIN:return "进化条件：毒雾光环 Lv.5 ＋ 攻击范围 Lv.5";case UpgradeId.METEOR_LAVA:return "进化条件：熔岩爆弹 Lv.5 ＋ 投射物数量 Lv.3";case UpgradeId.ABSOLUTE_ZERO:return "进化条件：穿透冰锥 Lv.5 ＋ 攻击速度 Lv.5";case UpgradeId.VOID_BLACK_HOLE:return "进化条件：引力黑洞 Lv.3 ＋ 攻击范围 Lv.5";default:return "升级时随机获得，可与其他魔法搭配。";}}
}
}
