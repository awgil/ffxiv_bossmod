using ImGuiNET;
using System.Text;

namespace BossMod;

sealed class DebugClassDefinitions : IDisposable
{
    private class StatusData
    {
        public HashSet<Class> Classes = [];
        public HashSet<ActionID> Actions = [];
        public bool OnSource;
        public bool OnTarget;

        public string AppliedByString() => string.Join(", ", Actions.Select(aid => aid.Name()));
        public string AppliedToString() => OnSource ? (OnTarget ? "self/target" : "self") : "target";
    }

    private class ClassData
    {
        public Type? AIDType;
        public Type? CDGType;
        public Type? SIDType;
        public Type? TraitIDType;
        public List<Lumina.Excel.GeneratedSheets.Action> Actions = [];
        public List<Lumina.Excel.GeneratedSheets.Trait> Traits = [];
        public List<uint>? Unlocks;
        public SortedDictionary<int, List<Lumina.Excel.GeneratedSheets.Action>> CooldownGroups = [];

        public ClassData(Class c)
        {
            AIDType = Type.GetType($"BossMod.{c}.AID");
            CDGType = Type.GetType($"BossMod.{c}.CDGroup");
            SIDType = Type.GetType($"BossMod.{c}.SID");
            TraitIDType = Type.GetType($"BossMod.{c}.TraitID");

            var cp = typeof(Lumina.Excel.GeneratedSheets.ClassJobCategory).GetProperty(c.ToString());
            bool actionIsInteresting(Lumina.Excel.GeneratedSheets.Action a) => !a.IsPvP && a.ClassJobLevel > 0 && (cp?.GetValue(a.ClassJobCategory.Value) as bool? ?? false);
            Actions = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.Where(actionIsInteresting).ToList() ?? [];
            Actions.SortBy(e => e.ClassJobLevel);

            bool traitIsInteresting(Lumina.Excel.GeneratedSheets.Trait t) => cp?.GetValue(t.ClassJobCategory.Value) as bool? ?? false;
            Traits = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Trait>()?.Where(traitIsInteresting).ToList() ?? [];
            Traits.SortBy(e => e.Level);

            foreach (var action in Actions)
            {
                var cg = action.CooldownGroup - 1;
                if (cg is >= 0 and < 80)
                    CooldownGroups.GetOrAdd(cg).Add(action);
            }

            List<(uint, bool)>? quests = [];
            foreach (var action in Actions.Where(a => a.UnlockLink != 0))
            {
                quests = BuildOrderedQuests(quests, action.UnlockLink);
            }
            foreach (var trait in Traits.Where(t => t.Quest.Row != 0))
            {
                quests = BuildOrderedQuests(quests, trait.Quest.Row);
            }
            Unlocks = quests?.Where(q => q.Item2).Select(q => q.Item1).ToList();
        }

        private List<(uint, bool)>? BuildOrderedQuests(List<(uint, bool)>? quests, uint questID)
        {
            if (quests == null)
                return null;

            var index = quests.FindIndex(q => q.Item1 == questID);
            if (index < 0)
            {
                var q = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Quest>(questID);
                if (q != null && AddToDepChain(quests, q))
                    index = quests.Count - 1;
            }

            if (index < 0)
                return null;

            quests[index] = (questID, true);
            return quests;
        }

