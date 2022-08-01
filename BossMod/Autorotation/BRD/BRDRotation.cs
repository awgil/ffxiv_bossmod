//using System.Text;

//namespace BossMod
//{
//    public static class BRDRotation
//    {
//        public enum AID : uint
//        {
//            None = 0,

//            // single target GCDs
//            HeavyShot = 97,
//            BurstShot = 16495,
//            StraightShot = 98,
//            RefulgentArrow = 7409,
//            VenomousBite = 100,
//            CausticBite = 7406,
//            Windbite = 113,
//            Stormbite = 7407,
//            IronJaws = 3560,
//            ApexArrow = 16496,
//            BlastArrow = 25784,

//            // aoe GCDs
//            QuickNock = 106,
//            Ladonsbite = 25783,
//            Shadowbite = 16494,

//            // oGCDs
//            Bloodletter = 110,
//            RainOfDeath = 117,
//            PitchPerfect = 7404,
//            EmpyrealArrow = 3558,
//            Sidewinder = 3562,

//            // offsensive CDs
//            RagingStrikes = 101,
//            Barrage = 107,
//            MagesBallad = 114,
//            ArmysPaeon = 116,
//            WanderersMinuet = 3559,
//            BattleVoice = 118,
//            RadiantFinale = 25785,

//            // defensive CDs
//            SecondWind = 7541,
//            Troubadour = 7405,
//            NaturesMinne = 7408,
//            ArmsLength = 7548,

//            // misc
//            Peloton = 7557,
//            LegGraze = 7554,
//            FootGraze = 7553,
//            HeadGraze = 7551,
//            RepellingShot = 112,
//            WardensPaean = 3561,
//        }
//        public static ActionID IDStatPotion = new(ActionType.Item, 1000000); // hq grade 6 tincture of ???

//        public enum SID : uint
//        {
//            None = 0,
//            StraightShotReady = 122,
//            VenomousBite = 124,
//            Windbite = 129,
//            RagingStrikes = 125,
//            Barrage = 128,
//            Peloton = 1199,
//            CausticBite = 1200,
//            Stormbite = 1201,
//        }

//        public static int BloodletterCDGroup = CommonRotation.SpellCDGroup(AID.Bloodletter); // shares with RainOfDeath
//        public static int PitchPerfectCDGroup = CommonRotation.SpellCDGroup(AID.PitchPerfect);
//        public static int EmpyrealArrowCDGroup = CommonRotation.SpellCDGroup(AID.EmpyrealArrow);
//        public static int SidewinderCDGroup = CommonRotation.SpellCDGroup(AID.Sidewinder);
//        public static int RagingStrikesCDGroup = CommonRotation.SpellCDGroup(AID.RagingStrikes);
//        public static int BarrageCDGroup = CommonRotation.SpellCDGroup(AID.Barrage);
//        public static int MagesBalladCDGroup = CommonRotation.SpellCDGroup(AID.MagesBallad);
//        public static int ArmysPaeonCDGroup = CommonRotation.SpellCDGroup(AID.ArmysPaeon);
//        public static int WanderersMinuetCDGroup = CommonRotation.SpellCDGroup(AID.WanderersMinuet);
//        public static int BattleVoiceCDGroup = CommonRotation.SpellCDGroup(AID.BattleVoice);
//        public static int RadiantFinaleCDGroup = CommonRotation.SpellCDGroup(AID.RadiantFinale);
//        public static int SecondWindCDGroup = CommonRotation.SpellCDGroup(AID.SecondWind);
//        public static int TroubadourCDGroup = CommonRotation.SpellCDGroup(AID.Troubadour);
//        public static int NaturesMinneCDGroup = CommonRotation.SpellCDGroup(AID.NaturesMinne);
//        public static int ArmsLengthCDGroup = CommonRotation.SpellCDGroup(AID.ArmsLength);
//        public static int PelotonCDGroup = CommonRotation.SpellCDGroup(AID.Peloton);
//        public static int LegGrazeCDGroup = CommonRotation.SpellCDGroup(AID.LegGraze);
//        public static int FootGrazeCDGroup = CommonRotation.SpellCDGroup(AID.FootGraze);
//        public static int HeadGrazeCDGroup = CommonRotation.SpellCDGroup(AID.HeadGraze);
//        public static int RepellingShotCDGroup = CommonRotation.SpellCDGroup(AID.RepellingShot);
//        public static int WardensPaeanCDGroup = CommonRotation.SpellCDGroup(AID.WardensPaean);

