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

// the configuration part of the rotation module
// importantly, it defines constraints (supported classes and level ranges) and strategy configs (with their sets of possible options) used by the module to make its decisions
// rotation modules can optionally be constrained to a specific boss module, if they are used to implement custom encounter-specific logic - these would only be available in plans for that module
public sealed record class RotationModuleDefinition(string DisplayName, string Description, string Author, RotationModuleQuality Quality, BitMask Classes, int MaxLevel, int MinLevel = 1, Type? RelatedBossModule = null, bool CanUseWhileMounted = false, bool CanUseWhileRoleplaying = false)
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
    public abstract void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving);

    public virtual string DescribeState() => "";

    // utility to check action/trait unlocks
    public bool ActionUnlocked(ActionID action) => ActionDefinitions.Instance[action]?.IsUnlocked(World, Player) ?? false;

    public bool TraitUnlocked(uint id)
    {
        var trait = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Trait>(id);
        var unlock = trait?.Quest.Row ?? 0;
        var level = trait?.Level ?? 0;
        return Player.Level >= level && (ActionDefinitions.Instance.UnlockCheck?.Invoke(unlock) ?? true);
    }

    // utility to resolve the target overrides; returns null on failure - in this case module is expected to run smart-targeting logic
    // expected usage is `ResolveTargetOverride(strategy) ?? CustomSmartTargetingLogic(...)`
    protected Actor? ResolveTargetOverride(in StrategyValue strategy) => Manager.ResolveTargetOverride(strategy.Target, strategy.TargetParam);

    // TODO: reconsider...
    public unsafe T GetGauge<T>() where T : unmanaged
    {
        T res = default;
        ((ulong*)&res)[1] = World.Client.GaugePayload.Low;
        if (sizeof(T) > 16)
            ((ulong*)&res)[2] = World.Client.GaugePayload.High;
        return res;
    }

    protected float StatusDuration(DateTime expireAt) => Math.Max((float)(expireAt - World.CurrentTime).TotalSeconds, 0.0f);

    // this also checks pending statuses
    // note that we check pending statuses first - otherwise we get the same problem with double refresh if we try to refresh early (we find old status even though we have pending one)
    protected (float Left, int Stacks) StatusDetails(Actor? actor, uint sid, ulong sourceID, float pendingDuration = 1000)
    {
        if (actor == null)
            return (0, 0);
        var pending = World.PendingEffects.PendingStatus(actor.InstanceID, sid, sourceID);
        if (pending != null)
            return (pendingDuration, pending.Value);
        var status = actor.FindStatus(sid, sourceID);
        return status != null ? (StatusDuration(status.Value.ExpireAt), status.Value.Extra & 0xFF) : (0, 0);
    }
    protected (float Left, int Stacks) StatusDetails<SID>(Actor? actor, SID sid, ulong sourceID, float pendingDuration = 1000) where SID : Enum => StatusDetails(actor, (uint)(object)sid, sourceID, pendingDuration);

    protected (float Left, int Stacks) SelfStatusDetails(uint sid, float pendingDuration = 1000) => StatusDetails(Player, sid, Player.InstanceID, pendingDuration);
    protected (float Left, int Stacks) SelfStatusDetails<SID>(SID sid, float pendingDuration = 1000) where SID : Enum => StatusDetails(Player, sid, Player.InstanceID, pendingDuration);

    protected float SelfStatusLeft(uint sid, float pendingDuration = 1000) => SelfStatusDetails(sid, pendingDuration).Left;
    protected float SelfStatusLeft<SID>(SID sid, float pendingDuration = 1000) where SID : Enum => SelfStatusDetails(sid, pendingDuration).Left;

    protected float PotionStatusLeft() => SelfStatusLeft(49, 30);

    protected float GCD => World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining; // 2.5 max (decreased by SkS), 0 if not on gcd
    protected float PotionCD => World.Client.Cooldowns[ActionDefinitions.PotionCDGroup].Remaining; // variable max

    // find a slot containing specified duty action; returns -1 if not found
    public int FindDutyActionSlot(ActionID action) => Array.IndexOf(World.Client.DutyActions, action);
    // find a slot containing specified duty action, if other duty action is the specified one; returns -1 if not found, or other action is different
    public int FindDutyActionSlot(ActionID action, ActionID other)
    {
        var slot = FindDutyActionSlot(action);
        return slot >= 0 && World.Client.DutyActions[1 - slot] == other ? slot : -1;
    }

    public float DutyActionCD(int slot) => slot is >= 0 and < 2 ? World.Client.Cooldowns[ActionDefinitions.DutyAction0CDGroup + slot].Remaining : float.MaxValue;
    public float DutyActionCD(ActionID action) => DutyActionCD(FindDutyActionSlot(action));

    protected (float Left, float In) EstimateRaidBuffTimings(Actor? primaryTarget)
    {
        if (primaryTarget?.OID != 0x385)
            return (Bossmods.RaidCooldowns.DamageBuffLeft(Player), Bossmods.RaidCooldowns.NextDamageBuffIn());

        // hack for a dummy: expect that raidbuffs appear at 7.8s and then every 120s
        var cycleTime = (float)(Player.InCombat ? (World.CurrentTime - Manager.CombatStart).TotalSeconds : 0) - 7.8f;
        if (cycleTime < 0)
            return (0, 0); // very beginning of a fight

        cycleTime %= 120;
        return cycleTime < 20 ? (20 - cycleTime, 0) : (0, 120 - cycleTime);
    }
}
