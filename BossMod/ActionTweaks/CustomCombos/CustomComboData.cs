namespace BossMod.ActionTweaks.CustomCombos;

public static class CustomComboData
{
    // special action ids for autorotation suggestions
    public const uint AutorotationNextAction = 0xFFFF0001;
    public const uint AutorotationNextGCD = 0xFFFF0002;
    public const uint AutorotationNextOGCD = 0xFFFF0003;
    public static readonly uint[] AutorotationActions = [AutorotationNextAction, AutorotationNextGCD, AutorotationNextOGCD];
    public static bool IsAutorotation(uint action) => action is >= AutorotationNextAction and <= AutorotationNextOGCD;
    public static bool? AutorotationGCDFilter(uint action) => action switch
    {
        AutorotationNextGCD => true,
        AutorotationNextOGCD => false,
        _ => null
    };

    public readonly record struct ActionEntry(uint ID, string Name, int Level, uint Icon, string Upgrades);

    public readonly record struct ComboChain(List<uint> Actions); // base actions linked by Action.ActionCombo, starter to finisher

    private const int ReplaceTypeTrait = 3; // permanent trait upgrade (e.g. split shot -> heated split shot); others are temp
    private static readonly Dictionary<(Class, bool), List<ActionEntry>> _actionsPerClass = [];
    private static readonly Dictionary<(Class, bool), List<ComboChain>> _chainsPerClass = [];
    private static readonly Dictionary<(Class, bool), Dictionary<uint, uint>> _comboParentsPerClass = []; // combo action -> action that has to precede it
    private static Dictionary<uint, List<uint>>? _upgradesOfBase;

    private static Dictionary<uint, uint> UpgradeToBase => field ??= BuildUpgradeMap();

    // get the base action
    public static uint Canonical(uint action)
    {
        for (var i = 0; i < 8 && UpgradeToBase.TryGetValue(action, out var b); ++i)
            action = b;
        return action;
    }

    public static bool IsUpgrade(uint action) => UpgradeToBase.ContainsKey(action);

    // highest trait upgrade of a base action available at given level
    public static uint DisplayAction(uint baseAction, int level)
    {
        _upgradesOfBase ??= UpgradeToBase.Keys.GroupBy(Canonical).ToDictionary(g => g.Key, g => g.OrderBy(ActionLevel).ToList());
        var res = baseAction;
        if (level > 0 && _upgradesOfBase.TryGetValue(baseAction, out var upgrades))
            foreach (var u in upgrades)
                if (ActionLevel(u) <= level)
                    res = u;
        return res;
    }

    // base actions that start or continue a game combo
    public static bool IsComboAction(Class job, bool pvp, uint action)
    {
        var parents = ComboParents(job, pvp);
        action = Canonical(action);
        return parents.ContainsKey(action) || parents.ContainsValue(action);
    }

    private static Dictionary<uint, uint> ComboParents(Class job, bool pvp)
    {
        if (_comboParentsPerClass.TryGetValue((job, pvp), out var parents))
            return parents;
        var actions = ActionsForClass(job, pvp).Select(a => a.ID).ToHashSet();
        parents = [];
        foreach (var id in actions)
        {
            var prev = Canonical(Service.LuminaRow<Lumina.Excel.Sheets.Action>(id)?.ActionCombo.RowId ?? 0);
            if (prev != 0 && prev != id && actions.Contains(prev))
                parents[id] = prev;
        }
        _comboParentsPerClass[(job, pvp)] = parents;
        return parents;
    }

