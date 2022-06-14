namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    public class Ex1Ultima : BossModule
    {
        public Ex1Ultima(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsCircle(new(0, 0), 20))
        {
            // TODO: reconsider...
            var states = new StateMachineBuilder(this);
            states.TrivialPhase(600);
            StateMachine = states.Build();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
