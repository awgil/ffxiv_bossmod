namespace BossMod.Endwalker.Alliance.A2Rhalgr
{
    public class A2Rhalgr : BossModule
    {
        public A2Rhalgr(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.WorldCenter = new(-15, 20, 275); // arena has a really complex shape...
            Arena.WorldHalfSize = 30;

            var sb = new StateMachineBuilder(this);
            var s = sb.Simple(0, 600, "Fight");
            //.ActivateOnEnter<ByregotStrike>()
            //.ActivateOnEnter<Hammers>()
            //.ActivateOnEnter<Reproduce>()
            //.DeactivateOnExit<ByregotStrike>()
            //.DeactivateOnExit<Hammers>()
            //.DeactivateOnExit<Reproduce>();
            s.Raw.Update = _ => PrimaryActor.IsDead ? s.Raw.Next : null;
            sb.Simple(1, 0, "???");
            InitStates(sb.Initial);
            //InitStates(new A2RhalgrStates(this).Initial);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
