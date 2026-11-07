using System;
using System.Collections.Generic;
using UnityEngine;
namespace SpatialSurvivor {
public enum UpgradeId { ORBITING_SWORD, ENERGY_PROJECTILE, POISON_AURA, CHAIN_LIGHTNING, PIERCING_ICE_CONE, LAVA_BOMB, GRAVITY_BLACK_HOLE, SWORD_RAIN, LIGHTNING_DOMAIN, ATTACK_DAMAGE, ATTACK_SPEED, ATTACK_RANGE, PICKUP_RANGE, EXPERIENCE_GAIN, MAX_HEALTH, HEALTH_REGENERATION, PROJECTILE_COUNT, PIERCE_COUNT, CRITICAL_CHANCE, DAMAGE_REDUCTION, CRITICAL_DAMAGE, DODGE_CHANCE, REGENERATING_SHIELD, EXPERIENCE_MAGNET, KILL_HEAL, MAGNETIC_FIELD, EXPLOSIVE_REMAINS, FREEZE_PULSE, REROLL_MASTER, NEAR_DEATH_PROTECTION, EXTRA_OPTION, DOUBLE_CRYSTAL, GATHERING_AURA, MYRIAD_SWORDS, NINE_HEAVENS_THUNDER, NETHER_POISON_DOMAIN, METEOR_LAVA, ABSOLUTE_ZERO, VOID_BLACK_HOLE, MOVEMENT_SPEED }
public enum Rarity { COMMON, RARE, EPIC, LEGENDARY }
[Serializable] public class Card {
 public UpgradeId id; public Rarity rarity; public string title, description; public int maxLevel;
 public Card(UpgradeId id,Rarity rarity,string title,string description,int maxLevel){this.id=id;this.rarity=rarity;this.title=title;this.description=description;this.maxLevel=maxLevel;}
}
public static class SurvivorRules {
 public const float Step=1f/90f, BossTime=600, WaveDuration=120;
 public static int Wave(float t)=>Mathf.Clamp(Mathf.FloorToInt(Mathf.Max(0,t)/120)+1,1,10);
 public static int MonsterLimit(int wave)=>Mathf.Min(18,Mathf.CeilToInt(12*Mathf.Pow(1.15f,Mathf.Max(0,wave-1))));
 public static int RequiredExp(int previous)=>Mathf.Max(previous+1,Mathf.CeilToInt(Mathf.Max(1,previous)*1.5f));
 public static int Reward(float seconds,int kills,bool victory,int income,int waveBonus)=>Mathf.FloorToInt(((int)(Mathf.Max(0,seconds)/60)*5+(Mathf.Max(0,kills)/10)*2+Mathf.FloorToInt((int)(seconds/120)*10*(1+Mathf.Max(0,waveBonus)*.15f)+.5f)+(victory?100:0))*(1+Mathf.Max(0,income)*.1f)+.5f);
 public static readonly Card[] Cards={
            new Card(UpgradeId.ORBITING_SWORD, Rarity.COMMON, "环绕飞剑", "3把飞剑环绕玩家，接触造成单体伤害。", 5),
            new Card(UpgradeId.ENERGY_PROJECTILE, Rarity.COMMON, "能量弹", "自动锁定最近怪物发射直线能量弹。", 5),
            new Card(UpgradeId.POISON_AURA, Rarity.COMMON, "毒雾光环", "玩家周身持续造成毒素范围伤害。", 5),
            new Card(UpgradeId.ATTACK_DAMAGE, Rarity.COMMON, "全伤害 +20%", "所有武器伤害提高20%。", 5),
            new Card(UpgradeId.ATTACK_SPEED, Rarity.COMMON, "攻击速度 +15%", "所有自动攻击间隔缩短。", 5),
            new Card(UpgradeId.ATTACK_RANGE, Rarity.COMMON, "攻击范围 +15%", "锁定与范围技能距离提高15%。", 5),
            new Card(UpgradeId.PICKUP_RANGE, Rarity.COMMON, "拾取范围 +0.3米", "经验晶石吸附距离扩大。", 5),
            new Card(UpgradeId.EXPERIENCE_GAIN, Rarity.COMMON, "经验获取 +20%", "晶石提供的经验提高20%。", 5),
            new Card(UpgradeId.MAX_HEALTH, Rarity.COMMON, "最大生命 +25", "最大生命和当前生命增加25。", 4),
            new Card(UpgradeId.HEALTH_REGENERATION, Rarity.COMMON, "生命恢复 +1", "每秒恢复1点生命。", 5),
            new Card(UpgradeId.EXPERIENCE_MAGNET, Rarity.COMMON, "经验磁铁", "晶石生成后会缓慢飘向玩家。", 1),
            new Card(UpgradeId.KILL_HEAL, Rarity.COMMON, "击杀回血", "每击杀怪物恢复0.5生命。", 1),
            new Card(UpgradeId.CHAIN_LIGHTNING, Rarity.RARE, "连锁闪电", "命中后弹射3个敌人，每次衰减20%。", 5),
            new Card(UpgradeId.PIERCING_ICE_CONE, Rarity.RARE, "穿透冰锥", "贯穿3个敌人并减速30%，持续2秒。", 5),
            new Card(UpgradeId.LAVA_BOMB, Rarity.RARE, "熔岩爆弹", "爆炸范围伤害并附加3秒灼烧。", 5),
            new Card(UpgradeId.PROJECTILE_COUNT, Rarity.RARE, "投射物数量 +1", "每轮自动攻击额外发射1个投射物。", 3),
            new Card(UpgradeId.PIERCE_COUNT, Rarity.RARE, "穿透次数 +1", "投射物额外贯穿1个目标。", 3),
            new Card(UpgradeId.CRITICAL_CHANCE, Rarity.RARE, "暴击概率 +10%", "攻击暴击概率提高10%。", 5),
            new Card(UpgradeId.DAMAGE_REDUCTION, Rarity.RARE, "伤害减免 +10%", "受到的伤害降低10%。", 5),
            new Card(UpgradeId.MAGNETIC_FIELD, Rarity.RARE, "磁吸领域", "更远处晶石也会缓慢吸附。", 1),
            new Card(UpgradeId.EXPLOSIVE_REMAINS, Rarity.RARE, "爆炸残骸", "怪物死亡时对周围敌人造成爆炸伤害。", 1),
            new Card(UpgradeId.FREEZE_PULSE, Rarity.RARE, "冻结脉冲", "每次升级确认后冻结全场怪物2秒。", 1),
            new Card(UpgradeId.REROLL_MASTER, Rarity.RARE, "重roll大师", "每次升级获得1次刷新卡牌机会。", 1),
            new Card(UpgradeId.GRAVITY_BLACK_HOLE, Rarity.EPIC, "引力黑洞", "每5秒生成黑洞牵引并持续伤害。", 3),
            new Card(UpgradeId.SWORD_RAIN, Rarity.EPIC, "剑雨轰击", "每4秒从上空进行大范围多段轰击。", 3),
            new Card(UpgradeId.LIGHTNING_DOMAIN, Rarity.EPIC, "雷电领域", "常驻雷击领域并有10%概率麻痹。", 3),
            new Card(UpgradeId.CRITICAL_DAMAGE, Rarity.EPIC, "暴击伤害 +50%", "暴击额外伤害提高50%。", 3),
            new Card(UpgradeId.DODGE_CHANCE, Rarity.EPIC, "闪避概率 +10%", "获得10%闪避概率。", 3),
            new Card(UpgradeId.REGENERATING_SHIELD, Rarity.EPIC, "再生护盾", "获得20%最大生命护盾，脱战5秒恢复。", 3),
            new Card(UpgradeId.NEAR_DEATH_PROTECTION, Rarity.EPIC, "濒死庇护", "低于20%生命时获得50%减伤3秒。", 1),
            new Card(UpgradeId.EXTRA_OPTION, Rarity.EPIC, "额外选项", "后续升级弹窗由三选一变为四选一。", 1),
            new Card(UpgradeId.DOUBLE_CRYSTAL, Rarity.EPIC, "双倍晶石", "怪物有15%概率掉落双倍经验晶石。", 1),
            new Card(UpgradeId.GATHERING_AURA, Rarity.EPIC, "聚怪光环", "怪物会更积极地向玩家聚拢。", 1),
            new Card(UpgradeId.MYRIAD_SWORDS, Rarity.LEGENDARY, "万剑归宗", "飞剑翻倍、无限穿透，总伤害+120%。", 1),
            new Card(UpgradeId.NINE_HEAVENS_THUNDER, Rarity.LEGENDARY, "九天雷劫", "弹射6次且无衰减，30%概率麻痹1秒。", 1),
            new Card(UpgradeId.NETHER_POISON_DOMAIN, Rarity.LEGENDARY, "玄冥毒域", "范围和毒伤翻倍，敌人移速减半。", 1),
            new Card(UpgradeId.METEOR_LAVA, Rarity.LEGENDARY, "陨星熔岩", "一次发射3颗爆弹，爆炸范围+50%。", 1),
            new Card(UpgradeId.ABSOLUTE_ZERO, Rarity.LEGENDARY, "绝对零度", "无限穿透、冻结1秒并使目标受伤+50%。", 1),
            new Card(UpgradeId.VOID_BLACK_HOLE, Rarity.LEGENDARY, "虚空黑洞", "范围+80%、牵引翻倍、持续时间+2秒。", 1),
 };
 public static Card Definition(UpgradeId id)=>Array.Find(Cards,c=>c.id==id);
 public static readonly UpgradeId[] Weapons={UpgradeId.ORBITING_SWORD,UpgradeId.ENERGY_PROJECTILE,UpgradeId.POISON_AURA,UpgradeId.CHAIN_LIGHTNING,UpgradeId.PIERCING_ICE_CONE,UpgradeId.LAVA_BOMB,UpgradeId.GRAVITY_BLACK_HOLE,UpgradeId.SWORD_RAIN,UpgradeId.LIGHTNING_DOMAIN};
 public static List<UpgradeId> Evolutions(PlayerState p){var r=new List<UpgradeId>();Ready(r,p,UpgradeId.ORBITING_SWORD,5,UpgradeId.ATTACK_DAMAGE,5,UpgradeId.MYRIAD_SWORDS);Ready(r,p,UpgradeId.CHAIN_LIGHTNING,5,UpgradeId.ATTACK_SPEED,5,UpgradeId.NINE_HEAVENS_THUNDER);Ready(r,p,UpgradeId.POISON_AURA,5,UpgradeId.ATTACK_RANGE,5,UpgradeId.NETHER_POISON_DOMAIN);Ready(r,p,UpgradeId.LAVA_BOMB,5,UpgradeId.PROJECTILE_COUNT,3,UpgradeId.METEOR_LAVA);Ready(r,p,UpgradeId.PIERCING_ICE_CONE,5,UpgradeId.ATTACK_SPEED,5,UpgradeId.ABSOLUTE_ZERO);Ready(r,p,UpgradeId.GRAVITY_BLACK_HOLE,3,UpgradeId.ATTACK_RANGE,5,UpgradeId.VOID_BLACK_HOLE);return r;}
 static void Ready(List<UpgradeId> list,PlayerState p,UpgradeId w,int wl,UpgradeId s,int sl,UpgradeId evo){if(p.Stack(w)>=wl&&p.Stack(s)>=sl&&!p.Owns(evo))list.Add(evo);}
 public static List<Card> Draw(PlayerState p,System.Random rng){var pool=new List<Card>();bool all=true;foreach(var w in Weapons)all&=p.Owns(w);foreach(var c in Cards)if(c.rarity!=Rarity.LEGENDARY&&p.Stack(c.id)<c.maxLevel&&!(all&&Array.IndexOf(Weapons,c.id)>=0))pool.Add(c);var result=new List<Card>();var ev=Evolutions(p);if(ev.Count>0)result.Add(Definition(ev[rng.Next(ev.Count)]));int count=p.Owns(UpgradeId.EXTRA_OPTION)?4:3;while(result.Count<count&&pool.Count>0){var c=Pick(pool,p.completed,rng);result.Add(c);pool.Remove(c);}if(p.completed>=9&&!result.Exists(c=>c.rarity!=Rarity.COMMON)){pool.RemoveAll(c=>c.rarity==Rarity.COMMON);if(pool.Count>0&&result.Count>0)result[result.Count-1]=Pick(pool,p.completed,rng);}return result;}
 static Card Pick(List<Card> pool,int completed,System.Random rng){int step=completed/3;int[] weights={Mathf.Max(5,70-5*step),25+3*step,4+2*step,1};int total=0;for(int i=0;i<4;i++)if(pool.Exists(c=>(int)c.rarity==i))total+=weights[i];int roll=rng.Next(total),rarity=0;for(int i=0;i<4;i++){if(!pool.Exists(c=>(int)c.rarity==i))continue;roll-=weights[i];if(roll<0){rarity=i;break;}}var group=pool.FindAll(c=>(int)c.rarity==rarity);return group[rng.Next(group.Count)];}
}
[Serializable] public class PlayerState {
 public float health=100,maxHealth=100,range=2,interval=1,pickup=.5f,experienceMultiplier=1,regeneration,critChance,critMultiplier=2,damageReduction,dodge,shield;
 public int damage=20,projectiles=1,pierce,level=1,experience,required=5,completed,kills; public float elapsed;
 public int[] stacks=new int[Enum.GetValues(typeof(UpgradeId)).Length];
 public int Stack(UpgradeId id)=>stacks[(int)id]; public bool Owns(UpgradeId id)=>Stack(id)>0;
 public void Apply(UpgradeId id){var c=SurvivorRules.Definition(id);if(c==null||Stack(id)>=c.maxLevel)return;stacks[(int)id]++;completed++;switch(id){
 case UpgradeId.ATTACK_DAMAGE:damage=Mathf.CeilToInt(damage*1.2f);break;case UpgradeId.ATTACK_SPEED:interval=Mathf.Max(.12f,interval/1.15f);break;case UpgradeId.ATTACK_RANGE:range=Mathf.Min(8,range*1.15f);break;case UpgradeId.PICKUP_RANGE:pickup=Mathf.Min(8,pickup+.3f);break;case UpgradeId.EXPERIENCE_GAIN:experienceMultiplier*=1.2f;break;case UpgradeId.MAX_HEALTH:maxHealth+=25;health=Mathf.Min(maxHealth,health+25);break;case UpgradeId.HEALTH_REGENERATION:regeneration++;break;case UpgradeId.PROJECTILE_COUNT:projectiles++;break;case UpgradeId.PIERCE_COUNT:pierce++;break;case UpgradeId.CRITICAL_CHANCE:critChance=Mathf.Min(.5f,critChance+.1f);break;case UpgradeId.CRITICAL_DAMAGE:critMultiplier+=.5f;break;case UpgradeId.DAMAGE_REDUCTION:damageReduction=Mathf.Min(.5f,damageReduction+.1f);break;case UpgradeId.DODGE_CHANCE:dodge=Mathf.Min(.3f,dodge+.1f);break;case UpgradeId.REGENERATING_SHIELD:shield=maxHealth*.2f*Stack(id);break;}}
}
[Serializable] public class MetaSave {
 public int version=3,crystals,coins,equippedWeapon;public int ownedWeapons=1;public bool soundEnabled=true,hapticsEnabled=true;public string legacyImportHash="";public int[] levels=new int[8];
 public bool OwnsWeapon(int i)=>SurvivorArmory.ValidIndex(i)&&(ownedWeapons&(1<<i))!=0;
 public bool BuyWeapon(int i){if(!SurvivorArmory.ValidIndex(i)||OwnsWeapon(i)||coins<SurvivorArmory.Get(i).cost)return false;coins-=SurvivorArmory.Get(i).cost;ownedWeapons|=1<<i;equippedWeapon=i;return true;}
 public bool EquipWeapon(int i){if(!OwnsWeapon(i))return false;equippedWeapon=i;return true;}
 public static readonly string[] Names={"起始生命","初始攻击","初始攻速","初始范围","拾取范围","经验倍率","晶石倍率","波次奖励"};
 public static readonly int[] Caps={10,10,8,8,8,8,10,10},Costs={40,45,50,40,35,45,50,60},Steps={25,30,32,28,25,30,35,40};
 public int Cost(int i)=>Costs[i]+levels[i]*Steps[i];
 public bool Buy(int i){if(i<0||i>=8||levels[i]>=Caps[i]||crystals<Cost(i))return false;crystals-=Cost(i);levels[i]++;return true;}
 public PlayerState NewPlayer(){var p=new PlayerState{maxHealth=100+10*levels[0],damage=20+2*levels[1],interval=Mathf.Max(.35f,1-.05f*levels[2]),range=2+.1f*levels[3],pickup=.5f+.1f*levels[4],experienceMultiplier=1+.1f*levels[5]};p.health=p.maxHealth;p.stacks[(int)UpgradeId.ENERGY_PROJECTILE]=1;return p;}
}
}
