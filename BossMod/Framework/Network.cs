using Dalamud.Game.Network;
using System;
using System.Runtime.InteropServices;

namespace BossMod
{
    // taken from Machina
    class Protocol
    {
        public enum Opcode
        {
            StatusEffectList = 0x188,
            StatusEffectList2 = 0x293,
            StatusEffectList3 = 0x353,
            BossStatusEffectList = 0x38f,
            Ability1 = 0x33e,
            Ability8 = 0x1f4,
            Ability16 = 0x1fa,
            Ability24 = 0x300,
            Ability32 = 0x3cd,
            ActorCast = 0x307,
            EffectResult = 0x203,
            EffectResultBasic = 0x330,
            ActorControl = 0x2cf,
            ActorControlSelf = 0x96,
            ActorControlTarget = 0x272,
            UpdateHpMpTp = 0xf4,
            PlayerSpawn = 0x1e8,
            NpcSpawn = 0x1d2,
            NpcSpawn2 = 0x270,
            ActorMove = 0xdb,
            ActorSetPos = 0x81,
            ActorGauge = 0x22d,
            PresetWaymark = 0x67,
            Waymark = 0xfd,
            SystemLogMessage = 0x27a,

            // 0x1fd == EventObjSpawn? for stuff like exit points, etc.
            // 0xbf == HideShowBackground???
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

        public enum Server_ActionEffectDisplayType : byte
        {
            HideActionName = 0,
            ShowActionName = 1,
            ShowItemName = 2,
            MountName = 0x0d
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffectHeader
        {
            public uint animationTargetId;  // who the animation targets
            public uint unknown;
            public uint actionId; // what the casting player casts, shown in battle log / ui
            public uint globalEffectCounter;
            public float animationLockTime;
            public uint SomeTargetID;
            public ushort hiddenAnimation; // 0 = show animation, otherwise hide animation; dissector calls this "sourceSequence"
            public ushort rotation;
            public ushort actionAnimationId;
            public byte variation; // animation
            public Server_ActionEffectDisplayType effectDisplayType; // is this also item id / mount id?
            public byte unknown20;
            public byte effectCount;
            public ushort padding21;
            public ushort padding22;
            public ushort padding23;
            public ushort padding24;
        }

