namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Bloom3Emblazon(BossModule module) : Emblazon(module)
{
    private BitMask TowerTiles;
    private BitMask ForbiddenTiles;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TileExplosion)
        {
            TowerTiles.Set(Tiles.GetTile(spell.LocXZ));
            if (TowerTiles.NumSetBits() == 4)
            {
                ForbiddenTiles |= new BitMask(0xFF00);
                ForbiddenTiles |= _tiles.Mask & new BitMask(0xFF);
                foreach (var i in (int[])[9, 10, 13, 14])
                    if (_tiles.Mask[i])
                        ForbiddenTiles.Set(i - 8);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Baiters[slot] && ForbiddenTiles[Tiles.GetTile(actor)])
            hints.Add("Don't connect towers!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Baiters[slot])
            hints.AddForbiddenZone(Tiles.TilesShape(ForbiddenTiles), Activation);
    }
}

class TileExplosion(BossModule module) : Components.CastCounter(module, AID.TileExplosion)
{
    private readonly Tiles _tiles = module.FindComponent<Tiles>()!;

    private bool _active;

    private readonly List<Actor> _casters = [];
    private readonly List<BitMask> _towers = [];
    private BitMask _forbiddenSoakers;

    private DateTime Activation => _casters.Count > 0 ? Module.CastFinishAt(_casters[0].CastInfo) : default;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.Emblazon)
            _active = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Rose)
            _forbiddenSoakers.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_active)
            Tiles.ZoneTiles(Arena, _tiles.Mask, _forbiddenSoakers[pcSlot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in _casters)
            Arena.AddCircle(c.Position, 1, _forbiddenSoakers[pcSlot] ? ArenaColor.Danger : ArenaColor.Safe, 2);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_casters.Count == 0 || !_active)
            return;

        if (_forbiddenSoakers[slot])
        {
            if (_tiles.InActiveTile(actor))
                hints.Add("GTFO from tile!");
        }
        else if (_towers.FindIndex(t => t[Tiles.GetTile(actor)]) is var soaked && soaked >= 0)
        {
            var count = Raid.WithSlot().ExcludedFromMask(_forbiddenSoakers).Count(p => _towers[soaked][Tiles.GetTile(p.Item2)]);
            if (count > 1)
                hints.Add("Too many people in tower!");
            hints.Add("Soak a tower!", false);
        }
        else if (!_tiles.InActiveTile(actor))
            hints.Add("Soak a tower!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_casters.Count == 0 || !_active)
            return;

        var ts = _tiles.TilesShape();
        var soakers = _forbiddenSoakers;

        hints.AddForbiddenZone(ts.Inverted(!soakers[slot]), Activation);

        hints.AddPredictedDamage(Raid.WithSlot().Where(r => ts.CallOnce(r.Item2.Position) < 0).Mask(), Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (!_active)
            return;

        if (state == 0x00800040)
        {
            _towers.Clear();
            foreach (var c in _casters)
                _towers.Add(_tiles.GetConnectedTiles(Tiles.GetTile(c.CastInfo!.LocXZ)));
        }
    }
}
