//using System.Text;

//namespace BossMod
//{
//    public static class MNKRotation
//    {
//        public enum AID : uint
//        {
//            None = 0,

//            // single target GCDs
//            Bootshine = 53,
//            DragonKick = 74,
//            TrueStrike = 54,
//            TwinSnakes = 61,
//            SnapPunch = 56,
//            Demolish = 66,
//            SixSidedStar = 16476,

//            // aoe GCDs
//            ArmOfTheDestroyer = 62,
//            ShadowOfTheDestroyer = 25767,
//            FourPointFury = 16473,
//            Rockbreaker = 70,

//            // masterful blitz variants
//            MasterfulBlitz = 25764,
//            ElixirField = 3545,
//            FlintStrike = 25882,
//            RisingPhoenix = 25768,
//            CelestialRevolution = 25765,
//            TornadoKick = 3543,
//            PhantomRush = 25769,

//            // oGCDs
//            SteelPeak = 25761,
//            ForbiddenChakra = 3547,
//            HowlingFist = 25763,
//            Enlightenment = 16474,

//            // offsensive CDs
//            PerfectBalance = 69,
//            RiddleOfFire = 7395,
//            Brotherhood = 7396,
//            RiddleOfWind = 25766,

//            // defensive CDs
//            SecondWind = 7541,
//            Mantra = 65,
//            RiddleOfEarth = 7394,
//            Bloodbath = 7542,
//            Feint = 7549,
//            ArmsLength = 7548,

//            // misc
//            Meditation = 3546,
//            TrueNorth = 7546,
//            Thunderclap = 25762,
//            FormShift = 4262,
//            Anatman = 16475,
//            LegSweep = 7863,
//        }
//        public static ActionID IDStatPotion = new(ActionType.Item, 1036109); // hq grade 6 tincture of strength

//        public enum SID : uint
//        {
//            None = 0,
//            OpoOpoForm = 107,
//            RaptorForm = 108,
//            CoeurlForm = 109,
//            DisciplinedFist = 3001,
//        }

//        public enum Form { None, OpoOpo, Raptor, Coeurl }

//        public static int ArmsLengthCDGroup = CommonRotation.SpellCDGroup(AID.ArmsLength);
//        public static int SecondWindCDGroup = CommonRotation.SpellCDGroup(AID.SecondWind);
//        public static int BloodbathCDGroup = CommonRotation.SpellCDGroup(AID.Bloodbath);
//        public static int LegSweepCDGroup = CommonRotation.SpellCDGroup(AID.LegSweep);

//        // full state needed for determining next action
//        public class State : CommonRotation.PlayerState
//        {
//            public int Chakra; // 0-5
//            public Form Form;
//            public float FormLeft; // 0 if no form, 30 max
//            public float DisciplinedFistLeft; // 15 max

//            public float ArmsLengthCD => Cooldowns[ArmsLengthCDGroup]; // 120 max, 0 if ready
//            public float SecondWindCD => Cooldowns[SecondWindCDGroup]; // 120 max, 0 if ready
//            public float BloodbathCD => Cooldowns[BloodbathCDGroup]; // 90 max, 0 if ready
//            public float LegSweepCD => Cooldowns[LegSweepCDGroup]; // 40 max, 0 if ready

