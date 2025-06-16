using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.PLD;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Atonement = SharedTrack.Count, BladeCombo, FightOrFlight, Requiescat, GoringBlade, Holy, Dash, Ranged, SpiritsWithin, CircleOfScorn, BladeOfHonor }
    public enum AtonementStrategy { Automatic, ForceAtonement, ForceSupplication, ForceSepulchre, Delay }
    public enum BladeComboStrategy { Automatic, ForceConfiteor, ForceFaith, ForceTruth, ForceValor, Delay }
    public enum GoringBladeStrategy { Automatic, Early, Late, Force, Delay }
    public enum HolyStrategy { Automatic, Early, Late, OnlySpirit, OnlyCircle, ForceSpirit, ForceCircle, Delay }
    public enum DashStrategy { Automatic, Force, Force1, GapClose, GapClose1, Delay }
    public enum BuffsStrategy { Automatic, Together, RaidBuffsOnly, Force, ForceWeave, Delay }
    public enum RangedStrategy { Automatic, OpenerRangedCast, OpenerCast, ForceCast, RangedCast, OpenerRanged, Opener, Force, Ranged, Forbid }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi PLD", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.GLA, (int)Class.PLD), 100);
        res.DefineAOE().AddAssociatedActions(AID.FastBlade, AID.RiotBlade, AID.RageOfHalone, AID.RoyalAuthority, AID.Prominence, AID.TotalEclipse);
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);
        res.Define(Track.Atonement).As<AtonementStrategy>("Atonement", "Atones", uiPriority: 155)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement & its combo chain")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Supplication", 0, 30, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Sepulchre", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement & its combo chain", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Atonement, AID.Supplication, AID.Sepulchre);
        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blade Combo", "Blades", uiPriority: 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Confiteor & Blades combo chain")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blades combo chain", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Confiteor, AID.BladeOfFaith, AID.BladeOfTruth, AID.BladeOfValor);
        res.Define(Track.FightOrFlight).As<BuffsStrategy>("Fight or Flight", "F.Flight", uiPriority: 170)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Normal use Fight or Flight")
            .AddOption(BuffsStrategy.Together, "Together", "Use Fight or Flight only with Requiescat / Imperator; will delay in attempt to align itself with Requiescat / Imperator if misaligned", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Raid Buffs Only", "Use Fight or Flight only with raid buffs; will delay in attempt to align itself with raid buffs", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Force, "Force", "Force Fight or Flight usage", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.ForceWeave, "Force Weave", "Force Fight or Flight usage inside the next possible weave window", 60, 20, ActionTargets.Self, 2)
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay Fight or Flight usage", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.FightOrFlight);
        res.Define(Track.Requiescat).As<BuffsStrategy>("Requiescat", "R.scat", uiPriority: 165)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Use Requiescat / Imperator normally")
            .AddOption(BuffsStrategy.Together, "Together", "Use Requiescat / Imperator only with Fight or Flight; will delay in attempt to align itself with Fight or Flight", 60, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.RaidBuffsOnly, "Raid Buffs Only", "Use Requiescat / Imperator only with raid buffs; will delay in attempt to align itself with raid buffs", 180, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.Force, "Force", "Force Requiescat / Imperator usage", 180, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.ForceWeave, "Force Weave", "Force Requiescat / Imperator usage inside the next possible weave window", 180, 20, ActionTargets.Self, 68)
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay Requiescat / Imperator usage", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.Requiescat, AID.Imperator);
        res.Define(Track.GoringBlade).As<GoringBladeStrategy>("Goring Blade", "G.Blade", uiPriority: 135)
            .AddOption(GoringBladeStrategy.Automatic, "Automatic", "Normal use of Goring Blade")
            .AddOption(GoringBladeStrategy.Early, "Early", "Use Goring Blade before spending Requiescat stacks", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(GoringBladeStrategy.Late, "Late", "Use Goring Blade after spending Requiescat stacks", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(GoringBladeStrategy.Force, "Force", "Force use of Goring Blade", 0, 0, ActionTargets.Hostile, 54)
            .AddOption(GoringBladeStrategy.Delay, "Delay", "Delay use of Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.GoringBlade);
        res.Define(Track.Holy).As<HolyStrategy>("Holy Spirit / Circle", "Holy S/C", uiPriority: 150)
            .AddOption(HolyStrategy.Automatic, "Automatic", "Automatically choose best Holy action under Divine Might based on targets")
            .AddOption(HolyStrategy.Early, "Early", "Use best Holy action under Divine Might ASAP", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.Late, "Late", "Use best Holy action under Divine Might after Atonement combo (or if nothing else left)", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(HolyStrategy.OnlySpirit, "Spirit", "Only use Holy Spirit as optimal Holy action", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.OnlyCircle, "Circle", "Only use Holy Circle as optimal Holy action", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.ForceSpirit, "Spirit", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(HolyStrategy.ForceCircle, "Circle", "Force use of Holy Circle", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(HolyStrategy.Delay, "Delay", "Delay use of Holy actions", 0, 0, ActionTargets.None, 64)
            .AddAssociatedActions(AID.HolySpirit, AID.HolyCircle);
        res.Define(Track.Dash).As<DashStrategy>("Intervene", "Dash", uiPriority: 95)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.Force1, "Force (Hold 1)", "Force use of Intervene; Hold one charge for manual usage", 30, 0, ActionTargets.Hostile, 66)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.GapClose1, "Gap Close (Hold 1)", "Use as gap closer if outside melee range; Hold one charge for manual usage", 30, 0, ActionTargets.None, 66)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 0, 0, ActionTargets.None, 66)
            .AddAssociatedActions(AID.Intervene);
        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", "Ranged", uiPriority: 100)
            .AddOption(RangedStrategy.Automatic, "Automatic", "Uses Holy Spirit when standing still; Uses Shield Lob if moving")
            .AddOption(RangedStrategy.OpenerRangedCast, "Opener (Cast)", "Use Holy Spirit at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerCast, "Opener", "Use Holy Spirit at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.ForceCast, "Force Cast", "Force use of Holy Spirit", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.RangedCast, "Ranged Cast", "Use Holy Spirit for ranged attacks", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.OpenerRanged, "Opener (Lob)", "Use Shield Lob at the start of combat if outside melee range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Opener, "Opener", "Use Shield Lob at the start of combat regardless of range", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Force, "Force", "Force use of Shield Lob", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Ranged, "Ranged", "Use Shield Lob for ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddOption(RangedStrategy.Forbid, "Forbid", "Prohibit the use of ranged attacks", 0, 0, ActionTargets.Hostile, 15)
            .AddAssociatedActions(AID.ShieldLob, AID.HolySpirit);
        res.DefineOGCD(Track.SpiritsWithin, AID.SpiritsWithin, "SpiritsWithin", "S.Within", uiPriority: 145, 30, 0, ActionTargets.Hostile, 30).AddAssociatedActions(AID.SpiritsWithin, AID.Expiacion);
        res.DefineOGCD(Track.CircleOfScorn, AID.CircleOfScorn, "CircleOfScorn", "C.Scorn", uiPriority: 140, 30, 15, ActionTargets.Self, 50).AddAssociatedActions(AID.CircleOfScorn);
        res.DefineOGCD(Track.BladeOfHonor, AID.BladeOfHonor, "BladeOfHonor", "B.Honor", uiPriority: 130, 0, 0, ActionTargets.Hostile, 100).AddAssociatedActions(AID.BladeOfHonor);
        return res;
    }
    #endregion

    #region Module Variables
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
    private bool ShouldHoldDMandAC;
    private bool Opener;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.FastBlade or AID.RiotBlade => FullST,
        AID.TotalEclipse => FullAOE,
        AID.RageOfHalone or AID.RoyalAuthority or AID.Prominence or _ => ShouldUseAOE ? FullAOE : FullST,
    };
    private AID AutoBreak => ShouldUseAOE ? FullAOE : FullST;
    private AID FullST => ComboLastMove is AID.RiotBlade ? (Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : Unlocked(AID.RageOfHalone) ? AID.RageOfHalone : AID.FastBlade) : Unlocked(AID.RiotBlade) && ComboLastMove is AID.FastBlade ? AID.RiotBlade : AID.FastBlade;
    private AID FullAOE => Unlocked(AID.Prominence) && ComboLastMove is AID.TotalEclipse ? AID.Prominence : AID.TotalEclipse;

    #region Upgrade Paths
    public AID BestSpirits => Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
    public AID BestRequiescat => Unlocked(AID.Imperator) ? AID.Imperator : AID.Requiescat;
    public AID BestHoly => ShouldUseAOE ? BestHolyCircle : AID.HolySpirit;
    public AID BestHolyCircle => Unlocked(AID.HolyCircle) ? AID.HolyCircle : AID.HolySpirit;
    #endregion

    #endregion

    #region Cooldown Helpers

    #region Buffs
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
    private (bool, OGCDPriority) ShouldUseFightOrFlight(BuffsStrategy strategy, Actor? target) => ShouldBuffUp(strategy, target, FightOrFlight.IsReady, Requiescat.CD < 1f);
    private (bool, OGCDPriority) ShouldUseRequiescat(BuffsStrategy strategy, Actor? target) => ShouldBuffUp(strategy, target, Requiescat.IsReady, FightOrFlight.CD > 56f);
    #endregion

    #region Other
    private bool ShouldUseSpiritsWithin(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, ActionReady(BestSpirits), In3y(target) && FightOrFlight.CD is < 57.55f and > 12);
    private bool ShouldUseCircleOfScorn(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, ActionReady(AID.CircleOfScorn), In5y(target) && FightOrFlight.CD is < 57.55f and > 12);
    private bool ShouldUseBladeOfHonor(OGCDStrategy strategy, Actor? target) => ShouldUseOGCD(strategy, target, BladeOfHonor.IsReady);
    private (bool, GCDPriority) ShouldUseGoringBlade(GoringBladeStrategy strategy, Actor? target)
    {
        if (!GoringBlade.IsReady || !In3y(target))
            return (false, GCDPriority.None);

        return strategy switch
        {
            GoringBladeStrategy.Automatic => (target != null, GCDPriority.High),
            GoringBladeStrategy.Early => (target != null, GCDPriority.High),
            GoringBladeStrategy.Late => (target != null && !Requiescat.IsActive && GoringBlade.Left is < 25f and not 0f, GCDPriority.SlightlyHigh),
            GoringBladeStrategy.Force => (true, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
    }
    private bool ShouldUseBladeCombo(BladeComboStrategy strategy, Actor? target) => strategy switch
    {
        BladeComboStrategy.Automatic => target != null && In25y(target) && Requiescat.IsActive && BladeComboStep is 0 or 1 or 2 or 3,
        BladeComboStrategy.ForceConfiteor => Confiteor.IsReady && BladeComboStep is 0,
        BladeComboStrategy.ForceFaith => BladeComboStep is 1,
        BladeComboStrategy.ForceTruth => BladeComboStep is 2,
        BladeComboStrategy.ForceValor => BladeComboStep is 3,
        _ => false
    };
    private bool ShouldUseAtonement(AtonementStrategy strategy, Actor? target) => strategy switch
    {
        AtonementStrategy.Automatic => Player.InCombat && target != null && In3y(target) && !ShouldHoldDMandAC && (Atonement.IsReady || Supplication.IsReady || Sepulchre.IsReady),
        AtonementStrategy.ForceAtonement => Atonement.IsReady,
        AtonementStrategy.ForceSupplication => Supplication.IsReady,
        AtonementStrategy.ForceSepulchre => Sepulchre.IsReady,
        _ => false
    };
    private (bool, GCDPriority) ShouldUseHoly(HolyStrategy strategy, Actor? target)
    {
        if (!Unlocked(AID.HolySpirit) || Unlocked(AID.HolySpirit) && MP < 1000)
            return (false, GCDPriority.None);
        var condition = target != null && DivineMight.IsActive && !ShouldHoldDMandAC;
        return strategy switch
        {
            HolyStrategy.Automatic or HolyStrategy.Late or HolyStrategy.OnlySpirit => (condition && In3y(target), GCDPriority.AboveAverage - 1),
            HolyStrategy.Early => (condition && In3y(target), GCDPriority.AboveAverage + 1),
            HolyStrategy.OnlyCircle => (condition && In5y(target), GCDPriority.AboveAverage - 1),
            HolyStrategy.ForceSpirit => (true, GCDPriority.Forced),
            HolyStrategy.ForceCircle => (Unlocked(AID.HolyCircle), GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
    }
    private (bool, OGCDPriority) ShouldUseDash(DashStrategy strategy, Actor? target) => strategy switch
    {
        DashStrategy.Automatic => (Player.InCombat && target != null && In3y(target) && Intervene.IsReady && FightOrFlight.IsActive, OGCDPriority.SlightlyLow),
        DashStrategy.Force => (true, OGCDPriority.Forced),
        DashStrategy.Force1 => (Intervene.TotalCD < 1f, OGCDPriority.Forced),
        DashStrategy.GapClose => (!In3y(target), OGCDPriority.SlightlyLow),
        DashStrategy.GapClose1 => (Intervene.TotalCD < 1f && !In3y(target), OGCDPriority.SlightlyLow),
        _ => (false, OGCDPriority.None)
    };
    private bool ShouldUseRanged(Actor? target, RangedStrategy strategy) => Unlocked(AID.ShieldLob) && strategy switch
    {
        RangedStrategy.Automatic => target != null && !In3y(target),
        RangedStrategy.OpenerRanged or RangedStrategy.OpenerRangedCast => IsFirstGCD && !In3y(target),
        RangedStrategy.Opener or RangedStrategy.OpenerCast => IsFirstGCD,
        RangedStrategy.Force or RangedStrategy.ForceCast => true,
        RangedStrategy.Ranged or RangedStrategy.RangedCast => !In3y(target),
        _ => false
    };
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs => Player.InCombat && FightOrFlight.CD <= 4f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
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
        ShouldHoldDMandAC = (strategy.Option(Track.FightOrFlight).As<BuffsStrategy>() != BuffsStrategy.Delay && !strategy.HoldBuffs()) && MP >= 4000 && (ComboLastMove is AID.RoyalAuthority ? !CanFitSkSGCD(FightOrFlight.CD, 2) : ComboLastMove is AID.FastBlade ? !CanFitSkSGCD(FightOrFlight.CD, 1) : ComboLastMove is AID.RiotBlade && !CanFitSkSGCD(FightOrFlight.CD));
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.Confiteor) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        Opener = CombatTimer <= 10 ? ComboLastMove == AID.RoyalAuthority : ComboTimer > 10;

        #region Strategy Definitions
        var fof = strategy.Option(Track.FightOrFlight);
        var fofStrat = fof.As<BuffsStrategy>();
        var req = strategy.Option(Track.Requiescat);
        var reqStrat = req.As<BuffsStrategy>();
        var atone = strategy.Option(Track.Atonement);
        var atoneStrat = atone.As<AtonementStrategy>();
        var blade = strategy.Option(Track.BladeCombo);
        var bladeStrat = blade.As<BladeComboStrategy>();
        var cos = strategy.Option(Track.CircleOfScorn);
        var cosStrat = cos.As<OGCDStrategy>();
        var sw = strategy.Option(Track.SpiritsWithin);
        var swStrat = sw.As<OGCDStrategy>();
        var dash = strategy.Option(Track.Dash);
        var dashStrat = dash.As<DashStrategy>();
        var gb = strategy.Option(Track.GoringBlade);
        var gbStrat = gb.As<GoringBladeStrategy>();
        var boh = strategy.Option(Track.BladeOfHonor);
        var bohStrat = boh.As<OGCDStrategy>();
        var holy = strategy.Option(Track.Holy);
        var holyStrat = holy.As<HolyStrategy>();
        var ranged = strategy.Option(Track.Ranged);
        var rangedStrat = ranged.As<RangedStrategy>();
        #endregion

        #endregion

        #region Full Rotation Execution
        if (!strategy.HoldEverything())
        {
            #region Standard Rotation
            var aoe = strategy.Option(SharedTrack.AOE);
            if (strategy.AutoFinish() && (ShouldUseAOE ? In5y(primaryTarget?.Actor) : In3y(primaryTarget?.Actor)))
                QueueGCD(AutoFinish, SingleTargetChoice(primaryTarget?.Actor, aoe), GCDPriority.Low);
            if (strategy.AutoBreak() && (ShouldUseAOE ? In5y(primaryTarget?.Actor) : In3y(primaryTarget?.Actor)))
                QueueGCD(AutoBreak, SingleTargetChoice(primaryTarget?.Actor, aoe), GCDPriority.Low);
            if (strategy.ForceST() && In3y(primaryTarget?.Actor))
                QueueGCD(FullST, SingleTargetChoice(primaryTarget?.Actor, aoe), GCDPriority.Low);
            if (strategy.ForceAOE() && In5y(primaryTarget?.Actor))
                QueueGCD(FullAOE, Player, GCDPriority.Low);
            #endregion

            #region Cooldowns
            if (!strategy.HoldAbilities())
            {
                if (!strategy.HoldCDs())
                {
                    if (!strategy.HoldBuffs())
                    {
                        var (fofCondition, fofPrio) = ShouldUseFightOrFlight(fofStrat, primaryTarget?.Actor);
                        if (fofCondition)
                            QueueOGCD(AID.FightOrFlight, Player, fofPrio);

                        var (reqCondition, reqPrio) = ShouldUseRequiescat(reqStrat, primaryTarget?.Actor);
                        if (reqCondition)
                            QueueOGCD(BestRequiescat, SingleTargetChoice(primaryTarget?.Actor, req), reqPrio);
                    }
                    if (ShouldUseCircleOfScorn(cosStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.CircleOfScorn, Player, OGCDPrio(cosStrat, OGCDPriority.AboveAverage));
                    if (ShouldUseSpiritsWithin(swStrat, primaryTarget?.Actor))
                        QueueOGCD(BestSpirits, SingleTargetChoice(primaryTarget?.Actor, sw), OGCDPrio(swStrat, OGCDPriority.Average));
                    var (dashCondition, dashPrio) = ShouldUseDash(dashStrat, primaryTarget?.Actor);
                    if (dashCondition)
                        QueueOGCD(AID.Intervene, SingleTargetChoice(primaryTarget?.Actor, dash), dashPrio);
                }
                if (ShouldUseBladeOfHonor(bohStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.BladeOfHonor, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, boh, strategy), OGCDPrio(bohStrat, OGCDPriority.Low));
                var (gbCondition, gbPrio) = ShouldUseGoringBlade(gbStrat, primaryTarget?.Actor);
                if (gbCondition)
                    QueueGCD(AID.GoringBlade, SingleTargetChoice(primaryTarget?.Actor, gb), gbPrio);
                if (ShouldUsePotion(strategy))
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.High - 1);
            }
            if (ShouldUseBladeCombo(bladeStrat, primaryTarget?.Actor))
            {
                switch (bladeStrat)
                {
                    case BladeComboStrategy.Automatic:
                        QueueGCD(BladeComboStep == 3 ? AID.BladeOfValor : BladeComboStep == 2 ? AID.BladeOfTruth : BladeComboStep == 1 && Unlocked(AID.BladeOfFaith) ? AID.BladeOfFaith : Confiteor.IsReady ? AID.Confiteor : BestHoly, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, blade, strategy), GCDPriority.ModeratelyHigh);
                        break;
                    case BladeComboStrategy.ForceConfiteor:
                        QueueGCD(AID.Confiteor, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, blade, strategy), GCDPriority.Forced);
                        break;
                    case BladeComboStrategy.ForceFaith:
                        QueueGCD(AID.BladeOfFaith, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, blade, strategy), GCDPriority.Forced);
                        break;
                    case BladeComboStrategy.ForceTruth:
                        QueueGCD(AID.BladeOfTruth, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, blade, strategy), GCDPriority.Forced);
                        break;
                    case BladeComboStrategy.ForceValor:
                        QueueGCD(AID.BladeOfValor, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, blade, strategy), GCDPriority.Forced);
                        break;
                }
            }
            if (ShouldUseAtonement(atoneStrat, primaryTarget?.Actor))
            {
                switch (atoneStrat)
                {
                    case AtonementStrategy.Automatic:
                        QueueGCD(Sepulchre.IsReady ? AID.Sepulchre : Supplication.IsReady ? AID.Supplication : AID.Atonement, SingleTargetChoice(primaryTarget?.Actor, atone), GCDPriority.AboveAverage);
                        break;
                    case AtonementStrategy.ForceAtonement:
                        QueueGCD(AID.Atonement, SingleTargetChoice(primaryTarget?.Actor, atone), GCDPriority.Forced);
                        break;
                    case AtonementStrategy.ForceSupplication:
                        QueueGCD(AID.Supplication, SingleTargetChoice(primaryTarget?.Actor, atone), GCDPriority.Forced);
                        break;
                    case AtonementStrategy.ForceSepulchre:
                        QueueGCD(AID.Sepulchre, SingleTargetChoice(primaryTarget?.Actor, atone), GCDPriority.Forced);
                        break;
                }
            }
            var (holyCondition, holyPrio) = ShouldUseHoly(holyStrat, primaryTarget?.Actor);
            if (holyCondition)
            {
                var needCondition = (fofStrat != BuffsStrategy.Delay && FightOrFlight.Left is <= 2.5f and >= 0.01f && !Supplication.IsActive && !Sepulchre.IsActive) || DivineMight.Left <= 2.5f;
                var prio = needCondition ? GCDPriority.SlightlyHigh : holyPrio;
                switch (holyStrat)
                {
                    case HolyStrategy.Automatic:
                    case HolyStrategy.Early:
                    case HolyStrategy.Late:
                        QueueGCD(BestHoly, SingleTargetChoice(primaryTarget?.Actor, holy), prio);
                        break;
                    case HolyStrategy.ForceSpirit:
                    case HolyStrategy.OnlySpirit:
                        QueueGCD(AID.HolySpirit, SingleTargetChoice(primaryTarget?.Actor, holy), prio);
                        break;
                    case HolyStrategy.ForceCircle:
                    case HolyStrategy.OnlyCircle:
                        QueueGCD(In5y(primaryTarget?.Actor) ? BestHolyCircle : AID.HolySpirit, Player, prio);
                        break;
                }
            }
            if (ShouldUseRanged(primaryTarget?.Actor, rangedStrat))
            {
                switch (rangedStrat)
                {
                    case RangedStrategy.Automatic:
                        QueueGCD(IsMoving || MP < 1000 ? AID.ShieldLob : AID.HolySpirit, SingleTargetChoice(primaryTarget?.Actor, ranged), GCDPriority.Low + 1);
                        break;
                    case RangedStrategy.OpenerRangedCast:
                    case RangedStrategy.OpenerCast:
                    case RangedStrategy.RangedCast:
                    case RangedStrategy.ForceCast:
                        QueueGCD(AID.HolySpirit, SingleTargetChoice(primaryTarget?.Actor, ranged), rangedStrat == RangedStrategy.ForceCast ? GCDPriority.Forced : GCDPriority.Low + 1);
                        break;
                    case RangedStrategy.OpenerRanged:
                    case RangedStrategy.Opener:
                    case RangedStrategy.Ranged:
                    case RangedStrategy.Force:
                        QueueGCD(AID.ShieldLob, SingleTargetChoice(primaryTarget?.Actor, ranged), rangedStrat == RangedStrategy.ForceCast ? GCDPriority.Forced : GCDPriority.Low + 1);
                        break;
                }
            }
            #endregion
        }
        #endregion

        #region AI
        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.TotalEclipse, 3, maximumActionRange: 20);
        #endregion
    }
}
