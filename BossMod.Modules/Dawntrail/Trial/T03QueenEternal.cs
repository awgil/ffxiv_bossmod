namespace BossMod.Dawntrail.Trial.T03QueenEternal;

public enum OID : uint
{
    Boss = 0x41C0, // R22.0
    Helper = 0x233C,
    WaltzCaster = 0x41C1, // R0.5
    RuthlessRegaliaCaster = 0x41C2, // R0.5
    WaltzBait = 0x41C4, // R1.0
    AutoAttacker = 0x4477, // R1.0
}

public enum AID : uint
{
    AutoAttack = 36656, // AutoAttacker->player, no cast, single-target

    LegitimateForceLL = 36638, // Boss->self, 8.0s cast, range 60 width 30 rect, L -> L
    LegitimateForceLR = 36639, // Boss->self, 8.0s cast, range 60 width 30 rect, L -> R
    LegitimateForceRR = 36640, // Boss->self, 8.0s cast, range 60 width 30 rect, R -> R
    LegitimateForceRL = 36641, // Boss->self, 8.0s cast, range 60 width 30 rect, R -> L
    LegitimateForceL = 36642, // Boss->self, no cast, range 60 width 30 rect, left second
    LegitimateForceR = 36643, // Boss->self, no cast, range 60 width 30 rect, right second

    AethertitheVisual = 36604, // Boss->self, 3.0s cast, single-target
    AethertitheRaidwide = 36605, // Helper->self, no cast, range 100 circle
    Aethertithe1 = 36657, // Boss->self, no cast, range 100 70-degree cone, west
    Aethertithe2 = 36658, // Boss->self, no cast, range 100 70-degree cone, center
    Aethertithe3 = 36659, // Boss->self, no cast, range 100 70-degree cone, east

    Coronation = 36629, // Boss->self, 3.0s cast, single-target
    WaltzOfTheRegaliaVisual = 36631, // WaltzCaster->self, no cast, single-target
    WaltzOfTheRegalia = 36632, // Helper->self, 1.0s cast, range 14 width 4 rect

    ProsecutionOfWar = 36602, // Boss->player, 5.0s cast, single-target tankbuster

    LockAndKey = 36630, // Boss->self, no cast, single-target

    VirtualShift1 = 36606, // Boss->self, 5.0s cast, range 100 circle, arena to X
    VirtualShift2 = 36607, // Boss->self, 5.0s cast, range 100 circle
    VirtualShift3 = 36608, // Boss->self, 5.0s cast, range 100 circle, arena to default

    RuthlessRegalia = 36634, // RuthlessRegaliaCaster->self, no cast, range 100 width 12 rect

    DownburstVisual = 36609, // Boss->self, 6.0s cast, single-target
    Downburst = 36610, // Helper->self, 6.0s cast, range 100 circle, knockback 10

    BrutalCrown = 36633, // WaltzCaster->self, 8.0s cast, range 5-60 donut

    PowerfulGustVisual = 36611, // Boss->self, 6.0s cast, single-target
    PowerfulGust = 36612, // Helper->self, 6.0s cast, range 60 width 60 rect, knockback 20 forward

    RoyalDomain = 36603, // Boss->location, 5.0s cast, range 100 circle

    Castellation = 36613, // Boss->self, 3.0s cast, single-target
    Besiegement1 = 36614, // Helper->self, no cast, range 60 width 4 rect
    Besiegement2 = 36615, // Helper->self, no cast, range 60 width 8 rect
    Besiegement3 = 36616, // Helper->self, no cast, range 60 width 10 rect
    Besiegement4 = 36617, // Helper->self, no cast, range 60 width 12 rect
    Besiegement5 = 36618, // Helper->self, no cast, range 60 width 18 rect

