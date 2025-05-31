using BossMod.Data;
using ImGuiNET;
using System.Text;

namespace BossMod.ReplayAnalysis;

class ClassDefinitions
{
    private const string GroupRoleActions = "Multi-role actions";
    private const string GroupLimitBreaks = "Shared limit breaks";

    private readonly record struct Entry(Replay Replay, Replay.Action Action);

    private record class ActionData(ActionID ID, ActionDefinition? Definition, Lumina.Excel.Sheets.Action? Row)
    {
        public bool CanBePutOnActionBar => Row?.IsPlayerAction ?? false;
        public bool IsRoleAction => Row?.IsRoleAction ?? false;
        public bool IsGCD => Row?.CooldownGroup == ActionDefinitions.GCDGroup + 1 || Row?.AdditionalCooldownGroup == ActionDefinitions.GCDGroup + 1;
        public int MainCDGroup => (Row?.CooldownGroup ?? 0) - 1;
        public int ExtraCDGroup => (Row?.AdditionalCooldownGroup ?? 0) - 1;
        public int ExpectedInstantAnimLock => (int)MathF.Round((Definition?.InstantAnimLock ?? 0) * 1000);
        public int ExpectedCastAnimLock => (int)MathF.Round((Definition?.CastAnimLock ?? 0) * 1000);
        public bool SeenInstant => InstantByAnimLock.Count != 0;
        public bool SeenCast => CastByAnimLock.Count != 0;
        public bool IsPhysRanged => OwningClasses.Any() && ((Class)OwningClasses.HighestSetBit()).GetClassCategory() == ClassCategory.PhysRanged;

        public BitMask OwningClasses;
        public ClassCategory SharedCategory;
        public int LimitBreakLevel;
        public bool PotentiallyRemoved;
        public bool ReplayOnly;
        public bool IsBozjaHolster;
        public bool IsPhantomAction;
        public bool SeenDifferentInstantAnimLocks;
        public bool SeenDifferentCastAnimLocks;
        public Dictionary<int, List<Entry>> InstantByAnimLock = [];
        public Dictionary<int, List<Entry>> CastByAnimLock = [];
        public HashSet<uint> AppliedStatusesToSource = [];
        public HashSet<uint> AppliedStatusesToTarget = [];

        public bool Warning => !SeenInstant && !SeenCast;
        public bool Error => Definition == null || Row == null || SeenDifferentInstantAnimLocks || SeenDifferentCastAnimLocks || PotentiallyRemoved || ReplayOnly;
    }

    private class StatusData
    {
        public HashSet<ActionID> Actions = [];
        public bool OnSource;
        public bool OnTarget;

        public string AppliedByString() => string.Join(", ", Actions.Select(aid => aid.Name()));
        public string AppliedToString() => OnSource ? (OnTarget ? "self/target" : "self") : "target";
    }

    private record class ClassData(Class ID, Class Base)
    {
        public readonly List<ActionData> Actions = [];
        public readonly SortedDictionary<int, List<ActionData>> ByCDGroup = [];
        public readonly List<Lumina.Excel.Sheets.Trait> Traits = [];
        public readonly Dictionary<uint, StatusData> Statuses = [];
    }

    private readonly Dictionary<ActionID, ActionData> _actionData = [];
    private readonly Dictionary<Class, ClassData> _classData = [];
    private readonly SortedDictionary<string, List<ActionData>> _byCategory = [];
    private readonly Dictionary<int, Dictionary<ActionID, List<Entry>>> _byLock = [];
    private readonly SortedDictionary<int, List<ActionData>> _byCDGroup = [];
    private readonly BitMask[] _classPerCategory = new BitMask[(int)ClassCategory.Limited + 1];

