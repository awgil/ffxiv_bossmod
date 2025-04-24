namespace BossMod.Endwalker.Savage.P9SKokytos;

class GluttonysAugur(BossModule module) : Components.CastCounter(module, AID.GluttonysAugurAOE);
class SoulSurge(BossModule module) : Components.CastCounter(module, AID.SoulSurge);
class BeastlyFury(BossModule module) : Components.CastCounter(module, AID.BeastlyFuryAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 937, NameID = 12369, PlanLevel = 90)]
public class P9SKokytos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
