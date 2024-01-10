namespace BossMod.Shadowbringers.HuntA.Maliktender
{
    public enum OID : uint
    {
        Boss = 0x2874,
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        Sabotendance = 18019, // Boss->self, 3.5s cast, range 8 circle
        TwentyKNeedles = 18022, // Boss->self, 3.5s cast, range 20 width 8 rect
        NineNineNineKNeedles = 18024, // Boss->self, 3.0s cast, range 20 width 8 rect
        Haste = 18020, // Boss->self, 3.0s cast, buff to self, boss will use 990k needles instead of 20k needles
    }

    class Sabotendance : Components.SelfTargetedAOEs
    {
        public Sabotendance() : base(ActionID.MakeSpell(AID.Sabotendance), new AOEShapeCircle(8)) { }
    }

    class TwentyKNeedles : Components.SelfTargetedAOEs
    {
        public TwentyKNeedles() : base(ActionID.MakeSpell(AID.TwentyKNeedles), new AOEShapeRect(20,4)) { }
    }
    class Haste : Components.CastHint
    {
        public Haste() : base(ActionID.MakeSpell(AID.Haste), "Needle attack will instant kill from now on!") { }
    }
    class NineNineNineKNeedles : Components.SelfTargetedAOEs
    {
        public NineNineNineKNeedles() : base(ActionID.MakeSpell(AID.NineNineNineKNeedles), new AOEShapeRect(20,4)) { }
    }

    class MaliktenderStates : StateMachineBuilder
    {
        public MaliktenderStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Sabotendance>()
                .ActivateOnEnter<TwentyKNeedles>()
                .ActivateOnEnter<NineNineNineKNeedles>();
        }
    }

    public class Maliktender : SimpleBossModule
    {
        public Maliktender(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
