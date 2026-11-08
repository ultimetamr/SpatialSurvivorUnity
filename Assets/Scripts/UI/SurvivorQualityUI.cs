using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialSurvivor {
public sealed partial class SurvivorUI {
    ScreenState qualityReturn=ScreenState.Menu;
    int guidePage;
    Text tacticalText;Button repairButton,trapButton;
    void OpenQuality(ScreenState page){qualityReturn=game.screen;guidePage=0;game.screen=page;}
    void ChangeComfort(Action change){change();game.Save();placement.Reset();previous=(ScreenState)(-1);}
    void BuildComfort(){
        Heading("舒适度与战斗视野","按自己的视线习惯调整。精简信息与低特效不会减少攻击预警和怪物血量。");
        Action("HUD 距离 −",-370,143,340,56,()=>ChangeComfort(()=>game.meta.hudDistance=Mathf.Max(1.2f,game.meta.hudDistance-.1f)));
        Copy(panel.transform,game.meta.hudDistance.ToString("F1")+" 米",0,143,300,45,26,Gold,TextAnchor.MiddleCenter);
        Action("HUD 距离 +",370,143,340,56,()=>ChangeComfort(()=>game.meta.hudDistance=Mathf.Min(2.2f,game.meta.hudDistance+.1f)));
        Action("HUD 上移",-370,70,340,56,()=>ChangeComfort(()=>game.meta.hudDrop=Mathf.Max(.25f,game.meta.hudDrop-.05f)));
        Copy(panel.transform,"低于视线 "+game.meta.hudDrop.ToString("F2")+" 米",0,70,300,45,23,Gold,TextAnchor.MiddleCenter);
        Action("HUD 下移",370,70,340,56,()=>ChangeComfort(()=>game.meta.hudDrop=Mathf.Min(.8f,game.meta.hudDrop+.05f)));
        Action("HUD 缩小",-370,-3,340,56,()=>ChangeComfort(()=>game.meta.hudScale=Mathf.Max(.75f,game.meta.hudScale-.05f)));
        Copy(panel.transform,Mathf.RoundToInt(game.meta.hudScale*100)+"%",0,-3,300,45,26,Gold,TextAnchor.MiddleCenter);
        Action("HUD 放大",370,-3,340,56,()=>ChangeComfort(()=>game.meta.hudScale=Mathf.Min(1.2f,game.meta.hudScale+.05f)));
        Action("信息："+(game.meta.compactHud?"精简":"完整"),-277,-86,530,58,()=>ChangeComfort(()=>game.meta.compactHud=!game.meta.compactHud));
        Action("跟随："+(game.meta.fixedHud?"固定朝向":"转头缓跟"),277,-86,530,58,()=>ChangeComfort(()=>game.meta.fixedHud=!game.meta.fixedHud));
        Action("特效："+(game.meta.lowEffects?"简化":"完整"),-277,-162,530,58,()=>ChangeComfort(()=>game.meta.lowEffects=!game.meta.lowEffects));
        Action("普通怪名称："+(game.meta.showEnemyNames?"常显":"瞄准显示"),277,-162,530,58,()=>ChangeComfort(()=>game.meta.showEnemyNames=!game.meta.showEnemyNames));
        Action("返回",0,-255,420,60,()=>game.screen=qualityReturn);
        Footer("距离、位置与大小即时生效；精英、首领、当前血量和危险标记始终保留。");
    }
    void BuildGuide(){
        string[] titles={"准备你的真实房间","挥剑与射击","读懂敌人的攻击","选择自己的战斗路线"};
        string[] text={
            "先在 MR 设置授权并扫描地面，等待房间就绪。\n\n真实走位会改变你与敌人的距离。\n遇到重新定位或边界确认，请先完成系统提示。\n\n战斗 HUD 的距离、大小和高度可在舒适度设置调整。",
            "用手柄射线瞄准菜单，松开扳机后再按下确认。\n\n近战短剑直接挥动即可击打；更快的挥击触发重击。枪械与法杖按住扳机连续攻击，霰弹枪在近距离更容易集中命中。\n\n短暂失去追踪会保留武器显示，恢复前不会造成伤害。",
            "地面法阵先预警，锁定后及时横向离开。\n\n荆棘守卫的长条预警代表突刺；月冠女王的双环之间危险，圆心和外侧安全；古树首领会释放荆棘和扇形弹。\n\n精英金色名字显示晶盾、狂奔或孵化词条。优先处理治疗祭司。",
            "经典生存途中会遇到可选择的林间馈赠。\n\n精英狩猎：18 秒内连续击杀精英可积累连击金币。晶核守卫：击杀获得能量，用 HUD 按钮修复或放冰阱。\n\n无尽试炼：每次击败首领后选择挑战契约。挑战手册记录进度并解锁武器核心。"
        };
        Heading("行者指南 · "+(guidePage+1)+" / 4",titles[guidePage]);
        Image(panel.transform,"Guide page",new Vector2(0,-12),new Vector2(1080,377),Card);
        Copy(panel.transform,text[guidePage],0,-10,970,334,26,Cream);
        Action("上一页",-365,-254,340,60,()=>{guidePage=(guidePage+3)%4;previous=(ScreenState)(-1);});
        Action("返回",0,-254,340,60,()=>game.screen=qualityReturn);
        Action("下一页",365,-254,340,60,()=>{guidePage=(guidePage+1)%4;previous=(ScreenState)(-1);},true);
        Footer("可随时从主菜单或暂停页重新打开指南。");
    }
    void BuildJournal(){
        Heading("行者挑战与武器核心","累计挑战进度永久保存。核心装备后下一局生效，战斗中装备保持不变。");
        for(int i=0;i<4;i++){
            int index=i;float x=i%2==0?-277:277,y=127-i/2*120;
            bool complete=SurvivorQualityRules.ChallengeComplete(game.meta,i),claimed=(game.meta.claimedChallenges&(1<<i))!=0;
            var button=Action(SurvivorQualityRules.ChallengeNames[i],x,y,530,103,()=>{game.ClaimChallenge(index);previous=(ScreenState)(-1);},complete&&!claimed);button.GetComponentInChildren<Text>().text="";
            Color color=complete&&!claimed?Ink:Cream;
            Copy(button.transform,SurvivorQualityRules.ChallengeNames[i]+" · "+(claimed?"已领取":complete?"领取 "+SurvivorQualityRules.ChallengeRewards[i]+" 金币":"奖励 "+SurvivorQualityRules.ChallengeRewards[i]+" 金币"),0,22,480,34,25,color);
            Copy(button.transform,SurvivorQualityRules.ChallengeProgress(game.meta,i),0,-22,480,40,20,color);button.interactable=complete&&!claimed;
        }
        for(int i=0;i<4;i++){
            int index=i;bool unlocked=SurvivorQualityRules.ModUnlocked(game.meta,i);
            var button=Action((game.meta.weaponModification==i?"已装备 · ":unlocked?"":"未解锁 · ")+SurvivorQualityRules.ModNames[i],-414+i*276,-111,264,60,()=>{game.EquipModification(index);previous=(ScreenState)(-1);},game.meta.weaponModification==i);button.interactable=unlocked;
        }
        Copy(panel.transform,SurvivorQualityRules.ModDescriptions[game.meta.weaponModification],0,-180,1080,60,23,Mint,TextAnchor.MiddleCenter);
        Action("返回",0,-260,420,58,()=>game.screen=qualityReturn);
        Footer("四境行者：经典通关、狩猎完成、晶核守卫完成、无尽生存至少 6 分钟。");
    }
    void BuildCamp(){
        bool trial=game.RunMode==SurvivorMode.Endless;
        Heading(game.CampTitle,trial?"战斗已暂停。每份契约使后续结算收益增加 15%，同时改变敌我能力。":"战斗已暂停。选择一次补给，继续构筑本局的成长路线。");
        string[] titles=trial?new[]{"猎风契约","坚壁契约","急咒契约"}:new[]{"研习法术","林间疗愈","带上悬赏"};
        string[] descriptions=trial?new[]{"敌人移速 +12%\n手持武器伤害 +18%","敌人伤害 +15%\n最大生命 +20，恢复 40","敌人移速 +8%\n攻击间隔 -10%"}:new[]{"全伤害强化 +1 级\n已满级时改为恢复 20 生命","恢复最大生命的 35%\n稳住状态再挑战首领","本局额外金币 +30\n敌人移速 +8%"};
        for(int i=0;i<3;i++){
            int choice=i;float x=(i-1)*365;var button=Action(titles[i],x,-5,345,320,()=>game.ChooseCamp(choice),i==0);button.GetComponentInChildren<Text>().text="";
            Color color=i==0?Ink:Cream;Copy(button.transform,titles[i],0,93,305,54,31,color,TextAnchor.MiddleCenter);Icon(button.transform,i==0?13:i==1?9:12,new Vector2(0,20),70);
            Copy(button.transform,descriptions[i],0,-77,300,111,24,color,TextAnchor.MiddleCenter);
        }
        Footer("选择后继续战斗；敌人、弹道和技能计时均在当前页面暂停。");
    }
    void BuildTacticalHud(){
        tacticalText=Label(hud.transform,"",new Vector2(0,-122),new Vector2(800,35),18,Mint);
        repairButton=AddButton(hud.transform,"修复 20",new Vector2(180,-49),new Vector2(140,55),()=>game.RepairCore(),Card,hudButtons);
        trapButton=AddButton(hud.transform,"冰阱 15",new Vector2(345,-49),new Vector2(140,55),()=>game.PlaceFrostTrap(),Card,hudButtons);
    }
    void RefreshQualityHud(){
        bool compact=game.meta.compactHud,defense=game.RunMode==SurvivorMode.CrystalDefense;
        foreach(var tile in skillTiles)if(tile&&compact)tile.SetActive(false);
        if(defense){foreach(var tile in skillTiles)if(tile)tile.SetActive(false);}
        statsText.gameObject.SetActive(!compact&&!defense);
        tacticalText.gameObject.SetActive(compact||defense);tacticalText.text=game.TacticalStatus;
        tacticalText.rectTransform.anchoredPosition=new Vector2(defense?-150:0,compact?-18:-122);
        tacticalText.rectTransform.sizeDelta=new Vector2(defense?470:800,35);
        repairButton.gameObject.SetActive(defense);trapButton.gameObject.SetActive(defense);
        repairButton.interactable=game.DefenseEnergy>=20&&game.CoreHealth<300;trapButton.interactable=game.DefenseEnergy>=15&&game.ActiveTrapCount<3;
        var outline=hud.transform.Find("HUD outer gold").GetComponent<RectTransform>();var background=hud.transform.Find("HUD ink").GetComponent<RectTransform>();
        outline.sizeDelta=new Vector2(840,compact?155:220);background.sizeDelta=outline.sizeDelta-new Vector2(4,4);
        outline.anchoredPosition=background.anchoredPosition=new Vector2(0,compact?33:0);
        repairButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(180,compact?-18:-49);
        trapButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(345,compact?-18:-49);
        hud.transform.localScale=Vector3.one*(.001f*game.meta.hudScale);
    }
}
}
