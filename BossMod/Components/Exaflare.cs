using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic 'exaflare' component - these mechanics are a bunch of moving aoes, with different lines either staggered or moving with different speed
    public class Exaflare : GenericAOEs
    {
        public class Line
        {
            public WPos Next;
            public WDir Advance;
            public DateTime NextExplosion;
            public float TimeToMove;
            public int ExplosionsLeft;
            public int MaxShownExplosions;
        }

        public AOEShape Shape { get; private init; }
        public uint ImminentColor = ArenaColor.Danger;
        protected List<Line> Lines = new();

        public bool Active => Lines.Count > 0;

        public Exaflare(AOEShape shape, ActionID watchedAction = new()) : base(watchedAction, "GTFO from exaflare!")
        {
            Shape = shape;
        }
        public Exaflare(float radius, ActionID watchedAction = new()) : this(new AOEShapeCircle(radius), watchedAction) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var (c, t) in FutureAOEs(module.WorldState.CurrentTime))
                yield return new(Shape, c, activation: t);
            foreach (var (c, t) in ImminentAOEs())
                yield return new(Shape, c, activation: t, color: ImminentColor);
        }

        protected IEnumerable<(WPos, DateTime)> ImminentAOEs() => Lines.Where(l => l.ExplosionsLeft > 0).Select(l => (l.Next, l.NextExplosion));

        protected IEnumerable<(WPos, DateTime)> FutureAOEs(DateTime currentTime)
        {
            foreach (var l in Lines)
            {
                int num = Math.Min(l.ExplosionsLeft, l.MaxShownExplosions);
                var pos = l.Next;
                var time = l.NextExplosion > currentTime ? l.NextExplosion : currentTime;
                for (int i = 1; i < num; ++i)
                {
                    pos += l.Advance;
                    time = time.AddSeconds(l.TimeToMove);
                    yield return (pos, time);
                }
            }
        }

        protected void AdvanceLine(BossModule module, Line l, WPos pos)
        {
            l.Next = pos + l.Advance;
            l.NextExplosion = module.WorldState.CurrentTime.AddSeconds(l.TimeToMove);
            --l.ExplosionsLeft;
        }
    }
}
