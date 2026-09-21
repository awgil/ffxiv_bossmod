#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.SirenPiece;

public enum OID : uint
{
    Boss = 0x4CA1, // R4.000, x1
    Helper = 0x233C, // R0.500, x9, Helper type
    _Gen_ShamblingPiece = 0x4CA3, // R0.750, x0 (spawn during fight)
    _Gen_CrawlingPiece = 0x4CA4, // R0.750, x0 (spawn during fight)
    _Gen_SweetSong = 0x4CA2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 50395, // Boss->players, no cast, range 9 ?-degree cone
    _Weaponskill_SongOfTorment = 48563, // Boss->player, 5.0s cast, single-target
    _Ability_ = 48564, // Boss->location, no cast, single-target
    _Weaponskill_UnmooringMelody = 48565, // Boss->self, no cast, single-target
    _Weaponskill_UnmooringMelody1 = 48566, // Helper->self, no cast, range 50 ?-degree cone
    _Weaponskill_FeralLunge = 48569, // Boss->self, 3.8+0.2s cast, single-target
    _Weaponskill_FeralLunge1 = 48570, // Helper->self, 4.0s cast, range 50 width 16 rect
    _Weaponskill_Summon = 48567, // Boss->self, 3.0s cast, single-target
    _AutoAttack_1 = 49682, // 4CA3->none, no cast, single-target
    _Weaponskill_DeadMansDirge = 48572, // Boss->self, 5.6+1.4s cast, single-target
    _Weaponskill_DeadMansDirge1 = 48573, // Helper->self, 7.0s cast, range 12 circle
    _Weaponskill_DistantTune = 48576, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Burst = 48577, // 4CA2->self, 1.0s cast, range 9 circle
    _Weaponskill_InvitingVerse = 48571, // Boss->self, 5.0s cast, range 40 circle
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon02k1 = 218, // player->self
}

class SirenPieceStates : StateMachineBuilder
{
    public SirenPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14583)]
public class SirenPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

