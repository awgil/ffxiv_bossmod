namespace BossMod;

public sealed class NetworkState
{
    public readonly record struct ServerIPC(Network.ServerIPC.PacketID ID, ushort Opcode, uint Epoch, uint SourceServerActor, DateTime SendTimestamp, byte[] Payload)
    {
        public readonly byte[] Payload = Payload;
    };

    public uint IDScramble;

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        if (IDScramble != 0)
            yield return new OpIDScramble(IDScramble);
    }

    // implementation of operations
    public Event<OpIDScramble> IDScrambleChanged = new();
    public sealed record class OpIDScramble(uint Value) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Network.IDScramble = Value;
            ws.Network.IDScrambleChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("IPCI"u8).Emit(Value);
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
