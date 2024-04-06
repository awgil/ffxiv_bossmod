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
    EarthShaker = 16244, // 28F0->self, 5,0s cast, single-target
    EarthShaker2 = 16245, // 233C->self, 5,0s cast, range 60 60-degree cone
    Sanctification = 16814, // 28F2->self, 5,0s cast, range 12 90-degree cone
    PunitiveLight = 16815, // 28F2->self, 5,0s cast, range 20 circle
};

class PunitiveLight : Components.CastInterruptHint
{ //Note: this attack is a r20 circle, not drawing it because it is too big and the damage not all that high even if interrupt/stun fails
    public PunitiveLight() : base(ActionID.MakeSpell(AID.PunitiveLight), true, true, "Raidwide", true) { }
}

class Sanctification : Components.SelfTargetedAOEs
{
    public Sanctification() : base(ActionID.MakeSpell(AID.Sanctification), new AOEShapeCone(12, 45.Degrees())) { }
}

class EarthShaker : Components.SelfTargetedAOEs
{
    public EarthShaker() : base(ActionID.MakeSpell(AID.EarthShaker2), new AOEShapeCone(60, 30.Degrees())) { }
}

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
public class D052ForgivenApathy : BossModule
{
    public D052ForgivenApathy(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 0)) { }
    protected override void UpdateModule()
    {
        if (PrimaryActor.Position.AlmostEqual(new(-11, -193), 1))
            Arena.Bounds = new ArenaBoundsRect(new(5, -198.5f), 8, 17, 105.Degrees());
        if (PrimaryActor.Position.AlmostEqual(new(-204, -106), 1))
            Arena.Bounds = new ArenaBoundsRect(new(-187.5f, -118), 12, 21, 120.Degrees());
    }
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
