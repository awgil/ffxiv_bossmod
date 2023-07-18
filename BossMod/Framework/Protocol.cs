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
            Ping = 0x0349,
            Init = 0x03BD,
            Logout = 0x0132,
            CFCancel = 0x00CC,
            CFDutyInfo = 0x0105,
            CFNotify = 0x0126,
            CFPreferredRole = 0x0211,
            CrossWorldLinkshellList = 0x011A,
            FellowshipList = 0x023A,
            Playtime = 0x02E5,
            CFRegistered = 0x0260,
            Chat = 0x0336,
            RSVData = 0x03CA,
            RSFData = 0x01E5,
            SocialMessage = 0x0066,
            SocialMessage2 = 0x027F,
            SocialList = 0x0208,
            SocialRequestResponse = 0x02C8,
            ExamineSearchInfo = 0x006B,
            UpdateSearchInfo = 0x02CE,
            InitSearchInfo = 0x00F9,
            ExamineSearchComment = 0x00CE,
            ServerNoticeShort = 0x029D,
            ServerNotice = 0x0194,
            SetOnlineStatus = 0x021D,
            LogMessage = 0x016B,
            Countdown = 0x0351,
            CountdownCancel = 0x00C8,
            PartyMessage = 0x013E,
            PlayerAddedToBlacklist = 0x0305,
            PlayerRemovedFromBlacklist = 0x0160,
            BlackList = 0x0085,
            MailDeleteRequest = 0x02F0,
            MarketBoardItemListingCount = 0x026B,
            MarketBoardItemListing = 0x00EB,
            MarketBoardPurchase = 0x017B,
            MarketBoardItemListingHistory = 0x03CB,
            RetainerSaleHistory = 0x0111,
            MarketBoardSearchResult = 0x02FD,
            FreeCompanyInfo = 0x0231,
            ExamineFreeCompanyInfo = 0x0202,
            FreeCompanyDialog = 0x023E,
            StatusEffectList = 0x03CE,
            StatusEffectListEureka = 0x0198,
            StatusEffectListBozja = 0x02B7,
            StatusEffectListDouble = 0x02EB,
            EffectResult1 = 0x00D2,
            EffectResult4 = 0x034C,
            EffectResult8 = 0x024D,
            EffectResult16 = 0x0376,
            EffectResultBasic1 = 0x034F,
            EffectResultBasic4 = 0x02DD,
            EffectResultBasic8 = 0x036E,
            EffectResultBasic16 = 0x0165,
            EffectResultBasic32 = 0x01E4,
            EffectResultBasic64 = 0x01DB,
            ActorControl = 0x0088,
            ActorControlSelf = 0x02F7,
            ActorControlTarget = 0x00A1,
            UpdateHpMpTp = 0x02FA,
            ActionEffect1 = 0x01E1,
            ActionEffect8 = 0x0181,
            ActionEffect16 = 0x0365,
            ActionEffect24 = 0x02EC,
            ActionEffect32 = 0x0103,
            StatusEffectListPlayer = 0x00EE,
            UpdateRecastTimes = 0x0244,
            UpdateAllianceNormal = 0x0315,
            UpdateAllianceSmall = 0x01E6,
            UpdatePartyMemberPositions = 0x01EA,
            UpdateAllianceNormalMemberPositions = 0x01D5,
            UpdateAllianceSmallMemberPositions = 0x0242,
            GCAffiliation = 0x01B9,
            SpawnPlayer = 0x02D3,
            SpawnNPC = 0x0179,
            SpawnBoss = 0x03D6,
            DespawnCharacter = 0x03A6,
            ActorMove = 0x01F1,
            Transfer = 0x018D,
            ActorSetPos = 0x0196,
            ActorCast = 0x0330,
            PlayerUpdateLook = 0x0335,
            UpdateParty = 0x036D,
            InitZone = 0x00D8,
            ApplyIDScramble = 0x007A,
            UpdateHate = 0x008F,
            UpdateHater = 0x00E3,
            SpawnObject = 0x0240,
            DespawnObject = 0x02E1,
            UpdateClassInfo = 0x03A5,
            UpdateClassInfoEureka = 0x00AD,
            UpdateClassInfoBozja = 0x0378,
            PlayerSetup = 0x02C5,
            PlayerStats = 0x030C,
            FirstAttack = 0x00DE,
            PlayerStateFlags = 0x0127,
            PlayerClassInfo = 0x01A2,
            ModelEquip = 0x009C,
            Examine = 0x00ED,
            CharaNameReq = 0x01C5,
            RetainerInformation = 0x02F8,
            ItemMarketBoardInfo = 0x0143,
            ItemInfo = 0x0333,
            ContainerInfo = 0x0343,
            InventoryTransactionFinish = 0x02EE,
            InventoryTransaction = 0x01D2,
            CurrencyCrystalInfo = 0x026A,
            InventoryActionAck = 0x00BD,
            UpdateInventorySlot = 0x013F,
            OpenTreasure = 0x02C7,
            LootMessage = 0x014B,
            CreateTreasure = 0x00D4,
            TreasureFadeOut = 0x0387,
            HuntingLogEntry = 0x015A,
            EventPlay = 0x014A,
            EventPlay4 = 0x01A5,
            EventPlay8 = 0x03B7,
            EventPlay16 = 0x03C4,
            EventPlay32 = 0x0108,
            EventPlay64 = 0x03E4,
            EventPlay128 = 0x0320,
            EventPlay255 = 0x0151,
            EventStart = 0x0133,
            EventFinish = 0x0125,
            EventContinue = 0x011D,
            ResultDialog = 0x00C2,
            DesynthResult = 0x01DF,
            QuestActiveList = 0x010C,
            QuestUpdate = 0x0227,
            QuestCompleteList = 0x0239,
            QuestFinish = 0x024F,
            MSQTrackerComplete = 0x0232,
            QuestTracker = 0x0192,
            Mount = 0x015E,
            DirectorVars = 0x027E,
            ContentDirectorSync = 0x0174,
            EnvControl = 0x029F,
            SystemLogMessage1 = 0x0100,
            SystemLogMessage2 = 0x01BB,
            SystemLogMessage4 = 0x01D8,
            SystemLogMessage8 = 0x00A9,
            SystemLogMessage16 = 0x032B,
            BattleTalk2 = 0x0255,
            BattleTalk4 = 0x03C5,
            BattleTalk8 = 0x0235,
            MapUpdate = 0x02F5,
            MapUpdate4 = 0x0185,
            MapUpdate8 = 0x032E,
            MapUpdate16 = 0x0329,
            MapUpdate32 = 0x011C,
            MapUpdate64 = 0x02BE,
            MapUpdate128 = 0x01F8,
            BalloonTalk2 = 0x03D9,
            BalloonTalk4 = 0x00E5,
            BalloonTalk8 = 0x00C6,
            WeatherChange = 0x00F0,
            PlayerTitleList = 0x0144,
            Discovery = 0x01A4,
            EorzeaTimeOffset = 0x018F,
            EquipDisplayFlags = 0x0069,
            NpcYell = 0x02AB,
            FateInfo = 0x020D,
            LandSetInitialize = 0x03D2,
            LandUpdate = 0x0101,
            YardObjectSpawn = 0x02BA,
            HousingIndoorInitialize = 0x02FE,
            LandAvailability = 0x0273,
            LandPriceUpdate = 0x01B4,
            LandInfoSign = 0x013D,
            LandRename = 0x027B,
            HousingEstateGreeting = 0x01C9,
            HousingUpdateLandFlagsSlot = 0x034B,
            HousingLandFlags = 0x012B,
            HousingShowEstateGuestAccess = 0x00FC,
            HousingObjectInitialize = 0x019C,
            HousingInternalObjectSpawn = 0x01A0,
            HousingWardInfo = 0x0284,
            HousingObjectMove = 0x00AA,
            HousingObjectDye = 0x033D,
            SharedEstateSettingsResponse = 0x032C,
            DailyQuests = 0x0301,
            DailyQuestRepeatFlags = 0x036A,
            LandUpdateHouseName = 0x02D2,
            AirshipTimers = 0x0075,
            PlaceMarker = 0x03B5,
            WaymarkPreset = 0x00D3,
            Waymark = 0x0172,
            UnMount = 0x00B1,
            CeremonySetActorAppearance = 0x02A3,
            AirshipStatusList = 0x0304,
            AirshipStatus = 0x01BC,
            AirshipExplorationResult = 0x022D,
            SubmarineStatusList = 0x034A,
            SubmarineProgressionStatus = 0x02C0,
            SubmarineExplorationResult = 0x00CD,
            SubmarineTimers = 0x02C1,
            PrepareZoning = 0x03A8,
            ActorGauge = 0x0080,
            CharaVisualEffect = 0x0252,
            LandSetMap = 0x036F,
            Fall = 0x01A6,
            PlayMotionSync = 0x012C,
            CEDirector = 0x0175,
            IslandWorkshopSupplyDemand = 0x0359,

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
