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
                    movementHints.Add(actor.Position, actor.Position + new WDir(10, 10), module.Arena.ColorDanger);
            }

            public override void AddGlobalHints(BossModule module, GlobalHints hints)
            {
                hints.Add("Global");
            }

            public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                arena.ZoneCircle(module.Bounds.Center, 10, arena.ColorAOE);
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                arena.Actor(module.Bounds.Center, 0.Degrees(), arena.ColorPC);
            }
        }

        public DemoModule(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            var smb = new StateMachineBuilder(this);
            smb.TrivialPhase();
            InitStates(smb.Build());

            ActivateComponent<DemoComponent>();
        }
    }
}
