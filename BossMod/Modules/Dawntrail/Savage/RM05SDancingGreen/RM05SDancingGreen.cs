namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class RM05SDancingGreenConfig : ConfigNode
{
    [PropertyDisplay("Seconds in advance to show spotlight safe spots (default: 15)")]
    [PropertySlider(5, 30)]
    public float SpotlightHintSeconds = 15;
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1020, NameID = 13778, PlanLevel = 100, Contributors = "xan")]
public class RM05SDancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
