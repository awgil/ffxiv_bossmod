namespace BossMod.Endwalker.Alliance.A11Byregot
{
    public class A11ByregotStates : StateMachineBuilder
    {
        public A11ByregotStates(BossModule module) : base(module)
        {
            // TODO: reconsider
            TrivialPhase()
                .ActivateOnEnter<ByregotStrike>()
                .ActivateOnEnter<Hammers>()
                .ActivateOnEnter<Reproduce>();
        }
    }

    public class A11Byregot : BossModule
    {
        public A11Byregot(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 700), 25)) { }
    }
}
