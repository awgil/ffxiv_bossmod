﻿using System.Diagnostics.CodeAnalysis;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public enum Targeting { Manual, Auto, AutoPrimary, AutoTryPri }
public enum OffensiveStrategy { Automatic, Delay, Force }
public enum AOEStrategy { AOE, ST, ForceAOE, ForceST }

public enum SharedTrack { Targeting, AOE, Buffs, Count }

public abstract class Attackxan<AID, TraitID>(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
    where AID : struct, Enum where TraitID : Enum
{
    protected sealed override float GCDLength => AttackGCDLength;
}

public abstract class Castxan<AID, TraitID>(RotationModuleManager manager, Actor player) : Basexan<AID, TraitID>(manager, player)
    where AID : struct, Enum where TraitID : Enum
{
    protected sealed override float GCDLength => SpellGCDLength;
}

public abstract class Basexan<AID, TraitID>(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
    where AID : struct, Enum where TraitID : Enum
{
    protected float PelotonLeft { get; private set; }
    protected float SwiftcastLeft { get; private set; }
    protected float TrueNorthLeft { get; private set; }
    protected float CombatTimer { get; private set; }
    protected float AnimationLockDelay { get; private set; }
    protected float RaidBuffsIn { get; private set; }
    protected float RaidBuffsLeft { get; private set; }
    protected float DowntimeIn { get; private set; }
    protected float? UptimeIn { get; private set; }
    protected Enemy? PlayerTarget { get; private set; }
    protected bool IsMoving { get; private set; }

    protected float? CountdownRemaining => World.Client.CountdownRemaining;
    protected float AnimLock => World.Client.AnimationLock;

    protected float AttackGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected float SpellGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    protected float ReadyIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    protected float MaxChargesIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;

    protected abstract float GCDLength { get; }

    public bool CanFitGCD(float duration, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < duration;

    protected bool OnCooldown(AID aid) => MaxChargesIn(aid) > 0;

    public bool CanWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, AnimLock) + actionLock + AnimationLockDelay <= GCD + GCDLength * extraGCDs + extraFixedDelay;

    public bool CanWeave(AID aid, int extraGCDs = 0, float extraFixedDelay = 0)
    {
        // TODO is this actually helpful?
        if (!Unlocked(aid))
            return false;

        var def = ActionDefinitions.Instance[ActionID.MakeSpell(aid)]!;

        // amnesia check
        if (def.Category == ActionCategory.Ability && Player.FindStatus(1092) != null)
            return false;

        return CanWeave(ReadyIn(aid), def.InstantAnimLock, extraGCDs, extraFixedDelay);
    }

    protected AID NextGCD;
    protected int NextGCDPrio;
    protected uint MP;

    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    // override if some action requires specific runtime checks that aren't covered by the existing framework code
    protected virtual bool CanUse(AID action) => true;

    protected void PushGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => PushGCD(aid, target, (int)(object)priority, delay);

    protected void PushGCD<P>(AID aid, Enemy? target, P priority, float delay = 0) where P : Enum
        => PushGCD(aid, target?.Actor, (int)(object)priority, delay);

    protected void PushGCD(AID aid, Enemy? target, int priority = 2, float delay = 0) => PushGCD(aid, target?.Actor, priority, delay);

    protected void PushGCD(AID aid, Actor? target, int priority = 2, float delay = 0)
    {
        if (priority == 0)
            return;

        if (PushAction(aid, target, ActionQueue.Priority.High + priority, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }

    protected void PushOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => PushOGCD(aid, target, (int)(object)priority, delay);

    protected void PushOGCD<P>(AID aid, Enemy? target, P priority, float delay = 0) where P : Enum
        => PushOGCD(aid, target?.Actor, (int)(object)priority, delay);

    protected void PushOGCD(AID aid, Enemy? target, int priority = 1, float delay = 0) => PushOGCD(aid, target?.Actor, priority, delay);

    protected void PushOGCD(AID aid, Actor? target, int priority = 1, float delay = 0)
    {
        if (priority == 0)
            return;

        PushAction(aid, target, ActionQueue.Priority.Low + priority, delay);
    }

    protected bool PushAction(AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        if (!CanUse(aid))
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null || !def.IsUnlocked(World, Player))
            return false;

        if (def.Range != 0 && target == null)
        {
            // Service.Log($"Queued targeted action ({aid}) with no target");
            return false;
        }

        Vector3 targetPos = default;

        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (def.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos, castTime: GetSlidecastTime(aid));
        return true;
    }

    /// <summary>
    /// <para>Tries to select a suitable primary target.</para>
    /// <para>If the provided <paramref name="primaryTarget"/> is null, an NPC, or non-enemy object; it will be reset to <c>null</c>.</para>
    /// <para>Additionally, if <paramref name="range"/> is set to <c>Targeting.Auto</c>, and the user's current target is more than <paramref name="range"/> yalms from the player, this function attempts to find a closer one. No prioritization is done; if any target is returned, it is simply the actor that was earliest in the object table. If no closer target is found, <paramref name="primaryTarget"/> will remain unchanged.</para>
    /// </summary>
    /// <param name="strategy">Targeting strategy</param>
    /// <param name="primaryTarget">Player's current target - may be null</param>
    /// <param name="range">Maximum distance from the player to search for a candidate target</param>
    protected void SelectPrimaryTarget(StrategyValues strategy, ref Enemy? primaryTarget, float range)
    {
        var t = strategy.Option(SharedTrack.Targeting).As<Targeting>();

        if (t is Targeting.Auto or Targeting.AutoTryPri)
        {
            if (Player.DistanceToHitbox(primaryTarget) > range)
            {
                var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range);
                if (newTarget != null)
                    primaryTarget = newTarget;
            }
        }
    }

    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);

    protected (Enemy? Best, int Targets) SelectTarget(
        StrategyValues strategy,
        Enemy? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);

    protected (Enemy? Best, int Targets) SelectTargetByHP(StrategyValues strategy, Enemy? primaryTarget, float range, PositionCheck isInAOE)
        => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, actor) => (numTargets, numTargets > 2 ? actor.HPMP.CurHP : 0), args => args.numTargets);

    protected (Enemy? Best, int Priority) SelectTarget<P>(
        StrategyValues strategy,
        Enemy? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize,
        Func<P, int> simplify
    ) where P : struct, IComparable
    {
        var aoe = strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
        var targeting = strategy.Option(SharedTrack.Targeting).As<Targeting>();

        P targetPrio(Actor potentialTarget)
        {
            var numTargets = Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor));
            return prioritize(AdjustNumTargets(strategy, numTargets), potentialTarget);
        }

        // in regular ST mode and when using a skill that deals splash damage (like Primal Rend), it is possible that primary target has prio 0 if the splash damage would hit a forbidden target
        // however, in force-ST mode, prio is *always* 0, so we cannot find a better target - in this case, skip entirely
        if (aoe == AOEStrategy.ForceST)
            targeting = Targeting.Manual;

        if (targeting == Targeting.AutoTryPri)
            targeting = Player.DistanceToHitbox(primaryTarget) <= range ? Targeting.AutoPrimary : Targeting.Auto;

        var (newtarget, newprio) = targeting switch
        {
            Targeting.Auto => FindBetterTargetBy(primaryTarget?.Actor, range, targetPrio),
            Targeting.AutoPrimary => primaryTarget == null ? (null, default) : FindBetterTargetBy(
                primaryTarget.Actor,
                range,
                targetPrio,
                enemy => isInAOE(enemy.Actor, primaryTarget.Actor)
            ),
            _ => (primaryTarget?.Actor, primaryTarget == null ? default : targetPrio(primaryTarget.Actor))
        };
        var newnewprio = simplify(newprio);
        return (newnewprio > 0 ? Hints.FindEnemy(newtarget) : null, newnewprio);
    }

    /// <summary>
    /// <para>Find a good target to apply a DoT effect to. Has no effect if auto-targeting is disabled.</para>
    /// <para>If <c>Hints.PriorityTargets</c> contains more than <c>maxAllowedTargets</c>, <c>null</c> will be returned. Enemies with <c>ForbidDOTs = true</c> are not counted in this case.</para>
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <param name="strategy"></param>
    /// <param name="initial"></param>
    /// <param name="getTimer"></param>
    /// <param name="maxAllowedTargets"></param>
    /// <returns></returns>
    protected (Enemy? Target, P Timer) SelectDotTarget<P>(StrategyValues strategy, Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        var forbidden = initial?.ForbidDOTs ?? false;
        switch (strategy.Targeting())
        {
            case Targeting.Manual:
            case Targeting.AutoPrimary:
                return forbidden ? (null, getTimer(null)) : (initial, getTimer(initial?.Actor));
            case Targeting.AutoTryPri:
                if (initial != null)
                    return forbidden ? (null, getTimer(null)) : (initial, getTimer(initial?.Actor));
                break;
        }

        var newTarget = initial;
        var initialTimer = getTimer(initial?.Actor);
        var newTimer = initialTimer;

        var numTargets = 0;

        foreach (var dotTarget in Hints.PriorityTargets)
        {
            if (dotTarget.ForbidDOTs)
                continue;

            if (++numTargets > maxAllowedTargets)
                return (null, getTimer(null));

            var thisTimer = getTimer(dotTarget.Actor);
            if (thisTimer.CompareTo(newTimer) < 0)
            {
                newTarget = dotTarget;
                newTimer = thisTimer;
            }
        }

        return (newTarget, newTimer);
    }

    // used for casters that don't have a separate maximize-AOE function
    protected void GoalZoneSingle(float range)
    {
        if (PlayerTarget != null)
            Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, range));
    }

    protected void GoalZoneCombined(StrategyValues strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
    {
        var (_, positional, imminent, _) = Hints.RecommendedPositional;

        if (!strategy.AOEOk() || !Unlocked(firstUnlockedAoeAction))
            minAoe = 50;

        if (PlayerTarget == null)
        {
            if (minAoe < 50)
                Hints.GoalZones.Add(fAoe);
        }
        else
        {
            Hints.GoalZones.Add(Hints.GoalCombined(Hints.GoalSingleTarget(PlayerTarget.Actor, imminent ? positional : Positional.Any, range), fAoe, minAoe));
            if (maximumActionRange is float r)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(PlayerTarget.Actor, r, 0.5f));
        }
    }

    protected int NumMeleeAOETargets(StrategyValues strategy) => NumNearbyTargets(strategy, 5);

    protected int NumNearbyTargets(StrategyValues strategy, float range) => AdjustNumTargets(strategy, Hints.NumPriorityTargetsInAOECircle(Player.Position, range));

    protected int AdjustNumTargets(StrategyValues strategy, int reported)
        => reported == 0 ? 0 : strategy.AOE() switch
        {
            AOEStrategy.AOE => reported,
            AOEStrategy.ST => 1,
            AOEStrategy.ForceAOE => 10,
            AOEStrategy.ForceST => 0,
            _ => 0
        };

    protected PositionCheck IsSplashTarget => (Actor primary, Actor other) => Hints.TargetInAOECircle(other, primary.Position, 5);
    protected PositionCheck Is25yRectTarget => (Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 25, 2);

    /// <summary>
    /// Get <em>effective</em> cast time for the provided action.<br/>
    /// The default implementation returns the action's base cast time multiplied by the player's spell-speed factor, which accounts for haste buffs (like Leylines) and slow debuffs. It also accounts for Swiftcast.<br/>
    /// Subclasses should handle job-specific cast speed adjustments, such as RDM's Dualcast or PCT's motifs.
    /// </summary>
    /// <param name="aid"></param>
    /// <returns></returns>
    protected virtual float GetCastTime(AID aid) => SwiftcastLeft > GCD ? 0 : ActionDefinitions.Instance.Spell(aid)!.CastTime * GCDLength / 2.5f;

    protected float NextCastStart => AnimLock > GCD ? AnimLock + AnimationLockDelay : GCD;

    protected float GetSlidecastTime(AID aid) => MathF.Max(0, GetCastTime(aid) - 0.5f);
    protected float GetSlidecastEnd(AID aid) => NextCastStart + GetSlidecastTime(aid);

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.7071068f => Positional.Rear,
        < 0.7071068f => Positional.Flank,
        _ => Positional.Front
    };

    protected bool NextPositionalImminent;
    protected bool NextPositionalCorrect;

    protected void UpdatePositionals(Enemy? enemy, ref (Positional pos, bool imm) positional)
    {
        var trueNorth = TrueNorthLeft > GCD;
        var target = enemy?.Actor;
        if ((target?.Omnidirectional ?? true) || target?.TargetID == Player.InstanceID && target?.CastInfo == null && positional.pos != Positional.Front && target?.NameID != 541)
            positional = (Positional.Any, false);

        NextPositionalImminent = !trueNorth && positional.imm;
        NextPositionalCorrect = trueNorth || target == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized())) < 0.7071067f,
            Positional.Rear => target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized()) < -0.7071068f,
            _ => true
        };
        Hints.RecommendedPositional = (target, positional.pos, NextPositionalImminent, NextPositionalCorrect);
    }

    private float? _prevCountdown;
    private DateTime _cdLockout;

    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "determinism is intentional here")]
    private void PretendCountdown()
    {
        if (CountdownRemaining == null)
        {
            _cdLockout = DateTime.MinValue;
            _prevCountdown = null;
        }
        else if (_prevCountdown == null)
        {
            var wait = (float)new Random((int)World.Frame.Index).NextDouble() + 0.5f;
            _cdLockout = World.FutureTime(wait);
            _prevCountdown = CountdownRemaining;
        }
    }

    public sealed override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        IsMoving = isMoving;
        NextGCD = default;
        NextGCDPrio = 0;
        PlayerTarget = Hints.FindEnemy(primaryTarget);

        PretendCountdown();

        var pelo = Player.FindStatus(ClassShared.SID.Peloton);
        PelotonLeft = pelo != null ? StatusDuration(pelo.Value.ExpireAt) : 0;
        SwiftcastLeft = MathF.Max(StatusLeft(ClassShared.SID.Swiftcast), StatusLeft(ClassShared.SID.LostChainspell));
        TrueNorthLeft = StatusLeft(ClassShared.SID.TrueNorth);

        AnimationLockDelay = estimatedAnimLockDelay;

        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);

        if (Manager.Planner?.EstimateTimeToNextDowntime() is (var downtimeNow, var stateLeft))
        {
            DowntimeIn = downtimeNow ? 0 : stateLeft;
            UptimeIn = downtimeNow ? stateLeft : 0;
        }
        else
        {
            DowntimeIn = float.MaxValue;
            UptimeIn = null;
        }

        // TODO max MP can be higher in eureka/bozja
        MP = (uint)Math.Clamp(Player.PredictedMPRaw, 0, 10000);

        if (_cdLockout > World.CurrentTime)
            return;

        if (Player.MountId is not (103 or 117 or 128))
            Exec(strategy, PlayerTarget);
    }

    // other classes have timed personal buffs to plan around, like blm leylines, mch overheat, gnb nomercy
    // war could also be here but i dont have a war rotation
    private bool IsSelfish(Class cls) => cls is Class.VPR or Class.SAM or Class.WHM or Class.SGE or Class.DRK;

    private new (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        if (Bossmods.ActiveModule?.Info?.GroupType is BossModuleInfo.GroupType.BozjaDuel && IsSelfish(Player.Class))
            return (float.MaxValue, 0);

        if (primaryTarget?.IsStrikingDummy == true)
        {
            // hack for a dummy: expect that raidbuffs appear at 7.8s and then every 120s
            var cycleTime = CombatTimer - 7.8f;
            if (cycleTime < 0)
                return (0, 7.8f - CombatTimer); // very beginning of a fight

            cycleTime %= 120;
            return cycleTime < 20 ? (20 - cycleTime, 0) : (0, 120 - cycleTime);
        }

        var buffsIn = Bossmods.RaidCooldowns.NextDamageBuffIn2();
        if (buffsIn == null)
        {
            if (CombatTimer < 7.8f && World.Party.WithoutSlot(false, true, true).Skip(1).Any(HavePartyBuff))
                buffsIn = 7.8f - CombatTimer;
            else
                // no party members with raid buffs, assume we're never getting any
                buffsIn = float.MaxValue;
        }

        return (Bossmods.RaidCooldowns.DamageBuffLeft(Player), buffsIn.Value);
    }

    private bool HavePartyBuff(Actor player) => player.Class switch
    {
        Class.MNK => player.Level >= 70, // brotherhood
        Class.DRG => player.Level >= 52, // battle litany
        Class.NIN => player.Level >= 45, // mug/dokumori - level check is for suiton/huton, which grant Shadow Walker
        Class.RPR => player.Level >= 72, // arcane circle

        Class.SMN => player.Level >= 66, // searing light
        Class.RDM => player.Level >= 58, // embolden
        Class.PCT => player.Level >= 70, // starry muse

        Class.BRD => player.Level >= 50, // battle voice - not counting songs since they are permanent kinda
        Class.DNC => player.Level >= 70, // tech finish

        Class.SCH => player.Level >= 66, // chain
        Class.AST => player.Level >= 50, // divination

        _ => false
    };

    public abstract void Exec(StrategyValues strategy, Enemy? primaryTarget);

    protected (float Left, int Stacks) Status<SID>(SID status) where SID : Enum => Player.FindStatus(status) is ActorStatus s ? (StatusDuration(s.ExpireAt), s.Extra & 0xFF) : (0, 0);
    protected float StatusLeft<SID>(SID status) where SID : Enum => Status(status).Left;
    protected int StatusStacks<SID>(SID status) where SID : Enum => Status(status).Stacks;

    protected float HPRatio(Actor actor) => (float)actor.HPMP.CurHP / Player.HPMP.MaxHP;
    protected float HPRatio() => HPRatio(Player);

    protected uint PredictedHP(Actor actor) => (uint)actor.PredictedHPClamped;
    protected float PredictedHPRatio(Actor actor) => (float)PredictedHP(actor) / actor.HPMP.MaxHP;
}

