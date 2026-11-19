namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class ArenaChanges : BossComponent
{
    public bool Shattered { get; private set; }
    private static readonly ArenaBoundsSquare _p1 = new(30, MapResolution: 0.3f);
    private bool _p1Active = true;
    private readonly bool[] _broken = new bool[9];
    private readonly bool[] _cracked = new bool[9];

    private static int TileIndex(WPos tile) => Math.Clamp((int)(tile.X / 20 + 1), 0, 2) + 3 * Math.Clamp((int)(tile.Z / 20 + 1), 0, 2);

    public bool Intact(WPos p) => Math.Abs(p.X) <= 30 && Math.Abs(p.Z) <= 30 && !_broken[TileIndex(Ex3Shinryu.TileCenter(p))];
    public bool Cracked(WPos p) => _cracked[TileIndex(Ex3Shinryu.TileCenter(p))] && Intact(p);

    public VBool Broken(WVec p)
    {
        Span<int> res = stackalloc int[VBool.Count];
        for (var i = 0; i < VBool.Count; ++i)
            res[i] = Intact(new(p.X[i], p.Z[i])) ? 0 : -1;
        return new(res);
    }

    public ArenaChanges(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (!_p1Active || (OID)actor.OID != OID.Platform)
            return;
        var center = Ex3Shinryu.TileCenter(actor.Position);
        if (state == 0x00040008)
            _cracked[TileIndex(center)] = true;
        if ((state >> 16) != 2 || (state & 0xFFFF) == 0x8000)
            return;
        _broken[TileIndex(center)] = true;
        Arena.Bounds = new ArenaBoundsCustom(30, _p1.Clipper.Simplify(BuildTiles(), Clipper2Lib.FillRule.NonZero), MapResolution: 0.3f);
    }

    // intact tiles only: two tiles that meet at a corner stay apart, players hop that gap and the AI can't
    private PolygonClipper.Operand BuildTiles()
    {
        var shape = new PolygonClipper.Operand();
        WDir TileOffset(int col, int row) => new(col * 20 - 20, row * 20 - 20);
        bool Whole(int col, int row) => !_broken[col + 3 * row];
        for (var row = 0; row < 3; ++row)
            for (var col = 0; col < 3; ++col)
                if (Whole(col, row))
                    shape.AddContour(CurveApprox.Rect(TileOffset(col, row), new WDir(0, 1), 10, 10));
        return shape;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarkMatter && _p1Active)
        {
            _p1Active = false;
            Arena.Bounds = _p1;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Touchdown:
                Arena.Center = Ex3Shinryu.P2Center;
                Arena.Bounds = Ex3Shinryu.P2Bounds;
                break;
            case AID.TailSpit:
                Arena.Center = Ex3Shinryu.ClimbCenter;
                Arena.Bounds = Ex3Shinryu.ClimbBounds;
                break;
            case AID.Shatter:
                Shattered = true;
                Arena.Center = default;
                Arena.Bounds = _p1;
                break;
        }
    }
}
