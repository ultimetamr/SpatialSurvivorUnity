# 2.6.1 · 幻域守卫 / 剑方向修复

- 作品正式更名为「幻域守卫 / REALM WARDENS」，主菜单、章节页及 Android 桌面名称统一更新。
- 启用翡翠符文剑与月冠新图标，覆盖 Android 应用、启动 Activity 与备用图标。
- 所有剑统一以本地 +Z 为剑尖；手持剑去掉额外 90 度旋转，握柄与护手位置重新对齐。
- 环绕飞剑与万剑归宗沿实际圆周运动切线朝前；剑雨剑尖向下，拖尾保持在运动后方。
- 保留原应用包名与存档兼容，更新安装可继续原进度。

## 原因与实现

旧飞剑模型沿 +Y 建模，手持/环绕分别补 90°，剑雨又在父级补 75°，朝向约定不统一。环绕位置使用 (cos a, sin a)，旧朝向直接用欧拉 yaw=a，与实际顺时针切线不一致。

本版将两种剑资产统一导出为 +Z 剑尖、竖直剑身宽面、握柄中心为原点；增加握柄、护手、剑尖标记。手持剑直接跟随追踪旋转，根节点向前 0.07 米 + 剑尖 0.83 米，与 0.9 米近战范围一致。环绕根节点朝向顺时针切线 (sin a, 0, -cos a)；普通/进化模型共用约定。剑雨在每次对象池发射时朝世界向下，拖尾在本地 -Z 即世界向上。商店预览单独转为竖直展示。

应用包名保留 com.example.spatialsurvivor.unity，Android 按包名定位旧进度；Windows 编辑器继续读取原产品名对应的保存目录。项目目录与 GitHub 地址保持原路径。

## 图标

新图标由内置 image_gen 生成，原始输出保留在 Codex generated_images；项目使用 1024×1024 PNG。当前源文件 Assets/Resources/Icons/AppIcon.png，交付文件 Marketing/RealmWardens-icon.png；为兼容旧资料路径同步更新 Marketing/SpatialSurvivor-icon.png。完整提示词见 Marketing/RealmWardens-icon-prompt.md。

## 验证

完整回归 **142/142 通过**，结果 `Validation/v261-blade-brand-tests.xml`。新增 5 项用例：两种剑资产实际顶点/标记轴向、追踪姿势下的握柄/剑尖/伤害范围、普通/进化环绕剑沿实际运动切线、剑雨三次复用后朝向与拖尾。

编辑器人工检查：`v261-hand-sword.png`、`v261-orbit-swords.png`、`v261-sword-rain.png`、`v261-menu.png`、`v261-armory-Shop.png`。预览中的青线用于方向审阅，不属于游戏新增 UI。

本轮先完成修复、最后打包，暂不覆盖安装或发布 GitHub。

## APK 验证

- Build 成功：Builds/SpatialSurvivorUnity.apk，80,833,747 字节；2.6.1 / versionCode 20，ARM64，minSdk 29 / targetSdk 35。
- aapt 回读应用名称与启动 Activity 名称均为「幻域守卫」。应用、圆形与启动图标引用新源图标。提取 APK 图标与项目 AppIcon.png 二进制完全相同，证据 Validation/v261-apk-icon.png。
- apksigner 验证成功，v2 签名，1 个签名者；当前仍为开发构建 / 调试签名。
- SHA256：7acdd3b9f05a7a7d0f3af722d217309a236cd4dae22a80aa166fb78bffd99576。
- 未覆盖安装真机；实际手柄握持体验与设备桌面缓存需真机确认。
