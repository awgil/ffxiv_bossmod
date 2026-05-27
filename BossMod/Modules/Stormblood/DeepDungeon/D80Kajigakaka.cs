namespace BossMod.Stormblood.DeepDungeon.D80Kajigakaka;

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
class HeavenswardHowl(BossModule module) : Components.StandardAOEs(module, AID.HeavenswardHowl, new AOEShapeCone(12.5f, 60.Degrees()), warningText: "Get out of boss cone AOE!");
class PillarImpact(BossModule module) : Components.StandardAOEs(module, AID.PillarImpact, 6f, warningText: "Get out of Ice Pillar AOE!");
class PillarPierce(BossModule module) : Components.StandardAOEs(module, AID.PillarPierce, new AOEShapeRect(82f, 2f), warningText: "Get out of Ice Pillar line AOE!");
class IceBoulders(BossModule module) : Components.GenericAOEs(module, warningText: "Get away from Ice Boulders!")
{
    private readonly List<Actor> _boulders = [];
    private static readonly AOEShapeCircle _shape = new(8f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _boulders.RemoveAll(b => b.IsDead);

        foreach (var b in _boulders)
            yield return new AOEInstance(_shape, b.Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IceBoulder)
            _boulders.Add(actor);
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

