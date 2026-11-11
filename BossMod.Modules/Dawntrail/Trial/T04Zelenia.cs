namespace BossMod.Dawntrail.Trial.T04Zelenia;

public enum OID : uint
{
    Boss = 0x47C3, // R5.500, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    BriarThorn1 = 0x47C4, // R1.000, x0 (spawn during fight)
    BriarThorn2 = 0x47C5, // R0.500, x0 (spawn during fight)
    RosebloodDrop = 0x485B, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 43055, // Boss->player, no cast, single-target
    Teleport = 43054, // Boss->location, no cast, single-target

    AlexandrianThunderIVVisual = 43480, // Boss->self, 6.0+0.7s cast, single-target
    AlexandrianThunderIVCircle1 = 43084, // Helper->self, 6.7s cast, range 8 circle
    AlexandrianThunderIVDonut1 = 43085, // Helper->self, 6.7s cast, range 8-24 donut
    AlexandrianThunderIVCircle2 = 43446, // Helper->self, 6.7s cast, range 8 circle
    AlexandrianThunderIVDonut2 = 43447, // Helper->self, 6.7s cast, range 8-24 donut

    ShockVisual = 43056, // Boss->self, 3.0s cast, single-target, visual (spreads)
    ShockLock = 43057, // Helper->self, no cast, single-target, locks voidzone
    Shock1 = 43058, // Helper->self, no cast, range 4 circle
    Shock2 = 43059, // Helper->self, no cast, range 4 circle
    Shock3 = 43060, // Helper->self, no cast, range 4 circle
    Shock4 = 43061, // Helper->self, no cast, range 4 circle
    Shock5 = 43062, // Helper->self, no cast, range 4 circle
    Shock6 = 43063, // Helper->self, no cast, range 4 circle

    PowerBreak1 = 43112, // Boss->self, 6.0s cast, range 24 width 64 rect
    PowerBreak2 = 43113, // Boss->self, 6.0s cast, range 24 width 64 rect

    HolyHazardVisual1 = 43124, // Boss->self, 7.0+0.7s cast, single-target, visual
    HolyHazardVisual2 = 43122, // Boss->self, 7.0+0.7s cast, single-target, visual
    HolyHazardVisual3 = 43120, // Boss->self, 7.0+0.7s cast, single-target, visual
    HolyHazardVisual4 = 43125, // Boss->self, 7.0+0.7s cast, single-target, visual
    HolyHazardVisual5 = 43121, // Boss->self, 7.0+0.7s cast, single-target, visual
    HolyHazardVisual6 = 43123, // Boss->self, 7.0+0.7s cast, single-target, visual
    HolyHazardVisual7 = 43065, // Boss->self, no cast, single-target, visual
    HolyHazard = 43126, // Helper->self, 7.7s cast, range 24 120-degree cone

    SpecterOfTheLostVisual = 43128, // Boss->self, 5.0+0.7s cast, single-target, visual (tank cones)
    SpecterOfTheLost = 43129, // Helper->player, 5.7s cast, range 50 45-degree cone

    ThunderSlashVisual1 = 43078, // Boss->self, 6.0+0.7s cast, single-target, visual
    ThunderSlashVisual2 = 43079, // Boss->self, 6.0+0.7s cast, single-target, visual
    ThunderSlash = 43083, // Helper->self, 6.7s cast, range 24 60-degree cone

    RosebloodBloomVisual1 = 43093, // Boss->self, 3.9s cast, range 50 circle, visual
    RosebloodBloomVisual2 = 43091, // Boss->self, 2.6+0.4s cast, single-target, visual
    RosebloodBloom = 43479, // Helper->self, 5.0s cast, range 50 circle, knockback 10 (ignore immunes)

    PerfumedQuietusVisual = 43094, // Boss->self, 4.0s cast, single-target, visual
    PerfumedQuietus = 43095, // Helper->self, 25.1s cast, range 50 circle, phase-transition raidwide (during cutscene stun)

