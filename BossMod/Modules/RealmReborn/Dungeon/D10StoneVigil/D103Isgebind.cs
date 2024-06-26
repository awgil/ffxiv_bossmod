namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D103Isgebind;

public enum OID : uint
{
    Boss = 0x5AF, // x1
    Helper = 0x233C, // x8
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    RimeWreath = 1025, // Boss->self, 3.0s cast, raidwide
    FrostBreath = 1022, // Boss->self, 1.0s cast, range 27 ?-degree cone cleave
    SheetOfIce = 1023, // Boss->location, 2.5s cast, range 5 aoe
    SheetOfIce2 = 1024, // Helper->location, 3.0s cast, range 5 aoe
    Cauterize = 1026, // Boss->self, 4.0s cast, range 48 width 20 rect aoe
    Touchdown = 1027, // Boss->self, no cast, range 5 aoe around center
}

class RimeWreath(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RimeWreath));
class FrostBreath(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.FrostBreath), new AOEShapeCone(27, 60.Degrees())); // TODO: verify angle
class SheetOfIce(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SheetOfIce), 5);
class SheetOfIce2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SheetOfIce2), 5);
class Cauterize(BossModule module) : Components.SelfTargetedLegacyRotationAOEs(module, ActionID.MakeSpell(AID.Cauterize), new AOEShapeRect(48, 10));

class Touchdown(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Touchdown))
{
    private readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: proper timings...
        if (!Module.PrimaryActor.IsTargetable && !Module.FindComponent<Cauterize>()!.ActiveCasters.Any())
            yield return new(_shape, Module.Center);
    }
}

class D103IsgebindStates : StateMachineBuilder
{
    public D103IsgebindStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<FrostBreath>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<SheetOfIce2>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1680)]
public class D103Isgebind(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Square(new(0, -248), 23.7f), new Square(new(-23, -259.4f), 1), new Square(new(3.4f, -271), 1),
    new Square(new(12.8f, -225), 1)];
    private static readonly List<Shape> difference = [new Square(new(-24, -224), 5.75f, 45.Degrees()), new Square(new(24, -224), 5.75f, 45.Degrees()),
    new Square(new(-24, -272), 5.75f, 45.Degrees()), new Square(new(24, -272), 5.75f, 45.Degrees()), new Circle(new(-23.9f, -248), 4.5f),
    new Circle(new(23.9f, -248), 4.5f), new Square(new(-16.2f, -271.9f), 0.5f, 45.Degrees()), new Square(new(16.2f, -271.9f), 0.5f, 45.Degrees()), new Square(new(-16.2f, -224.1f), 0.5f, 45.Degrees()),
    new Square(new(16.2f, -224.1f), 0.5f, 45.Degrees()), new Square(new(-23.9f, -264.2f), 0.45f, 45.Degrees()), new Square(new(23.9f, -264.2f), 0.45f, 45.Degrees()), new Square(new(-23.9f, -231.8f), 0.45f, 45.Degrees()),
    new Square(new(23.9f, -231.8f), 0.45f, 45.Degrees()), new Rectangle(new(0, -224), 3.8f, 0.4f), new Rectangle(new(2.1f, -224.3f), 1.5f, 0.4f), new Rectangle(new(-2.1f, -224.3f), 1.5f, 0.4f),
    new Square(new(-8, -224), 0.45f), new Square(new(7.9f, -224), 0.45f), new Square(new(7.9f, -272), 0.45f), new Square(new(0, -272), 0.45f),
    new Square(new(-7.9f, -272), 0.45f), new Square(new(24, -240), 0.45f), new Square(new(24, -256), 0.45f), new Square(new(-24, -240), 0.45f), new Square(new(-24, -256), 0.45f)];
    private static readonly List<Shape> union2 = [new Circle(new(-19.5f, -228.4f), 3), new Circle(new(19.5f, -228.4f), 3), new Circle(new(-19.5f, -267.6f), 3),
    new Circle(new(19.5f, -267.6f), 3), new Circle(new(-21.3f, -243.6f), 1.5f), new Circle(new(-21.3f, -252.4f), 1.5f), new Square(new(-23, -243.4f), 0.7f),
    new Square(new(-23, -252.6f), 0.7f), new Circle(new(21.3f, -243.6f), 1.5f), new Circle(new(21.3f, -252.4f), 1.5f), new Square(new(23, -243.4f), 0.7f),
    new Square(new(23, -252.6f), 0.7f)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference, union2);
}
