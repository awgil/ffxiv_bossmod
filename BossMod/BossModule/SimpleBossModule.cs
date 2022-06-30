namespace BossMod
{
    // base class for simple boss modules (hunts, fates, dungeons, etc.)
    // these (1) always center map around PC, (2) don't reset cooldowns on wipes, (3) provide single default simple state (assuming that they are purely reactive)
    public class SimpleBossModule : BossModule
    {
        public SimpleBossModule(WorldState ws, Actor primary)
            : base(ws, primary, new ArenaBoundsCircle(primary.Position, 30))
        {
        }

        // build a simple state machine, with first state activating provided component
        protected void BuildStateMachine<T>() where T : BossComponent, new()
        {
            var sb = new StateMachineBuilder(this);
            sb.TrivialPhase()
                .ActivateOnEnter<T>();
            StateMachine = sb.Build();
        }

        protected override void UpdateModule()
        {
            var pc = WorldState.Party.Player();
            if (pc != null && Bounds.Center != pc.Position)
                Arena.Bounds = new ArenaBoundsCircle(pc.Position, 30);
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
