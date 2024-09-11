using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;

public sealed class StandardWAR(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { AOE, Burst, Potion, PrimalRend, Tomahawk, InnerRelease, Infuriate, Upheaval, Wrath, Onslaught, Bozja }
    public enum AOEStrategy { SingleTarget, ForceAOE, Auto, AutoFinishCombo }
    public enum BurstStrategy { Automatic, Spend, Conserve, UnderRaidBuffs, UnderPotion, ForceExtendST, IgnoreST }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, AlignWithIR, Immediate }
    public enum PrimalRendStrategy { Automatic, Forbid, Force, GapClose, SmuggleNextPotion, SmuggleAlignedPotion, DelayUntilLastChance }
    public enum TomahawkStrategy { OpenerRanged, Opener, Forbid, Force, Ranged }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum InfuriateStrategy { Automatic, Delay, ForceIfNoNC, ForceIfChargesCapping }
    public enum OnslaughtStrategy { Automatic, Forbid, NoReserve, Force, ForceReserve, ReserveTwo, GapClose }
    public enum BozjaStrategy { None, WithIR, BloodRage }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Standard WAR", "Standard rotation module", "veyn", RotationModuleQuality.Good, BitMask.Build((int)Class.WAR, (int)Class.MRD), 100);

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
            .AddOption(PrimalRendStrategy.SmuggleNextPotion, "SmuggleNextPotion", "Delay until last possible GCD, if potion will be ready by the time ruin expires (useful for smuggling ruin into unaligned pot window)", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.SmuggleAlignedPotion, "SmuggleAlignedPotion", "Delay until last possible GCD, if potion will be ready by the time ruin expires and would overlap raidbuffs (useful for smuggling ruin into aligned pot window)", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(PrimalRendStrategy.DelayUntilLastChance, "DelayUntilLastChance", "Delay until last possible GCD", 0, 0, ActionTargets.Hostile, 90)
            .AddAssociatedActions(WAR.AID.PrimalRend, WAR.AID.PrimalRuination);

        res.Define(Track.Tomahawk).As<TomahawkStrategy>("Tomahawk", uiPriority: 10)
            .AddOption(TomahawkStrategy.OpenerRanged, "OpenerRanged", "Use as very first GCD and only if outside melee range")
            .AddOption(TomahawkStrategy.Opener, "Opener", "Use as very first GCD regardless of range")
            .AddOption(TomahawkStrategy.Forbid, "Forbid", "Do not use automatically")
            .AddOption(TomahawkStrategy.Force, "Force", "Force use ASAP (even in melee range)")
            .AddOption(TomahawkStrategy.Ranged, "Ranged", "Use if outside melee range")
            .AddAssociatedActions(WAR.AID.Tomahawk);

        res.Define(Track.InnerRelease).As<OffensiveStrategy>("IR", uiPriority: 50)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally (whenever ST is up)")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even during downtime or without ST)")
            .AddAssociatedActions(WAR.AID.Berserk, WAR.AID.InnerRelease);

        res.Define(Track.Infuriate).As<InfuriateStrategy>("Infuriate", uiPriority: 60)
            .AddOption(InfuriateStrategy.Automatic, "Automatic", "Try to delay uses until raidbuffs, avoiding overcap")
            .AddOption(InfuriateStrategy.Delay, "Delay", "Delay, even if risking overcap")
            .AddOption(InfuriateStrategy.ForceIfNoNC, "ForceIfNoNC", "Force unless NC active")
            .AddOption(InfuriateStrategy.ForceIfChargesCapping, "ForceIfChargesCapping", "Force use if charges are about to overcap (unless NC is already active), even if it would overcap gauge")
            .AddAssociatedActions(WAR.AID.Infuriate);

        res.Define(Track.Upheaval).As<OffensiveStrategy>("Upheaval", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally (ASAP, assuming ST is up)")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even without ST)")
            .AddAssociatedActions(WAR.AID.Upheaval, WAR.AID.Orogeny);

        res.Define(Track.Wrath).As<OffensiveStrategy>("Wrath", uiPriority: -10)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use normally (ASAP, assuming ST is up)")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use ASAP (even without ST)")
            .AddAssociatedActions(WAR.AID.PrimalWrath);

        res.Define(Track.Onslaught).As<OnslaughtStrategy>("Onslaught", uiPriority: 20)
            .AddOption(OnslaughtStrategy.Automatic, "Automatic", "Always keep one charge reserved, use other charges inside burst or to prevent overcapping")
            .AddOption(OnslaughtStrategy.Forbid, "Forbid", "Forbid automatic use")
            .AddOption(OnslaughtStrategy.NoReserve, "NoReserve", "Do not reserve charges: use all charges if inside burst, otherwise use as needed to prevent overcapping")
            .AddOption(OnslaughtStrategy.Force, "Force", "Use all charges ASAP")
            .AddOption(OnslaughtStrategy.ForceReserve, "ForceReserve", "Use all charges except one ASAP")
            .AddOption(OnslaughtStrategy.ReserveTwo, "ReserveTwo", "Reserve 2 charges, trying to prevent overcap")
            .AddOption(OnslaughtStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range")
            .AddAssociatedActions(WAR.AID.Onslaught);

        res.Define(Track.Bozja).As<BozjaStrategy>("Bozja", uiPriority: -20)
            .AddOption(BozjaStrategy.None, "None", "Do not use any lost actions automatically")
            .AddOption(BozjaStrategy.WithIR, "WithIR", "Use generic buff lost actions together (right before) inner release")
            .AddOption(BozjaStrategy.BloodRage, "BloodRage", "Optimize rotation around lost blood rage usage (delay IR, use onslaught to proc blood rage)")
            .AddAssociatedAction(BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfPower))
            .AddAssociatedAction(BozjaActionID.GetNormal(BozjaHolsterID.BannerHonoredSacrifice))
            .AddAssociatedAction(BozjaActionID.GetNormal(BozjaHolsterID.LostBloodRage));

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
        FlexibleCombo = 400, // normal flexible combo
        FlexibleFC = 470, // using FC/IC now is low value (not buffed), but we don't see any better opportunity to use it in future
        FlexiblePR = 480, // using PR now is low value (not buffed), but we don't see any better opportunity to use it in future
        FlexibleIR = 490, // using FC/IC under IR is low value, but we still wanna get it done sooner rather than later to minimize the chance to lose stacks
        // high value actions (under buffs)
        BuffedFC = 550,
        BuffedPR = 560,
        BuffedIR = 570, // FC/IC under IR; these are high priority, since we want to spend stacks asap to use wrath under buffs
        // critical actions (avoid overcapping gauge/infuriate, maintain ST, etc)
        AvoidDropCombo = 660, // if we don't continue combo, we'll lose it
        AvoidOvercapInfuriateIR = 670, // if we delay FC/IC, we'll be forced to spam IC/FC later to avoid losing IR stacks, which will overcap infuriate
        AvoidOvercapInfuriateNext = 680, // if we delay FC/IC, infuriate will overcap when it's used later
        AvoidDropST = 690, // if we delay combo, we will be dropping ST
        // last-chance priorities: if corresponding action is not used on next gcd, we will likely lose an opportunity
        LastChanceFC = 770, // last chance to use IC/FC (otherwise IR will expire)
        LastChanceIC = 780, // last chance to use IC (otherwise nascent chaos will expire)
        LastChancePR = 790, // last chance to use PR (otherwise buff will expire)
        // forced actions are explicitly requested by strategies, so we assume user knows what he's doing
        ForcedTomahawk = 870,
        ForcedCombo = 880,
        ForcedPR = 890,
        // gapcloser; note that if we're using onslaught as a gapcloser, it has higher priority than other gcds
        GapclosePR = 990,
    }

    public enum OGCDPriority
    {
        None = 0,
        Onslaught = 500,
        PrimalWrath = 550,
        Infuriate = 570,
        Upheaval = 580,
        InnerRelease = 590,
        LostBanner = 660,
        LostFont = 670,
        LostBloodRage = 680,
        BloodRageOnslaught = 690,
        Potion = 900,
        GapcloseOnslaught = 980, // note that it uses 'very high' prio
    }

    private int Gauge; // 0 to 100
    private float GCDLength; // 2.5s adjusted by sks/haste
    private float SurgingTempestLeft; // max 60
    private float NascentChaosLeft; // max 30
    private float InfuriateCD;
    private int InfuriateCDReduction; // 0 or 5, depending on trait
    private float InfuriateCDLeeway; // minimal CD on infuriate after next GCD to allow delaying; bare minimum is 0.6 (typical gcd anim lock)
    private bool InnerReleaseUnlocked;
    private float InnerReleaseLeft; // max 15 (either IR or berserk)
    private int InnerReleaseStacks; // max 3 (either IR or berserk)
    private float InnerReleaseCD;
    private float WrathfulLeft; // max 30
    private float PRLeft; // max 30 (rend) or 20 (ruination)
    private bool PrimalRuinationActive;
    private float OnslaughtCD;
    private float OnslaughtCapIn;
    private float PotionLeft; // max 30
    private float RaidBuffsLeft; // max 20
    private float RaidBuffsIn;
    private float LostBloodRageLeft; // max 30 (at 4 stacks) or 18 (at 1-3 stacks)
    private int LostBloodRageStacks; // 4 if actual blood rage is active
    private float LostBloodRageIn; // 0-180 if blood rage is slotted and desired by strategy; otherwise max-value
    private float LostFontCD; // 0-120 if font is slotted and desired by strategy; otherwise max-value
    private float LostBannerCD; // 0-90 if font is slotted and desired by strategy; otherwise max-value
    private float LostBuffsLeft; // max(font of power, banner of honored sacrifice), max 30
    private float LostBuffsIn; // depends on bozja strategy, when do we expect burst
    private float BurstWindowLeft;
    private float BurstWindowIn;
    private WAR.AID NextGCD; // this is needed to estimate gauge and make a decision on infuriate
    private GCDPriority NextGCDPrio;

    private const float OnslaughtMinGCD = 0.8f; // triple-weaving onslaught is not a good idea, since it might delay gcd for longer than normal anim lock; TODO reconsider implementation

    private bool Unlocked(WAR.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    private bool Unlocked(WAR.TraitID tid) => TraitUnlocked((uint)tid);
    private float CD(WAR.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline; // note: if deadline is 0 (meaning status not active etc), we can't fit a single gcd even if available immediately (GCD==0), so we use <
    private WAR.AID PrevCombo => (WAR.AID)World.Client.ComboState.Action;
    private bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3;
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        Gauge = World.Client.GetGauge<WarriorGauge>().BeastGauge;
        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
        SurgingTempestLeft = SelfStatusLeft(WAR.SID.SurgingTempest);

        NascentChaosLeft = SelfStatusLeft(WAR.SID.NascentChaos);
        InfuriateCD = CD(WAR.AID.Infuriate);
        // note: technically we can use infuriate 0.6s after gcd, but that could end up delaying other important ogcds - so we give a full extra gcd to these calculations; TODO investigate whether it results in suboptimal choices
        InfuriateCDLeeway = GCDLength;
        InfuriateCDReduction = Unlocked(WAR.TraitID.EnhancedInfuriate) ? 5 : 0;

        InnerReleaseUnlocked = Unlocked(WAR.AID.InnerRelease);
        (InnerReleaseLeft, InnerReleaseStacks) = SelfStatusDetails(InnerReleaseUnlocked ? WAR.SID.InnerRelease : WAR.SID.Berserk);
        InnerReleaseCD = CD(InnerReleaseUnlocked ? WAR.AID.InnerRelease : WAR.AID.Berserk);
        WrathfulLeft = SelfStatusLeft(WAR.SID.Wrathful);

        PRLeft = SelfStatusLeft(WAR.SID.PrimalRuinationReady);
        PrimalRuinationActive = PRLeft > 0;
        if (!PrimalRuinationActive)
            PRLeft = SelfStatusLeft(WAR.SID.PrimalRend);

        OnslaughtCD = CD(WAR.AID.Onslaught);
        OnslaughtCapIn = Math.Max(0, OnslaughtCD - (Unlocked(WAR.TraitID.EnhancedOnslaught) ? 0 : 30));
        PotionLeft = PotionStatusLeft();
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);

        var bozjaStrategy = strategy.Option(Track.Bozja).As<BozjaStrategy>();
        CalculateBozjaState(bozjaStrategy);

        NextGCD = WAR.AID.None;
        NextGCDPrio = GCDPriority.None;

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
        // note: if we have bozja buffs pending, assume that is our main burst, rather than pots or raidbuffs
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => LostBuffsIn < float.MaxValue ? (LostBuffsIn, LostBuffsLeft) : (RaidBuffsIn, Math.Max(LostBuffsLeft, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft))),
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
            QueueGCD(PrimalRuinationActive ? WAR.AID.PrimalRuination : WAR.AID.PrimalRend, target, prio);
        }

        if (Gauge >= 50 || InnerReleaseUnlocked && CanFitGCD(InnerReleaseLeft))
        {
            var action = FellCleaveAction(aoeTargets);
            var prio = InnerReleaseUnlocked ? FellCleavePriorityIR() : FellCleavePriorityBerserk();
            QueueGCD(action, action is WAR.AID.InnerBeast or WAR.AID.FellCleave or WAR.AID.InnerChaos ? primaryTarget : Player, prio);
        }

        var (comboAction, comboPrio) = ComboActionPriority(aoeStrategy, aoeTargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is WAR.AID.Overpower or WAR.AID.MythrilTempest ? Player : primaryTarget, comboPrio);

        if (ShouldUseTomahawk(primaryTarget, strategy.Option(Track.Tomahawk).As<TomahawkStrategy>()))
            QueueGCD(WAR.AID.Tomahawk, primaryTarget, GCDPriority.ForcedTomahawk);

        // oGCDs
        if (InnerReleaseUnlocked)
        {
            if (ShouldUseInnerRelease(strategy.Option(Track.InnerRelease).As<OffensiveStrategy>(), primaryTarget))
                QueueOGCD(WAR.AID.InnerRelease, Player, OGCDPriority.InnerRelease);
        }
        else if (Unlocked(WAR.AID.Berserk))
        {
            if (ShouldUseBerserk(strategy.Option(Track.InnerRelease).As<OffensiveStrategy>(), primaryTarget, aoeTargets))
                QueueOGCD(WAR.AID.Berserk, Player, OGCDPriority.InnerRelease);
        }

        if (Player.InCombat && Unlocked(WAR.AID.Infuriate))
        {
            var inf = ShouldUseInfuriate(strategy.Option(Track.Infuriate).As<InfuriateStrategy>(), primaryTarget);
            if (inf.Use)
                QueueOGCD(WAR.AID.Infuriate, Player, OGCDPriority.Infuriate, inf.Delayable ? ActionQueue.Priority.VeryLow : ActionQueue.Priority.Low);
        }

        if (Unlocked(WAR.AID.Upheaval) && ShouldUseUpheaval(strategy.Option(Track.Upheaval).As<OffensiveStrategy>()))
        {
            var aoe = aoeTargets >= 3 && Unlocked(WAR.AID.Orogeny);
            QueueOGCD(aoe ? WAR.AID.Orogeny : WAR.AID.Upheaval, aoe ? Player : primaryTarget, OGCDPriority.Upheaval);
        }

        if (aoeTargets > 0 && WrathfulLeft > World.Client.AnimationLock && ShouldUsePrimalWrath(strategy.Option(Track.Wrath).As<OffensiveStrategy>()))
        {
            QueueOGCD(WAR.AID.PrimalWrath, Player, OGCDPriority.PrimalWrath);
        }

        if (Unlocked(WAR.AID.Onslaught))
        {
            var onsStrategy = strategy.Option(Track.Onslaught).As<OnslaughtStrategy>();
            if (ShouldUseOnslaught(onsStrategy, primaryTarget))
            {
                // special case for use as gapcloser - it has to be very high priority
                var (prio, basePrio) = onsStrategy == OnslaughtStrategy.GapClose ? (OGCDPriority.GapcloseOnslaught, ActionQueue.Priority.High)
                    : LostBloodRageStacks is > 0 and < 4 ? (OGCDPriority.LostBanner, ActionQueue.Priority.Medium)
                    : (OGCDPriority.Onslaught, OnslaughtCD < GCDLength ? ActionQueue.Priority.VeryLow : ActionQueue.Priority.Low);
                QueueOGCD(WAR.AID.Onslaught, primaryTarget, prio, basePrio);
            }
        }

        // potion should be used as late as possible in ogcd window, so that if playing at <2.5 gcd, it can cover 13 gcds
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Low + (int)OGCDPriority.Potion, 0, GCD - 0.9f);

        // bozja actions
        if (ShouldUseLostBloodRage())
            Hints.ActionsToExecute.Push(BozjaActionID.GetNormal(BozjaHolsterID.LostBloodRage), Player, ActionQueue.Priority.Low + (int)OGCDPriority.LostBloodRage);
        if (ShouldUseLostBuff(LostFontCD, 120))
            Hints.ActionsToExecute.Push(BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfPower), Player, ActionQueue.Priority.Low + (int)OGCDPriority.LostFont);
        if (ShouldUseLostBuff(LostBannerCD, 90))
            Hints.ActionsToExecute.Push(BozjaActionID.GetNormal(BozjaHolsterID.BannerHonoredSacrifice), Player, ActionQueue.Priority.Low + (int)OGCDPriority.LostBanner);
    }

    private void QueueGCD(WAR.AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)
        {
            var delay = !Player.InCombat && World.Client.CountdownRemaining > 0 ? Math.Max(0, World.Client.CountdownRemaining.Value - EffectApplicationDelay(aid)) : 0;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio, delay: delay);
            if (prio > NextGCDPrio)
            {
                NextGCD = aid;
                NextGCDPrio = prio;
            }
        }
    }

    private void QueueOGCD(WAR.AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Low)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
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

    // calculate state of the bozja specific actions
    private void CalculateBozjaState(BozjaStrategy strategy)
    {
        LostBuffsLeft = Math.Max(SelfStatusLeft(WAR.SID.LostFontOfPower), SelfStatusLeft(WAR.SID.BannerOfHonoredSacrifice));
        (LostBloodRageLeft, LostBloodRageStacks) = SelfStatusDetails(WAR.SID.LostBloodRage);
        if (LostBloodRageStacks == 0)
        {
            LostBloodRageLeft = SelfStatusLeft(WAR.SID.BloodRush);
            if (LostBloodRageLeft > 0)
            {
                LostBloodRageStacks = 4;
                LostBuffsLeft = Math.Max(LostBuffsLeft, LostBloodRageLeft);
            }
        }

        LostFontCD = LostBannerCD = LostBloodRageIn = LostBuffsIn = float.MaxValue;
        if (strategy == BozjaStrategy.None)
            return; // don't expect any of the buffs ...

        // assume if both FoP & BoTH are slotted, we gonna stack them, otherwise whatever is available can be pressed when ready
        LostFontCD = DutyActionCD(BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfPower));
        LostBannerCD = DutyActionCD(BozjaActionID.GetNormal(BozjaHolsterID.BannerHonoredSacrifice));
        LostBuffsIn = LostFontCD < float.MaxValue && LostBannerCD < float.MaxValue ? Math.Max(LostFontCD, LostBannerCD) : Math.Min(LostFontCD, LostBannerCD);

        // we also wanna stack buffs with IR - this happens naturally for FoP, but BoTH will need to be delayed
        // note that we generally press buffs a gcd before IR
        LostBuffsIn = Math.Max(LostBuffsIn, InnerReleaseCD - GCDLength);

        // finally, if we're doing blood rage, we are delaying burst further
        if (strategy == BozjaStrategy.BloodRage)
        {
            LostBloodRageIn = DutyActionCD(BozjaActionID.GetNormal(BozjaHolsterID.LostBloodRage));
            var bloodRageWindowStart = LostBloodRageStacks switch
            {
                0 => LostBloodRageIn < float.MaxValue ? Math.Max(LostBloodRageIn, OnslaughtCapIn) + 30 : float.MaxValue, // we need around 30s to stack blood rage (2 reserved onslaughts + 1 to recharge)
                1 => OnslaughtCapIn + 30,
                2 => OnslaughtCapIn,
                3 => Math.Max(0, OnslaughtCapIn - 30),
                _ => 0
            };
            // if we can't use buffs and have them available by the time bloodrage window starts, delay
            if (bloodRageWindowStart < LostBuffsIn + 120)
                LostBuffsIn = bloodRageWindowStart;
        }
    }

    // heuristic: potion is often pressed ~10s before raidbuffs, so that last 20s overlaps
    // in such case, we don't want to treat first 10s of a potion as burst window by default
    // add extra gcd worth of overlap in case of a slight raidbuff drift
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    private bool CanSmugglePR(bool aligned)
    {
        if (CanFitGCD(PotionLeft))
            return false; // potion is already active, so no point in delaying
        var ruinExpireIn = PRLeft + (PrimalRuinationActive ? 0 : 20);
        if (ruinExpireIn < PotionCD)
            return false; // no way we can delay until potion comes off cd
        // ok, we can try delaying until potion - but if we're actually trying to align potion to raidbuffs, we might delay it too
        // TODO: this needs more thought... for now, assume raidbuffs need to become available at most 20s after expiration
        return !aligned || RaidBuffsLeft == 0 && RaidBuffsIn < ruinExpireIn + 20;
    }

    private bool ShouldSmugglePR(PrimalRendStrategy strategy) => strategy switch
    {
        PrimalRendStrategy.SmuggleNextPotion => CanSmugglePR(false),
        PrimalRendStrategy.SmuggleAlignedPotion => CanSmugglePR(true),
        PrimalRendStrategy.DelayUntilLastChance => true,
        _ => false, // by default, we don't smuggle
    };

    // rend/ruination use same strategy track and very similar considerations (differing only in treatment of gap-close)
    private GCDPriority PRPriority(PrimalRendStrategy strategy, Actor? target)
    {
        // first deal with explicit force/forbid strategies
        if (strategy == PrimalRendStrategy.Forbid)
            return GCDPriority.None;
        if (strategy == PrimalRendStrategy.Force)
            return PrimalRuinationActive || InMeleeRange(target) ? GCDPriority.ForcedPR : GCDPriority.GapclosePR;

        // we strongly prefer *not* losing PR; only consider doing that if explicitly forbidden by strategy
        if (!CanFitGCD(PRLeft, 1))
            return PrimalRuinationActive || InMeleeRange(target) ? GCDPriority.LastChancePR : GCDPriority.GapclosePR;
        // ok, PR is safe to delay - if we're trying to smuggle, that's it, we don't use it
        if (ShouldSmugglePR(strategy))
            return GCDPriority.None;

        if (!PrimalRuinationActive)
        {
            var outOfMelee = !InMeleeRange(target);
            var gapclose = strategy == PrimalRendStrategy.GapClose;
            if (gapclose != outOfMelee)
                return GCDPriority.None;
            if (gapclose)
                return GCDPriority.GapclosePR;
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

    private GCDPriority FellCleavePriorityIR()
    {
        // check for risk of losing NC buff (having a buff implies that we've unlocked at least Chaotic Cyclone)
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
        var irActive = CanFitGCD(InnerReleaseLeft);
        var effectiveIRStacks = InnerReleaseStacks + (ncActive ? 1 : 0);
        if (irActive && !CanFitGCD(InnerReleaseLeft, effectiveIRStacks))
            return GCDPriority.LastChanceFC;

        // check for risk of overcapping infuriate
        // first, check whether delaying FC/IC will overcap infuriate
        // note: we don't have to worry if gauge is <= 50 and NC isn't up - this means we're free to infuriate
        // note: there are situations where e.g. gauge is 40, if we delay FC we would cast SP, and then won't be able to infuriate - this is covered by combo priority calculations
        var needFCBeforeInf = ncActive || Gauge > 50;
        if (needFCBeforeInf && !CanFitGCD(InfuriateCD - InfuriateCDReduction - InfuriateCDLeeway, 1))
            return GCDPriority.AvoidOvercapInfuriateNext;

        // second, if we're under IR, delaying FC/IC might then force us to spam FC/IC to avoid losing IR stacks, without being able to infuriate, and thus overcap it
        // this can only happen if we won't be able to fit extra IC though
        // note: if IR is imminent, this doesn't matter - 6 gcds is more than enough to use all FC/IC
        if (irActive && !CanFitGCD(InnerReleaseLeft, effectiveIRStacks + 1) && !CanFitGCD(InfuriateCD - InfuriateCDReduction * effectiveIRStacks - InfuriateCDLeeway, effectiveIRStacks))
            return GCDPriority.AvoidOvercapInfuriateIR;

        // third, if IR is imminent, we have high (>50) gauge, we won't be able to spend this gauge (and use infuriate) before spending IR stacks
        // note: low IR cooldown implies that IR is not active
        var imminentIRStacks = ncActive ? 4 : 3;
        if (needFCBeforeInf && !CanFitGCD(InnerReleaseCD, 1) && !CanFitGCD(InfuriateCD - InfuriateCDReduction * imminentIRStacks - InfuriateCDLeeway, imminentIRStacks))
            return GCDPriority.AvoidOvercapInfuriateIR;

        // at this point, we can delay FC/IC safely, just need to consider whether it's worth it
        // first off, if we're bursting, no point in delaying
        if (CanFitGCD(BurstWindowLeft))
            return irActive ? GCDPriority.BuffedIR : GCDPriority.BuffedFC;

        if (irActive)
        {
            // under IR, we have very limited opportunity to delay FC/IC, but it still might be useful (eg with early IR opener, raidbuffs appear on second IR gcd, and we can do filler-IC-FC-FC-FC)
            var maxFillers = (int)((InnerReleaseLeft - GCD) / GCDLength) + 1 - effectiveIRStacks;
            var canDelayFC = maxFillers > 0 && !CanFitGCD(BurstWindowIn, maxFillers);
            return canDelayFC ? GCDPriority.DelayFC : GCDPriority.FlexibleIR;
        }
        else if (ncActive)
        {
            // NC outside IR could be delayed until buffs
            return NascentChaosLeft > BurstWindowIn ? GCDPriority.DelayFC : GCDPriority.FlexibleFC;
        }
        else
        {
            // no good reason to use FC now, unless we'd overcap gauge by combo (but that would be covered by combo priority)
            return GCDPriority.DelayFC;
        }
    }

    private GCDPriority FellCleavePriorityBerserk()
    {
        // check for risk of losing NC buff (having a buff implies that we've unlocked at least Chaotic Cyclone)
        // NC buff also implies 50+ gauge (given by infuriate that gave NC buff)
        // note: technically it's possible to unlock NC (and even PR) without unlocking IR by skipping a quest
        var ncActive = CanFitGCD(NascentChaosLeft);
        if (ncActive)
        {
            var prExpiringSoon = CanFitGCD(PRLeft) && !CanFitGCD(PRLeft, 2); // if PR expires within 2 gcds, next would be forced PR, which reduces deadline for IC
            if (!CanFitGCD(NascentChaosLeft, prExpiringSoon ? 2 : 1))
                return GCDPriority.LastChanceIC;
        }

        // before unlocking IR, we want to use IB/FC mainly during berserk; outside berserk, we only use it to avoid overcapping gauge & infuriate
        // we also can't delay spending berserk charges - any gcd will use them up
        if (CanFitGCD(InnerReleaseLeft))
            return GCDPriority.LastChanceFC;

        // check for risk of overcapping infuriate if we delay FC/IC
        // note: we don't have to worry if gauge is <= 50 and NC isn't up - this means we're free to infuriate
        // note: there are situations where e.g. gauge is 40, if we delay FC we would cast SP, and then won't be able to infuriate - this is covered by combo priority calculations
        var needFCBeforeInf = Unlocked(WAR.AID.Infuriate) && (ncActive || Gauge > 50);
        if (needFCBeforeInf && !CanFitGCD(InfuriateCD - InfuriateCDReduction - InfuriateCDLeeway, 1))
            return GCDPriority.AvoidOvercapInfuriateNext;

        // if we're in burst window and berserk is not up or imminent (eg. we've spent all our charges already but still have some gauge), might as well FC
        // note that if burst window is badly misaligned, we prefer keeping gauge for next berserk; getting back 80+ gauge will take us 9 gcds
        if (CanFitGCD(BurstWindowLeft) && CanFitGCD(InnerReleaseCD, 9))
            return GCDPriority.BuffedFC;

        // if we have NC, but it will expire before next burst/berserk, might as well use it now
        if (ncActive && NascentChaosLeft < BurstWindowIn && NascentChaosLeft < InnerReleaseCD)
            return GCDPriority.FlexibleFC;

        // no good reason to use FC now, unless we'd overcap gauge by combo (but that would be covered by combo priority)
        return GCDPriority.DelayFC;
    }

    private WAR.AID NextComboSingleTarget(bool wantSE, bool forceReset) => forceReset ? WAR.AID.HeavySwing : PrevCombo switch
    {
        WAR.AID.Maim => wantSE ? WAR.AID.StormEye : WAR.AID.StormPath,
        WAR.AID.HeavySwing => WAR.AID.Maim,
        _ => WAR.AID.HeavySwing,
    };

    private WAR.AID NextComboAOE(bool forceReset) => PrevCombo == WAR.AID.Overpower && !forceReset ? WAR.AID.MythrilTempest : WAR.AID.Overpower;

    private int GaugeGainedFromAction(WAR.AID action) => action switch
    {
        WAR.AID.Maim or WAR.AID.StormEye => 10,
        WAR.AID.StormPath => 20,
        WAR.AID.MythrilTempest => Unlocked(WAR.TraitID.MasteringTheBeast) ? 20 : 0,
        _ => 0
    };

    private (WAR.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int aoeTargets, BurstStrategy burstStrategy, float burstStrategyExpire)
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
        var wantAOEAction = Unlocked(WAR.AID.Overpower) && aoeStrategy switch
        {
            AOEStrategy.SingleTarget => false,
            AOEStrategy.ForceAOE => true,
            AOEStrategy.Auto => aoeTargets >= 3,
            AOEStrategy.AutoFinishCombo => comboStepsRemaining > 0 ? doingAOECombo : aoeTargets >= 3,
            _ => false
        };
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
                wantSERoute = SurgingTempestLeft <= 30 + GCD && SurgingTempestLeft <= 20 + Math.Max(GCD, InnerReleaseCD);

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
        var nextAction = wantAOEAction ? NextComboAOE(comboStepsRemaining == 0) : NextComboSingleTarget(wantSERoute, comboStepsRemaining == 0);
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

    private bool ShouldUseTomahawk(Actor? target, TomahawkStrategy strategy) => strategy switch
    {
        TomahawkStrategy.OpenerRanged => IsFirstGCD() && !InMeleeRange(target),
        TomahawkStrategy.Opener => IsFirstGCD(),
        TomahawkStrategy.Force => true,
        TomahawkStrategy.Ranged => !InMeleeRange(target),
        _ => false
    };

    private bool ShouldUseInnerRelease(OffensiveStrategy strategy, Actor? target)
    {
        if (strategy != OffensiveStrategy.Automatic)
            return strategy == OffensiveStrategy.Force;

        if (!Player.InCombat || target == null || target.IsAlly)
            return false; // prepull / downtime

        if (LostBloodRageStacks == 4)
            return true; // use under bloodrage asap

        if (LostBloodRageStacks > 0 || InnerReleaseCD + 30 > LostBloodRageIn)
            return false; // do not use if bloodrage is imminent and it won't come off cd if used now

        // by default, we use IR asap as soon as ST is up
        // TODO: reconsider ST duration threshold...
        return SurgingTempestLeft > InnerReleaseCD;
    }

    // this is relevant only until we unlock IR
    private bool ShouldUseBerserk(OffensiveStrategy strategy, Actor? target, int aoeTargets)
    {
        if (strategy != OffensiveStrategy.Automatic)
            return strategy == OffensiveStrategy.Force;

        if (!Player.InCombat || target == null || target.IsAlly)
            return false; // prepull / downtime

        if (Unlocked(WAR.AID.StormEye) && !CanFitGCD(SurgingTempestLeft, 2))
            return false; // ST will fall off during berserk

        if (aoeTargets >= 3)
            return true; // don't delay during aoe

        if (Unlocked(WAR.AID.Infuriate))
        {
            // we really want to cast SP + 2xIB or 3xIB under berserk; check whether we'll have infuriate before third GCD
            var availableGauge = Gauge;
            if (CD(WAR.AID.Infuriate) <= 65)
                availableGauge += 50;
            return PrevCombo switch
            {
                WAR.AID.Maim => availableGauge >= 80, // TODO: this isn't a very good check, improve...
                _ => availableGauge == 150
            };
        }
        else if (Unlocked(WAR.AID.InnerBeast))
        {
            // L35-49: ideally want to cast SP + 2xIB under berserk (we need to have 80+ gauge for that)
            // however, we are also content with casting Maim + SP + IB (we need to have 20+ gauge for that; but if we have 70+, it is better to delay for 1 GCD)
            // alternatively, we could delay for 3 GCDs at 40+ gauge - TODO determine which is better
            return PrevCombo switch
            {
                WAR.AID.HeavySwing => Gauge is >= 20 and < 70,
                WAR.AID.Maim => Gauge >= 80,
                _ => false,
            };
        }
        else if (Unlocked(WAR.AID.StormPath))
        {
            // L26-34: we are always going to use all 3 combo actions in arbitrary order, so no point delaying berserk
            return true;
        }
        else
        {
            // L10-25: it's better to use Maim->HS->Maim
            return PrevCombo == WAR.AID.HeavySwing;
        }
    }

    private (bool Use, bool Delayable) ShouldUseInfuriate(InfuriateStrategy strategy, Actor? target)
    {
        if (strategy == InfuriateStrategy.Delay || CanFitGCD(NascentChaosLeft))
            return (false, false); // explicitly forbidden or NC still active

        if (strategy == InfuriateStrategy.ForceIfNoNC)
            return (true, false); // explicitly requested
        if (strategy == InfuriateStrategy.ForceIfChargesCapping && InfuriateCD <= World.Client.AnimationLock)
            return (true, false); // this really makes most sense as a downtime option, so very tight check

        // ok, we need to actually make a decision here
        if (Gauge > 50)
            return (false, false); // using would overcap gauge
        if (target == null || target.IsAlly)
            return (false, false); // don't use during downtime, no guarantees it will end before NC expires

        var irActive = CanFitGCD(InnerReleaseLeft);
        if (!InnerReleaseUnlocked)
        {
            // before IR, main purpose of infuriate is to maximize buffed FCs under Berserk
            // note: technically it's possible to unlock NC without unlocking IR, but we really don't care to optimize this situation, it's probably gonna work well enough
            if (irActive)
                return (true, false);

            // don't delay if we risk overcapping stacks (TODO: improve this check...)
            if (!CanFitGCD(InfuriateCD, 4))
                return (true, true);

            // TODO: consider whether we want to spend both stacks in spend mode if berserk is not imminent...
            return (false, false);
        }

        var unlockedNC = Unlocked(WAR.AID.ChaoticCyclone);
        if (unlockedNC && irActive && !CanFitGCD(InnerReleaseLeft, InnerReleaseStacks))
            return (false, false); // using inf now would cost IR stack

        // check whether we are at risk of overcapping infuriate cooldown if we delay it for another gcd
        // calculation for max cooldown to consider delay:
        // - start with remaining GCD + leeway; if CD is smaller, after next gcd we risk overcap
        // - if we could have >50 gauge after next GCD, we might need one or more FCs, depending on IR, before we have another chance to infuriate
        // - if next GCD is FC, we get extra CD reduction
        var maxInfuriateCD = GCD + InfuriateCDLeeway;
        if (Gauge + GaugeGainedFromAction(NextGCD) > 50)
        {
            var numFCsToBurnGauge = 1;
            if (irActive)
                numFCsToBurnGauge += InnerReleaseStacks;
            else if (!CanFitGCD(InnerReleaseCD, 1))
                numFCsToBurnGauge += 3;
            maxInfuriateCD += (GCDLength + InfuriateCDReduction) * numFCsToBurnGauge;
        }
        else if (NextGCD is WAR.AID.FellCleave or WAR.AID.InnerBeast or WAR.AID.SteelCyclone or WAR.AID.Decimate)
        {
            maxInfuriateCD += InfuriateCDReduction;
        }
        if (InfuriateCD < maxInfuriateCD)
            return (true, false); // we can't safely delay infuriate

        // ok, at this point we are free to either delay or use it
        // if IR is active, prefer not using infuriate, so as not to delay spending stack; there are few reasons:
        // - at L96+, we really don't want to push wrath out of burst window; normally IC will be inside burst anyway, but in some corner cases it's better to have 3xFC+wrath under buffs than IC+2xFC
        // - less chance to waste IR in case of downtime, mob deaths, etc, if there is no plan
        // TODO: there are also arguments in favour of using infuriate ASAP in burst (<L96, or if burst window won't fit all FC's anyway, especially if we choose PR's instead), reconsider...
        if (irActive && unlockedNC)
            return (false, false);

        // at this point, use under burst or delay outside (TODO: reconsider, we might want to be smarter here...)
        return (CanFitGCD(BurstWindowLeft), true);
    }

    private bool ShouldUseUpheaval(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && SurgingTempestLeft > MathF.Max(CD(WAR.AID.Upheaval), World.Client.AnimationLock), // TODO: consider delaying until burst window in opener?..
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => false
    };

    private bool ShouldUsePrimalWrath(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && SurgingTempestLeft > MathF.Max(CD(WAR.AID.PrimalWrath), World.Client.AnimationLock),
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => false
    };

    private bool WantOnslaught(Actor? target, bool reserveLastCharge)
    {
        if (!Player.InCombat)
            return false; // don't use out of combat
        if (!InMeleeRange(target))
            return false; // don't use out of melee range to prevent fucking up player's position
        //if (PositionLockIn <= World.Client.AnimationLock)
        //    return false; // forbidden due to state flags
        if (LostBloodRageStacks == 3)
            return true; // we want to start the burst asap
        var onslaughtCapIn = OnslaughtCapIn;
        var closeToOvercap = onslaughtCapIn < GCD + GCDLength;
        if (LostBloodRageStacks is > 0 and < 4)
        {
            // special logic for stacking bloodrage
            if (LostBloodRageLeft < 3)
                return true; // if we don't use it now, we risk dropping stacks
            return LostBloodRageLeft < 3 || LostBloodRageStacks switch
            {
                1 => closeToOvercap, // at 1 charge, bare minimum (assuming no delays) is 6s to overcap - we need to use two charges in 18 and 36s; realistically we don't want to delay gcds
                2 => onslaughtCapIn < 15, // at 2 charges, bare minimum is 18s to overcap, we add extra 3s of leeway
                _ => true, // at 3 charges, just start burst asap
            };
        }
        if (LostBloodRageStacks == 0 && onslaughtCapIn + 27 > LostBloodRageIn)
            return false; // delay until bloodrage, even if overcapping charges
        if (SurgingTempestLeft <= World.Client.AnimationLock)
            return false; // delay until ST, even if overcapping charges
        if (closeToOvercap)
            return true; // if we won't onslaught now, we risk overcapping charges
        if (reserveLastCharge && OnslaughtCD > 30 + World.Client.AnimationLock)
            return false; // strategy prevents us from using last charge
        if (BurstWindowLeft > World.Client.AnimationLock)
            return true; // use now, since we're under raid buffs
        return onslaughtCapIn <= BurstWindowIn; // use if we won't be able to delay until next raid buffs
    }

    private bool ShouldUseOnslaught(OnslaughtStrategy strategy, Actor? target) => strategy switch
    {
        OnslaughtStrategy.Automatic => GCD >= OnslaughtMinGCD && WantOnslaught(target, true),
        OnslaughtStrategy.Forbid => false,
        OnslaughtStrategy.NoReserve => GCD >= OnslaughtMinGCD && WantOnslaught(target, false),
        OnslaughtStrategy.Force => GCD >= OnslaughtMinGCD,
        OnslaughtStrategy.ForceReserve => GCD >= OnslaughtMinGCD && OnslaughtCD <= 30 + World.Client.AnimationLock,
        OnslaughtStrategy.ReserveTwo => GCD >= OnslaughtMinGCD && OnslaughtCapIn <= GCD,
        OnslaughtStrategy.GapClose => !InMeleeRange(target),
        _ => false,
    };

    private bool IsPotionAlignedWithIR()
    {
        // we have several considerations for potion use:
        // 1. during opener, we generally want to use it after fourth gcd, to align with raidbuffs
        // 1.a. with standard tomahawk+combo opener, this coincides with first window with ST buff up
        // 1.b. with facepull opener, this actually occurs 1 gcd after ST/IR is up; the 4th gcd (first gcd under ST/IR) should be a HS
        // 1.c. however, if raidbuffs actually go out early (eg. right before 4th gcd), we would reconsider and use IC/PR/FC asap; we might not have enough time to react and pot
        // 1.d. if we're doing early IR, we actually need to use potion as soon as ST is up
        // so, at least for now, we're gonna use potion right after IR as long as ST is up
        if (CanFitGCD(SurgingTempestLeft) && CanFitGCD(InnerReleaseLeft) && InnerReleaseStacks == 3 && CanFitGCD(PRLeft))
            return true;

        // another consideration is - if we're smuggling ruin, we want to use pots before last gcd
        // we want at least 5 gcds (3xFC+PR+PR) after IR, and pot covers 12 gcds normally
        if (PrimalRuinationActive && CanFitGCD(PRLeft) && !CanFitGCD(PRLeft, 1) && !CanFitGCD(InnerReleaseCD, 7))
            return true;

        // we want to use potion together with bozja buffs (TODO: reconsider)
        if (LostBuffsLeft > 0 || LostBuffsIn < GCD)
            return true;

        // not aligned
        return false;
    }

    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => IsPotionAlignedWithIR() && (RaidBuffsLeft > 0 || RaidBuffsIn < 30),
        PotionStrategy.AlignWithIR => IsPotionAlignedWithIR(),
        PotionStrategy.Immediate => true,
        _ => false
    };

    private bool ShouldUseLostBloodRage() => LostBloodRageIn < float.MaxValue && LostBloodRageStacks == 0 && OnslaughtCapIn < 18 + LostBloodRageIn;

    private bool ShouldUseLostBuff(float availableIn, float cooldown) => availableIn < float.MaxValue && LostBloodRageStacks switch
    {
        0 => availableIn + cooldown <= LostBloodRageIn + 35 && InnerReleaseCD < GCDLength, // use buffs if bloodrage is on long enough cd and IR is imminent
        4 => true, // use all buffs asap
        _ => false // don't use any actions while stacking bloodrage
    };
}
