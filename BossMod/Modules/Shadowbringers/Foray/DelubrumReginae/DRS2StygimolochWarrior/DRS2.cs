namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class ViciousSwipe : Components.SelfTargetedAOEs
{
    public ViciousSwipe() : base(ActionID.MakeSpell(AID.ViciousSwipe), new AOEShapeCircle(15)) { }
}

class CrazedRampage : Components.KnockbackFromCastTarget
{
    public CrazedRampage() : base(ActionID.MakeSpell(AID.CrazedRampage), 13) { }
}

class Coerce : Components.StatusDrivenForcedMarch
{
    public Coerce() : base(4, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9754)]
public class DRS2 : BossModule
{
    public DRS2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-160, 78), 17.5f)) { }
}