static class Extendxan
{
    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineShared(this RotationModuleDefinition def)
    {
        return def.DefineSharedTA().DefineSimple(SharedTrack.Buffs, "Buffs");
    }

    public static RotationModuleDefinition DefineSharedTA(this RotationModuleDefinition def)
    {
        def.Define(SharedTrack.Targeting).As<Targeting>("Targeting")
            .AddOption(xan.Targeting.Manual, "Manual", "Use player's current target for all actions")
            .AddOption(xan.Targeting.Auto, "Auto", "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(xan.Targeting.AutoPrimary, "AutoPrimary", "Automatically select best target for AOE actions - ensure player target is hit")
            .AddOption(xan.Targeting.AutoTryPri, "AutoTryPri", "Automatically select best target for AOE actions - if player has a target, ensure that target is hit");

        def.Define(SharedTrack.AOE).As<AOEStrategy>("AOE")
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.ST, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.ForceAOE, "ForceAOE", "Always use AOE actions, even on one target")
            .AddOption(AOEStrategy.ForceST, "ForceST", "Forbid any action that can hit multiple targets");

        return def;
    }

    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineSimple<Index>(this RotationModuleDefinition def, Index track, string name, int minLevel = 1, float uiPriority = 0) where Index : Enum
    {
        return def.Define(track).As<OffensiveStrategy>(name, uiPriority: uiPriority)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Use when optimal", minLevel: minLevel)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't use", minLevel: minLevel)
            .AddOption(OffensiveStrategy.Force, "Force", "Use ASAP", minLevel: minLevel);
    }

    public static AOEStrategy AOE(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
    public static Targeting Targeting(this StrategyValues strategy) => strategy.Option(SharedTrack.Targeting).As<Targeting>();
    public static OffensiveStrategy Simple<Index>(this StrategyValues strategy, Index track) where Index : Enum => strategy.Option(track).As<OffensiveStrategy>();
    public static bool BuffsOk(this StrategyValues strategy) => strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;
    public static bool AOEOk(this StrategyValues strategy) => strategy.AOE() is AOEStrategy.AOE or AOEStrategy.ForceAOE;
    public static float DistanceToHitbox(this Actor actor, Enemy? other) => actor.DistanceToHitbox(other?.Actor);
}
