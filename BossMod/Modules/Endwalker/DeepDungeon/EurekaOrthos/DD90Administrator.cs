namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD90Administrator;

public enum OID : uint
{
    Boss = 0x3D23, // R5.950, x1
    EggInterceptor = 0x3D24, // R2.300, x?
    SquareInterceptor = 0x3D25, // R2.300, x?
    OrbInterceptor = 0x3D26, // R2.300, x?
    Helper = 0x233C, // R0.500, x12, 523 type
}

public enum AID : uint
{
    AetherochemicalLaserCone = 31451, // EggInterceptor->self, 6.0s cast, range 50 120-degree cone | Conal from Egg
    AetherochemicalLaserDonut = 31453, // OrbInterceptor->self, 6.0s cast, range ?-40 donut
    AetherochemicalLaserLine = 31452, // SquareInterceptor->self, 6.0s cast, range 40 width 5 rect

    AetherochemicalLaserCone2 = 32832, // EggInterceptor->self, 8.0s cast, range 50 120-degree cone | Cone from Egg, Paired w/ AID 32833
    AetherochemicalLaserLine2 = 32833, // SquareInterceptor->self, 8.0s cast, range 40 width 5 rect | Paired w/ Aid 32832

    AutoAttack = 31457, // Boss->player, no cast, single-target
    CrossLaser = 31448, // Boss->self, 6.0s cast, range 60 width 10 cross | Cross AOE After LC helpers happen (2/2 he can do at the end)
    PeripheralLasers = 31447, // Boss->self, 6.0s cast, range ?-60 donut | Donut on the boss after LC helpers happen (1 of 2 he can do at the end). Video looks like it's ~3.5r? It's smaller than boss's hitbox. // Need testing

    HomingLaser = 31461, // Helper->location, 3.0s cast, range 6 circle | Targeted circles, attacks x5

    Laserstream = 31456, // Boss->self, 4.0s cast, range 60 circle | Roomwide AOE
    ParallelExecution = 31454, // Boss->self, 3.0s cast, range 60 circle | Skill that preps the line of cubes + 5 puddles underneath the feet

    SalvoScript = 31455, // Boss->self, 3.0s cast, range 60 circle | Activating the cubes/Egg's for cast (tiny safe spot in corner) // Could be used for a hint here?
    SupportSystems = 31449, // Boss->self, 3.0s cast, single-target | Cast to summon Intercepters around the arena // Could be used for hints
    HomingLaserBoss = 31460, // Boss->self, no cast, single-target | Instacast animation, effect to simulate helper actually doing puddles 
    InterceptionSequence = 31450, // Boss->self, 3.0s cast, range 60 circle | Used to prep the Egg/Orb/Cube/Square for order of firing 

    Weaponskill_Unknown = 31458, // Boss->self, no cast, single-target
    Weaponskill_Unknown2 = 31459, // Boss->self, no cast, single-target
}

class AetherLaserCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserCone), new AOEShapeCone(50, 60.Degrees()));
class AetherLaserDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserDonut), new AOEShapeDonut(4f, 60));
class AetherLaserLine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserLine), new AOEShapeRect(40, 2.5f))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // Attempting to offput the lines going off here... since not all the time they go off at the same time (phase 2/3 of this boss)
        // There's only 2 of them going off at similar times (one N<->S, one E<->W)
        // Need to go back and test this a bit more to make sure it looks/feels good
        var timeLimit = Casters.FirstOrDefault()?.CastInfo?.NPCFinishAt.AddSeconds(1.5f) ?? new();
        return Casters.TakeWhile(c => c.CastInfo!.NPCFinishAt <= timeLimit).Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }
}

class AetherLaserLine2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserLine2), new AOEShapeRect(40, 2.5f));
class AetherLaserCone2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaserCone2), new AOEShapeCone(50, 60.Degrees()));

class CrossLasers(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrossLaser), new AOEShapeCross(60, 5));
class PeripheralLasers(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PeripheralLasers), new AOEShapeDonut(4.5f, 60));

class HomingLasers(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HomingLaser), 6f, "Get out of the puddle, avoid the lines");

class DD90AdministratorStates : StateMachineBuilder
{
    public DD90AdministratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherLaserCone>()
            .ActivateOnEnter<AetherLaserDonut>()
            .ActivateOnEnter<AetherLaserLine>()
            .ActivateOnEnter<AetherLaserLine2>()
            .ActivateOnEnter<AetherLaserCone2>()
            .ActivateOnEnter<CrossLasers>()
            .ActivateOnEnter<PeripheralLasers>()
            .ActivateOnEnter<HomingLasers>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 903, NameID = 12102)]
public class DD90Administrator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsSquare(20));
