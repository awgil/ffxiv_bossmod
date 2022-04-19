//namespace BossMod.Endwalker.Alliance.A4Naldthal
//{
//    public class A4Naldthal : BossModule
//    {
//        public A4Naldthal(BossModuleManager manager, Actor primary)
//            : base(manager, primary, true)
//        {
//            //Arena.WorldCenter = new(0, 20, 700);
//            //Arena.WorldHalfSize = 25;

//            var sb = new StateMachineBuilder(this);
//            var s = sb.Simple(0, 600, "Fight");
//            //.ActivateOnEnter<ByregotStrike>()
//            //.ActivateOnEnter<Hammers>()
//            //.ActivateOnEnter<Reproduce>()
//            //.DeactivateOnExit<ByregotStrike>()
//            //.DeactivateOnExit<Hammers>()
//            //.DeactivateOnExit<Reproduce>();
//            s.Raw.Update = _ => PrimaryActor.IsDead ? s.Raw.Next : null;
//            sb.Simple(1, 0, "???");
//            InitStates(sb.Initial);
//            //InitStates(new A4NaldthalStates(this).Initial);
//        }

//        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
//        {
//            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
//            Arena.Actor(pc, Arena.ColorPC);
//        }
//    }
//}
