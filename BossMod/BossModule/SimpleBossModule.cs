namespace BossMod
{
    // base class for simple boss modules (hunts, fates, dungeons, etc.)
    // these (1) always center map around PC, (2) don't reset cooldowns on wipes, (3) provide single default simple state (assuming that they are purely reactive)
    public class SimpleBossModule : BossModule
    {
        public SimpleBossModule(BossModuleManager manager, Actor primary)
            : base(manager, primary, false)
        {
            Arena.IsCircle = true;
            Arena.WorldHalfSize = 30;
        }

        // build a simple state machine, with first state activating provided component
        protected void BuildStateMachine<T>() where T : Component, new()
        {
            var sb = new StateMachineBuilder(this);

            var s = sb.Simple(0, 0, "Fight")
                .ActivateOnEnter<T>()
                .DeactivateOnExit<T>();
            s.Raw.Update = _ => PrimaryActor.IsDead ? s.Raw.Next : null;

            sb.Simple(1, 0, "???");
        }

        protected override void UpdateModule()
        {
            var pc = WorldState.Party.Player();
            if (pc != null)
                Arena.WorldCenter = pc.Position;
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
