namespace BossMod.Endwalker.Variant.V03Aloalo.V015Loquloqui;

public enum OID : uint
{
    Boss = 0x3FD6, // R6.480-16.200, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    UolosapaLoqua = 0x3FD7, // R0.800-2.400, x?, birds (Rush lines)
    RepurubaLoqua = 0x3FD8, // R0.960-2.880, x?, newts (Turnabout circles)
    PetalMarkerA = 0x3FD9, // R0.050, x?, O Petals Unfurl landing spots
    PetalMarkerB = 0x3FDA, // R0.050, x?, O Petals Unfurl wall buds
    PetalTarget = 0x3FDB, // R1.000, x?, dummy tether target for petal markers
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 34747, // Boss->location, no cast, single-target

    LongLostLight = 34748, // Boss->self, 5.0s cast, range 55 circle, raidwide (+ outer danger zone on first cast)
    ProtectiveWill = 34750, // Boss->player, 5.0s cast, single-target, tankbuster
    LandWave = 34749, // Boss->self, 6.0s cast, range 25 width 40 rect, half-arena cleave

    SummoningRite = 34751, // Boss->self, 4.0s cast, single-target, spawn birds/newts
    OLifeFlourish = 34756, // Boss->self, 5.0s cast, single-target, tether adds to enlarge AoEs
    Rush = 34752, // UolosapaLoqua->self, 8.0s cast, range 35 width 2 rect
    RushFlourish = 34753, // UolosapaLoqua->self, 8.0s cast, range 35 width 10 rect (the enlarged variant)
    Turnabout = 34754, // RepurubaLoqua->self, 8.0s cast, range 4 circle
    TurnaboutFlourish = 34755, // RepurubaLoqua->self, 8.0s cast, range 4 circle (enlarged)

    OPetalsUnfurl = 34757, // Boss->self, 5.0s cast, single-target
    PetalTravelShort = 35892, // Helper->self, 8.0s cast, single-target
    PetalTravelLong = 35893, // Helper->self, 8.0s cast, single-target
    PliantPetals = 34758, // Helper->self, 2.0s cast, range 15 circle

    OIsleBloom = 34759, // Boss->self, 5.0s cast, single-target
    BrilliantBlossoms = 34760, // Helper->self, no cast, range 10 width 10 rect

    OSkyBeMine = 34761, // Boss->self, 5.0s cast, single-target, grow
    SanctuaryVisual = 34762, // Boss->self, 5.0s cast, range 30 circle
    Sanctuary = 36152, // Helper->self, 5.7s cast, range 30 circle, proximity
    StirringOfSpirits = 34763, // Boss->self, 5.0s cast, range 30 circle
    Shockwave = 34764, // Boss->self, no cast, range 30 circle, knockback
    IslebloomLight = 34765, // Helper->self, 5.0s cast, range 7 circle
}

public enum TetherID : uint
{
    Flourish = 259, // bird/newt/petal -> ?
}

public static class Arenas
{
    public static readonly ArenaBoundsRect Initial = new(25, 20);
    public static readonly ArenaBoundsRect Shrunk = new(20, 15);
}

class LongLostLight(BossModule module) : Components.RaidwideCast(module, AID.LongLostLight)
{
    private bool _shrunk;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (!_shrunk && (AID)spell.Action.ID == AID.LongLostLight)
        {
            _shrunk = true;
            Module.Arena.Bounds = Arenas.Shrunk;
        }
    }
}

class ProtectiveWill(BossModule module) : Components.SingleTargetCast(module, AID.ProtectiveWill);
class LandWave(BossModule module) : Components.StandardAOEs(module, AID.LandWave, new AOEShapeRect(25, 20));

class Rush(BossModule module) : Components.StandardAOEs(module, AID.Rush, new AOEShapeRect(35, 1));
class RushFlourish(BossModule module) : Components.StandardAOEs(module, AID.RushFlourish, new AOEShapeRect(35, 5));
class Turnabout(BossModule module) : Components.StandardAOEs(module, AID.Turnabout, 4);
class TurnaboutFlourish(BossModule module) : Components.StandardAOEs(module, AID.TurnaboutFlourish, 12);

