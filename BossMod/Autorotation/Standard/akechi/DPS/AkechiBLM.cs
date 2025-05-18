using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Thunder = SharedTrack.Count, Polyglot, Manafont, Triplecast, Swiftcast, LeyLines, TPUS, Movement, Casting, Amplifier, Retrace, BTL }
    public enum ThunderStrategy { Allow3, Allow6, Allow9, AllowNoDOT, ForceAny, ForceST, ForceAOE, Forbid }
    public enum PolyglotStrategy { AutoSpendAll, AutoHold1, AutoHold2, AutoHold3, XenoSpendAll, XenoHold1, XenoHold2, XenoHold3, FoulSpendAll, FoulHold1, FoulHold2, FoulHold3, ForceXeno, ForceFoul, Delay }
    public enum UpgradeStrategy { Automatic, Force, ForceWeave, ForceEX, ForceWeaveEX, Delay }
    public enum ChargeStrategy { Automatic, Force, Force1, ForceWeave, ForceWeave1, Delay }
    public enum TPUSStrategy { Allow, OOConly, Forbid }
    public enum MovementStrategy { Allow, AllowNoScathe, OnlyGCDs, OnlyOGCDs, OnlyScathe, Forbid }
    public enum CastingOption { Allow, Forbid }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi BLM", "Standard Rotation Module", "Standard Rotations|Akechi|DPS", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.THM, Class.BLM), 100);
        res.DefineAOE().AddAssociatedActions(
            AID.Fire1, AID.Fire2, AID.Fire3, AID.Fire4, AID.HighFire2, //Fire
            AID.Blizzard1, AID.Blizzard2, AID.Blizzard3, AID.Blizzard4, AID.HighBlizzard2, //Blizzard
            AID.Flare, AID.Freeze, AID.Despair, AID.FlareStar); //Other
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionInt);
        res.Define(Track.Thunder).As<ThunderStrategy>("Thunder", "DOT", uiPriority: 198)
            .AddOption(ThunderStrategy.Allow3, "Allow3", "Allow the use Thunder if target Has 3s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow6, "Allow6", "Allow the use Thunder if target Has 6s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow9, "Allow9", "Allow the use Thunder if target Has 9s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.AllowNoDOT, "AllowNoDOT", "Allow the use Thunder only if target does not have DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAny, "ForceAny", "Force use of best Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceST, "ForceST", "Force use of single-target Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAOE, "ForceAOE", "Force use of AOE Thunder regardless of DoT effect", 0, 24, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Forbid, "Forbid", "Forbid the use of Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(AID.Thunder1, AID.Thunder2, AID.Thunder3, AID.Thunder4, AID.HighThunder, AID.HighThunder2);
        res.Define(Track.Polyglot).As<PolyglotStrategy>("Polyglot", "Polyglot", uiPriority: 197)
            .AddOption(PolyglotStrategy.AutoSpendAll, "AutoSpendAll", "Automatically select best polyglot based on targets; Spend all Polyglots as soon as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold1, "AutoHold1", "Automatically select best polyglot based on targets; holds 1 for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold2, "AutoHold2", "Automatically select best polyglot based on targets; holds 2 for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold3, "AutoHold3", "Automatically select best polyglot based on targets; holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.XenoSpendAll, "XenoSpendAll", "Use Xenoglossy as Polyglot spender, regardless of targets nearby; spends all Polyglots", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold1, "XenoHold1", "Use Xenoglossy as Polyglot spender, regardless of targets nearby; holds 1 Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold2, "XenoHold2", "Use Xenoglossy as Polyglot spender, regardless of targets nearby; holds 2 Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold3, "XenoHold3", "Use Xenoglossy as Polyglot spender; Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.FoulSpendAll, "FoulSpendAll", "Use Foul as Polyglot spender, regardless of targets nearby", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold1, "FoulHold1", "Use Foul as Polyglot spender, regardless of targets nearby; holds 1 Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold2, "FoulHold2", "Use Foul as Polyglot spender, regardless of targets nearby; holds 2 Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold3, "FoulHold3", "Use Foul as Polyglot spender; Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceXeno, "Force Xenoglossy", "Force use of Xenoglossy", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.ForceFoul, "Force Foul", "Force use of Foul", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.Delay, "Delay", "Delay the use of Polyglot abilities for manual or strategic usage", 0, 0, ActionTargets.Hostile, 70)
            .AddAssociatedActions(AID.Xenoglossy, AID.Foul);
        res.Define(Track.Manafont).As<UpgradeStrategy>("Manafont", "M.font", uiPriority: 196)
            .AddOption(UpgradeStrategy.Automatic, "Auto", "Automatically use Manafont optimally", 0, 0, ActionTargets.Self, 30)
            .AddOption(UpgradeStrategy.Force, "Force", "Force the use of Manafont (180s cooldown), regardless of weaving conditions", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(UpgradeStrategy.ForceWeave, "ForceWeave", "Force the use of Manafont (180s cooldown) in any next possible weave slot", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(UpgradeStrategy.ForceEX, "ForceEX", "Force the use of Manafont (100s cooldown), regardless of weaving conditions", 100, 0, ActionTargets.Self, 84)
            .AddOption(UpgradeStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Manafont (100s cooldown) in any next possible weave slot", 100, 0, ActionTargets.Self, 84)
            .AddOption(UpgradeStrategy.Delay, "Delay", "Delay the use of Manafont for strategic reasons", 0, 0, ActionTargets.Self, 30)
            .AddAssociatedActions(AID.Manafont);
        res.Define(Track.Triplecast).As<ChargeStrategy>("T.cast", uiPriority: 195)
            .AddOption(ChargeStrategy.Automatic, "Auto", "Use any charges available to maintain swift rotation by instant-casting Blizzard III after Fire Phase (e.g. Despair->Transpose->Triplecast->B3 etc.)", 0, 0, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.Force, "Force", "Force the use of Triplecast; uses all charges", 60, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.Force1, "Force1", "Force the use of Triplecast; holds one charge for manual usage", 60, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.ForceWeave, "ForceWeave", "Force the use of Triplecast in any next possible weave slot", 60, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.ForceWeave1, "ForceWeave1", "Force the use of Triplecast in any next possible weave slot; holds one charge for manual usage", 60, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.Delay, "Delay", "Delay the use of Triplecast", 0, 0, ActionTargets.Self, 66)
            .AddAssociatedActions(AID.Triplecast);
        res.Define(Track.Swiftcast).As<UpgradeStrategy>("Swiftcast", "S.cast", uiPriority: 194)
            .AddOption(UpgradeStrategy.Automatic, "Auto", "Use Swiftcast to maintain swift rotation by instant-casting Blizzard III after Fire Phase (e.g. Despair->Transpose->Swiftcast->B3 etc.)", 0, 0, ActionTargets.Self, 66)
            .AddOption(UpgradeStrategy.Force, "Force", "Force the use of Swiftcast (60s cooldown), regardless of weaving conditions", 60, 10, ActionTargets.Self, 18, 93)
            .AddOption(UpgradeStrategy.ForceWeave, "ForceWeave", "Force the use of Swiftcast (60s cooldown) in any next possible weave slot", 180, 10, ActionTargets.Self, 18, 93)
            .AddOption(UpgradeStrategy.ForceEX, "ForceEX", "Force the use of Swiftcast (40s cooldown), regardless of weaving conditions", 40, 10, ActionTargets.Self, 94)
            .AddOption(UpgradeStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Swiftcast (40s cooldown) in any next possible weave slot", 40, 10, ActionTargets.Self, 94)
            .AddOption(UpgradeStrategy.Delay, "Delay", "Delay the use of Swiftcast for strategic reasons", 0, 0, ActionTargets.Self, 18)
            .AddAssociatedActions(AID.Swiftcast);
        res.Define(Track.LeyLines).As<ChargeStrategy>("L.Lines", uiPriority: 196)
            .AddOption(ChargeStrategy.Automatic, "Auto", "Automatically decide when to use Ley Lines", 0, 0, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.Force, "Force", "Force the use of Ley Lines, regardless of weaving conditions", 120, 30, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.Force1, "Force1", "Force the use of Ley Lines; holds one charge for manual usage", 120, 30, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.ForceWeave, "ForceWeave", "Force the use of Ley Lines in any next possible weave slot", 120, 30, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.ForceWeave1, "ForceWeave1", "Force the use of Ley Lines in any next possible weave slot; holds one charge for manual usage", 120, 30, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.Delay, "Delay", "Delay the use of Ley Lines", 0, 0, ActionTargets.Self, 2)
            .AddAssociatedActions(AID.LeyLines);
        res.Define(Track.TPUS).As<TPUSStrategy>("Transpose & Umbral Soul", "TP/US", uiPriority: 160)
            .AddOption(TPUSStrategy.Allow, "Allow", "Allow Transpose & Umbral Soul combo when either out of combat or no targetable enemy is nearby", minLevel: 35)
            .AddOption(TPUSStrategy.OOConly, "OOConly", "Only use Transpose & Umbral Soul combo when fully out of combat", minLevel: 35)
            .AddOption(TPUSStrategy.Forbid, "Forbid", "Forbid Transpose & Umbral Soul combo", minLevel: 35)
            .AddAssociatedActions(AID.Transpose, AID.UmbralSoul);
        res.Define(Track.Movement).As<MovementStrategy>("Moving", uiPriority: 199)
            .AddOption(MovementStrategy.Allow, "Allow", "Allow the use of all appropriate abilities for use while moving")
            .AddOption(MovementStrategy.AllowNoScathe, "AllowNoScathe", "Allow the use of all appropriate abilities for use while moving except for Scathe")
            .AddOption(MovementStrategy.OnlyGCDs, "OnlyGCDs", "Only use instant cast GCDs for use while moving; Polyglots->Thunder->Scathe if nothing left")
            .AddOption(MovementStrategy.OnlyOGCDs, "OnlyOGCDs", "Only use OGCDs for use while moving; Swiftcast->Triplecast (NOTE: This will most likely cause you to enter UI3 without Transpose if moving frequently)")
            .AddOption(MovementStrategy.OnlyScathe, "OnlyScathe", "Only use Scathe for use while moving")
            .AddOption(MovementStrategy.Forbid, "Forbid", "Forbid the use of any abilities for use while moving");
        res.Define(Track.Casting).As<CastingOption>("Cast", uiPriority: 200)
            .AddOption(CastingOption.Allow, "Allow", "Allow casting while moving")
            .AddOption(CastingOption.Forbid, "Forbid", "Forbid casting while moving");
        res.DefineOGCD(Track.Amplifier, AID.Amplifier, "Amplifier", "Amp.", uiPriority: 196, 120, 0, ActionTargets.Self, 86);
        res.DefineOGCD(Track.Retrace, AID.Retrace, "Retrace", "Retrace", uiPriority: 170, 40, 0, ActionTargets.Self, 96);
        res.DefineOGCD(Track.BTL, AID.BetweenTheLines, "BTL", "BTL", uiPriority: 170, 40, 0, ActionTargets.Self, 62);

        return res;
    }
    #endregion

    #region Upgrade Paths
    private AID BestThunderST => Unlocked(AID.HighThunder) ? AID.HighThunder : Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;
    private AID BestThunderAOE => Unlocked(AID.HighThunder2) ? AID.HighThunder2 : Unlocked(AID.Thunder4) ? AID.Thunder4 : Unlocked(AID.Thunder2) ? AID.Thunder2 : AID.Thunder1;
    private AID BestThunder => ShouldUseAOE ? BestThunderAOE : BestThunderST;
    private AID BestPolyglot => ShouldUseAOE ? AID.Foul : BestXenoglossy;
    private AID BestXenoglossy => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    private AID BestBlizzardAOE => Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : Unlocked(AID.Blizzard2) ? AID.Blizzard2 : AID.Blizzard1;
    #endregion

    #region Module Variables
    private sbyte ElementStance;
    private bool NoUIorAF;
    private int AF;
    private bool InAF;
    private int UI;
    private bool InUI;
    private byte Hearts;
    private int MaxHearts;
    private int Souls;
    private byte Polyglots;
    private int MaxPolyglots;
    private int PolyglotTimer;
    private bool HasParadox;
    private bool HasThunderhead;
    private bool HasFirestarter;
    private bool HasMaxPolyglots;
    private bool HasMaxHearts;
    private bool HasMaxSouls;
    private bool CanFoul;
    private bool CanXG;
    private bool CanParadox;
    private bool CanLL;
    private bool CanAmplify;
    private bool CanTC;
    private bool CanMF;
    private bool CanRetrace;
    private bool CanBTL;
    private float ThunderLeft;
    private bool ShouldUseAOE;
    private bool ShouldUseSTDOT;
    private bool ShouldUseAOEDOT;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestDOTTargets;
    private Enemy? BestSplashTarget;
    private Enemy? BestDOTTarget;
    #endregion

    #region Rotation Helpers
    public void BestST(Actor? target, GCDPriority prio)
    {
        if (!In25y(target))
            return;

        if (InAF)
        {
            if ((Hearts > 0 ? MP >= 800 : MP >= 1600) && Souls != 6)
                QueueGCD(Unlocked(AID.Fire4) ? AID.Fire4 : AID.Fire1, target, prio);
            if (Unlocked(AID.Blizzard3) && (((!Unlocked(AID.Despair) || LastActionUsed(AID.Despair)) || MP == 0 && !HasMaxSouls) && ((!HasEffect(SID.Swiftcast) && !Unlocked(AID.Triplecast)) || (!HasEffect(SID.Swiftcast) || !HasEffect(SID.Triplecast)))))
                QueueGCD(AID.Blizzard3, target, prio);
        }
        if (InUI)
        {
            if (Unlocked(AID.Blizzard3) && UI != 3 && ((HasEffect(SID.Swiftcast) || !Unlocked(AID.Triplecast)) || (HasEffect(SID.Swiftcast) || HasEffect(SID.Triplecast))))
                QueueGCD(AID.Blizzard3, target, prio);
            if (LastActionUsed(AID.Blizzard3) || (Unlocked(TraitID.UmbralHeart) ? (UI == 3 && !HasMaxHearts) : MP != MaxMP))
                QueueGCD(Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Blizzard1, target, prio);
        }
    }
    private void BestAOE(Actor? target, GCDPriority prio)
    {
        if (!In25y(target))
            return;

        if (!Unlocked(AID.FlareStar))
        {
            if (Unlocked(AID.Fire2) && ((InUI && (MP == 10000 && UI == 3)) || (InAF && (Unlocked(TraitID.UmbralHeart) ? MP > 5500 : MP >= 3000))))
                QueueGCD(Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2, target, prio);

            if (InUI)
            {
                if (Unlocked(AID.Blizzard2) && ((InUI && UI < 3) || (InAF && MP < 400)))
                    QueueGCD(BestBlizzardAOE, target, prio);
                if (!LastActionUsed(AID.Freeze) && UI == 3 && MP < 10000)
                    QueueGCD(Unlocked(AID.Freeze) ? AID.Freeze : BestBlizzardAOE, target, prio);
            }
        }
        if (Unlocked(AID.FlareStar))
        {
            if (NoUIorAF) //start AOE with HB2
                QueueGCD(AID.HighBlizzard2, target, prio);
            if (InUI)
            {
                if (!HasMaxHearts)
                    QueueGCD(AID.Freeze, target, prio);
                if (HasMaxHearts)
                {
                    AOEfiller(target, prio);
                }
            }
            if (InAF)
            {
                if (MP < 800 && Souls == 0)
                {
                    AOEfiller(target, prio);
                }
                //desync or swap from ST
                if (MP > 800)
                    QueueGCD(AID.Flare, target, prio);
            }
        }
    }
    private void AOEfiller(Actor? target, GCDPriority prio)
    {
        if (HasThunderhead && (ThunderLeft < 15 || Polyglots == 0))
            QueueGCD(BestThunderAOE, target, prio + 2);
        if (Polyglots > 0)
            QueueGCD(AID.Foul, target, prio + 1);
        if (LastActionUsed(BestThunderAOE) || LastActionUsed(AID.Foul) ||
            (!HasThunderhead && Polyglots == 0)) //this will clip, but doing this raw is rare
            QueueOGCD(AID.Transpose, target, prio - 1000);
    }
    #endregion

    #region Cooldown Helpers
    public void QueueOpener(Actor? target)
    {
        if (target == null)
            return;

        if (!ShouldUseAOE)
        {
            if (NoUIorAF)
            {
                //Opener - F3
                if (CountdownRemaining == null || CombatTimer == 0 || !Player.InCombat)
                    QueueGCD(AID.Fire3, target, GCDPriority.SlightlyHigh);

                //Recovery - B3
                if (!IsFirstGCD)
                {
                    if (Unlocked(AID.Blizzard3) && UI < 3)
                        QueueGCD(OGCDReady(AID.Swiftcast) && !HasEffect(SID.Triplecast) ? AID.Swiftcast : CanTC && !HasEffect(SID.Swiftcast) ? AID.Triplecast : AID.Blizzard3, target, GCDPriority.SlightlyHigh);
                    if (InAF)
                        BestST(target, GCDPriority.Low);
                }
            }
        }
        if (ShouldUseAOE)
        {
            if (NoUIorAF || (InUI && UI < 3))
            {
                if (CountdownRemaining == null || CombatTimer == 0 || !Player.InCombat)
                {
                    if (CountdownRemaining <= 2)
                        QueueGCD(AID.HighBlizzard2, target, GCDPriority.SlightlyHigh);
                }
            }
        }

        return;
    }
    public void ShouldUseMovement(Actor? target, StrategyValues strategy)
    {
        var m = strategy.Option(Track.Movement).As<MovementStrategy>();
        if (m is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyGCDs)
        {
            if (!HasEffect(AID.Swiftcast) || !HasEffect(SID.Triplecast))
            {
                if (Unlocked(TraitID.EnhancedPolyglot) && Polyglots > 0)
                    QueueGCD(strategy.ForceST() ? BestXenoglossy : strategy.ForceAOE() ? AID.Foul : BestPolyglot, TargetChoice(strategy.Option(Track.Polyglot)) ?? (strategy.ForceST() ? target : BestSplashTarget?.Actor), GCDPriority.Severe);
                if (HasThunderhead)
                    QueueGCD(strategy.ForceST() ? BestThunderST : strategy.ForceAOE() ? BestThunderAOE : BestThunder, TargetChoice(strategy.Option(Track.Thunder)) ?? (strategy.ForceST() ? target : BestSplashTarget?.Actor), GCDPriority.Severe);
            }
        }
        if (m is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyOGCDs)
        {
            if (OGCDReady(AID.Swiftcast) && !HasEffect(SID.Triplecast))
                QueueOGCD(AID.Swiftcast, Player, OGCDPriority.Severe + 1);
            if (CanTC && !HasEffect(AID.Swiftcast))
                QueueOGCD(AID.Triplecast, Player, OGCDPriority.Severe);
        }
        if (m is MovementStrategy.Allow or MovementStrategy.OnlyScathe)
        {
            if (Unlocked(AID.Scathe) && MP >= 800)
                QueueGCD(AID.Scathe, TargetChoice(strategy.Option(SharedTrack.AOE)) ?? target, GCDPriority.Severe);
        }
    }
    #region GCD
    private (bool, GCDPriority) ShouldUsePolyglot(Actor? target, PolyglotStrategy strategy)
    {
        var max = HasMaxPolyglots;
        var veryclose = PolyglotTimer <= 3000f && max;
        var overcap = PolyglotTimer <= 7500f && max;
        var openMF = CombatTimer < 40 && CDRemaining(AID.Manafont) <= 1.5f && InAF && MP <= 400;
        var condition = Polyglots > 0 && InsideCombatWith(target) && (overcap ||
                (CDRemaining(AID.LeyLines) < 5 || CDRemaining(AID.LeyLines) == 0) || //LL overcap prep
                (CDRemaining(AID.Manafont) <= 1.5f && InAF && MP <= 400) || //MF prep
                (CDRemaining(AID.Amplifier) <= GCD && HasMaxPolyglots)); //Amp prep
        var prio = veryclose ? GCDPriority.ExtremelyHigh + 1 : GCDPriority.SlightlyHigh;
        return strategy switch
        {
            PolyglotStrategy.AutoSpendAll or PolyglotStrategy.XenoSpendAll or PolyglotStrategy.FoulSpendAll => (Polyglots > 0 && condition, prio),
            PolyglotStrategy.AutoHold1 or PolyglotStrategy.XenoHold1 or PolyglotStrategy.FoulHold1 => (openMF || (Polyglots > 1 && condition), prio),
            PolyglotStrategy.AutoHold2 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.FoulHold2 => (openMF || (Polyglots > 2 && condition), prio),
            PolyglotStrategy.AutoHold3 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.FoulHold3 => (openMF || (HasMaxPolyglots && overcap), prio),
            PolyglotStrategy.ForceXeno => (CanXG, GCDPriority.Forced),
            PolyglotStrategy.ForceFoul => (CanFoul, GCDPriority.Forced),
            _ => (false, GCDPriority.None),
        };
    }

    #region DOT
    private static SID[] GetDotStatus() => [SID.Thunder, SID.ThunderII, SID.ThunderIII, SID.ThunderIV, SID.HighThunder, SID.HighThunderII];
    private float ThunderRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);
    private (bool, GCDPriority) ShouldUseThunder(Actor? target, ThunderStrategy strategy)
    {
        if (!HasThunderhead)
            return (false, GCDPriority.None);

        var condition = InsideCombatWith(target) && In25y(target);
        return strategy switch
        {
            ThunderStrategy.Allow3 => (condition && ThunderLeft <= 3, GCDPriority.High),
            ThunderStrategy.Allow6 => (condition && ThunderLeft < 6, GCDPriority.High),
            ThunderStrategy.Allow9 => (condition && ThunderLeft < 9, GCDPriority.High),
            ThunderStrategy.AllowNoDOT => (condition && ThunderLeft == 0, GCDPriority.ExtremelyHigh),
            ThunderStrategy.ForceAny or ThunderStrategy.ForceST or ThunderStrategy.ForceAOE => (true, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
    }
    #endregion

    private bool ShouldUseF3P => Unlocked(AID.Fire3) && !ShouldUseAOE && HasFirestarter && InAF && AF < 3;
    private bool ShouldUseParadox => CanParadox && !ShouldUseAOE && (AF == 3 ? CombatTimer > 35 : CombatTimer >= 0) && ((InUI && Hearts == MaxHearts) || (InAF && !HasFirestarter));
    private bool ShouldUseFlare => Unlocked(AID.Flare) && (ShouldUseAOE ? (InAF && (Unlocked(AID.FlareStar) ? (InAF && Souls != 6 && MP >= 800) : (Unlocked(TraitID.UmbralHeart) ? (Hearts > 0 || (Hearts == 0 && MP >= 800)) : MP is < 3000 and >= 800))) : (InAF && ((Souls != 6 && MP is < 1600 and >= 800) || (DowntimeIn <= 6 && Souls >= 3))));
    #endregion

    #region OGCD
    private (bool, OGCDPriority) ShouldUseTranspose => (Unlocked(AID.Transpose) && ((InUI && UI == 3 && Hearts == MaxHearts && !HasParadox) || (InAF && AF == 3 && (LastActionUsed(AID.Despair) || (MP == 0 && Souls == 0)))), OGCDPriority.ExtremelyHigh);
    private (bool, OGCDPriority) ShouldUseManafont(Actor? target, UpgradeStrategy strategy)
    {
        if (!CanMF)
            return (false, OGCDPriority.None);

        var condition = InsideCombatWith(target) && InAF && (ShouldUseAOE ? HasParadox : !HasParadox) && MP < 800;
        return strategy switch
        {
            UpgradeStrategy.Automatic => (condition, OGCDPriority.Severe),
            UpgradeStrategy.Force or UpgradeStrategy.ForceEX => (CanMF, OGCDPriority.Severe),
            UpgradeStrategy.ForceWeave or UpgradeStrategy.ForceWeaveEX => (CanMF && CanWeaveIn, OGCDPriority.Severe),
            _ => (false, OGCDPriority.None)
        };
    }
    private (bool, OGCDPriority) ShouldUseLeyLines(Actor? target, ChargeStrategy strategy)
    {
        if (!CanLL)
            return (false, OGCDPriority.None);

        return strategy switch
        {
            ChargeStrategy.Automatic => (InsideCombatWith(target) && (RaidBuffsLeft > 0f || RaidBuffsIn < 3000f), OGCDPriority.Average + 1),
            ChargeStrategy.Force => (true, OGCDPriority.Average + 1),
            ChargeStrategy.Force1 => (CDRemaining(AID.LeyLines) <= 5, OGCDPriority.Average + 1),
            ChargeStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Average + 1),
            ChargeStrategy.ForceWeave1 => (CanWeaveIn && CDRemaining(AID.LeyLines) <= 5, OGCDPriority.Average + 1),
            _ => (false, OGCDPriority.None)
        };
    }
    private bool ShouldUseTriplecast(Actor? target, ChargeStrategy strategy)
    {
        if (!CanTC)
            return false;

        return strategy switch
        {
            ChargeStrategy.Automatic => InsideCombatWith(target) && CanTC && InUI && UI < 3 && !HasEffect(SID.Swiftcast) && CDRemaining(AID.Swiftcast) > 0.6f,
            ChargeStrategy.Force => true,
            ChargeStrategy.Force1 => CDRemaining(AID.Triplecast) <= 5f,
            ChargeStrategy.ForceWeave => CanWeaveIn,
            ChargeStrategy.ForceWeave1 => CanWeaveIn && CDRemaining(AID.Triplecast) <= 5f,
            _ => false
        };
    }
    private bool ShouldUseSwiftcast(Actor? target, UpgradeStrategy strategy)
    {
        if (!CanSwiftcast)
            return false;

        return strategy switch
        {
            UpgradeStrategy.Automatic => InsideCombatWith(target) && InUI && UI < 3 && !HasEffect(SID.Triplecast),
            UpgradeStrategy.Force or UpgradeStrategy.ForceEX => true,
            UpgradeStrategy.ForceWeave or UpgradeStrategy.ForceWeaveEX => CanWeaveIn,
            _ => false
        };
    }
    private (bool, OGCDPriority) TriggerInstantCasts(Actor? target, StrategyValues strategy)
    {
        if (ShouldUseAOE)
            return (false, OGCDPriority.None);

        //we want Swiftcast first primarily.. and if not up, send Triplecast
        if (InUI && (UI < 3 || LastActionUsed(AID.Transpose)))
        {
            if (ShouldUseSwiftcast(target, strategy.Option(Track.Swiftcast).As<UpgradeStrategy>()))
                return (true, OGCDPriority.Average + 1);
            if (ShouldUseTriplecast(target, strategy.Option(Track.Triplecast).As<ChargeStrategy>()))
                return (true, OGCDPriority.Average);
        }

        return (false, OGCDPriority.None);
    }
    private bool ShouldUseOGCD(bool minimum, bool condition, OGCDStrategy strategy)
    {
        if (!minimum)
            return false;

        return strategy switch
        {
            OGCDStrategy.Automatic => condition,
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            _ => false
        };
    }
    private bool ShouldUseAmplifier(OGCDStrategy strategy) => ShouldUseOGCD(CanAmplify, !HasMaxPolyglots, strategy);
    private bool ShouldUseRetrace(OGCDStrategy strategy) => ShouldUseOGCD(CanRetrace, false, strategy);
    private bool ShouldUseBTL(OGCDStrategy strategy) => ShouldUseOGCD(CanBTL, false, strategy);
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs or PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<BlackMageGauge>();
        ElementStance = gauge.ElementStance;
        NoUIorAF = ElementStance is 0 and not (1 or 2 or 3 or -1 or -2 or -3);
        InAF = ElementStance is 1 or 2 or 3 and not (0 or -1 or -2 or -3);
        AF = gauge.AstralStacks;
        InUI = ElementStance is -1 or -2 or -3 and not (0 or 1 or 2 or 3);
        UI = gauge.UmbralStacks;
        Hearts = gauge.UmbralHearts;
        MaxHearts = Unlocked(TraitID.UmbralHeart) ? 3 : 0;
        HasMaxHearts = Hearts == MaxHearts;
        Souls = gauge.AstralSoulStacks;
        HasMaxSouls = Souls == 6;
        HasParadox = gauge.ParadoxActive;
        Polyglots = gauge.PolyglotStacks;
        MaxPolyglots = Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;
        HasMaxPolyglots = Polyglots == MaxPolyglots;
        PolyglotTimer = gauge.EnochianTimer;
        CanFoul = Unlocked(AID.Foul) && Polyglots > 0;
        CanXG = Unlocked(AID.Xenoglossy) && Polyglots > 0;
        CanParadox = Unlocked(AID.Paradox) && HasParadox && MP >= 1600;
        CanLL = Unlocked(AID.LeyLines) && CDRemaining(AID.LeyLines) <= 120 && Player.FindStatus(SID.LeyLines) == null;
        CanAmplify = OGCDReady(AID.Amplifier);
        CanTC = Unlocked(AID.Triplecast) && CDRemaining(AID.Triplecast) <= 60 && Player.FindStatus(SID.Triplecast) == null;
        CanMF = OGCDReady(AID.Manafont);
        CanRetrace = OGCDReady(AID.Retrace) && Player.FindStatus(SID.LeyLines) != null;
        CanBTL = OGCDReady(AID.BetweenTheLines) && Player.FindStatus(SID.LeyLines) != null;
        HasThunderhead = Player.FindStatus(SID.Thunderhead) != null;
        HasFirestarter = Player.FindStatus(SID.Firestarter) != null;
        ThunderLeft = Utils.MaxAll(
            StatusDetails(BestSplashTarget?.Actor, SID.Thunder, Player.InstanceID, 24).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderII, Player.InstanceID, 18).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderIII, Player.InstanceID, 27).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderIV, Player.InstanceID, 21).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.HighThunder, Player.InstanceID, 30).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.HighThunderII, Player.InstanceID, 24).Left);
        ShouldUseAOE = strategy.ForceAOE() || (Unlocked(AID.Blizzard2) && NumSplashTargets > 2);
        ShouldUseSTDOT = Unlocked(AID.Thunder1) && NumSplashTargets <= 2;
        ShouldUseAOEDOT = Unlocked(AID.Thunder2) && NumSplashTargets > 2;
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        (BestDOTTargets, ThunderLeft) = GetDOTTarget(primaryTarget, ThunderRemaining, 2);
        BestSplashTarget = ShouldUseAOE ? BestSplashTargets : primaryTarget;
        BestDOTTarget = ShouldUseAOEDOT ? BestSplashTargets : ShouldUseSTDOT ? BestDOTTargets : primaryTarget;

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE);
        var movementStrat = strategy.Option(Track.Movement).As<MovementStrategy>();
        var thunder = strategy.Option(Track.Thunder);
        var thunderStrat = thunder.As<ThunderStrategy>();
        var polyglot = strategy.Option(Track.Polyglot);
        var polyglotStrat = polyglot.As<PolyglotStrategy>();
        var mf = strategy.Option(Track.Manafont);
        var mfStrat = mf.As<UpgradeStrategy>();
        var tc = strategy.Option(Track.Triplecast);
        var tcStrat = tc.As<ChargeStrategy>();
        var sc = strategy.Option(Track.Swiftcast);
        var scStrat = sc.As<UpgradeStrategy>();
        var ll = strategy.Option(Track.LeyLines);
        var llStrat = ll.As<ChargeStrategy>();
        var amp = strategy.Option(Track.Amplifier);
        var ampStrat = amp.As<OGCDStrategy>();
        var retrace = strategy.Option(Track.Retrace);
        var retraceStrat = retrace.As<OGCDStrategy>();
        var btl = strategy.Option(Track.BTL);
        var btlStrat = btl.As<OGCDStrategy>();
        var tpusStrat = strategy.Option(Track.TPUS).As<TPUSStrategy>();
        var movingOption = strategy.Option(Track.Casting).As<CastingOption>();
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Out of Combat
        if (BestSplashTarget?.Actor == null &&
            (tpusStrat == TPUSStrategy.Allow && (!Player.InCombat || Player.InCombat && Hints.NumPriorityTargetsInAOECircle(Player.Position, 30) == 0) ||
            tpusStrat == TPUSStrategy.OOConly && !Player.InCombat))
        {
            if (OGCDReady(AID.Transpose))
            {
                if (!Unlocked(AID.UmbralSoul))
                {
                    var fsPotential = Souls == 6 || (Souls >= 3 && MP >= 800);
                    if ((InAF && !fsPotential) || InUI)
                        QueueOGCD(AID.Transpose, Player, OGCDPriority.ExtremelyHigh);
                }
                if (Unlocked(AID.UmbralSoul))
                {
                    var fsPotential = Souls == 6 || (Souls >= 3 && MP >= 800);
                    if ((InAF && !fsPotential) || (InUI && !HasThunderhead))
                        QueueOGCD(AID.Transpose, Player, OGCDPriority.ExtremelyHigh);
                    if (InUI && HasThunderhead && (UI < 3 || !HasMaxHearts))
                        QueueGCD(AID.UmbralSoul, Player, GCDPriority.ExtremelyHigh);
                }
            }
        }

        if (CountdownRemaining == null || CombatTimer == 0 || !Player.InCombat)
            QueueOpener(primaryTarget?.Actor);
        if (InsideCombatWith(primaryTarget?.Actor) && IsMoving)
            ShouldUseMovement(primaryTarget?.Actor, strategy);

        #endregion

        #region Standard Rotations
        if (movingOption is CastingOption.Allow ||
            movingOption is CastingOption.Forbid &&
            (!IsMoving || HasEffect(SID.Swiftcast) || HasEffect(SID.Triplecast) || ShouldUseParadox || ShouldUseF3P || (Unlocked(TraitID.EnhancedAstralFire) && InAF && MP is <= 800 and not 0)))
        {
            if (strategy.AutoFinish() || strategy.AutoBreak())
            {
                if (ShouldUseAOE)
                    BestAOE(TargetChoice(AOE) ?? BestSplashTarget?.Actor, GCDPriority.Low);
                else
                    BestST(TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
            }
            if (strategy.ForceST())
                BestST(TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Low);
            if (strategy.ForceAOE())
                BestAOE(TargetChoice(AOE) ?? BestSplashTarget?.Actor, GCDPriority.Low);
        }
        #endregion

        #region Cooldowns

        #region GCD
        //Thunder
        var (tCondition, tPrio) = ShouldUseThunder(primaryTarget?.Actor, thunderStrat);
        if (tCondition)
        {
            var action = thunderStrat switch
            {
                ThunderStrategy.Allow3 or ThunderStrategy.Allow6 or ThunderStrategy.Allow9 or ThunderStrategy.AllowNoDOT or ThunderStrategy.ForceAny => BestThunder,
                ThunderStrategy.ForceST => BestThunderST,
                ThunderStrategy.ForceAOE => BestThunderAOE,
                _ => AID.None
            };
            QueueGCD(action, TargetChoice(thunder) ?? BestDOTTarget?.Actor, tPrio);
        }
        //F3P
        if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseF3P)
            QueueGCD(AID.Fire3, primaryTarget?.Actor, GCDPriority.Average);
        //Paradox
        if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseParadox)
            QueueGCD(AID.Paradox, primaryTarget?.Actor, GCDPriority.Average);
        //Despair
        if (InsideCombatWith(primaryTarget?.Actor) && Unlocked(AID.Despair) && !ShouldUseAOE && InAF && MP is < 1600 and >= 800)
            QueueGCD(AID.Despair, primaryTarget?.Actor, GCDPriority.Average + 1);
        //FlareStar
        if (InsideCombatWith(BestSplashTarget?.Actor) && Unlocked(AID.FlareStar) && HasMaxSouls)
            QueueGCD(AID.FlareStar, BestSplashTarget?.Actor, GCDPriority.Average + 6);
        //Flare
        if (InsideCombatWith(BestSplashTarget?.Actor) && ShouldUseFlare)
            QueueGCD(AID.Flare, BestSplashTarget?.Actor, GCDPriority.Average);
        //Polyglots
        var (pgCondition, pgPrio) = ShouldUsePolyglot(primaryTarget?.Actor, polyglotStrat);
        if (pgCondition)
        {
            var action = polyglotStrat switch
            {
                PolyglotStrategy.AutoSpendAll or PolyglotStrategy.AutoHold1 or PolyglotStrategy.AutoHold2 or PolyglotStrategy.AutoHold3 => BestPolyglot,
                PolyglotStrategy.XenoSpendAll or PolyglotStrategy.XenoHold1 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.ForceXeno => BestXenoglossy,
                PolyglotStrategy.FoulSpendAll or PolyglotStrategy.FoulHold1 or PolyglotStrategy.FoulHold2 or PolyglotStrategy.FoulHold3 or PolyglotStrategy.ForceFoul => AID.Foul,
                _ => AID.None
            };
            QueueGCD(action, TargetChoice(polyglot) ?? BestSplashTarget?.Actor, pgPrio);
        }

        #endregion

        #region OGCD
        //Swiftcast & Triplecast
        var (icCondition, icPrio) = TriggerInstantCasts(primaryTarget?.Actor, strategy);
        if (icCondition)
        {
            QueueOGCD(AID.Swiftcast, Player, icPrio + 1);
            QueueOGCD(AID.Triplecast, Player, icPrio);
        }
        //Transpose
        var (tpCondition, tpPrio) = ShouldUseTranspose;
        if (tpCondition)
            QueueOGCD(AID.Transpose, Player, tpPrio);
        //Manafont
        var (mfCondition, mfPrio) = ShouldUseManafont(primaryTarget?.Actor, mfStrat);
        if (mfCondition)
            QueueOGCD(AID.Manafont, Player, mfPrio);
        //LeyLines
        var (llCondition, llPrio) = ShouldUseLeyLines(primaryTarget?.Actor, llStrat);
        if (llCondition)
            QueueOGCD(AID.LeyLines, Player, llPrio);

        //Amplifier
        if (ShouldUseAmplifier(ampStrat))
            QueueOGCD(AID.Amplifier, Player, ampStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : OGCDPriority.High);
        if (ShouldUseRetrace(retraceStrat))
            QueueOGCD(AID.Retrace, Player, OGCDPriority.Forced);
        //Between the Lines
        if (ShouldUseBTL(btlStrat))
            QueueOGCD(AID.BetweenTheLines, Player, OGCDPriority.Forced);
        #endregion

        //Potion
        if (ShouldUsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, ActionQueue.Priority.Medium);

        #endregion

        #region AI
        GoalZoneSingle(25);
        #endregion

        #endregion
    }
}
