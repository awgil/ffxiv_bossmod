namespace BossMod.Dawntrail.Criterion.C01MerchantsTale.C013PariOfPlenty;

public enum OID : uint
{
    Boss = 0x4A70, // R5.016, x1
    Helper = 0x233C, // R0.500, x10-32 (spawn during fight), Helper type
    FalseFlame = 0x4A71, // R5.016, x4
    FieryBauble = 0x4A72, // R6.000, x8
    IcyBauble = 0x4A73, // R5.000, x1
    FlyingCarpet = 0x4A74, // R4.356, x8
    TetherHelper = 0x4A75, // R1.000, x6
}

public enum AID : uint
{
    AutoAttack = 45545, // Boss->player, no cast, single-target
    HeatBurst = 45517, // Boss->self, 5.0s cast, range 60 circle
    Jump = 45421, // Boss->location, no cast, single-target

    FireflightByEmberlightRight = 45434, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByEmberlightLeft = 45435, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByPyrelightRight = 45436, // Boss->self, 10.0s cast, range 40 width 4 rect
    FireflightByPyrelightLeft = 45437, // Boss->self, 10.0s cast, range 40 width 4 rect
    CarpetRideDashRight = 45440, // Boss/FalseFlame->location, no cast, single-target
    CarpetRideDashLeft = 45441, // Boss/FalseFlame->location, no cast, single-target
    CarpetRide1Right = 45442, // Helper->location, 2.0s cast, ???
    CarpetRide1Left = 45443, // Helper->location, 2.0s cast, ???
    FireflightByEmberlightStack = 45444, // Helper->player, no cast, range 3 circle
    FireflightByPyrelightSpread = 45445, // Helper->players, no cast, range 3 circle
    SunCirclet = 45448, // Boss->self, 2.0s cast, range 8-60 donut

    // name refers to the orientation of the initial rects, add rotation goes in either direction
    WheelOfFableflightCCW = 45478, // FalseFlame->self, 11.0s cast, range 40 width 4 rect
    WheelOfFableflightCW = 45479, // FalseFlame->self, 11.0s cast, range 40 width 4 rect
    ScatteredKindlingCast = 45536, // Boss->self, 8.0s cast, single-target
    ScatteredKindling = 45537, // Helper->players, no cast, range 6 circle
    KindledFlame1Cast = 45538, // Boss->self, 8.0s cast, single-target
    KindledFlame1 = 45539, // Helper->players, no cast, range 6 circle
    WheelOfFireflightRTLBack = 45469, // Boss/FalseFlame->self, no cast, range 40 ?-degree cone
    WheelOfFireflightRTLFront = 45470, // Boss/FalseFlame->self, no cast, range 40 ?-degree cone
    WheelOfFireflightLTRFront = 45471, // FalseFlame/Boss->self, no cast, range 40 ?-degree cone
    WheelOfFireflightLTRBack = 45472, // FalseFlame/Boss->self, no cast, range 40 ?-degree cone
    FireOfVictory = 45519, // Boss->player, 5.0s cast, range 4 circle

    FireflightFourLongNightsE = 45467, // Boss->self, 17.0s cast, range 40 width 4 rect
    FireflightFourLongNightsW = 45468, // Boss->self, 17.0s cast, range 40 width 4 rect
    FellFirework = 45476, // Helper->players, no cast, range 3 circle, spread
    FireWell = 45477, // Helper->players, no cast, range 3 circle, stack

    ParisCurse = 45551, // Boss->self, 5.0s cast, range 60 circle
    CharmingBaubles = 45503, // Boss->self, 3.0s cast, single-target
    CharmingBaublesInstant = 45497, // Boss->self, no cast, single-target
    ThievesWeave = 46754, // Boss->self, 3.0s cast, single-target
    CarpetAppear = 45504, // FlyingCarpet->self, 1.0s cast, single-target
    CarpetDash = 47153, // FlyingCarpet->location, no cast, single-target
    Unravel = 45506, // FlyingCarpet->self, 3.0s cast, single-target
    FableflightRightVerySlow = 45438, // FalseFlame->self, 24.1s cast, range 40 width 4 rect
    FableflightLeftVerySlow = 45439, // FalseFlame->self, 24.1s cast, range 40 width 4 rect
    BurningGleam = 45548, // FieryBauble->self, 7.0s cast, range 40 width 10 cross
    ChillingGleam = 45501, // IcyBauble->self, 2.0s cast, range 40 width 10 cross
    FirePowder = 45523, // Helper->self, no cast, range 15 circle, spread
    HighFirePowder = 45524, // Helper->location, no cast, range 15 circle, stack

