namespace BossMod.Shadowbringers.Foray.DelubrumReginae;

public abstract class TrinitySeeker : BossModule
{
    public TrinitySeeker(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private TrinitySeeker(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(0f, 278f), 30f, 48)], [.. GenerateBarricades(), new Rectangle(new(0f, 248f), 7.5f, 0.75f),
        new Rectangle(new(default, 307.85f), 7.5f, 0.75f)], AdjustForHitboxInwards: true);
        return (arena.Center, arena);
    }

    public static ArenaBoundsCustom GetDefaultArena() => new([new Polygon(new(0f, 278f), 25.5f, 48)], GenerateBarricades(), AdjustForHitboxInwards: true);

    private static DonutSegmentV[] GenerateBarricades()
    {
        var barricades = new DonutSegmentV[4];
        var a22 = 22.5f.Degrees();
        var a45 = 45f.Degrees();
        var a90 = 90f.Degrees();
        var center = new WPos(0f, 278f);
        for (var i = 0; i < 4; ++i)
        {
            var ai = a90 * i;
            barricades[i] = new(center, 19.2f, 21.107f, a45 + ai, a22, 6); // each donut segment got 6 inner and 6 outer edges
        }
        return barricades;
    }
}

public abstract class Dahu : BossModule
{
    public Dahu(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Dahu(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(82f, 138f), 29.5f, 48)], [new Rectangle(new(82f, 108.233f), 20f, 1.25f), new Rectangle(new(82f, 167.738f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}

public abstract class QueensGuard : BossModule
{
    public QueensGuard(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private QueensGuard(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(244f, -162f), 29.5f, 48)], [new Rectangle(new(244f, -132.145f), 20f, 1.25f), new Rectangle(new(244f, -192.063f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }

    public static ArenaBoundsCustom GetDefaultArena() => new([new Polygon(new(244f, -162f), 25f, 48)]);
}

public abstract class Phantom(WorldState ws, Actor primary) : BossModule(ws, primary, new(202f, -374f), new ArenaBoundsRect(23.5f, 29.5f))
{
    public static AOEShapeCustom GetArenaChangeAOE()
    {
        return new(new(202f, -374f), [new Rectangle(new(202f, -374f), 24f, 30f)], [new Rectangle(new(202f, -370f), 24f, 24f)]);
    }
}

public abstract class TrinityAvowed(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272f, -82f), new ArenaBoundsSquare(29.5f))
{
    public static AOEShapeCustom GetArenaChangeAOE()
    {
        var center = new WPos(-272f, -82f);
        return new(center, [new Square(center, 29.5f)], [new Square(center, 25f)]);
    }
}

public abstract class Queen(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272f, -415f), new ArenaBoundsSquare(29.5f))
{
    public static ArenaBoundsCustom GetDefaultArena() => new([new Polygon(new(-272f, -415f), 25f, 48)]);
}
