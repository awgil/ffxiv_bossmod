namespace BossMod.Dawntrail.Raid.RM05NDancingGreen;

public enum OID : uint
{
    Boss = 0x47B6, // R4.998, x1
    Helper = 0x233C, // R0.500, x40, Helper type
    Frogtourage = 0x47B7, // R3.142, x8
    Spotlight = 0x47B8, // R1.000, x8
}

public enum AID : uint
{
    AutoAttack = 44484, // Boss->player, no cast, single-target
    Teleport = 42693, // Boss->location, no cast, single-target

    DoTheHustle1 = 42697, // Boss->self, 5.0s cast, range 50 180-degree cone
    DoTheHustle2 = 42698, // Boss->self, 5.0s cast, range 50 180-degree cone
    DeepCut = 42694, // Boss->self, 5.0s cast, single-target, visual (tankbusters)
    DeepCutAOE = 42695, // Helper->self, no cast, range 60 45-degree cone
    DiscoInfernal = 42745, // Boss->self, 4.0s cast, range 60 circle, raidwide
    CelebrateGoodTimes = 42696, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Shame = 42747, // Helper->player, 1.0s cast, single-target, failed spotlight
    FullBeat = 42750, // Boss->self, 7.0s cast, single-target, visual (stack)
    FullBeatAOE = 42751, // Helper->players, 7.0s cast, range 6 circle stack
    EighthBeats = 42754, // Boss->self, 5.0s cast, single-target, visual (spreads)
    EighthBeatsAOE = 42755, // Helper->player, 5.0s cast, range 5 circle spread

    TwoSnapTwist1 = 42197, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist2 = 42198, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist3 = 42699, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist4 = 42700, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist5 = 42701, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist6 = 42702, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist7 = 42703, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist8 = 42704, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst = 42705, // Helper->self, 1.5s cast, range 25 width 50 rect
    TwoSnapTwistLast = 42706, // Helper->self, 3.5s cast, range 25 width 50 rect
    FourSnapTwist1 = 42201, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist2 = 42202, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist3 = 42716, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist4 = 42717, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist5 = 42718, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist6 = 42719, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist7 = 42720, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist8 = 42721, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst1 = 42722, // Helper->self, 1.0s cast, range 25 width 50 rect
    FourSnapTwistFirst2 = 42723, // Helper->self, 1.5s cast, range 25 width 50 rect
    FourSnapTwistFirst3 = 42724, // Helper->self, 2.0s cast, range 25 width 50 rect
    FourSnapTwistLast = 42725, // Helper->self, 3.5s cast, range 25 width 50 rect

    FunkyFloor = 42741, // Boss->self, 2.5+0.5s cast, single-target, visual (checkerboard)
    FunkyFloorAOE = 42742, // Helper->self, no cast, tiles
    RideTheWaves = 42743, // Boss->self, 3.5+0.5s cast, single-target, visual (rows)
    RideTheWavesAOE = 42744, // Helper->self, no cast, tiles

    EnsembleAssemble = 39472, // Boss->self, 3.0s cast, single-target
    ArcadyNightFever = 42757, // Boss->self, 4.8s cast, single-target
    ArcadyNightFeverVisual = 42758, // Boss->self, no cast, single-target
    FrogtourageDanceVisual1 = 37825, // Helper->Frogtourage, 1.2s cast, single-target
    FrogtourageDanceVisual2 = 37824, // Boss->self, no cast, single-target
    FrogtourageDanceVisual3 = 37897, // Boss->self, no cast, single-target
    FrogtourageDanceVisual4 = 37909, // Boss->self, no cast, single-target
    FrogtourageDanceVisual5 = 37930, // Boss->self, no cast, single-target
    FrogtourageDanceVisual6 = 37836, // Frogtourage->self, no cast, single-target
    FrogDanceRight = 42764, // Frogtourage->self, 1.7s cast, single-target
    FrogDanceLeft = 42765, // Frogtourage->self, 1.7s cast, single-target
    Stone = 39092, // Boss->self, no cast, single-target
    LetsDance = 42772, // Boss->self, 5.8s cast, single-target, visual (half-room cleaves)
    LetsDanceVisual1 = 42775, // Boss->self, no cast, single-target
    LetsDanceVisual2 = 42776, // Boss->self, no cast, single-target
    LetsDanceVisual3 = 37835, // Frogtourage->self, no cast, single-target
    LetsDanceAOE = 39900, // Helper->self, no cast, range 25 width 90 rect
    LetsPoseFrog1 = 37843, // Frogtourage->self, 5.0s cast, single-target
    LetsPoseFrog2 = 37844, // Frogtourage->self, 5.0s cast, single-target
    LetsPose1 = 42777, // Boss->self, 5.0s cast, range 60 circle, raidwide
    LetsPose2 = 42778, // Boss->self, 5.0s cast, range 60 circle, raidwide

