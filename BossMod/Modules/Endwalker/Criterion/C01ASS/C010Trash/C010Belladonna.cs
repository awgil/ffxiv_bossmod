namespace BossMod.Endwalker.Criterion.C01ASS.C010Belladonna
{
    public enum OID : uint
    {
        Boss = 0x3AD5, // R4.000, x1
    };

    public enum AID : uint
    {
        AutoAttack = 31320, // Boss->player, no cast, single-target
        AtropineSpore = 31072, // Boss->self, 4.0s cast, range 10-40 donut aoe
        FrondAffront = 31073, // Boss->self, 3.0s cast, gaze
        Deracinator = 31074, // Boss->player, 4.0s cast, single-target tankbuster
    };

    class AtropineSpore : Components.SelfTargetedAOEs
    {
        public AtropineSpore() : base(ActionID.MakeSpell(AID.AtropineSpore), new AOEShapeDonut(10, 40)) { }
    }

    class FrondAffront : Components.CastGaze
    {
        public FrondAffront() : base(ActionID.MakeSpell(AID.FrondAffront)) { }
    }

    class Deracinator : Components.SingleTargetCast
    {
        public Deracinator() : base(ActionID.MakeSpell(AID.Deracinator)) { }
    }

    class C010BelladonnaStates : StateMachineBuilder
    {
        public C010BelladonnaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<AtropineSpore>()
                .ActivateOnEnter<FrondAffront>()
                .ActivateOnEnter<Deracinator>();
        }
    }

    public class C010Belladonna : SimpleBossModule
    {
        public C010Belladonna(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
