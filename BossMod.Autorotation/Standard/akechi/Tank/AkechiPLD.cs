using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Atonement, BladeCombo, FightOrFlight, Requiescat, GoringBlade, Holy, Dash, Ranged, SpiritsWithin, CircleOfScorn, BladeOfHonor }
    public enum AOEStrategy { AutoFinish, ForceSTFinish, ForceAOEFinish, AutoBreak, ForceSTBreak, ForceAOEBreak }
    public enum AtonementStrategy { Automatic, Delay }
    public enum BladeComboStrategy { Automatic, Delay }
    public enum GoringBladeStrategy { Automatic, Late, Force, Delay }
    public enum HolyStrategy { Automatic, Early, Late, VeryLate, OnlySpirit, OnlyCircle, ForceSpirit, ForceCircle, Delay }
    public enum DashStrategy { Automatic, GapClose, GapClose5, GapClose10, GapCloseOpener, Opener, OvercapSafe, OvercapUnsafe, Force, Delay }
    public enum BuffsStrategy { Automatic, Together, Force, ForceWeave, Delay }
    public enum RangedStrategy { Automatic, OpenerRangedCast, OpenerCast, RangedCast, RangedCastStationary, OpenerRanged, Opener, Ranged, Force, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi PLD", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.GLA, (int)Class.PLD), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.AutoFinish, "Automatically select best rotation based on targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.ForceSTFinish, "Force Single-Target rotation, regardless of targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.ForceAOEFinish, "Force AoE rotation, regardless of targets nearby - finishes current combo if possible")
            .AddOption(AOEStrategy.AutoBreak, "Automatically select best rotation based on targets nearby - will break current combo if in one")
            .AddOption(AOEStrategy.ForceSTBreak, "Force Single-Target rotation, regardless of targets nearby - will break current combo if in one")
            .AddOption(AOEStrategy.ForceAOEBreak, "Force AoE rotation, regardless of targets nearby - will break current combo if in one")
            .AddAssociatedActions(AID.FastBlade, AID.RiotBlade, AID.RageOfHalone, AID.RoyalAuthority, AID.Prominence, AID.TotalEclipse);

        res.Define(Track.Atonement).As<AtonementStrategy>("Atones", "Atonement Combo", 155)
            .AddOption(AtonementStrategy.Automatic, "Automatically use full Atonement combo - will hold if Fight or Flight is imminent (unless Fight or Flight is user disabled)")
            .AddOption(AtonementStrategy.Delay, "Do not use Atonement combo", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);

        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blades", "Confiteor + Blade Combo", 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatically use full Blades combo - if no blades, will use best Holy action")
            .AddOption(BladeComboStrategy.Delay, "Do not use Confiteor or any Blades", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);

        res.Define(Track.FightOrFlight).As<BuffsStrategy>("FoF", "Fight or Flight", 170)
            .AddOption(BuffsStrategy.Automatic, "Automatically use Fight or Flight on cooldown")
            .AddOption(BuffsStrategy.Together, "Automatically use Fight or Flight only with Requiescat - will delay in attempt to align itself with Requiescat if misaligned", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Force, "Force Fight or Flight (if available)", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.ForceWeave, "Force Fight or Flight inside the next possible weave window (if available)", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Delay, "Do not use Fight or Flight", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.FightOrFlight);

        res.Define(Track.Requiescat).As<BuffsStrategy>("Req.", "Requiescat", 165)
            .AddOption(BuffsStrategy.Automatic, "Automatically use Requiescat on cooldown")
            .AddOption(BuffsStrategy.Together, "Automatically use Requiescat only with Fight or Flight - will delay in attempt to align itself with Fight or Flight if misaligned", 60, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.Force, "Force Requiescat (if available)", 60, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.ForceWeave, "Force Requiescat inside the next possible weave window (if available)", 60, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.Delay, "Do not use Requiescat", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.Requiescat, AID.Imperator);

        res.Define(Track.GoringBlade).As<GoringBladeStrategy>("GB", "Goring Blade", 135)
            .AddOption(GoringBladeStrategy.Automatic, "Automatically use Goring Blade as soon as possible when in burst")
            .AddOption(GoringBladeStrategy.Late, "Automatically use Goring Blade after spending Requiescat stacks in burst (or if status is expiring)", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(GoringBladeStrategy.Force, "Force Goring Blade (if available)", 0, 0, ActionTargets.Hostile, 54)
            .AddOption(GoringBladeStrategy.Delay, "Do not use Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.GoringBlade);

        res.Define(Track.Holy).As<HolyStrategy>("Holy", "Holy Spirit/Circle", 150)
            .AddOption(HolyStrategy.Automatic, "Automatically use best Holy action based on targets")
            .AddOption(HolyStrategy.Early, "Automatically use best Holy action as soon as possible", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.Late, "Automatically use best Holy action after Atonement combo (or if nothing else left to use)", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.VeryLate, "Automatically use best Holy action at the very last possible moment (right before next Atonement)", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.OnlySpirit, "Only use Holy Spirit as best Holy action", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.OnlyCircle, "Only use Holy Circle as best Holy action", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.ForceSpirit, "Force raw or buffed Holy Spirit (if available)", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.ForceCircle, "Force raw or buffed Holy Circle (if available)", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.Delay, "Do not use any Holy actions", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.HolySpirit, AID.HolyCircle);

        res.Define(Track.Dash).As<DashStrategy>("Dash", "Intervene", 95)
            .AddOption(DashStrategy.Automatic, "Automatically use both Intervene charges when in burst - will only use if in melee range and not moving")
            .AddOption(DashStrategy.GapClose, "Automatically use Intervene as gap closer if outside melee range", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose5, "Automatically use Intervene as gap closer if further than five yalms", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose10, "Automatically use Intervene as gap closer if further than ten yalms", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapCloseOpener, "Automatically use Intervene as gap closer at the start of combat if out of melee range", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Opener, "Automatically use Intervene at the start of combat, regardless of range", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.OvercapSafe, "Automatically use Intervene if close to overcapping on charges - will only use if in melee range and not moving", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.OvercapUnsafe, "Automatically use Intervene if close to overcapping on charges, regardless of any conditions (UNSAFE)", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Force, "Force Intervene", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Delay, "Do not use Intervene", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.Intervene);

        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", "Ranged Options", 100)
            .AddOption(RangedStrategy.Automatic, "Automatically use best ranged attack if outside of melee range - Holy Spirit first, then Shield Lob as fallback")
            .AddOption(RangedStrategy.OpenerRangedCast, "Automatically use Holy Spirit at the start of combat if outside melee range - will use Shield Lob as fallback", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerCast, "Automatically use Holy Spirit at the start of combat, regardless of range - will use Shield Lob as fallback", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCast, "Automatically use Holy Spirit as ranged attack if outside melee range - will use Shield Lob as fallback", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCastStationary, "Automatically use Holy Spirit as ranged attack if outside melee range and stationary - will use Shield Lob as fallback", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerRanged, "Automatically use Shield Lob at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Opener, "Automatically use Shield Lob at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Ranged, "Automatically use Shield Lob as ranged attack if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Force, "Force Shield Lob", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Forbid, "Do not use any ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddAssociatedActions(AID.ShieldLob, AID.HolySpirit);

        res.DefineOGCD(Track.SpiritsWithin, AID.SpiritsWithin, "SW", "Spirits Within", 145, 30, 0, ActionTargets.Hostile, 30).AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        res.DefineOGCD(Track.CircleOfScorn, AID.CircleOfScorn, "CoS", "Circle of Scorn", 140, 30, 15, ActionTargets.Self, 50).AddAssociatedActions(AID.CircleOfScorn);
        res.DefineOGCD(Track.BladeOfHonor, AID.BladeOfHonor, "BoH", "Blade of Honor", 130, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.BladeOfHonor);

        return res;
    }

    private int BladeComboStep => World.Client.GetGauge<PaladinGauge>().ConfiteorComboStep;
    private (float CD, float Left) FOF => (Cooldown(AID.FightOrFlight), Status(SID.FightOrFlight, 30));
    private float ReqLeft => Status(SID.Requiescat, 30);
    private float GBLeft => Status(SID.GoringBladeReady, 30);
    private float DMLeft => Status(SID.DivineMight, 30);
    private float AtonementLeft => Status(SID.AtonementReady, 30);
    private float SupplicationLeft => Status(SID.SupplicationReady, 30);
    private float SepulchreLeft => Status(SID.SepulchreReady, 30);

    private AID BestSpirits => Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
    private AID BestRequiescat => Unlocked(AID.Imperator) ? AID.Imperator : AID.Requiescat;
    private AID BestHolyCircle => Unlocked(AID.HolyCircle) ? AID.HolyCircle : AID.HolySpirit;
    private AID BestFinisher => Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : Unlocked(AID.RageOfHalone) ? AID.RageOfHalone : AID.FastBlade;

    private bool WantAOE;
    private bool BurstReady => CombatTimer <= 10 ? LastComboAction == AID.RoyalAuthority : ComboTimer > 10;
    private BuffsStrategy FOFStrategy;

    private Actor? BladesTarget;

    protected override float GetCastTime(AID aid) => aid is AID.HolySpirit or AID.HolyCircle && (DMLeft > GCD || ReqLeft > GCD) ? 0 : base.GetCastTime(aid);

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        BladesTarget = GetBestTarget(PrimaryEnemy, 25, IsSplashTarget).Best?.Actor;
        FOFStrategy = strategy.Option(Track.FightOrFlight).As<BuffsStrategy>();
        WantAOE = TargetsInAOECircle(5f, 3) || strategy.Option(Track.AOE).As<AOEStrategy>() is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;

        if (strategy.HoldEverything())
            return;

        if (strategy.AllowBuffs())
        {
            QueueFightOrFlight();
            QueueRequiescat(strategy);
        }
        if (strategy.AllowCDs())
        {
            QueueCircleOfScorn(strategy);
            QueueSpiritsWithin(strategy);
            QueueIntervene(strategy);
        }
        if (strategy.AllowAbilities())
        {
            QueueBladeOfHonor(strategy);
            QueueGoringBlade(strategy);
            QueuePotion(strategy, ActionDefinitions.IDPotionStr, FOF.CD <= 4f, ActionQueue.Priority.High - 1);
        }

        var holdFightOrFlight = ShouldHoldFightOrFlight(strategy);
        QueueBladeCombo(strategy);
        QueueAtonement(strategy, holdFightOrFlight);
        QueueHoly(strategy, holdFightOrFlight);
        QueueRanged(strategy);
        QueueCombo(strategy);

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.TotalEclipse, 3, maximumActionRange: 20);
    }

    private (bool Condition, OGCDPriority Priority) ShouldBuff(BuffsStrategy strategy, Actor? target, bool ready, bool aligned)
    {
        var canBuff = InCombat(target) && MP >= 4000 && (Unlocked(AID.Imperator) ? In25y(target) : In3y(target));
        return !ready ? (false, OGCDPriority.None) : strategy switch
        {
            BuffsStrategy.Automatic => (canBuff && BurstReady, OGCDPriority.High),
            BuffsStrategy.Together => (canBuff && aligned, OGCDPriority.High),
            BuffsStrategy.Force => (true, OGCDPriority.Forced),
            BuffsStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
    }
    private bool ShouldHoldFightOrFlight(StrategyValues strategy)
    {
        static bool Expiring(float left) => left is > 0 and < 3;

        var relevant = FOFStrategy != BuffsStrategy.Delay && strategy.AllowBuffs() && LastComboAction is AID.FastBlade or AID.RiotBlade or AID.RoyalAuthority;
        var anyProc = DMLeft > GCD || AtonementLeft > GCD || SupplicationLeft > GCD || SepulchreLeft > GCD;
        var anyExpiring = Expiring(DMLeft) || Expiring(AtonementLeft) || Expiring(SupplicationLeft) || Expiring(SepulchreLeft);
        var fightOrFlightSoon = LastComboAction switch
        {
            AID.FastBlade => !CanFitSkSGCD(FOF.CD, 1),
            AID.RiotBlade => FOF.CD <= GCD,
            _ => !CanFitSkSGCD(FOF.CD, 2)
        };
        return relevant && anyProc && !anyExpiring && fightOrFlightSoon;
    }
    private void QueueFightOrFlight()
    {
        var (condition, priority) = ShouldBuff(FOFStrategy, PrimaryTarget, ActionReady(AID.FightOrFlight), Cooldown(BestRequiescat) < 1f);
        if (condition)
            QueueOGCD(AID.FightOrFlight, Player, priority + 1);
    }
    private void QueueRequiescat(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Requiescat);
        var req = track.As<BuffsStrategy>();
        var target = Unlocked(AID.Imperator) ? AOETargetChoice(PrimaryTarget, BladesTarget, track, strategy) : SingleTargetChoice(PrimaryTarget, track);
        var (condition, priority) = ShouldBuff(req, target, req != BuffsStrategy.Delay && ActionReady(BestRequiescat), FOF.CD > 55f);
        if (condition)
            QueueOGCD(BestRequiescat, target, priority);
    }
    private void QueueCircleOfScorn(StrategyValues strategy)
    {
        var cos = strategy.Option(Track.CircleOfScorn).As<OGCDStrategy>();
        if (ShouldUseOGCD(cos, PrimaryTarget, ActionReady(AID.CircleOfScorn), In5y(PrimaryTarget) && FOF.CD > 12.5f))
            QueueOGCD(AID.CircleOfScorn, Player, OGCDPrio(cos, OGCDPriority.Average + 1));
    }
    private void QueueSpiritsWithin(StrategyValues strategy)
    {
        var track = strategy.Option(Track.SpiritsWithin);
        var sw = track.As<OGCDStrategy>();
        var target = Unlocked(AID.Expiacion)
            ? AOETargetChoice(PrimaryTarget, GetBestTarget(PrimaryEnemy, 3, IsSplashTarget).Best?.Actor, track, strategy)
            : SingleTargetChoice(PrimaryTarget, track);
        if (ShouldUseOGCD(sw, target, ActionReady(BestSpirits), In3y(target) && FOF.CD > 12.5f))
            QueueOGCD(BestSpirits, target, OGCDPrio(sw, OGCDPriority.Average));
    }
    private void QueueIntervene(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Dash);
        var dash = track.As<DashStrategy>();
        var target = SingleTargetChoice(PrimaryTarget, track);
        var cooldown = Cooldown(AID.Intervene);
        var safe = DashSafe(target);
        var (condition, priority) = dash switch
        {
            DashStrategy.Automatic => (!IsMoving && In3y(target) && HasStatus(SID.FightOrFlight), OGCDPriority.Low),
            DashStrategy.GapClose => (!In3y(target) && safe, OGCDPriority.Low),
            DashStrategy.GapClose5 => (!In5y(target) && safe, OGCDPriority.Low),
            DashStrategy.GapClose10 => (!In10y(target) && safe, OGCDPriority.Low),
            DashStrategy.GapCloseOpener => (IsFirstGCD && !In3y(target) && safe, OGCDPriority.Max),
            DashStrategy.Opener => (IsFirstGCD && safe, OGCDPriority.Max),
            DashStrategy.OvercapSafe => (!IsMoving && In3y(target) && cooldown <= GCD, OGCDPriority.Max),
            DashStrategy.OvercapUnsafe => (cooldown <= GCD, OGCDPriority.Max),
            DashStrategy.Force => (true, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (condition && target != null && cooldown < 30.6f)
            QueueOGCD(AID.Intervene, target, priority);
    }
    private void QueueBladeOfHonor(StrategyValues strategy)
    {
        var track = strategy.Option(Track.BladeOfHonor);
        var boh = track.As<OGCDStrategy>();
        var target = AOETargetChoice(PrimaryTarget, BladesTarget, track, strategy);
        if (ShouldUseOGCD(boh, target, HasStatus(SID.BladeOfHonorReady)))
            QueueOGCD(AID.BladeOfHonor, target, OGCDPrio(boh, OGCDPriority.Low));
    }

    private void QueueGoringBlade(StrategyValues strategy)
    {
        var track = strategy.Option(Track.GoringBlade);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var (condition, priority) = track.As<GoringBladeStrategy>() switch
        {
            GoringBladeStrategy.Automatic => (true, GCDPriority.High + 1),
            GoringBladeStrategy.Late => (ReqLeft <= GCD && GBLeft < 25f, GCDPriority.High - 1),
            GoringBladeStrategy.Force => (true, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
        if (condition && GBLeft > GCD && In3y(target))
            QueueGCD(AID.GoringBlade, target, priority);
    }
    private void QueueBladeCombo(StrategyValues strategy)
    {
        var track = strategy.Option(Track.BladeCombo);
        var target = AOETargetChoice(PrimaryTarget, BladesTarget, track, strategy);
        var action = BladeComboStep switch
        {
            3 => AID.BladeOfValor,
            2 => AID.BladeOfTruth,
            1 when Unlocked(AID.BladeOfFaith) => AID.BladeOfFaith,
            _ => Status(SID.ConfiteorReady, 30) > GCD ? AID.Confiteor : WantAOE ? BestHolyCircle : AID.HolySpirit
        };
        if (track.As<BladeComboStrategy>() == BladeComboStrategy.Automatic && MP >= 1000 && ReqLeft > GCD && In25y(target))
            QueueGCD(action, target, GCDPriority.High);
    }
    private void QueueAtonement(StrategyValues strategy, bool hold)
    {
        var track = strategy.Option(Track.Atonement);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var action = SepulchreLeft > GCD ? AID.Sepulchre
            : SupplicationLeft > GCD ? AID.Supplication
            : AtonementLeft > GCD ? AID.Atonement
            : AID.None;
        if (!hold && track.As<AtonementStrategy>() == AtonementStrategy.Automatic && In3y(target))
            QueueGCD(action, target, GCDPriority.Average);
    }
    private void QueueHoly(StrategyValues strategy, bool hold)
    {
        var track = strategy.Option(Track.Holy);
        var holy = track.As<HolyStrategy>();
        var forced = holy is HolyStrategy.ForceSpirit or HolyStrategy.ForceCircle;
        var useCircle = WantAOE || holy is HolyStrategy.OnlyCircle or HolyStrategy.ForceCircle;
        var enemy = SingleTargetChoice(PrimaryTarget, track);
        var target = useCircle ? Player : enemy;

        var spiritReady = Unlocked(AID.HolySpirit) && MP >= 1000 && In25y(enemy);
        var circleReady = Unlocked(AID.HolyCircle) && MP >= 1000 && In5y(enemy);
        var divineMight = target != null && !hold && DMLeft > GCD;
        var buffedSpirit = divineMight && spiritReady;
        var buffedCircle = Unlocked(AID.HolyCircle) ? divineMight && circleReady : buffedSpirit;
        var buffedBest = useCircle ? buffedCircle : buffedSpirit;
        var best = useCircle ? BestHolyCircle : AID.HolySpirit;

        var useEarly = FOFStrategy != BuffsStrategy.Delay && FOF.Left is > 0 and <= 2.5f && SupplicationLeft <= GCD && SepulchreLeft <= GCD
            || DMLeft is > 0 and <= 2.5f;
        var early = GCDPriority.Average + 1;
        var late = GCDPriority.Average - 1;
        var auto = useEarly ? early : late;
        var beforeAtonement = LastComboAction is AID.RiotBlade or AID.TotalEclipse || DMLeft is > 0 and <= 3;

        var (condition, action, priority) = holy switch
        {
            HolyStrategy.Automatic => (buffedBest, best, auto),
            HolyStrategy.Early => (buffedBest, best, early),
            HolyStrategy.Late => (buffedBest, best, late),
            HolyStrategy.VeryLate => (buffedBest && beforeAtonement, best, early),
            HolyStrategy.OnlySpirit => (buffedSpirit, AID.HolySpirit, auto),
            HolyStrategy.OnlyCircle => (buffedCircle, AID.HolyCircle, auto),
            HolyStrategy.ForceSpirit => (spiritReady, AID.HolySpirit, GCDPriority.Low + 2),
            HolyStrategy.ForceCircle => (circleReady, AID.HolyCircle, GCDPriority.Low + 2),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (condition && MP >= 1000 && (DMLeft > GCD || forced))
            QueueGCD(action, target, priority);
    }
    private void QueueRanged(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Ranged);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var away = WantAOE ? !In5y(target) : !In3y(target);
        var auto = (!Unlocked(AID.HolySpirit) || MP < 1000 || IsMoving && DMLeft <= GCD) ? AID.ShieldLob : AID.HolySpirit;
        var either = Unlocked(AID.HolySpirit) && MP >= 1000 && In25y(target) ? AID.HolySpirit : AID.ShieldLob;
        var prepullLob = NoPrepullWithin(0.7f);
        var prepullHoly = NoPrepullWithin(1.75f);

        var (condition, action) = track.As<RangedStrategy>() switch
        {
            RangedStrategy.Automatic => ((Player.InCombat || (auto == AID.HolySpirit ? prepullHoly : prepullLob)) && target != null && away, auto),
            RangedStrategy.OpenerRangedCast => (prepullHoly && IsFirstGCD && away && !IsMoving, either),
            RangedStrategy.OpenerCast => (prepullLob && IsFirstGCD && !IsMoving, either),
            RangedStrategy.RangedCast => ((Player.InCombat || prepullHoly) && away, either),
            RangedStrategy.RangedCastStationary => ((Player.InCombat || prepullHoly) && away && !IsMoving, either),
            RangedStrategy.OpenerRanged => (prepullLob && IsFirstGCD && away, AID.ShieldLob),
            RangedStrategy.Opener => (prepullLob && IsFirstGCD, AID.ShieldLob),
            RangedStrategy.Ranged => ((Player.InCombat || prepullLob) && away, AID.ShieldLob),
            RangedStrategy.Force => (true, AID.ShieldLob),
            _ => (false, AID.None)
        };
        if (condition)
            QueueGCD(action, target, GCDPriority.Low + 1);
    }
    private void QueueCombo(StrategyValues strategy)
    {
        var track = strategy.Option(Track.AOE);
        var aoe = track.As<AOEStrategy>();
        var finish = aoe is AOEStrategy.AutoFinish or AOEStrategy.ForceSTFinish or AOEStrategy.ForceAOEFinish;
        var useAOE = aoe switch
        {
            AOEStrategy.ForceSTFinish or AOEStrategy.ForceSTBreak => false,
            AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak => true,
            _ => WantAOE
        };
        var stTarget = SingleTargetChoice(PrimaryTarget, track);
        var target = useAOE ? Player : stTarget;
        var inRange = WantAOE ? In5y(stTarget) : In3y(stTarget);
        if (target != null && inRange && CountdownRemaining is not > 0.7f)
            QueueGCD(useAOE ? NextAOECombo(finish) : NextSTCombo(finish), target, GCDPriority.Low);
    }
    private AID NextSTCombo(bool finish) => LastComboAction switch
    {
        AID.RiotBlade => finish && Unlocked(BestFinisher) ? BestFinisher : AID.FastBlade,
        AID.FastBlade => Unlocked(AID.RiotBlade) ? AID.RiotBlade : AID.FastBlade,
        AID.TotalEclipse => finish && Unlocked(AID.Prominence) ? AID.Prominence : AID.FastBlade,
        _ => AID.FastBlade
    };
    private AID NextAOECombo(bool finish) => LastComboAction switch
    {
        AID.TotalEclipse => finish && Unlocked(AID.Prominence) ? AID.Prominence : AID.TotalEclipse,
        AID.RiotBlade => finish && Unlocked(BestFinisher) ? BestFinisher : AID.TotalEclipse,
        AID.FastBlade => Unlocked(AID.RiotBlade) ? AID.RiotBlade : AID.TotalEclipse,
        _ => AID.TotalEclipse
    };
}
