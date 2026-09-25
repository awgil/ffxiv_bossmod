#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.WyvernPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x12, Helper type
    Boss = 0x4C58, // R2.880, x1
    _Gen_WindSprite = 0x4C5B, // R1.600, x0 (spawn during fight)
    _Gen_Whirlwind = 0x4C59, // R2.000, x0 (spawn during fight)
    _BurnZone = 0x1EA66D,
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Weaponskill_TheStormsGrip = 48166, // Boss->self, 4.0s cast, range 60 circle
    _Weaponskill_Buffet = 48167, // 4C5B->self, 6.0s cast, range 40 width 10 rect
    _Weaponskill_Typhoon = 48168, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_StrongWind = 48169, // 4C59->player, no cast, single-target
    _Ability_ = 48173, // Boss->location, no cast, single-target
    _Weaponskill_LiquidHell = 48171, // Boss->self, 3.0s cast, single-target
    _Weaponskill_LiquidHell1 = 48172, // Helper->self, 6.0s cast, range 6 circle
    _Weaponskill_BlazingTrail = 48174, // Boss->self, 7.0+1.0s cast, single-target
    _Weaponskill_BlazingTrail1 = 48175, // Helper->self, 8.0s cast, range 60 180-degree cone
    _Weaponskill_StormTrail = 48178, // Helper->self, 6.0s cast, range 25 60-degree cone
    _Weaponskill_StormTrail1 = 48176, // Boss->self, 5.0+1.0s cast, single-target
}

class TheStormsGrip(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_TheStormsGrip);
class Buffet(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Buffet, new AOEShapeRect(40f, 5f));
class Typhoon(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_Typhoon, 8);
class LiquidHell(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LiquidHell1, 6f);
class Whrilwinds(BossModule module) : Components.Voidzone(module, 2, OID._Gen_Whirlwind, (a) => a.IsDead, 5f);
class BurnZones(BossModule module) : Components.Voidzone(module, 6, OID._BurnZone);
class BlazingTrail(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BlazingTrail1, new AOEShapeCone(60, 90.Degrees()));
class WyvernPieceStates : StateMachineBuilder
{
    public WyvernPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheStormsGrip>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Typhoon>()
            .ActivateOnEnter<LiquidHell>()
            .ActivateOnEnter<Whrilwinds>()
            .ActivateOnEnter<BlazingTrail>()
            .ActivateOnEnter<BurnZones>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14549)]
public class WyvernPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

