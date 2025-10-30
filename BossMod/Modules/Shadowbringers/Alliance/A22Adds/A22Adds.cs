namespace BossMod.Shadowbringers.Alliance.A22Adds;

public enum OID : uint
{
    Boss = 0x2EE4,
    Helper = 0x233C,
    YorhaCloseCombatUnitSpear = 0x2EE5, // R0.500, x3
    YorhaCloseCombatUnitBlade = 0x2EE7, // R0.500, x3
    YorhaCloseCombatUnitMartial = 0x2EE6, // R0.500, x3
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    SpearJab = 21405, // YorhaCloseCombatUnitSpear->player, no cast, single-target
    StrikingBlow = 21409, // YorhaCloseCombatUnitMartial->player, no cast, single-target
    BladeFlurry = 21413, // YorhaCloseCombatUnitBlade->player, no cast, single-target
    SpearSequence = 21406, // YorhaCloseCombatUnitSpear->player, no cast, single-target
    DancingBlade = 21414, // YorhaCloseCombatUnitBlade->player, no cast, single-target
    SequentialBlows = 21410, // YorhaCloseCombatUnitMartial->player, no cast, single-target
    PalmQuake = 21411, // YorhaCloseCombatUnitMartial->player, no cast, single-target
    SeamlessForm = 21407, // YorhaCloseCombatUnitSpear->player, no cast, single-target
    BalancedEdge = 21415, // YorhaCloseCombatUnitBlade->player, no cast, single-target
    TopplingStrike = 21412, // YorhaCloseCombatUnitMartial->player, no cast, single-target
    PointedSpearing = 21408, // YorhaCloseCombatUnitSpear->player, no cast, single-target
    WhirlingAssault = 21416, // YorhaCloseCombatUnitBlade->player, no cast, single-target
    WeightedBlade = 21417, // YorhaCloseCombatUnitBlade->player, no cast, single-target
    ManeuverLongBarreledLaser = 21010, // Boss->self, 4.0s cast, range 70 width 8 rect
    AuthorizationNoRestrictions = 21006, // Boss->self, 2.0s cast, single-target
    SurfaceMissileImpact = 21007, // Helper->location, 4.0s cast, range 6 circle
    HomingMissileImpact = 21008, // Helper->players, 6.0s cast, range 6 circle
    ManeuverVoltArray = 21009, // Boss->self, 5.0s cast, range 100 circle

    // _Ability_ = 21012, // Boss->self, no cast, single-target
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.Boss, OID.YorhaCloseCombatUnitSpear, OID.YorhaCloseCombatUnitBlade, OID.YorhaCloseCombatUnitMartial]);

class LongBarreledLaser(BossModule module) : Components.StandardAOEs(module, AID.ManeuverLongBarreledLaser, new AOEShapeRect(70, 4));
class VoltArray(BossModule module) : Components.CastInterruptHint(module, AID.ManeuverVoltArray, showNameInHint: true);
class SurfaceMissileImpact(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.SurfaceMissileImpact, m => m.Enemies(0x1E8D9B).Where(e => e.EventState != 7), 0);
class HomingMissileImpact(BossModule module) : Components.SpreadFromCastTargets(module, AID.HomingMissileImpact, 6);

class A22AddsStates : StateMachineBuilder
{
    public A22AddsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<LongBarreledLaser>()
            .ActivateOnEnter<VoltArray>()
            .ActivateOnEnter<SurfaceMissileImpact>()
            .ActivateOnEnter<HomingMissileImpact>()
            .Raw.Update = () => Module.Enemies(OID.Boss).All(b => b.IsDeadOrDestroyed || b.HPMP.CurHP == 1 && !b.IsTargetable);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9652)]
public class A22Adds(WorldState ws, Actor primary) : BossModule(ws, primary, new(-230, -15), new ArenaBoundsSquare(25));

