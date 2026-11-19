namespace BossMod.Endwalker.Raid.P1NErichthonios;

public enum OID : uint
{
    Boss = 0x3521, // R9.975, x1
    Helper = 0x233C, // R0.500, x24, Helper type
    FlailLR = 0x3523, // R3.500, x5, Part type
    FlailI = 0x3524, // R3.500, x2, Part type
    Actor1E8536 = 0x1E8536, // R2.000, x1, EventObj type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    GaolersFlailLR = 26073, // Boss->self, 8.0+1.0s cast, range 60 270-degree cone (P1S shape; never does damage in Normal)
    GaolersFlailRL = 26074, // Boss->self, 8.0+1.0s cast, range 60 270-degree cone (never does damage in Normal)
    AetherflailLR = 26079, // Boss->self, 8.0+1.0s cast, (as above), then aether explosion
    AetherflailRL = 26080, // Boss->self, 8.0+1.0s cast, (as above), then aether explosion
    PitilessFlail = 26085, // Boss->player, 5.0s cast, range 60 width 6 rect, knockback
    TrueHolyVisual = 26086, // Boss->self, no cast, single-target
    TrueHoly = 26087, // Helper->players, no cast, range 6 circle stack
    ShiningCells = 26089, // Boss->self, 7.0s cast, range 60 circle, raidwide and cells arena
    SlamShut = 26090, // Boss->self, 7.0s cast, range 60 circle, raidwide and square arena
    Aetherchain = 26091, // Boss->self, 5.0s cast, single-target
    PowerfulFire = 26092, // Helper->self, no cast, red cells
    PowerfulLight = 26093, // Helper->self, no cast, blue cells
    Intemperance = 26094, // Boss->self, 2.0s cast, single-target
    IntemperateTorment = 26095, // Boss->self, 6.0s cast, single-target
    HotSpellFirst = 26096, // Helper->self, 1.5s cast, range 19 width 19 rect
    ColdSpellFirst = 26097, // Helper->self, 1.5s cast, range 19 width 19 rect
    PainfulFlux = 26098, // Helper->self, no cast, kills anyone in the 2y cross between the quadrants, 0.1s after each wave
    HeavyHand = 26099, // Boss->player, 5.0s cast, single-target
    WardersWrath = 26100, // Boss->self, 5.0s cast, range 60 circle
    Teleport = 26101, // Boss->location, no cast, single-target
    HotSpell = 27845, // Helper->self, 4.0s cast, range 19 width 19 rect
    ColdSpell = 27846, // Helper->self, 4.0s cast, range 19 width 19 rect
    PitilessFlailTarget = 27925, // Helper->player, no cast, single-target
    GaolersFlailR2 = 28066, // Helper->self, 8.7s cast, range 60 180-degree cone
    GaolersFlailL2 = 28067, // Helper->self, 8.7s cast, range 60 180-degree cone
}

public enum SID : uint
{
    ColdSpell = 2739, // Helper->player, extra=0x0
    HotSpell = 2740, // Helper->player, extra=0x0
    AetherExplosion = 2195, // none->Boss, extra=0x14C (red) or 0x14D (blue)
}

public enum IconID : uint
{
    PitilessFlail = 1, // player
    TrueHoly = 62, // player
    HeavyHand = 218, // player
}

class Flails(BossModule module) : Components.StandardAOEs(module, AID.GaolersFlailR2, new AOEShapeCone(60, 90.Degrees()));
class FlailsL(BossModule module) : Components.StandardAOEs(module, AID.GaolersFlailL2, new AOEShapeCone(60, 90.Degrees()));

class AetherExplosion(BossModule module) : Components.GenericAOEs(module, AID.PowerfulFire)
{
    private static readonly AOEShapeCone _inner = new(P1NErichthonios.InnerCircleRadius, 22.5f.Degrees());
    private static readonly AOEShapeDonutSector _outer = new(P1NErichthonios.InnerCircleRadius, 60, 22.5f.Degrees());
    private Angle? _start;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_start is not { } start)
            yield break;
        for (var i = 0; i < 4; ++i)
        {
            var dir = start + (90 * i).Degrees();
            yield return new(_inner, Arena.Center, dir + 22.5f.Degrees(), _activation);
            yield return new(_outer, Arena.Center, dir + 67.5f.Degrees(), _activation);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (actor == Module.PrimaryActor && (SID)status.ID == SID.AetherExplosion)
        {
            _start = status.Extra switch
            {
                0x14C => 45.Degrees(),
                0x14D => 0.Degrees(),
                _ => null
            };
            if (_start == null)
                ReportError($"Unexpected aether explosion param {status.Extra:X}");
            _activation = WorldState.FutureTime(5.8f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aetherchain:
                _activation = Module.CastFinishAt(spell, 0.9f);
                break;
            case AID.AetherflailLR:
            case AID.AetherflailRL:
                _activation = Module.CastFinishAt(spell, 0.8f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PowerfulFire or AID.PowerfulLight)
        {
            ++NumCasts;
            _start = null;
        }
    }
}

class PitilessFlail(BossModule module) : Components.BaitAwayCast(module, AID.PitilessFlail, new AOEShapeRect(60, 3));

class PitilessFlailKnockback(BossModule module) : Components.Knockback(module, AID.PitilessFlail)
{
    private Actor? _caster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster?.CastInfo is { } ci && ci.TargetID == actor.InstanceID)
            yield return new(_caster.Position, 11, Module.CastFinishAt(ci));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PitilessFlail)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PitilessFlail)
            _caster = null;
    }
}
class TrueHoly(BossModule module) : Components.StackWithIcon(module, (uint)IconID.TrueHoly, AID.TrueHoly, 6, 5.1f);

