namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V03NPariOfPlenty;

public enum OID : uint
{
    Boss = 0x4A68, // R5.016, x?
    FieryBauble = 0x4A69, // R6.000, x?
    FlyingCarpet = 0x4A74, // R4.356, x?
    CapriciousChambermaid = 0x4A6A, // R0.600, x?
    SummonedSpirit = 0x4A6C, // R2.400, x1
    ChargeTargetHelper = 0x4A75, // R1.000, x6
    Whirlwind = 0x4A76, // R4.450, x0 (spawn during fight)
    LegendaryBird = 0x4A6B,
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    HeatBurst = 45515, // 4A68->self, 5.0s cast, range 60 circle
    Fireflight = 45422, // 4A68->self, 10.0s cast, single-target
    CarpetRide = 45423, // 4A68->location, no cast, single-target
    CarpetRide1 = 45424, // 233C->location, 2.0s cast, width 10 rect charge
    SunCirclet = 45446, // 4A68->self, 1.0s cast, range ?-60 donut

    CharmingBaubles1 = 45495, // 4A68->self, 3.0s cast, single-target
    CharmingBaubles2 = 45502, // Boss->self, 3.0s cast, single-target

    BurningGleam1 = 45498, // 4A69->self, 5.0s cast, range 40 width 10 cross
    BurningGleam2 = 46809, // FieryBauble->self, 6.0s cast, range 40 width 10 cross
    BurningGleamQuick = 45547, // FieryBauble->self, 1.0s cast, range 40 width 10 cross

    GaleForce = 45513, // 4A6A->self, 11.0s cast, range 15 circle
    GaleCannon = 45514,

    PredatorySwoop = 45510, // 4A6B->self, 5.0s cast, range 12 circle
    TranscendentFlight = 45512, // 4A6B->location, 3.0s cast, range 12 circle

    SpurningFlames = 45480, // 4A68->self, 7.0s cast, range 40 circle
    ImpassionedSparksCast = 45483, // 4A68->self, 5.0s cast, single-target
    ImpassionedSparks = 45484, // 4A68->self, no cast, single-target
    ImpassionedSparksShort = 45485, // 233C->self, 2.0s cast, single-target
    ImpassionedSparksLong = 45486, // 233C->self, 6.0s cast, range 8 circle
    ScouringScorn = 45489, // 4A68->self, 6.0s cast, range 40 circle

    LeftFireflightTwoNights = 45919, // 4A68->self, 9.0s cast, range 40 width 4 rect
    LeftFireflightThreeNights = 45460, // 4A68->self, 13.0s cast, range 40 width 4 rect
    RightFireflightTwoNights = 45918, // 4A68->self, 9.0s cast, range 40 width 4 rect
    RightFireflightThreeNights = 45459, // 4A68->self, 13.0s cast, range 40 width 4 rect
    WheelOfFireflight = 45458, // 4A68->self, no cast, range 40 ?-degree cone
    WheelOfFireflight1 = 45457, // 4A68->self, no cast, range 40 ?-degree cone
    WheelOfFireflight2 = 45456, // 4A68->self, no cast, range 40 ?-degree cone
    WheelOfFireflight3 = 45455, // 4A68->self, no cast, range 40 ?-degree cone

    StrongWindCast = 45507, // 4A6C->self, 8.0s cast, single-target
    StrongWind = 45508, // Helper->self, 8.0s cast, range 22 circle
    StrongWindCircle = 45509, // Helper->self, no cast, range 22 circle
    StrongWindPath = 46755, // Helper->self, 8.0s cast, range 40 width 20 rect

    ThievesWeave = 46753, // Boss->self, 4.0s cast, single-target
    Unravel = 45506, // FlyingCarpet->self, 3.0s cast, single-target
    CarpetConceal = 45504,
    CarpetShuffle = 45505,
}
public enum TetherID : uint
{
    RugCharge = 355, // Charge
}
public enum IconID : uint
{
    LeftTurnRight = 624, // Boss->self
    LeftTurnLeft = 625, // Boss->self
    RightTurnRight = 644, // Boss->self
    RightTurnLeft = 645, // Boss->self
}
public enum SID : uint
{
    VisibleStatus = 2056, // Boss->Boss/FieryBauble, extra=0x3EA/0x3D9/0x448/0x3D
}

class FireFlight(BossModule module) : Components.GenericAOEs(module)
{
    DateTime _start;
    readonly List<Path> _path = [];
    AOEInstance? _donut;

