using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod.Autorotation;

public static class UIStrategyValue
{
    private static readonly (string Name, float Value)[] PriorityBaselines =
    [
        ("Very Low", ActionQueue.Priority.VeryLow),
        ("Low", ActionQueue.Priority.Low),
        ("Medium", ActionQueue.Priority.Medium),
        ("High", ActionQueue.Priority.High),
        ("Very High", ActionQueue.Priority.VeryHigh),
    ];

    public static List<string> Preview(ref StrategyValue value, StrategyConfig cfg, ModuleRegistry.Info? moduleInfo)
    {
        var opt = cfg.Options[value.Option];
        return [
            $"Option: {opt.UIName}",
            $"Comment: {value.Comment}",
            $"Priority: {(float.IsNaN(value.PriorityOverride) ? $"default ({opt.DefaultPriority:f})" : value.PriorityOverride.ToString("f"))}",
            $"Target: {PreviewTarget(ref value, moduleInfo)}"
        ];
    }

    public static string PreviewTarget(ref StrategyValue value, ModuleRegistry.Info? moduleInfo)
    {
        var targetDetails = value.Target switch
        {
            StrategyTarget.PartyByAssignment => ((PartyRolesConfig.Assignment)value.TargetParam).ToString(),
            StrategyTarget.PartyWithLowestHP => $"{(value.TargetParam != 0 ? "include" : "exclude")} self",
            StrategyTarget.EnemyByOID => $"{(moduleInfo?.ObjectIDType != null ? Enum.ToObject(moduleInfo.ObjectIDType, (uint)value.TargetParam).ToString() : "???")} (0x{value.TargetParam:X})",
            _ => ""
        };
        return targetDetails.Length > 0 ? $"{value.Target} ({targetDetails})" : $"{value.Target}";
    }

    public static bool DrawEditor(ref StrategyValue value, StrategyConfig cfg, ModuleRegistry.Info? moduleInfo, int? level)
    {
        var modified = false;
        modified |= DrawEditorOption(ref value, cfg, level);
        modified |= ImGui.InputText("Comment", ref value.Comment, 512);
        modified |= DrawEditorPriority(ref value);
        modified |= DrawEditorTarget(ref value, cfg.Options[value.Option].SupportedTargets, moduleInfo);
        return modified;
    }

    public static bool DrawEditorOption(ref StrategyValue value, StrategyConfig cfg, int? level)
    {
        var modified = false;
        using (var combo = ImRaii.Combo("Option", cfg.Options[value.Option].UIName))
        {
            if (combo)
            {
                for (int i = 0; i < cfg.Options.Count; ++i)
                {
                    var opt = cfg.Options[i];
                    if (level < opt.MinLevel || level > opt.MaxLevel)
                        continue; // filter out options outside our level

                    if (ImGui.Selectable(cfg.Options[i].UIName, i == value.Option))
                    {
                        modified = true;
                        value.Option = i;
                    }
                }
            }
        }
        return modified;
    }

