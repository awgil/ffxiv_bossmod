namespace BossMod.Dawntrail.Raid.RM01NBlackCat;

public enum OID : uint
{
    Boss = 0x429C, // R3.993, x1
    Helper = 0x233C, // R0.500, x40, Helper type
    CopyCat = 0x429D, // R3.993, x1
    LeapTarget = 0x429E, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 39151, // Boss->player, no cast, single-target
    Teleport = 37640, // Boss->location, no cast, single-target

    BloodyScratch = 37696, // Boss->self, 5.0s cast, range 60 circle, raidwide
    BiscuitMaker = 37706, // Boss->player, 5.0s cast, single-target, tankbuster

    OneTwoPawRL = 37641, // Boss->self, 6.0s cast, single-target, visual (right then left)
    OneTwoPawRFirst = 37642, // Helper->self, 6.8s cast, range 60 180-degree cone
    OneTwoPawLSecond = 37643, // Helper->self, 9.8s cast, range 60 180-degree cone
    OneTwoPawLR = 37644, // Boss->self, 6.0s cast, single-target, visual (left then right)
    OneTwoPawRSecond = 37645, // Helper->self, 9.8s cast, range 60 180-degree cone
    OneTwoPawLFirst = 37646, // Helper->self, 6.8s cast, range 60 180-degree cone
    LeapingOneTwoPaw1 = 37663, // Boss->self, 7.0s cast, single-target, visual (jump, right then left)
    LeapingOneTwoPaw2 = 37664, // Boss->self, 7.0s cast, single-target, visual (jump, left then right)
    LeapingOneTwoPaw3 = 37665, // Boss->self, 7.0s cast, single-target, visual (jump, right then left)
    LeapingOneTwoPaw4 = 37666, // Boss->self, 7.0s cast, single-target, visual (jump, left then right)
    LeapingOneTwoPawJump1 = 37667, // Boss->location, no cast, single-target, visual
    LeapingOneTwoPawJump2 = 37670, // Boss->location, no cast, single-target, visual
    LeapingOneTwoPawAOE1 = 37668, // Helper->self, 0.8s cast, range 60 180-degree cone
    LeapingOneTwoPawAOE2 = 37669, // Helper->self, 2.8s cast, range 60 180-degree cone
    LeapingOneTwoPawAOE3 = 37671, // Helper->self, 2.8s cast, range 60 180-degree cone
    LeapingOneTwoPawAOE4 = 37672, // Helper->self, 0.8s cast, range 60 180-degree cone

    BlackCatCrossing = 37647, // Boss->self, 6.0s cast, single-target, visual (cones)
    BlackCatCrossingEnd = 37648, // Boss->self, 1.0s cast, single-target, visual
    BlackCatCrossingFirst = 37649, // Helper->self, 7.0s cast, range 60 45-degree cone
    BlackCatCrossingRest = 37650, // Helper->self, 7.0s cast, range 60 45-degree cone
    LeapingBlackCatCrossing1 = 37673, // Boss->self, 7.0s cast, single-target, visual (jump + cones)
    LeapingBlackCatCrossing2 = 38928, // Boss->self, 7.0s cast, single-target, visual (jump + cones)
    LeapingBlackCatCrossingJump1 = 37674, // Boss->self, no cast, single-target, visual
    LeapingBlackCatCrossingJump2 = 37675, // Boss->self, no cast, single-target, visual
    LeapingBlackCatCrossingFirst = 37676, // Helper->self, 1.0s cast, range 60 45-degree cone
    LeapingBlackCatCrossingRest = 37677, // Helper->self, 3.0s cast, range 60 45-degree cone

    ClawfulBoss = 37692, // Boss->self, 4.0+1.0s cast, single-target, visual (stack)
    Clawful = 37693, // Helper->players, 5.0s cast, range 5 circle stack

