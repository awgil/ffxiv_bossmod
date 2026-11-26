using BossMod.Autorotation;

namespace BossMod.ActionTweaks.CustomCombos;

// read-only combo that can be toggled on, or copied into user's combos to edit it
public sealed record class CustomComboPreset(string Key, string? Description, CustomCombo Combo);

public static class CustomComboPresets
{
    private static readonly Dictionary<(Class, bool), List<CustomComboPreset>> _presets = [];

    public static string KeyPrefix(Class job, bool pvp) => pvp ? $"pvp:{job}:" : $"{job}:";

    public static List<CustomComboPreset> ForClass(Class job, bool pvp)
    {
        if (_presets.TryGetValue((job, pvp), out var list))
            return list;

        list = [];
        if (Autorotation(job, pvp) is { } auto)
            list.Add(auto);
        if (!pvp)
        {
            foreach (var chain in CustomComboData.ChainsForClass(job))
                list.Add(new($"{KeyPrefix(job, false)}native:{chain.Actions[^1]}", null, CustomComboData.BuildComboFromChain(chain, job)));
            list.AddRange(AllBranches(job));
            list.AddRange(Handwritten().Where(p => p.Combo.Job == job));
        }
        _presets[(job, pvp)] = list;
        return list;
    }

    public static IEnumerable<CustomCombo> ActiveCombos(CustomComboConfig config, Class job, bool pvp)
    {
        foreach (var c in config.Combos)
            if (c.Enabled && c.PvP == pvp && (c.Job == Class.None || c.Job == job))
                yield return c;
        if (config.Presets.Count == 0)
            yield break;
        foreach (var p in ForClass(Class.None, pvp).Concat(ForClass(job, pvp)))
            if (config.Presets.Contains(p.Key))
                yield return p.Combo;
    }

