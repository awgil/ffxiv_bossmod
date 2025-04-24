namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class BrutalImpact(BossModule module) : Components.CastCounter(module, AID.BrutalImpactAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 990, NameID = 13356, PlanLevel = 100)]
public class RM03SBruteBomber(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));
