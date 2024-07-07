namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D073SerpentD073Serpent;

public enum OID : uint
{
    Boss = 0x4164, // R4.500, x1
    Helper = 0x233C, // R0.500, x16, 523 type
    LesserSerpentOfTural = 0x41DE, // R2.812, x0 (spawn during fight)
    GreatSerpentOfTural = 0x41E0, // R1.152-3.840, x0 (spawn during fight)
    BlackenedStatue = 0x4254, // R0.500, x6
    Actor1ead8a = 0x1EAD8A, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1eba86 = 0x1EBA86, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba87 = 0x1EBA87, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba88 = 0x1EBA88, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    DubiousTulidisaster = 36748, // Boss->self, 5.0s cast, range 40 circle

    UnknownSpell = 36747, // Boss->location, no cast, single-target
    BouncyCouncil = 36746, // Boss->self, 3.0s cast, single-target// spawns clones

    MisplacedMystery = 36750, // LesserSerpentOfTural->self, 7.0s cast, range 52 width 5 rect
    ExaltedWobble = 36749, // LesserSerpentOfTural->self, 7.0s cast, range 9 circle

    ScreesOfFury1 = 36744, // Boss->self, 4.5+0.5s cast, single-target // AOE Tankbuster
    ScreesOfFury2 = 36757, // Helper->player, no cast, range 3 circle 

    GreatestLabyrinth = 36745, // Boss->self, 4.0s cast, range 40 circle
    MoistSummoning = 36743, // Boss->self, 3.0s cast, single-target

    MightyBlorp1 = 36753, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target
    MightyBlorp2 = 39983, // GreatSerpentOfTural->players, no cast, range 6 circle
    MightyBlorp3 = 36752, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target
    MightyBlorp4 = 39982, // GreatSerpentOfTural->players, no cast, range 5 circle
    MightyBlorp5 = 36751, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target
    MightyBlorp6 = 39981, // GreatSerpentOfTural->players, no cast, range 4 circle

    GreatestFlood1 = 36742, // Boss->self, 5.0s cast, single-target
    GreatestFlood2 = 36756, // Helper->self, 6.0s cast, range 40 circle

    GreatTorrent1 = 36741, // Boss->self, 3.0s cast, single-target
    GreatTorrent2 = 36754, // Helper->location, 6.0s cast, range 6 circle 
    GreatTorrent3 = 36755, // Helper->player, no cast, range 6 circle // spread
}

public enum SID : uint
{
    Bind = 2518, // Boss->player, extra=0x0
    GreatestCurse = 3825, // Boss->player, extra=0x0
    Poison = 2089, // none->player, extra=0x0
    Sludge1 = 3071, // none->player, extra=0x0
    Sludge2 = 3072, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon341 = 341, // player
    Icon504 = 504, // player
    Icon503 = 503, // player
    Stackmarker = 62, // player
    Icon542 = 542, // player
    Icon543 = 543, // player
    Spreadmarker = 139, // player
}

class DubiousTulidisaster(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DubiousTulidisaster));
class ExaltedWobble(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExaltedWobble), new AOEShapeCircle(9));
class MisplacedMystery(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MisplacedMystery), new AOEShapeRect(52, 2.5f));
class GreatTorrent2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.GreatTorrent2), 6);

class D073SerpentStates : StateMachineBuilder
{
    public D073SerpentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DubiousTulidisaster>()
            .ActivateOnEnter<ExaltedWobble>()
            .ActivateOnEnter<MisplacedMystery>()
            .ActivateOnEnter<GreatTorrent2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12709)]
public class D073Serpent(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 150), new ArenaBoundsSquare(12));
