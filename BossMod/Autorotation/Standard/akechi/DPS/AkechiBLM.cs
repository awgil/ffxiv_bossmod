using BossMod.BLM;
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Thunder = SharedTrack.Count, Ender, Polyglot, Manafont, Triplecast, Swiftcast, LeyLines, TPUS, Movement, Casting, Amplifier, Retrace, BTL }
    public enum ThunderStrategy { Allow3, Allow6, Allow9, AllowNoDOT, ForceAny, ForceST, ForceAOE, Forbid }
    public enum EnderStrategy { Automatic, OnlyDespair, OnlyFlare, ForceDespair, ForceFlare }
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
        var res = new RotationModuleDefinition("Akechi BLM", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Ok, BitMask.Build(Class.THM, Class.BLM), 100);
        res.DefineAOE().AddAssociatedActions(
            AID.Fire1, AID.Fire2, AID.Fire3, AID.Fire4, AID.HighFire2, //Fire
            AID.Blizzard1, AID.Blizzard2, AID.Blizzard3, AID.Blizzard4, AID.HighBlizzard2, //Blizzard
            AID.Flare, AID.Freeze, AID.Despair, AID.FlareStar); //Other
        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionInt);
        res.Define(Track.Thunder).As<ThunderStrategy>("Thunder", "DOT", uiPriority: 198)
            .AddOption(ThunderStrategy.Allow3, "Allow3", "Allow the use Thunder if target Has 3s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow6, "Allow6", "Allow the use Thunder if target Has 6s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow9, "Allow9", "Allow the use Thunder if target Has 9s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.AllowNoDOT, "AllowNoDOT", "Allow the use Thunder only if target does not have DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAny, "ForceAny", "Force use of best Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceST, "ForceST", "Force use of single-target Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAOE, "ForceAOE", "Force use of AOE Thunder regardless of DoT effect", 0, 24, ActionTargets.Hostile, 26)
            .AddOption(ThunderStrategy.Forbid, "Forbid", "Forbid the use of Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(AID.Thunder1, AID.Thunder2, AID.Thunder3, AID.Thunder4, AID.HighThunder, AID.HighThunder2);
        res.Define(Track.Ender).As<EnderStrategy>("Despair", uiPriority: 197)
            .AddOption(EnderStrategy.Automatic, "Auto", "Automatically use Despair or Flare based on targets nearby", 0, 0, ActionTargets.Hostile, 50)
            .AddOption(EnderStrategy.OnlyDespair, "OnlyDespair", "Use Despair only, regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(EnderStrategy.OnlyFlare, "OnlyFlare", "Use Flare only, regardless of targets", 0, 0, ActionTargets.Hostile, 50)
            .AddOption(EnderStrategy.ForceDespair, "ForceDespair", "Force the use of Despair if possible", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(EnderStrategy.ForceFlare, "ForceFlare", "Force the use of Flare if possible", 0, 0, ActionTargets.Hostile, 50)
            .AddAssociatedActions(AID.Despair, AID.Flare);
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
            .AddOption(ChargeStrategy.Force, "Force", "Force the use of Ley Lines, regardless of weaving conditions", 120, 20, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.Force1, "Force1", "Force the use of Ley Lines; holds one charge for manual usage", 120, 20, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.ForceWeave, "ForceWeave", "Force the use of Ley Lines in any next possible weave slot", 120, 20, ActionTargets.Self, 2)
            .AddOption(ChargeStrategy.ForceWeave1, "ForceWeave1", "Force the use of Ley Lines in any next possible weave slot; holds one charge for manual usage", 120, 20, ActionTargets.Self, 2)
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

    #region Module Variables
    private sbyte ElementStance;
    private int MaxAspect;
    private bool NoUIorAF;
    private int AF;
    private bool InAF;
    private bool MaxAF;
    private int UI;
    private bool InUI;
    private bool MaxUI;
    private byte Hearts;
    private int MaxHearts;
    private bool HasMaxHearts;
    private int Souls;
    private int MaxSouls;
    private bool HasMaxSouls;
    private byte Polyglots;
    private int PolyglotTimer;
    private int MaxPolyglots;
    private bool HasMaxPolyglots;
    private bool HasThunderhead;
    private bool HasFirestarter;
    private bool HasParadox;
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
    private void AstralFireST(StrategyValues strategy, Actor? target, GCDPriority prio)
    {
        if (!In25y(target) || !InAF)
            return;

        //ideally we want to use F4 as much as possible
        //thanks to no more timers on our buffs, we can spend everything else asap and then spam F4 6-7 times in a row
        //F1 - used basically as placeholder until F4 because who cares?
        //F4 - as soon as we're in AF3 & no resources, we burn it all
        if (MaxAF && (Hearts > 0 ? MP >= 800 : MP >= 1600)) //MP cost for F4
        {
            if ((Unlocked(AID.FlareStar) && Souls != 6) || //Lv100 - Need 6 stacks to execute FlareStar
                !Unlocked(AID.FlareStar)) //below Lv100 - just burn until we need Despair or 7th F4
                QueueGCD(Unlocked(AID.Fire4) ? AID.Fire4 : AID.Fire1, target, prio);
        }

        //by now, we want to instant-cast B3 after a Transpose to get back into UI
        //at lower levels, we want to either rawdog B3 or instant-cast it after a TP
        //check if appropriate resources are available for it, and rawdog it if not
        if (Unlocked(AID.Blizzard3))
        {
            if ((Unlocked(AID.Flare) && Souls == 0 && (MP == 0 || LastActionUsed(AID.Flare) || LastActionUsed(AID.Despair))) || //Lv50+ - emptied by ender
                (!Unlocked(AID.Flare) && MP < 1600)) //below Lv50 - not enough MP to use F1
                QueueGCD(AID.Blizzard3, target, prio);
        }
    }
    private void UmbralIceST(Actor? target, GCDPriority prio)
    {
        if (!In25y(target) || !InUI)
            return;

        //this phase is usually pretty quick
        //we essentially only swap back to UI just to refresh our resources & then swap back to AF for dmg
        //first thing after swapping is to refresh hearts (if we're already in UI3)
        //this will halve our cost of 3 F4s
        if (LastActionUsed(AID.Blizzard3) ||
            (Unlocked(TraitID.UmbralHeart) ? (UI == 3 && !HasMaxHearts) : MP != MaxMP))
            QueueGCD(Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Blizzard1, target, prio);

        //if we so happen to not have UI3 on swap, then we B3 for it
        if (Unlocked(AID.Blizzard3) && UI != 3)
            QueueGCD(AID.Blizzard3, target, prio);

        //below Lv90 - we do not have access to Paradox, so we have to rawdog F3 after B4
        if (!Unlocked(AID.Paradox) && Unlocked(AID.Fire3) && MaxUI)
            QueueGCD(AID.Fire3, target, prio);
    }
    private void BestST(StrategyValues strategy, Actor? target, GCDPriority prio)
    {
        if (ShouldUseAOE || !In25y(target))
            return;

        AstralFireST(strategy, target, prio);
        UmbralIceST(target, prio);
    }
    private void BestAOE(Actor? target, GCDPriority prio, StrategyValues strategy)
    {
        if (!ShouldUseAOE || strategy.ForceST() || !In25y(target))
            return;

        //Lv58+ - we have a very simple AOE rotation
        //which is basically just 2xFlares in a row and then a Freeze with some filler
        if (Unlocked(TraitID.UmbralHeart))
        {
            //start with B2
            if (NoUIorAF)
                QueueGCD(Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : AID.Blizzard2, target, prio);

            //in UI, we use Freeze just to obtain max Hearts
            if (InUI)
            {
                if (!HasMaxHearts)
                    QueueGCD(AID.Freeze, target, prio);
                if (HasMaxHearts)
                    AOEfiller(target, prio, strategy);
            }
            //in AF, we use Flare->Flare->FlareStar (if available)
            if (InAF)
            {
                if (MP > 800)
                    QueueGCD(AID.Flare, target, prio);
                if (MP < 800 && ((Unlocked(AID.FlareStar) && Souls == 0) || !Unlocked(AID.FlareStar)))
                    AOEfiller(target, prio, strategy);
            }
        }
        if (!Unlocked(TraitID.UmbralHeart))
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
    }
    private void AOEfiller(Actor? target, GCDPriority prio, StrategyValues strategy)
    {
        if (HasThunderhead && (ThunderLeft < 15 || Polyglots == 0))
            QueueGCD(BestThunderAOE, strategy.AutoTarget() ? BestSplashTargets?.Actor : target, prio + 2);
        if (Polyglots > 0)
            QueueGCD(AID.Foul, strategy.AutoTarget() ? BestSplashTargets?.Actor : target, prio + 1);
        if (LastActionUsed(BestThunderAOE) || LastActionUsed(AID.Foul) ||
            (!HasThunderhead && Polyglots == 0)) //this will clip, but doing this raw is rare
            QueueOGCD(AID.Transpose, target, prio);
    }

    #region Upgrade Paths
    private AID BestThunderST => Unlocked(AID.HighThunder) ? AID.HighThunder : Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;
    private AID BestThunderAOE => Unlocked(AID.HighThunder2) ? AID.HighThunder2 : Unlocked(AID.Thunder4) ? AID.Thunder4 : Unlocked(AID.Thunder2) ? AID.Thunder2 : AID.Thunder1;
    private AID BestThunder => ShouldUseAOE ? BestThunderAOE : BestThunderST;
    private AID BestPolyglot => ShouldUseAOE ? AID.Foul : BestXenoglossy;
    private AID BestXenoglossy => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    private AID BestBlizzardAOE => Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : Unlocked(AID.Blizzard2) ? AID.Blizzard2 : AID.Blizzard1;
    private AID BestDespair => Unlocked(AID.Despair) ? AID.Despair : AID.Flare;
    #endregion

    #endregion

    #region Cooldown Helpers
    private bool HasInstants(StrategyValues strategy, Actor? target)
        => (HasEffect(SID.Swiftcast) || //has swiftcast
            HasEffect(SID.Triplecast) || //has triplecast
            ShouldUseF3P || //about to use F3P
            ShouldUseParadox || //about to use Paradox
            ShouldUseThunder(strategy, target).Item1 || //about to use Thunder
            (Unlocked(AID.Xenoglossy) && ShouldUsePolyglot(strategy, target).Item1) || //about to use instant Polyglots
            (Unlocked(AID.FlareStar) && InAF && Souls == 0 && MP is <= 1600 and >= 800)); //about to use instant Despair
    private bool IsNotMoving(StrategyValues strategy, Actor? target) => !IsMoving || HasInstants(strategy, target);
    private bool CastingCheck(StrategyValues strategy, Actor? target)
        => strategy.Option(Track.Casting).As<CastingOption>() == CastingOption.Allow ||
            (strategy.Option(Track.Casting).As<CastingOption>() == CastingOption.Forbid && IsNotMoving(strategy, target));
    private void UseTPUS(StrategyValues strategy, Actor? target)
    {
        var ok = (strategy.Option(Track.TPUS).As<TPUSStrategy>() == TPUSStrategy.Allow &&
            (!Player.InCombat || (Player.InCombat && Hints.NumPriorityTargetsInAOECircle(Player.Position, 35) == 0))) ||
            (strategy.Option(Track.TPUS).As<TPUSStrategy>() == TPUSStrategy.OOConly && !Player.InCombat);

        if (ok && ((strategy.AutoTarget() && BestSplashTarget?.Actor == null) || (strategy.ManualTarget() && target == null)))
        {
            if (Unlocked(AID.Transpose))
            {
                var fsPotential = Unlocked(AID.FlareStar) && (Souls == 6 || Souls >= 3 && MP >= 800);
                if (InAF && !fsPotential && MP < MaxMP)
                    QueueOGCD(AID.Transpose, Player, OGCDPriority.Max);
                if (InUI && HasFirestarter && MP == MaxMP)
                    QueueOGCD(AID.Transpose, Player, OGCDPriority.Max);
            }
            if (Unlocked(AID.UmbralSoul) &&
                InUI && (UI < 3 || Hearts < MaxHearts || MP < MaxMP))
                QueueGCD(AID.UmbralSoul, Player, GCDPriority.Minimal);
            return;
        }
    }
    private void UseOpener(StrategyValues strategy, Actor? target)
    {
        if (target == null)
            return;

        //F3 - best for opener or reopener if MP is good enough
        //B3 - best for recovering from death or if using F3 is not optimal
        if (NoUIorAF)
        {
            //Out of Combat - align with countdown end
            //In Combat - just send it
            if (CountdownRemaining is null or > 0 and < 4f)
                QueueGCD(AID.Fire3, target, GCDPriority.SlightlyHigh);
        }
    }
    private void UseMovement(StrategyValues strategy, Actor? target)
    {
        //if standing still or has instant casts, skip this entirely
        if (IsNotMoving(strategy, target))
            return;

        //we are thinking here as if we're inside instance & moving
        if (InsideCombatWith(target))
        {
            var m = strategy.Option(Track.Movement).As<MovementStrategy>();

            //we handle the OGCD usecases first to try and hold on to our GCD resources for as long as possible
            if (m is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyOGCDs && GCD == 0)
            {
                if (CanSwiftcast && !HasEffect(SID.Triplecast))
                    QueueOGCD(AID.Swiftcast, Player, OGCDPriority.Severe + 1);
                if (CanTC && !HasEffect(SID.Swiftcast))
                    QueueOGCD(AID.Triplecast, Player, OGCDPriority.Severe);
            }

            //our GCD usecase involves only Polyglots, Thunder, and Scathe since Paradox & F3P are primarily used for rotation
            if (m is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyGCDs)
            {
                var th = strategy.Option(Track.Thunder).As<ThunderStrategy>();
                var pg = strategy.Option(Track.Polyglot).As<PolyglotStrategy>();
                var wantSTth = strategy.ForceST() || th == ThunderStrategy.ForceST;
                var wantAOEth = ShouldUseAOE || th == ThunderStrategy.ForceAOE;
                var wantSTpg = strategy.ForceST() || pg is PolyglotStrategy.ForceXeno or PolyglotStrategy.XenoSpendAll or PolyglotStrategy.XenoHold1 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.XenoHold3;
                var wantAOEpg = ShouldUseAOE || pg is PolyglotStrategy.ForceFoul or PolyglotStrategy.FoulSpendAll or PolyglotStrategy.FoulHold1 or PolyglotStrategy.FoulHold2 or PolyglotStrategy.FoulHold3;

                if (Unlocked(TraitID.EnhancedPolyglot) && Polyglots > 0)
                    QueueGCD(wantSTpg ? BestXenoglossy : wantAOEpg ? AID.Foul : BestPolyglot,
                        AOETargetChoice(target, wantSTpg ? target : BestSplashTarget?.Actor, strategy.Option(Track.Polyglot), strategy), GCDPriority.Severe + 1);
                if (HasThunderhead)
                    QueueGCD(wantSTth ? BestThunderST : wantAOEth ? BestThunderAOE : BestThunder,
                        AOETargetChoice(target, wantSTpg ? target : BestSplashTarget?.Actor, strategy.Option(Track.Polyglot), strategy), GCDPriority.Severe);
            }
            if (m is MovementStrategy.Allow or MovementStrategy.OnlyScathe)
            {
                if (Unlocked(AID.Scathe) && MP >= 800)
                    QueueGCD(AID.Scathe, TargetChoice(strategy.Option(SharedTrack.AOE)) ?? target, GCDPriority.Low);
            }
        }
    }

    #region GCD
    #region DOT
    private static SID[] GetDotStatus() => [SID.Thunder, SID.ThunderII, SID.ThunderIII, SID.ThunderIV, SID.HighThunder, SID.HighThunderII];
    private float ThunderRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);
    private (bool, GCDPriority) ShouldUseThunder(StrategyValues strategy, Actor? target)
    {
        if (!HasThunderhead)
            return (false, GCDPriority.None);

        var condition = InsideCombatWith(target) && In25y(target);
        return strategy.Option(Track.Thunder).As<ThunderStrategy>() switch
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

    private (bool, GCDPriority) ShouldUsePolyglot(StrategyValues strategy, Actor? target)
    {
        if (!Unlocked(AID.Foul) || Polyglots == 0)
            return (false, GCDPriority.None);

        var veryclose = PolyglotTimer <= 4000f && HasMaxPolyglots;
        var overcap = PolyglotTimer <= 8000 && HasMaxPolyglots;
        var openMF = CombatTimer < 40 && CDRemaining(AID.Manafont) <= 1.5f && InAF && MP <= 400;
        var ezTPSC = !Unlocked(AID.FlareStar) && Unlocked(AID.Xenoglossy) && ((InAF && MP == 0) || LastActionUsed(AID.Despair)); //Lv80-L99 - for easy weaving of TP+SC
        var condition = Polyglots > 0 && InsideCombatWith(target) &&
                (overcap ||
                CanLL || //LL prep
                (Unlocked(AID.Xenoglossy) && InAF && (ShouldUseAOE ? HasParadox : !HasParadox) && MP < 800) || //MF prep
                (Unlocked(AID.Amplifier) && CDRemaining(AID.Amplifier) < 1f)); //Amp prep
        var prio = veryclose ? GCDPriority.ExtremelyHigh + 1 : GCDPriority.SlightlyHigh;
        return strategy.Option(Track.Polyglot).As<PolyglotStrategy>() switch
        {
            PolyglotStrategy.AutoSpendAll or PolyglotStrategy.XenoSpendAll or PolyglotStrategy.FoulSpendAll => ((condition || ezTPSC) && MaxPolyglots >= 1, prio),
            PolyglotStrategy.AutoHold1 or PolyglotStrategy.XenoHold1 or PolyglotStrategy.FoulHold1 => (openMF || ezTPSC || (condition && (MaxPolyglots >= 2 ? Polyglots > 1 : Polyglots > 0)), prio),
            PolyglotStrategy.AutoHold2 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.FoulHold2 => (openMF || ezTPSC || (condition && (MaxPolyglots == 3 ? Polyglots > 2 : MaxPolyglots >= 2 ? Polyglots > 1 : Polyglots > 0)), prio),
            PolyglotStrategy.AutoHold3 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.FoulHold3 => (openMF || ezTPSC || (HasMaxPolyglots && overcap), prio),
            PolyglotStrategy.ForceXeno => (CanXG, GCDPriority.Forced),
            PolyglotStrategy.ForceFoul => (CanFoul, GCDPriority.Forced),
            _ => (false, GCDPriority.None),
        };
    }
    private bool ShouldUseF3P => Unlocked(AID.Fire3) && //minimal
        !ShouldUseAOE && HasFirestarter && InAF && AF < 3; //grants AF3 after TP from UI
    private bool ShouldUseParadox => CanParadox && !ShouldUseAOE && //minimal
        ((InAF && !HasFirestarter) || //grant F3P in AF
        (InUI && Hearts == MaxHearts) || //transition back into AF
        (InAF && Souls == 0 && MP < 3200)); //after Manafont
    private bool ShouldUseEnder(StrategyValues strategy, Actor? target)
    {
        if (!Unlocked(AID.Flare) || !InAF || MP < 800)
            return false;

        var stFlare = Unlocked(AID.FlareStar) && ((Souls is >= 3 and < 6 && MP < 1600) || (DowntimeIn < 8 && Souls >= 3 && MP >= 800));
        var st = (MP is <= 1600 and >= 800) || stFlare;
        var aoe = (Unlocked(AID.FlareStar) ? (Souls != 6 && MP >= 800) : //Lv100 - frequently casting
            (Unlocked(TraitID.UmbralHeart) ? (Hearts > 0 || (Hearts == 0 && MP >= 800)) : //Lv58-Lv99 - frequently casting
            MP is < 3000 and >= 800)); //Lv50-Lv57 - as simple ender
        var condition = CastingCheck(strategy, target) && In25y(target) && (ShouldUseAOE ? aoe : st);

        return strategy.Option(Track.Ender).As<EnderStrategy>() switch
        {
            EnderStrategy.Automatic => condition,
            EnderStrategy.OnlyDespair => condition,
            EnderStrategy.OnlyFlare => condition,
            EnderStrategy.ForceDespair => Unlocked(AID.Despair) && MP >= 800,
            EnderStrategy.ForceFlare => Unlocked(AID.Flare) && MP >= 800,
            _ => false
        };
    }
    #endregion

    #region OGCD
    private bool ShouldUseTranspose()
    {
        if (!ActionReady(AID.Transpose))
            return false;

        //Ice
        //- transition into AF only if we can use Paradox
        //- if no Paradox, skip TP entirely and just rawdog into AF with 1/2 cast F3
        //Fire
        //- if we have ender, use TP after it
        //- if no ender, use TP after last F1
        return ((MaxUI && Unlocked(AID.Paradox)) ||
            (MaxAF && (HasEffect(SID.Triplecast) || HasEffect(SID.Swiftcast)) && //if no Instant buff, we skip TP
            (Unlocked(AID.Flare) ? (LastActionUsed(BestDespair) || (MP == 0 && Souls == 0)) : (LastActionUsed(AID.Fire1) && MP < 1600))));
    }
    private (bool, GCDPriority) ShouldUseManafont(UpgradeStrategy strategy, Actor? target)
    {
        if (!CanMF)
            return (false, GCDPriority.None);

        var condition = InsideCombatWith(target) && InAF && (ShouldUseAOE ? HasParadox : !HasParadox) && MP < 800;
        return strategy switch
        {
            UpgradeStrategy.Automatic => (condition, GCDPriority.ModeratelyLow + 4),
            UpgradeStrategy.Force or UpgradeStrategy.ForceEX => (CanMF, GCDPriority.Forced),
            UpgradeStrategy.ForceWeave or UpgradeStrategy.ForceWeaveEX => (CanMF && CanWeaveIn, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
    }
    private bool ShouldUseSwiftcast(UpgradeStrategy strategy, Actor? target)
    {
        if (!CanSwiftcast)
            return false;

        return strategy switch
        {
            UpgradeStrategy.Automatic => InsideCombatWith(target) && !HasEffect(SID.Swiftcast) && !HasEffect(SID.Triplecast) && InAF && (LastActionUsed(Unlocked(AID.Despair) ? AID.Despair : AID.Flare) || MP < 800),
            UpgradeStrategy.Force or UpgradeStrategy.ForceEX => true,
            UpgradeStrategy.ForceWeave or UpgradeStrategy.ForceWeaveEX => CanWeaveIn,
            _ => false
        };
    }
    private bool ShouldUseTriplecast(ChargeStrategy strategy, Actor? target)
    {
        if (!CanTC)
            return false;

        //primary use for this is to make sure we get instant B3
        if (strategy == ChargeStrategy.Automatic && InsideCombatWith(target) && !HasEffect(SID.Swiftcast) && !HasEffect(SID.Triplecast))
        {
            return InAF && CDRemaining(AID.Manafont) > 8f && //if Manafont is imminent, we dont want to early use
                ((!Unlocked(AID.FlareStar) && MP < 1600) || //Lv50-Lv99 - F4->Despair/Flare->B3
                (LastActionUsed(Unlocked(AID.Despair) ? AID.Despair : AID.Flare) || MP < 800)); //fallback attempt to use right before B3
        }

        return strategy switch
        {
            ChargeStrategy.Force => true,
            ChargeStrategy.Force1 => CDRemaining(AID.Triplecast) <= 5f,
            ChargeStrategy.ForceWeave => CanWeaveIn,
            ChargeStrategy.ForceWeave1 => CanWeaveIn && CDRemaining(AID.Triplecast) <= 5f,
            _ => false
        };
    }
    private (bool, GCDPriority) ShouldUseLeyLines(ChargeStrategy strategy, Actor? target)
    {
        if (!CanLL)
            return (false, GCDPriority.None);

        return strategy switch
        {
            ChargeStrategy.Automatic => (InsideCombatWith(target) && !IsMoving && (RaidBuffsLeft > 10f || RaidBuffsIn < 3000f), GCDPriority.ModeratelyLow + 1),
            ChargeStrategy.Force => (true, GCDPriority.Forced),
            ChargeStrategy.Force1 => (CDRemaining(AID.LeyLines) <= 5, GCDPriority.Forced),
            ChargeStrategy.ForceWeave => (CanWeaveIn, GCDPriority.Forced - 1500),
            ChargeStrategy.ForceWeave1 => (CanWeaveIn && CDRemaining(AID.LeyLines) <= 5, GCDPriority.Forced - 1500),
            _ => (false, GCDPriority.None)
        };
    }
    private bool ShouldUseAmplifier(OGCDStrategy strategy) => ShouldUseOGCD(strategy, Player, CanAmplify, (InUI || InAF) && !HasMaxPolyglots);
    private bool ShouldUseRetrace(OGCDStrategy strategy) => ShouldUseOGCD(strategy, Player, CanRetrace);
    private bool ShouldUseBTL(OGCDStrategy strategy) => ShouldUseOGCD(strategy, Player, CanBTL);
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
        MaxAspect = Unlocked(TraitID.AspectMastery3) ? 3 : Unlocked(TraitID.AspectMastery2) ? 2 : 1;
        NoUIorAF = ElementStance is 0 and not (1 or 2 or 3 or -1 or -2 or -3); //nothing
        InAF = ElementStance is 1 or 2 or 3 and not (0 or -1 or -2 or -3); //fire
        InUI = ElementStance is -1 or -2 or -3 and not (0 or 1 or 2 or 3); //ice
        AF = gauge.AstralStacks;
        MaxAF = AF == MaxAspect;
        Souls = gauge.AstralSoulStacks;
        MaxSouls = Unlocked(AID.FlareStar) ? 6 : 0;
        HasMaxSouls = Souls == MaxSouls;
        UI = gauge.UmbralStacks;
        Hearts = gauge.UmbralHearts;
        MaxHearts = Unlocked(TraitID.UmbralHeart) ? 3 : 0;
        HasMaxHearts = Hearts == MaxHearts;
        MaxUI = UI == MaxAspect && HasMaxHearts;
        Polyglots = gauge.PolyglotStacks;
        MaxPolyglots = Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : Unlocked(AID.Foul) ? 1 : 0;
        HasMaxPolyglots = Polyglots == MaxPolyglots;
        PolyglotTimer = gauge.EnochianTimer;
        HasParadox = gauge.ParadoxActive;
        CanFoul = Unlocked(AID.Foul) && Polyglots > 0;
        CanXG = Unlocked(AID.Xenoglossy) && Polyglots > 0;
        CanParadox = Unlocked(AID.Paradox) && HasParadox && MP >= 1600;
        CanLL = Unlocked(AID.LeyLines) && CDRemaining(AID.LeyLines) <= 120 && !HasEffect(SID.LeyLines);
        CanAmplify = ActionReady(AID.Amplifier);
        CanTC = Unlocked(AID.Triplecast) && CDRemaining(AID.Triplecast) <= 60 && !HasEffect(SID.Triplecast);
        CanMF = ActionReady(AID.Manafont) && InAF;
        CanRetrace = ActionReady(AID.Retrace) && HasEffect(SID.CircleOfPower);
        CanBTL = ActionReady(AID.BetweenTheLines) && HasEffect(SID.LeyLines);
        HasThunderhead = HasEffect(SID.Thunderhead);
        HasFirestarter = HasEffect(SID.Firestarter);
        ThunderLeft = Utils.MaxAll(
            StatusDetails(BestSplashTarget?.Actor, SID.Thunder, Player.InstanceID, 24).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderII, Player.InstanceID, 18).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderIII, Player.InstanceID, 27).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderIV, Player.InstanceID, 21).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.HighThunder, Player.InstanceID, 30).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.HighThunderII, Player.InstanceID, 24).Left);
        ShouldUseAOE = strategy.ForceAOE() || (Unlocked(AID.Blizzard2) && NumSplashTargets > 2);
        ShouldUseSTDOT = Unlocked(AID.Thunder1) && NumSplashTargets <= 2;
        ShouldUseAOEDOT = strategy.ForceAOE() || Unlocked(AID.Thunder2) && NumSplashTargets > 2;
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
        if (!strategy.HoldEverything())
        {
            if (!strategy.HoldAbilities())
            {
                #region Out of Combat
                UseOpener(strategy, primaryTarget?.Actor);
                UseTPUS(strategy, primaryTarget?.Actor);
                #endregion

                if (!strategy.HoldCDs())
                {
                    if (!strategy.HoldBuffs())
                    {
                        //Manafont
                        var (mfCondition, mfPrio) = ShouldUseManafont(mfStrat, primaryTarget?.Actor);
                        if (mfCondition)
                            QueueOGCD(AID.Manafont, Player, Unlocked(AID.Xenoglossy) ? mfPrio : mfPrio + 2004); //4254
                        //LeyLines
                        var (llCondition, llPrio) = ShouldUseLeyLines(llStrat, primaryTarget?.Actor);
                        if (llCondition)
                            QueueOGCD(AID.LeyLines, Player, Unlocked(AID.Xenoglossy) ? llPrio : llPrio + 2001); //4251
                        //Amplifier
                        if (ShouldUseAmplifier(ampStrat))
                            QueueOGCD(AID.Amplifier, Player, OGCDPrio(ampStrat, OGCDPriority.ModeratelyLow));
                    }
                    //Swiftcast
                    if (ShouldUseSwiftcast(strategy.Option(Track.Swiftcast).As<UpgradeStrategy>(), primaryTarget?.Actor))
                        QueueOGCD(AID.Swiftcast, Player, Unlocked(AID.Xenoglossy) ? GCDPriority.ModeratelyLow : GCDPriority.ModeratelyLow + 2003); //4253
                    //Triplecast
                    if (ShouldUseTriplecast(strategy.Option(Track.Triplecast).As<ChargeStrategy>(), primaryTarget?.Actor))
                        QueueOGCD(AID.Triplecast, Player, Unlocked(AID.Xenoglossy) ? GCDPriority.ModeratelyLow : GCDPriority.ModeratelyLow + 2002); //4252
                    //Transpose
                    if (ShouldUseTranspose())
                        QueueOGCD(AID.Transpose, Player, Unlocked(AID.Xenoglossy) ? OGCDPriority.ExtremelyHigh : OGCDPriority.ModeratelyLow + 2000);
                }
                if (!strategy.HoldGauge())
                {
                    //Thunder
                    var (tCondition, tPrio) = ShouldUseThunder(strategy, primaryTarget?.Actor);
                    if (tCondition)
                    {
                        var action = thunderStrat switch
                        {
                            ThunderStrategy.Allow3 or ThunderStrategy.Allow6 or ThunderStrategy.Allow9 or ThunderStrategy.AllowNoDOT or ThunderStrategy.ForceAny => BestThunder,
                            ThunderStrategy.ForceST => BestThunderST,
                            ThunderStrategy.ForceAOE => BestThunderAOE,
                            _ => AID.None
                        };
                        QueueGCD(action, AOETargetChoice(primaryTarget?.Actor, BestDOTTarget?.Actor, thunder, strategy), tPrio);
                    }
                    //F3P
                    if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseF3P)
                        QueueGCD(AID.Fire3, primaryTarget?.Actor, GCDPriority.Average);
                    //Paradox
                    if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseParadox)
                        QueueGCD(AID.Paradox, primaryTarget?.Actor, InUI ? GCDPriority.Average : GCDPriority.Average);

                    //Polyglots
                    var (pgCondition, pgPrio) = ShouldUsePolyglot(strategy, primaryTarget?.Actor);
                    if (pgCondition)
                    {
                        var action = polyglotStrat switch
                        {
                            PolyglotStrategy.AutoSpendAll or PolyglotStrategy.AutoHold1 or PolyglotStrategy.AutoHold2 or PolyglotStrategy.AutoHold3 => BestPolyglot,
                            PolyglotStrategy.XenoSpendAll or PolyglotStrategy.XenoHold1 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.ForceXeno => BestXenoglossy,
                            PolyglotStrategy.FoulSpendAll or PolyglotStrategy.FoulHold1 or PolyglotStrategy.FoulHold2 or PolyglotStrategy.FoulHold3 or PolyglotStrategy.ForceFoul => AID.Foul,
                            _ => AID.None
                        };
                        QueueGCD(action, AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, thunder, strategy), pgPrio);
                    }
                }

                UseMovement(strategy, primaryTarget?.Actor);
                //Potion   
                if (ShouldUsePotion(strategy))
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, ActionQueue.Priority.Medium);
                //Retrace
                if (ShouldUseRetrace(retraceStrat))
                    QueueOGCD(AID.Retrace, Player, OGCDPriority.Forced);
                //Between the Lines
                var zone = World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID);
                if (ShouldUseBTL(btlStrat))
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.BetweenTheLines), Player, (int)ActionQueue.Priority.Medium, targetPos: zone!.PosRot.XYZ());
            }

            if (CastingCheck(strategy, primaryTarget?.Actor))
            {
                //Rotation
                if (strategy.AutoFinish() || strategy.AutoBreak())
                {
                    if (ShouldUseAOE)
                        BestAOE(AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, AOE, strategy), GCDPriority.Low, strategy);
                    if (!ShouldUseAOE || strategy.ForceST())
                        BestST(strategy, SingleTargetChoice(primaryTarget?.Actor, AOE), GCDPriority.Low);
                }
                //Forced ST
                if (strategy.ForceST())
                    BestST(strategy, SingleTargetChoice(primaryTarget?.Actor, AOE), GCDPriority.Low);
                //Forced AOE
                if (strategy.ForceAOE())
                    BestAOE(AOETargetChoice(primaryTarget?.Actor, BestSplashTarget?.Actor, AOE, strategy), GCDPriority.Low, strategy);
                //FlareStar
                if (InsideCombatWith(primaryTarget?.Actor) && CastingCheck(strategy, primaryTarget?.Actor) && Unlocked(AID.FlareStar) && HasMaxSouls)
                    QueueGCD(AID.FlareStar, strategy.AutoTarget() ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.Average + 6);
                //Despair & Flare
                if (ShouldUseEnder(strategy, primaryTarget?.Actor))
                {
                    var stFlare = Unlocked(AID.FlareStar) && ((Souls is >= 3 and < 6 && MP < 1600) || (DowntimeIn < 8 && Souls >= 3 && MP >= 800));
                    var action = strategy.Option(Track.Ender).As<EnderStrategy>() switch
                    {
                        EnderStrategy.Automatic => ShouldUseAOE || stFlare ? AID.Flare : BestDespair,
                        EnderStrategy.OnlyFlare or EnderStrategy.ForceFlare => AID.Flare,
                        EnderStrategy.OnlyDespair or EnderStrategy.ForceDespair => BestDespair,
                        _ => AID.None
                    };
                    QueueGCD(action, strategy.AutoTarget() ? BestSplashTarget?.Actor : primaryTarget?.Actor, GCDPriority.Average);
                }
            }
        }

        #endregion

        #region AI
        GoalZoneSingle(25);
        #endregion
    }
}
