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

class ArenaChanges(BossModule module) : BossComponent(module)
{
    private BitMask _gravity;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00040004u)
            SetArena(new ArenaBoundsSquare(20), new(100, 100));
        else if (state == 0x00020001u)
        {
            switch (index)
            {
                case 0x00:
                    SetArena(T03QueenEternal.GetXArena(), T03QueenEternal.XArenaCenter);
                    break;
                case 0x02:
                    SetArena(T03QueenEternal.GetSplitArena(), T03QueenEternal.SplitArenaCenter);
                    break;
            }
        }
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

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.GravitationalAnomaly && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _gravity.Set(slot);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.GravitationalAnomaly && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _gravity.Clear(slot);
    }

    public override void Update()
    {
        if (Arena.Center != T03QueenEternal.SplitArenaCenter)
            return;

        var countGravity = 0;
        foreach (var (slot, _) in Raid.WithSlot())
        {
            if (_gravity[slot])
                ++countGravity;
        }

        var isRect = Arena.Bounds is ArenaBoundsRect;
        if (countGravity != 0 && !isRect)
            SetArena(new ArenaBoundsRect(12, 8), T03QueenEternal.SplitArenaCenter);
        else if (countGravity == 0 && isRect)
            SetArena(T03QueenEternal.GetSplitArena(), T03QueenEternal.SplitArenaCenter);
    }
}

class Besiegement(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeRect[] Rects = [new(60, 2), new(60, 4), new(60, 5), new(60, 6), new(60, 9)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03)
            return;

        (int, float)[] aoes = [];
        var check = NumCasts != 1;

        switch (state)
        {
            case 0x00200010u:
                aoes = check ? [(3, 114), (1, 90), (2, 101), (0, 82)] : [(1, 116), (1, 94), (2, 105), (1, 84)];
                break;
            case 0x02000100u:
                aoes = check ? [(2, 95), (4, 111), (1, 84)] : [];
                break;
            case 0x08000400u:
                aoes = check ? [(1, 116), (4, 101), (2, 85)] : [];
                break;
            case 0x00800040u:
                aoes = check ? [(1, 116), (2, 105), (1, 94), (1, 84)] : [(3, 114), (2, 101), (1, 90), (0, 82)];
                break;
        }

        if (aoes.Length == 0)
            return;

        ++NumCasts;
        var act = WorldState.FutureTime(6.6f);
        foreach (var (shapeIndex, x) in aoes)
            AOEs.Add(new(Rects[shapeIndex], new WPos(x, 80), 90.Degrees(), act));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count == 0)
            return;
        if ((AID)spell.Action.ID is AID.Besiegement1 or AID.Besiegement2 or AID.Besiegement3 or AID.Besiegement4 or AID.Besiegement5)
            AOEs.Clear();
    }
}

class Aethertithe(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone Cone = new(100, 35.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x00)
            return;
        Angle? angle = state switch
        {
            0x04000100u => -55.Degrees(),
            0x08000100u => default(Angle),
            0x10000100u => 55.Degrees(),
            _ => null
        };
        if (angle is Angle rot)
            _aoe = [new(Cone, Module.PrimaryActor.Position, rot, WorldState.FutureTime(5))];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aethertithe1 or AID.Aethertithe2 or AID.Aethertithe3)
            _aoe = [];
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

class WaltzOfTheRegaliaBait(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(MathF.Sqrt(212) * 0.5f);
    private readonly List<(Actor actor, DateTime activation)> _targets = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in _targets)
            yield return new(Circle, t.actor.Position, default, t.activation);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D7 && actor.OID == (uint)OID.WaltzBait)
            _targets.Add((actor, WorldState.FutureTime(7)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.WaltzOfTheRegaliaVisual)
            return;
        var pos = caster.Position;
        for (var i = 0; i < _targets.Count; ++i)
        {
            if (_targets[i].actor.Position.AlmostEqual(pos, 0.5f))
            {
                _targets.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID != (uint)OID.WaltzBait)
            return;
        for (var i = 0; i < _targets.Count; ++i)
        {
            if (_targets[i].actor == actor)
            {
                _targets.RemoveAt(i);
                return;
            }
        }
    }
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
class AbsoluteAuthorityDorito(BossModule module) : Components.StackWithIcon(module, (uint)IconID.DoritoStack, AID.AbsoluteAuthorityDoritoStack1, 3, 5.1f, 8, 8);

class AuthoritysHold(BossModule module) : Components.StayMove(module, 3)
{
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.AuthoritysHold && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.AuthoritysHold && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class AuthoritysGaze(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_affected.Count == 0 || WorldState.CurrentTime < _activation.AddSeconds(-10))
            yield break;
        foreach (var a in _affected)
        {
            if (a != actor)
                yield return new(a.Position, _activation);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.AuthoritysGaze)
        {
            _activation = status.ExpireAt;
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.AuthoritysGaze)
            _affected.Remove(actor);
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction)
            return;
        ++NumCasts;
        if (CurrentBaits.Count > 0)
            CurrentBaits.RemoveAt(0);
    }
}

class DownburstKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Downburst, 10, stopAtWall: true);
class PowerfulGustKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.PowerfulGust, 20, kind: Components.Knockback.Kind.DirForward, stopAtWall: true);
class ProsecutionOfWar(BossModule module) : Components.SingleTargetCast(module, AID.ProsecutionOfWar);
class VirtualShiftRoyalDomain(BossModule module) : Components.RaidwideCast(module, AID.VirtualShift1, "Virtual shift + raidwide");
class VirtualShift2(BossModule module) : Components.RaidwideCast(module, AID.VirtualShift2);
class VirtualShift3(BossModule module) : Components.RaidwideCast(module, AID.VirtualShift3);
class RoyalDomain(BossModule module) : Components.RaidwideCast(module, AID.RoyalDomain);
class BrutalCrown(BossModule module) : Components.StandardAOEs(module, AID.BrutalCrown, new AOEShapeDonut(5, 60));
class DynasticDiadem(BossModule module) : Components.StandardAOEs(module, AID.DynasticDiadem, new AOEShapeDonut(6, 70));
class RoyalBanishment(BossModule module) : Components.StandardAOEs(module, AID.RoyalBanishment, new AOEShapeCone(100, 15.Degrees()));
class RoyalBanishmentRaidwide(BossModule module) : Components.RaidwideCast(module, AID.RoyalBanishmentVisual, "Multiple raidwides");
class AbsoluteAuthorityRaidwide(BossModule module) : Components.RaidwideCast(module, AID.AbsoluteAuthorityRaidwide1, "Multiple raidwides");

class T03QueenEternalStates : StateMachineBuilder
{
    public T03QueenEternalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Besiegement>()
            .ActivateOnEnter<LegitimateForce>()
            .ActivateOnEnter<Aethertithe>()
            .ActivateOnEnter<WaltzOfTheRegalia>()
            .ActivateOnEnter<WaltzOfTheRegaliaBait>()
            .ActivateOnEnter<RuthlessRegalia>()
            .ActivateOnEnter<ProsecutionOfWar>()
            .ActivateOnEnter<VirtualShiftRoyalDomain>()
            .ActivateOnEnter<VirtualShift2>()
            .ActivateOnEnter<VirtualShift3>()
            .ActivateOnEnter<RoyalDomain>()
            .ActivateOnEnter<AbsoluteAuthorityRaidwide>()
            .ActivateOnEnter<DownburstKB>()
            .ActivateOnEnter<PowerfulGustKB>()
            .ActivateOnEnter<BrutalCrown>()
            .ActivateOnEnter<AbsoluteAuthorityCircle>()
            .ActivateOnEnter<AuthoritysGaze>()
            .ActivateOnEnter<AuthoritysHold>()
            .ActivateOnEnter<AbsoluteAuthorityDorito>()
            .ActivateOnEnter<AbsoluteAuthorityFlare>()
            .ActivateOnEnter<DynasticDiadem>()
            .ActivateOnEnter<DivideAndConquer>()
            .ActivateOnEnter<RoyalBanishment>()
            .ActivateOnEnter<RoyalBanishmentRaidwide>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 984, NameID = 13029)] // Incomplete cleared; TODO: re-check Absolute Authority / Castellation edge cases if reports come in
public sealed class T03QueenEternal(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20))
{
    private static readonly ArenaBoundsSquare Builder = new(20);
    public static readonly WPos XArenaCenter = new(100, 92.5f);
    public static readonly WPos SplitArenaCenter = new(100, 94);

    public static ArenaBoundsCustom GetXArena()
    {
        var clipper = Builder.Clipper;
        var north = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 12.5f, 2.5f).Select(c => c + new WDir(0, -10))]);
        var south = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 12.5f, 2.5f).Select(c => c + new WDir(0, 10))]);
        var arm1 = new PolygonClipper.Operand([.. CurveApprox.Rect(45.Degrees().ToDirection(), 15, 2.5f)]);
        var arm2 = new PolygonClipper.Operand([.. CurveApprox.Rect((-45).Degrees().ToDirection(), 15, 2.5f)]);
        return new(20, clipper.UnionAll(north, south, arm1, arm2));
    }

    public static ArenaBoundsCustom GetSplitArena()
    {
        var clipper = Builder.Clipper;
        var west = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 4, 8).Select(c => c + new WDir(-8, 0))]);
        var east = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 4, 8).Select(c => c + new WDir(8, 0))]);
        return new(20, clipper.Union(west, east));
    }
}
