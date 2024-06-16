using ImGuiNET;
using System.IO;

namespace BossMod.Autorotation;

// the manager contains a set of rotation module instances corresponding to the selected preset/plan
public sealed class RotationModuleManager : IDisposable
{
    private Preset? _preset; // if non-null, this preset overrides the configuration
    public Preset? Preset
    {
        get => _preset;
        set
        {
            DirtyActiveModules(_preset != value);
            _preset = value;
        }
    }

    public readonly AutorotationConfig Config = Service.Config.Get<AutorotationConfig>();
    public readonly PresetDatabase Database;
    private readonly BossModuleManager _bmm;
    private readonly int _playerSlot;
    private readonly AIHints _hints;
    private readonly EventSubscriptions _subscriptions;
    private List<(RotationModuleDefinition Definition, RotationModule Module)>? _activeModules;

    public static readonly Preset ForceDisable = new(""); // empty preset, so if it's activated, rotation is force disabled

    public RotationModuleManager(DirectoryInfo dbRoot, BossModuleManager bmm, AIHints hints, int playerSlot = PartyState.PlayerSlot)
    {
        Database = new(new(dbRoot.FullName + "/presets.json"));
        _bmm = bmm;
        _playerSlot = playerSlot;
        _hints = hints;
        _subscriptions = new
        (
            _bmm.WorldState.Actors.Added.Subscribe(a => DirtyActiveModules(_bmm.WorldState.Party.ActorIDs[_playerSlot] == a.InstanceID)),
            _bmm.WorldState.Actors.Removed.Subscribe(a => DirtyActiveModules(_bmm.WorldState.Party.ActorIDs[_playerSlot] == a.InstanceID)),
            _bmm.WorldState.Actors.ClassChanged.Subscribe(a => DirtyActiveModules(_bmm.WorldState.Party.ActorIDs[_playerSlot] == a.InstanceID)),
            _bmm.WorldState.Actors.InCombatChanged.Subscribe(OnCombatChanged),
            _bmm.WorldState.Actors.IsDeadChanged.Subscribe(OnDeadChanged),
            _bmm.WorldState.Party.Modified.Subscribe(op => DirtyActiveModules(op.Slot == _playerSlot)),
            _bmm.WorldState.Client.CountdownChanged.Subscribe(OnCountdownChanged),
            Database.PresetModified.Subscribe(OnPresetModified)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void Update(Actor? target, ActionQueue actions)
    {
        _activeModules ??= RebuildActiveModules();
        if (Preset != null)
        {
            Preset.Modifier mods = Preset.Modifier.None;
            if (ImGui.GetIO().KeyShift)
                mods |= Preset.Modifier.Shift;
            if (ImGui.GetIO().KeyCtrl)
                mods |= Preset.Modifier.Ctrl;
            if (ImGui.GetIO().KeyAlt)
                mods |= Preset.Modifier.Alt;

            foreach (var m in _activeModules)
            {
                var mt = m.Module.GetType();
                if (Preset.Modules.TryGetValue(mt, out var ms))
                {
                    var values = Utils.MakeArray<StrategyValue>(m.Definition.Configs.Count, new()); // TODO: if allocations are a problem, could use a single private scratch buffer...
                    foreach (ref var s in ms.AsSpan())
                        if ((s.Mod & mods) == s.Mod)
                            values[s.Track] = s.Value;
                    m.Module.Execute(values, target, actions);
                }
                else
                {
                    Service.Log($"[RMM] Preset for {mt.FullName} not found");
                }
            }
        }
        //else if (... plan ...)
        else if (_activeModules.Count != 0)
        {
            Service.Log($"[RMM] Non-empty active module list while there are no active preset/plan");
        }
    }

    // TODO: consider not recreating modules that were active and continue to be active?
    private List<(RotationModuleDefinition Definition, RotationModule Module)> RebuildActiveModules()
    {
        List<(RotationModuleDefinition Definition, RotationModule Module)> res = [];
        var player = _bmm.WorldState.Party[_playerSlot];
        if (player != null)
        {
            if (Preset != null)
            {
                foreach (var m in Preset.Modules)
                {
                    var def = RotationModuleRegistry.Modules.GetValueOrDefault(m.Key);
                    if (def.Definition != null && def.Definition.Classes[(int)player.Class] && player.Level >= def.Definition.MinLevel && player.Level <= def.Definition.MaxLevel)
                    {
                        res.Add((def.Definition, def.Builder(_bmm.WorldState, player, _hints)));
                    }
                }
            }
            // else if (... plan ...)
        }
        return res;
    }

    private void OnCombatChanged(Actor actor)
    {
        if (_bmm.WorldState.Party.ActorIDs[_playerSlot] != actor.InstanceID)
            return; // don't care

        if (!actor.InCombat)
        {
            // player exits combat => clear manual overrides
            Service.Log($"[RMM] Player exits combat => clear preset '{Preset?.Name ?? "<n/a>"}'");
            Preset = null;
        }
        else if (_bmm.WorldState.Client.CountdownRemaining > Config.EarlyPullThreshold)
        {
            // player enters combat while countdown is in progress => force disable
            Service.Log($"[RMM] Player ninja pulled => force-disabling from '{Preset?.Name ?? "<n/a>"}'");
            Preset = ForceDisable;
        }
        // else: player enters combat when countdown is either not active or around zero, proceed normally - if override is queued, let it run, otherwise let plan run
    }

    private void OnDeadChanged(Actor actor)
    {
        if (_bmm.WorldState.Party.ActorIDs[_playerSlot] != actor.InstanceID)
            return; // don't care

        // note: if combat ends while player is dead, we'll reset the preset, which is desirable
        if (actor.IsDead && actor.InCombat)
        {
            // player died in combat => force disable (otherwise there's a risk of dying immediately after rez)
            Service.Log($"[RMM] Player died in combat => force-disabling from '{Preset?.Name ?? "<n/a>"}'");
            Preset = ForceDisable;
        }
        // else: player either died outside combat (no need to touch anything) or rez'd (unless player cleared override, we stay in force disable mode)
    }

    private void OnCountdownChanged(ClientState.OpCountdownChange op)
    {
        if (op.Value == null && !(_bmm.WorldState.Party[_playerSlot]?.InCombat ?? false))
        {
            // countdown ended and player is not in combat - so either it was cancelled, or pull didn't happen => clear manual overrides
            // note that if pull will happen regardless after this, we'll start executing plan normally (without prepull part)
            Service.Log($"[RMM] Countdown expired or aborted => clear preset '{Preset?.Name ?? "<n/a>"}'");
            Preset = null;
        }
    }

    private void OnPresetModified(Preset? prev, Preset? curr)
    {
        if (prev != null && prev == Preset)
            Preset = curr;
    }

    private void DirtyActiveModules(bool condition)
    {
        if (condition)
            _activeModules = null;
    }
}
