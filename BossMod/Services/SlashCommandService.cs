using BossMod.Autorotation;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class SlashCommandService(
    ICommandManager commandManager,
    RotationModuleManager autorot,
    AI.AIManager aiManager,
    WorldState ws,
    AI.AIWindow wndAI,
    ReplayManagementWindow wndReplay,
    ConfigUI uiConfig,
    UIRotationWindow wndRotation,
    MainDebugWindow wndDebug
) : IHostedService
{
    readonly SlashCommandProvider _slashCmd = new(commandManager, "/vbm");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _slashCmd.SetSimpleHandler("show boss mod settings UI", () => uiConfig.Open());
        _slashCmd.AddSubcommand("r").SetSimpleHandler("show/hide replay management window", () => wndReplay.SetVisible(!wndReplay.IsOpen));
        RegisterAutorotationSlashCommands(_slashCmd.AddSubcommand("ar"));
        RegisterAISlashCommands(_slashCmd.AddSubcommand("ai"));
        _slashCmd.AddSubcommand("cfg").SetComplexHandler("<config-type> <field> [<value>]", "query or modify configuration setting", args =>
        {
            var output = Service.Config.ConsoleCommand(args);
            foreach (var msg in output)
                Service.ChatGui.Print(msg);
            return true;
        });
        _slashCmd.AddSubcommand("d").SetSimpleHandler("show debug UI", wndDebug.OpenAndBringToFront);
        _slashCmd.AddSubcommand("gc").SetSimpleHandler("execute C# garbage collector", () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        });

        _slashCmd.Register();
        _slashCmd.RegisterAlias("/vbmai", "ai"); // TODO: deprecated

    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _slashCmd.Dispose();
    }

    private void RegisterAutorotationSlashCommands(SlashCommandHandler cmd)
    {
        void SetOrToggle(Preset preset, bool toggle, bool exclusive)
        {
            if (toggle)
            {
                var verb = autorot.Presets.Contains(preset) ? "disables" : "enables";
                Service.Log($"Console: toggle {verb} preset '{preset.Name}'");
                autorot.Toggle(preset, exclusive);
            }
            else
            {
                Service.Log($"Console: set activates preset '{preset.Name}'");
                autorot.Activate(preset, exclusive);
            }
        }

        void SetOrToggleByName(ReadOnlySpan<char> presetName, bool toggle, bool exclusive)
        {
            var preset = autorot.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
                SetOrToggle(preset, toggle, exclusive);
            else
                Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }

        void ClearByName(ReadOnlySpan<char> presetName)
        {
            var preset = autorot.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
            {
                Service.Log($"Console: unset deactivates preset '{preset.Name}'");
                autorot.Deactivate(preset);
            }
            else
                Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }

        cmd.SetSimpleHandler("toggle autorotation ui", () => wndRotation.SetVisible(!wndRotation.IsOpen));
        cmd.AddSubcommand("clear").SetSimpleHandler("clear current preset; autorotation will do nothing unless plan is active", () =>
        {
            Service.Log($"Console: clearing autorotation preset(s) '{autorot.PresetNames}'");
            autorot.Clear();
        });
        cmd.AddSubcommand("disable").SetSimpleHandler("force disable autorotation; no actions will be executed automatically even if plan is active", () =>
        {
            Service.Log($"Console: force-disabling from presets '{autorot.PresetNames}'");
            autorot.SetForceDisabled();
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
        cmd.SetSimpleHandler("toggle AI ui", () => wndAI.SetVisible(!wndAI.IsOpen));
        cmd.AddSubcommand("on").SetSimpleHandler("enable AI mode", () => aiManager.Enabled = true);
        cmd.AddSubcommand("off").SetSimpleHandler("disable AI mode", () => aiManager.Enabled = false);
        cmd.AddSubcommand("toggle").SetSimpleHandler("toggle AI mode", () => aiManager.Enabled ^= true);
        cmd.AddSubcommand("follow").SetComplexHandler("<name>/slot<N>", "enable AI mode and follow party member with specified name or at specified slot", masterString =>
        {
            var masterSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? int.Parse(masterString[4..]) - 1 : ws.Party.FindSlot(masterString);
            if (ws.Party[masterSlot] != null)
            {
                aiManager.SwitchToFollow(masterSlot);
                aiManager.Enabled = true;
            }
            else
            {
                Service.ChatGui.PrintError($"[AI] [Follow] Error: can't find {masterString} in our party");
            }
            return true;
        });

        // TODO: this should really be removed, it's a weird synonym for /vbm cfg AIConfig ...
        cmd.SetComplexHandler("", "", args =>
        {
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
}
