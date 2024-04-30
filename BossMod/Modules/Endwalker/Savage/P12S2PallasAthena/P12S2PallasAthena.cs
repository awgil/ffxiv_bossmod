namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 943, NameID = 12382, SortOrder = 2)]
public class P12S2PallasAthena(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 95), DefaultBounds)
{
    public static readonly ArenaBoundsRect DefaultBounds = new(20, 15);
    public static readonly ArenaBoundsCircle SmallBounds = new(7);
}
