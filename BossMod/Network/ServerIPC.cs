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
    RSVData = 127,
    RSFData = 128,
    SocialMessage = 129,
    SocialMessage2 = 130,
    SocialList = 132,
    SocialRequestResponse = 133,
    ExamineSearchInfo = 134,
    UpdateSearchInfo = 135,
    InitSearchInfo = 136,
    ExamineSearchComment = 137,
    ServerNoticeShort = 140,
    ServerNotice = 141,
    SetOnlineStatus = 142,
    LogMessage = 143,
    Countdown = 147,
    CountdownCancel = 148,
    PartyMessage = 153,
    PlayerAddedToBlacklist = 155,
    PlayerRemovedFromBlacklist = 156,
    BlackList = 157,
    LinkshellList = 163,
    MailDeleteRequest = 164,
    MarketBoardItemListingCount = 169,
    MarketBoardItemListing = 170,
    PlayerRetainerInfo = 171,
    MarketBoardPurchase = 172,
    MarketBoardSale = 173,
    MarketBoardItemListingHistory = 174,
    RetainerSaleHistory = 175,
    RetainerState = 176,
    MarketBoardSearchResult = 177,
    FreeCompanyInfo = 179,
    ExamineFreeCompanyInfo = 181,
    FreeCompanyDialog = 182,
    StatusEffectList = 207,
    StatusEffectListEureka = 208,
    StatusEffectListBozja = 209,
    StatusEffectListForay3 = 210,
    StatusEffectListDouble = 211,
    EffectResult1 = 213,
    EffectResult4 = 214,
    EffectResult8 = 215,
    EffectResult16 = 216,
    EffectResultBasic1 = 218,
    EffectResultBasic4 = 219,
    EffectResultBasic8 = 220,
    EffectResultBasic16 = 221,
    EffectResultBasic32 = 222,
    EffectResultBasic64 = 223,
    ActorControl = 224,
    ActorControlSelf = 225,
    ActorControlTarget = 226,
    UpdateHpMpTp = 227,
    ActionEffect1 = 228,
    ActionEffect8 = 231,
    ActionEffect16 = 232,
    ActionEffect24 = 233,
    ActionEffect32 = 234,
    StatusEffectListPlayer = 237,
    StatusEffectListPlayerDouble = 238,
    UpdateRecastTimes = 240,
    UpdateDutyRecastTimes = 241,
    UpdateDutyRecastTimes5 = 242,
    UpdateAllianceNormal = 243,
    UpdateAllianceSmall = 244,
    UpdatePartyMemberPositions = 245,
    UpdateAllianceNormalMemberPositions = 246,
    UpdateAllianceSmallMemberPositions = 247,
    GCAffiliation = 250,
    SpawnPlayer = 268,
    SpawnNPC = 269,
    SpawnBoss = 270,
    DespawnCharacter = 271,
    ActorMove = 272,
    Transfer = 274,
    ActorSetPos = 275,
    ActorCast = 277,
    PlayerUpdateLook = 278,
    UpdateParty = 279,
    InitZone = 280,
    ApplyIDScramble = 281,
    UpdateHate = 282,
    UpdateHater = 283,
    SpawnObject = 284,
    DespawnObject = 285,
    UpdateClassInfo = 286,
    UpdateClassInfoEureka = 287,
    UpdateClassInfoBozja = 288,
    UpdateClassInfoForay3 = 289,
    PlayerSetup = 290,
    PlayerStats = 291,
    FirstAttack = 292,
    PlayerStateFlags = 293,
    PlayerClassInfo = 294,
    PlayerBlueMageActions = 295,
    ModelEquip = 296,
    Examine = 297,
    CharaNameReq = 300,
    RetainerSummary = 304,
    RetainerInformation = 305,
    ItemMarketBoardSummary = 306,
    ItemMarketBoardInfo = 307,
    ItemInfo = 309,
    ContainerInfo = 310,
    InventoryTransactionFinish = 311,
    InventoryTransaction = 312,
    CurrencyCrystalInfo = 313,
    InventoryActionAck = 315,
    UpdateInventorySlot = 316,
    OpenTreasure = 318,
    LootMessage = 321,
    CreateTreasure = 325,
    TreasureFadeOut = 326,
    HuntingLogEntry = 327,
    EventPlay = 329,
    EventPlay4 = 330,
    EventPlay8 = 331,
    EventPlay16 = 332,
    EventPlay32 = 333,
    EventPlay64 = 334,
    EventPlay128 = 335,
    EventPlay255 = 336,
    EventStart = 338,
    EventFinish = 339,
    EventContinue = 350,
    ResultDialog = 352,
    DesynthResult = 353,
    QuestActiveList = 358,
    QuestUpdate = 359,
    QuestCompleteList = 360,
    QuestFinish = 361,
    MSQTrackerComplete = 364,
    QuestTracker = 366,
    Mount = 367,
    DirectorVars = 369,
    ContentDirectorSync = 370,
    ServerRequestCallbackResponse1 = 378,
    ServerRequestCallbackResponse2 = 379,
    ServerRequestCallbackResponse3 = 380,
    EnvControl = 402,
    EnvControl4 = 403,
    EnvControl8 = 404,
    EnvControl12 = 405,
    SystemLogMessage1 = 408,
    SystemLogMessage2 = 409,
    SystemLogMessage4 = 410,
    SystemLogMessage8 = 411,
    SystemLogMessage16 = 412,
    BattleTalk2 = 414,
    BattleTalk4 = 415,
    BattleTalk8 = 416,
    MapUpdate = 418,
    MapUpdate4 = 419,
    MapUpdate8 = 420,
    MapUpdate16 = 421,
    MapUpdate32 = 422,
    MapUpdate64 = 423,
    MapUpdate128 = 424,
    BalloonTalk2 = 426,
    BalloonTalk4 = 427,
    BalloonTalk8 = 428,
    WeatherChange = 430,
    PlayerTitleList = 431,
    Discovery = 432,
    EorzeaTimeOffset = 434,
    EquipDisplayFlags = 447,
    NpcYell = 448,
    FateInfo = 453,
    CompletedAchievements = 458,
    LandSetInitialize = 467,
    LandUpdate = 468,
    YardObjectSpawn = 469,
    HousingIndoorInitialize = 470,
    LandAvailability = 471,
    LandPriceUpdate = 473,
    LandInfoSign = 474,
    LandRename = 475,
    HousingEstateGreeting = 476,
    HousingUpdateLandFlagsSlot = 477,
    HousingLandFlags = 478,
    HousingShowEstateGuestAccess = 479,
    HousingObjectInitialize = 481,
    HousingInternalObjectSpawn = 482,
    HousingWardInfo = 484,
    HousingObjectMove = 485,
    HousingObjectDye = 486,
    SharedEstateSettingsResponse = 498,
    DailyQuests = 510,
    DailyQuestRepeatFlags = 512,
    LandUpdateHouseName = 514,
    AirshipTimers = 525,
    PlaceMarker = 533,
    WaymarkPreset = 534,
    Waymark = 535,
    UnMount = 538,
    CeremonySetActorAppearance = 541,
    AirshipStatusList = 547,
    AirshipStatus = 548,
    AirshipExplorationResult = 549,
    SubmarineStatusList = 550,
    SubmarineProgressionStatus = 551,
    SubmarineExplorationResult = 552,
    SubmarineTimers = 554,
    PrepareZoning = 584,
    ActorGauge = 585,
    CharaVisualEffect = 586,
    LandSetMap = 587,
    Fall = 588,
    PlayMotionSync = 637,
    CEDirector = 646,
    IslandWorkshopDemandResearch = 665,
    IslandWorkshopSupplyDemand = 668,
    IslandWorkshopFavors = 684,
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
public unsafe struct MarketBoardItemListingCount
{
    public uint Error;
    public byte NumItems;
    public fixed byte Padding[3];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MarketBoardItemListingEntry
{
    public ulong ListingId;
    public ulong SellingRetainerContentId;
    public ulong SellingPlayerContentId;
    public ulong ArtisanId;
    public uint UnitPrice;
    public uint TotalTax;
    public uint Quantity;
    public uint ItemId;
    public ushort ContainerId;
    public ushort Durability;
    public ushort Spiritbond;
    public fixed ushort Materia[5];
    public uint Unk40;
    public ushort Unk44;
    public fixed byte RetainerName[32];
    public fixed byte Unk66[32];
    public byte IsHQ;
    public byte MateriaCount;
    public byte Unk88;
    public byte TownId;
    public byte Stain0Id;
    public byte Stain1Id;
    public uint Unk8C;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MarketBoardItemListing
{
    public fixed byte EntriesRaw[10 * 0x90];
    public byte NextPageIndex;
    public byte FirstPageIndex;
    public byte RequestId;
    public fixed byte Padding[5];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MarketBoardPurchase
{
    public uint ItemId;
    public uint ErrorLogId;
    public uint Quantity;
    public byte Stackable;
    public fixed byte Padding[3];
}

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public unsafe struct MarketBoardSale
{
    [FieldOffset(0x00)] public uint ItemId;
    [FieldOffset(0x04)] public uint Quantity;
    [FieldOffset(0x08)] public uint UnitPrice;
    [FieldOffset(0x0C)] public uint TotalTax;
    [FieldOffset(0x10)] public byte SaleType; // 1 = normal sale, 2 = everything sold, 3 = mannequin
    [FieldOffset(0x11)] public byte TownId;
}

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public unsafe struct MarketBoardItemListingHistoryEntry
{
    [FieldOffset(0x00)] public uint UnitPrice;
    [FieldOffset(0x04)] public uint SaleUnixTimestamp;
    [FieldOffset(0x08)] public uint Quantity;
    [FieldOffset(0x0C)] public byte IsHQ;
    [FieldOffset(0x0D)] public byte UnkD;
    [FieldOffset(0x0E)] public fixed byte RetainerName[32];
}

[StructLayout(LayoutKind.Explicit, Size = 0x3C8)]
public unsafe struct MarketBoardItemListingHistory
{
    [FieldOffset(0x00)] public uint ItemId;
    [FieldOffset(0x04)] public fixed byte RawEntries[20 * 0x30];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct RetainerState
{
    public ulong RetainerId;
    public ulong Flags; // high byte & 0xF is town, highest bit is whether retainer is now selling
    public uint CustomMessageId;
    public byte StateChange; // % 10 is type (1 for rename?, 3 for start sell, 4 for stop sell)
    public fixed byte Name[32];
    public fixed byte Padding[3];

    public readonly byte Town => (byte)((Flags >> 56) & 0xF);
    public readonly bool IsSelling => (Flags >> 63) != 0;
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
    ServerRequestCallbackResponse = 925,
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
public unsafe struct RetainerSummary
{
    public uint SequenceId;
    public byte NumInformationPackets;
    public byte MaxRetainerEntitlement;
    public byte IsResponseToServerCallbackRequest;
    public byte Pad1;
    public uint ServerCallbackListenerIndex;
    public fixed byte DisplayOrder[10];
    public fixed byte Pad2[2];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct RetainerInformation
{
    public uint SequenceId;
    public uint Pad1;
    public ulong RetainerId;
    public byte Index;
    public byte NumItemsInInventory;
    public ushort Pad2;
    public uint Gil;
    public byte NumItemsOnMarket;
    public byte Town;
    public byte ClassJob;
    public byte Level;
    public uint MarketExpire;
    public ushort VentureId;
    public ushort Pad3;
    public uint VentureComplete;
    public byte Available;
    public byte Pad4;
    public ushort Unk2A;
    public byte Unk2C;
    public fixed byte Name[32];
    public fixed byte Pad5[3];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ItemMarketBoardSummary
{
    public uint SequenceId;
    public uint NumItemPackets;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ItemMarketBoardInfo
{
    public uint SequenceId;
    public uint InventoryType;
    public ushort Slot;
    public fixed byte Pad1[6];
    public ulong Unk10;
    public uint Unk18;
    public uint Pad2;
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
public unsafe struct DecodeServerRequestCallbackResponse
{
    public uint ListenerIndex;
    public uint ListenerRequestType;
    public byte DataCount;
    public fixed byte Padding[3];
    public fixed uint Data[1]; // variable length
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
