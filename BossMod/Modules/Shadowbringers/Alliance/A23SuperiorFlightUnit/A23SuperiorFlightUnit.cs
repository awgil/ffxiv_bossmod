using Lumina.Extensions;

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
    _AutoAttack_ = 21423, // Alpha/Beta/Chi->player, no cast, single-target
    _Weaponskill_ApplyShieldProtocol = 20390, // Alpha->self, 5.0s cast, single-target
    _Weaponskill_ApplyShieldProtocol1 = 20392, // Chi->self, 5.0s cast, single-target
    _Weaponskill_ApplyShieldProtocol2 = 20391, // Beta->self, 5.0s cast, single-target
    _Weaponskill_ManeuverMissileCommand = 20413, // Alpha/Beta/Chi->self, 4.0s cast, single-target
    _Weaponskill_BarrageImpact = 20414, // Helper->self, no cast, range 50 circle
    _Weaponskill_ManeuverIncendiaryBombing = 20419, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    _Ability_ = 20406, // Helper->player, no cast, single-target
    _Weaponskill_IncendiaryBombing = 20409, // Helper->location, 5.0s cast, range 8 circle
    _Weaponskill_ManeuverHighPoweredLaser = 20404, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    _Weaponskill_ManeuverHighPoweredLaser1 = 20405, // Alpha/Beta/Chi->players, no cast, range 80 width 14 rect
    _Weaponskill_FormationSharpTurn = 20395, // Alpha/Beta/Chi->self, 3.0s cast, single-target
    _Weaponskill_ = 26807, // Alpha/Beta/Chi->location, no cast, single-target
    SharpTurnAlphaRight = 20393,  // Alpha->self, 9.0s cast, single-target
    SharpTurnAlphaLeft = 20394, // Alpha->self, 9.0s cast, single-target
    SharpTurnBetaRight = 21777,  // Beta->self, 9.0s cast, single-target
    SharpTurnBetaLeft = 21778, // Beta->self, 9.0s cast, single-target
    SharpTurnChiRight = 21779, // Chi->self, 9.0s cast, single target
    SharpTurnChiLeft = 21780, // Chi->self, 9.0s cast, single-target
    SharpTurnRight = 20589, // Helper->self, no cast, range 110 width 30 rect
    SharpTurnLeft = 20590, // Helper->self, no cast, range 110 width 30 rect
    _Weaponskill_ManeuverPrecisionGuidedMissile = 20420, // Alpha/Beta/Chi->self, 4.0s cast, single-target
    _Weaponskill_PrecisionGuidedMissile = 20421, // Helper->players, 4.0s cast, range 6 circle
    _Weaponskill_FormationAirRaid = 20400, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    _Weaponskill_StandardSurfaceMissile = 20401, // Helper->location, 5.0s cast, range 10 circle
    _Weaponskill_StandardSurfaceMissile1 = 20402, // Helper->location, 5.0s cast, range 10 circle
    _Weaponskill_LethalRevolution = 20403, // Alpha/Beta/Chi->self, 5.0s cast, range 15 circle
    _Weaponskill_FormationSlidingSwipe = 20398, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    _Weaponskill_SuperiorMobility = 20412, // Alpha/Beta/Chi->location, no cast, single-target
    _Weaponskill_IncendiaryBarrage = 20399, // Helper->location, 9.0s cast, range 27 circle
    AlphaSlidingSwipeRight = 20396, // Alpha->self, 6.0s cast, single-target
    AlphaSlidingSwipeLeft = 20397, // Alpha->self, 6.0s cast, single-target
    BetaSlidingSwipeRight = 21773, // Beta->self, 6.0s cast, single-target
    BetaSlidingSwipeLeft = 21774, // Beta->self, 6.0s cast, single-target
    ChiSlidingSwipeRight = 21775, // Chi->self, 6.0s cast, single-target
    ChiSlidingSwipeLeft = 21776, // Chi->self, 6.0s cast, single-target
    SlidingSwipeLeft = 20592, // Helper->self, no cast, range 130 width 30 rect, left hand (all)
    SlidingSwipeRight = 20591, // Helper->self, no cast, range 130 width 30 rect, right hand (all)
    _Weaponskill_ManeuverAreaBombardment = 20407, // Alpha/Beta/Chi->self, 5.0s cast, single-target
    _Weaponskill_GuidedMissile = 20408, // Helper->location, 3.0s cast, range 4 circle
    _Weaponskill_SurfaceMissile = 20410, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_AntiPersonnelMissile = 20411, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_ManeuverHighOrderExplosiveBlast = 20415, // Alpha/Chi->self, 4.0s cast, single-target
    _Weaponskill_HighOrderExplosiveBlast = 20416, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_HighOrderExplosiveBlast1 = 20417, // Helper->self, 1.5s cast, range 20 width 5 cross
}

