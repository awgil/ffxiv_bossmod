namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

public enum OID : uint
{
    Boss = 0x2EC4,
    BossP2 = 0x2EC6,
    Helper = 0x233C,
    ThePuppets1 = 0x2EC5, // R5.290, x0 (spawn during fight)
    ThePuppets2 = 0x2FA4, // R1.000, x0 (spawn during fight)
    ThePuppets3 = 0x2FA5, // R0.500, x0 (spawn during fight)
    CompoundPod = 0x2EC8, // R1.360, x0 (spawn during fight)
    Puppet2P = 0x2EC7, // R6.000, x0 (spawn during fight)
    EnergyCompressionTower1 = 0x1EB06E
}

public enum AID : uint
{
    AutoAttack = 21450, // Boss->player, no cast, single-target
    MechanicalLaceration = 20920, // Boss->self, 5.0s cast, range 100 circle
    MechanicalDecapitation = 20916, // Boss->self, 6.0s cast, range 7.5-43 donut
    MechanicalDissection = 20915, // Boss->self, 6.0s cast, range 85 width 11 rect
    MechanicalContusionCast = 20917, // Boss->self, 3.0s cast, single-target
    MechanicalContusionAOE = 20919, // Helper->location, 4.0s cast, range 6 circle
    MechanicalContusionSpread = 20918, // Helper->players, 5.0s cast, range 6 circle
    IncongruousSpinCast = 20913, // ThePuppets1->self, 8.0s cast, single-target
    IncongruousSpin = 20914, // Helper->self, 8.5s cast, range 80 width 150 rect
    MechanicalLacerationInstant = 21461, // Boss->self, no cast, range 100 circle

    AutoAttackP2 = 21490, // BossP2->player, no cast, single-target
    CentrifugalSlice = 20912, // BossP2->self, 5.0s cast, range 100 circle
    RelentlessSpiralCast = 20905, // BossP2->self, 3.5s cast, single-target
    RelentlessSpiralAOE = 20906, // Helper->location, 3.5s cast, range 8 circle
    PrimeBladeCircle = 21535, // BossP2->self, 7.0s cast, range 20 circle
    PrimeBladeRect = 21536, // BossP2->self, 7.0s cast, range 85 width 20 rect
    PrimeBladeDonut = 21537, // BossP2/Puppet2P->self, 7.0s cast, range 8-43 donut
    RelentlessSpiralTransferred = 20939, // Helper->self, 1.0s cast, range 8 circle
    PrimeBladeDonutSlow = 20890, // BossP2->self, 10.0s cast, range 8-43 donut
    ThreePartsDisdainCast = 20891, // BossP2->players, 6.0s cast, range 6 circle
    ThreePartsDisdain2 = 20892, // BossP2->players, no cast, range 6 circle
    ThreePartsDisdain3 = 20893, // BossP2->players, no cast, range 6 circle
    CompoundPodR012 = 20907, // BossP2->self, 3.0s cast, single-target
    R012Laser = 20908, // CompoundPod->self, no cast, single-target
    R012LaserGround = 20911, // Helper->location, 4.0s cast, range 6 circle
    R012LaserSpread = 20910, // Helper->players, 5.0s cast, range 6 circle
    R012LaserTank = 20909, // Helper->players, 5.0s cast, range 6 circle
    FourPartsResolveCast = 20894, // BossP2->self, 8.0s cast, single-target
    FourPartsResolveJump = 20895, // BossP2->players, no cast, range 6 circle
    FourPartsResolveRect = 20896, // BossP2->self, no cast, range 85 width 12 rect
    Reproduce = 20897, // BossP2->self, 4.0s cast, single-target
    EnergyCompression = 20902, // BossP2->self, 4.0s cast, single-target
    ForcedTransfer = 20898, // BossP2->self, 6.5s cast, single-target
    ForcedTransfer2 = 21562, // BossP2->self, 8.5s cast, single-target
    Explosion = 20903, // Helper->self, no cast, range 5 circle, tower
    BigExplosion = 20904, // Helper->self, no cast, range 100 circle, tower fail
    CompoundPodR011 = 20900, // BossP2->self, 4.0s cast, single-target
    R011LaserCast = 20901, // CompoundPod->self, 11.5s cast, single-target
    R011LaserAOE = 21531, // Helper->self, 1.5s cast, range 70 width 15 rect

    //_Weaponskill_ = 20899, // BossP2->location, no cast, single-target
    //_Weaponskill_1 = 21347, // Helper->location, no cast, single-target
    //_Weaponskill_2 = 21456, // Helper->self, no cast, single-target
    //_Weaponskill_3 = 21457, // Helper->self, no cast, single-target
}

