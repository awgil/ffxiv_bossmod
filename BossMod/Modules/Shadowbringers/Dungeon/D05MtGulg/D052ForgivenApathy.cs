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

//Note: this attack is a r20 circle, not drawing it because it is too big and the damage not all that high even if interrupt/stun fails
class PunitiveLight(BossModule module) : Components.CastInterruptHint(module, AID.PunitiveLight, true, true, "Raidwide", true);

class Sanctification(BossModule module) : Components.StandardAOEs(module, AID.Sanctification, new AOEShapeCone(12, 45.Degrees()));
class EarthShaker(BossModule module) : Components.StandardAOEs(module, AID.EarthShaker2, new AOEShapeCone(60, 30.Degrees()));

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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8267)]
public class D052ForgivenApathy(WorldState ws, Actor primary)
    : BossModule(ws, primary, primary.Position.X > -100 ? new(5, -198.5f) : new(-187.5f, -118), primary.Position.X > -100 ? new ArenaBoundsRect(8, 17, 105.Degrees()) : new ArenaBoundsRect(12, 21, 120.Degrees()))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ForgivenExtortion))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var e in Enemies(OID.ForgivenConformity))
            Arena.Actor(e, ArenaColor.Object);
        foreach (var e in Enemies(OID.ForgivenPrejudice))
            Arena.Actor(e, ArenaColor.Object);
    }
}
