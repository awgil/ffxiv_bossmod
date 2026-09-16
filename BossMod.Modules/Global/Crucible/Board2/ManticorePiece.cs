#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.ManticorePiece;

public enum OID : uint
{
    Boss = 0x4C53,
    Helper = 0x233C,
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Weaponskill_ArmAndHammer = 48124, // Boss->self, 5.0+0.6s cast, single-target
    _Weaponskill_ArmAndHammer1 = 48125, // Helper->self, 5.6s cast, range 30 ?-degree cone
    _Weaponskill_ArmAndHammer2 = 48122, // Boss->self, 5.0+0.6s cast, single-target
    _Weaponskill_ArmAndHammer3 = 48123, // Helper->self, 5.6s cast, range 30 ?-degree cone
    _Weaponskill_DeadlyHold = 48138, // Boss->player, 5.0s cast, single-target
    _Ability_ = 48126, // Boss->location, no cast, single-target
    _Weaponskill_Hammerleap = 48135, // Boss->location, 7.0+1.1s cast, single-target
    _Weaponskill_Hammerleap1 = 48137, // Helper->self, 8.1s cast, range 30 circle
    _Weaponskill_TailsAndHeads = 50410, // Boss->self, 3.5+0.4s cast, single-target
    _Weaponskill_TailsAndHeads1 = 50411, // Helper->self, 3.9s cast, range 40 180-degree cone
    _Weaponskill_TailsAndHeads2 = 50412, // Boss->self, 2.0+0.4s cast, single-target
    _Weaponskill_TailsAndHeads3 = 50413, // Helper->self, 2.4s cast, range 40 180-degree cone
}

class ArmAndHammer(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_ArmAndHammer1, AID._Weaponskill_ArmAndHammer3], new AOEShapeCone(30, 90.Degrees()));
class DeadlyHold(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_DeadlyHold);
class Hammerleap(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Hammerleap1, 30);
class TailsAndHeads(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_TailsAndHeads1, AID._Weaponskill_TailsAndHeads3], new AOEShapeCone(40, 90.Degrees()));

class ManticorePieceStates : StateMachineBuilder
{
    public ManticorePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArmAndHammer>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Hammerleap>()
            .ActivateOnEnter<TailsAndHeads>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14545)]
public class ManticorePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

