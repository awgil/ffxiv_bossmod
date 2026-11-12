namespace BossMod.Endwalker.Variant.V01Sildihn.V011GeryonTheSteer;

public enum OID : uint
{
    Boss = 0x398B, // R9.600, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    PowderKegBlue = 0x39C9, // R1.000, x0 (spawn during fight)
    PowderKegRed = 0x398C, // R1.000, x0 (spawn during fight)
    SludgeVoidzone = 0x1EB7F0, // R0.500, EventObj type
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    ColossalStrike = 29903, // Boss->player, 5.0s cast, single-target
    ExplodingCatapult = 29895, // Boss->self, 5.0s cast, range 60 circle
    ExplosionCircle = 29908, // PowderKegRed/PowderKegBlue->self, 2.5s cast, range 15 circle
    ExplosionDonut = 29909, // PowderKegBlue/PowderKegRed->self, 2.5s cast, range 3-17 donut
    ColossalSlam = 29904, // Boss->self, 6.0s cast, range 60 60-degree cone
    SubterraneanShudder = 29906, // Boss->self, 5.0s cast, range 60 circle
    RollingBoulder = 29914, // Helper->self, no cast, range 10 width 10 rect
    BossJump = 29894, // Boss->location, no cast, single-target
    ColossalLaunch = 29896, // Boss->self, 5.0s cast, range 40 width 40 rect
    FlipToRed = 31260, // PowderKegBlue->self, no cast, single-target
    FlipToBlue = 29907, // PowderKegRed->self, no cast, single-target
    ColossalChargeRight = 29900, // Boss->location, 8.0s cast, width 14 rect charge
    ColossalChargeLeft = 29901, // Boss->location, 8.0s cast, width 14 rect charge
    GigantomillCCWCast = 29897, // Boss->self, 8.0s cast, range 72 width 10 cross
    GigantomillCWCast = 29898, // Boss->self, 8.0s cast, range 72 width 10 cross
    GigantomillRepeat = 29899, // Boss->self, no cast, range 72 width 10 cross
    Shockwave = 29902, // Boss->self, 5.0s cast, range 40 width 40 rect, knockback 15 left/right
    ColossalSwing = 29905, // Boss->self, 5.0s cast, range 60 180-degree cone
    RunawaySludge = 29910, // Helper->self, 5.0s cast, range 9 circle, spawns voidzone
    RunawayRunoff = 29911, // Helper->self, 9.0s cast, range 60 circle, knockback 18 away from source
    SuddenlySewage = 29912, // Helper->self, no cast, range 20 width 20 rect
    Intake = 29913, // Helper->self, no cast, range 40 width 10 rect, pull 25 towards origin
}

class ColossalStrike(BossModule module) : Components.SingleTargetCast(module, AID.ColossalStrike);
class ExplodingCatapult(BossModule module) : Components.RaidwideCast(module, AID.ExplodingCatapult);
class ColossalSlam(BossModule module) : Components.StandardAOEs(module, AID.ColossalSlam, new AOEShapeCone(60, 30.Degrees()));
class SubterraneanShudder(BossModule module) : Components.RaidwideCast(module, AID.SubterraneanShudder);
class ColossalLaunch(BossModule module) : Components.RaidwideCast(module, AID.ColossalLaunch);
class ColossalChargeRight(BossModule module) : Components.ChargeAOEs(module, AID.ColossalChargeRight, 7);
class ColossalChargeLeft(BossModule module) : Components.ChargeAOEs(module, AID.ColossalChargeLeft, 7);

