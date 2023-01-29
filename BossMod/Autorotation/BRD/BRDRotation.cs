using Dalamud.Game.ClientState.JobGauge.Enums;
using System;
using static BossMod.WAR.Rotation.Strategy;

namespace BossMod.BRD
{
    public static class Rotation
    {
        public enum Song { None, MagesBallad, ArmysPaeon, WanderersMinuet }

        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public Song ActiveSong;
            public float ActiveSongLeft; // 45 max
            public int Repertoire;
            public int SoulVoice;
            public int NumCoda;
            public float StraightShotLeft;
            public float BlastArrowLeft;
            public float ShadowbiteLeft;
            public float RagingStrikesLeft;
            public float BattleVoiceLeft;
            public float RadiantFinaleLeft;
            public float ArmysMuseLeft;
            public float BarrageLeft;
            public float PelotonLeft; // 30 max
            public float TargetCausticLeft;
            public float TargetStormbiteLeft;

            // upgrade paths
            public AID BestBurstShot => Unlocked(AID.BurstShot) ? AID.BurstShot : AID.HeavyShot;
            public AID BestRefulgentArrow => Unlocked(AID.RefulgentArrow) ? AID.RefulgentArrow : AID.StraightShot;
            public AID BestCausticBite => Unlocked(AID.CausticBite) ? AID.CausticBite : AID.VenomousBite;
            public AID BestStormbite => Unlocked(AID.Stormbite) ? AID.Stormbite : AID.Windbite;
            public AID BestLadonsbite => Unlocked(AID.Ladonsbite) ? AID.Ladonsbite : AID.QuickNock;

            // statuses
            public SID ExpectedCaustic => Unlocked(AID.CausticBite) ? SID.CausticBite : SID.VenomousBite;
            public SID ExpectedStormbite => Unlocked(AID.Stormbite) ? SID.Stormbite : SID.Windbite;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"g={ActiveSong}/{ActiveSongLeft:f3}/{Repertoire}/{SoulVoice}/{NumCoda}, RB={RaidBuffsLeft:f3}, SS={StraightShotLeft:f3}, BA={BlastArrowLeft:f3}, SB={ShadowbiteLeft:f3}, Buffs={RagingStrikesLeft:f3}/{BattleVoiceLeft:f3}/{RadiantFinaleLeft:f3}, Muse={ArmysMuseLeft:f3}, Barr={BarrageLeft:f3}, Dots={TargetStormbiteLeft:f3}/{TargetCausticLeft:f3}, PotCD={PotionCD:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public enum SongUse : uint
            {
                Automatic = 0, // allow early MB->AP and AP->WM switches

                [PropertyDisplay("Extend active song", 0x8000ff00)]
                Extend = 1, // extend currently active song until window end or until last tick
            }

            public enum PotionUse : uint
            {
                Manual = 0, // potion won't be used automatically

                [PropertyDisplay("Use right before burst", 0x8000ff00)]
                Burst = 1,

                [PropertyDisplay("Use ASAP", 0x800000ff)]
                Force = 2,
            }

            public SongUse SongStrategy; // how are we supposed to switch songs
            public PotionUse PotionStrategy; // how are we supposed to use potions
            public OffensiveAbilityUse RagingStrikesUse; // how are we supposed to use RS
            public int NumLadonsbiteTargets; // range 12 90-degree cone
            public int NumRainOfDeathTargets; // range 8 circle around target

            public override string ToString()
            {
                return $"AOE={NumRainOfDeathTargets}/{NumLadonsbiteTargets}, no-dots={ForbidDOTs}";
            }

            // TODO: these bindings should be done by the framework...
            public void ApplyStrategyOverrides(uint[] overrides)
            {
                if (overrides.Length >= 2)
                {
                    SongStrategy = (SongUse)overrides[0];
                    PotionStrategy = (PotionUse)overrides[1];
                    RagingStrikesUse = (OffensiveAbilityUse)overrides[2];
                }
                else
                {
                    SongStrategy = SongUse.Automatic;
                    PotionStrategy = PotionUse.Manual;
                    RagingStrikesUse = OffensiveAbilityUse.Automatic;
                }
            }
        }

