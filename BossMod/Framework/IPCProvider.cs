using BossMod.AI;
using BossMod.Autorotation;
using System.Text.Json;

namespace BossMod;

sealed class IPCProvider : IDisposable
{
    private Action? _disposeActions;

    public IPCProvider(RotationModuleManager autorotation, ActionManagerEx amex, MovementOverride movement, AIManager ai)
    {
        Register("HasModuleByDataId", (uint dataId) => BossModuleRegistry.FindByOID(dataId) != null);
        Register("Configuration", (IReadOnlyList<string> args, bool save) => Service.Config.ConsoleCommand(string.Join(' ', args), save));

        DateTime lastModified = DateTime.Now;
        Service.Config.Modified.Subscribe(() => lastModified = DateTime.Now);
        Register("Configuration.LastModified", () => lastModified);

        Register("Rotation.ActionQueue.HasEntries", () => autorotation.Hints.ActionsToExecute.Entries.Any(x => !x.Manual));

        Register("Presets.Get", (string name) =>
        {
            var preset = autorotation.Database.Presets.FindPresetByName(name);
            return preset != null ? JsonSerializer.Serialize(preset, Serialization.BuildSerializationOptions()) : null;
        });
        Register("Presets.Create", (string presetSerialized, bool overwrite) =>
        {
            var p = JsonSerializer.Deserialize<Preset>(presetSerialized, Serialization.BuildSerializationOptions());
            if (p == null)
                return false;
            var index = autorotation.Database.Presets.UserPresets.FindIndex(x => x.Name == p.Name);
            if (index >= 0 && !overwrite)
                return false;
            autorotation.Database.Presets.Modify(index, p);
            return true;
        });
        Register("Presets.Delete", (string name) =>
        {
            var index = autorotation.Database.Presets.UserPresets.FindIndex(x => x.Name == name);
            if (index < 0)
                return false;
            autorotation.Database.Presets.Modify(index, null);
            return true;
        });

        Register("Presets.GetActive", () => autorotation.Presets.Count == 1 ? autorotation.Presets[0].Name : null);
        Register("Presets.SetActive", (string name) =>
        {
            var preset = autorotation.Database.Presets.FindPresetByName(name);
            if (preset == null)
                return false;
            autorotation.Clear();
            autorotation.Activate(preset);
            return true;
        });
        Register("Presets.ClearActive", () =>
        {
            if (autorotation.Presets.Count == 0)
                return false;
            autorotation.Clear();
            return true;
        });
        Register("Presets.GetForceDisabled", () => autorotation.IsForceDisabled);
        Register("Presets.SetForceDisabled", () =>
        {
            if (autorotation.IsForceDisabled)
                return false;
            autorotation.SetForceDisabled();
            return true;
        });

        Register("Presets.Activate", (string name) =>
        {
            var preset = autorotation.Database.Presets.FindPresetByName(name);
            if (preset == null || autorotation.Presets.Contains(preset))
                return false;
            autorotation.Activate(preset);
            return true;
        });
        Register("Presets.Deactivate", (string name) =>
        {
            var preset = autorotation.Database.Presets.FindPresetByName(name);
            if (preset == null || !autorotation.Presets.Contains(preset))
                return false;
            autorotation.Deactivate(preset);
            return true;
        });

        Register("Presets.GetActiveList", () => autorotation.Presets.Select(p => p.Name).ToList());
        Register("Presets.SetActiveList", (List<string> names) =>
        {
            List<Preset> presets = [];
            foreach (var name in names)
            {
                var p = autorotation.Database.Presets.FindPresetByName(name);
                if (p == null)
                {
                    Service.Log($"Presets.SetActiveList: input contained unrecognized preset name {name} - giving up");
                    return false;
                }
                presets.Add(p);
            }

            autorotation.Clear();
            foreach (var p in presets)
                autorotation.Activate(p);
            return true;
        });

        bool addTransientStrategy(string presetName, string moduleTypeName, string trackName, string value, StrategyTarget target = StrategyTarget.Automatic, int targetParam = 0)
        {
            var mt = Type.GetType(moduleTypeName);
            if (mt == null || !RotationModuleRegistry.Modules.TryGetValue(mt, out var md))
                return false;
            var iTrack = md.Definition.Configs.FindIndex(td => td.InternalName == trackName);
            if (iTrack < 0)
                return false;
            var iOpt = md.Definition.Configs[iTrack].Options.FindIndex(od => od.InternalName == value);
            if (iOpt < 0)
                return false;
            var ms = autorotation.Database.Presets.FindPresetByName(presetName)?.Modules.Find(m => m.Type == mt);
            if (ms == null)
                return false;
            var setting = new Preset.ModuleSetting(default, iTrack, new() { Option = iOpt, Target = target, TargetParam = targetParam });
            var index = ms.TransientSettings.FindIndex(s => s.Track == iTrack);
            if (index < 0)
                ms.TransientSettings.Add(setting);
            else
                ms.TransientSettings[index] = setting;
            return true;
        }
        Register("Presets.AddTransientStrategy", (string presetName, string moduleTypeName, string trackName, string value) => addTransientStrategy(presetName, moduleTypeName, trackName, value));
        Register("Presets.AddTransientStrategyTargetEnemyOID", (string presetName, string moduleTypeName, string trackName, string value, int oid) => addTransientStrategy(presetName, moduleTypeName, trackName, value, StrategyTarget.EnemyByOID, oid));

        Register("Presets.ClearTransientStrategy", (string presetName, string moduleTypeName, string trackName) =>
        {
            var mt = Type.GetType(moduleTypeName);
            if (mt == null || !RotationModuleRegistry.Modules.TryGetValue(mt, out var md))
                return false;
            var iTrack = md.Definition.Configs.FindIndex(td => td.InternalName == trackName);
            if (iTrack < 0)
                return false;
            var ms = autorotation.Database.Presets.FindPresetByName(presetName)?.Modules.Find(m => m.Type == mt);
            if (ms == null)
                return false;
            var index = ms.TransientSettings.FindIndex(s => s.Track == iTrack);
            if (index < 0)
                return false;
            ms.TransientSettings.RemoveAt(index);
            return true;
        });
        Register("Presets.ClearTransientModuleStrategies", (string presetName, string moduleTypeName) =>
        {
            var mt = Type.GetType(moduleTypeName);
            if (mt == null || !RotationModuleRegistry.Modules.TryGetValue(mt, out var md))
                return false;
            var ms = autorotation.Database.Presets.FindPresetByName(presetName)?.Modules.Find(m => m.Type == mt);
            if (ms == null)
                return false;
            ms.TransientSettings.Clear();
            return true;
        });
        Register("Presets.ClearTransientPresetStrategies", (string presetName) =>
        {
            var preset = autorotation.Database.Presets.FindPresetByName(presetName);
            if (preset == null)
                return false;
            foreach (var ms in preset.Modules)
                ms.TransientSettings.Clear();
            return true;
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

    private void Register<T1, T2, T3, TRet>(string name, Func<T1, T2, T3, TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, T2, T3, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, T3, T4, TRet>(string name, Func<T1, T2, T3, T4, TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, T2, T3, T4, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _disposeActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, T3, T4, T5, TRet>(string name, Func<T1, T2, T3, T4, T5, TRet> func)
    {
        var p = Service.PluginInterface.GetIpcProvider<T1, T2, T3, T4, T5, TRet>("BossMod." + name);
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