class Gigantomill(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.GigantomillCWCast or AID.GigantomillCCWCast)
            Sequences.Add(new(new AOEShapeCross(72, 5), spell.LocXZ, spell.Rotation, aid == AID.GigantomillCWCast ? -22.5f.Degrees() : 22.5f.Degrees(), Module.CastFinishAt(spell), 1.7f, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GigantomillCCWCast or AID.GigantomillCWCast or AID.GigantomillRepeat)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class ColossalSwing(BossModule module) : Components.StandardAOEs(module, AID.ColossalSwing, new AOEShapeCone(60, 90.Degrees()));
class RunawaySludge(BossModule module) : Components.VoidzoneAtCastTarget(module, 9, AID.RunawaySludge, OID.SludgeVoidzone, 0.2f, a => a.EventState == 7);

class PowderKegs(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeCircle Circle = new(15);
    public static readonly AOEShapeDonut Donut = new(3, 17);
    public readonly List<AOEInstance> AOEs = [];
    private bool _draw = true;
    private bool _intake;
    private bool _boulders;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _draw ? AOEs : [];

    public PointTest UnsafeTest() => ShapeDistance.Union([.. AOEs.Where(a => a.Shape != Donut).Select(a => a.DistV)]).Discretize();

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = (OID)actor.OID switch
        {
            OID.PowderKegRed => Circle,
            OID.PowderKegBlue => Donut,
            _ => null
        };
        if (shape != null)
            AOEs.Add(new(shape, actor.Position, default, WorldState.FutureTime(_intake ? 11.3f : _boulders ? 12.3f : 14.3f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_draw || AOEs.Count == 0)
            return;
        var delay = (AID)spell.Action.ID switch
        {
            AID.ColossalSlam or AID.ColossalChargeRight or AID.ColossalChargeLeft => 5.4f,
            AID.ColossalSwing => 5.6f,
            AID.GigantomillCWCast or AID.GigantomillCCWCast => -1.5f,
            AID.ColossalLaunch => 6.7f,
            AID.RunawayRunoff => 4.5f,
            _ => 0
        };
        if (delay == 0)
            return;
        _draw = true;
        var activation = Module.CastFinishAt(spell, delay);
        for (var i = 0; i < AOEs.Count; ++i)
            AOEs[i] = AOEs[i] with { Activation = activation };

        var shift = (AID)spell.Action.ID switch
        {
            AID.ColossalChargeRight => -9,
            AID.ColossalChargeLeft => 9,
            _ => 0
        };
        if (shift != 0)
        {
            var pos = Module.PrimaryActor.Position;
            var dir = V011GeryonTheSteer.Round90(caster.Rotation).ToDirection();
            var index = AOEs.FindIndex(a => (a.Origin - pos).Dot(dir) is >= 0 and <= 40 && Math.Abs((a.Origin - pos).Dot(dir.OrthoL())) <= 7);
            if (index >= 0)
            {
                var origin = AOEs[index].Origin;
                AOEs[index] = AOEs[index] with { Origin = new WPos(MathF.Round(origin.X), MathF.Round(origin.Z)) + shift * dir.OrthoL() };
            }
        }
        else if ((AID)spell.Action.ID == AID.ColossalLaunch)
        {
            for (var i = 0; i < AOEs.Count; ++i)
                AOEs[i] = AOEs[i] with { Shape = AOEs[i].Shape == Circle ? Donut : Circle };
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExplosionCircle or AID.ExplosionDonut)
        {
            AOEs.Clear();
            _draw = _intake = _boulders = false;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x09 && state is 0x00200010 or 0x00020001)
            _draw = _intake = true;
        else if (index == 0x0A && state is 0x00020001 or 0x00200010 or 0x02000100 or 0x00800040)
            _draw = _boulders = true;
    }
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _inner = new(20, 20, 20);
    private static readonly AOEShapeRect _hole = new(10, 10, 10);
    private AOEInstance? _shrink;
    private readonly Dictionary<byte, AOEInstance> _holes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_shrink).Concat(_holes.Values);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ColossalStrike && Arena.Bounds.Radius > 21)
            _shrink = new(_inner, Arena.Center, default, WorldState.FutureTime(4), Inverted: true);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x13 && state == 0x00080004)
        {
            Arena.Bounds = new ArenaBoundsSquare(20);
            _shrink = null;
        }
        else if (index is >= 0x05 and <= 0x08)
        {
            if (state == 0x00800040)
                _holes[index] = new(_hole, V011GeryonTheSteer.SewerSquare(index), default, WorldState.FutureTime(3));
            else if (state == 0x00080004)
                _holes.Clear();
        }
    }
}

class SuddenlySewage(BossModule module) : Components.GenericAOEs(module, AID.SuddenlySewage)
{
    private static readonly AOEShapeRect _rect = new(20, 10);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count == 4)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 6), _aoes[0].Activation.AddSeconds(2));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001 && index is >= 0x05 and <= 0x08)
            _aoes.Add(new(_rect, V011GeryonTheSteer.SewerSquare(index) + new WDir(0, -10), default, WorldState.FutureTime(8)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && (++NumCasts & 1) == 0 && _aoes.Count >= 2)
            _aoes.RemoveRange(0, 2);
    }
}

