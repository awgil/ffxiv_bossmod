#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.DurgaPiece;

public enum OID : uint
{
    Boss = 0x4CF6, // R7.500, x1
    _Gen_Missile = 0x4CF7, // R1.300, x8
    _Gen_SpinnerRookPiece = 0x4CF8, // R0.750, x0 (spawn during fight)
    _Gen_ = 0x4EA8, // R1.000, x4
    Helper = 0x233C, // R0.500, x14, mixed types
}

public enum AID : uint
{
    _AutoAttack_ = 49270, // Boss->player, no cast, single-target
    _Weaponskill_ElectricAbsorption = 49263, // Boss->self, 5.0s cast, single-target
    _Weaponskill_GroundingJolt = 49264, // Boss->self, 4.5+0.7s cast, single-target
    _Weaponskill_GroundingJolt1 = 49265, // Helper->self, 5.2s cast, range 20 circle
    _Weaponskill_GroundingJolt2 = 49266, // Helper->self, 5.2s cast, range 6 circle
    _Weaponskill_Missile = 49267, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Summon = 49269, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Voyage = 49268, // 4CF7->self, 1.0s cast, single-target
    _Weaponskill_Voyage1 = 50688, // Helper->self, 1.5s cast, range 100 width 4 rect
    _Ability_AetherCharge = 49271, // 4CF8->Boss, 3.0s cast, single-target
    _Weaponskill_AtomicRay = 49272, // Boss->self, 13.0s cast, range 60 circle
    _Weaponskill_ThermobaricCharge = 49273, // Boss->self, 4.2+0.8s cast, single-target
    _Weaponskill_ThermobaricCharge1 = 49274, // Helper->self, 2.0s cast, range 55 circle, distance 40 knockback
    _Weaponskill_Missile1 = 49512, // Boss->self, no cast, single-target
    _Weaponskill_DiffusionRay = 49275, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_DiffusionRay1 = 49276, // Helper->self, 5.0s cast, range 30 120-degree cone
}

public enum SID : uint
{
    _Gen_ = 2056, // Boss->4CF7/Boss, extra=0x476/0x23E/0xD1
    _Gen_1 = 2193, // Boss->Boss, extra=0x48C
    _Gen_Bleeding = 3077, // none->player, extra=0x0
    _Gen_Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_lockon5_t0h = 23, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0027_t1i2 = 417, // 4EA8->Boss
    _Gen_Tether_chn_m0027_t2i2 = 418, // 4EA8->Boss
    _Gen_Tether_chn_m0027_t0i2 = 416, // 4EA8->Boss
    _Gen_Tether_chn_m0027_t3i2 = 419, // 4EA8->Boss
}

class GroundingJoltSmall(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GroundingJolt2, 6);
class GroundingJoltLarge(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GroundingJolt1, 20);
class SpinnerRook(BossModule module) : Components.Adds(module, (uint)OID._Gen_SpinnerRookPiece);
class Voyage(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Voyage1, new AOEShapeRect(100, 2));
class AtomicRay(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_AtomicRay);

class DurgaPieceStates : StateMachineBuilder
{
    public DurgaPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GroundingJoltSmall>()
            .ActivateOnEnter<GroundingJoltLarge>()
            .ActivateOnEnter<SpinnerRook>()
            .ActivateOnEnter<Voyage>()
            .ActivateOnEnter<AtomicRay>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14657)]
public class DurgaPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

