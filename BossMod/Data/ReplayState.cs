using BossMod.Autorotation;
using System.Text.Json;

namespace BossMod;

// user interaction-related plugin state info useful for debugging replays
public sealed class ReplayState
{
    public readonly List<Preset> ActivePresets = [];
    public Preset.Modifier ModifierKeys = default;
    public PartyRolesConfig.Assignment[] RoleAssignments = Utils.MakeArray(PartyState.MaxPartySize, PartyRolesConfig.Assignment.Unassigned);
    public WDir MoveInput;
    public readonly List<SimpleEntry> ManualActions = [];

    public readonly record struct SimpleEntry(ActionID Action, ulong Target, float Priority, float CastTime, Vector3 TargetPos);

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
    public sealed record class OpRoleAssignmentsChange(PartyRolesConfig.Assignment[] Assignments) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            Assignments.CopyTo(ws.ReplayState.RoleAssignments);
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
}
