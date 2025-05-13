using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;

public sealed class UIRotationWindow : UIWindow
{
    private readonly RotationModuleManager _mgr;
    private readonly ActionManagerEx _amex;
    private readonly AutorotationConfig _config = Service.Config.Get<AutorotationConfig>();
    private readonly EventSubscriptions _subscriptions;

    public UIRotationWindow(RotationModuleManager mgr, ActionManagerEx amex, Action openConfig) : base("Autorotation", false, new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        _mgr = mgr;
        _amex = amex;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.ShowUI)
        );
        RespectCloseHotkey = false;
        TitleBarButtons.Add(new() { Icon = FontAwesomeIcon.Cog, IconOffset = new(1), Click = _ => openConfig() });
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.ShowUI != vis)
        {
            _config.ShowUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void PreOpenCheck()
    {
        DrawPositional();
    }

    public override bool DrawConditions() => _mgr.WorldState.Party.Player() != null;

    public override void Draw()
    {
        var player = _mgr.Player;
        if (player == null)
            return;

        #region Presets
        DrawRotationSelector(_mgr);
        ImGui.Spacing();
        #endregion

        #region Cooldown Plans
        var activeModule = _mgr.Bossmods.ActiveModule;
        if (activeModule != null)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted($"Current CD Plan:");

            if (activeModule.Info?.PlanLevel > 0)
            {
                ImGui.SameLine();
                var plans = _mgr.Database.Plans.GetPlans(activeModule.GetType(), player.Class);
                var newSel = UIPlanDatabaseEditor.DrawPlanCombo(plans, plans.SelectedIndex, "");
                if (newSel != plans.SelectedIndex)
                {
                    plans.SelectedIndex = newSel;
                    _mgr.Database.Plans.ModifyManifest(activeModule.GetType(), player.Class);
                }

                ImGui.SameLine();
                if (ImGui.Button(plans.SelectedIndex >= 0 ? "Edit" : "New"))
                {
                    if (plans.SelectedIndex < 0)
                    {
                        var plan = new Plan($"New {plans.Plans.Count + 1}", activeModule.GetType()) { Guid = Guid.NewGuid().ToString(), Class = player.Class, Level = activeModule.Info.PlanLevel };
                        plans.SelectedIndex = plans.Plans.Count;
                        _mgr.Database.Plans.ModifyPlan(null, plan);
                    }
                    UIPlanDatabaseEditor.StartPlanEditor(_mgr.Database.Plans, plans.Plans[plans.SelectedIndex], activeModule.StateMachine);
                }

                if (newSel >= 0 && _mgr.Presets.Count > 0)
                {
                    ImGui.SameLine();
                    using var style = ImRaii.PushColor(ImGuiCol.Text, 0xff00ffff);
                    UIMisc.HelpMarker(() => "You have a preset activated, which fully overrides the CD plan!", FontAwesomeIcon.ExclamationTriangle);
                }
            }
        }
        ImGui.Spacing();
        #endregion

        #region Modules
        ImGui.TextUnformatted("Modules Active:");

        var dups = _mgr.DuplicateModules.ToList();
        if (dups.Count > 0)
        {
            ImGui.SameLine();
            using var style = ImRaii.PushColor(ImGuiCol.Text, 0xff00ffff);
            UIMisc.HelpMarker(() => $"You have multiple copies of the same module active ({string.Join(", ", dups)}). Only one of each will execute. This is probably not what you want.", FontAwesomeIcon.ExclamationTriangle);
        }

        if (_mgr.Presets.Any(p => p.Modules.Any(m => m.TransientSettings.Count > 0)))
        {
            ImGui.SameLine();
            using (ImRaii.PushColor(ImGuiCol.Text, 0xff00ff00))
                UIMisc.IconText(FontAwesomeIcon.BoltLightning, "(4)");
            if (ImGui.IsItemHovered())
            {
                using var tooltip = ImRaii.Tooltip();
                ImGui.TextUnformatted("Transient strategies:");
                foreach (var p in _mgr.Presets)
                {
                    foreach (var m in p.Modules.Where(m => m.TransientSettings.Count > 0))
                    {
                        ImGui.TextUnformatted($"> {m.Type.FullName}");
                        using var indent = ImRaii.PushIndent();
                        foreach (var s in m.TransientSettings)
                        {
                            var track = m.Definition.Configs[s.Track];
                            ImGui.TextUnformatted($"{track.InternalName} = {track.Options[s.Value.Option].InternalName}");
                        }
                    }
                }
            }
        }
        ImGui.SameLine();
        ImGui.TextUnformatted(_mgr.ToString());
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        #endregion

        #region Timers
        if (!_config.DisableTimers)
        {
            ImGui.TextUnformatted($"GCD={_mgr.WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining:f3}, AnimLock={_amex.EffectiveAnimationLock:f3}+{_amex.AnimationLockDelayEstimate:f3}, Combo={_amex.ComboTimeLeft:f3}, RBIn={_mgr.Bossmods.RaidCooldowns.NextDamageBuffIn():f3}");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
        #endregion

        if (_config.EnableGCDs)
        {
            #region Next GCD
            var isGCD = _mgr.Hints.ActionsToExecute.Entries.FirstOrDefault(a => a.Action.IsGCD());
            if (isGCD.Action)
            {
                ImGui.TextUnformatted("Next GCD: ");
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.2f, 1f), isGCD.Action.Name());
                ImGui.SameLine(0f, 0f);
                ImGui.TextUnformatted($" [{isGCD.Action.ID}] ({isGCD.Priority:F2}) @ ({isGCD.Target?.Name ?? "<none>"})");
            }
            else
            {
                ImGui.TextUnformatted("Next GCD: <none>");
            }
            #endregion

            #region Next OGCD
            var isOGCD = _mgr.Hints.ActionsToExecute.Entries.FirstOrDefault(a => !a.Action.IsGCD());
            if (isOGCD.Action)
            {
                ImGui.TextUnformatted("Next OGCD: ");
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.9f, 1f), isOGCD.Action.Name());
                ImGui.SameLine(0f, 0f);
                ImGui.TextUnformatted($" [{isOGCD.Action.ID}] ({isOGCD.Priority:F2}) @ ({isOGCD.Target?.Name ?? "<none>"})");
            }
            else
            {
                ImGui.TextUnformatted("Next OGCD: <none>");
            }
            #endregion

            #region Last Action Used
            var lastCast = _mgr.LastCast;
            var action = lastCast.Data!.Action;
            if (action)
            {
                ImGui.TextUnformatted("Last Action Used: ");
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(new Vector4(0.6f, 0.6f, 1.0f, 1f), action.Name());
                ImGui.SameLine(0f, 0f);
                ImGui.TextUnformatted($" [{action.ID}]");
            }
            else
            {
                ImGui.TextUnformatted("Last Action Used: <none>");
            }
            #endregion

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }

        #region Action Queue
        ImGui.TextUnformatted("Actions in Queue:");

        var queue = _mgr.Hints.ActionsToExecute.Entries;
        if (queue == null || queue.Count == 0)
        {
            ImGui.TextUnformatted("> <none>");
        }
        else
        {
            foreach (var e in queue)
            {
                if (_config.ShowGCDs)
                {
                    ImGui.TextUnformatted("> [");
                    ImGui.SameLine(0f, 0f);
                    ImGui.TextUnformatted(e.Action.CDType());
                    ImGui.SameLine(0f, 0f);
                    ImGui.TextUnformatted("] ");
                    ImGui.SameLine(0f, 0f);
                }
                if (!_config.ShowGCDs)
                    ImGui.TextUnformatted("> ");
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(new Vector4(1.0f, 0.843f, 0.0f, 1.0f), e.Action.Name());
                ImGui.SameLine(0f, 0f);
                ImGui.TextUnformatted($" [{e.Action.ID}] ({e.Priority:F2}) @ ({e.Target?.Name ?? "<none>"})");
            }
        }
        #endregion
    }

    public override void OnClose() => SetVisible(false);
    public static bool DrawRotationSelector(RotationModuleManager mgr)
    {
        var modified = false;
        if (mgr.Player == null)
            return modified;
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Presets:");
        ImGui.SameLine();

        using (ImRaii.PushColor(ImGuiCol.Button, 0xff000080, mgr.IsForceDisabled))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, 0xff000050, mgr.IsForceDisabled))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, 0xff000060, mgr.IsForceDisabled))
        {
            if (ImGui.Button("Disabled"))
            {
                if (mgr.IsForceDisabled)
                    mgr.Clear();
                else
                    mgr.SetForceDisabled();
                modified |= true;
            }
        }

        foreach (var p in mgr.Database.Presets.PresetsForClass(mgr.Player.Class))
        {
            var isActive = mgr.Presets.Contains(p);
            ImGui.SameLine();
            using var col = ImRaii.PushColor(ImGuiCol.Button, 0xff008080, isActive);
            using var colHovered = ImRaii.PushColor(ImGuiCol.ButtonHovered, 0xff005050, isActive);
            using var colActive = ImRaii.PushColor(ImGuiCol.ButtonActive, 0xff006060, isActive);
            if (ImGui.Button(p.Name))
            {
                mgr.Toggle(p);
                modified |= true;
            }
        }

        return modified;
    }

    private void DrawPositional()
    {
        var pos = _mgr.Hints.RecommendedPositional;
        if (_config.ShowPositionals && pos.Target != null && !pos.Target.Omnidirectional)
        {
            var color = PositionalColor(pos.Imminent, pos.Correct);
            switch (pos.Pos)
            {
                case Positional.Flank:
                    Camera.Instance?.DrawWorldCone(pos.Target.PosRot.XYZ(), pos.Target.HitboxRadius + 3.5f, pos.Target.Rotation + 90.Degrees(), 45.Degrees(), color);
                    Camera.Instance?.DrawWorldCone(pos.Target.PosRot.XYZ(), pos.Target.HitboxRadius + 3.5f, pos.Target.Rotation - 90.Degrees(), 45.Degrees(), color);
                    break;
                case Positional.Rear:
                    Camera.Instance?.DrawWorldCone(pos.Target.PosRot.XYZ(), pos.Target.HitboxRadius + 3.5f, pos.Target.Rotation + 180.Degrees(), 45.Degrees(), color);
                    break;
            }
        }
    }

    private uint PositionalColor(bool imminent, bool correct) => imminent
        ? (correct ? 0xff00ff00 : 0xff0000ff)
        : (correct ? 0xffffffff : 0xff00ffff);
}
