namespace BossMod.Endwalker.Alliance.A1Byregot
{
    public class A1Byregot : BossModule
    {
        public A1Byregot(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.WorldCenter = new(0, 20, 700);
            Arena.WorldHalfSize = 25;

            var sb = new StateMachineBuilder(this);
            var s = sb.Simple(0, 600, "Fight")
                .ActivateOnEnter<ByregotStrike>()
                .ActivateOnEnter<Hammers>()
                .ActivateOnEnter<Reproduce>()
                .DeactivateOnExit<ByregotStrike>()
                .DeactivateOnExit<Hammers>()
                .DeactivateOnExit<Reproduce>();
            s.Raw.Update = _ => PrimaryActor.IsDead ? s.Raw.Next : null;
            sb.Simple(1, 0, "???");
            InitStates(sb.Initial);
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
