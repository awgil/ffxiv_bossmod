namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class WrathOfBozja : Components.CastSharedTankbuster
{
    public WrathOfBozja() : base(ActionID.MakeSpell(AID.WrathOfBozja), new AOEShapeCone(60, 45.Degrees())) { } // TODO: verify angle
}

class WrathOfBozjaBow : Components.CastSharedTankbuster
{
    public WrathOfBozjaBow() : base(ActionID.MakeSpell(AID.WrathOfBozjaBow), new AOEShapeCone(60, 45.Degrees())) { } // TODO: verify angle
}

// note: it is combined with different AOEs (bow1, bow2, staff1)
class QuickMarch : Components.StatusDrivenForcedMarch
{
    public QuickMarch() : base(3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }
}

class ElementalImpact1 : Components.SelfTargetedAOEs
{
    public ElementalImpact1() : base(ActionID.MakeSpell(AID.ElementalImpact1), new AOEShapeCircle(20)) { }
}

class ElementalImpact2 : Components.SelfTargetedAOEs
{
    public ElementalImpact2() : base(ActionID.MakeSpell(AID.ElementalImpact2), new AOEShapeCircle(20)) { }
}

class GleamingArrow : Components.SelfTargetedAOEs
{
    public GleamingArrow() : base(ActionID.MakeSpell(AID.GleamingArrow), new AOEShapeRect(60, 5)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9853)]
public class DRS6 : BossModule
{
    public DRS6(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-272, -82), 25)) { }
}
