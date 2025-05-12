namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D101Gargant;

public enum OID : uint
{
    Boss = 0x4791, // R4.2
    SandSphere = 0x4792, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ChillingChirp = 42547, // Boss->self, 5.0s cast, range 30 circle
    AlmightyRacket = 42546, // Boss->self, 4.0s cast, range 30 width 30 rect
    AerialAmbushVisual = 42542, // Boss->location, 3.0s cast, single-target
    AerialAmbush = 42543, // Helper->self, 3.5s cast, range 30 width 15 rect
    FoundationalDebris = 43161, // Helper->location, 6.0s cast, range 10 circle
    SedimentaryDebris = 43160, // Helper->players, 5.0s cast, range 5 circle, spread
    Earthsong = 42544, // Boss->self, 5.0s cast, range 30 circle
    SphereShatter1 = 42545, // SandSphere->self, 2.0s cast, range 6 circle
    SphereShatter2 = 43135, // SandSphere->self, 2.0s cast, range 6 circle
    TrapJaws = 42548 // Boss->player, 5.0s cast, single-target
}

class AerialAmbush(BossModule module) : Components.StandardAOEs(module, AID.AerialAmbush, new AOEShapeRect(30, 7.5f));
class AlmightyRacket(BossModule module) : Components.StandardAOEs(module, AID.AlmightyRacket, new AOEShapeRect(30, 15));
class FoundationalDebris(BossModule module) : Components.StandardAOEs(module, AID.FoundationalDebris, 10);
class SedimentaryDebris(BossModule module) : Components.SpreadFromCastTargets(module, AID.SedimentaryDebris, 5);
class Earthsong(BossModule module) : Components.RaidwideCast(module, AID.Earthsong);
class ChillingChirp(BossModule module) : Components.RaidwideCast(module, AID.ChillingChirp);
class TrapJaws(BossModule module) : Components.SingleTargetDelayableCast(module, AID.TrapJaws);
class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, DateTime Activation)> _spheres = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_spheres.Count == 0)
            return [];

        var act1 = _spheres[0].Activation;
        return _spheres.TakeWhile(s => s.Activation < act1.AddSeconds(1)).Select(s => new AOEInstance(new AOEShapeCircle(6), s.Caster.Position, Activation: act1));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SandSphere)
            _spheres.Add((actor, WorldState.FutureTime(7.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SphereShatter1 or AID.SphereShatter2)
            _spheres.RemoveAll(s => s.Caster == caster);
    }
}

class D101GargantStates : StateMachineBuilder
{
    public D101GargantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AerialAmbush>()
            .ActivateOnEnter<AlmightyRacket>()
            .ActivateOnEnter<FoundationalDebris>()
            .ActivateOnEnter<SedimentaryDebris>()
            .ActivateOnEnter<Earthsong>()
            .ActivateOnEnter<ChillingChirp>()
            .ActivateOnEnter<TrapJaws>()
            .ActivateOnEnter<SphereShatter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13753)]
public class D101Gargant(WorldState ws, Actor primary) : BossModule(ws, primary, new(-248, 122), new ArenaBoundsCircle(14.5f));

