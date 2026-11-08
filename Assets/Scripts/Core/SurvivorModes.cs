namespace SpatialSurvivor {
public enum SurvivorMode { Classic, EliteHunt, CrystalDefense, Endless }
public sealed class SurvivorModeDefinition {
    public readonly string name,description,objective;
    public SurvivorModeDefinition(string name,string description,string objective){this.name=name;this.description=description;this.objective=objective;}
}
public static class SurvivorModes {
    public static readonly SurvivorModeDefinition[] All={
        new SurvivorModeDefinition("经典生存","成长、进化并挑战第 2 / 5 / 10 分钟的三场首领战。","击败最终首领"),
        new SurvivorModeDefinition("精英狩猎","180 秒内猎杀 8 只精英。开局携带飞剑与雷域。","180 秒内击败 8 只精英"),
        new SurvivorModeDefinition("晶核守卫","保护开局位置的晶核 3 分钟。怪物优先攻击晶核，玩家可绕行拦截。","晶核存活 180 秒"),
        new SurvivorModeDefinition("无尽试炼","持续增强的怪潮，每 2 分钟出现一次首领。死亡或退出时结算。","挑战更高生存时间")
    };
    public static SurvivorModeDefinition Get(SurvivorMode mode)=>All[(int)mode];
}
}
