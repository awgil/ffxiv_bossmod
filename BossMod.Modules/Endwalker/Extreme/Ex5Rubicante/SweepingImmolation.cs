namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class SweepingImmolationSpread(BossModule module) : Components.StandardAOEs(module, AID.SweepingImmolationSpread, new AOEShapeCone(20, 90.Degrees()));
class SweepingImmolationStack(BossModule module) : Components.StandardAOEs(module, AID.SweepingImmolationStack, new AOEShapeCone(20, 90.Degrees()));
class PartialTotalImmolation(BossModule module) : Components.CastStackSpread(module, AID.TotalImmolation, AID.PartialImmolation, 6, 5, 8, 8, true);
class ScaldingSignal(BossModule module) : Components.StandardAOEs(module, AID.ScaldingSignal, new AOEShapeCircle(10));
class ScaldingRing(BossModule module) : Components.StandardAOEs(module, AID.ScaldingRing, new AOEShapeDonut(10, 20));
class ScaldingFleetFirst(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeRect(40, 3), AID.ScaldingFleetFirst);

// note: it seems to have incorrect target, but acts like self-targeted
class ScaldingFleetSecond(BossModule module) : Components.StandardAOEs(module, AID.ScaldingFleetSecond, new AOEShapeRect(60, 3));
