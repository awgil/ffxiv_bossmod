namespace BossMod.ActionTweaks.CustomCombos;

// read-only combo that can be toggled on, or copied into user's combos to edit it
public sealed record class CustomComboPreset(string Key, string? Description, CustomCombo Combo);

public static class CustomComboPresets
{
    private static readonly Dictionary<Class, List<CustomComboPreset>> _presets = [];

    public static string KeyPrefix(Class job) => $"{job}:";

    public static List<CustomComboPreset> ForClass(Class job)
    {
        if (_presets.TryGetValue(job, out var list))
            return list;

        list = [];
        if (Autorotation(job) is { } auto)
            list.Add(auto);
        foreach (var chain in CustomComboData.ChainsForClass(job))
            list.Add(new($"{KeyPrefix(job)}native:{chain.Actions[^1]}", null, CustomComboData.BuildComboFromChain(chain, job)));
        list.AddRange(Handwritten().Where(p => p.Combo.Job == job));
        _presets[job] = list;
        return list;
    }

    // user combos checked first, then anything else enabled
    public static IEnumerable<CustomCombo> ActiveCombos(CustomComboConfig config, Class job)
    {
        foreach (var c in config.Combos)
            if (c.Enabled && (c.Job == Class.None || c.Job == job))
                yield return c;
        if (config.Presets.Count == 0)
            yield break;
        foreach (var p in ForClass(Class.None).Concat(ForClass(job)))
            if (config.Presets.Contains(p.Key))
                yield return p.Combo;
    }

    // replaces lowest level damaging gcd
    private static CustomComboPreset? Autorotation(Class job)
    {
        if (job == Class.None)
            return null;
        var trigger = CustomComboData.ActionsForClass(job).FirstOrDefault(a => IsDamageGCD(a.ID)).ID;
        if (trigger == 0)
            return null;
        return new($"{KeyPrefix(job)}auto", "Uses the autorotation preset's next oGCD when it can be woven, otherwise its next GCD", new()
        {
            Name = "Autorotation",
            Job = job,
            Trigger = trigger,
            Steps =
            [
                Step(CustomComboData.AutorotationNextOGCD, new CustomComboCondition() { Type = CustomComboConditionType.CanWeave, Value = 0.6f }),
                Step(CustomComboData.AutorotationNextGCD),
            ]
        });
    }

    private static bool IsDamageGCD(uint action)
        => Service.LuminaRow<Lumina.Excel.Sheets.Action>(action) is { } row && ActionDefinitions.Instance.SpellMainCDGroup(row) == ActionDefinitions.GCDGroup && row.CanTargetHostile && row.ActionCategory.RowId is 2 or 3;

    private static List<CustomComboPreset>? _handwritten;
    private static List<CustomComboPreset> Handwritten() => _handwritten ??=
    [
        Preset(Class.MCH, "overdrive", "Overdrive", "Replaces Rook Autoturret / Automaton Queen with Overdrive while it's out",
            (uint)MCH.AID.RookAutoturret, Step((uint)MCH.AID.RookOverdrive, PetPresent())),

        WithSequence(Preset(Class.DNC, "procs", "Cascade combo with procs", "Replaces Cascade with Cascade > Fountain, using Fountainfall and Reverse Cascade when they're available",
            (uint)DNC.AID.Cascade,
            StepAny((uint)DNC.AID.Fountainfall, Status((uint)DNC.SID.SilkenFlow), Status((uint)DNC.SID.FlourishingFlow)),
            StepAny((uint)DNC.AID.ReverseCascade, Status((uint)DNC.SID.SilkenSymmetry), Status((uint)DNC.SID.FlourishingSymmetry))),
            (uint)DNC.AID.Cascade, (uint)DNC.AID.Fountain),

        SwiftRaise(Class.WHM, (uint)WHM.AID.Raise),
        SwiftRaise(Class.SCH, (uint)SCH.AID.Resurrection),
        SwiftRaise(Class.AST, (uint)AST.AID.Ascend),
        SwiftRaise(Class.SGE, (uint)SGE.AID.Egeiro),
        SwiftRaise(Class.SMN, (uint)SMN.AID.Resurrection),
        SwiftRaise(Class.RDM, (uint)RDM.AID.Verraise, Not(Status((uint)RDM.SID.Dualcast))),
    ];

    private static CustomComboPreset SwiftRaise(Class job, uint raise, params CustomComboCondition[] extra)
        => Preset(job, "swiftraise", "Swiftcast raise", $"Replaces {CustomComboData.ActionName(raise)} with Swiftcast while it's ready",
            raise, Step((uint)ClassShared.AID.Swiftcast, [Usable((uint)ClassShared.AID.Swiftcast), .. extra]));

    private static CustomComboPreset Preset(Class job, string id, string name, string description, uint trigger, params CustomComboStep[] steps)
        => new($"{KeyPrefix(job)}{id}", description, new() { Name = name, Job = job, Trigger = trigger, Steps = [.. steps] });

    private static CustomComboPreset WithSequence(CustomComboPreset preset, params uint[] sequence)
    {
        preset.Combo.Sequence = [.. sequence];
        return preset;
    }

    private static CustomComboStep Step(uint action, params CustomComboCondition[] conditions) => new() { Action = action, Conditions = [.. conditions] };
    private static CustomComboStep StepAny(uint action, params CustomComboCondition[] conditions) => new() { Action = action, Match = CustomComboGroupMode.Any, Conditions = [.. conditions] };
    private static CustomComboCondition Usable(uint action) => new() { Type = CustomComboConditionType.ActionUsable, ID = action };
    private static CustomComboCondition Status(uint status) => new() { Type = CustomComboConditionType.StatusOnSelf, ID = status };
    private static CustomComboCondition PetPresent() => new() { Type = CustomComboConditionType.PetPresent };
    private static CustomComboCondition Not(CustomComboCondition c)
    {
        c.Negate = true;
        return c;
    }
}
