namespace BossMod.Shadowbringers.Alliance.A23SuperiorFlightUnit;

public enum OID : uint
{
    Alpha = 0x2E10,
    Beta = 0x2E11,
    Chi = 0x2E12,
    Helper = 0x233C,
    IncendiaryBomb = 0x1EAEC8
}

public enum AID : uint
{
    AutoAttack = 21423, // Alpha/Beta/Chi->player, no cast, single-target
    ApplyShieldProtocolA = 20390, // Alpha->self, 5.0s cast, single-target
    ApplyShieldProtocolB = 20391, // Beta->self, 5.0s cast, single-target
    ApplyShieldProtocolC = 20392, // Chi->self, 5.0s cast, single-target
    ManeuverMissileCommand = 20413, // Alpha/Beta/Chi->self, 4.0s cast, single-target
    BarrageImpact = 20414, // Helper->self, no cast, range 50 circle
    ManeuverIncendiaryBombing = 20419, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    IncendiaryBombing = 20409, // Helper->location, 5.0s cast, range 8 circle
    ManeuverHighPoweredLaser = 20404, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    ManeuverHighPoweredLaser1 = 20405, // Alpha/Beta/Chi->players, no cast, range 80 width 14 rect
    FormationSharpTurn = 20395, // Alpha/Beta/Chi->self, 3.0s cast, single-target
    SharpTurnAlphaRight = 20393,  // Alpha->self, 9.0s cast, single-target
    SharpTurnAlphaLeft = 20394, // Alpha->self, 9.0s cast, single-target
    SharpTurnBetaRight = 21777,  // Beta->self, 9.0s cast, single-target
    SharpTurnBetaLeft = 21778, // Beta->self, 9.0s cast, single-target
    SharpTurnChiRight = 21779, // Chi->self, 9.0s cast, single target
    SharpTurnChiLeft = 21780, // Chi->self, 9.0s cast, single-target
    SharpTurnRight = 20589, // Helper->self, no cast, range 110 width 30 rect, right hand
    SharpTurnLeft = 20590, // Helper->self, no cast, range 110 width 30 rect, left hand
    ManeuverPrecisionGuidedMissile = 20420, // Alpha/Beta/Chi->self, 4.0s cast, single-target
    PrecisionGuidedMissile = 20421, // Helper->players, 4.0s cast, range 6 circle
    FormationAirRaid = 20400, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    StandardSurfaceMissile = 20401, // Helper->location, 5.0s cast, range 10 circle
    StandardSurfaceMissile1 = 20402, // Helper->location, 5.0s cast, range 10 circle
    LethalRevolution = 20403, // Alpha/Beta/Chi->self, 5.0s cast, range 15 circle
    FormationSlidingSwipe = 20398, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    SuperiorMobility = 20412, // Alpha/Beta/Chi->location, no cast, single-target
    IncendiaryBarrage = 20399, // Helper->location, 9.0s cast, range 27 circle
    AlphaSlidingSwipeRight = 20396, // Alpha->self, 6.0s cast, single-target
    AlphaSlidingSwipeLeft = 20397, // Alpha->self, 6.0s cast, single-target
    BetaSlidingSwipeRight = 21773, // Beta->self, 6.0s cast, single-target
    BetaSlidingSwipeLeft = 21774, // Beta->self, 6.0s cast, single-target
    ChiSlidingSwipeRight = 21775, // Chi->self, 6.0s cast, single-target
    ChiSlidingSwipeLeft = 21776, // Chi->self, 6.0s cast, single-target
    SlidingSwipeRight = 20591, // Helper->self, no cast, range 130 width 30 rect, right hand
    SlidingSwipeLeft = 20592, // Helper->self, no cast, range 130 width 30 rect, left hand
    ManeuverAreaBombardment = 20407, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    GuidedMissile = 20408, // Helper->location, 3.0s cast, range 4 circle
    SurfaceMissile = 20410, // Helper->location, 3.0s cast, range 6 circle
    AntiPersonnelMissile = 20411, // Helper->players, 5.0s cast, range 6 circle
    ManeuverHighOrderExplosiveBlast = 20415, // Alpha/Chi->self, 4.0s cast, single-target
    HighOrderExplosiveBlast = 20416, // Helper->location, 4.0s cast, range 6 circle
    HighOrderExplosiveBlast1 = 20417, // Helper->self, 1.5s cast, range 20 width 5 cross
    TargetSelect = 20406, // Helper->player, no cast, single-target

    // = 26807, // Alpha/Beta/Chi->location, no cast, single-target
}

public enum SID : uint
{
    ShieldProtocolA = 2288, // none->player, extra=0x0
    ShieldProtocolB = 2289, // none->player, extra=0x0
    ShieldProtocolC = 2290, // none->player, extra=0x0
    ProcessOfEliminationA = 2409, // none->Alpha, extra=0x0
    ProcessOfEliminationB = 2410, // none->Beta, extra=0x0
    ProcessOfEliminationC = 2411, // none->Chi, extra=0x0
    Burns1 = 2194, // none->player, extra=0x0
    Burns2 = 2401, // Helper->player, extra=0x0
    MagicVulnerabilityUp = 2091, // Beta/Alpha/Chi->player, extra=0x0
}

public enum TetherID : uint
{
    ShieldProtocol = 7, // player->Alpha/Beta/Chi
    SharpTurn = 54, // Alpha/Beta/Chi->Beta/Alpha/Chi
}

