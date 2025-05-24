namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Bloom3Emblazon(BossModule module) : Emblazon(module)
{
    private BitMask TowerTiles;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TileExplosion)
            TowerTiles.Set(Tiles.GetTile(spell.LocXZ));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Baiters[slot] && DangerTile(Tiles.GetTile(actor)))
            hints.Add("Don't connect towers!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Baiters[slot])
            hints.AddForbiddenZone(p => DangerTile(Tiles.GetTile(p)), Activation);
    }

    private bool DangerTile(int t)
    {
        var tileNext = t is 7 or 15 ? t - 7 : t + 1;
        var tilePrev = t is 0 or 8 ? t + 7 : t - 1;

        return t >= 8 // outer ring always unsafe
            || _tiles[t] // active tiles always unsafe
            || _tiles[t + 8] && _tiles[tileNext] != _tiles[tilePrev]; // inner "side tiles" always unsafe
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

        var ts = _tiles.TileShape();

        hints.AddForbiddenZone(p => _forbiddenSoakers[slot] ? ts(p) : !ts(p), Activation);

        hints.AddPredictedDamage(Raid.WithSlot().Where(r => ts(r.Item2.Position)).Mask(), Activation);
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

    public override void OnEventEnvControl(byte index, uint state)
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
