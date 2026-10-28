using BossMod.WHM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageWHM(RotationModuleManager manager, Actor player) : KageShared<KageWHM.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Dia", MinLevel = 4, Actions = [AID.Dia, AID.AeroII, AID.Aero])]
        public Track<DotStrategy> Dot;

        [Track("Presence of Mind", MinLevel = 30, Action = AID.PresenceOfMind)]
        public Track<PresenceOfMindStrategy> PresenceOfMind;

        [Track("Glare IV", MinLevel = 92, Action = AID.GlareIV)]
        public Track<GlareIVStrategy> GlareIV;

        [Track("Assize", MinLevel = 56, Action = AID.Assize)]
        public Track<OffensiveStrategy> Assize;

        [Track("Afflatus Misery", MinLevel = 74, Action = AID.AfflatusMisery)]
        public Track<MiseryStrategy> Misery;

        [Track("Afflatus Solace / Rapture", MinLevel = 74, Actions = [AID.AfflatusRapture, AID.AfflatusSolace])]
        public Track<LiliesStrategy> Lilies;

        [Track("Lucid Dreaming", MinLevel = 14, Action = ClassShared.AID.LucidDreaming)]
        public Track<LucidStrategy> Lucid;

        [Track("Potion", Item = 1049238)]
        public Track<PotionStrategy> Potion;

        [Track("Opener", UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum PresenceOfMindStrategy
    {
        [Option("With raid buffs, or if they're 30s+ away")]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use ASAP")]
        Force
    }

    public enum GlareIVStrategy
    {
        [Option("Spend in raid buffs or while moving")]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use ASAP")]
        Force
    }

    public enum LiliesStrategy
    {
        [Option("Spend at cap or on 2+ targets")]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use ASAP")]
        Force
    }

    public enum LucidStrategy
    {
        [Option("Use at 8000 MP or less")]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use ASAP")]
        Force
    }

    public enum OpenerStrategy
    {
        [Option("Early Dia refresh opener")]
        Standard,
        [Option("Double Misery opener", MinLevel = 76)]
        DoubleMisery,
        [Option("No opener-specific rules")]
        None
    }

    public enum DotStrategy
    {
        [Option("Keep up on up to two targets")]
        Automatic,
        [Option("Only on the current target")]
        TargetOnly,
        [Option("Refresh on the current target now")]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum MiseryStrategy
    {
        [Option("Use with raid buffs, or on 2+ targets")]
        Automatic,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use pre-pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use ASAP")]
        Immediate
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage WHM", "White Mage", "Standard rotation (Kage)|Healer", "Kagekazu", RotationModuleQuality.Ok, BitMask.Build(Class.WHM, Class.CNJ), 100).WithStrategies<Strategy>();
    }

    private int Lily;
    private int BloodLily;
    private float NextLily { get; set; }
    private float SacredSightLeft;
    private int SacredSight;

    private bool AOEMode;
    private bool ForceST;

    protected override bool UsesSpellSpeed => true;
    private bool CanCast => CastFits(CastTime(AID.Stone));
    private float LilyCapIn => Lily >= 3 ? 0 : NextLily + (2 - Lily) * 20;

    private UseStrategy PresenceOfMindStrat;
    private UseStrategy AssizeStrat;
    private MiseryStrategy MiseryStrat;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy, strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 25, strategy.AOE.Value);
        PresenceOfMindStrat = (UseStrategy)strategy.PresenceOfMind.Value;
        AssizeStrat = (UseStrategy)strategy.Assize.Value;
        MiseryStrat = strategy.Misery.Value;
        ForceST = strategy.AOE.Value == AOEStrategy.ForceST;

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
            PushPotion(ActionDefinitions.IDPotionMnd);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null && strategy.Opener.Value != OpenerStrategy.None && countdown < CastTime(AID.Stone) + 0.8f)
                PushGCD(AID.Stone, target.Actor, 10);
            return;
        }

        if (target == null)
        {
            if (Player.InCombat && CanSpendLily(strategy))
                PushGCD(LilySpell, Player, 10);
            return;
        }

        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25);
        Hints.GoalZones.Add(strategy.AOE.Value is AOEStrategy.ST or AOEStrategy.ForceST || !Unlocked(AID.Holy) ? goal : GoalCombined(goal, Hints.GoalAOECircle(8), minAoETargets));

        GCDs(strategy, target);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var splash = BestSplashTarget(target);
        var misery = ResolveTarget(strategy.Misery) ?? splash.Actor;

        if (Player.InCombat && strategy.Dot.Value != DotStrategy.Delay && DotTarget(strategy, target) is { } dotTarget)
            PushGCD(AID.Aero, dotTarget, 50);

        if (ShouldMisery(strategy, target, misery))
            PushGCD(AID.AfflatusMisery, misery, 45);

        if (SacredSight > 0 && (UseStrategy)strategy.GlareIV.Value != UseStrategy.Delay
            && ((UseStrategy)strategy.GlareIV.Value == UseStrategy.Force || !CanCast || RaidBuffsLeft > GCD || !HasRaidBuffJobs || SacredSightLeft < GCD + GCDLength * SacredSight + 1 || DowntimeIn < GCDLength * (SacredSight + 1)))
            PushGCD(AID.GlareIV, splash.Actor, 40);

        var setReachesMisery = TimeToKill.WillLive(target.Actor, GCD + GCDLength * (3 - BloodLily) + 1);
        if (Player.InCombat && CanSpendLily(strategy)
            && ((UseStrategy)strategy.Lilies.Value == UseStrategy.Force || setReachesMisery && (LilyCapIn < GCD + 8 || !ForceST && Hints.NumPriorityTargetsInAOECircle(misery.Position, 5) >= 2)))
            PushGCD(LilySpell, Player, 35);

        if (Player.InCombat && CanSpendLily(strategy) && BloodLily > 0 && Lily >= 3 - BloodLily && setReachesMisery && !TimeToKill.WillLive(target.Actor, 20))
            PushGCD(LilySpell, Player, 36);

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
            if (BloodLily >= 3 && MiseryStrat != MiseryStrategy.Delay)
                PushGCD(AID.AfflatusMisery, misery, 8);
            if (SacredSight > 0)
                PushGCD(AID.GlareIV, splash.Actor, 7);
            if (Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25 && DotWorthIt(e) && DotLeft(e.Actor) == 0).MaxBy(e => e.Actor.HPMP.CurHP) is { } spread)
                PushGCD(AID.Aero, spread.Actor, 6);
            PushGCD(AID.Aero, target.Actor, 5);
        }
    }

    private AID LilySpell => Unlocked(AID.AfflatusRapture) ? AID.AfflatusRapture : AID.AfflatusSolace;

    private bool CanSpendLily(in Strategy strategy) => (UseStrategy)strategy.Lilies.Value != UseStrategy.Delay && Lily > 0 && BloodLily < 3 && Unlocked(AID.AfflatusMisery);

    private bool ShouldMisery(in Strategy strategy, Enemy target, Actor misery)
    {
        if (BloodLily < 3 || MiseryStrat == MiseryStrategy.Delay)
            return false;
        if (MiseryStrat == MiseryStrategy.ASAP || !HasRaidBuffJobs || RaidBuffsLeft > GCD)
            return true;
        if (!ForceST && Hints.NumPriorityTargetsInAOECircle(misery.Position, 5) >= 2)
            return true;
        if (DowntimeIn < GCDLength * 2 || target.Priority == Enemy.PriorityPointless || !TimeToKill.WillLive(target.Actor, 5))
            return true;
        return LilyCapIn < RaidBuffsIn;
    }

    private Enemy BestSplashTarget(Enemy target)
        => ForceST ? target : Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25).MaxBy(e => Hints.NumPriorityTargetsInAOECircle(e.Actor.Position, 5) * 10 + (e == target ? 1 : 0)) ?? target;

    private float DotLeft(Actor target) => MaxDotLeft(target, SID.Dia, SID.AeroII, SID.Aero);

    private float DotRefresh => GCD + GCDLength;

    private Actor? DotTarget(in Strategy strategy, Enemy target)
    {
        if (strategy.Dot.Value == DotStrategy.Force)
            return target.Actor;
        return DotTargetFor(target, DotLeft, DotRefresh, 8, strategy.Dot.Value == DotStrategy.Automatic && !AOEMode);
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (PresenceOfMindStrat != UseStrategy.Delay && CanWeave(AID.PresenceOfMind)
            && (PresenceOfMindStrat == UseStrategy.Force || DowntimeIn > 15 && TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 15) && (!HasRaidBuffJobs || RaidBuffsLeft > 0 || RaidBuffsIn <= GCD + 1 || RaidBuffsIn > 30)))
            PushOGCD(AID.PresenceOfMind, Player, 50);

        if (AssizeStrat != UseStrategy.Delay && CanWeave(AID.Assize)
            && (AssizeStrat == UseStrategy.Force || AssizeHits(target) && !(CombatTime < 30 && HasRaidBuffJobs && RaidBuffsLeft == 0 && RaidBuffsIn < 12)))
            PushOGCD(AID.Assize, Player, 40);

        if ((UseStrategy)strategy.Lucid.Value == UseStrategy.Force || (UseStrategy)strategy.Lucid.Value == UseStrategy.Automatic && MP <= 8000)
            if (CanWeave(ClassShared.AID.LucidDreaming))
                PushOGCD(ClassShared.AID.LucidDreaming, Player, 20);
    }

    private bool AssizeHits(Enemy target) => ForceST
        ? TargetInAOECircle(target.Actor, Player.Position, 15) && !Hints.ForbiddenTargets.Any(e => TargetInAOECircle(e.Actor, Player.Position, 15))
        : Hints.NumPriorityTargetsInAOECircle(Player.Position, 15) > 0;

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private float CastTime(AID aid) => HardcastTime(aid);

    private void PushGCD(AID aid, Actor? target, int priority) => base.PushGCD(aid, target, priority, CastTime(aid));

    #endregion
}