    SpurningFlames = 45482, // Boss->self, 7.0s cast, range 40 circle
    ImpassionedSparksCast = 45483, // Boss->self, 5.0s cast, single-target
    ImpassionedSparksVisual = 45484, // Boss->self, no cast, single-target
    ImpassionedSparksFireball = 45485, // Helper->self, 2.0s cast, single-target
    ImpassionedSparksPuddle = 45488, // Helper->self, 6.0s cast, range 8 circle
    BurningPillar = 45527, // Helper->self, 4.0s cast, range 10 circle
    HotFoot = 45542, // Helper->player, no cast, range 10 circle
    ScouringScorn = 45491, // Boss->self, 6.0s cast, range 40 circle

    Doubling = 47041, // Boss->self, 3.0s cast, single-target
    FableflightRightFast = 45449, // FalseFlame->self, 8.0s cast, range 40 width 4 rect
    FableflightLeftFast = 45450, // FalseFlame->self, 8.0s cast, range 40 width 4 rect
    FableflightRightSlow = 45451, // FalseFlame->self, 15.0s cast, range 40 width 4 rect
    FableflightLeftSlow = 45452, // FalseFlame->self, 15.0s cast, range 40 width 4 rect
    Explosion = 45532, // Helper->self, 8.0s cast, range 4 circle
    UnmitigatedExplosion = 45533, // Helper->self, no cast, range 60 circle
    CarpetRideTetherRight = 46573, // Helper->location, 2.1s cast, ???
    CarpetRideTetherLeft = 46574, // Helper->location, 2.1s cast, ???

    KindledFlame2Cast = 45540, // Boss->self, 5.0s cast, single-target
    KindledFlame2 = 45541, // Helper->players, no cast, range 6 circle

    CharmdFlightFourNightsE = 47031, // Boss->self, 17.5s cast, range 40 width 4 rect
    CharmdFlightFourNightsW = 47032, // Boss->self, 17.5s cast, range 40 width 4 rect

    SpurningFlamesPreEnrage = 45492, // Boss->self, 7.0s cast, range 40 circle
    ScouringScornEnrageCast = 45493, // Boss->self, 7.0s cast, range 40 circle
    ScouringScornEnrage = 45494, // Boss->self, no cast, range 40 circle
    UtterImmolation = 45525, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    Unk = 2056, // Boss/FalseFlame->Boss/FalseFlame/4A73/4A72, extra=0x3DA/0x3D9/0x448
    WitchHunt = 2970, // none->Boss, extra=0x3F5 (far)/0x3F4 (close)
    FireResistanceDownII = 2937, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    DarkResistanceDown = 3619, // Helper->player, extra=0x0
    CurseOfImmolation = 4617, // none->player, extra=0x0, fire debuff
    CurseOfSolitude = 4615, // none->player, extra=0x0, spread
    CurseOfCompanionship = 4616, // none->player, extra=0x0, stack
    Fury = 4627, // Boss->Boss, extra=0x16B, visual
    ChainsOfPassionA = 4844, // none->player, extra=0x0
    ChainsOfPassionB = 4845, // none->player, extra=0x0
    Burns = 3065, // none->player, extra=0x0
    BurningChains = 769, // none->player, extra=0x0
    Prey = 2939, // none->player, extra=0x0
}

public enum IconID : uint
{
    LeftToRight3Sec = 624, // Boss->self
    LeftToLeft3Sec = 625, // Boss->self
    LeftToRight8Sec = 628, // FalseFlame->self
    LeftToLeft8Sec = 629, // FalseFlame->self
    RightToRight3Sec = 644, // Boss->self
    RightToLeft3Sec = 645, // Boss->self
    RightToLeft8Sec = 647, // FalseFlame->self
    RightToRight8Sec = 646, // FalseFlame->self
    Tankbuster = 342, // player->self
    Chain = 97, // player->self
    Spread10 = 138, // player->self
    Rug = 631, // player->self
    Stack = 161, // player->self
}

public enum TetherID : uint
{
    Rug = 355, // TetherHelper->TetherHelper
    Chain = 9, // player->player
    Generic = 17, // FalseFlame->player
}

class HeatBurst(BossModule module) : Components.RaidwideCast(module, AID.HeatBurst);
class FireflightRectStart(BossModule module) : Components.GroupedAOEs(module, [AID.FireflightByEmberlightRight, AID.FireflightByEmberlightLeft, AID.FireflightByPyrelightRight, AID.FireflightByPyrelightLeft], new AOEShapeRect(40, 2));

