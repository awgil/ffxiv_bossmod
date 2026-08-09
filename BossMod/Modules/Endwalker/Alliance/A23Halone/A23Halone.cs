namespace BossMod.Endwalker.Alliance.A23Halone;

sealed class RainOfSpearsFirst(BossModule module) : Components.CastCounter(module, (uint)AID.RainOfSpearsFirst);
sealed class RainOfSpearsRest(BossModule module) : Components.CastCounter(module, (uint)AID.RainOfSpearsRest);
sealed class SpearsThree(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SpearsThreeAOE, 5f);
sealed class WrathOfHalone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WrathOfHaloneAOE, 25f); // TODO: verify falloff
sealed class GlacialSpearSmall(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearSmall);
sealed class GlacialSpearLarge(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearLarge);
sealed class IceDart(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.IceDart, 6f);
sealed class IceRondel(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.IceRondel, 6f, 8, 8);
sealed class Niphas(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Niphas, 9f);
sealed class FurysAegis(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.Shockwave, (uint)AID.FurysAegisAOE1,
(uint)AID.FurysAegisAOE2, (uint)AID.FurysAegisAOE3, (uint)AID.FurysAegisAOE4, (uint)AID.FurysAegisAOE5,
(uint)AID.FurysAegisAOE6]);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911u, NameID = 12064u, PlanLevel = 90)]
public sealed class A23Halone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700f, 600f), new ArenaBoundsCircle(29.5f));
