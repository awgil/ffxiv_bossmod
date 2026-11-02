using BossMod.Autorotation;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace BossMod;

// user interaction-related plugin state info useful for debugging replays
public sealed class ReplayState
{
    public readonly List<Preset> ActivePresets = [];
    public Preset.Modifier ModifierKeys;
    public PartyRolesConfig.Assignment[] RoleAssignments = Utils.MakeArray(PartyState.MaxPartySize, PartyRolesConfig.Assignment.Unassigned);
    public WDir MoveInput;
    public readonly List<SimpleEntry> ManualActions = [];
    public readonly List<EnemyPriority> Priorities = [];

    public readonly record struct SimpleEntry(ActionID Action, ulong Target, float Priority, float CastTime, Vector3 TargetPos);
    public readonly record struct EnemyPriority(ulong InstanceID, int Priority, List<(int Value, string Reason)> Trace)
    {
        public EnemyPriority(AIHints.Enemy enemy) : this(enemy.Actor.InstanceID, enemy.Priority, enemy.PrioHistory) { }
    }

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (ActivePresets.Count > 0)
            yield return new OpActivePresetsChange(ActivePresets);
        if (ModifierKeys != default)
            yield return new OpModifierKeysChange(ModifierKeys);
        if (RoleAssignments.Any(a => a != PartyRolesConfig.Assignment.Unassigned))
            yield return new OpRoleAssignmentsChange(RoleAssignments);
        if (MoveInput != default)
            yield return new OpMoveInputChange(MoveInput);
        if (ManualActions.Count > 0)
            yield return new OpManualActionsChange(ManualActions);
        if (Priorities.Count > 0)
            yield return new OpPrioritiesChange(Priorities);
    }

    public Event<OpActivePresetsChange> ActivePresetsChanged = new();
    public sealed record class OpActivePresetsChange(List<Preset> Presets) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.ReplayState.ActivePresets.Clear();
            ws.ReplayState.ActivePresets.AddRange(Presets);
            ws.ReplayState.ActivePresetsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            // TODO: support transient presets (jsonconverter correctly ignores them)
            output.EmitFourCC("RPRE"u8).Emit(JsonSerializer.SerializeToUtf8Bytes(Presets));
        }
    }

    public Event<OpModifierKeysChange> ModifierKeysChanged = new();
    public sealed record class OpModifierKeysChange(Preset.Modifier Mod) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.ReplayState.ModifierKeys = Mod;
            ws.ReplayState.ModifierKeysChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("RMOD"u8).Emit((byte)Mod);
        }
    }

    public Event<OpRoleAssignmentsChange> RoleAssignmentsChanged = new();
    public sealed record class OpRoleAssignmentsChange(IReadOnlyList<PartyRolesConfig.Assignment> Assignments) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            for (var i = 0; i < Assignments.Count; ++i)
                ws.ReplayState.RoleAssignments[i] = Assignments[i];
            ws.ReplayState.RoleAssignmentsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("RROL"u8);
            foreach (var a in Assignments)
                output.Emit((byte)a);
        }
    }

    public Event<OpMoveInputChange> MoveInputChanged = new();
    public sealed record class OpMoveInputChange(WDir Direction) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.ReplayState.MoveInput = Direction;
            ws.ReplayState.MoveInputChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("RMOV"u8).Emit(Direction.X).Emit(Direction.Z);
        }
    }

    public Event<OpManualActionsChange> ManualActionsChanged = new();
    public sealed record class OpManualActionsChange(List<SimpleEntry> Entries) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.ReplayState.ManualActions.Clear();
            ws.ReplayState.ManualActions.AddRange(Entries);
            ws.ReplayState.ManualActionsChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("RACT"u8).Emit(Entries.Count);
            foreach (var entry in Entries)
                output.Emit(entry.Action).EmitActor(entry.Target).Emit(entry.Priority).Emit(entry.CastTime).Emit(entry.TargetPos);
        }
    }

    public Event<OpPrioritiesChange> PrioritiesChanged = new();
    public sealed record class OpPrioritiesChange(List<EnemyPriority> Entries) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.ReplayState.Priorities.Clear();
            ws.ReplayState.Priorities.AddRange(Entries);
            ws.ReplayState.PrioritiesChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("RPRI"u8).Emit(Entries.Count);
            foreach (var entry in Entries)
            {
                output.EmitActor(entry.InstanceID).Emit(entry.Priority).Emit(entry.Trace.Count);
                foreach (var (val, reason) in entry.Trace)
                    output.Emit(val).Emit(reason);
            }
        }
    }

    public sealed class PriorityComparer : IEqualityComparer<EnemyPriority>
    {
        public bool Equals(EnemyPriority x, EnemyPriority y) => x.InstanceID == y.InstanceID && x.Priority == y.Priority;
        public int GetHashCode([DisallowNull] EnemyPriority obj) => (obj.InstanceID, obj.Priority).GetHashCode();
    }
}
