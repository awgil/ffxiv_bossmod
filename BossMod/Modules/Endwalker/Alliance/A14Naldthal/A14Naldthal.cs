namespace BossMod.Endwalker.Alliance.A14Naldthal;

class GoldenTenet(BossModule module) : Components.CastSharedTankbuster(module, AID.GoldenTenetAOE, 6);
class StygianTenet(BossModule module) : Components.SpreadFromCastTargets(module, AID.StygianTenetAOE, 6);
class HellOfFireFront(BossModule module) : Components.StandardAOEs(module, AID.HellOfFireFrontAOE, new AOEShapeCone(60, 90.Degrees()));
class HellOfFireBack(BossModule module) : Components.StandardAOEs(module, AID.HellOfFireBackAOE, new AOEShapeCone(60, 90.Degrees()));
class WaywardSoul(BossModule module) : Components.StandardAOEs(module, AID.WaywardSoulAOE, new AOEShapeCircle(18), 3);
class SoulVessel(BossModule module) : Components.Adds(module, (uint)OID.SoulVesselReal);
class Twingaze(BossModule module) : Components.StandardAOEs(module, AID.Twingaze, new AOEShapeCone(60, 15.Degrees()));
class MagmaticSpell(BossModule module) : Components.StackWithCastTargets(module, AID.MagmaticSpellAOE, 6, 8);
class TippedScales(BossModule module) : Components.CastCounter(module, AID.TippedScalesAOE);

// TODO: balancing counter
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11286, SortOrder = 6)]
public class A14Naldthal(WorldState ws, Actor primary) : BossModule(ws, primary, new(750, -750), new ArenaBoundsCircle(30));
