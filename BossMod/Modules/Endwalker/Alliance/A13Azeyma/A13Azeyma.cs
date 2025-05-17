namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WardensWarmth(BossModule module) : Components.SpreadFromCastTargets(module, AID.WardensWarmthAOE, 6);
class FleetingSpark(BossModule module) : Components.StandardAOEs(module, AID.FleetingSpark, new AOEShapeCone(60, 135.Degrees()));
class SolarFold(BossModule module) : Components.StandardAOEs(module, AID.SolarFoldAOE, new AOEShapeCross(30, 5));
class Sunbeam(BossModule module) : Components.StandardAOEs(module, AID.Sunbeam, new AOEShapeCircle(9), 14);
class SublimeSunset(BossModule module) : Components.StandardAOEs(module, AID.SublimeSunsetAOE, 40); // TODO: check falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11277, SortOrder = 5)]
public class A13Azeyma(WorldState ws, Actor primary) : BossModule(ws, primary, new(-750, -750), new ArenaBoundsCircle(30));
