using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Thunder, Ender, Polyglot, Manafont, Triplecast, Swiftcast, LeyLines, TPUS, Movement, Casting, Amplifier, Retrace, BTL }
    public enum AOEStrategy { Automatic, ForceST, ForceAOE }
    public enum ThunderStrategy { Allow3, Allow6, Allow9, AllowNoDOT, ForceAny, ForceST, ForceAOE, Forbid }
    public enum EnderStrategy { Automatic, OnlyDespair, OnlyFlare, ForceDespair, ForceFlare }
    public enum PolyglotStrategy { AutoSpendAll, AutoHold1, AutoHold2, AutoHold3, XenoSpendAll, XenoHold1, XenoHold2, XenoHold3, FoulSpendAll, FoulHold1, FoulHold2, FoulHold3, ForceAny, ForceXeno, ForceFoul, Delay }
    public enum CommonStrategy { Automatic, Force, ForceWeave, Delay }
    public enum TPUSStrategy { Allow, OOConly, Forbid }
    public enum MovementStrategy { Allow, AllowNoScathe, OnlyGCDs, OnlyGCDsNoScathe, OnlyOGCDs, OnlyScathe, Forbid }
    public enum CastingOption { Forbid, Allow }
    public enum CommonOption { Forbid, Allow, AllowNoMoving }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi BLM", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Basic, BitMask.Build(Class.THM, Class.BLM), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionInt);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.Automatic, "Automatically use best rotation based on targets nearby")
            .AddOption(AOEStrategy.ForceST, "Force single-target rotation, regardless of targets nearby")
            .AddOption(AOEStrategy.ForceAOE, "Force AOE rotation, regardless of targets nearby")
            .AddAssociatedActions(
                AID.Fire1, AID.Fire3, AID.Fire4, AID.Despair, AID.FlareStar,
                AID.Fire2, AID.HighFire2, AID.Flare,
                AID.Blizzard1, AID.Blizzard3, AID.Blizzard4,
                AID.Blizzard2, AID.HighBlizzard2, AID.Freeze);

        res.Define(Track.Thunder).As<ThunderStrategy>("DOT", "Thunder", 200)
            .AddOption(ThunderStrategy.Allow3, "Allow the use Thunder if target Has 3s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow6, "Allow the use Thunder if target Has 6s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow9, "Allow the use Thunder if target Has 9s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.AllowNoDOT, "Allow the use Thunder only if target does not have DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAny, "Force best Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceST, "Force single-target Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAOE, "Force AOE Thunder regardless of DoT effect", 0, 24, ActionTargets.Hostile, 26)
            .AddOption(ThunderStrategy.Forbid, "Forbid Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(
                AID.Thunder1, AID.Thunder3, AID.HighThunder,
                AID.Thunder2, AID.Thunder4, AID.HighThunder2);

        res.Define(Track.Ender).As<EnderStrategy>("AF.Ender", "Despair / Flare", 194)
            .AddOption(EnderStrategy.Automatic, "Automatically end fire phase with Despair or Flare based on targets nearby", 0, 0, ActionTargets.Hostile, 50)
            .AddOption(EnderStrategy.OnlyDespair, "End fire phase with Despair only, regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(EnderStrategy.OnlyFlare, "End fire phase with Flare only, regardless of targets", 0, 0, ActionTargets.Hostile, 50)
            .AddOption(EnderStrategy.ForceDespair, "Force Despair if possible", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(EnderStrategy.ForceFlare, "Force Flare if possible", 0, 0, ActionTargets.Hostile, 50)
            .AddAssociatedActions(AID.Despair, AID.Flare);

        res.Define(Track.Polyglot).As<PolyglotStrategy>("Polyglot", "Xenoglossy / Foul", 197)
            .AddOption(PolyglotStrategy.AutoSpendAll, "Automatically select best polyglot based on targets - Spend all Polyglots as soon as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold1, "Automatically select best polyglot based on targets - holds 1 for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold2, "Automatically select best polyglot based on targets - holds 2 for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold3, "Automatically select best polyglot based on targets - holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.XenoSpendAll, "Use Xenoglossy as Polyglot spender, regardless of targets nearby - spends all Polyglots", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold1, "Use Xenoglossy as Polyglot spender, regardless of targets nearby - holds 1 Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold2, "Use Xenoglossy as Polyglot spender, regardless of targets nearby - holds 2 Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold3, "Use Xenoglossy as Polyglot spender - Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.FoulSpendAll, "Use Foul as Polyglot spender, regardless of targets nearby", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold1, "Use Foul as Polyglot spender, regardless of targets nearby - holds 1 Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold2, "Use Foul as Polyglot spender, regardless of targets nearby - holds 2 Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold3, "Use Foul as Polyglot spender - Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceAny, "Force best polyglot based on targets", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceXeno, "Force Xenoglossy", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.ForceFoul, "Force Foul", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.Delay, "Delay Polyglot abilities for manual or strategic usage", 0, 0, ActionTargets.Hostile, 70)
            .AddAssociatedActions(AID.Xenoglossy, AID.Foul);

        res.Define(Track.Manafont).As<CommonStrategy>("MF", "Manafont", 196)
            .AddOption(CommonStrategy.Automatic, "Automatically use Manafont", 0, 0, ActionTargets.Self, 30)
            .AddOption(CommonStrategy.Force, "Force Manafont ASAP", 100, 0, ActionTargets.Self, 30, 83)
            .AddOption(CommonStrategy.ForceWeave, "Force Manafont in next possible weave slot", 100, 0, ActionTargets.Self, 30, 83)
            .AddOption(CommonStrategy.Delay, "Delay Manafont", 0, 0, ActionTargets.Self, 30)
            .AddAssociatedActions(AID.Manafont);

        res.Define(Track.Triplecast).As<CommonStrategy>("3xCast", "Triplecast", 195)
            .AddOption(CommonStrategy.Automatic, "Automatically use Triplecast when any charges available to instant-cast Blizzard III after Fire Phase (e.g. Despair->Transpose->Triplecast->B3 etc.)", 0, 0, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.Force, "Force Triplecast ASAP", 0, 15, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.ForceWeave, "Force Triplecast in next possible weave slot", 0, 15, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.Delay, "Delay Triplecast", 0, 0, ActionTargets.Self, 66)
            .AddAssociatedActions(AID.Triplecast);

        res.Define(Track.Swiftcast).As<CommonStrategy>("Swift", "Swiftcast", 195)
            .AddOption(CommonStrategy.Automatic, "Automatically use Swiftcast when when available to instant-cast Blizzard III after Fire Phase (e.g. Despair->Transpose->Swiftcast->B3 etc.)", 0, 0, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.Force, "Force Swiftcast ASAP", 0, 10, ActionTargets.Self, 18, 93)
            .AddOption(CommonStrategy.ForceWeave, "Force Swiftcast in next possible weave slot", 0, 10, ActionTargets.Self, 18, 93)
            .AddOption(CommonStrategy.Delay, "Delay Swiftcast for strategic reasons", 0, 0, ActionTargets.Self, 18)
            .AddAssociatedActions(AID.Swiftcast);

        res.Define(Track.LeyLines).As<CommonStrategy>("LL", "Ley Lines", 197)
            .AddOption(CommonStrategy.Automatic, "Automatically use Ley Lines", 0, 0, ActionTargets.Self, 52)
            .AddOption(CommonStrategy.Force, "Force Ley Lines ASAP", 0, 20, ActionTargets.Self, 52)
            .AddOption(CommonStrategy.ForceWeave, "Force Ley Lines in next possible weave slot", 0, 20, ActionTargets.Self, 52)
            .AddOption(CommonStrategy.Delay, "Delay Ley Lines", 0, 0, ActionTargets.Self, 52)
            .AddAssociatedActions(AID.LeyLines);

        res.Define(Track.TPUS).As<TPUSStrategy>("TP/US", "Transpose & Umbral Soul", 198)
            .AddOption(TPUSStrategy.Allow, "Allow Transpose & Umbral Soul combo when either out of combat or no targetable enemy is nearby", minLevel: 35)
            .AddOption(TPUSStrategy.OOConly, "Only use Transpose & Umbral Soul combo when fully out of combat", minLevel: 35)
            .AddOption(TPUSStrategy.Forbid, "Forbid Transpose & Umbral Soul combo", minLevel: 35)
            .AddAssociatedActions(AID.Transpose, AID.UmbralSoul);

        res.Define(Track.Movement).As<MovementStrategy>("Move", "Movement Options", 199)
            .AddOption(MovementStrategy.Allow, "Allow all appropriate abilities for use while moving - Swiftcast->Triplecast->Polyglots->Thunder->Scathe if nothing left")
            .AddOption(MovementStrategy.AllowNoScathe, "Allow all appropriate abilities for use while moving except for Scathe - Swiftcast->Triplecast->Polyglots->Thunder->None")
            .AddOption(MovementStrategy.OnlyGCDs, "Only use instant cast GCDs for use while moving - Polyglots->Thunder->Scathe if nothing left")
            .AddOption(MovementStrategy.OnlyGCDsNoScathe, "Only use instant cast GCDs for use while moving except for Scathe - Polyglots->Thunder->None")
            .AddOption(MovementStrategy.OnlyOGCDs, "Only use OGCDs for use while moving - Swiftcast->Triplecast")
            .AddOption(MovementStrategy.OnlyScathe, "Only use Scathe while moving (not recommended)")
            .AddOption(MovementStrategy.Forbid, "Forbid any abilities for use while moving");

        res.Define(Track.Casting).As<CastingOption>("Cast", "Cast While Moving", 199)
            .AddOption(CastingOption.Forbid, "Forbid hard-casting while moving")
            .AddOption(CastingOption.Allow, "Allow hard-casting while moving");

        res.DefineOGCD(Track.Amplifier, AID.Amplifier, "Amplifier", "Amp.", 196, 120, 0, ActionTargets.Self, 86);

        res.Define(Track.Retrace).As<CommonOption>("Rt", "Retrace", 197)
            .AddOption(CommonOption.Forbid, "Forbid Retrace")
            .AddOption(CommonOption.Allow, "Allow Retrace")
            .AddOption(CommonOption.AllowNoMoving, "Allow Retrace only when not moving")
            .AddAssociatedActions(AID.Retrace);

        res.Define(Track.BTL).As<CommonOption>("BTL", "Between The Lines", 197)
            .AddOption(CommonOption.Forbid, "Forbid Between the Lines")
            .AddOption(CommonOption.Allow, "Allow Between the Lines")
            .AddOption(CommonOption.AllowNoMoving, "Allow Between the Lines only when not moving")
            .AddAssociatedActions(AID.BetweenTheLines);

        return res;
    }

    private static readonly SID[] ThunderStatuses = [SID.Thunder, SID.ThunderII, SID.ThunderIII, SID.ThunderIV, SID.HighThunder, SID.HighThunderII];

    private BlackMageGauge Gauge => World.Client.GetGauge<BlackMageGauge>();
    private int Element => Gauge.ElementStance;
    private bool InAF => Element > 0;
    private bool InUI => Element < 0;
    private int AF => Math.Max(0, Element);
    private int UI => Math.Max(0, -Element);
    private int MaxAspect => Unlocked(TraitID.AspectMastery3) ? 3 : Unlocked(TraitID.AspectMastery2) ? 2 : 1;
    private int Hearts => Gauge.UmbralHearts;
    private int MaxHearts => Unlocked(TraitID.UmbralHeart) ? 3 : 0;
    private int Souls => Gauge.AstralSoulStacks;
    private int Polyglots => Gauge.PolyglotStacks;
    private int MaxPolyglots => Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : Unlocked(AID.Foul) ? 1 : 0;
    private float NextPolyglotIn => Gauge.EnochianTimer * 0.001f;
    private bool HasParadox => Unlocked(AID.Paradox) && Gauge.ParadoxActive;
    private bool HasFirestarter => HasStatus(SID.Firestarter);
    private bool HasThunderhead => HasStatus(SID.Thunderhead);
    private bool HasTriplecast => HasStatus(SID.Triplecast);
    private bool HasInstantCast => HasSwiftcast || HasTriplecast;
    private bool CanTriplecast => Unlocked(AID.Triplecast) && ReadyIn(AID.Triplecast) <= 0.5f && !HasTriplecast;
    private bool FullMP => MP >= MaxMP * 0.96f;
    private bool FireReady => (MaxHearts == 0 || Hearts == MaxHearts) && FullMP;
    private int FireCost => Hearts > 0 ? 800 : 1600;
    private int DespairReserve => Unlocked(AID.Despair) ? 800 : 0;
    private bool HasInstantGCD
        => Polyglots > 0
        || HasInstantCast
        || HasThunderhead
        || HasFirestarter && InAF
        || HasParadox && (!InAF || MP >= 1600)
        || InAF && MP >= 800 && Unlocked(TraitID.EnhancedAstralFire);

    private AID BestThunderST => Unlocked(AID.HighThunder) ? AID.HighThunder : Unlocked(AID.Thunder3) ? AID.Thunder3 : AID.Thunder1;
    private AID BestThunderAOE => Unlocked(AID.HighThunder2) ? AID.HighThunder2 : Unlocked(AID.Thunder4) ? AID.Thunder4 : Unlocked(AID.Thunder2) ? AID.Thunder2 : AID.Thunder1;
    private AID BestXenoglossy => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    private AID BestFireAOE => Unlocked(AID.HighFire2) ? AID.HighFire2 : AID.Fire2;
    private AID BestBlizzardAOE => Unlocked(AID.HighBlizzard2) ? AID.HighBlizzard2 : Unlocked(AID.Blizzard2) ? AID.Blizzard2 : AID.Blizzard1;
    private AID BestBlizzardST => Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Blizzard1;

    private bool WantAOE;
    private bool CastWhileMoving;
    private bool ManafontSoon;
    private (Enemy? Best, int Targets) SplashTargets => GetBestTarget(PrimaryEnemy, 25, IsSplashTarget);
    private Actor? AOETarget(StrategyValues strategy, StrategyValues.OptionRef track) => AOETargetChoice(PrimaryTarget, SplashTargets.Best?.Actor, track, strategy);

    protected override float GetCastTime(AID aid)
    {
        var def = ActionDefinitions.Instance.Spell(aid);
        var instant = HasTriplecast
            || aid == AID.Despair && Unlocked(TraitID.EnhancedAstralFire)
            || aid == AID.Fire3 && HasFirestarter
            || aid == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            || def?.Aspect == ActionAspect.Thunder && HasThunderhead;
        var castTime = instant ? 0 : base.GetCastTime(aid);
        var halved = Element == -3 && def?.Aspect == ActionAspect.Fire || Element == 3 && def?.Aspect == ActionAspect.Ice;
        return halved ? castTime * 0.5f : castTime;
    }
    protected override int MPCost(AID aid)
    {
        int Fire(int cost) => InUI ? 0 : InAF && Hearts == 0 ? cost * 2 : cost;
        int Ice(int cost) => InUI || InAF ? 0 : cost;
        return aid switch
        {
            AID.Despair or AID.Flare or AID.Scathe => 800,
            AID.Paradox => InAF ? 1600 : 0,
            AID.Fire3 => HasFirestarter ? 0 : Fire(2000),
            AID.Fire1 or AID.Fire4 => Fire(800),
            AID.Fire2 or AID.HighFire2 => Fire(1500),
            AID.Blizzard1 => Ice(400),
            AID.Blizzard2 or AID.HighBlizzard2 or AID.Blizzard3 => Ice(800),
            _ => 0
        };
    }
    private float ThunderRemaining(Actor? target)
    {
        var left = target == null ? float.MaxValue : 0f;
        if (target != null)
            foreach (var status in ThunderStatuses)
                left = MathF.Max(left, StatusDetails(target, (uint)status, Player.InstanceID).Left);
        return left;
    }
    private void QueueCast(AID aid, Actor? target, GCDPriority priority)
    {
        var castTime = GetCastTime(aid);
        var prepull = CountdownRemaining is not { } countdown || countdown < castTime + 0.2f;
        if (aid != AID.None && target != null && HasMP(aid) && prepull && (CastWhileMoving || !IsMoving || castTime <= 0))
            QueueGCD(aid, target, priority);
    }
    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var aoe = strategy.Option(Track.AOE).As<AOEStrategy>();
        WantAOE = aoe == AOEStrategy.ForceAOE || aoe == AOEStrategy.Automatic && Unlocked(AID.Blizzard2) && SplashTargets.Targets >= 2;
        CastWhileMoving = strategy.Option(Track.Casting).As<CastingOption>() == CastingOption.Allow;
        ManafontSoon = InAF && strategy.Option(Track.Manafont).As<CommonStrategy>() != CommonStrategy.Delay && strategy.AllowBuffs() && Unlocked(AID.Manafont) && ReadyIn(AID.Manafont) < 1f;

        if (strategy.HoldEverything())
            return;

        if (strategy.AllowGauge())
        {
            QueueThunder(strategy);
            QueuePolyglot(strategy);
        }
        if (strategy.AllowBuffs())
        {
            QueueManafont(strategy);
            QueueLeyLines(strategy);
            QueueAmplifier(strategy);
        }
        if (strategy.AllowCDs())
            QueueForcedInstants(strategy);
        if (strategy.AllowAbilities())
        {
            QueueMovement(strategy);
            QueueRetrace(strategy);
            QueueBetweenTheLines(strategy);
            QueuePotion(strategy, ActionDefinitions.IDPotionInt, RaidBuffsIn <= 5 || RaidBuffsLeft > 0);
        }
        if (PrimaryTarget == null)
            QueueDowntime(strategy);
        else
            QueueRotation(strategy);

        GoalZoneSingle(25);
    }

    private void QueueDowntime(StrategyValues strategy)
    {
        var tpus = strategy.Option(Track.TPUS).As<TPUSStrategy>();
        var allowed = tpus == TPUSStrategy.Allow && (!Player.InCombat || Hints.NumPriorityTargetsInAOECircle(Player.Position, 35) == 0)
            || tpus == TPUSStrategy.OOConly && !Player.InCombat;
        if (!allowed)
            return;

        if (InAF && Souls < 6 && ActionReady(AID.Transpose))
            QueueOGCD(AID.Transpose, Player, OGCDPriority.Max);
        if (InUI && Unlocked(AID.UmbralSoul) && (UI < MaxAspect || Hearts < MaxHearts || !FullMP))
            QueueGCD(AID.UmbralSoul, Player, GCDPriority.Minimal);
    }
    private void QueueRotation(StrategyValues strategy)
    {
        var track = strategy.Option(Track.AOE);
        if (WantAOE)
            QueueAOE(strategy, AOETarget(strategy, track));
        else if (InAF)
            QueueFireST(strategy, SingleTargetChoice(PrimaryTarget, track));
        else if (InUI)
            QueueIceST(strategy, SingleTargetChoice(PrimaryTarget, track));
        else
            QueueNeutralST(strategy, SingleTargetChoice(PrimaryTarget, track));
    }
    private void QueueNeutralST(StrategyValues strategy, Actor? target)
    {
        if (FullMP && Unlocked(AID.Fire3))
            QueueCast(AID.Fire3, target, GCDPriority.ModeratelyHigh);
        else if (Unlocked(AID.Blizzard3))
        {
            QueueInstantForBlizzard(strategy);
            QueueCast(AID.Blizzard3, target, GCDPriority.ModeratelyHigh);
        }
        else
            QueueCast(FullMP ? AID.Fire1 : AID.Blizzard1, target, GCDPriority.ModeratelyHigh);
    }
    private void QueueFireST(StrategyValues strategy, Actor? target)
    {
        if (AF < MaxAspect && Unlocked(AID.Fire3))
            QueueCast(!HasFirestarter && HasParadox ? AID.Paradox : AID.Fire3, target, GCDPriority.ModeratelyHigh);
        if (Souls == 6)
            QueueCast(AID.FlareStar, target, GCDPriority.VeryHigh);
        if (AF == MaxAspect && HasParadox && !HasFirestarter && (MP <= 7600 || IsMoving))
            QueueCast(AID.Paradox, target, GCDPriority.SlightlyHigh);
        if (AF == MaxAspect && HasFirestarter && IsMoving)
            QueueCast(AID.Fire3, target, GCDPriority.Low + 1);

        var canFire = MP - FireCost >= DespairReserve && (!Unlocked(AID.FlareStar) || Souls < 6);
        if (canFire && (AF == MaxAspect || !Unlocked(AID.Fire3)))
            QueueCast(Unlocked(AID.Fire4) && AF == MaxAspect ? AID.Fire4 : AID.Fire1, target, GCDPriority.Average);

        QueueEnder(strategy, target);

        var depleted = MP < (Unlocked(AID.Flare) ? 800 : FireCost);
        if (depleted && Souls < 6 && !ManafontSoon)
            QueueSwapToIce(strategy, target);
    }
    private void QueueEnder(StrategyValues strategy, Actor? target)
    {
        var track = strategy.Option(Track.Ender);
        var ender = track.As<EnderStrategy>();
        var enderTarget = SingleTargetChoice(target, track);
        var finishing = MP >= 800 && MP - FireCost < DespairReserve;
        var soulsShort = Unlocked(AID.FlareStar) && Souls is >= 3 and < 6 && MP >= 800 && MP < (6 - Souls) * FireCost && !ManafontSoon;
        var automatic = WantAOE ? AID.Flare : Unlocked(AID.Despair) ? AID.Despair : MP < 2400 ? AID.Flare : AID.None;

        var (condition, action, priority) = ender switch
        {
            EnderStrategy.Automatic => (finishing || soulsShort, soulsShort ? AID.Flare : automatic, soulsShort ? GCDPriority.SlightlyHigh : GCDPriority.Average),
            EnderStrategy.OnlyDespair => (finishing, Unlocked(AID.Despair) ? AID.Despair : AID.Flare, GCDPriority.Average),
            EnderStrategy.OnlyFlare => (finishing || soulsShort, AID.Flare, GCDPriority.Average),
            EnderStrategy.ForceDespair => (MP >= 800, AID.Despair, GCDPriority.Forced),
            _ => (MP >= 800, AID.Flare, GCDPriority.Forced)
        };
        if (condition && InAF)
            QueueCast(action, enderTarget, priority);
    }
    private void QueueSwapToIce(StrategyValues strategy, Actor? target)
    {
        var sc = strategy.Option(Track.Swiftcast).As<CommonStrategy>();
        var tc = strategy.Option(Track.Triplecast).As<CommonStrategy>();
        var instantReady = HasInstantCast || sc == CommonStrategy.Automatic && CanSwiftcast || tc == CommonStrategy.Automatic && CanTriplecast;
        var transpose = ActionReady(AID.Transpose) && (instantReady || !Unlocked(AID.Blizzard3));
        if (transpose)
            QueueOGCD(AID.Transpose, Player, OGCDPriority.VeryHigh);
        else
            QueueCast(Unlocked(AID.Blizzard3) ? AID.Blizzard3 : AID.Blizzard1, target, GCDPriority.ModeratelyHigh);
    }
    private void QueueIceST(StrategyValues strategy, Actor? target)
    {
        if (UI < MaxAspect && Unlocked(AID.Blizzard3))
        {
            QueueInstantForBlizzard(strategy);
            QueueCast(AID.Blizzard3, target, GCDPriority.ModeratelyHigh);
        }
        if ((UI == MaxAspect || !Unlocked(AID.Blizzard3)) && !FireReady)
            QueueCast(BestBlizzardST, target, GCDPriority.Average);
        if (FireReady && HasFirestarter && ActionReady(AID.Transpose))
            QueueOGCD(AID.Transpose, Player, OGCDPriority.VeryHigh);
        else if (FireReady && Unlocked(AID.Fire3))
            QueueCast(AID.Fire3, target, GCDPriority.ModeratelyHigh);
        else if (FireReady && ActionReady(AID.Transpose))
            QueueOGCD(AID.Transpose, Player, OGCDPriority.VeryHigh);
    }
    private void QueueInstantForBlizzard(StrategyValues strategy)
    {
        var sc = strategy.Option(Track.Swiftcast).As<CommonStrategy>();
        var tc = strategy.Option(Track.Triplecast).As<CommonStrategy>();
        if (!HasInstantCast && Player.InCombat && sc == CommonStrategy.Automatic && CanSwiftcast)
            QueueOGCD(AID.Swiftcast, Player, OGCDPriority.ToGCDPriority + 560);
        else if (!HasInstantCast && Player.InCombat && tc == CommonStrategy.Automatic && CanTriplecast)
            QueueOGCD(AID.Triplecast, Player, OGCDPriority.ToGCDPriority + 560);
    }
    private void QueueAOE(StrategyValues strategy, Actor? target)
    {
        var threeOrMore = SplashTargets.Targets >= 3 || strategy.Option(Track.AOE).As<AOEStrategy>() == AOEStrategy.ForceAOE;
        if (InAF && MaxHearts > 0)
            QueueFireAOE(strategy, target);
        else if (InUI && MaxHearts > 0)
            QueueIceAOE(target, threeOrMore);
        else if (InAF)
            QueueFireAOELowLevel(target);
        else if (InUI)
            QueueIceAOELowLevel(target);
        else
            QueueCast(MaxHearts == 0 && FullMP && Unlocked(AID.Fire2) ? BestFireAOE : BestBlizzardAOE, target, GCDPriority.ModeratelyHigh);
    }
    private void QueueFireAOE(StrategyValues strategy, Actor? target)
    {
        if (Souls == 6)
            QueueCast(AID.FlareStar, target, GCDPriority.VeryHigh);
        if (MP >= 800 && Unlocked(AID.Flare) && strategy.Option(Track.Ender).As<EnderStrategy>() != EnderStrategy.OnlyDespair)
            QueueCast(AID.Flare, target, GCDPriority.Average);
        if (MP >= 800 && strategy.Option(Track.Ender).As<EnderStrategy>() == EnderStrategy.OnlyDespair)
            QueueCast(AID.Despair, target, GCDPriority.Average);
        if (MP < 800 && Souls < 6 && !ManafontSoon && ActionReady(AID.Transpose))
        {
            QueueFiller(target);
            QueueOGCD(AID.Transpose, Player, OGCDPriority.VeryHigh);
        }
    }
    private void QueueIceAOE(Actor? target, bool threeOrMore)
    {
        if (UI < MaxAspect && !Unlocked(AID.Freeze))
            QueueCast(BestBlizzardAOE, target, GCDPriority.ModeratelyHigh);
        if (Hearts < MaxHearts)
            QueueCast(threeOrMore && Unlocked(AID.Freeze) ? AID.Freeze : BestBlizzardST, threeOrMore ? target : PrimaryTarget, GCDPriority.Average);
        if (Hearts == MaxHearts && ActionReady(AID.Transpose))
        {
            QueueFiller(target);
            QueueOGCD(AID.Transpose, Player, OGCDPriority.VeryHigh);
        }
    }
    private void QueueFiller(Actor? target)
    {
        if (Polyglots > 0)
            QueueCast(AID.Foul, target, GCDPriority.SlightlyHigh);
        if (HasThunderhead && ThunderRemaining(target) < 10)
            QueueCast(BestThunderAOE, target, GCDPriority.SlightlyHigh - 1);
        if (InUI && HasParadox)
            QueueCast(AID.Paradox, PrimaryTarget, GCDPriority.SlightlyHigh - 2);
    }
    private void QueueFireAOELowLevel(Actor? target)
    {
        if (Unlocked(AID.Fire2) && MP >= 3000)
            QueueCast(BestFireAOE, target, GCDPriority.Average);
        if (Unlocked(AID.Flare) && MP is >= 800 and < 3000)
            QueueCast(AID.Flare, target, GCDPriority.Average);
        if (MP < (Unlocked(AID.Flare) ? 800 : 1500))
            QueueCast(BestBlizzardAOE, target, GCDPriority.ModeratelyHigh);
    }
    private void QueueIceAOELowLevel(Actor? target)
    {
        if (FullMP && Unlocked(AID.Fire2))
            QueueCast(BestFireAOE, target, GCDPriority.ModeratelyHigh);
        else if (UI == MaxAspect && Unlocked(AID.Freeze))
            QueueCast(AID.Freeze, target, GCDPriority.Average);
        else
            QueueCast(BestBlizzardAOE, target, GCDPriority.Average);
    }
    private void QueueThunder(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Thunder);
        var thunder = track.As<ThunderStrategy>();
        if (thunder == ThunderStrategy.Forbid || !HasThunderhead || PrimaryTarget == null)
            return;

        var aoe = thunder == ThunderStrategy.ForceAOE || thunder != ThunderStrategy.ForceST && WantAOE && Unlocked(AID.Thunder2);
        var target = ResolveTarget(track.Value) ?? (aoe ? SplashTargets.Best?.Actor : GetDOTTarget(PrimaryEnemy, ThunderRemaining, 2).Best?.Actor) ?? PrimaryTarget;
        var left = ThunderRemaining(target);
        var ticksBeforeDowntime = DowntimeIn > 20 && WorthDot(target, 12);
        var (condition, priority) = thunder switch
        {
            ThunderStrategy.Allow3 => (left <= 3 && ticksBeforeDowntime, GCDPriority.High),
            ThunderStrategy.Allow6 => (left <= 6 && ticksBeforeDowntime, GCDPriority.High),
            ThunderStrategy.Allow9 => (left <= 9 && ticksBeforeDowntime, GCDPriority.High),
            ThunderStrategy.AllowNoDOT => (left == 0, GCDPriority.High),
            _ => (true, GCDPriority.Forced)
        };
        if (condition && InCombat(target) && In25y(target))
            QueueGCD(aoe ? BestThunderAOE : BestThunderST, target, priority);
    }
    private void QueuePolyglot(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Polyglot);
        var polyglot = track.As<PolyglotStrategy>();
        if (polyglot == PolyglotStrategy.Delay || Polyglots == 0 || !Unlocked(AID.Foul))
            return;

        var auto = WantAOE ? AID.Foul : BestXenoglossy;
        var action = polyglot switch
        {
            PolyglotStrategy.XenoSpendAll or PolyglotStrategy.XenoHold1 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.ForceXeno => BestXenoglossy,
            PolyglotStrategy.FoulSpendAll or PolyglotStrategy.FoulHold1 or PolyglotStrategy.FoulHold2 or PolyglotStrategy.FoulHold3 or PolyglotStrategy.ForceFoul => AID.Foul,
            _ => auto
        };
        var target = action == AID.Foul ? AOETarget(strategy, track) : SingleTargetChoice(PrimaryTarget, track);
        var overcap = Polyglots >= MaxPolyglots && NextPolyglotIn <= 5;
        var amplifierSoon = Polyglots >= MaxPolyglots && Unlocked(AID.Amplifier) && ReadyIn(AID.Amplifier) <= GCD + 2.5f;
        var spendWindow = RaidBuffsLeft > GCD || DowntimeIn < 5 || amplifierSoon;
        var keep = polyglot switch
        {
            PolyglotStrategy.AutoHold1 or PolyglotStrategy.XenoHold1 or PolyglotStrategy.FoulHold1 => 1,
            PolyglotStrategy.AutoHold2 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.FoulHold2 => 2,
            PolyglotStrategy.AutoHold3 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.FoulHold3 => MaxPolyglots,
            _ => 0
        };

        var (condition, priority) = polyglot switch
        {
            PolyglotStrategy.ForceAny or PolyglotStrategy.ForceXeno or PolyglotStrategy.ForceFoul => (true, GCDPriority.Forced),
            _ when overcap => (true, GCDPriority.ExtremelyHigh),
            _ => (Polyglots > keep && (keep == 0 || spendWindow), GCDPriority.AboveAverage)
        };
        if (condition && InCombat(target))
            QueueCast(action, target, priority);
    }
    private void QueueManafont(StrategyValues strategy)
    {
        var (condition, priority) = strategy.Option(Track.Manafont).As<CommonStrategy>() switch
        {
            CommonStrategy.Automatic => (InCombat(PrimaryTarget) && InAF && MP < 800 && Souls < 6, OGCDPriority.High),
            CommonStrategy.Force => (true, OGCDPriority.Forced),
            CommonStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ActionReady(AID.Manafont))
            QueueOGCD(AID.Manafont, Player, priority);
    }
    private void QueueLeyLines(StrategyValues strategy)
    {
        var (condition, priority) = strategy.Option(Track.LeyLines).As<CommonStrategy>() switch
        {
            CommonStrategy.Automatic => (InCombat(PrimaryTarget) && !IsMoving && DowntimeIn > 20, OGCDPriority.Average),
            CommonStrategy.Force => (true, OGCDPriority.Forced),
            CommonStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (condition && !HasStatus(SID.LeyLines) && Unlocked(AID.LeyLines) && ReadyIn(AID.LeyLines) <= 0.5f)
            QueueOGCD(AID.LeyLines, Player, priority);
    }
    private void QueueAmplifier(StrategyValues strategy)
    {
        var amplifier = strategy.Option(Track.Amplifier).As<OGCDStrategy>();
        if (ShouldUseOGCD(amplifier, PrimaryTarget, ActionReady(AID.Amplifier), InCombat(PrimaryTarget) && (InAF || InUI) && Polyglots < MaxPolyglots))
            QueueOGCD(AID.Amplifier, Player, OGCDPrio(amplifier, OGCDPriority.Average - 1));
    }
    private void QueueForcedInstants(StrategyValues strategy)
    {
        var (swiftcast, swiftcastPriority) = strategy.Option(Track.Swiftcast).As<CommonStrategy>() switch
        {
            CommonStrategy.Force => (true, OGCDPriority.Forced),
            CommonStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (swiftcast && CanSwiftcast && !HasSwiftcast)
            QueueOGCD(AID.Swiftcast, Player, swiftcastPriority);

        var (triplecast, triplecastPriority) = strategy.Option(Track.Triplecast).As<CommonStrategy>() switch
        {
            CommonStrategy.Force => (true, OGCDPriority.Forced),
            CommonStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Forced),
            _ => (false, OGCDPriority.None)
        };
        if (triplecast && CanTriplecast)
            QueueOGCD(AID.Triplecast, Player, triplecastPriority);
    }

    private void QueueMovement(StrategyValues strategy)
    {
        var movement = strategy.Option(Track.Movement).As<MovementStrategy>();
        if (!IsMoving || CastWhileMoving || !InCombat(PrimaryTarget) || movement == MovementStrategy.Forbid)
            return;

        var ogcds = movement is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyOGCDs;
        var gcds = movement is MovementStrategy.Allow or MovementStrategy.AllowNoScathe or MovementStrategy.OnlyGCDs or MovementStrategy.OnlyGCDsNoScathe;
        var scathe = movement is MovementStrategy.Allow or MovementStrategy.OnlyGCDs or MovementStrategy.OnlyScathe;

        if (gcds && Polyglots > 0 && Unlocked(AID.Foul))
            QueueCast(WantAOE ? AID.Foul : BestXenoglossy, WantAOE ? SplashTargets.Best?.Actor ?? PrimaryTarget : PrimaryTarget, GCDPriority.Severe + 1);
        if (gcds && HasThunderhead)
            QueueGCD(WantAOE ? BestThunderAOE : BestThunderST, PrimaryTarget, GCDPriority.Severe);
        if (ogcds && !HasInstantGCD && CanSwiftcast)
            QueueOGCD(AID.Swiftcast, Player, OGCDPriority.Severe + 1);
        else if (ogcds && !HasInstantGCD && CanTriplecast)
            QueueOGCD(AID.Triplecast, Player, OGCDPriority.Severe);
        if (scathe && Unlocked(AID.Scathe))
            QueueCast(AID.Scathe, PrimaryTarget, GCDPriority.Low);
    }
    private static bool AllowedNow(CommonOption option, bool moving) => option switch
    {
        CommonOption.Allow => true,
        CommonOption.AllowNoMoving => !moving,
        _ => false
    };

    private void QueueRetrace(StrategyValues strategy)
    {
        if (AllowedNow(strategy.Option(Track.Retrace).As<CommonOption>(), IsMoving) && HasStatus(SID.LeyLines) && !HasStatus(SID.CircleOfPower) && ActionReady(AID.Retrace))
            QueueOGCD(AID.Retrace, Player, OGCDPriority.Forced);
    }
    private void QueueBetweenTheLines(StrategyValues strategy)
    {
        var allowed = AllowedNow(strategy.Option(Track.BTL).As<CommonOption>(), IsMoving) && HasStatus(SID.LeyLines) && !HasStatus(SID.CircleOfPower) && ActionReady(AID.BetweenTheLines);
        var circle = allowed ? FindLeyLines() : null;
        if (circle != null)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.BetweenTheLines), Player, ActionQueue.Priority.Medium, targetPos: circle.PosRot.XYZ());
    }
    private Actor? FindLeyLines()
    {
        foreach (var actor in World.Actors)
            if (actor.OID == 0x179 && actor.OwnerID == Player.InstanceID)
                return actor;
        return null;
    }
}
