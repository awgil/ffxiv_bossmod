namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class SweepingImmolationSpread(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SweepingImmolationSpread), new AOEShapeCone(20, 90.Degrees()));
class SweepingImmolationStack(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SweepingImmolationStack), new AOEShapeCone(20, 90.Degrees()));
class PartialTotalImmolation(BossModule module) : Components.CastStackSpread(module, ActionID.MakeSpell(AID.TotalImmolation), ActionID.MakeSpell(AID.PartialImmolation), 6, 5, 8, 8, true);
class ScaldingSignal(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScaldingSignal), new AOEShapeCircle(10));
class ScaldingRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScaldingRing), new AOEShapeDonut(10, 20));
class ScaldingFleetFirst(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeRect(40, 3), ActionID.MakeSpell(AID.ScaldingFleetFirst));

// note: it seems to have incorrect target, but acts like self-targeted
class ScaldingFleetSecond(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScaldingFleetSecond), new AOEShapeRect(60, 3));
