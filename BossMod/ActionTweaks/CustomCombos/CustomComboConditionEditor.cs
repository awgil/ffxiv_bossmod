using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.ActionTweaks.CustomCombos;

public static class CustomComboConditionEditor
{
    // ActionJob: class/job used for action lists; GaugeJob: class/job whose gauge fields are offered
    public readonly record struct Context(Class ActionJob, Class GaugeJob, Actor? Player, WorldState World);

    private const int MaxGroupDepth = 3;

    private static readonly (string Category, CustomComboConditionType[] Types)[] Categories =
    [
        ("Action", [CustomComboConditionType.ActionUsable, CustomComboConditionType.ActionReady, CustomComboConditionType.ActionCharges, CustomComboConditionType.UsedRecently]),
        ("Combo & timing", [CustomComboConditionType.LastComboAction, CustomComboConditionType.ComboTimeRemaining, CustomComboConditionType.CanWeave, CustomComboConditionType.CanLateWeave, CustomComboConditionType.CombatTime]),
        ("Me", [CustomComboConditionType.StatusOnSelf, CustomComboConditionType.PlayerHP, CustomComboConditionType.PlayerMP, CustomComboConditionType.PlayerLevel, CustomComboConditionType.InCombat, CustomComboConditionType.Moving, CustomComboConditionType.PetPresent]),
        ("Job gauge", [CustomComboConditionType.Gauge]),
        ("Target", [CustomComboConditionType.HasTarget, CustomComboConditionType.StatusOnTarget, CustomComboConditionType.TargetHP, CustomComboConditionType.TargetInActionRange, CustomComboConditionType.TargetDistance, CustomComboConditionType.Positional]),
        ("Enemies & encounter", [CustomComboConditionType.EnemiesNearPlayer, CustomComboConditionType.EnemiesNearTarget, CustomComboConditionType.InBossFight, CustomComboConditionType.RaidwideIncoming, CustomComboConditionType.TankbusterIncoming]),
        ("Party", [CustomComboConditionType.PartyMembersBelowHP, CustomComboConditionType.PartyAverageHP]),
    ];

    public static void LiveDot(bool? state)
    {
        var size = ImGui.GetFrameHeight();
        var pos = ImGui.GetCursorScreenPos();
        ImGui.Dummy(new(size * 0.6f, size));
        var color = state switch
        {
            true => 0xff50c050u,
            false => 0xff5050e0u,
            null => 0xff808080u,
        };
        ImGui.GetWindowDrawList().AddCircleFilled(pos + new Vector2(size * 0.3f, size * 0.5f), size * 0.22f, color);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(state switch { true => "Currently true", false => "Currently false", null => "n/a" });
    }

    // returns true if modified
    public static bool DrawConditionList(List<CustomComboCondition> list, ref CustomComboGroupMode mode, Context ctx, int depth)
    {
        var modified = false;
        using var indent = ImRaii.PushIndent();

        if (mode == CustomComboGroupMode.None)
            mode = CustomComboGroupMode.All;

        // nested groups show their mode on the group row
        if (depth == 0)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("when");
            if (list.Count > 1)
            {
                ImGui.SameLine();
                modified |= DrawModePicker(ref mode);
                ImGui.SameLine();
                ImGui.TextUnformatted("of these are true:");
            }
        }

