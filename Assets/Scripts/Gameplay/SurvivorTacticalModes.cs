using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    sealed class FrostTrap {public Vector3 point;public float remaining,tick;public LineRenderer line;}
    readonly FrostTrap[] frostTraps=new FrostTrap[3];
    float nextRespite=45,huntChainUntil,enemySpeedBonus=1,enemyDamageBonus=1,handDamageBonus=1,trialRewardBonus;
    int huntChain,runBossKills;bool campPending;
    public int TacticalBounty {get;private set;}
    public int DefenseEnergy {get;private set;}
    public int TrialRounds {get;private set;}
    public string LastThreat {get;private set;}="";
    public string CampTitle=>RunMode==SurvivorMode.Endless?"为下一轮立下契约":"林间奇遇 · 选择一项馈赠";
    public string TacticalStatus=>RunMode==SurvivorMode.CrystalDefense?"能量 "+DefenseEnergy+" · 修复 20 / 冰阱 15":
        RunMode==SurvivorMode.EliteHunt?"狩猎连击 "+huntChain+" · "+LastThreat:
        RunMode==SurvivorMode.Endless?"契约 "+TrialRounds+" · 额外收益 "+Mathf.RoundToInt(trialRewardBonus*100)+"%":"武器构型 · "+SurvivorQualityRules.ModNames[(int)runModification];
    public float RoomArea=>platform.Navigation.Count*RoomNavigation.Cell*RoomNavigation.Cell;
    public int RoomMonsterLimit=>Mathf.Clamp(Mathf.FloorToInt(RoomArea/1.15f),8,18);
    public float RoomSpellScale=>Mathf.Clamp(Mathf.Sqrt(RoomArea)/6,.65f,1);
    public int ActiveTrapCount {get{int count=0;foreach(var trap in frostTraps)if(trap!=null&&trap.remaining>0)count++;return count;}}
    void BuildTactics(){for(int i=0;i<frostTraps.Length;i++){var line=MakeLine("Friendly frost trap "+i,new Color(.25f,.85f,1,.8f),.018f);line.enabled=false;frostTraps[i]=new FrostTrap{line=line};}}
    void ResetTactics(){
        nextRespite=45;huntChainUntil=0;huntChain=runBossKills=0;TacticalBounty=TrialRounds=0;DefenseEnergy=30;
        campPending=false;enemySpeedBonus=enemyDamageBonus=handDamageBonus=1;trialRewardBonus=0;LastThreat="";
        foreach(var trap in frostTraps)if(trap!=null){trap.remaining=0;trap.line.enabled=false;}
    }
    void TacticsStep(float dt){
        if(huntChain>0&&player.elapsed>huntChainUntil)huntChain=0;
        if(RunMode==SurvivorMode.Classic&&player.elapsed>=nextRespite){
            if(player.elapsed-dt<nextRespite&&!Boss.active)campPending=true;
            while(nextRespite<=player.elapsed)nextRespite+=150;
        }
        if(campPending&&!Boss.active&&dt>0){campPending=false;screen=ScreenState.Camp;ResetHandCombat(false);notice="";return;}
        if(RunMode==SurvivorMode.EliteHunt){
            Enemy nearest=null;float distance=float.MaxValue;
            foreach(var e in enemies)if(e.active&&e.elite){float d=Distance(e.go.transform.position,PlayerPosition);if(d<distance){nearest=e;distance=d;}}
            if(nearest==null)LastThreat="等待精英出现";
            else{var local=platform.Head.transform.InverseTransformDirection(nearest.go.transform.position-platform.Head.transform.position);LastThreat=(local.z<0?"后方":Mathf.Abs(local.x)>.5f?(local.x<0?"左侧":"右侧"):"前方")+"精英 "+distance.ToString("F1")+" 米";}
        }
        foreach(var trap in frostTraps){
            if(trap.remaining<=0)continue;trap.remaining=Mathf.Max(0,trap.remaining-dt);trap.tick-=dt;
            if(trap.tick<=0){trap.tick=.5f;foreach(var enemy in enemies)if(enemy.active&&!SurvivorBestiary.Flying(enemy.type)&&Distance(enemy.go.transform.position,trap.point)<=.7f){enemy.slow=Mathf.Max(1,enemy.slow);Damage(enemy,Scaled(8),false);if(screen!=ScreenState.Playing)return;}}
        }
    }
    void TacticalKill(Enemy enemy){
        if(RunMode==SurvivorMode.CrystalDefense)DefenseEnergy=Mathf.Min(99,DefenseEnergy+(enemy.elite?12:3));
        if(RunMode==SurvivorMode.EliteHunt&&enemy.elite){
            huntChain=player.elapsed<=huntChainUntil?Mathf.Min(3,huntChain+1):1;huntChainUntil=player.elapsed+18;
            int extra=(huntChain-1)*4;TacticalBounty+=extra;notice="精英连击 ×"+huntChain+(extra>0?" · 连击金币 +"+extra:"");feedback=2;
            // Accelerate the next hunt when the current elite was defeated quickly.
            eliteSpawnClock=Mathf.Min(eliteSpawnClock,6);
        }
    }
    public bool RepairCore(){
        if(screen!=ScreenState.Playing||RunMode!=SurvivorMode.CrystalDefense||CoreHealth>=300||DefenseEnergy<20)return false;
        DefenseEnergy-=20;CoreHealth=Mathf.Min(300,CoreHealth+60);Cue(FeedbackCue.Purchase,CorePosition);notice="晶核修复 +60";feedback=2;return true;
    }
    public bool PlaceFrostTrap(){
        if(screen!=ScreenState.Playing||RunMode!=SurvivorMode.CrystalDefense||DefenseEnergy<15)return false;
        var direction=RoomNavigation.Horizontal(platform.Head.transform.forward).normalized;
        Vector3 point=PlayerPosition+direction*.9f;
        if(platform.WeaponPose(out var ray,out _)){
            var plane=new Plane(Vector3.up,new Vector3(0,platform.Navigation.Floor,0));
            if(plane.Raycast(ray,out float enter)&&enter>0&&enter<4)point=ray.GetPoint(enter);
        }
        point.y=platform.Navigation.Floor;
        if(!platform.Navigation.Contains(point,.2f))point=PlayerPosition;
        if(!platform.Navigation.Contains(point,.2f))return false;
        foreach(var trap in frostTraps)if(trap.remaining<=0){trap.point=point;trap.remaining=8;trap.tick=0;DefenseEnergy-=15;notice="冰阱已布置 · 持续 8 秒";feedback=2;Cue(FeedbackCue.Freeze,point);return true;}
        return false;
    }
    public bool ChooseCamp(int choice){
        if(screen!=ScreenState.Camp||choice<0||choice>2)return false;
        if(RunMode==SurvivorMode.Endless){
            TrialRounds++;trialRewardBonus+=.15f;
            if(choice==0){enemySpeedBonus*=1.12f;handDamageBonus*=1.18f;}
            if(choice==1){enemyDamageBonus*=1.15f;player.maxHealth+=20;player.health=Mathf.Min(player.maxHealth,player.health+40);}
            if(choice==2){enemySpeedBonus*=1.08f;player.interval=Mathf.Max(.12f,player.interval*.9f);}
            foreach(var enemy in enemies)if(enemy.active){if(choice==0)enemy.speed*=1.12f;if(choice==1)enemy.damage*=1.15f;if(choice==2)enemy.speed*=1.08f;}
            history.Add("试炼契约 · "+new[]{"猎风","坚壁","急咒"}[choice]);
        }else{
            if(choice==0){if(player.Stack(UpgradeId.ATTACK_DAMAGE)>=5)player.health=Mathf.Min(player.maxHealth,player.health+20);else player.Apply(UpgradeId.ATTACK_DAMAGE);history.Add("林间馈赠 · 全伤害");}
            if(choice==1){player.health=Mathf.Min(player.maxHealth,player.health+player.maxHealth*.35f);history.Add("林间馈赠 · 治愈");}
            if(choice==2){TacticalBounty+=30;enemySpeedBonus*=1.08f;foreach(var enemy in enemies)if(enemy.active)enemy.speed*=1.08f;history.Add("林间馈赠 · 冒险金币 +30");}
        }
        screen=ScreenState.Playing;ResetHandCombat(false);return true;
    }
    void TacticalBossDefeated(){runBossKills++;if(RunMode==SurvivorMode.Endless)campPending=true;}
    void RecordRun(bool victory){
        meta.totalKills=(int)System.Math.Min(int.MaxValue,(long)meta.totalKills+player.kills);
        meta.totalElites=(int)System.Math.Min(int.MaxValue,(long)meta.totalElites+EliteKills);
        meta.totalBosses=(int)System.Math.Min(int.MaxValue,(long)meta.totalBosses+runBossKills);
        if(victory||(RunMode==SurvivorMode.Endless&&player.elapsed>=360))meta.completedModes|=1<<(int)RunMode;
        if(RunMode==SurvivorMode.Endless)meta.bestEndless=Mathf.Max(meta.bestEndless,player.elapsed);
    }
    void RefreshTactics(bool visible){foreach(var trap in frostTraps){trap.line.enabled=visible&&trap.remaining>0;if(trap.line.enabled)RingShape(trap.line,trap.point,.7f);}}
}
}
