namespace BossMod.Dawntrail.Hunt.RankA.Keheniheyamewi;

public enum OID : uint
{
    Boss = 0x43DC, // R8.500, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Scatterscourge = 39807, // Boss->self, 4.0s cast, range 10-40 donut
    BodyPress1 = 40063, // Boss->self, 4.0s cast, range 15 circle
    SlipperyScatterscourge = 38648, // Boss->self, 5.0s cast, range 20 width 10 rect, visual (charge)
    WildCharge = 39559, // Boss->self, no cast, range 20 width 10 rect
    ScatterscourgeShort = 38650, // Boss->self, 1.5s cast, range 10-40 donut (immediately after charge)
    PoisonGas = 38652, // Boss->self, 5.0s cast, range 60 circle, raidwide + march debuffs
    BodyPress2 = 38651, // Boss->self, 4.0s cast, range 15 circle (how is it different from first one? happens after first one, between poison gas and march resolve)
    MalignantMucus = 38653, // Boss->self, 5.0s cast, single-target, interruptible (casts several quick poison mucus puddles if not interrupted)
    PoisonMucus = 38654, // Boss->location, 1.0s cast, range 6 circle
}

public enum SID : uint
{
    ForwardMarch = 2161, // Boss->player, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x4/0x2/0x8/0x1
}

class Scatterscourge(BossModule module) : Components.StandardAOEs(module, AID.Scatterscourge, new AOEShapeDonut(10, 40));
class BodyPress1(BossModule module) : Components.StandardAOEs(module, AID.BodyPress1, new AOEShapeCircle(15));
class BodyPress2(BossModule module) : Components.StandardAOEs(module, AID.BodyPress2, new AOEShapeCircle(15));

class SlipperyScatterscourge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeCharge = new(20, 5);
    private static readonly AOEShapeDonut _shapeIn = new(10, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SlipperyScatterscourge)
        {
            _aoes.Add(new(_shapeCharge, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.2f)));
            _aoes.Add(new(_shapeIn, caster.Position + _shapeCharge.LengthFront * spell.Rotation.ToDirection(), default, Module.CastFinishAt(spell, 2.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WildCharge or AID.ScatterscourgeShort && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class PoisonGasRaidwide(BossModule module) : Components.RaidwideCast(module, AID.PoisonGas, "Raidwide + apply forced march");
class PoisonGasMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, (uint)SID.ForcedMarch, 5.5f);
class MalignantMucus(BossModule module) : Components.CastInterruptHint(module, AID.MalignantMucus);
class PoisonMucus(BossModule module) : Components.StandardAOEs(module, AID.PoisonMucus, 6);

class KeheniheyamewiStates : StateMachineBuilder
{
    public KeheniheyamewiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Scatterscourge>()
            .ActivateOnEnter<BodyPress1>()
            .ActivateOnEnter<BodyPress2>()
            .ActivateOnEnter<SlipperyScatterscourge>()
            .ActivateOnEnter<PoisonGasRaidwide>()
            .ActivateOnEnter<PoisonGasMarch>()
            .ActivateOnEnter<MalignantMucus>()
            .ActivateOnEnter<PoisonMucus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13401)]
public class Keheniheyamewi(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
