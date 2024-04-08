namespace BossMod.Endwalker.Alliance.A23Halone;

class RainOfSpearsFirst() : Components.CastCounter(ActionID.MakeSpell(AID.RainOfSpearsFirst));
class RainOfSpearsRest() : Components.CastCounter(ActionID.MakeSpell(AID.RainOfSpearsRest));
class SpearsThree() : Components.BaitAwayCast(ActionID.MakeSpell(AID.SpearsThreeAOE), new AOEShapeCircle(5), true);
class WrathOfHalone() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.WrathOfHaloneAOE), new AOEShapeCircle(25)); // TODO: verify falloff
class GlacialSpearSmall() : Components.Adds((uint)OID.GlacialSpearSmall);
class GlacialSpearLarge() : Components.Adds((uint)OID.GlacialSpearLarge);
class IceDart() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.IceDart), 6);
class IceRondel() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.IceRondel), 6);
class Niphas() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.Niphas), new AOEShapeCircle(9));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12064)]
public class A23Halone(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(-700, 600), 30));