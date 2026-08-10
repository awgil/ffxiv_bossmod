namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[ConfigDisplay(Order = 0x174, Parent = typeof(DawntrailConfig))]
public sealed class IndexConfig : ConfigNode
{
    [PropertyDisplay("Force AI to target closest add when spawned")]
    public bool ForceAddTargeting = false;

    [PropertyDisplay("Force AI to target boss if no adds and no current target")]
    public bool ForceBossTargeting = false;
}
