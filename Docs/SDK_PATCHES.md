# 本地嵌入包补丁

为了保持项目可离线打开，依赖包以 embedded 形式保留。

- com.plattar.unitygltf/UnityGLTF/Scripts/GLTFUtils.cs：先统一 Windows 路径分隔符再转换为 Assets 相对路径。
- com.plattar.unitygltf/UnityGLTF/Scripts/AssetSerializer.cs：纹理写入后强制同步导入，避免 AssetDatabase.LoadAssetAtPath 返回空。
- com.bytedance.pico.xr/Editor/PXR_Utils.cs 与 PXR_PortalEditor.cs：在 AssetImportWorker 进程跳过项目配置写入，避免 out-of-process 导入异常。

这些修改仅位于本 Unity 工程的嵌入副本，未修改本机其他 Unity 工程或 SDK 源目录。以后升级 SDK 或 GLTF 包时需要重新检查。

Unity 6 升级后，Unity 自有包采用安装目录自带的兼容版本，具体版本见 UNITY6_UPGRADE.md；PICO 与 GLTF 嵌入副本保留上述补丁。
