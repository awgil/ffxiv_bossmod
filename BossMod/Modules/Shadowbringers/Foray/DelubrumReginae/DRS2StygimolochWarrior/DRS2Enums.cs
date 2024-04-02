namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

public enum OID : uint
{
    Boss = 0x30AB, // R4.000, x1
    Helper = 0x233C, // R0.500, x24, 523 type
    HiddenTrap = 0x18D6, // R0.500, x2, and more spawn during fight
    TrapNormal = 0x1EB0E3, // R0.500, EventObj type, spawn during fight when discovered by perception
    TrapToad = 0x1EB0E4, // R0.500, EventObj type, spawn during fight
    TrapIce = 0x1EB0E5, // R0.500, EventObj type, spawn during fight
    TrapMini = 0x1EB0E6, // R0.500, EventObj type, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 22396, // Boss->location, no cast, single-target
    SurgeOfVigor = 22422, // Boss->self, 3.0s cast, single-target, applies damage up
    UnrelentingCharge = 22425, // Boss->self, 3.0s cast, single-target, visual (knockbacks)
    UnrelentingChargeAOE = 22426, // Boss->player, no cast, single-target, charge + knockback 10

    Entrapment = 22397, // Boss->self, 3.0s cast, single-target, visual (spawn first traps)
    EntrapmentAttract = 22398, // Helper->self, no cast, range 60 circle, attract to southern point
    LethalBlow = 22399, // Boss->self, 20.0s cast, range 44 width 48 rect
    InescapableEntrapment = 22400, // Boss->self, 3.0s cast, single-target, visual (spawn second traps)
    SurgingFlames = 22401, // Boss->self, 13.0s cast, single-target, visual (resolve by freezing)
    SurgingFlamesAOE = 22402, // Helper->self, no cast, range 50 circle, deadly raidwide unless frozen
    SurgingFlood = 22403, // Boss->self, 10.0s cast, single-target, visual (resolve by toading)
    SurgingFloodAOE = 22404, // Helper->self, no cast, range 50 circle, attract into devour unless toad?
    WitheringCurse = 22405, // Boss->self, 13.0s cast, single-target, visual (resolve by minimizing)
    WitheringCurseAOE = 22406, // Helper->player, no cast, single-target, deadly raidwide unless minimized
    DevourVisual = 22407, // Boss->self, no cast, single-target, visual (attract)
    DevourAttract = 22408, // Helper->self, no cast, range 60 circle, attract 60 for devour
    Devour = 22409, // Boss->self, 2.0s cast, range 6 120-degree cone, deadly?
    LeapingSpark = 22410, // Boss->self, 8.0s cast, single-target, visual (resolve by untoading)
    LeapingSparkAOE = 22411, // Helper->player, no cast, range 5 circle
    MassiveExplosion = 22430, // HiddenTrap->location, no cast, range 3 circle, trap explosion
    TrappedUnderIce = 22431, // HiddenTrap->location, no cast, range 3 circle
    Toad = 22432, // HiddenTrap->location, no cast, range 3 circle
    Mini = 22433, // HiddenTrap->location, no cast, range 3 circle

    ViciousSwipe = 22423, // Boss->self, 4.0s cast, range 15 circle aoe
    CrazedRampage = 22424, // Helper->self, 6.0s cast, range 50 circle, knockback 13
    FocusedTremor = 22414, // Boss->self, 3.0s cast, single-target, visual (staggered aoes)
    FocusedTremorAOELarge = 22415, // Helper->self, 8.0s cast, range 20 width 20 rect aoe
    FocusedTremorExtra = 22416, // Helper->self, no cast, range 10 width 10 rect, ???
    FocusedTremorAOESmall = 22417, // Helper->self, 15.0s cast, range 10 width 10 rect aoe
    ForcefulStrike = 22418, // Boss->self, 15.0s cast, range 44 width 48 rect
    FlailingStrikeFirst = 22412, // Boss->self, 3.0s cast, range 40 60-degree cone
    FlailingStrikeRest = 22413, // Boss->self, 0.5s cast, range 40 ?-degree cone
    Coerce = 22420, // Boss->self, 3.0s cast, single-target, visual (apply forced march)

    SunsIre = 22394, // Boss->self, 12.0s cast, single-target, visual (enrage)
    SunsIreAOE = 22446, // Helper->self, no cast, range 50 circle, enrage
    SunsIreRepeat = 22395, // Boss->self, no cast, single-target, enrage repeat every 2s
};

public enum SID : uint
{
    ForwardMarch = 1293, // none->player, extra=0x0
    AboutFace = 1294, // none->player, extra=0x0
    LeftFace = 1295, // none->player, extra=0x0
    RightFace = 1296, // none->player, extra=0x0
    ForcedMarch = 1257, // none->player, extra=0x4/0x1/0x2/0x8
}

public enum IconID : uint
{
    FlailingStrike = 4, // player
};

public enum TetherID : uint
{
    FlailingStrike = 17, // Boss->player
};