class FireflightPath(BossModule module) : Components.GenericAOEs(module)
{
    record struct Path(Actor From, Actor To, DateTime Activation);

    readonly List<Path> _paths = [];

    Angle _angle;
    DateTime _first;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var (f, t, a) in _paths.Take(2))
        {
            if (f == t)
            {
                yield return new(new AOEShapeDonut(8, 60), f.Position, default, a, Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky: i == 0);
            }
            else
            {
                var from = f.Position;
                var to = t.Position;
                var dir = to - from;
                var center = from + dir * 0.5f;
                var ang = Angle.FromDirection(dir) + _angle;
                yield return new(new AOEShapeRect(50, dir.Length() * 0.5f), center, ang, a, Color: i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky: i == 0);
            }
            i++;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FireflightByEmberlightRight:
            case AID.FireflightByPyrelightRight:
                _first = Module.CastFinishAt(spell, 2.7f);
                _angle = -90.Degrees();
                break;
            case AID.FireflightByEmberlightLeft:
            case AID.FireflightByPyrelightLeft:
                _first = Module.CastFinishAt(spell, 2.7f);
                _angle = 90.Degrees();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetRide1Left or AID.CarpetRide1Right or AID.SunCirclet)
        {
            NumCasts++;
            if (_paths.Count > 0)
                _paths.RemoveAt(0);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Rug)
        {
            _paths.Add(new(source, WorldState.Actors.Find(tether.Target)!, _first.AddSeconds(2.1f * _paths.Count)));
            if (_paths.Count == 3)
                _paths.Add(new(_paths[^1].To, _paths[^1].To, _paths[^1].Activation.AddSeconds(2.1f)));
        }
    }
}

class EmberPyre(BossModule module) : Components.UniformStackSpread(module, 3, 3)
{
    int _next; // 1 = stack, 2 = spread
    int _numCarpets;
    public int NumCasts { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FireflightByEmberlightRight:
            case AID.FireflightByEmberlightLeft:
                _next = 2;
                break;
            case AID.FireflightByPyrelightRight:
            case AID.FireflightByPyrelightLeft:
                _next = 1;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (_next)
        {
            case 1:
                hints.Add("Next: stack");
                break;
            case 2:
                hints.Add("Next: spread");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CarpetRide1Left:
            case AID.CarpetRide1Right:
                _numCarpets++;
                if (_numCarpets == 3)
                {
                    if (_next == 1)
                    {
                        if (Raid.WithoutSlot().OrderByDescending(r => r.Role == Role.Healer).First() is { } target)
                            AddStack(target, WorldState.FutureTime(4.9f));
                    }
                    else
                        AddSpreads(Raid.WithoutSlot(), WorldState.FutureTime(4.9f));
                }
                break;
            case AID.FireflightByPyrelightSpread:
            case AID.FireflightByEmberlightStack:
                NumCasts++;
                Stacks.Clear();
                Spreads.Clear();
                break;
        }
    }
}

class SunCirclet(BossModule module) : Components.CastCounter(module, AID.SunCirclet);

class WheelRectStart(BossModule module) : Components.GroupedAOEs(module, [AID.WheelOfFableflightCW, AID.WheelOfFableflightCCW], new AOEShapeRect(40, 2), highlightImminent: true);
class WheelOfFireflight(BossModule module) : Components.GenericAOEs(module)
{
    WheelRectStart? _start;

    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Angle? rota = (IconID)iconID switch
        {
            IconID.LeftToRight8Sec => -90.Degrees(),
            IconID.RightToRight8Sec => -90.Degrees(),
            IconID.LeftToLeft8Sec => 90.Degrees(),
            IconID.RightToLeft8Sec => 90.Degrees(),
            _ => null
        };
        if (rota.HasValue)
        {
            _start ??= Module.FindComponent<WheelRectStart>();
            if (_start!.Casters.FirstOrDefault(c => c.Position.AlmostEqual(actor.Position, 0.5f)) is { } caster)
            {
                var coneCenter = caster.CastInfo!.Rotation + rota.Value;
                _predicted.Add(new(new AOEShapeCone(40, 90.Degrees()), caster.Position, coneCenter, Module.CastFinishAt(caster.CastInfo, 10)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WheelOfFireflightLTRBack or AID.WheelOfFireflightLTRFront or AID.WheelOfFireflightRTLFront or AID.WheelOfFireflightRTLBack)
        {
            NumCasts++;
            _predicted.Clear();
        }
    }
}

class Kindling(BossModule module) : Components.UniformStackSpread(module, 6, 6)
{
    public int NumCasts { get; private set; }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ScatteredKindlingCast:
                AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell));
                break;
            case AID.KindledFlame1Cast:
                AddStacks(Raid.WithoutSlot().OrderByDescending(r => r.Class.IsSupport()).Take(2), Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ScatteredKindling or AID.KindledFlame1)
        {
            NumCasts++;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, AID.FireOfVictory, new AOEShapeCircle(4), true, true);

class LongNightRectStart(BossModule module) : Components.GroupedAOEs(module, [AID.FireflightFourLongNightsE, AID.FireflightFourLongNightsW], new AOEShapeRect(40, 2), highlightImminent: true);
class FourLongNightsSide(BossModule module) : Components.GenericAOEs(module)
{
    enum Cardinal { N, E, S, W }

