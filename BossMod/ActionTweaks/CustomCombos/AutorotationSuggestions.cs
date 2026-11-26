using BossMod.Autorotation;

namespace BossMod.ActionTweaks.CustomCombos;

// private autorot for custom combo suggestions, independent of actual autorot being enabled
public sealed class AutorotationSuggestions : IDisposable
{
    public readonly AIHints Hints = new();
    private readonly WorldState _ws;
    private readonly AIHints _realHints;
    private readonly RotationDatabase _db;
    private readonly RotationModuleManager _manager;
    private Preset? _preset;

    public AutorotationSuggestions(WorldState ws, AIHints realHints, RotationDatabase db, BossModuleManager bossmods)
    {
        _ws = ws;
        _realHints = realHints;
        _db = db;
        _manager = new(db, bossmods, Hints);
    }

    public void Dispose() => _manager.Dispose();

    // configured preset, or first preset for the class; pvp and normal presets are separate, since modules only run in one of them
    public Preset? ResolvePreset(string name, Class cls, bool pvp) => (name.Length > 0 ? _db.Presets.FindPresetByName(name) : null) ?? PresetsForClass(cls, pvp).FirstOrDefault();
    public List<Preset> PresetsForClass(Class cls, bool pvp)
        => [.. _db.Presets.PresetsForClass(cls).Where(p => p.Modules.Any(m => m.Definition.Classes[(int)cls] && m.Definition.PvP != (pvp ? PvPCompatibility.None : PvPCompatibility.PvPOnly)))];

    public void Update(CustomComboConfig config, float animLockDelay, bool isMoving)
    {
        var player = _ws.Party.Player();
        var preset = player != null ? ResolvePreset(config.SuggestionPreset(_ws.IsPvPArea), player.Class, _ws.IsPvPArea) : null;
        if (preset == null)
        {
            if (_preset != null)
                _manager.Clear();
        }
        else if (preset != _preset || !_manager.Presets.Contains(preset) || _manager.IsForceDisabled)
        {
            // manager force-disables or clears presets on death, pulls and combat end, but this doesn't care about that
            _manager.Activate(preset, true);
        }
        _preset = preset;

        Hints.CopyFrom(_realHints);
        if (preset != null) // without a preset, manager would execute the cooldown plan instead
            _manager.Update(animLockDelay, isMoving, false);
    }

    public void Clear() => Hints.ActionsToExecute.Clear();
}
