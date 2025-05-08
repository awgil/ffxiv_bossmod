namespace BossMod.Dawntrail.Dungeon.D05Origenics.D051Herpekaris;

public enum OID : uint
{
    Boss = 0x4185, // R8.400, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    VasoconstrictorVoidzone = 0x1E9E3C, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36520, // Boss->location, no cast, single-target
    StridentShriek = 36519, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Vasoconstrictor = 36459, // Boss->self, 3.0+1.2s cast, single-target, visual (poison zones)
    PoisonHeartVoidzone = 36460, // Helper->location, 4.2s cast, range 2 circle voidzones
    VenomspillFirstL = 37451, // Boss->self, 5.0s cast, single-target, visual (hit left puddle)
    VenomspillFirstR = 36452, // Boss->self, 5.0s cast, single-target, visual (hit right puddle)
    VenomspillSecondR = 36453, // Boss->self, 4.0s cast, single-target, visual (hit right puddle)
    VenomspillSecondL = 36454, // Boss->self, 4.0s cast, single-target, visual (hit left puddle)
    PodBurstFirst = 38518, // Helper->location, 5.0s cast, range 6 circle
    PodBurstRest = 38519, // Helper->location, 4.0s cast, range 6 circle
    WrithingRiot = 36463, // Boss->self, 9.0s cast, single-target, visual (three sweeps)
    WrithingRiotRight = 36465, // Helper->self, 2.0s cast, range 25 210-degree cone, visual (telegraph)
    WrithingRiotLeft = 36466, // Helper->self, 2.0s cast, range 25 210-degree cone, visual (telegraph)
    WrithingRiotRear = 36467, // Helper->self, 2.0s cast, range 25 90-degree cone, visual (telegraph)
    RightSweep = 36469, // Boss->self, no cast, range 25 ?-degree cone
    LeftSweep = 36470, // Boss->self, no cast, range 25 ?-degree cone
    RearSweep = 36471, // Boss->self, no cast, range 25 ?-degree cone
    PoisonHeartSpread = 37921, // Helper->player, 8.0s cast, range 5 circle spread
    CollectiveAgony = 36473, // Boss->self/players, 5.5s cast, range 50 width 8 rect line stack
    CollectiveAgonyTargetSelect = 36474, // Helper->player, no cast, single-target, visual (target select)
    ConvulsiveCrush = 36518, // Boss->player, 5.0s cast, single-target, tankbuster
}

public enum IconID : uint
{
    PoisonHeartSpread = 345, // player
    ConvulsiveCrush = 218, // player
}

class StridentShriek(BossModule module) : Components.RaidwideCast(module, AID.StridentShriek);
class PoisonHeartVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 2, AID.PoisonHeartVoidzone, m => m.Enemies(OID.VasoconstrictorVoidzone).Where(z => z.EventState != 7), 0.5f);
class PodBurstFirst(BossModule module) : Components.StandardAOEs(module, AID.PodBurstFirst, 6);
class PodBurstRest(BossModule module) : Components.StandardAOEs(module, AID.PodBurstRest, 6);

class WrithingRiot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shapeSide = new(25, 105.Degrees());
    private static readonly AOEShapeCone _shapeRear = new(25, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 1)
            yield return _aoes[1];
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.WrithingRiotRight or AID.WrithingRiotLeft => _shapeSide,
            AID.WrithingRiotRear => _shapeRear,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, caster.Position, spell.Rotation, _aoes.Count > 0 ? _aoes[^1].Activation.AddSeconds(2) : Module.CastFinishAt(spell, 7.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RightSweep or AID.LeftSweep or AID.RearSweep && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class PoisonHeartSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.PoisonHeartSpread, 5);
class CollectiveAgony(BossModule module) : Components.SimpleLineStack(module, 4, 50, AID.CollectiveAgonyTargetSelect, AID.CollectiveAgony, 5.6f);
class ConvulsiveCrush(BossModule module) : Components.SingleTargetCast(module, AID.ConvulsiveCrush);

class D051HerpekarisStates : StateMachineBuilder
{
    public D051HerpekarisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StridentShriek>()
            .ActivateOnEnter<PoisonHeartVoidzone>()
            .ActivateOnEnter<PodBurstFirst>()
            .ActivateOnEnter<PodBurstRest>()
            .ActivateOnEnter<WrithingRiot>()
            .ActivateOnEnter<PoisonHeartSpread>()
            .ActivateOnEnter<CollectiveAgony>()
            .ActivateOnEnter<ConvulsiveCrush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12741)]
public class D051Herpekaris(WorldState ws, Actor primary) : BossModule(ws, primary, new(-88, -180), new ArenaBoundsSquare(18));