class RollingBoulder(BossModule module) : Components.Exaflare(module, new AOEShapeRect(10, 5))
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x0A)
            return;
        (float, float)? lanes = state switch
        {
            0x00020001 => (-15, 5),
            0x00200010 => (-15, 15),
            0x02000100 => (-5, 5),
            0x00800040 => (-5, 15),
            _ => null
        };
        if (lanes is not (var east, var west))
            return;
        var activation = WorldState.FutureTime(16.2f);
        Lines.Add(new() { Next = new(15, east - 5), Advance = new(-10, 0), NextExplosion = activation, TimeToMove = 0.3f, ExplosionsLeft = 4, MaxShownExplosions = 4 });
        Lines.Add(new() { Next = new(-15, west - 5), Advance = new(10, 0), NextExplosion = activation, TimeToMove = 0.3f, ExplosionsLeft = 4, MaxShownExplosions = 4 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.RollingBoulder)
            return;
        var pos = caster.Position - new WDir(0, 5);
        var index = Lines.FindIndex(l => l.Next.AlmostEqual(pos, 1));
        if (index < 0)
            return;
        ++NumCasts;
        AdvanceLine(Lines[index], pos);
        if (Lines[index].ExplosionsLeft == 0)
            Lines.RemoveAt(index);
    }
}

class Intake(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private static readonly AOEShapeRect _rect = new(40, 5);
    private readonly PowderKegs? _kegs = module.FindComponent<PowderKegs>();
    private readonly List<Source> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _kegs?.AOEs.Count > 0 ? _sources : [];

    public override PointTest DestinationUnsafe(int slot, Actor actor) => _kegs?.UnsafeTest() ?? PointTest.Never;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x09)
            return;
        var x = state switch
        {
            0x00200010 => 20f,
            0x00020001 => -20f,
            _ => 0f
        };
        if (x == 0)
            return;
        var activation = WorldState.FutureTime(14.2f);
        var dir = (x > 0 ? -90 : 90).Degrees();
        for (var i = 0; i < 4; ++i)
            _sources.Add(new(new(x, 15 - i * 10), 25, activation, _rect, dir, Kind.TowardsOrigin));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Intake)
            _sources.Clear();
    }
}

class RunawayRunoff(BossModule module) : Components.KnockbackFromCastTarget(module, AID.RunawayRunoff, 18)
{
    private readonly PowderKegs? _kegs = module.FindComponent<PowderKegs>();

    public override PointTest DestinationUnsafe(int slot, Actor actor) => _kegs?.UnsafeTest() ?? PointTest.Never;
}

class Shockwave(BossModule module) : Components.Knockback(module, AID.Shockwave)
{
    private static readonly AOEShapeRect _shape = new(40, 40);
    private readonly List<Source> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var activation = Module.CastFinishAt(spell);
            _sources.Add(new(Arena.Center, 15, activation, _shape, -90.Degrees(), Kind.DirForward));
            _sources.Add(new(Arena.Center, 15, activation, _shape, 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _sources.Clear();
    }
}

class V011GeryonTheSteerStates : StateMachineBuilder
{
    public V011GeryonTheSteerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PowderKegs>()
            .ActivateOnEnter<Intake>()
            .ActivateOnEnter<RunawaySludge>()
            .ActivateOnEnter<RunawayRunoff>()
            .ActivateOnEnter<SuddenlySewage>()
            .ActivateOnEnter<RollingBoulder>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Gigantomill>()
            .ActivateOnEnter<ColossalChargeLeft>()
            .ActivateOnEnter<ColossalChargeRight>()
            .ActivateOnEnter<ColossalSwing>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<ColossalStrike>()
            .ActivateOnEnter<ColossalLaunch>()
            .ActivateOnEnter<SubterraneanShudder>()
            .ActivateOnEnter<ExplodingCatapult>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "xan, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442, SortOrder = 2)]
public class V011GeryonTheSteer(ModuleInit init) : BossModule(init, ArenaCenter(init.Primary), new ArenaBoundsSquare(init.Primary.Position.X < -150 ? 24.5f : 19.5f))
{
    private static WPos ArenaCenter(Actor primary) => primary.Position.X < -150 ? new(-213, 101) : primary.Position.X > 100 ? new(183, 177) : default;

    public static Angle Round90(Angle a) => (MathF.Round(a.Deg / 90) * 90).Degrees();

    public static WPos SewerSquare(byte index) => index switch
    {
        0x05 => new(173, 167),
        0x06 => new(193, 167),
        0x07 => new(173, 187),
        _ => new(193, 187),
    };
}

