namespace BossMod.RealmReborn.Dungeon.D03Copperbell.D032IchorousIre;

public enum OID : uint
{
    Boss = 0x3870,
    IchorousDrip = 0x3871, // x6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast
    Syrup = 28462, // Boss->location, 4.0s cast, range 4 aoe
    FluidSpread = 28461, // Boss->player, 5.0s cast, tankbuster
    Divide = 28463, // Boss->self, 3.0s cast, visual
    DivideAppear = 28464, // IchorousDrip->location, no cast, teleport/appear
    Burst = 28465, // IchorousDrip->self, 6.0s cast, range 8 aoe
}

class Syrup(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Syrup), 4);
class FluidSpread(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.FluidSpread));
class Divide(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst), new AOEShapeCircle(8));

class D032IchorousIreStates : StateMachineBuilder
{
    public D032IchorousIreStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Syrup>()
            .ActivateOnEnter<FluidSpread>()
            .ActivateOnEnter<Divide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 3, NameID = 554)]
public class D032IchorousIre(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(27, 114), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(37.5f, 95), 20, 2.4f, 25.Degrees()), new Rectangle(new(6, 112), 20, 1.75f, 270.Degrees())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
}