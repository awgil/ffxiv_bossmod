using System.Numerics;

namespace BossMod
{
    public class DemoModule : BossModule
    {
        private class DemoComponent : Component
        {
            public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
            {
                hints.Add("Hint", false);
                hints.Add("Risk");
                if (movementHints != null)
                    movementHints.Add(actor.Position, actor.Position + new Vector3(10, 0, 10), module.Arena.ColorDanger);
            }

            public override void AddGlobalHints(BossModule module, GlobalHints hints)
            {
                hints.Add("Global");
            }

            public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                arena.ZoneCircle(arena.WorldCenter, 10, arena.ColorAOE);
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                arena.Actor(arena.WorldCenter, 0, arena.ColorPC);
            }
        }

        public DemoModule(BossModuleManager manager, Actor primary)
            : base(manager, primary, false)
        {
            InitStates(null);
            ActivateComponent<DemoComponent>();
        }
    }
}
