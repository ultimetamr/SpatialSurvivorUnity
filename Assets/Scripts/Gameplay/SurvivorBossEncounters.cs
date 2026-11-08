using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    static readonly float[] BossSchedule={120,300,600};
    static readonly string[] BossNames={"荆棘守卫","月冠女王","古树守望者"};
    readonly Vector3[] bossTargets=new Vector3[3];
    readonly LineRenderer[] bossWarnings=new LineRenderer[6];
    Transform bossCrown;
    MaterialPropertyBlock bossTint;
    int completedBosses,bossEncounter,bossAttackIndex,bossPattern,bossTargetCount,bossWarningIndex;
    float bossChargeDuration,bossAttackRadius;Vector3 bossChargeStart,bossChargeEnd;float bossDash;
    public int BossEncounter => bossEncounter;
    public int BossPhase => !Boss.active?0:Boss.hp>Boss.maxHp*.65f?1:Boss.hp>Boss.maxHp*.30f?2:3;
    public string BossName => BossNames[Mathf.Clamp(bossEncounter-1,0,2)];
    public string BossAction => Boss.telegraph>0?new[]{"震地冲击 · 离开圆内","荆棘锁定 · 离开标记","破阵突刺 · 横向避让","月环绽放 · 圆心或圆外安全","星种齐射 · 横向躲弹"}[bossPattern]:BossPhase>1?"狂怒追击":"追击中";
    public float NextBossTime => RunMode==SurvivorMode.Endless?(completedBosses+1)*120:BossSchedule[Mathf.Clamp(completedBosses,0,2)];
    public string BossCountdown => Boss.active?"首领战 · 阶段 "+BossPhase:"首领 "+Mathf.Max(0,Mathf.CeilToInt(NextBossTime-player.elapsed))+"秒";

    void BuildBossPresentation(){
        bossTint=new MaterialPropertyBlock();
        CacheBossModel(11,"ThornSentinel",.95f);CacheBossModel(12,"MoonRegent",.95f);
        for(int i=0;i<bossWarnings.Length;i++){
            bossWarnings[i]=MakeLine("Boss danger zone "+i,new Color(1,.24f,.12f,.9f),i%2==0?.025f:.045f);
            bossWarnings[i].enabled=false;
        }
        bossCrown=new GameObject("Boss living crown").transform;bossCrown.SetParent(Boss.go.transform,false);
        var prefab=Resources.Load<GameObject>("Art/Weapons/FrostCrystal");
        for(int i=0;i<5;i++){
            var crystal=Instantiate(prefab,bossCrown);float a=i*Mathf.PI*2/5;
            crystal.transform.localPosition=new Vector3(Mathf.Cos(a)*.42f,1.48f+(i%2)*.1f,Mathf.Sin(a)*.42f);
            crystal.transform.localScale=Vector3.one*.16f;
        }
    }
    void ResetBossEncounters(){completedBosses=bossEncounter=bossAttackIndex=bossWarningIndex=0;bossTargetCount=0;bossDash=0;}
    bool BossSpawnStep(){
        if(RunMode==SurvivorMode.EliteHunt||RunMode==SurvivorMode.CrystalDefense)return false;
        if(Boss.active)return true;
        int due=RunMode==SurvivorMode.Endless?Mathf.FloorToInt(player.elapsed/120):player.elapsed>=600?3:player.elapsed>=300?2:player.elapsed>=120?1:0;
        if(due<=completedBosses){
            if((completedBosses<3||RunMode==SurvivorMode.Endless)&&player.elapsed>=NextBossTime-8&&bossWarningIndex<=completedBosses){
                bossWarningIndex=completedBosses+1;notice="首领即将出现 · "+BossNames[Mathf.Clamp(completedBosses,0,2)];feedback=8;Cue(FeedbackCue.Boss,PlayerPosition);
            }
            return bossSpawned;
        }
        if(Boss.deathRemaining>0)return true;
        if(!Spawn(Boss,4))return false;
        bossEncounter=due;bossAttackIndex=0;bossTargetCount=0;bossDash=0;SetBossModel(Mathf.Min(due,3));
        Boss.hp=Boss.maxHp=Mathf.CeilToInt((due==1?900:due==2?2400:6000)*Mathf.Pow(1.35f,Mathf.Max(0,due-3)));
        Boss.speed=(due==1?.40f:due==2?.46f:.52f)*enemySpeedBonus;
        Boss.damage=(due==1?15:due==2?23:30)*enemyDamageBonus;Boss.aoe=2.2f;
        var renderer=Boss.visual.GetComponent<Renderer>();
        bossTint.SetColor("_Tint",due==1?new Color(.78f,1,.83f):due==2?new Color(.88f,.76f,1):new Color(1,.88f,.68f));
        renderer.SetPropertyBlock(bossTint);
        bossSpawned=due==3&&RunMode==SurvivorMode.Classic;
        foreach(var enemy in enemies)if(enemy!=Boss){enemy.active=false;enemy.deathRemaining=0;enemy.go.SetActive(false);}
        notice="BOSS · "+BossName+"  /  留意红色地面预警";feedback=5;
        Cue(FeedbackCue.Boss,Boss.go.transform.position);
        return true;
    }
    void CacheBossModel(int key,string model,float scale){
        var go=Instantiate(Resources.Load<GameObject>("Art/Creatures/"+model),Boss.go.transform);go.transform.localScale=Vector3.one*scale;go.SetActive(false);
        Boss.visuals[key]=go;Boss.visualScales[key]=go.transform.localScale;
    }
    void SetBossModel(int encounter){
        int key=encounter==1?11:encounter==2?12:4;
        foreach(var go in Boss.visuals.Values)go.SetActive(false);
        Boss.visual=Boss.visuals[key];Boss.baseScale=Boss.visualScales[key];Boss.visual.transform.localScale=Boss.baseScale;
        Boss.visual.SetActive(true);Boss.animator=Boss.visual.GetComponent<SurvivorCreatureAnimator>();Boss.animator.Respawn();
        float top=Boss.visual.GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds.max.y*Boss.baseScale.y;
        bossCrown.localPosition=Vector3.up*(top-1.3f);
    }
    bool BossCombatStep(Enemy boss,float dt){
        if(bossDash>0){
            float step=Mathf.Min(dt,bossDash);bossDash-=step;
            boss.go.transform.position=SurvivorEnemyCrowd.Move(18,enemies,bossChargeEnd,step*3.2f,platform.Navigation);boss.repath=0;return true;
        }
        if(boss.telegraph>0){
            boss.telegraph=Mathf.Max(0,boss.telegraph-dt);
            if(boss.telegraph<=0){
                bool hit=false;
                if(bossPattern==2){
                    hit=Distance(NearestOnSegment(bossChargeStart,bossChargeEnd,PlayerPosition),PlayerPosition)<=BossAttackRadius;
                    bossDash=.55f;
                }else if(bossPattern==3){
                    float distance=Distance(bossTargets[0],PlayerPosition);hit=distance>=BossAttackRadius*.48f&&distance<=BossAttackRadius;
                    Ring(bossTargets[0],BossAttackRadius,new Color(.8f,.6f,1),.5f);
                }else if(bossPattern==4){ShootHostile(boss,5);}
                else for(int i=0;i<bossTargetCount;i++){
                    if(Distance(bossTargets[i],PlayerPosition)<=BossAttackRadius)hit=true;
                    Ring(bossTargets[i],BossAttackRadius,new Color(1,.4f,.15f),.48f);
                    if(!meta.lowEffects)skillVisuals.Emit(SkillVisual.Explosion,bossTargets[i],.42f,Quaternion.identity);
                }
                if(hit)Hurt(boss.damage*(bossPattern==2?1.2f:bossPattern==0?1.1f:.8f));
                boss.aoe=BossPhase==3?2.4f:BossPhase==2?3.1f:4.2f;bossTargetCount=0;
            }
            return true;
        }
        boss.aoe-=dt;if(boss.aoe>0)return false;
        int sequence=bossAttackIndex++;
        bossPattern=bossEncounter==1?(sequence%2==0?0:2):bossEncounter==2?(sequence%2==0?1:3):sequence%3==0?0:sequence%3==1?1:4;
        bossAttackRadius=(bossPattern==0?(BossPhase==3?1.9f:1.65f):bossPattern==3?1.8f:bossPattern==2?.36f:.62f)*RoomSpellScale;
        bossChargeDuration=bossPattern==2?1.15f:bossPattern==0?1.5f:1.35f;
        boss.telegraph=bossChargeDuration;boss.animator.Attack(bossChargeDuration);bossTargetCount=bossPattern==1&&BossPhase>1?3:1;
        bossTargets[0]=bossPattern==0||bossPattern==3?boss.go.transform.position:PlayerPosition;
        if(bossTargetCount>1){var side=RoomNavigation.Horizontal(platform.Head.transform.right).normalized*.9f*RoomSpellScale;bossTargets[1]=PlayerPosition+side;bossTargets[2]=PlayerPosition-side;}
        if(bossPattern==2){
            bossChargeStart=boss.go.transform.position;var direction=RoomNavigation.Horizontal(PlayerPosition-bossChargeStart).normalized;
            bossChargeEnd=bossChargeStart+direction*Mathf.Min(2.8f,Distance(PlayerPosition,bossChargeStart)+.6f);
            while(!platform.Navigation.Contains(bossChargeEnd,boss.radius)&&Distance(bossChargeEnd,bossChargeStart)>.2f)bossChargeEnd=Vector3.MoveTowards(bossChargeEnd,bossChargeStart,.15f);
        }
        notice=BossName+" · "+BossAction;feedback=bossChargeDuration;Cue(FeedbackCue.Boss,boss.go.transform.position);return true;
    }
    float BossAttackRadius => bossAttackRadius;
    bool FindGroundSpawn(int type,float radius,out Vector3 position,out float ceiling){
        if(platform.Navigation.Spawn(PlayerPosition,type==4?3:Mathf.Min(2,Mathf.Max(1.15f,Mathf.Sqrt(RoomArea)*.42f)),radius,rng,out position,out ceiling,type==3,
            point=>type==4||SurvivorEnemyCrowd.SpawnClear(point,type,null,enemies)))return true;
        // A valid small MR room may have no point three metres from its player.
        // Keep the same footprint/wall clearance and require safe body separation.
        return type==4&&platform.Navigation.Spawn(PlayerPosition,1.2f,radius,rng,out position,out ceiling);
    }
    void BossDefeated(){
        completedBosses=Mathf.Max(completedBosses,bossEncounter);TacticalBossDefeated();
        bossTargetCount=0;
        if(bossEncounter>=3&&RunMode==SurvivorMode.Classic){Finish(true);return;}
        player.kills++;player.health=Mathf.Min(player.maxHealth,player.health+player.maxHealth*.2f);
        Drop(Boss.go.transform.position,bossEncounter==1?20:40);
        notice=BossName+"已击败 · 恢复 20% 生命，继续生存";feedback=6;spawn=2;
    }
    void RefreshBossPresentation(bool visible){
        bool warning=visible&&Boss.active&&Boss.telegraph>0;
        for(int i=0;i<3;i++){
            var edge=bossWarnings[i*2];var progress=bossWarnings[i*2+1];
            edge.enabled=progress.enabled=warning&&i<bossTargetCount;
            if(!edge.enabled)continue;
            if(bossPattern==2){
                var side=Vector3.Cross(Vector3.up,(bossChargeEnd-bossChargeStart).normalized)*BossAttackRadius;
                edge.positionCount=5;edge.SetPositions(new[]{bossChargeStart+side+Vector3.up*.035f,bossChargeEnd+side+Vector3.up*.035f,bossChargeEnd-side+Vector3.up*.035f,bossChargeStart-side+Vector3.up*.035f,bossChargeStart+side+Vector3.up*.035f});
                progress.positionCount=2;progress.SetPosition(0,bossChargeStart+Vector3.up*.04f);progress.SetPosition(1,Vector3.Lerp(bossChargeStart,bossChargeEnd,1-Boss.telegraph/bossChargeDuration)+Vector3.up*.04f);continue;
            }
            if(bossPattern==3){RingShape(edge,bossTargets[0],BossAttackRadius);RingShape(progress,bossTargets[0],BossAttackRadius*.48f);edge.startColor=edge.endColor=new Color(.85f,.6f,1,.9f);continue;}
            edge.startColor=edge.endColor=new Color(1,.24f,.12f,.9f);
            RingShape(edge,bossTargets[i],BossAttackRadius);
            RingShape(progress,bossTargets[i],BossAttackRadius*(1-Boss.telegraph/Mathf.Max(.01f,bossChargeDuration)));
        }
        if(bossCrown){bossCrown.gameObject.SetActive(visible&&Boss.active);bossCrown.localRotation=Quaternion.Euler(0,player.elapsed*(BossPhase>1?45:22),0);}
    }
}
}
