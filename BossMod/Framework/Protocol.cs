using System.Runtime.InteropServices;

namespace BossMod
{
    // taken from Machina, FFXIVPacketDissector, XIVAlexander, FFXIVOpcodes and custom research
    class Protocol
    {
        public enum Opcode
        {
            Ping = 0x0080,
            Init = 0x0281,
            Logout = 0x018B,
            CFCancel = 0x01C5,
            CFDutyInfo = 0x0189,
            CFNotify = 0x02D6,
            CFPreferredRole = 0x02F3,
            CrossWorldLinkshellList = 0x0083,
            FellowshipList = 0x00DB,
            Playtime = 0x018F,
            CFRegistered = 0x0136,
            Chat = 0x01A7,
            RSVData = 0x01AE,
            RSFData = 0x00CB,
            SocialMessage = 0x008D,
            SocialMessage2 = 0x029B,
            SocialList = 0x037D,
            SocialRequestResponse = 0x02E6,
            ExamineSearchInfo = 0x011B,
            UpdateSearchInfo = 0x00AE,
            InitSearchInfo = 0x0273,
            ExamineSearchComment = 0x039B,
            ServerNoticeShort = 0x0270,
            ServerNotice = 0x0177,
            SetOnlineStatus = 0x0127,
            LogMessage = 0x02AA,
            Countdown = 0x029F,
            CountdownCancel = 0x00E5,
            PartyMessage = 0x038F,
            PlayerAddedToBlacklist = 0x01C3,
            PlayerRemovedFromBlacklist = 0x014F,
            BlackList = 0x01D0,
            MailDeleteRequest = 0x0337,
            MarketBoardItemListingCount = 0x0097,
            MarketBoardItemListing = 0x0122,
            MarketBoardPurchase = 0x024E,
            MarketBoardItemListingHistory = 0x03E3,
            RetainerSaleHistory = 0x0129,
            MarketBoardSearchResult = 0x0081,
            FreeCompanyInfo = 0x0182,
            ExamineFreeCompanyInfo = 0x03AE,
            FreeCompanyDialog = 0x021E,
            StatusEffectList = 0x01CB,
            StatusEffectListEureka = 0x0349,
            StatusEffectListBozja = 0x018D, // machina: StatusEffectList2
            StatusEffectListDouble = 0x02CB, // machina: BossStatusEffectList
            EffectResult1 = 0x0140,
            EffectResult4 = 0x0091,
            EffectResult8 = 0x03C7,
            EffectResult16 = 0x0077,
            EffectResultBasic1 = 0x0366,
            EffectResultBasic4 = 0x006F,
            EffectResultBasic8 = 0x01FE,
            EffectResultBasic16 = 0x006D,
            EffectResultBasic32 = 0x0399,
            EffectResultBasic64 = 0x01B7,
            ActorControl = 0x03A0, // example: toggle weapon
            ActorControlSelf = 0x0286, // example: cooldown
            ActorControlTarget = 0x03BD, // example: target change
            UpdateHpMpTp = 0x008E,
            ActionEffect1 = 0x028A, // machina: AbilityN
            ActionEffect8 = 0x015D,
            ActionEffect16 = 0x01B4,
            ActionEffect24 = 0x034A,
            ActionEffect32 = 0x010C,
            StatusEffectListPlayer = 0x039E, // machina: StatusEffectList3
            UpdateRecastTimes = 0x039C,
            UpdateAllianceNormal = 0x02C3,
            UpdateAllianceSmall = 0x00DC,
            UpdatePartyMemberPositions = 0x0126,
            UpdateAllianceNormalMemberPositions = 0x0150,
            UpdateAllianceSmallMemberPositions = 0x00B8,
            GCAffiliation = 0x0068,
            SpawnPlayer = 0x0251,
            SpawnNPC = 0x011C,
            SpawnBoss = 0x0384,
            DespawnCharacter = 0x0227,
            ActorMove = 0x038D,
            Transfer = 0x0167,
            ActorSetPos = 0x02BB,
            ActorCast = 0x034D,
            PlayerUpdateLook = 0x01BB,
            UpdateParty = 0x01F9,
            InitZone = 0x02D0,
            ApplyIDScramble = 0x0117,
            UpdateHate = 0x01A3,
            UpdateHater = 0x01DA,
            SpawnObject = 0x012A, // FFXIVOpcodes: ObjectSpawn
            DespawnObject = 0x03B1,
            UpdateClassInfo = 0x0158,
            UpdateClassInfoEureka = 0x008F,
            UpdateClassInfoBozja = 0x03D4,
            PlayerSetup = 0x0263,
            PlayerStats = 0x026C,
            FirstAttack = 0x00D7,
            PlayerStateFlags = 0x02A6,
            PlayerClassInfo = 0x0383,
            ModelEquip = 0x03B0,
            Examine = 0x0075,
            CharaNameReq = 0x025E,
            RetainerInformation = 0x0233,
            ItemMarketBoardInfo = 0x02AF,
            ItemInfo = 0x0103,
            ContainerInfo = 0x021C,
            InventoryTransactionFinish = 0x016F,
            InventoryTransaction = 0x02DF,
            CurrencyCrystalInfo = 0x0350,
            InventoryActionAck = 0x0162,
            UpdateInventorySlot = 0x037A,
            OpenTreasure = 0x023B,
            LootMessage = 0x010D,
            CreateTreasure = 0x0199,
            TreasureFadeOut = 0x00EA,
            HuntingLogEntry = 0x03A4,
            EventPlay = 0x02D2,
            EventPlay4 = 0x00BC,
            EventPlay8 = 0x0250,
            EventPlay16 = 0x0391,
            EventPlay32 = 0x01FF,
            EventPlay64 = 0x01BE,
            EventPlay128 = 0x032F,
            EventPlay255 = 0x00E4,
            EventStart = 0x00AF,
            EventFinish = 0x014C,
            EventContinue = 0x02B5,
            ResultDialog = 0x03DC,
            DesynthResult = 0x0090,
            QuestActiveList = 0x015E,
            QuestUpdate = 0x03BB,
            QuestCompleteList = 0x0102,
            QuestFinish = 0x00BD,
            MSQTrackerComplete = 0x02C6,
            QuestTracker = 0x02B7,
            Mount = 0x0245,
            DirectorVars = 0x0169,
            ContentDirectorSync = 0x03AB,
            EnvControl = 0x00F9,
            SystemLogMessage1 = 0x0193, // FFXIVOpcodes: SomeDirectorUnk4
            SystemLogMessage2 = 0x0198,
            SystemLogMessage4 = 0x0368,
            SystemLogMessage8 = 0x02B1,
            SystemLogMessage16 = 0x00D3,
            BattleTalk2 = 0x03B9,
            BattleTalk4 = 0x036B,
            BattleTalk8 = 0x0267,
            MapUpdate = 0x0348,
            MapUpdate4 = 0x019B,
            MapUpdate8 = 0x030A,
            MapUpdate16 = 0x02AE,
            MapUpdate32 = 0x01C0,
            MapUpdate64 = 0x035C,
            MapUpdate128 = 0x00ED,
            BalloonTalk2 = 0x01B6,
            BalloonTalk4 = 0x01F7,
            BalloonTalk8 = 0x033F,
            WeatherChange = 0x0172,
            PlayerTitleList = 0x0214,
            Discovery = 0x02DC,
            EorzeaTimeOffset = 0x00E6,
            EquipDisplayFlags = 0x0191,
            NpcYell = 0x00C9,
            FateInfo = 0x018C,
            LandSetInitialize = 0x02DA,
            LandUpdate = 0x0085,
            YardObjectSpawn = 0x02CC,
            HousingIndoorInitialize = 0x0329,
            LandAvailability = 0x0248,
            LandPriceUpdate = 0x02FE,
            LandInfoSign = 0x00D0,
            LandRename = 0x0268,
            HousingEstateGreeting = 0x0221,
            HousingUpdateLandFlagsSlot = 0x03CE,
            HousingLandFlags = 0x0255,
            HousingShowEstateGuestAccess = 0x007C,
            HousingObjectInitialize = 0x017C,
            HousingInternalObjectSpawn = 0x02E0,
            HousingWardInfo = 0x0190,
            HousingObjectMove = 0x032D,
            HousingObjectDye = 0x021A,
            SharedEstateSettingsResponse = 0x0207,
            DailyQuests = 0x00A8,
            DailyQuestRepeatFlags = 0x0272,
            LandUpdateHouseName = 0x006A,
            AirshipTimers = 0x03AF,
            PlaceMarker = 0x008A,
            WaymarkPreset = 0x00B0, // FFXIVOpcodes: PlaceFieldMarkerPreset, machina: PresetWaymark
            Waymark = 0x02F8, // FFXIVOpcodes: PlaceFieldMarker
            UnMount = 0x024A,
            CeremonySetActorAppearance = 0x02C4,
            AirshipStatusList = 0x00B5,
            AirshipStatus = 0x013D,
            AirshipExplorationResult = 0x0181,
            SubmarineStatusList = 0x015F,
            SubmarineProgressionStatus = 0x03C4,
            SubmarineExplorationResult = 0x02EF,
            SubmarineTimers = 0x03AC,
            PrepareZoning = 0x018A,
            ActorGauge = 0x024F,
            CharaVisualEffect = 0x02F9,
            LandSetMap = 0x024D,
            Fall = 0x00FD,
            PlayMotionSync = 0x03D9,
            CEDirector = 0x028E,
            IslandWorkshopSupplyDemand = 0x0364,

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