    Frogtourage = 42756, // Boss->self, 3.0s cast, single-target, visual (frogs)
    MoonburnStart = 42781, // Frogtourage->self, no cast, single-target
    MoonburnEnd = 42782, // Frogtourage->self, 1.0s cast, single-target
    Moonburn1 = 42783, // Helper->self, 10.5s cast, range 40 width 15 rect
    Moonburn2 = 42784, // Helper->self, 10.5s cast, range 40 width 15 rect
}

public enum SID : uint
{
    BurnBabyBurn = 4460, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    DeepCut = 471, // player
    FullBeat = 317, // player
    EighthBeats = 376, // player
    BurnBabyBurn = 601, // player
}

class DoTheHustle(BossModule module) : Components.GroupedAOEs(module, [AID.DoTheHustle1, AID.DoTheHustle2], new AOEShapeCone(50, 90.Degrees()));
class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 22.5f.Degrees()), (uint)IconID.DeepCut, AID.DeepCutAOE, 5.7f, damageType: AIHints.PredictedDamageType.Tankbuster);
class DiscoInfernal(BossModule module) : Components.RaidwideCast(module, AID.DiscoInfernal);
class CelebrateGoodTimes(BossModule module) : Components.RaidwideCast(module, AID.CelebrateGoodTimes);
class LetsPose1(BossModule module) : Components.RaidwideCast(module, AID.LetsPose1);
class LetsPose2(BossModule module) : Components.RaidwideCast(module, AID.LetsPose2);
class FullBeat(BossModule module) : Components.StackWithCastTargets(module, AID.FullBeatAOE, 6, 4);
class EighthBeats(BossModule module) : Components.SpreadFromCastTargets(module, AID.EighthBeatsAOE, 5);
class Moonburn(BossModule module) : Components.GroupedAOEs(module, [AID.Moonburn1, AID.Moonburn2], new AOEShapeRect(40, 7.5f));

class SnapTwist(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(25, 25);

    private static readonly AID[] _bossCasts = [AID.TwoSnapTwist1, AID.TwoSnapTwist2, AID.TwoSnapTwist3, AID.TwoSnapTwist4, AID.TwoSnapTwist5, AID.TwoSnapTwist6, AID.TwoSnapTwist7, AID.TwoSnapTwist8,
        AID.FourSnapTwist1, AID.FourSnapTwist2, AID.FourSnapTwist3, AID.FourSnapTwist4, AID.FourSnapTwist5, AID.FourSnapTwist6, AID.FourSnapTwist7, AID.FourSnapTwist8];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => (_aoes.Count > 0 ? _aoes : _predicted).Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (_bossCasts.Contains(aid))
        {
            var four = aid is >= AID.FourSnapTwist3 and <= AID.FourSnapTwist8 or AID.FourSnapTwist1 or AID.FourSnapTwist2;
            _predicted.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1)));
            _predicted.Add(new(_shape, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, four ? 3.5f : 3.6f)));
        }
        else if (aid is AID.TwoSnapTwistFirst or AID.TwoSnapTwistLast or AID.FourSnapTwistFirst1 or AID.FourSnapTwistFirst2 or AID.FourSnapTwistFirst3 or AID.FourSnapTwistLast)
        {
            _predicted.Clear();
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.TwoSnapTwistFirst or AID.TwoSnapTwistLast or AID.FourSnapTwistFirst1 or AID.FourSnapTwistFirst2 or AID.FourSnapTwistFirst3 or AID.FourSnapTwistLast)
        {
            if (aid is AID.TwoSnapTwistFirst or AID.TwoSnapTwistLast or AID.FourSnapTwistFirst3 or AID.FourSnapTwistLast)
                ++NumCasts;
            var index = _aoes.FindIndex(aoe => aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f) && aoe.Activation <= WorldState.FutureTime(0.5f));
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}

class FunkyFloor(BossModule module) : Components.GenericAOEs(module, AID.FunkyFloorAOE)
{
    private bool? _evenLit;
    private DateTime _activation;