class PliantPetals(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Origin, DateTime Activation, int Wave)> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;

        var known = _aoes.Where(a => a.Wave >= 0).ToList();
        var imminentWave = known.Count > 0 ? known.Min(a => a.Wave) : -1;
        var hasSubsequent = known.Any(a => a.Wave > imminentWave);
        foreach (var a in _aoes)
        {
            var isImminent = known.Count == 0 || a.Wave >= 0 && a.Wave == imminentWave;
            var solid = isImminent && hasSubsequent;
            var risky = !hasSubsequent || isImminent;
            yield return new(_shape, a.Origin, default, a.Activation, solid ? ArenaColor.Danger : ArenaColor.AOE, Risky: risky);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OPetalsUnfurl:
                _aoes.Clear();
                break;
            case AID.PetalTravelShort:
            case AID.PetalTravelLong:
                RefreshFromTravels();
                break;
            case AID.PliantPetals:
                Upsert(spell.LocXZ, Module.CastFinishAt(spell), wave: null);
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((OID)source.OID != OID.PetalMarkerA || tether.ID != (uint)TetherID.Flourish)
            return;
        Upsert(source.Position, WorldState.FutureTime(20), -1);
        ReclassifyByDistance();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.PliantPetals)
            return;

        var origin = caster.CastInfo?.LocXZ ?? caster.Position;
        if (origin == default)
            origin = caster.Position;

        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(origin, 3f));
        if (i < 0 && _aoes.Count > 0)
            i = _aoes.Select((a, idx) => (a, idx)).MinBy(t => (t.a.Origin - origin).LengthSq()).idx;
        if (i >= 0)
            _aoes.RemoveAt(i);
    }

    private void RefreshFromTravels()
    {
        if (_aoes.Count == 0)
            return;

        var travels = new List<(WPos Wall, ActorCastInfo Cast)>();
        foreach (var a in WorldState.Actors)
        {
            if (a.CastInfo == null)
                continue;
            if ((AID)a.CastInfo.Action.ID is not (AID.PetalTravelShort or AID.PetalTravelLong))
                continue;
            var wall = a.CastInfo.LocXZ;
            if (wall == default)
                wall = a.Position;
            travels.Add((wall, a.CastInfo));
        }

        if (travels.Count == 0)
        {
            ReclassifyByDistance();
            return;
        }

        // wall <-> landing pairs, ranked by length
        var usedLanding = new bool[_aoes.Count];
        var usedTravel = new bool[travels.Count];
        var pairs = new List<(int Landing, int Travel, float DistSq)>();
        var n = Math.Min(_aoes.Count, travels.Count);
        for (var k = 0; k < n; ++k)
        {
            var bestD = float.MaxValue;
            var bestL = -1;
            var bestT = -1;
            for (var li = 0; li < _aoes.Count; ++li)
            {
                if (usedLanding[li])
                    continue;
                for (var ti = 0; ti < travels.Count; ++ti)
                {
                    if (usedTravel[ti])
                        continue;
                    var d = (_aoes[li].Origin - travels[ti].Wall).LengthSq();
                    if (d < bestD)
                    {
                        bestD = d;
                        bestL = li;
                        bestT = ti;
                    }
                }
            }
            if (bestL < 0)
                break;
            usedLanding[bestL] = usedTravel[bestT] = true;
            pairs.Add((bestL, bestT, bestD));
        }

        pairs.Sort((a, b) => a.DistSq.CompareTo(b.DistSq));
        var staggered = pairs.Count >= 4;
        var half = pairs.Count / 2;
        for (var r = 0; r < pairs.Count; ++r)
        {
            var (li, ti, _) = pairs[r];
            var wave = staggered && r >= half ? 1 : 0;
            var delay = wave == 0 ? 2.6f : 6.5f;
            var prev = _aoes[li];
            _aoes[li] = (prev.Origin, Module.CastFinishAt(travels[ti].Cast, delay), wave);
        }
    }

    private void ReclassifyByDistance()
    {
        if (_aoes.Count == 0)
            return;

        if (_aoes.Count < 4)
        {
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var p = _aoes[i];
                _aoes[i] = (p.Origin, p.Activation, 0);
            }
            return;
        }

        // only add tethered ones since there's a bunch inactive at all times
        var walls = Module.Enemies(OID.PetalMarkerB).Where(w => w.Tether.ID == (uint)TetherID.Flourish).Select(w => w.Position).ToList();
        if (walls.Count == 0)
            return;

        var usedL = new bool[_aoes.Count];
        var usedW = new bool[walls.Count];
        var distSq = new float[_aoes.Count];
        Array.Fill(distSq, float.MaxValue);
        var n = Math.Min(_aoes.Count, walls.Count);
        for (var k = 0; k < n; ++k)
        {
            var bestD = float.MaxValue;
            var bestL = -1;
            var bestW = -1;
            for (var li = 0; li < _aoes.Count; ++li)
            {
                if (usedL[li])
                    continue;
                for (var wi = 0; wi < walls.Count; ++wi)
                {
                    if (usedW[wi])
                        continue;
                    var d = (_aoes[li].Origin - walls[wi]).LengthSq();
                    if (d < bestD)
                    {
                        bestD = d;
                        bestL = li;
                        bestW = wi;
                    }
                }
            }
            if (bestL < 0)
                break;
            usedL[bestL] = usedW[bestW] = true;
            distSq[bestL] = bestD;
        }

        var order = Enumerable.Range(0, _aoes.Count).OrderBy(i => distSq[i]).ToArray();
        var half = order.Length / 2;
        for (var r = 0; r < order.Length; ++r)
        {
            var i = order[r];
            var p = _aoes[i];
            _aoes[i] = (p.Origin, p.Activation, r < half ? 0 : 1);
        }
    }

    private void Upsert(WPos origin, DateTime activation, int? wave)
    {
        if (origin == default)
            return;
        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(origin, 1.5f));
        if (i >= 0)
        {
            var prev = _aoes[i];
            _aoes[i] = (origin,
                activation != default ? activation : prev.Activation,
                wave ?? prev.Wave);
        }
        else
            _aoes.Add((origin, activation, wave ?? -1));
    }
}

