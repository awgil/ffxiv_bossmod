using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Atonement, BladeCombo, FightOrFlight, Requiescat, GoringBlade, Holy, Dash, Ranged, SpiritsWithin, CircleOfScorn, BladeOfHonor }
    public enum AOEStrategy { AutoFinish, ForceSTFinish, ForceAOEFinish, AutoBreak, ForceSTBreak, ForceAOEBreak }
    public enum AtonementStrategy { Automatic, ForceAtonement, ForceSupplication, ForceSepulchre, Delay }
    public enum BladeComboStrategy { Automatic, ForceConfiteor, ForceFaith, ForceTruth, ForceValor, Delay }
    public enum GoringBladeStrategy { Automatic, Late, Force, Delay }
    public enum HolyStrategy { Automatic, Early, Late, VeryLate, OnlySpirit, OnlyCircle, ForceSpirit, ForceCircle, Delay }
    public enum DashStrategy { Automatic, GapClose, GapClose5, GapClose10, GapCloseOpener, Opener, OvercapSafe, OvercapUnsafe, Force, Delay }
    public enum BuffsStrategy { Automatic, Together, RaidBuffsOnly, Force, ForceWeave, Delay }
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
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement (if available)", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication (if available)", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre (if available)", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Do not use Atonement combo", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);

        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blades", "Confiteor + Blade Combo", 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatically use full Blades combo - if no blades, will use best Holy action")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force Confiteor (if available)", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Blade of Faith (if available)", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Blade of Truth (if available)", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Blade of Valor (if available)", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Do not use Confiteor or any Blades", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);

        res.Define(Track.FightOrFlight).As<BuffsStrategy>("FoF", "Fight or Flight", 170)
            .AddOption(BuffsStrategy.Automatic, "Automatically use Fight or Flight on cooldown")
            .AddOption(BuffsStrategy.Together, "Automatically use Fight or Flight only with Requiescat - will delay in attempt to align itself with Requiescat if misaligned", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Automatically use Fight or Flight only with raid buffs - will delay in attempt to align itself with raid buffs", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Force, "Force Fight or Flight (if available)", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.ForceWeave, "Force Fight or Flight inside the next possible weave window (if available)", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Delay, "Do not use Fight or Flight", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.FightOrFlight);

        res.Define(Track.Requiescat).As<BuffsStrategy>("Req.", "Requiescat", 165)
            .AddOption(BuffsStrategy.Automatic, "Automatically use Requiescat on cooldown")
            .AddOption(BuffsStrategy.Together, "Automatically use Requiescat only with Fight or Flight - will delay in attempt to align itself with Fight or Flight", 60, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Automatically use Requiescat only with raid buffs - will delay in attempt to align itself with raid buffs", 60, 20, ActionTargets.Self, 68)
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

        res.Define(Track.Holy).As<HolyStrategy>("Holy", "(Divine Might) Holy Spirit/Circle", 150)
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
            .AddOption(RangedStrategy.Automatic, "Automatically use best ranged attack if outside of melee range - Holy Spirit if stationary; Shield Lob if moving")
            .AddOption(RangedStrategy.OpenerRangedCast, "Automatically use Holy Spirit at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerCast, "Automatically use Holy Spirit at the start of combat, regardless of range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCast, "Automatically use Holy Spirit as ranged choice if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCastStationary, "Automatically use Holy Spirit as ranged choice if outside melee range and stationary", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerRanged, "Automatically use Shield Lob at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Opener, "Automatically use Shield Lob at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Ranged, "Automatically use Shield Lob as ranged choice if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Force, "Force Shield Lob", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Forbid, "Do not use any ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddAssociatedActions(AID.ShieldLob, AID.HolySpirit);

        res.DefineOGCD(Track.SpiritsWithin, AID.SpiritsWithin, "SW", "Spirits Within", 145, 30, 0, ActionTargets.Hostile, 30).AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        res.DefineOGCD(Track.CircleOfScorn, AID.CircleOfScorn, "CoS", "Circle of Scorn", 140, 30, 15, ActionTargets.Self, 50).AddAssociatedActions(AID.CircleOfScorn);
        res.DefineOGCD(Track.BladeOfHonor, AID.BladeOfHonor, "BoH", "Blade of Honor", 130, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.BladeOfHonor);

        return res;
    }

    private bool Opener;

    private PaladinGauge Gauge => World.Client.GetGauge<PaladinGauge>();
    private int BladeComboStep => Gauge.ConfiteorComboStep;
    private float FOFstatus => Status(SID.FightOrFlight, 30);
    private float FOFcd => Cooldown(AID.FightOrFlight);
    private float REQstatus => Status(SID.Requiescat, 30);
    private float GBstatus => Status(SID.GoringBladeReady, 30);
    private float DMstatus => Status(SID.DivineMight, 30);
    private float ATOstatus => Status(SID.AtonementReady, 30);
    private float SUPstatus => Status(SID.SupplicationReady, 30);
    private float SEPstatus => Status(SID.SepulchreReady, 30);
    private float CONFstatus => Status(SID.ConfiteorReady, 30);
    private float INTcd => Cooldown(AID.Intervene);
    private AID BestSpirits => Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
    private AID BestRequiescat => Unlocked(AID.Imperator) ? AID.Imperator : AID.Requiescat;
    private AID BestHolyCircle => Unlocked(AID.HolyCircle) ? AID.HolyCircle : AID.HolySpirit;
    private AID BestEnder => Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : Unlocked(AID.RageOfHalone) ? AID.RageOfHalone : AID.FastBlade;

    private AID NextSTCombo(bool wantFinish = true) => LastComboAction switch
    {
        AID.RiotBlade => wantFinish && Unlocked(BestEnder) ? BestEnder : AID.FastBlade,
        AID.FastBlade => Unlocked(AID.RiotBlade) ? AID.RiotBlade : AID.FastBlade,
        AID.TotalEclipse => wantFinish && Unlocked(AID.Prominence) ? AID.Prominence : AID.FastBlade,
        _ => AID.FastBlade
    };
    private AID NextAOECombo(bool wantFinish) => LastComboAction switch
    {
        AID.TotalEclipse => wantFinish && Unlocked(AID.Prominence) ? AID.Prominence : AID.TotalEclipse,
        AID.RiotBlade => wantFinish && Unlocked(BestEnder) ? BestEnder : AID.TotalEclipse,
        AID.FastBlade => Unlocked(AID.RiotBlade) ? AID.RiotBlade : AID.TotalEclipse,
        _ => AID.TotalEclipse
    };
    private (bool, OGCDPriority) ShouldBuffUp(BuffsStrategy strategy, Actor? target, bool ready, bool together)
    {
        if (!ready)
            return (false, OGCDPriority.None);

        var minimal = InCombat(target) && MP >= 4000 && (Unlocked(AID.Imperator) ? In25y(target) : In3y(target));
        return strategy switch
        {
            BuffsStrategy.Automatic => (minimal && Opener, OGCDPriority.Severe),
            BuffsStrategy.Together => (minimal && together, OGCDPriority.Severe),
            BuffsStrategy.RaidBuffsOnly => (minimal && (RaidBuffsLeft > 0 || RaidBuffsIn < 3000), OGCDPriority.Severe),
            BuffsStrategy.Force => (true, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
    }
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs => Player.InCombat && FOFcd <= 4f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        Opener = CombatTimer <= 10 ? LastComboAction == AID.RoyalAuthority : ComboTimer > 10;
        var bladesTarget = GetBestTarget(primaryTarget, 25, IsSplashTarget).Best?.Actor;
        var mainTarget = primaryTarget?.Actor;

        if (strategy.HoldEverything())
            return;

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.TotalEclipse, 3, maximumActionRange: 20);

        var aoe = strategy.Option(Track.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        var forceAOE = aoeStrat is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak;
        var wantAOE = TargetsInAOECircle(5f, 3) || forceAOE;
        var stTarget = SingleTargetChoice(mainTarget, aoe);
        var autoTarget = wantAOE ? Player : stTarget;
        var (aoeAction, aoeTarget) = aoeStrat switch
        {
            AOEStrategy.AutoFinish => (wantAOE ? NextAOECombo(true) : NextSTCombo(true), autoTarget),
            AOEStrategy.AutoBreak => (wantAOE ? NextAOECombo(false) : NextSTCombo(false), autoTarget),
            AOEStrategy.ForceSTFinish => (NextSTCombo(true), stTarget),
            AOEStrategy.ForceSTBreak => (NextSTCombo(false), stTarget),
            AOEStrategy.ForceAOEFinish => (NextAOECombo(true), Player),
            AOEStrategy.ForceAOEBreak => (NextAOECombo(false), Player),
            _ => (AID.None, null)
        };
        if (aoeTarget != null && (wantAOE ? In5y(aoeTarget) : In3y(aoeTarget)))
            QueueGCD(aoeAction, aoeTarget, GCDPriority.Low);

        var fof = strategy.Option(Track.FightOrFlight);
        var fofStrat = fof.As<BuffsStrategy>();
        if (!strategy.HoldAbilities())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    var (fofCondition, fofPrio) = ShouldBuffUp(fofStrat, mainTarget, ActionReady(AID.FightOrFlight), Cooldown(BestRequiescat) < 1f);
                    if (fofCondition)
                        QueueOGCD(AID.FightOrFlight, Player, fofPrio);

                    var req = strategy.Option(Track.Requiescat);
                    var reqStrat = req.As<BuffsStrategy>();
                    var reqTarget = Unlocked(AID.Imperator) ? AOETargetChoice(mainTarget, bladesTarget, req, strategy) : SingleTargetChoice(mainTarget, req);
                    var (reqCondition, reqPrio) = ShouldBuffUp(reqStrat, reqTarget, ActionReady(BestRequiescat), FOFcd > 55f);
                    if (reqCondition)
                        QueueOGCD(BestRequiescat, reqTarget, reqPrio);
                }

                var cos = strategy.Option(Track.CircleOfScorn);
                var cosStrat = cos.As<OGCDStrategy>();
                if (ShouldUseOGCD(cosStrat, mainTarget, ActionReady(AID.CircleOfScorn), In5y(mainTarget) && FOFcd is < 57.55f and > 12))
                    QueueOGCD(AID.CircleOfScorn, Player, OGCDPrio(cosStrat, OGCDPriority.AboveAverage));

                var sw = strategy.Option(Track.SpiritsWithin);
                var swStrat = sw.As<OGCDStrategy>();
                var swTarget = SingleTargetChoice(mainTarget, sw);
                if (ShouldUseOGCD(swStrat, swTarget, ActionReady(BestSpirits), In3y(swTarget) && FOFcd is < 57.55f and > 12))
                    QueueOGCD(BestSpirits, swTarget, OGCDPrio(swStrat, OGCDPriority.Average));

                var dash = strategy.Option(Track.Dash);
                var dashStrat = dash.As<DashStrategy>();
                var dashTarget = SingleTargetChoice(mainTarget, dash);
                var dashMinimum = dashTarget != null && INTcd < 30.6f;
                var (dashCondition, dashPrio) = dashStrat switch
                {
                    DashStrategy.Automatic => (!IsMoving && In3y(dashTarget) && HasStatus(SID.FightOrFlight), OGCDPriority.SlightlyLow),
                    DashStrategy.Force => (true, OGCDPriority.Forced),
                    DashStrategy.GapClose => (!In3y(dashTarget), OGCDPriority.SlightlyLow),
                    DashStrategy.GapClose5 => (!In5y(dashTarget), OGCDPriority.SlightlyLow),
                    DashStrategy.GapClose10 => (!In10y(dashTarget), OGCDPriority.SlightlyLow),
                    DashStrategy.GapCloseOpener => (IsFirstGCD && !In3y(dashTarget), OGCDPriority.Max),
                    DashStrategy.Opener => (IsFirstGCD, OGCDPriority.Max),
                    DashStrategy.OvercapSafe => (!IsMoving && In3y(dashTarget) && INTcd <= GCD, OGCDPriority.Max),
                    DashStrategy.OvercapUnsafe => (INTcd <= GCD, OGCDPriority.Max),
                    _ => (false, OGCDPriority.None)
                };
                if (dashMinimum && dashCondition)
                    QueueOGCD(AID.Intervene, dashTarget, dashPrio);
            }

            var boh = strategy.Option(Track.BladeOfHonor);
            var bohStrat = boh.As<OGCDStrategy>();
            var bohTarget = AOETargetChoice(mainTarget, bladesTarget, boh, strategy);
            if (ShouldUseOGCD(bohStrat, bohTarget, HasStatus(SID.BladeOfHonorReady)))
                QueueOGCD(AID.BladeOfHonor, bohTarget, OGCDPrio(bohStrat, OGCDPriority.Low));

            var gb = strategy.Option(Track.GoringBlade);
            var gbStrat = gb.As<GoringBladeStrategy>();
            var gbTarget = SingleTargetChoice(mainTarget, gb);
            var gbMinimum = gbTarget != null && In3y(gbTarget) && GBstatus > GCD;
            var (gbCondition, gbPrio) = gbStrat switch
            {
                GoringBladeStrategy.Automatic => (true, GCDPriority.High),
                GoringBladeStrategy.Late => (REQstatus <= GCD && GBstatus is < 25f and not 0f, GCDPriority.SlightlyHigh),
                GoringBladeStrategy.Force => (true, GCDPriority.Forced),
                _ => (false, GCDPriority.None)
            };
            if (gbMinimum && gbCondition)
                QueueGCD(AID.GoringBlade, gbTarget, gbPrio);

            if (ShouldUsePotion(strategy))
                Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.High - 1);
        }

        var boftv = strategy.Option(Track.BladeCombo);
        var boftvStrat = boftv.As<BladeComboStrategy>();
        var boftvTarget = AOETargetChoice(mainTarget, bladesTarget, boftv, strategy);
        var boftvBest = BladeComboStep == 3 ? AID.BladeOfValor : BladeComboStep == 2 ? AID.BladeOfTruth : BladeComboStep == 1 && Unlocked(AID.BladeOfFaith) ? AID.BladeOfFaith : CONFstatus > GCD ? AID.Confiteor : wantAOE ? BestHolyCircle : AID.HolySpirit;
        var boftvMinimum = boftvTarget != null && In25y(boftvTarget);
        var (boftvCondition, boftvAction, boftvPrio) = boftvStrat switch
        {
            BladeComboStrategy.Automatic => (REQstatus > GCD && BladeComboStep is 0 or 1 or 2 or 3, boftvBest, GCDPriority.ModeratelyHigh),
            BladeComboStrategy.ForceConfiteor => (Unlocked(AID.Confiteor) && CONFstatus > GCD && BladeComboStep is 0, AID.Confiteor, GCDPriority.Forced),
            BladeComboStrategy.ForceFaith => (BladeComboStep is 1, AID.BladeOfFaith, GCDPriority.Forced),
            BladeComboStrategy.ForceTruth => (BladeComboStep is 2, AID.BladeOfTruth, GCDPriority.Forced),
            BladeComboStrategy.ForceValor => (BladeComboStep is 3, AID.BladeOfValor, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (boftvMinimum && boftvCondition)
            QueueGCD(boftvAction, boftvTarget, boftvPrio);

        var buffActive = DMstatus > GCD || ATOstatus > GCD || SUPstatus > GCD || SEPstatus > GCD;
        var dmacSkipHold = LastComboAction is AID.RiotBlade && FOFcd > GCD;
        var dmacHold = fofStrat != BuffsStrategy.Delay && !strategy.HoldBuffs() && buffActive && !dmacSkipHold &&
            ((LastComboAction is AID.RoyalAuthority ? !CanFitSkSGCD(FOFcd, 2) : LastComboAction is AID.FastBlade ? !CanFitSkSGCD(FOFcd, 1) : LastComboAction is AID.RiotBlade && !CanFitSkSGCD(FOFcd)) ||
            dmacSkipHold);

        var atone = strategy.Option(Track.Atonement);
        var atoneStrat = atone.As<AtonementStrategy>();
        var aTarget = SingleTargetChoice(mainTarget, atone);
        var aMinimum = aTarget != null && In3y(aTarget) && !dmacHold && (ATOstatus > GCD || SUPstatus > GCD || SEPstatus > GCD);
        var bestAtonement = SEPstatus > GCD ? AID.Sepulchre : SUPstatus > GCD ? AID.Supplication : AID.Atonement;
        var (aCondition, aAction, aPriority) = atoneStrat switch
        {
            AtonementStrategy.Automatic => (aMinimum, bestAtonement, GCDPriority.AboveAverage),
            AtonementStrategy.ForceAtonement => (ATOstatus > GCD, AID.Atonement, GCDPriority.Forced),
            AtonementStrategy.ForceSupplication => (SUPstatus > GCD, AID.Supplication, GCDPriority.Forced),
            AtonementStrategy.ForceSepulchre => (SEPstatus > GCD, AID.Sepulchre, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (aMinimum && aCondition)
            QueueGCD(aAction, aTarget, aPriority);

        var dmh = strategy.Option(Track.Holy);
        var dmhStrat = dmh.As<HolyStrategy>();
        var usedmhAOE = dmhStrat is HolyStrategy.OnlyCircle or HolyStrategy.ForceCircle || wantAOE;
        var dmhTarget = usedmhAOE ? Player : SingleTargetChoice(mainTarget, dmh);
        var dmhMinimum = dmhTarget != null && DMstatus > GCD && !dmacHold;
        var hsMinimum = Unlocked(AID.HolySpirit) && In25y(dmhTarget) && dmhMinimum;
        var hcMinimum = Unlocked(AID.HolyCircle) && In5y(dmhTarget) && dmhMinimum;
        var bestHoly = usedmhAOE ? BestHolyCircle : AID.HolySpirit;
        var (dmhCondition, dmhAction, dmhPrio) = dmhStrat switch
        {
            HolyStrategy.Automatic => (hsMinimum, bestHoly, GCDPriority.AboveAverage - 1),
            HolyStrategy.Early => (hsMinimum, bestHoly, GCDPriority.AboveAverage + 1),
            HolyStrategy.Late => (hsMinimum, bestHoly, GCDPriority.AboveAverage - 1),
            HolyStrategy.VeryLate => (Unlocked(AID.HolySpirit) && In25y(dmhTarget) && dmhTarget != null && DMstatus > GCD && (LastComboAction is AID.RiotBlade || DMstatus < 3), bestHoly, GCDPriority.AboveAverage - 2),
            HolyStrategy.OnlySpirit => (hsMinimum, AID.HolySpirit, GCDPriority.AboveAverage - 1),
            HolyStrategy.OnlyCircle => (hcMinimum, AID.HolyCircle, GCDPriority.AboveAverage - 1),
            HolyStrategy.ForceSpirit => (hsMinimum, AID.HolySpirit, GCDPriority.Forced),
            HolyStrategy.ForceCircle => (hcMinimum, AID.HolyCircle, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (dmhCondition)
        {
            var lastsecbuff = (fofStrat != BuffsStrategy.Delay && FOFstatus is <= 2.5f and >= 0.01f && SUPstatus <= GCD && SEPstatus <= GCD) || DMstatus is <= 2.5f and > 0f;
            QueueGCD(dmhAction, dmhTarget, lastsecbuff ? GCDPriority.SlightlyHigh : dmhPrio);
        }

        var r = strategy.Option(Track.Ranged);
        var rStrat = r.As<RangedStrategy>();
        var rTarget = SingleTargetChoice(mainTarget, r);
        var away = !In3y(rTarget);
        var (rCondition, rAction, rPriority) = rStrat switch
        {
            RangedStrategy.Automatic => (rTarget != null && away, IsMoving || MP < 1000 ? AID.ShieldLob : AID.HolySpirit, GCDPriority.Low + 1),
            RangedStrategy.OpenerRanged => (IsFirstGCD && away, AID.ShieldLob, GCDPriority.Low + 1),
            RangedStrategy.OpenerRangedCast => (IsFirstGCD && away && !IsMoving, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.AboveAverage - 1),
            RangedStrategy.Opener => (IsFirstGCD, AID.ShieldLob, GCDPriority.Low + 1),
            RangedStrategy.OpenerCast => (IsFirstGCD && !IsMoving, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.AboveAverage - 1),
            RangedStrategy.Force => (true, AID.ShieldLob, GCDPriority.Forced),
            RangedStrategy.Ranged => (away, AID.ShieldLob, GCDPriority.Low + 1),
            RangedStrategy.RangedCast => (away && !IsMoving, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.AboveAverage - 1),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (rCondition)
            QueueGCD(rAction, rTarget, rPriority);
    }
}