    AbsoluteAuthorityVisual = 36619, // Boss->self, no cast, single-target
    AbsoluteAuthorityRaidwide1 = 39531, // Boss->self, 19.0s cast, range 100 circle
    AbsoluteAuthorityRaidwide2 = 36620, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityRaidwide3 = 36621, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityRaidwide4 = 36627, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityRaidwide5 = 36628, // Helper->self, no cast, range 100 circle
    AbsoluteAuthorityCircle = 36622, // Helper->self, 3.8s cast, range 8 circle
    AbsoluteAuthorityGaze = 36624, // Helper->players, no cast, range 100 circle
    AbsoluteAuthorityFlare = 39518, // Helper->players, no cast, range 100 circle
    AbsoluteAuthorityDoritoStack1 = 39519, // Helper->player, no cast, single-target
    AbsoluteAuthorityDoritoStack2 = 39520, // Helper->player, no cast, single-target

    DivideAndConquerVisual = 36636, // Boss->self, 5.0s cast, single-target
    DivideAndConquer = 36637, // Helper->self, no cast, range 100 width 5 rect

    MorningStars = 39134, // Helper->player, no cast, single-target

    DynasticDiademVisual = 40058, // Boss->self, 5.0s cast, single-target
    DynasticDiadem = 40059, // Helper->self, 5.0s cast, range 6-70 donut

    RoyalBanishmentVisual = 36644, // Boss->self, 3.0s cast, single-target
    RoyalBanishmentRaidwide = 36645, // Helper->self, no cast, range 100 circle
    RoyalBanishment = 36647, // Helper->self, 3.5s cast, range 100 30-degree cone
}

public enum SID : uint
{
    AuthoritysGaze = 3815,
    GravitationalAnomaly = 3814,
    AuthoritysHold = 4130,
}

public enum IconID : uint
{
    DoritoStack = 55,
    AccelerationBomb = 75,
    Flare = 327,
    LineBaits = 521,
}

class ArenaTransitions(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00040004)
            SetArena(new ArenaBoundsSquare(20), new(100, 100));
        else if (index == 0x00 && state == 0x00020001)
            SetArena(T03QueenEternal.GetXArena(), T03QueenEternal.XArenaCenter);
        else if (index == 0x02 && state == 0x00020001)
            SetArena(T03QueenEternal.SplitPlatforms, T03QueenEternal.SplitArenaCenter);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x2BFE)
            SetArena(new ArenaBoundsRect(20, 15), new(100, 105));
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}

class FloatingBounds(BossModule module) : BossComponent(module)
{
    private BitMask _floating;

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.GravitationalAnomaly && Raid.TryFindSlot(actor, out var slot))
            _floating.Set(slot);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.GravitationalAnomaly && Raid.TryFindSlot(actor, out var slot))
            _floating.Clear(slot);
    }

    public override void Update()
    {
        if (Arena.Center != T03QueenEternal.SplitArenaCenter)
            return;
        var floating = _floating.Any();
        if (floating != Arena.Bounds is ArenaBoundsRect)
            Arena.Bounds = floating ? new ArenaBoundsRect(12, 8) : T03QueenEternal.SplitPlatforms;
    }
}

class GravityGap(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(8, 4, 8);
    private DateTime _end;
    private int _numApplications;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_end != default && _end <= WorldState.FutureTime(6))
            yield return new(_shape, T03QueenEternal.SplitArenaCenter, default, _end);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_end != default && _end <= WorldState.FutureTime(6))
            hints.AddForbiddenZone(Sdf.Discrete(p => ~T03QueenEternal.SplitPlatforms.Contains(p - T03QueenEternal.SplitArenaCenter)), _end);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.GravitationalAnomaly && _end == default)
            _end = WorldState.FutureTime(_numApplications++ == 0 ? 21.2f : 15f);
    }

    public override void Update()
    {
        if (_end != default && WorldState.CurrentTime > _end.AddSeconds(1))
            _end = default;
    }
}

