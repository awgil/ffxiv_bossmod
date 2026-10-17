namespace BossMod;

public sealed class ZoneModuleManager : IDisposable
{
    public readonly WorldState WorldState;
    public readonly ZoneModuleConfig Config;
    private readonly EventSubscriptions _subscriptions;

    public ZoneModule? ActiveModule;
    public Event<ZoneModule> ModuleLoaded = new();
    public Event<ZoneModule> ModuleUnloaded = new();

    public ZoneModuleManager(WorldState ws, ZoneModuleConfig config)
    {
        Config = config;
        WorldState = ws;
        _subscriptions = new
        (
            WorldState.CurrentZoneChanged.Subscribe(op => OnZoneChanged(op.CFCID))
        );
        OnZoneChanged(ws.CurrentCFCID);
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        ActiveModule?.Dispose();
    }

    private void OnZoneChanged(uint cfcid)
    {
        if (ActiveModule != null)
        {
            Service.Log($"[ZMM] Unloading zone module '{ActiveModule.GetType()}'");
            ModuleUnloaded.Fire(ActiveModule);
            ActiveModule.Dispose();
            ActiveModule = null;
        }

        var m = ZoneModuleRegistry.CreateModule(WorldState, cfcid);
        if (m != null)
        {
            Service.Log($"[ZMM] Loading module '{m.GetType()}' for zone {cfcid}");
            ActiveModule = m;
            ModuleLoaded.Fire(m);
        }
    }
}
