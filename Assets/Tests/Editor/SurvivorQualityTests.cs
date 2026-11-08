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
public class SurvivorQualityTests {
    static object Get(object obj,string name)=>obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(obj);
    static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).SetValue(obj,value);
    static object Call(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
    static SurvivorGame.Enemy[] Pool(SurvivorGame game)=>(SurvivorGame.Enemy[])Get(game,"enemies");
    static string Isolate(SurvivorGame game){
        game.enabled=false;game.GetComponent<SurvivorUI>().enabled=false;
        string dir=Path.Combine(Path.GetTempPath(),"SurvivorQuality",Guid.NewGuid().ToString("N"));
        Set(game,"progressStore",new SurvivorProgressStore(Path.Combine(dir,"save.json")));game.meta=new MetaSave();return dir;
    }
    static void Begin(SurvivorGame game,SurvivorMode mode=SurvivorMode.Classic){
        game.Menu();game.SelectMode(mode);game.StartRun();game.player.health=game.player.maxHealth=100000;game.player.critChance=game.player.dodge=0;
        game.player.damage=20;game.player.pickup=0;Array.Clear(game.player.stacks,0,game.player.stacks.Length);Set(game,"spawn",1000f);
    }
    static SurvivorGame.Enemy Spawn(SurvivorGame game,int type=0){
        var enemy=Pool(game).First(e=>e.type==type&&!e.active&&e.deathRemaining<=0);Assert.That((bool)Call(game,"Spawn",enemy,type));enemy.falling=false;return enemy;
    }
    static Button Button(SurvivorUI ui,string name)=>((System.Collections.Generic.List<Button>)Get(ui,"buttons")).Single(b=>b.name==name);
    [Test] public void HeavySwingsModsAndUnlocksHaveRealTradeoffs(){
        Assert.That(SurvivorQualityRules.SwingMultiplier(1),Is.EqualTo(1));Assert.That(SurvivorQualityRules.SwingMultiplier(3),Is.EqualTo(1.6f));
        Assert.That(SurvivorQualityRules.ModDamage(WeaponModification.Impact),Is.GreaterThan(1));Assert.That(SurvivorQualityRules.ModInterval(WeaponModification.Impact),Is.GreaterThan(1));
        Assert.That(SurvivorQualityRules.ModDamage(WeaponModification.Quickcast),Is.LessThan(1));Assert.That(SurvivorQualityRules.ModInterval(WeaponModification.Quickcast),Is.LessThan(1));
        var save=new MetaSave();Assert.That(SurvivorQualityRules.ModUnlocked(save,1),Is.False);save.totalKills=50;save.totalElites=8;save.totalBosses=3;
        for(int i=0;i<=3;i++)Assert.That(SurvivorQualityRules.ModUnlocked(save,i));Assert.That(SurvivorQualityRules.ModUnlocked(save,-1),Is.False);
        save.completedModes=15;Assert.That(SurvivorQualityRules.ChallengeComplete(save,3));
    }
    [Test] public void ComfortPreferencesRoundTripAndFixedHeadingStaysStable(){
        string dir=Path.Combine(Path.GetTempPath(),"QualitySave",Guid.NewGuid().ToString("N"));var eye=new GameObject("Eye");
        try{
            var store=new SurvivorProgressStore(Path.Combine(dir,"save.json"));var save=new MetaSave{hudDistance=2.1f,hudDrop=.7f,hudScale=.8f,compactHud=true,fixedHud=true,lowEffects=true,totalKills=75,weaponModification=1};store.Save(save);save=store.Load(out _);
            Assert.That(save.totalKills,Is.EqualTo(75));Assert.That(save.weaponModification,Is.EqualTo(1));Assert.That(save.lowEffects);
            var placement=new SurvivorUIPlacement();placement.Configure(save);eye.transform.position=Vector3.up*1.7f;placement.Tick(eye.transform,.01f);var before=placement.Position;
            eye.transform.rotation=Quaternion.Euler(0,110,0);for(int i=0;i<300;i++)placement.Tick(eye.transform,1/90f);
            Assert.That(Vector3.Distance(before,placement.Position),Is.LessThan(.001f));Assert.That((before-eye.transform.position).y,Is.EqualTo(-.7f).Within(.001f));
            Assert.That(SurvivorQualityRules.HudDistance(float.NaN),Is.EqualTo(1.6f));
            save.hudDistance=99;save.hudScale=0;save.totalElites=-2;save.weaponModification=99;store.Save(save);save=store.Load(out _);
            Assert.That(save.hudDistance,Is.EqualTo(2.2f));Assert.That(save.hudScale,Is.EqualTo(1));Assert.That(save.totalElites,Is.Zero);Assert.That(save.weaponModification,Is.Zero);
        }finally{Object.DestroyImmediate(eye);if(Directory.Exists(dir))Directory.Delete(dir,true);}
    }
    [UnityTest] public IEnumerator LightAndHeavyHitsReactAndRespectFurniture(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string dir=Isolate(game);game.meta.equippedWeapon=1;Begin(game);
        var target=Spawn(game);target.go.transform.position=game.PlayerPosition+new Vector3(0,0,.65f);target.hp=target.maxHp=1000;
        var origin=game.PlayerPosition+Vector3.up*.2f;var pose=new SurvivorWeaponTracking();var a=Quaternion.Euler(0,-15,0);var b=Quaternion.Euler(0,15,0);
        void Sample(Quaternion rotation,float time,float dt){pose.Sample(new SurvivorPointerSample(SurvivorPointerSource.RightController,new Ray(origin,rotation*Vector3.forward),false),rotation,time);Call(game,"ProcessHandPose",pose,dt);}
        Sample(a,0,.02f);for(int i=1;i<=15;i++)Sample(Quaternion.Slerp(a,b,i/15f),i*.02f,.02f);Assert.That(target.hp,Is.EqualTo(966));Assert.That(game.LastHeavyHit,Is.False);
        Set(game,"meleeClock",.5f);Sample(a,.32f,.02f);Assert.That(target.hp,Is.EqualTo(911));Assert.That(game.LastHeavyHit);Assert.That(game.ConfirmedHandHits,Is.EqualTo(2));
        Set(game,"runModification",WeaponModification.Quickcast);Set(game,"meleeClock",.85f);Sample(b,.34f,.02f);
        Assert.That(target.hp,Is.EqualTo(862),"Quickcast reduces hand damage to 90%");
        var next=(float[])Get(game,"meleeNextHit");Assert.That(next[Array.IndexOf(Pool(game),target)]-.85f,Is.EqualTo(.256f).Within(.001f),"Physical swings use the equipped core interval");
        Set(game,"meleeClock",1.13f);Sample(a,.36f,.02f);Assert.That(target.hp,Is.EqualTo(813),"Quickcast allows another swing before the original 0.32 second interval");
        var before=target.go.transform.position;Call(game,"ImpactMovement",target,.1f);Assert.That(Vector3.Distance(target.go.transform.position,before),Is.GreaterThan(.1f));
        Call(game,"CombatFeelStep",.01f);var block=new MaterialPropertyBlock();target.visual.GetComponent<Renderer>().GetPropertyBlock(block);Assert.That(block.GetFloat("_Hit"),Is.GreaterThan(0));
        var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=RoomNavigation.Layer;wall.transform.position=target.go.transform.position+Vector3.forward*.35f;wall.transform.localScale=new Vector3(1,1,.1f);Physics.SyncTransforms();
        var stopped=target.go.transform.position;target.knockDirection=Vector3.forward;target.knockRemaining=2;for(int i=0;i<20;i++)Call(game,"ImpactMovement",target,.02f);
        Assert.That(target.go.transform.position.z,Is.LessThan(wall.transform.position.z));Object.DestroyImmediate(wall);
        game.Pause();int hp=target.hp;game.Step(1);Assert.That(target.hp,Is.EqualTo(hp));game.Pause();
        Call(game,"Visuals");yield return null;Capture(game,"v250-impact",true);
        game.Menu();Directory.Delete(dir,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator BossModelsAndTelegraphsMatchDifferentHitRules(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string dir=Isolate(game);Begin(game);var head=game.platform.Head.transform;var origin=head.position;
        string[] names={"ThornSentinel","MoonRegent","ElderBloom"};var meshes=new System.Collections.Generic.HashSet<Mesh>();
        for(int i=0;i<3;i++){
            game.player.elapsed=new[]{120f,300f,600f}[i];Assert.That((bool)Call(game,"BossSpawnStep"));Assert.That(game.Boss.visual.name,Does.StartWith(names[i]));meshes.Add(game.Boss.visual.GetComponent<SkinnedMeshRenderer>().sharedMesh);
            game.Boss.go.transform.position=game.PlayerPosition+Vector3.forward*2;Set(game,"bossAttackIndex",i==2?2:1);game.Boss.telegraph=game.Boss.aoe=0;
            Call(game,"BossCombatStep",game.Boss,.01f);Assert.That((int)Get(game,"bossPattern"),Is.EqualTo(i==0?2:i==1?3:4));
            Call(game,"Visuals");yield return null;Capture(game,"v250-boss-"+(i+1),true);
            if(i==0){
                var start=(Vector3)Get(game,"bossChargeStart");var end=(Vector3)Get(game,"bossChargeEnd");head.position=Vector3.Lerp(start,end,.5f)+Vector3.up*1.65f;
                float health=game.player.health;Call(game,"BossCombatStep",game.Boss,2f);Assert.That(game.player.health,Is.LessThan(health));
                Set(game,"bossDash",0f);game.Boss.aoe=0;Set(game,"bossAttackIndex",1);Call(game,"BossCombatStep",game.Boss,.01f);
                head.position+=Vector3.right;health=game.player.health;Call(game,"BossCombatStep",game.Boss,2f);Assert.That(game.player.health,Is.EqualTo(health),"Step sideways to dodge the thrust");
            }else if(i==1){
                var center=((Vector3[])Get(game,"bossTargets"))[0];head.position=center+Vector3.up*1.65f;float health=game.player.health;
                Call(game,"BossCombatStep",game.Boss,2f);Assert.That(game.player.health,Is.EqualTo(health),"Moon ring centre is safe");
                game.Boss.aoe=0;Set(game,"bossAttackIndex",1);Call(game,"BossCombatStep",game.Boss,.01f);head.position=center+Vector3.right*1.3f+Vector3.up*1.65f;
                Call(game,"BossCombatStep",game.Boss,2f);Assert.That(game.player.health,Is.LessThan(health),"Standing between moon rings causes damage");
            }else{Call(game,"BossCombatStep",game.Boss,2f);Assert.That(game.HostileBoltCount,Is.EqualTo(5));}
            head.position=origin;game.Damage(game.Boss,999999,false);Call(game,"DeathStep",.5f);
        }
        Assert.That(meshes.Count,Is.EqualTo(3));Assert.That(game.screen,Is.EqualTo(ScreenState.Victory));Assert.That(game.meta.totalBosses,Is.EqualTo(3));
        game.Menu();Directory.Delete(dir,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator DefenseResourcesAndTrapsChangeTheFight(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string dir=Isolate(game);Begin(game,SurvivorMode.CrystalDefense);
        Assert.That(game.RepairCore(),Is.False);Call(game,"HitObjective",100f);Assert.That(game.RepairCore());Assert.That(game.CoreHealth,Is.EqualTo(260));Assert.That(game.DefenseEnergy,Is.EqualTo(10));Assert.That(game.PlaceFrostTrap(),Is.False);
        for(int i=0;i<5;i++){var victim=Spawn(game);game.Damage(victim,99999,false);Call(game,"DeathStep",.5f);}
        Assert.That(game.DefenseEnergy,Is.EqualTo(25));Assert.That(game.PlaceFrostTrap());Assert.That(game.DefenseEnergy,Is.EqualTo(10));Assert.That(game.ActiveTrapCount,Is.EqualTo(1));
        var traps=(Array)Get(game,"frostTraps");var point=(Vector3)Get(traps.GetValue(0),"point");var enemy=Spawn(game);enemy.go.transform.position=point;int hp=enemy.hp;Call(game,"TacticsStep",.01f);
        Assert.That(enemy.hp,Is.LessThan(hp));Assert.That(enemy.slow,Is.GreaterThan(0));game.Pause();game.Step(10);Assert.That(game.ActiveTrapCount,Is.EqualTo(1));Assert.That(game.PlaceFrostTrap(),Is.False);game.Pause();
        Call(game,"TacticsStep",8.1f);Assert.That(game.ActiveTrapCount,Is.Zero);game.Menu();Assert.That(game.PlaceFrostTrap(),Is.False);
        Begin(game);Assert.That(game.PlaceFrostTrap(),Is.False);Assert.That(game.ActiveTrapCount,Is.Zero);game.Menu();Directory.Delete(dir,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator CampsHuntCombosAndChallengesSettleOnce(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string dir=Isolate(game);Begin(game);
        game.player.elapsed=44.99f;game.Step(.02f);Assert.That(game.screen,Is.EqualTo(ScreenState.Camp));float clock=game.player.elapsed;game.Step(10);Assert.That(game.player.elapsed,Is.EqualTo(clock));
        Assert.That(game.ChooseCamp(2));Assert.That(game.TacticalBounty,Is.EqualTo(30));Assert.That(game.ChooseCamp(2),Is.False);
        game.Menu();int coins=game.meta.coins;game.Menu();Assert.That(game.meta.coins,Is.EqualTo(coins));
        Begin(game,SurvivorMode.EliteHunt);
        for(int i=0;i<3;i++){game.player.elapsed=new[]{10,20,40}[i];var elite=Spawn(game);Call(game,"PromoteElite",elite);game.Damage(elite,99999,false);Call(game,"DeathStep",.5f);}
        Assert.That(game.TacticalBounty,Is.EqualTo(4));Assert.That(game.EliteBounty,Is.EqualTo(36));game.Menu();Assert.That(game.meta.totalElites,Is.EqualTo(3));
        Begin(game,SurvivorMode.Endless);game.player.elapsed=120;Call(game,"BossSpawnStep");game.Damage(game.Boss,99999,false);game.Step(.01f);
        Assert.That(game.screen,Is.EqualTo(ScreenState.Camp));Assert.That(game.ChooseCamp(0));Assert.That(game.TrialRounds,Is.EqualTo(1));Assert.That((float)Get(game,"handDamageBonus"),Is.GreaterThan(1));
        game.Menu();Begin(game);Assert.That(game.TrialRounds,Is.Zero);Assert.That((float)Get(game,"handDamageBonus"),Is.EqualTo(1));game.Menu();
        game.screen=ScreenState.Journal;Assert.That(game.ClaimChallenge(0),Is.False);game.meta.totalKills=50;Assert.That(game.ClaimChallenge(0));coins=game.meta.coins;Assert.That(game.ClaimChallenge(0),Is.False);Assert.That(game.meta.coins,Is.EqualTo(coins));Assert.That(game.EquipModification(1));Assert.That(game.EquipModification(2),Is.False);
        string blocker=Path.Combine(dir,"blocked");File.WriteAllText(blocker,"block folder");Set(game,"progressStore",new SurvivorProgressStore(Path.Combine(blocker,"save.json")));game.meta.totalElites=8;
        LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("存档失败："));Assert.That(game.ClaimChallenge(1),Is.False);Assert.That((game.meta.claimedChallenges&2),Is.Zero);Assert.That(game.meta.coins,Is.EqualTo(coins));
        Set(game,"progressStore",new SurvivorProgressStore(Path.Combine(dir,"save.json")));game.Save();var restored=((SurvivorProgressStore)Get(game,"progressStore")).Load(out _);Assert.That(restored.weaponModification,Is.EqualTo(1));Assert.That(restored.totalElites,Is.EqualTo(8));
        game.Menu();Directory.Delete(dir,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator QualityPagesAndCompactHudAreUsable(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string dir=Isolate(game);var ui=game.GetComponent<SurvivorUI>();game.Menu();Call(ui,"Build");
        Button(ui,"舒适度设置").onClick.Invoke();Call(ui,"Build");Button(ui,"HUD 距离 +").onClick.Invoke();Call(ui,"Build");Assert.That(game.meta.hudDistance,Is.EqualTo(1.7f).Within(.001f));
        yield return null;Capture(game,"v250-comfort");
        game.screen=ScreenState.Guide;Call(ui,"Build");for(int i=0;i<4;i++){Assert.That(Object.FindObjectsOfType<Text>().Any(t=>t.text.StartsWith("行者指南")));Button(ui,"下一页").onClick.Invoke();Call(ui,"Build");}
        yield return null;Capture(game,"v250-guide");game.screen=ScreenState.Journal;game.meta.totalKills=50;Call(ui,"Build");yield return null;Capture(game,"v250-journal");
        Begin(game,SurvivorMode.CrystalDefense);Call(ui,"Build");Set(ui,"nextHud",-1f);Call(ui,"Update");Call(ui,"LateUpdate");
        var hud=(Canvas)Get(ui,"hud");Assert.That(hud.gameObject.activeSelf);Assert.That(((Button)Get(ui,"repairButton")).gameObject.activeSelf);
        Assert.That(((GameObject[])Get(ui,"skillTiles")).Any(t=>t.activeSelf),Is.False);Assert.That(((Text)Get(ui,"healthText")).gameObject.activeInHierarchy);
        game.screen=ScreenState.Camp;Call(ui,"Build");yield return null;Capture(game,"v250-camp");
        game.screen=ScreenState.Bestiary;Set(ui,"libraryIndex",4);for(int i=0;i<3;i++){Call(ui,"Build");Assert.That(((GameObject)Get(ui,"previewModel")).name,Does.StartWith(new[]{"ThornSentinel","MoonRegent","ElderBloom"}[i]));if(i==1){yield return null;Capture(game,"v250-boss-codex");}Button(ui,"切换首领 · "+(i+1)+" / 3").onClick.Invoke();}
        game.Menu();Directory.Delete(dir,true);yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator TwelveMinuteEndlessSoakKeepsPoolsBounded(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();string dir=Isolate(game);Begin(game,SurvivorMode.Endless);
        Set(game,"spawn",0f);game.player.damage=180;game.player.health=game.player.maxHealth=1000000;game.player.pickup=4;
        foreach(var upgrade in SurvivorRules.Weapons)game.player.Apply(upgrade);
        int capacity=game.skillVisuals.Capacity;int objects=Pool(game).Length;float previous=0;long before=GC.GetTotalMemory(true);int checks=0;
        for(int i=0;i<24000&&game.player.elapsed<720;i++){
            if(game.screen==ScreenState.Camp)game.ChooseCamp(i%3);
            if(game.screen==ScreenState.Upgrade){if(game.options.Count>0)game.Select(0);else game.screen=ScreenState.Playing;}
            game.Step(1/30f);Assert.That(game.screen!=ScreenState.Defeat&&game.screen!=ScreenState.Victory);
            Assert.That(game.player.elapsed,Is.GreaterThanOrEqualTo(previous));previous=game.player.elapsed;
            if(i%90==0){Call(game,"Visuals");checks++;foreach(var enemy in Pool(game))if(enemy.active){Assert.That(enemy.hp,Is.GreaterThan(0));Assert.That(float.IsNaN(enemy.go.transform.position.x),Is.False);}}
        }
        Assert.That(game.player.elapsed,Is.GreaterThanOrEqualTo(720));Assert.That(checks,Is.GreaterThan(200));Assert.That(game.skillVisuals.Capacity,Is.EqualTo(capacity));Assert.That(Pool(game).Length,Is.EqualTo(objects));
        long growth=GC.GetTotalMemory(true)-before;Assert.That(growth,Is.LessThan(64L*1024*1024),"Managed objects must not grow unbounded during repeated waves");
        game.Menu();Assert.That(game.meta.bestEndless,Is.GreaterThanOrEqualTo(720));Assert.That(game.meta.completedModes&8,Is.EqualTo(8));Directory.Delete(dir,true);yield return new ExitPlayMode();
    }
    static void Capture(SurvivorGame game,string name,bool combat=false){
        ((Canvas)Get(game.GetComponent<SurvivorUI>(),"hud")).gameObject.SetActive(false);
        var go=new GameObject("Quality review",typeof(Camera));var camera=go.GetComponent<Camera>();camera.CopyFrom(game.platform.Head);camera.stereoTargetEye=StereoTargetEyeMask.None;
        if(combat){((Canvas)Get(game.GetComponent<SurvivorUI>(),"panel")).gameObject.SetActive(false);camera.transform.position=game.PlayerPosition+new Vector3(0,1.7f,-.6f);camera.transform.LookAt(game.PlayerPosition+Vector3.forward*1.5f+Vector3.up*.5f);foreach(var e in Pool(game))if(e.active){e.go.transform.rotation=Quaternion.LookRotation(game.PlayerPosition-e.go.transform.position);e.animator.Tick(.7f);e.healthBar.Refresh(e,camera,true,game.BossName);}}
        else camera.transform.SetPositionAndRotation(game.platform.Head.transform.position,game.platform.Head.transform.rotation);
        var target=new RenderTexture(1500,1100,24);var pixels=new Texture2D(1500,1100,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{Canvas.ForceUpdateCanvases();camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1500,1100),0,0);pixels.Apply();Directory.CreateDirectory("Docs/Validation");File.WriteAllBytes("Docs/Validation/"+name+".png",pixels.EncodeToPNG());}
        finally{RenderTexture.active=old;camera.targetTexture=null;Object.Destroy(target);Object.Destroy(pixels);Object.Destroy(go);}
    }
}
}
