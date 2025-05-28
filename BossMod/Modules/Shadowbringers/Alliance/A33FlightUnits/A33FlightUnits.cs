namespace BossMod.Shadowbringers.Alliance.A33FlightUnits;

public enum OID : uint
{
    Boss = 0x3193, // R0.512-2.250, x3 (spawn during fight)
    Helper = 0x233C, // R0.500, x1, Helper type
    FlightUnit = 0x3194, // R2.800, x0 (spawn during fight)
}

public enum AID : uint
{
    BladeFlurry1 = 23543, // Boss->player, no cast, single-target
    BladeFlurry2 = 23544, // Boss->player, no cast, single-target
    WhirlingAssault = 23547, // Boss->self, 4.0s cast, range 40 width 4 rect
    LightfastBlade = 23550, // FlightUnit->self, 12.0s cast, range 48 180-degree cone
    ManeuverStandardLaser = 23551, // FlightUnit->self, 2.0s cast, range 52 width 5 rect
    BalancedEdge = 23546, // Boss->self, 4.0s cast, range 5 circle
}

class WhirlingAssault(BossModule module) : Components.StandardAOEs(module, AID.WhirlingAssault, new AOEShapeRect(40, 2));
class LightfastBlade(BossModule module) : Components.StandardAOEs(module, AID.LightfastBlade, new AOEShapeCone(48, 90.Degrees()), maxCasts: 2);
class ManeuverStandardLaser(BossModule module) : Components.StandardAOEs(module, AID.ManeuverStandardLaser, new AOEShapeRect(52, 2.5f));
class BalancedEdge(BossModule module) : Components.StandardAOEs(module, AID.BalancedEdge, 5);

public enum IconID : uint
{
    Lockon = 164, // player->self
}

class A33FlightUnitsStates : StateMachineBuilder
{
    public A33FlightUnitsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhirlingAssault>()
            .ActivateOnEnter<LightfastBlade>()
            .ActivateOnEnter<ManeuverStandardLaser>()
            .ActivateOnEnter<BalancedEdge>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(f => f.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9918)]
public class A33FlightUnits(WorldState ws, Actor primary) : BossModule(ws, primary, new(755, -749.4f), new ArenaBoundsCircle(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FlightUnit), ArenaColor.Enemy);
    }
}
