#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.FlaurosPiece;

public enum OID : uint
{
    Boss = 0x4CDB, // R5.040, x1
    LightningSprite = 0x4CDC, // R0.800-1.488, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x5, Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Weaponskill_HeatLightning = 49185, // Boss->self, 4.0s cast, single-target
    _Weaponskill_HeatLightning1 = 49186, // Helper->location, 4.0s cast, range 6 circle
    _Weaponskill_LineVoltage = 49194, // 4CDC->self, 2.0s cast, range 100 width 2 rect
    _Weaponskill_ChargedLightning = 49192, // Boss->self, 5.0+0.5s cast, single-target
    _Weaponskill_ChargedLightning1 = 49193, // Helper->self, 5.8s cast, range 50 width 40 rect
    _Weaponskill_AetherialOffering = 49187, // Boss->self, 5.0s cast, single-target
    _Weaponskill_LineVoltage1 = 49195, // 4CDC->self, 3.0s cast, range 100 width 6 rect
    _Weaponskill_ErraticBlaster = 49188, // Boss->self, 6.0+1.0s cast, single-target
    _Weaponskill_ErraticBlaster1 = 49189, // Helper->player, 7.0s cast, single-target
    _Weaponskill_ElectricShock = 49190, // Boss->self, 5.5+0.5s cast, single-target
    _Weaponskill_ElectricShock1 = 49191, // Helper->self, 6.0s cast, range 16 circle
}

class LightningSprite(BossModule module) : Components.AddsPointless(module, (uint)OID.LightningSprite);

class HeatLightning(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_HeatLightning1, 6);
class LineVoltageSmall(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LineVoltage, new AOEShapeRect(100, 1));
class LineVoltageBig(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LineVoltage1, new AOEShapeRect(100, 3));
class ChargedLightning(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ChargedLightning1, new AOEShapeRect(50, 20));
class ErraticBlaster(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_ErraticBlaster1, "Tankbuster + paralysis");
class ElectricShock(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_ElectricShock1, 16);

class FlaurosPieceStates : StateMachineBuilder
{
    public FlaurosPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningSprite>()
            .ActivateOnEnter<HeatLightning>()
            .ActivateOnEnter<LineVoltageSmall>()
            .ActivateOnEnter<LineVoltageBig>()
            .ActivateOnEnter<ChargedLightning>()
            .ActivateOnEnter<ErraticBlaster>()
            .ActivateOnEnter<ElectricShock>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14631)]
public class FlaurosPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

