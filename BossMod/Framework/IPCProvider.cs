using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Game.ClientState.Objects.Types;
using System.Text.Json;

namespace BossMod;

sealed class IPCProvider : IDisposable
{
    private Action? _disposeActions;

    private Preset? Deserialize(string p) => JsonSerializer.Deserialize<Preset>(p);
    private string Serialize(Preset p) => JsonSerializer.Serialize(p);
    private string? SerializeN(Preset? p) => p == null ? null : JsonSerializer.Serialize(p);
    private List<string> Serialize(IEnumerable<Preset> p) => p.Select(Serialize).ToList();

    public IPCProvider(RotationModuleManager autorotation, ActionManagerEx amex, MovementOverride movement, AIManager ai)
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
        Register("Configuration", (IReadOnlyList<string> args, bool save) => Service.Config.ConsoleCommand(args, save));
        //Register("InitiateCombat", () => autorotation.ClassActions?.UpdateAutoAction(CommonActions.AutoActionAIFight, float.MaxValue, true));
        //Register("SetAutorotationState", (bool state) => Service.Config.Get<AutorotationConfig>().Enabled = state);

        Register("Presets.List", () => Serialize(autorotation.Database.Presets.Presets));
        Register("Presets.Get", (string name) => SerializeN(autorotation.Database.Presets.Presets.FirstOrDefault(x => x.Name == name)));
        Register("Presets.ForClass", (byte classId) => Serialize(autorotation.Database.Presets.PresetsForClass((Class)classId)));
        Register("Presets.Create", (string presetSerialized, bool overwrite) =>
        {
            var p = Deserialize(presetSerialized);
            if (p == null)
                return false;

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

        Register("Presets.GetActive", () => autorotation.Preset?.Name);
        Register("Presets.SetActive", (string name) =>
        {
            var preset = autorotation.Database.Presets.Presets.FirstOrDefault(x => x.Name == name);
            if (preset != null)
            {
                autorotation.Preset = preset;
                return true;
            }

            return false;
        });
        Register("Presets.ClearActive", () =>
        {
            if (autorotation.Preset == null)
                return false;

            autorotation.Preset = null;
            return true;
        });
        Register("Presets.GetForceDisabled", () => autorotation.Preset == RotationModuleManager.ForceDisable);
        Register("Presets.SetForceDisabled", () =>
        {
            if (autorotation.Preset != RotationModuleManager.ForceDisable)
            {
                autorotation.Preset = RotationModuleManager.ForceDisable;
                return true;
            }

            return false;
        });

        Register("AI.SetPreset", (string name) => ai.SetAIPreset(autorotation.Database.Presets.Presets.FirstOrDefault(x => x.Name == name)));
        Register("AI.GetPreset", () => ai.GetAIPreset);
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

    private void Register<T1>(string name, Action<T1> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, object>("BossMod." + name);
        p.RegisterAction(func);
        _disposeActions += p.UnregisterAction;
    }
}
