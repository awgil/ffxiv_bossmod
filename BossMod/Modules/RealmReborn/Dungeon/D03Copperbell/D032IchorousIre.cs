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

class Syrup(BossModule module) : Components.StandardAOEs(module, AID.Syrup, 4);
class FluidSpread(BossModule module) : Components.SingleTargetCast(module, AID.FluidSpread);
class Divide(BossModule module) : Components.StandardAOEs(module, AID.Burst, new AOEShapeCircle(8));

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
public class D032IchorousIre(WorldState ws, Actor primary) : BossModule(ws, primary, new(26.97f, 113.97f), new ArenaBoundsCircle(20));
