namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardAerostat2;

public enum OID : uint
{
    Boss = 0x43D4, //R=2.3
    Turret = 0x41DB, //R=0.6
    SentryR7 = 0x41D6, //R=2.34
    SentryS7 = 0x4478, //R=2.34
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    AutoAttack2 = 39089, // Turret->player, no cast, single-target
    AutoAttack3 = 873, // SentryS7->player, no cast, single-target
    AutoAttack4 = 870, // SentryR7->player, no cast, single-target
    IncendiaryRing = 38452, // Boss->self, 4.8s cast, range 3-12 donut
    Electrobeam = 38060, // Turret->self, 4.0s cast, range 50 width 4 rect
    SpreadShot = 39017, // SentryS7->self, 4.0s cast, range 12 90-degree cone
}

class IncendiaryRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IncendiaryRing), new AOEShapeDonut(3, 10));
class Electrobeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrobeam), new AOEShapeRect(50, 2));
class SpreadShot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpreadShot), new AOEShapeCone(12, 90.Degrees()));

class D040VanguardAerostat2States : StateMachineBuilder
{
    public D040VanguardAerostat2States(D040VanguardAerostat2 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryRing>()
            .ActivateOnEnter<Electrobeam>()
            .ActivateOnEnter<SpreadShot>()
            .Raw.Update = () => module.Turret.All(e => e.IsDeadOrDestroyed) && module.PrimaryActor.IsDeadOrDestroyed && module.SentryS7.All(e => e.IsDeadOrDestroyed) && module.SentryG7.All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12780, SortOrder = 5)]
public class D040VanguardAerostat2 : BossModule
{
    private static readonly List<WPos> arenacoords = [new(-12.5f, -328.5f), new(12.5f, -328.5f), new(12.637f, -327.677f), new(21.5f, -327.759f),
    new(21.649f, -321.3f), new(40.939f, -321.27f), new(41.304f, -327.909f), new(87.469f, -327.561f), new(87.57f, -312.112f), new(41.304f, -312.84f),
    new(41.242f, -318.724f), new(21.684f, -318.724f), new(12.648f, -312.733f),
    new(12.5f, -311.5f), new(4.283f, -311.468f), new(3.086f, -310.288f),
    new(3, -302.5f), new(-3, -302.5f), new(-3.086f, -310.288f), new(-4.283f, -311.468f), new(-12.5f, -311.5f)];
    private static readonly ArenaBounds arena = new ArenaBoundsComplex([new PolygonCustom(arenacoords)]);
    public readonly IReadOnlyList<Actor> Turret;
    public readonly IReadOnlyList<Actor> SentryG7;
    public readonly IReadOnlyList<Actor> SentryS7;

    public D040VanguardAerostat2(WorldState ws, Actor primary) : base(ws, primary, arena.Center, arena)
    {
        Turret = Enemies(OID.Turret);
        SentryG7 = Enemies(OID.Turret);
        SentryS7 = Enemies(OID.Turret);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Turret, ArenaColor.Enemy);
        Arena.Actors(SentryS7, ArenaColor.Enemy);
        Arena.Actors(SentryG7, ArenaColor.Enemy);
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
    }
}
