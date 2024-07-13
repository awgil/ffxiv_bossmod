using BossMod.Autorotation.Legacy;
using static BossMod.Autorotation.StrategyValues;

namespace BossMod.Autorotation.xan;

// frick you i'll name my class whatever i want
#pragma warning disable CS8981
#pragma warning disable IDE1006

public enum Targeting { Auto, Manual, AutoPrimary }
public enum OffensiveStrategy { Automatic, Delay, Force }
public enum AOEStrategy { AOE, SingleTarget }

public abstract class xbase<AID, TraitID> : LegacyModule where AID : Enum where TraitID : Enum
{
    public class State(RotationModule module) : CommonState(module) { }

    protected State _state;

    protected float PelotonLeft { get; private set; }
    protected float SwiftcastLeft { get; private set; }
    protected float TrueNorthLeft { get; private set; }

    protected xbase(RotationModuleManager manager, Actor player) : base(manager, player)
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

    protected void QueueOGCD(Action<float, float> ogcdFun)
    {
        var deadline = _state.GCD > 0 ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength))
            ogcdFun(deadline - _state.OGCDSlotLength, deadline);
        if (_state.CanWeave(deadline))
            ogcdFun(deadline, deadline);
    }

    /// <summary>
    /// If the user's current target is more than <paramref name="range"/> yalms from the player, this function attempts to find a closer one. No prioritization is done; if any target is returned, it is simply the actor that was earliest in the object table.<br/>
    ///
    /// It is guaranteed that <paramref name="primaryTarget"/> will be set to either <c>null</c> or an attackable enemy (not, for example, an ally or event object).<br/>
    ///
    /// If the provided Targeting strategy is Manual, this function is otherwise a <strong>no-op</strong>.
    /// </summary>
    /// <param name="track">Reference to the Targeting track of the active strategy</param>
    /// <param name="primaryTarget">Player's current target - may be null</param>
    /// <param name="range">Maximum distance from the player to search for a candidate target</param>
    protected void SelectPrimaryTarget(OptionRef track, ref Actor? primaryTarget, float range)
    {
        if (!IsEnemy(primaryTarget))
            primaryTarget = null;

        var tars = track.As<Targeting>();
        if (tars == Targeting.Manual)
        {
            return;
        }

        if (Player.DistanceToHitbox(primaryTarget) > range)
        {
            primaryTarget = Hints.PriorityTargets.Where(x => x.Actor.DistanceToHitbox(Player) <= range).MaxBy(x => x.Actor.HPMP.CurHP)?.Actor;
            // Hints.ForcedTarget = primaryTarget;
        }
    }

    protected virtual float GetCastTime(AID aid) => SwiftcastLeft > _state.GCD ? 0 : ActionDefinitions.Instance.Spell(aid)!.CastTime * _state.SpellGCDTime / 2.5f;

    protected bool CanCast(AID aid) => GetCastTime(aid) <= ForceMovementIn;

    protected float ForceMovementIn => Manager.ActionManager.InputOverride.IsMoveRequested() ? 0 : Hints.ForceMovementIn;

    protected bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    protected bool Unlocked(TraitID tid) => TraitUnlocked((uint)(object)tid);

    private static bool IsEnemy(Actor? actor) => actor != null && actor.Type is ActorType.Enemy or ActorType.Part && !actor.IsAlly;

    protected (Actor? Best, int Priority) SelectTarget(
        OptionRef track,
        Actor? primaryTarget,
        float range,
        Func<Actor, Actor, bool> isInAOE
    ) => SelectTarget(track, primaryTarget, range, isInAOE, (numTargets, _) => numTargets);

    protected (Actor? Best, P Priority) SelectTarget<P>(
        OptionRef track,
        Actor? primaryTarget,
        float range,
        Func<Actor, Actor, bool> isInAOE,
        Func<int, Actor, P> prioFunc
    ) where P : struct, IComparable
    {
        P targetPrio(Actor a) => prioFunc(Hints.NumPriorityTargetsInAOE(enemy => isInAOE(a, enemy.Actor)), a);

        return track.As<Targeting>() switch
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

    protected bool IsSplashTarget(Actor primary, Actor other) => Hints.TargetInAOECircle(other, primary.Position, 5);

    protected int NumMeleeAOETargets() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    protected bool Is25yRectTarget(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, (primary.Position - Player.Position).Normalized(), 25, 4);

    public sealed override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        var pelo = Player.FindStatus(BRD.SID.Peloton);
        PelotonLeft = pelo != null ? _state.StatusDuration(pelo.Value.ExpireAt) : 0;
        SwiftcastLeft = StatusLeft(WHM.SID.Swiftcast);
        TrueNorthLeft = StatusLeft(DRG.SID.TrueNorth);

        _state.AnimationLockDelay = MathF.Max(0.1f, _state.AnimationLockDelay);

        Exec(strategy, primaryTarget);
    }

    public abstract void Exec(StrategyValues strategy, Actor? primaryTarget);

    protected (float Left, int Stacks) Status<SID>(SID status) where SID : Enum => _state.StatusDetails(Player, status, Player.InstanceID);
    protected float StatusLeft<SID>(SID status) where SID : Enum => Status(status).Left;
    protected int StatusStacks<SID>(SID status) where SID : Enum => Status(status).Stacks;
}

static class xtensions
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