    private static readonly AOEShapeRect _tile = new(2.5f, 2.5f, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_evenLit is not bool even)
            yield break;
        for (var row = 0; row < 8; ++row)
            for (var col = (row & 1) == (even ? 0 : 1) ? 0 : 1; col < 8; col += 2)
                yield return new(_tile, Module.Center + new WDir(-17.5f + 5 * col, -17.5f + 5 * row), default, _activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03)
            return;
        switch (state)
        {
            case 0x00020001:
                _evenLit = true;
                _activation = WorldState.FutureTime(2.5f);
                break;
            case 0x00200010:
                _evenLit = false;
                _activation = WorldState.FutureTime(2.5f);
                break;
            case 0x00080004:
            case 0x00800004:
                _evenLit = null;
                break;
        }
    }
}

class Spotlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> _spots = [];
    private bool _active;
    private DateTime _activation;
    private DateTime _lastResolve;

    private static readonly AOEShapeCircle _shape = new(2.5f);
    private static readonly WPos[] _three = [new(87.5f, 112.5f), new(112.5f, 100), new(100, 87.5f), new(112.5f, 87.5f), new(87.5f, 100), new(100, 112.5f)];
    private static readonly WPos[] _four1 = [new(95, 87.5f), new(105, 112.5f), new(112.5f, 95), new(87.5f, 105)];
    private static readonly WPos[] _four2 = [new(105, 87.5f), new(87.5f, 95), new(112.5f, 105), new(95, 112.5f)];

    private IEnumerable<WPos> CurrentSpots => _spots.Count == 6 ? _spots.Take(3) : _spots;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            foreach (var p in CurrentSpots)
                yield return new(_shape, p, default, _activation, ArenaColor.SafeFromAOE, false);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active)
            hints.Add("Go into a spotlight!", !CurrentSpots.Any(p => actor.Position.InCircle(p, 2.5f)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_active || _spots.Count == 0)
            return;
        var spots = CurrentSpots.ToList();
        hints.AddForbiddenZone(ShapeDistance.Intersection([.. spots.Select(s => ShapeDistance.InvertedCircle(s, 2.5f))]), _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BurnBabyBurn)
        {
            _active = true;
            _activation = WorldState.FutureTime(8);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID != OID.Spotlight || id != 0x11DC || _spots.Count > 0)
            return;
        if (actor.Position.AlmostEqual(new(112.5f, 87.5f), 0.5f))
            _spots.AddRange(_three);
        else if (actor.Position.AlmostEqual(new(87.5f, 112.5f), 0.5f))
            _spots.AddRange(_three.Reverse());
        else if (actor.Position.AlmostEqual(new(95, 87.5f), 0.5f))
            _spots.AddRange(_four1);
        else if (actor.Position.AlmostEqual(new(95, 112.5f), 0.5f))
            _spots.AddRange(_four2);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID != SID.BurnBabyBurn || !_active || WorldState.CurrentTime < _lastResolve.AddSeconds(2))
            return;
        _lastResolve = WorldState.CurrentTime;
        _active = false;
        ++NumCasts;
        if (_spots.Count == 6)
            _spots.RemoveRange(0, 3);
        else if (Raid.WithoutSlot().All(p => p.FindStatus(SID.BurnBabyBurn) == null))
            _spots.Clear();
    }
}

class RideTheWaves(BossModule module) : Components.GenericAOEs(module, AID.RideTheWavesAOE)
{
    private readonly List<AOEInstance> _aoes = [];
    private int _wavesStarted;
    private int _hits;
    private int? _safeColumn;

