namespace BossMod.Autorotation.xan;

public enum Targeting { Manual, Auto, AutoPrimary, AutoTryPri }
public enum OffensiveStrategy { Automatic, Delay, Force }
public enum AOEStrategy { ST, AOE, ForceAOE, ForceST }

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

public abstract class Basexan<AID, TraitID>(RotationModuleManager manager, Actor player) : Targetxan(manager, player)
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

    protected float? CountdownRemaining => World.Client.CountdownRemaining;

    protected float AttackGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected float SpellGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    protected float NextChargeIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ReadyIn(World.Client.Cooldowns) : float.MaxValue;
    protected float MaxChargesIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ChargeCapIn(World.Client.Cooldowns, Player.Level) : float.MaxValue;

    protected abstract float GCDLength { get; }

    public bool CanFitGCD(float duration, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < duration;

    protected float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    public bool CanWeave(float cooldown, float actionLock, int extraGCDs = 0, float extraFixedDelay = 0)
        => MathF.Max(cooldown, World.Client.AnimationLock) + actionLock + AnimationLockDelay <= GCD + GCDLength * extraGCDs + extraFixedDelay;
    public bool CanWeave(AID aid, int extraGCDs = 0, float extraFixedDelay = 0)
    {
        // TODO is this actually helpful?
        if (!Unlocked(aid))
            return false;

        var def = ActionDefinitions.Instance[ActionID.MakeSpell(aid)]!;
        return CanWeave(CD(aid), def.InstantAnimLock, extraGCDs, extraFixedDelay);
    }

    protected AID NextGCD;
    protected int NextGCDPrio;
    protected uint MP;

    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    protected void PushGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => PushGCD(aid, target, (int)(object)priority, delay);

    protected void PushGCD(AID aid, Actor? target, int priority = 0, float delay = 0)
    {
        if (priority < 0)
            return;

        if (PushAction(aid, target, ActionQueue.Priority.High + 100 + priority, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }

    protected void PushOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => PushOGCD(aid, target, (int)(object)priority, delay);

    protected void PushOGCD(AID aid, Actor? target, int priority = 0, float delay = 0)
        => PushAction(aid, target, ActionQueue.Priority.Low + 100 + priority, delay);

    protected bool PushAction(AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        if (!CanCast(aid))
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            // Service.Log($"Queued targeted action ({aid}) with no target");
            return false;
        }

        Vector3 targetPos = default;

        if (def.Range == 0 && def.AllowedTargets.HasFlag(ActionTargets.Area))
            targetPos = Player.PosRot.XYZ();

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
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
    protected void SelectPrimaryTarget(StrategyValues strategy, ref Actor? primaryTarget, float range)
    {
        var t = strategy.Option(SharedTrack.Targeting).As<Targeting>();

        if (!IsEnemy(primaryTarget))
            primaryTarget = null;

        if (t is Targeting.Auto or Targeting.AutoTryPri)
        {
            if (Player.DistanceToHitbox(primaryTarget) > range)
            {
                var newTarget = Hints.PriorityTargets.FirstOrDefault(x => Player.DistanceToHitbox(x.Actor) <= range)?.Actor;
                if (newTarget != null)
                    primaryTarget = newTarget;
                // Hints.ForcedTarget = primaryTarget;
            }
        }
    }

    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);

    protected (Actor? Best, int Targets) SelectTarget(
        StrategyValues strategy,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);

    protected (Actor? Best, int Targets) SelectTargetByHP(StrategyValues strategy, Actor? primaryTarget, float range, PositionCheck isInAOE)
        => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, actor) => (numTargets, numTargets > 2 ? actor.HPMP.CurHP : 0), args => args.numTargets);

    protected (Actor? Best, int Priority) SelectTarget<P>(
        StrategyValues strategy,
        Actor? primaryTarget,
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
            targeting = primaryTarget == null ? Targeting.Auto : Targeting.AutoPrimary;

        var (newtarget, newprio) = targeting switch
        {
            Targeting.Auto => FindBetterTargetBy(primaryTarget, range, targetPrio),
            Targeting.AutoPrimary => primaryTarget == null ? (null, default) : FindBetterTargetBy(
                primaryTarget,
                range,
                targetPrio,
                enemy => isInAOE(enemy.Actor, primaryTarget)
            ),
            _ => (primaryTarget, primaryTarget == null ? default : targetPrio(primaryTarget))
        };
        var newnewprio = simplify(newprio);
        return (newnewprio > 0 ? newtarget : null, newnewprio);
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
    protected (Actor? Target, P Timer) SelectDotTarget<P>(StrategyValues strategy, Actor? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        switch (strategy.Targeting())
        {
            case Targeting.Manual:
            case Targeting.AutoPrimary:
                return (initial, getTimer(initial));
            case Targeting.AutoTryPri:
                if (initial != null)
                    return (initial, getTimer(initial));
                break;
        }

        var newTarget = initial;
        var initialTimer = getTimer(initial);
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
                newTarget = dotTarget.Actor;
                newTimer = thisTimer;
            }
        }

        return (newTarget, newTimer);
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

    protected float NextCastStart => World.Client.AnimationLock > GCD ? World.Client.AnimationLock + AnimationLockDelay : GCD;

    protected float GetSlidecastTime(AID aid) => MathF.Max(0, GetCastTime(aid) - 0.5f);
    protected float GetSlidecastEnd(AID aid) => NextCastStart + GetSlidecastTime(aid);

    protected bool CanCast(AID aid)
    {
        var t = GetSlidecastTime(aid);
        if (t == 0)
            return true;

        return NextCastStart + t <= ForceMovementIn;
    }

    protected float ForceMovementIn;

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    private static bool IsEnemy(Actor? actor) => actor != null && actor.Type is ActorType.Enemy or ActorType.Part && !actor.IsAlly;

    protected Positional GetCurrentPositional(Actor target) => (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
    {
        < -0.707167f => Positional.Rear,
        < 0.707167f => Positional.Flank,
        _ => Positional.Front
    };

    protected bool NextPositionalImminent;
    protected bool NextPositionalCorrect;

    protected void UpdatePositionals(Actor? target, (Positional pos, bool imm) positional, bool trueNorth)
    {
        var ignore = trueNorth || (target?.Omnidirectional ?? true);
        var next = positional.pos;
        NextPositionalImminent = !ignore && positional.imm;
        NextPositionalCorrect = ignore || target == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized())) < 0.7071067f,
            Positional.Rear => target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized()) < -0.7071068f,
            _ => true
        };
        Manager.Hints.RecommendedPositional = (target, next, NextPositionalImminent, NextPositionalCorrect);
    }

    public sealed override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        var pelo = Player.FindStatus(BossMod.BRD.SID.Peloton);
        PelotonLeft = pelo != null ? StatusDuration(pelo.Value.ExpireAt) : 0;
        SwiftcastLeft = StatusLeft(BossMod.WHM.SID.Swiftcast);
        TrueNorthLeft = StatusLeft(BossMod.DRG.SID.TrueNorth);

        ForceMovementIn = forceMovementIn;
        AnimationLockDelay = estimatedAnimLockDelay;

        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime().Item2 ?? float.MaxValue;

        // TODO max MP can be higher in eureka/bozja
        MP = (uint)Math.Clamp(Player.HPMP.CurMP + World.PendingEffects.PendingMPDifference(Player.InstanceID), 0, 10000);

        Exec(strategy, primaryTarget);
    }

    private new (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        // striking dummy that spawns in Explorer Mode
        if (primaryTarget?.OID != 0x2DE0)
            return (Bossmods.RaidCooldowns.DamageBuffLeft(Player), Bossmods.RaidCooldowns.NextDamageBuffIn2());

        // hack for a dummy: expect that raidbuffs appear at 7.8s and then every 120s
        var cycleTime = CombatTimer - 7.8f;
        if (cycleTime < 0)
            return (0, 7.8f - CombatTimer); // very beginning of a fight

        cycleTime %= 120;
        return cycleTime < 20 ? (20 - cycleTime, 0) : (0, 120 - cycleTime);
    }

    public abstract void Exec(StrategyValues strategy, Actor? primaryTarget);

    protected (float Left, int Stacks) Status<SID>(SID status) where SID : Enum => Player.FindStatus(status) is ActorStatus s ? (StatusDuration(s.ExpireAt), s.Extra & 0xFF) : (0, 0);
    protected float StatusLeft<SID>(SID status) where SID : Enum => Status(status).Left;
    protected int StatusStacks<SID>(SID status) where SID : Enum => Status(status).Stacks;

    protected float HPRatio(Actor actor) => (float)actor.HPMP.CurHP / Player.HPMP.MaxHP;
    protected float HPRatio() => HPRatio(Player);

    protected uint PredictedHP(Actor actor) => (uint)Math.Clamp(actor.HPMP.CurHP + World.PendingEffects.PendingHPDifference(actor.InstanceID), 0, actor.HPMP.MaxHP);
    protected float PredictedHPRatio(Actor actor) => (float)PredictedHP(actor) / actor.HPMP.MaxHP;
}