    Mouser = 37651, // Boss->self, 9.0s cast, single-target, visual (tile jumps)
    MouserJumpsEnd = 37652, // Boss->self, no cast, single-target, visual
    MouserJump = 37654, // Helper->location, no cast, single-target
    MouserDamageTile = 37653, // Helper->self, 1.0s cast, range 10 width 10 rect, visual (damage tile)
    MouserDestroyTile = 39275, // Helper->self, 1.0s cast, range 10 width 10 rect, visual (destroy tile)
    MouserAOE = 38053, // Helper->self, no cast, range 10 width 10 rect

    Copycat = 37656, // Boss->self, 3.0s cast, single-target, visual (activate clone)
    ElevateAndEviscerate = 37655, // Boss/CopyCat->player, 8.0s cast, single-target, knockback 10
    Impact = 39250, // Helper->self, no cast, range 10 width 10 rect, landing cell

    Shockwave = 37661, // Boss->self, 6.0+1.0s cast, single-target, visual (knockback)
    ShockwaveAOE = 37662, // Helper->self, 7.0s cast, range 30 circle, knockback 18

    PredaceousPounce = 39634, // Boss/CopyCat->location, 13.0s cast, single-target, visual
    PredaceousPounceJump = 37680, // Boss/CopyCat->location, no cast, single-target, visual
    PredaceousPounceTelegraphCharge1 = 37682, // Helper->location, 2.0s cast, width 6 rect charge
    PredaceousPounceTelegraphCircle1 = 37683, // Helper->self, 3.0s cast, range 11 circle
    PredaceousPounceTelegraphCharge2 = 37684, // Helper->location, 4.0s cast, width 6 rect charge
    PredaceousPounceTelegraphCircle2 = 37685, // Helper->self, 5.0s cast, range 11 circle
    PredaceousPounceTelegraphCharge3 = 37686, // Helper->location, 6.0s cast, width 6 rect charge
    PredaceousPounceTelegraphCircle3 = 37687, // Helper->self, 7.0s cast, range 11 circle
    PredaceousPounceTelegraphCharge4 = 37688, // Helper->location, 8.0s cast, width 6 rect charge
    PredaceousPounceTelegraphCircle4 = 37689, // Helper->self, 9.0s cast, range 11 circle
    PredaceousPounceTelegraphCharge5 = 37690, // Helper->location, 10.0s cast, width 6 rect charge
    PredaceousPounceTelegraphCircle5 = 37691, // Helper->self, 11.0s cast, range 11 circle
    PredaceousPounceTelegraphCharge6 = 39630, // Helper->location, 12.0s cast, width 6 rect charge
    PredaceousPounceTelegraphCircle6 = 39631, // Helper->self, 13.0s cast, range 11 circle
    PredaceousPounceChargeFirst = 39702, // Helper->location, 13.5s cast, width 6 rect charge
    PredaceousPounceCircleFirst = 39703, // Helper->self, 14.0s cast, range 11 circle
    PredaceousPounceChargeRest = 39268, // Helper->location, 1.0s cast, width 6 rect charge
    PredaceousPounceCircleRest = 37681, // Helper->self, 1.5s cast, range 11 circle

    GrimalkinGaleBoss = 37694, // Boss->self, no cast, single-target, visual (spreads)
    GrimalkinGale = 37695, // Helper->player, 5.0s cast, range 5 circle spread

    Overshadow = 37657, // Boss->player, 5.0s cast, single-target, visual (line stack)
    OvershadowTargetSelect = 26708, // Helper->player, no cast, single-target
    OvershadowAOE = 37658, // Boss->players, no cast, range 60 width 5 rect
}

public enum SID : uint
{
    CrossingCardinals = 2056, // none->Boss, extra=0x2CB (cardinal cones next) / 0x307 (mouser)
    CrossingIntercardinals = 2193, // none->Boss, extra=0x2CC (intercardinal cones next)
    Stun = 2656, // Boss/CopyCat->player, extra=0x0
}

