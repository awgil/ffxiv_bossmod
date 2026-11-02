namespace BossMod.Endwalker.Variant.V02MountRokkon.V024Shishio;

public enum OID : uint
{
    Boss = 0x3F40, // R3.450, x1
    Raiun = 0x3F41, // R0.800, x0 (spawn during fight)
    Rairin = 0x3F42, // R1.000, x0 (spawn during fight)
    CleverThrallBaboons = 0x3F43, // R1.800, x0 (spawn during fight)
    FeralThrallTigers = 0x3F44, // R3.250, x0 (spawn during fight)
    VenomousThrallSnakes = 0x3F45, // R1.440, x0 (spawn during fight)
    DevilishThrall = 0x3F46, // R2.000, x0 (spawn during fight)
    HauntingThrall = 0x3F47, // R2.000, x0 (spawn during fight)
    Iwakura = 0x1EB8BD, // R0.500, EventObj type
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 34526, // Boss->player, no cast, single-target
    Teleport = 33783, // Boss->location, no cast, single-target
    StormcloudSummons = 33751, // Boss->self, 3.0s cast, single-target
    SmokeaterFirst = 33752, // Boss->self, 2.5s cast, single-target
    SmokeaterRest = 33753, // Boss->self, no cast, single-target
    SmokeaterAbsorb = 33754, // Raiun->Boss, no cast, single-target
    LeapingLevin1 = 33758, // Raiun->self, 1.0s cast, range 8 circle
    LeapingLevin2 = 33759, // Raiun->self, 1.5s cast, range 12 circle
    LeapingLevin3 = 33760, // Raiun->self, 2.5s cast, range 23 circle
    NoblePursuit = 33766, // Boss->location, 8.0s cast, width 12 rect charge
    Levinburst = 33767, // Rairin->self, no cast, range 10 width 40 rect
    Enkyo = 33781, // Boss->self, 5.0s cast, range 60 circle, raidwide
    SplittingCry = 33782, // Boss->self/player, 5.0s cast, range 60 width 14 rect, tankbuster
    OnceOnRokujoVisual = 33755, // Boss->self, 7.5s cast, single-target
    OnceOnRokujo = 33757, // Helper->self, 8.0s cast, range 60 width 14 rect
    TwiceOnRokujoVisual1 = 34723, // Boss->self, 7.5s cast, single-target
    TwiceOnRokujoVisual2 = 34724, // Boss->self, no cast, single-target
    TwiceOnRokujo = 34725, // Helper->self, 8.0s cast, range 60 width 14 rect
    ThriceOnRokujoVisual1 = 34726, // Boss->self, 7.5s cast, single-target
    ThriceOnRokujoVisual2 = 34731, // Boss->self, no cast, single-target
    ThriceOnRokujo = 34732, // Helper->self, 8.0s cast, range 60 width 14 rect
    ThunderOnefoldVisual = 33761, // Boss->self, 3.0s cast, single-target
    ThunderOnefold = 33762, // Helper->location, 4.0s cast, range 6 circle
    ThunderTwofoldVisual = 34733, // Boss->self, 3.0s cast, single-target
    ThunderTwofold = 34809, // Helper->location, 4.0s cast, range 6 circle
    ThunderThreefoldVisual = 34810, // Boss->self, 3.0s cast, single-target
    ThunderThreefold = 34811, // Helper->location, 4.0s cast, range 6 circle
    CloudToCloud1 = 33763, // Raiun->self, 2.5s cast, range 100 width 2 rect
    CloudToCloud2 = 33764, // Raiun->self, 3.0s cast, range 100 width 6 rect
    CloudToCloud3 = 33765, // Raiun->self, 4.0s cast, range 100 width 12 rect
    HauntingCry = 33768, // Boss->self, 3.0s cast, single-target, route 8 adds
    ThunderVortex = 33780, // Boss->self, 5.0s cast, range 6-30 donut
    UnsagelySpin = 33773, // VenomousThrallSnakes->self, 4.0s cast, range 6 circle
    Rush = 33774, // FeralThrallTigers->location, 6.0s cast, width 8 rect charge
    Vasoconstrictor = 33775, // VenomousThrallSnakes->location, 3.0s cast, range 5 circle
    FocusedTremor = 33769, // Helper->self, 5.0s cast, range 60 circle, route 9
    YokiVisual = 33770, // Boss->self, 3.0s cast, single-target
    Yoki = 33771, // Helper->self, 4.0s cast, range 6 circle
    YokiUzu = 33772, // Boss->self, 8.0s cast, range 23 circle
    RightSwipe = 33776, // DevilishThrall->self, 8.0s cast, range 40 180-degree cone, route 10
    LeftSwipe = 33777, // DevilishThrall->self, 8.0s cast, range 40 180-degree cone
    ReishoVisual = 34603, // HauntingThrall->self, 4.0s cast, single-target, route 11 ghosts tether players
    ReishoFirst = 34604, // Helper->self, 4.0s cast, range 6 circle
    ReishoRest = 33778, // Helper->self, no cast, range 6 circle
}