//        // full state needed for determining next action
//        public class State : CommonRotation.PlayerState
//        {
//            public float StraightShotLeft;
//            public float RagingStrikesLeft;
//            public float BarrageLeft;
//            public float PelotonLeft; // 30 max
//            public float TargetVenomousLeft;
//            public float TargetWindbiteLeft;

//            public float BloodletterCD => Cooldowns[BloodletterCDGroup]; // 45 max, >30 if 0 charges ready, >15 if 1 charge ready, >0 if 2 charges ready, ==0 if 3 charges are ready
//            public float PitchPerfectCD => Cooldowns[PitchPerfectCDGroup];
//            public float EmpyrealArrowCD => Cooldowns[EmpyrealArrowCDGroup];
//            public float SidewinderCD => Cooldowns[SidewinderCDGroup];
//            public float RagingStrikesCD => Cooldowns[RagingStrikesCDGroup];
//            public float BarrageCD => Cooldowns[BarrageCDGroup];
//            public float MagesBalladCD => Cooldowns[MagesBalladCDGroup];
//            public float ArmysPaeonCD => Cooldowns[ArmysPaeonCDGroup];
//            public float WanderersMinuetCD => Cooldowns[WanderersMinuetCDGroup];
//            public float BattleVoiceCD => Cooldowns[BattleVoiceCDGroup];
//            public float RadiantFinaleCD => Cooldowns[RadiantFinaleCDGroup];
//            public float SecondWindCD => Cooldowns[SecondWindCDGroup];
//            public float TroubadourCD => Cooldowns[TroubadourCDGroup];
//            public float NaturesMinneCD => Cooldowns[NaturesMinneCDGroup];
//            public float ArmsLengthCD => Cooldowns[ArmsLengthCDGroup]; // 120 max, 0 if ready
//            public float PelotonCD => Cooldowns[PelotonCDGroup];
//            public float LegGrazeCD => Cooldowns[LegGrazeCDGroup];
//            public float FootGrazeCD => Cooldowns[FootGrazeCDGroup];
//            public float HeadGrazeCD => Cooldowns[HeadGrazeCDGroup];
//            public float RepellingShotCD => Cooldowns[RepellingShotCDGroup];
//            public float WardensPaeanCD => Cooldowns[WardensPaeanCDGroup];

//            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
//            public bool UnlockedStraightShot => Level >= 2;
//            public bool UnlockedRagingStrikes => Level >= 4;
//            public bool UnlockedVenomousBite => Level >= 6;
//            public bool UnlockedLegGraze => Level >= 6;
//            public bool UnlockedSecondWind => Level >= 8;
//            public bool UnlockedFootGraze => Level >= 10;
//            public bool UnlockedBloodletter => Level >= 12;
//            public bool UnlockedRepellingShot => Level >= 15; // quest-locked
//            public bool UnlockedQuickNock => Level >= 18;
//            public bool UnlockedPeloton => Level >= 20;
//            public bool UnlockedHeadGraze => Level >= 24;
//            public bool UnlockedWindbite => Level >= 30; // quest-locked
//            public bool UnlockedMagesBallad => Level >= 30; // quest-locked
//            public bool UnlockedArmsLength => Level >= 32;
//            public bool UnlockedWardensPaean => Level >= 35; // quest-locked
//            public bool UnlockedBarrage => Level >= 38;
//            public bool UnlockedArmysPaean => Level >= 40; // quest-locked
//            public bool UnlockedRainOfDeath => Level >= 45; // quest-locked
//            public bool UnlockedBattleVoice => Level >= 50; // quest-locked
//            public bool UnlockedWanderersMenuet => Level >= 52; // quest-locked
//            public bool UnlockedEmpyrealArrow => Level >= 54; // quest-locked
//            public bool UnlockedIronJaws => Level >= 56; // quest-locked
//            public bool UnlockedSidewinder => Level >= 60; // quest-locked
//            // TODO: L62+

