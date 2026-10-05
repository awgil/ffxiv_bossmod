namespace BossMod.RealmReborn.Alliance.A32AngraMainyu;

public enum OID : uint
{
    Boss = 0xE00, // R3.200, x1
    Helper = 0x1B2, // R0.500
    FinalHourglass = 0xE02, // R1.500
    GrimReaper = 0xE01, // R2.000
    AngraMainyusDaewa = 0xE4E, // R1.800
    RoulettePointerNE = 0x1E9727, // → 45°
    RoulettePointerSE = 0x1E9726, // → 135°
    RoulettePointerSW = 0x1E970C, // → -135°
    RoulettePointerNW = 0x1E9728, // → -45°
    DoomPlatformNE = 0x1E9712, // R2.000
    DoomPlatformNW = 0x1E9711, // R2.000
    DoomPlatformSE = 0x1E9713, // R2.000
    DoomPlatformSW = 0x1E9714, // R2.000
}

public enum AID : uint
{
    AutoAttack = 3508, // Boss->player, no cast, single-target
    DoubleVision = 3272, // Boss->self, 2.5s cast, range 60 circle
    SullenGaze = 3273, // Helper->self, no cast, forward 180-degree half
    IrefulGaze = 3274, // Helper->self, no cast, backward 180-degree half
    Stare = 3280, // Boss->self, no cast, range 60+R width 8 rect
    Level100Flare = 3275, // Boss->location, 4.5s cast
    Level100FlareResolve = 3276, // Helper->players, no cast, range 12
    MortalGaze = 3281, // Boss->self, 3.0s cast, range 60 circle
    MortalGazeHelper = 3499, // Helper->self, 4.5s cast, range 60 circle
    Death = 3279, // GrimReaper->self, no cast, lethal roulette quarter
    Thunder = 968, // AngraMainyusDaewa->player, 1.0s cast, single-target
    EyesOnMe = 3358, // AngraMainyusDaewa->self, 4.0s cast, range 30+R circle
    Level150Death = 3277, // Boss->location, 4.5s cast
    Level150DeathResolve = 3278, // Helper->players, no cast, range 12
    Paralyze = 1118, // AngraMainyusDaewa->player, 4.0s cast, single-target
}

public enum SID : uint
{
    BrandOfTheSullen = 636, // Helper->player, extra=0x1/0x2/0x3
    BrandOfTheIreful = 637, // Helper->player, extra=0x1/0x2/0x3
    Bind = 280, // none->player, extra=0x0
    Suppuration = 375, // Helper->player, extra=0x1
    Doom = 210, // Helper->player, extra=0x0
}

public enum TetherID : uint
{
    Flare = 5, // player->player
    Death = 1, // player->player
}

public enum IconID : uint
{
    Level100Flare = 44, // player->self
    Level150Death = 45, // player->self
}

class Stare(BossModule module) : Components.Cleave(module, AID.Stare, new AOEShapeRect(63.2f, 4), activeWhileCasting: false);

class DoubleVision(BossModule module) : Components.GenericAOEs(module, AID.DoubleVision)
{
    private DateTime _activation;
    private Angle _rotation;
    private WPos _origin;

    private static readonly AOEShapeCone _half = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;

        // first hit doesn't matter
        if (actor.FindStatus(SID.BrandOfTheSullen) != null)
            yield return new(_half, _origin, _rotation, _activation);
        if (actor.FindStatus(SID.BrandOfTheIreful) != null)
            yield return new(_half, _origin, _rotation + 180.Degrees(), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DoubleVision)
        {
            _origin = caster.Position;
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SullenGaze or AID.IrefulGaze)
            _activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;
        if (actor.FindStatus(SID.BrandOfTheSullen) != null)
            hints.Add("Stand on red (Ireful) side!");
        else if (actor.FindStatus(SID.BrandOfTheIreful) != null)
            hints.Add("Stand on white (Sullen) side!");
    }
}

class MortalGaze(BossModule module) : Components.CastGaze(module, AID.MortalGaze);
class MortalGazeHelper(BossModule module) : Components.CastGaze(module, AID.MortalGazeHelper);

class Level100Flare(BossModule module) : PlayerCountCircle(module, (uint)IconID.Level100Flare, AID.Level100FlareResolve, requireMultipleOf: 2);
class Level150Death(BossModule module) : PlayerCountCircle(module, (uint)IconID.Level150Death, AID.Level150DeathResolve, requireMultipleOf: 3);

class PlayerCountCircle(BossModule module, uint iconID, AID resolve, int requireMultipleOf) : BossComponent(module)
{
    private readonly uint _iconID = iconID;
    private readonly AID _resolve = resolve;
    private readonly int _requireMultipleOf = requireMultipleOf;
    private Actor? _target;
    private DateTime _activation;

