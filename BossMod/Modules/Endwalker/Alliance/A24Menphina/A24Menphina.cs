namespace BossMod.Endwalker.Alliance.A24Menphina;

class BlueMoon(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlueMoonAOE));
class FirstBlush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FirstBlush), new AOEShapeRect(80, 12.5f));
class SilverMirror(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SilverMirrorAOE), new AOEShapeCircle(7));
class Moonset(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MoonsetAOE), new AOEShapeCircle(12));
class LoversBridgeShort(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LoversBridgeShort), new AOEShapeCircle(19));
class LoversBridgeLong(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LoversBridgeLong), new AOEShapeCircle(19));
class CeremonialPillar(BossModule module) : Components.Adds(module, (uint)OID.CeremonialPillar);
class AncientBlizzard(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(45, 22.5f.Degrees()));
class KeenMoonbeam(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.KeenMoonbeamAOE), 6);
class RiseOfTheTwinMoons(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RiseOfTheTwinMoons));
class CrateringChill(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrateringChillAOE), new AOEShapeCircle(20));
class MoonsetRays(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MoonsetRaysAOE), 6, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12063)]
public class A24Menphina(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 750), new ArenaBoundsCircle(30));
