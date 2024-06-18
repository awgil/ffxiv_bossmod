using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;

public sealed class UIRotationWindow : UIWindow
{
    private readonly RotationModuleManager _mgr;
    private readonly AutorotationConfig _config = Service.Config.Get<AutorotationConfig>();

    public UIRotationWindow(RotationModuleManager mgr) : base("Autorotation", false, new(400, 400), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        _mgr = mgr;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = _config.ShowUI;

        // TODO: draw positionals
    }

    public override void Draw()
    {
        var player = _mgr.Player;
        if (player == null)
            return;

        using (ImRaii.PushColor(ImGuiCol.Button, 0xff000080, _mgr.Preset == RotationModuleManager.ForceDisable))
        {
            if (ImGui.Button("X"))
            {
                _mgr.Preset = _mgr.Preset == RotationModuleManager.ForceDisable ? null : RotationModuleManager.ForceDisable;
            }
        }

        foreach (var p in _mgr.Database.Presets.Presets.Where(p => p.Modules.Any(m => RotationModuleRegistry.Modules[m.Key].Definition.Classes[(int)player.Class])))
        {
            ImGui.SameLine();
            using var col = ImRaii.PushColor(ImGuiCol.Button, 0xff008080, _mgr.Preset == p);
            if (ImGui.Button(p.Name))
            {
                _mgr.Preset = _mgr.Preset == p ? null : p;
            }
        }

        var activeModule = _mgr.Bossmods.ActiveModule;
        if (activeModule != null)
        {
            if (ImGui.Button("Timeline"))
            {
                _ = new StateMachineWindow(activeModule);
            }

            if (activeModule.Info?.PlanLevel > 0)
            {
                ImGui.SameLine();
                var plans = _mgr.Database.Plans.GetPlans(activeModule.GetType(), player.Class);
                var newSel = UIPlanDatabaseEditor.DrawPlanCombo(plans, plans.SelectedIndex, "Plan");
                if (newSel != plans.SelectedIndex)
                {
                    plans.SelectedIndex = newSel;
                    _mgr.Database.Plans.ModifyManifest(activeModule.GetType(), player.Class);
                }
            }
        }

        // TODO: more fancy action history/queue...
        ImGui.TextUnformatted($"GCD={_mgr.WorldState.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining:f3}, AnimLock={_mgr.ActionManager.EffectiveAnimationLock:f3}+{_mgr.ActionManager.AnimationLockDelayEstimate:f3}, Combo={_mgr.ActionManager.ComboTimeLeft:f3}");
        foreach (var a in _mgr.Hints.ActionsToExecute.Entries)
        {
            ImGui.TextUnformatted($"> {a.Action} ({a.Priority:f2})");
        }
    }
}
