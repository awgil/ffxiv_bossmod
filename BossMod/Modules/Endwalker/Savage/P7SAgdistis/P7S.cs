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

    class HemitheosAeroKnockback : CommonComponents.KnockbackFromCaster
    {
        public HemitheosAeroKnockback() : base(ActionID.MakeSpell(AID.HemitheosAeroKnockback), 10) { } // TODO: verify distance...
    }

    class HemitheosHolySpread : Components.SpreadFromCastTargets
    {
        public HemitheosHolySpread() : base(ActionID.MakeSpell(AID.HemitheosHolySpread), 6) { }
    }

    // TODO: these should be a part of forbidden fruit component
    class StaticMoon : Components.SelfTargetedAOEs
    {
        public StaticMoon() : base(ActionID.MakeSpell(AID.StaticMoon), new AOEShapeCircle(10)) { }
    }

    class StymphalianStrike : Components.SelfTargetedAOEs
    {
        public StymphalianStrike() : base(ActionID.MakeSpell(AID.StymphalianStrike), new AOEShapeRect(60, 4)) { }
    }

    public class P7S : BossModule
    {
        public P7S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 27)) { }

        //protected override void DrawEnemies(int pcSlot, Actor pc)
        //{
        //    base.DrawEnemies(pcSlot, pc);
        //    foreach (var e in Enemies(OID.ImmatureIo))
        //        Arena.Actor(e, 0xffffff00);
        //    foreach (var e in Enemies(OID.ImmatureStymphalide))
        //        Arena.Actor(e, 0xff00ffff);
        //    foreach (var e in Enemies(OID.ImmatureMinotaur))
        //        Arena.Actor(e, 0xffff00ff);
        //}
    }
}
