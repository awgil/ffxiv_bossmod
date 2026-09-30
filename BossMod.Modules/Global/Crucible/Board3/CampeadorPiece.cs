#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.CampeadorPiece;

public enum OID : uint
{
    Boss = 0x4CA5, // R2.000, x1
    Helper = 0x233C, // R0.500, x7, Helper type
    _Gen_SabotenderPiece = 0x4CA6, // R0.800, x38
    _Gen_FlowertenderPiece = 0x4CA7, // R1.600, x2
    _Gen_SoldadoPiece = 0x4CA8, // R2.000, x4
    _Gen_GuardiaPiece = 0x4CA9, // R2.400, x1
}

public enum AID : uint
{
    _Weaponskill_EmergencyOrder = 48578, // Boss->self, 5.0s cast, single-target
    _AutoAttack_ = 49682, // 4CA6/4CA7/4CA8->player, no cast, single-target
    _Spell_HealingWater = 48579, // 4CA7->4CA6, 3.0s cast, range 6 circle
    _Weaponskill_Needles = 48580, // 4CA8->self, 8.0s cast, single-target
    _Weaponskill_Needles1 = 48581, // Helper->self, 8.0s cast, range 15 circle
}

public enum IconID : uint
{
    _Gen_Icon_sph_lockon2_num01_s8p = 336, // 4CA8->self
    _Gen_Icon_sph_lockon2_num02_s8p = 337, // 4CA8->self
    _Gen_Icon_sph_lockon2_num03_s8p = 338, // 4CA8->self
    _Gen_Icon_sph_lockon2_num04_s8p = 339, // 4CA8->self
}

class Flowertender(BossModule module) : Components.Adds(module, (uint)OID._Gen_FlowertenderPiece, 1);
class SabotenderPiece(BossModule module) : Components.AddsMulti(module, [OID._Gen_SabotenderPiece, OID._Gen_SoldadoPiece, OID._Gen_GuardiaPiece]);
class Needles(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Needles1, 15, 3);

class CampeadorPieceStates : StateMachineBuilder
{
    public CampeadorPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flowertender>()
            .ActivateOnEnter<SabotenderPiece>()
            .ActivateOnEnter<Needles>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14587)]
public class CampeadorPiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20));