    readonly List<(Cardinal, DateTime)> _sides = [];

    Cardinal _prev;
    DateTime _start;

    static readonly Cardinal[] RotateL = [Cardinal.W, Cardinal.N, Cardinal.E, Cardinal.S];
    static readonly Cardinal[] RotateR = [Cardinal.E, Cardinal.S, Cardinal.W, Cardinal.N];
    static readonly Cardinal[] Invert = [Cardinal.S, Cardinal.W, Cardinal.N, Cardinal.E];
    static readonly Angle[] Angles = [180.Degrees(), 90.Degrees(), 0.Degrees(), -90.Degrees()];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sides.Take(1).Select(s => new AOEInstance(new AOEShapeCone(40, 90.Degrees()), Module.PrimaryActor.Position, Angles[(int)s.Item1], s.Item2));

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_sides.Count > 0)
            hints.Add($"Side: {string.Join(", ", _sides.Select(s => s.Item1))}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FireflightFourLongNightsE or AID.FireflightFourLongNightsW)
            _start = Module.CastFinishAt(spell).AddSeconds(2.2f);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.LeftToRight3Sec:
                if (_sides.Count == 0)
                    _prev = Cardinal.W;
                AddSide(RotateR[(int)_prev]);
                break;
            case IconID.RightToRight3Sec:
                if (_sides.Count == 0)
                    _prev = Cardinal.E;
                AddSide(RotateR[(int)_prev]);
                break;
            case IconID.LeftToLeft3Sec:
                if (_sides.Count == 0)
                    _prev = Cardinal.W;
                AddSide(RotateL[(int)_prev]);
                break;
            case IconID.RightToLeft3Sec:
                if (_sides.Count == 0)
                    _prev = Cardinal.E;
                AddSide(RotateL[(int)_prev]);
                break;
        }
    }

    void AddSide(Cardinal c)
    {
        _sides.Add((c, _start.AddSeconds(3 * _sides.Count)));
        _prev = Invert[(int)_prev];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WheelOfFireflightLTRBack or AID.WheelOfFireflightLTRFront or AID.WheelOfFireflightRTLFront or AID.WheelOfFireflightRTLBack)
        {
            NumCasts++;
            if (_sides.Count > 0)
                _sides.RemoveAt(0);
        }
    }
}
class FourLongNightsBait(BossModule module) : Components.UniformStackSpread(module, 3, 3, minStackSize: 3)
{
    enum Bait { Close, Far }

    readonly List<(Bait, DateTime)> _baits = [];

    DateTime _start;

    public int NumStacks { get; private set; }
    public int NumSpreads { get; private set; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_baits.Count > 0)
            hints.Add($"Bait: {string.Join(", ", _baits.Select(b => b.Item1))}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FireflightFourLongNightsE or AID.FireflightFourLongNightsW)
            _start = Module.CastFinishAt(spell, 2.2f);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.WitchHunt)
        {
            switch (status.Extra)
            {
                case 0x3F4:
                    _baits.Add((Bait.Close, _start.AddSeconds(3 * _baits.Count)));
                    break;
                case 0x3F5:
                    _baits.Add((Bait.Far, _start.AddSeconds(3 * _baits.Count)));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FellFirework:
                NumSpreads++;
                if (NumStacks == NumSpreads && _baits.Count > 0)
                    _baits.RemoveAt(0);
                break;
            case AID.FireWell:
                NumStacks++;
                if (NumStacks == NumSpreads && _baits.Count > 0)
                    _baits.RemoveAt(0);
                break;
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        if (_baits.Count > 0)
        {
            var ordered = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position).ToList();
            if (_baits[0].Item1 == Bait.Close)
            {
                AddSpread(ordered[0], _baits[0].Item2);
                AddStack(ordered[^1], _baits[0].Item2);
            }
            else
            {
                AddStack(ordered[0], _baits[0].Item2);
                AddSpread(ordered[^1], _baits[0].Item2);
            }
        }
    }
}

