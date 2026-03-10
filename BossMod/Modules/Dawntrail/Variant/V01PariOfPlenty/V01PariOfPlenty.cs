namespace BossMod.Dawntrail.Variant.V01PariOfPlenty;

public enum OID : uint
{
    FlyingCarpet = 0x4A74, // R4.356, x8
    FalseFlame = 0x4A6E, // R4.356, x4
    FieryBauble = 0x4A6F, // R6.000, x8
    Tether = 0x4A75, // R1.000, x6
    Boss = 0x4A6D, // R5.016, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    BurningPillar = 0x1EBf20
}

public enum AID : uint
{
    AutoAttack = 45544, // Boss->player, no cast, single-target
    HeatBurst = 45516, // Boss->self, 5.0s cast, range 60 circle
    Warp = 45421, // Boss->location, no cast, single-target
    CarpetWarp = 45425, // FlyingCarpet->location, no cast, single-target
    CarpetRideVisual1 = 46948, // FalseFlame->location, no cast, single-target
    CarpetRideVisual2 = 46949, // FalseFlame->location, no cast, single-target
    RightFireflight = 45426, // Boss->self, 10.0s cast, range 40 width 4 rect
    LeftFireflight = 45427, // Boss->self, 10.0s cast, range 40 width 4 rect
    RightFableflight1 = 45428, // FalseFlame->self, 10.5s cast, range 40 width 4 rect
    LeftFableflight1 = 45429, // FalseFlame->self, 10.5s cast, range 40 width 4 rect
    RightFableflight2 = 46946, // FalseFlame->self, 9.6s cast, range 40 width 4 rect
    LeftFableflight2 = 46947, // FalseFlame->self, 9.6s cast, range 40 width 4 rect
    RightFireflightFourLongNights = 45461, // Boss->self, 17.0s cast, range 40 width 4 rect
    LeftFireflightFourLongNights = 45462, // Boss->self, 17.0s cast, range 40 width 4 rect
    RightFireflightFactAndFiction = 47025, // Boss->self, 10.0s cast, range 40 width 4 rect
    LeftFireflightFactAndFiction = 47026, // Boss->self, 10.0s cast, range 40 width 4 rect
    CarpetRideBossRight = 45430, // FalseFlame/Boss->location, no cast, single-target
    CarpetRideBossLeft = 45431, // Boss/FalseFlame->location, no cast, single-target
    CarpetRideRight1 = 45432, // Helper->location, 2.0s cast, charge, hits right side
    CarpetRideLeft1 = 45433, // Helper->location, 2.0s cast, charge, hits left side
    CarpetRideRight2 = 46950, // Helper->location, 2.0s cast, ???
    CarpetRideLeft2 = 46951, // Helper->location, 2.0s cast, ???
    CarpetRideRight3 = 47020, // Helper->location, 3.7s cast, ???
    CarpetRideLeft3 = 47021, // Helper->location, 3.7s cast, ???
    SunCirclet = 45447, // Boss->self, 2.0s cast, range 8-60 donut
    Doubling = 45203, // Boss->self, 3.0s cast, single-target
    CharmedChains = 45199, // Boss->self, 4.0s cast, single-target
    FireOfVictory = 45518, // Boss->players, 5.0s cast, range 4 circle
    BurningGleam1 = 45499, // FieryBauble->self, 7.0s cast, range 40 width 10 cross
    BurningGleam2 = 47397, // FieryBauble->self, 8.0s cast, range 40 width 10 cross
    BurningGleam3 = 45043, // FieryBauble->self, 14.0s cast, range 40 width 10 cross
    WheelOfFireflight1 = 45463, // Boss->self, no cast, range 40 180-degree cone
    WheelOfFireflight2 = 45464, // Boss->self, no cast, range 40 180-degree cone
    WheelOfFireflight3 = 45465, // Boss->self, no cast, range 40 180-degree cone
    WheelOfFireflight4 = 45466, // Boss->self, no cast, range 40 180-degree cone
    FellSpark = 45475, // Helper->player, no cast, single-target
    ParisCurse = 45520, // Boss->self, 5.0s cast, range 60 circle
    CharmingBaubles = 45496, // Boss->self, 3.0s cast, single-target
    HighFirePowder = 45522, // Helper->location, no cast, range 15 circle, stack
    FirePowder = 45521, // Helper->self, no cast, range 15 circle, spread
    CharmdFableflight = 47024, // Boss->self, 4.0s cast, single-target
    SpurningFlames = 45481, // Boss->self, 7.0s cast, range 40 circle
    ImpassionedSparksCast = 45483, // Boss->self, 5.0s cast, single-target
    ImpassionedSparksBoss = 45484, // Boss->self, no cast, single-target
    ImpassionedSparksVisual = 45485, // Helper->self, 2.0s cast, single-target
    ImpassionedSparksPuddle = 45487, // Helper->self, 6.0s cast, range 8 circle
    BurningPillar = 45526, // Helper->self, 4.0s cast, range 10 circle
    FireWell = 45528, // Helper->players, no cast, range 6 circle
    ScouringScorn = 45490, // Boss->self, 6.0s cast, range 40 circle
}