    QueensCrusade = 43092, // Helper->self, 3.7s cast, range 4 circle, arena shrink telegraph
    AlexandrianThunderIIIVisual = 43100, // Boss->self, 3.0s cast, single-target, visual
    AlexandrianThunderIII1 = 43102, // Helper->location, 7.0s cast, range 4 circle + connected tile cones
    AlexandrianThunderIII2 = 43439, // Helper->location, 6.0s cast, range 4 circle + connected tile cones

    StockBreakVisual = 43086, // Boss->self, 7.0s cast, single-target, visual (stack, 4 hits)
    StockBreak1 = 43087, // Helper->location, no cast, range 6 circle
    StockBreak2 = 43088, // Helper->location, no cast, range 6 circle
    StockBreak3 = 43089, // Helper->location, no cast, range 6 circle
    StockBreak4 = 43090, // Helper->location, no cast, range 6 circle

    RosebloodDropSpawn = 43442, // Boss->self, 3.0s cast, single-target, visual
    RosebloodDropAOE = 43443, // RosebloodDrop->self, no cast, donut segment

    ValorousAscension = 43070, // Boss->self, 3.0s cast, single-target, visual (raidwide x3)
    ValorousAscension1 = 43071, // Helper->self, 3.7s cast, range 50 circle
    ValorousAscension2 = 43072, // Helper->self, no cast, range 50 circle
    ValorousAscension3 = 43073, // Helper->self, no cast, range 50 circle
    ValorousAscensionRect = 43074, // BriarThorn1->self, 5.0s cast, range 40 width 8 rect
    ThornedCatharsis = 43127, // Boss->self, 5.0s cast, range 50 circle, raidwide
}

public enum IconID : uint
{
    Shock = 581, // player
    StockBreak = 590, // player
}

public enum TetherID : uint
{
    RosebloodDrop = 197, // Boss->RosebloodDrop
}

class AlexandrianThunderIV(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(ulong caster, AOEInstance aoe)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 0 ? _aoes.Where(c => c.aoe.Activation <= _aoes[0].aoe.Activation.AddSeconds(1)).Select(c => c.aoe) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.AlexandrianThunderIVCircle1 or AID.AlexandrianThunderIVCircle2 => new AOEShapeCircle(8),
            AID.AlexandrianThunderIVDonut1 or AID.AlexandrianThunderIVDonut2 => new AOEShapeDonut(8, 24),
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add((caster.InstanceID, new(shape, spell.LocXZ, default, Module.CastFinishAt(spell))));
            _aoes.SortBy(c => c.aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AlexandrianThunderIVCircle1 or AID.AlexandrianThunderIVCircle2 or AID.AlexandrianThunderIVDonut1 or AID.AlexandrianThunderIVDonut2)
        {
            var index = _aoes.FindIndex(c => c.caster == caster.InstanceID);
            if (index >= 0)
            {
                ++NumCasts;
                _aoes.RemoveAt(index);
            }
        }
    }
}

class ShockBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.Shock, AID.ShockLock, 8, centerAtTarget: true);

class ShockPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Origin, int Cycles)> _puddles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _puddles.Select(p => new AOEInstance(new AOEShapeCircle(4), p.Origin));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShockLock:
                _puddles.Add((caster.Position, 0));
                break;
            case AID.Shock6:
                ++NumCasts;
                if (_puddles.Count == 0)
                    break;
                var index = _puddles.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 1));
                if (index < 0)
                {
                    ReportError($"no puddle at {caster.Position}");
                    index = 0;
                }
                if (_puddles[index].Cycles > 0)
                    _puddles.RemoveAt(index);
                else
                    _puddles[index] = (_puddles[index].Origin, 1);
                break;
        }
    }
}

class PowerBreak(BossModule module) : Components.GroupedAOEs(module, [AID.PowerBreak1, AID.PowerBreak2], new AOEShapeRect(24, 32));
class HolyHazard(BossModule module) : Components.StandardAOEs(module, AID.HolyHazard, new AOEShapeCone(24, 60.Degrees()), maxCasts: 2);
class SpecterOfTheLost(BossModule module) : Components.BaitAwayCast(module, AID.SpecterOfTheLost, new AOEShapeCone(50, 22.5f.Degrees()));
class ThunderSlash(BossModule module) : Components.StandardAOEs(module, AID.ThunderSlash, new AOEShapeCone(24, 30.Degrees()), maxCasts: 4);

