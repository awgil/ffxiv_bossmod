namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7Queen
{
    class NorthswainsGlow : Components.SelfTargetedAOEs
    {
        public NorthswainsGlow() : base(ActionID.MakeSpell(AID.NorthswainsGlowAOE), new AOEShapeCircle(20)) { }
    }

    class CleansingSlashSecond : Components.CastCounter
    {
        public CleansingSlashSecond() : base(ActionID.MakeSpell(AID.CleansingSlashSecond)) { }
    }

    class GodsSaveTheQueen : Components.CastCounter
    {
        public GodsSaveTheQueen() : base(ActionID.MakeSpell(AID.GodsSaveTheQueenAOE)) { }
    }

    // note: apparently there is no 'front unseen' status
    class QueensShot : Components.CastWeakpoint
    {
        public QueensShot() : base(ActionID.MakeSpell(AID.QueensShot), new AOEShapeCircle(60), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen) { }
    }

    class TurretsTourUnseen : Components.CastWeakpoint
    {
        public TurretsTourUnseen() : base(ActionID.MakeSpell(AID.TurretsTourUnseen), new AOEShapeRect(50, 2.5f), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen) { }
    }

    class OptimalOffensive : Components.ChargeAOEs
    {
        public OptimalOffensive() : base(ActionID.MakeSpell(AID.OptimalOffensive), 2.5f) { }
    }

    // note: there are two casters (as usual in bozja content for raidwides)
    // TODO: not sure whether it ignores immunes, I assume so...
    class OptimalOffensiveKnockback : Components.KnockbackFromCastTarget
    {
        public OptimalOffensiveKnockback() : base(ActionID.MakeSpell(AID.OptimalOffensiveKnockback), 10, true, 1) { }
    }

    class PawnOff : Components.SelfTargetedAOEs
    {
        public PawnOff() : base(ActionID.MakeSpell(AID.PawnOffReal), new AOEShapeCircle(20)) { }
    }

    class OptimalPlayShield : Components.SelfTargetedAOEs
    {
        public OptimalPlayShield() : base(ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5, 60)) { }
    }

    class OptimalPlayCone : Components.SelfTargetedAOEs
    {
        public OptimalPlayCone() : base(ActionID.MakeSpell(AID.OptimalPlayCone), new AOEShapeCone(60, 135.Degrees())) { }
    }

    public class DRS7 : BossModule
    {
        public DRS7(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-272, -415), 25)) { } // note: initially arena is square, but it quickly changes to circle
    }
}
