namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D101ChudoYudo;

public enum OID : uint
{
    Boss = 0x5B5, // x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    Rake = 901, // Boss->player, no cast, extra attack on tank
    LionsBreath = 902, // Boss->self, 1.0s cast, range 10.25 ?-degree cone aoe
    Swinge = 903, // Boss->self, 4.0s cast, range 40 ?-degree cone aoe
}

class LionsBreath(BossModule module) : Components.StandardAOEs(module, AID.LionsBreath, new AOEShapeCone(10.25f, 60.Degrees())); // TODO: verify angle
class Swinge(BossModule module) : Components.StandardAOEs(module, AID.Swinge, new AOEShapeCone(40, 30.Degrees())); // TODO: verify angle

// due to relatively short casts and the fact that boss likes moving across arena to cast swinge, we always want non-tanks to be positioned slightly behind
class Positioning(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Role != Role.Tank)
            hints.AddForbiddenZone(ShapeContains.Cone(Module.PrimaryActor.Position, 100, Module.PrimaryActor.Rotation, 45.Degrees()), DateTime.MaxValue);
    }
}

class D101ChudoYudoStates : StateMachineBuilder
{
    public D101ChudoYudoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LionsBreath>()
            .ActivateOnEnter<Swinge>()
            .ActivateOnEnter<Positioning>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1677)]
public class D101ChudoYudo(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 115), new ArenaBoundsSquare(20));