public enum SID : uint
{
    BurningChains = 769, // none->player, extra=0x0
    DarkResistanceDown = 3619, // Helper->player, extra=0x0
    CurseOfSolitude = 4615, // none->player, extra=0x0
    CurseOfCompanionship = 4616, // none->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    Fury = 4627, // Boss->Boss, extra=0x16B
    Burns = 3065, // none->player, extra=0x0
    Burns1 = 3066, // none->player, extra=0x0
}

public enum IconID : uint
{
    Chains = 97, // player->self
    FireWell = 318, // player->self
    Tankbuster = 342, // player->self
    TurnRightW = 624, // Boss->self
    TurnLeftW = 625, // Boss->self
    TurnRightE = 644, // Boss->self
    TurnLeftE = 645, // Boss->self
}

public enum TetherID : uint
{
    Rug = 355, // Tether->Tether
    Chains = 9, // player->player
    Generic = 84, // Boss->player
}

class HeatBurst(BossModule module) : Components.RaidwideCast(module, AID.HeatBurst);
class SpurningFlames(BossModule module) : Components.RaidwideCast(module, AID.SpurningFlames);
class ScouringScorn(BossModule module) : Components.RaidwideCast(module, AID.ScouringScorn);
class ParisCurse(BossModule module) : Components.RaidwideCast(module, AID.ParisCurse);

class FlightRect(BossModule module) : Components.GroupedAOEs(module, [AID.RightFireflight, AID.LeftFireflight, AID.RightFireflightFourLongNights, AID.LeftFireflightFourLongNights, AID.RightFireflightFactAndFiction, AID.LeftFireflightFactAndFiction, AID.RightFableflight1, AID.LeftFableflight1, AID.RightFableflight2, AID.LeftFableflight2], new AOEShapeRect(40, 2));

class Fireflight(BossModule module) : Components.GenericAOEs(module)
{
    Angle _rotation;
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
        var dir = path.ToPos - path.FromPos;
        var center = WPos.Lerp(path.FromPos, path.ToPos, 0.5f);
        return new(new AOEShapeRect(50, 50), center, dir.ToAngle() + _rotation, path.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RightFireflight:
            case AID.RightFireflightFactAndFiction:
                _rotation = -90.Degrees();
                _start = Module.CastFinishAt(spell, 2.7f);
                break;
            case AID.LeftFireflight:
            case AID.LeftFireflightFactAndFiction:
                _rotation = 90.Degrees();
                _start = Module.CastFinishAt(spell, 2.7f);
                break;
            case AID.SunCirclet:
                _path.Clear();
                _rotation = default;
                _start = default;
                _donut = null;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Rug && _rotation != default)
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
        if ((AID)spell.Action.ID is AID.CarpetRideRight1 or AID.CarpetRideLeft1)
        {
            if (_path.Count > 0)
                _path.RemoveAt(0);
        }
    }
}
class SunCirclet(BossModule module) : Components.StandardAOEs(module, AID.SunCirclet, new AOEShapeDonut(8, 60));

