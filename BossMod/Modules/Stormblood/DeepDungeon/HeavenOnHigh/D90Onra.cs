namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D90Onra;

public enum OID : uint
{
    Boss = 0x23EF, // R3.000, x1
    SandSphere = 0x2413, // R1.000, x0 (spawn during fight)
    Onra = 0x22A1, // R0.500, x0 (spawn during fight)
}
public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    BurningRave = 12213, // Boss->location, 3.5s cast, range 8 circle
    KnucklePress = 12216, // Boss->self, 3.0s cast, range 6+R circle
    AuraCannon = 12215, // Boss->self, 3.0s cast, range 60+R width 10 rect
    AncientQuaga = 12214, // Boss->self, 5.0s cast, range 60+R circle
    Subduction = 12210, // SandSphere->self, 3.0s cast, range 8+R circle
    MeteorImpact = 12211, // Boss->self, 3.0s cast, single-target
    MeteorImpactFlare = 12212, // Onra->location, 6.0s cast, range 60 circle
}

class BurningRave(BossModule module) : Components.StandardAOEs(module, AID.BurningRave, 8f);
class KnucklePress(BossModule module) : Components.StandardAOEs(module, AID.KnucklePress, 9f);
class AuraCannon(BossModule module) : Components.StandardAOEs(module, AID.AuraCannon, new AOEShapeRect(63f, 5f), warningText: "Get out of Aura Cannon line AOE!");
class AncientQuaga(BossModule module) : Components.RaidwideCast(module, AID.AncientQuaga, "Big Raidwide incoming!");
class Subduction(BossModule module) : Components.GenericAOEs(module, warningText: "Get away from Sand Sphere!")
{
    private readonly List<Actor> _spheres = [];
    private static readonly AOEShapeCircle _shape = new(9f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in _spheres)
            yield return new AOEInstance(_shape, s.Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SandSphere)
            _spheres.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.SandSphere)
            _spheres.Remove(actor);
    }
}
class MeteorImpact(BossModule module) : Components.GenericAOEs(module, warningText: "Get away from flare!")
{
    private readonly List<AOEInstance> _flare = [];
    private static readonly AOEShapeCircle _shape = new(20); //TODO: check actual fall-off

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _flare.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorImpactFlare)
        {
            _flare.Add(new(_shape, caster.Position, default, Module.CastFinishAt(spell)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorImpactFlare)
        {
            _flare.Clear();
        }
    }
}

class D90OnraStates : StateMachineBuilder
{
    public D90OnraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningRave>()
            .ActivateOnEnter<KnucklePress>()
            .ActivateOnEnter<AuraCannon>()
            .ActivateOnEnter<AncientQuaga>()
            .ActivateOnEnter<Subduction>()
            .ActivateOnEnter<MeteorImpact>();
    }
}

[ModuleInfo(Contributors = "Akechi", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 548, NameID = 7582)]
public class D90Onra(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(25f));

