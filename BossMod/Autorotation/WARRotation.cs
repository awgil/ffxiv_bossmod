using System;

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
            public int Gauge; // 0 to 100
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
            public bool EnableUpheaval; // if true, enable using upheaval when needed; setting to false is useful during opener before first party buffs
            public bool EnableMovement; // if true, allows selecting moving abilities (Primal Rend and Onslaught) - disabling is useful if there are positioning mechanics active
            public float NeedChargeIn; // ensure that at least 1 charge stack is available in specified time (setting this to 0 will always preserve 1 charge stack, setting to >=30 will spend all stacks)
            public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net
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
            // TODO: consider whether we should try to cast ability if correspoding buff didn't expire yet, but will expire before GCD is up
            float minBuff = 0; // or state.GCD?

            // 1. if it is the last CD possible for PR/NC, don't waste them
            float gcdDelay = state.GCD + (strategy.Aggressive ? 0 : 2.5f);
            float secondGCDIn = gcdDelay + 2.5f;
            float thirdGCDIn = gcdDelay + 5f;
            if (state.PrimalRendLeft > minBuff && state.PrimalRendLeft < secondGCDIn && strategy.EnableMovement)
                return AID.PrimalRend;
            if (state.NascentChaosLeft > minBuff && state.NascentChaosLeft < secondGCDIn)
                return AID.InnerChaos;
            if (state.NascentChaosLeft > minBuff && state.PrimalRendLeft > minBuff && strategy.EnableMovement && state.PrimalRendLeft < thirdGCDIn && state.NascentChaosLeft < thirdGCDIn)
                return AID.PrimalRend; // either is fine

            // 2. if IR is up, don't waste charges - it is only safe to cast FC/PR
            if (state.InnerReleaseStacks > 0)
            {
                // currently we prioritize PR in 'spend' phase, unless doing so will cost us IR stacks
                if (state.PrimalRendLeft > minBuff && strategy.EnableMovement && strategy.SpendGauge &&
                    state.InnerReleaseLeft >= (gcdDelay + state.InnerReleaseStacks * 2.5))
                {
                    return AID.PrimalRend;
                }

                // just cast FC/IC
                return state.NascentChaosLeft > 0 ? AID.InnerChaos : AID.FellCleave;
            }

            // TODO: what if we have really high gauge and low ST? is it worth it to delay ST application to avoid overcapping gauge?
            // TODO: what if we have low but non-zero ST?
            // 3. no ST => apply buff asap
            if (state.SurgingTempestLeft <= 0)
                return GetNextStormEyeComboAction(state);

            // 4. if we're delaying IR due to nascent chaos, cast it asap
            if (state.NascentChaosLeft > 0 && state.InnerReleaseCD <= secondGCDIn && state.Gauge >= 50)
                return AID.InnerChaos;

            // 5. if we're delaying Infuriate due to gauge, cast FC asap (7.5 for FC)
            if (state.Gauge > 50 && state.InfuriateCD <= gcdDelay + 7.5)
                return state.NascentChaosLeft > 0 ? AID.InnerChaos : AID.FellCleave;

            // 6. if we have >50 gauge, IR is imminent, and casting it will cause us to overcap infuriate, spend gauge asap, so that we can infuriate after
            // 30 seconds is for FC + IR + 3xFC - this is 4 gcds (10 sec) and 4 FCs (another 20 sec)
            if (state.Gauge > 50 && state.InfuriateCD <= gcdDelay + 30 && state.InnerReleaseCD < thirdGCDIn)
                return state.NascentChaosLeft > 0 ? AID.InnerChaos : AID.FellCleave;

            if (!strategy.SpendGauge)
            {
                // we want to delay spending gauge unless doing so will cause us problems later
                var maxSTToAvoidOvercap = 20 + Math.Clamp(state.InnerReleaseCD, 0, 10);
                var combo = state.SurgingTempestLeft < maxSTToAvoidOvercap ? GetNextStormEyeComboAction(state) : GetNextStormPathComboAction(state);
                int comboGauge = combo == AID.HeavySwing ? 0 : (combo == AID.StormPath ? 20 : 10);
                if (state.Gauge + comboGauge <= 100)
                    return combo;
            }

            // ok at this point, we just want to spend gauge - either because we're using greedy strategy, or something prevented us from casting combo
            if (state.PrimalRendLeft > minBuff && strategy.EnableMovement)
                return AID.PrimalRend;
            if (state.NascentChaosLeft > minBuff)
                return AID.InnerChaos;
            if (state.Gauge >= 50)
                return AID.FellCleave;

            // TODO: reconsider min time left...
            return state.SurgingTempestLeft < gcdDelay + 12.5 ? GetNextStormEyeComboAction(state) : GetNextStormPathComboAction(state);
        }

        public static AID GetNextBestOGCD(State state, Strategy strategy, float timeOffset = 0)
        {
            // 1. spend second infuriate stacks asap (unless have IR or >50 gauge)
            if (state.InfuriateCD <= timeOffset && state.InnerReleaseStacks == 0 && state.NascentChaosLeft <= timeOffset && state.Gauge <= 50)
                return AID.Infuriate;

            // 2. upheaval, if surging tempest up
            // TODO: delay for 1 GCD during opener...
            if (state.UpheavalCD <= timeOffset && state.SurgingTempestLeft > timeOffset && strategy.EnableUpheaval)
                return AID.Upheaval;

            // 3. inner release, if surging tempest up and no nascent chaos up
            if (state.InnerReleaseCD <= timeOffset && state.SurgingTempestLeft > timeOffset && state.NascentChaosLeft <= timeOffset)
                return AID.InnerRelease;

            // 4. infuriate - this is complex decision
            // if we are spending gauge, this is easy - just make sure we're not overcapping gauge or interfering with IR or previous infuriate cast
            // otherwise, we're hitting infuriate when either CD is very low:
            // - if IR is imminent, we need at least 22.5 secs of CD (IR+3xFC is 7.5s from spent gcds and 15s from FCs)
            // - if next combo action would overcap our gauge, we need at least 10 secs of CD (it+FC would take 2 gcds)
            // - otherwise we need to still be not overcapping by the next GCD
            if (state.InfuriateCD < (timeOffset + 60) && state.InnerReleaseStacks == 0 && state.NascentChaosLeft <= timeOffset && state.Gauge <= 50)
            {
                float gcdDelay = state.GCD + (strategy.Aggressive ? 0 : 2.5f);
                var irImminent = state.InnerReleaseCD < gcdDelay + 2.5;
                int gaugeCap = state.ComboLastMove == AID.None ? 50 : (state.ComboLastMove == AID.HeavySwing ? 40 : 30); // theoretically we could 
                float maxInfuriateCD = irImminent ? 22.5f : (state.Gauge > gaugeCap ? 10f : 2.5f);
                if (strategy.SpendGauge || state.InfuriateCD <= gcdDelay + maxInfuriateCD)
                    return AID.Infuriate;
            }

            // 5. onslaught, if surging tempest up
            if (state.OnslaughtCD <= (timeOffset + strategy.NeedChargeIn + 30) && state.SurgingTempestLeft > timeOffset && strategy.EnableMovement)
                return AID.Onslaught;

            // no suitable oGCDs...
            return AID.None;
        }

        public static AID GetNextBestAction(State state, Strategy strategy)
        {
            // first ogcd slot
            if (state.GCD > 1.7)
            {
                var ogcd = GetNextBestOGCD(state, strategy, state.GCD - 1.7f);
                if (ogcd != AID.None)
                    return ogcd;
            }

            // second ogcd slot
            if (state.GCD > 0.7)
            {
                var ogcd = GetNextBestOGCD(state, strategy, state.GCD - 0.7f);
                if (ogcd != AID.None)
                    return ogcd;
            }

            // old tried-and-true conservative logic, remove?
            //if (state.GCD > 0.7)
            //{
            //    var ogcd = GetNextBestOGCD(state, strategy);
            //    if (ogcd != AID.None)
            //        return ogcd;
            //}

            return GetNextBestGCD(state, strategy);
        }

        //public static AID GetNextBestAOE(State state, Strategy strategy)
        //{
        //    // TODO implement!
        //    return GetNextAOEComboAction(state);
        //}
    }
}
