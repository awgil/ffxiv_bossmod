namespace BossMod.Stormblood.Raid.O10NMidgardsormr;

public enum OID : uint
{
    Boss = 0x2448, // R10.000
    BossSmall = 0x2449, // R4.550
    Helper = 0x233C, // R0.500
    Dragonfire = 0x244A, // R1.000
    AncientDragon = 0x244C, // R5.000
    ImmortalKey = 0x244D, // R1.000
}

public enum AID : uint
{
    AutoAttack = 12793, // Boss->player, no cast, single-target
    HorizontalSpin1 = 12743, // Boss->self, no cast, horizontal spin
    VerticalSpin1 = 12744, // Boss->self, no cast, vertical spin
    HorizontalSpin2 = 12745, // Boss->self, no cast, second horizontal spin
    VerticalSpin2 = 12747, // Boss->self, no cast, second vertical spin
    AzureWingsOut = 12749, // Helper->self, 1.0s cast, range 14 circle
    StygianMawIn = 12751, // Helper->self, 1.0s cast, range 10-40 donut
    BloodiedMaw = 12752, // Helper->self, 2.0s cast, range 30 width 18 cross
    EarthShaker = 12753, // Helper->self, no cast, cone on marked players
    AkhMorn = 12742, // Boss->player, 5.0s cast, range 6 circle
    AkhMornRepeat = 12795, // Boss->player, no cast, range 6 circle
    AkhMornInterval = 12873, // Boss->self, no cast, single-target
    Thunderstorm = 12754, // Helper->player, 5.0s cast, range 5 circle
    ThunderstormVisual = 12815, // Boss->self, 4.5s cast, single-target
    HorridRoar = 12755, // Helper->location, 3.5s cast, range 6 circle
    DryIce = 13874, // Helper->location, no cast, range 6 circle
    DryIceGrow = 13755, // Helper->self, 2.0s cast, range 9 circle
    DryIceGrowLarge = 13756, // Helper->self, 2.0s cast, range 12 circle
    Cauterize = 12865, // Boss->self, 4.5s cast, range 40+R width 20 rect
    ScarletThread = 13867, // Dragonfire->self, 2.0s cast, range 80+R width 4 rect
    AncientAuto = 870, // Ancient Dragon->player, no cast, single-target
    FrostBreath = 13294, // Ancient Dragon->self, no cast, range 15+R cone
    RimeWreath = 13295, // Ancient Dragon->self, 2.0s cast, range 100 circle
    Protostar = 12739, // BossSmall->self, no cast, single-target
    ProtostarRaidwide = 12757, // Helper->self, no cast, range 60 circle
    TailEnd = 12741, // Boss->player, 4.0s cast, single-target
    Exaflare = 13880, // Boss->self, 3.0s cast, single-target
    ExaflareFirst = 13871, // Helper->self, 4.0s cast, range 6 circle
    ExaflareMove = 13869, // Helper->location, no cast, range 6 circle
    NorthernCross = 13865, // Boss->self, 3.0s cast, range 60 circle
    AkhRhai = 13858, // Boss->self, 3.0s cast, single-target
    AkhRhaiFirst = 13872, // Helper->location, no cast, range 4 circle
    AkhRhaiPulse = 13860, // Helper->location, no cast, range 4 circle
}

public enum IconID : uint
{
    EarthShaker = 40, // player
    Stack = 62, // player
    DryIce = 67, // player
    Thunderstorm = 160, // player
}

public enum SID : uint
{
    ThinIce = 911, // extra/10 = slide distance
}

class AmbientAether(BossModule module) : Components.GenericAOEs(module)
{
    private enum Spin { None, Horizontal, Vertical }

    private static readonly AOEShapeCircle Out = new(14);
    private static readonly AOEShapeDonut In = new(10, 40);
    private static readonly AOEShapeCross Cross = new(30, 9);

    private Spin _first;
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is { } aoe)
            yield return aoe;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HorizontalSpin1:
                _first = Spin.Horizontal;
                break;
            case AID.VerticalSpin1:
                _first = Spin.Vertical;
                break;
            case AID.HorizontalSpin2:
                if (_first == Spin.Horizontal)
                    _aoe = new(Out, Arena.Center, default, WorldState.FutureTime(3.4f));
                break;
            case AID.VerticalSpin2:
                if (_first == Spin.Horizontal)
                    _aoe = new(In, Arena.Center, default, WorldState.FutureTime(3.4f));
                else if (_first == Spin.Vertical)
                    _aoe = new(Cross, Arena.Center, caster.Rotation, WorldState.FutureTime(3.9f));
                break;
            case AID.AzureWingsOut:
            case AID.StygianMawIn:
            case AID.BloodiedMaw:
                _aoe = null;
                _first = Spin.None;
                break;
        }
    }
}

