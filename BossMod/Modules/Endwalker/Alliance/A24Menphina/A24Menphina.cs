namespace BossMod.Endwalker.Alliance.A24Menphina;

class BlueMoon() : Components.CastCounter(ActionID.MakeSpell(AID.BlueMoonAOE));
class FirstBlush() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.FirstBlush), new AOEShapeRect(80, 12.5f));
class SilverMirror() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.SilverMirrorAOE), new AOEShapeCircle(7));
class Moonset() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.MoonsetAOE), new AOEShapeCircle(12));
class LoversBridgeShort() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.LoversBridgeShort), new AOEShapeCircle(19));
class LoversBridgeLong() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.LoversBridgeLong), new AOEShapeCircle(19));
class CeremonialPillar() : Components.Adds((uint)OID.CeremonialPillar);
class AncientBlizzard() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(45, 22.5f.Degrees()));
class KeenMoonbeam() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.KeenMoonbeamAOE), 6);
class RiseOfTheTwinMoons() : Components.CastCounter(ActionID.MakeSpell(AID.RiseOfTheTwinMoons));
class CrateringChill() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.CrateringChillAOE), new AOEShapeCircle(20));
class MoonsetRays() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.MoonsetRaysAOE), 6, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12063)]
public class A24Menphina(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(800, 750), 25));