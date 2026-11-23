using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Text.Json;

namespace BossMod.ActionTweaks.CustomCombos;

public sealed class CustomComboEditor
{
    private static string _actionSearch = "";
    private static WorldState? _ws; // for showing actions as they look at player's level
    private static Class _displayJob; // class/job of the combo being drawn
    private string _importError = "";

    public void Draw(CustomComboConfig config, WorldState ws)
    {
        _ws = ws;
        var player = ws.Party.Player();
        var modified = false;

        UIMisc.SectionHeader("Combos");
        ImGui.TextWrapped("A combo replaces an action on your hotbar: conditional actions are checked first, then the combo chain.");

        if (ImGui.Button("New combo"))
        {
            config.Combos.Add(new() { Name = "New combo", Job = player?.Class ?? Class.None });
            modified = true;
        }
        ImGui.SameLine();
        if (ImGui.Button("New autorotation combo"))
        {
            config.Combos.Add(new()
            {
                Name = "Autorotation",
                Job = player?.Class ?? Class.None,
                Steps =
                [
                    new() { Action = CustomComboData.AutorotationNextOGCD, Conditions = [new() { Type = CustomComboConditionType.CanWeave, Value = 0.6f }] },
                    new() { Action = CustomComboData.AutorotationNextGCD },
                ]
            });
            modified = true;
        }
        ImGui.SameLine();
        modified |= DrawNewFromChain(config, player);
        ImGui.SameLine();
        if (ImGui.Button("Import from clipboard"))
            modified |= Import(config);
        if (_importError.Length > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(0xff4c4cff, _importError);
        }

        var remove = -1;
        (int A, int B) swap = (-1, -1);
        // grouped by class/job, in job picker order
        var jobOrder = GetJobGroups().SelectMany(g => g.Classes).Prepend(Class.None).ToList();
        foreach (var group in config.Combos.Select((c, i) => (Combo: c, Index: i)).GroupBy(e => e.Combo.Job).OrderBy(g => jobOrder.IndexOf(g.Key) is var o && o < 0 ? int.MaxValue : o))
        {
            using var jobId = ImRaii.PushId((int)group.Key);
            DrawJobIcon(group.Key);
            ImGui.SameLine();
            var flags = group.Key == Class.None || group.Key == player?.Class ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
            using var jobNode = ImRaii.TreeNode($"{JobName(group.Key)} ({group.Count()})###job", flags);
            if (!jobNode)
                continue;

            var entries = group.ToList();
            for (var k = 0; k < entries.Count; ++k)
            {
                var (combo, i) = entries[k];
                using var id = ImRaii.PushId(i);

                modified |= ImGui.Checkbox("##enabled", ref combo.Enabled);
                ImGui.SameLine();
                if (UIMisc.IconButton(FontAwesomeIcon.ArrowUp, "up") && k > 0)
                    swap = (entries[k - 1].Index, i);
                ImGui.SameLine();
                if (UIMisc.IconButton(FontAwesomeIcon.ArrowDown, "down") && k + 1 < entries.Count)
                    swap = (i, entries[k + 1].Index);
                ImGui.SameLine();
                if (UIMisc.IconButton(FontAwesomeIcon.FileExport, "export"))
                    ImGui.SetClipboardText(JsonSerializer.Serialize(combo, Serialization.BuildSerializationOptions()));
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Copy to clipboard");
                ImGui.SameLine();
                using (ImRaii.Disabled(!ImGui.GetIO().KeyShift))
                    if (UIMisc.IconButton(FontAwesomeIcon.Trash, "delete"))
                        remove = i;
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    ImGui.SetTooltip("Hold shift to delete");
                ImGui.SameLine();
                DrawPreviewIcon(combo);
                ImGui.SameLine();

                _displayJob = combo.Job;
                using var node = ImRaii.TreeNode($"{combo.Name} ({ActionName(combo.Trigger)})###combo");
                if (node)
                    modified |= DrawCombo(combo, player, ws);
            }
        }

        if (remove >= 0)
        {
            config.Combos.RemoveAt(remove);
            modified = true;
        }
        if (swap.A >= 0)
        {
            (config.Combos[swap.A], config.Combos[swap.B]) = (config.Combos[swap.B], config.Combos[swap.A]);
            modified = true;
        }

        if (modified)
            config.Modified.Fire();
    }

