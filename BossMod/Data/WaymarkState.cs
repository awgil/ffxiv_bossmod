namespace BossMod;

public enum Waymark : byte
{
    A, B, C, D, N1, N2, N3, N4, Count
}

// waymark positions in world; part of the world state structure
public sealed class WaymarkState
{
    private BitMask _setMarkers;
    private readonly Vector3[] _positions = new Vector3[(int)Waymark.Count];

    public Vector3? this[int wm]
    {
        get => _setMarkers[wm] ? _positions[wm] : null;
        private set
        {
            _setMarkers[wm] = value != null;
            _positions[wm] = value ?? default;
        }
    }

    public Vector3? this[Waymark wm]
    {
        get => this[(int)wm];
        private set => this[(int)wm] = value;
    }

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        foreach (var i in _setMarkers.SetBits())
            yield return new OpWaymarkChange((Waymark)i, _positions[i]);
    }

    // implementation of operations
    public Event<OpWaymarkChange> Changed = new();
    public sealed record class OpWaymarkChange(Waymark ID, Vector3? Pos) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Waymarks[ID] = Pos;
            ws.Waymarks.Changed.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Pos != null)
                output.EmitFourCC("WAY+"u8).Emit((byte)ID).Emit(Pos.Value);
            else
                output.EmitFourCC("WAY-"u8).Emit((byte)ID);
        }
    }
}
