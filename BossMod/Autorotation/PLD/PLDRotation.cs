//using System.Text;

//namespace BossMod
//{
//    public static class PLDRotation
//    {
//        public enum AID : uint
//        {
//            None = 0,

//            // single target GCDs
//            FastBlade = 9,
//            RiotBlade = 15,
//            RageOfHalone = 21,
//            GoringBlade = 3538,
//            RoyalAuthority = 3539,
//            HolySpirit = 7384,
//            Atonement = 16460,
//            Confiteor = 16459,
//            Expiacion = 25747,
//            BladeOfFaith = 25748,
//            BladeOfTruth = 25749,
//            BladeOfValor = 25750,

//            // aoe GCDs
//            TotalEclipse = 7381,
//            Prominence = 16457,
//            HolyCircle = 16458,

//            // oGCDs
//            SpiritsWithin = 29,
//            CircleOfScorn = 23,
//            Intervene = 16461,

//            // offsensive CDs
//            FightOrFlight = 20,
//            Requiescat = 7383,

//            // defensive CDs
//            Rampart = 7531,
//            Sheltron = 3542,
//            Sentinel = 17,
//            Cover = 27,
//            HolySheltron = 25746,
//            HallowedGround = 30,
//            Reprisal = 7535,
//            PassageOfArms = 7385,
//            DivineVeil = 3540,
//            Intervention = 7382,
//            ArmsLength = 7548,

//            // misc
//            Clemency = 3541,
//            ShieldBash = 16,
//            ShieldLob = 24,
//            IronWill = 28,
//            Provoke = 7533,
//            Shirk = 7537,
//            LowBlow = 7540,
//            Interject = 7538,
//        }
//        public static ActionID IDStatPotion = new(ActionType.Item, 1036109); // hq grade 6 tincture of strength

//        public enum SID : uint
//        {
//            None = 0,
//            FightOrFlight = 76,
//        }

//        public static int FightOrFlightCDGroup = CommonRotation.SpellCDGroup(AID.FightOrFlight);
//        public static int RampartCDGroup = CommonRotation.SpellCDGroup(AID.Rampart);
//        public static int ReprisalCDGroup = CommonRotation.SpellCDGroup(AID.Reprisal);
//        public static int ArmsLengthCDGroup = CommonRotation.SpellCDGroup(AID.ArmsLength);
//        public static int ProvokeCDGroup = CommonRotation.SpellCDGroup(AID.Provoke);
//        public static int ShirkCDGroup = CommonRotation.SpellCDGroup(AID.Shirk);
//        public static int LowBlowCDGroup = CommonRotation.SpellCDGroup(AID.LowBlow);
//        public static int InterjectCDGroup = CommonRotation.SpellCDGroup(AID.Interject);

//        // full state needed for determining next action
//        public class State : CommonRotation.PlayerState
//        {
//            public float FightOrFlightLeft; // 0 if buff not up, max 25

//            public float FightOrFlightCD => Cooldowns[FightOrFlightCDGroup]; // 60 max, 0 if ready
//            public float RampartCD => Cooldowns[RampartCDGroup]; // 90 max, 0 if ready
//            public float ReprisalCD => Cooldowns[ReprisalCDGroup]; // 60 max, 0 if ready
//            public float ArmsLengthCD => Cooldowns[ArmsLengthCDGroup]; // 120 max, 0 if ready
//            public float ProvokeCD => Cooldowns[ProvokeCDGroup]; // 30 max, 0 if ready
//            public float ShirkCD => Cooldowns[ShirkCDGroup]; // 120 max, 0 if ready
//            public float LowBlowCD => Cooldowns[LowBlowCDGroup]; // 25 max, 0 if ready
//            public float InterjectCD => Cooldowns[InterjectCDGroup]; // 30 max, 0 if ready

//            public AID ComboLastMove => (AID)ComboLastAction;

