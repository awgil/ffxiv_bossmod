using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyMNK : LegacyModule
{
    public enum Track { AOE, Dash, TrueNorth, DisciplinedFist, Demolish, NextNadi, RiddleOfFire, RiddleOfWind, Brotherhood, TFC, Meditation, PerfectBalance, PBForm1, PBForm2, PBForm3, FormShift, FormShiftForm, Blitz, DragonKick, SSS, Potion }
    public enum AOEStrategy { SingleTarget, AOE, STQOpener }
    public enum DashStrategy { Automatic, Forbid, GapClose }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum NadiChoice { Automatic, Lunar, Solar, LunarDowntime, SolarDowntime }
    public enum FireStrategy { Automatic, Delay, Force, DelayUntilBrotherhood, DelayBeast1, DelayBeast2, DelayBeast3 }
    public enum FormChoice { Automatic, Opo, Raptor, Coeurl }
    public enum FormShiftStrategy { Automatic, Delay }
    public enum BlitzStrategy { Automatic, Delay, DelayUntilMultiTarget }
    public enum DragonKickStrategy { Automatic, Filler }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense
        var res = new RotationModuleDefinition("Legacy MNK", "Old pre-refactoring module", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.MNK), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 210)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AOE, "AOE", "Use aoe rotation on primary target even if it's less total damage than single-target")
            .AddOption(AOEStrategy.STQOpener, "STQOpener");

        res.Define(Track.Dash).As<DashStrategy>("Dash", uiPriority: 200)
            .AddOption(DashStrategy.Automatic, "Automatic", "Only use in opener")
            .AddOption(DashStrategy.Forbid, "Forbid")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use if outside melee range");

        res.Define(Track.TrueNorth).As<OffensiveStrategy>("TrueN", uiPriority: 190)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force")
            .AddAssociatedActions(MNK.AID.TrueNorth);

        res.Define(Track.DisciplinedFist).As<OffensiveStrategy>("DF", uiPriority: 180)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Demolish).As<OffensiveStrategy>("Demo", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.NextNadi).As<NadiChoice>("Nadi", uiPriority: 160)
            .AddOption(NadiChoice.Automatic, "Automatic", "Lunar -> Solar")
            .AddOption(NadiChoice.Lunar, "Lunar")
            .AddOption(NadiChoice.Solar, "Solar")
            .AddOption(NadiChoice.LunarDowntime, "LunarDowntime", "Lunar (downtime)")
            .AddOption(NadiChoice.SolarDowntime, "SolarDowntime", "Solar (downtime)");

        res.Define(Track.RiddleOfFire).As<FireStrategy>("RoF", uiPriority: 150)
            .AddOption(FireStrategy.Automatic, "Automatic", "Use on cooldown-ish if something is targetable")
            .AddOption(FireStrategy.Delay, "Delay", "Don't use")
            .AddOption(FireStrategy.Force, "Force", "Force use")
            .AddOption(FireStrategy.DelayUntilBrotherhood, "DelayUntilBrotherhood", "Delay until Brotherhood is off cooldown")
            .AddOption(FireStrategy.DelayBeast1, "DelayBeast1", "Delay until 1 Beast Chakra is opened")
            .AddOption(FireStrategy.DelayBeast2, "DelayBeast2", "Delay until 2 Beast Chakra are opened")
            .AddOption(FireStrategy.DelayBeast3, "DelayBeast3", "Delay until 3 Beast Chakra are opened");

        res.Define(Track.RiddleOfWind).As<OffensiveStrategy>("RoW", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Brotherhood).As<OffensiveStrategy>("BH", uiPriority: 130)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.TFC).As<OffensiveStrategy>("TFC", uiPriority: 120)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Meditation).As<OffensiveStrategy>("Meditate", uiPriority: 110)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.PerfectBalance).As<OffensiveStrategy>("PB", uiPriority: 100)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.PBForm1).As<FormChoice>("PB1", uiPriority: 90)
            .AddOption(FormChoice.Automatic, "Automatic")
            .AddOption(FormChoice.Opo, "Opo", "Opo-Opo")
            .AddOption(FormChoice.Raptor, "Raptor")
            .AddOption(FormChoice.Coeurl, "Coeurl");

        res.Define(Track.PBForm2).As<FormChoice>("PB2", uiPriority: 80)
            .AddOption(FormChoice.Automatic, "Automatic")
            .AddOption(FormChoice.Opo, "Opo", "Opo-Opo")
            .AddOption(FormChoice.Raptor, "Raptor")
            .AddOption(FormChoice.Coeurl, "Coeurl");

        res.Define(Track.PBForm3).As<FormChoice>("PB3", uiPriority: 70)
            .AddOption(FormChoice.Automatic, "Automatic")
            .AddOption(FormChoice.Opo, "Opo", "Opo-Opo")
            .AddOption(FormChoice.Raptor, "Raptor")
            .AddOption(FormChoice.Coeurl, "Coeurl");

        res.Define(Track.FormShift).As<FormShiftStrategy>("FS", uiPriority: 60)
            .AddOption(FormShiftStrategy.Automatic, "Automatic", "Use if there are no targets in range")
            .AddOption(FormShiftStrategy.Delay, "Delay", "Do not use");

        res.Define(Track.FormShiftForm).As<FormChoice>("FSForm", uiPriority: 50)
            .AddOption(FormChoice.Automatic, "Automatic")
            .AddOption(FormChoice.Opo, "Opo", "Opo-Opo")
            .AddOption(FormChoice.Raptor, "Raptor")
            .AddOption(FormChoice.Coeurl, "Coeurl");

        res.Define(Track.Blitz).As<BlitzStrategy>("Blitz", uiPriority: 40)
            .AddOption(BlitzStrategy.Automatic, "Automatic", "Use when available")
            .AddOption(BlitzStrategy.Delay, "Delay")
            .AddOption(BlitzStrategy.DelayUntilMultiTarget, "DelayUntilMultiTarget", "Delay until at least two targets are in range");

        res.Define(Track.DragonKick).As<DragonKickStrategy>("DK", uiPriority: 30)
            .AddOption(DragonKickStrategy.Automatic, "Automatic", "Standard rotation, use in opo-opo form to proc leaden fist")
            .AddOption(DragonKickStrategy.Filler, "Filler", "Replace all GCDs unless Leaden Fist is active or Disciplined Fist will expire");

        res.Define(Track.SSS).As<OffensiveStrategy>("SSS", uiPriority: 20)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Potion).As<OffensiveStrategy>("Potion", uiPriority: 10)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        return res;
    }

    public enum Form { None, OpoOpo, Raptor, Coeurl }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
    {
        public int Chakra; // 0-5
        public BeastChakraType[] BeastChakra = [];
        public NadiFlags Nadi;
        public Form Form;
        public float BlitzLeft; // 20 max
        public float FormLeft; // 0 if no form, 30 max
        public float DisciplinedFistLeft; // 15 max
        public float LeadenFistLeft; // 30 max
        public float TargetDemolishLeft; // TODO: this shouldn't be here...
        public float PerfectBalanceLeft; // 20 max
        public float FormShiftLeft; // 30 max
        public float FireLeft; // 20 max
        public float TrueNorthLeft; // 10 max
        public float LostExcellenceLeft; // 60(?) max
        public float FoPLeft; // 30 max
        public float HsacLeft; // 15 max

        public int NumBlitzTargets; // 5y around self
        public int NumPointBlankAOETargets; // 5y around self
        public int NumEnlightenmentTargets; // 10y/4y rect

        public bool UseAOE;

        public bool UseSTQOpener;

        public float ActualFightEndIn => FightEndIn == 0 ? 10000f : FightEndIn;

        public bool HasLunar => Nadi.HasFlag(NadiFlags.Lunar);
        public bool HasSolar => Nadi.HasFlag(NadiFlags.Solar);
        public bool HasBothNadi => HasLunar && HasSolar;

        public bool CanFormShift => Unlocked(MNK.AID.FormShift) && PerfectBalanceLeft == 0;

        public int BeastCount => BeastChakra.Count(x => x != BeastChakraType.None);

        public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HasBothNadi;
        public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HasBothNadi;

        // upgrade paths
        public MNK.AID BestForbiddenChakra => Unlocked(MNK.AID.ForbiddenChakra) ? MNK.AID.ForbiddenChakra : MNK.AID.SteelPeak;
        public MNK.AID BestEnlightenment => Unlocked(MNK.AID.Enlightenment) ? MNK.AID.Enlightenment : MNK.AID.HowlingFist;
        public MNK.AID BestShadowOfTheDestroyer => Unlocked(MNK.AID.ShadowOfTheDestroyer) ? MNK.AID.ShadowOfTheDestroyer : MNK.AID.ArmOfTheDestroyer;
        public MNK.AID BestRisingPhoenix => Unlocked(MNK.AID.RisingPhoenix) ? MNK.AID.RisingPhoenix : MNK.AID.FlintStrike;
        public MNK.AID BestPhantomRush => Unlocked(MNK.AID.PhantomRush) ? MNK.AID.PhantomRush : MNK.AID.TornadoKick;

        public MNK.AID BestBlitz
        {
            get
            {
                if (BeastCount != 3)
                    return MNK.AID.MasterfulBlitz;

                if (HasLunar && HasSolar)
                    return BestPhantomRush;

                var bc = BeastChakra;

                if (bc[0] == bc[1] && bc[1] == bc[2])
                    return MNK.AID.ElixirField;
                if (bc[0] != bc[1] && bc[1] != bc[2] && bc[0] != bc[2])
                    return BestRisingPhoenix;
                return MNK.AID.CelestialRevolution;
            }
        }

        public bool Unlocked(MNK.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(MNK.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public override string ToString()
        {
            return $"AOE={NumPointBlankAOETargets}/{NumEnlightenmentTargets}, no-dots={ForbidDOTs}, RB={RaidBuffsLeft:f1}, Demo={TargetDemolishLeft:f1}, DF={DisciplinedFistLeft:f1}, Blitz={BlitzLeft:f1}, Form={Form}/{FormLeft:f1}, LFist={LeadenFistLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
        }
    }

    private readonly State _state;
    public bool Debug; // make configurable? idk? only rotation devs would care about this

    private const float SSSApplicationDelay = 0.62f;

    public LegacyMNK(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);

        var gauge = GetGauge<MonkGauge>();
        _state.Chakra = gauge.Chakra;
        _state.BeastChakra = gauge.BeastChakra;
        _state.Nadi = gauge.Nadi;
        _state.BlitzLeft = gauge.BlitzTimeRemaining / 1000f;

        (_state.Form, _state.FormLeft) = DetermineForm();
        _state.DisciplinedFistLeft = _state.StatusDetails(Player, MNK.SID.DisciplinedFist, Player.InstanceID).Left;
        _state.LeadenFistLeft = _state.StatusDetails(Player, MNK.SID.LeadenFist, Player.InstanceID).Left;
        _state.PerfectBalanceLeft = _state.StatusDetails(Player, MNK.SID.PerfectBalance, Player.InstanceID).Left;
        _state.FormShiftLeft = _state.StatusDetails(Player, MNK.SID.FormlessFist, Player.InstanceID).Left;
        _state.FireLeft = _state.StatusDetails(Player, MNK.SID.RiddleOfFire, Player.InstanceID).Left;
        _state.TrueNorthLeft = _state.StatusDetails(Player, MNK.SID.TrueNorth, Player.InstanceID).Left;

        // these are functionally the same as far as the rotation is concerned
        _state.LostExcellenceLeft = MathF.Max(
            _state.StatusDetails(Player, MNK.SID.LostExcellence, Player.InstanceID).Left,
            _state.StatusDetails(Player, MNK.SID.Memorable, Player.InstanceID).Left
        );
        _state.FoPLeft = _state.StatusDetails(Player, MNK.SID.LostFontofPower, Player.InstanceID).Left;
        _state.HsacLeft = _state.StatusDetails(Player, MNK.SID.BannerHonoredSacrifice, Player.InstanceID).Left;

        // TODO: multidot support
        _state.TargetDemolishLeft = _state.StatusDetails(primaryTarget, MNK.SID.Demolish, Player.InstanceID).Left;

        // TODO: see how BRD/DRG do aoes
        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        _state.NumBlitzTargets = NumTargetsHitByBlitz(primaryTarget);
        _state.NumPointBlankAOETargets = aoeStrategy == AOEStrategy.SingleTarget ? 0 : NumTargetsHitByPBAOE();
        _state.NumEnlightenmentTargets = primaryTarget != null && aoeStrategy != AOEStrategy.SingleTarget && _state.Unlocked(MNK.AID.HowlingFist) ? NumTargetsHitByEnlightenment(primaryTarget) : 0;

        _state.UseAOE = _state.NumPointBlankAOETargets >= 3;
        _state.UseSTQOpener = aoeStrategy == AOEStrategy.STQOpener;

        _state.UpdatePositionals(primaryTarget, GetNextPositional(strategy), _state.TrueNorthLeft > _state.GCD);

        // TODO: refactor all that, it's kinda senseless now
        MNK.AID gcd = GetNextBestGCD(strategy);
        PushResult(gcd, primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength, deadline);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline, deadline);
        PushResult(ogcd, primaryTarget);
    }

    //protected override void QueueAIActions()
    //{
    //    if (_state.Unlocked(AID.SecondWind))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
    //    if (_state.Unlocked(AID.Bloodbath))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.Bloodbath), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
    //    if (_state.Unlocked(AID.Meditation))
    //        SimulateManualActionForAI(ActionID.MakeSpell(AID.Meditation), Player, !Player.InCombat && _state.Chakra < 5);
    //    // TODO: this ends up being super annoying in some cases, maybe reconsider conditions
    //    // if (_state.Unlocked(AID.FormShift))
    //    //     SimulateManualActionForAI(ActionID.MakeSpell(AID.FormShift), Player, !Player.InCombat && _state.FormShiftLeft == 0 && _state.PerfectBalanceLeft == 0);
    //}

    public override string DescribeState() => _state.ToString();

    private (Form, float) DetermineForm()
    {
        var s = _state.StatusDetails(Player, MNK.SID.OpoOpoForm, Player.InstanceID).Left;
        if (s > 0)
            return (Form.OpoOpo, s);
        s = _state.StatusDetails(Player, MNK.SID.RaptorForm, Player.InstanceID).Left;
        if (s > 0)
            return (Form.Raptor, s);
        s = _state.StatusDetails(Player, MNK.SID.CoeurlForm, Player.InstanceID).Left;
        if (s > 0)
            return (Form.Coeurl, s);
        return (Form.None, 0);
    }

    private int NumTargetsHitByBlitz(Actor? primaryTarget)
    {
        if (_state.BestBlitz is MNK.AID.TornadoKick or MNK.AID.PhantomRush)
            return primaryTarget == null ? 0 : Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 5);
        return Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    }

    private int NumTargetsHitByPBAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    private int NumTargetsHitByEnlightenment(Actor primary) => Hints.NumPriorityTargetsInAOERect(Player.Position, (primary.Position - Player.Position).Normalized(), 10, _state.Unlocked(MNK.AID.Enlightenment) ? 2 : 1);

    // old MNKRotation
    private MNK.AID GetOpoOpoFormAction()
    {
        // TODO: what should we use if form is not up?..
        if (_state.Unlocked(MNK.AID.ArmOfTheDestroyer) && _state.UseAOE)
            return _state.BestShadowOfTheDestroyer;

        if (_state.Unlocked(MNK.AID.DragonKick) && _state.LeadenFistLeft <= _state.GCD)
            return MNK.AID.DragonKick;

        return MNK.AID.Bootshine;
    }

    private MNK.AID GetRaptorFormAction(StrategyValues strategy)
    {
        // TODO: low level - consider early restart...
        // TODO: better threshold for buff reapplication...
        if (_state.Unlocked(MNK.AID.FourPointFury) && _state.UseAOE)
            return MNK.AID.FourPointFury;

        if (!_state.Unlocked(MNK.AID.TwinSnakes))
            return MNK.AID.TrueStrike;

        // TODO: this is kind of a hack
        if (_state.FireLeft > _state.GCD && _state.PerfectBalanceLeft > _state.GCD)
            return MNK.AID.TwinSnakes;

        var rofIsAligned = _state.FireLeft > _state.GCD || ShouldUseRoF(strategy, _state.GCD + _state.AttackGCDTime * 4);

        // during fire windows, if next GCD is demo, force refresh to align loop; we can't use a lunar PB unless
        // DF + demo are close to max duration, since DF only lasts about 7-8 GCDs and a blitz window is 5
        if (rofIsAligned && NeedDemolishRefresh(strategy, 4))
            return MNK.AID.TwinSnakes;

        // force refresh if we anticipate another PB use in this buff window
        if (_state.FireLeft >= _state.GCD + _state.AttackGCDTime * 3 &&
            _state.CanWeave(_state.CD(MNK.AID.PerfectBalance) - 40, 0.6f, _state.GCD + _state.AttackGCDTime) &&
            _state.PerfectBalanceLeft == 0 &&
            _state.HasSolar)
        {
            return MNK.AID.TwinSnakes;
        }

        // normal refresh
        if (NeedDFRefresh(strategy, 3))
            return MNK.AID.TwinSnakes;

        return MNK.AID.TrueStrike;
    }

    private MNK.AID GetCoeurlFormAction(StrategyValues strategy)
    {
        // TODO: multidot support...
        // TODO: low level - consider early restart...
        // TODO: better threshold for debuff reapplication...
        if (_state.Unlocked(MNK.AID.Rockbreaker) && _state.UseAOE)
            return MNK.AID.Rockbreaker;

        // normal refresh
        if (!_state.ForbidDOTs && _state.Unlocked(MNK.AID.Demolish) && NeedDemolishRefresh(strategy, 3))
            return MNK.AID.Demolish;

        return MNK.AID.SnapPunch;
    }

    private MNK.AID GetNextComboAction(StrategyValues strategy)
    {
        var form = GetEffectiveForm(strategy);
        if (form == Form.Coeurl && _state.Unlocked(MNK.AID.SnapPunch))
            return GetCoeurlFormAction(strategy);

        if (form == Form.Raptor && _state.Unlocked(MNK.AID.TrueStrike))
            return GetRaptorFormAction(strategy);

        return GetOpoOpoFormAction();
    }

    private MNK.AID GetNextBestGCD(StrategyValues strategy)
    {
        // tradeoff here between always using meditation + form shift when not in combat ("optimal") versus only using
        // them during countdowns (mostly optimal).
        // the tradeoff is that "not in combat" includes the scenario of manually targeting an enemy you want to attack,
        // even if they're already in melee range, which incurs an annoying 3s delay
        // maybe AI mode should separately handle the out of combat form shift + meditate usage?
        var formShiftUse = strategy.Option(Track.FormShift).As<FormShiftStrategy>();
        if (_state.CountdownRemaining > 0)
        {
            if (_state.Chakra < 5 && _state.Unlocked(MNK.AID.Meditation))
                return MNK.AID.Meditation;

            if (formShiftUse == FormShiftStrategy.Automatic && _state.FormShiftLeft < 3 && _state.CanFormShift)
                return MNK.AID.FormShift;

            if (_state.CountdownRemaining < 10)
            {
                // form shift on countdown. TODO: ignore Never here? don't think there's ever any reason not to use it on countdown
                if (formShiftUse == FormShiftStrategy.Automatic && _state.CountdownRemaining > 9 && _state.FormShiftLeft < 15 && _state.Unlocked(MNK.AID.FormShift))
                    return MNK.AID.FormShift;

                return MNK.AID.None;
            }
        }

        if (!HaveTarget())
        {
            if (_state.Chakra < 5 && _state.Unlocked(MNK.AID.Meditation) && strategy.Option(Track.Meditation).As<OffensiveStrategy>() != OffensiveStrategy.Delay)
                return MNK.AID.Meditation;

            if (formShiftUse == FormShiftStrategy.Automatic && _state.CanFormShift && _state.FormShiftLeft < 3)
                return MNK.AID.FormShift;

            var nextNadi = strategy.Option(Track.NextNadi).As<NadiChoice>();
            if (nextNadi == NadiChoice.LunarDowntime && _state.BeastCount < 3 && _state.PerfectBalanceLeft > 0)
                return MNK.AID.ShadowOfTheDestroyer;

            if (nextNadi == NadiChoice.SolarDowntime && _state.PerfectBalanceLeft > 0)
                return _state.BeastCount switch
                {
                    0 => MNK.AID.ShadowOfTheDestroyer,
                    1 => MNK.AID.FourPointFury,
                    2 => MNK.AID.Rockbreaker,
                    _ => MNK.AID.None
                };

            return MNK.AID.None;
        }

        if (_state.RangeToTarget > 3 && strategy.Option(Track.Dash).As<DashStrategy>() == DashStrategy.GapClose && _state.CD(MNK.AID.Thunderclap) <= 60 && _state.Unlocked(MNK.AID.Thunderclap))
            return MNK.AID.Thunderclap;

        var sssStrategy = strategy.Option(Track.SSS).As<OffensiveStrategy>();
        if (_state.Unlocked(MNK.AID.SixSidedStar) && sssStrategy == OffensiveStrategy.Force)
            return MNK.AID.SixSidedStar;

        if (_state.UseSTQOpener && _state.LostExcellenceLeft > 0 && _state.FoPLeft == 0)
            return MNK.AID.SixSidedStar;

        if (_state.BestBlitz != MNK.AID.MasterfulBlitz && _state.NumBlitzTargets > 0 && ShouldBlitz(strategy))
            return _state.BestBlitz;

        // TODO: calculate optimal DK spam before SSS
        if (sssStrategy == OffensiveStrategy.Automatic && _state.ActualFightEndIn < _state.GCD + _state.AttackGCDTime + SSSApplicationDelay && _state.Unlocked(MNK.AID.SixSidedStar))
            return MNK.AID.SixSidedStar;

        if (_state.Unlocked(MNK.AID.DragonKick) && ShouldDKSpam(strategy))
            return MNK.AID.DragonKick;

        return GetNextComboAction(strategy);
    }

    private (Positional, bool) GetNextPositional(StrategyValues strategy)
    {
        if (_state.UseAOE)
            return (Positional.Any, false);

        var curForm = GetEffectiveForm(strategy);

        var gcdsUntilCoeurl = curForm switch
        {
            Form.Coeurl => 3,
            Form.Raptor => 4,
            _ => 5
        };

        var isCastingGcd = _state.AttackGCDTime - 0.500 < _state.GCD;
        var formIsPending = _state.FormLeft == 1000;
        // the previous form sticks around for about 200ms before being updated. this results in an off-by-one error
        // in the refresh calculation that causes an annoying flickering effect in the positionals predictor.
        // if we know a form swap is imminent, bump the predicted GCD count back.
        // if PB is active, the current "form" is updated instantly since it's based on job gauge instead of a status effect,
        // so skip the adjustment
        if (isCastingGcd && !formIsPending && _state.PerfectBalanceLeft == 0)
            gcdsUntilCoeurl -= 1;

        var willDemolish = _state.Unlocked(MNK.AID.Demolish) && NeedDemolishRefresh(strategy, gcdsUntilCoeurl);

        return (willDemolish ? Positional.Rear : Positional.Flank, curForm == Form.Coeurl);
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline, float finalOGCDDeadline)
    {
        // TODO: potion

        if (_state.CountdownRemaining > 0)
        {
            if (_state.CountdownRemaining < 0.2 && _state.RangeToTarget > 3 && strategy.Option(Track.Dash).As<DashStrategy>() != DashStrategy.Forbid && _state.Unlocked(MNK.AID.Thunderclap))
                return ActionID.MakeSpell(MNK.AID.Thunderclap);

            if (strategy.Option(Track.Potion).As<OffensiveStrategy>() == OffensiveStrategy.Force && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
                return ActionDefinitions.IDPotionStr;

            return new();
        }

        if (_state.UseSTQOpener && HaveTarget())
        {
            var hsac = BozjaActionID.GetNormal(BozjaHolsterID.BannerHonoredSacrifice);
            var fop = BozjaActionID.GetNormal(BozjaHolsterID.LostFontOfPower);
            var ex = BozjaActionID.GetNormal(BozjaHolsterID.LostExcellence);

            var hsacInBag = _state.Module.World.Client.BozjaHolster[(int)BozjaHolsterID.BannerHonoredSacrifice] > 0;
            var hsacSlot = _state.FindDutyActionSlot(hsac, fop);
            var exSlot = _state.FindDutyActionSlot(ex, fop);

            if (_state.LostExcellenceLeft > 0)
            {
                if (_state.HsacLeft > 0)
                {
                    if (_state.FoPLeft > 0)
                    {
                        if (_state.CanWeave(_state.PotionCD, 0.6f, deadline))
                            return ActionDefinitions.IDPotionStr;
                    }

                    if (_state.CanWeave(_state.DutyActionCD(fop), 0.6f, deadline))
                        return fop;
                }

                if (_state.CanWeave(_state.DutyActionCD(hsac), 0.6f, deadline))
                    return hsac;

                if (hsacSlot < 0)
                    return ActionID.MakeBozjaHolster(BozjaHolsterID.BannerHonoredSacrifice, exSlot);
            }

            if (_state.Form == Form.Raptor && hsacInBag && exSlot >= 0 && _state.CanWeave(_state.DutyActionCD(ex), 0.6f, deadline))
                return ex;
        }

        if (_state.GCD <= 0.800f && ShouldUseRoF(strategy, deadline))
        {
            // this is checked separately here because other functions (notably ShouldUsePB) make decisions
            // based on whether RoF is expected to be off cooldown by a given time
            var shouldRoFDelayed = strategy.Option(Track.RiddleOfFire).As<FireStrategy>() switch
            {
                FireStrategy.DelayBeast1 => _state.BeastCount >= 1,
                FireStrategy.DelayBeast2 => _state.BeastCount >= 2,
                FireStrategy.DelayBeast3 => _state.BeastCount == 3,
                _ => true
            };
            if (shouldRoFDelayed)
                return ActionID.MakeSpell(MNK.AID.RiddleOfFire);
        }

        if (strategy.Option(Track.Potion).As<OffensiveStrategy>() == OffensiveStrategy.Force && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionStr;

        if (ShouldUseBrotherhood(strategy, deadline))
            return ActionID.MakeSpell(MNK.AID.Brotherhood);

        if (ShouldUsePB(strategy, deadline))
            return ActionID.MakeSpell(MNK.AID.PerfectBalance);

        // 2. steel peek, if have chakra
        if (ShouldUseTFC(strategy, deadline))
        {
            // L15 Steel Peak is 180p
            // L40 Howling Fist is 100p/target => HF at 2+ targets
            // L54 Forbidden Chakra is 340p => HF at 4+ targets
            // L72 Enlightenment is 170p/target => at 2+ targets
            if (_state.Unlocked(MNK.AID.Enlightenment))
                return ActionID.MakeSpell(_state.NumEnlightenmentTargets >= 2 ? MNK.AID.Enlightenment : MNK.AID.ForbiddenChakra);
            else if (_state.Unlocked(MNK.AID.ForbiddenChakra))
                return ActionID.MakeSpell(_state.NumEnlightenmentTargets >= 4 ? MNK.AID.HowlingFist : MNK.AID.ForbiddenChakra);
            else if (_state.Unlocked(MNK.AID.HowlingFist))
                return ActionID.MakeSpell(_state.NumEnlightenmentTargets >= 2 ? MNK.AID.HowlingFist : MNK.AID.SteelPeak);
            else
                return ActionID.MakeSpell(MNK.AID.SteelPeak);
        }

        if (ShouldUseRoW(strategy, deadline))
            return ActionID.MakeSpell(MNK.AID.RiddleOfWind);

        if (ShouldUseTrueNorth(strategy, finalOGCDDeadline) && _state.CanWeave(_state.CD(MNK.AID.TrueNorth) - 45, 0.6f, deadline))
            return ActionID.MakeSpell(MNK.AID.TrueNorth);

        if (ShouldDash(strategy))
            return ActionID.MakeSpell(MNK.AID.Thunderclap);

        // no suitable oGCDs...
        return new();
    }

    private Form GetEffectiveForm(StrategyValues strategy)
    {
        if (_state.PerfectBalanceLeft > _state.GCD)
        {
            var formOverride = strategy.Option(Track.PBForm1 + _state.BeastCount).As<FormChoice>();
            switch (formOverride)
            {
                case FormChoice.Opo:
                    return Form.OpoOpo;
                case FormChoice.Coeurl:
                    return Form.Coeurl;
                case FormChoice.Raptor:
                    return Form.Raptor;
                default:
                    break;
            }

            bool canCoeurl, canRaptor, canOpo;

            var nextNadi = strategy.Option(Track.NextNadi).As<NadiChoice>();
            // if a blitz is already in progress, finish it even if buffs would fall off in the process, since celestial revolution is always a mistake
            var forcedLunar = nextNadi == NadiChoice.Lunar || _state.ForcedLunar;
            var forcedSolar = nextNadi == NadiChoice.Solar || _state.ForcedSolar;
            canCoeurl = !forcedLunar;
            canRaptor = !forcedLunar;
            canOpo = true;

            if (!_state.HasBothNadi)
                foreach (var chak in _state.BeastChakra)
                {
                    canCoeurl &= chak != BeastChakraType.Coeurl;
                    canRaptor &= chak != BeastChakraType.Raptor;
                    if (forcedSolar)
                        canOpo &= chak != BeastChakraType.OpoOpo;
                }

            // big pile of conditionals to check whether this is a forced solar (buffs are running out).
            // odd windows are planned out such that buffed demo was used right before perfect balance, so this
            // block only applies to even windows
            // see ShouldUsePB for more context
            if (canCoeurl && canRaptor)
            {
                if (_state.DisciplinedFistLeft == 0)
                    return Form.Raptor;
                if (NeedDemolishRefresh(strategy, 2))
                    return Form.Coeurl;
                if (NeedDFRefresh(strategy, 2))
                    return Form.Raptor;
            }
            else if (canCoeurl)
            {
                if (_state.BeastCount == 1 && NeedDemolishRefresh(strategy, 1))
                    return Form.Coeurl;
                else if (_state.BeastCount == 2 && NeedDemolishRefresh(strategy, 5))
                    return Form.Coeurl;
            }
            else if (canRaptor)
            {
                if (_state.BeastCount == 1 && NeedDFRefresh(strategy, 1))
                    return Form.Raptor;
                else if (_state.BeastCount == 2 && NeedDFRefresh(strategy, 4))
                    return Form.Raptor;
            }

            // PB is used preemptively in two cases
            // 1. odd windows (both nadi, NOT COVERED HERE): 1-2 GCDs before RoF so that phantom rush happens in the
            //    buff window, followed by immediate natural demolish refresh
            // 2. BH2 (no nadi, HERE): 1-3 GCDs before RoF (at the latest, it will be used right before Rising Phoenix).
            //    this window consists of Solar -> natural demolish -> Lunar;
            //      if we do lunar first, it's possible for all 3 opo GCDs to miss the RoF window;
            //      if we try to delay both lunar/solar until RoF is up, like the standard opener (which is just BH3),
            //      pre-PB demolish will fall off for multiple GCDs;
            //      so early non-demo solar is the only way to prevent clipping

            // TODO: full demo is more potency than any single gcd, so we should use opo before demo if a refresh is imminent
            var isBH2 = _state.FireLeft == 0 && (forcedSolar || !_state.HasSolar) && _state.Unlocked(MNK.AID.RiddleOfFire);
            if (isBH2)
                return canRaptor ? Form.Raptor : canCoeurl ? Form.Coeurl : Form.OpoOpo;

            return canOpo ? Form.OpoOpo : canCoeurl ? Form.Coeurl : Form.Raptor;
        }

        if (_state.FormShiftLeft > _state.GCD)
        {
            switch (strategy.Option(Track.FormShiftForm).As<FormChoice>())
            {
                case FormChoice.Automatic:
                    break;
                case FormChoice.Coeurl:
                    return Form.Coeurl;
                case FormChoice.Raptor:
                    return Form.Raptor;
                default:
                    return Form.OpoOpo;
            }

            if (NeedDemolishRefresh(strategy, 2) && _state.DisciplinedFistLeft > _state.GCD)
                return Form.Coeurl;

            return Form.OpoOpo;
        }

        return _state.Form;
    }

    private bool ShouldBlitz(StrategyValues strategy) => _state.DisciplinedFistLeft > _state.GCD && strategy.Option(Track.Blitz).As<BlitzStrategy>() switch
    {
        BlitzStrategy.Delay => false,
        BlitzStrategy.DelayUntilMultiTarget => _state.NumBlitzTargets > 1 || _state.BlitzLeft < _state.AttackGCDTime,
        _ => true,
    };

    private bool ShouldDKSpam(StrategyValues strategy) => strategy.Option(Track.DragonKick).As<DragonKickStrategy>() switch
    {
        DragonKickStrategy.Filler => _state.LeadenFistLeft == 0 && _state.DisciplinedFistLeft > _state.GCD,
        _ => false,
    };

    private bool ShouldDash(StrategyValues strategy)
    {
        if (!_state.Unlocked(MNK.AID.Thunderclap) || _state.CD(MNK.AID.Thunderclap) > 60)
            return false;

        return strategy.Option(Track.Dash).As<DashStrategy>() switch
        {
            DashStrategy.Automatic => Player.InCombat && /*strategy.CombatTimer < 1 &&*/ _state.RangeToTarget > 3,
            DashStrategy.Forbid => false,
            DashStrategy.GapClose => _state.RangeToTarget > 3,
            _ => false,
        };
    }

    private bool ShouldUseRoF(StrategyValues strategy, float deadline)
    {
        var rofStrategy = strategy.Option(Track.RiddleOfFire).As<FireStrategy>();
        if (!_state.Unlocked(MNK.AID.RiddleOfFire) || rofStrategy == FireStrategy.Delay || !_state.CanWeave(MNK.AID.RiddleOfFire, 0.6f, deadline))
            return false;

        if (rofStrategy == FireStrategy.Force)
            return true;

        if (!HaveTarget() || _state.ActualFightEndIn < 20)
            return false;

        // prevent early use in standard opener
        return _state.DisciplinedFistLeft > _state.GCD;
    }

    private bool ShouldUseRoW(StrategyValues strategy, float deadline)
    {
        var rowStrategy = strategy.Option(Track.RiddleOfWind).As<OffensiveStrategy>();
        if (!_state.Unlocked(MNK.AID.RiddleOfWind) || rowStrategy == OffensiveStrategy.Delay || !_state.CanWeave(MNK.AID.RiddleOfWind, 0.6f, deadline))
            return false;

        if (rowStrategy == OffensiveStrategy.Force)
            return true;

        if (!HaveTarget() || _state.ActualFightEndIn < 15)
            return false;

        // thebalance recommends using RoW like an oGCD dot, so we use on cooldown as long as buffs have been used first
        return _state.CD(MNK.AID.RiddleOfFire) > 0 && _state.CD(MNK.AID.Brotherhood) > 0;
    }

    private bool ShouldUseBrotherhood(StrategyValues strategy, float deadline)
    {
        var bhStrategy = strategy.Option(Track.Brotherhood).As<OffensiveStrategy>();
        if (!_state.Unlocked(MNK.AID.Brotherhood) || bhStrategy == OffensiveStrategy.Delay || !_state.CanWeave(MNK.AID.Brotherhood, 0.6f, deadline))
            return false;

        if (bhStrategy == OffensiveStrategy.Force)
            return true;

        if (!HaveTarget() || _state.ActualFightEndIn < 15)
            return false;

        // opener timing mostly important as long as rof is used first, we just want to align with party buffs -
        // the default opener is bhood after first bootshine
        // later uses can be asap
        return !_state.UseAOE && _state.CD(MNK.AID.RiddleOfFire) > 0 && (_state.LeadenFistLeft == 0 /*|| strategy.CombatTimer > 30*/);
    }

    private bool ShouldUsePB(StrategyValues strategy, float deadline)
    {
        var pbStrategy = strategy.Option(Track.PerfectBalance).As<OffensiveStrategy>();
        if (_state.PerfectBalanceLeft > 0 || !_state.Unlocked(MNK.AID.PerfectBalance) || !_state.CanWeave(_state.CD(MNK.AID.PerfectBalance) - 40, 0.6f, deadline) || pbStrategy == OffensiveStrategy.Delay)
            return LogWhy(false, "PB", $"PBLeft = {_state.PerfectBalanceLeft}, cd = {_state.CD(MNK.AID.PerfectBalance)}");

        if (pbStrategy == OffensiveStrategy.Force)
            return LogWhy(true, "PB", "forced");

        if (!HaveTarget() || _state.ActualFightEndIn < _state.GCD + _state.AttackGCDTime * 3)
            return LogWhy(false, "PB", $"target={HaveTarget()}, fight end={_state.ActualFightEndIn}");

        // with enough haste/low enough GCD (< 1.6, currently exclusive to bozja), double lunar is possible without dropping buffs
        // via lunar -> opo -> snakes -> pb -> lunar
        // this is the only time PB use is not directly after an opo GCD
        if (_state.Form == Form.Coeurl && _state.FireLeft > deadline + _state.AttackGCDTime * 3)
            return LogWhy(!NeedDFRefresh(strategy, 5) && !NeedDemolishRefresh(strategy, 3), "PB", $"nonstandard (coeurl) lunar, DF={_state.DisciplinedFistLeft}, Demo={_state.TargetDemolishLeft}");

        if (_state.Form != Form.Raptor)
            return LogWhy(false, "PB", "not in raptor");

        // bh1 and bh3 even windows where RoF is used no earlier than 2 GCDs before this; also odd windows where
        // natural demolish happens during RoF
        // before level 68 (RoF unlock) we have nothing to plan our blitzes around, so just use PB whenever it's off cooldown
        // as long as buffs won't fall off
        // TODO: before level 60 (blitz unlock) PB is just a free opo GCD generator so use it right after DF + demo
        if (ShouldUseRoF(strategy, deadline) || _state.FireLeft > deadline + _state.AttackGCDTime * 3 || !_state.Unlocked(MNK.AID.RiddleOfFire))
        {
            if (!CanSolar(strategy))
                return LogWhy(!NeedDFRefresh(strategy, 5) && !NeedDemolishRefresh(strategy, 6), "PB", $"BH1 (RoF active or imminent), solar unavailable, DF={_state.DisciplinedFistLeft}, Demo={_state.TargetDemolishLeft}");

            // see haste note above; delay standard even window PB2 in favor of double lunar
            if (NeedDFRefresh(strategy, 3) && !NeedDemolishRefresh(strategy, 4))
                return LogWhy(false, "PB", $"BH1 (RoF active or imminent), DF expiring = {_state.DisciplinedFistLeft}");

            return LogWhy(true, "PB", "BH1 (RoF active or imminent)");
        }

        // odd windows where natural demolish happens before RoF, at most 3 GCDs prior - raptor GCD is forced to
        // be twin snakes if this is the case, so we don't need to check DF timer
        if (!CanSolar(strategy) && ShouldUseRoF(strategy, _state.GCD + _state.AttackGCDTime))
            return LogWhy(!NeedDemolishRefresh(strategy, 7), "PB", $"odd window, solar unavailable, RoF imminent, demo = {_state.TargetDemolishLeft}");

        // bhood 2 window: natural demolish happens in the middle of RoF. it's possible that only the blitz itself
        // gets the RoF buff, so BH2 consists of
        // 1. PB -> "weak" non-OPO gcds until RoF is active
        // 2. RoF -> RP
        // 3. opo, DF, demolish
        // 4. PB -> lunar
        if (CanSolar(strategy) && !ShouldUseRoF(strategy, deadline) && ShouldUseRoF(strategy, deadline + _state.AttackGCDTime * 3))
            return LogWhy(!NeedDemolishRefresh(strategy, 7), "PB", $"BH2 (early unbuffed solar), demo = {_state.TargetDemolishLeft}");

        // forced solar (cdplan or because we would otherwise overcap lunar)
        // (we are guaranteed to be in raptor form due to conditional above)
        if ((strategy.Option(Track.NextNadi).As<NadiChoice>() == NadiChoice.Solar || _state.HasLunar && !_state.HasSolar) && _state.CD(MNK.AID.RiddleOfFire) == 0)
            return LogWhy(true, "PB", "Solar forced");

        return LogWhy(false, "PB", "fallback");
    }

    private bool ShouldUseTrueNorth(StrategyValues strategy, float lastOgcdDeadline)
    {
        var tnStrategy = strategy.Option(Track.TrueNorth).As<OffensiveStrategy>();
        if (tnStrategy == OffensiveStrategy.Delay || _state.TrueNorthLeft > _state.AnimationLock)
            return false;
        if (tnStrategy == OffensiveStrategy.Force)
            return true;
        if (!HaveTarget())
            return false;

        var positionalIsWrong = _state.NextPositionalImminent && !_state.NextPositionalCorrect;

        // always late weave true north if possible (it's annoying for it to be used immediately)
        // but prioritize Riddle of Fire over it
        if (ShouldUseRoF(strategy, lastOgcdDeadline))
            return positionalIsWrong;
        else
            return positionalIsWrong && _state.GCD <= 0.800;
    }

    private bool ShouldUseTFC(StrategyValues strategy, float deadline)
    {
        var tfcStrategy = strategy.Option(Track.TFC).As<OffensiveStrategy>();
        if (!_state.Unlocked(MNK.AID.SteelPeak) || _state.Chakra < 5 || tfcStrategy == OffensiveStrategy.Delay || !_state.CanWeave(MNK.AID.SteelPeak, 0.6f, deadline))
            return false;

        if (tfcStrategy == OffensiveStrategy.Force)
            return true;

        // prevent early use in opener
        return _state.CD(MNK.AID.RiddleOfFire) > 0 || !_state.Unlocked(MNK.AID.RiddleOfFire);
    }

    // UseAOE is only true if enemies are in range
    private bool HaveTarget() => _state.TargetingEnemy || _state.UseAOE;

    private bool NeedDemolishRefresh(StrategyValues strategy, int gcds)
    {
        // don't care
        if (_state.UseAOE)
            return false;

        var demolishStrategy = strategy.Option(Track.Demolish).As<OffensiveStrategy>();
        if (demolishStrategy == OffensiveStrategy.Force)
            return true;

        if (demolishStrategy == OffensiveStrategy.Delay)
            return false;

        if (WillStatusExpire(gcds, _state.TargetDemolishLeft))
            // snap is 280 (if flank) potency
            // demo is 310 (if rear) potency after 3 ticks: 100 + 70 * 3
            // TODO: this should actually be calculating from the time when we expect to refresh demolish, rather than naively adding duration to the current one, but it probably works for most purposes?
            return true; // strategy.ActualFightEndIn > _state.TargetDemolishLeft + 9;

        return false;
    }

    private bool NeedDFRefresh(StrategyValues strategy, int gcds)
    {
        var dfStrategy = strategy.Option(Track.DisciplinedFist).As<OffensiveStrategy>();
        if (dfStrategy == OffensiveStrategy.Force)
            return true;

        if (dfStrategy == OffensiveStrategy.Delay)
            return false;

        return WillStatusExpire(gcds, _state.DisciplinedFistLeft);
    }

    private bool WillStatusExpire(int gcds, float statusDuration)
        => statusDuration < _state.GCD + _state.AttackGCDTime * gcds;

    private bool CanSolar(StrategyValues strategy) => strategy.Option(Track.NextNadi).As<NadiChoice>() switch
    {
        NadiChoice.Solar => true,
        NadiChoice.Lunar => false,
        _ => !_state.HasSolar
    };

    private T LogWhy<T>(T value, string tag, string message)
    {
        if (Debug)
            Service.Log($"[{tag}] {value}: {message}");
        return value;
    }
}
