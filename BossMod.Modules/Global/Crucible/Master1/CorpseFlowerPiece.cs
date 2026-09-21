#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.CorpseFlowerPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x3, Helper type
    Boss = 0x4CBD, // R3.200, x1
    _Gen_SaplingPiece = 0x4CBE, // R0.750, x0 (spawn during fight)
    _Gen_QueenHawkPiece = 0x4CBF, // R0.720, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 49681, // Boss->player, no cast, single-target
    _Weaponskill_BuddingThorns = 48687, // Boss->self, 3.0s cast, single-target
    _Weaponskill_FloralTrap = 48683, // Boss->self, 5.0s cast, range 80 circle
    _Weaponskill_FloralTrap1 = 48684, // Boss->self, no cast, range 45 ?-degree cone
    _Weaponskill_Devour = 48685, // Boss->self, no cast, range 8 ?-degree cone
    _Weaponskill_Spit = 48686, // Boss->self, 4.0s cast, range 0 ???
    _Weaponskill_RottenStench = 48690, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_RottenStench1 = 48691, // Boss->self, no cast, range 45 width 12 rect
    _AutoAttack_1 = 48688, // 4CBF->player, no cast, single-target
    _Weaponskill_AcidRain = 48692, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AcidRain1 = 48693, // Helper->location, 2.0s cast, range 6 circle
    _Weaponskill_AcidRain2 = 48694, // Helper->location, no cast, range 6 circle
    _Weaponskill_FinalSting = 48689, // 4CBF->player, 5.0s cast, single-target
}

public enum SID : uint
{
    _Gen_Bind = 2518, // Boss->player, extra=0x0
    _Gen_Stun = 2656, // Boss->player/4CBF, extra=0x0
    _Gen_DamageUp = 2550, // none->Boss, extra=0x1/0x2/0x3
    _Gen_Devoured = 421, // Boss->player, extra=0x0
    _Gen_Briar = 5176, // none->player, extra=0x32
}

public enum IconID : uint
{
    _Gen_Icon_m0005_loc_x2 = 703, // player->self
    _Gen_Icon_share_laser_5sec_0t = 525, // Boss->player
    _Gen_Icon_tracking_lockon01i = 197, // player->self
}

class CorpseFlowerPieceStates : StateMachineBuilder
{
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// sapling puddle spawns with radius 5, starts growing during EObjAnim 00100020 to maximum radius of 10, then despawns with EObjState 0004
// spit sends the player 10 units
[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14603)]
public class CorpseFlowerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(PrimaryActor) is { } p)
            p.DesiredPosition = Arena.Center;
    }
}

