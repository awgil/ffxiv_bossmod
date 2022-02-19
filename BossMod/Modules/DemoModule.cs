using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class DemoModule : BossModule
    {
        private class DemoComponent : Component
        {
            public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
            {
                hints.Add("Hint", false);
                hints.Add("Risk");
            }

            public override void DrawArenaBackground(MiniArena arena)
            {
                arena.ZoneCircle(arena.WorldCenter, 10, arena.ColorAOE);
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                arena.Actor(arena.WorldCenter, 0, arena.ColorPC);
            }
        }

        public DemoModule(WorldState ws)
            : base(ws, 8)
        {
            ActivateComponent(new DemoComponent());
        }
    }
}
