namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class P1Explosion(BossModule module) : Components.CastTowers(module, AID._Spell_Explosion, 3);

class SpecterOfTheLost(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(48, 30.Degrees()), (uint)TetherID._Gen_Tether_89, AID._Ability_SpecterOfTheLost);
class SpecterOfTheLostAOE(BossModule module) : Components.StandardAOEs(module, AID._Ability_SpecterOfTheLost, new AOEShapeCone(48, 30.Degrees()));

class AlexandrianThunderIV(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AlexandrianThunderIV or AID._Ability_AlexandrianThunderIV1)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AlexandrianThunderIV or AID._Ability_AlexandrianThunderIV1)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Take(1).Select(csr => new AOEInstance((AID)csr.CastInfo!.Action.ID == AID._Ability_AlexandrianThunderIV ? new AOEShapeCircle(8) : new AOEShapeDonut(8, 20), csr.Position, Activation: Module.CastFinishAt(csr.CastInfo)));
}

class ThunderSlash(BossModule module) : Components.StandardAOEs(module, AID._Ability_ThunderSlash, new AOEShapeCone(24, 30.Degrees()))
{
    private Tiles? Tiles;

    public override void Update()
    {
        Tiles ??= Module.FindComponent<Tiles>();
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(2, Casters.Count); i++)
        {
            var caster = Casters[i];
            yield return new AOEInstance(Shape, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation, Module.CastFinishAt(caster.CastInfo), Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky: i == 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 0 && Tiles is { } t)
            hints.AddForbiddenZone(t.TileShape(), NumCasts > 0 ? default : Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class P2Explosion(BossModule module) : Components.CastTowers(module, AID._Spell_Explosion1, 3, minSoakers: 3, maxSoakers: 4)
{
    private BitMask TetheredPlayers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.AddsTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            TetheredPlayers.Set(slot);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.AddsTether && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            TetheredPlayers.Clear(slot);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers |= TetheredPlayers;
    }
}

class StockBreak(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            AddStack(actor, WorldState.FutureTime(8.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_StockBreak1:
            case AID._Ability_StockBreak2:
                NumCasts++;
                break;
            case AID._Ability_StockBreak3:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }
}

class RosebloodDrop(BossModule module) : Components.Adds(module, (uint)OID.RosebloodDrop1)
{
    public bool Spawned { get; private set; }

    public override void Update()
    {
        Spawned |= ActiveActors.Any();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.RosebloodDrop1, 1);

        if (actor.Role is Role.Healer or Role.Ranged && ActiveActors.MaxBy(a => a.HPMP.CurHP) is { } target)
            hints.SetPriority(target, 2);
    }
}

class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_PerfumedQuietus1);

class AlexandrianThunderII(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle Rotation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.ThunderCCW:
                Rotation = 10.Degrees();
                break;
            case IconID.ThunderCW:
                Rotation = -10.Degrees();
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_AlexandrianThunderII && Rotation != default)
            Sequences.Add(new(new AOEShapeCone(24, 22.5f.Degrees()), caster.Position, caster.Rotation, Rotation, Module.CastFinishAt(spell), 1, 15));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_AlexandrianThunderII or AID._Ability_AlexandrianThunderII1)
        {
            NumCasts++;
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }
}

class AlexandrianThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AlexandrianThunderIII, AID._Spell_AlexandrianThunderIII1, 4, 5)
{
    private Tiles? Tiles;

    public override void Update()
    {
        Tiles ??= Module.FindComponent<Tiles>();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Tiles != null && Spreads.Count > 0)
            hints.AddForbiddenZone(Tiles.TileShape(), Spreads[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Count > 0 && Tiles != null && Tiles.InActiveTile(actor))
            hints.Add($"GTFO from rose tile!");
    }
}

class Voidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime NextActivation;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00020001)
            NextActivation = WorldState.CurrentTime;
        if (index == 1 && state == 0x00080004)
            NextActivation = default;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NextActivation != default)
            yield return new AOEInstance(new AOEShapeCircle(2), Arena.Center, Activation: NextActivation);
    }
}

class B3Emblazon(BossModule module) : BossComponent(module)
{
    public BitMask Baiters;
    private BitMask TowerTiles;
    private DateTime Activation;
    private Tiles? TilesComponent;

    private Tiles Tiles => TilesComponent!;

    public override void Update()
    {
        TilesComponent ??= Module.FindComponent<Tiles>();
    }

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
        if ((AID)spell.Action.ID == AID._Spell_Explosion2)
            TowerTiles.Set(Tiles.GetTile(spell.LocXZ));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_Emblazon)
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
            foreach (var t in Tiles.ActiveTiles)
                Tiles.ZoneTile(Arena, t, ArenaColor.AOE);
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

class B3Explosion(BossModule module) : Components.CastCounter(module, AID._Spell_Explosion2)
{
    private Tiles? TilesComponent;
    private Tiles Tiles => TilesComponent!;
    private bool Active;

    private readonly List<Actor> Casters = [];
    private readonly List<BitMask> Towers = [];
    private BitMask ForbiddenSoakers;

    public override void Update()
    {
        TilesComponent ??= Module.FindComponent<Tiles>();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID._Ability_Emblazon)
            Active = true;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var t in Tiles.ActiveTiles)
                Tiles.ZoneTile(Arena, t, ForbiddenSoakers[pcSlot] ? ArenaColor.AOE : ArenaColor.SafeFromAOE);
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

        hints.AddForbiddenZone(p => ForbiddenSoakers[slot] ? ts(p) : !ts(p), Module.CastFinishAt(Casters[0].CastInfo));
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
        if ((SID)status.ID == SID._Gen_MagicVulnerabilityUp)
            ForbiddenSoakers.Set(Raid.FindSlot(actor.InstanceID));
    }
}
