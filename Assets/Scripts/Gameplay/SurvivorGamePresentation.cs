using System;
using System.IO;
using UnityEngine;

namespace SpatialSurvivor {
public sealed partial class SurvivorGame {
 public SurvivorSkillVisuals skillVisuals {get;private set;}
 public SurvivorFeedback Feedback {get;private set;}
 public float DamageFlashUntil {get;private set;}
 public Enemy Boss=>enemies[18];
 public string LegacyImportPath=>Path.Combine(SurvivorIdentity.SaveDirectory,"legacy-progress.xml");
 void BuildPresentation(){skillVisuals=gameObject.AddComponent<SurvivorSkillVisuals>();var parents=new Transform[swords.Length];for(int i=0;i<swords.Length;i++)parents[i]=swords[i].transform;skillVisuals.Initialize(parents);Feedback=gameObject.AddComponent<SurvivorFeedback>();BuildBossPresentation();BuildModePresentation();BuildEnemyAbilities();BuildCombatFeel();BuildTactics();}
 void Cue(FeedbackCue cue,Vector3 position){if(Feedback)Feedback.Play(cue,position);}
 void DeathStep(float dt){foreach(var enemy in enemies){if(enemy.deathRemaining<=0)continue;enemy.deathRemaining=Mathf.Max(0,enemy.deathRemaining-dt);if(enemy.animator)enemy.animator.Tick(dt);if(enemy.deathRemaining<=0)enemy.go.SetActive(false);}}
 void ClearWorld(){ResetCombatFeel();RefreshCombatFeel(false);RefreshTactics(false);ResetEnemyAbilities();RefreshModePresentation(false);ResetHandCombat();foreach(var enemy in enemies){enemy.active=false;enemy.deathRemaining=0;enemy.go.SetActive(false);}foreach(var shot in shots)shot.go.SetActive(false);foreach(var crystal in crystals){crystal.active=false;crystal.go.SetActive(false);}foreach(var sword in swords)sword.SetActive(false);foreach(var effect in fx){effect.life=0;effect.line.enabled=false;}if(skillVisuals)skillVisuals.ResetVisuals();if(Feedback)Feedback.Stop();}
 public bool TryImportLegacy(bool report=true){if(screen!=ScreenState.Menu&&screen!=ScreenState.Progression){if(report)notice="请返回主菜单后导入";return false;}if(!File.Exists(LegacyImportPath)){if(report){notice="未找到原版导出存档 legacy-progress.xml";feedback=5;}return false;}try{var xml=File.ReadAllText(LegacyImportPath);bool result=LegacyProgressMigration.MergeOnce(meta,xml,out var message);if(result)Save();if(report){notice=message;feedback=5;}return result;}catch(Exception exception){if(report){notice="原版进度导入失败："+exception.Message;feedback=5;}return false;}}
 public bool ImportLegacyXml(string xml,out string message){if(screen!=ScreenState.Menu&&screen!=ScreenState.Progression){message="请返回主菜单后导入";return false;}bool success=LegacyProgressMigration.MergeOnce(meta,xml,out message);if(success)Save();notice=message;feedback=5;return success;}
}
}
