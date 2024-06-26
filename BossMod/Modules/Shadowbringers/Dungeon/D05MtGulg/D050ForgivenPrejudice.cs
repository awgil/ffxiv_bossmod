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
    SanctifiedAero = 16813, // 28F1->self, 5.0s cast, range 40 width 8 rect
    PunitiveLight = 16815, // Boss->self, 5.0s cast, range 20 circle
    Sanctification = 16814, // Boss->self, 5.0s cast, range 12 90-degree cone
}

class SanctifiedAero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SanctifiedAero), new AOEShapeRect(40, 4));
class PunitiveLight(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.PunitiveLight), true, true, "Raidwide", true);
class Sanctification(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Sanctification), new AOEShapeCone(12, 45.Degrees()));

class D050ForgivenPrejudiceStates : StateMachineBuilder
{
    public D050ForgivenPrejudiceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sanctification>()
            .ActivateOnEnter<PunitiveLight>()
            .ActivateOnEnter<SanctifiedAero>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.ForgivenVenery).All(e => e.IsDead) && module.Enemies(OID.ForgivenExtortion).All(e => e.IsDead) && module.Enemies(OID.ForgivenConformity).All(e => e.IsDead) || module.Enemies(OID.ForgivenApathy).Any(e => e.InCombat) || module.Enemies(OID.ForgivenApathy).Any(e => e.IsTargetable);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8269)]

public class D050ForgivenPrejudice : SimpleBossModule
{
    public readonly IReadOnlyList<Actor> ForgivenPrejudice;
    public readonly IReadOnlyList<Actor> ForgivenExtortion;
    public readonly IReadOnlyList<Actor> ForgivenConformity;
    public readonly IReadOnlyList<Actor> ForgivenVenery;
    public readonly IReadOnlyList<Actor> ForgivenApathy;

    public D050ForgivenPrejudice(WorldState ws, Actor primary) : base(ws, primary)
    {
        ForgivenPrejudice = Enemies(OID.Boss);
        ForgivenExtortion = Enemies(OID.ForgivenExtortion);
        ForgivenConformity = Enemies(OID.ForgivenConformity);
        ForgivenVenery = Enemies(OID.ForgivenVenery);
        ForgivenApathy = Enemies(OID.ForgivenApathy);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(ForgivenPrejudice, ArenaColor.Enemy);
        Arena.Actors(ForgivenConformity, ArenaColor.Enemy);
        Arena.Actors(ForgivenExtortion, ArenaColor.Enemy);
        Arena.Actors(ForgivenVenery, ArenaColor.Enemy);
    }

    protected override bool CheckPull() => (!ForgivenApathy.Any(e => e.InCombat) || !ForgivenApathy.Any(e => e.IsTargetable)) && PrimaryActor.IsTargetable && PrimaryActor.InCombat || ForgivenExtortion.Any(e => e.InCombat) || ForgivenPrejudice.Any(e => e.InCombat) || ForgivenVenery.Any(e => e.InCombat);
}
