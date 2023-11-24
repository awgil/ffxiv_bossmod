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
            Ping = 0x0184,
            Init = 0x027F,
            Logout = 0x02AB,
            CFCancel = 0x039E,
            CFDutyInfo = 0x0318,
            CFNotify = 0x0117,
            CFPreferredRole = 0x0244,
            CrossWorldLinkshellList = 0x0122,
            FellowshipList = 0x03B8,
            Playtime = 0x02DD,
            CFRegistered = 0x030B,
            Chat = 0x00A2,
            RSVData = 0x028D,
            RSFData = 0x02B3,
            SocialMessage = 0x00DB,
            SocialMessage2 = 0x012A,
            SocialList = 0x0366,
            SocialRequestResponse = 0x02A0,
            ExamineSearchInfo = 0x03E3,
            UpdateSearchInfo = 0x01CB,
            InitSearchInfo = 0x0068,
            ExamineSearchComment = 0x0250,
            ServerNoticeShort = 0x01F3,
            ServerNotice = 0x01C8,
            SetOnlineStatus = 0x0291,
            LogMessage = 0x00DD,
            Countdown = 0x0125,
            CountdownCancel = 0x03BC,
            PartyMessage = 0x02A7,
            PlayerAddedToBlacklist = 0x022C,
            PlayerRemovedFromBlacklist = 0x00EE,
            BlackList = 0x02D1,
            LinkshellList = 0x02F3,
            MailDeleteRequest = 0x02C8,
            MarketBoardItemListingCount = 0x0283,
            MarketBoardItemListing = 0x025A,
            MarketBoardPurchase = 0x0106,
            MarketBoardItemListingHistory = 0x00E7,
            RetainerSaleHistory = 0x026B,
            MarketBoardSearchResult = 0x015E,
            FreeCompanyInfo = 0x0182,
            ExamineFreeCompanyInfo = 0x038C,
            FreeCompanyDialog = 0x015B,
            StatusEffectList = 0x0131,
            StatusEffectListEureka = 0x0287,
            StatusEffectListBozja = 0x03BF,
            StatusEffectListDouble = 0x0147,
            EffectResult1 = 0x031E,
            EffectResult4 = 0x03A4,
            EffectResult8 = 0x006E,
            EffectResult16 = 0x030E,
            EffectResultBasic1 = 0x0301,
            EffectResultBasic4 = 0x0175,
            EffectResultBasic8 = 0x031F,
            EffectResultBasic16 = 0x01A3,
            EffectResultBasic32 = 0x00CD,
            EffectResultBasic64 = 0x014C,
            ActorControl = 0x03B6,
            ActorControlSelf = 0x00E2,
            ActorControlTarget = 0x0341,
            UpdateHpMpTp = 0x00B2,
            ActionEffect1 = 0x019F,
            ActionEffect8 = 0x03A2,
            ActionEffect16 = 0x0126,
            ActionEffect24 = 0x0087,
            ActionEffect32 = 0x0248,
            StatusEffectListPlayer = 0x01CC,
            StatusEffectListPlayerDouble = 0x0150,
            UpdateRecastTimes = 0x0268,
            UpdateAllianceNormal = 0x018C,
            UpdateAllianceSmall = 0x00E0,
            UpdatePartyMemberPositions = 0x01B4,
            UpdateAllianceNormalMemberPositions = 0x0228,
            UpdateAllianceSmallMemberPositions = 0x0195,
            GCAffiliation = 0x020E,
            SpawnPlayer = 0x007E,
            SpawnNPC = 0x023B,
            SpawnBoss = 0x0315,
            DespawnCharacter = 0x025D,
            ActorMove = 0x0223,
            Transfer = 0x02B7,
            ActorSetPos = 0x00A8,
            ActorCast = 0x0107,
            PlayerUpdateLook = 0x03A0,
            UpdateParty = 0x03AA,
            InitZone = 0x0180,
            ApplyIDScramble = 0x024E,
            UpdateHate = 0x0201,
            UpdateHater = 0x00BE,
            SpawnObject = 0x0391,
            DespawnObject = 0x02FF,
            UpdateClassInfo = 0x01D7,
            UpdateClassInfoEureka = 0x02B6,
            UpdateClassInfoBozja = 0x0282,
            PlayerSetup = 0x023D,
            PlayerStats = 0x01B0,
            FirstAttack = 0x029A,
            PlayerStateFlags = 0x010A,
            PlayerClassInfo = 0x0226,
            ModelEquip = 0x01F5,
            Examine = 0x01D1,
            CharaNameReq = 0x00C9,
            RetainerInformation = 0x01EB,
            ItemMarketBoardInfo = 0x0256,
            ItemInfo = 0x0270,
            ContainerInfo = 0x0069,
            InventoryTransactionFinish = 0x03BD,
            InventoryTransaction = 0x021B,
            CurrencyCrystalInfo = 0x0375,
            InventoryActionAck = 0x0258,
            UpdateInventorySlot = 0x025C,
            OpenTreasure = 0x01F6,
            LootMessage = 0x0077,
            CreateTreasure = 0x017F,
            TreasureFadeOut = 0x02C9,
            HuntingLogEntry = 0x03E6,
            EventPlay = 0x0331,
            EventPlay4 = 0x012C,
            EventPlay8 = 0x0217,
            EventPlay16 = 0x02E5,
            EventPlay32 = 0x0198,
            EventPlay64 = 0x013C,
            EventPlay128 = 0x0373,
            EventPlay255 = 0x02CD,
            EventStart = 0x03C0,
            EventFinish = 0x034D,
            EventContinue = 0x0313,
            ResultDialog = 0x0352,
            DesynthResult = 0x00C1,
            QuestActiveList = 0x0095,
            QuestUpdate = 0x014B,
            QuestCompleteList = 0x029D,
            QuestFinish = 0x00F7,
            MSQTrackerComplete = 0x0298,
            QuestTracker = 0x02DE,
            Mount = 0x03CC,
            DirectorVars = 0x012B,
            ContentDirectorSync = 0x0141,
            EnvControl = 0x0245,
            SystemLogMessage1 = 0x032A,
            SystemLogMessage2 = 0x00A6,
            SystemLogMessage4 = 0x0367,
            SystemLogMessage8 = 0x01AB,
            SystemLogMessage16 = 0x036B,
            BattleTalk2 = 0x008C,
            BattleTalk4 = 0x01DD,
            BattleTalk8 = 0x01DB,
            MapUpdate = 0x01E6,
            MapUpdate4 = 0x0361,
            MapUpdate8 = 0x033C,
            MapUpdate16 = 0x0297,
            MapUpdate32 = 0x0213,
            MapUpdate64 = 0x0173,
            MapUpdate128 = 0x0092,
            BalloonTalk2 = 0x03C6,
            BalloonTalk4 = 0x0152,
            BalloonTalk8 = 0x03DF,
            WeatherChange = 0x02DC,
            PlayerTitleList = 0x0381,
            Discovery = 0x03DE,
            EorzeaTimeOffset = 0x037F,
            EquipDisplayFlags = 0x02D2,
            NpcYell = 0x0070,
            FateInfo = 0x02EA,
            LandSetInitialize = 0x0334,
            LandUpdate = 0x0082,
            YardObjectSpawn = 0x02D8,
            HousingIndoorInitialize = 0x025B,
            LandAvailability = 0x035B,
            LandPriceUpdate = 0x006B,
            LandInfoSign = 0x0091,
            LandRename = 0x0066,
            HousingEstateGreeting = 0x02A8,
            HousingUpdateLandFlagsSlot = 0x011E,
            HousingLandFlags = 0x00F2,
            HousingShowEstateGuestAccess = 0x0196,
            HousingObjectInitialize = 0x01E2,
            HousingInternalObjectSpawn = 0x031A,
            HousingWardInfo = 0x02B5,
            HousingObjectMove = 0x0113,
            HousingObjectDye = 0x00CC,
            SharedEstateSettingsResponse = 0x0357,
            DailyQuests = 0x0164,
            DailyQuestRepeatFlags = 0x03D4,
            LandUpdateHouseName = 0x013D,
            AirshipTimers = 0x0200,
            PlaceMarker = 0x036A,
            WaymarkPreset = 0x0072,
            Waymark = 0x0254,
            UnMount = 0x02EC,
            CeremonySetActorAppearance = 0x0330,
            AirshipStatusList = 0x02BC,
            AirshipStatus = 0x0300,
            AirshipExplorationResult = 0x03B0,
            SubmarineStatusList = 0x00CF,
            SubmarineProgressionStatus = 0x00D9,
            SubmarineExplorationResult = 0x006F,
            SubmarineTimers = 0x0192,
            PrepareZoning = 0x03B9,
            ActorGauge = 0x02B4,
            CharaVisualEffect = 0x0116,
            LandSetMap = 0x017B,
            Fall = 0x00AA,
            PlayMotionSync = 0x0139,
            CEDirector = 0x02EB,
            IslandWorkshopDemandResearch = 0x0229,
            IslandWorkshopSupplyDemand = 0x0158,
            IslandWorkshopFavors = 0x0328,

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
            public byte Flags;
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
