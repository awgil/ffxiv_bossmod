using BossMod.Autorotation;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal class CommandService(
    ConfigUI configUI,
    UIRotationWindow wndRotation,
    AI.AIWindow wndAI,
    ReplayManagementWindow wndReplay,
    IEnumerable<MainDebugWindow> wndDebug,
    RotationModuleManager rotation,
    AI.AIManager ai,
    WorldState worldState,
    SlashCommandProvider.Factory scf,
    IChatGui chat,
    ConfigRoot config,
    IPluginLog logger
) : IHostedService
{
    private readonly SlashCommandProvider _slashCmd = scf.Invoke("/vbm");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        RegisterSlashCommands();
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _slashCmd.Dispose();
    }

    private void RegisterSlashCommands()
    {
        _slashCmd.SetSimpleHandler("show boss mod settings UI", () => configUI.Open());
        _slashCmd.AddSubcommand("r").SetSimpleHandler("show/hide replay management window", () => wndReplay.SetVisible(!wndReplay.IsOpen));
        RegisterAutorotationSlashCommands(_slashCmd.AddSubcommand("ar"));
        RegisterAISlashCommands(_slashCmd.AddSubcommand("ai"));
        _slashCmd.AddSubcommand("cfg").SetComplexHandler("<config-type> <field> [<value>]", "query or modify configuration setting", args =>
        {
            var output = config.ConsoleCommand(args);
            foreach (var msg in output)
                chat.Print(msg);
            return true;
        });
        _slashCmd.AddSubcommand("d").SetSimpleHandler("show debug UI", () => wndDebug.FirstOrDefault()?.OpenAndBringToFront());
        _slashCmd.AddSubcommand("gc").SetSimpleHandler("execute C# garbage collector", () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        });

        _slashCmd.Register();
        _slashCmd.RegisterAlias("/vbmai", "ai"); // TODO: deprecated
    }

    private void RegisterAutorotationSlashCommands(SlashCommandHandler cmd)
    {
        void SetOrToggle(Preset preset, bool toggle, bool exclusive)
        {
            if (toggle)
            {
                var verb = rotation.Presets.Contains(preset) ? "disables" : "enables";
                logger.Debug($"Console: toggle {verb} preset '{preset.Name}'");
                rotation.Toggle(preset, exclusive);
            }
            else
            {
                logger.Debug($"Console: set activates preset '{preset.Name}'");
                rotation.Activate(preset, exclusive);
            }
        }

        void SetOrToggleByName(ReadOnlySpan<char> presetName, bool toggle, bool exclusive)
        {
            var preset = rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
                SetOrToggle(preset, toggle, exclusive);
            else
                chat.PrintError($"Failed to find preset '{presetName}'");
        }

        void ClearByName(ReadOnlySpan<char> presetName)
        {
            var preset = rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
            {
                logger.Debug($"Console: unset deactivates preset '{preset.Name}'");
                rotation.Deactivate(preset);
            }
            else
                chat.PrintError($"Failed to find preset '{presetName}'");
        }

        cmd.SetSimpleHandler("toggle autorotation ui", () => wndRotation.SetVisible(!wndRotation.IsOpen));
        cmd.AddSubcommand("clear").SetSimpleHandler("clear current preset; autorotation will do nothing unless plan is active", () =>
        {
            logger.Debug($"Console: clearing autorotation preset(s) '{rotation.PresetNames}'");
            rotation.Clear();
        });
        cmd.AddSubcommand("disable").SetSimpleHandler("force disable autorotation; no actions will be executed automatically even if plan is active", () =>
        {
            logger.Debug($"Console: force-disabling from presets '{rotation.PresetNames}'");
            rotation.SetForceDisabled();
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
        cmd.AddSubcommand("on").SetSimpleHandler("enable AI mode", () => ai.Enabled = true);
        cmd.AddSubcommand("off").SetSimpleHandler("disable AI mode", () => ai.Enabled = false);
        cmd.AddSubcommand("toggle").SetSimpleHandler("toggle AI mode", () => ai.Enabled ^= true);
        cmd.AddSubcommand("follow").SetComplexHandler("<name>/slot<N>", "enable AI mode and follow party member with specified name or at specified slot", masterString =>
        {
            var masterSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? int.Parse(masterString[4..]) - 1 : worldState.Party.FindSlot(masterString);
            if (worldState.Party[masterSlot] != null)
            {
                ai.SwitchToFollow(masterSlot);
                ai.Enabled = true;
            }
            else
            {
                chat.PrintError($"[AI] [Follow] Error: can't find {masterString} in our party");
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
                var value = config.ConsoleCommand($"AIConfig {args}");
                return bool.TryParse(value[0], out var boolValue) && config.ConsoleCommand($"AIConfig {args} {!boolValue}").Count == 0;
            }
            else if (numRanges == 2)
            {
                // set
                var value = args[ranges[1]];
                if (value.Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    value = "true";
                else if (value.Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    value = "false";
                return config.ConsoleCommand($"AIConfig {args[ranges[0]]} {value}").Count == 0;
            }
            return false;
        });
    }
}