public enum IconID : uint
{
    Target = 139, // player->self
    Stack = 62, // player->self
    Tankbuster = 218, // player->self
    Order1 = 79, // player->self
    Order2 = 80, // player->self
    Order3 = 81, // player->self
    Order4 = 82, // player->self
}

public enum TetherID : uint
{
    Transfer = 116, // Helper->Helper
    NoTransfer = 117, // Helper->Helper
}

class MechanicalLaceration(BossModule module) : Components.RaidwideCast(module, AID.MechanicalLaceration);
class MechanicalDecapitation(BossModule module) : Components.StandardAOEs(module, AID.MechanicalDecapitation, new AOEShapeDonut(7.5f, 43));
class MechanicalDissection(BossModule module) : Components.StandardAOEs(module, AID.MechanicalDissection, new AOEShapeRect(85, 5.5f));
class MechanicalContusion1(BossModule module) : Components.StandardAOEs(module, AID.MechanicalContusionAOE, 6);
class MechanicalContusion2(BossModule module) : Components.SpreadFromCastTargets(module, AID.MechanicalContusionSpread, 6);
class IncongruousSpin(BossModule module) : Components.StandardAOEs(module, AID.IncongruousSpin, new AOEShapeRect(80, 75));
class CentrifugalSlice(BossModule module) : Components.RaidwideCast(module, AID.CentrifugalSlice);
class RelentlessSpiral(BossModule module) : Components.StandardAOEs(module, AID.RelentlessSpiralAOE, 8);
class PrimeBladeCircle(BossModule module) : Components.StandardAOEs(module, AID.PrimeBladeCircle, new AOEShapeCircle(20));
class PrimeBladeRect(BossModule module) : Components.StandardAOEs(module, AID.PrimeBladeRect, new AOEShapeRect(85, 10));
class PrimeBladeDonut(BossModule module) : Components.StandardAOEs(module, AID.PrimeBladeDonut, new AOEShapeDonut(8, 43));
class RelentlessSpiralTeleport(BossModule module) : Components.StandardAOEs(module, AID.RelentlessSpiralTransferred, 8);
class PrimeBladeDonutTeleport(BossModule module) : Components.GenericAOEs(module, AID.PrimeBladeDonutSlow)
{
    private Actor? _caster;
    private WPos? _location;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster is { } c && _location is { } l)
            yield return new AOEInstance(new AOEShapeDonut(8, 43), l, default, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = caster;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.Helper && (TetherID)tether.ID == TetherID.Transfer)
            _location = source.Position;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _caster = null;
            _location = null;
        }
    }
}
class ThreePartsDisdain(BossModule module) : Components.StackWithCastTargets(module, AID.ThreePartsDisdainCast, 6);
class R012LaserGround(BossModule module) : Components.StandardAOEs(module, AID.R012LaserGround, 6);
class R012LaserSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.R012LaserSpread, 6);
class R012LaserTank(BossModule module) : Components.BaitAwayCast(module, AID.R012LaserTank, new AOEShapeCircle(6), centerAtTarget: true);

class A25Compound2PStates : StateMachineBuilder
{
    public A25Compound2PStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MechanicalLaceration>()
            .ActivateOnEnter<MechanicalDecapitation>()
            .ActivateOnEnter<MechanicalDissection>()
            .ActivateOnEnter<MechanicalContusion1>()
            .ActivateOnEnter<MechanicalContusion2>()
            .ActivateOnEnter<IncongruousSpin>()
            .ActivateOnEnter<CentrifugalSlice>()
            .ActivateOnEnter<RelentlessSpiral>()
            .ActivateOnEnter<PrimeBladeCircle>()
            .ActivateOnEnter<PrimeBladeRect>()
            .ActivateOnEnter<PrimeBladeDonut>()
            .ActivateOnEnter<PrimeBladeDonutTeleport>()
            .ActivateOnEnter<RelentlessSpiralTeleport>()
            .ActivateOnEnter<ThreePartsDisdain>()
            .ActivateOnEnter<R012LaserGround>()
            .ActivateOnEnter<R012LaserSpread>()
            .ActivateOnEnter<R012LaserTank>()
            .ActivateOnEnter<FourPartsResolve>()
            .ActivateOnEnter<EnergyCompression1>()
            .ActivateOnEnter<R011Laser>()
            .ActivateOnEnter<R011LaserAOE>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && Module.Enemies(OID.BossP2).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646)]
public class A25Compound2P(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsSquare(30))
{
    public Actor? BossP2 => Enemies(OID.BossP2).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BossP2), ArenaColor.Enemy);
    }
}
