using System.Runtime.InteropServices;

namespace BossMod
{
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
    // actor control examples: normal = toggle weapon, self = cooldown, target = target change
    class Protocol
    {
        public enum Opcode
        {
            Ping = 0x0319,
            Init = 0x035D,
            Logout = 0x0363,
            CFCancel = 0x02AD,
            CFDutyInfo = 0x00BA,
            CFNotify = 0x021A,
            CFPreferredRole = 0x00FB,
            CrossWorldLinkshellList = 0x0113,
            FellowshipList = 0x02A9,
            Playtime = 0x03A3,
            CFRegistered = 0x02D5,
            Chat = 0x01D6,
            RSVData = 0x008E,
            RSFData = 0x0328,
            SocialMessage = 0x031E,
            SocialMessage2 = 0x011C,
            SocialList = 0x023E,
            SocialRequestResponse = 0x0225,
            ExamineSearchInfo = 0x03DA,
            UpdateSearchInfo = 0x0092,
            InitSearchInfo = 0x00D8,
            ExamineSearchComment = 0x007E,
            ServerNoticeShort = 0x027B,
            ServerNotice = 0x006F,
            SetOnlineStatus = 0x00D9,
            LogMessage = 0x0133,
            Countdown = 0x0128,
            CountdownCancel = 0x0377,
            PartyMessage = 0x03CF,
            PlayerAddedToBlacklist = 0x03B3,
            PlayerRemovedFromBlacklist = 0x00E4,
            BlackList = 0x03CC,
            MailDeleteRequest = 0x01B5,
            MarketBoardItemListingCount = 0x0067,
            MarketBoardItemListing = 0x0221,
            MarketBoardPurchase = 0x01B6,
            MarketBoardItemListingHistory = 0x0069,
            RetainerSaleHistory = 0x00CF,
            MarketBoardSearchResult = 0x019F,
            FreeCompanyInfo = 0x00CE,
            ExamineFreeCompanyInfo = 0x0095,
            FreeCompanyDialog = 0x033E,
            StatusEffectList = 0x0171,
            StatusEffectListEureka = 0x00FF,
            StatusEffectListBozja = 0x032F,
            StatusEffectListDouble = 0x00DA,
            EffectResult1 = 0x010C,
            EffectResult4 = 0x0134,
            EffectResult8 = 0x03B8,
            EffectResult16 = 0x0355,
            EffectResultBasic1 = 0x039F,
            EffectResultBasic4 = 0x013B,
            EffectResultBasic8 = 0x01DF,
            EffectResultBasic16 = 0x0248,
            EffectResultBasic32 = 0x03CD,
            EffectResultBasic64 = 0x0169,
            ActorControl = 0x019D,
            ActorControlSelf = 0x00A6,
            ActorControlTarget = 0x02FE,
            UpdateHpMpTp = 0x0316,
            ActionEffect1 = 0x01BB,
            ActionEffect8 = 0x02BA,
            ActionEffect16 = 0x027F,
            ActionEffect24 = 0x0093,
            ActionEffect32 = 0x02A6,
            StatusEffectListPlayer = 0x009E,
            UpdateRecastTimes = 0x00C5,
            UpdateAllianceNormal = 0x03A4,
            UpdateAllianceSmall = 0x00F6,
            UpdatePartyMemberPositions = 0x00E8,
            UpdateAllianceNormalMemberPositions = 0x015D,
            UpdateAllianceSmallMemberPositions = 0x0290,
            GCAffiliation = 0x01E0,
            SpawnPlayer = 0x01A7,
            SpawnNPC = 0x00AC,
            SpawnBoss = 0x009B,
            DespawnCharacter = 0x0072,
            ActorMove = 0x02C4,
            Transfer = 0x02D0,
            ActorSetPos = 0x015F,
            ActorCast = 0x01C0,
            PlayerUpdateLook = 0x0351,
            UpdateParty = 0x0219,
            InitZone = 0x01D5,
            ApplyIDScramble = 0x0100,
            UpdateHate = 0x02D2,
            UpdateHater = 0x017F,
            SpawnObject = 0x0295,
            DespawnObject = 0x0386,
            UpdateClassInfo = 0x00A0,
            UpdateClassInfoEureka = 0x02FF,
            UpdateClassInfoBozja = 0x02FC,
            PlayerSetup = 0x03E6,
            PlayerStats = 0x0343,
            FirstAttack = 0x0215,
            PlayerStateFlags = 0x00A2,
            PlayerClassInfo = 0x01C5,
            ModelEquip = 0x036E,
            Examine = 0x0228,
            CharaNameReq = 0x01FF,
            RetainerInformation = 0x0229,
            ItemMarketBoardInfo = 0x00FE,
            ItemInfo = 0x0388,
            ContainerInfo = 0x021F,
            InventoryTransactionFinish = 0x01D1,
            InventoryTransaction = 0x0252,
            CurrencyCrystalInfo = 0x01A4,
            InventoryActionAck = 0x034C,
            UpdateInventorySlot = 0x0079,
            OpenTreasure = 0x02B7,
            LootMessage = 0x0360,
            CreateTreasure = 0x03CA,
            TreasureFadeOut = 0x01AE,
            HuntingLogEntry = 0x0138,
            EventPlay = 0x036F,
            EventPlay4 = 0x0124,
            EventPlay8 = 0x009A,
            EventPlay16 = 0x02F2,
            EventPlay32 = 0x00E3,
            EventPlay64 = 0x038F,
            EventPlay128 = 0x0197,
            EventPlay255 = 0x0202,
            EventStart = 0x03A6,
            EventFinish = 0x02FA,
            EventContinue = 0x01F2,
            ResultDialog = 0x007D,
            DesynthResult = 0x0325,
            QuestActiveList = 0x0257,
            QuestUpdate = 0x0306,
            QuestCompleteList = 0x02AA,
            QuestFinish = 0x00A1,
            MSQTrackerComplete = 0x02F8,
            QuestTracker = 0x0109,
            Mount = 0x0244,
            DirectorVars = 0x0192,
            ContentDirectorSync = 0x03C4,
            EnvControl = 0x02E0,
            SystemLogMessage1 = 0x039D,
            SystemLogMessage2 = 0x02F0,
            SystemLogMessage4 = 0x00DE,
            SystemLogMessage8 = 0x0392,
            SystemLogMessage16 = 0x01B0,
            BattleTalk2 = 0x012C,
            BattleTalk4 = 0x0318,
            BattleTalk8 = 0x0366,
            MapUpdate = 0x02EE,
            MapUpdate4 = 0x03BA,
            MapUpdate8 = 0x0272,
            MapUpdate16 = 0x01AB,
            MapUpdate32 = 0x03BC,
            MapUpdate64 = 0x01CA,
            MapUpdate128 = 0x00C7,
            BalloonTalk2 = 0x0241,
            BalloonTalk4 = 0x0276,
            BalloonTalk8 = 0x028F,
            WeatherChange = 0x02D9,
            PlayerTitleList = 0x0380,
            Discovery = 0x01D0,
            EorzeaTimeOffset = 0x0108,
            EquipDisplayFlags = 0x0259,
            NpcYell = 0x0088,
            FateInfo = 0x028A,
            LandSetInitialize = 0x02EF,
            LandUpdate = 0x01ED,
            YardObjectSpawn = 0x0167,
            HousingIndoorInitialize = 0x0174,
            LandAvailability = 0x0284,
            LandPriceUpdate = 0x016C,
            LandInfoSign = 0x02F4,
            LandRename = 0x019B,
            HousingEstateGreeting = 0x00A7,
            HousingUpdateLandFlagsSlot = 0x0333,
            HousingLandFlags = 0x01C4,
            HousingShowEstateGuestAccess = 0x0296,
            HousingObjectInitialize = 0x0251,
            HousingInternalObjectSpawn = 0x00F1,
            HousingWardInfo = 0x0110,
            HousingObjectMove = 0x008B,
            HousingObjectDye = 0x0154,
            SharedEstateSettingsResponse = 0x0254,
            DailyQuests = 0x02D6,
            DailyQuestRepeatFlags = 0x0337,
            LandUpdateHouseName = 0x014C,
            AirshipTimers = 0x03C5,
            PlaceMarker = 0x016F,
            WaymarkPreset = 0x00CC,
            Waymark = 0x0139,
            UnMount = 0x010B,
            CeremonySetActorAppearance = 0x02C6,
            AirshipStatusList = 0x021E,
            AirshipStatus = 0x0383,
            AirshipExplorationResult = 0x01EA,
            SubmarineStatusList = 0x01D3,
            SubmarineProgressionStatus = 0x0356,
            SubmarineExplorationResult = 0x0372,
            SubmarineTimers = 0x0293,
            PrepareZoning = 0x039C,
            ActorGauge = 0x026D,
            CharaVisualEffect = 0x00F4,
            LandSetMap = 0x035E,
            Fall = 0x038C,
            PlayMotionSync = 0x0099,
            CEDirector = 0x0122,
            IslandWorkshopSupplyDemand = 0x032E,

