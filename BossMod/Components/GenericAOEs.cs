using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component that shows arbitrary shapes representing avoidable aoes
    public abstract class GenericAOEs : CastCounter
    {
        public GenericAOEs(ActionID aid) : base(aid) { }

        public abstract IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module);

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveAOEs(module).Any(c => c.shape.Check(actor.Position, c.origin, c.rotation)))
                hints.Add("GTFO from aoe!");
        }

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            foreach (var c in ActiveAOEs(module))
                zone.ForbidZone(c.shape, c.origin, c.rotation, c.time);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var c in ActiveAOEs(module))
                c.shape.Draw(arena, c.origin, c.rotation);
        }
    }
}