class Besiegement(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly (float X, float Width)[] _lanesA = [(82, 4), (90, 8), (101, 10), (114, 12)];
    private static readonly (float X, float Width)[] _lanesB = [(84, 8), (94, 8), (105, 10), (116, 8)];
    private static readonly (float X, float Width)[] _lanesC = [(84, 8), (95, 10), (111, 18)];
    private static readonly (float X, float Width)[] _lanesD = [(85, 10), (101, 18), (116, 8)];

    public readonly List<AOEInstance> AOEs = [];
    private int _layoutsSeen;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 3)
            return;

        var second = _layoutsSeen == 1;
        var lanes = state switch
        {
            0x00200010 => second ? _lanesB : _lanesA,
            0x00800040 => second ? _lanesA : _lanesB,
            0x02000100 when !second => _lanesC,
            0x08000400 when !second => _lanesD,
            _ => null
        };
        if (lanes == null)
            return;

        ++_layoutsSeen;
        var activation = WorldState.FutureTime(6.6f);
        foreach (var (x, width) in lanes)
            AOEs.Add(new(new AOEShapeRect(60, width * 0.5f), new WPos(x, 80), default, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is >= AID.Besiegement1 and <= AID.Besiegement5)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}

class Aethertithe(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0)
            return;
        Angle? dir = state switch
        {
            0x04000100 => -55.Degrees(),
            0x08000100 => 0.Degrees(),
            0x10000100 => 55.Degrees(),
            _ => null
        };
        if (dir != null)
            _aoe = new(new AOEShapeCone(100, 35.Degrees()), Module.PrimaryActor.Position, dir.Value, WorldState.FutureTime(5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aethertithe1 or AID.Aethertithe2 or AID.Aethertithe3)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}

class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeRect _shape = new(60, 15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<Besiegement>() is { AOEs.Count: > 0 })
            return [];
        return AOEs.Take(1);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dx = (AID)spell.Action.ID switch
        {
            AID.LegitimateForceLL or AID.LegitimateForceLR => -15,
            AID.LegitimateForceRR or AID.LegitimateForceRL => +15,
            _ => 0
        };
        if (dx == 0)
            return;

        var secondDx = (AID)spell.Action.ID switch
        {
            AID.LegitimateForceLL or AID.LegitimateForceRL => -15,
            AID.LegitimateForceRR or AID.LegitimateForceLR => +15,
            _ => 0
        };

        AOEs.Add(new(_shape, caster.Position + new WDir(dx, 0), default, Module.CastFinishAt(spell)));
        AOEs.Add(new(_shape, caster.Position + new WDir(secondDx, 0), default, Module.CastFinishAt(spell, 3.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LegitimateForceLL or AID.LegitimateForceLR or AID.LegitimateForceRR or AID.LegitimateForceRL
            or AID.LegitimateForceR or AID.LegitimateForceL)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}

class WaltzMarkers(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, DateTime> _markers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (id, activation) in _markers)
            if (WorldState.Actors.Find(id) is { } marker)
                yield return new(new AOEShapeCircle(7.3f), marker.Position, default, activation, Risky: activation <= WorldState.FutureTime(2.5f));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D7 && (OID)actor.OID == OID.WaltzBait)
            _markers[actor.InstanceID] = WorldState.FutureTime(7);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.WaltzOfTheRegaliaVisual)
            return;
        var id = _markers.Keys.FirstOrDefault(m => WorldState.Actors.Find(m)?.Position.AlmostEqual(caster.Position, 0.5f) ?? false);
        if (id != 0)
        {
            ++NumCasts;
            _markers.Remove(id);
        }
    }

    public override void OnActorDestroyed(Actor actor) => _markers.Remove(actor.InstanceID);
}

class WaltzOfTheRegalia(BossModule module) : Components.StandardAOEs(module, AID.WaltzOfTheRegalia, new AOEShapeRect(14, 2));

class RuthlessRegalia(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(100, 6);
    private (Actor source, DateTime activation)? _cast;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_cast is { } s)
            yield return new(_shape, s.source.Position, s.source.Rotation, s.activation);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.RuthlessRegaliaCaster && id == 0x11D2)
            _cast = (actor, WorldState.FutureTime(11.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RuthlessRegalia)
            _cast = null;
    }
}