    private static readonly AOEShapeRect _shape = new(15, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x04)
            return;
        int? safe = state switch
        {
            0x04000400 => 1,
            0x00020002 or 0x08000800 => 2,
            0x10001000 or 0x00100010 => 3,
            0x20002000 or 0x00200020 => 4,
            0x00400040 or 0x40004000 => 5,
            0x80008000 => 6,
            _ => null
        };
        if (state == 0x00080004)
            ++_wavesStarted;
        if (safe is int column)
        {
            _safeColumn = column;
            for (var i = 0; i < 8; ++i)
                if (i != column)
                    _aoes.Add(new(_shape, new(82.5f + i * 5, 70), default, WorldState.FutureTime(3)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction)
            return;
        ++NumCasts;
        ++_hits;
        if (_aoes.Count > 6 && _hits is 10 or 16)
        {
            _aoes.RemoveRange(0, 7);
            --_wavesStarted;
        }
        if (_hits is 3 or 7)
            _safeColumn = null;
        if (_aoes.Count == 0)
        {
            _hits = 0;
            return;
        }
        var moving = Math.Min(_aoes.Count, _wavesStarted <= 1 ? 7 : 14);
        var activation = WorldState.FutureTime(2);
        for (var i = 0; i < moving; ++i)
            _aoes[i] = _aoes[i] with { Origin = _aoes[i].Origin + new WDir(0, 5), Activation = activation };
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (_safeColumn is int column)
            Arena.ZoneRect(new WPos(82.5f + column * 5, 80), new WDir(0, 1), 5, 0, 2.5f, ArenaColor.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_safeColumn is int column && _aoes.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(new WPos(82.5f + column * 5, 80), new WDir(0, 1), 5, 0, 2.5f), _aoes[_wavesStarted <= 1 ? 0 : Math.Min(7, _aoes.Count - 1)].Activation.AddSeconds(1));
    }
}

class LetsDance(BossModule module) : Components.GenericAOEs(module, AID.LetsDanceAOE)
{
    private readonly List<Angle> _frogs = [];
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(25, 45);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0];
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FrogDanceRight:
                _frogs.Add(90.Degrees());
                break;
            case AID.FrogDanceLeft:
                _frogs.Add(-90.Degrees());
                break;
            case AID.LetsDance:
                for (var i = 0; i < _frogs.Count; ++i)
                    _aoes.Add(new(_shape, Module.Center, _frogs[i], Module.CastFinishAt(spell, 1.1f + 1.95f * i)));
                _frogs.Clear();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class RM05NDancingGreenStates : StateMachineBuilder
{
    public RM05NDancingGreenStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Spotlight>()
            .ActivateOnEnter<FunkyFloor>();
    }

    private void SinglePhase(uint id)
    {
        DoTheHustle(id, 9.3f);
        Snap(id + 0x10000, 8.3f, false);
        Snap(id + 0x20000, 2.6f, true);
        DeepCut(id + 0x30000, 1.6f);
        FunkyFloorStack(id + 0x40000, 7.7f);
        DiscoSpotlights(id + 0x50000, 1.2f);
        CelebrateGoodTimes(id + 0x60000, 4.8f);
        RideTheWaves(id + 0x70000, 6.1f);
        LetsDance(id + 0x80000, 8.6f);
        Frogtourage(id + 0x90000, 16.6f);
        FunkyFloorSnap(id + 0xA0000, 5.2f);
        CelebrateGoodTimes(id + 0xB0000, 1.4f);
        DiscoInfernal(id + 0xC0000, 8.5f);

        SimpleState(id + 0xD0000, 120, "Until enrage")
            .ActivateOnEnter<DoTheHustle>()
            .ActivateOnEnter<DeepCut>()
            .ActivateOnEnter<CelebrateGoodTimes>()
            .ActivateOnEnter<LetsPose1>()
            .ActivateOnEnter<LetsPose2>()
            .ActivateOnEnter<FullBeat>()
            .ActivateOnEnter<EighthBeats>()
            .ActivateOnEnter<Moonburn>()
            .ActivateOnEnter<SnapTwist>()
            .ActivateOnEnter<RideTheWaves>()
            .ActivateOnEnter<LetsDance>();
    }

    private void DoTheHustle(uint id, float delay)
    {
        CastMulti(id, [AID.DoTheHustle1, AID.DoTheHustle2], delay, 5, "Cleave")
            .ActivateOnEnter<DoTheHustle>()
            .DeactivateOnExit<DoTheHustle>();
    }

    private void Snap(uint id, float delay, bool four)
    {
        CastMulti(id, [AID.TwoSnapTwist1, AID.TwoSnapTwist2, AID.TwoSnapTwist3, AID.TwoSnapTwist4, AID.TwoSnapTwist5, AID.TwoSnapTwist6, AID.TwoSnapTwist7, AID.TwoSnapTwist8,
            AID.FourSnapTwist1, AID.FourSnapTwist2, AID.FourSnapTwist3, AID.FourSnapTwist4, AID.FourSnapTwist5, AID.FourSnapTwist6, AID.FourSnapTwist7, AID.FourSnapTwist8], delay, 5)
            .ActivateOnEnter<SnapTwist>();
        ComponentCondition<SnapTwist>(id + 0x10, four ? 2 : 1.6f, c => c.NumCasts >= 1, "Side 1");
        ComponentCondition<SnapTwist>(id + 0x20, four ? 1.5f : 2, c => c.NumCasts >= 2, "Side 2")
            .DeactivateOnExit<SnapTwist>();
    }

