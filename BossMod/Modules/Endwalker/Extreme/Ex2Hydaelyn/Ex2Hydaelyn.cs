namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// state related to mousa scorn mechanic (shared tankbuster)
class MousaScorn(BossModule module) : Components.CastSharedTankbuster(module, AID.MousaScorn, 4);

// cast counter for pre-intermission AOE
class PureCrystal(BossModule module) : Components.CastCounter(module, AID.PureCrystal);

// cast counter for post-intermission AOE
class Exodus(BossModule module) : Components.CastCounter(module, AID.Exodus);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 791, NameID = 10453, PlanLevel = 90)]
public class Ex2Hydaelyn(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