    // replaces lowest level damaging gcd
    private static CustomComboPreset? Autorotation(Class job, bool pvp)
    {
        if (job == Class.None || pvp && !RotationModuleRegistry.Modules.Values.Any(m => AutorotationSuggestions.IsRotation(m.Definition, job, true)))
            return null;
        var actions = CustomComboData.ActionsForClass(job, pvp).Where(a => IsDamageGCD(a.ID)).ToList();
        var trigger = actions.FirstOrDefault(a => CustomComboData.IsPvPRouteStart(a.ID), actions.FirstOrDefault()).ID;
        if (trigger == 0)
            return null;
        return new($"{KeyPrefix(job, pvp)}auto", "Uses the autorotation preset's next oGCD when it can be woven, otherwise its next GCD", new()
        {
            Name = "Autorotation",
            Job = job,
            PvP = pvp,
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

    private readonly record struct BranchRule(uint Action, CustomComboCondition? When);

    private static Dictionary<Class, BranchRule[]>? _branchRules;
    private static Dictionary<Class, BranchRule[]> BranchRules() => _branchRules ??= new()
    {
        // copying veynWAR and xan (rest) for these
        [Class.WAR] = [new((uint)WAR.AID.StormEye, AllOf(Not(Status((uint)WAR.SID.SurgingTempest, 32.5f)), AnyOf(Not(Status((uint)WAR.SID.SurgingTempest, 22.5f)), Not(Ready((uint)WAR.AID.InnerRelease, 12.5f)))))],
        [Class.NIN] = [new((uint)NIN.AID.ArmorCrush, AnyOf(GaugeIs("kazematoi", CustomComboCompare.Equal, 0), AllOf(GaugeIs("kazematoi", CustomComboCompare.Less, 4), Not(At(Positional.Rear)))))],
        [Class.SAM] =
        [
            new((uint)SAM.AID.Jinpu, Not(Status((uint)SAM.SID.Fugetsu, 5))),
            new((uint)SAM.AID.Shifu, Not(Status((uint)SAM.SID.Fuka, 5))),
            new((uint)SAM.AID.Yukikaze, GaugeIs("setsu", CustomComboCompare.Equal, 0)),
            new((uint)SAM.AID.Shifu, GaugeIs("ka", CustomComboCompare.Equal, 0)),
            new((uint)SAM.AID.Jinpu, GaugeIs("getsu", CustomComboCompare.Equal, 0)),
            new((uint)SAM.AID.Yukikaze, null),
            new((uint)SAM.AID.Oka, StatusLess((uint)SAM.SID.Fuka, (uint)SAM.SID.Fugetsu)),
        ],
        [Class.DRG] = [new((uint)DRG.AID.Disembowel, Not(Status((uint)DRG.SID.PowerSurge, 10)))],
        [Class.VPR] =
        [
            new((uint)VPR.AID.HuntersSting, StatusLess((uint)VPR.SID.HuntersInstinct, (uint)VPR.SID.Swiftscaled)),
            new((uint)VPR.AID.FlankstingStrike, Status((uint)VPR.SID.FlankstungVenom, 2.5f)),
            new((uint)VPR.AID.HindstingStrike, Status((uint)VPR.SID.HindstungVenom, 2.5f)),
        ],
    };

    private static IEnumerable<CustomComboPreset> AllBranches(Class job)
    {
        if (!BranchRules().TryGetValue(job, out var rules))
            yield break;

        foreach (var group in CustomComboData.ChainsForClass(job).GroupBy(c => c.Actions[0]))
        {
            Dictionary<uint, List<uint>> children = [];
            foreach (var chain in group)
            {
                for (var i = 1; i < chain.Actions.Count; ++i)
                {
                    var kids = children.GetOrAdd(chain.Actions[i - 1]);
                    if (!kids.Contains(chain.Actions[i]))
                        kids.Add(chain.Actions[i]);
                }
            }
            // only if the data still branches where the rules expect it to
            if (!children.Values.Any(kids => kids.Count > 1 && kids.Any(k => rules.Any(r => r.Action == k && r.When != null))))
                continue;

            uint defaultChild(List<uint> kids)
                => kids.FirstOrDefault(k => rules.Any(r => r.Action == k && r.When == null), kids.FirstOrDefault(k => !rules.Any(r => r.Action == k), kids[0]));

            List<uint> sequence = [group.Key];
            while (children.TryGetValue(sequence[^1], out var kids))
                sequence.Add(defaultChild(kids));

            List<CustomComboStep> steps = [];
            void visit(uint node, int depth, bool onSequence)
            {
                if (!children.TryGetValue(node, out var kids))
                {
                    // shorter branch continues like the default one, eg drg's wheeling thrust > drakesbane
                    if (!onSequence && depth + 1 < sequence.Count)
                        steps.Add(Step(sequence[depth + 1], LastCombo(node)));
                    return;
                }
                var def = defaultChild(kids);
                foreach (var r in rules)
                    if (r.When != null && kids.Contains(r.Action))
                        steps.Add(Step(r.Action, LastCombo(node), r.When));
                if (!onSequence)
                    steps.Add(Step(def, LastCombo(node)));
                foreach (var k in kids)
                    visit(k, depth + 1, onSequence && k == def);
            }
            visit(group.Key, 0, true);

            var name = CustomComboData.ActionName(group.Key);
            yield return new($"{KeyPrefix(job, false)}branches:{group.Key}", $"Replaces {name} with all branches of the combo", new()
            {
                Name = $"{name} full branched combo",
                Job = job,
                Trigger = group.Key,
                Steps = steps,
                Sequence = sequence,
            });
        }
    }

    private static List<CustomComboPreset>? _handwritten;
    private static List<CustomComboPreset> Handwritten() => _handwritten ??=
    [
        Preset(Class.MCH, "overdrive", "Overdrive", "Replaces Rook Autoturret / Automaton Queen with Overdrive while it's out",
            (uint)MCH.AID.RookAutoturret, Step((uint)MCH.AID.RookOverdrive, PetPresent())),
        Preset(Class.MCH, "hypercharge", "Heat Blast > Hypercharge", "Replaces Heat Blast / Blazing Shot with Hypercharge while not overheated",
            (uint)MCH.AID.HeatBlast, Step((uint)MCH.AID.Hypercharge, Not(Status((uint)MCH.SID.Overheated)))),
        Preset(Class.MCH, "hypercharge-aoe", "Auto Crossbow > Hypercharge", "Replaces Auto Crossbow with Hypercharge while not overheated",
            (uint)MCH.AID.AutoCrossbow, Step((uint)MCH.AID.Hypercharge, Not(Status((uint)MCH.SID.Overheated)))),
        Preset(Class.MCH, "tools", "Tools on one button", "Replaces Hot Shot / Air Anchor with whichever of Air Anchor, Drill and Chain Saw / Excavator is ready",
            (uint)MCH.AID.HotShot,
            Step((uint)MCH.AID.HotShot, Usable((uint)MCH.AID.HotShot)),
            Step((uint)MCH.AID.Drill, Usable((uint)MCH.AID.Drill)),
            Step((uint)MCH.AID.ChainSaw, Usable((uint)MCH.AID.ChainSaw))),
        Preset(Class.MCH, "gauss", "Gauss Round / Ricochet", "Replaces Gauss Round / Double Check with Ricochet / Checkmate when that has more charges",
            (uint)MCH.AID.GaussRound, Step((uint)MCH.AID.Ricochet, MoreCharges((uint)MCH.AID.Ricochet, (uint)MCH.AID.GaussRound))),
        Preset(Class.MCH, "ricochet", "Ricochet / Gauss Round", "Replaces Ricochet / Checkmate with Gauss Round / Double Check when that has more charges",
            (uint)MCH.AID.Ricochet, Step((uint)MCH.AID.GaussRound, MoreCharges((uint)MCH.AID.GaussRound, (uint)MCH.AID.Ricochet))),

        WithSequence(Preset(Class.DNC, "procs", "Cascade combo with procs", "Replaces Cascade with Cascade > Fountain, using Fountainfall and Reverse Cascade when they're available",
            (uint)DNC.AID.Cascade,
            StepAny((uint)DNC.AID.Fountainfall, Status((uint)DNC.SID.SilkenFlow), Status((uint)DNC.SID.FlourishingFlow)),
            StepAny((uint)DNC.AID.ReverseCascade, Status((uint)DNC.SID.SilkenSymmetry), Status((uint)DNC.SID.FlourishingSymmetry))),
            (uint)DNC.AID.Cascade, (uint)DNC.AID.Fountain),
        WithSequence(Preset(Class.DNC, "procs-aoe", "Windmill combo with procs", "Replaces Windmill with Windmill > Bladeshower, using Bloodshower and Rising Windmill when they're available",
            (uint)DNC.AID.Windmill,
            StepAny((uint)DNC.AID.Bloodshower, Status((uint)DNC.SID.SilkenFlow), Status((uint)DNC.SID.FlourishingFlow)),
            StepAny((uint)DNC.AID.RisingWindmill, Status((uint)DNC.SID.SilkenSymmetry), Status((uint)DNC.SID.FlourishingSymmetry))),
            (uint)DNC.AID.Windmill, (uint)DNC.AID.Bladeshower),
        FanDanceProcs("fandance", (uint)DNC.AID.FanDance),
        FanDanceProcs("fandance-aoe", (uint)DNC.AID.FanDanceII),

        Preset(Class.BRD, "ironjaws", "Iron Jaws > missing DoT", "Replaces Iron Jaws with Caustic Bite / Stormbite while your target is missing that DoT",
            (uint)BRD.AID.IronJaws,
            Step((uint)BRD.AID.VenomousBite, Not(AnyOf(TargetStatus((uint)BRD.SID.VenomousBite), TargetStatus((uint)BRD.SID.CausticBite)))),
            Step((uint)BRD.AID.Windbite, Not(AnyOf(TargetStatus((uint)BRD.SID.Windbite), TargetStatus((uint)BRD.SID.Stormbite))))),

        Preset(Class.DRG, "litany", "Lance Charge > Battle Litany", "Replaces Lance Charge with Battle Litany while Lance Charge is on cooldown",
            (uint)DRG.AID.LanceCharge, Step((uint)DRG.AID.BattleLitany, Not(Ready((uint)DRG.AID.LanceCharge)), Usable((uint)DRG.AID.BattleLitany))),

        Preset(Class.MNK, "brotherhood", "Riddle of Fire > Brotherhood", "Replaces Riddle of Fire with Brotherhood while Riddle of Fire is on cooldown",
            (uint)MNK.AID.RiddleOfFire, Step((uint)MNK.AID.Brotherhood, Not(Ready((uint)MNK.AID.RiddleOfFire)), Usable((uint)MNK.AID.Brotherhood))),

        Preset(Class.SCH, "aetherflow", "Energy Drain > Aetherflow", "Replaces Energy Drain with Aetherflow when you're out of Aetherflow stacks",
            (uint)SCH.AID.EnergyDrain, Step((uint)SCH.AID.Aetherflow, GaugeIs("aetherflow", CustomComboCompare.Equal, 0), Usable((uint)SCH.AID.Aetherflow))),

        Preset(Class.SMN, "energydrain", "Fester > Energy Drain", "Replaces Fester / Necrotize with Energy Drain when you're out of Aetherflow stacks",
            (uint)SMN.AID.Fester, Step((uint)SMN.AID.EnergyDrain, GaugeIs("aetherflow", CustomComboCompare.Equal, 0), Usable((uint)SMN.AID.EnergyDrain))),
        Preset(Class.SMN, "energysiphon", "Painflare > Energy Siphon", "Replaces Painflare with Energy Siphon when you're out of Aetherflow stacks",
            (uint)SMN.AID.Painflare, Step((uint)SMN.AID.EnergySiphon, GaugeIs("aetherflow", CustomComboCompare.Equal, 0), Usable((uint)SMN.AID.EnergySiphon))),
        Preset(Class.SMN, "carbuncle", "Summon Carbuncle reminder", "Replaces Ruin with Summon Carbuncle when you have no pet out",
            (uint)SMN.AID.Ruin1, Step((uint)SMN.AID.SummonCarbuncle, Not(PetPresent()), Usable((uint)SMN.AID.SummonCarbuncle))),

        Rhizomata("druochole", (uint)SGE.AID.Druochole),
        Rhizomata("ixochole", (uint)SGE.AID.Ixochole),
        Rhizomata("kerachole", (uint)SGE.AID.Kerachole),
        Rhizomata("taurochole", (uint)SGE.AID.Taurochole),

        SwiftRaise(Class.WHM, (uint)WHM.AID.Raise),
        SwiftRaise(Class.SCH, (uint)SCH.AID.Resurrection),
        SwiftRaise(Class.AST, (uint)AST.AID.Ascend),
        SwiftRaise(Class.SGE, (uint)SGE.AID.Egeiro),
        SwiftRaise(Class.SMN, (uint)SMN.AID.Resurrection),
        SwiftRaise(Class.RDM, (uint)RDM.AID.Verraise, Not(Status((uint)RDM.SID.Dualcast))),

        Interrupt(Class.PLD, (uint)ClassShared.AID.LowBlow, (uint)ClassShared.AID.Interject),
        Interrupt(Class.WAR, (uint)ClassShared.AID.LowBlow, (uint)ClassShared.AID.Interject),
        Interrupt(Class.DRK, (uint)ClassShared.AID.LowBlow, (uint)ClassShared.AID.Interject),
        Interrupt(Class.GNB, (uint)ClassShared.AID.LowBlow, (uint)ClassShared.AID.Interject),
        Interrupt(Class.BRD, (uint)ClassShared.AID.FootGraze, (uint)ClassShared.AID.HeadGraze),
        Interrupt(Class.MCH, (uint)ClassShared.AID.FootGraze, (uint)ClassShared.AID.HeadGraze),
        Interrupt(Class.DNC, (uint)ClassShared.AID.FootGraze, (uint)ClassShared.AID.HeadGraze),
    ];

    private static CustomComboPreset SwiftRaise(Class job, uint raise, params CustomComboCondition[] extra)
        => Preset(job, "swiftraise", "Swiftcast raise", $"Replaces {CustomComboData.ActionName(raise)} with Swiftcast while it's ready",
            raise, Step((uint)ClassShared.AID.Swiftcast, [Usable((uint)ClassShared.AID.Swiftcast), .. extra]));

    private static CustomComboPreset Interrupt(Class job, uint trigger, uint interrupt)
        => Preset(job, "interrupt", $"{CustomComboData.ActionName(trigger)} > {CustomComboData.ActionName(interrupt)}", $"Replaces {CustomComboData.ActionName(trigger)} with {CustomComboData.ActionName(interrupt)} when your target's cast can be interrupted",
            trigger, Step(interrupt, new CustomComboCondition() { Type = CustomComboConditionType.TargetInterruptible }, Usable(interrupt)));

    private static CustomComboPreset FanDanceProcs(string id, uint trigger)
        => Preset(Class.DNC, id, $"{CustomComboData.ActionName(trigger)} procs", $"Replaces {CustomComboData.ActionName(trigger)} with Fan Dance III / IV when they're available",
            trigger, Step((uint)DNC.AID.FanDanceIII, Status((uint)DNC.SID.ThreefoldFanDance)), Step((uint)DNC.AID.FanDanceIV, Status((uint)DNC.SID.FourfoldFanDance)));

    private static CustomComboPreset Rhizomata(string id, uint trigger)
        => Preset(Class.SGE, $"rhizomata-{id}", $"{CustomComboData.ActionName(trigger)} > Rhizomata", $"Replaces {CustomComboData.ActionName(trigger)} with Rhizomata when Addersgall is empty",
            trigger, Step((uint)SGE.AID.Rhizomata, GaugeIs("addersgall", CustomComboCompare.Equal, 0), Usable((uint)SGE.AID.Rhizomata)));

    private static CustomComboPreset Preset(Class job, string id, string name, string description, uint trigger, params CustomComboStep[] steps)
        => new($"{KeyPrefix(job, false)}{id}", description, new() { Name = name, Job = job, Trigger = trigger, Steps = [.. steps] });

    private static CustomComboPreset WithSequence(CustomComboPreset preset, params uint[] sequence)
    {
        preset.Combo.Sequence = [.. sequence];
        return preset;
    }

    private static CustomComboStep Step(uint action, params CustomComboCondition[] conditions) => new() { Action = action, Conditions = [.. conditions] };
    private static CustomComboStep StepAny(uint action, params CustomComboCondition[] conditions) => new() { Action = action, Match = CustomComboGroupMode.Any, Conditions = [.. conditions] };
    private static CustomComboCondition Usable(uint action) => new() { Type = CustomComboConditionType.ActionUsable, ID = action };
    private static CustomComboCondition Ready(uint action, float within = 0) => new() { Type = CustomComboConditionType.ActionReady, ID = action, Value = within };
    private static CustomComboCondition StatusLess(uint status, uint other) => new() { Type = CustomComboConditionType.StatusComparedTo, ID = status, OtherID = other, Compare = CustomComboCompare.Less };
    private static CustomComboCondition At(Positional positional) => new() { Type = CustomComboConditionType.Positional, Positional = positional };
    private static CustomComboCondition AllOf(params CustomComboCondition[] children) => new() { Group = CustomComboGroupMode.All, Children = [.. children] };
    private static CustomComboCondition MoreCharges(uint action, uint other) => new() { Type = CustomComboConditionType.ChargesComparedTo, ID = action, OtherID = other, Compare = CustomComboCompare.Greater };
    private static CustomComboCondition LastCombo(uint action) => new() { Type = CustomComboConditionType.LastComboAction, ID = action };
    private static CustomComboCondition Status(uint status, float moreThan = 0) => new() { Type = CustomComboConditionType.StatusOnSelf, ID = status, Value = moreThan };
    private static CustomComboCondition TargetStatus(uint status) => new() { Type = CustomComboConditionType.StatusOnTarget, ID = status };
    private static CustomComboCondition GaugeIs(string field, CustomComboCompare op, float value) => new() { Type = CustomComboConditionType.Gauge, Field = field, Compare = op, Value = value };
    private static CustomComboCondition PetPresent() => new() { Type = CustomComboConditionType.PetPresent };
    private static CustomComboCondition AnyOf(params CustomComboCondition[] children) => new() { Group = CustomComboGroupMode.Any, Children = [.. children] };
    private static CustomComboCondition Not(CustomComboCondition c)
    {
        c.Negate = true;
        return c;
    }
}