public enum IconID : uint
{
    BiscuitMaker = 218, // player
    Clawful = 93, // player
    GrimalkinGale = 376, // player
}

public enum TetherID : uint
{
    ElevateAndEviscerateA = 267, // Boss/CopyCat->player
    ElevateAndEviscerateB = 268, // Boss/CopyCat->player
    Leap = 12, // Boss->LeapTarget
}

class BloodyScratch(BossModule module) : Components.RaidwideCast(module, AID.BloodyScratch);
class BiscuitMaker(BossModule module) : Components.SingleTargetCast(module, AID.BiscuitMaker);
class Clawful(BossModule module) : Components.StackWithCastTargets(module, AID.Clawful, 5, 4);
class GrimalkinGale(BossModule module) : Components.SpreadFromCastTargets(module, AID.GrimalkinGale, 5);

class Floor(BossModule module) : Components.GenericAOEs(module, AID.MouserAOE)
{
    public BitMask DamagedCells;
    public BitMask DestroyedCells;
    private readonly List<AOEInstance> _jumps = [];

    public static readonly AOEShapeRect CellShape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var i in DestroyedCells.SetBits())
            yield return new(CellShape, CellCenter(i));
        if (_jumps.Count == 0)
            yield break;
        var deadline = _jumps[0].Activation.AddSeconds(2.5f);
        foreach (var j in _jumps.TakeWhile(j => j.Activation < deadline).Where(j => !DestroyedCells[CellIndex(j.Origin)]))
            yield return j;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var x = -10; x <= 10; x += 10)
            Arena.AddLine(Module.Center + new WDir(x, -20), Module.Center + new WDir(x, 20), ArenaColor.Border);
        for (var z = -10; z <= 10; z += 10)
            Arena.AddLine(Module.Center + new WDir(-20, z), Module.Center + new WDir(20, z), ArenaColor.Border);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MouserDamageTile or AID.MouserDestroyTile)
        {
            _jumps.Add(new(CellShape, caster.Position, default, Module.CastFinishAt(spell, 9.5f)));
            _jumps.SortBy(j => j.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            var index = _jumps.FindIndex(j => j.Origin.AlmostEqual(caster.Position, 1));
            if (index >= 0)
                _jumps.RemoveAt(index);
            else if (_jumps.Count > 0)
                _jumps.RemoveAt(0);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index > 15)
            return;

        switch (state)
        {
            case 0x00020001:
                DamagedCells.Set(index);
                DestroyedCells.Clear(index);
                break;
            case 0x00200010:
                DamagedCells.Clear(index);
                DestroyedCells.Set(index);
                break;
            case 0x01000004:
            case 0x00800004:
            case 0x00040004:
                DamagedCells.Clear(index);
                DestroyedCells.Clear(index);
                break;
        }
    }

    public bool Unsafe(WPos pos) => !Module.InBounds(pos) || DestroyedCells[CellIndex(pos)];
    public bool NotIntact(WPos pos) => !Module.InBounds(pos) || DamagedCells[CellIndex(pos)] || DestroyedCells[CellIndex(pos)];

    public WPos CellCenter(int index) => Module.Center + new WDir(-15 + 10 * (index & 3), -15 + 10 * (index >> 2));

    public int CellIndex(WPos pos)
    {
        var off = pos - Module.Center;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private static int CoordinateIndex(float coord) => coord switch
    {
        < -10 => 0,
        < 0 => 1,
        < 10 => 2,
        _ => 3
    };
}

class OneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];
    private readonly List<AOEInstance> _aoes = [];
    private Actor? _leapTarget;
    private Angle _rotation;
    private Angle _firstDirection;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => (_aoes.Count > 0 ? _aoes : _predicted).Take(1);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapTarget)
        {
            _leapTarget = actor;
            Predict();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LeapingOneTwoPaw1:
            case AID.LeapingOneTwoPaw3:
                StartLeap(spell, -90.Degrees());
                break;
            case AID.LeapingOneTwoPaw2:
            case AID.LeapingOneTwoPaw4:
                StartLeap(spell, 90.Degrees());
                break;
            case AID.OneTwoPawRFirst:
            case AID.OneTwoPawLSecond:
            case AID.OneTwoPawRSecond:
            case AID.OneTwoPawLFirst:
            case AID.LeapingOneTwoPawAOE1:
            case AID.LeapingOneTwoPawAOE2:
            case AID.LeapingOneTwoPawAOE3:
            case AID.LeapingOneTwoPawAOE4:
                _predicted.Clear();
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.SortBy(aoe => aoe.Activation);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OneTwoPawRFirst or AID.OneTwoPawLSecond or AID.OneTwoPawRSecond or AID.OneTwoPawLFirst
            or AID.LeapingOneTwoPawAOE1 or AID.LeapingOneTwoPawAOE2 or AID.LeapingOneTwoPawAOE3 or AID.LeapingOneTwoPawAOE4)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }

    private void StartLeap(ActorCastInfo spell, Angle firstDirection)
    {
        _rotation = spell.Rotation;
        _firstDirection = firstDirection;
        _activation = Module.CastFinishAt(spell, 1.8f);
        Predict();
    }

    private void Predict()
    {
        if (_leapTarget == null || _activation == default)
            return;
        _predicted.Add(new(_shape, _leapTarget.Position, _rotation + _firstDirection, _activation));
        _predicted.Add(new(_shape, _leapTarget.Position, _rotation - _firstDirection, _activation.AddSeconds(2)));
        _leapTarget = null;
        _activation = default;
    }
}

class BlackCatCrossing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _predicted = [];
    private readonly List<AOEInstance> _aoes = [];
    private Actor? _leapTarget;
    private bool? _cardinalsFirst;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var list = _aoes.Count > 0 ? _aoes : _predicted;
        if (list.Count == 0)
            return [];
        var deadline = list[0].Activation.AddSeconds(1);
        return list.TakeWhile(aoe => aoe.Activation < deadline);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapTarget)
        {
            _leapTarget = actor;
            Predict();
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (actor != Module.PrimaryActor || _cardinalsFirst != null)
            return;
        if ((SID)status.ID == SID.CrossingCardinals && status.Extra != 0x307)
            _cardinalsFirst = true;
        else if ((SID)status.ID == SID.CrossingIntercardinals)
            _cardinalsFirst = false;
        Predict();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LeapingBlackCatCrossing1:
            case AID.LeapingBlackCatCrossing2:
                _activation = Module.CastFinishAt(spell, 2);
                Predict();
                break;
            case AID.BlackCatCrossingFirst:
            case AID.BlackCatCrossingRest:
            case AID.LeapingBlackCatCrossingFirst:
            case AID.LeapingBlackCatCrossingRest:
                _predicted.Clear();
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.SortBy(aoe => aoe.Activation);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BlackCatCrossingFirst or AID.BlackCatCrossingRest or AID.LeapingBlackCatCrossingFirst or AID.LeapingBlackCatCrossingRest)
        {
            ++NumCasts;
            var index = _aoes.FindIndex(aoe => aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index >= 0)
                _aoes.RemoveAt(index);
            if (_aoes.Count == 0)
                _cardinalsFirst = null;
        }
    }

    private void Predict()
    {
        if (_leapTarget == null || _cardinalsFirst == null || _activation == default)
            return;
        var first = _cardinalsFirst.Value ? 0.Degrees() : 45.Degrees();
        for (var i = 0; i < 4; ++i)
        {
            _predicted.Add(new(_shape, _leapTarget.Position, first + i * 90.Degrees(), _activation));
            _predicted.Add(new(_shape, _leapTarget.Position, first + 45.Degrees() + i * 90.Degrees(), _activation.AddSeconds(2)));
        }
        _predicted.SortBy(aoe => aoe.Activation);
        _leapTarget = null;
        _activation = default;
    }
}