class AbsoluteAuthorityCircle(BossModule module) : Components.StandardAOEs(module, AID.AbsoluteAuthorityCircle, 8);
class AbsoluteAuthorityFlare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(12), (uint)IconID.Flare, AID.AbsoluteAuthorityFlare, 6, centerAtTarget: true);
class AbsoluteAuthorityDorito(BossModule module) : Components.StackTogether(module, (uint)IconID.DoritoStack, 5.2f);

class AuthoritysHold(BossModule module) : Components.StayMove(module, 3)
{
    public override void Update()
    {
        foreach (var (slot, player) in Raid.WithSlot())
            PlayerStates[slot] = player.FindStatus(SID.AuthoritysHold) is { } hold ? new(Requirement.Stay, hold.ExpireAt) : default;
    }
}

class AuthoritysGaze(BossModule module) : Components.GenericGaze(module)
{
    private readonly Dictionary<ulong, DateTime> _carriers = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        foreach (var (id, expires) in _carriers)
            if (id != actor.InstanceID && expires <= WorldState.FutureTime(10) && WorldState.Actors.Find(id) is { } carrier)
                yield return new(carrier.Position, expires);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.AuthoritysGaze)
            _carriers[actor.InstanceID] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.AuthoritysGaze)
            _carriers.Remove(actor.InstanceID);
    }
}

class DivideAndConquer(BossModule module) : Components.GenericBaitAway(module, AID.DivideAndConquer)
{
    private static readonly AOEShapeRect _shape = new(100, 2.5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LineBaits && WorldState.Actors.Find(targetID) is { } target)
            CurrentBaits.Add(new(Module.PrimaryActor, target, _shape, WorldState.FutureTime(3)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaits)
            if (b.Target != actor)
                hints.AddForbiddenZone(b.Shape, b.Source.Position, b.Rotation, b.Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction)
            return;
        ++NumCasts;
        if (CurrentBaits.Count > 0)
            CurrentBaits.RemoveAt(0);
    }
}

class PlatformKnockback(BossModule module, AID aid, float distance, Components.Knockback.Kind kind = Components.Knockback.Kind.AwayFromOrigin) : Components.KnockbackFromCastTarget(module, aid, distance, kind: kind);

class DownburstKB(BossModule module) : PlatformKnockback(module, AID.Downburst, 10);
class PowerfulGustKB(BossModule module) : PlatformKnockback(module, AID.PowerfulGust, 20, Kind.DirForward);
class ProsecutionOfWar(BossModule module) : Components.SingleTargetCast(module, AID.ProsecutionOfWar);
class VirtualShiftRoyalDomain(BossModule module) : Components.RaidwideCast(module, AID.VirtualShift1, "Virtual shift + raidwide");
class VirtualShift2(BossModule module) : Components.RaidwideCast(module, AID.VirtualShift2);
class VirtualShift3(BossModule module) : Components.RaidwideCast(module, AID.VirtualShift3);
class RoyalDomain(BossModule module) : Components.RaidwideCast(module, AID.RoyalDomain);
class BrutalCrown(BossModule module) : Components.StandardAOEs(module, AID.BrutalCrown, new AOEShapeDonut(5, 60));
class DynasticDiadem(BossModule module) : Components.StandardAOEs(module, AID.DynasticDiadem, new AOEShapeDonut(6, 70));
class RoyalBanishment(BossModule module) : Components.StandardAOEs(module, AID.RoyalBanishment, new AOEShapeCone(100, 15.Degrees()));
class RoyalBanishmentRaidwide(BossModule module) : Components.RaidwideMultiHit(module, AID.RoyalBanishmentVisual, [AID.RoyalBanishmentRaidwide], 1, 1.7f, 5);
class AbsoluteAuthorityRaidwide(BossModule module) : Components.RaidwideCast(module, AID.AbsoluteAuthorityRaidwide1, "Multiple raidwides");
class AbsoluteAuthorityHits(BossModule module) : Components.RaidwideMultiHit(module, AID.AbsoluteAuthorityRaidwide1, [AID.AbsoluteAuthorityRaidwide2, AID.AbsoluteAuthorityRaidwide3, AID.AbsoluteAuthorityRaidwide4, AID.AbsoluteAuthorityRaidwide5], 6.7f, 1.2f, 11)
{
    protected override (int Hits, float Interval) Sequence(int index) => index == 0 ? (11, 1.2f) : (3, 1.4f);
}
class AethertitheTicks(BossModule module) : Components.RaidwideMultiHit(module, AID.AethertitheVisual, [AID.AethertitheRaidwide], 0.8f, 1.07f, 24);

