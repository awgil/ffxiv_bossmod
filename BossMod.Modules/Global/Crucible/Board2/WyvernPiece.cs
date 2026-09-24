#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.WyvernPiece;

public enum OID : uint
{
    Boss = 0x4C58, // R2.880, x1
    _Gen_Whirlwind = 0x4C59, // R2.000, x0 (spawn during fight)
    _Gen_Whirlwind1 = 0x4C5A, // R3.000, x0 (spawn during fight)
    _Gen_WindSprite = 0x4C5B, // R1.600, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x12, Helper type

    LiquidHell = 0x1EA66D, // fire puddle
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
    _Weaponskill_StormTrail2 = 48177, // Boss->self, 5.0+1.0s cast, single-target
}

class TheStormsGrip(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_TheStormsGrip);
class Buffet(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Buffet, new AOEShapeRect(40, 5));
class Typhoon(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_Typhoon, 10);
class LiquidHell(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LiquidHell1, 6);
class LiquidHellPuddle(BossModule module) : Components.Voidzone(module, 5, OID.LiquidHell);
class WhirlwindSmall(BossModule module) : Components.Voidzone(module, 2, OID._Gen_Whirlwind, moveHintLength: 6);
class WhirlwindLarge(BossModule module) : Components.Voidzone(module, 3, OID._Gen_Whirlwind1, moveHintLength: 9);
class BlazingTrail(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_BlazingTrail1, new AOEShapeCone(60, 90.Degrees()));
class StormTrail(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_StormTrail, new AOEShapeCone(25, 30.Degrees()));

class WyvernPieceStates : StateMachineBuilder
{
    public WyvernPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheStormsGrip>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Typhoon>()
            .ActivateOnEnter<LiquidHell>()
            .ActivateOnEnter<LiquidHellPuddle>()
            .ActivateOnEnter<BlazingTrail>()
            .ActivateOnEnter<StormTrail>()
            .ActivateOnEnter<WhirlwindSmall>()
            .ActivateOnEnter<WhirlwindLarge>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14549)]
public class WyvernPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

