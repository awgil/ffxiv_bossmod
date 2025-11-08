using BossMod.Data;
using System.Diagnostics.CodeAnalysis;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

[Renderer(typeof(TargetingRenderer))]
public enum Targeting
{
    Manual,
    Auto,
    AutoPrimary,
    AutoTryPri
}

[Renderer(typeof(OffensiveStrategyRenderer))]
public enum OffensiveStrategy
{
    Automatic,
    Delay,
    Force
}

public enum AOEStrategy
{
    AOE,
    ST,
    ForceAOE,
    ForceST
}

public enum SharedTrack { Targeting, AOE, Buffs, Count }

public abstract class AttackxanOld<AID, TraitID>(RotationModuleManager manager, Actor player, PotionType potType = PotionType.None) : Basexan<AID, TraitID>(manager, player, potType)
    where AID : struct, Enum
    where TraitID : Enum
{
    protected sealed override float GCDLength => AttackGCDLength;
}

public abstract class CastxanOld<AID, TraitID>(RotationModuleManager manager, Actor player, PotionType potType = PotionType.None) : Basexan<AID, TraitID>(manager, player, potType)
    where AID : struct, Enum
    where TraitID : Enum
{
    protected sealed override float GCDLength => SpellGCDLength;
}

public abstract class Basexan<AID, TraitID>(RotationModuleManager manager, Actor player, PotionType potType) : Basexan<AID, TraitID, StrategyValues>(manager, player, potType)
    where AID : struct, Enum
    where TraitID : Enum
{
    protected sealed override StrategyValues FromValues(StrategyValues strategy) => strategy;
}

public abstract class Castxan<AID, TraitID, TValues>(RotationModuleManager manager, Actor player, PotionType potType = PotionType.None) : Basexan<AID, TraitID, TValues>(manager, player, potType)
    where AID : struct, Enum
    where TraitID : Enum
    where TValues : struct
{
    protected sealed override float GCDLength => SpellGCDLength;
}

