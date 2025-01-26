namespace BossMod.Autorotation;

public sealed class ClassNINUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ShadeShift = SharedTrack.Count, Shukuchi }
    public enum DashStrategy { None, GapClose, GapCloseHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(NIN.AID.Chimatsuri);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: NIN", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.NIN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ShadeShift, "Shade", "", 400, NIN.AID.ShadeShift, 20);

        res.Define(Track.Shukuchi).As<DashStrategy>("Shukuchi", "Dash", 20)
            .AddOption(DashStrategy.None, "Automatic", "No use.")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use Shukuchi as gapcloser if outside melee range of any target; will dash to target selected via CDPlanner. If none selected, will dash to current target or not at all if no target", 60, 0, ActionTargets.All, 40)
            .AddOption(DashStrategy.GapCloseHold1, "GapCloseHold1", "Use Shukuchi as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.All, 76)
            .AddAssociatedActions(NIN.AID.Shukuchi);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ShadeShift), NIN.AID.ShadeShift, Player);

        var dash = strategy.Option(Track.Shukuchi);
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget;
        var dashStrategy = strategy.Option(Track.Shukuchi).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, primaryTarget))
            QueueOGCD(NIN.AID.Shukuchi, dashTarget);

    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.GapClose => Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20,
        DashStrategy.GapCloseHold1 => Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20 && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRG.AID.WingedGlide)!.MainCooldownGroup].Remaining <= 59.9f,
        _ => false,
    };

    #region Core Execution Helpers

    public NIN.AID NextGCD; //Next global cooldown action to be used
    public void QueueGCD<P>(NIN.AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueGCD(aid, target, (int)(object)priority, delay);

    public void QueueGCD(NIN.AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;

        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }

    public void QueueOGCD<P>(NIN.AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);

    public void QueueOGCD(NIN.AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }

    public bool QueueAction(NIN.AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
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

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
        return true;
    }
    #endregion

}