        var remove = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            using var id = ImRaii.PushId(i);
            var (changed, delete) = DrawConditionItem(list[i], ctx, depth);
            modified |= changed;
            if (delete)
                remove = i;
        }
        if (remove >= 0)
        {
            list.RemoveAt(remove);
            modified = true;
        }

        if (DrawAddMenu(ctx) is { } added)
        {
            list.Add(added);
            modified = true;
        }
        if (depth + 1 < MaxGroupDepth)
        {
            ImGui.SameLine();
            if (ImGui.Button("+ Group"))
            {
                list.Add(new() { Group = CustomComboGroupMode.Any });
                modified = true;
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Add a nested group");
        }
        return modified;
    }

    private static (bool modified, bool delete) DrawConditionItem(CustomComboCondition c, Context ctx, int depth)
    {
        var modified = false;
        LiveDot(Service.CustomCombos?.EvaluateLive(c));
        ImGui.SameLine();
        if (UIMisc.ToggleButton("NOT###negate", c.Negate, 0xff3030b0))
        {
            c.Negate = !c.Negate;
            modified = true;
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(c.Negate ? "Uninvert condition" : "Invert condition");
        ImGui.SameLine();

        if (c.Group != CustomComboGroupMode.None)
        {
            Text("group:");
            modified |= DrawModePicker(ref c.Group);
            ImGui.SameLine();
            Text("of");
            var delete = DeleteButton(c.Children.Count > 0);
            modified |= DrawConditionList(c.Children, ref c.Group, ctx, depth + 1);
            return (modified, delete);
        }

        modified |= DrawLeaf(c, ctx);
        return (modified, DeleteButton(false));
    }

    private static bool DeleteButton(bool requireShift)
    {
        bool clicked;
        using (ImRaii.Disabled(requireShift && !ImGui.GetIO().KeyShift))
            clicked = UIMisc.IconButton(FontAwesomeIcon.Times, "delete");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(requireShift ? "Hold shift to delete this group and everything in it" : "Delete");
        return clicked;
    }

    // note: every branch leaves cursor on the same line, so that caller can append the delete button
    private static bool DrawLeaf(CustomComboCondition c, Context ctx)
    {
        var m = false;
        switch (c.Type)
        {
            case CustomComboConditionType.ActionUsable:
                m |= Action(ref c.ID, ctx);
                Text("is usable");
                break;
            case CustomComboConditionType.ActionReady:
                m |= Action(ref c.ID, ctx);
                Text("is off cooldown, or will be within");
                m |= Float("##v", ref c.Value);
                Text("s");
                break;
            case CustomComboConditionType.ActionCharges:
                m |= Action(ref c.ID, ctx);
                Text("has at least");
                m |= Int("##n", ref c.Count);
                Text("charges");
                break;
            case CustomComboConditionType.UsedRecently:
                m |= Action(ref c.ID, ctx);
                Text("was used in the last");
                m |= Float("##v", ref c.Value);
                Text("s");
                break;

            case CustomComboConditionType.LastComboAction:
                Text("last combo action was");
                m |= Action(ref c.ID, ctx);
                break;
            case CustomComboConditionType.ComboTimeRemaining:
                Text("combo time left");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value);
                Text("s");
                break;
            case CustomComboConditionType.CanWeave:
                Text("an oGCD fits before the next GCD (oGCD takes");
                m |= Float("##v", ref c.Value);
                Text("s)");
                break;
            case CustomComboConditionType.CanLateWeave:
                Text("in the late-weave window (second half of the GCD)");
                break;
            case CustomComboConditionType.CombatTime:
                Text("time in combat");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value);
                Text("s");
                break;

            case CustomComboConditionType.StatusOnSelf:
            case CustomComboConditionType.StatusOnTarget:
                var onSelf = c.Type == CustomComboConditionType.StatusOnSelf;
                Text(onSelf ? "I have" : "target has");
                m |= CustomComboEditor.DrawStatusPicker(ref c.ID, onSelf ? ctx.Player : ctx.World.Actors.Find(ctx.Player?.TargetID ?? 0), onSelf);
                ImGui.SameLine();
                Text("with more than");
                m |= Float("##v", ref c.Value);
                Text("s left, at least");
                m |= Int("##n", ref c.Count);
                Text("stacks,");
                m |= ImGui.Checkbox("applied by me", ref c.OnlySelf);
                ImGui.SameLine();
                break;
            case CustomComboConditionType.PlayerHP:
                Text("my HP");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value);
                Text("%");
                break;
            case CustomComboConditionType.PlayerMP:
                Text("my MP");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value, 70);
                break;
            case CustomComboConditionType.PlayerLevel:
                Text("my level is at least");
                m |= Float("##v", ref c.Value);
                break;
            case CustomComboConditionType.InCombat:
                Text("I'm in combat");
                break;
            case CustomComboConditionType.Moving:
                Text("I'm moving");
                break;
            case CustomComboConditionType.PetPresent:
                Text("my pet is summoned");
                break;

            case CustomComboConditionType.Gauge:
                m |= DrawGauge(c, ctx);
                break;

            case CustomComboConditionType.HasTarget:
                Text("I have a target");
                break;
            case CustomComboConditionType.TargetHP:
                Text("target HP");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value);
                Text("%");
                break;
            case CustomComboConditionType.TargetInActionRange:
                Text("target is in range of");
                m |= Action(ref c.ID, ctx);
                break;
            case CustomComboConditionType.TargetDistance:
                Text("distance to target");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value);
                Text("y");
                break;
            case CustomComboConditionType.Positional:
                Text("I'm at the target's");
                m |= DrawPositional(ref c.Positional);
                break;

            case CustomComboConditionType.EnemiesNearPlayer:
            case CustomComboConditionType.EnemiesNearTarget:
                Text("at least");
                m |= Int("##n", ref c.Count);
                Text("enemies within");
                m |= Float("##v", ref c.Value);
                Text(c.Type == CustomComboConditionType.EnemiesNearPlayer ? "y of me" : "y of my target");
                break;
            case CustomComboConditionType.InBossFight:
                Text("in a boss fight (a bossmod module is active)");
                break;
            case CustomComboConditionType.RaidwideIncoming:
                Text("a raidwide hits me within");
                m |= Float("##v", ref c.Value);
                Text("s");
                break;
            case CustomComboConditionType.TankbusterIncoming:
                Text("a tankbuster hits me within");
                m |= Float("##v", ref c.Value);
                Text("s");
                break;

            case CustomComboConditionType.PartyMembersBelowHP:
                Text("at least");
                m |= Int("##n", ref c.Count);
                Text("party members are below");
                m |= Float("##v", ref c.Value);
                Text("% HP");
                break;
            case CustomComboConditionType.PartyAverageHP:
                Text("party average HP");
                m |= Compare(ref c.Compare);
                m |= Float("##v", ref c.Value);
                Text("%");
                break;

            default:
                Text($"unknown condition {c.Type}");
                break;
        }
        return m;
    }

    private static bool DrawGauge(CustomComboCondition c, Context ctx)
    {
        var m = false;
        var fields = CustomComboGauge.Fields(ctx.GaugeJob);
        if (fields.Count == 0)
        {
            Text(ctx.GaugeJob == Class.None ? "(set a class/job for this combo to pick a gauge value)" : $"({ctx.GaugeJob} has no gauge values)");
            return false;
        }

        var field = CustomComboGauge.FindField(ctx.GaugeJob, c.Field);
        ImGui.SetNextItemWidth(180 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##field", field?.Label ?? "<pick gauge value>"))
        {
            if (combo)
            {
                foreach (var f in fields)
                {
                    if (ImGui.Selectable(f.Label, f == field))
                    {
                        c.Field = f.Key;
                        (c.Compare, c.Value) = DefaultComparison(f);
                        m = true;
                    }
                }
            }
        }
        ImGui.SameLine();
        if (field == null)
            return m;

        switch (field.Kind)
        {
            case CustomComboGaugeKind.Bool:
                ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);
                using (var combo = ImRaii.Combo("##bool", c.Value != 0 ? "is true" : "is false"))
                {
                    if (combo)
                    {
                        if (ImGui.Selectable("is true", c.Value != 0))
                            (c.Compare, c.Value, m) = (CustomComboCompare.Equal, 1, true);
                        if (ImGui.Selectable("is false", c.Value == 0))
                            (c.Compare, c.Value, m) = (CustomComboCompare.Equal, 0, true);
                    }
                }
                ImGui.SameLine();
                break;
            case CustomComboGaugeKind.Choice:
                Text("is");
                ImGui.SetNextItemWidth(160 * ImGuiHelpers.GlobalScale);
                using (var combo = ImRaii.Combo("##choice", field.Format(c.Value)))
                {
                    if (combo)
                        foreach (var (label, value) in field.Options)
                            if (ImGui.Selectable(label, value == (int)c.Value))
                                (c.Compare, c.Value, m) = (CustomComboCompare.Equal, value, true);
                }
                ImGui.SameLine();
                break;
            default:
                m |= Compare(ref c.Compare);
                if (Float("##v", ref c.Value))
                {
                    c.Value = Math.Max(0, field.Max > 0 ? Math.Min(c.Value, field.Max) : c.Value);
                    m = true;
                }
                if (field.Unit.Length > 0)
                    Text(field.Unit);
                break;
        }

        if (ctx.Player?.Class == ctx.GaugeJob)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled($"(now {field.Format(field.Read(ctx.World))})");
            ImGui.SameLine();
        }
        return m;
    }

    private static (CustomComboCompare, float) DefaultComparison(CustomComboGaugeField f) => f.Kind switch
    {
        CustomComboGaugeKind.Bool => (CustomComboCompare.Equal, 1),
        CustomComboGaugeKind.Choice => (CustomComboCompare.Equal, f.Options.Length > 1 ? f.Options[1].Value : 0),
        _ => (CustomComboCompare.GreaterEqual, f.Max > 0 ? f.Max / 2 : 0)
    };

    private static CustomComboCondition? DrawAddMenu(Context ctx)
    {
        CustomComboCondition? res = null;
        if (ImGui.Button("+ Condition"))
            ImGui.OpenPopup("addcond");
        using var popup = ImRaii.Popup("addcond");
        if (!popup)
            return null;
        foreach (var (category, types) in Categories)
        {
            using var menu = ImRaii.Menu(category);
            if (!menu)
                continue;
            foreach (var t in types)
            {
                if (ImGui.MenuItem(TypeLabel(t)))
                    res = MakeDefault(t, ctx);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(TypeDescription(t));
            }
        }
        return res;
    }

    private static CustomComboCondition MakeDefault(CustomComboConditionType type, Context ctx)
    {
        var c = new CustomComboCondition() { Type = type };
        (c.Compare, c.Value, c.Count) = type switch
        {
            CustomComboConditionType.ActionCharges => (CustomComboCompare.GreaterEqual, 0, 1),
            CustomComboConditionType.UsedRecently => (CustomComboCompare.GreaterEqual, 3, 0),
            CustomComboConditionType.ComboTimeRemaining => (CustomComboCompare.Less, 5, 0),
            CustomComboConditionType.CanWeave => (CustomComboCompare.GreaterEqual, 0.6f, 0),
            CustomComboConditionType.CombatTime => (CustomComboCompare.GreaterEqual, 10, 0),
            CustomComboConditionType.PlayerHP => (CustomComboCompare.Less, 50, 0),
            CustomComboConditionType.PlayerMP => (CustomComboCompare.GreaterEqual, 1000, 0),
            CustomComboConditionType.PlayerLevel => (CustomComboCompare.GreaterEqual, 1, 0),
            CustomComboConditionType.Gauge => (CustomComboCompare.GreaterEqual, 50, 0),
            CustomComboConditionType.TargetHP => (CustomComboCompare.Less, 20, 0),
            CustomComboConditionType.TargetDistance => (CustomComboCompare.LessEqual, 3, 0),
            CustomComboConditionType.EnemiesNearPlayer or CustomComboConditionType.EnemiesNearTarget => (CustomComboCompare.GreaterEqual, 5, 3),
            CustomComboConditionType.RaidwideIncoming or CustomComboConditionType.TankbusterIncoming => (CustomComboCompare.GreaterEqual, 5, 0),
            CustomComboConditionType.PartyMembersBelowHP => (CustomComboCompare.GreaterEqual, 70, 2),
            CustomComboConditionType.PartyAverageHP => (CustomComboCompare.Less, 80, 0),
            _ => (CustomComboCompare.GreaterEqual, 0, 0)
        };
        if (type == CustomComboConditionType.Gauge && CustomComboGauge.Fields(ctx.GaugeJob).FirstOrDefault() is { } field)
        {
            c.Field = field.Key;
            (c.Compare, c.Value) = DefaultComparison(field);
        }
        return c;
    }

    private static string TypeLabel(CustomComboConditionType type) => type switch
    {
        CustomComboConditionType.ActionUsable => "Action is usable",
        CustomComboConditionType.ActionReady => "Action is off cooldown",
        CustomComboConditionType.ActionCharges => "Action has charges",
        CustomComboConditionType.UsedRecently => "Action was used recently",
        CustomComboConditionType.LastComboAction => "Last combo action",
        CustomComboConditionType.ComboTimeRemaining => "Combo time left",
        CustomComboConditionType.CanWeave => "Can weave",
        CustomComboConditionType.CanLateWeave => "Can late-weave",
        CustomComboConditionType.CombatTime => "Time in combat",
        CustomComboConditionType.StatusOnSelf => "Status on me",
        CustomComboConditionType.PlayerHP => "HP",
        CustomComboConditionType.PlayerMP => "MP",
        CustomComboConditionType.PlayerLevel => "Level",
        CustomComboConditionType.InCombat => "In combat",
        CustomComboConditionType.Moving => "Moving",
        CustomComboConditionType.PetPresent => "Pet summoned",
        CustomComboConditionType.Gauge => "Gauge value",
        CustomComboConditionType.HasTarget => "Has target",
        CustomComboConditionType.StatusOnTarget => "Status on target",
        CustomComboConditionType.TargetHP => "Target HP",
        CustomComboConditionType.TargetInActionRange => "Target in range of action",
        CustomComboConditionType.TargetDistance => "Distance to target",
        CustomComboConditionType.Positional => "Positional (rear/flank)",
        CustomComboConditionType.EnemiesNearPlayer => "Enemies near me",
        CustomComboConditionType.EnemiesNearTarget => "Enemies near target",
        CustomComboConditionType.InBossFight => "In boss fight",
        CustomComboConditionType.RaidwideIncoming => "Raidwide incoming",
        CustomComboConditionType.TankbusterIncoming => "Tankbuster incoming",
        CustomComboConditionType.PartyMembersBelowHP => "Party members below HP",
        CustomComboConditionType.PartyAverageHP => "Party average HP",
        _ => type.ToString()
    };

    private static string TypeDescription(CustomComboConditionType type) => type switch
    {
        CustomComboConditionType.ActionUsable => "Unlocked, off cooldown (GCDs count as ready while the GCD rolls) and usable right now: enough MP/gauge, required buff or proc active, etc.",
        CustomComboConditionType.ActionReady => "Only checks the action's own cooldown, ignoring resources and procs.",
        CustomComboConditionType.UsedRecently => "You used the action within the last few seconds.",
        CustomComboConditionType.CanWeave => "There's room for an oGCD before the next GCD without clipping.",
        CustomComboConditionType.CanLateWeave => "Second half of the GCD window, for oGCDs that should be weaved late.",
        CustomComboConditionType.Gauge => "Compare one of your job gauge values.",
        CustomComboConditionType.InBossFight => "A boss module is active for the current fight.",
        CustomComboConditionType.RaidwideIncoming => "The active bossmod module predicts upcoming raidwide.",
        CustomComboConditionType.TankbusterIncoming => "The active bossmod module predicts an upcoming tankbuster.",
        _ => TypeLabel(type)
    };

    // one-line summary, shown when collapsed
    public static string StepSummary(CustomComboStep step)
    {
        var action = step.Action != 0 ? CustomComboEditor.ActionName(step.Action) : "<none>";
        var conds = step.Conditions.Count == 0 ? "always" : $"when {ListSummary(step.Conditions, step.Match)}";
        var res = $"{action}: {conds}";
        return res.Length > 120 ? res[..117] + "..." : res;
    }

    private static string ListSummary(List<CustomComboCondition> list, CustomComboGroupMode mode)
        => string.Join(mode == CustomComboGroupMode.Any ? " or " : " and ", list.Select(Summary));

    private static string Summary(CustomComboCondition c)
    {
        var s = c.Group != CustomComboGroupMode.None ? $"({(c.Children.Count == 0 ? "always" : ListSummary(c.Children, c.Group))})" : LeafSummary(c);
        return c.Negate ? $"not {s}" : s;
    }

    private static string LeafSummary(CustomComboCondition c)
    {
        var a = c.ID != 0 ? CustomComboEditor.ActionName(c.ID) : "?";
        var st = c.ID != 0 ? CustomComboEditor.StatusName(c.ID) : "?";
        var cmp = $"{CompareSymbol(c.Compare)} {c.Value:0.##}";
        return c.Type switch
        {
            CustomComboConditionType.ActionUsable => $"{a} usable",
            CustomComboConditionType.ActionReady => c.Value > 0 ? $"{a} ready within {c.Value:0.##}s" : $"{a} off cooldown",
            CustomComboConditionType.ActionCharges => $"{a} has {Math.Max(1, c.Count)}+ charges",
            CustomComboConditionType.UsedRecently => $"{a} used in last {c.Value:0.##}s",
            CustomComboConditionType.LastComboAction => $"after {a}",
            CustomComboConditionType.ComboTimeRemaining => $"combo time {cmp}s",
            CustomComboConditionType.CanWeave => "can weave",
            CustomComboConditionType.CanLateWeave => "can late-weave",
            CustomComboConditionType.CombatTime => $"combat time {cmp}s",
            CustomComboConditionType.StatusOnSelf => $"I have {st}",
            CustomComboConditionType.PlayerHP => $"my HP {cmp}%",
            CustomComboConditionType.PlayerMP => $"my MP {cmp}",
            CustomComboConditionType.PlayerLevel => $"level {c.Value:0}+",
            CustomComboConditionType.InCombat => "in combat",
            CustomComboConditionType.Moving => "moving",
            CustomComboConditionType.PetPresent => "pet summoned",
            CustomComboConditionType.Gauge => CustomComboGauge.FindAnyField(c.Field) is { } f ? f.Kind == CustomComboGaugeKind.Number ? $"{f.Label} {cmp}{f.Unit}" : $"{f.Label}: {f.Format(c.Value)}" : "gauge ?",
            CustomComboConditionType.HasTarget => "has target",
            CustomComboConditionType.StatusOnTarget => $"target has {st}",
            CustomComboConditionType.TargetHP => $"target HP {cmp}%",
            CustomComboConditionType.TargetInActionRange => $"in range of {a}",
            CustomComboConditionType.TargetDistance => $"distance {cmp}y",
            CustomComboConditionType.Positional => $"at {c.Positional.ToString().ToLowerInvariant()}",
            CustomComboConditionType.EnemiesNearPlayer => $"{Math.Max(1, c.Count)}+ enemies near me",
            CustomComboConditionType.EnemiesNearTarget => $"{Math.Max(1, c.Count)}+ enemies near target",
            CustomComboConditionType.InBossFight => "boss fight",
            CustomComboConditionType.RaidwideIncoming => $"raidwide within {c.Value:0.##}s",
            CustomComboConditionType.TankbusterIncoming => $"tankbuster within {c.Value:0.##}s",
            CustomComboConditionType.PartyMembersBelowHP => $"{Math.Max(1, c.Count)}+ party below {c.Value:0.##}%",
            CustomComboConditionType.PartyAverageHP => $"party HP {cmp}%",
            _ => c.Type.ToString()
        };
    }

    private static string CompareSymbol(CustomComboCompare op) => op switch
    {
        CustomComboCompare.Less => "<",
        CustomComboCompare.LessEqual => "<=",
        CustomComboCompare.Equal => "=",
        CustomComboCompare.GreaterEqual => ">=",
        CustomComboCompare.Greater => ">",
        _ => "?"
    };

    private static void Text(string text)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(text);
        ImGui.SameLine();
    }

    private static bool Float(string id, ref float value, float width = 50)
    {
        ImGui.SetNextItemWidth(width * ImGuiHelpers.GlobalScale);
        var res = ImGui.InputFloat(id, ref value, 0, 0, "%g");
        ImGui.SameLine();
        return res;
    }

    private static bool Int(string id, ref int value)
    {
        ImGui.SetNextItemWidth(40 * ImGuiHelpers.GlobalScale);
        var res = ImGui.InputInt(id, ref value);
        if (res)
            value = Math.Max(0, value);
        ImGui.SameLine();
        return res;
    }

    private static bool Action(ref uint action, Context ctx)
    {
        var res = CustomComboEditor.DrawActionPicker("##action", ref action, ctx.ActionJob, width: 170);
        ImGui.SameLine();
        return res;
    }

    private static bool Compare(ref CustomComboCompare op)
    {
        var modified = false;
        ImGui.SetNextItemWidth(50 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##cmp", CompareSymbol(op)))
        {
            if (combo)
            {
                foreach (var o in Enum.GetValues<CustomComboCompare>())
                {
                    if (ImGui.Selectable(CompareSymbol(o), o == op))
                    {
                        op = o;
                        modified = true;
                    }
                }
            }
        }
        ImGui.SameLine();
        return modified;
    }

    private static bool DrawModePicker(ref CustomComboGroupMode mode)
    {
        var modified = false;
        ImGui.SetNextItemWidth(55 * ImGuiHelpers.GlobalScale);
        using var combo = ImRaii.Combo("##mode", mode == CustomComboGroupMode.Any ? "any" : "all");
        if (combo)
        {
            if (ImGui.Selectable("all", mode != CustomComboGroupMode.Any))
            {
                mode = CustomComboGroupMode.All;
                modified = true;
            }
            if (ImGui.Selectable("any", mode == CustomComboGroupMode.Any))
            {
                mode = CustomComboGroupMode.Any;
                modified = true;
            }
        }
        return modified;
    }

    private static bool DrawPositional(ref Positional positional)
    {
        var modified = false;
        ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##pos", positional.ToString().ToLowerInvariant()))
        {
            if (combo)
            {
                foreach (var p in (Positional[])[Positional.Rear, Positional.Flank, Positional.Front])
                {
                    if (ImGui.Selectable(p.ToString().ToLowerInvariant(), p == positional))
                    {
                        positional = p;
                        modified = true;
                    }
                }
            }
        }
        ImGui.SameLine();
        return modified;
    }
}