//            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
//            public bool UnlockedFightOrFlight => Level >= 2;
//            public bool UnlockedRiotBlade => Level >= 4;
//            public bool UnlockedTotalEclipse => Level >= 6;
//            public bool UnlockedRampart => Level >= 8;
//            public bool UnlockedShieldBash => Level >= 10;
//            public bool UnlockedIronWill => Level >= 10;
//            public bool UnlockedLowBlow => Level >= 12;
//            public bool UnlockedProvoke => Level >= 15;
//            public bool UnlockedShieldLob => Level >= 15; // quest-locked
//            public bool UnlockedInterject => Level >= 18;
//            public bool UnlockedReprisal => Level >= 22;
//            public bool UnlockedRageOfHalone => Level >= 26;
//            public bool UnlockedSpiritsWithin => Level >= 30; // quest-locked
//            public bool UnlockedArmsLength => Level >= 32;
//            public bool UnlockedSheltron => Level >= 35; // quest-locked
//            public bool UnlockedSentinel => Level >= 38;
//            public bool UnlockedProminence => Level >= 40; // quest-locked
//            public bool UnlockedCover => Level >= 45; // quest-locked
//            public bool UnlockedShirk => Level >= 48;
//            public bool UnlockedCircleOfScorn => Level >= 50;
//            public bool UnlockedHallowedGround => Level >= 50; // quest-locked
//            public bool UnlockedGoringBlade => Level >= 54; // quest-locked
//            public bool UnlockedDivineVeil => Level >= 56; // quest-locked
//            public bool UnlockedClemency => Level >= 58; // quest-locked
//            public bool UnlockedRoyalAuthority => Level >= 60; // quest-locked
//            // TODO: L62+

//            public override string ToString()
//            {
//                return $"RB={RaidBuffsLeft:f1}, FF={FightOrFlightLeft:f1}/{FightOrFlightCD:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
//            }
//        }

//        // strategy configuration
//        public class Strategy : CommonRotation.Strategy
//        {
//            // cooldowns
//            public bool ExecuteRampart;
//            public bool ExecuteReprisal;
//            public bool ExecuteArmsLength;
//            public bool ExecuteProvoke;
//            public bool ExecuteShirk;
//            public bool ExecuteLowBlow;
//            public bool ExecuteInterject;

//            public override string ToString()
//            {
//                var sb = new StringBuilder("SmartQueue:");
//                if (ExecuteProvoke)
//                    sb.Append(" Provoke");
//                if (ExecuteShirk)
//                    sb.Append(" Shirk");
//                if (ExecuteArmsLength)
//                    sb.Append(" ArmsLength");
//                if (ExecuteRampart)
//                    sb.Append(" Rampart");
//                if (ExecuteReprisal)
//                    sb.Append(" Reprisal");
//                if (ExecuteLowBlow)
//                    sb.Append(" LowBlow");
//                if (ExecuteInterject)
//                    sb.Append(" Interject");
//                if (ExecuteSprint)
//                    sb.Append(" Sprint");
//                return sb.ToString();
//            }
//        }

//        public static AID GetNextRiotBladeComboAction(AID comboLastMove)
//        {
//            return comboLastMove == AID.FastBlade ? AID.RiotBlade : AID.FastBlade;
//        }

//        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
//        {
//            if (aoe)
//            {
//                return AID.TotalEclipse;
//            }
//            else
//            {
//                if (state.UnlockedRageOfHalone && state.ComboLastMove == AID.RiotBlade)
//                    return AID.RageOfHalone;
//                else if (state.UnlockedRiotBlade && state.ComboLastMove == AID.FastBlade)
//                    return AID.RiotBlade;
//                else
//                    return AID.FastBlade;
//            }
//        }

//        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe)
//        {
//            // 1. use cooldowns if requested in rough priority order
//            if (strategy.ExecuteProvoke && state.UnlockedProvoke && state.CanWeave(state.ProvokeCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Provoke);
//            if (strategy.ExecuteShirk && state.UnlockedShirk && state.CanWeave(state.ShirkCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Shirk);
//            if (strategy.ExecuteArmsLength && state.UnlockedArmsLength && state.CanWeave(state.ArmsLengthCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.ArmsLength);
//            if (strategy.ExecuteRampart && state.UnlockedRampart && state.CanWeave(state.RampartCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Rampart);
//            if (strategy.ExecuteReprisal && state.UnlockedReprisal && state.CanWeave(state.ReprisalCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Reprisal);
//            if (strategy.ExecuteLowBlow && state.UnlockedLowBlow && state.CanWeave(state.LowBlowCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.LowBlow);
//            if (strategy.ExecuteInterject && state.UnlockedInterject && state.CanWeave(state.InterjectCD, 0.6f, windowEnd))
//                return ActionID.MakeSpell(AID.Interject);
//            if (strategy.ExecuteSprint && state.CanWeave(state.SprintCD, 0.6f, windowEnd))
//                return CommonDefinitions.IDSprint;

//            // 2. fight or flight, if off gcd and late-weaving
//            if (state.UnlockedFightOrFlight && state.CanWeave(state.FightOrFlightCD, 0.6f, windowEnd) && state.GCD <= 1.0f)
//                return ActionID.MakeSpell(AID.FightOrFlight);

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
