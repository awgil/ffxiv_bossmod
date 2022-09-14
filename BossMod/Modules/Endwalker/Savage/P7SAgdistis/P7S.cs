namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class HemitheosHoly : Components.StackWithCastTargets
    {
        public HemitheosHoly() : base(ActionID.MakeSpell(AID.HemitheosHolyAOE), 6, 4) { }
    }

    class BoughOfAttisBack : Components.SelfTargetedAOEs
    {
        public BoughOfAttisBack() : base(ActionID.MakeSpell(AID.BoughOfAttisBackAOE), new AOEShapeCircle(25)) { }
    }

    class BoughOfAttisFront : Components.SelfTargetedAOEs
    {
        public BoughOfAttisFront() : base(ActionID.MakeSpell(AID.BoughOfAttisFrontAOE), new AOEShapeCircle(19)) { }
    }

    class BoughOfAttisSide : Components.SelfTargetedAOEs
    {
        public BoughOfAttisSide() : base(ActionID.MakeSpell(AID.BoughOfAttisSideAOE), new AOEShapeRect(50, 12.5f)) { }
    }

    class HemitheosAeroKnockback1 : Components.KnockbackFromCaster
    {
        public HemitheosAeroKnockback1() : base(ActionID.MakeSpell(AID.HemitheosAeroKnockback1), 16) { } // TODO: verify distance...
    }

    class HemitheosAeroKnockback2 : Components.KnockbackFromCaster
    {
        public HemitheosAeroKnockback2() : base(ActionID.MakeSpell(AID.HemitheosAeroKnockback2), 16) { }
    }

    class HemitheosHolySpread : Components.SpreadFromCastTargets
    {
        public HemitheosHolySpread() : base(ActionID.MakeSpell(AID.HemitheosHolySpread), 6) { }
    }

    class HemitheosTornado : Components.SelfTargetedAOEs
    {
        public HemitheosTornado() : base(ActionID.MakeSpell(AID.HemitheosTornado), new AOEShapeCircle(25)) { }
    }

    class HemitheosGlareMine : Components.SelfTargetedAOEs
    {
        public HemitheosGlareMine() : base(ActionID.MakeSpell(AID.HemitheosGlareMine), new AOEShapeDonut(5, 30)) { } // TODO: verify inner radius
    }

    public class P7S : BossModule
    {
        public P7S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 27)) { }
    }
}
