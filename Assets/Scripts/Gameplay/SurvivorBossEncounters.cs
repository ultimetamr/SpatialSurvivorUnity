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
    public string BossAction => Boss.telegraph>0?new[]{"盾矛追击 · 保持距离","荆棘护体 · 避免近身","破阵突刺 · 横向避让","月冠护体 · 避免近身","古树狂怒 · 保持距离"}[bossPattern]:BossPhase>1?"狂怒追击":"追击中";
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
        notice="BOSS · "+BossName+"  /  避免近身接触，突刺时横向避让";feedback=5;
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
            var before=boss.go.transform.position;
            boss.go.transform.position=SurvivorEnemyCrowd.Move(18,enemies,bossChargeEnd,step*3.2f,platform.Navigation);boss.repath=0;
            bool moved=Distance(before,boss.go.transform.position)>.00001f;
            if(!moved||Distance(boss.go.transform.position,bossChargeEnd)<.08f)bossDash=0;
            TryEnemyContact(boss,1.2f);return moved;
        }
        if(boss.telegraph>0){
            MoveDuringBossWindup(boss,dt);
            if(bossPattern==2)bossChargeStart=boss.go.transform.position;
            boss.telegraph=Mathf.Max(0,boss.telegraph-dt);
            if(boss.telegraph<=0){
                if(bossPattern==2)bossDash=.55f;
                else if(!meta.lowEffects)skillVisuals.Emit(SkillVisual.Explosion,EnemyCenter(boss),.35f,Quaternion.identity);
                boss.aoe=BossPhase==3?2.4f:BossPhase==2?3.1f:4.2f;bossTargetCount=0;
            }
            TryEnemyContact(boss);return true;
        }
        boss.aoe-=dt;if(boss.aoe>0)return false;
        int sequence=bossAttackIndex++;
        bossPattern=bossEncounter==1?(sequence%2==0?0:2):bossEncounter==2?(sequence%2==0?1:3):sequence%3==0?0:sequence%3==1?1:4;
        bossAttackRadius=bossPattern==2?.36f:boss.radius+.22f;
        bossChargeDuration=bossPattern==2?1.15f:bossPattern==0?1.5f:1.35f;
        boss.telegraph=bossChargeDuration;boss.animator.Attack(bossChargeDuration);bossTargetCount=1;
        bossTargets[0]=boss.go.transform.position;
        if(bossPattern==2){
            bossChargeStart=boss.go.transform.position;var direction=RoomNavigation.Horizontal(PlayerPosition-bossChargeStart).normalized;
            bossChargeEnd=bossChargeStart+direction*Mathf.Min(2.8f,Distance(PlayerPosition,bossChargeStart)+.6f);
        }
        notice=BossName+" · "+BossAction;feedback=bossChargeDuration;Cue(FeedbackCue.Boss,boss.go.transform.position);return false;
    }
    void MoveDuringBossWindup(Enemy boss,float dt){
        var goal=bossPattern==2?bossChargeEnd:SurvivorEnemyCrowd.PursuitGoal(18,enemies,EnemyGoal,platform.Navigation);
        var waypoint=goal;
        boss.go.transform.position=SurvivorEnemyCrowd.Move(18,enemies,waypoint,boss.speed*(boss.slow>0?.7f:1)*dt,platform.Navigation);boss.repath=0;
    }
    float BossAttackRadius => bossAttackRadius;
    bool FindFreeSpawn(float distance,float radius,System.Func<Vector3,bool> clear,out Vector3 position){
        // Spawn around the current target even outside the captured floor. Room
        // models remain visible, but never constrain virtual creature movement.
        float angle=(float)rng.NextDouble()*Mathf.PI*2;
        for(int i=0;i<96;i++){
            float a=angle+i*2.399963f;float ring=distance+radius+.12f+(i/24)*.45f;
            var p=PlayerPosition+new Vector3(Mathf.Cos(a)*ring,0,Mathf.Sin(a)*ring);
            if(clear(p)){position=p;return true;}
        }
        position=PlayerPosition;return false;
    }
    bool FindGroundSpawn(int type,float radius,out Vector3 position,out float ceiling){
        ceiling=platform.Navigation.Floor+2.4f;
        return FindFreeSpawn(type==4?3:2,radius,p=>SurvivorEnemyCrowd.SpawnClear(p,type,null,enemies),out position);
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
        bool warning=visible&&Boss.active&&Boss.telegraph>0&&bossPattern==2;
        if(warning&&bossPattern==2)bossChargeStart=Boss.go.transform.position;
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
