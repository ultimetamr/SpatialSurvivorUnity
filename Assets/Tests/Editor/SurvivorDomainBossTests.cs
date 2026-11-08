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
public class SurvivorDomainBossTests {
    static object Get(object obj,string name)=>obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj);
    static void Set(object obj,string name,object value)=>obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,value);
    static object Call(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);

    [TestCase(SkillVisual.PoisonAura)]
    [TestCase(SkillVisual.NetherPoison)]
    [TestCase(SkillVisual.LightningField)]
    [TestCase(SkillVisual.MagneticField)]
    public void DomainsHaveAnimatedBatchedArtworkAtTheirDamageRadius(SkillVisual kind){
        var root=new GameObject("Domain test");
        try{
            var go=SurvivorVisualCatalog.Create(kind,root.transform);var field=go.GetComponent<SurvivorDomainField>();
            field.SetRadius(3.1f);Assert.That(field.VertexCount,Is.GreaterThan(1000));
            Assert.That(go.GetComponentsInChildren<Renderer>().Length,Is.EqualTo(1),"Domain artwork must use a single renderer");
            var mesh=go.GetComponent<MeshFilter>().sharedMesh;var before=mesh.vertices;
            go.GetComponent<SurvivorMagicEffect>().Tick(.27f);
            Assert.That(mesh.vertices.Where((v,i)=>v!=before[i]).Any(),Is.True);
            Assert.That(mesh.bounds.size.x,Is.InRange(6.1f,6.3f));
            Assert.That(mesh.bounds.max.y,Is.LessThan(.5f),"Domain motes must leave the MR eye line clear");
            foreach(var color in mesh.colors)Assert.That(color.a,Is.InRange(0,1));
        }finally{Object.DestroyImmediate(root);}
    }

    [UnityTest] public IEnumerator DomainsBossesAndLiveHealthSurviveCombatAndLifecycle(){
        EditorSceneManager.OpenScene("Assets/Scenes/SpatialSurvivor.unity");yield return new EnterPlayMode();yield return null;yield return null;
        var game=Object.FindObjectOfType<SurvivorGame>();game.enabled=false;var ui=game.GetComponent<SurvivorUI>();ui.enabled=false;
        string dir=Path.Combine(Path.GetTempPath(),"SurvivorBoss",Guid.NewGuid().ToString("N"));
        Set(game,"progressStore",new SurvivorProgressStore(Path.Combine(dir,"progress.json")));game.meta=new MetaSave();
        Assert.That(game.Ready);game.StartRun();Call(ui,"Build");
        game.player.maxHealth=game.player.health=100000;game.player.damage=0;game.player.pickup=0;
        Array.Clear(game.player.stacks,0,game.player.stacks.Length);Set(game,"spawn",100f);
        var pool=(SurvivorGame.Enemy[])Get(game,"enemies");
        Assert.That((bool)Call(game,"Spawn",pool[0],0));Assert.That((bool)Call(game,"Spawn",pool[19],5));
        pool[0].go.transform.position=game.PlayerPosition+new Vector3(-.6f,0,1.3f);
        pool[19].go.transform.position=game.PlayerPosition+new Vector3(.8f,.8f,1.8f);
        game.player.stacks[(int)UpgradeId.POISON_AURA]=3;game.player.stacks[(int)UpgradeId.LIGHTNING_DOMAIN]=2;
        Call(game,"Visuals");game.skillVisuals.Tick(.9f);Call(game,"Visuals");
        var fields=game.GetComponentsInChildren<SurvivorDomainField>(true);
        var poison=fields.Single(f=>f.name=="PoisonAura");var lightning=fields.Single(f=>f.name=="LightningField");
        Assert.That(poison.Radius,Is.EqualTo(game.player.range*1.35f).Within(.001f));
        Assert.That(lightning.Radius,Is.EqualTo(game.player.range*1.55f).Within(.001f));
        game.Damage(pool[0],13,false);Call(game,"Visuals");
        Assert.That(pool[0].healthBar.DisplayedHealth,Is.EqualTo("27 / 40"));Assert.That(pool[0].healthBar.Fill,Is.EqualTo(.675f).Within(.001f));
        Assert.That(pool[19].healthBar.Visible,Is.True,"Flying monsters also need current HP");
        yield return null;Capture(game,"v230-poison-and-lightning");
        game.Pause();float clock=poison.Clock;float elapsed=game.player.elapsed;game.Step(.5f);Call(game,"Visuals");
        Assert.That(poison.Clock,Is.EqualTo(clock));Assert.That(game.player.elapsed,Is.EqualTo(elapsed));game.Pause();
        game.player.stacks[(int)UpgradeId.NETHER_POISON_DOMAIN]=1;Call(game,"Visuals");game.skillVisuals.Tick(.6f);
        Assert.That(poison.gameObject.activeSelf,Is.False);var nether=fields.Single(f=>f.name=="NetherPoison");
        Assert.That(nether.gameObject.activeSelf);Assert.That(nether.Radius,Is.EqualTo(game.player.range*2.7f).Within(.001f));yield return null;Capture(game,"v230-nether-domain");
        // Real damage must stop outside the same displayed boundary.
        game.player.stacks[(int)UpgradeId.LIGHTNING_DOMAIN]=0;game.player.damage=20;
        pool[0].hp=pool[0].maxHp=500;pool[0].go.transform.position=game.PlayerPosition+Vector3.forward*(nether.Radius-.05f);
        Call(game,"WeaponStep",.3f);Assert.That(pool[0].hp,Is.LessThan(500));int hp=pool[0].hp;
        pool[0].go.transform.position=game.PlayerPosition+Vector3.forward*(nether.Radius+.1f);Call(game,"WeaponStep",.3f);Assert.That(pool[0].hp,Is.EqualTo(hp));
        game.player.damage=0;Array.Clear(game.player.stacks,0,game.player.stacks.Length);
        game.player.elapsed=119;Assert.That((bool)Call(game,"BossSpawnStep"),Is.False);
        game.player.elapsed=120;Assert.That((bool)Call(game,"BossSpawnStep"),Is.True);
        Assert.That(game.Boss.active);Assert.That(game.BossEncounter,Is.EqualTo(1));Assert.That(game.Boss.hp,Is.EqualTo(900));
        Assert.That(pool.Count(e=>e.active),Is.EqualTo(1),"Boss entrance clears normal monsters");
        Call(game,"Visuals");Assert.That(pool[0].healthBar.Visible,Is.False);Assert.That(game.Boss.healthBar.Visible);
        game.Boss.go.transform.position=game.PlayerPosition+Vector3.forward*2.3f;
        Call(game,"BossCombatStep",game.Boss,2.3f);Assert.That(game.Boss.telegraph,Is.GreaterThan(1));
        var fixedTarget=((Vector3[])Get(game,"bossTargets"))[0];Call(game,"Visuals");
        Assert.That(((LineRenderer[])Get(game,"bossWarnings")).Count(l=>l.enabled),Is.EqualTo(2));
        game.Pause();float charge=game.Boss.telegraph;game.Step(1);Assert.That(game.Boss.telegraph,Is.EqualTo(charge));game.Pause();
        Vector3 origin=game.platform.Head.transform.position;game.platform.Head.transform.position=fixedTarget+Vector3.up*1.65f;
        float health=game.player.health;Call(game,"BossCombatStep",game.Boss,1.6f);Assert.That(game.player.health,Is.LessThan(health),"Standing in a completed telegraph causes damage");
        game.platform.Head.transform.position=origin;
        game.Boss.hp=220;Assert.That(game.BossPhase,Is.EqualTo(3));Call(game,"BossCombatStep",game.Boss,5f);
        Assert.That((int)Get(game,"bossPattern"),Is.EqualTo(2),"First boss uses its own directional thrust");
        game.platform.Head.transform.position=origin+Vector3.right*1.2f;health=game.player.health;
        Call(game,"BossCombatStep",game.Boss,1.4f);Assert.That(game.player.health,Is.EqualTo(health),"Moving sideways out of the locked thrust avoids damage");
        game.platform.Head.transform.position=origin;
        // A slow first encounter must not become a victory just because the clock
        // reaches ten minutes before that earlier boss is defeated.
        game.player.elapsed=600;
        game.Damage(game.Boss,99999,false);Assert.That(game.Boss.hp,Is.Zero);Assert.That(game.screen,Is.EqualTo(ScreenState.Playing));
        Assert.That(game.NextBossTime,Is.EqualTo(300));Call(game,"DeathStep",.5f);
        game.player.elapsed=300;Call(game,"BossSpawnStep");Assert.That(game.BossEncounter,Is.EqualTo(2));Assert.That(game.Boss.maxHp,Is.EqualTo(2400));
        game.Damage(game.Boss,99999,false);Assert.That(game.screen,Is.EqualTo(ScreenState.Playing));Call(game,"DeathStep",.5f);
        game.player.elapsed=600;Call(game,"BossSpawnStep");Assert.That(game.BossEncounter,Is.EqualTo(3));Assert.That(game.Boss.maxHp,Is.EqualTo(6000));
        game.Boss.go.transform.position=game.PlayerPosition+Vector3.forward*2.4f;game.Damage(game.Boss,4200,false);
        Call(game,"BossCombatStep",game.Boss,2.3f);Call(game,"Visuals");Call(ui,"Update");
        Assert.That(game.Boss.healthBar.DisplayedHealth,Is.EqualTo("1800 / 6000"));Call(game,"CombatPresentationStep",.6f);yield return null;Capture(game,"v230-boss-telegraph-health");
        game.Damage(game.Boss,99999,false);Assert.That(game.screen,Is.EqualTo(ScreenState.Victory));
        game.Menu();Call(game,"Visuals");Assert.That(pool.Any(e=>e.healthBar.Visible),Is.False);Assert.That(fields.Any(f=>f.gameObject.activeSelf),Is.False);
        game.StartRun();Assert.That(game.NextBossTime,Is.EqualTo(120));Assert.That(game.BossEncounter,Is.Zero);
        var room=new GameObject("Small room boss fixture");var center=new Vector3(100,0,100);
        void Box(Vector3 offset,Vector3 scale){var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.layer=RoomNavigation.Layer;box.transform.SetParent(room.transform);box.transform.position=center+offset;box.transform.localScale=scale;}
        Box(new Vector3(0,-.1f,0),new Vector3(3.6f,.2f,3.6f));
        Box(new Vector3(0,1.2f,1.8f),new Vector3(3.6f,2.4f,.1f));Box(new Vector3(0,1.2f,-1.8f),new Vector3(3.6f,2.4f,.1f));
        Box(new Vector3(1.8f,1.2f,0),new Vector3(.1f,2.4f,3.6f));Box(new Vector3(-1.8f,1.2f,0),new Vector3(.1f,2.4f,3.6f));
        Physics.SyncTransforms();var originalHead=game.platform.Head.transform.position;game.platform.Head.transform.position=center+Vector3.up*1.65f;
        game.platform.Navigation.Rebuild(game.platform.Head.transform.position);
        Assert.That(game.platform.Navigation.Ready);Assert.That(game.platform.Navigation.Spawn(game.PlayerPosition,3,.475f,new System.Random(3),out _,out _),Is.False);
        game.player.elapsed=120;Assert.That((bool)Call(game,"BossSpawnStep"),Is.True,"Small rooms must still spawn the scheduled boss");
        Assert.That(game.Boss.active);Assert.That(game.platform.Navigation.Contains(game.Boss.go.transform.position,game.Boss.radius));
        Assert.That(SurvivorGame.Distance(game.PlayerPosition,game.Boss.go.transform.position),Is.GreaterThanOrEqualTo(1.2f));
        game.Menu();Object.DestroyImmediate(room);Physics.SyncTransforms();game.platform.Head.transform.position=originalHead;game.platform.Navigation.Rebuild(originalHead);
        game.Menu();if(Directory.Exists(dir))Directory.Delete(dir,true);
        yield return new ExitPlayMode();
    }
    static void Capture(SurvivorGame game,string name){
        var go=new GameObject("Validation camera",typeof(Camera));var c=go.GetComponent<Camera>();c.CopyFrom(game.platform.Head);c.stereoTargetEye=StereoTargetEyeMask.None;
        c.transform.position=game.PlayerPosition+new Vector3(0,2.45f,-3.6f);c.transform.LookAt(game.PlayerPosition+Vector3.forward*.7f);c.fieldOfView=65;
        var target=new RenderTexture(1600,1100,24);var image=new Texture2D(1600,1100,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{
            var pool=(SurvivorGame.Enemy[])Get(game,"enemies");foreach(var e in pool){if(e.active)e.animator.Tick(.7f);e.healthBar.Refresh(e,c,true,game.BossName);}
            Canvas.ForceUpdateCanvases();
            c.targetTexture=target;c.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1600,1100),0,0);image.Apply();
            Directory.CreateDirectory("Docs/Validation");File.WriteAllBytes("Docs/Validation/"+name+".png",image.EncodeToPNG());
            // Verify actual pixels, not merely HP strings on invisible UI objects.
            var shown=image.GetPixels32();foreach(var e in pool)if(e.active)e.go.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases();c.Render();image.ReadPixels(new Rect(0,0,1600,1100),0,0);image.Apply();var hidden=image.GetPixels32();
            int changed=0;for(int i=0;i<shown.Length;i++)if(Mathf.Abs(shown[i].r-hidden[i].r)+Mathf.Abs(shown[i].g-hidden[i].g)+Mathf.Abs(shown[i].b-hidden[i].b)>30)changed++;
            foreach(var e in pool)e.healthBar.Refresh(e,game.platform.Head,true,game.BossName);
            Assert.That(changed,Is.GreaterThan(50),"Visible health bars must change rendered pixels: "+name);
        }finally{RenderTexture.active=old;c.targetTexture=null;Object.Destroy(target);Object.Destroy(image);Object.Destroy(go);}
    }
}
}
