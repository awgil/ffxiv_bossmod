namespace BossMod.Endwalker.Alliance.A23Halone;

class RainOfSpearsFirst(BossModule module) : Components.CastCounter(module, AID.RainOfSpearsFirst);
class RainOfSpearsRest(BossModule module) : Components.CastCounter(module, AID.RainOfSpearsRest);
class SpearsThree(BossModule module) : Components.BaitAwayCast(module, AID.SpearsThreeAOE, new AOEShapeCircle(5), true);
class WrathOfHalone(BossModule module) : Components.StandardAOEs(module, AID.WrathOfHaloneAOE, new AOEShapeCircle(25)); // TODO: verify falloff
class GlacialSpearSmall(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearSmall);
class GlacialSpearLarge(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearLarge);
class IceDart(BossModule module) : Components.SpreadFromCastTargets(module, AID.IceDart, 6);
class IceRondel(BossModule module) : Components.StackWithCastTargets(module, AID.IceRondel, 6);
class Niphas(BossModule module) : Components.StandardAOEs(module, AID.Niphas, new AOEShapeCircle(9));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12064)]
public class A23Halone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, 600), new ArenaBoundsCircle(30));
