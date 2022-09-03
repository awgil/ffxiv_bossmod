namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    // TODO: improve component?
    class ToxicCrunch : Components.CastCounter
    {
        public ToxicCrunch() : base(ActionID.MakeSpell(AID.ToxicCrunchAOE)) { }
    }

    class DoubleRush : Components.ChargeAOEs
    {
        public DoubleRush() : base(ActionID.MakeSpell(AID.DoubleRush), 50) { }
    }

    // TODO: show knockback?
    class DoubleRushReturn : Components.CastCounter
    {
        public DoubleRushReturn() : base(ActionID.MakeSpell(AID.DoubleRushReturn)) { }
    }

    class SonicShatter : Components.CastCounter
    {
        public SonicShatter() : base(ActionID.MakeSpell(AID.SonicShatterRest)) { }
    }

    class DevourBait : Components.CastCounter
    {
        public DevourBait() : base(ActionID.MakeSpell(AID.DevourBait)) { }
    }

    public class P5S : BossModule
    {
        public P5S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 15)) { }
    }
}
