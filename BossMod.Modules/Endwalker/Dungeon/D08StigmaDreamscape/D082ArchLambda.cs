namespace BossMod.Endwalker.Dungeon.D08StigmaDreamscape.D082ArchLambda;

public enum OID : uint
{
    Boss = 0x3416, // R6.000, x1
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 25514, // Boss->location, no cast, single-target
    Wheel = 25525, // Boss->player, 5.0s cast, single-target, tankbuster
    AutoMobileAssaultCannon = 25515, // Boss->self, 6.0s cast, single-target, visual (charges)
    AssaultCannonTelegraph = 25518, // Helper->location, 1.5s cast, width 8 rect charge
    Tread = 25516, // Boss->location, no cast, width 8 rect charge
    TreadLast = 25517, // Boss->location, no cast, width 8 rect charge
    WaveCannon = 25519, // Boss->self, 2.5s cast, range 40 180-degree cone
    Unknown25523 = 25523, // Boss->?, 6.0s cast, TODO: not seen yet
    DragonshadowDive = 25526, // Boss->?, 3.0s cast, TODO: not seen yet
    AtomicFlame = 25524, // Boss->self, 5.0s cast, range 40 circle, raidwide
    AutoMobileSniperCannon = 25520, // Boss->location, 7.0s cast, single-target, visual (numbered line baits)
    Entrench = 25521, // Helper->self, 7.5s cast, range 41 width 8 rect
    SniperCannon = 25522, // Helper->player, no cast, range 40 width 6 rect
}

public enum IconID : uint
{
    Wheel = 218, // player
    Sniper1 = 79, // player
    Sniper2 = 80, // player
    Sniper3 = 81, // player
    Sniper4 = 82, // player
}

class Wheel(BossModule module) : Components.SingleTargetCast(module, AID.Wheel);
class WaveCannon(BossModule module) : Components.StandardAOEs(module, AID.WaveCannon, new AOEShapeCone(40, 90.Degrees()));
class AtomicFlame(BossModule module) : Components.RaidwideCast(module, AID.AtomicFlame);
class Entrench(BossModule module) : Components.StandardAOEs(module, AID.Entrench, new AOEShapeRect(41, 4));

class AssaultCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _charges = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _charges.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultCannonTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _charges.Add(new(new AOEShapeRect(dir.Length(), 4), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell, 2.9f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Tread or AID.TreadLast && _charges.Count > 0)
        {
            ++NumCasts;
            _charges.RemoveAt(0);
        }
    }
}

class SniperCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Actor?[] _targets = new Actor?[4];
    private DateTime _activation;
    private int _fired;

    private static readonly AOEShapeRect _line = new(40, 3);

    private static WPos Origin(int order) => new(0, -12 + 8 * order);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = _fired; i < _targets.Length; ++i)
            if (_targets[i] is { } t && t != actor)
                yield return new(_line, Origin(i), Angle.FromDirection(t.Position - Origin(i)), _activation.AddSeconds(0.1f * i));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = _fired; i < _targets.Length; ++i)
            if (_targets[i] == pc)
                _line.Outline(Arena, Origin(i), Angle.FromDirection(pc.Position - Origin(i)), ArenaColor.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Sniper1 and <= (uint)IconID.Sniper4 && Raid.TryFindSlot(actor, out _))
            _targets[iconID - (uint)IconID.Sniper1] = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AutoMobileSniperCannon)
            _activation = Module.CastFinishAt(spell, 0.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SniperCannon && ++_fired >= _targets.Length)
        {
            Array.Fill(_targets, null);
            _fired = 0;
        }
    }
}

class D082ArchLambdaStates : StateMachineBuilder
{
    public D082ArchLambdaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wheel>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<AtomicFlame>()
            .ActivateOnEnter<Entrench>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<SniperCannon>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10403)]
public class D082ArchLambda(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
