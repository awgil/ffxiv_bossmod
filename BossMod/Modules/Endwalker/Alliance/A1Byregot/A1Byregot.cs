namespace BossMod.Endwalker.Alliance.A1Byregot
{
    public class A1Byregot : BossModule
    {
        public A1Byregot(WorldState ws, Actor primary)
            : base(ws, primary, new ArenaBoundsSquare(new(0, 700), 25))
        {
            var sb = new StateMachineBuilder(this);
            sb.TrivialPhase()
                .ActivateOnEnter<ByregotStrike>()
                .ActivateOnEnter<Hammers>()
                .ActivateOnEnter<Reproduce>();
            StateMachine = sb.Build();
            //StateMachine = new A1ByregotStates(this).Initial;
        }
    }
}