    private void DeepCut(uint id, float delay)
    {
        Cast(id, AID.DeepCut, delay, 5)
            .ActivateOnEnter<DeepCut>();
        ComponentCondition<DeepCut>(id + 0x10, 0.7f, c => c.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<DeepCut>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void FunkyFloorStack(uint id, float delay)
    {
        Cast(id, AID.FunkyFloor, delay, 2.5f)
            .ActivateOnEnter<FullBeat>();
        ComponentCondition<FunkyFloor>(id + 0x10, 0.5f, c => c.NumCasts >= 1, "Floor 1");
        ComponentCondition<FullBeat>(id + 0x20, 13.2f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<FullBeat>();
        ComponentCondition<FunkyFloor>(id + 0x30, 3.1f, c => c.NumCasts >= 5, "Floor end");
    }

    private void DiscoInfernal(uint id, float delay)
    {
        Cast(id, AID.DiscoInfernal, delay, 4, "Raidwide")
            .ActivateOnEnter<DiscoInfernal>()
            .DeactivateOnExit<DiscoInfernal>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DiscoSpotlights(uint id, float delay)
    {
        DiscoInfernal(id, delay);
        ComponentCondition<Spotlight>(id + 0x100, 18, c => c.NumCasts >= 1, "Spotlights 1");
        DoTheHustle(id + 0x200, 3.5f);
        ComponentCondition<Spotlight>(id + 0x300, 9.5f, c => c.NumCasts >= 2, "Spotlights 2");
    }

    private void CelebrateGoodTimes(uint id, float delay)
    {
        Cast(id, AID.CelebrateGoodTimes, delay, 5, "Raidwide")
            .ActivateOnEnter<CelebrateGoodTimes>()
            .DeactivateOnExit<CelebrateGoodTimes>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RideTheWaves(uint id, float delay)
    {
        Cast(id, AID.RideTheWaves, delay, 3.5f)
            .ActivateOnEnter<RideTheWaves>();
        ComponentCondition<RideTheWaves>(id + 0x10, 4, c => c.NumCasts >= 1, "Waves start");
        Snap(id + 0x100, 18.5f, false);
        CelebrateGoodTimes(id + 0x200, 0.5f);
        ComponentCondition<RideTheWaves>(id + 0x300, 0.1f, c => c.NumCasts >= 16, "Waves end")
            .DeactivateOnExit<RideTheWaves>();
    }

    private void LetsDance(uint id, float delay)
    {
        Cast(id, AID.EnsembleAssemble, delay, 3);
        Cast(id + 0x10, AID.ArcadyNightFever, 2.2f, 4.8f)
            .ActivateOnEnter<LetsDance>();
        Cast(id + 0x20, AID.LetsDance, 14.1f, 5.8f);
        ComponentCondition<LetsDance>(id + 0x30, 1.1f, c => c.NumCasts >= 1, "Dance 1");
        ComponentCondition<LetsDance>(id + 0x40, 13.7f, c => c.NumCasts >= 8, "Dance 8")
            .DeactivateOnExit<LetsDance>();
        CastMulti(id + 0x50, [AID.LetsPose1, AID.LetsPose2], 2.2f, 5, "Raidwide")
            .ActivateOnEnter<LetsPose1>()
            .ActivateOnEnter<LetsPose2>()
            .DeactivateOnExit<LetsPose1>()
            .DeactivateOnExit<LetsPose2>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Frogtourage(uint id, float delay)
    {
        Cast(id, AID.Frogtourage, delay, 3)
            .ActivateOnEnter<Moonburn>()
            .ActivateOnEnter<EighthBeats>();
        ComponentCondition<Moonburn>(id + 0x10, 14.8f, c => c.NumCasts > 0, "Lanes")
            .DeactivateOnExit<Moonburn>();
        ComponentCondition<EighthBeats>(id + 0x20, 1.9f, c => c.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<EighthBeats>();
    }

    private void FunkyFloorSnap(uint id, float delay)
    {
        Cast(id, AID.FunkyFloor, delay, 2.5f);
        ComponentCondition<FunkyFloor>(id + 0x10, 0.5f, c => c.NumCasts >= 6, "Floor 1");
        Snap(id + 0x100, 9.5f, true);
        ComponentCondition<FunkyFloor>(id + 0x200, 2.3f, c => c.NumCasts >= 11, "Floor end");
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1019, NameID = 13778, Incomplete = true)]
public class RM05NDancingGreen(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
