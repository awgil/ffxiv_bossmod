namespace BossMod.Endwalker.Alliance.A1Byregot
{
    public class A1Byregot : BossModule
    {
        public A1Byregot(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            Arena.WorldCenter = new(0, 700);
            Arena.WorldHalfSize = 25;

            var sb = new StateMachineBuilder(this);
            sb.TrivialPhase()
                .ActivateOnEnter<ByregotStrike>()
                .ActivateOnEnter<Hammers>()
                .ActivateOnEnter<Reproduce>();
            InitStates(sb.Build());
            //InitStates(new A1ByregotStates(this).Initial);
        }

        protected override void DrawArenaForegroundPre(int pcSlot, Actor pc)
        {
            foreach (var p in WorldState.Actors)
                if (p.Type == ActorType.Player && !p.IsDead)
                    Arena.Actor(p, Arena.ColorPlayerGeneric);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
