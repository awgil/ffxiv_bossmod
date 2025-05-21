namespace BossMod.Shadowbringers.Alliance.A24HeavyArtilleryUnit;

public enum OID : uint
{
    Boss = 0x2E6A,
    Helper = 0x233C,
    _Gen_905POperatedHeavyArtilleryUnit = 0x18D6, // R0.500, x9 (spawn during fight)
    _Gen_Energy = 0x2E6C, // R1.000, x0 (spawn during fight)
    _Gen_Pod = 0x2E6D, // R0.800, x0 (spawn during fight)
    ChemicalBurn = 0x1EB055
}

public enum AID : uint
{
    _AutoAttack_Attack = 21561, // Boss->player, no cast, single-target
    _Weaponskill_ManeuverVoltArray = 20486, // Boss->self, 4.0s cast, range 60 circle
    _Ability_OperationActivateLaserTurret = 20461, // Boss->self, 6.0s cast, single-target
    _Weaponskill_UpperLaser = 20615, // Helper->self, 1.8s cast, range 16 ?-degree cone
    _Weaponskill_LowerLaser = 20614, // Helper->self, 1.8s cast, range 30 ?-degree cone
    _Weaponskill_UpperLaser1 = 20471, // Helper->self, no cast, range 16 ?-degree cone
    _Weaponskill_LowerLaser1 = 20470, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_UpperLaser2 = 20616, // Helper->self, 4.8s cast, range ?-23 donut
    _Weaponskill_UpperLaser3 = 20472, // Helper->self, no cast, range ?-23 donut
    _Weaponskill_UpperLaser4 = 20617, // Helper->self, 7.8s cast, range ?-30 donut
    _Weaponskill_UpperLaser5 = 20473, // Helper->self, no cast, range ?-30 donut
    _Weaponskill_ManeuverHighPoweredLaser = 20481, // Boss->self, 5.0s cast, single-target
    _Weaponskill_ManeuverHighPoweredLaser1 = 20482, // Boss->self, no cast, range 60 width 8 rect
    _Ability_ = 20485, // Helper->player, no cast, single-target
    _Weaponskill_ManeuverUnconventionalVoltage = 20483, // Boss->self, 6.0s cast, single-target
    _Weaponskill_UnconventionalVoltage = 20484, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_EnergyBombardment = 20475, // Boss->self, 2.0s cast, single-target
    _Spell_EnergyBombardment = 20476, // Helper->location, 3.0s cast, range 4 circle
    _Weaponskill_ManeuverImpactCrusher = 20477, // Boss->self, 4.0s cast, single-target
    _Weaponskill_ = 20479, // Helper->location, 4.0s cast, range 8 circle
    _Weaponskill_ManeuverImpactCrusher1 = 20478, // Boss->location, no cast, range 8 circle
    _Weaponskill_ManeuverRevolvingLaser = 20480, // Boss->self, 3.0s cast, range ?-60 donut
    _Ability_OperationAccessSelfConsciousnessData = 20463, // Boss->self, 8.0s cast, single-target
    _Ability_OperationActivateSuppressiveUnit = 20462, // Boss->self, 6.0s cast, single-target
    _Spell_EnergyBomb = 20474, // 2E6C->player, no cast, single-target
    _Ability_SupportPod = 20457, // Boss->self, 2.0s cast, single-target
    _Ability_OperationPodProgram = 20458, // Boss->self, 6.0s cast, single-target
    _Weaponskill_R010Laser = 20464, // 2E6D->self, 10.0s cast, range 60 width 12 rect
    _Weaponskill_R030Hammer = 20465, // 2E6D->self, 10.0s cast, range 18 circle
    _Ability_OperationSynthesizeCompound = 20460, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ChemicalBurn = 20468, // Helper->self, no cast, range 3 circle
    _Weaponskill_ChemicalConflagration = 20469, // Helper->self, no cast, range 60 circle
}

public enum IconID : uint
{
    _Gen_Icon_tank_laser_lockon01p = 230, // player->self
    _Gen_Icon_com_trg06_0v = 172, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0410_0p = 122, // Boss->18D6
    _Gen_Tether_chn_data_tensou_0p = 123, // 18D6->2E6D/Helper
}

class ManeuverVoltArray(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ManeuverVoltArray);
class HighPoweredLaser(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 4), (uint)IconID._Gen_Icon_tank_laser_lockon01p, AID._Weaponskill_ManeuverHighPoweredLaser1, 5.7f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && CurrentBaits.Count > 0)
            CurrentBaits.RemoveAt(0);
    }
}
class UnconventionalVoltage(BossModule module) : Components.GenericBaitAway(module, AID._Weaponskill_UnconventionalVoltage)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Ability_)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.MainTargetID)!, new AOEShapeCone(60, 15.Degrees()), WorldState.FutureTime(6.8f)));

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}
class EnergyBombardment(BossModule module) : Components.StandardAOEs(module, AID._Spell_EnergyBombardment, 4);
class ImpactCrusher(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_, 8)
{
    private WPos? _hintLocation;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_hintLocation is { } p)
            hints.GoalZones.Add(hints.GoalSingleTarget(p, 12, 0.5f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && Casters.Count == 3)
            _hintLocation = spell.LocXZ;

        if ((AID)spell.Action.ID == AID._Weaponskill_ManeuverRevolvingLaser)
            _hintLocation = null;
    }
}
class RevolvingLaser(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ManeuverRevolvingLaser, new AOEShapeDonut(12, 60));
class R010Laser(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_R010Laser, new AOEShapeRect(60, 6));
class R030Hammer(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_R030Hammer, new AOEShapeCircle(18));
class Energy : Components.PersistentVoidzone
{
    private readonly List<Actor> _balls = [];

    public Energy(BossModule module) : base(module, 2, _ => [], 8)
    {
        Sources = _ => _balls;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID._Gen_Energy)
        {
            if (id == 0x11D2)
                _balls.Add(actor);
            if (id == 0x11E7)
                _balls.Remove(actor);
        }
    }
}
class ChemicalBurn(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ChemicalBurn && state == 0x00010002)
            Towers.Add(new(actor.Position, 3, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(20)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_ChemicalBurn or AID._Weaponskill_ChemicalConflagration)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
    }
}

class A24HeavyArtilleryUnitStates : StateMachineBuilder
{
    public A24HeavyArtilleryUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManeuverVoltArray>()
            .ActivateOnEnter<LowerLaser>()
            .ActivateOnEnter<UpperLaser>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<UnconventionalVoltage>()
            .ActivateOnEnter<EnergyBombardment>()
            .ActivateOnEnter<ImpactCrusher>()
            .ActivateOnEnter<RevolvingLaser>()
            .ActivateOnEnter<R010Laser>()
            .ActivateOnEnter<R030Hammer>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<ChemicalBurn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9650)]
public class A24HeavyArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -100), new ArenaBoundsCustom(29.5f, new(CurveApprox.Donut(5.7f, 30, 0.02f))));