class Intemperance(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _quadrant = new(9.5f, 9.5f, 9.5f);
    private static readonly AOEShapeCross _cross = new(20, 1.5f);
    private static readonly WPos[] _centers = [new(110.5f, 110.5f), new(89.5f, 110.5f), new(89.5f, 89.5f), new(110.5f, 89.5f)];
    private readonly bool?[] _hot = new bool?[12];
    private int _wave;
    private DateTime _next;
    private DateTime _lastWave;
    private readonly bool?[] _casts = new bool?[4];
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active || _wave >= 3)
            yield break;
        var casting = _casts.Any(c => c != null);
        if (casting || _wave == 0)
            yield return new(_cross, Arena.Center, default, _next.AddSeconds(0.1f));
        bool? hot = actor.FindStatus(SID.HotSpell) != null ? true : actor.FindStatus(SID.ColdSpell) != null ? false : null;
        if (hot == null || !casting && _wave > 0)
            yield break;
        for (var q = 0; q < 4; ++q)
            if ((casting ? _casts[q] : _hot[3 * q + _wave]) == hot)
                yield return new(_quadrant, _centers[q], default, _next);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_active && _wave < 3 && actor.FindStatus(SID.HotSpell) != null)
            hints.Add("Go to a cold quadrant!", false);
        else if (_active && _wave < 3 && actor.FindStatus(SID.ColdSpell) != null)
            hints.Add("Go to a hot quadrant!", false);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index < 12)
        {
            switch (state)
            {
                case 0x00200010:
                    _hot[index] = true;
                    break;
                case 0x00020001:
                    _hot[index] = false;
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Intemperance:
                _active = true;
                _wave = 0;
                _next = Module.CastFinishAt(spell, 14.6f);
                break;
            case AID.HotSpellFirst:
            case AID.ColdSpellFirst:
            case AID.HotSpell:
            case AID.ColdSpell:
                _next = Module.CastFinishAt(spell);
                var off = spell.LocXZ - Arena.Center;
                _casts[off.X > 0 ? (off.Z > 0 ? 0 : 3) : (off.Z > 0 ? 1 : 2)] = (AID)spell.Action.ID is AID.HotSpellFirst or AID.HotSpell;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HotSpellFirst or AID.ColdSpellFirst or AID.HotSpell or AID.ColdSpell && WorldState.CurrentTime > _lastWave.AddSeconds(1))
        {
            _lastWave = WorldState.CurrentTime;
            Array.Fill(_casts, null);
            ++_wave;
            ++NumCasts;
            _active = _wave < 3;
            _next = WorldState.FutureTime(9);
        }
    }
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShiningCells:
                Arena.Bounds = new ArenaBoundsCircle(20);
                break;
            case AID.SlamShut:
                Arena.Bounds = new ArenaBoundsSquare(20);
                break;
        }
    }
}

class ShiningCells(BossModule module) : Components.RaidwideCast(module, AID.ShiningCells);
class SlamShut(BossModule module) : Components.RaidwideCast(module, AID.SlamShut);
class HeavyHand(BossModule module) : Components.SingleTargetCast(module, AID.HeavyHand);
class WardersWrath(BossModule module) : Components.RaidwideCast(module, AID.WardersWrath);

class P1NErichthoniosStates : StateMachineBuilder
{
    public P1NErichthoniosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Flails>()
            .ActivateOnEnter<FlailsL>()
            .ActivateOnEnter<AetherExplosion>()
            .ActivateOnEnter<PitilessFlail>()
            .ActivateOnEnter<PitilessFlailKnockback>()
            .ActivateOnEnter<TrueHoly>()
            .ActivateOnEnter<Intemperance>()
            .ActivateOnEnter<ShiningCells>()
            .ActivateOnEnter<SlamShut>()
            .ActivateOnEnter<HeavyHand>()
            .ActivateOnEnter<WardersWrath>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 808, NameID = 10345)]
public class P1NErichthonios(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20))
{
    public const float InnerCircleRadius = 11.5f;

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bounds is ArenaBoundsCircle)
        {
            var diag = Bounds.Radius / 1.414214f;
            Arena.AddCircle(Center, InnerCircleRadius, ArenaColor.Border);
            Arena.AddLine(Center + new WDir(Bounds.Radius, 0), Center - new WDir(Bounds.Radius, 0), ArenaColor.Border);
            Arena.AddLine(Center + new WDir(0, Bounds.Radius), Center - new WDir(0, Bounds.Radius), ArenaColor.Border);
            Arena.AddLine(Center + new WDir(diag, +diag), Center - new WDir(diag, +diag), ArenaColor.Border);
            Arena.AddLine(Center + new WDir(diag, -diag), Center - new WDir(diag, -diag), ArenaColor.Border);
        }
    }
}
