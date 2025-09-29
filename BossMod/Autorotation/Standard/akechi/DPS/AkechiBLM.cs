using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { Thunder = SharedTrack.Count, Ender, Polyglot, Manafont, Triplecast, Swiftcast, LeyLines, TPUS, Movement, Casting, Amplifier, Retrace, BTL }
    public enum ThunderStrategy { Allow3, Allow6, Allow9, AllowNoDOT, ForceAny, ForceST, ForceAOE, Forbid }
    public enum EnderStrategy { Automatic, OnlyDespair, OnlyFlare, ForceDespair, ForceFlare }
    public enum PolyglotStrategy { AutoSpendAll, AutoHold1, AutoHold2, AutoHold3, XenoSpendAll, XenoHold1, XenoHold2, XenoHold3, FoulSpendAll, FoulHold1, FoulHold2, FoulHold3, ForceAny, ForceXeno, ForceFoul, Delay }
    public enum UpgradeStrategy { Automatic, Force, ForceWeave, ForceEX, ForceWeaveEX, Delay }
    public enum ChargeStrategy { Automatic, Force, Force1, ForceWeave, ForceWeave1, Delay }
    public enum TPUSStrategy { Allow, OOConly, Forbid }
    public enum MovementStrategy { Allow, AllowNoScathe, OnlyGCDs, OnlyOGCDs, OnlyScathe, Forbid }
    public enum CastingOption { Allow, Forbid }
    public enum CommonOption { Forbid, Allow, AllowNoMoving }

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

        res.Define(Track.Thunder).As<ThunderStrategy>("DOT", "Thunder", 200)
            .AddOption(ThunderStrategy.Allow3, "Allow3", "Allow the use Thunder if target Has 3s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow6, "Allow6", "Allow the use Thunder if target Has 6s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow9, "Allow9", "Allow the use Thunder if target Has 9s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.AllowNoDOT, "AllowNoDOT", "Allow the use Thunder only if target does not have DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAny, "ForceAny", "Force use of best Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceST, "ForceST", "Force use of single-target Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAOE, "ForceAOE", "Force use of AOE Thunder regardless of DoT effect", 0, 24, ActionTargets.Hostile, 26)
            .AddOption(ThunderStrategy.Forbid, "Forbid", "Forbid the use of Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(AID.Thunder1, AID.Thunder2, AID.Thunder3, AID.Thunder4, AID.HighThunder, AID.HighThunder2);

        res.Define(Track.Ender).As<EnderStrategy>("AF.Ender", "Despair / Flare", 194)
            .AddOption(EnderStrategy.Automatic, "Auto", "Automatically end fire phase with Despair or Flare based on targets nearby", 0, 0, ActionTargets.Hostile, 50)
            .AddOption(EnderStrategy.OnlyDespair, "OnlyDespair", "End fire phase with Despair only, regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(EnderStrategy.OnlyFlare, "OnlyFlare", "End fire phase with Flare only, regardless of targets", 0, 0, ActionTargets.Hostile, 50)
            .AddOption(EnderStrategy.ForceDespair, "ForceDespair", "Force the use of Despair if possible", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(EnderStrategy.ForceFlare, "ForceFlare", "Force the use of Flare if possible", 0, 0, ActionTargets.Hostile, 50)
            .AddAssociatedActions(AID.Despair, AID.Flare);

        res.Define(Track.Polyglot).As<PolyglotStrategy>("Polyglot", "Xenoglossy / Foul", 197)
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
            .AddOption(PolyglotStrategy.ForceAny, "Force Polyglot", "Force use of best polyglot based on targets", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceXeno, "Force Xenoglossy", "Force use of Xenoglossy", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.ForceFoul, "Force Foul", "Force use of Foul", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.Delay, "Delay", "Delay the use of Polyglot abilities for manual or strategic usage", 0, 0, ActionTargets.Hostile, 70)
            .AddAssociatedActions(AID.Xenoglossy, AID.Foul);

        res.Define(Track.Manafont).As<UpgradeStrategy>("MF", "Manafont", 196)
            .AddOption(UpgradeStrategy.Automatic, "Auto", "Automatically use Manafont optimally", 0, 0, ActionTargets.Self, 30)
            .AddOption(UpgradeStrategy.Force, "Force", "Force the use of Manafont (180s cooldown), regardless of weaving conditions", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(UpgradeStrategy.ForceWeave, "ForceWeave", "Force the use of Manafont (180s cooldown) in any next possible weave slot", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(UpgradeStrategy.ForceEX, "ForceEX", "Force the use of Manafont (100s cooldown), regardless of weaving conditions", 100, 0, ActionTargets.Self, 84)
            .AddOption(UpgradeStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Manafont (100s cooldown) in any next possible weave slot", 100, 0, ActionTargets.Self, 84)
            .AddOption(UpgradeStrategy.Delay, "Delay", "Delay the use of Manafont for strategic reasons", 0, 0, ActionTargets.Self, 30)
            .AddAssociatedActions(AID.Manafont);

        res.Define(Track.Triplecast).As<ChargeStrategy>("3xCast", "Triplecast", 195)
            .AddOption(ChargeStrategy.Automatic, "Auto", "Use any charges available to maintain swift rotation by instant-casting Blizzard III after Fire Phase (e.g. Despair->Transpose->Triplecast->B3 etc.)", 0, 0, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.Force, "Force", "Force the use of Triplecast; uses all charges", 0, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.Force1, "Force1", "Force the use of Triplecast; holds one charge for manual usage", 0, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.ForceWeave, "ForceWeave", "Force the use of Triplecast in any next possible weave slot", 0, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.ForceWeave1, "ForceWeave1", "Force the use of Triplecast in any next possible weave slot; holds one charge for manual usage", 0, 15, ActionTargets.Self, 66)
            .AddOption(ChargeStrategy.Delay, "Delay", "Delay the use of Triplecast", 0, 0, ActionTargets.Self, 66)
            .AddAssociatedActions(AID.Triplecast);

        res.Define(Track.Swiftcast).As<UpgradeStrategy>("Swift", "Swiftcast", 195)
            .AddOption(UpgradeStrategy.Automatic, "Auto", "Use Swiftcast to maintain swift rotation by instant-casting Blizzard III after Fire Phase (e.g. Despair->Transpose->Swiftcast->B3 etc.)", 0, 0, ActionTargets.Self, 66)
            .AddOption(UpgradeStrategy.Force, "Force", "Force the use of Swiftcast (60s cooldown), regardless of weaving conditions", 0, 10, ActionTargets.Self, 18, 93)
            .AddOption(UpgradeStrategy.ForceWeave, "ForceWeave", "Force the use of Swiftcast (60s cooldown) in any next possible weave slot", 0, 10, ActionTargets.Self, 18, 93)
            .AddOption(UpgradeStrategy.ForceEX, "ForceEX", "Force the use of Swiftcast (40s cooldown), regardless of weaving conditions", 0, 10, ActionTargets.Self, 94)
            .AddOption(UpgradeStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Swiftcast (40s cooldown) in any next possible weave slot", 0, 10, ActionTargets.Self, 94)
            .AddOption(UpgradeStrategy.Delay, "Delay", "Delay the use of Swiftcast for strategic reasons", 0, 0, ActionTargets.Self, 18)
            .AddAssociatedActions(AID.Swiftcast);

        res.Define(Track.LeyLines).As<ChargeStrategy>("LL", "Ley Lines", 197)
            .AddOption(ChargeStrategy.Automatic, "Auto", "Automatically decide when to use Ley Lines", 0, 0, ActionTargets.Self, 52)
            .AddOption(ChargeStrategy.Force, "Force", "Force the use of Ley Lines, regardless of weaving conditions", 0, 20, ActionTargets.Self, 52)
            .AddOption(ChargeStrategy.Force1, "Force1", "Force the use of Ley Lines; holds one charge for manual usage", 0, 20, ActionTargets.Self, 52)
            .AddOption(ChargeStrategy.ForceWeave, "ForceWeave", "Force the use of Ley Lines in any next possible weave slot", 0, 20, ActionTargets.Self, 52)
            .AddOption(ChargeStrategy.ForceWeave1, "ForceWeave1", "Force the use of Ley Lines in any next possible weave slot; holds one charge for manual usage", 0, 20, ActionTargets.Self, 52)
            .AddOption(ChargeStrategy.Delay, "Delay", "Delay the use of Ley Lines", 0, 0, ActionTargets.Self, 52)
            .AddAssociatedActions(AID.LeyLines);

        res.Define(Track.TPUS).As<TPUSStrategy>("TP/US", "Transpose & Umbral Soul", 198)
            .AddOption(TPUSStrategy.Allow, "Allow", "Allow Transpose & Umbral Soul combo when either out of combat or no targetable enemy is nearby", minLevel: 35)
            .AddOption(TPUSStrategy.OOConly, "OOConly", "Only use Transpose & Umbral Soul combo when fully out of combat", minLevel: 35)
            .AddOption(TPUSStrategy.Forbid, "Forbid", "Forbid Transpose & Umbral Soul combo", minLevel: 35)
            .AddAssociatedActions(AID.Transpose, AID.UmbralSoul);

        res.Define(Track.Movement).As<MovementStrategy>("Move", "Movement Options", 199)
            .AddOption(MovementStrategy.Allow, "Allow", "Allow the use of all appropriate abilities for use while moving")
            .AddOption(MovementStrategy.AllowNoScathe, "AllowNoScathe", "Allow the use of all appropriate abilities for use while moving except for Scathe")
            .AddOption(MovementStrategy.OnlyGCDs, "OnlyGCDs", "Only use instant cast GCDs for use while moving; Polyglots->Thunder->Scathe if nothing left")
            .AddOption(MovementStrategy.OnlyOGCDs, "OnlyOGCDs", "Only use OGCDs for use while moving; Swiftcast->Triplecast (NOTE: This will most likely cause you to enter UI3 without Transpose if moving frequently)")
            .AddOption(MovementStrategy.OnlyScathe, "OnlyScathe", "Only use Scathe for use while moving")
            .AddOption(MovementStrategy.Forbid, "Forbid", "Forbid the use of any abilities for use while moving");

        res.Define(Track.Casting).As<CastingOption>("Cast", "Cast While Moving", 199)
            .AddOption(CastingOption.Allow, "Allow", "Allow casting while moving")
            .AddOption(CastingOption.Forbid, "Forbid", "Forbid casting while moving");
        res.DefineOGCD(Track.Amplifier, AID.Amplifier, "Amplifier", "Amp.", 196, 120, 0, ActionTargets.Self, 86);

        res.Define(Track.Retrace).As<CommonOption>("Rt", "Retrace", 197)
            .AddOption(CommonOption.Forbid, "Forbid", "Forbid the use of Retrace")
            .AddOption(CommonOption.Allow, "Allow", "Allow the use of Retrace")
            .AddOption(CommonOption.AllowNoMoving, "Allow No Moving", "Allow the use of Retrace only when not moving")
            .AddAssociatedActions(AID.Retrace);

        res.Define(Track.BTL).As<CommonOption>("BTL", "Between The Lines", 197)
            .AddOption(CommonOption.Forbid, "Forbid", "Forbid the use of Between the Lines")
            .AddOption(CommonOption.Allow, "Allow", "Allow the use of Between the Lines")
            .AddOption(CommonOption.AllowNoMoving, "Allow No Moving", "Allow the use of Between the Lines only when not moving")
            .AddAssociatedActions(AID.BetweenTheLines);

        return res;
    }

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

    private AID BestThunderST => Unlocked(AID.HighThunder) ? AID.HighThunder : Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;
    private AID BestThunderAOE => Unlocked(AID.HighThunder2) ? AID.HighThunder2 : Unlocked(AID.Thunder4) ? AID.Thunder4 : Unlocked(AID.Thunder2) ? AID.Thunder2 : AID.Thunder1;
    private AID BestThunder => ShouldUseAOE ? BestThunderAOE : BestThunderST;
    private AID BestPolyglot => ShouldUseAOE ? AID.Foul : BestXenoglossy;
    private AID BestXenoglossy => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    private AID BestBlizzardAOE => Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : Unlocked(AID.Blizzard2) ? AID.Blizzard2 : AID.Blizzard1;
    private AID BestDespair => Unlocked(AID.Despair) ? AID.Despair : AID.Flare;

    private bool HasInstants(StrategyValues strategy, Actor? target) => HasSwiftcast || HasEffect(SID.Triplecast) || (Unlocked(AID.Fire3) && HasFirestarter && ((InAF && AF < 3) || (InUI && MaxUI))) || WantParadox || WantThunder(strategy, target).Condition || WantPolyglot(strategy, target).Condition || (Unlocked(AID.Despair) && Unlocked(AID.FlareStar) && InAF && MP < 1600 && Souls == 0);
    private static SID[] ThunderStatus() => [SID.Thunder, SID.ThunderII, SID.ThunderIII, SID.ThunderIV, SID.HighThunder, SID.HighThunderII];
    private float ThunderRemaining(Actor? target) => target == null ? float.MaxValue : ThunderStatus().Select(status => StatusDetails(target, (uint)status, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);
    private bool WantParadox => CanParadox && !ShouldUseAOE && ((InAF && !HasFirestarter) || (InUI && Hearts == MaxHearts) || (InAF && Souls == 0 && MP < 3200));
    private (bool Condition, AID Action, GCDPriority Priority) WantThunder(StrategyValues strategy, Actor? target)
    {
        var th = strategy.Option(Track.Thunder);
        var thStrat = th.As<ThunderStrategy>();
        var thTarget = AOETargetChoice(target, BestDOTTarget?.Actor, th, strategy);
        if (thTarget == null || !HasThunderhead || !In25y(thTarget))
            return (false, AID.None, GCDPriority.None);

        return thStrat switch
        {
            ThunderStrategy.Allow3 => (Player.InCombat && ThunderLeft <= 3, BestThunder, GCDPriority.High),
            ThunderStrategy.Allow6 => (Player.InCombat && ThunderLeft <= 6, BestThunder, GCDPriority.High),
            ThunderStrategy.Allow9 => (Player.InCombat && ThunderLeft <= 9, BestThunder, GCDPriority.High),
            ThunderStrategy.AllowNoDOT => (Player.InCombat && ThunderLeft == 0, BestThunder, GCDPriority.ExtremelyHigh),
            ThunderStrategy.ForceAny => (true, BestThunder, GCDPriority.Forced),
            ThunderStrategy.ForceST => (true, BestThunderST, GCDPriority.Forced),
            ThunderStrategy.ForceAOE => (true, BestThunderAOE, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
    }
    private (bool Condition, AID Action, GCDPriority Priority) WantPolyglot(StrategyValues strategy, Actor? target)
    {
        if (!Unlocked(AID.Foul) || Polyglots == 0)
            return (false, AID.None, GCDPriority.None);

        var pg = strategy.Option(Track.Polyglot);
        var pgStrat = pg.As<PolyglotStrategy>();
        var veryclose = PolyglotTimer <= 4000f && HasMaxPolyglots;
        var overcap = PolyglotTimer <= 8000 && HasMaxPolyglots;
        var openMF = CombatTimer < 40 && CDRemaining(AID.Manafont) <= 1.5f && InAF && MP <= 400;
        var ezTPSC = !Unlocked(AID.FlareStar) && Unlocked(AID.Xenoglossy) && ((InAF && MP == 0) || LastActionUsed(AID.Despair)); //Lv80-L99 - for easy weaving of TP+SC
        var condition = Polyglots > 0 && InsideCombatWith(target) && ((Unlocked(AID.Xenoglossy) && InAF &&
                (ShouldUseAOE ? HasParadox : !HasParadox) && MP < 800) || //MF prep
                (Unlocked(AID.Amplifier) && CDRemaining(AID.Amplifier) < 1f)); //Amp prep
        var prio = veryclose ? GCDPriority.ExtremelyHigh + 1 : GCDPriority.SlightlyHigh;
        var spendAll = (condition || ezTPSC) && MaxPolyglots >= 1;
        var hold1 = openMF || ezTPSC || (condition && (MaxPolyglots >= 2 ? Polyglots > 1 : Polyglots > 0));
        var hold2 = openMF || ezTPSC || (condition && (MaxPolyglots == 3 ? Polyglots > 2 : MaxPolyglots >= 2 ? Polyglots > 1 : Polyglots > 0));
        var hold3 = openMF || ezTPSC || (HasMaxPolyglots && overcap);
        return pgStrat switch
        {
            PolyglotStrategy.AutoSpendAll => (spendAll, BestPolyglot, prio),
            PolyglotStrategy.XenoSpendAll => (spendAll, BestXenoglossy, prio),
            PolyglotStrategy.FoulSpendAll => (spendAll, AID.Foul, prio),
            PolyglotStrategy.AutoHold1 => (hold1, BestPolyglot, prio),
            PolyglotStrategy.XenoHold1 => (hold1, BestXenoglossy, prio),
            PolyglotStrategy.FoulHold1 => (hold1, AID.Foul, prio),
            PolyglotStrategy.AutoHold2 => (hold2, BestPolyglot, prio),
            PolyglotStrategy.XenoHold2 => (hold2, BestXenoglossy, prio),
            PolyglotStrategy.FoulHold2 => (hold2, AID.Foul, prio),
            PolyglotStrategy.AutoHold3 => (hold3, BestPolyglot, prio),
            PolyglotStrategy.XenoHold3 => (hold3, BestXenoglossy, prio),
            PolyglotStrategy.FoulHold3 => (hold3, AID.Foul, prio),
            PolyglotStrategy.ForceAny => (CanFoul, BestPolyglot, GCDPriority.Forced),
            PolyglotStrategy.ForceXeno => (CanXG, BestXenoglossy, GCDPriority.Forced),
            PolyglotStrategy.ForceFoul => (CanFoul, AID.Foul, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None),
        };
    }

    private void ST(StrategyValues strategy, Actor? target, GCDPriority prio)
    {
        if (ShouldUseAOE)
            return;

        if (Player.InCombat && NoUIorAF && target != null)
        {
            if (CanSwiftcast)
                QueueGCD(AID.Swiftcast, target, prio + 1);

            QueueGCD(AID.Blizzard3, target, prio);
        }

        if (InAF)
        {
            if (MaxAF && (Hearts > 0 ? MP >= 800 : MP >= 1600) && (!Unlocked(AID.FlareStar) || Souls != 6))
                QueueGCD(Unlocked(AID.Fire4) ? AID.Fire4 : AID.Fire1, target, StatusRemaining(Player, SID.Triplecast) is <= 4.5f and not 0 ? prio + 500 : prio);

            var lowMP = MP < 800;
            if (Unlocked(AID.Blizzard3) &&
                ((Unlocked(AID.Flare) ? (Souls == 0 && (MP == 0 || LastActionUsed(AID.Flare) || LastActionUsed(AID.Despair))) : MP < 1600) ||
                Unlocked(AID.FlareStar) ? lowMP && Souls != 6 : lowMP))
                QueueGCD(AID.Blizzard3, target, prio);

            //if we end up in a situation where we can't use FlareStar but we can end, we will use Flare to save rotation
            if (Unlocked(AID.FlareStar) && MP is < 2400 and > 800 && Souls is < 6 and >= 3)
                QueueGCD(AID.Flare, target, prio + 1);
        }
        if (InUI)
        {
            if (LastActionUsed(AID.Blizzard3) || (Unlocked(TraitID.UmbralHeart) ? (UI == 3 && !HasMaxHearts) : MP != MaxMP))
                QueueGCD(Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Blizzard1, target, prio);

            //if we so happen to not have UI3 on swap, then we B3 for it
            if (Unlocked(AID.Blizzard3) && UI != 3)
                QueueGCD(AID.Blizzard3, target, prio);

            //below Lv90 - we do not have access to Paradox, so we have to rawdog F3 after B4
            if (!HasParadox && Unlocked(AID.Fire3) && MaxUI)
                QueueGCD(AID.Fire3, target, prio);
        }
    }
    private void AOE(StrategyValues strategy, Actor? target, GCDPriority prio)
    {
        if (!ShouldUseAOE)
            return;

        var aoeTarget = strategy.AutoTarget() ? BestSplashTargets?.Actor : target;
        if (Player.InCombat && NoUIorAF && target != null)
        {
            if (CanSwiftcast)
                QueueGCD(AID.Swiftcast, aoeTarget, prio);

            QueueGCD(BestBlizzardAOE, aoeTarget, prio);
        }

        if (Unlocked(TraitID.UmbralHeart))
        {
            if (InUI)
            {
                if (!HasMaxHearts)
                    QueueGCD(AID.Freeze, aoeTarget, prio);

                if (HasMaxHearts)
                {
                    if (ActionReady(AID.Transpose))
                        QueueGCD(AID.Transpose, Player, prio);

                    if (HasThunderhead && (ThunderLeft < 15 || Polyglots == 0))
                        QueueGCD(BestThunderAOE, aoeTarget, prio + 2);

                    if (Polyglots > 0)
                        QueueGCD(AID.Foul, aoeTarget, prio + 1);
                }
            }
            if (InAF)
            {
                if (MP >= 800)
                    QueueGCD(AID.Flare, aoeTarget, prio);

                if (MP < 800 && (!Unlocked(AID.FlareStar) || Souls < 6))
                {
                    if (ActionReady(AID.Transpose))
                        QueueGCD(AID.Transpose, aoeTarget, prio);

                    if (HasThunderhead && (ThunderLeft < 15 || Polyglots == 0))
                        QueueGCD(BestThunderAOE, aoeTarget, prio + 2);

                    if (Polyglots > 0)
                        QueueGCD(AID.Foul, aoeTarget, prio + 1);
                }
            }
        }
        if (!Unlocked(TraitID.UmbralHeart))
        {
            if (InAF)
            {
                if (Unlocked(AID.Fire2) && ((InUI && (MP == 10000 && UI == 3)) || (InAF && (Unlocked(TraitID.UmbralHeart) ? MP > 5500 : MP >= 3000))))
                    QueueGCD(Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2, target, prio);

                if (Unlocked(AID.Flare) && MP < 3000 && MP >= 800)
                    QueueGCD(AID.Flare, target, prio);
            }

            if (InUI)
            {
                if (Unlocked(AID.Blizzard2) && ((InUI && UI < 3) || (InAF && MP < 800)))
                    QueueGCD(BestBlizzardAOE, target, prio);

                if (!LastActionUsed(AID.Freeze) && UI == 3 && MP < 10000)
                    QueueGCD(Unlocked(AID.Freeze) ? AID.Freeze : BestBlizzardAOE, target, prio);
            }
        }
    }

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var gauge = World.Client.GetGauge<BlackMageGauge>();
        ElementStance = gauge.ElementStance;
        MaxAspect = Unlocked(TraitID.AspectMastery3) ? 3 : Unlocked(TraitID.AspectMastery2) ? 2 : 1;
        NoUIorAF = ElementStance is 0 and not (1 or 2 or 3 or -1 or -2 or -3); //nothing
        InAF = ElementStance is (1 or 2 or 3) and not (0 or -1 or -2 or -3); //fire
        InUI = ElementStance is (-1 or -2 or -3) and not (0 or 1 or 2 or 3); //ice
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
        CanRetrace = ActionReady(AID.Retrace) && HasEffect(SID.LeyLines) && !HasEffect(SID.CircleOfPower);
        CanBTL = ActionReady(AID.BetweenTheLines) && HasEffect(SID.LeyLines) && !HasEffect(SID.CircleOfPower);
        HasThunderhead = HasEffect(SID.Thunderhead);
        HasFirestarter = Unlocked(TraitID.Firestarter) && HasEffect(SID.Firestarter);
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
        var mainTarget = primaryTarget?.Actor;

        if (strategy.HoldEverything())
            return;

        GoalZoneSingle(25);

        var casting = strategy.Option(Track.Casting).As<CastingOption>();
        var castingOk = casting == CastingOption.Allow || (casting != CastingOption.Forbid || (!IsMoving || HasInstants(strategy, mainTarget)));
        if (!strategy.HoldAbilities())
        {
            if (mainTarget != null && NoUIorAF && CountdownRemaining is null or > 0 and < 4f)
                QueueGCD(ShouldUseAOE ? AID.HighBlizzard2 : MP < 8000 ? AID.Blizzard3 : AID.Fire3, mainTarget, GCDPriority.SlightlyHigh);

            var tpusStrat = strategy.Option(Track.TPUS).As<TPUSStrategy>();
            var tpusOk = (tpusStrat == TPUSStrategy.Allow &&
                (!Player.InCombat || (Player.InCombat && Hints.NumPriorityTargetsInAOECircle(Player.Position, 35) == 0))) ||
                (tpusStrat == TPUSStrategy.OOConly && !Player.InCombat);
            if (tpusOk && ((strategy.AutoTarget() && BestSplashTarget?.Actor == null) || (strategy.ManualTarget() && mainTarget == null)))
            {
                if (MaxUI && HasThunderhead)
                    return;

                if (ActionReady(AID.Transpose))
                {
                    var fsPotential = Unlocked(AID.FlareStar) && (Souls == 6 || (Souls >= 3 && MP >= 800));
                    var canFS = (Souls == 6 || (Souls >= 3 && MP >= 800));
                    if ((InAF && Unlocked(AID.FlareStar) ? !canFS : MP < MaxMP) || (InUI && !HasThunderhead))
                        QueueOGCD(AID.Transpose, Player, OGCDPriority.Max);
                }
                if (Unlocked(AID.UmbralSoul) &&
                    InUI && (UI < 3 || Hearts < MaxHearts || MP < MaxMP))
                    QueueGCD(AID.UmbralSoul, Player, GCDPriority.Minimal);
            }

            if (!strategy.HoldCDs())
            {
                var decreasePrio = Unlocked(AID.FlareStar) || HasInstants(strategy, mainTarget);
                if (!strategy.HoldBuffs())
                {
                    var mf = strategy.Option(Track.Manafont);
                    var mfStrat = mf.As<UpgradeStrategy>();
                    var mfMinimum = ActionReady(AID.Manafont) && InAF;
                    var (mfCondition, mfPrio) = mfStrat switch
                    {
                        UpgradeStrategy.Automatic => (InsideCombatWith(mainTarget) && InAF && (ShouldUseAOE ? HasParadox : !HasParadox) && MP < 800, GCDPriority.ModeratelyLow + 4),
                        UpgradeStrategy.Force or UpgradeStrategy.ForceEX => (CanMF, GCDPriority.Forced),
                        UpgradeStrategy.ForceWeave or UpgradeStrategy.ForceWeaveEX => (CanMF && CanWeaveIn, GCDPriority.Forced),
                        _ => (false, GCDPriority.None)
                    };
                    if (mfCondition)
                        QueueGCD(AID.Manafont, Player, mfPrio + 2004);

                    var ll = strategy.Option(Track.LeyLines);
                    var llStrat = ll.As<ChargeStrategy>();
                    var (llCondition, llPrio) = llStrat switch
                    {
                        ChargeStrategy.Automatic => (InsideCombatWith(mainTarget) && !IsMoving && (RaidBuffsLeft > 10f || RaidBuffsIn < 100), OGCDPriority.ModeratelyLow + 1),
                        ChargeStrategy.Force => (true, OGCDPriority.Forced + 1500),
                        ChargeStrategy.Force1 => (CDRemaining(AID.LeyLines) <= 5, OGCDPriority.Forced + 1500),
                        ChargeStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
                        ChargeStrategy.ForceWeave1 => (CanWeaveIn && CDRemaining(AID.LeyLines) <= 5, OGCDPriority.Forced),
                        _ => (false, OGCDPriority.None)
                    };
                    if (CanLL && llCondition)
                        QueueOGCD(AID.LeyLines, Player, decreasePrio ? llPrio : llPrio + 2001);

                    var amp = strategy.Option(Track.Amplifier);
                    var ampStrat = amp.As<OGCDStrategy>();
                    if (ShouldUseOGCD(ampStrat, Player, CanAmplify, (InUI || InAF) && !HasMaxPolyglots))
                        QueueOGCD(AID.Amplifier, Player, OGCDPrio(ampStrat, OGCDPriority.ModeratelyLow));
                }

                var sc = strategy.Option(Track.Swiftcast);
                var scStrat = sc.As<UpgradeStrategy>();
                var (scCondition, scPrio) = scStrat switch
                {
                    UpgradeStrategy.Automatic => (InsideCombatWith(mainTarget) && !HasEffect(SID.Swiftcast) && !HasEffect(SID.Triplecast) && MP == 0 && Souls == 0, GCDPriority.ModeratelyLow + 3),
                    UpgradeStrategy.Force or UpgradeStrategy.ForceEX => (true, GCDPriority.Forced + 1500),
                    UpgradeStrategy.ForceWeave or UpgradeStrategy.ForceWeaveEX => (CanWeaveIn, GCDPriority.Forced),
                    _ => (false, GCDPriority.None)

                };
                if (CanSwiftcast && scCondition)
                    QueueOGCD(AID.Swiftcast, Player, decreasePrio ? scPrio : GCDPriority.Average + 2003);

                var tc = strategy.Option(Track.Triplecast);
                var tcStrat = tc.As<ChargeStrategy>();
                var (tcCondition, tcPrio) = tcStrat switch
                {
                    ChargeStrategy.Automatic => (InsideCombatWith(mainTarget) && !HasEffect(SID.Swiftcast) && !HasEffect(SID.Triplecast) && InAF && MP is <= 1600, GCDPriority.ModeratelyLow + 2),
                    ChargeStrategy.Force => (true, GCDPriority.Forced + 1500),
                    ChargeStrategy.Force1 => (CDRemaining(AID.Triplecast) <= 5f, GCDPriority.Forced + 1500),
                    ChargeStrategy.ForceWeave => (CanWeaveIn, GCDPriority.Forced),
                    ChargeStrategy.ForceWeave1 => (CanWeaveIn && CDRemaining(AID.Triplecast) <= 5f, GCDPriority.Forced),
                    _ => (false, GCDPriority.None)

                };
                if (CanTC && tcCondition)
                    QueueOGCD(AID.Triplecast, Player, decreasePrio ? tcPrio : GCDPriority.Average + 2001);

                if (ActionReady(AID.Transpose) &&
                    ((MaxUI && Unlocked(AID.Paradox) && HasFirestarter && !HasParadox) ||
                    (MaxAF && (HasEffect(SID.Triplecast) || HasEffect(SID.Swiftcast)) &&
                    (Unlocked(AID.Flare) ? (LastActionUsed(BestDespair) || (MP == 0 && Souls == 0)) : (LastActionUsed(AID.Fire1) && MP < 1600)))))
                    QueueOGCD(AID.Transpose, Player, decreasePrio ? OGCDPriority.ExtremelyHigh : OGCDPriority.ModeratelyLow + 2000);
            }
            if (!strategy.HoldGauge())
            {
                var th = strategy.Option(Track.Thunder);
                var thStrat = th.As<ThunderStrategy>();
                var thTarget = AOETargetChoice(mainTarget, BestDOTTarget?.Actor, th, strategy);
                var thMinimum = InsideCombatWith(thTarget) && In25y(thTarget) && HasThunderhead;
                var (thCondition, thAction, thPrio) = WantThunder(strategy, thTarget);
                if (thMinimum && thCondition)
                    QueueGCD(thAction, thTarget, thPrio);

                if (Unlocked(AID.Fire3) && InsideCombatWith(mainTarget) && !ShouldUseAOE &&
                    (HasFirestarter ? ((InAF && AF < 3) || (InUI && MaxUI)) : castingOk && InUI && MaxUI))
                    QueueGCD(AID.Fire3, mainTarget, GCDPriority.Average);

                if (InsideCombatWith(mainTarget) && WantParadox)
                    QueueGCD(AID.Paradox, mainTarget, InUI ? GCDPriority.Average + 1 : GCDPriority.Average);

                var pgTarget = AOETargetChoice(mainTarget, BestSplashTarget?.Actor, strategy.Option(Track.Polyglot), strategy);
                var (pgCondition, pgAction, pgPrio) = WantPolyglot(strategy, pgTarget);
                if (pgCondition)
                    QueueGCD(pgAction, pgTarget, pgPrio);
            }

            if (!(IsMoving || !HasInstants(strategy, mainTarget)) && InsideCombatWith(mainTarget))
            {
                var m = strategy.Option(Track.Movement).As<MovementStrategy>();

                if (m is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyOGCDs)
                {
                    if (CanSwiftcast && !HasEffect(SID.Triplecast))
                        QueueOGCD(AID.Swiftcast, Player, OGCDPriority.Severe + 1);

                    if (CanTC && !HasEffect(SID.Swiftcast))
                        QueueOGCD(AID.Triplecast, Player, OGCDPriority.Severe);
                }

                if (m is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyGCDs)
                {
                    var th = strategy.Option(Track.Thunder);
                    var thStrat = th.As<ThunderStrategy>();
                    var wantSTth = strategy.ForceST() || thStrat == ThunderStrategy.ForceST;
                    var wantAOEth = ShouldUseAOE || thStrat == ThunderStrategy.ForceAOE;
                    var thTarget = AOETargetChoice(mainTarget, BestDOTTarget?.Actor, strategy.Option(Track.Thunder), strategy);
                    if (HasThunderhead)
                        QueueGCD(wantSTth ? BestThunderST : wantAOEth ? BestThunderAOE : BestThunder, thTarget, GCDPriority.Severe);

                    var pg = strategy.Option(Track.Polyglot);
                    var pgStrat = pg.As<PolyglotStrategy>();
                    var wantSTpg = strategy.ForceST() || pgStrat is PolyglotStrategy.ForceXeno or PolyglotStrategy.XenoSpendAll or PolyglotStrategy.XenoHold1 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.XenoHold3;
                    var wantAOEpg = ShouldUseAOE || pgStrat is PolyglotStrategy.ForceFoul or PolyglotStrategy.FoulSpendAll or PolyglotStrategy.FoulHold1 or PolyglotStrategy.FoulHold2 or PolyglotStrategy.FoulHold3;
                    var pgTarget = AOETargetChoice(mainTarget, wantSTpg ? mainTarget : BestSplashTarget?.Actor, strategy.Option(Track.Polyglot), strategy);
                    if (Unlocked(TraitID.EnhancedPolyglot) && Polyglots > 0)
                        QueueGCD(wantSTpg ? BestXenoglossy : wantAOEpg ? AID.Foul : BestPolyglot, pgTarget, GCDPriority.Severe + 1);
                }

                if (m is MovementStrategy.Allow or MovementStrategy.OnlyScathe)
                    if (Unlocked(AID.Scathe) && MP >= 800)
                        QueueGCD(AID.Scathe, TargetChoice(strategy.Option(SharedTrack.AOE)) ?? mainTarget, GCDPriority.Low);
            }

            if (strategy.Potion() switch
            {
                PotionStrategy.AlignWithBuffs or PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
                PotionStrategy.Immediate => true,
                _ => false
            })
                ExecutePotINT();

            if (CanRetrace && strategy.Option(Track.Retrace).As<CommonOption>() switch
            {
                CommonOption.Allow => true,
                CommonOption.AllowNoMoving => !IsMoving,
                _ => false
            })
                QueueOGCD(AID.Retrace, Player, OGCDPriority.Forced);

            var zone = World.Actors.FirstOrDefault(x => x.OID == 0x179 && x.OwnerID == Player.InstanceID);
            if (CanBTL && strategy.Option(Track.BTL).As<CommonOption>() switch
            {
                CommonOption.Allow => true,
                CommonOption.AllowNoMoving => !IsMoving,
                _ => false
            })
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.BetweenTheLines), Player, (int)ActionQueue.Priority.Medium, targetPos: zone!.PosRot.XYZ());
        }

        if (castingOk)
        {
            var fsTarget = strategy.AutoTarget() ? BestSplashTarget?.Actor : mainTarget;
            if (InsideCombatWith(fsTarget) && Unlocked(AID.FlareStar) && HasMaxSouls)
                QueueGCD(AID.FlareStar, fsTarget, GCDPriority.Average + 6);

            var e = strategy.Option(Track.Ender);
            var eStrat = e.As<EnderStrategy>();
            var fMinimum = Unlocked(AID.Flare) && MP >= 800 && InAF;
            var dMinimum = Unlocked(AID.Despair) && MP >= 800 && InAF;
            var fST = !Unlocked(AID.FlareStar) ? Unlocked(AID.Flare) && !Unlocked(AID.Despair) && MP is < 2400 and >= 800 && InAF :
                      ((Souls is >= 3 and < 6 && MP is < 2400 and >= 800 && InAF) || (DowntimeIn < 8 && Souls >= 3 && MP >= 800));
            var eTarget = AOETargetChoice(mainTarget, BestSplashTarget?.Actor, e, strategy);
            var eST = MP is <= 1600 and >= 800 || fST;
            var eAOE = Unlocked(AID.FlareStar) ? (Souls != 6 && MP >= 800) : (Unlocked(TraitID.UmbralHeart) ? (Hearts > 0 || (Hearts == 0 && MP >= 800)) : MP is < 2400 and >= 800);
            var eMinimum = fMinimum && In25y(eTarget) && (ShouldUseAOE ? eAOE : eST);
            var (eCondition, eAction, ePrio) = eStrat switch
            {
                EnderStrategy.Automatic => (eMinimum, fST ? AID.Flare : AID.Despair, GCDPriority.Average),
                EnderStrategy.OnlyDespair => (fMinimum, BestDespair, GCDPriority.Average),
                EnderStrategy.OnlyFlare => (fMinimum, AID.Flare, GCDPriority.Average),
                EnderStrategy.ForceDespair => (dMinimum, BestDespair, GCDPriority.Average),
                EnderStrategy.ForceFlare => (fMinimum, AID.Flare, GCDPriority.Average),
                _ => (false, AID.None, GCDPriority.None)
            };
            if (eCondition)
                QueueGCD(eAction, eTarget, ePrio);

            var aoePrio = StatusRemaining(Player, SID.Triplecast) is < 5f and > 0f ? GCDPriority.Average : GCDPriority.Low;
            var aoe = strategy.Option(SharedTrack.AOE);
            var aoeTarget = AOETargetChoice(mainTarget, BestSplashTarget?.Actor, aoe, strategy);
            if (strategy.AutoFinish() || strategy.AutoBreak())
            {
                if (ShouldUseAOE)
                    AOE(strategy, aoeTarget, aoePrio + 201);
                else
                    ST(strategy, aoeTarget, aoePrio);
            }

            if (strategy.ForceST())
                ST(strategy, mainTarget, aoePrio);

            if (strategy.ForceAOE())
                AOE(strategy, aoeTarget, aoePrio);
        }
    }
}
