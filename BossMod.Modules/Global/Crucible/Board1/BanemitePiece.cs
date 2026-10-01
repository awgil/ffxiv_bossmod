namespace BossMod.Global.Crucible.BanemitePiece;

public enum OID : uint
{
    Boss = 0x4B8B,
    MitelingPiece = 0x4B8C, // R1.800, x0 (spawn during fight)
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 50398, // Boss->player, no cast, single-target
    BedrockUpliftCast = 46901, // Boss->self, 4.0s cast, single-target
    BedrockUplift1 = 46902, // Helper->self, 5.0s cast, range 6 circle
    BedrockUplift2 = 46903, // Helper->self, 7.0s cast, range 6-12 donut
    BedrockUplift3 = 46904, // Helper->self, 9.0s cast, range 12-18 donut
    BedrockUplift4 = 46905, // Helper->self, 11.0s cast, range 18-24 donut
    DeadlyThrust = 46906, // Boss->player, 5.0s cast, single-target
    VenomWebCast = 46907, // Boss->self, 3.0s cast, single-target
    VenomWeb = 46908, // Helper->location, 6.0s cast, range 9 circle
    MitelingAuto = 49682, // 4B8C->player, no cast, single-target
    Silkscreen = 46909, // 4B8C->self, 5.0s cast, range 40 width 4 rect
}

class BedrockUplift(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BedrockUplift1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.BedrockUplift1 => 0,
            AID.BedrockUplift2 => 1,
            AID.BedrockUplift3 => 2,
            AID.BedrockUplift4 => 3,
            _ => -1
        };

        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class DeadlyThrust(BossModule module) : Components.SingleTargetCast(module, AID.DeadlyThrust, "Tankbuster + poison");
class VenomWeb(BossModule module) : Components.StandardAOEs(module, AID.VenomWeb, 9, 8);
class MitelingPiece(BossModule module) : ProximityAdds(module, OID.MitelingPiece);
class Silkscreen(BossModule module) : Components.StandardAOEs(module, AID.Silkscreen, new AOEShapeRect(40, 2));

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

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14536)]
public class BanemitePiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20));

