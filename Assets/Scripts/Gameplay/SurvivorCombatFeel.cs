using UnityEngine;
using UnityEngine.UI;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    sealed class DamageLabel {public Transform root;public Text text;public float remaining;public Vector3 origin;}
    readonly DamageLabel[] damageLabels=new DamageLabel[16];
    MaterialPropertyBlock hitPaint;
    WeaponModification runModification;
    float swingPower=1;
    public bool LastHeavyHit {get;private set;}
    public int ConfirmedHandHits {get;private set;}
    public WeaponModification RunModification=>runModification;
    public float HandInterval=>SurvivorQualityRules.ModInterval(runModification);
    int HandDamage(float value)=>Scaled(value*SurvivorQualityRules.ModDamage(runModification)*handDamageBonus);
    void BuildCombatFeel(){
        hitPaint=new MaterialPropertyBlock();
        for(int i=0;i<damageLabels.Length;i++){
            var go=new GameObject("Impact number "+i,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(transform,false);
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.sortingOrder=14;
            go.GetComponent<RectTransform>().sizeDelta=new Vector2(180,60);go.transform.localScale=Vector3.one*.0012f;
            var child=new GameObject("Damage",typeof(RectTransform),typeof(Text));child.transform.SetParent(go.transform,false);
            var label=child.GetComponent<Text>();label.font=Resources.Load<Font>("Fonts/Chinese");label.fontSize=30;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;
            child.GetComponent<RectTransform>().sizeDelta=new Vector2(180,60);
            damageLabels[i]=new DamageLabel{root=go.transform,text=label};go.SetActive(false);
        }
    }
    void ResetCombatFeel(){
        ConfirmedHandHits=0;LastHeavyHit=false;swingPower=1;
        runModification=(WeaponModification)meta.weaponModification;
        if(!SurvivorQualityRules.ModUnlocked(meta,(int)runModification))runModification=WeaponModification.Balanced;
        foreach(var label in damageLabels)if(label!=null){label.remaining=0;label.root.gameObject.SetActive(false);}
    }
    void ImpactNumber(Enemy enemy,int amount,bool critical){
        if(amount<=0)return;
        foreach(var label in damageLabels)if(label.remaining<=0){
            label.origin=EnemyCenter(enemy)+Vector3.up*.16f;label.remaining=.55f;
            label.text.text=(critical?"暴击 ":"")+amount;label.text.color=critical?new Color(1,.8f,.28f):Color.white;
            label.root.position=label.origin;return;
        }
    }
    void HandImpact(Enemy enemy,Vector3 from,bool heavy){
        ConfirmedHandHits++;LastHeavyHit=heavy;enemy.hitFlash=.12f;
        float force=heavy?.28f:.12f;if(runModification==WeaponModification.Impact)force*=1.5f;
        if(enemy.active){
            enemy.knockDirection=RoomNavigation.Horizontal(enemy.go.transform.position-from).normalized;
            enemy.knockRemaining=enemy.type==4?force*.2f:force;
            enemy.stagger=enemy.type==4?0:heavy?.2f:.055f;
            if(runModification==WeaponModification.Frostbite)enemy.slow=Mathf.Max(2,enemy.slow);
        }
        Ring(EnemyCenter(enemy)-Vector3.up*.1f,heavy?.2f:.1f,heavy?new Color(1,.8f,.35f):new Color(.6f,.9f,1),.18f);
        if(Feedback)Feedback.HandHit(heavy,EnemyCenter(enemy),platform.TrackedWeaponPose.Source);
    }
    bool ImpactMovement(Enemy enemy,float dt){
        if(enemy.knockRemaining>0){
            float step=Mathf.Min(enemy.knockRemaining,dt*2);enemy.knockRemaining-=step;
            var goal=enemy.go.transform.position+enemy.knockDirection*step;
            if(SurvivorBestiary.Flying(enemy.type)){if(AirClear(goal,enemy,enemy.radius))enemy.go.transform.position=goal;}
            else enemy.go.transform.position=SurvivorEnemyCrowd.Move(System.Array.IndexOf(enemies,enemy),enemies,goal,step,platform.Navigation);
            enemy.repath=0;
        }
        bool staggered=enemy.stagger>0;enemy.stagger=Mathf.Max(0,enemy.stagger-dt);return staggered;
    }
    void CombatFeelStep(float dt){
        foreach(var label in damageLabels)if(label.remaining>0){label.remaining=Mathf.Max(0,label.remaining-dt);label.root.position=label.origin+Vector3.up*(.55f-label.remaining)*.25f;}
        foreach(var e in enemies)if(e.visual){
            e.hitFlash=Mathf.Max(0,e.hitFlash-dt);e.visual.GetComponent<Renderer>().GetPropertyBlock(hitPaint);hitPaint.SetFloat("_Hit",e.hitFlash/.12f);e.visual.GetComponent<Renderer>().SetPropertyBlock(hitPaint);
        }
    }
    void RefreshCombatFeel(bool visible){
        foreach(var label in damageLabels){
            bool show=visible&&label.remaining>0&&!meta.lowEffects;label.root.gameObject.SetActive(show);
            if(show){var direction=label.root.position-platform.Head.transform.position;label.root.rotation=Quaternion.LookRotation(direction.sqrMagnitude>.001f?direction:platform.Head.transform.forward);}
        }
    }
    public bool EquipModification(int index){
        if(screen!=ScreenState.Journal||!SurvivorQualityRules.ModUnlocked(meta,index))return false;
        int old=meta.weaponModification;meta.weaponModification=index;Save();if(saveError.Length>0){meta.weaponModification=old;return false;}return true;
    }
    public bool ClaimChallenge(int index){
        if(screen!=ScreenState.Journal||index<0||index>=4||!SurvivorQualityRules.ChallengeComplete(meta,index)||(meta.claimedChallenges&(1<<index))!=0)return false;
        int coins=meta.coins,claimed=meta.claimedChallenges;
        meta.coins=(int)System.Math.Min(int.MaxValue,(long)coins+SurvivorQualityRules.ChallengeRewards[index]);meta.claimedChallenges|=1<<index;Save();
        if(saveError.Length>0){meta.coins=coins;meta.claimedChallenges=claimed;return false;}
        notice="挑战完成 · 金币 +"+SurvivorQualityRules.ChallengeRewards[index];feedback=4;Cue(FeedbackCue.Purchase,PlayerPosition);return true;
    }
}
}
