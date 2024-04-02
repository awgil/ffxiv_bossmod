using ImGuiNET;

namespace BossMod.ReplayVisualization;

// a set of columns containing cast events (typically by non-players)
// by default contains a single column showing all actions from all sources, but extra columns can be added and per-column filters can be assigned
public class ColumnEnemiesCastEvents : Timeline.ColumnGroup
{
    private StateMachineTree _tree;
    private List<int> _phaseBranches;
    private Replay _replay;
    private Replay.Encounter _encounter;
    private ModuleRegistry.Info? _moduleInfo;
    private List<Replay.Action> _actions;
    private Dictionary<ActionID, List<(Replay.Participant source, BitMask cols)>> _filters = new(); // [action][sourceid]

    public ColumnEnemiesCastEvents(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc)
        : base(timeline)
    {
        //Name = "Enemy cast events";
        _tree = tree;
        _phaseBranches = phaseBranches;
        _replay = replay;
        _encounter = enc;
        _moduleInfo = ModuleRegistry.FindByOID(enc.OID);
        _actions = replay.EncounterActions(enc).Where(a => !(a.Source.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo)).ToList();
        foreach (var a in _actions)
        {
            var f = _filters.GetOrAdd(a.ID);
            if (!f.Any(e => e.source == a.Source))
                f.Add((a.Source, new(1)));
        }
        AddColumn();
        RebuildEvents();
    }

    public void DrawConfig(UITree tree)
    {
        if (ImGui.Button("Add new!"))
            AddColumn();

        bool needRebuild = false;
        foreach (var na in tree.Nodes(_filters, kv => new($"{kv.Key} ({_moduleInfo?.ActionIDType?.GetEnumName(kv.Key.ID)})")))
        {
            foreach (ref var v in na.Value.AsSpan())
            {
                needRebuild |= DrawConfigColumns(ref v.cols, $"{ReplayUtils.ParticipantString(v.source, v.source.WorldExistence.FirstOrDefault().Start)} ({_moduleInfo?.ObjectIDType?.GetEnumName(v.source.OID)})");
            }
        }

        if (needRebuild)
            RebuildEvents();
    }

    private void AddColumn()
    {
        Add(new ColumnGenericHistory(Timeline, _tree, _phaseBranches));
    }

    private bool DrawConfigColumns(ref BitMask mask, string name)
    {
        bool changed = false;
        for (int c = 0; c < Columns.Count; ++c)
        {
            bool set = mask[c];
            if (ImGui.Checkbox($"###{name}/{c}", ref set))
            {
                mask[c] = set;
                changed = true;
            }
            ImGui.SameLine();
        }
        ImGui.TextUnformatted(name);
        return changed;
    }

    private void RebuildEvents()
    {
        foreach (var c in Columns.Cast<ColumnGenericHistory>())
            c.Entries.Clear();

        foreach (var a in _actions)
        {
            var cols = _filters[a.ID].Find(c => c.source == a.Source).cols;
            if (cols.None())
                continue;

            var name = $"{a.ID} ({_moduleInfo?.ActionIDType?.GetEnumName(a.ID.ID)}) {ReplayUtils.ParticipantString(a.Source, a.Timestamp)} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
            var color = EventColor(a);
            foreach (var c in cols.SetBits())
            {
                var col = (ColumnGenericHistory)Columns[c];
                col.AddHistoryEntryDot(_encounter.Time.Start, a.Timestamp, name, color).AddActionTooltip(a);
            }
        }
    }

    private uint EventColor(Replay.Action action)
    {
        bool phys = false;
        bool magic = false;
        foreach (var t in action.Targets.Where(t => t.Target.Type == ActorType.Player))
        {
            foreach (var e in t.Effects.Where(e => e.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage))
            {
                switch (e.DamageType)
                {
                    case DamageType.Slashing:
                    case DamageType.Piercing:
                    case DamageType.Blunt:
                    case DamageType.Shot:
                        phys = true;
                        break;
                    case DamageType.Magic:
                        magic = true;
                        break;
                    default:
                        phys = magic = true; // TODO: reconsider
                        break;
                }
            }
        }
        return phys ? (magic ? 0xffffffff : 0xff0080ff) : (magic ? 0xffff00ff : 0x80808080);
    }
}