        public static float SwitchAtRemainingSongTimer(State state, Strategy strategy) => strategy.SongStrategy == Strategy.SongUse.Automatic ? state.ActiveSong switch
        {
            Song.WanderersMinuet => 3, // WM->MB transition when no more repertoire ticks left
            Song.MagesBallad => 12, // MB->AP transition asap as long as we won't end up songless (active song condition 15 == 45 - (120 - 2*45); get extra MB tick at 12s to avoid being songless for a moment)
            Song.ArmysPaeon => state.Repertoire == 4 ? 15 : 3, // AP->WM transition asap as long as we'll have MB ready when WM ends, if we either have full repertoire or AP is about to run out anyway
            _ => 3
        } : 3;

        public static bool ShouldUsePotion(State state, Strategy strategy) => strategy.PotionStrategy switch
        {
            Strategy.PotionUse.Manual => false,
            Strategy.PotionUse.Burst => strategy.CombatTimer < 0 ? strategy.CombatTimer > -1.8f : state.TargetingEnemy && state.CD(CDGroup.RagingStrikes) < state.GCD + 3.5f, // pre-pull or RS ready in 2 gcds (assume pot -> late-weaved WM -> RS)
            Strategy.PotionUse.Force => true,
            _ => false
        };

        // by default, we use RS asap as soon as WM is up
        public static bool ShouldUseRagingStrikes(State state, Strategy strategy) => strategy.RagingStrikesUse switch
        {
            Strategy.OffensiveAbilityUse.Delay => false,
            Strategy.OffensiveAbilityUse.Force => true,
            _ => state.TargetingEnemy && (state.ActiveSong == Song.WanderersMinuet || !state.Unlocked(AID.WanderersMinuet))
        };

