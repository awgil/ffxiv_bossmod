namespace BossMod.Autorotation;

// base class that simplifies implementation of utility modules - these are really only useful for planning support
public abstract class GenericUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum SimpleOption { None, Use }
    public enum LBOption { None, LB3, LB2, LB1, LB2Only, LB1Only, LB12 }

    protected static void DefineSimpleConfig<Index, AID>(RotationModuleDefinition def, Index expectedIndex, string internalName, string displayName, int uiPriority, AID aid, float effect = 0)
        where Index : Enum
        where AID : Enum
    {
        var adefs = ActionDefinitions.Instance;
        var action = ActionID.MakeSpell(aid);
        var adata = adefs[action]!;
        var cfg = def.AddConfig(expectedIndex, new(internalName, displayName, uiPriority));
        cfg.AddOption(SimpleOption.None, new(0x80ffffff, ActionTargets.None, "None", "Do not use automatically"));
        cfg.AddOption(SimpleOption.Use, new(0x8000ffff, adata.AllowedTargets, "Use", $"Use {action.Name()}", adata.Cooldown, effect, adefs.MinActionLevel(action)));
    }

    protected static void DefineLimitBreak<Index>(RotationModuleDefinition def, Index expectedIndex, ActionTargets allowedTargets, float effectLB1 = 0, float effectLB2 = 0, float effectLB3 = 0) where Index : Enum
    {
        // note: it assumes that effect durations are either 0's or correspond to tank LB (so lb2 > lb1 > lb3)
        var lb = def.AddConfig(expectedIndex, new("LB"));
        lb.AddOption(LBOption.None, new(0x80ffffff, ActionTargets.None, "None", "Do not use automatically"));
        lb.AddOption(LBOption.LB3, new(0x8000ff00, allowedTargets, "LB3", "Use LB3 if available", Effect: effectLB3));
        lb.AddOption(LBOption.LB2, new(0x8000ff40, allowedTargets, "LB2+", "Use LB2/3 if available", Effect: effectLB3));
        lb.AddOption(LBOption.LB1, new(0x8000ff80, allowedTargets, "LB1+", "Use any LB if available", Effect: effectLB3));
        lb.AddOption(LBOption.LB2Only, new(0x80ffff00, allowedTargets, "LB2", "Use LB2 if available, but not LB3", Effect: effectLB2));
        lb.AddOption(LBOption.LB1Only, new(0x80ffff40, allowedTargets, "LB1", "Use LB1 if available, but not LB2+", Effect: effectLB1));
        lb.AddOption(LBOption.LB12, new(0x80ffff80, allowedTargets, "LB1/2", "Use LB1/2 if available, but not LB3", Effect: effectLB1));
    }

    protected void ExecuteSimple<AID>(in StrategyValue value, AID aid, Actor? target) where AID : Enum
    {
        if ((SimpleOption)value.Option == SimpleOption.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(value) ?? target, value.Priority(ActionQueue.Priority.Low));
    }

    // returns 0 if not needed, or current LB level
    protected int LBLevelToExecute(in StrategyValue value)
    {
        // note: limit break's animation lock is very long, so we're guaranteed to delay next gcd at least a bit => priority has to be higher than gcd
        // note: while we could arguably delay that until right after next gcd, it's too risky, let the user deal with it by planning carefully...
        if ((LBOption)value.Option == LBOption.None)
            return 0;
        var curLevel = World.Party.LimitBreakMax > 0 ? World.Party.LimitBreakCur / World.Party.LimitBreakMax : 0;
        return (LBOption)value.Option switch
        {
            LBOption.LB3 => curLevel == 3,
            LBOption.LB2 => curLevel >= 2,
            LBOption.LB1 => curLevel >= 1,
            LBOption.LB2Only => curLevel == 2,
            LBOption.LB1Only => curLevel == 1,
            LBOption.LB12 => curLevel is 1 or 2,
            _ => false
        } ? curLevel : 0;
    }
}
