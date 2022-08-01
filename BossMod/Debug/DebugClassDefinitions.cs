using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BossMod
{
    class DebugClassDefinitions : IDisposable
    {
        private class StatusData
        {
            public HashSet<Class> Classes = new();
            public HashSet<uint> Actions = new();
            public bool OnSource;
            public bool OnTarget;

            public string AppliedByString() => string.Join(", ", Actions.Select(aid => new ActionID(aid).Name()));
            public string AppliedToString() => OnSource ? (OnTarget ? "self/target" : "self") : "target";
        }

        private class ClassData
        {
            public Type? AIDType;
            public Type? CDGType;
            public Type? SIDType;
            public List<Lumina.Excel.GeneratedSheets.Action> Actions = new();
            public List<Lumina.Excel.GeneratedSheets.Trait> Traits = new();
            public SortedDictionary<int, List<Lumina.Excel.GeneratedSheets.Action>> CooldownGroups = new();

            public ClassData(Class c)
            {
                AIDType = Type.GetType($"BossMod.{c}.AID");
                CDGType = Type.GetType($"BossMod.{c}.CDGroup");
                SIDType = Type.GetType($"BossMod.{c}.SID");

                var cp = typeof(Lumina.Excel.GeneratedSheets.ClassJobCategory).GetProperty(c.ToString());
                Func<Lumina.Excel.GeneratedSheets.Action, bool> actionIsInteresting = a => !a.IsPvP && a.ClassJobLevel > 0 && (cp?.GetValue(a.ClassJobCategory.Value) as bool? ?? false);
                Actions = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.Where(actionIsInteresting).ToList() ?? new();
                Actions.Sort((l, r) => l.ClassJobLevel.CompareTo(r.ClassJobLevel));

                Func<Lumina.Excel.GeneratedSheets.Trait, bool> traitIsInteresting = t => (cp?.GetValue(t.ClassJobCategory.Value) as bool? ?? false);
                Traits = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Trait>()?.Where(traitIsInteresting).ToList() ?? new();
                Traits.Sort((l, r) => l.Level.CompareTo(r.Level));

                foreach (var action in Actions)
                {
                    var cg = action.CooldownGroup - 1;
                    if (cg is >= 0 and < 80)
                        CooldownGroups.GetOrAdd(cg).Add(action);
                }
            }
        }

        private WorldState _ws;
        private ClassData?[] _classes = new ClassData?[41];
        private Dictionary<uint, float> _seenActionLocks = new();
        private Dictionary<uint, StatusData> _seenStatuses = new();
        private UITree _tree = new();

        public DebugClassDefinitions(WorldState ws)
        {
            _ws = ws;
            _ws.Actors.CastEvent += OnCast;
        }

        public void Dispose()
        {
            _ws.Actors.CastEvent -= OnCast;
        }

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
                    var data = _classes[i];
                    if (data == null)
                        data = _classes[i] = new(c);

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

                    foreach (var n in _tree.Node("Traits"))
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

            if (ImGui.MenuItem("Build defitions"))
            {
                var sb = new StringBuilder();
                foreach (var action in cd.Actions)
                {
                    var aidEnum = cd.AIDType?.GetEnumName(action.RowId) ?? Utils.StringToIdentifier(action.Name);
                    float animLock = _seenActionLocks.GetValueOrDefault(new ActionID(ActionType.Spell, action.RowId).Raw, 0.6f);
                    var animLockStr = animLock == 0.6f ? "" : $", {animLock:f3}f";
                    var cg = action.CooldownGroup - 1;
                    if (cg == CommonDefinitions.GCDGroup)
                    {
                        sb.Append($"SupportedActions.GCD(AID.{aidEnum}{animLockStr});\n");
                    }
                    else
                    {
                        var cdgName = cd.CDGType?.GetEnumName(cg) ?? Utils.StringToIdentifier(action.Name);
                        var firstArgsStr = $"AID.{aidEnum}, CDGroup.{cdgName}, {action.Recast100ms * 0.1f:f1}f";
                        var charges = MaxChargesAtCap(action.RowId);
                        if (charges <= 1)
                            sb.Append($"SupportedActions.OGCD({firstArgsStr}{animLockStr});\n");
                        else
                            sb.Append($"SupportedActions.OGCDWithCharges({firstArgsStr}, {charges}{animLockStr});\n");
                    }
                }
                ImGui.SetClipboardText(sb.ToString());
            }

            if (ImGui.MenuItem("Generate MinLevel enum"))
            {
                var sb = new StringBuilder("public enum MinLevel : int\n{\n    // actions");
                foreach (var action in cd.Actions.Where(a => a.ClassJobLevel > 1))
                {
                    var aidEnum = cd.AIDType?.GetEnumName(action.RowId) ?? Utils.StringToIdentifier(action.Name);
                    sb.Append($"\n    {aidEnum} = {action.ClassJobLevel},");
                    if (action.UnlockLink != 0)
                        sb.Append($" // {UnlockLinkString(action.UnlockLink)}");
                }
                sb.Append("\n\n    // traits");
                foreach (var trait in cd.Traits.Where(t => t.Level > 1))
                {
                    sb.Append($"\n    {Utils.StringToIdentifier(trait.Name)} = {trait.Level},");
                    if (trait.Quest.Row != 0)
                        sb.Append($" // {UnlockLinkString(trait.Quest.Row)}");
                }
                sb.Append("\n}\n");
                ImGui.SetClipboardText(sb.ToString());
            }

            if (ImGui.MenuItem("Generate quest lock definitions"))
            {
                List<(int, uint)> questLocks = new();
                foreach (var action in cd.Actions.Where(a => a.UnlockLink != 0))
                    questLocks.Add((action.ClassJobLevel, action.UnlockLink));
                foreach (var trait in cd.Traits.Where(t => t.Quest.Row != 0))
                    questLocks.Add((trait.Level, trait.Quest.Row));
                questLocks.Sort();
                var sb = new StringBuilder("public static QuestLockEntry[] QuestsPerLevel = {");
                for (int i = 0; i < questLocks.Count; ++i)
                    if (i == 0 || questLocks[i - 1] != questLocks[i])
                        sb.Append($"\n    new({questLocks[i].Item1}, {questLocks[i].Item2}),");
                sb.Append("\n};\n");
                ImGui.SetClipboardText(sb.ToString());
            }
        }

        private void CDGroupsContextMenu(ClassData cd)
        {
            if (ImGui.MenuItem("Generate CDGroup enum"))
            {
                var sb = new StringBuilder("public enum CDGroup : int\n{");
                foreach (var (cg, actions) in cd.CooldownGroups)
                {
                    if (cg == CommonDefinitions.GCDGroup)
                        continue;

                    ushort? commonRecast = actions.First().Recast100ms;
                    int? commonMaxCharges = MaxChargesAtCap(actions.First().RowId);
                    foreach (var action in actions.Skip(1))
                    {
                        if (commonRecast != action.Recast100ms)
                            commonRecast = null;
                        if (commonMaxCharges != MaxChargesAtCap(action.RowId))
                            commonMaxCharges = null;
                    }

                    var cdgName = cd.CDGType?.GetEnumName(cg) ?? Utils.StringToIdentifier(actions.First().Name);
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
            return new(name, false, aidEnum == null ? 0xff0000ff : _seenActionLocks.ContainsKey(new ActionID(ActionType.Spell, action.RowId).Raw) ? 0xffffffff : 0xff00ffff);
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
            return _seenActionLocks.ContainsKey(id.Raw) ? $"{_seenActionLocks[id.Raw]:f3}s" : "???";
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

        private void OnCast(object? sender, (Actor actor, ActorCastEvent ev) args)
        {
            if (args.actor != _ws.Party.Player())
                return;
            _seenActionLocks[args.ev.Action.Raw] = args.ev.AnimationLockTime;
            foreach (var t in args.ev.Targets)
            {
                foreach (var eff in t.Effects.Where(eff => eff.Type is ActionEffectType.ApplyStatusEffectTarget or ActionEffectType.ApplyStatusEffectSource))
                {
                    var data = _seenStatuses.GetOrAdd(eff.Value);
                    data.Actions.Add(args.ev.Action.Raw);

                    bool onTarget = eff.Type == ActionEffectType.ApplyStatusEffectTarget && t.ID != args.actor.InstanceID && (eff.Param4 & 0x80) == 0;
                    if (onTarget)
                        data.OnTarget = true;
                    else
                        data.OnSource = true;
                }
            }
        }
    }
}
