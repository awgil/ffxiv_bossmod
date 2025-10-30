namespace BossMod.Shadowbringers.Alliance.A24HeavyArtilleryUnit;

public enum OID : uint
{
    Boss = 0x2E6A,
    Helper = 0x233C,
    TetherHelper = 0x18D6, // R0.500, x9 (spawn during fight)
    Energy = 0x2E6C, // R1.000, x0 (spawn during fight)
    Pod = 0x2E6D, // R0.800, x0 (spawn during fight)
    ChemicalBurn = 0x1EB055
}

public enum AID : uint
{
    AutoAttack = 21561, // Boss->player, no cast, single-target
    ManeuverVoltArray = 20486, // Boss->self, 4.0s cast, range 60 circle
    OperationActivateLaserTurret = 20461, // Boss->self, 6.0s cast, single-target
    LowerLaserCast = 20614, // Helper->self, 1.8s cast, range 30 60-degree cone
    LowerLaserRepeat = 20470, // Helper->self, no cast, range 60 60-degree cone
    UpperLaser1Cast = 20615, // Helper->self, 1.8s cast, range 16 60-degree cone
    UpperLaser1Repeat = 20471, // Helper->self, no cast, range 16 60-degree cone
    UpperLaser2Cast = 20616, // Helper->self, 4.8s cast, range 14-23 donut
    UpperLaser2Repeat = 20472, // Helper->self, no cast, range 14-23 donut
    UpperLaser3Cast = 20617, // Helper->self, 7.8s cast, range 21-30 donut
    UpperLaser3Repeat = 20473, // Helper->self, no cast, range 21-30 donut
    ManeuverHighPoweredLaserCast = 20481, // Boss->self, 5.0s cast, single-target
    ManeuverHighPoweredLaser = 20482, // Boss->self, no cast, range 60 width 8 rect
    UnconventionalVoltageTargetSelect = 20485, // Helper->player, no cast, single-target
    ManeuverUnconventionalVoltage = 20483, // Boss->self, 6.0s cast, single-target
    UnconventionalVoltage = 20484, // Helper->self, no cast, range 60 30-degree cone
    EnergyBombardmentCast = 20475, // Boss->self, 2.0s cast, single-target
    EnergyBombardment = 20476, // Helper->location, 3.0s cast, range 4 circle
    ManeuverImpactCrusher = 20477, // Boss->self, 4.0s cast, single-target
    ManeuverImpactCrusherAOE = 20479, // Helper->location, 4.0s cast, range 8 circle
    ManeuverImpactCrusherJump = 20478, // Boss->location, no cast, range 8 circle
    ManeuverRevolvingLaser = 20480, // Boss->self, 3.0s cast, range 12-60 donut
    OperationAccessSelfConsciousnessData = 20463, // Boss->self, 8.0s cast, single-target
    OperationActivateSuppressiveUnit = 20462, // Boss->self, 6.0s cast, single-target
    EnergyBomb = 20474, // Energy->player, no cast, single-target
    SupportPod = 20457, // Boss->self, 2.0s cast, single-target
    OperationPodProgram = 20458, // Boss->self, 6.0s cast, single-target
    R010Laser = 20464, // Pod->self, 10.0s cast, range 60 width 12 rect
    R030Hammer = 20465, // Pod->self, 10.0s cast, range 18 circle
    OperationSynthesizeCompound = 20460, // Boss->self, 3.0s cast, single-target
    ChemicalBurn = 20468, // Helper->self, no cast, range 3 circle
    ChemicalConflagration = 20469, // Helper->self, no cast, range 60 circle
}

public enum IconID : uint
{
    Tankbuster = 230, // player->self
    UnconventionalVoltage = 172, // player->self
}

public enum TetherID : uint
{
    BossTether = 122, // Boss->18D6
    PodTether = 123, // 18D6->2E6D/Helper
}

class ManeuverVoltArray(BossModule module) : Components.RaidwideCast(module, AID.ManeuverVoltArray);
class HighPoweredLaser(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60, 4), (uint)IconID.Tankbuster, AID.ManeuverHighPoweredLaser, 5.7f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && CurrentBaits.Count > 0)
            CurrentBaits.RemoveAt(0);
    }
}
class UnconventionalVoltage(BossModule module) : Components.GenericBaitAway(module, AID.UnconventionalVoltage)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UnconventionalVoltageTargetSelect && WorldState.Actors.Find(spell.MainTargetID) is { } tar)
            CurrentBaits.Add(new(caster, tar, new AOEShapeCone(60, 15.Degrees()), WorldState.FutureTime(6.8f)));

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}
class EnergyBombardment(BossModule module) : Components.StandardAOEs(module, AID.EnergyBombardment, 4);
class ImpactCrusher(BossModule module) : Components.StandardAOEs(module, AID.ManeuverImpactCrusherAOE, 8)
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

        if ((AID)spell.Action.ID == AID.ManeuverRevolvingLaser)
            _hintLocation = null;
    }
}
class RevolvingLaser(BossModule module) : Components.StandardAOEs(module, AID.ManeuverRevolvingLaser, new AOEShapeDonut(12, 60));
class R010Laser(BossModule module) : Components.StandardAOEs(module, AID.R010Laser, new AOEShapeRect(60, 6));
class R030Hammer(BossModule module) : Components.StandardAOEs(module, AID.R030Hammer, new AOEShapeCircle(18));
class Energy : Components.PersistentVoidzone
{
    private readonly List<Actor> _balls = [];

    public Energy(BossModule module) : base(module, 2, _ => [], 8)
    {
        Sources = _ => _balls;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EnergyBomb)
            _balls.Remove(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Energy)
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
        if ((AID)spell.Action.ID is AID.ChemicalBurn or AID.ChemicalConflagration)
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9650)]
public class A24HeavyArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -100), new ArenaBoundsCustom(29.5f, new(CurveApprox.Donut(5.7f, 30, 0.02f))));
