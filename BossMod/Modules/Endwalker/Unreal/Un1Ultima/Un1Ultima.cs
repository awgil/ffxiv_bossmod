namespace BossMod.Endwalker.Unreal.Un1Ultima
{
    // TODO: consider how phase changes could be detected and create different states for them?..
    class Phases : BossModule.Component
    {
        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            var hint = ((float)module.PrimaryActor.HPCur / module.PrimaryActor.HPMax) switch
            {
                > 0.8f => "Garuda -> 80% Titan",
                > 0.65f => "Titan -> 65% Ifrit",
                > 0.5f => "Ifrit -> 50% Boom1",
                > 0.4f => "Boom1 -> 40% Bits1",
                > 0.3f => "Bits1 -> 30% Boom2",
                > 0.25f => "Boom2 -> 25% Bits2",
                > 0.15f => "Bits2 -> 15% Boom3",
                _ => "Boom3 -> Enrage"
            };
            hints.Add(hint);
        }
    }

    public class Un1Ultima : BossModule
    {
        public Un1Ultima(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsCircle(new(0, 0), 20))
        {
            // TODO: reconsider...
            var states = new StateMachineBuilder(this);
            states.TrivialPhase(600)
                .ActivateOnEnter<Phases>()
                .ActivateOnEnter<Mechanics>()
                .ActivateOnEnter<Garuda>()
                .ActivateOnEnter<TitanIfrit>();
            InitStates(states.Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
