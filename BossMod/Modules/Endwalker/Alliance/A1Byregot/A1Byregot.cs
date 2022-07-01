namespace BossMod.Endwalker.Alliance.A1Byregot
{
    public class A1ByregotStates : StateMachineBuilder
    {
        public A1ByregotStates(BossModule module) : base(module)
        {
            // TODO: reconsider
            TrivialPhase()
                .ActivateOnEnter<ByregotStrike>()
                .ActivateOnEnter<Hammers>()
                .ActivateOnEnter<Reproduce>();
        }
    }

    public class A1Byregot : BossModule
    {
        public A1Byregot(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, 700), 25)) { }
    }
}