public enum SID : uint
{
    SixFulmsUnder = 567, // none->player, extra=0xC27-0xC32
}

public enum IconID : uint
{
    Tankbuster = 471, // player
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StormcloudSummons)
            _aoe = new(new AOEShapeDonut(20, 28), Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x64)
            return;
        if (state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = new ArenaBoundsCircle(20);
        }
        else if (state == 0x00080004)
            Arena.Bounds = new ArenaBoundsSquare(20);
    }
}

class FocusedTremorYokiUzu(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _quicksand = new(30, 20);
    private static readonly AOEShapeDonut _safe = new(23, 40);
    private AOEInstance? _quicksandAOE;
    private AOEInstance? _yokiUzu;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_yokiUzu is { } y)
            yield return y;
        else if (_quicksandAOE is { } q)
            yield return q with { Activation = actor.FindStatus(SID.SixFulmsUnder)?.ExpireAt ?? DateTime.MaxValue };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_yokiUzu is { } y)
        {
            if (y.Check(actor.Position))
                hints.Add("Go into quicksand to avoid AOE!");
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var expireAt = actor.FindStatus(SID.SixFulmsUnder)?.ExpireAt ?? DateTime.MaxValue;
        if (_yokiUzu != null && (expireAt - WorldState.CurrentTime).TotalSeconds <= 8)
            hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, actor, ActionQueue.Priority.High);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.YokiUzu)
            _yokiUzu = new(_safe, spell.LocXZ, default, Module.CastFinishAt(spell), ArenaColor.SafeFromAOE, Inverted: true);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.YokiUzu)
            _yokiUzu = null;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var center = Arena.Center;
        if (state == 0x00020001)
        {
            _quicksandAOE = index switch
            {
                0x67 => new(_quicksand, center - new WDir(20, 0), 90.Degrees()),
                0x65 => new(_quicksand, center + new WDir(20, 0), -90.Degrees()),
                0x66 => new(_quicksand, center - new WDir(0, 20)),
                0x68 => new(_quicksand, center + new WDir(0, 20), 180.Degrees()),
                _ => _quicksandAOE
            };
        }
        else if (state == 0x00080004 && index is >= 0x65 and <= 0x68)
        {
            _quicksandAOE = null;
        }
    }
}

class Levinburst(BossModule module) : Components.GenericAOEs(module, AID.Levinburst)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Rairin)
            _aoes.Add(new(new AOEShapeRect(5, 20, 5), actor.Position, actor.Rotation, WorldState.FutureTime(6.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

class Reisho(BossModule module) : Components.Voidzone(module, 6, OID.HauntingThrall, moveHintLength: 10)
{
    private bool _started;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _started && NumCasts < 20 ? base.ActiveAOEs(slot, actor) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ReishoFirst)
            _started = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ReishoRest)
        {
            ++NumCasts;
        }
        else if ((AID)spell.Action.ID == AID.ThunderVortex)
        {
            _started = false;
            NumCasts = 0;
        }
    }
}

