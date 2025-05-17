namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class DestructiveStatic(BossModule module) : Components.StandardAOEs(module, AID.DestructiveStatic, new AOEShapeCone(50, 90.Degrees()));
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBoltAOE, 6);
class BoltsFromTheBlue(BossModule module) : Components.CastCounter(module, AID.BoltsFromTheBlueAOE);
class DestructiveStrike(BossModule module) : Components.BaitAwayCast(module, AID.DestructiveStrike, new AOEShapeCone(13, 60.Degrees())); // TODO: verify angle

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11274, SortOrder = 2)]
public class A10RhalgrEmissary(WorldState ws, Actor primary) : BossModule(ws, primary, new(74, 516), new ArenaBoundsCircle(25));
