namespace BossMod.Components;

// generic 'exaflare' component - these mechanics are a bunch of moving aoes, with different lines either staggered or moving with different speed
public class Exaflare(BossModule module, AOEShape shape, ActionID aid = default) : GenericAOEs(module, aid, "GTFO from exaflare!")
{
    public class Line
    {
        public WPos Next;
        public WDir Advance;
        public Angle Rotation;
        public DateTime NextExplosion;
        public float TimeToMove;
        public int ExplosionsLeft;
        public int MaxShownExplosions;
    }

    public AOEShape Shape { get; init; } = shape;
    public uint ImminentColor = ArenaColor.Danger;
    public uint FutureColor = ArenaColor.AOE;
    protected List<Line> Lines = [];

    public bool Active => Lines.Count > 0;

    public Exaflare(BossModule module, float radius, ActionID aid = new()) : this(module, new AOEShapeCircle(radius), aid) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, t, r) in FutureAOEs())
            yield return new(Shape, c, r, t, FutureColor);
        foreach (var (c, t, r) in ImminentAOEs())
            yield return new(Shape, c, r, t, ImminentColor);
    }

    protected IEnumerable<(WPos, DateTime, Angle)> ImminentAOEs() => Lines.Where(l => l.ExplosionsLeft > 0).Select(l => (l.Next, l.NextExplosion, l.Rotation));

    protected IEnumerable<(WPos, DateTime, Angle)> FutureAOEs()
    {
        foreach (var l in Lines)
        {
            int num = Math.Min(l.ExplosionsLeft, l.MaxShownExplosions);
            var pos = l.Next;
            var time = l.NextExplosion > WorldState.CurrentTime ? l.NextExplosion : WorldState.CurrentTime;
            for (int i = 1; i < num; ++i)
            {
                pos += l.Advance;
                time = time.AddSeconds(l.TimeToMove);
                yield return (pos, time, l.Rotation);
            }
        }
    }

    protected void AdvanceLine(Line l, WPos pos)
    {
        l.Next = pos + l.Advance;
        l.NextExplosion = WorldState.FutureTime(l.TimeToMove);
        --l.ExplosionsLeft;
    }
}
