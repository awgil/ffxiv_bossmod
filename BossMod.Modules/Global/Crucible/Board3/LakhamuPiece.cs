namespace BossMod.Global.Crucible.LakhamuPiece;

public enum OID : uint
{
    Boss = 0x4C9E, // R3.400, x1
    Helper = 0x233C, // R0.500, x9 (spawn during fight), Helper type
    _Gen_GolemPiece = 0x4C9F, // R2.200, x0 (spawn during fight)
    _Gen_SandSphere = 0x4CA0, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_Stone = 50792, // Boss->player, no cast, single-target
    _Ability_Landslip = 48553, // Boss->self, 7.5+1.0s cast, single-target
    _Ability_Landslip1 = 48556, // Helper->self, 8.0s cast, range 45 width 10 rect
    _Weaponskill_Rockslide = 48554, // 4C9F->self, 6.0s cast, single-target
    _Weaponskill_Rockslide1 = 48555, // Helper->self, 7.0s cast, range 45 width 10 rect
    _AutoAttack_ = 50398, // 4C9F->player, no cast, single-target
    _Spell_SandTempest = 48561, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_Burst = 48562, // 4CA0->self, 3.0s cast, range 12 circle
    _Spell_EarthShaker = 48557, // Boss->self, 4.0+0.2s cast, single-target
    _Spell_EarthShaker1 = 48558, // Helper->self, no cast, range 60 ?-degree cone
    _Spell_Earthrender = 48559, // Boss->self, 4.0s cast, single-target
    _Spell_Earthrender1 = 48560, // Helper->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_Blind = 5389, // Boss->player, extra=0x0
    _Gen_EarthResistanceDown = 5025, // Helper->player, extra=0x1
}

public enum IconID : uint
{
    _Gen_Icon_m0117_earth_shake_01s = 40, // player/49E8->self
}

class LakhamuPieceStates : StateMachineBuilder
{
    public LakhamuPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14580)]
public class LakhamuPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