//            public override string ToString()
//            {
//                return $"RB={RaidBuffsLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
//            }
//        }

//        // strategy configuration
//        public class Strategy : CommonRotation.Strategy
//        {
//            // cooldowns
//            public bool ExecuteArmsLength;
//            public bool ExecuteSecondWind;
//            public bool ExecuteHeadGraze;

//            public override string ToString()
//            {
//                var sb = new StringBuilder("SmartQueue:");
//                if (ExecuteArmsLength)
//                    sb.Append(" ArmsLength");
//                if (ExecuteSecondWind)
//                    sb.Append(" SecondWind");
//                if (ExecuteHeadGraze)
//                    sb.Append(" HeadGraze");
//                if (ExecuteSprint)
//                    sb.Append(" Sprint");
//                return sb.ToString();
//            }
//        }

//        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
//        {
//            // TODO: this is correct until L30
//            if (aoe)
//            {
//                return AID.QuickNock;
//            }
//            else
//            {
//                // 1. dots
//                if (state.UnlockedWindbite && state.TargetWindbiteLeft < state.GCD)
//                    return AID.Windbite;
//                if (state.UnlockedVenomousBite && state.TargetVenomousLeft < state.GCD)
//                    return AID.VenomousBite;

//                // 2. straight shot if possible
//                if (state.StraightShotLeft > state.GCD)
//                    return AID.StraightShot;

//                // 3. heavy shot
//                return AID.HeavyShot;
//            }
//        }

//        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe)
//        {
//            // 1. use cooldowns if requested in rough priority order
//            if (strategy.ExecuteArmsLength && state.UnlockedArmsLength && state.CanWeave(state.ArmsLengthCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.ArmsLength);
//            if (strategy.ExecuteSecondWind && state.UnlockedSecondWind && state.CanWeave(state.SecondWindCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.SecondWind);
//            if (strategy.ExecuteHeadGraze && state.UnlockedHeadGraze && state.CanWeave(state.HeadGrazeCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.HeadGraze);
//            if (strategy.ExecuteSprint && state.CanWeave(state.SprintCD, 0.6f, windowEnd))
//                return CommonDefinitions.IDSprint;

//            // TODO: this should be improved... correct for low levels
//            if (state.UnlockedRagingStrikes && state.CanWeave(state.RagingStrikesCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.RagingStrikes);
//            if (state.UnlockedBloodletter && state.CanWeave(state.BloodletterCD - 30, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Bloodletter);

//            // no suitable oGCDs...
//            return new();
//        }

//        public static ActionID GetNextBestAction(State state, Strategy strategy, bool aoe)
//        {
//            ActionID res = new();
//            if (state.CanDoubleWeave) // first ogcd slot
//                res = GetNextBestOGCD(state, strategy, state.DoubleWeaveWindowEnd, aoe);
//            if (!res && state.CanSingleWeave) // second/only ogcd slot
//                res = GetNextBestOGCD(state, strategy, state.GCD, aoe);
//            if (!res) // gcd
//                res = ActionID.MakeSpell(GetNextBestGCD(state, strategy, aoe));
//            return res;
//        }

//        // short string for supported action
//        public static string ActionShortString(ActionID action)
//        {
//            return action == CommonDefinitions.IDSprint ? "Sprint" : action == IDStatPotion ? "StatPotion" : ((AID)action.ID).ToString();
//        }
//    }
//}
