using BossMod.WHM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageWHM(RotationModuleManager manager, Actor player) : TypedRotationModule<KageWHM.Strategy>(manager, player)
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
    private float NextLily;
    private float SacredSightLeft;
    private int SacredSight;

    private float RaidBuffsLeft;
    private float RaidBuffsIn;
    private float DowntimeIn;
    private bool AOEMode;
    private float AnimLockDelay;
    private Targeting TargetMode;

    private float GCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);
    private bool CanCast => Hints.MaxCastTime >= Math.Max(0, CastTime(AID.Stone) - 0.5f);
    private bool HasRaidBuffJobs => TimeToKill.InBossFight(Bossmods.ActiveModule, World.Actors.Find(Player.TargetID)) && (RaidBuffsIn < 9000 || RaidBuffsLeft > 0);
    private float LilyCapIn => Lily >= 3 ? 0 : NextLily + (2 - Lily) * 20;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        AnimLockDelay = estimatedAnimLockDelay;

        TimeToKill.Update(World, Hints);
        var target = Hints.FindEnemy(primaryTarget);
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden || target?.Priority == Enemy.PriorityPointless && Hints.PriorityTargets.Any())
            target = null;

        TargetMode = strategy.Targeting.Value == Targeting.AutoTryPri ? (target != null ? Targeting.AutoPrimary : Targeting.Auto) : strategy.Targeting.Value;
        if (TargetMode == Targeting.Auto && target == null)
        {
            target = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25).MinBy(e => Player.DistanceToHitbox(e.Actor)) ?? target;
            primaryTarget = target?.Actor;
        }

        var gauge = World.Client.GetGauge<WhiteMageGauge>();
        Lily = gauge.Lily;
        BloodLily = gauge.BloodLily;
        NextLily = MathF.Max(0, 20f - gauge.LilyTimer / 1000f);
        var sight = Player.FindStatus(SID.SacredSight, Player.InstanceID);
        SacredSight = sight?.Extra ?? 0;
        SacredSightLeft = sight is { } s ? StatusDuration(s.ExpireAt) : 0;

        (RaidBuffsLeft, RaidBuffsIn) = RaidBuffs.Estimate(Bossmods, World, Player, primaryTarget, Player.InCombat ? (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds : 0);
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;

        AOEMode = Unlocked(AID.Holy) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => Hints.NumPriorityTargetsInAOECircle(Player.Position, 8) >= (Player.Level < 82 ? 2 : 3),
            _ => false
        };

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null && countdown < CastTime(AID.Stone) + 0.8f)
                PushGCD(AID.Stone, target.Actor, 10);
            return;
        }

        if (target == null && strategy.Lilies.Value != OffensiveStrategy.Delay && Lily > 0 && BloodLily < 3 && Unlocked(AID.AfflatusMisery))
            PushGCD(LilySpell, Player, 10);

        if (target == null)
            return;

        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25);
        Hints.GoalZones.Add(strategy.AOE.Value == AOEStrategy.ST || !Unlocked(AID.Holy) ? goal : GoalCombined(goal, Hints.GoalAOECircle(8), Player.Level < 82 ? 2 : 3));

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

        if (Player.InCombat && strategy.Lilies.Value != OffensiveStrategy.Delay && Unlocked(AID.AfflatusMisery) && BloodLily < 3 && Lily > 0
            && (strategy.Lilies.Value == OffensiveStrategy.Force || LilyCapIn < GCD + 8 || Hints.NumPriorityTargetsInAOECircle(splash.Actor.Position, 5) >= 2))
            PushGCD(LilySpell, Player, 35);

        if (AOEMode)
            PushGCD(AID.Holy, Player, 20);

        PushGCD(AID.Stone, target.Actor, 10);

        if (!CanCast)
        {
            if (Lily > 0 && BloodLily < 3 && Unlocked(AID.AfflatusMisery) && strategy.Lilies.Value != OffensiveStrategy.Delay)
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

    private float DotLeft(Actor target)
    {
        float Left(SID sid) => target.FindStatus((uint)sid, Player.InstanceID) is { } st ? StatusDuration(st.ExpireAt) : 0;
        return MathF.Max(Left(SID.Dia), MathF.Max(Left(SID.AeroII), Left(SID.Aero)));
    }

    private float DotRefresh => GCD + GCDLength;

    private bool DotWorthIt(Enemy e)
    {
        if (e.ForbidDOTs || e.Priority < 0 && !TimeToKill.IsBossTier(Bossmods.ActiveModule, Hints, e.Actor) || DowntimeIn < 15)
            return false;
        return TimeToKill.WillLive(e.Actor, 15);
    }

    private Actor? DotTarget(in Strategy strategy, Enemy target)
    {
        if (DotWorthIt(target) && (DotLeft(target.Actor) < DotRefresh || RaidBuffsLeft > GCD && RaidBuffsLeft < GCD + GCDLength && DotLeft(target.Actor) < 8))
            return target.Actor;
        if (strategy.Dot.Value != DotStrategy.Automatic || AOEMode)
            return null;
        if (Hints.PriorityTargets.Count(e => DotLeft(e.Actor) >= DotRefresh) >= 2)
            return null;
        return Hints.PriorityTargets.Where(e => e != target && Player.DistanceToHitbox(e.Actor) <= 25 && DotWorthIt(e) && DotLeft(e.Actor) < DotRefresh).MaxBy(e => e.Actor.HPMP.CurHP)?.Actor;
    }

    private bool MPCheckSoon
        => StateTimeline.Raidwides(Bossmods.ActiveModule, World, Hints).Any(t => t >= World.CurrentTime && t <= World.FutureTime(15))
        || World.Party.WithoutSlot(excludeAlliance: true).Any(p => p.IsDead);

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (strategy.PresenceOfMind.Value != OffensiveStrategy.Delay && CanWeave(AID.PresenceOfMind)
            && (strategy.PresenceOfMind.Value == OffensiveStrategy.Force || DowntimeIn > 15 && TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 15) && (!HasRaidBuffJobs || RaidBuffsLeft > 0 || RaidBuffsIn <= GCD + 1 || RaidBuffsIn > 30)))
            PushOGCD(AID.PresenceOfMind, Player, 50);

        if (strategy.Assize.Value != OffensiveStrategy.Delay && CanWeave(AID.Assize)
            && (strategy.Assize.Value == OffensiveStrategy.Force || Hints.NumPriorityTargetsInAOECircle(Player.Position, 20) > 0 && (!HasRaidBuffJobs || RaidBuffsLeft > 0 || RaidBuffsIn > 8)))
            PushOGCD(AID.Assize, Player, 40);

        if (strategy.Lucid.Value == OffensiveStrategy.Force || strategy.Lucid.Value == OffensiveStrategy.Automatic && (Player.HPMP.CurMP <= 8000 || Player.HPMP.CurMP <= 9000 && MPCheckSoon))
            if (CanWeave(ActionID.MakeSpell(ClassShared.AID.LucidDreaming)))
                PushOGCD(ActionID.MakeSpell(ClassShared.AID.LucidDreaming), Player, 20);
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.Immediate => true,
        _ => false
    };

    private float CastTime(AID aid)
    {
        if (Player.FindStatus((uint)ClassShared.SID.Swiftcast, Player.InstanceID) != null)
            return 0;
        var def = ActionDefinitions.Instance.Spell(aid);
        return def == null || def.CastTime == 0 ? 0 : def.CastTime * GCDLength / 2.5f;
    }

    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    private bool CanWeave(AID aid) => CanWeave(ActionID.MakeSpell(aid));
    private bool CanWeave(ActionID action)
    {
        if (!ActionUnlocked(action) || ActionDefinitions.Instance[action] is not { } def)
            return false;
        return MathF.Max(def.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions), World.Client.AnimationLock) + def.TotalDuration + AnimLockDelay <= GCD;
    }

    private void PushGCD(AID aid, Actor? target, int priority)
    {
        if (!Unlocked(aid))
            return;
        var action = ActionID.MakeSpell(aid);
        var def = ActionDefinitions.Instance[action];
        if (def == null || def.Range != 0 && target == null)
            return;
        Hints.ActionsToExecute.Push(action, def.Range == 0 ? Player : target, ActionQueue.Priority.High + priority, castTime: Math.Max(0, CastTime(aid) - 0.5f));
    }

    private void PushOGCD(AID aid, Actor? target, int priority) => PushOGCD(ActionID.MakeSpell(aid), target, priority);
    private void PushOGCD(ActionID action, Actor? target, int priority)
    {
        if (!ActionUnlocked(action))
            return;
        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.Low + priority);
    }

    #endregion
}
