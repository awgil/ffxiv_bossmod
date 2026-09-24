namespace BossMod.Global.Crucible.TaurusPiece;

public enum OID : uint
{
    Boss = 0x4C54, // R2.240, x1
    Helper = 0x233C, // R0.500, x3 (spawn during fight), Helper type
    _Gen_AethericCharge = 0x4C55, // R1.000-3.010, x0 (spawn during fight)
    _Gen_TaurusPiece = 0x4C56, // R1.500, x0 (spawn during fight)
    _Gen_AhrimanPiece = 0x4C57, // R0.900, x0 (spawn during fight)
    _Gen_ = 0x4C8D, // R1.000, x10
}

public enum AID : uint
{
    _AutoAttack_ = 50784, // Boss->player, no cast, single-target
    _Weaponskill_MortalRay = 48143, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_RuinousLocus = 48154, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousLocus1 = 48155, // Helper->self, 6.0s cast, range 8 circle
    _Weaponskill_RuinousRing = 48156, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousRing1 = 48157, // Helper->self, 6.0s cast, range 8?-50 donut
    _Weaponskill_RayOfIgnorance = 48144, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_RayOfIgnorance1 = 48145, // Helper->player, no cast, single-target
    _Weaponskill_Burst = 48146, // 4C55->self, 1.5s cast, range 6 circle
    _Weaponskill_AetherialFissure = 48149, // Boss->self, 4.0s cast, single-target
    _Weaponskill_Burst1 = 48147, // 4C55->self, 1.5s cast, range 12 circle
    _Weaponskill_Aetherwave = 48150, // 4C56->self, 6.0s cast, range 50 width 10 rect
    _Weaponskill_Burst2 = 48148, // 4C55->self, 1.5s cast, range 18 circle
    _Weaponskill_Summon = 48151, // Boss->self, 3.0s cast, single-target
    _Weaponskill_MortalGaze = 48152, // 4C57->self, 4.5+0.5s cast, single-target
    _Weaponskill_MortalGaze1 = 48153, // Helper->self, 5.0s cast, range 50 circle
    _Spell_Stone = 48625, // 4C57->player, no cast, single-target
    _Weaponskill_RuinousExpansion = 48158, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousLocus2 = 48159, // Helper->self, 6.0s cast, range 8 circle
    _Weaponskill_RuinousExpansion1 = 48160, // Boss->self, no cast, single-target
    _Weaponskill_RuinousRing2 = 48161, // Helper->self, 9.5s cast, range ?-50 donut
    _Weaponskill_EyesOnMe = 48372, // 4C57->self, 10.0s cast, range 60 circle
    _Weaponskill_RuinousContraction = 48162, // Boss->self, 4.5+1.5s cast, single-target
    _Weaponskill_RuinousRing3 = 48163, // Helper->self, 6.0s cast, range ?-50 donut
    _Weaponskill_RuinousContraction1 = 48164, // Boss->self, no cast, single-target
    _Weaponskill_RuinousLocus3 = 48165, // Helper->self, 9.5s cast, range 8 circle
}

public enum SID : uint
{
    _Gen_Doom = 5421, // Boss->player, extra=0x358
    _Gen_ = 4215, // none->4C55, extra=0x1/0x2/0x3
}

public enum IconID : uint
{
    _Gen_Icon_lockon6_t0t = 234, // player->self
    _Gen_Icon_shisen_lockon_c0y2 = 667, // 4C57->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_sinentai01p = 102, // 4C8D->Boss
}

class TaurusPieceStates : StateMachineBuilder
{
    public TaurusPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// platforms width 5 ("radius" 2.5)
// 520, -7.5
// 520, 7.5
// 530, 0
// 510, 0
[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14546)]
public class TaurusPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

