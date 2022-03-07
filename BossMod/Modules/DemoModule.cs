namespace BossMod
{
    public class DemoModule : BossModule
    {
        private class DemoComponent : Component
        {
            public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
            {
                hints.Add("Hint", false);
                hints.Add("Risk");
            }

            public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
            {
                arena.ZoneCircle(arena.WorldCenter, 10, arena.ColorAOE);
            }

            public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
            {
                arena.Actor(arena.WorldCenter, 0, arena.ColorPC);
            }
        }

        public DemoModule(WorldState ws)
            : base(ws)
        {
            ActivateComponent(new DemoComponent());
        }
    }
}
