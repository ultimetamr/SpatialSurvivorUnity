# 魔法生物美术重制 · 2.0.0

Unity 项目：`E:\DEMO\SpatialSurvivorUnity`。Android 包名保持 `com.example.spatialsurvivor.unity`，versionCode 6。

## 新资源

- 五种角色：苔叶精灵 Mossling、余烬小鬼 EmberImp、晶石守卫 CrystalGuardian、月翼灵 MoonWisp、古树守望者 ElderBloom。
- 每个角色有 12 个关节、一张顶点绘色网格、一个材质，以及 Idle / Walk / Attack / Hit / Spawn / Death 六段真实关节动画。移动、攻击、受击、出生和死亡均接入战斗状态；暂停、追踪丢失和冰冻遵循战斗时钟。对象池重生重置动画。
- 新符文剑、冰晶、熔岩种子、奥术核心和经验晶核网格；各技能使用新的模型与动画魔法环、闪电折线、剑雨、扩散脉冲。
- 生成的菜单森林插画和 4×4 技能图集。提示词、模式与来源记录在 `Docs/Art/generation.json`。
- 主菜单、三/四选一升级、暂停、永久强化、MR 设置、胜利/失败结算和 HUD 全部重新排版。手柄射线与扳机、手势捏合、禁用状态与跨弹窗松手门控继续使用现有输入链路。

资源入口：`Assets/Resources/Art/`。通过 Unity 菜单 `SpatialSurvivor/Art/Rebuild Storybook Assets` 可重建网格、预制体与动画；改造型请编辑 `Assets/Editor/SurvivorArtBuilder.cs`，改动画状态请编辑 `Assets/Scripts/Art/SurvivorCreatureAnimator.cs`。UI 布局位于 `Assets/Scripts/UI/SurvivorUILayout.cs`，射线逻辑保留在 `SurvivorUI.cs`。

## 制作来源

本轮尝试启动 PICO Spatial Editor 的 3D 生成流程，但后台和窗口模式均在启动阶段退出，没有产生可用 3D 结果。已向用户说明改为 Unity 原生源代码制作网格、关节和动画。五个角色不是外部 AI3D 输出，也没有沿用旧 GLB 怪物。插画与图集由内置图像生成工具制作。

旧 GLB 资源保留作历史参考，当前游戏的角色、技能和经验晶核均使用新的 Art 资源。

## 验证

58 项 EditMode 测试通过（含 PlayMode 场景流程）。包括五种角色的六段动画与实际关节旋转、冻结动画时钟、死亡及重生、稀疏寻路帧下走路连续性、持久化网格索引、全部技能资源、战斗/经验/升级/暂停/Boss、MR 遮挡与手柄 UI 点击回归。

`Docs/Validation/art-tests.xml` 为结果。`creature-gallery.png`、`attack-animation.png` 为模型与动作预览；菜单、升级、强化、暂停、MR、胜败和 HUD 截图同目录。

以上图片来自 Unity 编辑器渲染。头显上的清晰度、帧率、透视融合和实际按钮手感仍需佩戴设备确认，不能以编辑器截图代替。
