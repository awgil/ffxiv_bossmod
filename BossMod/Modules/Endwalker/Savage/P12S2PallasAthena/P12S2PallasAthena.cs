namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 943, NameID = 12382, SortOrder = 2)]
public class P12S2PallasAthena(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds)
{
    public static ArenaBoundsRect DefaultBounds = new ArenaBoundsRect(new(100, 95), 20, 15);
    public static ArenaBoundsCircle SmallBounds = new ArenaBoundsCircle(new(100, 90), 7);
}