        public static AID GetNextBestGCD(State state, Strategy strategy)
        {
            // prepull
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
                return AID.None;

            if (strategy.NumLadonsbiteTargets >= 2 && state.Unlocked(AID.QuickNock))
            {
                // TODO: AA/BA targeting/condition (it might hit fewer targets)
                if (state.BlastArrowLeft > state.GCD)
                    return AID.BlastArrow;

                // TODO: consider using AA at <100 gauge?
                if (state.SoulVoice >= 100)
                    return AID.ApexArrow;

                // TODO: better shadowbite targeting (it might hit fewer targets)
                return state.ShadowbiteLeft > state.GCD ? AID.Shadowbite : state.BestLadonsbite;
            }
            else
            {
                if (state.Unlocked(AID.IronJaws))
                {
                    // apply dots if not up and allowed by strategy
                    if (!strategy.ForbidDOTs && state.TargetStormbiteLeft <= state.GCD)
                        return state.BestStormbite;
                    if (!strategy.ForbidDOTs && state.TargetCausticLeft <= state.GCD)
                        return state.BestCausticBite;

                    // at this point, we have to prioritize IJ, AA/BA and RA procs
                    var majorBuffsUp = state.BattleVoiceLeft > state.GCD; // doesn't matter too much which of the buffs we use

                    // IJ generally has to be used at last possible gcd before dots fall off -or- before major buffs fall off (to snapshot buffs to dots), but in some cases we want to use it earlier:
                    // - 1 gcd earlier if we don't have RA proc (otherwise we might use filler, it would proc RA, then on next gcd we'll have to use IJ to avoid dropping dots and potentially waste another RA)
                    // - 1/2 gcds earlier if we're waiting for more gauge for AA
                    // note that we don't do that during MB, since early IJ would overwrite buffed dot tick with unbuffed, which is a bigger loss (song==MB is a heuristic for determining buffed dots, assuming normal cycle)
                    var refreshDotsDeadline = Math.Min(state.TargetStormbiteLeft, state.TargetCausticLeft);
                    if (majorBuffsUp && refreshDotsDeadline < state.BattleVoiceLeft + 30) // second condition: current dots were applied before buffs if 45-dotsLeft > 15-bvLeft => dotsLeft < bvLeft+30
                        refreshDotsDeadline = Math.Min(refreshDotsDeadline, state.BattleVoiceLeft);

                    if (!strategy.ForbidDOTs && refreshDotsDeadline > state.GCD)
                    {
                        int maxDotRemainingGCDs = 1; // by default, refresh on last possible GCD before we either drop dots or drop major buffs
                        if (state.ActiveSong != Song.MagesBallad)
                        {
                            if (state.StraightShotLeft <= state.GCD)
                                ++maxDotRemainingGCDs; // 1 gcd if we don't have RA proc

                            if (majorBuffsUp && state.SoulVoice is > 50 and < 100) // last condition: best we can hope for over 4 gcds is ~25 gauge (4 ticks + EA)
                                maxDotRemainingGCDs += state.Unlocked(AID.BlastArrow) ? 2 : 1; // 1/2 gcds for AA/BA; only under buffs - outside buffs it's simpler to delay AA
                        }
                        // else: never use early IJ under MB, since that would likely overwrite buffed ticks (heuristic, since we can't determine active dot potency easily)

                        if (refreshDotsDeadline <= state.GCD + 2.5f * maxDotRemainingGCDs)
                            return AID.IronJaws;
                    }

                    // RA if we'd be delaying barrage
                    if (state.StraightShotLeft > state.GCD && state.CD(CDGroup.Barrage) < state.GCD + 2.5f)
                        return state.BestRefulgentArrow;

                    // BA if possible
                    if (state.BlastArrowLeft > state.GCD)
                        return AID.BlastArrow;

                    // AA - different conditions inside and outside burst and depending on gauge
                    // TODO: reconsider deadline
                    if (state.SoulVoice >= 100)
                    {
                        // at 100 gauge we want to use AA asap, unless we're delaying it for burst (TODO: better condition)
                        if (majorBuffsUp || state.ActiveSong == Song.MagesBallad)
                            return AID.ApexArrow;
                    }
                    else if (state.SoulVoice >= 80)
                    {
                        // at 80+ gauge we want to use AA if we can't delay it anymore (TODO: better condition)
                        if (majorBuffsUp ? state.BattleVoiceLeft < state.GCD + 5 : state.ActiveSong == Song.MagesBallad && state.ActiveSongLeft < state.GCD + 12)
                            return AID.ApexArrow;
                    }
                    // else: never use AA at <80 gauge automatically (TODO: consider things like end-of-fight or downtimes?..)

                    // RA/BS
                    return state.StraightShotLeft > state.GCD ? state.BestRefulgentArrow : state.BestBurstShot;
                }
                else
                {
                    // pre IJ our gcds are extremely boring: keep dots up and use up straight shot procs asap
                    // only HS can proc straight shot, so we're not wasting potential procs here
                    // TODO: tweak threshold so that we don't overwrite or miss ticks...
                    // TODO: do we care about reapplying dots early under raidbuffs?..
                    if (!strategy.ForbidDOTs && state.Unlocked(AID.Windbite) && state.TargetStormbiteLeft < state.GCD + 3)
                        return AID.Windbite;
                    if (!strategy.ForbidDOTs && state.Unlocked(AID.VenomousBite) && state.TargetCausticLeft < state.GCD + 3)
                        return AID.VenomousBite;
                    return state.StraightShotLeft > state.GCD ? AID.StraightShot : AID.HeavyShot;
                }
            }
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline)
        {
            // potion
            if (ShouldUsePotion(state, strategy) && state.CanWeave(state.PotionCD, 1.1f, deadline))
                return CommonDefinitions.IDPotionDex;

            // maintain songs
            if (state.TargetingEnemy && strategy.CombatTimer >= 0)
            {
                if (state.ActiveSong == Song.None)
                {
                    // if no song is up, use best available one
                    if (state.Unlocked(AID.WanderersMinuet) && state.CanWeave(CDGroup.WanderersMinuet, 0.6f, deadline))
                        return ActionID.MakeSpell(AID.WanderersMinuet);
                    if (state.Unlocked(AID.MagesBallad) && state.CanWeave(CDGroup.MagesBallad, 0.6f, deadline))
                        return ActionID.MakeSpell(AID.MagesBallad);
                    if (state.Unlocked(AID.ArmysPaeon) && state.CanWeave(CDGroup.ArmysPaeon, 0.6f, deadline))
                        return ActionID.MakeSpell(AID.ArmysPaeon);
                }
                else if (state.Unlocked(AID.WanderersMinuet) && state.ActiveSongLeft < SwitchAtRemainingSongTimer(state, strategy) - 0.1f) // TODO: rethink this extra leeway, we want to make sure we use up last tick's repertoire e.g. in WM
                {
                    // once we have WM, we can have a proper song cycle
                    if (state.ActiveSong == Song.WanderersMinuet)
                    {
                        if (state.Repertoire > 0 && state.CanWeave(CDGroup.PitchPerfect, 0.6f, deadline))
                            return ActionID.MakeSpell(AID.PitchPerfect); // spend remaining repertoire before leaving WM
                        if (state.CanWeave(CDGroup.MagesBallad, 0.6f, deadline))
                            return ActionID.MakeSpell(AID.MagesBallad);
                    }
                    if (state.ActiveSong == Song.MagesBallad && state.CD(CDGroup.WanderersMinuet) < 45 && state.CanWeave(CDGroup.ArmysPaeon, 0.6f, deadline))
                        return ActionID.MakeSpell(AID.ArmysPaeon);
                    if (state.ActiveSong == Song.ArmysPaeon && state.CD(CDGroup.MagesBallad) < 45 && state.CanWeave(CDGroup.WanderersMinuet, 0.6f, deadline) && state.GCD < 0.9f)
                        return ActionID.MakeSpell(AID.WanderersMinuet); // late-weave
                }
            }

            // apply major buffs
            // RS as soon as we enter WM (or just on CD, if we don't have it yet)
            // in opener, it end up being late-weaved after WM (TODO: can we weave it extra-late to ensure 9th gcd is buffed?)
            // in 2-minute bursts, it ends up being early-weaved after first WM gcd (TODO: can we weave it later to ensure 10th gcd is buffed?)
            if (state.Unlocked(AID.RagingStrikes) && ShouldUseRagingStrikes(state, strategy) && state.CanWeave(CDGroup.RagingStrikes, 0.6f, deadline))
                return ActionID.MakeSpell(AID.RagingStrikes);

            // BV+RF 2 gcds after RS (RF first with 1 coda, ? with 2 coda, BV first with 3 coda)
            // opener: gcd is slightly smaller than 2.5, RS should've happened in second ogcd slot at 1.2, we should apply buff at >= 2*2.5-0.6 = 4.4 => RS will have <= 1.2+20-4.4 == 16.8 (+ER-delay) left
            // burst: gcd is slightly smaller than 2.1 (assuming 4-stack AP), RS should've happened in first ogcd slot at 0.6, apply buff at >= 2*2.1-0.6 = 3.6 => RS will have <= 0.6+20-3.6 == 17 (+ER-delay) left
            if (state.TargetingEnemy && state.RagingStrikesLeft > state.AnimationLock && state.RagingStrikesLeft < 16.8f)
            {
                if (state.NumCoda == 1 && state.CanWeave(CDGroup.RadiantFinale, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.RadiantFinale);
                if (state.Unlocked(AID.BattleVoice) && state.CanWeave(CDGroup.BattleVoice, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.BattleVoice);
                if (state.NumCoda > 1 && state.CanWeave(CDGroup.RadiantFinale, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.RadiantFinale);
            }

            // TODO: consider moving PP3 check here and delay EA if we actually fuck up

            // EA - important not to drift (TODO: is it actually better to delay it if we're capped on PP/BL?)
            // we should not be at risk of capping BL (since we spend charges asap in WM/MB anyway)
            // we might risk capping PP, but we should've dealt with that on previous slots by using PP2
            if (state.TargetingEnemy && strategy.CombatTimer >= 0 && state.Unlocked(AID.EmpyrealArrow) && state.CanWeave(CDGroup.EmpyrealArrow, 0.6f, deadline))
                return ActionID.MakeSpell(AID.EmpyrealArrow);

            // PP here should not conflict with anything priority-wise
            // note that we already handle PPx after last repertoire tick before switching to WM (see song cycle code above)
            if (state.TargetingEnemy && state.ActiveSong == Song.WanderersMinuet && state.Repertoire > 0 && state.CanWeave(CDGroup.PitchPerfect, 0.6f, deadline))
            {
                if (state.Repertoire == 3)
                    return ActionID.MakeSpell(AID.PitchPerfect); // PP3 is a no-brainer

                var nextProcIn = state.ActiveSongLeft % 3.0f;
                if (state.BattleVoiceLeft > state.AnimationLock && state.BattleVoiceLeft <= nextProcIn + 1)
                    return ActionID.MakeSpell(AID.PitchPerfect); // PPx if we won't get any more stacks under buffs (TODO: better leeway)

                // if we're at PP2 and EA is about to come off cd, we might be in a situation where waiting for PP3 would have us choose between delaying EA or wasting its guaranteed proc; in such case we want to PP2 early
                if (state.Repertoire == 2)
                {
                    bool usePP2 = false;
                    if (state.CanWeave(CDGroup.EmpyrealArrow, 0.6f, state.GCD))
                    {
                        // we're going to use EA in next ogcd slot before GCD => use PP2 if we won't be able to wait until tick and weave before EA
                        usePP2 = !state.CanWeave(nextProcIn, 0.6f, state.CD(CDGroup.EmpyrealArrow));
                    }
                    else if (state.CD(CDGroup.EmpyrealArrow) < state.GCD + 1.2f)
                    {
                        // we're going to use EA just after next GCD => use PP2 if we won't be able to wait until tick and late-weave before next GCD
                        usePP2 = !state.CanWeave(nextProcIn, 0.6f, state.GCD);
                    }

                    if (usePP2)
                        return ActionID.MakeSpell(AID.PitchPerfect); // PP2 if we might get conflict with EA
                }
            }

            // barrage, under buffs and if there is no proc already
            // TODO: consider barrage usage during aoe
            // TODO: consider moving up to avoid drifting? seems risky...
            if (state.TargetingEnemy && strategy.CombatTimer >= 0 && strategy.NumLadonsbiteTargets < 2 && state.StraightShotLeft <= state.GCD && state.Unlocked(AID.Barrage) && (state.Unlocked(AID.BattleVoice) ? state.BattleVoiceLeft : state.RagingStrikesLeft) > 0 && state.CanWeave(CDGroup.Barrage, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Barrage);

            // sidewinder, unless we're delaying it until buffs (TODO: consider exact delay condition)
            if (state.TargetingEnemy && strategy.CombatTimer >= 0 && state.Unlocked(AID.Sidewinder) && state.CanWeave(CDGroup.Sidewinder, 0.6f, deadline) && state.CD(CDGroup.BattleVoice) > 45)
                return ActionID.MakeSpell(AID.Sidewinder);

            // bloodletter, unless we're pooling them for burst
            if (state.TargetingEnemy && strategy.CombatTimer >= 0 && state.Unlocked(AID.Bloodletter) && state.CanWeave(state.CD(CDGroup.Bloodletter) - 30, 0.6f, deadline))
            {
                bool poolBL = false;
                if (state.Unlocked(AID.WanderersMinuet) && state.ActiveSong != Song.MagesBallad && state.BattleVoiceLeft <= state.AnimationLock)
                {
                    float chargeCapIn = state.CD(CDGroup.Bloodletter) - (state.Unlocked(TraitID.EnhancedBloodletter) ? 0 : 15);
                    poolBL = chargeCapIn > Math.Min(state.CD(CDGroup.RagingStrikes), state.CD(CDGroup.BattleVoice));
                }
                if (!poolBL)
                    return ActionID.MakeSpell(strategy.NumRainOfDeathTargets >= 2 ? AID.RainOfDeath : AID.Bloodletter);
            }

            // no suitable oGCDs...
            return new();
        }
    }
}