    public static List<ActionEntry> ActionsForClass(Class job, bool pvp = false)
    {
        if (_actionsPerClass.TryGetValue((job, pvp), out var list))
            return list;

        Dictionary<uint, List<string>> upgradeNames = [];
        foreach (var (upgrade, b) in UpgradeToBase)
            if (Service.LuminaRow<Lumina.Excel.Sheets.Action>(upgrade) is { } u)
                upgradeNames.GetOrAdd(Canonical(b)).Add(u.Name.ToString());

        list = [];
        foreach (var row in Service.LuminaSheet<Lumina.Excel.Sheets.Action>())
        {
            if (!row.IsPlayerAction || row.IsPvP != pvp || !pvp && row.ClassJobLevel == 0 || row.Name.IsEmpty || IsUpgrade(row.RowId))
                continue;
            var allowed = ActionDefinitions.Instance.SpellAllowedClasses(row);
            if (job != Class.None ? !allowed[(int)job] : allowed.Raw == 0)
                continue;
            var upgrades = upgradeNames.TryGetValue(row.RowId, out var names) ? string.Join(" / ", names) : "";
            list.Add(new(row.RowId, row.Name.ToString(), row.ClassJobLevel, row.Icon, upgrades));
        }
        list.Sort((a, b) => a.Level != b.Level ? a.Level.CompareTo(b.Level) : a.ID.CompareTo(b.ID));
        _actionsPerClass[(job, pvp)] = list;
        return list;
    }

    public static List<ComboChain> ChainsForClass(Class job, bool pvp = false)
    {
        if (_chainsPerClass.TryGetValue((job, pvp), out var chains))
            return chains;

        chains = [];
        if (job != Class.None)
        {
            var parent = ComboParents(job, pvp);
            var hasChildren = parent.Values.ToHashSet();
            foreach (var finisher in parent.Keys.Where(a => !hasChildren.Contains(a)))
            {
                List<uint> path = [finisher];
                for (var cur = finisher; parent.TryGetValue(cur, out var p) && !path.Contains(p) && path.Count < 10; cur = p)
                    path.Add(p);
                path.Reverse();
                chains.Add(new(path));
            }
            chains.Sort((a, b) => ActionLevel(a.Actions[0]) != ActionLevel(b.Actions[0]) ? ActionLevel(a.Actions[0]).CompareTo(ActionLevel(b.Actions[0])) : ActionLevel(a.Actions[^1]).CompareTo(ActionLevel(b.Actions[^1])));
        }
        _chainsPerClass[(job, pvp)] = chains;
        return chains;
    }

    // trigger is the starter, unless several chains share it (branching combos), then each branch uses its finisher
    public static CustomCombo BuildComboFromChain(ComboChain chain, Class job)
    {
        var actions = chain.Actions;
        var sharedStarter = ChainsForClass(job).Count(c => c.Actions[0] == actions[0]) > 1;
        return new()
        {
            Name = string.Join(" > ", actions.Select(ActionName)),
            Job = job,
            Trigger = sharedStarter ? actions[^1] : actions[0],
            Sequence = [.. actions],
        };
    }

    public static bool ContinuesCombo(uint prev, uint next)
        => Canonical(Service.LuminaRow<Lumina.Excel.Sheets.Action>(Canonical(next))?.ActionCombo.RowId ?? 0) == Canonical(prev);

    public static string ActionName(uint id) => id switch
    {
        AutorotationNextAction => "Autorotation: next action",
        AutorotationNextGCD => "Autorotation: next GCD",
        AutorotationNextOGCD => "Autorotation: next oGCD",
        _ => LuminaActionName(id)
    };
    private static string LuminaActionName(uint id) => Service.LuminaRow<Lumina.Excel.Sheets.Action>(id)?.Name.ToString() ?? $"#{id}";
    public static uint ActionIcon(uint id) => Service.LuminaRow<Lumina.Excel.Sheets.Action>(id)?.Icon ?? 0;
    public static int ActionLevel(uint id) => Service.LuminaRow<Lumina.Excel.Sheets.Action>(id)?.ClassJobLevel ?? 0;

    private static Dictionary<uint, uint> BuildUpgradeMap()
    {
        Dictionary<uint, uint> res = [];
        foreach (var rows in Service.LuminaGameData!.GetSubrowExcelSheet<Lumina.Excel.Sheets.ReplaceAction>()!)
        {
            foreach (var row in rows)
            {
                var from = row.Action.RowId;
                if (from == 0)
                    continue;
                ReadOnlySpan<sbyte> types = [row.Type1, row.Type2, row.Type3, row.Type4];
                for (var i = 0; i < row.ReplaceActions.Count && i < types.Length; ++i)
                {
                    var to = row.ReplaceActions[i].RowId;
                    if (types[i] == ReplaceTypeTrait && to != 0 && to != from)
                        res.TryAdd(to, from);
                }
            }
        }
        return res;
    }
}
