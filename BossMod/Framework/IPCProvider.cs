using Dalamud.Game.ClientState.Objects.Types;

namespace BossMod;

class IPCProvider : IDisposable
{
    private List<Action> _disposeActions = new();

    public IPCProvider(Autorotation autorotation)
    {
        // TODO: this really needs to be reconsidered, this exposes implementation detail
        // for usecase description, see PR 330 - really AI itself should handle heal range
        Register("ActiveModuleComponentBaseList", () => autorotation.Bossmods.ActiveModule?.Components.Select(c => c.GetType().BaseType?.Name).ToList() ?? default);
        Register("ActiveModuleComponentList", () => autorotation.Bossmods.ActiveModule?.Components.Select(c => c.GetType().Name).ToList() ?? default);
        Register("ActiveModuleHasComponent", (string name) => autorotation.Bossmods.ActiveModule?.Components.Any(c => c.GetType().Name == name || c.GetType().BaseType?.Name == name) ?? false);

        Register("HasModule", (GameObject obj) => ModuleRegistry.FindByOID(obj.DataId) != null);
        Register("IsMoving", () => ActionManagerEx.Instance!.InputOverride.IsMoving());
        Register("ForbiddenZonesCount", () => autorotation.Hints.ForbiddenZones.Count);
        Register("InitiateCombat", () => autorotation.ClassActions?.UpdateAutoAction(CommonActions.AutoActionAIFight, float.MaxValue, true));
        Register("SetAutorotationState", (bool state) => Service.Config.Get<AutorotationConfig>().Enabled = state);
    }

    public void Dispose()
    {
        foreach (var a in _disposeActions)
            a();
    }

    private void Register<TRet>(string name, Func<TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions.Add(p.UnregisterFunc);
    }

    private void Register<TRet, T1>(string name, Func<TRet, T1> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet, T1>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions.Add(p.UnregisterFunc);
    }

    private void Register(string name, Action func)
    {
        var p = Service.PluginInterface.GetIpcProvider<object>("BossMod." + name);
        p.RegisterAction(func);
        _disposeActions.Add(p.UnregisterAction);
    }

    private void Register<T1>(string name, Action<T1> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, object>("BossMod." + name);
        p.RegisterAction(func);
        _disposeActions.Add(p.UnregisterAction);
    }
}