class Fableflight(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(WPos Source, Angle Rotation, DateTime Activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime next = default;
        foreach (var (c, r, a) in _casters)
        {
            if (next == default)
                next = a;

            if (a < next.AddSeconds(1))
                yield return new(new AOEShapeRect(40, 100), c, r, a);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RightFableflight1:
                _casters.Add((caster.Position, caster.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 2.7f)));
                break;
            case AID.RightFableflight2:
                _casters.Add((caster.Position, caster.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 2.7f)));
                _casters.Add((Arena.Center - (caster.Position - Arena.Center), caster.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 4.4f)));
                break;
            case AID.LeftFableflight1:
                _casters.Add((caster.Position, caster.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 2.7f)));
                break;
            case AID.LeftFableflight2:
                _casters.Add((caster.Position, caster.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 2.7f)));
                _casters.Add((Arena.Center - (caster.Position - Arena.Center), caster.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 4.4f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetRideLeft1 or AID.CarpetRideRight1 or AID.CarpetRideRight2 or AID.CarpetRideLeft2 or AID.CarpetRideRight3 or AID.CarpetRideLeft3)
        {
            _casters.RemoveAll(c => c.Source.AlmostEqual(caster.Position, 1));
        }
    }
}

class ChainStack(BossModule module) : BossComponent(module)
{
    BitMask _targets;
    DateTime _activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Chains)
        {
            _targets.Set(Raid.FindSlot(targetID));
            _activation = WorldState.FutureTime(4.1f);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Chains)
        {
            _targets.Reset();
            _activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_targets[slot])
            foreach (var (_, buddy) in Raid.WithSlot().Exclude(actor).IncludedInMask(_targets))
                hints.AddForbiddenZone(ShapeContains.Donut(buddy.Position, 2, 60), _activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets[slot])
            hints.Add("Stack with buddy!", Raid.WithSlot().Exclude(actor).IncludedInMask(_targets).Any(t => !t.Item2.Position.InCircle(actor.Position, 2)));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets[pcSlot] && _targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;
}
class CharmedChains(BossModule module) : Components.Chains(module, (uint)TetherID.Chains, chainLength: 22, activationDelay: 5);
class BurningGleam(BossModule module) : Components.GroupedAOEs(module, [AID.BurningGleam1, AID.BurningGleam2, AID.BurningGleam3], new AOEShapeCross(40, 5));
class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, AID.FireOfVictory, new AOEShapeCircle(4), centerAtTarget: true, endsOnCastEvent: true);

