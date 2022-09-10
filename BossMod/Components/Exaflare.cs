using System;
using System.Collections.Generic;

namespace BossMod.Components
{
    // generic 'exaflare' component - these mechanics are a bunch of moving aoes, with different lines either staggered or moving with different speed
    public class Exaflare : GenericAOEs
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

        public Exaflare(float radius, ActionID watchedAction = new()) : base(watchedAction, "GTFO from exaflare!")
        {
            Shape = new(radius);
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var l in Lines)
                foreach (var (c, t) in ImminentAOEs(module, l))
                    yield return (Shape, c, new(), t);
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
