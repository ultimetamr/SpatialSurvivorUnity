using UnityEngine;
namespace SpatialSurvivor {
public sealed class SurvivorBootstrap:MonoBehaviour {
 void Awake(){Application.targetFrameRate=90;QualitySettings.vSyncCount=0;if(FindObjectOfType<SurvivorGame>())return;var runtime=new GameObject("SpatialSurvivor Runtime");runtime.AddComponent<SurvivorPlatform>();runtime.AddComponent<SurvivorGame>();}
}
}
