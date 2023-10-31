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
            Ping = 0x0300,
            Init = 0x00F4,
            Logout = 0x02B0,
            CFCancel = 0x0121,
            CFDutyInfo = 0x030D,
            CFNotify = 0x025A,
            CFPreferredRole = 0x0392,
            CrossWorldLinkshellList = 0x025E,
            FellowshipList = 0x00C5,
            Playtime = 0x008D,
            CFRegistered = 0x0188,
            Chat = 0x02E3,
            RSVData = 0x01BD,
            RSFData = 0x03D8,
            SocialMessage = 0x037A,
            SocialMessage2 = 0x01BF,
            SocialList = 0x018C,
            SocialRequestResponse = 0x00FC,
            ExamineSearchInfo = 0x0150,
            UpdateSearchInfo = 0x0110,
            InitSearchInfo = 0x03C8,
            ExamineSearchComment = 0x02EE,
            ServerNoticeShort = 0x011A,
            ServerNotice = 0x0205,
            SetOnlineStatus = 0x007A,
            LogMessage = 0x029E,
            Countdown = 0x03DA,
            CountdownCancel = 0x03A3,
            PartyMessage = 0x00A3,
            PlayerAddedToBlacklist = 0x02CF,
            PlayerRemovedFromBlacklist = 0x036F,
            BlackList = 0x0221,
            LinkshellList = 0x009A,
            MailDeleteRequest = 0x00D8,
            MarketBoardItemListingCount = 0x014A,
            MarketBoardItemListing = 0x0376,
            MarketBoardPurchase = 0x02AF,
            MarketBoardItemListingHistory = 0x0253,
            RetainerSaleHistory = 0x017D,
            MarketBoardSearchResult = 0x016E,
            FreeCompanyInfo = 0x01F6,
            ExamineFreeCompanyInfo = 0x02CB,
            FreeCompanyDialog = 0x0093,
            StatusEffectList = 0x0163,
            StatusEffectListEureka = 0x0290,
            StatusEffectListBozja = 0x03B0,
            StatusEffectListDouble = 0x01DF,
            EffectResult1 = 0x01FC,
            EffectResult4 = 0x015F,
            EffectResult8 = 0x0362,
            EffectResult16 = 0x035C,
            EffectResultBasic1 = 0x033E,
            EffectResultBasic4 = 0x01AA,
            EffectResultBasic8 = 0x03DC,
            EffectResultBasic16 = 0x01C3,
            EffectResultBasic32 = 0x02C2,
            EffectResultBasic64 = 0x039A,
            ActorControl = 0x0342,
            ActorControlSelf = 0x00E4,
            ActorControlTarget = 0x01D7,
            UpdateHpMpTp = 0x0353,
            ActionEffect1 = 0x00C0,
            ActionEffect8 = 0x01D6,
            ActionEffect16 = 0x025D,
            ActionEffect24 = 0x00D9,
            ActionEffect32 = 0x01A3,
            StatusEffectListPlayer = 0x02D0,
            StatusEffectListPlayerDouble = 0x02BA,
            UpdateRecastTimes = 0x01A9,
            UpdateAllianceNormal = 0x0233,
            UpdateAllianceSmall = 0x01D5,
            UpdatePartyMemberPositions = 0x0340,
            UpdateAllianceNormalMemberPositions = 0x033B,
            UpdateAllianceSmallMemberPositions = 0x01CA,
            GCAffiliation = 0x01AC,
            SpawnPlayer = 0x0167,
            SpawnNPC = 0x02BB,
            SpawnBoss = 0x01C8,
            DespawnCharacter = 0x01E7,
            ActorMove = 0x02F9,
            Transfer = 0x01E3,
            ActorSetPos = 0x028F,
            ActorCast = 0x02D2,
            PlayerUpdateLook = 0x011B,
            UpdateParty = 0x0361,
            InitZone = 0x0243,
            ApplyIDScramble = 0x0277,
            UpdateHate = 0x01EB,
            UpdateHater = 0x03E3,
            SpawnObject = 0x00DD,
            DespawnObject = 0x0337,
            UpdateClassInfo = 0x02EA,
            UpdateClassInfoEureka = 0x0288,
            UpdateClassInfoBozja = 0x010B,
            PlayerSetup = 0x0095,
            PlayerStats = 0x0087,
            FirstAttack = 0x0297,
            PlayerStateFlags = 0x031A,
            PlayerClassInfo = 0x0130,
            ModelEquip = 0x00C4,
            Examine = 0x0153,
            CharaNameReq = 0x0196,
            RetainerInformation = 0x00B3,
            ItemMarketBoardInfo = 0x0088,
            ItemInfo = 0x008A,
            ContainerInfo = 0x019D,
            InventoryTransactionFinish = 0x02D5,
            InventoryTransaction = 0x02FD,
            CurrencyCrystalInfo = 0x01E1,
            InventoryActionAck = 0x02CD,
            UpdateInventorySlot = 0x0295,
            OpenTreasure = 0x01B8,
            LootMessage = 0x0135,
            CreateTreasure = 0x0097,
            TreasureFadeOut = 0x0170,
            HuntingLogEntry = 0x0333,
            EventPlay = 0x022E,
            EventPlay4 = 0x034A,
            EventPlay8 = 0x02E8,
            EventPlay16 = 0x00DB,
            EventPlay32 = 0x00A5,
            EventPlay64 = 0x0147,
            EventPlay128 = 0x0197,
            EventPlay255 = 0x00A6,
            EventStart = 0x00CD,
            EventFinish = 0x01BE,
            EventContinue = 0x00D3,
            ResultDialog = 0x03E2,
            DesynthResult = 0x030F,
            QuestActiveList = 0x02F0,
            QuestUpdate = 0x0172,
            QuestCompleteList = 0x0079,
            QuestFinish = 0x038D,
            MSQTrackerComplete = 0x0385,
            QuestTracker = 0x0199,
            Mount = 0x029F,
            DirectorVars = 0x019A,
            ContentDirectorSync = 0x020A,
            EnvControl = 0x014B,
            SystemLogMessage1 = 0x03D7,
            SystemLogMessage2 = 0x02AC,
            SystemLogMessage4 = 0x0148,
            SystemLogMessage8 = 0x01B9,
            SystemLogMessage16 = 0x030E,
            BattleTalk2 = 0x0106,
            BattleTalk4 = 0x018F,
            BattleTalk8 = 0x01F1,
            MapUpdate = 0x0128,
            MapUpdate4 = 0x0343,
            MapUpdate8 = 0x0257,
            MapUpdate16 = 0x00D2,
            MapUpdate32 = 0x007D,
            MapUpdate64 = 0x00DC,
            MapUpdate128 = 0x0068,
            BalloonTalk2 = 0x0160,
            BalloonTalk4 = 0x01B4,
            BalloonTalk8 = 0x00C8,
            WeatherChange = 0x0397,
            PlayerTitleList = 0x0329,
            Discovery = 0x02DE,
            EorzeaTimeOffset = 0x0393,
            EquipDisplayFlags = 0x02F8,
            NpcYell = 0x0129,
            FateInfo = 0x017B,
            LandSetInitialize = 0x03E1,
            LandUpdate = 0x028D,
            YardObjectSpawn = 0x01E5,
            HousingIndoorInitialize = 0x0131,
            LandAvailability = 0x03CD,
            LandPriceUpdate = 0x010C,
            LandInfoSign = 0x0398,
            LandRename = 0x0384,
            HousingEstateGreeting = 0x034F,
            HousingUpdateLandFlagsSlot = 0x00B6,
            HousingLandFlags = 0x0086,
            HousingShowEstateGuestAccess = 0x0319,
            HousingObjectInitialize = 0x024D,
            HousingInternalObjectSpawn = 0x008B,
            HousingWardInfo = 0x021A,
            HousingObjectMove = 0x0112,
            HousingObjectDye = 0x00CC,
            SharedEstateSettingsResponse = 0x030C,
            DailyQuests = 0x02DD,
            DailyQuestRepeatFlags = 0x019B,
            LandUpdateHouseName = 0x01D1,
            AirshipTimers = 0x00AB,
            PlaceMarker = 0x03BC,
            WaymarkPreset = 0x02ED,
            Waymark = 0x02A1,
            UnMount = 0x034D,
            CeremonySetActorAppearance = 0x02FF,
            AirshipStatusList = 0x01FD,
            AirshipStatus = 0x01A2,
            AirshipExplorationResult = 0x027C,
            SubmarineStatusList = 0x02F7,
            SubmarineProgressionStatus = 0x0358,
            SubmarineExplorationResult = 0x03C2,
            SubmarineTimers = 0x0186,
            PrepareZoning = 0x02C3,
            ActorGauge = 0x036C,
            CharaVisualEffect = 0x0344,
            LandSetMap = 0x007F,
            Fall = 0x0136,
            PlayMotionSync = 0x039C,
            CEDirector = 0x009D,
            IslandWorkshopDemandResearch = 0x0127,
            IslandWorkshopSupplyDemand = 0x0326,
            IslandWorkshopFavors = 0x01C7,

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
