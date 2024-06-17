using Dalamud.Game.ClientState.JobGauge.Types;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyWAR : LegacyModule
{
    public enum Track { AOE, GCD, Infuriate, Potion, InnerRelease, Upheaval, PrimalRend, Onslaught }
    public enum AOEStrategy { SingleTarget, ForceAOE, Auto, AutoFinishCombo }
    public enum GCDStrategy { Automatic, Spend, ConserveIfNoBuffs, Conserve, ForceExtendST, ForceSPCombo, TomahawkIfNotInMelee, ComboFitBeforeDowntime, PenultimateComboThenSpend, ForceSpend }
    public enum InfuriateStrategy { Automatic, Delay, ForceIfNoNC, AutoUnlessIR, ForceIfChargesCapping }
    public enum PotionStrategy { Manual, Immediate, DelayUntilRaidBuffs, Force }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum OnslaughtStrategy { Automatic, Forbid, NoReserve, Force, ForceReserve, ReserveTwo, UseOutsideMelee }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense (ST stuff, esp things like onslaught?)
        var res = new RotationModuleDefinition("Legacy WAR", "Old pre-refactoring module", BitMask.Build((int)Class.WAR), 90);

        var aoe = res.AddConfig(Track.AOE, new("AOE", UIPriority: 90));
        aoe.AddOption(AOEStrategy.SingleTarget, new(0x80ffffff, ActionTargets.None, "ST", "Use single-target rotation"));
        aoe.AddOption(AOEStrategy.ForceAOE, new(0x8000ffff, ActionTargets.None, "AOE", "Use aoe rotation"));
        aoe.AddOption(AOEStrategy.Auto, new(0x8000ff00, ActionTargets.None, "Auto", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; break combo if necessary"));
        aoe.AddOption(AOEStrategy.AutoFinishCombo, new(0x80ff00ff, ActionTargets.None, "AutoFinishCombo", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation; finish combo route before switching"));

        var gcd = res.AddConfig(Track.GCD, new("Gauge", "GCD", UIPriority: 80));
        gcd.AddOption(GCDStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic")); // spend gauge either under raid buffs or if next downtime is soon (so that next raid buff window won't cover at least 4 GCDs); TODO reconsider...
        gcd.AddOption(GCDStrategy.Spend, new(0x8000ff00, ActionTargets.None, "Spend", "Spend gauge freely, ensure ST is properly maintained"));
        gcd.AddOption(GCDStrategy.ConserveIfNoBuffs, new(0x8000ffff, ActionTargets.None, "ConserveIfNoBuffs", "Conserve unless under raid buffs"));
        gcd.AddOption(GCDStrategy.Conserve, new(0x800000ff, ActionTargets.None, "Conserve", "Conserve as much as possible"));
        gcd.AddOption(GCDStrategy.ForceExtendST, new(0x80ff00ff, ActionTargets.None, "ForceExtendST", "Force extend ST buff, potentially overcapping gauge and/or ST"));
        gcd.AddOption(GCDStrategy.ForceSPCombo, new(0x80ff0080, ActionTargets.None, "ForceSPCombo", "Force SP combo, potentially overcapping gauge"));
        gcd.AddOption(GCDStrategy.TomahawkIfNotInMelee, new(0x80c08000, ActionTargets.None, "TomahawkIfNotInMelee", "Use tomahawk if outside melee"));
        gcd.AddOption(GCDStrategy.ComboFitBeforeDowntime, new(0x80c0c000, ActionTargets.None, "ComboFitBeforeDowntime", "Use combo, unless it can't be finished before downtime and unless gauge and/or ST would overcap"));
        gcd.AddOption(GCDStrategy.PenultimateComboThenSpend, new(0x80400080, ActionTargets.None, "PenultimateComboThenSpend", "Use combo until second-last step, then spend gauge"));
        gcd.AddOption(GCDStrategy.ForceSpend, new(0x8000ffc0, ActionTargets.None, "ForceSpend", "Force gauge spender if possible, even if ST is not up/running out soon"));

        var inf = res.AddConfig(Track.Infuriate, new("Infuriate", UIPriority: 70));
        inf.AddOption(InfuriateStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Try to delay uses until raidbuffs, avoiding overcap"));
        inf.AddOption(InfuriateStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay, even if risking overcap"));
        inf.AddOption(InfuriateStrategy.ForceIfNoNC, new(0x8000ff00, ActionTargets.None, "ForceIfNoNC", "Force unless NC active"));
        inf.AddOption(InfuriateStrategy.AutoUnlessIR, new(0x8000ffff, ActionTargets.None, "AutoUnlessIR", "Use normally, but not during IR"));
        inf.AddOption(InfuriateStrategy.ForceIfChargesCapping, new(0x8000ff80, ActionTargets.None, "ForceIfChargesCapping", "Force use if charges are about to overcap (unless NC is already active), even if it would overcap gauge"));

        var pot = res.AddConfig(Track.Potion, new("Potion", UIPriority: 60));
        pot.AddOption(PotionStrategy.Manual, new(0x80ffffff, ActionTargets.None, "Manual", "Do not use automatically"));
        pot.AddOption(PotionStrategy.Immediate, new(0x8000ff00, ActionTargets.None, "Immediate", "Use ASAP, but delay slightly during opener", 270, 30));
        pot.AddOption(PotionStrategy.DelayUntilRaidBuffs, new(0x8000ffff, ActionTargets.None, "DelayUntilRaidBuffs", "Delay until raidbuffs", 270, 30));
        pot.AddOption(PotionStrategy.Force, new(0x800000ff, ActionTargets.None, "Force", "Use ASAP, even if without ST", 270, 30));

        var ir = res.AddConfig(Track.InnerRelease, new("IR", UIPriority: 50));
        ir.AddOption(OffensiveStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Use normally"));
        ir.AddOption(OffensiveStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay"));
        ir.AddOption(OffensiveStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Force use ASAP (even during downtime or without ST)"));

        var uph = res.AddConfig(Track.Upheaval, new("Upheaval", UIPriority: 40));
        uph.AddOption(OffensiveStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Use normally"));
        uph.AddOption(OffensiveStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay"));
        uph.AddOption(OffensiveStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Force use ASAP (even without ST)"));

        var pr = res.AddConfig(Track.Upheaval, new("PR", UIPriority: 30));
        pr.AddOption(OffensiveStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Use normally"));
        pr.AddOption(OffensiveStrategy.Delay, new(0x800000ff, ActionTargets.None, "Delay", "Delay"));
        pr.AddOption(OffensiveStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Force use ASAP (do not delay to raidbuffs)"));

        var onsl = res.AddConfig(Track.Onslaught, new("Onslaught", UIPriority: 20));
        onsl.AddOption(OnslaughtStrategy.Automatic, new(0x80ffffff, ActionTargets.None, "Automatic", "Always keep one charge reserved, use other charges under raidbuffs or to prevent overcapping"));
        onsl.AddOption(OnslaughtStrategy.Forbid, new(0x800000ff, ActionTargets.None, "Forbid", "Forbid automatic use"));
        onsl.AddOption(OnslaughtStrategy.NoReserve, new(0x8000ffff, ActionTargets.None, "NoReserve", "Do not reserve charges: use all charges if under raidbuffs, otherwise use as needed to prevent overcapping"));
        onsl.AddOption(OnslaughtStrategy.Force, new(0x8000ff00, ActionTargets.None, "Force", "Use all charges ASAP"));
        onsl.AddOption(OnslaughtStrategy.ForceReserve, new(0x80ff0000, ActionTargets.None, "ForceReserve", "Use all charges except one ASAP"));
        onsl.AddOption(OnslaughtStrategy.ReserveTwo, new(0x80ffff00, ActionTargets.None, "ReserveTwo", "Reserve 2 charges, trying to prevent overcap"));
        onsl.AddOption(OnslaughtStrategy.UseOutsideMelee, new(0x80ff00ff, ActionTargets.None, "UseOutsideMelee", "Use as gapcloser if outside melee range"));

        // TODO: consider these:
        //public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net
        //public bool OnslaughtHeadroom; // if true, consider onslaught to have slightly higher animation lock than in reality, to account for potential small movement animation delay

        return res;
    }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
    {
        public int Gauge; // 0 to 100
        public float SurgingTempestLeft; // 0 if buff not up, max 60
        public float NascentChaosLeft; // 0 if buff not up, max 30
        public float PrimalRendLeft; // 0 if buff not up, max 30
        public float InnerReleaseLeft; // 0 if buff not up, max 15
        public int InnerReleaseStacks; // 0 if buff not up, max 3

        // upgrade paths
        public WAR.AID BestFellCleave => NascentChaosLeft > GCD && Unlocked(WAR.AID.InnerChaos) ? WAR.AID.InnerChaos : Unlocked(WAR.AID.FellCleave) ? WAR.AID.FellCleave : WAR.AID.InnerBeast;
        public WAR.AID BestDecimate => NascentChaosLeft > GCD ? WAR.AID.ChaoticCyclone : Unlocked(WAR.AID.Decimate) ? WAR.AID.Decimate : WAR.AID.SteelCyclone;
        public WAR.AID BestInnerRelease => Unlocked(WAR.AID.InnerRelease) ? WAR.AID.InnerRelease : WAR.AID.Berserk;
        public WAR.AID BestBloodwhetting => Unlocked(WAR.AID.Bloodwhetting) ? WAR.AID.Bloodwhetting : WAR.AID.RawIntuition;

        public WAR.AID ComboLastMove => (WAR.AID)ComboLastAction;
        //public float InnerReleaseCD => CD(UnlockedInnerRelease ? AID.InnerRelease : AID.Berserk); // note: technically berserk and IR don't share CD, and with level sync you can have both...

        public bool Unlocked(WAR.AID aid) => WAR.Definitions.Unlocked(aid, Level, UnlockProgress);
        public bool Unlocked(WAR.TraitID tid) => WAR.Definitions.Unlocked(tid, Level, UnlockProgress);

        public override string ToString()
        {
            return $"g={Gauge}, RB={RaidBuffsLeft:f1}, ST={SurgingTempestLeft:f1}, NC={NascentChaosLeft:f1}, PR={PrimalRendLeft:f1}, IR={InnerReleaseStacks}/{InnerReleaseLeft:f1}, IRCD={CD(WAR.AID.Berserk):f1}/{CD(WAR.AID.InnerRelease):f1}, InfCD={CD(WAR.AID.Infuriate):f1}, UphCD={CD(WAR.AID.Upheaval):f1}, OnsCD={CD(WAR.AID.Onslaught):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
        }
    }

    private readonly State _state;
    private readonly QuestLockCheck _questLock = new(WAR.Definitions.UnlockQuests); // TODO: rethink...

    public LegacyWAR(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(ReadOnlySpan<StrategyValue> strategy, Actor? primaryTarget)
    {
        _state.UpdateCommon(primaryTarget, _questLock.Progress());
        _state.HaveTankStance = Player.FindStatus(WAR.SID.Defiance) != null;

        _state.Gauge = Service.JobGauges.Get<WARGauge>().BeastGauge;
        _state.SurgingTempestLeft = _state.StatusDetails(Player, WAR.SID.SurgingTempest, Player.InstanceID).Left;
        _state.NascentChaosLeft = _state.StatusDetails(Player, WAR.SID.NascentChaos, Player.InstanceID).Left;
        _state.PrimalRendLeft = _state.StatusDetails(Player, WAR.SID.PrimalRend, Player.InstanceID).Left;
        (_state.InnerReleaseLeft, _state.InnerReleaseStacks) = _state.StatusDetails(Player, _state.Unlocked(WAR.AID.InnerRelease) ? WAR.SID.InnerRelease : WAR.SID.Berserk, Player.InstanceID);

        var aoe = (AOEStrategy)strategy[(int)Track.AOE].Option switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.Auto => PreferAOE(),
            AOEStrategy.AutoFinishCombo => _state.ComboLastMove switch
            {
                WAR.AID.HeavySwing => !_state.Unlocked(WAR.AID.Maim) && PreferAOE(),
                WAR.AID.Maim => !_state.Unlocked(WAR.AID.StormPath) && PreferAOE(),
                WAR.AID.Overpower => _state.Unlocked(WAR.AID.MythrilTempest) || PreferAOE(),
                _ => PreferAOE()
            },
            _ => false,
        };

        // TODO: refactor all that, it's kinda senseless now
        WAR.AID gcd = GetNextBestGCD(strategy, aoe);
        PushResult(gcd, primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength, aoe);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline, aoe);
        PushResult(ogcd, primaryTarget);
    }

    public override string DescribeState() => _state.ToString();

    private int NumTargetsHitByAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    private bool PreferAOE() => NumTargetsHitByAOE() >= 3;

    // old WARRotation
    private int GaugeGainedFromAction(WAR.AID action) => action switch
    {
        WAR.AID.Maim or WAR.AID.StormEye => 10,
        WAR.AID.StormPath => 20,
        WAR.AID.MythrilTempest => _state.Unlocked(WAR.TraitID.MasteringTheBeast) ? 20 : 0,
        _ => 0
    };

    private WAR.AID GetNextSTComboAction(WAR.AID comboLastMove, WAR.AID finisher) => comboLastMove switch
    {
        WAR.AID.Maim => finisher,
        WAR.AID.HeavySwing => WAR.AID.Maim,
        _ => WAR.AID.HeavySwing
    };

    private int GetSTComboLength(WAR.AID comboLastMove) => comboLastMove switch
    {
        WAR.AID.Maim => 1,
        WAR.AID.HeavySwing => 2,
        _ => 3
    };

    private int GetAOEComboLength(WAR.AID comboLastMove) => comboLastMove == WAR.AID.Overpower ? 1 : 2;

    //private WAR.AID GetNextMaimComboAction(WAR.AID comboLastMove) => comboLastMove == WAR.AID.HeavySwing ? WAR.AID.Maim : WAR.AID.HeavySwing;

    private WAR.AID GetNextAOEComboAction(WAR.AID comboLastMove) => comboLastMove == WAR.AID.Overpower ? WAR.AID.MythrilTempest : WAR.AID.Overpower;

    private WAR.AID GetNextUnlockedComboAction(State state, float minBuffToRefresh, bool aoe)
    {
        if (aoe && state.Unlocked(WAR.AID.Overpower))
        {
            // for AOE rotation, assume dropping ST combo is fine
            return state.Unlocked(WAR.AID.MythrilTempest) && state.ComboLastMove == WAR.AID.Overpower ? WAR.AID.MythrilTempest : WAR.AID.Overpower;
        }
        else
        {
            // for ST rotation, assume dropping AOE combo is fine (HS is 200 pot vs MT 100, is 20 gauge + 30 sec ST worth it?..)
            return state.ComboLastMove switch
            {
                WAR.AID.Maim => state.Unlocked(WAR.AID.StormPath) ? (state.Unlocked(WAR.AID.StormEye) && state.SurgingTempestLeft < minBuffToRefresh ? WAR.AID.StormEye : WAR.AID.StormPath) : WAR.AID.HeavySwing,
                WAR.AID.HeavySwing => state.Unlocked(WAR.AID.Maim) ? WAR.AID.Maim : WAR.AID.HeavySwing,
                _ => WAR.AID.HeavySwing
            };
        }
    }

    private WAR.AID GetNextFCAction(bool aoe)
    {
        // note: under nascent chaos, if IC is not unlocked yet, we want to use cyclone even in non-aoe situations
        if (_state.NascentChaosLeft > _state.GCD)
            return _state.Unlocked(WAR.AID.InnerChaos) && !aoe ? WAR.AID.InnerChaos : WAR.AID.ChaoticCyclone;

        // aoe gauge spender
        if (aoe && _state.Unlocked(WAR.AID.SteelCyclone))
            return _state.Unlocked(WAR.AID.Decimate) ? WAR.AID.Decimate : WAR.AID.SteelCyclone;

        // single-target gauge spender
        return _state.Unlocked(WAR.AID.FellCleave) ? WAR.AID.FellCleave : WAR.AID.InnerBeast;
    }

    // by default, we spend resources either under raid buffs or if another raid buff window will cover at least 4 GCDs of the fight
    private bool ShouldSpendGauge(GCDStrategy strategy, bool aoe) => strategy switch
    {
        GCDStrategy.Automatic or GCDStrategy.TomahawkIfNotInMelee => (_state.RaidBuffsLeft > _state.GCD || _state.FightEndIn <= _state.RaidBuffsIn + 10) && _state.SurgingTempestLeft > _state.GCD,
        GCDStrategy.Spend or GCDStrategy.ForceSpend => true,
        GCDStrategy.ConserveIfNoBuffs => _state.RaidBuffsLeft > _state.GCD,
        GCDStrategy.Conserve => false,
        GCDStrategy.ForceExtendST => false,
        GCDStrategy.ForceSPCombo => false,
        GCDStrategy.ComboFitBeforeDowntime => _state.SurgingTempestLeft > _state.GCD && _state.FightEndIn <= _state.GCD + 2.5f * ((aoe ? GetAOEComboLength(_state.ComboLastMove) : GetSTComboLength(_state.ComboLastMove)) - 1),
        GCDStrategy.PenultimateComboThenSpend => _state.ComboLastMove is WAR.AID.Maim or WAR.AID.Overpower,
        _ => true
    };

    private bool ShouldUseInfuriate(InfuriateStrategy strategy, GCDStrategy gcdStrategy, bool aoe)
    {
        switch (strategy)
        {
            case InfuriateStrategy.Delay:
                return false;

            case InfuriateStrategy.ForceIfNoNC:
                return _state.NascentChaosLeft <= _state.GCD;

            case InfuriateStrategy.ForceIfChargesCapping:
                return _state.NascentChaosLeft <= _state.GCD && _state.CD(WAR.AID.Infuriate) <= _state.AnimationLock;

            default:
                if (!_state.TargetingEnemy)
                    return false; // don't cast during downtime
                if (_state.Gauge > 50)
                    return false; // never cast infuriate if doing so would overcap gauge
                if (_state.NascentChaosLeft > _state.GCD)
                    return false; // never cast infuriate if NC from previous infuriate is still up for next GCD
                if (_state.Unlocked(WAR.AID.ChaoticCyclone) && _state.InnerReleaseLeft > _state.GCD && _state.InnerReleaseLeft <= _state.GCD + 2.5f * _state.InnerReleaseStacks)
                    return false; // never cast infuriate if it will cause us to lose IR stacks

                // different logic before IR and after IR
                if (_state.Unlocked(WAR.AID.InnerRelease))
                {
                    if (strategy == InfuriateStrategy.AutoUnlessIR && _state.InnerReleaseLeft > _state.GCD)
                        return false;

                    // with IR, main purpose of infuriate is to generate gauge to burn in spend mode
                    if (ShouldSpendGauge(gcdStrategy, aoe))
                        return true;

                    // don't delay if we risk overcapping stacks
                    // max safe cooldown calculation:
                    // - start with remaining GCD + grace period; if CD is smaller, by the time we get a chance to reconsider, we'll have 2 stacks
                    //   grace period should at very least be LockDelay, but next-best GCD could be Primal Rend with longer animation lock, plus we might prioritize different oGCDs, so use full extra GCD to be safe
                    // - if next GCD could give us >50 gauge, we'd need one more GCD to cast FC (which would also reduce cd by extra 5 seconds), so add 7.5s
                    // - if IR is imminent, we delay infuriate now, cast some GCD that gives us >50 gauge, we'd need to cast 3xFCs, which would add extra 22.5s
                    // - if IR is active, we delay infuriate now, we might need to spend remaining GCDs on FCs, which would add extra N * 7.5s
                    float maxInfuriateCD = _state.GCD + 2.5f;
                    int gaugeCap = _state.ComboLastMove == WAR.AID.None ? 50 : (_state.ComboLastMove == WAR.AID.HeavySwing ? 40 : 30);
                    if (_state.Gauge > gaugeCap)
                        maxInfuriateCD += 7.5f;
                    bool irImminent = _state.CD(WAR.AID.InnerRelease) < _state.GCD + 2.5;
                    maxInfuriateCD += (irImminent ? 3 : _state.InnerReleaseStacks) * 7.5f;
                    if (_state.CD(WAR.AID.Infuriate) <= maxInfuriateCD)
                        return true;
                }
                else
                {
                    // before IR, main purpose of infuriate is to maximize buffed FCs under Berserk
                    if (_state.InnerReleaseLeft > _state.GCD)
                        return true;

                    // don't delay if we risk overcapping stacks
                    if (_state.CD(WAR.AID.Infuriate) <= _state.GCD + 10)
                        return true;

                    // TODO: consider whether we want to spend both stacks in spend mode if Berserk is not imminent...
                }
                return false;
        }
    }

    // note: this check will not allow using non-forced potions before lvl 50, but who cares...
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.Manual => false,
        PotionStrategy.Immediate => _state.SurgingTempestLeft > 0 || _state.ComboLastMove == WAR.AID.Maim, // TODO: reconsider potion use during opener (delayed IR prefers after maim, early IR prefers after storm eye, to cover third IC on 13th GCD)
        PotionStrategy.DelayUntilRaidBuffs => _state.SurgingTempestLeft > 0 && _state.RaidBuffsLeft > 0,
        PotionStrategy.Force => true,
        _ => false
    };

    // by default, we use IR asap as soon as ST is up
    // TODO: early IR option: technically we can use right after heavy swing, we'll use maim->SE->IC->3xFC
    private bool ShouldUseInnerRelease(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && _state.SurgingTempestLeft > _state.GCD + 5
    };

    // check whether berserk should be delayed (we want to spend it on FCs)
    // this is relevant only until we unlock IR
    private bool ShouldUseBerserk(OffensiveStrategy strategy, bool aoe)
    {
        if (strategy != OffensiveStrategy.Automatic)
            return strategy == OffensiveStrategy.Force;

        if (!Player.InCombat)
            return false; // don't use before pull

        if (!_state.TargetingEnemy)
            return false; // no target, maybe downtime?

        if (_state.Unlocked(WAR.AID.StormEye) && _state.SurgingTempestLeft <= _state.GCD + 5)
            return false; // no ST yet

        if (aoe)
            return true; // don't delay during aoe

        if (_state.Unlocked(WAR.AID.Infuriate))
        {
            // we really want to cast SP + 2xIB or 3xIB under berserk; check whether we'll have infuriate before third GCD
            var availableGauge = _state.Gauge;
            if (_state.CD(WAR.AID.Infuriate) <= 65)
                availableGauge += 50;
            return _state.ComboLastMove switch
            {
                WAR.AID.Maim => availableGauge >= 80, // TODO: this isn't a very good check, improve...
                _ => availableGauge == 150
            };
        }
        else if (_state.Unlocked(WAR.AID.InnerBeast))
        {
            // pre level 50 we ideally want to cast SP + 2xIB under berserk (we need to have 80+ gauge for that)
            // however, we are also content with casting Maim + SP + IB (we need to have 20+ gauge for that; but if we have 70+, it is better to delay for 1 GCD)
            // alternatively, we could delay for 3 GCDs at 40+ gauge - TODO determine which is better
            return _state.ComboLastMove switch
            {
                WAR.AID.HeavySwing => _state.Gauge is >= 20 and < 70,
                WAR.AID.Maim => _state.Gauge >= 80,
                _ => false,
            };
        }
        else
        {
            // pre level 35 there is no point delaying berserk at all
            return true;
        }
    }

    // by default, we use upheaval asap as soon as ST is up
    // TODO: consider delaying for 1 GCD during opener...
    private bool ShouldUseUpheaval(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && _state.SurgingTempestLeft > MathF.Max(_state.CD(WAR.AID.Upheaval), _state.AnimationLock)
    };

    private bool ShouldUseOnslaught(OnslaughtStrategy strategy)
    {
        switch (strategy)
        {
            case OnslaughtStrategy.Forbid:
                return false;
            case OnslaughtStrategy.Force:
                return true;
            case OnslaughtStrategy.ForceReserve:
                return _state.CD(WAR.AID.Onslaught) <= 30 + _state.AnimationLock;
            case OnslaughtStrategy.ReserveTwo:
                return _state.CD(WAR.AID.Onslaught) - (_state.Unlocked(WAR.TraitID.EnhancedOnslaught) ? 0 : 30) <= _state.GCD;
            case OnslaughtStrategy.UseOutsideMelee:
                return _state.RangeToTarget > 3;
            default:
                if (!Player.InCombat)
                    return false; // don't use out of combat
                if (_state.RangeToTarget > 3)
                    return false; // don't use out of melee range to prevent fucking up player's position
                if (_state.PositionLockIn <= _state.AnimationLock)
                    return false; // forbidden due to _state flags
                if (_state.SurgingTempestLeft <= _state.AnimationLock)
                    return false; // delay until ST, even if overcapping charges
                float chargeCapIn = _state.CD(WAR.AID.Onslaught) - (_state.Unlocked(WAR.TraitID.EnhancedOnslaught) ? 0 : 30);
                if (chargeCapIn < _state.GCD + 2.5)
                    return true; // if we won't onslaught now, we risk overcapping charges
                if (strategy != OnslaughtStrategy.NoReserve && _state.CD(WAR.AID.Onslaught) > 30 + _state.AnimationLock)
                    return false; // strategy prevents us from using last charge
                if (_state.RaidBuffsLeft > _state.AnimationLock)
                    return true; // use now, since we're under raid buffs
                return chargeCapIn <= _state.RaidBuffsIn; // use if we won't be able to delay until next raid buffs
        }
    }

    private WAR.AID GetNextBestGCD(ReadOnlySpan<StrategyValue> strategy, bool aoe)
    {
        // prepull or no target
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            return WAR.AID.None;

        // 0. non-standard actions forced by strategy
        // forced PR
        var strategyPR = (OffensiveStrategy)strategy[(int)Track.PrimalRend].Option;
        if (strategyPR == OffensiveStrategy.Force && _state.PrimalRendLeft > _state.GCD)
            return WAR.AID.PrimalRend;
        // forced tomahawk
        var strategyGCD = (GCDStrategy)strategy[(int)Track.GCD].Option;
        if (strategyGCD == GCDStrategy.TomahawkIfNotInMelee && _state.RangeToTarget > 3)
            return WAR.AID.Tomahawk;
        // forced surging tempest combo (TODO: at which point does AOE combo start giving ST?)
        if (strategyGCD == GCDStrategy.ForceExtendST && _state.Unlocked(WAR.AID.StormEye))
            return aoe ? GetNextAOEComboAction(_state.ComboLastMove) : GetNextSTComboAction(_state.ComboLastMove, WAR.AID.StormEye);
        // forced SP combo
        if (strategyGCD == GCDStrategy.ForceSPCombo)
            return GetNextSTComboAction(_state.ComboLastMove, WAR.AID.StormPath);
        // forced combo until penultimate step
        if (strategyGCD == GCDStrategy.PenultimateComboThenSpend && _state.ComboLastMove != WAR.AID.Maim && _state.ComboLastMove != WAR.AID.Overpower && (_state.ComboLastMove != WAR.AID.HeavySwing || _state.Gauge <= 90))
            return aoe ? WAR.AID.Overpower : _state.ComboLastMove == WAR.AID.HeavySwing ? WAR.AID.Maim : WAR.AID.HeavySwing;
        // forced gauge spender
        bool canUseFC = _state.Gauge >= 50 || _state.InnerReleaseStacks > 0 && _state.Unlocked(WAR.AID.InnerRelease);
        if (strategyGCD == GCDStrategy.ForceSpend && canUseFC)
            return GetNextFCAction(aoe);

        // forbid automatic PR when out of melee range, to avoid fucking up player positioning when avoiding mechanics
        float primalRendWindow = (strategyPR == OffensiveStrategy.Delay || _state.RangeToTarget > 3) ? 0 : MathF.Min(_state.PrimalRendLeft, _state.PositionLockIn);
        var irCD = _state.CD(_state.Unlocked(WAR.AID.InnerRelease) ? WAR.AID.InnerRelease : WAR.AID.Berserk);

        bool spendGauge = ShouldSpendGauge(strategyGCD, aoe);
        if (!_state.Unlocked(WAR.AID.InnerRelease))
            spendGauge &= irCD > 5; // TODO: improve...

        // 1. if it is the last CD possible for PR/NC, don't waste them
        bool aggressive = false; // TODO: strategy? or don't care?
        float gcdDelay = _state.GCD + (aggressive ? 0 : 2.5f);
        float secondGCDIn = gcdDelay + 2.5f;
        float thirdGCDIn = gcdDelay + 5f;
        if (primalRendWindow > _state.GCD && primalRendWindow < secondGCDIn)
            return WAR.AID.PrimalRend;
        if (_state.NascentChaosLeft > _state.GCD && _state.NascentChaosLeft < secondGCDIn)
            return GetNextFCAction(aoe);
        if (primalRendWindow > _state.GCD && _state.NascentChaosLeft > _state.GCD && primalRendWindow < thirdGCDIn && _state.NascentChaosLeft < thirdGCDIn)
            return WAR.AID.PrimalRend; // either is fine

        // 2. if IR/berserk is up, don't waste charges
        if (_state.InnerReleaseStacks > 0)
        {
            if (_state.Unlocked(WAR.AID.InnerRelease))
            {
                // only consider not casting FC action if delaying won't cost IR stack
                int fcCastsLeft = _state.InnerReleaseStacks;
                if (_state.NascentChaosLeft > _state.GCD)
                    ++fcCastsLeft;
                if (_state.InnerReleaseLeft <= _state.GCD + fcCastsLeft * 2.5f)
                    return GetNextFCAction(aoe);

                // don't delay if it won't give us anything (but still prefer PR under buffs)
                if (spendGauge || _state.InnerReleaseLeft <= _state.RaidBuffsIn)
                    return primalRendWindow > _state.GCD && spendGauge ? WAR.AID.PrimalRend : GetNextFCAction(aoe);

                // don't delay FC if it can cause infuriate overcap (e.g. we use combo action, gain gauge and then can't spend it in time)
                if (_state.CD(WAR.AID.Infuriate) < _state.GCD + (_state.InnerReleaseStacks + 1) * 7.5f)
                    return GetNextFCAction(aoe);

            }
            else if (_state.Gauge >= 50 && (_state.Unlocked(WAR.AID.FellCleave) || _state.ComboLastMove != WAR.AID.Maim || aoe && _state.Unlocked(WAR.AID.SteelCyclone)))
            {
                // single-target: FC > SE/ST > IB > Maim > HS
                // aoe: Decimate > SC > Combo
                return GetNextFCAction(aoe);
            }
        }

        // 3. no ST (or it will expire if we don't combo asap) => apply buff asap
        // TODO: what if we have really high gauge and low ST? is it worth it to delay ST application to avoid overcapping gauge?
        if (!aoe)
        {
            if (_state.Unlocked(WAR.AID.StormEye) && _state.SurgingTempestLeft <= _state.GCD + 2.5f * GetSTComboLength(_state.ComboLastMove))
                return GetNextSTComboAction(_state.ComboLastMove, WAR.AID.StormEye);
        }
        else
        {
            if (_state.Unlocked(WAR.TraitID.MasteringTheBeast) && _state.SurgingTempestLeft <= _state.GCD + 2.5f * (_state.ComboLastMove != WAR.AID.Overpower ? 2 : 1))
                return GetNextAOEComboAction(_state.ComboLastMove);
        }

        // 4. if we're delaying Infuriate due to gauge, cast FC asap (7.5 for FC)
        if (_state.Gauge > 50 && _state.Unlocked(WAR.AID.Infuriate) && _state.CD(WAR.AID.Infuriate) <= gcdDelay + 7.5)
            return GetNextFCAction(aoe);

        // 5. if we have >50 gauge, IR is imminent, and not spending gauge now will cause us to overcap infuriate, spend gauge asap
        // 30 seconds is for FC + IR + 3xFC - this is 4 gcds (10 sec) and 4 FCs (another 20 sec)
        if (_state.Gauge > 50 && _state.Unlocked(WAR.AID.Infuriate) && _state.CD(WAR.AID.Infuriate) <= gcdDelay + 30 && irCD < secondGCDIn)
            return GetNextFCAction(aoe);

        // 6. if there is no chance we can delay PR until next raid buffs, just cast it now
        if (primalRendWindow > _state.GCD && primalRendWindow <= _state.RaidBuffsIn)
            return WAR.AID.PrimalRend;

        // TODO: do not spend gauge if we're delaying berserk
        if (!spendGauge)
        {
            // we want to delay spending gauge unless doing so will cause us problems later
            var maxSTToAvoidOvercap = 20 + Math.Clamp(irCD, 0, 10);
            var nextCombo = GetNextUnlockedComboAction(_state, maxSTToAvoidOvercap, aoe);
            if (_state.Gauge + GaugeGainedFromAction(nextCombo) <= 100)
                return nextCombo;
        }

        // ok at this point, we just want to spend gauge - either because we're using greedy strategy, or something prevented us from casting combo
        if (primalRendWindow > _state.GCD)
            return WAR.AID.PrimalRend;
        if (canUseFC)
            return GetNextFCAction(aoe);

        // TODO: reconsider min time left...
        return GetNextUnlockedComboAction(_state, strategyGCD == GCDStrategy.ForceSpend ? 0 : gcdDelay + 12.5f, aoe);
    }

    // window-end is either GCD or GCD - time-for-second-ogcd; we are allowed to use ogcds only if their animation lock would complete before window-end
    private ActionID GetNextBestOGCD(ReadOnlySpan<StrategyValue> strategy, float deadline, bool aoe)
    {
        // 0. onslaught as a gap-filler - this should be used asap even if we're delaying GCD, since otherwise we'll probably end up delaying it even more
        var strategyOnslaught = (OnslaughtStrategy)strategy[(int)Track.Onslaught].Option;
        bool wantOnslaught = _state.Unlocked(WAR.AID.Onslaught) && _state.TargetingEnemy && ShouldUseOnslaught(strategyOnslaught);
        if (wantOnslaught && _state.RangeToTarget > 3)
            return ActionID.MakeSpell(WAR.AID.Onslaught);

        // 1. potion
        var strategyPotion = (PotionStrategy)strategy[(int)Track.Potion].Option;
        if (ShouldUsePotion(strategyPotion) && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionStr;

        // 2. inner release / berserk
        var strategyIR = (OffensiveStrategy)strategy[(int)Track.InnerRelease].Option;
        if (_state.Unlocked(WAR.AID.InnerRelease))
        {
            if (ShouldUseInnerRelease(strategyIR) && _state.CanWeave(WAR.AID.InnerRelease, 0.6f, deadline))
                return ActionID.MakeSpell(WAR.AID.InnerRelease);
        }
        else if (_state.Unlocked(WAR.AID.Berserk))
        {
            if (ShouldUseBerserk(strategyIR, aoe) && _state.CanWeave(WAR.AID.Berserk, 0.6f, deadline))
                return ActionID.MakeSpell(WAR.AID.Berserk);
        }

        // 3. upheaval
        // TODO: reconsider priority compared to IR
        var strategyUpheaval = (OffensiveStrategy)strategy[(int)Track.Upheaval].Option;
        if (_state.Unlocked(WAR.AID.Upheaval) && ShouldUseUpheaval(strategyUpheaval) && _state.CanWeave(WAR.AID.Upheaval, 0.6f, deadline))
            return ActionID.MakeSpell(aoe && _state.Unlocked(WAR.AID.Orogeny) ? WAR.AID.Orogeny : WAR.AID.Upheaval);

        // 4. infuriate, if not forbidden and not delayed; note that infuriate can't be used out of combat
        var strategyGCD = (GCDStrategy)strategy[(int)Track.GCD].Option;
        var strategyInfuriate = (InfuriateStrategy)strategy[(int)Track.Infuriate].Option;
        if (_state.Unlocked(WAR.AID.Infuriate) && Player.InCombat && _state.CanWeave(_state.CD(WAR.AID.Infuriate) - 60, 0.6f, deadline) && ShouldUseInfuriate(strategyInfuriate, strategyGCD, aoe))
            return ActionID.MakeSpell(WAR.AID.Infuriate);

        // 5. onslaught, if surging tempest up and not forbidden
        bool onslaughtHeadroom = true; // TODO: customize via strategy?..
        if (wantOnslaught && _state.CanWeave(_state.CD(WAR.AID.Onslaught) - 60, onslaughtHeadroom ? 0.8f : 0.6f, deadline))
            return ActionID.MakeSpell(WAR.AID.Onslaught);

        // no suitable oGCDs...
        return new();
    }
}
