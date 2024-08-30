namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class WrathOfBozja(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.WrathOfBozja), new AOEShapeCone(60, 45.Degrees())); // TODO: verify angle
class WrathOfBozjaBow(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.WrathOfBozjaBow), new AOEShapeCone(60, 45.Degrees())); // TODO: verify angle

// note: it is combined with different AOEs (bow1, bow2, staff1)
class QuickMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);

class ElementalImpact1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElementalImpact1), new AOEShapeCircle(20));
class ElementalImpact2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElementalImpact2), new AOEShapeCircle(20));
class GleamingArrow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GleamingArrow), new AOEShapeRect(60, 5));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9853, PlanLevel = 80)]
public class DRS6(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272, -82), new ArenaBoundsSquare(25));
