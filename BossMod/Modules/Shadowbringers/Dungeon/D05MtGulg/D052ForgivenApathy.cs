namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D052ForgivenApathy;

public enum OID : uint
{
    Boss = 0x28F0, //R=8.4
    Helper = 0x233C, //R=0.5
    //trash that can be pulled into to miniboss
    ForgivenConformity = 0x28EE, //R=1.65
    ForgivenExtortion = 0x28EF, //R=2.7
    ForgivenPrejudice = 0x28F2, //R=3.6
}

public enum AID : uint
{
    AutoAttack = 870, // 28EE/28EF->player, no cast, single-target
    AutoAttack2 = 872, // 28F2->player, no cast, single-target
    RavenousBite = 16812, // 28EF->player, no cast, single-target
    AetherialPull = 16242, // 28F0->self, no cast, single-target
    AetherialPull2 = 16243, // 233C->self, no cast, range 50 circle, pull 50 between hitboxes, can most likely be ignored
    EarthShaker = 16244, // 28F0->self, 5.0s cast, single-target
    EarthShaker2 = 16245, // 233C->self, 5.0s cast, range 60 60-degree cone
    Sanctification = 16814, // 28F2->self, 5.0s cast, range 12 90-degree cone
    PunitiveLight = 16815, // 28F2->self, 5.0s cast, range 20 circle
}

class PunitiveLight(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.PunitiveLight), true, true, "Raidwide", true);
class Sanctification(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Sanctification), new AOEShapeCone(12, 45.Degrees()));
class EarthShaker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EarthShaker2), new AOEShapeCone(60, 30.Degrees()));

class D052ForgivenApathyStates : StateMachineBuilder
{
    public D052ForgivenApathyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sanctification>()
            .ActivateOnEnter<PunitiveLight>()
            .ActivateOnEnter<EarthShaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8267)]
public class D052ForgivenApathy : BossModule
{
    public D052ForgivenApathy(WorldState ws, Actor primary) : base(ws, primary, primary.Position.Z > -106 ? arena2.Center : arena1.Center, primary.Position.Z > -106 ? arena2 : arena1)
    {
        ForgivenPrejudice = Enemies(OID.ForgivenPrejudice);
        ForgivenExtortion = Enemies(OID.ForgivenExtortion);
        ForgivenConformity = Enemies(OID.ForgivenConformity);
    }

    private static readonly List<WPos> arenacoords1 = [new(21, -215.1f), new(16.3f, -213.2f), new(9.8f, -208.8f), new(4.4f, -206.3f), new(0.2f, -204.8f), new(-9.6f, -202.8f), new(-10, -202.5f),
    new(-10.7f, -201.9f), new(-11.5f, -201.4f), new(-13.2f, -200.9f), new(-8.1f, -186.8f), new(-3.7f, -188.5f), new(-1.9f, -188.7f), new(2.5f, -190f), new(9.3f, -193.5f), new(18.8f, -198.8f),
    new(27.1f, -203.2f)];

    private static readonly List<WPos> arenacoords2 = [new(-176, -138.1f), new(-199.1f, -124.8f), new(-197.3f, -121.5f), new(-204.15f, -117.3f), new(-204f, -116.1f), new(-205, -114.5f),
    new(-205.3f, -114.3f), new(-205.2f, -112.4f), new(-205.5f, -111.9f), new(-206.6f, -111f), new(-207f, -110.6f), new(-198.8f, -98.4f), new(-190, -103.5f), new(-190, -104.5f),
    new(-187.1f, -106.1f), new(-186.3f, -105.7f), new(-177.1f, -111.1f), new(-177.2f, -111.7f), new(-174, -113.6f), new(-173.3f, -113.2f), new(-164.1f, -118.5f)];

    public static readonly ArenaBounds arena1 = new ArenaBoundsComplex([new PolygonCustom(arenacoords1)]);
    public static readonly ArenaBounds arena2 = new ArenaBoundsComplex([new PolygonCustom(arenacoords2)]);

    public readonly IReadOnlyList<Actor> ForgivenPrejudice;
    public readonly IReadOnlyList<Actor> ForgivenExtortion;
    public readonly IReadOnlyList<Actor> ForgivenConformity;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(ForgivenPrejudice, ArenaColor.Enemy);
        Arena.Actors(ForgivenConformity, ArenaColor.Enemy);
        Arena.Actors(ForgivenExtortion, ArenaColor.Enemy);
    }
}
