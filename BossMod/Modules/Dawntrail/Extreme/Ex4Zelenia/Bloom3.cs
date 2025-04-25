namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class Bloom3Emblazon(BossModule module) : Components.CastCounter(module, AID.Emblazon)
{
    public BitMask Baiters;
    private BitMask TowerTiles;
    private DateTime Activation;
    private readonly Tiles Tiles = module.FindComponent<Tiles>()!;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Rose)
        {
            Baiters.Set(Raid.FindSlot(actor.InstanceID));
            Activation = WorldState.FutureTime(6.7f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TileExplosion)
            TowerTiles.Set(Tiles.GetTile(spell.LocXZ));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.Emblazon)
        {
            Baiters.Clear(Raid.FindSlot(spell.MainTargetID));
            if (!Baiters.Any())
                Activation = default;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (slot, player) in Raid.WithSlot().IncludedInMask(Baiters))
        {
            var tile = Tiles.GetTile(player);
            if (slot == pcSlot)
                Tiles.DrawTile(Arena, tile, ArenaColor.Danger);
            else if (Baiters[pcSlot])
                Tiles.ZoneTile(Arena, tile, ArenaColor.AOE);
        }

        if (Baiters[pcSlot])
            Tiles.ZoneTiles(Arena, Tiles.Mask, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // rose icons/spreads are harmless to non-baiting players
        if (!Baiters[slot])
            return;

        var tile = Tiles.GetTile(actor);

        if (Tiles[tile])
            hints.Add("GTFO from tile!");
        else if (DangerTile(tile))
            hints.Add("Don't connect towers!");

        if (OtherBaits(actor).Any(p => Tiles.GetTile(p) == tile))
            hints.Add("GTFO from spreads!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Baiters[slot])
            return;

        var tile = Tiles.GetTile(actor);

        hints.AddForbiddenZone(p => DangerTile(Tiles.GetTile(p)), Activation);

        foreach (var b in OtherBaits(actor))
        {
            var t = Tiles.GetTile(b);
            hints.AddForbiddenZone(p => Tiles.GetTile(p) == t, Activation);
        }
    }

    private bool DangerTile(int t)
    {
        var tileNext = t is 7 or 15 ? t - 7 : t + 1;
        var tilePrev = t is 0 or 8 ? t + 7 : t - 1;

        return t >= 8 // outer ring always unsafe
            || Tiles[t] // active tiles always unsafe
            || Tiles[t + 8] && Tiles[tileNext] != Tiles[tilePrev]; // inner "side tiles" always unsafe
    }

    private IEnumerable<Actor> OtherBaits(Actor actor) => Raid.WithSlot().IncludedInMask(Baiters).Exclude(actor).Select(p => p.Item2);
}

class Bloom3Explosion(BossModule module) : Components.CastCounter(module, AID.TileExplosion)
{
    private readonly Tiles Tiles = module.FindComponent<Tiles>()!;

    private bool Active;

    private readonly List<Actor> Casters = [];
    private readonly List<BitMask> Towers = [];
    private BitMask ForbiddenSoakers;

    private DateTime Activation => Casters.Count > 0 ? Module.CastFinishAt(Casters[0].CastInfo) : default;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.Emblazon)
            Active = true;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            Tiles.ZoneTiles(Arena, Tiles.Mask, ForbiddenSoakers[pcSlot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count == 0 || !Active)
            return;

        if (ForbiddenSoakers[slot])
        {
            if (Tiles.InActiveTile(actor))
                hints.Add("GTFO from tile!");
        }
        else if (Towers.FindIndex(t => t[Tiles.GetTile(actor)]) is var soaked && soaked >= 0)
        {
            var count = Raid.WithSlot().ExcludedFromMask(ForbiddenSoakers).Count(p => Towers[soaked][Tiles.GetTile(p.Item2)]);
            if (count > 1)
                hints.Add("Too many people in tower!");
        }
        else if (!Tiles.InActiveTile(actor))
            hints.Add("Soak a tower!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0 || !Active)
            return;

        var ts = Tiles.TileShape();

        hints.AddForbiddenZone(p => ForbiddenSoakers[slot] ? ts(p) : !ts(p), Activation);

        hints.PredictedDamage.Add((Raid.WithSlot().Where(r => ts(r.Item2.Position)).Mask(), Activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (!Active)
            return;

        if (state == 0x00800040)
        {
            Towers.Clear();
            foreach (var c in Casters)
                Towers.Add(Tiles.GetConnectedTiles(Tiles.GetTile(c.CastInfo!.LocXZ)));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp)
            ForbiddenSoakers.Set(Raid.FindSlot(actor.InstanceID));
    }
}
