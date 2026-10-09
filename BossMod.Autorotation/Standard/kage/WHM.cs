using BossMod.WHM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageWHM(RotationModuleManager manager, Actor player) : KageRotation<KageWHM.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Dia", MinLevel = 4, Actions = [AID.Dia, AID.AeroII, AID.Aero])]
        public Track<DotStrategy> Dot;

        [Track("Presence of Mind", MinLevel = 30, Action = AID.PresenceOfMind)]
        public Track<OffensiveStrategy> PresenceOfMind;

        [Track("Glare IV", MinLevel = 92, Action = AID.GlareIV)]
        public Track<OffensiveStrategy> GlareIV;

        [Track("Assize", MinLevel = 56, Action = AID.Assize)]
        public Track<OffensiveStrategy> Assize;

        [Track("Afflatus Misery", MinLevel = 74, Action = AID.AfflatusMisery)]
        public Track<MiseryStrategy> Misery;

        [Track("Lilies (overcap / downtime / cleave)", MinLevel = 52, Actions = [AID.AfflatusRapture, AID.AfflatusSolace])]
        public Track<OffensiveStrategy> Lilies;

        [Track("Lucid Dreaming", MinLevel = 14, Action = ClassShared.AID.LucidDreaming)]
        public Track<OffensiveStrategy> Lucid;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum OpenerStrategy
    {
        [Option("Early Dia refresh opener (Glare III at -2.3s)")]
        Standard,
        [Option("Double Misery opener (Rapture x3 in the first buffs for a second Misery)", MinLevel = 76)]
        DoubleMisery,
        [Option("No pre-pull cast")]
        None
    }

    public enum DotStrategy
    {
        [Option("Keep up on up to two targets")]
        Automatic,
        [Option("Only on the current target")]
        TargetOnly,
        [Option("Do not use")]
        Delay
    }

    public enum MiseryStrategy
    {
        [Option("Use with raid buffs; right away on 2+ targets or to avoid overcapping")]
        Automatic,
        [Option("Use as soon as possible", Targets = ActionTargets.Hostile)]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage WHM", "White Mage", "Standard rotation (Kage)|Healer", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.WHM, Class.CNJ), 100).WithStrategies<Strategy>();
    }

    private int Lily;
    private int BloodLily;
    private float NextLily { get; set; }
    private float SacredSightLeft;
    private int SacredSight;

    private bool AOEMode;

    protected override bool UsesSpellSpeed => true;
    private bool CanCast => Hints.MaxCastTime >= SlideCast(CastTime(AID.Stone));
    private float LilyCapIn => Lily >= 3 ? 0 : NextLily + (2 - Lily) * 20;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 25);

        var gauge = World.Client.GetGauge<WhiteMageGauge>();
        Lily = gauge.Lily;
        BloodLily = gauge.BloodLily;
        NextLily = MathF.Max(0, 20f - gauge.LilyTimer / 1000f);
        var sight = Player.FindStatus(SID.SacredSight, Player.InstanceID);
        SacredSight = sight?.Extra ?? 0;
        SacredSightLeft = sight is { } s ? StatusDuration(s.ExpireAt) : 0;

        var minAoETargets = Player.Level < 82 ? 2 : 3;
        AOEMode = Unlocked(AID.Holy) && UseAOE(strategy.AOE.Value, Hints.NumPriorityTargetsInAOECircle(Player.Position, 8), minAoETargets);

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null && strategy.Opener.Value != OpenerStrategy.None && countdown < CastTime(AID.Stone) + 0.8f)
                PushGCD(AID.Stone, target.Actor, 10);
            return;
        }

        if (target == null)
        {
            if (CanSpendLily(strategy))
                PushGCD(LilySpell, Player, 10);
            return;
        }

        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25);
        Hints.GoalZones.Add(strategy.AOE.Value == AOEStrategy.ST || !Unlocked(AID.Holy) ? goal : GoalCombined(goal, Hints.GoalAOECircle(8), minAoETargets));

        GCDs(strategy, target);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var splash = BestSplashTarget(target);

        if (Player.InCombat && strategy.Dot.Value != DotStrategy.Delay && DotTarget(strategy, target) is { } dotTarget)
            PushGCD(AID.Aero, dotTarget, 50);

        if (ShouldMisery(strategy, target, splash))
            PushGCD(AID.AfflatusMisery, splash.Actor, 45);

        if (SacredSight > 0 && strategy.GlareIV.Value != OffensiveStrategy.Delay
            && (strategy.GlareIV.Value == OffensiveStrategy.Force || !CanCast || RaidBuffsLeft > GCD || !HasRaidBuffJobs || SacredSightLeft < GCD + GCDLength * SacredSight + 1 || DowntimeIn < GCDLength * (SacredSight + 1)))
            PushGCD(AID.GlareIV, splash.Actor, 40);

        if (Player.InCombat && CanSpendLily(strategy)
            && (strategy.Lilies.Value == OffensiveStrategy.Force || LilyCapIn < GCD + 8 || Hints.NumPriorityTargetsInAOECircle(splash.Actor.Position, 5) >= 2))
            PushGCD(LilySpell, Player, 35);

        if (strategy.Opener.Value == OpenerStrategy.DoubleMisery && Player.InCombat && CombatTime < 30 && Unlocked(AID.AfflatusRapture) && CanSpendLily(strategy)
            && Lily >= 3 - BloodLily && RaidBuffsLeft > GCD + GCDLength * (3 - BloodLily))
            PushGCD(AID.AfflatusRapture, Player, 38);

        if (AOEMode)
            PushGCD(AID.Holy, Player, 20);

        PushGCD(AID.Stone, target.Actor, 10);

        if (!CanCast)
        {
            if (CanSpendLily(strategy))
                PushGCD(LilySpell, Player, 9);
            if (BloodLily >= 3 && strategy.Misery.Value != MiseryStrategy.Delay)
                PushGCD(AID.AfflatusMisery, splash.Actor, 8);
            if (SacredSight > 0)
                PushGCD(AID.GlareIV, splash.Actor, 7);
            if (Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25 && DotWorthIt(e) && DotLeft(e.Actor) == 0).MaxBy(e => e.Actor.HPMP.CurHP) is { } spread)
                PushGCD(AID.Aero, spread.Actor, 6);
            PushGCD(AID.Aero, target.Actor, 5);
        }
    }

    private AID LilySpell => Unlocked(AID.AfflatusRapture) ? AID.AfflatusRapture : AID.AfflatusSolace;

    // lilies only do damage by feeding Blood Lily
    private bool CanSpendLily(in Strategy strategy) => strategy.Lilies.Value != OffensiveStrategy.Delay && Lily > 0 && BloodLily < 3 && Unlocked(AID.AfflatusMisery);

    private bool ShouldMisery(in Strategy strategy, Enemy target, Enemy splash)
    {
        if (BloodLily < 3 || strategy.Misery.Value == MiseryStrategy.Delay)
            return false;
        if (strategy.Misery.Value == MiseryStrategy.ASAP || !HasRaidBuffJobs || RaidBuffsLeft > GCD)
            return true;
        if (Hints.NumPriorityTargetsInAOECircle(splash.Actor.Position, 5) >= 2)
            return true;
        if (DowntimeIn < GCDLength * 2 || target.Priority == Enemy.PriorityPointless || !TimeToKill.WillLive(target.Actor, 5))
            return true;
        return LilyCapIn < RaidBuffsIn;
    }

    private Enemy BestSplashTarget(Enemy target)
        => Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25).MaxBy(e => Hints.NumPriorityTargetsInAOECircle(e.Actor.Position, 5) * 10 + (e == target ? 1 : 0)) ?? target;

    private float DotLeft(Actor target) => MaxDotLeft(target, SID.Dia, SID.AeroII, SID.Aero);

    private float DotRefresh => GCD + GCDLength;

    private Actor? DotTarget(in Strategy strategy, Enemy target)
    {
        if (DotWorthIt(target) && (DotLeft(target.Actor) < DotRefresh || RaidBuffsLeft > GCD && RaidBuffsLeft < GCD + GCDLength && DotLeft(target.Actor) < 8))
            return target.Actor;
        if (strategy.Dot.Value != DotStrategy.Automatic || AOEMode || Hints.PriorityTargets.Count(e => DotLeft(e.Actor) >= DotRefresh) >= 2)
            return null;
        return Hints.PriorityTargets.Where(e => e != target && Player.DistanceToHitbox(e.Actor) <= 25 && DotWorthIt(e) && DotLeft(e.Actor) < DotRefresh).MaxBy(e => e.Actor.HPMP.CurHP)?.Actor;
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (strategy.PresenceOfMind.Value != OffensiveStrategy.Delay && CanWeave(AID.PresenceOfMind)
            && (strategy.PresenceOfMind.Value == OffensiveStrategy.Force || DowntimeIn > 15 && TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 15) && (!HasRaidBuffJobs || RaidBuffsLeft > 0 || RaidBuffsIn <= GCD + 1 || RaidBuffsIn > 30)))
            PushOGCD(AID.PresenceOfMind, Player, 50);

        if (strategy.Assize.Value != OffensiveStrategy.Delay && CanWeave(AID.Assize)
            && (strategy.Assize.Value == OffensiveStrategy.Force || Hints.NumPriorityTargetsInAOECircle(Player.Position, 20) > 0))
            PushOGCD(AID.Assize, Player, 40);

        if (strategy.Lucid.Value == OffensiveStrategy.Force || strategy.Lucid.Value == OffensiveStrategy.Automatic && MP <= 9000)
            if (CanWeave(ClassShared.AID.LucidDreaming))
                PushOGCD(ClassShared.AID.LucidDreaming, Player, 20);
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private float CastTime(AID aid) => SelfStatusLeft(ClassShared.SID.Swiftcast) > 0 ? 0 : ScaledCastTime(aid);

    private void PushGCD(AID aid, Actor? target, int priority) => base.PushGCD(aid, target, priority, CastTime(aid));

    #endregion
}