class BrilliantBlossoms(BossModule module) : Components.GenericAOEs(module, AID.BrilliantBlossoms)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    // relative to arena center
    private static readonly WDir[] _pattern =
    [
        new(-15, -10), new(-5, -10), new(5, -10), new(15, -10),
        new(15, 0), new(5, 0), new(-5, 0), new(-15, 0),
        new(-15, 10), new(-5, 10), new(5, 10), new(15, 10),
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imminent = _aoes.Count > 0 ? _aoes[0].Activation.AddSeconds(1.0f) : default;
        foreach (var a in _aoes)
            yield return a with { Color = a.Activation <= imminent ? ArenaColor.Danger : ArenaColor.AOE };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.OIsleBloom)
            return;

        _aoes.Clear();
        var t = Module.CastFinishAt(spell, 11.5f);
        for (var i = 0; i < _pattern.Length; ++i)
        {
            _aoes.Add(new(_shape, Module.Center + _pattern[i], default, t));
            t = t.AddSeconds(0.5f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.BrilliantBlossoms)
            return;

        ++NumCasts;
        var origin = caster.Position;
        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(origin, 2));
        if (i >= 0)
            _aoes.RemoveAt(i);
        else if (_aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class Sanctuary(BossModule module) : Components.ProximityAOEs(module, AID.Sanctuary, 18);

class IslebloomLight(BossModule module) : Components.StandardAOEs(module, AID.IslebloomLight, 7);

// TODO: better AI support. I can get it to adjust on the first shockwave but subsequent it gives up cause my inverted rect forbids the whole arena.
class Shockwave(BossModule module) : Components.Knockback(module, AID.Shockwave, stopAtWall: false)
{
    private DateTime _next;
    private int _left;
    private const float Distance = 8;
    private const float StandOffset = 2f;
    private const float WallMargin = 2.5f;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_left > 0 && _next != default)
            yield return new(Module.Center, Distance, _next);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
        => !Module.InBounds(pos) || (Module.FindComponent<IslebloomLight>()?.ActiveAOEs(slot, actor).Any(a => a.Check(pos)) ?? false);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_left <= 0 || _next == default || IsImmune(slot, _next))
            return;

        var origin = Module.Center;
        var puddles = Module.FindComponent<IslebloomLight>()?.ActiveAOEs(slot, actor)
            .Select(a => a.Origin).Distinct().ToArray() ?? [];
        var aim = SafeAim(origin, puddles);
        var stand = origin + aim * StandOffset;

        // soft hint only cause a hard makes the ai confused
        hints.GoalZones.Add(AIHints.GoalProximity(stand, 1.5f, 80));
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(stand, 1.5f), _next);
    }

    private WDir SafeAim(WPos origin, WPos[] puddles)
    {
        WDir best = new(1, 0); // prefer long axis of shrunk rect
        var bestScore = float.MinValue;
        for (var i = 0; i < 24; ++i)
        {
            var dir = (i * 15).Degrees().ToDirection();
            var dest = origin + dir * (StandOffset + Distance);
            var off = dest - Module.Center;
            var clear = Math.Min(Arenas.Shrunk.HalfWidth - WallMargin - Math.Abs(off.X),
                Arenas.Shrunk.HalfHeight - WallMargin - Math.Abs(off.Z));
            if (clear < 0)
                continue;
            foreach (var p in puddles)
                clear = Math.Min(clear, (dest - p).Length() - 7.5f);
            if (clear > bestScore)
            {
                bestScore = clear;
                best = dir;
            }
        }
        return best;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StirringOfSpirits)
        {
            _left = 4;
            NumCasts = 0;
            _next = Module.CastFinishAt(spell, 2.8f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.Shockwave || _left <= 0)
            return;

        ++NumCasts;
        --_left;
        _next = _left > 0 ? WorldState.FutureTime(4.5f) : default;
    }
}

class V015LoquloquiStates : StateMachineBuilder
{
    public V015LoquloquiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LongLostLight>()
            .ActivateOnEnter<ProtectiveWill>()
            .ActivateOnEnter<LandWave>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<RushFlourish>()
            .ActivateOnEnter<Turnabout>()
            .ActivateOnEnter<TurnaboutFlourish>()
            .ActivateOnEnter<PliantPetals>()
            .ActivateOnEnter<BrilliantBlossoms>()
            .ActivateOnEnter<Sanctuary>()
            .ActivateOnEnter<IslebloomLight>()
            .ActivateOnEnter<Shockwave>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12636)]
public class V015Loquloqui(WorldState ws, Actor primary) : BossModule(ws, primary, new(950, -860), Arenas.Initial);
