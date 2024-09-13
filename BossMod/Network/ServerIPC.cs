using System.Runtime.InteropServices;

namespace BossMod.Network.ServerIPC;

// taken from Machina, FFXIVPacketDissector, XIVAlexander, FFXIVOpcodes and custom research
// alternative names:
// StatusEffectListBozja: machina = StatusEffectList2
// StatusEffectListPlayer: machina = StatusEffectList3
// StatusEffectListDouble: machina = BossStatusEffectList
// ActionEffectN: machina = AbilityN
// SpawnObject: FFXIVOpcodes = ObjectSpawn
// SystemLogMessage1: FFXIVOpcodes = SomeDirectorUnk4
// WaymarkPreset: FFXIVOpcodes = PlaceFieldMarkerPreset, machina = PresetWaymark
// Waymark: FFXIVOpcodes = PlaceFieldMarker
// ActorCustomizeData: PlayerUpdateLook
// actor control examples: normal = toggle weapon, self = cooldown, target = target change
public enum PacketID
{
    Ping = 2,
    Init = 3,
    Logout = 8,
    CFCancel = 11,
    CFDutyInfo = 13,
    CFNotify = 14,
    CFPreferredRole = 18,
    CrossWorldLinkshellList = 81,
    FellowshipList = 89,
    Playtime = 111,
    CFRegistered = 112,
    Chat = 115,
    RSVData = 125,
    RSFData = 126,
    SocialMessage = 127,
    SocialMessage2 = 128,
    SocialList = 130,
    SocialRequestResponse = 131,
    ExamineSearchInfo = 132,
    UpdateSearchInfo = 133,
    InitSearchInfo = 134,
    ExamineSearchComment = 135,
    ServerNoticeShort = 138,
    ServerNotice = 139,
    SetOnlineStatus = 140,
    LogMessage = 141,
    Countdown = 145,
    CountdownCancel = 146,
    PartyMessage = 151,
    PlayerAddedToBlacklist = 153,
    PlayerRemovedFromBlacklist = 154,
    BlackList = 155,
    LinkshellList = 161,
    MailDeleteRequest = 162,
    MarketBoardItemListingCount = 166,
    MarketBoardItemListing = 168,
    MarketBoardPurchase = 170,
    MarketBoardItemListingHistory = 172,
    RetainerSaleHistory = 173,
    MarketBoardSearchResult = 175,
    FreeCompanyInfo = 177,
    ExamineFreeCompanyInfo = 179,
    FreeCompanyDialog = 180,
    StatusEffectList = 205,
    StatusEffectListEureka = 206,
    StatusEffectListBozja = 207,
    StatusEffectListDouble = 208,
    EffectResult1 = 210,
    EffectResult4 = 211,
    EffectResult8 = 212,
    EffectResult16 = 213,
    EffectResultBasic1 = 215,
    EffectResultBasic4 = 216,
    EffectResultBasic8 = 217,
    EffectResultBasic16 = 218,
    EffectResultBasic32 = 219,
    EffectResultBasic64 = 220,
    ActorControl = 221,
    ActorControlSelf = 222,
    ActorControlTarget = 223,
    UpdateHpMpTp = 224,
    ActionEffect1 = 225,
    ActionEffect8 = 228,
    ActionEffect16 = 229,
    ActionEffect24 = 230,
    ActionEffect32 = 231,
    StatusEffectListPlayer = 234,
    StatusEffectListPlayerDouble = 235,
    UpdateRecastTimes = 237,
    UpdateDutyRecastTimes = 238,
    UpdateAllianceNormal = 239,
    UpdateAllianceSmall = 240,
    UpdatePartyMemberPositions = 241,
    UpdateAllianceNormalMemberPositions = 242,
    UpdateAllianceSmallMemberPositions = 243,
    GCAffiliation = 246,
    SpawnPlayer = 264,
    SpawnNPC = 265,
    SpawnBoss = 266,
    DespawnCharacter = 267,
    ActorMove = 268,
    Transfer = 270,
    ActorSetPos = 271,
    ActorCast = 273,
    PlayerUpdateLook = 274,
    UpdateParty = 275,
    InitZone = 276,
    ApplyIDScramble = 277,
    UpdateHate = 278,
    UpdateHater = 279,
    SpawnObject = 280,
    DespawnObject = 281,
    UpdateClassInfo = 282,
    UpdateClassInfoEureka = 283,
    UpdateClassInfoBozja = 284,
    PlayerSetup = 285,
    PlayerStats = 286,
    FirstAttack = 287,
    PlayerStateFlags = 288,
    PlayerClassInfo = 289,
    PlayerBlueMageActions = 290,
    ModelEquip = 291,
    Examine = 292,
    CharaNameReq = 295,
    RetainerInformation = 299,
    ItemMarketBoardInfo = 301,
    ItemInfo = 303,
    ContainerInfo = 304,
    InventoryTransactionFinish = 305,
    InventoryTransaction = 306,
    CurrencyCrystalInfo = 307,
    InventoryActionAck = 309,
    UpdateInventorySlot = 310,
    OpenTreasure = 312,
    LootMessage = 315,
    CreateTreasure = 319,
    TreasureFadeOut = 320,
    HuntingLogEntry = 321,
    EventPlay = 323,
    EventPlay4 = 324,
    EventPlay8 = 325,
    EventPlay16 = 326,
    EventPlay32 = 327,
    EventPlay64 = 328,
    EventPlay128 = 329,
    EventPlay255 = 330,
    EventStart = 332,
    EventFinish = 333,
    EventContinue = 344,
    ResultDialog = 346,
    DesynthResult = 347,
    QuestActiveList = 352,
    QuestUpdate = 353,
    QuestCompleteList = 354,
    QuestFinish = 355,
    MSQTrackerComplete = 358,
    QuestTracker = 360,
    Mount = 361,
    DirectorVars = 363,
    ContentDirectorSync = 364,
    EnvControl = 396,
    SystemLogMessage1 = 402,
    SystemLogMessage2 = 403,
    SystemLogMessage4 = 404,
    SystemLogMessage8 = 405,
    SystemLogMessage16 = 406,
    BattleTalk2 = 408,
    BattleTalk4 = 409,
    BattleTalk8 = 410,
    MapUpdate = 412,
    MapUpdate4 = 413,
    MapUpdate8 = 414,
    MapUpdate16 = 415,
    MapUpdate32 = 416,
    MapUpdate64 = 417,
    MapUpdate128 = 418,
    BalloonTalk2 = 420,
    BalloonTalk4 = 421,
    BalloonTalk8 = 422,
    WeatherChange = 424,
    PlayerTitleList = 425,
    Discovery = 426,
    EorzeaTimeOffset = 428,
    EquipDisplayFlags = 441,
    NpcYell = 442,
    FateInfo = 447,
    CompletedAchievements = 452,
    LandSetInitialize = 461,
    LandUpdate = 462,
    YardObjectSpawn = 463,
    HousingIndoorInitialize = 464,
    LandAvailability = 465,
    LandPriceUpdate = 467,
    LandInfoSign = 468,
    LandRename = 469,
    HousingEstateGreeting = 470,
    HousingUpdateLandFlagsSlot = 471,
    HousingLandFlags = 472,
    HousingShowEstateGuestAccess = 473,
    HousingObjectInitialize = 475,
    HousingInternalObjectSpawn = 476,
    HousingWardInfo = 478,
    HousingObjectMove = 479,
    HousingObjectDye = 480,
    SharedEstateSettingsResponse = 492,
    DailyQuests = 504,
    DailyQuestRepeatFlags = 506,
    LandUpdateHouseName = 508,
    AirshipTimers = 519,
    PlaceMarker = 527,
    WaymarkPreset = 528,
    Waymark = 529,
    UnMount = 532,
    CeremonySetActorAppearance = 535,
    AirshipStatusList = 541,
    AirshipStatus = 542,
    AirshipExplorationResult = 543,
    SubmarineStatusList = 544,
    SubmarineProgressionStatus = 545,
    SubmarineExplorationResult = 546,
    SubmarineTimers = 548,
    PrepareZoning = 578,
    ActorGauge = 579,
    CharaVisualEffect = 580,
    LandSetMap = 581,
    Fall = 582,
    PlayMotionSync = 631,
    CEDirector = 640,
    IslandWorkshopDemandResearch = 659,
    IslandWorkshopSupplyDemand = 662,
    IslandWorkshopFavors = 678,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IPCHeader
{
    public ushort Magic; // 0x0014
    public ushort MessageType;
    public uint Unknown1;
    public uint Epoch;
    public uint Unknown2;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct RSVData
{
    public int ValueLength;
    public fixed byte Key[48];
    public fixed byte Value[1]; // variable-length
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Countdown
{
    public uint SenderID;
    public ushort u4;
    public ushort Time;
    public byte FailedInCombat;
    public byte u9;
    public byte u10;
    public fixed byte Text[37];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CountdownCancel
{
    public uint SenderID;
    public ushort u4;
    public ushort u6;
    public fixed byte Text[32];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Status
{
    public ushort ID;
    public ushort Extra;
    public float RemainingTime;
    public uint SourceID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct StatusEffectList
{
    public Class ClassID;
    public byte Level;
    public byte u2;
    public byte u3; // != 0 => set alliance member flag 8
    public int CurHP;
    public int MaxHP;
    public ushort CurMP;
    public ushort MaxMP;
    public ushort ShieldValue;
    public ushort u12;
    public fixed byte Statuses[30 * 12]; // Status[30]
    public uint u17C;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StatusEffectListEureka
{
    public byte Rank;
    public byte Element;
    public byte u2;
    public byte pad3;
    public StatusEffectList Data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StatusEffectListBozja
{
    public byte Rank;
    public byte pad1;
    public ushort pad2;
    public StatusEffectList Data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct StatusEffectListDouble
{
    public fixed byte SecondSet[30 * 12]; // Status[30]
    public StatusEffectList Data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EffectResultEffect
{
    public byte EffectIndex;
    public byte pad1;
    public ushort StatusID;
    public ushort Extra;
    public ushort pad2;
    public float Duration;
    public uint SourceID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct EffectResultEntry
{
    public uint RelatedActionSequence;
    public uint ActorID;
    public uint CurHP;
    public uint MaxHP;
    public ushort CurMP;
    public byte RelatedTargetIndex;
    public Class ClassID;
    public byte ShieldValue;
    public byte EffectCount;
    public ushort u16;
    public fixed byte Effects[4 * 16]; // EffectResultEffect[4]
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct EffectResultN
{
    public byte NumEntries;
    public byte pad1;
    public ushort pad2;
    public fixed byte Entries[1 * 0x58]; // N=1/4/8/16
    // followed by 1 dword padding
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct EffectResultBasicEntry
{
    public uint RelatedActionSequence;
    public uint ActorID;
    public uint CurHP;
    public byte RelatedTargetIndex;
    public byte uD;
    public ushort uE;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct EffectResultBasicN
{
    public byte NumEntries;
    public byte pad1;
    public ushort pad2;
    public fixed byte Entries[1 * 16]; // N=1/4/8/16/32/64
    // followed by 1 dword padding
}

public enum ActorControlCategory : ushort
{
    ToggleWeapon = 0, // from dissector
    AutoAttack = 1, // from dissector
    SetStatus = 2, // from dissector
    CastStart = 3, // from dissector
    ToggleAggro = 4, // from dissector
    ClassJobChange = 5, // from dissector
    Death = 6, // dissector calls it DefeatMsg
    GainExpMsg = 7, // from dissector
    LevelUpEffect = 10, // from dissector
    ExpChainMsg = 12, // from dissector
    HpSetStat = 13, // from dissector
    DeathAnimation = 14, // from dissector
    CancelCast = 15, // dissector calls it CastInterrupt (ActorControl), machina calls it CancelAbility
    RecastDetails = 16, // p1=group id, p2=elapsed, p3=total
    Cooldown = 17, // dissector calls it ActionStart (ActorControlSelf)
    GainEffect = 20, // note: this packet only causes log message and hit vfx to appear, it does not actually update statuses
    LoseEffect = 21,
    UpdateEffect = 22,
    HotDot = 23, // dissector calls it HPFloatingText
    UpdateRestedExp = 24, // from dissector
    Flee = 27, // from dissector
    UnkVisControl = 30, // visibility control ??? (ActorControl, params=delay-after-spawn, visible, id, 0)
    TargetIcon = 34, // dissector calls it CombatIndicationShow, this is for boss-related markers, param1 = marker id, param2=param3=param4=0
    Tether = 35,
    SpawnEffect = 37, // from dissector
    ToggleInvisible = 38, // from dissector
    ToggleActionUnlock = 41, // from dissector
    UpdateUiExp = 43, // from dissector
    DmgTakenMsg = 45, // from dissector
    TetherCancel = 47,
    SetTarget = 50, // from dissector
    Targetable = 54, // dissector calls it ToggleNameHidden
    SetAnimationState = 62, // example - ASSN beacon activation; param1 = animation set index (0 or 1), param2 = animation index (0-7)
    SetModelState = 63, // example - TEA liquid hand (open/closed); param1=ModelState row index, rest unused
    LimitBreakStart = 71, // from dissector
    LimitBreakPartyStart = 72, // from dissector
    BubbleText = 73, // from dissector
    DamageEffect = 80, // from dissector
    RaiseAnimation = 81, // from dissector
    TreasureScreenMsg = 87, // from dissector
    SetOwnerId = 89, // from dissector
    ItemRepairMsg = 92, // from dissector
    SetName = 98,
    BluActionLearn = 99, // from dissector
    DirectorInit = 100, // from dissector
    DirectorClear = 101, // from dissector
    LeveStartAnim = 102, // from dissector
    LeveStartError = 103, // from dissector
    DirectorEObjMod = 106, // from dissector
    DirectorUpdate = 109,
    ItemObtainMsg = 117, // from dissector
    DutyQuestScreenMsg = 123, // from dissector
    FatePosition = 125, // from dissector
    ItemObtainIcon = 132, // from dissector
    FateItemFailMsg = 133, // from dissector
    FateFailMsg = 134, // from dissector
    ActionLearnMsg1 = 135, // from dissector
    FreeEventPos = 138, // from dissector
    FateSync = 139, // from dissector
    DailyQuestSeed = 144, // from dissector
    SetBGM = 161, // from dissector
    UnlockAetherCurrentMsg = 164, // from dissector
    RemoveName = 168, // from dissector
    ScreenFadeOut = 170, // from dissector
    ZoneIn = 200, // from dissector
    ZoneInDefaultPos = 201, // from dissector
    TeleportStart = 203, // from dissector
    TeleportDone = 205, // from dissector
    TeleportDoneFadeOut = 206, // from dissector
    DespawnZoneScreenMsg = 207, // from dissector
    InstanceSelectDlg = 210, // from dissector
    ActorDespawnEffect = 212, // from dissector
    ForcedMovement = 226,
    CompanionUnlock = 253, // from dissector
    ObtainBarding = 254, // from dissector
    EquipBarding = 255, // from dissector
    CompanionMsg1 = 258, // from dissector
    CompanionMsg2 = 259, // from dissector
    ShowPetHotbar = 260, // from dissector
    ActionLearnMsg = 265, // from dissector
    ActorFadeOut = 266, // from dissector
    ActorFadeIn = 267, // from dissector
    WithdrawMsg = 268, // from dissector
    OrderCompanion = 269, // from dissector
    ToggleCompanion = 270, // from dissector
    LearnCompanion = 271, // from dissector
    ActorFateOut1 = 272, // from dissector
    Emote = 290, // from dissector
    EmoteInterrupt = 291, // from dissector
    SetPose = 295, // from dissector
    FishingLightChange = 300, // from dissector
    GatheringSenseMsg = 304, // from dissector
    PartyMsg = 305, // from dissector
    GatheringSenseMsg1 = 306, // from dissector
    GatheringSenseMsg2 = 312, // from dissector
    FishingMsg = 320, // from dissector
    FishingTotalFishCaught = 322, // from dissector
    FishingBaitMsg = 325, // from dissector
    FishingReachMsg = 327, // from dissector
    FishingFailMsg = 328, // from dissector
    WeeklyIntervalUpdateTime = 336, // from dissector
    MateriaConvertMsg = 350, // from dissector
    MeldSuccessMsg = 351, // from dissector
    MeldFailMsg = 352, // from dissector
    MeldModeToggle = 353, // from dissector
    AetherRestoreMsg = 355, // from dissector
    DyeMsg = 360, // from dissector
    ToggleCrestMsg = 362, // from dissector
    ToggleBulkCrestMsg = 363, // from dissector
    MateriaRemoveMsg = 364, // from dissector
    GlamourCastMsg = 365, // from dissector
    GlamourRemoveMsg = 366, // from dissector
    RelicInfuseMsg = 377, // from dissector
    PlayerCurrency = 378, // from dissector
    AetherReductionDlg = 381, // from dissector
    PlayActionTimeline = 407, // seems to be equivalent to 412?..
    EObjSetState = 409, // from dissector
    Unk6 = 412, // from dissector
    EObjAnimation = 413, // from dissector
    SetCompanionOwnerId = 417,
    SetTitle = 500, // from dissector
    SetTargetSign = 502,
    SetStatusIcon = 504, // from dissector
    LimitBreakGauge = 505, // name from dissector
    SetHomepoint = 507, // from dissector
    SetFavorite = 508, // from dissector
    LearnTeleport = 509, // from dissector
    OpenRecommendationGuide = 512, // from dissector
    ArmoryErrorMsg = 513, // from dissector
    AchievementProgress = 514,
    AchievementPopup = 515, // from dissector
    LogMsg = 517, // from dissector
    AchievementMsg = 518, // from dissector
    SetCutsceneFlags = 519,
    SetItemLevel = 521, // from dissector
    ChallengeEntryCompleteMsg = 523, // from dissector
    ChallengeEntryUnlockMsg = 524, // from dissector
    DesynthOrReductionResult = 527, // from dissector
    GilTrailMsg = 529, // from dissector
    HuntingLogRankUnlock = 541, // from dissector
    HuntingLogEntryUpdate = 542, // from dissector
    HuntingLogSectionFinish = 543, // from dissector
    HuntingLogRankFinish = 544, // from dissector
    SetMaxGearSets = 560, // from dissector
    SetCharaGearParamUI = 608, // from dissector
    ToggleWireframeRendering = 609, // from dissector
    ActionRejected = 700, // from XivAlexander (ActorControlSelf)
    ExamineError = 703, // from dissector
    GearSetEquipMsg = 801, // from dissector
    SetFestival = 902, // from dissector
    ToggleOrchestrionUnlock = 918, // from dissector
    SetMountSpeed = 927, // from dissector
    Dismount = 929, // from dissector
    BeginReplayAck = 930, // from dissector
    EndReplayAck = 931, // from dissector
    ShowBuildPresetUI = 1001, // from dissector
    ShowEstateExternalAppearanceUI = 1002, // from dissector
    ShowEstateInternalAppearanceUI = 1003, // from dissector
    BuildPresetResponse = 1005, // from dissector
    RemoveExteriorHousingItem = 1007, // from dissector
    RemoveInteriorHousingItem = 1009, // from dissector
    ShowHousingItemUI = 1015, // from dissector
    HousingItemMoveConfirm = 1017, // from dissector
    OpenEstateSettingsUI = 1023, // from dissector
    HideAdditionalChambersDoor = 1024, // from dissector
    HousingStoreroomStatus = 1049, // from dissector
    TripleTriadCard = 1204, // from dissector
    TripleTriadUnknown = 1205, // from dissector
    FateNpc = 2351, // from dissector
    FateInit = 2353, // from dissector
    FateAssignID = 2356, // p1 = fate id, assigned to main obj
    FateStart = 2357, // from dissector
    FateEnd = 2358, // from dissector
    FateProgress = 2366, // from dissector
    SetPvPState = 1504, // from dissector
    EndDuelSession = 1505, // from dissector
    StartDuelCountdown = 1506, // from dissector
    StartDuel = 1507, // from dissector
    DuelResultScreen = 1508, // from dissector
    SetDutyActionSet = 1512,
    SetDutyActionDetails = 1513,
    SetDutyActionPresent = 1514,
    SetDutyActionActive = 1515,
    SetDutyActionCharges = 1516,
    IncrementRecast = 1536, // p1=cooldown group, p2=delta time quantized to 100ms; example is brd mage ballad proc
    EurekaStep = 1850, // from dissector
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorControl
{
    public ActorControlCategory category;
    public ushort padding0;
    public uint param1;
    public uint param2;
    public uint param3;
    public uint param4;
    public uint padding1;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorControlSelf
{
    public ActorControlCategory category;
    public ushort padding0;
    public uint param1;
    public uint param2;
    public uint param3;
    public uint param4;
    public uint param5;
    public uint param6;
    public uint padding1;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorControlTarget
{
    public ActorControlCategory category;
    public ushort padding0;
    public uint param1;
    public uint param2;
    public uint param3;
    public uint param4;
    public uint padding1;
    public ulong TargetID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UpdateHpMpTp
{
    public uint HP;
    public ushort MP;
    public ushort GP;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActionEffect
{
    public ActionEffectType Type;
    public byte Param0;
    public byte Param1;
    public byte Param2;
    public byte Param3;
    public byte Param4;
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffectHeader
{
    public ulong animationTargetId;  // who the animation targets
    public uint actionId; // what the casting player casts, shown in battle log / ui
    public uint globalEffectCounter;
    public float animationLockTime;
    public uint BallistaEntityId; // for 'artillery' actions - entity id of ballista source
    public ushort SourceSequence; // 0 = initiated by server, otherwise corresponds to client request sequence id
    public ushort rotation;
    public ushort actionAnimationId;
    public byte variation; // animation
    public ActionType actionType;
    public byte Flags;
    public byte NumTargets; // machina calls it 'effectCount', but it is misleading imo
    public ushort padding21;
    public ushort padding22;
    public ushort padding23;
    public ushort padding24;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffect1
{
    public ActionEffectHeader Header;
    public fixed ulong Effects[8]; // ActionEffect[8]
    public ushort padding3;
    public uint padding4;
    public fixed ulong TargetID[1];
    public uint padding5;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffect8
{
    public ActionEffectHeader Header;
    public fixed ulong Effects[8 * 8]; // ActionEffect[8 * 8]
    public ushort padding3;
    public uint padding4;
    public fixed ulong TargetID[8];
    public ushort TargetX; // floatCoord = ((intCoord * 3.0518043) * 0.0099999998) - 1000.0 (0 => -1000, 65535 => +1000)
    public ushort TargetY;
    public ushort TargetZ;
    public ushort padding5;
    public uint padding6;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffect16
{
    public ActionEffectHeader Header;
    public fixed ulong Effects[8 * 16]; // ActionEffect[8 * 16]
    public ushort padding3;
    public uint padding4;
    public fixed ulong TargetID[16];
    public ushort TargetX;
    public ushort TargetY;
    public ushort TargetZ;
    public ushort padding5;
    public uint padding6;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffect24
{
    public ActionEffectHeader Header;
    public fixed ulong Effects[8 * 24]; // ActionEffect[8 * 24]
    public ushort padding3;
    public uint padding4;
    public fixed ulong TargetID[24];
    public ushort TargetX;
    public ushort TargetY;
    public ushort TargetZ;
    public ushort padding5;
    public uint padding6;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ActionEffect32
{
    public ActionEffectHeader Header;
    public fixed ulong Effects[8 * 32]; // ActionEffect[8 * 32]
    public ushort padding3;
    public uint padding4;
    public fixed ulong TargetID[32];
    public ushort TargetX;
    public ushort TargetY;
    public ushort TargetZ;
    public ushort padding5;
    public uint padding6;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct StatusEffectListPlayer
{
    public fixed byte Statuses[30 * 12]; // Status[30]
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct UpdateRecastTimes
{
    public fixed float Elapsed[80];
    public fixed float Total[80];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct UpdateDutyRecastTimes
{
    public fixed float Elapsed[2];
    public fixed float Total[2];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorMove
{
    public ushort Rotation;
    public ushort AnimationFlags;
    public byte AnimationSpeed;
    public byte UnknownRotation;
    public ushort X;
    public ushort Y;
    public ushort Z;
    public uint Unknown;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorSetPos
{
    public ushort Rotation;
    public byte u2;
    public byte u3;
    public uint u4;
    public float X;
    public float Y;
    public float Z;
    public uint u14;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorCast
{
    public ushort SpellID;
    public ActionType ActionType;
    public byte BaseCastTime100ms;
    public uint ActionID; // also action ID; dissector calls it ItemId - matches actionId of ActionEffectHeader - e.g. when using KeyItem, action is generic 'KeyItem 1', Unknown1 is actual item id, probably similar for stuff like mounts etc.
    public float CastTime;
    public uint TargetID;
    public ushort Rotation;
    public byte Interruptible;
    public byte u1;
    public uint BallistaEntityId; // for 'artillery' actions - entity id of ballista source
    public ushort PosX;
    public ushort PosY;
    public ushort PosZ;
    public ushort u3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UpdateHateEntry
{
    public uint ObjectID;
    public byte Enmity;
    public byte pad5;
    public ushort pad6;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct UpdateHate
{
    public byte NumEntries;
    public byte pad1;
    public ushort pad2;
    public fixed ulong Entries[8]; // UpdateHateEntry[8]
    public uint pad3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct UpdateHater
{
    public byte NumEntries;
    public byte pad1;
    public ushort pad2;
    public fixed ulong Entries[32]; // UpdateHateEntry[32]
    public uint pad3;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct SpawnObject
{
    public byte Index;
    public byte Kind;
    public byte u2_state;
    public byte u3;
    public uint DataID;
    public uint InstanceID;
    public uint u_levelID;
    public uint DutyID;
    public uint OwnerID;
    public uint u_gimmickID;
    public float Scale;
    public ushort u20;
    public ushort Rotation;
    public ushort FateID;
    public ushort EventState; // for common gameobject field
    public uint EventObjectState; // for eventobject-specific field
    public uint u_modelID;
    public Vector3 Position;
    public ushort u3C;
    public ushort u3E;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct UpdateClassInfo
{
    public Class ClassID;
    public byte pad1;
    public ushort CurLevel;
    public ushort ClassLevel;
    public ushort SyncedLevel;
    public uint CurExp;
    public uint RestedExp;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UpdateClassInfoEureka
{
    public byte Rank;
    public byte Element;
    public byte u2;
    public byte pad3;
    public UpdateClassInfo Data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UpdateClassInfoBozja
{
    public byte Rank;
    public byte pad1;
    public ushort pad2;
    public UpdateClassInfo Data;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct EventPlayN
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PayloadCrafting // for EventHandler == 0x000A0001
    {
        public enum OperationId
        {
            StartPrepare = 1,
            StartInfo = 2,
            StartReady = 3,
            Finish = 4,
            Abort = 6,
            ReturnedReagents = 8,
            AdvanceCraftAction = 9,
            AdvanceNormalAction = 10,
            QuickSynthStart = 12,
            QuickSynthProgress = 13,
        }

        [Flags]
        public enum StepFlags : uint
        {
            u1 = 0x00000002, // always set?
            CompleteSuccess = 0x00000004, // set even if craft fails due to durability
            CompleteFail = 0x00000008,
            LastActionSucceeded = 0x00000010,
            ComboBasicTouch = 0x08000000,
            ComboStandardTouch = 0x10000000,
            ComboObserve = 0x20000000,
            NoCarefulsLeft = 0x40000000,
            NoHSLeft = 0x80000000,
        }

        [StructLayout(LayoutKind.Explicit, Size = 12)]
        public struct StartInfo // op id == StartInfo
        {
            [FieldOffset(0)] public ushort RecipeId;
            [FieldOffset(4)] public int StartingQuality;
            [FieldOffset(8)] public byte u8;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ReturnedReagents // op id == ReturnedReagents
        {
            public int u0;
            public int u4;
            public int u8;
            public fixed uint ItemIds[8];
            public fixed int NumNQ[8];
            public fixed int NumHQ[8];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AdvanceStep // op id == Advance*Action
        {
            public int u0;
            public int u4;
            public int u8;
            public int LastActionId;
            public int DeltaCP;
            public int StepIndex;
            public int CurProgress;
            public int DeltaProgress;
            public int CurQuality;
            public int DeltaQuality;
            public int HQChance;
            public int CurDurability;
            public int DeltaDurability;
            public int Condition; // 1 = normal, ...
            public int u38; // usually 1, sometimes 2? related to quality
            public int ConditionParam; // used for good, related to splendorous?
            public StepFlags Flags;
            public int u44;
            public fixed int RemoveStatusIds[7];
            public fixed int RemoveStatusParams[7];
        }

        [StructLayout(LayoutKind.Explicit, Size = 12)]
        public struct QuickSynthStart // op id == QuickSynthStart
        {
            [FieldOffset(0)] public ushort RecipeId;
            [FieldOffset(4)] public byte MaxCount;
        }

        [FieldOffset(0)] public OperationId OpId;
        [FieldOffset(4)] public StartInfo OpStartInfo;
        [FieldOffset(4)] public ReturnedReagents OpReturnedReagents;
        [FieldOffset(4)] public AdvanceStep OpAdvanceStep;
        [FieldOffset(4)] public QuickSynthStart OpQuickSynthStart;
    }

    public ulong TargetID;
    public uint EventHandler;
    public ushort uC;
    public ushort pad1;
    public ulong u10;
    public byte PayloadLength; // in dwords
    public byte pad2;
    public ushort pad3;
    public fixed uint Payload[1]; // N = 1/4/8/16/32/64/128/255
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvControl
{
    public uint DirectorID;
    public ushort State1; // typically has 1 bit set
    public ushort State2; // typically has 1 bit set
    public byte Index;
    public byte pad9;
    public ushort padA;
    public uint padC;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NpcYell
{
    public ulong SourceID;
    public int u8;
    public ushort Message;
    public ushort uE;
    public ulong u10;
    public ulong u18;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct WaymarkPreset
{
    public byte Mask;
    public byte pad1;
    public ushort pad2;
    public fixed int PosX[8];// Xints[0] has X of waymark A, Xints[1] X of B, etc.
    public fixed int PosY[8];// To calculate 'float' coords from these you cast them to float and then divide by 1000.0
    public fixed int PosZ[8];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Waymark
{
    public BossMod.Waymark ID;
    public byte Active; // 0=off, 1=on
    public ushort pad2;
    public int PosX;
    public int PosY;// To calculate 'float' coords from these you cast them to float and then divide by 1000.0
    public int PosZ;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActorGauge
{
    public Class ClassJobID;
    public ulong Payload;
}
