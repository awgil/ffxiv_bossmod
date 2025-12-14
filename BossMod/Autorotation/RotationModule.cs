using System.Reflection;

namespace BossMod.Autorotation;

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

public enum RotationModuleOrder
{
    [PropertyDisplay("[1] High-level strategy module. Responsible for targeting and enemy prioritization.")]
    HighLevel = 1,

    [PropertyDisplay("[2] Standard rotation/utility module. Responsible for deciding which actions to use and setting up goal zones.")]
    Actions = 2,

    [PropertyDisplay("[3] Movement module. Responsible for pathfinding and executing movement.")]
    Movement = 3,
}

// the configuration part of the rotation module
// importantly, it defines constraints (supported classes and level ranges) and strategy configs (with their sets of possible options) used by the module to make its decisions
// rotation modules can optionally be constrained to a specific boss module, if they are used to implement custom encounter-specific logic - these would only be available in plans for that module
public sealed record class RotationModuleDefinition(string DisplayName, string Description, string Category, string Author, RotationModuleQuality Quality, BitMask Classes, int MaxLevel, int MinLevel = 1, RotationModuleOrder Order = RotationModuleOrder.Actions, Type? RelatedBossModule = null, bool CanUseWhileRoleplaying = false, bool DevMode = false)
{
    public readonly BitMask Classes = Classes;
    public readonly List<StrategyConfig> Configs = [];

    // unfortunately, c# doesn't support partial type inference, and forcing user to spell out track enum twice is obnoxious, so here's the hopefully cheap solution
    public readonly ref struct DefineRef(List<StrategyConfig> configs, int index)
    {
        public ConfigRef<Selector> As<Selector>(string internalName, string displayName = "", float uiPriority = 0, Type? renderer = null) where Selector : Enum
        {
            if (configs.Count != index)
                throw new ArgumentException($"Unexpected index for {internalName}: expected {index}, cur size {configs.Count}");
            var config = new StrategyConfigTrack(typeof(Selector), internalName, displayName, uiPriority, renderer ?? typeof(TrackRenderer));
            configs.Add(config);
            return new(config);
        }
    }

