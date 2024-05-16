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

class LionsBreath(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.LionsBreath), new AOEShapeCone(10.25f, 60.Degrees())); // TODO: verify angle
class Swinge(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.Swinge), new AOEShapeCone(40, 30.Degrees())); // TODO: verify angle

// due to relatively short casts and the fact that boss likes moving across arena to cast swinge, we always want non-tanks to be positioned slightly behind
class Positioning(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Role != Role.Tank)
            hints.AddForbiddenZone(ShapeDistance.Cone(Module.PrimaryActor.Position, 10, Module.PrimaryActor.Rotation, 90.Degrees()));
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
public class D101ChudoYudo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Square(new(0, 116), 20)];
    private static readonly List<Shape> difference = [new Square(new(-20, 136), 3, 45.Degrees()), new Square(new(20, 136), 3, 45.Degrees()), new Square(new(-20, 96), 3, 45.Degrees()),
    new Square(new(20, 96), 3, 45.Degrees()), new Rectangle(new(-4.1f, 99), 0.5f, 4, 180.Degrees()), new Circle(new(-4.5f, 96), 0.9f), new Rectangle(new(4.1f, 99), 0.5f, 4, 180.Degrees()),
    new Circle(new(4.5f, 96), 0.9f), new Square(new(7.7f, 95.9f), 0.5f), new Square(new(16.3f, 96.2f), 0.45f, 45.Degrees()), new Square(new(20, 99.8f), 0.5f, 45.Degrees()), new Square(new(20.1f, 108), 0.5f),
    new Square(new(20.1f, 116), 0.5f), new Square(new(20.1f, 124), 0.5f), new Square(new(20, 132.2f), 0.5f, 45.Degrees()), new Square(new(16.2f, 136), 0.5f, 45.Degrees()), new Square(new(7.6f, 136.2f), 0.5f),
    new Square(new(-8, 136.2f), 0.5f), new Square(new(-16.2f, 136), 0.5f, 45.Degrees()), new Square(new(-20, 132.2f), 0.5f, 45.Degrees()), new Square(new(-20.1f, 124), 0.5f), new Square(new(-20.1f, 116), 0.5f),
    new Square(new(-20.1f, 108), 0.5f), new Square(new(-16.3f, 96.2f), 0.45f, 45.Degrees()), new Square(new(-20, 99.8f), 0.5f, 45.Degrees()), new Square(new(-7.7f, 95.9f), 0.5f)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
}