    private bool Import(CustomComboConfig config)
    {
        try
        {
            var combo = JsonSerializer.Deserialize<CustomCombo>(ImGui.GetClipboardText(), Serialization.BuildSerializationOptions());
            if (combo == null)
            {
                _importError = "Clipboard does not contain a combo";
                return false;
            }
            config.Combos.Add(combo);
            _importError = "";
            return true;
        }
        catch (Exception ex)
        {
            _importError = $"Failed to import: {ex.Message}";
            return false;
        }
    }

    private static void DrawPreviewIcon(CustomCombo combo)
    {
        var size = new Vector2(ImGui.GetFrameHeight());
        var action = CustomComboTweak.Current?.PreviewLive(combo) ?? 0;
        UIMisc.Image(action != 0 ? Service.Texture?.GetFromGameIcon(CustomComboData.ActionIcon(action)) : null, size);
        if (ImGui.IsItemHovered() && action != 0)
            ImGui.SetTooltip($"Right now this button shows {CustomComboData.ActionName(action)}");
    }

    private static bool DrawCombo(CustomCombo combo, Actor? player, WorldState ws)
    {
        var modified = false;

        ImGui.SetNextItemWidth(250);
        modified |= ImGui.InputText("Name", ref combo.Name, 256);
        modified |= DrawJobPicker(ref combo.Job);
        modified |= DrawActionPicker("Replaced action", ref combo.Trigger, combo.Job);

        _displayJob = combo.Job;
        var ctx = new CustomComboConditionEditor.Context(combo.Job, combo.Job != Class.None ? combo.Job : player?.Class ?? Class.None, player, ws);
        modified |= DrawSequence(combo);
        modified |= DrawOverrides(combo, ctx);
        if (combo.Sequence.Count == 0)
            ImGui.TextDisabled($"Otherwise: {ActionName(combo.Trigger)}");
        return modified;
    }

    private static bool DrawSequence(CustomCombo combo)
    {
        var modified = false;
        UIMisc.SectionHeader("Combo chain");
        ImGui.TextDisabled("For native combos ('1 -> 2 -> 3's)");

        var remove = -1;
        var swap = -1;
        for (var k = 0; k < combo.Sequence.Count; ++k)
        {
            using var id = ImRaii.PushId($"seq{k}");
            ImGui.AlignTextToFramePadding();
            var action = combo.Sequence[k];
            var prev = k > 0 ? combo.Sequence[k - 1] : 0;
            if (DrawActionPicker("##action", ref action, combo.Job, filter: SequenceFilter(combo.Job, prev)))
            {
                combo.Sequence[k] = action;
                modified = true;
            }
            ImGui.SameLine();
            if (UIMisc.IconButton(FontAwesomeIcon.ArrowUp, "up") && k > 0)
                swap = k - 1;
            ImGui.SameLine();
            if (UIMisc.IconButton(FontAwesomeIcon.ArrowDown, "down") && k + 1 < combo.Sequence.Count)
                swap = k;
            ImGui.SameLine();
            if (UIMisc.IconButton(FontAwesomeIcon.Trash, "delete"))
                remove = k;
            if (k > 0 && combo.Sequence[k] != 0 && combo.Sequence[k - 1] != 0 && !CustomComboData.ContinuesCombo(combo.Sequence[k - 1], combo.Sequence[k]))
            {
                ImGui.SameLine();
                using (ImRaii.PushColor(ImGuiCol.Text, 0xff40c0ff))
                    UIMisc.IconText(FontAwesomeIcon.ExclamationTriangle);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip($"{ActionName(combo.Sequence[k])} doesn't follow {ActionName(combo.Sequence[k - 1])} in a game combo, so the chain never reaches it.\nUse a conditional action instead.");
            }
        }
        if (remove >= 0)
        {
            combo.Sequence.RemoveAt(remove);
            modified = true;
        }
        if (swap >= 0)
        {
            (combo.Sequence[swap], combo.Sequence[swap + 1]) = (combo.Sequence[swap + 1], combo.Sequence[swap]);
            modified = true;
        }

        var last = combo.Sequence.Count > 0 ? combo.Sequence[^1] : 0;
        var addFilter = SequenceFilter(combo.Job, last);
        if (CustomComboData.ActionsForClass(combo.Job).Any(a => addFilter(a.ID)))
        {
            uint added = 0;
            if (DrawActionPicker("##add", ref added, combo.Job, combo.Sequence.Count == 0 ? "Start with..." : "then...", filter: addFilter) && added != 0)
            {
                combo.Sequence.Add(added);
                modified = true;
            }
        }
        return modified;
    }

