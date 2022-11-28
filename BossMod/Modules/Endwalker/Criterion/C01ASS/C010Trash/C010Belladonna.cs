namespace BossMod.Endwalker.Criterion.C01ASS.C010Belladonna
{
    class AtropineSpore : Components.SelfTargetedAOEs
    {
        public AtropineSpore(ActionID aid) : base(aid, new AOEShapeDonut(10, 40)) { }
    }

    class FrondAffront : Components.CastGaze
    {
        public FrondAffront(ActionID aid) : base(aid) { }
    }

    class Deracinator : Components.SingleTargetCast
    {
        public Deracinator(ActionID aid) : base(aid) { }
    }

    namespace Normal
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

        class NAtropineSpore : AtropineSpore { public NAtropineSpore() : base(ActionID.MakeSpell(AID.AtropineSpore)) { } }
        class NFrondAffront : FrondAffront { public NFrondAffront() : base(ActionID.MakeSpell(AID.FrondAffront)) { } }
        class NDeracinator : Deracinator { public NDeracinator() : base(ActionID.MakeSpell(AID.Deracinator)) { } }

        class C010NBelladonnaStates : StateMachineBuilder
        {
            public C010NBelladonnaStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<NAtropineSpore>()
                    .ActivateOnEnter<NFrondAffront>()
                    .ActivateOnEnter<NDeracinator>();
            }
        }

        public class C010NBelladonna : SimpleBossModule
        {
            public C010NBelladonna(WorldState ws, Actor primary) : base(ws, primary) { }
        }
    }

    namespace Savage
    {
        public enum OID : uint
        {
            Boss = 0x3ADE, // R4.000, x1
        };

        public enum AID : uint
        {
            AutoAttack = 31320, // Boss->player, no cast, single-target
            AtropineSpore = 31096, // Boss->self, 4.0s cast, range 10-40 donut aoe
            FrondAffront = 31097, // Boss->self, 3.0s cast, gaze
            Deracinator = 31098, // Boss->player, 4.0s cast, single-target tankbuster
        };

        class SAtropineSpore : AtropineSpore { public SAtropineSpore() : base(ActionID.MakeSpell(AID.AtropineSpore)) { } }
        class SFrondAffront : FrondAffront { public SFrondAffront() : base(ActionID.MakeSpell(AID.FrondAffront)) { } }
        class SDeracinator : Deracinator { public SDeracinator() : base(ActionID.MakeSpell(AID.Deracinator)) { } }

        class C010SBelladonnaStates : StateMachineBuilder
        {
            public C010SBelladonnaStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<SAtropineSpore>()
                    .ActivateOnEnter<SFrondAffront>()
                    .ActivateOnEnter<SDeracinator>();
            }
        }

        public class C010SBelladonna : SimpleBossModule
        {
            public C010SBelladonna(WorldState ws, Actor primary) : base(ws, primary) { }
        }
    }
}