        public enum Server_ActionEffectType : byte
        {
            Nothing = 0,
            Miss = 1,
            FullResist = 2,
            Damage = 3,
            Heal = 4,
            BlockedDamage = 5,
            ParriedDamage = 6,
            Invulnerable = 7,
            NoEffectText = 8,
            Unknown_9 = 9,
            MpLoss = 10, // 0x0A
            MpGain = 11, // 0x0B
            TpLoss = 12, // 0x0C
            TpGain = 13, // 0x0D
            ApplyStatusEffectTarget = 14, // 0x0E - dissector calls this "GpGain"
            ApplyStatusEffectSource = 15, // 0x0F
            RecoveredFromStatusEffect = 16, // 0x10
            LoseStatusEffectTarget = 17, // 0x11
            LoseStatusEffectSource = 18, // 0x12
            Unknown_13 = 19, // 0x13
            StatusNoEffect = 20, // 0x14
            ThreatPosition = 24, // 0x18
            EnmityAmountUp = 25, // 0x19
            EnmityAmountDown = 26, // 0x1A
            StartActionCombo = 27, // 0x1B
            Unknown_1d = 29, // 0x1D
            Unknown_20 = 32, // 0x20 -- this looks like knockback?.. p1s, p2s
            Knockback = 33, // 0x21
            Unknown_22 = 34, // 0x22
            Unknown_27 = 39, // 0x27
            Mount = 40, // 0x28
            unknown_30 = 48, // 0x30
            unknown_31 = 49, // 0x31
            Unknown_32 = 50, // 0x32
            Unknown_33 = 51, // 0x33
            FullResistStatus = 52, // 0x34
            Unknown_37 = 55, // 0x37
            Unknown_38 = 56, // 0x38
            Unknown_39 = 57, // 0x39
            VFX = 59, // 0x3B
            Gauge = 60, // 0x3C
            Unknown_3d = 61, // 0x3D
            Unknown_40 = 64, // 0x40
            Unknown_42 = 66, // 0x42
            Unknown_46 = 70, // 0x46
            Unknown_47 = 71, // 0x47
            Unknown_48 = 72, // 0x48
            Unknown_49 = 73, // 0x49
            Partial_Invulnerable = 74, // 0x4A
            Interrupt = 75, // 0x4B
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect_EffectEntry
        {
            public Server_ActionEffectType effectType;
            public byte hitSeverity;
            public byte param;
            public byte bonusPercent;
            public byte valueMultiplier;
            public byte flag;
            public ushort value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect1
        {
            public Server_ActionEffectHeader Header;
            public fixed uint Effects[2 * 8]; // Server_ActionEffect_EffectEntry[8]
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[1]; // dissector calls hi-word "effect flags"
            public uint padding5;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect8
        {
            public Server_ActionEffectHeader Header;
            public fixed uint Effects[2 * 8 * 8];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[8];
            public uint effectflags1;
            public ushort effectflags2;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect16
        {
            public Server_ActionEffectHeader Header;
            public fixed uint Effects[2 * 8 * 16];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[16];
            public uint effectflags1;
            public ushort effectflags2;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect24
        {
            public Server_ActionEffectHeader Header;
            public fixed uint Effects[2 * 8 * 24];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[24];
            public uint effectflags1;
            public ushort effectflags2;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_ActionEffect32
        {
            public Server_ActionEffectHeader Header;
            public fixed uint Effects[2 * 8 * 32];
            public ushort padding3;
            public uint padding4;
            public fixed ulong TargetID[32];
            public uint effectflags1;
            public ushort effectflags2;
            public ushort padding5;
            public uint padding6;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_ActorCast
        {
            public ushort ActionID;
            public byte SkillType; // type = FFXIVClientStructs.FFXIV.Client.Game.ActionType
            public byte Unknown;
            public uint Unknown1; // also action ID; dissector calls it ItemId
            public float CastTime;
            public uint TargetID;
            public float Rotation; // in radians
            public uint Unknown2;
            public ushort PosX;
            public ushort PosY;
            public ushort PosZ;
            public ushort Unknown3;
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
            CancelAbility = 15, // dissector calls it CastInterrupt
            Cooldown = 17, // dissector calls it ActionStart
            GainEffect = 20,
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
            SetTarget = 50, // from dissector
            Targetable = 54, // dissector calls it ToggleNameHidden
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
            ActionRejected = 700, // from XivAlexander
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
            public uint param1; // first byte is classjobid
            public uint param2;
            public uint param3;
            public uint param4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Server_EffectResultEntry
        {
            public byte EffectIndex;
            public byte unknown1;
            public ushort EffectID;
            public ushort unknown2;
            public ushort unknown3;
            public float duration;
            public uint SourceActorID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_EffectResult
        {
            public uint Unknown1;
            public uint RelatedActionSequence;
            public uint ActorID;
            public uint CurrentHP;
            public uint MaxHP;
            public ushort CurrentMP;
            public ushort Unknown3;
            //        public UInt16 MaxMP;
            //      public UInt16 Unknown4;
            public byte DamageShield;
            public byte EffectCount;
            public ushort Unknown6;
            public fixed byte Effects[4 * 4 * 4];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_EffectResultBasic
        {
            public uint Unknown1;
            public uint RelatedActionSequence;
            public uint ActorID;
            public uint CurrentHP;
            public uint Unknown2;
            public ushort Unknown3;
            public ushort Unknown4;
        }

        [Flags]
        public enum WaymarkType : byte
        {
            A = 0x1,
            B = 0x2,
            C = 0x4,
            D = 0x8,
            One = 0x10,
            Two = 0x20,
            Three = 0x40,
            Four = 0x80,
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_Waymark
        {
            public enum WaymarkStatus : byte
            {
                Off = 0,
                On = 1
            };
            public WaymarkType Waymark;
            public WaymarkStatus Status;
            public ushort unknown;
            public int PosX;
            public int PosZ;// To calculate 'float' coords from these you cast them to float and then divide by 1000.0
            public int PosY;
        }

        // Thanks to Discord user Wintermute for decoding this
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Server_PresetWaymark
        {
            public WaymarkType WaymarkType;
            public byte Unknown1;
            public short Unknown2;
            public fixed int PosX[8];// Xints[0] has X of waymark A, Xints[1] X of B, etc.
            public fixed int PosZ[8];// To calculate 'float' coords from these you cast them to float and then divide by 1000.0
            public fixed int PosY[8];
        }
    }

    class Network : IDisposable
    {
        public Network()
        {
            Service.GameNetwork.NetworkMessage += HandleMessage;
        }

        public void Dispose()
        {
            Service.GameNetwork.NetworkMessage -= HandleMessage;
        }

        private unsafe void HandleMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (direction == NetworkMessageDirection.ZoneDown)
            {
                // server->client
                var header = (Protocol.Server_IPCHeader*)(dataPtr - 0x10);
                Service.Log($"[Network] Server message {(Protocol.Opcode)opCode} -> {Utils.ObjectString(targetActorId)} (seq={header->Epoch})");
                switch ((Protocol.Opcode)opCode)
                {
                    case Protocol.Opcode.Ability1:
                        {
                            var p = (Protocol.Server_ActionEffect1*)dataPtr;
                            HandleActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 1, 0, 0);
                            break;
                        }
                    case Protocol.Opcode.Ability8:
                        {
                            var p = (Protocol.Server_ActionEffect8*)dataPtr;
                            HandleActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 8, p->effectflags1, p->effectflags2);
                            break;
                        }
                    case Protocol.Opcode.Ability16:
                        {
                            var p = (Protocol.Server_ActionEffect16*)dataPtr;
                            HandleActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 16, p->effectflags1, p->effectflags2);
                            break;
                        }
                    case Protocol.Opcode.Ability24:
                        {
                            var p = (Protocol.Server_ActionEffect24*)dataPtr;
                            HandleActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 24, p->effectflags1, p->effectflags2);
                            break;
                        }
                    case Protocol.Opcode.Ability32:
                        {
                            var p = (Protocol.Server_ActionEffect32*)dataPtr;
                            HandleActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 32, p->effectflags1, p->effectflags2);
                            break;
                        }
                    case Protocol.Opcode.ActorCast:
                        {
                            var p = (Protocol.Server_ActorCast*)dataPtr;
                            Service.Log($"[Network] - AID={Utils.ActionString(p->ActionID)}, type={p->SkillType}, target={Utils.ObjectString(p->TargetID)}, time={p->CastTime:f2}, rot={p->Rotation:f2}, x={p->PosX}, y={p->PosY}, z={p->PosZ}, u={p->Unknown:X2}, u1={Utils.ActionString(p->Unknown1)}, u2={Utils.ObjectString(p->Unknown2)}, u3={p->Unknown3:X4}");
                            break;
                        }
                    case Protocol.Opcode.ActorControl:
                        {
                            var p = (Protocol.Server_ActorControl*)dataPtr;
                            Service.Log($"[Network] - cat={p->category}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8}, unk={p->unk0:X4}");
                            switch (p->category)
                            {
                                case Protocol.Server_ActorControlCategory.CancelAbility: // note: some successful boss casts have this message on completion, seen param1=param4=0, param2=1; param1 is related to cast time?..
                                    Service.Log($"[Network] -- cancelled {Utils.ActionString(p->param3)}, interrupted={p->param4 == 1}");
                                    break;
                                case Protocol.Server_ActorControlCategory.GainEffect: // gain status effect, seen param2=param3=param4=0
                                    Service.Log($"[Network] -- gained {Utils.StatusString(p->param1)}");
                                    break;
                                case Protocol.Server_ActorControlCategory.LoseEffect: // lose status effect, seen param2=param4=0, param3=invalid-oid
                                    Service.Log($"[Network] -- lost {Utils.StatusString(p->param1)}");
                                    break;
                            }
                            break;
                        }
                    case Protocol.Opcode.ActorControlSelf:
                        {
                            var p = (Protocol.Server_ActorControlSelf*)dataPtr;
                            Service.Log($"[Network] - cat={p->category}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8} {p->param6:X8} {p->param7:X8}, unk={p->unk0:X4}");
                            break;
                        }
                    case Protocol.Opcode.ActorControlTarget:
                        {
                            var p = (Protocol.Server_ActorControlTarget*)dataPtr;
                            Service.Log($"[Network] - cat={p->category}, target={Utils.ObjectString(p->TargetID)}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8}, unk={p->unk0:X4} {p->unk1:X8}");
                            break;
                        }
                    case Protocol.Opcode.ActorGauge:
                        {
                            var p = (Protocol.Server_ActorGauge*)dataPtr;
                            Service.Log($"[Network] - params={p->param1:X} {p->param2:X} {p->param3:X} {p->param4:X}");
                            break;
                        }
                    case Protocol.Opcode.EffectResult:
                        {
                            var p = (Protocol.Server_EffectResult*)dataPtr;
                            Service.Log($"[Network] - seq={p->RelatedActionSequence}, actor={Utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}/{p->MaxHP}, mp={p->CurrentMP}, shield={p->DamageShield}, u={p->Unknown1:X8} {p->Unknown3:X4} {p->Unknown6:X4}");
                            var cnt = Math.Min(4, (int)p->EffectCount);
                            for (int i = 0; i < cnt; ++i)
                            {
                                var eff = ((Protocol.Server_EffectResultEntry*)p->Effects) + i;
                                Service.Log($"[Network] -- idx={eff->EffectIndex}, id={Utils.StatusString(eff->EffectID)}, dur={eff->duration:f2}, src={Utils.ObjectString(eff->SourceActorID)}, u={eff->unknown1:X2} {eff->unknown2:X4} {eff->unknown3:X4}");
                            }
                            break;
                        }
                    case Protocol.Opcode.EffectResultBasic:
                        {
                            var p = (Protocol.Server_EffectResultBasic*)dataPtr;
                            Service.Log($"[Network] - seq={p->RelatedActionSequence}, actor={Utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}, u={p->Unknown1:X8} {p->Unknown2:X8} {p->Unknown3:X4} {p->Unknown4:X4}");
                            break;
                        }
                    case Protocol.Opcode.Waymark:
                        {
                            var p = (Protocol.Server_Waymark*)dataPtr;
                            Service.Log($"[Network] - {p->Waymark}: {p->Status} at {p->PosX} {p->PosY} {p->PosZ}");
                            break;
                        }
                    case Protocol.Opcode.PresetWaymark:
                        {
                            var p = (Protocol.Server_PresetWaymark*)dataPtr;
                            for (int i = 0; i < 8; ++i)
                            {
                                var t = (Protocol.WaymarkType)(1 << i);
                                Service.Log($"[Network] - {t}: {((p->WaymarkType & t) == t ? Protocol.Server_Waymark.WaymarkStatus.On : Protocol.Server_Waymark.WaymarkStatus.Off)} at {p->PosX[i]} {p->PosY[i]} {p->PosZ[i]}");
                            }
                            break;
                        }
                    case (Protocol.Opcode)0xBF:
                        {
                            var p = (uint*)dataPtr;
                            Service.Log($"[Network] - {p[0]:X8} {p[1]:X8} {p[2]:X8} {p[3]:X8}");
                            break;
                        }
                }
            }
            else
            {
                // client->server
                //Service.Log($"[Network] Client message {opCode}");
            }
        }

        private unsafe void HandleActionEffect(Protocol.Server_ActionEffectHeader* data, Protocol.Server_ActionEffect_EffectEntry* effects, ulong* targetIDs, uint maxTargets, uint flags1, ushort flags2)
        {
            // rotation: 0 -> -180, 65535 -> +180
            float rot = (data->rotation / 65535.0f * 360.0f) - 180.0f;
            Service.Log($"[Network] - AID={Utils.ActionString(data->actionId)}, animAID={Utils.ActionString(data->actionAnimationId)}, animTarget={Utils.ObjectString(data->animationTargetId)}, animLock={data->animationLockTime:f2}, animHidden={data->hiddenAnimation}, type={data->effectDisplayType}, someTarget={Utils.ObjectString(data->SomeTargetID)}, rot={rot:f0}, var={data->variation}, cntr={data->globalEffectCounter}, flags={flags1:X8} {flags2:X4}, u={data->unknown:X8} {data->unknown20:X2} {data->padding21:X4}");
            var targets = Math.Min(data->effectCount, maxTargets);
            for (int i = 0; i < targets; ++i)
            {
                uint targetId = (uint)targetIDs[i];
                if (targetId == 0)
                    continue;

                Service.Log($"[Network] -- target {i} == {Utils.ObjectString(targetId)}, hiword = {targetIDs[i] >> 32:X8}");
                for (int j = 0; j < 8; ++j)
                {
                    Protocol.Server_ActionEffect_EffectEntry* eff = effects + (i * 8) + j;
                    if (eff->effectType == Protocol.Server_ActionEffectType.Nothing)
                        continue;

                    Service.Log($"[Network] --- effect {j} == {eff->effectType}, params={eff->hitSeverity:X2} {eff->param:X2} {eff->bonusPercent:X2} {eff->valueMultiplier:X2} {eff->flag:X2} {eff->value:X4}");
                }
            }
        }
    }
}