public abstract class Targetxan(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    protected (Actor? Target, P Priority) FindBetterTargetBy<P>(Actor? initial, float maxDistanceFromPlayer, Func<Actor, P> prioFunc, Func<AIHints.Enemy, bool>? filterFunc = null) where P : struct, IComparable
    {
        var bestTarget = initial;
        var bestPrio = initial != null ? prioFunc(initial) : default;
        foreach (var enemy in Hints.PriorityTargets.Where(x =>
            x.Actor != initial &&
            Player.DistanceToHitbox(x.Actor) <= maxDistanceFromPlayer
            && (filterFunc == null || filterFunc(x))
        ))
        {
            var newPrio = prioFunc(enemy.Actor);
            if (newPrio.CompareTo(bestPrio) > 0)
            {
                bestPrio = newPrio;
                bestTarget = enemy.Actor;
            }
        }
        return (bestTarget, bestPrio);
    }
}

static class Extendxan
{
    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineShared(this RotationModuleDefinition def)
    {
        def.Define(SharedTrack.Targeting).As<Targeting>("Targeting")
            .AddOption(xan.Targeting.Manual, "Manual", "Use player's current target for all actions")
            .AddOption(xan.Targeting.Auto, "Auto", "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(xan.Targeting.AutoPrimary, "AutoPrimary", "Automatically select best target for AOE actions - ensure player target is hit")
            .AddOption(xan.Targeting.AutoTryPri, "AutoTryPri", "Automatically select best target for AOE actions - if player has a target, ensure that target is hit");

        def.Define(SharedTrack.AOE).As<AOEStrategy>("AOE")
            .AddOption(AOEStrategy.ST, "ST", "Use single-target actions")
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.ForceAOE, "ForceAOE", "Always use AOE actions, even on one target")
            .AddOption(AOEStrategy.ForceST, "ForceST", "Forbid any action that can hit multiple targets");

        return def.DefineSimple(SharedTrack.Buffs, "Buffs");
    }

    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineSimple<Index>(this RotationModuleDefinition def, Index track, string name) where Index : Enum
    {
        return def.Define(track).As<OffensiveStrategy>(name)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Use when optimal")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't use")
            .AddOption(OffensiveStrategy.Force, "Force", "Use ASAP");
    }

    public static AOEStrategy AOE(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
    public static Targeting Targeting(this StrategyValues strategy) => strategy.Option(SharedTrack.Targeting).As<Targeting>();
    public static OffensiveStrategy Simple<Index>(this StrategyValues strategy, Index track) where Index : Enum => strategy.Option(track).As<OffensiveStrategy>();
    public static bool BuffsOk(this StrategyValues strategy) => strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;
}
