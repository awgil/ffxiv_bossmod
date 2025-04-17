namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ByregotStrikeJump(BossModule module) : Components.LocationTargetedAOEs(module, AID.ByregotStrike, 8);
class ByregotStrikeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ByregotStrikeKnockback, 20);
class ByregotStrikeCone(BossModule module) : Components.SelfTargetedAOEs(module, AID.ByregotStrikeAOE, new AOEShapeCone(90, 15.Degrees()));