public abstract class Basexan<AID, TraitID, TValues>(RotationModuleManager manager, Actor player, PotionType potType) : TypedRotationModule<TValues>(manager, player)
    where AID : struct, Enum
    where TraitID : Enum
    where TValues : struct
{
    public PotionType PotionType { get; init; } = potType;

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
    protected float PotionLeft { get; private set; }

    protected float? CountdownRemaining => World.Client.CountdownRemaining;
    protected float AnimLock => World.Client.AnimationLock;

    protected float AttackGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    protected float SpellGCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);

    protected float ReadyIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    protected float MaxChargesIn(AID action) => Unlocked(action) ? ActionDefinitions.Instance.Spell(action)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;

    protected float DutyActionReadyIn<ID>(ID aid) where ID : Enum => DutyActionReadyIn(ActionID.MakeSpell(aid));

    protected float DutyActionReadyIn(ActionID aid)
    {
        if (World.Client.DutyActions.Any(d => d.Action == aid))
            return ActionDefinitions.Instance[aid]!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);
        return float.MaxValue;
    }

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

    public const float DefaultOGCDPriority = ActionQueue.Priority.Low + 1;
    public const float DefaultGCDPriority = ActionQueue.Priority.High + 2;

    protected AID ComboLastMove => (AID)(object)World.Client.ComboState.Action;

    // override if some action requires specific runtime checks that aren't covered by the existing framework code
    protected virtual bool CanUse(AID action) => true;

    protected void PushGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => PushGCD(aid, target, (int)(object)priority, delay);

    protected void PushGCD<P>(AID aid, Enemy? target, P priority, float delay = 0, bool useOnDyingTarget = true) where P : Enum
    {
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden)
            return;

        if (!useOnDyingTarget && target?.Priority is Enemy.PriorityPointless)
            return;

        PushGCD(aid, target?.Actor, (int)(object)priority, delay);
    }

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

    protected void PushOGCD<P>(AID aid, Enemy? target, P priority, float delay = 0, bool useOnDyingTarget = true) where P : Enum
    {
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden)
            return;

        if (!useOnDyingTarget && target?.Priority is Enemy.PriorityPointless)
            return;

        PushOGCD(aid, target?.Actor, (int)(object)priority, delay);
    }

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
        SelectPrimaryTarget(strategy.Option(SharedTrack.Targeting).As<Targeting>(), ref primaryTarget, range);
    }

    protected void SelectPrimaryTarget(Targeting t, ref Enemy? primaryTarget, float range)
    {
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

    protected (Enemy? Best, int Targets) SelectTarget(
        Targeting targeting,
        AOEStrategy aoe,
        Enemy? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(targeting, aoe, primaryTarget, range, isInAOE, (numTargets, _) => numTargets, a => a);

    protected (Enemy? Best, int Targets) SelectTargetByHP(StrategyValues strategy, Enemy? primaryTarget, float range, PositionCheck isInAOE)
        => SelectTarget(strategy, primaryTarget, range, isInAOE, (numTargets, actor) => (numTargets, numTargets > 2 ? actor.HPMP.CurHP : 0), args => args.numTargets);

    protected (Enemy? Best, int Priority) SelectTarget<P>(
        StrategyValues strategy,
        Enemy? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize,
        Func<P, int> simplify
    ) where P : struct, IComparable => SelectTarget(strategy.Targeting(), strategy.AOE(), primaryTarget, range, isInAOE, prioritize, simplify);

    protected (Enemy? Best, int Priority) SelectTarget<P>(
        Targeting targeting,
        AOEStrategy aoe,
        Enemy? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize,
        Func<P, int> simplify
    ) where P : struct, IComparable
    {
        var targetOutOfCombat = primaryTarget?.Priority == Enemy.PriorityUndesirable;

        P targetPrio(Actor potentialTarget)
        {
            var numForbidden = Hints.ForbiddenTargets.Count(enemy => isInAOE(potentialTarget, enemy.Actor));
            var numOk = Hints.PriorityTargets.Count(enemy => isInAOE(potentialTarget, enemy.Actor));

            var numTargets = targetOutOfCombat && numForbidden == 1 && numOk == 0
                // primary target will be the only one hit by aoe so they are ok to target
                ? 1
                : numForbidden > 0
                    // unwanted targets will be hit
                    ? 0
                    // wanted targets will be hit
                    : numOk;

            return prioritize(AdjustNumTargets(aoe, numTargets), potentialTarget);
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
    protected (Enemy? Target, P Timer) SelectDotTarget<P>(StrategyValues strategy, Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable => SelectDotTarget(strategy.Targeting(), initial, getTimer, maxAllowedTargets);

    protected (Enemy? Target, P Timer) SelectDotTarget<P>(Targeting targeting, Enemy? initial, Func<Actor?, P> getTimer, int maxAllowedTargets) where P : struct, IComparable
    {
        var forbidden = initial?.ForbidDOTs ?? false;
        switch (targeting)
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
        var initialTimer = forbidden ? getTimer(null) : getTimer(initial?.Actor);
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

    protected void GoalZoneCombined(StrategyValues strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null) => GoalZoneCombined(strategy.AOE(), range, fAoe, firstUnlockedAoeAction, minAoe, maximumActionRange);

    protected void GoalZoneCombined(AOEStrategy strategy, float range, Func<WPos, float> fAoe, AID firstUnlockedAoeAction, int minAoe, float? maximumActionRange = null)
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

    protected int NumNearbyTargets(StrategyValues strategy, float range) => NumNearbyTargets(strategy.AOE(), range);
    protected int NumNearbyTargets(AOEStrategy aoe, float range) => AdjustNumTargets(aoe, Hints.NumPriorityTargetsInAOECircle(Player.Position, range));

    protected int AdjustNumTargets(StrategyValues strategy, int reported) => AdjustNumTargets(strategy.AOE(), reported);

    protected int AdjustNumTargets(AOEStrategy aoe, int reported)
        => reported == 0 ? 0 : aoe switch
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
    protected virtual float GetCastTime(AID aid)
    {
        var def = ActionDefinitions.Instance.Spell(aid)!;
        var hasteMod = GCDLength / 2.5f;

        if (SwiftcastLeft > GCD && def.Category is ActionCategory.Spell)
            return 0;

        return def.CastTime * hasteMod;
    }

    protected float NextCastStart => AnimLock > GCD ? AnimLock + AnimationLockDelay : GCD;

    protected float GetSlidecastTime(AID aid) => MathF.Max(0, GetCastTime(aid) - 0.5f);
    protected float GetSlidecastEnd(AID aid) => NextCastStart + GetSlidecastTime(aid);

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    protected Positional GetCurrentPositional(Actor target) => target.Omnidirectional
        ? Positional.Any
        : (Player.Position - target.Position).Normalized().Dot(target.Rotation.ToDirection()) switch
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
        if (
            // positionals irrelevant
            target is { Omnidirectional: true }
            // enemy is targeting us and is not busy casting, so we assume they will turn to face the player
            // (excluding striking dummies, which don't move)
            || target is { TargetID: var t, CastInfo: null, IsStrikingDummy: false } && t == Player.InstanceID
        )
            positional = (Positional.Any, false);

        NextPositionalImminent = !trueNorth && positional.imm;
        NextPositionalCorrect = trueNorth || target == null || positional.pos switch
        {
            Positional.Flank => MathF.Abs(target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized())) < 0.7071067f,
            Positional.Rear => target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized()) < -0.7071068f,
            // the only Front positional is Goblin Punch, used by BLU, who can't use True North anyway, so it's irrelevant
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

    protected override TValues FromValues(StrategyValues strategy) => ValueConverter.FromValues<TValues>(strategy);

    public sealed override void Execute(TValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        IsMoving = isMoving;
        NextGCD = default;
        NextGCDPrio = 0;
        PlayerTarget = Hints.FindEnemy(primaryTarget);

        PretendCountdown();

        var pelo = Player.FindStatus(ClassShared.SID.Peloton);
        PelotonLeft = pelo != null ? StatusDuration(pelo.Value.ExpireAt) : 0;
        SwiftcastLeft = Utils.MaxAll(StatusLeft(ClassShared.SID.Swiftcast), StatusLeft(ClassShared.SID.LostChainspell), StatusLeft(PhantomSID.OccultQuick));
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

        MP = (uint)Math.Clamp(Player.PendingMPRaw, 0, Player.HPMP.MaxMP);

        if (_cdLockout > World.CurrentTime)
            return;

        if (Player.FindStatus(49) is ActorStatus st && Food.GetPotionType(st.Extra) == PotionType)
            PotionLeft = StatusDuration(st.ExpireAt);
        else
            PotionLeft = 0;

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
            if (CombatTimer < 7.8f && World.Party.WithoutSlot(includeDead: true, excludeAlliance: true, excludeNPCs: true).Skip(1).Any(HavePartyBuff))
                buffsIn = 7.8f - CombatTimer;
            else
                // no party members with raid buffs, assume we're never getting any
                buffsIn = float.MaxValue;
        }

        return (Bossmods.RaidCooldowns.DamageBuffLeft(Player, primaryTarget), buffsIn.Value);
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

    public abstract void Exec(TValues strategy, Enemy? primaryTarget);

    protected (float Left, int Stacks) Status<SID>(SID status, float? pendingDuration = null) where SID : Enum => Player.FindStatus(status, pendingDuration == null ? null : World.FutureTime(pendingDuration.Value)) is ActorStatus s ? (StatusDuration(s.ExpireAt), s.Extra & 0xFF) : (0, 0);
    protected float StatusLeft<SID>(SID status, float? pendingDuration = null) where SID : Enum => Status(status, pendingDuration).Left;
    protected int StatusStacks<SID>(SID status, float? pendingDuration = null) where SID : Enum => Status(status, pendingDuration).Stacks;

    protected float HPRatio(Actor actor) => (float)actor.HPMP.CurHP / Player.HPMP.MaxHP;
    protected float HPRatio() => HPRatio(Player);

    protected uint PredictedHP(Actor actor) => (uint)actor.PendingHPClamped;
    protected float PredictedHPRatio(Actor actor) => (float)PredictedHP(actor) / actor.HPMP.MaxHP;
}

static class Extendxan
{
    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineShared(this RotationModuleDefinition def, string buffTrackName)
    {
        return def.DefineSharedTA().DefineSimple(SharedTrack.Buffs, "Buffs", displayName: buffTrackName, uiPriority: 498, renderer: typeof(OffensiveStrategyRenderer));
    }

    public static RotationModuleDefinition DefineSharedTA(this RotationModuleDefinition def)
    {
        def.Define(SharedTrack.Targeting).As<Targeting>("Targeting", uiPriority: 500, renderer: typeof(TargetingRenderer))
            .AddOption(xan.Targeting.Manual, "Use player's current target for all actions")
            .AddOption(xan.Targeting.Auto, "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(xan.Targeting.AutoPrimary, "Automatically select best target for AOE actions - ensure player target is hit")
            .AddOption(xan.Targeting.AutoTryPri, "Automatically select best target for AOE actions - if player has a target, ensure that target is hit");

        def.Define(SharedTrack.AOE).As<AOEStrategy>("AOE", uiPriority: 499)
            .AddOption(AOEStrategy.AOE, "Use AOE rotation if beneficial")
            .AddOption(AOEStrategy.ST, "Use single-target rotation")
            .AddOption(AOEStrategy.ForceAOE, "Always use AOE rotation, even on one target")
            .AddOption(AOEStrategy.ForceST, "Use single-target rotation; do not use ANY actions that hit multiple targets");

        return def;
    }

    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineSimple<Index>(this RotationModuleDefinition def, Index track, string name, string displayName = "", int minLevel = 1, float uiPriority = 0, Type? renderer = null) where Index : Enum
    {
        return def.Define(track).As<OffensiveStrategy>(name, displayName, uiPriority: uiPriority, renderer: renderer ?? typeof(OffensiveStrategyRenderer))
            .AddOption(OffensiveStrategy.Automatic, "Use when optimal", minLevel: minLevel)
            .AddOption(OffensiveStrategy.Delay, "Don't use", minLevel: minLevel)
            .AddOption(OffensiveStrategy.Force, "Use ASAP", minLevel: minLevel);
    }

    public static AOEStrategy AOE(this StrategyValues strategy) => strategy.Option(SharedTrack.AOE).As<AOEStrategy>();
    public static Targeting Targeting(this StrategyValues strategy) => strategy.Option(SharedTrack.Targeting).As<Targeting>();
    public static OffensiveStrategy Simple<Index>(this StrategyValues strategy, Index track) where Index : Enum => strategy.Option(track).As<OffensiveStrategy>();
    public static bool BuffsOk(this StrategyValues strategy) => strategy.Option(SharedTrack.Buffs).As<OffensiveStrategy>() != OffensiveStrategy.Delay;
    public static bool AOEOk(this StrategyValues strategy) => strategy.AOE().AOEOk();
    public static bool AOEOk(this AOEStrategy aoe) => aoe is AOEStrategy.AOE or AOEStrategy.ForceAOE;
    public static float DistanceToHitbox(this Actor actor, Enemy? other) => actor.DistanceToHitbox(other?.Actor);
}