    // first step: any combo action; later steps: actions continuing the combo from previous step
    private static Func<uint, bool> SequenceFilter(Class job, uint prev)
        => prev == 0 ? a => CustomComboData.IsComboAction(job, a) : a => CustomComboData.ContinuesCombo(prev, a);

    private static int JobLevel(Class job) => job != Class.None ? _ws?.Client.ClassJobLevel(job) ?? 0 : 0;
    private static uint DisplayAction(uint action) => CustomComboData.DisplayAction(CustomComboData.Canonical(action), JobLevel(_displayJob));

    private static void DrawSuggestionPreset(Class job)
    {
        var tweak = CustomComboTweak.Current;
        if (tweak == null || job == Class.None)
            return;
        var config = Service.Config.Get<CustomComboConfig>();
        var current = tweak.SuggestionPreset(job);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Use combo for preset: ");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200 * ImGuiHelpers.GlobalScale);
        using var combo = ImRaii.Combo("##preset", current?.Name ?? "<no preset for this job>");
        if (!combo)
            return;
        foreach (var p in tweak.SuggestionPresets(job))
        {
            if (ImGui.Selectable(p.Name, p == current))
            {
                config.AutorotationPreset = p.Name;
                config.Modified.Fire();
            }
        }
    }

    private static bool DrawOverrides(CustomCombo combo, CustomComboConditionEditor.Context ctx)
    {
        var modified = false;
        UIMisc.SectionHeader("Conditional actions");
        if (combo.Steps.Any(s => CustomComboData.IsAutorotation(s.Action)))
            DrawSuggestionPreset(ctx.GaugeJob);

        var removeStep = -1;
        var swapStep = -1;
        for (var k = 0; k < combo.Steps.Count; ++k)
        {
            using var id = ImRaii.PushId($"step{k}");
            var step = combo.Steps[k];

            modified |= ImGui.Checkbox("##enabled", ref step.Enabled);
            ImGui.SameLine();
            if (UIMisc.IconButton(FontAwesomeIcon.ArrowUp, "up") && k > 0)
                swapStep = k - 1;
            ImGui.SameLine();
            if (UIMisc.IconButton(FontAwesomeIcon.ArrowDown, "down") && k + 1 < combo.Steps.Count)
                swapStep = k;
            ImGui.SameLine();
            using (ImRaii.Disabled(!ImGui.GetIO().KeyShift && step.Conditions.Count > 0))
                if (UIMisc.IconButton(FontAwesomeIcon.Trash, "delete"))
                    removeStep = k;
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && step.Conditions.Count > 0)
                ImGui.SetTooltip("Hold shift to delete");
            ImGui.SameLine();
            CustomComboConditionEditor.LiveDot(CustomComboTweak.Current?.EvaluateLive(step));
            ImGui.SameLine();
            UIMisc.Image(step.Action != 0 ? Service.Texture?.GetFromGameIcon(PickerIcon(step.Action)) : null, new(ImGui.GetFrameHeight()));
            ImGui.SameLine();

            var dim = ImRaii.PushColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled), !step.Enabled);
            using var node = ImRaii.TreeNode($"{CustomComboConditionEditor.StepSummary(step)}###step");
            dim.Dispose();
            if (node)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("Use");
                ImGui.SameLine();
                modified |= DrawActionPicker("##action", ref step.Action, combo.Job, allowAutorotation: true);
                modified |= CustomComboConditionEditor.DrawConditionList(step.Conditions, ref step.Match, ctx, 0);
            }
        }

        if (removeStep >= 0)
        {
            combo.Steps.RemoveAt(removeStep);
            modified = true;
        }
        if (swapStep >= 0)
        {
            (combo.Steps[swapStep], combo.Steps[swapStep + 1]) = (combo.Steps[swapStep + 1], combo.Steps[swapStep]);
            modified = true;
        }

        if (ImGui.Button("Add conditional action"))
        {
            combo.Steps.Add(new());
            modified = true;
        }
        return modified;
    }

    private static readonly (string Label, ClassCategory Category)[] JobGroups =
    [
        ("Tanks", ClassCategory.Tank),
        ("Healers", ClassCategory.Healer),
        ("Melee DPS", ClassCategory.Melee),
        ("Physical ranged DPS", ClassCategory.PhysRanged),
        ("Magical ranged DPS", ClassCategory.Caster),
        ("Limited jobs", ClassCategory.Limited),
    ];
    private static List<(string Label, List<Class> Classes)>? _jobGroups;
    private static string _statusSearch = "";
    private static List<(uint ID, string Name, uint Icon)>? _statuses;

    private static uint JobIcon(Class job) => job == Class.None ? 0 : 62100u + (uint)job;
    private static string JobName(Class job)
    {
        if (job == Class.None)
            return "Any class/job";
        var row = Service.LuminaRow<Lumina.Excel.Sheets.ClassJob>((uint)job);
        return row != null ? $"{System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(row.Value.Name.ToString())} ({job})" : job.ToString();
    }

    internal static void DrawJobIcon(Class job)
    {
        var size = new Vector2(ImGui.GetFrameHeight());
        UIMisc.Image(job != Class.None ? Service.Texture?.GetFromGameIcon(JobIcon(job)) : null, size);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(JobName(job));
    }

    private static List<(string Label, List<Class> Classes)> GetJobGroups()
    {
        if (_jobGroups != null)
            return _jobGroups;
        static int priority(Class c) => Service.LuminaRow<Lumina.Excel.Sheets.ClassJob>((uint)c)?.UIPriority ?? 255;
        static bool isJob(Class c) => (Service.LuminaRow<Lumina.Excel.Sheets.ClassJob>((uint)c)?.JobIndex ?? 0) > 0;
        var all = Enum.GetValues<Class>().Where(c => c.GetClassCategory() != ClassCategory.Undefined).ToList();
        _jobGroups = [.. JobGroups.Select(g => (g.Label, all.Where(c => isJob(c) && c.GetClassCategory() == g.Category).OrderBy(priority).ToList()))];
        _jobGroups.Add(("Base classes", all.Where(c => !isJob(c)).OrderBy(priority).ToList()));
        return _jobGroups;
    }

    internal static bool DrawJobPicker(ref Class job)
    {
        var modified = false;
        var iconSize = new Vector2(ImGui.GetFrameHeight());
        DrawJobIcon(job);
        ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        ImGui.SetNextItemWidth(220 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##job", JobName(job), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (DrawIconEntry("any", 0, JobName(Class.None), job == Class.None, iconSize))
                    (job, modified) = (Class.None, true);
                foreach (var (label, classes) in GetJobGroups())
                {
                    if (classes.Count == 0)
                        continue;
                    ImGui.Spacing();
                    ImGui.TextDisabled(label);
                    foreach (var c in classes)
                        if (DrawIconEntry(c.ToString(), JobIcon(c), JobName(c), c == job, iconSize))
                            (job, modified) = (c, true);
                }
            }
        }
        ImGui.SameLine();
        ImGui.TextUnformatted("Class/job");
        return modified;
    }

    // autorotation actions show current suggestion
    private static uint PickerIcon(uint action)
        => CustomComboData.IsAutorotation(action) ? CustomComboData.ActionIcon(CustomComboTweak.Current?.PreviewRecommendation(action) ?? 0) : CustomComboData.ActionIcon(DisplayAction(action));

    internal static bool DrawActionPicker(string label, ref uint action, Class job, string? placeholder = null, float width = 220, Func<uint, bool>? filter = null, bool allowAutorotation = false)
    {
        var modified = false;
        using var id = ImRaii.PushId(label);

        var iconSize = new Vector2(ImGui.GetFrameHeight());
        UIMisc.Image(action != 0 ? Service.Texture?.GetFromGameIcon(PickerIcon(action)) : null, iconSize);
        ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);

        ImGui.SetNextItemWidth(width * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##list", action == 0 ? placeholder ?? "<none>" : ActionName(action), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.IsWindowAppearing())
                {
                    _actionSearch = "";
                    ImGui.SetKeyboardFocusHere();
                }
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##search", "Search by name or ID...", ref _actionSearch, 100);

                var autos = allowAutorotation ? CustomComboData.AutorotationActions.Where(a => _actionSearch.Length == 0 || CustomComboData.ActionName(a).Contains(_actionSearch, StringComparison.OrdinalIgnoreCase)).ToList() : [];
                // show actions as they look on hotbar at current level (e.g. heated split shot)
                var level = JobLevel(job);
                var actions = CustomComboData.ActionsForClass(job)
                    .Where(a => filter == null || filter(a.ID))
                    .Select(a => (Base: a, Shown: CustomComboData.DisplayAction(a.ID, level)))
                    .Select(e => (e.Base, e.Shown, Name: e.Shown == e.Base.ID ? e.Base.Name : CustomComboData.ActionName(e.Shown)))
                    .Where(e => _actionSearch.Length == 0 || e.Name.Contains(_actionSearch, StringComparison.OrdinalIgnoreCase) || e.Shown.ToString().StartsWith(_actionSearch, StringComparison.Ordinal))
                    .ToList();

                var style = ImGui.GetStyle();
                var contentHeight = Math.Max(1, autos.Count + actions.Count) * (iconSize.Y + style.ItemSpacing.Y) + (autos.Count > 0 ? style.ItemSpacing.Y + 1 : 0);
                using var list = ImRaii.Child("##actions", new(0, Math.Min(400 * ImGuiHelpers.GlobalScale, contentHeight)));
                if (list)
                {
                    foreach (var auto in autos)
                    {
                        if (DrawIconEntry(auto.ToString(), PickerIcon(auto), CustomComboData.ActionName(auto), auto == action, iconSize))
                        {
                            action = auto;
                            modified = true;
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    if (autos.Count > 0)
                        ImGui.Separator();

                    foreach (var (a, shown, name) in actions)
                    {
                        var shownLevel = CustomComboData.ActionLevel(shown);
                        var clicked = DrawIconEntry(a.ID.ToString(), CustomComboData.ActionIcon(shown), name, a.ID == CustomComboData.Canonical(action), iconSize, $"lv{shownLevel}");
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip(shown != a.ID ? $"{name}\nLevel {shownLevel} · ID {shown}\nUpgraded from {a.Name} (level {a.Level})" : $"{name}\nLevel {a.Level} · ID {a.ID}");
                        if (clicked)
                        {
                            action = a.ID;
                            modified = true;
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    if (autos.Count + actions.Count == 0)
                        ImGui.TextDisabled("(nothing found)");
                }
            }
        }
        if (!label.StartsWith("##", StringComparison.Ordinal))
        {
            ImGui.SameLine();
            ImGui.TextUnformatted(label);
        }
        return modified;
    }

    private static bool DrawIconEntry(string id, uint icon, string text, bool selected, Vector2 iconSize, string? detail = null)
    {
        var pos = ImGui.GetCursorPos();
        if (ImGui.IsRectVisible(new Vector2(Math.Max(1, ImGui.GetContentRegionAvail().X), iconSize.Y))) // don't load icons for entries out of view
        {
            UIMisc.Image(icon != 0 ? Service.Texture?.GetFromGameIcon(icon) : null, iconSize);
            var textY = pos.Y + 0.5f * (iconSize.Y - ImGui.GetTextLineHeight());
            ImGui.SameLine();
            ImGui.SetCursorPosY(textY);
            ImGui.TextUnformatted(text);
            if (detail != null)
            {
                ImGui.SameLine();
                ImGui.SetCursorPosY(textY);
                ImGui.TextDisabled(detail);
            }
            ImGui.SetCursorPos(pos);
        }
        return ImGui.Selectable($"###{id}", selected, ImGuiSelectableFlags.None, new(0, iconSize.Y));
    }

    private static List<(uint ID, string Name, uint Icon)> AllStatuses()
        => _statuses ??= [.. Service.LuminaSheet<Lumina.Excel.Sheets.Status>().Where(s => s.Icon != 0 && !s.Name.IsEmpty).Select(s => (s.RowId, s.Name.ToString(), s.Icon))];

    private static bool DrawStatusEntry(uint id, string name, uint icon, ref uint status, Vector2 iconSize)
    {
        var clicked = DrawIconEntry(id.ToString(), icon, name, id == status, iconSize);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"{name}\nID {id}\n{Service.LuminaRow<Lumina.Excel.Sheets.Status>(id)?.Description.ExtractText()}");
        if (!clicked)
            return false;
        status = id;
        ImGui.CloseCurrentPopup();
        return true;
    }

    internal static bool DrawStatusPicker(ref uint status, Actor? source, bool onSelf)
    {
        var modified = false;
        var iconSize = new Vector2(ImGui.GetFrameHeight() * 0.75f, ImGui.GetFrameHeight()); // status icons are 24x32
        UIMisc.Image(status != 0 ? Service.Texture?.GetFromGameIcon(StatusIcon(status)) : null, iconSize);
        ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        ImGui.SetNextItemWidth(180 * ImGuiHelpers.GlobalScale);
        using (var combo = ImRaii.Combo("##statuslist", status == 0 ? "<none>" : StatusName(status), ImGuiComboFlags.HeightLargest))
        {
            if (combo)
            {
                if (ImGui.IsWindowAppearing())
                {
                    _statusSearch = "";
                    ImGui.SetKeyboardFocusHere();
                }
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##search", "Search all statuses by name or ID...", ref _statusSearch, 100);

                var current = _statusSearch.Length == 0 ? (source?.Statuses ?? []).Select(s => s.ID).Where(id => id != 0).Distinct().ToList() : null;
                var maxHeight = 400 * ImGuiHelpers.GlobalScale;
                var height = current != null ? Math.Min(maxHeight, ImGui.GetTextLineHeightWithSpacing() + Math.Max(1, current.Count) * (iconSize.Y + ImGui.GetStyle().ItemSpacing.Y)) : maxHeight;
                using var list = ImRaii.Child("##statuses", new(0, height));
                if (list)
                {
                    if (current != null)
                    {
                        ImGui.TextDisabled(onSelf ? "On you right now:" : "On your target right now:");
                        foreach (var id in current)
                            modified |= DrawStatusEntry(id, StatusName(id), StatusIcon(id), ref status, iconSize);
                        if (current.Count == 0)
                            ImGui.TextDisabled("(nothing)");
                    }
                    else
                    {
                        foreach (var s in AllStatuses())
                        {
                            if (!s.Name.Contains(_statusSearch, StringComparison.OrdinalIgnoreCase) && !s.ID.ToString().StartsWith(_statusSearch, StringComparison.Ordinal))
                                continue;
                            modified |= DrawStatusEntry(s.ID, s.Name, s.Icon, ref status, iconSize);
                        }
                    }
                }
            }
        }
        return modified;
    }

    internal static string ActionName(uint id) => id == 0 ? "<none>" : CustomComboData.ActionName(DisplayAction(id));
    internal static string StatusName(uint id) => id == 0 ? "<none>" : Service.LuminaRow<Lumina.Excel.Sheets.Status>(id)?.Name.ToString() ?? $"#{id}";
    private static uint StatusIcon(uint id) => Service.LuminaRow<Lumina.Excel.Sheets.Status>(id)?.Icon ?? 0;

    private static bool DrawNewFromChain(CustomComboConfig config, Actor? player)
    {
        var job = player?.Class ?? Class.None;
        var chains = CustomComboData.ChainsForClass(job);
        var modified = false;
        ImGui.SetNextItemWidth(250);
        using (ImRaii.Disabled(chains.Count == 0))
        {
            using var combo = ImRaii.Combo("##fromchain", "New from native combo...", ImGuiComboFlags.HeightLargest);
            if (combo)
            {
                var iconSize = new Vector2(ImGui.GetFrameHeight());
                for (var i = 0; i < chains.Count; ++i)
                {
                    var chain = chains[i];
                    var pos = ImGui.GetCursorPos();
                    if (ImGui.Selectable($"###chain{i}", false, ImGuiSelectableFlags.None, new(0, iconSize.Y)))
                    {
                        config.Combos.Add(CustomComboData.BuildComboFromChain(chain, job));
                        modified = true;
                    }
                    ImGui.SetCursorPos(pos);
                    for (var k = 0; k < chain.Actions.Count; ++k)
                    {
                        if (k > 0)
                        {
                            ImGui.SameLine();
                            ImGui.TextDisabled(">");
                            ImGui.SameLine();
                        }
                        UIMisc.Image(Service.Texture?.GetFromGameIcon(CustomComboData.ActionIcon(chain.Actions[k])), iconSize);
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip(CustomComboData.ActionName(chain.Actions[k]));
                    }
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(pos.Y + 0.5f * (iconSize.Y - ImGui.GetTextLineHeight()));
                    ImGui.TextUnformatted(string.Join(" > ", chain.Actions.Select(CustomComboData.ActionName)));
                }
            }
        }
        return modified;
    }
}
