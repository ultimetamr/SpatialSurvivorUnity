namespace SpatialSurvivor {
public enum SurvivorMrPhase {Booting,Permission,Starting,Loading,Scanning,Running,Denied,Failed,Simulation}
public sealed class SurvivorMrState {
 public SurvivorMrPhase Phase {get;private set;}=SurvivorMrPhase.Booting;
 public string Detail {get;private set;}="等待 PICO XR 初始化";
 public bool Busy=>Phase==SurvivorMrPhase.Booting||Phase==SurvivorMrPhase.Permission||Phase==SurvivorMrPhase.Starting||Phase==SurvivorMrPhase.Loading||Phase==SurvivorMrPhase.Scanning;
 public bool AllowsCombat=>Phase==SurvivorMrPhase.Running||Phase==SurvivorMrPhase.Simulation;
 public bool CanPlay(bool tracked,bool roomReady)=>AllowsCombat&&tracked&&roomReady;
 public void Set(SurvivorMrPhase phase,string detail){Phase=phase;Detail=detail;}
}
}
