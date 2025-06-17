namespace BossMod;

// class that creates and manages instances of proper boss modules in response to world state changes
public sealed class BossModuleManager : IDisposable
{
    public readonly WorldState WorldState;
    public readonly RaidCooldowns RaidCooldowns;
    public readonly BossModuleConfig Config = Service.Config.Get<BossModuleConfig>();
    private readonly EventSubscriptions _subsciptions;

    public List<BossModule> LoadedModules { get; } = [];
    public Event<BossModule> ModuleLoaded = new();
    public Event<BossModule> ModuleUnloaded = new();
    public Event<BossModule> ModuleActivated = new();
    public Event<BossModule> ModuleDeactivated = new();

    // drawn module among loaded modules; this can be changed explicitly if needed
    // usually we don't have multiple concurrently active modules, since this prevents meaningful cd planning, raid cooldown tracking, etc.
    // but it can theoretically happen e.g. around checkpoints and in typically trivial outdoor content
    private BossModule? _activeModule;
    private bool _activeModuleOverridden;
    public BossModule? ActiveModule
    {
        get => _activeModule;
        set
        {
            Service.Log($"[BMM] Active module override: from {_activeModule?.GetType().FullName ?? "<n/a>"} (manual-override={_activeModuleOverridden}) to {value?.GetType().FullName ?? "<n/a>"}");
            _activeModule = value;
            _activeModuleOverridden = true;
        }
    }

    public BossModuleManager(WorldState ws)
    {
        WorldState = ws;
        RaidCooldowns = new(ws);
        _subsciptions = new
        (
            WorldState.Actors.Added.Subscribe(ActorAdded),
            Config.Modified.ExecuteAndSubscribe(ConfigChanged)
        );

        foreach (var a in WorldState.Actors)
            ActorAdded(a);
    }

    public void Dispose()
    {
        _activeModule = null;
        foreach (var m in LoadedModules)
            m.Dispose();
        LoadedModules.Clear();

        _subsciptions.Dispose();
        RaidCooldowns.Dispose();
    }

    public void Update()
    {
        // update all loaded modules, handle activation/deactivation
        int bestPriority = 0;
        BossModule? bestModule = null;
        bool anyModuleActivated = false;
        for (int i = 0; i < LoadedModules.Count; ++i)
        {
            var m = LoadedModules[i];
            bool wasActive = m.StateMachine.ActiveState != null;
            bool allowUpdate = wasActive || !LoadedModules.Any(other => other.StateMachine.ActiveState != null && other.GetType() == m.GetType()); // hack: forbid activating multiple modules of the same type
            bool isActive;
            try
            {
                if (allowUpdate)
                    m.Update();
                isActive = m.StateMachine.ActiveState != null;
            }
            catch (Exception ex)
            {
                Service.Log($"Boss module {m.GetType()} crashed: {ex}");
                wasActive = true; // force unload if exception happened before activation
                isActive = false;
            }

            // if module was activated or deactivated, notify listeners
            if (isActive != wasActive)
                (isActive ? ModuleActivated : ModuleDeactivated).Fire(m);

            // unload module either if it became deactivated or its primary actor disappeared without ever activating
            if (!isActive && (wasActive || m.PrimaryActor.IsDestroyed))
            {
                UnloadModule(i--);
                continue;
            }

            // if module is active and wants to be reset, oblige
            if (isActive && m.CheckReset())
            {
                ModuleDeactivated.Fire(m);
                var actor = m.PrimaryActor;
                UnloadModule(i--);
                if (!actor.IsDestroyed)
                    ActorAdded(actor);
                continue;
            }

            // module remains loaded
            int priority = ModuleDisplayPriority(m);
            if (priority > bestPriority)
            {
                bestPriority = priority;
                bestModule = m;
            }

            if (!wasActive && isActive)
            {
                Service.Log($"[BMM] Boss module '{m.GetType()}' for actor {m.PrimaryActor.InstanceID:X} ({m.PrimaryActor.OID:X}) '{m.PrimaryActor.Name}' activated");
                anyModuleActivated |= true;
            }
        }

        var curPriority = ModuleDisplayPriority(_activeModule);
        if (bestPriority > curPriority && (anyModuleActivated || !_activeModuleOverridden))
        {
            Service.Log($"[BMM] Active module change: from {_activeModule?.GetType().FullName ?? "<n/a>"} (prio {curPriority}, manual-override={_activeModuleOverridden}) to {bestModule?.GetType().FullName ?? "<n/a>"} (prio {bestPriority})");
            _activeModule = bestModule;
            _activeModuleOverridden = false;
        }
    }

    private void LoadModule(BossModule m)
    {
        LoadedModules.Add(m);
        Service.Log($"[BMM] Boss module '{m.GetType()}' loaded for actor {m.PrimaryActor}");
        ModuleLoaded.Fire(m);
    }

    private void UnloadModule(int index)
    {
        var m = LoadedModules[index];
        Service.Log($"[BMM] Boss module '{m.GetType()}' unloaded for actor {m.PrimaryActor}");
        ModuleUnloaded.Fire(m);
        if (_activeModule == m)
        {
            _activeModule = null;
            _activeModuleOverridden = false;
        }
        m.Dispose();
        LoadedModules.RemoveAt(index);
    }

    private static int ModuleDisplayPriority(BossModule? m)
    {
        if (m == null)
            return 0;
        if (m.StateMachine.ActiveState != null)
            return 4;
        if (m.PrimaryActor.InstanceID == 0)
            return 2; // demo module
        if (!m.PrimaryActor.IsDestroyed && !m.PrimaryActor.IsDead && m.PrimaryActor.IsTargetable)
            return 3;
        return 1;
    }

    private DemoModule CreateDemoModule() => new(WorldState, new(0, 0, -1, 0, "", 0, ActorType.None, Class.None, 0, new()));

    private void ActorAdded(Actor actor)
    {
        var m = BossModuleRegistry.CreateModuleForActor(WorldState, actor, Config.MinMaturity);
        if (m != null)
        {
            LoadModule(m);
        }
    }

    private void ConfigChanged()
    {
        int demoIndex = LoadedModules.FindIndex(m => m is DemoModule);
        if (Config.ShowDemo && demoIndex < 0)
            LoadModule(CreateDemoModule());
        else if (!Config.ShowDemo && demoIndex >= 0)
            UnloadModule(demoIndex);
    }
}