        private bool AddToDepChain(List<(uint, bool)> list, Lumina.Excel.GeneratedSheets.Quest item)
        {
            var prereq = item.PreviousQuest[0].Value;
            if (prereq == null)
            {
                if (list.Count == 0)
                {
                    list.Add((item.RowId, false));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (list.Count == 0 || list[^1].Item1 == prereq.RowId || AddToDepChain(list, prereq))
            {
                list.Add((item.RowId, false));
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private readonly WorldState _ws;
    private readonly EventSubscription _onCast;
    private readonly ClassData?[] _classes = new ClassData?[41];
    private readonly Dictionary<ActionID, float> _seenActionLocks = [];
    private readonly Dictionary<uint, StatusData> _seenStatuses = [];
    private readonly UITree _tree = new();

    public DebugClassDefinitions(WorldState ws)
    {
        _ws = ws;
        _onCast = ws.Actors.CastEvent.Subscribe(OnCast);
    }

    public void Dispose() => _onCast.Dispose();

    public unsafe void Draw()
    {
        var player = _ws.Party.Player();
        if (player == null)
            return;

        for (int i = 1; i < _classes.Length; ++i)
        {
            var c = (Class)i;
            foreach (var cn in _tree.Node(c.ToString(), false, c == player.Class ? 0xff00ff00 : 0xffffffff))
            {
                var data = _classes[i] ??= new(c);
                foreach (var n in _tree.Node("Actions", contextMenu: () => ActionsContextMenu(data)))
                {
                    foreach (var action in _tree.Nodes(data.Actions, a => ActionNode(data, a)))
                    {
                        _tree.LeafNode($"Unlock: {UnlockString(action.ClassJobLevel, action.UnlockLink)}");
                        _tree.LeafNode($"Cast time: {CastTimeString(action)}");
                        _tree.LeafNode($"Cooldown: {CooldownString(action)}");
                        _tree.LeafNode($"Range: {action.Range} ({FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(action.RowId)})");
                        _tree.LeafNode($"Type: {CastTypeString(action.CastType)} {action.EffectRange}/{action.XAxisModifier}");
                        _tree.LeafNode($"Omen: {action.Omen.Value?.Path}/{action.Omen.Value?.PathAlly}");
                        _tree.LeafNode($"Targets: {TargetsString(action)}");
                        _tree.LeafNode($"Animation lock: {AnimLockString(new ActionID(ActionType.Spell, action.RowId))}");
                    }
                }

                foreach (var n in _tree.Node("Traits", contextMenu: () => TraitsContextMenu(data)))
                {
                    _tree.LeafNodes(data.Traits, t => $"{t.RowId} '{t.Name}': {UnlockString(t.Level, t.Quest.Row)}");
                }

                foreach (var n in _tree.Node("Cooldown groups", contextMenu: () => CDGroupsContextMenu(data)))
                {
                    foreach (var (cg, actions) in data.CooldownGroups)
                    {
                        var cdgName = data.CDGType?.GetEnumName(cg);
                        _tree.LeafNode($"{cg} ({cdgName}): {string.Join(", ", actions.Select(a => a.Name))}", cdgName != null || cg == CommonDefinitions.GCDGroup ? 0xffffffff : 0xff0000ff);
                    }
                }

                foreach (var n in _tree.Node("Seen statuses", contextMenu: () => StatusesContextMenu(data)))
                {
                    foreach (var sn in _tree.Nodes(_seenStatuses.Where(kv => kv.Value.Classes.Contains(c)), idData => new(Utils.StatusString(idData.Key), false, data.SIDType?.GetEnumName(idData.Key) != null ? 0xffffffff : 0xff0000ff)))
                    {
                        _tree.LeafNode($"Applied by: {sn.Value.AppliedByString()}");
                        _tree.LeafNode($"Applied to: {sn.Value.AppliedToString()}");
                    }
                }
            }
        }
    }

    private void ActionsContextMenu(ClassData cd)
    {
        if (ImGui.MenuItem("Generate AID enum"))
        {
            var sb = new StringBuilder("public enum AID : uint\n{\n    None = 0,\n\n    // GCDs");
            foreach (var action in cd.Actions.Where(a => a.CooldownGroup - 1 == CommonDefinitions.GCDGroup))
                sb.Append($"\n    {ActionEnumString(cd, action)}");
            sb.Append("\n\n    // oGCDs");
            foreach (var action in cd.Actions.Where(a => a.CooldownGroup - 1 != CommonDefinitions.GCDGroup))
                sb.Append($"\n    {ActionEnumString(cd, action)}");
            sb.Append("\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Build Unlocked() function"))
        {
            var sb = new StringBuilder("public static bool Unlocked(AID aid, int level, int questProgress)\n{\n    return aid switch\n    {");
            foreach (var action in cd.Actions.Where(a => a.ClassJobLevel > 1))
            {
                var aidEnum = cd.AIDType?.GetEnumName(action.RowId) ?? Utils.StringToIdentifier(action.Name);
                sb.Append($"        AID.{aidEnum} => level >= {action.ClassJobLevel}");
                if (action.UnlockLink != 0)
                    sb.Append($" && questProgress > {cd.Unlocks?.IndexOf(action.UnlockLink).ToString() ?? "???"}");
                sb.Append(",\n");
            }
            sb.Append("        _ => true,\n    };\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Build definitions"))
        {
            var sb = new StringBuilder();
            foreach (var action in cd.Actions)
            {
                var aidEnum = cd.AIDType?.GetEnumName(action.RowId) ?? Utils.StringToIdentifier(action.Name);
                float defaultAnimLock = action.Cast100ms == 0 ? 0.6f : 0.1f;
                float animLock = _seenActionLocks.GetValueOrDefault(new ActionID(ActionType.Spell, action.RowId), defaultAnimLock);
                var animLockStr = animLock == defaultAnimLock ? "" : $", {animLock:f3}f";
                var cg = action.CooldownGroup - 1;
                if (cg == CommonDefinitions.GCDGroup)
                {
                    if (action.Cast100ms == 0)
                        sb.Append($"res.GCD(AID.{aidEnum}, {action.Range}{animLockStr});\n");
                    else
                        sb.Append($"res.GCDCast(AID.{aidEnum}, {action.Range}, {action.Cast100ms * 0.1f:f1}f{animLockStr});\n");
                }
                else
                {
                    var cdgName = cd.CDGType?.GetEnumName(cg) ?? Utils.StringToIdentifier(action.Name);
                    var firstArgsStr = $"AID.{aidEnum}, {action.Range}, CDGroup.{cdgName}, {action.Recast100ms * 0.1f:f1}f";
                    var charges = MaxChargesAtCap(action.RowId);
                    if (action.Cast100ms != 0)
                        sb.Append($"res.OGCDCast({firstArgsStr}{animLockStr});\n"); // don't think such exist?..
                    else if (charges <= 1)
                        sb.Append($"res.OGCD({firstArgsStr}{animLockStr});\n");
                    else
                        sb.Append($"res.OGCDWithCharges({firstArgsStr}, {charges}{animLockStr});\n");
                }
            }
            ImGui.SetClipboardText(sb.ToString());
        }

        if (cd.Unlocks != null && ImGui.MenuItem("Generate quest lock definitions"))
        {
            ImGui.SetClipboardText($"public static readonly uint[] UnlockQuests = [{string.Join(", ", cd.Unlocks)}];\n");
        }
    }

    private void TraitsContextMenu(ClassData cd)
    {
        if (ImGui.MenuItem("Generate TraitID enum"))
        {
            var sb = new StringBuilder("public enum TraitID : uint\n{\n    None = 0,");
            foreach (var trait in cd.Traits)
            {
                var tidEnum = cd.TraitIDType?.GetEnumName(trait.RowId) ?? Utils.StringToIdentifier(trait.Name);
                sb.Append($"\n    {tidEnum} = {trait.RowId}, // L{trait.Level}");
            }
            sb.Append("\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Build Unlocked() function"))
        {
            var sb = new StringBuilder("public static bool Unlocked(TraitID tid, int level, int questProgress)\n{\n    return tid switch\n    {");
            foreach (var trait in cd.Traits.Where(t => t.Level > 1))
            {
                var tidEnum = cd.TraitIDType?.GetEnumName(trait.RowId) ?? Utils.StringToIdentifier(trait.Name);
                sb.Append($"        TraitID.{tidEnum} => level >= {trait.Level}");
                if (trait.Quest.Row != 0)
                    sb.Append($" && questProgress > {cd.Unlocks?.IndexOf(trait.Quest.Row).ToString() ?? "???"}");
                sb.Append(",\n");
            }
            sb.Append("        _ => true,\n    };\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private void CDGroupsContextMenu(ClassData cd)
    {
        if (ImGui.MenuItem("Generate CDGroup enum"))
        {
            GenerateCDGroup(cd, false);
        }
        if (ImGui.MenuItem("Force regenerate CDGroup enum"))
        {
            GenerateCDGroup(cd, true);
        }
    }

    private void GenerateCDGroup(ClassData cd, bool forceRegen)
    {
        var sb = new StringBuilder("public enum CDGroup : int\n{");
        foreach (var (cg, actions) in cd.CooldownGroups)
        {
            if (cg == CommonDefinitions.GCDGroup)
                continue;

            ushort? commonRecast = actions[0].Recast100ms;
            int? commonMaxCharges = MaxChargesAtCap(actions[0].RowId);
            foreach (var action in actions.Skip(1))
            {
                if (commonRecast != action.Recast100ms)
                    commonRecast = null;
                if (commonMaxCharges != MaxChargesAtCap(action.RowId))
                    commonMaxCharges = null;
            }

            var cdgName = (forceRegen ? null : cd.CDGType?.GetEnumName(cg)) ?? Utils.StringToIdentifier(actions[0].Name);
            sb.Append($"\n    {cdgName} = {cg}, // ");

            if (commonRecast == null || commonMaxCharges == null)
                sb.Append("variable max");
            else if (commonMaxCharges.Value > 1)
                sb.Append($"{commonMaxCharges.Value}*{commonRecast.Value * 0.1f:f1} max");
            else
                sb.Append($"{commonRecast.Value * 0.1f:f1} max");

            if (actions.Count > 1)
                sb.Append($", shared by {string.Join(", ", actions.Select(a => a.Name))}");
        }
        sb.Append("\n}\n");
        ImGui.SetClipboardText(sb.ToString());
    }

    private void StatusesContextMenu(ClassData cd)
    {
        if (ImGui.MenuItem("Generate SID enum"))
        {
            var sb = new StringBuilder("public enum SID : uint\n{\n    None = 0,");
            foreach (var (id, data) in _seenStatuses)
            {
                var name = cd.SIDType?.GetEnumName(id) ?? Utils.StringToIdentifier(Service.LuminaRow<Lumina.Excel.GeneratedSheets.Status>(id)?.Name ?? $"Status{id}");
                sb.Append($"\n    {name} = {id}, // applied by {data.AppliedByString()} to {data.AppliedToString()}");
            }
            sb.Append("\n}\n");
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private UITree.NodeProperties ActionNode(ClassData cd, Lumina.Excel.GeneratedSheets.Action action)
    {
        var aidEnum = cd.AIDType?.GetEnumName(action.RowId);
        var name = $"{action.RowId} '{action.Name}' ({aidEnum}): L{action.ClassJobLevel}";
        return new(name, false, aidEnum == null ? 0xff0000ff : _seenActionLocks.ContainsKey(new ActionID(ActionType.Spell, action.RowId)) ? 0xffffffff : 0xff00ffff);
    }

    private string ActionEnumString(ClassData cd, Lumina.Excel.GeneratedSheets.Action action)
    {
        var aidEnum = cd.AIDType?.GetEnumName(action.RowId) ?? Utils.StringToIdentifier(action.Name);
        var sb = new StringBuilder($"{aidEnum} = {action.RowId}, // L{action.ClassJobLevel}, {CastTimeString(action)}");
        if (action.CooldownGroup - 1 != CommonDefinitions.GCDGroup)
            sb.Append($", {CooldownString(action)}");
        sb.Append($", range {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(action.RowId)}, {CastTypeString(action.CastType)} {action.EffectRange}/{action.XAxisModifier}, targets={TargetsString(action)}, animLock={AnimLockString(new ActionID(ActionType.Spell, action.RowId))}");
        return sb.ToString();
    }

    private string UnlockString(int level, uint link)
    {
        var res = $"L{level}";
        if (link != 0)
            res += $" ({UnlockLinkString(link)})";
        return res;
    }

    private string UnlockLinkString(uint link)
    {
        return $"unlocked by quest {link} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.Quest>(link)?.Name}'";
    }

    private string CastTimeString(Lumina.Excel.GeneratedSheets.Action action)
    {
        return action.Cast100ms != 0 ? $"{action.Cast100ms * 0.1f:f1}s cast" : "instant";
    }

    private string CooldownString(Lumina.Excel.GeneratedSheets.Action action)
    {
        var cg = action.CooldownGroup - 1;
        var res = cg == CommonDefinitions.GCDGroup ? "GCD" : $"{action.Recast100ms * 0.1f:f1}s CD (group {cg})";
        var charges = MaxChargesAtCap(action.RowId);
        if (charges > 1)
            res += $" ({charges} charges)";
        return res;
    }

    private string TargetsString(Lumina.Excel.GeneratedSheets.Action action)
    {
        var res = new List<string>();
        if (action.CanTargetSelf)
            res.Add("self");
        if (action.CanTargetParty)
            res.Add("party");
        if (action.CanTargetFriendly)
            res.Add("friendly");
        if (action.CanTargetHostile)
            res.Add("hostile");
        if (action.TargetArea)
            res.Add("area");
        if (!action.CanTargetDead)
            res.Add("!dead");
        return res.Count > 0 ? string.Join('/', res) : "n/a";
    }

    private string AnimLockString(ActionID id)
    {
        return _seenActionLocks.TryGetValue(id, out var animLock) ? $"{animLock:f3}s" : "???";
    }

    private string CastTypeString(int castType)
    {
        return castType switch
        {
            1 => "single-target",
            2 => "AOE circle",
            3 => "AOE cone",
            4 => "AOE rect",
            5 => "Enemy PBAoE circle?",
            6 => "??? 6 cone??",
            7 => "Ground circle",
            8 => "Charge rect",
            10 => "Enemy AOE donut",
            11 => "Enemy AOE cross???",
            12 => "Enemy AOE rect",
            13 => "Enemy AOE cone",
            _ => castType.ToString()
        };
    }

    private unsafe int MaxChargesAtCap(uint aid) => FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(aid, 90);

    private void OnCast(Actor actor, ActorCastEvent ev)
    {
        if (actor != _ws.Party.Player())
            return;
        _seenActionLocks[ev.Action] = ev.AnimationLockTime;
        foreach (var t in ev.Targets)
        {
            foreach (var eff in t.Effects.Where(eff => eff.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource))
            {
                var data = _seenStatuses.GetOrAdd(eff.Value);
                data.Classes.Add(actor.Class);
                data.Actions.Add(ev.Action);

                bool onTarget = eff.Type == ActionEffectType.ApplyStatusEffectTarget && t.ID != actor.InstanceID && !eff.AtSource;
                if (onTarget)
                    data.OnTarget = true;
                else
                    data.OnSource = true;
            }
        }
    }
}
