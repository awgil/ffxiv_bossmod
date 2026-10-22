using Autofac.Features.AttributeFilters;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Autorotation;

// note: presets in the database are immutable (otherwise eg. manager won't see the changes in active preset)
public sealed class PresetDatabase(
    [KeyFilter(Keys.DefaultPresetsFile)] FileInfo defaultPresets,
    [KeyFilter(Keys.PresetDatabase)] FileInfo dbPath,
    JsonSerializerOptions serOptions,
    IPluginLog logger
) : IHostedService
{
    private readonly AutorotationConfig config = Service.Config.Get<AutorotationConfig>();

    public readonly List<Preset> DefaultPresets = []; // default presets, distributed as part of the plugin
    public readonly List<Preset> UserPresets = []; // user-defined presets, stored in user's preset db
    public Event<Preset?, Preset?> PresetModified = new(); // (old, new); old == null if preset is added, new == null if preset is removed

    private readonly Lock _lock = new();

    public IEnumerable<Preset> AllPresets => DefaultPresets.Select(p => p with { HiddenByDefault = config.HideDefaultPreset || p.Name == "VBM Multibox" }).Concat(UserPresets);

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        Load();
    }
    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Load()
    {
        lock (_lock)
        {
            DefaultPresets.Clear();
            DefaultPresets.AddRange(LoadPresetsFromFile(defaultPresets));
            UserPresets.Clear();
            UserPresets.AddRange(LoadPresetsFromFile(dbPath));
        }
    }

    private List<Preset> LoadPresetsFromFile(FileInfo file)
    {
        try
        {
            var data = PlanPresetConverter.PresetSchema.Load(file);
            using var json = data.document;
            return data.payload.Deserialize<List<Preset>>(serOptions) ?? [];
        }
        catch (Exception ex)
        {
            logger.Warning(ex, $"Failed to parse preset database '{file.FullName}'");
            return [];
        }
    }

    // if index >= 0: replace or delete
    // if index == -1: add (if replacement is non-null) or notify about reordering (otherwise)
    public void Modify(int index, Preset? replacement)
    {
        var previous = index >= 0 ? UserPresets[index] : null;

        if (index < 0 && replacement != null)
            UserPresets.Add(replacement);
        else if (index >= 0 && replacement == null)
            UserPresets.RemoveAt(index);
        else if (index >= 0 && replacement != null)
            UserPresets[index] = replacement;

        if (previous != null || replacement != null)
            PresetModified.Fire(previous, replacement);

        Save();
    }

    public void Save()
    {
        try
        {
            PlanPresetConverter.PresetSchema.Save(dbPath, jwriter => JsonSerializer.Serialize(jwriter, UserPresets, serOptions));
            logger.Debug($"Database saved successfully to '{dbPath.FullName}'");
        }
        catch (Exception ex)
        {
            logger.Warning(ex, $"Failed to write database to '{dbPath.FullName}'");
        }
    }

    public List<Preset> PresetsForClass(Class c)
    {
        lock (_lock)
        {
            return [.. AllPresets.Where(p => p.Modules.Any(m => m.Definition.Classes[(int)c]))];
        }
    }

    public Preset? FindPresetByName(ReadOnlySpan<char> name, StringComparison cmp = StringComparison.CurrentCultureIgnoreCase)
    {
        foreach (var p in AllPresets)
            if (name.Equals(p.Name, cmp))
                return p;
        return null;
    }
}
