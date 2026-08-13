namespace BossMod;

// this class represents parts of a world state that are interesting to boss modules
// it does not know anything about dalamud, so it can be used for UI test - there is a separate utility that updates it based on game state every frame
// world state is supposed to be modified using "operations" - this provides opportunity to listen and react to state changes
[SkipLocalsInit]
public sealed class WorldState
{
    // state access
    public ulong QPF;
    public string GameVersion;
    public FrameState Frame;
    public ushort CurrentZone;
    public ushort CurrentCFCID;
    public bool IsPvPArea;
    public readonly Dictionary<string, string> RSVEntries = [];
    public readonly WaymarkState Waymarks = new();
    public readonly ActorState Actors = new();
    public readonly PartyState Party;
    public readonly ClientState Client = new();
    public readonly DeepDungeonState DeepDungeon = new();
    public readonly NetworkState Network = new();

    public DateTime CurrentTime => Frame.Timestamp;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTime FutureTime(double deltaSeconds) => Frame.Timestamp.AddSeconds(deltaSeconds);

    public WorldState(ulong qpf, string gameVersion)
    {
        QPF = qpf;
        GameVersion = gameVersion;
        Party = new(Actors);
    }

    // state modification
    public Event<Operation> Modified = new();
    public abstract class Operation
    {
        public DateTime Timestamp; // TODO: reconsider this field; it's very convenient for replays, but not really needed for operations themselves, and is filled late

        internal void Execute(WorldState ws)
        {
            Exec(ws);
            Timestamp = ws.CurrentTime;
        }

        protected abstract void Exec(WorldState ws);
        public abstract void Write(ReplayRecorder.Output output);
    }

    public void Execute(Operation op)
    {
        op.Execute(this);
        Modified.Fire(op);
    }

    // generate a set of operations that would turn default-constructed state into current state
    public List<Operation> CompareToInitial()
    {
        var waymarks = Waymarks.CompareToInitial();
        var actors = Actors.CompareToInitial();
        var party = Party.CompareToInitial();
        var client = Client.CompareToInitial();
        // var network = Network.CompareToInitial();
        var deepdungeon = DeepDungeon.CompareToInitial();
        List<Operation> ops = [with(RSVEntries.Count + waymarks.Count + actors.Count + party.Count + client.Count + deepdungeon.Count + 2)]; // todo add network back

        if (CurrentTime != default)
        {
            ops.Add(new OpFrameStart(Frame, default, Client.GaugePayload, Client.CameraAzimuth));
        }

        if (CurrentZone != default || CurrentCFCID != default)
        {
            ops.Add(new OpZoneChange(CurrentZone, CurrentCFCID));
        }

        if (IsPvPArea)
        {
            ops.Add(new OpPvPArea(true));
        }
        foreach (var (k, v) in RSVEntries)
        {
            ops.Add(new OpRSVData(k, v));
        }

        ops.AddRange(waymarks);
        ops.AddRange(actors);
        ops.AddRange(party);
        ops.AddRange(client);
        // ops.AddRange(network);
        ops.AddRange(deepdungeon);
        return ops;
    }
    // implementation of operations
    public Event<OpFrameStart> FrameStarted = new();
    public sealed class OpFrameStart(in FrameState frame, TimeSpan prevUpdateTime, ClientState.Gauge gaugePayload, Angle cameraAzimuth) : Operation
    {
        public readonly FrameState Frame = frame;
        public readonly TimeSpan PrevUpdateTime = prevUpdateTime;
        public readonly ClientState.Gauge GaugePayload = gaugePayload;
        public readonly Angle CameraAzimuth = cameraAzimuth;

        protected override void Exec(WorldState ws)
        {
            ws.Frame = Frame;
            ws.Client.CameraAzimuth = CameraAzimuth;
            ws.Client.GaugePayload = GaugePayload;
            ws.Client.Tick(Frame.Duration);
            ws.Actors.Tick(Frame);
            ws.FrameStarted.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("FRAM"u8)
            .Emit(PrevUpdateTime.TotalMilliseconds, "f3")
            .Emit()
            .Emit(GaugePayload.Low, "X16")
            .Emit(GaugePayload.High, "X16")
            .Emit(Frame.QPC)
            .Emit(Frame.Index)
            .Emit(Frame.DurationRaw)
            .Emit(Frame.Duration)
            .Emit(Frame.TickSpeedMultiplier)
            .Emit(CameraAzimuth);
    }

