namespace BossMod.Global.Crucible.BanemitePiece;

public enum OID : uint
{
    Boss = 0x4B8B,
    Helper = 0x233C,
    _Gen_MitelingPiece = 0x4B8C, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 50398, // Boss->player, no cast, single-target
    _Weaponskill_BedrockUplift = 46901, // Boss->self, 4.0s cast, single-target
    _Weaponskill_BedrockUplift1 = 46902, // Helper->self, 5.0s cast, range 6 circle
    _Weaponskill_BedrockUplift2 = 46903, // Helper->self, 7.0s cast, range 6-12 donut
    _Weaponskill_BedrockUplift3 = 46904, // Helper->self, 9.0s cast, range 12-18 donut
    _Weaponskill_BedrockUplift4 = 46905, // Helper->self, 11.0s cast, range 18-24 donut
    _Weaponskill_DeadlyThrust = 46906, // Boss->player, 5.0s cast, single-target
    _Weaponskill_VenomWeb = 46907, // Boss->self, 3.0s cast, single-target
    _Weaponskill_VenomWeb1 = 46908, // Helper->location, 6.0s cast, range 9 circle
    _AutoAttack_1 = 49682, // 4B8C->player, no cast, single-target
    _Weaponskill_Silkscreen = 46909, // 4B8C->self, 5.0s cast, range 40 width 4 rect
}

class BedrockUplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_BedrockUplift1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_BedrockUplift1 => 0,
            AID._Weaponskill_BedrockUplift2 => 1,
            AID._Weaponskill_BedrockUplift3 => 2,
            AID._Weaponskill_BedrockUplift4 => 3,
            _ => -1
        };

        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class DeadlyThrust(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_DeadlyThrust, "Tankbuster + poison");

class VenomWeb(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_VenomWeb1, 9, 8);
class MitelingPiece(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_MitelingPiece);
class Silkscreen(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Silkscreen, new AOEShapeRect(40, 2));

class BanemitePieceStates : StateMachineBuilder
{
    public BanemitePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BedrockUplift>()
            .ActivateOnEnter<DeadlyThrust>()
            .ActivateOnEnter<VenomWeb>()
            .ActivateOnEnter<MitelingPiece>()
            .ActivateOnEnter<Silkscreen>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14536)]
public class BanemitePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

