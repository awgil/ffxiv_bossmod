using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public struct ClientActionRequest
    {
        public ActionID Action;
        public ulong TargetID;
        public Vector3 TargetPos;
        public uint SourceSequence;
        public float InitialAnimationLock;
        public float InitialCastTimeElapsed;
        public float InitialCastTimeTotal;
        public float InitialRecastElapsed;
        public float InitialRecastTotal;
    }

    public struct ClientActionReject
    {
        public ActionID Action;
        public uint SourceSequence;
        public float RecastElapsed;
        public float RecastTotal;
        public uint LogMessageID;
    }

    // client-specific state and events (action requests, gauge, etc)
    // this is generally not available for non-player party members, but we can try to guess
    public class ClientState
    {
        public float? CountdownRemaining;
        public byte[] BozjaHolster = new byte[(int)BozjaHolsterID.Count]; // number of copies in holster per item

        public IEnumerable<WorldState.Operation> CompareToInitial()
        {
            if (CountdownRemaining != null)
                yield return new OpCountdownChange() { Value = CountdownRemaining };
            if (BozjaHolster.Any(count => count != 0))
                yield return new OpBozjaHolsterChange(BozjaHolster);
        }

        public void Tick(float dt)
        {
            if (CountdownRemaining != null)
                CountdownRemaining = CountdownRemaining.Value - dt;
        }

        // implementation of operations
        public event EventHandler<OpActionRequest>? ActionRequested;
        public class OpActionRequest : WorldState.Operation
        {
            public ClientActionRequest Request;

            protected override void Exec(WorldState ws)
            {
                ws.Client.ActionRequested?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLAR")
                .Emit(Request.Action)
                .EmitActor(Request.TargetID)
                .Emit(Request.TargetPos)
                .Emit(Request.SourceSequence)
                .Emit(Request.InitialAnimationLock, "f3")
                .EmitFloatPair(Request.InitialCastTimeElapsed, Request.InitialCastTimeTotal)
                .EmitFloatPair(Request.InitialRecastElapsed, Request.InitialRecastTotal);
        }

        public event EventHandler<OpActionReject>? ActionRejected;
        public class OpActionReject : WorldState.Operation
        {
            public ClientActionReject Value;

            protected override void Exec(WorldState ws)
            {
                ws.Client.ActionRejected?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output) => WriteTag(output, "CLRJ")
                .Emit(Value.Action)
                .Emit(Value.SourceSequence)
                .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
                .Emit(Value.LogMessageID);
        }

        public event EventHandler<OpCountdownChange>? CountdownChanged;
        public class OpCountdownChange : WorldState.Operation
        {
            public float? Value;

            protected override void Exec(WorldState ws)
            {
                ws.Client.CountdownRemaining = Value;
                ws.Client.CountdownChanged?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output)
            {
                if (Value != null)
                    WriteTag(output, "CDN+").Emit(Value.Value);
                else
                    WriteTag(output, "CDN-");
            }
        }

        public event EventHandler<OpBozjaHolsterChange>? BozjaHolsterChanged;
        public class OpBozjaHolsterChange : WorldState.Operation
        {
            public List<(BozjaHolsterID entry, byte count)> Contents = new();

            public OpBozjaHolsterChange(byte numEntries) => Contents.Capacity = numEntries;
            public OpBozjaHolsterChange(ReadOnlySpan<byte> entries)
            {
                for (int i = 0; i < entries.Length; ++i)
                    if (entries[i] != 0)
                        Contents.Add(((BozjaHolsterID)i, entries[i]));
            }

            protected override void Exec(WorldState ws)
            {
                Array.Fill(ws.Client.BozjaHolster, (byte)0);
                foreach (var e in Contents)
                    ws.Client.BozjaHolster[(int)e.entry] = e.count;
                ws.Client.BozjaHolsterChanged?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output)
            {
                WriteTag(output, "CLBH");
                output.Emit((byte)Contents.Count);
                foreach (var e in Contents)
                    output.Emit((byte)e.entry).Emit(e.count);
            }
        }
    }
}
