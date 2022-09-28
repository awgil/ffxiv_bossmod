using System;

namespace BossMod.WAR
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int Gauge; // 0 to 100
            public float SurgingTempestLeft; // 0 if buff not up, max 60
            public float NascentChaosLeft; // 0 if buff not up, max 30
            public float PrimalRendLeft; // 0 if buff not up, max 30
            public float InnerReleaseLeft; // 0 if buff not up, max 15
            public int InnerReleaseStacks; // 0 if buff not up, max 3

            // upgrade paths
            public AID BestFellCleave => NascentChaosLeft > GCD && Unlocked(AID.InnerChaos) ? AID.InnerChaos : Unlocked(AID.FellCleave) ? AID.FellCleave : AID.InnerBeast;
            public AID BestDecimate => NascentChaosLeft > GCD ? AID.ChaoticCyclone : Unlocked(AID.Decimate) ? AID.Decimate : AID.SteelCyclone;
            public AID BestInnerRelease => Unlocked(AID.InnerRelease) ? AID.InnerRelease : AID.Berserk;
            public AID BestBloodwhetting => Unlocked(AID.Bloodwhetting) ? AID.Bloodwhetting : AID.RawIntuition;

            public AID ComboLastMove => (AID)ComboLastAction;
            //public float InnerReleaseCD => CD(UnlockedInnerRelease ? CDGroup.InnerRelease : CDGroup.Berserk); // note: technically berserk and IR don't share CD, and with level sync you can have both...

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"g={Gauge}, RB={RaidBuffsLeft:f1}, ST={SurgingTempestLeft:f1}, NC={NascentChaosLeft:f1}, PR={PrimalRendLeft:f1}, IR={InnerReleaseStacks}/{InnerReleaseLeft:f1}, IRCD={CD(CDGroup.Berserk):f1}/{CD(CDGroup.InnerRelease):f1}, InfCD={CD(CDGroup.Infuriate):f1}, UphCD={CD(CDGroup.Upheaval):f1}, OnsCD={CD(CDGroup.Onslaught):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        // many strategy decisions are represented as "need-something-in" counters; 0 means "use asap", >0 means "do not use unless value is larger than cooldown" (so 'infinity' means 'free to use')
        public class Strategy : CommonRotation.Strategy
        {
            public float FirstChargeIn; // when do we need to use onslaught charge (0 means 'use asap if out of melee range', >0 means that we'll try to make sure 1 charge is available in this time)
            public float SecondChargeIn; // when do we need to use two onslaught charges in a short amount of time
            public bool EnableUpheaval = true; // if true, enable using upheaval when needed; setting to false is useful during opener before first party buffs
            public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net

            public override string ToString()
            {
                return $"";
            }
        }

        public static int GaugeGainedFromAction(State state, AID action)
        {
            return action switch
            {
                AID.Maim or AID.StormEye => 10,
                AID.StormPath => 20,
                AID.MythrilTempest => state.Unlocked(TraitID.MasteringTheBeast) ? 20 : 0,
                _ => 0
            };
        }

        public static AID GetNextSTComboAction(AID comboLastMove, AID finisher)
        {
            return comboLastMove switch
            {
                AID.Maim => finisher,
                AID.HeavySwing => AID.Maim,
                _ => AID.HeavySwing
            };
        }

        public static int GetSTComboLength(AID comboLastMove)
        {
            return comboLastMove switch
            {
                AID.Maim => 1,
                AID.HeavySwing => 2,
                _ => 3
            };
        }

        public static AID GetNextMaimComboAction(AID comboLastMove)
        {
            return comboLastMove == AID.HeavySwing ? AID.Maim : AID.HeavySwing;
        }

        public static AID GetNextAOEComboAction(AID comboLastMove)
        {
            return comboLastMove == AID.Overpower ? AID.MythrilTempest : AID.Overpower;
        }

        public static AID GetNextUnlockedComboAction(State state, float minBuffToRefresh, bool aoe)
        {
            if (aoe && state.Unlocked(AID.Overpower))
            {
                // for AOE rotation, assume dropping ST combo is fine
                return state.Unlocked(AID.MythrilTempest) && state.ComboLastMove == AID.Overpower ? AID.MythrilTempest : AID.Overpower;
            }
            else
            {
                // for ST rotation, assume dropping AOE combo is fine (HS is 200 pot vs MT 100, is 20 gauge + 30 sec ST worth it?..)
                return state.ComboLastMove switch
                {
                    AID.Maim => state.Unlocked(AID.StormPath) ? (state.Unlocked(AID.StormEye) && state.SurgingTempestLeft < minBuffToRefresh ? AID.StormEye : AID.StormPath) : AID.HeavySwing,
                    AID.HeavySwing => state.Unlocked(AID.Maim) ? AID.Maim : AID.HeavySwing,
                    _ => AID.HeavySwing
                };
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            var irCD = state.CD(state.Unlocked(AID.InnerRelease) ? CDGroup.InnerRelease : CDGroup.Berserk);

            // we spend resources either under raid buffs or if another raid buff window will cover at least 4 GCDs of the fight
            bool spendGauge = state.RaidBuffsLeft > state.GCD || strategy.FightEndIn <= strategy.RaidBuffsIn + 10;
            if (!state.Unlocked(AID.InnerRelease))
                spendGauge &= irCD > 5; // TODO: improve...

            float primalRendWindow = MathF.Min(state.PrimalRendLeft, strategy.PositionLockIn);
            var nextFCAction = state.NascentChaosLeft > state.GCD ? (state.Unlocked(AID.InnerChaos) && !aoe ? AID.InnerChaos : AID.ChaoticCyclone)
                : (aoe && state.Unlocked(AID.SteelCyclone)) ? (state.Unlocked(AID.Decimate) ? AID.Decimate : AID.SteelCyclone)
                : (state.Unlocked(AID.FellCleave) ? AID.FellCleave : AID.InnerBeast);

            // 1. if it is the last CD possible for PR/NC, don't waste them
            float gcdDelay = state.GCD + (strategy.Aggressive ? 0 : 2.5f);
            float secondGCDIn = gcdDelay + 2.5f;
            float thirdGCDIn = gcdDelay + 5f;
            if (primalRendWindow > state.GCD && primalRendWindow < secondGCDIn)
                return AID.PrimalRend;
            if (state.NascentChaosLeft > state.GCD && state.NascentChaosLeft < secondGCDIn)
                return nextFCAction;
            if (primalRendWindow > state.GCD && state.NascentChaosLeft > state.GCD && primalRendWindow < thirdGCDIn && state.NascentChaosLeft < thirdGCDIn)
                return AID.PrimalRend; // either is fine

            // 2. if IR/berserk is up, don't waste charges
            if (state.InnerReleaseStacks > 0)
            {
                if (state.Unlocked(AID.InnerRelease))
                {
                    // only consider not casting FC action if delaying won't cost IR stack
                    int fcCastsLeft = state.InnerReleaseStacks;
                    if (state.NascentChaosLeft > state.GCD)
                        ++fcCastsLeft;
                    if (state.InnerReleaseLeft <= state.GCD + fcCastsLeft * 2.5f)
                        return nextFCAction;

                    // don't delay if it won't give us anything (but still prefer PR under buffs)
                    if (state.InnerReleaseLeft <= strategy.RaidBuffsIn)
                        return primalRendWindow > state.GCD && spendGauge ? AID.PrimalRend : nextFCAction;

                    // don't delay FC if it can cause infuriate overcap (e.g. we use combo action, gain gauge and then can't spend it in time)
                    if (state.CD(CDGroup.Infuriate) < state.GCD + (state.InnerReleaseStacks + 1) * 7.5f)
                        return nextFCAction;

                }
                else if (state.Gauge >= 50 && (state.Unlocked(AID.FellCleave) || state.ComboLastMove != AID.Maim || aoe && state.Unlocked(AID.SteelCyclone)))
                {
                    // single-target: FC > SE/ST > IB > Maim > HS
                    // aoe: Decimate > SC > Combo
                    return nextFCAction;
                }
            }

            // 3. no ST (or it will expire if we don't combo asap) => apply buff asap
            // TODO: what if we have really high gauge and low ST? is it worth it to delay ST application to avoid overcapping gauge?
            if (!aoe)
            {
                if (state.Unlocked(AID.StormEye) && state.SurgingTempestLeft <= state.GCD + 2.5f * (GetSTComboLength(state.ComboLastMove) - 1))
                    return GetNextSTComboAction(state.ComboLastMove, AID.StormEye);
            }
            else
            {
                if (state.Unlocked(TraitID.MasteringTheBeast) && state.SurgingTempestLeft <= state.GCD + (state.ComboLastMove != AID.Overpower ? 2.5f : 0))
                    return GetNextAOEComboAction(state.ComboLastMove);
            }

            // 4. if we're delaying Infuriate due to gauge, cast FC asap (7.5 for FC)
            if (state.Gauge > 50 && state.Unlocked(AID.Infuriate) && state.CD(CDGroup.Infuriate) <= gcdDelay + 7.5)
                return nextFCAction;

            // 5. if we have >50 gauge, IR is imminent, and not spending gauge now will cause us to overcap infuriate, spend gauge asap
            // 30 seconds is for FC + IR + 3xFC - this is 4 gcds (10 sec) and 4 FCs (another 20 sec)
            if (state.Gauge > 50 && state.Unlocked(AID.Infuriate) && state.CD(CDGroup.Infuriate) <= gcdDelay + 30 && irCD < secondGCDIn)
                return nextFCAction;

            // 6. if there is no chance we can delay PR until next raid buffs, just cast it now
            if (primalRendWindow > state.GCD && primalRendWindow <= strategy.RaidBuffsIn)
                return AID.PrimalRend;

            // TODO: do not spend gauge if we're delaying berserk
            if (!spendGauge)
            {
                // we want to delay spending gauge unless doing so will cause us problems later
                var maxSTToAvoidOvercap = 20 + Math.Clamp(irCD, 0, 10);
                var nextCombo = GetNextUnlockedComboAction(state, maxSTToAvoidOvercap, aoe);
                if (state.Gauge + GaugeGainedFromAction(state, nextCombo) <= 100)
                    return nextCombo;
            }

            // ok at this point, we just want to spend gauge - either because we're using greedy strategy, or something prevented us from casting combo
            if (primalRendWindow > state.GCD)
                return AID.PrimalRend;
            if (state.Gauge >= 50 || state.InnerReleaseStacks > 0 && state.Unlocked(AID.InnerRelease))
                return nextFCAction;

            // TODO: reconsider min time left...
            return GetNextUnlockedComboAction(state, gcdDelay + 12.5f, aoe);
        }

        // check whether berserk should be delayed (we want to spend it on FCs)
        // this is relevant only until we unlock IR
        public static bool DelayBerserk(State state)
        {
            if (state.Unlocked(AID.Infuriate))
            {
                // we really want to cast SP + 2xIB or 3xIB under berserk; check whether we'll have infuriate before third GCD
                var availableGauge = state.Gauge;
                if (state.CD(CDGroup.Infuriate) <= 65)
                    availableGauge += 50;
                return state.ComboLastMove switch
                {
                    AID.Maim => availableGauge < 80, // TODO: this isn't a very good check, improve...
                    _ => availableGauge < 150
                };
            }
            else if (state.Unlocked(AID.InnerBeast))
            {
                // pre level 50 we ideally want to cast SP + 2xIB under berserk (we need to have 80+ gauge for that)
                // however, we are also content with casting Maim + SP + IB (we need to have 20+ gauge for that; but if we have 70+, it is better to delay for 1 GCD)
                // alternatively, we could delay for 3 GCDs at 40+ gauge - TODO determine which is better
                return state.ComboLastMove switch
                {
                    AID.HeavySwing => state.Gauge < 20 || state.Gauge >= 70,
                    AID.Maim => state.Gauge < 80,
                    _ => true,
                };
            }
            else
            {
                // pre level 35 there is no point delaying berserk at all
                return false;
            }
        }

        // window-end is either GCD or GCD - time-for-second-ogcd; we are allowed to use ogcds only if their animation lock would complete before window-end
        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, bool aoe)
        {
            var irCD = state.CD(state.Unlocked(AID.InnerRelease) ? CDGroup.InnerRelease : CDGroup.Berserk);

            // 1. potion, if required by strategy, and not too early in opener (TODO: reconsider priority)
            // TODO: reconsider potion use during opener (delayed IR prefers after maim, early IR prefers after storm eye, to cover third IC on 13th GCD)
            // note: this check will not allow using potions before lvl 50, but who cares...
            if (strategy.Potion != Strategy.PotionUse.Manual && state.CanWeave(state.PotionCD, 1.1f, deadline) && (state.SurgingTempestLeft > 0 || state.ComboLastMove == AID.Maim))
            {
                // note: potion should never be delayed during opener slot
                // we have a problem with late buff application during opener: between someone casting first raidbuff and us receiving buff, RaidBuffsLeft will be 0 and RaidBuffsIn will be very large
                // after opener this won't be a huge deal, since we have several GCDs of leeway + most likely we have several raid buffs that are at least somewhat staggered
                bool allowPotion = true;
                if (strategy.Potion != Strategy.PotionUse.Immediate && state.SurgingTempestLeft > 0)
                {
                    // if we're delaying potion, make sure it covers IR (note: if IR is already up, it is too late...)
                    allowPotion &= irCD < 15; // note: absolute max is 10, since we need 4 GCDs to fully consume IR
                    if (strategy.Potion == Strategy.PotionUse.DelayUntilRaidBuffs)
                    {
                        // further delay potion until raidbuffs are up or imminent
                        // we can't really control whether raidbuffs cover IR window, so skip potion only if we're sure raidbuffs might be up for next IR window
                        // we assume that typical average raidbuff window is 20 sec, so raidbuffs will cover next window if they will start in ~(time to next IR - buff duration) ~= (IRCD + 60 - 20)
                        allowPotion &= state.RaidBuffsLeft > 0 || strategy.RaidBuffsIn < irCD + 40;
                    }
                }

                if (allowPotion)
                    return CommonDefinitions.IDPotionStr;
            }

            // 2. inner release, if surging tempest up
            // TODO: early IR option: technically we can use right after heavy swing, we'll use maim->SE->IC->3xFC
            // if not unlocked yet, use berserk instead, but only if we have enough gauge
            if (state.Unlocked(AID.Berserk) && state.CanWeave(irCD, 0.6f, deadline) && (state.SurgingTempestLeft > state.GCD + 5 || !state.Unlocked(AID.StormEye)))
            {
                if (state.Unlocked(AID.InnerRelease))
                    return ActionID.MakeSpell(AID.InnerRelease);
                else if (aoe || !DelayBerserk(state))
                    return ActionID.MakeSpell(AID.Berserk);
            }

            // 3. upheaval, if surging tempest up and not forbidden
            // TODO: delay for 1 GCD during opener...
            // TODO: reconsider priority compared to IR
            if (state.Unlocked(AID.Upheaval) && state.CanWeave(CDGroup.Upheaval, 0.6f, deadline) && state.SurgingTempestLeft > MathF.Max(state.CD(CDGroup.Upheaval), 0) && strategy.EnableUpheaval)
                return ActionID.MakeSpell(aoe && state.Unlocked(AID.Orogeny) ? AID.Orogeny : AID.Upheaval);

            // 4. infuriate, if not forbidden and not delayed
            bool spendGauge = state.RaidBuffsLeft >= state.GCD || strategy.FightEndIn <= strategy.RaidBuffsIn + 10;
            bool infuriateAvailable = state.Unlocked(AID.Infuriate) && state.CanWeave(state.CD(CDGroup.Infuriate) - 60, 0.6f, deadline); // note: for second stack, this will be true if casting it won't delay our next gcd
            infuriateAvailable &= state.Gauge <= 50; // never cast infuriate if doing so would overcap gauge
            if (state.Unlocked(AID.ChaoticCyclone))
            {
                // if we have NC, we should not cast infuriate if IR is about to expire or if we haven't spent NC yet
                infuriateAvailable &= state.InnerReleaseLeft <= state.GCD || state.InnerReleaseLeft > state.GCD + 2.5f * state.InnerReleaseStacks; // never cast infuriate if it will cause us to lose IR stacks
                infuriateAvailable &= state.NascentChaosLeft <= state.GCD; // never cast infuriate if NC from previous infuriate is still up for next GCD
            }
            if (infuriateAvailable)
            {
                // different logic before IR and after IR
                if (state.Unlocked(AID.InnerRelease))
                {
                    // with IR, main purpose of infuriate is to generate gauge to burn in spend mode
                    if (spendGauge)
                        return ActionID.MakeSpell(AID.Infuriate);

                    // don't delay if we risk overcapping stacks
                    // max safe cooldown calculation:
                    // - start with remaining GCD + grace period; if CD is smaller, by the time we get a chance to reconsider, we'll have 2 stacks
                    //   grace period should at very least be LockDelay, but next-best GCD could be Primal Rend with longer animation lock, plus we might prioritize different oGCDs, so use full extra GCD to be safe
                    // - if next GCD could give us >50 gauge, we'd need one more GCD to cast FC (which would also reduce cd by extra 5 seconds), so add 7.5s
                    // - if IR is imminent, we delay infuriate now, cast some GCD that gives us >50 gauge, we'd need to cast 3xFCs, which would add extra 22.5s
                    // - if IR is active, we delay infuriate now, we might need to spend remaining GCDs on FCs, which would add extra N * 7.5s
                    float maxInfuriateCD = state.GCD + 2.5f;
                    int gaugeCap = state.ComboLastMove == AID.None ? 50 : (state.ComboLastMove == AID.HeavySwing ? 40 : 30);
                    if (state.Gauge > gaugeCap)
                        maxInfuriateCD += 7.5f;
                    bool irImminent = irCD < state.GCD + 2.5;
                    maxInfuriateCD += (irImminent ? 3 : state.InnerReleaseStacks) * 7.5f;
                    if (state.CD(CDGroup.Infuriate) <= maxInfuriateCD)
                        return ActionID.MakeSpell(AID.Infuriate);

                }
                else
                {
                    // before IR, main purpose of infuriate is to maximize buffed FCs under Berserk
                    if (state.InnerReleaseLeft > state.GCD)
                        return ActionID.MakeSpell(AID.Infuriate);

                    // don't delay if we risk overcapping stacks
                    if (state.CD(CDGroup.Infuriate) <= state.GCD + 10)
                        return ActionID.MakeSpell(AID.Infuriate);

                    // TODO: consider whether we want to spend both stacks in spend mode if Berserk is not imminent...
                }
            }

            // 5. onslaught, if surging tempest up and not forbidden
            if (state.Unlocked(AID.Onslaught) && state.CanWeave(state.CD(CDGroup.Onslaught) - 60, 0.6f, deadline) && strategy.PositionLockIn > state.AnimationLock && state.SurgingTempestLeft > state.AnimationLock)
            {
                float chargeCapIn = state.CD(CDGroup.Onslaught) - (state.Unlocked(TraitID.EnhancedOnslaught) ? 0 : 30);
                if (chargeCapIn < state.GCD + 2.5)
                    return ActionID.MakeSpell(AID.Onslaught); // onslaught now, otherwise we risk overcapping charges

                if (strategy.FirstChargeIn <= 0)
                    return ActionID.MakeSpell(AID.Onslaught); // onslaught now, since strategy asks for it

                // check whether using onslaught now won't prevent us from using it when strategy demands
                // first charge: if we use charge now, CD will become curr+30; after dt, it will then become curr+30-dt; if we want to charge there, we need it to be <= 60 ==> it's safe to charge if curr+30-dt <= 60 => curr <= dt+30
                // second charge: if we use charge now, CD will become curr+30; after dt (and using 'first' charge inside this dt) it will then become curr+60-dt; condition for second charge is then curr+60-dt <= 60 => curr <= dt
                bool safeToUseOnslaught = state.CD(CDGroup.Onslaught) <= strategy.FirstChargeIn + 30 && state.CD(CDGroup.Onslaught) <= strategy.SecondChargeIn;

                // use onslaught now if it's safe and we're either spending gauge or won't be able to delay it until next buff window anyway
                if (safeToUseOnslaught && (spendGauge || chargeCapIn <= strategy.RaidBuffsIn))
                    return ActionID.MakeSpell(AID.Onslaught);
            }

            // no suitable oGCDs...
            return new();
        }
    }
}
