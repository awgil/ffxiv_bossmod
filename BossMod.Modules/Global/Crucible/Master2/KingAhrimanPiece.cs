#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.KingAhrimanPiece;

public enum OID : uint
{
    Boss = 0x4D18, // R2.800, x1
    _Gen_FinalHourglass = 0x4D19, // R1.000, x0 (spawn during fight)
    _Gen_PointyFinger = 0x4D1A, // R1.000, x0 (spawn during fight)
    _Gen_GrimReaperPiece = 0x4D1B, // R2.000, x0 (spawn during fight)
    _Gen_HapalitPiece = 0x4D1C, // R2.080, x0 (spawn during fight)
    _Gen_DirtyEyePiece = 0x4D1D, // R1.350, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x5, Helper type
}

public enum AID : uint
{
    _AutoAttack_ = 49682, // Boss->player, no cast, single-target
    _Weaponskill_Roulette = 49418, // Boss->self, 5.0s cast, single-target
    _Ability_ = 49419, // Helper->self, no cast, range 40 circle
    _Ability_1 = 49421, // 4D1A->self, no cast, single-target
    _Weaponskill_Death = 49422, // 4D1B->self, no cast, range 23 90-degree cone
    _Weaponskill_Stare = 50940, // Boss->players, no cast, range 50 width 8 rect
    _Ability_2 = 49423, // Boss->location, no cast, single-target
    _Ability_MercurialBrand = 49427, // Helper->player, no cast, single-target
    _Weaponskill_DoubleVision = 49424, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_IrefulGaze = 49426, // Helper->self, 6.0s cast, range 30 180-degree cone
    _Weaponskill_SullenGaze = 49425, // Helper->self, 6.0s cast, range 30 180-degree cone
    _Weaponskill_CurtainsForRank5 = 49428, // Boss->self, 5.0s cast, single-target
    _Weaponskill_CurtainsForRank51 = 49429, // Helper->self, 6.0s cast, range 50 circle
    _Weaponskill_ = 49420, // Helper->self, 25.0s cast, single-target
    _AutoAttack_1 = 50398, // 4D1C->player, no cast, single-target
    _Spell_Stone = 50747, // 4D1D->player, no cast, single-target
    _Weaponskill_StraightPunch = 49435, // 4D1C->player, no cast, single-target
    _Weaponskill_MortalGaze = 49434, // 4D1D->self, 4.0s cast, range 50 circle
    _Spell_Flare = 49431, // Boss->self, 6.0s cast, single-target
    _Spell_Flare1 = 49432, // Helper->players, no cast, range 43 circle
}

public enum SID : uint
{
    _Gen_Bind = 3625, // none->player, extra=0x0
    _Gen_Invincibility = 4539, // none->Boss, extra=0x0
    _Gen_BrandOfTheSullen = 5536, // Helper->player, extra=0x1
    _Gen_BrandOfTheIreful = 5537, // Helper->player, extra=0x1
}

public enum IconID : uint
{
    _Gen_Icon_tank_laser_5sec_lockon_c0a1 = 471, // player->self
    _Gen_Icon_m0742trg_b1t1 = 327, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_dark001f = 1, // 4D1D/4D1C->Boss
}

class FinalHourglass(BossModule module) : Components.Adds(module, (uint)OID._Gen_FinalHourglass, 1);

// TODO: hits the pet :(
class Stare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(50, 4), (uint)IconID._Gen_Icon_tank_laser_5sec_lockon_c0a1, AID._Weaponskill_Stare);

class DoubleVision(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_IrefulGaze, AID._Weaponskill_SullenGaze], new AOEShapeCone(30, 90.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            if (c.CastInfo?.IsSpell(AID._Weaponskill_IrefulGaze) == true && actor.FindStatus(SID._Gen_BrandOfTheIreful) != null || c.CastInfo?.IsSpell(AID._Weaponskill_SullenGaze) == true && actor.FindStatus(SID._Gen_BrandOfTheSullen) != null)
                yield return new(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        }
    }
}

class CurtainsForRank5(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_CurtainsForRank51)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count > 0 && WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { IsDead: false })
            hints.Add("Dismiss pet!");
    }
}

class Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID._Gen_Invincibility);
class Adds(BossModule module) : Components.AddsMulti(module, [OID._Gen_HapalitPiece, OID._Gen_DirtyEyePiece]);
class MortalGaze(BossModule module) : Components.CastGaze(module, AID._Weaponskill_MortalGaze);

class KingAhrimanPieceStates : StateMachineBuilder
{
    public KingAhrimanPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FinalHourglass>()
            .ActivateOnEnter<Stare>()
            .ActivateOnEnter<DoubleVision>()
            .ActivateOnEnter<CurtainsForRank5>()
            .ActivateOnEnter<Invincibility>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<MortalGaze>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14688)]
public class KingAhrimanPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

