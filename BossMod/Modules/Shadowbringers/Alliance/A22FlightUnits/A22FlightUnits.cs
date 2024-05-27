namespace BossMod.Shadowbringers.Alliance.A22FlightUnits;

class IncendiaryBarrage(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IncendiaryBarrage), 27);
class StandardSurfaceMissile1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.StandardSurfaceMissile1), 10);
class StandardSurfaceMissile2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.StandardSurfaceMissile2), 10);
class LethalRevolution(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LethalRevolution), new AOEShapeCircle(15));

class GuidedMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GuidedMissile), 4);
class IncendiaryBombing(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IncendiaryBombing), 8);
class SurfaceMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SurfaceMissile), 6);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AntiPersonnelMissile), 6);

class PrecisionGuidedMissile(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PrecisionGuidedMissile), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.FlightUnitALpha, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9140)]
public class A22FlightUnits(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly List<Shape> union = [new Circle(new(-250, -160.4f), 19), new Circle(new(-230, -195), 19), new Circle(new(-210, -160.4f), 19), new Rectangle(new(-230, -172), 18, 30.5f), new Rectangle(new(-230, -172), 18, 30.5f, 60.Degrees()), new Rectangle(new(-230, -172), 18, 30.5f, -60.Degrees())];

    public static readonly ArenaBoundsComplex DefaultBounds = new(union);

    private Actor? _beta;
    private Actor? _chi;

    public Actor? FlightUnitALpha() => PrimaryActor;
    public Actor? FlightUnitBEta() => _beta;
    public Actor? FlightUnitCHi() => _chi;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _beta ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.FlightUnitBEta).FirstOrDefault() : null;
        _chi ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.FlightUnitCHi).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_beta, ArenaColor.Enemy);
        Arena.Actor(_chi, ArenaColor.Enemy);
    }
}
