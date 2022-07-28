using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class DebugClassDefinitions : IDisposable
    {
        private WorldState _ws;
        private Class _curClass;
        private Type? _aidType;
        private Type? _cdgType;
        private List<Lumina.Excel.GeneratedSheets.Action> _actions = new();
        private SortedDictionary<int, List<Lumina.Excel.GeneratedSheets.Action>> _cooldownGroups = new();
        private Dictionary<uint, float> _seenActionLocks = new();
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

        public unsafe void Draw(Class c)
        {
            if (_curClass != c)
            {
                _curClass = c;
                Reinit(c);
            }

            foreach (var n in _tree.Node("Actions", contextMenu: ActionsContextMenu))
            {
                foreach (var action in _tree.Nodes(_actions, ActionNode))
                {
                    _tree.LeafNode($"Unlock: {UnlockString(action)}");
                    _tree.LeafNode($"Cast time: {CastTimeString(action)}");
                    _tree.LeafNode($"Cooldown: {CooldownString(action)}");
                    _tree.LeafNode($"Range: {action.Range} ({FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(action.RowId)})");
                    _tree.LeafNode($"Type: {CastTypeString(action.CastType)} {action.EffectRange}/{action.XAxisModifier}");
                    _tree.LeafNode($"Omen: {action.Omen.Value?.Path}/{action.Omen.Value?.PathAlly}");
                    _tree.LeafNode($"Targets: {TargetsString(action)}");
                    _tree.LeafNode($"Animation lock: {_seenActionLocks.GetValueOrDefault(new ActionID(ActionType.Spell, action.RowId).Raw):f1}s");
                }
            }

            foreach (var n in _tree.Node("Cooldown groups", contextMenu: CDGroupsContextMenu))
            {
                foreach (var (cg, actions) in _cooldownGroups)
                {
                    var cdgName = _cdgType?.GetEnumName(cg);
                    _tree.LeafNode($"{cg} ({cdgName}): {string.Join(", ", actions.Select(a => a.Name))}", cdgName != null || cg == CommonRotation.GCDGroup ? 0xffffffff : 0xff0000ff);
                }
            }
        }

        private void Reinit(Class c)
        {
            _aidType = Type.GetType($"BossMod.{c}.AID");
            _cdgType = Type.GetType($"BossMod.{c}.CDGroup");

            var cp = typeof(Lumina.Excel.GeneratedSheets.ClassJobCategory).GetProperty(c.ToString());
            Func<Lumina.Excel.GeneratedSheets.Action, bool> actionIsInteresting = a => !a.IsPvP && a.ClassJobLevel > 0 && (cp?.GetValue(a.ClassJobCategory.Value) as bool? ?? false);
            _actions = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.Where(actionIsInteresting).ToList() ?? new();
            _actions.Sort((l, r) => l.ClassJobLevel.CompareTo(r.ClassJobLevel));

            _cooldownGroups.Clear();
            foreach (var action in _actions)
            {
                var cg = action.CooldownGroup - 1;
                if (cg is >= 0 and < 80)
                    _cooldownGroups.GetOrAdd(cg).Add(action);
            }
        }

        private void ActionsContextMenu()
        {
            if (ImGui.MenuItem("Generate AID enum"))
            {
                var sb = new StringBuilder("public enum AID : uint\n{    None = 0,\n\n    // GCDs");
                foreach (var action in _actions.Where(a => a.CooldownGroup - 1 == CommonRotation.GCDGroup))
                    sb.Append($"\n    {ActionEnumString(action)}");
                sb.Append("\n\n    // oGCDs");
                foreach (var action in _actions.Where(a => a.CooldownGroup - 1 != CommonRotation.GCDGroup))
                    sb.Append($"\n    {ActionEnumString(action)}");
                sb.Append("\n}\n");
                ImGui.SetClipboardText(sb.ToString());
            }
        }

        private void CDGroupsContextMenu()
        {
            if (ImGui.MenuItem("Generate CDGroup enum"))
            {
                var sb = new StringBuilder("public enum CDGroup : int\n{");
                foreach (var (cg, actions) in _cooldownGroups)
                {
                    if (cg == CommonRotation.GCDGroup)
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

                    var cdgName = _cdgType?.GetEnumName(cg) ?? Utils.StringToIdentifier(actions.First().Name);
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

        private UITree.NodeProperties ActionNode(Lumina.Excel.GeneratedSheets.Action action)
        {
            var aidEnum = _aidType?.GetEnumName(action.RowId);
            var name = $"{action.RowId} '{action.Name}' ({aidEnum}): L{action.ClassJobLevel}";
            return new(name, false, aidEnum == null ? 0xff0000ff : _seenActionLocks.ContainsKey(new ActionID(ActionType.Spell, action.RowId).Raw) ? 0xffffffff : 0xff00ffff);
        }

        private string ActionEnumString(Lumina.Excel.GeneratedSheets.Action action)
        {
            var aidEnum = _aidType?.GetEnumName(action.RowId) ?? Utils.StringToIdentifier(action.Name);
            var sb = new StringBuilder($"{aidEnum} = {action.RowId}, // L{action.ClassJobLevel}, {CastTimeString(action)}");
            if (action.CooldownGroup - 1 != CommonRotation.GCDGroup)
                sb.Append($", {CooldownString(action)}");
            sb.Append($", range {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(action.RowId)}, {CastTypeString(action.CastType)} {action.EffectRange}/{action.XAxisModifier}, targets={TargetsString(action)}");
            return sb.ToString();
        }

        private string UnlockString(Lumina.Excel.GeneratedSheets.Action action)
        {
            var res = $"L{action.ClassJobLevel}";
            if (action.UnlockLink != 0)
                res += $" (unlocked by quest {action.UnlockLink} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.Quest>(action.UnlockLink)?.Name}')";
            return res;
        }

        private string CastTimeString(Lumina.Excel.GeneratedSheets.Action action)
        {
            return action.Cast100ms != 0 ? $"{action.Cast100ms * 0.1f:f1}s cast" : "instant";
        }

        private string CooldownString(Lumina.Excel.GeneratedSheets.Action action)
        {
            var cg = action.CooldownGroup - 1;
            var res = cg == CommonRotation.GCDGroup ? "GCD" : $"{action.Recast100ms * 0.1f:f1}s CD (group {cg})";
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
            _seenActionLocks[args.ev.Action.Raw] = args.ev.AnimationLockTime;
        }
    }
}
