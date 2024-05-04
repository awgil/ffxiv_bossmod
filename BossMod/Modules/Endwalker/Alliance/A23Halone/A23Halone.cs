namespace BossMod.Endwalker.Alliance.A23Halone;

class RainOfSpearsFirst(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RainOfSpearsFirst));
class RainOfSpearsRest(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RainOfSpearsRest));
class SpearsThree(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SpearsThreeAOE), new AOEShapeCircle(5), true);
class WrathOfHalone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WrathOfHaloneAOE), new AOEShapeCircle(25)); // TODO: verify falloff
class GlacialSpearSmall(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearSmall);
class GlacialSpearLarge(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearLarge);
class IceDart(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.IceDart), 6);
class IceRondel(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.IceRondel), 6);
class Niphas(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Niphas), new AOEShapeCircle(9));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12064)]
public class A23Halone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, 600), new ArenaBoundsCircle(30));