public enum SID : uint
{
    _Gen_ShieldProtocolA = 2288, // none->player, extra=0x0
    _Gen_ShieldProtocolB = 2289, // none->player, extra=0x0
    _Gen_ShieldProtocolC = 2290, // none->player, extra=0x0
    _Gen_ProcessOfEliminationA = 2409, // none->Alpha, extra=0x0
    _Gen_ProcessOfEliminationB = 2410, // none->Beta, extra=0x0
    _Gen_ProcessOfEliminationC = 2411, // none->Chi, extra=0x0
    _Gen_Burns = 2194, // none->player, extra=0x0
    _Gen_MagicVulnerabilityUp = 2091, // Beta/Alpha/Chi->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_VulnerabilityUp = 1789, // Helper/Beta->player, extra=0x1/0x3
    _Gen_Burns1 = 2401, // Helper->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
}

public enum TetherID : uint
{
    _Gen_Tether_chn_earth001f = 7, // player->Alpha/Beta/Chi
    _Gen_Tether_chn_m0354_0c = 54, // Alpha/Beta/Chi->Beta/Alpha/Chi
}

public enum IconID : uint
{
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_tank_lockon01i = 198, // player->self
    _Gen_Icon_target_ae_s5f = 139, // player->self
}

// raidwide actually has 3 hits but i cba
class ManeuverMissileCommand(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_ManeuverMissileCommand, AID._Weaponskill_BarrageImpact, 2.1f);

class IncendiaryBombingSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8), (uint)IconID._Gen_Icon_lockon5_t0h, AID._Weaponskill_IncendiaryBombing, 6, centerAtTarget: true, damageType: AIHints.PredictedDamageType.None)
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
class IncendiaryBombing(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID._Weaponskill_IncendiaryBombing, m => m.Enemies(OID.IncendiaryBomb).Where(b => b.EventState != 7), 0.8f);

class PrecisionGuidedMissile(BossModule module) : Components.BaitAwayCast(module, AID._Weaponskill_PrecisionGuidedMissile, new AOEShapeCircle(6), true, true);
class LethalRevolution(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LethalRevolution, new AOEShapeCircle(15));
class StandardSurfaceMissile(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_StandardSurfaceMissile, AID._Weaponskill_StandardSurfaceMissile1], new AOEShapeCircle(10), maxCasts: 9);
class IncendiaryBarrage(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 27, AID._Weaponskill_IncendiaryBarrage, m => m.Enemies(0x1EB07B).Where(e => e.EventState != 7), 0.8f);
class GuidedMissile(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GuidedMissile, 4);
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SurfaceMissile, 6);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_AntiPersonnelMissile, 6);
class HighOrderExplosiveBlast(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HighOrderExplosiveBlast, 6);
class HighOrderExplosiveBlast2(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HighOrderExplosiveBlast1, new AOEShapeCross(20, 2.5f));

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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9364, PrimaryActorOID = (uint)OID.Alpha)]
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

