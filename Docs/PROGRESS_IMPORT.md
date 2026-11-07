# 可选：导入旧安卓永久进度

迁移内容：晶核余额与 8 项永久强化等级。安卓原版没有持久化局内战斗快照，因此不迁移正在进行的战斗。

## 导出

保留原安卓应用安装，在电脑连接已授权 USB 调试的 PICO 设备。原项目的 debug APK 支持 Android run-as。

运行项目内 Tools/Export-AndroidProgress.ps1，传入实际设备序列号：

    .\Tools\Export-AndroidProgress.ps1 -DeviceId YOUR_DEVICE_SERIAL -OutFile .\legacy-progress.xml

脚本只读取 com.example.spatialsurvivor 的 shared_prefs/spatial_survivor_progression.xml，不修改原版进度。若原版是不可调试的 release 包，需要先通过原应用提供的导出方式取出 XML；脚本不会绕过 Android 应用隔离。

## 导入编辑器

启动 Unity 场景并停留在主菜单，选择 SpatialSurvivor → Import Android Progress XML，再选导出的文件。新一局应用永久强化。

## 导入头显

先运行 Unity APK 一次，生成应用目录。再次运行导出脚本并添加 -PushToUnity；脚本会复制到 /sdcard/Android/data/com.example.spatialsurvivor.unity/files/legacy-progress.xml。复制完成后，在 Unity 应用主菜单 → 永久强化 → 导入原版进度中手动执行。应用启动时不自动读取或导入该文件。

## 合并规则

余额和每项等级取安卓与 Unity 的较大值，不相加。首次成功导入后记录标记，后续不再导入，避免消费晶核后反复恢复余额。无效、负数、缺少余额或带 DTD 的文件会被拒绝；等级按当前上限裁剪。导入不修改原版 XML。
