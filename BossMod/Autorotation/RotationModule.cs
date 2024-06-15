namespace BossMod.Autorotation;

// the configuration part of the rotation module
// importantly, it defines constraints (supported classes and level ranges) and strategy configs (with their sets of possible options) used by the module to make its decisions
public sealed record class RotationModuleDefinition(string DisplayName, string Description, BitMask Classes, int MaxLevel, int MinLevel = 1)
{
    public readonly BitMask Classes = Classes;
    public readonly List<StrategyConfig> Configs = [];

    public StrategyConfig AddConfig<Index>(Index expectedIndex, StrategyConfig config) where Index : Enum
    {
        if (Configs.Count != (int)(object)expectedIndex)
            throw new ArgumentException($"Unexpected index for {config.InternalName}: expected {expectedIndex} ({(int)(object)expectedIndex}), got {Configs.Count}");
        Configs.Add(config);
        return config;
    }
}

// base class for rotation modules
// each rotation module should contain a `public static RotationModuleDefinition Definition()` function
public abstract record class RotationModule(WorldState World, Actor Player, AIHints Hints)
{
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();

    // the main entry point of the module - given a set of strategy values, fill the queue with a set of actions to execute
    public abstract void Execute(ReadOnlySpan<StrategyValue> strategy, Actor? primaryTarget, ActionQueue actions);

    // utility to resolve the target overrides; returns null on failure - in this case module is expected to run smart-targeting logic
    // expected usage is `ResolveTargetOverride(strategy) ?? CustomSmartTargetingLogic(...)`
    protected Actor? ResolveTargetOverride(in StrategyValue strategy) => strategy.Target switch
    {
        StrategyTarget.Self => Player,
        StrategyTarget.PartyByAssignment => _prc.SlotsPerAssignment(World.Party) is var spa && strategy.TargetParam < spa.Length ? World.Party[spa[strategy.TargetParam]] : null,
        StrategyTarget.PartyWithLowestHP => World.Party.WithoutSlot().Exclude(strategy.TargetParam != 0 ? null : Player).MinBy(a => a.HPMP.CurHP),
        StrategyTarget.EnemyWithHighestPriority => Hints.PriorityTargets.MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor,
        StrategyTarget.EnemyByOID => (uint)strategy.TargetParam is var oid && oid != 0 ? Hints.PotentialTargets.Where(e => e.Actor.OID == oid).MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null,
        _ => null
    };
}
