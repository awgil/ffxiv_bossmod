namespace BossMod.Heavensward.Dungeon.D03Aery.D031Rangda;

public enum OID : uint
{
    Boss = 0xEA6, // R4.900, x?
    Rangda = 0x1144, // R0.500, x?, mixed types
    Leyak = 0xEA7, // R3.600, x?
    BlackenedStatue = 0xEA8, // R1.400, x?
    Helper = 0x233C, // x3
}

public enum AID : uint
{
    AutoAttack = 872, // EA6/1098/1093/3970->player, no cast, single-target
    ElectricPredation = 3887, // EA6->self, no cast, range 8+R ?-degree cone
    ElectricCachexia = 3889, // EA6->self, 7.0s cast, range 60+R circle
    IonosphericCharge = 3888, // EA6->self, 3.0s cast, single-target
    LightningBolt = 3893, // 1144->location, 3.0s cast, range 3 circle
    LightningRod = 2573,
    Ground = 3892, // 1144->player/EA8, no cast, single-target
    Electrocution = 3890, // EA6->self, 3.0s cast, single-target
    Electrocution2 = 3891, // 1144->self, no cast, range 60+R width 5 rect
    //Leyak
    Attack = 870, // 1091/1090/1095/1092/EA7/1097/1099/1096/109A/109B/109C/11AE->player, no cast, single-target
    Reflux = 3894, // EA7->player, no cast, single-target
}
public enum GID : uint
{
    LightningRod = 2574,
}
public enum TetherID : uint
{
    LightningRod = 6,
}

class ElectricPredation(BossModule module) : Components.Cleave(module, AID.ElectricPredation, new AOEShapeCone(8, 45.Degrees()));
class ElectricCachexia(BossModule module) : Components.StandardAOEs(module, AID.ElectricCachexia, new AOEShapeDonut(7, 60));
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, 3);
class LightningRod(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Statues => Module.Enemies(OID.BlackenedStatue).Where(e => e.FindStatus(GID.LightningRod) == null && !e.IsDead);

    private int LightningRodTarget => WorldState.Party.WithSlot().Where(x => x.Item2.FindStatus(GID.LightningRod) != null).Select(x => x.Item1).FirstOrDefault(-1);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot == LightningRodTarget)
            hints.Add("Bait tether to a statue!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot == LightningRodTarget)
        {
            var closestTower = Statues.MinBy(actor.DistanceToHitbox)!;
            hints.AddForbiddenZone(new AOEShapeDonut(5, 60), closestTower.Position);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Statues, ArenaColor.Object, true);

        if (pcSlot == LightningRodTarget)
            foreach (var enemy in Statues)
                Arena.AddCircle(enemy.Position, 6, ArenaColor.Safe);
    }
}
// electrocution also involves a random line baits mechanic but it does so little damage it's not worth hinting
class Electrocution(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Electrocution, 17, stopAtWall: true);
class Adds(BossModule module) : Components.Adds(module, (uint)OID.Leyak, 2);

class D031RangdaStates : StateMachineBuilder
{
    public D031RangdaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricPredation>()
            .ActivateOnEnter<ElectricCachexia>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<LightningRod>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<Adds>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3452)]
public class D031Rangda(WorldState ws, Actor primary) : BossModule(ws, primary, new(334.9f, -203), new ArenaBoundsCircle(26));
