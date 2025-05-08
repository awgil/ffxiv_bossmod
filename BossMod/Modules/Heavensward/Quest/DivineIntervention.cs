namespace BossMod.Heavensward.Quest.DivineIntervention;

public enum OID : uint
{
    Boss = 0x1010,
    Helper = 0x233C,
    IshgardianSteelChain = 0x102C, // R1.000, x1
    SerPaulecrainColdfire = 0x1011, // R0.500, x1
    ThunderPicket = 0xEC4, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    LightningBolt = 3993, // EC4->E0F, 2.0s cast, width 4 rect charge
    IronTempest = 1003, // Boss->self, 3.5s cast, range 5+R circle
    Overpower = 720, // Boss->self, 2.5s cast, range 6+R 90-degree cone
    RingOfFrost = 1316, // 1011->self, 3.0s cast, range 6+R circle
    Rive = 1135, // Boss->self, 2.5s cast, range 30+R width 2 rect
    Heartstopper = 866, // 1011->self, 2.5s cast, range 3+R width 3 rect
}

class LightningBolt(BossModule module) : Components.ChargeAOEs(module, AID.LightningBolt, 2);
class IronTempest(BossModule module) : Components.StandardAOEs(module, AID.IronTempest, new AOEShapeCircle(5.5f));
class Overpower(BossModule module) : Components.StandardAOEs(module, AID.Overpower, new AOEShapeCone(6.5f, 45.Degrees()));
class RingOfFrost(BossModule module) : Components.StandardAOEs(module, AID.RingOfFrost, new AOEShapeCircle(6.5f));
class Rive(BossModule module) : Components.StandardAOEs(module, AID.Rive, new AOEShapeRect(30.5f, 1));
class Heartstopper(BossModule module) : Components.StandardAOEs(module, AID.Heartstopper, new AOEShapeRect(3.5f, 1.5f));
class Chain(BossModule module) : Components.Adds(module, (uint)OID.IshgardianSteelChain, 1);

class SerGrinnauxTheBullStates : StateMachineBuilder
{
    public SerGrinnauxTheBullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<IronTempest>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<RingOfFrost>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<Chain>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.SerPaulecrainColdfire).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67133, NameID = 3850)]
public class SerGrinnauxTheBull(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 2), FunnyBounds)
{
    public static ArenaBoundsCustom NewBounds()
    {
        var arc = CurveApprox.CircleArc(new(3.6f, 0), 11.5f, 0.Degrees(), 180.Degrees(), 0.01f);
        var arc2 = CurveApprox.CircleArc(new(-3.6f, 0), 11.5f, 180.Degrees(), 360.Degrees(), 0.01f);

        return new(16, new(arc.Concat(arc2).Select(a => a.ToWDir())));
    }

    public static readonly ArenaBoundsCustom FunnyBounds = NewBounds();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
    }
}
