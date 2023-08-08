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
            Ping = 0x0378,
            Init = 0x02AC,
            Logout = 0x0116,
            CFCancel = 0x01BB,
            CFDutyInfo = 0x01BE,
            CFNotify = 0x0069,
            CFPreferredRole = 0x0160,
            CrossWorldLinkshellList = 0x03CC,
            FellowshipList = 0x01C5,
            Playtime = 0x0313,
            CFRegistered = 0x022A,
            Chat = 0x039B,
            RSVData = 0x0212,
            RSFData = 0x019B,
            SocialMessage = 0x015B,
            SocialMessage2 = 0x0373,
            SocialList = 0x00B1,
            SocialRequestResponse = 0x02F1,
            ExamineSearchInfo = 0x0357,
            UpdateSearchInfo = 0x0115,
            InitSearchInfo = 0x0070,
            ExamineSearchComment = 0x0199,
            ServerNoticeShort = 0x0125,
            ServerNotice = 0x02B1,
            SetOnlineStatus = 0x017A,
            LogMessage = 0x0316,
            Countdown = 0x0399,
            CountdownCancel = 0x0342,
            PartyMessage = 0x0336,
            PlayerAddedToBlacklist = 0x00E2,
            PlayerRemovedFromBlacklist = 0x00D0,
            BlackList = 0x0233,
            MailDeleteRequest = 0x01F8,
            MarketBoardItemListingCount = 0x0306,
            MarketBoardItemListing = 0x01DB,
            MarketBoardPurchase = 0x01F0,
            MarketBoardItemListingHistory = 0x02F4,
            RetainerSaleHistory = 0x023D,
            MarketBoardSearchResult = 0x03D6,
            FreeCompanyInfo = 0x030F,
            ExamineFreeCompanyInfo = 0x0158,
            FreeCompanyDialog = 0x01B4,
            StatusEffectList = 0x01DD,
            StatusEffectListEureka = 0x0192,
            StatusEffectListBozja = 0x0166,
            StatusEffectListDouble = 0x02CB,
            EffectResult1 = 0x02A3,
            EffectResult4 = 0x015D,
            EffectResult8 = 0x00C2,
            EffectResult16 = 0x00B3,
            EffectResultBasic1 = 0x00FA,
            EffectResultBasic4 = 0x0263,
            EffectResultBasic8 = 0x01B9,
            EffectResultBasic16 = 0x0370,
            EffectResultBasic32 = 0x028C,
            EffectResultBasic64 = 0x03DA,
            ActorControl = 0x00D4,
            ActorControlSelf = 0x03C1,
            ActorControlTarget = 0x00EF,
            UpdateHpMpTp = 0x01FB,
            ActionEffect1 = 0x0354,
            ActionEffect8 = 0x018F,
            ActionEffect16 = 0x038F,
            ActionEffect24 = 0x00D1,
            ActionEffect32 = 0x0340,
            StatusEffectListPlayer = 0x031F,
            UpdateRecastTimes = 0x037C,
            UpdateAllianceNormal = 0x02C9,
            UpdateAllianceSmall = 0x01F5,
            UpdatePartyMemberPositions = 0x01C2,
            UpdateAllianceNormalMemberPositions = 0x0093,
            UpdateAllianceSmallMemberPositions = 0x01B5,
            GCAffiliation = 0x0280,
            SpawnPlayer = 0x010E,
            SpawnNPC = 0x0091,
            SpawnBoss = 0x02B6,
            DespawnCharacter = 0x0112,
            ActorMove = 0x01AA,
            Transfer = 0x0276,
            ActorSetPos = 0x032C,
            ActorCast = 0x00C8,
            PlayerUpdateLook = 0x00A8,
            UpdateParty = 0x016F,
            InitZone = 0x0071,
            ApplyIDScramble = 0x03DE,
            UpdateHate = 0x033F,
            UpdateHater = 0x0356,
            SpawnObject = 0x0190,
            DespawnObject = 0x00D2,
            UpdateClassInfo = 0x03E3,
            UpdateClassInfoEureka = 0x00DA,
            UpdateClassInfoBozja = 0x006C,
            PlayerSetup = 0x020E,
            PlayerStats = 0x02F3,
            FirstAttack = 0x0161,
            PlayerStateFlags = 0x02ED,
            PlayerClassInfo = 0x00FB,
            ModelEquip = 0x0082,
            Examine = 0x0200,
            CharaNameReq = 0x0267,
            RetainerInformation = 0x02FE,
            ItemMarketBoardInfo = 0x011B,
            ItemInfo = 0x03A4,
            ContainerInfo = 0x0208,
            InventoryTransactionFinish = 0x0298,
            InventoryTransaction = 0x03DB,
            CurrencyCrystalInfo = 0x0389,
            InventoryActionAck = 0x0134,
            UpdateInventorySlot = 0x00A4,
            OpenTreasure = 0x0393,
            LootMessage = 0x0219,
            CreateTreasure = 0x0225,
            TreasureFadeOut = 0x0169,
            HuntingLogEntry = 0x00B9,
            EventPlay = 0x02DB,
            EventPlay4 = 0x00E8,
            EventPlay8 = 0x00FE,
            EventPlay16 = 0x008F,
            EventPlay32 = 0x0374,
            EventPlay64 = 0x027F,
            EventPlay128 = 0x0365,
            EventPlay255 = 0x00DB,
            EventStart = 0x02BE,
            EventFinish = 0x0289,
            EventContinue = 0x00D9,
            ResultDialog = 0x021F,
            DesynthResult = 0x0296,
            QuestActiveList = 0x0108,
            QuestUpdate = 0x02F0,
            QuestCompleteList = 0x017F,
            QuestFinish = 0x00F4,
            MSQTrackerComplete = 0x020A,
            QuestTracker = 0x02EC,
            Mount = 0x0242,
            DirectorVars = 0x0114,
            ContentDirectorSync = 0x01FA,
            EnvControl = 0x0137,
            SystemLogMessage1 = 0x03C8,
            SystemLogMessage2 = 0x0319,
            SystemLogMessage4 = 0x022F,
            SystemLogMessage8 = 0x00BA,
            SystemLogMessage16 = 0x01E5,
            BattleTalk2 = 0x0151,
            BattleTalk4 = 0x0372,
            BattleTalk8 = 0x00E3,
            MapUpdate = 0x00A3,
            MapUpdate4 = 0x0345,
            MapUpdate8 = 0x010C,
            MapUpdate16 = 0x0360,
            MapUpdate32 = 0x01B1,
            MapUpdate64 = 0x0325,
            MapUpdate128 = 0x009C,
            BalloonTalk2 = 0x02D6,
            BalloonTalk4 = 0x037E,
            BalloonTalk8 = 0x030B,
            WeatherChange = 0x021C,
            PlayerTitleList = 0x017C,
            Discovery = 0x014F,
            EorzeaTimeOffset = 0x01A2,
            EquipDisplayFlags = 0x024E,
            NpcYell = 0x0163,
            FateInfo = 0x025B,
            LandSetInitialize = 0x0228,
            LandUpdate = 0x026C,
            YardObjectSpawn = 0x02C0,
            HousingIndoorInitialize = 0x024F,
            LandAvailability = 0x0258,
            LandPriceUpdate = 0x0094,
            LandInfoSign = 0x0330,
            LandRename = 0x0255,
            HousingEstateGreeting = 0x0253,
            HousingUpdateLandFlagsSlot = 0x03A1,
            HousingLandFlags = 0x0197,
            HousingShowEstateGuestAccess = 0x02F2,
            HousingObjectInitialize = 0x039E,
            HousingInternalObjectSpawn = 0x031C,
            HousingWardInfo = 0x0395,
            HousingObjectMove = 0x021B,
            HousingObjectDye = 0x02A6,
            SharedEstateSettingsResponse = 0x02D1,
            DailyQuests = 0x0090,
            DailyQuestRepeatFlags = 0x0382,
            LandUpdateHouseName = 0x0098,
            AirshipTimers = 0x034F,
            PlaceMarker = 0x0202,
            WaymarkPreset = 0x0221,
            Waymark = 0x0194,
            UnMount = 0x01B0,
            CeremonySetActorAppearance = 0x0111,
            AirshipStatusList = 0x0359,
            AirshipStatus = 0x00C4,
            AirshipExplorationResult = 0x0152,
            SubmarineStatusList = 0x0376,
            SubmarineProgressionStatus = 0x0088,
            SubmarineExplorationResult = 0x03AF,
            SubmarineTimers = 0x0198,
            PrepareZoning = 0x0188,
            ActorGauge = 0x02A4,
            CharaVisualEffect = 0x02FC,
            LandSetMap = 0x0366,
            Fall = 0x023B,
            PlayMotionSync = 0x0097,
            CEDirector = 0x00F2,
            IslandWorkshopSupplyDemand = 0x00CD,

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
