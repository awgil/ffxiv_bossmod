#if false
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;

public sealed class StandardWAR(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { AOE, Burst, Potion, PrimalRend, Tomahawk, Infuriate, InnerRelease, Upheaval, Onslaught }
    public enum AOEStrategy { SingleTarget, ForceAOE, Auto, AutoFinishCombo }
    public enum BurstStrategy { Automatic, Spend, Conserve, UnderRaidBuffs, UnderPotion, ForceExtendST, IgnoreST }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, AlignWithIR, Immediate }
    public enum PrimalRendStrategy { Automatic, Forbid, Force, GapClose, Smuggle }
    public enum TomahawkStrategy { Opener, Forbid, Force, Ranged }
    //public enum GCDStrategy { Automatic, ForceSPCombo, TomahawkIfNotInMelee, ComboFitBeforeDowntime, PenultimateComboThenSpend }
    //public enum InfuriateStrategy { Automatic, Delay, ForceIfNoNC, AutoUnlessIR, ForceIfChargesCapping }
    //public enum OffensiveStrategy { Automatic, Delay, Force }
    //public enum OnslaughtStrategy { Automatic, Forbid, NoReserve, Force, ForceReserve, ReserveTwo, UseOutsideMelee }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense (ST stuff, esp things like onslaught?)
        var res = new RotationModuleDefinition("Legacy WAR", "Old pre-refactoring module", "veyn", RotationModuleQuality.WIP, BitMask.Build((int)Class.WAR), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 90)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target rotation")
            .AddOption(AOEStrategy.ForceAOE, "AOE", "Use aoe rotation")
            .AddOption(AOEStrategy.Auto, "Auto", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; break combo if necessary")
            .AddOption(AOEStrategy.AutoFinishCombo, "AutoFinishCombo", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; finish combo route before switching");

        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 80)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Spend gauge under potion/raidbuffs, otherwise conserve")
            .AddOption(BurstStrategy.Spend, "Spend", "Spend gauge freely (as if inside burst window), ensure ST is properly maintained (useful at the end of the fight)")
            .AddOption(BurstStrategy.Conserve, "Conserve", "Conserve gauge as much as possible (as if outside burst window)")
            .AddOption(BurstStrategy.UnderRaidBuffs, "UnderRaidBuffs", "Spend gauge under raidbuffs, otherwise conserve; ignore potion (useful if potion is delayed)")
            .AddOption(BurstStrategy.UnderPotion, "UnderPotion", "Spend gauge under potion, otherwise conserve; ignore raidbuffs (useful for misaligned potions)")
            .AddOption(BurstStrategy.ForceExtendST, "ForceExtendST", "Force extend ST buff by the end of the window, potentially overcapping gauge and/or ST")
            .AddOption(BurstStrategy.IgnoreST, "IgnoreST", "Aggressively spend gauge, disregarding ST buff, even if is not up/running out soon (useful before long downtimes)");

        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 70)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with 2-minute raid buffs (0/6, 2/8, etc)", 270, 30)
            .AddOption(PotionStrategy.AlignWithIR, "AlignWithIR", "Align with IR, allow on odd minutes (0/5/10)")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, even if without ST and with IR on cd (0/4:30/9)", 270, 30)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        res.Define(Track.PrimalRend).As<PrimalRendStrategy>("PR", uiPriority: 30)
            .AddOption(PrimalRendStrategy.Automatic, "Automatic", "Use normally: rend only when already in melee range, delay until raidbuffs if possible", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.Forbid, "Forbid", "Do not use automatically", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.Force, "Force", "Force use ASAP", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.GapClose, "GapClose", "Use Rend if outside melee range as a gap-closer", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.Smuggle, "Smuggle", "Delay until last possible GCD (useful for smuggling ruin into pot window)", 0, 0, ActionTargets.Hostile, 90)
            .AddAssociatedActions(WAR.AID.PrimalRend, WAR.AID.PrimalRuination);

        res.Define(Track.Tomahawk).As<TomahawkStrategy>("Tomahawk", uiPriority: -10)
            .AddOption(TomahawkStrategy.)
            .AddAssociatedActions(WAR.AID.Tomahawk);

        //res.Define(Track.GCD).As<GCDStrategy>("Gauge", "GCD", uiPriority: 80)
        //    .AddOption(GCDStrategy.Automatic, "Automatic", "Spend gauge either under raid buffs or if next downtime is soon (so that next raid buff window won't cover at least 4 GCDs)") // TODO reconsider...
        //    .AddOption(GCDStrategy.Spend, "Spend", "Spend gauge freely, ensure ST is properly maintained")
        //    .AddOption(GCDStrategy.ConserveIfNoBuffs, "ConserveIfNoBuffs", "Conserve unless under raid buffs")
        //    .AddOption(GCDStrategy.Conserve, "Conserve", "Conserve as much as possible")
        //    .AddOption(GCDStrategy.ForceExtendST, "ForceExtendST", "Force extend ST buff, potentially overcapping gauge and/or ST")
        //    .AddOption(GCDStrategy.ForceSPCombo, "ForceSPCombo", "Force SP combo, potentially overcapping gauge")
        //    .AddOption(GCDStrategy.TomahawkIfNotInMelee, "TomahawkIfNotInMelee", "Use tomahawk if outside melee")
        //    .AddOption(GCDStrategy.ComboFitBeforeDowntime, "ComboFitBeforeDowntime", "Use combo, unless it can't be finished before downtime and unless gauge and/or ST would overcap")
        //    .AddOption(GCDStrategy.PenultimateComboThenSpend, "PenultimateComboThenSpend", "Use combo until second-last step, then spend gauge")
        //    .AddOption(GCDStrategy.ForceSpend, "ForceSpend", "Force gauge spender if possible, even if ST is not up/running out soon");

        //res.Define(Track.Infuriate).As<InfuriateStrategy>("Infuriate", uiPriority: 70)
        //    .AddOption(InfuriateStrategy.Automatic, "Automatic", "Try to delay uses until raidbuffs, avoiding overcap")
        //    .AddOption(InfuriateStrategy.Delay, "Delay", "Delay, even if risking overcap")
        //    .AddOption(InfuriateStrategy.ForceIfNoNC, "ForceIfNoNC", "Force unless NC active")
        //    .AddOption(InfuriateStrategy.AutoUnlessIR, "AutoUnlessIR", "Use normally, but not during IR")
        //    .AddOption(InfuriateStrategy.ForceIfChargesCapping, "ForceIfChargesCapping", "Force use if charges are about to overcap (unless NC is already active), even if it would overcap gauge")
        //    .AddAssociatedActions(WAR.AID.Infuriate);



        //res.Define(Track.InnerRelease).As<OffensiveStrategy>("IR", uiPriority: 50)
        //    .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
        //    .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
        //    .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even during downtime or without ST)")
        //    .AddAssociatedActions(WAR.AID.Berserk, WAR.AID.InnerRelease);

        //res.Define(Track.Upheaval).As<OffensiveStrategy>("Upheaval", uiPriority: 40)
        //    .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally")
        //    .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
        //    .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even without ST)")
        //    .AddAssociatedActions(WAR.AID.Upheaval, WAR.AID.Orogeny);

        //res.Define(Track.Onslaught).As<OnslaughtStrategy>("Onslaught", uiPriority: 20)
        //    .AddOption(OnslaughtStrategy.Automatic, "Automatic", "Always keep one charge reserved, use other charges under raidbuffs or to prevent overcapping")
        //    .AddOption(OnslaughtStrategy.Forbid, "Forbid", "Forbid automatic use")
        //    .AddOption(OnslaughtStrategy.NoReserve, "NoReserve", "Do not reserve charges: use all charges if under raidbuffs, otherwise use as needed to prevent overcapping")
        //    .AddOption(OnslaughtStrategy.Force, "Force", "Use all charges ASAP")
        //    .AddOption(OnslaughtStrategy.ForceReserve, "ForceReserve", "Use all charges except one ASAP")
        //    .AddOption(OnslaughtStrategy.ReserveTwo, "ReserveTwo", "Reserve 2 charges, trying to prevent overcap")
        //    .AddOption(OnslaughtStrategy.UseOutsideMelee, "UseOutsideMelee", "Use as gapcloser if outside melee range")
        //    .AddAssociatedActions(WAR.AID.Onslaught);

        // TODO: consider these:
        //public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net
        //public bool OnslaughtHeadroom; // if true, consider onslaught to have slightly higher animation lock than in reality, to account for potential small movement animation delay

        return res;
    }

    public enum GCDPriority
    {
        None = 0,
        // actions that we prefer to delay, but they can be used if we have no better ideas
        DelayPR = 300, // we see an opportunity to delay PR until next burst, we'd really rather not press it...
        DelayCombo = 350, // we'd rather not use combo now, to prevent overcapping gauge/ST
        DelayFC = 390, // we see an opportunity to delay FC/IC until next burst (but might have to press it anyway to avoid overcapping gauge)
        // baseline flexible actions
        FlexibleCombo = 500, // normal flexible combo
        FlexibleFC = 580, // using FC/IC now is low value (not buffed), but we don't see any better opportunity to use it in future
        FlexiblePR = 590, // using PR now is low value (not buffed), but we don't see any better opportunity to use it in future
        // high value actions (under buffs)
        BuffedFC = 680,
        BuffedPR = 690,
        // critical actions (avoid overcapping gauge/infuriate, maintain ST, etc)
        AvoidDropCombo = 760, // if we don't continue combo, we'll lose it
        AvoidOvercapInfuriateIR = 770, // if we delay FC/IC, we'll be forced to spam IC/FC later to avoid losing IR stacks, which will overcap infuriate
        AvoidOvercapInfuriateNext = 780, // if we delay FC/IC, infuriate will overcap when it's used later
        AvoidDropST = 790, // if we delay combo, we will be dropping ST
        // last-chance priorities: if corresponding action is not used on next gcd, we will likely lose an opportunity
        LastChanceFC = 870, // last chance to use IC/FC (otherwise IR will expire)
        LastChanceIC = 880, // last chance to use IC (otherwise nascent chaos will expire)
        LastChancePR = 890, // last chance to use PR (otherwise buff will expire)
        // forced actions are explicitly requested by strategies, so we assume user knows what he's doing
        ForcedCombo = 980, // need to use combo action asap
        ForcedPR = 990, // need to use PR asap (forced strategy or as a gapcloser)
    }

    private int Gauge; // 0 to 100
    private float GCDLength; // 2.5s adjusted by sks/haste
    private float SurgingTempestLeft; // max 60
    private float NascentChaosLeft; // max 30
    private float PRLeft; // max 30 (rend) or 20 (ruination)
    private bool PrimalRuinationActive;
    private float WrathfulLeft; // max 30
    private bool InnerReleaseUnlocked;
    private float InnerReleaseLeft; // max 15 (either IR or berserk)
    private int InnerReleaseStacks; // max 3 (either IR or berserk)
    private float PotionLeft; // max 30
    private float RaidBuffsLeft; // max 20
    private float RaidBuffsIn;
    private float BurstWindowLeft;
    private float BurstWindowIn;

    private bool Unlocked(WAR.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    private bool Unlocked(WAR.TraitID tid) => TraitUnlocked((uint)tid);
    private float CD(WAR.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline; // note: if deadline is 0 (meaning status not active etc), we can't fit a single gcd even if available immediately (GCD==0), so we use <
    private WAR.AID PrevCombo => (WAR.AID)World.Client.ComboState.Action;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn)
    {
        Gauge = GetGauge<WarriorGauge>().BeastGauge;
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        SurgingTempestLeft = SelfStatusLeft(WAR.SID.SurgingTempest);
        NascentChaosLeft = SelfStatusLeft(WAR.SID.NascentChaos);
        PRLeft = SelfStatusLeft(WAR.SID.PrimalRuinationReady);
        if (PRLeft > 0)
            PrimalRuinationActive = true;
        else
            PRLeft = SelfStatusLeft(WAR.SID.PrimalRend);
        WrathfulLeft = SelfStatusLeft(WAR.SID.Wrathful);
        InnerReleaseUnlocked = Unlocked(WAR.AID.InnerRelease);
        (InnerReleaseLeft, InnerReleaseStacks) = SelfStatusDetails(InnerReleaseUnlocked ? WAR.SID.InnerRelease : WAR.SID.Berserk);
        PotionLeft = PotionStatusLeft();
        RaidBuffsLeft = Bossmods.RaidCooldowns.DamageBuffLeft(Player);
        RaidBuffsIn = Bossmods.RaidCooldowns.NextDamageBuffIn();

        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        var aoeTargets = aoeStrategy switch
        {
            AOEStrategy.SingleTarget => NumTargetsHitByAOE() > 0 ? 1 : 0,
            AOEStrategy.ForceAOE => NumTargetsHitByAOE() > 0 ? 100 : 0,
            _ => NumTargetsHitByAOE()
        };

        // burst (raid buff) windows are normally 20s every 120s (barring downtimes, deaths, etc)
        // potions are 30s, usually they are aligned to overlap; we generally pot early (to allow ruin smuggle), in such case we consider first potion gcds to be non-burst (and so conserve gauge)
        // however, we might also be potting outside raidbuffs (0/5/10 or even 0/4:30/9), in such case we prefer bursting in potion window rather than in raidbuff window
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),
            BurstStrategy.Spend or BurstStrategy.IgnoreST => (0, float.MaxValue),
            BurstStrategy.Conserve or BurstStrategy.ForceExtendST => (0, 0), // 'in' is 0, meaning 'raid buffs are imminent, but not yet active, so delay everything'
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),
            _ => (0, 0)
        };

        // GCDs - we have a choice between combos, FC/IC, PRend/PRuin and tomahawk
        if (CanFitGCD(PRLeft))
        {
            var pr = strategy.Option(Track.PrimalRend);
            var target = ResolveTargetOverride(pr.Value) ?? primaryTarget;
            var prio = PRPriority(pr.As<PrimalRendStrategy>(), target);
            QueueGCD(PrimalRuinationActive ? WAR.AID.PrimalRend : WAR.AID.PrimalRend, target, prio);
        }

        var infCD = CD(WAR.AID.Infuriate);
        var irCD = CD(InnerReleaseUnlocked ? WAR.AID.InnerRelease : WAR.AID.Berserk);
        if (Gauge >= 50 || InnerReleaseUnlocked && CanFitGCD(InnerReleaseLeft))
        {
            var action = FellCleaveAction(aoeTargets);
            var prio = FellCleavePriority(infCD, irCD);
            QueueGCD(action, action is WAR.AID.InnerBeast or WAR.AID.FellCleave or WAR.AID.InnerChaos ? primaryTarget : Player, prio);
        }

        var (comboAction, comboPrio) = ComboActionPriority(aoeStrategy, aoeTargets, burstStrategy, burst.Value.ExpireIn, irCD);
        QueueGCD(comboAction, comboAction is WAR.AID.Overpower or WAR.AID.MythrilTempest ? Player : primaryTarget, comboPrio);

        // TODO: tomahawk
        // TODO: ogcds
    }

    private void QueueGCD(WAR.AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)
        {
            var delay = !Player.InCombat && World.Client.CountdownRemaining > 0 ? Math.Max(0, World.Client.CountdownRemaining.Value - EffectApplicationDelay(aid)) : 0;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio, delay: delay);
        }
    }

    // TODO: consider moving to class definitions and autogenerating?
    private float EffectApplicationDelay(WAR.AID aid) => aid switch
    {
        WAR.AID.Bloodwhetting => 0.40f,
        WAR.AID.Holmgang => 0.45f,
        WAR.AID.MythrilTempest => 0.49f,
        WAR.AID.HeavySwing => 0.53f,
        WAR.AID.StormEye => 0.62f,
        WAR.AID.Orogeny => 0.62f,
        WAR.AID.Onslaught => 0.62f,
        WAR.AID.Overpower => 0.62f,
        WAR.AID.Upheaval => 0.62f,
        WAR.AID.Maim => 0.62f,
        WAR.AID.FellCleave => 0.62f,
        WAR.AID.Equilibrium => 0.62f,
        WAR.AID.ThrillOfBattle => 0.62f,
        WAR.AID.Tomahawk => 0.71f,
        WAR.AID.InnerChaos => 0.94f,
        WAR.AID.PrimalRuination => 1.06f,
        WAR.AID.PrimalWrath => 1.15f,
        WAR.AID.PrimalRend => 1.16f,
        WAR.AID.LandWaker => 1.34f,
        WAR.AID.ChaoticCyclone => 1.43f,
        WAR.AID.StormPath => 1.52f,
        WAR.AID.Decimate => 1.83f,
        _ => 0
    };

    // all our aoes have the same shape (except for PR, but we don't really care about its aoe damage)
    private int NumTargetsHitByAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    // heuristic: potion is often pressed ~10s before raidbuffs, so that last 20s overlaps
    // in such case, we don't want to treat first 10s of a potion as burst window by default
    // add extra gcd worth of overlap in case of a slight raidbuff drift
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    private bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3;

    // rend/ruination use same strategy track and very similar considerations (differing only in treatment of gap-close)
    private GCDPriority PRPriority(PrimalRendStrategy strategy, Actor? target)
    {
        // first deal with explicit force/forbid strategies
        if (strategy == PrimalRendStrategy.Forbid)
            return GCDPriority.None;
        if (strategy == PrimalRendStrategy.Force)
            return GCDPriority.ForcedPR;

        // we strongly prefer *not* losing PR; only consider doing that if explicitly forbidden by strategy
        if (!CanFitGCD(PRLeft, 1))
            return GCDPriority.LastChancePR;
        // ok, PR is safe to delay - if we're trying to smuggle, that's it, we don't use it
        if (strategy == PrimalRendStrategy.Smuggle)
            return GCDPriority.None;

        if (!PrimalRuinationActive)
        {
            var inMelee = InMeleeRange(target);
            var needMelee = strategy != PrimalRendStrategy.GapClose;
            if (inMelee != needMelee)
                return GCDPriority.None;
            if (!needMelee)
                return GCDPriority.ForcedPR; // force use as gapcloser to satisfy strategy
        }

        // normal delayable use
        return CanFitGCD(BurstWindowLeft) ? GCDPriority.BuffedPR : PRLeft > BurstWindowIn ? GCDPriority.DelayPR : GCDPriority.FlexiblePR;
    }

    private WAR.AID FellCleaveAction(int aoeTargets)
    {
        // note: under nascent chaos, if IC is not unlocked yet, we want to use cyclone even in non-aoe situations
        // otherwise cyclone is profitable at 3+ targets (300p vs 660p)
        if (NascentChaosLeft > GCD)
            return aoeTargets < 3 && Unlocked(WAR.AID.InnerChaos) ? WAR.AID.InnerChaos : WAR.AID.ChaoticCyclone;

        // aoe gauge spender is profitable:
        // L45 (unlock) to L53 (FC) at 3+ targets (160p vs 330p)
        // L54 (FC) to L59 (Decimate) at 4+ targets (160p vs 520p)
        // L60 (Decimate) to L93 (trait) at 3+ targets (180p vs 520p)
        // L94+ at 4+ targets (180p vs 580p)
        var haveFC = Unlocked(WAR.AID.FellCleave);
        if (Unlocked(WAR.AID.SteelCyclone))
        {
            var haveDecimate = Unlocked(WAR.AID.Decimate);
            var aoeThreshold = haveFC && !haveDecimate || Unlocked(WAR.TraitID.MeleeMastery2) ? 4 : 3;
            if (aoeTargets >= aoeThreshold)
                return haveDecimate ? WAR.AID.Decimate : WAR.AID.SteelCyclone;
        }

        // single-target gauge spender
        return haveFC ? WAR.AID.FellCleave : WAR.AID.InnerBeast;
    }

    private GCDPriority FellCleavePriority(float infuriateCD, float irCD)
    {
        // check for risk of losing NC buff
        // note: NC buff implies that we've unlocked at least Chaotic Cyclone; technically we could've skipped IR quest, but we don't really care to support that
        // NC buff also implies 50+ gauge (given by infuriate that gave NC buff)
        var ncActive = CanFitGCD(NascentChaosLeft);
        if (ncActive)
        {
            var prExpiringSoon = CanFitGCD(PRLeft) && !CanFitGCD(PRLeft, 2); // if PR expires within 2 gcds, next would be forced PR, which reduces deadline for IC
            if (!CanFitGCD(NascentChaosLeft, prExpiringSoon ? 2 : 1))
                return GCDPriority.LastChanceIC;
        }

        // check for risk of losing IR stacks
        // calculation: we need to fit IRStacks (+1 if NC up) FC/ICs into IR window
        // note: this does not check for double IC before IR expiration (which we might do to avoid overcapping infuriate), this is handled below
        var irActive = InnerReleaseUnlocked && CanFitGCD(InnerReleaseLeft);
        var effectiveIRStacks = InnerReleaseStacks + (ncActive ? 1 : 0);
        if (irActive && !CanFitGCD(InnerReleaseLeft, effectiveIRStacks))
            return GCDPriority.LastChanceFC;

        // check for risk of overcapping infuriate
        // first, check whether delaying FC/IC will overcap infuriate
        // note: technically we can use infuriate 0.6s after gcd, but that could end up delaying other important ogcds - so we give a full extra gcd to these calculations; TODO investigate whether it results in suboptimal choices
        // note: we don't have to worry if gauge is <= 50 and NC isn't up - this means we're free to infuriate
        // note: there are situations where e.g. gauge is 40, if we delay FC we would cast SP, and then won't be able to infuriate - this is covered by combo priority calculations
        var infLeeway = GCDLength; // TODO: bare minimum is 0.6
        var infCDReduction = Unlocked(WAR.TraitID.EnhancedInfuriate) ? 5 : 0;
        var needFCBeforeInf = ncActive || Gauge > 50;
        if (needFCBeforeInf && !CanFitGCD(infuriateCD - infCDReduction - infLeeway, 1))
            return GCDPriority.AvoidOvercapInfuriateNext;

        // second, if we're under IR, delaying FC/IC might then force us to spam FC/IC to avoid losing IR stacks, without being able to infuriate, and thus overcap it
        // this can only happen if we won't be able to fit extra IC though
        // note: if IR is imminent, this doesn't matter - 6 gcds is more than enough to use all FC/IC
        if (irActive && !CanFitGCD(InnerReleaseLeft, effectiveIRStacks + 1) && !CanFitGCD(infuriateCD - infCDReduction * effectiveIRStacks - infLeeway, effectiveIRStacks))
            return GCDPriority.AvoidOvercapInfuriateIR;

        // third, if IR is imminent, we have high (>50) gauge, we won't be able to spend this gauge (and use infuriate) before spending IR stacks
        // note: low IR cooldown implies that IR is not active
        var imminentIRStacks = ncActive ? 4 : 3;
        if (needFCBeforeInf && InnerReleaseUnlocked && !CanFitGCD(irCD, 1) && !CanFitGCD(infuriateCD - infCDReduction * imminentIRStacks - infLeeway, imminentIRStacks))
            return GCDPriority.AvoidOvercapInfuriateIR;

        // at this point, we can delay FC/IC safely, just need to consider whether it's worth it
        // first off, if we're bursting, no point in delaying
        if (CanFitGCD(BurstWindowLeft))
            return GCDPriority.BuffedFC;

        if (irActive)
        {
            // under IR, we have very limited opportunity to delay FC/IC, but it still might be useful (eg with early IR opener, raidbuffs appear on second IR gcd, and we can do filler-IC-FC-FC-FC)
            var maxFillers = (int)((InnerReleaseLeft - GCD) / GCDLength) + 1 - effectiveIRStacks;
            var canDelayFC = maxFillers > 0 && !CanFitGCD(BurstWindowIn, maxFillers);
            return canDelayFC ? GCDPriority.DelayFC : GCDPriority.FlexibleFC;
        }
        else if (ncActive)
        {
            // NC outside IR could be delayed until buffs
            return NascentChaosLeft > RaidBuffsIn ? GCDPriority.DelayFC : GCDPriority.FlexibleFC;
        }
        else
        {
            // no good reason to use FC now, unless we'd overcap gauge by combo (but that would be covered by combo priority)
            // TODO: low-level logic (pre IR)
            return GCDPriority.DelayFC;
        }
    }

    private WAR.AID NextComboSingleTarget(bool wantSE) => PrevCombo switch
    {
        WAR.AID.Maim => wantSE ? WAR.AID.StormEye : WAR.AID.StormPath,
        WAR.AID.HeavySwing => WAR.AID.Maim,
        _ => WAR.AID.HeavySwing,
    };

    private WAR.AID NextComboAOE() => PrevCombo == WAR.AID.Overpower ? WAR.AID.MythrilTempest : WAR.AID.Overpower;

    private int GaugeGainedFromAction(WAR.AID action) => action switch
    {
        WAR.AID.Maim or WAR.AID.StormEye => 10,
        WAR.AID.StormPath => 20,
        WAR.AID.MythrilTempest => Unlocked(WAR.TraitID.MasteringTheBeast) ? 20 : 0,
        _ => 0
    };

    private (WAR.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int aoeTargets, BurstStrategy burstStrategy, float burstStrategyExpire, float irCD)
    {
        var comboStepsRemaining = PrevCombo switch
        {
            WAR.AID.HeavySwing => Unlocked(WAR.AID.StormPath) ? 2 : Unlocked(WAR.AID.Maim) ? 1 : 0,
            WAR.AID.Maim => Unlocked(WAR.AID.StormPath) ? 1 : 0,
            WAR.AID.Overpower => Unlocked(WAR.AID.MythrilTempest) ? 1 : 0,
            _ => 0
        };
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0;
        var doingAOECombo = PrevCombo == WAR.AID.Overpower;

        // aoe combo is profitable at 3+ targets (TODO: this is different at low levels!)
        // TODO: reconsider combo breaks:
        // - L94+ ST->AOE after HS replaces 340+480=820p + 20/30g with 250*N + 20g, so profitable at 4+ targets? (assuming 10g is worth ~50p)
        // - L94+ ST->AOE after Maim replaces 480 + 10/20g with ~125*N + 10g (half of aoe combo), so profitable at 4/5+ targets?
        // - L94+ AOE->ST replaces 140*N + 20g with ~345p + 6.7/10g (1/3 of st combo), so profitable at 1/2 targets?
        var wantAOEAction = aoeStrategy == AOEStrategy.AutoFinishCombo && comboStepsRemaining > 0 ? doingAOECombo : Unlocked(WAR.AID.Overpower) && aoeTargets >= 3;
        if (comboStepsRemaining > 0 && wantAOEAction != doingAOECombo)
            comboStepsRemaining = 0;

        // TODO: L40-49: as far as i understand, ST can already be applied by MT, does that mean it should be used even on single target to maintain the buff?
        // TODO: if at last combo step, might be worth finishing combo to prevent ST from falling off
        var needToMaintainST = burstStrategy != BurstStrategy.IgnoreST && Unlocked(WAR.AID.StormEye);
        var gcdsToST = comboStepsRemaining > 0 ? comboStepsRemaining : wantAOEAction ? 2 : 3;

        // see which combo route to take (only relevant when we're at last step of a single-target combo and even care about ST)
        var wantSERoute = false;
        if (!wantAOEAction && needToMaintainST && comboStepsRemaining == 1)
        {
            if (burstStrategy == BurstStrategy.ForceExtendST)
            {
                // forced strategy
                wantSERoute = true;
            }
            else
            {
                // by default, go for SE if we won't overcap ST
                // calculation: SE will overcap if STLeft-GCD+30 > 60 => STLeft > 30+GCD
                // SE+IR will overcap if STLeft-max(IRCD,GCD)+40 > 60 => STLeft > 20+max(IRCD,GCD)
                wantSERoute = SurgingTempestLeft <= 30 + GCD && SurgingTempestLeft <= 20 + Math.Max(GCD, irCD);

                // there's an extra consideration inside burst window: replacing SE with SP might give us a buffed FC in burst
                // note: SP & SE potencies are equal, the only reason to prefer SP inside burst is if it gives us extra gauge to then FC inside burst
                // note: this condition doesn't really matter if we're at >= 50 gauge, since we'd use FC under buffs anyway
                // calculation: it's worth replacing SE with SP if we'll be able to do SP-FC-HS-Maim-SE (4 extra gcds) without dropping ST
                // TODO: consider using the same condition at lower gauge if the burst window is very long (eg infinite due to strategy)
                if (wantSERoute && CanFitGCD(BurstWindowLeft, 1) && Gauge == 30 && CanFitGCD(SurgingTempestLeft, 4))
                {
                    wantSERoute = false;
                }
            }
        }
        var nextAction = wantAOEAction ? NextComboAOE() : NextComboSingleTarget(wantSERoute);
        var riskOvercappingGauge = Gauge + GaugeGainedFromAction(nextAction) > 100;

        // first deal with forced combo; for ST extension, we generally want to minimize overcap by using combo finisher as late as possible
        // TODO: reconsider what to do if we can't fit in combo - do we still want to do partial combo? especially if it would cause gauge overcap
        if (needToMaintainST && burstStrategy == BurstStrategy.ForceExtendST /*&& CanFitGCD(burstStrategyExpire, gcdsToST - 1)*/)
        {
            // if we can delay combo and still finish it within strategy window, do so if it reduces overcaps
            var wouldOvercapST = comboStepsRemaining == 1 && SurgingTempestLeft - GCD > 30;
            var delayCombo = CanFitGCD(burstStrategyExpire, gcdsToST) && (riskOvercappingGauge || wouldOvercapST);
            return (nextAction, delayCombo ? GCDPriority.DelayCombo : GCDPriority.ForcedCombo);
        }

        // check for risk of dropping ST
        // TODO: consider tradeoffs between overcapping gauge vs dropping ST - the optimal answer could be different depending on what ends up unbuffed if ST is dropped
        // for example: if ST is up for 1 more gcd, next combo is HS, and we're at 100 gauge - might be better to do FC now (and lose 10% of 1040p) instead of HS (and lose 20 gauge and 10% of 820p)?
        // another example to consider is if ST is about to drop, but we have full gauge and overcapping infuriate (eg after long downtime)
        // TODO: if ST is up and IR is imminent, this is not a concern? we don't want to overcap gauge... 
        if (needToMaintainST && !CanFitGCD(SurgingTempestLeft, gcdsToST))
            return (nextAction, GCDPriority.AvoidDropST);

        // check for risk of expiring combo (this realistically only happens after downtime)
        // TODO: reconsider leeway; we could be in a situation where we have eg 2 gcds left to combo, but next gcd is something really important (eg last-chance action)
        // the longest non-downtime sequence of non-combo actions is: (starting with 100 gauge) smuggled ruin - smuggled IC - IR+FCx3 -> FC -> Inf+IC x2 -> PR/PR (10 gcds)
        // TODO: consider tradeoffs between overcapping gauge vs dropping combo
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.AvoidDropCombo);

        // just a normal combo action; delay if overcapping gauge
        return (nextAction, riskOvercappingGauge ? GCDPriority.DelayCombo : GCDPriority.FlexibleCombo);
    }
}
#endif
