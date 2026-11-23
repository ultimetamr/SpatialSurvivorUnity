using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    MaterialPropertyBlock enemyPaint;
    // Kept for diagnostics: enemies no longer create ranged damage objects.
    public int HostileBoltCount=>0;
    public int HostileZoneCount=>0;
    void BuildEnemyAbilities(){enemyPaint=new MaterialPropertyBlock();}
    bool TryEnemyContact(Enemy enemy,float multiplier=1){
        if(!enemy.active||enemy.freeze>0||enemy.falling||enemy.attack>0)return false;
        bool touching=SurvivorBestiary.Flying(enemy.type)?Vector3.Distance(EnemyCenter(enemy),HostileTarget)<=enemy.radius+.22f:
            Distance(enemy.go.transform.position,EnemyGoal)<=enemy.radius+.22f&&Mathf.Abs(enemy.go.transform.position.y-EnemyGoal.y)<.45f;
        if(!touching)return false;
        enemy.attack=.8f;enemy.animator.Attack();HitObjective(enemy.damage*multiplier);return true;
    }
    void PrepareEnemy(Enemy e){
        if(e.chargeWarning)e.chargeWarning.enabled=false;
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
            var before=e.go.transform.position;
            e.go.transform.position=SurvivorEnemyCrowd.Move(System.Array.IndexOf(enemies,e),enemies,e.abilityTarget,step*(e.type==9?3.2f:3.8f),platform.Navigation);e.repath=0;
            bool moved=Distance(before,e.go.transform.position)>.00001f;
            if(!moved||Distance(e.go.transform.position,e.abilityTarget)<.08f)e.dashRemaining=0;
            TryEnemyContact(e,1.2f);
            return moved;
        }
        if(e.abilityWindup>0){
            // Keep advancing toward the locked charge target during anticipation.
            e.go.transform.position=SurvivorEnemyCrowd.Move(System.Array.IndexOf(enemies,e),enemies,SurvivorEnemyCrowd.PursuitGoal(System.Array.IndexOf(enemies,e),enemies,EnemyGoal,platform.Navigation),e.speed*(e.slow>0?.7f:1)*dt,platform.Navigation);e.repath=0;
            e.abilityWindup=Mathf.Max(0,e.abilityWindup-dt);
            if(e.abilityWindup<=0)e.dashRemaining=e.elite?.8f:.58f;TryEnemyContact(e);
            return true;
        }
        e.abilityCooldown-=dt;
        if(e.abilityCooldown>0||Distance(e.go.transform.position,EnemyGoal)>7)return false;
        e.abilityCooldown=(e.elite?(e.affix==EliteAffix.Frenzy?2.8f:3.5f):5.5f)+(e.type%3)*.45f;if(e.type!=1&&e.type!=9)e.animator.Attack();
        switch(e.type){
            case 0:
                e.ward=e.maxHp*(e.elite?.16f:.08f);e.wardTime=1.6f;break;
            case 1:case 9:
                e.abilityTarget=EnemyGoal;e.abilityWindup=e.elite?.65f:.85f;e.animator.Attack(e.abilityWindup);
                if(!e.chargeWarning)e.chargeWarning=MakeLine("Charge path "+System.Array.IndexOf(enemies,e),new Color(1,.43f,.2f),.025f);
                RefreshChargeWarning(e,true);
                return false;
            case 2:
                e.ward=e.maxHp*(e.elite?.35f:.20f);e.wardTime=3;
                Ring(e.go.transform.position,e.radius+.1f,new Color(.5f,.8f,1),.7f);break;
            case 3:case 10:
                e.ward=e.maxHp*(e.elite?.22f:.12f);e.wardTime=2.5f;
                Ring(EnemyCenter(e),e.radius,new Color(.7f,.65f,1),.35f);break;
            case 5:case 6:case 7:
                // The spell animation is local; the creature must approach to hit.
                Ring(EnemyCenter(e),e.radius,new Color(1,.65f,.3f),.3f);break;
            case 8:
                foreach(var ally in enemies)if(ally.active&&ally!=e&&Distance(ally.go.transform.position,e.go.transform.position)<3){
                    int restore=Mathf.CeilToInt(ally.maxHp*(e.elite?.18f:.1f));ally.hp=Mathf.Min(ally.maxHp,ally.hp+restore);
                    Beam(EnemyCenter(e),EnemyCenter(ally),new Color(.45f,1,.5f),.4f);
                }
                break;
        }
        return false;
    }
    void HostileSpellStep(float dt){} // No delayed or remote enemy damage.
    void ResetEnemyAbilities(){foreach(var enemy in enemies)if(enemy.chargeWarning)enemy.chargeWarning.enabled=false;}
    void RefreshEnemyAbilities(bool visible){
        foreach(var e in enemies){if(e.eliteCrown)e.eliteCrown.SetActive(visible&&e.active&&e.elite);RefreshChargeWarning(e,visible);}
    }
    void RefreshChargeWarning(Enemy enemy,bool visible){
        if(!enemy.chargeWarning)return;
        enemy.chargeWarning.enabled=visible&&enemy.active&&enemy.abilityWindup>0;
        if(!enemy.chargeWarning.enabled)return;
        enemy.chargeWarning.positionCount=2;enemy.chargeWarning.SetPosition(0,enemy.go.transform.position+Vector3.up*.07f);enemy.chargeWarning.SetPosition(1,enemy.abilityTarget+Vector3.up*.07f);
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
}
}