class RosebloodBloom(BossModule module) : Components.KnockbackFromCastTarget(module, AID.RosebloodBloom, 10, ignoreImmunes: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(c.Position, 6), Module.CastFinishAt(c.CastInfo));
    }
}

// TODO: Quietus resolves during cutscene stun — warn on Bloom knockback instead
class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, AID.RosebloodBloom);
class ThornedCatharsis(BossModule module) : Components.RaidwideCast(module, AID.ThornedCatharsis);
class ValorousAscension(BossModule module) : Components.RaidwideMultiHit(module, AID.ValorousAscension1, [AID.ValorousAscension1, AID.ValorousAscension2, AID.ValorousAscension3], 0, 0.95f, 3);

class StockBreak(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8, 8, stackHits: 4)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            AddStack(actor, WorldState.FutureTime(8.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StockBreak1:
            case AID.StockBreak2:
            case AID.StockBreak3:
                for (var i = 0; i < Stacks.Count; ++i)
                {
                    var s = Stacks[i];
                    s.Hits--;
                    s.Activation = WorldState.FutureTime(1.1f);
                    Stacks[i] = s;
                }
                break;
            case AID.StockBreak4:
                Stacks.Clear();
                break;
        }
    }
}

class CenterVoid(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.QueensCrusade)
            _aoe = new(new AOEShapeCircle(4), Arena.Center, default, Module.CastFinishAt(spell, 0.1f));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 1 && state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = T04Zelenia.DonutArena;
        }
        else if (index == 1 && state == 0x00080004)
        {
            Arena.Bounds = T04Zelenia.DefaultArena;
        }
    }
}

class RoseTiles(BossModule module) : BossComponent(module)
{
    public const int NumTiles = 6;
    public static readonly AOEShapeCone Slice = new(16, 30.Degrees());

    private const float DropRoll = 29;

    public BitMask Lit;

    public static Angle Direction(int tile) => (180 - 60 * tile).Degrees();

    public override void OnMapEffect(byte index, uint state)
    {
        var tile = index - 4;
        if (tile is < 0 or >= NumTiles)
            return;

        if (state is 0x00400100 or 0x00800040)
            Lit.Set(tile);
        else if (state == 0x00040020)
            Lit.Clear(tile);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.RosebloodDrop && WorldState.Actors.Find(tether.Target) is { } drop)
            Lit.Set(TileAt(Angle.FromDirection(drop.Position + DropRoll * drop.Rotation.ToDirection() - Arena.Center)));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E46)
            Lit.Set(TileAt(actor.Rotation));
    }

    public static int TileAt(Angle direction) => ((int)MathF.Round((180 - direction.Deg) / 60) % NumTiles + NumTiles) % NumTiles;

    public BitMask ConnectedTo(int tile)
    {
        var group = new BitMask();
        if (!Lit[tile])
            return group;

        group.Set(tile);
        for (var t = (tile + 1) % NumTiles; t != tile && Lit[t]; t = (t + 1) % NumTiles)
            group.Set(t);
        for (var t = (tile + NumTiles - 1) % NumTiles; t != tile && Lit[t]; t = (t + NumTiles - 1) % NumTiles)
            group.Set(t);
        return group;
    }
}

