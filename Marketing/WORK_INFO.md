# 作品发布资料

- 作品名称：幻域守卫（Realm Wardens）
- 版本名称：2.7.5 · 魔法光圈修复
- 版本号：2.7.5（versionCode 26）
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

- 恢复战斗时脚下的蓝色魔法光圈，沿用此前缩小一半的显示尺寸。
- 走路的小黄圈继续隐藏，实际吸附功能保留。
- 保留怪物直接追击、仅近身接触伤害、冰冻技能以及扫描房间模型。
- 修复 PICO SDK 日志回调跨线程读取帧号导致的启动崩溃。

验证范围见 Docs/Validation/v275-halo-summary.json；PICO 云端审核未执行。
