namespace BossMod;

public enum Waymark : byte
{
    A, B, C, D, N1, N2, N3, N4, Count
}

public enum Sign : byte
{
    Attack1,
    Attack2,
    Attack3,
    Attack4,
    Attack5,
    Bind1,
    Bind2,
    Bind3,
    Ignore1,
    Ignore2,
    Square,
    Circle,
    Cross,
    Triangle,
    Attack6,
    Attack7,
    Attack8,
    Count
}

public static class SignExtensions
{
    public static readonly uint[] SignIcons = [
        // numbers 1-5
        61301, 61302, 61303, 61304, 61305,
        // bind
        61311, 61312, 61313,
        // ignore
        61321, 61322,
        // shapes
        61331, 61332, 61333, 61334,
        // numbers 6-8
        61306, 61307, 61308,
        // extra element for count
        0
    ];

    public static uint IconId(this Sign s) => SignIcons[(int)s];
}

// waymark and sign positions in world; part of the world state structure
public sealed class WaymarkState
{
    private BitMask _setMarkers;
    private readonly Vector3[] _positions = new Vector3[(int)Waymark.Count];

    private BitMask _setSigns;
    private readonly ulong[] _targets = new ulong[(int)Sign.Count];

    public Vector3? GetFieldMark(int id) => this[(Waymark)id];
    public ulong GetSign(int id) => this[(Sign)id];

    public Vector3? this[Waymark wm]
    {
        get => _setMarkers[(int)wm] ? _positions[(int)wm] : null;
        private set
        {
            _setMarkers[(int)wm] = value != null;
            _positions[(int)wm] = value ?? default;
        }
    }

    public ulong this[Sign sgn]
    {
        get => _setSigns[(int)sgn] ? _targets[(int)sgn] : 0;
        private set
        {
            _setSigns[(int)sgn] = value is not (0 or 0xE0000000);
            _targets[(int)sgn] = value;
        }
    }

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        foreach (var i in _setMarkers.SetBits())
            yield return new OpWaymarkChange((Waymark)i, _positions[i]);
        foreach (var i in _setSigns.SetBits())
            yield return new OpSignChange((Sign)i, _targets[i]);
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

    public Event<OpSignChange> SignChanged = new();
    public sealed record class OpSignChange(Sign ID, ulong Target) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Waymarks[ID] = Target;
            ws.Waymarks.SignChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Target is not (0 or 0xE0000000))
                output.EmitFourCC("SGN+"u8).Emit((byte)ID).EmitActor(Target);
            else
                output.EmitFourCC("SGN-"u8).Emit((byte)ID);
        }
    }
}
