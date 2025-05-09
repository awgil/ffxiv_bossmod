namespace BossMod;

public sealed class NetworkState
{
    public readonly record struct ServerIPC(Network.ServerIPC.PacketID ID, ushort Opcode, uint Epoch, uint SourceServerActor, DateTime SendTimestamp, byte[] Payload)
    {
        public readonly byte[] Payload = Payload;
    };

    public readonly record struct IDScrambleFields(uint GameSessionRandom, uint ZoneRandom, uint Key0, uint Key1, uint Key2)
    {
        public uint Decode(ushort opcode, uint value)
        {
            var key = (new uint[] { Key0, Key1, Key2 })[opcode % 3];
            return value - (key - GameSessionRandom - ZoneRandom);
        }
    }

    public IDScrambleFields IDScramble;

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (IDScramble != default)
            yield return new OpIDScramble(IDScramble);
    }

    public Event<OpLegacyIDScramble> LegacyIDScrambleChanged = new();
    public sealed record class OpLegacyIDScramble(uint Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Network.LegacyIDScrambleChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("IPCI"u8).Emit(Value);
    }

    // implementation of operations
    public Event<OpIDScramble> IDScrambleChanged = new();
    public sealed record class OpIDScramble(IDScrambleFields Fields) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Network.IDScramble = Fields;
            ws.Network.IDScrambleChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("IPCX"u8)
            .Emit(Fields.GameSessionRandom)
            .Emit(Fields.ZoneRandom)
            .Emit(Fields.Key0)
            .Emit(Fields.Key1)
            .Emit(Fields.Key2);
    }

    public Event<OpServerIPC> ServerIPCReceived = new();
    public sealed record class OpServerIPC(ServerIPC Packet) : WorldState.Operation
    {
        protected override void Exec(WorldState ws) => ws.Network.ServerIPCReceived.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("IPCS"u8)
            .Emit((int)Packet.ID)
            .Emit(Packet.Opcode)
            .Emit(Packet.Epoch)
            .Emit(Packet.SourceServerActor, "X8")
            .Emit(Packet.SendTimestamp.Ticks)
            .Emit(Packet.Payload);
    }
}
