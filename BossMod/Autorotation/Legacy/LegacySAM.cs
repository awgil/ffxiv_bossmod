using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacySAM : LegacyModule
{
    public enum Track { AOE, TrueNorth, Iaijutsu, Higanbana, Meikyo, Dash, Enpi, Kenki }
    public enum AOEStrategy { SingleTarget, AutoOnPrimary }
    public enum OffensiveStrategy { Automatic, Delay, Force }
    public enum HiganbanaStrategy { Automatic, Never, Eager }
    public enum MeikyoStrategy { Automatic, Never, Force, ForceBreakCombo }
    public enum DashStrategy { Automatic, Never, UseOutsideMelee }
    public enum EnpiStrategy { Automatic, Ranged, Never }
    public enum KenkiStrategy { Automatic, Force, ForceDash, Never }

    public static RotationModuleDefinition Definition()
    {
        // TODO: think about target overrides where they make sense
        var res = new RotationModuleDefinition("Legacy SAM", "Old pre-refactoring module", "xan", RotationModuleQuality.WIP, BitMask.Build((int)Class.SAM), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 100)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AutoOnPrimary, "AutoOnPrimary", "Use aoe actions on primary target if profitable");

        res.Define(Track.TrueNorth).As<OffensiveStrategy>("TrueN", uiPriority: 90)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Iaijutsu).As<OffensiveStrategy>("Cast", uiPriority: 80)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force", "Setting this to Force will make iaijutsu get used during position-lock windows");

        res.Define(Track.Higanbana).As<HiganbanaStrategy>("Higanbana", uiPriority: 70)
            .AddOption(HiganbanaStrategy.Automatic, "Automatic")
            .AddOption(HiganbanaStrategy.Never, "Never", "Never use")
            .AddOption(HiganbanaStrategy.Eager, "Eager", "Ignore downtime prediction");

        res.Define(Track.Meikyo).As<MeikyoStrategy>("Meikyo", uiPriority: 60)
            .AddOption(MeikyoStrategy.Automatic, "Automatic", "Use after Tsubame during Higanbana refresh")
            .AddOption(MeikyoStrategy.Never, "Never", "Never automatically use")
            .AddOption(MeikyoStrategy.Force, "Force", "Force use after the next weaponskill combo ender")
            .AddOption(MeikyoStrategy.ForceBreakCombo, "ForceBreakCombo", "Force use even if a combo is in progress");

        res.Define(Track.Dash).As<DashStrategy>("Dash", uiPriority: 50)
            .AddOption(DashStrategy.Automatic, "Automatic", "Use as a damage skill during raid buffs")
            .AddOption(DashStrategy.Never, "Never", "Never automatically use")
            .AddOption(DashStrategy.UseOutsideMelee, "UseOutsideMelee", "Use as a gap closer if outside melee range");

        res.Define(Track.Enpi).As<EnpiStrategy>("Enpi", uiPriority: 40)
            .AddOption(EnpiStrategy.Automatic, "Automatic", "Use when outside melee range if Enhanced Enpi is active")
            .AddOption(EnpiStrategy.Ranged, "Ranged", "Use when outside melee range, even if unbuffed")
            .AddOption(EnpiStrategy.Never, "Never", "Never automatically use");

        res.Define(Track.Kenki).As<KenkiStrategy>("Kenki", uiPriority: 30)
            .AddOption(KenkiStrategy.Automatic, "Automatic", "Spend all kenki when in raid buff window, otherwise prevent overcap")
            .AddOption(KenkiStrategy.Force, "Force", "Always spend kenki at 25")
            .AddOption(KenkiStrategy.ForceDash, "ForceDash", "Always spend kenki at 35, reserving 10 for mobility")
            .AddOption(KenkiStrategy.Never, "Never", "Forbid automatic kenki spending");

        return res;
    }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
    {
        public int MeditationStacks; // 3 max
        public int Kenki; // 100 max, changes by 5
        public KaeshiAction Kaeshi; // see SAMGauge.Kaeshi
        public float FukaLeft; // 40 max
        public float FugetsuLeft; // 40 max
        public float TrueNorthLeft; // 10 max
        public float MeikyoLeft; // 15 max
        public float TargetHiganbanaLeft; // 60 max
        public float OgiNamikiriLeft; // 30 max
        public float EnhancedEnpiLeft; // 15 max
        public bool HasIceSen;
        public bool HasMoonSen;
        public bool HasFlowerSen;

        public float LostExcellenceLeft; // 60(?) max
        public float FoPLeft; // 30 max
        public float HsacLeft; // 15 max

        public float GCDTime; // TODO: should be moved to base state
        public float LastTsubame; // can be infinite

        // for action selection during meikyo if both combo enders are usable.
        // doesn't check whether you're in melee range or not
        public Positional ClosestPositional;

        public int NumAOETargets;
        public int NumTenkaTargets;
        public int NumOgiTargets;
        public int NumGurenTargets;

        public bool UseAOERotation => NumAOETargets >= 3;

        public int SenCount => (HasIceSen ? 1 : 0) + (HasMoonSen ? 1 : 0) + (HasFlowerSen ? 1 : 0);

        public float CastTime => Unlocked(SAM.TraitID.EnhancedIaijutsu) ? 1.3f : 1.8f;

        public bool HasCombatBuffs => FukaLeft > GCD && FugetsuLeft > GCD;
        public bool InCombo => ComboTimeLeft > GCD && ComboLastMove is SAM.AID.Fuko or SAM.AID.Fuga or SAM.AID.Hakaze or SAM.AID.Jinpu or SAM.AID.Shifu;

        public float NextMeikyoCharge => CD(SAM.AID.MeikyoShisui) - 55;
        public float NextTsubameCharge => CD(SAM.AID.TsubameGaeshi) - 60;

        public int GCDsUntilNextTsubame => (int)MathF.Ceiling(NextTsubameCharge / AttackGCDTime);

        public bool Unlocked(SAM.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(SAM.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public SAM.AID ComboLastMove => (SAM.AID)ComboLastAction;

        public SAM.AID AOEStarter => Unlocked(SAM.AID.Fuko) ? SAM.AID.Fuko : SAM.AID.Fuga;

        public SAM.AID BestIai => SenCount switch
        {
            0 => SAM.AID.Iaijutsu,
            1 => SAM.AID.Higanbana,
            2 => SAM.AID.TenkaGoken,
            _ => SAM.AID.MeikyoShisui
        };

        public override string ToString()
        {
            var senReadable = new List<string>();
            if (HasIceSen)
                senReadable.Add("Ice");
            if (HasMoonSen)
                senReadable.Add("Moon");
            if (HasFlowerSen)
                senReadable.Add("Flower");
            return $"GCDWait={GCDsUntilNextTsubame}, Sen=[{string.Join(",", senReadable)}], H={TargetHiganbanaLeft}, K={Kenki}, M={MeditationStacks}, Kae={Kaeshi}, TCD={CD(SAM.AID.TsubameGaeshi)}, MCD={CD(SAM.AID.MeikyoShisui)}, Fuka={FukaLeft:f3}, Fugetsu={FugetsuLeft:f3}, TN={TrueNorthLeft:f3}, PotCD={PotionCD:f3}, GCDT={GCDTime:f3}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
        }
    }

    private readonly State _state;
    private DateTime _lastTsubame;
    private float _tsubameCooldown;

    public LegacySAM(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        _state.UpdateCommon(primaryTarget);

        var newTsubameCooldown = _state.CD(SAM.AID.TsubameGaeshi);
        if (newTsubameCooldown > _tsubameCooldown + 10) // eliminate variance, cd increment is 60s
            _lastTsubame = World.CurrentTime;
        _tsubameCooldown = newTsubameCooldown;

        var gauge = GetGauge<SamuraiGauge>();

        _state.HasIceSen = gauge.SenFlags.HasFlag(SenFlags.Setsu);
        _state.HasMoonSen = gauge.SenFlags.HasFlag(SenFlags.Getsu);
        _state.HasFlowerSen = gauge.SenFlags.HasFlag(SenFlags.Ka);
        _state.MeditationStacks = gauge.MeditationStacks;
        _state.Kenki = gauge.Kenki;
        _state.Kaeshi = gauge.Kaeshi;
        _state.FukaLeft = _state.StatusDetails(Player, SAM.SID.Fuka, Player.InstanceID).Left;
        _state.FugetsuLeft = _state.StatusDetails(Player, SAM.SID.Fugetsu, Player.InstanceID).Left;
        _state.TrueNorthLeft = _state.StatusDetails(Player, SAM.SID.TrueNorth, Player.InstanceID).Left;
        _state.MeikyoLeft = _state.StatusDetails(Player, SAM.SID.MeikyoShisui, Player.InstanceID).Left;
        _state.OgiNamikiriLeft = _state.StatusDetails(Player, SAM.SID.OgiNamikiriReady, Player.InstanceID).Left;

        _state.LostExcellenceLeft = MathF.Max(
            _state.StatusDetails(Player, SAM.SID.LostExcellence, Player.InstanceID).Left,
            _state.StatusDetails(Player, SAM.SID.Memorable, Player.InstanceID).Left
        );
        _state.FoPLeft = _state.StatusDetails(Player, SAM.SID.LostFontofPower, Player.InstanceID).Left;
        _state.HsacLeft = _state.StatusDetails(Player, SAM.SID.BannerHonoredSacrifice, Player.InstanceID).Left;

        // TODO: multidot support
        _state.TargetHiganbanaLeft = _state.ForbidDOTs ? float.MaxValue : _state.StatusDetails(primaryTarget, SAM.SID.Higanbana, Player.InstanceID).Left;

        _state.GCDTime = _state.AttackGCDTime;
        _state.LastTsubame = _lastTsubame == default ? float.MaxValue : (float)(World.CurrentTime - _lastTsubame).TotalSeconds;

        _state.ClosestPositional = GetClosestPositional(primaryTarget);

        // TODO: aoe targeting; see how BRD/DRG do aoes
        //if (_state.Kenki >= 50)
        //{
        //    (var target, var prio) = FindBetterTargetBy(initial, 10, x => NumGurenTargets(x.Actor));
        //    if (prio >= 3)
        //        return new(target, 10);
        //}
        //if (_state.OgiNamikiriLeft > 0 || !_state.Unlocked(AID.Fuko))
        //{
        //    (var target, var prio) = FindBetterTargetBy(initial, 8, x => NumConeTargets(x.Actor));
        //    if (prio >= 3)
        //        return new(target, 8);
        //}
        var aoeStrategy = strategy.Option(Track.AOE).As<AOEStrategy>();
        _state.NumAOETargets = aoeStrategy == AOEStrategy.SingleTarget ? 0 : NumAOETargets();
        _state.NumTenkaTargets = aoeStrategy == AOEStrategy.SingleTarget ? 0 : Hints.NumPriorityTargetsInAOECircle(Player.Position, 8);
        _state.NumOgiTargets = aoeStrategy == AOEStrategy.SingleTarget ? 0 : NumConeTargets(primaryTarget);
        _state.NumGurenTargets = aoeStrategy == AOEStrategy.SingleTarget ? 0 : NumGurenTargets(primaryTarget);

        _state.UpdatePositionals(primaryTarget, GetNextPositional(strategy), _state.TrueNorthLeft > _state.GCD);

        // TODO: refactor all that, it's kinda senseless now
        SAM.AID gcd = GetNextBestGCD(strategy);
        PushResult(gcd, primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline);
        PushResult(ogcd, primaryTarget);
    }

    //protected override void QueueAIActions()
    //{
    //    if (_state.Unlocked(SAM.AID.SecondWind))
    //        SimulateManualActionForAI(ActionID.MakeSpell(SAM.AID.SecondWind), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.5f);
    //    if (_state.Unlocked(SAM.AID.Bloodbath))
    //        SimulateManualActionForAI(ActionID.MakeSpell(SAM.AID.Bloodbath), Player, Player.InCombat && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8f);
    //    if (_state.Unlocked(SAM.AID.MeikyoShisui))
    //        SimulateManualActionForAI(ActionID.MakeSpell(SAM.AID.MeikyoShisui), Player, !_state.HasCombatBuffs && _strategy.CombatTimer > 0 && _strategy.CombatTimer < 5 && _state.MeikyoLeft == 0);
    //}

    public override string DescribeState() => _state.ToString();

    private int NumAOETargets() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
    private int NumGurenTargets(Actor? enemy) => enemy == null ? 0 : Hints.NumPriorityTargetsInAOERect(Player.Position, (enemy.Position - Player.Position).Normalized(), 10, 4);
    private int NumConeTargets(Actor? enemy) => enemy == null ? 0 : Hints.NumPriorityTargetsInAOECone(Player.Position, 8, (enemy.Position - Player.Position).Normalized(), 60.Degrees());

    private Positional GetClosestPositional(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return Positional.Any;

        return (Player.Position - primaryTarget.Position).Normalized().Dot(primaryTarget.Rotation.ToDirection()) switch
        {
            < -0.707167f => Positional.Rear,
            < 0.707167f => Positional.Flank,
            _ => Positional.Front
        };
    }

    // old SAMRotation
    private SAM.AID ImminentKaeshi()
    {
        if (!_state.Unlocked(SAM.AID.TsubameGaeshi))
            return SAM.AID.None;

        if (_state.Kaeshi == KaeshiAction.Namikiri)
        {
            // namikiri is not tied to tsubame cooldown
            return SAM.AID.KaeshiNamikiri;
        }
        else if (_state.NextTsubameCharge <= _state.GCD)
        {
            // will have tsubame on next gcd
            return _state.Kaeshi switch
            {
                KaeshiAction.Goken => SAM.AID.KaeshiGoken,
                KaeshiAction.Setsugekka => SAM.AID.KaeshiSetsugekka,
                // higanbana is worthless
                _ => SAM.AID.None
            };
        }

        return SAM.AID.None;
    }

    private bool CanCast(StrategyValues strategy)
    {
        var iaijutsuStrategy = strategy.Option(Track.Iaijutsu).As<OffensiveStrategy>();
        if (iaijutsuStrategy == OffensiveStrategy.Force)
            return true;
        if (iaijutsuStrategy == OffensiveStrategy.Delay)
            return false;

        return true; //_state.ForceMovementIn >= _state.GCD + _state.CastTime;
    }

    private SAM.AID GetNextBestGCD(StrategyValues strategy)
    {
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            return SAM.AID.None;

        var canCast = CanCast(strategy);

        // fallback 1: out of range for ogi
        if (CanEnpi(strategy) && _state.RangeToTarget > 8)
            return SAM.AID.Enpi;

        var k = ImminentKaeshi();
        if (k != SAM.AID.None)
            return k;

        if (_state.OgiNamikiriLeft > 0 && _state.HasCombatBuffs && canCast && !ShouldRefreshHiganbana(strategy) && _state.SenCount == 0)
            return SAM.AID.OgiNamikiri;

        // fallback 2: out of range for iaijutsu
        if (CanEnpi(strategy) && _state.RangeToTarget > 6)
            return SAM.AID.Enpi;

        if (_state.SenCount == 3 && canCast)
            return SAM.AID.MidareSetsugekka;

        if (_state.HasCombatBuffs && canCast)
        {
            if (_state.SenCount == 1 && _state.Unlocked(SAM.AID.Higanbana) && ShouldRefreshHiganbana(strategy))
                return SAM.AID.Higanbana;

            if (_state.NumTenkaTargets >= 3 && _state.SenCount == 2 && _state.Unlocked(SAM.AID.TenkaGoken))
                return SAM.AID.TenkaGoken;
        }

        // fallback 3: out of range for weaponskills
        if (CanEnpi(strategy) && _state.RangeToTarget > 3)
            return SAM.AID.Enpi;

        if (_state.UseAOERotation)
        {
            if (_state.MeikyoLeft > _state.GCD)
            {
                if (!_state.HasMoonSen)
                    return SAM.AID.Mangetsu;
                if (!_state.HasFlowerSen)
                    return SAM.AID.Oka;
            }

            if (_state.ComboLastMove == _state.AOEStarter)
            {
                if (_state.Unlocked(SAM.AID.Oka) && _state.FukaLeft <= _state.FugetsuLeft)
                    return SAM.AID.Oka;
                if (_state.Unlocked(SAM.AID.Mangetsu) && _state.FugetsuLeft <= _state.FukaLeft)
                    return SAM.AID.Mangetsu;
            }

            return _state.AOEStarter;
        }
        else
        {
            if (_state.MeikyoLeft > _state.GCD)
                return GetMeikyoPositional(strategy).Action;

            if (_state.ComboLastMove == SAM.AID.Jinpu && _state.Unlocked(SAM.AID.Gekko))
                return SAM.AID.Gekko;
            if (_state.ComboLastMove == SAM.AID.Shifu && _state.Unlocked(SAM.AID.Kasha))
                return SAM.AID.Kasha;

            if (_state.ComboLastMove == SAM.AID.Hakaze)
            {
                var aid = GetHakazeComboAction();
                if (aid != SAM.AID.None)
                    return aid;
            }

            return SAM.AID.Hakaze;
        }
    }

    // range checked at callsite
    private bool CanEnpi(StrategyValues strategy)
    {
        if (_state.UseAOERotation)
            return false;

        return strategy.Option(Track.Enpi).As<EnpiStrategy>() switch
        {
            EnpiStrategy.Automatic => _state.Unlocked(SAM.AID.Enpi) && _state.EnhancedEnpiLeft > _state.GCD,
            EnpiStrategy.Ranged => _state.Unlocked(SAM.AID.Enpi),
            EnpiStrategy.Never or _ => false,
        };
    }

    private (SAM.AID Action, bool Imminent) GetMeikyoPositional(StrategyValues strategy)
    {
        if (!_state.HasMoonSen && !_state.HasFlowerSen || _state.SenCount == 1 && ShouldRefreshHiganbana(strategy))
        {
            if (_state.TrueNorthLeft > _state.GCD)
                return (SAM.AID.Gekko, false);

            return _state.ClosestPositional switch
            {
                Positional.Flank => (SAM.AID.Kasha, false),
                Positional.Rear => (SAM.AID.Gekko, false),
                _ => (SAM.AID.Kasha, true) // flank is closer
            };
        }

        if (!_state.HasMoonSen)
            return (SAM.AID.Gekko, true);
        if (!_state.HasFlowerSen)
            return (SAM.AID.Kasha, true);
        if (!_state.HasIceSen)
            return (SAM.AID.Yukikaze, true);

        // full on sen but can't cast due to a cdplan fuckup, e.g. midare planned during a forced movement mechanic
        // gotta do something
        return (_state.ClosestPositional == Positional.Rear ? SAM.AID.Gekko : SAM.AID.Kasha, false);
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline)
    {
        if (_state.CountdownRemaining > 0.7f)
        {
            if (_state.CountdownRemaining < 9 && _state.MeikyoLeft == 0)
                return ActionID.MakeSpell(SAM.AID.MeikyoShisui);
            if (_state.CountdownRemaining < 5 && _state.TrueNorthLeft == 0 && strategy.Option(Track.TrueNorth).As<OffensiveStrategy>() != OffensiveStrategy.Delay)
                return ActionID.MakeSpell(SAM.AID.TrueNorth);

            return new();
        }

        if (_state.MeikyoLeft == 0 && _state.CanWeave(_state.NextMeikyoCharge, 0.6f, deadline))
        {
            var meikyoStrategy = strategy.Option(Track.Meikyo).As<MeikyoStrategy>();
            if (meikyoStrategy == MeikyoStrategy.Force && !_state.InCombo)
                return ActionID.MakeSpell(SAM.AID.MeikyoShisui);
            if (meikyoStrategy == MeikyoStrategy.ForceBreakCombo)
                return ActionID.MakeSpell(SAM.AID.MeikyoShisui);
        }

        if (ShouldUseTrueNorth(strategy) && _state.CanWeave(_state.CD(SAM.AID.TrueNorth) - 45, 0.6f, deadline) && _state.GCD < 0.800)
            return ActionID.MakeSpell(SAM.AID.TrueNorth);

        if (!_state.TargetingEnemy)
            return default;

        if (_state.MeikyoLeft == 0 && _state.LastTsubame < _state.GCDTime * 3)
            return ActionID.MakeSpell(SAM.AID.MeikyoShisui);

        var dashStrategy = strategy.Option(Track.Dash).As<DashStrategy>();
        if (_state.RangeToTarget > 3 && dashStrategy == DashStrategy.UseOutsideMelee)
            return ActionID.MakeSpell(SAM.AID.HissatsuGyoten);

        if (ShouldUseHagakure() && _state.CanWeave(SAM.AID.Hagakure, 0.6f, deadline))
        {
            return ActionID.MakeSpell(SAM.AID.Hagakure);
        }

        if (_state.HasCombatBuffs)
        {
            if (_state.CanWeave(SAM.AID.Ikishoten, 0.6f, deadline))
                return ActionID.MakeSpell(SAM.AID.Ikishoten);

            if (CanUseKenki(strategy) && _state.Unlocked(SAM.AID.HissatsuGuren) && _state.CanWeave(SAM.AID.HissatsuGuren, 0.6f, deadline))
            {
                if (_state.UseAOERotation || !_state.Unlocked(SAM.AID.HissatsuSenei))
                    return ActionID.MakeSpell(SAM.AID.HissatsuGuren);

                if (_state.SenCount == 0)
                    return ActionID.MakeSpell(SAM.AID.HissatsuSenei);
            }

            if (CanUseKenki(strategy, 10)
                && _state.RaidBuffsLeft > 0
                && _state.CanWeave(SAM.AID.HissatsuGyoten, 0.6f, deadline)
                && (_state.CD(SAM.AID.HissatsuGuren) > _state.GCDTime || _state.Kenki >= 35)
                && _state.RangeToTarget <= 3
                && dashStrategy != DashStrategy.Never
            )
                return ActionID.MakeSpell(SAM.AID.HissatsuGyoten);
        }

        if (CanUseKenki(strategy) && _state.CanWeave(SAM.AID.HissatsuShinten, 0.6f, deadline) && (_state.CD(SAM.AID.HissatsuGuren) > _state.GCDTime || _state.Kenki >= 50))
        {
            if (_state.Unlocked(SAM.AID.HissatsuKyuten) && _state.NumAOETargets >= 3)
                return ActionID.MakeSpell(SAM.AID.HissatsuKyuten);

            return ActionID.MakeSpell(SAM.AID.HissatsuShinten);
        }

        if (_state.MeditationStacks == 3 && _state.CanWeave(deadline))
            return ActionID.MakeSpell(SAM.AID.Shoha);

        return new();
    }

    private bool ShouldUseTrueNorth(StrategyValues strategy)
    {
        if (_state.TrueNorthLeft > _state.AnimationLock)
            return false;
        var tnStrategy = strategy.Option(Track.TrueNorth).As<OffensiveStrategy>();
        if (tnStrategy == OffensiveStrategy.Force)
            return true;
        if (!_state.TargetingEnemy || tnStrategy == OffensiveStrategy.Delay)
            return false;

        return _state.NextPositionalImminent && !_state.NextPositionalCorrect;
    }

    private bool ShouldUseBurst(float deadline)
    {
        return _state.RaidBuffsLeft > deadline
            || _state.RaidBuffsIn > _state.FightEndIn // fight will end before next window, use everything
            || _state.RaidBuffsIn > 9000; // general combat, no module active. yolo
    }

    private bool ShouldRefreshHiganbana(StrategyValues strategy, uint gcdsInAdvance = 0)
    {
        var higanbanaStrategy = strategy.Option(Track.Higanbana).As<HiganbanaStrategy>();
        if (higanbanaStrategy == HiganbanaStrategy.Never || !_state.HasCombatBuffs)
            return false;

        if (_state.NumAOETargets > 0)
            return false;

        // force use to get shoha even if the target is dying, dot overwrite doesn't matter
        if (higanbanaStrategy != HiganbanaStrategy.Eager && _state.FightEndIn > 0 && (_state.FightEndIn - _state.GCD) < 45)
            return _state.MeditationStacks == 2;

        return _state.TargetHiganbanaLeft < (5 + _state.GCD + _state.GCDTime * gcdsInAdvance);
    }

    private bool CanUseKenki(StrategyValues strategy, int minCost = 25)
    {
        return strategy.Option(Track.Kenki).As<KenkiStrategy>() switch
        {
            KenkiStrategy.Automatic => _state.Kenki >= 90 || _state.Kenki >= minCost && ShouldUseBurst(_state.AnimationLock),
            KenkiStrategy.Force => _state.Kenki >= minCost,
            KenkiStrategy.ForceDash => _state.Kenki - 10 >= minCost,
            KenkiStrategy.Never or _ => false,
        };
    }

    private bool ShouldUseHagakure() => _state.SenCount == 1 && !_state.InCombo && _state.GCDsUntilNextTsubame is 18 or 20;

    private SAM.AID GetHakazeComboAction()
    {
        // refresh buffs if they are about to expire
        if (_state.Unlocked(SAM.AID.Jinpu) && _state.FugetsuLeft < _state.GCDTime * 2)
            return SAM.AID.Jinpu;
        if (_state.Unlocked(SAM.AID.Shifu) && _state.FukaLeft < _state.GCDTime * 2)
            return SAM.AID.Shifu;

        if (_state.SenCount == 0 && _state.GCDsUntilNextTsubame is 19 or 21)
            return SAM.AID.Yukikaze;

        if (_state.Unlocked(SAM.AID.Yukikaze) && !_state.HasIceSen && (_state.TargetHiganbanaLeft == 0 || _state.Unlocked(SAM.AID.OgiNamikiri)))
            return SAM.AID.Yukikaze;

        // if not using ice, refresh the buff that runs out first
        if (_state.Unlocked(SAM.AID.Shifu) && !_state.HasFlowerSen && _state.FugetsuLeft >= _state.FukaLeft)
            return SAM.AID.Shifu;

        if (_state.Unlocked(SAM.AID.Jinpu) && !_state.HasMoonSen)
            return SAM.AID.Jinpu;

        if (_state.Unlocked(SAM.AID.Yukikaze) && !_state.HasIceSen)
            return SAM.AID.Yukikaze;

        return SAM.AID.None;
    }

    private (Positional, bool) GetNextPositional(StrategyValues strategy)
    {
        if (_state.UseAOERotation)
            return default;

        if (_state.MeikyoLeft > _state.GCD)
            return GetMeikyoPositional(strategy) switch
            {
                (SAM.AID.Gekko, var imminent) => (Positional.Rear, imminent),
                (SAM.AID.Kasha, var imminent) => (Positional.Flank, imminent),
                _ => default
            };

        if (_state.ComboLastMove == SAM.AID.Jinpu && _state.Unlocked(SAM.AID.Gekko))
            return (Positional.Rear, true);

        if (_state.ComboLastMove == SAM.AID.Shifu && _state.Unlocked(SAM.AID.Kasha))
            return (Positional.Flank, true);

        if (_state.ComboLastMove == SAM.AID.Hakaze)
        {
            var predicted = GetHakazeComboAction();
            // TODO: DRY
            if (predicted == SAM.AID.Jinpu && _state.Unlocked(SAM.AID.Gekko))
                return (Positional.Rear, false);

            if (predicted == SAM.AID.Shifu && _state.Unlocked(SAM.AID.Kasha))
                return (Positional.Flank, false);
        }

        return default;
    }
}