class ParisCurseRaidwide(BossModule module) : Components.RaidwideCast(module, AID.ParisCurse);
class ParisCurseStackSpread(BossModule module) : Components.UniformStackSpread(module, 15, 15, maxStackSize: 2)
{
    public int NumCasts { get; private set; }

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
        switch ((AID)spell.Action.ID)
        {
            case AID.FirePowder:
                NumCasts++;
                Spreads.Clear();
                break;
            case AID.HighFirePowder:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }
}

class FableflightLongRectStart(BossModule module) : Components.GroupedAOEs(module, [AID.FableflightLeftVerySlow, AID.FableflightRightVerySlow], new AOEShapeRect(40, 2));
class FableflightLong(BossModule module) : Components.GenericAOEs(module)
{
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FableflightLeftVerySlow or AID.FableflightRightVerySlow)
        {
            _predicted = new(new AOEShapeRect(40, 80), caster.Position, spell.Rotation, Module.CastFinishAt(spell, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetRide1Left or AID.CarpetRide1Right)
        {
            NumCasts++;
            _predicted = null;
        }
    }
}

class BurningGleam(BossModule module) : Components.GenericAOEs(module, AID.BurningGleam)
{
    public readonly List<(Actor Gem, DateTime Activation)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCross(40, 5), c.Gem.Position, default, c.Activation));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.FieryBauble && id == 0x11D3)
            Casters.Add((actor, WorldState.FutureTime(9)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Gem.Position.AlmostEqual(caster.Position, 1));
        }
    }
}
class BurningGleamSlow(BossModule module) : BurningGleam(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Take(2);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.FieryBauble && id == 0x11D3)
            Casters.Add((actor, WorldState.FutureTime(18)));
    }
}

class ChillingGleam(BossModule module) : Components.GenericAOEs(module, AID.ChillingGleam)
{
    Actor? IceGem;
    Actor? IceCarpet;
    WPos? _source;
    int _numMoves;
    DateTime _activation;
    BitMask _immolation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source is { } src)
            yield return new(new AOEShapeCross(40, 5), src, default, _activation, Inverted: _immolation[slot]);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.CurseOfImmolation)
            _immolation.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.IcyBauble && id == 0x11D3)
        {
            IceGem = actor;
            _activation = WorldState.FutureTime(23.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CarpetAppear:
                if (caster.Position.AlmostEqual(IceGem?.Position ?? default, 1))
                    IceCarpet = caster;
                break;
            case AID.CarpetDash:
                if (caster == IceCarpet)
                {
                    _numMoves++;
                    if (_numMoves == 3)
                        _source = spell.TargetXZ;
                }
                break;
            case AID.ChillingGleam:
                NumCasts++;
                _source = null;
                break;
        }
    }
}

class SpurningFlames(BossModule module) : Components.RaidwideCast(module, AID.SpurningFlames);
class SpurningFlamesPreEnrage(BossModule module) : Components.RaidwideCast(module, AID.SpurningFlamesPreEnrage);
class ImpassionedSparks(BossModule module) : Components.StandardAOEs(module, AID.ImpassionedSparksPuddle, 8);
class BurningPillar(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 10, AID.BurningPillar, m => m.Enemies(0x1EBF20).Where(e => e.EventState != 7), 0.8f)
{
    public int Groups { get; private set; }
    DateTime _prevCast;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (WorldState.CurrentTime > _prevCast.AddSeconds(1))
                Groups++;
            _prevCast = WorldState.CurrentTime;
        }
    }
}

// guessing at break length
class ChainsOfPassion(BossModule module) : Components.Chains(module, (uint)TetherID.Chain, AID.HotFoot, 35);
class HotFoot(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread10, AID.HotFoot, 10, 5.2f);
class ScouringScorn(BossModule module) : Components.RaidwideCast(module, AID.ScouringScorn);

