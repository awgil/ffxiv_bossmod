namespace BossMod.Endwalker.Alliance.A14Naldthal;

class GoldenTenet() : Components.CastSharedTankbuster(ActionID.MakeSpell(AID.GoldenTenetAOE), 6);
class StygianTenet() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.StygianTenetAOE), 6);
class HellOfFireFront() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.HellOfFireFrontAOE), new AOEShapeCone(60, 90.Degrees()));
class HellOfFireBack() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.HellOfFireBackAOE), new AOEShapeCone(60, 90.Degrees()));
class WaywardSoul() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.WaywardSoulAOE), new AOEShapeCircle(18), 3);
class SoulVessel() : Components.Adds((uint)OID.SoulVesselReal);
class Twingaze() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Twingaze), new AOEShapeCone(60, 15.Degrees()));
class MagmaticSpell() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.MagmaticSpellAOE), 6, 8);
class TippedScales() : Components.CastCounter(ActionID.MakeSpell(AID.TippedScalesAOE));

// TODO: balancing counter
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11286, SortOrder = 6)]
public class A14Naldthal(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(750, -750), 30));