class ElevateAndEviscerate(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    public Actor? Caster;
    public Actor? Target;
    public DateTime Activation;
    private readonly Floor? _floor = module.FindComponent<Floor>();

    public const float Distance = 10;

    public WPos Landing => Target != null && Caster != null ? AwayFromSource(Target.Position, Caster.Position, Distance) : default;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Caster != null && actor == Target)
            yield return new(Caster.Position, Distance, Activation);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _floor?.NotIntact(pos) ?? !Module.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            hints.AddForbiddenZone(Sdf.Discrete(p => DestinationUnsafe(slot, actor, AwayFromSource(p, src.Origin, src.Distance))), src.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElevateAndEviscerate)
        {
            Caster = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell, 0.3f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElevateAndEviscerate)
        {
            ++NumCasts;
            Caster = null;
            Target = null;
        }
    }
}

class ElevateAndEviscerateImpact(BossModule module) : Components.GenericAOEs(module, AID.Impact)
{
    private readonly ElevateAndEviscerate? _kb = module.FindComponent<ElevateAndEviscerate>();
    private readonly Floor? _floor = module.FindComponent<Floor>();
    private Actor? _target;
    private AOEInstance? _impact;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_impact != null && actor != _target)
            yield return _impact.Value;
        if (_floor != null && _kb?.Target is { } target && target != actor && Module.InBounds(_kb.Landing))
            yield return new(Floor.CellShape, _floor.CellCenter(_floor.CellIndex(_kb.Landing)), default, _kb.Activation.AddSeconds(3.7f), Risky: false);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElevateAndEviscerate && _floor != null && WorldState.Actors.Find(spell.TargetID) is { } target)
        {
            var landing = Components.Knockback.AwayFromSource(target.Position, caster.Position, ElevateAndEviscerate.Distance);
            if (Module.InBounds(landing))
            {
                _target = target;
                _impact = new(Floor.CellShape, _floor.CellCenter(_floor.CellIndex(landing)), default, Module.CastFinishAt(spell, 3.7f));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _impact = null;
            _target = null;
        }
    }

    public override void Update()
    {
        if (_target?.IsDead == true)
        {
            _impact = null;
            _target = null;
        }
    }
}

class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ShockwaveAOE, 18)
{
    private readonly Floor? _floor = module.FindComponent<Floor>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => _floor?.Unsafe(pos) ?? !Module.InBounds(pos);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(Sdf.Discrete(p => DestinationUnsafe(slot, actor, AwayFromSource(p, src.Origin, src.Distance))), src.Activation);
    }
}

class PredaceousPounce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _start;

    private static readonly AOEShapeCircle _circle = new(11);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in _aoes.Skip(NumCasts))
            yield return i++ < 2 ? aoe with { Color = ArenaColor.Danger } : aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var index = (AID)spell.Action.ID switch
        {
            AID.PredaceousPounceTelegraphCharge1 => 0,
            AID.PredaceousPounceTelegraphCircle1 => 1,
            AID.PredaceousPounceTelegraphCharge2 => 2,
            AID.PredaceousPounceTelegraphCircle2 => 3,
            AID.PredaceousPounceTelegraphCharge3 => 4,
            AID.PredaceousPounceTelegraphCircle3 => 5,
            AID.PredaceousPounceTelegraphCharge4 => 6,
            AID.PredaceousPounceTelegraphCircle4 => 7,
            AID.PredaceousPounceTelegraphCharge5 => 8,
            AID.PredaceousPounceTelegraphCircle5 => 9,
            AID.PredaceousPounceTelegraphCharge6 => 10,
            AID.PredaceousPounceTelegraphCircle6 => 11,
            _ => -1
        };
        if (index < 0)
            return;

        if (_aoes.Count == NumCasts)
        {
            _aoes.Clear();
            NumCasts = 0;
            _start = WorldState.CurrentTime;
        }
        var activation = _start.AddSeconds(13.5f + 0.55f * index);
        if ((index & 1) == 0)
        {
            var toDest = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(toDest.Length(), 3), caster.Position, Angle.FromDirection(toDest), activation));
        }
        else
        {
            _aoes.Add(new(_circle, caster.Position, default, activation));
        }
        _aoes.SortBy(aoe => aoe.Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PredaceousPounceChargeFirst or AID.PredaceousPounceCircleFirst or AID.PredaceousPounceChargeRest or AID.PredaceousPounceCircleRest)
            ++NumCasts;
    }
}

