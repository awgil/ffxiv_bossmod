using BossMod.Autorotation.Legacy;

namespace BossMod.Autorotation.akechi;

public enum Targeting { Auto, Manual, AutoPrimary }
public enum OffensiveStrategy { Automatic, Delay, Force }
public enum AOEStrategy { AOE, SingleTarget }

public abstract class Baseakechi<AID, TraitID> : LegacyModule where AID : Enum where TraitID : Enum
{
    public class State(RotationModule module) : CommonState(module) { }

    protected State _state;

    protected float SwiftcastLeft { get; private set; }
    protected float TrueNorthLeft { get; private set; }
    protected float CombatTimer { get; private set; }

    protected Baseakechi(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    protected void PushGCD(AID aid, Actor? target, int additionalPrio = 0)
        => PushAction(aid, target, ActionQueue.Priority.High + 500 + additionalPrio);

    protected void PushOGCD(AID aid, Actor? target, int additionalPrio = 0)
        => PushAction(aid, target, ActionQueue.Priority.Low + 500 + additionalPrio);

    protected void PushAction(AID aid, Actor? target, float priority)
    {
        if (!CanCast(aid))
            return;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return;

        if (def.Range != 0 && target == null)
        {
            // Service.Log($"Queued targeted action ({aid}) with no target");
            return;
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority);
    }

    protected void QueueOGCD(Action<float> oGCDFun)
    {
        var deadline = _state.GCD > 0 ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength))
            oGCDFun(deadline - _state.OGCDSlotLength);
        if (_state.CanWeave(deadline))
            oGCDFun(deadline);
    }

    /// <summary>
    /// Tries to select a suitable primary target.<br/>
    ///
    /// If the provided <paramref name="primaryTarget"/> is null, an NPC, or non-enemy object; it will be reset to <c>null</c>.<br/>
    ///
    /// Additionally, if <paramref name="range"/> is set to <c>Targeting.Auto</c>, and the user's current target is more than <paramref name="range"/> yalms from the player, this function attempts to find a closer one. No prioritization is done; if any target is returned, it is simply the actor that was earliest in the object table.
    /// </summary>
    /// <param name="strategy">Targeting strategy</param>
    /// <param name="primaryTarget">Player's current target - may be null</param>
    /// <param name="range">Maximum distance from the player to search for a candidate target</param>
    protected void SelectPrimaryTarget(Targeting strategy, ref Actor? primaryTarget, float range)
    {
        if (!IsEnemy(primaryTarget))
            primaryTarget = null;

        if (strategy != Targeting.Auto)
            return;

        if (Player.DistanceToHitbox(primaryTarget) > range)
        {
            primaryTarget = Hints.PriorityTargets.Where(x => x.Actor.DistanceToHitbox(Player) <= range).MaxBy(x => x.Actor.HPMP.CurHP)?.Actor;
            // Hints.ForcedTarget = primaryTarget;
        }
    }

    /// <summary>
    /// Get <em>effective</em> cast time for the provided action.<br/>
    /// The default implementation returns the action's base cast time multiplied by the player's spell-speed factor, which accounts for haste buffs (like Leylines) and slow debuffs. It also accounts for Swiftcast.<br/>
    /// Subclasses should handle job-specific cast speed adjustments, such as RDM's Dualcast or PCT's motifs.
    /// </summary>
    /// <param name="aid"></param>
    /// <returns></returns>
    protected virtual float GetCastTime(AID aid) => SwiftcastLeft > _state.GCD ? 0 : ActionDefinitions.Instance.Spell(aid)!.CastTime * _state.SpellGCDTime / 2.5f;

    protected bool CanCast(AID aid) => GetCastTime(aid) <= ForceMovementIn;

    protected float ForceMovementIn;

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    private static bool IsEnemy(Actor? actor) => actor != null && actor.Type is ActorType.Enemy or ActorType.Part && !actor.IsAlly;

    protected delegate bool PositionCheck(Actor playerTarget, Actor targetToTest);
    protected delegate P PriorityFunc<P>(int totalTargets, Actor primaryTarget);

    protected (Actor? Best, int Targets) SelectTarget(
        Targeting track,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE
    ) => SelectTarget(track, primaryTarget, range, isInAOE, (numTargets, _) => numTargets);

    protected (Actor? Best, P Priority) SelectTarget<P>(
        Targeting track,
        Actor? primaryTarget,
        float range,
        PositionCheck isInAOE,
        PriorityFunc<P> prioritize
    ) where P : struct, IComparable
    {
        P targetPrio(Actor potentialTarget) => prioritize(Hints.NumPriorityTargetsInAOE(enemy => isInAOE(potentialTarget, enemy.Actor)), potentialTarget);

        return track switch
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
    }

    protected int NumMeleeAOETargets() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    public sealed override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        SwiftcastLeft = StatusLeft(WHM.SID.Swiftcast);
        TrueNorthLeft = StatusLeft(DRG.SID.TrueNorth);

        _state.AnimationLockDelay = MathF.Max(0.1f, _state.AnimationLockDelay);
        ForceMovementIn = forceMovementIn;

        CombatTimer = (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;

        Exec(strategy, primaryTarget, estimatedAnimLockDelay);
    }

    public abstract void Exec(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay);
    protected (float Left, int Stacks) Status<SID>(SID status) where SID : Enum => _state.StatusDetails(Player, status, Player.InstanceID);
    protected float StatusLeft<SID>(SID status) where SID : Enum => Status(status).Left;
    protected int StatusStacks<SID>(SID status) where SID : Enum => Status(status).Stacks;
}

static class Extensionsakechi
{
    public static RotationModuleDefinition.ConfigRef<Targeting> DefineTargeting<Index>(this RotationModuleDefinition def, Index trackname)
         where Index : Enum
    {
        return def.Define(trackname).As<Targeting>("Targeting")
            .AddOption(Targeting.Auto, "Auto", "Automatically select best target (highest number of nearby targets) for AOE actions")
            .AddOption(Targeting.Manual, "Manual", "Use player's current target for all actions")
            .AddOption(Targeting.AutoPrimary, "AutoPrimary", "Automatically select best target for AOE actions - ensure player target is hit");
    }

    public static RotationModuleDefinition.ConfigRef<AOEStrategy> DefineAOE<Index>(this RotationModuleDefinition def, Index trackname) where Index : Enum
    {
        return def.Define(trackname).As<AOEStrategy>("AOE")
            .AddOption(AOEStrategy.AOE, "AOE", "Use AOE actions if beneficial")
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target actions");
    }

    public static RotationModuleDefinition.ConfigRef<OffensiveStrategy> DefineSimple<Index>(this RotationModuleDefinition def, Index track, string name) where Index : Enum
    {
        return def.Define(track).As<OffensiveStrategy>(name)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Use when optimal")
            .AddOption(OffensiveStrategy.Delay, "Delay", "Don't use")
            .AddOption(OffensiveStrategy.Force, "Force", "Use ASAP");
    }
}
