//Note: This module exists because of the mini raidwide, to not confuse the AI
namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D050ForgivenPrejudice;

public enum OID : uint
{
    Boss = 0x28F2, //R=3.6-5.112
    ForgivenConformity = 0x28EE, //R=1.65
    ForgivenExtortion = 0x28EF, //R=2.7-4.509
    ForgivenVenery = 0x28E1, //R=1.6-2.0
    ForgivenApathy = 0x28F0, //R=8.4
}

public enum AID : uint
{
    AutoAttack = 870, // ForgivenExtortion/28F1->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    RavenousBite = 16812, // ForgivenExtortion->player, no cast, single-target
    SanctifiedAero = 16813, // 28F1->self, 5,0s cast, range 40 width 8 rect
    PunitiveLight = 16815, // Boss->self, 5,0s cast, range 20 circle
    Sanctification = 16814, // Boss->self, 5,0s cast, range 12 90-degree cone
};

class SanctifiedAero : Components.SelfTargetedAOEs
{
    public SanctifiedAero() : base(ActionID.MakeSpell(AID.SanctifiedAero), new AOEShapeRect(40, 4)) { }
}

class PunitiveLight : Components.CastInterruptHint
{ //Note: this attack is a r20 circle, not drawing it because it is too big and the damage not all that high even if interrupt/stun fails
    public PunitiveLight() : base(ActionID.MakeSpell(AID.PunitiveLight), true, true, "Raidwide", true) { }
}

class Sanctification : Components.SelfTargetedAOEs
{
    public Sanctification() : base(ActionID.MakeSpell(AID.Sanctification), new AOEShapeCone(12, 45.Degrees())) { }
}

class D050ForgivenPrejudiceStates : StateMachineBuilder
{
    public D050ForgivenPrejudiceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sanctification>()
            .ActivateOnEnter<PunitiveLight>()
            .ActivateOnEnter<SanctifiedAero>()
            .Raw.Update = () => (module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.ForgivenVenery).All(e => e.IsDead) && module.Enemies(OID.ForgivenExtortion).All(e => e.IsDead) && module.Enemies(OID.ForgivenConformity).All(e => e.IsDead)) || module.Enemies(OID.ForgivenApathy).Any(e => e.InCombat) || module.Enemies(OID.ForgivenApathy).Any(e => e.IsTargetable);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8269)]
public class D050ForgivenPrejudice : SimpleBossModule
{
    public D050ForgivenPrejudice(WorldState ws, Actor primary) : base(ws, primary) { }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        foreach (var s in Enemies(OID.Boss))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ForgivenExtortion))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ForgivenConformity))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.ForgivenVenery))
            Arena.Actor(s, ArenaColor.Enemy);

    }
    protected override bool CheckPull() { return (!Enemies(OID.ForgivenApathy).Any(e => e.InCombat) || !Enemies(OID.ForgivenApathy).Any(e => e.IsTargetable)) && PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.ForgivenExtortion).Any(e => e.InCombat) || Enemies(OID.ForgivenConformity).Any(e => e.InCombat) || Enemies(OID.ForgivenVenery).Any(e => e.InCombat); }
}
