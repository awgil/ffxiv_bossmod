namespace BossMod;

public sealed class ZoneModuleManager : IDisposable
{
    public readonly WorldState WorldState;
    public readonly ZoneModuleConfig Config;
    private readonly EventSubscriptions _subsciptions;
    private readonly ZoneModuleArgs.Factory _zfac;

    public ZoneModule? ActiveModule;
    public Event<ZoneModule> ModuleLoaded = new();
    public Event<ZoneModule> ModuleUnloaded = new();

    public ZoneModuleManager(WorldState ws, ZoneModuleArgs.Factory zfac, ZoneModuleConfig cfg)
    {
        _zfac = zfac;
        Config = cfg;
        WorldState = ws;
        _subsciptions = new
        (
            WorldState.CurrentZoneChanged.Subscribe(op => OnZoneChanged(op.CFCID))
        );
        OnZoneChanged(ws.CurrentCFCID);
    }

    public void Dispose()
    {
        _subsciptions.Dispose();
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

        var args = _zfac.Invoke(WorldState);
        var m = ZoneModuleRegistry.CreateModule(args, cfcid, Config.MinMaturity);
        if (m != null)
        {
            Service.Log($"[ZMM] Loading module '{m.GetType()}' for zone {cfcid}");
            ActiveModule = m;
            ModuleLoaded.Fire(m);
        }
    }
}
