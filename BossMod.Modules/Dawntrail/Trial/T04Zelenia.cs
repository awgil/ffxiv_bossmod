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

class AlexandrianThunderIV(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(8);
    private static readonly AOEShapeDonut _donut = new(8, 24);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.AlexandrianThunderIVCircle1 or AID.AlexandrianThunderIVCircle2 => _circle,
            AID.AlexandrianThunderIVDonut1 or AID.AlexandrianThunderIVDonut2 => _donut,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.AlexandrianThunderIVCircle1 or AID.AlexandrianThunderIVCircle2 or AID.AlexandrianThunderIVDonut1 or AID.AlexandrianThunderIVDonut2)
            _aoes.RemoveAt(0);
    }
}

class ShockSpread(BossModule module) : Components.UniformStackSpread(module, 0, 4, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Shock)
            AddSpread(actor, WorldState.FutureTime(8));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ShockLock)
            Spreads.Clear();
    }
}

class ShockAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShockLock:
                _aoes.Add(new(_shape, caster.Position));
                break;
            case AID.Shock6:
                if (++NumCasts >= 2 * _aoes.Count)
                {
                    _aoes.Clear();
                    NumCasts = 0;
                }
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
        var remaining = (AID)spell.Action.ID switch
        {
            AID.StockBreak1 => 3,
            AID.StockBreak2 => 2,
            AID.StockBreak3 => 1,
            AID.StockBreak4 => 0,
            _ => -1
        };
        if (remaining == 0)
            Stacks.Clear();
        else if (remaining > 0)
            for (var i = 0; i < Stacks.Count; ++i)
                Stacks[i] = Stacks[i] with { Activation = WorldState.FutureTime(1.1f), Hits = remaining };
    }
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(4);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.QueensCrusade)
            _aoe = new(_shape, Arena.Center, default, Module.CastFinishAt(spell, 0.1f));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
            return;
        switch (state)
        {
            case 0x00020001:
                _aoe = null;
                Arena.Bounds = T04Zelenia.DonutArena;
                break;
            case 0x00080004:
                Arena.Bounds = T04Zelenia.DefaultArena;
                break;
        }
    }
}

class AlexandrianThunderIII(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _cone = new(16, 30.Degrees());
    private static readonly AOEShapeCircle _circle = new(4);
    private readonly List<(ulong caster, AOEInstance aoe)> _temp = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly bool[] _tiles = new bool[6];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 0x04 or > 0x09)
            return;
        var tile = (6 - (index - 0x04)) % 6;
        switch (state)
        {
            case 0x00400100:
                _tiles[tile] = true;
                break;
            case 0x00040020:
                _tiles[tile] = false;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.AlexandrianThunderIII1 or AID.AlexandrianThunderIII2)
        {
            var visited = new bool[6];
            var pos = Arena.Center;
            var loc = spell.LocXZ;
            var act = Module.CastFinishAt(spell);
            var id = caster.InstanceID;
            _temp.Add((id, new(_circle, loc, default, act)));

            for (var i = 0; i < 6; ++i)
            {
                if (!_tiles[i] || visited[i])
                    continue;
                if (!Intersect.CircleCone(loc, 4, pos, 16, (180 - 60 * i).Degrees().ToDirection(), 30.Degrees()))
                    continue;

                var left = i;
                while (true)
                {
                    var prev = (left - 1 + 6) % 6;
                    if (!_tiles[prev] || visited[prev])
                        break;
                    left = prev;
                }
                var right = i;
                while (true)
                {
                    var next = (right + 1) % 6;
                    if (!_tiles[next] || visited[next])
                        break;
                    right = next;
                }

                for (var j = left; ; j = (j + 1) % 6)
                {
                    if (!visited[j])
                    {
                        visited[j] = true;
                        _temp.Add((id, new(_cone, pos, (180 - 60 * j).Degrees(), act)));
                    }
                    if (j == right)
                        break;
                }
                break;
            }
            Rebuild();
        }
        else if (aid is AID.AlexandrianThunderIVCircle2 or AID.AlexandrianThunderIVDonut2 && _aoes.Count == 0)
        {
            var act = Module.CastFinishAt(spell);
            for (var i = 0; i < 6; ++i)
                if (_tiles[i])
                    _aoes.Add(new(_cone, Arena.Center, (180 - 60 * i).Degrees(), act));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E46)
            return;
        var index = (int)MathF.Round(actor.Rotation.Deg) switch
        {
            143 or -143 or 180 => 0,
            83 or 156 or 119 => 1,
            96 or 23 or 59 => 2,
            0 or -36 or 36 => 3,
            -96 or -23 or -60 => 4,
            -120 or -156 or -83 => 5,
            _ => -1
        };
        if (index >= 0)
            _tiles[index] = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIII1:
            case AID.AlexandrianThunderIII2:
                _temp.RemoveAll(a => a.caster == caster.InstanceID);
                Rebuild();
                break;
            case AID.AlexandrianThunderIVCircle2:
            case AID.AlexandrianThunderIVDonut2:
                if (++NumCasts == 2)
                {
                    _aoes.Clear();
                    NumCasts = 0;
                }
                break;
        }
    }

    private void Rebuild()
    {
        _aoes.Clear();
        var visited = new bool[6];
        foreach (var (_, aoe) in _temp)
        {
            if (ReferenceEquals(aoe.Shape, _circle))
            {
                _aoes.Add(aoe);
                continue;
            }
            var slice = (int)MathF.Round((180 - aoe.Rotation.Deg) / 60f);
            if (slice is >= 0 and < 6 && !visited[slice])
            {
                visited[slice] = true;
                _aoes.Add(aoe);
            }
        }
    }
}

class ValorousAscensionRect(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(40, 4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DB && (OID)actor.OID == OID.BriarThorn1)
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(11.1f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.ValorousAscensionRect)
            _aoes.RemoveAt(0);
    }
}

class T04ZeleniaStates : StateMachineBuilder
{
    public T04ZeleniaStates(BossModule module) : base(module)
    {
        // TODO: enrage never reached (kill at ~6:50)
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<AlexandrianThunderIV>()
            .ActivateOnEnter<AlexandrianThunderIII>()
            .ActivateOnEnter<ShockSpread>()
            .ActivateOnEnter<ShockAOE>()
            .ActivateOnEnter<PowerBreak>()
            .ActivateOnEnter<HolyHazard>()
            .ActivateOnEnter<SpecterOfTheLost>()
            .ActivateOnEnter<ThunderSlash>()
            .ActivateOnEnter<RosebloodBloom>()
            .ActivateOnEnter<PerfumedQuietus>()
            .ActivateOnEnter<ValorousAscension>()
            .ActivateOnEnter<ThornedCatharsis>()
            .ActivateOnEnter<StockBreak>()
            .ActivateOnEnter<ValorousAscensionRect>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1030, NameID = 13861)]
public class T04Zelenia(ModuleInit init) : BossModule(init, new(100, 100), DefaultArena)
{
    public static readonly ArenaBoundsCircle DefaultArena = new(16);
    public static readonly ArenaBoundsCustom DonutArena = new(16, DefaultArena.Clipper.Difference(new(CurveApprox.Circle(16, 0.05f)), new(CurveApprox.Circle(4, 0.05f))));
}
