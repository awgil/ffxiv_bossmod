namespace BossMod.Endwalker.Savage.P8S2;

class TyrantsFlare(BossModule module) : Components.StandardAOEs(module, AID.TyrantsFlareAOE, 6);

// TODO: autoattack component
// TODO: HC components
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 884, NameID = 11399, SortOrder = 2, PlanLevel = 90)]
public class P8S2(ModuleInitializer init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
