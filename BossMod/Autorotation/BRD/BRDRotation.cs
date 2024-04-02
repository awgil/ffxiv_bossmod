namespace BossMod.BRD;

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

        public State(WorldState ws) : base(ws) { }

        public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"g={ActiveSong}/{ActiveSongLeft:f3}/{Repertoire}/{SoulVoice}/{NumCoda}, RB={RaidBuffsLeft:f3}, SS={StraightShotLeft:f3}, BA={BlastArrowLeft:f3}, SB={ShadowbiteLeft:f3}, Buffs={RagingStrikesLeft:f3}/{BattleVoiceLeft:f3}/{RadiantFinaleLeft:f3}, Muse={ArmysMuseLeft:f3}, Barr={BarrageLeft:f3}, Dots={TargetStormbiteLeft:f3}/{TargetCausticLeft:f3}, PotCD={PotionCD:f3}, BVCD={CD(CDGroup.BattleVoice):f3}, BLCD={CD(CDGroup.Bloodletter):f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    // strategy configuration
    public class Strategy : CommonRotation.Strategy
    {
        public enum SongUse : uint
        {
            Automatic = 0, // allow early MB->AP and AP->WM switches

            [PropertyDisplay("Extend until last tick", 0x8000ff00)]
            Extend = 1, // extend currently active song until window end or until last tick

            [PropertyDisplay("Extend until last moment", 0x8000ffff)]
            Overextend = 2, // extend currently active song until window end or until last possible moment

            [PropertyDisplay("Force switch to Wanderer's Minuet", 0x80ff0000)]
            ForceWM = 3,

            [PropertyDisplay("Force switch to Mage's Ballad", 0x80ff00ff)]
            ForceMB = 4,

            [PropertyDisplay("Force switch to Army's Paeon", 0x80ffff00)]
            ForceAP = 5,

            [PropertyDisplay("Force Pitch Perfect (assuming WM is up)", 0x80ff8080)]
            ForcePP = 6,
        }

        public enum PotionUse : uint
        {
            Manual = 0, // potion won't be used automatically

            [PropertyDisplay("Use right before burst", 0x8000ff00)]
            Burst = 1,

            [PropertyDisplay("Use ASAP", 0x800000ff)]
            Force = 2,
        }

        public enum DotUse : uint
        {
            Automatic = 0, // apply dots asap, reapply when either dots about to expire or in buff window

            [PropertyDisplay("Do not apply new dots, extend existing normally", 0x800080ff)]
            AutomaticExtendOnly = 1, // do not apply dots if not up, but use IJ to refresh using standard logic

            [PropertyDisplay("Do not apply new or extend existing dots", 0x800000ff)]
            Forbid = 2, // do not apply dots, do not use IJ

            [PropertyDisplay("Force extend dots via IJ ASAP", 0x8000ff00)]
            ForceExtend = 3, // use IJ asap (as long as both dots are up)

            [PropertyDisplay("Extend dots via IJ only if they are about to fall off (but don't risk proc overwrites), don't extend early under buffs", 0x8000ffff)]
            ExtendIgnoreBuffs = 4, // use IJ only if dots are about to fall off, or 1 gcd earlier if it avoids proc overwrite risk (using filler instead and then being forced to use IJ under proc)

            [PropertyDisplay("Extend dots via IJ at last possible moment, even if it might overwrite proc", 0x8080ffff)]
            ExtendDelayed = 5, // use IJ at last possible moment
        }

        public enum ApexArrowUse : uint
        {
            Automatic = 0, // use at 80+ if buffs are about to run off, use at 100 asap unless raid buffs are imminent

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1, // delay until window end

            [PropertyDisplay("Force at any gauge", 0x8000ff00)]
            ForceAnyGauge = 2, // force use ASAP, even at low gauge (meaning no BA)

            [PropertyDisplay("Force at 80+ gauge", 0x80ffff00)]
            ForceHighGauge = 3, // force use if at 80+ gauge

            [PropertyDisplay("Force at 100 gauge", 0x8000ffff)]
            ForceCapGauge = 4, // force use if at 100 gauge (don't delay until raidbuffs)
        }

        public enum BloodletterUse : uint
        {
            Automatic = 0, // pool for raid buffs, otherwise use freely

            [PropertyDisplay("Delay", 0x800000ff)]
            Delay = 1, // do not use, allowing overcap

            [PropertyDisplay("Force use all charges", 0x8000ff00)]
            Force = 2, // force use ASAP

            [PropertyDisplay("Keep 1 charge, use if 2+ charges available", 0x8000ffff)]
            KeepOneCharge = 3,

            [PropertyDisplay("Keep 2 charges, use if overcap is imminent", 0x80ffff00)]
            KeepTwoCharges = 4,
        }

        public SongUse SongStrategy; // how are we supposed to switch songs
        public PotionUse PotionStrategy; // how are we supposed to use potions
        public DotUse DotStrategy; // how are we supposed to use dots/IJ
        public ApexArrowUse ApexArrowStrategy; // how are we supposed to use AA
        public OffensiveAbilityUse BlastArrowStrategy; // how are we supposed to use BA
        public OffensiveAbilityUse RagingStrikesUse; // how are we supposed to use RS
        public BloodletterUse BloodletterStrategy; // how are we supposed to use bloodletters
        public OffensiveAbilityUse EmpyrealArrowUse; // how are we supposed to use EA
        public OffensiveAbilityUse BarrageUse; // how are we supposed to use barrage
        public OffensiveAbilityUse SidewinderUse; // how are we supposed to use sidewinder
        public int NumLadonsbiteTargets; // range 12 90-degree cone
        public int NumRainOfDeathTargets; // range 8 circle around target

        public override string ToString()
        {
            return $"AOE={NumRainOfDeathTargets}/{NumLadonsbiteTargets}, no-dots={ForbidDOTs}";
        }

        // TODO: these bindings should be done by the framework...
        public void ApplyStrategyOverrides(uint[] overrides)
        {
            if (overrides.Length >= 10)
            {
                SongStrategy = (SongUse)overrides[0];
                PotionStrategy = (PotionUse)overrides[1];
                DotStrategy = (DotUse)overrides[2];
                ApexArrowStrategy = (ApexArrowUse)overrides[3];
                BlastArrowStrategy = (OffensiveAbilityUse)overrides[4];
                RagingStrikesUse = (OffensiveAbilityUse)overrides[5];
                BloodletterStrategy = (BloodletterUse)overrides[6];
                EmpyrealArrowUse = (OffensiveAbilityUse)overrides[7];
                BarrageUse = (OffensiveAbilityUse)overrides[8];
                SidewinderUse = (OffensiveAbilityUse)overrides[9];
            }
            else
            {
                SongStrategy = SongUse.Automatic;
                PotionStrategy = PotionUse.Manual;
                DotStrategy = DotUse.Automatic;
                ApexArrowStrategy = ApexArrowUse.Automatic;
                BlastArrowStrategy = OffensiveAbilityUse.Automatic;
                RagingStrikesUse = OffensiveAbilityUse.Automatic;
                BloodletterStrategy = BloodletterUse.Automatic;
                EmpyrealArrowUse = OffensiveAbilityUse.Automatic;
                BarrageUse = OffensiveAbilityUse.Automatic;
                SidewinderUse = OffensiveAbilityUse.Automatic;
            }
        }
    }

    public static bool CanRefreshDOTsIn(State state, int numGCDs)
    {
        var minLeft = Math.Min(state.TargetStormbiteLeft, state.TargetCausticLeft);
        return minLeft > state.GCD && minLeft <= state.GCD + 2.5f * numGCDs;
    }

    // heuristic to determine whether currently active dots were applied under raidbuffs (assumes dots are actually active)
    // it's not easy to directly determine active dot potency
    public static bool AreActiveDOTsBuffed(State state)
    {
        // dots last for 45s => their time of application Td = t + dotsLeft - 45
        // assuming we're using BV as the main buff, its cd is 120 => it was last used at Ts = t + bvcd - 120, and lasted until Te = Ts + 15
        // so dots are buffed if Ts < Td < Te => t + bvcd - 120 < t + dotsLeft - 45 < t + bvcd - 105 => bvcd - 75 < dotsLeft < bvcd - 60
        // this works when BV is off cd (dotsLeft < -60 is always false)
        // this doesn't really work if dots are not up (can return true if bvcd is between 75 and 60)
        var dotsLeft = Math.Min(state.TargetStormbiteLeft, state.TargetCausticLeft);
        var bvCD = state.CD(CDGroup.BattleVoice);
        return dotsLeft > bvCD - 75 && dotsLeft < bvCD - 60;
    }

    // IJ generally has to be used at last possible gcd before dots fall off -or- before major buffs fall off (to snapshot buffs to dots), but in some cases we want to use it earlier:
    // - 1 gcd earlier if we don't have RA proc (otherwise we might use filler, it would proc RA, then on next gcd we'll have to use IJ to avoid dropping dots and potentially waste another RA)
    // - 1/2 gcds earlier if we're waiting for more gauge for AA
    public static bool ShouldUseIronJawsAutomatic(State state, Strategy strategy)
    {
        var refreshDotsDeadline = Math.Min(state.TargetStormbiteLeft, state.TargetCausticLeft);
        if (refreshDotsDeadline <= state.GCD)
            return false; // don't bother, we won't make it...
        if (refreshDotsDeadline <= state.GCD + 2.5f)
            return true; // last possible gcd to refresh dots - just use IJ now
        if (AreActiveDOTsBuffed(state))
            return false; // never extend buffed dots early: we obviously don't want to use multiple IJs in a single buff window, and outside buff window we don't want to overwrite buffed ticks, even if that means risking losing a proc

        // ok, dots aren't falling off imminently, and they are not buffed - see if we want to ij early and overwrite last ticks
        if (state.StraightShotLeft <= state.GCD && refreshDotsDeadline <= state.GCD + 5 && !ShouldUseApexArrow(state, strategy) && (state.BlastArrowLeft <= state.GCD || strategy.BlastArrowStrategy == Strategy.OffensiveAbilityUse.Delay))
            return true; // refresh 1 gcd early, if we would be forced to cast BS otherwise - if so, we could proc RA and then overwrite it by IJ on next gcd (TODO: i don't really like these conditions...)
        if (state.BattleVoiceLeft <= state.GCD)
            return false; // outside buff window, so no more reasons to extend early

        // under buffs, we might want to do early IJ, so that AA can be slightly delayed, or so that we don't risk proc overwrites
        int maxRemainingGCDs = 1; // by default, refresh on last possible GCD before we either drop dots or drop major buffs
        if (state.StraightShotLeft <= state.GCD)
            ++maxRemainingGCDs; // 1 extra gcd if we don't have RA proc (if we don't refresh early, we might use filler, which could give us a proc; then on next gcd we'll be forced to IJ to avoid dropping dots, which might give another proc)
        // if we're almost at the gauge cap, we want to delay AA/BA (but still fit them into buff window), so we want to IJ earlier
        if (state.SoulVoice is > 50 and < 100) // best we can hope for over 4 gcds is ~25 gauge (4 ticks + EA) - TODO: improve condition
            maxRemainingGCDs += state.Unlocked(AID.BlastArrow) ? 2 : 1; // 1/2 gcds for AA/BA; only under buffs - outside buffs it's simpler to delay AA
        return state.BattleVoiceLeft <= state.GCD + 2.5f * maxRemainingGCDs;
    }

    public static bool ShouldUseIronJaws(State state, Strategy strategy) => strategy.DotStrategy switch
    {
        Strategy.DotUse.Forbid => false,
        Strategy.DotUse.ForceExtend => true,
        Strategy.DotUse.ExtendIgnoreBuffs => CanRefreshDOTsIn(state, state.StraightShotLeft <= state.GCD ? 2 : 1),
        Strategy.DotUse.ExtendDelayed => CanRefreshDOTsIn(state, 1),
        _ => ShouldUseIronJawsAutomatic(state, strategy)
    };

    // you get 5 gauge for every repertoire tick, meaning every 15s you get 5 gauge from EA + up to 25 gauge (*80% = 20 average) from songs
    // using AA at 80+ gauge procs BA, meaning AA at <80 gauge is rarely worth it
    public static bool ShouldUseApexArrow(State state, Strategy strategy) => strategy.ApexArrowStrategy switch
    {
        Strategy.ApexArrowUse.Delay => false,
        Strategy.ApexArrowUse.ForceAnyGauge => state.SoulVoice > 0,
        Strategy.ApexArrowUse.ForceHighGauge => state.SoulVoice >= 80,
        Strategy.ApexArrowUse.ForceCapGauge => state.SoulVoice >= 100,
        _ => state.SoulVoice switch
        {
            >= 100 => state.CD(CDGroup.BattleVoice) >= state.GCD + 45, // use asap, unless we are unlikely to have 80+ gauge by the next buff window (TODO: reconsider time limit)
            >= 80 => state.BattleVoiceLeft > state.GCD
                ? state.BattleVoiceLeft < state.GCD + 5 // under buffs, don't delay AA if doing that will make BA miss buffs (TODO: also don't delay if it can drift barrage past third gcd...)
                : state.CD(CDGroup.BattleVoice) - state.GCD is >= 45 and < 55, // outside buffs, delay unless we risk entering a window where next buffs are imminent and we can't AA (TODO: reconsider window size)
            _ => false // never use AA at <80 gauge automatically; assume manual planning for things like end-of-fight or downtimes
        }
    };

    public static float SwitchAtRemainingSongTimer(State state, Strategy strategy) => strategy.SongStrategy switch
    {
        Strategy.SongUse.Automatic => state.ActiveSong switch
        {
            Song.WanderersMinuet => 3, // WM->MB transition when no more repertoire ticks left
            Song.MagesBallad => strategy.NumRainOfDeathTargets < 3 ? 12 : 3, // MB->AP transition asap as long as we won't end up songless (active song condition 15 == 45 - (120 - 2*45); get extra MB tick at 12s to avoid being songless for a moment), unless we're doing aoe rotation
            Song.ArmysPaeon => state.Repertoire == 4 ? 15 : 3, // AP->WM transition asap as long as we'll have MB ready when WM ends, if we either have full repertoire or AP is about to run out anyway
            _ => 3
        },
        Strategy.SongUse.Extend => 3,
        Strategy.SongUse.Overextend => 0, // TODO: think more about it...
        Strategy.SongUse.ForcePP => state.Repertoire > 0 ? 0 : 3, // if we still have PP charges, don't switch; otherwise switch after last tick (assuming we're under WM)
        _ => 3
    };

    public static bool ShouldUsePotion(State state, Strategy strategy) => strategy.PotionStrategy switch
    {
        Strategy.PotionUse.Manual => false,
        Strategy.PotionUse.Burst => strategy.CombatTimer < 0 ? strategy.CombatTimer > -2f : state.TargetingEnemy && state.CD(CDGroup.RagingStrikes) < state.GCD + 3.5f, // pre-pull or RS ready in 2 gcds (assume pot -> late-weaved WM -> RS)
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

    // by default, we pool bloodletter for burst
    public static bool ShouldUseBloodletter(State state, Strategy strategy) => strategy.BloodletterStrategy switch
    {
        Strategy.BloodletterUse.Delay => false,
        Strategy.BloodletterUse.Force => true,
        Strategy.BloodletterUse.KeepOneCharge => state.CD(CDGroup.Bloodletter) <= 15 + state.AnimationLock,
        Strategy.BloodletterUse.KeepTwoCharges => state.Unlocked(TraitID.EnhancedBloodletter) ? state.CD(CDGroup.Bloodletter) <= state.AnimationLock : false,
        _ => !state.Unlocked(AID.WanderersMinuet) || // don't try to pool BLs at low level (reconsider)
            state.ActiveSong == Song.MagesBallad || // don't try to pool BLs during MB, it's risky
            state.BattleVoiceLeft > state.AnimationLock || // don't pool BLs during buffs
            state.CD(CDGroup.Bloodletter) - (state.Unlocked(TraitID.EnhancedBloodletter) ? 0 : 15) <= Math.Min(state.CD(CDGroup.RagingStrikes), state.CD(CDGroup.BattleVoice)) // don't pool BLs if they will overcap before next buffs
    };

    // by default, we use EA asap if in combat
    public static bool ShouldUseEmpyrealArrow(State state, Strategy strategy) => strategy.EmpyrealArrowUse switch
    {
        Strategy.OffensiveAbilityUse.Delay => false,
        Strategy.OffensiveAbilityUse.Force => true,
        _ => strategy.CombatTimer >= 0
    };

    // by default, we use barrage under raid buffs, being careful not to overwrite RA proc
    // TODO: reconsider barrage usage during aoe
    public static bool ShouldUseBarrage(State state, Strategy strategy) => strategy.BarrageUse switch
    {
        Strategy.OffensiveAbilityUse.Delay => false,
        Strategy.OffensiveAbilityUse.Force => true,
        _ => strategy.CombatTimer >= 0 // in combat
            && (state.Unlocked(AID.BattleVoice) ? state.BattleVoiceLeft : state.RagingStrikesLeft) > 0 // and under raid buffs
            && (strategy.NumLadonsbiteTargets < 2
                ? state.StraightShotLeft <= state.GCD // in non-aoe situation - if there is no RA proc already
                : strategy.NumLadonsbiteTargets >= 4 && state.ShadowbiteLeft > state.GCD) // in aoe situations - use on shadowbite on 4+ targets (TODO: verify!!!)
    };

    // by default, we use sidewinder asap, unless raid buffs are imminent
    public static bool ShouldUseSidewinder(State state, Strategy strategy) => strategy.SidewinderUse switch
    {
        Strategy.OffensiveAbilityUse.Delay => false,
        Strategy.OffensiveAbilityUse.Force => true,
        _ => strategy.CombatTimer >= 0 && state.CD(CDGroup.BattleVoice) > 45 // TODO: consider exact delay condition
    };

    public static AID GetNextBestGCD(State state, Strategy strategy)
    {
        // prepull
        if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
            return AID.None;

        if (strategy.NumLadonsbiteTargets >= 2 && state.Unlocked(AID.QuickNock))
        {
            // TODO: AA/BA targeting/condition (it might hit fewer targets)
            if (state.BlastArrowLeft > state.GCD && strategy.BlastArrowStrategy != Strategy.OffensiveAbilityUse.Delay)
                return AID.BlastArrow;
            if (ShouldUseApexArrow(state, strategy))
                return AID.ApexArrow;

            // TODO: barraged RA on 3 targets?..
            // TODO: better shadowbite targeting (it might hit fewer targets)
            return state.ShadowbiteLeft > state.GCD ? AID.Shadowbite : state.BestLadonsbite;
        }
        else
        {
            var forbidApplyDOTs = strategy.ForbidDOTs || strategy.DotStrategy is Strategy.DotUse.AutomaticExtendOnly or Strategy.DotUse.Forbid;
            if (state.Unlocked(AID.IronJaws))
            {
                // apply dots if not up and allowed by strategy
                if (!forbidApplyDOTs && state.TargetStormbiteLeft <= state.GCD)
                    return state.BestStormbite;
                if (!forbidApplyDOTs && state.TargetCausticLeft <= state.GCD)
                    return state.BestCausticBite;

                // at this point, we have to prioritize IJ, AA/BA and RA procs
                if (!strategy.ForbidDOTs && ShouldUseIronJaws(state, strategy))
                    return AID.IronJaws;

                // there are cases where we want to prioritize RA over AA/BA:
                // - if barrage is about to come off CD, we don't want to delay it needlessly
                // - if delaying RA would force us to IJ on next gcd (potentially overwriting proc)
                // we only do that if there are no explicit AA/BA force strategies (in that case we assume just doing AA/BA is more important than wasting a proc)
                bool highPriorityRA = state.StraightShotLeft > state.GCD // RA ready
                    && strategy.ApexArrowStrategy is Strategy.ApexArrowUse.Automatic or Strategy.ApexArrowUse.Delay // no forced AA
                    && strategy.BlastArrowStrategy != Strategy.OffensiveAbilityUse.Force // no forced BA
                    && (state.CD(CDGroup.Barrage) < state.GCD + 2.5f || CanRefreshDOTsIn(state, 2)); // either barrage coming off cd or dots falling off imminent
                if (highPriorityRA)
                    return state.BestRefulgentArrow;

                // BA if possible and not forbidden
                if (state.BlastArrowLeft > state.GCD && strategy.BlastArrowStrategy != Strategy.OffensiveAbilityUse.Delay)
                    return AID.BlastArrow;

                // AA depending on conditions
                if (ShouldUseApexArrow(state, strategy))
                    return AID.ApexArrow;

                // RA/BS
                return state.StraightShotLeft > state.GCD ? state.BestRefulgentArrow : state.BestBurstShot;
            }
            else
            {
                // pre IJ our gcds are extremely boring: keep dots up and use up straight shot procs asap
                // only HS can proc straight shot, so we're not wasting potential procs here
                // TODO: tweak threshold so that we don't overwrite or miss ticks...
                // TODO: do we care about reapplying dots early under raidbuffs?..
                if (!forbidApplyDOTs && state.Unlocked(AID.Windbite) && state.TargetStormbiteLeft < state.GCD + 3)
                    return AID.Windbite;
                if (!forbidApplyDOTs && state.Unlocked(AID.VenomousBite) && state.TargetCausticLeft < state.GCD + 3)
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
            if (strategy.SongStrategy == Strategy.SongUse.ForceWM && state.Unlocked(AID.WanderersMinuet) && state.CanWeave(CDGroup.WanderersMinuet, 0.6f, deadline))
                return ActionID.MakeSpell(AID.WanderersMinuet);
            if (strategy.SongStrategy == Strategy.SongUse.ForceMB && state.Unlocked(AID.MagesBallad) && state.CanWeave(CDGroup.MagesBallad, 0.6f, deadline))
                return ActionID.MakeSpell(AID.MagesBallad);
            if (strategy.SongStrategy == Strategy.SongUse.ForceAP && state.Unlocked(AID.ArmysPaeon) && state.CanWeave(CDGroup.ArmysPaeon, 0.6f, deadline))
                return ActionID.MakeSpell(AID.ArmysPaeon);

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
        // visualization:
        // -GCD               0               GCD
        //   * -gcd---------- * -gcd---------- * -gcd---------- * -gcd----------
        //   * ---- RS ------ * -------------- * ---- BV - RF - * --------------
        //           ^^^^----------------^^^----------^^^
        //         20s  20s              max          min
        // GCD is slightly smaller than 2.5 during opener, and slightly smaller than 2.1 during reopener (assuming 4-stack AP)
        // RS should happen in second ogcd slot during opener (t in [-gcd + 1.2, -0.6] == [-1.3 + sksDelta, -0.6], or anywhere during burst (t in [-gcd + 0.6, -0.6] == [-1.5 + sksDelta, -0.6])
        // RS buff starts ticking down from 20 at t+erDelay, so we can imagine that RS has effective time-left == 20+erDelay when applied
        // we want to enable BV/RF between [gcd-0.6, gcd+0.6]
        // at T=gcd RS buff will have remaining 20+erDelay-(T-t) == [opener] 20+erDelay-(2.5-sksDelta)+[-1.3+sksDelta, -0.6] == 17.5+erDelay+sksDelta+[-1.3+sksDelta, -0.6] == [16.2+sksDelta, 16.9]+erDelay+sksDelta
        //                                                       == [burst]  20+erDelay-(2.1-sksDelta)+[-1.5+sksDelta, -0.6] == 17.9+erDelay+sksDelta+[-1.5+sksDelta, -0.6] == [16.4+sksDelta, 17.3]+erDelay+sksDelta
        // so condition is [opener] RSLeft <= 16.8 + [sksDelta, 0.7]+erDelay+sksDelta
        //                 [burst]  RSLeft <= 17.0 + [sksDelta, 0.9]+erDelay+sksDelta
        // but note that if we select too small limit (e.g. 16.8), we might run into a problem: if 0.7+erDelay+sksDelta > 1.2, then we might not allow using buff at min, use something else instead, and push second buff to next GCD
        if (state.TargetingEnemy && state.RagingStrikesLeft > state.AnimationLock && state.RagingStrikesLeft < 17)
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
        // TODO: consider clipping gcd to avoid ea drift...
        if (state.TargetingEnemy && ShouldUseEmpyrealArrow(state, strategy) && state.Unlocked(AID.EmpyrealArrow) && state.CanWeave(CDGroup.EmpyrealArrow, 0.6f, deadline))
            return ActionID.MakeSpell(AID.EmpyrealArrow);

        // PP here should not conflict with anything priority-wise
        // note that we already handle PPx after last repertoire tick before switching to WM (see song cycle code above)
        if (state.TargetingEnemy && state.ActiveSong == Song.WanderersMinuet && state.Repertoire > 0 && state.CanWeave(CDGroup.PitchPerfect, 0.6f, deadline))
        {
            if (state.Repertoire == 3)
                return ActionID.MakeSpell(AID.PitchPerfect); // PP3 is a no-brainer

            if (strategy.SongStrategy == Strategy.SongUse.ForcePP)
                return ActionID.MakeSpell(AID.PitchPerfect); // PPx if strategy says so

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
        // TODO: consider moving up to avoid drifting? seems risky...
        if (state.TargetingEnemy && ShouldUseBarrage(state, strategy) && state.Unlocked(AID.Barrage) && state.CanWeave(CDGroup.Barrage, 0.6f, deadline))
            return ActionID.MakeSpell(AID.Barrage);

        // sidewinder, unless we're delaying it until buffs
        if (state.TargetingEnemy && ShouldUseSidewinder(state, strategy) && state.Unlocked(AID.Sidewinder) && state.CanWeave(CDGroup.Sidewinder, 0.6f, deadline))
            return ActionID.MakeSpell(AID.Sidewinder);

        // bloodletter, unless we're pooling them for burst
        if (state.TargetingEnemy && strategy.CombatTimer >= 0 && state.Unlocked(AID.Bloodletter) && ShouldUseBloodletter(state, strategy) && state.CanWeave(state.CD(CDGroup.Bloodletter) - 30, 0.6f, deadline))
            return ActionID.MakeSpell(strategy.NumRainOfDeathTargets >= 2 ? AID.RainOfDeath : AID.Bloodletter);

        // no suitable oGCDs...
        return new();
    }
}
