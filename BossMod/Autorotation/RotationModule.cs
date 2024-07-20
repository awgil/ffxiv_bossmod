﻿namespace BossMod.Autorotation;

public enum RotationModuleQuality
{
    [PropertyDisplay("Work-in-progress - expect it to break a lot (or just be straight broken)")]
    WIP,

    [PropertyDisplay("Basic - expect the standard rotation to work, but not much more - suitable for leveling or casual content, expect green/blue logs")]
    Basic,

    [PropertyDisplay("OK - expect to execute reasonable actions in most circumstances, recover from deaths, have basic planning support - suitable for savages, expect blue/purple logs")]
    Ok,

    [PropertyDisplay("Good - expect to execute optimal actions in most cases, and planner should cover the remaining situations - suitable for all content, expect purple/orange logs")]
    Good,

    [PropertyDisplay("Excellent - expect to be able to get orange/pink logs consistently if you utilize planner correctly")]
    Excellent,

    Count
}

// the configuration part of the rotation module
// importantly, it defines constraints (supported classes and level ranges) and strategy configs (with their sets of possible options) used by the module to make its decisions
public sealed record class RotationModuleDefinition(string DisplayName, string Description, string Author, RotationModuleQuality Quality, BitMask Classes, int MaxLevel, int MinLevel = 1)
{
    public readonly BitMask Classes = Classes;
    public readonly List<StrategyConfig> Configs = [];

    public DefineRef Define<Index>(Index expectedIndex) where Index : Enum => new(Configs, (int)(object)expectedIndex);

    // unfortunately, c# doesn't support partial type inference, and forcing user to spell out track enum twice is obnoxious, so here's the hopefully cheap solution
    public readonly ref struct DefineRef(List<StrategyConfig> configs, int index)
    {
        public ConfigRef<Selector> As<Selector>(string internalName, string displayName = "", float uiPriority = 0) where Selector : Enum
        {
            if (configs.Count != index)
                throw new ArgumentException($"Unexpected index for {internalName}: expected {index}, cur size {configs.Count}");
            var config = new StrategyConfig(typeof(Selector), internalName, displayName, uiPriority);
            configs.Add(config);
            return new(config);
        }
    }

    public readonly ref struct ConfigRef<Index>(StrategyConfig config) where Index : Enum
    {
        public ConfigRef<Index> AddOption(Index expectedIndex, string internalName, string displayName = "", float cooldown = 0, float effect = 0, ActionTargets supportedTargets = ActionTargets.None,
            int minLevel = 1, int maxLevel = int.MaxValue, float defaultPriority = ActionQueue.Priority.Medium)
        {
            var idx = (int)(object)expectedIndex;
            if (config.Options.Count != idx)
                throw new ArgumentException($"Unexpected index value for {internalName}: expected {expectedIndex} ({idx}), got {config.Options.Count}");
            config.Options.Add(new(internalName, displayName)
            {
                Cooldown = cooldown,
                Effect = effect,
                SupportedTargets = supportedTargets,
                MinLevel = minLevel,
                MaxLevel = maxLevel,
                DefaultPriority = defaultPriority,
            });
            return this;
        }

        public ConfigRef<Index> AddAssociatedAction(ActionID aid)
        {
            config.AssociatedActions.Add(aid);
            return this;
        }

        public ConfigRef<Index> AddAssociatedActions<AID>(params AID[] aids) where AID : Enum
        {
            foreach (var aid in aids)
                config.AssociatedActions.Add(ActionID.MakeSpell(aid));
            return this;
        }
    }
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
    public abstract void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay);

    public virtual string DescribeState() => "";

    // utility to check action/trait unlocks
    public bool ActionUnlocked(ActionID action)
    {
        var def = ActionDefinitions.Instance[action];
        return def != null && def.AllowedClasses[(int)Player.Class] && Player.Level >= def.MinLevel && (ActionDefinitions.Instance.UnlockCheck?.Invoke(def.UnlockLink) ?? true);
    }

    public bool TraitUnlocked(uint id)
    {
        var trait = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Trait>(id);
        var unlock = trait?.Quest.Row ?? 0;
        var level = trait?.Level ?? 0;
        return Player.Level >= level && (ActionDefinitions.Instance.UnlockCheck?.Invoke(unlock) ?? true);
    }

    // utility to resolve the target overrides; returns null on failure - in this case module is expected to run smart-targeting logic
    // expected usage is `ResolveTargetOverride(strategy) ?? CustomSmartTargetingLogic(...)`
    protected Actor? ResolveTargetOverride(in StrategyValue strategy) => Manager.ResolveTargetOverride(strategy);

    // TODO: reconsider...
    protected unsafe T GetGauge<T>() where T : unmanaged
    {
        T res = default;
        ((ulong*)&res)[1] = World.Client.GaugePayload.Low;
        if (sizeof(T) > 16)
            ((ulong*)&res)[2] = World.Client.GaugePayload.High;
        return res;
    }
}