class FableflightRectStart(BossModule module) : Components.GroupedAOEs(module, [AID.FableflightLeftFast, AID.FableflightRightFast, AID.FableflightRightSlow, AID.FableflightLeftSlow], new AOEShapeRect(40, 2));
class CarpetRideBaited(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor Caster, Angle Side)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, s) in _casters)
        {
            var from = c.Position;
            var to = c.CastInfo!.LocXZ;
            var middle = WPos.Lerp(from, to, 0.5f);
            var dir = to - from;
            var width = dir.Length();
            yield return new(new AOEShapeRect(40, width * 0.5f), middle, dir.ToAngle() + s, Module.CastFinishAt(c.CastInfo));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CarpetRideTetherRight:
                _casters.Add((caster, -90.Degrees()));
                break;
            case AID.CarpetRideTetherLeft:
                _casters.Add((caster, 90.Degrees()));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetRideTetherRight or AID.CarpetRideTetherLeft)
        {
            _casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }
}
// TODO: i think this breaks down if someone is dead when tethers go out - does one player get two prey statuses?
class FableflightTether(BossModule module) : Components.GenericBaitAway(module)
{
    readonly Actor?[] _sources = new Actor?[4];
    readonly DateTime[] _resolve = new DateTime[4];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Generic && Raid.TryFindSlot(tether.Target, out var slot))
        {
            _sources[slot] = source;
            Init();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CarpetRideTetherRight or AID.CarpetRideTetherLeft && CurrentBaits.Count > 0)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Explosion or AID.UnmitigatedExplosion)
        {
            if (CurrentBaits.Count == 0 && FutureBaits.Count == 2)
            {
                CurrentBaits.AddRange(FutureBaits);
                FutureBaits.Clear();
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Prey && Raid.TryFindSlot(actor, out var slot))
        {
            _resolve[slot] = status.ExpireAt;
            Init();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaits)
        {
            if (b.Target != actor)
                hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
            else
            {
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                {
                    var cone = (AOEShapeCone)b.Shape;
                    var dirInverse = (cone.DirectionOffset + 180.Degrees()).Normalized();
                    hints.AddForbiddenZone(new AOEShapeCone(cone.Radius, cone.HalfAngle, dirInverse), b.Source.Position, b.Source.AngleTo(p), b.Activation);
                }
            }
        }
    }

    void Init()
    {
        if (_sources.Any(s => s == null))
            return;
        if (_resolve.Any(s => s == default))
            return;

        foreach (var (i, p) in Raid.WithSlot())
        {
            var res = _resolve[i];
            var src = _sources[i]!;

            var rota = src.CastInfo?.IsSpell(AID.FableflightRightSlow) == true ? -90.Degrees() : 90.Degrees();

            var bait = new Bait(src, p, new AOEShapeCone(50, 90.Degrees(), rota), res);

            if (res > WorldState.FutureTime(8))
                FutureBaits.Add(bait);
            else
                CurrentBaits.Add(bait);
        }
    }

    readonly List<Bait> FutureBaits = [];
}
class Explosion(BossModule module) : Components.CastTowers(module, AID.Explosion, 4);

class KindledFlameStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.KindledFlame2, 6, 5.2f);

class CharmdFlightRect(BossModule module) : Components.GroupedAOEs(module, [AID.CharmdFlightFourNightsW, AID.CharmdFlightFourNightsE], new AOEShapeRect(40, 2), highlightImminent: true);

