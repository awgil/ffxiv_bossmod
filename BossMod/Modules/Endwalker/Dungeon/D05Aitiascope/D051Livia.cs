namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D051Livia;

public enum OID : uint
{
    Boss = 0x3469, // R7.000, x1
    Helper = 0x233C, // R0.500, x12, mixed types
}

public enum AID : uint
{
    AutoAttack = 24771, // Boss->player, no cast, single-target
    AglaeaBite = 25673, // Boss->self/player, 5.0s cast, range 9 ?-degree cone //Tankbuster 

    AglaeaClimb1 = 25666, // Boss->self, 7.0s cast, single-target
    AglaeaClimb2 = 25667, // Boss->self, 7.0s cast, single-target
    AglaeaClimbAOE = 25668, // Helper->self, 7.0s cast, range 20 90-degree cone

    AglaeaShot1 = 25669, // Boss->self, 3.0s cast, single-target
    AglaeaShotAOE1 = 25670, // 346A->location, 3.0s cast, range 20 width 4 rect
    AglaeaShotAOE2 = 25671, // 346A->location, 1.0s cast, range 40 width 4 rect

    Disparagement = 25674, // Boss->self, 5.0s cast, range 40 120-degree cone

    Frustration = 25672, // Boss->self, 5.0s cast, range 40 circle //Raidwide

    IgnisAmoris = 25676, // Helper->location, 4.0s cast, range 6 circle
    IgnisOdi = 25677, // Helper->players, 5.0s cast, range 6 circle

    OdiEtAmo = 25675, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // 346A->player, extra=0x2
}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon161 = 161, // player
}

class AglaeaShot(BossModule module) : Components.GroupedAOEs(module, [AID.AglaeaShotAOE1, AID.AglaeaShotAOE2], new AOEShapeRect(40, 2));

class AglaeaClimbAOE(BossModule module) : Components.StandardAOEs(module, AID.AglaeaClimbAOE, new AOEShapeCone(20, 45.Degrees()));
class Disparagement(BossModule module) : Components.StandardAOEs(module, AID.Disparagement, new AOEShapeCone(40, 60.Degrees()));

class IgnisOdi(BossModule module) : Components.StackWithCastTargets(module, AID.IgnisOdi, 6, 8)
{

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // force AI to move to center - trust NPCs won't stack with you lol
        if (ActiveStackTargets.Contains(actor))
            hints.AddForbiddenZone(new AOEShapeDonut(3, 50), Arena.Center, activation: Stacks[0].Activation);
    }
}

class IgnisAmoris(BossModule module) : Components.StandardAOEs(module, AID.IgnisAmoris, 6);
class Frustration(BossModule module) : Components.RaidwideCast(module, AID.Frustration);
class AglaeaBite(BossModule module) : Components.SingleTargetCast(module, AID.AglaeaBite);

class D051LiviaStates : StateMachineBuilder
{
    public D051LiviaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AglaeaShot>()
            .ActivateOnEnter<AglaeaClimbAOE>()
            .ActivateOnEnter<Disparagement>()
            .ActivateOnEnter<IgnisOdi>()
            .ActivateOnEnter<IgnisAmoris>()
            .ActivateOnEnter<Frustration>()
            .ActivateOnEnter<AglaeaBite>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10290)]
public class D051Livia(WorldState ws, Actor primary) : BossModule(ws, primary, new(-6, 471), new ArenaBoundsCircle(20));
