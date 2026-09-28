namespace BossMod.Global.Crucible.DrakePiece;

public enum OID : uint
{
    Boss = 0x4CF0, // R3.600, x1
    _Gen_BallOfFire = 0x4CF1, // R1.500, x0 (spawn during fight)
    _Gen_BarbmolePiece = 0x4CF2, // R1.750, x0 (spawn during fight)
    _Gen_TwistingBlaze = 0x4CF3, // R1.000, x0 (spawn during fight)
    AbaddonPiece = 0x4CF4, // R4.800, x0 (spawn during fight)
    MorphoPiece = 0x4CF5, // R0.800, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x1, Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 49681, // Boss->player, no cast, single-target
    _Ability_ = 49257, // Boss->location, no cast, single-target
    _Weaponskill_BurningCyclone = 49255, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_BurningCyclone1 = 49256, // Helper->self, 5.0s cast, range 50 120-degree cone
    _Weaponskill_ArmOfPurgatory = 49258, // 4CF1->self, 1.0s cast, range 4 circle
    _Ability_BlazeSpikes = 49253, // Boss->self, 3.0s cast, single-target
    _AutoAttack_1 = 50397, // 4CF2->player, no cast, single-target
    _Weaponskill_ManglingFang = 49254, // Boss->player, 5.0s cast, single-target
    _Weaponskill_BlowingRingOfFire = 49259, // 4CF3->self, 5.0s cast, range 3-50 donut
    _AutoAttack_2 = 49682, // AbaddonPiece->player, no cast, single-target
    _Weaponskill_UnwittingWings = 49260, // MorphoPiece->player, 4.0s cast, single-target
    _Ability_NeedlesOut = 49250, // 4CF2->self, 3.0s cast, single-target
}

public enum SID : uint
{
    _Gen_BlazeSpikes = 5465, // Boss->Boss, extra=0x64
    _Gen_WitsEnd = 5146, // MorphoPiece->player, extra=0x1/0x4/0x6/0x7
    _Gen_ = 3572, // AbaddonPiece->MorphoPiece, extra=0x12
    _Gen_Rehabilitation = 989, // none->AbaddonPiece, extra=0x5/0xB/0x10
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix1f = 17, // MorphoPiece->player
}

class BurningCyclone(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BurningCyclone1, new AOEShapeCone(50, 60.Degrees()));
class ArmOfPurgatory(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ArmOfPurgatory, 4);
class BlazeSpikes(BossModule module) : Components.InvincibleStatus(module, (uint)SID._Gen_BlazeSpikes, "Spikes!", AIHints.Enemy.PriorityForbidden);
class Barbmole(BossModule module) : Components.Adds(module, (uint)OID._Gen_BarbmolePiece);
class BlowingRingOfFire(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BlowingRingOfFire, new AOEShapeDonut(3, 50));
class MorphoPiece(BossModule module) : Components.AddsPointless(module, (uint)OID.MorphoPiece);
class Abaddon(BossModule module) : Components.Adds(module, (uint)OID.AbaddonPiece)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == (uint)OID.AbaddonPiece))
        {
            target.Priority = target.Actor.FindStatus(SID._Gen_Rehabilitation)?.Extra > 4 ? AIHints.Enemy.PriorityPointless : 0;
        }
    }
}

class DrakePieceStates : StateMachineBuilder
{
    public DrakePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<BlazeSpikes>()
            .ActivateOnEnter<Barbmole>()
            .ActivateOnEnter<BlowingRingOfFire>()
            .ActivateOnEnter<MorphoPiece>()
            .ActivateOnEnter<Abaddon>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && ((DrakePiece)module).Abaddon is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14651)]
public class DrakePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15))
{
    public Actor? Abaddon;

    protected override void UpdateModule()
    {
        Abaddon ??= Enemies(OID.AbaddonPiece).FirstOrDefault();
    }
}