class C013PariOfPlentyStates : StateMachineBuilder
{
    public C013PariOfPlentyStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Fireflight1(id, 8.2f);
        KindledFlame(id + 0x10000, 12.7f);
        FourLongNights(id + 0x20000, 7);
        ParisCurse(id + 0x30000, 9.2f);
        SpurningFlames(id + 0x40000, 6.8f);
        Doubling(id + 0x50000, 7.2f);
        CharmedFlight(id + 0x60000, 5.9f);
        Enrage(id + 0x70000, 8.6f);
    }

    void Fireflight1(uint id, float delay)
    {
        Cast(id, AID.HeatBurst, delay, 5, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();

        CastStartMulti(id + 0x10, [AID.FireflightByEmberlightRight, AID.FireflightByEmberlightLeft, AID.FireflightByPyrelightRight, AID.FireflightByPyrelightLeft], 10.1f)
            .ActivateOnEnter<FireflightPath>()
            .ActivateOnEnter<FireflightRectStart>()
            .ActivateOnEnter<EmberPyre>()
            .ActivateOnEnter<SunCirclet>();

        ComponentCondition<FireflightPath>(id + 0x20, 12.7f, r => r.NumCasts > 0, "Carpet 1");
        ComponentCondition<FireflightPath>(id + 0x22, 4.1f, r => r.NumCasts > 2, "Carpet 3");
        ComponentCondition<SunCirclet>(id + 0x23, 3.7f, r => r.NumCasts > 0, "Donut")
            .DeactivateOnExit<FireflightRectStart>()
            .DeactivateOnExit<FireflightPath>()
            .DeactivateOnExit<SunCirclet>();

        ComponentCondition<EmberPyre>(id + 0x30, 1.1f, e => e.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<EmberPyre>();
    }

    void KindledFlame(uint id, float delay)
    {
        // adds start casting before boss does anything
        Timeout(id, 0)
            .ActivateOnEnter<WheelOfFireflight>()
            .ActivateOnEnter<WheelRectStart>();

        CastMulti(id + 0x10, [AID.ScatteredKindlingCast, AID.KindledFlame1Cast], delay, 8)
            .ActivateOnEnter<Kindling>();

        ComponentCondition<Kindling>(id + 0x20, 0.1f, k => k.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<Kindling>();
        ComponentCondition<WheelOfFireflight>(id + 0x21, 0.1f, w => w.NumCasts > 0, "Safe corner")
            .DeactivateOnExit<WheelOfFireflight>()
            .DeactivateOnExit<WheelRectStart>();

        Cast(id + 0x30, AID.FireOfVictory, 5, 5, "Tankbuster")
            .ActivateOnEnter<FireOfVictory>()
            .DeactivateOnExit<FireOfVictory>();
    }

    void FourLongNights(uint id, float delay)
    {
        CastStartMulti(id, [AID.FireflightFourLongNightsE, AID.FireflightFourLongNightsW], delay)
            .ActivateOnEnter<FourLongNightsSide>()
            .ActivateOnEnter<FourLongNightsBait>()
            .ActivateOnEnter<LongNightRectStart>();

        ComponentCondition<FourLongNightsBait>(id + 0x10, 19.1f, b => b.NumSpreads > 0, "Bait 1")
            .DeactivateOnExit<LongNightRectStart>();
        ComponentCondition<FourLongNightsBait>(id + 0x11, 3, b => b.NumSpreads > 1, "Bait 2");
        ComponentCondition<FourLongNightsBait>(id + 0x12, 3, b => b.NumSpreads > 2, "Bait 3");
        ComponentCondition<FourLongNightsBait>(id + 0x13, 3, b => b.NumSpreads > 3, "Bait 4")
            .DeactivateOnExit<FourLongNightsSide>()
            .DeactivateOnExit<FourLongNightsBait>();
    }

    void ParisCurse(uint id, float delay)
    {
        Cast(id, AID.ParisCurse, delay, 5, "Raidwide")
            .ActivateOnEnter<ParisCurseRaidwide>()
            .ActivateOnEnter<ParisCurseStackSpread>()
            .ActivateOnEnter<FableflightLongRectStart>()
            .ActivateOnEnter<FableflightLong>()
            .ExecOnEnter<ParisCurseStackSpread>(p => p.EnableHints = false)
            .DeactivateOnExit<ParisCurseRaidwide>();

        Cast(id + 0x10, AID.CharmingBaubles, 5.2f, 3)
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<ChillingGleam>();

        Cast(id + 0x20, AID.ThievesWeave, 2.1f, 3);
        ComponentCondition<BurningGleam>(id + 0x30, 9.8f, g => g.Casters.Count > 0)
            .ExecOnExit<ParisCurseStackSpread>(p => p.EnableHints = true);

        Condition(id + 0x100, 9.2f, () => Module.FindComponent<ChillingGleam>()?.NumCasts > 0 && Module.FindComponent<BurningGleam>()?.NumCasts > 0 && Module.FindComponent<FableflightLong>()?.NumCasts > 0, "Carpet + crosses")
            .DeactivateOnExit<FableflightLongRectStart>()
            .DeactivateOnExit<FableflightLong>()
            .DeactivateOnExit<ChillingGleam>()
            .DeactivateOnExit<BurningGleam>();

        ComponentCondition<ParisCurseStackSpread>(id + 0x110, 0.4f, p => p.NumCasts > 0, "Stack + spread")
            .DeactivateOnExit<ParisCurseStackSpread>();
    }

    void SpurningFlames(uint id, float delay)
    {
        Cast(id, AID.SpurningFlames, delay, 7, "Raidwide")
            .ActivateOnEnter<SpurningFlames>()
            .DeactivateOnExit<SpurningFlames>();

        Cast(id + 0x10, AID.ImpassionedSparksCast, 5.2f, 5)
            .ActivateOnEnter<ImpassionedSparks>()
            .ActivateOnEnter<BurningPillar>();

        ComponentCondition<ImpassionedSparks>(id + 0x12, 4.5f, s => s.Casters.Count > 0, "Fireballs start");
        ComponentCondition<BurningPillar>(id + 0x13, 5.4f, b => b.Groups > 0, "First baits");
        ComponentCondition<BurningPillar>(id + 0x14, 15, b => b.Groups > 3, "Last baits");

        ComponentCondition<ChainsOfPassion>(id + 0x20, 3.7f, c => c.TethersAssigned, "Chains appear")
            .ActivateOnEnter<HotFoot>()
            .ActivateOnEnter<ChainsOfPassion>();

        ComponentCondition<HotFoot>(id + 0x30, 4.3f, h => h.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<HotFoot>()
            .DeactivateOnExit<ImpassionedSparks>();

        Cast(id + 0x100, AID.ScouringScorn, 1.3f, 6, "Raidwide")
            .ActivateOnEnter<ScouringScorn>()
            .DeactivateOnExit<ScouringScorn>()
            .DeactivateOnExit<BurningPillar>()
            .DeactivateOnExit<ChainsOfPassion>();
    }

    void Doubling(uint id, float delay)
    {
        Cast(id, AID.Doubling, delay, 3);
        Targetable(id + 0x10, false, 4.1f, "Boss disappears")
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<FableflightRectStart>()
            .ActivateOnEnter<FableflightTether>()
            .ActivateOnEnter<CarpetRideBaited>();

        ComponentCondition<Explosion>(id + 0x20, 10.2f, e => e.NumCasts > 0, "Towers 1");
        ComponentCondition<CarpetRideBaited>(id + 0x21, 0.7f, c => c.NumCasts > 0, "Baits 1");
        ComponentCondition<Explosion>(id + 0x22, 6.3f, e => e.NumCasts > 4, "Towers 2")
            .DeactivateOnExit<Explosion>();
        ComponentCondition<CarpetRideBaited>(id + 0x23, 0.7f, c => c.NumCasts > 2, "Baits 2")
            .DeactivateOnExit<CarpetRideBaited>()
            .DeactivateOnExit<FableflightRectStart>()
            .DeactivateOnExit<FableflightTether>();

        Targetable(id + 0x30, true, 0.4f, "Boss reappears");
    }

    void CharmedFlight(uint id, float delay)
    {
        CastStart(id, AID.KindledFlame2Cast, delay)
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<KindledFlameStack>();

        ComponentCondition<KindledFlameStack>(id + 0x10, 5.1f, k => k.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<KindledFlameStack>();
        ComponentCondition<BurningGleam>(id + 0x11, 0.5f, b => b.NumCasts > 0, "Crosses")
            .DeactivateOnExit<BurningGleam>();

        CastMulti(id + 0x100, [AID.CharmdFlightFourNightsW, AID.CharmdFlightFourNightsE], 7.3f, 17.5f)
            .ActivateOnEnter<BurningGleamSlow>()
            .ActivateOnEnter<FourLongNightsSide>()
            .ActivateOnEnter<CharmdFlightRect>();

        ComponentCondition<FourLongNightsSide>(id + 0x110, 2.1f, f => f.NumCasts > 0, "Side 1")
            .DeactivateOnExit<CharmdFlightRect>();
        ComponentCondition<FourLongNightsSide>(id + 0x111, 3, f => f.NumCasts > 1, "Side 2");
        ComponentCondition<FourLongNightsSide>(id + 0x112, 3, f => f.NumCasts > 2, "Side 3");
        ComponentCondition<FourLongNightsSide>(id + 0x113, 3, f => f.NumCasts > 3, "Side 4")
            .DeactivateOnExit<FourLongNightsSide>()
            .DeactivateOnExit<BurningGleamSlow>();

        Cast(id + 0x200, AID.HeatBurst, 7, 5, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();
    }

    void Enrage(uint id, float delay)
    {
        Cast(id, AID.SpurningFlamesPreEnrage, delay, 7, "Raidwide")
            .ActivateOnEnter<SpurningFlamesPreEnrage>()
            .DeactivateOnExit<SpurningFlamesPreEnrage>();

        Cast(id + 0x100, AID.ScouringScornEnrageCast, 3.2f, 7, "Enrage");
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079, NameID = 14274)]
public class C013PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760, -805), new ArenaBoundsSquare(20));

