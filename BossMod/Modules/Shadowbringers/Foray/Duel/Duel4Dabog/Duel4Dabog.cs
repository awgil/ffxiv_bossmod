namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class RightArmBlasterFragment(BossModule module) : Components.SelfTargetedAOEs(module, AID.RightArmBlasterFragment, new AOEShapeRect(100, 3));
class RightArmBlasterBoss(BossModule module) : Components.SelfTargetedAOEs(module, AID.RightArmBlasterBoss, new AOEShapeRect(100, 3));
class LeftArmSlash(BossModule module) : Components.SelfTargetedAOEs(module, AID.LeftArmSlash, new AOEShapeCone(10, 90.Degrees())); // TODO: verify angle
class LeftArmWave(BossModule module) : Components.LocationTargetedAOEs(module, AID.LeftArmWaveAOE, 24);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 19)] // bnpcname=9958
public class Duel4Dabog(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, 710), new ArenaBoundsCircle(20));
