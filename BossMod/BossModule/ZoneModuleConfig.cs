namespace BossMod;

[ConfigDisplay(Name = "Full duty automation", Order = 6)]
public sealed class ZoneModuleConfig : ConfigNode
{
    [PropertyDisplay("Required maturity for zone modules to be loaded")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("Enable automatic execution of quest battles / solo duties")]
    public bool EnableQuestBattles = false;

    [PropertyDisplay("Draw waypoints in game world")]
    public bool ShowWaypoints = false;

    [PropertyDisplay("Use dash abilities for navigation (Smudge, Elusive Jump, etc)")]
    public bool UseDash = true;

    [PropertyDisplay("Show xan debug UI")]
    public bool ShowXanDebugger = false;
}
