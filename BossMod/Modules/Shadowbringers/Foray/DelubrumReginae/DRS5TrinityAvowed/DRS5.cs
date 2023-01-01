namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
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

    public class DRS5 : BossModule
    {
        public DRS5(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-272, -82), 25)) { }
    }
}
