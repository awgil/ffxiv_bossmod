using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Atonement = SharedTrack.Count, BladeCombo, FightOrFlight, Requiescat, GoringBlade, Holy, Dash, Ranged, SpiritsWithin, CircleOfScorn, BladeOfHonor }
    public enum AtonementStrategy { Automatic, ForceAtonement, ForceSupplication, ForceSepulchre, Delay }
    public enum BladeComboStrategy { Automatic, ForceConfiteor, ForceFaith, ForceTruth, ForceValor, Delay }
    public enum GoringBladeStrategy { Automatic, Early, Late, Force, Delay }
    public enum HolyStrategy { Automatic, Early, Late, OnlySpirit, OnlyCircle, ForceSpirit, ForceCircle, Delay }
    public enum DashStrategy { Automatic, Force, Force1, GapClose, GapClose1, Delay }
    public enum BuffsStrategy { Automatic, Together, RaidBuffsOnly, Force, ForceWeave, Delay }
    public enum RangedStrategy { Automatic, OpenerRangedCast, OpenerCast, ForceCast, RangedCast, RangedCastStationary, OpenerRanged, Opener, Force, Ranged, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi PLD", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.GLA, (int)Class.PLD), 100);
        res.DefineAOE().AddAssociatedActions(AID.FastBlade, AID.RiotBlade, AID.RageOfHalone, AID.RoyalAuthority, AID.Prominence, AID.TotalEclipse);
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.Atonement).As<AtonementStrategy>("Atones", "Atonement Combo", 155)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement & its combo chain")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Supplication", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Sepulchre", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement & its combo chain", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);

        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blades", "Confiteor + Blade Combo", 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Confiteor & Blades combo chain")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blades combo chain", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);

        res.Define(Track.FightOrFlight).As<BuffsStrategy>("FoF", "Fight or Flight", 170)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Normal use Fight or Flight")
            .AddOption(BuffsStrategy.Together, "Together", "Use Fight or Flight only with Requiescat / Imperator; will delay in attempt to align itself with Requiescat / Imperator if misaligned", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Raid Buffs Only", "Use Fight or Flight only with raid buffs; will delay in attempt to align itself with raid buffs", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Force, "Force", "Force Fight or Flight usage", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.ForceWeave, "Force Weave", "Force Fight or Flight usage inside the next possible weave window", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay Fight or Flight usage", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.FightOrFlight);

        res.Define(Track.Requiescat).As<BuffsStrategy>("Req.", "Requiescat", 165)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Use Requiescat / Imperator normally")
            .AddOption(BuffsStrategy.Together, "Together", "Use Requiescat / Imperator only with Fight or Flight; will delay in attempt to align itself with Fight or Flight", 60, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Raid Buffs Only", "Use Requiescat / Imperator only with raid buffs; will delay in attempt to align itself with raid buffs", 180, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.Force, "Force", "Force Requiescat / Imperator usage", 180, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.ForceWeave, "Force Weave", "Force Requiescat / Imperator usage inside the next possible weave window", 180, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay Requiescat / Imperator usage", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.Requiescat, AID.Imperator);

        res.Define(Track.GoringBlade).As<GoringBladeStrategy>("GB", "Goring Blade", 135)
            .AddOption(GoringBladeStrategy.Automatic, "Automatic", "Normal use of Goring Blade")
            .AddOption(GoringBladeStrategy.Early, "Early", "Use Goring Blade before spending Requiescat stacks", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(GoringBladeStrategy.Late, "Late", "Use Goring Blade after spending Requiescat stacks", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(GoringBladeStrategy.Force, "Force", "Force use of Goring Blade", 0, 0, ActionTargets.Hostile, 54)
            .AddOption(GoringBladeStrategy.Delay, "Delay", "Delay use of Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.GoringBlade);

        res.Define(Track.Holy).As<HolyStrategy>("Holy", "Holy Spirit / Circle", 150)
            .AddOption(HolyStrategy.Automatic, "Automatic", "Automatically choose best Holy action under Divine Might based on targets")
            .AddOption(HolyStrategy.Early, "Early", "Use best Holy action under Divine Might ASAP", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.Late, "Late", "Use best Holy action under Divine Might after Atonement combo (or if nothing else left)", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.OnlySpirit, "Spirit", "Only use Holy Spirit as optimal Holy action", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.OnlyCircle, "Circle", "Only use Holy Circle as optimal Holy action", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.ForceSpirit, "Spirit", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.ForceCircle, "Circle", "Force use of Holy Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.Delay, "Delay", "Delay use of Holy actions", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.HolySpirit, AID.HolyCircle);

        res.Define(Track.Dash).As<DashStrategy>("Dash", "Intervene", 95)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Force1, "Force (Hold 1)", "Force use of Intervene; Hold one charge for manual usage", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.GapClose1, "Gap Close (Hold 1)", "Use as gap closer if outside melee range; Hold one charge for manual usage", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.Intervene);

        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", "Shield Lob / Holy Spirit", 100)
            .AddOption(RangedStrategy.Automatic, "Automatic", "Uses Holy Spirit when stationary; Uses Shield Lob if moving")
            .AddOption(RangedStrategy.OpenerRangedCast, "Opener (Cast)", "Use Holy Spirit at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerCast, "Opener", "Use Holy Spirit at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.ForceCast, "Force Cast", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCast, "Ranged Cast", "Use Holy Spirit as ranged choice if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCastStationary, "Ranged Cast Stationary", "Use Holy Spirit as ranged choice if outside melee range and stationary", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerRanged, "Opener (Lob)", "Use Shield Lob at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Opener, "Opener", "Use Shield Lob at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Force, "Force", "Force use of Shield Lob", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Ranged, "Ranged", "Use Shield Lob as ranged choice if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Forbid, "Forbid", "Prohibit the use of any ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddAssociatedActions(AID.ShieldLob, AID.HolySpirit);

        res.DefineOGCD(Track.SpiritsWithin, AID.SpiritsWithin, "SW", "Spirits Within", 145, 30, 0, ActionTargets.Hostile, 30).AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        res.DefineOGCD(Track.CircleOfScorn, AID.CircleOfScorn, "CoS", "Circle of Scorn", 140, 30, 15, ActionTargets.Self, 50).AddAssociatedActions(AID.CircleOfScorn);
        res.DefineOGCD(Track.BladeOfHonor, AID.BladeOfHonor, "BoH", "Blade of Honor", 130, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.BladeOfHonor);

        return res;
    }

    private int BladeComboStep;
    private (float Left, bool IsActive) DivineMight;
    private (float CD, float Left, bool IsReady, bool IsActive) FightOrFlight;
    private (float CD, float Left, bool IsReady, bool IsActive) GoringBlade;
    private (float TotalCD, int Charges, bool IsReady) Intervene;
    private (float CD, float Left, bool IsReady, bool IsActive) Requiescat;
    private (float Left, bool IsReady, bool IsActive) Atonement;
    private (float Left, bool IsReady, bool IsActive) Supplication;
    private (float Left, bool IsReady, bool IsActive) Sepulchre;
    private (float Left, bool HasMP, bool IsReady, bool IsActive) Confiteor;
    private (float Left, bool IsReady, bool IsActive) BladeOfHonor;
    private bool ShouldUseAOE;
    private bool Opener;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;

    private AID FullST => ComboLastMove is AID.RiotBlade ? (Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : Unlocked(AID.RageOfHalone) ? AID.RageOfHalone : AID.FastBlade) : Unlocked(AID.RiotBlade) && ComboLastMove is AID.FastBlade ? AID.RiotBlade : AID.FastBlade;
    private AID FullAOE => Unlocked(AID.Prominence) && ComboLastMove is AID.TotalEclipse ? AID.Prominence : AID.TotalEclipse;
    private AID BestSpirits => Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
    private AID BestRequiescat => Unlocked(AID.Imperator) ? AID.Imperator : AID.Requiescat;
    private AID BestHolyCircle => Unlocked(AID.HolyCircle) ? AID.HolyCircle : AID.HolySpirit;

    private (bool, OGCDPriority) ShouldBuffUp(BuffsStrategy strategy, Actor? target, bool ready, bool together)
    {
        if (!ready)
            return (false, OGCDPriority.None);

        var minimal = Player.InCombat && target != null && In3y(target) && MP >= 4000;
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
        PotionStrategy.AlignWithBuffs => Player.InCombat && FightOrFlight.CD <= 4f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var gauge = World.Client.GetGauge<PaladinGauge>();
        BladeComboStep = gauge.ConfiteorComboStep;
        DivineMight.Left = StatusRemaining(Player, SID.DivineMight, 30);
        DivineMight.IsActive = DivineMight.Left > 0f;
        FightOrFlight.CD = CDRemaining(AID.FightOrFlight);
        FightOrFlight.Left = StatusRemaining(Player, SID.FightOrFlight, 20);
        FightOrFlight.IsActive = FightOrFlight.CD is >= 39.5f and <= 60;
        FightOrFlight.IsReady = ActionReady(AID.FightOrFlight);
        GoringBlade.Left = StatusRemaining(Player, SID.GoringBladeReady, 30);
        GoringBlade.IsActive = GoringBlade.Left > 0f;
        GoringBlade.IsReady = Unlocked(AID.GoringBlade) && GoringBlade.IsActive;
        Intervene.TotalCD = CDRemaining(AID.Intervene);
        Intervene.Charges = Intervene.TotalCD <= 2f ? 2 : Intervene.TotalCD is <= 31f and > 2f ? 1 : 0;
        Intervene.IsReady = Unlocked(AID.Intervene) && Intervene.Charges > 0;
        Requiescat.CD = CDRemaining(BestRequiescat);
        Requiescat.Left = StatusRemaining(Player, SID.Requiescat, 30);
        Requiescat.IsActive = Requiescat.Left > 0;
        Requiescat.IsReady = Unlocked(AID.Requiescat) && Requiescat.CD < 0.6f;
        Atonement.Left = StatusRemaining(Player, SID.AtonementReady, 30);
        Atonement.IsActive = Atonement.Left > 0;
        Atonement.IsReady = Unlocked(AID.Atonement) && Atonement.IsActive;
        Supplication.Left = StatusRemaining(Player, SID.SupplicationReady, 30);
        Supplication.IsActive = Supplication.Left > 0;
        Supplication.IsReady = Unlocked(AID.Supplication) && Supplication.IsActive;
        Sepulchre.Left = StatusRemaining(Player, SID.SepulchreReady, 30);
        Sepulchre.IsActive = Sepulchre.Left > 0;
        Sepulchre.IsReady = Unlocked(AID.Sepulchre) && Sepulchre.IsActive;
        Confiteor.Left = StatusRemaining(Player, SID.ConfiteorReady, 30);
        Confiteor.IsActive = Confiteor.Left > 0;
        Confiteor.IsReady = Unlocked(AID.Confiteor) && Confiteor.IsActive && MP >= 1000;
        BladeOfHonor.Left = StatusRemaining(Player, SID.BladeOfHonorReady, 30);
        BladeOfHonor.IsActive = BladeOfHonor.Left > 0;
        BladeOfHonor.IsReady = Unlocked(AID.BladeOfHonor) && BladeOfHonor.IsActive;
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore || strategy.ForceAOE();
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.Confiteor) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        Opener = CombatTimer <= 10 ? ComboLastMove == AID.RoyalAuthority : ComboTimer > 10;

        if (strategy.HoldEverything())
            return;

        var aoe = strategy.Option(SharedTrack.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        var aoeTarget = SingleTargetChoice(primaryTarget?.Actor, aoe);
        var aoeCondition = (ShouldUseAOE ? In5y(aoeTarget) : In3y(aoeTarget)) && aoeTarget != null;
        if (strategy.AutoFinish() && aoeCondition)
            QueueGCD(ComboLastMove switch
            {
                AID.FastBlade or AID.RiotBlade => FullST,
                AID.TotalEclipse => FullAOE,
                AID.RageOfHalone or AID.RoyalAuthority or AID.Prominence or _ => ShouldUseAOE ? FullAOE : FullST,
            },
            aoeTarget, GCDPriority.Low);
        if (strategy.AutoBreak() && aoeCondition)
            QueueGCD(ShouldUseAOE ? FullAOE : FullST, aoeTarget, GCDPriority.Low);
        if (strategy.ForceST() && In3y(aoeTarget))
            QueueGCD(FullST, aoeTarget, GCDPriority.Low);
        if (strategy.ForceAOE() && In5y(aoeTarget))
            QueueGCD(FullAOE, Player, GCDPriority.Low);

        var fof = strategy.Option(Track.FightOrFlight);
        var fofStrat = fof.As<BuffsStrategy>();
        if (!strategy.HoldAbilities())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    var (fofCondition, fofPrio) = ShouldBuffUp(fofStrat, primaryTarget?.Actor, FightOrFlight.IsReady, Requiescat.CD < 1f);
                    if (fofCondition)
                        QueueOGCD(AID.FightOrFlight, Player, fofPrio);

                    var req = strategy.Option(Track.Requiescat);
                    var reqStrat = req.As<BuffsStrategy>();
                    var (reqCondition, reqPrio) = ShouldBuffUp(reqStrat, primaryTarget?.Actor, Requiescat.IsReady, FightOrFlight.CD > 55f);
                    if (reqCondition)
                        QueueOGCD(BestRequiescat, SingleTargetChoice(primaryTarget?.Actor, req), reqPrio);
                }

                var cos = strategy.Option(Track.CircleOfScorn);
                var cosStrat = cos.As<OGCDStrategy>();
                if (ShouldUseOGCD(cosStrat, primaryTarget?.Actor, ActionReady(AID.CircleOfScorn), In5y(primaryTarget?.Actor) && FightOrFlight.CD is < 57.55f and > 12))
                    QueueOGCD(AID.CircleOfScorn, Player, OGCDPrio(cosStrat, OGCDPriority.AboveAverage));

                var sw = strategy.Option(Track.SpiritsWithin);
                var swStrat = sw.As<OGCDStrategy>();
                var swTarget = SingleTargetChoice(primaryTarget?.Actor, sw);
                if (ShouldUseOGCD(swStrat, swTarget, ActionReady(BestSpirits), In3y(swTarget) && FightOrFlight.CD is < 57.55f and > 12))
                    QueueOGCD(BestSpirits, swTarget, OGCDPrio(swStrat, OGCDPriority.Average));

                var dash = strategy.Option(Track.Dash);
                var dashStrat = dash.As<DashStrategy>();
                var dashTarget = SingleTargetChoice(primaryTarget?.Actor, dash);
                var (dashCondition, dashPrio) = dashStrat switch
                {
                    DashStrategy.Automatic => (Player.InCombat && dashTarget != null && !IsMoving && In3y(dashTarget) && Intervene.IsReady && FightOrFlight.IsActive, OGCDPriority.SlightlyLow),
                    DashStrategy.Force => (true, OGCDPriority.Forced),
                    DashStrategy.Force1 => (Intervene.TotalCD < 1f, OGCDPriority.Forced),
                    DashStrategy.GapClose => (!In3y(dashTarget), OGCDPriority.SlightlyLow),
                    DashStrategy.GapClose1 => (Intervene.TotalCD < 1f && !In3y(dashTarget), OGCDPriority.SlightlyLow),
                    _ => (false, OGCDPriority.None)
                };
                if (dashCondition)
                    QueueOGCD(AID.Intervene, dashTarget, dashPrio);
            }

            var boh = strategy.Option(Track.BladeOfHonor);
            var bohStrat = boh.As<OGCDStrategy>();
            var bohTarget = AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, boh, strategy);
            if (ShouldUseOGCD(bohStrat, bohTarget, BladeOfHonor.IsReady))
                QueueOGCD(AID.BladeOfHonor, bohTarget, OGCDPrio(bohStrat, OGCDPriority.Low));

            var gb = strategy.Option(Track.GoringBlade);
            var gbStrat = gb.As<GoringBladeStrategy>();
            var gbTarget = SingleTargetChoice(primaryTarget?.Actor, gb);
            var gbMinimum = gbTarget != null && In3y(gbTarget) && GoringBlade.IsReady;
            var (gbCondition, gbPrio) = gbStrat switch
            {
                GoringBladeStrategy.Automatic => (true, GCDPriority.High),
                GoringBladeStrategy.Early => (true, GCDPriority.High),
                GoringBladeStrategy.Late => (!Requiescat.IsActive && GoringBlade.Left is < 25f and not 0f, GCDPriority.SlightlyHigh),
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
        var boftvTarget = AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, boftv, strategy);
        var boftvBest = BladeComboStep == 3 ? AID.BladeOfValor : BladeComboStep == 2 ? AID.BladeOfTruth : BladeComboStep == 1 && Unlocked(AID.BladeOfFaith) ? AID.BladeOfFaith : Confiteor.IsReady ? AID.Confiteor : ShouldUseAOE ? BestHolyCircle : AID.HolySpirit;
        var boftvMinimum = boftvTarget != null && In25y(boftvTarget);
        var (boftvCondition, boftvAction, boftvPrio) = boftvStrat switch
        {
            BladeComboStrategy.Automatic => (Requiescat.IsActive && BladeComboStep is 0 or 1 or 2 or 3, boftvBest, GCDPriority.ModeratelyHigh),
            BladeComboStrategy.ForceConfiteor => (Confiteor.IsReady && BladeComboStep is 0, AID.Confiteor, GCDPriority.Forced),
            BladeComboStrategy.ForceFaith => (BladeComboStep is 1, AID.BladeOfFaith, GCDPriority.Forced),
            BladeComboStrategy.ForceTruth => (BladeComboStep is 2, AID.BladeOfTruth, GCDPriority.Forced),
            BladeComboStrategy.ForceValor => (BladeComboStep is 3, AID.BladeOfValor, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (boftvMinimum && boftvCondition)
            QueueGCD(boftvAction, boftvTarget, boftvPrio);

        var buffActive = DivineMight.IsActive || Atonement.IsActive || Supplication.IsActive || Sepulchre.IsActive;
        var dmacSkipHold = ComboLastMove is AID.RiotBlade && FightOrFlight.CD < 0.6f;
        var dmacHold = fofStrat != BuffsStrategy.Delay && !strategy.HoldBuffs() && buffActive && !dmacSkipHold &&
            ((ComboLastMove is AID.RoyalAuthority ? !CanFitSkSGCD(FightOrFlight.CD, 2) : ComboLastMove is AID.FastBlade ? !CanFitSkSGCD(FightOrFlight.CD, 1) : ComboLastMove is AID.RiotBlade && !CanFitSkSGCD(FightOrFlight.CD)) ||
            dmacSkipHold);

        var atone = strategy.Option(Track.Atonement);
        var atoneStrat = atone.As<AtonementStrategy>();
        var aTarget = SingleTargetChoice(primaryTarget?.Actor, atone);
        var aMinimum = aTarget != null && In3y(aTarget) && !dmacHold && (Atonement.IsReady || Supplication.IsReady || Sepulchre.IsReady);
        var bestAtonement = Sepulchre.IsReady ? AID.Sepulchre : Supplication.IsReady ? AID.Supplication : AID.Atonement;
        var (aCondition, aAction, aPriority) = atoneStrat switch
        {
            AtonementStrategy.Automatic => (aMinimum, bestAtonement, GCDPriority.AboveAverage),
            AtonementStrategy.ForceAtonement => (Atonement.IsReady, AID.Atonement, GCDPriority.Forced),
            AtonementStrategy.ForceSupplication => (Supplication.IsReady, AID.Supplication, GCDPriority.Forced),
            AtonementStrategy.ForceSepulchre => (Sepulchre.IsReady, AID.Sepulchre, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (aMinimum && aCondition)
            QueueGCD(aAction, aTarget, aPriority);

        var dmh = strategy.Option(Track.Holy);
        var dmhStrat = dmh.As<HolyStrategy>();
        var usedmhAOE = dmhStrat is HolyStrategy.OnlyCircle or HolyStrategy.ForceCircle || ShouldUseAOE;
        var dmhTarget = usedmhAOE ? Player : SingleTargetChoice(primaryTarget?.Actor, dmh);
        var dmhMinimum = dmhTarget != null && DivineMight.IsActive && !dmacHold;
        var hsMinimum = Unlocked(AID.HolySpirit) && In25y(dmhTarget) && dmhMinimum;
        var hcMinimum = Unlocked(AID.HolyCircle) && In5y(dmhTarget) && dmhMinimum;
        var bestHoly = usedmhAOE ? BestHolyCircle : AID.HolySpirit;
        var (dmhCondition, dmhAction, dmhPrio) = dmhStrat switch
        {
            HolyStrategy.Automatic => (hsMinimum, bestHoly, GCDPriority.AboveAverage - 1),
            HolyStrategy.Early => (hsMinimum, bestHoly, GCDPriority.AboveAverage + 1),
            HolyStrategy.Late => (hsMinimum, bestHoly, GCDPriority.AboveAverage - 1),
            HolyStrategy.OnlySpirit => (hsMinimum, AID.HolySpirit, GCDPriority.AboveAverage - 1),
            HolyStrategy.OnlyCircle => (hcMinimum, AID.HolyCircle, GCDPriority.AboveAverage - 1),
            HolyStrategy.ForceSpirit => (hsMinimum, AID.HolySpirit, GCDPriority.Forced),
            HolyStrategy.ForceCircle => (hcMinimum, AID.HolyCircle, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (dmhCondition)
        {
            var lastsecbuff = (fofStrat != BuffsStrategy.Delay && FightOrFlight.Left is <= 2.5f and >= 0.01f && !Supplication.IsActive && !Sepulchre.IsActive) || DivineMight.Left is <= 2.5f and > 0f;
            QueueGCD(dmhAction, dmhTarget, lastsecbuff ? GCDPriority.SlightlyHigh : dmhPrio);
        }

        var r = strategy.Option(Track.Ranged);
        var rStrat = r.As<RangedStrategy>();
        var rTarget = SingleTargetChoice(primaryTarget?.Actor, r);
        var (rCondition, rAction, rPriority) = rStrat switch
        {
            RangedStrategy.Automatic => (rTarget != null && !In3y(rTarget), IsMoving || MP < 1000 ? AID.ShieldLob : AID.HolySpirit, GCDPriority.Low + 1),
            RangedStrategy.OpenerRanged => (IsFirstGCD && !In3y(rTarget), AID.ShieldLob, GCDPriority.Low + 1),
            RangedStrategy.OpenerRangedCast => (IsFirstGCD && !In3y(rTarget) && !IsMoving, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.AboveAverage - 1),
            RangedStrategy.Opener => (IsFirstGCD, AID.ShieldLob, (GCDPriority)0),
            RangedStrategy.OpenerCast => (IsFirstGCD && !IsMoving, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.AboveAverage - 1),
            RangedStrategy.ForceCast => (true, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.Forced),
            RangedStrategy.Force => (true, AID.ShieldLob, GCDPriority.Forced),
            RangedStrategy.Ranged => (!In3y(rTarget), AID.ShieldLob, GCDPriority.Low + 1),
            RangedStrategy.RangedCast => (!In3y(rTarget) && !IsMoving, Unlocked(AID.HolySpirit) ? AID.HolySpirit : AID.ShieldLob, GCDPriority.AboveAverage - 1),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (rCondition)
            QueueGCD(rAction, rTarget, rPriority);

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.TotalEclipse, 3, maximumActionRange: 20);
    }
}
