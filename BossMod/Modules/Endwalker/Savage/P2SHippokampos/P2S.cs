namespace BossMod.Endwalker.Savage.P2SHippokampos;

class DoubledImpact(BossModule module) : Components.CastSharedTankbuster(module, AID.DoubledImpact, 6);
class SewageEruption(BossModule module) : Components.StandardAOEs(module, AID.SewageEruptionAOE, 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 811, NameID = 10348, PlanLevel = 90)]
public class P2S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
