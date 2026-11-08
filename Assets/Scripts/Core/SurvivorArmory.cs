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
  new CreatureDefinition("苔芽精","Mossling","追逐时会在目标脚下生长荆棘，预警后造成范围伤害。","离开橙色荆棘圈；用飞剑清理近身群体。",40,8,.65f,1,.32f),
  new CreatureDefinition("余烬小鬼","EmberImp","先用橙线锁定目标位置，再发动直线冲锋。","看到冲锋预警后横向走位，冰系减速可阻止逼近。",30,7,1.2f,2,.33f),
  new CreatureDefinition("晶岩守卫","CrystalGuardian","周期生成吸收最大生命 20% 伤害的晶盾，持续 3 秒。","护盾到期再集中输出，或用高伤害武器击破晶盾。",140,18,.38f,3,.515f),
  new CreatureDefinition("月翼灵","MoonWisp","从天花板降落，落地后在目标位置施放月影法阵。","法阵锁定位置后不会跟随，及时离开预警范围。",55,11,.78f,4,.36f),
  new CreatureDefinition("古树守望者","ElderBloom","第 2、5、10 分钟挑战独立首领：盾矛守卫、月冠女王和古树守望者。拥有突刺、月环、荆棘和扇形弹，低血量进入狂怒。","红圈外缘即危险边界。荆棘锁定后及时移动；击败前两位首领恢复 20% 生命并获得晶核，最终首领被击败后通关。",6000,30,.48f,6,1.075f),
  new CreatureDefinition("星粉飞蛾","SkyMoth","拍翼悬浮，沿空中弧线追击并发射单枚星粉弹。","举起手部武器瞄准空中目标；自动技能也能命中。",35,7,.85f,2,.32f),
  new CreatureDefinition("雷翼幼龙","StormDrake","飞行追击，周期发射三枚扇形雷弹封锁前方。","用冰弹或连锁雷弹处理空中群体，避免贴脸。",85,13,1.05f,3,.4f),
  new CreatureDefinition("孢帽投手","Sporecap","发射孢子弹，同时在目标脚下留下短时毒区。","横向移动避开弹道，优先打断后排投手。",65,10,.58f,2,.37f),
  new CreatureDefinition("符叶祭司","RuneShaman","每次施法为 3 米内同伴恢复最大生命 10%，并发射符文弹。","优先击杀祭司，阻止敌群反复回血。",90,9,.52f,3,.38f),
  new CreatureDefinition("铁角甲虫","IronBeetle","厚重甲壳与长角，蓄力后沿锁定方向重装冲撞。","侧向躲过橙线，不要沿冲撞路径后退。",180,17,.48f,4,.43f),
  new CreatureDefinition("霜铃巫灵","FrostWitch","戴着冰晶冠，在目标脚下生成持续 2.5 秒的霜阵。","预警后离开固定霜阵，利用远程武器击杀。",100,14,.62f,4,.39f)
 };
 public static readonly int[] Experience={1,2,4,3,0,3,5,3,4,4,5};
 public static readonly float[] Radii={.1f,.1f,.21f,.135f,.475f,.14f,.18f,.15f,.15f,.21f,.16f};
 public static readonly string[] EliteSkills={"荆棘范围扩大，持续 3 秒", "冲锋蓄力更短、距离更远", "晶盾吸收最大生命 35% 的伤害", "同时释放两处月影法阵", "首领独立拥有三阶段技能", "同时发射三枚高速星粉弹", "扇形雷弹速度提升、施法更频繁", "三发孢子弹并留下持续毒区", "治疗提升至 18%，更频繁施法", "缩短蓄力，延长重装冲撞", "同时释放两处持续霜阵"};
 public static bool Flying(int type)=>type==5||type==6;
 public static string Evolution(UpgradeId id){switch(id){case UpgradeId.MYRIAD_SWORDS:return "进化条件：环绕飞剑 Lv.5 ＋ 全伤害 Lv.5";case UpgradeId.NINE_HEAVENS_THUNDER:return "进化条件：连锁闪电 Lv.5 ＋ 攻击速度 Lv.5";case UpgradeId.NETHER_POISON_DOMAIN:return "进化条件：毒雾光环 Lv.5 ＋ 攻击范围 Lv.5";case UpgradeId.METEOR_LAVA:return "进化条件：熔岩爆弹 Lv.5 ＋ 投射物数量 Lv.3";case UpgradeId.ABSOLUTE_ZERO:return "进化条件：穿透冰锥 Lv.5 ＋ 攻击速度 Lv.5";case UpgradeId.VOID_BLACK_HOLE:return "进化条件：引力黑洞 Lv.3 ＋ 攻击范围 Lv.5";default:return "升级时随机获得，可与其他魔法搭配。";}}
}
}