class AlexandrianThunderIII(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _strike = new(4);
    private RoseTiles? _tiles;
    private readonly List<(ulong Caster, WPos Target, BitMask Tiles, DateTime Activation)> _strikes = [];
    private DateTime _surge;
    private int _surgeCasts;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var tileActivation = new DateTime[RoseTiles.NumTiles];
        foreach (var s in _strikes)
        {
            yield return new(_strike, s.Target, default, s.Activation);
            foreach (var t in s.Tiles.SetBits())
                if (tileActivation[t] == default || s.Activation < tileActivation[t])
                    tileActivation[t] = s.Activation;
        }

        _tiles ??= Module.FindComponent<RoseTiles>();
        if (_surge != default && _tiles != null)
            foreach (var t in _tiles.Lit.SetBits())
                if (tileActivation[t] == default)
                    tileActivation[t] = _surge;

        for (var t = 0; t < RoseTiles.NumTiles; ++t)
            if (tileActivation[t] != default)
                yield return new(RoseTiles.Slice, Arena.Center, RoseTiles.Direction(t), tileActivation[t]);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.Shape == RoseTiles.Slice ? p => aoe.Distance(p) - new Vector<float>(0.5f) : aoe.DistV, aoe.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIII1:
            case AID.AlexandrianThunderIII2:
                _strikes.Add((caster.InstanceID, spell.LocXZ, TilesHitFrom(spell.LocXZ), Module.CastFinishAt(spell)));
                break;
            case AID.AlexandrianThunderIVCircle2:
            case AID.AlexandrianThunderIVDonut2:
                if (_surge == default && _strikes.Count == 0)
                    _surge = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIII1:
            case AID.AlexandrianThunderIII2:
                ++NumCasts;
                _strikes.RemoveAll(s => s.Caster == caster.InstanceID);
                break;
            case AID.AlexandrianThunderIVCircle2:
            case AID.AlexandrianThunderIVDonut2:
                if (_surge != default && ++_surgeCasts >= 2)
                {
                    _surge = default;
                    _surgeCasts = 0;
                }
                break;
        }
    }

    private BitMask TilesHitFrom(WPos target)
    {
        var hit = new BitMask();
        _tiles ??= Module.FindComponent<RoseTiles>();
        if (_tiles == null)
            return hit;
        for (var t = 0; t < RoseTiles.NumTiles; ++t)
            if (_tiles.Lit[t] && !hit[t] && Intersect.CircleCone(target, _strike.Radius, Arena.Center, RoseTiles.Slice.Radius, RoseTiles.Direction(t).ToDirection(), RoseTiles.Slice.HalfAngle))
                hit |= _tiles.ConnectedTo(t);
        return hit;
    }
}

class ValorousAscensionRect(BossModule module) : Components.GenericAOEs(module, AID.ValorousAscensionRect)
{
    private readonly List<(Actor Thorn, AOEInstance AOE)> _lines = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _lines.Select(l => l.AOE).OrderBy(a => a.Activation).Take(2);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DB && (OID)actor.OID == OID.BriarThorn1)
            _lines.Add((actor, new(new AOEShapeRect(40, 4), actor.Position, actor.Rotation, WorldState.FutureTime(11.1f))));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _lines.RemoveAll(l => l.Thorn == caster);
            _lines.Add((caster, new(new AOEShapeRect(40, 4), caster.Position, spell.Rotation, Module.CastFinishAt(spell))));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _lines.RemoveAll(l => l.Thorn == caster);
    }
}

class T04ZeleniaStates : StateMachineBuilder
{
    public T04ZeleniaStates(BossModule module) : base(module)
    {
        // TODO: enrage never reached (kill at ~6:50)
        TrivialPhase()
            .ActivateOnEnter<ThornedCatharsis>()
            .ActivateOnEnter<PerfumedQuietus>()
            .ActivateOnEnter<ValorousAscension>()
            .ActivateOnEnter<ValorousAscensionRect>()
            .ActivateOnEnter<SpecterOfTheLost>()
            .ActivateOnEnter<PowerBreak>()
            .ActivateOnEnter<HolyHazard>()
            .ActivateOnEnter<ThunderSlash>()
            .ActivateOnEnter<RosebloodBloom>()
            .ActivateOnEnter<StockBreak>()
            .ActivateOnEnter<ShockBait>()
            .ActivateOnEnter<ShockPuddles>()
            .ActivateOnEnter<CenterVoid>()
            .ActivateOnEnter<AlexandrianThunderIV>()
            .ActivateOnEnter<RoseTiles>()
            .ActivateOnEnter<AlexandrianThunderIII>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1030, NameID = 13861)]
public class T04Zelenia(ModuleInit init) : BossModule(init, new(100, 100), DefaultArena)
{
    public static readonly ArenaBoundsCircle DefaultArena = new(16);
    public static readonly ArenaBoundsCustom DonutArena = new(16, DefaultArena.Clipper.Difference(new(CurveApprox.Circle(16, 0.05f)), new(CurveApprox.Circle(4, 0.05f))));
}
