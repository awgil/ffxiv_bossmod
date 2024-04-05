namespace BossMod.Endwalker.Savage.P8S2;

class TyrantsFlare : Components.LocationTargetedAOEs
{
    public TyrantsFlare() : base(ActionID.MakeSpell(AID.TyrantsFlareAOE), 6) { }
}

// TODO: autoattack component
// TODO: HC components
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 884, NameID = 11399, SortOrder = 2)]
public class P8S2 : BossModule
{
    public P8S2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
}
