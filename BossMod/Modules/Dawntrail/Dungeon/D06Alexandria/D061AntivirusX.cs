namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D061AntivirusX;

public enum OID : uint
{
    Boss = 0x4173, // R8.000, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    ElectricCharge = 0x18D6, // R0.500, x6
    InterferonR = 0x4174, // R1.000, x0 (spawn during fight) - donut
    InterferonC = 0x4175, // R1.000, x0 (spawn during fight) - cross
}

public enum AID : uint
{
    AutoAttack = 36388, // Boss->player, no cast, single-target
    ImmuneResponseFront = 36378, // Boss->self, 5.0s cast, single-target, visual (front cone)
    ImmuneResponseFrontAOE = 36379, // Helper->self, 6.0s cast, range 40 ?-degree cone
    ImmuneResponseBack = 36380, // Boss->self, 5.0s cast, single-target, visual (back cone)
    ImmuneResponseBackAOE = 36381, // Helper->self, 6.0s cast, range 40 ?-degree cone
    PathocircuitPurge = 36382, // InterferonR->self, 1.0s cast, range 4-40 donut
    PathocrossPurge = 36383, // InterferonC->self, 1.0s cast, range 40 width 6 cross
    Quarantine = 36384, // Boss->self, 3.0s cast, single-target, visual (tankbuster + stack)
    Disinfection = 36385, // Helper->player, no cast, range 6 circle, tankbuster
    QuarantineAOE = 36386, // Helper->none, no cast, range 6 circle stack
    Cytolysis = 36387, // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum IconID : uint
{
    Disinfection = 344, // player
    Quarantine = 62, // player
}

class PathoPurge(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeDonut _shapeDonut = new(4, 40);
    private static readonly AOEShapeCross _shapeCross = new(40, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // if next is cross and there are donuts after it, we still want to stay closer to it, to simplify getting to the next donut
        if (AOEs.Count >= 2 && AOEs[0].Shape == _shapeCross && AOEs.Skip(1).Any(aoe => aoe.Shape == _shapeDonut))
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(AOEs[0].Origin, 8), DateTime.MaxValue);
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = (OID)actor.OID switch
        {
            OID.InterferonR => _shapeDonut,
            OID.InterferonC => _shapeCross,
            _ => null
        };
        if (shape != null)
        {
            AOEs.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(9.7f + 1.7f * AOEs.Count)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PathocircuitPurge or AID.PathocrossPurge && AOEs.Count > 0)
            AOEs.RemoveAt(0);
    }
}

class ImmuneResponseFront(BossModule module) : Components.StandardAOEs(module, AID.ImmuneResponseFrontAOE, new AOEShapeCone(40, 60.Degrees())); // TODO: verify angle

class ImmuneResponseBack(BossModule module) : Components.StandardAOEs(module, AID.ImmuneResponseBackAOE, new AOEShapeCone(40, 120.Degrees())) // TODO: verify angle
{
    private readonly PathoPurge? _purge = module.FindComponent<PathoPurge>();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _purge?.AOEs.Count is 5 or 2 or 1 ? [] : base.ActiveAOEs(slot, actor); // special case to handle overlap
}

// for quarantine/disinfection, duty support always stacks at the middle
class Disinfection(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Disinfection, AID.Disinfection, 5.1f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 6), b.Activation);
    }
}

class Quarantine(BossModule module) : Components.UniformStackSpread(module, 6, 0, 3)
{
    private BitMask _forbidden;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in ActiveStacks)
            if (!s.ForbiddenPlayers[slot])
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center, 3), s.Activation); // stack neatly in center
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.Disinfection:
                _forbidden.Set(Raid.FindSlot(actor.InstanceID));
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers = _forbidden;
                break;
            case IconID.Quarantine:
                AddStack(actor, WorldState.FutureTime(5.1f), _forbidden);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.QuarantineAOE)
        {
            Stacks.Clear();
            _forbidden.Reset();
        }
    }
}

class Cytolysis(BossModule module) : Components.RaidwideCast(module, AID.Cytolysis);

class D061AntivirusXStates : StateMachineBuilder
{
    public D061AntivirusXStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PathoPurge>()
            .ActivateOnEnter<ImmuneResponseFront>()
            .ActivateOnEnter<ImmuneResponseBack>()
            .ActivateOnEnter<Disinfection>()
            .ActivateOnEnter<Quarantine>()
            .ActivateOnEnter<Cytolysis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12844)]
public class D061AntivirusX(WorldState ws, Actor primary) : BossModule(ws, primary, new(852, 823), new ArenaBoundsRect(20, 15));
