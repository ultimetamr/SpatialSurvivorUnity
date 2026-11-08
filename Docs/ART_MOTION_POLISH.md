# 2.6.0 · 生灵律动

## 模型与动画

- 13 种生物保留原有小体型，完善叶脉、披饰、甲壳分层、爪尖、法杖、护盾、月冠与符文细节；荆棘守卫使用森林配色。
- 12 关节扩展至 20 关节：新增双膝、下颌、冠饰、尾尖、披饰、双眼睑。肘部和膝部使用双骨骼权重，减轻弯曲时的断节感。
- 重制全部 78 段动画。小型怪跳扑、重型怪蓄力砸击、甲虫前冲、施法怪抬杖、飞行怪振翅俯冲、荆棘守卫持盾突刺。攻击包含前摇、出手、跟随与收招，冠饰和披饰有延迟摆动。
- 待机加入呼吸和眨眼，移动加入弯膝与摆尾；每次重生错开步频相位，动作切换使用 85 毫秒混合，避免整群同步踏步和姿势突变。
- 首领/冲刺怪的蓄力姿势与实际预警计时绑定；命中只叠加短上身受击，不重置正在进行的攻击。死亡、冻结和复用保持由游戏时钟控制。
- 每个模型仍为一个蒙皮渲染器、一个共享材质；细小零件降低细分，恒定动画通道压缩至两个关键帧。模型三角形 12,184–15,368，完整预算见 Validation/v260-mesh-budgets.csv；不代表真机帧率测量。

## 预览与复现

编辑器执行 `SpatialSurvivor.Editor.SurvivorPolishPreview.Render`，重建资源并生成模型图集、攻击关键姿势与动画帧序列。帧序列保存在 Logs/ArtReview，GIF 保存在 Docs/Validation。

- 模型图集：Validation/v260-creature-gallery.png
- 攻击姿势：Validation/v260-attack-poses.png
- 行走：Validation/v260-walk.gif
- 持盾突刺：Validation/v260-thrust.gif
- 飞龙俯冲：Validation/v260-dive.gif
- 月冠施法：Validation/v260-cast.gif
- 呼吸/眨眼：Validation/v260-idle.gif

上述均为 Unity 编辑器实际模型渲染，使用纯色审阅背景，不是头显透视截图。已检查轮廓、持杖姿势、护盾朝向和攻击序列。

## 自动验证

新增 18 项动画专项用例，覆盖 13 种生物蒙皮权重、循环接缝、动作包围盒、三角形预算、受击期间预警对齐、攻击优先级、冻结/死亡/复用、切换过渡、眨眼和弯膝。完整回归还包括上一轮的玩法与武器检查。

完整回归 **137/137 通过**，结果文件 `Validation/v260-art-motion-tests.xml` 与 `v260-art-motion-summary.json`。本版按用户安排最后打包，暂不真机安装；真实佩戴动作观感与长时性能仍待真机验收。公开 GitHub 仍为 v2.2.3，本轮未自动发布。

## Android 交付

- 本地构建成功：Builds/SpatialSurvivorUnity.apk，81,458,055 字节，开发构建 / 调试签名。
- APK 回读：com.example.spatialsurvivor.unity，2.6.0 / versionCode 19，ARM64，minSdk 29 / targetSdk 35。
- 应用与启动 Activity 都引用原游戏图标 spatial_survivor_icon.png，提取证据 Validation/v260-apk-icon.png。
- apksigner 验证成功：APK Signature Scheme v2，1 个签名者。
- SHA256：3222f76ff056a2f7a56b15e412cb9289fd6e9e41cf9bc34d0ed3307b06201008。
- 包体从上一版约 97.1 MB 降为约 81.5 MB，主要来自恒定动画通道关键帧减少；没有以此推断真机帧率。
- 未安装到头显，未推送 GitHub。
