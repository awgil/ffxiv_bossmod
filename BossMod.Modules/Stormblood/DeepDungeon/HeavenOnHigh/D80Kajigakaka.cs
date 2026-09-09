namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.D80Kajigakaka;

public enum OID : uint
{
    Boss = 0x23EC, // R4.500, x1
    IceBoulder = 0x23ED, // R1.500, x0 (spawn during fight)
    IcePillar = 0x23EE, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    HeavenswardHowl = 11985, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    EclipticBite = 11986, // Boss->player, no cast, single-target
    HowlingMoon = 11988, // Boss->self, no cast, single-target
    PillarImpact = 11990, // IcePillar->self, 2.5s cast, range 4+R circle
    PillarPierce = 11989, // IcePillar->self, 2.5s cast, range 80+R width 4 rect
    SphereShatter = 11992, // IceBoulder->self, no cast, range 8 circle
    LunarCry = 11987, // Boss->self, 3.0s cast, range 80+R circle
}

class LunarCry(BossModule module) : Components.RaidwideCast(module, AID.LunarCry);
class HeavenswardHowl(BossModule module) : Components.StandardAOEs(module, AID.HeavenswardHowl, new AOEShapeCone(12.5f, 60.Degrees()));
class PillarImpact(BossModule module) : Components.StandardAOEs(module, AID.PillarImpact, 6);
class PillarPierce(BossModule module) : Components.StandardAOEs(module, AID.PillarPierce, new AOEShapeRect(82, 2));
class IceBoulders(BossModule module) : Components.GenericAOEs(module, AID.SphereShatter)
{
    private readonly List<(Actor Actor, DateTime Spawn)> _boulders = [];
    const float Radius = 8;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _boulders.Select(b => new AOEInstance(new AOEShapeCircle(Radius), b.Actor.Position, default, b.Spawn.AddSeconds(11.8f)));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _boulders.RemoveAll(b => b.Actor == caster);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IceBoulder)
            _boulders.Add((actor, WorldState.CurrentTime));
    }
}

class D80KajigakakaStates : StateMachineBuilder
{
    public D80KajigakakaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavenswardHowl>()
            .ActivateOnEnter<PillarImpact>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<LunarCry>()
            .ActivateOnEnter<IceBoulders>();
    }
}

[ModuleInfo(Contributors = "Akechi", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 547, NameID = 7490)]
public class D80Kajigakaka(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(25f));

