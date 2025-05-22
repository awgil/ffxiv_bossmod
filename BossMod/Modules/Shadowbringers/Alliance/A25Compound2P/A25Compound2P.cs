namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

public enum OID : uint
{
    Boss = 0x2EC4,
    BossP2 = 0x2EC6,
    Helper = 0x233C,
    _Gen_ThePuppets = 0x2EC5, // R5.290, x0 (spawn during fight)
    _Gen_ThePuppets1 = 0x2FA5, // R0.500, x0 (spawn during fight)
    _Gen_ThePuppets2 = 0x2FA4, // R1.000, x0 (spawn during fight)
    _Gen_CompoundPod = 0x2EC8, // R1.360, x0 (spawn during fight)
    _Gen_Puppet2P = 0x2EC7, // R6.000, x0 (spawn during fight)
    EnergyCompressionTower1 = 0x1EB06E
}

public enum AID : uint
{
    _AutoAttack_ = 21450, // Boss->player, no cast, single-target
    _Weaponskill_MechanicalLaceration = 20920, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_MechanicalDecapitation = 20916, // Boss->self, 6.0s cast, range 7.5-43 donut
    _Weaponskill_MechanicalDissection = 20915, // Boss->self, 6.0s cast, range 85 width 11 rect
    _Weaponskill_MechanicalContusion = 20917, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MechanicalContusion1 = 20919, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_MechanicalContusion2 = 20918, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_IncongruousSpin = 20913, // 2EC5->self, 8.0s cast, single-target
    _Weaponskill_IncongruousSpin1 = 20914, // Helper->self, 8.5s cast, range 80 width 150 rect
    _Weaponskill_MechanicalLaceration1 = 21461, // Boss->self, no cast, range 100 circle
    _AutoAttack_1 = 21490, // BossP2->player, no cast, single-target
    _Weaponskill_CentrifugalSlice = 20912, // BossP2->self, 5.0s cast, range 100 circle
    _Weaponskill_RelentlessSpiral = 20905, // BossP2->self, 3.5s cast, single-target
    _Weaponskill_RelentlessSpiral1 = 20906, // Helper->location, 3.5s cast, range 8 circle
    _Weaponskill_PrimeBlade = 21535, // BossP2->self, 7.0s cast, range 20 circle
    _Weaponskill_PrimeBlade1 = 21536, // BossP2->self, 7.0s cast, range 85 width 20 rect
    _Weaponskill_PrimeBlade2 = 21537, // BossP2/2EC7->self, 7.0s cast, range ?-43 donut
    _Weaponskill_ = 20899, // BossP2->location, no cast, single-target
    _Weaponskill_1 = 21347, // Helper->location, no cast, single-target
    _Weaponskill_2 = 21456, // Helper->self, no cast, single-target
    _Weaponskill_RelentlessSpiral2 = 20939, // Helper->self, 1.0s cast, range 8 circle
    _Weaponskill_PrimeBlade3 = 20890, // BossP2->self, 10.0s cast, range ?-43 donut
    _Weaponskill_ThreePartsDisdain = 20891, // BossP2->players, 6.0s cast, range 6 circle
    _Weaponskill_ThreePartsDisdain1 = 20892, // BossP2->players, no cast, range 6 circle
    _Weaponskill_ThreePartsDisdain2 = 20893, // BossP2->players, no cast, range 6 circle
    _Weaponskill_CompoundPodR012 = 20907, // BossP2->self, 3.0s cast, single-target
    _Weaponskill_R012Laser = 20908, // 2EC8->self, no cast, single-target
    _Weaponskill_R012Laser1 = 20911, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_R012Laser2 = 20910, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_R012Laser3 = 20909, // Helper->players, 5.0s cast, range 6 circle
    _Weaponskill_FourPartsResolve = 20894, // BossP2->self, 8.0s cast, single-target
    _Weaponskill_FourPartsResolve1 = 20895, // BossP2->players, no cast, range 6 circle
    _Weaponskill_FourPartsResolve2 = 20896, // BossP2->self, no cast, range 85 width 12 rect
    _Weaponskill_Reproduce = 20897, // BossP2->self, 4.0s cast, single-target
    _Weaponskill_3 = 21457, // Helper->self, no cast, single-target
    _Weaponskill_EnergyCompression = 20902, // BossP2->self, 4.0s cast, single-target
    _Weaponskill_Explosion = 20903, // Helper->self, no cast, range 5 circle
    _Weaponskill_ForcedTransfer = 20898, // BossP2->self, 6.5s cast, single-target
    _Weaponskill_BigExplosion = 20904, // Helper->self, no cast, range 100 circle
    _Weaponskill_CompoundPodR011 = 20900, // BossP2->self, 4.0s cast, single-target
    _Weaponskill_ForcedTransfer1 = 21562, // BossP2->self, 8.5s cast, single-target
    _Weaponskill_R011Laser = 20901, // 2EC8->self, 11.5s cast, single-target
    _Weaponskill_R011Laser1 = 21531, // Helper->self, 1.5s cast, range 70 width 15 rect
}

public enum IconID : uint
{
    _Gen_Icon_target_ae_s5f = 139, // player->self
    _Gen_Icon_com_share0c = 62, // player->self
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
    _Gen_Icon_m0361trg_a1t = 79, // player->self
    _Gen_Icon_m0361trg_a2t = 80, // player->self
    _Gen_Icon_m0361trg_a3t = 81, // player->self
    _Gen_Icon_m0361trg_a4t = 82, // player->self
}

public enum TetherID : uint
{
    Transfer = 116, // Helper->Helper
    NoTransfer = 117, // Helper->Helper
    _Gen_Tether_chn_d1007_01f = 41, // player->BossP2
    _Gen_Tether_chn_m0354_0c = 54, // 2EC7->BossP2
}

class MechanicalLaceration(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_MechanicalLaceration);
class MechanicalDecapitation(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MechanicalDecapitation, new AOEShapeDonut(7.5f, 43));
class MechanicalDissection(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MechanicalDissection, new AOEShapeRect(85, 5.5f));
class MechanicalContusion1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MechanicalContusion1, 6);
class MechanicalContusion2(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_MechanicalContusion2, 6);
class IncongruousSpin(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_IncongruousSpin1, new AOEShapeRect(80, 75));
class CentrifugalSlice(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_CentrifugalSlice);
class RelentlessSpiral(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RelentlessSpiral1, 8);
class PrimeBladeCircle(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PrimeBlade, new AOEShapeCircle(20));
class PrimeBladeRect(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PrimeBlade1, new AOEShapeRect(85, 10));
class PrimeBladeDonut(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PrimeBlade2, new AOEShapeDonut(8, 43));
class RelentlessSpiralTeleport(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RelentlessSpiral2, 8);
class PrimeBladeDonutTeleport(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_PrimeBlade3)
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
class ThreePartsDisdain(BossModule module) : Components.StackWithCastTargets(module, AID._Weaponskill_ThreePartsDisdain, 6);
class R012LaserGround(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_R012Laser1, 6);
class R012LaserSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_R012Laser2, 6);
class R012LaserTank(BossModule module) : Components.SpreadFromCastTargets(module, AID._Weaponskill_R012Laser3, 6);

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

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646)]
public class A25Compound2P(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsSquare(30))
{
    public Actor? BossP2 => Enemies(OID.BossP2).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BossP2), ArenaColor.Enemy);
    }
}