class FourLongNights(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Angle, DateTime)> _rotations = [];
    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (a, d) in _rotations.Take(1))
            yield return new(new AOEShapeCone(40, 90.Degrees()), Module.PrimaryActor.Position, a, d);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RightFireflightFourLongNights:
            case AID.LeftFireflightFourLongNights:
                _activation = Module.CastFinishAt(spell, 2.1f);
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (_rotations.Count > 0)
            return;

        switch ((IconID)iconID)
        {
            case IconID.TurnRightW:
            case IconID.TurnLeftE:
                _rotations.Add((180.Degrees(), _activation));
                _rotations.Add((default, _activation.AddSeconds(4.6f)));
                _rotations.Add((default, _activation.AddSeconds(9.2f)));
                _rotations.Add((180.Degrees(), _activation.AddSeconds(13.8f)));
                break;
            case IconID.TurnLeftW:
            case IconID.TurnRightE:
                _rotations.Add((default, _activation));
                _rotations.Add((180.Degrees(), _activation.AddSeconds(4.6f)));
                _rotations.Add((180.Degrees(), _activation.AddSeconds(9.2f)));
                _rotations.Add((default, _activation.AddSeconds(13.8f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WheelOfFireflight3 or AID.WheelOfFireflight4 or AID.WheelOfFireflight1 or AID.WheelOfFireflight2)
        {
            if (_rotations.Count > 0)
                _rotations.RemoveAt(0);
        }
    }
}

class FellSpark(BossModule module) : BossComponent(module)
{
    DateTime _next;
    readonly DateTime[] _debuffLeft = new DateTime[4];
    int _target = -1;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RightFireflightFourLongNights or AID.LeftFireflightFourLongNights)
            _next = Module.CastFinishAt(spell, 2.2f);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkResistanceDown && Raid.TryFindSlot(actor, out var slot))
            _debuffLeft[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkResistanceDown && Raid.TryFindSlot(actor, out var slot))
            _debuffLeft[slot] = default;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Raid[_target] is { } target)
        {
            var color = _debuffLeft[pcSlot] > _next ? ArenaColor.Danger : ArenaColor.Safe;
            Arena.AddLine(Module.PrimaryActor.Position, target.Position, color, 1);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_debuffLeft.BoundSafeAt(_target) > _next)
        {
            if (_target == slot)
                hints.Add("Pass tether!");
            else if (_debuffLeft[slot] <= _next)
                hints.Add("Take tether!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_debuffLeft[slot] > _next)
        {
            // if we need to pass the tether, just wait, it's usually more annoying for other party members to try to chase us around
            if (_target != slot && Raid[_target] is { } otherTarget)
                hints.AddForbiddenZone(ShapeContains.Rect(Module.PrimaryActor.Position, otherTarget.Position, 1), _next);
        }
        else if (_target >= 0 && _debuffLeft[_target] > _next)
        {
            if (Raid[_target] is { } badTarget)
                hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.PrimaryActor.Position, badTarget.Position, 1), _next.AddSeconds(-1));
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Generic)
            _target = Raid.FindSlot(tether.Target);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Generic)
            _target = -1;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FellSpark)
            _next = WorldState.FutureTime(4.5f);
    }
}

class FirePowder(BossModule module) : Components.UniformStackSpread(module, 15, 15, alwaysShowSpreads: true)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.CurseOfCompanionship:
                AddStack(actor, status.ExpireAt);
                break;
            case SID.CurseOfSolitude:
                AddSpread(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HighFirePowder or AID.FirePowder)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}

class ImpassionedSparks(BossModule module) : Components.StandardAOEs(module, AID.ImpassionedSparksPuddle, 8);
class BurningPillar(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 10, AID.BurningPillar, m => m.Enemies(OID.BurningPillar).Where(p => p.EventState != 7), 0);
class FireWell(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FireWell, AID.FireWell, 6, 6);

class V01PariOfPlentyStates : StateMachineBuilder
{
    public V01PariOfPlentyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<Fireflight>()
            .ActivateOnEnter<FlightRect>()
            .ActivateOnEnter<SunCirclet>()
            .ActivateOnEnter<Fableflight>()
            .ActivateOnEnter<ChainStack>()
            .ActivateOnEnter<CharmedChains>()
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<FireOfVictory>()
            .ActivateOnEnter<FourLongNights>()
            .ActivateOnEnter<FellSpark>()
            .ActivateOnEnter<SpurningFlames>()
            .ActivateOnEnter<ParisCurse>()
            .ActivateOnEnter<FirePowder>()
            .ActivateOnEnter<ImpassionedSparks>()
            .ActivateOnEnter<BurningPillar>()
            .ActivateOnEnter<FireWell>()
            .ActivateOnEnter<ScouringScorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14274)]
public class V01PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760, -805), new ArenaBoundsSquare(20));
