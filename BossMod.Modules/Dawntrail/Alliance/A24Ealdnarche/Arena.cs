namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

class Duplicate1(BossModule module) : Components.GenericAOEs(module, AID.DuplicateFast)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public static WPos GetTile(int i)
    {
        var col = i % 3;
        var row = i / 3;
        return new WPos(784 + col * 16, -816 + row * 16);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.RemoveAll(c => c.Origin.AlmostEqual(caster.Position, 1));
            NumCasts++;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x2C and <= 0x34 && state == 0x00020001)
        {
            var tile = index - 0x2C;
            _aoes.Add(new(new AOEShapeCross(24, 8), GetTile(tile), default, WorldState.FutureTime(10.3f)));
        }
    }
}

class Duplicate2(BossModule module) : Components.GenericAOEs(module, AID.DuplicateSlow)
{
    private readonly List<AOEInstance> _aoes = [];
    private int _sourceTile = -1;
    private int _destinationTile = -1;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sourceTile >= 0 && _destinationTile >= 0 ? _aoes : [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x2C and <= 0x34 && state == 0x00020001)
        {
            var tile = index - 0x2C;
            if (tile == _sourceTile && _destinationTile >= 0)
                tile = _destinationTile;

            _aoes.Add(new(new AOEShapeCross(24, 8), Duplicate1.GetTile(tile), default, WorldState.FutureTime(12.6f)));
        }

        if (index is >= 0x35 and <= 0x3D)
        {
            // disappearing tile
            if (state == 0x00020001)
            {
                _sourceTile = index - 0x35;
                UpdateAOE();
            }

            if (state == 0x00080010)
            {
                _destinationTile = index - 0x35;
                UpdateAOE();
            }
        }
    }

    private void UpdateAOE()
    {
        if (_sourceTile >= 0 && _destinationTile >= 0)
        {
            var srcPos = Duplicate1.GetTile(_sourceTile);
            var destPos = Duplicate1.GetTile(_destinationTile);
            for (var i = 0; i < _aoes.Count; i++)
                if (_aoes[i].Origin == srcPos)
                    _aoes.Ref(i).Origin = destPos;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _aoes.RemoveAll(c => c.Origin.AlmostEqual(caster.Position, 1));
            _sourceTile = -1;
            _destinationTile = -1;
            NumCasts++;
        }
    }
}

class TileArena(BossModule module) : BossComponent(module)
{
    private BitMask _missing;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1A and <= 0x22)
        {
            if (state is 0x02000004 or 0x00040004)
                _missing.Set(index - 0x1A);

            if (state is 0x04000001)
                _missing.Clear(index - 0x1A);

            UpdateBounds();
        }
    }

    private void UpdateBounds()
    {
        var curBounds = CurveApprox.Rect(new(24, 0), new(0, 24));
        RelSimplifiedComplexPolygon poly = new(curBounds);
        foreach (var b in _missing.SetBits())
        {
            var dir = Duplicate1.GetTile(b) - Arena.Center;
            var tile = CurveApprox.Rect(new(8, 0), new(0, 8)).Select(d => d + dir);

            poly = Arena.Bounds.Clipper.Difference(new(poly), new(tile));
        }

        Arena.Bounds = new ArenaBoundsCustom(24, poly);
    }
}

class TileSwap(BossModule module) : Components.StandardAOEs(module, AID.TileSwap, new AOEShapeRect(16, 8));

class TileVanish(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x35 and <= 0x3D && state == 0x00020001)
            _aoe = new(new AOEShapeRect(8, 8, 8), Duplicate1.GetTile(index - 0x35), default, WorldState.FutureTime(7.9f), ArenaColor.Danger);

        if (index is >= 0x1A and <= 0x22 && state is 0x02000004 or 0x00040004)
            _aoe = null;
    }
}

class QuakeZone(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _cast;
    private BitMask _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_cast is { } c)
            yield return c;

        foreach (var b in _active.SetBits())
            yield return new AOEInstance(new AOEShapeRect(8, 8, 8), Duplicate1.GetTile(b));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Quake or AID.Freeze)
            _cast = new(new AOEShapeRect(16, 24), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Quake or AID.Freeze)
            NumCasts++;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1A and <= 0x22)
        {
            if (state is 0x00080010 or 0x00400080)
            {
                _cast = null;
                _active.Set(index - 0x1A);
            }

            if (state is 0x00200001 or 0x01000001)
                _active.Clear(index - 0x1A);
        }
    }
}
