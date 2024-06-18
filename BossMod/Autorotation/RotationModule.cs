namespace BossMod.Autorotation;

// the configuration part of the rotation module
// importantly, it defines constraints (supported classes and level ranges) and strategy configs (with their sets of possible options) used by the module to make its decisions
public sealed record class RotationModuleDefinition(string DisplayName, string Description, BitMask Classes, int MaxLevel, int MinLevel = 1)
{
    public readonly BitMask Classes = Classes;
    public readonly List<StrategyConfig> Configs = [];

    // unfortunately, c# doesn't support partial type inference, and forcing user to spell out track enum twice is obnoxious, so here's the hopefully cheap solution
    public readonly ref struct DefineRef(List<StrategyConfig> configs, int index)
    {
        public StrategyConfig As<Selector>(string internalName, string displayName = "", float uiPriority = 0) where Selector : Enum
        {
            if (configs.Count != index)
                throw new ArgumentException($"Unexpected index for {internalName}: expected {index}, cur size {configs.Count}");
            var config = new StrategyConfig(typeof(Selector), internalName, displayName, uiPriority);
            configs.Add(config);
            return config;
        }
    }

    public DefineRef Define<Index>(Index expectedIndex) where Index : Enum => new(Configs, (int)(object)expectedIndex);
}

// base class for rotation modules
// each rotation module should contain a `public static RotationModuleDefinition Definition()` function
// TODO: i don't think it should know about manager, rework this...
public abstract class RotationModule(RotationModuleManager manager, Actor player)
{
    public readonly RotationModuleManager Manager = manager;
    public readonly Actor Player = player;
    public BossModuleManager Bossmods => Manager.Bossmods;
    public WorldState World => Manager.Bossmods.WorldState;
    public AIHints Hints => Manager.Hints;

    // the main entry point of the module - given a set of strategy values, fill the queue with a set of actions to execute
    public abstract void Execute(StrategyValues strategy, Actor? primaryTarget);

    public virtual string DescribeState() => "";

    // utility to check action/trait unlocks
    public bool ActionUnlocked(ActionID action)
    {
        var def = ActionDefinitions.Instance[action];
        return def != null && def.AllowedClasses[(int)Player.Class] && Player.Level >= def.MinLevel && (ActionDefinitions.Instance.UnlockCheck?.Invoke(def.UnlockLink) ?? true);
    }

    public bool TraitUnlocked(uint id)
    {
        var unlock = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Trait>(id)?.Quest.Row ?? 0;
        return ActionDefinitions.Instance.UnlockCheck?.Invoke(unlock) ?? true;
    }

    // utility to resolve the target overrides; returns null on failure - in this case module is expected to run smart-targeting logic
    // expected usage is `ResolveTargetOverride(strategy) ?? CustomSmartTargetingLogic(...)`
    protected Actor? ResolveTargetOverride(in StrategyValue strategy) => Manager.ResolveTargetOverride(strategy);
}
