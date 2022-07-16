using System.Text;

namespace BossMod
{
    public static class DRGRotation
    {
        public enum AID : uint
        {
            None = 0,

            // single target GCDs
            TrueThrust = 75,
            VorpalThrust = 78,
            Disembowel = 87,
            FullThrust = 84,
            ChaosThrust = 88,
            FangAndClaw = 3554,
            WheelingThrust = 3556,
            RaidenThrust = 16479,
            HeavensThrust = 25771,
            ChaoticSpring = 25772,

            // aoe GCDs
            DoomSpike = 86,
            SonicThrust = 7397,
            CoerthanTorment = 16477,
            DraconianFury = 25770,

            // oGCDs
            Jump = 92,
            HighJump = 16478,
            SpineshatterDive = 95,
            DragonfireDive = 96,
            Geirskogul = 3555,
            Nastrond = 7400,
            MirageDive = 7399,
            Stardiver = 16480,
            WyrmwindThrust = 25773,

            // offsensive CDs
            LifeSurge = 83,
            LanceCharge = 85,
            BattleLitany = 3557,
            DragonSight = 7398,

            // defensive CDs
            SecondWind = 7541,
            Bloodbath = 7542,
            Feint = 7549,
            ArmsLength = 7548,

            // misc
            PiercingTalon = 90,
            ElusiveJump = 94,
            TrueNorth = 7546,
            LegSweep = 7863,
        }
        public static ActionID IDStatPotion = new(ActionType.Item, 1036109); // hq grade 6 tincture of strength

        public enum SID : uint
        {
            PowerSurge = 2720,
            None = 0,
        }

        // full state needed for determining next action
        public class State : CommonRotation.State
        {
            public float PowerSurgeLeft; // 30 max
            public float LifeSurgeCD; // 45 max, 0 if ready
            public float ArmsLengthCD; // 120 max, 0 if ready
            public float SecondWindCD;
            public float BloodbathCD;
            public float LegSweepCD;

            public AID ComboLastMove => (AID)ComboLastAction;

            // per-level ability unlocks (TODO: consider abilities unlocked by quests - they could be unavailable despite level being high enough)
            public bool UnlockedVorpalThrust => Level >= 4;
            public bool UnlockedLifeSurge => Level >= 6;
            public bool UnlockedSecondWind => Level >= 8;
            public bool UnlockedLegSweep => Level >= 10;
            public bool UnlockedBloodbath => Level >= 12;
            public bool UnlockedPiercingTalon => Level >= 15; // quest-locked
            public bool UnlockedDisembowel => Level >= 18;
            public bool UnlockedFeint => Level >= 22;
            public bool UnlockedFullThrust => Level >= 26;
            public bool UnlockedLanceCharge => Level >= 30; // quest-locked
            public bool UnlockedJump => Level >= 30; // quest-locked
            public bool UnlockedArmsLength => Level >= 32;
            public bool UnlockedElusiveJump => Level >= 35; // quest-locked
            public bool UnlockedDoomSpike => Level >= 40; // quest-locked
            public bool UnlockedSpineshatterDive => Level >= 45; // quest-locked
            public bool UnlockedTrueNorth => Level >= 50;
            public bool UnlockedChaosThrust => Level >= 50;
            public bool UnlockedDragonfireDive => Level >= 50; // quest-locked
            public bool UnlockedBattleLitany => Level >= 52; // quest-locked
            public bool UnlockedFangAndClaw => Level >= 56; // quest-locked
            public bool UnlockedWheelingThrust => Level >= 58; // quest-locked
            public bool UnlockedGeirskogul => Level >= 60; // quest-locked
            // TODO: L62+

            public override string ToString()
            {
                return $"RB={RaidBuffsLeft:f1}, PS={PowerSurgeLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            // cooldowns
            public bool ExecuteArmsLength;
            public bool ExecuteSecondWind;
            public bool ExecuteBloodbath;
            public bool ExecuteLegSweep;

            public override string ToString()
            {
                var sb = new StringBuilder("SmartQueue:");
                if (ExecuteArmsLength)
                    sb.Append(" ArmsLength");
                if (ExecuteSecondWind)
                    sb.Append(" SecondWind");
                if (ExecuteBloodbath)
                    sb.Append(" Bloodbath");
                if (ExecuteLegSweep)
                    sb.Append(" LegSweep");
                if (ExecuteSprint)
                    sb.Append(" Sprint");
                return sb.ToString();
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            // TODO: this is correct until L26
            return state.ComboLastMove switch
            {
                AID.TrueThrust => state.UnlockedDisembowel && state.PowerSurgeLeft < state.GCD + 5 ? AID.Disembowel : state.UnlockedVorpalThrust ? AID.VorpalThrust : AID.TrueThrust, // TODO: better threshold (probably depends on combo length?)
                _ => AID.TrueThrust
            };
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float windowEnd, bool aoe)
        {
            // 1. use cooldowns if requested in rough priority order
            if (strategy.ExecuteArmsLength && state.UnlockedArmsLength && state.CanWeave(state.ArmsLengthCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.ArmsLength);
            if (strategy.ExecuteSecondWind && state.UnlockedSecondWind && state.CanWeave(state.SecondWindCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.SecondWind);
            if (strategy.ExecuteBloodbath && state.UnlockedBloodbath && state.CanWeave(state.BloodbathCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.Bloodbath);
            if (strategy.ExecuteLegSweep && state.UnlockedLegSweep && state.CanWeave(state.LegSweepCD, 0.6f, windowEnd))
                return ActionID.MakeSpell(AID.LegSweep);
            if (strategy.ExecuteSprint && state.CanWeave(state.SprintCD, 0.6f, windowEnd))
                return CommonRotation.IDSprint;

            // 2. life surge on most damaging gcd (TODO: reconsider condition, it's valid until L26...)
            if (state.UnlockedLifeSurge && state.CanWeave(state.LifeSurgeCD, 0.6f, windowEnd) && state.ComboLastMove == AID.TrueThrust && (!state.UnlockedDisembowel || state.PowerSurgeLeft >= state.GCD + 5))
                return ActionID.MakeSpell(AID.LifeSurge);

            // no suitable oGCDs...
            return new();
        }

        public static ActionID GetNextBestAction(State state, Strategy strategy, bool aoe)
        {
            ActionID res = new();
            if (state.CanDoubleWeave) // first ogcd slot
                res = GetNextBestOGCD(state, strategy, state.DoubleWeaveWindowEnd, aoe);
            if (!res && state.CanSingleWeave) // second/only ogcd slot
                res = GetNextBestOGCD(state, strategy, state.GCD, aoe);
            if (!res) // gcd
                res = ActionID.MakeSpell(GetNextBestGCD(state, strategy, aoe));
            return res;
        }

        // short string for supported action
        public static string ActionShortString(ActionID action)
        {
            return action == CommonRotation.IDSprint ? "Sprint" : action == IDStatPotion ? "StatPotion" : ((AID)action.ID).ToString();
        }
    }
}