            // client->server; TODO move to a different enum
            //ActionRequest = 0x029E,
            //ActionRequestGroundTargeted = 0x0205,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_IPCHeader
        {
            public ushort Magic; // 0x0014
            public ushort MessageType;
            public uint Unknown1;
            public uint Epoch;
            public uint Unknown2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffectHeader
        {
            public ulong animationTargetId;  // who the animation targets
            public uint actionId; // what the casting player casts, shown in battle log / ui
            public uint globalEffectCounter;
            public float animationLockTime;
            public uint SomeTargetID;
            public ushort SourceSequence; // 0 = initiated by server, otherwise corresponds to client request sequence id
            public ushort rotation;
            public ushort actionAnimationId;
            public byte variation; // animation
            public ActionType actionType;
            public byte unknown20;
            public byte NumTargets; // machina calls it 'effectCount', but it is misleading imo
            public ushort padding21;
            public ushort padding22;
            public ushort padding23;
            public ushort padding24;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect1
        {
            public Server_ActionEffectHeader Header;
            public fixed ulong Effects[8]; // ActionEffect[8]
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[1];
            public uint padding5;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect8
        {
            public Server_ActionEffectHeader Header;
            public fixed ulong Effects[8 * 8];
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
        public unsafe struct Server_ActionEffect16
        {
            public Server_ActionEffectHeader Header;
            public fixed ulong Effects[8 * 16];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[16];
            public ushort TargetX; // floatCoord = ((intCoord * 3.0518043) * 0.0099999998) - 1000.0 (0 => -1000, 65535 => +1000)
            public ushort TargetY;
            public ushort TargetZ;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect24
        {
            public Server_ActionEffectHeader Header;
            public fixed ulong Effects[8 * 24];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[24];
            public ushort TargetX; // floatCoord = ((intCoord * 3.0518043) * 0.0099999998) - 1000.0 (0 => -1000, 65535 => +1000)
            public ushort TargetY;
            public ushort TargetZ;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect32
        {
            public Server_ActionEffectHeader Header;
            public fixed ulong Effects[8 * 32];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[32];
            public ushort TargetX; // floatCoord = ((intCoord * 3.0518043) * 0.0099999998) - 1000.0 (0 => -1000, 65535 => +1000)
            public ushort TargetY;
            public ushort TargetZ;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorCast
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
            public uint u2_objID;
            public ushort PosX;
            public ushort PosY;
            public ushort PosZ;
            public ushort u3;
        }

        public enum Server_ActorControlCategory : ushort
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
            HoT_DoT = 23, // dissector calls it HPFloatingText
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
            SetTitle = 500, // from dissector
            SetTargetSign = 502,
            SetStatusIcon = 504, // from dissector
            LimitBreakGauge = 505, // name from dissector
            SetHomepoint = 507, // from dissector
            SetFavorite = 508, // from dissector
            LearnTeleport = 509, // from dissector
            OpenRecommendationGuide = 512, // from dissector
            ArmoryErrorMsg = 513, // from dissector
            AchievementPopup = 515, // from dissector
            LogMsg = 517, // from dissector
            AchievementMsg = 518, // from dissector
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
            SetDutyActionId = 1512, // from dissector
            SetDutyActionHud = 1513, // from dissector
            SetDutyActionActive = 1514, // from dissector
            SetDutyActionRemaining = 1515, // from dissector
            IncrementRecast = 1536, // p1=cooldown group, p2=delta time quantized to 100ms; example is brd mage ballad proc
            EurekaStep = 1850, // from dissector
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorControl
        {
            public Server_ActorControlCategory category;
            public ushort unk0;
            public uint param1;
            public uint param2;
            public uint param3;
            public uint param4;
            public uint param5;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorControlSelf
        {
            public Server_ActorControlCategory category;
            public ushort unk0;
            public uint param1;
            public uint param2;
            public uint param3;
            public uint param4;
            public uint param5;
            public uint param6;
            public uint param7;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorControlTarget
        {
            public Server_ActorControlCategory category;
            public ushort unk0;
            public uint param1;
            public uint param2;
            public uint param3;
            public uint param4;
            public uint param5;
            public uint TargetID;
            public uint unk1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorGauge
        {
            public Class ClassJobID;
            public ulong Payload;
            public byte u5;
            public ushort u6;
            public ulong u8;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorMove
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
        public struct Server_EffectResultEffectEntry
        {
            public byte EffectIndex;
            public byte padding1;
            public ushort EffectID;
            public ushort Extra;
            public ushort padding2;
            public float Duration;
            public uint SourceActorID;
        }

        // EffectResultN has byte NumEntries at offset 0 and array EffectResultEntry[N] at offset 4
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_EffectResultEntry
        {
            public uint RelatedActionSequence;
            public uint ActorID;
            public uint CurrentHP;
            public uint MaxHP;
            public ushort CurrentMP;
            public byte RelatedTargetIndex;
            public byte ClassJob;
            public byte DamageShield;
            public byte EffectCount;
            public ushort padding3;
            public fixed byte Effects[4 * 4 * 4]; // Server_EffectResultEffectEntry[4]
        }

        // EffectResultBasicN has byte NumEntries at offset 0 and array EffectResultBasicEntry[N] at offset 4
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_EffectResultBasicEntry
        {
            public uint RelatedActionSequence;
            public uint ActorID;
            public uint CurrentHP;
            public byte RelatedTargetIndex;
            public byte padding3;
            public ushort padding4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_Waymark
        {
            public Waymark Waymark;
            public byte Active; // 0=off, 1=on
            public ushort unknown;
            public int PosX;
            public int PosY;// To calculate 'float' coords from these you cast them to float and then divide by 1000.0
            public int PosZ;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_WaymarkPreset
        {
            public byte WaymarkMask;
            public byte Unknown1;
            public short Unknown2;
            public fixed int PosX[8];// Xints[0] has X of waymark A, Xints[1] X of B, etc.
            public fixed int PosY[8];// To calculate 'float' coords from these you cast them to float and then divide by 1000.0
            public fixed int PosZ[8];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_EnvControl
        {
            public uint FeatureID; // seen 0x80xxxxxx, seems to be unique identifier of controlled feature
            public uint State; // typically hiword and loword both have one bit set; in disassembly this is actually 2 words
            public byte Index; // if feature has multiple elements, this is a 0-based index of element
            public byte u0; // padding?
            public ushort u1; // padding?
            public uint u2; // padding?
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_UpdateRecastTimes
        {
            public fixed float Elapsed[80];
            public fixed float Total[80];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Client_ActionRequest
        {
            public byte ActionProcState; // see ActionManager.GetAdjustedCastTime implementation, last optional arg
            public ActionType Type;
            public ushort u1;
            public uint ActionID;
            public ushort Sequence;
            public ushort IntCasterRot; // 0 = N, increases CCW, 0xFFFF = 2pi
            public ushort IntDirToTarget; // 0 = N, increases CCW, 0xFFFF = 2pi
            public ushort u3;
            public ulong TargetID;
            public ushort ItemSourceSlot;
            public ushort ItemSourceContainer;
            public uint u4;
            public ulong u5;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Client_ActionRequestGroundTargeted
        {
            public byte ActionProcState; // see ActionManager.GetAdjustedCastTime implementation, last optional arg
            public ActionType Type;
            public ushort u1;
            public uint ActionID;
            public ushort Sequence;
            public ushort IntCasterRot; // 0 = N, increases CCW, 0xFFFF = 2pi
            public ushort IntDirToTarget; // 0 = N, increases CCW, 0xFFFF = 2pi
            public ushort u3;
            public float LocX;
            public float LocY;
            public float LocZ;
            public uint u4;
            public ulong u5;
        }
    }
}
