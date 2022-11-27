namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    class FiresteelFracture : Components.Cleave
    {
        public FiresteelFracture() : base(ActionID.MakeSpell(AID.FiresteelFracture), new AOEShapeCone(40, 30.Degrees())) { }
    }

    // TODO: show AOEs
    class BlazingBenifice : Components.CastCounter
    {
        public BlazingBenifice() : base(ActionID.MakeSpell(AID.BlazingBenifice)) { }
    }

    // TODO: show stuff
    class InfernWave : Components.CastCounter
    {
        public InfernWave() : base(ActionID.MakeSpell(AID.InfernWaveAOE)) { }
    }

    class PureFire : Components.LocationTargetedAOEs
    {
        public PureFire() : base(ActionID.MakeSpell(AID.PureFireAOE), 6) { }
    }

    public class C013Shadowcaster : BossModule
    {
        public C013Shadowcaster(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(289, -105), 15, 20)) { }
    }
}