    public readonly ref struct ConfigRef<Index>(StrategyConfigTrack config) where Index : Enum
    {
        public ConfigRef<Index> AddOption(Index expectedIndex, string displayName = "", float cooldown = 0, float effect = 0, ActionTargets supportedTargets = ActionTargets.None, int minLevel = 1, int maxLevel = int.MaxValue, float defaultPriority = ActionQueue.Priority.Medium, string? internalNameOverride = null)
        {
            var idx = (int)(object)expectedIndex;
            var internalName = internalNameOverride ?? expectedIndex.ToString();
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

    public DefineRef Define<Index>(Index expectedIndex) where Index : Enum => new(Configs, (int)(object)expectedIndex);

    public void DefineFloat<Index>(Index expectedIndex, string displayName = "", float minValue = 0, float maxValue = float.MaxValue, float uiPriority = 0) where Index : Enum
    {
        var idx = (int)(object)expectedIndex;
        var internalName = expectedIndex.ToString();
        if (Configs.Count != idx)
            throw new ArgumentException($"Unexpected index value for {internalName}: expected {idx}, cur size {Configs.Count}");
        Configs.Add(new StrategyConfigFloat(internalName, displayName, minValue, maxValue, uiPriority, typeof(FloatRenderer)));
    }

    public void DefineInt<Index>(Index expectedIndex, string displayName = "", long minValue = 0, long maxValue = long.MaxValue, float uiPriority = 0) where Index : Enum
    {
        var idx = (int)(object)expectedIndex;
        var internalName = expectedIndex.ToString();
        if (Configs.Count != idx)
            throw new ArgumentException($"Unexpected index value for {internalName}: expected {idx}, cur size {Configs.Count}");
        Configs.Add(new StrategyConfigInt(internalName, displayName, minValue, maxValue, uiPriority, typeof(IntRenderer)));
    }

    internal T NonDefault<T>(params T[] args) where T : struct
    {
        T last = default;
        foreach (var arg in args)
        {
            if (!EqualityComparer<T>.Default.Equals(arg, default))
                return arg;
            last = arg;
        }

        return last;
    }

    public RotationModuleDefinition WithStrategies<S>()
    {
        foreach (var field in typeof(S).GetFields())
        {
            if (field.FieldType.Name == typeof(Track<>).Name)
            {
                var inner = field.FieldType.GetGenericArguments()[0];

                if (inner.IsEnum)
                {
                    var trackInfo = field.GetCustomAttribute<TrackAttribute>() ?? new();
                    var renderer = trackInfo.Renderer ?? inner.GetCustomAttribute<RendererAttribute>()?.Type ?? typeof(TrackRenderer);

                    var trackCfg = new StrategyConfigTrack(inner, trackInfo.InternalName ?? field.Name, trackInfo.DisplayName ?? field.Name, trackInfo.UiPriority, renderer);

                    foreach (var variantName in inner.GetEnumNames())
                    {
                        var variantField = inner.GetField(variantName)!;
                        var fieldSettings = variantField.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute();

                        trackCfg.Options.Add(new(variantField.Name, fieldSettings.DisplayName ?? "")
                        {
                            Cooldown = NonDefault(fieldSettings.Cooldown, trackInfo.Cooldown, 0),
                            Effect = NonDefault(fieldSettings.Effect, trackInfo.Effect, 0),
                            SupportedTargets = NonDefault(fieldSettings.Targets, trackInfo.Targets, ActionTargets.None),
                            MinLevel = NonDefault(fieldSettings.MinLevel, trackInfo.MinLevel, 1),
                            MaxLevel = NonDefault(fieldSettings.MaxLevel, trackInfo.MaxLevel, int.MaxValue),
                            DefaultPriority = NonDefault(fieldSettings.DefaultPriority, trackInfo.DefaultPriority, ActionQueue.Priority.Medium),
                            Context = NonDefault(fieldSettings.Context, StrategyContext.All),
                            Color = fieldSettings.Color
                        });
                    }

                    Configs.Add(trackCfg);
                    continue;
                }

                if (inner == typeof(float))
                {
                    var attr = field.GetCustomAttribute<NumberAttribute>() ?? new();
                    Configs.Add(new StrategyConfigFloat(field.Name, attr.DisplayName, attr.MinValue, attr.MaxValue, attr.UiPriority, attr.Renderer ?? typeof(FloatRenderer), attr.Slider, attr.Speed));
                    continue;
                }

                if (inner == typeof(int))
                {
                    var attr = field.GetCustomAttribute<NumberAttribute>() ?? new();
                    Configs.Add(new StrategyConfigInt(field.Name, attr.DisplayName, (long)attr.MinValue, (long)attr.MaxValue, attr.UiPriority, attr.Renderer ?? typeof(IntRenderer), attr.Slider, attr.Speed));
                    continue;
                }
            }

            throw new ArgumentException($"not sure what to do with field {field.Name} of type {field.FieldType}");
        }

        return this;
    }
}

// base class for rotation modules
// each rotation module should contain a `public static RotationModuleDefinition Definition()` function
public abstract class RotationModule(RotationModuleManager manager, Actor player)
{
    public readonly RotationModuleManager Manager = manager;
    public readonly Actor Player = player;
    public BossModuleManager Bossmods => Manager.Bossmods;
    public WorldState World => Manager.Bossmods.WorldState;
    public AIHints Hints => Manager.Hints;

    // the main entry point of the module - given a set of strategy values, fill the queue with a set of actions to execute
    public abstract void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving);

    public virtual string DescribeState() => "";

    // utility to check action/trait unlocks
    public bool ActionUnlocked(ActionID action) => ActionDefinitions.Instance[action]?.IsUnlocked(World, Player) ?? false;

    public bool ActionUnlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));

    public AID BestActionUnlocked<AID>(params AID[] aids) where AID : struct, Enum
    {
        foreach (var aid in aids)
            if (ActionUnlocked(aid))
                return aid;

        return default;
    }

    public bool TraitUnlocked(uint id)
    {
        var trait = Service.LuminaRow<Lumina.Excel.Sheets.Trait>(id);
        var unlock = trait?.Quest.RowId ?? 0;
        var level = trait?.Level ?? 0;
        return Player.Level >= level && (ActionDefinitions.Instance.UnlockCheck?.Invoke(unlock) ?? true);
    }

    // utility to resolve the target overrides; returns null on failure - in this case module is expected to run smart-targeting logic
    // expected usage is `ResolveTargetOverride(strategy) ?? CustomSmartTargetingLogic(...)`
    protected Actor? ResolveTargetOverride(in StrategyValueTrack strategy) => Manager.ResolveTargetOverride(strategy.Target, strategy.TargetParam);
    protected AIHints.Enemy? ResolveTargetOverride<T>(in Track<T> track) where T : struct => Hints.FindEnemy(Manager.ResolveTargetOverride(track.TrackRaw.Target, track.TrackRaw.TargetParam));
    protected WPos ResolveTargetLocation(in StrategyValueTrack strategy) => Manager.ResolveTargetLocation(strategy.Target, strategy.TargetParam, strategy.Offset1, strategy.Offset2);

    protected float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - World.CurrentTime).TotalSeconds, 0.0f);

    // this also checks pending statuses
    // note that we check pending statuses first - otherwise we get the same problem with double refresh if we try to refresh early (we find old status even though we have pending one)
    protected (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        var status = actor?.FindStatus(sid, sourceID, World.FutureTime(pendingDuration));
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    protected (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);
    protected (float Left, int Stacks) StatusDetails<SID>(AIHints.Enemy? enemy, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(enemy?.Actor, (uint)(object)sid, sourceID, pendingDuration);
    protected (float Left, int Stacks) SelfStatusDetails(uint sid, float pendingDuration = 1000) => StatusDetails(Player, sid, Player.InstanceID, pendingDuration);
    protected (float Left, int Stacks) SelfStatusDetails<SID>(SID sid, float pendingDuration = 1000) where SID : Enum => StatusDetails(Player, sid, Player.InstanceID, pendingDuration);

    protected float SelfStatusLeft(uint sid, float pendingDuration = 1000) => SelfStatusDetails(sid, pendingDuration).Left;
    protected float SelfStatusLeft<SID>(SID sid, float pendingDuration = 1000) where SID : Enum => SelfStatusDetails(sid, pendingDuration).Left;
    protected float PotionStatusLeft() => SelfStatusLeft(49, 30);

    protected float GCD => World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining; // 2.5 max (decreased by SkS), 0 if not on gcd
    protected float PotionCD => World.Client.Cooldowns[ActionDefinitions.PotionCDGroup].Remaining; // variable max

    // find a slot containing specified duty action; returns -1 if not found
    public int FindDutyActionSlot(ActionID action) => Array.FindIndex(World.Client.DutyActions, d => d.Action == action);
    // find a slot containing specified duty action, if other duty action is the specified one; returns -1 if not found, or other action is different
    public int FindDutyActionSlot(ActionID action, ActionID other)
    {
        var slot = FindDutyActionSlot(action);
        return slot >= 0 && World.Client.DutyActions[1 - slot].Action == other ? slot : -1;
    }

    public int FindDutyActionSlot<AID>(AID aid) where AID : Enum => FindDutyActionSlot(ActionID.MakeSpell(aid));

    public float DutyActionCD(int slot) => slot is >= 0 and < 7
        ? (ActionDefinitions.Instance[World.Client.DutyActions[slot].Action]?.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) ?? float.MaxValue)
        : float.MaxValue;
    public float DutyActionCD(ActionID action) => DutyActionCD(FindDutyActionSlot(action));

    protected (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        if (primaryTarget == null || !primaryTarget.IsStrikingDummy)
            return (Bossmods.RaidCooldowns.DamageBuffLeft(Player, primaryTarget), Bossmods.RaidCooldowns.NextDamageBuffIn());

        // hack for a dummy: expect that raidbuffs appear at 7.8s and then every 120s
        var cycleTime = (float)(Player.InCombat ? (World.CurrentTime - Manager.CombatStart).TotalSeconds : 0) - 7.8f;
        if (cycleTime < 0)
            return (0, 0); // very beginning of a fight

        cycleTime %= 120;
        return cycleTime < 20 ? (20 - cycleTime, 0) : (0, 120 - cycleTime);
    }

    protected (Actor? Target, P Priority) FindBetterTargetBy<P>(Actor? initial, float maxDistanceFromPlayer, Func<Actor, P> prioFunc, Func<AIHints.Enemy, bool>? filterFunc = null) where P : struct, IComparable
    {
        bool inRange(Actor tar) => tar.Position.InCircle(Player.Position, maxDistanceFromPlayer + tar.HitboxRadius + 0.5f);

        if (initial != null && !inRange(initial))
            initial = null;

        var bestTarget = initial;
        var bestPrio = initial != null ? prioFunc(initial) : default;
        foreach (var enemy in Hints.PriorityTargets.Where(x => x.Actor != initial && inRange(x.Actor) && (filterFunc?.Invoke(x) ?? true)))
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

public abstract class TypedRotationModule<TValues>(RotationModuleManager manager, Actor player) : RotationModule(manager, player) where TValues : struct
{
    public abstract void Execute(in TValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving);

    public sealed override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) => Execute(ValueConverter.FromValues<TValues>(strategy), ref primaryTarget, estimatedAnimLockDelay, isMoving);
}
