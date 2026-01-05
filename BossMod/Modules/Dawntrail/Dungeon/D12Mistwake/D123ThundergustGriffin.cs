namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D123ThundergustGriffin;

public enum OID : uint
{
    Boss = 0x4A65, // R6.900, x1
    Helper = 0x233C, // R0.500, x41, Helper type
    BallLightning = 0x4A66, // R2.000, x0 (spawn during fight)
    ShockStorm = 0x4A67, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 45307, // Boss->player, no cast, single-target
    Thunderspark = 45291, // Boss->self, 5.0s cast, range 60 circle
    Jump = 46852, // Boss->location, no cast, single-target
    HighVolts = 45292, // Boss->self, 4.0s cast, single-target
    LightningBoltGround = 45293, // Helper->self, 5.0s cast, range 5 circle
    LightningBoltSpread = 46856, // Helper->player, 5.0s cast, range 5 circle
    Jump2 = 45294, // Boss->location, no cast, single-target
    ThunderboltPre1 = 46943, // Helper->self, 4.3s cast, range 20 width 3 rect
    ThunderboltPre2 = 46944, // Helper->self, 4.3s cast, range 16 width 3 rect
    ThunderboltPre3 = 46945, // Helper->self, 4.3s cast, range 16 width 3 rect
    ThunderingRoar = 45295, // Boss->self, 5.0s cast, single-target
    Thunderbolt1 = 45296, // Helper->self, 5.5s cast, range 92 width 6 rect
    Thunderbolt2 = 45297, // Helper->self, 5.5s cast, range 92 width 6 rect
    Thunderbolt3 = 45298, // Helper->self, 5.5s cast, range 92 width 6 rect
    GoldenTalons = 45305, // Boss->player, 5.0s cast, single-target
    FulgurousFall = 45301, // Boss->location, 6.0s cast, single-target
    Rush = 45302, // Helper->self, 6.0s cast, range 40 width 10 rect
    ElectrifyingFlight = 47022, // Helper->self, 6.2s cast, range 50 width 40 rect
    ElectrogeneticForce = 45304, // Helper->self, 9.6s cast, range 40 width 18 rect
    StormSurge = 45299, // Boss->self, 3.0s cast, range 50 width 10 rect
    ElectriflyingFlightStorm = 45300, // ShockStorm->location, 0.5s cast, single-target
}

class Thunderspark(BossModule module) : Components.RaidwideCast(module, AID.Thunderspark);
class LightningBoltGround(BossModule module) : Components.StandardAOEs(module, AID.LightningBoltGround, 5);
class LightningBoltSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.LightningBoltSpread, 5);
class Thunderbolt(BossModule module) : Components.GroupedAOEs(module, [AID.Thunderbolt2, AID.Thunderbolt1, AID.Thunderbolt3], new AOEShapeRect(92, 3));

class GoldenTalons(BossModule module) : Components.SingleTargetCast(module, AID.GoldenTalons);
class Rush(BossModule module) : Components.StandardAOEs(module, AID.Rush, new AOEShapeRect(40, 5));

class ElectrifyingFlight(BossModule module) : Components.Knockback(module)
{
    private Actor? _caster;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElectrifyingFlight)
            _caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ElectrifyingFlight)
            _caster = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            var kbDir = src.Kind switch
            {
                Kind.DirLeft => src.Direction.ToDirection().OrthoL(),
                Kind.DirRight => src.Direction.ToDirection().OrthoR(),
                _ => default
            };
            var dist = src.Distance;
            var ctr = Arena.Center;
            hints.AddForbiddenZone(p => !(p + kbDir * dist).InCircle(ctr, 20), src.Activation);
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster == null)
            yield break;

        var activation = Module.CastFinishAt(_caster.CastInfo);
        if (IsImmune(slot, activation))
            yield break;

        var dir = _caster.Rotation.ToDirection().OrthoL().Dot(_caster.DirectionTo(actor)) > 0 ? Kind.DirLeft : Kind.DirRight;

        yield return new(_caster.Position, 12, activation, Direction: _caster.Rotation, Kind: dir);
    }
}

class ElectrogeneticForce(BossModule module) : Components.StandardAOEs(module, AID.ElectrogeneticForce, new AOEShapeRect(40, 9))
{
    private readonly ElectrifyingFlight _flight = module.FindComponent<ElectrifyingFlight>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _flight.Sources(slot, actor).Any() ? [] : base.ActiveAOEs(slot, actor);
    }
}

class StormSurge(BossModule module) : Components.StandardAOEs(module, AID.StormSurge, new AOEShapeRect(50, 5));

class D123ThundergustGriffinStates : StateMachineBuilder
{
    public D123ThundergustGriffinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderspark>()
            .ActivateOnEnter<LightningBoltGround>()
            .ActivateOnEnter<LightningBoltSpread>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<GoldenTalons>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<ElectrifyingFlight>()
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<StormSurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1064, NameID = 14288)]
public class D123ThundergustGriffin(WorldState ws, Actor primary) : BossModule(ws, primary, new(281, -620), new ArenaBoundsCircle(20));

