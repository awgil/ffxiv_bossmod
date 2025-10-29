using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;

// TODO: low level (<68): ea does not proc repertoire, how does it affect the cycle?
// TODO: low level (<78): there's no army's muse, how does it affect the cycle / raidbuffs?..
// TODO: low level (<84): 2-charge bloodletter, esp verify ARC
public sealed class VeynBRD(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { AOE, Songs, Buffs, Potion, DOTs, ApexArrow, BlastArrow, ResonantArrow, RadiantEncore, Bloodletter, EmpyrealArrow, Barrage, Sidewinder, GCDDelay }
    public enum AOEStrategy { SingleTarget, AutoTargetHitPrimary, AutoTargetHitMost, AutoOnPrimary, ForceAOE }
    public enum SongStrategy { Automatic, Extend, Overextend, ForceWM, ForceMB, ForceAP, ForcePP, Delay }
    public enum BuffsStrategy { Automatic, Delay, ForceRF, ForceBV, ForceRS }
    public enum PotionStrategy { Manual, Burst, Force }
    public enum DotStrategy { Automatic, ApplyOrExtend, AutomaticExtendOnly, Forbid, ForceExtend, ExtendIgnoreBuffs, ExtendDelayed }
    public enum ApexArrowStrategy { Automatic, Delay, ForceAnyGauge, ForceHighGauge, ForceCapGauge }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum BloodletterStrategy { Automatic, Delay, Force, KeepOneCharge, KeepTwoCharges }
    public enum GCDDelayStrategy { NoPrepull, EarlyPrepull, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Veyn BRD", "Standard rotation module", "Standard rotation (veyn)", "veyn", RotationModuleQuality.Basic, BitMask.Build((int)Class.BRD, (int)Class.ARC), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 110)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AutoTargetHitPrimary, "AutoTargetHitPrimary", "Use aoe actions if profitable, select best target that ensures primary target is hit")
            .AddOption(AOEStrategy.AutoTargetHitMost, "AutoTargetHitMost", "Use aoe actions if profitable, select a target that ensures maximal number of targets are hit")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use aoe actions on primary target if profitable")
            .AddOption(AOEStrategy.ForceAOE, "AOE", "Use aoe rotation on primary target even if it's less total damage than single-target")
            .AddAssociatedActions(BRD.AID.QuickNock, BRD.AID.WideVolley, BRD.AID.Ladonsbite, BRD.AID.RainOfDeath, BRD.AID.Shadowbite);

        res.Define(Track.Songs).As<SongStrategy>("Songs", uiPriority: 100)
            .AddOption(SongStrategy.Automatic, "Automatic (3-6-9)")
            .AddOption(SongStrategy.Extend, "Extend", "Extend until last tick")
            .AddOption(SongStrategy.Overextend, "Overextend", "Extend until last possible moment")
            .AddOption(SongStrategy.ForceWM, "ForceWM", "Force switch to Wanderer's Minuet")
            .AddOption(SongStrategy.ForceMB, "ForceMB", "Force switch to Mage's Ballad")
            .AddOption(SongStrategy.ForceAP, "ForceAP", "Force switch to Army's Paeon")
            .AddOption(SongStrategy.ForcePP, "ForcePP", "Force Pitch Perfect (assuming WM is up)", supportedTargets: ActionTargets.Hostile)
            .AddOption(SongStrategy.Delay, "Delay", "Do not use any songs; stay songless if needed")
            .AddAssociatedActions(BRD.AID.WanderersMinuet, BRD.AID.MagesBallad, BRD.AID.ArmysPaeon, BRD.AID.PitchPerfect);

        res.Define(Track.Buffs).As<BuffsStrategy>("Buffs", uiPriority: 95)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay")
            .AddOption(BuffsStrategy.ForceRF, "ForceRF", "Force use Radiant Finale ASAP")
            .AddOption(BuffsStrategy.ForceBV, "ForceBV", "Force use Battle Voice ASAP")
            .AddOption(BuffsStrategy.ForceRS, "ForceRS", "Force use Raging Strikes ASAP")
            .AddAssociatedActions(BRD.AID.RagingStrikes, BRD.AID.BattleVoice, BRD.AID.RadiantFinale);

        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 90)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.Burst, "Burst", "Use right before burst", 270, 30)
            .AddOption(PotionStrategy.Force, "Force", "Use ASAP", 270, 30)
            .AddAssociatedAction(ActionDefinitions.IDPotionDex);

        // TODO: consider multidotting - should it be a separate track (default is primary only, think about interactions with ij etc)? for now user can just do it via planner where it matters
        res.Define(Track.DOTs).As<DotStrategy>("DOTs", uiPriority: 80)
            .AddOption(DotStrategy.Automatic, "Automatic", "Apply dots asap (unless aoe filler is better), reapply when either dots about to expire or in buff window", supportedTargets: ActionTargets.Hostile)
            .AddOption(DotStrategy.ApplyOrExtend, "ApplyOrExtend", "Apply dots asap (even if aoe filler is better), extend existing normally with IJ", supportedTargets: ActionTargets.Hostile)
            .AddOption(DotStrategy.AutomaticExtendOnly, "AutomaticExtendOnly", "Do not apply new dots, extend existing normally with IJ", supportedTargets: ActionTargets.Hostile)
            .AddOption(DotStrategy.Forbid, "Forbid", "Do not apply new or extend existing dots")
            .AddOption(DotStrategy.ForceExtend, "ForceExtend", "Force extend dots via IJ ASAP", supportedTargets: ActionTargets.Hostile)
            .AddOption(DotStrategy.ExtendIgnoreBuffs, "ExtendIgnoreBuffs", "Extend dots via IJ only if they are about to fall off (but don't risk proc overwrites), don't extend early under buffs", supportedTargets: ActionTargets.Hostile)
            .AddOption(DotStrategy.ExtendDelayed, "ExtendDelayed", "Extend dots via IJ at last possible moment, even if it might overwrite proc", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.VenomousBite, BRD.AID.Windbite, BRD.AID.IronJaws, BRD.AID.CausticBite, BRD.AID.Stormbite);

        res.Define(Track.ApexArrow).As<ApexArrowStrategy>("Apex", uiPriority: 70)
            .AddOption(ApexArrowStrategy.Automatic, "Automatic", "Use at 80+ if buffs are about to run off, use at 100 asap unless raid buffs are imminent", supportedTargets: ActionTargets.Hostile)
            .AddOption(ApexArrowStrategy.Delay, "Delay", "Delay")
            .AddOption(ApexArrowStrategy.ForceAnyGauge, "ForceAnyGauge", "Force at any gauge (even if it means no BA)", supportedTargets: ActionTargets.Hostile)
            .AddOption(ApexArrowStrategy.ForceHighGauge, "ForceHighGauge", "Force at 80+ gauge", supportedTargets: ActionTargets.Hostile)
            .AddOption(ApexArrowStrategy.ForceCapGauge, "ForceCapGauge", "Force at 100 gauge (don't delay until raidbuffs)", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.ApexArrow);

        res.Define(Track.BlastArrow).As<OffensiveStrategy>("Blast", uiPriority: -10)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.BlastArrow);

        res.Define(Track.ResonantArrow).As<OffensiveStrategy>("Reso", uiPriority: -20)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.ResonantArrow);

        res.Define(Track.RadiantEncore).As<OffensiveStrategy>("Encore", uiPriority: -30)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.RadiantEncore);

        res.Define(Track.Bloodletter).As<BloodletterStrategy>("BL", uiPriority: 40)
            .AddOption(BloodletterStrategy.Automatic, "Automatic", "Pool for raid buffs, otherwise use freely", supportedTargets: ActionTargets.Hostile)
            .AddOption(BloodletterStrategy.Delay, "Delay", "Do not use, allowing overcap")
            .AddOption(BloodletterStrategy.Force, "Force", "Force use all charges", supportedTargets: ActionTargets.Hostile)
            .AddOption(BloodletterStrategy.KeepOneCharge, "KeepOneCharge", "Keep 1 charge, use if 2+ charges available", supportedTargets: ActionTargets.Hostile)
            .AddOption(BloodletterStrategy.KeepTwoCharges, "KeepTwoCharges", "Keep 2 charges, use if overcap is imminent", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.Bloodletter, BRD.AID.RainOfDeath);

        res.Define(Track.EmpyrealArrow).As<OffensiveStrategy>("EA", uiPriority: 30)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.EmpyrealArrow);

        res.Define(Track.Barrage).As<OffensiveStrategy>("Barrage", uiPriority: 20)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP")
            .AddAssociatedActions(BRD.AID.Barrage);

        res.Define(Track.Sidewinder).As<OffensiveStrategy>("SW", uiPriority: 10)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally", supportedTargets: ActionTargets.Hostile)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP", supportedTargets: ActionTargets.Hostile)
            .AddAssociatedActions(BRD.AID.Sidewinder);

        res.Define(Track.GCDDelay).As<GCDDelayStrategy>("GCDDelay", "GCD", uiPriority: 5)
            .AddOption(GCDDelayStrategy.NoPrepull, "NoPrepull", "Delay first GCD until pull (for better raidbuff timings)")
            .AddOption(GCDDelayStrategy.EarlyPrepull, "EarlyPrepull", "Use first GCD as early as possible, so that it hits boss exactly at countdown end")
            .AddOption(GCDDelayStrategy.Delay, "Delay", "Do not use any GCDs");

        return res;
    }

    public enum GCDPriority
    {
        None = 0,
        Filler = 100, // burst shot / ladonsbite, it's our default fallback when there's nothing better to do
        FlexibleEncore = 410, // Encore is flexible, we should always have opportunity to use it in burst
        FlexibleReso = 420, // Reso is flexible, we should always have opportunity to use it in burst
        FlexibleBA = 430, // BA is flexible, we should always have opportunity to use it in burst
        FlexibleBarrage = 440, // RA to use barrage proc, if hawk eye is not up; quite flexible
        FlexibleIJ = 450, // IJ to refresh dots, can be delayed if necessary (eg to use up hawk eye proc or AA)
        FlexibleHawkEye = 510, // RA to use hawk eye proc (or barrage proc, if hawk eye is also active); flexible, but generally we want to use that before IJ to avoid wasting another proc
        ApplyCausticBite = 520, // initial dot application
        ApplyStormbite = 530, // initial dot application
        ApexArrow = 590, // AA (either at full gauge or if delaying is not possible - either buffs are about to run out or we risk not getting enough gauge for next burst window)
        LastChanceBarrage = 780, // last chance to use triple RA, or barrage will expire
        LastChanceIJ = 790, // last chance to use IJ, or dots will expire
        ForcedIJ = 890, // forced IJ via strategy
    }

    public enum OGCDPriority
    {
        None = 0,
        Barrage = 670, // barrage should simply be used on CD (with even uses inside buff window), but otherwise is flexible
        Sidewinder = 680, // sidewinder should simply be used on CD in buff window, but otherwise is flexible
        Bloodletter = 690, // we don't want to overcap BL, but that can only happen at the beginning of the burst when we pool charges
        Potion = 900, // shouldn't conflict with anything
        EmpyrealArrowAfterSong = 910, // when switching to AP, we might want to delay EA a tiny bit
        Song = 920, // switching songs in time sometimes requires clipping gcds
        PitchPerfect = 930, // PP has to be higher priority than songs, otherwise we can switch to MB without using last stacks
        EmpyrealArrow = 940, // it's extremely important to avoid drifting EA; PP accounts for it, and WM does not generally interfere with it, and other songs can be delayed
        RadiantFinale = 950,
        BattleVoice = 960,
        RagingStrikes = 970,
    }

    public enum Song { None, MagesBallad, ArmysPaeon, WanderersMinuet }

    private Song ActiveSong;
    private float ActiveSongLeft; // 45 max
    private int Repertoire;
    private int SoulVoice;
    private int NumCoda;
    private float GCDLength; // 2.5s adjusted by sks/haste
    private float HawkEyeLeft;
    private float BlastArrowLeft;
    private float RagingStrikesLeft;
    private float BattleVoiceLeft;
    private float RadiantFinaleLeft;
    private float ArmysMuseLeft;
    private float ArmysEthosLeft;
    private float BarrageLeft;
    private float ResonantArrowLeft;
    private float RadiantEncoreLeft;
    private float FullBuffsLeft;
    private float FullBuffsIn;
    private float BloodletterCDTotal; // 30 or 45, depending on max charges
    private float BloodletterCDElapsed; // [0,15) if we have 0 charges, [15,30) if we have 1, [30,45) if we have 2, 45 if we have 3; always <= Total
    private float TargetCausticLeft;
    private float TargetStormbiteLeft;
    private float TargetDotsLeft; // min of two dots

    private bool Unlocked(BRD.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    private bool Unlocked(BRD.TraitID tid) => TraitUnlocked((uint)tid);
    private float CD(BRD.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline; // note: if deadline is 0 (meaning status not active etc), we can't fit a single gcd even if available immediately (GCD==0), so we use <

    // upgrade paths
    public BRD.AID BestBurstShot => Unlocked(BRD.AID.BurstShot) ? BRD.AID.BurstShot : BRD.AID.HeavyShot;
    public BRD.AID BestRefulgentArrow => Unlocked(BRD.AID.RefulgentArrow) ? BRD.AID.RefulgentArrow : BRD.AID.StraightShot;
    public BRD.AID BestCausticBite => Unlocked(BRD.AID.CausticBite) ? BRD.AID.CausticBite : BRD.AID.VenomousBite;
    public BRD.AID BestStormbite => Unlocked(BRD.AID.Stormbite) ? BRD.AID.Stormbite : BRD.AID.Windbite;
    public BRD.AID BestLadonsbite => Unlocked(BRD.AID.Ladonsbite) ? BRD.AID.Ladonsbite : BRD.AID.QuickNock;
    public BRD.AID BestShadowbite => Unlocked(BRD.AID.Shadowbite) ? BRD.AID.Shadowbite : BRD.AID.WideVolley;
    public BRD.AID BestBloodletter => Unlocked(BRD.AID.HeartbreakShot) ? BRD.AID.HeartbreakShot : BRD.AID.Bloodletter;

    // statuses
    public BRD.SID ExpectedCaustic => Unlocked(BRD.AID.CausticBite) ? BRD.SID.CausticBite : BRD.SID.VenomousBite;
    public BRD.SID ExpectedStormbite => Unlocked(BRD.AID.Stormbite) ? BRD.SID.Stormbite : BRD.SID.Windbite;

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var gauge = World.Client.GetGauge<BardGauge>();
        ActiveSong = (Song)((byte)gauge.SongFlags & 3);
        ActiveSongLeft = gauge.SongTimer * 0.001f;
        Repertoire = gauge.Repertoire;
        SoulVoice = gauge.SoulVoice;
        NumCoda = BitOperations.PopCount((uint)gauge.SongFlags & 0x70);
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);

        HawkEyeLeft = SelfStatusLeft(BRD.SID.HawksEye, 30);
        BlastArrowLeft = SelfStatusLeft(BRD.SID.BlastArrowReady, 10);
        RagingStrikesLeft = SelfStatusLeft(BRD.SID.RagingStrikes, 20);
        BattleVoiceLeft = SelfStatusLeft(BRD.SID.BattleVoice, 15);
        RadiantFinaleLeft = SelfStatusLeft(BRD.SID.RadiantFinale, 15);
        ArmysMuseLeft = SelfStatusLeft(BRD.SID.ArmysMuse, 10);
        ArmysEthosLeft = SelfStatusLeft(BRD.SID.ArmysEthos, 30);
        BarrageLeft = SelfStatusLeft(BRD.SID.Barrage, 10);
        ResonantArrowLeft = SelfStatusLeft(BRD.SID.ResonantArrowReady, 30);
        RadiantEncoreLeft = SelfStatusLeft(BRD.SID.RadiantEncoreReady, 30);

        var unlockedBV = Unlocked(BRD.AID.BattleVoice);
        var unlockedRF = Unlocked(BRD.AID.RadiantFinale);
        var cdRS = CD(BRD.AID.RagingStrikes);
        var cdBV = unlockedBV ? CD(BRD.AID.BattleVoice) : float.MaxValue;
        FullBuffsLeft = Math.Min(RagingStrikesLeft, Math.Min(unlockedBV ? BattleVoiceLeft : float.MaxValue, unlockedRF ? RadiantFinaleLeft : float.MaxValue));
        FullBuffsIn = cdRS;

        BloodletterCDTotal = Player.Class == Class.BRD && Unlocked(BRD.TraitID.EnhancedBloodletter) ? 45 : 30;
        ref readonly var bloodletterCD = ref World.Client.Cooldowns[ActionDefinitions.Instance.Spell(BRD.AID.Bloodletter)!.MainCooldownGroup];
        BloodletterCDElapsed = bloodletterCD.Total == 0 ? BloodletterCDTotal : Math.Min(bloodletterCD.Elapsed, BloodletterCDTotal);

        var strategyDOTsOption = strategy.Option(Track.DOTs);
        var dotTarget = ResolveTargetOverride(strategyDOTsOption.Value) ?? primaryTarget;
        TargetCausticLeft = StatusDetails(dotTarget, ExpectedCaustic, Player.InstanceID, 45).Left;
        TargetStormbiteLeft = StatusDetails(dotTarget, ExpectedStormbite, Player.InstanceID, 45).Left;
        TargetDotsLeft = Math.Min(TargetCausticLeft, TargetStormbiteLeft);

        var isUptime = Player.InCombat && primaryTarget != null && !primaryTarget.IsAlly;
        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        var gcdStrategy = strategy.Option(Track.GCDDelay).As<GCDDelayStrategy>();
        var unlockedEA = Unlocked(BRD.AID.EmpyrealArrow);
        var cdEA = unlockedEA ? CD(BRD.AID.EmpyrealArrow) : float.MaxValue;

        // GCD considerations:
        // - during our 2-minute burst, we need to fit 6 gcds into buff window: ij, aa+ba, reso+encore, barrage ra; these are the considerations:
        // -- ij might need to be used asap if dots are about to fall off (eg because there was downtime or something else caused misalignment)
        // -- otherwise ij should be delayed if we have hawk's eye to avoid wasting proc, we should do filler ra first
        // -- otherwise ij should be used early to get buffed dots up asap
        // -- aa needs to be used asap at 100 gauge (to avoid wasting gauge), or delayed otherwise, but not as much as to push ba out of burst
        // -- barrage is up for only 10 seconds, so barrage ra is relatively high priority (lower than last-chance ij or 100-gauge aa, though)
        // -- if both barrage and hawk's eye are up, barrage is consumed first
        // -- ba/reso/encore can be used whenever - and should be delayed in favour of ra if hawk's eye is up and ij was not used yet
        // -- before L100, we simply replace missing actions with more fillers
        // -- before IJ we simply refresh dots as they fall off (TODO: investigate optimizations here)
        // - outside burst, the we only need to consider aa/ba (once, around 1min in) and ij (twice, around 45s and 90s in):
        // -- at 45s, ij should be used at last possible gcd; there's a chance we might lose a hawk eye proc (if we do HS 1 gcd earlier and it procs), but it's less of a loss on average than 1 buffed dot tick
        // -- there's very little chance to have 100 gauge aa vs ij conflict at 45s (previous aa has to be done very early, previous ij very late, all song ticks should proc, to get to this situation), TODO investigate this more
        // -- at 90s, ij can be done a few gcds earlier if needed (eg 1 gcd earlier if there's no hawk eye, etc)
        // -- there are tradeoffs involved in casting aa at <100 gauge (too early and we might end up capping gauge before next burst, too late and we might not get gauge by the end of next burst window)
        // - aoe is generally the same as st with different fillers (profitable at 2+ targets) and no dots application (but still maintaining existing ones via IJ by default, in case we're dealing with short-lived adds in a boss fight)
        // -- ladonsbite potency is 130*N vs 220, shadowbite is 170*N vs 280 - meaning 2+ is profitable
        // -- the only exception is barrage: shadowbite is 270*N vs 280*3, which is only profitable at 4+ targets
        // -- there are some corner cases (eg when we could use ladonsbite at 2+ targets, but shadowbite would only hit 1), we generally don't bother too much with this
        // -- dot potencies are 150+15*20 = 450 and 100 + 15*25 = 475 (assuming full duration) (100+15*15 = 325 and 60 + 15*20 = 360 at low level); roughly this means that ladonsbite is better at 3+ targets and shadowbite at 4+ (TODO: revise this)

        // baseline: filler gcd
        var fillerBetterThanDots = false;
        var haveBarrage = CanFitGCD(BarrageLeft);
        var haveHawkEye = CanFitGCD(HawkEyeLeft);
        if (haveBarrage || haveHawkEye)
        {
            var (aoeBestTarget, aoeTargetCount) = Unlocked(BRD.AID.WideVolley) ? CheckAOETargeting(aoeStrategy, primaryTarget, 25, NumTargetsHitByShadowbite, IsHitByShadowbite) : (null, 0);
            var useAOE = aoeTargetCount >= (haveBarrage ? 4 : 2);
            var target = useAOE ? aoeBestTarget : primaryTarget;
            var priority = haveBarrage && !CanFitGCD(BarrageLeft, 1) ? GCDPriority.LastChanceBarrage : haveHawkEye ? GCDPriority.FlexibleHawkEye : GCDPriority.FlexibleBarrage;
            if (QueueGCDAtHostile(useAOE ? BestShadowbite : BestRefulgentArrow, target, priority, gcdStrategy))
                fillerBetterThanDots = useAOE && aoeTargetCount >= 3;
        }
        else
        {
            // some filler is always possible to use
            var (aoeBestTarget, aoeTargetCount) = Unlocked(BRD.AID.QuickNock) ? CheckAOETargeting(aoeStrategy, primaryTarget, 12, NumTargetsHitByLadonsbite, IsHitByLadonsbite) : (null, 0);
            var useAOE = aoeTargetCount >= 2;
            var target = useAOE ? aoeBestTarget : primaryTarget;
            if (QueueGCDAtHostile(useAOE ? BestLadonsbite : BestBurstShot, target, GCDPriority.Filler, gcdStrategy))
                fillerBetterThanDots = useAOE && aoeTargetCount >= 4;
        }

        // initial dot application & refresh
        // TODO: pre-IJ refresh logic: slightly early but avoiding clipping ticks? early during buffs?
        var strategyDOTs = strategyDOTsOption.As<DotStrategy>();
        var allowApplyDOTs = Hints.FindEnemy(dotTarget)?.ForbidDOTs != true && strategyDOTs switch
        {
            DotStrategy.ApplyOrExtend => true,
            DotStrategy.AutomaticExtendOnly or DotStrategy.Forbid => false,
            _ => !fillerBetterThanDots
        };
        if (allowApplyDOTs && !CanFitGCD(TargetStormbiteLeft) && Unlocked(BRD.AID.Windbite))
            QueueGCDAtHostile(BestStormbite, dotTarget, GCDPriority.ApplyStormbite, gcdStrategy);
        if (allowApplyDOTs && !CanFitGCD(TargetCausticLeft) && Unlocked(BRD.AID.VenomousBite))
            QueueGCDAtHostile(BestCausticBite, dotTarget, GCDPriority.ApplyCausticBite, gcdStrategy);
        if (Unlocked(BRD.AID.IronJaws))
            QueueGCDAtHostile(BRD.AID.IronJaws, dotTarget, IronJawsPriority(strategyDOTs), gcdStrategy);

        // misc gcds
        var strategyAA = strategy.Option(Track.ApexArrow);
        if (ShouldUseApexArrow(strategyAA.As<ApexArrowStrategy>()))
            QueueGCDAtHostile(BRD.AID.ApexArrow, ResolveTargetOverride(strategyAA.Value) ?? primaryTarget, GCDPriority.ApexArrow, gcdStrategy);
        var strategyBA = strategy.Option(Track.BlastArrow);
        if (CanFitGCD(BlastArrowLeft) && strategyBA.As<OffensiveStrategy>() != OffensiveStrategy.Delay)
            QueueGCDAtHostile(BRD.AID.BlastArrow, ResolveTargetOverride(strategyBA.Value) ?? primaryTarget, GCDPriority.FlexibleBA, gcdStrategy);
        var strategyReso = strategy.Option(Track.ResonantArrow);
        if (CanFitGCD(ResonantArrowLeft) && ShouldUseResoEncore(strategyReso.As<OffensiveStrategy>(), ResonantArrowLeft))
            QueueGCDAtHostile(BRD.AID.ResonantArrow, ResolveTargetOverride(strategyReso.Value) ?? primaryTarget, GCDPriority.FlexibleReso, gcdStrategy);
        var strategyEncore = strategy.Option(Track.RadiantEncore);
        if (CanFitGCD(RadiantEncoreLeft) && ShouldUseResoEncore(strategyEncore.As<OffensiveStrategy>(), RadiantEncoreLeft))
            QueueGCDAtHostile(BRD.AID.RadiantEncore, ResolveTargetOverride(strategyEncore.Value) ?? primaryTarget, GCDPriority.FlexibleEncore, gcdStrategy);

        // songs (can only be used in combat)
        var strategySongs = strategy.Option(Track.Songs);
        if (Player.InCombat)
        {
            var song = NextSong(strategySongs.As<SongStrategy>());
            if (song != BRD.AID.None)
            {
                var basePrio = ActionQueue.Priority.Low;
                var delay = 0.0f;
                if (song == BRD.AID.WanderersMinuet && (ActiveSong == Song.ArmysPaeon || ArmysEthosLeft > 0))
                {
                    // outside opener, WM needs to be late-weaved, even if it slightly delays gcd (depending on AP procs, we might need to drift the cycle slightly)
                    // late-weave threshold: with no sks, 2.5 GCD under muse becomes 2.2, we want 5 GCDs to be fast, meaning that max gcd = 10 - 4 * 2.2 = 1.2 (minus safety delta)
                    if (CD(song) < GCD)
                        basePrio = ActionQueue.Priority.High;
                    delay = GCD - 1.0f;
                }
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(song), Player, strategySongs.Priority(basePrio + (int)OGCDPriority.Song), delay: delay);
            }
        }

        // pp
        if (ActiveSong == Song.WanderersMinuet && Repertoire > 0 && ShouldUsePitchPerfect(strategySongs.As<SongStrategy>(), cdEA, estimatedAnimLockDelay))
            QueueOGCDAtHostile(BRD.AID.PitchPerfect, ResolveTargetOverride(strategySongs.Value) ?? primaryTarget, OGCDPriority.PitchPerfect, strategySongs.Value.PriorityOverride);

        // ea
        var strategyEA = strategy.Option(Track.EmpyrealArrow);
        if (unlockedEA && ShouldUseEmpyrealArrow(strategyEA.As<OffensiveStrategy>()))
        {
            var basePrio = AllowClippingGCDByEA(cdEA, estimatedAnimLockDelay) ? ActionQueue.Priority.High : ActionQueue.Priority.Low;
            var deltaPrio = ActiveSong == Song.MagesBallad && cdEA > World.Client.AnimationLock + 0.1f ? OGCDPriority.EmpyrealArrowAfterSong : OGCDPriority.EmpyrealArrow; // allow very slight delay of EA by AP
            QueueOGCDAtHostile(BRD.AID.EmpyrealArrow, ResolveTargetOverride(strategyEA.Value) ?? primaryTarget, deltaPrio, strategyEA.Value.PriorityOverride, basePrio);
        }

        // bloodletter / rain of death
        var strategyBL = strategy.Option(Track.Bloodletter);
        if (Unlocked(BRD.AID.Bloodletter) && ShouldUseBloodletter(strategyBL.As<BloodletterStrategy>(), cdBV))
        {
            var (aoeBestTarget, aoeTargetCount) = Unlocked(BRD.AID.RainOfDeath) ? CheckAOETargeting(aoeStrategy, primaryTarget, 25, NumTargetsHitByRainOfDeath, IsHitByRainOfDeath) : (null, 0);
            var useAOE = aoeTargetCount >= 2; // 100*N vs 130/180
            var target = useAOE ? aoeBestTarget : primaryTarget;
            var basePrio = World.Client.CountdownRemaining > 0 ? ActionQueue.Priority.High : BloodletterCDElapsed + GCDLength < BloodletterCDTotal ? ActionQueue.Priority.VeryLow : ActionQueue.Priority.Low;
            QueueOGCDAtHostile(useAOE ? BRD.AID.RainOfDeath : BestBloodletter, ResolveTargetOverride(strategyBL.Value) ?? target, OGCDPriority.Bloodletter, strategyBL.Value.PriorityOverride, basePrio);
        }

        // barrage
        var strategyBarrage = strategy.Option(Track.Barrage);
        if (Unlocked(BRD.AID.Barrage) && ShouldUseBarrage(strategyBarrage.As<OffensiveStrategy>(), isUptime))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.Barrage), Player, strategyBarrage.Priority(ActionQueue.Priority.Low + (int)OGCDPriority.Barrage));

        // sidewinder
        var strategySidewinder = strategy.Option(Track.Sidewinder);
        if (Unlocked(BRD.AID.Sidewinder) && ShouldUseSidewinder(strategySidewinder.As<OffensiveStrategy>()))
            QueueOGCDAtHostile(BRD.AID.Sidewinder, ResolveTargetOverride(strategySidewinder.Value) ?? primaryTarget, OGCDPriority.Sidewinder, strategySidewinder.Value.PriorityOverride);

        // major buffs: RS/BV/RF/potion
        // normally we want to use them as follows:
        // - during opener (low coda, no muse, everything off cd): after second gcd (first gcd under WM) we do (potion) + late-weaved RF, after third gcd we do BV + late-weaved RS
        // -- RF is smaller value than usual due to low coda
        // -- BV can be slightly delayed so that PP @ 21s remaining on WM is covered (TODO: verify, ensure it won't clip next burst...)
        // - during later bursts (muse): on first gcd under WM we do (potion), on second we do RF + late-weaved BV, on third we do (BL) + late-weaved RS
        // -- potion can be moved to BL spot if BL is not about to cap, but it seems pointless? (won't cover partially buffed filler)
        // -- late-weaved BV should cover PP @ 21s WM
        // -- late-weaved BV & RF should cover 9 gcds each
        // -- we definitely can't triple weave under muse
        // - note that with perfect AP procs BV & RS could very slightly clip gcd
        var strategyBuffs = strategy.Option(Track.Buffs);
        var strategyBuffsVal = strategyBuffs.As<BuffsStrategy>();
        if (unlockedRF && NumCoda > 0 && ShouldUseRadiantFinale(strategyBuffsVal, isUptime))
        {
            // late-weave RF in opener, so that we cover ogcd slot immediately after EA
            // slightly delay RF in burst, so that we cover 9 gcds (bare minimum is 2 * (0.6+animlockdelay))
            var idealTimeBeforeGCD = ArmysMuseLeft == 0 ? 0.8f : 1.3f;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.RadiantFinale), Player, strategyBuffs.Priority(ActionQueue.Priority.Medium + (int)OGCDPriority.RadiantFinale), delay: GCD - idealTimeBeforeGCD);
        }

        if (unlockedBV && ShouldUseBattleVoice(strategyBuffsVal, isUptime))
        {
            // TODO: do we want a small delay in opener (for PP @ 21s)? or does buff application delay takes care of that? it might make BV clip gcd on next burst...
            // TODO: do we want to explicitly delay in burst? RF should handle that, but what about low levels?
            // TODO: L70 (<78) doesn't even have muse, L80 (78-89) has muse but no RF
            var basePrio = AllowClippingGCDByBuffs(cdBV) ? ActionQueue.Priority.High : ActionQueue.Priority.Medium;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.BattleVoice), Player, strategyBuffs.Priority(basePrio + (int)OGCDPriority.BattleVoice));
        }

        if (Unlocked(BRD.AID.RagingStrikes) && ShouldUseRagingStrikes(strategyBuffsVal, isUptime))
        {
            // late-weave under muse, so that we cover 9th gcd, and so that we can early-weave bloodletter
            // during opener, we want to delay it ever so slightly, so that first EA is pushed to after GCD (otherwise it might not catch the buff)
            var idealTimeBeforeGCD = ArmysMuseLeft == 0 ? 1.1f : 0.8f;
            var basePrio = AllowClippingGCDByBuffs(cdRS) ? ActionQueue.Priority.High : ActionQueue.Priority.Medium;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.RagingStrikes), Player, strategyBuffs.Priority(basePrio + (int)OGCDPriority.RagingStrikes), delay: GCD - idealTimeBeforeGCD);
        }

        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>(), isUptime))
        {
            // late-weave under muse (TODO: verify timings)
            var delay = ArmysMuseLeft == 0 ? 0 : GCD - 1.0f;
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Low + (int)OGCDPriority.Potion, delay: delay);
        }

        // ai hints for positioning - ladonsbite is the most restrictive generally
        var goalST = primaryTarget != null ? Hints.GoalSingleTarget(primaryTarget, 3) : null;
        var goalAOE = primaryTarget != null ? Hints.GoalAOECone(primaryTarget, 12, 45.Degrees()) : null;
        var goal = aoeStrategy switch
        {
            AOEStrategy.SingleTarget => goalST,
            AOEStrategy.ForceAOE => goalAOE,
            _ => goalST != null && goalAOE != null ? Hints.GoalCombined(goalST, goalAOE, 2) : goalAOE
        };
        if (goal != null)
            Hints.GoalZones.Add(goal);
    }

    // TODO: consider moving to class definitions and autogenerating?
    private float EffectApplicationDelay(BRD.AID aid) => aid switch
    {
        BRD.AID.BurstShot => 1.45f,
        BRD.AID.RefulgentArrow => 1.45f,
        BRD.AID.Stormbite => 1.3f,
        BRD.AID.CausticBite => 1.3f,
        BRD.AID.IronJaws => 0.6f,
        BRD.AID.ApexArrow => 1.05f,
        BRD.AID.BlastArrow => 1.65f,
        BRD.AID.Bloodletter => 1.6f,
        BRD.AID.HeartbreakShot => 1.65f,
        BRD.AID.RainOfDeath => 1.65f,
        BRD.AID.EmpyrealArrow => 1.0f,
        BRD.AID.PitchPerfect => 0.8f,
        BRD.AID.Sidewinder => 0.55f,
        BRD.AID.RagingStrikes => 0.5f,
        BRD.AID.BattleVoice => 0.6f,
        BRD.AID.RadiantFinale => 0.6f,
        _ => 0
    };

    private int NumTargetsHitByLadonsbite(Actor primary) => Hints.NumPriorityTargetsInAOECone(Player.Position, 12, (primary.Position - Player.Position).Normalized(), 45.Degrees());
    private int NumTargetsHitByShadowbite(Actor primary) => Hints.NumPriorityTargetsInAOECircle(primary.Position, 4);
    private int NumTargetsHitByRainOfDeath(Actor primary) => Hints.NumPriorityTargetsInAOECircle(primary.Position, 8);
    private bool IsHitByLadonsbite(Actor primary, Actor check) => Hints.TargetInAOECone(check, Player.Position, 12, (primary.Position - Player.Position).Normalized(), 45.Degrees());
    private bool IsHitByShadowbite(Actor primary, Actor check) => Hints.TargetInAOECircle(check, primary.Position, 5);
    private bool IsHitByRainOfDeath(Actor primary, Actor check) => Hints.TargetInAOECircle(check, primary.Position, 8);

    private (Actor?, int) CheckAOETargeting(AOEStrategy strategy, Actor? primaryTarget, float range, Func<Actor, int> numTargets, Func<Actor, Actor, bool> check) => strategy switch
    {
        AOEStrategy.AutoTargetHitPrimary => FindBetterTargetBy(primaryTarget, range, t => primaryTarget == null || check(t, primaryTarget) ? numTargets(t) : 0),
        AOEStrategy.AutoTargetHitMost => FindBetterTargetBy(primaryTarget, range, numTargets),
        AOEStrategy.AutoOnPrimary => (primaryTarget, primaryTarget != null ? numTargets(primaryTarget) : 0),
        AOEStrategy.ForceAOE => (primaryTarget, int.MaxValue),
        _ => (null, 0)
    };

    private bool QueueAtHostile(BRD.AID aid, Actor? target, float prio, float maxPrepull)
    {
        if (target != null && !target.IsAlly)
        {
            var delay = !Player.InCombat && World.Client.CountdownRemaining > 0 ? World.Client.CountdownRemaining.Value - maxPrepull : 0;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, prio, delay: delay);
            return true;
        }
        return false;
    }

    private bool QueueGCDAtHostile(BRD.AID aid, Actor? target, GCDPriority prio, GCDDelayStrategy strategy)
        => strategy != GCDDelayStrategy.Delay && prio != GCDPriority.None && QueueAtHostile(aid, target, ActionQueue.Priority.High + (int)prio, strategy == GCDDelayStrategy.NoPrepull ? 0 : EffectApplicationDelay(aid));
    private bool QueueOGCDAtHostile(BRD.AID aid, Actor? target, OGCDPriority prio, float prioOverride, float basePrio = ActionQueue.Priority.Low)
        => prio != OGCDPriority.None && QueueAtHostile(aid, target, float.IsNaN(prioOverride) ? basePrio + (int)prio : prioOverride, EffectApplicationDelay(aid));

    // heuristic to determine whether currently active dots were applied under raidbuffs (assumes dots are actually active)
    // it's not easy to directly determine active dot potency
    private bool AreActiveDOTsBuffed()
    {
        // dots last for 45s => their time of application Td = t + dotsLeft - 45
        // assuming we're using RS as the main buff, its cd is 120 => it was last used at Ts = t + rscd - 120, and lasted until Te = Ts + 20
        // so dots are buffed if Ts < Td < Te => t + rscd - 120 < t + dotsLeft - 45 < t + rscd - 100 => -75 < dotsLeft - rscd < -55
        // this works when RS is off cd (dotsLeft < -55 is always false)
        // this doesn't really work if dots are not up (can return true if rscd is between 75 and 55)
        return TargetDotsLeft - FullBuffsIn is > -75 and < -55;
    }

    // IJ generally has to be used at last possible gcd before dots fall off -or- before major buffs fall off (to snapshot buffs to dots), but in some cases we want to use it earlier:
    // - 1 gcd earlier if we don't have RA proc (otherwise we might use filler, it would proc RA, then on next gcd we'll have to use IJ to avoid dropping dots and potentially waste another RA)
    // - 1/2 gcds earlier if we're waiting for more gauge for AA
    private GCDPriority IronJawsPriority(DotStrategy strategy)
    {
        if (strategy == DotStrategy.Forbid || !CanFitGCD(TargetDotsLeft))
            return GCDPriority.None; // we can't IJ
        if (strategy == DotStrategy.ForceExtend)
            return GCDPriority.ForcedIJ; // we're forced to IJ
        if (!CanFitGCD(TargetDotsLeft, 1))
            return GCDPriority.LastChanceIJ; // last chance to IJ
        if (AreActiveDOTsBuffed())
            return GCDPriority.None; // never extend buffed dots early: we obviously don't want to use multiple IJs in a single buff window, and outside buff window we don't want to overwrite buffed ticks, even if that means risking losing a proc
        if (strategy == DotStrategy.ExtendDelayed)
            return GCDPriority.None; // early extend is forbidden by strategy

        // ok, dots aren't falling off imminently, and they are not buffed - see if we want to ij early and overwrite last ticks
        if (strategy != DotStrategy.ExtendIgnoreBuffs && CanFitGCD(FullBuffsLeft))
            return GCDPriority.FlexibleIJ; // we're inside buff window, so use ij when we don't have anything higher priority (eg using hawk eye proc or 100-gauge AA)
        // finally, outside buff window refresh up to 2 gcds earlier - this gives us chance to use higher priority stuff (hawk eye/100-gauge AA) if needed
        // TODO: this isn't really a concern before L76 - we don't have AA and IJ can't proc hawk's eye..
        return CanFitGCD(TargetDotsLeft, 3) ? GCDPriority.None : GCDPriority.FlexibleIJ;
    }

    // you get 5 gauge for every repertoire tick, meaning every 15s you get 5 gauge from EA + up to 25 gauge (*80% = 20 average) from songs
    // using AA at 80+ gauge procs BA, meaning AA at <80 gauge is rarely worth it
    // note that if AA is not unlocked, soul voice gauge is always 0
    private bool ShouldUseApexArrow(ApexArrowStrategy strategy) => strategy switch
    {
        ApexArrowStrategy.Delay => false,
        ApexArrowStrategy.ForceAnyGauge => SoulVoice > 0,
        ApexArrowStrategy.ForceHighGauge => SoulVoice >= 80,
        ApexArrowStrategy.ForceCapGauge => SoulVoice >= 100,
        _ => SoulVoice switch
        {
            >= 100 => FullBuffsIn >= GCD + 45, // use asap, unless we are unlikely to have 80+ gauge by the next buff window (TODO: reconsider time limit)
            >= 80 => CanFitGCD(FullBuffsLeft)
                ? !CanFitGCD(FullBuffsLeft, 2) // under buffs, don't delay AA if doing that will make BA miss buffs
                : FullBuffsIn - GCD is >= 45 and < 55, // outside buffs, delay unless we risk entering a window where next buffs are imminent and we can't AA (TODO: reconsider window size)
            _ => false // never use AA at <80 gauge automatically; assume manual planning for things like end-of-fight or downtimes
        }
    };

    // normally we want to use burst gcds (reso/encore) under full buff stack, or if buffs aren't happening any time soon
    private bool ShouldUseResoEncore(OffensiveStrategy strategy, float remainingWindow) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => CanFitGCD(FullBuffsLeft) || FullBuffsIn >= remainingWindow,
    };

    private float SwitchAtRemainingSongTimer(SongStrategy strategy) => strategy switch
    {
        SongStrategy.Extend => 3,
        SongStrategy.Overextend => 0, // TODO: think more about it...
        _ => ActiveSong switch
        {
            Song.WanderersMinuet => 3, // WM->MB transition when no more repertoire ticks left
            Song.MagesBallad => 6, // MB->AP transition generally wants to be delayed as much as possible, but losing last tick leads to better 120s cycle (3-6-9 rotation)
            Song.ArmysPaeon => Repertoire == 4 ? 9 : 3, // AP->WM transition asap if we have full repertoire; note that if there's micro downtime, we should delay a bit rather switch at e.g. 10s, as then we might not have BV/RS available when needed
            _ => 3
        },
    };

    // if no song is up, we want best available (unlocked and with smallest cooldown, WM>MB>AP if multiple are off cooldown)
    private BRD.AID BestAvailableSong()
    {
        var cdWM = Unlocked(BRD.AID.WanderersMinuet) ? CD(BRD.AID.WanderersMinuet) : float.MaxValue;
        var cdMB = Unlocked(BRD.AID.MagesBallad) ? CD(BRD.AID.MagesBallad) : float.MaxValue;
        var cdAP = Unlocked(BRD.AID.ArmysPaeon) ? CD(BRD.AID.ArmysPaeon) : float.MaxValue;
        var cdMin = Math.Min(cdWM, Math.Min(cdMB, cdAP)); // == MaxValue if no songs unlocked
        return cdMin == float.MaxValue ? BRD.AID.None : cdMin == cdWM ? BRD.AID.WanderersMinuet : cdMin == cdMB ? BRD.AID.MagesBallad : BRD.AID.ArmysPaeon;
    }

    private BRD.AID NextSongInCycle(SongStrategy strategy)
    {
        if (!Unlocked(BRD.AID.WanderersMinuet))
            return BRD.AID.None; // before WM, we don't really have a song cycle - each song needs to run to completion
        if (ActiveSongLeft >= SwitchAtRemainingSongTimer(strategy) - 0.1f)
            return BRD.AID.None; // we want to continue current song; TODO: rethink this extra leeway, we want to make sure we use up last tick's repertoire e.g. in WM
        var (nextSong, secondSong) = ActiveSong switch
        {
            Song.WanderersMinuet => (BRD.AID.MagesBallad, BRD.AID.ArmysPaeon),
            Song.MagesBallad => (BRD.AID.ArmysPaeon, BRD.AID.WanderersMinuet),
            Song.ArmysPaeon => (BRD.AID.WanderersMinuet, BRD.AID.MagesBallad),
            _ => default
        };
        return CD(secondSong) < 45 ? nextSong : BRD.AID.None; // switch early only if we won't end up songless after next song ends
    }

    private BRD.AID NextSong(SongStrategy strategy) => strategy switch
    {
        SongStrategy.ForceWM => BRD.AID.WanderersMinuet,
        SongStrategy.ForceMB => BRD.AID.MagesBallad,
        SongStrategy.ForceAP => BRD.AID.ArmysPaeon,
        SongStrategy.Delay => BRD.AID.None,
        _ => ActiveSong == Song.None ? BestAvailableSong() : NextSongInCycle(strategy)
    };

    private bool ShouldUsePitchPerfect(SongStrategy strategy, float eaCD, float animLockDelay)
    {
        if (strategy == SongStrategy.ForcePP || Repertoire == 3)
            return true; // forced by strategy or at max stacks, using it is a no-brainer
        if (ActiveSongLeft < 3)
            return true; // we don't have any more ticks left, so spend what we have before leaving WM

        // we want to use PPx under buffs; note that we use RS time for condition, other buffs might fall off earlier, and it's not worth double-burning or using PP earlier
        var nextProcIn = ActiveSongLeft % 3.0f;
        if (RagingStrikesLeft > World.Client.AnimationLock && RagingStrikesLeft <= nextProcIn + 1)
            return true; // PPx if we won't get any more stacks under buffs

        // if we're at PP2 and EA is about to come off cd, we might be in a situation where waiting for PP3 would have us choose between delaying EA or wasting its guaranteed proc; in such case we want to PP2 early
        if (Repertoire == 2)
        {
            // we never want to delay EA for PP (but we might delay it a bit for GCD)
            if (CanFitGCD(eaCD, 1))
                return false; // we have at least 2 gcds before EA, plenty of opportunities to do PP later

            var animLockSlot = 0.6f + animLockDelay;
            if (eaCD + animLockSlot > GCD && eaCD < GCD + animLockSlot && !AllowClippingGCDByEA(eaCD, animLockDelay))
                eaCD = GCD + animLockSlot;

            // see if we can fit PP between proc and EA; if EA is first, earliest we can cast PP is after EA anim lock ends; if proc is first, PP's anim lock has to finish before EA is off cd
            var (ppWindowStart, ppWindowEnd) = eaCD < nextProcIn ? (eaCD + animLockSlot, nextProcIn) : (nextProcIn, eaCD - animLockSlot);
            if (ppWindowStart > ppWindowEnd)
                return true; // window is non-existent

            // see whether window overlaps GCD
            if (GCD < ppWindowStart + animLockSlot)
                ppWindowStart = Math.Max(ppWindowStart, GCD + animLockSlot); // it's not possible to fit PP before GCD, earliest is after
            if (GCD + animLockSlot > ppWindowEnd)
                ppWindowEnd = Math.Min(ppWindowEnd, GCD - animLockSlot); // it's not possible to fit PP after GCD, latest is before
            return ppWindowStart > ppWindowEnd; // use PP2 if there's no window after accounting for GCD
        }

        return false; // no reason to use PPx
    }

    private bool AllowClippingGCDByEA(float cdEA, float animLockDelay) => false;
    private bool AllowClippingGCDByBuffs(float cd) => cd + 0.3f <= GCD;

    // by default, we use EA asap if song is up and if we're not delaying for buffs
    private bool ShouldUseEmpyrealArrow(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => ActiveSong != Song.None && FullBuffsIn > 0
    };

    private bool PreferPoolingBloodletters(float cdBV)
    {
        if (FullBuffsLeft > 0)
            return false; // buffs up, no point delaying
        var capIn = BloodletterCDTotal - BloodletterCDElapsed;
        if (BattleVoiceLeft > 0)
            return CanFitGCD(capIn - 0.6f, 1); // partial buffs up: use before RS only if cap is imminent (next gcd after RS is filled with EA+PP)
        // no buffs - pool if it won't cap before next BV
        return capIn > cdBV;
    }

    // by default, we pool bloodletter for burst
    private bool ShouldUseBloodletter(BloodletterStrategy strategy, float cdBV) => strategy switch
    {
        BloodletterStrategy.Delay => false,
        BloodletterStrategy.Force => true,
        BloodletterStrategy.KeepOneCharge => BloodletterCDElapsed + World.Client.AnimationLock >= 30,
        BloodletterStrategy.KeepTwoCharges => BloodletterCDElapsed + World.Client.AnimationLock >= 45,
        _ => !PreferPoolingBloodletters(cdBV)
    };

    // by default, we use barrage under raid buffs, but not in downtime
    private bool ShouldUseBarrage(OffensiveStrategy strategy, bool isUptime) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => isUptime && FullBuffsLeft > 0
    };

    // by default, we use sidewinder asap, unless raid buffs are imminent
    private bool ShouldUseSidewinder(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => FullBuffsIn > 45 // TODO: consider exact delay condition
    };

    // by default, use RF under WM (1-2 gcds later, depending on muse), and if all buffs can be cast by the end of next gcd
    // if player did not unlock WM by the time he gets RF, too fucking bad
    private bool ShouldUseRadiantFinale(BuffsStrategy strategy, bool isUptime) => strategy switch
    {
        BuffsStrategy.Automatic => isUptime && WantRF(),
        BuffsStrategy.ForceRF => true,
        _ => false
    };

    // by default, if RF is unlocked, we want to cast BV asap after RF; otherwise if WM is unlocked, we want to cast it 2 gcds after WM; otherwise we just cast it asap under MB
    private bool ShouldUseBattleVoice(BuffsStrategy strategy, bool isUptime) => strategy switch
    {
        BuffsStrategy.Automatic => isUptime && WantBV(),
        BuffsStrategy.ForceBV => true,
        _ => false
    };

    // by default, if BV is unlocked, we want to cast RF asap after BV; otherwise just use it asap (under MB, if unlocked)
    private bool ShouldUseRagingStrikes(BuffsStrategy strategy, bool isUptime) => strategy switch
    {
        BuffsStrategy.Automatic => isUptime && WantRS(),
        BuffsStrategy.ForceRS => true,
        _ => false
    };

    // normally we use potion on next gcd after WM
    private bool ShouldUsePotion(PotionStrategy strategy, bool isUptime) => strategy switch
    {
        PotionStrategy.Manual => false,
        PotionStrategy.Burst => isUptime && ActiveSong == Song.WanderersMinuet && FullBuffsIn < 10 && MinGCDsSinceSongStart(1),
        PotionStrategy.Force => true,
        _ => false
    };

    // true if GCD seconds in future, time since song start will be greater than N gcd lengths
    private bool MinGCDsSinceSongStart(int numGCDs) => 45 - ActiveSongLeft + GCD > numGCDs * GCDLength;
    // buff application logic (note that we use cooldowns instead of status timings, because of status application delay)
    private bool WantRF() => ActiveSong == Song.WanderersMinuet && MinGCDsSinceSongStart(ArmysMuseLeft == 0 ? 1 : 2) && !CanFitGCD(FullBuffsIn, 1);
    private bool WantBV() => Unlocked(BRD.AID.RadiantFinale) ? CD(BRD.AID.RadiantFinale) > 90 : Unlocked(BRD.AID.WanderersMinuet) ? ActiveSong == Song.WanderersMinuet && MinGCDsSinceSongStart(2) : ActiveSong == Song.MagesBallad;
    private bool WantRS() => Unlocked(BRD.AID.BattleVoice) ? CD(BRD.AID.BattleVoice) > 100 : ActiveSong == Song.MagesBallad || !Unlocked(BRD.AID.MagesBallad);
}