    public static bool DrawEditorPriority(ref StrategyValue value)
    {
        var modified = false;
        var overridePriority = !float.IsNaN(value.PriorityOverride);
        if (ImGui.Checkbox("Override priority", ref overridePriority))
        {
            modified = true;
            value.PriorityOverride = overridePriority ? ActionQueue.Priority.Low : float.NaN;
        }
        ImGui.SameLine();
        UIMisc.HelpMarker("""
            Define custom priority for the corresponding action.
            Priority is compared against other candidate actions; it is suggested to use a predefined base and add a small offset to disambiguate multiple actions.
            Base priorities are the following:
            * Very Low (1000) - action will be used only if there is nothing else to press.
            * Low (2000) - action will be used only if it won't delay any dps action (it might delay eg. spending a second charge when there is no risk of overcapping).
            * Medium (3000) - action will be used in next possible ogcd slot, but it won't delay gcd or any extremely important ogcds; you can expect to have at least 1 slot for medium actions per gcd.
            * High (4000) - action will be used in the next possible ogcd slot; it won't delay gcd, but might break the rotation in some cases if not used carefully.
            * Very High (5000) - action will be used asap; will delay gcd if needed.
            """);

        if (overridePriority)
        {
            var priority = value.PriorityOverride;
            int upperBound = Array.FindIndex(PriorityBaselines, b => b.Value > priority);
            var baselineIndex = upperBound switch
            {
                -1 => PriorityBaselines.Length - 1,
                0 => 0,
                _ => upperBound - 1
            };
            var priorityDelta = value.PriorityOverride - PriorityBaselines[baselineIndex].Value;

            using var indent = ImRaii.PushIndent();
            ImGui.SetNextItemWidth(100);
            using (var combo = ImRaii.Combo("###baseline", PriorityBaselines[baselineIndex].Name))
            {
                if (combo)
                {
                    for (int i = 0; i < PriorityBaselines.Length; ++i)
                    {
                        if (ImGui.Selectable(PriorityBaselines[i].Name, i == baselineIndex))
                        {
                            modified = true;
                            value.PriorityOverride = PriorityBaselines[i].Value + priorityDelta;
                        }
                    }
                }
            }
            ImGui.SameLine();
            ImGui.TextUnformatted("+");
            ImGui.SameLine();
            if (ImGui.InputFloat("###delta", ref priorityDelta))
            {
                modified = true;
                value.PriorityOverride = PriorityBaselines[baselineIndex].Value + priorityDelta;
            }
        }

        return modified;
    }

    public static bool DrawEditorTarget(ref StrategyValue value, ActionTargets supportedTargets, ModuleRegistry.Info? moduleInfo)
    {
        var modified = false;
        using (var combo = ImRaii.Combo("Target", value.Target.ToString()))
        {
            if (combo)
            {
                for (var i = StrategyTarget.Automatic; i < StrategyTarget.Count; ++i)
                {
                    if (AllowTarget(i, supportedTargets, moduleInfo) && ImGui.Selectable(i.ToString(), i == value.Target))
                    {
                        value.Target = i;
                        value.TargetParam = 0;
                        modified = true;
                    }
                }
            }
        }

        using var indent = ImRaii.PushIndent();
        switch (value.Target)
        {
            case StrategyTarget.PartyByAssignment:
                var assignment = (PartyRolesConfig.Assignment)value.TargetParam;
                if (UICombo.Enum("Assignment", ref assignment))
                {
                    value.TargetParam = (int)assignment;
                    modified = true;
                }
                break;
            case StrategyTarget.PartyWithLowestHP:
                if (supportedTargets.HasFlag(ActionTargets.Self))
                {
                    var includeSelf = value.TargetParam != 0;
                    if (ImGui.Checkbox("Allow self", ref includeSelf))
                    {
                        value.TargetParam = includeSelf ? 1 : 0;
                        modified = true;
                    }
                }
                break;
            case StrategyTarget.EnemyByOID:
                if (moduleInfo?.ObjectIDType != null)
                {
                    var v = (Enum)Enum.ToObject(moduleInfo.ObjectIDType, (uint)value.TargetParam);
                    if (UICombo.Enum("OID", ref v))
                    {
                        value.TargetParam = (int)(uint)(object)v;
                        modified = true;
                    }
                }
                break;
        }
        return modified;
    }

    public static bool AllowTarget(StrategyTarget t, ActionTargets supported, ModuleRegistry.Info? moduleInfo) => t switch
    {
        StrategyTarget.Self => supported.HasFlag(ActionTargets.Self),
        StrategyTarget.PartyByAssignment => supported.HasFlag(ActionTargets.Party),
        StrategyTarget.PartyWithLowestHP => supported.HasFlag(ActionTargets.Party),
        StrategyTarget.EnemyWithHighestPriority => supported.HasFlag(ActionTargets.Hostile),
        StrategyTarget.EnemyByOID => supported.HasFlag(ActionTargets.Hostile) && moduleInfo != null,
        _ => true
    };
}