public enum IconID : uint
{
    Lockon = 23, // player->self
    Tankbuster = 198, // player->self
}

// raidwide actually has 3 hits but i cba
class ManeuverMissileCommand(BossModule module) : Components.RaidwideCastDelay(module, AID.ManeuverMissileCommand, AID.BarrageImpact, 2.1f);

class HighPoweredLaser(BossModule module) : Components.MultiLineStack(module, 7, 80, AID.TargetSelect, AID.ManeuverHighPoweredLaser1, 5.4f);

class IncendiaryBombingSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8), (uint)IconID.Lockon, AID.IncendiaryBombing, 6, centerAtTarget: true, damageType: AIHints.PredictedDamageType.None)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // encourage AI not to bait explosion near party
        if (ActiveBaitsOn(actor).FirstOrNull() is { } bait)
            hints.AddForbiddenZone(p => A23SuperiorFlightUnit.PlatformCenters.Any(c => p.InCircle(c, 17)), bait.Activation);
    }
}
class IncendiaryBombing(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.IncendiaryBombing, m => m.Enemies(OID.IncendiaryBomb).Where(b => b.EventState != 7), 0.8f);

class PrecisionGuidedMissile(BossModule module) : Components.BaitAwayCast(module, AID.PrecisionGuidedMissile, new AOEShapeCircle(6), true, true);
class LethalRevolution(BossModule module) : Components.StandardAOEs(module, AID.LethalRevolution, new AOEShapeCircle(15));
class StandardSurfaceMissile(BossModule module) : Components.GroupedAOEs(module, [AID.StandardSurfaceMissile, AID.StandardSurfaceMissile1], new AOEShapeCircle(10), maxCasts: 9);
class IncendiaryBarrage(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 27, AID.IncendiaryBarrage, m => m.Enemies(0x1EB07B).Where(e => e.EventState != 7), 0.8f);
class GuidedMissile(BossModule module) : Components.StandardAOEs(module, AID.GuidedMissile, 4);
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissile, 6);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.AntiPersonnelMissile, 6);
class HighOrderExplosiveBlast(BossModule module) : Components.StandardAOEs(module, AID.HighOrderExplosiveBlast, 6);
class HighOrderExplosiveBlast2(BossModule module) : Components.StandardAOEs(module, AID.HighOrderExplosiveBlast1, new AOEShapeCross(20, 2.5f));

class SharpTurn(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos caster, Angle rotation, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(30, 100), c.caster, c.rotation, c.activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var angle = (AID)spell.Action.ID switch
        {
            AID.SharpTurnAlphaRight or AID.SharpTurnBetaRight or AID.SharpTurnChiRight => -90.Degrees(),
            AID.SharpTurnAlphaLeft or AID.SharpTurnBetaLeft or AID.SharpTurnChiLeft => 90.Degrees(),
            _ => default
        };
        if (angle != default)
            _casters.Add((caster.Position, spell.Rotation + angle, Module.CastFinishAt(spell, 1.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SharpTurnRight or AID.SharpTurnLeft)
        {
            NumCasts++;
            _casters.Clear();
        }
    }
}

class A23SuperiorFlightUnitStates : StateMachineBuilder
{
    public A23SuperiorFlightUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShieldProtocol>()
            .ActivateOnEnter<SharpTurn>()
            .ActivateOnEnter<ManeuverMissileCommand>()
            .ActivateOnEnter<IncendiaryBombingSpread>()
            .ActivateOnEnter<IncendiaryBombing>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<PrecisionGuidedMissile>()
            .ActivateOnEnter<LethalRevolution>()
            .ActivateOnEnter<StandardSurfaceMissile>()
            .ActivateOnEnter<IncendiaryBarrage>()
            .ActivateOnEnter<SlidingSwipe>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<HighOrderExplosiveBlast>()
            .ActivateOnEnter<HighOrderExplosiveBlast2>()
            .Raw.Update = () =>
                module.Enemies(OID.Alpha).All(a => a.IsDeadOrDestroyed) &&
                module.Enemies(OID.Beta).All(b => b.IsDeadOrDestroyed) &&
                module.Enemies(OID.Chi).All(c => c.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9364, PrimaryActorOID = (uint)OID.Alpha)]
public class A23SuperiorFlightUnit(WorldState ws, Actor primary) : BossModule(ws, primary, new(-230, -172), MakeBounds())
{
    public Actor? Alpha => PrimaryActor;
    public Actor? Beta => Enemies(OID.Beta).FirstOrDefault();
    public Actor? Chi => Enemies(OID.Chi).FirstOrDefault();

    public static readonly WPos[] PlatformCenters = [.. new float[] { 60, -60, 120 }.Select(t => new WDir(0, 23).Rotate(t.Degrees())).Select(t => new WPos(-230, -172) + t)];

    private static ArenaBoundsCustom MakeBounds()
    {
        var capprox = CurveApprox.CircleArc(19.5f, 60.Degrees(), -60.Degrees(), 0.02f).Select(c => c + new WDir(0, 23));
        IEnumerable<WDir> rotated(Angle deg) => capprox.Select(c => c.Rotate(deg));

        return new(43, new([.. rotated(180.Degrees()), .. rotated(60.Degrees()), .. rotated(-60.Degrees())]));
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Alpha, ArenaColor.Enemy);
        Arena.Actor(Beta, ArenaColor.Enemy);
        Arena.Actor(Chi, ArenaColor.Enemy);
    }
}

