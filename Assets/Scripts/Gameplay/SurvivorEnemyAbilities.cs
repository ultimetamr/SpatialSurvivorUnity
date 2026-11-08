using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    sealed class HostileBolt {public GameObject go;public Enemy owner;public int generation;public Vector3 velocity;public float remaining,damage;}
    sealed class HostileZone {public LineRenderer edge,progress;public Enemy owner;public int generation;public Vector3 position;public float radius,charge,duration,tick,damage;}
    readonly HostileBolt[] hostileBolts=new HostileBolt[24];
    readonly HostileZone[] hostileZones=new HostileZone[12];
    Material hostileMaterial;MaterialPropertyBlock enemyPaint;
    public int HostileBoltCount {get{int count=0;foreach(var bolt in hostileBolts)if(bolt!=null&&bolt.remaining>0)count++;return count;}}
    public int HostileZoneCount {get{int count=0;foreach(var zone in hostileZones)if(zone!=null&&zone.duration>0)count++;return count;}}
    void BuildEnemyAbilities(){
        enemyPaint=new MaterialPropertyBlock();
        hostileMaterial=new Material(Resources.Load<Shader>("Shaders/Additive"));hostileMaterial.SetColor("_Color",new Color(1,.42f,.22f,1));
        for(int i=0;i<hostileBolts.Length;i++){
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name="Enemy spell "+i;Destroy(go.GetComponent<Collider>());go.transform.SetParent(transform,false);go.transform.localScale=Vector3.one*.085f;go.GetComponent<Renderer>().sharedMaterial=hostileMaterial;go.SetActive(false);
            hostileBolts[i]=new HostileBolt{go=go};
        }
        for(int i=0;i<hostileZones.Length;i++){
            var edge=MakeLine("Enemy spell boundary "+i,new Color(1,.46f,.18f,.8f),.022f);
            var progress=MakeLine("Enemy spell charge "+i,new Color(1,.75f,.25f,.8f),.032f);
            edge.enabled=progress.enabled=false;hostileZones[i]=new HostileZone{edge=edge,progress=progress};
        }
    }
    void PrepareEnemy(Enemy e){
        e.affix=EliteAffix.None;e.summoned=false;e.hitFlash=e.stagger=e.knockRemaining=0;e.elite=false;e.abilityCooldown=1.3f+System.Array.IndexOf(enemies,e)*.11f;e.abilityWindup=e.dashRemaining=e.ward=e.wardTime=0;
        enemyPaint.Clear();enemyPaint.SetColor("_Tint",Color.white);enemyPaint.SetFloat("_Hit",0);e.visual.GetComponent<Renderer>().SetPropertyBlock(enemyPaint);
        if(e.eliteCrown)e.eliteCrown.SetActive(false);
    }
    void PromoteElite(Enemy e){
        if(!e.active||e.type==4||e.elite)return;
        e.elite=true;e.hp=e.maxHp=Mathf.CeilToInt(e.maxHp*2.6f);e.damage*=1.25f;e.speed*=1.12f;e.exp*=4;e.affix=(EliteAffix)(1+(System.Array.IndexOf(enemies,e)+e.generation)%3);if(e.affix==EliteAffix.Bulwark){e.ward=e.maxHp*.1f;e.wardTime=4;}if(e.affix==EliteAffix.Frenzy)e.speed*=1.12f;
        enemyPaint.SetColor("_Tint",new Color(1,.83f,.45f));e.visual.GetComponent<Renderer>().SetPropertyBlock(enemyPaint);
        e.eliteCrown.transform.localPosition=Vector3.up*(e.visual.GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds.max.y*e.baseScale.y+.08f);e.eliteCrown.SetActive(true);
        notice="精英出现 · "+SurvivorBestiary.Creatures[e.type].name;feedback=3;Cue(FeedbackCue.Boss,e.go.transform.position);
    }
    void BuildEliteCrown(Enemy e){
        e.eliteCrown=new GameObject("Elite gold crest");e.eliteCrown.transform.SetParent(e.go.transform,false);
        float top=Resources.Load<GameObject>("Art/Creatures/"+SurvivorBestiary.Creatures[e.type].model).GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds.max.y*e.baseScale.y+.08f;
        e.eliteCrown.transform.localPosition=Vector3.up*top;
        for(int i=0;i<3;i++){
            var gem=Instantiate(Resources.Load<GameObject>("Art/Weapons/ExperienceGem"),e.eliteCrown.transform);
            gem.transform.localPosition=new Vector3((i-1)*.075f,(i==1?.05f:0),0);gem.transform.localScale=Vector3.one*.12f;
            var block=new MaterialPropertyBlock();block.SetColor("_Tint",new Color(1,.73f,.25f));gem.GetComponent<Renderer>().SetPropertyBlock(block);
        }
        e.eliteCrown.SetActive(false);
    }
    Vector3 HostileTarget=>RunMode==SurvivorMode.CrystalDefense?CorePosition+Vector3.up*.45f:platform.Head.transform.position-Vector3.up*.3f;
    bool EnemyAbilityStep(Enemy e,float dt){
        if(e.type==4)return false;
        e.wardTime=Mathf.Max(0,e.wardTime-dt);if(e.wardTime<=0)e.ward=0;
        if(e.dashRemaining>0){
            float step=Mathf.Min(dt,e.dashRemaining);e.dashRemaining-=step;
            e.go.transform.position=SurvivorEnemyCrowd.Move(System.Array.IndexOf(enemies,e),enemies,e.abilityTarget,step*(e.type==9?3.2f:3.8f),platform.Navigation);
            if(Distance(e.go.transform.position,EnemyGoal)<e.radius+.4f&&e.attack<=0){HitObjective(e.damage*1.2f);e.attack=.9f;}
            return true;
        }
        if(e.abilityWindup>0){
            e.abilityWindup=Mathf.Max(0,e.abilityWindup-dt);
            if(e.abilityWindup<=0)e.dashRemaining=e.elite?.8f:.58f;
            return true;
        }
        e.abilityCooldown-=dt;
        if(e.abilityCooldown>0||Distance(e.go.transform.position,EnemyGoal)>7)return false;
        e.abilityCooldown=(e.elite?(e.affix==EliteAffix.Frenzy?2.8f:3.5f):5.5f)+(e.type%3)*.45f;if(e.type!=1&&e.type!=9)e.animator.Attack();
        switch(e.type){
            case 0: // Roots are fixed on the floor and give time to move away.
                CreateHostileZone(e,EnemyGoal,e.elite?.75f:.48f,e.elite?3:1.1f,e.damage*.55f);break;
            case 1:case 9:
                e.abilityTarget=EnemyGoal;e.abilityWindup=e.elite?.65f:.85f;e.animator.Attack(e.abilityWindup);
                Beam(e.go.transform.position+Vector3.up*.07f,e.abilityTarget+Vector3.up*.07f,new Color(1,.43f,.2f),e.abilityWindup);
                return true;
            case 2:
                e.ward=e.maxHp*(e.elite?.35f:.20f);e.wardTime=3;
                Ring(e.go.transform.position,e.radius+.1f,new Color(.5f,.8f,1),.7f);break;
            case 3:case 10:
                CreateHostileZone(e,EnemyGoal,e.elite?1:.62f,e.type==10?2.5f:1.2f,e.damage*.7f);
                if(e.elite)CreateHostileZone(e,EnemyGoal+RoomNavigation.Horizontal(platform.Head.transform.right)*.9f,.6f,1.8f,e.damage*.6f);break;
            case 5:case 6:case 7:
                ShootHostile(e,e.type==6||e.elite?3:1);
                if(e.type==7)CreateHostileZone(e,EnemyGoal,e.elite?.7f:.4f,e.elite?3:1.5f,e.damage*.45f);break;
            case 8:
                foreach(var ally in enemies)if(ally.active&&ally!=e&&Distance(ally.go.transform.position,e.go.transform.position)<3){
                    int restore=Mathf.CeilToInt(ally.maxHp*(e.elite?.18f:.1f));ally.hp=Mathf.Min(ally.maxHp,ally.hp+restore);
                    Beam(EnemyCenter(e),EnemyCenter(ally),new Color(.45f,1,.5f),.4f);
                }
                ShootHostile(e,1);break;
        }
        return false;
    }
    void ShootHostile(Enemy owner,int count){
        var origin=EnemyCenter(owner);var direction=(HostileTarget-origin).normalized;int fired=0;
        foreach(var bolt in hostileBolts)if(bolt.remaining<=0){
            float angle=count==1?0:(fired-(count-1)*.5f)*12;
            bolt.owner=owner;bolt.generation=owner.generation;bolt.velocity=Quaternion.AngleAxis(angle,Vector3.up)*direction*(owner.elite?2.4f:1.8f);
            bolt.damage=owner.damage*.65f;bolt.remaining=5;bolt.go.transform.position=origin;bolt.go.SetActive(true);
            if(++fired>=count)break;
        }
    }
    void CreateHostileZone(Enemy owner,Vector3 point,float radius,float duration,float damage){
        point.y=platform.Navigation.Floor;radius*=RoomSpellScale;
        // Skip spell marks outside scanned floor rather than extending into walls.
        if(!platform.Navigation.Contains(point,.12f))return;
        foreach(var zone in hostileZones)if(zone.duration<=0){zone.owner=owner;zone.generation=owner.generation;zone.position=point;zone.radius=radius;zone.charge=.95f;zone.duration=duration;zone.tick=0;zone.damage=damage;return;}
    }
    void HostileSpellStep(float dt){
        foreach(var bolt in hostileBolts){
            if(bolt.remaining<=0)continue;
            if(!bolt.owner.active||bolt.owner.generation!=bolt.generation){bolt.remaining=0;bolt.go.SetActive(false);continue;}
            var from=bolt.go.transform.position;var to=from+bolt.velocity*Mathf.Min(dt,bolt.remaining);bolt.remaining-=dt;
            bool wall=Physics.SphereCast(from,.04f,bolt.velocity.normalized,out var wallHit,Vector3.Distance(from,to),1<<RoomNavigation.Layer,QueryTriggerInteraction.Ignore);
            if(wall)to=wallHit.point;
            if(Vector3.Distance(NearestOnSegment(from,to,HostileTarget),HostileTarget)<.24f){HitObjective(bolt.damage);bolt.remaining=0;}
            bolt.go.transform.position=to;if(wall)bolt.remaining=0;if(bolt.remaining<=0)bolt.go.SetActive(false);
            if(screen!=ScreenState.Playing)return;
        }
        foreach(var zone in hostileZones){
            if(zone.duration<=0)continue;
            if(!zone.owner.active||zone.owner.generation!=zone.generation){zone.duration=0;continue;}
            if(zone.charge>0){zone.charge=Mathf.Max(0,zone.charge-dt);continue;}
            zone.duration=Mathf.Max(0,zone.duration-dt);zone.tick-=dt;
            if(zone.tick<=0){zone.tick=.6f;if(Distance(zone.position,EnemyGoal)<=zone.radius)HitObjective(zone.damage);}
            if(screen!=ScreenState.Playing)return;
        }
    }
    void ResetEnemyAbilities(){
        foreach(var bolt in hostileBolts)if(bolt!=null){bolt.remaining=0;bolt.go.SetActive(false);}
        foreach(var zone in hostileZones)if(zone!=null){zone.duration=0;zone.edge.enabled=zone.progress.enabled=false;}
    }
    void RefreshEnemyAbilities(bool visible){
        foreach(var bolt in hostileBolts)bolt.go.SetActive(visible&&bolt.remaining>0&&bolt.owner.active&&bolt.generation==bolt.owner.generation);
        foreach(var zone in hostileZones){
            bool show=visible&&zone.duration>0&&zone.owner.active&&zone.generation==zone.owner.generation;
            zone.edge.enabled=zone.progress.enabled=show;
            if(show){var color=zone.owner.type==10?new Color(.55f,.8f,1,.85f):zone.owner.type==7?new Color(.72f,1,.25f,.85f):zone.owner.type==3?new Color(.9f,.55f,1,.85f):new Color(1,.6f,.25f,.85f);zone.edge.startColor=zone.edge.endColor=color;RingShape(zone.edge,zone.position,zone.radius);RingShape(zone.progress,zone.position,zone.radius*(zone.charge>0?1-zone.charge/.95f:.95f));}
        }
        foreach(var e in enemies)if(e.eliteCrown)e.eliteCrown.SetActive(visible&&e.active&&e.elite);
    }
    void EliteBrood(Enemy enemy){
        if(!enemy.elite||enemy.affix!=EliteAffix.Brood)return;
        int alive=0;foreach(var actor in enemies)if(actor.active)alive++;
        int count=0;foreach(var minion in enemies){
            if(alive+count>=RoomMonsterLimit)break;
            if(minion.type!=0||minion.active||minion.deathRemaining>0)continue;
            if(!Spawn(minion,0))continue;
            minion.summoned=true;minion.hp=minion.maxHp=Mathf.Max(1,minion.maxHp/2);minion.exp=1;
            if(++count==2)break;
        }
    }
    void OnDestroy(){if(hostileMaterial)Destroy(hostileMaterial);}
}
}
