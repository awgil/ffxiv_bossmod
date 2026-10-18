using BossMod.AI;
using BossMod.Autorotation;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

public sealed class CommandService : DisposableMediatorSubscriberBase, IHostedService
{
    private readonly SlashCommandProvider _slashCmd;

    private readonly WorldState _ws;
    private readonly AIHintsBuilder _hintsBuilder;
    private readonly AIHints _hints;
    private readonly IAmex _amex;
    private readonly BossModuleManager _bossmod;
    private readonly AIWindow aiWindow;
    private readonly RotationModuleManager _rotation;
    private readonly AIConfig aiConfig;

    public CommandService(
        ILogger<DisposableMediatorSubscriberBase> logger,
        MediatorService mediator,
        ICommandManager commandManager,
        WorldState ws,
        AIHintsBuilder hintsBuilder,
        AIHints hints,
        IAmex amex,
        RotationModuleManager rmm,
        BossModuleManager bmm,
        AIWindow aiWindow,
        AIConfig aiConfig
    ) : base(logger, mediator)
    {
        _ws = ws;
        _hintsBuilder = hintsBuilder;
        _hints = hints;
        _amex = amex;
        _rotation = rmm;
        _bossmod = bmm;
        this.aiWindow = aiWindow;
        this.aiConfig = aiConfig;
        _slashCmd = new(commandManager, "/vbm");

        _slashCmd.SetSimpleHandler("show boss mod settings UI", () => OpenConfigUI());
        _slashCmd.AddSubcommand("r").SetSimpleHandler("show/hide replay management window", () => MediatorService.Publish(new ToggleWindow(typeof(ReplayManagementWindow))));
        RegisterAutorotationSlashCommands(_slashCmd.AddSubcommand("ar"));
        RegisterAISlashCommands(_slashCmd.AddSubcommand("ai"));
        _slashCmd.AddSubcommand("cfg").SetComplexHandler("<config-type> <field> [<value>]", "query or modify configuration setting", args =>
        {
            var output = Service.Config.ConsoleCommand(args);
            foreach (var msg in output)
                Service.ChatGui.Print(msg);
            return true;
        });
        _slashCmd.AddSubcommand("d").SetSimpleHandler("show debug UI", () => MediatorService.Publish(new OpenWindow(typeof(MainDebugWindow))));
        _slashCmd.AddSubcommand("gc").SetSimpleHandler("execute C# garbage collector", () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        });

        _slashCmd.AddSubcommand("clear-maps").SetSimpleHandler("clear all generated bitmaps (for pathfinding)", _hintsBuilder.Obstacles.ClearGenerated);

        _slashCmd.AddSubcommand("macro").SetSimpleHandler("enables VBM action queue inside macros (by default, any actions used in macros bypass the queue)", () =>
        {
            unsafe
            {
                if (RaptureShellModule.Instance()->MacroCurrentLine >= 0)
                    _amex.MacroCapture = true;
                else
                    Service.ChatMessage("That command doesn't do anything unless it's inside a macro.");
            }
        });
        _slashCmd.AddSubcommand("macro-off").SetSimpleHandler("disables VBM action queue inside a macro, if it has been enabled previously", () =>
        {
            unsafe
            {
                if (RaptureShellModule.Instance()->MacroCurrentLine >= 0)
                    _amex.MacroCapture = false;
                else
                    Service.ChatMessage("That command doesn't do anything unless it's inside a macro.");
            }
        });

