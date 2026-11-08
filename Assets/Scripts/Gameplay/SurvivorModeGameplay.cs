using UnityEngine;
using UnityEngine.UI;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
    public SurvivorMode SelectedMode {get;private set;}
    public SurvivorMode RunMode {get;private set;}
    public int EliteKills {get;private set;}
    public int EliteBounty {get;private set;}
    public float CoreHealth {get;private set;}
    public Vector3 CorePosition {get;private set;}
    public string ModeName=>SurvivorModes.Get(RunMode).name;
    public string ModeObjective=>RunMode==SurvivorMode.EliteHunt?"精英 "+EliteKills+" / 8  · 剩余 "+Mathf.Max(0,Mathf.CeilToInt(180-player.elapsed))+"秒":
        RunMode==SurvivorMode.CrystalDefense?"晶核 "+Mathf.CeilToInt(CoreHealth)+" / 300  · 剩余 "+Mathf.Max(0,Mathf.CeilToInt(180-player.elapsed))+"秒":BossCountdown;
    public int CombatWave=>RunMode==SurvivorMode.Classic?SurvivorRules.Wave(player.elapsed):Mathf.Min(12,1+Mathf.FloorToInt(player.elapsed/(RunMode==SurvivorMode.Endless?80:30)));
    public Vector3 EnemyGoal=>RunMode==SurvivorMode.CrystalDefense?CorePosition:PlayerPosition;
    Transform coreModel;Canvas coreCanvas;Text coreText;Image coreFill;
    float eliteSpawnClock;
    int spawnSequence;
    public bool SelectMode(SurvivorMode mode){
        if((screen!=ScreenState.Menu&&screen!=ScreenState.Modes)||(int)mode<0||(int)mode>=SurvivorModes.All.Length)return false;
        SelectedMode=mode;return true;
    }
    void BuildModePresentation(){
        coreModel=new GameObject("Defend the crystal").transform;coreModel.SetParent(transform,false);
        var gem=Instantiate(Resources.Load<GameObject>("Art/Weapons/FrostCrystal"),coreModel);gem.transform.localScale=Vector3.one*.6f;gem.transform.localPosition=Vector3.up*.35f;
        var go=new GameObject("Crystal health",typeof(RectTransform),typeof(Canvas));go.transform.SetParent(coreModel,false);
        coreCanvas=go.GetComponent<Canvas>();coreCanvas.renderMode=RenderMode.WorldSpace;coreCanvas.sortingOrder=12;
        go.GetComponent<RectTransform>().sizeDelta=new Vector2(340,80);go.transform.localScale=Vector3.one*.0017f;
        var label=new GameObject("Objective health",typeof(RectTransform),typeof(Text));label.transform.SetParent(go.transform,false);
        coreText=label.GetComponent<Text>();coreText.font=Resources.Load<Font>("Fonts/Chinese");coreText.fontSize=27;coreText.alignment=TextAnchor.MiddleCenter;coreText.color=new Color(.6f,.9f,1);coreText.raycastTarget=false;
        label.GetComponent<RectTransform>().sizeDelta=new Vector2(400,60);label.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,24);
        Image Bar(string name,Color color){var child=new GameObject(name,typeof(RectTransform),typeof(Image));child.transform.SetParent(go.transform,false);var img=child.GetComponent<Image>();img.rectTransform.sizeDelta=new Vector2(300,12);img.color=color;img.raycastTarget=false;return img;}
        Bar("Core HP background",new Color(.06f,.1f,.18f,.95f));coreFill=Bar("Core HP",new Color(.35f,.8f,1));coreFill.rectTransform.pivot=new Vector2(0,.5f);coreFill.rectTransform.anchoredPosition=new Vector2(-150,0);
        coreModel.gameObject.SetActive(false);
    }
    void StartMode(){
        RunMode=SelectedMode;EliteKills=EliteBounty=spawnSequence=0;eliteSpawnClock=RunMode==SurvivorMode.EliteHunt?4:25;
        CoreHealth=300;CorePosition=PlayerPosition;ResetEnemyAbilities();
        if(RunMode!=SurvivorMode.Classic){
            player.Apply(UpgradeId.ORBITING_SWORD);player.Apply(UpgradeId.LIGHTNING_DOMAIN);
            history.Add("挑战初始魔法 · 飞剑 / 雷域");
        }
    }
    void ModeStep(float dt){
        if(screen!=ScreenState.Playing)return;
        eliteSpawnClock-=dt;
        if(RunMode==SurvivorMode.EliteHunt){if(EliteKills>=8)Finish(true);else if(player.elapsed>=180)Finish(false);}
        if(RunMode==SurvivorMode.CrystalDefense){if(CoreHealth<=0)Finish(false);else if(player.elapsed>=180)Finish(true);}
    }
    void HitObjective(float damage){
        if(screen!=ScreenState.Playing)return;
        if(RunMode!=SurvivorMode.CrystalDefense){Hurt(damage);return;}
        CoreHealth=Mathf.Max(0,CoreHealth-damage);Cue(FeedbackCue.Hurt,CorePosition);if(CoreHealth<=0)Finish(false);
    }
    bool ChooseElite(){
        if(eliteSpawnClock>0)return false;
        int active=0;foreach(var enemy in enemies)if(enemy.active&&enemy.elite)active++;
        if(active>=(RunMode==SurvivorMode.EliteHunt?3:2))return false;
        eliteSpawnClock=RunMode==SurvivorMode.EliteHunt?14:RunMode==SurvivorMode.Endless?16:28;
        return true;
    }
    void RewardElite(Enemy enemy){
        if(!enemy.elite)return;
        EliteKills++;EliteBounty+=12;
        notice="精英击破 · "+SurvivorBestiary.Creatures[enemy.type].name+" · 额外金币 +12";feedback=2.5f;
    }
    void RefreshModePresentation(bool visible){
        bool show=visible&&RunMode==SurvivorMode.CrystalDefense;coreModel.gameObject.SetActive(show);if(!show)return;
        coreModel.position=CorePosition;coreModel.GetChild(0).localRotation=Quaternion.Euler(0,player.elapsed*30,12);
        var p=CorePosition+Vector3.up*.95f;var forward=p-platform.Head.transform.position;
        coreCanvas.transform.SetPositionAndRotation(p,Quaternion.LookRotation(forward.sqrMagnitude>.001f?forward:platform.Head.transform.forward));
        coreText.text="守护晶核  "+Mathf.CeilToInt(CoreHealth)+" / 300";coreFill.rectTransform.localScale=new Vector3(CoreHealth/300,1,1);
    }
}
}
