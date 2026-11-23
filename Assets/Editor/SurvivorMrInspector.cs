using UnityEditor;
using UnityEngine;
namespace SpatialSurvivor.Editor {
[CustomEditor(typeof(SurvivorMrSession))]
public sealed class SurvivorMrInspector:UnityEditor.Editor {
 public override void OnInspectorGUI(){
  EditorGUILayout.LabelField("PICO MR 房间运行时",EditorStyles.boldLabel);
  EditorGUILayout.HelpBox("负责空间权限、PICO 透视、房间扫描、实时网格、追击导航与房间主题替换。XR Origin 与 Main Camera 已保存在当前场景。\n编辑器 Play 使用模拟房间；真实透视和系统房间扫描需要 PICO 头显。",MessageType.Info);
  if(!Application.isPlaying)return;
  var session=(SurvivorMrSession)target;
  EditorGUILayout.LabelField("状态",session.State.Phase.ToString());EditorGUILayout.HelpBox(session.Diagnostics,MessageType.None);
  using(new EditorGUI.DisabledScope(session.State.Busy)){
   if(GUILayout.Button("扫描 / 重扫房间"))session.ScanRoom();
   if(GUILayout.Button("重新授权 / 重试"))session.Retry();
  }
  EditorGUILayout.HelpBox("原始扫描网格隐藏，房间主题模型保留。",MessageType.None);
  if(GUILayout.Button("魔法场景："+(session.ThemeVisible?"开":"关")))session.ToggleTheme();
  Repaint();
 }
}
}
