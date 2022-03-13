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

            var s = CommonStates.Simple(ref InitialState, 0, "Fight");
            s.Update = (_) => PrimaryActor.IsDead ? s.Next : null;
            CommonStates.Simple(ref s.Next, 0, "???");
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