        _slashCmd.AddSubcommand("helpme").SetSimpleHandler("gather diagnostic information", HelpMe);
    }

    private void RegisterAutorotationSlashCommands(SlashCommandHandler cmd)
    {
        void SetOrToggle(Preset preset, bool toggle, bool exclusive)
        {
            if (toggle)
            {
                var verb = _rotation.Presets.Contains(preset) ? "disables" : "enables";
                Service.Log($"Console: toggle {verb} preset '{preset.Name}'");
                _rotation.Toggle(preset, exclusive);
            }
            else
            {
                Service.Log($"Console: set activates preset '{preset.Name}'");
                _rotation.Activate(preset, exclusive);
            }
        }

        void SetOrToggleByName(ReadOnlySpan<char> presetName, bool toggle, bool exclusive)
        {
            var preset = _rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
                SetOrToggle(preset, toggle, exclusive);
            else
                Service.ChatError($"Failed to find preset '{presetName}'");
        }

        void ClearByName(ReadOnlySpan<char> presetName)
        {
            var preset = _rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
            {
                Service.Log($"Console: unset deactivates preset '{preset.Name}'");
                _rotation.Deactivate(preset);
            }
            else
                Service.ChatError($"Failed to find preset '{presetName}'");
        }

        cmd.SetSimpleHandler("toggle autorotation ui", () => MediatorService.Publish(new ToggleWindow(typeof(UIRotationWindow))));
        cmd.AddSubcommand("clear").SetSimpleHandler("clear current preset; autorotation will do nothing unless plan is active", () =>
        {
            Service.Log($"Console: clearing autorotation preset(s) '{_rotation.PresetNames}'");
            _rotation.Clear();
        });
        cmd.AddSubcommand("disable").SetSimpleHandler("force disable autorotation; no actions will be executed automatically even if plan is active", () =>
        {
            Service.Log($"Console: force-disabling from presets '{_rotation.PresetNames}'");
            _rotation.SetForceDisabled();
        });
        cmd.AddSubcommand("set").SetComplexHandler("<preset>", "start executing specified preset, and deactivate others", preset =>
        {
            SetOrToggleByName(preset, false, true);
            return true;
        });
        var toggle = cmd.AddSubcommand("toggle");
        toggle.SetSimpleHandler("force disable autorotation if not already; otherwise clear overrides", () => SetOrToggle(RotationModuleManager.ForceDisable, true, true));
        toggle.SetComplexHandler("<preset>", "start executing specified preset unless it's already active; clear otherwise", preset =>
        {
            SetOrToggleByName(preset, true, true);
            return true;
        });

        cmd.AddSubcommand("activate").SetComplexHandler("<preset>", "add specified preset to active list", preset =>
        {
            SetOrToggleByName(preset, false, false);
            return true;
        });
        cmd.AddSubcommand("deactivate").SetComplexHandler("<preset>", "remove specified preset from active list", preset =>
        {
            ClearByName(preset);
            return true;
        });
        cmd.AddSubcommand("togglemulti").SetComplexHandler("<preset>", "if specified preset is in active list, disable it, otherwise enable it", preset =>
        {
            SetOrToggleByName(preset, true, false);
            return true;
        });
    }

    private void RegisterAISlashCommands(SlashCommandHandler cmd)
    {
        cmd.SetSimpleHandler("toggle multibox ui", () =>
        {
            aiConfig.DrawUI ^= true;
            aiConfig.Modified.Fire();
        });
        cmd.AddSubcommand("on").SetSimpleHandler("enable AI mode", () =>
        {
            var was = aiConfig.Enabled;
            aiConfig.Enabled = true;
            aiConfig.Modified.Fire();
            if (aiConfig.ChatConfirmation && !was)
                Service.ChatMessage("AI enabled");
        });
        cmd.AddSubcommand("off").SetSimpleHandler("disable AI mode", () =>
        {
            var was = aiConfig.Enabled;
            aiConfig.Enabled = false;
            aiConfig.Modified.Fire();
            if (aiConfig.ChatConfirmation && was)
                Service.ChatMessage("AI disabled");
        });
        cmd.AddSubcommand("toggle").SetSimpleHandler("toggle AI mode", () =>
        {
            aiConfig.Enabled ^= true;
            aiConfig.Modified.Fire();
            if (aiConfig.ChatConfirmation)
                Service.ChatMessage($"AI {(aiConfig.Enabled ? "enabled" : "disabled")}");
        });
        cmd.AddSubcommand("follow").SetComplexHandler("<name>/slot<N>", "enable multibox mode and follow party member with specified name or at specified slot", masterString =>
        {
            var ms = masterString.ToString();
            var masterSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? int.Parse(masterString[4..]) - 1 : Array.FindIndex(_ws.Party.Members, m => _ws.Actors.Find(m.InstanceId)?.Name == ms);
            if (_ws.Party[masterSlot] != null)
            {
                aiWindow.SetSlot(masterSlot);
                if (aiConfig.ChatConfirmation)
                    Service.ChatMessage($"AI follow slot = {masterSlot}");
            }
            else
                Service.ChatError($"Error: can't find {masterString} in our party");
            return true;
        });

        // TODO: this should really be removed, it's a weird synonym for /vbm cfg AIConfig ...
        cmd.SetComplexHandler("", "", args =>
        {
            Service.Log($"vbmai args: {args}");
            Span<Range> ranges = stackalloc Range[2];
            var numRanges = args.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (numRanges == 1)
            {
                // toggle
                var value = Service.Config.ConsoleCommand($"AIConfig {args}");
                return bool.TryParse(value[0], out var boolValue) && Service.Config.ConsoleCommand($"AIConfig {args} {!boolValue}").Count == 0;
            }
            else if (numRanges == 2)
            {
                // set
                var value = args[ranges[1]];
                if (value.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    value = "true";
                else if (value.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    value = "false";
                return Service.Config.ConsoleCommand($"AIConfig {args[ranges[0]]} {value}").Count == 0;
            }
            return false;
        });
    }

    private void HelpMe()
    {
        StringBuilder diag = new();

        var i = 0;

        diag.AppendLine($"VBM {typeof(TickService).Assembly.GetName().Version}");
        diag.AppendLine($"current zone: {_ws.CurrentZone}/{_ws.CurrentCFCID}");

        var player = _ws.Party.Player();

        if (player != null)
        {
            diag.AppendLine();
            diag.AppendLine("player:");
            diag.AppendLine($"L{player.Level} {player.Class}, HP {player.HPMP.CurHP}/{player.HPMP.MaxHP}, MP {player.HPMP.CurMP}/{player.HPMP.MaxMP}");

            diag.AppendLine();
            diag.AppendLine("statuses:");
            foreach (var sid in player.Statuses)
            {
                if (sid.ID == 0)
                    continue;
                var desc = Service.LuminaRow<Lumina.Excel.Sheets.Status>(sid.ID);
                diag.AppendLine($"status [{i++}]: {sid.ID} '{desc?.Name ?? "unknown"}' p=0x{sid.Extra:X} ({(sid.ExpireAt - _ws.CurrentTime).TotalSeconds:f1})");
            }
        }

        diag.AppendLine();
        diag.AppendLine("autorot:");
        foreach (var m in _rotation.Presets)
        {
            diag.AppendLine($"{m.Name}");
            foreach (var mod in m.Modules)
            {
                diag.AppendLine($"- {mod.Definition.DisplayName} ({mod.Type.FullName})");
                foreach (var s in mod.TransientSettings)
                {
                    var track = mod.Definition.Configs[s.Track];
                    diag.AppendLine($"  - {track.InternalName} = {track.ToDisplayString(s.Value)}");
                }
            }
        }

        diag.AppendLine();
        diag.AppendLine("modules:");
        foreach (var m in _bossmod.LoadedModules)
        {
            var s = m.StateMachine.ActiveState == null ? "inactive" : "active";
            diag.AppendLine($"{m.GetType()} ({s}) (OID=0x{m.PrimaryActor.OID:X})");
        }

        if (player != null)
        {
            diag.AppendLine();
            diag.AppendLine("bitmap:");

            var (entry, _) = _hintsBuilder.Obstacles.Find(player.PosRot.XYZ());
            if (entry != null)
                diag.AppendLine(entry.ToString());

            var (map, rect) = _hints.PathfindMapObstacles;
            diag.AppendLine($"current bounds: {rect}");

            if (map != null)
            {
                var offX = -rect.Left;
                var offY = -rect.Top;
                var r = rect.Clamped(map.FullRect).Clamped(new(0, 0, map.Width, map.Height), offX, offY);
                var allPx = (r.Bottom - r.Top) * (r.Right - r.Left);
                var filledPx = 0;
                for (var y = r.Top; y < r.Bottom; ++y)
                    for (var x = r.Left; x < r.Right; ++x)
                        if (map[x, y])
                            filledPx++;

                diag.AppendLine($"map is {filledPx}/{allPx} filled ({filledPx / (float)allPx * 100:f2}%)");
            }
        }

        ImGui.SetClipboardText($"```{diag.ToString()}```");

        Service.ChatMessage("Diagnostic data has been copied to your clipboard.");
    }

    void OpenConfigUI(string tab = "")
    {
        MediatorService.Publish(new OpenSettingsWindow(tab));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _slashCmd.Register();
        _slashCmd.RegisterAlias("/vbmai", "ai"); // TODO: deprecated

    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _slashCmd.Unregister();
    }
}