//            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
//            public bool UnlockedTrueStrike => Level >= 4;
//            public bool UnlockedSnapPunch => Level >= 6;
//            public bool UnlockedSecondWind => Level >= 8;
//            public bool UnlockedLegSweep => Level >= 10;
//            public bool UnlockedBloodbath => Level >= 12;
//            public bool UnlockedSteelPeak => Level >= 15;
//            public bool UnlockedMeditation => Level >= 15; // quest-locked
//            public bool UnlockedTwinSnakes => Level >= 18;
//            public bool UnlockedFeint => Level >= 22;
//            public bool UnlockedArmOfTheDestroyer => Level >= 26;
//            public bool UnlockedDemolish => Level >= 30; // quest-locked
//            public bool UnlockedRockbreaker => Level >= 30; // quest-locked
//            public bool UnlockedArmsLength => Level >= 32;
//            public bool UnlockedThunderclap => Level >= 35; // quest-locked
//            public bool UnlockedHowlingFist => Level >= 40; // quest-locked
//            public bool UnlockedMantra => Level >= 42;
//            public bool UnlockedFourPointFury => Level >= 45; // quest-locked
//            public bool UnlockedTrueNorth => Level >= 50;
//            public bool UnlockedDragonKick => Level >= 50;
//            public bool UnlockedPerfectBalance => Level >= 50; // quest-locked
//            public bool UnlockedFormShift => Level >= 52; // quest-locked
//            public bool UnlockedForbiddenChakra => Level >= 54; // quest-locked
//            public bool UnlockedMasterfulBlitz => Level >= 60; // quest-locked
//            public bool UnlockedRiddleOfEarth => Level >= 64;
//            public bool UnlockedRiddleOfFire => Level >= 68;
//            public bool UnlockedBrotherhood => Level >= 70; // quest-locked
//            public bool UnlockedRiddleOfWind => Level >= 72;
//            public bool UnlockedEnlightenment => Level >= 74; // level 74 also gives passive, gives chakra when crit
//            public bool UnlockedAnatman => Level >= 78;
//            public bool UnlockedSixSidedStar => Level >= 80;
//            public bool UnlockedShadowOfTheDestroyer => Level >= 82;
//            public bool UnlockedEnhancedThunderclap => Level >= 84; // passive, third charge for Thunderclap
//            public bool UnlockedRisingPhoenix => Level >= 86;
//            public bool UnlockedEnhancedBrotherhood => Level >= 88; // passive, gives chakra for each GCD under Brotherhood buff
//            public bool UnlockedPhantomRush => Level >= 90;

//            public override string ToString()
//            {
//                return $"RB={RaidBuffsLeft:f1}, Chakra={Chakra}, Form={Form}/{FormLeft:f1}, DFist={DisciplinedFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
//            }
//        }

//        // strategy configuration
//        public class Strategy : CommonRotation.Strategy
//        {
//            // cooldowns
//            public bool ExecuteArmsLength;
//            public bool ExecuteSecondWind;
//            public bool ExecuteBloodbath;
//            public bool ExecuteLegSweep;

//            public override string ToString()
//            {
//                var sb = new StringBuilder("SmartQueue:");
//                if (ExecuteArmsLength)
//                    sb.Append(" ArmsLength");
//                if (ExecuteSecondWind)
//                    sb.Append(" SecondWind");
//                if (ExecuteBloodbath)
//                    sb.Append(" Bloodbath");
//                if (ExecuteLegSweep)
//                    sb.Append(" LegSweep");
//                if (ExecuteSprint)
//                    sb.Append(" Sprint");
//                return sb.ToString();
//            }
//        }

//        public static AID GetNextAOEComboAction(State state)
//        {
//            if (!state.UnlockedRockbreaker)
//                return AID.ArmOfTheDestroyer;

//            return state.Form switch
//            {
//                Form.Coeurl => AID.Rockbreaker,
//                Form.Raptor => state.UnlockedFourPointFury ? AID.FourPointFury : AID.TwinSnakes,
//                _ => state.UnlockedShadowOfTheDestroyer ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer,
//            };
//        }

//        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
//        {
//            if (aoe && state.UnlockedArmOfTheDestroyer)
//            {
//                // TODO: this is not right...
//                return AID.ArmOfTheDestroyer;
//            }
//            else
//            {
//                // TODO: this is kinda correct at least until L15
//                return state.Form switch
//                {
//                    Form.Coeurl => AID.SnapPunch,
//                    Form.Raptor => state.UnlockedTwinSnakes && state.DisciplinedFistLeft < 7 ? AID.TwinSnakes : AID.TrueStrike, // TODO: better threshold for debuff reapplication
//                    _ => AID.Bootshine
//                };
//            }
//        }

//        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe)
//        {
//            // 1. use cooldowns if requested in rough priority order
//            if (strategy.ExecuteArmsLength && state.UnlockedArmsLength && state.CanWeave(state.ArmsLengthCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.ArmsLength);
//            if (strategy.ExecuteSecondWind && state.UnlockedSecondWind && state.CanWeave(state.SecondWindCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.SecondWind);
//            if (strategy.ExecuteBloodbath && state.UnlockedBloodbath && state.CanWeave(state.BloodbathCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Bloodbath);
//            if (strategy.ExecuteLegSweep && state.UnlockedLegSweep && state.CanWeave(state.LegSweepCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.LegSweep);
//            if (strategy.ExecuteSprint && state.CanWeave(state.SprintCD, 0.6f, windowEnd))
//                return CommonDefinitions.IDSprint;

//            // 2. steel peek, if have chakra
//            if (state.UnlockedSteelPeak && state.Chakra == 5)
//                return ActionID.MakeSpell(AID.SteelPeak);

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