    public Event<OpUserMarker> UserMarkerAdded = new();
    public sealed class OpUserMarker(string text) : Operation
    {
        public readonly string Text = text;

        protected override void Exec(WorldState ws) => ws.UserMarkerAdded.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("UMRK"u8).Emit(Text);
    }

    public Event<OpRSVData> RSVDataReceived = new();
    public sealed class OpRSVData(string key, string value) : Operation
    {
        public readonly string Key = key;
        public readonly string Value = value;

        protected override void Exec(WorldState ws)
        {
            Service.LuminaRSV[Key] = Encoding.UTF8.GetBytes(Value); // TODO: reconsider...
            ws.RSVEntries[Key] = Value;
            ws.RSVDataReceived.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("RSV "u8).Emit(Key).Emit(Value);
    }

    public Event<OpZoneChange> CurrentZoneChanged = new();
    public sealed class OpZoneChange(ushort zone, ushort cfcid) : Operation
    {
        public readonly ushort Zone = zone;
        public readonly ushort CFCID = cfcid;

        protected override void Exec(WorldState ws)
        {
            ws.CurrentZone = Zone;
            ws.CurrentCFCID = CFCID;
            ws.CurrentZoneChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ZONE"u8).Emit(Zone).Emit(CFCID);
    }

    // global events
    public Event<OpDirectorUpdate> DirectorUpdate = new();
    public sealed class OpDirectorUpdate(uint directorID, uint updateID, uint param1, uint param2, uint param3, uint param4) : Operation
    {
        public readonly uint DirectorID = directorID;
        public readonly uint UpdateID = updateID;
        public readonly uint Param1 = param1;
        public readonly uint Param2 = param2;
        public readonly uint Param3 = param3;
        public readonly uint Param4 = param4;

        protected override void Exec(WorldState ws) => ws.DirectorUpdate.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("DIRU"u8).Emit(DirectorID, "X8").Emit(UpdateID, "X8").Emit(Param1, "X8").Emit(Param2, "X8").Emit(Param3, "X8").Emit(Param4, "X8");
    }

    public Event<OpMapEffect> MapEffect = new();
    public sealed class OpMapEffect(byte index, uint state) : Operation
    {
        public readonly byte Index = index;
        public readonly uint State = state;

        protected override void Exec(WorldState ws) => ws.MapEffect.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("ENVC"u8).Emit(Index, "X2").Emit(State, "X8");
    }

    public Event<OpLegacyMapEffect> LegacyMapEffect = new();
    public sealed class OpLegacyMapEffect(byte sequence, byte param, byte[] data) : Operation
    {
        public readonly byte Sequence = sequence;
        public readonly byte Param = param;
        public readonly byte[] Data = data;

        protected override void Exec(WorldState ws) => ws.LegacyMapEffect.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("LEME"u8).Emit(Sequence, "X2").Emit(Param, "X2").Emit(Data);
    }

    public Event<OpPvPArea> IsPvPAreaChanged = new();
    public sealed class OpPvPArea(bool value) : Operation
    {
        public bool Value = value;

        protected override void Exec(WorldState ws)
        {
            ws.IsPvPArea = Value;
            ws.IsPvPAreaChanged.Fire(this);
        }

        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC(Value ? "PVP+"u8 : "PVP-"u8);
    }

    public Event<OpSystemLogMessage> SystemLogMessage = new();
    public sealed class OpSystemLogMessage(uint messageId, int[] args) : Operation
    {
        public readonly uint MessageID = messageId;
        public readonly int[] Args = args;

        protected override void Exec(WorldState ws) => ws.SystemLogMessage.Fire(this);
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("SLOG"u8).Emit(MessageID);
            var len = Args.Length;
            output.Emit(len);
            for (var i = 0; i < len; ++i)
            {
                output.Emit(Args[i]);
            }
        }
    }
}