class EarthShaker(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 15.Degrees()), (uint)IconID.EarthShaker, AID.EarthShaker, 4.2f);
class AkhMorn(BossModule module) : Components.StackWithCastTargets(module, AID.AkhMorn, 6)
{
    private int _left;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AkhMorn:
                _left = 3;
                break;
            case AID.AkhMornRepeat:
                if (--_left > 0)
                    break;
                Stacks.Clear();
                return;
            default:
                base.OnEventCast(caster, spell);
                return;
        }

        for (var i = 0; i < Stacks.Count; ++i)
        {
            var stack = Stacks[i];
            stack.Activation = WorldState.FutureTime(1.1f);
            Stacks[i] = stack;
        }
    }
}
class Thunderstorm(BossModule module) : Components.SpreadFromCastTargets(module, AID.Thunderstorm, 5);
class HorridRoar(BossModule module) : Components.StandardAOEs(module, AID.HorridRoar, 6);
class Cauterize(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(50, 10));
class ScarletThread(BossModule module) : Components.StandardAOEs(module, AID.ScarletThread, new AOEShapeRect(81, 2));
class FrostBreath(BossModule module) : Components.Cleave(module, AID.FrostBreath, new AOEShapeCone(20, 45.Degrees()), (uint)OID.AncientDragon);
class RimeWreath(BossModule module) : Components.RaidwideCast(module, AID.RimeWreath);
class TailEnd(BossModule module) : Components.SingleTargetCast(module, AID.TailEnd);
class NorthernCross(BossModule module) : Components.RaidwideCast(module, AID.NorthernCross);

class DryIceBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.DryIce, AID.DryIce, 5.2f, true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Any() && (actor.Position - Arena.Center).LengthSq() < 16 * 16)
            hints.Add("Drop ice at the edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 16), b.Activation);
    }
}

class DryIce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos Position, float Radius, DateTime Expire)> _puddles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var puddle in _puddles)
            yield return new(new AOEShapeCircle(puddle.Radius), puddle.Position, default, WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DryIce:
                _puddles.Add((caster.Position, 6, WorldState.FutureTime(40)));
                break;
            case AID.RimeWreath:
                _puddles.Clear();
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var radius = (AID)spell.Action.ID switch
        {
            AID.DryIceGrow => 9f,
            AID.DryIceGrowLarge => 12f,
            _ => 0f,
        };
        if (radius == 0)
            return;

        var index = _puddles.FindIndex(p => p.Position.AlmostEqual(caster.Position, 2));
        if (index >= 0)
            _puddles[index] = (_puddles[index].Position, radius, _puddles[index].Expire);
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _puddles.RemoveAll(p => now >= p.Expire);
    }
}

class Exaflare(BossModule module) : Components.Exaflare(module, 6, AID.ExaflareFirst)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action != WatchedAction)
            return;

        Lines.Add(new()
        {
            Next = spell.LocXZ,
            Advance = spell.Rotation.ToDirection() * 8,
            Rotation = spell.Rotation,
            NextExplosion = Module.CastFinishAt(spell),
            TimeToMove = 1.5f,
            ExplosionsLeft = 5,
            MaxShownExplosions = 5,
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.ExaflareFirst or AID.ExaflareMove))
            return;

        var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
        if (line != null)
            AdvanceLine(line, caster.Position);
    }
}

class AkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(4);
    private readonly List<(WPos Position, DateTime Expire)> _circles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var circle in _circles)
            yield return new(Shape, circle.Position, default, WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhaiFirst)
            _circles.Add((caster.Position, WorldState.FutureTime(5.2f)));
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _circles.RemoveAll(c => now >= c.Expire);
    }
}

class ThinIce(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private static float SlideDistance(Actor actor) => actor.FindStatus((uint)SID.ThinIce) is { } status ? status.Extra / 10f : 0;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var dist = SlideDistance(actor);
        if (dist > 0)
            yield return new(actor.Position, dist, default, null, actor.Rotation, Kind.DirForward);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Module.InBounds(pos))
            return true;
        foreach (var comp in Module.Components.OfType<Components.GenericAOEs>())
            if (comp.ActiveAOEs(slot, actor).Any(a => a.Risky && a.Check(pos)))
                return true;
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (SlideDistance(actor) <= 0)
            return;
        hints.Add("Slippery floor", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var dist = SlideDistance(actor);
        if (dist <= 0)
            return;

        var aoes = Module.Components.OfType<Components.GenericAOEs>().SelectMany(c => c.ActiveAOEs(slot, actor)).Where(a => a.Risky).ToList();
        if (!aoes.Any(a => a.Check(actor.Position)))
            return;

        WDir bestDir = default;
        var bestScore = float.MinValue;
        for (var i = 0; i < 16; ++i)
        {
            var dir = (i * 22.5f).Degrees().ToDirection();
            var landing = actor.Position + dist * dir;
            if (!Module.InBounds(landing) || aoes.Any(a => a.Check(landing)))
                continue;

            var score = -(landing - Module.PrimaryActor.Position).LengthSq();
            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }
        if (bestDir != default)
            hints.ForcedMovement = bestDir.ToVec3(actor.PosRot.Y);
    }
}

class MidgardsormrStates : StateMachineBuilder
{
    public MidgardsormrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AmbientAether>()
            .ActivateOnEnter<EarthShaker>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<HorridRoar>()
            .ActivateOnEnter<DryIceBait>()
            .ActivateOnEnter<DryIce>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<ScarletThread>()
            .ActivateOnEnter<FrostBreath>()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<TailEnd>()
            .ActivateOnEnter<Exaflare>()
            .ActivateOnEnter<NorthernCross>()
            .ActivateOnEnter<AkhRhai>()
            .ActivateOnEnter<ThinIce>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 588, NameID = 7641)]
public class Midgardsormr(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