class T03QueenEternalStates : StateMachineBuilder
{
    public T03QueenEternalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaTransitions>()
            .ActivateOnEnter<FloatingBounds>()
            .ActivateOnEnter<GravityGap>()
            .ActivateOnEnter<RoyalDomain>()
            .ActivateOnEnter<VirtualShiftRoyalDomain>()
            .ActivateOnEnter<VirtualShift2>()
            .ActivateOnEnter<VirtualShift3>()
            .ActivateOnEnter<AethertitheTicks>()
            .ActivateOnEnter<RoyalBanishmentRaidwide>()
            .ActivateOnEnter<AbsoluteAuthorityRaidwide>()
            .ActivateOnEnter<AbsoluteAuthorityHits>()
            .ActivateOnEnter<ProsecutionOfWar>()
            .ActivateOnEnter<Aethertithe>()
            .ActivateOnEnter<LegitimateForce>()
            .ActivateOnEnter<Besiegement>()
            .ActivateOnEnter<BrutalCrown>()
            .ActivateOnEnter<DynasticDiadem>()
            .ActivateOnEnter<RoyalBanishment>()
            .ActivateOnEnter<RuthlessRegalia>()
            .ActivateOnEnter<WaltzOfTheRegalia>()
            .ActivateOnEnter<WaltzMarkers>()
            .ActivateOnEnter<DivideAndConquer>()
            .ActivateOnEnter<DownburstKB>()
            .ActivateOnEnter<PowerfulGustKB>()
            .ActivateOnEnter<AbsoluteAuthorityCircle>()
            .ActivateOnEnter<AbsoluteAuthorityFlare>()
            .ActivateOnEnter<AbsoluteAuthorityDorito>()
            .ActivateOnEnter<AuthoritysHold>()
            .ActivateOnEnter<AuthoritysGaze>();
    }
}

// TODO: re-check Absolute Authority / Castellation edge cases if reports come in
[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 984, NameID = 13029)]
public sealed class T03QueenEternal(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20))
{
    private static readonly ArenaBoundsSquare Builder = new(20);
    public static readonly WPos XArenaCenter = new(100, 92.5f);
    public static readonly WPos SplitArenaCenter = new(100, 94);
    public static readonly ArenaBoundsCustom SplitPlatforms = GetSplitArena();

    public static ArenaBoundsCustom GetXArena()
    {
        var clipper = Builder.Clipper;
        const float margin = 0.3f;
        var north = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 12.5f - margin, 2.5f - margin).Select(c => c + new WDir(0, -10))]);
        var south = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 12.5f - margin, 2.5f - margin).Select(c => c + new WDir(0, 10))]);
        var arm1 = new PolygonClipper.Operand([.. CurveApprox.Rect(45.Degrees().ToDirection(), 15, 2.5f - margin)]);
        var arm2 = new PolygonClipper.Operand([.. CurveApprox.Rect((-45).Degrees().ToDirection(), 15, 2.5f - margin)]);
        return new(20, clipper.UnionAll(north, south, arm1, arm2));
    }

    public static ArenaBoundsCustom GetSplitArena()
    {
        var clipper = Builder.Clipper;
        var west = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 3.25f, 5.75f).Select(c => c + new WDir(-8, 0))]);
        var east = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 3.25f, 5.75f).Select(c => c + new WDir(8, 0))]);
        return new(20, clipper.Union(west, east));
    }
}
