using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace SpatialSurvivor.Tests {
public class SurvivorModeEnemyTests {
    static object Get(object obj,string name)=>obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj);
    static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,value);
    static object Call(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
    static SurvivorGame.Enemy[] Pool(SurvivorGame game)=>(SurvivorGame.Enemy[])Get(game,"enemies");
    static string Isolate(SurvivorGame game){
        game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;
        string folder=Path.Combine(Path.GetTempPath(),"SurvivorModes",Guid.NewGuid().ToString("N"));
        Set(game,"progressStore",new SurvivorProgressStore(Path.Combine(folder,"progress.json")));game.meta=new MetaSave();return folder;
    }
    static void Begin(SurvivorGame game,SurvivorMode mode){
        game.Menu();Assert.That(game.SelectMode(mode));game.StartRun();
        Set(game,"spawn",1000f);game.player.health=game.player.maxHealth=100000;game.player.damage=0;
        Array.Clear(game.player.stacks,0,game.player.stacks.Length);game.player.pickup=0;game.player.dodge=0;
    }
    static SurvivorGame.Enemy Spawn(SurvivorGame game,int type){
        var enemy=Pool(game).First(e=>e.type==type&&!e.active);
        Assert.That((bool)Call(game,"Spawn",enemy,type),Is.True,"Spawn type "+type);
        enemy.falling=false;enemy.go.transform.position=game.PlayerPosition+new Vector3(0,SurvivorBestiary.Flying(type)?1.2f:0,2);
        return enemy;
    }
    [Test] public void ModesAndSpeciesHaveDistinctPlayableDefinitions(){
        Assert.That(SurvivorModes.All.Length,Is.EqualTo(4));Assert.That(SurvivorBestiary.Creatures.Length,Is.EqualTo(11));
        Assert.That(SurvivorBestiary.Creatures.Select(c=>c.model).Distinct().Count(),Is.EqualTo(11));
        Assert.That(SurvivorBestiary.EliteSkills.Distinct().Count(),Is.EqualTo(11));
        foreach(var creature in SurvivorBestiary.Creatures){Assert.That(creature.description,Is.Not.Empty);Assert.That(creature.tactic,Is.Not.Empty);}
    }
    [UnityTest] public IEnumerator ModeGoalsSettlementAndInfiniteBossesWork(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string folder=Isolate(game);
        Begin(game,SurvivorMode.EliteHunt);Assert.That(game.RunMode,Is.EqualTo(SurvivorMode.EliteHunt));
        Assert.That(game.SelectMode(SurvivorMode.Classic),Is.False,"Cannot replace a running mode");
        for(int i=0;i<8;i++){
            var enemy=Spawn(game,0);Call(game,"PromoteElite",enemy);game.Damage(enemy,99999,false);Call(game,"DeathStep",.5f);
        }
        Assert.That(game.screen,Is.EqualTo(ScreenState.Victory));Assert.That(game.EliteKills,Is.EqualTo(8));Assert.That(game.EliteBounty,Is.EqualTo(96));
        int expected=SurvivorArmory.CoinReward(game.player.elapsed,8,true)+96+game.TacticalBounty;
        Assert.That(game.lastCoinReward,Is.EqualTo(expected));int coins=game.meta.coins;
        game.Menu();game.Menu();Assert.That(game.meta.coins,Is.EqualTo(coins),"Only one settlement per run");
        Begin(game,SurvivorMode.EliteHunt);game.player.elapsed=180;game.Step(.01f);
        Assert.That(game.screen,Is.EqualTo(ScreenState.Defeat));Assert.That(game.EliteKills,Is.Zero);
        Begin(game,SurvivorMode.CrystalDefense);var original=game.CorePosition;
        game.platform.Head.transform.position+=Vector3.right;Assert.That(game.EnemyGoal,Is.EqualTo(original));
        float health=game.player.health;Call(game,"HitObjective",40f);Assert.That(game.CoreHealth,Is.EqualTo(260));Assert.That(game.player.health,Is.EqualTo(health));
        Call(game,"Visuals");Assert.That(((Transform)Get(game,"coreModel")).gameObject.activeSelf);
        game.Pause();game.Step(10);Assert.That(game.player.elapsed,Is.Zero);game.Pause();
        game.player.elapsed=180;game.Step(.01f);Assert.That(game.screen,Is.EqualTo(ScreenState.Victory));
        Begin(game,SurvivorMode.CrystalDefense);Call(game,"HitObjective",300f);Assert.That(game.screen,Is.EqualTo(ScreenState.Defeat));
        Begin(game,SurvivorMode.CrystalDefense);game.player.elapsed=120;Assert.That((bool)Call(game,"BossSpawnStep"),Is.False);
        var attacker=Spawn(game,1);attacker.go.transform.position=game.CorePosition+Vector3.forward*.25f;attacker.abilityCooldown=100;
        Call(game,"EnemyStep",.02f);Assert.That(game.CoreHealth,Is.LessThan(300),"Contact damage attacks the crystal");
        Begin(game,SurvivorMode.Endless);
        for(int encounter=1;encounter<=4;encounter++){
            game.player.elapsed=120*encounter;Assert.That((bool)Call(game,"BossSpawnStep"));Assert.That(game.BossEncounter,Is.EqualTo(encounter));
            game.Damage(game.Boss,99999,false);Assert.That(game.screen,Is.EqualTo(ScreenState.Playing),"Endless never ends after boss three");
            Call(game,"DeathStep",.5f);Assert.That(game.NextBossTime,Is.EqualTo(120*(encounter+1)));
        }
        Assert.That(game.Boss.maxHp,Is.GreaterThan(6000));game.Menu();Call(game,"Visuals");
        Assert.That(((Transform)Get(game,"coreModel")).gameObject.activeSelf,Is.False);
        Begin(game,SurvivorMode.Classic);Assert.That(game.NextBossTime,Is.EqualTo(120));Assert.That(game.EliteBounty,Is.Zero);
        game.Menu();Directory.Delete(folder,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator EveryMonsterCastsAndEliteStateResetsOnReuse(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string folder=Isolate(game);
        foreach(int type in new[]{0,1,2,3,5,6,7,8,9,10}){
            Begin(game,SurvivorMode.Classic);var enemy=Spawn(game,type);int hp=enemy.hp;int exp=enemy.exp;var scale=enemy.baseScale;
            enemy.abilityCooldown=0;Call(game,"EnemyAbilityStep",enemy,.01f);
            if(type==1||type==9){
                Assert.That(enemy.abilityWindup,Is.GreaterThan(0));var before=enemy.go.transform.position;
                Call(game,"EnemyAbilityStep",enemy,1f);Call(game,"EnemyAbilityStep",enemy,.2f);
                Assert.That(Vector3.Distance(before,enemy.go.transform.position),Is.GreaterThan(.3f),"Charge actually travels");
            }else if(type==2){
                Assert.That(enemy.ward,Is.EqualTo(hp*.2f));game.Damage(enemy,10,false);Assert.That(enemy.hp,Is.EqualTo(hp));
                Call(game,"EnemyAbilityStep",enemy,3.1f);game.Damage(enemy,10,false);Assert.That(enemy.hp,Is.EqualTo(hp-10));
            }else if(type==0||type==3||type==10){
                Assert.That(game.HostileZoneCount,Is.GreaterThan(0));float health=game.player.health;
                Call(game,"HostileSpellStep",1f);Call(game,"HostileSpellStep",.02f);Assert.That(game.player.health,Is.LessThan(health));
            }else{
                Assert.That(game.HostileBoltCount,Is.GreaterThan(0));float health=game.player.health;
                for(int i=0;i<120;i++)Call(game,"HostileSpellStep",.02f);
                Assert.That(game.player.health,Is.LessThan(health),"Projectile must hit: "+type);
            }
            Call(game,"PromoteElite",enemy);Assert.That(enemy.elite);Assert.That(enemy.maxHp,Is.EqualTo(Mathf.CeilToInt(hp*2.6f)));
            Assert.That(enemy.exp,Is.EqualTo(exp*4));Assert.That(enemy.baseScale,Is.EqualTo(scale),"Elites retain small creature size");
            Call(game,"Visuals");Assert.That(enemy.eliteCrown.activeSelf);Assert.That(enemy.healthBar.Visible);
            game.Damage(enemy,999999,false);Assert.That(game.EliteBounty,Is.EqualTo(12));Call(game,"DeathStep",.5f);
            Assert.That((bool)Call(game,"Spawn",enemy,type));Assert.That(enemy.elite,Is.False);Assert.That(enemy.ward,Is.Zero);Assert.That(enemy.eliteCrown.activeSelf,Is.False);
        }
        Begin(game,SurvivorMode.Classic);var healer=Spawn(game,8);var ally=Spawn(game,0);ally.go.transform.position=healer.go.transform.position+Vector3.right*.7f;ally.hp=10;
        healer.abilityCooldown=0;Call(game,"EnemyAbilityStep",healer,.01f);Assert.That(ally.hp,Is.EqualTo(14));
        Call(game,"PromoteElite",healer);healer.abilityCooldown=0;Call(game,"EnemyAbilityStep",healer,.01f);Assert.That(ally.hp,Is.EqualTo(22));
        // Spell marks target a fixed position; walking out prevents their damage.
        Begin(game,SurvivorMode.Classic);var caster=Spawn(game,10);caster.abilityCooldown=0;Call(game,"EnemyAbilityStep",caster,.01f);
        var head=game.platform.Head.transform;var origin=head.position;head.position+=Vector3.right*1.5f;float beforeHealth=game.player.health;
        Call(game,"HostileSpellStep",1f);Call(game,"HostileSpellStep",.1f);Assert.That(game.player.health,Is.EqualTo(beforeHealth));head.position=origin;
        game.Pause();int count=game.HostileZoneCount;game.Step(10);Assert.That(game.HostileZoneCount,Is.EqualTo(count));game.Pause();
        game.Damage(caster,99999,false);Call(game,"HostileSpellStep",.01f);Assert.That(game.HostileZoneCount,Is.Zero,"Dead owner cancels hazards");
        Begin(game,SurvivorMode.Classic);caster=Spawn(game,7);caster.abilityCooldown=0;Call(game,"EnemyAbilityStep",caster,.01f);
        caster.generation++;Call(game,"HostileSpellStep",.01f);Assert.That(game.HostileBoltCount,Is.Zero,"Pooled spawn cannot inherit projectiles");
        game.Menu();Directory.Delete(folder,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator ModeSelectionEliteDirectorAndCodexAreReachable(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string folder=Isolate(game);var ui=game.GetComponent<SurvivorUI>();
        game.Menu();game.screen=ScreenState.Modes;Call(ui,"Build");yield return null;Capture(game,"v240-modes");
        var buttons=(System.Collections.Generic.List<Button>)Get(ui,"buttons");buttons.Single(b=>b.name=="晶核守卫").onClick.Invoke();Call(ui,"Build");
        Assert.That(game.SelectedMode,Is.EqualTo(SurvivorMode.CrystalDefense));
        buttons=(System.Collections.Generic.List<Button>)Get(ui,"buttons");buttons.Single(b=>b.name=="开始 · 晶核守卫").onClick.Invoke();
        Assert.That(game.RunMode,Is.EqualTo(SurvivorMode.CrystalDefense));Assert.That(game.player.Owns(UpgradeId.ORBITING_SWORD));Assert.That(game.player.Owns(UpgradeId.LIGHTNING_DOMAIN));
        Begin(game,SurvivorMode.EliteHunt);Set(game,"spawn",0f);
        for(int i=0;i<7;i++){Call(game,"ModeStep",.8f);Call(game,"SpawnStep",.8f);}
        Assert.That(Pool(game).Any(e=>e.active&&e.elite),Is.True,"Hunt director reliably produces elites");
        Begin(game,SurvivorMode.Classic);game.player.elapsed=480;Set(game,"completedBosses",2);Set(game,"spawn",0f);
        for(int i=0;i<80;i++){
            Call(game,"ModeStep",.8f);Call(game,"SpawnStep",.8f);
            foreach(var e in Pool(game))if(e.active){game.Damage(e,99999,false);Call(game,"DeathStep",.5f);}
        }
        Assert.That(game.EliteKills,Is.GreaterThan(0),"Classic also includes elite encounters");
        game.Menu();game.screen=ScreenState.Bestiary;
        for(int type=7;type<11;type++){
            Set(ui,"libraryIndex",type);Set(ui,"elitePreview",false);Call(ui,"Build");yield return null;Capture(game,"v240-creature-"+type);
            Assert.That(((GameObject)Get(ui,"previewModel")).GetComponent<SurvivorCreatureAnimator>(),Is.Not.Null);
            Set(ui,"elitePreview",true);Call(ui,"Build");Assert.That(Object.FindObjectsOfType<Text>().Any(t=>t.text.Contains(SurvivorBestiary.EliteSkills[type])));
        }
        yield return null;Capture(game,"v240-elite-codex");
        game.Menu();Directory.Delete(folder,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator MixedSpeciesAndElitesKeepMovingWithoutOverlapping(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string folder=Isolate(game);Begin(game,SurvivorMode.CrystalDefense);
        var active=new System.Collections.Generic.List<SurvivorGame.Enemy>();
        foreach(int type in new[]{0,1,2,3,5,6,7,8,9,10}){
            var enemy=Pool(game).First(e=>e.type==type);Assert.That((bool)Call(game,"Spawn",enemy,type));
            if(type%2==0)Call(game,"PromoteElite",enemy);enemy.damage=0;active.Add(enemy);
        }
        for(int frame=0;frame<900;frame++)game.Step(1/90f);
        var positions=active.Select(e=>e.go.transform.position).ToArray();var travel=new float[active.Count];
        for(int frame=0;frame<270;frame++){
            game.Step(1/90f);
            for(int i=0;i<active.Count;i++){
                var e=active[i];travel[i]+=Vector3.Distance(positions[i],e.go.transform.position);positions[i]=e.go.transform.position;
                Assert.That(e.active&&!e.falling,Is.True);
                if(!SurvivorBestiary.Flying(e.type)){
                    Assert.That(game.platform.Navigation.Contains(e.go.transform.position,e.radius));
                    for(int j=i+1;j<active.Count;j++)if(!SurvivorBestiary.Flying(active[j].type))
                        Assert.That(SurvivorGame.Distance(e.go.transform.position,active[j].go.transform.position),Is.GreaterThanOrEqualTo(SurvivorEnemyCrowd.Radius(e)+SurvivorEnemyCrowd.Radius(active[j])+.02f));
                }
            }
        }
        for(int i=0;i<travel.Length;i++)Assert.That(travel[i],Is.GreaterThan(.1f),"Species must keep moving: "+active[i].type);
        Assert.That(game.CoreHealth,Is.EqualTo(300));Call(game,"Visuals");
        var camera=game.platform.Head;var pos=camera.transform.position;var rot=camera.transform.rotation;
        camera.transform.position=game.CorePosition+new Vector3(0,2,-2.6f);camera.transform.LookAt(game.CorePosition);
        foreach(var enemy in active){enemy.animator.Tick(.7f);enemy.healthBar.Refresh(enemy,camera,true,game.BossName);}
        Call(game,"RefreshModePresentation",true);yield return null;Capture(game,"v240-defense-mixed-elites");
        camera.transform.SetPositionAndRotation(pos,rot);game.Menu();Directory.Delete(folder,true);yield return new ExitPlayMode();
    }
    static void Capture(SurvivorGame game,string name){
        var camera=game.platform.Head;var oldTarget=camera.targetTexture;var old=RenderTexture.active;
        var target=new RenderTexture(1500,1100,24);var pixels=new Texture2D(1500,1100,TextureFormat.RGB24,false);
        try{Canvas.ForceUpdateCanvases();camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1500,1100),0,0);pixels.Apply();Directory.CreateDirectory("Docs/Validation");File.WriteAllBytes("Docs/Validation/"+name+".png",pixels.EncodeToPNG());}
        finally{camera.targetTexture=oldTarget;RenderTexture.active=old;Object.Destroy(target);Object.Destroy(pixels);}
    }
}
}
