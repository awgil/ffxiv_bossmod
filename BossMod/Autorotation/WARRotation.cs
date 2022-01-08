namespace BossMod
{
    // TODO: consider making a 'plan' based on selected strategy, instead of determining best action reactively
    public class WARRotation
    {
        public enum AID
        {
            None = 0,

            // single target GCDs
            HeavySwing = 31,
            Maim = 37,
            StormPath = 42,
            StormEye = 45,
            InnerBeast = 49,
            FellCleave = 3549,
            InnerChaos = 16465,
            PrimalRend = 25753,

            // aoe GCDs
            Overpower = 41,
            MythrilTempest = 16462,
            SteelCyclone = 51,
            Decimate = 3550,
            ChaoticCyclone = 16463,

            // oGCDs
            Infuriate = 52,
            Onslaught = 7386,
            Upheaval = 7387,
            Orogeny = 25752,

            // offsensive CDs
            Berserk = 38,
            InnerRelease = 7389,

            // defensive CDs
            Rampart = 7531,
            Vengeance = 44,
            ThrillOfBattle = 40,
            Holmgang = 43,
            Equilibrium = 3552,
            Reprisal = 7535,
            ShakeItOff = 7388,
            RawIntuition = 3551,
            NascentFlash = 16464,
            Bloodwhetting = 25751,
            ArmsLength = 7548,

            // misc
            Tomahawk = 46,
            Defiance = 48,
            Provoke = 7533,
            Shirk = 7537,
            LowBlow = 7540,
            Interject = 7538,
        }

        public enum StatusID
        {
            None = 0,
            SurgingTempest = 2677, // applied by StormEye, damage buff
            NascentChaos = 1897, // applied by Infuriate, converts next FC to IC
            InnerRelease = 1177, // applied by InnerRelease, next 3 GCDs should be free FCs
            PrimalRend = 2624, // applied by InnerRelease, allows casting PR
        }

        // full state needed for determining next action
        public struct State
        {
            public float ComboTimeLeft; // 0 if not in combo, max 30
            public AID ComboLastMove;
            public float Gauge; // 0 to 100
            public float SurgingTempestLeft; // 0 if buff not up, max 60
            public float NascentChaosLeft; // 0 if buff not up, max 30
            public float PrimalRendLeft; // 0 if buff not up, max 30
            public float InnerReleaseLeft; // 0 if buff not up, max 15
            public int InnerReleaseStacks; // 0 if buff not up, max 3
            public float InnerReleaseCD; // 60 max, 0 if ready
            public float InfuriateCD; // 120 max, >60 if 0 stacks ready, >0 if 1 stack ready, ==0 if 2 stacks ready
            public float UpheavalCD; // 60 max, 0 if ready
            public float OnslaughtCD; // 90 max, >60 if 0 stacks ready, >30 if 1 stack ready, >0 if 2 stacks ready, ==0 if 3 stacks ready
            public float GCD; // 2.5 max (decreased by SkS), 0 if not on gcd
        }

        // strategy configuration
        public struct Strategy
        {
            public bool SpendGauge; // if true, spend as much gauge as possible (e.g. during buff window); if false, retain as much as possible, but avoid overcapping anything
            public bool EnableMovement; // if true, allows selecting moving abilities (Primal Rend and Onslaught) - disabling is useful if there are positioning mechanics active
            public float NeedChargeIn; // ensure that at least 1 charge stack is available in specified time (setting this to 0 will always preserve 1 charge stack, setting to >=30 will spend all stacks)
        }

        public static AID GetNextStormPathComboAction(State state)
        {
            if (state.ComboLastMove == AID.Maim)
                return AID.StormPath;
            else if (state.ComboLastMove == AID.HeavySwing)
                return AID.Maim;
            else
                return AID.HeavySwing;
        }

        public static AID GetNextStormEyeComboAction(State state)
        {
            if (state.ComboLastMove == AID.Maim)
                return AID.StormEye;
            else if (state.ComboLastMove == AID.HeavySwing)
                return AID.Maim;
            else
                return AID.HeavySwing;
        }

        public static AID GetNextAOEComboAction(State state)
        {
            return state.ComboLastMove == AID.Overpower ? AID.MythrilTempest : AID.Overpower;
        }

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            // TODO: currently this spends gauge asap; take strategy into account
            // TODO: don't let PR/NC fall off, even if surging tempest is not up
            // TODO: if IR is up, never spend charges on combos
            // TODO: consider what to do if we have >80 gauge on opener - is it really ok to overcap to put buff asap?..

            // GCDs:
            // 1. no surging tempest => apply buff asap
            // 2. nascent chaos up => inner chaos
            // 3. primal rend up => primal rend
            // 4. inner release up => fell cleave
            // 5. gauge > 50 => fell cleave
            // 6. otherwise => combo
            if (state.SurgingTempestLeft == 0)
            {
                // TODO: low prio - what to do if we start fight with full gauge?..
                return GetNextStormEyeComboAction(state);
            }
            else if (state.NascentChaosLeft > 0)
            {
                return AID.InnerChaos;
            }
            else if (state.PrimalRendLeft > 0)
            {
                return AID.PrimalRend;
            }
            else if (state.InnerReleaseLeft > 0 || state.Gauge >= 50)
            {
                return AID.FellCleave;
            }
            else
            {
                return state.SurgingTempestLeft < 15 ? GetNextStormEyeComboAction(state) : GetNextStormPathComboAction(state);
            }
        }

        public static AID GetNextBestOGCD(State state, Strategy strategy)
        {
            // oGCDs:
            // 1. no inner release and gauge <= 50 and infuriate ready => infuriate
            // 2. surging tempest up and upheaval ready => upheaval
            // 3. no nascent chaos and inner release ready => inner release
            // 4. no nascent chaos and no inner release and buff up and gauge <= 50 and infuriate ready => infuriate
            // 5. buff up and onslaught 3 stacks => onslaught (?)
            if (state.InfuriateCD == 0 && state.InnerReleaseLeft == 0 && state.NascentChaosLeft == 0 && state.Gauge <= 50)
            {
                return AID.Infuriate;
            }
            else if (state.UpheavalCD == 0 && state.SurgingTempestLeft > 0)
            {
                return AID.Upheaval;
            }
            else if (state.InnerReleaseCD == 0 && state.SurgingTempestLeft > 0 && state.NascentChaosLeft == 0)
            {
                return AID.InnerRelease;
            }
            else if (state.InfuriateCD < 60 && state.InnerReleaseLeft == 0 && state.NascentChaosLeft == 0 && state.Gauge <= 50)
            {
                return AID.Infuriate;
            }
            else if (state.OnslaughtCD == 0 && state.SurgingTempestLeft > 0)
            {
                return AID.Onslaught;
            }
            return AID.None;
        }

        public static AID GetNextBestAction(State state, Strategy strategy)
        {
            var gcd = GetNextBestGCD(state, strategy);
            var ogcd = GetNextBestOGCD(state, strategy);
            return (state.GCD < 0.7 || ogcd == AID.None) ? gcd : ogcd;
        }

        //public static AID GetNextBestAOE(State state, Strategy strategy)
        //{
        //    // TODO implement!
        //    return GetNextAOEComboAction(state);
        //}
    }
}
