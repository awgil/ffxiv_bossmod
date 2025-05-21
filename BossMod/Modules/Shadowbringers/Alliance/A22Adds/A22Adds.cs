namespace BossMod.Shadowbringers.Alliance.A22Adds;

public enum OID : uint
{
    Boss = 0x2EE4,
    Helper = 0x233C,
    _Gen_YorhaCloseCombatUnitSpear = 0x2EE5, // R0.500, x3
    _Gen_YorhaCloseCombatUnitBlade = 0x2EE7, // R0.500, x3
    _Gen_YorhaCloseCombatUnitMartial = 0x2EE6, // R0.500, x3
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_SpearJab = 21405, // _Gen_YorhaCloseCombatUnitSpear->player, no cast, single-target
    _Weaponskill_StrikingBlow = 21409, // _Gen_YorhaCloseCombatUnitMartial->player, no cast, single-target
    _Weaponskill_BladeFlurry = 21413, // _Gen_YorhaCloseCombatUnitBlade->player, no cast, single-target
    _Weaponskill_SpearSequence = 21406, // _Gen_YorhaCloseCombatUnitSpear->player, no cast, single-target
    _Weaponskill_DancingBlade = 21414, // _Gen_YorhaCloseCombatUnitBlade->player, no cast, single-target
    _Weaponskill_SequentialBlows = 21410, // _Gen_YorhaCloseCombatUnitMartial->player, no cast, single-target
    _Weaponskill_PalmQuake = 21411, // _Gen_YorhaCloseCombatUnitMartial->player, no cast, single-target
    _Weaponskill_SeamlessForm = 21407, // _Gen_YorhaCloseCombatUnitSpear->player, no cast, single-target
    _Weaponskill_BalancedEdge = 21415, // _Gen_YorhaCloseCombatUnitBlade->player, no cast, single-target
    _Weaponskill_TopplingStrike = 21412, // _Gen_YorhaCloseCombatUnitMartial->player, no cast, single-target
    _Weaponskill_PointedSpearing = 21408, // _Gen_YorhaCloseCombatUnitSpear->player, no cast, single-target
    _Weaponskill_WhirlingAssault = 21416, // _Gen_YorhaCloseCombatUnitBlade->player, no cast, single-target
    _Weaponskill_WeightedBlade = 21417, // _Gen_YorhaCloseCombatUnitBlade->player, no cast, single-target
    _Weaponskill_ManeuverLongBarreledLaser = 21010, // Boss->self, 4.0s cast, range 70 width 8 rect
    _Weaponskill_AuthorizationNoRestrictions = 21006, // Boss->self, 2.0s cast, single-target
    _Weaponskill_SurfaceMissileImpact = 21007, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_HomingMissileImpact = 21008, // Helper->players, 6.0s cast, range 6 circle
    _Ability_ = 21012, // Boss->self, no cast, single-target
    _Weaponskill_ManeuverVoltArray = 21009, // Boss->self, 5.0s cast, range 100 circle
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.Boss, OID._Gen_YorhaCloseCombatUnitSpear, OID._Gen_YorhaCloseCombatUnitBlade, OID._Gen_YorhaCloseCombatUnitMartial]);

class LongBarreledLaser(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ManeuverLongBarreledLaser, new AOEShapeRect(70, 4));
class VoltArray(BossModule module) : Components.CastInterruptHint(module, AID._Weaponskill_ManeuverVoltArray, showNameInHint: true);
class SurfaceMissileImpact(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID._Weaponskill_SurfaceMissileImpact, m => m.Enemies(0x1E8D9B).Where(e => e.EventState != 7), 0);
class HomingMissileImpact(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_HomingMissileImpact, 6);

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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9652)]
public class A22Adds(WorldState ws, Actor primary) : BossModule(ws, primary, new(-230, -15), new ArenaBoundsSquare(25));

