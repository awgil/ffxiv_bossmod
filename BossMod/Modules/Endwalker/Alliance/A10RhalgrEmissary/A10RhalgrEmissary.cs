namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class DestructiveStatic() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.DestructiveStatic), new AOEShapeCone(50, 90.Degrees()));
class LightningBolt() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.LightningBoltAOE), 6);
class BoltsFromTheBlue() : Components.CastCounter(ActionID.MakeSpell(AID.BoltsFromTheBlueAOE));
class DestructiveStrike() : Components.BaitAwayCast(ActionID.MakeSpell(AID.DestructiveStrike), new AOEShapeCone(13, 60.Degrees())); // TODO: verify angle

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11274, SortOrder = 2)]
public class A10RhalgrEmissary(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(74, 516), 25));