    class Path
    {
        public required Actor From;
        public required Actor To;
        public required DateTime Activation;
        public required DateTime Appeared;
        public WPos FromPos { get; private set; }
        public WPos ToPos { get; private set; }
        public bool IsBoss;

        bool _updated;

        public void Update(DateTime now, WPos bossPosition)
        {
            if (now <= Appeared || _updated)
                return;

            _updated = true;
            FromPos = From.Position;
            ToPos = To.Position;
            IsBoss = FromPos.AlmostEqual(bossPosition, 0.5f);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _path.Where(p => p.IsBoss).Select(MakeAOE).Concat(Utils.ZeroOrOne(_donut)).Take(3);

    AOEInstance MakeAOE(Path path)
    {
        var diff = path.ToPos - path.FromPos;
        var length = diff.Length();
        var rot = diff.ToAngle();
        return new(new AOEShapeRect(length, 5), path.FromPos, rot, path.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Fireflight:
                _start = Module.CastFinishAt(spell, 2.7f);
                break;
            case AID.SunCirclet:
                _path.Clear();
                _start = default;
                _donut = null;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.RugCharge)
        {
            if (WorldState.Actors.Find(tether.Target) is { } dest)
            {
                _path.Add(new Path()
                {
                    From = source,
                    To = dest,
                    Activation = _start.AddSeconds(_path.Count * 2.1f),
                    Appeared = WorldState.CurrentTime
                });
            }
        }
    }

    public override void Update()
    {
        var pathLen = 0;
        var prevPos = Module.PrimaryActor.Position;
        foreach (var p in _path)
        {
            p.Update(WorldState.CurrentTime, prevPos);
            if (p.IsBoss)
            {
                pathLen++;
                prevPos = p.ToPos;
            }
        }

        if (pathLen == 3 && _donut == null)
        {
            _donut = new(new AOEShapeDonut(8, 60), prevPos, default, _path[^1].Activation.AddSeconds(3.6f));
            _path.RemoveAll(p => !p.IsBoss);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetRide1)
        {
            if (_path.Count > 0)
                _path.RemoveAt(0);
        }
    }
}
class HeatBurst(BossModule module) : Components.RaidwideCast(module, AID.HeatBurst);
class CarpetRide(BossModule module) : Components.ChargeAOEs(module, AID.CarpetRide1, 5f);
class SunCirclet(BossModule module) : Components.StandardAOEs(module, AID.SunCirclet, new AOEShapeDonut(8, 60));
class BurningGleam(BossModule module) : Components.GroupedAOEs(module, [AID.BurningGleam1, AID.BurningGleam2], new AOEShapeCross(40, 5f));
class Unravel(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCross cross = new(40f, 5f);

    private readonly List<Actor> _carpets = [];
    private readonly List<Actor> _baubles = [];
    private readonly List<Actor> _unsafeCarpets = [];

    private static readonly WPos[] CarpetPositions =
    [
        new(-745, -800),
        new(-755, -800),
        new(-765, -800),
        new(-775, -800),
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var e in _aoes)
            yield return e with { Color = ArenaColor.AOE };
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.FlyingCarpet && !_carpets.Contains(actor))
            _carpets.Add(actor);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.FieryBauble && (SID)status.ID == SID.VisibleStatus && !_baubles.Contains(actor))
        {
            _baubles.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetConceal)
        {

            _unsafeCarpets.Clear();
            _aoes.Clear();

            foreach (var pos in CarpetPositions)
            {
                bool hasVisibleBauble = _baubles.Any(b => b.Position.AlmostEqual(pos, 1f));
                if (!hasVisibleBauble)
                    continue;

                var carpet = _carpets.OrderBy(c => (c.Position - pos).LengthSq()).FirstOrDefault();

                if (carpet != null && !_unsafeCarpets.Contains(carpet))
                    _unsafeCarpets.Add(carpet);
            }
        }
        if ((AID)spell.Action.ID is AID.Unravel)
        {
            _aoes.Clear();

            foreach (var carpet in _unsafeCarpets)
            {
                var snappedPos = CarpetPositions.OrderBy(p => (p - carpet.Position).LengthSq()).First();
                _aoes.Add(new(cross, snappedPos));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BurningGleamQuick)
        {
            _aoes.Clear();
            _baubles.Clear();
            _unsafeCarpets.Clear();
        }
    }
}

class GaleForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, AOEInstance aoe)> _aoes = [];
    private readonly AOEShapeCircle circ = new(15f);
    private bool _canDraw;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0 && _canDraw)
        {
            foreach (var (_, aoe) in _aoes.Take(2))
                yield return aoe with { Color = ArenaColor.AOE };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.GaleForce && NumCasts < 5)
        {
            _aoes.Add((caster, new(circ, caster.Position)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((OID)caster.OID is OID.CapriciousChambermaid)
        {
            _canDraw = true;
            _aoes.RemoveAll(a => a.caster.InstanceID == caster.InstanceID);
        }
        if ((AID)spell.Action.ID is AID.GaleForce)
        {
            NumCasts++;
            if (NumCasts >= 5)
            {
                _aoes.Clear();
            }
        }
    }
}
class PredatorySwoop(BossModule module) : Components.StandardAOEs(module, AID.PredatorySwoop, 12f);
class TranscendentFlight(BossModule module) : Components.StandardAOEs(module, AID.TranscendentFlight, 12f);
class SpurningFlames(BossModule module) : Components.RaidwideCast(module, AID.SpurningFlames);
class ImpassionedSparks(BossModule module) : Components.StandardAOEs(module, AID.ImpassionedSparksLong, new AOEShapeCircle(8f), 4);
class ScouringScorn(BossModule module) : Components.RaidwideCast(module, AID.ScouringScorn);
class StrongWindPath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect rect = new(40f, 23f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StrongWindPath)
        {
            _aoes.Add(new(rect, caster.Position, caster.Rotation));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Whirlwind)
            _aoes.RemoveAt(0);
    }
}
class StrongWind : Components.PersistentVoidzone
{
    private readonly List<Actor> _whirlwind = [];

    public StrongWind(BossModule module) : base(module, 23f, _ => [])
    {
        Sources = _ => _whirlwind;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Whirlwind && !actor.Position.AlmostEqual(Module.Center, 5))
            _whirlwind.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Whirlwind)
            _whirlwind.Remove(actor);
    }
}
class GroupedFireflights(BossModule module) : Components.GroupedAOEs(module, [AID.RightFireflightTwoNights, AID.RightFireflightThreeNights, AID.LeftFireflightTwoNights, AID.LeftFireflightThreeNights], new AOEShapeRect(40f, 2f));
class FireflightNights(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            yield return _aoes[0] with { Color = ArenaColor.AOE };
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WheelOfFireflight or AID.WheelOfFireflight1 or AID.WheelOfFireflight2 or AID.WheelOfFireflight3)
        {
            _aoes.RemoveAt(0);
        }
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is not ((uint)IconID.LeftTurnLeft or (uint)IconID.RightTurnRight or (uint)IconID.LeftTurnRight or (uint)IconID.RightTurnLeft))
            return;

        bool even = _aoes.Count % 2 == 0;
        bool reverseOnEven = iconID is (uint)IconID.LeftTurnLeft or (uint)IconID.RightTurnRight;

        var rot = (even == reverseOnEven) ? Module.PrimaryActor.Rotation - 180.Degrees() : Module.PrimaryActor.Rotation;
        _aoes.Add(new(new AOEShapeCone(40, 90.Degrees()), Module.PrimaryActor.Position, rot));
    }
}

class V03NPariOfPlentyStates : StateMachineBuilder
{
    public V03NPariOfPlentyStates(BossModule module) : base(module)
    {
        TrivialPhase()
        .ActivateOnEnter<FireFlight>()
        .ActivateOnEnter<HeatBurst>()
        .ActivateOnEnter<CarpetRide>()
        .ActivateOnEnter<SunCirclet>()
        .ActivateOnEnter<BurningGleam>()
        .ActivateOnEnter<GaleForce>()
        .ActivateOnEnter<PredatorySwoop>()
        .ActivateOnEnter<TranscendentFlight>()
        .ActivateOnEnter<SpurningFlames>()
        .ActivateOnEnter<ImpassionedSparks>()
        .ActivateOnEnter<ScouringScorn>()
        .ActivateOnEnter<StrongWind>()
        .ActivateOnEnter<StrongWindPath>()
        .ActivateOnEnter<BurningGleam>()
        .ActivateOnEnter<Unravel>()
        .ActivateOnEnter<GroupedFireflights>()
        .ActivateOnEnter<FireflightNights>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14274)]
public class V03NPariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));