class Rokujo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _clouds = [];
    private readonly List<(Actor Cloud, AOEInstance AOE)> _explosions = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(_explosions.Count, 6); ++i)
            yield return _explosions.Count > 2 && i < 2 ? _explosions[i].AOE with { Color = ArenaColor.Danger } : _explosions[i].AOE;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Raiun)
            _clouds.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor) => _clouds.Remove(actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SmokeaterAbsorb)
            _clouds.Remove(caster);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OnceOnRokujo:
                Chain(caster, spell, 8, 1, 1, false);
                break;
            case AID.TwiceOnRokujo:
                Chain(caster, spell, 12, 1.6f, 1.5f, true);
                break;
            case AID.ThriceOnRokujoVisual1:
                foreach (var c in _clouds)
                    _explosions.Add((c, new(new AOEShapeCircle(23), c.Position, default, Module.CastFinishAt(spell, 7.1f))));
                _clouds.Clear();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeapingLevin1 or AID.LeapingLevin2 or AID.LeapingLevin3 && _explosions.Count > 0)
        {
            ++NumCasts;
            var index = _explosions.FindIndex(e => e.Cloud == caster);
            _explosions.RemoveAt(index >= 0 ? index : 0);
        }
    }

    private void Chain(Actor caster, ActorCastInfo spell, float radius, float firstDelay, float linkDelay, bool linksTogether)
    {
        var wave = _clouds.Where(c => c.Position.InRect(caster.Position, spell.Rotation.ToDirection(), 30, 30, 7)).ToList();
        for (var link = 0; wave.Count > 0; ++link)
        {
            var delay = firstDelay + (link == 0 ? 0 : linksTogether ? linkDelay : linkDelay * link);
            foreach (var c in wave)
            {
                _explosions.Add((c, new(new AOEShapeCircle(radius), c.Position, default, Module.CastFinishAt(spell, delay))));
                _clouds.Remove(c);
            }
            var prev = wave;
            wave = [.. _clouds.Where(c => prev.Any(p => p.Position.InCircle(c.Position, radius)))];
        }
    }
}

class NoblePursuit(BossModule module) : Components.ChargeAOEs(module, AID.NoblePursuit, 6);
class Enkyo(BossModule module) : Components.RaidwideCast(module, AID.Enkyo);
class RokujoLines(BossModule module) : Components.GroupedAOEs(module, [AID.OnceOnRokujo, AID.TwiceOnRokujo, AID.ThriceOnRokujo], new AOEShapeRect(60, 7));
class CloudToCloud1(BossModule module) : Components.StandardAOEs(module, AID.CloudToCloud1, new AOEShapeRect(100, 1));
class CloudToCloud2(BossModule module) : Components.StandardAOEs(module, AID.CloudToCloud2, new AOEShapeRect(100, 3));
class CloudToCloud3(BossModule module) : Components.StandardAOEs(module, AID.CloudToCloud3, new AOEShapeRect(100, 6));
class SplittingCry(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 7), (uint)IconID.Tankbuster, AID.SplittingCry, 5, false, AIHints.PredictedDamageType.Tankbuster);
class ThunderVortex(BossModule module) : Components.StandardAOEs(module, AID.ThunderVortex, new AOEShapeDonut(8, 30));
class SmallCircles(BossModule module) : Components.GroupedAOEs(module, [AID.UnsagelySpin, AID.Yoki, AID.ThunderOnefold, AID.ThunderTwofold, AID.ThunderThreefold], new AOEShapeCircle(6));
class Rush(BossModule module) : Components.ChargeAOEs(module, AID.Rush, 4);
class Vasoconstrictor(BossModule module) : Components.StandardAOEs(module, AID.Vasoconstrictor, new AOEShapeCircle(5));
class Swipe(BossModule module) : Components.GroupedAOEs(module, [AID.RightSwipe, AID.LeftSwipe], new AOEShapeCone(40, 90.Degrees()));
class ReishoFirst(BossModule module) : Components.StandardAOEs(module, AID.ReishoFirst, new AOEShapeCircle(6));

class V024ShishioStates : StateMachineBuilder
{
    public V024ShishioStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Enkyo>()
            .ActivateOnEnter<SplittingCry>()
            .ActivateOnEnter<NoblePursuit>()
            .ActivateOnEnter<Levinburst>()
            .ActivateOnEnter<RokujoLines>()
            .ActivateOnEnter<Rokujo>()
            .ActivateOnEnter<CloudToCloud1>()
            .ActivateOnEnter<CloudToCloud2>()
            .ActivateOnEnter<CloudToCloud3>()
            .ActivateOnEnter<SmallCircles>()
            .ActivateOnEnter<ThunderVortex>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Vasoconstrictor>()
            .ActivateOnEnter<FocusedTremorYokiUzu>()
            .ActivateOnEnter<Swipe>()
            .ActivateOnEnter<ReishoFirst>()
            .ActivateOnEnter<Reisho>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12428, SortOrder = 6)]
public class V024Shishio(ModuleInit init) : BossModule(init, new(-40, -300), new ArenaBoundsSquare(20));
