using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Pathfinding;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal sealed class IpcService(
    IDalamudPluginInterface dalamud,
    RotationModuleManager autorotation,
    ObstacleMapManager obstacles,
    MovementOverride movement,
    IPluginLog logger,
    ConfigRoot config,
    JsonSerializerOptions serOptions
) : IHostedService
{
    private Action _stopActions = () => { };
    private readonly AIConfig aiConfig = config.Get<AIConfig>();
    private readonly ActionTweaksConfig tweaksConfig = config.Get<ActionTweaksConfig>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Register("HasModuleByDataId", (uint dataId) => BossModuleRegistry.FindByOID(dataId) != null);
        // 'save' argument is deprecated
        Register("Configuration", (IReadOnlyList<string> args, bool save) => config.ConsoleCommand(string.Join(' ', args)));

        var lastModified = DateTime.Now;
        config.Modified.Subscribe(() => lastModified = DateTime.Now);
        Register("Configuration.LastModified", () => lastModified);

        Register("Configuration.DisableModule", (string name, bool disable) =>
        {
            var disabledModules = config.Get<BossModuleConfig>().DisabledModules;

            if (!disable)
                return disabledModules.Remove(name);

            if (!disabledModules.Contains(name))
            {
                disabledModules.Add(name);
                return true;
            }

            return false;
        });

        Register("Rotation.ActionQueue.HasEntries", () => autorotation.Hints.ActionsToExecute.Entries.Any(x => !x.Manual));

        IEnumerable<Type> activeAutorotModules(bool includeMultibox) => autorotation.Presets
            .Where(p => includeMultibox || p.Name != "VBM Multibox")
            .SelectMany(p => p.Modules)
            .Select(m => m.Type);
        Register("Rotation.GetActiveModules", () => activeAutorotModules(true).Select(t => t.FullName ?? t.Name).ToList());
        Register("Rotation.IsAutorotationActive", () => activeAutorotModules(true).Any(t => t.Namespace != "BossMod.Autorotation.MiscAI"));
        Register("Rotation.IsAutoTargetActive", () => activeAutorotModules(false).Any(t => t.FullName == "BossMod.Autorotation.MiscAI.AutoTarget"));

        Register("Rotation.ActionTweaks.IsManualQueueEnabled", () => tweaksConfig.UseManualQueue);
        Register("Rotation.ActionTweaks.IsSmartTargetingEnabled", () => tweaksConfig.UseManualQueue && tweaksConfig.SmartTargets);
        Register("Rotation.ActionTweaks.IsPreventMovingWhileCastingEnabled", () => tweaksConfig.PreventMovingWhileCasting);
        Register("Rotation.ActionTweaks.IsMoveEscapeHatchHeld", movement.IsForceUnblocked);

        Register("AI.IsEnabled", () => aiConfig.Enabled);
        Register("AI.IsAutoTargetEnabled", () => aiConfig.Enabled && !aiConfig.ForbidActions);

        Register("Presets.ListAll", () => autorotation.Database.Presets.AllPresets.Select(p => p.Name).ToList());
        Register("Presets.Get", (string name) =>
        {
            var preset = autorotation.Database.Presets.FindPresetByName(name);
            return preset != null ? JsonSerializer.Serialize(preset, serOptions) : null;
        });
        Register("Presets.Create", (string presetSerialized, bool overwrite) =>
        {
            var node = JsonNode.Parse(presetSerialized, documentOptions: new JsonDocumentOptions() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
            if (node == null)
                return false;

            // preset converter operates on array of presets; plan converter doesn't but expects an `Encounter` key in the object
            node = new JsonArray(node);

            var version = 0;
            var finfo = new FileInfo("<in-memory preset>");

            foreach (var conv in PlanPresetConverter.PresetSchema.Converters)
                node = conv(node, version, finfo);

            var p = node.AsArray()[0].Deserialize<Preset>(serOptions);
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
                    logger.Info($"Presets.SetActiveList: input contained unrecognized preset name {name} - giving up");
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
            if (!RotationModuleRegistry.Modules.TryGetValue(moduleTypeName, out var md))
                return false;
            var iTrack = md.Definition.Configs.FindIndex(td => td.InternalName == trackName);
            if (iTrack < 0)
                return false;

            StrategyValue tempValue;

            switch (md.Definition.Configs[iTrack])
            {
                case StrategyConfigTrack tr:
                    var iOpt = tr.Options.FindIndex(od => od.InternalName == value);
                    if (iOpt < 0)
                        return false;
                    tempValue = new StrategyValueTrack() { Option = iOpt, Target = target, TargetParam = targetParam };
                    break;
                case StrategyConfigFloat sc:
                    tempValue = new StrategyValueFloat() { Value = Math.Clamp(float.Parse(value), sc.MinValue, sc.MaxValue) };
                    break;
                case StrategyConfigInt si:
                    tempValue = new StrategyValueInt() { Value = Math.Clamp(long.Parse(value), si.MinValue, si.MaxValue) };
                    break;
                case var x:
                    throw new ArgumentException($"unhandled config type {x.GetType()}");
            }

            var ms = autorotation.Database.Presets.FindPresetByName(presetName)?.Modules.Find(m => m.Type == md.ModuleType);
            if (ms == null)
                return false;
            var setting = new Preset.ModuleSetting(default, iTrack, tempValue);
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
            if (!RotationModuleRegistry.Modules.TryGetValue(moduleTypeName, out var md))
                return false;
            var iTrack = md.Definition.Configs.FindIndex(td => td.InternalName == trackName);
            if (iTrack < 0)
                return false;
            var ms = autorotation.Database.Presets.FindPresetByName(presetName)?.Modules.Find(m => m.Type == md.ModuleType);
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
            if (!RotationModuleRegistry.Modules.TryGetValue(moduleTypeName, out var md))
                return false;
            var ms = autorotation.Database.Presets.FindPresetByName(presetName)?.Modules.Find(m => m.Type == md.ModuleType);
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

        Register("ObstacleMap.Generate", (Vector3 centerWorld, float radius, bool writeToFile) => obstacles.GenerateMap(centerWorld, radius, writeToFile, false));
        Register("ObstacleMap.GetGenerationStatus", () => obstacles.GenerationStatus);
        Register("ObstacleMap.HasTempMap", obstacles.HasTempMap);
        Register("ObstacleMap.ClearTempMap", obstacles.ClearTempMap);
        Register("ObstacleMap.EvaluateTempMapQuality", obstacles.EvaluateTempMapQuality);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stopActions.Invoke();
        return Task.CompletedTask;
    }

    private void Register<TRet>(string name, Func<TRet> func)
    {
        var p = dalamud.GetIpcProvider<TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _stopActions += p.UnregisterFunc;
    }

    private void Register<T1, TRet>(string name, Func<T1, TRet> func)
    {
        var p = dalamud.GetIpcProvider<T1, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _stopActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, TRet>(string name, Func<T1, T2, TRet> func)
    {
        var p = dalamud.GetIpcProvider<T1, T2, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _stopActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, T3, TRet>(string name, Func<T1, T2, T3, TRet> func)
    {
        var p = dalamud.GetIpcProvider<T1, T2, T3, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _stopActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, T3, T4, TRet>(string name, Func<T1, T2, T3, T4, TRet> func)
    {
        var p = dalamud.GetIpcProvider<T1, T2, T3, T4, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _stopActions += p.UnregisterFunc;
    }

    private void Register<T1, T2, T3, T4, T5, TRet>(string name, Func<T1, T2, T3, T4, T5, TRet> func)
    {
        var p = dalamud.GetIpcProvider<T1, T2, T3, T4, T5, TRet>("BossMod." + name);
        p.RegisterFunc(func);
        _stopActions += p.UnregisterFunc;
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
