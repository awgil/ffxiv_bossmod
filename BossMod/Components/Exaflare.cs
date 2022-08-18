using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'exaflare' component - these mechanics are a bunch of moving aoes, with different lines either staggered or moving with different speed
    public class Exaflare : BossComponent
    {
        public class Line
        {
            public WPos Next;
            public WDir Advance;
            public DateTime LastExplosion;
            public float TimeToMove;
            public int ExplosionsLeft;
            public int MaxShownExplosions;
        }

        public AOEShapeCircle Shape { get; private init; }
        protected List<Line> Lines = new();

        public Exaflare(float radius)
        {
            Shape = new(radius);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Lines.Any(l => ImminentAOEs(module, l).Any(c => Shape.Check(actor.Position, c.Item1))))
            {
                hints.Add("GTFO from aoe!");
            }
        }

        public override void UpdateSafeZone(BossModule module, int slot, Actor actor, SafeZone zone)
        {
            foreach (var l in Lines)
            {
                foreach (var (c, t) in ImminentAOEs(module, l))
                {
                    zone.ForbidZone(Shape, c, new(), t);
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var l in Lines)
            {
                foreach (var (c, _) in ImminentAOEs(module, l))
                {
                    Shape.Draw(arena, c);
                }
            }
        }

        protected IEnumerable<(WPos, DateTime)> ImminentAOEs(BossModule module, Line l)
        {
            int num = Math.Min(l.ExplosionsLeft, l.MaxShownExplosions);
            var pos = l.Next;
            var time = l.LastExplosion.AddSeconds(l.TimeToMove);
            if (time < module.WorldState.CurrentTime)
                time = module.WorldState.CurrentTime;
            for (int i = 0; i < num; ++i)
            {
                yield return (pos, time);
                pos += l.Advance;
                time = time.AddSeconds(l.TimeToMove);
            }
        }

        protected void AdvanceLine(BossModule module, Line l, WPos pos)
        {
            l.Next = pos + l.Advance;
            l.LastExplosion = module.WorldState.CurrentTime;
            --l.ExplosionsLeft;
        }
    }
}
