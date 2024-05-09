namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ByregotStrikeJump(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ByregotStrike), 8);
class ByregotStrikeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ByregotStrikeKnockback), 20);
class ByregotStrikeCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeAOE), new AOEShapeCone(90, 15.Degrees()));
