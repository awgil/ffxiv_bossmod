namespace BossMod.Heavensward.Dungeon.D04TheVault.D042SerGrinnaux;

public enum OID : uint
{
    Boss = 0x1054, // R2.200, x1
    SerGrinnauxTheBull = 0x1053, // R0.500, x1
    StellarImplodeArea = 0x1E91C1, // R0.500, x0 (spawn during fight), EventObj type
    AetherialTear = 0x1055, // R2.000, x0 (spawn during fight)
    Helper = 0xD25, // R0.500, x13, mixed types
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Overpower = 2188, // SerGrinnauxTheBull->self, no cast, range 6+R 90-degree cone
    Rive = 1135, // SerGrinnauxTheBull->self, 2.5s cast, range 30+R width 2 rect

    AdventVisual1 = 4979,  // SerGrinnauxTheBull->self, no cast, single-target
    AdventVisual2 = 4122, // SerGrinnauxTheBull->self, no cast, single-target
    AdventVisual3 = 4123, // Boss->self, no cast, single-target
    Advent = 4980, // Helper->self, no cast, range 80 circle, knockback 18, away from source
    Retreat = 4257, // SerGrinnauxTheBull->self, no cast, single-target

    DimensionalCollapseVisual = 4136, // Boss->self, 2.5s cast, single-target
    DimensionalCollapse1 = 4137, // Helper->self, 3.0s cast, range 7+R 180-degree cone inner
    DimensionalCollapse2 = 4138, // Helper->self, 3.0s cast, range 12+R 180-degree cone middle
    DimensionalCollapse3 = 4139, // Helper->self, 3.0s cast, range 17+R 180-degree cone outer

    DimensionalRip = 4140, // Boss->location, 3.0s cast, range 5 circle
    StellarImplosion = 4141, // Helper->location, no cast, range 5 circle
    DimensionalTorsion = 4142, // AetherialTear->player, no cast, single-target

    FaithUnmoving = 4135, // Boss->self, 3.0s cast, range 80+R circle
    HeavySwing = 4133, // Boss->player, no cast, range 8+R 90-degree cone
    HyperdimensionalSlash = 4134, // Boss->self, 3.0s cast, range 45+R width 8 rect

    BossPhase1Vanish = 4124, // Boss->self, no cast, single-target
    BossPhase2Vanish = 4256, // SerGrinnauxTheBull->self, no cast, single-target
}

class HeavySwing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.HeavySwing), new AOEShapeCone(6.5f, 45.Degrees()), (uint)OID.SerGrinnauxTheBull);
class Overpower(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Overpower), new AOEShapeCone(10.2f, 45.Degrees()));
class DimensionalRip(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.DimensionalRip), m => m.Enemies(OID.StellarImplodeArea).Where(e => e.EventState != 7), 1.1f);

class FaithUnmoving(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.FaithUnmoving), 20, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<AetherialTear>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (Module.FindComponent<DimensionalRip>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);
}

class Rive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Rive), new AOEShapeRect(30.5f, 1));
class HyperdimensionalSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HyperdimensionalSlash), new AOEShapeRect(47.2f, 4));
class DimensionalCollapse1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapse1), new AOEShapeDonutSector(2.5f, 7.5f, 90.Degrees()));
class DimensionalCollapse2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapse2), new AOEShapeDonutSector(7.5f, 12.5f, 90.Degrees()));
class DimensionalCollapse3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapse3), new AOEShapeDonutSector(12.5f, 17.5f, 90.Degrees()));

class AetherialTear(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(7);
    private readonly List<Actor> _tears = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in _tears)
            yield return new(circle, s.Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialTear)
            _tears.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialTear)
            _tears.Remove(actor);
    }
}

class D042SerGrinnauxStates : StateMachineBuilder
{
    public D042SerGrinnauxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<HeavySwing>()
            .ActivateOnEnter<DimensionalRip>()
            .ActivateOnEnter<FaithUnmoving>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<AetherialTear>()
            .ActivateOnEnter<DimensionalCollapse1>()
            .ActivateOnEnter<DimensionalCollapse2>()
            .ActivateOnEnter<DimensionalCollapse3>()
            .ActivateOnEnter<HyperdimensionalSlash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3639)]
public class D042SerGrinnaux(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.SerGrinnauxTheBull).Any(e => e.InCombat);

    private static readonly List<Shape> union = [new Circle(new(0, 72), 19.7f)];
    private static readonly List<Shape> difference = [new Rectangle(new(19.5f, 72), 7.75f, 1.75f, 90.Degrees()), new Rectangle(new(0, 51), 7.75f, 2),
    new Rectangle(new(-20.8f, 72), 5, 1.75f, 90.Degrees())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.SerGrinnauxTheBull))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
