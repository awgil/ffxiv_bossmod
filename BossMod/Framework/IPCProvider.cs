using BossMod.Autorotation;
using Dalamud.Game.ClientState.Objects.Types;

namespace BossMod;

sealed class IPCProvider : IDisposable
{
    private Action? _disposeActions;

    public IPCProvider(RotationModuleManager autorotation, ActionManagerEx amex, MovementOverride movement)
    {
        // TODO: this really needs to be reconsidered, this exposes implementation detail
        // for usecase description, see PR 330 - really AI itself should handle heal range
        Register("ActiveModuleComponentBaseList", () => autorotation.Bossmods.ActiveModule?.Components.Select(c => c.GetType().BaseType?.Name).ToList() ?? default);
        Register("ActiveModuleComponentList", () => autorotation.Bossmods.ActiveModule?.Components.Select(c => c.GetType().Name).ToList() ?? default);
        Register("ActiveModuleHasComponent", (string name) => autorotation.Bossmods.ActiveModule?.Components.Any(c => c.GetType().Name == name || c.GetType().BaseType?.Name == name) ?? false);

        Register("HasModule", (IGameObject obj) => ModuleRegistry.FindByOID(obj.DataId) != null);
        Register("HasModuleByDataId", (uint dataId) => ModuleRegistry.FindByOID(dataId) != null);
        Register("IsMoving", movement.IsMoving);
        Register("ForbiddenZonesCount", () => autorotation.Hints.ForbiddenZones.Count);
        Register("Configuration", (IReadOnlyList<string> args) => Service.Config.ConsoleCommand(args));
        //Register("InitiateCombat", () => autorotation.ClassActions?.UpdateAutoAction(CommonActions.AutoActionAIFight, float.MaxValue, true));
        //Register("SetAutorotationState", (bool state) => Service.Config.Get<AutorotationConfig>().Enabled = state);

        Register("Presets.List", () => autorotation.Database.Presets.Presets);
        Register("Presets.Get", (string name) => autorotation.Database.Presets.Presets.FirstOrDefault(x => x.Name == name));
        Register("Presets.ForClass", (byte classId) => autorotation.Database.Presets.PresetsForClass((Class)classId));
        Register("Presets.Create", (Preset p, bool overwrite) =>
        {
            if (autorotation.Database.Presets.Presets.Any(x => x.Name == p.Name) && !overwrite)
                return false;

            autorotation.Database.Presets.Modify(-1, p);
            return true;
        });
        Register("Presets.Delete", (string name) =>
        {
            var i = autorotation.Database.Presets.Presets.FindIndex(x => x.Name == name);
            if (i >= 0)
                autorotation.Database.Presets.Modify(i, null);

            return i >= 0;
        });
    }

    public void Dispose() => _disposeActions?.Invoke();

    private void Register<TRet>(string name, Func<TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register<T1, TRet>(string name, Func<T1, TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, TRet>(string name, Func<T1, T2, TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, T2, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    //private void Register(string name, Action func)
    //{
    //    var p = Service.PluginInterface.GetIpcProvider<object>("BossMod." + name);
    //    p.RegisterAction(func);
    //    _disposeActions += p.UnregisterAction;
    //}

    //private void Register<T1>(string name, Action<T1> func)
    //{
    //    var p = Service.PluginInterface.GetIpcProvider<T1, object>("BossMod." + name);
    //    p.RegisterAction(func);
    //    _disposeActions += p.UnregisterAction;
    //}
}
