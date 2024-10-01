namespace BossMod;

[ConfigDisplay(Name = "Quest Battles (Solo Duties)", Order = 6)]
public sealed class QuestBattleConfig : ConfigNode
{
    [PropertyDisplay("Enable VBM Quest handler")]
    public bool Enabled = false;

    [PropertyDisplay("Show VBM Quest UI")]
    public bool ShowUI = true;

    [PropertyDisplay("Required maturity for quest modules to be loaded")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("Draw waypoints in game world")]
    public bool ShowWaypoints = false;

    [PropertyDisplay("Use dash abilities for navigation (Smudge, Elusive Jump, etc)")]
    public bool UseDash = true;
}
