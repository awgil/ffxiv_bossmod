using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyRPR : LegacyModule
{
    public enum Track { AOE, Gauge, Bloodstalk, SoulSlice, TrueNorth, Enshroud, ArcaneCircle, Gluttony, Potion }
    public enum AOEStrategy { SingleTarget, AutoOnPrimary, ForceAOE }
    public enum GaugeStrategy { Automatic, ForceExtendDD, HarpeorHarvestMoonIfNotInMelee, HarvestMoonIfNotInMelee, ForceHarvestMoon, ComboFitBeforeDowntime }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum PotionStrategy { Manual, Opener, Burst, Force, Special }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense
        var res = new RotationModuleDefinition("Legacy RPR", "Old pre-refactoring module", "lazylemo", RotationModuleQuality.WIP, BitMask.Build((int)Class.RPR), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 100)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use aoe actions on primary target if profitable")
            .AddOption(AOEStrategy.ForceAOE, "AOE", "Use aoe rotation on primary target even if it's less total damage than single-target");

        res.Define(Track.Gauge).As<GaugeStrategy>("Gauge", uiPriority: 90)
            .AddOption(GaugeStrategy.Automatic, "Automatic")
            .AddOption(GaugeStrategy.ForceExtendDD, "ForceExtendDD", "Force extend DD Target Debuff, potentially overcapping DD")
            .AddOption(GaugeStrategy.HarpeorHarvestMoonIfNotInMelee, "HarpeorHarvestMoonIfNotInMelee", "Use Harpe or HarvestMoon if outside melee")
            .AddOption(GaugeStrategy.HarvestMoonIfNotInMelee, "HarvestMoonIfNotInMelee", "Use only HarvestMoon if outside melee")
            .AddOption(GaugeStrategy.ForceHarvestMoon, "ForceHarvestMoon", "Force Harvest Moon")
            .AddOption(GaugeStrategy.ComboFitBeforeDowntime, "ComboFitBeforeDowntime", "Use combo, unless it can't be finished before downtime");

        res.Define(Track.Bloodstalk).As<OffensiveStrategy>("SOUL", uiPriority: 80)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.SoulSlice).As<OffensiveStrategy>("SS", uiPriority: 70)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.TrueNorth).As<OffensiveStrategy>("TrN", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Enshroud).As<OffensiveStrategy>("ENSH", uiPriority: 50)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.ArcaneCircle).As<OffensiveStrategy>("ARC", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Gluttony).As<OffensiveStrategy>("Glut", uiPriority: 30)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Potion won't be used automatically")
            .AddOption(PotionStrategy.Opener, "Opener")
            .AddOption(PotionStrategy.Burst, "Burst", "2+ minute windows")
            .AddOption(PotionStrategy.Force, "Force")
            .AddOption(PotionStrategy.Special, "Special", "Special (Needs testing)");

        return res;
    }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
    {
        public int ShroudGauge;
        public int SoulGauge;
        public int LemureShroudCount;
        public int VoidShroudCount;
        public float SoulReaverLeft;
        public float EnhancedGibbetLeft;
        public float EnhancedGallowsLeft;
        public float EnhancedVoidReapingLeft;
        public float EnhancedCrossReapingLeft;
        public float BloodsownCircleLeft;
        public float EnshroudedLeft;
        public float ArcaneCircleLeft;
        public float ImmortalSacrificeLeft;
        public float TrueNorthLeft;
        public float EnhancedHarpeLeft;
        public bool HasSoulsow;
        public float TargetDeathDesignLeft;
        public float CircleofSacrificeLeft;
        public bool lastActionisSoD;

        public int NumAOEGCDTargets;
        public bool UseAOERotation;

        public RPR.AID Beststalk => EnhancedGallowsLeft > AnimationLock ? RPR.AID.UnveiledGallows
            : EnhancedGibbetLeft > AnimationLock ? RPR.AID.UnveiledGibbet
            : EnshroudedLeft > AnimationLock ? RPR.AID.LemuresSlice
            : RPR.AID.BloodStalk;
        public RPR.AID BestGallow => EnshroudedLeft > AnimationLock ? RPR.AID.CrossReaping : RPR.AID.Gallows;
        public RPR.AID BestGibbet => EnshroudedLeft > AnimationLock ? RPR.AID.VoidReaping : RPR.AID.Gibbet;
        public RPR.AID BestSow => HasSoulsow ? RPR.AID.HarvestMoon : RPR.AID.SoulSow;
        public RPR.SID ExpectedShadowofDeath => RPR.SID.DeathsDesign;
        public RPR.AID ComboLastMove => (RPR.AID)ComboLastAction;

        public bool Unlocked(RPR.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(RPR.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public override string ToString()
        {
            return $"AOE={NumAOEGCDTargets}, no-dots={ForbidDOTs}, shg={ShroudGauge}, Bloodsown={BloodsownCircleLeft} sog={SoulGauge}, RB={RaidBuffsLeft:f1}, DD={TargetDeathDesignLeft:f1}, EGI={EnhancedGibbetLeft:f1}, EGA={EnhancedGallowsLeft:f1}, CircleofSac={CircleofSacrificeLeft} SoulSlice={CD(RPR.AID.SoulSlice)}, Enshroud={CD(RPR.AID.Enshroud)}, AC={ArcaneCircleLeft}, ACCD={CD(RPR.AID.ArcaneCircle):f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
        }
    }

    private readonly State _state;

    public LegacyRPR(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        _state.HasSoulsow = Player.FindStatus(RPR.SID.Soulsow) != null;

        var gauge = World.Client.GetGauge<ReaperGauge>();
        _state.LemureShroudCount = gauge.LemureShroud;
        _state.VoidShroudCount = gauge.VoidShroud;
        _state.ShroudGauge = gauge.Shroud;
        _state.SoulGauge = gauge.Soul;
        //if (_state.ComboLastMove == RPR.AID.InfernalSlice)
        //    _state.ComboTimeLeft = 0;

        _state.SoulReaverLeft = _state.StatusDetails(Player, RPR.SID.SoulReaver, Player.InstanceID).Left;
        _state.ImmortalSacrificeLeft = _state.StatusDetails(Player, RPR.SID.ImmortalSacrifice, Player.InstanceID).Left;
        _state.ArcaneCircleLeft = _state.StatusDetails(Player, RPR.SID.ArcaneCircle, Player.InstanceID).Left;
        _state.EnhancedGibbetLeft = _state.StatusDetails(Player, RPR.SID.EnhancedGibbet, Player.InstanceID).Left;
        _state.EnhancedGallowsLeft = _state.StatusDetails(Player, RPR.SID.EnhancedGallows, Player.InstanceID).Left;
        _state.EnhancedVoidReapingLeft = _state.StatusDetails(Player, RPR.SID.EnhancedVoidReaping, Player.InstanceID).Left;
        _state.EnhancedCrossReapingLeft = _state.StatusDetails(Player, RPR.SID.EnhancedCrossReaping, Player.InstanceID).Left;
        _state.EnhancedHarpeLeft = _state.StatusDetails(Player, RPR.SID.EnhancedHarpe, Player.InstanceID).Left;
        _state.EnshroudedLeft = _state.StatusDetails(Player, RPR.SID.Enshrouded, Player.InstanceID).Left;
        _state.TrueNorthLeft = _state.StatusDetails(Player, RPR.SID.TrueNorth, Player.InstanceID).Left;
        _state.BloodsownCircleLeft = _state.StatusDetails(Player, RPR.SID.BloodsownCircle, Player.InstanceID).Left;
        _state.CircleofSacrificeLeft = _state.StatusDetails(Player, RPR.SID.CircleofSacrifice, Player.InstanceID).Left;
        //_state.lastActionisSoD = ev.Action.Type == ActionType.Spell && (AID)ev.Action.ID is AID.ShadowofDeath or AID.WhorlofDeath; - note: modules can't really react to actions reliably, they could be recreated at any point...

        // TODO: multidot support
        //var adjTarget = initial;
        //if (_state.Unlocked(AID.WhorlofDeath) && !WithoutDOT(initial.Actor))
        //{
        //    var multidotTarget = Autorot.Hints.PriorityTargets.FirstOrDefault(e => e != initial && !e.ForbidDOTs && e.Actor.Position.InCircle(Player.Position, 5) && WithoutDOT(e.Actor));
        //    if (multidotTarget != null)
        //        adjTarget = multidotTarget;
        //}
        _state.TargetDeathDesignLeft = _state.StatusDetails(primaryTarget, _state.ExpectedShadowofDeath, Player.InstanceID).Left;

        // TODO: see how BRD/DRG do aoes
        var aoe = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.AutoOnPrimary => NumTargetsHitByAOEGCD() >= 3,
            AOEStrategy.ForceAOE => true,
            _ => false
        };

        _state.UpdatePositionals(primaryTarget, GetNextPositional(), _state.TrueNorthLeft > _state.GCD);

        // TODO: refactor all that, it's kinda senseless now
        RPR.AID gcd = GetNextBestGCD(strategy, aoe);
        PushResult(gcd, primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength, aoe);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline, aoe);
        PushResult(ogcd, primaryTarget);
    }

    //protected override void QueueAIActions()
    //{
    //    if (_state.Unlocked(AID.LegSweep))
    //    {
    //        var interruptibleEnemy = Autorot.Hints.PotentialTargets.Find(e => e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false) && e.Actor.Position.InCircle(Player.Position, 25 + e.Actor.HitboxRadius + Player.HitboxRadius));
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.LegSweep), interruptibleEnemy?.Actor, interruptibleEnemy != null);
    //    }
    //    if (_state.Unlocked(AID.SecondWind))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
    //    if (_state.Unlocked(AID.Bloodbath))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
    //    // TODO: true north...
    //}

    public override string DescribeState() => _state.ToString();

    private int NumTargetsHitByAOEGCD() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    //private bool WithoutDOT(Actor a) => RefreshDOT(_state.StatusDetails(a, RPR.SID.DeathsDesign, Player.InstanceID).Left);

    // old RPRRotation
    //private int SoulGaugeGainedFromAction(RPR.AID action) => action switch
    //{
    //    RPR.AID.Slice or RPR.AID.WaxingSlice or RPR.AID.InfernalSlice => 10,
    //    RPR.AID.SoulSlice => 50,
    //    RPR.AID.SoulScythe => 50,
    //    RPR.AID.SpinningScythe or RPR.AID.NightmareScythe => 10,
    //    _ => 0
    //};

    //private int ShroudGaugeGainedFromAction(RPR.AID action) => action switch
    //{
    //    RPR.AID.Gibbet or RPR.AID.Gallows or RPR.AID.Guillotine => 10,
    //    RPR.AID.PlentifulHarvest => 50,
    //    _ => 0
    //};

    //private RPR.AID GetNextSTComboAction(RPR.AID comboLastMove, RPR.AID finisher) => comboLastMove switch
    //{
    //    RPR.AID.WaxingSlice => finisher,
    //    RPR.AID.Slice => RPR.AID.WaxingSlice,
    //    _ => RPR.AID.Slice
    //};

    //private int GetSTComboLength(RPR.AID comboLastMove) => comboLastMove switch
    //{
    //    RPR.AID.WaxingSlice => 1,
    //    RPR.AID.Slice => 2,
    //    _ => 3
    //};

    //private int GetAOEComboLength(RPR.AID comboLastMove) => comboLastMove == RPR.AID.SpinningScythe ? 1 : 2;

    //private RPR.AID GetNextMaimComboAction(RPR.AID comboLastMove) => comboLastMove == RPR.AID.Slice ? RPR.AID.WaxingSlice : RPR.AID.Slice;

    //private RPR.AID GetNextAOEComboAction(RPR.AID comboLastMove) => comboLastMove == RPR.AID.SpinningScythe ? RPR.AID.NightmareScythe : RPR.AID.SpinningScythe;

    private RPR.AID GetNextUnlockedComboAction(bool aoe)
    {
        if (aoe && _state.Unlocked(RPR.AID.SpinningScythe))
        {
            return _state.Unlocked(RPR.AID.NightmareScythe) && _state.ComboLastMove == RPR.AID.SpinningScythe ? RPR.AID.NightmareScythe : RPR.AID.SpinningScythe;
        }
        else
        {
            return _state.ComboLastMove switch
            {
                RPR.AID.WaxingSlice => _state.Unlocked(RPR.AID.InfernalSlice) ?
                RPR.AID.InfernalSlice : RPR.AID.Slice,
                RPR.AID.Slice => _state.Unlocked(RPR.AID.WaxingSlice) ? RPR.AID.WaxingSlice : RPR.AID.Slice,
                _ => RPR.AID.Slice
            };
        }
    }

    private RPR.AID GetNextBSAction(bool aoe)
    {
        if (!aoe)
        {
            if (_state.EnhancedGibbetLeft > _state.GCD)
                return RPR.AID.Gibbet;

            if (_state.EnhancedGallowsLeft > _state.GCD)
                return RPR.AID.Gallows;
        }

        if (aoe)
            return RPR.AID.Guillotine;

        return RPR.AID.Gallows;
    }

    //private bool RefreshDOT(float timeLeft) => timeLeft < _state.GCD;

    private bool ShouldUseBloodstalk(StrategyValues strategy, bool aoe)
    {
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        switch (strategy.Option(Track.Bloodstalk).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                return false;
            case OffensiveStrategy.Force:
                if (_state.SoulGauge >= 50)
                    return true;
                return false;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;

                if (enshrouded)
                    return false;

                if (ShouldUseEnshroud(strategy) && _state.CD(RPR.AID.Enshroud) < _state.GCD)
                    return false;

                if (_state.SoulGauge >= 50 && _state.CD(RPR.AID.Gluttony) > 28 && !aoe && (_state.ComboTimeLeft > 2.5 || _state.ComboTimeLeft == 0) && _state.ShroudGauge <= 90 && _state.CD(RPR.AID.ArcaneCircle) > 9)
                    return true;

                if (_state.SoulGauge == 100 && _state.CD(RPR.AID.Gluttony) > _state.AnimationLock && !aoe && (_state.ComboTimeLeft > 2.5 || _state.ComboTimeLeft == 0) && _state.ShroudGauge <= 90 && _state.ImmortalSacrificeLeft < _state.AnimationLock)
                    return true;

                if (_state.SoulGauge >= 50 && !aoe && (_state.ComboTimeLeft > 2.5 || _state.ComboTimeLeft == 0) && _state.ImmortalSacrificeLeft > _state.AnimationLock && _state.BloodsownCircleLeft > 4.8f && (_state.CD(RPR.AID.SoulSlice) > 30 || _state.CD(RPR.AID.SoulSlice) < 60) && _state.ShroudGauge <= 40)
                    return true;

                if ((_state.CD(RPR.AID.ArcaneCircle) < 9 || _state.CD(RPR.AID.ArcaneCircle) > 60) && _state.ShroudGauge >= 50 && (_state.ComboTimeLeft > 11 || _state.ComboTimeLeft == 0))
                    return false;
                return false;
        }
    }

    private bool ShouldUseGrimSwathe(StrategyValues strategy, bool aoe)
    {
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        switch (strategy.Option(Track.Bloodstalk).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                return false;
            case OffensiveStrategy.Force:
                if (_state.SoulGauge >= 50)
                    return true;
                return false;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;

                if (enshrouded)
                    return false;

                if (_state.SoulGauge >= 50 && _state.CD(RPR.AID.Gluttony) > 28 && aoe && _state.ShroudGauge <= 90)
                    return true;

                if (_state.SoulGauge == 100 && _state.CD(RPR.AID.Gluttony) > _state.AnimationLock && aoe && _state.ShroudGauge <= 90)
                    return true;

                return false;
        }
    }

    private bool ShouldUseGluttony(StrategyValues strategy)
    {
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        bool plentifulReady = _state.Unlocked(RPR.AID.PlentifulHarvest) && ((_state.ImmortalSacrificeLeft > _state.AnimationLock) || (_state.CircleofSacrificeLeft > _state.AnimationLock));
        switch (strategy.Option(Track.Gluttony).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                return false;
            case OffensiveStrategy.Force:
                if (_state.SoulGauge >= 50)
                    return true;
                return false;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;

                if (enshrouded)
                    return false;

                if (!_state.Unlocked(RPR.AID.Gluttony))
                    return false;

                if (ShouldUseEnshroud(strategy))
                    return false;

                if ((!plentifulReady || (plentifulReady && _state.BloodsownCircleLeft > 4.8f)) && (_state.ComboTimeLeft > 5 || _state.ComboTimeLeft == 0) && _state.SoulGauge >= 50 && _state.ShroudGauge <= 80 && _state.TargetDeathDesignLeft > 0)
                    return true;

                return false;
        }
    }

    private bool ShouldUseEnshroud(StrategyValues strategy)
    {
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        switch (strategy.Option(Track.Enshroud).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                return false;

            case OffensiveStrategy.Force:
                if (_state.ShroudGauge >= 50)
                    return true;
                return false;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (soulReaver)
                    return false;
                if (!_state.Unlocked(RPR.AID.Enshroud))
                    return false;

                if (enshrouded)
                    return false;
                if (_state.ArcaneCircleLeft > _state.AnimationLock && _state.ShroudGauge >= 50 && (_state.ComboTimeLeft > 11 || _state.ComboTimeLeft == 0) && _state.CD(RPR.AID.Enshroud) < _state.GCD)
                    return true;
                if ((_state.CD(RPR.AID.ArcaneCircle) < 9 || _state.CD(RPR.AID.ArcaneCircle) > 60) && _state.ShroudGauge >= 50 && (_state.ComboTimeLeft > 11 || _state.ComboTimeLeft == 0) && _state.CD(RPR.AID.Enshroud) < _state.GCD)
                    return true;

                return false;
        }
    }

    private bool ShouldUseArcaneCircle(StrategyValues strategy)
    {
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;

        if (strategy.Option(Track.ArcaneCircle).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return false;

        else if (strategy.Option(Track.ArcaneCircle).As<OffensiveStrategy>() == OffensiveStrategy.Force)
            return true;

        else
        {
            if (!_state.TargetingEnemy)
                return false;
            if (soulReaver)
                return false;

            if (enshrouded && _state.LemureShroudCount is 3 && _state.TargetDeathDesignLeft > 30)
                return true;
            if (_state.ShroudGauge < 50 && !enshrouded && _state.TargetDeathDesignLeft > 0)
                return true;
            return false;
        }
    }

    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Option(Track.Potion).As<PotionStrategy>() switch
    {
        PotionStrategy.Manual => false,
        PotionStrategy.Opener => _state.CD(RPR.AID.ArcaneCircle) > _state.GCD && _state.CD(RPR.AID.SoulSlice) > 0,
        PotionStrategy.Burst => _state.CD(RPR.AID.ArcaneCircle) < 9 && _state.lastActionisSoD && _state.TargetDeathDesignLeft > 28,
        PotionStrategy.Force => true,
        _ => false
    };

    private (Positional, bool) GetNextPositional()
    {
        if (_state.UseAOERotation)
            return default;

        if (_state.Unlocked(RPR.AID.Gibbet) && !_state.UseAOERotation)
        {
            if (_state.EnhancedGibbetLeft > _state.GCD)
                return (Positional.Flank, true);
            if (_state.EnhancedGallowsLeft > _state.GCD)
                return (Positional.Rear, true);

            return (Positional.Rear, true);
        }
        else
        {
            return default;
        }
    }

    private bool ShouldUseTrueNorth(StrategyValues strategy)
    {
        switch (strategy.Option(Track.TrueNorth).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                return false;
            case OffensiveStrategy.Force:
                return true;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (_state.TrueNorthLeft > _state.AnimationLock)
                    return false;
                if (GetNextPositional().Item2 && _state.NextPositionalCorrect && _state.SoulReaverLeft > _state.AnimationLock)
                    return false;
                if (GetNextPositional().Item2 && !_state.NextPositionalCorrect && _state.SoulReaverLeft > _state.AnimationLock)
                    return true;
                if (GetNextPositional().Item2 && !_state.NextPositionalCorrect && ShouldUseGluttony(strategy) && _state.CD(RPR.AID.Gluttony) < 2.5)
                    return true;
                return false;
        }
    }

    private bool ShouldUseSoulSlice(StrategyValues strategy, bool aoe)
    {
        //bool plentifulReady = _state.Unlocked(RPR.AID.PlentifulHarvest) && _state.ImmortalSacrificeLeft > _state.AnimationLock && _state.CircleofSacrificeLeft < _state.GCD;
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        switch (strategy.Option(Track.SoulSlice).As<OffensiveStrategy>())
        {
            case OffensiveStrategy.Delay:
                return false;

            case OffensiveStrategy.Force:
                return true;

            default:
                if (!_state.TargetingEnemy)
                    return false;
                if (_state.SoulGauge <= 50 && _state.CD(RPR.AID.SoulSlice) - 30 < _state.GCD && (_state.ComboTimeLeft > 5 || _state.ComboTimeLeft == 0 || (_state.ArcaneCircleLeft > _state.AnimationLock && _state.ComboTimeLeft > 11)) && _state.CD(RPR.AID.ArcaneCircle) > 11.5f)
                    return true;
                if (enshrouded)
                    return false;
                if (soulReaver)
                    return false;
                if (_state.ArcaneCircleLeft > _state.AnimationLock && _state.ComboTimeLeft < 11)
                    return false;
                return false;
        }
    }

    private RPR.AID GetNextBestGCD(StrategyValues strategy, bool aoe)
    {
        if (!_state.TargetingEnemy)
            return RPR.AID.None;

        bool plentifulReady = _state.Unlocked(RPR.AID.PlentifulHarvest) && _state.ImmortalSacrificeLeft > _state.AnimationLock && _state.CircleofSacrificeLeft < _state.GCD;
        bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        // prepull
        if (_state.CountdownRemaining > 4.2f && !_state.HasSoulsow)
            return RPR.AID.SoulSow;
        if (_state.CountdownRemaining > 1.6f)
            return RPR.AID.None;
        if (_state.CountdownRemaining > 0)
            return RPR.AID.Harpe;

        var gaugeStrategy = strategy.Option(Track.Gauge).As<GaugeStrategy>();
        if (gaugeStrategy == GaugeStrategy.HarvestMoonIfNotInMelee && _state.HasSoulsow && _state.RangeToTarget > 3 && Player.InCombat)
            return RPR.AID.HarvestMoon;
        if (gaugeStrategy == GaugeStrategy.ForceHarvestMoon && _state.HasSoulsow)
            return RPR.AID.HarvestMoon;
        if (gaugeStrategy == GaugeStrategy.HarpeorHarvestMoonIfNotInMelee && _state.RangeToTarget > 3 && !Player.InCombat)
            return RPR.AID.Harpe;
        if (gaugeStrategy == GaugeStrategy.HarpeorHarvestMoonIfNotInMelee && _state.HasSoulsow && _state.RangeToTarget > 3 && Player.InCombat)
            return RPR.AID.HarvestMoon;
        if (gaugeStrategy == GaugeStrategy.HarpeorHarvestMoonIfNotInMelee && !_state.HasSoulsow && _state.RangeToTarget > 3 && Player.InCombat)
            return RPR.AID.Harpe;
        if (gaugeStrategy == GaugeStrategy.ForceExtendDD && _state.Unlocked(RPR.AID.ShadowofDeath) && !soulReaver)
            return aoe ? RPR.AID.WhorlofDeath : RPR.AID.ShadowofDeath;

        var potionStrategy = strategy.Option(Track.Potion).As<PotionStrategy>();
        if (potionStrategy == PotionStrategy.Special && _state.HasSoulsow && (_state.CD(RPR.AID.ArcaneCircle) < 11.5 || _state.CD(RPR.AID.ArcaneCircle) > 115))
        {
            if (_state.CD(RPR.AID.ArcaneCircle) < 11.5f && _state.TargetDeathDesignLeft < 30)
                return RPR.AID.ShadowofDeath;
            if (_state.ComboTimeLeft != 0 || !enshrouded && !soulReaver)
                return GetNextUnlockedComboAction(aoe);
            if (_state.LemureShroudCount is 3 && !_state.lastActionisSoD && _state.PotionCD < 1)
                return RPR.AID.ShadowofDeath;
            if (_state.LemureShroudCount is 1)
                return RPR.AID.HarvestMoon;
            if (enshrouded && !aoe)
            {
                if (_state.Unlocked(RPR.AID.Communio) && _state.LemureShroudCount is 1 && _state.VoidShroudCount is 0)
                    return RPR.AID.Communio;
                if (_state.EnhancedVoidReapingLeft > _state.AnimationLock)
                    return RPR.AID.VoidReaping;
                if (_state.EnhancedCrossReapingLeft > _state.AnimationLock)
                    return RPR.AID.CrossReaping;

                return RPR.AID.CrossReaping;
            }

            return GetNextUnlockedComboAction(aoe);
        }

        if (!aoe)
        {
            if (_state.Unlocked(RPR.AID.ShadowofDeath) && _state.TargetDeathDesignLeft <= _state.GCD + 2.5f && !soulReaver)
                return RPR.AID.ShadowofDeath;
        }
        else
        {
            if (_state.Unlocked(RPR.AID.WhorlofDeath) && _state.TargetDeathDesignLeft <= _state.GCD + 2.5f && !soulReaver)
                return RPR.AID.WhorlofDeath;
        }

        if (plentifulReady && _state.BloodsownCircleLeft < 1 && !soulReaver && !enshrouded && (_state.ComboTimeLeft > 2.5 || _state.ComboTimeLeft == 0))
            return RPR.AID.PlentifulHarvest;

        if ((_state.CD(RPR.AID.Gluttony) < 7.5 && _state.Unlocked(RPR.AID.Gluttony) && !enshrouded && !soulReaver && _state.TargetDeathDesignLeft < 10) || (_state.CD(RPR.AID.Gluttony) > 25 && _state.Unlocked(RPR.AID.Gluttony) && _state.SoulGauge >= 50 && !soulReaver && !enshrouded && _state.TargetDeathDesignLeft < 7.5))
            return RPR.AID.ShadowofDeath;

        if (enshrouded && !aoe)
        {
            if ((_state.LemureShroudCount == 4 || _state.LemureShroudCount == 3) && (!_state.lastActionisSoD || potionStrategy == PotionStrategy.Burst && !_state.lastActionisSoD && _state.PotionCD < 1) && _state.CD(RPR.AID.ArcaneCircle) < 9)
                return RPR.AID.ShadowofDeath;
            if (_state.Unlocked(RPR.AID.Communio) && _state.LemureShroudCount is 1 && _state.VoidShroudCount is 0)
                return RPR.AID.Communio;
            if (_state.Unlocked(RPR.AID.LemuresSlice) && _state.VoidShroudCount >= 2 && _state.CD(RPR.AID.ArcaneCircle) > 10)
                return RPR.AID.LemuresSlice;
            if (_state.EnhancedVoidReapingLeft > _state.AnimationLock)
                return RPR.AID.VoidReaping;
            if (_state.EnhancedCrossReapingLeft > _state.AnimationLock)
                return RPR.AID.CrossReaping;

            return RPR.AID.CrossReaping;
        }

        if (enshrouded && aoe)
        {
            if (_state.CD(RPR.AID.ArcaneCircle) < 6)
                return RPR.AID.WhorlofDeath;
            if (_state.Unlocked(RPR.AID.Communio) && _state.LemureShroudCount is 1 && _state.VoidShroudCount is 0)
                return RPR.AID.Communio;

            return RPR.AID.GrimReaping;
        }

        if (_state.SoulReaverLeft > _state.GCD)
            return GetNextBSAction(aoe);

        if (ShouldUseSoulSlice(strategy, aoe) && aoe)
            return RPR.AID.SoulScythe;

        if (ShouldUseSoulSlice(strategy, aoe) && !aoe)
            return RPR.AID.SoulSlice;

        return GetNextUnlockedComboAction(aoe);
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline, bool aoe)
    {
        if (!_state.TargetingEnemy)
            return default;

        //bool soulReaver = _state.Unlocked(RPR.AID.BloodStalk) && _state.SoulReaverLeft > _state.AnimationLock;
        bool enshrouded = _state.Unlocked(RPR.AID.Enshroud) && _state.EnshroudedLeft > _state.AnimationLock;
        //var (positional, shouldUsePositional) = GetNextPositional();
        //if (strategy.Option(Track.ArcaneCircle).As<OffensiveStrategy>() == Strategy.ArcaneCircleUse.Delay)
        //    return ActionID.MakeSpell(RPR.AID.Enshroud);
        if (strategy.Option(Track.Potion).As<PotionStrategy>() == PotionStrategy.Special && _state.HasSoulsow)
        {
            if (_state.CD(RPR.AID.ArcaneCircle) < 7.5f && _state.ShroudGauge >= 50 && _state.CanWeave(RPR.AID.Enshroud, 0.6f, deadline))
                return ActionID.MakeSpell(RPR.AID.Enshroud);
            if (_state.LemureShroudCount is 3 && _state.CanWeave(_state.PotionCD, 1.1f, deadline) && _state.lastActionisSoD)
                return ActionDefinitions.IDPotionStr;
            if (_state.LemureShroudCount is 2 && _state.CanWeave(RPR.AID.ArcaneCircle, 0.6f, deadline))
                return ActionID.MakeSpell(RPR.AID.ArcaneCircle);
            if (_state.CD(RPR.AID.ArcaneCircle) > 11 && _state.VoidShroudCount >= 2 && _state.CanWeave(RPR.AID.LemuresSlice, 0.6f, deadline))
                return ActionID.MakeSpell(RPR.AID.LemuresSlice);
        }

        if (ShouldUsePotion(strategy) && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionStr;
        if (ShouldUseTrueNorth(strategy) && _state.CanWeave(RPR.AID.TrueNorth - 45, 0.6f, deadline) && !aoe && _state.GCD < 0.8)
            return ActionID.MakeSpell(RPR.AID.TrueNorth);
        if (ShouldUseEnshroud(strategy) && _state.Unlocked(RPR.AID.Enshroud) && _state.CanWeave(RPR.AID.Enshroud, 0.6f, deadline))
            return ActionID.MakeSpell(RPR.AID.Enshroud);
        if (ShouldUseArcaneCircle(strategy) && _state.Unlocked(RPR.AID.ArcaneCircle) && _state.CanWeave(RPR.AID.ArcaneCircle, 0.6f, deadline))
            return ActionID.MakeSpell(RPR.AID.ArcaneCircle);
        if (_state.VoidShroudCount >= 2 && _state.CanWeave(RPR.AID.LemuresSlice, 0.6f, deadline) && !aoe && _state.CD(RPR.AID.ArcaneCircle) > 10)
            return ActionID.MakeSpell(RPR.AID.LemuresSlice);
        if (_state.VoidShroudCount >= 2 && _state.CanWeave(RPR.AID.LemuresSlice, 0.6f, deadline) && aoe && _state.CD(RPR.AID.ArcaneCircle) > 10)
            return ActionID.MakeSpell(RPR.AID.LemuresScythe);
        if (ShouldUseGluttony(strategy) && _state.Unlocked(RPR.AID.Gluttony) && _state.CanWeave(RPR.AID.Gluttony, 0.6f, deadline) && !enshrouded && _state.TargetDeathDesignLeft > 5)
            return ActionID.MakeSpell(RPR.AID.Gluttony);
        if (ShouldUseBloodstalk(strategy, aoe) && _state.Unlocked(RPR.AID.BloodStalk) && _state.CanWeave(RPR.AID.BloodStalk, 0.6f, deadline) && !enshrouded && _state.TargetDeathDesignLeft > 2.5)
            return ActionID.MakeSpell(_state.Beststalk);
        if (ShouldUseGrimSwathe(strategy, aoe) && _state.Unlocked(RPR.AID.GrimSwathe) && _state.CanWeave(RPR.AID.BloodStalk, 0.6f, deadline) && !enshrouded && _state.TargetDeathDesignLeft > 2.5)
            return ActionID.MakeSpell(RPR.AID.GrimSwathe);

        return new();
    }
}
