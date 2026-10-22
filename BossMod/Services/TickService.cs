using Autofac.Features.AttributeFilters;
using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Dev;
using BossMod.Interfaces;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal sealed class TickService(
    [KeyFilter(Keys.ConfigDirectory)] DirectoryInfo configDir,
    [KeyFilter(Keys.ReplayDirectory)] DirectoryInfo replayDir,
    MediatorService mediator,
    ILogger<DisposableMediatorSubscriberBase> mLogger,
    IDalamudPluginInterface dalamud,
    ICommandManager commandManager,
    IUiBuilder uiBuilder,
    ICondition condition,
    IPluginLog logger,
    IClientState clientState,
    IWindowSystemFactory windowSystemFactory,
    IFileDialogManager fileDialogManager,
    ModulePackService packService,
    IAmex amex,
    IMovementOverride movementOverride,
    IWorldStateGameSync wsSync,
    IHintExecutor hintExecutor,
    BossModuleManager bossmod,
    RotationDatabase rotationDB,
    RotationModuleManager rotation,
    ZoneModuleManager zonemod,
    ReplayManager replayManager,
    WorldState worldstateGlobal,
    AIHintsBuilder hintsBuilder,
    AIHints hintsGlobal,
    IEnumerable<Func<MainDebugWindow>> mainDebugWindow
) : DisposableMediatorSubscriberBase(mLogger, mediator), IHostedService
{
#nullable disable
    private IWindowSystem windowSystem;

    private SlashCommandProvider _slashCmd;
    private TimeSpan _prevUpdateTime;

    private BossModuleMainWindow _wndBossmod;
    private BossModuleHintsWindow _wndBossmodHints;
    private ZoneModuleWindow _wndZone;
    private ReplayManagementWindow _wndReplay;
    private UIRotationWindow _wndRotation;
    private AIWindow _wndAI;
    private ConfigWindow _wndConfig;
    private ConfigChangelogWindow _wndChangelog;
#nullable restore
    private MainDebugWindow? _wndDebug;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Service.WindowSystem = windowSystem = windowSystemFactory.Create("vbm");

        MigratePlans(configDir.FullName);

        _slashCmd = new(commandManager, "/vbm");

        _wndConfig = new(new(Service.Config, worldstateGlobal, replayDir, rotationDB));
        _wndBossmod = new BossModuleMainWindow(bossmod, zonemod, hintsGlobal);
        _wndBossmodHints = new BossModuleHintsWindow(bossmod, zonemod, hintsGlobal);
        _wndZone = new ZoneModuleWindow(zonemod, rotation);
        _wndReplay = new ReplayManagementWindow(worldstateGlobal, bossmod, replayDir, replayManager);
        _wndRotation = new UIRotationWindow(rotation, amex);
        _wndAI = new AIWindow(rotation);
        _wndChangelog = new();

        if (Service.IsMock)
        {
            Service.Config.Get<ReplayManagementConfig>().ShowUI = _wndReplay.IsOpen = true;
            _ = new MainDevWindow(dalamud, packService) { IsOpen = true };
        }

        _wndDebug = mainDebugWindow.FirstOrDefault()?.Invoke();

        RegisterSlashCommands();

        Version cutoff = new(7, 5, 1, 13);
        if (Service.Config.PreviousAssemblyVersion < cutoff && Service.Config.AssemblyVersion >= cutoff)
        {
            Service.Log($"Removing potentially broken bitmaps from {hintsBuilder.Obstacles.UserRoot.FullName}");
            hintsBuilder.Obstacles.ClearGenerated();
        }

        // TODO: this should be in worldstate, but how?
        clientState.Logout += OnLogout;

        uiBuilder.Draw += Update;
        uiBuilder.DisableAutomaticUiHide = true;
        uiBuilder.OpenConfigUi += OpenUi;
        uiBuilder.OpenMainUi += OpenUi;

        condition.ConditionChange += OnConditionChanged;

        MediatorService.Subscribe<OpenWindow>(this, o =>
        {
            windowSystem.Windows.FirstOrDefault(w => w.GetType() == o.WindowType)?.IsOpen = true;
        });
        MediatorService.Subscribe<ToggleWindow>(this, t =>
        {
            windowSystem.Windows.FirstOrDefault(w => w.GetType() == t.WindowType)?.IsOpen ^= true;
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        clientState.Logout -= OnLogout;

        uiBuilder.Draw -= Update;
        uiBuilder.OpenConfigUi -= OpenUi;
        uiBuilder.OpenMainUi -= OpenUi;

        condition.ConditionChange -= OnConditionChanged;
    }

    void Update()
    {
        var tsStart = DateTime.Now;
        var moveImminent = movementOverride.IsMoveRequested() && (!amex.Config.PreventMovingWhileCasting || movementOverride.IsForceUnblocked());

        Service.Camera.Update();
        wsSync.Update(_prevUpdateTime);
        bossmod.Update();
        zonemod.ActiveModule?.Update();
        hintsBuilder.Update(hintsGlobal, PartyState.PlayerSlot, moveImminent);
        amex.QueueManualActions();
        rotation.Update(amex.AnimationLockDelayEstimate, movementOverride.IsMoving(), Service.Condition[ConditionFlag.DutyRecorderPlayback]);
        amex.FinishActionGather();
        wsSync.UpdateReplayState(rotation, movementOverride, hintsGlobal);

        var uiHidden = Service.GameGui.GameUiHidden || Service.Condition.Any(ConditionFlag.OccupiedInCutSceneEvent, ConditionFlag.WatchingCutscene78, ConditionFlag.WatchingCutscene);
        if (!uiHidden)
        {
            windowSystem.Draw();
            fileDialogManager.Draw();
        }

        hintExecutor.Execute();

        Service.Camera.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    void OnConditionChanged(ConditionFlag flag, bool value)
    {
        logger.Debug($"Condition change: {flag}={value}");
    }

    void OnLogout(int type, int code)
    {
        bossmod.ForceUnload($"logout type={type}, code={code}");
    }

    void OpenConfigUI(string tab = "")
    {
        MediatorService.Publish(new OpenSettingsWindow(tab));
    }

    void OpenUi() => OpenConfigUI();

    void RegisterSlashCommands()
    {
        _slashCmd.SetSimpleHandler("show boss mod settings UI", () => OpenConfigUI());
        _slashCmd.AddSubcommand("r").SetSimpleHandler("show/hide replay management window", () => _wndReplay.SetVisible(!_wndReplay.IsOpen));
        RegisterAutorotationSlashCommands(_slashCmd.AddSubcommand("ar"));
        RegisterAISlashCommands(_slashCmd.AddSubcommand("ai"));
        _slashCmd.AddSubcommand("cfg").SetComplexHandler("<config-type> <field> [<value>]", "query or modify configuration setting", args =>
        {
            var output = Service.Config.ConsoleCommand(args);
            foreach (var msg in output)
                Service.ChatGui.Print(msg);
            return true;
        });
        if (_wndDebug != null)
            _slashCmd.AddSubcommand("d").SetSimpleHandler("show debug UI", _wndDebug.OpenAndBringToFront);
        _slashCmd.AddSubcommand("gc").SetSimpleHandler("execute C# garbage collector", () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        });

        _slashCmd.AddSubcommand("clear-maps").SetSimpleHandler("clear all generated bitmaps (for pathfinding)", hintsBuilder.Obstacles.ClearGenerated);

        _slashCmd.AddSubcommand("macro").SetSimpleHandler("enables VBM action queue inside macros (by default, any actions used in macros bypass the queue)", () =>
        {
            unsafe
            {
                if (RaptureShellModule.Instance()->MacroCurrentLine >= 0)
                    amex.MacroCapture = true;
                else
                    Service.ChatMessage("That command doesn't do anything unless it's inside a macro.");
            }
        });
        _slashCmd.AddSubcommand("macro-off").SetSimpleHandler("disables VBM action queue inside a macro, if it has been enabled previously", () =>
        {
            unsafe
            {
                if (RaptureShellModule.Instance()->MacroCurrentLine >= 0)
                    amex.MacroCapture = false;
                else
                    Service.ChatMessage("That command doesn't do anything unless it's inside a macro.");
            }
        });

        _slashCmd.AddSubcommand("helpme").SetSimpleHandler("gather diagnostic information", HelpMe);

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
                Service.Log($"Console: toggle {verb} preset '{preset.Name}'");
                rotation.Toggle(preset, exclusive);
            }
            else
            {
                Service.Log($"Console: set activates preset '{preset.Name}'");
                rotation.Activate(preset, exclusive);
            }
        }

        void SetOrToggleByName(ReadOnlySpan<char> presetName, bool toggle, bool exclusive)
        {
            var preset = rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
                SetOrToggle(preset, toggle, exclusive);
            else
                Service.ChatError($"Failed to find preset '{presetName}'");
        }

        void ClearByName(ReadOnlySpan<char> presetName)
        {
            var preset = rotation.Database.Presets.FindPresetByName(presetName);
            if (preset != null)
            {
                Service.Log($"Console: unset deactivates preset '{preset.Name}'");
                rotation.Deactivate(preset);
            }
            else
                Service.ChatError($"Failed to find preset '{presetName}'");
        }

        cmd.SetSimpleHandler("toggle autorotation ui", () => _wndRotation.SetVisible(!_wndRotation.IsOpen));
        cmd.AddSubcommand("clear").SetSimpleHandler("clear current preset; autorotation will do nothing unless plan is active", () =>
        {
            Service.Log($"Console: clearing autorotation preset(s) '{rotation.PresetNames}'");
            rotation.Clear();
        });
        cmd.AddSubcommand("disable").SetSimpleHandler("force disable autorotation; no actions will be executed automatically even if plan is active", () =>
        {
            Service.Log($"Console: force-disabling from presets '{rotation.PresetNames}'");
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
        var aiConfig = Service.Config.Get<AIConfig>();
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
            var masterSlot = masterString.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ? int.Parse(masterString[4..]) - 1 : Array.FindIndex(worldstateGlobal.Party.Members, m => worldstateGlobal.Actors.Find(m.InstanceId)?.Name == ms);
            if (worldstateGlobal.Party[masterSlot] != null)
            {
                _wndAI.SetSlot(masterSlot);
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
        diag.AppendLine($"current zone: {worldstateGlobal.CurrentZone}/{worldstateGlobal.CurrentCFCID}");

        var player = worldstateGlobal.Party.Player();

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
                diag.AppendLine($"status [{i++}]: {sid.ID} '{desc?.Name ?? "unknown"}' p=0x{sid.Extra:X} ({(sid.ExpireAt - worldstateGlobal.CurrentTime).TotalSeconds:f1})");
            }
        }

        diag.AppendLine();
        diag.AppendLine("autorot:");
        foreach (var m in rotation.Presets)
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
        foreach (var m in bossmod.LoadedModules)
        {
            var s = m.StateMachine.ActiveState == null ? "inactive" : "active";
            diag.AppendLine($"{m.GetType()} ({s}) (OID=0x{m.PrimaryActor.OID:X})");
        }

        if (player != null)
        {
            diag.AppendLine();
            diag.AppendLine("bitmap:");

            var (entry, _) = hintsBuilder.Obstacles.Find(player.PosRot.XYZ());
            if (entry != null)
                diag.AppendLine(entry.ToString());

            var (map, rect) = hintsGlobal.PathfindMapObstacles;
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

    private static void MigratePlans(string pluginConfigDir)
    {
        var source = Path.Join(pluginConfigDir, "autorot");

        // no autorot dir means fresh install
        if (!Path.Exists(source))
        {
            Service.PluginLog.Verbose($"[Migrator] No configs, nothing to do.");
            return;
        }

        var destination = Path.Join(Plugin.GetStorageDir(), "autorot");

        if (File.Exists(Path.Join(destination, ".migrate-ok")))
        {
            Service.PluginLog.Verbose($"[Migrator] Nothing to do.");
            return;
        }

        Utils.CopyRecursive(source, destination);

        File.Create(Path.Join(destination, ".migrate-ok"));

        Service.Log($"[Migrator] Done.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _wndChangelog.Dispose();
        _wndAI.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndZone.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _wndConfig.Dispose();
        _slashCmd.Dispose();
    }
}