    public ClassDefinitions(List<Replay> replays)
    {
        var actionSheet = Service.LuminaSheet<Lumina.Excel.Sheets.Action>()!;
        var classSheet = Service.LuminaSheet<Lumina.Excel.Sheets.ClassJob>()!;
        var cjcSheet = Service.LuminaGameData!.Excel.GetSheet<Lumina.Excel.RawRow>(null, "ClassJobCategory")!;
        var traitSheet = Service.LuminaSheet<Lumina.Excel.Sheets.Trait>()!;

        // we don't care about base classes and DoH/DoL here...
        for (var i = (uint)Class.PLD; i < classSheet.Count; ++i)
        {
            var row = classSheet.GetRow(i)!;
            var curClass = (Class)i;
            if (curClass is Class.ACN or Class.ROG)
                continue;

            _classPerCategory[(int)curClass.GetClassCategory()].Set((int)i);
            var baseClass = curClass == Class.SCH ? Class.SCH : (Class)row.ClassJobParent.RowId; // both SCH and SMN are based on ACN, but SMN is closer
            var classData = _classData[curClass] = new(curClass, baseClass);

            bool actionIsInteresting(Lumina.Excel.Sheets.Action a) => !a.IsPvP && ((int)a.ClassJob.RowId != -1 || a.IsRoleAction) && a.ClassJobLevel > 0 && cjcSheet.GetRow(a.ClassJobCategory.RowId).ReadBoolColumn((int)i + 1);
            foreach (var a in actionSheet.Where(actionIsInteresting))
                RegisterAction(new ActionID(ActionType.Spell, a.RowId), a, classData, out _);
            RegisterLimitBreak(row.LimitBreak1.RowId, classData, 1);
            RegisterLimitBreak(row.LimitBreak2.RowId, classData, 2);
            RegisterLimitBreak(row.LimitBreak3.RowId, classData, 3);

            bool traitIsInteresting(Lumina.Excel.Sheets.Trait t) => cjcSheet.GetRow(t.ClassJobCategory.RowId).ReadBoolColumn((int)i + 1);
            classData.Traits.AddRange(traitSheet.Where(traitIsInteresting));
            classData.Traits.SortBy(e => e.Level);
        }
        var nullOwner = _classData[Class.None] = new(Class.None, Class.None);

        // add any actions from definitions that weren't found yet
        foreach (var def in ActionDefinitions.Instance.Definitions)
            if (RegisterAction(def.ID, def.ID.Type == ActionType.Spell ? actionSheet?.GetRow(def.ID.ID) : null, nullOwner, out var data))
                data.PotentiallyRemoved = true;

        // add any actions observed in replays
        foreach (var r in replays)
        {
            foreach (var a in r.Actions.Where(a => a.SourceSequence != 0))
            {
                var alock = (int)MathF.Round(a.AnimationLock * 1000);
                _byLock.GetOrAdd(alock).GetOrAdd(a.ID).Add(new(r, a));

                if (RegisterAction(a.ID, a.ID.Type == ActionType.Spell ? actionSheet?.GetRow(a.ID.ID) : null, nullOwner, out var data))
                    data.ReplayOnly = true;

                var cast = a.Source.Casts.Find(c => c.ID == a.ID && c.Time.Contains(a.Timestamp));
                if (cast != null)
                {
                    data.SeenDifferentCastAnimLocks |= data.ExpectedCastAnimLock != alock;
                    data.CastByAnimLock.GetOrAdd(alock).Add(new(r, a));
                }
                else
                {
                    data.SeenDifferentInstantAnimLocks |= data.ExpectedInstantAnimLock != alock;
                    data.InstantByAnimLock.GetOrAdd(alock).Add(new(r, a));
                }

                foreach (var target in a.Targets)
                {
                    foreach (var eff in target.Effects.Where(eff => eff.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource or ActionEffectType.FullResistStatus && !eff.FromTarget))
                    {
                        var onTarget = eff.Type == ActionEffectType.ApplyStatusEffectTarget && target.Target != a.Source && !eff.AtSource;
                        (onTarget ? data.AppliedStatusesToTarget : data.AppliedStatusesToSource).Add(eff.Value);
                    }
                }
            }
        }

        // mark bozja holster actions
        for (var i = BozjaHolsterID.None + 1; i < BozjaHolsterID.Count; ++i)
            _actionData[BozjaActionID.GetNormal(i)].IsBozjaHolster = true;

        foreach (var id in typeof(PhantomID).GetEnumValues())
            if ((uint)id > 0)
                _actionData[ActionID.MakeSpell((PhantomID)id)].IsPhantomAction = true;

        // split actions by categories
        foreach (var (aid, data) in _actionData)
        {
            data.OwningClasses.Clear(0); // that's pointless
            var shared = data.OwningClasses.NumSetBits() > 1 ? Array.IndexOf(_classPerCategory, data.OwningClasses) : -1;
            if (shared >= 0)
                data.SharedCategory = (ClassCategory)shared;

            var category = aid.Type switch
            {
                ActionType.Spell => data.SharedCategory > ClassCategory.Undefined ? $"Category: {data.SharedCategory}"
                    : data.IsRoleAction ? GroupRoleActions
                    : data.LimitBreakLevel > 0 && data.OwningClasses.NumSetBits() > 1 ? GroupLimitBreaks
                    : data.IsBozjaHolster ? "Bozja action"
                    : data.IsPhantomAction ? "Phantom action"
                    : data.OwningClasses.Any() ? $"Class: {string.Join(" ", data.OwningClasses.SetBits().Select(i => (Class)i))}"
                    : "???",
                ActionType.Item => "Item",
                ActionType.General => "General actions",
                ActionType.Mount => "Mount",
                ActionType.Ornament => "Ornament",
                ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1 => "Bozja holster",
                _ => data.Row?.ClassJobCategory.ValueNullable?.Name.ToString() ?? "???"
            };
            _byCategory.GetOrAdd(category).Add(data);
            AddActionsToCDGroups(data, data.MainCDGroup);
            AddActionsToCDGroups(data, data.ExtraCDGroup);
        }
        foreach (var (_, list) in _byCategory)
            list.SortBy(d => d.Row?.ClassJobLevel ?? 0);
        foreach (var (_, cd) in _classData)
        {
            cd.Actions.SortBy(d => d.Row?.ClassJobLevel ?? 0);
            foreach (var a in cd.Actions)
            {
                foreach (var s in a.AppliedStatusesToSource)
                {
                    var sd = cd.Statuses.GetOrAdd(s);
                    sd.Actions.Add(a.ID);
                    sd.OnSource = true;
                }
                foreach (var s in a.AppliedStatusesToTarget)
                {
                    var sd = cd.Statuses.GetOrAdd(s);
                    sd.Actions.Add(a.ID);
                    sd.OnTarget = true;
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        tree.LeafNode("Shared data generator", contextMenu: CtxMenuShared);
        foreach (var nr in tree.Node("Classes"))
        {
            foreach (var nc in tree.Nodes(_classData, kv => new(kv.Key.ToString()), kv => CtxMenuClass(kv.Value)))
            {
                foreach (var na in tree.Node("All actions"))
                {
                    DrawActions(tree, nc.Value.Actions);
                }
                foreach (var ng in tree.Node("Actions by CD group"))
                {
                    foreach (var ncd in tree.Nodes(nc.Value.ByCDGroup, kv => new($"{kv.Key} ({kv.Value.Count} actions)")))
                    {
                        DrawActions(tree, ncd.Value);
                    }
                }
                foreach (var nt in tree.Node("Traits"))
                {
                    tree.LeafNodes(nc.Value.Traits, t => $"{t.RowId} '{t.Name}': {UnlockString(t.Level, t.Quest.RowId)}");
                }
            }
        }
        foreach (var nr in tree.Node("Actions by anim lock"))
        {
            foreach (var nl in tree.Nodes(_byLock, kv => new(kv.Key.ToString())))
            {
                foreach (var na in tree.Nodes(nl.Value, kv => new(kv.Key.ToString())))
                {
                    DrawEntries(tree, na.Value);
                }
            }
        }
        foreach (var nr in tree.Node("Actions by category"))
        {
            foreach (var nc in tree.Nodes(_byCategory, kv => new(kv.Key)))
            {
                DrawActions(tree, nc.Value);
            }
        }
        foreach (var nr in tree.Node("Actions by CD group"))
        {
            foreach (var nc in tree.Nodes(_byCDGroup, kv => new($"{kv.Key} ({kv.Value.Count} actions)")))
            {
                DrawActions(tree, nc.Value);
            }
        }
    }

    private bool RegisterAction(ActionID action, Lumina.Excel.Sheets.Action? row, ClassData owner, out ActionData actionData)
    {
        var newlyCreated = false;
        if (!_actionData.TryGetValue(action, out var data))
        {
            newlyCreated = true;
            data = _actionData[action] = new(action, ActionDefinitions.Instance[action], row);
        }
        actionData = data;
        if (!data.OwningClasses[(int)owner.ID])
        {
            data.OwningClasses.Set((int)owner.ID);
            owner.Actions.Add(data);
        }
        return newlyCreated;
    }

    private void RegisterLimitBreak(uint id, ClassData owner, int level)
    {
        if (id != 0)
        {
            RegisterAction(new(ActionType.Spell, id), Service.LuminaRow<Lumina.Excel.Sheets.Action>(id), owner, out var data);
            data.LimitBreakLevel = level;
        }
    }

    private void AddActionsToCDGroups(ActionData action, int cdgroup)
    {
        if (cdgroup < 0)
            return;
        _byCDGroup.GetOrAdd(cdgroup).Add(action);
        foreach (var c in action.OwningClasses.SetBits())
            _classData[(Class)c].ByCDGroup.GetOrAdd(cdgroup).Add(action);
    }

    private void DrawActions(UITree tree, List<ActionData> actions)
    {
        static string suffix(BitMask m) => m.NumSetBits() switch
        {
            0 => " [no owning classes]",
            1 => "",
            _ => " [shared]"
        };
        foreach (var na in tree.Nodes(actions, kv => new($"L{kv.Row?.ClassJobLevel} {kv.ID}{suffix(kv.OwningClasses)}", false, kv.Error ? 0xff0000ff : kv.Warning ? 0xff00ffff : 0xffffffff)))
        {
            tree.LeafNode($"Definition: {na.Definition}");
            tree.LeafNode($"Row: {na.Row}, raw range: {na.Row?.Range}, class: {na.Row?.ClassJob.ValueNullable?.Abbreviation}, category: {na.Row?.ClassJobCategory.ValueNullable?.Name}");
            tree.LeafNode($"Unlock: {UnlockString(na.Row?.ClassJobLevel ?? 0, na.Row?.UnlockLink.RowId ?? 0)}");
            tree.LeafNode($"Warnings: {(na.PotentiallyRemoved ? "PR " : "")}{(na.ReplayOnly ? "RO " : "")}", na.PotentiallyRemoved || na.ReplayOnly ? 0xff0000ff : 0xffffffff);
            tree.LeafNode($"Targets: [{ActionDefinitions.Instance.ActionAllowedTargets(na.ID)}]");
            tree.LeafNode($"Can be put on action bar: {na.CanBePutOnActionBar}");
            tree.LeafNode($"Is role action: {na.IsRoleAction}");
            tree.LeafNode($"Expected anim lock: {na.ExpectedInstantAnimLock} / {na.ExpectedCastAnimLock}", na.SeenDifferentInstantAnimLocks || na.SeenDifferentCastAnimLocks ? 0xff0000ff : 0xffffffff);
            foreach (var nl in tree.Nodes(na.InstantByAnimLock, kv => new($"Observed instant anim lock: {kv.Key}")))
            {
                DrawEntries(tree, nl.Value);
            }
            foreach (var nl in tree.Nodes(na.CastByAnimLock, kv => new($"Observed cast anim lock: {kv.Key}")))
            {
                DrawEntries(tree, nl.Value);
            }
            foreach (var ns in tree.Node("Statuses applied to self", na.AppliedStatusesToSource.Count == 0))
            {
                tree.LeafNodes(na.AppliedStatusesToSource, Utils.StatusString);
            }
            foreach (var ns in tree.Node("Statuses applied to target", na.AppliedStatusesToTarget.Count == 0))
            {
                tree.LeafNodes(na.AppliedStatusesToTarget, Utils.StatusString);
            }
        }
    }

    private void DrawEntries(UITree tree, List<Entry> entries)
    {
        foreach (var n in tree.Nodes(entries, e => new($"{e.Replay.Path} @ {e.Action.Timestamp:O}: {ReplayUtils.ParticipantString(e.Action.Source, e.Action.Timestamp)} -> {ReplayUtils.ParticipantString(e.Action.MainTarget, e.Action.Timestamp)}", e.Action.Targets.Count == 0)))
        {
            foreach (var t in tree.Nodes(n.Action.Targets, t => new(ReplayUtils.ActionTargetString(t, n.Action.Timestamp))))
            {
                tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }
    }

    private void CtxMenuShared()
    {
        var ns = "ClassShared";

        if (ImGui.MenuItem("Generate AID enum"))
        {
            var writer = new AIDWriter(ns);
            writer.Add("None", 0);
            writer.Add("Sprint", ActionDefinitions.IDSprint.ID);
            for (var c = ClassCategory.Tank; c <= ClassCategory.Limited; ++c)
                writer.Group(c.ToString(), _byCategory.GetOrAdd($"Category: {c}"), false);
            writer.Group(GroupRoleActions, _byCategory.GetOrAdd(GroupRoleActions), true);
            writer.Group(GroupLimitBreaks, _byCategory.GetOrAdd(GroupLimitBreaks), true);
            // TODO: bozja...
            writer.Group("Misc", _actionData.Values.Where(a => !a.IsRoleAction && a.LimitBreakLevel == 0 && a.OwningClasses.NumSetBits() > 1), true);
            ImGui.SetClipboardText(writer.Result());
        }

        if (ImGui.MenuItem("Generate definitions constructor"))
        {
            var writer = new DefinitionWriter(ns);
            writer.Add(_actionData[ActionDefinitions.IDSprint]);
            for (var c = ClassCategory.Tank; c <= ClassCategory.Limited; ++c)
                writer.Group(c.ToString(), _byCategory.GetOrAdd($"Category: {c}"));
            writer.Group(GroupRoleActions, _byCategory.GetOrAdd(GroupRoleActions));
            writer.Group(GroupLimitBreaks, _byCategory.GetOrAdd(GroupLimitBreaks));
            // TODO: bozja...
            writer.Group("Misc", _actionData.Values.Where(a => !a.IsRoleAction && a.LimitBreakLevel == 0 && a.OwningClasses.NumSetBits() > 1));
            ImGui.SetClipboardText(writer.Result());
        }
    }

    private void CtxMenuClass(ClassData cd)
    {
        if (ImGui.MenuItem("Generate full definition"))
        {
            ImGui.SetClipboardText(GenerateClassDefinition(cd, false));
        }

        if (ImGui.MenuItem("Generate definition stub"))
        {
            ImGui.SetClipboardText(GenerateClassDefinition(cd, true));
        }

        if (ImGui.MenuItem("Generate AID enum"))
        {
            ImGui.SetClipboardText(GenerateClassAID(cd));
        }

        if (ImGui.MenuItem("Generate TraitID enum"))
        {
            ImGui.SetClipboardText(GenerateClassTraitID(cd));
        }

        if (ImGui.MenuItem("Generate SID enum"))
        {
            ImGui.SetClipboardText(GenerateClassSID(cd));
        }

        if (ImGui.MenuItem("Generate definitions constructor"))
        {
            ImGui.SetClipboardText(GenerateClassRegistration(cd));
        }
    }

    private string GenerateClassDefinition(ClassData cd, bool stub)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"namespace BossMod.{cd.ID.ToString()};");
        sb.AppendLine();
        sb.Append(stub ? "// *** paste AID enum here ***\n" : GenerateClassAID(cd));
        sb.AppendLine();
        sb.Append(stub ? "// *** paste TraitID enum here ***\n" : GenerateClassTraitID(cd));
        sb.AppendLine();
        sb.Append(stub ? "// *** paste SID enum here ***\n" : GenerateClassSID(cd));
        sb.AppendLine();
        sb.AppendLine("public sealed class Definitions : IDisposable");
        sb.AppendLine("{");
        sb.Append(stub ? "    // *** paste constructor here ***\n" : GenerateClassRegistration(cd));
        sb.AppendLine();
        sb.AppendLine("    public void Dispose() { }");
        sb.AppendLine();
        sb.AppendLine("    private void Customize(ActionDefinitions d)");
        sb.AppendLine("    {");
        sb.AppendLine("        // *** add any properties that can't be autogenerated here ***");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GenerateClassAID(ClassData cd)
    {
        var writer = new AIDWriter(cd.ID.ToString());
        writer.Add("None", 0);
        writer.Add("Sprint", "ClassShared.AID.Sprint");
        writer.Group("", cd.Actions.Where(a => a.OwningClasses.NumSetBits() == 1), false);
        writer.Group("Shared", cd.Actions.Where(a => a.OwningClasses.NumSetBits() > 1), false, true);
        return writer.Result();
    }

    private string GenerateClassTraitID(ClassData cd)
    {
        var writer = new EnumWriter("TraitID");
        writer.Add("None", 0);
        foreach (var trait in cd.Traits)
            writer.Add(TraitIDName(cd.ID.ToString(), trait), trait.RowId, $"L{trait.Level}");
        return writer.Result();
    }

    private string GenerateClassSID(ClassData cd)
    {
        var writer = new EnumWriter("SID");
        writer.Add("None", 0);
        foreach (var (sid, data) in cd.Statuses)
            writer.Add(StatusIDName(cd.ID.ToString(), sid), sid, $"applied by {data.AppliedByString()} to {data.AppliedToString()}");
        return writer.Result();
    }

    private string GenerateClassRegistration(ClassData cd)
    {
        var writer = new DefinitionWriter(cd.ID.ToString());
        foreach (var a in cd.Actions.Where(a => a.OwningClasses.NumSetBits() == 1))
            writer.Add(a);
        return writer.Result();
    }

    private record class EnumWriter(string Name)
    {
        protected readonly StringBuilder _sb = new($"public enum {Name} : uint\n{{\n");

        public void Add(string name, string value, string comment = "") => _sb.AppendLine($"    {name} = {value},{(comment.Length > 0 ? " // " : "")}{comment}");
        public void Add(string name, uint value, string comment = "") => Add(name, value.ToString(), comment);

        public string Result()
        {
            _sb.AppendLine("}");
            return _sb.ToString();
        }
    }

    private record class AIDWriter(string Namespace) : EnumWriter("AID")
    {
        public void Add(ActionData a, bool allowClasses, bool shared = false)
        {
            var name = ActionIDName(Namespace, a.ID);
            Add(name, shared ? $"ClassShared.AID.{name}" : a.ID.ID.ToString(), Comment(a, allowClasses));
        }

        public void Group(string group, IEnumerable<ActionData> actions, bool allowClasses, bool shared = false)
        {
            bool writtenHeader = false;
            foreach (var a in actions)
            {
                if (!writtenHeader)
                {
                    _sb.AppendLine();
                    if (group.Length > 0)
                        _sb.AppendLine($"    // {group}");
                    writtenHeader = true;
                }
                Add(a, allowClasses, shared);
            }
        }

        public string Comment(ActionData action, bool allowClasses)
            => $"{LevelString(action, allowClasses)}, {CastTimeString(action)}{CooldownString(action)}{ChargesString(action)}, range {ActionDefinitions.Instance.ActionRange(action.ID, action.IsPhysRanged)}, {(action.Row != null ? DescribeShape(action.Row.Value) : "????")}, targets={ActionDefinitions.Instance.ActionAllowedTargets(action.ID).ToString().Replace(", ", "/", StringComparison.InvariantCulture)}{AnimLockString(action)}";

        private string LevelString(ActionData action, bool allowClasses)
        {
            var level = action.LimitBreakLevel > 0 ? $"LB{action.LimitBreakLevel}" : action.Row?.ClassJobLevel > 0 ? $"L{action.Row?.ClassJobLevel}" : "";
            var classes = !allowClasses || action.OwningClasses.NumSetBits() <= 1 ? "" : action.SharedCategory != ClassCategory.Undefined ? action.SharedCategory.ToString() : string.Join("/", action.OwningClasses.SetBits().Select(i => (Class)i));
            return level.Length > 0 ? (classes.Length > 0 ? $"{level} {classes}" : level) : classes;
        }

        private string CastTimeString(ActionData action)
        {
            var castTime = ActionDefinitions.Instance.ActionBaseCastTime(action.ID);
            return castTime != 0 ? $"{castTime:f1}s cast" : "instant";
        }

        private string CooldownString(ActionData action) => action.MainCDGroup switch
        {
            < 0 => "",
            ActionDefinitions.GCDGroup => ", GCD",
            _ => $", {ActionDefinitions.Instance.ActionBaseCooldown(action.ID):f1}s CD (group {action.MainCDGroup}{(action.ExtraCDGroup >= 0 ? $"/{action.ExtraCDGroup}" : "")})"
        };

        private string ChargesString(ActionData action)
        {
            // assume if definition is present, we know what we're doing...
            var min = ActionDefinitions.Instance.ActionBaseMaxCharges(action.ID);
            var max = action.Definition?.MaxChargesAtCap() ?? 0;
            return min == 1 && max <= 1 ? "" // simple
                : min == max ? $" ({min} charges)"
                : max == 0 ? $" ({min}? charges)"
                : min < max ? $" ({min}-{max} charges)"
                : $" (??? charges)";
        }

        private string DescribeShape(Lumina.Excel.Sheets.Action data) => data.CastType switch
        {
            1 => "single-target",
            2 => $"AOE {data.EffectRange} circle",
            3 => $"AOE {data.EffectRange}+R {DetermineConeAngle(data)?.ToString() ?? "?"}-degree cone",
            4 => $"AOE {data.EffectRange}+R width {data.XAxisModifier} rect",
            5 => $"AOE {data.EffectRange}+R circle",
            8 => $"AOE width {data.XAxisModifier} rect charge",
            10 => $"AOE {DetermineDonutInner(data)?.ToString() ?? "?"}-{data.EffectRange} donut",
            11 => $"AOE {data.EffectRange} width {data.XAxisModifier} cross",
            12 => $"AOE {data.EffectRange} width {data.XAxisModifier} rect",
            13 => $"AOE {data.EffectRange} {DetermineConeAngle(data)?.ToString() ?? "?"}-degree cone",
            _ => "???"
        };

        private Angle? DetermineConeAngle(Lumina.Excel.Sheets.Action data)
        {
            var omen = data.Omen.ValueNullable;
            if (omen == null)
                return null;

            var path = omen.Value.Path.ToString();
            var pos = path.IndexOf("fan", StringComparison.Ordinal);
            return pos >= 0 && pos + 6 <= path.Length && int.TryParse(path.AsSpan(pos + 3, 3), out var angle) ? angle.Degrees() : null;
        }

        private float? DetermineDonutInner(Lumina.Excel.Sheets.Action data)
        {
            var omen = data.Omen.ValueNullable;
            if (omen == null)
                return null;

            var path = omen.Value.Path.ToString();
            var pos = path.IndexOf("sircle_", StringComparison.Ordinal);
            if (pos >= 0 && pos + 11 <= path.Length && int.TryParse(path.AsSpan(pos + 9, 2), out var inner))
                return inner;

            pos = path.IndexOf("circle", StringComparison.Ordinal);
            if (pos >= 0 && pos + 10 <= path.Length && int.TryParse(path.AsSpan(pos + 8, 2), out inner))
                return inner;

            return null;
        }
    }

    private record class DefinitionWriter(string Namespace)
    {
        private readonly StringBuilder _sb = new("public Definitions(ActionDefinitions d)\n{\n");

        public void Add(List<string> args, string comment = "") => _sb.AppendLine($"    d.RegisterSpell({string.Join(", ", args)});{(comment.Length > 0 ? " // " : "")}{comment}");
        public void Add(ActionData a)
        {
            List<string> args = [$"AID.{ActionIDName(Namespace, a.ID)}"];
            if (a.IsPhysRanged)
                args.Add("true");
            var instAnimLock = AnimLock(a.InstantByAnimLock, a.ExpectedInstantAnimLock, 0.6f);
            if (instAnimLock != 0.6f)
                args.Add($"instantAnimLock: {instAnimLock:f}f");
            var castAnimLock = AnimLock(a.CastByAnimLock, a.ExpectedCastAnimLock, 0.1f);
            if (castAnimLock != 0.1f)
                args.Add($"castAnimLock: {castAnimLock:f}f");
            var animLockStr = AnimLockString(a);
            Add(args, animLockStr.Length > 0 ? animLockStr[2..] : "");
        }

        public void Group(string group, IEnumerable<ActionData> actions)
        {
            bool writtenHeader = false;
            foreach (var a in actions)
            {
                if (!writtenHeader)
                {
                    _sb.AppendLine();
                    if (group.Length > 0)
                        _sb.AppendLine($"        // {group}");
                    writtenHeader = true;
                }
                Add(a);
            }
        }

        public string Result()
        {
            _sb.AppendLine();
            _sb.AppendLine("    Customize(d);");
            _sb.AppendLine("}");
            return _sb.ToString();
        }

        private float AnimLock(Dictionary<int, List<Entry>> entries, int expected, float usual) => entries.Count switch
        {
            0 => expected != 0 ? expected * 0.001f : usual,
            1 => entries.First().Key * 0.001f,
            _ => usual,
        };
    }

    private record class UnlockWriter(string EnumName)
    {
        private readonly StringBuilder _sb = new($"public static bool Unlocked({EnumName} id, int level, int questProgress) => id switch\n{{\n");

        public void Add(string id, int level, int questIndex)
        {
            var questCond = questIndex >= 0 ? $" && questProgress > {questIndex}" : "";
            _sb.AppendLine($"    {EnumName}.{id} => level >= {level}{questCond},");
        }

        public string Result()
        {
            _sb.AppendLine("    _ => true");
            _sb.AppendLine("};");
            return _sb.ToString();
        }
    }

    private string UnlockString(int level, uint link)
    {
        var res = $"L{level}";
        if (link != 0)
            res += $" (unlocked by quest {link} '{Service.LuminaRow<Lumina.Excel.Sheets.Quest>(link)?.Name}')";
        return res;
    }

    private static string ActionIDName(string ns, ActionID aid) => Type.GetType($"BossMod.{ns}.AID")?.GetEnumName(aid.ID) ?? Utils.StringToIdentifier(aid.Name());
    private static string TraitIDName(string ns, Lumina.Excel.Sheets.Trait trait) => Type.GetType($"BossMod.{ns}.TraitID")?.GetEnumName(trait.RowId) ?? Utils.StringToIdentifier(trait.Name.ToString());
    private static string StatusIDName(string ns, uint sid) => Type.GetType($"BossMod.{ns}.SID")?.GetEnumName(sid) ?? Utils.StringToIdentifier(Service.LuminaRow<Lumina.Excel.Sheets.Status>(sid)?.Name.ToString() ?? $"Status{sid}");

    private static string AnimLockString(ActionData action)
    {
        string resInst = "";
        if (action.SeenInstant)
        {
            if (action.InstantByAnimLock.Count > 1 || action.InstantByAnimLock.First().Key != 600)
            {
                resInst = string.Join('/', action.InstantByAnimLock.Keys.Select(k => (k * 0.001f).ToString("f3")).Order());
            }
        }
        else if (action.Definition != null && action.Definition.InstantAnimLock != 0.6f)
        {
            resInst = $"{action.Definition.InstantAnimLock:f3}s?";
        }
        else if (action.Definition == null)
        {
            resInst = "???";
        }

        var resCast = "";
        if (action.SeenCast)
        {
            if (action.CastByAnimLock.Count > 1 || action.CastByAnimLock.First().Key != 100)
            {
                resCast = string.Join('/', action.CastByAnimLock.Keys.Select(k => (k * 0.001f).ToString("f3")).Order());
            }
        }
        else if (action.Definition != null && action.Definition.CastAnimLock != 0.1f)
        {
            resInst = $"{action.Definition.CastAnimLock:f3}s?";
        }

        return (resInst.Length > 0 ? $", animLock={resInst}" : "") + (resCast.Length == 0 ? "" : $", castAnimLock={resCast}");
    }
}
