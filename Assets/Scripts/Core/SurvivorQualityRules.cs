using UnityEngine;

namespace SpatialSurvivor {
public enum WeaponModification { Balanced, Impact, Quickcast, Frostbite }
public enum EliteAffix { None, Bulwark, Frenzy, Brood }
public static class SurvivorQualityRules {
    public static readonly string[] ModNames={"标准构型","破阵核心","速射核心","霜痕核心"};
    public static readonly string[] ModDescriptions={"保留武器原始伤害与节奏。","手持伤害 +25%，攻击间隔 +15%，强化击退。","攻击间隔 -20%，手持伤害 -10%。","手持命中减速 30% 持续 2 秒，伤害 -10%。"};
    public static readonly string[] ChallengeNames={"初露锋芒","精英克星","首领挑战者","四境行者"};
    public static readonly int[] ChallengeRewards={60,100,160,240};
    public static readonly string[] AffixNames={"","晶盾","狂奔","孵化"};
    public static float SwingMultiplier(float metresPerSecond)=>metresPerSecond>=2.8f?1.6f:1;
    public static float ModDamage(WeaponModification mod)=>mod==WeaponModification.Impact?1.25f:mod==WeaponModification.Balanced?1:.9f;
    public static float ModInterval(WeaponModification mod)=>mod==WeaponModification.Impact?1.15f:mod==WeaponModification.Quickcast?.8f:1;
    public static float HudDistance(float value)=>float.IsNaN(value)||float.IsInfinity(value)?1.6f:Mathf.Clamp(value,1.2f,2.2f);
    public static bool ChallengeComplete(MetaSave save,int index){
        switch(index){case 0:return save.totalKills>=50;case 1:return save.totalElites>=8;case 2:return save.totalBosses>=3;case 3:return (save.completedModes&15)==15;default:return false;}
    }
    public static string ChallengeProgress(MetaSave save,int index){
        switch(index){case 0:return Mathf.Min(50,save.totalKills)+" / 50 次击杀 · 解锁破阵核心";case 1:return Mathf.Min(8,save.totalElites)+" / 8 只精英 · 解锁速射核心";case 2:return Mathf.Min(3,save.totalBosses)+" / 3 位首领 · 解锁霜痕核心";default:int count=0;for(int i=0;i<4;i++)if((save.completedModes&(1<<i))!=0)count++;return count+" / 4 种模式完成目标";}
    }
    public static bool ModUnlocked(MetaSave save,int index)=>index==0||index>=1&&index<=3&&ChallengeComplete(save,index-1);
}
}