    private const float Radius = 12;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == _iconID)
        {
            _target = WorldState.Actors.Find(targetID) ?? actor;
            _activation = WorldState.FutureTime(4.5f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == _resolve)
        {
            _target = null;
            _activation = default;
        }
    }

    private int CountInside() => _target == null ? 0 : Raid.WithoutSlot().InRadius(_target.Position, Radius).Count();

    private bool IsSafe(int count) => count > 0 && count % _requireMultipleOf == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null || !actor.Position.InCircle(_target.Position, Radius))
            return;

        var count = CountInside();
        if (IsSafe(count))
            hints.Add($"Safe count ({count}) - stay", false);
        else
            hints.Add(_requireMultipleOf == 2 ? $"{count} isn't even - GTFO!" : $"{count} not divisible by 3 - GTFO!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_target == null || !actor.Position.InCircle(_target.Position, Radius))
            return;

        // stay if it's safe, leave otherwise, but forbid entering
        if (IsSafe(CountInside()))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_target.Position, Radius), _activation);
        else
            hints.AddForbiddenZone(new AOEShapeCircle(Radius), _target.Position, default, _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == null)
            return;
        var safe = IsSafe(CountInside());
        Arena.AddCircle(_target.Position, Radius, safe ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == _target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
}

class Roulette(BossModule module) : Components.GenericAOEs(module, AID.Death)
{
    private static readonly AOEShapeCone _quarter = new(40, 45.Degrees());

    private bool _active;
    private Angle? _lit;
    private DateTime _litAt;
    private Angle? _aoe;
    private DateTime _until;

    private bool Showing => _aoe != null && (_until == default || WorldState.CurrentTime < _until);
    private bool GlassesAlive => Module.Enemies(OID.FinalHourglass).Any(h => !h.IsDeadOrDestroyed);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Showing)
            yield return new(_quarter, Module.Center, _aoe!.Value);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID != OID.FinalHourglass || _active)
            return;
        _active = true;
        _lit = _aoe = null;
        _litAt = _until = default;
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state != 0x8 || !PointerDir((OID)actor.OID, out var dir))
            return;
        _lit = dir;
        _litAt = WorldState.CurrentTime;
        if (_active && !GlassesAlive)
            _aoe = dir;
    }

    public override void OnIsDeadChanged(Actor actor)
    {
        if ((OID)actor.OID == OID.FinalHourglass && actor.IsDead)
            LockIfStopped();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.FinalHourglass)
            LockIfStopped();
    }

    public override void Update()
    {
        // cycle is ~1s; a 1.3s stall means the finger has stopped (also covers non-cleared glasses)
        // cycle is about 1s, so a 1.3s stall should mean the finger stopped spinning
        if (_aoe == null && _lit != null && _active && WorldState.CurrentTime >= _litAt.AddSeconds(1.3f))
            _aoe = _lit;
    }

    private void LockIfStopped()
    {
        if (_aoe == null && _lit != null && _active && !GlassesAlive)
            _aoe = _lit;
    }

    private static bool PointerDir(OID oid, out Angle dir)
    {
        dir = oid switch
        {
            OID.RoulettePointerNE => 45.Degrees(),
            OID.RoulettePointerSE => 135.Degrees(),
            OID.RoulettePointerSW => -135.Degrees(),
            OID.RoulettePointerNW => -45.Degrees(),
            _ => default
        };
        return oid is OID.RoulettePointerNE or OID.RoulettePointerSE or OID.RoulettePointerSW or OID.RoulettePointerNW;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Death)
        {
            _aoe = spell.Rotation;
            _until = WorldState.FutureTime(3);
            _active = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_active && !Showing)
            return;
        var left = Module.Enemies(OID.FinalHourglass).Count(h => !h.IsDeadOrDestroyed);
        if (left > 0)
            hints.Add($"Roulette: kill hourglasses ({left} left)!");
        else if (Showing)
            hints.Add("Roulette: GTFO from cone!");
    }
}

class DoomPads(BossModule module) : BossComponent(module)
{
    private BitMask _dooms;
    private Actor? _activePlatform;
    private static readonly AOEShapeCircle _platformShape = new(2);
    private static readonly OID[] _platformOIDs = [OID.DoomPlatformNE, OID.DoomPlatformNW, OID.DoomPlatformSE, OID.DoomPlatformSW];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_dooms[slot])
            hints.Add("Cleanse your doom on glowy platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_dooms[slot] && _activePlatform is { } target)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target.Position, _platformShape.Radius), actor.FindStatus(SID.Doom)!.Value.ExpireAt);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_dooms[pcSlot])
            _platformShape.Draw(Arena, _activePlatform, ArenaColor.SafeFromAOE);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x4 && _platformOIDs.Contains((OID)actor.OID))
            _activePlatform = actor;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _dooms.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _dooms.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class EyesOnMe(BossModule module) : Components.StandardAOEs(module, AID.EyesOnMe, new AOEShapeCircle(31.8f));
class Paralyze(BossModule module) : Components.CastInterruptHint(module, AID.Paralyze, showNameInHint: true);
class Adds(BossModule module) : Components.AddsMulti(module, [OID.FinalHourglass, OID.GrimReaper, OID.AngraMainyusDaewa], 1);

class A32AngraMainyuStates : StateMachineBuilder
{
    public A32AngraMainyuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stare>()
            .ActivateOnEnter<DoubleVision>()
            .ActivateOnEnter<MortalGaze>()
            .ActivateOnEnter<MortalGazeHelper>()
            .ActivateOnEnter<DoomPads>()
            .ActivateOnEnter<Level100Flare>()
            .ActivateOnEnter<Level150Death>()
            .ActivateOnEnter<Roulette>()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<Paralyze>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3231)]
public class A32AngraMainyu(ModuleInit init) : BossModule(init, new(-147, 297), new ArenaBoundsCircle(30));
