namespace BossMod.Endwalker.Alliance.A1Byregot
{
    public class A1Byregot : BossModule
    {
        public A1Byregot(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsSquare(new(0, 700), 25))
        {
            var sb = new StateMachineBuilder(this);
            sb.TrivialPhase()
                .ActivateOnEnter<ByregotStrike>()
                .ActivateOnEnter<Hammers>()
                .ActivateOnEnter<Reproduce>();
            StateMachine = sb.Build();
            //StateMachine = new A1ByregotStates(this).Initial;
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