class Overshadow(BossModule module) : Components.GenericWildCharge(module, 2.5f, AID.OvershadowAOE, 60)
{
    private readonly ElevateAndEviscerate? _kb = module.FindComponent<ElevateAndEviscerate>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Overshadow)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell, 0.3f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.TargetID ? PlayerRole.Target : _kb?.Target == p ? PlayerRole.Ignore : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            Source = null;
            ++NumCasts;
        }
    }
}

class RM01NBlackCatStates : StateMachineBuilder
{
    public RM01NBlackCatStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Floor>();
    }

    private void SinglePhase(uint id)
    {
        BloodyScratch(id, 8.2f);
        OneTwoPaw(id + 0x10000, 7.7f);
        OneTwoPaw(id + 0x20000, 1.2f);
        BlackCatCrossing(id + 0x30000, 9.4f);
        BiscuitMaker(id + 0x40000, 1.9f);
        Clawful(id + 0x50000, 2.1f);
        Mouser(id + 0x60000, 13.5f);
        ElevateAndEviscerate(id + 0x70000, 12.8f);
        Shockwave(id + 0x80000, 4.7f);
        BloodyScratch(id + 0x90000, 2.0f);
        PredaceousPounce(id + 0xA0000, 10.6f);
        GrimalkinGale(id + 0xB0000, 7.8f);
        BiscuitMaker(id + 0xC0000, 2.2f);
        LeapingOneTwoPaw(id + 0xD0000, 8.3f);

        // TODO: Leaping Black Cat Crossing + Copycat never reached in a replay
        SimpleState(id + 0xE0000, 120, "Until enrage")
            .ActivateOnEnter<BloodyScratch>()
            .ActivateOnEnter<BiscuitMaker>()
            .ActivateOnEnter<Clawful>()
            .ActivateOnEnter<GrimalkinGale>()
            .ActivateOnEnter<OneTwoPaw>()
            .ActivateOnEnter<BlackCatCrossing>()
            .ActivateOnEnter<ElevateAndEviscerate>()
            .ActivateOnEnter<ElevateAndEviscerateImpact>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<PredaceousPounce>()
            .ActivateOnEnter<Overshadow>();
    }

    private void BloodyScratch(uint id, float delay)
    {
        Cast(id, AID.BloodyScratch, delay, 5, "Raidwide")
            .ActivateOnEnter<BloodyScratch>()
            .DeactivateOnExit<BloodyScratch>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void OneTwoPaw(uint id, float delay)
    {
        CastMulti(id, [AID.OneTwoPawRL, AID.OneTwoPawLR], delay, 6)
            .ActivateOnEnter<OneTwoPaw>();
        ComponentCondition<OneTwoPaw>(id + 0x10, 0.8f, c => c.NumCasts >= 1, "Cleave 1");
        ComponentCondition<OneTwoPaw>(id + 0x20, 3, c => c.NumCasts >= 2, "Cleave 2")
            .DeactivateOnExit<OneTwoPaw>();
    }

    private void LeapingOneTwoPaw(uint id, float delay)
    {
        CastMulti(id, [AID.LeapingOneTwoPaw1, AID.LeapingOneTwoPaw2, AID.LeapingOneTwoPaw3, AID.LeapingOneTwoPaw4], delay, 7)
            .ActivateOnEnter<OneTwoPaw>();
        ComponentCondition<OneTwoPaw>(id + 0x10, 1.8f, c => c.NumCasts >= 1, "Cleave 1");
        ComponentCondition<OneTwoPaw>(id + 0x20, 2, c => c.NumCasts >= 2, "Cleave 2")
            .DeactivateOnExit<OneTwoPaw>();
    }

    private void BlackCatCrossing(uint id, float delay)
    {
        Cast(id, AID.BlackCatCrossing, delay, 6)
            .ActivateOnEnter<BlackCatCrossing>();
        ComponentCondition<BlackCatCrossing>(id + 0x10, 1, c => c.NumCasts >= 4, "Cones 1");
        ComponentCondition<BlackCatCrossing>(id + 0x20, 3.1f, c => c.NumCasts >= 8, "Cones 2")
            .DeactivateOnExit<BlackCatCrossing>();
    }

    private void BiscuitMaker(uint id, float delay)
    {
        Cast(id, AID.BiscuitMaker, delay, 5, "Tankbuster")
            .ActivateOnEnter<BiscuitMaker>()
            .DeactivateOnExit<BiscuitMaker>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Clawful(uint id, float delay)
    {
        Cast(id, AID.ClawfulBoss, delay, 4)
            .ActivateOnEnter<Clawful>();
        ComponentCondition<Clawful>(id + 0x10, 1, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<Clawful>();
    }

    private void Mouser(uint id, float delay)
    {
        Cast(id, AID.Mouser, delay, 9);
        ComponentCondition<Floor>(id + 0x10, 1.2f, c => c.NumCasts > 0, "Jumps start");
        ComponentCondition<Floor>(id + 0x20, 8, c => c.NumCasts >= 19, "Jumps end");
    }

    private void ElevateAndEviscerate(uint id, float delay)
    {
        Cast(id, AID.ElevateAndEviscerate, delay, 8, "Knockback 1")
            .ActivateOnEnter<ElevateAndEviscerate>()
            .ActivateOnEnter<ElevateAndEviscerateImpact>();
        Cast(id + 0x10, AID.ElevateAndEviscerate, 2.2f, 8, "Knockback 2");
        Cast(id + 0x20, AID.ElevateAndEviscerate, 2.3f, 8, "Knockback 3");
        Cast(id + 0x30, AID.ElevateAndEviscerate, 2.3f, 8, "Knockback 4")
            .DeactivateOnExit<ElevateAndEviscerate>();
        ComponentCondition<ElevateAndEviscerateImpact>(id + 0x40, 3.6f, c => c.NumCasts >= 4, "Impact")
            .DeactivateOnExit<ElevateAndEviscerateImpact>();
    }

    private void Shockwave(uint id, float delay)
    {
        Cast(id, AID.Shockwave, delay, 6)
            .ActivateOnEnter<Shockwave>();
        ComponentCondition<Shockwave>(id + 0x10, 1.1f, c => c.NumCasts > 0, "Knockback")
            .DeactivateOnExit<Shockwave>();
    }

    private void PredaceousPounce(uint id, float delay)
    {
        Cast(id, AID.PredaceousPounce, delay, 13)
            .ActivateOnEnter<PredaceousPounce>()
            .ActivateOnEnter<GrimalkinGale>();
        ComponentCondition<PredaceousPounce>(id + 0x10, 0.6f, c => c.NumCasts > 0, "Pounces start");
        ComponentCondition<PredaceousPounce>(id + 0x20, 6.2f, c => c.NumCasts >= 12, "Pounces end")
            .DeactivateOnExit<PredaceousPounce>();
    }

    private void GrimalkinGale(uint id, float delay)
    {
        ComponentCondition<GrimalkinGale>(id, delay, c => c.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<GrimalkinGale>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 985, NameID = 12686)]
public class RM01NBlackCat(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
