# 作品发布资料

- 作品名称：幻域守卫（Realm Wardens）
- 版本名称：2.6.1 · 幻域守卫
- 版本号：2.6.1（versionCode 20）
- 应用包名：`com.example.spatialsurvivor.unity`
- 平台：PICO / Android ARM64 / MR
- 一句话简介：把客厅变成魔法战场，用挥剑与射击对抗怪潮，在混合现实中打造你的生存流派。
- GitHub：https://github.com/ultimetamr/SpatialSurvivorUnity
- 图标：RealmWardens-icon.png（1024×1024）
- 历史海报：SpatialSurvivor-poster.png（旧名称概念图，非当前版本截图）

## 作品简介

幻域守卫是一款面向 PICO 头显的混合现实肉鸽生存游戏。扫描真实房间后，地面与空中魔法生物进入你的现实空间；你可以真实走位、挥动近战武器，也可以使用星火手枪、符文霰弹枪和自动技能抵挡怪潮。

游戏包含经验升级、随机强化与技能进化，搭配技能手册、怪物图鉴、武器商店和独立局外金币，让每局战斗都能形成不同成长路线。项目使用 Unity 6 与 C# 实现，采用卡通魔法生物、骨骼动画、空间 UI 和魔法特效，并提供编辑器模拟房间与 PICO 真机两种开发验证方式。

## 版本说明

- 作品正式更名为「幻域守卫 / REALM WARDENS」，主菜单、章节页及 Android 桌面名称统一更新。
- 启用翡翠符文剑与月冠新图标，覆盖 Android 应用、启动 Activity 与备用图标。
- 所有剑统一以本地 +Z 为剑尖；手持剑去掉额外 90 度旋转，握柄与护手位置重新对齐。
- 环绕飞剑与万剑归宗沿实际圆周运动切线朝前；剑雨剑尖向下，拖尾保持在运动后方。
- 保留原应用包名与存档兼容，更新安装可继续原进度。

2.6.1 源码提交至 main 供 PICO 云打包审核；公开 GitHub Release 仍为 v2.2.3。云端构建与审核状态需在 PICO 平台确认。交接步骤见 Docs/PICO_CLOUD_BUILD.md，验证记录见 Docs/BLADE_BRANDING_FIX.md。
